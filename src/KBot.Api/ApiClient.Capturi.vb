Option Strict On
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' The captures taken while the operator works in the FOREXE page (operator, 28.09.2026):
' routes/forexe/capturi.py. Raw JPEG bytes, never base64 in JSON - the same rule as the
' PDFs (slice 0041) and as the DDF captures of slice 0081-04.
Partial Public Class ApiClient
    Implements IForexeCapturiApi

    Private Const H_MOMENT As String = "X-Moment"
    Private Const H_COD_ANGAJAMENT As String = "X-Cod-Angajament"

    Private NotInheritable Class CapturaForexeResponse
        Public Property id As Integer
        Public Property exista_deja As Boolean
    End Class

    Public Function UrcaCapturaRezervareAsync(idrev As Integer, cod As String, nume As String,
                                              moment As String, octeti As Byte(),
                                              ct As CancellationToken) As Task(Of Integer) _
        Implements IForexeCapturiApi.UrcaCapturaRezervareAsync
        Return UrcaCapturaAsync($"/api/forexe/capturi/rezervare/{idrev}", idrev, cod, nume, moment, octeti, ct)
    End Function

    Public Function UrcaCapturaReceptieAsync(idrh As Integer, cod As String, nume As String,
                                             moment As String, octeti As Byte(),
                                             ct As CancellationToken) As Task(Of Integer) _
        Implements IForexeCapturiApi.UrcaCapturaReceptieAsync
        Return UrcaCapturaAsync($"/api/forexe/capturi/receptie/{idrh}", idrh, cod, nume, moment, octeti, ct)
    End Function

    ''' <summary>
    ''' Both routes take the same shape; only the address and the meaning of the number
    ''' differ. The name and the moment are ASCII by construction (CapturaStore builds them),
    ''' so they travel as headers as they are.
    ''' </summary>
    Private Async Function UrcaCapturaAsync(adresa As String, numar As Integer, cod As String,
                                            nume As String, moment As String, octeti As Byte(),
                                            ct As CancellationToken) As Task(Of Integer)
        Try
            EnsureConfigured()
            If numar <= 0 Then Throw New ArgumentException("Numărul capturii lipsește.", NameOf(numar))
            If octeti Is Nothing OrElse octeti.Length = 0 Then
                Throw New ArgumentException("Captura este goală.", NameOf(octeti))
            End If
            If String.IsNullOrWhiteSpace(nume) Then
                Throw New ArgumentException("Numele capturii lipsește.", NameOf(nume))
            End If

            Using msg As New HttpRequestMessage(HttpMethod.Put, adresa)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                msg.Content = New ByteArrayContent(octeti)
                msg.Content.Headers.ContentType = New Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                msg.Headers.TryAddWithoutValidation(H_SHA, PdfHash.Compute(octeti))
                msg.Headers.TryAddWithoutValidation(H_NUME_FISIER, nume)
                msg.Headers.TryAddWithoutValidation(H_MOMENT, If(moment, String.Empty))
                msg.Headers.TryAddWithoutValidation(H_COD_ANGAJAMENT, If(cod, String.Empty))
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea capturii din FOREXE", CInt(resp.StatusCode))
                    End If
                    Dim payload As CapturaForexeResponse =
                        JsonSerializer.Deserialize(Of CapturaForexeResponse)(respText, _json)
                    If payload Is Nothing OrElse payload.id <= 0 Then
                        Throw New ApiException("Serverul nu a întors cheia capturii.", CInt(resp.StatusCode))
                    End If
                    Return payload.id
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.UrcaCapturaAsync", ex)
            Throw
        End Try
    End Function

End Class
