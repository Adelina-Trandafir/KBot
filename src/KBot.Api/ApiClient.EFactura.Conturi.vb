Option Strict On
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 00EF-12 -- /api/efactura/furnizor/conturi: PYTHON/routes/efactura/factura_routes.py. The unit's own IBANs.
' Every method is a boundary: log + rethrow (an ApiException keeps the server's Romanian text).
Partial Public Class ApiClient

    Private NotInheritable Class EFacturaContWire
        Public Property IdCont As Integer
        Public Property Cont As String
        Public Property Banca As String
    End Class

    ' What a save sends: only the account; the bank is the server's.
    Private NotInheritable Class EFacturaContSaveWire
        Public Property Cont As String
    End Class

    Private NotInheritable Class EFacturaConturiSaveWire
        Public Property conturi As List(Of EFacturaContSaveWire)
    End Class

    Private NotInheritable Class EFacturaConturiAnswerWire
        Public Property conturi As List(Of EFacturaContWire)
    End Class

    Public Async Function GetConturiAsync(ct As CancellationToken) _
        As Task(Of List(Of EFacturaCont)) Implements IEFacturaApi.GetConturiAsync
        Try
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Get, "/api/efactura/furnizor/conturi", Nothing,
                                                             "citirea conturilor unității emitente E-Factura", ct).ConfigureAwait(False)
            Return ToConturi(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetConturiAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveConturiAsync(k_conturi As IEnumerable(Of EFacturaCont), ct As CancellationToken) _
        As Task(Of List(Of EFacturaCont)) Implements IEFacturaApi.SaveConturiAsync
        Try
            ArgumentNullException.ThrowIfNull(k_conturi)
            Dim k_wire As New EFacturaConturiSaveWire() With {.conturi = New List(Of EFacturaContSaveWire)()}
            For Each k_item As EFacturaCont In k_conturi
                k_wire.conturi.Add(New EFacturaContSaveWire() With {.Cont = k_item.Cont})
            Next
            Dim respText As String = Await SendEFacturaAsync(HttpMethod.Put, "/api/efactura/furnizor/conturi",
                                                             JsonSerializer.Serialize(k_wire, _json),
                                                             "salvarea conturilor unității emitente E-Factura", ct).ConfigureAwait(False)
            Return ToConturi(respText)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveConturiAsync", ex)
            Throw
        End Try
    End Function

    Private Function ToConturi(k_respText As String) As List(Of EFacturaCont)
        Dim k_answer As EFacturaConturiAnswerWire = JsonSerializer.Deserialize(Of EFacturaConturiAnswerWire)(k_respText, _json)
        If k_answer Is Nothing Then Throw New ApiException("Serverul nu a trimis conturile unității.")
        Dim k_result As New List(Of EFacturaCont)()
        If k_answer.conturi Is Nothing Then Return k_result
        For Each k_w As EFacturaContWire In k_answer.conturi
            k_result.Add(New EFacturaCont() With {
                .IdCont = k_w.IdCont, .Cont = If(k_w.Cont, String.Empty), .Banca = If(k_w.Banca, String.Empty)})
        Next
        Return k_result
    End Function

End Class
