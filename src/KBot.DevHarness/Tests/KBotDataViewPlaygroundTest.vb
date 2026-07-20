Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: deschide «playground-ul» KBotDataView — o grilă însoțită de un panou care
' expune LIVE fiecare proprietate comutabilă la runtime (mod auto-size, mod de umplere, eșantion,
' coloane înghețate, înălțimi, antet, rânduri alternante, read-only) plus un inspector de coloană
' (Visible / Enabled / ReadOnly / Width / MinWidth / MaxWidth). NU e distructiv, nu cere live.
Public NotInheritable Class KBotDataViewPlaygroundTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "KBotDataView — proprietăți runtime (playground)"
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
        Using f As New DataViewPlaygroundForm(AddressOf context.Log)
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
