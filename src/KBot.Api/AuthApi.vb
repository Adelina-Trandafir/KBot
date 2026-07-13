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
' + Timeout setate la DI, ca ApiClient). Apelurile pre-auth (units, login) NU poarta
' niciun header de autorizare — credentialele sunt in corp, peste TLS; logout / periods /
' last-ss poarta token-ul bearer. Caile sunt relative "/api/auth/..." — acelasi tipar
' ca ApiClient.
Public NotInheritable Class AuthApi
    Implements IAuthApi

    Private ReadOnly _http As HttpClient

    ' PropertyNamingPolicy=Nothing => numele proprietatilor raman verbatim (contract
    ' Python). DTO-urile de cerere isi forteaza cheile lowercase cu <JsonPropertyName>.
    ' Deserializarea accepta si potriviri case-insensitive pentru robustete.
    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {
            .PropertyNamingPolicy = Nothing,
            .PropertyNameCaseInsensitive = True
        }

    Public Sub New(http As HttpClient)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        _http = http
    End Sub

    Public Async Function GetUnitsAsync(username As String, password As String,
                                        ct As CancellationToken) _
        As Task(Of IReadOnlyList(Of UnitInfo)) Implements IAuthApi.GetUnitsAsync

        Dim payload As New UnitsRequest With {.Username = username, .Password = password}
        Dim respText As String = Await PostAsync("/api/auth/units", payload, "listarea unităților", ct).ConfigureAwait(False)

        Dim body As UnitsResponse = JsonSerializer.Deserialize(Of UnitsResponse)(respText, _json)
        If body Is Nothing OrElse body.Units Is Nothing Then Return Array.Empty(Of UnitInfo)()
        Return body.Units
    End Function

    Public Async Function LoginAsync(username As String, password As String, dc As String,
                                     machine As String, ct As CancellationToken) _
        As Task(Of LoginResult) Implements IAuthApi.LoginAsync

        Dim payload As New LoginRequest With {
            .Username = username, .Password = password, .DbName = dc, .Machine = machine}
        Dim respText As String = Await PostAsync("/api/auth/login", payload, "autentificare", ct).ConfigureAwait(False)

        Dim result As LoginResult = JsonSerializer.Deserialize(Of LoginResult)(respText, _json)
        If result Is Nothing OrElse result.SessionContext Is Nothing OrElse String.IsNullOrEmpty(result.Token) Then
            Throw New ApiException("Răspuns de autentificare invalid de la server.")
        End If
        Return result
    End Function

    Public Async Function LogoutAsync(token As String, ct As CancellationToken) _
        As Task Implements IAuthApi.LogoutAsync
        ' Corp gol; token-ul din header identifica sesiunea de revocat.
        Await PostAsync("/api/auth/logout", New Object(), "deconectare", ct, bearer:=token).ConfigureAwait(False)
    End Function

    Public Async Function GetPeriodsAsync(token As String, dbName As String,
                                          ct As CancellationToken) _
        As Task(Of IReadOnlyList(Of PeriodInfo)) Implements IAuthApi.GetPeriodsAsync

        Dim url As String = "/api/auth/periods?db_name=" & Uri.EscapeDataString(If(dbName, String.Empty))
        Dim respText As String = Await GetAsync(url, "citirea perioadelor", ct, bearer:=token).ConfigureAwait(False)

        Dim body As PeriodsResponse = JsonSerializer.Deserialize(Of PeriodsResponse)(respText, _json)
        If body Is Nothing OrElse body.Periods Is Nothing Then Return Array.Empty(Of PeriodInfo)()
        Return body.Periods
    End Function

    Public Async Function SaveLastSsAsync(token As String, ss As String, ct As CancellationToken) _
        As Task Implements IAuthApi.SaveLastSsAsync

        Dim payload As New LastSsRequest With {.Ss = ss}
        Await PostAsync("/api/auth/last-ss", payload, "salvarea SS", ct, bearer:=token).ConfigureAwait(False)
    End Function

    ' POST JSON (+ Authorization: Bearer doar cand exista token). Intoarce corpul brut
    ' la 2xx; altfel Throw ApiException cu mesajul serverului (sau fallback pe cod) si
    ' codul HTTP atasat. Nu inghite niciodata.
    Private Async Function PostAsync(path As String, payload As Object,
                                     actiune As String, ct As CancellationToken,
                                     Optional bearer As String = Nothing) As Task(Of String)
        EnsureConfigured()
        Dim body As String = JsonSerializer.Serialize(payload, _json)
        Using msg As New HttpRequestMessage(HttpMethod.Post, path)
            If Not String.IsNullOrEmpty(bearer) Then
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer)
            End If
            msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
            Return Await SendAsync(msg, actiune, ct).ConfigureAwait(False)
        End Using
    End Function

    ' GET (+ Authorization: Bearer). Aceleasi reguli de eroare ca PostAsync.
    Private Async Function GetAsync(path As String, actiune As String, ct As CancellationToken,
                                    Optional bearer As String = Nothing) As Task(Of String)
        EnsureConfigured()
        Using msg As New HttpRequestMessage(HttpMethod.Get, path)
            If Not String.IsNullOrEmpty(bearer) Then
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer)
            End If
            Return Await SendAsync(msg, actiune, ct).ConfigureAwait(False)
        End Using
    End Function

    Private Async Function SendAsync(msg As HttpRequestMessage, actiune As String,
                                     ct As CancellationToken) As Task(Of String)
        Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
            Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
            If resp.IsSuccessStatusCode Then Return respText
            Throw New ApiException(BuildError(respText, actiune, resp.StatusCode), CInt(resp.StatusCode))
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

    ' Fara BaseAddress, caile relative "/api/..." arunca o exceptie criptica de
    ' framework ("An invalid request URI..."); o transformam intr-un mesaj clar pentru
    ' operator. Nu inghite niciodata — arunca ApiException, ca restul stratului.
    Private Sub EnsureConfigured()
        If _http.BaseAddress Is Nothing Then
            Throw New ApiException(
            "Configurație lipsă: adresa serverului nu este setată. Contactați administratorul.")
        End If
    End Sub

    ' ---- DTO-uri de cerere: forteaza cheile lowercase pe care serverul le citeste ----
    ' (serializatorul pastreaza numele verbatim, deci le fixam explicit).
    Private NotInheritable Class UnitsRequest
        <JsonPropertyName("username")> Public Property Username As String
        <JsonPropertyName("password")> Public Property Password As String
    End Class

    Private NotInheritable Class LoginRequest
        <JsonPropertyName("username")> Public Property Username As String
        <JsonPropertyName("password")> Public Property Password As String
        <JsonPropertyName("db_name")> Public Property DbName As String
        <JsonPropertyName("machine")> Public Property Machine As String
    End Class

    Private NotInheritable Class LastSsRequest
        <JsonPropertyName("ss")> Public Property Ss As String
    End Class

    ' ---- DTO-uri de raspuns ----
    Private NotInheritable Class UnitsResponse
        <JsonPropertyName("units")> Public Property Units As List(Of UnitInfo)
    End Class

    Private NotInheritable Class PeriodsResponse
        <JsonPropertyName("periods")> Public Property Periods As List(Of PeriodInfo)
    End Class

    Private NotInheritable Class ErrorResponse
        <JsonPropertyName("error")> Public Property [Error] As String
    End Class
End Class
