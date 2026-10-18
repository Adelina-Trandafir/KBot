Imports System.Globalization
Imports System.Text

''' <summary>
''' Access SQL for a group of findings of one table and one field, to be run in the source file so the operator sees the exact rows.
''' Built only from what the finding itself carries (table, field, rule) and the import map; never reads Access.
''' </summary>
Friend NotInheritable Class VrcAccessSql

    Private Const MaxKeys As Integer = 100

    Private Sub New()
    End Sub

    ''' <summary>The queries (one or more statements, each ending with «;») for the findings of one field; empty when none is known for the rule.</summary>
    Friend Shared Function ForField(k_issues As IList(Of VrcIssue)) As String
        If k_issues Is Nothing OrElse k_issues.Count = 0 Then Return String.Empty
        Dim k_first = k_issues(0)
        If k_first.Severity = "CONVERSIE" Then Return String.Empty
        Dim k_table As String = Bracket(k_first.TableName)
        Dim k_schema = VrcImportMap.Find(k_first.TableName)
        Dim k_pk As String = If(k_schema Is Nothing OrElse k_schema.SourceKey.Length = 0, String.Empty, Bracket(k_schema.SourceKey))
        Dim k_column = If(k_schema Is Nothing OrElse k_first.FieldName.Length = 0, Nothing,
                          k_schema.Columns.FirstOrDefault(Function(k_c) String.Equals(k_c.Name, k_first.FieldName, StringComparison.OrdinalIgnoreCase)))
        Dim k_field As String = If(k_first.FieldName.Length > 0, Bracket(k_first.FieldName), String.Empty)
        Dim k_select As String = If(k_pk.Length > 0, k_pk & ", " & k_field, k_field)
        Dim k_sb As New StringBuilder()

        Select Case k_first.RuleCode
            Case "X10"
                If k_field.Length = 0 Then Return String.Empty
                k_sb.AppendLine($"SELECT {k_select} FROM {k_table} WHERE Nz({k_field},0)<>0{OrderBy(k_pk)};")
                k_sb.AppendLine($"SELECT {k_field}, Count(*) AS Cazuri FROM {k_table} WHERE Nz({k_field},0)<>0 GROUP BY {k_field} ORDER BY Count(*) DESC;")
            Case "X05"
                If k_field.Length = 0 OrElse k_column Is Nothing Then Return KeyList(k_issues, k_table, k_pk)
                If k_column.Link = VrcLinkKind.Origin Then
                    k_sb.AppendLine($"SELECT {k_select} FROM {k_table} WHERE Nz({k_field},0)<>0 AND {k_field} NOT IN (SELECT IDBP FROM OI_Predare WHERE IDBP Is Not Null) AND {k_field} NOT IN (SELECT IDBT FROM OI_Transfer WHERE IDBT Is Not Null){OrderBy(k_pk)};")
                ElseIf k_column.Parent.Length > 0 Then
                    Dim k_parentKey As String = ParentKey(k_column)
                    ' No table aliases (operator, 18.10.2026): full table names; a table joined to itself would need one, so it falls back to the key list.
                    If k_parentKey.Length = 0 OrElse String.Equals(k_column.Parent, k_first.TableName, StringComparison.OrdinalIgnoreCase) Then Return KeyList(k_issues, k_table, k_pk)
                    Dim k_parent As String = Bracket(k_column.Parent)
                    Dim k_childSelect As String = If(k_pk.Length > 0, $"{k_table}.{k_pk}, {k_table}.{k_field}", $"{k_table}.{k_field}")
                    k_sb.AppendLine($"SELECT {k_childSelect} FROM {k_table} LEFT JOIN {k_parent} ON {k_table}.{k_field}={k_parent}.{k_parentKey} WHERE Nz({k_table}.{k_field},0)<>0 AND {k_parent}.{k_parentKey} Is Null{If(k_pk.Length > 0, $" ORDER BY {k_table}.{k_pk}", "")};")
                Else
                    Return KeyList(k_issues, k_table, k_pk)
                End If
            Case "X14"
                If k_field.Length = 0 Then Return String.Empty
                k_sb.AppendLine($"SELECT {k_select} FROM {k_table} WHERE Nz({k_field},0)<>0 AND {k_field} IN (SELECT IDBP FROM OI_Predare WHERE IDBP Is Not Null) AND {k_field} IN (SELECT IDBT FROM OI_Transfer WHERE IDBT Is Not Null){OrderBy(k_pk)};")
            Case "X11"
                If k_field.Length = 0 Then Return String.Empty
                k_sb.AppendLine($"SELECT {k_select} FROM {k_table} WHERE Abs(Nz({k_field},0))>=10000000000000000{OrderBy(k_pk)};")
            Case "X12"
                If k_field.Length = 0 Then Return String.Empty
                k_sb.AppendLine($"SELECT {k_select} FROM {k_table} WHERE {k_field} Is Not Null AND Abs({k_field}-Round({k_field},0))>0.0000001{OrderBy(k_pk)};")
            Case Else
                If k_first.Severity = "BLOCAJ" Then Return KeyList(k_issues, k_table, k_pk)
                Return String.Empty
        End Select
        Return k_sb.ToString().TrimEnd()
    End Function

    ''' <summary>The key column of the parent table: the one named in the map, else the parent's own source key.</summary>
    Private Shared Function ParentKey(k_column As VrcImportColumn) As String
        If k_column.ParentField.Length > 0 Then Return Bracket(k_column.ParentField)
        Dim k_parent = VrcImportMap.Find(k_column.Parent)
        Return If(k_parent Is Nothing OrElse k_parent.SourceKey.Length = 0, String.Empty, Bracket(k_parent.SourceKey))
    End Function

    ''' <summary>Last resort: the rows by their source keys, when every key is a whole number (the first 100).</summary>
    Private Shared Function KeyList(k_issues As IList(Of VrcIssue), k_table As String, k_pk As String) As String
        If k_pk.Length = 0 Then Return String.Empty
        Dim k_keys As New List(Of Long)()
        For Each k_issue In k_issues
            Dim k_number As Long
            If Not Long.TryParse(k_issue.SourceKey, NumberStyles.Integer, CultureInfo.InvariantCulture, k_number) Then Return String.Empty
            If Not k_keys.Contains(k_number) Then k_keys.Add(k_number)
        Next
        Dim k_list = String.Join(",", k_keys.Take(MaxKeys).Select(Function(k_k) k_k.ToString(CultureInfo.InvariantCulture)))
        Return $"SELECT * FROM {k_table} WHERE {k_pk} IN ({k_list}) ORDER BY {k_pk};"
    End Function

    Private Shared Function OrderBy(k_pk As String) As String
        Return If(k_pk.Length > 0, " ORDER BY " & k_pk, String.Empty)
    End Function

    Private Shared Function Bracket(k_name As String) As String
        Return "[" & k_name.Replace("]", "") & "]"
    End Function

End Class
