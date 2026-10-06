#If DEBUG Then
Option Strict On
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.DevHarness

' Adobe/PDF, not destructive: opens the older-Adobe bench of slice 0078-12
' (OldAdobeHostHarnessForm) -- the hosted Adobe window alone, with the script-error monitor. Nothing
' reaches the server (there is no signing session); the PDF is copied into Temp\PDF\Banc first.
' Lives in KBot.App because the bench uses the application's own settings preface
' (AdobeUiPreference) and the «Mesaje de script» dialog, which DevHarness cannot reference.
Public NotInheritable Class OldAdobeHostHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Fereastră găzduită — Adobe vechi (< 2024): monitor erori JS"
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
            Dim verdict As DialogResult
            Using f As New OldAdobeHostHarnessForm(AddressOf context.Log)
                verdict = f.ShowDialog()
            End Using
            Select Case verdict
                Case DialogResult.Yes
                    Return Task.FromResult(HarnessTestResult.Passed("fereastra găzduită cu Adobe vechi: OK"))
                Case DialogResult.No
                    Return Task.FromResult(HarnessTestResult.Failed("respins de operator"))
                Case Else
                    Return Task.FromResult(HarnessTestResult.Skipped("închis fără verdict"))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("OldAdobeHostHarnessTest.RunAsync", ex)
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        End Try
    End Function
End Class
#End If
