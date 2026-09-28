Option Strict On
Imports System.Globalization
Imports System.Text.RegularExpressions

''' <summary>
''' Parser for the server's <c>forexe_timing.log</c> (slice 0089), as handed back by
''' <c>GET /api/logs/timing</c>. One entry = one BLOCK, one block per timed request
''' (<c>PYTHON/utils/timing.py</c>, <c>_write</c>):
''' <code>2026-09-28 14:19:45.476  rulare 10  [prelucrare]  status 200</code>
''' followed by the notes line, the stage table, the SQL shapes and the <c>SUMAR</c> line --
''' all continuation lines. The route leaves out the <c>====</c> separators, so the header is
''' the only thing that opens a block.
'''
''' <para><b>Level from the HTTP status</b>, because the file has no level column: 2xx is Info,
''' 4xx is Warn, 5xx and <c>EXC ...</c> (the route raised) are Error. That is what lets the level
''' chips split the timing journal the same way they split the others.</para>
'''
''' <para>The timestamp is the server's local time with no offset, like the legacy
''' <c>api_server.log</c> lines, so it gets the <c>ServerClock</c> correction.</para>
''' </summary>
Public NotInheritable Class ForexeTimingParser
    Implements ILogEntryParser

    Private Shared ReadOnly _rx As New Regex(
        "^(?<ts>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}\.\d{3})\s+rulare\s+(?<run>\d+)\s+\[(?<label>[^\]]*)\]\s+status\s+(?<status>.+)$",
        RegexOptions.Compiled Or RegexOptions.CultureInvariant)

    Public ReadOnly Property Name As String Implements ILogEntryParser.Name
        Get
            Return "ForexeTiming"
        End Get
    End Property

    ''' <summary>A block spans twenty-odd lines: only the probe's «at least one header» applies.</summary>
    Public ReadOnly Property ExpectsHeaderOnEveryLine As Boolean Implements ILogEntryParser.ExpectsHeaderOnEveryLine
        Get
            Return False
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

        Dim status As String = m.Groups("status").Value.Trim()
        result = New LogEntry(stamp,
                              LevelFromStatus(status),
                              m.Groups("label").Value.Trim(),
                              "rulare " & m.Groups("run").Value & " status " & status,
                              line)
        result.Origin = LogOrigin.Server
        result.TimestampNeedsClockCorrection = True
        Return True
    End Function

    Private Shared Function LevelFromStatus(status As String) As KBotLogLevel
        If status.StartsWith("5", StringComparison.Ordinal) OrElse
           status.StartsWith("EXC", StringComparison.Ordinal) Then Return KBotLogLevel.Error
        If status.StartsWith("4", StringComparison.Ordinal) Then Return KBotLogLevel.Warn
        Return KBotLogLevel.Info
    End Function

End Class
