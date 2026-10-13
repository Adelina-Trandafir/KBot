Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' GET /api/actiuni: routes/actiuni.py.
Partial Public Class ApiClient
    Implements IActionsApi

    ' Wire shape: field names exactly as routes/actiuni.py writes them.
    Private NotInheritable Class ActionWireRow
        Public Property moment As String
        Public Property text As String
    End Class

    Private NotInheritable Class ActionWire
        Public Property actiuni As List(Of ActionWireRow)
    End Class

    Public Async Function GetRecentActionsAsync(k_count As Integer, k_ct As CancellationToken) _
        As Task(Of IReadOnlyList(Of RecentAction)) Implements IActionsApi.GetRecentActionsAsync
        Try
            EnsureConfigured()
            Dim k_result As New List(Of RecentAction)()
            Using k_msg As New HttpRequestMessage(HttpMethod.Get, "/api/actiuni?n=" & k_count.ToString(CultureInfo.InvariantCulture))
                k_msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using k_resp As HttpResponseMessage = Await _http.SendAsync(k_msg, k_ct).ConfigureAwait(False)
                    ' An older server has no such route: not an error, just nothing to show.
                    If k_resp.StatusCode = Net.HttpStatusCode.NotFound Then Return k_result
                    Dim k_text As String = Await k_resp.Content.ReadAsStringAsync(k_ct).ConfigureAwait(False)
                    If Not k_resp.IsSuccessStatusCode Then
                        Throw BuildApiException(k_text, "citirea acțiunilor recente", CInt(k_resp.StatusCode))
                    End If
                    Dim k_wire As ActionWire = JsonSerializer.Deserialize(Of ActionWire)(k_text, _json)
                    If k_wire IsNot Nothing AndAlso k_wire.actiuni IsNot Nothing Then
                        For Each k_row As ActionWireRow In k_wire.actiuni
                            Dim k_moment As DateTimeOffset
                            If Not DateTimeOffset.TryParse(k_row.moment, CultureInfo.InvariantCulture, DateTimeStyles.None, k_moment) Then Continue For
                            k_result.Add(New RecentAction With {.Moment = k_moment.LocalDateTime, .Text = If(k_row.text, String.Empty)})
                        Next
                    End If
                    Return k_result
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetRecentActionsAsync", ex)
            Throw
        End Try
    End Function

End Class
