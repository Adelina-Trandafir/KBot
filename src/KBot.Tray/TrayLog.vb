Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' The tray's own journal (slice TRAY-02): what it did and why, one line per step, in <c>&lt;AppDir&gt;\Logs\tray.log</c> (next to
''' <c>harness_errors.log</c>, which keeps the exceptions). It exists so that a check, a sign-in or a window can be followed when there
''' is nothing to sign. Never a password, a token or an e-mail address: roles, units and counts only. A terminal sink: if it cannot be
''' written, the failure goes to the trace and the program goes on.
''' </summary>
Friend NotInheritable Class TrayLog

    Private Sub New()
    End Sub

    Friend Const FileNameOnly As String = "tray.log"

    Private Shared ReadOnly Gate As New Object()

    ''' <summary>Adds one line: time, <paramref name="k_source"/> in brackets, then the message.</summary>
    Friend Shared Sub Write(k_source As String, k_message As String)
        Try
            LogPaths.EnsureLogsDirectory()
            Dim k_path As String = LogPaths.Combine(FileNameOnly)
            Dim k_line As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  [" & k_source & "] " & k_message & Environment.NewLine
            SyncLock Gate
                LogRotation.Roll(k_path)
                File.AppendAllText(k_path, k_line, New UTF8Encoding(True))
            End SyncLock
        Catch ex As Exception
            Trace.WriteLine("TrayLog failure (" & k_source & "): " & ex.Message)
        End Try
    End Sub
End Class
