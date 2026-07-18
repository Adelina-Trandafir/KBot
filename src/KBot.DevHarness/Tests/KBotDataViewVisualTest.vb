Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: deschide proba vizuală a KBotDataView (5.000 × 20 sintetic) și cere
' verdict uman. Operatorul verifică: derularea e fluidă pe ambele axe, prima coloană rămâne
' înghețată, rândurile alternante + bifele se văd corect, iar comutarea temei re-pictează
' grila. NU e distructiv, nu cere conexiune live.
Public NotInheritable Class KBotDataViewVisualTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "KBotDataView — virtualizare + temă (5.000 × 20)"
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
        Using f As New DataViewHarnessForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using
        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("derulare + temă OK"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("probă vizuală respinsă de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
