Imports System.Globalization
Imports System.Text.RegularExpressions
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>Small SQL and value helpers every step uses (the KBot.Migrator ones are not public).</summary>
Friend NotInheritable Class AvcSql

    Private Sub New()
    End Sub

    Private Shared ReadOnly ParameterRegex As New Regex("@(\w+)", RegexOptions.CultureInvariant)

    ''' <summary>A backtick-quoted identifier.</summary>
    Public Shared Function Quote(k_identifier As String) As String
        Return "`" & k_identifier.Replace("`", "``") & "`"
    End Function

    ''' <summary>
    ''' The statement with its <c>@Name</c> parameters replaced by literals, for the journal file. A reconstruction, never a
    ''' transcript: the driver sends parameters, not text.
    ''' </summary>
    Public Shared Function RenderStatement(k_sql As String, k_values As IReadOnlyDictionary(Of String, Object)) As String
        Return ParameterRegex.Replace(k_sql, Function(k_match)
                                                 Dim k_value As Object = Nothing
                                                 If k_values.TryGetValue(k_match.Groups(1).Value, k_value) Then
                                                     Return ValueConverter.ToLiteral(k_value)
                                                 End If
                                                 Return k_match.Value
                                             End Function)
    End Function

    ''' <summary>The cell as trimmed text (empty for NULL / missing).</summary>
    Public Shared Function AsText(k_value As Object) As String
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(k_value, CultureInfo.InvariantCulture).Trim()
    End Function

    ''' <summary>True when <paramref name="k_table"/> exists in <paramref name="k_database"/>. Server I/O: logs and rethrows.</summary>
    Public Shared Function TableExists(k_cn As MySqlConnection, k_database As String, k_table As String) As Boolean
        Try
            Using k_cmd As New MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @db AND TABLE_NAME = @t", k_cn)
                k_cmd.Parameters.AddWithValue("@db", k_database)
                k_cmd.Parameters.AddWithValue("@t", k_table)
                Return Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSql.TableExists", ex)
            Throw
        End Try
    End Function

    ''' <summary>The names in <paramref name="k_columns"/> that <paramref name="k_table"/> does not have. Server I/O: logs and rethrows.</summary>
    Public Shared Function MissingColumns(k_cn As MySqlConnection, k_database As String, k_table As String,
                                          k_columns As IEnumerable(Of String)) As List(Of String)
        Try
            Dim k_have As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Using k_cmd As New MySqlCommand(
                "SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = @db AND TABLE_NAME = @t", k_cn)
                k_cmd.Parameters.AddWithValue("@db", k_database)
                k_cmd.Parameters.AddWithValue("@t", k_table)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        k_have.Add(k_reader.GetString(0))
                    End While
                End Using
            End Using
            Return k_columns.Where(Function(k_c) Not k_have.Contains(k_c)).ToList()
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSql.MissingColumns", ex)
            Throw
        End Try
    End Function

    ''' <summary>Rows in <paramref name="k_table"/>, or -1 when the table does not exist. Server I/O: logs and rethrows.</summary>
    Public Shared Function CountRows(k_cn As MySqlConnection, k_database As String, k_table As String) As Long
        Try
            If Not TableExists(k_cn, k_database, k_table) Then Return -1
            Using k_cmd As New MySqlCommand($"SELECT COUNT(*) FROM {Quote(k_table)}", k_cn)
                Return Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSql.CountRows", ex)
            Throw
        End Try
    End Function

End Class
