Imports KBot.Migrator

Partial Public NotInheritable Class VrcWriter

    ''' <summary>Application requirements, not a second physical schema. Business writes use the runner's column list.</summary>
    Private Shared Function RequiredTables() As List(Of (Name As String, Columns As String()))
        Dim k_tables As New List(Of (Name As String, Columns As String()))(ControlTables)
        For Each k_table In VrcImportMap.Tables()
            k_tables.Add((k_table.TargetName, VrcRunner.InsertColumns(k_table).ToArray()))
        Next
        Return k_tables
    End Function

    ''' <summary>The same gate is used by Testează and immediately before the transaction, using KBot.Migrator's live metadata.</summary>
    Friend Shared Function SchemaProblems(k_schema As TargetSchema) As List(Of String)
        Dim k_found As New List(Of String)()
        For Each k_table In RequiredTables()
            If Not k_schema.HasTable(k_table.Name) Then
                k_found.Add($"Lipsește tabelul {k_table.Name} în «{k_schema.SchemaName}». Aplicați DDL-ul incremental necesar pe AVACONT_SURSA și sincronizați schema; migratorul nu creează tabele.")
                Continue For
            End If
            For Each k_name In k_table.Columns
                If k_schema.Column(k_table.Name, k_name) Is Nothing Then
                    k_found.Add($"Lipsește coloana {k_table.Name}.{k_name} în «{k_schema.SchemaName}». Sincronizați schema după aplicarea DDL-ului incremental pe AVACONT_SURSA.")
                End If
            Next
        Next

        For Each k_table In VrcImportMap.Tables()
            If Not k_schema.HasTable(k_table.TargetName) Then Continue For
            Dim k_written As New HashSet(Of String)(VrcRunner.InsertColumns(k_table), StringComparer.OrdinalIgnoreCase)
            For Each k_column In k_schema.RequiredColumns(k_table.TargetName)
                If Not k_written.Contains(k_column.Name) Then
                    k_found.Add($"{k_table.TargetName}.{k_column.Name} este obligatorie în schema reală, fără valoare implicită, dar importul nu o furnizează. Completați maparea Access.")
                End If
            Next
            For Each k_name In k_written
                Dim k_column = k_schema.Column(k_table.TargetName, k_name)
                If k_column IsNot Nothing AndAlso (Not k_column.IsWritable OrElse k_column.IsAutoIncrement) Then
                    k_found.Add($"Importul încearcă să furnizeze {k_table.TargetName}.{k_name}, dar schema reală o generează. Corectați maparea înainte de import.")
                End If
            Next
            Dim k_keys = k_schema.Columns(k_table.TargetName).Where(Function(k_c) k_c.IsAutoIncrement).ToList()
            If k_keys.Count <> 1 OrElse Not k_schema.UniqueKeysOf(k_table.TargetName).Any(Function(k_key) k_key.IsPrimary AndAlso k_keys.All(Function(k_c) k_key.Columns.Contains(k_c.Name, StringComparer.OrdinalIgnoreCase))) Then
                k_found.Add($"{k_table.TargetName} trebuie să aibă o singură coloană AUTO_INCREMENT în cheia primară pentru remaparea identificatorilor Access.")
            End If
            For Each k_column In k_table.Columns.Where(Function(k_c) k_c.FieldClass = "VALOARE-INT" OrElse k_c.FieldClass = "COTA")
                CheckDecimal(k_schema, k_table.TargetName, k_column.Name, k_found)
            Next
        Next
        CheckDecimal(k_schema, "VC_Settings", "IntValue", k_found)
        CheckFiscalIdentity(k_schema, "VC_NIR", "IdPartener", k_found)
        CheckFiscalIdentity(k_schema, "VC_FACTURI", "PARTENER_ID", k_found)

        If k_found.Count = 0 Then
            Try
                VrcImportMap.OrderedTables(k_schema)
            Catch ex As InvalidOperationException
                k_found.Add(ex.Message)
            Catch ex As WriteOrderCycleException
                k_found.Add(ex.Message)
            End Try
        End If
        Return k_found
    End Function

    ''' <summary>Both document headers use the same nullable textual fiscal identity; legacy IDs are not supplied.</summary>
    Private Shared Sub CheckFiscalIdentity(k_schema As TargetSchema, k_table As String, k_legacy As String, k_found As List(Of String))
        If Not k_schema.HasTable(k_table) Then Return ' Already reported by RequiredTables.
        Dim k_column = k_schema.Column(k_table, "CodPartener")
        If k_column Is Nothing Then
            k_found.Add($"Pentru furnizorii prin cod fiscal, aplicați manual sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql pe bazele folosite de migrator ({k_table}.CodPartener lipsește).")
        ElseIf Not String.Equals(k_column.DataType, "varchar", StringComparison.OrdinalIgnoreCase) OrElse
               Not k_column.CharacterMaxLength.HasValue OrElse k_column.CharacterMaxLength.Value < 50 OrElse
               Not k_column.IsNullable OrElse Not k_column.IsWritable OrElse k_column.IsAutoIncrement Then
            k_found.Add($"{k_table}.CodPartener trebuie să fie text VARCHAR de minimum 50 de caractere, NULL permis și scriibil, pentru identitatea fiscală VERCON. Aplicați manual DDL-ul necesar; migratorul nu modifică structura.")
        End If
        Dim k_old = k_schema.Column(k_table, k_legacy)
        If k_old IsNot Nothing AndAlso Not k_old.IsNullable Then
            k_found.Add($"{k_table}.{k_legacy} trebuie să permită NULL: importul folosește exclusiv CodPartener ca identitate fiscală și nu furnizează ID-ul istoric. Corectați manual structura înainte de transfer.")
        End If
    End Sub

    Private Shared Sub CheckDecimal(k_schema As TargetSchema, k_table As String, k_name As String, k_found As List(Of String))
        Dim k_column = k_schema.Column(k_table, k_name)
        If k_column Is Nothing Then Return ' Already reported by the required-column gate.
        If Not String.Equals(k_column.DataType, "decimal", StringComparison.OrdinalIgnoreCase) OrElse
           Not String.Equals(k_column.ColumnType.Replace(" ", String.Empty), "decimal(20,4)", StringComparison.OrdinalIgnoreCase) Then
            k_found.Add($"{k_table}.{k_name} este {k_column.ColumnType} în «{k_schema.SchemaName}»; conversia exactă a valorilor cere DECIMAL(20,4). Aplicați manual VC_precizie_valori_4_zecimale.sql pe sursă și sincronizați schema.")
        End If
    End Sub

End Class
