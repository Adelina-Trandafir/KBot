Imports System.Globalization
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports KBot.Common

''' <summary>What ANAF knows about one tax code: the name and the flat seat address.</summary>
Public NotInheritable Class VrcAnafEntry

    Public Property TaxCode As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Address As String = String.Empty

End Class

''' <summary>
''' The ANAF lookup of the missing suppliers (same call as PYTHON/routes/inregistrare/anaf.py: POST, v9, body
''' [{"cui": n, "data": "yyyy-mm-dd"}], answer with «found» / «notFound»). The v9 service takes up to 100 codes per request, so the
''' codes go in chunks, one second apart. Nothing is written anywhere.
''' </summary>
Public NotInheritable Class VrcAnaf

    Private Const ServiceUrl As String = "https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva"
    Private Const ChunkSize As Integer = 100
    Private Shared ReadOnly Http As New HttpClient() With {.Timeout = TimeSpan.FromSeconds(30)}

    Private Sub New()
    End Sub

    ''' <summary>True for a digits-only code of a plausible length (2 to 10 digits): the only shape ANAF can be asked about.</summary>
    Public Shared Function IsAskable(k_code As String) As Boolean
        If k_code Is Nothing OrElse k_code.Length < 2 OrElse k_code.Length > 10 Then Return False
        Return k_code.All(Function(k_c) k_c >= "0"c AndAlso k_c <= "9"c)
    End Function

    ''' <summary>The codes ANAF knows, by code. A code missing from the result is unknown to ANAF. Throws when ANAF cannot be reached or answers with an error page.</summary>
    Public Shared Function Lookup(k_codes As IEnumerable(Of String), k_cancel As CancellationToken) As Dictionary(Of String, VrcAnafEntry)
        Try
            Dim k_result As New Dictionary(Of String, VrcAnafEntry)(StringComparer.Ordinal)
            Dim k_list = k_codes.Where(Function(k_c) IsAskable(k_c)).Distinct(StringComparer.Ordinal).ToList()
            For k_start As Integer = 0 To k_list.Count - 1 Step ChunkSize
                k_cancel.ThrowIfCancellationRequested()
                If k_start > 0 Then Thread.Sleep(1100)
                AskChunk(k_list.Skip(k_start).Take(ChunkSize).ToList(), k_result)
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("VrcAnaf.Lookup", ex)
            Throw
        End Try
    End Function

    Private Shared Sub AskChunk(k_chunk As List(Of String), k_result As Dictionary(Of String, VrcAnafEntry))
        Dim k_day As String = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        Dim k_body As New StringBuilder("[")
        For k_i As Integer = 0 To k_chunk.Count - 1
            If k_i > 0 Then k_body.Append(","c)
            k_body.Append("{""cui"":").Append(k_chunk(k_i)).Append(",""data"":""").Append(k_day).Append("""}")
        Next
        k_body.Append("]"c)
        Using k_request As New HttpRequestMessage(HttpMethod.Post, ServiceUrl)
            k_request.Content = New StringContent(k_body.ToString(), Encoding.UTF8, "application/json")
            k_request.Headers.UserAgent.ParseAdd("K-BOT")
            Using k_response = Http.Send(k_request)
                k_response.EnsureSuccessStatusCode()
                Dim k_text As String
                Using k_reader As New IO.StreamReader(k_response.Content.ReadAsStream(), Encoding.UTF8)
                    k_text = k_reader.ReadToEnd()
                End Using
                If k_text.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException("ANAF a răspuns cu o pagină de eroare; încercați mai târziu.")
                End If
                Using k_doc = JsonDocument.Parse(k_text)
                    Dim k_found As JsonElement
                    If k_doc.RootElement.ValueKind <> JsonValueKind.Object OrElse Not k_doc.RootElement.TryGetProperty("found", k_found) OrElse k_found.ValueKind <> JsonValueKind.Array Then Return
                    For Each k_item In k_found.EnumerateArray()
                        Dim k_general As JsonElement
                        If Not k_item.TryGetProperty("date_generale", k_general) Then Continue For
                        Dim k_entry As New VrcAnafEntry() With {
                            .TaxCode = TextOf(k_general, "cui"), .Name = TextOf(k_general, "denumire"), .Address = TextOf(k_general, "adresa")}
                        If k_entry.TaxCode.Length > 0 AndAlso k_entry.Name.Length > 0 Then k_result(k_entry.TaxCode) = k_entry
                    Next
                End Using
            End Using
        End Using
    End Sub

    Private Shared Function TextOf(k_element As JsonElement, k_name As String) As String
        Dim k_value As JsonElement
        If Not k_element.TryGetProperty(k_name, k_value) Then Return String.Empty
        Select Case k_value.ValueKind
            Case JsonValueKind.String : Return k_value.GetString().Trim()
            Case JsonValueKind.Number : Return k_value.GetRawText()
            Case Else : Return String.Empty
        End Select
    End Function

End Class
