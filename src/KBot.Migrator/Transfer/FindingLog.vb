Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports KBot.Common

''' <summary>
''' The log of everything the findings grid ever showed:
''' <c>&lt;AppDir&gt;\Logs\migrator_constatari.log</c>.
''' </summary>
''' <remarks>
''' <para>
''' The grid is a live view of ONE verification: the next «Verifică» clears it, and the
''' detail pane holds one row at a time. Once the operator has run again, or closed the
''' window, there is nothing left to read - and a report of the form "it said unit 53 has
''' no FOREXE file" cannot be answered without knowing which path it tried. Here every
''' verification leaves its whole picture: the request it ran on, every finding with every
''' field, and the <see cref="Finding.Detail"/> the grid has no column for.
''' </para>
''' <para>
''' Same family as <see cref="OperatorLog"/> and <see cref="GlobalErrorLog"/>, and does not
''' overlap either: the message boxes still go to <c>mesaje_operator.log</c> and the
''' exceptions to <c>harness_errors.log</c>. This one is the grid. A verification that
''' throws instead of producing a report is written here too (<see cref="WriteFailure"/>),
''' so the file never has a run that simply is not there.
''' </para>
''' <para>
''' One BLOCK per verification, not one line - a finding's detail is multi-line by nature.
''' Never a password: the request carries three and none of them is read here.
''' </para>
''' <para>
''' TERMINAL sink, like the other two: if the file cannot be written the text goes to
''' <see cref="Trace"/> and nothing is rethrown. A verification must never fail because
''' its log did.
''' </para>
''' </remarks>
Public Module FindingLog

    Private ReadOnly _gate As New Object()

    ''' <summary>The file name, next to the executable, under <c>Logs\</c>.</summary>
    Public Const FileNameOnly As String = "migrator_constatari.log"

    ''' <summary>The full path of the log, for telling the operator where to look.</summary>
    Public Function FilePath() As String
        Return LogPaths.Combine(FileNameOnly)
    End Function

    ''' <summary>
    ''' Writes one verification: the request, every finding in the grid's own order
    ''' (blocking first), its detail, and the summary. Never throws.
    ''' </summary>
    Public Sub WriteReport(request As TransferRequest, report As VerificationReport)
        Try
            Dim sb As New StringBuilder()
            AppendHeader(sb, "VERIFICARE", request)

            Dim ordered = report.Findings.
                OrderByDescending(Function(f) CInt(f.Severity)).
                ThenBy(Function(f) f.Table, StringComparer.OrdinalIgnoreCase).
                ToList()

            sb.AppendLine($"findings: {ordered.Count}  (blocking {report.BlockingCount})")
            Dim index = 0
            For Each finding In ordered
                index += 1
                sb.AppendLine()
                sb.AppendLine($"--- #{index.ToString(CultureInfo.InvariantCulture)}  {finding.Severity}  [{finding.Kind}]")
                sb.AppendLine($"table:    {Blank(finding.Table)}")
                sb.AppendLine($"column:   {Blank(finding.Column)}")
                If finding.RowCount > 0 Then
                    sb.AppendLine($"rows:     {finding.RowCount.ToString(CultureInfo.InvariantCulture)}")
                End If
                sb.AppendLine($"message:  {finding.Message}")
                If Not String.IsNullOrWhiteSpace(finding.Detail) Then
                    sb.AppendLine("detail:")
                    For Each line In finding.Detail.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
                        sb.AppendLine("    " & line.TrimEnd())
                    Next
                End If
            Next

            sb.AppendLine()
            sb.AppendLine("summary:  " & report.Summary())
            If report.WriteOrder IsNot Nothing AndAlso report.WriteOrder.Count > 0 Then
                sb.AppendLine("write order: " & String.Join(" > ", report.WriteOrder))
            End If
            For Each entry In report.RowCounts.OrderBy(Function(p) p.Key, StringComparer.OrdinalIgnoreCase)
                sb.AppendLine($"rows measured: {entry.Key} = {entry.Value.ToString(CultureInfo.InvariantCulture)}")
            Next
            sb.AppendLine()

            Append(sb.ToString())
        Catch terminalEx As Exception
            Trace.WriteLine("FindingLog terminal failure (WriteReport): " & terminalEx.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Writes a verification that ended in an exception instead of a report: the same
    ''' request header, then the exception in full (type, message, stack, inner ones).
    ''' Never throws.
    ''' </summary>
    Public Sub WriteFailure(request As TransferRequest, ex As Exception)
        Try
            Dim sb As New StringBuilder()
            AppendHeader(sb, "VERIFICARE ESUATA", request)
            sb.AppendLine("exception:")
            sb.AppendLine(If(ex IsNot Nothing, ex.ToString(), "<null exception>"))
            sb.AppendLine()
            Append(sb.ToString())
        Catch terminalEx As Exception
            Trace.WriteLine("FindingLog terminal failure (WriteFailure): " & terminalEx.Message)
        End Try
    End Sub

    ''' <summary>
    ''' The run's context: when, on what server and database, which units with which
    ''' files (and whether each file is on this disk), which tables. Every value the
    ''' operator could have typed differently, so a finding can be read against the
    ''' request that produced it. No password of any kind.
    ''' </summary>
    Private Sub AppendHeader(sb As StringBuilder, title As String, request As TransferRequest)
        sb.AppendLine("==== " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) &
                      "  [" & title & "] ====")
        If request Is Nothing Then
            sb.AppendLine("request:  (none - failed before it was built)")
            Return
        End If

        sb.AppendLine($"operator: {Blank(request.OperatorName)}")
        sb.AppendLine($"server:   {request.Server.Describe()}")
        sb.AppendLine($"target:   {request.TargetDatabase}   (template {request.TemplateDatabase}, common {request.CommonDatabase})")
        sb.AppendLine($"cod fiscal: registry «{Blank(request.RegistryCodFiscal())}», used «{Blank(request.ResolvedCodFiscal())}»")
        sb.AppendLine($"journal:  {Blank(request.JournalFolder)}")
        If request.ForexeFileOverride.Length > 0 Then
            ' Say on how many units the path actually landed: before 21.09 this line read
            ' "used «...»" while zero units had received it, and the 1364 findings below
            ' looked like a broken file rather than a file nobody opened.
            Dim covered = request.Units.Where(Function(u) String.Equals(u.ForexeFilePath, request.ForexeFileOverride, StringComparison.OrdinalIgnoreCase)).Count()
            sb.AppendLine($"forexe file: registry «{Blank(request.RegistryForexeFile)}», used «{request.ForexeFileOverride}» " &
                          $"(operator override, applied to {covered.ToString(CultureInfo.InvariantCulture)} of {request.Units.Count.ToString(CultureInfo.InvariantCulture)} selected units)")
        End If
        sb.AppendLine($"registry units: {request.RegistryUnits.Count.ToString(CultureInfo.InvariantCulture)} in total")
        sb.AppendLine($"selected units: {request.Units.Count.ToString(CultureInfo.InvariantCulture)}")
        For Each unit In request.Units
            sb.AppendLine($"    {unit.IdUnitate.ToString(CultureInfo.InvariantCulture)}  {unit.NumeUnitate}  [{unit.Sursa}]")
            sb.AppendLine($"        nomenclator: {PathState(unit.UnitFilePath)}")
            sb.AppendLine($"        forexe:      {PathState(unit.ForexeFilePath)}")
        Next
        sb.AppendLine($"selected tables: {request.SelectedTables.Count.ToString(CultureInfo.InvariantCulture)}")
        sb.AppendLine("    " & String.Join(", ", request.SelectedTables.OrderBy(Function(t) t, StringComparer.OrdinalIgnoreCase)))
    End Sub

    ''' <summary>«path  (exists)» / «path  (MISSING)» / «(none)».</summary>
    Private Function PathState(path As String) As String
        If String.IsNullOrWhiteSpace(path) Then Return "(none)"
        Return path & If(File.Exists(path), "  (exists)", "  (MISSING)")
    End Function

    Private Function Blank(value As String) As String
        Return If(String.IsNullOrWhiteSpace(value), "-", value)
    End Function

    Private Sub Append(text As String)
        LogPaths.EnsureLogsDirectory()
        Dim path = FilePath()
        SyncLock _gate
            ' Rotation never throws: if it fails, the block is written anyway. See LogRotation.
            LogRotation.Roll(path)
            File.AppendAllText(path, text, New UTF8Encoding(True))
        End SyncLock
    End Sub

End Module
