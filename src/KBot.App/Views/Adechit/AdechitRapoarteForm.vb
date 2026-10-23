Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 0123-01 -- the page «ADECHIT Rapoarte» of the ADECHIT ribbon tab: the six reports of the evidence (monthly situation, payment
''' notice, cash register, bank report, account sheet, cancelled documents) made by the SAME server code as the site. The page only chooses what
''' they need (subunit, year, month, group, interval, child); the server builds the report and keeps it for a few minutes, the page shows its PDF and,
''' on request, saves its Excel. When the report is replaced or the page is closed the page tells the server to drop the saved copy (nothing stays
''' on the server). The rights are the user's own ADECHIT roles; a refusal comes back as the server's Romanian text.
''' </summary>
Public Class AdechitRapoarteForm

    Private ReadOnly _api As IAdechitApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _subunits As New List(Of AdechitSubunit)()
    Private _months As New List(Of AdechitMonth)()
    Private _groups As New List(Of AdechitGroup)()
    Private _children As New List(Of AdechitChild)()
    Private _filling As Boolean
    Private _busy As Boolean
    Private _seq As Integer

    ' The report on screen (Nothing before the first one): the saved copy on the server and the subunit it was made for.
    Private _last As AdechitReportHandle
    Private _lastSubunitId As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The ADECHIT routes; the shell passes its API client.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_unitName">The open unit's name, shown in the title.</param>
    Public Sub New(k_api As IAdechitApi, k_gate As ReauthGate, k_unitName As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        capBar.Text = "ADECHIT — Rapoarte" & If(String.IsNullOrWhiteSpace(k_unitName), String.Empty, " — " & k_unitName)
        cboSub.CaptionSelector = Function(k_item) CType(k_item, AdechitSubunit).Name
        cboAn.CaptionSelector = Function(k_item) Convert.ToString(k_item, CultureInfo.InvariantCulture)
        cboLuna.CaptionSelector = Function(k_item) MonthCaption(CType(k_item, AdechitMonth))
        cboGrupa.CaptionSelector = Function(k_item) GroupCaption(CType(k_item, AdechitGroup))
        cboCopil.CaptionSelector = Function(k_item) ChildCaption(CType(k_item, AdechitChild))
        previewPdf.UserChoosesSave = True   ' Adobe's Save asks whether and where, like on the received invoices
        previewPdf.ShowNotice("Alegeți un raport din stânga.")
    End Sub

    ' ── Captions ────────────────────────────────────────────────────────────────

    Private Shared Function MonthCaption(k_month As AdechitMonth) As String
        Dim k_name As String = If(String.IsNullOrWhiteSpace(k_month.MonthName), k_month.MonthNumber.ToString(CultureInfo.InvariantCulture), k_month.MonthName)
        Return Char.ToUpper(k_name(0), CultureInfo.CurrentCulture) & k_name.Substring(1) & If(k_month.Closed, " (închisă)", String.Empty)
    End Function

    ' The group names are shown without the word «Grupa», trimmed and in capitals, like on the site.
    Private Shared Function GroupCaption(k_group As AdechitGroup) As String
        Dim k_name As String = If(k_group.Name, String.Empty).Trim()
        If k_name.StartsWith("grupa ", StringComparison.CurrentCultureIgnoreCase) Then k_name = k_name.Substring(6).Trim()
        Return k_name.ToUpper(CultureInfo.CurrentCulture) & If(k_group.Hidden, " (ascunsă)", String.Empty)
    End Function

    Private Shared Function ChildCaption(k_child As AdechitChild) As String
        Return k_child.Name & If(String.IsNullOrEmpty(k_child.GroupName), String.Empty, " — " & k_child.GroupName) &
               If(String.IsNullOrEmpty(k_child.CnpTail), String.Empty, " (CNP …" & k_child.CnpTail & ")")
    End Function

    ' ── What the page has chosen ────────────────────────────────────────────────

    Private Function SelectedSubunit() As AdechitSubunit
        Return TryCast(cboSub.SelectedItem, AdechitSubunit)
    End Function

    Private Function SelectedMonth() As AdechitMonth
        Return TryCast(cboLuna.SelectedItem, AdechitMonth)
    End Function

    Private Function SelectedGroup() As AdechitGroup
        Return TryCast(cboGrupa.SelectedItem, AdechitGroup)
    End Function

    Private Function SelectedChild() As AdechitChild
        Return TryCast(cboCopil.SelectedItem, AdechitChild)
    End Function

    Private Shared Sub Fill(Of T)(k_combo As KBotComboBox, k_items As IEnumerable(Of T))
        k_combo.BeginUpdate()
        Try
            k_combo.Items.Clear()
            For Each k_item As T In k_items
                k_combo.Items.Add(k_item)
            Next
        Finally
            k_combo.EndUpdate()
        End Try
    End Sub

    ' ── Loading, level by level: subunit > year > months > groups ──────────────

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub AdechitRapoarteForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            SetBusy(True)
            Dim k_context As AdechitContext = Await _gate.RunAsync(Function() _api.GetAdechitContextAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _subunits = k_context.Subunits
            If _subunits.Count = 0 Then
                ShowNotice("Unitatea nu are nicio subunitate ADECHIT la care să ai acces.", NoticeKind.Warning)
                SetBusy(False)
                Return
            End If
            _filling = True
            Fill(cboSub, _subunits)
            cboSub.SelectedIndex = 0
            cboSub.Enabled = _subunits.Count > 1
            _filling = False
            Await LoadYearsAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("AdechitRapoarteForm.Load", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.Load", ex)
            ShowNotice("Datele ADECHIT nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            _filling = False
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    Private Async Function LoadYearsAsync() As Task
        Dim k_sub As AdechitSubunit = SelectedSubunit()
        Dim k_mySeq As Integer = Interlocked.Increment(_seq)
        ClearBelowSubunit()
        If k_sub Is Nothing Then Return
        Dim k_years As List(Of Integer) = Await _gate.RunAsync(Function() _api.GetAdechitYearsAsync(k_sub.Id, _cts.Token)).ConfigureAwait(True)
        If IsDisposed OrElse k_mySeq <> _seq Then Return
        If k_years.Count = 0 Then
            ShowNotice("Subunitatea nu are încă luni în evidență.", NoticeKind.Warning)
            Return
        End If
        _filling = True
        Fill(cboAn, k_years)
        cboAn.SelectedIndex = 0
        cboAn.Enabled = True
        _filling = False
        Await LoadMonthsAsync().ConfigureAwait(True)
    End Function

    Private Async Function LoadMonthsAsync() As Task
        Dim k_sub As AdechitSubunit = SelectedSubunit()
        Dim k_mySeq As Integer = Interlocked.Increment(_seq)
        ClearBelowYear()
        If k_sub Is Nothing OrElse cboAn.SelectedItem Is Nothing Then Return
        Dim k_year As Integer = Convert.ToInt32(cboAn.SelectedItem, CultureInfo.InvariantCulture)
        _months = Await _gate.RunAsync(Function() _api.GetAdechitMonthsAsync(k_sub.Id, k_year, _cts.Token)).ConfigureAwait(True)
        If IsDisposed OrElse k_mySeq <> _seq Then Return
        _filling = True
        Fill(cboLuna, _months)
        If _months.Count > 0 Then cboLuna.SelectedIndex = 0
        cboLuna.Enabled = _months.Count > 0
        _filling = False
        Await MonthChosenAsync().ConfigureAwait(True)
    End Function

    ' A month is the key of everything else: the groups it had, the default interval, the child search.
    Private Async Function MonthChosenAsync() As Task
        Dim k_sub As AdechitSubunit = SelectedSubunit()
        Dim k_month As AdechitMonth = SelectedMonth()
        Dim k_mySeq As Integer = Interlocked.Increment(_seq)
        ClearBelowMonth()
        If k_sub Is Nothing OrElse k_month Is Nothing Then
            UpdateButtons()
            Return
        End If
        ' The cash and bank reports start on the whole month on screen, like on the site.
        Dim k_first As New Date(k_month.Year, Math.Max(1, Math.Min(12, k_month.MonthNumber)), 1)
        dtpDeLa.Value = k_first
        dtpPana.Value = k_first.AddMonths(1).AddDays(-1)
        txtCauta.Enabled = True
        chkToti.Enabled = True
        _groups = Await _gate.RunAsync(Function() _api.GetAdechitGroupsAsync(k_sub.Id, k_month.Id, _cts.Token)).ConfigureAwait(True)
        If IsDisposed OrElse k_mySeq <> _seq Then Return
        _filling = True
        Fill(cboGrupa, _groups)
        cboGrupa.Enabled = _groups.Count > 0
        _filling = False
        UpdateButtons()
    End Function

    Private Sub ClearBelowSubunit()
        _filling = True
        cboAn.Items.Clear()
        cboAn.Enabled = False
        _filling = False
        ClearBelowYear()
    End Sub

    Private Sub ClearBelowYear()
        _filling = True
        cboLuna.Items.Clear()
        cboLuna.Enabled = False
        _months = New List(Of AdechitMonth)()
        _filling = False
        ClearBelowMonth()
    End Sub

    Private Sub ClearBelowMonth()
        _filling = True
        cboGrupa.Items.Clear()
        cboGrupa.Enabled = False
        cboCopil.Items.Clear()
        cboCopil.Enabled = False
        _groups = New List(Of AdechitGroup)()
        _children = New List(Of AdechitChild)()
        txtCauta.Text = String.Empty
        txtCauta.Enabled = False
        chkToti.Checked = False
        chkToti.Enabled = False
        _filling = False
    End Sub

    ' ── Choices ─────────────────────────────────────────────────────────────────

    ' UI boundary (event of a combo): log, tell the operator, swallow.
    Private Async Sub CboSub_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSub.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed Then Return
            Await RunLoad(AddressOf LoadYearsAsync).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.CboSub_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CboAn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAn.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed Then Return
            Await RunLoad(AddressOf LoadMonthsAsync).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.CboAn_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CboLuna_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboLuna.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed Then Return
            Await RunLoad(AddressOf MonthChosenAsync).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.CboLuna_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' A load started by the operator's choice: the page is busy while it runs and a failure becomes a notice.
    Private Async Function RunLoad(k_load As Func(Of Task)) As Task
        Try
            SetBusy(True)
            Await k_load().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("AdechitRapoarteForm.RunLoad", ex)
            If Not IsDisposed Then ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.RunLoad", ex)
            If Not IsDisposed Then ShowNotice("Datele nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    Private Sub Chk_CheckedChanged(sender As Object, e As EventArgs) Handles chkToate.CheckedChanged, chkToti.CheckedChanged, cboGrupa.SelectedIndexChanged, cboCopil.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed Then Return
            chkSepara.Enabled = chkToate.Checked
            If Not chkToate.Checked Then chkSepara.Checked = False
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.Chk_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub Dtp_ValueChanged(sender As Object, e As EventArgs) Handles dtpDeLa.ValueChanged, dtpPana.ValueChanged
        Try
            If _filling OrElse IsDisposed Then Return
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.Dtp_ValueChanged", ex)
        End Try
    End Sub

    ' The search runs when the operator stops typing (the timer) or presses Enter: every call waits for the evidence's lock, so not one per key.
    Private Sub TxtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        Try
            If _filling OrElse IsDisposed Then Return
            tmrCauta.Stop()
            If txtCauta.Text.Trim().Length >= 3 Then tmrCauta.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.TxtCauta_TextChanged", ex)
        End Try
    End Sub

    Private Async Sub TmrCauta_Tick(sender As Object, e As EventArgs) Handles tmrCauta.Tick
        Try
            tmrCauta.Stop()
            If IsDisposed OrElse _busy Then Return
            Await SearchChildrenAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.TmrCauta_Tick", ex)
        End Try
    End Sub

    Private Async Sub TxtCauta_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtCauta.FieldKeyDown
        Try
            If e.KeyCode <> Keys.Enter Then Return
            e.Handled = True
            e.SuppressKeyPress = True
            tmrCauta.Stop()
            Await SearchChildrenAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.TxtCauta_FieldKeyDown", ex)
        End Try
    End Sub

    Private Async Function SearchChildrenAsync() As Task
        Dim k_sub As AdechitSubunit = SelectedSubunit()
        Dim k_month As AdechitMonth = SelectedMonth()
        If k_sub Is Nothing OrElse k_month Is Nothing Then Return
        Dim k_text As String = txtCauta.Text.Trim()
        If k_text.Length < 3 Then
            ShowNotice("Scrie cel puțin 3 caractere din numele sau CNP-ul copilului.", NoticeKind.Warning)
            Return
        End If
        Try
            SetBusy(True)
            Dim k_found As AdechitChildSearch = Await _gate.RunAsync(
                Function() _api.SearchAdechitChildrenAsync(k_sub.Id, k_month.Id, k_text, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _children = k_found.Rows
            _filling = True
            Fill(cboCopil, _children)
            cboCopil.Enabled = _children.Count > 0
            If _children.Count > 0 Then cboCopil.SelectedIndex = 0
            _filling = False
            ShowNotice(If(_children.Count = 0, "Niciun copil găsit în luna aleasă.",
                          If(k_found.HasMore, "Sunt mai mult de 100 de rezultate; restrânge căutarea.",
                             $"{_children.Count} copii găsiți.")),
                       If(_children.Count = 0 OrElse k_found.HasMore, NoticeKind.Warning, NoticeKind.Success))
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("AdechitRapoarteForm.SearchChildrenAsync", ex)
            If Not IsDisposed Then ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.SearchChildrenAsync", ex)
            If Not IsDisposed Then ShowNotice("Căutarea nu a putut fi făcută. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            _filling = False
            If Not IsDisposed Then
                SetBusy(False)
                UpdateButtons()
            End If
        End Try
    End Function

    ' ── Which report can be asked for now ───────────────────────────────────────

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        busyBar.Running = k_busy
        UpdateButtons()
    End Sub

    Private Sub UpdateButtons()
        Dim k_month As Boolean = SelectedMonth() IsNot Nothing
        Dim k_groupOrAll As Boolean = chkToate.Checked OrElse SelectedGroup() IsNot Nothing
        Dim k_dates As Boolean = dtpDeLa.HasValue AndAlso dtpPana.HasValue
        Dim k_account As Boolean = If(chkToti.Checked, SelectedGroup() IsNot Nothing, SelectedChild() IsNot Nothing)
        Dim k_free As Boolean = Not _busy AndAlso SelectedSubunit() IsNot Nothing
        btnSituatie.Enabled = k_free AndAlso k_month AndAlso k_groupOrAll
        btnPlata.Enabled = k_free AndAlso k_month AndAlso k_groupOrAll
        btnCasa.Enabled = k_free AndAlso k_dates
        btnBanca.Enabled = k_free AndAlso k_dates
        btnFisa.Enabled = k_free AndAlso k_month AndAlso k_account
        btnAnulate.Enabled = k_free AndAlso k_month
        btnExcel.Enabled = k_free AndAlso _last IsNot Nothing
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind)
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
    End Sub

    ' ── The reports ─────────────────────────────────────────────────────────────

    Private Shared Function Iso(k_date As Date) As String
        Return k_date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    End Function

    Private Shared Function NumText(k_value As Integer) As String
        Return k_value.ToString(CultureInfo.InvariantCulture)
    End Function

    Private Async Sub BtnSituatie_Click(sender As Object, e As EventArgs) Handles btnSituatie.Click
        Try
            Dim k_params As New Dictionary(Of String, String) From {{"month", NumText(SelectedMonth().Id)}}
            AddGroupScope(k_params, chkSepara.Checked)
            Await ShowReportAsync(AdechitReportKind.Situation, k_params).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnSituatie_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnPlata_Click(sender As Object, e As EventArgs) Handles btnPlata.Click
        Try
            Dim k_params As New Dictionary(Of String, String) From {{"month", NumText(SelectedMonth().Id)}}
            AddGroupScope(k_params, False)
            Await ShowReportAsync(AdechitReportKind.PaymentNotice, k_params).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnPlata_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnCasa_Click(sender As Object, e As EventArgs) Handles btnCasa.Click
        Try
            Await ShowReportAsync(AdechitReportKind.Cash, IntervalParams()).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnCasa_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnBanca_Click(sender As Object, e As EventArgs) Handles btnBanca.Click
        Try
            Await ShowReportAsync(AdechitReportKind.Bank, IntervalParams()).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnBanca_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnAnulate_Click(sender As Object, e As EventArgs) Handles btnAnulate.Click
        Try
            Dim k_params As New Dictionary(Of String, String) From {{"month", NumText(SelectedMonth().Id)}}
            Await ShowReportAsync(AdechitReportKind.Cancelled, k_params).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnAnulate_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnFisa_Click(sender As Object, e As EventArgs) Handles btnFisa.Click
        Try
            Dim k_params As New Dictionary(Of String, String) From {{"month", NumText(SelectedMonth().Id)}}
            If chkToti.Checked Then
                k_params("all") = "1"
                k_params("group") = NumText(SelectedGroup().Id)
            Else
                k_params("child") = NumText(SelectedChild().ChildId)
            End If
            Await ShowReportAsync(AdechitReportKind.Account, k_params).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnFisa_Click", ex)
        End Try
    End Sub

    ' «Toate grupele» sends all=1 (and split=1 when asked), else the one group chosen.
    Private Sub AddGroupScope(k_params As Dictionary(Of String, String), k_split As Boolean)
        If chkToate.Checked Then
            k_params("all") = "1"
            If k_split Then k_params("split") = "1"
        Else
            k_params("group") = NumText(SelectedGroup().Id)
        End If
    End Sub

    Private Function IntervalParams() As Dictionary(Of String, String)
        Return New Dictionary(Of String, String) From {{"start", Iso(dtpDeLa.Value)}, {"end", Iso(dtpPana.Value)}}
    End Function

    ''' <summary>
    ''' Asks the server for the report, brings its PDF and shows it. The copy the server kept of the report shown before is released
    ''' only after the new one is on screen. A failure is told on the notice line and leaves what was on screen as it was.
    ''' </summary>
    Private Async Function ShowReportAsync(k_kind As String, k_params As IReadOnlyDictionary(Of String, String)) As Task
        If _busy Then Return
        Dim k_sub As AdechitSubunit = SelectedSubunit()
        If k_sub Is Nothing Then Return
        Dim k_handle As AdechitReportHandle = Nothing
        Try
            SetBusy(True)
            ntfMesaj.Visible = False
            k_handle = Await _gate.RunAsync(Function() _api.GenerateAdechitReportAsync(k_sub.Id, k_kind, k_params, _cts.Token)).ConfigureAwait(True)
            Dim k_bytes As Byte() = Await _gate.RunAsync(
                Function() _api.DownloadAdechitReportAsync(k_sub.Id, k_handle.Snapshot, "pdf", _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then
                Return
            End If
            TempPdfStore.EnsureRoot()
            Dim k_file As String = TempPdfStore.PathFor($"ADECHIT_{SafeName(k_handle.FileName)}_{Guid.NewGuid():N}.pdf")
            File.WriteAllBytes(k_file, k_bytes)
            previewPdf.ShowDocument(k_file, True)
            ReleaseLast()
            _last = k_handle
            _lastSubunitId = k_sub.Id
            k_handle = Nothing
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("AdechitRapoarteForm.ShowReportAsync", ex)
            If Not IsDisposed Then ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.ShowReportAsync", ex)
            If Not IsDisposed Then ShowNotice("Raportul nu a putut fi afișat. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            ' A report the server built but the page could not show is not kept either.
            If k_handle IsNot Nothing Then ReleaseQuietly(k_sub.Id, k_handle.Snapshot)
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    Private Shared Function SafeName(k_name As String) As String
        Dim k_bad As Char() = Path.GetInvalidFileNameChars()
        Dim k_clean As String = New String(If(k_name, "raport").Select(Function(k_c) If(Array.IndexOf(k_bad, k_c) >= 0, "_"c, k_c)).ToArray())
        Return If(k_clean.Length = 0, "raport", k_clean)
    End Function

    ' ── Excel ───────────────────────────────────────────────────────────────────

    Private Async Sub BtnExcel_Click(sender As Object, e As EventArgs) Handles btnExcel.Click
        Try
            If _busy OrElse _last Is Nothing Then Return
            Dim k_handle As AdechitReportHandle = _last
            Dim k_sub As Integer = _lastSubunitId
            Using k_dialog As New SaveFileDialog()
                k_dialog.Title = "Salvează raportul ca Excel"
                k_dialog.Filter = "Excel (*.xlsx)|*.xlsx"
                k_dialog.FileName = SafeName(k_handle.FileName) & ".xlsx"
                k_dialog.OverwritePrompt = True
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                SetBusy(True)
                Try
                    Dim k_bytes As Byte() = Await _gate.RunAsync(
                        Function() _api.DownloadAdechitReportAsync(k_sub, k_handle.Snapshot, "xlsx", _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    File.WriteAllBytes(k_dialog.FileName, k_bytes)
                    ShowNotice("Excelul a fost salvat: " & k_dialog.FileName, NoticeKind.Success)
                Catch ex As OperationCanceledException
                    ' The page was closed.
                Catch ex As ApiException
                    GlobalErrorLog.Write("AdechitRapoarteForm.BtnExcel_Click", ex)
                    If Not IsDisposed Then ShowNotice(ex.Message, NoticeKind.Error)
                Catch ex As IOException
                    GlobalErrorLog.Write("AdechitRapoarteForm.BtnExcel_Click", ex)
                    If Not IsDisposed Then ShowNotice("Fișierul nu a putut fi scris. Este deschis în alt program sau locul nu permite scrierea.", NoticeKind.Error)
                Finally
                    If Not IsDisposed Then SetBusy(False)
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.BtnExcel_Click", ex)
            If Not IsDisposed Then ShowNotice("Excelul nu a putut fi salvat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── Letting the server drop the saved copy ──────────────────────────────────

    ' The report shown before is not needed any more.
    Private Sub ReleaseLast()
        If _last Is Nothing Then Return
        ReleaseQuietly(_lastSubunitId, _last.Snapshot)
        _last = Nothing
    End Sub

    ' Best effort: the server sweeps what is left anyway, so a failure here is logged and nothing more. Not tied to the page's
    ' cancellation: it must still go out when the page is closing.
    Private Async Sub ReleaseQuietly(k_subunitId As Integer, k_snapshot As String)
        Try
            Await _gate.RunAsync(Function() _api.ReleaseAdechitReportAsync(k_subunitId, k_snapshot, CancellationToken.None)).ConfigureAwait(False)
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.ReleaseQuietly", ex)
        End Try
    End Sub

    Private Sub AdechitRapoarteForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
            If _api IsNot Nothing Then ReleaseLast()
        Catch ex As Exception
            GlobalErrorLog.Write("AdechitRapoarteForm.AdechitRapoarteForm_FormClosing", ex)
        End Try
    End Sub

End Class
