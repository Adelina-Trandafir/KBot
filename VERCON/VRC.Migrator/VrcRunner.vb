Imports System.Data.OleDb
Imports System.Diagnostics
Imports System.Globalization
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>Raised inside the runner to stop at a table boundary once a blocking finding exists. Never leaves the runner.</summary>
Friend NotInheritable Class VrcBlockedException
    Inherits Exception

    Public Sub New(k_message As String)
        MyBase.New(k_message)
    End Sub

End Class

''' <summary>
''' SLICE-VC05-03/04/05: the ONE transfer of a subunit. One MariaDB transaction holds everything (subunit, gestiuni, exercises,
''' the import marker, every row, the id map, the findings, the counters); nothing is visible until the single COMMIT. The
''' transfer is driven by <see cref="VrcTargetSchema"/>: parents first, every key remapped through the id map, values exact
''' (INT x 10^scale), the decided rules applied (N01/N02, price sign, opening stock, ProduseP only when used). Before COMMIT the
''' target is read back and compared with what was converted; any difference or blocking finding rolls everything back.
''' An unanswered COMMIT is reported as unknown, never as cancelled.
''' </summary>
Friend NotInheritable Class VrcRunner

    Private Const MapBatch As Integer = 400
    Private Const MaxBlockersShown As Integer = 200
    Private Const CheckEvery As Integer = 200

    Private ReadOnly _plan As VrcPlan
    Private ReadOnly _server As TargetServer
    Private ReadOnly _user As String
    Private ReadOnly _cancel As CancellationToken
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _result As New VrcRunResult()

    Private _cn As MySqlConnection
    Private _tx As MySqlTransaction
    Private _subunitId As Long
    Private _importId As Long
    Private _gestiuni As New List(Of GestInfo)()
    Private ReadOnly _access As New Dictionary(Of String, OleDbConnection)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _partitions As New Dictionary(Of String, VrcPartition)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _sourceIds As New Dictionary(Of VrcPlanSource, Long)()
    Private ReadOnly _map As New Dictionary(Of String, Long)(StringComparer.Ordinal)
    Private ReadOnly _pendingMap As New List(Of (SourceId As Long, TableName As String, Kind As String, SourceKey As String, TargetId As Long))()
    Private ReadOnly _groupCounters As New Dictionary(Of String, Long)(StringComparer.Ordinal)
    Private ReadOnly _partners As New Dictionary(Of String, Long)(StringComparer.Ordinal)
    Private ReadOnly _missingSuppliers As New HashSet(Of String)(StringComparer.Ordinal)
    Private ReadOnly _counts As New Dictionary(Of String, TrackedCount)(StringComparer.Ordinal)
    Private ReadOnly _lastNumber As New Dictionary(Of String, Long)(StringComparer.Ordinal)
    Private _currentSourceName As String = String.Empty

    Private NotInheritable Class GestInfo
        Public Property Name As String = String.Empty
        Public Property GestiuneType As String = String.Empty
        Public Property Id As Long
        ''' <summary>Sources of this gestiune, oldest exercise first.</summary>
        Public Property Sources As New List(Of VrcPlanSource)()
        Public Property ExerciseIds As New Dictionary(Of VrcPlanSource, Long)()
    End Class

    Private NotInheritable Class TrackedCount
        Public Property Count As VrcCount
        Public Property GestiuneId As Long
        Public Property ExerciseId As Long
        Public Property Table As VrcTargetTable
        Public Property Sums As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
    End Class

    Friend Sub New(k_server As TargetServer, k_plan As VrcPlan, k_user As String, k_cancel As CancellationToken, k_log As Action(Of String))
        _server = k_server
        _plan = k_plan
        _user = k_user
        _cancel = k_cancel
        _log = k_log
    End Sub

    Private Sub Log(k_text As String)
        _log?.Invoke(k_text)
    End Sub

    ' ====================================================================================================================
    ' The run
    ' ====================================================================================================================
    Friend Function Run() As VrcRunResult
        Dim k_clock = Stopwatch.StartNew()
        Dim k_commitSent As Boolean = False
        Try
            If Not _plan.IsComplete Then
                _result.State = VrcRunState.Blocked
                _result.Message = "Planul nu este complet: " & String.Join(" ", _plan.Blockers.Take(3))
                Return _result
            End If
            Log($"Conectare la {_server.Describe()}, baza {_plan.Dc}…")
            _cn = _server.Open(_plan.Dc)
            _tx = _cn.BeginTransaction()
            Log("Tranzacție unică deschisă; nimic nu devine vizibil înainte de COMMIT.")
            CreateContext()
            OpenSources()
            LoadPartners()
            For Each k_gest In _gestiuni
                MigrateGestiune(k_gest)
            Next
            WriteCounters()
            If _result.Blockers > 0 Then Throw New VrcBlockedException("Există constatări care blochează transferul.")
            _cancel.ThrowIfCancellationRequested()
            Log("Reconciliere înainte de COMMIT: numărători și sume, sursă față de țintă…")
            Reconcile()
            If _result.Counts.Any(Function(k_c) Not k_c.Matches) Then
                _result.Message = "Reconcilierea a găsit diferențe între sursă și țintă; transferul a fost anulat."
                Throw New VrcBlockedException(_result.Message)
            End If
            FinishImport()
            _cancel.ThrowIfCancellationRequested()
            Log("COMMIT…")
            k_commitSent = True
            _tx.Commit()
            _result.State = VrcRunState.CommitConfirmed
            _result.Message = "Transferul a fost confirmat (COMMIT). Subunitatea este închisă pentru orice alt import."
            Log(_result.Message)
        Catch ex As OperationCanceledException
            RollbackQuietly()
            _result.State = VrcRunState.RollbackConfirmed
            _result.Message = "Transferul a fost oprit înainte de COMMIT; nimic nu a fost scris."
            Log(_result.Message)
        Catch ex As VrcBlockedException
            RollbackQuietly()
            _result.State = VrcRunState.RollbackConfirmed
            If _result.Message.Length = 0 Then _result.Message = ex.Message
            Log("Anulat (rollback): " & ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcRunner.Run", ex)
            If k_commitSent Then
                _result.State = VrcRunState.ResultUnknown
                _result.Message = "COMMIT trimis, dar răspunsul nu a sosit (" & ex.Message & "). Nu repetați transferul: folosiți «Diagnostic» pentru a verifica ce a rămas pe server."
            Else
                RollbackQuietly()
                _result.State = VrcRunState.RollbackConfirmed
                _result.Message = "Transferul a eșuat și a fost anulat: " & ex.Message
            End If
            Log(_result.Message)
        Finally
            CloseSources()
            Try
                _tx?.Dispose()
            Catch ex As Exception
                GlobalErrorLog.Write("VrcRunner.Run.DisposeTx", ex)
            End Try
            _cn?.Dispose()
            k_clock.Stop()
            _result.Seconds = k_clock.Elapsed.TotalSeconds
        End Try
        Return _result
    End Function

    Private Sub RollbackQuietly()
        Try
            _tx?.Rollback()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcRunner.Rollback", ex)   ' the connection closing rolls back anyway
        End Try
    End Sub

    Private Sub CloseSources()
        For Each k_cn In _access.Values
            Try
                k_cn.Dispose()
            Catch ex As Exception
                GlobalErrorLog.Write("VrcRunner.CloseSources", ex)
            End Try
        Next
        _access.Clear()
        _partitions.Clear()
    End Sub

    ' ====================================================================================================================
    ' Context: subunit, gestiuni, exercises, the marker, the sources
    ' ====================================================================================================================
    Private Sub CreateContext()
        _subunitId = Insert("INSERT INTO VC_Subunitati (Name) VALUES (@n)", "@n", CType(_plan.SubunitName.Trim(), Object))
        _result.SubunitId = _subunitId
        _importId = Insert("INSERT INTO VC_Imports (SubunitId, ConfirmedBy, ToolVersion, PlanHash) VALUES (@s, @u, @v, @h)",
                           "@s", CType(_subunitId, Object), "@u", _user, "@v", "VRC.Migrator " & VrcPlan.RuleVersion, "@h", _plan.Hash())
        _result.ImportId = _importId
        Log($"Subunitatea «{_plan.SubunitName.Trim()}» (id {_subunitId}) și markerul de import (id {_importId}) sunt create în tranzacție.")

        For Each k_group In _plan.Sources.GroupBy(Function(k_s) k_s.GestiuneName.Trim(), StringComparer.OrdinalIgnoreCase)
            Dim k_first = k_group.First()
            Dim k_gest As New GestInfo() With {.Name = k_group.Key, .GestiuneType = k_first.GestiuneType}
            k_gest.Id = Insert("INSERT INTO VC_Gestiuni (SubunitId, Name, Type, State, Zec, Scara, Populated) VALUES (@s, @n, @t, 'IMPORTATA_IN_REVIZIE', @z, @c, 1)",
                               "@s", CType(_subunitId, Object), "@n", k_gest.Name, "@t", k_gest.GestiuneType, "@z", _plan.Zec, "@c", _plan.Scale)
            For Each k_source In k_group.OrderBy(Function(k_s) k_s.Year)
                k_gest.Sources.Add(k_source)
                k_gest.ExerciseIds(k_source) = Insert("INSERT INTO VC_Exercitii (SubunitId, GestiuneId, An) VALUES (@s, @g, @a)",
                                                      "@s", CType(_subunitId, Object), "@g", k_gest.Id, "@a", k_source.Year)
                _sourceIds(k_source) = Insert("INSERT INTO VC_ImportSources (ImportId, SubunitId, GestiuneId, SourceFile, SourceHash, SourceYear, SizeBytes) VALUES (@i, @s, @g, @f, @h, @y, @z)",
                                              "@i", CType(_importId, Object), "@s", _subunitId, "@g", k_gest.Id, "@f", k_source.Profile.FileName, "@h", k_source.Profile.HashBefore,
                                              "@y", k_source.Year, "@z", k_source.Profile.SizeBytes)
            Next
            _gestiuni.Add(k_gest)
            Log($"Gestiunea «{k_gest.Name}» ({k_gest.GestiuneType}), {k_gest.Sources.Count} sursă/surse.")
        Next
    End Sub

    Private Function Insert(k_sql As String, ParamArray k_args As Object()) As Long
        Using k_cmd As New MySqlCommand(k_sql, _cn, _tx)
            For k_i As Integer = 0 To k_args.Length - 1 Step 2
                k_cmd.Parameters.AddWithValue(Convert.ToString(k_args(k_i), CultureInfo.InvariantCulture), If(k_args(k_i + 1), DBNull.Value))
            Next
            k_cmd.ExecuteNonQuery()
            Return k_cmd.LastInsertedId
        End Using
    End Function

    Private Sub OpenSources()
        For Each k_source In _plan.Sources
            Dim k_provider As String = String.Empty
            If _access.ContainsKey(k_source.Source.Path) Then Continue For
            _access(k_source.Source.Path) = VrcAccess.OpenReadOnly(k_source.Source.Path, k_provider)
            Log($"Citire doar-citire: {k_source.Profile.FileName} ({k_provider}).")
        Next
    End Sub

    ''' <summary>KBOT partners by normalised tax code. A code shared by several partners takes the lowest id (reported once).</summary>
    Private Sub LoadPartners()
        Using k_cmd As New MySqlCommand("SELECT IdPartener, CodFiscal FROM Parteneri WHERE CodFiscal IS NOT NULL AND CodFiscal <> '' ORDER BY IdPartener", _cn, _tx)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_code As String = VrcConvert.NormalizeTaxCode(k_reader.GetString(1))
                    If k_code.Length > 0 AndAlso Not _partners.ContainsKey(k_code) Then _partners(k_code) = k_reader.GetInt64(0)
                End While
            End Using
        End Using
        Log($"{_partners.Count:N0} parteneri KBOT cu cod fiscal.")
    End Sub

    ' ====================================================================================================================
    ' One gestiune: shared tables once (merged over the years), then every exercise
    ' ====================================================================================================================
    Private Sub MigrateGestiune(k_gest As GestInfo)
        Dim k_used = UsedProducts(k_gest)
        For Each k_table In VrcTargetSchema.Tables().Where(Function(k_t) Not k_t.IsExercise)
            _cancel.ThrowIfCancellationRequested()
            MigrateShared(k_gest, k_table, k_used)
            StopIfBlocked(k_table.Name)
        Next
        For Each k_source In k_gest.Sources
            _currentSourceName = k_source.Profile.FileName
            Dim k_lineSums As New Dictionary(Of String, Dictionary(Of String, Decimal))(StringComparer.OrdinalIgnoreCase)
            For Each k_table In VrcTargetSchema.Tables().Where(Function(k_t) k_t.IsExercise)
                _cancel.ThrowIfCancellationRequested()
                MigrateExercise(k_gest, k_source, k_table, k_lineSums)
                StopIfBlocked(k_table.Name)
            Next
        Next
    End Sub

    Private Sub StopIfBlocked(k_table As String)
        Dim k_n As Integer = _result.Blockers
        If k_n > 0 Then
            _result.Message = $"{k_n:N0} constatări blochează transferul (primul tabel afectat: {k_table}). Corectați sursa sau planul."
            Throw New VrcBlockedException(_result.Message)
        End If
    End Sub

    ''' <summary>IdProdus values used by any document or OI/MF table of the gestiune, over all its sources (a ProduseP row without a use is not migrated).</summary>
    Private Function UsedProducts(k_gest As GestInfo) As HashSet(Of Long)
        Dim k_used As New HashSet(Of Long)()
        For Each k_source In k_gest.Sources
            For Each k_name In {"Intrari", "Iesiri", "IesiriRetur", "OI_Casare", "OI_Predare", "OI_Produse", "OI_Transfer"}
                For Each k_row In ReadRows(k_source, k_name, {"IdProdus"})
                    Dim k_id As Decimal? = VrcConvert.ToDecimal(k_row("IdProdus"))
                    If k_id.HasValue Then k_used.Add(CLng(Math.Round(k_id.Value)))
                Next
            Next
        Next
        Return k_used
    End Function

    ''' <summary>All rows of a table of a source (every column, or only the listed ones). A table the file does not have gives no rows.</summary>
    Private Function ReadRows(k_source As VrcPlanSource, k_table As String, Optional k_columns As IEnumerable(Of String) = Nothing) As List(Of Dictionary(Of String, Object))
        Dim k_all = ReadRawRows(k_source.Source.Path, k_source.Profile, k_table)
        Dim k_partition = PartitionOf(k_source)
        Dim k_part As String = k_source.Part
        If k_partition IsNot Nothing AndAlso k_part <> VrcPartition.Shared_ Then
            Dim k_kept As New List(Of Dictionary(Of String, Object))()
            For Each k_row In k_all
                Dim k_rowPart As String = k_partition.PartOf(k_table, k_row)
                If k_rowPart = VrcPartition.Shared_ OrElse k_rowPart = k_part Then k_kept.Add(k_row)
            Next
            Return k_kept
        End If
        Return k_all
    End Function

    ''' <summary>Every row of a table of a file with every column (a column the file lacks reads as NULL). No part filter.</summary>
    Private Function ReadRawRows(k_path As String, k_profile As VrcFileProfile, k_table As String) As List(Of Dictionary(Of String, Object))
        Dim k_rows As New List(Of Dictionary(Of String, Object))()
        If k_profile.Tables.All(Function(k_t) Not String.Equals(k_t.Name, k_table, StringComparison.OrdinalIgnoreCase)) Then Return k_rows
        Using k_cmd As New OleDbCommand($"SELECT * FROM [{k_table}]", _access(k_path))
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    Dim k_row As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
                    For k_i As Integer = 0 To k_reader.FieldCount - 1
                        k_row(k_reader.GetName(k_i)) = If(k_reader.IsDBNull(k_i), Nothing, k_reader.GetValue(k_i))
                    Next
                    For Each k_c In ExpectedColumns(k_table)
                        If Not k_row.ContainsKey(k_c) Then k_row(k_c) = Nothing   ' a column the file lacks reads as NULL (the plan already warned it is ABSENT)
                    Next
                    k_rows.Add(k_row)
                End While
            End Using
        End Using
        Return k_rows
    End Function

    ''' <summary>The Alimente / Materiale placement of a year file's rows; Nothing for a special base (OI, MF), which is not split.</summary>
    Private Function PartitionOf(k_source As VrcPlanSource) As VrcPartition
        If k_source.Source.Kind <> VrcFileKind.Year Then Return Nothing
        Dim k_known As VrcPartition = Nothing
        If _partitions.TryGetValue(k_source.Source.Path, k_known) Then Return k_known
        k_known = VrcPartition.Build(Function(k_t) ReadRawRows(k_source.Source.Path, k_source.Profile, k_t))
        _partitions(k_source.Source.Path) = k_known
        Log($"{k_source.Profile.FileName}: conturi Alimente {k_known.AccountsAlimente}, Materiale {k_known.AccountsMateriale}; NIR A/M {k_known.NirAlimente}/{k_known.NirMateriale}; bonuri A/M {k_known.BonAlimente}/{k_known.BonMateriale}.")
        Return k_known
    End Function

    ''' <summary>Every column the rules and the conversion look at, so a column missing from a file is NULL instead of a missing key.</summary>
    Private Shared Function ExpectedColumns(k_table As String) As IEnumerable(Of String)
        Dim k_names As New List(Of String) From {"NrFact", "IDF", "Cant", "Pret", "PUTVA", "PU", "Numar", "NumarBC", "NumarLA", "Valoare", "ValoareBC", "IdProdus"}
        Dim k_schema = VrcTargetSchema.Find(k_table)
        If k_schema IsNot Nothing Then
            If k_schema.PrimaryKey.Length > 0 Then k_names.Add(k_schema.PrimaryKey)
            k_names.AddRange(k_schema.Columns.Select(Function(k_c) k_c.Name))
        End If
        Return k_names
    End Function

    ' ---- shared (gestiune-level) tables ------------------------------------------------------------------------------
    Private Sub MigrateShared(k_gest As GestInfo, k_table As VrcTargetTable, k_used As HashSet(Of Long))
        Dim k_merged As New Dictionary(Of String, (Source As VrcPlanSource, Row As Dictionary(Of String, Object), Contributors As List(Of VrcPlanSource)))(StringComparer.Ordinal)
        Dim k_order As New List(Of String)()
        Dim k_total As Long = 0
        For Each k_source In k_gest.Sources
            Dim k_n As Integer = 0
            For Each k_row In ReadRows(k_source, k_table.Name)
                k_n += 1
                Dim k_key As String = If(k_table.PrimaryKey.Length > 0, VrcConvert.KeyText(k_row(k_table.PrimaryKey)), $"{k_source.Profile.FileName}#{k_n}")
                If k_key.Length = 0 Then
                    AddIssue(k_source, k_table.Name, $"#{k_n}", k_table.PrimaryKey, "X06", "BLOCAJ", "NULL", String.Empty, "Rând fără cheie primară.")
                    Continue For
                End If
                If k_merged.ContainsKey(k_key) Then
                    k_merged(k_key).Contributors.Add(k_source)
                    k_merged(k_key) = (k_source, k_row, k_merged(k_key).Contributors)   ' the later exercise wins the attributes
                Else
                    k_merged(k_key) = (k_source, k_row, New List(Of VrcPlanSource) From {k_source})
                    k_order.Add(k_key)
                End If
            Next
            k_total += k_n
        Next
        If k_order.Count = 0 Then Return

        Dim k_insert = PrepareInsert(k_table)
        Dim k_tracked = Track(k_gest, 0, k_table, k_gest.Name, 0)
        k_tracked.Count.SourceRows = k_order.Count
        Dim k_done As Integer = 0
        For Each k_key In k_order
            Dim k_entry = k_merged(k_key)
            k_done += 1
            If k_done Mod CheckEvery = 0 Then _cancel.ThrowIfCancellationRequested()
            If k_table.Name = "ProduseP" Then
                Dim k_idProduct As Decimal? = VrcConvert.ToDecimal(k_entry.Row("IdProdus"))
                If Not k_idProduct.HasValue OrElse Not k_used.Contains(CLng(Math.Round(k_idProduct.Value))) Then
                    k_tracked.Count.SkippedRows += 1
                    Continue For
                End If
            End If
            Dim k_id As Long = InsertRow(k_gest, k_entry.Source, k_table, k_insert, k_entry.Row, k_key, k_tracked, 0, Nothing)
            If k_id < 0 Then Continue For
            _map($"G{k_gest.Id}|{k_table.Name}|{k_key}") = k_id
            For Each k_contributor In k_entry.Contributors.Distinct()
                _pendingMap.Add((_sourceIds(k_contributor), k_table.Name, "ROW", k_key, k_id))
            Next
        Next
        FlushMap()
        If k_gest.Sources.Count > 1 Then Log($"{k_table.Name}: {k_total:N0} rânduri în {k_gest.Sources.Count} surse → {k_order.Count:N0} distincte (cheie identică = aceeași entitate).")
        k_insert.Dispose()
    End Sub

    ' ---- exercise tables ---------------------------------------------------------------------------------------------
    Private Sub MigrateExercise(k_gest As GestInfo, k_source As VrcPlanSource, k_table As VrcTargetTable,
                                k_lineSums As Dictionary(Of String, Dictionary(Of String, Decimal)))
        Dim k_rows = ReadRows(k_source, k_table.Name)
        If k_rows.Count = 0 Then Return
        Dim k_exercise As Long = k_gest.ExerciseIds(k_source)
        Dim k_sourceIndex As Integer = _plan.Sources.IndexOf(k_source)
        Dim k_insert = PrepareInsert(k_table)
        Dim k_tracked = Track(k_gest, k_exercise, k_table, k_gest.Name, k_source.Year)
        k_tracked.Count.SourceRows = k_rows.Count
        If k_table.Name = "NIR" Then EnsureSums(k_source, k_lineSums, "NIR", "Intrari", "IdIntrare")
        If k_table.Name = "Iesiri_H" Then EnsureSums(k_source, k_lineSums, "Iesiri_H", "Iesiri", "IdBonConsum")
        Dim k_suppliers As Dictionary(Of String, String) = If(k_table.Name = "NIR", SupplierCodes(k_source), Nothing)
        Dim k_n As Integer = 0
        For Each k_row In k_rows
            k_n += 1
            If k_n Mod CheckEvery = 0 Then _cancel.ThrowIfCancellationRequested()
            Dim k_key As String = If(k_table.PrimaryKey.Length > 0, VrcConvert.KeyText(k_row(k_table.PrimaryKey)), $"#{k_n}")
            If k_key.Length = 0 Then
                AddIssue(k_source, k_table.Name, $"#{k_n}", k_table.PrimaryKey, "X06", "BLOCAJ", "NULL", String.Empty, "Rând fără cheie primară.")
                Continue For
            End If
            Dim k_extra As New ExtraContext() With {.LineSums = k_lineSums, .Suppliers = k_suppliers}
            Dim k_id As Long = InsertRow(k_gest, k_source, k_table, k_insert, k_row, k_key, k_tracked, k_exercise, k_extra)
            If k_id < 0 Then Continue For
            _map($"S{k_sourceIndex}|{k_table.Name}|{k_key}") = k_id
            _pendingMap.Add((_sourceIds(k_source), k_table.Name, "ROW", k_key, k_id))
            If _pendingMap.Count >= MapBatch Then FlushMap()
        Next
        FlushMap()
        k_insert.Dispose()
        Log($"{k_gest.Name} {k_source.Year}: {k_table.Name} — {k_tracked.Count.SourceRows:N0} rânduri citite.")
    End Sub

    Private NotInheritable Class ExtraContext
        Public Property LineSums As Dictionary(Of String, Dictionary(Of String, Decimal))
        Public Property Suppliers As Dictionary(Of String, String)
    End Class

    ''' <summary>Sum of the line totals (with VAT, per line) for each header key: input of the N01/N02 rule.</summary>
    Private Sub EnsureSums(k_source As VrcPlanSource, k_store As Dictionary(Of String, Dictionary(Of String, Decimal)),
                           k_headerTable As String, k_lineTable As String, k_key As String)
        If k_store.ContainsKey(k_headerTable) Then Return
        Dim k_sums As New Dictionary(Of String, Decimal)(StringComparer.Ordinal)
        For Each k_row In ReadRows(k_source, k_lineTable, {k_key, "ValoareCUTVA"})
            Dim k_id As String = VrcConvert.KeyText(k_row(k_key))
            If k_id.Length = 0 Then Continue For
            Dim k_value As Decimal = If(VrcConvert.ToDecimal(k_row("ValoareCUTVA")), 0D)
            Dim k_sum As Decimal = 0D
            k_sums.TryGetValue(k_id, k_sum)
            k_sums(k_id) = k_sum + k_value
        Next
        k_store(k_headerTable) = k_sums
    End Sub

    ''' <summary>Furnizori.IDF → tax code of the source, for the partner match.</summary>
    Private Function SupplierCodes(k_source As VrcPlanSource) As Dictionary(Of String, String)
        Dim k_codes As New Dictionary(Of String, String)(StringComparer.Ordinal)
        For Each k_row In ReadRows(k_source, "Furnizori", {"IDF", "CF"})
            Dim k_id As String = VrcConvert.KeyText(k_row("IDF"))
            If k_id.Length > 0 Then k_codes(k_id) = Convert.ToString(k_row("CF"), CultureInfo.InvariantCulture)
        Next
        Return k_codes
    End Function

    ' ====================================================================================================================
    ' One row: convert → insert → remember the new key
    ' ====================================================================================================================
    Private Function PrepareInsert(k_table As VrcTargetTable) As MySqlCommand
        Dim k_names As New List(Of String) From {"SubunitId", "GestiuneId"}
        If k_table.IsExercise Then k_names.Add("ExercitiuId")
        For Each k_c In k_table.Columns
            If k_c.Link = VrcLinkKind.Skip OrElse k_c.Link = VrcLinkKind.Supplier Then Continue For
            k_names.Add(k_c.Name)
        Next
        If k_table.Name = "NIR" Then k_names.AddRange({"IdPartener", "CodFiscalSursa", "TipDoc", "NumarSub"})
        Dim k_cmd As New MySqlCommand(
            $"INSERT INTO `{k_table.TargetName}` ({String.Join(", ", k_names.Select(Function(k_n) $"`{k_n}`"))}) VALUES ({String.Join(", ", k_names.Select(Function(k_n, k_i) "@p" & k_i.ToString(CultureInfo.InvariantCulture)))})",
            _cn, _tx)
        For k_i As Integer = 0 To k_names.Count - 1
            k_cmd.Parameters.Add(New MySqlParameter() With {.ParameterName = "@p" & k_i.ToString(CultureInfo.InvariantCulture)})
        Next
        Return k_cmd
    End Function

    Private Function Track(k_gest As GestInfo, k_exercise As Long, k_table As VrcTargetTable, k_gestiuneName As String, k_year As Integer) As TrackedCount
        Dim k_id As String = $"{k_gest.Id}|{k_exercise}|{k_table.Name}"
        Dim k_tracked As TrackedCount = Nothing
        If Not _counts.TryGetValue(k_id, k_tracked) Then
            k_tracked = New TrackedCount() With {
                .Count = New VrcCount() With {.Gestiune = k_gestiuneName, .TableName = k_table.TargetName, .Year = k_year},
                .GestiuneId = k_gest.Id, .ExerciseId = k_exercise, .Table = k_table}
            _counts(k_id) = k_tracked
        End If
        Return k_tracked
    End Function

    ''' <summary>Converts and inserts one row. Returns the new id, or -1 when the row was not written (blocking finding or a rule skipped it).</summary>
    Private Function InsertRow(k_gest As GestInfo, k_source As VrcPlanSource, k_table As VrcTargetTable, k_cmd As MySqlCommand,
                               k_row As Dictionary(Of String, Object), k_key As String, k_tracked As TrackedCount,
                               k_exercise As Long, k_extra As ExtraContext) As Long
        Dim k_values As New List(Of Object) From {_subunitId, k_gest.Id}
        If k_table.IsExercise Then k_values.Add(k_exercise)
        Dim k_sourceIndex As Integer = _plan.Sources.IndexOf(k_source)
        Dim k_sums As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
        Dim k_ok As Boolean = True

        If k_table.Name = "NIR" Then _currentNumarSub = 0
        If k_table.Name = "NIR" Then _currentRowIsStock = VrcConvert.IsOpeningStock(Convert.ToString(k_row("NrFact"), CultureInfo.InvariantCulture))
        ApplyRowRules(k_gest, k_source, k_table, k_row, k_key, k_extra, k_ok)

        For Each k_column In k_table.Columns
            If k_column.Link = VrcLinkKind.Skip OrElse k_column.Link = VrcLinkKind.Supplier Then Continue For
            Dim k_raw As Object = Nothing
            k_row.TryGetValue(k_column.Name, k_raw)
            Dim k_converted As Object = ConvertColumn(k_gest, k_source, k_sourceIndex, k_table, k_column, k_raw, k_key, k_ok)
            k_values.Add(k_converted)
            If (k_column.FieldClass = "VALOARE-INT" OrElse k_column.FieldClass = "COTA") AndAlso k_converted IsNot Nothing AndAlso Not TypeOf k_converted Is DBNull Then
                k_sums(k_column.Name) = CLng(k_converted)
            End If
        Next
        If k_table.Name = "NIR" Then AddNirExtras(k_source, k_row, k_key, k_extra, k_values)
        If Not k_ok Then Return -1

        For k_i As Integer = 0 To k_values.Count - 1
            k_cmd.Parameters(k_i).Value = If(k_values(k_i), DBNull.Value)
        Next
        Try
            k_cmd.ExecuteNonQuery()
        Catch ex As MySqlException
            AddIssue(k_source, k_table.Name, k_key, String.Empty, "X07", "BLOCAJ", String.Empty, String.Empty, $"MariaDB a refuzat rândul (cod {CInt(ex.ErrorCode)}): {ex.Message}")
            Return -1
        End Try
        For Each k_pair In k_sums
            Dim k_sum As Long = 0
            k_tracked.Sums.TryGetValue(k_pair.Key, k_sum)
            k_tracked.Sums(k_pair.Key) = k_sum + k_pair.Value
        Next
        TrackNumbering(k_gest, k_exercise, k_table, k_row)
        Return k_cmd.LastInsertedId
    End Function

    ''' <summary>Rules that change the raw values of a row before the generic conversion: price sign, N01/N02, opening stock numbers.</summary>
    Private Sub ApplyRowRules(k_gest As GestInfo, k_source As VrcPlanSource, k_table As VrcTargetTable, k_row As Dictionary(Of String, Object),
                              k_key As String, k_extra As ExtraContext, ByRef k_ok As Boolean)
        Select Case k_table.Name
            Case "Intrari", "Iesiri", "IesiriRetur"
                ApplyPriceSign(k_source, k_table.Name, k_row, k_key, k_ok)
            Case "ProduseP"
                Dim k_price As Decimal? = VrcConvert.ToDecimal(k_row("PUTVA"))
                If k_price.HasValue AndAlso k_price.Value < 0D Then
                    AddIssue(k_source, k_table.Name, k_key, "PUTVA", "X01", "BLOCAJ", k_price.Value.ToString("0.####", CultureInfo.InvariantCulture), String.Empty,
                             "Preț negativ într-un rând ProduseP folosit în documente; corectați-l în sursă.")
                    k_ok = False
                End If
            Case "NIR"
                ApplyHeaderTotal(k_source, k_table.Name, k_row, k_key, "Valoare", "NIR", k_extra, k_ok)
            Case "Iesiri_H"
                ApplyHeaderTotal(k_source, k_table.Name, k_row, k_key, "ValoareBC", "Iesiri_H", k_extra, k_ok)
        End Select
    End Sub

    ''' <summary>Price made positive and quantity negative (same absolute value) on document lines; both negative is blocking.</summary>
    Private Sub ApplyPriceSign(k_source As VrcPlanSource, k_table As String, k_row As Dictionary(Of String, Object), k_key As String, ByRef k_ok As Boolean)
        Dim k_quantity As Decimal? = VrcConvert.ToDecimal(k_row("Cant"))
        Dim k_priceColumns = {"Pret", "PUTVA", "PU"}.Where(Function(k_c) k_row.ContainsKey(k_c)).ToList()
        Dim k_negative = k_priceColumns.Where(Function(k_c)
                                                  Dim k_p As Decimal? = VrcConvert.ToDecimal(k_row(k_c))
                                                  Return k_p.HasValue AndAlso k_p.Value < 0D
                                              End Function).ToList()
        If k_negative.Count = 0 Then
            If k_quantity.HasValue AndAlso k_quantity.Value < 0D Then
                AddIssue(k_source, k_table, k_key, "Cant", "X08", "AVERTIZARE", k_quantity.Value.ToString("0.####", CultureInfo.InvariantCulture),
                         k_quantity.Value.ToString("0.####", CultureInfo.InvariantCulture), "Cantitate negativă păstrată ca în sursă.")
            End If
            Return
        End If
        If k_quantity.HasValue AndAlso k_quantity.Value < 0D Then
            AddIssue(k_source, k_table, k_key, String.Join("/", k_negative), "X09", "BLOCAJ", "Cant " & k_quantity.Value.ToString("0.####", CultureInfo.InvariantCulture), String.Empty,
                     "Cantitatea și prețul sunt ambele negative; corectați linia în sursă.")
            k_ok = False
            Return
        End If
        Dim k_originalText As String = String.Join(", ", k_priceColumns.Select(Function(k_c) k_c & "=" & Convert.ToString(VrcConvert.ToDecimal(k_row(k_c)), CultureInfo.InvariantCulture))) &
                                       ", Cant=" & Convert.ToString(k_quantity, CultureInfo.InvariantCulture)
        For Each k_c In k_priceColumns
            Dim k_p As Decimal? = VrcConvert.ToDecimal(k_row(k_c))
            If k_p.HasValue Then k_row(k_c) = Math.Abs(k_p.Value)
        Next
        If k_quantity.HasValue Then k_row("Cant") = -Math.Abs(k_quantity.Value) Else k_row("Cant") = 0D
        AddIssue(k_source, k_table, k_key, String.Join("/", k_negative), "X01", "AVERTIZARE", k_originalText,
                 String.Join(", ", k_priceColumns.Select(Function(k_c) k_c & "=" & Convert.ToString(VrcConvert.ToDecimal(k_row(k_c)), CultureInfo.InvariantCulture))) & ", Cant=" & Convert.ToString(VrcConvert.ToDecimal(k_row("Cant")), CultureInfo.InvariantCulture),
                 "Preț negativ: prețul devine pozitiv, cantitatea negativă (aceeași valoare absolută).")
    End Sub

    ''' <summary>N01/N02: target total = sum of the lines when the header is 0 or differs by more than 1 leu; otherwise the header.</summary>
    Private Sub ApplyHeaderTotal(k_source As VrcPlanSource, k_table As String, k_row As Dictionary(Of String, Object), k_key As String,
                                 k_valueColumn As String, k_store As String, k_extra As ExtraContext, ByRef k_ok As Boolean)
        Dim k_header As Decimal? = VrcConvert.ToDecimal(k_row(k_valueColumn))
        Dim k_sums As Dictionary(Of String, Decimal) = Nothing
        If k_extra Is Nothing OrElse Not k_extra.LineSums.TryGetValue(k_store, k_sums) Then Return
        Dim k_sum As Decimal = 0D
        Dim k_hasLines As Boolean = k_sums.TryGetValue(k_key, k_sum)
        Dim k_text As Func(Of Decimal?, String) = Function(k_v) If(k_v.HasValue, k_v.Value.ToString("0.####", CultureInfo.InvariantCulture), "NULL")
        If Not k_hasLines Then
            AddIssue(k_source, k_table, k_key, k_valueColumn, "N03", "AVERTIZARE", k_text(k_header), k_text(k_header), "Document fără linii: totalul antetului rămâne neschimbat.")
            Return
        End If
        If Not k_header.HasValue Then
            k_row(k_valueColumn) = k_sum
            AddIssue(k_source, k_table, k_key, k_valueColumn, "N00", "AVERTIZARE", "NULL", k_text(k_sum), "Total antet lipsă (NULL): se folosește suma liniilor.")
        ElseIf k_header.Value = 0D Then
            k_row(k_valueColumn) = k_sum
            AddIssue(k_source, k_table, k_key, k_valueColumn, "N01", "CONVERSIE", "0", k_text(k_sum), "Antet 0: totalul devine suma liniilor (cu TVA).")
        ElseIf Math.Abs(k_header.Value - k_sum) > 1D Then
            k_row(k_valueColumn) = k_sum
            AddIssue(k_source, k_table, k_key, k_valueColumn, "N02", "AVERTIZARE", k_text(k_header), k_text(k_sum), "Antetul diferă cu mai mult de 1 leu de suma liniilor: totalul devine suma liniilor (cu TVA).")
        End If
    End Sub

    ''' <summary>One column of a row, converted by its class and link. A refused value is reported as blocking and the row is not written.</summary>
    Private Function ConvertColumn(k_gest As GestInfo, k_source As VrcPlanSource, k_sourceIndex As Integer, k_table As VrcTargetTable,
                                   k_column As VrcTargetColumn, k_raw As Object, k_key As String, ByRef k_ok As Boolean) As Object
        Dim k_number As Decimal? = Nothing
        Dim k_reason As String = String.Empty
        Select Case k_column.Link
            Case VrcLinkKind.ForeignKey, VrcLinkKind.Logical
                Return ConvertReference(k_gest, k_source, k_sourceIndex, k_table, k_column, k_raw, k_key, k_ok)
            Case VrcLinkKind.Group
                Dim k_group As Decimal? = VrcConvert.ToDecimal(k_raw)
                If Not k_group.HasValue OrElse k_group.Value = 0D Then Return Nothing
                Return GroupNumber(k_gest, k_source, k_sourceIndex, k_column, VrcConvert.KeyText(k_raw))
            Case VrcLinkKind.Technical
                Return Nothing
            Case VrcLinkKind.Open
                Dim k_open As Decimal? = VrcConvert.ToDecimal(k_raw)
                If k_open.HasValue AndAlso k_open.Value <> 0D Then
                    AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X10", "BLOCAJ", VrcConvert.KeyText(k_raw), String.Empty,
                             "Legătură fără părinte stabilit (neclasificată) cu valoare nenulă; părintele trebuie hotărât înainte de transfer.")
                    k_ok = False
                End If
                Return Nothing
        End Select

        Select Case k_column.FieldClass
            Case "VALOARE-INT", "COTA"
                k_number = VrcConvert.ToDecimal(k_raw)
                If Not k_number.HasValue Then Return Nothing
                Dim k_scale As Integer = If(k_column.FieldClass = "COTA", 4, _plan.Scale)
                Dim k_int As Integer
                If Not VrcConvert.Scaled(k_number.Value, k_scale, k_int, k_reason) Then
                    AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X11", "BLOCAJ", k_number.Value.ToString("0.########", CultureInfo.InvariantCulture), String.Empty, $"Valoarea {k_reason}.")
                    k_ok = False
                    Return Nothing
                End If
                Return k_int
            Case "NUMAR"
                Return ConvertDocumentNumber(k_source, k_table, k_column, k_raw, k_key, k_ok)
            Case "INT"
                k_number = VrcConvert.ToDecimal(k_raw)
                If Not k_number.HasValue Then Return Nothing
                Dim k_whole As Integer
                If Not VrcConvert.WholeInt(k_number.Value, k_whole, k_reason) Then
                    AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X12", "BLOCAJ", k_number.Value.ToString("0.####", CultureInfo.InvariantCulture), String.Empty, $"Valoarea {k_reason}.")
                    k_ok = False
                    Return Nothing
                End If
                Return k_whole
            Case "BOOL"
                Return VrcConvert.ToBit(k_raw)
            Case "DATA"
                If k_raw Is Nothing Then Return Nothing
                Return Convert.ToDateTime(k_raw, CultureInfo.InvariantCulture)
            Case Else
                If k_raw Is Nothing Then Return Nothing
                Return Convert.ToString(k_raw, CultureInfo.InvariantCulture)
        End Select
    End Function

    ''' <summary>NIR.Numar: a negative number is accepted only for an opening stock (NrFact contains «stoc»); a fraction is cut toward zero with a finding.</summary>
    Private Function ConvertDocumentNumber(k_source As VrcPlanSource, k_table As VrcTargetTable, k_column As VrcTargetColumn, k_raw As Object,
                                           k_key As String, ByRef k_ok As Boolean) As Object
        Dim k_number As Decimal? = VrcConvert.ToDecimal(k_raw)
        If Not k_number.HasValue Then Return Nothing
        Dim k_value As Decimal = k_number.Value
        If k_value < 0D Then
            Dim k_stock As Boolean = False
            If _currentRowIsStock Then k_stock = True
            If Not k_stock Then
                AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X02", "BLOCAJ", k_value.ToString("0.####", CultureInfo.InvariantCulture), String.Empty,
                         "Număr de document negativ la un document care nu este stoc inițial (NrFact nu conține «stoc»).")
                k_ok = False
                Return Nothing
            End If
        End If
        _currentNumarSub = 0
        Dim k_cut As Decimal = Math.Truncate(k_value)
        If k_cut <> k_value Then
            Dim k_hundredths As Decimal = Math.Abs(k_value - k_cut) * 100D
            If Math.Abs(k_hundredths - Math.Round(k_hundredths)) > 0.0001D Then
                AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X03", "BLOCAJ", k_value.ToString("0.########", CultureInfo.InvariantCulture), String.Empty,
                         "Număr de document cu mai mult de 2 zecimale; nu poate fi păstrat în NumarSub.")
                k_ok = False
                Return Nothing
            End If
            _currentNumarSub = CInt(Math.Round(k_hundredths))
            AddIssue(k_source, k_table.Name, k_key, k_column.Name, "N10", "CONVERSIE", k_value.ToString("0.##", CultureInfo.InvariantCulture), $"{k_cut:0} + NumarSub {_currentNumarSub}",
                     "Document introdus între două numere: partea întreagă rămâne în Numar, fracția (în sutimi) în NumarSub; ordinea cronologică se păstrează.")
        End If
        Dim k_result As Integer
        Dim k_reason As String = String.Empty
        If Not VrcConvert.WholeInt(k_cut, k_result, k_reason) Then
            AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X12", "BLOCAJ", k_value.ToString("0.####", CultureInfo.InvariantCulture), String.Empty, $"Valoarea {k_reason}.")
            k_ok = False
            Return Nothing
        End If
        Return k_result
    End Function

    ' set by AddNirExtras' pre-pass: whether the NIR row being converted is an opening stock
    Private _currentRowIsStock As Boolean
    ' set by ConvertDocumentNumber: the fraction of the NIR number in hundredths (707.1 -> 10)
    Private _currentNumarSub As Integer

    Private Function ConvertReference(k_gest As GestInfo, k_source As VrcPlanSource, k_sourceIndex As Integer, k_table As VrcTargetTable,
                                      k_column As VrcTargetColumn, k_raw As Object, k_key As String, ByRef k_ok As Boolean) As Object
        Dim k_value As Decimal? = VrcConvert.ToDecimal(k_raw)
        If Not k_value.HasValue OrElse k_value.Value = 0D Then
            If Not k_column.Nullable Then
                AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X05", "BLOCAJ", If(k_value.HasValue, "0", "NULL"), String.Empty, "Referință obligatorie lipsă.")
                k_ok = False
            End If
            Return Nothing
        End If
        Dim k_parent = VrcTargetSchema.Find(k_column.Parent)
        Dim k_textKey As String = VrcConvert.KeyText(k_raw)
        Dim k_mapKey As String = If(k_parent IsNot Nothing AndAlso k_parent.IsExercise, $"S{k_sourceIndex}|{k_column.Parent}|{k_textKey}", $"G{k_gest.Id}|{k_column.Parent}|{k_textKey}")
        Dim k_target As Long = 0
        If _map.TryGetValue(k_mapKey, k_target) Then Return k_target
        AddIssue(k_source, k_table.Name, k_key, k_column.Name, "X05", "BLOCAJ", k_textKey, String.Empty, $"Părintele din {k_column.Parent} nu există (rând orfan).")
        k_ok = False
        Return Nothing
    End Function

    ''' <summary>OI group labels get a fresh number per gestiune; the same label of the same source and column name gets the same number everywhere.</summary>
    Private Function GroupNumber(k_gest As GestInfo, k_source As VrcPlanSource, k_sourceIndex As Integer, k_column As VrcTargetColumn, k_label As String) As Long
        Dim k_mapKey As String = $"S{k_sourceIndex}|group:{k_column.GroupName}|{k_label}"
        Dim k_number As Long = 0
        If _map.TryGetValue(k_mapKey, k_number) Then Return k_number
        Dim k_counterKey As String = $"{k_gest.Id}|{k_column.GroupName}"
        Dim k_next As Long = 0
        _groupCounters.TryGetValue(k_counterKey, k_next)
        k_next += 1
        _groupCounters(k_counterKey) = k_next
        _map(k_mapKey) = k_next
        _pendingMap.Add((_sourceIds(k_source), "group:" & k_column.GroupName, "GROUP", k_label, k_next))
        Return k_next
    End Function

    ''' <summary>NIR only: IdPartener, CodFiscalSursa and TipDoc, appended in the order PrepareInsert declared them.</summary>
    Private Sub AddNirExtras(k_source As VrcPlanSource, k_row As Dictionary(Of String, Object), k_key As String, k_extra As ExtraContext, k_values As List(Of Object))
        Dim k_stock As Boolean = VrcConvert.IsOpeningStock(Convert.ToString(k_row("NrFact"), CultureInfo.InvariantCulture))
        Dim k_idPartener As Object = Nothing
        Dim k_cf As Object = Nothing
        Dim k_idf As String = VrcConvert.KeyText(k_row("IDF"))
        Dim k_code As String = Nothing
        If k_idf.Length > 0 AndAlso k_extra IsNot Nothing AndAlso k_extra.Suppliers IsNot Nothing AndAlso k_extra.Suppliers.TryGetValue(k_idf, k_code) Then
            If k_code IsNot Nothing AndAlso k_code.Trim().Length > 0 Then k_cf = If(k_code.Trim().Length > 255, k_code.Trim().Substring(0, 255), k_code.Trim())
            Dim k_normalized As String = VrcConvert.NormalizeTaxCode(k_code)
            Dim k_partner As Long = 0
            If Not k_stock Then
                If k_normalized.Length > 0 AndAlso _partners.TryGetValue(k_normalized, k_partner) Then
                    k_idPartener = k_partner
                ElseIf _missingSuppliers.Add($"{k_source.Profile.FileName}|{k_normalized}|{k_idf}") Then
                    AddIssue(k_source, "NIR", k_key, "IDF", "X04", "AVERTIZARE", If(k_code, "NULL"), "IdPartener NULL",
                             "Furnizorul nu are partener KBOT cu același cod fiscal: documentul se importă fără partener; codul rămâne în CodFiscalSursa.")
                End If
            End If
        End If
        k_values.Add(k_idPartener)
        k_values.Add(k_cf)
        k_values.Add(If(k_stock, "STOC_INITIAL", "NIR"))
        k_values.Add(_currentNumarSub)
    End Sub

    ''' <summary>Last real number per exercise: NIR, BC and list. The Access counters (NR.*) are not used.</summary>
    Private Sub TrackNumbering(k_gest As GestInfo, k_exercise As Long, k_table As VrcTargetTable, k_row As Dictionary(Of String, Object))
        Dim k_kind As String = Nothing
        Dim k_number As Long? = Nothing
        Select Case k_table.Name
            Case "NIR"
                Dim k_n As Decimal? = VrcConvert.ToDecimal(k_row("Numar"))
                If k_n.HasValue AndAlso k_n.Value > 0D Then k_number = CLng(Math.Truncate(k_n.Value))
                k_kind = "NIR"
            Case "Iesiri_H"
                k_number = VrcConvert.LeadingNumber(Convert.ToString(k_row("NumarBC"), CultureInfo.InvariantCulture))
                k_kind = "BC"
            Case "LABC"
                Dim k_n As Decimal? = VrcConvert.ToDecimal(k_row("NumarLA"))
                If k_n.HasValue AndAlso k_n.Value > 0D Then k_number = CLng(Math.Truncate(k_n.Value))
                k_kind = "LISTA"
        End Select
        If k_kind Is Nothing OrElse Not k_number.HasValue Then Return
        Dim k_id As String = $"{k_gest.Id}|{k_exercise}|{k_kind}"
        Dim k_current As Long = 0
        _lastNumber.TryGetValue(k_id, k_current)
        If k_number.Value > k_current Then _lastNumber(k_id) = k_number.Value
    End Sub

    ' ====================================================================================================================
    ' Counters, settings, the id map, the findings
    ' ====================================================================================================================
    Private Sub WriteCounters()
        For Each k_pair In _lastNumber
            Dim k_parts = k_pair.Key.Split("|"c)
            Insert("INSERT INTO VC_Numbering (SubunitId, GestiuneId, ExercitiuId, Kind, LastNumber) VALUES (@s, @g, @e, @k, @n)",
                   "@s", CType(_subunitId, Object), "@g", CLng(k_parts(0)), "@e", CLng(k_parts(1)), "@k", k_parts(2), "@n", k_pair.Value)
        Next
        Log($"Numerotare: {_lastNumber.Count} contoare pornite de la ultimul număr real (nu din NR.*).")
        For Each k_gest In _gestiuni.Where(Function(k_g) k_g.GestiuneType = "ALIMENTE")
            Dim k_latest = k_gest.Sources.Last()
            Dim k_si As Decimal? = Nothing
            For Each k_row In ReadRows(k_latest, "NR", {"SI_LA"})
                k_si = VrcConvert.ToDecimal(k_row("SI_LA"))
            Next
            If k_si.HasValue AndAlso k_si.Value <> 0D Then
                Dim k_scaled As Integer
                Dim k_reason As String = String.Empty
                If VrcConvert.Scaled(k_si.Value, _plan.Scale, k_scaled, k_reason) Then
                    Insert("INSERT INTO VC_Settings (SubunitId, GestiuneId, SettingKey, IntValue) VALUES (@s, @g, 'SI_LA', @v)",
                           "@s", CType(_subunitId, Object), "@g", k_gest.Id, "@v", k_scaled)
                Else
                    AddIssue(k_latest, "NR", "1", "SI_LA", "X11", "BLOCAJ", k_si.Value.ToString("0.########", CultureInfo.InvariantCulture), String.Empty, $"Soldul inițial Alimente {k_reason}.")
                End If
            End If
        Next
        FlushIssues()
    End Sub

    Private Sub FlushMap()
        If _pendingMap.Count = 0 Then Return
        For k_start As Integer = 0 To _pendingMap.Count - 1 Step MapBatch
            Dim k_batch = _pendingMap.Skip(k_start).Take(MapBatch).ToList()
            Dim k_sql As New StringBuilder("INSERT INTO VC_IdMap (SourceId, TableName, Kind, SourceKey, TargetId) VALUES ")
            Using k_cmd As New MySqlCommand(String.Empty, _cn, _tx)
                For k_i As Integer = 0 To k_batch.Count - 1
                    If k_i > 0 Then k_sql.Append(", ")
                    k_sql.Append($"(@a{k_i}, @b{k_i}, @c{k_i}, @d{k_i}, @e{k_i})")
                    k_cmd.Parameters.AddWithValue($"@a{k_i}", k_batch(k_i).SourceId)
                    k_cmd.Parameters.AddWithValue($"@b{k_i}", k_batch(k_i).TableName)
                    k_cmd.Parameters.AddWithValue($"@c{k_i}", k_batch(k_i).Kind)
                    k_cmd.Parameters.AddWithValue($"@d{k_i}", k_batch(k_i).SourceKey)
                    k_cmd.Parameters.AddWithValue($"@e{k_i}", k_batch(k_i).TargetId)
                Next
                k_cmd.CommandText = k_sql.ToString()
                k_cmd.ExecuteNonQuery()
            End Using
        Next
        _pendingMap.Clear()
    End Sub

    Private Sub AddIssue(k_source As VrcPlanSource, k_table As String, k_key As String, k_field As String, k_rule As String, k_severity As String,
                         k_original As String, k_target As String, k_text As String)
        If k_severity = "BLOCAJ" AndAlso _result.Issues.Where(Function(k_i) k_i.Severity = "BLOCAJ").Count() >= MaxBlockersShown Then Return
        _result.Issues.Add(New VrcIssue() With {
            .SourceFile = k_source.Profile.FileName, .TableName = k_table, .SourceKey = k_key, .FieldName = If(k_field, String.Empty), .RuleCode = k_rule,
            .Severity = k_severity, .OriginalValue = If(k_original, String.Empty), .TargetValue = If(k_target, String.Empty), .Justification = k_text})
    End Sub

    ''' <summary>The findings go to VC_MigrationIssues in the same transaction (so a rolled-back run leaves none behind; the report keeps them).</summary>
    Private Sub FlushIssues()
        Dim k_sourceIds = _sourceIds.ToDictionary(Function(k_p) k_p.Key.Profile.FileName, Function(k_p) k_p.Value, StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New MySqlCommand("INSERT INTO VC_MigrationIssues (ImportId, SourceId, TableName, SourceKey, FieldName, RuleCode, Severity, OriginalValue, TargetValue, Justification) VALUES (@i, @s, @t, @k, @f, @r, @v, @o, @g, @j)", _cn, _tx)
            For Each k_name In {"@i", "@s", "@t", "@k", "@f", "@r", "@v", "@o", "@g", "@j"}
                k_cmd.Parameters.Add(New MySqlParameter() With {.ParameterName = k_name})
            Next
            For Each k_issue In _result.Issues.Where(Function(k_x) k_x.Severity <> "BLOCAJ")
                Dim k_sid As Long = 0
                k_cmd.Parameters("@i").Value = _importId
                k_cmd.Parameters("@s").Value = If(k_sourceIds.TryGetValue(k_issue.SourceFile, k_sid), CType(k_sid, Object), DBNull.Value)
                k_cmd.Parameters("@t").Value = Trunc(k_issue.TableName, 40)
                k_cmd.Parameters("@k").Value = Trunc(k_issue.SourceKey, 100)
                k_cmd.Parameters("@f").Value = Trunc(k_issue.FieldName, 40)
                k_cmd.Parameters("@r").Value = Trunc(k_issue.RuleCode, 20)
                k_cmd.Parameters("@v").Value = k_issue.Severity
                k_cmd.Parameters("@o").Value = Trunc(k_issue.OriginalValue, 255)
                k_cmd.Parameters("@g").Value = Trunc(k_issue.TargetValue, 255)
                k_cmd.Parameters("@j").Value = Trunc(k_issue.Justification, 500)
                k_cmd.ExecuteNonQuery()
            Next
        End Using
    End Sub

    Private Shared Function Trunc(k_text As String, k_max As Integer) As Object
        If k_text Is Nothing OrElse k_text.Length = 0 Then Return DBNull.Value
        Return If(k_text.Length > k_max, k_text.Substring(0, k_max), k_text)
    End Function

    ' ====================================================================================================================
    ' Reconciliation (before COMMIT) and the closing of the marker
    ' ====================================================================================================================
    Private Sub Reconcile()
        For Each k_tracked In _counts.Values
            Dim k_columns = k_tracked.Sums.Keys.OrderBy(Function(k_c) k_c, StringComparer.OrdinalIgnoreCase).ToList()
            Dim k_sql As New StringBuilder("SELECT COUNT(*)")
            For Each k_c In k_columns
                k_sql.Append($", COALESCE(SUM(`{k_c}`), 0)")
            Next
            k_sql.Append($" FROM `{k_tracked.Table.TargetName}` WHERE SubunitId=@s AND GestiuneId=@g")
            If k_tracked.Table.IsExercise Then k_sql.Append(" AND ExercitiuId=@e")
            Using k_cmd As New MySqlCommand(k_sql.ToString(), _cn, _tx)
                k_cmd.Parameters.AddWithValue("@s", _subunitId)
                k_cmd.Parameters.AddWithValue("@g", k_tracked.GestiuneId)
                If k_tracked.Table.IsExercise Then k_cmd.Parameters.AddWithValue("@e", k_tracked.ExerciseId)
                Using k_reader = k_cmd.ExecuteReader()
                    If k_reader.Read() Then
                        k_tracked.Count.TargetRows = k_reader.GetInt64(0)
                        For k_i As Integer = 0 To k_columns.Count - 1
                            Dim k_read As Long = Convert.ToInt64(k_reader.GetValue(k_i + 1), CultureInfo.InvariantCulture)
                            If k_read <> k_tracked.Sums(k_columns(k_i)) Then
                                k_tracked.Count.SumDifferences.Add($"{k_columns(k_i)}: convertit {k_tracked.Sums(k_columns(k_i)):N0}, în MariaDB {k_read:N0}")
                            End If
                        Next
                    End If
                End Using
            End Using
            _result.Counts.Add(k_tracked.Count)
        Next
    End Sub

    Private Sub FinishImport()
        Dim k_summary = New With {
            .planHash = _plan.Hash(),
            .ruleVersion = VrcPlan.RuleVersion,
            .scale = _plan.Scale,
            .zec = _plan.Zec,
            .tables = _result.Counts.Select(Function(k_c) New With {k_c.Gestiune, .table = k_c.TableName, k_c.Year, .source = k_c.SourceRows, .skipped = k_c.SkippedRows, .target = k_c.TargetRows}).ToList(),
            .findings = _result.Issues.GroupBy(Function(k_i) k_i.Severity & "/" & k_i.RuleCode).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.Count())}
        Using k_cmd As New MySqlCommand("UPDATE VC_Imports SET Summary=@j, ConfirmedAt=NOW() WHERE ImportId=@i", _cn, _tx)
            k_cmd.Parameters.AddWithValue("@j", JsonSerializer.Serialize(k_summary))
            k_cmd.Parameters.AddWithValue("@i", _importId)
            k_cmd.ExecuteNonQuery()
        End Using
    End Sub

End Class
