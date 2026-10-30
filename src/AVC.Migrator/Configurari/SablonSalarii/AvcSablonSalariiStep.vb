Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>One line of the Access <c>DefaSalariiVA</c> (retention), <c>DefaSalariiPC</c> (virament) or <c>DefaSalariiPN</c> (cash payment).</summary>
Friend NotInheritable Class AvcSalLinieRow
    ''' <summary>RETINERE, VIRAMENT or NUMERAR.</summary>
    Public Property Tip As String = String.Empty
    ''' <summary>The Access key of the line (IDVA, IDPC, IDPN).</summary>
    Public Property IdAccess As Long
    ''' <summary>For a virament: the Access <c>IDVA</c> of the retention it pays (IDVAA); 0 = none.</summary>
    Public Property IdParinteAccess As Long
    Public Property Explicatie As String = String.Empty
    Public Property ContD As String = String.Empty
    Public Property PartD As String = String.Empty
    Public Property ContC As String = String.Empty
    Public Property PartC As String = String.Empty
    Public Property CM As Boolean
End Class

''' <summary>One article of the Access <c>DefaSalarii</c> with its lines.</summary>
Friend NotInheritable Class AvcSalArticolRow
    Public Property IdUnitate As Integer
    ''' <summary>The Access key of the article (IDS).</summary>
    Public Property Ids As Long
    Public Property Clsf As String = String.Empty
    Public Property Explicatie As String = String.Empty
    Public Property ContD As String = String.Empty
    Public Property PartD As String = String.Empty
    Public Property ContC As String = String.Empty
    Public Property PartC As String = String.Empty
    ''' <summary>The chapter the article was defined for (65.01 ...).</summary>
    Public Property Cap As String = String.Empty
    Public ReadOnly Property Linii As New List(Of AvcSalLinieRow)()
End Class

''' <summary>Everything read from the Access salary-template tables of the ticked units.</summary>
Friend NotInheritable Class AvcSalSet
    Public ReadOnly Property Rows As New List(Of AvcSalArticolRow)()
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    Public ReadOnly Property MissingColumns As New List(Of String)()
    ''' <summary>Rows marked hidden in Access (the soft delete): not migrated.</summary>
    Public Property HiddenRows As Integer
End Class

''' <summary>Reads <c>DefaSalarii</c> + <c>DefaSalariiVA</c> + <c>DefaSalariiPC</c> + <c>DefaSalariiPN</c> of every ticked unit's file.</summary>
Friend NotInheritable Class AvcSalReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "DefaSalarii"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcSalSet
        Try
            Dim k_result As New AvcSalSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
                    Dim k_articles = AccessSchema.ResolveTableName(k_cn, AccessTable)
                    Dim k_va = AccessSchema.ResolveTableName(k_cn, "DefaSalariiVA")
                    Dim k_pc = AccessSchema.ResolveTableName(k_cn, "DefaSalariiPC")
                    Dim k_pn = AccessSchema.ResolveTableName(k_cn, "DefaSalariiPN")
                    If k_articles Is Nothing OrElse k_va Is Nothing OrElse k_pc Is Nothing OrElse k_pn Is Nothing Then
                        k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                        Continue For
                    End If
                    Dim k_byIds As New Dictionary(Of Long, AvcSalArticolRow)()
                    Using k_reader = AccessSchema.OpenReader(k_cn, k_articles)
                        CheckColumns(k_reader, {"IDS", "Clsf", "Explicatie", "ContD", "PartD", "ContC", "PartC", "Cap"}, k_result)
                        While k_reader.Read()
                            Dim k_ids As Long = AsLong(k_reader.ValueOrMissing("IDS"))
                            If k_ids = 0 OrElse k_byIds.ContainsKey(k_ids) Then Continue While
                            If AsFlag(k_reader.ValueOrMissing("Ascuns")) Then
                                k_result.HiddenRows += 1
                                Continue While
                            End If
                            Dim k_row As New AvcSalArticolRow() With {
                                .IdUnitate = k_unit.IdUnitate, .Ids = k_ids,
                                .Clsf = AvcSql.AsText(k_reader.ValueOrMissing("Clsf")), .Explicatie = AvcSql.AsText(k_reader.ValueOrMissing("Explicatie")),
                                .ContD = AvcSql.AsText(k_reader.ValueOrMissing("ContD")), .PartD = AvcSql.AsText(k_reader.ValueOrMissing("PartD")),
                                .ContC = AvcSql.AsText(k_reader.ValueOrMissing("ContC")), .PartC = AvcSql.AsText(k_reader.ValueOrMissing("PartC")),
                                .Cap = AvcSql.AsText(k_reader.ValueOrMissing("Cap"))}
                            k_byIds(k_ids) = k_row
                            k_result.Rows.Add(k_row)
                        End While
                    End Using
                    ReadLines(k_cn, k_va, "RETINERE", "IDVA", k_byIds, k_result)
                    ReadLines(k_cn, k_pc, "VIRAMENT", "IDPC", k_byIds, k_result)
                    ReadLines(k_cn, k_pn, "NUMERAR", "IDPN", k_byIds, k_result)
                End Using
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSalReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Sub ReadLines(k_cn As System.Data.OleDb.OleDbConnection, k_table As String, k_tip As String, k_idColumn As String, k_byIds As Dictionary(Of Long, AvcSalArticolRow), k_result As AvcSalSet)
        Using k_reader = AccessSchema.OpenReader(k_cn, k_table)
            Dim k_columns As New List(Of String) From {k_idColumn, "IDD", "Explicatie", "ContD", "PartD", "ContC", "PartC"}
            If k_tip = "VIRAMENT" Then k_columns.Add("IDVAA")
            CheckColumns(k_reader, k_columns, k_result)
            While k_reader.Read()
                Dim k_article As AvcSalArticolRow = Nothing
                If Not k_byIds.TryGetValue(AsLong(k_reader.ValueOrMissing("IDD")), k_article) Then Continue While
                If AsFlag(k_reader.ValueOrMissing("Ascuns")) Then
                    k_result.HiddenRows += 1
                    Continue While
                End If
                k_article.Linii.Add(New AvcSalLinieRow() With {
                    .Tip = k_tip, .IdAccess = AsLong(k_reader.ValueOrMissing(k_idColumn)),
                    .IdParinteAccess = If(k_tip = "VIRAMENT", AsLong(k_reader.ValueOrMissing("IDVAA")), 0),
                    .Explicatie = AvcSql.AsText(k_reader.ValueOrMissing("Explicatie")),
                    .ContD = AvcSql.AsText(k_reader.ValueOrMissing("ContD")), .PartD = AvcSql.AsText(k_reader.ValueOrMissing("PartD")),
                    .ContC = AvcSql.AsText(k_reader.ValueOrMissing("ContC")), .PartC = AvcSql.AsText(k_reader.ValueOrMissing("PartC")),
                    .CM = k_tip <> "NUMERAR" AndAlso AsFlag(k_reader.ValueOrMissing("CM"))})
            End While
        End Using
    End Sub

    Private Shared Sub CheckColumns(k_reader As AccessTableReader, k_columns As IEnumerable(Of String), k_result As AvcSalSet)
        For Each k_column In k_columns
            If Not k_reader.HasColumn(k_column) AndAlso Not k_result.MissingColumns.Contains(k_column) Then
                k_result.MissingColumns.Add(k_column)
            End If
        Next
    End Sub

    Private Shared Function AsLong(k_value As Object) As Long
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return 0
        Try
            Return Convert.ToInt64(k_value, CultureInfo.InvariantCulture)
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
            GlobalErrorLog.Write("AvcSalReader.AsLong", ex)
            Return 0
        End Try
    End Function

    ' Access Yes/No (-1 / 0), a real Boolean, or the words Da / Nu.
    Private Shared Function AsFlag(k_value As Object) As Boolean
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return False
        If TypeOf k_value Is Boolean Then Return DirectCast(k_value, Boolean)
        Dim k_text As String = AvcSql.AsText(k_value)
        If k_text.Length = 0 Then Return False
        If String.Equals(k_text, "da", StringComparison.OrdinalIgnoreCase) OrElse String.Equals(k_text, "true", StringComparison.OrdinalIgnoreCase) Then Return True
        Dim k_number As Long
        If Long.TryParse(k_text, NumberStyles.Integer, CultureInfo.InvariantCulture, k_number) Then Return k_number <> 0
        Return False
    End Function

End Class

''' <summary>
''' Stage 1, step 6 (slice AV01-15): the Access salary template (<c>DefaSalarii</c> articles with their <c>DefaSalariiVA</c> retentions, <c>DefaSalariiPC</c> viraments and
''' <c>DefaSalariiPN</c> cash payments) becomes <c>TZ_SablonSalarii</c> + <c>TZ_SablonSalariiLinie</c> (named SablonSalarii by the operator).
''' </summary>
''' <remarks>
''' <para>
''' An article goes to the subunit of its unit (the chapter <c>Cap</c> must be the one of that subunit). Accounts are the BASE accounts (the first seven digits of the Access
''' symbol, which must be in <c>DefaCont</c>); a partner mark <c>XXX</c> is «the partner of the operation», a code is a fixed partner (it must exist in <c>Parteneri</c>), empty is none.
''' The debit of a retention is the credit of its article (the target writes it that way). A virament keeps the link to the retention it pays. Rows hidden in Access (<c>Ascuns</c>)
''' are not migrated. An article the target would refuse (no virament, an account or partner that is missing ...) is LEFT OUT whole and named in the verification.
''' </para>
''' <para>
''' The write adds the articles that are not there yet (same subunit, classification and explanation) and changes nothing else.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcSablonSalariiStep
    Inherits AvcStep

    Private Shared ReadOnly ClsfRegex As New Regex("^[0-9]{2}\.[0-9]{2}\.[0-9]{2}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly SixDigits As New Regex("^[0-9]{6}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly HeaderColumns As String() = {"IdSubunitate", "Clasificatie", "Explicatie", "ContDebit", "ContCredit", "PartDebitMod", "IdPartDebit", "PartCreditMod", "IdPartCredit"}
    Private Shared ReadOnly LineColumns As String() = {"IdArticol", "Tip", "IdParinte", "Ordine", "Explicatie", "ContDebit", "ContCredit", "PartDebitMod", "IdPartDebit", "PartCreditMod", "IdPartCredit", "EsteCM"}

    Private NotInheritable Class ResolvedLine
        Public Property Source As AvcSalLinieRow
        Public Property ContDebit As Integer
        Public Property ContCredit As Integer
        Public Property PartDebitMod As String = "NIMIC"
        Public Property IdPartDebit As Long?
        Public Property PartCreditMod As String = "NIMIC"
        Public Property IdPartCredit As Long?
        Public Property EsteCM As Boolean
    End Class

    Private NotInheritable Class Resolved
        Public Sub New(k_row As AvcSalArticolRow)
            Row = k_row
        End Sub

        Public ReadOnly Property Row As AvcSalArticolRow
        Public Property IdSubunitate As Long?
        Public Property Clasificatie As String = String.Empty
        Public Property ContDebit As Integer
        Public Property ContCredit As Integer
        Public Property PartDebitMod As String = "NIMIC"
        Public Property IdPartDebit As Long?
        Public Property PartCreditMod As String = "NIMIC"
        Public Property IdPartCredit As Long?
        Public ReadOnly Property Linii As New List(Of ResolvedLine)()
        Public ReadOnly Property Problems As New List(Of String)()
    End Class

    Private _set As AvcSalSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "SablonSalarii"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Șabloane de salarii"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcSalReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"TZ_SablonSalarii", "TZ_SablonSalariiLinie"}
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
        _set = AvcSalReader.Read(k_context)
        AccessRows = _set.Rows.Count
        RowsToWrite = _set.Rows.Count
    End Sub

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "«DefaSalarii» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "Fișierele Access ale unităților bifate nu au tabelele «DefaSalarii», «DefaSalariiVA», «DefaSalariiPC» și «DefaSalariiPN»."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Fișierul Access nu are tabelele șablonului de salarii la unitățile: " & String.Join(", ", _set.UnitsWithoutTable) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Blocant, "TZ_SablonSalarii", String.Empty,
                                           "Tabelele șablonului de salarii din Access nu au coloanele: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If

            Dim k_db = k_context.TargetDatabase
            For Each k_table In {"TZ_SablonSalarii", "TZ_SablonSalariiLinie", "TZ_Subunitati", "Parteneri"}
                If Not AvcSql.TableExists(k_cn, k_db, k_table) Then
                    k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, k_table, String.Empty,
                                               $"Tabelul «{k_table}» nu există în «{k_db}». Pentru TZ_SablonSalarii* rulați sql/AVACONT/Configurari/SablonSalarii/AV01_15_sablon_salarii.sql " &
                                               "pe AVACONT_SURSA și sincronizați schema; «Parteneri» vine din KBot.Migrator."))
                End If
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings
            If _set.Rows.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_SablonSalarii", String.Empty, "«DefaSalarii» nu are niciun articol; nu se scrie nimic."))
                Return k_findings
            End If
            Dim k_noSub As List(Of Integer) = _set.Rows.Select(Function(k_r) k_r.IdUnitate).Distinct().
                Where(Function(k_u) Not k_context.PlannedSubunits.ContainsKey(k_u) AndAlso Not AvcSubunitStore.UnitMap(k_cn, Nothing).ContainsKey(k_u)).ToList()
            If k_noSub.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_LIPSA, FindingClass.Blocant, "TZ_SablonSalarii", "IdSubunitate",
                                           $"Articolele unităților {String.Join(", ", k_noSub)} merg la subunitatea lor, iar ele nu au încă subunitate. " &
                                           "Bifați pasul «Subunități» (le creează în aceeași migrare) și verificați din nou."))
                Return k_findings
            End If

            Dim k_resolved As List(Of Resolved) = Resolve(k_context, k_cn, Nothing)
            Dim k_clean As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count = 0).ToList()
            Dim k_left As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count > 0).ToList()
            If k_left.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Atentie, "TZ_SablonSalarii", "Explicatie",
                                           "articole lăsate pe dinafară (se scriu doar cele fără probleme); fiecare rând spune de ce", k_left.Count).
                    WithDetail(String.Join(vbLf, k_left.Select(Function(k_r) $"unitatea {k_r.Row.IdUnitate}, «{k_r.Row.Clsf}» {k_r.Row.Explicatie}: {String.Join("; ", k_r.Problems)}"))))
            End If
            If _set.HiddenRows > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Informativ, "TZ_SablonSalarii", String.Empty,
                                           "rânduri ascunse în Access (ștergerea de acolo); nu se migrează", _set.HiddenRows))
            End If
            Dim k_unique As List(Of Resolved) = k_clean.GroupBy(Function(k_r) DedupKey(k_r), StringComparer.Ordinal).Select(Function(k_g) k_g.First()).ToList()
            RowsToWrite = k_unique.Count
            k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_SablonSalarii", String.Empty,
                                       $"{k_unique.Count} articole, {k_unique.Sum(Function(k_r) k_r.Linii.Count)} linii ▸ «TZ_SablonSalarii» / «TZ_SablonSalariiLinie» (din {_set.Rows.Count} articole Access).", k_unique.Count))
            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_SablonSalarii`", k_cn)
                Dim k_existing As Long = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                If k_existing > 0 Then
                    k_findings.Add(New Finding(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, "TZ_SablonSalarii", "Explicatie",
                                               $"«TZ_SablonSalarii» are deja {k_existing} articole. Un articol existent rămâne neschimbat (ce ați editat în fereastră nu se suprascrie); " &
                                               "se adaugă doar cele care lipsesc.", CInt(Math.Min(k_existing, Integer.MaxValue))))
                End If
            End Using
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonSalariiStep.Verify", ex)
            Throw
        End Try
    End Function

    Private Shared Function DedupKey(k_item As Resolved) As String
        Return $"{k_item.IdSubunitate}|{k_item.Clasificatie}|{k_item.Row.Explicatie.ToUpperInvariant()}"
    End Function

    ' Puts the target values on every article, or the reasons it cannot be written. Server I/O: logs and rethrows.
    Private Function Resolve(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction) As List(Of Resolved)
        Try
            Dim k_result As New List(Of Resolved)()
            Dim k_map = AvcSubunitStore.UnitMap(k_cn, k_transaction)
            Dim k_accounts As New HashSet(Of Integer)()
            Using k_cmd As New MySqlCommand($"SELECT `Simbol` FROM {AvcSql.Quote(k_context.CommonDatabase)}.`DefaCont`", k_cn, k_transaction)
                Using k_reader = k_cmd.ExecuteReader()
                    While k_reader.Read()
                        k_accounts.Add(k_reader.GetInt32(0))
                    End While
                End Using
            End Using
            Dim k_partners As New Dictionary(Of String, Long?)(StringComparer.OrdinalIgnoreCase)
            Using k_partnerCmd As New MySqlCommand("SELECT `IdPartener` FROM `Parteneri` WHERE `IdUnitate` = @unit AND `CodPartener` = @cod", k_cn, k_transaction)
                k_partnerCmd.Parameters.Add("@unit", MySqlDbType.Int32)
                k_partnerCmd.Parameters.Add("@cod", MySqlDbType.VarChar)
                For Each k_row In _set.Rows
                    Dim k_item As New Resolved(k_row)
                    k_result.Add(k_item)
                    ResolveArticle(k_row, k_item, k_map, k_context, k_accounts, k_partners, k_partnerCmd)
                Next
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonSalariiStep.Resolve", ex)
            Throw
        End Try
    End Function

    Private Sub ResolveArticle(k_row As AvcSalArticolRow, k_item As Resolved, k_map As Dictionary(Of Integer, AvcSubunitRef), k_context As AvcContext,
                               k_accounts As HashSet(Of Integer), k_partners As Dictionary(Of String, Long?), k_partnerCmd As MySqlCommand)
        Dim k_sub As AvcSubunitRef = Nothing
        Dim k_planned As AvcSubunit = Nothing
        Dim k_capitol As String = String.Empty
        If k_map.TryGetValue(k_row.IdUnitate, k_sub) Then
            k_item.IdSubunitate = k_sub.IdSubunitate
            k_capitol = k_sub.Capitol
        ElseIf k_context.PlannedSubunits.TryGetValue(k_row.IdUnitate, k_planned) Then
            k_capitol = k_planned.Capitol
            ' Planned by this run: its id is not known yet; the write finds it (the verification only checks the rest).
        End If
        If k_row.Cap.Length > 0 AndAlso k_capitol.Length > 0 AndAlso Not String.Equals(k_row.Cap.Trim(), k_capitol, StringComparison.Ordinal) Then
            k_item.Problems.Add($"capitolul «{k_row.Cap}» nu este al subunității unității ({k_capitol})")
        End If
        Dim k_clsf As String = k_row.Clsf.Trim()
        If SixDigits.IsMatch(k_clsf) Then k_clsf = $"{k_clsf.Substring(0, 2)}.{k_clsf.Substring(2, 2)}.{k_clsf.Substring(4, 2)}"
        If Not ClsfRegex.IsMatch(k_clsf) Then k_item.Problems.Add($"clasificația «{k_row.Clsf}» nu este de felul 10.01.01")
        k_item.Clasificatie = k_clsf
        If String.IsNullOrWhiteSpace(k_row.Explicatie) OrElse k_row.Explicatie.Length > 255 Then k_item.Problems.Add("explicația lipsește sau este prea lungă")
        k_item.ContDebit = BaseAccount(k_row.ContD, k_accounts, "angajamentului (debit)", k_item)
        k_item.ContCredit = BaseAccount(k_row.ContC, k_accounts, "angajamentului (credit)", k_item)
        ResolvePartner(k_row.IdUnitate, k_row.PartD, "angajamentului (debit)", k_partners, k_partnerCmd, k_item, k_item.PartDebitMod, k_item.IdPartDebit)
        ResolvePartner(k_row.IdUnitate, k_row.PartC, "angajamentului (credit)", k_partners, k_partnerCmd, k_item, k_item.PartCreditMod, k_item.IdPartCredit)
        For Each k_line In k_row.Linii
            Dim k_resolved As New ResolvedLine() With {.Source = k_line, .EsteCM = k_line.CM}
            Dim k_where As String = $"linia «{k_line.Explicatie}» ({k_line.Tip.ToLowerInvariant()})"
            If String.IsNullOrWhiteSpace(k_line.Explicatie) OrElse k_line.Explicatie.Length > 255 Then k_item.Problems.Add($"{k_where}: explicația lipsește sau este prea lungă")
            k_resolved.ContDebit = BaseAccount(k_line.ContD, k_accounts, "liniei " & k_where & " (debit)", k_item)
            k_resolved.ContCredit = BaseAccount(k_line.ContC, k_accounts, "liniei " & k_where & " (credit)", k_item)
            ResolvePartner(k_row.IdUnitate, k_line.PartD, "liniei " & k_where & " (debit)", k_partners, k_partnerCmd, k_item, k_resolved.PartDebitMod, k_resolved.IdPartDebit)
            ResolvePartner(k_row.IdUnitate, k_line.PartC, "liniei " & k_where & " (credit)", k_partners, k_partnerCmd, k_item, k_resolved.PartCreditMod, k_resolved.IdPartCredit)
            If k_line.Tip = "RETINERE" Then
                ' The debit of a retention IS the credit of its article.
                k_resolved.ContDebit = k_item.ContCredit
                k_resolved.PartDebitMod = k_item.PartCreditMod
                k_resolved.IdPartDebit = k_item.IdPartCredit
            End If
            k_item.Linii.Add(k_resolved)
        Next
        If Not k_item.Linii.Any(Function(k_l) k_l.Source.Tip = "VIRAMENT") Then k_item.Problems.Add("nu are niciun virament (un articol cere cel puțin unul)")
        For Each k_tip In {"RETINERE", "VIRAMENT"}
            Dim k_first As Boolean = True
            For Each k_line In k_item.Linii.Where(Function(k_l) k_l.Source.Tip = k_tip AndAlso k_l.EsteCM)
                If k_first Then
                    k_first = False
                Else
                    k_line.EsteCM = False       ' at most one medical-leave line per kind: the first one stays
                End If
            Next
        Next
    End Sub

    ' The base account: the first seven digits of the Access symbol, which must be in DefaCont.
    Private Shared Function BaseAccount(k_symbol As String, k_accounts As HashSet(Of Integer), k_side As String, k_item As Resolved) As Integer
        Dim k_text As String = k_symbol.Trim()
        Dim k_number As Integer
        If k_text.Length < 7 OrElse Not Integer.TryParse(k_text.Substring(0, 7), NumberStyles.None, CultureInfo.InvariantCulture, k_number) Then
            k_item.Problems.Add($"contul {k_side} «{k_symbol}» nu începe cu un cont de șapte cifre")
            Return 0
        End If
        If Not k_accounts.Contains(k_number) Then
            k_item.Problems.Add($"contul {k_number.ToString(CultureInfo.InvariantCulture)} {k_side} nu este în nomenclatorul de conturi")
            Return 0
        End If
        Return k_number
    End Function

    ' XXX = the partner of the operation, a code = a fixed partner (it must exist), empty = none.
    Private Shared Sub ResolvePartner(k_idUnitate As Integer, k_mark As String, k_side As String, k_partners As Dictionary(Of String, Long?),
                                      k_cmd As MySqlCommand, k_item As Resolved, ByRef k_mode As String, ByRef k_id As Long?)
        Dim k_text As String = k_mark.Trim()
        k_mode = "NIMIC"
        k_id = Nothing
        If k_text.Length = 0 Then Return
        If String.Equals(k_text, "XXX", StringComparison.OrdinalIgnoreCase) Then
            k_mode = "OPERATIE"
            Return
        End If
        Dim k_key As String = $"{k_idUnitate.ToString(CultureInfo.InvariantCulture)}|{k_text}"
        If Not k_partners.TryGetValue(k_key, k_id) Then
            k_cmd.Parameters("@unit").Value = k_idUnitate
            k_cmd.Parameters("@cod").Value = k_text
            Dim k_found As Object = k_cmd.ExecuteScalar()
            k_id = If(k_found Is Nothing OrElse k_found Is DBNull.Value, CType(Nothing, Long?), Convert.ToInt64(k_found, CultureInfo.InvariantCulture))
            k_partners(k_key) = k_id
        End If
        If k_id.HasValue Then
            k_mode = "FIX"
        Else
            k_item.Problems.Add($"partenerul «{k_text}» {k_side} nu este în «Parteneri» (rulați întâi KBot.Migrator, pasul Parteneri)")
        End If
    End Sub

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«DefaSalarii» nu a fost citit din Access.")
            If _set.Rows.Count = 0 Then
                k_progress.Report("«TZ_SablonSalarii»: «DefaSalarii» nu are articole, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("TZ_SablonSalarii", 0), New AvcWritten("TZ_SablonSalariiLinie", 0)}
            End If
            ' The subunits step wrote its subunits inside this transaction: they are found now.
            Dim k_resolved As List(Of Resolved) = Resolve(k_context, k_cn, k_transaction)
            Dim k_noSub As Integer = k_resolved.Where(Function(k_r) k_r.Problems.Count = 0 AndAlso Not k_r.IdSubunitate.HasValue).Count()
            If k_noSub > 0 Then Throw New InvalidOperationException($"{k_noSub} articole nu au subunitate în țintă: bifați pasul «Subunități».")
            Dim k_toWrite As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count = 0).
                GroupBy(Function(k_r) DedupKey(k_r), StringComparer.Ordinal).Select(Function(k_g) k_g.First()).
                OrderBy(Function(k_r) k_r.IdSubunitate.Value).ThenBy(Function(k_r) k_r.Clasificatie, StringComparer.Ordinal).ToList()
            k_dump.WriteComment("TZ_SablonSalarii", "Fostele «DefaSalarii» + VA / PC / PN: articol pe subunitatea unității, conturi de bază, viramentul păstrează legătura cu reținerea.")

            Dim k_headerSql = $"INSERT INTO {AvcSql.Quote("TZ_SablonSalarii")} ({String.Join(", ", HeaderColumns.Select(AddressOf AvcSql.Quote))}) " &
                              $"VALUES ({String.Join(", ", HeaderColumns.Select(Function(c) "@" & c))})"
            Dim k_lineSql = $"INSERT INTO {AvcSql.Quote("TZ_SablonSalariiLinie")} ({String.Join(", ", LineColumns.Select(AddressOf AvcSql.Quote))}) " &
                            $"VALUES ({String.Join(", ", LineColumns.Select(Function(c) "@" & c))})"
            Dim k_written As Integer = 0
            Dim k_skipped As Integer = 0
            Dim k_lines As Integer = 0
            Using k_exists As New MySqlCommand("SELECT COUNT(*) FROM `TZ_SablonSalarii` WHERE `IdSubunitate` = @sub AND `Clasificatie` = @cls AND `Explicatie` = @expl", k_cn, k_transaction)
                k_exists.Parameters.Add("@sub", MySqlDbType.Int64)
                k_exists.Parameters.Add("@cls", MySqlDbType.VarChar)
                k_exists.Parameters.Add("@expl", MySqlDbType.VarChar)
                Using k_headerCmd As New MySqlCommand(k_headerSql, k_cn, k_transaction)
                    For Each k_column In HeaderColumns
                        k_headerCmd.Parameters.Add(New MySqlParameter("@" & k_column, Nothing))
                    Next
                    Using k_lineCmd As New MySqlCommand(k_lineSql, k_cn, k_transaction)
                        For Each k_column In LineColumns
                            k_lineCmd.Parameters.Add(New MySqlParameter("@" & k_column, Nothing))
                        Next
                        For Each k_item In k_toWrite
                            k_cancel.ThrowIfCancellationRequested()
                            k_exists.Parameters("@sub").Value = k_item.IdSubunitate.Value
                            k_exists.Parameters("@cls").Value = k_item.Clasificatie
                            k_exists.Parameters("@expl").Value = k_item.Row.Explicatie
                            If Convert.ToInt64(k_exists.ExecuteScalar(), CultureInfo.InvariantCulture) > 0 Then
                                k_skipped += 1
                                Continue For
                            End If
                            Dim k_header As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                                {"IdSubunitate", k_item.IdSubunitate.Value}, {"Clasificatie", k_item.Clasificatie}, {"Explicatie", k_item.Row.Explicatie},
                                {"ContDebit", k_item.ContDebit}, {"ContCredit", k_item.ContCredit},
                                {"PartDebitMod", k_item.PartDebitMod}, {"IdPartDebit", If(k_item.IdPartDebit.HasValue, CObj(k_item.IdPartDebit.Value), DBNull.Value)},
                                {"PartCreditMod", k_item.PartCreditMod}, {"IdPartCredit", If(k_item.IdPartCredit.HasValue, CObj(k_item.IdPartCredit.Value), DBNull.Value)}}
                            For Each k_column In HeaderColumns
                                k_headerCmd.Parameters("@" & k_column).Value = k_header(k_column)
                            Next
                            k_dump.WriteStatement("TZ_SablonSalarii", AvcSql.RenderStatement(k_headerSql, k_header))
                            k_headerCmd.ExecuteNonQuery()
                            Dim k_idArticol As Long = k_headerCmd.LastInsertedId
                            ' Retentions first (a virament may point at one), then viraments, then cash payments.
                            Dim k_newIds As New Dictionary(Of Long, Long)()
                            Dim k_order As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
                            For Each k_line In k_item.Linii.OrderBy(Function(k_l) If(k_l.Source.Tip = "RETINERE", 0, If(k_l.Source.Tip = "VIRAMENT", 1, 2)))
                                k_order(k_line.Source.Tip) = If(k_order.ContainsKey(k_line.Source.Tip), k_order(k_line.Source.Tip), 0) + 1
                                Dim k_parent As Long = 0
                                Dim k_hasParent As Boolean = k_line.Source.Tip = "VIRAMENT" AndAlso k_line.Source.IdParinteAccess > 0 AndAlso k_newIds.TryGetValue(k_line.Source.IdParinteAccess, k_parent)
                                Dim k_values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                                    {"IdArticol", k_idArticol}, {"Tip", k_line.Source.Tip}, {"IdParinte", If(k_hasParent, CObj(k_parent), DBNull.Value)},
                                    {"Ordine", k_order(k_line.Source.Tip)}, {"Explicatie", k_line.Source.Explicatie},
                                    {"ContDebit", k_line.ContDebit}, {"ContCredit", k_line.ContCredit},
                                    {"PartDebitMod", k_line.PartDebitMod}, {"IdPartDebit", If(k_line.IdPartDebit.HasValue, CObj(k_line.IdPartDebit.Value), DBNull.Value)},
                                    {"PartCreditMod", k_line.PartCreditMod}, {"IdPartCredit", If(k_line.IdPartCredit.HasValue, CObj(k_line.IdPartCredit.Value), DBNull.Value)},
                                    {"EsteCM", If(k_line.EsteCM, 1, 0)}}
                                For Each k_column In LineColumns
                                    k_lineCmd.Parameters("@" & k_column).Value = k_values(k_column)
                                Next
                                k_dump.WriteStatement("TZ_SablonSalariiLinie", AvcSql.RenderStatement(k_lineSql, k_values))
                                k_lineCmd.ExecuteNonQuery()
                                If k_line.Source.Tip = "RETINERE" AndAlso k_line.Source.IdAccess > 0 Then k_newIds(k_line.Source.IdAccess) = k_lineCmd.LastInsertedId
                                k_lines += 1
                            Next
                            k_written += 1
                        Next
                    End Using
                End Using
            End Using
            k_dump.FlushAll()
            If k_skipped > 0 Then k_progress.Report($"«TZ_SablonSalarii»: {k_skipped} articole existau deja și au rămas neschimbate.")
            k_progress.Report($"«TZ_SablonSalarii»: {k_written} articole, {k_lines} linii (confirmate la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("TZ_SablonSalarii", k_written), New AvcWritten("TZ_SablonSalariiLinie", k_lines)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonSalariiStep.Write", ex)
            Throw
        End Try
    End Function

End Class
