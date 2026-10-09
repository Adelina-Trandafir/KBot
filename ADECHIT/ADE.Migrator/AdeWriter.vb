Imports System.Globalization
Imports System.IO
Imports System.Text.Json
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' The MariaDB side: what is in the unit database now, whether it is ready to receive the data, and the write itself.
''' The write is ONE transaction: any failure leaves the database exactly as it was.
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
            GlobalErrorLog.Write("AdeWriter.CountRows", ex)
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
                If CountOne(k_cn, "AD_Imports") > 0 Then k_found.Add("AD_Imports arată că s-a mai migrat o dată în această bază.")
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
            GlobalErrorLog.Write("AdeWriter.Problems", ex)
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
                                 k_progress As IProgress(Of String)) As Dictionary(Of String, Integer)
        Try
            Dim k_written As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Using k_cn = k_server.Open(k_dc)
                Using k_tx = k_cn.BeginTransaction()
                    Try
                        For Each k_planTable In k_plan.Tables
                            k_progress.Report($"Scriu {k_planTable.Table.Target} ({k_planTable.Rows.Count} rânduri)...")
                            k_written(k_planTable.Table.Name) = InsertRows(k_cn, k_tx, k_planTable)
                        Next
                        If k_plan.ReceiptConfig IsNot Nothing Then
                            k_progress.Report("Scriu seria de chitanțe...")
                            InsertReceiptConfig(k_cn, k_tx, k_dc, k_plan.ReceiptConfig)
                        End If
                        InsertImportRecord(k_cn, k_tx, k_source, k_written)
                        k_tx.Commit()
                    Catch
                        k_tx.Rollback()
                        Throw
                    End Try
                End Using
            End Using
            Return k_written
        Catch ex As Exception
            GlobalErrorLog.Write("AdeWriter.Write", ex)
            Throw
        End Try
    End Function

    Private Shared Function InsertRows(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_planTable As AdePlanTable) As Integer
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
                For Each k_col In k_columns
                    Dim k_value = k_row(k_col.Name)
                    k_cmd.Parameters("@" & k_col.Name).Value = If(k_value, DBNull.Value)
                Next
                k_cmd.ExecuteNonQuery()
                k_count += 1
            Next
            Return k_count
        End Using
    End Function

    Private Shared Sub InsertReceiptConfig(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_dc As String, k_config As AdeReceiptConfig)
        Using k_cmd As New MySqlCommand("INSERT INTO AVACONT_COMUN.Unitati_Chitante (DC,Serie,Numar,Explicatie) VALUES (@dc,@s,@n,@e)", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@dc", k_dc)
            k_cmd.Parameters.AddWithValue("@s", k_config.Serie)
            k_cmd.Parameters.AddWithValue("@n", k_config.Numar)
            k_cmd.Parameters.AddWithValue("@e", k_config.Explicatie)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

    Private Shared Sub InsertImportRecord(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_source As AdeSource,
                                          k_written As Dictionary(Of String, Integer))
        Dim k_json = JsonSerializer.Serialize(New With {.counts = k_written, .unit = k_source.Dc})
        Using k_cmd As New MySqlCommand("INSERT INTO AD_Imports (SourceHash,SourceFile,Manifest,Result) VALUES (@h,@f,@m,@r)", k_cn, k_tx)
            k_cmd.Parameters.AddWithValue("@h", k_source.FileHash)
            k_cmd.Parameters.AddWithValue("@f", Path.GetFileName(k_source.FilePath))
            k_cmd.Parameters.AddWithValue("@m", k_json)
            k_cmd.Parameters.AddWithValue("@r", k_json)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
