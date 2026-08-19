Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common

''' <summary>Rezultatul unei scrieri de chunk pe <c>/api/forexe/seed/rows</c>.</summary>
Public NotInheritable Class SeedWriteResult
    Public Property Received As Integer
    Public Property Inserted As Integer
    Public Property Skipped As Integer
End Class

''' <summary>Rezultatul unei verificări de id-uri pe <c>/api/forexe/seed/ids</c>.</summary>
Public NotInheritable Class SeedIdsResult
    Public Property Found As New List(Of Long)()
    Public Property Missing As New List(Of Long)()
End Class

''' <summary>
''' Clientul HTTP al rutelor de seed. Transportul e HTTP prin Flask, NU o conexiune
''' MariaDB directă.
'''
''' Garda rutelor e <c>X-Api-Key</c> (flota FOREXE veche), nu tokenul bearer — acela e
''' exclusiv pentru K-BOT/VB.NET. De aceea NU se folosește <c>ApiClient</c> din KBot.Api:
''' de acolo se ia doar adresa serverului, care are un singur loc unde e scrisă.
'''
''' Toate metodele sunt de graniță (HTTP + JSON): logăm și RE-ARUNCĂM.
''' </summary>
Public NotInheritable Class SeedApiClient
    Implements IDisposable

    ' Plafoanele serverului (PYTHON/routes/forexe/seed.py). Ținute aici ca să nu trimitem
    ' niciodată o cerere despre care știm dinainte că va lua 400.
    Public Const MaxRowsPerRequest As Integer = 1000
    Public Const MaxIdsPerRequest As Integer = 1000

    Private ReadOnly _http As HttpClient
    Private ReadOnly _baseUrl As String
    Private _disposed As Boolean

    Public Sub New(apiKey As String, Optional baseUrl As String = Nothing, Optional timeoutSeconds As Integer = 300)
        If String.IsNullOrWhiteSpace(apiKey) Then
            Throw New ArgumentException("Cheia API (X-Api-Key) lipsește.", NameOf(apiKey))
        End If

        Dim opts As New ApiOptions()
        If Not String.IsNullOrWhiteSpace(baseUrl) Then opts.BaseUrl = baseUrl
        ' Aceeași gardă https ca restul soluției: nicio cheie nu pleacă necriptată.
        opts.EnsureHttpsBaseUrl()
        _baseUrl = opts.BaseUrl.TrimEnd("/"c)

        _http = New HttpClient() With {.Timeout = TimeSpan.FromSeconds(timeoutSeconds)}
        _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey)
    End Sub

    ''' <summary>
    ''' <c>GET /api/forexe/seed/columns</c> — lista REALĂ de coloane din MariaDB. Listă
    ''' goală = tabelul nu există în baza aceea (200 cu listă goală e răspuns valid, vezi
    ''' SLICE-0012-01), ceea ce pentru migrare e o eroare: schema se instalează separat.
    ''' </summary>
    Public Async Function GetColumnsAsync(dbName As String, table As String) As Task(Of List(Of String))
        Try
            Dim url As String = _baseUrl & "/api/forexe/seed/columns?db_name=" &
                                Uri.EscapeDataString(dbName) & "&table=" & Uri.EscapeDataString(table)
            Dim body As String = Await GetStringAsync(url).ConfigureAwait(False)

            Dim result As New List(Of String)()
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Dim cols As JsonElement
                If doc.RootElement.TryGetProperty("columns", cols) AndAlso cols.ValueKind = JsonValueKind.Array Then
                    For Each e As JsonElement In cols.EnumerateArray()
                        If e.ValueKind = JsonValueKind.String Then result.Add(If(e.GetString(), ""))
                    Next
                End If
            End Using
            Return result

        Catch ex As Exception
            GlobalErrorLog.Write("SeedApiClient.GetColumnsAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' <c>POST /api/forexe/seed/ids</c> — care dintre id-urile astea există deja pe server?
    '''
    ''' Se VERIFICĂ, nu se traduce. Pe MariaDB FX_DDF.IDDF și FX_DDF_REV.IDREV sunt
    ''' AUTO_INCREMENT și nu păstrează id-ul Access alături, deci potrivirea celor două părți
    ''' e o presupunere; asta o testează. La prima lipsă, apelantul oprește DC-ul.
    '''
    ''' Se folosește varianta POST, nu GET: loturile depășesc ușor lungimea rezonabilă a unui URL.
    ''' </summary>
    Public Async Function CheckIdsAsync(dbName As String, table As String, column As String,
                                        values As IReadOnlyList(Of Long)) As Task(Of SeedIdsResult)
        Try
            Dim result As New SeedIdsResult()
            Dim index As Integer = 0
            While index < values.Count
                Dim take As Integer = Math.Min(MaxIdsPerRequest, values.Count - index)

                Dim payload As String
                Using ms As New MemoryStream()
                    Using w As New Utf8JsonWriter(ms)
                        w.WriteStartObject()
                        w.WriteString("db_name", dbName)
                        w.WriteString("table", table)
                        w.WriteString("column", column)
                        w.WriteStartArray("values")
                        For i As Integer = index To index + take - 1
                            w.WriteNumberValue(values(i))
                        Next
                        w.WriteEndArray()
                        w.WriteEndObject()
                    End Using
                    payload = Encoding.UTF8.GetString(ms.ToArray())
                End Using

                Dim body As String = Await PostJsonAsync(_baseUrl & "/api/forexe/seed/ids", payload).ConfigureAwait(False)
                Using doc As JsonDocument = JsonDocument.Parse(body)
                    AppendLongs(doc.RootElement, "found", result.Found)
                    AppendLongs(doc.RootElement, "missing", result.Missing)
                End Using

                index += take
            End While
            Return result

        Catch ex As Exception
            GlobalErrorLog.Write("SeedApiClient.CheckIdsAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' <c>POST /api/forexe/seed/rows</c> cu <c>mode="insert_missing"</c>: un rând deja
    ''' prezent pe MariaDB rămâne NEATINS (decizia 3 — nu se suprascrie nimic).
    '''
    ''' Valorile se scriu direct din <see cref="JsonElement"/>-ele citite din fișier, deci
    ''' niciun număr nu trece printr-o conversie .NET pe drum.
    ''' </summary>
    Public Async Function InsertMissingAsync(dbName As String, table As String,
                                             columns As IReadOnlyList(Of String),
                                             rows As IReadOnlyList(Of JsonElement())) As Task(Of SeedWriteResult)
        Try
            If rows.Count > MaxRowsPerRequest Then
                Throw New ArgumentException(
                    "Lot prea mare pentru server: " & rows.Count.ToString() & " rânduri (maxim " &
                    MaxRowsPerRequest.ToString() & ").", NameOf(rows))
            End If

            Dim payload As String
            Using ms As New MemoryStream()
                Using w As New Utf8JsonWriter(ms)
                    w.WriteStartObject()
                    w.WriteString("db_name", dbName)
                    w.WriteString("table", table)
                    w.WriteString("mode", "insert_missing")
                    w.WriteStartArray("columns")
                    For Each c As String In columns
                        w.WriteStringValue(c)
                    Next
                    w.WriteEndArray()
                    w.WriteStartArray("rows")
                    For Each r As JsonElement() In rows
                        w.WriteStartArray()
                        For Each v As JsonElement In r
                            v.WriteTo(w)
                        Next
                        w.WriteEndArray()
                    Next
                    w.WriteEndArray()
                    w.WriteEndObject()
                End Using
                payload = Encoding.UTF8.GetString(ms.ToArray())
            End Using

            Dim body As String = Await PostJsonAsync(_baseUrl & "/api/forexe/seed/rows", payload).ConfigureAwait(False)

            Dim res As New SeedWriteResult()
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Dim root As JsonElement = doc.RootElement
                res.Received = ReadInt(root, "received")
                ' „inserted"/„skipped" există doar în modul insert_missing; „affected" e
                ' câmpul istoric și e egal cu „inserted" în modul ăsta.
                res.Inserted = If(HasProperty(root, "inserted"), ReadInt(root, "inserted"), ReadInt(root, "affected"))
                res.Skipped = If(HasProperty(root, "skipped"), ReadInt(root, "skipped"), res.Received - res.Inserted)
            End Using
            Return res

        Catch ex As Exception
            GlobalErrorLog.Write("SeedApiClient.InsertMissingAsync", ex)
            Throw
        End Try
    End Function

    ' --- transport ------------------------------------------------------------------
    ' Private, atinse doar prin metodele publice deja învelite mai sus.

    Private Async Function GetStringAsync(url As String) As Task(Of String)
        Using resp As HttpResponseMessage = Await _http.GetAsync(url).ConfigureAwait(False)
            Dim body As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
            EnsureOk(resp, body, url)
            Return body
        End Using
    End Function

    Private Async Function PostJsonAsync(url As String, payload As String) As Task(Of String)
        Using content As New StringContent(payload, Encoding.UTF8, "application/json")
            Using resp As HttpResponseMessage = Await _http.PostAsync(url, content).ConfigureAwait(False)
                Dim body As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
                EnsureOk(resp, body, url)
                Return body
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Serverul întoarce mesaje în română, în câmpul „error". Operatorului i se arată acel
    ''' mesaj, niciodată JSON brut.
    ''' </summary>
    Private Shared Sub EnsureOk(resp As HttpResponseMessage, body As String, url As String)
        If resp.IsSuccessStatusCode Then Return

        Dim message As String = ""
        Try
            Using doc As JsonDocument = JsonDocument.Parse(body)
                message = ReadString(doc.RootElement, "error")
            End Using
        Catch
            ' Corp care nu e JSON (proxy, pagină de eroare) — rămânem pe mesajul generic.
            message = ""
        End Try

        If String.IsNullOrWhiteSpace(message) Then
            message = "Serverul a răspuns " & CInt(resp.StatusCode).ToString() & " la " & url & "."
        End If
        Throw New InvalidOperationException(message)
    End Sub

    Private Shared Function HasProperty(parent As JsonElement, name As String) As Boolean
        Dim v As JsonElement
        Return parent.TryGetProperty(name, v)
    End Function

    Private Shared Function ReadInt(parent As JsonElement, name As String) As Integer
        Dim v As JsonElement
        Dim n As Integer
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt32(n) Then
            Return n
        End If
        Return 0
    End Function

    Private Shared Function ReadString(parent As JsonElement, name As String) As String
        Dim v As JsonElement
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then
            Return If(v.GetString(), "")
        End If
        Return ""
    End Function

    Private Shared Sub AppendLongs(root As JsonElement, name As String, target As List(Of Long))
        Dim arr As JsonElement
        If root.TryGetProperty(name, arr) AndAlso arr.ValueKind = JsonValueKind.Array Then
            For Each e As JsonElement In arr.EnumerateArray()
                Dim n As Long
                If e.ValueKind = JsonValueKind.Number AndAlso e.TryGetInt64(n) Then target.Add(n)
            Next
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _http.Dispose()
    End Sub

End Class
