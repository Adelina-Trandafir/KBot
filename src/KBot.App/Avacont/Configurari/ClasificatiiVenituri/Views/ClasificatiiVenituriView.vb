Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Venituri» page of the «Clasificatii» window (slice AV01-11): the revenue classifications as a tree Capitol > Subcapitol > Paragraf (code in the
''' first column, name in the second), and for the chosen classification its budget for the working year as VERSIONS (one row = the budget from its
''' «Inceput» date on, quarters 1-4 typed; «+» in the footer adds a version, «✕» removes one) and its corrections (typed straight in the grid), saved
''' together by «Salveaza» in one transaction. It is the revenue twin of <see cref="ClasificatiiBugetView"/> and follows the same rules and the same
''' grids; a node above the leaves shows the same two grids as a read-only summary (the LAST budget of each classification under it and the TOTAL of
''' its corrections) and the row under them adds the last budget and all the corrections.
'''
''' The budget belongs to a UNIT: the sector+source of the title bar and the working year name it (the server finds the row of <c>Unitati</c>). With the title
''' bar on «Toate» (or on a source that has no unit) the tree is shown but the grids stay empty and say why. A change of the year or of the source in the title
''' bar settles what is typed first (the question of unsaved changes) and reads the budget of the new unit. «+» in the tree footer opens
''' <see cref="ClasificatiiVenituriAddForm"/> (a code of the nomenclator <c>DefaClsfV</c>).
''' </summary>
Public Class ClasificatiiVenituriView
    Implements IThemedControl, IClasificatiiPage

    Private Const ColTrim1 As String = "trim1"
    Private Const ColTrim2 As String = "trim2"
    Private Const ColTrim3 As String = "trim3"
    Private Const ColTrim4 As String = "trim4"
    Private Const ColTotal As String = "total"
    Private Const ColStart As String = "inceput"
    Private Const ColDocument As String = "document"
    Private Const ColData As String = "data"
    Private Const ColDelete As String = "sterge"
    Private Const ColId As String = "id"
    Private Const ColClsf As String = "clsf"
    Private Const DeleteCaption As String = "✕"
    Private Const NoSourceText As String = "Alegeți o sursă (nu «Toate») în bara de titlu: bugetul de venituri este al unei unități."
    Private Shared ReadOnly QuarterColumns As String() = {ColTrim1, ColTrim2, ColTrim3, ColTrim4}
    Private Shared ReadOnly DateFormats As String() = {
        "d.M.yyyy", "dd.MM.yyyy", "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "yyyy-MM-dd", "ddMMyyyy"}

    Private _api As IClasificatiiVenituriApi
    Private _gate As ReauthGate
    Private _session As SessionContext
    Private ReadOnly _deletedIds As New List(Of Integer)()
    Private ReadOnly _deletedBudgetIds As New List(Of Integer)()
    Private ReadOnly _leafNodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
    ' The last budget + corrections total of each classification, for the unit and year in _scopeAn / _scopeSs.
    Private ReadOnly _summary As New Dictionary(Of Integer, VenitBugetSumar)()
    Private _catalog As ClasificatiiVenituriCatalog
    Private _current As ClasificatieVenit
    Private _currentNode As AdvancedTreeControl.TreeItem
    ' The year and sector+source the budget on screen was read for (and is saved for), whatever the title bar says now.
    Private _scopeAn As Integer
    Private _scopeSs As String = String.Empty
    Private _scopeOk As Boolean
    ' The source the tree on screen was read for (slice AV01-12): Nothing = every source («Toate»).
    Private _treeSs As String
    ' A child subunit has the same source as its parent and works on the parent's data (one unit): nothing is blocked, the footer says so
    ' (empty = the source has no child subunit).
    Private _childNote As String = String.Empty
    Private _dirty As Boolean
    Private _busy As Boolean
    Private _loadSeq As Integer
    Private _summaryMode As Boolean
    Private _plusIcon As Image
    Private _budgetCaption As String
    Private _correctionsCaption As String

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <summary>Gives the view its API and the session (year and source of the title bar) and loads the tree. The window calls it once, the first time the page is shown.</summary>
    Public Sub Bind(k_api As IClasificatiiVenituriApi, k_gate As ReauthGate, k_session As SessionContext)
        Try
            ArgumentNullException.ThrowIfNull(k_api)
            ArgumentNullException.ThrowIfNull(k_gate)
            ArgumentNullException.ThrowIfNull(k_session)
            _api = k_api
            _gate = k_gate
            _session = k_session
            AddHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
            AddHandler Disposed, AddressOf ViewDisposed
            _plusIcon = gridBuget.FooterRightIcon
            _budgetCaption = gridBuget.FooterCaption
            _correctionsCaption = gridRectificari.FooterCaption
            ShowNoSelection()
            LoadAll(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.Bind", ex)
            Throw
        End Try
    End Sub

    Private Sub ViewDisposed(sender As Object, e As EventArgs)
        Try
            If _session IsNot Nothing Then RemoveHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.ViewDisposed", ex)
        End Try
    End Sub

    ' The sector+source the budget is read for: the title bar's, or Nothing on «Toate» / with none.
    Private Function CurrentSs() As String
        If _session Is Nothing OrElse _session.ToateSursele OrElse String.IsNullOrWhiteSpace(_session.SectorSursa) Then Return Nothing
        Return _session.SectorSursa
    End Function

    ' ── Loading ─────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await. Reads the classifications, builds the tree, then the budget side.
    Private Async Sub LoadAll(k_select As Integer?)
        Try
            Dim k_seq As Integer = Interlocked.Increment(_loadSeq)
            SetBusy(True, "Se încarcă clasificațiile de venituri…")
            ' The tree follows the source of the title bar: one source, or every source on «Toate».
            Dim k_treeSs As String = CurrentSs()
            Dim k_catalog As ClasificatiiVenituriCatalog = Await _gate.RunAsync(
                Function() _api.GetClasificatiiVenituriAsync(k_treeSs, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _loadSeq Then Return
            _catalog = k_catalog
            _treeSs = k_treeSs
            BuildTree(k_catalog, k_select)
            Await LoadScopeAsync(k_seq).ConfigureAwait(True)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriView.LoadAll", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.LoadAll", ex)
            If Not IsDisposed Then SetStatus("Clasificațiile de venituri nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    ' Reads the budget summary of the unit the title bar names, then shows again what was selected. Never throws to the caller (a refusal of the
    ' server, like «no unit has this source», goes to the status line). The caller shows busy.
    Private Async Function LoadScopeAsync(k_seq As Integer) As Task
        Try
            _summary.Clear()
            _scopeOk = False
            _childNote = String.Empty
            Dim k_ss As String = CurrentSs()
            If k_ss Is Nothing Then
                ShowNoSelection()
                SetStatus(NoSourceText)
                Return
            End If
            Dim k_an As Integer = _session.An
            Dim k_data As VenitSumarUnitate = Await _gate.RunAsync(
                Function() _api.GetSumarVenituriAsync(k_an, k_ss, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed OrElse k_seq <> _loadSeq Then Return
            For Each k_row As VenitBugetSumar In k_data.Rows
                _summary(k_row.IdClsfV) = k_row
            Next
            _scopeAn = k_an
            _scopeSs = k_ss
            _scopeOk = True
            _childNote = If(k_data.Child Is Nothing, String.Empty, k_data.Child.NoteFor(k_ss))
            SetStatus(If(_catalog IsNot Nothing AndAlso _catalog.Items.Count = 0,
                         $"Sursa {k_ss} nu are clasificații de venituri. Le adăugați cu «+» de sub arbore.", String.Empty))
            Await ReapplySelectionAsync().ConfigureAwait(True)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriView.LoadScopeAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.LoadScopeAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus("Bugetul de venituri nu a putut fi încărcat. Detalii în jurnalul de erori.")
            End If
        End Try
    End Function

    ' Shows the budget of the selected node again (a leaf: its versions and corrections; a node above the leaves: the summary).
    Private Async Function ReapplySelectionAsync() As Task
        If _currentNode Is Nothing OrElse Not _scopeOk Then
            ShowNoSelection()
            Return
        End If
        Dim k_c As ClasificatieVenit = TryCast(_currentNode.Tag, ClasificatieVenit)
        If k_c IsNot Nothing Then
            Await LoadBudgetAsync(k_c).ConfigureAwait(True)
            Return
        End If
        Dim k_leaves As List(Of ClasificatieVenit) = LeavesUnder(_currentNode)
        If k_leaves Is Nothing Then
            ShowNoSelection()
            Return
        End If
        _current = Nothing
        FillSummary(_currentNode, k_leaves)
    End Function

    ' UI boundary: a change of the year or of the source in the title bar. What is typed is settled first (the question asks Yes / No / Cancel);
    ' Cancel keeps the screen as it is, still showing the budget of the unit it was read for.
    Private Async Sub SessionPeriodChanged(sender As Object, e As PeriodChangedEventArgs)
        Try
            If IsDisposed OrElse _api Is Nothing OrElse _busy Then Return
            If Not Await CommitAsync().ConfigureAwait(True) Then
                SetStatus($"Modificările nesalvate au rămas: bugetul afișat este cel din {_scopeAn.ToString(CultureInfo.InvariantCulture)}, sursa {_scopeSs}.")
                Return
            End If
            If IsDisposed Then Return
            ' Another source = another list of classifications: the tree is read again (what was open stays open when it is still there).
            If Not String.Equals(CurrentSs(), _treeSs, StringComparison.OrdinalIgnoreCase) Then
                LoadAll(If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsfV))
                Return
            End If
            Dim k_seq As Integer = Interlocked.Increment(_loadSeq)
            SetBusy(True, Nothing)
            Try
                Await LoadScopeAsync(k_seq).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.SessionPeriodChanged", ex)
        End Try
    End Sub

    ' ── Tree ────────────────────────────────────────────────────────────────────

    Private Sub BuildTree(k_catalog As ClasificatiiVenituriCatalog, k_selectId As Integer?)
        tree.Clear()
        _leafNodes.Clear()
        _currentNode = Nothing
        Dim k_folder As Image = My.Resources.Resources.folder_open
        Dim k_toSelect As AdvancedTreeControl.TreeItem = Nothing
        ' On «Toate» the same code can exist on several sources: the leaf then says which one it is.
        Dim k_multi As Boolean = k_catalog.Items.Select(Function(k_c) k_c.Ss).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1

        For Each k_cap In k_catalog.Items.GroupBy(Function(k_c) k_c.Capitol).OrderBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
            Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"C|{k_cap.Key}", $"{k_cap.Key}~~~{Escape(NameOr(k_catalog.CapitolNames, k_cap.Key))}",
                pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
            k_root.Bold = True
            For Each k_sub In k_cap.GroupBy(Function(k_c) k_c.Subcapitol).OrderBy(Function(k_g) k_g.Key, StringComparer.Ordinal)
                ' Sub-chapter «00» is the chapter itself: its classification hangs straight under the chapter.
                Dim k_parent As AdvancedTreeControl.TreeItem = k_root
                If Not String.Equals(k_sub.Key, "00", StringComparison.Ordinal) Then
                    k_parent = tree.AddItem(
                        $"S|{k_cap.Key}|{k_sub.Key}",
                        $"{k_cap.Key}.{k_sub.Key}~~~{Escape(NameOr(k_catalog.SubcapitolNames, k_cap.Key & k_sub.Key))}", k_root,
                        pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
                End If
                For Each k_item As ClasificatieVenit In k_sub.OrderBy(Function(k_c) k_c.Paragraf, StringComparer.Ordinal)
                    Dim k_code As String = If(k_multi, $"{k_item.Cod} ({k_item.Ss})", k_item.Cod)
                    Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                        $"L|{k_item.IdClsfV}", $"{k_code}~~~{Escape(k_item.Denumire)}", k_parent)
                    k_leaf.Tag = k_item
                    k_leaf.Tooltip = k_code
                    _leafNodes(k_item.IdClsfV) = k_leaf
                    If k_selectId.HasValue AndAlso k_item.IdClsfV = k_selectId.Value Then k_toSelect = k_leaf
                Next
            Next
        Next

        If k_toSelect IsNot Nothing Then
            tree.SelectAndReveal(k_toSelect)
            _currentNode = k_toSelect
        End If
        tree.Invalidate()
        UpdateDeleteButton()
    End Sub

    Private Shared Function NameOr(k_names As Dictionary(Of String, String), k_key As String) As String
        Dim k_name As String = Nothing
        If k_names IsNot Nothing AndAlso k_names.TryGetValue(k_key, k_name) AndAlso Not String.IsNullOrWhiteSpace(k_name) Then
            Return k_name.Trim()
        End If
        Return String.Empty
    End Function

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing OrElse pNode Is _currentNode Then Return
            ChangeSelection(pNode, _currentNode)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' UI boundary: asks about unsaved changes, then loads the node's budget.
    Private Async Sub ChangeSelection(k_node As AdvancedTreeControl.TreeItem, k_previous As AdvancedTreeControl.TreeItem)
        Try
            If _busy Then
                If k_previous IsNot Nothing Then tree.SelectAndReveal(k_previous)
                Return
            End If
            If _dirty Then
                Dim k_answer As DialogResult = AskToSave()
                If k_answer = DialogResult.Cancel Then
                    If k_previous IsNot Nothing Then tree.SelectAndReveal(k_previous)
                    Return
                End If
                If k_answer = DialogResult.Yes Then
                    If Not Await SaveAsync().ConfigureAwait(True) Then
                        If k_previous IsNot Nothing Then tree.SelectAndReveal(k_previous)
                        Return
                    End If
                End If
            End If
            _currentNode = k_node
            UpdateDeleteButton()
            Await ReapplySelectionAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.ChangeSelection", ex)
        End Try
    End Sub

    ' UI boundary: «+» of the tree footer -> the window that adds a classification from the nomenclator, then the tree is read again with the new one selected.
    Private Async Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            If _busy OrElse _api Is Nothing Then Return
            If Not Await CommitAsync().ConfigureAwait(True) Then Return
            ' A classification belongs to a source: on «Toate» there is no source to add it to.
            Dim k_ss As String = CurrentSs()
            If k_ss Is Nothing Then
                KBotMessage.Show(Me, "Alegeți o sursă (nu «Toate») în bara de titlu: clasificația se adaugă la o sursă.",
                                 "Clasificații venituri", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Using k_form As New ClasificatiiVenituriAddForm(_api, _gate, k_ss)
                If k_form.ShowDialog(Me) <> DialogResult.OK OrElse Not k_form.AddedId.HasValue Then Return
                LoadAll(k_form.AddedId)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.Tree_FooterRightIconClicked", ex)
            KBotMessage.Show(Me, "Fereastra de adăugare nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Clasificații venituri", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── Budget + corrections ────────────────────────────────────────────────────

    ' Risky (HTTP): shows its own error in the status line; never throws to the caller.
    Private Async Function LoadBudgetAsync(k_c As ClasificatieVenit) As Task
        Try
            Dim k_an As Integer = _scopeAn
            Dim k_ss As String = _scopeSs
            SetBusy(True, "Se încarcă bugetul…")
            Dim k_data As BugetClasificatie = Await _gate.RunAsync(
                Function() _api.GetBugetVenitAsync(k_c.IdClsfV, k_an, k_ss, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _current = k_c
            FillGrids(k_data)
            SetStatus(String.Empty)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriView.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus("Bugetul nu a putut fi încărcat. Detalii în jurnalul de erori.")
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The classifications under a chapter («C|capitol») or sub-chapter («S|capitol|sub») node, in tree order; Nothing for any other node.
    Private Function LeavesUnder(k_node As AdvancedTreeControl.TreeItem) As List(Of ClasificatieVenit)
        If _catalog Is Nothing OrElse String.IsNullOrEmpty(k_node.Key) Then Return Nothing
        Dim k_parts As String() = k_node.Key.Split("|"c)
        Dim k_level As String = k_parts(0)
        Dim k_expected As Integer = If(k_level = "C", 2, If(k_level = "S", 3, Integer.MaxValue))
        If k_parts.Length <> k_expected Then Return Nothing
        Return _catalog.Items.
            Where(Function(k_x) k_x.Capitol = k_parts(1) AndAlso (k_parts.Length < 3 OrElse k_x.Subcapitol = k_parts(2))).
            OrderBy(Function(k_x) k_x.Subcapitol, StringComparer.Ordinal).
            ThenBy(Function(k_x) k_x.Paragraf, StringComparer.Ordinal).
            ToList()
    End Function

    ' Budget grid: every classification under the node with its LAST version; corrections grid: the classifications that have corrections, with the
    ' total of the year. Both read from the summary of the unit.
    Private Sub FillSummary(k_node As AdvancedTreeControl.TreeItem, k_leaves As List(Of ClasificatieVenit))
        ApplyMode(True)
        SetCorrectionsTotal(False)
        gridBuget.BeginUpdate()
        gridRectificari.BeginUpdate()
        Try
            gridBuget.ClearRows()
            gridRectificari.ClearRows()
            For Each k_c As ClasificatieVenit In k_leaves
                Dim k_s As VenitBugetSumar = Nothing
                _summary.TryGetValue(k_c.IdClsfV, k_s)

                Dim k_row As KBotDataRow = gridBuget.AddRow()
                k_row(ColClsf) = k_c.Cod
                If k_s IsNot Nothing AndAlso k_s.LastBudget IsNot Nothing Then
                    k_row(ColTrim1) = Box(k_s.LastBudget.Amounts.Trim1)
                    k_row(ColTrim2) = Box(k_s.LastBudget.Amounts.Trim2)
                    k_row(ColTrim3) = Box(k_s.LastBudget.Amounts.Trim3)
                    k_row(ColTrim4) = Box(k_s.LastBudget.Amounts.Trim4)
                    k_row(ColTotal) = k_s.LastBudget.Amounts.Total
                End If

                If k_s IsNot Nothing AndAlso k_s.CorrectionsTotal IsNot Nothing Then
                    Dim k_corr As KBotDataRow = gridRectificari.AddRow()
                    k_corr(ColClsf) = k_c.Cod
                    k_corr(ColTrim1) = Box(k_s.CorrectionsTotal.Trim1)
                    k_corr(ColTrim2) = Box(k_s.CorrectionsTotal.Trim2)
                    k_corr(ColTrim3) = Box(k_s.CorrectionsTotal.Trim3)
                    k_corr(ColTrim4) = Box(k_s.CorrectionsTotal.Trim4)
                    k_corr(ColTotal) = k_s.CorrectionsTotal.Total
                End If
            Next
        Finally
            gridRectificari.EndUpdate()
            gridBuget.EndUpdate()
        End Try
        gridBuget.ClearDirty()
        gridRectificari.ClearDirty()
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()

        gridBuget.Enabled = True
        gridRectificari.Enabled = True
        gridTotal.Enabled = True
        UpdateTotalRow()
        Dim k_parts As String() = k_node.Key.Split("|"c)
        Dim k_code As String = String.Join(".", k_parts.Skip(1))   ' capitol[.sub]
        lblBuget.Text = $"Buget {_scopeAn.ToString(CultureInfo.InvariantCulture)} — {k_code} ({_scopeSs}): {k_leaves.Count} clasificații"
        lblRectificari.Text = $"Rectificări bugetare {_scopeAn.ToString(CultureInfo.InvariantCulture)} — total pe clasificație"
        Dim k_split As Integer = If(k_node.Caption, String.Empty).IndexOf("~~~", StringComparison.Ordinal)
        tips.SetToolTipText(lblBuget, If(k_split >= 0, k_node.Caption.Substring(k_split + 3), k_code))
        SetDirty(False)
    End Sub

    ' The summary is read-only: it has the «Clsf» column (the one that stretches) and no «Inceput», «Nr. doc.», «Data», «✕» or footer «+»; a leaf has the
    ' editable grids as the designer made them.
    Private Sub ApplyMode(k_summary As Boolean)
        If k_summary = _summaryMode Then Return
        _summaryMode = k_summary
        SetColumn(gridBuget, ColStart, Not k_summary)
        SetColumn(gridBuget, ColDelete, Not k_summary)
        SetColumn(gridRectificari, ColClsf, k_summary)
        SetColumn(gridRectificari, ColDocument, Not k_summary)
        SetColumn(gridRectificari, ColData, Not k_summary)
        SetColumn(gridRectificari, ColDelete, Not k_summary)
        SetColumn(gridTotal, ColStart, Not k_summary)
        SetColumn(gridTotal, ColDelete, Not k_summary)
        For Each k_grid As KBotDataView In {gridBuget, gridRectificari}
            k_grid.ReadOnlyGrid = k_summary
            k_grid.FooterRightIcon = If(k_summary, Nothing, _plusIcon)
        Next
        gridRectificari.FillColumnKey = If(k_summary, ColClsf, ColDocument)
        gridBuget.FooterCaption = If(k_summary, "Ultimul buget al fiecărei clasificații", _budgetCaption)
        gridRectificari.FooterCaption = If(k_summary, "Rectificările anului, pe clasificație", _correctionsCaption)
    End Sub

    ' The footer sums belong to ONE classification's corrections; with a node (or nothing) chosen there is no leaf, and summing the corrections of every
    ' classification means nothing.
    Private Sub SetCorrectionsTotal(k_shown As Boolean)
        For Each k_key As String In {ColTrim1, ColTrim2, ColTrim3, ColTrim4, ColTotal}
            gridRectificari.Columns.First(Function(k_col) String.Equals(k_col.Key, k_key, StringComparison.Ordinal)).Aggregate =
                If(k_shown, KBotAggregate.Sum, KBotAggregate.None)
        Next
        If Not k_shown AndAlso Not _summaryMode Then gridRectificari.FooterCaption = String.Empty
    End Sub

    Private Shared Sub SetColumn(k_grid As KBotDataView, k_key As String, k_shown As Boolean)
        Dim k_column As KBotDataColumn = k_grid.Columns.First(Function(k_c) String.Equals(k_c.Key, k_key, StringComparison.Ordinal))
        k_column.Visible = If(k_shown, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
    End Sub

    ' The row under both grids = the LAST budget (newest «Inceput») + ALL the corrections, quarter by quarter, read from what the two grids show now, so it
    ' follows every edit. A node adds up every classification under it (the budget grid holds the last budget of each).
    Private Sub UpdateTotalRow()
        gridTotal.ClearRows()
        If _current Is Nothing AndAlso Not _summaryMode Then Return
        Dim k_sums(QuarterColumns.Length - 1) As Decimal
        Dim k_lastStart As Date? = Nothing
        Dim k_lastRow As KBotDataRow = Nothing
        For i As Integer = 0 To gridBuget.RowCount - 1
            Dim k_row As KBotDataRow = gridBuget.Rows(i)
            If _summaryMode Then
                AddQuarters(k_sums, k_row)
                Continue For
            End If
            Dim k_startValue As Object = k_row(ColStart)
            If TypeOf k_startValue Is Date AndAlso (Not k_lastStart.HasValue OrElse DirectCast(k_startValue, Date) > k_lastStart.Value) Then
                k_lastStart = DirectCast(k_startValue, Date)
                k_lastRow = k_row
            End If
        Next
        If k_lastRow IsNot Nothing Then AddQuarters(k_sums, k_lastRow)
        For i As Integer = 0 To gridRectificari.RowCount - 1
            AddQuarters(k_sums, gridRectificari.Rows(i))
        Next

        Dim k_total As KBotDataRow = gridTotal.AddRow()
        k_total(ColClsf) = "Buget + rectificări"
        Dim k_sum As Decimal = 0D
        For q As Integer = 0 To k_sums.Length - 1
            k_total(QuarterColumns(q)) = k_sums(q)
            k_sum += k_sums(q)
        Next
        k_total(ColTotal) = k_sum
    End Sub

    ' The three grids must keep their columns on the same vertical lines. A grid that has more rows than fit shows its scroll bar and loses that strip on
    ' the right; the others keep the strip free only then, so there is no empty gutter when nobody scrolls.
    Private Sub Grid_VScrollShownChanged(sender As Object, e As EventArgs) _
        Handles gridBuget.VScrollShownChanged, gridRectificari.VScrollShownChanged
        Try
            Dim k_reserve As Boolean = gridBuget.VScrollShown OrElse gridRectificari.VScrollShown
            gridBuget.ReserveVScrollSpace = k_reserve
            gridRectificari.ReserveVScrollSpace = k_reserve
            gridTotal.ReserveVScrollSpace = k_reserve
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.Grid_VScrollShownChanged", ex)
        End Try
    End Sub

    Private Shared Sub AddQuarters(k_sums As Decimal(), k_row As KBotDataRow)
        For q As Integer = 0 To k_sums.Length - 1
            k_sums(q) += ReadAmount(k_row, QuarterColumns(q)).GetValueOrDefault()
        Next
    End Sub

    Private Sub FillGrids(k_data As BugetClasificatie)
        ApplyMode(False)
        SetCorrectionsTotal(True)
        gridRectificari.FooterCaption = _correctionsCaption
        gridBuget.BeginUpdate()
        Try
            gridBuget.ClearRows()
            For Each k_v As BudgetVersion In k_data.Budgets
                Dim k_row As KBotDataRow = gridBuget.AddRow()
                k_row(ColClsf) = _current.Cod
                k_row(ColId) = If(k_v.Id.HasValue, CObj(k_v.Id.Value), Nothing)
                k_row(ColStart) = If(k_v.StartDate.HasValue, CObj(k_v.StartDate.Value), Nothing)
                k_row(ColTrim1) = Box(k_v.Amounts.Trim1)
                k_row(ColTrim2) = Box(k_v.Amounts.Trim2)
                k_row(ColTrim3) = Box(k_v.Amounts.Trim3)
                k_row(ColTrim4) = Box(k_v.Amounts.Trim4)
                k_row(ColTotal) = k_v.Amounts.Total
                k_row(ColDelete) = DeleteCaption
            Next
        Finally
            gridBuget.EndUpdate()
        End Try

        gridRectificari.BeginUpdate()
        Try
            gridRectificari.ClearRows()
            For Each k_r As RectificareBugetara In k_data.Corrections
                Dim k_row As KBotDataRow = gridRectificari.AddRow()
                k_row(ColId) = If(k_r.Id.HasValue, CObj(k_r.Id.Value), Nothing)
                k_row(ColDocument) = k_r.Document
                k_row(ColData) = If(k_r.Data.HasValue, CObj(k_r.Data.Value), Nothing)
                k_row(ColTrim1) = Box(k_r.Amounts.Trim1)
                k_row(ColTrim2) = Box(k_r.Amounts.Trim2)
                k_row(ColTrim3) = Box(k_r.Amounts.Trim3)
                k_row(ColTrim4) = Box(k_r.Amounts.Trim4)
                k_row(ColTotal) = k_r.Amounts.Total
                k_row(ColDelete) = DeleteCaption
            Next
        Finally
            gridRectificari.EndUpdate()
        End Try
        gridBuget.ClearDirty()
        gridRectificari.ClearDirty()
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()

        gridBuget.Enabled = True
        gridRectificari.Enabled = True
        gridTotal.Enabled = True
        UpdateTotalRow()
        lblBuget.Text = $"Buget {_scopeAn.ToString(CultureInfo.InvariantCulture)} — {_current.Cod} ({_scopeSs})"
        lblRectificari.Text = $"Rectificări bugetare {_scopeAn.ToString(CultureInfo.InvariantCulture)}"
        tips.SetToolTipText(lblBuget, _current.Denumire)
        SetDirty(False)
    End Sub

    Private Sub ShowNoSelection()
        ApplyMode(False)
        SetCorrectionsTotal(False)
        _current = Nothing
        gridBuget.ClearRows()
        gridRectificari.ClearRows()
        gridTotal.ClearRows()
        gridBuget.Enabled = False
        gridRectificari.Enabled = False
        gridTotal.Enabled = False
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()
        lblBuget.Text = If(_scopeOk OrElse CurrentSs() IsNot Nothing, "Buget — alegeți o clasificație din arbore", "Buget — alegeți o sursă în bara de titlu")
        lblRectificari.Text = "Rectificări bugetare"
        SetDirty(False)
    End Sub

    Private Shared Function Box(k_value As Decimal?) As Object
        If k_value.HasValue Then Return k_value.Value
        Return Nothing
    End Function

    Private Shared Function ReadAmount(k_row As KBotDataRow, k_key As String) As Decimal?
        Dim k_v As Object = k_row(k_key)
        If k_v Is Nothing OrElse TypeOf k_v Is DBNull Then Return Nothing
        If TypeOf k_v Is Decimal Then Return DirectCast(k_v, Decimal)
        Dim k_parsed As Decimal
        If Decimal.TryParse(Convert.ToString(k_v, CultureInfo.CurrentCulture), NumberStyles.Number,
                            CultureInfo.CurrentCulture, k_parsed) Then Return k_parsed
        Return Nothing
    End Function

    Private Shared Function RowTotal(k_row As KBotDataRow) As Decimal
        Dim k_total As Decimal = 0D
        For Each k_key As String In QuarterColumns
            k_total += ReadAmount(k_row, k_key).GetValueOrDefault()
        Next
        Return k_total
    End Function

    ' Typed text -> the value the cell keeps: Decimal for the quarters, Date for «Data».
    Private Sub Grid_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) _
        Handles gridBuget.CellValidating, gridRectificari.CellValidating
        Try
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
            If QuarterColumns.Contains(e.ColumnKey) Then
                If k_text.Length = 0 Then
                    e.ProposedValue = Nothing
                    Return
                End If
                Dim k_amount As Decimal
                If Not Decimal.TryParse(k_text, NumberStyles.Number, CultureInfo.CurrentCulture, k_amount) Then
                    e.Cancel = True
                    SetStatus($"«{k_text}» nu este o sumă.")
                    Return
                End If
                e.ProposedValue = Math.Round(k_amount, 2)
            ElseIf e.ColumnKey = ColData OrElse e.ColumnKey = ColStart Then
                If k_text.Length = 0 Then
                    e.ProposedValue = Nothing
                    Return
                End If
                Dim k_day As Date
                If Not Date.TryParseExact(k_text, DateFormats, CultureInfo.CurrentCulture, DateTimeStyles.None, k_day) AndAlso
                   Not Date.TryParse(k_text, CultureInfo.CurrentCulture, DateTimeStyles.None, k_day) Then
                    e.Cancel = True
                    SetStatus($"«{k_text}» nu este o dată (zz.ll.aaaa).")
                    Return
                End If
                e.ProposedValue = k_day.Date
            ElseIf e.ColumnKey = ColDocument Then
                e.ProposedValue = k_text
            End If
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.Grid_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles gridBuget.CellValueChanged, gridRectificari.CellValueChanged
        Try
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            If QuarterColumns.Contains(e.ColumnKey) Then
                k_grid(ColTotal, e.RowIndex) = RowTotal(k_grid.Rows(e.RowIndex))
            End If
            If QuarterColumns.Contains(e.ColumnKey) OrElse e.ColumnKey = ColStart Then UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.Grid_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridBuget_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridBuget.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy Then Return
            If Not gridBuget.CommitPendingEdit() Then Return
            Dim k_row As KBotDataRow = gridBuget.AddRow()
            k_row(ColClsf) = _current.Cod
            ' The first version of a year starts on 01.01; the next ones default to today (inside the year), the day a budget is usually changed.
            k_row(ColStart) = If(gridBuget.RowCount > 1 AndAlso Date.Today.Year = _scopeAn, Date.Today, New Date(_scopeAn, 1, 1))
            k_row(ColTotal) = 0D
            k_row(ColDelete) = DeleteCaption
            k_row.IsDirty = True
            SetDirty(True)
            UpdateTotalRow()
            Dim k_index As Integer = gridBuget.RowCount - 1
            gridBuget.EnsureVisible(k_index)
            gridBuget.EditCell(ColStart, k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.GridBuget_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridBuget_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridBuget.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridBuget.RowCount Then Return
            Dim k_id As Object = gridBuget(ColId, e.RowIndex)
            If k_id IsNot Nothing Then _deletedBudgetIds.Add(Convert.ToInt32(k_id, CultureInfo.InvariantCulture))
            gridBuget.RemoveRowAt(e.RowIndex)
            UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.GridBuget_ButtonClick", ex)
        End Try
    End Sub

    Private Sub GridRectificari_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridRectificari.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy Then Return
            If Not gridRectificari.CommitPendingEdit() Then Return
            Dim k_row As KBotDataRow = gridRectificari.AddRow()
            k_row(ColDocument) = String.Empty
            k_row(ColData) = If(Date.Today.Year = _scopeAn, Date.Today, New Date(_scopeAn, 1, 1))
            k_row(ColTotal) = 0D
            k_row(ColDelete) = DeleteCaption
            k_row.IsDirty = True
            SetDirty(True)
            Dim k_index As Integer = gridRectificari.RowCount - 1
            gridRectificari.EnsureVisible(k_index)
            gridRectificari.EditCell(ColDocument, k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.GridRectificari_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridRectificari_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridRectificari.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridRectificari.RowCount Then Return
            Dim k_id As Object = gridRectificari(ColId, e.RowIndex)
            If k_id IsNot Nothing Then _deletedIds.Add(Convert.ToInt32(k_id, CultureInfo.InvariantCulture))
            gridRectificari.RemoveRowAt(e.RowIndex)
            UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.GridRectificari_ButtonClick", ex)
        End Try
    End Sub

    ' ── Save / pending ──────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ' ── Delete ──────────────────────────────────────────────────────────────────

    ' The classification of the leaf chosen in the tree (Nothing for a node above the leaves or nothing chosen). It does not need the unit of the
    ' title bar: the classification is deleted, not its budget.
    Private Function SelectedLeaf() As ClasificatieVenit
        Return If(_currentNode Is Nothing, Nothing, TryCast(_currentNode.Tag, ClasificatieVenit))
    End Function

    Private Sub UpdateDeleteButton()
        btnSterge.Enabled = _api IsNot Nothing AndAlso SelectedLeaf() IsNot Nothing AndAlso Not _dirty AndAlso Not _busy
    End Sub

    ' UI boundary: asks, deletes, reads the tree again. A classification that is used (budget, corrections, bank statements) is refused by the server,
    ' which names where; the message is shown as it comes.
    Private Async Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            Dim k_leaf As ClasificatieVenit = SelectedLeaf()
            If k_leaf Is Nothing OrElse _busy OrElse _dirty OrElse _api Is Nothing Then Return
            If KBotMessage.Show(Me, $"Ștergeți clasificația «{k_leaf.Cod}» ({k_leaf.Ss})?" & vbLf &
                                "O clasificație care are buget, rectificări sau este în extrase de cont nu se șterge.",
                                "Ștergere clasificație", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            Dim k_id As Integer = k_leaf.IdClsfV
            SetBusy(True, "Se șterge clasificația…")
            Try
                Await _gate.RunAsync(Function() _api.DeleteClasificatieVenitAsync(k_id, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return
            LoadAll(Nothing)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriView.BtnSterge_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Ștergere clasificație", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.BtnSterge_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Clasificația nu a putut fi ștearsă. Detalii în jurnalul de erori.",
                                 "Ștergere clasificație", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    ''' <summary>True when a budget or a correction was typed and not saved yet.</summary>
    Public ReadOnly Property HasPending As Boolean Implements IClasificatiiPage.HasPending
        Get
            Return _dirty
        End Get
    End Property

    ''' <summary>
    ''' Settles the unsaved changes before the window closes, the other page takes over or the unit changes: asks (Yes saves, No drops them and shows the
    ''' saved budget again, Cancel stays). True = the caller may go on.
    ''' </summary>
    Public Async Function CommitAsync() As Task(Of Boolean) Implements IClasificatiiPage.CommitAsync
        Try
            If Not _dirty Then Return True
            Dim k_answer As DialogResult = AskToSave()
            If k_answer = DialogResult.Cancel Then Return False
            If k_answer = DialogResult.Yes Then Return Await SaveAsync().ConfigureAwait(True)
            SetDirty(False)
            If _current IsNot Nothing Then Await LoadBudgetAsync(_current).ConfigureAwait(True)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.CommitAsync", ex)
            Return False
        End Try
    End Function

    ''' <summary>Writes the budget and the corrections of the current classification. True = saved (or nothing to save). Every failure is shown to the operator here.</summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        If _current Is Nothing OrElse _api Is Nothing OrElse Not _scopeOk Then Return True
        Try
            If Not gridBuget.CommitPendingEdit() OrElse Not gridRectificari.CommitPendingEdit() Then Return False

            Dim k_problem As String = Nothing
            Dim k_budgets As List(Of BudgetVersion) = ReadBudgets(k_problem)
            If k_problem IsNot Nothing Then
                KBotMessage.Show(Me, k_problem, "Buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
            Dim k_corrections As List(Of RectificareBugetara) = ReadCorrections(k_problem)
            If k_problem IsNot Nothing Then
                KBotMessage.Show(Me, k_problem, "Rectificări bugetare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim k_deletedBudgets As New List(Of Integer)(_deletedBudgetIds)
            Dim k_deleted As New List(Of Integer)(_deletedIds)
            Dim k_idClsfV As Integer = _current.IdClsfV
            Dim k_an As Integer = _scopeAn
            Dim k_ss As String = _scopeSs

            SetBusy(True, "Se salvează…")
            Dim k_saved As BugetClasificatie = Await _gate.RunAsync(
                Function() _api.SaveBugetVenitAsync(k_idClsfV, k_an, k_ss, k_budgets, k_deletedBudgets, k_corrections,
                                                    k_deleted, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return True
            FillGrids(k_saved)
            RememberSummary(k_idClsfV, k_saved)
            SetStatus($"Salvat: {_current.Cod}.")
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriView.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Bugetul nu a putut fi salvat. Detalii în jurnalul de erori.",
                                 "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' What a node above the leaves shows follows the save: the last version of the budget and the total of the corrections, as just written.
    Private Sub RememberSummary(k_idClsfV As Integer, k_data As BugetClasificatie)
        Dim k_last As BudgetVersion = k_data.Budgets.
            Where(Function(k_v) k_v.StartDate.HasValue).
            OrderByDescending(Function(k_v) k_v.StartDate.Value).
            FirstOrDefault()
        Dim k_corr As QuarterAmounts = Nothing
        If k_data.Corrections.Count > 0 Then
            k_corr = New QuarterAmounts() With {
                .Trim1 = k_data.Corrections.Sum(Function(k_r) k_r.Amounts.Trim1.GetValueOrDefault()),
                .Trim2 = k_data.Corrections.Sum(Function(k_r) k_r.Amounts.Trim2.GetValueOrDefault()),
                .Trim3 = k_data.Corrections.Sum(Function(k_r) k_r.Amounts.Trim3.GetValueOrDefault()),
                .Trim4 = k_data.Corrections.Sum(Function(k_r) k_r.Amounts.Trim4.GetValueOrDefault())}
        End If
        If k_last Is Nothing AndAlso k_corr Is Nothing Then
            _summary.Remove(k_idClsfV)
        Else
            _summary(k_idClsfV) = New VenitBugetSumar() With {.IdClsfV = k_idClsfV, .LastBudget = k_last, .CorrectionsTotal = k_corr}
        End If
    End Sub

    ' The budget versions as the grid shows them; the first incomplete or repeated row is reported and selected. «Inceput» is required, inside the year
    ' the budget was read for, and one per day.
    Private Function ReadBudgets(ByRef k_problem As String) As List(Of BudgetVersion)
        Dim k_list As New List(Of BudgetVersion)()
        Dim k_seen As New HashSet(Of Date)()
        For i As Integer = 0 To gridBuget.RowCount - 1
            Dim k_row As KBotDataRow = gridBuget.Rows(i)
            Dim k_startValue As Object = k_row(ColStart)
            If Not TypeOf k_startValue Is Date Then
                k_problem = $"Bugetul de la rândul {i + 1}: lipsește data de început."
            ElseIf DirectCast(k_startValue, Date).Year <> _scopeAn Then
                k_problem = $"Bugetul de la rândul {i + 1}: data de început {DirectCast(k_startValue, Date):dd.MM.yyyy} nu este în anul {_scopeAn}."
            ElseIf Not k_seen.Add(DirectCast(k_startValue, Date).Date) Then
                k_problem = $"Bugetul de la rândul {i + 1}: există deja o versiune care începe la {DirectCast(k_startValue, Date):dd.MM.yyyy}."
            End If
            If k_problem IsNot Nothing Then
                gridBuget.EnsureVisible(i)
                gridBuget.EditCell(ColStart, i)
                Return k_list
            End If
            Dim k_idValue As Object = k_row(ColId)
            k_list.Add(New BudgetVersion() With {
                .Id = If(k_idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(k_idValue, CultureInfo.InvariantCulture)),
                .StartDate = DirectCast(k_startValue, Date).Date,
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ReadAmount(k_row, ColTrim1), .Trim2 = ReadAmount(k_row, ColTrim2),
                    .Trim3 = ReadAmount(k_row, ColTrim3), .Trim4 = ReadAmount(k_row, ColTrim4)}})
        Next
        Return k_list
    End Function

    ' The corrections as the grid shows them; the first incomplete row is reported and selected.
    Private Function ReadCorrections(ByRef k_problem As String) As List(Of RectificareBugetara)
        Dim k_list As New List(Of RectificareBugetara)()
        For i As Integer = 0 To gridRectificari.RowCount - 1
            Dim k_row As KBotDataRow = gridRectificari.Rows(i)
            Dim k_document As String = Convert.ToString(k_row(ColDocument), CultureInfo.CurrentCulture).Trim()
            Dim k_dataValue As Object = k_row(ColData)
            If k_document.Length = 0 Then
                k_problem = $"Rândul {i + 1}: lipsește numărul documentului."
            ElseIf Not TypeOf k_dataValue Is Date Then
                k_problem = $"Rândul {i + 1}: lipsește data."
            ElseIf DirectCast(k_dataValue, Date).Year <> _scopeAn Then
                k_problem = $"Rândul {i + 1}: data {DirectCast(k_dataValue, Date):dd.MM.yyyy} nu este în anul {_scopeAn}."
            End If
            If k_problem IsNot Nothing Then
                gridRectificari.EnsureVisible(i)
                gridRectificari.EditCell(If(k_document.Length = 0, ColDocument, ColData), i)
                Return k_list
            End If
            Dim k_idValue As Object = k_row(ColId)
            k_list.Add(New RectificareBugetara() With {
                .Id = If(k_idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(k_idValue, CultureInfo.InvariantCulture)),
                .Document = k_document,
                .Data = DirectCast(k_dataValue, Date),
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ReadAmount(k_row, ColTrim1), .Trim2 = ReadAmount(k_row, ColTrim2),
                    .Trim3 = ReadAmount(k_row, ColTrim3), .Trim4 = ReadAmount(k_row, ColTrim4)}})
        Next
        Return k_list
    End Function

    Private Function AskToSave() As DialogResult
        Return KBotMessage.Show(Me, $"Bugetul clasificației {_current?.Cod} are modificări nesalvate." & vbLf &
                                "Le salvați?", "Clasificații venituri", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
    End Function

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetDirty(k_value As Boolean)
        _dirty = k_value
        btnSalveaza.Enabled = k_value AndAlso Not _busy AndAlso _current IsNot Nothing AndAlso _scopeOk
        UpdateDeleteButton()
    End Sub

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        UseWaitCursor = k_busy
        btnSalveaza.Enabled = _dirty AndAlso Not k_busy AndAlso _current IsNot Nothing AndAlso _scopeOk
        UpdateDeleteButton()
        If k_status IsNot Nothing Then SetStatus(k_status)
    End Sub

    ' An empty text shows the note about child subunits when there is one (it stays on while nothing else is said).
    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(String.IsNullOrEmpty(k_text), _childNote, k_text)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_p As ThemePalette = k_scheme.Palette
            BackColor = k_p.SurfaceAltColor
            For Each k_c As Control In {tlyMain, pnlCard, tlyBody, tlyRight, tlySubsol}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
            lblBuget.BackColor = k_p.SurfaceAltColor
            lblBuget.ForeColor = k_p.TextColor
            lblRectificari.BackColor = k_p.SurfaceAltColor
            lblRectificari.ForeColor = k_p.TextColor
            lblStare.ForeColor = k_p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            ButtonStyles.ApplySecondary(btnSterge, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriView.ApplyTheme", ex)
        End Try
    End Sub

End Class
