Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' What one schema-sync run produced.
''' </summary>
Public NotInheritable Class SchemaSyncResult

    Public Sub New(succeeded As Boolean, refused As Boolean, statements As Integer,
                   destructive As Integer, executed As Integer, failed As Integer,
                   sqlFile As String)
        Me.Succeeded = succeeded
        Me.Refused = refused
        Me.Statements = statements
        Me.Destructive = destructive
        Me.Executed = executed
        Me.Failed = failed
        Me.SqlFile = If(sqlFile, String.Empty)
    End Sub

    ''' <summary>Everything that was planned ran, and nothing failed.</summary>
    Public ReadOnly Property Succeeded As Boolean

    ''' <summary>
    ''' The plan contained destructive statements and the run was refused whole.
    ''' </summary>
    ''' <remarks>
    ''' Not a failure of the server and not something to retry blindly - it means the target
    ''' is not the empty database this tool thought it was. See the remarks on
    ''' <see cref="SchemaSyncClient"/>.
    ''' </remarks>
    Public ReadOnly Property Refused As Boolean

    ''' <summary>How many DDL statements the comparison produced. Zero = already in step.</summary>
    Public ReadOnly Property Statements As Integer
    Public ReadOnly Property Destructive As Integer
    Public ReadOnly Property Executed As Integer
    Public ReadOnly Property Failed As Integer

    ''' <summary>The .sql file the server wrote, ON THE SERVER. Empty when nothing was planned.</summary>
    Public ReadOnly Property SqlFile As String

End Class

''' <summary>
''' Runs the schema tool ON THE SERVER, over the migration API, to build a target database's
''' structure.
''' </summary>
''' <remarks>
''' <para>
''' Used for one case only: the target database EXISTS but holds no tables. Creating it
''' from <c>AVACONT_SURSA</c> (plan §4) is the migrator's own path; this is the other one,
''' for a database somebody already created empty.
''' </para>
''' <para>
''' <b>The work happens on the VPS, never on the operator's machine.</b> That is not a
''' preference: the schema tool is Python, it lives beside the databases, and the deployed
''' copy there - with its own <c>config.py</c> and its own interpreter - is the one allowed
''' to alter them. Driving a local checkout instead would be a different codebase pointed at
''' the same server.
''' </para>
''' <para>
''' <b>What this is NOT any more.</b> Until this slice the same job was started by opening an
''' SSH session and typing a command line. That meant every operator needed a shell account
''' on the server and a private key file on their machine, to run one thing, once, on a
''' database that happened to be empty. The server now exposes the step as a route -
''' <c>POST /api/migrare/schema-sync</c> - guarded by the <c>X-Api-Key</c> that already
''' guards every other migration route. No shell, no key file, no second credential:
''' the same header the rest of the migration API uses.
''' </para>
''' <para>
''' <b>Long work behind a short request.</b> The route starts a job and answers at once with
''' its id; this class then polls <c>/api/migrare/stare/&lt;id&gt;</c> and copies each new
''' journal line into the form's log, so the operator watches the run rather than a frozen
''' window. <c>de_la</c> carries how many lines have already been read, so a poll brings back
''' only what is new.
''' </para>
''' <para>
''' <b>Cancelling stops the WATCHING, not the run.</b> There is no route that stops a job
''' half-way, and there could not honestly be one: DDL commits implicitly in MariaDB, so a
''' stopped run is a half-applied schema either way. Cancelling here is the same promise the
''' SSH version made when it killed the channel - the form stops waiting, the server finishes
''' or fails on its own, and the re-verify that follows is what finds out which. The message
''' says so.
''' </para>
''' <para>
''' <b>SAFE, not FORCE.</b> The offer only appears for a database with no tables at all, and
''' on a table-less database the two modes are provably identical: there is nothing to modify
''' and nothing to drop, so every statement is a CREATE either way. SAFE is what gets sent
''' because it is the mode that stays right if this is ever pointed somewhere else by mistake.
''' In the same spirit the destructive gate is left shut - <c>permite_distructive</c> is never
''' sent. A plan for an "empty" database that turns out to contain a DROP means the database
''' was not empty, and that is a thing to look at, not to wave through from a message box.
''' </para>
''' <para>
''' No MariaDB credential travels this way. The server talks to the databases through its own
''' configuration, exactly as the command line does.
''' </para>
''' </remarks>
Public NotInheritable Class SchemaSyncClient
    Implements IDisposable

    ''' <summary>How long between two polls of the job's state.</summary>
    Private Const PollMilliseconds As Integer = 700

    ''' <summary>
    ''' What a DC name is allowed to look like before it is put in a request.
    ''' </summary>
    ''' <remarks>
    ''' The server checks this too, and its check is the one that counts. This one is here so
    ''' a name that could never work is refused before a job is started and polled for it.
    ''' </remarks>
    Private Shared ReadOnly SafeDc As New Regex("^[0-9]{3}_[A-Za-z0-9]+$", RegexOptions.Compiled)

    Private ReadOnly _http As HttpClient
    Private ReadOnly _baseUrl As String
    Private ReadOnly _log As Action(Of String)
    Private _disposed As Boolean

    Public Sub New(baseUrl As String, apiKey As String, log As Action(Of String))
        Dim reason As String = Nothing
        If Not Validate(baseUrl, apiKey, reason) Then Throw New ArgumentException(reason)

        _baseUrl = NormaliseUrl(baseUrl)
        _log = log
        _http = New HttpClient() With {.Timeout = TimeSpan.FromSeconds(120)}
        _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey.Trim())
    End Sub

    ''' <summary>
    ''' Checks the address and the key, so a misconfiguration is reported in Romanian instead
    ''' of as a URI exception or a bare 401.
    ''' </summary>
    ''' <remarks>
    ''' https only, like every other client in the solution: the key travels in a header, and
    ''' a header on plain http is a key handed to whoever is on the wire.
    ''' </remarks>
    Public Shared Function Validate(baseUrl As String, apiKey As String, ByRef reason As String) As Boolean
        reason = String.Empty

        Dim address = If(baseUrl, String.Empty).Trim()
        If address.Length = 0 Then
            reason = "Adresa serverului nu este configurată. Se scrie ca «https://server.exemplu.ro» " &
                     "în «Adresă server», în grupul «Server Python»."
            Return False
        End If

        Dim uri As Uri = Nothing
        If Not Uri.TryCreate(address, UriKind.Absolute, uri) Then
            reason = $"Adresa «{address}» nu este o adresă web validă."
            Return False
        End If
        If Not String.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) Then
            reason = $"Adresa «{address}» nu folosește «https». Cheia API călătorește într-un " &
                     "antet, deci nu pleacă niciodată necriptată."
            Return False
        End If

        If String.IsNullOrWhiteSpace(apiKey) Then
            reason = "Cheia API lipsește. Se tastează în «Cheie API», în grupul «Server Python», " &
                     "și nu se păstrează nicăieri după închiderea ferestrei."
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Builds one database's structure on the server, streaming the job's journal into the log.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">
    ''' The DC name is not one this route accepts, or the server refused the request.
    ''' </exception>
    Public Async Function RunAsync(dc As String, cancel As CancellationToken) As Task(Of SchemaSyncResult)
        Try
            If String.IsNullOrWhiteSpace(dc) OrElse Not SafeDc.IsMatch(dc) Then
                Throw New InvalidOperationException(
                    $"Numele bazei «{dc}» nu are forma unei baze de unitate («000_DEMO»), deci " &
                    "sincronizarea de schemă nu poate fi cerută pentru ea.")
            End If

            Say($"Se pornește sincronizarea de schemă PE SERVER, pentru «{dc}».")
            Say($"   server: {_baseUrl}   mod: SAFE")

            Dim jobId = Await StartAsync(dc, cancel).ConfigureAwait(False)
            Say($"   lucrarea «{jobId}» a pornit. Se urmărește jurnalul ei.")

            Return Await WatchAsync(jobId, cancel).ConfigureAwait(False)

        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaSyncClient.RunAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Asks the server to start the job. Returns its id.</summary>
    Private Async Function StartAsync(dc As String, cancel As CancellationToken) As Task(Of String)
        ' Only «dc» and «mod» are sent. «permite_distructive» and «doar_vezi» are deliberately
        ' left off rather than sent as False - the server's defaults are the shut gate and the
        ' real run, and naming them here would invite somebody to flip one from a message box.
        Dim payload = BuildJson(Sub(w)
                                    w.WriteString("dc", dc)
                                    w.WriteString("mod", "SAFE")
                                End Sub)

        Dim body = Await PostJsonAsync(_baseUrl & "/api/migrare/schema-sync", payload, cancel).ConfigureAwait(False)
        Using doc = JsonDocument.Parse(body)
            Dim id = ReadString(doc.RootElement, "lucrare")
            If id.Length = 0 Then
                Throw New InvalidOperationException(
                    "Serverul a acceptat cererea, dar nu a întors nicio lucrare de urmărit.")
            End If
            Return id
        End Using
    End Function

    ''' <summary>
    ''' Polls the job until it ends, copying every new journal line into the log.
    ''' </summary>
    Private Async Function WatchAsync(jobId As String, cancel As CancellationToken) As Task(Of SchemaSyncResult)
        Dim seen = 0

        While Not cancel.IsCancellationRequested
            Dim url = _baseUrl & "/api/migrare/stare/" & Uri.EscapeDataString(jobId) &
                      "?de_la=" & seen.ToString(Globalization.CultureInfo.InvariantCulture)
            Dim body = Await GetStringAsync(url, cancel).ConfigureAwait(False)

            Dim state As String
            Dim failure As String
            Dim result As SchemaSyncResult = Nothing

            Using doc = JsonDocument.Parse(body)
                Dim job As JsonElement
                If Not doc.RootElement.TryGetProperty("lucrare", job) Then
                    Throw New InvalidOperationException("Serverul nu a întors starea lucrării.")
                End If

                Dim lines As JsonElement
                If job.TryGetProperty("jurnal", lines) AndAlso lines.ValueKind = JsonValueKind.Array Then
                    For Each line As JsonElement In lines.EnumerateArray()
                        If line.ValueKind <> JsonValueKind.String Then Continue For
                        Say("   | " & If(line.GetString(), String.Empty))
                        seen += 1
                    Next
                End If

                state = ReadString(job, "stare")
                failure = ReadString(job, "eroare")

                Dim payload As JsonElement
                If job.TryGetProperty("rezultat", payload) AndAlso payload.ValueKind = JsonValueKind.Object Then
                    result = ReadResult(payload)
                End If
            End Using

            ' Romanian on the wire, because the server's own vocabulary is Romanian: the
            ' three job states are spelled out in PYTHON/routes/migrare/jobs.py.
            If String.Equals(state, "eroare", StringComparison.Ordinal) Then
                Throw New InvalidOperationException(
                    If(failure.Length > 0, failure,
                       "Sincronizarea de schemă a eșuat pe server, fără mesaj."))
            End If

            If String.Equals(state, "gata", StringComparison.Ordinal) Then
                If result Is Nothing Then
                    Throw New InvalidOperationException(
                        "Lucrarea s-a încheiat, dar serverul nu a întors niciun rezultat.")
                End If
                SayOutcome(result)
                Return result
            End If

            Try
                Await Task.Delay(PollMilliseconds, cancel).ConfigureAwait(False)
            Catch ex As OperationCanceledException
                ' Cancelling almost always lands HERE - most of a run is spent waiting out
                ' this delay, not inside a request. Caught rather than let out, so the path
                ' leaves through the message below instead of throwing past it.
                Exit While
            End Try
        End While

        ' The loop is left only by a return, a throw, or a cancellation. This is that last
        ' case, and it is the one place the honest message belongs.
        Say("Oprire cerută — se încheie urmărirea. Lucrarea CONTINUĂ pe server: DDL-ul nu se " &
            "poate anula, deci ea merge până la capăt sau eșuează singură. Reluați verificarea " &
            "ca să vedeți cum a rămas baza.")
        Throw New OperationCanceledException(cancel)
    End Function

    ''' <summary>Says what the run came to, in the operator's words rather than in numbers.</summary>
    Private Sub SayOutcome(result As SchemaSyncResult)
        If result.Statements = 0 Then
            Say("Structura era deja la zi: nu s-a găsit nicio diferență față de șablon.")
        ElseIf result.Refused Then
            Say($"Rularea a fost REFUZATĂ: din cele {result.Statements} instrucțiuni, " &
                $"{result.Destructive} sunt distructive, iar o bază despre care credeam că " &
                "e goală nu ar trebui să aibă nimic de șters. Nu s-a executat nimic.")
        Else
            Say($"Sincronizare încheiată: {result.Executed} instrucțiuni reușite, " &
                $"{result.Failed} eșuate, din {result.Statements}.")
        End If
        If result.SqlFile.Length > 0 Then
            Say($"   Instrucțiunile au rămas scrise pe server, în «{result.SqlFile}».")
        End If
    End Sub

    Private Shared Function ReadResult(payload As JsonElement) As SchemaSyncResult
        Return New SchemaSyncResult(
            succeeded:=ReadBool(payload, "reusit"),
            refused:=ReadBool(payload, "refuzat"),
            statements:=ReadInt(payload, "instructiuni"),
            destructive:=ReadInt(payload, "distructive"),
            executed:=ReadInt(payload, "reusite"),
            failed:=ReadInt(payload, "esuate"),
            sqlFile:=ReadString(payload, "fisier_sql"))
    End Function

    ' ---- transport ---------------------------------------------------------------------
    ' Private, reached only through the public methods already wrapped above.

    Private Async Function GetStringAsync(url As String, cancel As CancellationToken) As Task(Of String)
        Using resp = Await _http.GetAsync(url, cancel).ConfigureAwait(False)
            Dim body = Await resp.Content.ReadAsStringAsync(cancel).ConfigureAwait(False)
            EnsureOk(resp, body, url)
            Return body
        End Using
    End Function

    Private Async Function PostJsonAsync(url As String, payload As String,
                                         cancel As CancellationToken) As Task(Of String)
        Using content As New StringContent(payload, Encoding.UTF8, "application/json")
            Using resp = Await _http.PostAsync(url, content, cancel).ConfigureAwait(False)
                Dim body = Await resp.Content.ReadAsStringAsync(cancel).ConfigureAwait(False)
                EnsureOk(resp, body, url)
                Return body
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Turns a failed response into a Romanian message the operator can act on.
    ''' </summary>
    ''' <remarks>
    ''' The migration routes answer with a Romanian «error» field, so that is what is shown.
    ''' 401 is the exception: <c>require_api_key</c> answers a bare English «Unauthorized»,
    ''' which tells an operator nothing about which of the two boxes is wrong.
    ''' </remarks>
    Private Shared Sub EnsureOk(resp As HttpResponseMessage, body As String, url As String)
        If resp.IsSuccessStatusCode Then Return

        If resp.StatusCode = HttpStatusCode.Unauthorized Then
            Throw New InvalidOperationException(
                "Serverul a refuzat cheia API. Verificați «Cheie API» din grupul " &
                "«Server Python» — este cheia serverului, nu parola MariaDB și nu parola " &
                "fișierelor Access.")
        End If

        Dim message = String.Empty
        Try
            Using doc = JsonDocument.Parse(body)
                message = ReadString(doc.RootElement, "error")
            End Using
        Catch ex As JsonException
            ' A body that is not JSON at all - a proxy, or an error page from something in
            ' front of Flask. Nothing to read out of it; the generic message says the code.
            message = String.Empty
        End Try

        If message.Length = 0 Then
            message = $"Serverul a răspuns {CInt(resp.StatusCode)} la «{url}»."
        End If
        Throw New InvalidOperationException(message)
    End Sub

    ''' <summary>Trims the trailing slash, so the paths below concatenate cleanly.</summary>
    Private Shared Function NormaliseUrl(baseUrl As String) As String
        Return baseUrl.Trim().TrimEnd("/"c)
    End Function

    Private Shared Function BuildJson(write As Action(Of Utf8JsonWriter)) As String
        Using stream As New IO.MemoryStream()
            Using writer As New Utf8JsonWriter(stream)
                writer.WriteStartObject()
                write(writer)
                writer.WriteEndObject()
            End Using
            Return Encoding.UTF8.GetString(stream.ToArray())
        End Using
    End Function

    Private Shared Function ReadString(parent As JsonElement, name As String) As String
        Dim value As JsonElement
        If parent.TryGetProperty(name, value) AndAlso value.ValueKind = JsonValueKind.String Then
            Return If(value.GetString(), String.Empty)
        End If
        Return String.Empty
    End Function

    Private Shared Function ReadInt(parent As JsonElement, name As String) As Integer
        Dim value As JsonElement
        Dim number As Integer
        If parent.TryGetProperty(name, value) AndAlso value.ValueKind = JsonValueKind.Number _
           AndAlso value.TryGetInt32(number) Then
            Return number
        End If
        Return 0
    End Function

    Private Shared Function ReadBool(parent As JsonElement, name As String) As Boolean
        Dim value As JsonElement
        If parent.TryGetProperty(name, value) Then
            If value.ValueKind = JsonValueKind.True Then Return True
            If value.ValueKind = JsonValueKind.False Then Return False
        End If
        Return False
    End Function

    Private Sub Say(message As String)
        _log?.Invoke(message)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _http.Dispose()
    End Sub

End Class
