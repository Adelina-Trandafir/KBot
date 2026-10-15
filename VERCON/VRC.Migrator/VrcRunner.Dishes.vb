Imports System.Globalization
Imports System.Text.RegularExpressions

Partial Friend NotInheritable Class VrcRunner

    Private Shared ReadOnly MealTables As String() = {"MicDejun", "Dejun", "Cina"}

    ''' <summary>Access stores the course label in Tip (Felul 1, Felul 2, ...), independently of IdFel.</summary>
    Private Shared Function DishType(k_raw As Object) As String
        Dim k_text = Regex.Replace(Convert.ToString(k_raw, CultureInfo.InvariantCulture).Trim(), "\s+", " ")
        If k_text.Length = 0 Then Return String.Empty
        Dim k_match = Regex.Match(k_text, "^felul ([0-9]+)$", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
        Dim k_number As Integer
        If Not k_match.Success OrElse Not Integer.TryParse(k_match.Groups(1).Value, NumberStyles.None, CultureInfo.InvariantCulture, k_number) OrElse k_number < 1 Then
            Throw New FormatException("TIP trebuie să conțină numai «Felul 1», «Felul 2», ...")
        End If
        Return "Felul " & k_number.ToString(CultureInfo.InvariantCulture)
    End Function

    ''' <summary>One nomenclature per gestiune from distinct meal TIPs across all its years.</summary>
    Private Sub MigrateDishTypes(k_gest As GestInfo, k_table As VrcTargetTable)
        Dim k_types As New SortedDictionary(Of String, List(Of (Source As VrcPlanSource, TableName As String)))(StringComparer.Ordinal)
        For Each k_source In k_gest.Sources
            For Each k_meal In MealTables
                For Each k_row In ReadRows(k_source, k_meal)
                    _cancel.ThrowIfCancellationRequested()
                    Dim k_label As String
                    Try
                        k_label = DishType(k_row("Tip"))
                    Catch ex As FormatException
                        AddIssue(k_source, k_meal, VrcConvert.KeyText(k_row("Id")), "Tip", "X13", "BLOCAJ",
                                 Convert.ToString(k_row("Tip"), CultureInfo.InvariantCulture), String.Empty, ex.Message)
                        Continue For
                    End Try
                    If k_label.Length = 0 Then Continue For
                    If Not k_types.ContainsKey(k_label) Then k_types(k_label) = New List(Of (Source As VrcPlanSource, TableName As String))()
                    Dim k_contributor = (Source:=k_source, TableName:=k_meal)
                    If Not k_types(k_label).Contains(k_contributor) Then k_types(k_label).Add(k_contributor)
                Next
            Next
        Next
        If k_types.Count = 0 Then Return
        Dim k_tracked = Track(k_gest, 0, k_table, k_gest.Name, 0)
        k_tracked.Count.SourceRows = k_types.Count
        Dim k_pending As New List(Of PendingRow)()
        For Each k_pair In k_types
            Dim k_source = k_pair.Value(0).Source
            Dim k_row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                {"DenFel", k_pair.Key}, {"Explicatie", Nothing}}
            Dim k_values = BuildRow(k_gest, k_source, k_table, k_row, k_pair.Key, k_tracked, 0, Nothing)
            k_pending.Add(New PendingRow() With {.Key = k_pair.Key, .Values = k_values, .Source = k_source,
                          .Contributors = k_pair.Value.Select(Function(k_c) k_c.Source).Distinct().ToList()})
        Next
        Dim k_ids = InsertPending(k_gest, k_table, 0, k_pending)
        For k_i As Integer = 0 To k_pending.Count - 1
            If k_ids(k_i) < 0 Then Continue For
            Dim k_label = k_pending(k_i).Key
            _map($"G{k_gest.Id}|dish-type|{k_label}") = k_ids(k_i)
            For Each k_contributor In k_types(k_label)
                _pendingMap.Add((_sourceIds(k_contributor.Source), k_contributor.TableName, "TIP", k_label, k_ids(k_i)))
                AddIssue(k_contributor.Source, k_contributor.TableName, k_label, "Tip", "N11", "CONVERSIE", k_label,
                         k_ids(k_i).ToString(CultureInfo.InvariantCulture),
                         "Felul de mâncare este creat din TIP; IdFel țintă indică nomenclatorul generat, fără a folosi IdFel din Access.")
            Next
        Next
        FlushMap()
        Log($"{k_gest.Name}: {k_types.Count} feluri de mâncare distincte din TIP (MicDejun / Dejun / Cina); fără explicații în nomenclator.")
    End Sub

    Private Function MealDishId(k_gest As GestInfo, k_source As VrcPlanSource, k_table As VrcTargetTable,
                                k_row As Dictionary(Of String, Object), k_key As String, ByRef k_ok As Boolean) As Object
        Dim k_label As String
        Try
            k_label = DishType(k_row("Tip"))
        Catch ex As FormatException
            AddIssue(k_source, k_table.Name, k_key, "Tip", "X13", "BLOCAJ",
                     Convert.ToString(k_row("Tip"), CultureInfo.InvariantCulture), String.Empty, ex.Message)
            k_ok = False
            Return Nothing
        End Try
        If k_label.Length = 0 Then Return Nothing
        Dim k_id As Long
        If _map.TryGetValue($"G{k_gest.Id}|dish-type|{k_label}", k_id) Then Return k_id
        AddIssue(k_source, k_table.Name, k_key, "Tip", "X13", "BLOCAJ", k_label, String.Empty,
                 "TIP nu are un fel de mâncare în nomenclatorul generat pentru această gestiune.")
        k_ok = False
        Return Nothing
    End Function

End Class
