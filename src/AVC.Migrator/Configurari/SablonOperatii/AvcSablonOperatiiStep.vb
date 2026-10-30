Imports System.Globalization
Imports System.Text.RegularExpressions
Imports System.Threading
Imports KBot.Common
Imports KBot.Migrator
Imports MySqlConnector

''' <summary>One line of the Access <c>DefaNote</c>: the accounts as Access keeps them (resolved, with sector, source and classification inside) and the partner marks.</summary>
Friend NotInheritable Class AvcSablonLinieRow
    Public Property Forma As String = String.Empty
    ''' <summary>The Access <c>IDClsf</c> of the line (0 = none); the target <c>Clasificatii.IdClsfAcc</c> holds the same number.</summary>
    Public Property Clsf As Long
    Public Property ContDebit As String = String.Empty
    Public Property PartDebit As String = String.Empty
    Public Property ContCredit As String = String.Empty
    Public Property PartCredit As String = String.Empty
    Public Property PA As Boolean
    Public Property Ordonantare As Boolean
    Public Property Chelt As Boolean
End Class

''' <summary>One template of the Access <c>DefaNoteA</c> (the header) with its <c>DefaNote</c> lines.</summary>
Friend NotInheritable Class AvcSablonRow
    Public Property IdUnitate As Integer
    Public Property Ida As Long
    Public Property Explicatie As String = String.Empty
    ''' <summary>The Access type: A, C or M.</summary>
    Public Property TipAccess As String = String.Empty
    Public Property Jurnal As String = String.Empty
    Public Property Fel As String = String.Empty
    Public Property Compartiment As String = String.Empty
    ''' <summary>The overloaded Access column: the classification key (type C), a partner (type M) or a mark (type A).</summary>
    Public Property CodPartener As String = String.Empty
    Public Property FelOP As String = String.Empty
    Public ReadOnly Property Linii As New List(Of AvcSablonLinieRow)()
End Class

''' <summary>Everything read from the Access <c>DefaNoteA</c> / <c>DefaNote</c> of the ticked units.</summary>
Friend NotInheritable Class AvcSablonSet
    Public ReadOnly Property Rows As New List(Of AvcSablonRow)()
    Public ReadOnly Property UnitsWithoutTable As New List(Of Integer)()
    Public ReadOnly Property MissingColumns As New List(Of String)()
End Class

''' <summary>Reads the <c>DefaNoteA</c> + <c>DefaNote</c> of every ticked unit's file.</summary>
Friend NotInheritable Class AvcSablonReader

    Private Sub New()
    End Sub

    Public Const AccessTable As String = "DefaNoteA"
    Public Const AccessLines As String = "DefaNote"

    ''' <summary>Access I/O: logs and rethrows.</summary>
    Public Shared Function Read(k_context As AvcContext) As AvcSablonSet
        Try
            Dim k_result As New AvcSablonSet()
            For Each k_unit In k_context.Units
                If Not k_unit.HasUnitFile Then Continue For
                Using k_cn = AccessProvider.Open(k_unit.UnitFilePath, k_context.AccessPassword)
                    Dim k_headerTable = AccessSchema.ResolveTableName(k_cn, AccessTable)
                    Dim k_lineTable = AccessSchema.ResolveTableName(k_cn, AccessLines)
                    If k_headerTable Is Nothing OrElse k_lineTable Is Nothing Then
                        k_result.UnitsWithoutTable.Add(k_unit.IdUnitate)
                        Continue For
                    End If
                    Dim k_byIda As New Dictionary(Of Long, AvcSablonRow)()
                    Using k_reader = AccessSchema.OpenReader(k_cn, k_headerTable)
                        For Each k_column In {"IDA", "Explicatie", "Tip", "Jurnal", "Fel", "Compartiment", "CodPartener", "FelOP"}
                            If Not k_reader.HasColumn(k_column) AndAlso Not k_result.MissingColumns.Contains(k_column) Then
                                k_result.MissingColumns.Add(k_column)
                            End If
                        Next
                        While k_reader.Read()
                            Dim k_ida As Long = AsLong(k_reader.ValueOrMissing("IDA"))
                            If k_ida = 0 OrElse k_byIda.ContainsKey(k_ida) Then Continue While
                            Dim k_row As New AvcSablonRow() With {
                                .IdUnitate = k_unit.IdUnitate, .Ida = k_ida,
                                .Explicatie = AvcSql.AsText(k_reader.ValueOrMissing("Explicatie")),
                                .TipAccess = AvcSql.AsText(k_reader.ValueOrMissing("Tip")).ToUpperInvariant(),
                                .Jurnal = AvcSql.AsText(k_reader.ValueOrMissing("Jurnal")),
                                .Fel = AvcSql.AsText(k_reader.ValueOrMissing("Fel")).ToUpperInvariant(),
                                .Compartiment = AvcSql.AsText(k_reader.ValueOrMissing("Compartiment")),
                                .CodPartener = AvcSql.AsText(k_reader.ValueOrMissing("CodPartener")),
                                .FelOP = AvcSql.AsText(k_reader.ValueOrMissing("FelOP"))}
                            k_byIda(k_ida) = k_row
                            k_result.Rows.Add(k_row)
                        End While
                    End Using
                    Using k_reader = AccessSchema.OpenReader(k_cn, k_lineTable)
                        For Each k_column In {"IDA", "Forma", "Clsf", "ContDebit", "PartDebit", "ContCredit", "PartCredit", "Ordonantare", "PA", "Chelt"}
                            If Not k_reader.HasColumn(k_column) AndAlso Not k_result.MissingColumns.Contains(k_column) Then
                                k_result.MissingColumns.Add(k_column)
                            End If
                        Next
                        While k_reader.Read()
                            Dim k_header As AvcSablonRow = Nothing
                            If Not k_byIda.TryGetValue(AsLong(k_reader.ValueOrMissing("IDA")), k_header) Then Continue While
                            k_header.Linii.Add(New AvcSablonLinieRow() With {
                                .Forma = AvcSql.AsText(k_reader.ValueOrMissing("Forma")),
                                .Clsf = AsLong(k_reader.ValueOrMissing("Clsf")),
                                .ContDebit = AvcSql.AsText(k_reader.ValueOrMissing("ContDebit")),
                                .PartDebit = AvcSql.AsText(k_reader.ValueOrMissing("PartDebit")),
                                .ContCredit = AvcSql.AsText(k_reader.ValueOrMissing("ContCredit")),
                                .PartCredit = AvcSql.AsText(k_reader.ValueOrMissing("PartCredit")),
                                .PA = AsFlag(k_reader.ValueOrMissing("PA")),
                                .Ordonantare = AsFlag(k_reader.ValueOrMissing("Ordonantare")),
                                .Chelt = AsFlag(k_reader.ValueOrMissing("Chelt"))})
                        End While
                    End Using
                End Using
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonReader.Read", ex)
            Throw
        End Try
    End Function

    Private Shared Function AsLong(k_value As Object) As Long
        If k_value Is Nothing OrElse k_value Is DBNull.Value Then Return 0
        Try
            Return Convert.ToInt64(k_value, CultureInfo.InvariantCulture)
        Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidCastException OrElse TypeOf ex Is OverflowException
            GlobalErrorLog.Write("AvcSablonReader.AsLong", ex)
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
''' Stage 1, step 5 (slice AV01-14): the Access <c>DefaNoteA</c> + <c>DefaNote</c> (the operation templates, button OCI, form <c>Operatii2020</c>) become
''' <c>TZ_SablonOperatii</c> + <c>TZ_SablonOperatiiLinie</c> (named SablonOperatii by the operator, 27.10.2026).
''' </summary>
''' <remarks>
''' <para>
''' Type: the Access types <c>C</c> and <c>M</c> become <c>M</c> (an operation with a classification and a partner on one side), <c>A</c> stays <c>A</c> (and then the kind
''' is <c>A</c>). The classification key of type M is the header's <c>CodPartener</c> when the type was <c>C</c>; otherwise (and when that is not a key) it is read
''' from the classification of the first line, through <c>Clasificatii.IdClsfAcc</c> (the Access <c>IDClsf</c>).
''' </para>
''' <para>
''' Lines: the account is the BASE account, the first seven digits of the resolved Access symbol, and the analytic is what follows the dot of the symbol
''' (<c>421000002A.01</c> -> 4210000 + 01; slice AV01-17; it must exist in <c>TZ_PlanCont</c>); the partner mark <c>XXX</c> becomes «the partner of the operation»,
''' a code becomes a fixed partner (it must exist in <c>Parteneri</c>), empty becomes none. The Access <c>DefaNote.Clsf</c> (the <c>IDClsf</c> of the line) becomes
''' <c>IdClsf</c> through <c>Clasificatii.IdClsfAcc</c>: type M and type A both keep it; type A keeps no form, and keeps the three flags only when the line has a
''' classification. A template with anything the target would refuse (unknown type, journal or
''' account missing, a partner that is not in <c>Parteneri</c>, a classification of a type-A line that is not in <c>Clasificatii</c>, an analytic that is not in the chart
''' of accounts, a repeated form ...) is LEFT OUT whole and named in the verification: a template is never half written.
''' </para>
''' <para>
''' The write adds the templates that are not there yet (same journal, type, kind, explanation and key) and changes nothing else: a template the operator edited in the
''' window is not overwritten. The same template in two unit files is written once.
''' </para>
''' </remarks>
Friend NotInheritable Class AvcSablonOperatiiStep
    Inherits AvcStep

    Private Shared ReadOnly KeyRegex As New Regex("^[0-9X.]{1,30}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly FormRegex As New Regex("^[0-9]{2}\.[0-9]{2}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly HeaderColumns As String() = {"IdJurnal", "Tip", "Fel", "Explicatie", "Compartiment", "FelOP", "Cheie"}
    Private Shared ReadOnly AnalyticRegex As New Regex("^[0-9A-Z_]{1,10}$", RegexOptions.CultureInvariant)
    Private Shared ReadOnly LineColumns As String() = {"IdSablon", "Forma", "IdClsf", "ContDebit", "AnaliticDebit", "ContCredit", "AnaliticCredit", "PartDebitMod", "IdPartDebit",
                                                       "PartCreditMod", "IdPartCredit", "PA", "Ordonantare", "Chelt"}
    ' The columns slice AV01-17 added to TZ_SablonOperatiiLinie.
    Private Shared ReadOnly NewLineColumns As String() = {"IdClsf", "AnaliticDebit", "AnaliticCredit"}

    ''' <summary>The classification an Access line points at, as the target holds it (<c>Clasificatii.IdClsfAcc</c> = the Access id).</summary>
    Private NotInheritable Class ClassRef
        Public Property Key As String = String.Empty
        Public Property Form As String = String.Empty
        Public Property IdClsf As Long
    End Class

    ''' <summary>A line with its target values.</summary>
    Private NotInheritable Class ResolvedLine
        Public Property Forma As String = String.Empty
        Public Property IdClsf As Long?
        Public Property ContDebit As Integer
        Public Property AnaliticDebit As String = String.Empty
        Public Property ContCredit As Integer
        Public Property AnaliticCredit As String = String.Empty
        Public Property PartDebitMod As String = "NIMIC"
        Public Property IdPartDebit As Long?
        Public Property PartCreditMod As String = "NIMIC"
        Public Property IdPartCredit As Long?
        Public Property PA As Boolean
        Public Property Ordonantare As Boolean
        Public Property Chelt As Boolean
    End Class

    ''' <summary>A template with its target values, or the reasons it is left out.</summary>
    Private NotInheritable Class Resolved
        Public Sub New(k_row As AvcSablonRow)
            Row = k_row
        End Sub

        Public ReadOnly Property Row As AvcSablonRow
        Public Property SursaSector As String = String.Empty
        Public Property IdJurnal As Long?
        ''' <summary>True when an analytic of a line is not in TZ_PlanCont YET (Verify only: the PlanCont step of this run writes it before this step).</summary>
        Public Property PlanPending As Boolean
        Public Property Tip As String = "M"
        Public Property Fel As String = "E"
        Public Property Cheie As String = String.Empty
        Public ReadOnly Property Linii As New List(Of ResolvedLine)()
        Public ReadOnly Property Problems As New List(Of String)()

        Public ReadOnly Property DedupKey As String
            Get
                Return $"{SursaSector}|{Row.Jurnal.ToUpperInvariant()}|{Tip}|{Fel}|{Row.Explicatie.ToUpperInvariant()}|{Cheie}"
            End Get
        End Property
    End Class

    Private _set As AvcSablonSet

    Public Overrides ReadOnly Property Key As String
        Get
            Return "SablonOperatii"
        End Get
    End Property

    Public Overrides ReadOnly Property Stage As String
        Get
            Return "Etapa 1 - Configurare"
        End Get
    End Property

    Public Overrides ReadOnly Property Title As String
        Get
            Return "Șabloane de operații"
        End Get
    End Property

    Public Overrides ReadOnly Property AccessTable As String
        Get
            Return AvcSablonReader.AccessTable
        End Get
    End Property

    Public Overrides ReadOnly Property TargetTables As IReadOnlyList(Of String)
        Get
            Return {"TZ_SablonOperatii", "TZ_SablonOperatiiLinie"}
        End Get
    End Property

    Public Overrides ReadOnly Property DependsOn As IReadOnlyList(Of String)
        Get
            Return {"Subunitati", "Jurnale", "PlanCont"}
        End Get
    End Property

    Public Overrides Sub Reset()
        MyBase.Reset()
        _set = Nothing
    End Sub

    Public Overrides Sub ReadAccess(k_context As AvcContext)
        _set = AvcSablonReader.Read(k_context)
        AccessRows = _set.Rows.Count
        RowsToWrite = _set.Rows.Count
    End Sub

    Public Overrides Function Verify(k_context As AvcContext, k_cn As MySqlConnection) As List(Of Finding)
        Try
            Dim k_findings As New List(Of Finding)()
            If _set Is Nothing Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty, "«DefaNoteA» nu a fost citit din Access."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count = k_context.Units.Count Then
                k_findings.Add(New Finding(AvcKinds.SURSA_LIPSA, FindingClass.Blocant, AccessTable, String.Empty,
                                           "Fișierele Access ale unităților bifate nu au tabelele «DefaNoteA» și «DefaNote»."))
                Return k_findings
            End If
            If _set.UnitsWithoutTable.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Informativ, AccessTable, String.Empty,
                                           "Fișierul Access nu are «DefaNoteA» / «DefaNote» la unitățile: " & String.Join(", ", _set.UnitsWithoutTable) & "."))
            End If
            If _set.MissingColumns.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Blocant, "TZ_SablonOperatii", String.Empty,
                                           "«DefaNoteA» / «DefaNote» din Access nu au coloanele: " & String.Join(", ", _set.MissingColumns) & "."))
                Return k_findings
            End If

            Dim k_db = k_context.TargetDatabase
            For Each k_table In {"TZ_SablonOperatii", "TZ_SablonOperatiiLinie", "TZ_Jurnale", "TZ_Subunitati", "TZ_PlanCont", "Parteneri", "Clasificatii"}
                If Not AvcSql.TableExists(k_cn, k_db, k_table) Then
                    k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, k_table, String.Empty,
                                               $"Tabelul «{k_table}» nu există în «{k_db}». Pentru TZ_SablonOperatii* rulați sql/AVACONT/Configurari/SablonOperatii/AV01_14_sablon_operatii.sql " &
                                               "pe AVACONT_SURSA și sincronizați schema; «TZ_PlanCont» vine din AV01_04; «Parteneri» și «Clasificatii» vin din KBot.Migrator."))
                End If
            Next
            If k_findings.Any(Function(f) f.IsBlocking) Then Return k_findings
            Dim k_noColumns As List(Of String) = AvcSql.MissingColumns(k_cn, k_db, "TZ_SablonOperatiiLinie", NewLineColumns)
            If k_noColumns.Count > 0 Then
                k_findings.Add(New Finding(Finding.TABEL_LIPSA, FindingClass.Blocant, "TZ_SablonOperatiiLinie", String.Join(", ", k_noColumns),
                                           "Liniile de șablon nu au coloanele: " & String.Join(", ", k_noColumns) & ". Rulați sql/AVACONT/Configurari/SablonOperatii/AV01_17_sablon_linie_clasificatie_analitic.sql " &
                                           "pe AVACONT_SURSA și sincronizați schema."))
                Return k_findings
            End If
            If _set.Rows.Count = 0 Then
                k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_SablonOperatii", String.Empty, "«DefaNoteA» nu are niciun rând; nu se scrie nimic."))
                Return k_findings
            End If

            Dim k_missingUnits As List(Of Integer) = UnitsWithoutSubunit(k_context, k_cn, Nothing)
            If k_missingUnits.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SUBUNITATE_LIPSA, FindingClass.Blocant, "TZ_SablonOperatii", "IdJurnal",
                                           $"Jurnalele șabloanelor unităților {String.Join(", ", k_missingUnits)} iau sectorul și sursa subunității lor, iar ele nu au încă subunitate. " &
                                           "Bifați pasul «Subunități» (le creează în aceeași migrare) și verificați din nou."))
                Return k_findings
            End If

            Dim k_resolved As List(Of Resolved) = Resolve(k_context, k_cn, Nothing)
            Dim k_clean As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count = 0).ToList()
            Dim k_left As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count > 0).ToList()
            If k_left.Count > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Atentie, "TZ_SablonOperatii", "Explicatie",
                                           "șabloane lăsate pe dinafară (se scriu doar cele fără probleme); fiecare rând spune de ce", k_left.Count).
                    WithDetail(String.Join(vbLf, k_left.Select(Function(k_r) $"unitatea {k_r.Row.IdUnitate}, «{k_r.Row.Explicatie}»: {String.Join("; ", k_r.Problems)}"))))
            End If
            Dim k_unique As List(Of Resolved) = k_clean.GroupBy(Function(k_r) k_r.DedupKey, StringComparer.Ordinal).Select(Function(k_g) k_g.First()).ToList()
            If k_unique.Count < k_clean.Count Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Informativ, "TZ_SablonOperatii", "Explicatie",
                                           "același șablon în fișierele mai multor unități; se scrie o singură dată", k_clean.Count - k_unique.Count))
            End If
            RowsToWrite = k_unique.Count
            Dim k_notYet As Integer = k_unique.Where(Function(k_r) Not k_r.IdJurnal.HasValue).Count()
            If k_notYet > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Informativ, "TZ_SablonOperatii", "IdJurnal",
                                           "șabloane al căror jurnal nu este încă în «TZ_Jurnale»; se caută din nou la scriere, după pasul «Jurnale» (bifați-l); " &
                                           "un șablon al cărui jurnal tot lipsește nu se scrie", k_notYet))
            End If
            Dim k_planNotYet As Integer = k_unique.Where(Function(k_r) k_r.PlanPending).Count()
            If k_planNotYet > 0 Then
                k_findings.Add(New Finding(AvcKinds.SABLON_INVALID, FindingClass.Informativ, "TZ_SablonOperatii", "AnaliticDebit",
                                           "șabloane cu conturi analitice care nu sunt încă în «TZ_PlanCont»; se caută din nou la scriere, după pasul «PlanCont» (bifați-l); " &
                                           "un șablon al cărui analitic tot lipsește nu se scrie", k_planNotYet))
            End If
            Dim k_mCount As Integer = k_unique.Where(Function(k_r) k_r.Tip = "M").Count()
            k_findings.Add(New Finding(AvcKinds.REZUMAT, FindingClass.Informativ, "TZ_SablonOperatii", String.Empty,
                                       $"{k_unique.Count} șabloane ({k_mCount} cu clasificație, {k_unique.Count - k_mCount} diverse), {k_unique.Sum(Function(k_r) k_r.Linii.Count)} linii " &
                                       $"▸ «TZ_SablonOperatii» / «TZ_SablonOperatiiLinie» (din {_set.Rows.Count} rânduri Access).", k_unique.Count))

            Using k_cmd As New MySqlCommand("SELECT COUNT(*) FROM `TZ_SablonOperatii`", k_cn)
                Dim k_existing As Long = Convert.ToInt64(k_cmd.ExecuteScalar(), CultureInfo.InvariantCulture)
                If k_existing > 0 Then
                    k_findings.Add(New Finding(Finding.RANDURI_EXISTENTE, FindingClass.Atentie, "TZ_SablonOperatii", "Explicatie",
                                               $"«TZ_SablonOperatii» are deja {k_existing} șabloane. Un șablon existent rămâne neschimbat (ce ați editat în fereastră nu se suprascrie); " &
                                               "se adaugă doar cele care lipsesc.", CInt(Math.Min(k_existing, Integer.MaxValue))))
                End If
            End Using
            Return k_findings
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonOperatiiStep.Verify", ex)
            Throw
        End Try
    End Function

    ' The units that have templates and no subunit (neither in the target nor planned by the subunits step of this run).
    Private Function UnitsWithoutSubunit(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction) As List(Of Integer)
        Dim k_map = AvcSubunitStore.UnitMap(k_cn, k_transaction)
        Return _set.Rows.Select(Function(k_r) k_r.IdUnitate).Distinct().
            Where(Function(k_u) Not k_map.ContainsKey(k_u) AndAlso Not k_context.PlannedSubunits.ContainsKey(k_u)).OrderBy(Function(k_u) k_u).ToList()
    End Function

    ' Puts the target values on every template, or the reasons it cannot be written. Server I/O: logs and rethrows.
    Private Function Resolve(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction) As List(Of Resolved)
        Try
            Dim k_result As New List(Of Resolved)()
            Dim k_map = AvcSubunitStore.UnitMap(k_cn, k_transaction)
            Dim k_accounts As HashSet(Of Integer) = LoadAccounts(k_context, k_cn, k_transaction)
            Dim k_journals As Dictionary(Of String, Long) = LoadJournals(k_cn, k_transaction)
            Dim k_analytics As HashSet(Of String) = LoadAnalytics(k_cn, k_transaction)
            ' At Verify the chart of accounts of this run is not written yet: an analytic that is missing then is only noted, and a problem at the write.
            Dim k_atWrite As Boolean = k_transaction IsNot Nothing
            Dim k_partners As New Dictionary(Of String, Long?)(StringComparer.OrdinalIgnoreCase)
            Dim k_classes As New Dictionary(Of String, ClassRef)(StringComparer.Ordinal)
            Using k_partnerCmd As New MySqlCommand("SELECT `IdPartener` FROM `Parteneri` WHERE `IdUnitate` = @unit AND `CodPartener` = @cod", k_cn, k_transaction)
                Using k_classCmd As New MySqlCommand("SELECT `ClsfX`, `Subcapitol`, `IDClsf` FROM `Clasificatii` WHERE `IdClsfAcc` = @id AND `IdUnitate` = @unit LIMIT 1", k_cn, k_transaction)
                    k_partnerCmd.Parameters.Add("@unit", MySqlDbType.Int32)
                    k_partnerCmd.Parameters.Add("@cod", MySqlDbType.VarChar)
                    k_classCmd.Parameters.Add("@id", MySqlDbType.Int64)
                    k_classCmd.Parameters.Add("@unit", MySqlDbType.Int32)
                    For Each k_row In _set.Rows
                        Dim k_item As New Resolved(k_row)
                        k_result.Add(k_item)
                        ResolveHeader(k_row, k_item, k_map, k_context, k_journals, k_cn, k_transaction)
                        If k_item.Linii.Count = 0 AndAlso k_row.Linii.Count = 0 Then k_item.Problems.Add("nu are nicio linie")
                        For Each k_line In k_row.Linii
                            ResolveLine(k_row, k_line, k_item, k_accounts, k_analytics, k_atWrite, k_partners, k_partnerCmd, k_classes, k_classCmd)
                        Next
                        FinishKey(k_row, k_item, k_classes, k_classCmd)
                        CheckTemplate(k_item)
                    Next
                End Using
            End Using
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonOperatiiStep.Resolve", ex)
            Throw
        End Try
    End Function

    Private Shared Function LoadAccounts(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction) As HashSet(Of Integer)
        Dim k_result As New HashSet(Of Integer)()
        Using k_cmd As New MySqlCommand($"SELECT `Simbol` FROM {AvcSql.Quote(k_context.CommonDatabase)}.`DefaCont`", k_cn, k_transaction)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_result.Add(k_reader.GetInt32(0))
                End While
            End Using
        End Using
        Return k_result
    End Function

    ' The accounts with an analytic the chart of accounts has, as «base|analytic».
    Private Shared Function LoadAnalytics(k_cn As MySqlConnection, k_transaction As MySqlTransaction) As HashSet(Of String)
        Dim k_result As New HashSet(Of String)(StringComparer.Ordinal)
        Using k_cmd As New MySqlCommand("SELECT DISTINCT `Cont`, `Analitic` FROM `TZ_PlanCont` WHERE `Analitic` IS NOT NULL AND `Analitic` <> ''", k_cn, k_transaction)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_result.Add($"{k_reader.GetInt32(0).ToString(CultureInfo.InvariantCulture)}|{k_reader.GetString(1)}")
                End While
            End Using
        End Using
        Return k_result
    End Function

    Private Shared Function LoadJournals(k_cn As MySqlConnection, k_transaction As MySqlTransaction) As Dictionary(Of String, Long)
        Dim k_result As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
        Using k_cmd As New MySqlCommand("SELECT `IdJurnal`, `SursaSector`, `Cod` FROM `TZ_Jurnale`", k_cn, k_transaction)
            Using k_reader = k_cmd.ExecuteReader()
                While k_reader.Read()
                    k_result($"{k_reader.GetString(1)}|{k_reader.GetString(2)}") = k_reader.GetInt64(0)
                End While
            End Using
        End Using
        Return k_result
    End Function

    ' Type, kind, journal. Verify and Write both go through here; at Verify time a journal the Jurnale step of this run will write is not in the target yet,
    ' so a missing journal is only a problem when the table has been written (the caller decides: see Write).
    Private Sub ResolveHeader(k_row As AvcSablonRow, k_item As Resolved, k_map As Dictionary(Of Integer, AvcSubunitRef), k_context As AvcContext,
                              k_journals As Dictionary(Of String, Long), k_cn As MySqlConnection, k_transaction As MySqlTransaction)
        Dim k_sub As AvcSubunitRef = Nothing
        Dim k_planned As AvcSubunit = Nothing
        If k_map.TryGetValue(k_row.IdUnitate, k_sub) Then
            k_item.SursaSector = k_sub.SursaSector
        ElseIf k_context.PlannedSubunits.TryGetValue(k_row.IdUnitate, k_planned) Then
            k_item.SursaSector = k_planned.SursaSector
        End If
        Select Case k_row.TipAccess
            Case "A"
                k_item.Tip = "A"
                k_item.Fel = "A"
            Case "C", "M"
                k_item.Tip = "M"
                If k_row.Fel = "E" OrElse k_row.Fel = "P" Then
                    k_item.Fel = k_row.Fel
                Else
                    k_item.Problems.Add($"felul «{k_row.Fel}» nu este evidențiere (E) sau plată (P), cum cere o operație cu clasificație")
                End If
            Case Else
                k_item.Problems.Add($"tipul «{k_row.TipAccess}» nu este A, C sau M")
        End Select
        If String.IsNullOrWhiteSpace(k_row.Explicatie) Then k_item.Problems.Add("fără explicație")
        If k_row.Explicatie.Length > 255 OrElse k_row.Compartiment.Length > 50 OrElse k_row.FelOP.Length > 50 Then k_item.Problems.Add("explicația, compartimentul sau felul OP sunt prea lungi")
        Dim k_id As Long = 0
        If k_item.SursaSector.Length > 0 AndAlso k_journals.TryGetValue($"{k_item.SursaSector}|{k_row.Jurnal.ToUpperInvariant()}", k_id) Then
            k_item.IdJurnal = k_id
        End If
    End Sub

    Private Sub ResolveLine(k_row As AvcSablonRow, k_line As AvcSablonLinieRow, k_item As Resolved, k_accounts As HashSet(Of Integer),
                            k_analytics As HashSet(Of String), k_atWrite As Boolean,
                            k_partners As Dictionary(Of String, Long?), k_partnerCmd As MySqlCommand,
                            k_classes As Dictionary(Of String, ClassRef), k_classCmd As MySqlCommand)
        Dim k_resolved As New ResolvedLine() With {.PA = k_line.PA, .Ordonantare = k_line.Ordonantare, .Chelt = k_line.Chelt}
        k_resolved.ContDebit = BaseAccount(k_line.ContDebit, k_accounts, k_analytics, k_atWrite, "debit", k_item, k_resolved.AnaliticDebit)
        k_resolved.ContCredit = BaseAccount(k_line.ContCredit, k_accounts, k_analytics, k_atWrite, "credit", k_item, k_resolved.AnaliticCredit)
        ResolvePartner(k_row, k_line.PartDebit, "debit", k_partners, k_partnerCmd, k_item, k_resolved.PartDebitMod, k_resolved.IdPartDebit)
        ResolvePartner(k_row, k_line.PartCredit, "credit", k_partners, k_partnerCmd, k_item, k_resolved.PartCreditMod, k_resolved.IdPartCredit)
        ' The classification of the line (Access DefaNote.Clsf = Clasificatii.IdClsfAcc).
        Dim k_class As ClassRef = Nothing
        If k_line.Clsf > 0 Then k_class = LookUpClass(k_row.IdUnitate, k_line.Clsf, k_classes, k_classCmd)
        Dim k_form As String = k_line.Forma.Trim()
        If k_form.Length = 0 AndAlso k_item.Tip = "M" AndAlso k_class IsNot Nothing Then k_form = k_class.Form
        If k_form.Length > 0 AndAlso Not FormRegex.IsMatch(k_form) Then k_item.Problems.Add($"forma «{k_form}» nu este de felul NN.NN")
        k_resolved.Forma = k_form
        If k_class IsNot Nothing Then k_resolved.IdClsf = k_class.IdClsf
        If k_item.Tip = "A" Then
            ' Type A: no form; the classification belongs to the line, and so do the flags, which need it.
            k_resolved.Forma = String.Empty
            If k_line.Clsf > 0 AndAlso k_class Is Nothing Then
                k_item.Problems.Add($"clasificația liniei (IDClsf {k_line.Clsf.ToString(CultureInfo.InvariantCulture)}) nu este în «Clasificatii» (rulați KBot.Migrator, pasul Clasificații)")
            End If
            If Not k_resolved.IdClsf.HasValue Then
                k_resolved.PA = False
                k_resolved.Ordonantare = False
                k_resolved.Chelt = False
            End If
        Else
            If k_resolved.PA AndAlso k_item.Fel <> "E" Then k_resolved.PA = False
            If k_resolved.Ordonantare AndAlso k_item.Fel <> "P" Then k_resolved.Ordonantare = False
        End If
        k_item.Linii.Add(k_resolved)
    End Sub

    ' The base account: the first seven digits of the Access symbol, which must be in DefaCont; what follows the dot is the analytic (it must be in TZ_PlanCont).
    Private Shared Function BaseAccount(k_symbol As String, k_accounts As HashSet(Of Integer), k_analytics As HashSet(Of String), k_atWrite As Boolean,
                                        k_side As String, k_item As Resolved, ByRef k_analytic As String) As Integer
        Dim k_text As String = k_symbol.Trim()
        k_analytic = String.Empty
        Dim k_dot As Integer = k_text.IndexOf("."c)
        If k_dot >= 0 Then
            k_analytic = k_text.Substring(k_dot + 1).Trim().ToUpperInvariant()
            k_text = k_text.Substring(0, k_dot)
        End If
        Dim k_number As Integer
        If k_text.Length < 7 OrElse Not Integer.TryParse(k_text.Substring(0, 7), NumberStyles.None, CultureInfo.InvariantCulture, k_number) Then
            k_item.Problems.Add($"contul de la {k_side} «{k_symbol}» nu începe cu un cont de șapte cifre")
            k_analytic = String.Empty
            Return 0
        End If
        If Not k_accounts.Contains(k_number) Then
            k_item.Problems.Add($"contul {k_number.ToString(CultureInfo.InvariantCulture)} de la {k_side} nu este în nomenclatorul de conturi")
            k_analytic = String.Empty
            Return 0
        End If
        If k_analytic.Length > 0 Then
            If Not AnalyticRegex.IsMatch(k_analytic) Then
                k_item.Problems.Add($"analiticul «{k_analytic}» al contului {k_number.ToString(CultureInfo.InvariantCulture)} de la {k_side} nu are doar cifre, litere mari și _ (cel mult 10)")
                k_analytic = String.Empty
            ElseIf Not k_analytics.Contains($"{k_number.ToString(CultureInfo.InvariantCulture)}|{k_analytic}") Then
                If k_atWrite Then
                    k_item.Problems.Add($"contul {k_number.ToString(CultureInfo.InvariantCulture)}.{k_analytic} de la {k_side} nu este în planul de conturi (rulați pasul «PlanCont»)")
                Else
                    k_item.PlanPending = True
                End If
            End If
        End If
        Return k_number
    End Function

    ' XXX = the partner of the operation, a code = a fixed partner (it must exist), empty = none.
    Private Shared Sub ResolvePartner(k_row As AvcSablonRow, k_mark As String, k_side As String, k_partners As Dictionary(Of String, Long?),
                                      k_cmd As MySqlCommand, k_item As Resolved, ByRef k_mode As String, ByRef k_id As Long?)
        Dim k_text As String = k_mark.Trim()
        k_mode = "NIMIC"
        k_id = Nothing
        If k_text.Length = 0 Then Return
        If String.Equals(k_text, "XXX", StringComparison.OrdinalIgnoreCase) Then
            k_mode = "OPERATIE"
            Return
        End If
        Dim k_key As String = $"{k_row.IdUnitate.ToString(CultureInfo.InvariantCulture)}|{k_text}"
        If Not k_partners.TryGetValue(k_key, k_id) Then
            k_cmd.Parameters("@unit").Value = k_row.IdUnitate
            k_cmd.Parameters("@cod").Value = k_text
            Dim k_found As Object = k_cmd.ExecuteScalar()
            k_id = If(k_found Is Nothing OrElse k_found Is DBNull.Value, CType(Nothing, Long?), Convert.ToInt64(k_found, CultureInfo.InvariantCulture))
            k_partners(k_key) = k_id
        End If
        If k_id.HasValue Then
            k_mode = "FIX"
        Else
            k_item.Problems.Add($"partenerul «{k_text}» de la {k_side} nu este în «Parteneri» (rulați întâi KBot.Migrator, pasul Parteneri)")
        End If
    End Sub

    ' The key and the subchapter of the classification an Access line points at (the target holds the Access id in IdClsfAcc); Nothing = not found.
    Private Shared Function LookUpClass(k_idUnitate As Integer, k_idClsf As Long, k_classes As Dictionary(Of String, ClassRef), k_cmd As MySqlCommand) As ClassRef
        Dim k_cacheKey As String = $"{k_idUnitate.ToString(CultureInfo.InvariantCulture)}|{k_idClsf.ToString(CultureInfo.InvariantCulture)}"
        Dim k_value As ClassRef = Nothing
        If k_classes.TryGetValue(k_cacheKey, k_value) Then Return k_value
        k_cmd.Parameters("@id").Value = k_idClsf
        k_cmd.Parameters("@unit").Value = k_idUnitate
        Using k_reader = k_cmd.ExecuteReader()
            If k_reader.Read() Then
                k_value = New ClassRef() With {
                    .Key = Convert.ToString(k_reader.GetValue(0), CultureInfo.InvariantCulture), .Form = Convert.ToString(k_reader.GetValue(1), CultureInfo.InvariantCulture),
                    .IdClsf = Convert.ToInt64(k_reader.GetValue(2), CultureInfo.InvariantCulture)}
            End If
        End Using
        k_classes(k_cacheKey) = k_value
        Return k_value
    End Function

    ' The key of a type-M template: the header's CodPartener when it is one (the old type C), else the classification of the first line that has one.
    Private Shared Sub FinishKey(k_row As AvcSablonRow, k_item As Resolved, k_classes As Dictionary(Of String, ClassRef), k_cmd As MySqlCommand)
        If k_item.Tip <> "M" Then Return
        If String.Equals(k_row.TipAccess, "C", StringComparison.Ordinal) AndAlso KeyRegex.IsMatch(k_row.CodPartener.Trim().ToUpperInvariant()) Then
            k_item.Cheie = k_row.CodPartener.Trim().ToUpperInvariant()
            Return
        End If
        For Each k_line In k_row.Linii.Where(Function(k_l) k_l.Clsf > 0)
            Dim k_class As ClassRef = LookUpClass(k_row.IdUnitate, k_line.Clsf, k_classes, k_cmd)
            If k_class IsNot Nothing AndAlso KeyRegex.IsMatch(k_class.Key.ToUpperInvariant()) Then
                k_item.Cheie = k_class.Key.ToUpperInvariant()
                Return
            End If
        Next
        k_item.Problems.Add("nu se poate stabili cheia clasificației (nici antetul, nici liniile nu duc la o clasificație din «Clasificatii»)")
    End Sub

    ' What the target would refuse: a repeated form, a partner on both sides or on none for type M.
    Private Shared Sub CheckTemplate(k_item As Resolved)
        Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_line In k_item.Linii.Where(Function(k_l) k_l.Forma.Length > 0)
            If Not k_seen.Add(k_line.Forma) Then
                k_item.Problems.Add($"forma {k_line.Forma} apare de două ori")
                Exit For
            End If
        Next
        If k_item.Tip = "M" Then
            Dim k_bad As Integer = k_item.Linii.Where(Function(k_l) (k_l.PartDebitMod <> "NIMIC") = (k_l.PartCreditMod <> "NIMIC")).Count()
            If k_bad > 0 Then k_item.Problems.Add($"{k_bad} linii fără partener sau cu partener pe amândouă părțile (o operație cu clasificație are partener pe o singură parte)")
        End If
        If k_item.Linii.Count = 0 AndAlso Not k_item.Problems.Contains("nu are nicio linie") Then k_item.Problems.Add("nu are nicio linie")
    End Sub

    Public Overrides Function Write(k_context As AvcContext, k_cn As MySqlConnection, k_transaction As MySqlTransaction,
                                    k_dump As SqlDumpWriter, k_progress As IProgress(Of String),
                                    k_cancel As CancellationToken) As List(Of AvcWritten)
        Try
            If _set Is Nothing Then Throw New InvalidOperationException("«DefaNoteA» nu a fost citit din Access.")
            If _set.Rows.Count = 0 Then
                k_progress.Report("«TZ_SablonOperatii»: «DefaNoteA» nu are rânduri, nimic de scris.")
                Return New List(Of AvcWritten) From {New AvcWritten("TZ_SablonOperatii", 0), New AvcWritten("TZ_SablonOperatiiLinie", 0)}
            End If
            If UnitsWithoutSubunit(k_context, k_cn, k_transaction).Count > 0 Then
                Throw New InvalidOperationException("Unele unități cu șabloane nu au subunitate: bifați pasul «Subunități».")
            End If
            ' The journals step wrote its journals inside this transaction: they are found now.
            Dim k_resolved As List(Of Resolved) = Resolve(k_context, k_cn, k_transaction)
            Dim k_noJournal As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count = 0 AndAlso Not k_r.IdJurnal.HasValue).ToList()
            If k_noJournal.Count > 0 Then
                k_progress.Report($"«TZ_SablonOperatii»: {k_noJournal.Count} șabloane au un jurnal care nu există în «TZ_Jurnale» (bifați pasul «Jurnale»); nu se scriu.")
            End If
            Dim k_toWrite As List(Of Resolved) = k_resolved.Where(Function(k_r) k_r.Problems.Count = 0 AndAlso k_r.IdJurnal.HasValue).
                GroupBy(Function(k_r) k_r.DedupKey, StringComparer.Ordinal).Select(Function(k_g) k_g.First()).
                OrderBy(Function(k_r) k_r.SursaSector, StringComparer.Ordinal).ThenBy(Function(k_r) k_r.Tip, StringComparer.Ordinal).
                ThenBy(Function(k_r) k_r.Cheie, StringComparer.Ordinal).ThenBy(Function(k_r) k_r.Row.Explicatie, StringComparer.OrdinalIgnoreCase).ToList()
            k_dump.WriteComment("TZ_SablonOperatii", "Fostele «DefaNoteA» + «DefaNote»: tip A sau M (C și M din Access devin M), jurnal al subunității unității, cheia clasificației; conturile sunt cele de bază cu analitic, iar fiecare linie are clasificația ei (IdClsf).")

            Dim k_headerSql = $"INSERT INTO {AvcSql.Quote("TZ_SablonOperatii")} ({String.Join(", ", HeaderColumns.Select(AddressOf AvcSql.Quote))}) " &
                              $"VALUES ({String.Join(", ", HeaderColumns.Select(Function(c) "@" & c))})"
            Dim k_lineSql = $"INSERT INTO {AvcSql.Quote("TZ_SablonOperatiiLinie")} ({String.Join(", ", LineColumns.Select(AddressOf AvcSql.Quote))}) " &
                            $"VALUES ({String.Join(", ", LineColumns.Select(Function(c) "@" & c))})"
            Dim k_written As Integer = 0
            Dim k_skipped As Integer = 0
            Dim k_lines As Integer = 0
            Using k_exists As New MySqlCommand(
                "SELECT COUNT(*) FROM `TZ_SablonOperatii` WHERE `IdJurnal` = @IdJurnal AND `Tip` = @Tip AND `Fel` = @Fel AND `Explicatie` = @Explicatie AND COALESCE(`Cheie`, '') = @Cheie", k_cn, k_transaction)
                k_exists.Parameters.Add("@IdJurnal", MySqlDbType.Int64)
                k_exists.Parameters.Add("@Tip", MySqlDbType.VarChar)
                k_exists.Parameters.Add("@Fel", MySqlDbType.VarChar)
                k_exists.Parameters.Add("@Explicatie", MySqlDbType.VarChar)
                k_exists.Parameters.Add("@Cheie", MySqlDbType.VarChar)
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
                            k_exists.Parameters("@IdJurnal").Value = k_item.IdJurnal.Value
                            k_exists.Parameters("@Tip").Value = k_item.Tip
                            k_exists.Parameters("@Fel").Value = k_item.Fel
                            k_exists.Parameters("@Explicatie").Value = k_item.Row.Explicatie
                            k_exists.Parameters("@Cheie").Value = k_item.Cheie
                            If Convert.ToInt64(k_exists.ExecuteScalar(), CultureInfo.InvariantCulture) > 0 Then
                                k_skipped += 1
                                Continue For
                            End If
                            Dim k_header As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                                {"IdJurnal", k_item.IdJurnal.Value}, {"Tip", k_item.Tip}, {"Fel", k_item.Fel}, {"Explicatie", k_item.Row.Explicatie},
                                {"Compartiment", If(k_item.Row.Compartiment.Length = 0, DBNull.Value, CObj(k_item.Row.Compartiment))},
                                {"FelOP", If(k_item.Fel <> "P" OrElse k_item.Row.FelOP.Length = 0, DBNull.Value, CObj(k_item.Row.FelOP))},
                                {"Cheie", If(k_item.Cheie.Length = 0, DBNull.Value, CObj(k_item.Cheie))}}
                            For Each k_column In HeaderColumns
                                k_headerCmd.Parameters("@" & k_column).Value = k_header(k_column)
                            Next
                            k_dump.WriteStatement("TZ_SablonOperatii", AvcSql.RenderStatement(k_headerSql, k_header))
                            k_headerCmd.ExecuteNonQuery()
                            Dim k_idSablon As Long = k_headerCmd.LastInsertedId
                            For Each k_line In k_item.Linii
                                Dim k_values As New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {
                                    {"IdSablon", k_idSablon},
                                    {"Forma", If(k_line.Forma.Length = 0, DBNull.Value, CObj(k_line.Forma))},
                                    {"IdClsf", If(k_line.IdClsf.HasValue, CObj(k_line.IdClsf.Value), DBNull.Value)},
                                    {"ContDebit", k_line.ContDebit}, {"ContCredit", k_line.ContCredit},
                                    {"AnaliticDebit", If(k_line.AnaliticDebit.Length = 0, DBNull.Value, CObj(k_line.AnaliticDebit))},
                                    {"AnaliticCredit", If(k_line.AnaliticCredit.Length = 0, DBNull.Value, CObj(k_line.AnaliticCredit))},
                                    {"PartDebitMod", k_line.PartDebitMod}, {"IdPartDebit", If(k_line.IdPartDebit.HasValue, CObj(k_line.IdPartDebit.Value), DBNull.Value)},
                                    {"PartCreditMod", k_line.PartCreditMod}, {"IdPartCredit", If(k_line.IdPartCredit.HasValue, CObj(k_line.IdPartCredit.Value), DBNull.Value)},
                                    {"PA", If(k_line.PA, 1, 0)}, {"Ordonantare", If(k_line.Ordonantare, 1, 0)}, {"Chelt", If(k_line.Chelt, 1, 0)}}
                                For Each k_column In LineColumns
                                    k_lineCmd.Parameters("@" & k_column).Value = k_values(k_column)
                                Next
                                k_dump.WriteStatement("TZ_SablonOperatiiLinie", AvcSql.RenderStatement(k_lineSql, k_values))
                                k_lineCmd.ExecuteNonQuery()
                                k_lines += 1
                            Next
                            k_written += 1
                        Next
                    End Using
                End Using
            End Using
            k_dump.FlushAll()
            If k_skipped > 0 Then k_progress.Report($"«TZ_SablonOperatii»: {k_skipped} șabloane existau deja și au rămas neschimbate.")
            k_progress.Report($"«TZ_SablonOperatii»: {k_written} șabloane, {k_lines} linii (confirmate la COMMIT).")
            Return New List(Of AvcWritten) From {New AvcWritten("TZ_SablonOperatii", k_written), New AvcWritten("TZ_SablonOperatiiLinie", k_lines)}
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("AvcSablonOperatiiStep.Write", ex)
            Throw
        End Try
    End Function

End Class
