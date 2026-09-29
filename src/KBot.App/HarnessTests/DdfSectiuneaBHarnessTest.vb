#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.DevHarness

' Adobe/PDF: opens the section B bench of slice 0078-04 (DdfSectiuneaBHarnessForm). NOT
' destructive: the server is only read; every file stays in Temp\PDF. Lives in KBot.App because
' the bench drives ReaderHostPreview and DdfPdfGenerator, which DevHarness cannot reference.
Public NotInheritable Class DdfSectiuneaBHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "DDF — Secțiunea A semnată, apoi Secțiunea B inserată și semnată (doar local)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Adobe/PDF"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            ' The bench logs in by itself; a saved PDF can be reopened without a login.
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
            Using f As New DdfSectiuneaBHarnessForm(context.GetService(Of IApiClient)(),
                                                    context.GetService(Of SessionContext)(),
                                                    context.GetService(Of Func(Of LoginForm))(),
                                                    AddressOf context.Log)
                verdict = f.ShowDialog()
            End Using
            Select Case verdict
                Case DialogResult.Yes
                    Return Task.FromResult(HarnessTestResult.Passed("A semnată, B inserată și semnată"))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("DdfSectiuneaBHarnessTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
