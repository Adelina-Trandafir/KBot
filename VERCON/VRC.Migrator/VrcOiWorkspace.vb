Imports System.Globalization
Imports System.IO
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Controls

Public Class VrcOiWorkspace
    Private _plan As VrcPlan
    Private _conversion As VrcOiConversion
    Private _accountsHash As String = String.Empty
    Private _browserProfile As VrcFileProfile
    Private _browserFile As VrcSourceFile
    Private _loading As Boolean
    Private _operationCount As Integer
    Private _saved As New List(Of VrcOiSavedSelection)()
    Private _holderAssociations As New List(Of VrcOiHolderAssociation)()
    Private ReadOnly _romanian As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)
    Public Event PlanChanged()
    Public Event BusyChanged(k_busy As Boolean)

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Configure(k_files As IEnumerable(Of VrcSourceFile), k_plan As VrcPlan)
        _plan = k_plan
        Dim k_path = TryCast(cmbDatabase.SelectedItem, VrcSourceFile)?.Path
        _loading = True
        Try
            cmbDatabase.Items.Clear()
            For Each k_file In k_files.OrderBy(Function(k_f) Path.GetDirectoryName(k_f.Path), _romanian).ThenBy(Function(k_f) k_f.Year).ThenBy(Function(k_f) k_f.FileName, _romanian)
                cmbDatabase.Items.Add(k_file)
                If String.Equals(k_file.Path, k_path, StringComparison.OrdinalIgnoreCase) Then cmbDatabase.SelectedItem = k_file
            Next
            If cmbDatabase.SelectedIndex < 0 AndAlso cmbDatabase.Items.Count > 0 Then cmbDatabase.SelectedIndex = 0
            If _accountsHash.Length > 0 AndAlso _accountsHash <> _plan.Hash() Then
                _conversion = Nothing
                lblOiState.Text = "Plan schimbat: reîncărcați conturile."
            End If
        Finally
            _loading = False
        End Try
    End Sub

    Public Sub ApplyTo(k_plan As VrcPlan)
        If _conversion IsNot Nothing Then
            _conversion.Apply(k_plan)
        ElseIf SavedSelections().Any(Function(k_saved) k_plan.Sources.Any(Function(k_s) k_s.GestiuneType = "MATERIALE" AndAlso String.Equals(k_saved.Path, k_s.Source.Path, StringComparison.OrdinalIgnoreCase))) Then
            k_plan.Blockers.Add("Există conturi bifate pentru separare, dar planul separării nu este generat sau s-a schimbat. Folosiți «Generează separarea».")
        End If
    End Sub

    Public Sub ClearPlan()
        SavedSelections()
        SaveHolderAssociations()
        _conversion = Nothing
        _accountsHash = String.Empty
        dgvAccounts.ClearRows()
        dgvRepeats.ClearRows()
        dgvHolders.ClearRows()
        lblOiState.Text = "Recitiți bazele și încărcați conturile."
    End Sub

    Private Sub InvalidateSeparation()
        If _loading Then Return
        _conversion = Nothing
        lblOiState.Text = "Selecție schimbată: generați din nou separarea."
        RaiseEvent PlanChanged()
    End Sub

    Private Sub ChoiceChanged(sender As Object, e As KBotCellValueEventArgs) Handles dgvAccounts.CellValueChanged
        If _loading Then Return
        dgvRepeats.ClearRows()
        RefreshHolders()
        InvalidateSeparation()
    End Sub

    Private Sub HolderChanged(sender As Object, e As KBotCellValueEventArgs) Handles dgvHolders.CellValueChanged
        SaveHolderAssociations()
        dgvRepeats.ClearRows()
        InvalidateSeparation()
    End Sub

    Private Sub RepeatChanged(sender As Object, e As KBotCellValueEventArgs) Handles dgvRepeats.CellValueChanged
        InvalidateSeparation()
    End Sub

    Private Async Sub BtnAccounts_Click(sender As Object, e As EventArgs) Handles btnAccounts.Click
        If _plan Is Nothing Then Return
        Try
            If Not _plan.IsComplete Then Throw New InvalidOperationException("Completați planul din fila Migrare înainte de încărcarea conturilor.")
            _loading = True
            BeginOperation()
            _conversion = Nothing
            _accountsHash = String.Empty
            Dim k_sources = _plan.Sources.Where(Function(k_s) k_s.GestiuneType = "MATERIALE").ToList()
            Dim k_problems As New List(Of String)()
            Dim k_data = Await Task.Run(Function() VrcOiData.LoadMany(k_sources, k_problems))
            If k_problems.Count > 0 Then
                lblOiState.Text = $"Încărcare incompletă: {k_problems.Count} baze cu probleme."
                rtbOi.Text = "Nu se poate pregăti separarea până când toate bazele din plan pot fi citite:" & Environment.NewLine & String.Join(Environment.NewLine, k_problems.Select(Function(k_p) "• " & k_p))
                Return
            End If
            ClearPlan()
            dgvAccounts.BeginUpdate()
            Try
                For Each k_source In k_data.OrderBy(Function(k_d) k_d.Source.SubunitName, _romanian).ThenBy(Function(k_d) k_d.Source.GestiuneName, _romanian).ThenBy(Function(k_d) k_d.Source.Year)
                    For Each k_account In k_source.Rows("Masini").OrderBy(Function(k_r) VrcOiData.Text(k_r, "Masina"), _romanian)
                        If VrcOiData.Key(k_account, "IdMasina") = "0" Then Continue For
                        Dim k_choice As New VrcOiChoice With {.Data = k_source, .AccountKey = VrcOiData.Key(k_account, "IdMasina"), .AccountName = VrcOiData.Text(k_account, "Masina")}
                        Dim k_row = dgvAccounts.AddRow()
                        k_row.Tag = k_choice
                        Dim k_saved = _saved.FirstOrDefault(Function(k_s) String.Equals(k_s.Path, k_source.Source.Source.Path, StringComparison.OrdinalIgnoreCase) AndAlso k_s.AccountKey = k_choice.AccountKey)
                        k_row("select") = k_saved IsNot Nothing
                        k_row("source") = k_source.Source.Profile.Label
                        k_row("subunit") = k_source.Source.SubunitName
                        k_row("gestiune") = k_source.Source.GestiuneName
                        k_row("year") = k_source.Source.Year
                        k_row("account") = k_choice.AccountName
                        k_row("use") = If(k_saved?.UseAccountName, String.Empty)
                        k_row("targettype") = If(k_saved?.TargetType, String.Empty)
                        k_row("targetname") = If(String.IsNullOrWhiteSpace(k_saved?.TargetName), k_choice.AccountName, k_saved.TargetName)
                    Next
                Next
            Finally
                dgvAccounts.EndUpdate()
            End Try
            _accountsHash = _plan.Hash()
            RefreshHolders()
            lblOiState.Text = $"{dgvAccounts.RowCount} conturi disponibile; bifați conturile de separat și alegeți tipul destinației."
            rtbOi.Text = "Alegeți tipul și numele destinației pe fiecare cont. Același nume de gestiune în mai mulți ani formează o singură gestiune. Numai OI cere contul de folosință și transformă bonurile. În grila deținătorilor, alegeți același nume destinație pentru a asocia variantele aceluiași nume; numele diferite rămân distincte."
        Catch ex As Exception
            ShowFailure("Încărcarea conturilor", ex)
        Finally
            _loading = False
            EndOperation()
            RaiseEvent PlanChanged()
        End Try
    End Sub

    Private Async Sub BtnAnalyze_Click(sender As Object, e As EventArgs) Handles btnAnalyze.Click
        Await Analyze(False)
    End Sub

    Private Async Sub BtnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        Await Analyze(True)
    End Sub

    Private Async Function Analyze(k_generate As Boolean) As Task
        Try
            If _plan Is Nothing OrElse _accountsHash <> _plan.Hash() Then Throw New InvalidOperationException("Planul s-a schimbat sau conturile nu au fost încărcate. Reîncărcați conturile din plan.")
            Dim k_choices As New List(Of VrcOiChoice)()
            For Each k_row In dgvAccounts.Rows.Where(Function(k_r) Convert.ToBoolean(k_r("select"), CultureInfo.InvariantCulture))
                Dim k_choice = DirectCast(k_row.Tag, VrcOiChoice)
                k_choice.UseAccountName = Convert.ToString(k_row("use"), CultureInfo.InvariantCulture)
                k_choice.TargetName = Convert.ToString(k_row("targetname"), CultureInfo.InvariantCulture)
                k_choice.TargetType = Convert.ToString(k_row("targettype"), CultureInfo.InvariantCulture)
                k_choices.Add(k_choice)
            Next
            Dim k_accept = dgvRepeats.Rows.Where(Function(k_r) Convert.ToBoolean(k_r("accept"), CultureInfo.InvariantCulture)).Select(Function(k_r) DirectCast(k_r.Tag, VrcOiRepeat).Key).ToHashSet(StringComparer.OrdinalIgnoreCase)
            BeginOperation()
            _conversion = Nothing
            Dim k_holders = SaveHolderAssociations()
            Dim k_builder As New VrcAccountSeparation(k_accept, k_holders)
            Dim k_allData = dgvAccounts.Rows.Select(Function(k_r) DirectCast(k_r.Tag, VrcOiChoice).Data).Distinct().ToList()
            Dim k_result = Await Task.Run(Function() k_builder.Build(_plan, k_choices, k_allData))
            rtbOi.Text = k_result.Report
            _loading = True
            dgvRepeats.ClearRows()
            For Each k_repeat In k_builder.Repeats.OrderBy(Function(k_r) k_r.Product, _romanian).ThenBy(Function(k_r) k_r.Unit, _romanian).ThenBy(Function(k_r) k_r.Price).ThenBy(Function(k_r) k_r.Holder, _romanian).ThenBy(Function(k_r) k_r.File, _romanian)
                Dim k_row = dgvRepeats.AddRow()
                k_row.Tag = k_repeat
                k_row("accept") = k_accept.Contains(k_repeat.Key)
                k_row("file") = k_repeat.File
                k_row("previous") = k_repeat.PreviousFile
                k_row("product") = k_repeat.Product
                k_row("unit") = k_repeat.Unit
                k_row("price") = k_repeat.Price
                k_row("holder") = k_repeat.Holder
                k_row("quantity") = k_repeat.Quantity
            Next
            If k_result.Blockers.Count > 0 Then
                lblOiState.Text = $"Blocat: {k_result.Blockers.Count} constatări (raportul de mai jos)."
            ElseIf k_generate Then
                _conversion = k_result
                lblOiState.Text = "Generat în plan. Continuați în Migrare: Verifică planul → Testează → Migrează."
                rtbOi.AppendText(Environment.NewLine & "Separarea este pregătită în memorie. Transferul se face numai prin butonul Migrează, în aceeași tranzacție cu restul planului.")
            Else
                lblOiState.Text = "Analiză completă. Puteți genera separarea în plan."
            End If
        Catch ex As Exception
            ShowFailure("Separarea OI", ex)
        Finally
            _loading = False
            EndOperation()
            RaiseEvent PlanChanged()
        End Try
    End Function

    Private Sub BtnDiscard_Click(sender As Object, e As EventArgs) Handles btnDiscard.Click
        _loading = True
        Try
            For Each k_row In dgvAccounts.Rows
                k_row("select") = False
            Next
            If _plan IsNot Nothing Then _saved.RemoveAll(Function(k_saved) _plan.Sources.Any(Function(k_s) String.Equals(k_s.Source.Path, k_saved.Path, StringComparison.OrdinalIgnoreCase)))
            dgvRepeats.ClearRows()
            dgvHolders.ClearRows()
            _conversion = Nothing
            lblOiState.Text = "Separarea a fost scoasă din plan; sursele de Materiale rămân întregi."
        Finally
            _loading = False
            RaiseEvent PlanChanged()
        End Try
    End Sub

    Private Async Sub BtnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Dim k_file = TryCast(cmbDatabase.SelectedItem, VrcSourceFile)
        If k_file Is Nothing Then Return
        Try
            BeginOperation()
            Dim k_profile = Await Task.Run(Function() VrcProfiler.Profile(k_file, Nothing))
            If k_profile.ReadError.Length > 0 Then Throw New InvalidOperationException(k_profile.ReadError)
            If Not k_profile.Stable OrElse k_profile.LinkedTables.Count > 0 Then Throw New InvalidOperationException("Baza s-a schimbat la citire sau are tabele legate; nu se deschid surse externe.")
            _browserProfile = k_profile
            _browserFile = k_file
            cmbTable.Items.Clear()
            For Each k_table In k_profile.Tables.OrderBy(Function(k_t) k_t.Name, _romanian)
                cmbTable.Items.Add(k_table.Name)
            Next
            If cmbTable.Items.Count > 0 Then cmbTable.SelectedIndex = 0
        Catch ex As Exception
            ShowFailure("Deschiderea bazei", ex)
        Finally
            EndOperation()
        End Try
    End Sub

    Private Sub DatabaseChanged(sender As Object, e As EventArgs) Handles cmbDatabase.SelectedIndexChanged
        If _loading Then Return
        _browserProfile = Nothing
        _browserFile = Nothing
        cmbTable.Items.Clear()
        dgvBrowse.ClearRows()
        dgvBrowse.Columns.Clear()
    End Sub

    Private Async Sub TableChanged(sender As Object, e As EventArgs) Handles cmbTable.SelectedIndexChanged
        If _browserProfile Is Nothing OrElse _browserFile Is Nothing OrElse cmbTable.SelectedItem Is Nothing Then Return
        BeginOperation()
        Try
            Dim k_table = CStr(cmbTable.SelectedItem)
            Dim k_file = _browserFile
            Dim k_profile = _browserProfile
            Dim k_rows = Await Task.Run(Function()
                                           Dim k_provider As String = String.Empty
                                           Using k_cn = VrcAccess.OpenReadOnly(k_file.Path, k_provider)
                                               Return VrcUsageReader.AllRows(k_cn, k_profile, k_table)
                                           End Using
                                       End Function)
            If _browserFile IsNot k_file OrElse CStr(cmbTable.SelectedItem) <> k_table Then Return
            dgvBrowse.BeginUpdate()
            Try
                dgvBrowse.ClearRows()
                dgvBrowse.Columns.Clear()
                Dim k_schema = k_profile.Tables.Single(Function(k_t) k_t.Name = k_table)
                For Each k_name In k_schema.Columns
                    Dim k_sample = k_rows.Select(Function(k_r) VrcOiData.Value(k_r, k_name)).FirstOrDefault(Function(k_v) k_v IsNot Nothing)
                    Dim k_type = KBotValueType.Text
                    If TypeOf k_sample Is Date Then
                        k_type = KBotValueType.DateTime
                    ElseIf TypeOf k_sample Is Boolean Then
                        k_type = KBotValueType.Boolean
                    ElseIf k_sample IsNot Nothing AndAlso TypeOf k_sample Is IConvertible AndAlso Type.GetTypeCode(k_sample.GetType()) >= TypeCode.SByte AndAlso Type.GetTypeCode(k_sample.GetType()) <= TypeCode.Decimal Then
                        k_type = KBotValueType.Number
                    End If
                    dgvBrowse.Columns.Add(New KBotDataColumn With {.Key = k_name, .HeaderText = k_name, .ReadOnly = True, .ValueType = k_type, .Width = 170})
                Next
                Dim k_sort = {"Masina", "DenumireP", "Denumire", "DataBC", "Data", "DATABP", "DATABT", "DATABC", "PUTVA", "TVA"}.FirstOrDefault(Function(k_n) k_schema.Columns.Contains(k_n, StringComparer.OrdinalIgnoreCase))
                If k_sort IsNot Nothing Then k_rows.Sort(Function(k_a, k_b) CompareValues(VrcOiData.Value(k_a, k_sort), VrcOiData.Value(k_b, k_sort)))
                For Each k_data In k_rows
                    Dim k_row = dgvBrowse.AddRow()
                    For Each k_cell In k_data
                        k_row(k_cell.Key) = k_cell.Value
                    Next
                Next
            Finally
                dgvBrowse.EndUpdate()
            End Try
        Catch ex As Exception
            ShowFailure("Citirea tabelului", ex)
        Finally
            EndOperation()
        End Try
    End Sub

    Private Sub BeginOperation()
        _operationCount += 1
        tlpOi.Enabled = False
        RaiseEvent BusyChanged(True)
    End Sub

    Private Sub EndOperation()
        _operationCount = Math.Max(0, _operationCount - 1)
        tlpOi.Enabled = _operationCount = 0
        RaiseEvent BusyChanged(_operationCount > 0)
    End Sub

    Private Function CompareValues(k_left As Object, k_right As Object) As Integer
        If k_left Is Nothing Then Return If(k_right Is Nothing, 0, -1)
        If k_right Is Nothing Then Return 1
        If TypeOf k_left Is String Then Return _romanian.Compare(CStr(k_left), Convert.ToString(k_right, CultureInfo.InvariantCulture))
        Return DirectCast(k_left, IComparable).CompareTo(k_right)
    End Function

    Private Sub ShowFailure(k_context As String, k_ex As Exception)
        GlobalErrorLog.Write("VrcOiWorkspace." & k_context, k_ex)
        lblOiState.Text = k_context & ": " & k_ex.Message
        rtbOi.Text = lblOiState.Text
    End Sub

    Public Sub SetEditingEnabled(k_enabled As Boolean)
        dgvAccounts.Enabled = k_enabled
        dgvRepeats.Enabled = k_enabled
        dgvHolders.Enabled = k_enabled
        btnAccounts.Enabled = k_enabled
        btnAnalyze.Enabled = k_enabled
        btnGenerate.Enabled = k_enabled
        btnDiscard.Enabled = k_enabled
    End Sub

    Public Sub RestoreSelections(k_saved As List(Of VrcOiSavedSelection), Optional k_holders As List(Of VrcOiHolderAssociation) = Nothing)
        _saved = If(k_saved, New List(Of VrcOiSavedSelection)()).ToList()
        _holderAssociations = If(k_holders, New List(Of VrcOiHolderAssociation)()).ToList()
    End Sub

    Public Function SavedSelections() As List(Of VrcOiSavedSelection)
        Dim k_paths = dgvAccounts.Rows.Select(Function(k_r) DirectCast(k_r.Tag, VrcOiChoice).Data.Source.Source.Path).ToHashSet(StringComparer.OrdinalIgnoreCase)
        _saved.RemoveAll(Function(k_s) k_paths.Contains(k_s.Path))
        For Each k_row In dgvAccounts.Rows.Where(Function(k_r) Convert.ToBoolean(k_r("select"), CultureInfo.InvariantCulture))
            Dim k_choice = DirectCast(k_row.Tag, VrcOiChoice)
            _saved.Add(New VrcOiSavedSelection With {.Path = k_choice.Data.Source.Source.Path, .AccountKey = k_choice.AccountKey, .UseAccountName = Convert.ToString(k_row("use"), CultureInfo.InvariantCulture), .TargetName = Convert.ToString(k_row("targetname"), CultureInfo.InvariantCulture), .TargetType = Convert.ToString(k_row("targettype"), CultureInfo.InvariantCulture)})
        Next
        Return _saved.ToList()
    End Function

    Public Function SaveHolderAssociations() As List(Of VrcOiHolderAssociation)
        For Each k_row In dgvHolders.Rows
            Dim k_holder = DirectCast(k_row.Tag, VrcOiHolderAssociation)
            k_holder.TargetName = Convert.ToString(k_row("target"), CultureInfo.InvariantCulture)
            _holderAssociations.RemoveAll(Function(k_h) String.Equals(k_h.Identity, k_holder.Identity, StringComparison.OrdinalIgnoreCase))
            _holderAssociations.Add(k_holder)
        Next
        Return _holderAssociations.ToList()
    End Function

    Private Sub RefreshHolders()
        SaveHolderAssociations()
        Dim k_names = dgvAccounts.Rows.Select(Function(k_r) DirectCast(k_r.Tag, VrcOiChoice).Data).Distinct().SelectMany(Function(k_d) k_d.Rows("Clienti")).Where(Function(k_r) VrcOiData.Key(k_r, "IDC").Length > 0 AndAlso VrcOiData.Key(k_r, "IDC") <> "0").Select(Function(k_r) VrcOiData.Text(k_r, "Denumire")).Concat(_holderAssociations.Select(Function(k_h) k_h.TargetName)).Where(Function(k_n) Not String.IsNullOrWhiteSpace(k_n)).Distinct(StringComparer.Ordinal).OrderBy(Function(k_n) k_n, _romanian).Cast(Of Object)().ToList()
        dgvHolders.Columns.First(Function(k_c) k_c.Key = "target").ComboItems = k_names
        dgvHolders.BeginUpdate()
        Try
            dgvHolders.ClearRows()
            For Each k_group In dgvAccounts.Rows.Where(Function(k_r) Convert.ToBoolean(k_r("select"), CultureInfo.InvariantCulture)).Select(Function(k_r) DirectCast(k_r.Tag, VrcOiChoice)).GroupBy(Function(k_c) k_c.Data)
                Dim k_data = k_group.Key
                Dim k_accounts = k_group.Select(Function(k_c) k_c.AccountKey).ToHashSet(StringComparer.Ordinal)
                Dim k_products = k_data.Rows("Produse").Where(Function(k_r) k_accounts.Contains(VrcOiData.Key(k_r, "IdMasina"))).Select(Function(k_r) VrcOiData.Key(k_r, "ID")).ToHashSet(StringComparer.Ordinal)
                Dim k_variants = k_data.Rows("ProduseP").Where(Function(k_r) k_products.Contains(VrcOiData.Key(k_r, "ID"))).Select(Function(k_r) VrcOiData.Key(k_r, "IdProdus")).ToHashSet(StringComparer.Ordinal)
                Dim k_bons = k_data.Rows("Iesiri").Where(Function(k_r) k_variants.Contains(VrcOiData.Key(k_r, "IdProdus"))).Select(Function(k_r) VrcOiData.Key(k_r, "IdBonConsum")).ToHashSet(StringComparer.Ordinal)
                Dim k_headers = k_data.Rows("Iesiri_H").Where(Function(k_r) k_bons.Contains(VrcOiData.Key(k_r, "IdBonConsum")) OrElse k_accounts.Contains(VrcOiData.Key(k_r, "IdMasina"))).ToList()
                For Each k_uses In k_headers.GroupBy(Function(k_r) VrcOiData.Key(k_r, "IDC")).OrderBy(Function(k_g) VrcOiHolders.SourceName(k_data, k_g.Key), _romanian).
                    ThenBy(Function(k_g) k_g.Min(Function(k_b) VrcOiHolders.DocumentDate(k_b).GetValueOrDefault(Date.MaxValue))).
                    ThenBy(Function(k_g) k_g.Min(Function(k_b) VrcOiHolders.DocumentNumber(k_b).GetValueOrDefault(Decimal.MaxValue)))
                    Dim k_holder As New VrcOiHolderAssociation With {.SourcePath = k_data.Source.Source.Path, .SourceKey = k_uses.Key, .SourceName = VrcOiHolders.SourceName(k_data, k_uses.Key)}
                    k_holder.TargetName = If(_holderAssociations.FirstOrDefault(Function(k_h) String.Equals(k_h.Identity, k_holder.Identity, StringComparison.OrdinalIgnoreCase))?.TargetName, VrcOiHolders.DefaultTarget(k_data, k_uses.Key))
                    Dim k_first = k_uses.OrderBy(Function(k_b) VrcOiHolders.DocumentDate(k_b).GetValueOrDefault(Date.MaxValue)).ThenBy(Function(k_b) VrcOiHolders.DocumentNumber(k_b).GetValueOrDefault(Decimal.MaxValue)).First()
                    Dim k_row = dgvHolders.AddRow()
                    k_row.Tag = k_holder
                    k_row("source") = k_data.Source.Profile.Label
                    k_row("year") = k_data.Source.Year
                    k_row("original") = k_holder.SourceName
                    k_row("target") = k_holder.TargetName
                    k_row("firstdate") = VrcOiHolders.DocumentDate(k_first)
                    k_row("firstnumber") = VrcOiHolders.DocumentNumber(k_first)
                    k_row("documents") = k_uses.Count()
                Next
            Next
        Finally
            dgvHolders.EndUpdate()
        End Try
    End Sub
End Class
