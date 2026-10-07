Option Strict On
Imports System.Collections.Generic

' Slice 00EF-08 -- what the issued-invoice routes of the server answer and take (PYTHON/routes/efactura/
' factura_routes.py, slice 00EF-06). Plain data, no logic. The names follow the columns of the unit database
' (ASCII); what the server computes (state, total, what the operator may do) is carried as read-only facts so the
' window never repeats the rules.

''' <summary>The unit as the issuer of the invoices (<c>AVACONT_COMUN.Unitati_Detalii</c>, one row per unit).</summary>
Public NotInheritable Class EFacturaFurnizor

    Public Property Denumire As String = String.Empty

    ''' <summary>As sent to ANAF: digits, or «RO» + digits when the unit pays VAT.</summary>
    Public Property CodFiscal As String = String.Empty

    Public Property Adresa As String = String.Empty
    Public Property Orasul As String = String.Empty

    ''' <summary>County code the XML puts after «RO-» (for example PH).</summary>
    Public Property Judetul As String = String.Empty

    Public Property Mail As String = String.Empty
    Public Property Telefon As String = String.Empty

    ''' <summary>Goes in front of the number of every invoice of the unit.</summary>
    Public Property SerieFactura As String = String.Empty

    ''' <summary>First number of a series that has no invoice yet.</summary>
    Public Property NumarInitial As Integer = 1

    ''' <summary>Belongs to the received invoices (a later slice); carried so a save does not reset it.</summary>
    Public Property AfiseazaPrimiteNoi As Boolean

    ''' <summary>Read only (the server decides): the name and address were already taken from ANAF once, so the button is closed.</summary>
    Public Property AnafPreluat As Boolean

    ''' <summary>Read only (the server decides): the unit has issued invoices, so the series and the first number are fixed.</summary>
    Public Property AreFacturi As Boolean

End Class

''' <summary>A customer of the issued invoices (<c>EF_Clienti</c>). <see cref="IdClient"/> 0 = not saved yet.</summary>
Public NotInheritable Class EFacturaClient

    Public Property IdClient As Integer
    Public Property DenumireClient As String = String.Empty
    Public Property CodFiscal As String = String.Empty

    ''' <summary>The «RO» prefix when the customer pays VAT.</summary>
    Public Property IndFiscal As String = String.Empty

    ''' <summary>IBAN.</summary>
    Public Property Cont As String = String.Empty

    Public Property Banca As String = String.Empty
    Public Property Adresa As String = String.Empty
    Public Property Judetul As String = String.Empty
    Public Property Orasul As String = String.Empty
    Public Property Sector As String = String.Empty

    ''' <summary>The customer is a private person: the code field then holds a personal number (never log it).</summary>
    Public Property Cnp As Boolean

End Class

''' <summary>One unit of measure (UN/ECE code) of the common list.</summary>
Public NotInheritable Class EFacturaUm

    Public Property Cod As String = String.Empty
    Public Property Explicatie As String = String.Empty

End Class

''' <summary>One line of an invoice. <see cref="Valoare"/> is the server's: quantity x price, two decimals.</summary>
Public NotInheritable Class EFacturaLinie

    Public Property NrCrt As String = String.Empty
    Public Property Continut As String = String.Empty
    Public Property Um As String = String.Empty
    Public Property Cant As Decimal
    Public Property PU As Decimal
    Public Property Valoare As Decimal

    ''' <summary>Kept from migrated data so an edit does not reset it.</summary>
    Public Property Platit As Boolean

    ''' <summary>Kept from migrated data so an edit does not reset it.</summary>
    Public Property Grup As Integer

End Class

''' <summary>
''' The life of an invoice as the server names it: <c>ciorna</c> (not sent), <c>incarcata</c> (sent, result not known),
''' <c>refuzata</c> (ANAF refused it), <c>acceptata</c> (ANAF took it).
''' </summary>
Public NotInheritable Class EFacturaStare

    Public Const Ciorna As String = "ciorna"
    Public Const Incarcata As String = "incarcata"
    Public Const Refuzata As String = "refuzata"
    Public Const Acceptata As String = "acceptata"

    Private Sub New()
    End Sub

End Class

''' <summary>
''' An issued invoice: the header, and in the detail answer also the customer and the lines. The list answer fills the
''' header, <see cref="ClientDenumire"/> and <see cref="Total"/> only (<see cref="Client"/> is Nothing, <see cref="Linii"/> empty).
''' </summary>
Public NotInheritable Class EFacturaFactura

    Public Property IdFactura As Integer
    Public Property IdClient As Integer
    Public Property SerieFactura As String = String.Empty
    Public Property NumarFactura As Integer
    Public Property DataFactura As Date

    ''' <summary>380 = invoice, 384 = corrected invoice.</summary>
    Public Property TipFactura As String = "380"

    Public Property Comentarii As String = String.Empty

    ''' <summary>The order reference (BT-13).</summary>
    Public Property BT_13 As String = String.Empty

    ''' <summary>The unit's IBAN printed as the payment account.</summary>
    Public Property ContPlata As String = String.Empty

    Public Property AtasamentOriginal As Boolean

    ''' <summary>Why ANAF refused the invoice; empty otherwise.</summary>
    Public Property EroareAnaf As String = String.Empty

    ''' <summary>The invoice a storno cancels; Nothing for an ordinary invoice.</summary>
    Public Property IdFacturaA As Integer?

    Public Property SerieFacturaA As String = String.Empty
    Public Property NumarFacturaA As String = String.Empty
    Public Property Corectata As Boolean

    Public Property ClientDenumire As String = String.Empty
    Public Property Total As Decimal

    ''' <summary>One of the <see cref="EFacturaStare"/> constants.</summary>
    Public Property Stare As String = EFacturaStare.Ciorna

    Public Property EsteStorno As Boolean
    Public Property PoateModifica As Boolean
    Public Property PoateSterge As Boolean

    ''' <summary>Slice 00EF-09: the date of a draft moves only while the invoice is the last of its series.</summary>
    Public Property PoateModificaData As Boolean

    ''' <summary>Slice 00EF-09: the date of the invoice just before this one in its series; this one is not dated before it.</summary>
    Public Property DataMinima As Date?

    ''' <summary>Slice 00EF-09: an accepted invoice that was not cancelled yet (and is not itself a storno).</summary>
    Public Property PoateStorna As Boolean

    Public Property Client As EFacturaClient
    Public Property Linii As New List(Of EFacturaLinie)()

    ''' <summary>«SERIE_NUMAR» as the XML identifies the invoice, the empty series left out.</summary>
    Public ReadOnly Property Eticheta As String
        Get
            Return If(String.IsNullOrWhiteSpace(SerieFactura), NumarFactura.ToString(Globalization.CultureInfo.InvariantCulture),
                      SerieFactura.Trim() & "_" & NumarFactura.ToString(Globalization.CultureInfo.InvariantCulture))
        End Get
    End Property

End Class

''' <summary>The series and the number a new invoice would get now (<c>GET /facturi/numar-urmator</c>).</summary>
Public NotInheritable Class EFacturaNumarUrmator

    Public Property Serie As String = String.Empty
    Public Property Numar As Integer

    ''' <summary>Slice 00EF-09: the date of the newest invoice of the series; a new invoice is not dated before it.</summary>
    Public Property DataMinima As Date?

End Class

''' <summary>How ANAF answered a state check (<c>POST /facturi/{id}/verifica</c>): the <c>rezultat</c> values of the server.</summary>
Public NotInheritable Class EFacturaRezultat

    Public Const Acceptata As String = "acceptata"
    Public Const Refuzata As String = "refuzata"
    Public Const InPrelucrare As String = "in_prelucrare"
    Public Const Necunoscut As String = "necunoscut"

    Private Sub New()
    End Sub

End Class

''' <summary>ANAF took the file (<c>POST /facturi/{id}/trimite</c>): the invoice is now «incarcata».</summary>
Public NotInheritable Class EFacturaTrimitere

    Public Property Factura As EFacturaFactura
    Public Property IdIncarcare As String = String.Empty

    ''' <summary>The warnings of the server's own checks (errors would have stopped the send).</summary>
    Public Property Avertismente As New List(Of String)()

End Class

''' <summary>The state read from ANAF (<c>POST /facturi/{id}/verifica</c>). <see cref="Rezultat"/> is an <see cref="EFacturaRezultat"/> constant.</summary>
Public NotInheritable Class EFacturaVerificare

    Public Property Rezultat As String = EFacturaRezultat.Necunoscut
    Public Property StareAnaf As String = String.Empty
    Public Property Mesaj As String = String.Empty
    Public Property Factura As EFacturaFactura

End Class

''' <summary>A storno and the replacement invoice written with it (<c>POST /facturi/{id}/storno</c>); both are drafts.</summary>
Public NotInheritable Class EFacturaStornare

    Public Property Storno As EFacturaFactura
    Public Property Factura As EFacturaFactura

End Class
