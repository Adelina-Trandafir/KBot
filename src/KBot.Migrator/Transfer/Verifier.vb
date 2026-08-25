Imports System.Data.OleDb
Imports System.Globalization
Imports KBot.Common
Imports MySqlConnector

''' <summary>
''' Runs every gate before a single row is written.
''' </summary>
''' <remarks>
''' The gates, in the order they fire. Each one is cheapest-first, so a run that cannot
''' possibly work fails on a query rather than on a transaction:
''' <list type="number">
''' <item>the Access files the selected units need actually exist;</item>
''' <item>the target database exists, or may be created;</item>
''' <item>AVACONT_COMUN exists and its dictionaries cover every classification the run
''' would write - the gate the plan did not have;</item>
''' <item>Unitati covers the selected units, or the tool will populate it;</item>
''' <item>every required column is in each table's column list (the 1364 guard);</item>
''' <item>no text value is wider than the target column (the 1406 guard);</item>
''' <item>every FX_* classification resolves against the Clasificatii about to be written;</item>
''' <item>every Parteneri_Coduri partner code resolves;</item>
''' <item>an insert-only table holds no rows for a selected unit;</item>
''' <item>the write order is derivable, with no cycle.</item>
''' </list>
''' The whole resolution is run DRY - the Access rows are read and the distinct
''' (IdClsf, IdUnitate) set is built - so the operator sees the misses before anything is
''' written.
''' </remarks>
Public NotInheritable Class Verifier

    ''' <summary>
    ''' How many steps <see cref="Run"/> reports, so a progress bar has a scale.
    ''' </summary>
    ''' <remarks>
    ''' A constant rather than a count of anything: the gates are a fixed list, and a bar
    ''' that rescales itself halfway through reads as a bug even when it is not.
    ''' </remarks>
    Public Const StepCount As Integer = 13

    Private ReadOnly _request As TransferRequest
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _progress As Action(Of Integer, Integer, String)
    Private _step As Integer

    Public Sub New(request As TransferRequest, log As Action(Of String),
                   progress As Action(Of Integer, Integer, String))
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        _request = request
        _log = log
        _progress = progress
    End Sub

    ''' <summary>Advances the reported step and names what is happening.</summary>
    Private Sub Step1(label As String)
        _step += 1
        _progress?.Invoke(Math.Min(_step, StepCount), StepCount, label)
    End Sub

    ''' <summary>Runs every gate and returns what it found.</summary>
    Public Function Run(cancel As Threading.CancellationToken) As VerificationReport
        Dim report As New VerificationReport()
        Try
            Say("Verificare pornită.")

            If _request.Units.Count = 0 Then
                report.Add(Finding.UNITATE_LIPSA, FindingClass.Blocant, String.Empty, String.Empty,
                           "Nu a fost aleasă nicio unitate. Transferul nu are ce scrie.")
                Return report
            End If

            If _request.SelectedTables.Count = 0 Then
                report.Add(Finding.TABEL_LIPSA, FindingClass.Blocant, String.Empty, String.Empty,
                           "Nu a fost bifat niciun tabel. Lista goală nu înseamnă «toate».")
                Return report
            End If

            Step1("Se verifică fișierele Access…")
            CheckAccessFiles(report)
            cancel.ThrowIfCancellationRequested()

            Step1("Se caută baza-țintă…")
            Dim exists = _request.Server.DatabaseExists(_request.TargetDatabase)
            If Not exists Then
                If Not _request.CreateDatabaseIfMissing Then
                    report.Add(Finding.TABEL_LIPSA, FindingClass.Blocant, String.Empty, String.Empty,
                               $"Baza «{_request.TargetDatabase}» nu există pe server, iar crearea " &
                               "ei nu a fost cerută.")
                    Return report
                End If
                report.Add(Finding.TABEL_LIPSA, FindingClass.Atentie, String.Empty, String.Empty,
                           $"Baza «{_request.TargetDatabase}» nu există și va fi creată după " &
                           $"«{_request.TemplateDatabase}». Verificările pe țintă se opresc aici — " &
                           "reluați «Verifică» după creare.")
                Return report
            End If

            Using cn = _request.Server.Open(_request.TargetDatabase)
                Step1("Se citește schema țintei…")
                Dim schema = TargetSchema.Load(cn, _request.TargetDatabase)
                Say($"Schema țintei citită: {schema.TableNames.Count} tabele.")

                ' Zero tables is its own kind, not a pile of TABEL_LIPSA. The database was
                ' created empty by somebody, and schema_sync builds the whole structure in
                ' one go - which is what the form offers when it sees this. Reporting it as
                ' 28 missing tables would bury that remedy in noise.
                If schema.TableNames.Count = 0 Then
                    report.Add(Finding.BAZA_FARA_TABELE, FindingClass.Blocant,
                               String.Empty, String.Empty,
                               $"Baza «{_request.TargetDatabase}» există pe server, dar nu are " &
                               "NICIUN tabel. Structura poate fi construită cu «schema_sync».")
                    Say(report.Summary())
                    Return report
                End If

                Dim maps = TableMaps.All().
                    Where(Function(m) _request.IsTableSelected(m.TargetTable)).
                    ToList()

                Step1("Se stabilește proprietatea rândurilor…")
                ' Built HERE and nowhere else. The report carries it out to the form, which
                ' hands the same object to TransferRunner - so the selection the operator
                ' was shown is the selection that gets written, and there is no second
                ' construction site to drift from this one.
                report.Ownership = OwnershipPlan.Build(_request, _log)
                For Each finding In report.Ownership.Findings
                    report.Add(finding)
                Next
                cancel.ThrowIfCancellationRequested()

                Step1("Se verifică existența tabelelor…")
                CheckTablesExist(report, schema, maps)
                cancel.ThrowIfCancellationRequested()

                Step1("Se verifică «Unitati»…")
                CheckUnitati(report, cn, schema)
                cancel.ThrowIfCancellationRequested()

                Step1("Se citesc clasificațiile din Access…")
                Dim clasificatii = ReadClasificatii(report)
                cancel.ThrowIfCancellationRequested()

                Step1("Se verifică dicționarele din baza comună…")
                CheckCommonDictionaries(report, cn, schema, clasificatii)
                cancel.ThrowIfCancellationRequested()

                Step1("Se compun listele de coloane…")
                CheckColumnPlans(report, schema, maps, clasificatii)
                cancel.ThrowIfCancellationRequested()

                Step1("Se verifică lățimile…")
                CheckClasificatiiWidths(report, schema, clasificatii)
                cancel.ThrowIfCancellationRequested()

                Step1("Se verifică cheile părinților…")
                CheckParentKeysTrackable(report, schema, maps)
                cancel.ThrowIfCancellationRequested()

                Step1("Se rezolvă clasificațiile (uscat)…")
                CheckClasificatiiResolution(report, maps, clasificatii)
                cancel.ThrowIfCancellationRequested()

                Step1("Se rezolvă partenerii…")
                CheckParteneriResolution(report, maps)
                cancel.ThrowIfCancellationRequested()

                Step1("Se caută rânduri deja scrise…")
                CheckExistingRows(report, cn, schema, maps)
                cancel.ThrowIfCancellationRequested()

                Step1("Se deduce ordinea de scriere…")
                CheckWriteOrder(report, schema, maps)
                NoteNameMatchedTables(report, maps)
            End Using

            Say(report.Summary())
            Return report

        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("Verifier.Run", ex)
            Throw
        End Try
    End Function

    ' ---- gate 1: the Access files exist -------------------------------------------

    Private Sub CheckAccessFiles(report As VerificationReport)
        For Each unit In _request.Units
            If Not unit.HasUnitFile Then
                report.Add(Finding.FISIER_LIPSA, FindingClass.Blocant, String.Empty, String.Empty,
                           $"Unitatea {unit.IdUnitate} («{unit.NumeUnitate}») nu are fișierul de " &
                           $"nomenclatoare la «{unit.UnitFilePath}».")
            End If
            If Not unit.HasForexeFile Then
                ' Not blocking: cai carries a NULL CaleForexe on two of its thirteen rows,
                ' so a unit with nomenclators and no FX data is a normal shape.
                report.Add(Finding.FISIER_LIPSA, FindingClass.Atentie, String.Empty, String.Empty,
                           $"Unitatea {unit.IdUnitate} («{unit.NumeUnitate}») nu are fișier FOREXE. " &
                           "Nomenclatoarele se transferă, tabelele FX_* nu au ce citi.")
            End If
        Next
    End Sub

    ' ---- gate 2: every selected table exists on the target ------------------------

    Private Shared Sub CheckTablesExist(report As VerificationReport, schema As TargetSchema,
                                        maps As IEnumerable(Of TableMap))
        For Each map In maps
            If Not schema.HasTable(map.TargetTable) Then
                report.Add(Finding.TABEL_LIPSA, FindingClass.Blocant, map.TargetTable, String.Empty,
                           $"Tabelul «{map.TargetTable}» nu există în baza-țintă.")
            End If
        Next
    End Sub

    ' ---- gate 3: Unitati covers the selected units --------------------------------

    Private Sub CheckUnitati(report As VerificationReport, cn As MySqlConnection, schema As TargetSchema)
        If Not schema.HasTable("Unitati") Then
            report.Add(Finding.UNITATE_LIPSA, FindingClass.Blocant, "Unitati", String.Empty,
                       "Tabelul «Unitati» lipsește din baza-țintă, dar patru chei străine " &
                       "arată spre el. Nimic nu poate fi scris.")
            Return
        End If

        Dim present = ExistingUnitIds(cn)
        Dim missing = _request.Units.Where(Function(u) Not present.Contains(u.IdUnitate)).ToList()
        If missing.Count = 0 Then Return

        Dim names = String.Join(", ", missing.Select(Function(u) u.IdUnitate.ToString(CultureInfo.InvariantCulture)))
        If _request.PopulateUnitati Then
            report.Add(Finding.UNITATE_LIPSA, FindingClass.Informativ, "Unitati", String.Empty,
                       $"«Unitati» nu conține încă unitățile {names}. Vor fi scrise de unealtă, " &
                       "înaintea oricărui nomenclator.", missing.Count)
        Else
            report.Add(Finding.UNITATE_LIPSA, FindingClass.Blocant, "Unitati", String.Empty,
                       $"«Unitati» nu conține unitățile {names}, iar unealta nu are voie să le " &
                       "scrie. Clasificatii, Clasificatii_Buget, Parteneri și FX_ORD_TBL au chei " &
                       "străine spre ea, deci nimic nu poate fi scris.", missing.Count)
        End If
    End Sub

    Private Shared Function ExistingUnitIds(cn As MySqlConnection) As HashSet(Of Integer)
        Dim ids As New HashSet(Of Integer)()
        Using cmd As New MySqlCommand("SELECT IdUnitate FROM Unitati", cn)
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    ids.Add(reader.GetInt32(0))
                End While
            End Using
        End Using
        Return ids
    End Function

    ' ---- gate 4: AVACONT_COMUN covers what Clasificatii will compute ---------------

    Private Sub CheckCommonDictionaries(report As VerificationReport, cn As MySqlConnection,
                                        schema As TargetSchema,
                                        rows As List(Of ClasificatieRow))
        Dim crossKeys = schema.CrossSchemaForeignKeys()
        If crossKeys.Count = 0 Then Return

        Dim commonName = crossKeys.
            Select(Function(fk) fk.ParentSchema).
            FirstOrDefault(Function(s) Not String.IsNullOrEmpty(s))
        If String.IsNullOrEmpty(commonName) Then commonName = _request.CommonDatabase

        If Not _request.Server.DatabaseExists(commonName) Then
            report.Add(Finding.BAZA_COMUNA_LIPSA, FindingClass.Blocant, "Clasificatii", String.Empty,
                       $"Baza «{commonName}» nu există pe server, dar {crossKeys.Count} chei " &
                       "străine ale lui «Clasificatii» arată spre ea. Niciun rând de clasificație " &
                       "nu poate fi scris. Crearea bazei-țintă după șablon NU o creează și pe " &
                       "aceasta — e altă bază de date.")
            Return
        End If

        If rows.Count = 0 Then Return

        ' One query per dictionary, not one per row: the distinct set is small (54 rows
        ' produce a handful of distinct titles and source-sectors).
        Dim derived = rows.Select(Function(r) New ClasificatieDerived(r.Capitol, r.Subcapitol, r.Articol, r.Alineat)).ToList()

        CheckDictionary(report, cn, commonName, "DefaArticol", "Articol",
                        derived.Select(Function(d) d.Articol), "Articol")
        CheckDictionary(report, cn, commonName, "DefaTitlu", "Titlu",
                        derived.Select(Function(d) d.Titlu), "Titlu (generat)")
        CheckDictionary(report, cn, commonName, "DefaClsfF", "ClsfF",
                        derived.Select(Function(d) d.ClsfF), "ClsfF (generat)")
        CheckDictionary(report, cn, commonName, "DefaClsfE", "ClsfE",
                        derived.Select(Function(d) d.ClsfE), "ClsfE (generat)")
        CheckDictionary(report, cn, commonName, "DefaSursaSector", "SursaSector",
                        derived.Select(Function(d) d.SS), "SS (generat)")
    End Sub

    Private Sub CheckDictionary(report As VerificationReport, cn As MySqlConnection,
                                commonDatabase As String, table As String, column As String,
                                values As IEnumerable(Of String), label As String)
        Try
            Dim wanted = values.
                Where(Function(v) v IsNot Nothing).
                Distinct(StringComparer.OrdinalIgnoreCase).
                ToList()
            If wanted.Count = 0 Then Return

            Dim present As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim sql = $"SELECT {TargetServer.Quote(column)} FROM " &
                      $"{TargetServer.Quote(commonDatabase)}.{TargetServer.Quote(table)}"
            Using cmd As New MySqlCommand(sql, cn)
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        If Not reader.IsDBNull(0) Then present.Add(reader.GetString(0))
                    End While
                End Using
            End Using

            Dim missing = wanted.Where(Function(v) Not present.Contains(v)).ToList()
            If missing.Count = 0 Then Return

            Dim shown = String.Join(", ", missing.Take(12).Select(Function(v) If(v.Length = 0, "«»", v)))
            Dim more = If(missing.Count > 12, $" și încă {missing.Count - 12}", String.Empty)
            report.Add(Finding.DICTIONAR_COMUN_LIPSA, FindingClass.Blocant, "Clasificatii", label,
                       $"{missing.Count} valori «{label}» nu există în " &
                       $"«{commonDatabase}.{table}.{column}»: {shown}{more}. " &
                       "Cheia străină va refuza rândurile cu 1452.", missing.Count)

        Catch ex As Exception
            ' A dictionary that cannot be read is a finding, not a crash - the table may
            ' simply not be there, and the operator must be told which one.
            GlobalErrorLog.Write("Verifier.CheckDictionary", ex)
            report.Add(Finding.DICTIONAR_COMUN_LIPSA, FindingClass.Blocant, "Clasificatii", label,
                       $"Dicționarul «{commonDatabase}.{table}» nu a putut fi citit: {ex.Message}")
        End Try
    End Sub

    ' ---- gate 5 and 6: required columns and widths --------------------------------

    Private Sub CheckColumnPlans(report As VerificationReport, schema As TargetSchema,
                                 maps As IEnumerable(Of TableMap),
                                 clasificatii As List(Of ClasificatieRow))
        For Each map In maps
            If Not schema.HasTable(map.TargetTable) Then Continue For

            Dim accessColumns = AccessColumnsFor(map)
            Dim plan = ColumnPlan.Build(map, accessColumns, schema)
            report.WrittenColumns(map.TargetTable) = plan.ColumnNames()

            For Each duplicate In plan.DuplicateTargets
                report.Add(Finding.COLOANA_NECORELATA, FindingClass.Blocant, map.TargetTable, duplicate,
                           $"Coloana-țintă «{duplicate}» e revendicată de două ori. " &
                           "O țintă dublă oprește rularea; nu se alege tăcut una dintre ele.")
            Next

            ' The 1364 guard, checked on the RESULT rather than on the routes that led to it.
            Dim written As New HashSet(Of String)(plan.ColumnNames(), StringComparer.OrdinalIgnoreCase)
            For Each required In schema.RequiredColumns(map.TargetTable)
                If written.Contains(required.Name) Then Continue For
                report.Add(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, map.TargetTable, required.Name,
                           $"«{required.Name}» este NOT NULL, fără implicit și necompletată de " &
                           "server, dar nu se află în lista de coloane scrise. MariaDB ar " &
                           "răspunde 1364.")
            Next

            ' Rule 1 at the writing end, checked on the RESULT like the 1364 guard above.
            ' The target's IdClsf is the id MariaDB assigned; the Access one is local to
            ' the file. FX_DDF_REV_SA/SB and FX_ORD_TBL resolve it explicitly, so only a
            ' name-matched table can reach here - and only when the target really has the
            ' column, which is why this asks the plan rather than the catalogue.
            For Each mapping In plan.Mappings
                If mapping.Kind <> ColumnSourceKind.AccessColumn Then Continue For
                If Not String.Equals(mapping.TargetColumn, "IdClsf", StringComparison.OrdinalIgnoreCase) Then Continue For

                report.Add(Finding.CLASIFICATIE_NECORELATA, FindingClass.Atentie, map.TargetTable,
                           mapping.TargetColumn,
                           $"«IdClsf» primește direct coloana Access «{mapping.AccessColumn}», pe " &
                           "potrivire după nume. Pe țintă «IdClsf» e id-ul atribuit de MariaDB, " &
                           "iar în Access e id-ul local al fișierului — Regula 1 spune că nu " &
                           "călătorește. Tabelul are nevoie de o corelație explicită, ca la " &
                           "«FX_DDF_REV_SA».")
            Next

            If Not schema.CanUpsert(map.TargetTable, plan.ColumnNames()) AndAlso Not map.InsertOnly Then
                report.Add(Finding.FARA_CHEIE_UPSERT, FindingClass.Atentie, map.TargetTable, String.Empty,
                           $"Coloanele scrise nu acoperă nicio cheie unică a lui «{map.TargetTable}», " &
                           "deci «ON DUPLICATE KEY UPDATE» nu are pe ce să se potrivească și o a " &
                           "doua rulare ar dubla rândurile.")
            End If

            If map.InsertOnly Then
                report.Add(Finding.FARA_CHEIE_UPSERT, FindingClass.Informativ, map.TargetTable, String.Empty,
                           map.Note)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Text values wider than the target column. The 1406 guard.
    ''' </summary>
    ''' <remarks>
    ''' Only Clasificatii narrows today - Access WChar(50) into varchar(5)/varchar(2). Every
    ''' sample value fits exactly, but nothing on the Access side enforces it, and under
    ''' strict mode an over-long value is an error rather than a truncation. The plan's
    ''' §5.3 guard covers MISSING REQUIRED columns, not TOO-WIDE values.
    ''' </remarks>
    Private Sub CheckClasificatiiWidths(report As VerificationReport, schema As TargetSchema,
                                        rows As List(Of ClasificatieRow))
        If rows.Count = 0 Then Return
        If Not schema.HasTable("Clasificatii") Then Return

        Dim checks = New(Name As String, Reader As Func(Of ClasificatieRow, String))() {
            ("Capitol", Function(r) r.Capitol),
            ("Subcapitol", Function(r) r.Subcapitol),
            ("Articol", Function(r) r.Articol),
            ("Alineat", Function(r) r.Alineat),
            ("Denumire", Function(r) r.Denumire)
        }

        For Each check In checks
            Dim column = schema.Column("Clasificatii", check.Name)
            If column Is Nothing OrElse Not column.CharacterMaxLength.HasValue Then Continue For

            Dim limit = column.CharacterMaxLength.Value
            Dim over = rows.
                Select(check.Reader).
                Where(Function(v) v IsNot Nothing AndAlso v.Length > limit).
                Distinct(StringComparer.Ordinal).
                ToList()
            If over.Count = 0 Then Continue For

            report.Add(Finding.COLOANA_PREA_INGUSTA, FindingClass.Blocant, "Clasificatii", check.Name,
                       $"{over.Count} valori depășesc {limit} caractere pe «{check.Name}» " &
                       $"({column.ColumnType}): {String.Join(", ", over.Take(6))}. " &
                       "MariaDB ar răspunde 1406.", over.Count)
        Next

        ' Denumire is NOT NULL on the target and nullable in Access.
        Dim denumire = schema.Column("Clasificatii", "Denumire")
        If denumire IsNot Nothing AndAlso Not denumire.IsNullable Then
            Dim empty = rows.Where(Function(r) String.IsNullOrEmpty(r.Denumire)).Count
            If empty > 0 Then
                report.Add(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, "Clasificatii", "Denumire",
                           $"{empty} clasificații au «Denumire» goală, dar coloana e NOT NULL pe țintă.",
                           empty)
            End If
        End If
    End Sub

    ' ---- gate 7 and 8: the resolutions, run dry -----------------------------------

    ''' <summary>
    ''' Every FX_* classification must resolve against the Clasificatii about to be written.
    ''' </summary>
    ''' <remarks>
    ''' Run once per FILE now, not once per selected unit. The row's unit comes from
    ''' <see cref="OwnershipPlan.Decide"/> - the same function the writer asks - so a row
    ''' is checked against ITS nomenclator and against no other. Reading it once per unit
    ''' is what made the same rows produce a finding N times and, worse, produce mirrored
    ''' findings that contradicted each other on real data.
    ''' </remarks>
    Private Sub CheckClasificatiiResolution(report As VerificationReport,
                                            maps As IEnumerable(Of TableMap),
                                            clasificatii As List(Of ClasificatieRow))
        Dim ownership = report.Ownership
        If ownership Is Nothing Then Return

        ' What the transferred nomenclator WOULD cover.
        Dim covered As New HashSet(Of String)(StringComparer.Ordinal)
        For Each row In clasificatii
            covered.Add($"{row.AccessIdClsf}|{row.IdUnitate}")
        Next

        Dim consumers = maps.
            Where(Function(m) m.Source = SourceFile.ForexeFile AndAlso
                              m.Derived.Any(Function(d) d.Kind = ColumnSourceKind.ResolvedClasificatie)).
            ToList()
        If consumers.Count = 0 Then Return

        For Each pass In ForexePasses()
            Using cn = AccessProvider.Open(pass.Path, _request.ForexeFilePassword)
                For Each map In consumers
                    Dim realName = AccessSchema.ResolveTableName(cn, map.AccessTable)
                    If realName Is Nothing Then Continue For

                    Dim mapping = map.Derived.First(Function(d) d.Kind = ColumnSourceKind.ResolvedClasificatie)
                    Dim misses As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
                    Dim withoutUnit = 0

                    Using reader = AccessSchema.OpenReader(cn, realName)
                        While reader.Read()
                            Dim verdict = ownership.Decide(map, reader, pass.Units)
                            If verdict.Disposition <> RowDisposition.Travels Then Continue While

                            If Not verdict.HasUnit Then
                                ' Travelling is enough to WRITE a row, but not to resolve a
                                ' classification: that needs one unit by name. No consumer
                                ' is in this state today, and the writer refuses it too.
                                withoutUnit += 1
                                Continue While
                            End If

                            Dim idClsf = AsInteger(reader.ValueOrMissing(mapping.AccessColumn))
                            If Not idClsf.HasValue OrElse idClsf.Value = 0 Then Continue While

                            Dim key = $"{idClsf.Value}|{verdict.IdUnitate}"
                            If covered.Contains(key) Then Continue While

                            Dim count As Integer
                            misses.TryGetValue(key, count)
                            misses(key) = count + 1
                        End While
                    End Using

                    If withoutUnit > 0 Then
                        report.Add(Finding.UNITATE_NEDETERMINATA, FindingClass.Blocant,
                                   map.TargetTable, UnitOwnership.UnitColumn,
                                   $"{withoutUnit} rânduri călătoresc, dar nu numesc UNA " &
                                   "singură unitate, iar clasificația lor se rezolvă pe " &
                                   "nomenclatorul unei unități. Un fișier FOREXE ține " &
                                   "rândurile tuturor unităților din DC, deci nu pot fi " &
                                   "atribuite uneia alese la întâmplare — s-ar rezolva pe " &
                                   "nomenclatorul greșit.", withoutUnit)
                    End If

                    If misses.Count = 0 Then Continue For

                    ' Grouped by unit, because "unit 75 is missing 141" and "unit 76 is
                    ' missing 141" are different facts with different remedies.
                    For Each group In misses.GroupBy(Function(kv) kv.Key.Split("|"c)(1)).
                                             OrderBy(Function(g) g.Key, StringComparer.Ordinal)
                        Dim total = group.Sum(Function(kv) kv.Value)
                        Dim shown = String.Join(", ", group.Take(10).Select(Function(kv) kv.Key.Split("|"c)(0)))
                        report.Add(Finding.CLASIFICATIE_NEREZOLVATA,
                                   If(mapping.BlockingOnMiss, FindingClass.Blocant, FindingClass.Atentie),
                                   map.TargetTable, mapping.TargetColumn,
                                   $"Unitatea {group.Key}: {group.Count()} clasificații de pe " &
                                   $"{total} rânduri nu se regăsesc în «Clasificatii» care se " &
                                   $"transferă (IdClsf: {shown}).", total)
                    Next
                Next
            End Using
        Next
    End Sub

    ''' <summary>
    ''' One entry per distinct Forexe FILE, with the selected units pointing at it.
    ''' </summary>
    ''' <remarks>
    ''' The verifier's half of <c>TransferRunner.FilePasses</c>. Eleven of the thirteen
    ''' <c>cai</c> rows name the same file, so this is one pass where the old code did
    ''' eleven - and the eleven were the reason the same finding appeared "de două ori".
    ''' </remarks>
    Private Function ForexePasses() As List(Of ForexePass)
        Dim byPath As New Dictionary(Of String, ForexePass)(StringComparer.OrdinalIgnoreCase)
        Dim ordered As New List(Of ForexePass)()

        For Each unit In _request.Units
            If Not unit.HasForexeFile Then Continue For
            Dim pass As ForexePass = Nothing
            If Not byPath.TryGetValue(unit.ForexeFilePath, pass) Then
                pass = New ForexePass(unit.ForexeFilePath)
                byPath(unit.ForexeFilePath) = pass
                ordered.Add(pass)
            End If
            pass.Units.Add(unit)
        Next

        Return ordered
    End Function

    ''' <summary>
    ''' Every partner code <c>Parteneri_Coduri</c> needs must exist in the
    ''' <c>Parteneri</c> being written from the same file.
    ''' </summary>
    ''' <remarks>
    ''' <c>Parteneri_Coduri.IdPartener</c> is NOT NULL with a foreign key, and Access only
    ''' carries the partner CODE - so a code with no partner cannot be written at all.
    ''' Access's own IdPartener column (7605..7621) is NOT a usable source: those are ids
    ''' the server assigned on an earlier sync, the IdClsfPY trap wearing another name.
    ''' </remarks>
    Private Sub CheckParteneriResolution(report As VerificationReport, maps As IEnumerable(Of TableMap))
        Dim consumers = maps.
            Where(Function(m) m.Source = SourceFile.UnitFile AndAlso
                              m.Derived.Any(Function(d) d.Kind = ColumnSourceKind.ResolvedPartener)).
            ToList()
        If consumers.Count = 0 Then Return

        For Each unit In _request.Units
            If Not unit.HasUnitFile Then Continue For

            Try
                Using cn = AccessProvider.Open(unit.UnitFilePath, _request.UnitFilePassword)
                    Dim codes = ReadPartnerCodes(cn)

                    For Each map In consumers
                        Dim realName = AccessSchema.ResolveTableName(cn, map.AccessTable)
                        If realName Is Nothing Then Continue For

                        Dim mapping = map.Derived.First(Function(d) d.Kind = ColumnSourceKind.ResolvedPartener)
                        Dim misses As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

                        Using reader = AccessSchema.OpenReader(cn, realName)
                            While reader.Read()
                                Dim code = AsText(reader.ValueOrMissing(mapping.AccessColumn))
                                If code.Length = 0 OrElse codes.Contains(code) Then Continue While
                                Dim count As Integer
                                misses.TryGetValue(code, count)
                                misses(code) = count + 1
                            End While
                        End Using

                        If misses.Count = 0 Then Continue For

                        Dim total = misses.Values.Sum()
                        report.Add(Finding.PARTENER_NEREZOLVAT,
                                   If(mapping.BlockingOnMiss, FindingClass.Blocant, FindingClass.Atentie),
                                   map.TargetTable, mapping.TargetColumn,
                                   $"Unitatea {unit.IdUnitate}: {misses.Count} coduri de partener de pe " &
                                   $"{total} rânduri nu se regăsesc în «Parteneri» care se transferă " &
                                   $"({String.Join(", ", misses.Keys.Take(10))}).", total)
                    Next
                End Using
            Catch ex As Exception
                GlobalErrorLog.Write("Verifier.CheckParteneriResolution", ex)
                report.Add(Finding.PARTENER_NEREZOLVAT, FindingClass.Blocant, "Parteneri_Coduri", String.Empty,
                           $"Partenerii unității {unit.IdUnitate} nu au putut fi citiți: {ex.Message}")
            End Try
        Next
    End Sub

    Private Shared Function ReadPartnerCodes(cn As OleDbConnection) As HashSet(Of String)
        Dim codes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim realName = AccessSchema.ResolveTableName(cn, "Parteneri")
        If realName Is Nothing Then Return codes
        Using reader = AccessSchema.OpenReader(cn, realName)
            While reader.Read()
                Dim code = AsText(reader.ValueOrMissing("CodPartener"))
                If code.Length > 0 Then codes.Add(code)
            End While
        End Using
        Return codes
    End Function

    ' ---- gate 9: an insert-only table already holds rows --------------------------

    Private Sub CheckExistingRows(report As VerificationReport, cn As MySqlConnection,
                                  schema As TargetSchema, maps As IEnumerable(Of TableMap))
        For Each map In maps
            If Not schema.HasTable(map.TargetTable) Then Continue For
            If String.IsNullOrEmpty(map.UnitScopeColumn) Then Continue For
            If schema.Column(map.TargetTable, map.UnitScopeColumn) Is Nothing Then Continue For

            For Each unit In _request.Units
                Dim sql = $"SELECT COUNT(*) FROM {TargetServer.Quote(map.TargetTable)} " &
                          $"WHERE {TargetServer.Quote(map.UnitScopeColumn)} = @unit"
                Dim count As Long
                Using cmd As New MySqlCommand(sql, cn)
                    cmd.Parameters.AddWithValue("@unit", unit.IdUnitate)
                    count = Convert.ToInt64(cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                End Using
                If count = 0 Then Continue For

                If map.InsertOnly Then
                    report.Add(Finding.RANDURI_EXISTENTE, FindingClass.Blocant, map.TargetTable,
                               map.UnitScopeColumn,
                               $"«{map.TargetTable}» are deja {count} rânduri pentru unitatea " &
                               $"{unit.IdUnitate}. Tabelul nu poate face upsert (D8), deci o a " &
                               "doua rulare ar dubla rândurile. Goliți-le sau alegeți altă unitate.",
                               CInt(Math.Min(count, Integer.MaxValue)))
                Else
                    report.Add(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, map.TargetTable,
                               map.UnitScopeColumn,
                               $"«{map.TargetTable}» are deja {count} rânduri pentru unitatea " &
                               $"{unit.IdUnitate}. Vor fi actualizate prin upsert.",
                               CInt(Math.Min(count, Integer.MaxValue)))
                End If
            Next
        Next
    End Sub

    ' ---- gate: a parent must be able to record the key its children are checked on ----

    ''' <summary>
    ''' Every table in the write set that is a PARENT of another table in the write set
    ''' must be able to record the key value its children will be checked against.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Decision D13, written because of a defect that ran in total silence.
    ''' <c>TransferRunner.PrimaryKeyColumn</c> returns Nothing for a multi-column primary
    ''' key. <c>FX_DDF</c> was <c>PRIMARY KEY (IDDF, CUAL)</c>, so <c>RecordPrimaryKey</c>
    ''' recorded nothing, <c>WrittenKeys.Tracks("FX_DDF")</c> stayed False, and the first
    ''' line of <c>ParentsTravelled</c> - <c>If Not _written.Tracks(...) Then Continue For</c>
    ''' - dropped the FX_DDF link for EVERY child pointing at it. The orphan check for
    ''' FX_DDF_REV, FX_ORD and tblDocFund_Revizii_Clsf was disarmed, and the run died much
    ''' later on <c>1452</c> naming the child rather than the cause.
    ''' </para>
    ''' <para>
    ''' The operator has since made the key <c>IDDF</c> alone, so the immediate cause is
    ''' gone. This gate exists because a parent whose keys are silently not tracked must
    ''' never again be a silent condition - it is cheap, it asks the LIVE schema, and it
    ''' would have caught the original before the first row was written.
    ''' </para>
    ''' <para>
    ''' A table that FEEDS a resolution map is exempt: its key is assigned by the server and
    ''' recorded from <c>LAST_INSERT_ID()</c>, never from the primary-key column.
    ''' </para>
    ''' </remarks>
    Private Shared Sub CheckParentKeysTrackable(report As VerificationReport, schema As TargetSchema,
                                                maps As IReadOnlyList(Of TableMap))
        Dim inScope As New Dictionary(Of String, TableMap)(StringComparer.OrdinalIgnoreCase)
        For Each map In maps
            inScope(map.TargetTable) = map
        Next

        ' parent ▸ the children in scope that point at it, with the constraint that does it.
        Dim needed As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
        For Each map In maps
            If Not schema.HasTable(map.TargetTable) Then Continue For

            For Each fk In schema.ForeignKeysOf(map.TargetTable)
                If fk.IsCrossSchema(schema.SchemaName) Then Continue For
                If String.Equals(fk.ChildTable, fk.ParentTable, StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not inScope.ContainsKey(fk.ParentTable) Then Continue For

                Dim children As List(Of String) = Nothing
                If Not needed.TryGetValue(fk.ParentTable, children) Then
                    children = New List(Of String)()
                    needed(fk.ParentTable) = children
                End If
                children.Add($"«{fk.ChildTable}».«{fk.ChildColumn}» ({fk.ConstraintName})")
            Next
        Next

        For Each entry In needed.OrderBy(Function(e) e.Key, StringComparer.OrdinalIgnoreCase)
            Dim parentMap = inScope(entry.Key)
            If parentMap.Feeds <> ResolutionTarget.None Then Continue For
            If Not schema.HasTable(entry.Key) Then Continue For
            If TransferRunner.PrimaryKeyColumn(schema, entry.Key) IsNot Nothing Then Continue For

            Dim primary = schema.UniqueKeysOf(entry.Key).FirstOrDefault(Function(k) k.IsPrimary)
            Dim keyText = If(primary Is Nothing,
                             "nu are deloc cheie primară",
                             $"are cheia primară compusă ({String.Join(", ", primary.Columns)})")

            report.Add(Finding.CHEIE_PARINTE_NEURMARITA, FindingClass.Blocant, entry.Key, String.Empty,
                       $"«{entry.Key}» {keyText}, deci unealta nu poate ține minte ce chei a " &
                       $"scris pentru el — iar {entry.Value.Count} copii din transfer sunt " &
                       $"verificați tocmai pe ele: {String.Join(", ", entry.Value.Take(6))}. " &
                       "Verificarea «părintele a călătorit?» s-ar dezarma TĂCUT pentru toți, " &
                       "iar rularea ar muri mai târziu cu 1452 pe copil, nu pe cauză. " &
                       "Cheia primară a părintelui trebuie să fie pe o singură coloană.",
                       entry.Value.Count)
        Next
    End Sub

    ' ---- gate 10: the write order --------------------------------------------------

    Private Shared Sub CheckWriteOrder(report As VerificationReport, schema As TargetSchema,
                                       maps As IReadOnlyList(Of TableMap))
        Try
            report.WriteOrder = WriteOrder.Derive(schema, maps.Select(Function(m) m.TargetTable))
        Catch ex As WriteOrderCycleException
            report.Add(Finding.CICLU_TABELE, FindingClass.Blocant, String.Empty, String.Empty, ex.Message)
        End Try
    End Sub

    Private Shared Sub NoteNameMatchedTables(report As VerificationReport, maps As IEnumerable(Of TableMap))
        Dim byName = maps.Where(Function(m) m.NameMatchOnly).Select(Function(m) m.TargetTable).ToList()
        If byName.Count = 0 Then Return

        report.Add(Finding.POTRIVIRE_DUPA_NUME, FindingClass.Atentie, String.Empty, String.Empty,
                   $"{byName.Count} tabele călătoresc pe potrivire după NUME, nu pe o corelație " &
                   $"citită dintr-un document de mapare: {String.Join(", ", byName)}. " &
                   "«MAPARE_ACCESS_MARIADB.md» descrie coloană cu coloană doar familiile DDF și ORD.",
                   byName.Count)

        For Each map In maps
            Dim nulled = map.Derived.Where(Function(d) d.Kind = ColumnSourceKind.ForcedNull).ToList()
            For Each mapping In nulled
                report.Add(Finding.PARTENER_NULAT, FindingClass.Informativ, map.TargetTable,
                           mapping.TargetColumn,
                           $"«{mapping.TargetColumn}» se scrie NULL pe toate rândurile " &
                           "(«Parteneri» nu se corelează pentru tabelele FX_*).")
            Next
        Next
    End Sub

    ' ---- reading the nomenclator, once, for every gate that needs it --------------

    Private Function ReadClasificatii(report As VerificationReport) As List(Of ClasificatieRow)
        Dim rows As New List(Of ClasificatieRow)()
        For Each unit In _request.Units
            If Not unit.HasUnitFile Then Continue For
            Try
                Using cn = AccessProvider.Open(unit.UnitFilePath, _request.UnitFilePassword)
                    Dim realName = AccessSchema.ResolveTableName(cn, "Clasificatii")
                    If realName Is Nothing Then
                        report.Add(Finding.TABEL_LIPSA, FindingClass.Blocant, "Clasificatii", String.Empty,
                                   $"Fișierul unității {unit.IdUnitate} nu conține tabelul «Clasificatii».")
                        Continue For
                    End If

                    Using reader = AccessSchema.OpenReader(cn, realName)
                        While reader.Read()
                            Dim id = AsInteger(reader.ValueOrMissing("IDClsf"))
                            If Not id.HasValue Then Continue While
                            rows.Add(New ClasificatieRow(
                                id.Value, unit.IdUnitate,
                                AsText(reader.ValueOrMissing("Capitol")),
                                AsText(reader.ValueOrMissing("Subcapitol")),
                                AsText(reader.ValueOrMissing("Articol")),
                                AsText(reader.ValueOrMissing("Alineat")),
                                AsText(reader.ValueOrMissing("Denumire"))))
                        End While
                    End Using
                End Using
            Catch ex As Exception
                GlobalErrorLog.Write("Verifier.ReadClasificatii", ex)
                report.Add(Finding.FISIER_LIPSA, FindingClass.Blocant, "Clasificatii", String.Empty,
                           $"Nomenclatorul unității {unit.IdUnitate} nu a putut fi citit: {ex.Message}")
            End Try
        Next
        report.RowCounts("Clasificatii") = rows.Count
        Return rows
    End Function

    Private Sub Say(message As String)
        _log?.Invoke(message)
    End Sub

    Friend Shared Function AsInteger(value As Object) As Integer?
        If value Is Nothing OrElse value Is DBNull.Value Then Return Nothing
        Try
            Return Convert.ToInt32(value, CultureInfo.InvariantCulture)
        Catch ex As Exception
            GlobalErrorLog.Write("Verifier.AsInteger", ex)
            Return Nothing
        End Try
    End Function

    Friend Shared Function AsText(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
        Return Convert.ToString(value, CultureInfo.InvariantCulture)
    End Function

    Private Function AccessColumnsFor(map As TableMap) As List(Of String)
        If map.Source = SourceFile.Derived Then Return New List(Of String)()

        For Each unit In _request.Units
            Dim path = If(map.Source = SourceFile.UnitFile, unit.UnitFilePath, unit.ForexeFilePath)
            Dim password = If(map.Source = SourceFile.UnitFile, _request.UnitFilePassword, _request.ForexeFilePassword)
            If String.IsNullOrEmpty(path) OrElse Not IO.File.Exists(path) Then Continue For

            Try
                Using cn = AccessProvider.Open(path, password)
                    Dim realName = AccessSchema.ResolveTableName(cn, map.AccessTable)
                    If realName Is Nothing Then Continue For
                    Return AccessSchema.Columns(cn, realName).Select(Function(c) c.Name).ToList()
                End Using
            Catch ex As Exception
                GlobalErrorLog.Write("Verifier.AccessColumnsFor", ex)
            End Try
        Next

        Return New List(Of String)()
    End Function

End Class

''' <summary>One Forexe file, with the selected units that point at it.</summary>
Friend NotInheritable Class ForexePass

    Public Sub New(path As String)
        Me.Path = path
        Units = New List(Of CaiUnit)()
    End Sub

    Public ReadOnly Property Path As String
    Public ReadOnly Property Units As List(Of CaiUnit)

End Class

''' <summary>One Access classification, with the unit it belongs to.</summary>
''' <remarks>
''' The unit is NOT read from the row - Access Clasificatii has no IdUnitate column at
''' all. It comes from the cai row whose file is open.
''' </remarks>
Public NotInheritable Class ClasificatieRow

    Public Sub New(accessIdClsf As Integer, idUnitate As Integer, capitol As String,
                   subcapitol As String, articol As String, alineat As String, denumire As String)
        Me.AccessIdClsf = accessIdClsf
        Me.IdUnitate = idUnitate
        Me.Capitol = capitol
        Me.Subcapitol = subcapitol
        Me.Articol = articol
        Me.Alineat = alineat
        Me.Denumire = denumire
    End Sub

    Public ReadOnly Property AccessIdClsf As Integer
    Public ReadOnly Property IdUnitate As Integer
    Public ReadOnly Property Capitol As String
    Public ReadOnly Property Subcapitol As String
    Public ReadOnly Property Articol As String
    Public ReadOnly Property Alineat As String
    Public ReadOnly Property Denumire As String

End Class
