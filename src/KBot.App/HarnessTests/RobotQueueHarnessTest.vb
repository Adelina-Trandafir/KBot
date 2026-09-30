#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.DevHarness

' Slice 0098: opens the robot queue bench (RobotQueueHarnessForm) -- the real queue, gate and
' queue window over a simulated FOREXE robot and a simulated server. NOT destructive and needs no
' connection: nothing leaves the PC. Lives in KBot.App because the queue, the controller and the
' queue window do, and DevHarness cannot reference KBot.App.
Public NotInheritable Class RobotQueueHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Coada robotului + poarta serverului (FOREXE și server simulate)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "FOREXE"
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

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Try
            Dim verdict As DialogResult
            Using f As New RobotQueueHarnessForm(AddressOf context.Log)
                verdict = f.ShowDialog()
            End Using
            Select Case verdict
                Case DialogResult.Yes
                    Return Task.FromResult(HarnessTestResult.Passed("coada și poarta s-au purtat cum trebuie"))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueHarnessTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
