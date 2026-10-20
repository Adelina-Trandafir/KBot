''' <summary>Only storno lines attached to an actual source NIR with Storno checked belong to the import.</summary>
Partial Friend NotInheritable Class VrcRunner

    Private ReadOnly _stornoParents As New Dictionary(Of Integer, HashSet(Of String))()

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
