Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0000-21 - the help's questions and ratings, sent in batches from the client's waiting list.
Partial Public Class ApiClient
    Implements IHelpFeedbackApi

    Private NotInheritable Class HelpFeedbackRequest
        Public Property rows As IReadOnlyList(Of HelpFeedbackRow)
    End Class

    Public Async Function SendHelpFeedbackAsync(rows As IReadOnlyList(Of HelpFeedbackRow),
                                                ct As CancellationToken) As Task(Of HelpFeedbackResult) _
        Implements IHelpFeedbackApi.SendHelpFeedbackAsync
        Try
            EnsureConfigured()
            If rows Is Nothing OrElse rows.Count = 0 Then Throw New ArgumentException("No rows to send.", NameOf(rows))

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/help/feedback")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Dim body As String = JsonSerializer.Serialize(New HelpFeedbackRequest() With {.rows = rows}, _json)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "trimiterea întrebărilor din ajutor", CInt(resp.StatusCode))
                    End If
                    Dim payload As HelpFeedbackResult = JsonSerializer.Deserialize(Of HelpFeedbackResult)(respText, _json)
                    If payload Is Nothing Then Throw New ApiException("Serverul nu a confirmat întrebările primite.", CInt(resp.StatusCode))
                    Return payload
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            ' The type only: the exception never carries the questions, but keep it that way.
            GlobalErrorLog.Write("ApiClient.SendHelpFeedbackAsync", New InvalidOperationException("Help feedback not sent: " & ex.GetType().Name))
            Throw
        End Try
    End Function

End Class
