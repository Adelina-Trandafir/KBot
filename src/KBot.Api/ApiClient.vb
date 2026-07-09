Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Clientul HTTP real. Deține BaseUrl / retry / timeout / JSON. BaseAddress + Timeout
' se setează pe HttpClient la înregistrarea DI (Program.vb); autorizarea e token-ul
' bearer opac citit per-request din SessionContext (singleton, populat de login).
Public Class ApiClient
    Implements IApiClient

    Private ReadOnly _http As HttpClient
    Private ReadOnly _options As ApiOptions
    Private ReadOnly _session As SessionContext

    ' PropertyNamingPolicy=Nothing => numele proprietăților DTO rămân neschimbate
    ' (db_name / Cod / Descriere / Stare), exact ca în contractul rutei Python.
    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {.PropertyNamingPolicy = Nothing}

    Public Sub New(http As HttpClient, options As ApiOptions, session As SessionContext)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
        If session Is Nothing Then Throw New ArgumentNullException(NameOf(session))
        _http = http
        _options = options
        _session = session
    End Sub

    Public Async Function UpsertAngajamenteAsync(dbName As String,
                                                 rows As IReadOnlyList(Of Angajament),
                                                 ct As CancellationToken) As Task(Of String) Implements IApiClient.UpsertAngajamenteAsync
        EnsureConfigured()
        If String.IsNullOrEmpty(dbName) Then Throw New ArgumentException("dbName gol.", NameOf(dbName))
        If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))

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
                    msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                    msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                    Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                        Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                        If Not resp.IsSuccessStatusCode Then
                            ' ApiException NU e prinsă de catch-urile tranzitorii de mai jos:
                            ' un 401 iese direct spre stratul App (re-login §4.9), fără retry aici.
                            Throw New ApiException($"upsert angajamente HTTP {CInt(resp.StatusCode)}: {respText}", CInt(resp.StatusCode))
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

    ' Read-back for the ListaAngajamente round-trip. GETs the rows scoped to dbName
    ' (server filters by DC = db_name). 'an' is forwarded for forward-compat but the
    ' server ignores it (FX_Angajamente has no An column). Hard-fail (Throw) on non-2xx.
    Public Async Function GetAngajamenteAsync(dbName As String, an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) Implements IApiClient.GetAngajamenteAsync
        EnsureConfigured()
        If String.IsNullOrEmpty(dbName) Then Throw New ArgumentException("dbName gol.", NameOf(dbName))

        Dim url As String = $"/api/forexe/angajamente?db_name={Uri.EscapeDataString(dbName)}&an={an}"

        Using msg As New HttpRequestMessage(HttpMethod.Get, url)
            msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                If Not resp.IsSuccessStatusCode Then
                    Throw New ApiException($"get angajamente HTTP {CInt(resp.StatusCode)}: {respText}", CInt(resp.StatusCode))
                End If

                Dim payload As GetAngajamenteResponse = JsonSerializer.Deserialize(Of GetAngajamenteResponse)(respText, _json)
                Dim result As New List(Of Angajament)()
                If payload IsNot Nothing AndAlso payload.rows IsNot Nothing Then
                    For Each r As AngajamentRow In payload.rows
                        result.Add(New Angajament() With {
                            .CodAngajament = If(r.Cod, String.Empty),
                            .Descriere = If(r.Descriere, String.Empty),
                            .Stare = If(r.Stare, String.Empty)
                        })
                    Next
                End If
                Return result
            End Using
        End Using
    End Function

    ' Apelurile de date cer o adresă de server ȘI o sesiune autentificată (token viu).
    ' Fara BaseAddress, caile relative arunca o exceptie criptica de framework; fara
    ' token, serverul ar raspunde oricum 401 — dar aici dam mesajul clar, local.
    Private Sub EnsureConfigured()
        If _http.BaseAddress Is Nothing Then
            Throw New ApiException(
                "Configurație lipsă: adresa serverului nu este setată. Contactați administratorul.")
        End If
        If String.IsNullOrEmpty(_session.Token) Then
            Throw New ApiException("Nu există o sesiune activă. Autentificați-vă.")
        End If
    End Sub

    Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) Implements IApiClient.GetAsync
        Throw New NotImplementedException()
    End Function

    Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest, ct As CancellationToken) As Task(Of TResponse) Implements IApiClient.PostAsync
        Throw New NotImplementedException()
    End Function
End Class
