Option Strict On
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0112-04 - an error message sent by the operator from the message window.
Partial Public Class ApiClient
    Implements IErrorReportApi

    Public Async Function SendErrorReportAsync(report As ErrorReportRequest,
                                               ct As CancellationToken) As Task _
        Implements IErrorReportApi.SendErrorReportAsync
        Try
            EnsureConfigured()
            If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/errors/report")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(JsonSerializer.Serialize(report, _json), Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "trimiterea raportului de eroare", CInt(resp.StatusCode))
                    End If
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SendErrorReportAsync", ex)
            Throw
        End Try
    End Function

End Class
