Imports System.Globalization
Imports System.IO
Imports System.Text

Public NotInheritable Class VrcOiRepeat
    Public Property Key As String
    Public Property File As String
    Public Property Product As String
    Public Property Unit As String
    Public Property Price As Decimal
    Public Property Holder As String
    Public Property Quantity As Decimal
    Public Property PreviousFile As String
End Class

' One continuous OI exercise, assembled only from the accounts explicitly selected by the operator.
Public NotInheritable Class VrcOiBuilder
    Private ReadOnly _result As New VrcOiConversion()
    Private ReadOnly _report As New StringBuilder()
    Private ReadOnly _ids As New Dictionary(Of String, Long)(StringComparer.Ordinal)
    Private ReadOnly _maps As New Dictionary(Of String, Long)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _counters As New Dictionary(Of String, Long)(StringComparer.Ordinal)
    Private ReadOnly _tables As New Dictionary(Of String, List(Of Dictionary(Of String, Object)))(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _productAttributes As New Dictionary(Of Long, String)()
    Private ReadOnly _variantAttributes As New Dictionary(Of Long, String)()
    Private ReadOnly _holderOriginals As New Dictionary(Of String, (Name As String, File As String))(StringComparer.Ordinal)
    Private ReadOnly _holderAliases As Dictionary(Of String, String)
    Private _catalog As List(Of VrcOiData)
    Private ReadOnly _seenPredare As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private ReadOnly _seenReceipts As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private ReadOnly _accepted As HashSet(Of String)
    Private ReadOnly _notes As New List(Of String)()
    Public ReadOnly Repeats As New List(Of VrcOiRepeat)()

    Public Sub New(k_accepted As HashSet(Of String), Optional k_holders As IEnumerable(Of VrcOiHolderAssociation) = Nothing, Optional k_catalog As IEnumerable(Of VrcOiData) = Nothing)
        _accepted = k_accepted
        _catalog = k_catalog?.ToList()
        _holderAliases = If(k_holders, Enumerable.Empty(Of VrcOiHolderAssociation)()).GroupBy(Function(k_h) k_h.Identity, StringComparer.OrdinalIgnoreCase).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.Last().TargetName, StringComparer.OrdinalIgnoreCase)
    End Sub

    Public Function Build(k_plan As VrcPlan, k_choices As List(Of VrcOiChoice), k_name As String) As VrcOiConversion
        _result.BaseHash = k_plan.Hash()
        Try
            If k_choices.Count = 0 Then Throw New InvalidOperationException("Bifați conturile de separat din Materiale.")
            If _catalog Is Nothing Then _catalog = k_choices.Select(Function(k_c) k_c.Data).Distinct().ToList()
            If String.IsNullOrWhiteSpace(k_name) OrElse k_name.Trim().Length > 100 Then Throw New InvalidOperationException("Completați denumirea noii gestiuni OI (cel mult 100 de caractere).")
            If Not k_plan.IsComplete Then Throw New InvalidOperationException("Completați mai întâi planul de migrare: " & String.Join("; ", k_plan.Blockers))
            ' One builder is used for one subunit; the workspace combines independent results for several subunits.
            If k_choices.Select(Function(k_c) Normal(k_c.Data.Source.SubunitName)).Distinct().Count() <> 1 Then Throw New InvalidOperationException("Selectați conturi din aceeași subunitate pentru o separare OI.")
            Dim k_unit = k_choices(0).Data.Source.SubunitName.Trim()
            If k_plan.Sources.Any(Function(k_s) Normal(k_s.SubunitName) = Normal(k_unit) AndAlso Normal(k_s.GestiuneName) = Normal(k_name)) Then Throw New InvalidOperationException("Denumirea aleasă există deja în plan. Separarea creează o gestiune OI nouă.")
            For Each k_table In VrcImportMap.Tables()
                _tables(k_table.Name) = New List(Of Dictionary(Of String, Object))()
            Next
            _tables("Furnizori") = New List(Of Dictionary(Of String, Object))()
            CheckSelection(k_plan, k_choices)
            For Each k_data In k_choices.Select(Function(k_c) k_c.Data).Distinct().OrderBy(Function(k_d) k_d.Source.Year).ThenBy(Function(k_d) k_d.Source.Source.Path, StringComparer.OrdinalIgnoreCase)
                Collect(Sub() PrepareNames(k_data, k_choices.Where(Function(k_c) k_c.Data Is k_data).ToList()), k_data.Source.Profile.Label)
            Next
            For Each k_group In k_choices.GroupBy(Function(k_c) Lineage(k_c.Data.Source) & "|" & k_c.AccountKey)
                Collect(Sub() ConvertAccount(k_group.OrderBy(Function(k_c) k_c.Data.Source.Year).ToList()), k_group.First().AccountName)
            Next
            Dim k_first = k_choices.OrderBy(Function(k_c) k_c.Data.Source.Year).First().Data.Source
            Dim k_config As New SortedDictionary(Of String, String)(k_first.Profile.Config, StringComparer.OrdinalIgnoreCase)
            k_config("Setari.OBINV") = "1"
            k_config("Setari.MF") = "0"
            Dim k_year = k_choices.Min(Function(k_c) k_c.Data.Source.Year)
            For Each k_row In _tables("NIR")
                k_year = Math.Min(k_year, CDate(k_row("Data")).Year)
            Next
            Dim k_hash = VrcOiData.Fingerprint(_tables)
            Dim k_profile As New VrcFileProfile With {
                .FileName = "OI-din-Materiale", .FullPath = k_first.Source.Path, .Kind = VrcFileKind.Special,
                .HashBefore = k_hash, .HashAfter = k_hash, .Stable = True, .Config = k_config,
                .Tables = VrcImportMap.Tables().Select(Function(k_t) New VrcTableProfile With {.Name = k_t.Name, .Rows = _tables(k_t.Name).Count}).ToList()}
            _result.Generated.Add(New VrcPlanSource With {
                .Source = New VrcSourceFile With {.Path = k_first.Source.Path, .Kind = VrcFileKind.Special}, .Profile = k_profile,
                .GestiuneName = k_name.Trim(), .GestiuneType = "OI", .SubunitName = k_unit, .Year = k_year,
                .PreparedRows = _tables, .PreparedHash = k_hash, .GeneratedOi = True, .IsSeparatedSource = True, .PreparedNotes = _notes})
            _report.AppendLine($"Gestiune OI «{k_name.Trim()}», subunitate «{k_unit}», exercițiu multianual din {k_year}.")
            _report.AppendLine($"BP: {_tables("OI_Predare").Select(Function(k_r) Key(k_r, "IDBP")).Distinct().Count()}; linii BP/registru: {_tables("OI_Predare").Count}; NIR păstrate: {_tables("NIR").Count}.")
            _report.AppendLine("Obiectele predate rămân în folosință până la transfer/casare; lipsa din baza anului următor nu generează casări.")
            _report.AppendLine("Nu se reconstruiește folosința din anii anteriori primei baze selectate. Numele identic al produsului nu dovedește identitatea fizică.")
        Catch ex As InvalidOperationException
            _result.Blockers.Add(ex.Message)
        End Try
        _result.Report = _report.ToString()
        Return _result
    End Function

    Private Sub Collect(k_action As Action, k_context As String)
        Try
            k_action()
        Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is KeyNotFoundException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException OrElse TypeOf ex Is InvalidCastException
            _result.Blockers.Add(k_context & ": " & ex.Message)
        End Try
    End Sub

    Private Sub CheckSelection(k_plan As VrcPlan, k_choices As List(Of VrcOiChoice))
        For Each k_choice In k_choices
            If String.IsNullOrWhiteSpace(k_choice.UseAccountName) OrElse k_choice.UseAccountName.Trim().Length > 100 Then _result.Blockers.Add($"{k_choice.Data.Source.Profile.Label}, «{k_choice.AccountName}»: completați contul de folosință (cel mult 100 de caractere).")
            Dim k_lineage = Lineage(k_choice.Data.Source)
            For Each k_source In k_plan.Sources.Where(Function(k_s) k_s.GestiuneType = "MATERIALE" AndAlso Lineage(k_s) = k_lineage)
                If Not k_choices.Any(Function(k_c) k_c.Data.Source.Source.Path = k_source.Source.Path AndAlso k_c.AccountKey = k_choice.AccountKey) Then
                    ' The account may not yet exist in an earlier year. Its presence is checked by the workspace snapshot.
                    Dim k_provider As String = String.Empty
                    Using k_cn = VrcAccess.OpenReadOnly(k_source.Source.Path, k_provider)
                        If VrcUsageReader.AllRows(k_cn, k_source.Profile, "Masini").Any(Function(k_r) Key(k_r, "IdMasina") = k_choice.AccountKey) Then _result.Blockers.Add($"Contul «{k_choice.AccountName}» există și în {k_source.Profile.Label}. Bifați-l în toate bazele încărcate ale aceleiași gestiuni, pentru a evita separarea incompletă.")
                    End Using
                End If
            Next
        Next
    End Sub

    Private Shared Function Lineage(k_source As VrcPlanSource) As String
        Return Normal(k_source.SubunitName) & "|" & Normal(k_source.GestiuneName) & "|" & Normal(Path.GetDirectoryName(k_source.Source.Path))
    End Function

    Private Function Allocate(k_table As String, k_identity As String) As Long
        Dim k_key = k_table & "|" & k_identity
        Dim k_id As Long
        If _ids.TryGetValue(k_key, k_id) Then Return k_id
        _counters.TryGetValue(k_table, k_id)
        k_id += 1
        _counters(k_table) = k_id
        _ids(k_key) = k_id
        Return k_id
    End Function

    Private Shared Function MapKey(k_data As VrcOiData, k_table As String, k_key As String) As String
        Return k_data.Source.Source.Path & "|" & k_table & "|" & k_key
    End Function

    Private Function Mapped(k_data As VrcOiData, k_table As String, k_value As Object) As Object
        Dim k_key = VrcConvert.KeyText(k_value)
        Dim k_id As Long
        If k_table = "Clienti" AndAlso _maps.TryGetValue(MapKey(k_data, k_table, k_key), k_id) Then Return k_id
        If k_key.Length = 0 OrElse k_key = "0" Then Return Nothing
        If Not _maps.TryGetValue(MapKey(k_data, k_table, k_key), k_id) Then
            Dim k_parent = VrcImportMap.Find(k_table)
            Dim k_row = If(k_parent Is Nothing, Nothing, k_data.Rows(k_table).FirstOrDefault(Function(k_r) Key(k_r, k_parent.SourceKey) = k_key))
            Dim k_label = If(k_row Is Nothing, "(nume lipsă în baza sursă)", If(k_table = "Masini", Text(k_row, "Masina"), If(k_table = "Produse", Text(k_row, "DenumireP"), Text(k_row, "Denumire"))))
            Dim k_kind = If(k_table = "Clienti", "deținătorul", If(k_table = "Masini", "contul", If(k_table = "Produse" OrElse k_table = "ProduseP", "produsul", "referința " & k_table)))
            Throw New InvalidOperationException($"Nu poate fi identificat {k_kind} «{k_label}» în datele pregătite. Completați asocierea sau referința din sursă.")
        End If
        Return k_id
    End Function

    Private Sub AddNamed(k_data As VrcOiData, k_table As String, k_row As Dictionary(Of String, Object), k_identity As String)
        Dim k_spec = VrcImportMap.Find(k_table)
        Dim k_id = Allocate(k_table, k_identity)
        _maps(MapKey(k_data, k_table, Key(k_row, k_spec.SourceKey))) = k_id
        Dim k_copy = CloneRow(k_row)
        k_copy(k_spec.SourceKey) = k_id
        If k_table = "Produse" Then k_copy("IdMasina") = Mapped(k_data, "Masini", Value(k_row, "IdMasina"))
        If k_table = "ProduseP" Then k_copy("ID") = Mapped(k_data, "Produse", Value(k_row, "ID"))
        Dim k_existing = _tables(k_table).FindIndex(Function(k_r) CLng(k_r(k_spec.SourceKey)) = k_id)
        If k_existing < 0 Then
            _tables(k_table).Add(k_copy)
        Else
            _tables(k_table)(k_existing) = k_copy
        End If
    End Sub

    Private Sub PrepareNames(k_data As VrcOiData, k_choices As List(Of VrcOiChoice))
        Dim k_accounts = k_choices.Select(Function(k_c) k_c.AccountKey).ToHashSet(StringComparer.Ordinal)
        Dim k_products = k_data.Rows("Produse").Where(Function(k_r) k_accounts.Contains(Key(k_r, "IdMasina"))).ToList()
        Dim k_productIds = k_products.Select(Function(k_r) Key(k_r, "ID")).ToHashSet(StringComparer.Ordinal)
        For Each k_choice In k_choices
            Dim k_account = k_data.Rows("Masini").Single(Function(k_r) Key(k_r, "IdMasina") = k_choice.AccountKey)
            AddNamed(k_data, "Masini", k_account, Lineage(k_data.Source) & "|" & k_choice.AccountKey)
            Dim k_useId = Allocate("Masini", "USE|" & Normal(k_choice.UseAccountName))
            If Not _tables("Masini").Any(Function(k_r) CLng(k_r("IdMasina")) = k_useId) Then _tables("Masini").Add(New Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase) From {{"IdMasina", k_useId}, {"Masina", k_choice.UseAccountName.Trim()}, {"TIP", "M"}})
        Next
        For Each k_row In k_products
            Collect(Sub()
            Dim k_identity = Lineage(k_data.Source) & "|" & Key(k_row, "ID")
            Dim k_id = Allocate("Produse", k_identity)
            Dim k_attributes = Normal(Text(k_row, "DenumireP")) & "|" & Normal(Text(k_row, "UM")) & "|" & Key(k_row, "IdMasina")
            Dim k_before As String = Nothing
            If _productAttributes.TryGetValue(k_id, k_before) AndAlso k_before <> k_attributes Then _result.Blockers.Add($"{k_data.Source.Profile.Label}: produsul «{Text(k_row, "DenumireP") }», UM «{Text(k_row, "UM")}», apare anterior ca «{k_before.Split("|"c)(0)}», UM «{k_before.Split("|"c)(1)}», ori și-a schimbat contul. Asocierea trebuie lămurită înaintea separării.")
            _productAttributes(k_id) = k_attributes
            AddNamed(k_data, "Produse", k_row, k_identity)
                    End Sub, Context(k_data, k_row))
        Next
        For Each k_row In k_data.Rows("ProduseP").Where(Function(k_r) k_productIds.Contains(Key(k_r, "ID")))
            Collect(Sub()
                        Dim k_identity = Lineage(k_data.Source) & "|" & Key(k_row, "IdProdus")
                        Dim k_id = Allocate("ProduseP", k_identity)
                        Dim k_attributes = Key(k_row, "ID") & "|" & Number(k_row, "PUTVA").ToString("G29", CultureInfo.InvariantCulture)
                        Dim k_before As String = Nothing
                        If _variantAttributes.TryGetValue(k_id, k_before) AndAlso k_before <> k_attributes Then _result.Blockers.Add(Context(k_data, k_row) & ": varianta avea alt produs sau alt preț în baza anterioară; nu se reunesc automat variante cu identitate schimbată.")
                        _variantAttributes(k_id) = k_attributes
                        AddNamed(k_data, "ProduseP", k_row, k_identity)
                    End Sub, Context(k_data, k_row))
        Next
        Dim k_variantIds = k_data.Rows("ProduseP").Where(Function(k_r) k_productIds.Contains(Key(k_r, "ID"))).Select(Function(k_r) Key(k_r, "IdProdus")).ToHashSet(StringComparer.Ordinal)
        Dim k_nirIds = k_data.Rows("Intrari").Where(Function(k_r) k_variantIds.Contains(Key(k_r, "IdProdus"))).Select(Function(k_r) Key(k_r, "IdIntrare")).ToHashSet(StringComparer.Ordinal)
        Dim k_bonIds = k_data.Rows("Iesiri").Where(Function(k_r) k_variantIds.Contains(Key(k_r, "IdProdus"))).Select(Function(k_r) Key(k_r, "IdBonConsum")).ToHashSet(StringComparer.Ordinal)
        Dim k_selectedNirs = k_data.Rows("NIR").Where(Function(k_r) k_nirIds.Contains(Key(k_r, "IdIntrare")) OrElse k_accounts.Contains(Key(k_r, "IdMasina"))).ToList()
        Dim k_selectedBons = k_data.Rows("Iesiri_H").Where(Function(k_r) k_bonIds.Contains(Key(k_r, "IdBonConsum")) OrElse k_accounts.Contains(Key(k_r, "IdMasina"))).ToList()
        PrepareHolders(k_data, k_selectedBons)
        Dim k_needed As New Dictionary(Of String, HashSet(Of String))(StringComparer.OrdinalIgnoreCase) From {
            {"ComisieNIR", k_selectedNirs.Select(Function(k_r) Key(k_r, "IdComisie")).ToHashSet(StringComparer.Ordinal)},
            {"TVA", k_data.Rows("Intrari").Where(Function(k_r) k_nirIds.Contains(Key(k_r, "IdIntrare"))).Select(Function(k_r) Key(k_r, "IDTVA")).ToHashSet(StringComparer.Ordinal)}}
        For Each k_table In {"TVA", "ComisieNIR"}
            For Each k_row In k_data.Rows(k_table).Where(Function(k_r) k_needed(k_table).Contains(Key(k_r, VrcImportMap.Find(k_table).SourceKey)))
                Dim k_key = Key(k_row, VrcImportMap.Find(k_table).SourceKey)
                If k_key = "0" Then Continue For
                Dim k_identity = k_data.Source.Source.Path & "|" & k_key
                AddNamed(k_data, k_table, k_row, k_identity)
            Next
        Next
        Dim k_suppliers = k_selectedNirs.Select(Function(k_r) Key(k_r, "IDF")).ToHashSet(StringComparer.Ordinal)
        For Each k_row In k_data.Rows("Furnizori").Where(Function(k_r) k_suppliers.Contains(Key(k_r, "IDF")))
            Dim k_id = Allocate("Furnizori", MapKey(k_data, "Furnizori", Key(k_row, "IDF")))
            _maps(MapKey(k_data, "Furnizori", Key(k_row, "IDF"))) = k_id
            Dim k_copy = CloneRow(k_row)
            k_copy("IDF") = k_id
            _tables("Furnizori").Add(k_copy)
        Next
    End Sub

    Private Sub PrepareHolders(k_data As VrcOiData, k_bons As List(Of Dictionary(Of String, Object)))
        For Each k_group In k_bons.GroupBy(Function(k_r) Key(k_r, "IDC"))
            Dim k_key = k_group.Key
            Dim k_sourceName = VrcOiHolders.SourceName(k_data, k_key)
            Dim k_targetName As String = Nothing
            If Not _holderAliases.TryGetValue(k_data.Source.Source.Path & "|" & k_key, k_targetName) Then k_targetName = VrcOiHolders.DefaultTarget(k_data, k_key)
            If String.IsNullOrWhiteSpace(k_targetName) Then
                For Each k_bon In k_group
                    _result.Blockers.Add($"{Context(k_data, k_bon)}: «{k_sourceName}». Alegeți deținătorul în grila de asocieri.")
                Next
                Continue For
            End If
            Collect(Sub()
                        Dim k_copy = VrcOiHolders.Resolve(k_data, k_key, k_targetName, _catalog)
                        If Not VrcOiHolders.IsMissing(k_data, k_key) Then
                            Dim k_identity = Lineage(k_data.Source) & "|" & k_key
                            Dim k_before As (Name As String, File As String)
                            Dim k_changedName = _holderOriginals.TryGetValue(k_identity, k_before) AndAlso Normal(k_before.Name) <> Normal(k_sourceName)
                            _holderOriginals(k_identity) = (k_sourceName, k_data.Source.Profile.Label)
                            If k_changedName Then
                                Dim k_previousTarget As String = Nothing
                                If Not _holderAliases.TryGetValue(k_before.File & "|" & k_key, k_previousTarget) Then k_previousTarget = k_before.Name
                                _report.AppendLine($"NUME DIFERITE ÎN SURSE: «{k_before.Name}» în {k_before.File} / «{k_sourceName}» în {k_data.Source.Profile.Label}. Destinație: «{k_previousTarget}» / «{k_targetName}» — {If(Normal(k_previousTarget) = Normal(k_targetName), "asociate în același deținător", "păstrate ca deținători distincți; se pot asocia în grilă")}.")
                            End If
                        End If
                        AddNamed(k_data, "Clienti", k_copy, "HOLDER|" & Normal(k_targetName))
                        _notes.Add($"Deținător: «{k_sourceName}» din {k_data.Source.Profile.Label} → «{k_targetName.Trim()}»; bonuri: {String.Join("; ", k_group.Select(Function(k_b) VrcOiHolders.DocumentLabel(k_b)))}.")
                    End Sub, k_data.Source.Profile.Label & ", deținător «" & k_sourceName & "», " & String.Join("; ", k_group.Select(Function(k_b) VrcOiHolders.DocumentLabel(k_b))))
        Next
    End Sub

    Private Sub ConvertAccount(k_choices As List(Of VrcOiChoice))
        Dim k_previous As Dictionary(Of String, Decimal) = Nothing
        Dim k_previousYear As Integer = 0
        Dim k_useName = Normal(k_choices(0).UseAccountName)
        Dim k_accountName = Normal(k_choices(0).AccountName)
        For Each k_choice In k_choices
            Dim k_data = k_choice.Data
            Try
            Dim k_problemsBeforeYear = _result.Blockers.Count
            If Normal(k_choice.UseAccountName) <> k_useName OrElse Normal(k_choice.AccountName) <> k_accountName Then _result.Blockers.Add($"{k_data.Source.Profile.Label}, «{k_choice.AccountName}»: denumirea sau contul de folosință diferă de «{k_choices(0).AccountName}» / «{k_choices(0).UseAccountName}» din {k_choices(0).Data.Source.Profile.Label}.")
            If k_previous IsNot Nothing AndAlso k_data.Source.Year <> k_previousYear + 1 Then
                _result.Blockers.Add($"Lipsește o bază anuală între {k_previousYear} și {k_data.Source.Year} pentru «{k_choice.AccountName}».")
                k_previous = Nothing
            End If
            Dim k_products = k_data.Rows("Produse").Where(Function(k_r) Key(k_r, "IdMasina") = k_choice.AccountKey).Select(Function(k_r) Key(k_r, "ID")).ToHashSet(StringComparer.Ordinal)
            Dim k_variants = k_data.Rows("ProduseP").Where(Function(k_r) k_products.Contains(Key(k_r, "ID"))).Select(Function(k_r) Key(k_r, "IdProdus")).ToHashSet(StringComparer.Ordinal)
            Dim k_nirs = SelectDocuments(k_data, "NIR", "Intrari", "IdIntrare", k_choice.AccountKey, k_variants)
            Dim k_bons = SelectDocuments(k_data, "Iesiri_H", "Iesiri", "IdBonConsum", k_choice.AccountKey, k_variants)
            For Each k_table In {"IntrariStorno", "IesiriRetur", "OI_Predare", "OI_Transfer", "OI_Casare", "OI_Produse"}
                For Each k_row In k_data.Rows(k_table).Where(Function(k_r) k_variants.Contains(Key(k_r, "IdProdus")) OrElse (k_table = "IntrariStorno" AndAlso k_nirs.Contains(Key(k_r, "IdIntrare"))))
                    If k_table = "IntrariStorno" AndAlso Not k_data.Rows("NIR").Any(Function(k_r) Key(k_r, "IdIntrare") = Key(k_row, "IdIntrare") AndAlso VrcConvert.ToBit(Value(k_r, "Storno")).GetValueOrDefault() <> 0) Then Continue For
                    _result.Blockers.Add($"{Context(k_data, k_row)}, cont «{k_choice.AccountName}»: există o corecție/operație {k_table}. Aceasta cere o regulă explicită înainte de conversia la OI.")
                Next
            Next
            For Each k_row In k_data.Rows("LegBCLA").Where(Function(k_r) k_bons.Contains(Key(k_r, "IdBonConsum")))
                _result.Blockers.Add(Context(k_data, k_row) & ": bonul este legat de o listă de alimente; separarea necesită lămurirea legăturii.")
            Next
            For Each k_table In {"Intrari", "Iesiri"}
                For Each k_row In k_data.Rows(k_table).Where(Function(k_r) k_variants.Contains(Key(k_r, "IdProdus")))
                    Collect(Sub() Number(k_row, "Cant"), Context(k_data, k_row))
                    Collect(Sub() Number(k_row, "PUTVA"), Context(k_data, k_row))
                    Dim k_q = VrcConvert.ToDecimal(Value(k_row, "Cant"))
                    Dim k_p = VrcConvert.ToDecimal(Value(k_row, "PUTVA"))
                    If k_q.HasValue AndAlso k_p.HasValue AndAlso (k_q.Value <= 0D OrElse k_p.Value < 0D) Then _result.Blockers.Add(Context(k_data, k_row) & ": cantitate nepozitivă sau preț negativ; este necesară o regulă explicită pentru corecție.")
                Next
            Next
            Dim k_balanceComplete = _result.Blockers.Count = k_problemsBeforeYear
            Dim k_opening = k_data.Rows("NIR").Where(Function(k_r) k_nirs.Contains(Key(k_r, "IdIntrare")) AndAlso VrcConvert.IsOpeningStock(Text(k_r, "NrFact"))).Select(Function(k_r) Key(k_r, "IdIntrare")).ToHashSet(StringComparer.Ordinal)
            Dim k_initial As New Dictionary(Of String, Decimal)(StringComparer.Ordinal)
            Dim k_balance As New Dictionary(Of String, Decimal)(StringComparer.Ordinal)
            Dim k_events As New List(Of (Day As Date, Direction As Integer, Row As Dictionary(Of String, Object)))()
            For Each k_row In k_data.Rows("Intrari").Where(Function(k_r) k_nirs.Contains(Key(k_r, "IdIntrare")))
                Collect(Sub()
                Dim k_header = k_data.Rows("NIR").Single(Function(k_r) Key(k_r, "IdIntrare") = Key(k_row, "IdIntrare"))
                If k_opening.Contains(Key(k_row, "IdIntrare")) Then AddQuantity(k_initial, StockKey(k_data, k_row), Number(k_row, "Cant"))
                k_events.Add((DocumentDay(k_header, "Data"), 1, k_row))
                        End Sub, Context(k_data, k_row))
            Next
            For Each k_row In k_data.Rows("Iesiri").Where(Function(k_r) k_bons.Contains(Key(k_r, "IdBonConsum")))
                Collect(Sub()
                Dim k_header = k_data.Rows("Iesiri_H").Single(Function(k_r) Key(k_r, "IdBonConsum") = Key(k_row, "IdBonConsum"))
                k_events.Add((DocumentDay(k_header, "DataBC"), -1, k_row))
                        End Sub, Context(k_data, k_row))
            Next
            k_balanceComplete = k_balanceComplete AndAlso _result.Blockers.Count = k_problemsBeforeYear
            If k_previous IsNot Nothing AndAlso k_balanceComplete Then
                For Each k_key In k_previous.Keys.Union(k_initial.Keys)
                    Dim k_before As Decimal = 0D, k_after As Decimal = 0D
                    k_previous.TryGetValue(k_key, k_before)
                    k_initial.TryGetValue(k_key, k_after)
                    If k_before <> k_after Then _result.Blockers.Add($"{k_data.Source.Profile.Label}, «{k_choice.AccountName}», {StockLabel(k_key)}: sold {k_previousYear} = {k_before}, deschidere = {k_after}. Nu se inventează intrări/ieșiri pentru diferență.")
                Next
            End If
            For Each k_event In k_events.OrderBy(Function(k_e) k_e.Day).ThenByDescending(Function(k_e) k_e.Direction)
                Collect(Sub()
                Dim k_quantity = Number(k_event.Row, "Cant")
                If k_quantity <= 0D OrElse Number(k_event.Row, "PUTVA") < 0D Then Throw New InvalidOperationException("Cantitatea este nepozitivă sau prețul negativ. Este necesară o regulă explicită pentru corecție.")
                Dim k_key = StockKey(k_data, k_event.Row)
                AddQuantity(k_balance, k_key, k_quantity * k_event.Direction)
                If k_balanceComplete AndAlso k_balance(k_key) < 0D Then _result.Blockers.Add($"{k_data.Source.Profile.Label}, {k_event.Day:d}, {StockLabel(k_key)}: magazie negativă ({k_balance(k_key)}).")
                        End Sub, Context(k_data, k_event.Row))
            Next
            Dim k_keptNirs = k_nirs.Where(Function(k_id) k_previous Is Nothing OrElse Not k_opening.Contains(k_id)).ToHashSet(StringComparer.Ordinal)
            CopyReceipts(k_data, k_keptNirs)
            CopyHandovers(k_choice, k_bons)
            RemoveFromMaterials(k_data, k_choice.AccountKey, k_products, k_variants, k_nirs, k_bons)
            Dim k_line = $"{k_data.Source.Profile.Label} | {k_choice.AccountName}: {k_keptNirs.Count} NIR păstrate; {k_nirs.Count - k_keptNirs.Count} reporturi eliminate după reconciliere; {k_bons.Count} BC → BP + BC legat."
            _report.AppendLine(k_line)
            _notes.Add(k_line)
            If k_balanceComplete Then
                For Each k_stock In k_balance.Where(Function(k_p) k_p.Value <> 0D)
                    _report.AppendLine($"  Magazie la sfârșitul bazei: {StockLabel(k_stock.Key)} = {k_stock.Value}.")
                Next
            Else
                _report.AppendLine("  Soldul și reconcilierea acestui an nu sunt concluzive cât timp există documente/linii invalide; analiza celorlalte baze continuă.")
            End If
            k_previous = If(k_balanceComplete, k_balance, Nothing)
            k_previousYear = k_data.Source.Year
            Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is KeyNotFoundException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException OrElse TypeOf ex Is InvalidCastException
                _result.Blockers.Add($"{k_data.Source.Profile.Label}, «{k_choice.AccountName}»: {ex.Message}")
                k_previous = Nothing
            End Try
        Next
    End Sub

    Private Function SelectDocuments(k_data As VrcOiData, k_header As String, k_lines As String, k_key As String, k_account As String, k_variants As HashSet(Of String)) As HashSet(Of String)
        Dim k_result As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_row In k_data.Rows(k_header)
            Dim k_id = Key(k_row, k_key)
            Dim k_items = k_data.Rows(k_lines).Where(Function(k_r) Key(k_r, k_key) = k_id).ToList()
            If Key(k_row, "IdMasina") <> k_account AndAlso Not k_items.Any(Function(k_r) k_variants.Contains(Key(k_r, "IdProdus"))) Then Continue For
            Dim k_before = _result.Blockers.Count
            If k_items.Any(Function(k_r) Not k_variants.Contains(Key(k_r, "IdProdus"))) Then _result.Blockers.Add(Context(k_data, k_row) & ": documentul conține produse din alte conturi sau neidentificabile. Separarea parțială cere o decizie explicită.")
            Dim k_headerAccount = Key(k_row, "IdMasina")
            If k_headerAccount.Length > 0 AndAlso k_headerAccount <> "0" AndAlso k_headerAccount <> k_account Then _result.Blockers.Add(Context(k_data, k_row) & ": contul antetului diferă de contul produselor selectate.")
            If VrcConvert.ToBit(Value(k_row, "Anulat")).GetValueOrDefault() <> 0 OrElse
               VrcConvert.ToBit(Value(k_row, If(k_header = "NIR", "Storno", "Retur"))).GetValueOrDefault() <> 0 Then _result.Blockers.Add(Context(k_data, k_row) & ": document anulat, storno sau retur; este necesară o regulă explicită de conversie.")
            If Value(k_row, If(k_header = "NIR", "Data", "DataBC")) Is Nothing Then _result.Blockers.Add(Context(k_data, k_row) & ": data documentului lipsește.")
            If Value(k_row, If(k_header = "NIR", "Data", "DataBC")) IsNot Nothing Then Collect(Sub() DocumentDay(k_row, If(k_header = "NIR", "Data", "DataBC")), Context(k_data, k_row))
            If k_header = "Iesiri_H" AndAlso k_items.Count = 0 Then _result.Blockers.Add(Context(k_data, k_row) & ": bonul nu are produse.")
            If k_header = "Iesiri_H" AndAlso DocumentNumber(Text(k_row, "NumarBC")) <= 0D Then _result.Blockers.Add(Context(k_data, k_row) & ": numărul bonului lipsește sau nu poate fi interpretat.")
            If k_header = "NIR" AndAlso Not VrcConvert.ToDecimal(Value(k_row, "Numar")).HasValue Then _result.Blockers.Add(Context(k_data, k_row) & ": numărul NIR-ului lipsește sau nu este numeric.")
            If k_before = _result.Blockers.Count Then k_result.Add(k_id)
        Next
        Dim k_headers = k_data.Rows(k_header).Select(Function(k_r) Key(k_r, k_key)).ToHashSet(StringComparer.Ordinal)
        For Each k_orphan In k_data.Rows(k_lines).Where(Function(k_r) k_variants.Contains(Key(k_r, "IdProdus")) AndAlso Not k_headers.Contains(Key(k_r, k_key)))
            _result.Blockers.Add(Context(k_data, k_orphan) & ": lipsește documentul părinte (antetul).")
        Next
        Return k_result
    End Function

    Private Shared Function DocumentDay(k_row As Dictionary(Of String, Object), k_field As String) As Date
        If Value(k_row, k_field) Is Nothing Then Throw New InvalidOperationException("Un document selectat nu are dată.")
        Return Convert.ToDateTime(k_row(k_field), CultureInfo.InvariantCulture).Date
    End Function

    Private Function StockKey(k_data As VrcOiData, k_row As Dictionary(Of String, Object)) As String
        Return Convert.ToString(Mapped(k_data, "ProduseP", Value(k_row, "IdProdus")), CultureInfo.InvariantCulture) & "|" & Number(k_row, "PUTVA").ToString("G29", CultureInfo.InvariantCulture)
    End Function

    Private Function StockLabel(k_key As String) As String
        Dim k_id = Long.Parse(k_key.Split("|"c)(0), CultureInfo.InvariantCulture)
        Dim k_product = ProductForVariant(k_id)
        Return Text(k_product, "DenumireP") & " [" & Text(k_product, "UM") & "; preț " & k_key.Split("|"c)(1) & "]"
    End Function

    Private Function ProductForVariant(k_id As Long) As Dictionary(Of String, Object)
        Dim k_variant = _tables("ProduseP").Single(Function(k_r) CLng(k_r("IdProdus")) = k_id)
        Return _tables("Produse").Single(Function(k_r) CLng(k_r("ID")) = CLng(k_variant("ID")))
    End Function

    Private Shared Sub AddQuantity(k_balance As Dictionary(Of String, Decimal), k_key As String, k_delta As Decimal)
        Dim k_before As Decimal = 0D
        k_balance.TryGetValue(k_key, k_before)
        k_balance(k_key) = k_before + k_delta
    End Sub

    Private Sub CopyReceipts(k_data As VrcOiData, k_nirs As HashSet(Of String))
        For Each k_header In k_data.Rows("NIR").Where(Function(k_r) k_nirs.Contains(Key(k_r, "IdIntrare"))).OrderBy(Function(k_r) DocumentDay(k_r, "Data")).ThenBy(Function(k_r) VrcConvert.ToDecimal(Value(k_r, "Numar")).GetValueOrDefault())
            Collect(Sub()
            Dim k_id = Allocate("NIR", MapKey(k_data, "NIR", Key(k_header, "IdIntrare")))
            _maps(MapKey(k_data, "NIR", Key(k_header, "IdIntrare"))) = k_id
            Dim k_copy = CloneRow(k_header)
            k_copy("IdIntrare") = k_id
            k_copy("Numar") = If(VrcConvert.IsOpeningStock(Text(k_header, "NrFact")), -k_id, k_id)
            For Each k_field In {"IdMasina", "IdComisie", "IDF"}
                k_copy(k_field) = Mapped(k_data, If(k_field = "IdMasina", "Masini", If(k_field = "IDF", "Furnizori", "ComisieNIR")), Value(k_header, k_field))
            Next
            k_copy("IdStorno") = Nothing
            _tables("NIR").Add(k_copy)
            _notes.Add($"{k_data.Source.Profile.Label}: NIR/{Key(k_header, "IdIntrare")}, număr {Text(k_header, "Numar")} → NIR/{k_id}, număr {k_copy("Numar")}.")
            For Each k_row In k_data.Rows("Intrari").Where(Function(k_r) Key(k_r, "IdIntrare") = Key(k_header, "IdIntrare"))
                Collect(Sub()
                If Not VrcConvert.IsOpeningStock(Text(k_header, "NrFact")) Then
                    Dim k_supplier = k_data.Rows("Furnizori").FirstOrDefault(Function(k_r) Key(k_r, "IDF") = Key(k_header, "IDF"))
                    Dim k_signature = Normal(Text(k_header, "NrFact")) & "|" & Text(k_header, "DataFc") & "|" & If(k_supplier Is Nothing, "?", VrcConvert.NormalizeTaxCode(Text(k_supplier, "CF"))) & "|" & Normal(StockLabel(StockKey(k_data, k_row))) & "|" & Number(k_row, "Cant").ToString("G29", CultureInfo.InvariantCulture)
                    Dim k_prior As String = Nothing
                    If _seenReceipts.TryGetValue(k_signature, k_prior) AndAlso k_prior <> k_data.Source.Profile.Label Then _result.Blockers.Add($"Posibil NIR preluat de două ori: {k_prior} / {k_data.Source.Profile.Label}, factura {Text(k_header, "NrFact")}, {StockLabel(StockKey(k_data, k_row))}. Lămuriți documentul înainte de generare.")
                    _seenReceipts(k_signature) = k_data.Source.Profile.Label
                End If
                Dim k_line = CloneRow(k_row)
                k_line("ID") = Allocate("Intrari", MapKey(k_data, "Intrari", Key(k_row, "ID")))
                k_line("IdIntrare") = k_id
                k_line("IdProdus") = Mapped(k_data, "ProduseP", Value(k_row, "IdProdus"))
                k_line("IDTVA") = Mapped(k_data, "TVA", Value(k_row, "IDTVA"))
                _tables("Intrari").Add(k_line)
                        End Sub, Context(k_data, k_row))
            Next
                    End Sub, Context(k_data, k_header))
        Next
    End Sub

    Private Sub CopyHandovers(k_choice As VrcOiChoice, k_bons As HashSet(Of String))
        Dim k_data = k_choice.Data
        Dim k_useId = Allocate("Masini", "USE|" & Normal(k_choice.UseAccountName))
        For Each k_header In k_data.Rows("Iesiri_H").Where(Function(k_r) k_bons.Contains(Key(k_r, "IdBonConsum"))).OrderBy(Function(k_r) DocumentDay(k_r, "DataBC")).ThenBy(Function(k_r) DocumentNumber(Text(k_r, "NumarBC")))
            Collect(Sub()
            Dim k_oldId = Key(k_header, "IdBonConsum")
            Dim k_lines = k_data.Rows("Iesiri").Where(Function(k_r) Key(k_r, "IdBonConsum") = k_oldId).ToList()
            If k_lines.Count = 0 Then Throw New InvalidOperationException("Bonul nu are produse; nu se generează un BP gol.")
            Dim k_holder = Mapped(k_data, "Clienti", Value(k_header, "IDC"))
            If k_holder Is Nothing Then Throw New InvalidOperationException("Bonul nu are un deținător identificabil. Verificați primitorul în baza sursă.")
            Dim k_id = Allocate("Iesiri_H", MapKey(k_data, "Iesiri_H", k_oldId))
            Dim k_copy = CloneRow(k_header)
            k_copy("IdBonConsum") = k_id
            k_copy("IDC") = k_holder
            k_copy("IdMasina") = Mapped(k_data, "Masini", k_choice.AccountKey)
            k_copy("NumarBC") = "BP " & k_id.ToString(CultureInfo.InvariantCulture)
            k_copy("IdRetur") = Nothing
            k_copy("Expl") = Value(k_header, "Expl")
            _tables("Iesiri_H").Add(k_copy)
            _notes.Add($"{k_data.Source.Profile.Label}: BC/{k_oldId} ({Text(k_header, "NumarBC")}) → BP {k_id} + BC legat, aceeași cantitate și valoare.")
            For Each k_row In k_lines
                Collect(Sub()
                Dim k_variant = Mapped(k_data, "ProduseP", Value(k_row, "IdProdus"))
                Dim k_repeatKey = MapKey(k_data, "Iesiri", Key(k_row, "ID"))
                Dim k_holderName = Text(_tables("Clienti").Single(Function(k_r) CLng(k_r("IDC")) = CLng(k_holder)), "Denumire")
                Dim k_seenKey = Normal(StockLabel(StockKey(k_data, k_row))) & "|" & Normal(k_holderName) & "|" & Normal(k_choice.UseAccountName)
                Dim k_prior As String = Nothing
                If _seenPredare.TryGetValue(k_seenKey, k_prior) AndAlso k_prior <> k_data.Source.Profile.Label Then
                    Dim k_product = ProductForVariant(CLng(k_variant))
                    Repeats.Add(New VrcOiRepeat With {.Key = k_repeatKey, .File = k_data.Source.Profile.Label, .PreviousFile = k_prior,
                        .Product = Text(k_product, "DenumireP"), .Unit = Text(k_product, "UM"), .Price = Number(k_row, "PUTVA"), .Quantity = Number(k_row, "Cant"),
                        .Holder = k_holderName})
                    If Not _accepted.Contains(k_repeatKey) Then _result.Blockers.Add($"Posibilă predare repetată: {Context(k_data, k_row)}, către «{k_holderName}»; predare anterioară în {k_prior}. Confirmați individual numai dacă este o predare nouă; dublurile reale necesită lămurire.")
                    If _accepted.Contains(k_repeatKey) Then _notes.Add($"Predare distinctă confirmată individual: {k_repeatKey}; suprapunere semnalată cu {k_prior}.")
                End If
                _seenPredare(k_seenKey) = k_data.Source.Profile.Label
                Dim k_line = CloneRow(k_row)
                k_line("ID") = Allocate("Iesiri", k_repeatKey)
                k_line("IdBonConsum") = k_id
                k_line("IdProdus") = k_variant
                _tables("Iesiri").Add(k_line)
                Dim k_bp = CloneRow(k_line)
                k_bp("ID") = Allocate("OI_Predare", k_repeatKey)
                k_bp("IDBP") = k_id
                k_bp("NRBP") = k_copy("NumarBC")
                k_bp("DATABP") = k_header("DataBC")
                k_bp("IDC_BP") = k_holder
                k_bp("IdMasina") = k_useId
                k_bp("IdMasinaP") = k_copy("IdMasina")
                k_bp("Expl") = k_copy("Expl")
                _tables("OI_Predare").Add(k_bp)
                Dim k_registry = CloneRow(k_line)
                k_registry("IDOI") = Allocate("OI_Produse", k_repeatKey)
                k_registry("IDBP") = k_id
                k_registry("IDBT") = Nothing
                k_registry("IDBC") = k_id
                k_registry("IDBO") = Nothing
                k_registry("IDM") = k_useId
                k_registry("IDC_PTC") = k_holder
                _tables("OI_Produse").Add(k_registry)
                        End Sub, Context(k_data, k_row))
            Next
                    End Sub, Context(k_data, k_header))
        Next
    End Sub

    Private Shared Function DocumentNumber(k_text As String) As Decimal
        Dim k_match = System.Text.RegularExpressions.Regex.Match(k_text, "[0-9]+(?:[.,][0-9]+)?")
        Dim k_number As Decimal
        If Not k_match.Success OrElse Not Decimal.TryParse(k_match.Value.Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, k_number) Then Return 0D
        Return k_number
    End Function

    Private Shared Function Context(k_data As VrcOiData, k_row As Dictionary(Of String, Object)) As String
        Dim k_header = k_row
        Dim k_bon = Key(k_row, "IdBonConsum")
        Dim k_nir = Key(k_row, "IdIntrare")
        If k_bon.Length > 0 AndAlso Value(k_row, "NumarBC") Is Nothing Then k_header = If(k_data.Rows("Iesiri_H").FirstOrDefault(Function(k_r) Key(k_r, "IdBonConsum") = k_bon), k_row)
        If k_bon.Length = 0 AndAlso k_nir.Length > 0 AndAlso Value(k_row, "Numar") Is Nothing Then k_header = If(k_data.Rows("NIR").FirstOrDefault(Function(k_r) Key(k_r, "IdIntrare") = k_nir), k_row)
        Dim k_text = k_data.Source.Profile.Label
        If Value(k_header, "NumarBC") IsNot Nothing Then
            k_text &= $", bonul «{Text(k_header, "NumarBC") }», data {Text(k_header, "DataBC") }"
            Dim k_holder = k_data.Rows("Clienti").FirstOrDefault(Function(k_r) Key(k_r, "IDC") = Key(k_header, "IDC"))
            If k_holder IsNot Nothing Then k_text &= $", deținător «{Text(k_holder, "Denumire")}»"
        ElseIf Value(k_header, "Numar") IsNot Nothing Then
            k_text &= $", NIR «{Text(k_header, "Numar") }», data {Text(k_header, "Data") }, factura «{Text(k_header, "NrFact")}»"
        End If
        Dim k_variant = k_data.Rows("ProduseP").FirstOrDefault(Function(k_r) Key(k_r, "IdProdus") = Key(k_row, "IdProdus"))
        Dim k_product As Dictionary(Of String, Object) = Nothing
        If k_variant IsNot Nothing Then k_product = k_data.Rows("Produse").FirstOrDefault(Function(k_r) Key(k_r, "ID") = Key(k_variant, "ID"))
        If k_product Is Nothing AndAlso k_row.ContainsKey("DenumireP") Then k_product = k_row
        If k_product IsNot Nothing Then k_text &= $", produs «{Text(k_product, "DenumireP")}», UM «{Text(k_product, "UM")}»"
        Return k_text
    End Function

    Private Sub RemoveFromMaterials(k_data As VrcOiData, k_account As String, k_products As HashSet(Of String), k_variants As HashSet(Of String), k_nirs As HashSet(Of String), k_bons As HashSet(Of String))
        Dim k_remaining As VrcOiData = Nothing
        If Not _result.Residual.TryGetValue(k_data.Source.Source.Path, k_remaining) Then
            k_remaining = New VrcOiData With {.Source = k_data.Source, .Tables = k_data.Tables.ToDictionary(Function(k_t) k_t.Key, Function(k_t) k_t.Value.ToList(), StringComparer.OrdinalIgnoreCase)}
            _result.Residual(k_data.Source.Source.Path) = k_remaining
        End If
        k_remaining.Tables("Masini").RemoveAll(Function(k_r) Key(k_r, "IdMasina") = k_account)
        k_remaining.Tables("Produse").RemoveAll(Function(k_r) k_products.Contains(Key(k_r, "ID")))
        k_remaining.Tables("ProduseP").RemoveAll(Function(k_r) k_variants.Contains(Key(k_r, "IdProdus")))
        For Each k_table In {"NIR", "Intrari"}
            k_remaining.Tables(k_table).RemoveAll(Function(k_r) k_nirs.Contains(Key(k_r, "IdIntrare")))
        Next
        For Each k_table In {"Iesiri_H", "Iesiri"}
            k_remaining.Tables(k_table).RemoveAll(Function(k_r) k_bons.Contains(Key(k_r, "IdBonConsum")))
        Next
    End Sub

    Private Shared Function Value(k_row As Dictionary(Of String, Object), k_field As String) As Object
        Return VrcOiData.Value(k_row, k_field)
    End Function
    Private Shared Function Key(k_row As Dictionary(Of String, Object), k_field As String) As String
        Return VrcOiData.Key(k_row, k_field)
    End Function
    Private Shared Function Text(k_row As Dictionary(Of String, Object), k_field As String) As String
        Return VrcOiData.Text(k_row, k_field)
    End Function
    Private Shared Function Number(k_row As Dictionary(Of String, Object), k_field As String) As Decimal
        Return VrcOiData.Number(k_row, k_field)
    End Function
    Private Shared Function Normal(k_text As String) As String
        Return VrcOiData.Normal(k_text)
    End Function
    Private Shared Function CloneRow(k_row As Dictionary(Of String, Object)) As Dictionary(Of String, Object)
        Return VrcOiData.CloneRow(k_row)
    End Function
End Class
