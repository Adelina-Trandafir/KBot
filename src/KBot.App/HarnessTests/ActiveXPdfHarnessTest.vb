#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.DevHarness

' Adobe/PDF, not destructive: opens the ActiveX bench of slice 0078-15 (ActiveXPdfHarnessForm) -- the
' AcroPDF control alone, loading the PDF the operator picks. Nothing is copied, nothing reaches the server.
Public NotInheritable Class ActiveXPdfHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "ActiveX — doar deschiderea unui PDF"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Adobe/PDF"
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
            Dim k_verdict As DialogResult
            Using f As New ActiveXPdfHarnessForm()
                k_verdict = f.ShowDialog()
            End Using
            Select Case k_verdict
                Case DialogResult.Yes
                    Return Task.FromResult(HarnessTestResult.Passed("deschiderea în ActiveX: OK"))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("ActiveXPdfHarnessTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class

' Adobe/PDF, not destructive: the viewer of activex_check.log on its own (the ActiveX bench opens it too).
Public NotInheritable Class ActivexLogViewerTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "ActiveX — vizualizator activex_check.log"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Adobe/PDF"
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
            Using f As New ActivexLogViewerForm()
                f.ShowDialog()
            End Using
            Return Task.FromResult(HarnessTestResult.Skipped("vizualizator închis"))
        Catch ex As Exception
            GlobalErrorLog.Write("ActivexLogViewerTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
