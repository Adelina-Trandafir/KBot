Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice AV01-09 - the journals. Wire field names exactly as routes/avacont/configurari/jurnale/jurnale.py.
Partial Public Class ApiClient
    Implements IJurnaleApi

    Private NotInheritable Class JurnalWire
        Public Property idjurnal As Integer
        Public Property sursasector As String
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class SursaWire
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class JurnaleResponse
        Public Property jurnale As List(Of JurnalWire)
        Public Property surse As List(Of SursaWire)
    End Class

    Private NotInheritable Class JurnalRand
        Public Property idjurnal As Integer?
        Public Property sursasector As String
        Public Property cod As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class JurnaleSaveRequest
        Public Property randuri As List(Of JurnalRand)
        Public Property sterse As List(Of Integer)
    End Class

    Public Async Function GetJurnaleAsync(ct As CancellationToken) As Task(Of JurnaleCatalog) _
        Implements IJurnaleApi.GetJurnaleAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}/jurnale", Nothing, "citirea jurnalelor", ct).ConfigureAwait(False)
            Dim k_payload As JurnaleResponse = JsonSerializer.Deserialize(Of JurnaleResponse)(k_resp, _json)
            If k_payload Is Nothing Then Return New JurnaleCatalog()
            Return New JurnaleCatalog() With {
                .Jurnale = If(k_payload.jurnale, New List(Of JurnalWire)()).Select(
                    Function(k_j) New JurnalInfo() With {
                        .IdJurnal = k_j.idjurnal, .SursaSector = If(k_j.sursasector, String.Empty),
                        .Cod = If(k_j.cod, String.Empty), .Denumire = If(k_j.denumire, String.Empty)}).ToList(),
                .Surse = If(k_payload.surse, New List(Of SursaWire)()).Select(
                    Function(k_s) New ClsfCont() With {.Cod = If(k_s.cod, String.Empty), .Denumire = If(k_s.denumire, String.Empty)}).ToList()}
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetJurnaleAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveJurnaleAsync(k_randuri As IReadOnlyList(Of JurnalInfo), k_sterse As IReadOnlyList(Of Integer), ct As CancellationToken) As Task _
        Implements IJurnaleApi.SaveJurnaleAsync
        Try
            ArgumentNullException.ThrowIfNull(k_randuri)
            ArgumentNullException.ThrowIfNull(k_sterse)
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/jurnale",
                New JurnaleSaveRequest() With {
                    .randuri = k_randuri.Select(
                        Function(k_j) New JurnalRand() With {
                            .idjurnal = If(k_j.IdJurnal > 0, CType(k_j.IdJurnal, Integer?), Nothing), .sursasector = k_j.SursaSector,
                            .cod = k_j.Cod, .denumire = k_j.Denumire}).ToList(),
                    .sterse = k_sterse.ToList()},
                "salvarea jurnalelor", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveJurnaleAsync", ex)
            Throw
        End Try
    End Function

End Class
