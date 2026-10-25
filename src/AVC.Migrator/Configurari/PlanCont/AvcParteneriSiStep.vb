Imports System.Globalization
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>One row of the Access <c>ParteneriSI</c>: the opening balance of one partner on one account (the turnovers RPC / RPD are not migrated).</summary>
Friend NotInheritable Class AvcParteneriSiRow
    Public Property IdUnitate As Integer
    ''' <summary>The Access key of the account (<c>PlanCont.SimbolCont</c>).</summary>
    Public Property SimbolAccess As String = String.Empty
    Public Property CodPartener As String = String.Empty
    Public Property SoldDebitor As Decimal
    Public Property SoldCreditor As Decimal

    ''' <summary>The symbol the account has in <c>TZ_PlanCont</c> (composed); empty = the account is not in the Access <c>PlanCont</c> of the unit.</summary>
    Public Property SimbolComus As String = String.Empty

    Public ReadOnly Property HasAmounts As Boolean
        Get
            Return SoldDebitor <> 0D OrElse SoldCreditor <> 0D
        End Get
    End Property

    Public ReadOnly Property HasNegativeAmount As Boolean
        Get
            Return SoldDebitor < 0D OrElse SoldCreditor < 0D
        End Get
    End Property
End Class

''' <summary>Everything read from the Access <c>ParteneriSI</c> of the ticked units.</summary>
Friend NotInheritable Class AvcParteneriSiSet
    Public ReadOnly Property Rows As New List(Of AvcParteneriSiRow)()
    ''' <summary>Units whose file has no <c>ParteneriSI</c> table.</summary>
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    Public ReadOnly Property UnitsWithRows As New HashSet(Of Integer)()
    Public ReadOnly Property MissingColumns As New List(Of String)()
End Class

''' <summary>Reads the <c>ParteneriSI</c> of every ticked unit's file and puts on each row the composed symbol of its account.</summary>
Friend NotInheritable Class AvcParteneriSiReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "ParteneriSI"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcParteneriSiSet
        Try
            Dim k_result As New AvcParteneriSiSet()
            ' The account of a row is named by the Access key; the target symbol is the composed one (the same one the chart step writes).
            Dim k_chart = AvcPlanContReader.Read(k_context)
            Dim k_symbols As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each k_account In k_chart.Rows
                If k_account.SimbolAccess.Length = 0 Then Continue For
                k_symbols.TryAdd(SymbolKey(k_account.IdUnitate, k_account.SimbolAccess), k_account.ComposedSymbol())
            Next

            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                ReadUnit(k_context, k_unit, k_result, k_symbols)
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriSiReader.Read", ex)
            Throw
        End Try
    End Function

    Friend Shared Function SymbolKey(k_idUnitate As Integer, k_simbolAccess As String) As String
        Return $"{k_idUnitate.ToString(CultureInfo.InvariantCulture)}|{k_simbolAccess.Trim()}"
    End Function

    Private Shared Sub ReadUnit(k_context As AvcContext, k_unit As CaiUnit, k_result As AvcParteneriSiSet, k_symbols As Dictionary(Of String, String))
        Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
            Dim k_table = AccessSchema.ResolveTableName(k_cn, AccessTable)
            If k_table Is Nothing Then
                k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                Return
            End If
            Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
                For Each k_column In {"SimbolCont", "CodPartener", "SID", "SIC"}
                    If Not k_reader.HasColumn(k_column) AndAlso Not k_result.MissingColumns.Contains(k_column) Then
                        k_result.MissingColumns.Add(k_column)
                    End If
                Next
                While k_reader.Read()
                    Dim k_simbol As String = AvcSql.AsText(k_reader.ValueOrMissing("SimbolCont")).Trim()
                    Dim k_composed As String = String.Empty
                    k_symbols.TryGetValue(SymbolKey(k_unit.IdUnitate, k_simbol), k_composed)
                    k_result.Rows.Add(New AvcParteneriSiRow() With {
                        .IdUnitate = k_unit.IdUnitate,
                        .SimbolAccess = k_simbol,
                        .CodPartener = AvcSql.AsText(k_reader.ValueOrMissing("CodPartener")).Trim(),
                        .SoldDebitor = Amount(k_reader.ValueOrMissing("SID")),
                        .SoldCreditor = Amount(k_reader.ValueOrMissing("SIC")),
                        .SimbolComus = If(k_composed, String.Empty)})
                    k_result.UnitsWithRows.Add(k_unit.IdUnitate)
                End While
            End Using
        End Using
    End Sub

    ' Currency / Double to two decimals; empty = 0. A negative one stays negative so the verification can name the row.
    Private Shared Function Amount(k_value As Object) As Decimal
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return 0D
        Try
            Return Math.Round(Convert.ToDecimal(k_value, CultureInfo.InvariantCulture), 2, MidpointRounding.AwayFromZero)
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
            GlobalErrorLog.Write("AvcParteneriSiReader.Amount", ex)
            Return 0D
        End Try
    End Function

End Class

''' <summary>
''' Stage 1, step 3 (slice AV01-08): the Access <c>ParteneriSI</c> (opening balances per partner and account) becomes the rows of
''' <c>TZ_SoldInitial</c> that have a partner (the unit's year, <c>SID</c> / <c>SIC</c> only; the turnovers are not migrated).
''' </summary>
''' <remarks>
''' <para>
''' A row needs two things that already exist in the target: the account (<c>TZ_PlanCont</c>, written by the <c>PlanCont</c> step just before, in
''' the same run) and the partner (<c>Parteneri</c>, by <c>IdUnitate</c> + <c>CodPartener</c>, written by KBot.Migrator). A row whose account or partner
''' is not there is NOT written and is listed by the verification; the other rows are not held back.
''' </para>
''' <para>
''' The write is an upsert that changes nothing on a repeat: a balance row that exists keeps its amounts (what the operator typed in the
''' «Parteneri» window is not overwritten). Rows with both amounts zero are not written.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcParteneriSiStep
    Inherits AvcStep

    Private Shared ReadOnly SoldColumns As String() = {"An", "IdCont", "IdPartener", "SoldDebitor", "SoldCreditor"}

    Private _set As AvcParteneriSiSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "ParteneriSI"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Solduri inițiale pe parteneri"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcParteneriSiReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"TZ_SoldInitial"}
        End Get
    End Property

    Public Overrides ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return {"PlanCont"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcParteneriSiReader.Read(k_context)
        AccessRows = _set.Rows.Count
        RowsToWrite = _set.Rows.Where(Function(k_r) k_r.HasAmounts).Count()
    End Sub

    ''' <summary>The target ids a row resolves to; Nothing = not found (yet).</summary>
    Private NotInheritable Class Resolved
        Public ReadOnly Property Row As AvcParteneriSiRow
        Public Property IdCont As Long?
        Public Property IdPartener As Long?
        ''' <summary>The account is not in the target yet but the chart step of this run writes it.</summary>
        Public Property AccountPlanned As Boolean

        Public Sub New(k_row As AvcParteneriSiRow)
            Row = k_row
        End Sub
    End Class

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "«ParteneriSI» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "Fișierele Access ale unităților bifate nu au tabelul «ParteneriSI»."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Fișierul Access nu are tabelul «ParteneriSI» la unitățile: " & String.Join(", ", _set.UnitsWithoutTable) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SOLD_PARTENER_INVALID, FindingClass.Blocant, "TZ_SoldInitial", String.Empty,
                                           "«ParteneriSI» din Access nu are coloanele: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If
            If _set.Rows.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_SoldInitial", String.Empty,
                                           "«ParteneriSI» nu are niciun rând; nu se scrie nimic."))
                Return k_findings
            End If

            Dim k_db = k_context.TargetDatabase
            For Each k_table In {"TZ_SoldInitial", "TZ_PlanCont", "TZ_Subunitati", "Parteneri"}
                If Not AvcSql.TableExists(k_cn, k_db, k_table) Then
                    k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, k_table, String.Empty,
                                               $"Tabelul «{k_table}» nu există în «{k_db}». Pentru TZ_* rulați sql/AVACONT/Configurari/PlanCont/AV01_04*.sql " &
                                               "pe AVACONT_SURSA și sincronizați schema; «Parteneri» vine din KBot.Migrator."))
                End If
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings
            For Each k_column In AvcSql.MissingColumns(k_cn, k_db, "TZ_SoldInitial", SoldColumns)
                k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, "TZ_SoldInitial", k_column,
                                           $"Coloana «{k_column}» nu există în «TZ_SoldInitial» din «{k_db}». Rulați sql/AVACONT/Configurari/PlanCont/AV01_04_conturi_sursa.sql " &
                                           "pe AVACONT_SURSA, apoi «Sincronizare schemă» pe baza unității."))
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings

            Dim k_resolved = Resolve(k_context, k_cn, Nothing)
            ReportProblems(k_context, k_resolved, k_findings)
            RowsToWrite = k_resolved.Where(Function(k_r) k_r.Row.HasAmounts AndAlso k_r.IdPartener.HasValue AndAlso (k_r.IdCont.HasValue OrElse k_r.AccountPlanned)).Count()

            For Each k_unitId In _set.UnitsWithRows.OrderBy(Function(u) u)
                Dim k_count = k_resolved.Where(Function(k_r) k_r.Row.IdUnitate = k_unitId AndAlso k_r.Row.HasAmounts AndAlso k_r.IdPartener.HasValue AndAlso (k_r.IdCont.HasValue OrElse k_r.AccountPlanned)).Count()
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_SoldInitial", String.Empty,
                                           $"Unitatea {UnitLabel(k_context, k_unitId)}: {k_count} solduri pe parteneri ▸ «TZ_SoldInitial» (anul {k_context.Year}).", k_count))
            Next

            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_SoldInitial` WHERE `An` = @an AND `IdPartener` IS NOT NULL", k_cn)
                k_cmd.Parameters.AddWithValue("@an", k_context.Year)
                Dim k_existing As Long = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                If k_existing > 0 Then
                    k_findings.Add(New Finding(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, "TZ_SoldInitial", "IdPartener",
                                               $"«TZ_SoldInitial» are deja {k_existing} solduri pe parteneri în anul {k_context.Year}. Un sold existent rămâne neschimbat " &
                                               "(ce ați tastat în fereastra «Parteneri» nu se suprascrie); se adaugă doar cele care lipsesc.",
                                               CInt(Math.Min(k_existing, Integer.MaxValue))))
                End If
            End Using
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriSiStep.Verify", ex)
            Throw
        End Try
    End Function

    ' Finds the target ids of every row. The account: by the subunit of the unit and the composed symbol (in the target now, or about to be written by
    ' the chart step of this run). The partner: by unit and code. Server I/O: logs and rethrows.
    Private Function Resolve(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction) As List(Of Resolved)
        Try
            Dim k_result As New List(Of Resolved)()
            Dim k_subunits = AvcSubunitStore.UnitMap(k_cn, k_transaction)
            Using k_accountCmd As New MySqlCommand("SELECT `IdCont` FROM `TZ_PlanCont` WHERE `IdSubunitate` = @sub AND `SimbolCont` = @sym", k_cn, k_transaction)
                Using k_partnerCmd As New MySqlCommand("SELECT `IdPartener` FROM `Parteneri` WHERE `IdUnitate` = @unit AND `CodPartener` = @cod", k_cn, k_transaction)
                    k_accountCmd.Parameters.Add("@sub", MySqlDbType.Int32)
                    k_accountCmd.Parameters.Add("@sym", MySqlDbType.VarChar)
                    k_partnerCmd.Parameters.Add("@unit", MySqlDbType.Int32)
                    k_partnerCmd.Parameters.Add("@cod", MySqlDbType.VarChar)
                    Dim k_accountCache As New Dictionary(Of String, Long?)(StringComparer.Ordinal)
                    Dim k_partnerCache As New Dictionary(Of String, Long?)(StringComparer.OrdinalIgnoreCase)
                    For Each k_row In _set.Rows
                        Dim k_item As New Resolved(k_row)
                        If k_row.SimbolComus.Length > 0 Then
                            Dim k_sub As AvcSubunitRef = Nothing
                            If k_subunits.TryGetValue(k_row.IdUnitate, k_sub) Then
                                Dim k_accountKey = $"{k_sub.IdSubunitate.ToString(CultureInfo.InvariantCulture)}|{k_row.SimbolComus}"
                                If Not k_accountCache.TryGetValue(k_accountKey, k_item.IdCont) Then
                                    k_accountCmd.Parameters("@sub").Value = k_sub.IdSubunitate
                                    k_accountCmd.Parameters("@sym").Value = k_row.SimbolComus
                                    Dim k_found As Object = k_accountCmd.ExecuteScalar()
                                    k_item.IdCont = If(k_found Is Nothing OrElse k_found Is DBNull.Value, CType(Nothing, Long?), Convert.ToInt64(k_found, CultureInfo.InvariantCulture))
                                    k_accountCache(k_accountKey) = k_item.IdCont
                                End If
                            End If
                            ' Not in the target yet, but in the Access chart of the unit: the chart step of this run writes it.
                            k_item.AccountPlanned = Not k_item.IdCont.HasValue AndAlso (k_context.PlannedSubunits.ContainsKey(k_row.IdUnitate) OrElse k_subunits.ContainsKey(k_row.IdUnitate))
                        End If
                        If k_row.CodPartener.Length > 0 Then
                            Dim k_partnerKey = $"{k_row.IdUnitate.ToString(CultureInfo.InvariantCulture)}|{k_row.CodPartener}"
                            If Not k_partnerCache.TryGetValue(k_partnerKey, k_item.IdPartener) Then
                                k_partnerCmd.Parameters("@unit").Value = k_row.IdUnitate
                                k_partnerCmd.Parameters("@cod").Value = k_row.CodPartener
                                Dim k_found As Object = k_partnerCmd.ExecuteScalar()
                                k_item.IdPartener = If(k_found Is Nothing OrElse k_found Is DBNull.Value, CType(Nothing, Long?), Convert.ToInt64(k_found, CultureInfo.InvariantCulture))
                                k_partnerCache(k_partnerKey) = k_item.IdPartener
                            End If
                        End If
                        k_result.Add(k_item)
                    Next
                End Using
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriSiStep.Resolve", ex)
            Throw
        End Try
    End Function

    Private Sub ReportProblems(k_context As AvcContext, k_resolved As List(Of Resolved), k_findings As List(Of Finding))
        Dim k_withAmounts = k_resolved.Where(Function(k_r) k_r.Row.HasAmounts).ToList()
        Dim k_zero As Integer = k_resolved.Count - k_withAmounts.Count
        If k_zero > 0 Then
            k_findings.Add(New Finding(AvcKinds.SOLD_PARTENER_INVALID, FindingClass.Informativ, "TZ_SoldInitial", "SoldDebitor",
                                       "rânduri cu ambele solduri zero; nu se scriu", k_zero))
        End If

        Dim k_negative = k_withAmounts.Where(Function(k_r) k_r.Row.HasNegativeAmount).Select(Function(k_r) Label(k_context, k_r.Row)).ToList()
        If k_negative.Count > 0 Then
            k_findings.Add(New Finding(AvcKinds.SOLD_PARTENER_INVALID, FindingClass.Blocant, "TZ_SoldInitial", "SoldDebitor",
                                       "solduri negative; tabelul le refuză (cel puțin zero)", k_negative.Count).WithDetail(String.Join(vbLf, k_negative)))
        End If

        Dim k_duplicates = k_withAmounts.GroupBy(Function(k_r) $"{k_r.Row.IdUnitate}|{k_r.Row.SimbolComus}|{k_r.Row.CodPartener.ToUpperInvariant()}").
            Where(Function(k_g) k_g.Count() > 1 AndAlso k_g.First().Row.SimbolComus.Length > 0).Select(Function(k_g) Label(k_context, k_g.First().Row)).ToList()
        If k_duplicates.Count > 0 Then
            k_findings.Add(New Finding(AvcKinds.SOLD_PARTENER_INVALID, FindingClass.Blocant, "TZ_SoldInitial", "IdPartener",
                                       "același partener pe același cont în două rânduri Access; ar deveni un singur sold", k_duplicates.Count).WithDetail(String.Join(vbLf, k_duplicates)))
        End If

        Dim k_noAccount = k_withAmounts.Where(Function(k_r) k_r.Row.SimbolComus.Length = 0).Select(Function(k_r) Label(k_context, k_r.Row)).ToList()
        If k_noAccount.Count > 0 Then
            k_findings.Add(New Finding(AvcKinds.CONT_LIPSA, FindingClass.Atentie, "TZ_SoldInitial", "IdCont",
                                       "solduri al căror cont nu este în «PlanCont» din Access; NU se scriu", k_noAccount.Count).WithDetail(String.Join(vbLf, k_noAccount)))
        End If
        Dim k_notInTarget = k_withAmounts.Where(Function(k_r) k_r.Row.SimbolComus.Length > 0 AndAlso Not k_r.IdCont.HasValue AndAlso Not k_r.AccountPlanned).
            Select(Function(k_r) Label(k_context, k_r.Row)).ToList()
        If k_notInTarget.Count > 0 Then
            k_findings.Add(New Finding(AvcKinds.CONT_LIPSA, FindingClass.Atentie, "TZ_SoldInitial", "IdCont",
                                       "solduri al căror cont nu este în «TZ_PlanCont» (unitatea nu are subunitate); bifați «Plan de conturi» și «Subunități». NU se scriu", k_notInTarget.Count).
                WithDetail(String.Join(vbLf, k_notInTarget)))
        End If

        Dim k_noPartner = k_withAmounts.Where(Function(k_r) Not k_r.IdPartener.HasValue).Select(Function(k_r) Label(k_context, k_r.Row)).ToList()
        If k_noPartner.Count > 0 Then
            k_findings.Add(New Finding(AvcKinds.PARTENER_LIPSA, FindingClass.Atentie, "TZ_SoldInitial", "IdPartener",
                                       "solduri al căror partener nu există în «Parteneri» (rulați întâi KBot.Migrator, pasul Parteneri); NU se scriu", k_noPartner.Count).
                WithDetail(String.Join(vbLf, k_noPartner)))
        End If
    End Sub

    Private Shared Function Label(k_context As AvcContext, k_row As AvcParteneriSiRow) As String
        Return $"unitatea {UnitLabel(k_context, k_row.IdUnitate)}: cont «{k_row.SimbolAccess}», partener «{k_row.CodPartener}»"
    End Function

    Private Shared Function UnitLabel(k_context As AvcContext, k_idUnitate As Integer) As String
        Dim k_unit = k_context.Units.FirstOrDefault(Function(u) u.IdUnitate = k_idUnitate)
        Return If(k_unit Is Nothing, k_idUnitate.ToString(CultureInfo.InvariantCulture), $"{k_idUnitate} ({k_unit.NumeUnitate})")
    End Function

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«ParteneriSI» nu a fost citit din Access.")
            If _set.Rows.Count = 0 Then
                k_progress.Report("«TZ_SoldInitial»: «ParteneriSI» nu are rânduri, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("TZ_SoldInitial", 0)}
            End If

            ' The chart step already wrote its accounts inside this transaction: they are found now.
            Dim k_resolved = Resolve(k_context, k_cn, k_transaction)
            Dim k_toWrite = k_resolved.Where(Function(k_r) k_r.Row.HasAmounts AndAlso k_r.IdCont.HasValue AndAlso k_r.IdPartener.HasValue).ToList()
            Dim k_skipped As Integer = k_resolved.Where(Function(k_r) k_r.Row.HasAmounts).Count() - k_toWrite.Count
            If k_skipped > 0 Then
                k_progress.Report($"«TZ_SoldInitial»: {k_skipped} solduri fără cont sau fără partener în țintă nu se scriu (le-a numit verificarea).")
            End If
            k_dump.WriteComment("TZ_SoldInitial", "Fostul «ParteneriSI»: sold inițial pe (cont, partener), anul unității; rulajele nu se aduc.")

            Dim k_sql = $"INSERT INTO {AvcSql.Quote("TZ_SoldInitial")} ({String.Join(", ", SoldColumns.Select(AddressOf AvcSql.Quote))}) " &
                        $"VALUES ({String.Join(", ", SoldColumns.Select(Function(c) "@" & c))}) " &
                        "ON DUPLICATE KEY UPDATE `IdSold` = LAST_INSERT_ID(`IdSold`)"
            Dim k_written As Integer = 0
            Using k_cmd As New MySqlCommand(k_sql, k_cn, k_transaction)
                For Each k_column In SoldColumns
                    k_cmd.Parameters.Add(New MySqlParameter("@" & k_column, Nothing))
                Next
                For Each k_item In k_toWrite.OrderBy(Function(r) r.Row.IdUnitate).ThenBy(Function(r) r.Row.SimbolComus, StringComparer.Ordinal).ThenBy(Function(r) r.Row.CodPartener, StringComparer.OrdinalIgnoreCase)
                    k_cancel.ThrowIfCancellationRequested()
                    ' Debit before credit, always.
                    Dim k_values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                        {"An", k_context.Year},
                        {"IdCont", k_item.IdCont.Value},
                        {"IdPartener", k_item.IdPartener.Value},
                        {"SoldDebitor", k_item.Row.SoldDebitor},
                        {"SoldCreditor", k_item.Row.SoldCreditor}
                    }
                    For Each k_column In SoldColumns
                        k_cmd.Parameters("@" & k_column).Value = k_values(k_column)
                    Next
                    k_dump.WriteStatement("TZ_SoldInitial", AvcSql.RenderStatement(k_sql, k_values))
                    k_cmd.ExecuteNonQuery()
                    k_written += 1
                    If k_written Mod 100 = 0 Then
                        k_dump.FlushAll()
                        k_progress.Report($"«TZ_SoldInitial»: {k_written} din {k_toWrite.Count} solduri pe parteneri pregătite (confirmate la COMMIT).")
                    End If
                Next
            End Using
            k_dump.FlushAll()
            k_progress.Report($"«TZ_SoldInitial»: {k_written} solduri pe parteneri (confirmate la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("TZ_SoldInitial", k_written)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcParteneriSiStep.Write", ex)
            Throw
        End Try
    End Function

End Class
