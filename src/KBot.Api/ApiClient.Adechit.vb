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

' Slice 0123-01 -- /api/adechit/...: PYTHON/routes/adechit/ (the same routes the site uses; kbot_bridge.py lets the bearer token reach
' the read calls below). The wire is read with JsonDocument by the server's own names (IDL, IDG, Grupa, ...). Every method is a boundary:
' log + rethrow (an ApiException keeps the server's Romanian text and reason).
Partial Public Class ApiClient
    Implements IAdechitApi

    Private Const AdechitUnitHeader As String = "X-Ade-Unit"
    Private Const AdechitSubunitHeader As String = "X-Ade-Subunit"

    ' One call to an ADECHIT route. The unit header is the session's unit; the subunit header is left out for the context call.
    Private Async Function SendAdechitAsync(k_method As HttpMethod, k_path As String, k_subunitId As Integer?,
                                            k_what As String, ct As CancellationToken) As Task(Of Byte())
        EnsureConfigured()
        Using msg As New HttpRequestMessage(k_method, k_path)
            msg.Headers.Authorization = New Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token)
            msg.Headers.TryAddWithoutValidation(AdechitUnitHeader, _session.DbName)
            If k_subunitId.HasValue Then
                msg.Headers.TryAddWithoutValidation(AdechitSubunitHeader, k_subunitId.Value.ToString(CultureInfo.InvariantCulture))
            End If
            Using resp As HttpResponseMessage = Await _http.SendAsync(msg, ct).ConfigureAwait(False)
                Dim k_bytes As Byte() = Await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(False)
                If Not resp.IsSuccessStatusCode Then
                    Throw BuildApiException(Encoding.UTF8.GetString(k_bytes), k_what, CInt(resp.StatusCode))
                End If
                Return k_bytes
            End Using
        End Using
    End Function

    Private Async Function SendAdechitJsonAsync(k_path As String, k_subunitId As Integer?, k_what As String, ct As CancellationToken) As Task(Of JsonDocument)
        Dim k_bytes As Byte() = Await SendAdechitAsync(HttpMethod.Get, k_path, k_subunitId, k_what, ct).ConfigureAwait(False)
        Return JsonDocument.Parse(k_bytes)
    End Function

    Private Shared Function JsonInt(k_element As JsonElement, k_name As String) As Integer
        Dim k_value As JsonElement
        If Not k_element.TryGetProperty(k_name, k_value) Then Return 0
        Select Case k_value.ValueKind
            Case JsonValueKind.Number
                Return k_value.GetInt32()
            Case JsonValueKind.String
                Dim k_parsed As Integer
                Return If(Integer.TryParse(k_value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, k_parsed), k_parsed, 0)
            Case Else
                Return 0
        End Select
    End Function

    Private Shared Function JsonText(k_element As JsonElement, k_name As String) As String
        Dim k_value As JsonElement
        If k_element.TryGetProperty(k_name, k_value) AndAlso k_value.ValueKind = JsonValueKind.String Then Return If(k_value.GetString(), String.Empty)
        Return String.Empty
    End Function

    ' The evidence keeps flags as true/false or 0/1 depending on the table.
    Private Shared Function JsonFlag(k_element As JsonElement, k_name As String) As Boolean
        Dim k_value As JsonElement
        If Not k_element.TryGetProperty(k_name, k_value) Then Return False
        Select Case k_value.ValueKind
            Case JsonValueKind.True
                Return True
            Case JsonValueKind.Number
                Return k_value.GetInt32() <> 0
            Case Else
                Return False
        End Select
    End Function

    Public Async Function GetAdechitContextAsync(ct As CancellationToken) As Task(Of AdechitContext) Implements IAdechitApi.GetAdechitContextAsync
        Try
            Using k_doc As JsonDocument = Await SendAdechitJsonAsync("/api/adechit/context", Nothing, "citirea contextului ADECHIT", ct).ConfigureAwait(False)
                Dim k_result As New AdechitContext()
                Dim k_list As JsonElement
                If k_doc.RootElement.TryGetProperty("subunits", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        k_result.Subunits.Add(New AdechitSubunit() With {.Id = JsonInt(k_item, "id"), .Name = JsonText(k_item, "name")})
                    Next
                End If
                If k_doc.RootElement.TryGetProperty("permissions", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        If k_item.ValueKind = JsonValueKind.String Then k_result.Permissions.Add(k_item.GetString())
                    Next
                End If
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAdechitContextAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetAdechitYearsAsync(k_subunitId As Integer, ct As CancellationToken) As Task(Of List(Of Integer)) Implements IAdechitApi.GetAdechitYearsAsync
        Try
            Using k_doc As JsonDocument = Await SendAdechitJsonAsync("/api/adechit/years", k_subunitId, "citirea anilor ADECHIT", ct).ConfigureAwait(False)
                Dim k_result As New List(Of Integer)()
                Dim k_list As JsonElement
                If k_doc.RootElement.TryGetProperty("years", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        If k_item.ValueKind = JsonValueKind.Number Then k_result.Add(k_item.GetInt32())
                    Next
                End If
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAdechitYearsAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetAdechitMonthsAsync(k_subunitId As Integer, k_year As Integer, ct As CancellationToken) _
        As Task(Of List(Of AdechitMonth)) Implements IAdechitApi.GetAdechitMonthsAsync
        Try
            Dim k_path As String = "/api/adechit/rows/LunaD?Anul=" & k_year.ToString(CultureInfo.InvariantCulture)
            Using k_doc As JsonDocument = Await SendAdechitJsonAsync(k_path, k_subunitId, "citirea lunilor ADECHIT", ct).ConfigureAwait(False)
                Dim k_result As New List(Of AdechitMonth)()
                Dim k_list As JsonElement
                If k_doc.RootElement.TryGetProperty("rows", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        k_result.Add(New AdechitMonth() With {
                            .Id = JsonInt(k_item, "IDL"), .Year = JsonInt(k_item, "Anul"), .MonthNumber = JsonInt(k_item, "Luna"),
                            .MonthName = JsonText(k_item, "LunaT"), .Closed = JsonFlag(k_item, "Inchisa")})
                    Next
                End If
                k_result.Sort(Function(k_a, k_b) k_b.MonthNumber.CompareTo(k_a.MonthNumber))
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAdechitMonthsAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GetAdechitGroupsAsync(k_subunitId As Integer, k_monthId As Integer, ct As CancellationToken) _
        As Task(Of List(Of AdechitGroup)) Implements IAdechitApi.GetAdechitGroupsAsync
        Try
            Dim k_path As String = "/api/adechit/catalog-data?include_hidden=1&IDL=" & k_monthId.ToString(CultureInfo.InvariantCulture)
            Using k_doc As JsonDocument = Await SendAdechitJsonAsync(k_path, k_subunitId, "citirea grupelor ADECHIT", ct).ConfigureAwait(False)
                Dim k_result As New List(Of AdechitGroup)()
                Dim k_list As JsonElement
                If k_doc.RootElement.TryGetProperty("groups", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        k_result.Add(New AdechitGroup() With {
                            .Id = JsonInt(k_item, "IDG"), .Name = JsonText(k_item, "Grupa"),
                            .Educators = JsonText(k_item, "Educators"), .Hidden = JsonFlag(k_item, "Ascunsa")})
                    Next
                End If
                k_result.Sort(Function(k_a, k_b) String.Compare(k_a.Name, k_b.Name, StringComparison.CurrentCultureIgnoreCase))
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GetAdechitGroupsAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function SearchAdechitChildrenAsync(k_subunitId As Integer, k_monthId As Integer, k_query As String, ct As CancellationToken) _
        As Task(Of AdechitChildSearch) Implements IAdechitApi.SearchAdechitChildrenAsync
        Try
            Dim k_path As String = "/api/adechit/child-search?IDL=" & k_monthId.ToString(CultureInfo.InvariantCulture) &
                                   "&q=" & Uri.EscapeDataString(If(k_query, String.Empty))
            Using k_doc As JsonDocument = Await SendAdechitJsonAsync(k_path, k_subunitId, "căutarea copilului în ADECHIT", ct).ConfigureAwait(False)
                Dim k_result As New AdechitChildSearch()
                Dim k_list As JsonElement
                If k_doc.RootElement.TryGetProperty("rows", k_list) AndAlso k_list.ValueKind = JsonValueKind.Array Then
                    For Each k_item As JsonElement In k_list.EnumerateArray()
                        k_result.Rows.Add(New AdechitChild() With {
                            .ChildId = JsonInt(k_item, "IDP"), .Name = JsonText(k_item, "Nume"),
                            .GroupName = JsonText(k_item, "Grupa"), .CnpTail = JsonText(k_item, "CNP_ULTIME")})
                    Next
                End If
                k_result.HasMore = JsonFlag(k_doc.RootElement, "has_more")
                Return k_result
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.SearchAdechitChildrenAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function GenerateAdechitReportAsync(k_subunitId As Integer, k_kind As String, k_parameters As IReadOnlyDictionary(Of String, String),
                                                     ct As CancellationToken) As Task(Of AdechitReportHandle) Implements IAdechitApi.GenerateAdechitReportAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_kind)
            Dim k_query As New StringBuilder()
            If k_parameters IsNot Nothing Then
                For Each k_pair As KeyValuePair(Of String, String) In k_parameters
                    k_query.Append(If(k_query.Length = 0, "?", "&")).Append(Uri.EscapeDataString(k_pair.Key)).Append("="c).Append(Uri.EscapeDataString(k_pair.Value))
                Next
            End If
            Dim k_path As String = "/api/adechit/reports/" & Uri.EscapeDataString(k_kind) & "/generate" & k_query.ToString()
            Using k_doc As JsonDocument = Await SendAdechitJsonAsync(k_path, k_subunitId, "pregătirea raportului ADECHIT", ct).ConfigureAwait(False)
                Return New AdechitReportHandle() With {
                    .Snapshot = JsonText(k_doc.RootElement, "snapshot"), .Title = JsonText(k_doc.RootElement, "title"),
                    .FileName = JsonText(k_doc.RootElement, "filename")}
            End Using
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.GenerateAdechitReportAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function DownloadAdechitReportAsync(k_subunitId As Integer, k_snapshot As String, k_format As String, ct As CancellationToken) _
        As Task(Of Byte()) Implements IAdechitApi.DownloadAdechitReportAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_snapshot)
            If k_format <> "pdf" AndAlso k_format <> "xlsx" Then Throw New ArgumentException($"Unknown report format '{k_format}'.", NameOf(k_format))
            Dim k_path As String = "/api/adechit/reports/export/" & Uri.EscapeDataString(k_snapshot) & "/" & k_format
            Return Await SendAdechitAsync(HttpMethod.Get, k_path, k_subunitId, "descărcarea raportului ADECHIT", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.DownloadAdechitReportAsync", ex)
            Throw
        End Try
    End Function

    Public Async Function ReleaseAdechitReportAsync(k_subunitId As Integer, k_snapshot As String, ct As CancellationToken) As Task Implements IAdechitApi.ReleaseAdechitReportAsync
        Try
            ArgumentException.ThrowIfNullOrWhiteSpace(k_snapshot)
            Await SendAdechitAsync(HttpMethod.Delete, "/api/adechit/reports/export/" & Uri.EscapeDataString(k_snapshot), k_subunitId,
                                   "eliberarea raportului ADECHIT", ct).ConfigureAwait(False)
        Catch ex As ApiException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("ApiClient.ReleaseAdechitReportAsync", ex)
            Throw
        End Try
    End Function

End Class
