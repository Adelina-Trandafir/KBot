Imports System.Globalization

''' <summary>Storno parents and recovery of storno/return prices rounded by legacy Access Long columns.</summary>
Partial Friend NotInheritable Class VrcRunner

    Private ReadOnly _stornoParents As New Dictionary(Of Integer, HashSet(Of String))()
    Private ReadOnly _correctionPrices As New Dictionary(Of Integer, Dictionary(Of String, Decimal?))()

    ''' <summary>
    ''' Access FCS/BR and their reports use ProduseP.PUTVA, but both correction line tables store Long.
    ''' Recover only a matching integer rounding, from the SAME source and partition.
    ''' The returned copy goes through the normal decimal conversion and reconciliation.
    ''' </summary>
    Private Function PrepareCorrectionPriceRow(k_source As VrcPlanSource, k_table As VrcImportTable,
                                               k_row As Dictionary(Of String, Object), k_key As String) As Dictionary(Of String, Object)
        Dim k_raw As Object = Nothing
        k_row.TryGetValue("PUTVA", k_raw)
        If Not TypeOf k_raw Is Integer Then Return k_row

        Dim k_sourceIndex = _plan.Sources.IndexOf(k_source)
        Dim k_prices As Dictionary(Of String, Decimal?) = Nothing
        If Not _correctionPrices.TryGetValue(k_sourceIndex, k_prices) Then
            k_prices = New Dictionary(Of String, Decimal?)(StringComparer.Ordinal)
            For Each k_product In ReadRows(k_source, "ProduseP")
                k_prices(VrcConvert.KeyText(k_product("IdProdus"))) = VrcConvert.ToDecimal(k_product("PUTVA"))
            Next
            _correctionPrices(k_sourceIndex) = k_prices
        End If

        Dim k_productKey = VrcConvert.KeyText(k_row("IdProdus"))
        Dim k_price As Decimal? = Nothing
        If Not k_prices.TryGetValue(k_productKey, k_price) OrElse Not k_price.HasValue Then Return k_row
        Dim k_original = CDec(k_raw)
        If k_original = k_price.Value Then Return k_row
        If k_original <> Math.Round(k_price.Value, 0, MidpointRounding.ToEven) Then
            AddIssue(k_source, k_table.Name, k_key, "PUTVA", "N21", "AVERTIZARE",
                     k_original.ToString(CultureInfo.InvariantCulture), k_original.ToString(CultureInfo.InvariantCulture),
                     $"IdProdus={k_productKey}: prețul întreg din {k_table.Name} nu corespunde rotunjirii prețului ProduseP ({k_price.Value.ToString(CultureInfo.InvariantCulture)}). Se păstrează valoarea sursă; diferența trebuie lămurită din documente.")
            Return k_row
        End If

        Dim k_copy As New Dictionary(Of String, Object)(k_row, StringComparer.OrdinalIgnoreCase)
        k_copy("PUTVA") = k_price.Value
        AddIssue(k_source, k_table.Name, k_key, "PUTVA", "N20", "CONVERSIE",
                 k_original.ToString(CultureInfo.InvariantCulture), k_price.Value.ToString(CultureInfo.InvariantCulture),
                 $"IdProdus={k_productKey}: zecimalele prețului cu TVA au fost recuperate din ProduseP din aceeași sursă/gestiune, ca în formularul și raportul Access. Coloana {k_table.Name}.PUTVA din Access este Long și a rotunjit prețul la întreg. Fișierul Access rămâne nemodificat.")
        Return k_copy
    End Function

    Private Function HasStornoParent(k_source As VrcPlanSource, k_row As Dictionary(Of String, Object)) As Boolean
        Dim k_sourceIndex = _plan.Sources.IndexOf(k_source)
        Dim k_parents As HashSet(Of String) = Nothing
        If Not _stornoParents.TryGetValue(k_sourceIndex, k_parents) Then
            k_parents = New HashSet(Of String)(StringComparer.Ordinal)
            For Each k_nir In ReadRows(k_source, "NIR")
                If VrcConvert.ToBit(k_nir("Storno")).GetValueOrDefault() = 0 Then Continue For
                Dim k_id = VrcConvert.ToDecimal(k_nir("IdIntrare"))
                If k_id.HasValue AndAlso k_id.Value <> 0D Then k_parents.Add(VrcConvert.KeyText(k_nir("IdIntrare")))
            Next
            _stornoParents(k_sourceIndex) = k_parents
        End If
        Return k_parents.Contains(VrcConvert.KeyText(k_row("IdIntrare")))
    End Function

    Private Sub ReportSkippedStorno(k_source As VrcPlanSource, k_row As Dictionary(Of String, Object), k_key As String)
        Dim k_parent = VrcConvert.KeyText(k_row("IdIntrare"))
        Dim k_product = VrcConvert.KeyText(k_row("IdProdus"))
        AddIssue(k_source, "IntrariStorno", k_key, "IdIntrare", "N15", "AVERTIZARE",
                 $"IdIntrare={If(k_parent.Length = 0, "NULL", k_parent)}; IdProdus={If(k_product.Length = 0, "NULL", k_product)}",
                 "IGNORAT",
                 "Linia nu are un NIR părinte cu Storno bifat în aceeași sursă/gestiune. Este ignorată înaintea verificării produsului, fără blocarea importului; nu se scrie în țintă și nu afectează stocul.")
    End Sub

End Class
