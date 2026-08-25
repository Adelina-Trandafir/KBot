Imports System.Globalization
Imports System.Threading
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' Writes the transfer.
''' </summary>
''' <remarks>
''' <para>
''' <b>One transaction for the whole thing.</b> Any failure rolls everything back - a
''' half-migrated unit is worse than none. Cancelling rolls back too.
''' </para>
''' <para>
''' <b><c>FOREIGN_KEY_CHECKS</c> stays ON throughout.</b> Correct order is the point;
''' suppressing the check would hide exactly the bugs this tool exists to avoid. That is
''' the opposite of the database-creation path, where the checks go off precisely because
''' creation order carries no meaning.
''' </para>
''' <para>
''' Rows are written one at a time through a prepared command reused per table, not as
''' multi-row batches. Three things need per-row treatment and would be lost in a batch:
''' the id the server assigns (LAST_INSERT_ID returns only a batch's FIRST id, and
''' consecutive ids are not guaranteed under every innodb_autoinc_lock_mode), the
''' per-row orphan decision, and the per-row journal line. The journal is flushed and
''' progress reported every <see cref="BatchSize"/> rows.
''' </para>
''' </remarks>
Public NotInheritable Class TransferRunner

    ''' <summary>Rows between journal flushes and progress reports.</summary>
    Public Const BatchSize As Integer = 500

    Private ReadOnly _request As TransferRequest
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _progress As Action(Of String, Long, Long)
    Private ReadOnly _clasificatii As New ClasificatiiMap()
    Private ReadOnly _parteneri As New ParteneriMap()
    Private ReadOnly _written As New WrittenKeys()

    ''' <summary>
    ''' Which rows travel. Built by the verification that unlocked this run, and handed in
    ''' rather than rebuilt.
    ''' </summary>
    ''' <remarks>
    ''' Handed in on purpose, and required rather than optional. The selection has to be
    ''' resolved once and reused unchanged, or it could shift between the run that MEASURED
    ''' it and the run that WRITES it - and «Transferă» is only ever enabled by a
    ''' verification that has just built one, so there is nothing to fall back to and no
    ''' second construction site to drift.
    ''' </remarks>
    Private ReadOnly _ownership As OwnershipPlan

    Public Sub New(request As TransferRequest, ownership As OwnershipPlan,
                   log As Action(Of String), progress As Action(Of String, Long, Long))
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If ownership Is Nothing Then Throw New ArgumentNullException(NameOf(ownership))
        _request = request
        _ownership = ownership
        _log = log
        _progress = progress
    End Sub

    ''' <summary>Runs the transfer. Returns what happened; does not throw on a data error.</summary>
    Public Function Run(cancel As CancellationToken) As TransferResult
        Dim result As New TransferResult()
        Dim dump As SqlDumpWriter = Nothing

        Try
            dump = New SqlDumpWriter(_request.JournalFolder, _request.TargetDatabase, _log)
            result.JournalFolder = dump.Folder
            dump.WriteInfo(HeaderLines())
            Say($"Jurnalul rulării: {dump.Folder}")

            Using cn = _request.Server.Open(_request.TargetDatabase)
                Dim schema = TargetSchema.Load(cn, _request.TargetDatabase)

                Dim maps = TableMaps.All().
                    Where(Function(m) _request.IsTableSelected(m.TargetTable)).
                    ToList()

                Dim order = WriteOrder.Derive(schema, maps.Select(Function(m) m.TargetTable))
                Say("Ordinea de scriere, dedusă din cheile străine vii: " & String.Join(" ▸ ", order))

                Using transaction = cn.BeginTransaction()
                    Try
                        If _request.PopulateUnitati AndAlso schema.HasTable("Unitati") Then
                            result.Tables.Add(WriteUnitati(cn, transaction, schema, dump, cancel))
                        End If

                        For Each targetTable In order
                            cancel.ThrowIfCancellationRequested()
                            Dim map = TableMaps.ByTarget(maps, targetTable)
                            If map Is Nothing Then Continue For
                            If map.Source = SourceFile.Derived Then Continue For
                            result.Tables.Add(WriteTable(cn, transaction, schema, map, dump, cancel))
                        Next

                        transaction.Commit()
                        result.Committed = True
                        Say($"COMMIT. {result.TotalWritten} rânduri scrise.")

                    Catch ex As OperationCanceledException
                        transaction.Rollback()
                        result.Cancelled = True
                        Say("Rulare oprită de operator. ROLLBACK — baza a rămas exact cum era.")
                    Catch ex As Exception
                        GlobalErrorLog.Write("TransferRunner.Run", ex)
                        Try
                            transaction.Rollback()
                        Catch rollbackError As Exception
                            GlobalErrorLog.Write("TransferRunner.Rollback", rollbackError)
                        End Try
                        result.Error = ex
                        Say($"ROLLBACK. Transferul a eșuat: {ex.Message}")
                    End Try
                End Using
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("TransferRunner.Run", ex)
            result.Error = ex
            Say($"Transferul nu a putut porni: {ex.Message}")
        Finally
            If dump IsNot Nothing Then
                Try
                    dump.WriteFinal(result.Committed, result.Totals(), result.Error)
                Finally
                    dump.Dispose()
                End Try
            End If
        End Try

        Return result
    End Function

    ' ---- Unitati, the precondition -------------------------------------------------

    ''' <summary>
    ''' Writes the selected units into <c>Unitati</c> before anything else.
    ''' </summary>
    ''' <remarks>
    ''' Operator decision, 23.08: the tool does this. D7 makes it unavoidable - a database
    ''' created from the template has structure but no rows, and Clasificatii,
    ''' Clasificatii_Buget, Parteneri and FX_ORD_TBL all carry a foreign key into Unitati.
    ''' IdUnitate is a primary key WITHOUT auto_increment, so it can only arrive here.
    ''' SursaSector and CodProgram come from the cai row and the unit's own UNIT table; An is
    ''' 2026 (D1). Detalii prefers the unit's own UNIT.Detalii, falls back to the cai row's
    ''' AlteDetalii, and falls back again to the unit's display name.
    ''' </remarks>
    Private Function WriteUnitati(cn As MySqlConnection, transaction As MySqlTransaction,
                                  schema As TargetSchema, dump As SqlDumpWriter,
                                  cancel As CancellationToken) As TableOutcome
        Dim outcome As New TableOutcome("Unitati")
        Dim columns = {"IdUnitate", "Detalii", "SursaSector", "An", "CodProgram", "Ascuns"}.
            Where(Function(c) schema.Column("Unitati", c) IsNot Nothing).
            ToList()
        outcome.ColumnsWritten = columns.Count

        Dim sql = BuildUpsert("Unitati", columns, schema)
        Using cmd As New MySqlCommand(sql, cn, transaction)
            For Each column In columns
                cmd.Parameters.Add(New MySqlParameter("@" & column, Nothing))
            Next
            cmd.Prepare()

            For Each unit In _request.Units
                cancel.ThrowIfCancellationRequested()
                outcome.RowsRead += 1

                Dim details = ReadUnitDetails(unit)
                Dim detalii = If(String.IsNullOrWhiteSpace(details.Detalii), unit.AlteDetalii, details.Detalii)
                detalii = If(String.IsNullOrWhiteSpace(detalii), unit.NumeUnitate, detalii)
                Dim values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                    {"IdUnitate", unit.IdUnitate},
                    {"Detalii", detalii},
                    {"SursaSector", unit.Sursa},
                    {"An", TableMaps.TransferYear},
                    {"CodProgram", If(String.IsNullOrEmpty(details.CodProgram), CObj(DBNull.Value), details.CodProgram)},
                    {"Ascuns", 0}
                }

                For Each column In columns
                    cmd.Parameters("@" & column).Value = values(column)
                Next

                dump.WriteStatement("Unitati", RenderStatement(sql, columns, values))
                cmd.ExecuteNonQuery()
                _written.Add("Unitati", unit.IdUnitate)
                outcome.RowsWritten += 1
            Next
        End Using

        dump.FlushAll()
        Say($"«Unitati»: {outcome.RowsWritten} unități scrise.")
        Return outcome
    End Function

    Private Function ReadUnitDetails(unit As CaiUnit) As (Detalii As String, CodProgram As String)
        If Not unit.HasUnitFile Then Return (unit.NumeUnitate, String.Empty)
        Try
            Using cn = AccessProvider.Open(unit.UnitFilePath, _request.UnitFilePassword)
                Dim realName = AccessSchema.ResolveTableName(cn, "UNIT")
                If realName Is Nothing Then Return (unit.NumeUnitate, String.Empty)
                Using reader = AccessSchema.OpenReader(cn, realName)
                    If Not reader.Read() Then Return (unit.NumeUnitate, String.Empty)
                    Return (Verifier.AsText(reader.ValueOrMissing("Detalii")),
                            Verifier.AsText(reader.ValueOrMissing("CodProgram")))
                End Using
            End Using
        Catch ex As Exception
            ' The unit label is a courtesy; a missing UNIT table must not stop the run.
            GlobalErrorLog.Write("TransferRunner.ReadUnitDetails", ex)
            Return (unit.NumeUnitate, String.Empty)
        End Try
    End Function

    ' ---- one table ------------------------------------------------------------------

    ''' <summary>
    ''' Writes one table: one pass per distinct Access FILE, not per unit.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>Decision D4 collapsed the per-unit loop.</b> One target database holds every
    ''' IdUnitate of its DC, and eleven of the thirteen <c>cai</c> rows name the SAME
    ''' Forexe file - so looping the units meant reading that one file once per ticked
    ''' unit and asking "is this row unit 75's?" instead of "is this row one of ours?".
    ''' The question is membership in the selected set now, asked once per row, and the
    ''' double-counting of <see cref="TableOutcome.RowsWritten"/> that slice 0045-07 left
    ''' open disappears with the second pass.
    ''' </para>
    ''' <para>
    ''' It is grouped by FILE rather than flattened to a single pass because the
    ''' nomenclators are genuinely per unit: each unit has its own <c>baza2026.accdb</c>,
    ''' and those rows carry no <c>IdUnitate</c> at all - the FILE is the unit. A pass over
    ''' a file that exactly one unit points at still knows whose rows it is reading, which
    ''' is what keeps <see cref="ColumnSourceKind.UnitId"/> honest on Clasificatii and
    ''' Parteneri without any of them needing a column Access never had.
    ''' </para>
    ''' </remarks>
    Private Function WriteTable(cn As MySqlConnection, transaction As MySqlTransaction,
                                schema As TargetSchema, map As TableMap,
                                dump As SqlDumpWriter, cancel As CancellationToken) As TableOutcome
        Dim outcome As New TableOutcome(map.TargetTable)
        If Not String.IsNullOrEmpty(map.Note) Then dump.WriteComment(map.TargetTable, map.Note)

        Dim links = ParentLinks(schema, map.TargetTable)
        Dim primaryKey = PrimaryKeyColumn(schema, map.TargetTable)

        For Each pass In FilePasses(map)
            cancel.ThrowIfCancellationRequested()

            Using accessCn = AccessProvider.Open(pass.Path, pass.Password)
                Dim realName = AccessSchema.ResolveTableName(accessCn, map.AccessTable)
                If realName Is Nothing Then
                    dump.WriteComment(map.TargetTable,
                                      $"«{pass.Path}»: tabelul Access «{map.AccessTable}» lipsește")
                    Continue For
                End If

                Dim accessColumns = AccessSchema.Columns(accessCn, realName).Select(Function(c) c.Name).ToList()
                Dim plan = ColumnPlan.Build(map, accessColumns, schema)
                If plan.Mappings.Count = 0 Then Continue For
                outcome.ColumnsWritten = plan.Mappings.Count

                LogPlan(map, plan, pass)

                Dim columnNames = plan.ColumnNames()
                Dim sql = BuildUpsert(map.TargetTable, columnNames, schema)

                Using cmd As New MySqlCommand(sql, cn, transaction)
                    For Each column In columnNames
                        cmd.Parameters.Add(New MySqlParameter("@" & column, Nothing))
                    Next
                    cmd.Prepare()

                    Using reader = AccessSchema.OpenReader(accessCn, realName)
                        While reader.Read()
                            cancel.ThrowIfCancellationRequested()
                            outcome.RowsRead += 1

                            Dim verdict = _ownership.Decide(map, reader, pass.Units)
                            Select Case verdict.Disposition
                                Case RowDisposition.OtherUnit
                                    outcome.RowsOtherUnit += 1
                                    Continue While
                                Case RowDisposition.SubtreeStayedBehind
                                    outcome.RowsSubtreeSkipped += 1
                                    Continue While
                            End Select

                            Dim values As Dictionary(Of String, Object) = Nothing
                            Dim skip = False
                            values = BuildValues(map, plan, schema, reader, verdict, outcome, skip)
                            If skip Then
                                outcome.RowsOrphanParent += 1
                                Continue While
                            End If

                            If Not ParentsTravelled(reader, links, values, outcome) Then
                                outcome.RowsOrphanParent += 1
                                Continue While
                            End If

                            For Each column In columnNames
                                cmd.Parameters("@" & column).Value = values(column)
                            Next

                            dump.WriteStatement(map.TargetTable, RenderStatement(sql, columnNames, values))
                            cmd.ExecuteNonQuery()
                            outcome.RowsWritten += 1

                            RecordAssignedId(cmd, map, reader, verdict)
                            RecordPrimaryKey(cmd, map, primaryKey, values)

                            If outcome.RowsWritten Mod BatchSize = 0 Then
                                dump.FlushAll()
                                _progress?.Invoke(map.TargetTable, outcome.RowsWritten, outcome.RowsRead)
                            End If
                        End While
                    End Using
                End Using
            End Using
        Next

        dump.FlushAll()
        Say(Describe(outcome))
        Return outcome
    End Function

    ''' <summary>
    ''' The Access files this table must be read from, each with the units that point at it.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>Only the FOREXE side is grouped.</b> That is where D4 pays: eleven of the
    ''' thirteen <c>cai</c> rows name the same <c>FX_2026.accdb</c>, so grouping turns
    ''' eleven reads of one file into one, and turns "is this row unit 75's?" into "is this
    ''' row one of ours?".
    ''' </para>
    ''' <para>
    ''' A NOMENCLATOR file keeps one pass per unit, deliberately. Each unit has its own
    ''' <c>baza2026.accdb</c> today (verified on the live registry, 24.08: thirteen units,
    ''' thirteen distinct paths, none shared), and those rows carry no <c>IdUnitate</c> at
    ''' all - the file IS the unit. If two <c>cai</c> rows ever pointed at ONE nomenclator
    ''' file, grouping them would leave the rows with no single unit and stop the run,
    ''' where writing the same classifications once per unit is a defensible reading and
    ''' the behaviour every earlier slice had. Not grouping costs nothing - the paths are
    ''' distinct anyway - and it declines to turn an untested shape into a refusal.
    ''' </para>
    ''' </remarks>
    Private Function FilePasses(map As TableMap) As List(Of FilePass)
        Dim ordered As New List(Of FilePass)()
        Dim byPath As New Dictionary(Of String, FilePass)(StringComparer.OrdinalIgnoreCase)
        Dim groupByFile = map.Source <> SourceFile.UnitFile

        For Each unit In _request.Units
            Dim path = If(map.Source = SourceFile.UnitFile, unit.UnitFilePath, unit.ForexeFilePath)
            If String.IsNullOrEmpty(path) OrElse Not IO.File.Exists(path) Then Continue For
            Dim password = If(map.Source = SourceFile.UnitFile,
                              _request.UnitFilePassword, _request.ForexeFilePassword)

            Dim pass As FilePass = Nothing
            If Not groupByFile OrElse Not byPath.TryGetValue(path, pass) Then
                pass = New FilePass(path, password)
                If groupByFile Then byPath(path) = pass
                ordered.Add(pass)
            End If
            pass.Units.Add(unit)
        Next

        Return ordered
    End Function

    ' ---- values ---------------------------------------------------------------------

    ''' <summary>
    ''' One row's values, with every derived column resolved against the ROW's own unit.
    ''' </summary>
    ''' <remarks>
    ''' Since D4 there is no loop unit to borrow. <paramref name="verdict"/> carries the
    ''' unit the row itself named - or says it has none, in which case a mapping that
    ''' needs one stops the run rather than picking a nomenclator at random. That is the
    ''' whole lesson of 0045-07 expressed as a signature: the unit arrives with the row.
    ''' </remarks>
    Private Function BuildValues(map As TableMap, plan As ColumnPlan, schema As TargetSchema,
                                 reader As AccessTableReader, verdict As RowVerdict,
                                 outcome As TableOutcome, ByRef skipRow As Boolean) As Dictionary(Of String, Object)
        Dim values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)

        For Each mapping In plan.Mappings
            Dim target = schema.Column(map.TargetTable, mapping.TargetColumn)

            Select Case mapping.Kind
                Case ColumnSourceKind.UnitId
                    values(mapping.TargetColumn) = RowUnit(map, verdict, mapping.TargetColumn)

                Case ColumnSourceKind.Constant
                    values(mapping.TargetColumn) = If(mapping.ConstantValue, DBNull.Value)

                Case ColumnSourceKind.ForcedNull
                    values(mapping.TargetColumn) = DBNull.Value
                    outcome.ValuesNulled += 1

                Case ColumnSourceKind.ResolvedClasificatie
                    Dim clsfUnit = RowUnit(map, verdict, mapping.TargetColumn)
                    Dim accessId = Verifier.AsInteger(reader.ValueOrMissing(mapping.AccessColumn))
                    Dim assigned As Integer
                    If accessId.HasValue AndAlso accessId.Value <> 0 AndAlso
                       _clasificatii.TryResolve(accessId.Value, clsfUnit, assigned) Then
                        values(mapping.TargetColumn) = assigned
                    ElseIf mapping.BlockingOnMiss Then
                        Throw New TransferException(
                            $"«{map.TargetTable}»: clasificația Access {If(accessId.HasValue, accessId.Value.ToString(CultureInfo.InvariantCulture), "(lipsă)")} " &
                            $"a unității {clsfUnit} nu se regăsește în «Clasificatii» transferate. " &
                            "Rularea s-a oprit — un rând neclasificat tăcut e mai rău decât un refuz.")
                    Else
                        values(mapping.TargetColumn) = DBNull.Value
                        outcome.ValuesNulled += 1
                    End If

                Case ColumnSourceKind.ResolvedPartener
                    Dim partnerUnit = RowUnit(map, verdict, mapping.TargetColumn)
                    Dim code = Verifier.AsText(reader.ValueOrMissing(mapping.AccessColumn))
                    Dim assigned As Integer
                    If _parteneri.TryResolve(code, partnerUnit, assigned) Then
                        values(mapping.TargetColumn) = assigned
                    ElseIf mapping.BlockingOnMiss Then
                        Throw New TransferException(
                            $"«{map.TargetTable}»: partenerul «{code}» al unității {partnerUnit} " &
                            "nu se regăsește în «Parteneri» transferați.")
                    Else
                        values(mapping.TargetColumn) = DBNull.Value
                        outcome.ValuesNulled += 1
                    End If

                Case Else
                    values(mapping.TargetColumn) =
                        ValueConverter.ToParameter(reader.ValueOrMissing(mapping.AccessColumn), target)
            End Select
        Next

        Return values
    End Function

    ' ---- unit and parent filtering ---------------------------------------------------

    ''' <summary>
    ''' The unit a derived mapping must resolve against, or a refusal.
    ''' </summary>
    ''' <remarks>
    ''' There is no loop unit any more (D4), so "which nomenclator?" has exactly one honest
    ''' source: the row. When the row has no single unit - a DDF header serving several, or
    ''' a table with no unit column read from the shared Forexe file - the answer is not
    ''' available and the run stops. It does NOT pick one. Picking one is precisely what
    ''' produced the mirrored 141 / 97+374 findings of 23.08, and the verifier refuses this
    ''' case before a single row is written, so reaching here means the catalogue changed
    ''' under the gate.
    ''' </remarks>
    Private Shared Function RowUnit(map As TableMap, verdict As RowVerdict,
                                    targetColumn As String) As Integer
        If verdict.HasUnit Then Return verdict.IdUnitate

        Dim why = If(verdict.Scope = UnitScope.SharedByMany,
                     "rândul servește mai multe unități deodată",
                     "tabelul nu are deloc coloana «IdUnitate», iar fișierul FOREXE ține rândurile tuturor unităților din DC")
        Throw New TransferException(
            $"«{map.TargetTable}».«{targetColumn}» are nevoie de UNA singură, dar {why}. " &
            "Unealta nu alege o unitate la întâmplare — s-ar rezolva pe nomenclatorul " &
            "greșit, tăcut. Declarați proprietarul rândului sau scoateți coloana din " &
            "maparea derivată.")
    End Function

    ''' <summary>
    ''' True when every parent this row points at actually travelled.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The parent key sets come from <see cref="WrittenKeys"/>, filled as the parents were
    ''' written - and the topological order guarantees they are complete by now. This is
    ''' what replaces slice 0044's hand-built A-E routing maps. Since slice 0046 it is the
    ''' BACKSTOP rather than the only defence: <see cref="OwnershipPlan"/> holds whole
    ''' subtrees back up front, so a child normally never gets this far.
    ''' </para>
    ''' <para>
    ''' <b>D14 - the nullable path now actually blanks the value.</b> It used to increment
    ''' <see cref="TableOutcome.ValuesNulled"/> and move on, while
    ''' <see cref="BuildValues"/> had no orphan branch at all - <c>ForcedNull</c> is a
    ''' DECLARED mapping, not an orphan decision - so the orphan value went to the server
    ''' unchanged and the reported count was a lie. The operator's rule of 22.08 ("nullable
    ''' column ▸ the row is written with that column emptied") had therefore never once
    ''' happened. It needs the values dictionary, which is why that arrived as a parameter.
    ''' </para>
    ''' </remarks>
    Private Function ParentsTravelled(reader As AccessTableReader,
                                      links As IReadOnlyList(Of ParentLink),
                                      values As Dictionary(Of String, Object),
                                      outcome As TableOutcome) As Boolean
        For Each link In links
            If Not _written.Tracks(link.ParentTable) Then Continue For
            If Not reader.HasColumn(link.AccessColumn) Then Continue For

            Dim raw = reader.ValueOrMissing(link.AccessColumn)
            If raw Is Nothing OrElse raw Is DBNull.Value Then Continue For
            If ValueConverter.IsOrphanValue(raw, link.ParentKeyIsAutoIncrement) Then Continue For
            If _written.Contains(link.ParentTable, raw) Then Continue For

            ' The parent is in scope but this value was not written for it.
            If link.ChildColumnIsNullable Then
                ' The column is named on the TARGET side; the values dictionary is keyed
                ' the same way, so a link whose column is not being written at all simply
                ' finds nothing to blank - and then there is no dangling id to write either.
                If values.ContainsKey(link.ChildColumn) Then
                    values(link.ChildColumn) = DBNull.Value
                    outcome.ValuesNulled += 1
                End If
                Continue For
            End If
            Return False
        Next
        Return True
    End Function

    Private Shared Function ParentLinks(schema As TargetSchema, targetTable As String) As List(Of ParentLink)
        Dim links As New List(Of ParentLink)()
        For Each fk In schema.ForeignKeysOf(targetTable)
            If fk.IsCrossSchema(schema.SchemaName) Then Continue For
            If String.Equals(fk.ChildTable, fk.ParentTable, StringComparison.OrdinalIgnoreCase) Then Continue For

            Dim childColumn = schema.Column(targetTable, fk.ChildColumn)
            Dim parentColumn = schema.Column(fk.ParentTable, fk.ParentColumn)
            links.Add(New ParentLink(
                fk.ParentTable,
                fk.ChildColumn,
                childColumn Is Nothing OrElse childColumn.IsNullable,
                parentColumn IsNot Nothing AndAlso parentColumn.IsAutoIncrement))
        Next
        Return links
    End Function

    ' ---- key bookkeeping --------------------------------------------------------------

    Private Sub RecordAssignedId(cmd As MySqlCommand, map As TableMap,
                                 reader As AccessTableReader, verdict As RowVerdict)
        If map.Feeds = ResolutionTarget.None Then Return

        Dim assigned = CInt(cmd.LastInsertedId)
        If assigned = 0 Then Return

        ' Both maps are keyed on (key, unit), and the unit is the ROW's - which for the two
        ' tables that feed them is the unit of the nomenclator file they came from, since
        ' Access Clasificatii and Parteneri carry no IdUnitate column at all.
        Dim unit = RowUnit(map, verdict, map.FeedKeyColumn)

        Select Case map.Feeds
            Case ResolutionTarget.Clasificatii
                Dim accessId = Verifier.AsInteger(reader.ValueOrMissing(map.FeedKeyColumn))
                If accessId.HasValue Then _clasificatii.Add(accessId.Value, unit, assigned)
                _written.Add(map.TargetTable, assigned)
            Case ResolutionTarget.Parteneri
                Dim code = Verifier.AsText(reader.ValueOrMissing(map.FeedKeyColumn))
                If code.Length > 0 Then _parteneri.Add(code, unit, assigned)
                _written.Add(map.TargetTable, assigned)
        End Select
    End Sub

    Private Sub RecordPrimaryKey(cmd As MySqlCommand, map As TableMap, primaryKey As String,
                                 values As Dictionary(Of String, Object))
        If map.Feeds <> ResolutionTarget.None Then Return
        If String.IsNullOrEmpty(primaryKey) Then Return

        Dim value As Object = Nothing
        If values.TryGetValue(primaryKey, value) AndAlso value IsNot Nothing AndAlso value IsNot DBNull.Value Then
            _written.Add(map.TargetTable, value)
            Return
        End If

        ' The key was omitted, so the server assigned it.
        If cmd.LastInsertedId <> 0 Then _written.Add(map.TargetTable, cmd.LastInsertedId)
    End Sub

    ''' <summary>
    ''' The single-column primary key whose values are recorded, or Nothing.
    ''' </summary>
    ''' <remarks>
    ''' <b>Returning Nothing for a composite key is honest; the SILENCE was the defect.</b>
    ''' <c>FX_DDF</c> was <c>PRIMARY KEY (IDDF, CUAL)</c>, so <see cref="RecordPrimaryKey"/>
    ''' recorded nothing, <c>WrittenKeys.Tracks("FX_DDF")</c> stayed False, and the first
    ''' line of <see cref="ParentsTravelled"/> then dropped the FX_DDF link for EVERY child
    ''' pointing at it - FX_DDF_REV, FX_ORD and tblDocFund_Revizii_Clsf alike. Nothing said
    ''' a word, and the run died later on a 1452 that named the child.
    ''' The behaviour here is unchanged on purpose. What changed is that
    ''' <c>Verifier.CheckParentKeysTrackable</c> now refuses the run by name when a parent
    ''' in the write set lands in this state, so the condition can never again be silent.
    ''' </remarks>
    Friend Shared Function PrimaryKeyColumn(schema As TargetSchema, targetTable As String) As String
        Dim primary = schema.UniqueKeysOf(targetTable).FirstOrDefault(Function(k) k.IsPrimary)
        If primary Is Nothing OrElse primary.Columns.Count <> 1 Then Return Nothing
        Return primary.Columns(0)
    End Function

    ' ---- SQL ---------------------------------------------------------------------------

    ''' <summary>
    ''' <c>INSERT ... ON DUPLICATE KEY UPDATE</c>, with the key columns left out of the
    ''' update list.
    ''' </summary>
    ''' <remarks>
    ''' A table with no unique key covered by the written columns gets a plain INSERT -
    ''' there is nothing for the upsert to match on, and pretending otherwise would make a
    ''' second run duplicate rows silently. That is Clasificatii today (decision D8), and
    ''' the verifier refuses a second run rather than letting it happen.
    ''' </remarks>
    Private Shared Function BuildUpsert(targetTable As String, columns As IReadOnlyList(Of String),
                                        schema As TargetSchema) As String
        Dim quoted = String.Join(", ", columns.Select(AddressOf TargetServer.Quote))
        Dim placeholders = String.Join(", ", columns.Select(Function(c) "@" & c))
        Dim sql = $"INSERT INTO {TargetServer.Quote(targetTable)} ({quoted}) VALUES ({placeholders})"

        If Not schema.CanUpsert(targetTable, columns) Then Return sql

        Dim keyColumns As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each key In schema.UniqueKeysOf(targetTable)
            For Each column In key.Columns
                keyColumns.Add(column)
            Next
        Next

        Dim updates = columns.
            Where(Function(c) Not keyColumns.Contains(c)).
            Select(Function(c) $"{TargetServer.Quote(c)} = VALUES({TargetServer.Quote(c)})").
            ToList()
        If updates.Count = 0 Then Return sql

        Return sql & " ON DUPLICATE KEY UPDATE " & String.Join(", ", updates)
    End Function

    ''' <summary>
    ''' The journal rendering of a statement: the same SQL with literals in place of
    ''' placeholders.
    ''' </summary>
    ''' <remarks>
    ''' A RECONSTRUCTION, never a transcript - the driver sends parameters, not text. Each
    ''' journal file says so in its own header.
    ''' </remarks>
    Private Shared Function RenderStatement(sql As String, columns As IReadOnlyList(Of String),
                                            values As Dictionary(Of String, Object)) As String
        Dim rendered = sql
        ' Longest placeholder first, so @IdClsf does not eat the head of @IdClsfAcc.
        For Each column In columns.OrderByDescending(Function(c) c.Length)
            Dim value As Object = Nothing
            values.TryGetValue(column, value)
            rendered = rendered.Replace("@" & column, ValueConverter.ToLiteral(value))
        Next
        Return rendered
    End Function

    ' ---- logging ------------------------------------------------------------------------

    Private Sub LogPlan(map As TableMap, plan As ColumnPlan, pass As FilePass)
        Say($"«{map.TargetTable}» ({pass.Describe()}): se scriu {plan.Mappings.Count} coloane — " &
            String.Join(", ", plan.ColumnNames()))
        If plan.Skipped.Count > 0 Then
            Say($"   sărite: {String.Join("; ", plan.Skipped)}")
        End If
    End Sub

    Private Shared Function Describe(outcome As TableOutcome) As String
        Dim line = $"«{outcome.TargetTable}»: {outcome.RowsWritten} scrise din {outcome.RowsRead} citite"
        If outcome.RowsOtherUnit > 0 Then line &= $", {outcome.RowsOtherUnit} ale altei unități"
        If outcome.RowsSubtreeSkipped > 0 Then line &= $", {outcome.RowsSubtreeSkipped} cu documentul rămas în urmă"
        If outcome.RowsOrphanParent > 0 Then line &= $", {outcome.RowsOrphanParent} cu părinte netransferat"
        If outcome.ValuesNulled > 0 Then line &= $", {outcome.ValuesNulled} valori golite"
        Return line & "."
    End Function

    Private Function HeaderLines() As List(Of String)
        Dim lines As New List(Of String)()
        lines.Add($"Server:    {_request.Server.Describe()}")
        lines.Add($"Bază:      {_request.TargetDatabase}")
        lines.Add($"Șablon:    {_request.TemplateDatabase}")
        lines.Add($"Comună:    {_request.CommonDatabase}")
        lines.Add($"An:        {TableMaps.TransferYear}")
        ' D16: BOTH values, always. A journal that recorded only the one used could not
        ' answer "was this run done on the registry's code, or on a typed one?" months
        ' later, which is exactly when somebody will ask.
        Dim fromRegistry = _request.RegistryCodFiscal()
        Dim used = _request.ResolvedCodFiscal()
        lines.Add($"Cod fiscal din registry: {If(fromRegistry.Length = 0, "(lipsă)", fromRegistry)}")
        lines.Add($"Cod fiscal folosit:      {If(used.Length = 0, "(lipsă)", used)}" &
                  If(String.Equals(used, fromRegistry, StringComparison.Ordinal), String.Empty, "   ◂ SUPRASCRIS de operator"))
        lines.Add($"Operator:  {_request.OperatorName}")
        lines.Add($"Pornit:    {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        lines.Add(String.Empty)
        lines.Add("Unități:")
        For Each unit In _request.Units
            lines.Add($"  {unit.IdUnitate} — {unit.NumeUnitate} ({unit.Sursa})")
            lines.Add($"      nomenclatoare: {unit.UnitFilePath}")
            lines.Add($"      forexe:        {If(unit.ForexeFilePath, "(niciunul)")}")
        Next
        lines.Add(String.Empty)
        lines.Add("Tabele bifate: " & String.Join(", ", _request.SelectedTables.OrderBy(Function(t) t)))
        Return lines
    End Function

    Private Sub Say(message As String)
        _log?.Invoke(message)
    End Sub

End Class

''' <summary>
''' One Access file, and the selected units that point at it.
''' </summary>
''' <remarks>
''' The unit of a PASS, not of a row. It is one unit for a nomenclator file - where the
''' rows carry no <c>IdUnitate</c> and the file IS the unit - and eleven for the shared
''' Forexe file, where it says nothing at all about who a row belongs to and
''' <see cref="OwnershipPlan"/> answers instead. Keeping the whole list rather than a count
''' is what lets the single-unit case stay honest without special-casing the table names.
''' </remarks>
Friend NotInheritable Class FilePass

    Public Sub New(path As String, password As String)
        Me.Path = path
        Me.Password = password
        Units = New List(Of CaiUnit)()
    End Sub

    Public ReadOnly Property Path As String
    Public ReadOnly Property Password As String
    Public ReadOnly Property Units As List(Of CaiUnit)

    ''' <summary>How the pass names itself in the log.</summary>
    Public Function Describe() As String
        If Units.Count = 1 Then Return $"unitatea {Units(0).IdUnitate}"
        Return $"{Units.Count} unități: " &
               String.Join(", ", Units.Select(Function(u) u.IdUnitate.ToString(CultureInfo.InvariantCulture)))
    End Function

End Class

''' <summary>One foreign key, reduced to what the row filter needs.</summary>
Friend NotInheritable Class ParentLink

    Public Sub New(parentTable As String, accessColumn As String,
                   childColumnIsNullable As Boolean, parentKeyIsAutoIncrement As Boolean)
        Me.ParentTable = parentTable
        Me.AccessColumn = accessColumn
        Me.ChildColumnIsNullable = childColumnIsNullable
        Me.ParentKeyIsAutoIncrement = parentKeyIsAutoIncrement
    End Sub

    Public ReadOnly Property ParentTable As String
    ''' <summary>
    ''' The child column's name, which is also the Access column's name for every link the
    ''' filter can use - a renamed key is looked up under the TARGET name, and the reader
    ''' answers "no such column" for the rest, which is a skip rather than a wrong answer.
    ''' </summary>
    Public ReadOnly Property AccessColumn As String
    ''' <summary>
    ''' The same name read as the TARGET column, which is how the values dictionary is
    ''' keyed. One string, two roles: <see cref="AccessColumn"/> is what the reader is
    ''' asked for, this is what gets blanked when the parent turns out to be absent (D14).
    ''' </summary>
    Public ReadOnly Property ChildColumn As String
        Get
            Return AccessColumn
        End Get
    End Property
    Public ReadOnly Property ChildColumnIsNullable As Boolean
    Public ReadOnly Property ParentKeyIsAutoIncrement As Boolean

End Class

''' <summary>A data condition that stops the transfer. Carries a Romanian message.</summary>
Public NotInheritable Class TransferException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

End Class
