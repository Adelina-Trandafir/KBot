Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 01EF-03 -- /api/efactura/arhiva and /api/efactura/primite/{id}/arhiva: PYTHON/routes/efactura/primite_arhiva_routes.py.
' Wire names are exactly the server's. Every method is a boundary: log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient

    Private NotInheritable Class ArhivaRandWire
        Public Property IdPrimita As Integer
        Public Property DataArhivare As String
        Public Property UN As String
        Public Property Motiv As String
    End Class

    Private NotInheritable Class ArhivaListaWire
        Public Property arhivate As List(Of ArhivaRandWire)
    End Class

    Private Shared Function ToArhivaRand(w As ArhivaRandWire) As EFacturaArhivaRand
        Return New EFacturaArhivaRand() With {
            .IdPrimita = w.IdPrimita, .DataArhivare = ParseDay(w.DataArhivare), .Un = If(w.UN, String.Empty), .Motiv = If(w.Motiv, String.Empty)}
    End Function

    Public Async Function GetArhivaAsync(ct As CancellationToken) _
        As Task(Of List(Of EFacturaArhivaRand)) Implements IEFacturaApi.GetArhivaAsync
        Try
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/arhiva", Nothing,
                                                           "citirea facturilor din arhivă", ct).ConfigureAwait(False)
            Dim wire As ArhivaListaWire = JsonSerializer.Deserialize(Of ArhivaListaWire)(k_text, _json)
            Dim k_result As New List(Of EFacturaArhivaRand)()
            If wire Is Nothing OrElse wire.arhivate Is Nothing Then Return k_result
            For Each w As ArhivaRandWire In wire.arhivate
                k_result.Add(ToArhivaRand(w))
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetArhivaAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function ArchiveAsync(k_idPrimita As Integer, k_motiv As String, ct As CancellationToken) _
        As Task(Of EFacturaArhivaRand) Implements IEFacturaApi.ArchiveAsync
        Try
            Dim k_body As String = JsonSerializer.Serialize(New Dictionary(Of String, Object) From {
                {"motiv", If(String.IsNullOrWhiteSpace(k_motiv), Nothing, k_motiv.Trim())}}, _json)
            Dim k_text As String = Await SendEFacturaAsync(HttpMethod.Post, PrimitaPath(k_idPrimita, "arhiva"), k_body,
                                                           "arhivarea facturii", ct).ConfigureAwait(False)
            Dim wire As ArhivaRandWire = JsonSerializer.Deserialize(Of ArhivaRandWire)(k_text, _json)
            If wire Is Nothing Then Throw New ApiException("Serverul nu a confirmat arhivarea facturii.")
            Return ToArhivaRand(wire)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ArchiveAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function UnarchiveAsync(k_idPrimita As Integer, ct As CancellationToken) _
        As Task Implements IEFacturaApi.UnarchiveAsync
        Try
            Await SendEFacturaAsync(HttpMethod.Delete, PrimitaPath(k_idPrimita, "arhiva"), Nothing,
                                    "scoaterea facturii din arhivă", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.UnarchiveAsync", ex)
            Throw
        End Try
    End Function

End Class
