#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.DevHarness

' Adobe/PDF: opens the chain bench (DdfFluxServerHarnessForm) -- A signed, uploaded, Adobe closed, local copy deleted;
' then B from the server's A; then the director from the server's A+B. NOT destructive: every upload goes to the
' bench table (KBOT_BANC_PDF, only in 000_DEMO), the real DDF of the revision is never written. Lives in KBot.App
' because the bench drives ReaderHostPreview and DdfPdfGenerator, which DevHarness cannot reference.
Public NotInheritable Class DdfFluxServerHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "DDF — flux A → B → director cu serverul între pași (tabela de probă)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Adobe/PDF"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            ' The bench logs in by itself.
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
            Using f As New DdfFluxServerHarnessForm(context.GetService(Of IApiClient)(),
                                                    context.GetService(Of SessionContext)(),
                                                    context.GetService(Of Func(Of LoginForm))(),
                                                    AddressOf context.Log)
                verdict = f.ShowDialog()
            End Using
            Select Case verdict
                Case DialogResult.Yes
                    Return Task.FromResult(HarnessTestResult.Passed("A, B și directorul au trecut prin server"))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFluxServerHarnessTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
