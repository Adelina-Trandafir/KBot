Option Strict On
Imports System.Collections.Generic

' Slice 0087 -- the two lists K-BOT now edits itself: the budget classifications (with their yearly
' budget and corrections) and the partners (with their angajament codes). Plain data carried
' between KBot.Api and the windows in KBot.App; no I/O, so no Try/Catch (house rule).

''' <summary>One row of <c>Clasificatii</c>: a leaf (alineat) of the classification tree.</summary>
Public NotInheritable Class Clasificatie
    Public Property IdClsf As Integer
    ''' <summary>The unit the classification belongs to (slice 0103-06: picks its Access file).</summary>
    Public Property IdUnitate As Integer?
    ''' <summary>The id of the same classification in the Access file (<c>Clasificatii.IdClsfAcc</c>).</summary>
    Public Property IdClsfAcc As Integer
    ''' <summary>«65.02».</summary>
    Public Property Capitol As String = String.Empty
    ''' <summary>«04.02».</summary>
    Public Property Subcapitol As String = String.Empty
    ''' <summary>«10.01».</summary>
    Public Property Articol As String = String.Empty
    ''' <summary>«01».</summary>
    Public Property Alineat As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>Sector-source, «02A».</summary>
    Public Property Ss As String = String.Empty
    ''' <summary>The full code, «65.02.04.02.10.01.01».</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Slice 0107: FOREXE reported a credit for it (<c>FX_Indicatori_Buget</c> has a row).</summary>
    Public Property InForexe As Boolean
End Class

''' <summary>
''' A sector-source that has CHILD subunits (slice AV01-11). A child has the same source as its parent, so it works on the parent's classifications and budget:
''' nothing is blocked, the windows only tell the operator that the work goes straight into the parent's data.
''' </summary>
Public NotInheritable Class ChildSourceInfo
    ''' <summary>The parent subunit's name (several names joined by «, »).</summary>
    Public Property Parinte As String = String.Empty
    Public Property Copii As New List(Of String)()

    ''' <summary>The sentence the classification windows show for the source <paramref name="k_ss"/> (operator text).</summary>
    Public Function NoteFor(k_ss As String) As String
        Return $"Sursa {k_ss} are subunități copil ({String.Join(", ", Copii)}): clasificațiile și bugetul sunt ale părintelui ({Parinte}) " &
               "și se lucrează direct în ele."
    End Function
End Class

''' <summary>
''' Every classification of the database plus the names of the upper tree levels, which are not
''' stored in <c>Clasificatii</c> (they come from the AVACONT_COMUN dictionaries).
''' </summary>
Public NotInheritable Class ClasificatiiCatalog
    Public Property Items As New List(Of Clasificatie)()
    ''' <summary>Chapter names by the first two digits of the capitol: «65» -&gt; «Invatamant».</summary>
    Public Property CapitolNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Sub-chapter names by ClsfF (six digits): «650402» -&gt; name.</summary>
    Public Property SubcapitolNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Title names by the first two digits of the articol: «10» -&gt; name.</summary>
    Public Property TitluNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Article names by articol: «10.01» -&gt; name.</summary>
    Public Property ArticolNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Sector-source names: «02A» -&gt; name.</summary>
    Public Property SsNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Slice AV01-11: the sector-sources that have child subunits («02A» -&gt; parent + children); empty when none or when the database has no subunits yet.</summary>
    Public Property ChildSources As New Dictionary(Of String, ChildSourceInfo)(StringComparer.Ordinal)
End Class

''' <summary>The four quarters of a budget version or of a correction. Nothing = empty cell.</summary>
Public NotInheritable Class QuarterAmounts
    Public Property Trim1 As Decimal?
    Public Property Trim2 As Decimal?
    Public Property Trim3 As Decimal?
    Public Property Trim4 As Decimal?

    ''' <summary>The sum of the four quarters, empty ones counted as zero. Used for a correction's
    ''' row only: a budget has no yearly total (operator, 02.10.2026).</summary>
    Public ReadOnly Property Total As Decimal
        Get
            Return Trim1.GetValueOrDefault() + Trim2.GetValueOrDefault() +
                   Trim3.GetValueOrDefault() + Trim4.GetValueOrDefault()
        End Get
    End Property
End Class

''' <summary>One row of <c>Clasificatii_Rectificari</c>.</summary>
Public NotInheritable Class RectificareBugetara
    ''' <summary>Nothing = a row added in the window, not saved yet.</summary>
    Public Property Id As Integer?
    ''' <summary>«Nr. doc.» (the <c>Document</c> column).</summary>
    Public Property Document As String = String.Empty
    Public Property Data As Date?
    Public Property Amounts As New QuarterAmounts()
End Class

''' <summary>
''' One row of <c>Clasificatii_Buget</c> (slice 0102): the budget of a classification for a year
''' FROM <see cref="StartDate"/> on. The budget on a day is the version with the greatest start date
''' up to it, plus the corrections from that start date on (see <c>routes/forexe/budget_on_day.py</c>).
''' </summary>
Public NotInheritable Class BudgetVersion
    ''' <summary>Nothing = a version added in the window, not saved yet.</summary>
    Public Property Id As Integer?
    ''' <summary>«Început» (the <c>DataInceput</c> column): the day this version starts to apply.</summary>
    Public Property StartDate As Date?
    Public Property Amounts As New QuarterAmounts()
End Class

''' <summary>The budget versions of one classification for one year, with that year's corrections.</summary>
Public NotInheritable Class BugetClasificatie
    ''' <summary>Empty = no <c>Clasificatii_Buget</c> row for that year yet. Oldest start date first.</summary>
    Public Property Budgets As New List(Of BudgetVersion)()
    Public Property Corrections As New List(Of RectificareBugetara)()
End Class

''' <summary>
''' What the budget grids show for a node above the leaves of the tree: per classification, its LAST
''' budget version of the year and the TOTAL of the year's corrections (quarter by quarter).
''' </summary>
Public NotInheritable Class BudgetSummaryRow
    Public Property IdClsf As Integer
    ''' <summary>True when some quarter of some budget version or correction of the year is not zero
    ''' (quarters tested one by one: +1000 and -1000 total 0 and still count).</summary>
    Public Property Active As Boolean
    ''' <summary>Nothing = the classification has no budget version in the year.</summary>
    Public Property LastBudget As BudgetVersion
    ''' <summary>Nothing = the classification has no correction in the year.</summary>
    Public Property CorrectionsTotal As QuarterAmounts
End Class

''' <summary>
''' One indicator of an angajament that leads to a classification, with the three totals the angajament summary shows (slice 0126-01):
''' reservations, receptions and payments.
''' </summary>
Public NotInheritable Class ClasificatieAngajament
    Public Property CodAngajament As String = String.Empty
    Public Property Descriere As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property TotalRezervari As Decimal
    Public Property TotalReceptii As Decimal
    Public Property TotalPlati As Decimal
End Class

''' <summary>
''' One classification in the check of slice 0103-04: what FOREXE last reported as the budget of the
''' classification (<c>FX_Indicatori_Buget.CreditBugetar</c>, one row per classification since slice
''' 0108) against what K-BOT holds (the TOTAL of the version in force + the total of its
''' rectifications; no quarter cut-off).
''' </summary>
Public NotInheritable Class BudgetCheckRow
    Public Property IdClsf As Integer
    Public Property IdUnitate As Integer?
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Ss As String = String.Empty
    ''' <summary>Nothing = no budget version covers the day.</summary>
    Public Property BugetKbot As Decimal?
    ''' <summary>Nothing = FOREXE reported no credit for the classification yet.</summary>
    Public Property CreditFx As Decimal?
    ''' <summary>FOREXE minus K-BOT.</summary>
    Public Property Diferenta As Decimal
    Public Property Egal As Boolean
End Class

''' <summary>The whole check: the day it was made for and one row per classification.</summary>
Public NotInheritable Class BudgetCheck
    Public Property Day As Date
    Public Property Rows As New List(Of BudgetCheckRow)()
    ''' <summary>The rows whose difference is not zero (rounded to cents): the only ones the operator sees.</summary>
    Public ReadOnly Property Differences As IEnumerable(Of BudgetCheckRow)
        Get
            Return Rows.Where(Function(r) r.Diferenta <> 0D)
        End Get
    End Property
End Class

''' <summary>A code + name pair from a dictionary (sector-source, functional or economic code).</summary>
Public NotInheritable Class CodeName
    Public Property Code As String = String.Empty
    Public Property Name As String = String.Empty
End Class

''' <summary>
''' The lists the «add classifications» window picks from -- the same three the public registration
''' page offers, with the sector-sources limited to the units of this database.
''' </summary>
Public NotInheritable Class ClasificatiiNomenclator
    Public Property SectorSources As New List(Of CodeName)()
    ''' <summary>The functional chapters («65») each sector-source already has classifications in, keyed by sector-source.</summary>
    Public Property ChaptersBySs As New Dictionary(Of String, List(Of String))(StringComparer.Ordinal)
    ''' <summary>Functional codes (ClsfF, six digits).</summary>
    Public Property FunctionalCodes As New List(Of CodeName)()
    Public Property FunctionalGroups As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Economic codes (ClsfE, six digits).</summary>
    Public Property EconomicCodes As New List(Of CodeName)()
    ''' <summary>Names of the upper economic levels: «20» (title) and «2001» (article).</summary>
    Public Property EconomicGroups As New Dictionary(Of String, String)(StringComparer.Ordinal)
End Class

''' <summary>What the server did with an «add classifications» request.</summary>
Public NotInheritable Class ClasificatiiAddResult
    Public Property Requested As Integer
    Public Property Inserted As Integer
    Public Property Existing As Integer
End Class

''' <summary>
''' One row of the supplier's «Coduri angajament» (slice 0121-03): an indicator of one of its angajamente
''' (<c>Parteneri_Angajamente_Indicatori</c>), or the angajament alone while it has no indicator (<see cref="CodInd"/>
''' empty, <see cref="Id"/> Nothing, <see cref="IdLeg"/> set). The rows belong to the supplier's CodFiscal.
''' </summary>
Public NotInheritable Class PartenerCod
    ''' <summary>The indicator row. Nothing = a row added in the window, or an angajament without indicator.</summary>
    Public Property Id As Integer?
    ''' <summary>The link supplier-angajament. Nothing = a row added in the window, not saved yet.</summary>
    Public Property IdLeg As Integer?
    ''' <summary>Shown only (from FX_Indicatori by CodAI); Nothing = the indicator is not known to FX_Indicatori.</summary>
    Public Property IdClsf As Integer?
    Public Property Clsf As String = String.Empty
    Public Property DenumireClsf As String = String.Empty
    Public Property ContBancar As String = String.Empty
    Public Property CodAng As String = String.Empty
    Public Property CodInd As String = String.Empty
End Class

''' <summary>One row of <c>Parteneri</c>.</summary>
Public NotInheritable Class Partener
    ''' <summary>Nothing = a partner added in the window, not saved yet.</summary>
    Public Property IdPartener As Integer?
    Public Property IdUnitate As Integer
    Public Property Ss As String = String.Empty
    Public Property CodPartener As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property CodFiscal As String = String.Empty
    Public Property ContIban As String = String.Empty
    Public Property Banca As String = String.Empty
    Public Property Adresa As String = String.Empty
    ''' <summary>Always "1" since slice 0093 (the server writes it; the window does not show it).</summary>
    Public Property Tip As String = String.Empty
    Public Property Ascuns As Boolean
    ''' <summary>True when a DDF / ORD document uses the partner (it cannot be deleted then).</summary>
    Public Property Activ As Boolean
    Public Property Coduri As New List(Of PartenerCod)()
End Class

''' <summary>A classification offered in the «Coduri angajament» combo.</summary>
Public NotInheritable Class ClasificatieOption
    Public Property IdClsf As Integer
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Ss As String = String.Empty
    Public Property IdUnitate As Integer?
End Class

''' <summary>
''' Everything the «Parteneri» window shows. Since slice 0093 the server sends only partners with
''' Tip "1", a fiscal code other than the unit's own, not hidden, one per fiscal code.
''' </summary>
Public NotInheritable Class ParteneriCatalog
    Public Property Partners As New List(Of Partener)()
    Public Property Clasificatii As New List(Of ClasificatieOption)()
    ''' <summary>AVACONT_COMUN.BIC: 4-letter BIC code (IBAN characters 5-8, upper case) to bank name.</summary>
    Public Property Bic As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>The unit's own fiscal code, digits only (it cannot be a partner).</summary>
    Public Property CfUnitate As String = String.Empty
End Class

''' <summary>What ANAF says about a fiscal code, used to pre-fill a partner.</summary>
Public NotInheritable Class PartenerAnaf
    Public Property Cui As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Adresa As String = String.Empty
End Class

''' <summary>One partner to save, with the codes changed in the window.</summary>
Public NotInheritable Class PartenerSaveRequest
    Public Property Partner As Partener
    ''' <summary>The codes as the grid shows them (new rows have no id).</summary>
    Public Property Coduri As New List(Of PartenerCod)()
    ''' <summary>Ids (<see cref="PartenerCod.Id"/>) of saved indicators removed in the window.</summary>
    Public Property CoduriSterse As New List(Of Integer)()
    ''' <summary>Ids (<see cref="PartenerCod.IdLeg"/>) of saved angajamente without indicator removed in the window.</summary>
    Public Property LegaturiSterse As New List(Of Integer)()
End Class
