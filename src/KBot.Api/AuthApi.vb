Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Domain

' Implementarea clientului de login. Refoloseste HttpClient-ul injectat (BaseAddress
' + Timeout setate la DI, ca ApiClient) si trimite cheia X-Api-Key per-request.
' Caile sunt relative "/api/auth/..." — acelasi tipar ca ApiClient.
Public NotInheritable Class AuthApi
    Implements IAuthApi

    Private ReadOnly _http As HttpClient
    Private ReadOnly _options As ApiOptions

    ' Numele proprietatilor raman neschimbate (contract Python), dar acceptam si
    ' potriviri case-insensitive pentru robustete la raspunsuri.
    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {
            .PropertyNamingPolicy = Nothing,
            .PropertyNameCaseInsensitive = True
        }

    Public Sub New(http As HttpClient, options As ApiOptions)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
        _http = http
        _options = options
    End Sub

    Public Async Function GetUnitsAsync(username As String, password As String,
                                        an As Integer?, ct As CancellationToken) _
        As Task(Of IReadOnlyList(Of UnitInfo)) Implements IAuthApi.GetUnitsAsync

        Dim payload = New With {Key .username = username, Key .password = password, Key .an = an}
        Dim respText As String = Await SendAsync("/api/auth/units", payload, "listarea unităților", ct).ConfigureAwait(False)

        Dim body As UnitsResponse = JsonSerializer.Deserialize(Of UnitsResponse)(respText, _json)
        If body Is Nothing OrElse body.Units Is Nothing Then Return Array.Empty(Of UnitInfo)()
        Return body.Units
    End Function

    Public Async Function LoginAsync(username As String, password As String, idUnitate As Integer,
                                     pcname As String, ct As CancellationToken) _
        As Task(Of LoginResult) Implements IAuthApi.LoginAsync

        Dim payload = New With {
            Key .username = username, Key .password = password,
            Key .IdUnitate = idUnitate, Key .pcname = pcname}
        Dim respText As String = Await SendAsync("/api/auth/login", payload, "autentificare", ct).ConfigureAwait(False)

        Dim result As LoginResult = JsonSerializer.Deserialize(Of LoginResult)(respText, _json)
        If result Is Nothing OrElse result.SessionContext Is Nothing OrElse result.SessionId <= 0 Then
            Throw New ApiException("Răspuns de autentificare invalid de la server.")
        End If
        Return result
    End Function

    Public Async Function LogoutAsync(sessionId As Integer, ct As CancellationToken) _
        As Task Implements IAuthApi.LogoutAsync
        Dim payload = New With {Key .session_id = sessionId}
        Await SendAsync("/api/auth/logout", payload, "deconectare", ct).ConfigureAwait(False)
    End Function

    ' POST JSON + X-Api-Key. Intoarce corpul brut la 2xx; altfel Throw ApiException
    ' cu mesajul serverului (sau un fallback pe cod). Nu inghite niciodata.
    Private Async Function SendAsync(path As String, payload As Object,
                                     actiune As String, ct As CancellationToken) As Task(Of String)
        Dim body As String = JsonSerializer.Serialize(payload, _json)
        Using msg As New HttpRequestMessage(HttpMethod.Post, path)
            msg.Headers.Add("X-Api-Key", _options.ApiKey)
            msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                If resp.IsSuccessStatusCode Then Return respText
                Throw New ApiException(BuildError(respText, actiune, resp.StatusCode))
            End Using
        End Using
    End Function

    ' Non-2xx -> mesajul roman al serverului daca exista, altfel fallback pe cod.
    Private Shared Function BuildError(respText As String, actiune As String,
                                       status As Net.HttpStatusCode) As String
        Dim serverMsg As String = Nothing
        Try
            Dim err As ErrorResponse = JsonSerializer.Deserialize(Of ErrorResponse)(respText, _json)
            If err IsNot Nothing Then serverMsg = err.Error
        Catch
            ' corpul nu era JSON-ul de eroare asteptat; cadem pe fallback-ul de mai jos
        End Try
        If Not String.IsNullOrWhiteSpace(serverMsg) Then Return serverMsg
        Return $"Eroare la {actiune} (cod {CInt(status)})."
    End Function

    Private NotInheritable Class UnitsResponse
        <JsonPropertyName("units")> Public Property Units As List(Of UnitInfo)
    End Class

    Private NotInheritable Class ErrorResponse
        <JsonPropertyName("error")> Public Property [Error] As String
    End Class
End Class
