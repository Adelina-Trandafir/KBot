Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Text

''' <summary>
''' The whole road of a FOREXE page picture (operator, 28.09.2026):
''' <c>&lt;AppDir&gt;\Logs\capturi_forexe.log</c>.
'''
''' <para>Why a file of its own: the FOREXE console is not kept anywhere, and on 28.09.2026
''' pictures taken on a client's PC never reached the server with nothing left to say why.
''' Every step writes one line here - the marker the page was given, the picture taken or
''' not, the ingest that did or did not save, every upload with its answer, every picture
''' left on disk and why, the operator's answer to the delete question. Read it from the
''' Setări log viewer like any other file under <c>Logs\</c>.</para>
'''
''' <para>Same line shape as <see cref="OperatorLog"/>:</para>
''' <code>2026-09-28 20:06:09.255  [INFO ] [MainForm.TrimiteCapturileAsync] message</code>
'''
''' <para>TERMINAL sink: if the file cannot be written the line goes to <see cref="Trace"/>
''' and nothing is thrown.</para>
''' </summary>
Public Module CapturiLog

    Private ReadOnly _gate As New Object()

    ''' <summary>The file name, next to the executable, under <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "capturi_forexe.log"

    ''' <summary>Writes one line. Never throws.</summary>
    Public Sub Write(source As String, message As String,
                     Optional level As KBotLogLevel = KBotLogLevel.Info)
        Try
            LogPaths.EnsureLogsDirectory()
            Dim filePath As String = LogPaths.Combine(FileNameOnly)

            Dim sb As New StringBuilder()
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
            sb.Append("  [")
            sb.Append(LevelTag(level))
            sb.Append("] [")
            sb.Append(If(source, String.Empty).Trim())
            sb.Append("] ")
            sb.Append(OneLine(message))
            sb.Append(Environment.NewLine)

            SyncLock _gate
                LogRotation.Roll(filePath)
                File.AppendAllText(filePath, sb.ToString(), New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            Trace.WriteLine("CapturiLog terminal failure (" & If(source, "?") & "): " & terminalEx.Message)
        End Try
    End Sub

    Private Function LevelTag(level As KBotLogLevel) As String
        Select Case level
            Case KBotLogLevel.Error : Return "ERR  "
            Case KBotLogLevel.Warn : Return "WARN "
            Case KBotLogLevel.Debug : Return "DEBUG"
            Case KBotLogLevel.Trace : Return "TRACE"
            Case Else : Return "INFO "
        End Select
    End Function

    Private Function OneLine(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Return text.Replace(vbCrLf, " · ").Replace(vbCr, " · ").Replace(vbLf, " · ").Trim()
    End Function

End Module
