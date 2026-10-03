Option Strict On
Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

''' <summary>
''' <see cref="IAccessTypeApi"/> over the shared <see cref="HttpClient"/>, like <see cref="UpdateApi"/>:
''' no Authorization header (the call precedes login). The e-mail goes in the JSON BODY, never in the
''' address, so it does not land in a proxy's access log.
''' </summary>
Public NotInheritable Class AccessTypeApi
    Implements IAccessTypeApi

    Private Const PATH As String = "/api/access/client-type"

    Private ReadOnly _http As HttpClient

    Private Shared ReadOnly _json As JsonSerializerOptions =
        New JsonSerializerOptions With {
            .PropertyNamingPolicy = Nothing,
            .PropertyNameCaseInsensitive = True
        }

    ' Wire shapes, field names exactly as routes/access.py writes / reads them.
    Private NotInheritable Class RequestWire
        Public Property email As String
    End Class

    Private NotInheritable Class ResponseWire
        Public Property access As Integer?
    End Class

    Public Sub New(http As HttpClient)
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        _http = http
    End Sub

    Public Async Function GetAccessAsync(email As String, ct As CancellationToken) As Task(Of Boolean) _
        Implements IAccessTypeApi.GetAccessAsync
        If String.IsNullOrWhiteSpace(email) Then Throw New ArgumentException("E-mailul lipsește.", NameOf(email))
        Try
            If _http.BaseAddress Is Nothing Then
                Throw New ApiException("Configurație lipsă: adresa serverului nu este setată. Contactați administratorul.")
            End If
            Dim body As String = JsonSerializer.Serialize(New RequestWire With {.email = email.Trim()}, _json)
            Using msg As New HttpRequestMessage(HttpMethod.Post, PATH)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                msg.Options.Set(ServerGate.Bypass, True)   ' slice 0098: a POST that only reads; the gate must not hold it
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Dim err As ApiErrorBody = ApiErrorBody.Parse(respText)
                        Throw New ApiException(err.MessageOrFallback("verificarea tipului de client", CInt(resp.StatusCode)),
                                               CInt(resp.StatusCode), err.Reason)
                    End If
                    Dim wire As ResponseWire = JsonSerializer.Deserialize(Of ResponseWire)(respText, _json)
                    ' Only an explicit 0 or 1 is an answer: anything else is a server we do not understand.
                    If wire Is Nothing OrElse Not wire.access.HasValue OrElse (wire.access.Value <> 0 AndAlso wire.access.Value <> 1) Then
                        Throw New ApiException("Răspuns invalid de la server la verificarea tipului de client.",
                                               CInt(resp.StatusCode), "CLIENT_TYPE_INVALID")
                    End If
                    Return wire.access.Value = 1
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AccessTypeApi.GetAccessAsync", ex)
            Throw
        End Try
    End Function
End Class
