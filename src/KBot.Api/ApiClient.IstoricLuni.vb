Option Strict On
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Deleting the last month of the FOREXE history (routes/forexe/istoric_luni.py).
Partial Public Class ApiClient
    Implements IIstoricLuniApi

    ' Wire shapes: field names exactly as istoric_luni.py writes / reads them.
    Private NotInheritable Class IstoricLunaWire
        Public Property luna As String
        Public Property eticheta As String
        Public Property randuri_istoric As Integer
        Public Property angajamente As Integer
        Public Property rezervari As Integer
        Public Property receptii As Integer
        Public Property plati As Integer
        Public Property ddf As Integer
        Public Property ord As Integer
        Public Property blocata As Boolean
        Public Property motiv As String
        Public Property receptii_r_sterse As Integer
    End Class

    Private NotInheritable Class StergeLunaRequest
        Public Property luna As String
    End Class

    Public Async Function CitesteUltimaLunaIstoricAsync(ct As CancellationToken) As Task(Of IstoricLunaInfo) _
        Implements IIstoricLuniApi.CitesteUltimaLunaIstoricAsync
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Get, "/api/forexe/istoric/luna-ultima")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "citirea ultimei luni din istoric", CInt(resp.StatusCode))
                    End If
                    Return IstoricLunaToInfo(JsonSerializer.Deserialize(Of IstoricLunaWire)(respText, _json))
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.CitesteUltimaLunaIstoricAsync", ex)
            Throw
        End Try
    End Function

    Public Function StergeLunaIstoricAsync(luna As String, ct As CancellationToken) As Task(Of IstoricLunaInfo) _
        Implements IIstoricLuniApi.StergeLunaIstoricAsync
        If String.IsNullOrWhiteSpace(luna) Then Throw New ArgumentException("luna goala.", NameOf(luna))
        Return StergeLunaAsync(luna, ct)
    End Function

    Private Async Function StergeLunaAsync(k_luna As String, ct As CancellationToken) As Task(Of IstoricLunaInfo)
        Try
            EnsureConfigured()
            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/istoric/sterge-luna")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Dim body As String = JsonSerializer.Serialize(New StergeLunaRequest() With {.luna = k_luna}, _json)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "ștergerea lunii din istoric", CInt(resp.StatusCode))
                    End If
                    Return IstoricLunaToInfo(JsonSerializer.Deserialize(Of IstoricLunaWire)(respText, _json))
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.StergeLunaIstoricAsync", ex)
            Throw
        End Try
    End Function

    Private Shared Function IstoricLunaToInfo(k_wire As IstoricLunaWire) As IstoricLunaInfo
        Dim k_info As New IstoricLunaInfo()
        If k_wire Is Nothing Then Return k_info
        k_info.Luna = If(k_wire.luna, String.Empty)
        k_info.Eticheta = If(k_wire.eticheta, String.Empty)
        k_info.RanduriIstoric = k_wire.randuri_istoric
        k_info.Angajamente = k_wire.angajamente
        k_info.Rezervari = k_wire.rezervari
        k_info.Receptii = k_wire.receptii
        k_info.Plati = k_wire.plati
        k_info.Ddf = k_wire.ddf
        k_info.Ord = k_wire.ord
        k_info.Blocata = k_wire.blocata
        k_info.Motiv = If(k_wire.motiv, String.Empty)
        k_info.ReceptiiRSterse = k_wire.receptii_r_sterse
        Return k_info
    End Function

End Class
