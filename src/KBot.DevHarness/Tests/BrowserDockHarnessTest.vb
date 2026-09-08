Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

' FOREXE, live: opens a real Chromium window, navigates to the FOREXE public page, docks
' the window into a panel and runs the real Wicket monitor against it. No certificate and
' no token - the point is the window, not the session - so it works on any machine.
Public NotInheritable Class BrowserDockHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FOREXE — Andocare browser + monitor Wicket"
        End Get
    End Property

    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "FOREXE"
        End Get
    End Property

    ' Live: it opens a browser and loads a page off the internet.
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Dim verdict As DialogResult
        Using f As New BrowserDockHarnessForm(AddressOf context.Log)
            verdict = f.ShowDialog()
        End Using

        Select Case verdict
            Case DialogResult.OK
                Return Task.FromResult(HarnessTestResult.Passed("andocare confirmată vizual"))
            Case DialogResult.Cancel
                Return Task.FromResult(HarnessTestResult.Failed("andocare respinsă de operator"))
            Case Else
                Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
        End Select
    End Function
End Class
