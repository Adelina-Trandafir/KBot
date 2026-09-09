Option Strict On
Imports System.Diagnostics
Imports System.IO
Imports System.Text

''' <summary>
''' The log of every message SHOWN to the operator: <c>&lt;AppDir&gt;\Logs\mesaje_operator.log</c>.
'''
''' <para>House rule, asked for by the operator on 08.09.2026: <b>anything put in front of the
''' operator must also reach a log</b>. A dialog lives until someone presses OK; after that
''' nobody can say what it said, and a report of the form "it told me something and it went
''' away" has nothing left to read. Here the exact text stays, with its caption and with the
''' place in the code that raised it.</para>
'''
''' <para>This does NOT replace <see cref="GlobalErrorLog"/> and does not overlap it:
''' GlobalErrorLog takes EXCEPTIONS (type, message, stack); this takes the very string a
''' person read. A defect usually writes to both -- the stack in one, the sentence in the
''' other -- and that is the intent: they answer different questions.</para>
'''
''' <para>One entry per line, read back by <c>OperatorLogParser</c>:</para>
''' <code>2026-09-08 20:06:09.255  [INFO ] [KBOT.DuLaIngestieAsync] «Titlu» the message</code>
'''
''' <para>TERMINAL sink, like <c>GlobalErrorLog</c> and <c>AdobeHostLog</c>: if even this file
''' cannot be written, the line goes to <see cref="Trace"/> and NOTHING is rethrown. A dialog
''' must never fail because logging did.</para>
''' </summary>
Public Module OperatorLog

    Private ReadOnly _gate As New Object()

    ''' <summary>The file name, next to the executable, under <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "mesaje_operator.log"

    ''' <summary>Writes one message shown to the operator. Never throws.</summary>
    ''' <param name="source">
    ''' Who raised it, as <c>Type.Method</c> -- the same convention as
    ''' <see cref="GlobalErrorLog.Write"/>, so the two logs read side by side.
    ''' </param>
    ''' <param name="title">The dialog caption (may be empty).</param>
    ''' <param name="message">The exact text the operator saw.</param>
    ''' <param name="level">
    ''' Severity, taken from the dialog's icon. It lands in the log viewer's level column, so a
    ''' warning can be filtered apart from an information box.
    ''' </param>
    Public Sub Write(source As String, title As String, message As String,
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
            If Not String.IsNullOrWhiteSpace(title) Then
                ' Guillemets, not the low quotes: VB reads a typographic opening quote as a
                ' string delimiter. Same convention as every other operator string here.
                sb.Append("«")
                sb.Append(title.Trim())
                sb.Append("» ")
            End If
            ' A multi-line message becomes ONE line: the format is one entry per line, and its
            ' wrapped rows are not new entries. Visible separators instead of the breaks, so the
            ' text stays readable in the viewer's grid.
            sb.Append(OneLine(message))
            sb.Append(Environment.NewLine)

            SyncLock _gate
                ' Rotation never throws: if it fails, the line is written anyway. See LogRotation.
                LogRotation.Roll(filePath)
                File.AppendAllText(filePath, sb.ToString(), New UTF8Encoding(True))
            End SyncLock
        Catch terminalEx As Exception
            ' TERMINAL sink: no rethrow -- the caller is about to show a dialog, and the dialog
            ' matters more than the log.
            Trace.WriteLine("OperatorLog terminal failure (" & If(source, "?") & "): " & terminalEx.Message)
        End Try
    End Sub

    ''' <summary>
    ''' The level tag, padded to 5 characters -- as in <c>TreeLogger</c>, so whatever follows it
    ''' always starts in the same column.
    ''' </summary>
    Private Function LevelTag(level As KBotLogLevel) As String
        Select Case level
            Case KBotLogLevel.Error : Return "ERR  "
            Case KBotLogLevel.Warn : Return "WARN "
            Case KBotLogLevel.Debug : Return "DEBUG"
            Case KBotLogLevel.Trace : Return "TRACE"
            Case Else : Return "INFO "
        End Select
    End Function

    ''' <summary>Line breaks become " · ", so the entry fits on one line.</summary>
    Private Function OneLine(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        Return text.Replace(vbCrLf, " · ").Replace(vbCr, " · ").Replace(vbLf, " · ").Trim()
    End Function

End Module
