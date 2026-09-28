Option Strict On
Imports System.Collections.Generic
Imports System.IO

''' <summary>What came out of loading one log file.</summary>
Public NotInheritable Class LogLoadResult

    Public Sub New(entries As IReadOnlyList(Of LogEntry),
                   parserName As String,
                   parserWasGuessCorrect As Boolean,
                   inheritedTimestampCount As Integer,
                   withoutTimestampCount As Integer,
                   wasTruncated As Boolean,
                   fileLengthBytes As Long)
        Me.Entries = entries
        Me.ParserName = parserName
        Me.ParserWasGuessCorrect = parserWasGuessCorrect
        Me.InheritedTimestampCount = inheritedTimestampCount
        Me.WithoutTimestampCount = withoutTimestampCount
        Me.WasTruncated = wasTruncated
        Me.FileLengthBytes = fileLengthBytes
    End Sub

    Public ReadOnly Property Entries As IReadOnlyList(Of LogEntry)

    ''' <summary>The parser that WON, not the one guessed from the name.</summary>
    Public ReadOnly Property ParserName As String

    ''' <summary>
    ''' False if the guess from the file name failed the probe and loading fell back to choosing
    ''' per line. Worth showing: it means a file does not have the format its name promises.
    ''' </summary>
    Public ReadOnly Property ParserWasGuessCorrect As Boolean

    ''' <summary>How many entries took their timestamp from the entry before them.</summary>
    Public ReadOnly Property InheritedTimestampCount As Integer

    ''' <summary>How many entries were left WITHOUT a timestamp -- the date range filter excludes them.</summary>
    Public ReadOnly Property WithoutTimestampCount As Integer

    Public ReadOnly Property WasTruncated As Boolean
    Public ReadOnly Property FileLengthBytes As Long

End Class

''' <summary>
''' Picks the parser, stitches continuation lines into blocks and fills in missing timestamps.
'''
''' <para><b>The guess from the name is CHECKED.</b> A wrong parser raises no error -- it gives a
''' grid full of <c>Unknown</c> rows, exactly the kind of defect that eats an afternoon. So the
''' guessed parser is run over the first non-empty lines and, if it recognises too few of them as
''' headers, loading falls back to choosing per line and REPORTS who actually won.</para>
''' </summary>
Public Module LogFileLoader

    ''' <summary>How many non-empty lines are used as the probe for the parser guess.</summary>
    Public Const ProbeLineCount As Integer = 50

    ''' <summary>Below this share of recognised headers, the guess from the name is considered wrong.</summary>
    Public Const MinimumHeaderRatio As Double = 0.3

    ''' <summary>
    ''' Loads a file from disk: reads the tail, picks the parser, builds the entries.
    ''' Throws on I/O -- see <c>LogFileReader.ReadTail</c>.
    ''' </summary>
    Public Function LoadFile(filePath As String,
                             Optional windowBytes As Long = LogFileReader.DefaultWindowBytes) As LogLoadResult
        Dim read As LogReadResult = LogFileReader.ReadTail(filePath, windowBytes)
        Dim fileName As String = Path.GetFileName(filePath)

        ' The file date: the only source of a date for the TreeLogger format, which writes the time only.
        Dim fileDate As Date = Date.Today
        Try
            Dim info As New FileInfo(filePath)
            If info.Exists Then fileDate = info.LastWriteTime
        Catch ex As IOException
            ' The date could not be read: today stays. Not worth stopping the load for, but not
            ' hidden either -- TreeLogger entries will carry an approximate date.
            Diagnostics.Trace.WriteLine("LogFileLoader: could not read the date of " & filePath & ": " & ex.Message)
        End Try

        Return LoadText(read.Text, fileName, fileDate, LogOrigin.Client, read.WasTruncated, read.FileLengthBytes)
    End Function

    ''' <summary>
    ''' Loads from text already read -- the path for server logs (brought by the API) and for
    ''' tests. Pure: never touches the disk.
    ''' </summary>
    Public Function LoadText(text As String,
                             fileName As String,
                             fileDate As Date,
                             origin As LogOrigin,
                             Optional wasTruncated As Boolean = False,
                             Optional fileLengthBytes As Long = 0L) As LogLoadResult
        Dim lines As String() = SplitLines(If(text, String.Empty))
        Dim candidates As List(Of ILogEntryParser) = BuildParsers(fileDate)

        Dim guessed As ILogEntryParser = GuessByFileName(fileName, candidates)
        Dim guessCorrect As Boolean = True
        Dim chosen As ILogEntryParser = guessed

        If guessed IsNot Nothing AndAlso Not GuessSurvivesProbe(guessed, lines) Then
            guessCorrect = False
            chosen = Nothing        ' per line: try every parser, in order
        ElseIf guessed Is Nothing Then
            chosen = Nothing
        End If

        Dim entries As New List(Of LogEntry)()
        Dim winner As String = If(chosen IsNot Nothing, chosen.Name, String.Empty)

        For i As Integer = 0 To lines.Length - 1
            Dim line As String = lines(i)

            ' An empty line is ALWAYS a continuation. It never opens an entry -- otherwise the
            ' empty line GlobalErrorLog puts after every block would become a row in the grid.
            If String.IsNullOrWhiteSpace(line) Then
                If entries.Count > 0 Then entries(entries.Count - 1).AppendRawLine(line)
                Continue For
            End If

            Dim parsed As LogEntry = Nothing
            Dim usedParser As String = String.Empty

            If chosen IsNot Nothing Then
                If chosen.TryParseHeader(line, parsed) Then usedParser = chosen.Name
            Else
                For Each p As ILogEntryParser In candidates
                    Dim attempt As LogEntry = Nothing
                    If p.TryParseHeader(line, attempt) Then
                        parsed = attempt
                        usedParser = p.Name
                        Exit For
                    End If
                Next
            End If

            If parsed IsNot Nothing Then
                parsed.FileName = fileName
                parsed.LineNumber = i + 1
                ' Server parsers set Origin themselves; for the rest the caller decides.
                If parsed.Origin <> LogOrigin.Server Then parsed.Origin = origin
                entries.Add(parsed)
                If String.IsNullOrEmpty(winner) Then winner = usedParser
            ElseIf entries.Count > 0 Then
                ' Continuation: goes into the block before. It reaches Message too, but ONLY if
                ' the message is still empty -- the GlobalErrorLog header case, whose real message
                ' is the first line of ex.ToString().
                Dim last As LogEntry = entries(entries.Count - 1)
                last.AppendRawLine(line)
                If String.IsNullOrEmpty(last.Message) Then last.Message = line.Trim()
            Else
                ' A continuation with nothing before it: the read window cut the block it belonged
                ' to. It becomes its own unrecognised entry -- never an exception.
                Dim orphan As New LogEntry(Nothing, KBotLogLevel.Unknown, String.Empty, line.Trim(), line) With {
                    .FileName = fileName,
                    .LineNumber = i + 1,
                    .Origin = origin}
                entries.Add(orphan)
            End If
        Next

        ' Inherited timestamps: an entry without a date takes the one of the entry before it. The
        ' test bench run file is the base case -- a whole run has a single date.
        Dim inherited As Integer = 0
        Dim without As Integer = 0
        Dim running As Date? = Nothing
        For Each e As LogEntry In entries
            If e.Timestamp.HasValue Then
                running = e.Timestamp
            ElseIf running.HasValue Then
                e.Timestamp = running
                e.TimestampInherited = True
                inherited += 1
            Else
                without += 1
            End If
        Next

        If String.IsNullOrEmpty(winner) Then winner = "Fallback"
        Return New LogLoadResult(entries, winner, guessCorrect, inherited, without, wasTruncated, fileLengthBytes)
    End Function

    ''' <summary>The available parsers, in the order they are tried when choosing per line.</summary>
    Private Function BuildParsers(fileDate As Date) As List(Of ILogEntryParser)
        ' Order matters: strict headers before permissive ones. FallbackParser is NOT in the list --
        ' it would accept any line and make every choice pointless.
        ' OperatorLogParser sits BEFORE AdobeHostParser: both start with the same timestamp
        ' followed by two spaces, and the Adobe one takes whatever comes after -- so placed
        ' first it would swallow every operator-message line and lose its level and source.
        ' ForexeTimingParser (slice 0089) sits before AdobeHostParser for the same reason.
        Return New List(Of ILogEntryParser) From {
            New HarnessErrorParser(),
            New ApiServerParser(),
            New ForexeTimingParser(),
            New TreeLoggerParser(fileDate),
            New OperatorLogParser(),
            New AdobeHostParser(),
            New RunLogParser()}
    End Function

    ''' <summary>The guess from the file name pattern (section 5.5 of the plan).</summary>
    Private Function GuessByFileName(fileName As String, candidates As List(Of ILogEntryParser)) As ILogEntryParser
        Dim name As String = If(fileName, String.Empty).ToLowerInvariant()
        ' Archives carry the generation at the end (".log.3") -- guess by the base name.
        Dim baseName As String = name
        For generation As Integer = 1 To LogRotation.BackupCount
            Dim suffix As String = "." & generation.ToString(Globalization.CultureInfo.InvariantCulture)
            If baseName.EndsWith(suffix, StringComparison.Ordinal) Then
                baseName = baseName.Substring(0, baseName.Length - suffix.Length)
                Exit For
            End If
        Next

        Dim wanted As String
        If baseName = "harness_errors.log" Then
            wanted = "HarnessError"
        ElseIf baseName = "adobe_preview.log" OrElse baseName = "acropdf_trace.log" Then
            ' acropdf_trace.log (AcroPdfTraceLog, KBot.Controls) uses the same line format.
            wanted = "AdobeHost"
        ElseIf baseName = OperatorLog.FileNameOnly Then
            wanted = "OperatorLog"
        ElseIf baseName = "forexe_timing.log" Then
            wanted = "ForexeTiming"
        ElseIf baseName.StartsWith("api_", StringComparison.Ordinal) Then
            wanted = "ApiServer"
        ElseIf baseName.StartsWith("test_", StringComparison.Ordinal) Then
            wanted = "RunLog"
        ElseIf baseName.StartsWith("log_", StringComparison.Ordinal) Then
            wanted = "TreeLogger"
        Else
            Return Nothing
        End If

        For Each p As ILogEntryParser In candidates
            If p.Name = wanted Then Return p
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Runs the guessed parser over the first <see cref="ProbeLineCount"/> non-empty lines.
    ''' True if it recognises at least <see cref="MinimumHeaderRatio"/> of them as headers.
    ''' </summary>
    Private Function GuessSurvivesProbe(parser As ILogEntryParser, lines As String()) As Boolean
        Dim probed As Integer = 0
        Dim headers As Integer = 0
        For Each line As String In lines
            If String.IsNullOrWhiteSpace(line) Then Continue For
            probed += 1
            Dim tmp As LogEntry = Nothing
            If parser.TryParseHeader(line, tmp) Then headers += 1
            If probed >= ProbeLineCount Then Exit For
        Next
        ' Empty file: nothing to disprove, so the guess stands.
        If probed = 0 Then Return True

        ' Block formats: the ratio does NOT apply (see ExpectsHeaderOnEveryLine). A single
        ' recognised header proves the format is the one the file name promises.
        If Not parser.ExpectsHeaderOnEveryLine Then Return headers > 0

        Return (headers / CDbl(probed)) >= MinimumHeaderRatio
    End Function

    ''' <summary>Splits into lines, tolerating CRLF, LF and CR.</summary>
    Private Function SplitLines(text As String) As String()
        Return text.Replace(vbCrLf, vbLf).Replace(ControlChars.Cr, ControlChars.Lf).Split(ControlChars.Lf)
    End Function

End Module
