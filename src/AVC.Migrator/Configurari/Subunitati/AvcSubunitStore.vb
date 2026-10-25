Imports System.Globalization
Imports System.Text.RegularExpressions
Imports KBot.Common
Imports MySqlConnector

''' <summary>One row of <c>TZ_Subunitati</c>.</summary>
Friend NotInheritable Class AvcSubunit

    Public Property IdSubunitate As Integer
    Public Property Denumire As String = String.Empty
    ''' <summary>Budget chapter, as 65.01.</summary>
    Public Property Capitol As String = String.Empty
    ''' <summary>Sector + source, e.g. 01A: the DEFAULT of a new account of this subunit.</summary>
    Public Property SursaSector As String = String.Empty
    ''' <summary>0 or 2: salaries typed with 2 decimals (EU projects).</summary>
    Public Property Zecimale As Integer
    ''' <summary>The server keeps exactly one; the first subunit of a unit is it. Never chosen in the migrator.</summary>
    Public Property Implicita As Boolean
    Public Property Activ As Boolean = True
    ''' <summary>Accounts the subunit's chart holds (read only).</summary>
    Public Property Conturi As Integer
    ''' <summary>The IdUnitate of the Access unit this subunit comes from (cale.accdb ▸ cai); 0 = made by hand.</summary>
    Public Property IdUnitateAccess As Integer
    ''' <summary>Of a row read: the parent subunit, 0 = none (it has its own classifications and budget).</summary>
    Public Property IdSubunitateParinte As Integer
    ''' <summary>Of a row to add: the IdUnitate of the Access PARENT unit, 0 = none. Its subunit must exist or be added before this one.</summary>
    Public Property ParinteUnitateAccess As Integer

End Class

''' <summary>The subunit a ported Access unit went into.</summary>
Friend NotInheritable Class AvcSubunitRef

    Public Sub New(k_id As Integer, k_denumire As String, k_capitol As String)
        IdSubunitate = k_id
        Denumire = k_denumire
        Capitol = k_capitol
    End Sub

    Public ReadOnly Property IdSubunitate As Integer
    Public ReadOnly Property Denumire As String
    Public ReadOnly Property Capitol As String

End Class

''' <summary>
''' Reads and adds the subunits of a unit database, with the SAME rules as the «Plan de conturi» window's server route
''' (<c>PYTHON/routes/avacont/configurari/plan_cont/subunitati.py</c>): denomination up to 100 characters, chapter as 65.01,
''' a sector+source that exists in <c>DefaSursaSector</c>, 0 or 2 salary decimals, the FIRST subunit is always the default one,
''' denomination unique (the chapter may repeat, script AV01_04e). One Access unit (a <c>cai</c> row) becomes one subunit, and the chart of accounts of that unit
''' goes into it (<see cref="AvcPlanContStep"/>).
''' </summary>
Friend NotInheritable Class AvcSubunitStore

    Private Sub New()
    End Sub

    Private Shared ReadOnly CapitolRegex As New Regex("^\d{2}\.\d{2}$", RegexOptions.CultureInvariant)
    Public Const DenumireMax As Integer = 100

    ''' <summary>The columns script AV01_04d adds; the dialog and the chart step refuse to go on without them.</summary>
    Public Shared ReadOnly LinkColumns As String() = {"IdUnitateAccess", "IdSubunitateParinte"}

    ''' <summary>
    ''' The chapter proposed for a sector+source (the operator's rule: 01A = 65.01, 02A = 65.02, 02E = 65.10). A second unit of the
    ''' same source (65.11 is a 01A too) has no rule: empty, the operator types it. Always editable.
    ''' </summary>
    Public Shared Function SuggestedCapitol(k_sursaSector As String) As String
        Select Case k_sursaSector.Trim().ToUpperInvariant()
            Case "01A"
                Return "65.01"
            Case "02A"
                Return "65.02"
            Case "02E"
                Return "65.10"
            Case Else
                Return String.Empty
        End Select
    End Function

    ''' <summary>Every subunit with the size of its chart. Needs the AV01_04d columns. Server I/O: logs and rethrows.</summary>
    Public Shared Function List(k_cn As MySqlConnection, Optional k_transaction As MySqlTransaction = Nothing) As List(Of AvcSubunit)
        Try
            Dim k_result As New List(Of AvcSubunit)()
            Using k_cmd As New MySqlCommand(
                "SELECT S.IdSubunitate, S.Denumire, S.Capitol, S.SursaSector, S.ZecimaleSalarii, S.Implicita, S.Activ, " &
                "(SELECT COUNT(*) FROM TZ_PlanCont P WHERE P.IdSubunitate = S.IdSubunitate), " &
                "S.IdUnitateAccess, S.IdSubunitateParinte " &
                "FROM TZ_Subunitati S ORDER BY S.Capitol, S.IdSubunitate", k_cn, k_transaction)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        k_result.Add(New AvcSubunit() With {
                            .IdSubunitate = k_reader.GetInt32(0),
                            .Denumire = k_reader.GetString(1),
                            .Capitol = k_reader.GetString(2),
                            .SursaSector = k_reader.GetString(3),
                            .Zecimale = k_reader.GetInt32(4),
                            .Implicita = k_reader.GetInt32(5) <> 0,
                            .Activ = k_reader.GetInt32(6) <> 0,
                            .Conturi = k_reader.GetInt32(7),
                            .IdUnitateAccess = If(k_reader.IsDBNull(8), 0, k_reader.GetInt32(8)),
                            .IdSubunitateParinte = If(k_reader.IsDBNull(9), 0, k_reader.GetInt32(9))})
                    End While
                End Using
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitStore.List", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The subunit of every ported Access unit (IdUnitate ▸ subunit). <paramref name="k_transaction"/> is the open one, or Nothing.
    ''' Needs the AV01_04d columns. Server I/O: logs and rethrows.
    ''' </summary>
    Public Shared Function UnitMap(k_cn As MySqlConnection, k_transaction As MySqlTransaction) As Dictionary(Of Integer, AvcSubunitRef)
        Try
            Dim k_result As New Dictionary(Of Integer, AvcSubunitRef)()
            Using k_cmd As New MySqlCommand(
                "SELECT `IdUnitateAccess`, `IdSubunitate`, `Denumire`, `Capitol` FROM `TZ_Subunitati` WHERE `IdUnitateAccess` IS NOT NULL", k_cn, k_transaction)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        k_result(k_reader.GetInt32(0)) = New AvcSubunitRef(k_reader.GetInt32(1), k_reader.GetString(2), k_reader.GetString(3))
                    End While
                End Using
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitStore.UnitMap", ex)
            Throw
        End Try
    End Function

    ''' <summary>The sector+source codes of the common database (01A, 02A, 02E ...). Server I/O: logs and rethrows.</summary>
    Public Shared Function SectorSources(k_cn As MySqlConnection, k_commonDatabase As String, Optional k_transaction As MySqlTransaction = Nothing) As List(Of String)
        Try
            Dim k_result As New List(Of String)()
            Using k_cmd As New MySqlCommand(
                $"SELECT `SursaSector` FROM {AvcSql.Quote(k_commonDatabase)}.`DefaSursaSector` ORDER BY `SursaSector`", k_cn, k_transaction)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        If Not k_reader.IsDBNull(0) Then k_result.Add(k_reader.GetString(0))
                    End While
                End Using
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitStore.SectorSources", ex)
            Throw
        End Try
    End Function

    ''' <summary>The Romanian reason a subunit is refused, or Nothing when it is valid.</summary>
    Public Shared Function Validate(k_subunit As AvcSubunit) As String
        If k_subunit.Denumire.Trim().Length = 0 Then Return "Introduceți denumirea subunității."
        If k_subunit.Denumire.Trim().Length > DenumireMax Then Return $"Denumirea subunității are cel mult {DenumireMax} de caractere."
        If Not CapitolRegex.IsMatch(k_subunit.Capitol.Trim()) Then Return "Capitolul trebuie să aibă forma 65.01 (două cifre, punct, două cifre)."
        If k_subunit.SursaSector.Trim().Length = 0 Then Return "Unitatea nu are sursă în registru (coloana SURSA)."
        If k_subunit.Zecimale <> 0 AndAlso k_subunit.Zecimale <> 2 Then Return "Zecimalele salariilor pot fi 0 sau 2."
        Return Nothing
    End Function

    ''' <summary>
    ''' Adds the subunits in ONE transaction (all or none) and returns how many. The list is in dependency order: a parent unit's
    ''' subunit exists already or comes earlier in the list. The first subunit of the unit database is the default one. Throws
    ''' <see cref="InvalidOperationException"/> with a Romanian reason when one is refused; other server errors are logged and rethrown.
    ''' </summary>
    Public Shared Function AddMany(k_cn As MySqlConnection, k_commonDatabase As String, k_subunits As IReadOnlyList(Of AvcSubunit)) As Integer
        Try
            ' Read before the transaction opens: a command on this connection must carry the active transaction.
            Dim k_sectors As New HashSet(Of String)(SectorSources(k_cn, k_commonDatabase), StringComparer.OrdinalIgnoreCase)
            Using k_tx = k_cn.BeginTransaction()
                Try
                    Dim k_added = AddInTransaction(k_cn, k_tx, k_commonDatabase, k_subunits, k_sectors, Nothing)
                    k_tx.Commit()
                    Return k_added
                Catch
                    k_tx.Rollback()
                    Throw
                End Try
            End Using
        Catch ex As InvalidOperationException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitStore.AddMany", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' The inserts of <see cref="AddMany"/> inside a transaction the CALLER owns (it commits or rolls back): the migration run adds the
    ''' subunits as its first step, in the same transaction as the chart of accounts. Same rules and the same Romanian
    ''' <see cref="InvalidOperationException"/> reasons. Server I/O: logs and rethrows.
    ''' </summary>
    ''' <param name="k_sectors">The sector+source codes of the common database (read by the caller, with the transaction when one is open).</param>
    ''' <param name="k_journal">Called with each INSERT and its values for the SQL journal; Nothing = no journal.</param>
    Public Shared Function AddInTransaction(k_cn As MySqlConnection, k_tx As MySqlTransaction, k_commonDatabase As String,
                                            k_subunits As IReadOnlyList(Of AvcSubunit), k_sectors As HashSet(Of String),
                                            k_journal As Action(Of String, Dictionary(Of String, Object))) As Integer
        Dim k_current As AvcSubunit = Nothing
        Try
            For Each k_subunit In k_subunits
                Dim k_reason As String = Validate(k_subunit)
                If k_reason IsNot Nothing Then Throw New InvalidOperationException($"«{k_subunit.Denumire.Trim()}»: {k_reason}")
            Next

            Dim k_total As Long
            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_Subunitati` FOR UPDATE", k_cn, k_tx)
                k_total = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
            End Using

            Dim k_insertSql = "INSERT INTO `TZ_Subunitati` (`Denumire`, `Capitol`, `SursaSector`, `ZecimaleSalarii`, `Implicita`, `Activ`, `IdUnitateAccess`, `IdSubunitateParinte`) " &
                              "VALUES (@d, @c, @ss, @z, @i, 1, @u, @par)"
            Dim k_added As Integer = 0
            For Each k_subunit In k_subunits
                k_current = k_subunit
                Dim k_ss As String = k_subunit.SursaSector.Trim().ToUpperInvariant()
                If Not k_sectors.Contains(k_ss) Then
                    Throw New InvalidOperationException($"«{k_subunit.Denumire.Trim()}»: sectorul și sursa «{k_ss}» nu există în «{k_commonDatabase}».")
                End If

                Dim k_parentId As Object = DBNull.Value
                If k_subunit.ParinteUnitateAccess > 0 Then
                    Using k_cmd As New MySqlCommand("SELECT `IdSubunitate` FROM `TZ_Subunitati` WHERE `IdUnitateAccess` = @p", k_cn, k_tx)
                        k_cmd.Parameters.AddWithValue("@p", k_subunit.ParinteUnitateAccess)
                        Dim k_found = k_cmd.ExecuteScalar()
                        If k_found Is Nothing OrElse k_found Is DBNull.Value Then
                            Throw New InvalidOperationException(
                                $"«{k_subunit.Denumire.Trim()}»: părintele ei (unitatea {k_subunit.ParinteUnitateAccess}) nu este încă o subunitate. Bifați-l și pe el.")
                        End If
                        k_parentId = Convert.ToInt32(k_found, CultureInfo.InvariantCulture)
                    End Using
                End If

                Dim k_values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                    {"d", k_subunit.Denumire.Trim()},
                    {"c", k_subunit.Capitol.Trim()},
                    {"ss", k_ss},
                    {"z", k_subunit.Zecimale},
                    {"i", If(k_total = 0 AndAlso k_added = 0, 1, 0)},
                    {"u", If(k_subunit.IdUnitateAccess > 0, CObj(k_subunit.IdUnitateAccess), DBNull.Value)},
                    {"par", k_parentId}
                }
                Using k_cmd As New MySqlCommand(k_insertSql, k_cn, k_tx)
                    For Each k_entry In k_values
                        k_cmd.Parameters.AddWithValue("@" & k_entry.Key, k_entry.Value)
                    Next
                    k_cmd.ExecuteNonQuery()
                End Using
                k_journal?.Invoke(k_insertSql, k_values)
                k_added += 1
            Next
            Return k_added
        Catch ex As MySqlException When ex.ErrorCode = MySqlErrorCode.DuplicateKeyEntry
            Dim k_name As String = If(k_current Is Nothing, String.Empty, $"«{k_current.Denumire.Trim()}»: ")
            If ex.Message.Contains("UnitateAccess", StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException($"{k_name}unitatea este deja o subunitate.")
            End If
            If ex.Message.Contains("Capitol", StringComparison.OrdinalIgnoreCase) Then
                ' A database that still has the old unique index on the chapter: the script that drops it was not run there.
                Throw New InvalidOperationException($"{k_name}baza are încă indexul unic pe capitol. Rulați sql/AVACONT/Configurari/PlanCont/AV01_04e_subunitati_capitol_nu_unic.sql " &
                                                    "pe AVACONT_SURSA, apoi «Sincronizare schemă» pe baza unității.")
            End If
            Throw New InvalidOperationException($"{k_name}există deja o subunitate cu această denumire.")
        Catch ex As InvalidOperationException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSubunitStore.AddInTransaction", ex)
            Throw
        End Try
    End Function

End Class
