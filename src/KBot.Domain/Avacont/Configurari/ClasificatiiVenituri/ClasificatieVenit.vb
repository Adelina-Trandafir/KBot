Option Strict On
Imports System.Collections.Generic

''' <summary>One revenue classification of the database (slice AV01-11, <c>Clasificatii_Venituri</c>): chapter, sub-chapter and paragraph, each of two characters.</summary>
Public NotInheritable Class ClasificatieVenit
    Public Property IdClsfV As Integer
    ''' <summary>The sector+source the classification belongs to (slice AV01-12).</summary>
    Public Property Ss As String = String.Empty
    Public Property Capitol As String = String.Empty
    Public Property Subcapitol As String = String.Empty
    Public Property Paragraf As String = String.Empty
    Public Property Denumire As String = String.Empty

    ''' <summary>«CC.SS.PP», the code the operator reads.</summary>
    Public ReadOnly Property Cod As String
        Get
            Return $"{Capitol}.{Subcapitol}.{Paragraf}"
        End Get
    End Property
End Class

''' <summary>The revenue classifications and the names of the two upper levels of the tree (from the nomenclator <c>DefaClsfV</c>).</summary>
Public NotInheritable Class ClasificatiiVenituriCatalog
    Public Property Items As IReadOnlyList(Of ClasificatieVenit) = New List(Of ClasificatieVenit)()
    ''' <summary>Chapter names by the two-character chapter: «01» -&gt; name.</summary>
    Public Property CapitolNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>Sub-chapter names by chapter + sub-chapter (four characters): «0101» -&gt; name.</summary>
    Public Property SubcapitolNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
End Class

''' <summary>One revenue code of the nomenclator (<c>DefaClsfV</c>, type VN): what the «add» window offers.</summary>
Public NotInheritable Class CodVenitNomenclator
    ''' <summary>The six-character code (chapter + sub-chapter + paragraph).</summary>
    Public Property ClsfV As String = String.Empty
    Public Property Capitol As String = String.Empty
    Public Property Subcapitol As String = String.Empty
    Public Property Paragraf As String = String.Empty
    Public Property Denumire As String = String.Empty
    ''' <summary>True = the classification is already in the database (the window does not offer it again).</summary>
    Public Property Exista As Boolean
End Class

''' <summary>
''' The budget summary of the unit the page shows, and whether the title bar's source has CHILD subunits. A child has the same sector+source as its parent, so it
''' works on the parent's data (one unit): nothing is blocked, the page only tells the operator that the work goes straight into the parent's data
''' (<see cref="Child"/>).
''' </summary>
Public NotInheritable Class VenitSumarUnitate
    Public Property Rows As IReadOnlyList(Of VenitBugetSumar) = New List(Of VenitBugetSumar)()
    ''' <summary>The child subunits of the source and their parent; Nothing = the source has no child subunit.</summary>
    Public Property Child As ChildSourceInfo
End Class

''' <summary>
''' What the budget grids show for a node above the leaves of the revenue tree: per classification, its LAST budget version of the
''' year and the TOTAL of the year's corrections (quarter by quarter), for one unit.
''' </summary>
Public NotInheritable Class VenitBugetSumar
    Public Property IdClsfV As Integer
    ''' <summary>Nothing = the classification has no budget version in the year.</summary>
    Public Property LastBudget As BudgetVersion
    ''' <summary>Nothing = the classification has no correction in the year.</summary>
    Public Property CorrectionsTotal As QuarterAmounts
End Class
