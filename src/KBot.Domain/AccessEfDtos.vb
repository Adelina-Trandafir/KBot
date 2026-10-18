Option Strict On
Imports System.Collections.Generic

' Slice 00EF-43 -- the received e-invoices as they travel between the Access e-invoice store (ef_<year>.accdb: tables EF, EFT, EFS, EFT_C, EFT_M, EFT_O)
' and the K-BOT server (PYTHON/routes/efactura/primite_access.py). Plain data, no logic. An invoice is identified by its NATURAL key (the message's
' IdSol + NrFact + CUI), never by an Access number: Access and K-BOT both number their rows and the numbers collide.

''' <summary>A line of a received invoice (Access <c>EFS</c>, K-BOT <c>EF_PrimiteLinii</c>).</summary>
Public NotInheritable Class AccessEfLine
    ''' <summary>The line number the supplier wrote (Access <c>EFS.ID</c>).</summary>
    Public Property NrLinie As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Explicatie As String = String.Empty
    Public Property Unit As String = String.Empty
    Public Property Cant As Decimal
    Public Property Pret As Decimal
    Public Property Valoare As Decimal
End Class

''' <summary>A note on a received invoice (Access <c>EFT_C</c>, K-BOT <c>EF_PrimiteNote</c>).</summary>
Public NotInheritable Class AccessEfNote
    Public Property Nota As String = String.Empty
    Public Property Added As Date?
End Class

''' <summary>A message exchanged with ANAF about an invoice (Access <c>EFT_M</c>, K-BOT <c>EF_PrimiteMesaje</c>).</summary>
Public NotInheritable Class AccessEfInvoiceMessage
    Public Property IdSol As String = String.Empty
    Public Property IdMesajAnaf As String = String.Empty
    Public Property Mesaj As String = String.Empty
    Public Property DataMesaj As Date?
    Public Property Added As Date?
End Class

''' <summary>The link of an invoice to an accounting operation (Access <c>EFT_O</c>, K-BOT <c>EF_PrimiteOperatii</c>). Preluare only.</summary>
Public NotInheritable Class AccessEfOperation
    ''' <summary>Access <c>EFT_O.IDEFTO</c>: only Access creates it, so it identifies the operation.</summary>
    Public Property IdOperatiePrimita As Integer
    Public Property IdOperatie As Integer
    ''' <summary>The Access classification id; the server resolves it on <see cref="IdUnitate"/>.</summary>
    Public Property IdClsfAcc As Integer
    Public Property IdUnitate As Integer
    Public Property Sursa As String = String.Empty
    Public Property Suma As Decimal
    Public Property Added As Date?
End Class

''' <summary>A received invoice (Access <c>EFT</c>, K-BOT <c>EF_Primite</c>) with everything under it.</summary>
Public NotInheritable Class AccessEfInvoice
    ''' <summary>Access <c>EFT.IDEFT</c>; used on the PC only, never sent.</summary>
    Public Property AccessId As Integer
    Public Property NrFact As String = String.Empty
    Public Property DataFact As Date?
    Public Property DataScad As Date?
    Public Property CotaTva As Decimal
    Public Property Tva As Decimal
    Public Property Valoare As Decimal
    Public Property Total As Decimal
    Public Property Cui As String = String.Empty
    Public Property DenumireP As String = String.Empty
    Public Property Adresa As String = String.Empty
    Public Property Atasament As String = String.Empty
    ''' <summary>«FC» invoice, «NC» credit note.</summary>
    Public Property Tip As String = "FC"
    ''' <summary>-1 for a credit note that counts against the account (Access <c>EFT.NC</c>), else 1.</summary>
    Public Property Semn As Integer = 1
    Public Property Ref As String = String.Empty
    ''' <summary>The invoice a credit note refers to, by natural key (Access <c>IDEFT_REF</c> told as message + number); empty when none.</summary>
    Public Property RefIdSol As String = String.Empty
    Public Property RefNrFact As String = String.Empty
    Public Property Added As Date?
    Public ReadOnly Property Lines As New List(Of AccessEfLine)()
    Public ReadOnly Property Notes As New List(Of AccessEfNote)()
    Public ReadOnly Property Messages As New List(Of AccessEfInvoiceMessage)()
    Public ReadOnly Property Operations As New List(Of AccessEfOperation)()
End Class

''' <summary>A received message (Access <c>EF</c>, K-BOT <c>EF_Mesaje</c>): the ANAF archive and the invoice(s) in it.</summary>
Public NotInheritable Class AccessEfMessage
    ''' <summary>ANAF's request number: the key of the message on both sides.</summary>
    Public Property IdSol As String = String.Empty
    Public Property IdIncarcare As String = String.Empty
    Public Property CifEmitent As String = String.Empty
    Public Property DataMesaj As Date?
    Public Property CuiUnitate As String = String.Empty
    Public Property Nou As Boolean
    Public Property NumeFisier As String = String.Empty
    ''' <summary>The ANAF archive (the zip with the invoice XML and the seal); Nothing when none is kept.</summary>
    Public Property Xml As Byte()
    Public Property Added As Date?
    Public ReadOnly Property Invoices As New List(Of AccessEfInvoice)()
End Class

''' <summary>What a write into the Access store did.</summary>
Public NotInheritable Class AccessEfWriteResult
    Public Property MessagesAdded As Integer
    Public Property MessagesSkipped As Integer
    Public Property InvoicesAdded As Integer
    Public Property LinesAdded As Integer
    Public Property NotesAdded As Integer
    Public Property InvoiceMessagesAdded As Integer
    ''' <summary>Text shortened to Access's 255 characters (Text columns), counted so the operator is told.</summary>
    Public Property TextsShortened As Integer
End Class

''' <summary>A credit note's reference, told by natural key, for the step that follows the last batch of a preluare.</summary>
Public NotInheritable Class AccessEfReference
    Public Property IdSol As String = String.Empty
    Public Property NrFact As String = String.Empty
    Public Property RefIdSol As String = String.Empty
    Public Property RefNrFact As String = String.Empty
End Class

''' <summary>What the server did with one batch of a preluare.</summary>
Public NotInheritable Class EFacturaAccessPreluare
    Public Property MesajeNoi As Integer
    Public Property MesajeSarite As Integer
    Public Property FacturiNoi As Integer
    Public Property FacturiSarite As Integer
    Public Property Linii As Integer
    Public Property Note As Integer
    Public Property MesajeFactura As Integer
    Public Property Operatii As Integer
    Public Property OperatiiSarite As Integer
    ''' <summary>The server's ids of the invoices this batch ADDED.</summary>
    Public ReadOnly Property IdPrimite As New List(Of Integer)()
    Public ReadOnly Property Probleme As New List(Of String)()
    Public ReadOnly Property Erori As New List(Of String)()
End Class

''' <summary>What the server did after the last batch of a preluare.</summary>
Public NotInheritable Class EFacturaAccessFinal
    Public Property ReferinteSetate As Integer
    Public Property Citite As Integer
    Public Property Legate As Integer
    Public Property FaraFurnizor As Integer
    Public Property FaraReceptie As Integer
    Public Property Nesigure As Integer
    Public Property MaiMulteClasificatii As Integer
    Public Property Conflicte As Integer
    ''' <summary>Why the automatic link was skipped (a table is missing); empty when it ran.</summary>
    Public Property Omisa As String = String.Empty
End Class

''' <summary>A batch of messages the server has and the Access file does not.</summary>
Public NotInheritable Class EFacturaAccessExport
    Public ReadOnly Property Mesaje As New List(Of AccessEfMessage)()
    ''' <summary>Messages still missing from the file after this batch.</summary>
    Public Property Ramase As Integer
    ''' <summary>All the messages of the year the file did not have when the first batch was asked.</summary>
    Public Property TotalLipsa As Integer
End Class
