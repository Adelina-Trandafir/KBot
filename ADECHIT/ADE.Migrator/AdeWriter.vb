Imports System.Globalization
Imports System.IO
Imports System.Text.Json
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' The MariaDB side: what is in the unit database now, whether it is ready to receive the data, and the write itself.
''' The write is one transaction. A failed commit acknowledgement is reported as an unknown outcome.
''' </summary>
Public NotInheritable Class AdeWriter

    Private Sub New()
    End Sub

    ''' <summary>Every table the migration writes, in insertion order, with the two it builds itself.</summary>
    Private Shared Function AllTargets(k_plan As AdePlan) As List(Of AdeTable)
        Return k_plan.Tables.Select(Function(t) t.Table).ToList()
    End Function

    ''' <summary>Rows now in each AD_ table of <paramref name="k_dc"/>; -1 when the table does not exist.</summary>
    Public Shared Function CountRows(k_server As TargetServer, k_dc As String, k_tables As IEnumerable(Of AdeTable)) As Dictionary(Of String, Integer)
        Try
            Dim k_result As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Using k_cn = k_server.Open(k_dc)
                For Each k_table In k_tables
                    k_result(k_table.Name) = CountOne(k_cn, k_table.Target)
                Next
            End Using
            Return k_result
        Catch ex As Exception
            ' The UI boundary records the propagated exception once.
            Throw
        End Try
    End Function

    Private Shared Function CountOne(k_cn As MySqlConnection, k_tableName As String) As Integer
        Try
            Using k_cmd As New MySqlCommand($"SELECT COUNT(*) FROM `{k_tableName}`", k_cn)
                Return Convert.ToInt32(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
            End Using
        Catch ex As MySqlException When ex.ErrorCode = MySqlErrorCode.NoSuchTable
            Return -1
        End Try
    End Function

    ''' <summary>
    ''' What stops a write: no such database, a missing table or column (sql/AD_03_schema_finala.sql not run), tables that
    ''' already hold rows, a receipt series that already exists. Empty list = ready.
    ''' </summary>
    Public Shared Function Problems(k_server As TargetServer, k_dc As String, k_plan As AdePlan) As List(Of String)
        Try
            Dim k_found As New List(Of String)()
            If Not k_server.DatabaseNames().Contains(k_dc, StringComparer.OrdinalIgnoreCase) Then
                k_found.Add($"Pe server nu există baza «{k_dc}» (DC-ul din Access).")
                Return k_found
            End If
            Using k_cn = k_server.Open(k_dc)
                For Each k_table In AllTargets(k_plan)
                    Dim k_have = ColumnsOf(k_cn, k_dc, k_table.Target)
                    If k_have.Count = 0 Then
                        k_found.Add($"Lipsește tabelul {k_table.Target}. Rulați întâi sql/AD_03_schema_finala.sql pe baza «{k_dc}».")
                        Continue For
                    End If
                    For Each k_col In k_table.Columns
                        If Not k_have.Contains(k_col.Name) Then k_found.Add($"Lipsește coloana {k_table.Target}.{k_col.Name}.")
                    Next
                    Dim k_rows = CountOne(k_cn, k_table.Target)
                    If k_rows > 0 Then k_found.Add($"{k_table.Target} are deja {k_rows} rânduri; migrarea scrie numai în tabele goale.")
                Next
                Dim k_importColumns = ColumnsOf(k_cn, k_dc, "AD_Imports")
                For Each k_column In {"SourceHash", "SourceFile", "Manifest", "Result"}
                    If Not k_importColumns.Contains(k_column) Then k_found.Add($"Lipsește coloana AD_Imports.{k_column}.")
                Next
                If CountOne(k_cn, "AD_Imports") > 0 Then k_found.Add("AD_Imports arată că s-a mai migrat o dată în această bază.")
                ' Rollback is only meaningful when every destination participates in the transaction.
                Dim k_targets As New List(Of (Database As String, Target As String))
                For Each k_table In AllTargets(k_plan)
                    k_targets.Add((k_dc, k_table.Target))
                Next
                k_targets.Add((k_dc, "AD_Imports"))
                If k_plan.ReceiptConfig IsNot Nothing Then k_targets.Add(("AVACONT_COMUN", "Unitati_Chitante"))
                For Each k_target In k_targets
                    Using k_engine As New MySqlCommand("SELECT ENGINE FROM information_schema.TABLES WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t", k_cn)
                        k_engine.Parameters.AddWithValue("@db", k_target.Database)
                        k_engine.Parameters.AddWithValue("@t", k_target.Target)
                        If Not String.Equals(Convert.ToString(k_engine.ExecuteScalar(), CultureInfo.InvariantCulture), "InnoDB", StringComparison.OrdinalIgnoreCase) Then
                            k_found.Add($"{k_target.Database}.{k_target.Target} trebuie să existe și să folosească InnoDB pentru rollback.")
                        End If
                    End Using
                Next
                If k_plan.ReceiptConfig IsNot Nothing Then
                    Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM AVACONT_COMUN.Unitati_Chitante WHERE DC=@dc", k_cn)
                        k_cmd.Parameters.AddWithValue("@dc", k_dc)
                        If Convert.ToInt32(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0 Then
                            k_found.Add($"Seria de chitanțe a lui {k_dc} există deja în AVACONT_COMUN.Unitati_Chitante; nu se suprascrie.")
                        End If
                    End Using
                End If
            End Using
            Return k_found
        Catch ex As Exception
            ' The UI boundary records the propagated exception once.
            Throw
        End Try
    End Function

    Private Shared Function ColumnsOf(k_cn As MySqlConnection, k_dc As String, k_tableName As String) As HashSet(Of String)
        Dim k_names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New MySqlCommand("SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@db AND TABLE_NAME=@t", k_cn)
            k_cmd.Parameters.AddWithValue("@db", k_dc)
            k_cmd.Parameters.AddWithValue("@t", k_tableName)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_names.Add(k_reader.GetString(0))
                End While
            End Using
        End Using
        Return k_names
    End Function

    ''' <summary>Writes the whole plan in one transaction. Returns the rows written per table.</summary>
    Public Shared Function Write(k_server As TargetServer, k_dc As String, k_source As AdeSource, k_plan As AdePlan,
                                 k_progress As IProgress(Of String), k_cancel As CancellationToken,
                                 k_journalRoot As String) As Dictionary(Of String, Integer)
        Try
            If k_plan.HasBlocking Then Throw New InvalidOperationException("Planul are blocaje; migrarea nu poate porni.")
            Using k_file As New FileStream(k_source.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim k_hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(k_file)).ToLowerInvariant()
                If Not String.Equals(k_hash, k_source.FileHash, StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException("Fișierul Access s-a modificat după citire. Citiți din nou fișierul și verificați planul.")
                End If
            End Using
            k_cancel.ThrowIfCancellationRequested()
            Dim k_problems = Problems(k_server, k_dc, k_plan)
            If k_problems.Count > 0 Then Throw New InvalidOperationException(String.Join(Environment.NewLine, k_problems))
            Dim k_written As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Using k_dump As New SqlDumpWriter(k_journalRoot, k_dc, Sub(message) k_progress.Report(message))
                k_progress.Report("Jurnal SQL: " & k_dump.Folder)
                k_dump.WriteInfo({"Aplicație: ADE.Migrator", "Bază destinație: " & k_dc, "DC sursă Access: " & k_source.Dc, "Fișier: " & Path.GetFileName(k_source.FilePath), "SHA-256: " & k_source.FileHash})
                Using k_cn = k_server.Open(k_dc)
                    Using k_tx = k_cn.BeginTransaction()
                        Dim k_commitAttempted = False
                        Try
                            For Each k_planTable In k_plan.Tables
                                k_progress.Report($"Scriu {k_planTable.Table.Target} ({k_planTable.Rows.Count} rânduri)...")
                                k_cancel.ThrowIfCancellationRequested()
                                k_written(k_planTable.Table.Name) = InsertRows(k_cn, k_tx, k_planTable, k_cancel, k_progress, k_dump)
                            Next
                            If k_plan.ReceiptConfig IsNot Nothing Then
                                k_progress.Report("Scriu seria de chitanțe...")
                                k_cancel.ThrowIfCancellationRequested()
                                InsertReceiptConfig(k_cn, k_tx, k_dc, k_plan.ReceiptConfig, k_dump)
                            End If
                            InsertImportRecord(k_cn, k_tx, k_source, k_written, k_dump, k_dc)
                            k_cancel.ThrowIfCancellationRequested()
                            k_commitAttempted = True
                            k_tx.Commit()
                            k_dump.WriteFinal(True, k_written.Select(Function(pair) $"{pair.Key}: {pair.Value} rânduri"), Nothing)
                        Catch ex As Exception
                            Try
                                k_tx.Rollback()
                            Catch rollbackEx As Exception
                                k_dump.WriteComment("_outcome", "Rezultat necunoscut: rollback neconfirmat. Verificați serverul înainte de reluare.")
                                k_dump.FlushAll()
                                Throw New AggregateException("Rezultatul tranzacției nu poate fi confirmat. Verificați serverul înainte de reluare.", ex, rollbackEx)
                            End Try
                            ' A failed COMMIT acknowledgement can mean that the server committed before the connection failed.
                            If k_commitAttempted Then
                                k_dump.WriteComment("_outcome", "COMMIT neconfirmat. Verificați AD_Imports și tabelele pe server înainte de reluare.")
                                k_dump.FlushAll()
                                Throw New InvalidOperationException("Confirmarea COMMIT nu a fost primită. Verificați serverul înainte de reluare.", ex)
                            End If
                            k_dump.WriteFinal(False, {"Rollback confirmat."}, ex)
                            Throw
                        End Try
                    End Using
                End Using
            End Using
            Return k_written
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            ' Preserve the stack and let the UI boundary record the failure once.
            Throw
        End Try
    End Function

    Private Shared Function InsertRows(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_planTable As AdePlanTable,
                                      k_cancel As CancellationToken, k_progress As IProgress(Of String), k_dump As SqlDumpWriter) As Integer
        Dim k_table = k_planTable.Table
        ' The two tables the migration builds have their own AUTO_INCREMENT key; the others keep the key they had in Access.
        Dim k_auto = k_planTable.SourceRows < 0
        Dim k_columns = k_table.Columns.Where(Function(c) Not (k_auto AndAlso c.Name = k_table.Key)).ToList()
        If k_planTable.Rows.Count = 0 Then Return 0
        Dim k_sql = $"INSERT INTO `{k_table.Target}` ({String.Join(",", k_columns.Select(Function(c) $"`{c.Name}`"))}) " &
                    $"VALUES ({String.Join(",", k_columns.Select(Function(c) "@" & c.Name))})"
        Using k_cmd As New MySqlCommand(k_sql, k_cn, k_tx)
            For Each k_col In k_columns
                k_cmd.Parameters.Add(New MySqlParameter() With {.ParameterName = "@" & k_col.Name})
            Next
            Dim k_count = 0
            For Each k_row In k_planTable.Rows
                k_cancel.ThrowIfCancellationRequested()
                For Each k_col In k_columns
                    Dim k_value = k_row(k_col.Name)
                    k_cmd.Parameters("@" & k_col.Name).Value = If(k_value, DBNull.Value)
                Next
                RecordCommand(k_dump, k_table.Target, k_cmd)
                k_cmd.ExecuteNonQuery()
                k_count += 1
                If k_count Mod 100 = 0 OrElse k_count = k_planTable.Rows.Count Then
                    k_dump.FlushAll()
                    k_progress.Report($"{k_table.Target}: {k_count}/{k_planTable.Rows.Count} rânduri pregătite; tranzacția nu este încă confirmată.")
                End If
            Next
            Return k_count
        End Using
    End Function

    Private Shared Sub InsertReceiptConfig(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_dc As String, k_config As AdeReceiptConfig, k_dump As SqlDumpWriter)
        Using k_cmd As New MySqlCommand("INSERT INTO AVACONT_COMUN.Unitati_Chitante (DC,Serie,Numar,Explicatie) VALUES (@dc,@s,@n,@e)", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@dc", k_dc)
            k_cmd.Parameters.AddWithValue("@s", k_config.Serie)
            k_cmd.Parameters.AddWithValue("@n", k_config.Numar)
            k_cmd.Parameters.AddWithValue("@e", k_config.Explicatie)
            RecordCommand(k_dump, "Unitati_Chitante", k_cmd)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Shared Sub InsertImportRecord(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_source As AdeSource,
                                          k_written As Dictionary(Of String, Integer), k_dump As SqlDumpWriter, targetDc As String)
        Dim k_json = JsonSerializer.Serialize(New With {.counts = k_written, .unit = targetDc, .source_unit = k_source.Dc})
        Using k_cmd As New MySqlCommand("INSERT INTO AD_Imports (SourceHash,SourceFile,Manifest,Result) VALUES (@h,@f,@m,@r)", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@h", k_source.FileHash)
            k_cmd.Parameters.AddWithValue("@f", Path.GetFileName(k_source.FilePath))
            k_cmd.Parameters.AddWithValue("@m", k_json)
            k_cmd.Parameters.AddWithValue("@r", k_json)
            RecordCommand(k_dump, "AD_Imports", k_cmd)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Renders the actual parameter values through the shared SQL literal converter before execution.</summary>
    Private Shared Sub RecordCommand(k_dump As SqlDumpWriter, k_table As String, k_command As MySqlCommand)
        Dim k_sql = k_command.CommandText
        ' Replace placeholders in one pass: inserted text may itself contain another parameter name.
        Dim k_values = k_command.Parameters.Cast(Of MySqlParameter)().ToDictionary(Function(p) p.ParameterName, Function(p) ValueConverter.ToLiteral(p.Value))
        k_sql = System.Text.RegularExpressions.Regex.Replace(k_sql, "@[A-Za-z][A-Za-z0-9_]*", Function(m) k_values(m.Value))
        k_dump.WriteStatement(k_table, k_sql)
    End Sub

End Class
