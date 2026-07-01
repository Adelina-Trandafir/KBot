Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

' Clientul HTTP real. Deține BaseUrl / X-Api-Key / retry / timeout / JSON.
' BaseAddress + Timeout se setează pe HttpClient la înregistrarea DI (Program.vb);
' cheia X-Api-Key se trimite per-request din ApiOptions.
Public Class ApiClient
    Implements IApiClient

    Private ReadOnly _http As HttpClient
    Private ReadOnly _options As ApiOptions

    ' PropertyNamingPolicy=Nothing => numele proprietăților DTO rămân neschimbate
    ' (db_name / Cod / Descriere / Stare), exact ca în contractul rutei Python.
    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {.PropertyNamingPolicy = Nothing}

    Public Sub New(http As HttpClient, options As ApiOptions)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
        _http = http
        _options = options
    End Sub

    Public Async Function UpsertAngajamenteAsync(dbName As String,
                                                 rows As IReadOnlyList(Of Angajament),
                                                 ct As CancellationToken) As Task(Of String) Implements IApiClient.UpsertAngajamenteAsync
        If String.IsNullOrEmpty(dbName) Then Throw New ArgumentException("dbName gol.", NameOf(dbName))
        If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
        If String.IsNullOrEmpty(_options.ApiKey) Then Throw New InvalidOperationException("API key neconfigurat (ApiOptions.ApiKey).")

        Dim req As New UpsertAngajamenteRequest() With {.db_name = dbName}
        For Each a In rows
            req.rows.Add(New AngajamentRow() With {
                .Cod = a.CodAngajament,
                .Descriere = a.Descriere,
                .Stare = a.Stare
            })
        Next

        Dim body As String = JsonSerializer.Serialize(req, _json)

        Dim maxAttempts As Integer = Math.Max(1, _options.MaxRetries)
        Dim attempt As Integer = 0
        Do
            attempt += 1
            ' VB.NET nu permite Await într-un Catch; marcăm reîncercarea și așteptăm după.
            Dim retryDelay As TimeSpan = TimeSpan.Zero
            Try
                Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/angajamente/upsert")
                    msg.Headers.Add("X-Api-Key", _options.ApiKey)
                    msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                    Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                        Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        If Not resp.IsSuccessStatusCode Then
                            Throw New ApplicationException($"upsert angajamente HTTP {CInt(resp.StatusCode)}: {respText}")
                        End If
                        Return respText
                    End Using
                End Using
            Catch ex As HttpRequestException When attempt < maxAttempts
                retryDelay = TimeSpan.FromSeconds(2 * attempt)
            Catch ex As TaskCanceledException When (Not ct.IsCancellationRequested) AndAlso attempt < maxAttempts
                ' Timeout tranzitoriu (nu anulare de la apelant) — reîncercăm.
                retryDelay = TimeSpan.FromSeconds(2 * attempt)
            End Try
            Await Task.Delay(retryDelay, ct).ConfigureAwait(False)
        Loop
    End Function

    Public Function GetAngajamenteAsync(dbName As String, an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) Implements IApiClient.GetAngajamenteAsync
        Throw New NotImplementedException()
    End Function

    Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) Implements IApiClient.GetAsync
        Throw New NotImplementedException()
    End Function

    Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse) Implements IApiClient.PostAsync
        Throw New NotImplementedException()
    End Function
End Class
