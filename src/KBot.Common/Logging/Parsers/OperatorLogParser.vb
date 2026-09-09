Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Parser for <c>mesaje_operator.log</c>, written by <c>OperatorLog.Write</c>:
''' <code>2026-09-08 20:06:09.255  [INFO ] [KBOT.DuLaIngestieAsync] «Titlu» the message</code>
'''
''' Three things checked in the writer, not assumed:
''' <list type="bullet">
''' <item>the timestamp and the rest are separated by TWO spaces, as in <c>AdobeHostLog</c>;</item>
''' <item>the level is padded to 5 characters (<c>INFO </c>, <c>WARN </c>, <c>ERR  </c>), so it
''' is compared TRIMMED, not literally;</item>
''' <item>the guillemet-wrapped caption is absent entirely when the dialog had none -- it is
''' never an empty pair.</item>
''' </list>
'''
''' <para>The caption stays inside <c>Message</c>, where the writer put it: it is part of what
''' the operator read, and the <c>Source</c> column is reserved for the place in the code.</para>
''' </summary>
Public NotInheritable Class OperatorLogParser
    Implements ILogEntryParser

    Private Shared ReadOnly _rx As New Regex(
        "^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s{2}\[(?<lvl>[^\]]*)\]\s\[(?<src>[^\]]*)\]\s(?<msg>.*)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "OperatorLog"
        End Get
    End Property

    ''' <summary>One entry per line -- the writer folds multi-line messages.</summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return True
        End Get
    End Property

    Public Function TryParseHeader(line As String, ByRef result As LogEntry) As Boolean Implements ILogEntryParser.TryParseHeader
        If String.IsNullOrEmpty(line) Then Return False

        Dim m As Match = _rx.Match(line)
        If Not m.Success Then Return False

        Dim stamp As Date
        If Not Date.TryParseExact(m.Groups("ts").Value, "yyyy-MM-dd HH:mm:ss.fff",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, stamp) Then
            Return False
        End If

        result = New LogEntry(stamp,
                              ParseLevel(m.Groups("lvl").Value),
                              m.Groups("src").Value.Trim(),
                              m.Groups("msg").Value.Trim(),
                              line)
        Return True
    End Function

    ''' <summary>The padded tag, read trimmed. Anything unknown is Info.</summary>
    Private Shared Function ParseLevel(tag As String) As KBotLogLevel
        Select Case If(tag, String.Empty).Trim().ToUpperInvariant()
            Case "ERR", "ERROR" : Return KBotLogLevel.Error
            Case "WARN" : Return KBotLogLevel.Warn
            Case "DEBUG" : Return KBotLogLevel.Debug
            Case "TRACE" : Return KBotLogLevel.Trace
            Case Else : Return KBotLogLevel.Info
        End Select
    End Function

End Class
