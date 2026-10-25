Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice AV01-08 - the opening balances of the partners. Wire field names exactly as routes/avacont/configurari/plan_cont/solduri_parteneri.py.
Partial Public Class ApiClient

    Private NotInheritable Class ContDisponibilWire
        Public Property idcont As Integer
        Public Property simbol As String
        Public Property denumire As String
        Public Property capitol As String
    End Class

    Private NotInheritable Class ConturiDisponibileResponse
        Public Property conturi As List(Of ContDisponibilWire)
    End Class

    Private NotInheritable Class SoldPartenerWire
        Public Property idcont As Integer
        Public Property simbol As String
        Public Property denumire As String
        Public Property capitol As String
        Public Property sold_debitor As Double
        Public Property sold_creditor As Double
        Public Property rulaj_debitor As Double
        Public Property rulaj_creditor As Double
    End Class

    Private NotInheritable Class SolduriPartenerResponse
        Public Property solduri As List(Of SoldPartenerWire)
    End Class

    Private NotInheritable Class SoldPartenerRand
        Public Property idcont As Integer
        Public Property sold_debitor As Decimal
        Public Property sold_creditor As Decimal
        Public Property rulaj_debitor As Decimal
        Public Property rulaj_creditor As Decimal
    End Class

    Private NotInheritable Class SolduriPartenerSaveRequest
        Public Property partener As Integer
        Public Property an As Integer
        Public Property randuri As List(Of SoldPartenerRand)
    End Class

    Public Async Function GetConturiDisponibileAsync(ct As CancellationToken) As Task(Of IReadOnlyList(Of ContDisponibil)) _
        Implements IConturiApi.GetConturiDisponibileAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{AvacontRoot}/parteneri/conturi", Nothing, "citirea conturilor", ct).ConfigureAwait(False)
            Dim k_payload As ConturiDisponibileResponse = JsonSerializer.Deserialize(Of ConturiDisponibileResponse)(k_resp, _json)
            If k_payload Is Nothing OrElse k_payload.conturi Is Nothing Then Return New List(Of ContDisponibil)()
            Return k_payload.conturi.Select(
                Function(k_c) New ContDisponibil() With {
                    .IdCont = k_c.idcont, .Simbol = If(k_c.simbol, String.Empty),
                    .Denumire = If(k_c.denumire, String.Empty), .Capitol = If(k_c.capitol, String.Empty)}).ToList()
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetConturiDisponibileAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetSolduriPartenerAsync(k_idPartener As Integer, k_an As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of SoldPartener)) _
        Implements IConturiApi.GetSolduriPartenerAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get,
                $"{AvacontRoot}/parteneri/solduri?partener={k_idPartener.ToString(CultureInfo.InvariantCulture)}&an={k_an.ToString(CultureInfo.InvariantCulture)}",
                Nothing, "citirea soldurilor partenerului", ct).ConfigureAwait(False)
            Dim k_payload As SolduriPartenerResponse = JsonSerializer.Deserialize(Of SolduriPartenerResponse)(k_resp, _json)
            If k_payload Is Nothing OrElse k_payload.solduri Is Nothing Then Return New List(Of SoldPartener)()
            Return k_payload.solduri.Select(
                Function(k_s) New SoldPartener() With {
                    .IdCont = k_s.idcont, .Simbol = If(k_s.simbol, String.Empty), .Denumire = If(k_s.denumire, String.Empty),
                    .Capitol = If(k_s.capitol, String.Empty),
                    .SoldDebitor = CDec(k_s.sold_debitor), .SoldCreditor = CDec(k_s.sold_creditor),
                    .RulajDebitor = CDec(k_s.rulaj_debitor), .RulajCreditor = CDec(k_s.rulaj_creditor)}).ToList()
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSolduriPartenerAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveSolduriPartenerAsync(k_idPartener As Integer, k_an As Integer, k_randuri As IReadOnlyList(Of SoldPartener), ct As CancellationToken) As Task _
        Implements IConturiApi.SaveSolduriPartenerAsync
        Try
            ArgumentNullException.ThrowIfNull(k_randuri)
            Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{AvacontRoot}/parteneri/solduri",
                New SolduriPartenerSaveRequest() With {
                    .partener = k_idPartener, .an = k_an,
                    .randuri = k_randuri.Select(
                        Function(k_r) New SoldPartenerRand() With {
                            .idcont = k_r.IdCont, .sold_debitor = k_r.SoldDebitor, .sold_creditor = k_r.SoldCreditor,
                            .rulaj_debitor = k_r.RulajDebitor, .rulaj_creditor = k_r.RulajCreditor}).ToList()},
                "salvarea soldurilor partenerului", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveSolduriPartenerAsync", ex)
            Throw
        End Try
    End Function

End Class
