Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Slice 0123-02 -- /api/vercon/...: PYTHON/routes/vercon/ (the same routes the site uses; kbot_bridge.py lets the bearer token reach the calls below).
' The wire is read with JsonDocument by the server's own names. Every method is a boundary: log + rethrow (an ApiException keeps the server's
' Romanian text and reason).
Partial Public Class ApiClient
    Implements IVerconApi

    Private Const VerconUnitHeader As String = "X-Vercon-Unit"

    ' The query of a call: the parts of the choice that are made, then the report's own parameters.
    Private Shared Function VerconQuery(k_selection As VerconSelection, k_parameters As IReadOnlyDictionary(Of String, String)) As String
        Dim k_query As New StringBuilder()
        Dim k_add As Action(Of String, String) =
            Sub(k_key As String, k_value As String)
                k_query.Append(If(k_query.Length = 0, "?", "&")).Append(Uri.EscapeDataString(k_key)).Append("="c).Append(Uri.EscapeDataString(k_value))
            End Sub
        If k_selection IsNot Nothing Then
            If k_selection.Subunit.HasValue Then k_add("subunit", k_selection.Subunit.Value.ToString(CultureInfo.InvariantCulture))
            If k_selection.Gestiune.HasValue Then k_add("gestiune", k_selection.Gestiune.Value.ToString(CultureInfo.InvariantCulture))
            If k_selection.Year.HasValue Then k_add("year", k_selection.Year.Value.ToString(CultureInfo.InvariantCulture))
        End If
        If k_parameters IsNot Nothing Then
            For Each k_pair As KeyValuePair(Of String, String) In k_parameters
                k_add(k_pair.Key, k_pair.Value)
            Next
        End If
        Return k_query.ToString()
    End Function

    Private Async Function SendVerconAsync(k_method As HttpMethod, k_path As String, k_what As String, ct As CancellationToken) As Task(Of Byte())
        EnsureConfigured()
        Using msg As New HttpRequestMessage(k_method, k_path)
            msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
            msg.Headers.TryAddWithoutValidation(VerconUnitHeader, _session.DbName)
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim k_bytes As Byte() = Await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(False)
                If Not resp.IsSuccessStatusCode Then
                    Throw BuildApiException(Encoding.UTF8.GetString(k_bytes), k_what, CInt(resp.StatusCode))
                End If
                Return k_bytes
            End Using
        End Using
    End Function

    Private Async Function SendVerconJsonAsync(k_path As String, k_what As String, ct As CancellationToken) As Task(Of JsonDocument)
        Dim k_bytes As Byte() = Await SendVerconAsync(HttpMethod.Get, k_path, k_what, ct).ConfigureAwait(False)
        Return JsonDocument.Parse(k_bytes)
    End Function

    ' An id the server writes as a number or as a text (documents are keyed by number, tree nodes by text).
    Private Shared Function JsonIdText(k_element As JsonElement, k_name As String) As String
        Dim k_value As JsonElement
        If Not k_element.TryGetProperty(k_name, k_value) Then Return String.Empty
        Select Case k_value.ValueKind
            Case JsonValueKind.String
                Return If(k_value.GetString(), String.Empty)
            Case JsonValueKind.Number
                Return k_value.GetRawText()
            Case Else
                Return String.Empty
        End Select
    End Function

    Private Shared Function JsonOptionalInt(k_element As JsonElement, k_name As String) As Integer?
        Dim k_value As JsonElement
        If Not k_element.TryGetProperty(k_name, k_value) Then Return Nothing
        Select Case k_value.ValueKind
            Case JsonValueKind.Number
                Return k_value.GetInt32()
            Case JsonValueKind.String
                Dim k_parsed As Integer
                Return If(Integer.TryParse(k_value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_parsed), CType(k_parsed, Integer?), Nothing)
            Case Else
                Return Nothing
        End Select
    End Function

    Private Shared Sub JsonTexts(k_element As JsonElement, k_name As String, k_target As List(Of String))
        Dim k_list As JsonElement
        If Not k_element.TryGetProperty(k_name, k_list) OrElse k_list.ValueKind <> JsonValueKind.Array Then Return
        For Each k_item As JsonElement In k_list.EnumerateArray()
            If k_item.ValueKind = JsonValueKind.String Then k_target.Add(k_item.GetString())
        Next
    End Sub

    Public Async Function GetVerconContextAsync(k_selection As VerconSelection, ct As CancellationToken) As Task(Of VerconContext) Implements IVerconApi.GetVerconContextAsync
        Try
            Dim k_path As String = "/api/vercon/context" & VerconQuery(k_selection, Nothing)
            Using k_doc As JsonDocument = Await SendVerconJsonAsync(k_path, "citirea contextului VERCON", ct).ConfigureAwait(False)
                Dim k_root As JsonElement = k_doc.RootElement
                Dim k_result As New VerconContext() With {.Ready = JsonFlag(k_root, "ready")}
                Dim k_node As JsonElement
                If k_root.TryGetProperty("selection", k_node) AndAlso k_node.ValueKind = JsonValueKind.Object Then
                    k_result.Selection = New VerconSelection() With {
                        .Subunit = JsonOptionalInt(k_node, "subunit"), .Gestiune = JsonOptionalInt(k_node, "gestiune"),
                        .Year = JsonOptionalInt(k_node, "year"), .GestiuneType = JsonText(k_node, "type")}
                End If
                If k_root.TryGetProperty("options", k_node) AndAlso k_node.ValueKind = JsonValueKind.Object Then
                    Dim k_list As JsonElement
                    If k_node.TryGetProperty("subunits", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                        For Each k_item As JsonElement In k_list.EnumerateArray()
                            k_result.Subunits.Add(New VerconSubunit() With {.Id = JsonInt(k_item, "id"), .Name = JsonText(k_item, "name")})
                        Next
                    End If
                    If k_node.TryGetProperty("gestiuni", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                        For Each k_item As JsonElement In k_list.EnumerateArray()
                            k_result.Gestiuni.Add(New VerconGestiune() With {
                                .Id = JsonInt(k_item, "id"), .Name = JsonText(k_item, "name"), .GestiuneType = JsonText(k_item, "type")})
                        Next
                    End If
                    If k_node.TryGetProperty("years", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                        For Each k_item As JsonElement In k_list.EnumerateArray()
                            If k_item.ValueKind = JsonValueKind.Number Then k_result.Years.Add(k_item.GetInt32())
                        Next
                    End If
                End If
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetVerconContextAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetVerconCatalogAsync(k_selection As VerconSelection, ct As CancellationToken) As Task(Of VerconCatalog) Implements IVerconApi.GetVerconCatalogAsync
        Try
            Dim k_path As String = "/api/vercon/reports/catalog" & VerconQuery(k_selection, Nothing)
            Using k_doc As JsonDocument = Await SendVerconJsonAsync(k_path, "citirea rapoartelor VERCON", ct).ConfigureAwait(False)
                Dim k_root As JsonElement = k_doc.RootElement
                Dim k_result As New VerconCatalog() With {
                    .LatestYear = JsonInt(k_root, "latest_year"), .DefaultStart = JsonText(k_root, "default_start"), .DefaultEnd = JsonText(k_root, "default_end")}
                Dim k_list As JsonElement
                If k_root.TryGetProperty("reports", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        Dim k_spec As New VerconReportSpec() With {
                            .Id = JsonText(k_item, "id"), .Label = JsonText(k_item, "label"), .Kind = JsonText(k_item, "kind"),
                            .Group = JsonText(k_item, "group"), .TreeGroup = JsonText(k_item, "tree_group"), .Family = JsonText(k_item, "family"), .Ledger = JsonText(k_item, "ledger"),
                            .MfSelection = JsonText(k_item, "mf_selection"), .DocumentOptions = JsonFlag(k_item, "document_options"),
                            .ReceiverOptions = JsonFlag(k_item, "receiver_options"), .OutgoingSelection = JsonFlag(k_item, "outgoing_selection"),
                            .IncomingSelection = JsonFlag(k_item, "incoming_selection"), .OutgoingFilters = JsonFlag(k_item, "outgoing_filters"),
                            .VariantRequired = JsonFlag(k_item, "variant_required"), .SupplierRequired = JsonFlag(k_item, "supplier_required"),
                            .DocumentRequired = JsonFlag(k_item, "document_required")}
                        JsonTexts(k_item, "types", k_spec.Types)
                        JsonTexts(k_item, "filters", k_spec.Filters)
                        k_result.Reports.Add(k_spec)
                    Next
                End If
                If k_root.TryGetProperty("periods", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        k_result.Periods.Add(New VerconPeriod() With {
                            .Year = JsonInt(k_item, "year"), .MinDate = JsonText(k_item, "min"), .MaxDate = JsonText(k_item, "max"),
                            .LastMovementMonth = JsonInt(k_item, "last_movement_month")})
                    Next
                End If
                If k_root.TryGetProperty("years", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        If k_item.ValueKind = JsonValueKind.Number Then k_result.Years.Add(k_item.GetInt32())
                    Next
                End If
                Dim k_all As JsonElement
                If k_root.TryGetProperty("all_years", k_all) AndAlso k_all.ValueKind = JsonValueKind.Object Then
                    k_result.AllYearsStart = JsonText(k_all, "start")
                    k_result.AllYearsEnd = JsonText(k_all, "end")
                End If
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetVerconCatalogAsync", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadVerconNodes(k_array As JsonElement, k_target As List(Of VerconSelNode))
        For Each k_item As JsonElement In k_array.EnumerateArray()
            Dim k_node As New VerconSelNode() With {.Id = JsonIdText(k_item, "id"), .Label = JsonIdText(k_item, "label"), .Lazy = JsonFlag(k_item, "lazy")}
            Dim k_children As JsonElement
            If k_item.TryGetProperty("children", k_children) AndAlso k_children.ValueKind = JsonValueKind.Array Then
                ReadVerconNodes(k_children, k_node.Children)
            End If
            k_target.Add(k_node)
        Next
    End Sub

    Public Async Function GetVerconSelectionAsync(k_selection As VerconSelection, k_report As String, k_parameters As IReadOnlyDictionary(Of String, String),
                                                  ct As CancellationToken) As Task(Of VerconBranch) Implements IVerconApi.GetVerconSelectionAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_report)
            Dim k_path As String = "/api/vercon/reports/" & Uri.EscapeDataString(k_report) & "/selection" & VerconQuery(k_selection, k_parameters)
            Using k_doc As JsonDocument = Await SendVerconJsonAsync(k_path, "citirea selecției raportului VERCON", ct).ConfigureAwait(False)
                Dim k_result As New VerconBranch()
                Dim k_list As JsonElement
                If k_doc.RootElement.TryGetProperty("nodes", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    ReadVerconNodes(k_list, k_result.Nodes)
                End If
                If k_doc.RootElement.TryGetProperty("documents", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        k_result.Documents.Add(New VerconSelDocument() With {.Id = JsonIdText(k_item, "id"), .Label = JsonIdText(k_item, "label")})
                    Next
                End If
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetVerconSelectionAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GenerateVerconReportAsync(k_selection As VerconSelection, k_report As String, k_parameters As IReadOnlyDictionary(Of String, String),
                                                    ct As CancellationToken) As Task(Of VerconReportHandle) Implements IVerconApi.GenerateVerconReportAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_report)
            Dim k_path As String = "/api/vercon/reports/" & Uri.EscapeDataString(k_report) & "/generate" & VerconQuery(k_selection, k_parameters)
            Using k_doc As JsonDocument = Await SendVerconJsonAsync(k_path, "pregătirea raportului VERCON", ct).ConfigureAwait(False)
                Return New VerconReportHandle() With {
                    .Snapshot = JsonText(k_doc.RootElement, "snapshot"), .Title = JsonText(k_doc.RootElement, "title"),
                    .FileName = JsonText(k_doc.RootElement, "filename")}
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GenerateVerconReportAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DownloadVerconReportAsync(k_selection As VerconSelection, k_snapshot As String, k_format As String, ct As CancellationToken) _
        As Task(Of Byte()) Implements IVerconApi.DownloadVerconReportAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_snapshot)
            If k_format <> "pdf" AndAlso k_format <> "xlsx" Then Throw New ArgumentException($"Unknown report format '{k_format}'.", NameOf(k_format))
            Dim k_path As String = "/api/vercon/reports/export/" & Uri.EscapeDataString(k_snapshot) & "/" & k_format & VerconQuery(k_selection, Nothing)
            Return Await SendVerconAsync(HttpMethod.Get, k_path, "descărcarea raportului VERCON", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DownloadVerconReportAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function ReleaseVerconReportAsync(k_selection As VerconSelection, k_snapshot As String, ct As CancellationToken) As Task Implements IVerconApi.ReleaseVerconReportAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_snapshot)
            Dim k_path As String = "/api/vercon/reports/export/" & Uri.EscapeDataString(k_snapshot) & VerconQuery(k_selection, Nothing)
            Await SendVerconAsync(HttpMethod.Delete, k_path, "eliberarea raportului VERCON", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ReleaseVerconReportAsync", ex)
            Throw
        End Try
    End Function

End Class
