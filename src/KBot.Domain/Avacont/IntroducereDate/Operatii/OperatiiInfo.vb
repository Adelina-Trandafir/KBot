Option Strict On
Imports System.Collections.Generic

' Slices AV02-03 / AV02-04 -- the «OperatiiSabloane» window: «Operatii diverse» (the Access form OP9) and «Facturi si plati» (OPuri2017). Plain data, no logic.

''' <summary>A note (NoteC) of the subunit and year, for the note combo of both pages.</summary>
Public NotInheritable Class OperatiiNotaInfo
    Public Property NumarNC As Integer
    Public Property DataNc As Date?
    ''' <summary>How many documents the note already holds.</summary>
    Public Property Documente As Integer
End Class

''' <summary>What the header of both pages needs: the notes, the next note number and the last closing.</summary>
Public NotInheritable Class OperatiiContext
    Public Property IdSubunitate As Integer
    Public Property Subunitate As String = String.Empty
    Public Property Note As IReadOnlyList(Of OperatiiNotaInfo) = New List(Of OperatiiNotaInfo)()
    ''' <summary>The next free note number of the subunit and year.</summary>
    Public Property Urmatoare As Integer
    ''' <summary>Date of the last closing document (CCV) of the year; the documents must be dated after it. Nothing = no closing yet.</summary>
    Public Property Inchidere As Date?
End Class

''' <summary>One line of a «Operatii diverse» posting: the template line and its amount.</summary>
Public NotInheritable Class OperatiiDiverseLinie
    Public Property IdLinie As Integer
    Public Property Suma As Decimal
End Class

''' <summary>The «Operatii diverse» posting: a template of type A, the header and the amount of each line.</summary>
Public NotInheritable Class OperatiiDiverseCerere
    Public Property IdSubunitate As Integer
    Public Property An As Integer
    Public Property IdSablon As Integer
    Public Property NumarNC As Integer
    Public Property NumarDocument As String = String.Empty
    Public Property FelDocument As String = String.Empty
    Public Property DataDocument As Date
    ''' <summary>The partner of the operation; 0 = none (only when no side of any line asks for it).</summary>
    Public Property IdPartener As Integer
    Public Property Linii As New List(Of OperatiiDiverseLinie)()
End Class

''' <summary>What the server posted.</summary>
Public NotInheritable Class OperatiiDiverseRezultat
    Public Property NumarNC As Integer
    Public Property An As Integer
    ''' <summary>How many operations (one per line with an amount) were posted.</summary>
    Public Property Operatii As Integer
End Class

' ── Facturi si plati ───────────────────────────────────────────────────────────

''' <summary>A partner of the unit (visible ones) with the tax code and the bank account kept on it.</summary>
Public NotInheritable Class FacturiPlatiPartener
    Public Property IdPartener As Integer
    Public Property Cod As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Cui As String = String.Empty
    Public Property Iban As String = String.Empty
End Class

''' <summary>A template offered for a classification (evidence or payment).</summary>
Public NotInheritable Class FacturiPlatiSablon
    Public Property IdSablon As Integer
    Public Property Explicatie As String = String.Empty
    ''' <summary>The text added to the payment order (payment templates only).</summary>
    Public Property FelOP As String = String.Empty
End Class

''' <summary>A classification that has templates in the subunit, with its evidence and payment templates.</summary>
Public NotInheritable Class FacturiPlatiClasificatie
    Public Property IdClsf As Integer
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Forma As String = String.Empty
    Public Property SabloaneE As IReadOnlyList(Of FacturiPlatiSablon) = New List(Of FacturiPlatiSablon)()
    Public Property SabloaneP As IReadOnlyList(Of FacturiPlatiSablon) = New List(Of FacturiPlatiSablon)()
End Class

''' <summary>What the page «Facturi si plati» needs to open: notes, partners, classifications.</summary>
Public NotInheritable Class FacturiPlatiContext
    Public Property Baza As OperatiiContext = New OperatiiContext()
    Public Property Parteneri As IReadOnlyList(Of FacturiPlatiPartener) = New List(Of FacturiPlatiPartener)()
    Public Property Clasificatii As IReadOnlyList(Of FacturiPlatiClasificatie) = New List(Of FacturiPlatiClasificatie)()
End Class

''' <summary>An evidenced invoice that is not paid in full.</summary>
Public NotInheritable Class FacturaDeschisa
    ''' <summary>The evidence operation (what a payment is allocated on).</summary>
    Public Property IdOper As Integer
    Public Property Numar As String = String.Empty
    Public Property Data As Date?
    Public Property Fel As String = String.Empty
    Public Property Valoare As Decimal
    Public Property Platit As Decimal
    Public Property Rest As Decimal
End Class

''' <summary>A payment order of the partner in the year, with its lock.</summary>
Public NotInheritable Class PlataOrdin
    Public Property IdOP As Integer
    Public Property NrOP As Integer
    Public Property DataOP As Date?
    Public Property Valoare As Decimal
    Public Property Explicatie As String = String.Empty
    Public Property Documente As String = String.Empty
    ''' <summary>True when the order sits in an OPME or a bank statement confirms it: it can no longer be deleted.</summary>
    Public Property Blocat As Boolean
    Public Property Motiv As String = String.Empty
End Class

''' <summary>The invoices and orders of one partner.</summary>
Public NotInheritable Class FacturiPartener
    Public Property Curent As IReadOnlyList(Of FacturaDeschisa) = New List(Of FacturaDeschisa)()
    Public Property Anterior As IReadOnlyList(Of FacturaDeschisa) = New List(Of FacturaDeschisa)()
    Public Property Plati As IReadOnlyList(Of PlataOrdin) = New List(Of PlataOrdin)()
    ''' <summary>The number the next order would get (PROVISIONAL: the real one comes back from the save).</summary>
    Public Property NumarOP As Integer
    Public Property ContIban As String = String.Empty
End Class

''' <summary>An invoice typed by hand.</summary>
Public NotInheritable Class FacturaNoua
    Public Property Numar As String = String.Empty
    Public Property Data As Date
    Public Property Suma As Decimal
    ''' <summary>«Factura» or «Nota credit».</summary>
    Public Property Fel As String = "Factură"
    ''' <summary>What is paid now on it (0 = nothing).</summary>
    Public Property Platit As Decimal
End Class

''' <summary>The amount paid now on an invoice that is already evidenced.</summary>
Public NotInheritable Class FacturaAleasa
    Public Property IdOper As Integer
    Public Property Platit As Decimal
End Class

''' <summary>The save of «Facturi si plati»: typed invoices (evidence) and/or the payment with its order.</summary>
Public NotInheritable Class FacturiPlatiCerere
    Public Property IdSubunitate As Integer
    Public Property An As Integer
    Public Property IdPartener As Integer
    Public Property IdClsf As Integer
    ''' <summary>0 = no evidence template (no typed invoice).</summary>
    Public Property IdSablonE As Integer
    ''' <summary>0 = no payment.</summary>
    Public Property IdSablonP As Integer
    Public Property NumarNC As Integer
    Public Property DataOP As Date
    Public Property ExplicatieOP As String = String.Empty
    Public Property Noi As New List(Of FacturaNoua)()
    Public Property Existente As New List(Of FacturaAleasa)()
End Class

''' <summary>What the save posted; the order number is the REAL one.</summary>
Public NotInheritable Class FacturiPlatiRezultat
    Public Property NumarNC As Integer
    Public Property An As Integer
    Public Property Facturi As Integer
    ''' <summary>The real number of the order; 0 = no payment was made.</summary>
    Public Property NrOP As Integer
    Public Property IdOP As Integer
    Public Property ValoarePlata As Decimal
End Class
