Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0081-09 - the program -> SS map of section A (routes/forexe/ddf_edit.py).
Partial Public Class ApiClient
    Implements IDdfProgramApi

    ' Wire shapes: field names exactly as ddf_edit.py writes them.
    Private NotInheritable Class SurseProgramResponse
        Public Property surse As List(Of SursaProgramWire)
    End Class

    Private NotInheritable Class SursaProgramWire
        Public Property program As String
        Public Property ss As String
        Public Property denumire As String
    End Class

    Public Async Function GetDdfSurseProgramAsync(ct As CancellationToken) _
        As Task(Of List(Of DdfSursaProgram)) Implements IDdfProgramApi.GetDdfSurseProgramAsync

        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, "/api/forexe/ddf/surse-program")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea surselor programelor", CInt(resp.StatusCode))
                    End If
                    Dim payload As SurseProgramResponse =
                        JsonSerializer.Deserialize(Of SurseProgramResponse)(respText, _json)
                    Dim rezultat As New List(Of DdfSursaProgram)()
                    If payload Is Nothing OrElse payload.surse Is Nothing Then Return rezultat
                    For Each s As SursaProgramWire In payload.surse
                        If s Is Nothing Then Continue For
                        rezultat.Add(New DdfSursaProgram() With {
                            .Program = If(s.program, String.Empty),
                            .Ss = If(s.ss, String.Empty),
                            .Denumire = If(s.denumire, String.Empty)})
                    Next
                    Return rezultat
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetDdfSurseProgramAsync", ex)
            Throw
        End Try
    End Function
End Class
