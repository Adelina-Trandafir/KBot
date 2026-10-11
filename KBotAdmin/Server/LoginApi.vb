Option Strict On
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

''' <summary>One row of GET /api/admin/users: an operator on one unit, with the live failed-login state.</summary>
Public NotInheritable Class LoginUser
    <JsonPropertyName("un")> Public Property Un As String = String.Empty
    <JsonPropertyName("dc")> Public Property Dc As String = String.Empty
    <JsonPropertyName("nume_unitate")> Public Property UnitName As String = String.Empty
    <JsonPropertyName("rol")> Public Property Rol As String = String.Empty
    <JsonPropertyName("last_ss")> Public Property LastSs As String = String.Empty
    <JsonPropertyName("fails")> Public Property Fails As Integer
    ''' <summary>Seconds of lockout left; 0 = not blocked.</summary>
    <JsonPropertyName("blocked")> Public Property Blocked As Integer
End Class

Friend NotInheritable Class LoginUsersResponse
    <JsonPropertyName("users")> Public Property Users As List(Of LoginUser)
End Class

Friend NotInheritable Class LoginResetResponse
    <JsonPropertyName("cleared")> Public Property Cleared As Integer
End Class

Friend NotInheritable Class LoginErrorResponse
    <JsonPropertyName("error")> Public Property ErrorText As String
End Class

''' <summary>
''' HTTPS client for the X-Admin-Token admin routes of the Flask server (<c>routes/admin.py</c>): the list of
''' operators with their failed-login state, and the reset of those counters. The counters live in the
''' memory of the running server, so SSH cannot reach them. The key is read once over SSH from the
''' server's own config.py and kept in memory only. Slice ME01.
''' </summary>
Public NotInheritable Class LoginApi

    Private Shared ReadOnly Http As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(30)}

    Private ReadOnly _baseUrl As String
    Private ReadOnly _apiKey As String

    Public Sub New(k_baseUrl As String, k_apiKey As String)
        _baseUrl = If(k_baseUrl, String.Empty).Trim().TrimEnd("/"c)
        _apiKey = If(k_apiKey, String.Empty)
        If Not _baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
            Throw New AdminException("Adresa API trebuie să înceapă cu https://.")
        End If
        If _apiKey = String.Empty Then Throw New AdminException("Cheia API lipsește.")
    End Sub

    ''' <summary>The command that prints the server's own ADMIN_TOKEN (the administration credential, never the key shipped to clients). Its output must not reach any log.</summary>
    Public Shared Function ReadKeyCommand(k_settings As AdminSettings) As String
        Return "cd " & AdminClient.Quote(k_settings.RemoteRoot) & " && " & AdminClient.Quote(k_settings.RemotePython) &
               " -c 'from config import ADMIN_TOKEN; print(ADMIN_TOKEN)'"
    End Function

    Public Async Function GetUsersAsync() As Task(Of List(Of LoginUser))
        Using k_req As New HttpRequestMessage(HttpMethod.Get, _baseUrl & "/api/admin/users")
            Dim k_body As String = Await SendAsync(k_req).ConfigureAwait(True)
            Dim k_parsed As LoginUsersResponse = JsonSerializer.Deserialize(Of LoginUsersResponse)(k_body)
            Return If(k_parsed?.Users, New List(Of LoginUser)())
        End Using
    End Function

    ''' <summary>Returns how many (IP, user) counters the server dropped.</summary>
    Public Async Function ResetAsync(k_username As String) As Task(Of Integer)
        Using k_req As New HttpRequestMessage(HttpMethod.Post, _baseUrl & "/api/admin/login/reset")
            Dim k_json As String = JsonSerializer.Serialize(New Dictionary(Of String, String) From {{"username", k_username}})
            k_req.Content = New StringContent(k_json, Encoding.UTF8, "application/json")
            Dim k_body As String = Await SendAsync(k_req).ConfigureAwait(True)
            Dim k_parsed As LoginResetResponse = JsonSerializer.Deserialize(Of LoginResetResponse)(k_body)
            Return If(k_parsed Is Nothing, 0, k_parsed.Cleared)
        End Using
    End Function

    Private Async Function SendAsync(k_req As HttpRequestMessage) As Task(Of String)
        k_req.Headers.Add("X-Admin-Token", _apiKey)
        Dim k_resp As HttpResponseMessage
        Try
            k_resp = Await Http.SendAsync(k_req).ConfigureAwait(True)
        Catch ex As HttpRequestException
            GlobalErrorLog.Write("LoginApi.SendAsync", ex)
            Throw New AdminException("Serverul API nu răspunde: " & ex.Message, ex)
        Catch ex As TaskCanceledException
            GlobalErrorLog.Write("LoginApi.SendAsync", ex)
            Throw New AdminException("Serverul API nu a răspuns la timp.", ex)
        End Try
        Using k_resp
            Dim k_body As String = Await k_resp.Content.ReadAsStringAsync().ConfigureAwait(True)
            If k_resp.StatusCode = HttpStatusCode.Unauthorized Then Throw New AdminException("Cheia API a fost refuzată de server.")
            If Not k_resp.IsSuccessStatusCode Then
                Throw New AdminException("Serverul a răspuns " & CInt(k_resp.StatusCode).ToString(Globalization.CultureInfo.InvariantCulture) & ": " & ErrorText(k_body))
            End If
            Return k_body
        End Using
    End Function

    Private Shared Function ErrorText(k_body As String) As String
        Try
            Dim k_error As LoginErrorResponse = JsonSerializer.Deserialize(Of LoginErrorResponse)(k_body)
            If k_error IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(k_error.ErrorText) Then Return k_error.ErrorText
        Catch ex As JsonException
            ' Not JSON (a proxy page, the 404 of an older server): the raw start is shown instead.
        End Try
        Return If(k_body.Length > 200, k_body.Substring(0, 200), k_body)
    End Function

End Class
