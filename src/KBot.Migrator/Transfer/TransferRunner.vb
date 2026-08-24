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

    Public Sub New(request As TransferRequest, log As Action(Of String),
                   progress As Action(Of String, Long, Long))
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        _request = request
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

    Private Function WriteTable(cn As MySqlConnection, transaction As MySqlTransaction,
                                schema As TargetSchema, map As TableMap,
                                dump As SqlDumpWriter, cancel As CancellationToken) As TableOutcome
        Dim outcome As New TableOutcome(map.TargetTable)
        If Not String.IsNullOrEmpty(map.Note) Then dump.WriteComment(map.TargetTable, map.Note)

        Dim links = ParentLinks(schema, map.TargetTable)
        Dim primaryKey = PrimaryKeyColumn(schema, map.TargetTable)

        For Each unit In _request.Units
            cancel.ThrowIfCancellationRequested()

            Dim path = If(map.Source = SourceFile.UnitFile, unit.UnitFilePath, unit.ForexeFilePath)
            Dim password = If(map.Source = SourceFile.UnitFile, _request.UnitFilePassword, _request.ForexeFilePassword)
            If String.IsNullOrEmpty(path) OrElse Not IO.File.Exists(path) Then Continue For

            Using accessCn = AccessProvider.Open(path, password)
                Dim realName = AccessSchema.ResolveTableName(accessCn, map.AccessTable)
                If realName Is Nothing Then
                    dump.WriteComment(map.TargetTable,
                                      $"unitatea {unit.IdUnitate}: tabelul Access «{map.AccessTable}» lipsește")
                    Continue For
                End If

                ' Built per unit rather than cached: the parent tables are small (FX_DDF
                ' has 9 rows, FX_Extrase_H has 338) and a cache keyed by file path would
                ' outlive the connection it was read through.
                Dim ownership = UnitOwnership.Build(accessCn, map)

                Dim accessColumns = AccessSchema.Columns(accessCn, realName).Select(Function(c) c.Name).ToList()
                Dim plan = ColumnPlan.Build(map, accessColumns, schema)
                If plan.Mappings.Count = 0 Then Continue For
                outcome.ColumnsWritten = plan.Mappings.Count

                LogPlan(map, plan, unit)

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

                            If Not BelongsToUnit(reader, unit, map, ownership) Then
                                outcome.RowsOtherUnit += 1
                                Continue While
                            End If

                            Dim values As Dictionary(Of String, Object) = Nothing
                            Dim skip = False
                            values = BuildValues(map, plan, schema, reader, unit, outcome, skip)
                            If skip Then
                                outcome.RowsOrphanParent += 1
                                Continue While
                            End If

                            If Not ParentsTravelled(reader, links, outcome) Then
                                outcome.RowsOrphanParent += 1
                                Continue While
                            End If

                            For Each column In columnNames
                                cmd.Parameters("@" & column).Value = values(column)
                            Next

                            dump.WriteStatement(map.TargetTable, RenderStatement(sql, columnNames, values))
                            cmd.ExecuteNonQuery()
                            outcome.RowsWritten += 1

                            RecordAssignedId(cmd, map, reader, unit)
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

    ' ---- values ---------------------------------------------------------------------

    Private Function BuildValues(map As TableMap, plan As ColumnPlan, schema As TargetSchema,
                                 reader As AccessTableReader, unit As CaiUnit,
                                 outcome As TableOutcome, ByRef skipRow As Boolean) As Dictionary(Of String, Object)
        Dim values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)

        For Each mapping In plan.Mappings
            Dim target = schema.Column(map.TargetTable, mapping.TargetColumn)

            Select Case mapping.Kind
                Case ColumnSourceKind.UnitId
                    values(mapping.TargetColumn) = unit.IdUnitate

                Case ColumnSourceKind.Constant
                    values(mapping.TargetColumn) = If(mapping.ConstantValue, DBNull.Value)

                Case ColumnSourceKind.ForcedNull
                    values(mapping.TargetColumn) = DBNull.Value
                    outcome.ValuesNulled += 1

                Case ColumnSourceKind.ResolvedClasificatie
                    Dim accessId = Verifier.AsInteger(reader.ValueOrMissing(mapping.AccessColumn))
                    Dim assigned As Integer
                    If accessId.HasValue AndAlso accessId.Value <> 0 AndAlso
                       _clasificatii.TryResolve(accessId.Value, unit.IdUnitate, assigned) Then
                        values(mapping.TargetColumn) = assigned
                    ElseIf mapping.BlockingOnMiss Then
                        Throw New TransferException(
                            $"«{map.TargetTable}»: clasificația Access {If(accessId.HasValue, accessId.Value.ToString(CultureInfo.InvariantCulture), "(lipsă)")} " &
                            $"a unității {unit.IdUnitate} nu se regăsește în «Clasificatii» transferate. " &
                            "Rularea s-a oprit — un rând neclasificat tăcut e mai rău decât un refuz.")
                    Else
                        values(mapping.TargetColumn) = DBNull.Value
                        outcome.ValuesNulled += 1
                    End If

                Case ColumnSourceKind.ResolvedPartener
                    Dim code = Verifier.AsText(reader.ValueOrMissing(mapping.AccessColumn))
                    Dim assigned As Integer
                    If _parteneri.TryResolve(code, unit.IdUnitate, assigned) Then
                        values(mapping.TargetColumn) = assigned
                    ElseIf mapping.BlockingOnMiss Then
                        Throw New TransferException(
                            $"«{map.TargetTable}»: partenerul «{code}» al unității {unit.IdUnitate} " &
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
    ''' True when this Access row belongs to the unit being written.
    ''' </summary>
    ''' <remarks>
    ''' Only the tables that carry IdUnitate can answer this directly - one Forexe file can
    ''' hold rows for several units. The rest reach their unit through their parents, which
    ''' <see cref="ParentsTravelled"/> answers from the keys already written.
    ''' A row belonging to another unit is skipped SILENTLY: that is the normal shape of a
    ''' shared file, not a finding.
    '''
    ''' A NULL IdUnitate is NOT "belongs to whoever is being written". It used to be read
    ''' that way, and the four FX_DDF_REV_SA rows that carry one were then written once
    ''' per selected unit, each time resolving IdClsf against the wrong nomenclator. The
    ''' unit now comes from the parent chain the map declares, and a row that still
    ''' cannot be attributed stops the run - the verifier refuses it first.
    ''' </remarks>
    Private Shared Function BelongsToUnit(reader As AccessTableReader, unit As CaiUnit,
                                          map As TableMap, ownership As UnitOwnership) As Boolean
        Dim rowUnit As Integer
        Select Case UnitOwnership.Resolve(reader, ownership, rowUnit)
            Case UnitScope.Named
                Return rowUnit = unit.IdUnitate
            Case UnitScope.ParentScoped
                ' No IdUnitate column at all: the row reaches its unit through its
                ' parents, and ParentsTravelled answers that.
                Return True
            Case Else
                Throw New TransferException(
                    $"«{map.TargetTable}»: un rând are «IdUnitate» gol și nicio legătură " &
                    "de proprietate care să spună cărei unități îi aparține. Un fișier " &
                    "FOREXE ține rândurile tuturor unităților din DC, deci rândul NU " &
                    "poate fi atribuit unității care se scrie acum.")
        End Select
    End Function

    ''' <summary>
    ''' True when every parent this row points at actually travelled.
    ''' </summary>
    ''' <remarks>
    ''' The parent key sets come from <see cref="WrittenKeys"/>, filled as the parents were
    ''' written - and the topological order guarantees they are complete by now. This is
    ''' what replaces slice 0044's hand-built A-E routing maps.
    ''' </remarks>
    Private Function ParentsTravelled(reader As AccessTableReader,
                                      links As IReadOnlyList(Of ParentLink),
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
                outcome.ValuesNulled += 1
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
                                 reader As AccessTableReader, unit As CaiUnit)
        If map.Feeds = ResolutionTarget.None Then Return

        Dim assigned = CInt(cmd.LastInsertedId)
        If assigned = 0 Then Return

        Select Case map.Feeds
            Case ResolutionTarget.Clasificatii
                Dim accessId = Verifier.AsInteger(reader.ValueOrMissing(map.FeedKeyColumn))
                If accessId.HasValue Then _clasificatii.Add(accessId.Value, unit.IdUnitate, assigned)
                _written.Add(map.TargetTable, assigned)
            Case ResolutionTarget.Parteneri
                Dim code = Verifier.AsText(reader.ValueOrMissing(map.FeedKeyColumn))
                If code.Length > 0 Then _parteneri.Add(code, unit.IdUnitate, assigned)
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

    Private Shared Function PrimaryKeyColumn(schema As TargetSchema, targetTable As String) As String
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

    Private Sub LogPlan(map As TableMap, plan As ColumnPlan, unit As CaiUnit)
        Say($"«{map.TargetTable}» (unitatea {unit.IdUnitate}): se scriu {plan.Mappings.Count} coloane — " &
            String.Join(", ", plan.ColumnNames()))
        If plan.Skipped.Count > 0 Then
            Say($"   sărite: {String.Join("; ", plan.Skipped)}")
        End If
    End Sub

    Private Shared Function Describe(outcome As TableOutcome) As String
        Dim line = $"«{outcome.TargetTable}»: {outcome.RowsWritten} scrise din {outcome.RowsRead} citite"
        If outcome.RowsOtherUnit > 0 Then line &= $", {outcome.RowsOtherUnit} ale altei unități"
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
