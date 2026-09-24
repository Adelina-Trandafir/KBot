#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.DevHarness

' Adobe/PDF, destructive: opens the signing bench of slice 0078 (PdfSigningHarnessForm). With
' «Încarcă pe server» ticked a signature REALLY goes to the server for the id typed in, and writes
' FX_DDF_REV.Semnatura / FX_ORD.Semnatura -- hence IsDestructive. Lives in KBot.App because the
' bench drives ReaderHostPreview and PdfSigningSession, which DevHarness cannot reference.
Public NotInheritable Class PdfSigningHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Semnare PDF — capcana «Salvare ca», încărcare pe server, recitire"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Adobe/PDF"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            ' The bench logs in by itself; without a login it still tests the trap alone.
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return True
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Try
            Dim verdict As DialogResult
            Using f As New PdfSigningHarnessForm(context.GetService(Of IApiClient)(),
                                                 context.GetService(Of SessionContext)(),
                                                 context.GetService(Of Func(Of LoginForm))(),
                                                 AddressOf context.Log)
                verdict = f.ShowDialog()
            End Using
            Select Case verdict
                Case DialogResult.Yes
                    Return Task.FromResult(HarnessTestResult.Passed("semnare + încărcare + recitire OK"))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PdfSigningHarnessTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
