Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading.Tasks

' One row of GET /api/admin/users: an operator on one unit, plus the live
' failed-login state of that operator from the running server.
Public NotInheritable Class AdminUser
    <JsonPropertyName("un")> Public Property Un As String = ""
    <JsonPropertyName("dc")> Public Property Dc As String = ""
    <JsonPropertyName("nume_unitate")> Public Property NumeUnitate As String = ""
    <JsonPropertyName("rol")> Public Property Rol As String = ""
    <JsonPropertyName("last_ss")> Public Property LastSs As String = ""
    <JsonPropertyName("fails")> Public Property Fails As Integer
    ' Seconds of lockout left; 0 = not blocked.
    <JsonPropertyName("blocked")> Public Property Blocked As Integer
End Class

Friend NotInheritable Class AdminUsersResponse
    <JsonPropertyName("users")> Public Property Users As List(Of AdminUser)
End Class

Friend NotInheritable Class AdminResetResponse
    <JsonPropertyName("cleared")> Public Property Cleared As Integer
End Class

Friend NotInheritable Class AdminErrorResponse
    <JsonPropertyName("error")> Public Property [Error] As String
End Class

' HTTPS client for the X-Api-Key admin routes of the Flask server
' (routes/admin.py). Throws ApplicationException with a Romanian message on
' any failure, like the SSH/SFTP services.
Public NotInheritable Class AdminApiService

    Private Shared ReadOnly Http As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(30)}

    Private ReadOnly _baseUrl As String
    Private ReadOnly _apiKey As String

    Public Sub New(settings As PushSettings)
        _baseUrl = If(settings.ApiBaseUrl, "").Trim().TrimEnd("/"c)
        _apiKey = If(settings.ApiKey, "")
        If Not _baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
            Throw New ApplicationException("Adresa API trebuie să înceapă cu https://.")
        End If
        If _apiKey = "" Then Throw New ApplicationException("Cheia API lipsește.")
    End Sub

    Public Async Function GetUsersAsync() As Task(Of List(Of AdminUser))
        Using req As New HttpRequestMessage(HttpMethod.Get, _baseUrl & "/api/admin/users")
            Dim body = Await SendAsync(req)
            Dim parsed = JsonSerializer.Deserialize(Of AdminUsersResponse)(body)
            Return If(parsed?.Users, New List(Of AdminUser)())
        End Using
    End Function

    ' Returns how many (IP, user) counters the server dropped.
    Public Async Function ResetLoginAsync(username As String) As Task(Of Integer)
        Using req As New HttpRequestMessage(HttpMethod.Post, _baseUrl & "/api/admin/login/reset")
            Dim json = JsonSerializer.Serialize(New Dictionary(Of String, String) From {{"username", username}})
            req.Content = New StringContent(json, Encoding.UTF8, "application/json")
            Dim body = Await SendAsync(req)
            Dim parsed = JsonSerializer.Deserialize(Of AdminResetResponse)(body)
            Return If(parsed Is Nothing, 0, parsed.Cleared)
        End Using
    End Function

    Private Async Function SendAsync(req As HttpRequestMessage) As Task(Of String)
        req.Headers.Add("X-Api-Key", _apiKey)
        Dim resp As HttpResponseMessage
        Try
            resp = Await Http.SendAsync(req)
        Catch ex As HttpRequestException
            Throw New ApplicationException("Serverul API nu răspunde: " & ex.Message, ex)
        Catch ex As TaskCanceledException
            Throw New ApplicationException("Serverul API nu a răspuns la timp.", ex)
        End Try

        Using resp
            Dim body = Await resp.Content.ReadAsStringAsync()
            If resp.StatusCode = HttpStatusCode.Unauthorized Then
                Throw New ApplicationException("Cheia API a fost refuzată de server.")
            End If
            If Not resp.IsSuccessStatusCode Then
                Throw New ApplicationException($"Serverul a răspuns {CInt(resp.StatusCode)}: {ErrorText(body)}")
            End If
            Return body
        End Using
    End Function

    Private Shared Function ErrorText(body As String) As String
        Try
            Dim e = JsonSerializer.Deserialize(Of AdminErrorResponse)(body)
            If e IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(e.Error) Then Return e.Error
        Catch ex As JsonException
            ' Not JSON (proxy error page, 404 of an older server): show the raw start.
        End Try
        Return If(body.Length > 200, body.Substring(0, 200), body)
    End Function

    ' Prints the server's own API_KEY, from the pushed tree (config.py is
    ' host-only). Run over SSH; the output must not reach the log or the pane.
    Public Shared Function ReadKeyCommand(settings As PushSettings) As String
        Return $"cd {Quote(settings.RemoteRoot)} && {Quote(settings.RemotePython)} " &
               "-c 'from config import API_KEY; print(API_KEY)'"
    End Function

    Private Shared Function Quote(value As String) As String
        Return "'" & If(value, "").Replace("'", "'\''") & "'"
    End Function

End Class
