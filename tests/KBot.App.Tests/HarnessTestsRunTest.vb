#If DEBUG Then
Option Strict On
Imports System
Imports System.Threading
Imports Xunit
Imports KBot.App
Imports KBot.DevHarness

' Actually RUNS the three IHarnessTest classes added this session and asserts each
' returns Passed — the same objects the Dev Harness tree discovers/executes. Runs on a
' dedicated STA thread because the list-mode test creates a Form and calls DrawToBitmap.
' Debug-only: the harness types (KBot.DevHarness) are referenced by KBot.App only on Debug.
Public Class HarnessTestsRunTest

    Private NotInheritable Class NullProvider
        Implements IServiceProvider
        Public Function GetService(serviceType As Type) As Object Implements IServiceProvider.GetService
            Return Nothing
        End Function
    End Class

    Private Shared Sub NoOpLog(message As String)
    End Sub

    Private Shared Sub NoOpProgress(info As HarnessProgressInfo)
    End Sub

    Private Shared Function RunOnSta(test As IHarnessTest) As HarnessTestResult
        Dim result As HarnessTestResult = Nothing
        Dim err As Exception = Nothing
        Dim th As New Thread(
            Sub()
                Try
                    Dim ctx As New HarnessContext(New NullProvider(),
                                                  AddressOf NoOpLog,
                                                  New Progress(Of HarnessProgressInfo)(AddressOf NoOpProgress))
                    result = test.RunAsync(ctx, CancellationToken.None).GetAwaiter().GetResult()
                Catch e As Exception
                    err = e
                End Try
            End Sub)
        th.SetApartmentState(ApartmentState.STA)
        th.IsBackground = True
        th.Start()
        th.Join()
        If err IsNot Nothing Then Throw err
        Return result
    End Function

    <Fact>
    Public Sub FxIconsHarnessTest_Passes()
        Dim r = RunOnSta(New FxIconsHarnessTest())
        Assert.Equal(HarnessTestOutcome.Passed, r.Outcome)
    End Sub

    <Fact>
    Public Sub AngajamenteListModeTest_Passes()
        Dim r = RunOnSta(New AngajamenteListModeTest())
        Assert.Equal(HarnessTestOutcome.Passed, r.Outcome)
    End Sub

    <Fact>
    Public Sub GetAngajamenteApiClientTest_Passes()
        Dim r = RunOnSta(New GetAngajamenteApiClientTest())
        Assert.Equal(HarnessTestOutcome.Passed, r.Outcome)
    End Sub
End Class
#End If
