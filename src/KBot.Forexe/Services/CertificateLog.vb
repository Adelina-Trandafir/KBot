Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' Working log of the certificate picker: <c>&lt;AppDir&gt;\Logs\certificate_filter.log</c>.
'''
''' One line per certificate the store offered, accepted or rejected, with the rule that decided
''' it. Rejections are not errors, so they never reach <see cref="GlobalErrorLog"/>; without this
''' file a certificate the browser shows but the picker does not leaves nothing to read.
'''
''' Terminal sink, like <c>GlobalErrorLog</c>: if the file cannot be written the line goes to
''' <see cref="Trace"/> and nothing is thrown.
''' </summary>
Public Module CertificateLog

    Private ReadOnly _gate As New Object()

    ''' <summary>File name, next to the executable, under <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "certificate_filter.log"

    ''' <summary>Writes one time-stamped line. Never throws.</summary>
    Public Sub Write(line As String)
        Try
            LogPaths.EnsureLogsDirectory()
            Dim filePath As String = LogPaths.Combine(FileNameOnly)
            SyncLock _gate
                LogRotation.Roll(filePath)
                File.AppendAllText(filePath,
                                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "  " & line & Environment.NewLine,
                                   New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            Trace.WriteLine("CertificateLog terminal failure: " & terminalEx.Message)
        End Try
    End Sub

End Module
