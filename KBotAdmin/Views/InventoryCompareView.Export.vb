Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json

Partial Public Class InventoryCompareView
    Private Sub Export_Click(k_sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            If _results.Count = 0 OrElse _reportContext = String.Empty Then Return
            Using k_dialog As New SaveFileDialog With {.Filter = "CSV (*.csv)|*.csv", .FileName = "comparatie-inventar.csv", .Title = "Salvează comparația inventarului"}
                If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                Dim k_csv As New StringBuilder()
                AppendCsv(k_csv, {"Context", "Rând Excel", "Denumire Excel", "Cod Excel", "UM Excel", "Cantitate scriptică Excel",
                    "Preț Excel", "Rezultat", "Observații", "Nr. candidați", "Ordine candidat", "Denumire VERCON", "Cod VERCON",
                    "UM VERCON", "Preț cu TVA VERCON", "VERCON minus Excel", "Scor nume din 100", "Scor total din 100",
                    "Explicație", "ID produs VERCON", "ID variantă VERCON"})
                For Each k_result In _results.OrderBy(Function(k_r) _source(k_r.GetProperty("row").GetInt32()).Name, _names)
                    Dim k_item = _source(k_result.GetProperty("row").GetInt32())
                    Dim k_prefix As String() = {_reportContext, k_item.Row.ToString(CultureInfo.InvariantCulture), k_item.Name, k_item.Code,
                        k_item.Um, k_item.Quantity, k_item.Price, k_result.GetProperty("status").GetString(),
                        k_result.GetProperty("note").GetString(), k_result.GetProperty("candidate_count").GetInt32().ToString(CultureInfo.InvariantCulture)}
                    Dim k_candidates = k_result.GetProperty("candidates")
                    If k_candidates.GetArrayLength() = 0 Then
                        AppendCsv(k_csv, k_prefix.Concat(Enumerable.Repeat(String.Empty, 11)))
                    Else
                        Dim k_rank = 0
                        For Each k_c In k_candidates.EnumerateArray()
                            k_rank += 1
                            Dim k_variant = k_c.GetProperty("variant_id").GetInt64()
                            AppendCsv(k_csv, k_prefix.Concat({k_rank.ToString(CultureInfo.InvariantCulture),
                                k_c.GetProperty("name").GetString(), k_c.GetProperty("code").GetString(), k_c.GetProperty("um").GetString(),
                                CsvNumber(k_c.GetProperty("price").GetString()), CsvNumber(k_c.GetProperty("price_delta").GetString()),
                                CsvNumber(k_c.GetProperty("name_score").GetRawText()), CsvNumber(k_c.GetProperty("score").GetRawText()),
                                k_c.GetProperty("reason").GetString(), k_c.GetProperty("product_id").GetInt64().ToString(CultureInfo.InvariantCulture),
                                If(k_variant > 0, k_variant.ToString(CultureInfo.InvariantCulture), String.Empty)}))
                        Next
                    End If
                Next
                File.WriteAllText(k_dialog.FileName, k_csv.ToString(), New UTF8Encoding(True))
                _context.Say("Comparația a fost salvată: " & k_dialog.FileName)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("InventoryCompareView.Export_Click", ex)
            KBotMessage.Show("Nu am putut salva comparația: " & ex.Message, "Inventar Excel", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function CsvNumber(k_text As String) As String
        Dim k_number = Numeric(k_text)
        Return If(k_number Is Nothing, String.Empty, DirectCast(k_number, Decimal).ToString("0.####", CultureInfo.GetCultureInfo("ro-RO")))
    End Function

    Private Shared Sub AppendCsv(k_output As StringBuilder, k_values As IEnumerable(Of String))
        k_output.AppendLine(String.Join(";", k_values.Select(Function(k_value) CsvCell(k_value))))
    End Sub

    Private Shared Function CsvCell(k_value As String) As String
        Dim k_text = If(k_value, String.Empty)
        Dim k_trim = k_text.TrimStart()
        ' Keep numbers numeric; protect other fields against spreadsheet formula interpretation.
        If k_trim.Length > 0 AndAlso "=+-@".Contains(k_trim(0)) AndAlso Numeric(k_trim) Is Nothing Then k_text = "'" & k_text
        Return """" & k_text.Replace("""", """""") & """"
    End Function
End Class

' Culture is scoped to this grid's messages, including its filter/sort popup. Other pages keep their culture.
Friend Class InventoryCompareGrid
    Inherits KBotDataView

    Protected Overrides Sub WndProc(ByRef m As Message)
        Dim k_previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ro-RO")
            MyBase.WndProc(m)
        Finally
            CultureInfo.CurrentCulture = k_previous
        End Try
    End Sub

    Public Sub FinishUpdate()
        Dim k_previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ro-RO")
            EndUpdate()
        Finally
            CultureInfo.CurrentCulture = k_previous
        End Try
    End Sub
End Class
