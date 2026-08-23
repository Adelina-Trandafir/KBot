Imports System.Globalization
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' Reads the target database's own description of itself out of
''' <c>information_schema</c>.
''' </summary>
''' <remarks>
''' The validation rules come from the TARGET, never from the Access types. MariaDB is
''' what accepts or refuses the row, so MariaDB is what gets asked. A constraint added on
''' the server changes the answer with no code change - which matters, because the
''' constraint that failed the 21.08 run did not even exist in the schema copy held at
''' the time.
'''
''' Everything here is keyed <see cref="StringComparer.OrdinalIgnoreCase"/>.
''' </remarks>
Public NotInheritable Class TargetSchema

    Private ReadOnly _columns As Dictionary(Of String, List(Of TargetColumn))
    Private ReadOnly _foreignKeys As List(Of TargetForeignKey)
    Private ReadOnly _uniqueKeys As Dictionary(Of String, List(Of TargetUniqueKey))

    Private Sub New(schemaName As String,
                    columns As Dictionary(Of String, List(Of TargetColumn)),
                    foreignKeys As List(Of TargetForeignKey),
                    uniqueKeys As Dictionary(Of String, List(Of TargetUniqueKey)))
        Me.SchemaName = schemaName
        _columns = columns
        _foreignKeys = foreignKeys
        _uniqueKeys = uniqueKeys
    End Sub

    ''' <summary>The database this description was read from.</summary>
    Public ReadOnly Property SchemaName As String

    ''' <summary>Table names present in the database.</summary>
    Public ReadOnly Property TableNames As IReadOnlyCollection(Of String)
        Get
            Return _columns.Keys
        End Get
    End Property

    ''' <summary>Reads columns, foreign keys and unique keys for one database.</summary>
    Public Shared Function Load(cn As MySqlConnection, schemaName As String) As TargetSchema
        Try
            Return New TargetSchema(
                schemaName,
                LoadColumns(cn, schemaName),
                LoadForeignKeys(cn, schemaName),
                LoadUniqueKeys(cn, schemaName))
        Catch ex As Exception
            GlobalErrorLog.Write("TargetSchema.Load", ex)
            Throw
        End Try
    End Function

    ''' <summary>True when the database carries this table.</summary>
    Public Function HasTable(tableName As String) As Boolean
        Return _columns.ContainsKey(tableName)
    End Function

    ''' <summary>Columns of one table, in ordinal order. Empty when the table is absent.</summary>
    Public Function Columns(tableName As String) As IReadOnlyList(Of TargetColumn)
        Dim list As List(Of TargetColumn) = Nothing
        If _columns.TryGetValue(tableName, list) Then Return list
        Return Array.Empty(Of TargetColumn)()
    End Function

    ''' <summary>One column, or Nothing when table or column is absent.</summary>
    Public Function Column(tableName As String, columnName As String) As TargetColumn
        Return Columns(tableName).
            FirstOrDefault(Function(c) String.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase))
    End Function

    ''' <summary>
    ''' Columns that MUST appear in the INSERT list: NOT NULL, no default, not filled by
    ''' the server.
    ''' </summary>
    Public Function RequiredColumns(tableName As String) As IReadOnlyList(Of TargetColumn)
        Return Columns(tableName).Where(Function(c) c.IsRequired).ToList()
    End Function

    ''' <summary>Columns a value can actually be written into (i.e. not GENERATED).</summary>
    Public Function WritableColumns(tableName As String) As IReadOnlyList(Of TargetColumn)
        Return Columns(tableName).Where(Function(c) c.IsWritable).ToList()
    End Function

    ''' <summary>Foreign keys whose child is this table.</summary>
    Public Function ForeignKeysOf(tableName As String) As IReadOnlyList(Of TargetForeignKey)
        Return _foreignKeys.
            Where(Function(fk) String.Equals(fk.ChildTable, tableName, StringComparison.OrdinalIgnoreCase)).
            ToList()
    End Function

    ''' <summary>Every foreign key in the database.</summary>
    Public ReadOnly Property ForeignKeys As IReadOnlyList(Of TargetForeignKey)
        Get
            Return _foreignKeys
        End Get
    End Property

    ''' <summary>
    ''' Foreign keys pointing at a table in ANOTHER database.
    ''' </summary>
    ''' <remarks>
    ''' These are invisible to a write-order sort - no ordering inside this database can
    ''' satisfy them - so they need their own gate before the run. On today's schema they
    ''' are the five Clasificatii constraints into AVACONT_COMUN.
    ''' </remarks>
    Public Function CrossSchemaForeignKeys() As IReadOnlyList(Of TargetForeignKey)
        Return _foreignKeys.Where(Function(fk) fk.IsCrossSchema(SchemaName)).ToList()
    End Function

    ''' <summary>Unique and primary keys of one table.</summary>
    Public Function UniqueKeysOf(tableName As String) As IReadOnlyList(Of TargetUniqueKey)
        Dim list As List(Of TargetUniqueKey) = Nothing
        If _uniqueKeys.TryGetValue(tableName, list) Then Return list
        Return Array.Empty(Of TargetUniqueKey)()
    End Function

    ''' <summary>
    ''' True when the written column list covers at least one unique key, i.e. when
    ''' <c>ON DUPLICATE KEY UPDATE</c> has something to match on.
    ''' </summary>
    Public Function CanUpsert(tableName As String, writtenColumns As IEnumerable(Of String)) As Boolean
        Dim written = writtenColumns.ToList()
        Return UniqueKeysOf(tableName).Any(Function(k) k.IsCoveredBy(written))
    End Function

    Private Shared Function LoadColumns(cn As MySqlConnection, schemaName As String) _
        As Dictionary(Of String, List(Of TargetColumn))

        Dim result As New Dictionary(Of String, List(Of TargetColumn))(StringComparer.OrdinalIgnoreCase)

        Const sql As String =
            "SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, IS_NULLABLE, COLUMN_DEFAULT, " &
            "       EXTRA, DATA_TYPE, COLUMN_TYPE, CHARACTER_MAXIMUM_LENGTH " &
            "FROM information_schema.COLUMNS " &
            "WHERE TABLE_SCHEMA = @schema " &
            "ORDER BY TABLE_NAME, ORDINAL_POSITION"

        Using cmd As New MySqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@schema", schemaName)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim table = reader.GetString("TABLE_NAME")
                    Dim list As List(Of TargetColumn) = Nothing
                    If Not result.TryGetValue(table, list) Then
                        list = New List(Of TargetColumn)()
                        result(table) = list
                    End If

                    list.Add(New TargetColumn(
                        reader.GetString("COLUMN_NAME"),
                        Convert.ToInt32(reader("ORDINAL_POSITION"), CultureInfo.InvariantCulture),
                        String.Equals(reader.GetString("IS_NULLABLE"), "YES", StringComparison.OrdinalIgnoreCase),
                        NullableString(reader("COLUMN_DEFAULT")),
                        NullableString(reader("EXTRA")),
                        NullableString(reader("DATA_TYPE")),
                        NullableString(reader("COLUMN_TYPE")),
                        NullableInt64(reader("CHARACTER_MAXIMUM_LENGTH"))))
                End While
            End Using
        End Using

        Return result
    End Function

    Private Shared Function LoadForeignKeys(cn As MySqlConnection, schemaName As String) _
        As List(Of TargetForeignKey)

        Dim result As New List(Of TargetForeignKey)()

        ' REFERENCED_TABLE_SCHEMA is selected deliberately: a parent in another database
        ' is a real case here and must not be silently read as a local one.
        Const sql As String =
            "SELECT CONSTRAINT_NAME, TABLE_NAME, COLUMN_NAME, " &
            "       REFERENCED_TABLE_SCHEMA, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME, " &
            "       ORDINAL_POSITION " &
            "FROM information_schema.KEY_COLUMN_USAGE " &
            "WHERE TABLE_SCHEMA = @schema AND REFERENCED_TABLE_NAME IS NOT NULL " &
            "ORDER BY TABLE_NAME, CONSTRAINT_NAME, ORDINAL_POSITION"

        Using cmd As New MySqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@schema", schemaName)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    result.Add(New TargetForeignKey(
                        reader.GetString("CONSTRAINT_NAME"),
                        reader.GetString("TABLE_NAME"),
                        reader.GetString("COLUMN_NAME"),
                        NullableString(reader("REFERENCED_TABLE_SCHEMA")),
                        NullableString(reader("REFERENCED_TABLE_NAME")),
                        NullableString(reader("REFERENCED_COLUMN_NAME")),
                        Convert.ToInt32(reader("ORDINAL_POSITION"), CultureInfo.InvariantCulture)))
                End While
            End Using
        End Using

        Return result
    End Function

    Private Shared Function LoadUniqueKeys(cn As MySqlConnection, schemaName As String) _
        As Dictionary(Of String, List(Of TargetUniqueKey))

        Dim byTable As New Dictionary(Of String, List(Of TargetUniqueKey))(StringComparer.OrdinalIgnoreCase)

        ' STATISTICS rather than TABLE_CONSTRAINTS: it carries the column order, which a
        ' composite key needs, and NON_UNIQUE = 0 covers PRIMARY and UNIQUE together.
        Const sql As String =
            "SELECT TABLE_NAME, INDEX_NAME, COLUMN_NAME, SEQ_IN_INDEX " &
            "FROM information_schema.STATISTICS " &
            "WHERE TABLE_SCHEMA = @schema AND NON_UNIQUE = 0 " &
            "ORDER BY TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX"

        Dim gathered As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
        Dim owners As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Using cmd As New MySqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@schema", schemaName)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    Dim table = reader.GetString("TABLE_NAME")
                    Dim index = reader.GetString("INDEX_NAME")
                    Dim key = table & "|" & index

                    Dim columns As List(Of String) = Nothing
                    If Not gathered.TryGetValue(key, columns) Then
                        columns = New List(Of String)()
                        gathered(key) = columns
                        owners(key) = table
                    End If
                    columns.Add(reader.GetString("COLUMN_NAME"))
                End While
            End Using
        End Using

        For Each pair In gathered
            Dim table = owners(pair.Key)
            Dim indexName = pair.Key.Substring(table.Length + 1)

            Dim list As List(Of TargetUniqueKey) = Nothing
            If Not byTable.TryGetValue(table, list) Then
                list = New List(Of TargetUniqueKey)()
                byTable(table) = list
            End If
            list.Add(New TargetUniqueKey(
                indexName,
                pair.Value,
                String.Equals(indexName, "PRIMARY", StringComparison.OrdinalIgnoreCase)))
        Next

        Return byTable
    End Function

    Private Shared Function NullableString(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Shared Function NullableInt64(value As Object) As Long?
        If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
        Return Convert.ToInt64(value, CultureInfo.InvariantCulture)
    End Function

End Class
