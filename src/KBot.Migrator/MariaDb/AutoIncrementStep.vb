Imports System.Threading
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' One of the seven primary keys that becomes AUTO_INCREMENT at the end of a migration.
''' </summary>
Public NotInheritable Class AutoIncrementTarget

    Public Sub New(table As String, keyColumn As String)
        Me.Table = table
        Me.KeyColumn = keyColumn
    End Sub

    Public ReadOnly Property Table As String
    Public ReadOnly Property KeyColumn As String

End Class

''' <summary>What the step did to one table. Reported per §3.3 of the plan.</summary>
Public NotInheritable Class AutoIncrementTableResult

    Public Sub New(table As String, keyColumn As String)
        Me.Table = table
        Me.KeyColumn = keyColumn
    End Sub

    Public ReadOnly Property Table As String
    Public ReadOnly Property KeyColumn As String

    ''' <summary>Rows in the table before the ALTER.</summary>
    Public Property RowCount As Long

    ''' <summary>MAX(key) before the ALTER. Nothing when the table is empty.</summary>
    Public Property MaxKeyBefore As Long?

    ''' <summary>AUTO_INCREMENT reported by SHOW TABLE STATUS after the ALTER.</summary>
    Public Property AutoIncrementAfter As Long?

    ''' <summary>True when the table was already AUTO_INCREMENT and nothing was run.</summary>
    Public Property AlreadyDone As Boolean

    ''' <summary>Set when the table is absent from this database.</summary>
    Public Property Missing As Boolean

    Public Function Describe() As String
        If Missing Then Return $"{Table}.{KeyColumn}: tabel absent — sărit."
        If AlreadyDone Then
            Return $"{Table}.{KeyColumn}: era deja AUTO_INCREMENT (={Format(AutoIncrementAfter)}) — nimic de făcut."
        End If
        Return $"{Table}.{KeyColumn}: {RowCount} rânduri, MAX înainte={Format(MaxKeyBefore)}, " &
               $"AUTO_INCREMENT după={Format(AutoIncrementAfter)}."
    End Function

    Private Shared Function Format(value As Long?) As String
        Return If(value.HasValue, value.Value.ToString(), "—")
    End Function

End Class

''' <summary>Everything the step did, for the operator and for the worklog.</summary>
Public NotInheritable Class AutoIncrementReport

    Public ReadOnly Property Tables As New List(Of AutoIncrementTableResult)

    ''' <summary>True only when every ALTER ran (or was already in place).</summary>
    Public Property Succeeded As Boolean

    ''' <summary>Why the step refused to run. Nothing when it did run.</summary>
    Public Property RefusedBecause As String

    Public Property [Error] As Exception

    Public Function Lines() As List(Of String)
        Dim output As New List(Of String)
        For Each table In Tables
            output.Add(table.Describe())
        Next
        Return output
    End Function

End Class

''' <summary>
''' The LAST thing that happens to a unit database: turns the seven migrated primary keys
''' into AUTO_INCREMENT. See docs/PLAN_ForexeIngest.md §3.
'''
''' WHY THE KEYS ARE NOT AUTO_INCREMENT ALREADY, and why this must stay a separate,
''' final step rather than part of the reference schema:
'''
''' A new unit database is created from AVACONT_SURSA and migrated afterwards. The
''' migration writes the Access ids VERBATIM. On a plain INT NOT NULL column, a row
''' arriving with a missing, NULL or zero id RAISES and the run stops. On an
''' AUTO_INCREMENT column the very same row is accepted and MariaDB silently invents a
''' key — a fabricated id nobody asked for, in a table whose ids are referenced by five
''' other tables. That guard is the whole reason the columns are plain, and it has to
''' survive for every database that has not yet been migrated.
'''
''' So the order is always: create from AVACONT_SURSA ▸ migrate ▸ verify ▸ ALTER.
''' This class refuses to run out of that order, and refuses to touch the template.
''' </summary>
Public NotInheritable Class AutoIncrementStep

    ' The seven pairs, verified against MariaDB_Schema/000_DEMO.sql. Written out in full
    ' rather than derived from a rule: a rule such as "every primary key of an FX_ table"
    ' would silently widen the moment a table is added.
    '
    ' The SAME seven pairs are exempted from schema_sync, in
    ' PYTHON/routes/schema_sync/schema_common.py (EXEMPT_COLUMNS). If this list ever
    ' changes, that one has to change with it — they describe one decision.
    Public Shared ReadOnly Property Targets As IReadOnlyList(Of AutoIncrementTarget) =
        New List(Of AutoIncrementTarget) From {
            New AutoIncrementTarget("FX_Istoric", "ID"),
            New AutoIncrementTarget("FX_Receptii_R", "IDRR"),
            New AutoIncrementTarget("FX_Receptii_H", "IDRH"),
            New AutoIncrementTarget("FX_Receptii", "IDR"),
            New AutoIncrementTarget("FX_Receptii_RHR", "IDRHR"),
            New AutoIncrementTarget("FX_Plati", "IdPlataFX"),
            New AutoIncrementTarget("FX_Rezervari", "IDRZ")
        }

    Private ReadOnly _server As TargetServer
    Private ReadOnly _database As String
    Private ReadOnly _templateDatabase As String
    Private ReadOnly _commonDatabase As String
    Private ReadOnly _log As Action(Of String)

    Public Sub New(server As TargetServer, database As String,
                   templateDatabase As String, commonDatabase As String,
                   log As Action(Of String))
        _server = server
        _database = database
        _templateDatabase = templateDatabase
        _commonDatabase = commonDatabase
        _log = log
    End Sub

    ''' <summary>
    ''' Runs the seven ALTERs. <paramref name="transferCommitted"/> is the ONLY thing that
    ''' authorises it: a transfer that rolled back, was cancelled, or never ran leaves the
    ''' database in a state where fabricated keys are exactly the risk §3.1 describes.
    ''' </summary>
    Public Function Run(transferCommitted As Boolean, cancel As CancellationToken) As AutoIncrementReport
        Dim report As New AutoIncrementReport()

        Try
            Dim refusal = RefusalReason(transferCommitted)
            If refusal IsNot Nothing Then
                report.RefusedBecause = refusal
                Say("Pasul AUTO_INCREMENT NU a rulat: " & refusal)
                Return report
            End If

            Say($"Pasul final: AUTO_INCREMENT pe cele {Targets.Count} chei primare din «{_database}».")

            Using cn = _server.Open(_database)
                For Each target In Targets
                    cancel.ThrowIfCancellationRequested()
                    Dim outcome = Apply(cn, target)
                    report.Tables.Add(outcome)
                    Say("   " & outcome.Describe())
                Next
            End Using

            report.Succeeded = True
            Say("Pasul AUTO_INCREMENT s-a încheiat.")

        Catch ex As OperationCanceledException
            ' Each ALTER is its own implicit transaction in MariaDB — DDL cannot be rolled
            ' back — so a stop here leaves the tables done so far already converted. Say so
            ' plainly instead of implying the database is untouched.
            report.[Error] = ex
            Say("Pasul AUTO_INCREMENT a fost oprit. Tabelele deja convertite RĂMÂN convertite " &
                "(o instrucțiune ALTER nu se poate derula înapoi). Rulați pasul din nou: " &
                "tabelele gata sunt recunoscute și sărite.")
            Throw

        Catch ex As Exception
            GlobalErrorLog.Write("AutoIncrementStep.Run", ex)
            report.[Error] = ex
            Say("Pasul AUTO_INCREMENT a eșuat: " & ex.Message)
            Throw
        End Try

        Return report
    End Function

    ''' <summary>
    ''' Why the step must not run, or Nothing when it may. Every one of these is a
    ''' refusal §3.1 or §3.3 asks for explicitly.
    ''' </summary>
    Private Function RefusalReason(transferCommitted As Boolean) As String
        If Not transferCommitted Then
            Return "transferul nu s-a încheiat cu COMMIT. Cheile devin AUTO_INCREMENT " &
                   "doar după un transfer încheiat și verificat."
        End If

        If String.IsNullOrWhiteSpace(_database) Then
            Return "nu s-a indicat nicio bază de date."
        End If

        ' The reference schema NEVER gets this change (§3.1). Compared case-insensitively:
        ' MariaDB folds database names on a server with lower_case_table_names=1, so an
        ' exact match would let «avacont_sursa» through the guard that exists to stop it.
        If String.Equals(_database, _templateDatabase, StringComparison.OrdinalIgnoreCase) Then
            Return $"«{_database}» este schema de referință. Ea rămâne cu chei INT simple, " &
                   "ca migrările viitoare să respingă rândurile fără id în loc să le inventeze unul."
        End If

        If String.Equals(_database, _commonDatabase, StringComparison.OrdinalIgnoreCase) Then
            Return $"«{_database}» este baza comună, nu o bază de unitate."
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' One table: measure, ALTER, measure again. Skips a table that is already
    ''' AUTO_INCREMENT so the step is safe to re-run after an interruption.
    ''' </summary>
    Private Function Apply(cn As MySqlConnection, target As AutoIncrementTarget) As AutoIncrementTableResult
        Dim outcome As New AutoIncrementTableResult(target.Table, target.KeyColumn)

        If Not TableExists(cn, target.Table) Then
            outcome.Missing = True
            Return outcome
        End If

        Dim columnType = KeyColumnType(cn, target)
        If columnType Is Nothing Then
            Throw New InvalidOperationException(
                $"Coloana {target.Table}.{target.KeyColumn} nu există în «{_database}».")
        End If

        If columnType.IsAutoIncrement Then
            outcome.AlreadyDone = True
            outcome.AutoIncrementAfter = AutoIncrementValue(cn, target.Table)
            Return outcome
        End If

        outcome.RowCount = ScalarLong(cn, $"SELECT COUNT(*) FROM {Quote(target.Table)}").GetValueOrDefault()
        outcome.MaxKeyBefore = ScalarLong(cn,
            $"SELECT MAX({Quote(target.KeyColumn)}) FROM {Quote(target.Table)}")

        ' The column type is taken from the LIVE column rather than hard-coded to INT(11):
        ' if the reference ever widens it to BIGINT, MODIFY must not narrow it back.
        Dim sql = $"ALTER TABLE {Quote(target.Table)} " &
                  $"MODIFY {Quote(target.KeyColumn)} {columnType.ColumnType} NOT NULL AUTO_INCREMENT"
        Execute(cn, sql)

        ' §3.3: InnoDB is expected to seed the counter from MAX(key) + 1. Measured, not
        ' assumed — and reported so the first database can be checked by eye.
        outcome.AutoIncrementAfter = AutoIncrementValue(cn, target.Table)
        Return outcome
    End Function

    Private Shared Function TableExists(cn As MySqlConnection, table As String) As Boolean
        Using command = cn.CreateCommand()
            command.CommandText =
                "SELECT COUNT(*) FROM information_schema.TABLES " &
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t"
            command.Parameters.AddWithValue("@t", table)
            Return Convert.ToInt64(command.ExecuteScalar()) > 0L
        End Using
    End Function

    Private Shared Function KeyColumnType(cn As MySqlConnection,
                                          target As AutoIncrementTarget) As KeyColumnInfo
        Using command = cn.CreateCommand()
            command.CommandText =
                "SELECT COLUMN_TYPE, EXTRA FROM information_schema.COLUMNS " &
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t AND COLUMN_NAME = @c"
            command.Parameters.AddWithValue("@t", target.Table)
            command.Parameters.AddWithValue("@c", target.KeyColumn)
            Using reader = command.ExecuteReader()
                If Not reader.Read() Then Return Nothing
                Dim columnType = reader.GetString(0)
                Dim extra = If(reader.IsDBNull(1), "", reader.GetString(1))
                Return New KeyColumnInfo(columnType,
                    extra.IndexOf("auto_increment", StringComparison.OrdinalIgnoreCase) >= 0)
            End Using
        End Using
    End Function

    Private Shared Function AutoIncrementValue(cn As MySqlConnection, table As String) As Long?
        Using command = cn.CreateCommand()
            command.CommandText =
                "SELECT AUTO_INCREMENT FROM information_schema.TABLES " &
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @t"
            command.Parameters.AddWithValue("@t", table)
            Dim value = command.ExecuteScalar()
            If value Is Nothing OrElse Convert.IsDBNull(value) Then Return Nothing
            Return Convert.ToInt64(value)
        End Using
    End Function

    Private Shared Function ScalarLong(cn As MySqlConnection, sql As String) As Long?
        Using command = cn.CreateCommand()
            command.CommandText = sql
            Dim value = command.ExecuteScalar()
            If value Is Nothing OrElse Convert.IsDBNull(value) Then Return Nothing
            Return Convert.ToInt64(value)
        End Using
    End Function

    Private Shared Sub Execute(cn As MySqlConnection, sql As String)
        Using command = cn.CreateCommand()
            command.CommandText = sql
            command.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Backtick-quote an identifier, escaping embedded backticks.</summary>
    Private Shared Function Quote(identifier As String) As String
        Return "`" & identifier.Replace("`", "``") & "`"
    End Function

    Private Sub Say(message As String)
        _log?.Invoke(message)
    End Sub

    Private NotInheritable Class KeyColumnInfo
        Public Sub New(columnType As String, isAutoIncrement As Boolean)
            Me.ColumnType = columnType
            Me.IsAutoIncrement = isAutoIncrement
        End Sub

        Public ReadOnly Property ColumnType As String
        Public ReadOnly Property IsAutoIncrement As Boolean
    End Class

End Class
