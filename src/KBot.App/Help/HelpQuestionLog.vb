Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports KBot.Api
Imports KBot.Common

''' <summary>
''' One question typed in the help (slice 0000-21) and what came of it. Kept by the search
''' session while the operator works with it; written to the waiting list at every change.
''' </summary>
Friend NotInheritable Class HelpQuestion

    Public Const MaxLength As Integer = 300
    Public Const MaxHits As Integer = 5

    ''' <summary>A fresh random id: nothing about the user, the unit, the PC or the session.</summary>
    Public ReadOnly Qid As String = Guid.NewGuid().ToString("D")
    Public ReadOnly AskedUtc As DateTime = DateTime.UtcNow
    Public ReadOnly Where As String
    Public Text As String = String.Empty
    Public Hits As New List(Of String)()
    Public Opened As String
    Public Action As String
    Public Rating As Integer

    ''' <summary>True once it has been written to the waiting list.</summary>
    Public Saved As Boolean

    Public Sub New(where As String)
        Me.Where = where
    End Sub

    ''' <summary>The question as it goes on the wire.</summary>
    Public Function ToRow(parts As String, appVersion As String, helpVersion As String) As HelpFeedbackRow
        Return New HelpFeedbackRow With {
            .qid = Qid,
            .question = If(Text.Length > MaxLength, Text.Substring(0, MaxLength), Text),
            .asked_utc = AskedUtc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            .parts = parts,
            .app_version = appVersion,
            .help_version = helpVersion,
            .hits = Hits.Take(MaxHits).ToList(),
            .opened = Opened,
            .action = Action,
            .rating = If(Rating >= 1 AndAlso Rating <= 5, Rating, CType(Nothing, Integer?)),
            .where = Where}
    End Function

End Class

''' <summary>
''' The help's question log (slice 0000-21): every question goes to the server, without anything
''' about who asked or where from.
'''
''' <para><b>Waiting list.</b> One small JSON file per question, named by its random id, in
''' <c>%APPDATA%\AVACONT\KBot\HelpOutbox\</c>. Written through a temporary file and a rename, so a
''' crash leaves the old version or the new one, never half a file; the files are still there
''' after a restart without network. A later change to the same question (a click, a rating)
''' overwrites its file. The screen never waits for any of this.</para>
'''
''' <para><b>Sending.</b> In the background, in batches of at most <see cref="BatchSize"/>: every
''' <see cref="FlushMinutes"/> minutes, as soon as <see cref="FlushAt"/> questions wait, and once
''' more when K-BOT closes (a few seconds at most). Only while logged in. A file is deleted only
''' after the server confirmed the batch, and only if the question did not change meanwhile. A
''' failure is logged without the question text and tried again later.</para>
''' </summary>
Friend NotInheritable Class HelpQuestionLog
    Implements IDisposable

    Private Const BatchSize As Integer = 50
    Private Const FlushAt As Integer = 10
    Private Const FlushMinutes As Integer = 3
    Private Const ExitWaitSeconds As Integer = 3

    Private ReadOnly _api As IHelpFeedbackApi
    Private ReadOnly _session As SessionContext
    Private ReadOnly _folder As String
    Private ReadOnly _sync As New Object()
    ' Bumped at every write of a question; a file is deleted only if unchanged since it was sent.
    Private ReadOnly _versions As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _timer As System.Threading.Timer
    Private _flushing As Integer
    Private _disposed As Boolean

    Private Shared ReadOnly JsonOptions As New JsonSerializerOptions With {.PropertyNamingPolicy = Nothing}

    Public Sub New(api As IHelpFeedbackApi, session As SessionContext)
        _api = api
        _session = session
        _folder = Path.Combine(SetariFoldere.DirectorSetari(), "HelpOutbox")
        Dim period As TimeSpan = TimeSpan.FromMinutes(FlushMinutes)
        _timer = New System.Threading.Timer(AddressOf OnTimer, Nothing, period, period)
    End Sub

    ''' <summary>Writes (or rewrites) <paramref name="q"/> in the waiting list. Never throws to the screen.</summary>
    Public Sub Save(q As HelpQuestion, parts As String, appVersion As String, helpVersion As String)
        Try
            If q Is Nothing OrElse q.Text.Length < 2 Then Return
            Dim json As String = JsonSerializer.Serialize(q.ToRow(parts, appVersion, helpVersion), JsonOptions)
            Dim waiting As Integer
            SyncLock _sync
                Directory.CreateDirectory(_folder)
                Dim target As String = FileFor(q.Qid)
                Dim temp As String = target & ".tmp"
                File.WriteAllText(temp, json, New UTF8Encoding(False))
                File.Move(temp, target, overwrite:=True)
                Dim v As Integer = 0
                _versions.TryGetValue(q.Qid, v)
                _versions(q.Qid) = v + 1
                waiting = Directory.GetFiles(_folder, "*.json").Length
            End SyncLock
            q.Saved = True
            If waiting >= FlushAt Then FlushInBackground()
        Catch ex As Exception
            ' Logged without the question: the type and the waiting-list folder only.
            GlobalErrorLog.Write("HelpQuestionLog.Save", New IOException("A help question could not be written to the waiting list (" & ex.GetType().Name & ")."))
        End Try
    End Sub

    Private Function FileFor(qid As String) As String
        Return Path.Combine(_folder, qid & ".json")
    End Function

    Private Sub OnTimer(state As Object)
        FlushInBackground()
    End Sub

    ''' <summary>Starts a send if none is running; returns at once.</summary>
    Public Sub FlushInBackground()
        If _disposed Then Return
        Task.Run(Function() FlushAsync(CancellationToken.None))
    End Sub

    ''' <summary>When K-BOT closes: one last send, waited for a few seconds at most.</summary>
    Public Sub FlushOnExit()
        Try
            Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(ExitWaitSeconds))
                Dim t As Task = FlushAsync(cts.Token)
                t.Wait(TimeSpan.FromSeconds(ExitWaitSeconds + 1))
            End Using
        Catch ex As Exception
            ' Whatever did not go stays in the waiting list for the next start.
            GlobalErrorLog.Write("HelpQuestionLog.FlushOnExit", New InvalidOperationException("Help questions not sent at exit (" & ex.GetType().Name & ")."))
        End Try
    End Sub

    ' Sends every waiting question, batch after batch, until the list is empty or a batch fails.
    Private Async Function FlushAsync(ct As CancellationToken) As Task
        If Interlocked.Exchange(_flushing, 1) = 1 Then Return
        Try
            If _api Is Nothing OrElse _session Is Nothing OrElse String.IsNullOrEmpty(_session.Token) Then Return
            Do
                Dim batch As New List(Of HelpFeedbackRow)()
                Dim sent As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
                SyncLock _sync
                    If Not Directory.Exists(_folder) Then Return
                    Dim files As List(Of FileInfo) = New DirectoryInfo(_folder).GetFiles("*.json") _
                        .OrderBy(Function(f) f.LastWriteTimeUtc).Take(BatchSize).ToList()
                    For Each f As FileInfo In files
                        Dim qid As String = Path.GetFileNameWithoutExtension(f.Name)
                        Dim row As HelpFeedbackRow = Nothing
                        Try
                            row = JsonSerializer.Deserialize(Of HelpFeedbackRow)(File.ReadAllText(f.FullName, Encoding.UTF8), JsonOptions)
                        Catch ex As JsonException
                            row = Nothing
                        End Try
                        If row Is Nothing OrElse Not String.Equals(row.qid, qid, StringComparison.OrdinalIgnoreCase) Then
                            ' A file that cannot be read would block the list for good: it goes.
                            File.Delete(f.FullName)
                            GlobalErrorLog.Write("HelpQuestionLog.FlushAsync", New InvalidDataException("An unreadable help question file was removed from the waiting list."))
                            Continue For
                        End If
                        batch.Add(row)
                        Dim v As Integer = 0
                        _versions.TryGetValue(qid, v)
                        sent(qid) = v
                    Next
                End SyncLock
                If batch.Count = 0 Then Return

                Await _api.SendHelpFeedbackAsync(batch, ct).ConfigureAwait(False)

                SyncLock _sync
                    For Each kv As KeyValuePair(Of String, Integer) In sent
                        Dim now As Integer = 0
                        _versions.TryGetValue(kv.Key, now)
                        If now <> kv.Value Then Continue For   ' changed while on the wire: send again
                        Dim f As String = FileFor(kv.Key)
                        If File.Exists(f) Then File.Delete(f)
                        _versions.Remove(kv.Key)
                    Next
                End SyncLock
                If batch.Count < BatchSize Then Return
            Loop
        Catch ex As OperationCanceledException
            ' Closing, or the server did not answer in time: the list waits for the next try.
            GlobalErrorLog.Write("HelpQuestionLog.FlushAsync", New OperationCanceledException("Help questions not sent in time; they wait for the next try."))
        Catch ex As ApiException
            Dim status As String = If(ex.StatusCode.HasValue, ex.StatusCode.Value.ToString(CultureInfo.InvariantCulture), "-")
            GlobalErrorLog.Write("HelpQuestionLog.FlushAsync", New InvalidOperationException("Help questions not sent: HTTP " & status & "."))
        Catch ex As Exception
            GlobalErrorLog.Write("HelpQuestionLog.FlushAsync", New InvalidOperationException("Help questions not sent (" & ex.GetType().Name & ")."))
        Finally
            Interlocked.Exchange(_flushing, 0)
        End Try
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        _disposed = True
        _timer.Dispose()
    End Sub

End Class
