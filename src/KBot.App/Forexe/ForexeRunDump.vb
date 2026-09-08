Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode
Imports GeneralClasses
Imports KBot.Common
Imports KBot.Forexe

''' <summary>
''' Black box of the FOREXE downloader (slice 0054): ONE folder per download attempt, written
''' whatever the outcome - success, failed workflow, missing table, cancelled, or a thrown
''' exception. <see cref="WorkflowResultStore"/> only writes the happy path, so a run that
''' brought back nothing usable left nothing behind to look at afterwards; that is exactly the
''' run somebody needs to read.
'''
''' <para>Each run gets <c>&lt;WorkflowResults&gt;\Runs\&lt;stamp&gt;_&lt;operation&gt;_&lt;code&gt;\</c>
''' holding, as far as each one exists:
''' <list type="bullet">
''' <item><c>run.json</c> - what was asked, in which context, and how it ended.</item>
''' <item><c>raw.json</c> - the executor variables exactly as the robot left them.</item>
''' <item><c>tables.json</c> - the same variables after the runner broke them into tables.</item>
''' <item><c>mapped.json</c> - the mapped form, when the caller got that far.</item>
''' <item><c>forexe.log</c> - the whole robot log of the job, from <see cref="JobHistoryManager"/>.</item>
''' </list></para>
'''
''' <para>Writing the dump can NEVER fail the download it describes: a broken diagnostic that
''' kills the real work is worse than no diagnostic. <see cref="Save"/> logs and returns Nothing
''' instead of throwing, and the caller says so on the operator console - the failure is
''' reported, not swallowed in silence.</para>
''' </summary>
Public NotInheritable Class ForexeRunDump

    ''' <summary>Folder holding the runs, inside the existing WorkflowResults folder.</summary>
    Public Const FolderName As String = "Runs"

    ' Same relaxed encoder as WorkflowResultStore: these files are meant to be read by a human,
    ' so extended-latin letters stay literal instead of turning into \uXXXX escapes.
    Private Shared ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    Private ReadOnly _info As New ForexeRunInfo()
    Private ReadOnly _folder As String
    ' Where the job history stood when this run started, so Save can pick out the entries this
    ' run added and nobody else's.
    Private ReadOnly _historyMark As Integer

    ''' <param name="operation">Which download this is - "PrelucrareCompleta" or "ListaAngajamente".</param>
    ''' <param name="code">Angajament code; empty for the list, which has none.</param>
    ''' <param name="session">Operator context, so a dump read later says WHICH database it came from.</param>
    Public Sub New(operation As String, code As String, session As SessionContext)
        _info.Operation = If(operation, String.Empty)
        _info.Code = If(code, String.Empty)
        _info.StartedAt = DateTime.Now
        If session IsNot Nothing Then
            _info.DbName = session.DbName
            _info.FiscalYear = session.An
            _info.SectorSursa = session.SectorSursa
        End If
        _historyMark = HistoryCount()
        _folder = Path.Combine(RunsFolder, StampedName())
    End Sub

    ''' <summary>The folder this run will be written to (created only when <see cref="Save"/> runs).</summary>
    Public ReadOnly Property Folder As String
        Get
            Return _folder
        End Get
    End Property

    ''' <summary>Root folder of all runs.</summary>
    Public Shared ReadOnly Property RunsFolder As String
        Get
            Return Path.Combine(WorkflowResultStore.OutputFolder, FolderName)
        End Get
    End Property

    ''' <summary>
    ''' Records a free-form fact about this run - the reverse date taken from local history, the
    ''' name of a table that was expected and missing, the reason the run never started.
    ''' </summary>
    Public Sub Note(key As String, value As String)
        If String.IsNullOrWhiteSpace(key) Then Return
        _info.Notes(key) = If(value, String.Empty)
    End Sub

    ''' <summary>Records the job as it was handed to the robot, parameters included.</summary>
    Public Sub NoteRequest(job As JobRequest)
        If job Is Nothing Then Return
        _info.WorkflowName = job.WorkflowName
        _info.WflPath = job.WflPath
        If job.Parameters Is Nothing Then Return
        For Each kvp In job.Parameters
            _info.Parameters(kvp.Key) = kvp.Value
        Next
    End Sub

    ''' <summary>
    ''' Writes the whole run to disk and returns the folder, or Nothing if the writing itself
    ''' failed (logged; the caller reports it, the download carries on either way).
    ''' </summary>
    ''' <param name="outcome">
    ''' How it ended, in one word for grepping: <c>ok</c>, <c>esuat</c>, <c>tabel-lipsa</c>,
    ''' <c>ocupat</c>, <c>fara-sesiune</c>, <c>exceptie</c>.
    ''' </param>
    ''' <param name="result">What the robot returned, or Nothing when it never ran.</param>
    ''' <param name="mapped">The mapped form (package or list), when the caller got that far.</param>
    Public Function Save(outcome As String, result As JobResult, Optional mapped As Object = Nothing) As String
        Try
            _info.Outcome = If(outcome, String.Empty)
            _info.FinishedAt = DateTime.Now
            _info.DurationSeconds = Math.Round((_info.FinishedAt - _info.StartedAt).TotalSeconds, 1)

            If result IsNot Nothing Then
                _info.Success = result.Success
                _info.Message = If(result.Message, String.Empty)
                If result.Tables IsNot Nothing Then
                    For Each kvp In result.Tables
                        _info.TableRowCounts(kvp.Key) = If(kvp.Value Is Nothing, 0, kvp.Value.Count)
                    Next
                End If
                If result.Data IsNot Nothing Then
                    _info.ScalarKeys.AddRange(result.Data.Keys.Where(
                        Function(k) result.Tables Is Nothing OrElse Not result.Tables.ContainsKey(k)))
                End If
            End If

            Directory.CreateDirectory(_folder)
            Write("run.json", _info)
            If result IsNot Nothing Then
                ' RAW first: a renamed or empty FOREXE variable shows up here even when the runner
                ' could not turn it into a table, which is the whole point of keeping both.
                Write("raw.json", result.Data)
                If result.Tables IsNot Nothing AndAlso result.Tables.Count > 0 Then
                    Write("tables.json", result.Tables)
                End If
            End If
            If mapped IsNot Nothing Then Write("mapped.json", mapped)
            WriteLog()
            Return _folder
        Catch ex As Exception
            ' Terminal for the dump only: the download must not die because its black box did.
            GlobalErrorLog.Write("ForexeRunDump.Save", ex)
            Return Nothing
        End Try
    End Function

    ' ---- internals -------------------------------------------------------

    Private Sub Write(fileName As String, content As Object)
        File.WriteAllText(Path.Combine(_folder, fileName),
                          JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8)
    End Sub

    ''' <summary>
    ''' The robot log of this run, taken from the entries the job history gained since the
    ''' constructor ran. If the history rolled over (200 jobs in one session) the mark is no
    ''' longer meaningful, so the last entry is taken instead - one job log is better than none.
    ''' </summary>
    Private Sub WriteLog()
        Dim jobs As List(Of JobHistoryItem) = HistorySince(_historyMark)
        If jobs.Count = 0 Then Return

        Dim text As New StringBuilder()
        For Each job As JobHistoryItem In jobs
            text.AppendLine($"=== {job.JobName} | {job.Timestamp:yyyy-MM-dd HH:mm:ss} | {job.Status} ===")
            SyncLock job
                text.AppendLine(job.FullLog.ToString())
            End SyncLock
            For Each kvp In job.OutputData
                text.AppendLine($"[out] {kvp.Key} = {kvp.Value}")
            Next
            text.AppendLine()
        Next
        File.WriteAllText(Path.Combine(_folder, "forexe.log"), text.ToString(), Encoding.UTF8)
    End Sub

    Private Shared Function HistoryCount() As Integer
        Try
            SyncLock JobHistoryManager.History
                Return JobHistoryManager.History.Count
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeRunDump.HistoryCount", ex)
            Return 0
        End Try
    End Function

    Private Shared Function HistorySince(mark As Integer) As List(Of JobHistoryItem)
        Try
            SyncLock JobHistoryManager.History
                Dim all As List(Of JobHistoryItem) = JobHistoryManager.History
                If all.Count = 0 Then Return New List(Of JobHistoryItem)()
                If mark < 0 OrElse mark >= all.Count Then
                    Return New List(Of JobHistoryItem) From {all(all.Count - 1)}
                End If
                Return all.Skip(mark).ToList()
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeRunDump.HistorySince", ex)
            Return New List(Of JobHistoryItem)()
        End Try
    End Function

    ' Folder name: sortable stamp first, then what it was and for which code.
    Private Function StampedName() As String
        Dim parts As New List(Of String) From {_info.StartedAt.ToString("yyyyMMdd_HHmmss")}
        If Not String.IsNullOrWhiteSpace(_info.Operation) Then parts.Add(_info.Operation)
        If Not String.IsNullOrWhiteSpace(_info.Code) Then parts.Add(_info.Code)
        Return SafeName(String.Join("_", parts))
    End Function

    ' The code and the operation end up in a FOLDER NAME: drop whatever Windows refuses.
    Private Shared Function SafeName(name As String) As String
        Dim bad As Char() = Path.GetInvalidFileNameChars()
        Return New String(name.Where(Function(c) Not bad.Contains(c)).ToArray())
    End Function

End Class

''' <summary>
''' The <c>run.json</c> of one download attempt. Flat on purpose: it is read by eye, and every
''' field answers a question somebody asks when a download brought back nothing.
''' </summary>
Public NotInheritable Class ForexeRunInfo
    Public Property Code As String = String.Empty
    Public Property Operation As String = String.Empty
    Public Property StartedAt As Date
    Public Property FinishedAt As Date
    Public Property DurationSeconds As Double
    Public Property Outcome As String = String.Empty
    Public Property Success As Boolean
    Public Property Message As String = String.Empty
    Public Property DbName As String = String.Empty
    Public Property FiscalYear As Integer
    Public Property SectorSursa As String = String.Empty
    Public Property WorkflowName As String = String.Empty
    Public Property WflPath As String = String.Empty
    Public Property Parameters As New Dictionary(Of String, String)
    Public Property Notes As New Dictionary(Of String, String)
    Public Property TableRowCounts As New Dictionary(Of String, Integer)
    Public Property ScalarKeys As New List(Of String)
End Class
