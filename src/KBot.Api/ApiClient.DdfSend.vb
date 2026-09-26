Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common

' Slice 0081 - sending a DDF revision KBOT -> forexecab (routes/forexe/ddf_trimitere.py).
Partial Public Class ApiClient
    Implements IDdfSendApi

    ' Wire shapes: field names exactly as ddf_trimitere.py reads / writes them.
    Private NotInheritable Class CoduriRequest
        Public Property cod_real As String
        Public Property randuri As List(Of CodRandWire)
    End Class

    Private NotInheritable Class CodRandWire
        Public Property id_sec_a As Integer
        Public Property cod_indicator As String
    End Class

    Private NotInheritable Class CoduriResponse
        Public Property cod As String
    End Class

    Private NotInheritable Class StareRequest
        Public Property stare As Integer
    End Class

    Private NotInheritable Class CapturaResponse
        Public Property id_rev_att As Integer
    End Class

    Private NotInheritable Class DeSemnatResponse
        Public Property revizii As List(Of DeSemnatWire)
        Public Property unitati_necitite As List(Of String)
    End Class

    Private NotInheritable Class DeSemnatWire
        Public Property db_name As String
        Public Property nume_unitate As String
        Public Property idrev As Integer
        Public Property iddf As Integer
        Public Property cual As Integer
        Public Property cod_angajament As String
        Public Property obiect_ddf As String
        Public Property numar_rev As Integer
        Public Property data_rev As Date?
        Public Property desc_scurta As String
        Public Property total As Double
        Public Property semnatura As String
        Public Property pdf_sha256 As String
    End Class

    Private Const H_NUME_FISIER As String = "X-Nume-Fisier"

    Public Function AnuleazaSemnaturaDdfAsync(idrev As Integer, ct As CancellationToken) As Task _
        Implements IDdfSendApi.AnuleazaSemnaturaDdfAsync
        If idrev <= 0 Then Throw New ArgumentException("idrev invalid.", NameOf(idrev))
        Return TrimiteJsonAsync(Of Object, Object)($"/api/forexe/ddf/trimitere/{idrev}/anuleaza-semnatura",
                                                   Nothing, "anularea semnăturii reviziei",
                                                   "ApiClient.AnuleazaSemnaturaDdfAsync", ct)
    End Function

    Public Function IncepeTrimitereaDdfAsync(idrev As Integer, ct As CancellationToken) As Task _
        Implements IDdfSendApi.IncepeTrimitereaDdfAsync
        If idrev <= 0 Then Throw New ArgumentException("idrev invalid.", NameOf(idrev))
        Return TrimiteJsonAsync(Of Object, Object)($"/api/forexe/ddf/trimitere/{idrev}/start",
                                                   Nothing, "începerea trimiterii",
                                                   "ApiClient.IncepeTrimitereaDdfAsync", ct)
    End Function

    Public Async Function SalveazaCoduriDdfAsync(idrev As Integer, codReal As String,
                                                 randuri As IReadOnlyList(Of DdfCodRand),
                                                 ct As CancellationToken) As Task(Of String) _
        Implements IDdfSendApi.SalveazaCoduriDdfAsync
        If idrev <= 0 Then Throw New ArgumentException("idrev invalid.", NameOf(idrev))
        Dim req As New CoduriRequest() With {.cod_real = If(codReal, String.Empty), .randuri = New List(Of CodRandWire)()}
        If randuri IsNot Nothing Then
            For Each r As DdfCodRand In randuri
                req.randuri.Add(New CodRandWire() With {.id_sec_a = r.IdSecA, .cod_indicator = If(r.CodIndicator, String.Empty)})
            Next
        End If
        Dim resp As CoduriResponse = Await TrimiteJsonAsync(Of CoduriRequest, CoduriResponse)(
            $"/api/forexe/ddf/trimitere/{idrev}/coduri", req, "salvarea codurilor din FOREXE",
            "ApiClient.SalveazaCoduriDdfAsync", ct).ConfigureAwait(False)
        Return If(resp?.cod, String.Empty)
    End Function

    Public Function SeteazaStareTrimitereDdfAsync(idrev As Integer, stare As Integer, ct As CancellationToken) As Task _
        Implements IDdfSendApi.SeteazaStareTrimitereDdfAsync
        If idrev <= 0 Then Throw New ArgumentException("idrev invalid.", NameOf(idrev))
        Return TrimiteJsonAsync(Of StareRequest, Object)($"/api/forexe/ddf/trimitere/{idrev}/stare",
                                                         New StareRequest() With {.stare = stare},
                                                         "schimbarea stării de trimitere",
                                                         "ApiClient.SeteazaStareTrimitereDdfAsync", ct)
    End Function

    Public Async Function UrcaCapturaDdfAsync(idrev As Integer, numeFisier As String, png As Byte(),
                                              ct As CancellationToken) As Task(Of Integer) _
        Implements IDdfSendApi.UrcaCapturaDdfAsync
        Try
            EnsureConfigured()
            If idrev <= 0 Then Throw New ArgumentException("idrev invalid.", NameOf(idrev))
            If png Is Nothing OrElse png.Length = 0 Then Throw New ArgumentException("The capture is empty.", NameOf(png))
            If String.IsNullOrWhiteSpace(numeFisier) Then Throw New ArgumentException("The capture name is missing.", NameOf(numeFisier))

            Using msg As New HttpRequestMessage(HttpMethod.Put, $"/api/forexe/ddf/trimitere/{idrev}/captura")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                ' Raw bytes, never base64 in JSON -- the same rule as the PDFs (slice 0041).
                msg.Content = New ByteArrayContent(png)
                msg.Content.Headers.ContentType = New Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream")
                msg.Headers.TryAddWithoutValidation(H_SHA, PdfHash.Compute(png))
                ' The name is ASCII by construction (Poza_IdSecA-12.png), so it fits a header as is.
                msg.Headers.TryAddWithoutValidation(H_NUME_FISIER, numeFisier)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea capturii din FOREXE", CInt(resp.StatusCode))
                    End If
                    Dim payload As CapturaResponse = JsonSerializer.Deserialize(Of CapturaResponse)(respText, _json)
                    If payload Is Nothing OrElse payload.id_rev_att <= 0 Then
                        Throw New ApiException("Serverul nu a întors cheia capturii.", CInt(resp.StatusCode))
                    End If
                    Return payload.id_rev_att
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.UrcaCapturaDdfAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetDdfDeSemnatDirectorAsync(ct As CancellationToken) As Task(Of DdfDeSemnatLista) _
        Implements IDdfSendApi.GetDdfDeSemnatDirectorAsync
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, "/api/forexe/ddf/director/de-semnat")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea documentelor de semnat", CInt(resp.StatusCode))
                    End If
                    Dim payload As DeSemnatResponse = JsonSerializer.Deserialize(Of DeSemnatResponse)(respText, _json)
                    Dim rezultat As New DdfDeSemnatLista()
                    If payload?.unitati_necitite IsNot Nothing Then rezultat.UnitatiNecitite.AddRange(payload.unitati_necitite)
                    If payload?.revizii Is Nothing Then Return rezultat
                    For Each w As DeSemnatWire In payload.revizii
                        rezultat.Revizii.Add(New DdfDeSemnat() With {
                            .DbName = If(w.db_name, String.Empty),
                            .NumeUnitate = If(w.nume_unitate, String.Empty),
                            .Idrev = w.idrev,
                            .Iddf = w.iddf,
                            .Cual = w.cual,
                            .CodAngajament = If(w.cod_angajament, String.Empty),
                            .ObiectDdf = If(w.obiect_ddf, String.Empty),
                            .NumarRev = w.numar_rev,
                            .DataRev = w.data_rev,
                            .DescScurta = If(w.desc_scurta, String.Empty),
                            .Total = w.total,
                            .Semnatura = If(w.semnatura, String.Empty),
                            .PdfSha256 = If(w.pdf_sha256, String.Empty)})
                    Next
                    Return rezultat
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetDdfDeSemnatDirectorAsync", ex)
            Throw
        End Try
    End Function

    ' POST a JSON body (or none) and read a JSON answer. TResp = Object means "only the status matters".
    Private Async Function TrimiteJsonAsync(Of TReq, TResp)(url As String, req As TReq, eticheta As String,
                                                            sursa As String, ct As CancellationToken) As Task(Of TResp)
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Post, url)
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Dim body As String = If(req Is Nothing, "{}", JsonSerializer.Serialize(req, _json))
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, eticheta, CInt(resp.StatusCode))
                    End If
                    If GetType(TResp) Is GetType(Object) Then Return Nothing
                    Return JsonSerializer.Deserialize(Of TResp)(respText, _json)
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write(sursa, ex)
            Throw
        End Try
    End Function

End Class
