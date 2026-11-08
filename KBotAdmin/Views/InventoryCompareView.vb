Option Strict On
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Text.Json

' SLICE-VC12-01. Gestiune / Subgestiune / Cont = SubunitId / GestiuneId / IdMasina.
Public Class InventoryCompareView
    Implements IAdminView

    Private _context As AdminContext
    Private _targetsDb As String = String.Empty
    Private _targets As JsonElement
    Private _items As New List(Of InventoryItem)()
    Private _results As New List(Of JsonElement)()
    Private _source As New Dictionary(Of Integer, InventoryItem)()
    Private _readPath As String = String.Empty
    Private _readSheet As String = String.Empty
    Private _reportContext As String = String.Empty
    Private _busy As Boolean
    Private ReadOnly _names As StringComparer = StringComparer.Create(CultureInfo.GetCultureInfo("ro-RO"), True)

    Private Class Pick
        Public Property Id As Integer
        Public Property Name As String
        Public Property Kind As String
        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    Public Sub New()
        InitializeComponent()
        AddColumn(grid, "row", "Rând Excel", 85, True)
        AddColumn(grid, "name", "Denumire Excel", 260)
        AddColumn(grid, "code", "Cod Excel", 100)
        AddColumn(grid, "um", "UM Excel", 70)
        AddColumn(grid, "quantity", "Cant. scriptică Excel", 100, True)
        AddColumn(grid, "price", "Preț Excel", 105, True)
        AddColumn(grid, "status", "Rezultat", 240)
        AddColumn(grid, "best", "Candidat principal VERCON", 260)
        AddColumn(grid, "score", "Scor /100", 90, True)
        AddColumn(grid, "count", "Nr. candidați", 90, True)
        AddColumn(grid, "note", "Observații", 340)
        AddColumn(candidates, "rank", "Ordine potrivire", 90, True)
        AddColumn(candidates, "name", "Denumire VERCON", 300)
        AddColumn(candidates, "code", "Cod VERCON", 100)
        AddColumn(candidates, "um", "UM VERCON", 75)
        AddColumn(candidates, "price", "Preț cu TVA VERCON", 130, True)
        AddColumn(candidates, "delta", "VERCON − Excel", 120, True)
        AddColumn(candidates, "name_score", "Scor nume /100", 110, True)
        AddColumn(candidates, "score", "Scor total /100", 110, True)
        AddColumn(candidates, "reason", "Explicație", 470)
        AddColumn(candidates, "product_id", "ID produs", 90, True)
        AddColumn(candidates, "variant_id", "ID variantă", 90, True)
        grid.ApplySort("name", KBotSortDirection.Ascending)
        candidates.ApplySort("rank", KBotSortDirection.Ascending)
    End Sub

    Private Shared Sub AddColumn(k_grid As KBotDataView, k_key As String, k_title As String, k_width As Integer, Optional k_numeric As Boolean = False)
        Dim k_column = k_grid.AddColumn(k_key, k_title, KBotColumnType.Text, k_width)
        k_column.ReadOnly = True
        k_column.ShowColumnFilter = True
        k_column.ValueType = If(k_numeric, KBotValueType.Number, KBotValueType.Text)
        If k_numeric Then k_column.FormatString = "0.####"
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        SelectionChanged()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        If _targetsDb <> String.Empty AndAlso Not String.Equals(_targetsDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
            ClearReport()
            lblState.Text = "Baza s-a schimbat. Apăsați «Citește destinațiile» pentru baza selectată."
        End If
    End Sub

    Private Sub ClearReport()
        _results.Clear()
        grid.ClearRows()
        candidates.ClearRows()
        btnExport.Enabled = False
        _reportContext = String.Empty
    End Sub

    Private Async Sub Browse_Click(k_sender As Object, e As EventArgs) Handles btnBrowse.Click
        Await RunAsync(Async Function()
            Using k_dialog As New OpenFileDialog With {.Filter = "Excel (*.xlsx)|*.xlsx", .Title = "Inventar de comparat"}
                If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                txtFile.Text = k_dialog.FileName
            End Using
            Await LoadSheetsAsync()
        End Function)
    End Sub

    Private Async Function LoadSheetsAsync() As Task
        Dim k_path = txtFile.Text.Trim().Trim(""""c)
        Dim k_sheets = Await Task.Run(Function() InventoryExcel.Sheets(k_path))
        cmbSheet.Items.Clear()
        For Each k_sheet In k_sheets
            cmbSheet.Items.Add(k_sheet)
        Next
        If k_sheets.Count > 0 Then cmbSheet.SelectedIndex = Math.Max(0, k_sheets.FindIndex(Function(k_s) k_s.Equals("scoala 2025", StringComparison.OrdinalIgnoreCase)))
    End Function

    Private Sub SourceChanged(k_sender As Object, e As EventArgs) Handles txtFile.TextChanged, cmbSheet.SelectedIndexChanged
        _items.Clear()
        _readPath = String.Empty
        If k_sender Is txtFile Then cmbSheet.Items.Clear()
        ClearReport()
        lblState.Text = "Sursa s-a schimbat. Apăsați «Citește foaia»."
    End Sub

    Private Async Sub Read_Click(k_sender As Object, e As EventArgs) Handles btnRead.Click
        Await RunAsync(Async Function()
            ClearReport()
            _items.Clear()
            If cmbSheet.SelectedIndex < 0 Then Await LoadSheetsAsync()
            If cmbSheet.SelectedIndex < 0 Then Throw New AdminException("Alegeți o foaie Excel.")
            Dim k_path = txtFile.Text.Trim().Trim(""""c)
            Dim k_sheet = CStr(cmbSheet.SelectedItem)
            _items = Await Task.Run(Function() InventoryExcel.Read(k_path, k_sheet))
            _readPath = k_path
            _readSheet = k_sheet
            lblState.Text = _items.Count.ToString() & " poziții citite: " & _items.Where(Function(k_i) k_i.Kind = "MF").Count().ToString() &
                " MF și " & _items.Where(Function(k_i) k_i.Kind = "OI").Count().ToString() & " OI. Se compară numai secțiunea de tipul subgestiunii alese."
        End Function)
    End Sub

    Private Shared Function Selected(k_combo As KBotComboBox) As Pick
        Return TryCast(k_combo.SelectedItem, Pick)
    End Function

    Private Sub Fill(k_combo As KBotComboBox, k_rows As IEnumerable(Of JsonElement), Optional k_kind As Boolean = False)
        k_combo.Items.Clear()
        For Each k_row In k_rows.OrderBy(Function(k_r) k_r.GetProperty("name").GetString(), _names)
            k_combo.Items.Add(New Pick With {.Id = k_row.GetProperty("id").GetInt32(),
                .Name = k_row.GetProperty("name").GetString(), .Kind = If(k_kind, k_row.GetProperty("type").GetString(), String.Empty)})
        Next
        If k_combo.Items.Count > 0 Then k_combo.SelectedIndex = 0
    End Sub

    Private Async Sub Targets_Click(k_sender As Object, e As EventArgs) Handles btnTargets.Click
        Await RunAsync(Async Function()
            Dim k_db = _context.RequireSingleDb("Comparația inventarului")
            If k_db = String.Empty Then Return
            ClearReport()
            _targetsDb = String.Empty
            cmbSubunit.Items.Clear()
            cmbGestiune.Items.Clear()
            cmbAccount.Items.Clear()
            _targets = Await _context.Client.CallRawAsync("vercon-inventory-targets", "--db", k_db)
            If Not String.Equals(k_db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then Throw New AdminException("Baza s-a schimbat în timpul citirii. Citiți din nou destinațiile.")
            _targetsDb = _targets.GetProperty("unit").GetString()
            Fill(cmbSubunit, _targets.GetProperty("subunits").EnumerateArray())
            lblState.Text = "Destinații citite pentru " & _targetsDb & ". Alegeți Gestiune / Subgestiune / Cont și apăsați «Compară»."
        End Function)
    End Sub

    Private Sub SubunitChanged(k_sender As Object, e As EventArgs) Handles cmbSubunit.SelectedIndexChanged
        Try
            ClearReport()
            cmbGestiune.Items.Clear()
            cmbAccount.Items.Clear()
            Dim k_subunit = Selected(cmbSubunit)
            If k_subunit Is Nothing Then Return
            Fill(cmbGestiune, _targets.GetProperty("gestiuni").EnumerateArray().Where(
                Function(k_r) k_r.GetProperty("subunit").GetInt32() = k_subunit.Id), True)
        Catch ex As Exception
            GlobalErrorLog.Write("InventoryCompareView.SubunitChanged", ex)
        End Try
    End Sub

    Private Sub GestiuneChanged(k_sender As Object, e As EventArgs) Handles cmbGestiune.SelectedIndexChanged
        Try
            ClearReport()
            cmbAccount.Items.Clear()
            Dim k_subunit = Selected(cmbSubunit)
            Dim k_gestiune = Selected(cmbGestiune)
            If k_subunit Is Nothing OrElse k_gestiune Is Nothing Then Return
            Fill(cmbAccount, _targets.GetProperty("accounts").EnumerateArray().Where(
                Function(k_r) k_r.GetProperty("subunit").GetInt32() = k_subunit.Id AndAlso k_r.GetProperty("gestiune").GetInt32() = k_gestiune.Id))
        Catch ex As Exception
            GlobalErrorLog.Write("InventoryCompareView.GestiuneChanged", ex)
        End Try
    End Sub

    Private Sub AccountChanged(k_sender As Object, e As EventArgs) Handles cmbAccount.SelectedIndexChanged
        ClearReport()
    End Sub

    Private Async Sub Compare_Click(k_sender As Object, e As EventArgs) Handles btnCompare.Click
        Await RunAsync(AddressOf CompareAsync)
    End Sub

    Private Async Function CompareAsync() As Task
        ClearReport()
        Dim k_db = _context.RequireSingleDb("Comparația inventarului")
        If k_db = String.Empty Then Return
        If Not String.Equals(k_db, _targetsDb, StringComparison.OrdinalIgnoreCase) Then Throw New AdminException("Citiți destinațiile pentru baza aleasă.")
        Dim k_subunit = Selected(cmbSubunit)
        Dim k_gestiune = Selected(cmbGestiune)
        Dim k_account = Selected(cmbAccount)
        If k_subunit Is Nothing OrElse k_gestiune Is Nothing OrElse k_account Is Nothing Then Throw New AdminException("Alegeți Gestiune / Subgestiune / Cont.")
        If _items.Count = 0 OrElse _readPath = String.Empty Then Throw New AdminException("Citiți mai întâi foaia Excel.")
        Dim k_items = _items.Where(Function(k_i) k_i.Kind = k_gestiune.Kind).ToList()
        If k_items.Count = 0 Then Throw New AdminException("Foaia nu conține poziții de tip " & k_gestiune.Kind & ".")
        Dim k_payload = k_items.Select(Function(k_i) New With {.row = k_i.Row, .name = k_i.Name, .code = k_i.Code,
            .um = k_i.Um, .price = k_i.Price, .quantity = k_i.Quantity, .kind = k_i.Kind}).ToList()
        Dim k_bytes = JsonSerializer.SerializeToUtf8Bytes(k_payload)
        If k_bytes.Length > 2000000 OrElse k_items.Count > 5000 Then Throw New AdminException("Maximum 5000 de poziții / 2 MB per comparație.")
        Dim k_remote = "/tmp/kbot-inventory-" & Guid.NewGuid().ToString("N") & ".json"
        Dim k_error As Exception = Nothing
        Dim k_result As JsonElement
        lblState.Text = "Se compară " & k_items.Count.ToString() & " poziții " & k_gestiune.Kind & " cu nomenclatorul VERCON…"
        Try
            Await _context.Client.UploadBytesAsync(k_bytes, k_remote)
            k_result = Await _context.Client.CallRawAsync("vercon-inventory-check", "--db", k_db,
                "--subunit", k_subunit.Id.ToString(CultureInfo.InvariantCulture), "--gestiune", k_gestiune.Id.ToString(CultureInfo.InvariantCulture),
                "--account", k_account.Id.ToString(CultureInfo.InvariantCulture), "--payload-file", k_remote)
        Catch ex As Exception
            k_error = ex
        End Try
        ' The server consumes/removes the file; also clean up when the command could not start.
        Try
            Await _context.Client.DeleteRemoteAsync(k_remote)
        Catch ex As Exception
            GlobalErrorLog.Write("InventoryCompareView.Cleanup", ex)
            Throw New AdminException("Nu am putut confirma ștergerea fișierului temporar " & k_remote & ". " & If(k_error?.Message, String.Empty), ex)
        End Try
        If k_error IsNot Nothing Then Throw New AdminException(k_error.Message, k_error)
        If Not String.Equals(k_db, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then Throw New AdminException("Baza s-a schimbat în timpul comparației. Reluați pentru baza aleasă.")
        _results = k_result.GetProperty("rows").EnumerateArray().ToList()
        _source = k_items.ToDictionary(Function(k_i) k_i.Row)
        _reportContext = k_db & " / " & k_subunit.Name & " / " & k_gestiune.Name & " / " & k_account.Name &
            "; " & Path.GetFileName(_readPath) & " / " & _readSheet
        FillResults()
        Dim k_ambiguous = _results.Where(Function(k_r) k_r.GetProperty("status").GetString().StartsWith("Ambiguu", StringComparison.Ordinal)).Count()
        Dim k_without = _results.Where(Function(k_r) k_r.GetProperty("candidate_count").GetInt32() = 0).Count()
        lblState.Text = _reportContext & vbLf & _results.Count.ToString() & " poziții comparate; " & k_ambiguous.ToString() &
            " ambigue; " & k_without.ToString() & " fără candidat; " & (_items.Count - k_items.Count).ToString() & " poziții de alt tip omise. Nicio scriere în baza de date."
        btnExport.Enabled = True
    End Function

    Friend Shared Function Numeric(k_value As String) As Object
        If String.IsNullOrWhiteSpace(k_value) Then Return Nothing
        Dim k_text = k_value.Trim().Replace(ChrW(160).ToString(), String.Empty).Replace(" ", String.Empty)
        If k_text.Contains(",") AndAlso k_text.Contains(".") Then
            k_text = If(k_text.LastIndexOf(","c) > k_text.LastIndexOf("."c), k_text.Replace(".", String.Empty).Replace(",", "."), k_text.Replace(",", String.Empty))
        Else
            k_text = k_text.Replace(",", ".")
        End If
        Dim k_number As Decimal
        Return If(Decimal.TryParse(k_text, NumberStyles.Float, CultureInfo.InvariantCulture, k_number), CType(k_number, Object), Nothing)
    End Function

    Private Sub FillResults()
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each k_result In _results.OrderBy(Function(k_r) _source(k_r.GetProperty("row").GetInt32()).Name, _names)
                Dim k_item = _source(k_result.GetProperty("row").GetInt32())
                Dim k_row = grid.AddRow()
                k_row.Tag = k_result
                k_row("row") = k_item.Row
                k_row("name") = k_item.Name
                k_row("code") = k_item.Code
                k_row("um") = k_item.Um
                k_row("quantity") = Numeric(k_item.Quantity)
                k_row("price") = Numeric(k_item.Price)
                k_row("status") = k_result.GetProperty("status").GetString()
                k_row("count") = k_result.GetProperty("candidate_count").GetInt32()
                Dim k_list = k_result.GetProperty("candidates")
                If k_list.GetArrayLength() > 0 Then
                    k_row("best") = k_list(0).GetProperty("name").GetString()
                    k_row("score") = k_list(0).GetProperty("score").GetDecimal()
                End If
                k_row("note") = k_result.GetProperty("note").GetString()
            Next
        Finally
            grid.FinishUpdate()
        End Try
        grid.CurrentRowIndex = grid.FirstDisplayedRowIndex()
        FillCandidates()
    End Sub

    Private Sub GridSelectionChanged(k_sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            FillCandidates()
        Catch ex As Exception
            GlobalErrorLog.Write("InventoryCompareView.GridSelectionChanged", ex)
        End Try
    End Sub

    Private Sub FillCandidates()
        candidates.BeginUpdate()
        Try
            candidates.ClearRows()
            If grid.CurrentRow Is Nothing OrElse grid.CurrentRow.Tag Is Nothing Then Return
            Dim k_result = DirectCast(grid.CurrentRow.Tag, JsonElement)
            Dim k_rank = 0
            For Each k_candidate In k_result.GetProperty("candidates").EnumerateArray()
                k_rank += 1
                Dim k_row = candidates.AddRow()
                k_row("rank") = k_rank
                For Each k_key In {"name", "code", "um", "reason"}
                    k_row(k_key) = k_candidate.GetProperty(k_key).GetString()
                Next
                k_row("price") = Numeric(k_candidate.GetProperty("price").GetString())
                k_row("delta") = Numeric(k_candidate.GetProperty("price_delta").GetString())
                k_row("score") = k_candidate.GetProperty("score").GetDecimal()
                k_row("name_score") = k_candidate.GetProperty("name_score").GetDecimal()
                k_row("product_id") = k_candidate.GetProperty("product_id").GetInt64()
                Dim k_variant = k_candidate.GetProperty("variant_id").GetInt64()
                k_row("variant_id") = If(k_variant > 0, CType(k_variant, Object), Nothing)
            Next
        Finally
            candidates.FinishUpdate()
        End Try
    End Sub

    Private Async Function RunAsync(k_work As Func(Of Task)) As Task
        If _busy Then Return
        _busy = True
        tlyFile.Enabled = False
        tlyTarget.Enabled = False
        Try
            Await UiRun.Guard("InventoryCompareView", _context, k_work)
        Finally
            tlyFile.Enabled = True
            tlyTarget.Enabled = True
            _busy = False
        End Try
    End Function
End Class
