Imports System.Globalization
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' How to reach the MariaDB server. The password lives here for the duration of a run
''' and is never persisted, logged or written into a dump file.
''' </summary>
Public NotInheritable Class TargetConnection

    Public Sub New(host As String, port As UInteger, user As String, password As String)
        Me.Host = host
        Me.Port = port
        Me.User = user
        Me.Password = password
    End Sub

    Public ReadOnly Property Host As String
    Public ReadOnly Property Port As UInteger
    Public ReadOnly Property User As String
    Public ReadOnly Property Password As String

    ''' <summary>
    ''' A connection string, optionally with a default database.
    ''' </summary>
    ''' <remarks>
    ''' Built with the builder, never concatenated, so a password containing ';' is
    ''' escaped rather than terminating the string.
    ''' </remarks>
    Public Function BuildConnectionString(database As String) As String
        Dim builder As New MySqlConnectionStringBuilder() With {
            .Server = Host,
            .Port = Port,
            .UserID = User,
            .Password = Password,
            .AllowUserVariables = True,
            .UseCompression = False
        }
        If Not String.IsNullOrWhiteSpace(database) Then builder.Database = database
        Return builder.ConnectionString
    End Function

    ''' <summary>A description safe to put in a log or a dump header - no password.</summary>
    Public Function Describe() As String
        Return $"{User}@{Host}:{Port.ToString(CultureInfo.InvariantCulture)}"
    End Function

End Class

''' <summary>
''' Server-level work: connecting, listing databases, and creating one from a template.
''' </summary>
''' <remarks>
''' All database work goes through the single admin account the operator supplies.
''' </remarks>
Public NotInheritable Class TargetServer

    Private ReadOnly _connection As TargetConnection

    Public Sub New(connection As TargetConnection)
        If connection Is Nothing Then Throw New ArgumentNullException(NameOf(connection))
        _connection = connection
    End Sub

    ''' <summary>
    ''' <c>user@host:port</c> - the form safe to put in a log or a journal header.
    ''' </summary>
    ''' <remarks>The password never leaves <see cref="TargetConnection"/>.</remarks>
    Public Function Describe() As String
        Return _connection.Describe()
    End Function

    ''' <summary>Opens a connection, optionally with a default database.</summary>
    Public Function Open(database As String) As MySqlConnection
        Dim cn As MySqlConnection = Nothing
        Try
            cn = New MySqlConnection(_connection.BuildConnectionString(database))
            cn.Open()
            Return cn
        Catch ex As Exception
            If cn IsNot Nothing Then cn.Dispose()
            GlobalErrorLog.Write("TargetServer.Open", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Proves the credentials work, returning the server version.
    ''' </summary>
    Public Function TestConnection() As String
        Try
            Using cn = Open(Nothing)
                Using cmd As New MySqlCommand("SELECT VERSION()", cn)
                    Return Convert.ToString(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                End Using
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TargetServer.TestConnection", ex)
            Throw
        End Try
    End Function

    ''' <summary>Database names on the server, sorted, system schemas excluded.</summary>
    Public Function DatabaseNames() As List(Of String)
        Try
            Dim names As New List(Of String)()
            Using cn = Open(Nothing)
                Const sql As String =
                    "SELECT SCHEMA_NAME FROM information_schema.SCHEMATA " &
                    "WHERE SCHEMA_NAME NOT IN " &
                    "  ('information_schema','performance_schema','mysql','sys') " &
                    "ORDER BY SCHEMA_NAME"
                Using cmd As New MySqlCommand(sql, cn)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            names.Add(reader.GetString(0))
                        End While
                    End Using
                End Using
            End Using
            Return names
        Catch ex As Exception
            GlobalErrorLog.Write("TargetServer.DatabaseNames", ex)
            Throw
        End Try
    End Function

    ''' <summary>True when a database of this name exists.</summary>
    Public Function DatabaseExists(name As String) As Boolean
        Try
            Using cn = Open(Nothing)
                Return DatabaseExists(cn, name)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TargetServer.DatabaseExists", ex)
            Throw
        End Try
    End Function

    Private Shared Function DatabaseExists(cn As MySqlConnection, name As String) As Boolean
        Const sql As String =
            "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @name"
        Using cmd As New MySqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@name", name)
            Return Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
        End Using
    End Function

    ''' <summary>
    ''' The charset and collation a database was declared with.
    ''' </summary>
    Public Function CharacterSetOf(database As String, ByRef collation As String) As String
        Try
            Using cn = Open(Nothing)
                Return CharacterSetOf(cn, database, collation)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("TargetServer.CharacterSetOf", ex)
            Throw
        End Try
    End Function

    Private Shared Function CharacterSetOf(cn As MySqlConnection, database As String,
                                           ByRef collation As String) As String
        collation = String.Empty
        Const sql As String =
            "SELECT DEFAULT_CHARACTER_SET_NAME, DEFAULT_COLLATION_NAME " &
            "FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @name"
        Using cmd As New MySqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@name", database)
            Using reader = cmd.ExecuteReader()
                If Not reader.Read() Then Return String.Empty
                collation = reader.GetString(1)
                Return reader.GetString(0)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Copies every BASE TABLE of <paramref name="template"/> into
    ''' <paramref name="database"/> via <c>SHOW CREATE TABLE</c>, with
    ''' <c>FOREIGN_KEY_CHECKS</c> off for the duration.
    ''' </summary>
    ''' <remarks>
    ''' Creation order does not matter while the checks are off, so the tables need no
    ''' sorting here - unlike the DATA, which is written with the checks ON precisely so
    ''' that a wrong order fails loudly.
    ''' <para>
    ''' Runs on the connection the caller already checked the database over, so that no
    ''' second connection can find the database in a different state than the checks did.
    ''' The DDL that <c>SHOW CREATE TABLE</c> returns names its table WITHOUT a schema, so
    ''' each <c>CREATE TABLE</c> lands in whatever the session's default database is:
    ''' <see cref="MySqlConnection.ChangeDatabase"/> points the session at the target here
    ''' rather than trusting the caller to have opened it that way.
    ''' </para>
    ''' <para>
    ''' Both callers have already confirmed the database is in the state they need before
    ''' this runs; this method does no such checking itself.
    ''' </para>
    ''' </remarks>
    ''' <param name="progress">Called with each table name as it is created. May be Nothing.</param>
    ''' <returns>The number of tables created.</returns>
    Private Shared Function CopyStructure(cn As MySqlConnection, database As String,
                                          template As String,
                                          progress As Action(Of String)) As Integer
        Dim created = 0
        cn.ChangeDatabase(database)
        Execute(cn, "SET FOREIGN_KEY_CHECKS = 0")
        Try
            For Each table In TablesOf(cn, template)
                Dim ddl = ShowCreateTable(cn, template, table)
                Execute(cn, RetargetSchema(ddl, template, database))
                created += 1
                progress?.Invoke(table)
            Next
        Finally
            Execute(cn, "SET FOREIGN_KEY_CHECKS = 1")
        End Try
        Return created
    End Function

    ''' <summary>
    ''' Creates <paramref name="database"/> as a structural copy of
    ''' <paramref name="template"/>.
    ''' </summary>
    ''' <remarks>
    ''' Structure only - <c>SHOW CREATE TABLE</c> copies no rows. That is deliberate and
    ''' it has a consequence the caller must handle: the new database's <c>Unitati</c> is
    ''' EMPTY, and Clasificatii, Clasificatii_Buget, Parteneri and FX_ORD_TBL all carry a
    ''' foreign key into it, so nothing at all can be written until it is populated.
    '''
    ''' No hand-written DDL and no schema diff: the target is empty, there is nothing to
    ''' compare against.
    '''
    ''' CREATE DATABASE is DDL and cannot be rolled back, so this runs outside any
    ''' transaction and is reported as its own step.
    '''
    ''' One connection for the whole method: the checks and the copy must not be able to
    ''' see the server in two different states.
    ''' </remarks>
    ''' <param name="progress">Called with each table name as it is created. May be Nothing.</param>
    ''' <returns>The number of tables created.</returns>
    Public Function CreateDatabaseFrom(database As String, template As String,
                                       progress As Action(Of String)) As Integer
        Try
            Using cn = Open(Nothing)
                Dim collation As String = Nothing
                Dim charset = CharacterSetOf(cn, template, collation)
                If String.IsNullOrEmpty(charset) Then
                    Throw New InvalidOperationException(
                        $"Baza-șablon «{template}» nu există pe server, deci baza «{database}» " &
                        "nu poate fi creată după ea.")
                End If

                If DatabaseExists(cn, database) Then
                    Throw New InvalidOperationException(
                        $"Baza «{database}» există deja pe server. Crearea a fost oprită.")
                End If

                Execute(cn, $"CREATE DATABASE {Quote(database)} " &
                            $"CHARACTER SET {charset} COLLATE {collation}")

                Return CopyStructure(cn, database, template, progress)
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("TargetServer.CreateDatabaseFrom", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Builds an ALREADY-EXISTING but empty database's structure from
    ''' <paramref name="template"/>. The direct replacement for the schema_sync/SSH remedy
    ''' to <see cref="Finding.BAZA_FARA_TABELE"/>.
    ''' </summary>
    ''' <remarks>
    ''' schema_sync's own route broke once the migrator's target moved to a different server
    ''' than schema_sync's <c>DB_CONFIG</c>: it compared the DC name against the OLD server,
    ''' where it was already fully populated, and reported nothing to build - leaving the
    ''' real target, on the NEW server, still empty. This runs on the SAME connection the
    ''' migrator already has open, so there is no second server to be pointed at the wrong
    ''' place.
    ''' <para>
    ''' Refuses outright if the database is missing (use <see cref="CreateDatabaseFrom"/>
    ''' for that) or already holds any table at all - this is a one-shot remedy for a
    ''' database somebody created empty, not a diff tool, and running it twice against a
    ''' database that has moved on would be worse than doing nothing.
    ''' </para>
    ''' <para>
    ''' One connection for the whole method - the "is it empty" check and the copy that
    ''' relies on the answer run over the SAME session, so nothing can create a table in
    ''' the gap between them and have it silently survive the copy.
    ''' </para>
    ''' </remarks>
    ''' <param name="progress">Called with each table name as it is created. May be Nothing.</param>
    ''' <returns>The number of tables created.</returns>
    Public Function BuildStructureInEmptyDatabase(database As String, template As String,
                                                  progress As Action(Of String)) As Integer
        Try
            Using cn = Open(Nothing)
                Dim collation As String = Nothing
                If String.IsNullOrEmpty(CharacterSetOf(cn, template, collation)) Then
                    Throw New InvalidOperationException(
                        $"Baza-șablon «{template}» nu există pe server, deci structura bazei " &
                        $"«{database}» nu poate fi construită după ea.")
                End If

                If Not DatabaseExists(cn, database) Then
                    Throw New InvalidOperationException(
                        $"Baza «{database}» nu există pe server. Această operație construiește " &
                        "structura unei baze care EXISTĂ deja, dar e goală — pentru o bază " &
                        "lipsă cu totul, folosiți crearea din șablon.")
                End If

                Dim existing = TablesOf(cn, database)
                If existing.Count > 0 Then
                    Throw New InvalidOperationException(
                        $"Baza «{database}» are deja {existing.Count} tabel(e) — nu mai e goală. " &
                        "Refuz să rulez peste o bază care a ieșit din starea pentru care există " &
                        "această operație, ca structura să nu fie construită de două ori peste " &
                        "aceeași bază.")
                End If

                Return CopyStructure(cn, database, template, progress)
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("TargetServer.BuildStructureInEmptyDatabase", ex)
            Throw
        End Try
    End Function

    Private Shared Function TablesOf(cn As MySqlConnection, database As String) As List(Of String)
        Dim names As New List(Of String)()
        Const sql As String =
            "SELECT TABLE_NAME FROM information_schema.TABLES " &
            "WHERE TABLE_SCHEMA = @schema AND TABLE_TYPE = 'BASE TABLE' " &
            "ORDER BY TABLE_NAME"
        Using cmd As New MySqlCommand(sql, cn)
            cmd.Parameters.AddWithValue("@schema", database)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    names.Add(reader.GetString(0))
                End While
            End Using
        End Using
        Return names
    End Function

    Private Shared Function ShowCreateTable(cn As MySqlConnection, database As String,
                                            table As String) As String
        Using cmd As New MySqlCommand($"SHOW CREATE TABLE {Quote(database)}.{Quote(table)}", cn)
            Using reader = cmd.ExecuteReader()
                If Not reader.Read() Then
                    Throw New InvalidOperationException(
                        $"«SHOW CREATE TABLE» nu a întors nimic pentru «{database}.{table}».")
                End If
                ' Column 0 is the table name, column 1 the DDL.
                Return reader.GetString(1)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Rewrites any explicit reference to the template database into the new one.
    ''' </summary>
    ''' <remarks>
    ''' SHOW CREATE TABLE normally emits unqualified names, so this usually changes
    ''' nothing. It matters for a foreign key the server chose to qualify - and it must
    ''' NOT touch a genuinely cross-database reference such as AVACONT_COMUN, which is
    ''' why it matches the template's name specifically rather than any qualifier.
    ''' </remarks>
    Friend Shared Function RetargetSchema(ddl As String, template As String, database As String) As String
        If String.IsNullOrEmpty(ddl) Then Return ddl
        Return ddl.Replace(Quote(template) & ".", Quote(database) & ".")
    End Function

    ''' <summary>
    ''' Backtick-quotes an identifier, doubling any backtick inside it.
    ''' </summary>
    ''' <remarks>
    ''' A database or table name cannot be a parameter in any SQL dialect, so it is
    ''' quoted instead. Every name reaching this method comes from information_schema or
    ''' from the cai registry, never straight from a text box.
    ''' </remarks>
    Friend Shared Function Quote(identifier As String) As String
        If identifier Is Nothing Then Throw New ArgumentNullException(NameOf(identifier))
        Return "`" & identifier.Replace("`", "``") & "`"
    End Function

    Private Shared Sub Execute(cn As MySqlConnection, sql As String)
        Using cmd As New MySqlCommand(sql, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
