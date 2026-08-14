Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Controls/UI, safe: deschide vizualizatorul de jurnale și cere verdict uman — singurul mod în care
' felia 0031-04 poate fi văzută pe ecran. Operatorul verifică lista de fișiere, jetoanele de nivel
' (inclusiv roșul de EROARE și chihlimbariul de AVERTISMENT), culorile rândurilor, panoul de
' detaliu și bara de stare, în toate cele trei scheme. NU e distructiv (butonul de golire cere el
' însuși două confirmări) și nu are nevoie de conexiune live: fișierele locale sunt de ajuns.
Public NotInheritable Class LogViewerTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Jurnale — vizualizatorul de jurnale"
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
        Dim launcher As ILogViewerLauncher
        Try
            launcher = context.GetService(Of ILogViewerLauncher)()
        Catch ex As Exception
            ' Fără shell nu există vizualizator: e o gazdă nepotrivită, nu un defect.
            Return Task.FromResult(HarnessTestResult.Skipped(
                "ILogViewerLauncher nu e înregistrat (vezi Program.vb): " & ex.Message))
        End Try

        Dim verdict As DialogResult
        Using f As Form = launcher.CreateLogViewer()
            context.Log("Verifică: fișierele din stânga, jetoanele de nivel, culorile rândurilor, " &
                        "panoul de detaliu (brut, neconvertit) și bara de stare. OK = arată corect.")
            verdict = f.ShowDialog()
        End Using

        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("vizualizator confirmat vizual"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("probă vizuală respinsă de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function

End Class
