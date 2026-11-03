Option Strict On
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Text.Json
Imports System.Threading

''' <summary>What one finished copy of one database added to the store.</summary>
Public NotInheritable Class BackupRunSummary
    Public Property Db As String = String.Empty
    Public Property SnapshotId As String = String.Empty
    Public Property Mode As String = String.Empty
    Public Property Tables As Integer
    Public Property PagesNew As Integer
    Public Property PagesSame As Integer
    ''' <summary>Bytes written to the store by this copy (the new objects, compressed and encrypted).</summary>
    Public Property BytesNew As Long
    Public Property Rows As Long
    Public Property Seconds As Double
    ''' <summary>Where the time of this copy went; also saved under Logs\backup_profile\.</summary>
    Public Property Profile As BackupProfile
End Class

''' <summary>The outcome of one database in a run over several: a summary, or the readable reason it failed.</summary>
Public NotInheritable Class BackupRunResult
    Public Property Db As String = String.Empty
    Public Property Summary As BackupRunSummary
    Public Property [Error] As String = String.Empty
    Public ReadOnly Property Ok As Boolean
        Get
            Return Summary IsNot Nothing
        End Get
    End Property
End Class

''' <summary>
''' Takes one database from the server into the local store (slice ME02-03/04, docs/ME02_FORMAT.md): sends the pages of the
''' previous copy, reads the server's stream of page records, writes the new objects, and writes the snapshot LAST.
''' </summary>
Public NotInheritable Class BackupRunner

    Private Sub New()
    End Sub

    ''' <summary>Copies one database. kind = "manual" or "periodic"; mode = "fast" or "full". Raises on any failure; nothing is half-written
    ''' as a snapshot (a failed run leaves only objects no snapshot names).</summary>
    Public Shared Async Function RunAsync(k_client As AdminClient, k_store As BackupStore, k_db As String, k_kind As String, k_reason As String,
                                          k_mode As String, k_progress As Action(Of String), k_token As CancellationToken) As Task(Of BackupRunSummary)
        ' VB does not allow Await in a Finally block, so the clean-up of the file left on the server comes after the Try.
        Dim k_remoteFiles As New List(Of String)()
        Dim k_failure As Runtime.ExceptionServices.ExceptionDispatchInfo = Nothing
        Dim k_summary As BackupRunSummary = Nothing
        Dim k_profile As New BackupProfile With {.Db = k_db, .Mode = k_mode, .Kind = k_kind}
        Dim k_clock As Stopwatch = Stopwatch.StartNew()
        Try
            k_summary = Await RunCoreAsync(k_client, k_store, k_db, k_kind, k_reason, k_mode, k_progress, k_token, k_remoteFiles, k_profile).ConfigureAwait(True)
        Catch ex As Exception
            k_failure = Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex)
        End Try
        Dim k_cleanup As Stopwatch = Stopwatch.StartNew()
        For Each k_remote As String In k_remoteFiles
            Try
                Await k_client.DeleteRemoteAsync(k_remote).ConfigureAwait(True)   ' the server deletes it itself once read; this covers a run that failed before that
            Catch ex As Exception
                GlobalErrorLog.Write("BackupRunner.RunAsync.Cleanup", ex)
            End Try
        Next
        If k_remoteFiles.Count > 0 Then k_profile.AddPhase("ștergerea fișierului de stare de pe server", k_cleanup.ElapsedMilliseconds)
        k_profile.TotalMs = k_clock.ElapsedMilliseconds
        If k_failure IsNot Nothing Then k_profile.Error = k_failure.SourceException.Message
        k_profile.Save()
        If k_summary IsNot Nothing Then k_summary.Profile = k_profile
        If k_failure IsNot Nothing Then
            If Not (TypeOf k_failure.SourceException Is OperationCanceledException) Then GlobalErrorLog.Write("BackupRunner.RunAsync", k_failure.SourceException)
            k_failure.Throw()
        End If
        Return k_summary
    End Function

    Private Shared Async Function RunCoreAsync(k_client As AdminClient, k_store As BackupStore, k_db As String, k_kind As String, k_reason As String,
                                               k_mode As String, k_progress As Action(Of String), k_token As CancellationToken,
                                               k_remoteFiles As List(Of String), k_profile As BackupProfile) As Task(Of BackupRunSummary)
        Dim k_remoteState As String = String.Empty
        If k_mode <> "fast" AndAlso k_mode <> "full" Then Throw New ArgumentException("Mod necunoscut: " & k_mode, NameOf(k_mode))
            Dim k_phase As Stopwatch = Stopwatch.StartNew()
            Using k_lock As IDisposable = k_store.AcquireLock()
                Dim k_started As DateTimeOffset = DateTimeOffset.Now
                k_profile.Started = k_started
                Dim k_previous As SnapshotManifest = k_store.LatestSnapshot(k_db)
                k_profile.AddPhase("încuietoarea dosarului + citirea copiei precedente", k_phase.ElapsedMilliseconds)
                Dim k_args As New List(Of String) From {"--db", k_db, "--mode", k_mode}
                If k_previous IsNot Nothing Then
                    k_progress?.Invoke(k_db & ": se trimite serverului lista copiei precedente...")
                    k_phase.Restart()
                    Dim k_stateBytes As Byte() = BuildState(k_previous)
                    k_profile.AddPhase("alcătuirea listei copiei precedente (" & UiRun.FormatBytes(k_stateBytes.LongLength) & ")", k_phase.ElapsedMilliseconds)
                    k_remoteState = k_client.Settings.RemoteRoot.TrimEnd("/"c) & "/backup/.kbak_state_" & Guid.NewGuid().ToString("N") & ".json"
                    k_remoteFiles.Add(k_remoteState)
                    k_phase.Restart()
                    Await k_client.UploadBytesAsync(k_stateBytes, k_remoteState).ConfigureAwait(True)
                    k_profile.AddPhase("trimiterea listei pe server (conectare SFTP + încărcare)", k_phase.ElapsedMilliseconds)
                    k_args.Add("--state-file")
                    k_args.Add(k_remoteState)
                End If
                Dim k_state As New BackupRunState(k_store, k_previous, k_db, k_progress, k_token, k_profile)
                k_progress?.Invoke(k_db & ": se citește de pe server...")
                k_phase.Restart()
                Dim k_result As SshResult = Await k_client.StreamLinesAsync(AddressOf k_state.Handle, "snap-run", k_args.ToArray()).ConfigureAwait(True)
                k_profile.AddPhase("snap-run: pornirea comenzii + tot fluxul de la server", k_phase.ElapsedMilliseconds)
                If k_state.ServerError <> String.Empty Then Throw New AdminException(k_state.ServerError)
                If Not k_state.Ended Then
                    Throw New AdminException("Serverul a oprit citirea bazei " & k_db & " înainte de sfârșit (cod " & k_result.ExitStatus.ToString(CultureInfo.InvariantCulture) & "). " &
                                             k_result.StdErr.Trim() & " Nu s-a scris nicio copie.")
                End If
                Dim k_finished As DateTimeOffset = DateTimeOffset.Now
                Dim k_manifest As SnapshotManifest = k_state.Manifest
                k_manifest.Id = BackupStore.NewSnapshotId()
                k_manifest.Db = k_db
                k_manifest.Kind = k_kind
                k_manifest.Reason = k_reason
                k_manifest.Mode = k_mode
                k_manifest.Started = k_started.ToString("o", CultureInfo.InvariantCulture)
                k_manifest.Finished = k_finished.ToString("o", CultureInfo.InvariantCulture)
                k_manifest.PreviousId = If(k_previous Is Nothing, Nothing, k_previous.Id)
                k_manifest.Stats.Seconds = Math.Round((k_finished - k_started).TotalSeconds, 1)
                k_phase.Restart()
                k_store.PutSnapshot(k_manifest)
                k_profile.AddPhase("scrierea instantaneului pe disc", k_phase.ElapsedMilliseconds)
                k_progress?.Invoke(k_db & ": gata (" & k_manifest.Stats.PagesNew.ToString(CultureInfo.InvariantCulture) & " pagini noi, " &
                                   k_manifest.Stats.PagesSame.ToString(CultureInfo.InvariantCulture) & " neschimbate).")
                Return New BackupRunSummary With {
                    .Db = k_db, .SnapshotId = k_manifest.Id, .Mode = k_mode, .Tables = k_manifest.Tables.Count,
                    .PagesNew = k_manifest.Stats.PagesNew, .PagesSame = k_manifest.Stats.PagesSame, .BytesNew = k_manifest.Stats.BytesNew,
                    .Rows = k_manifest.Stats.Rows, .Seconds = k_manifest.Stats.Seconds
                }
            End Using
    End Function

    ''' <summary>Copies several databases one after the other. A database that fails does not stop the others.</summary>
    Public Shared Async Function RunManyAsync(k_client As AdminClient, k_store As BackupStore, k_dbs As IEnumerable(Of String), k_kind As String,
                                              k_reason As String, k_mode As String, k_progress As Action(Of String),
                                              k_token As CancellationToken) As Task(Of List(Of BackupRunResult))
        Dim k_results As New List(Of BackupRunResult)()
        For Each k_db As String In k_dbs
            k_token.ThrowIfCancellationRequested()
            Dim k_one As New BackupRunResult With {.Db = k_db}
            Try
                k_one.Summary = Await RunAsync(k_client, k_store, k_db, k_kind, k_reason, k_mode, k_progress, k_token).ConfigureAwait(True)
            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                k_one.Error = ex.Message
                k_progress?.Invoke(k_db & ": EȘUAT — " & ex.Message)
            End Try
            k_results.Add(k_one)
        Next
        Return k_results
    End Function

    ''' <summary>The pages of the previous copy, in the shape the server's snap-run reads (intervals and check values; no data).</summary>
    Private Shared Function BuildState(k_previous As SnapshotManifest) As Byte()
        Using k_stream As New MemoryStream()
            Using k_writer As New Utf8JsonWriter(k_stream)
                k_writer.WriteStartObject()
                k_writer.WritePropertyName("tables")
                k_writer.WriteStartObject()
                For Each k_table As BackupTable In k_previous.Tables
                    k_writer.WritePropertyName(k_table.Name)
                    k_writer.WriteStartObject()
                    k_writer.WritePropertyName("pk")
                    WriteKey(k_writer, k_table.PrimaryKey)
                    k_writer.WritePropertyName("pages")
                    k_writer.WriteStartArray()
                    For Each k_page As BackupPage In k_table.Pages
                        k_writer.WriteStartObject()
                        k_writer.WritePropertyName("lo")
                        WriteKey(k_writer, k_page.Lo)
                        k_writer.WritePropertyName("hi")
                        WriteKey(k_writer, k_page.Hi)
                        k_writer.WriteNumber("n", k_page.Rows)
                        k_writer.WriteString("sum", k_page.Sum)
                        k_writer.WriteString("full", k_page.Full)
                        k_writer.WriteEndObject()
                    Next
                    k_writer.WriteEndArray()
                    k_writer.WriteEndObject()
                Next
                k_writer.WriteEndObject()
                k_writer.WriteEndObject()
            End Using
            Return k_stream.ToArray()
        End Using
    End Function

    Private Shared Sub WriteKey(k_writer As Utf8JsonWriter, k_values As List(Of String))
        If k_values Is Nothing Then
            k_writer.WriteNullValue()
            Return
        End If
        k_writer.WriteStartArray()
        For Each k_value As String In k_values
            k_writer.WriteStringValue(k_value)
        Next
        k_writer.WriteEndArray()
    End Sub

End Class

''' <summary>Reads the server's lines of one run (one per call, on a worker thread) and writes what they carry into the store.</summary>
Friend NotInheritable Class BackupRunState

    Private ReadOnly _store As BackupStore
    Private ReadOnly _previousPages As New Dictionary(Of String, BackupPage)(StringComparer.Ordinal)
    Private ReadOnly _progress As Action(Of String)
    Private ReadOnly _token As CancellationToken
    Private ReadOnly _db As String
    Private _table As BackupTable
    Private _pagesSeen As Integer
    Private ReadOnly _profile As BackupProfile
    Private _tableProfile As BackupTableProfile
    Private ReadOnly _lap As Stopwatch = Stopwatch.StartNew()      ' from the end of the previous table (or the "begin" record) to now

    Public Sub New(k_store As BackupStore, k_previous As SnapshotManifest, k_db As String, k_progress As Action(Of String), k_token As CancellationToken,
                   k_profile As BackupProfile)
        _store = k_store
        _db = k_db
        _progress = k_progress
        _token = k_token
        _profile = k_profile
        Manifest = New SnapshotManifest()
        If k_previous IsNot Nothing Then
            For Each k_table As BackupTable In k_previous.Tables
                For Each k_page As BackupPage In k_table.Pages
                    _previousPages(PageKey(k_table.Name, k_page.Lo, k_page.Hi)) = k_page
                Next
            Next
        End If
    End Sub

    Public ReadOnly Property Manifest As SnapshotManifest
    Public Property Ended As Boolean
    Public Property ServerError As String = String.Empty

    Private Shared Function KeyText(k_values As List(Of String)) As String
        Return If(k_values Is Nothing, "~", "[" & String.Join(ChrW(31), k_values))
    End Function

    Private Shared Function PageKey(k_table As String, k_lo As List(Of String), k_hi As List(Of String)) As String
        Return k_table & ChrW(30) & KeyText(k_lo) & ChrW(30) & KeyText(k_hi)
    End Function

    Private Shared Function ReadKey(k_element As JsonElement, k_name As String) As List(Of String)
        Dim k_value As JsonElement
        If Not k_element.TryGetProperty(k_name, k_value) OrElse k_value.ValueKind = JsonValueKind.Null Then Return Nothing
        Dim k_list As New List(Of String)()
        For Each k_item As JsonElement In k_value.EnumerateArray()
            k_list.Add(k_item.GetString())
        Next
        Return k_list
    End Function

    Private Function Store(k_plain As Byte()) As BackupPutResult
        Dim k_put As BackupPutResult = _store.PutObject(k_plain)
        If k_put.IsNew Then Manifest.Stats.BytesNew += k_put.StoredBytes
        Return k_put
    End Function

    Private Shared Function IntOf(k_element As JsonElement, k_name As String) As Integer
        Dim k_value As JsonElement
        Return If(k_element.TryGetProperty(k_name, k_value) AndAlso k_value.ValueKind = JsonValueKind.Number, k_value.GetInt32(), 0)
    End Function

    ''' <summary>Called once per line of the server's stream; the time spent here is the client's share of the table being read.</summary>
    Public Sub Handle(k_line As String)
        Dim k_clock As Stopwatch = Stopwatch.StartNew()
        Try
            HandleLine(k_line)
        Finally
            If _tableProfile IsNot Nothing Then _tableProfile.ClientMs += k_clock.ElapsedMilliseconds
        End Try
    End Sub

    Private Sub HandleLine(k_line As String)
        _token.ThrowIfCancellationRequested()
        If String.IsNullOrEmpty(k_line) OrElse k_line(0) <> "{"c Then Return        ' stderr lines and anything that is not a record
        Using k_doc As JsonDocument = JsonDocument.Parse(k_line)
            Dim k_root As JsonElement = k_doc.RootElement
            Dim k_type As JsonElement
            If k_root.TryGetProperty("t", k_type) Then
                Select Case k_type.GetString()
                    Case "begin"
                        Manifest.ServerTime = k_root.GetProperty("server_time").GetString()
                        Manifest.ServerVersion = k_root.GetProperty("version").GetString()
                        Dim k_prof As JsonElement
                        _profile.ServerProfiled = k_root.TryGetProperty("prof", k_prof)
                        _profile.ServerListMs = IntOf(k_root, "list_ms")
                        _profile.StateTables = IntOf(k_root, "state_tables")
                        _lap.Restart()
                    Case "table"
                        StartTable(k_root)
                    Case "page"
                        AddPage(k_root)
                    Case "tableend"
                        EndTableProfile(k_root)
                        FinishTable()
                    Case "extra"
                        Dim k_ddl As BackupPutResult = Store(Encoding.UTF8.GetBytes(k_root.GetProperty("ddl").GetString()))
                        Manifest.Extras.Add(New BackupExtra With {
                            .Kind = k_root.GetProperty("kind").GetString(), .Name = k_root.GetProperty("name").GetString(), .DdlObject = k_ddl.Name})
                    Case "end"
                        _profile.ServerTablesMs = IntOf(k_root, "tables_ms")
                        _profile.ServerExtrasMs = IntOf(k_root, "extras_ms")
                        Ended = True
                End Select
            Else
                Dim k_ok As JsonElement
                If k_root.TryGetProperty("ok", k_ok) AndAlso k_ok.ValueKind = JsonValueKind.False Then
                    Dim k_message As JsonElement
                    ServerError = If(k_root.TryGetProperty("error", k_message), k_message.GetString(), "Eroare necunoscută de la server.")
                End If
            End If
        End Using
    End Sub

    Private Sub StartTable(k_root As JsonElement)
        FinishTable()
        _table = New BackupTable With {.Name = k_root.GetProperty("name").GetString()}
        For Each k_col As JsonElement In k_root.GetProperty("cols").EnumerateArray()
            _table.Columns.Add(New BackupColumn With {
                .Name = k_col.GetProperty("n").GetString(), .DataType = k_col.GetProperty("dt").GetString(), .IsBinary = k_col.GetProperty("bin").GetBoolean()})
        Next
        For Each k_key As JsonElement In k_root.GetProperty("pk").EnumerateArray()
            _table.PrimaryKey.Add(k_key.GetString())
        Next
        Dim k_mod As JsonElement = k_root.GetProperty("modcol")
        If k_mod.ValueKind = JsonValueKind.String Then _table.ModifiedColumn = k_mod.GetString()
        _table.DdlObject = Store(Encoding.UTF8.GetBytes(k_root.GetProperty("ddl").GetString())).Name
        _pagesSeen = 0
        _tableProfile = New BackupTableProfile With {.Name = _table.Name}
        _profile.Tables.Add(_tableProfile)
        _progress?.Invoke(_db & ": tabela " & _table.Name)
    End Sub

    ''' <summary>The server's timings for the table that just ended (when it sends them) and the wall time the client saw for it.</summary>
    Private Sub EndTableProfile(k_root As JsonElement)
        If _tableProfile Is Nothing Then Return
        _tableProfile.WallMs = _lap.ElapsedMilliseconds
        _lap.Restart()
        Dim k_prof As JsonElement
        If Not k_root.TryGetProperty("prof", k_prof) Then Return
        Dim k_action As JsonElement
        If k_prof.TryGetProperty("action", k_action) Then _tableProfile.Action = k_action.GetString()
        _tableProfile.ServerTotalMs = IntOf(k_prof, "total_ms")
        _tableProfile.DescribeMs = IntOf(k_prof, "describe_ms")
        _tableProfile.CountMs = IntOf(k_prof, "count_ms")
        _tableProfile.CheckMs = IntOf(k_prof, "check_ms")
        _tableProfile.ReadMs = IntOf(k_prof, "read_ms")
        _tableProfile.EmitMs = IntOf(k_prof, "emit_ms")
        _tableProfile.Checks = IntOf(k_prof, "checks")
    End Sub

    Private Sub AddPage(k_root As JsonElement)
        If _table Is Nothing Then Throw New AdminException("Serverul a trimis o pagină fără tabel.")
        Dim k_page As New BackupPage With {
            .Lo = ReadKey(k_root, "lo"), .Hi = ReadKey(k_root, "hi"),
            .Rows = k_root.GetProperty("n").GetInt32(), .Sum = k_root.GetProperty("sum").GetString(), .Full = k_root.GetProperty("full").GetString()
        }
        If k_root.GetProperty("same").GetBoolean() Then
            Dim k_old As BackupPage = Nothing
            If Not _previousPages.TryGetValue(PageKey(_table.Name, k_page.Lo, k_page.Hi), k_old) Then
                Throw New AdminException("Serverul a spus că o pagină din " & _table.Name & " e neschimbată, dar copia precedentă nu o are. Nu s-a scris nicio copie.")
            End If
            k_page.ObjectName = k_old.ObjectName
            k_page.Bytes = k_old.Bytes
            If Not _store.HasObject(k_page.ObjectName) Then
                Throw New AdminException("Copia precedentă nu mai are un obiect din " & _table.Name & ". Rulați o verificare a copiei locale.")
            End If
            Manifest.Stats.PagesSame += 1
        Else
            Dim k_rows As Byte() = Inflate(Convert.FromBase64String(k_root.GetProperty("z").GetString()))
            k_page.ObjectName = Store(k_rows).Name
            k_page.Bytes = k_rows.Length
            Manifest.Stats.PagesNew += 1
        End If
        If _tableProfile IsNot Nothing Then
            _tableProfile.Pages += 1
            _tableProfile.Rows += k_page.Rows
            If k_root.GetProperty("same").GetBoolean() Then _tableProfile.PagesSame += 1 Else _tableProfile.PagesNew += 1
        End If
        _table.Pages.Add(k_page)
        _table.Rows += k_page.Rows
        Manifest.Stats.Rows += k_page.Rows
        _pagesSeen += 1
    End Sub

    Private Sub FinishTable()
        If _table Is Nothing Then Return
        Manifest.Tables.Add(_table)
        _table = Nothing
        _tableProfile = Nothing
    End Sub

    Private Shared Function Inflate(k_compressed As Byte()) As Byte()
        Using k_in As New MemoryStream(k_compressed)
            Using k_zlib As New ZLibStream(k_in, CompressionMode.Decompress)
                Using k_out As New MemoryStream()
                    k_zlib.CopyTo(k_out)
                    Return k_out.ToArray()
                End Using
            End Using
        End Using
    End Function

End Class
