Imports System.Globalization

' SLICE-VC10-01: the explicit omission of NULL-priced products applies to the loaded Materiale snapshots only.
Friend NotInheritable Class VrcOiNullPrices
    Private Sub New()
    End Sub

    Friend Shared Function Prepare(k_data As VrcOiData, k_ignored As List(Of VrcOiIgnoredPrice)) As VrcOiData
        Dim k_copy As New VrcOiData With {.Source = k_data.Source, .Tables = k_data.Tables.ToDictionary(Function(k_t) k_t.Key, Function(k_t) k_t.Value.ToList(), StringComparer.OrdinalIgnoreCase)}
        Dim k_nulls = k_data.Rows("ProduseP").Where(Function(k_r) IsNull(k_r, "PUTVA")).Select(Function(k_r) VrcOiData.Key(k_r, "IdProdus")).ToHashSet(StringComparer.Ordinal)
        For Each k_variant In k_data.Rows("ProduseP").Where(Function(k_r) k_nulls.Contains(VrcOiData.Key(k_r, "IdProdus")))
            k_ignored.Add(Detail(k_data, k_variant, "Nomenclator: varianta și toate mișcările sale sunt omise", Nothing))
        Next
        For Each k_table In k_copy.Tables.Keys.ToList()
            If k_table = "ProduseP" Then
                k_copy.Tables(k_table).RemoveAll(Function(k_r) k_nulls.Contains(VrcOiData.Key(k_r, "IdProdus")))
                Continue For
            End If
            For Each k_row In k_copy.Rows(k_table).ToList()
                If Not k_row.ContainsKey("IdProdus") Then Continue For
                Dim k_fromVariant = k_nulls.Contains(VrcOiData.Key(k_row, "IdProdus"))
                Dim k_nullLine = k_row.ContainsKey("PUTVA") AndAlso IsNull(k_row, "PUTVA")
                If Not k_fromVariant AndAlso Not k_nullLine Then Continue For
                k_copy.Tables(k_table).Remove(k_row)
                k_ignored.Add(Detail(k_data, k_row, Document(k_data, k_table, k_row), VrcOiData.Value(k_row, "Cant")))
            Next
        Next
        Dim k_removedProducts = k_data.Rows("ProduseP").Where(Function(k_r) k_nulls.Contains(VrcOiData.Key(k_r, "IdProdus"))).Select(Function(k_r) VrcOiData.Key(k_r, "ID")).ToHashSet(StringComparer.Ordinal)
        Dim k_keptProducts = k_copy.Rows("ProduseP").Select(Function(k_r) VrcOiData.Key(k_r, "ID")).ToHashSet(StringComparer.Ordinal)
        k_copy.Tables("Produse").RemoveAll(Function(k_r) k_removedProducts.Contains(VrcOiData.Key(k_r, "ID")) AndAlso Not k_keptProducts.Contains(VrcOiData.Key(k_r, "ID")))
        RemoveEmptyDocuments(k_data, k_copy, "NIR", "Intrari", "IdIntrare", {"IntrariStorno"})
        RemoveEmptyDocuments(k_data, k_copy, "Iesiri_H", "Iesiri", "IdBonConsum", {"IesiriRetur", "LegBCLA"})
        Return k_copy
    End Function

    Private Shared Function IsNull(k_row As Dictionary(Of String, Object), k_column As String) As Boolean
        Dim k_value = VrcOiData.Value(k_row, k_column)
        Return k_value Is Nothing OrElse Convert.IsDBNull(k_value)
    End Function

    Private Shared Sub RemoveEmptyDocuments(k_original As VrcOiData, k_copy As VrcOiData, k_header As String, k_lines As String, k_key As String, k_dependents As String())
        Dim k_hadLines = k_original.Rows(k_lines).Select(Function(k_r) VrcOiData.Key(k_r, k_key)).ToHashSet(StringComparer.Ordinal)
        Dim k_hasLines = k_copy.Rows(k_lines).Select(Function(k_r) VrcOiData.Key(k_r, k_key)).ToHashSet(StringComparer.Ordinal)
        Dim k_removed = k_hadLines.Except(k_hasLines, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal)
        k_copy.Tables(k_header).RemoveAll(Function(k_r) k_removed.Contains(VrcOiData.Key(k_r, k_key)))
        For Each k_table In k_dependents
            If k_copy.Tables.ContainsKey(k_table) Then k_copy.Tables(k_table).RemoveAll(Function(k_r) k_removed.Contains(VrcOiData.Key(k_r, k_key)))
        Next
    End Sub

    Private Shared Function Detail(k_data As VrcOiData, k_row As Dictionary(Of String, Object), k_document As String, k_quantity As Object) As VrcOiIgnoredPrice
        Dim k_variant = k_data.Rows("ProduseP").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdProdus") = VrcOiData.Key(k_row, "IdProdus"))
        Dim k_product = If(k_variant Is Nothing, Nothing, k_data.Rows("Produse").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "ID") = VrcOiData.Key(k_variant, "ID")))
        Dim k_account = If(k_product Is Nothing, Nothing, k_data.Rows("Masini").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdMasina") = VrcOiData.Key(k_product, "IdMasina")))
        Dim k_header As Dictionary(Of String, Object) = Nothing
        Dim k_isNir = k_row.ContainsKey("IdIntrare")
        If k_row.ContainsKey("IdBonConsum") OrElse k_isNir Then
            Dim k_key = If(k_isNir, "IdIntrare", "IdBonConsum")
            k_header = k_data.Rows(If(k_isNir, "NIR", "Iesiri_H")).FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, k_key) = VrcOiData.Key(k_row, k_key))
        End If
        Dim k_day = If(k_header Is Nothing, Nothing, VrcOiData.Value(k_header, If(k_isNir, "Data", "DataBC")))
        Dim k_docNumber As Decimal? = Nothing
        If k_header IsNot Nothing Then k_docNumber = If(k_isNir, VrcConvert.ToDecimal(VrcOiData.Value(k_header, "Numar")), VrcOiHolders.DocumentNumber(k_header))
        Return New VrcOiIgnoredPrice With {.Source = k_data.Source.Source.Path, .Year = k_data.Source.Year, .Product = If(k_product Is Nothing, "Produs neidentificabil", VrcOiData.Text(k_product, "DenumireP")),
            .Unit = If(k_product Is Nothing, String.Empty, VrcOiData.Text(k_product, "UM")), .Account = If(k_account Is Nothing, String.Empty, VrcOiData.Text(k_account, "Masina")), .Document = k_document, .Quantity = k_quantity,
            .Day = If(TypeOf k_day Is Date, CType(CDate(k_day), Date?), Nothing), .DocumentNumber = k_docNumber}
    End Function

    Private Shared Function Document(k_data As VrcOiData, k_table As String, k_row As Dictionary(Of String, Object)) As String
        If k_row.ContainsKey("IdBonConsum") Then
            Dim k_header = k_data.Rows("Iesiri_H").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdBonConsum") = VrcOiData.Key(k_row, "IdBonConsum"))
            If k_header IsNot Nothing Then Return VrcOiHolders.DocumentLabel(k_header)
        End If
        If k_row.ContainsKey("IdIntrare") Then
            Dim k_header = k_data.Rows("NIR").FirstOrDefault(Function(k_r) VrcOiData.Key(k_r, "IdIntrare") = VrcOiData.Key(k_row, "IdIntrare"))
            If k_header IsNot Nothing Then
                Dim k_day = VrcOiData.Value(k_header, "Data")
                Return $"NIR {VrcOiData.Text(k_header, "Numar")}, {If(TypeOf k_day Is Date, CDate(k_day).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "dată lipsă")}, factura {VrcOiData.Text(k_header, "NrFact")}"
            End If
        End If
        Return "Mișcare omisă: " & k_table
    End Function
End Class

Public NotInheritable Class VrcOiIgnoredPrice
    Public Property Source As String
    Public Property Year As Integer
    Public Property Account As String
    Public Property Product As String
    Public Property Unit As String
    Public Property Document As String
    Public Property Quantity As Object
    Public Property Day As Date?
    Public Property DocumentNumber As Decimal?
End Class
