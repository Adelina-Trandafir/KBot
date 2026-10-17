Option Strict On
Imports System.Collections.Generic

' Slice 00EF-18 -- what the received-invoice routes of the server answer (PYTHON/routes/efactura/primite_routes.py, slice 00EF-17).
' Plain data, no logic. Amounts are as the supplier wrote them (a credit note may be negative or carry Semn = -1).

''' <summary>A reception a received invoice is linked to, as the list of invoices carries it (slice 00EF-42).</summary>
Public NotInheritable Class EFacturaPrimitaReceptie

    ''' <summary>The date of the reception; Nothing while it waits for the download that brings it into K-BOT.</summary>
    Public Property DataR As Date?

    ''' <summary>The description written on the reception (without the K-BOT marker).</summary>
    Public Property Descriere As String = String.Empty

    ''' <summary>What the links of THIS invoice allocate on the reception (all classifications added), with VAT, with the sign of the invoice.</summary>
    Public Property Suma As Decimal

    ''' <summary>The reception is not known to K-BOT yet (the download that brings it has not run).</summary>
    Public Property AsteaptaDescarcarea As Boolean

End Class

''' <summary>A received e-invoice (<c>EF_Primite</c>): the header, as listed and as detailed.</summary>
Public NotInheritable Class EFacturaPrimita

    Public Property IdPrimita As Integer

    ''' <summary>ANAF's request number of the message (the supplier's upload number); the key of the message.</summary>
    Public Property IdSol As String = String.Empty

    Public Property NrFact As String = String.Empty
    Public Property DataFact As Date?
    Public Property DataScad As Date?

    ''' <summary>The VAT rate of the biggest taxable amount (a summary; the rates are in <see cref="EFacturaPrimitaDetaliu.Cote"/>).</summary>
    Public Property CotaTva As Decimal

    Public Property Tva As Decimal

    ''' <summary>The taxable amount.</summary>
    Public Property Valoare As Decimal

    ''' <summary>The payable amount.</summary>
    Public Property Total As Decimal

    ''' <summary>The supplier's tax code as written (the «RO» kept).</summary>
    Public Property Cui As String = String.Empty

    ''' <summary>The tax code without «RO», spaces and dots: what the DDF partners are compared with.</summary>
    Public Property CuiNormalizat As String = String.Empty

    Public Property Furnizor As String = String.Empty
    Public Property Adresa As String = String.Empty

    ''' <summary>Name of the first file embedded in the XML; empty when there is none.</summary>
    Public Property Atasament As String = String.Empty

    ''' <summary>«FC» invoice, «NC» credit note.</summary>
    Public Property Tip As String = "FC"

    Public Property Semn As Integer = 1
    Public Property Ref As String = String.Empty
    Public Property IdPrimitaRef As Integer?

    ''' <summary>The message was never opened.</summary>
    Public Property Nou As Boolean

    ''' <summary>How many booked operations (AVACONT) the invoice has; 0 = none (also when the server has no such table yet).</summary>
    Public Property NrOperatii As Integer

    ''' <summary>In a list asked for a DDF: «auto» (the supplier is a partner of the DDF) or «manual» (the operator linked it); else empty.</summary>
    Public Property Legatura As String = String.Empty

    ''' <summary>The receptions the invoice is linked to (slice 00EF-42), one item per reception; empty when it has none (or the list comes from a server without them).</summary>
    Public ReadOnly Property Receptii As New List(Of EFacturaPrimitaReceptie)()

    Public ReadOnly Property Eticheta As String
        Get
            Return $"{NrFact}"
        End Get
    End Property

End Class

Public NotInheritable Class EFacturaPrimitaLinie
    Public Property NrLinie As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Explicatie As String = String.Empty

    ''' <summary>
    ''' The CPV code of the line (<c>ItemClassificationCode</c> listID «STI»). <c>Nothing</c> = the line was stored before the
    ''' column existed (not read); empty = the invoice was read and the line has no such code.
    ''' </summary>
    Public Property Cpv As String

    ''' <summary>True when the invoice was read and this line carries no CPV code.</summary>
    Public ReadOnly Property CpvLipsa As Boolean
        Get
            Return Cpv IsNot Nothing AndAlso Cpv.Length = 0
        End Get
    End Property

    Public Property Unit As String = String.Empty
    Public Property Cant As Decimal
    Public Property Pret As Decimal
    Public Property Valoare As Decimal
End Class

''' <summary>One VAT rate of an invoice (one <c>TaxSubtotal</c> of the XML).</summary>
Public NotInheritable Class EFacturaPrimitaCota
    Public Property Categorie As String = String.Empty
    Public Property CotaTva As Decimal
    Public Property Baza As Decimal
    Public Property Tva As Decimal
End Class

Public NotInheritable Class EFacturaPrimitaMesaj
    Public Property IdMsg As Integer
    Public Property IdMesajAnaf As String = String.Empty
    Public Property Mesaj As String = String.Empty
    Public Property DataMesaj As Date?
End Class

''' <summary>A file embedded in the XML of the invoice.</summary>
Public NotInheritable Class EFacturaPrimitaAtasament
    ''' <summary>Position among the embedded files (0-based): what the server's file route takes.</summary>
    Public Property Index As Integer

    Public Property Nume As String = String.Empty
    Public Property Mime As String = String.Empty
    Public Property Octeti As Long
End Class

''' <summary>The buyer of a received invoice, as the supplier wrote it in the XML.</summary>
Public NotInheritable Class EFacturaPrimitaParte
    Public Property Nume As String = String.Empty
    Public Property Cui As String = String.Empty
    Public Property Adresa As String = String.Empty
End Class

''' <summary>An account the supplier asks to be paid into (the means code and the IBAN).</summary>
Public NotInheritable Class EFacturaPrimitaPlata
    Public Property Cod As String = String.Empty
    Public Property Cont As String = String.Empty
End Class

''' <summary>A DDF an invoice is linked to.</summary>
Public NotInheritable Class EFacturaPrimitaDdf
    Public Property IdDdf As Integer
    Public Property CodAngajament As String = String.Empty
End Class

''' <summary>A received invoice with everything the views show.</summary>
Public NotInheritable Class EFacturaPrimitaDetaliu
    Public Property Factura As New EFacturaPrimita()
    Public ReadOnly Property Linii As New List(Of EFacturaPrimitaLinie)()
    Public ReadOnly Property Cote As New List(Of EFacturaPrimitaCota)()
    Public ReadOnly Property Note As New List(Of String)()
    Public ReadOnly Property Mesaje As New List(Of EFacturaPrimitaMesaj)()
    Public ReadOnly Property Atasamente As New List(Of EFacturaPrimitaAtasament)()
    Public Property Cumparator As New EFacturaPrimitaParte()
    Public ReadOnly Property Plati As New List(Of EFacturaPrimitaPlata)()
    Public ReadOnly Property LegaturiAuto As New List(Of EFacturaPrimitaDdf)()
    Public ReadOnly Property LegaturiManuale As New List(Of EFacturaPrimitaDdf)()
End Class

''' <summary>What one call of the synchronisation did.</summary>
Public NotInheritable Class EFacturaSincronizare
    Public Property Cui As String = String.Empty
    Public Property Zile As Integer

    ''' <summary>Received messages ANAF lists for the period.</summary>
    Public Property Gasite As Integer

    Public Property Adaugate As Integer

    ''' <summary>Messages already in the database.</summary>
    Public Property Sarite As Integer

    ''' <summary>New messages this call left for the next one (it takes a batch at a time).</summary>
    Public Property Ramase As Integer

    Public ReadOnly Property Erori As New List(Of String)()
End Class

''' <summary>A DDF the operator can pick to link a received invoice to (slice 00EF-20).</summary>
Public NotInheritable Class EFacturaDdfAlegere
    Public Property IdDdf As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property Obiect As String = String.Empty
    Public Property Partener As String = String.Empty
    Public Property CodFiscal As String = String.Empty
End Class
