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

' Slice AV01-11 - the revenue classifications. Wire field names exactly as routes/avacont/configurari/clasificatii_venituri/clasificatii_venituri.py.
' The budget/corrections bodies have the shape of the expenses window's (ApiClient.Nomenclatoare.vb), so its wire classes and ReadBudget are reused.
Partial Public Class ApiClient
    Implements IClasificatiiVenituriApi

    Private Const ClsfVRoot As String = AvacontRoot & "/clasificatii-venituri"

    Private NotInheritable Class ClsfVItemWire
        Public Property idclsfv As Integer
        Public Property ss As String
        Public Property capitol As String
        Public Property subcapitol As String
        Public Property paragraf As String
        Public Property denumire As String
    End Class

    Private NotInheritable Class ClsfVNamesWire
        Public Property capitol As Dictionary(Of String, String)
        Public Property subcapitol As Dictionary(Of String, String)
    End Class

    Private NotInheritable Class ClsfVTreeResponse
        Public Property items As List(Of ClsfVItemWire)
        Public Property names As ClsfVNamesWire
    End Class

    Private NotInheritable Class ClsfVCodeWire
        Public Property clsfv As String
        Public Property capitol As String
        Public Property subcapitol As String
        Public Property paragraf As String
        Public Property denumire As String
        Public Property exista As Boolean
    End Class

    Private NotInheritable Class ClsfVNomenclatorResponse
        Public Property items As List(Of ClsfVCodeWire)
    End Class

    Private NotInheritable Class ClsfVAddRequest
        Public Property clsfv As String
        Public Property ss As String
    End Class

    Private NotInheritable Class ClsfVChildrenWire
        Public Property parinte As String
        Public Property copii As List(Of String)
    End Class

    Private NotInheritable Class ClsfVAddResponse
        Public Property idclsfv As Integer
    End Class

    Private NotInheritable Class ClsfVSummaryRowWire
        Public Property idclsfv As Integer
        Public Property budget As BudgetVersionWire
        Public Property corrections As CorrectionWire
    End Class

    Private NotInheritable Class ClsfVSummaryResponse
        Public Property items As List(Of ClsfVSummaryRowWire)
        Public Property copii As ClsfVChildrenWire
    End Class

    Private NotInheritable Class ClsfVBudgetRequest
        Public Property an As Integer
        Public Property ss As String
        Public Property budgets As List(Of BudgetVersionWire)
        Public Property deleted_budgets As List(Of Integer)
        Public Property corrections As List(Of CorrectionWire)
        Public Property deleted As List(Of Integer)
    End Class

    Public Async Function GetClasificatiiVenituriAsync(k_ss As String, ct As CancellationToken) As Task(Of ClasificatiiVenituriCatalog) _
        Implements IClasificatiiVenituriApi.GetClasificatiiVenituriAsync
        Try
            Dim k_url As String = If(String.IsNullOrWhiteSpace(k_ss), ClsfVRoot, $"{ClsfVRoot}?ss={Uri.EscapeDataString(k_ss)}")
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, k_url, Nothing, "citirea clasificațiilor de venituri", ct).ConfigureAwait(False)
            Dim k_payload As ClsfVTreeResponse = JsonSerializer.Deserialize(Of ClsfVTreeResponse)(k_resp, _json)
            Dim k_result As New ClasificatiiVenituriCatalog()
            If k_payload Is Nothing Then Return k_result
            k_result.Items = If(k_payload.items, New List(Of ClsfVItemWire)()).Select(
                Function(k_w) New ClasificatieVenit() With {
                    .IdClsfV = k_w.idclsfv, .Ss = If(k_w.ss, String.Empty),
                    .Capitol = If(k_w.capitol, String.Empty), .Subcapitol = If(k_w.subcapitol, String.Empty),
                    .Paragraf = If(k_w.paragraf, String.Empty), .Denumire = If(k_w.denumire, String.Empty)}).ToList()
            If k_payload.names IsNot Nothing Then
                If k_payload.names.capitol IsNot Nothing Then k_result.CapitolNames = k_payload.names.capitol
                If k_payload.names.subcapitol IsNot Nothing Then k_result.SubcapitolNames = k_payload.names.subcapitol
            End If
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetClasificatiiVenituriAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetNomenclatorVenituriAsync(k_ss As String, ct As CancellationToken) As Task(Of IReadOnlyList(Of CodVenitNomenclator)) _
        Implements IClasificatiiVenituriApi.GetNomenclatorVenituriAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, $"{ClsfVRoot}/nomenclator?ss={Uri.EscapeDataString(If(k_ss, String.Empty))}", Nothing,
                "citirea nomenclatorului de venituri", ct).ConfigureAwait(False)
            Dim k_payload As ClsfVNomenclatorResponse = JsonSerializer.Deserialize(Of ClsfVNomenclatorResponse)(k_resp, _json)
            If k_payload Is Nothing Then Return New List(Of CodVenitNomenclator)()
            Return If(k_payload.items, New List(Of ClsfVCodeWire)()).Select(
                Function(k_w) New CodVenitNomenclator() With {
                    .ClsfV = If(k_w.clsfv, String.Empty), .Capitol = If(k_w.capitol, String.Empty),
                    .Subcapitol = If(k_w.subcapitol, String.Empty), .Paragraf = If(k_w.paragraf, String.Empty),
                    .Denumire = If(k_w.denumire, String.Empty), .Exista = k_w.exista}).ToList()
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetNomenclatorVenituriAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function AddClasificatieVenitAsync(k_clsfv As String, k_ss As String, ct As CancellationToken) As Task(Of Integer) _
        Implements IClasificatiiVenituriApi.AddClasificatieVenitAsync
        Try
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{ClsfVRoot}/adauga", New ClsfVAddRequest() With {.clsfv = k_clsfv, .ss = k_ss},
                "adăugarea clasificației de venituri", ct).ConfigureAwait(False)
            Dim k_payload As ClsfVAddResponse = JsonSerializer.Deserialize(Of ClsfVAddResponse)(k_resp, _json)
            Return If(k_payload Is Nothing, 0, k_payload.idclsfv)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.AddClasificatieVenitAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DeleteClasificatieVenitAsync(k_idClsfV As Integer, ct As CancellationToken) As Task _
        Implements IClasificatiiVenituriApi.DeleteClasificatieVenitAsync
        Try
            Await SendNomenclatoareAsync(
                HttpMethod.Delete, $"{ClsfVRoot}/{k_idClsfV.ToString(CultureInfo.InvariantCulture)}", Nothing,
                "ștergerea clasificației de venituri", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DeleteClasificatieVenitAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetSumarVenituriAsync(k_an As Integer, k_ss As String, ct As CancellationToken) _
        As Task(Of VenitSumarUnitate) Implements IClasificatiiVenituriApi.GetSumarVenituriAsync
        Try
            Dim k_url As String = $"{ClsfVRoot}/sumar?an={k_an.ToString(CultureInfo.InvariantCulture)}&ss={Uri.EscapeDataString(If(k_ss, String.Empty))}"
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, k_url, Nothing, "citirea sumarului de buget", ct).ConfigureAwait(False)
            Dim k_payload As ClsfVSummaryResponse = JsonSerializer.Deserialize(Of ClsfVSummaryResponse)(k_resp, _json)
            Dim k_rows As New List(Of VenitBugetSumar)()
            Dim k_result As New VenitSumarUnitate() With {.Rows = k_rows}
            If k_payload Is Nothing Then Return k_result
            If k_payload.copii IsNot Nothing AndAlso k_payload.copii.copii IsNot Nothing AndAlso k_payload.copii.copii.Count > 0 Then
                k_result.Child = New ChildSourceInfo() With {.Parinte = If(k_payload.copii.parinte, String.Empty), .Copii = k_payload.copii.copii}
            End If
            For Each k_w As ClsfVSummaryRowWire In If(k_payload.items, New List(Of ClsfVSummaryRowWire)())
                Dim k_row As New VenitBugetSumar() With {.IdClsfV = k_w.idclsfv}
                If k_w.budget IsNot Nothing Then
                    Dim k_start As Date
                    Dim k_hasStart As Boolean = Date.TryParseExact(If(k_w.budget.data_inceput, String.Empty), "yyyy-MM-dd",
                                                                   CultureInfo.InvariantCulture, DateTimeStyles.None, k_start)
                    k_row.LastBudget = New BudgetVersion() With {
                        .Id = k_w.budget.id,
                        .StartDate = If(k_hasStart, k_start, CType(Nothing, Date?)),
                        .Amounts = New QuarterAmounts() With {
                            .Trim1 = ToDecimal(k_w.budget.trim1), .Trim2 = ToDecimal(k_w.budget.trim2),
                            .Trim3 = ToDecimal(k_w.budget.trim3), .Trim4 = ToDecimal(k_w.budget.trim4)}}
                End If
                If k_w.corrections IsNot Nothing Then
                    k_row.CorrectionsTotal = New QuarterAmounts() With {
                        .Trim1 = ToDecimal(k_w.corrections.trim1), .Trim2 = ToDecimal(k_w.corrections.trim2),
                        .Trim3 = ToDecimal(k_w.corrections.trim3), .Trim4 = ToDecimal(k_w.corrections.trim4)}
                End If
                k_rows.Add(k_row)
            Next
            Return k_result
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetSumarVenituriAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetBugetVenitAsync(k_idClsfV As Integer, k_an As Integer, k_ss As String, ct As CancellationToken) _
        As Task(Of BugetClasificatie) Implements IClasificatiiVenituriApi.GetBugetVenitAsync
        Try
            Dim k_url As String = $"{ClsfVRoot}/{k_idClsfV.ToString(CultureInfo.InvariantCulture)}/buget" &
                                  $"?an={k_an.ToString(CultureInfo.InvariantCulture)}&ss={Uri.EscapeDataString(If(k_ss, String.Empty))}"
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Get, k_url, Nothing, "citirea bugetului", ct).ConfigureAwait(False)
            Return ReadBudget(k_resp)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetBugetVenitAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SaveBugetVenitAsync(k_idClsfV As Integer, k_an As Integer, k_ss As String,
                                              k_budgets As IReadOnlyList(Of BudgetVersion), k_deletedBudgetIds As IReadOnlyList(Of Integer),
                                              k_corrections As IReadOnlyList(Of RectificareBugetara), k_deletedIds As IReadOnlyList(Of Integer),
                                              ct As CancellationToken) As Task(Of BugetClasificatie) _
        Implements IClasificatiiVenituriApi.SaveBugetVenitAsync
        Try
            ArgumentNullException.ThrowIfNull(k_budgets)
            ArgumentNullException.ThrowIfNull(k_corrections)
            Dim k_request As New ClsfVBudgetRequest() With {
                .an = k_an,
                .ss = k_ss,
                .budgets = k_budgets.Select(Function(k_b) New BudgetVersionWire() With {
                    .id = k_b.Id,
                    .data_inceput = If(k_b.StartDate.HasValue, k_b.StartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Nothing),
                    .trim1 = ToDouble(k_b.Amounts.Trim1), .trim2 = ToDouble(k_b.Amounts.Trim2),
                    .trim3 = ToDouble(k_b.Amounts.Trim3), .trim4 = ToDouble(k_b.Amounts.Trim4)}).ToList(),
                .deleted_budgets = If(k_deletedBudgetIds Is Nothing, New List(Of Integer)(), k_deletedBudgetIds.ToList()),
                .corrections = k_corrections.Select(Function(k_c) New CorrectionWire() With {
                    .id = k_c.Id,
                    .document = k_c.Document,
                    .data = If(k_c.Data.HasValue, k_c.Data.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Nothing),
                    .trim1 = ToDouble(k_c.Amounts.Trim1), .trim2 = ToDouble(k_c.Amounts.Trim2),
                    .trim3 = ToDouble(k_c.Amounts.Trim3), .trim4 = ToDouble(k_c.Amounts.Trim4)}).ToList(),
                .deleted = If(k_deletedIds Is Nothing, New List(Of Integer)(), k_deletedIds.ToList())}
            Dim k_resp As String = Await SendNomenclatoareAsync(
                HttpMethod.Post, $"{ClsfVRoot}/{k_idClsfV.ToString(CultureInfo.InvariantCulture)}/buget", k_request,
                "salvarea bugetului", ct).ConfigureAwait(False)
            Return ReadBudget(k_resp)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SaveBugetVenitAsync", ex)
            Throw
        End Try
    End Function

End Class
