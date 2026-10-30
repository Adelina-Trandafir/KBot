Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0084 - the «Operatiuni necorectate» of the FOREXE landing page, saved into FX_Operatiuni.
Partial Public Class ApiClient
    Implements IUncorrectedOperationsApi

    ' Wire shapes: field names exactly as routes/forexe/operatiuni.py reads / writes them.
    Private NotInheritable Class UncorrectedOperationWire
        Public Property angajament As String
        Public Property program As String
        Public Property ssi As String
        Public Property referinta_trezor As String
        Public Property nr_doc As String
        Public Property data_plata As String
        Public Property tip As String
        Public Property suma As Decimal?
        Public Property probleme As String
    End Class

    Private NotInheritable Class UncorrectedOperationsRequest
        Public Property operatiuni As List(Of UncorrectedOperationWire)
    End Class

    Private NotInheritable Class UncorrectedOperationsResponse
        Public Property primite As Integer
        Public Property inserate As Integer
        Public Property existente As Integer
        Public Property avertismente As List(Of String)
        Public Property noi As List(Of Integer)
    End Class

    Private NotInheritable Class UncorrelatedWire
        Public Property idfxp As Integer
        Public Property angajament As String
        Public Property program As String
        Public Property ss As String
        Public Property clsf_sal As String
        Public Property ssi As String
        Public Property referinta_trezor As String
        Public Property nr_doc As String
        Public Property data_plata As String
        Public Property tip As String
        Public Property suma As Decimal?
        Public Property probleme As String
    End Class

    Private NotInheritable Class UncorrelatedResponse
        Public Property operatiuni As List(Of UncorrelatedWire)
    End Class

    Private NotInheritable Class SkipRequest
        Public Property idfxp As List(Of Integer)
        Public Property omis As Boolean
    End Class

    Private NotInheritable Class SkipResponse
        Public Property actualizate As Integer
    End Class

    Public Async Function SaveUncorrectedOperationsAsync(rows As IReadOnlyList(Of UncorrectedOperation),
                                                         ct As CancellationToken) As Task(Of UncorrectedOperationsSaveResult) _
        Implements IUncorrectedOperationsApi.SaveUncorrectedOperationsAsync
        Try
            EnsureConfigured()
            ArgumentNullException.ThrowIfNull(rows)

            Dim request As New UncorrectedOperationsRequest() With {
                .operatiuni = rows.Select(Function(r) New UncorrectedOperationWire() With {
                    .angajament = If(String.IsNullOrWhiteSpace(r.Commitment), Nothing, r.Commitment),
                    .program = r.Program,
                    .ssi = r.Ssi,
                    .referinta_trezor = r.TreasuryReference,
                    .nr_doc = r.DocumentNumber,
                    .data_plata = If(r.PaymentDate.HasValue,
                                     r.PaymentDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                     Nothing),
                    .tip = r.Kind,
                    .suma = r.Amount,
                    .probleme = If(String.IsNullOrWhiteSpace(r.Problems), Nothing, r.Problems)
                }).ToList()
            }

            Using msg As New HttpRequestMessage(HttpMethod.Post, "/api/forexe/operatiuni/necorectate")
                msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
                Dim body As String = JsonSerializer.Serialize(request, _json)
                msg.Content = New StringContent(body, Encoding.UTF8, "application/json")
                Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                    Dim respText As String = Await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(False)
                    If Not resp.IsSuccessStatusCode Then
                        Throw BuildApiException(respText, "salvarea operațiunilor necorectate", CInt(resp.StatusCode))
                    End If
                    Dim payload As UncorrectedOperationsResponse =
                        JsonSerializer.Deserialize(Of UncorrectedOperationsResponse)(respText, _json)
                    If payload Is Nothing Then
                        Throw New ApiException("Serverul nu a întors rezultatul salvării.", CInt(resp.StatusCode))
                    End If
                    Return New UncorrectedOperationsSaveResult() With {
                        .Received = payload.primite,
                        .Inserted = payload.inserate,
                        .AlreadySaved = payload.existente,
                        .Warnings = If(payload.avertismente, New List(Of String)()),
                        .NewIds = If(payload.noi, New List(Of Integer)())
                    }
                End Using
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveUncorrectedOperationsAsync", ex)
            Throw
        End Try
    End Function

    Public Function GetUncorrelatedOperationsAsync(ct As CancellationToken) As Task(Of List(Of UncorrectedOperation)) _
        Implements IUncorrectedOperationsApi.GetUncorrelatedOperationsAsync
        Return ReadUncorrelatedAsync("/api/forexe/operatiuni/necorelate", "citirea operațiunilor necorelate", ct)
    End Function

    ' Slice 0088-06: the omitted ones, shown only by «Operațiuni omise».
    Public Function GetSkippedOperationsAsync(ct As CancellationToken) As Task(Of List(Of UncorrectedOperation)) _
        Implements IUncorrectedOperationsApi.GetSkippedOperationsAsync
        Return ReadUncorrelatedAsync("/api/forexe/operatiuni/necorelate?omise=1", "citirea operațiunilor omise", ct)
    End Function

    Private Async Function ReadUncorrelatedAsync(url As String, action As String, ct As CancellationToken) As Task(Of List(Of UncorrectedOperation))
        Try
            Dim text As String = Await SendCabAsync(HttpMethod.Get, url, Nothing, action, ct).ConfigureAwait(False)
            Dim payload As UncorrelatedResponse = JsonSerializer.Deserialize(Of UncorrelatedResponse)(text, _json)
            Dim result As New List(Of UncorrectedOperation)()
            If payload Is Nothing OrElse payload.operatiuni Is Nothing Then Return result
            For Each w As UncorrelatedWire In payload.operatiuni
                Dim d As Date
                Dim paid As Date? = Nothing
                If Date.TryParseExact(If(w.data_plata, String.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, d) Then paid = d
                result.Add(New UncorrectedOperation() With {
                    .IdFxp = w.idfxp,
                    .Commitment = If(w.angajament, String.Empty),
                    .Program = If(w.program, String.Empty),
                    .Ssi = If(String.IsNullOrEmpty(w.ss), If(w.ssi, String.Empty), w.ss & "-" & If(w.clsf_sal, String.Empty)),
                    .SsiTitle = If(w.ssi, String.Empty),
                    .TreasuryReference = If(w.referinta_trezor, String.Empty),
                    .DocumentNumber = If(w.nr_doc, String.Empty),
                    .PaymentDate = paid,
                    .Kind = If(w.tip, String.Empty),
                    .Amount = w.suma,
                    .AmountText = If(w.suma.HasValue, w.suma.Value.ToString("N2", CultureInfo.GetCultureInfo("ro-RO")), String.Empty),
                    .Problems = If(w.probleme, String.Empty)})
            Next
            Return result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ReadUncorrelatedAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SetOperationsSkippedAsync(ids As IReadOnlyList(Of Integer), skipped As Boolean,
                                                    ct As CancellationToken) As Task(Of Integer) _
        Implements IUncorrectedOperationsApi.SetOperationsSkippedAsync
        Try
            ArgumentNullException.ThrowIfNull(ids)
            Dim body As String = JsonSerializer.Serialize(New SkipRequest() With {.idfxp = ids.ToList(), .omis = skipped}, _json)
            Dim text As String = Await SendCabAsync(HttpMethod.Post, "/api/forexe/operatiuni/omitere", body,
                                                    If(skipped, "omiterea operațiunilor", "reluarea operațiunilor"), ct).ConfigureAwait(False)
            Dim payload As SkipResponse = JsonSerializer.Deserialize(Of SkipResponse)(text, _json)
            Return If(payload Is Nothing, 0, payload.actualizate)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SetOperationsSkippedAsync", ex)
            Throw
        End Try
    End Function

End Class
