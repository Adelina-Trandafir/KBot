Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common

''' <summary>
''' One column of an Access table, as ACE reports it.
''' </summary>
Public NotInheritable Class AccessColumn

    Public Sub New(name As String, ordinal As Integer, dataType As OleDbType,
                   maxLength As Integer?, isNullable As Boolean)
        Me.Name = name
        Me.Ordinal = ordinal
        Me.DataType = dataType
        Me.MaxLength = maxLength
        Me.IsNullable = isNullable
    End Sub

    Public ReadOnly Property Name As String
    Public ReadOnly Property Ordinal As Integer
    Public ReadOnly Property DataType As OleDbType
    ''' <summary>Declared text length, or Nothing for non-text columns and Memo.</summary>
    Public ReadOnly Property MaxLength As Integer?
    Public ReadOnly Property IsNullable As Boolean

End Class

''' <summary>
''' Reads table and column metadata out of an open Access connection.
''' </summary>
''' <remarks>
''' Every name comparison here is <see cref="StringComparer.OrdinalIgnoreCase"/>.
''' Access spelling is not predictable - the same column is CodAI, CodAi and CodAI
''' across three tables, and IdClsfPY / IdClsfPy across two. See
''' MAPARE_ACCESS_MARIADB.md Rule 4.
'''
''' ACE exposes no "ForeignKeys" collection at all (verified slice 0045-01 on all three
''' files: "The requested collection (ForeignKeys) is not defined"). Access relationships
''' are visible only as index names. The write order is therefore derived from the
''' TARGET's information_schema, never from here.
''' </remarks>
Public NotInheritable Class AccessSchema

    Private Sub New()
    End Sub

    ''' <summary>User table names, sorted, excluding Access system and link tables.</summary>
    Public Shared Function TableNames(cn As OleDbConnection) As List(Of String)
        Try
            Dim names As New List(Of String)()
            Using schema = cn.GetSchema("Tables")
                For Each row As DataRow In schema.Rows
                    Dim tableType = TryCast(row("TABLE_TYPE"), String)
                    If String.Equals(tableType, "TABLE", StringComparison.OrdinalIgnoreCase) Then
                        Dim name = TryCast(row("TABLE_NAME"), String)
                        If Not String.IsNullOrEmpty(name) Then names.Add(name)
                    End If
                Next
            End Using
            names.Sort(StringComparer.OrdinalIgnoreCase)
            Return names
        Catch ex As Exception
            GlobalErrorLog.Write("AccessSchema.TableNames", ex)
            Throw
        End Try
    End Function

    ''' <summary>True when the file carries a table of this name, case-insensitively.</summary>
    Public Shared Function HasTable(cn As OleDbConnection, tableName As String) As Boolean
        Try
            Return TableNames(cn).Any(Function(n) String.Equals(n, tableName, StringComparison.OrdinalIgnoreCase))
        Catch ex As Exception
            GlobalErrorLog.Write("AccessSchema.HasTable", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The real, case-correct table name for <paramref name="wanted"/>, or Nothing.
    ''' </summary>
    ''' <remarks>
    ''' ACE will happily accept a differently-cased name in SQL, but the schema rows come
    ''' back with the file's own spelling, so anything that matches rows against a name
    ''' needs the real one.
    ''' </remarks>
    Public Shared Function ResolveTableName(cn As OleDbConnection, wanted As String) As String
        Try
            Return TableNames(cn).FirstOrDefault(
                Function(n) String.Equals(n, wanted, StringComparison.OrdinalIgnoreCase))
        Catch ex As Exception
            GlobalErrorLog.Write("AccessSchema.ResolveTableName", ex)
            Throw
        End Try
    End Function

    ''' <summary>Columns of one table, in ordinal order.</summary>
    Public Shared Function Columns(cn As OleDbConnection, tableName As String) As List(Of AccessColumn)
        Try
            Dim result As New List(Of AccessColumn)()
            Using schema = cn.GetSchema("Columns")
                For Each row As DataRow In schema.Rows
                    Dim owner = TryCast(row("TABLE_NAME"), String)
                    If Not String.Equals(owner, tableName, StringComparison.OrdinalIgnoreCase) Then Continue For

                    Dim name = TryCast(row("COLUMN_NAME"), String)
                    If String.IsNullOrEmpty(name) Then Continue For

                    result.Add(New AccessColumn(
                        name,
                        ToInt32(row("ORDINAL_POSITION"), 0),
                        CType(ToInt32(row("DATA_TYPE"), 0), OleDbType),
                        ToNullableInt32(row("CHARACTER_MAXIMUM_LENGTH")),
                        ToBoolean(row("IS_NULLABLE"), True)))
                Next
            End Using
            result.Sort(Function(a, b) a.Ordinal.CompareTo(b.Ordinal))
            Return result
        Catch ex As Exception
            GlobalErrorLog.Write("AccessSchema.Columns", ex)
            Throw
        End Try
    End Function

    ''' <summary>Row count of one table.</summary>
    ''' <remarks>
    ''' The table name is bracketed, not parameterised - a table name cannot be a
    ''' parameter in any SQL dialect. It comes from <see cref="TableNames"/>, i.e. from
    ''' the file's own catalogue, never from operator input.
    ''' </remarks>
    Public Shared Function CountRows(cn As OleDbConnection, tableName As String) As Long
        Try
            Using cmd As New OleDbCommand($"SELECT COUNT(*) FROM [{tableName}]", cn)
                Dim scalar = cmd.ExecuteScalar()
                If scalar Is Nothing OrElse scalar Is DBNull.Value Then Return 0
                Return Convert.ToInt64(scalar, CultureInfo.InvariantCulture)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AccessSchema.CountRows", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Opens a streaming reader over one table.
    ''' </summary>
    ''' <remarks>
    ''' Deliberately a reader and not a DataTable: the plan forbids loading a whole table
    ''' into memory, so the tool does not depend on today's row counts.
    ''' The caller owns the result and must dispose it - that disposes the command too,
    ''' which is why this returns a wrapper rather than a bare reader.
    ''' </remarks>
    Public Shared Function OpenReader(cn As OleDbConnection, tableName As String) As AccessTableReader
        Dim cmd As OleDbCommand = Nothing
        Try
            cmd = New OleDbCommand($"SELECT * FROM [{tableName}]", cn)
            Return New AccessTableReader(cmd, cmd.ExecuteReader())
        Catch ex As Exception
            If cmd IsNot Nothing Then cmd.Dispose()
            GlobalErrorLog.Write("AccessSchema.OpenReader", ex)
            Throw
        End Try
    End Function

    Private Shared Function ToInt32(value As Object, fallback As Integer) As Integer
        If value Is Nothing OrElse value Is DBNull.Value Then Return fallback
        Return Convert.ToInt32(value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function ToNullableInt32(value As Object) As Integer?
        If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
        Return Convert.ToInt32(value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function ToBoolean(value As Object, fallback As Boolean) As Boolean
        If value Is Nothing OrElse value Is DBNull.Value Then Return fallback
        Return Convert.ToBoolean(value, CultureInfo.InvariantCulture)
    End Function

End Class
