Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' Slice 0081-07 -- every answer FOREXE gave a workflow, kept in <c>Rezultate_Forexe</c> in a form
''' that can be loaded back AS IF the workflow had run again.
'''
''' <para><b>Why.</b> A run that saves in forexecab (a new angajament, a reservation row, a
''' confirmation) cannot be repeated to try again: it would save a second time. If K-BOT read the
''' answer wrong, the fix is made in K-BOT and the SAME answer is loaded again
''' (<see cref="ForexeController.ReplayMode"/>), with nothing sent to FOREXE.</para>
'''
''' <para><b>What is kept is enough.</b> A <see cref="JobResult"/> is its verdict (Success,
''' Message, StoppedBeforeSave) plus the executor's flat variables (<c>Data</c>); the tables are
''' only those variables parsed (<see cref="ForexeRunner.TryParseTable"/>), and the captures are
''' base64 inside them. So the file keeps the variables and the tables are rebuilt on load, exactly
''' as the runner builds them.</para>
'''
''' <para>Writing an answer can NEVER fail the run it describes (same rule as
''' <see cref="ForexeRunDump"/>): <see cref="Save"/> logs and returns Nothing.</para>
''' </summary>
Public NotInheritable Class ForexeAnswerStore

    ''' <summary>The version of the file layout, written into every file.</summary>
    Public Const CurrentVersion As Integer = 1

    ' Same relaxed encoder as the other FOREXE files: extended-latin letters stay literal.
    Private Shared ReadOnly _jsonOptions As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    ''' <summary>The folder of the answers (the «RezultateForexe» folder setting).</summary>
    Public Shared ReadOnly Property Folder As String
        Get
            Return KBotPaths.FolderRezultateForexe
        End Get
    End Property

    ''' <summary>The answer of one run, ready to be written.</summary>
    Public Shared Function FromRun(job As JobRequest, code As String, session As SessionContext,
                                   result As JobResult, recordedAt As Date) As ForexeAnswer
        ArgumentNullException.ThrowIfNull(job)
        ArgumentNullException.ThrowIfNull(result)
        Dim answer As New ForexeAnswer With {
            .Version = CurrentVersion,
            .RecordedAt = recordedAt,
            .Workflow = If(job.WorkflowName, String.Empty),
            .WflFile = If(String.IsNullOrEmpty(job.WflPath), String.Empty, Path.GetFileName(job.WflPath)),
            .Code = If(code, String.Empty),
            .DryRun = job.StopBeforeSave,
            .Success = result.Success,
            .Message = If(result.Message, String.Empty),
            .StoppedBeforeSave = result.StoppedBeforeSave
        }
        If session IsNot Nothing Then
            answer.DbName = If(session.DbName, String.Empty)
            answer.FiscalYear = session.An
            answer.SectorSursa = If(session.SectorSursa, String.Empty)
        End If
        If job.Parameters IsNot Nothing Then
            For Each kvp In job.Parameters
                answer.Parameters(kvp.Key) = kvp.Value
            Next
        End If
        If result.Data IsNot Nothing Then
            For Each kvp In result.Data
                answer.Variables(kvp.Key) = kvp.Value
            Next
        End If
        Return answer
    End Function

    ''' <summary>
    ''' Writes the answer of one run and returns the file, or Nothing when the writing failed
    ''' (logged; the run carries on either way).
    ''' </summary>
    Public Shared Function Save(job As JobRequest, code As String, session As SessionContext,
                                result As JobResult) As String
        Try
            If job Is Nothing OrElse result Is Nothing Then Return Nothing
            Dim answer As ForexeAnswer = FromRun(job, code, session, result, DateTime.Now)
            Dim target As String = Path.Combine(Folder, FileName(answer))
            Directory.CreateDirectory(Folder)
            File.WriteAllText(target, JsonSerializer.Serialize(answer, _jsonOptions), New UTF8Encoding(False))
            Return target
        Catch ex As Exception
            ' Terminal for the answer only: the run it describes has already happened.
            GlobalErrorLog.Write("ForexeAnswerStore.Save", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>Reads one answer file. Risky boundary: logs and rethrows.</summary>
    Public Shared Function Load(filePath As String) As ForexeAnswer
        Try
            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("Fișierul răspunsului lipsește.", NameOf(filePath))
            End If
            Dim answer As ForexeAnswer = JsonSerializer.Deserialize(Of ForexeAnswer)(File.ReadAllText(filePath, Encoding.UTF8))
            If answer Is Nothing Then Throw New InvalidDataException($"Fișierul «{filePath}» nu conține un răspuns FOREXE.")
            If answer.Version <> CurrentVersion Then
                Throw New InvalidDataException(
                    $"Fișierul «{filePath}» are versiunea {answer.Version}; acest K-BOT citește versiunea {CurrentVersion}.")
            End If
            If answer.Parameters Is Nothing Then answer.Parameters = New Dictionary(Of String, String)()
            If answer.Variables Is Nothing Then answer.Variables = New Dictionary(Of String, String)()
            Return answer
        Catch ex As Exception
            GlobalErrorLog.Write("ForexeAnswerStore.Load", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The answer turned back into the result the runner would have returned: the same verdict,
    ''' the same variables, and the tables parsed from them by the runner's own parser.
    ''' </summary>
    Public Shared Function ToJobResult(answer As ForexeAnswer) As JobResult
        ArgumentNullException.ThrowIfNull(answer)
        Dim result As New JobResult With {
            .Success = answer.Success,
            .Message = If(answer.Message, String.Empty),
            .StoppedBeforeSave = answer.StoppedBeforeSave
        }
        If answer.Variables IsNot Nothing Then
            For Each kvp In answer.Variables
                result.Data(kvp.Key) = kvp.Value
                Dim table As TabelRezultat = ForexeRunner.TryParseTable(kvp.Value)
                If table IsNot Nothing Then result.Tables(kvp.Key) = table
            Next
        End If
        Return result
    End Function

    ''' <summary>
    ''' The names of the parameters that differ between the recorded run and the job K-BOT wants
    ''' to run now (missing on one side counts as different). Empty = the same request.
    ''' </summary>
    Public Shared Function ParameterDifferences(answer As ForexeAnswer, job As JobRequest) As List(Of String)
        Dim result As New List(Of String)()
        If answer Is Nothing OrElse job Is Nothing Then Return result
        Dim recorded As IDictionary(Of String, String) = If(answer.Parameters, New Dictionary(Of String, String)())
        Dim asked As IDictionary(Of String, String) = If(job.Parameters, New Dictionary(Of String, String)())
        For Each key As String In recorded.Keys.Union(asked.Keys, StringComparer.Ordinal).OrderBy(Function(k) k, StringComparer.Ordinal)
            Dim a As String = Nothing
            Dim b As String = Nothing
            Dim inRecorded As Boolean = recorded.TryGetValue(key, a)
            Dim inAsked As Boolean = asked.TryGetValue(key, b)
            If inRecorded <> inAsked OrElse Not String.Equals(a, b, StringComparison.Ordinal) Then result.Add(key)
        Next
        Return result
    End Function

    ''' <summary>
    ''' The file name: sortable stamp, workflow, code -- <c>20260926_143012_457_CreareAngajament_AAB2EF2MCP4.json</c>.
    ''' </summary>
    Public Shared Function FileName(answer As ForexeAnswer) As String
        ArgumentNullException.ThrowIfNull(answer)
        Dim parts As New List(Of String) From {answer.RecordedAt.ToString("yyyyMMdd_HHmmss_fff", Globalization.CultureInfo.InvariantCulture)}
        If Not String.IsNullOrWhiteSpace(answer.Workflow) Then parts.Add(SafeName(answer.Workflow))
        If Not String.IsNullOrWhiteSpace(answer.Code) Then parts.Add(SafeName(answer.Code))
        Return String.Join("_", parts) & ".json"
    End Function

    ''' <summary>The file-dialog pattern that lists the answers of one workflow.</summary>
    Public Shared Function FilePattern(workflow As String) As String
        Dim name As String = SafeName(If(workflow, String.Empty))
        If name.Length = 0 Then Return "*.json"
        Return $"*_{name}_*.json;*_{name}.json"
    End Function

    ' The workflow and the code end up in a FILE NAME: drop whatever Windows refuses.
    Private Shared Function SafeName(name As String) As String
        Dim bad As Char() = Path.GetInvalidFileNameChars()
        Return New String(name.Where(Function(c) Not bad.Contains(c) AndAlso c <> "_"c AndAlso Not Char.IsWhiteSpace(c)).ToArray())
    End Function

End Class

''' <summary>
''' One FOREXE answer on disk (slice 0081-07). Flat on purpose: it is also read by eye.
''' </summary>
Public NotInheritable Class ForexeAnswer
    Public Property Version As Integer = ForexeAnswerStore.CurrentVersion
    Public Property RecordedAt As Date
    ''' <summary>The job's workflow name (JobBuilder), e.g. «CreareAngajament».</summary>
    Public Property Workflow As String = String.Empty
    ''' <summary>The .wfl file that ran, for the reader.</summary>
    Public Property WflFile As String = String.Empty
    ''' <summary>The angajament code the run was for; empty for the list.</summary>
    Public Property Code As String = String.Empty
    Public Property DbName As String = String.Empty
    Public Property FiscalYear As Integer
    Public Property SectorSursa As String = String.Empty
    ''' <summary>True when the run was a dry run (nothing saved in FOREXE).</summary>
    Public Property DryRun As Boolean
    ''' <summary>What K-BOT handed the workflow.</summary>
    Public Property Parameters As New Dictionary(Of String, String)
    Public Property Success As Boolean
    Public Property Message As String = String.Empty
    Public Property StoppedBeforeSave As Boolean
    ''' <summary>The executor's variables at the end of the run, exactly as the runner returned them.</summary>
    Public Property Variables As New Dictionary(Of String, String)
End Class
