Option Strict On
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0076-01 / 0076-02 - what K-BOT saw in the FOREXE page about a reception (date moved, deleted).
Partial Public Class ApiClient
    Implements IReceptiiObservateApi

    ' Wire shape: field names exactly as routes/forexe/receptii_observate.py reads them.
    Private NotInheritable Class ObservatieReceptieRequest
        Public Property cod As String
        Public Property data_veche As String
        Public Property data_noua As String
        Public Property suma_veche As String
        Public Property rang As Integer?
    End Class

    Public Function SpuneDataSchimbataAsync(obs As ObservatieReceptie, ct As CancellationToken) As Task _
        Implements IReceptiiObservateApi.SpuneDataSchimbataAsync
        Return TrimiteObservatiaAsync(obs, "/api/forexe/receptii/data-schimbata", "mutarea datei recepției",
                                      "ApiClient.SpuneDataSchimbataAsync", ct)
    End Function

    Public Function SpuneStearsaAsync(obs As ObservatieReceptie, ct As CancellationToken) As Task _
        Implements IReceptiiObservateApi.SpuneStearsaAsync
        Return TrimiteObservatiaAsync(obs, "/api/forexe/receptii/stearsa", "marcarea recepției ca ștearsă",
                                      "ApiClient.SpuneStearsaAsync", ct)
    End Function

    ' One POST, same body for both facts (a deletion leaves data_noua empty).
    Private Async Function TrimiteObservatiaAsync(obs As ObservatieReceptie, k_url As String, k_ce As String,
                                                  k_sursa As String, ct As CancellationToken) As Task
        Try
            EnsureConfigured()
            If obs Is Nothing Then Throw New ArgumentNullException(NameOf(obs))
            If String.IsNullOrWhiteSpace(obs.CodAngajament) Then
                Throw New ArgumentException("cod gol.", NameOf(obs))
            End If

            Dim k_body As String = JsonSerializer.Serialize(
                New ObservatieReceptieRequest() With {
                    .cod = obs.CodAngajament, .data_veche = obs.DataVeche, .data_noua = obs.DataNoua,
                    .suma_veche = obs.SumaVeche, .rang = If(obs.Rang > 0, CType(obs.Rang, Integer?), Nothing)},
                _json)
            Using msg As New HttpRequestMessage(HttpMethod.Post, k_url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New StringContent(k_body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, k_ce, CInt(resp.StatusCode))
                    End If
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write(k_sursa, ex)
            Throw
        End Try
    End Function

End Class
