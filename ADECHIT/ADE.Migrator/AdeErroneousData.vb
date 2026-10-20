Imports System.Data
Imports System.IO
Imports System.Text.Json

''' <summary>Separates orphan financial documents before conversion and foreign-key validation.</summary>
Public NotInheritable Class AdeErroneousData

    Private Sub New()
    End Sub

    Public Shared Function Build(k_source As AdeSource, k_plan As AdePlan) As HashSet(Of DataRow)
        Dim k_reasons As New Dictionary(Of DataRow, String)()
        Dim k_children = KeysOf(k_source.Tables("Platitori"), "IDP")
        Dim k_payers = KeysOf(k_source.Tables("Platitori_sub"), "IDS")
        Dim k_payments As New Dictionary(Of Integer, DataRow)()
        Dim k_months As New Dictionary(Of Integer, Integer)()
        For Each k_row As DataRow In k_source.Tables("LunaD").Rows
            Dim k_id = IdOf(k_row, "IDL")
            Dim k_year = IdOf(k_row, "Anul")
            If k_id.HasValue AndAlso k_year.HasValue Then k_months(k_id.Value) = k_year.Value
        Next

        For Each k_name In {"Plati", "Retur"}
            For Each k_row As DataRow In k_source.Tables(k_name).Rows
                Dim k_missing As New List(Of String)()
                MissingLink(k_row, "IDP", "Platitori", k_children, k_missing)
                MissingLink(k_row, "IDS", "Platitori_sub", k_payers, k_missing)
                If k_missing.Count > 0 Then k_reasons(k_row) = String.Join("; ", k_missing)
                If k_name = "Plati" Then
                    Dim k_id = IdOf(k_row, "IDPL")
                    If k_id.HasValue Then k_payments.TryAdd(k_id.Value, k_row)
                End If
            Next
        Next

        ' Receipts and other documents inherit the missing payer through their payment.
        For Each k_name In {"Chitante", "AlteDoc"}
            For Each k_row As DataRow In k_source.Tables(k_name).Rows
                Dim k_id = IdOf(k_row, "IDPL")
                If Not k_id.HasValue Then Continue For ' NULL references retain the existing nullable-link policy.
                Dim k_payment As DataRow = Nothing
                If Not k_payments.TryGetValue(k_id.Value, k_payment) Then
                    k_reasons(k_row) = $"IDPL={k_id.Value}: Plati inexistent; corespondența cu Platitori/Platitori_sub nu poate fi găsită"
                ElseIf k_reasons.ContainsKey(k_payment) Then
                    k_reasons(k_row) = $"IDPL={k_id.Value}: plată exclusă ({k_reasons(k_payment)})"
                End If
            Next
        Next

        Dim k_archive As New AdePlanTable(AdeSchema.DateEronate, -1)
        For Each k_table In AdeSchema.Tables
            Dim k_count = 0
            Dim k_recent = 0
            Dim k_example As String = Nothing
            For Each k_row As DataRow In k_source.Tables(k_table.Name).Rows
                Dim k_reason As String = Nothing
                If Not k_reasons.TryGetValue(k_row, k_reason) Then Continue For
                Dim k_years = YearsOf(k_row, k_months)
                If k_years.Count = 0 AndAlso (k_table.Name = "Chitante" OrElse k_table.Name = "AlteDoc") Then
                    Dim k_id = IdOf(k_row, "IDPL")
                    Dim k_payment As DataRow = Nothing
                    If k_id.HasValue AndAlso k_payments.TryGetValue(k_id.Value, k_payment) Then k_years = YearsOf(k_payment, k_months)
                End If
                Dim k_blocking = k_years.Contains(2025) OrElse k_years.Contains(2026)
                Dim k_key = IdOf(k_row, k_table.Key)
                Dim k_raw As New Dictionary(Of String, Object)(StringComparer.Ordinal)
                For Each k_column As DataColumn In k_row.Table.Columns
                    k_raw(k_column.ColumnName) = If(k_row.IsNull(k_column), Nothing, k_row(k_column))
                Next
                k_archive.Rows.Add(New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                    {"IDE", Nothing}, {"TabelSursa", k_table.Name}, {"CheieSursa", k_key},
                    {"AniDetectati", If(k_years.Count = 0, Nothing, String.Join(",", k_years.OrderBy(Function(y) y)))},
                    {"Motiv", k_reason}, {"DateSursa", JsonSerializer.Serialize(k_raw)},
                    {"SourceHash", k_source.FileHash}, {"SourceFile", Path.GetFileName(k_source.FilePath)}
                })
                k_count += 1
                If k_blocking Then
                    k_recent += 1
                    If k_example Is Nothing Then k_example = $"{k_table.Key}={k_key}; ani: {String.Join(",", k_years.OrderBy(Function(y) y))}; {k_reason}"
                End If
            Next
            If k_count = 0 Then Continue For
            k_plan.Notes.Add($"{k_table.Name}: {k_count} rânduri fără corespondență Platitori/Platitori_sub excluse din transferul obișnuit și pregătite în AD_DateEronate (rânduri Access complete, în JSON).")
            If k_recent > 0 Then
                k_plan.Findings.Add(New AdeFinding(True, $"{k_table.Name}: {k_recent} rânduri eronate din 2025/2026 blochează migrarea (ex.: {k_example}). Corectați sursa; cât timp există blocaje nu se scrie nici AD_DateEronate."))
            End If
        Next
        If k_archive.Rows.Count > 0 Then k_plan.Tables.Add(k_archive)
        Return New HashSet(Of DataRow)(k_reasons.Keys)
    End Function

    Private Shared Function IdOf(k_row As DataRow, k_column As String) As Integer?
        If Not k_row.Table.Columns.Contains(k_column) Then Return Nothing
        Dim k_value = AdeValue.ToInt(k_row(k_column), Nothing)
        If k_value Is Nothing Then Return Nothing
        Return CInt(k_value)
    End Function

    Private Shared Function KeysOf(k_table As DataTable, k_column As String) As HashSet(Of Integer)
        Dim k_keys As New HashSet(Of Integer)()
        For Each k_row As DataRow In k_table.Rows
            Dim k_id = IdOf(k_row, k_column)
            If k_id.HasValue Then k_keys.Add(k_id.Value)
        Next
        Return k_keys
    End Function

    Private Shared Sub MissingLink(k_row As DataRow, k_column As String, k_parent As String,
                                   k_keys As HashSet(Of Integer), k_missing As List(Of String))
        Dim k_id = IdOf(k_row, k_column)
        If k_id.HasValue AndAlso Not k_keys.Contains(k_id.Value) Then k_missing.Add($"{k_column}={k_id.Value}: {k_parent} inexistent")
    End Sub

    Private Shared Function YearsOf(k_row As DataRow, k_months As Dictionary(Of Integer, Integer)) As HashSet(Of Integer)
        Dim k_years As New HashSet(Of Integer)()
        Dim k_dateColumn = If(k_row.Table.TableName = "AlteDoc", "DataDoc", "Data")
        If k_row.Table.Columns.Contains(k_dateColumn) Then
            Dim k_date = AdeValue.ToMoment(k_row(k_dateColumn))
            If k_date IsNot Nothing Then k_years.Add(CType(k_date, DateTime).Year)
        End If
        Dim k_idl = IdOf(k_row, "IDL")
        Dim k_year As Integer
        If k_idl.HasValue AndAlso k_months.TryGetValue(k_idl.Value, k_year) Then k_years.Add(k_year)
        Return k_years
    End Function

End Class
