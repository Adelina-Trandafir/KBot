Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports KBot.Common

''' <summary>
''' Starts K-BOT for the tray. The request (what to open) goes into <c>tray_request.json</c> first, then
''' <c>KBot.App.exe</c> is started: a K-BOT that is not running reads the file once its window is up; one that is already
''' running is only brought forward by its single-instance guard and reads the file through its watcher.
''' </summary>
Friend NotInheritable Class KBotLauncher

    Private Sub New()
    End Sub

    Friend Const AppExeName As String = "KBot.App.exe"

    ''' <summary>
    ''' Opens the list of documents waiting for the signature, with the revision picked when <paramref name="k_idrev"/> is not
    ''' 0. I/O boundary: logs and rethrows.
    ''' </summary>
    Friend Shared Sub OpenToSign(k_appDir As String, k_dc As String, k_idrev As Integer)
        Try
            TrayRequest.Write(New TrayRequest() With {.Action = TrayRequest.ActionSign, .Dc = k_dc, .Idrev = k_idrev})
            Start(k_appDir)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLauncher.OpenToSign", ex)
            Throw
        End Try
    End Sub

    ''' <summary>Opens K-BOT's main window (for the roles that sign inside the main window). I/O boundary: logs and rethrows.</summary>
    Friend Shared Sub OpenMain(k_appDir As String)
        Try
            TrayRequest.Write(New TrayRequest() With {.Action = TrayRequest.ActionMain})
            Start(k_appDir)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotLauncher.OpenMain", ex)
            Throw
        End Try
    End Sub

    Private Shared Sub Start(k_appDir As String)
        Dim k_exe As String = Path.Combine(k_appDir, AppExeName)
        If Not File.Exists(k_exe) Then
            Throw New FileNotFoundException("K-BOT nu a fost găsit lângă aplicația din systray.", k_exe)
        End If
        Process.Start(New ProcessStartInfo(k_exe) With {.UseShellExecute = False, .WorkingDirectory = k_appDir})?.Dispose()
    End Sub
End Class
