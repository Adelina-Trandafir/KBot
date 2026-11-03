Option Strict On
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' Turns a saved copy back into SQL text (slice ME02-06). The export is a file for the operator to run on a SCRATCH database:
''' its default target name starts with zz_, so running it can never overwrite the live database by accident. Nothing here
''' touches the server.
''' </summary>
Public NotInheritable Class BackupSql

    Private Const BatchBytes As Integer = 1000000
    Private Shared ReadOnly RxDefiner As New Regex("DEFINER\s*=\s*`[^`]*`@`[^`]*`\s*", RegexOptions.Compiled Or RegexOptions.IgnoreCase)

    Private Sub New()
    End Sub

    ''' <summary>The default scratch database name for an export of this snapshot.</summary>
    Public Shared Function DefaultTarget(k_manifest As SnapshotManifest) As String
        Return "zz_restore_" & k_manifest.Db & "_" & k_manifest.FinishedAt.ToString("yyyyMMdd", Globalization.CultureInfo.InvariantCulture)
    End Function

    Public Shared Function Quote(k_name As String) As String
        Return "`" & k_name.Replace("`", "``") & "`"
    End Function

    Private Shared Function Escape(k_text As String) As String
        Dim k_sb As New StringBuilder(k_text.Length + 8)
        For Each k_ch As Char In k_text
            Select Case k_ch
                Case "\"c : k_sb.Append("\\")
                Case "'"c : k_sb.Append("\'")
                Case ChrW(0) : k_sb.Append("\0")
                Case ChrW(10) : k_sb.Append("\n")
                Case ChrW(13) : k_sb.Append("\r")
                Case ChrW(26) : k_sb.Append("\Z")
                Case Else : k_sb.Append(k_ch)
            End Select
        Next
        Return k_sb.ToString()
    End Function

    ''' <summary>One value as an SQL literal: NULL, 0x... for binary columns, a quoted string for everything else (MariaDB converts it to the column type).</summary>
    Public Shared Function Literal(k_value As String, k_column As BackupColumn) As String
        If k_value Is Nothing Then Return "NULL"
        If k_column.IsBinary Then
            Dim k_bytes As Byte() = Convert.FromBase64String(k_value)
            Return If(k_bytes.Length = 0, "''", "0x" & Convert.ToHexString(k_bytes))
        End If
        Return "'" & Escape(k_value) & "'"
    End Function

    Private Shared Function ColumnList(k_table As BackupTable) As String
        Return String.Join(",", k_table.Columns.Select(Function(k_c) Quote(k_c.Name)))
    End Function

    Private Shared Function RowValues(k_table As BackupTable, k_row As String()) As String
        Dim k_parts(k_table.Columns.Count - 1) As String
        For k_i As Integer = 0 To k_parts.Length - 1
            k_parts(k_i) = Literal(If(k_i < k_row.Length, k_row(k_i), Nothing), k_table.Columns(k_i))
        Next
        Return "(" & String.Join(",", k_parts) & ")"
    End Function

    ''' <summary>A statement that puts ONE record back into the real table (insert, or overwrite the same key). For the operator to read, then run.</summary>
    Public Shared Function UpsertStatement(k_table As BackupTable, k_values As String()) As String
        Dim k_pk As New HashSet(Of String)(k_table.PrimaryKey, StringComparer.OrdinalIgnoreCase)
        Dim k_updates As List(Of String) = k_table.Columns.Where(Function(k_c) Not k_pk.Contains(k_c.Name)).Select(Function(k_c) Quote(k_c.Name) & "=VALUES(" & Quote(k_c.Name) & ")").ToList()
        Dim k_text As String = "INSERT " & If(k_updates.Count = 0, "IGNORE ", String.Empty) & "INTO " & Quote(k_table.Name) & " (" & ColumnList(k_table) & ") VALUES " & RowValues(k_table, k_values)
        If k_updates.Count > 0 Then k_text &= " ON DUPLICATE KEY UPDATE " & String.Join(", ", k_updates)
        Return k_text & ";"
    End Function

    ''' <summary>
    ''' Writes a whole copy (or the chosen tables of it) as SQL: the table definitions, the rows, then views / triggers / routines / events.
    ''' A path ending in .gz is compressed. Returns the number of rows written.
    ''' </summary>
    Public Shared Function Export(k_store As BackupStore, k_manifest As SnapshotManifest, k_path As String, k_targetDb As String,
                                  k_onlyTables As ISet(Of String), k_progress As Action(Of String)) As Long
        Try
            If String.IsNullOrWhiteSpace(k_targetDb) OrElse Not k_targetDb.StartsWith("zz_", StringComparison.OrdinalIgnoreCase) Then
                Throw New BackupException("Baza țintă a exportului trebuie să înceapă cu zz_ (o bază de probă), ca să nu poată suprascrie o bază vie.")
            End If
            Dim k_rowsTotal As Long = 0
            Using k_file As New FileStream(k_path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16)
                Dim k_target As Stream = k_file
                Dim k_gzip As GZipStream = Nothing
                If k_path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) Then
                    k_gzip = New GZipStream(k_file, CompressionLevel.Optimal, True)
                    k_target = k_gzip
                End If
                Try
                    Using k_writer As New StreamWriter(k_target, New UTF8Encoding(False), 1 << 16)
                        k_writer.NewLine = vbLf
                        k_writer.WriteLine("-- KBotAdmin ME02: " & k_manifest.Db & ", copia din " & k_manifest.Finished & " (instantaneu " & k_manifest.Id & "), motiv: " & k_manifest.Reason.Replace(vbLf, " "))
                        k_writer.WriteLine("-- Se rulează pe o bază de probă. Baza țintă: " & k_targetDb)
                        k_writer.WriteLine("SET NAMES utf8mb4;")
                        k_writer.WriteLine("SET time_zone='+00:00';")                  ' the server's sessions are UTC (PYTHON/docs/TIMEZONES.md)
                        k_writer.WriteLine("SET FOREIGN_KEY_CHECKS=0, UNIQUE_CHECKS=0;")
                        k_writer.WriteLine("SET SESSION sql_mode='NO_AUTO_VALUE_ON_ZERO';")
                        k_writer.WriteLine("CREATE DATABASE IF NOT EXISTS " & Quote(k_targetDb) & " DEFAULT CHARACTER SET utf8mb3;")
                        k_writer.WriteLine("USE " & Quote(k_targetDb) & ";")
                        For Each k_table As BackupTable In k_manifest.Tables
                            If k_onlyTables IsNot Nothing AndAlso k_onlyTables.Count > 0 AndAlso Not k_onlyTables.Contains(k_table.Name) Then Continue For
                            k_progress?.Invoke("Se scrie tabela " & k_table.Name & "...")
                            k_writer.WriteLine("DROP TABLE IF EXISTS " & Quote(k_table.Name) & ";")
                            k_writer.WriteLine(Encoding.UTF8.GetString(k_store.GetObject(k_table.DdlObject)) & ";")
                            k_rowsTotal += WriteRows(k_store, k_table, k_writer)
                        Next
                        If k_onlyTables Is Nothing OrElse k_onlyTables.Count = 0 Then
                            For Each k_extra As BackupExtra In k_manifest.Extras
                                Dim k_ddl As String = RxDefiner.Replace(Encoding.UTF8.GetString(k_store.GetObject(k_extra.DdlObject)), String.Empty)
                                If k_ddl.Trim().Length = 0 Then Continue For
                                k_writer.WriteLine("DELIMITER ;;")
                                k_writer.WriteLine(k_ddl & ";;")
                                k_writer.WriteLine("DELIMITER ;")
                            Next
                        End If
                        k_writer.WriteLine("SET FOREIGN_KEY_CHECKS=1, UNIQUE_CHECKS=1;")
                    End Using
                Finally
                    If k_gzip IsNot Nothing Then k_gzip.Dispose()
                End Try
            End Using
            Return k_rowsTotal
        Catch ex As Exception
            GlobalErrorLog.Write("BackupSql.Export", ex)
            Throw
        End Try
    End Function

    Private Shared Function WriteRows(k_store As BackupStore, k_table As BackupTable, k_writer As StreamWriter) As Long
        Dim k_count As Long = 0
        Dim k_head As String = "INSERT INTO " & Quote(k_table.Name) & " (" & ColumnList(k_table) & ") VALUES "
        Dim k_batch As New StringBuilder()
        For Each k_page As BackupPage In k_table.Pages
            For Each k_row As String() In BackupReader.ReadPage(k_store, k_page)
                If k_batch.Length = 0 Then k_batch.Append(k_head) Else k_batch.Append(",")
                k_batch.Append(RowValues(k_table, k_row))
                k_count += 1
                If k_batch.Length >= BatchBytes Then
                    k_writer.Write(k_batch.ToString())
                    k_writer.WriteLine(";")
                    k_batch.Clear()
                End If
            Next
        Next
        If k_batch.Length > 0 Then
            k_writer.Write(k_batch.ToString())
            k_writer.WriteLine(";")
        End If
        Return k_count
    End Function

End Class
