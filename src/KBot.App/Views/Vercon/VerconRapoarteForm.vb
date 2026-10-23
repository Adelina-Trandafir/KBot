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
''' Slice 0123-02 -- the page «VERCON Rapoarte» of the VERCON ribbon tab: the reports of the site (balances, sheets, receipts, consumption, food lists,
''' inventory objects, fixed assets) made by the SAME server code, with the SAME choices in the SAME order: subunit, gestiune, year; the period
''' (a list of years with «Tot anul» and the months, or two dates); the options of the report; the report type; the data of the report (a tree with a
''' box on each row, its branches read from the server when opened). The server builds the report and keeps it for a few minutes; the page shows its PDF
''' and, on request, saves its Excel. When the report is replaced or the page is closed the page tells the server to drop the saved copy (nothing stays
''' on the server). The rights are the user's own VERCON roles; a refusal comes back as the server's Romanian text.
''' </summary>
Public Class VerconRapoarteForm

    Private Const NoReport As String = "Alegeți mai întâi tipul raportului."

    Private ReadOnly _api As IVerconApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()

    ' What the server accepted, and what it offers for it.
    Private _ctx As VerconContext
    Private _catalog As VerconCatalog
    Private _reports As New Dictionary(Of String, VerconReportSpec)(StringComparer.Ordinal)
    Private _categories As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private _periodItems As New List(Of VerconPeriodItem)()

    ' The report chosen, the group it belongs to, and the choices that belong to it.
    Private _spec As VerconReportSpec
    Private _category As String
    Private _dateOnly As Boolean
    Private ReadOnly _chosen As New HashSet(Of String)(StringComparer.Ordinal)

    ' The selection tree is asked for with the choices it was made for (the site keeps them for the lazy branches).
    Private _lookupReport As String = String.Empty
    Private _lookupBase As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private _lookupSeq As Integer
    Private _ctxSeq As Integer
    Private _lookupLoading As Boolean
    Private _selectionLoading As Integer
    Private _searchLoading As Boolean
    Private ReadOnly _checking As New HashSet(Of String)(StringComparer.Ordinal)

    Private _filling As Boolean
    Private _busy As Boolean
    Private _loading As Boolean

    ' The report the server holds for this page (Nothing before the first one): valid while no choice changed since it was made.
    Private _handle As VerconReportHandle
    Private _handleSelection As VerconSelection
    Private _handleValid As Boolean
    Private _stateSeq As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The VERCON routes; the shell passes its API client.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_unitName">The open unit's name, shown in the title.</param>
    Public Sub New(k_api As IVerconApi, k_gate As ReauthGate, k_unitName As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        capBar.Text = "VERCON — Rapoarte" & If(String.IsNullOrWhiteSpace(k_unitName), String.Empty, " — " & k_unitName)
        cboSub.CaptionSelector = Function(k_item) CType(k_item, VerconSubunit).Name
        cboGest.CaptionSelector = Function(k_item) CType(k_item, VerconGestiune).Name
        cboAn.CaptionSelector = Function(k_item) Convert.ToString(k_item, CultureInfo.InvariantCulture)
        previewPdf.UserChoosesSave = True   ' Adobe's Save asks whether and where, like on the received invoices
        previewPdf.ShowNotice("Alegeți un raport din listă.")
        lblStare.Text = NoReport
    End Sub

    ' ── Loading: subunit > gestiune > year, then the catalog of the gestiune ─────

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub VerconRapoarteForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            Await RunLoad(Function() ChangeContextAsync(New VerconSelection())).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.Load", ex)
        End Try
    End Sub

    ' A load started by the operator's choice: the page is busy while it runs and a failure becomes a notice.
    Private Async Function RunLoad(k_load As Func(Of Task)) As Task
        Try
            SetLoading(True)
            Await k_load().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("VerconRapoarteForm.RunLoad", ex)
            If Not IsDisposed Then
                ShowNotice(ex.Message, NoticeKind.Error)
                If _ctx IsNot Nothing Then ApplyContext(_ctx)   ' the combos show the accepted choice again
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.RunLoad", ex)
            If Not IsDisposed Then
                ShowNotice("Datele nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
                If _ctx IsNot Nothing Then ApplyContext(_ctx)
            End If
        Finally
            If Not IsDisposed Then SetLoading(False)
        End Try
    End Function

    ' The server resolves the choice (a change above clears everything below it; a single remaining option is taken by the server). Only the
    ' last answer is accepted.
    Private Async Function ChangeContextAsync(k_wanted As VerconSelection) As Task
        Dim k_mySeq As Integer = Interlocked.Increment(_ctxSeq)
        Dim k_ctx As VerconContext = Nothing
        Dim k_changed As Boolean
        Try
            k_ctx = Await _gate.RunAsync(Function() _api.GetVerconContextAsync(k_wanted, _cts.Token)).ConfigureAwait(True)
        Catch ex As ApiException When String.Equals(ex.Reason, "SELECTION_CHANGED", StringComparison.Ordinal)
            ShowNotice(ex.Message, NoticeKind.Warning)
            k_changed = True
        End Try
        ' The choice no longer exists on the server (it changed meanwhile): start again from nothing, like the site.
        If k_changed Then k_ctx = Await _gate.RunAsync(Function() _api.GetVerconContextAsync(New VerconSelection(), _cts.Token)).ConfigureAwait(True)
        If IsDisposed OrElse k_mySeq <> _ctxSeq Then Return
        ApplyContext(k_ctx)
        ' The site waits for the click on the subunit even when there is one; here there is nothing to choose, so it is taken.
        If Not k_ctx.Selection.Subunit.HasValue AndAlso k_ctx.Subunits.Count = 1 Then
            Await ChangeContextAsync(New VerconSelection() With {.Subunit = k_ctx.Subunits(0).Id}).ConfigureAwait(True)
            Return
        End If
        ResetReport()
        If Not k_ctx.Ready Then
            ShowNotice("VERCON este în pregătire pentru această unitate. Nu există încă gestiuni de lucru.", NoticeKind.Warning)
            Return
        End If
        If k_ctx.Subunits.Count = 0 Then
            ShowNotice("Unitatea nu are nicio subunitate VERCON la care să ai acces.", NoticeKind.Warning)
            Return
        End If
        If k_ctx.Selection.Gestiune.HasValue Then Await LoadCatalogAsync(k_mySeq).ConfigureAwait(True)
    End Function

    Private Sub ApplyContext(k_ctx As VerconContext)
        _ctx = k_ctx
        _filling = True
        Try
            Fill(cboSub, k_ctx.Subunits)
            cboSub.SelectedIndex = k_ctx.Subunits.FindIndex(Function(k_s) k_ctx.Selection.Subunit.HasValue AndAlso k_s.Id = k_ctx.Selection.Subunit.Value)
            cboSub.Enabled = k_ctx.Subunits.Count > 1
            Fill(cboGest, k_ctx.Gestiuni)
            cboGest.SelectedIndex = k_ctx.Gestiuni.FindIndex(Function(k_g) k_ctx.Selection.Gestiune.HasValue AndAlso k_g.Id = k_ctx.Selection.Gestiune.Value)
            Dim k_showGest As Boolean = k_ctx.Selection.Subunit.HasValue AndAlso k_ctx.Gestiuni.Count > 1
            rowGest.Visible = k_showGest
            cboGest.Enabled = k_showGest
            Fill(cboAn, k_ctx.Years)
            cboAn.SelectedIndex = k_ctx.Years.FindIndex(Function(k_y) k_ctx.Selection.Year.HasValue AndAlso k_y = k_ctx.Selection.Year.Value)
            ' Fixed assets and inventory objects have no exercise: their period is chosen directly.
            Dim k_showYear As Boolean = k_ctx.Selection.Gestiune.HasValue AndAlso k_ctx.Years.Count > 0 AndAlso
                                        k_ctx.Selection.GestiuneType <> "MF" AndAlso k_ctx.Selection.GestiuneType <> "OI"
            rowAn.Visible = k_showYear
            cboAn.Enabled = k_showYear
        Finally
            _filling = False
        End Try
    End Sub

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

    Private Async Function LoadCatalogAsync(k_ctxSeq As Integer) As Task
        Dim k_catalog As VerconCatalog = Await _gate.RunAsync(Function() _api.GetVerconCatalogAsync(_ctx.Selection.Clone(), _cts.Token)).ConfigureAwait(True)
        If IsDisposed OrElse k_ctxSeq <> _ctxSeq Then Return
        _catalog = k_catalog
        _reports = New Dictionary(Of String, VerconReportSpec)(StringComparer.Ordinal)
        For Each k_spec As VerconReportSpec In k_catalog.Reports
            _reports(k_spec.Id) = k_spec
        Next
        ConfigurePeriods(k_catalog)
        BuildReportTree(k_catalog)
        If k_catalog.Reports.Count = 0 Then ShowNotice("Gestiunea nu are rapoarte disponibile.", NoticeKind.Warning)
    End Function

    ' ── The period ──────────────────────────────────────────────────────────────

    ' The list: «Anul curent până azi» and «TOTI anii» above the years, each year with «Tot anul» and the months of its movements, newest first.
    Private Sub ConfigurePeriods(k_catalog As VerconCatalog)
        _periodItems = VerconPeriods.Build(k_catalog)
        _filling = True
        Try
            cboPer.BeginUpdate()
            Try
                cboPer.Items.Clear()
                For Each k_item As VerconPeriodItem In _periodItems
                    cboPer.Items.Add(New KBotComboGroupedItem(k_item.Text, k_item.Group, k_item.RowText, k_item))
                Next
            Finally
                cboPer.EndUpdate()
            End Try
            cboPer.Enabled = _periodItems.Count > 0
            ' The pickers take the years the gestiune has data in (and the current year), whole.
            Dim k_years As List(Of Integer) = If(k_catalog.Years.Count > 0, k_catalog.Years.ToList(), k_catalog.Periods.Select(Function(k_p) k_p.Year).ToList())
            If k_years.Count > 0 Then
                ApplyPickerBounds(dtpDeLa, New Date(k_years.Min(), 1, 1), New Date(k_years.Max(), 12, 31))
                ApplyPickerBounds(dtpPana, New Date(k_years.Min(), 1, 1), New Date(k_years.Max(), 12, 31))
            End If
            Dim k_default As VerconPeriodItem = _periodItems.FirstOrDefault(Function(k_i) k_i.Text = VerconPeriods.CurrentToToday)
            If k_default Is Nothing Then
                Dim k_latest As String = "Tot anul / " & k_catalog.LatestYear.ToString(CultureInfo.InvariantCulture)
                k_default = _periodItems.FirstOrDefault(Function(k_i) k_i.Text = k_latest)
            End If
            If k_default IsNot Nothing Then
                dtpDeLa.Value = k_default.StartDate
                dtpPana.Value = k_default.EndDate
            End If
            SyncPeriodText()
        Finally
            _filling = False
        End Try
    End Sub

    Private Shared Sub ApplyPickerBounds(k_picker As KBotDatePicker, k_min As Date, k_max As Date)
        ' MinDate may not pass the old MaxDate and the reverse: the one that cannot conflict goes first.
        If k_min > k_picker.MaxDate Then
            k_picker.MaxDate = k_max
            k_picker.MinDate = k_min
        Else
            k_picker.MinDate = k_min
            k_picker.MaxDate = k_max
        End If
    End Sub

    ' The face of the period list says what the two dates are: a row of the list, or «Interval personalizat».
    Private Sub SyncPeriodText()
        If Not dtpDeLa.HasValue OrElse Not dtpPana.HasValue Then Return
        Dim k_was As Boolean = _filling
        _filling = True
        Try
            Dim k_match As VerconPeriodItem = VerconPeriods.Match(_periodItems, dtpDeLa.Value, dtpPana.Value)
            If k_match Is Nothing Then
                cboPer.Text = VerconPeriods.Custom
            Else
                cboPer.Text = k_match.Text
            End If
        Finally
            _filling = k_was
        End Try
    End Sub

    Private Sub CboPer_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboPer.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed Then Return
            Dim k_grouped As KBotComboGroupedItem = TryCast(cboPer.SelectedItem, KBotComboGroupedItem)
            Dim k_item As VerconPeriodItem = TryCast(k_grouped?.Tag, VerconPeriodItem)
            If k_item Is Nothing Then Return
            _filling = True
            Try
                dtpDeLa.Value = k_item.StartDate
                dtpPana.Value = k_item.EndDate
            Finally
                _filling = False
            End Try
            PeriodChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.CboPer_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' What the box shows when it is left must be true: a text that is no row goes back to what the two dates say.
    Private Sub CboPer_Leave(sender As Object, e As EventArgs) Handles cboPer.Leave
        Try
            If IsDisposed OrElse _catalog Is Nothing Then Return
            SyncPeriodText()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.CboPer_Leave", ex)
        End Try
    End Sub

    Private Sub Dtp_ValueChanged(sender As Object, e As EventArgs) Handles dtpDeLa.ValueChanged, dtpPana.ValueChanged
        Try
            If _filling OrElse IsDisposed Then Return
            SyncPeriodText()
            PeriodChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.Dtp_ValueChanged", ex)
        End Try
    End Sub

    ' The data of a report depends on the period: the report on screen is stale and the tree is read again (once the dates stop changing).
    Private Sub PeriodChanged()
        InvalidateReport()
        If _spec Is Nothing Then Return
        tmrLookups.Stop()
        tmrLookups.Start()
    End Sub

    Private Async Sub TmrLookups_Tick(sender As Object, e As EventArgs) Handles tmrLookups.Tick
        Try
            tmrLookups.Stop()
            If IsDisposed Then Return
            Await RunLoad(AddressOf LookupsAsync).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.TmrLookups_Tick", ex)
        End Try
    End Sub

    Private Function PeriodError() As String
        If _dateOnly Then Return If(dtpPana.HasValue, String.Empty, "Completați data raportului.")
        If Not dtpDeLa.HasValue OrElse Not dtpPana.HasValue Then Return "Completați data de început și data de sfârșit."
        Return If(dtpDeLa.Value.Date > dtpPana.Value.Date, "Data de început trebuie să fie înaintea datei de sfârșit.", String.Empty)
    End Function

    ' ── The choices of context ──────────────────────────────────────────────────

    Private Async Sub CboSub_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboSub.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed OrElse _ctx Is Nothing Then Return
            Dim k_sub As VerconSubunit = TryCast(cboSub.SelectedItem, VerconSubunit)
            If k_sub Is Nothing OrElse (_ctx.Selection.Subunit.HasValue AndAlso k_sub.Id = _ctx.Selection.Subunit.Value) Then Return
            Await RunLoad(Function() ChangeContextAsync(New VerconSelection() With {.Subunit = k_sub.Id})).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.CboSub_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CboGest_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboGest.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed OrElse _ctx Is Nothing Then Return
            Dim k_gest As VerconGestiune = TryCast(cboGest.SelectedItem, VerconGestiune)
            If k_gest Is Nothing OrElse (_ctx.Selection.Gestiune.HasValue AndAlso k_gest.Id = _ctx.Selection.Gestiune.Value) Then Return
            Await RunLoad(Function() ChangeContextAsync(New VerconSelection() With {.Subunit = _ctx.Selection.Subunit, .Gestiune = k_gest.Id})).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.CboGest_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CboAn_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboAn.SelectedIndexChanged
        Try
            If _filling OrElse IsDisposed OrElse _ctx Is Nothing OrElse cboAn.SelectedItem Is Nothing Then Return
            Dim k_year As Integer = Convert.ToInt32(cboAn.SelectedItem, CultureInfo.InvariantCulture)
            If _ctx.Selection.Year.HasValue AndAlso k_year = _ctx.Selection.Year.Value Then Return
            Await RunLoad(Function() ChangeContextAsync(New VerconSelection() With {
                .Subunit = _ctx.Selection.Subunit, .Gestiune = _ctx.Selection.Gestiune, .Year = k_year})).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.CboAn_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' ── The report types ────────────────────────────────────────────────────────

    ' Everything that belongs to the report chosen is forgotten (a new gestiune or year): the report on screen is released first.
    Private Sub ResetReport()
        ReleaseLast()
        _spec = Nothing
        _category = Nothing
        _dateOnly = False
        _catalog = Nothing
        _reports = New Dictionary(Of String, VerconReportSpec)(StringComparer.Ordinal)
        _periodItems = New List(Of VerconPeriodItem)()
        treeRapoarte.Clear()
        ClearSelectionTree()
        _filling = True
        Try
            cboPer.Items.Clear()
            cboPer.Enabled = False
            cboPer.Text = String.Empty
        Finally
            _filling = False
        End Try
        ShowOptions(Nothing)
        previewPdf.ShowNotice("Alegeți un raport din listă.")
        InvalidateReport()
    End Sub

    Private Sub BuildReportTree(k_catalog As VerconCatalog)
        treeRapoarte.Clear()
        Dim k_nodes As List(Of VerconTreeNode) = VerconReportTree.Build(k_catalog.Reports)
        _categories = VerconReportTree.CategoriesOf(k_nodes)
        For Each k_node As VerconTreeNode In k_nodes
            AddReportNode(k_node, Nothing)
        Next
        ' Like the site: the first group is open.
        If treeRapoarte.Items.Count > 0 Then treeRapoarte.Items(0).Expanded = True
        treeRapoarte.Invalidate()
    End Sub

    Private Sub AddReportNode(k_node As VerconTreeNode, k_parent As AdvancedTreeControl.TreeItem)
        Dim k_item As AdvancedTreeControl.TreeItem = treeRapoarte.AddItem(k_node.Id, k_node.Label, k_parent)
        k_item.Bold = k_parent Is Nothing
        For Each k_child As VerconTreeNode In k_node.Children
            AddReportNode(k_child, k_item)
        Next
    End Sub

    ' A press on a report chooses it; a press on a group opens or closes it (and, at the top, closes the other groups, like the site).
    Private Async Sub TreeRapoarte_NodeMouseDown(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles treeRapoarte.NodeMouseDown
        Try
            If k_node Is Nothing OrElse e.Button <> MouseButtons.Left Then Return
            If k_node.Key.StartsWith(VerconReportTree.ReportPrefix, StringComparison.Ordinal) Then
                Await RunLoad(Function() SelectReportAsync(k_node.Key.Substring(VerconReportTree.ReportPrefix.Length))).ConfigureAwait(True)
                Return
            End If
            If k_node.Children.Count = 0 Then Return
            Dim k_open As Boolean = Not k_node.Expanded
            If k_node.Level = 0 AndAlso k_open Then
                For Each k_other As AdvancedTreeControl.TreeItem In treeRapoarte.Items
                    If Not ReferenceEquals(k_other, k_node) Then k_other.Expanded = False
                Next
            End If
            k_node.Expanded = k_open
            treeRapoarte.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.TreeRapoarte_NodeMouseDown", ex)
        End Try
    End Sub

    Private Async Function SelectReportAsync(k_id As String) As Task
        Dim k_report As VerconReportSpec = Nothing
        If Not _reports.TryGetValue(k_id, k_report) Then Return
        If _spec IsNot Nothing AndAlso _spec.Id = k_id AndAlso (treeSel.Items.Count > 0 OrElse _lookupLoading) Then Return
        Dim k_category As String = Nothing
        _categories.TryGetValue(k_id, k_category)
        Dim k_same As Boolean = k_category IsNot Nothing AndAlso _spec IsNot Nothing AndAlso k_category = _category AndAlso
                                _spec.MfSelection.Length = 0 AndAlso k_report.MfSelection.Length = 0
        ShowOptions(k_report)
        _spec = k_report
        _category = k_category
        InvalidateReport()
        ' Two reports of one group share the tree of data already read (the site keeps it); the fixed-asset reports each have their own.
        If k_same AndAlso (treeSel.Items.Count > 0 OrElse _lookupLoading) Then
            UpdateButtons()
            Return
        End If
        _chosen.Clear()
        Await LookupsAsync().ConfigureAwait(True)
    End Function

    ' The rows of options and the period rows that belong to the report.
    Private Sub ShowOptions(k_report As VerconReportSpec)
        Dim k_balance As Boolean = k_report IsNot Nothing AndAlso k_report.Kind = "balance"
        rowGrupeaza.Visible = k_balance
        rowFara.Visible = k_balance
        rowDocumente.Visible = k_report IsNot Nothing AndAlso k_report.DocumentOptions
        rowPrimitori.Visible = k_report IsNot Nothing AndAlso k_report.ReceiverOptions
        rowGrupPrim.Visible = k_report IsNot Nothing AndAlso k_report.MfSelection = "depreciation"
        _dateOnly = k_report IsNot Nothing AndAlso k_report.Kind = "depreciation"
        lblPerTitlu.Text = If(_dateOnly, "Selectează data", "Selectează perioada")
        rowPer.Visible = Not _dateOnly
        rowDeLa.Visible = Not _dateOnly
        lblPana.Text = If(_dateOnly, "La data", "Dată sfârșit")
    End Sub

    Private Sub ChkOption_CheckedChanged(sender As Object, e As EventArgs) Handles chkGrupeaza.CheckedChanged, chkFara.CheckedChanged
        Try
            If _filling OrElse IsDisposed Then Return
            InvalidateReport()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.ChkOption_CheckedChanged", ex)
        End Try
    End Sub

    ' Documents / receivers / grouping by receivers change the SHAPE of the tree: it is read again with nothing ticked.
    Private Async Sub ChkShape_CheckedChanged(sender As Object, e As EventArgs) Handles chkDocumente.CheckedChanged, chkPrimitori.CheckedChanged, chkGrupPrim.CheckedChanged
        Try
            If _filling OrElse IsDisposed OrElse _spec Is Nothing Then Return
            Dim k_applies As Boolean = (sender Is chkDocumente AndAlso _spec.DocumentOptions) OrElse
                                       (sender Is chkPrimitori AndAlso _spec.ReceiverOptions) OrElse
                                       (sender Is chkGrupPrim AndAlso _spec.MfSelection = "depreciation")
            If Not k_applies Then Return
            _chosen.Clear()
            InvalidateReport()
            Await RunLoad(AddressOf LookupsAsync).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.ChkShape_CheckedChanged", ex)
        End Try
    End Sub

    ' ── What the report is asked with ───────────────────────────────────────────

    Private Shared Function Iso(k_date As Date) As String
        Return k_date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    End Function

    ' The parameters of a call: the period, the options, and (withFilters) the ticked data as filters.
    Private Function BuildQuery(k_withFilters As Boolean) As Dictionary(Of String, String)
        Dim k_result As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
            {"start", Iso(If(_dateOnly, dtpPana.Value, dtpDeLa.Value))}, {"end", Iso(dtpPana.Value)}}
        If _spec IsNot Nothing AndAlso _spec.MfSelection = "depreciation" Then
            k_result("group_by_receivers") = If(chkGrupPrim.Checked, "1", "0")
        End If
        If k_withFilters AndAlso _spec IsNot Nothing Then
            If _spec.Kind = "balance" Then
                k_result("page_break_accounts") = If(chkGrupeaza.Checked, "1", "0")
                k_result("show_without_movements") = If(chkFara.Checked, "1", "0")
            End If
            For Each k_pair As KeyValuePair(Of String, String) In VerconSelectionLogic.Filters(_chosen, _spec, AllTicked())
                k_result(k_pair.Key) = k_pair.Value
            Next
        End If
        Return k_result
    End Function

    ' ── The selection tree ──────────────────────────────────────────────────────

    Private Sub ClearSelectionTree()
        Interlocked.Increment(_lookupSeq)
        treeSel.Clear()
        _chosen.Clear()
        _lookupLoading = False
        _filling = True
        Try
            chkToate.CheckState = CheckState.Unchecked
            chkToate.Enabled = False
        Finally
            _filling = False
        End Try
    End Sub

    ' Reads the root of the tree for the report chosen, with the period and options as they are NOW.
    Private Async Function LookupsAsync() As Task
        If _spec Is Nothing OrElse _ctx Is Nothing Then Return
        Dim k_name As String = _spec.Id
        ' A change of period keeps the ticks that are still there (the site reads the roots that hold them again); a new report or a new
        ' shape of the tree cleared _chosen before getting here.
        Dim k_keep As List(Of String) = _chosen.ToList()
        ClearSelectionTree()
        Dim k_mySeq As Integer = _lookupSeq
        If PeriodError().Length > 0 Then
            UpdateButtons()
            Return
        End If
        _lookupLoading = True
        UpdateButtons()
        Try
            Dim k_base As Dictionary(Of String, String) = BuildQuery(False)
            If _spec.DocumentOptions AndAlso chkDocumente.Checked Then
                k_base("selection_mode") = "documents"
            ElseIf _spec.OutgoingSelection AndAlso _spec.ReceiverOptions AndAlso chkPrimitori.Checked Then
                k_base("selection_mode") = "receivers"
            End If
            _lookupReport = k_name
            _lookupBase = k_base
            Dim k_first As New Dictionary(Of String, String)(k_base, StringComparer.Ordinal) From {{"branch", "root"}}
            Dim k_branch As VerconBranch = Await _gate.RunAsync(
                Function() _api.GetVerconSelectionAsync(_ctx.Selection.Clone(), k_name, k_first, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _lookupSeq Then Return
            For Each k_node As VerconSelNode In k_branch.Nodes
                AddSelNode(k_node, Nothing)
            Next
            SetBoxes()
            treeSel.Invalidate()
            chkToate.Enabled = treeSel.Items.Count > 0
            If k_keep.Count > 0 Then
                _chosen.UnionWith(k_keep)
                For Each k_root As AdvancedTreeControl.TreeItem In treeSel.Items.ToList()
                    Dim k_prefix As String = VerconSelectionLogic.PrefixOf(k_root.Key)
                    If k_keep.Any(Function(k_id) k_id.StartsWith(k_prefix, StringComparison.Ordinal)) AndAlso HasLazy(k_root) Then
                        Await LoadBranchAsync(k_root, True).ConfigureAwait(True)
                        If IsDisposed OrElse k_mySeq <> _lookupSeq Then Return
                    End If
                Next
                ' Only the rows that exist for the new period stay ticked.
                Dim k_valid As HashSet(Of String) = AllLeaves().Select(Function(k_l) k_l.Key).ToHashSet(StringComparer.Ordinal)
                _chosen.IntersectWith(k_valid)
                ReapplyTicks()
                TicksChanged()
            End If
        Finally
            If k_mySeq = _lookupSeq Then _lookupLoading = False
            If Not IsDisposed Then UpdateButtons()
        End Try
    End Function

    Private Sub AddSelNode(k_node As VerconSelNode, k_parent As AdvancedTreeControl.TreeItem)
        Dim k_item As AdvancedTreeControl.TreeItem = treeSel.AddItem(k_node.Id, k_node.Label, k_parent, pLazyNode:=k_node.Lazy AndAlso k_node.Children.Count = 0)
        For Each k_child As VerconSelNode In k_node.Children
            AddSelNode(k_child, k_item)
        Next
    End Sub

    ' Which rows carry a box: the data rows, the branches that speak for what is under them, and the branches below the top level that hold data rows.
    Private Sub SetBoxes()
        For Each k_item As AdvancedTreeControl.TreeItem In treeSel.Items
            SetBoxesOf(k_item)
        Next
    End Sub

    Private Shared Sub SetBoxesOf(k_item As AdvancedTreeControl.TreeItem)
        Dim k_kids As List(Of AdvancedTreeControl.TreeItem) = k_item.Children.Where(Function(k_c) Not k_c.IsLoader).ToList()
        For Each k_kid As AdvancedTreeControl.TreeItem In k_kids
            SetBoxesOf(k_kid)
        Next
        k_item.HasCheckBox = VerconSelectionLogic.IsLeaf(k_item.Key) OrElse
                             (VerconSelectionLogic.IsBoxedBranch(k_item.Key) AndAlso (k_item.LazyNode OrElse k_kids.Count > 0)) OrElse
                             (k_kids.Count > 0 AndAlso k_item.Level >= 1 AndAlso k_kids.Any(Function(k_c) k_c.HasCheckBox))
    End Sub

    Private Shared Function HasLazy(k_item As AdvancedTreeControl.TreeItem) As Boolean
        Return k_item.LazyNode OrElse k_item.Children.Any(Function(k_c) Not k_c.IsLoader AndAlso HasLazy(k_c))
    End Function

    Private Function AnyLazy() As Boolean
        Return treeSel.Items.Any(Function(k_i) HasLazy(k_i))
    End Function

    Private Shared Sub CollectLeaves(k_item As AdvancedTreeControl.TreeItem, k_into As List(Of AdvancedTreeControl.TreeItem))
        If VerconSelectionLogic.IsLeaf(k_item.Key) Then k_into.Add(k_item)
        For Each k_kid As AdvancedTreeControl.TreeItem In k_item.Children
            If Not k_kid.IsLoader Then CollectLeaves(k_kid, k_into)
        Next
    End Sub

    Private Function AllLeaves() As List(Of AdvancedTreeControl.TreeItem)
        Dim k_leaves As New List(Of AdvancedTreeControl.TreeItem)()
        For Each k_item As AdvancedTreeControl.TreeItem In treeSel.Items
            CollectLeaves(k_item, k_leaves)
        Next
        Return k_leaves
    End Function

    ' The ticked data rows, in the order of the tree.
    Private Sub CollectChosen()
        _chosen.Clear()
        For Each k_leaf As AdvancedTreeControl.TreeItem In AllLeaves()
            If k_leaf.CheckState = AdvancedTreeControl.TreeCheckState.Checked Then _chosen.Add(k_leaf.Key)
        Next
    End Sub

    ' Every data row is ticked and no branch is left unread (then the filters mean «everything» and are left out).
    Private Function AllTicked() As Boolean
        If treeSel.Items.Count = 0 OrElse AnyLazy() Then Return False
        Dim k_leaves As List(Of AdvancedTreeControl.TreeItem) = AllLeaves()
        Return k_leaves.Count > 0 AndAlso k_leaves.All(Function(k_l) k_l.CheckState = AdvancedTreeControl.TreeCheckState.Checked)
    End Function

    ' Puts the ticks of _chosen back on the rows (after rows were replaced) and derives the state of every branch from its rows.
    Private Sub ReapplyTicks()
        For Each k_item As AdvancedTreeControl.TreeItem In treeSel.Items
            DeriveState(k_item)
        Next
    End Sub

    Private Function DeriveState(k_item As AdvancedTreeControl.TreeItem) As AdvancedTreeControl.TreeCheckState
        Dim k_kids As List(Of AdvancedTreeControl.TreeItem) = k_item.Children.Where(Function(k_c) Not k_c.IsLoader).ToList()
        If k_kids.Count = 0 Then
            k_item.CheckState = If(VerconSelectionLogic.IsLeaf(k_item.Key) AndAlso _chosen.Contains(k_item.Key),
                                   AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked)
            Return k_item.CheckState
        End If
        Dim k_states As List(Of AdvancedTreeControl.TreeCheckState) = k_kids.Select(Function(k_c) DeriveState(k_c)).ToList()
        If k_states.All(Function(k_s) k_s = AdvancedTreeControl.TreeCheckState.Checked) Then
            k_item.CheckState = AdvancedTreeControl.TreeCheckState.Checked
        ElseIf k_states.All(Function(k_s) k_s = AdvancedTreeControl.TreeCheckState.Unchecked) Then
            k_item.CheckState = AdvancedTreeControl.TreeCheckState.Unchecked
        Else
            k_item.CheckState = AdvancedTreeControl.TreeCheckState.Indeterminate
        End If
        Return k_item.CheckState
    End Function

    ' Opens a branch: asks the server for its rows (all the levels under it when k_recursive) and puts them in place of what was there.
    Private Async Function LoadBranchAsync(k_item As AdvancedTreeControl.TreeItem, k_recursive As Boolean) As Task
        Dim k_mySeq As Integer = _lookupSeq
        Dim k_params As New Dictionary(Of String, String)(_lookupBase, StringComparer.Ordinal) From {{"branch", k_item.Key}}
        If k_recursive Then k_params("recursive") = "1"
        Dim k_report As String = _lookupReport
        Interlocked.Increment(_selectionLoading)
        UpdateButtons()
        Try
            Dim k_branch As VerconBranch = Await _gate.RunAsync(
                Function() _api.GetVerconSelectionAsync(_ctx.Selection.Clone(), k_report, k_params, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _lookupSeq Then Return
            k_item.Children.Clear()
            For Each k_node As VerconSelNode In k_branch.Nodes
                AddSelNode(k_node, k_item)
            Next
            treeSel.CompleteLazyLoad(k_item)
            SetBoxes()
            ReapplyTicks()
            treeSel.Invalidate()
        Catch
            ' The branch stays closed and can be opened again.
            If Not IsDisposed AndAlso k_mySeq = _lookupSeq Then
                k_item.Children.Clear()
                treeSel.CompleteLazyLoad(k_item)
                k_item.LazyNode = True
                k_item.Expanded = False
                treeSel.Invalidate()
            End If
            Throw
        Finally
            Interlocked.Decrement(_selectionLoading)
            If Not IsDisposed Then UpdateButtons()
        End Try
    End Function

    ' A press on the expander of a branch not read yet.
    Private Async Sub TreeSel_RequestLazyLoad(sender As Object, k_item As AdvancedTreeControl.TreeItem) Handles treeSel.RequestLazyLoad
        Try
            Await LoadBranchAsync(k_item, False).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.TreeSel_RequestLazyLoad", ex)
            If Not IsDisposed Then ShowNotice(BranchMessage(ex), NoticeKind.Error)
        End Try
    End Sub

    Private Shared Function BranchMessage(k_error As Exception) As String
        Dim k_api As ApiException = TryCast(k_error, ApiException)
        Return If(k_api IsNot Nothing, k_api.Message, "Datele nu au putut fi citite.") & " Închideți și redeschideți ramura pentru a reîncerca."
    End Function

    ' A box was pressed (the tree already set the rows under it and the branches above it). A branch not read yet is read first.
    Private Async Sub TreeSel_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles treeSel.NodeChecked
        Try
            CollectChosen()
            If k_node.CheckState = AdvancedTreeControl.TreeCheckState.Checked AndAlso HasLazy(k_node) AndAlso _checking.Add(k_node.Key) Then
                Try
                    Await LoadBranchAsync(k_node, True).ConfigureAwait(True)
                    treeSel.SetItemCheckState(k_node, AdvancedTreeControl.TreeCheckState.Checked)
                Catch ex As OperationCanceledException
                    ' The page was closed.
                Catch ex As Exception
                    GlobalErrorLog.Write("VerconRapoarteForm.TreeSel_NodeChecked", ex)
                    If Not IsDisposed Then
                        treeSel.SetItemCheckState(k_node, AdvancedTreeControl.TreeCheckState.Unchecked)
                        ShowNotice(BranchMessage(ex), NoticeKind.Error)
                    End If
                Finally
                    _checking.Remove(k_node.Key)
                End Try
                If IsDisposed Then Return
                CollectChosen()
            End If
            TicksChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.TreeSel_NodeChecked", ex)
        End Try
    End Sub

    ' The ticks changed: the report on screen is stale and «Selectează TOATE» says what is true.
    Private Sub TicksChanged()
        _filling = True
        Try
            chkToate.CheckState = If(AllTicked(), CheckState.Checked, If(_chosen.Count > 0, CheckState.Indeterminate, CheckState.Unchecked))
        Finally
            _filling = False
        End Try
        InvalidateReport()
    End Sub

    ' «Selectează TOATE»: reads every branch, then ticks (or clears) every row.
    Private Async Sub ChkToate_Click(sender As Object, e As EventArgs) Handles chkToate.Click
        Try
            If IsDisposed OrElse _selectionLoading > 0 OrElse _checking.Count > 0 Then Return
            Dim k_on As Boolean = chkToate.Checked
            Dim k_state As AdvancedTreeControl.TreeCheckState = If(k_on, AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked)
            Try
                If k_on Then
                    For Each k_root As AdvancedTreeControl.TreeItem In treeSel.Items.ToList()
                        If HasLazy(k_root) Then Await LoadBranchAsync(k_root, True).ConfigureAwait(True)
                    Next
                End If
                For Each k_root As AdvancedTreeControl.TreeItem In treeSel.Items.ToList()
                    treeSel.SetItemCheckState(k_root, k_state)
                Next
            Catch ex As OperationCanceledException
                ' The page was closed.
            Catch ex As Exception
                GlobalErrorLog.Write("VerconRapoarteForm.ChkToate_Click", ex)
                If Not IsDisposed Then ShowNotice(BranchMessage(ex), NoticeKind.Error)
            End Try
            If IsDisposed Then Return
            CollectChosen()
            TicksChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.ChkToate_Click", ex)
        End Try
    End Sub

    ' The search looks in the rows read so far: the rest is read first (the site does the same), then the search is run again.
    Private Async Sub TreeSel_SearchStarting(k_text As String) Handles treeSel.SearchStarting
        Try
            If IsDisposed OrElse _searchLoading OrElse Not AnyLazy() Then Return
            _searchLoading = True
            Try
                For Each k_root As AdvancedTreeControl.TreeItem In treeSel.Items.ToList()
                    If HasLazy(k_root) Then Await LoadBranchAsync(k_root, True).ConfigureAwait(True)
                Next
                If Not IsDisposed Then treeSel.RerunSearch()
            Finally
                _searchLoading = False
            End Try
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.TreeSel_SearchStarting", ex)
            If Not IsDisposed Then ShowNotice(BranchMessage(ex), NoticeKind.Error)
        End Try
    End Sub

    ' ── Which report can be asked for now ───────────────────────────────────────

    Private Sub SetLoading(k_loading As Boolean)
        _loading = k_loading
        UpdateButtons()
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        UpdateButtons()
    End Sub

    ' Anything changed that the report on screen was made from: the saved copy is stale and the buttons say what is missing.
    Private Sub InvalidateReport()
        _stateSeq += 1
        _handleValid = False
        UpdateButtons()
    End Sub

    Private Sub UpdateButtons()
        If IsDisposed Then Return
        Dim k_working As Boolean = _busy OrElse _loading OrElse _lookupLoading OrElse _selectionLoading > 0
        busyBar.Running = k_working
        Dim k_reason As String = String.Empty
        If _spec Is Nothing Then
            k_reason = NoReport
        Else
            k_reason = PeriodError()
            If k_reason.Length = 0 Then k_reason = VerconSelectionLogic.SelectionError(_spec, _chosen)
        End If
        lblStare.Text = k_reason
        Dim k_free As Boolean = Not k_working AndAlso _spec IsNot Nothing AndAlso k_reason.Length = 0
        btnListare.Enabled = k_free
        btnExcel.Enabled = k_free
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind)
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
    End Sub

    ' ── The report: PDF on the page, Excel on disk ──────────────────────────────

    ' The report the server holds for the choices now: the one already made while nothing changed, else a new one (the old copy is let go of).
    ' Nothing when a choice changed while the server was working.
    Private Async Function EnsureHandleAsync() As Task(Of VerconReportHandle)
        If _handle IsNot Nothing AndAlso _handleValid Then Return _handle
        Dim k_mySeq As Integer = _stateSeq
        Dim k_selection As VerconSelection = _ctx.Selection.Clone()
        Dim k_name As String = _spec.Id
        Dim k_params As Dictionary(Of String, String) = BuildQuery(True)
        Dim k_new As VerconReportHandle = Await _gate.RunAsync(
            Function() _api.GenerateVerconReportAsync(k_selection, k_name, k_params, _cts.Token)).ConfigureAwait(True)
        If IsDisposed Then Return Nothing
        If k_mySeq <> _stateSeq Then
            ReleaseQuietly(k_selection, k_new.Snapshot)   ' made for choices that are gone
            Return Nothing
        End If
        ReleaseLast()
        _handle = k_new
        _handleSelection = k_selection
        _handleValid = True
        Return k_new
    End Function

    ' The saved report as a file; a copy the server already dropped is made again once.
    Private Async Function DownloadAsync(k_format As String) As Task(Of KeyValuePair(Of VerconReportHandle, Byte()))
        For k_attempt As Integer = 1 To 2
            Dim k_handle As VerconReportHandle = Await EnsureHandleAsync().ConfigureAwait(True)
            If k_handle Is Nothing Then Return Nothing
            Dim k_selection As VerconSelection = _handleSelection
            Try
                Dim k_bytes As Byte() = Await _gate.RunAsync(
                    Function() _api.DownloadVerconReportAsync(k_selection, k_handle.Snapshot, k_format, _cts.Token)).ConfigureAwait(True)
                Return New KeyValuePair(Of VerconReportHandle, Byte())(k_handle, k_bytes)
            Catch ex As ApiException When k_attempt = 1 AndAlso String.Equals(ex.Reason, "REPORT_EXPIRED", StringComparison.Ordinal)
                _handleValid = False
            End Try
        Next
        Return Nothing
    End Function

    Private Async Sub BtnListare_Click(sender As Object, e As EventArgs) Handles btnListare.Click
        Try
            If _busy OrElse _spec Is Nothing Then Return
            Try
                SetBusy(True)
                ntfMesaj.Visible = False
                Dim k_got As KeyValuePair(Of VerconReportHandle, Byte()) = Await DownloadAsync("pdf").ConfigureAwait(True)
                If IsDisposed OrElse k_got.Key Is Nothing Then Return
                TempPdfStore.EnsureRoot()
                Dim k_file As String = TempPdfStore.PathFor($"VERCON_{SafeName(k_got.Key.FileName)}_{Guid.NewGuid():N}.pdf")
                File.WriteAllBytes(k_file, k_got.Value)
                previewPdf.ShowDocument(k_file, True)
            Catch ex As OperationCanceledException
                ' The page was closed.
            Catch ex As ApiException
                GlobalErrorLog.Write("VerconRapoarteForm.BtnListare_Click", ex)
                If Not IsDisposed Then ShowNotice(ex.Message, NoticeKind.Error)
            Catch ex As Exception
                GlobalErrorLog.Write("VerconRapoarteForm.BtnListare_Click", ex)
                If Not IsDisposed Then ShowNotice("Raportul nu a putut fi afișat. Detalii în jurnalul de erori.", NoticeKind.Error)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.BtnListare_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnExcel_Click(sender As Object, e As EventArgs) Handles btnExcel.Click
        Try
            If _busy OrElse _spec Is Nothing Then Return
            Using k_dialog As New SaveFileDialog()
                k_dialog.Title = "Salvează raportul ca Excel"
                k_dialog.Filter = "Excel (*.xlsx)|*.xlsx"
                k_dialog.FileName = SafeName(_spec.Id & "_" & Iso(dtpPana.Value)) & ".xlsx"
                k_dialog.OverwritePrompt = True
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                Try
                    SetBusy(True)
                    ntfMesaj.Visible = False
                    Dim k_got As KeyValuePair(Of VerconReportHandle, Byte()) = Await DownloadAsync("xlsx").ConfigureAwait(True)
                    If IsDisposed OrElse k_got.Key Is Nothing Then Return
                    File.WriteAllBytes(k_dialog.FileName, k_got.Value)
                    ShowNotice("Excelul a fost salvat: " & k_dialog.FileName, NoticeKind.Success)
                Catch ex As OperationCanceledException
                    ' The page was closed.
                Catch ex As ApiException
                    GlobalErrorLog.Write("VerconRapoarteForm.BtnExcel_Click", ex)
                    If Not IsDisposed Then ShowNotice(ex.Message, NoticeKind.Error)
                Catch ex As IOException
                    GlobalErrorLog.Write("VerconRapoarteForm.BtnExcel_Click", ex)
                    If Not IsDisposed Then ShowNotice("Fișierul nu a putut fi scris. Este deschis în alt program sau locul nu permite scrierea.", NoticeKind.Error)
                Finally
                    If Not IsDisposed Then SetBusy(False)
                End Try
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.BtnExcel_Click", ex)
            If Not IsDisposed Then ShowNotice("Excelul nu a putut fi salvat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    Private Shared Function SafeName(k_name As String) As String
        Dim k_bad As Char() = Path.GetInvalidFileNameChars()
        Dim k_clean As String = New String(If(k_name, "raport").Select(Function(k_c) If(Array.IndexOf(k_bad, k_c) >= 0, "_"c, k_c)).ToArray())
        Return If(k_clean.Length = 0, "raport", k_clean)
    End Function

    ' ── Letting the server drop the saved copy ──────────────────────────────────

    ' The report held before is not needed any more.
    Private Sub ReleaseLast()
        If _handle Is Nothing Then Return
        ReleaseQuietly(_handleSelection, _handle.Snapshot)
        _handle = Nothing
        _handleValid = False
    End Sub

    ' Best effort: the server sweeps what is left anyway, so a failure here is logged and nothing more. Not tied to the page's cancellation: it
    ' must still go out when the page is closing.
    Private Async Sub ReleaseQuietly(k_selection As VerconSelection, k_snapshot As String)
        Try
            Await _gate.RunAsync(Function() _api.ReleaseVerconReportAsync(k_selection, k_snapshot, CancellationToken.None)).ConfigureAwait(False)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.ReleaseQuietly", ex)
        End Try
    End Sub

    Private Sub VerconRapoarteForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
            tmrLookups.Stop()
            If _api IsNot Nothing Then ReleaseLast()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconRapoarteForm.VerconRapoarteForm_FormClosing", ex)
        End Try
    End Sub

End Class
