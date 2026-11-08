Imports System.Globalization
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>
''' Stage 2, step «Plati» (slice AV02-02): the Access <c>OPME</c>, <c>OPuri</c> and <c>PlatiFacturi</c> become <c>TZ_OPME</c>, <c>TZ_OPuri</c> and <c>TZ_PlatiFacturi</c> of the
''' subunit of the unit, and the flag that made the OP numbers common (<c>Scheme._OP9.C6</c>) becomes the setting <c>PlajaComunaOPuri</c> of that subunit in <c>TZ_Settings</c>.
''' </summary>
''' <remarks>
''' <para>
''' The history is moved AS IT IS: amounts, differences, dates, document numbers and the Access keys (kept in the columns ending in «Access»). Nothing is recalculated and
''' the reception protection of «Contari» is NOT applied here. What cannot be tied to the target is left out and named, never half written.
''' </para>
''' <para>
''' The order is found through the PAYMENT operation: <c>PlatiFacturi.IDOP</c> found no order in the analysed copy, but <c>PlatiFacturi.IdPlata = OPuri.Oper</c> matched with equal
''' amounts, so an allocation takes the order whose <c>Oper</c> is its payment operation when that is unique; an ambiguous case is reported and the allocation stays without an order.
''' </para>
''' <para>
''' Needs the step «Operatii» to have run first (the payment and evidence operations are found by <c>TZ_Oper.IdOperatieAccess</c>). A repeat adds only what is missing.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcPlatiStep
    Inherits AvcStep

    Private Shared ReadOnly Tables As String() = {"TZ_OPME", "TZ_OPuri", "TZ_PlatiFacturi", "TZ_Settings"}

    Private _set As AvcPlatiSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "Plati"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 2 - Introducere date"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Plăți ale facturilor și ordine de plată (OP, OPME, plaja comună de numere)"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcPlatiReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return Tables
        End Get
    End Property

    Public Overrides ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return {"Subunitati", "Operatii"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcPlatiReader.Read(k_context)
        AccessRows = _set.Alocari.Count
        RowsToWrite = _set.Alocari.Count
    End Sub

    ' ── what the target holds ───────────────────────────────────────────────────

    Private NotInheritable Class Lookups
        ''' <summary>IdUnitate (Access) ▸ subunit.</summary>
        Public ReadOnly Property Subunits As New Dictionary(Of Integer, Integer)()
        ''' <summary>"IdUnitate|Access operation" ▸ IdOper.</summary>
        Public ReadOnly Property Opers As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        ''' <summary>"IdUnitate|CodPartener" ▸ IdPartener.</summary>
        Public ReadOnly Property Parteneri As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        ''' <summary>IdOper (evidence) ▸ IdPrimita, from the links of the migrated invoices.</summary>
        Public ReadOnly Property PrimitaDupaOper As New Dictionary(Of Integer, Integer)()
        Public ReadOnly Property Primite As New HashSet(Of Integer)()
        ''' <summary>"subunit|IdOPAccess" ▸ IdOP, "subunit|IdOPMEAccess" ▸ IdOPME, "IdPlata|IdAlocareAccess" present.</summary>
        Public ReadOnly Property OpuriAcces As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        Public ReadOnly Property OpmeAcces As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        Public ReadOnly Property AlocariAcces As New HashSet(Of String)(StringComparer.Ordinal)
        Public ReadOnly Property OpPeOper As New HashSet(Of Integer)()
        Public ReadOnly Property Setari As New HashSet(Of Integer)()
        Public Property SubunitatiToate As Integer
    End Class

    Private Shared Function K2(k_a As Integer, k_b As Integer) As String
        Return k_a.ToString(CultureInfo.InvariantCulture) & "|" & k_b.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Shared Sub ReadPairs(k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_sql As String, k_each As Action(Of MySqlDataReader))
        Using k_cmd As New MySqlCommand(k_sql, k_cn, k_transaction)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_each(k_reader)
                End While
            End Using
        End Using
    End Sub

    Private Shared Function LoadLookups(k_cn As MySqlConnection, k_transaction As MySqlTransaction, Optional k_withTargets As Boolean = True) As Lookups
        Try
            Dim k_result As New Lookups()
            For Each k_pair In AvcSubunitStore.UnitMap(k_cn, k_transaction)
                k_result.Subunits(k_pair.Key) = k_pair.Value.IdSubunitate
            Next
            ReadPairs(k_cn, k_transaction, "SELECT o.`IdOper`, o.`IdOperatieAccess`, s.`IdUnitateAccess` FROM `TZ_Oper` o JOIN `TZ_Documente` d ON d.`IdDocument` = o.`IdDocument` " &
                      "JOIN `TZ_NoteC` n ON n.`IdNota` = d.`IdNota` JOIN `TZ_Subunitati` s ON s.`IdSubunitate` = n.`IdSubunitate` WHERE o.`IdOperatieAccess` IS NOT NULL AND s.`IdUnitateAccess` IS NOT NULL",
                      Sub(k_r) k_result.Opers(K2(k_r.GetInt32(2), k_r.GetInt32(1))) = k_r.GetInt32(0))
            ReadPairs(k_cn, k_transaction, "SELECT `IdPartener`, `IdUnitate`, `CodPartener` FROM `Parteneri`",
                      Sub(k_r) k_result.Parteneri(k_r.GetInt32(1).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetString(2)) = k_r.GetInt32(0))
            If AvcSql.TableExists(k_cn, k_cn.Database, "EF_PrimiteOperatii", k_transaction) Then
                ReadPairs(k_cn, k_transaction, "SELECT `IdOperatie`, `IdPrimita` FROM `EF_PrimiteOperatii` WHERE `IdOperatie` > 0",
                          Sub(k_r) k_result.PrimitaDupaOper.TryAdd(k_r.GetInt32(0), k_r.GetInt32(1)))
                ReadPairs(k_cn, k_transaction, "SELECT `IdPrimita` FROM `EF_Primite`", Sub(k_r) k_result.Primite.Add(k_r.GetInt32(0)))
            End If
            If k_withTargets Then
                ReadPairs(k_cn, k_transaction, "SELECT `IdOP`, `IdSubunitate`, `IdOPAccess` FROM `TZ_OPuri` WHERE `IdOPAccess` IS NOT NULL",
                          Sub(k_r) k_result.OpuriAcces(K2(k_r.GetInt32(1), k_r.GetInt32(2))) = k_r.GetInt32(0))
                ReadPairs(k_cn, k_transaction, "SELECT `IdOPME`, `IdSubunitate`, `IdOPMEAccess` FROM `TZ_OPME` WHERE `IdOPMEAccess` IS NOT NULL",
                          Sub(k_r) k_result.OpmeAcces(K2(k_r.GetInt32(1), k_r.GetInt32(2))) = k_r.GetInt32(0))
                ReadPairs(k_cn, k_transaction, "SELECT `IdPlata`, `IdAlocareAccess` FROM `TZ_PlatiFacturi` WHERE `IdAlocareAccess` IS NOT NULL",
                          Sub(k_r) k_result.AlocariAcces.Add(K2(k_r.GetInt32(0), k_r.GetInt32(1))))
                ReadPairs(k_cn, k_transaction, "SELECT `IdOper` FROM `TZ_OPuri`", Sub(k_r) k_result.OpPeOper.Add(k_r.GetInt32(0)))
                ReadPairs(k_cn, k_transaction, "SELECT `IdSubunitate` FROM `TZ_Settings` WHERE `SettingKey` = 'PlajaComunaOPuri' AND `IdSubunitate` IS NOT NULL",
                          Sub(k_r) k_result.Setari.Add(k_r.GetInt32(0)))
            End If
            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_Subunitati`", k_cn, k_transaction)
                k_result.SubunitatiToate = CInt(Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture))
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlatiStep.LoadLookups", ex)
            Throw
        End Try
    End Function

    ' ── the plan (what would be written; the same lookups give the same plan) ───

    Private NotInheritable Class PlanOp
        Public Property Row As AvcOpRow
        Public Property IdSubunitate As Integer
        Public Property IdOper As Integer
        Public Property IdPartener As Integer?
    End Class

    Private NotInheritable Class PlanAlocare
        Public Property Row As AvcAlocareRow
        Public Property IdPlata As Integer
        Public Property IdEvidentiere As Integer?
        Public Property IdPrimita As Integer?
        Public Property OpAccess As Integer?   ' the Access IDOP of the order this allocation joins (found through the payment operation)
    End Class

    Private NotInheritable Class Plan
        Public ReadOnly Property Opme As New List(Of AvcOpmeRow)()
        Public ReadOnly Property Opuri As New List(Of PlanOp)()
        Public ReadOnly Property Alocari As New List(Of PlanAlocare)()
        Public ReadOnly Property Plaje As New List(Of AvcPlajaRow)()
        Public ReadOnly Property Omise As New SortedDictionary(Of String, List(Of String))(StringComparer.Ordinal)
        Public ReadOnly Property FaraSubunitate As New SortedSet(Of Integer)()
        Public Property AlocariFaraOp As Integer
        Public Property AlocariCuOpAmbiguu As Integer
        Public Property FaraPartener As Integer

        Public Sub Omit(k_reason As String, k_what As String)
            Dim k_list As List(Of String) = Nothing
            If Not Omise.TryGetValue(k_reason, k_list) Then
                k_list = New List(Of String)()
                Omise(k_reason) = k_list
            End If
            k_list.Add(k_what)
        End Sub
    End Class

    Private Function BuildPlan(k_look As Lookups, k_context As AvcContext) As Plan
        Dim k_plan As New Plan()
        Dim k_opmeOk As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_row In _set.Opme
            If Not k_look.Subunits.ContainsKey(k_row.IdUnitate) Then
                k_plan.FaraSubunitate.Add(k_row.IdUnitate)
                Continue For
            End If
            k_plan.Opme.Add(k_row)
            k_opmeOk.Add(K2(k_row.IdUnitate, k_row.IdOpme))
        Next
        ' orders: found through the payment operation; one order per operation (TZ_OPuri.IdOper is unique)
        Dim k_seenOper As New HashSet(Of String)(StringComparer.Ordinal)
        Dim k_orderByOper As New Dictionary(Of String, List(Of AvcOpRow))(StringComparer.Ordinal)
        For Each k_row In _set.Opuri
            If Not k_look.Subunits.ContainsKey(k_row.IdUnitate) Then
                k_plan.FaraSubunitate.Add(k_row.IdUnitate)
                Continue For
            End If
            Dim k_what As String = $"OP {If(k_row.NrOp.HasValue, k_row.NrOp.Value.ToString(CultureInfo.InvariantCulture), "?")} (unitatea {k_context.UnitLabel(k_row.IdUnitate)}, {k_row.Explicatie})"
            If Not k_row.Oper.HasValue OrElse Not k_row.NrOp.HasValue OrElse Not k_row.Data.HasValue Then
                k_plan.Omit("Ordine de plată fără operație de plată, număr sau dată în Access", k_what)
                Continue For
            End If
            Dim k_idOper As Integer
            If Not k_look.Opers.TryGetValue(K2(k_row.IdUnitate, k_row.Oper.Value), k_idOper) Then
                k_plan.Omit("Operația de plată a ordinului nu este în «TZ_Oper» (pasul «Operații»)", k_what)
                Continue For
            End If
            If Not k_seenOper.Add(K2(k_row.IdUnitate, k_row.Oper.Value)) Then
                k_plan.Omit("Mai multe ordine pe aceeași operație de plată (un ordin = o plată)", k_what)
                Continue For
            End If
            Dim k_partner As Integer? = Nothing
            If k_row.CodPartener.Length > 0 Then
                Dim k_id As Integer
                If k_look.Parteneri.TryGetValue(k_row.IdUnitate.ToString(CultureInfo.InvariantCulture) & "|" & k_row.CodPartener, k_id) Then
                    k_partner = k_id
                Else
                    k_plan.FaraPartener += 1
                End If
            End If
            If k_row.IdOpme.HasValue AndAlso Not k_opmeOk.Contains(K2(k_row.IdUnitate, k_row.IdOpme.Value)) Then
                k_plan.Omit("Ordinul arată un OPME care nu există în Access: se migrează fără lot", k_what)
            End If
            k_plan.Opuri.Add(New PlanOp() With {.Row = k_row, .IdSubunitate = k_look.Subunits(k_row.IdUnitate), .IdOper = k_idOper, .IdPartener = k_partner})
            Dim k_list As List(Of AvcOpRow) = Nothing
            If Not k_orderByOper.TryGetValue(K2(k_row.IdUnitate, k_row.Oper.Value), k_list) Then
                k_list = New List(Of AvcOpRow)()
                k_orderByOper(K2(k_row.IdUnitate, k_row.Oper.Value)) = k_list
            End If
            k_list.Add(k_row)
        Next
        ' allocations
        For Each k_row In _set.Alocari
            If Not k_look.Subunits.ContainsKey(k_row.IdUnitate) Then
                k_plan.FaraSubunitate.Add(k_row.IdUnitate)
                Continue For
            End If
            Dim k_what As String = $"alocarea {k_row.Id.ToString(CultureInfo.InvariantCulture)} (unitatea {k_context.UnitLabel(k_row.IdUnitate)}, factura {k_row.NrDoc})"
            If Not k_row.IdPlata.HasValue Then
                k_plan.Omit("Alocări fără operație de plată în Access", k_what)
                Continue For
            End If
            Dim k_idPlata As Integer
            If Not k_look.Opers.TryGetValue(K2(k_row.IdUnitate, k_row.IdPlata.Value), k_idPlata) Then
                k_plan.Omit("Operația de plată a alocării nu este în «TZ_Oper» (pasul «Operații»)", k_what)
                Continue For
            End If
            Dim k_alloc As New PlanAlocare() With {.Row = k_row, .IdPlata = k_idPlata}
            If k_row.IdFactura.HasValue Then
                Dim k_idEv As Integer
                If k_look.Opers.TryGetValue(K2(k_row.IdUnitate, k_row.IdFactura.Value), k_idEv) Then
                    k_alloc.IdEvidentiere = k_idEv
                Else
                    k_plan.Omit("Operația de evidențiere a facturii nu este în «TZ_Oper»: alocarea se migrează fără ea", k_what)
                End If
            End If
            ' the received e-invoice: by the Access link when it is a known invoice, else through the evidence operation
            If k_row.IdEft.HasValue AndAlso k_look.Primite.Contains(k_row.IdEft.Value) Then
                k_alloc.IdPrimita = k_row.IdEft.Value
            ElseIf k_alloc.IdEvidentiere.HasValue Then
                Dim k_idPrimita As Integer
                If k_look.PrimitaDupaOper.TryGetValue(k_alloc.IdEvidentiere.Value, k_idPrimita) Then k_alloc.IdPrimita = k_idPrimita
            End If
            ' the order: through the payment operation, only when it is unique
            Dim k_orders As List(Of AvcOpRow) = Nothing
            If k_orderByOper.TryGetValue(K2(k_row.IdUnitate, k_row.IdPlata.Value), k_orders) Then
                If k_orders.Count = 1 Then
                    k_alloc.OpAccess = k_orders(0).IdOp
                Else
                    k_plan.AlocariCuOpAmbiguu += 1
                End If
            Else
                k_plan.AlocariFaraOp += 1
            End If
            k_plan.Alocari.Add(k_alloc)
        Next
        k_plan.Plaje.AddRange(_set.Plaje)
        Return k_plan
    End Function

    ' ── verification ────────────────────────────────────────────────────────────

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "«Plăți» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "Fișierele Access ale unităților bifate nu au tabelele PlatiFacturi și OPuri."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Unitățile fără tabelele plăților: " & String.Join(", ", _set.UnitsWithoutTable.Select(Function(k_uid) k_context.UnitLabel(k_uid))) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, AccessTable, String.Empty, "Coloane lipsă în Access: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If
            Dim k_db = k_context.TargetDatabase
            For Each k_table In Tables.Concat({"TZ_Subunitati", "TZ_Oper", "Parteneri"})
                If Not AvcSql.TableExists(k_cn, k_db, k_table) Then
                    k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, k_table, String.Empty,
                        If(Tables.Contains(k_table),
                           $"Tabelul «{k_table}» nu există în «{k_db}». Rulați sql/AVACONT/IntroducereDate/Operatii/AV02_02_plati_op_setari.sql pe AVACONT_SURSA și sincronizați schema.",
                           $"Tabelul «{k_table}» nu există în «{k_db}».")))
                End If
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings

            Dim k_look = LoadLookups(k_cn, Nothing)
            Dim k_plan = BuildPlan(k_look, k_context)
            If k_look.Opers.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.PAS_DEPENDENT, FindingClass.Atentie, "TZ_Oper", String.Empty,
                                           "«TZ_Oper» nu are operații migrate din Access. Dacă pasul «Operații» rulează în aceeași migrare, plățile se leagă la scriere; altfel rulați-l înainte."))
            End If
            If k_plan.FaraSubunitate.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_LIPSA, FindingClass.Blocant, "TZ_OPuri", "IdSubunitate",
                    $"Unitățile {String.Join(", ", k_plan.FaraSubunitate.Select(Function(k_uid) k_context.UnitLabel(k_uid)))} au nevoie de subunități. Bifați pasul «Subunități» în aceeași migrare sau creați-le înainte."))
            End If
            For Each k_pair In k_plan.Omise
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Atentie, "TZ_OPuri", String.Empty,
                                           $"{k_pair.Value.Count} rânduri NU se migrează sau se migrează incomplet: {k_pair.Key}", k_pair.Value.Count).WithDetail(String.Join(vbLf, k_pair.Value.Take(30))))
            Next
            If k_plan.FaraPartener > 0 Then
                k_findings.Add(New Finding(AvcKinds.PARTENER_LIPSA, FindingClass.Atentie, "TZ_OPuri", "IdPartener",
                                           $"{k_plan.FaraPartener} ordine au un partener care nu este în «Parteneri» (migrat de KBot.Migrator): se migrează fără legătura la partener, cu denumirea și codul fiscal din Access.", k_plan.FaraPartener))
            End If
            If k_plan.AlocariCuOpAmbiguu > 0 OrElse k_plan.AlocariFaraOp > 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Atentie, "TZ_PlatiFacturi", "IdOP",
                                           $"{k_plan.AlocariFaraOp} alocări nu găsesc niciun ordin prin operația de plată, {k_plan.AlocariCuOpAmbiguu} găsesc mai multe (ambigue): rămân fără ordin, nimic nu se ghicește."))
            End If
            k_findings.AddRange(RangeFindings(k_plan, k_look, k_context))
            RowsToWrite = k_plan.Alocari.Count
            k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_PlatiFacturi", String.Empty,
                                       $"{k_plan.Opme.Count} loturi OPME, {k_plan.Opuri.Count} ordine de plată, {k_plan.Alocari.Count} alocări de plăți " &
                                       $"(din {_set.Opme.Count} / {_set.Opuri.Count} / {_set.Alocari.Count} în Access).", k_plan.Alocari.Count))
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlatiStep.Verify", ex)
            Throw
        End Try
    End Function

    ' PlajaComunaOPuri: written only where Access had it ON. In K-BOT ON searches ALL subunits; in Access the range ran only over the units marked in LEGUNIT.
    Private Function RangeFindings(k_plan As Plan, k_look As Lookups, k_context As AvcContext) As List(Of Finding)
        Dim k_result As New List(Of Finding)()
        Dim k_on = k_plan.Plaje.Where(Function(k_p) k_p.Comuna).ToList()
        If k_on.Count = 0 Then
            k_result.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Settings", "PlajaComunaOPuri", "Nicio unitate nu avea numerotarea comună a ordinelor de plată (implicit: OFF)."))
            Return k_result
        End If
        k_result.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Settings", "PlajaComunaOPuri",
                                 $"«PlajaComunaOPuri» = ON pentru {k_on.Count} subunități: {String.Join(", ", k_on.Select(Function(k_p) k_context.UnitLabel(k_p.IdUnitate)))}.", k_on.Count))
        Dim k_allUnits As New HashSet(Of Integer)(k_look.Subunits.Keys)
        For Each k_p In k_on
            Dim k_linked As New HashSet(Of Integer)(k_p.Legate) From {k_p.IdUnitate}
            If Not k_linked.SetEquals(k_allUnits) Then
                k_result.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Atentie, "TZ_Settings", "PlajaComunaOPuri",
                    $"Unitatea {k_context.UnitLabel(k_p.IdUnitate)}: în Access numerotarea comună acoperea doar unitățile marcate în «LEGUNIT» ({k_linked.Count}); în K-BOT, ON caută în toate subunitățile ({k_allUnits.Count}). Verificați."))
            End If
        Next
        Return k_result
    End Function

    ' ── write ───────────────────────────────────────────────────────────────────

    Private Shared Function InsertRow(k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_dump As SqlDumpWriter, k_table As String,
                                      k_values As Dictionary(Of String, Object)) As Long
        Dim k_columns As List(Of String) = k_values.Keys.ToList()
        Dim k_sql As String = $"INSERT INTO {AvcSql.Quote(k_table)} ({String.Join(", ", k_columns.Select(AddressOf AvcSql.Quote))}) VALUES ({String.Join(", ", k_columns.Select(Function(k_c) "@" & k_c))})"
        Using k_cmd As New MySqlCommand(k_sql, k_cn, k_transaction)
            For Each k_column In k_columns
                k_cmd.Parameters.AddWithValue("@" & k_column, If(k_values(k_column), DBNull.Value))
            Next
            k_dump.WriteStatement(k_table, AvcSql.RenderStatement(k_sql, k_values))
            k_cmd.ExecuteNonQuery()
            Return k_cmd.LastInsertedId
        End Using
    End Function

    Private Shared Function Opt(k_value As Integer?) As Object
        Return If(k_value.HasValue, CObj(k_value.Value), Nothing)
    End Function

    Private Shared Function OptD(k_value As Date?) As Object
        Return If(k_value.HasValue, CObj(k_value.Value), Nothing)
    End Function

    Private Shared Function OptS(k_value As String) As Object
        Return If(String.IsNullOrEmpty(k_value), Nothing, k_value)
    End Function

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«Plăți» nu a fost citit din Access.")
            Dim k_look = LoadLookups(k_cn, k_transaction)
            Dim k_plan = BuildPlan(k_look, k_context)
            If k_plan.FaraSubunitate.Count > 0 Then Throw New InvalidOperationException("Unele unități nu au subunitate.")
            k_dump.WriteComment("TZ_OPuri", "Fostele OPME / OPuri / PlatiFacturi; CreatedAt = DTQ din Access; id-urile Access în coloanele *Access (o repetare adaugă doar ce lipsește).")
            Dim k_count As New Dictionary(Of String, Integer)(StringComparer.Ordinal) From {{"TZ_OPME", 0}, {"TZ_OPuri", 0}, {"TZ_PlatiFacturi", 0}, {"TZ_Settings", 0}}

            Dim k_idOpme As New Dictionary(Of String, Integer)(k_look.OpmeAcces, StringComparer.Ordinal)
            For Each k_row In k_plan.Opme
                k_cancel.ThrowIfCancellationRequested()
                Dim k_sub As Integer = k_look.Subunits(k_row.IdUnitate)
                If k_idOpme.ContainsKey(K2(k_sub, k_row.IdOpme)) Then Continue For
                Dim k_id As Integer = CInt(InsertRow(k_cn, k_transaction, k_dump, "TZ_OPME", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                    {"IdSubunitate", k_sub}, {"An", AvcContext.TransferYear}, {"NrOPME", k_row.NrOpme}, {"DataDoc", OptD(k_row.DataDoc)}, {"NrRec", k_row.NrRec}, {"Total", k_row.Total},
                    {"Platitor", OptS(k_row.Platitor)}, {"CIF", OptS(k_row.Cif)}, {"Adresa", OptS(k_row.Adresa)}, {"IdOPMEAccess", k_row.IdOpme}, {"CreatedAt", OptD(k_row.Dtq)}}))
                k_idOpme(K2(k_sub, k_row.IdOpme)) = k_id
                k_count("TZ_OPME") += 1
            Next

            Dim k_idOp As New Dictionary(Of String, Integer)(k_look.OpuriAcces, StringComparer.Ordinal)
            Dim k_done As Integer = 0
            For Each k_p In k_plan.Opuri
                k_cancel.ThrowIfCancellationRequested()
                k_done += 1
                k_context.Tick(k_done, k_plan.Opuri.Count, "«TZ_OPuri»")
                Dim k_row = k_p.Row
                If k_idOp.ContainsKey(K2(k_p.IdSubunitate, k_row.IdOp)) OrElse k_look.OpPeOper.Contains(k_p.IdOper) Then Continue For
                Dim k_opme As Integer? = Nothing
                Dim k_idm As Integer
                If k_row.IdOpme.HasValue AndAlso k_idOpme.TryGetValue(K2(k_p.IdSubunitate, k_row.IdOpme.Value), k_idm) Then k_opme = k_idm
                Dim k_id As Integer = CInt(InsertRow(k_cn, k_transaction, k_dump, "TZ_OPuri", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                    {"IdOper", k_p.IdOper}, {"IdSubunitate", k_p.IdSubunitate}, {"IdOPME", Opt(k_opme)}, {"NrOP", k_row.NrOp.Value}, {"DataOP", k_row.Data.Value}, {"Valoare", k_row.Valoare},
                    {"Explicatie", k_row.Explicatie}, {"Documente", OptS(k_row.Documente)}, {"IdPartener", Opt(k_p.IdPartener)}, {"DenPartener", OptS(k_row.DenPartener)},
                    {"CFPartener", OptS(k_row.CfPartener)}, {"ContBeneficiar", OptS(k_row.ContBeneficiar)}, {"bBIC", OptS(k_row.BBic)}, {"DenUnitate", OptS(k_row.DenUnitate)},
                    {"CFUnitate", OptS(k_row.CfUnitate)}, {"ContUnitate", OptS(k_row.ContUnitate)}, {"pBIC", OptS(k_row.PBic)}, {"CodAngajament", OptS(k_row.CodAngajament)},
                    {"CodIndicator", OptS(k_row.CodIndicator)}, {"CodProgram", OptS(k_row.CodProgram)}, {"Auto", If(k_row.Auto, 1, 0)}, {"DataIN", OptD(k_row.DataIn)},
                    {"IdOPAccess", k_row.IdOp}, {"CreatedAt", OptD(k_row.Dtq)}}))
                k_idOp(K2(k_p.IdSubunitate, k_row.IdOp)) = k_id
                k_count("TZ_OPuri") += 1
            Next

            k_done = 0
            For Each k_a In k_plan.Alocari
                k_cancel.ThrowIfCancellationRequested()
                k_done += 1
                k_context.Tick(k_done, k_plan.Alocari.Count, "«TZ_PlatiFacturi»")
                Dim k_row = k_a.Row
                If k_look.AlocariAcces.Contains(K2(k_a.IdPlata, k_row.Id)) Then Continue For
                Dim k_op As Integer? = Nothing
                Dim k_ido As Integer
                If k_a.OpAccess.HasValue AndAlso k_idOp.TryGetValue(K2(k_look.Subunits(k_row.IdUnitate), k_a.OpAccess.Value), k_ido) Then k_op = k_ido
                InsertRow(k_cn, k_transaction, k_dump, "TZ_PlatiFacturi", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                    {"IdPlata", k_a.IdPlata}, {"IdOP", Opt(k_op)}, {"IdEvidentiere", Opt(k_a.IdEvidentiere)}, {"IdEvidentiereAnterior", Opt(k_row.IdFacturaAA)}, {"IdPrimita", Opt(k_a.IdPrimita)},
                    {"NumarDocument", OptS(k_row.NrDoc)}, {"DataDocument", OptD(k_row.DtDoc)}, {"ValoareFactura", k_row.ValoareFactura}, {"Platit", k_row.Platit}, {"Diferenta", k_row.Diferenta},
                    {"ValoarePlata", k_row.ValoarePlata}, {"DataPlata", OptD(k_row.DataPlata)}, {"NrAL", If(k_row.NrAl.HasValue, CObj(k_row.NrAl.Value), Nothing)},
                    {"Numerar", If(k_row.Numerar, 1, 0)}, {"AlteDetalii", OptS(k_row.AlteDetalii)}, {"IdAlocareAccess", k_row.Id}, {"CreatedAt", OptD(k_row.Dtq)}})
                k_count("TZ_PlatiFacturi") += 1
            Next

            ' the setting: only where Access had the common range ON (OFF is the default and needs no row); an existing row is never changed
            For Each k_p In k_plan.Plaje.Where(Function(k_x) k_x.Comuna)
                Dim k_sub As Integer
                If Not k_look.Subunits.TryGetValue(k_p.IdUnitate, k_sub) OrElse k_look.Setari.Contains(k_sub) Then Continue For
                InsertRow(k_cn, k_transaction, k_dump, "TZ_Settings", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                    {"SettingKey", "PlajaComunaOPuri"}, {"IdSubunitate", k_sub}, {"Valoare", "1"}})
                k_count("TZ_Settings") += 1
            Next
            k_dump.FlushAll()
            k_progress.Report($"«TZ_PlatiFacturi»: {k_count("TZ_OPME")} OPME, {k_count("TZ_OPuri")} ordine, {k_count("TZ_PlatiFacturi")} alocări, {k_count("TZ_Settings")} setări (confirmate la COMMIT).")
            Return Tables.Select(Function(k_t) New AvcWritten(k_t, k_count(k_t))).ToList()
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcPlatiStep.Write", ex)
            Throw
        End Try
    End Function

End Class
