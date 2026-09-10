Option Strict On
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' THE STOPWATCH OF <see cref="KBotLaneView"/>, and only of it: a separate journal, written to
''' <c>&lt;AppDir&gt;\Logs\laneview.log</c>, holding how long each pass took and what it was
''' working on.
'''
''' <para><b>DEBUG BUILDS ONLY.</b> <see cref="CompiledIn"/> is a compile-time constant, so in a
''' Release build every method below folds down to an immediate return, no file is ever opened,
''' and no string is ever built. This is a bench instrument, not a feature of the application.</para>
'''
''' <para><b>Why a journal of its own.</b> <c>harness_errors.log</c> takes exceptions and
''' <c>mesaje_operator.log</c> takes the sentences the operator read. Neither takes measurements,
''' and mixing thousands of timing lines into either would ruin the one thing they are good for.
''' Same reasoning, and the same shape, as the server's <c>forexe_timing.log</c> and
''' <c>asociere.log</c>: own file, own switch, nothing leaking into the neighbours.</para>
'''
''' <para><b>Why it buffers.</b> The pointer sends mouse moves faster than a disk write returns.
''' A journal that appended per line would be measuring itself, and the number it reported for
''' "mouse move" would be the cost of writing the word "mouse move" to a file. Lines are held in
''' memory and written in batches — see <see cref="FlushEvery"/> and <see cref="FlushAfterMs"/>.
''' The cost on the measured path is a timestamp, a string and a list append.</para>
'''
''' <para><b>The two kinds of line.</b> An EVENT line is one pass through one handler, with its
''' duration. A ROLLUP line is every operation seen since the last rollup, with count, total,
''' worst and average. The event lines say what happened; the rollup says where the time went,
''' which on a surface repainting under the mouse is rarely the same question.</para>
'''
''' <para><b>Never throws.</b> Terminal sink, like <c>GlobalErrorLog</c>: a journal that can break
''' the paint it is measuring is worse than no journal. Every failure goes to
''' <see cref="Trace"/> and the instrument switches itself off.</para>
'''
''' <para>Switched off with <c>KBOT_LANE_LOG=0</c> in the environment, or by assigning
''' <see cref="Enabled"/> = False.</para>
''' </summary>
Public NotInheritable Class KBotLaneLog

    ' Static instrument only — never instantiated.
    Private Sub New()
    End Sub

#If DEBUG Then
    ''' <summary>True in a Debug build. A CONSTANT, so Release folds every path below away.</summary>
    Private Const CompiledIn As Boolean = True
#Else
    ''' <summary>False in a Release build. A CONSTANT, so Release folds every path below away.</summary>
    Private Const CompiledIn As Boolean = False
#End If

    ''' <summary>The file, in the same folder as every other client journal.</summary>
    Public Const FileNameOnly As String = "laneview.log"

    ''' <summary>Environment switch. <c>0</c>, <c>no</c> or <c>off</c> closes the journal.</summary>
    Public Const EnvironmentSwitch As String = "KBOT_LANE_LOG"

    ''' <summary>Lines held before the batch is written.</summary>
    Public Const FlushEvery As Integer = 128

    ''' <summary>Milliseconds a line may wait in memory before the batch is written anyway.</summary>
    Public Const FlushAfterMs As Integer = 1500

    ''' <summary>Milliseconds between two ROLLUP lines. Only ever written next to a real event.</summary>
    Public Const RollupAfterMs As Integer = 5000

    ''' <summary>Column the duration and the detail are aligned to, so a run reads as a table.</summary>
    Private Const OperationWidth As Integer = 11

    Private Shared ReadOnly _gate As New Object()
    Private Shared ReadOnly _pending As New List(Of String)(FlushEvery)
    Private Shared ReadOnly _tally As New Dictionary(Of String, Measure)(StringComparer.Ordinal)
    Private Shared ReadOnly _clock As Stopwatch = Stopwatch.StartNew()

    Private Shared _enabled As Boolean = CompiledIn AndAlso AllowedByEnvironment()
    Private Shared _bannerWritten As Boolean
    Private Shared _lastFlushMs As Long
    Private Shared _lastRollupMs As Long

    ''' <summary>
    ''' Is the journal open? Always False in a Release build, whatever is assigned — the code that
    ''' would write it is not compiled in, so reporting True would be a lie.
    ''' </summary>
    Public Shared Property Enabled As Boolean
        Get
            Return _enabled
        End Get
        Set(value As Boolean)
            _enabled = CompiledIn AndAlso value
        End Set
    End Property

    ''' <summary>
    ''' The moment a measured pass starts. Feed the answer back to <see cref="Done"/>.
    ''' Returns 0 when the journal is closed, and <see cref="Done"/> ignores a 0.
    ''' </summary>
    Public Shared Function Mark() As Long
        If Not _enabled Then Return 0L
        Return Stopwatch.GetTimestamp()
    End Function

    ''' <summary>
    ''' Closes a pass opened by <see cref="Mark"/>: writes one EVENT line carrying its duration,
    ''' and adds it to the running totals the next ROLLUP will report.
    ''' </summary>
    ''' <param name="operation">Short ASCII name, e.g. <c>PAINT</c>, <c>MOVE</c>, <c>DRAGOVER</c>.</param>
    ''' <param name="mark">Whatever <see cref="Mark"/> returned. 0 = the journal was closed then.</param>
    ''' <param name="detail">What the pass was working on. May be Nothing.</param>
    Public Shared Sub Done(operation As String, mark As Long, Optional detail As String = Nothing)
        If Not _enabled OrElse mark = 0L Then Return
        Try
            Dim ms As Double = (Stopwatch.GetTimestamp() - mark) * 1000.0R / Stopwatch.Frequency
            Add(operation, ms, detail, True)
        Catch ex As Exception
            Fail("Done", ex)
        End Try
    End Sub

    ''' <summary>
    ''' One EVENT line with no duration — something happened, and it is worth seeing WHERE among
    ''' the timed passes it happened. Used for the invalidations, which cost nothing themselves
    ''' but decide how many repaints follow.
    ''' </summary>
    Public Shared Sub Note(operation As String, Optional detail As String = Nothing)
        If Not _enabled Then Return
        Try
            Add(operation, -1.0R, detail, False)
        Catch ex As Exception
            Fail("Note", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Writes everything still in memory, then a final ROLLUP. Called when a lane view is
    ''' disposed, so a bench run that ends by closing its window leaves a complete file behind.
    ''' </summary>
    Public Shared Sub Flush()
        If Not _enabled Then Return
        Try
            SyncLock _gate
                AppendRollup()
                WriteBatch()
            End SyncLock
        Catch ex As Exception
            Fail("Flush", ex)
        End Try
    End Sub

    ''' <summary>The file this run is writing to. Empty when the journal is closed.</summary>
    Public Shared Function FilePath() As String
        If Not _enabled Then Return String.Empty
        Try
            Return LogPaths.Combine(FileNameOnly)
        Catch ex As Exception
            Fail("FilePath", ex)
            Return String.Empty
        End Try
    End Function

    ' =====================================================================
    ' THE BUFFER
    ' =====================================================================

    Private Shared Sub Add(operation As String, ms As Double, detail As String, timed As Boolean)
        Dim now As Long = _clock.ElapsedMilliseconds
        SyncLock _gate
            If timed Then Tally(operation).Record(ms)

            _pending.Add(Compose(operation, ms, detail, timed))

            ' The rollup goes in NEXT TO a real event, never on a timer of its own: a timer would
            ' mean a background thread touching the same list the paint path is appending to, for
            ' a line nobody reads while nothing is happening.
            If now - _lastRollupMs >= RollupAfterMs Then AppendRollup()

            If _pending.Count >= FlushEvery OrElse now - _lastFlushMs >= FlushAfterMs Then WriteBatch()
        End SyncLock
    End Sub

    Private Shared Function Compose(operation As String, ms As Double, detail As String, timed As Boolean) As String
        Dim sb As New StringBuilder(96)
        sb.Append(Date.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture))
        sb.Append("  ")
        sb.Append(Pad(operation))
        If timed Then
            ' Right-aligned in a fixed column so the eye finds the outlier without reading.
            sb.Append(ms.ToString("F3", CultureInfo.InvariantCulture).PadLeft(9))
            sb.Append(" ms  ")
        Else
            sb.Append(New String(" "c, 14))
        End If
        If Not String.IsNullOrEmpty(detail) Then sb.Append(detail)
        ' A pass with nothing to say about itself must not leave a column of trailing blanks:
        ' `grep -n "  markers"` on a file full of them is unreadable.
        Return sb.ToString().TrimEnd()
    End Function

    Private Shared Function Pad(operation As String) As String
        Dim name As String = If(operation, "?")
        If name.Length >= OperationWidth Then Return name & " "
        Return name.PadRight(OperationWidth) & " "
    End Function

    Private Shared Function Tally(operation As String) As Measure
        Dim m As Measure = Nothing
        If _tally.TryGetValue(operation, m) Then Return m
        m = New Measure()
        _tally.Add(operation, m)
        Return m
    End Function

    ''' <summary>
    ''' One ROLLUP line: every operation measured since the last one, worst first, then the
    ''' totals are cleared. Worst first because the whole reason to read this file is to find the
    ''' pass that ate the frame, and it is never the one with the most entries.
    ''' </summary>
    Private Shared Sub AppendRollup()
        _lastRollupMs = _clock.ElapsedMilliseconds
        If _tally.Count = 0 Then Return

        Dim ordered As New List(Of KeyValuePair(Of String, Measure))(_tally.Count)
        For Each pair As KeyValuePair(Of String, Measure) In _tally
            ordered.Add(pair)
        Next
        ordered.Sort(Function(a, b) b.Value.Total.CompareTo(a.Value.Total))

        Dim sb As New StringBuilder(160)
        sb.Append(Date.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture))
        sb.Append("  ")
        sb.Append(Pad("ROLLUP"))
        sb.Append(New String(" "c, 14))
        For i As Integer = 0 To ordered.Count - 1
            If i > 0 Then sb.Append(" | ")
            Dim name As String = ordered(i).Key
            Dim m As Measure = ordered(i).Value
            sb.Append(name)
            sb.Append(" n=")
            sb.Append(m.Count.ToString(CultureInfo.InvariantCulture))
            sb.Append(" tot=")
            sb.Append(m.Total.ToString("F1", CultureInfo.InvariantCulture))
            sb.Append(" avg=")
            sb.Append(m.Average.ToString("F3", CultureInfo.InvariantCulture))
            sb.Append(" max=")
            sb.Append(m.Worst.ToString("F3", CultureInfo.InvariantCulture))
        Next
        _pending.Add(sb.ToString())
        _tally.Clear()
    End Sub

    ' =====================================================================
    ' THE FILE
    ' =====================================================================

    ''' <summary>
    ''' Writes the whole batch in one append. Called under <c>_gate</c>, so the list cannot grow
    ''' while it is being drained.
    ''' </summary>
    Private Shared Sub WriteBatch()
        _lastFlushMs = _clock.ElapsedMilliseconds
        If _pending.Count = 0 Then Return
        Try
            LogPaths.EnsureLogsDirectory()
            Dim path As String = LogPaths.Combine(FileNameOnly)

            Dim sb As New StringBuilder(_pending.Count * 96)
            If Not _bannerWritten Then
                _bannerWritten = True
                sb.Append("==== KBotLaneView, ")
                sb.Append(Date.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                sb.AppendLine(" (Debug build; KBOT_LANE_LOG=0 closes this file) ====")
            End If
            For Each line As String In _pending
                sb.AppendLine(line)
            Next
            _pending.Clear()

            ' Rotation first, exactly as GlobalErrorLog does it: same 10 MB / five generations
            ' rule across every journal the client writes.
            LogRotation.Roll(path)
            File.AppendAllText(path, sb.ToString(), New UTF8Encoding(True))
        Catch ex As Exception
            ' Whatever was pending is dropped on purpose: keeping it would grow without bound on
            ' a disk that has already refused us once.
            _pending.Clear()
            Fail("WriteBatch", ex)
        End Try
    End Sub

    ''' <summary>
    ''' A failed journal SWITCHES ITSELF OFF and says so on the trace listener. It never throws
    ''' and never calls <c>GlobalErrorLog</c>: an instrument that fills the error file with its
    ''' own failures, once per mouse move, would bury the errors it was brought in to help find.
    ''' </summary>
    Private Shared Sub Fail(where As String, ex As Exception)
        _enabled = False
        Trace.WriteLine("KBotLaneLog." & where & " failed, journal closed: " &
                        If(ex IsNot Nothing, ex.Message, "<null>"))
    End Sub

    Private Shared Function AllowedByEnvironment() As Boolean
        Try
            Dim raw As String = Environment.GetEnvironmentVariable(EnvironmentSwitch)
            If String.IsNullOrWhiteSpace(raw) Then Return True
            Select Case raw.Trim().ToLowerInvariant()
                Case "0", "no", "off", "false" : Return False
                Case Else : Return True
            End Select
        Catch
            ' No environment to read (a restricted host). Open is the useful answer in a Debug
            ' build, and the first write will close the journal if the folder is unreachable too.
            Return True
        End Try
    End Function

    ''' <summary>Running totals for one operation name, cleared by every ROLLUP.</summary>
    Private NotInheritable Class Measure
        Public Property Count As Integer
        Public Property Total As Double
        Public Property Worst As Double

        Public Sub Record(ms As Double)
            Count += 1
            Total += ms
            If ms > Worst Then Worst = ms
        End Sub

        Public ReadOnly Property Average As Double
            Get
                If Count = 0 Then Return 0.0R
                Return Total / Count
            End Get
        End Property
    End Class
End Class
