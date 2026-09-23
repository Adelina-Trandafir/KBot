Option Strict On
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0076 - the id markers the FOREXE page writes into what the operator types.
Partial Public Class ApiClient
    Implements IMarcajApi

    ' Wire shapes: field names exactly as routes/forexe/marcaj.py writes / reads them.
    Private NotInheritable Class RezervaMarcajRequest
        Public Property tip As String
        Public Property cod As String
    End Class

    Private NotInheritable Class RezervaMarcajResponse
        Public Property marcaj As String
    End Class

    Public Async Function RezervaMarcajAsync(tip As String, cod As String,
                                             ct As CancellationToken) As Task(Of String) _
        Implements IMarcajApi.RezervaMarcajAsync
        Try
            EnsureConfigured()
            If String.IsNullOrWhiteSpace(tip) Then Throw New ArgumentException("tip gol.", NameOf(tip))
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/marcaj/rezerva")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Dim body As String = JsonSerializer.Serialize(
                    New RezervaMarcajRequest() With {.tip = tip, .cod = cod}, _json)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "rezervarea marcajului", CInt(resp.StatusCode))
                    End If
                    Dim payload As RezervaMarcajResponse =
                        JsonSerializer.Deserialize(Of RezervaMarcajResponse)(respText, _json)
                    If payload Is Nothing OrElse String.IsNullOrWhiteSpace(payload.marcaj) Then
                        Throw New ApiException("Serverul nu a întors marcajul.", CInt(resp.StatusCode))
                    End If
                    Return payload.marcaj.Trim()
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.RezervaMarcajAsync", ex)
            Throw
        End Try
    End Function

End Class
