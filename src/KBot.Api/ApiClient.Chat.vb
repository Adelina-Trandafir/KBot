Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' SLICE-CHAT-08 - the shared chat, as K-BOT.app uses it. Same routes as the web pages; the K-BOT bearer
' is the door (the server tells the two apart by the header). Message bodies never reach the log.
Partial Public Class ApiClient
    Implements IChatApi

    Public Async Function GetChatStateAsync(ct As CancellationToken) As Task(Of ChatState) _
        Implements IChatApi.GetChatStateAsync
        Return Await ChatCallAsync(Of ChatState)(HttpMethod.Get, "state", Nothing, "citirea chatului", ct).ConfigureAwait(False)
    End Function

    Public Async Function GetChatMessagesAsync(before As String, after As String, limit As Integer,
                                               ct As CancellationToken) As Task(Of ChatMessagePage) _
        Implements IChatApi.GetChatMessagesAsync
        If Not String.IsNullOrEmpty(before) AndAlso Not String.IsNullOrEmpty(after) Then
            Throw New ArgumentException("Only one of before / after may be given.", NameOf(before))
        End If
        Dim k_path As String = "messages?limit=" & limit.ToString(Globalization.CultureInfo.InvariantCulture)
        If Not String.IsNullOrEmpty(before) Then k_path &= "&before=" & Uri.EscapeDataString(before)
        If Not String.IsNullOrEmpty(after) Then k_path &= "&after=" & Uri.EscapeDataString(after)
        Return Await ChatCallAsync(Of ChatMessagePage)(HttpMethod.Get, k_path, Nothing, "citirea mesajelor", ct).ConfigureAwait(False)
    End Function

    Public Async Function SendChatMessageAsync(request As ChatSendRequest, ct As CancellationToken) As Task(Of ChatSendResult) _
        Implements IChatApi.SendChatMessageAsync
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        Return Await ChatCallAsync(Of ChatSendResult)(HttpMethod.Post, "messages", request, "trimiterea mesajului", ct).ConfigureAwait(False)
    End Function

    Public Async Function SaveChatPreferencesAsync(displayName As String, anonymous As Boolean?,
                                                   applications As IReadOnlyList(Of String),
                                                   ct As CancellationToken) As Task(Of ChatState) _
        Implements IChatApi.SaveChatPreferencesAsync
        Dim k_body As New Dictionary(Of String, Object)()
        If displayName IsNot Nothing Then k_body("display_name") = displayName
        If anonymous.HasValue Then k_body("anonymous") = anonymous.Value
        If applications IsNot Nothing Then k_body("applications") = applications
        Return Await ChatCallAsync(Of ChatState)(HttpMethod.Post, "preferences", k_body, "salvarea preferințelor chatului", ct).ConfigureAwait(False)
    End Function

    Public Async Function MarkChatReadAsync(application As String, lastId As String,
                                            ct As CancellationToken) As Task(Of ChatState) _
        Implements IChatApi.MarkChatReadAsync
        If String.IsNullOrEmpty(application) Then Throw New ArgumentException("application is empty.", NameOf(application))
        If String.IsNullOrEmpty(lastId) Then Throw New ArgumentException("lastId is empty.", NameOf(lastId))
        Dim k_frontiers As New Dictionary(Of String, String)() From {{application, lastId}}
        Dim k_body As New Dictionary(Of String, Object)() From {{"frontiers", k_frontiers}}
        Return Await ChatCallAsync(Of ChatState)(HttpMethod.Post, "read", k_body, "marcarea mesajelor ca citite", ct).ConfigureAwait(False)
    End Function

    ' One call to /api/chat/<path>. A refusal becomes an ApiException (status kept, so the caller can tell a dead session
    ' from a busy server). The log gets the type and the status, never a body: what people wrote is not ours to copy.
    Private Async Function ChatCallAsync(Of T)(k_method As HttpMethod, k_path As String, k_body As Object,
                                               k_what As String, ct As CancellationToken) As Task(Of T)
        Try
            EnsureConfigured()
            Using k_msg As New HttpRequestMessage(k_method, "/api/chat/" & k_path)
                k_msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                If k_body IsNot Nothing Then
                    k_msg.Content = New StringContent(JsonSerializer.Serialize(k_body, k_body.GetType(), _json), Encoding.UTF8, "application/json")
                End If
                Using k_resp As HttpResponseMessage = Await _http.SendAsync(k_msg, ct).ConfigureAwait(False)
                    Dim k_text As String = Await k_resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not k_resp.IsSuccessStatusCode Then
                        Throw BuildApiException(k_text, k_what, CInt(k_resp.StatusCode))
                    End If
                    Dim k_payload As T = JsonSerializer.Deserialize(Of T)(k_text, _json)
                    If k_payload Is Nothing Then Throw New ApiException("Serverul nu a dat un răspuns pentru chat.", CInt(k_resp.StatusCode))
                    Return k_payload
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ChatCallAsync", New InvalidOperationException("Chat call failed (" & k_what & "): " & ex.GetType().Name))
            Throw
        End Try
    End Function

End Class
