Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: deschide «playground-ul» AdvancedTreeControl — un arbore însoțit de un
' panou care expune LIVE fiecare proprietate comutabilă la runtime: antet (vizibil, caption,
' înălțime, iconițe), banda de căutare (SearchShow, ✕, etichetă, placeholder, font, SearchIn/
' SearchType, culori), geometria arborelui (înălțime rând, indentare, expander, checkbox,
' radio, iconițe, scrollbar) și tooltip-ul. NU e distructiv, nu cere live.
Public NotInheritable Class AdvancedTreePlaygroundTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "AdvancedTreeControl — proprietăți runtime (playground)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Controls/UI"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Dim verdict As DialogResult
        Using f As New TreePlaygroundForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("proprietăți runtime OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("playground respins de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
