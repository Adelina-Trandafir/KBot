Option Strict On
Imports System.Globalization
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

''' <summary>
''' «Import MF din Access» (VERCON, sliceless): the active fixed assets of an old mf.mdb go into an MF gestiune of one unit,
''' in the warehouse account the operator picks, with no holder (the holder comes later with a handover voucher).
''' The file is read on this PC (<see cref="MfAccess"/>); the server half is <c>routes/admin_cli/vercon_mf_import.py</c>,
''' which writes through the receipt code of the VERCON page. Nothing is sent to the server before «Verifică» / «Importă».
''' </summary>
Public Class MfImportView
    Implements IAdminView

    Private Class Pick
        Public Property Id As Integer
        Public Property Parent As Integer
        Public Property Name As String = String.Empty
    End Class

    Private _context As AdminContext
    Private _assets As New List(Of MfAsset)()
    Private ReadOnly _failed As New HashSet(Of MfAsset)()  ' assets the server refused at the last check (by object: the grid can be sorted)
    Private ReadOnly _subunits As New List(Of Pick)()
    Private ReadOnly _gestiuni As New List(Of Pick)()      ' Parent = subunit id
    Private ReadOnly _accounts As New List(Of Pick)()      ' Parent = gestiune id, the subunit is the one chosen
    Private ReadOnly _accountSubunit As New Dictionary(Of Integer, Integer)()
    Private _targetsDb As String = String.Empty
    Private _busy As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Attach(k_context As AdminContext) Implements IAdminView.Attach
        _context = k_context
    End Sub

    Public Function ActivatedAsync() As Task Implements IAdminView.ActivatedAsync
        ShowState()
        Return Task.CompletedTask
    End Function

    Public Sub SelectionChanged() Implements IAdminView.SelectionChanged
        ShowState()
    End Sub

    Private Sub ShowState()
        If _targetsDb <> String.Empty AndAlso Not String.Equals(_targetsDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
            lblState.Text = "Unitatea s-a schimbat față de cea citită (" & _targetsDb & "). Apăsați «Citește subunitățile» din nou."
        ElseIf _assets.Count = 0 Then
            lblState.Text = "Alegeți fișierul mf.mdb, apoi «Citește mijloacele fixe»; alegeți unitatea în arbore și «Citește subunitățile»."
        End If
    End Sub

    Private Sub BtnBrowse_Click(k_sender As Object, e As EventArgs) Handles btnBrowse.Click
        Try
            Using k_dialog As New OpenFileDialog With {.Filter = "Access (*.mdb)|*.mdb|Toate fișierele (*.*)|*.*", .Title = "Registrul vechi de mijloace fixe"}
                If k_dialog.ShowDialog(FindForm()) = DialogResult.OK Then txtFile.Text = k_dialog.FileName
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.BtnBrowse_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRead_Click(k_sender As Object, e As EventArgs) Handles btnRead.Click
        Try
            If _busy Then Return
            Dim k_path As String = txtFile.Text.Trim().Trim(""""c)
            Await RunAsync("MfImportView.BtnRead_Click",
                Function() As Task
                    _assets = MfAccess.ReadActive(k_path)
                    FillGrid()
                    Dim k_units As Integer = _assets.Select(Function(k_a) k_a.UnitId).Distinct().Count()
                    lblState.Text = _assets.Count.ToString(CultureInfo.InvariantCulture) & " mijloace fixe active citite" &
                        If(k_units > 1, " (atenție: fișierul are " & k_units.ToString(CultureInfo.InvariantCulture) & " unități, verificați lista)", String.Empty) & "."
                    _context.Say(lblState.Text)
                    Return Task.CompletedTask
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.BtnRead_Click", ex)
        End Try
    End Sub

    Private Sub FillGrid()
        _failed.Clear()
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each k_asset As MfAsset In _assets
                Dim k_row As KBotDataRow = grid.AddRow()
                k_row.Tag = k_asset
                k_row("sel") = True
                k_row("code") = k_asset.Code
                k_row("name") = k_asset.Name
                k_row("entry") = k_asset.EntryDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                k_row("value") = k_asset.Value.ToString("N2", CultureInfo.GetCultureInfo("ro-RO"))
                k_row("dnf") = k_asset.Dnf.ToString(CultureInfo.InvariantCulture)
                k_row("df") = k_asset.Df.ToString(CultureInfo.InvariantCulture)
                k_row("cc") = k_asset.Cc
                k_row("verdict") = String.Empty
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Async Sub BtnTargets_Click(k_sender As Object, e As EventArgs) Handles btnTargets.Click
        Try
            If _busy Then Return
            If _context.IsAll Then
                KBotMessage.Show("Alegeți o singură unitate în arbore.", "Import MF", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_db As String = _context.SelectedDb
            Await RunAsync("MfImportView.BtnTargets_Click",
                Async Function() As Task
                    Dim k_result As JsonElement = Await _context.Client.CallRawAsync("vercon-mf-targets", "--db", k_db)
                    _subunits.Clear()
                    _gestiuni.Clear()
                    _accounts.Clear()
                    _accountSubunit.Clear()
                    For Each k_item As JsonElement In k_result.GetProperty("subunits").EnumerateArray()
                        _subunits.Add(New Pick With {.Id = k_item.GetProperty("id").GetInt32(), .Name = k_item.GetProperty("name").GetString()})
                    Next
                    For Each k_item As JsonElement In k_result.GetProperty("gestiuni").EnumerateArray()
                        _gestiuni.Add(New Pick With {.Id = k_item.GetProperty("id").GetInt32(), .Parent = k_item.GetProperty("subunit").GetInt32(),
                                                     .Name = k_item.GetProperty("name").GetString() & " (" & k_item.GetProperty("state").GetString() & ")"})
                    Next
                    For Each k_item As JsonElement In k_result.GetProperty("accounts").EnumerateArray()
                        Dim k_id As Integer = k_item.GetProperty("id").GetInt32()
                        _accounts.Add(New Pick With {.Id = k_id, .Parent = k_item.GetProperty("gestiune").GetInt32(), .Name = k_item.GetProperty("name").GetString()})
                        _accountSubunit(k_id) = k_item.GetProperty("subunit").GetInt32()
                    Next
                    _targetsDb = k_result.GetProperty("unit").GetString()
                    cmbSubunit.Items.Clear()
                    For Each k_pick As Pick In _subunits
                        cmbSubunit.Items.Add(k_pick.Name)
                    Next
                    FillGestiuni()
                    lblState.Text = "Unitatea " & _targetsDb & ": " & _subunits.Count.ToString(CultureInfo.InvariantCulture) & " subunități, " &
                        _gestiuni.Count.ToString(CultureInfo.InvariantCulture) & " gestiuni MF."
                    _context.Say(lblState.Text)
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.BtnTargets_Click", ex)
        End Try
    End Sub

    Private Sub CmbSubunit_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbSubunit.SelectedIndexChanged
        Try
            FillGestiuni()
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.CmbSubunit_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub CmbGestiune_SelectedIndexChanged(k_sender As Object, e As EventArgs) Handles cmbGestiune.SelectedIndexChanged
        Try
            FillAccounts()
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.CmbGestiune_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Function ChosenGestiuni() As List(Of Pick)
        If cmbSubunit.SelectedIndex < 0 OrElse cmbSubunit.SelectedIndex >= _subunits.Count Then Return New List(Of Pick)()
        Dim k_subunit As Integer = _subunits(cmbSubunit.SelectedIndex).Id
        Return _gestiuni.Where(Function(k_g) k_g.Parent = k_subunit).ToList()
    End Function

    Private Sub FillGestiuni()
        cmbGestiune.Items.Clear()
        For Each k_pick As Pick In ChosenGestiuni()
            cmbGestiune.Items.Add(k_pick.Name)
        Next
        If cmbGestiune.Items.Count > 0 Then cmbGestiune.SelectedIndex = 0
        FillAccounts()
    End Sub

    Private Function ChosenAccounts() As List(Of Pick)
        Dim k_gestiuni As List(Of Pick) = ChosenGestiuni()
        If cmbGestiune.SelectedIndex < 0 OrElse cmbGestiune.SelectedIndex >= k_gestiuni.Count Then Return New List(Of Pick)()
        Dim k_gestiune As Integer = k_gestiuni(cmbGestiune.SelectedIndex).Id
        Dim k_subunit As Integer = _subunits(cmbSubunit.SelectedIndex).Id
        Return _accounts.Where(Function(k_a) k_a.Parent = k_gestiune AndAlso _accountSubunit(k_a.Id) = k_subunit).ToList()
    End Function

    Private Sub FillAccounts()
        cmbAccount.Items.Clear()
        For Each k_pick As Pick In ChosenAccounts()
            cmbAccount.Items.Add(k_pick.Name)
        Next
        If cmbAccount.Items.Count > 0 Then cmbAccount.SelectedIndex = 0
    End Sub

    ''' <summary>The server arguments of the chosen subunit / gestiune / account, or Nothing (with a message) when one is missing.</summary>
    Private Function ChosenTarget() As String()
        If _targetsDb = String.Empty OrElse Not String.Equals(_targetsDb, _context.SelectedDb, StringComparison.OrdinalIgnoreCase) Then
            KBotMessage.Show("Alegeți unitatea în arbore și apăsați «Citește subunitățile».", "Import MF", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Dim k_gestiuni As List(Of Pick) = ChosenGestiuni()
        Dim k_accounts As List(Of Pick) = ChosenAccounts()
        If cmbSubunit.SelectedIndex < 0 OrElse cmbGestiune.SelectedIndex < 0 OrElse cmbAccount.SelectedIndex < 0 Then
            KBotMessage.Show("Alegeți subunitatea, gestiunea și contul de magazie.", "Import MF", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Return New String() {"--db", _targetsDb,
                             "--subunit", _subunits(cmbSubunit.SelectedIndex).Id.ToString(CultureInfo.InvariantCulture),
                             "--gestiune", k_gestiuni(cmbGestiune.SelectedIndex).Id.ToString(CultureInfo.InvariantCulture),
                             "--account", k_accounts(cmbAccount.SelectedIndex).Id.ToString(CultureInfo.InvariantCulture)}
    End Function

    ''' <summary>The ticked rows as the server's JSON (base64); <paramref name="k_rows"/> gets the grid row of each element.</summary>
    Private Function Payload(k_rows As List(Of Integer)) As String
        Dim k_array As New JsonArray()
        For k_i As Integer = 0 To grid.Rows.Count - 1
            If Not Convert.ToBoolean(grid("sel", k_i), CultureInfo.InvariantCulture) Then Continue For
            Dim k_asset As MfAsset = DirectCast(grid.Rows(k_i).Tag, MfAsset)
            k_array.Add(New JsonObject From {
                {"code", k_asset.Code},
                {"name", k_asset.Name},
                {"entry_date", k_asset.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)},
                {"value", k_asset.Value.ToString("0.####", CultureInfo.InvariantCulture)},
                {"dnf", k_asset.Dnf},
                {"df", k_asset.Df},
                {"cc", k_asset.Cc}})
            k_rows.Add(k_i)
        Next
        If k_rows.Count = 0 Then Throw New AdminException("Bifați cel puțin un mijloc fix.")
        Dim k_b64 As String = Convert.ToBase64String(Encoding.UTF8.GetBytes(k_array.ToJsonString()))
        If k_b64.Length > 30000 Then
            Throw New AdminException("Lista bifată este prea mare pentru o singură trimitere. Bifați mai puține rânduri.")
        End If
        Return k_b64
    End Function

    Private Async Sub BtnCheck_Click(k_sender As Object, e As EventArgs) Handles btnCheck.Click
        Try
            If _busy Then Return
            Dim k_target As String() = ChosenTarget()
            If k_target Is Nothing Then Return
            Await RunAsync("MfImportView.BtnCheck_Click", Function() CheckAsync(k_target))
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.BtnCheck_Click", ex)
        End Try
    End Sub

    ''' <summary>Asks the server which ticked rows it would accept, paints the verdicts, returns how many were refused.</summary>
    Private Async Function CheckAsync(k_target As String()) As Task(Of Integer)
        Dim k_rows As New List(Of Integer)()
        Dim k_args As New List(Of String)(k_target)
        k_args.Add("--payload")
        k_args.Add(Payload(k_rows))
        Dim k_result As JsonElement = Await _context.Client.CallRawAsync("vercon-mf-check", k_args.ToArray())
        _failed.Clear()
        Dim k_index As Integer = 0
        For Each k_item As JsonElement In k_result.GetProperty("assets").EnumerateArray()
            Dim k_row As Integer = k_rows(k_index)
            k_index += 1
            Dim k_ok As Boolean = k_item.GetProperty("ok").GetBoolean()
            Dim k_reasons As New List(Of String)()
            For Each k_reason As JsonElement In k_item.GetProperty("reasons").EnumerateArray()
                k_reasons.Add(k_reason.GetString())
            Next
            grid("verdict", k_row) = If(k_ok, "Se poate importa.", String.Join(" ", k_reasons))
            If Not k_ok Then _failed.Add(DirectCast(grid.Rows(k_row).Tag, MfAsset))
        Next
        grid.Invalidate()
        Dim k_warning As String = k_result.GetProperty("warning").GetString()
        lblState.Text = (k_rows.Count - _failed.Count).ToString(CultureInfo.InvariantCulture) & " din " & k_rows.Count.ToString(CultureInfo.InvariantCulture) &
            " se pot importa în «" & k_result.GetProperty("gestiune").GetString() & "»." & If(String.IsNullOrEmpty(k_warning), String.Empty, " " & k_warning)
        _context.Say(lblState.Text)
        Return _failed.Count
    End Function

    Private Async Sub BtnImport_Click(k_sender As Object, e As EventArgs) Handles btnImport.Click
        Try
            If _busy Then Return
            Dim k_target As String() = ChosenTarget()
            If k_target Is Nothing Then Return
            Dim k_refused As Integer = -1
            Await RunAsync("MfImportView.BtnImport_Click.Check", Async Function() As Task
                                                                     k_refused = Await CheckAsync(k_target)
                                                                 End Function)
            If k_refused < 0 Then Return
            If k_refused > 0 Then
                KBotMessage.Show(k_refused.ToString(CultureInfo.InvariantCulture) & " rânduri sunt refuzate (roșu, cu motivul în ultima coloană). Debifați-le sau corectați-le; importul este totul sau nimic.",
                                 "Import MF", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim k_count As Integer = 0
            For k_i As Integer = 0 To grid.Rows.Count - 1
                If Convert.ToBoolean(grid("sel", k_i), CultureInfo.InvariantCulture) Then k_count += 1
            Next
            If Not UiRun.Confirm("Se importă " & k_count.ToString(CultureInfo.InvariantCulture) & " mijloace fixe în unitatea " & _targetsDb & ", subunitatea «" &
                                 cmbSubunit.Text & "», gestiunea «" & cmbGestiune.Text & "», contul «" & cmbAccount.Text & "»." & vbLf &
                                 "Fiecare intră ca document «Stoc inițial» la data intrării lui, fără primitor." & vbLf & vbLf & "Continuați?", "Import MF") Then Return
            Await RunAsync("MfImportView.BtnImport_Click.Run",
                Async Function() As Task
                    Dim k_rows As New List(Of Integer)()
                    Dim k_args As New List(Of String)(k_target)
                    k_args.Add("--payload")
                    k_args.Add(Payload(k_rows))
                    Dim k_result As JsonElement = Await _context.Client.CallRawAsync("vercon-mf-import", k_args.ToArray())
                    Dim k_created As Integer = k_result.GetProperty("created").GetArrayLength()
                    For Each k_row As Integer In k_rows
                        grid("sel", k_row) = False
                        grid("verdict", k_row) = "Importat."
                    Next
                    lblState.Text = k_created.ToString(CultureInfo.InvariantCulture) & " mijloace fixe importate în gestiune (documente «Stoc inițial»)."
                    _context.Say(lblState.Text)
                    KBotMessage.Show(lblState.Text & vbLf & "Predați-le din VERCON cu bon de predare pentru a le trece în folosință.", "Import MF", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Function)
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.BtnImport_Click", ex)
        End Try
    End Sub

    Private Sub Grid_RowFormatting(k_sender As Object, e As KBotRowFormattingEventArgs) Handles grid.RowFormatting
        Try
            Dim k_asset As MfAsset = TryCast(e.Row.Tag, MfAsset)
            If k_asset IsNot Nothing AndAlso _failed.Contains(k_asset) Then e.ForeColor = ThemeManager.Current.Palette.ErrorColor
        Catch ex As Exception
            GlobalErrorLog.Write("MfImportView.Grid_RowFormatting", ex)
        End Try
    End Sub

    Private Async Function RunAsync(k_source As String, k_work As Func(Of Task)) As Task
        If _busy Then Return
        _busy = True
        Try
            Await UiRun.Guard(k_source, _context, k_work)
        Finally
            _busy = False
        End Try
    End Function

End Class
