Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0084-02 - the partners associated with a DDF (routes/forexe/ddf_parteneri.py).
Partial Public Class ApiClient
    Implements IDdfParteneriApi

    ' Wire shapes: field names exactly as ddf_parteneri.py writes / reads them.
    Private NotInheritable Class ParteneriAsociatiWire
        Public Property iddf As Integer
        Public Property adaugati As Integer
        Public Property parteneri As List(Of DdfDraftPartenerDto)
    End Class

    Private NotInheritable Class ParteneriAsociatiRequest
        Public Property cod_angajament As String
        Public Property parteneri As List(Of DdfDraftPartenerDto)
    End Class

    Public Async Function GetDdfParteneriAsociatiAsync(cod As String, ct As CancellationToken) _
        As Task(Of DdfParteneriAsociati) Implements IDdfParteneriApi.GetDdfParteneriAsociatiAsync

        Try
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))
            Dim respText As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"/api/forexe/ddf/parteneri-asociati?cod={Uri.EscapeDataString(cod)}",
                Nothing, "citirea partenerilor asociati", ct).ConfigureAwait(False)
            Return ReadParteneriAsociati(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetDdfParteneriAsociatiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function AdaugaDdfParteneriAsync(cod As String, parteneri As IEnumerable(Of DdfPartenerAsociat),
                                                  ct As CancellationToken) _
        As Task(Of DdfParteneriAsociati) Implements IDdfParteneriApi.AdaugaDdfParteneriAsync

        Try
            If String.IsNullOrWhiteSpace(cod) Then Throw New ArgumentException("cod gol.", NameOf(cod))
            ArgumentNullException.ThrowIfNull(parteneri)
            Dim body As New ParteneriAsociatiRequest() With {
                .cod_angajament = cod, .parteneri = New List(Of DdfDraftPartenerDto)()}
            For Each p As DdfPartenerAsociat In parteneri
                body.parteneri.Add(New DdfDraftPartenerDto() With {
                    .cod_fiscal = p.CodFiscal, .nume_partener = p.NumePartener, .din_antet = p.DinAntet})
            Next
            Dim respText As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, "/api/forexe/ddf/parteneri-asociati", body,
                "asocierea partenerilor", ct).ConfigureAwait(False)
            Return ReadParteneriAsociati(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.AdaugaDdfParteneriAsync", ex)
            Throw
        End Try
    End Function

    Private Shared Function ReadParteneriAsociati(respText As String) As DdfParteneriAsociati
        Dim payload As ParteneriAsociatiWire = JsonSerializer.Deserialize(Of ParteneriAsociatiWire)(respText, _json)
        Dim rezultat As New DdfParteneriAsociati()
        If payload Is Nothing Then Return rezultat
        rezultat.Iddf = payload.iddf
        rezultat.Adaugati = payload.adaugati
        If payload.parteneri IsNot Nothing Then
            For Each p As DdfDraftPartenerDto In payload.parteneri
                If p Is Nothing OrElse String.IsNullOrWhiteSpace(p.cod_fiscal) Then Continue For
                rezultat.Parteneri.Add(New DdfPartenerAsociat() With {
                    .CodFiscal = p.cod_fiscal, .NumePartener = If(p.nume_partener, String.Empty),
                    .DinAntet = p.din_antet})
            Next
        End If
        Return rezultat
    End Function
End Class
