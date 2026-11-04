Imports System.Globalization

' SLICE-VC10-01: split only receipts touched by the account choices; line economics stay unchanged.
Friend NotInheritable Class VrcOiReceiptSplit
    Friend Const OriginalKey As String = "__OiOriginalReceipt"
    Private Shared ReadOnly Names As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)

    Private Sub New()
    End Sub

    Friend Shared Sub Prepare(k_data As VrcOiData, k_choices As List(Of VrcOiChoice), k_result As VrcOiConversion)
        If k_choices.Count = 0 Then Return
        Dim k_accounts = k_choices.Select(Function(k_c) k_c.AccountKey).ToHashSet(StringComparer.Ordinal)
        Dim k_headers = k_data.Rows("NIR")
        Dim k_storno = k_headers.Where(Function(k_r) VrcConvert.ToBit(VrcOiData.Value(k_r, "Storno")).GetValueOrDefault() <> 0).Select(Function(k_r) VrcOiData.Key(k_r, "IdIntrare")).ToHashSet(StringComparer.Ordinal)
        Dim k_next = k_headers.Select(Function(k_r) CLng(VrcOiData.Value(k_r, "IdIntrare"))).DefaultIfEmpty(0L).Max()
        Dim k_maps As New Dictionary(Of String, Dictionary(Of String, Long))(StringComparer.Ordinal)
        For Each k_header In k_headers.ToList()
            Dim k_old = VrcOiData.Key(k_header, "IdIntrare")
            Dim k_lines = k_data.Rows("Intrari").Where(Function(k_r) VrcOiData.Key(k_r, "IdIntrare") = k_old).ToList()
            Dim k_touched = k_accounts.Contains(VrcOiData.Key(k_header, "IdMasina")) OrElse k_lines.Any(Function(k_r) k_accounts.Contains(Account(k_data, k_r)))
            If Not k_touched OrElse k_lines.Count = 0 Then Continue For
            Dim k_label = Label(k_data, k_header)
            Try
                If k_lines.Any(Function(k_r) Not k_data.Rows("Masini").Any(Function(k_a) VrcOiData.Key(k_a, "IdMasina") = Account(k_data, k_r) AndAlso Account(k_data, k_r) <> "0")) Then
                    k_result.Blockers.Add(k_label & ": există o linie al cărei produs sau cont nu poate fi identificat. Împărțirea automată cere conturi identificabile pentru toate liniile.")
                    Continue For
                End If
                Dim k_groups = k_lines.GroupBy(Function(k_r) Account(k_data, k_r)).OrderBy(Function(k_g) AccountName(k_data, k_g.Key), Names).ToList()
                If k_groups.Count = 1 AndAlso k_groups(0).Key = VrcOiData.Key(k_header, "IdMasina") Then Continue For
                Dim k_newHeaders As New List(Of Dictionary(Of String, Object))()
                Dim k_newLines As New List(Of Dictionary(Of String, Object))()
                Dim k_fragments As New List(Of VrcOiReceiptFragment)()
                Dim k_map As New Dictionary(Of String, Long)(StringComparer.Ordinal)
                For Each k_group In k_groups
                    Dim k_id As Long
                    If k_newHeaders.Count = 0 Then
                        k_id = CLng(VrcOiData.Value(k_header, "IdIntrare"))
                    Else
                        k_next += 1
                        k_id = k_next
                    End If
                    k_map(k_group.Key) = k_id
                    Dim k_copy = VrcOiData.CloneRow(k_header)
                    k_copy("IdIntrare") = k_id
                    k_copy("IdMasina") = k_group.Key
                    k_copy(OriginalKey) = k_old
                    k_copy("Valoare") = k_group.Sum(Function(k_r) VrcOiData.Number(k_r, "ValoareCUTVA"))
                    k_newHeaders.Add(k_copy)
                    Dim k_choice = k_choices.FirstOrDefault(Function(k_c) k_c.AccountKey = k_group.Key)
                    Dim k_part As New VrcOiReceiptFragment With {.Source = k_data.Source.Source.Path, .OriginalKey = k_old, .Number = VrcOiData.Text(k_header, "Numar"),
                        .Day = If(TypeOf VrcOiData.Value(k_header, "Data") Is Date, CDate(VrcOiData.Value(k_header, "Data")), Date.MinValue),
                        .Invoice = VrcOiData.Text(k_header, "NrFact"), .Account = AccountName(k_data, k_group.Key),
                        .Destination = If(k_choice Is Nothing, k_data.Source.GestiuneName & " (Materiale rămas)", If(k_choice.TargetType = VrcOiChoice.ExcludeType, VrcOiChoice.ExcludeType, k_choice.TargetName & " (" & k_choice.TargetType & ")")),
                        .OriginalValue = VrcOiData.Value(k_header, "Valoare"), .Value = VrcOiData.Value(k_copy, "Valoare"), .Lines = k_group.ToList()}
                    k_fragments.Add(k_part)
                    For Each k_line In k_group
                        Dim k_newLine = VrcOiData.CloneRow(k_line)
                        k_newLine("IdIntrare") = k_id
                        k_newLines.Add(k_newLine)
                    Next
                Next
                k_headers.Remove(k_header)
                k_headers.AddRange(k_newHeaders)
                k_data.Tables("Intrari").RemoveAll(Function(k_r) VrcOiData.Key(k_r, "IdIntrare") = k_old)
                k_data.Tables("Intrari").AddRange(k_newLines)
                k_maps(k_old) = k_map
                k_result.ReceiptFragments.AddRange(k_fragments)
            Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
                k_result.Blockers.Add(k_label & ": împărțirea nu poate calcula valorile liniilor: " & ex.Message)
            End Try
        Next
        For Each k_row In k_data.Rows("IntrariStorno").ToList()
            If Not k_storno.Contains(VrcOiData.Key(k_row, "IdIntrare")) Then Continue For
            Dim k_map As Dictionary(Of String, Long) = Nothing
            If Not k_maps.TryGetValue(VrcOiData.Key(k_row, "IdIntrare"), k_map) Then Continue For
            Dim k_account = Account(k_data, k_row)
            If Not k_map.ContainsKey(k_account) Then
                k_result.Blockers.Add($"{k_data.Source.Profile.Label}: o corecție a unui NIR împărțit nu are un cont identificabil în documentul rezultat.")
                Continue For
            End If
            Dim k_copy = VrcOiData.CloneRow(k_row)
            k_copy("IdIntrare") = k_map(k_account)
            k_data.Tables("IntrariStorno")(k_data.Tables("IntrariStorno").IndexOf(k_row)) = k_copy
        Next
        For Each k_header In k_headers.ToList()
            Dim k_map As Dictionary(Of String, Long) = Nothing
            If Not k_maps.TryGetValue(VrcOiData.Key(k_header, "IdStorno"), k_map) Then Continue For
            Dim k_account = VrcOiData.Key(k_header, "IdMasina")
            If Not k_map.ContainsKey(k_account) Then
                k_result.Blockers.Add(Label(k_data, k_header) & ": NIR-ul stornat nu are o parte pentru contul acestei corecții.")
                Continue For
            End If
            Dim k_copy = VrcOiData.CloneRow(k_header)
            k_copy("IdStorno") = k_map(k_account)
            k_headers(k_headers.IndexOf(k_header)) = k_copy
        Next
    End Sub

    Private Shared Function Account(k_data As VrcOiData, k_line As Dictionary(Of String, Object)) As String
        Dim k_variant = k_data.Rows("ProduseP").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdProdus") = VrcOiData.Key(k_line, "IdProdus"))
        Dim k_product = If(k_variant Is Nothing, Nothing, k_data.Rows("Produse").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "ID") = VrcOiData.Key(k_variant, "ID")))
        Return If(k_product Is Nothing, String.Empty, VrcOiData.Key(k_product, "IdMasina"))
    End Function

    Private Shared Function AccountName(k_data As VrcOiData, k_key As String) As String
        Return VrcOiData.Text(k_data.Rows("Masini").First(Function(k_r) VrcOiData.Key(k_r, "IdMasina") = k_key), "Masina")
    End Function

    Private Shared Function Label(k_data As VrcOiData, k_header As Dictionary(Of String, Object)) As String
        Dim k_date = VrcOiData.Value(k_header, "Data")
        Return $"{k_data.Source.Profile.Label}, NIR «{VrcOiData.Text(k_header, "Numar")}», data {If(TypeOf k_date Is Date, CDate(k_date).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "lipsă")}, factura «{VrcOiData.Text(k_header, "NrFact")}»"
    End Function
End Class

Public NotInheritable Class VrcOiReceiptFragment
    Public Property Source As String
    Public Property OriginalKey As String
    Public Property Number As String
    Public Property Day As Date
    Public Property Invoice As String
    Public Property Account As String
    Public Property Destination As String
    Public Property OriginalValue As Object
    Public Property Value As Object
    Public Property Lines As New List(Of Dictionary(Of String, Object))()
End Class
