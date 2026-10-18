Option Strict On
Imports System.Collections.Generic

' Slices KE01..KE07 -- what the routes of the links invoice <-> reception answer
' (PYTHON/routes/efactura/primite_ke_routes.py). Plain data, no logic. Amounts are WITH VAT and carry the sign of the invoice:
' a credit note is negative, and so are its allocations (plan docs/KE_PLAN.md).

''' <summary>The three kinds of link a reception can have with an invoice (the <c>Tip</c> of the routes).</summary>
Public NotInheritable Class EFacturaKeTip

    Private Sub New()
    End Sub

    ''' <summary>An existing reception that already has the value: only the link is written.</summary>
    Public Const Existenta As String = "existenta"

    ''' <summary>A reception K-BOT creates in FOREXE for this invoice, then links.</summary>
    Public Const Noua As String = "noua"

    ''' <summary>An existing reception K-BOT raises in FOREXE by the allocated amount, then links.</summary>
    Public Const Modificata As String = "modificata"

End Class

''' <summary>A line of the invoice with its value WITH VAT and the classification the accountant gave it.</summary>
Public NotInheritable Class EFacturaKeLinie
    Public Property IdLinie As Integer
    Public Property NrLinie As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Cpv As String = String.Empty

    ''' <summary>The taxable value of the line.</summary>
    Public Property Valoare As Decimal

    Public Property CotaTva As Decimal

    ''' <summary>True when the XML gave the line no rate and the server took the rate of the biggest taxable amount of the invoice.</summary>
    Public Property CotaDedusa As Boolean

    ''' <summary>The value WITH VAT, with the sign of the invoice: what a reception takes for this line.</summary>
    Public Property ValoareCuTva As Decimal

    ''' <summary>Clasificatii.IDClsf chosen for the line; Nothing = not chosen.</summary>
    Public Property IdClsf As Integer?

    Public Property Clsf As String = String.Empty
End Class

''' <summary>One classification of the invoice: what its lines are worth, what receptions cover, what is left.</summary>
Public NotInheritable Class EFacturaKeClasificatie
    Public Property IdClsf As Integer
    Public Property Clsf As String = String.Empty
    Public Property Valoare As Decimal
    Public Property Alocat As Decimal
    Public Property Rest As Decimal
End Class

''' <summary>A link row: an amount of one classification of the invoice covered by one reception.</summary>
Public NotInheritable Class EFacturaKeLegatura
    Public Property IdLegatura As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property IdClsf As Integer
    Public Property Clsf As String = String.Empty
    Public Property Suma As Decimal
    Public Property Tip As String = EFacturaKeTip.Existenta
    Public Property Un As String = String.Empty

    ''' <summary>FX_Receptii_R.IDRR of the reception; Nothing while the link waits for the download that brings the reception.</summary>
    Public Property Idrr As Integer?

    Public Property Idrh As Integer?
    Public Property Idr As Integer?
    Public Property DataR As Date?
    Public Property TipReceptie As String = String.Empty
    Public Property DescriereR As String = String.Empty

    ''' <summary>The reception is not known to K-BOT yet (the download that brings it has not run).</summary>
    Public Property AsteaptaDescarcarea As Boolean

    ''' <summary>The reception was deleted on the site: the link stays visible.</summary>
    Public Property StearsaPeSite As Boolean
End Class

''' <summary>An angajament an invoice can be linked to.</summary>
Public NotInheritable Class EFacturaKeAngajament
    Public Property CodAngajament As String = String.Empty
    Public Property Descriere As String = String.Empty
    Public Property Stare As String = String.Empty

    ''' <summary>«ddf» (the supplier is a partner of its DDF, or it was linked by hand) or «legatura» (the invoice already has a link to it).</summary>
    Public Property Sursa As String = String.Empty
End Class

''' <summary>The state of the links of one invoice, as the «Receptii» view shows it.</summary>
Public NotInheritable Class EFacturaKeStare

    ''' <summary>The session's role may write (it is «Contabil»); the server checks it again on every write.</summary>
    Public Property PoateScrie As Boolean

    Public Property Total As Decimal
    Public Property Alocat As Decimal
    Public Property Rest As Decimal

    ''' <summary>«nelegata», «partiala», «acoperita» or «depasita».</summary>
    Public Property Stare As String = "nelegata"

    ''' <summary>What the lines do not explain of the total (an allowance or a charge on the whole document); shown, never spread.</summary>
    Public Property NealocatLiniilor As Decimal

    ''' <summary>The value of the lines that have no classification yet.</summary>
    Public Property FaraClasificatie As Decimal

    Public ReadOnly Property Linii As New List(Of EFacturaKeLinie)()
    Public ReadOnly Property PeClasificatie As New List(Of EFacturaKeClasificatie)()
    Public ReadOnly Property Legaturi As New List(Of EFacturaKeLegatura)()
    Public ReadOnly Property Angajamente As New List(Of EFacturaKeAngajament)()
End Class

''' <summary>An angajament in the picker used when the supplier has no DDF.</summary>
Public NotInheritable Class EFacturaKeAngajamentAlegere
    Public Property CodAngajament As String = String.Empty
    Public Property Descriere As String = String.Empty
    Public Property AliasName As String = String.Empty
    Public Property Stare As String = String.Empty
    Public Property DataCreare As Date?
    Public Property Parteneri As String = String.Empty

    Public ReadOnly Property Eticheta As String
        Get
            ' The alias is the short name the operator gave in K-BOT; the description is the one the site shows: both are recognised.
            If String.IsNullOrWhiteSpace(AliasName) Then Return Descriere
            If String.IsNullOrWhiteSpace(Descriere) Then Return AliasName
            Return $"{AliasName} — {Descriere}"
        End Get
    End Property
End Class

''' <summary>An indicator of an angajament: the classification a reception can take a value on.</summary>
Public NotInheritable Class EFacturaKeIndicator
    Public Property CodAi As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property IdClsf As Integer?
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property AngajamentLegal As Decimal
    Public Property Receptii As Decimal
    Public Property Plati As Decimal
    Public Property Ss As String = String.Empty
End Class

''' <summary>Slice KE12-03: what an indicator has on a day -- the budget of its classification then and the receptions taken BEFORE that day (the rule of the fundamentation document).</summary>
Public NotInheritable Class EFacturaKeDisponibil
    Public Property IdClsf As Integer?
    Public Property CodIndicator As String = String.Empty

    ''' <summary>The budget on the day; Nothing = none known (the cells stay empty and nothing is checked).</summary>
    Public Property Buget As Decimal?

    Public Property Receptii As Decimal
End Class

''' <summary>The answer of <c>GetKeDisponibilAsync</c>.</summary>
Public NotInheritable Class EFacturaKeDisponibilZi
    Public ReadOnly Property Indicatori As New List(Of EFacturaKeDisponibil)()

    ''' <summary>What the server tells about the budget (a classification that fell back to today's credit).</summary>
    Public ReadOnly Property Avertismente As New List(Of String)()
End Class

''' <summary>One classification of a reception: its value now, what invoices cover, what is left.</summary>
Public NotInheritable Class EFacturaKeReceptieClasificatie
    Public Property IdClsf As Integer
    Public Property Clsf As String = String.Empty
    Public Property Valoare As Decimal
    Public Property Alocat As Decimal
    Public Property Rest As Decimal
End Class

''' <summary>A reception of the angajament (the last counting snapshot gives its value).</summary>
Public NotInheritable Class EFacturaKeReceptie
    Public Property Idrr As Integer
    Public Property DataR As Date?
    Public Property TipReceptie As String = String.Empty
    Public Property Descriere As String = String.Empty
    Public Property Idrh As Integer?
    Public Property Total As Decimal

    ''' <summary>Every snapshot number (FX_Receptii_H.IDRH) of the reception: the K-BOT marker the site keeps in its description names one of them.</summary>
    Public ReadOnly Property Idrhs As New List(Of Integer)()

    Public ReadOnly Property PeClasificatie As New List(Of EFacturaKeReceptieClasificatie)()
End Class

''' <summary>The indicators and the receptions of one angajament.</summary>
Public NotInheritable Class EFacturaKeReceptiiAngajament
    Public ReadOnly Property Indicatori As New List(Of EFacturaKeIndicator)()
    Public ReadOnly Property Receptii As New List(Of EFacturaKeReceptie)()
End Class

''' <summary>An amount of one classification to allocate.</summary>
Public NotInheritable Class EFacturaKeAlocare
    Public Property IdClsf As Integer
    Public Property Suma As Decimal
End Class

''' <summary>What the accountant asks for: an angajament, a kind of link and the amounts.</summary>
Public NotInheritable Class EFacturaKeCerere
    Public Property CodAngajament As String = String.Empty
    Public Property Tip As String = EFacturaKeTip.Existenta

    ''' <summary>The reception: for <see cref="EFacturaKeTip.Existenta"/> and <see cref="EFacturaKeTip.Modificata"/>.</summary>
    Public Property Idrr As Integer?

    ''' <summary>The numbers the K-BOT marker reserved for a NEW reception.</summary>
    Public Property Idrh As Integer?

    Public Property Idr As Integer?
    Public ReadOnly Property Alocari As New List(Of EFacturaKeAlocare)()
End Class

''' <summary>A row the robot needs: where on the site a classification is, and what to write on it.</summary>
Public NotInheritable Class EFacturaKeRand
    Public Property IdClsf As Integer
    Public Property CodAi As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property Clsf As String = String.Empty

    ''' <summary>The classification as the site's table matches it: letters and digits only.</summary>
    Public Property ClsfSal As String = String.Empty

    Public Property Ss As String = String.Empty
    Public Property Suma As Decimal

    ''' <summary>For a modified reception: its value on this classification now and after the change.</summary>
    Public Property ValoareVeche As Decimal?

    Public Property ValoareNoua As Decimal?
End Class

''' <summary>The server's check of an allocation, before the robot starts or a link is written.</summary>
Public NotInheritable Class EFacturaKeVerificare
    Public Property Ok As Boolean
    Public ReadOnly Property Probleme As New List(Of String)()
    Public ReadOnly Property Avertismente As New List(Of String)()
    Public ReadOnly Property Randuri As New List(Of EFacturaKeRand)()
End Class
