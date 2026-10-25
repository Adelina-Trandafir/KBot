Imports System.Globalization
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' Stage 1, step 1 (slice AV01-05): the Access <c>PlanCont</c> becomes <c>TZ_PlanCont</c> (one row per account) and, for the
''' accounts that carry an opening balance, <c>TZ_SoldInitial</c> (the unit's year, partner NULL). The turnovers (RPD / RPC) are not migrated.
''' </summary>
''' <remarks>
''' <para>
''' Each ticked unit's accounts go into ITS OWN subunit (one Access unit = one subunit, found by <c>TZ_Subunitati.IdUnitateAccess</c>).
''' The subunits step runs just before this one, in the same transaction, and creates the subunits the units do not have yet (the chapter
''' 65.01 ... is in no Access table: it comes from the unit grid). The verification refuses a unit that has none and is not about to get one.
''' </para>
''' <para>
''' Both statements are upserts that change nothing on a repeat: an account with the same symbol keeps what the operator edited
''' in the window, a balance row that exists keeps its amounts. A second run only adds what is missing.
''' </para>
''' <para>
''' The symbol written is the COMPOSED one (base account + sector/source + classifications + analytic), the one the window and
''' the server compose; the verification lists every account where it differs from the Access key.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcPlanContStep
    Inherits AvcStep

    Private Shared ReadOnly AccountColumns As String() = {
        "IdSubunitate", "SursaSector", "Cont", "Analitic", "ClsfF", "ClsfE", "SimbolCont", "Denumire", "FelCont", "Extra", "Activ"}
    Private Shared ReadOnly SoldColumns As String() = {
        "An", "IdCont", "SoldDebitor", "SoldCreditor"}
    ''' <summary>Columns that must EXIST on <c>TZ_SoldInitial</c> although their values are not migrated: the window shows and edits the turnovers.</summary>
    Private Shared ReadOnly SoldColumnsMustExist As String() = {"RulajDebitor", "RulajCreditor"}

    Private _set As AvcPlanContSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "PlanCont"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Plan de conturi (cu solduri inițiale)"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcPlanContReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"TZ_PlanCont", "TZ_SoldInitial"}
        End Get
    End Property

    Public Overrides ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return {"Subunitati"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcPlanContReader.Read(k_context)
        AccessRows = _set.Rows.Count
        RowsToWrite = _set.Rows.Count
    End Sub

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "«PlanCont» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "Fișierele Access ale unităților bifate nu au tabelul «PlanCont»."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Fișierul Access nu are tabelul «PlanCont» la unitățile: " & String.Join(", ", _set.UnitsWithoutTable) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.PLAN_CONT_INVALID, FindingClass.Blocant, "TZ_PlanCont", String.Empty,
                                           "«PlanCont» din Access nu are coloanele: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If
            If _set.Rows.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_PlanCont", String.Empty,
                                           "«PlanCont» nu are niciun rând; nu se scrie nimic în planul de conturi."))
                Return k_findings
            End If

            ' The tables the window needs: TZ_Subunitati too, which this step reads but does not write.
            Dim k_db = k_context.TargetDatabase
            If Not AvcSql.TableExists(k_cn, k_db, "TZ_Subunitati") Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, "TZ_Subunitati", String.Empty,
                                           $"Tabelul «TZ_Subunitati» nu există în «{k_db}». Scriptele sql/AVACONT/Configurari/PlanCont/AV01_04*.sql " &
                                           "trebuie rulate pe AVACONT_SURSA și sincronizate."))
                Return k_findings
            End If
            CheckColumns(k_cn, k_db, "TZ_PlanCont", AccountColumns, "AV01_04b_plancont_sursasector.sql", k_findings)
            CheckColumns(k_cn, k_db, "TZ_SoldInitial", SoldColumns, "AV01_04_conturi_sursa.sql", k_findings)
            CheckColumns(k_cn, k_db, "TZ_SoldInitial", SoldColumnsMustExist, "AV01_04c_soldinitial_rulaje.sql", k_findings, False)
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings

            CheckColumns(k_cn, k_db, "TZ_Subunitati", AvcSubunitStore.LinkColumns, "AV01_04d_subunitati_unitate_parinte.sql", k_findings, False)
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings

            ' A unit with no subunit yet is fine when the subunits step (ticked, verified before this one) will create it in the same run.
            Dim k_map = AvcSubunitStore.UnitMap(k_cn, Nothing)
            Dim k_withoutSubunit = _set.UnitsWithRows.Where(Function(u) Not k_map.ContainsKey(u) AndAlso Not k_context.PlannedSubunits.ContainsKey(u)).OrderBy(Function(u) u).ToList()
            If k_withoutSubunit.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_LIPSA, FindingClass.Blocant, "TZ_Subunitati", String.Empty,
                                           $"În «{k_db}» nu sunt încă subunități pentru unitățile: {String.Join(", ", k_withoutSubunit.Select(Function(u) UnitLabel(k_context, u)))}. " &
                                           "Planul de conturi al fiecărei unități intră în subunitatea ei. Bifați pasul «Subunități» (le creează în aceeași migrare) și verificați din nou."))
                Return k_findings
            End If

            Dim k_lookups As AvcPlanContLookups
            Try
                k_lookups = AvcPlanContLookups.Load(k_cn, Nothing, k_context.CommonDatabase)
            Catch ex As Exception
                GlobalErrorLog.Write("AvcPlanContStep.Verify", ex)
                k_findings.Add(New Finding(AvcKinds.PLAN_CONT_INVALID, FindingClass.Blocant, "TZ_PlanCont", String.Empty,
                                           $"Nomenclatoarele din «{k_context.CommonDatabase}» (DefaCont, DefaClsfF, DefaClsfE, DefaSursaSector) nu au putut fi citite: {ex.Message}"))
                Return k_findings
            End Try

            For Each k_issue In _set.Validate(k_lookups)
                k_findings.Add(New Finding(AvcKinds.PLAN_CONT_INVALID, k_issue.Severity, "TZ_PlanCont", k_issue.Column,
                                           k_issue.Message, k_issue.Count).WithDetail(k_issue.Detail))
            Next
            RowsToWrite = _set.ActiveRows().Count
            If _set.UsageUnknownUnits.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.PLAN_CONT_INVALID, FindingClass.Informativ, "TZ_PlanCont", String.Empty,
                                           "Folosirea conturilor în «Debit», «Credit» și «ParteneriSI» nu a putut fi citită la unitățile: " &
                                           String.Join(", ", _set.UsageUnknownUnits.Select(Function(u) UnitLabel(k_context, u))) &
                                           ". Conturile lor nu se consideră nefolosite, deci cele cu valori refuzate de nomenclatoare rămân blocaje."))
            End If

            For Each k_unitId In _set.UnitsWithRows.OrderBy(Function(u) u)
                Dim k_unitRows = _set.ActiveRows().Where(Function(r) r.IdUnitate = k_unitId).ToList()
                If k_unitRows.Count = 0 Then Continue For
                If Not k_map.ContainsKey(k_unitId) Then
                    ' Its subunit does not exist yet: the subunits step creates it just before this step writes.
                    Dim k_planned = k_context.PlannedSubunits(k_unitId)
                    k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_PlanCont", String.Empty,
                                               $"Unitatea {UnitLabel(k_context, k_unitId)}: {k_unitRows.Count} conturi ▸ «TZ_PlanCont» (subunitatea nouă «{k_planned.Denumire}», capitolul {k_planned.Capitol}) și " &
                                               $"{k_unitRows.Where(Function(r) r.HasAmounts).Count()} rânduri cu sold inițial ▸ «TZ_SoldInitial» (anul {k_context.Year}).",
                                               k_unitRows.Count))
                    Continue For
                End If
                Dim k_sub = k_map(k_unitId)
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_PlanCont", String.Empty,
                                           $"Unitatea {UnitLabel(k_context, k_unitId)}: {k_unitRows.Count} conturi ▸ «TZ_PlanCont» (subunitatea «{k_sub.Denumire}», capitolul {k_sub.Capitol}) și " &
                                           $"{k_unitRows.Where(Function(r) r.HasAmounts).Count()} rânduri cu sold inițial ▸ «TZ_SoldInitial» (anul {k_context.Year}).",
                                           k_unitRows.Count))

                Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_PlanCont` WHERE `IdSubunitate` = @sub", k_cn)
                    k_cmd.Parameters.AddWithValue("@sub", k_sub.IdSubunitate)
                    Dim k_existing As Long = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                    If k_existing > 0 Then
                        k_findings.Add(New Finding(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, "TZ_PlanCont", "IdSubunitate",
                                                   $"Subunitatea «{k_sub.Denumire}» are deja {k_existing} conturi. Un cont cu același simbol rămâne neschimbat " &
                                                   "(ce ați editat în fereastră nu se suprascrie); se adaugă doar cele care lipsesc.",
                                                   CInt(Math.Min(k_existing, Integer.MaxValue))))
                    End If
                End Using
            Next
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlanContStep.Verify", ex)
            Throw
        End Try
    End Function

    Private Shared Function UnitLabel(k_context As AvcContext, k_idUnitate As Integer) As String
        Dim k_unit = k_context.Units.FirstOrDefault(Function(u) u.IdUnitate = k_idUnitate)
        Return If(k_unit Is Nothing, k_idUnitate.ToString(CultureInfo.InvariantCulture), $"{k_idUnitate} ({k_unit.NumeUnitate})")
    End Function

    ''' <summary>
    ''' The columns in <paramref name="k_columns"/> must exist; with <paramref name="k_isWritten"/> no other column may be NOT NULL
    ''' without a default either (MariaDB would answer 1364). <paramref name="k_script"/> names the script that creates them.
    ''' </summary>
    Private Shared Sub CheckColumns(k_cn As MySqlConnection, k_db As String, k_table As String,
                                    k_columns As String(), k_script As String, k_into As List(Of Finding),
                                    Optional k_isWritten As Boolean = True)
        For Each k_column In AvcSql.MissingColumns(k_cn, k_db, k_table, k_columns)
            k_into.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, k_table, k_column,
                                   $"Coloana «{k_column}» nu există în «{k_table}» din «{k_db}». Rulați sql/AVACONT/Configurari/PlanCont/{k_script} " &
                                   "pe AVACONT_SURSA, apoi «Sincronizare schemă» pe baza unității."))
        Next
        If Not k_isWritten Then Return
        Dim k_names As New HashSet(Of String)(k_columns.Concat(SoldColumnsMustExist), StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New MySqlCommand(
            "SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = @db AND TABLE_NAME = @t " &
            "AND IS_NULLABLE = 'NO' AND COLUMN_DEFAULT IS NULL AND EXTRA NOT LIKE '%auto_increment%' AND EXTRA NOT LIKE '%GENERATED%'", k_cn)
            k_cmd.Parameters.AddWithValue("@db", k_db)
            k_cmd.Parameters.AddWithValue("@t", k_table)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_name As String = k_reader.GetString(0)
                    If k_names.Contains(k_name) Then Continue While
                    k_into.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, k_table, k_name,
                                           $"«{k_name}» este NOT NULL, fără implicit, dar nu se află în lista de coloane scrise. MariaDB ar răspunde 1364."))
                End While
            End Using
        End Using
    End Sub

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            Dim k_accountsWritten As Integer = 0
            Dim k_soldWritten As Integer = 0
            If _set Is Nothing Then Throw New InvalidOperationException("«PlanCont» nu a fost citit din Access.")
            If _set.Rows.Count = 0 Then
                k_progress.Report("«TZ_PlanCont»: «PlanCont» nu are rânduri, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("TZ_PlanCont", 0), New AvcWritten("TZ_SoldInitial", 0)}
            End If

            Dim k_map = AvcSubunitStore.UnitMap(k_cn, k_transaction)
            For Each k_unitId In _set.UnitsWithRows.OrderBy(Function(u) u)
                If Not k_map.ContainsKey(k_unitId) Then
                    Throw New InvalidOperationException($"Unitatea {UnitLabel(k_context, k_unitId)} nu are subunitate; planul ei de conturi nu are unde intra.")
                End If
            Next
            Dim k_lookups = AvcPlanContLookups.Load(k_cn, k_transaction, k_context.CommonDatabase)
            ' Sets aside again the unused accounts the nomenclators refuse (the verification already named them).
            _set.Validate(k_lookups)
            Dim k_active = _set.ActiveRows()
            If k_active.Count < _set.Rows.Count Then
                k_progress.Report($"«TZ_PlanCont»: {_set.Rows.Count - k_active.Count} conturi nefolosite și fără sold, refuzate de nomenclatoare, nu se migrează.")
            End If
            For Each k_unitId In _set.UnitsWithRows.OrderBy(Function(u) u)
                Dim k_sub = k_map(k_unitId)
                k_progress.Report($"«TZ_PlanCont»: unitatea {UnitLabel(k_context, k_unitId)}: {k_active.Where(Function(r) r.IdUnitate = k_unitId).Count()} conturi în subunitatea «{k_sub.Denumire}» (capitolul {k_sub.Capitol}).")
            Next
            k_dump.WriteComment("TZ_PlanCont", "Fostul «PlanCont»: un rând Access = un cont complet; simbolul scris e cel compus.")

            Dim k_accountSql = $"INSERT INTO {AvcSql.Quote("TZ_PlanCont")} ({String.Join(", ", AccountColumns.Select(AddressOf AvcSql.Quote))}) " &
                               $"VALUES ({String.Join(", ", AccountColumns.Select(Function(c) "@" & c))}) " &
                               "ON DUPLICATE KEY UPDATE `IdCont` = LAST_INSERT_ID(`IdCont`)"
            Dim k_soldSql = $"INSERT INTO {AvcSql.Quote("TZ_SoldInitial")} ({String.Join(", ", SoldColumns.Select(AddressOf AvcSql.Quote))}) " &
                            $"VALUES ({String.Join(", ", SoldColumns.Select(Function(c) "@" & c))}) " &
                            "ON DUPLICATE KEY UPDATE `IdSold` = LAST_INSERT_ID(`IdSold`)"
            Dim k_lookupSql = "SELECT `IdCont` FROM `TZ_PlanCont` WHERE `IdSubunitate` = @IdSubunitate AND `SimbolCont` = @SimbolCont"

            Using k_accountCmd As New MySqlCommand(k_accountSql, k_cn, k_transaction)
                Using k_soldCmd As New MySqlCommand(k_soldSql, k_cn, k_transaction)
                    Using k_lookupCmd As New MySqlCommand(k_lookupSql, k_cn, k_transaction)
                        For Each k_column In AccountColumns
                            k_accountCmd.Parameters.Add(New MySqlParameter("@" & k_column, Nothing))
                        Next
                        For Each k_column In SoldColumns
                            k_soldCmd.Parameters.Add(New MySqlParameter("@" & k_column, Nothing))
                        Next
                        k_lookupCmd.Parameters.Add("@IdSubunitate", MySqlDbType.Int32)
                        k_lookupCmd.Parameters.Add("@SimbolCont", MySqlDbType.VarChar)

                        For Each k_row In k_active.OrderBy(Function(r) r.ComposedSymbol(), StringComparer.Ordinal)
                            k_cancel.ThrowIfCancellationRequested()

                            Dim k_symbol As String = k_row.ComposedSymbol()
                            Dim k_idSubunitate As Integer = k_map(k_row.IdUnitate).IdSubunitate
                            Dim k_accountValues As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                                {"IdSubunitate", k_idSubunitate},
                                {"SursaSector", k_row.SursaSector},
                                {"Cont", k_row.Cont.Value},
                                {"Analitic", If(k_row.Analitic.Length = 0, CObj(DBNull.Value), k_row.Analitic)},
                                {"ClsfF", If(k_row.ClsfF.Length = 0, CObj(DBNull.Value), k_row.ClsfF)},
                                {"ClsfE", If(k_row.ClsfE.Length = 0, CObj(DBNull.Value), k_row.ClsfE)},
                                {"SimbolCont", k_symbol},
                                {"Denumire", k_row.EffectiveDenumire(k_lookups)},
                                {"FelCont", k_row.EffectiveFel(k_lookups)},
                                {"Extra", If(k_row.Extra, 1, 0)},
                                {"Activ", 1}
                            }
                            For Each k_column In AccountColumns
                                k_accountCmd.Parameters("@" & k_column).Value = k_accountValues(k_column)
                            Next
                            k_dump.WriteStatement("TZ_PlanCont", AvcSql.RenderStatement(k_accountSql, k_accountValues))
                            k_accountCmd.ExecuteNonQuery()
                            k_accountsWritten += 1

                            If k_row.HasAmounts Then
                                Dim k_idCont As Long = k_accountCmd.LastInsertedId
                                If k_idCont = 0 Then
                                    k_lookupCmd.Parameters("@IdSubunitate").Value = k_idSubunitate
                                    k_lookupCmd.Parameters("@SimbolCont").Value = k_symbol
                                    k_idCont = Convert.ToInt64(k_lookupCmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                                End If
                                ' Debit before credit, always.
                                Dim k_soldValues As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                                    {"An", k_context.Year},
                                    {"IdCont", k_idCont},
                                    {"SoldDebitor", k_row.SoldDebitor},
                                    {"SoldCreditor", k_row.SoldCreditor}
                                }
                                For Each k_column In SoldColumns
                                    k_soldCmd.Parameters("@" & k_column).Value = k_soldValues(k_column)
                                Next
                                k_dump.WriteStatement("TZ_SoldInitial", AvcSql.RenderStatement(k_soldSql, k_soldValues))
                                k_soldCmd.ExecuteNonQuery()
                                k_soldWritten += 1
                            End If

                            If k_accountsWritten Mod 100 = 0 Then
                                k_dump.FlushAll()
                                k_progress.Report($"«TZ_PlanCont»: {k_accountsWritten} din {k_active.Count} conturi pregătite (confirmate la COMMIT).")
                            End If
                        Next
                    End Using
                End Using
            End Using

            k_dump.FlushAll()
            k_progress.Report($"«TZ_PlanCont»: {k_accountsWritten} conturi, «TZ_SoldInitial»: {k_soldWritten} rânduri (confirmate la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("TZ_PlanCont", k_accountsWritten), New AvcWritten("TZ_SoldInitial", k_soldWritten)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlanContStep.Write", ex)
            Throw
        End Try
    End Function

End Class
