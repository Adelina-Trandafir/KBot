#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.DevHarness

' Slice 0100-03: opens the queue-window playground (QueuePlaygroundForm) -- the real «Coada robotului» window
' over a simulated robot, with a property grid on each of its grids and columns, and a button that saves what was
' changed as designer lines. NOT destructive and needs no connection. Lives in KBot.App because the queue, the
' controller and the queue window do, and DevHarness cannot reference KBot.App.
Public NotInheritable Class QueuePlaygroundTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Coada robotului — playground coloane (descărcare multiplă simulată)"
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
            Using f As New QueuePlaygroundForm(AddressOf context.Log)
                verdict = f.ShowDialog()
            End Using
            Return Task.FromResult(If(verdict = DialogResult.OK,
                                      HarnessTestResult.Passed("playground închis"),
                                      HarnessTestResult.Skipped("închis fără verdict")))
        Catch ex As Exception
            GlobalErrorLog.Write("QueuePlaygroundTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
