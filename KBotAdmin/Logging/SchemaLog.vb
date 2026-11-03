Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>One row of the server's <c>schema_diff_log</c>: a DDL statement the schema sync planned for a database, and when it ran.</summary>
Public NotInheritable Class SchemaChange
    Public Property Id As Long
    Public Property TargetDb As String = String.Empty
    Public Property TableName As String = String.Empty
    Public Property ObjectName As String = String.Empty
    ''' <summary>TABLE, COLUMN, INDEX, FK, PK, COLLATION, RENAME_CLEANUP.</summary>
    Public Property ObjectType As String = String.Empty
    ''' <summary>CREATE, ADD, MODIFY, DROP, RENAME.</summary>
    Public Property ActionType As String = String.Empty
    Public Property Ddl As String = String.Empty
    ''' <summary>SAFE or FORCE.</summary>
    Public Property SyncMode As String = String.Empty
    Public Property Destructive As Boolean
    Public Property CreatedAt As Date = Date.MinValue
    Public Property ExecutedAt As Date = Date.MinValue
    Public Property ErrorText As String = String.Empty

    ''' <summary>It ran: it has an execution time and no error.</summary>
    Public ReadOnly Property Executed As Boolean
        Get
            Return ExecutedAt <> Date.MinValue AndAlso ErrorText.Length = 0
        End Get
    End Property

    ''' <summary>When it happened: the execution time, else the time it was planned.</summary>
    Public ReadOnly Property Moment As Date
        Get
            Return If(ExecutedAt <> Date.MinValue, ExecutedAt, CreatedAt)
        End Get
    End Property
End Class

''' <summary>
''' Reads <c>schema_diff_log</c> out of the COMUN dump in <c>MariaDB_Schema\</c>: one INSERT per line, thirteen values per row. A row that does not
''' have the expected shape is skipped and counted, never guessed at. Only that table's lines are parsed; the rest of the 10 MB dump is passed over.
''' </summary>
Public Module SchemaLogScanner

    Private Const InsertPrefix As String = "INSERT INTO `schema_diff_log` VALUES ("
    Private Const ColumnCount As Integer = 13

    ''' <summary>The changes, oldest first, and the number of lines that could not be read.</summary>
    Public Function Read(k_folder As String, ByRef k_skipped As Integer, ByRef k_dumpWritten As Date) As List(Of SchemaChange)
        Try
            Dim k_path As String = Path.Combine(k_folder, "AVACONT_COMUN.sql")
            If Not File.Exists(k_path) Then Throw New AdminException("Nu găsesc AVACONT_COMUN.sql în MariaDB_Schema (jurnalul schema_diff_log e acolo).")
            k_dumpWritten = File.GetLastWriteTime(k_path)
            k_skipped = 0
            Dim k_result As New List(Of SchemaChange)()
            For Each k_line As String In File.ReadLines(k_path, Encoding.UTF8)
                If Not k_line.StartsWith(InsertPrefix, StringComparison.Ordinal) Then Continue For
                Dim k_values As List(Of String) = ParseValues(k_line, InsertPrefix.Length)
                If k_values Is Nothing OrElse k_values.Count <> ColumnCount Then
                    k_skipped += 1
                    Continue For
                End If
                Dim k_change As SchemaChange = ToChange(k_values)
                If k_change Is Nothing Then
                    k_skipped += 1
                    Continue For
                End If
                k_result.Add(k_change)
            Next
            k_result.Sort(Function(k_a, k_b)
                              Dim k_cmp As Integer = k_a.Moment.CompareTo(k_b.Moment)
                              Return If(k_cmp <> 0, k_cmp, k_a.Id.CompareTo(k_b.Id))
                          End Function)
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("SchemaLogScanner.Read", ex)
            Throw
        End Try
    End Function

    Private Function ToChange(k_v As List(Of String)) As SchemaChange
        Dim k_id As Long
        If Not Long.TryParse(k_v(0), NumberStyles.Integer, CultureInfo.InvariantCulture, k_id) Then Return Nothing
        Dim k_change As New SchemaChange With {
            .Id = k_id,
            .TargetDb = If(k_v(1), String.Empty),
            .TableName = If(k_v(2), String.Empty),
            .ObjectName = If(k_v(3), String.Empty),
            .ObjectType = If(k_v(4), String.Empty),
            .ActionType = If(k_v(5), String.Empty),
            .Ddl = If(k_v(6), String.Empty),
            .SyncMode = If(k_v(7), String.Empty),
            .Destructive = k_v(8) = "1",
            .ErrorText = If(k_v(12), String.Empty)}
        k_change.CreatedAt = ParseDate(k_v(10))
        k_change.ExecutedAt = ParseDate(k_v(11))
        If k_change.Moment = Date.MinValue Then Return Nothing
        Return k_change
    End Function

    Private Function ParseDate(k_text As String) As Date
        Dim k_date As Date
        If k_text IsNot Nothing AndAlso Date.TryParseExact(k_text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, k_date) Then Return k_date
        Return Date.MinValue
    End Function

    ''' <summary>
    ''' The values of one INSERT row: <c>Nothing</c> in the list for SQL NULL, the text for a string (escapes undone) or a number. Returns <c>Nothing</c>
    ''' when the row ends before it closes.
    ''' </summary>
    Private Function ParseValues(k_line As String, k_start As Integer) As List(Of String)
        Dim k_values As New List(Of String)()
        Dim k_i As Integer = k_start
        While k_i < k_line.Length
            While k_i < k_line.Length AndAlso k_line(k_i) = " "c
                k_i += 1
            End While
            If k_i >= k_line.Length Then Return Nothing

            If k_line(k_i) = "'"c Then
                Dim k_text As New StringBuilder()
                k_i += 1
                Dim k_closed As Boolean = False
                While k_i < k_line.Length
                    Dim k_c As Char = k_line(k_i)
                    If k_c = "\"c AndAlso k_i + 1 < k_line.Length Then
                        k_i += 1
                        Select Case k_line(k_i)
                            Case "n"c : k_text.Append(ControlChars.Lf)
                            Case "r"c : k_text.Append(ControlChars.Cr)
                            Case "t"c : k_text.Append(ControlChars.Tab)
                            Case "0"c : k_text.Append(" "c)
                            Case Else : k_text.Append(k_line(k_i))
                        End Select
                    ElseIf k_c = "'"c Then
                        If k_i + 1 < k_line.Length AndAlso k_line(k_i + 1) = "'"c Then
                            k_text.Append("'"c)
                            k_i += 1
                        Else
                            k_closed = True
                            k_i += 1
                            Exit While
                        End If
                    Else
                        k_text.Append(k_c)
                    End If
                    k_i += 1
                End While
                If Not k_closed Then Return Nothing
                k_values.Add(k_text.ToString())
            Else
                Dim k_end As Integer = k_i
                While k_end < k_line.Length AndAlso k_line(k_end) <> ","c AndAlso k_line(k_end) <> ")"c
                    k_end += 1
                End While
                Dim k_token As String = k_line.Substring(k_i, k_end - k_i).Trim()
                k_values.Add(If(String.Equals(k_token, "NULL", StringComparison.OrdinalIgnoreCase), Nothing, k_token))
                k_i = k_end
            End If

            While k_i < k_line.Length AndAlso k_line(k_i) = " "c
                k_i += 1
            End While
            If k_i >= k_line.Length Then Return Nothing
            If k_line(k_i) = ")"c Then Return k_values
            If k_line(k_i) <> ","c Then Return Nothing
            k_i += 1
        End While
        Return Nothing
    End Function

End Module
