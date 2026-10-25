Option Strict On
Imports System.Collections.Generic

' Slice AV01-04: subunits (TZ_Subunitati) and the chart of accounts (TZ_PlanCont).
' Plain data: the rules are in PYTHON/routes/avacont/subunitati.py and conturi.py.

''' <summary>Stable reason codes the accounts routes answer with (the client reacts to them).</summary>
Public NotInheritable Class ConturiReasons
    Private Sub New()
    End Sub

    ''' <summary>An analytic on a 401* / class 6 / class 7 account: ask the operator, then send the save again confirmed.</summary>
    Public Const ConfirmareAnalitic As String = "CONFIRMARE_ANALITIC"
End Class

''' <summary>
''' A subordinate unit with its own accounting. It decides the budget chapter and the sector+source of its
''' accounts, and the salary decimals (the old Access flag Zec2). Not linked to the VERCON subunits.
''' </summary>
Public NotInheritable Class SubunitateInfo
    ''' <summary>0 = a subunit not saved yet.</summary>
    Public Property IdSubunitate As Integer
    Public Property Denumire As String = String.Empty
    ''' <summary>Chapter as 65.01.</summary>
    Public Property Capitol As String = String.Empty
    ''' <summary>Sector + source as 01A.</summary>
    Public Property SursaSector As String = String.Empty
    ''' <summary>0 or 2: decimals of the salaries.</summary>
    Public Property Zecimale As Integer
    Public Property Implicita As Boolean
    Public Property Activ As Boolean = True
    ''' <summary>How many accounts its chart holds (read only).</summary>
    Public Property NrConturi As Integer
End Class

''' <summary>One full account of a chart: base account, classifications, analytic and the symbol the server composed.</summary>
Public NotInheritable Class ContPlan
    Public Property IdCont As Integer
    ''' <summary>The base account (7 digits).</summary>
    Public Property Cont As Integer
    ''' <summary>Sector + source of this account as 01A (follows the title bar choice; typed by the operator on «Toate»).</summary>
    Public Property SursaSector As String = String.Empty
    Public Property Analitic As String = String.Empty
    Public Property ClsfF As String = String.Empty
    Public Property ClsfE As String = String.Empty
    ''' <summary>Cont + sector/source + [ClsfF] + [ClsfE] + [.analitic]; written by the server.</summary>
    Public Property Simbol As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>A, B or P.</summary>
    Public Property Fel As String = String.Empty
    Public Property Extra As Boolean
    Public Property Activ As Boolean = True
End Class

''' <summary>What GET /api/avacont/conturi returns: the subunit asked for (Nothing = the unit has none) and its chart.</summary>
Public NotInheritable Class PlanConturiInfo
    Public Property Subunitate As SubunitateInfo
    Public Property Conturi As IReadOnlyList(Of ContPlan) = Array.Empty(Of ContPlan)()
End Class

''' <summary>A base account of the common nomenclator (AVACONT_COMUN.DefaCont).</summary>
Public NotInheritable Class ContNomenclator
    Public Property Simbol As Integer
    Public Property Denumire As String = String.Empty
    ''' <summary>A = asset, B = bifunctional, P = liability.</summary>
    Public Property Tip As String = String.Empty
    ''' <summary>True = the account must be split by the functional classification.</summary>
    Public Property CereClsfF As Boolean
    ''' <summary>True = the account must be split by the economic classification.</summary>
    Public Property CereClsfE As Boolean
End Class

''' <summary>A class of accounts (1 = capital ... 8 = off-balance).</summary>
Public NotInheritable Class ClasaCont
    Public Property Clasa As Integer
    Public Property Denumire As String = String.Empty
End Class

''' <summary>A functional or economic classification code with its name.</summary>
Public NotInheritable Class ClsfCont
    Public Property Cod As String = String.Empty
    Public Property Denumire As String = String.Empty
End Class

''' <summary>What the account editor offers: the classes, the base accounts and both classifications.</summary>
Public NotInheritable Class NomenclatorConturi
    Public Property Clase As IReadOnlyList(Of ClasaCont) = Array.Empty(Of ClasaCont)()
    Public Property Conturi As IReadOnlyList(Of ContNomenclator) = Array.Empty(Of ContNomenclator)()
    Public Property ClsfF As IReadOnlyList(Of ClsfCont) = Array.Empty(Of ClsfCont)()
    Public Property ClsfE As IReadOnlyList(Of ClsfCont) = Array.Empty(Of ClsfCont)()
    ''' <summary>Every sector+source (Cod = 01A ...), for the subunit editor.</summary>
    Public Property SurseSector As IReadOnlyList(Of ClsfCont) = Array.Empty(Of ClsfCont)()
End Class

''' <summary>An account to save. <see cref="IdCont"/> Nothing = a new account.</summary>
Public NotInheritable Class ContSave
    Public Property IdCont As Integer?
    Public Property IdSubunitate As Integer
    Public Property Cont As Integer
    ''' <summary>Sector + source of the account (01A); empty = the default of the subunit.</summary>
    Public Property SursaSector As String = String.Empty
    Public Property Analitic As String = String.Empty
    Public Property ClsfF As String = String.Empty
    Public Property ClsfE As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>A, B or P; empty = the type of the base account.</summary>
    Public Property Fel As String = String.Empty
    Public Property Extra As Boolean
    Public Property Activ As Boolean = True
End Class

''' <summary>The answer to saving an account: its number and the symbol the server composed.</summary>
Public NotInheritable Class ContSalvat
    Public Property IdCont As Integer
    Public Property Simbol As String = String.Empty
End Class

''' <summary>
''' The opening balance and the cumulated turnovers of one account for one year (TZ_SoldInitial, the row of the account itself).
''' An account with no stored row is zero everywhere.
''' </summary>
Public NotInheritable Class SoldCont
    Public Property IdCont As Integer
    Public Property SoldDebitor As Decimal
    Public Property SoldCreditor As Decimal
    Public Property RulajDebitor As Decimal
    Public Property RulajCreditor As Decimal

    ''' <summary>True when the four amounts are the same as in <paramref name="k_other"/>.</summary>
    Public Function SameAmounts(k_other As SoldCont) As Boolean
        If k_other Is Nothing Then Return SoldDebitor = 0D AndAlso SoldCreditor = 0D AndAlso RulajDebitor = 0D AndAlso RulajCreditor = 0D
        Return SoldDebitor = k_other.SoldDebitor AndAlso SoldCreditor = k_other.SoldCreditor AndAlso
               RulajDebitor = k_other.RulajDebitor AndAlso RulajCreditor = k_other.RulajCreditor
    End Function
End Class
