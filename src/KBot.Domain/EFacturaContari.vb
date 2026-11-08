Option Strict On
Imports System.Collections.Generic

' Slice 01EF-02 -- the «Contari» of the received invoices: what the server calculates for the ticked invoices (nothing is written yet; the save is 01EF-04/05).
' Plain data, no logic. Wire names and rules: PYTHON/routes/efactura/primite_contari.py.

''' <summary>What the page asks: the ticked invoices of ONE supplier and the choices made so far.</summary>
Public NotInheritable Class EFacturaContariCerere
    Public ReadOnly Property Ids As New List(Of Integer)()
    Public Property IdClsf As Integer?
    Public Property IdSablonE As Integer?
    Public Property IdSablonP As Integer?

    ''' <summary>The value to pay per invoice, when the operator changed it (an invoice not named here pays the whole rest).</summary>
    Public ReadOnly Property Platit As New Dictionary(Of Integer, Decimal)()

    ''' <summary>The work year: an invoice of another year gets a warning.</summary>
    Public Property An As Integer?

    ''' <summary>The codes of the warnings the operator confirmed (the save only).</summary>
    Public ReadOnly Property Confirmari As New List(Of String)()

    ''' <summary>The subunit chosen in the title bar when several share the sector+source of the classification.</summary>
    Public Property IdSubunitate As Integer?

    ''' <summary>The accounting note chosen or typed (the save only): an existing number takes the new documents, a new number makes a new note, Nothing = the next number.</summary>
    Public Property NumarNC As Integer?

    ''' <summary>The partner chosen when several have the supplier's tax code (Nothing = the server uses the only one, or answers that a choice is needed).</summary>
    Public Property IdPartener As Integer?

    ''' <summary>Slice 04EF-01: what is booked -- «evidentiere» (the default), «plata» or «ambele».</summary>
    Public Property [Mod] As String = "evidentiere"

    ''' <summary>Slice 04EF-01: the tick «Protectie contari Plati/Receptii» of the operator: a missing reception then blocks the payment (without it, it only warns).</summary>
    Public Property Protectie As Boolean

    ''' <summary>Slice 04EF-01 (the save only): the date of the payment and of its order; Nothing = today.</summary>
    Public Property DataOP As Date?

    ''' <summary>Slice 04EF-01 (the save only): the explanation of the order of payment as the operator left it; Nothing = the server's composition.</summary>
    Public Property ExplicatieOP As String
End Class

''' <summary>A subunit of the booking.</summary>
Public NotInheritable Class EFacturaContariSubunitate
    Public Property IdSubunitate As Integer
    Public Property Denumire As String = String.Empty
End Class

''' <summary>An accounting note of the subunit and year, to choose from.</summary>
Public NotInheritable Class EFacturaContariNota
    Public Property NumarNC As Integer
    Public Property DataNc As Date?
    Public Property Documente As Integer
End Class

''' <summary>Slice 04EF-01 -- the payment the server booked: the order of payment with its REAL number (the one shown before saving was only provisional).</summary>
Public NotInheritable Class EFacturaContariPlataSalvata
    Public Property IdOper As Integer
    Public Property IdOP As Integer
    Public Property NrOP As Integer
    Public Property Valoare As Decimal
    Public Property DataOP As Date?
End Class

''' <summary>What the server booked (slice 01EF-04).</summary>
Public NotInheritable Class EFacturaContariSalvat
    Public Property IdNota As Integer
    Public Property NumarNC As Integer
    Public Property An As Integer
    Public Property Subunitate As String = String.Empty
    Public Property NrOperatii As Integer

    ''' <summary>Slice 04EF-01: the payment and its order, when the save made one; Nothing for an evidence-only save.</summary>
    Public Property Plata As EFacturaContariPlataSalvata
End Class

''' <summary>A classification a template fits.</summary>
Public NotInheritable Class EFacturaContariClsf
    Public Property IdClsf As Integer
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Forma As String = String.Empty

    ''' <summary>One of the classifications the supplier has configured (<c>Parteneri_Coduri</c>): the list shows them first.</summary>
    Public Property AlePartenerului As Boolean
End Class

''' <summary>A template of type M that fits the classification (evidence or payment).</summary>
Public NotInheritable Class EFacturaContariSablon
    Public Property IdSablon As Integer
    Public Property Explicatie As String = String.Empty
    Public Property FelOP As String = String.Empty
    Public Property Jurnal As String = String.Empty
End Class

''' <summary>One invoice in the evidence grid (Access EF_F).</summary>
Public NotInheritable Class EFacturaContariRand
    Public Property IdPrimita As Integer
    Public Property NrFact As String = String.Empty
    Public Property NumarDocument As String = String.Empty
    Public Property DataDocument As Date?
    Public Property FelDocument As String = String.Empty
    Public Property Total As Decimal

    ''' <summary>Already booked (<c>EF_PrimiteOperatii</c>).</summary>
    Public Property Salvat As Decimal

    ''' <summary>Left to book (negative for a credit note).</summary>
    Public Property Suma As Decimal

    ''' <summary>The value to pay; never above <see cref="Suma"/> (evidence only) or the rest to pay (a payment).</summary>
    Public Property Platit As Decimal

    ''' <summary>Slice 04EF-01: what is evidenced on the invoice and not yet paid; Nothing while only the evidence is booked.</summary>
    Public Property RestPlata As Decimal?
End Class

''' <summary>The payment row (Access EF_P).</summary>
Public NotInheritable Class EFacturaContariPlata
    Public Property Valoare As Decimal
    Public Property ExplicatieOP As String = String.Empty

    ''' <summary>The «kind of invoice» of the chosen payment template (<c>FelOP</c>; empty when the template has none): it ends the explanation of the order.</summary>
    Public Property FelOP As String = String.Empty

    ''' <summary>The number the order of payment would get now (highest + 1 by the setting PlajaComunaOPuri). Slice 04EF-01: it is only PROVISIONAL, the server gives the real one on save.</summary>
    Public Property NumarOP As Integer?

    ''' <summary>True when <see cref="NumarOP"/> is a provisional number (the cell says so in its tooltip).</summary>
    Public Property NumarOPProvizoriu As Boolean

    ''' <summary>The newest date of the invoices: the order of payment cannot be older.</summary>
    Public Property DataMinima As Date?

    ''' <summary>There is a payment template and something to pay.</summary>
    Public Property Activa As Boolean
End Class

''' <summary>One debit / credit line a booking would produce.</summary>
Public NotInheritable Class EFacturaContariLinie
    Public Property Debit As String = String.Empty
    Public Property PartenerDebit As String = String.Empty
    Public Property Credit As String = String.Empty
    Public Property PartenerCredit As String = String.Empty
    Public Property PA As Boolean
    Public Property Ordonantare As Boolean
    Public Property Chelt As Boolean
End Class

''' <summary>A booking the chosen template would produce: the evidence of an invoice (<see cref="Faza"/> E) or the payment (P).</summary>
Public NotInheritable Class EFacturaContariOperatie
    Public Property Titlu As String = String.Empty
    Public Property Faza As String = String.Empty
    Public Property IdPrimita As Integer?
    Public Property Suma As Decimal
    Public ReadOnly Property Linii As New List(Of EFacturaContariLinie)()
End Class

''' <summary>A warning. <see cref="Grav"/>: «eroare» (blocks the booking), «confirmare» (the operator must confirm), «atentie», «info».</summary>
Public NotInheritable Class EFacturaContariAvertisment
    Public Property Cod As String = String.Empty
    Public Property Grav As String = String.Empty
    Public Property Text As String = String.Empty
End Class

''' <summary>The server's calculation for the ticked invoices.</summary>
Public NotInheritable Class EFacturaContariRaspuns
    Public Property PoateScrie As Boolean
    Public Property FurnizorDenumire As String = String.Empty
    Public Property FurnizorCui As String = String.Empty
    Public Property PartenerGasit As Boolean

    ''' <summary>EVERY partner with the supplier's tax code (found by tax code only). More than one and none chosen: <see cref="PartenerGasit"/> is False and the operator picks.</summary>
    Public ReadOnly Property Parteneri As New List(Of EFacturaPartenerCui)()
    Public ReadOnly Property Clasificatii As New List(Of EFacturaContariClsf)()
    Public Property Clasificatie As EFacturaContariClsf
    Public Property ClasificatieSugerata As Integer?
    Public Property Subunitate As EFacturaContariSubunitate
    Public ReadOnly Property Subunitati As New List(Of EFacturaContariSubunitate)()
    Public Property NoteAn As Integer
    Public Property NotaUrmatoare As Integer
    Public ReadOnly Property Note As New List(Of EFacturaContariNota)()
    Public ReadOnly Property SabloaneEvidentiere As New List(Of EFacturaContariSablon)()
    Public ReadOnly Property SabloanePlata As New List(Of EFacturaContariSablon)()
    Public Property IdSablonE As Integer?
    Public Property IdSablonP As Integer?

    ''' <summary>Slice 04EF-01: the mode the server calculated for -- «evidentiere», «plata» or «ambele».</summary>
    Public Property [Mod] As String = "evidentiere"
    Public Property CodAng As String = String.Empty
    Public Property CodInd As String = String.Empty
    Public Property ContIban As String = String.Empty
    Public ReadOnly Property Evidenta As New List(Of EFacturaContariRand)()
    Public Property Plata As New EFacturaContariPlata()
    Public ReadOnly Property Operatii As New List(Of EFacturaContariOperatie)()
    Public ReadOnly Property Avertismente As New List(Of EFacturaContariAvertisment)()
End Class

''' <summary>Slice 03EF-13 -- one accounting operation an invoice already has: <see cref="Faza"/> E (evidence) or P (payment).</summary>
Public NotInheritable Class EFacturaContariExistenta
    Public Property Faza As String = String.Empty
    Public Property Suma As Decimal

    ''' <summary>The template the operation was made from; Nothing for a migrated one or one typed by hand.</summary>
    Public Property IdSablon As Integer?
    Public Property Explicatie As String = String.Empty
    Public Property NumarDocument As String = String.Empty
    Public Property DataDocument As Date?
    Public Property FelDocument As String = String.Empty
    Public Property NumarNC As Integer?
    Public Property Subunitate As String = String.Empty

    ''' <summary>False = no TZ_Oper row under the key of the link (the figures of the row are only the amount of the link).</summary>
    Public Property Gasita As Boolean = True
End Class

''' <summary>Slice 04EF-01 -- a payment already made on an invoice, with its order and whether the order can still be changed.</summary>
Public NotInheritable Class EFacturaContariPlataExistenta
    Public Property IdOP As Integer?
    Public Property IdPlata As Integer
    Public Property NrOP As Integer?
    Public Property Platit As Decimal
    Public Property DataPlata As Date?
    Public Property Explicatie As String = String.Empty
    Public Property NumarNC As Integer?

    ''' <summary>The order sits in an OPME or a bank statement confirms it: neither the payment nor the order can be edited or deleted.</summary>
    Public Property Blocat As Boolean
    Public Property MotivBlocare As String = String.Empty
End Class

''' <summary>Slice 03EF-13 -- what is already booked for one invoice, to show on the page «Contari» when the invoice is clicked.</summary>
Public NotInheritable Class EFacturaContariExistente
    Public Property IdPrimita As Integer
    Public Property NrFact As String = String.Empty
    Public Property NumarDocument As String = String.Empty
    Public Property DataFact As Date?
    Public Property FelDocument As String = String.Empty

    ''' <summary>The total of the invoice (negative for a credit note).</summary>
    Public Property Total As Decimal
    Public Property Clasificatie As EFacturaContariClsf
    Public ReadOnly Property SabloaneEvidentiere As New List(Of EFacturaContariSablon)()
    Public ReadOnly Property SabloanePlata As New List(Of EFacturaContariSablon)()
    Public ReadOnly Property Operatii As New List(Of EFacturaContariExistenta)()

    ''' <summary>Slice 04EF-01: the payments of the invoice and what is left to pay.</summary>
    Public ReadOnly Property Plati As New List(Of EFacturaContariPlataExistenta)()
    Public Property RestPlata As Decimal
End Class
