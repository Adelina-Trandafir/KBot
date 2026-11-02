Imports System.Globalization
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>What the target already holds that the operations point at (read-only).</summary>
Friend NotInheritable Class AvcOperatiiLookups
    ''' <summary>IdUnitate (Access) ▸ the subunit of the unit.</summary>
    Public ReadOnly Property Subunits As Dictionary(Of Integer, AvcSubunitRef) = New Dictionary(Of Integer, AvcSubunitRef)()
    ''' <summary>"IdSubunitate|composed symbol" ▸ IdCont.</summary>
    Public ReadOnly Property Conturi As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    ''' <summary>"IdUnitate|Access key" ▸ composed symbol (what the step «Plan de conturi» wrote).</summary>
    Public ReadOnly Property SimbolComp As New Dictionary(Of String, String)(StringComparer.Ordinal)
    ''' <summary>"SursaSector|COD" ▸ IdJurnal.</summary>
    Public ReadOnly Property Jurnale As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    ''' <summary>"IdUnitate|IdClsfAcc" ▸ IDClsf.</summary>
    Public ReadOnly Property Clasificatii As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
    ''' <summary>IdClsfAcc ▸ every IDClsf with it (used only when exactly one: a unit reading the classifications of its parent).</summary>
    Public ReadOnly Property ClasificatiiDupaAcc As New Dictionary(Of Integer, List(Of Integer))()
    ''' <summary>"IdUnitate|CodPartener" ▸ IdPartener.</summary>
    Public ReadOnly Property Parteneri As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
End Class

Friend NotInheritable Class AvcPlanLinie
    Public Property IdCont As Integer
    Public Property IdPartener As Integer?
    Public Property Suma As Decimal
End Class

Friend NotInheritable Class AvcPlanOper
    Public Property Row As AvcOperRow
    Public Property IdJurnal As Integer
    Public Property IdClsf As Integer?
    Public ReadOnly Property Debit As New List(Of AvcPlanLinie)()
    Public ReadOnly Property Credit As New List(Of AvcPlanLinie)()
End Class

Friend NotInheritable Class AvcPlanDoc
    Public Property Row As AvcDocumentRow
    Public ReadOnly Property Operatii As New List(Of AvcPlanOper)()
End Class

Friend NotInheritable Class AvcPlanNota
    Public Property IdUnitate As Integer
    Public Property IdSubunitate As Integer
    Public Property NumarNC As Integer
    Public Property DataNc As Date?
    Public ReadOnly Property Documente As New List(Of AvcPlanDoc)()
End Class

''' <summary>What would be written: the notes with their documents, operations and lines, and what is left out with the reason.</summary>
Friend NotInheritable Class AvcOperatiiPlan
    Public ReadOnly Property Note As New List(Of AvcPlanNota)()
    ''' <summary>reason ▸ descriptions of the rows left out (operations or documents).</summary>
    Public ReadOnly Property Omise As New SortedDictionary(Of String, List(Of String))(StringComparer.Ordinal)
    Public ReadOnly Property UnitatiFaraSubunitate As New SortedSet(Of Integer)()

    ''' <summary>IdUnitate ▸ the name the operator knows the unit by (<c>AlteDetalii</c>); the bare id when no context was given.</summary>
    Public Property UnitName As Func(Of Integer, String) = Function(k_id) k_id.ToString(CultureInfo.InvariantCulture)

    ''' <summary>The unit names of <see cref="UnitatiFaraSubunitate"/>, for the messages.</summary>
    Public Function UnitatiFaraSubunitateText() As String
        Return String.Join(", ", UnitatiFaraSubunitate.Select(Function(k_id) UnitName(k_id)))
    End Function

    Public Sub Omit(k_reason As String, k_what As String)
        Dim k_list As List(Of String) = Nothing
        If Not Omise.TryGetValue(k_reason, k_list) Then
            k_list = New List(Of String)()
            Omise(k_reason) = k_list
        End If
        k_list.Add(k_what)
    End Sub

    ''' <summary>The line that names an operation in the list of what was left out.</summary>
    Public Function Describe(k_op As AvcOperRow) As String
        Return $"operația {k_op.IdOperatie.ToString(CultureInfo.InvariantCulture)} (unitatea {UnitName(k_op.IdUnitate)}): {k_op.Explicatie}"
    End Function

    Public Function OperatiiCount() As Integer
        Return Note.Sum(Function(k_n) k_n.Documente.Sum(Function(k_d) k_d.Operatii.Count))
    End Function

    Public Function DocumenteCount() As Integer
        Return Note.Sum(Function(k_n) k_n.Documente.Count)
    End Function
End Class

''' <summary>
''' Stage 2, step «Operatii» (slice AV02-01): the Access <c>NoteC</c>, <c>Documente</c>, <c>Oper</c>, <c>Debit</c> and <c>Credit</c> become <c>TZ_NoteC</c>,
''' <c>TZ_Documente</c>, <c>TZ_Oper</c>, <c>TZ_Debit</c> and <c>TZ_Credit</c> of the subunit of the unit. NO INVOICE is moved here.
''' </summary>
''' <remarks>
''' <para>
''' The steps «Subunitati», «Plan de conturi» and «Jurnale» of AVC.Migrator and the partners and classifications of KBot.Migrator must ALREADY be in the target (an earlier
''' migration): the verification looks the accounts, journals, partners and classifications up there, and what it cannot find is left out of the migration and named
''' (an operation is never written half: either with all its lines or not at all; a document without a kept operation, and a note without a kept document, are left out too).
''' </para>
''' <para>
''' The Access ids are kept in <c>IdDocumentAccess</c> / <c>IdOperatieAccess</c> (and the salary links in <c>IdSalariiAccess</c> / <c>IdxAccess</c>), so a repeat adds only what is missing.
''' <c>CreatedAt</c> takes the Access <c>DTQ</c>; the template (<c>IDA</c>) is not carried over, <c>IdSablon</c> stays empty.
''' </para>
''' <para>
''' LAST STATEMENT: the invoices already migrated point (<c>EF_PrimiteOperatii.IdOperatie</c>) at the Access operation number; for the operations INSERTED by this run the number is
''' replaced by the new <c>IdOper</c>, in one statement (a temporary table), matching the unit (<c>IdUnitate</c> = the Access unit). Not done when an operation made in K-BOT already exists
''' (its number could be taken for an Access one) and never for operations that existed before the run, so a repeat does not touch the links again. Only the key of the link changes.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcOperatiiStep
    Inherits AvcStep

    Private Shared ReadOnly Tables As String() = {"TZ_NoteC", "TZ_Documente", "TZ_Oper", "TZ_Debit", "TZ_Credit"}

    Private _set As AvcOperatiiSet
    Private _planContRows As AvcPlanContSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "Operatii"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 2 - Introducere date"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Operații contabile (note, documente, operații, debit, credit)"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcOperatiiReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return Tables
        End Get
    End Property

    Public Overrides ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return {"Subunitati", "PlanCont", "Jurnale"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
        _planContRows = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcOperatiiReader.Read(k_context)
        _planContRows = AvcPlanContReader.Read(k_context)
        AccessRows = _set.Operatii.Count
        RowsToWrite = _set.Operatii.Count
    End Sub

    ' ── the lookups and the plan ────────────────────────────────────────────────

    Private Function LoadLookups(k_cn As MySqlConnection, k_transaction As MySqlTransaction) As AvcOperatiiLookups
        Try
            Dim k_result As New AvcOperatiiLookups()
            For Each k_pair In AvcSubunitStore.UnitMap(k_cn, k_transaction)
                k_result.Subunits(k_pair.Key) = k_pair.Value
            Next
            ReadPairs(k_cn, k_transaction, "SELECT `IdSubunitate`, `SimbolCont`, `IdCont` FROM `TZ_PlanCont`",
                      Sub(k_r) k_result.Conturi(k_r.GetInt32(0).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetString(1)) = k_r.GetInt32(2))
            ReadPairs(k_cn, k_transaction, "SELECT `SursaSector`, `Cod`, `IdJurnal` FROM `TZ_Jurnale`",
                      Sub(k_r) k_result.Jurnale(k_r.GetString(0) & "|" & k_r.GetString(1)) = k_r.GetInt32(2))
            ReadPairs(k_cn, k_transaction, "SELECT `IDClsf`, `IdClsfAcc`, `IdUnitate` FROM `Clasificatii`",
                      Sub(k_r)
                          Dim k_unit As Integer = If(k_r.IsDBNull(2), 0, k_r.GetInt32(2))
                          k_result.Clasificatii(k_unit.ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetInt32(1).ToString(CultureInfo.InvariantCulture)) = k_r.GetInt32(0)
                          Dim k_list As List(Of Integer) = Nothing
                          If Not k_result.ClasificatiiDupaAcc.TryGetValue(k_r.GetInt32(1), k_list) Then
                              k_list = New List(Of Integer)()
                              k_result.ClasificatiiDupaAcc(k_r.GetInt32(1)) = k_list
                          End If
                          k_list.Add(k_r.GetInt32(0))
                      End Sub)
            ReadPairs(k_cn, k_transaction, "SELECT `IdPartener`, `IdUnitate`, `CodPartener` FROM `Parteneri`",
                      Sub(k_r) k_result.Parteneri(k_r.GetInt32(1).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetString(2)) = k_r.GetInt32(0))
            For Each k_row In _planContRows.Rows
                k_result.SimbolComp(k_row.IdUnitate.ToString(CultureInfo.InvariantCulture) & "|" & k_row.SimbolAccess) = k_row.ComposedSymbol()
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcOperatiiStep.LoadLookups", ex)
            Throw
        End Try
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

    Private Shared Function Key2(k_a As Integer, k_b As String) As String
        Return k_a.ToString(CultureInfo.InvariantCulture) & "|" & k_b
    End Function

    ''' <summary>Resolves every operation against the lookups; nothing is written. Pure: the same lookups give the same plan.</summary>
    Private Function BuildPlan(k_look As AvcOperatiiLookups, k_context As AvcContext) As AvcOperatiiPlan
        Dim k_plan As New AvcOperatiiPlan() With {.UnitName = AddressOf k_context.UnitLabel}
        Dim k_notes = _set.Note.GroupBy(Function(k_n) Key2(k_n.IdUnitate, k_n.NumarNC.ToString(CultureInfo.InvariantCulture))).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.First(), StringComparer.Ordinal)
        Dim k_opsByDoc = _set.Operatii.Where(Function(k_o) k_o.IdDocument.HasValue).GroupBy(Function(k_o) Key2(k_o.IdUnitate, k_o.IdDocument.Value.ToString(CultureInfo.InvariantCulture))).
            ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.ToList(), StringComparer.Ordinal)
        Dim k_debit = _set.Debit.GroupBy(Function(k_l) Key2(k_l.IdUnitate, k_l.IdOperatie.ToString(CultureInfo.InvariantCulture))).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.ToList(), StringComparer.Ordinal)
        Dim k_credit = _set.Credit.GroupBy(Function(k_l) Key2(k_l.IdUnitate, k_l.IdOperatie.ToString(CultureInfo.InvariantCulture))).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.ToList(), StringComparer.Ordinal)
        For Each k_op In _set.Operatii.Where(Function(k_o) Not k_o.IdDocument.HasValue)
            k_plan.Omit("Operații fără document în Access", k_plan.Describe(k_op))
        Next

        Dim k_groups = _set.Documente.GroupBy(Function(k_d) New With {k_d.IdUnitate, .Nota = k_d.NumarNC.GetValueOrDefault(0)})
        For Each k_group In k_groups.OrderBy(Function(k_g) k_g.Key.IdUnitate).ThenBy(Function(k_g) k_g.Key.Nota)
            Dim k_sub As AvcSubunitRef = Nothing
            If Not k_look.Subunits.TryGetValue(k_group.Key.IdUnitate, k_sub) Then
                k_plan.UnitatiFaraSubunitate.Add(k_group.Key.IdUnitate)
                Continue For
            End If
            If k_group.Key.Nota = 0 Then
                For Each k_doc In k_group
                    k_plan.Omit("Documente fără număr de notă contabilă", $"{k_doc.NumarDocument} (unitatea {k_context.UnitLabel(k_doc.IdUnitate)})")
                Next
                Continue For
            End If
            Dim k_noteRow As AvcNotaRow = Nothing
            k_notes.TryGetValue(Key2(k_group.Key.IdUnitate, k_group.Key.Nota.ToString(CultureInfo.InvariantCulture)), k_noteRow)
            Dim k_nota As New AvcPlanNota() With {
                .IdUnitate = k_group.Key.IdUnitate, .IdSubunitate = k_sub.IdSubunitate, .NumarNC = k_group.Key.Nota, .DataNc = If(k_noteRow Is Nothing, Nothing, k_noteRow.DataNc)}
            For Each k_doc In k_group.OrderBy(Function(k_d) k_d.IdDocument)
                Dim k_planDoc As New AvcPlanDoc() With {.Row = k_doc}
                Dim k_ops As List(Of AvcOperRow) = Nothing
                If Not k_opsByDoc.TryGetValue(Key2(k_doc.IdUnitate, k_doc.IdDocument.ToString(CultureInfo.InvariantCulture)), k_ops) Then k_ops = New List(Of AvcOperRow)()
                If Not k_doc.DataDocument.HasValue Then
                    k_plan.Omit("Documente fără dată", $"{k_doc.NumarDocument} (unitatea {k_context.UnitLabel(k_doc.IdUnitate)})")
                    Continue For
                End If
                For Each k_op In k_ops.OrderBy(Function(k_o) k_o.IdOperatie)
                    Dim k_planOp As AvcPlanOper = ResolveOperation(k_op, k_sub, k_look, k_debit, k_credit, k_plan)
                    If k_planOp IsNot Nothing Then k_planDoc.Operatii.Add(k_planOp)
                Next
                If k_planDoc.Operatii.Count = 0 Then
                    If k_ops.Count = 0 Then k_plan.Omit("Documente fără operații", $"{k_doc.NumarDocument} (unitatea {k_context.UnitLabel(k_doc.IdUnitate)})")
                    Continue For
                End If
                k_nota.Documente.Add(k_planDoc)
            Next
            If k_nota.Documente.Count > 0 Then k_plan.Note.Add(k_nota)
        Next
        Return k_plan
    End Function

    Private Shared Function ResolveOperation(k_op As AvcOperRow, k_sub As AvcSubunitRef, k_look As AvcOperatiiLookups,
                                             k_debit As Dictionary(Of String, List(Of AvcLinieRow)), k_credit As Dictionary(Of String, List(Of AvcLinieRow)),
                                             k_plan As AvcOperatiiPlan) As AvcPlanOper
        Dim k_result As New AvcPlanOper() With {.Row = k_op}
        If Not k_op.DataOperatie.HasValue Then
            k_plan.Omit("Operații fără dată", k_plan.Describe(k_op))
            Return Nothing
        End If
        Dim k_journal As Integer
        If Not k_look.Jurnale.TryGetValue(k_sub.SursaSector & "|" & k_op.CodJurnal.ToUpperInvariant(), k_journal) Then
            k_plan.Omit($"Jurnal «{k_op.CodJurnal}» negăsit în TZ_Jurnale (rulați pasul «Jurnale»)", k_plan.Describe(k_op))
            Return Nothing
        End If
        k_result.IdJurnal = k_journal
        If k_op.IdClsfAccess.HasValue Then
            Dim k_clsf As Integer
            If k_look.Clasificatii.TryGetValue(Key2(k_op.IdUnitate, k_op.IdClsfAccess.Value.ToString(CultureInfo.InvariantCulture)), k_clsf) Then
                k_result.IdClsf = k_clsf
            Else
                Dim k_candidates As List(Of Integer) = Nothing
                If k_look.ClasificatiiDupaAcc.TryGetValue(k_op.IdClsfAccess.Value, k_candidates) AndAlso k_candidates.Count = 1 Then
                    k_result.IdClsf = k_candidates(0)
                Else
                    k_plan.Omit("Clasificație negăsită în Clasificatii (migrată de KBot.Migrator)", k_plan.Describe(k_op))
                    Return Nothing
                End If
            End If
        End If
        Dim k_key As String = Key2(k_op.IdUnitate, k_op.IdOperatie.ToString(CultureInfo.InvariantCulture))
        Dim k_d As List(Of AvcLinieRow) = Nothing
        Dim k_c As List(Of AvcLinieRow) = Nothing
        If Not k_debit.TryGetValue(k_key, k_d) OrElse Not k_credit.TryGetValue(k_key, k_c) Then
            k_plan.Omit("Operații fără linii de debit sau de credit", k_plan.Describe(k_op))
            Return Nothing
        End If
        For Each k_pair In {Tuple.Create(k_d, k_result.Debit), Tuple.Create(k_c, k_result.Credit)}
            For Each k_line In k_pair.Item1
                Dim k_composed As String = Nothing
                Dim k_cont As Integer
                If Not k_look.SimbolComp.TryGetValue(Key2(k_line.IdUnitate, k_line.SimbolCont), k_composed) OrElse
                   Not k_look.Conturi.TryGetValue(Key2(k_sub.IdSubunitate, k_composed), k_cont) Then
                    k_plan.Omit($"Cont «{k_line.SimbolCont}» negăsit în TZ_PlanCont (rulați pasul «Plan de conturi»)", k_plan.Describe(k_op))
                    Return Nothing
                End If
                Dim k_partner As Integer? = Nothing
                If k_line.CodPartener.Length > 0 Then
                    Dim k_id As Integer
                    If Not k_look.Parteneri.TryGetValue(Key2(k_line.IdUnitate, k_line.CodPartener), k_id) Then
                        k_plan.Omit("Partener negăsit în Parteneri (migrat de KBot.Migrator)", k_plan.Describe(k_op) & $" (partener {k_line.CodPartener})")
                        Return Nothing
                    End If
                    k_partner = k_id
                End If
                k_pair.Item2.Add(New AvcPlanLinie() With {.IdCont = k_cont, .IdPartener = k_partner, .Suma = k_line.Suma})
            Next
        Next
        If k_result.Debit.Sum(Function(k_l) k_l.Suma) <> k_result.Credit.Sum(Function(k_l) k_l.Suma) Then
            k_plan.Omit("Operații cu debitul diferit de credit", k_plan.Describe(k_op))
            Return Nothing
        End If
        Return k_result
    End Function

    ' ── verification ────────────────────────────────────────────────────────────

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "«Operații» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "Fișierele Access ale unităților bifate nu au tabelele NoteC, Documente, Oper, Debit și Credit."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Unitățile fără tabelele operațiilor: " & String.Join(", ", _set.UnitsWithoutTable.Select(Function(k_uid) k_context.UnitLabel(k_uid))) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, AccessTable, String.Empty, "Coloane lipsă în Access: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If
            Dim k_db = k_context.TargetDatabase
            For Each k_table In Tables.Concat({"TZ_Subunitati", "TZ_PlanCont", "TZ_Jurnale", "Clasificatii", "Parteneri"})
                If Not AvcSql.TableExists(k_cn, k_db, k_table) Then
                    k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, k_table, String.Empty,
                        If(Tables.Contains(k_table),
                           $"Tabelul «{k_table}» nu există în «{k_db}». Rulați sql/AVACONT/IntroducereDate/Operatii/AV02_01_operatii.sql pe AVACONT_SURSA și sincronizați schema.",
                           $"Tabelul «{k_table}» nu există în «{k_db}».")))
                End If
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings
            For Each k_pair In {("TZ_Oper", New String() {"IdDocument", "IdJurnal", "IdClsf", "IdSablon", "DataOperatie", "Explicatie", "Tip", "Fel", "Compartiment", "Cfpp", "Angajament", "Propunere",
                                                           "Ordonantare", "Chelt", "NrAng", "NrOpe", "Auto", "Platit", "IdOperatieAccess", "IdxAccess", "CreatedAt"}),
                                ("TZ_Documente", New String() {"IdNota", "NumarDocument", "DataDocument", "FelDocument", "Fel", "Auto", "EditManual", "Platit", "IdDocumentAccess", "IdSalariiAccess", "CreatedAt"})}
                For Each k_column In AvcSql.MissingColumns(k_cn, k_db, k_pair.Item1, k_pair.Item2)
                    k_findings.Add(New Finding(Finding.COLOANA_OBLIGATORIE, FindingClass.Blocant, k_pair.Item1, k_column, $"Coloana «{k_column}» nu există în «{k_pair.Item1}» din «{k_db}»."))
                Next
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings

            Dim k_plan = BuildPlan(LoadLookups(k_cn, Nothing), k_context)
            If k_plan.UnitatiFaraSubunitate.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_LIPSA, FindingClass.Blocant, "TZ_NoteC", "IdSubunitate",
                                           $"Unitățile {k_plan.UnitatiFaraSubunitateText()} nu au încă subunitate. Rulați întâi pașii «Subunități», «Plan de conturi» și «Jurnale» într-o migrare anterioară."))
            End If
            For Each k_pair In k_plan.Omise
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Atentie, "TZ_Oper", String.Empty,
                                           $"{k_pair.Value.Count} rânduri NU se migrează: {k_pair.Key}", k_pair.Value.Count).WithDetail(String.Join(vbLf, k_pair.Value.Take(30))))
            Next
            k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_Oper", String.Empty,
                                       $"{k_plan.Note.Count} note, {k_plan.DocumenteCount()} documente, {k_plan.OperatiiCount()} operații ▸ TZ_NoteC / TZ_Documente / TZ_Oper / TZ_Debit / TZ_Credit " &
                                       $"(din {_set.Operatii.Count} operații Access).", k_plan.OperatiiCount()))
            Dim k_existing As Long = AvcSql.CountRows(k_cn, k_db, "TZ_Oper")
            If k_existing > 0 Then
                k_findings.Add(New Finding(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, "TZ_Oper", String.Empty,
                                           $"«TZ_Oper» are deja {k_existing} operații. Se adaugă doar ce lipsește (după numerele din Access); nimic existent nu se modifică.", CInt(Math.Min(k_existing, Integer.MaxValue))))
            End If
            If AvcSql.TableExists(k_cn, k_db, "EF_PrimiteOperatii") Then
                Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_Oper` WHERE `IdOperatieAccess` IS NULL", k_cn)
                    If Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0 Then
                        k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Atentie, "EF_PrimiteOperatii", "IdOperatie",
                                                   "Există deja operații făcute în K-BOT: legăturile facturilor migrate NU se actualizează (un număr nou ar putea fi luat drept număr din Access)."))
                    Else
                        k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "EF_PrimiteOperatii", "IdOperatie",
                                                   "Ultima instrucțiune: legăturile facturilor migrate din «EF_PrimiteOperatii» primesc numerele noi ale operațiilor (se schimbă doar cheia legăturii)."))
                    End If
                End Using
            End If
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcOperatiiStep.Verify", ex)
            Throw
        End Try
    End Function

    ' ── write ───────────────────────────────────────────────────────────────────

    Private Shared Function Nb(k_value As Boolean) As Object
        Return If(k_value, 1, 0)
    End Function

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

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«Operații» nu a fost citit din Access.")
            Dim k_plan = BuildPlan(LoadLookups(k_cn, k_transaction), k_context)
            If k_plan.UnitatiFaraSubunitate.Count > 0 Then Throw New InvalidOperationException($"Unitățile {k_plan.UnitatiFaraSubunitateText()} nu au subunitate.")
            k_dump.WriteComment("TZ_NoteC", "Fostele NoteC / Documente / Oper / Debit / Credit; CreatedAt = DTQ din Access; id-urile Access în coloanele *Access (o repetare adaugă doar ce lipsește).")

            ' What is already there (a repeat adds only what is missing).
            Dim k_notes As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            ReadPairs(k_cn, k_transaction, "SELECT `IdNota`, `IdSubunitate`, `An`, `NumarNC` FROM `TZ_NoteC`",
                      Sub(k_r) k_notes(k_r.GetInt32(1).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetInt32(2).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetInt32(3).ToString(CultureInfo.InvariantCulture)) = k_r.GetInt32(0))
            Dim k_docs As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            ReadPairs(k_cn, k_transaction, "SELECT `IdDocument`, `IdNota`, `IdDocumentAccess` FROM `TZ_Documente` WHERE `IdDocumentAccess` IS NOT NULL",
                      Sub(k_r) k_docs(k_r.GetInt32(1).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetInt32(2).ToString(CultureInfo.InvariantCulture)) = k_r.GetInt32(0))
            Dim k_ops As New HashSet(Of String)(StringComparer.Ordinal)
            ReadPairs(k_cn, k_transaction, "SELECT `IdDocument`, `IdOperatieAccess` FROM `TZ_Oper` WHERE `IdOperatieAccess` IS NOT NULL",
                      Sub(k_r) k_ops.Add(k_r.GetInt32(0).ToString(CultureInfo.InvariantCulture) & "|" & k_r.GetInt32(1).ToString(CultureInfo.InvariantCulture)))
            Dim k_keptByKbot As Boolean
            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_Oper` WHERE `IdOperatieAccess` IS NULL", k_cn, k_transaction)
                k_keptByKbot = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0
            End Using

            Dim k_count As New Dictionary(Of String, Integer)(StringComparer.Ordinal) From {{"TZ_NoteC", 0}, {"TZ_Documente", 0}, {"TZ_Oper", 0}, {"TZ_Debit", 0}, {"TZ_Credit", 0}}
            Dim k_remap As New List(Of Tuple(Of Integer, Integer, Long))()
            Dim k_docsTotal As Integer = k_plan.DocumenteCount()
            Dim k_docsDone As Integer = 0
            For Each k_nota In k_plan.Note
                k_cancel.ThrowIfCancellationRequested()
                Dim k_noteKey As String = $"{k_nota.IdSubunitate.ToString(CultureInfo.InvariantCulture)}|{AvcContext.TransferYear.ToString(CultureInfo.InvariantCulture)}|{k_nota.NumarNC.ToString(CultureInfo.InvariantCulture)}"
                Dim k_idNota As Integer
                If Not k_notes.TryGetValue(k_noteKey, k_idNota) Then
                    k_idNota = CInt(InsertRow(k_cn, k_transaction, k_dump, "TZ_NoteC", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                        {"IdSubunitate", k_nota.IdSubunitate}, {"An", AvcContext.TransferYear}, {"NumarNC", k_nota.NumarNC},
                        {"DataNc", If(k_nota.DataNc.HasValue, CObj(k_nota.DataNc.Value), Nothing)}, {"CreatedAt", If(k_nota.DataNc.HasValue, CObj(k_nota.DataNc.Value), Nothing)}}))
                    k_notes(k_noteKey) = k_idNota
                    k_count("TZ_NoteC") += 1
                End If
                For Each k_doc In k_nota.Documente
                    k_docsDone += 1
                    k_context.Tick(k_docsDone, k_docsTotal, "«TZ_Oper»: documente")
                    Dim k_docKey As String = $"{k_idNota.ToString(CultureInfo.InvariantCulture)}|{k_doc.Row.IdDocument.ToString(CultureInfo.InvariantCulture)}"
                    Dim k_idDoc As Integer
                    If Not k_docs.TryGetValue(k_docKey, k_idDoc) Then
                        k_idDoc = CInt(InsertRow(k_cn, k_transaction, k_dump, "TZ_Documente", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                            {"IdNota", k_idNota}, {"NumarDocument", k_doc.Row.NumarDocument}, {"DataDocument", k_doc.Row.DataDocument.Value}, {"FelDocument", k_doc.Row.FelDocument},
                            {"Fel", If(k_doc.Row.Fel.Length > 0, k_doc.Row.Fel, Nothing)}, {"Auto", Nb(k_doc.Row.Auto)}, {"EditManual", Nb(k_doc.Row.EditManual)}, {"Platit", Nb(k_doc.Row.Platit)},
                            {"IdDocumentAccess", k_doc.Row.IdDocument}, {"IdSalariiAccess", If(k_doc.Row.IdSalarii.HasValue, CObj(k_doc.Row.IdSalarii.Value), Nothing)},
                            {"CreatedAt", If(k_doc.Row.Dtq.HasValue, CObj(k_doc.Row.Dtq.Value), Nothing)}}))
                        k_docs(k_docKey) = k_idDoc
                        k_count("TZ_Documente") += 1
                    End If
                    For Each k_op In k_doc.Operatii
                        Dim k_opKey As String = $"{k_idDoc.ToString(CultureInfo.InvariantCulture)}|{k_op.Row.IdOperatie.ToString(CultureInfo.InvariantCulture)}"
                        If k_ops.Contains(k_opKey) Then Continue For
                        Dim k_created As Object = If(k_op.Row.Dtq.HasValue, CObj(k_op.Row.Dtq.Value), Nothing)
                        Dim k_idOper As Long = InsertRow(k_cn, k_transaction, k_dump, "TZ_Oper", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                            {"IdDocument", k_idDoc}, {"IdJurnal", k_op.IdJurnal}, {"IdClsf", If(k_op.IdClsf.HasValue, CObj(k_op.IdClsf.Value), Nothing)},
                            {"DataOperatie", k_op.Row.DataOperatie.Value}, {"Explicatie", k_op.Row.Explicatie}, {"Tip", If(k_op.Row.Tip.Length > 0, k_op.Row.Tip, Nothing)},
                            {"Fel", If(k_op.Row.Fel.Length > 0, k_op.Row.Fel, Nothing)}, {"Compartiment", If(k_op.Row.Comp.Length > 0, k_op.Row.Comp, Nothing)},
                            {"Cfpp", Nb(k_op.Row.Cfpp)}, {"Angajament", Nb(k_op.Row.Angajament)}, {"Propunere", Nb(k_op.Row.Propunere)}, {"Ordonantare", Nb(k_op.Row.Ordonantare)},
                            {"Chelt", Nb(k_op.Row.Chelt)}, {"NrAng", If(k_op.Row.NrAng.HasValue, CObj(k_op.Row.NrAng.Value), Nothing)}, {"NrOpe", If(k_op.Row.NrOpe.HasValue, CObj(k_op.Row.NrOpe.Value), Nothing)},
                            {"Auto", Nb(k_op.Row.Auto)}, {"Platit", Nb(k_op.Row.Platit)}, {"IdOperatieAccess", k_op.Row.IdOperatie},
                            {"IdxAccess", If(k_op.Row.Idx.HasValue, CObj(k_op.Row.Idx.Value), Nothing)}, {"CreatedAt", k_created}})
                        k_count("TZ_Oper") += 1
                        k_remap.Add(Tuple.Create(k_nota.IdUnitate, k_op.Row.IdOperatie, k_idOper))
                        For Each k_pair In {Tuple.Create("TZ_Debit", k_op.Debit), Tuple.Create("TZ_Credit", k_op.Credit)}
                            For Each k_line In k_pair.Item2
                                InsertRow(k_cn, k_transaction, k_dump, k_pair.Item1, New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                                    {"IdOper", k_idOper}, {"IdCont", k_line.IdCont}, {"IdPartener", If(k_line.IdPartener.HasValue, CObj(k_line.IdPartener.Value), Nothing)},
                                    {"Suma", k_line.Suma}, {"CreatedAt", k_created}})
                                k_count(k_pair.Item1) += 1
                            Next
                        Next
                    Next
                Next
            Next
            RemapInvoiceLinks(k_cn, k_transaction, k_dump, k_remap, k_keptByKbot, k_progress)
            k_dump.FlushAll()
            k_progress.Report($"«TZ_Oper»: {k_count("TZ_NoteC")} note, {k_count("TZ_Documente")} documente, {k_count("TZ_Oper")} operații (confirmate la COMMIT).")
            Return Tables.Select(Function(k_t) New AvcWritten(k_t, k_count(k_t))).ToList()
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcOperatiiStep.Write", ex)
            Throw
        End Try
    End Function

    ' The last statement: the migrated invoices point at the NEW operations. One statement over a temporary table, so no number is replaced twice
    ' (an old number may equal a new one). Only the key of the link changes.
    Private Shared Sub RemapInvoiceLinks(k_cn As MySqlConnection, k_transaction As MySqlTransaction, k_dump As SqlDumpWriter,
                                         k_remap As List(Of Tuple(Of Integer, Integer, Long)), k_keptByKbot As Boolean, k_progress As IProgress(Of String))
        If k_remap.Count = 0 Then Return
        If k_keptByKbot OrElse Not AvcSql.TableExists(k_cn, k_cn.Database, "EF_PrimiteOperatii", k_transaction) Then
            k_progress.Report("«EF_PrimiteOperatii»: legăturile facturilor nu s-au actualizat (există operații făcute în K-BOT sau tabelul lipsește).")
            Return
        End If
        Dim k_create As String = "CREATE TEMPORARY TABLE `tmp_av_remap_oper` (`IdUnitate` INT NOT NULL, `Vechi` INT NOT NULL, `Nou` INT NOT NULL)"
        k_dump.WriteStatement("EF_PrimiteOperatii", k_create)
        Using k_cmd As New MySqlCommand(k_create, k_cn, k_transaction)
            k_cmd.ExecuteNonQuery()
        End Using
        For Each k_item In k_remap
            InsertRow(k_cn, k_transaction, k_dump, "tmp_av_remap_oper", New Dictionary(Of String, Object)(StringComparer.Ordinal) From {
                {"IdUnitate", k_item.Item1}, {"Vechi", k_item.Item2}, {"Nou", CInt(k_item.Item3)}})
        Next
        Dim k_update As String = "UPDATE `EF_PrimiteOperatii` e JOIN `tmp_av_remap_oper` m ON e.`IdOperatie` = m.`Vechi` AND e.`IdUnitate` = m.`IdUnitate` SET e.`IdOperatie` = m.`Nou`"
        k_dump.WriteStatement("EF_PrimiteOperatii", k_update)
        Dim k_changed As Integer
        Using k_cmd As New MySqlCommand(k_update, k_cn, k_transaction)
            k_changed = k_cmd.ExecuteNonQuery()
        End Using
        Using k_cmd As New MySqlCommand("DROP TEMPORARY TABLE `tmp_av_remap_oper`", k_cn, k_transaction)
            k_cmd.ExecuteNonQuery()
        End Using
        k_progress.Report($"«EF_PrimiteOperatii»: {k_changed.ToString(CultureInfo.InvariantCulture)} legături de facturi trimise la operațiile noi.")
    End Sub

End Class
