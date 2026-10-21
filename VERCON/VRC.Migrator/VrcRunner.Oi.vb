''' <summary>OI/MF current-document and provenance references, sharing the document identity allocator.</summary>
Partial Friend NotInheritable Class VrcRunner

    Private ReadOnly _oiDocumentLabels As New Dictionary(Of Integer, Dictionary(Of String, HashSet(Of String)))()

    Private Function OiDocumentLabels(k_source As VrcPlanSource, k_sourceIndex As Integer) As Dictionary(Of String, HashSet(Of String))
        Dim k_labels As Dictionary(Of String, HashSet(Of String)) = Nothing
        If Not _oiDocumentLabels.TryGetValue(k_sourceIndex, k_labels) Then
            k_labels = New Dictionary(Of String, HashSet(Of String))(StringComparer.Ordinal) From {
                {"IDBP", OriginLabels(k_source, "OI_Predare", "IDBP")},
                {"IDBT", OriginLabels(k_source, "OI_Transfer", "IDBT")},
                {"IDBC", OriginLabels(k_source, "OI_Casare", "IDBC")}}
            _oiDocumentLabels(k_sourceIndex) = k_labels
        End If
        Return k_labels
    End Function

    Private Shared Function OiLabel(k_raw As Object) As String
        Dim k_number As Decimal? = VrcConvert.ToDecimal(k_raw)
        If Not k_number.HasValue OrElse k_number.Value = 0D Then Return String.Empty
        Return VrcConvert.KeyText(k_raw)
    End Function

    ''' <summary>
    ''' Registry IDBC names the current document of any family. IDBP/IDBT identify its family;
    ''' with neither marker it is a write-off. Older VBA left IDBC=0 for BP/BT: fill it from the marker.
    ''' </summary>
    Private Function ConvertOiDocument(k_gest As GestInfo, k_source As VrcPlanSource, k_sourceIndex As Integer,
                                       k_table As VrcImportTable, k_row As Dictionary(Of String, Object),
                                       k_key As String, ByRef k_ok As Boolean) As Object
        Dim k_bp As String = OiLabel(k_row("IDBP"))
        Dim k_bt As String = OiLabel(k_row("IDBT"))
        Dim k_current As String = OiLabel(k_row("IDBC"))
        Dim k_origin As String = OiLabel(k_row("IDBO"))
        If k_bp.Length > 0 AndAlso k_bt.Length > 0 Then
            AddIssue(k_source, k_table.Name, k_key, "IDBP/IDBT", "X15", "BLOCAJ", k_bp & "/" & k_bt, String.Empty,
                     "Poziția are completate și numărul bonului de predare, și numărul bonului de transfer; trebuie să fie doar unul dintre ele.")
            k_ok = False
            Return Nothing
        End If

        Dim k_group As String = If(k_bp.Length > 0, "IDBP", If(k_bt.Length > 0, "IDBT", "IDBC"))
        Dim k_label As String = If(k_group = "IDBP", k_bp, If(k_group = "IDBT", k_bt, k_current))
        If k_label.Length = 0 Then
            AddIssue(k_source, k_table.Name, k_key, "IDBC", "X15", "BLOCAJ", "NULL/0", String.Empty,
                     "Poziția nu are niciun bon completat (nici de predare, nici de transfer, nici de casare).")
            k_ok = False
            Return Nothing
        End If
        If k_group <> "IDBC" AndAlso k_current.Length > 0 AndAlso k_current <> k_label Then
            AddIssue(k_source, k_table.Name, k_key, "IDBC", "X15", "BLOCAJ", k_current, String.Empty,
                     $"Poziția are bonul curent {k_current}, dar și bonul de {If(k_group = "IDBP", "predare", "transfer")} {k_label}; cele două numere trebuie să fie același.")
            k_ok = False
            Return Nothing
        End If
        If Not OiDocumentLabels(k_source, k_sourceIndex)(k_group).Contains(k_label) Then
            AddIssue(k_source, k_table.Name, k_key, "IDBC", "X15", "BLOCAJ", k_label, String.Empty,
                     $"Bonul de {If(k_group = "IDBP", "predare", If(k_group = "IDBT", "transfer", "casare"))} nr. {k_label}, scris pe această poziție, nu există în tabelul bonurilor ({If(k_group = "IDBP", "OI_Predare", If(k_group = "IDBT", "OI_Transfer", "OI_Casare"))}).")
            k_ok = False
            Return Nothing
        End If

        If k_group = "IDBP" AndAlso k_origin.Length > 0 Then
            AddIssue(k_source, k_table.Name, k_key, "IDBO", "X15", "BLOCAJ", k_origin, String.Empty,
                     "O poziție de predare nu trebuie să aibă bon de origine; acesta se completează doar la transfer sau casare.")
            k_ok = False
            Return Nothing
        End If
        Dim k_quantity As Decimal? = VrcConvert.ToDecimal(k_row("Cant"))
        If k_group <> "IDBP" AndAlso k_origin.Length = 0 AndAlso k_quantity.GetValueOrDefault() <> 0D Then
            AddIssue(k_source, k_table.Name, k_key, "IDBO", "X15", "BLOCAJ", "NULL/0", String.Empty,
                     "Poziția este un transfer sau o casare cu cantitate, dar nu are completat bonul din care a plecat obiectul (bonul de origine).")
            k_ok = False
            Return Nothing
        End If

        Dim k_column As New VrcImportColumn() With {.Name = "IDBC", .Link = VrcLinkKind.Group, .GroupName = k_group}
        Return GroupNumber(k_gest, k_source, k_sourceIndex, k_column, k_label)
    End Function

End Class
