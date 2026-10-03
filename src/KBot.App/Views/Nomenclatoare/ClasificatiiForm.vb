Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
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
''' «Clasificatii bugetare» (slice 0087-01, operator 26.09.2026): the classifications of the
''' database as a tree Capitol > Subcapitol > Articol > Alineat (code in the first column, name in
''' the second), and for the chosen alineat its budget for the working year as VERSIONS (slice 0102:
''' one row = the budget from its «Început» date on, quarters 1-4 typed, no total; «+» in the footer
''' adds a version, «✕» removes one) and its corrections (typed straight in the grid, «+» adds a row,
''' «✕» removes one, totals in the footer). «Salveaza» writes both in one transaction. The «+»
''' in the tree footer opens <see cref="ClasificatiiAddForm"/>. A node ABOVE the leaves (slice 0105) shows
''' the same two grids as a read-only summary: a «Clsf» column that stretches, the LAST budget of each
''' classification under the node and the TOTAL of its corrections; no «Început», «Nr. doc.», «Data»,
''' «✕» or footer «+».
''' </summary>
Public Class ClasificatiiForm

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
    Private Shared ReadOnly QuarterColumns As String() = {ColTrim1, ColTrim2, ColTrim3, ColTrim4}
    Private Shared ReadOnly DateFormats As String() = {
        "d.M.yyyy", "dd.MM.yyyy", "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "yyyy-MM-dd", "ddMMyyyy"}

    Private ReadOnly _api As INomenclatoareApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _an As Integer
    Private ReadOnly _deletedIds As New List(Of Integer)()
    Private ReadOnly _deletedBudgetIds As New List(Of Integer)()
    Private _catalog As ClasificatiiCatalog
    Private _current As Clasificatie
    Private _currentNode As AdvancedTreeControl.TreeItem
    ' Slice 0105: the classifications with movement in the year (some quarter of a budget version or of a
    ' correction is not zero); the tree shows only them until «Arată toate clasificațiile» is ticked.
    Private _activeIds As New HashSet(Of Integer)()
    Private _suppressToggle As Boolean
    Private _dirty As Boolean
    Private _busy As Boolean
    Private _closeAfterSave As Boolean
    ' Slice 0105: a node above the leaves shows the read-only summary (Clsf column, last budget, corrections
    ' total); the designer's «+» icons and captions are kept here to put back for a leaf.
    Private _summaryMode As Boolean
    Private _plusIcon As Image
    Private _budgetCaption As String
    Private _correctionsCaption As String

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    Public Sub New(api As INomenclatoareApi, gate As ReauthGate, an As Integer)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(gate)
        InitializeComponent()
        _api = api
        _gate = gate
        _an = an
    End Sub

    Private Sub ClasificatiiForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            capBar.Text = $"K-BOT — Clasificații bugetare {_an}"
            _plusIcon = gridBuget.FooterRightIcon
            _budgetCaption = gridBuget.FooterCaption
            _correctionsCaption = gridRectificari.FooterCaption
            ' Slice 0104: «Trimite în Access» exists only for a unit that has the Access application
            ' (Setari.Access = 1) AND only where the Access component is installed.
            btnTrimiteAccess.Visible = AccessFeature.Enabled
            ShowNoSelection()
            LoadTree(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.ClasificatiiForm_Load", ex)
        End Try
    End Sub

    ' ── Tree ────────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadTree(selectIdClsf As Integer?)
        Try
            SetBusy(True, "Se încarcă clasificațiile…")
            Dim catalog As ClasificatiiCatalog = Await _gate.RunAsync(
                Function() _api.GetClasificatiiAsync(CancellationToken.None)).ConfigureAwait(True)
            Dim summary As IReadOnlyList(Of BudgetSummaryRow) = Await _gate.RunAsync(
                Function() _api.GetBudgetSummaryAsync(_an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _catalog = catalog
            _activeIds = New HashSet(Of Integer)(summary.Where(Function(r) r.Active).Select(Function(r) r.IdClsf))
            BuildTree(catalog, selectIdClsf)
            SetStatus(CountText(catalog))
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.LoadTree", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.LoadTree", ex)
            If Not IsDisposed Then SetStatus("Clasificațiile nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub BuildTree(catalog As ClasificatiiCatalog, selectIdClsf As Integer?)
        tree.Clear()
        _currentNode = Nothing
        Dim folder As Image = My.Resources.Resources.folder_open
        Dim toSelect As AdvancedTreeControl.TreeItem = Nothing

        Dim chapters = VisibleItems(catalog).GroupBy(Function(c) New With {Key c.Capitol, Key c.Ss}).
                                     OrderBy(Function(gp) gp.Key.Capitol, StringComparer.Ordinal).
                                     ThenBy(Function(gp) gp.Key.Ss, StringComparer.Ordinal)
        For Each chapter In chapters
            Dim capitol As String = chapter.Key.Capitol
            Dim ss As String = chapter.Key.Ss
            Dim chapterName As String = NameOr(catalog.CapitolNames, Left2(capitol))
            Dim ssName As String = NameOr(catalog.SsNames, ss)
            Dim root As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"C|{capitol}|{ss}",
                $"{capitol} ({ss})~~~{Escape(JoinNames(chapterName, ssName))}",
                pLeftIconClosed:=folder, pLeftIconOpen:=folder, pExpanded:=True)
            root.Bold = True

            For Each sub1 In chapter.GroupBy(Function(c) c.Subcapitol).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                Dim subName As String = NameOr(catalog.SubcapitolNames, Left2(capitol) & sub1.Key.Replace(".", String.Empty))
                Dim subNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                    $"S|{capitol}|{ss}|{sub1.Key}", $"{sub1.Key}~~~{Escape(subName)}", root,
                    pLeftIconClosed:=folder, pLeftIconOpen:=folder)

                For Each art In sub1.GroupBy(Function(c) c.Articol).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                    Dim artNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                        $"A|{capitol}|{ss}|{sub1.Key}|{art.Key}",
                        $"{art.Key}~~~{Escape(NameOr(catalog.ArticolNames, art.Key))}", subNode,
                        pLeftIconClosed:=folder, pLeftIconOpen:=folder)

                    For Each c As Clasificatie In art.OrderBy(Function(x) x.Alineat, StringComparer.Ordinal)
                        Dim leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                            $"L|{c.IdClsf}", $"{c.Articol}.{c.Alineat}~~~{Escape(c.Denumire)}", artNode)
                        leaf.Tag = c
                        leaf.Tooltip = c.Clsf
                        If selectIdClsf.HasValue AndAlso c.IdClsf = selectIdClsf.Value Then toSelect = leaf
                    Next
                Next
            Next
        Next

        If toSelect IsNot Nothing Then
            tree.SelectAndReveal(toSelect)
            _currentNode = toSelect
        End If
        tree.Invalidate()
    End Sub

    ' The classifications the tree shows: all of them, or only those with movement in the year.
    Private Function VisibleItems(catalog As ClasificatiiCatalog) As List(Of Clasificatie)
        If chkToate.Checked Then Return catalog.Items
        Return catalog.Items.Where(Function(c) _activeIds.Contains(c.IdClsf)).ToList()
    End Function

    Private Function CountText(catalog As ClasificatiiCatalog) As String
        Dim shown As Integer = VisibleItems(catalog).Count
        Return If(chkToate.Checked, $"{shown} clasificații.",
                  $"{shown} din {catalog.Items.Count} clasificații, cele cu mișcare în {_an}.")
    End Function

    ' UI boundary: the tree is rebuilt from the catalog already read; the open budget stays on screen.
    Private Sub ChkToate_CheckedChanged(sender As Object, e As EventArgs) Handles chkToate.CheckedChanged
        Try
            If _suppressToggle OrElse _catalog Is Nothing OrElse _busy Then Return
            BuildTree(_catalog, If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf))
            SetStatus(CountText(_catalog))
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.ChkToate_CheckedChanged", ex)
        End Try
    End Sub

    Private Shared Function Left2(capitol As String) As String
        Return If(capitol.Length >= 2, capitol.Substring(0, 2), capitol)
    End Function

    Private Shared Function NameOr(names As Dictionary(Of String, String), key As String) As String
        Dim name As String = Nothing
        If names IsNot Nothing AndAlso names.TryGetValue(key, name) AndAlso Not String.IsNullOrWhiteSpace(name) Then
            Return name.Trim()
        End If
        Return String.Empty
    End Function

    Private Shared Function JoinNames(first As String, second As String) As String
        If String.IsNullOrEmpty(first) Then Return second
        If String.IsNullOrEmpty(second) Then Return first
        Return first & " — " & second
    End Function

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(text As String) As String
        Return If(text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing OrElse pNode Is _currentNode Then Return
            Dim previous As AdvancedTreeControl.TreeItem = _currentNode
            ChangeSelection(pNode, previous)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' UI boundary: asks about unsaved changes, then loads the node's budget.
    Private Async Sub ChangeSelection(node As AdvancedTreeControl.TreeItem, previous As AdvancedTreeControl.TreeItem)
        Try
            If _busy Then
                If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                Return
            End If
            If _dirty Then
                Dim answer As DialogResult = AskToSave()
                If answer = DialogResult.Cancel Then
                    If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                    Return
                End If
                If answer = DialogResult.Yes Then
                    Dim saved As Boolean = Await SaveAsync().ConfigureAwait(True)
                    If Not saved Then
                        If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                        Return
                    End If
                End If
            End If

            _currentNode = node
            Dim c As Clasificatie = TryCast(node.Tag, Clasificatie)
            If c Is Nothing Then
                Dim leaves As List(Of Clasificatie) = LeavesUnder(node)
                If leaves Is Nothing Then
                    ShowNoSelection()
                    Return
                End If
                Await LoadSummaryAsync(node, leaves).ConfigureAwait(True)
                Return
            End If
            Await LoadBudgetAsync(c).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.ChangeSelection", ex)
        End Try
    End Sub

    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            If _busy OrElse _api Is Nothing Then Return
            Using f As New ClasificatiiAddForm(_api, _gate, _an)
                If f.ShowDialog(Me) <> DialogResult.OK Then Return
                If f.Result IsNot Nothing AndAlso f.Result.Inserted > 0 Then
                    ' New classifications have no movement yet: show them, or the operator would not find them.
                    _suppressToggle = True
                    chkToate.Checked = True
                    _suppressToggle = False
                    LoadTree(If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf))
                End If
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Tree_FooterRightIconClicked", ex)
            KBotMessage.Show(Me, "Fereastra de adăugare nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Clasificații", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── Budget + corrections ────────────────────────────────────────────────────

    ' Risky (HTTP): shows its own error in the status line; never throws to the caller.
    Private Async Function LoadBudgetAsync(c As Clasificatie) As Task
        Try
            SetBusy(True, "Se încarcă bugetul…")
            Dim data As BugetClasificatie = Await _gate.RunAsync(
                Function() _api.GetBugetClasificatieAsync(c.IdClsf, _an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _current = c
            FillGrids(data)
            SetStatus(String.Empty)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus("Bugetul nu a putut fi încărcat. Detalii în jurnalul de erori.")
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The classifications under a chapter («C|capitol|ss»), sub-chapter («S|…|sub») or article («A|…|art»)
    ' node, in tree order; Nothing for any other node.
    Private Function LeavesUnder(node As AdvancedTreeControl.TreeItem) As List(Of Clasificatie)
        If _catalog Is Nothing OrElse String.IsNullOrEmpty(node.Key) Then Return Nothing
        Dim parts As String() = node.Key.Split("|"c)
        Dim level As String = parts(0)
        Dim minParts As Integer = If(level = "C", 3, If(level = "S", 4, If(level = "A", 5, Integer.MaxValue)))
        If parts.Length <> minParts Then Return Nothing
        Return VisibleItems(_catalog).
            Where(Function(x) x.Capitol = parts(1) AndAlso x.Ss = parts(2) AndAlso
                              (parts.Length < 4 OrElse x.Subcapitol = parts(3)) AndAlso
                              (parts.Length < 5 OrElse x.Articol = parts(4))).
            OrderBy(Function(x) x.Subcapitol, StringComparer.Ordinal).
            ThenBy(Function(x) x.Articol, StringComparer.Ordinal).
            ThenBy(Function(x) x.Alineat, StringComparer.Ordinal).
            ToList()
    End Function

    ' Risky (HTTP): shows its own error in the status line; never throws to the caller.
    Private Async Function LoadSummaryAsync(node As AdvancedTreeControl.TreeItem, leaves As List(Of Clasificatie)) As Task
        Try
            SetBusy(True, "Se încarcă bugetele…")
            Dim summary As IReadOnlyList(Of BudgetSummaryRow) = Await _gate.RunAsync(
                Function() _api.GetBudgetSummaryAsync(_an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _current = Nothing
            FillSummary(node, leaves, summary)
            SetStatus(String.Empty)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.LoadSummaryAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.LoadSummaryAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus("Bugetele nu au putut fi încărcate. Detalii în jurnalul de erori.")
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' Budget grid: every classification under the node with its LAST version; corrections grid: the
    ' classifications that have corrections, with the total of the year.
    Private Sub FillSummary(node As AdvancedTreeControl.TreeItem, leaves As List(Of Clasificatie),
                            summary As IReadOnlyList(Of BudgetSummaryRow))
        ApplyMode(True)
        Dim byId As New Dictionary(Of Integer, BudgetSummaryRow)()
        For Each s As BudgetSummaryRow In summary
            byId(s.IdClsf) = s
        Next

        gridBuget.BeginUpdate()
        gridRectificari.BeginUpdate()
        Try
            gridBuget.ClearRows()
            gridRectificari.ClearRows()
            For Each c As Clasificatie In leaves
                Dim s As BudgetSummaryRow = Nothing
                byId.TryGetValue(c.IdClsf, s)

                Dim row As KBotDataRow = gridBuget.AddRow()
                row(ColClsf) = c.Clsf
                If s IsNot Nothing AndAlso s.LastBudget IsNot Nothing Then
                    row(ColTrim1) = Box(s.LastBudget.Amounts.Trim1)
                    row(ColTrim2) = Box(s.LastBudget.Amounts.Trim2)
                    row(ColTrim3) = Box(s.LastBudget.Amounts.Trim3)
                    row(ColTrim4) = Box(s.LastBudget.Amounts.Trim4)
                End If

                If s IsNot Nothing AndAlso s.CorrectionsTotal IsNot Nothing Then
                    Dim corr As KBotDataRow = gridRectificari.AddRow()
                    corr(ColClsf) = c.Clsf
                    corr(ColTrim1) = Box(s.CorrectionsTotal.Trim1)
                    corr(ColTrim2) = Box(s.CorrectionsTotal.Trim2)
                    corr(ColTrim3) = Box(s.CorrectionsTotal.Trim3)
                    corr(ColTrim4) = Box(s.CorrectionsTotal.Trim4)
                    corr(ColTotal) = s.CorrectionsTotal.Total
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
        Dim parts As String() = node.Key.Split("|"c)
        Dim code As String = String.Join(".", parts.Skip(1).Where(Function(p, i) i <> 1))   ' capitol[.sub[.articol]], without the source
        lblBuget.Text = $"Buget {_an} — {code} ({parts(2)}): {leaves.Count} clasificații"
        lblRectificari.Text = $"Rectificări bugetare {_an} — total pe clasificație"
        Dim split As Integer = If(node.Caption, String.Empty).IndexOf("~~~", StringComparison.Ordinal)
        tips.SetToolTipText(lblBuget, If(split >= 0, node.Caption.Substring(split + 3), code))
        SetDirty(False)
    End Sub

    ' The summary is read-only: it has the «Clsf» column (the one that stretches) and no «Început»,
    ' «Nr. doc.», «Data», «✕» or footer «+»; a leaf has the editable grids as the designer made them.
    Private Sub ApplyMode(summary As Boolean)
        If summary = _summaryMode Then Return
        _summaryMode = summary
        SetColumn(gridBuget, ColClsf, summary)
        SetColumn(gridBuget, ColStart, Not summary)
        SetColumn(gridBuget, ColDelete, Not summary)
        SetColumn(gridRectificari, ColClsf, summary)
        SetColumn(gridRectificari, ColDocument, Not summary)
        SetColumn(gridRectificari, ColData, Not summary)
        SetColumn(gridRectificari, ColDelete, Not summary)
        For Each grid As KBotDataView In {gridBuget, gridRectificari}
            grid.ReadOnlyGrid = summary
            grid.FooterRightIcon = If(summary, Nothing, _plusIcon)
        Next
        gridRectificari.FillColumnKey = If(summary, ColClsf, ColDocument)
        gridBuget.FooterCaption = If(summary, "Ultimul buget al fiecărei clasificații", _budgetCaption)
        gridRectificari.FooterCaption = _correctionsCaption
    End Sub

    Private Shared Sub SetColumn(grid As KBotDataView, key As String, shown As Boolean)
        Dim column As KBotDataColumn = grid.Columns.First(Function(c) String.Equals(c.Key, key, StringComparison.Ordinal))
        column.Visible = If(shown, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
    End Sub

    Private Sub FillGrids(data As BugetClasificatie)
        ApplyMode(False)
        gridBuget.BeginUpdate()
        Try
            gridBuget.ClearRows()
            For Each v As BudgetVersion In data.Budgets
                Dim row As KBotDataRow = gridBuget.AddRow()
                row(ColId) = If(v.Id.HasValue, CObj(v.Id.Value), Nothing)
                row(ColStart) = If(v.StartDate.HasValue, CObj(v.StartDate.Value), Nothing)
                row(ColTrim1) = Box(v.Amounts.Trim1)
                row(ColTrim2) = Box(v.Amounts.Trim2)
                row(ColTrim3) = Box(v.Amounts.Trim3)
                row(ColTrim4) = Box(v.Amounts.Trim4)
                row(ColDelete) = DeleteCaption
            Next
        Finally
            gridBuget.EndUpdate()
        End Try

        gridRectificari.BeginUpdate()
        Try
            gridRectificari.ClearRows()
            For Each r As RectificareBugetara In data.Corrections
                Dim row As KBotDataRow = gridRectificari.AddRow()
                row(ColId) = If(r.Id.HasValue, CObj(r.Id.Value), Nothing)
                row(ColDocument) = r.Document
                row(ColData) = If(r.Data.HasValue, CObj(r.Data.Value), Nothing)
                row(ColTrim1) = Box(r.Amounts.Trim1)
                row(ColTrim2) = Box(r.Amounts.Trim2)
                row(ColTrim3) = Box(r.Amounts.Trim3)
                row(ColTrim4) = Box(r.Amounts.Trim4)
                row(ColTotal) = r.Amounts.Total
                row(ColDelete) = DeleteCaption
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
        lblBuget.Text = $"Buget {_an} — {_current.Clsf}"
        lblRectificari.Text = $"Rectificări bugetare {_an}"
        tips.SetToolTipText(lblBuget, _current.Denumire)
        SetDirty(False)
    End Sub

    Private Sub ShowNoSelection()
        ApplyMode(False)
        _current = Nothing
        gridBuget.ClearRows()
        gridRectificari.ClearRows()
        gridBuget.Enabled = False
        gridRectificari.Enabled = False
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()
        lblBuget.Text = "Buget — alegeți un alineat din arbore"
        lblRectificari.Text = "Rectificări bugetare"
        SetDirty(False)
    End Sub

    ' Quarter by quarter, never through a total: +1000 and -1000 total 0 and still count as movement.
    Private Shared Function HasMovement(data As BugetClasificatie) As Boolean
        Return data.Budgets.Any(Function(b) AnyQuarter(b.Amounts)) OrElse
               data.Corrections.Any(Function(r) AnyQuarter(r.Amounts))
    End Function

    Private Shared Function AnyQuarter(a As QuarterAmounts) As Boolean
        Return a.Trim1.GetValueOrDefault() <> 0D OrElse a.Trim2.GetValueOrDefault() <> 0D OrElse
               a.Trim3.GetValueOrDefault() <> 0D OrElse a.Trim4.GetValueOrDefault() <> 0D
    End Function

    Private Shared Function Box(value As Decimal?) As Object
        If value.HasValue Then Return value.Value
        Return Nothing
    End Function

    Private Shared Function ReadAmount(row As KBotDataRow, key As String) As Decimal?
        Dim v As Object = row(key)
        If v Is Nothing OrElse TypeOf v Is DBNull Then Return Nothing
        If TypeOf v Is Decimal Then Return DirectCast(v, Decimal)
        Dim parsed As Decimal
        If Decimal.TryParse(Convert.ToString(v, CultureInfo.CurrentCulture), NumberStyles.Number,
                            CultureInfo.CurrentCulture, parsed) Then Return parsed
        Return Nothing
    End Function

    Private Shared Function RowTotal(row As KBotDataRow) As Decimal
        Dim total As Decimal = 0D
        For Each key As String In QuarterColumns
            total += ReadAmount(row, key).GetValueOrDefault()
        Next
        Return total
    End Function

    ' Typed text -> the value the cell keeps: Decimal for the quarters, Date for «Data».
    Private Sub Grid_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) _
        Handles gridBuget.CellValidating, gridRectificari.CellValidating
        Try
            Dim text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
            If QuarterColumns.Contains(e.ColumnKey) Then
                If text.Length = 0 Then
                    e.ProposedValue = Nothing
                    Return
                End If
                Dim amount As Decimal
                If Not Decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, amount) Then
                    e.Cancel = True
                    SetStatus($"«{text}» nu este o sumă.")
                    Return
                End If
                e.ProposedValue = Math.Round(amount, 2)
            ElseIf e.ColumnKey = ColData OrElse e.ColumnKey = ColStart Then
                If text.Length = 0 Then
                    e.ProposedValue = Nothing
                    Return
                End If
                Dim day As Date
                If Not Date.TryParseExact(text, DateFormats, CultureInfo.CurrentCulture, DateTimeStyles.None, day) AndAlso
                   Not Date.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, day) Then
                    e.Cancel = True
                    SetStatus($"«{text}» nu este o dată (zz.ll.aaaa).")
                    Return
                End If
                e.ProposedValue = day.Date
            ElseIf e.ColumnKey = ColDocument Then
                e.ProposedValue = text
            End If
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Grid_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles gridBuget.CellValueChanged, gridRectificari.CellValueChanged
        Try
            Dim grid As KBotDataView = DirectCast(sender, KBotDataView)
            ' Only a correction's row has a «Total» column; a budget has none (slice 0102).
            If QuarterColumns.Contains(e.ColumnKey) AndAlso grid Is gridRectificari Then
                grid(ColTotal, e.RowIndex) = RowTotal(grid.Rows(e.RowIndex))
            End If
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.Grid_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridBuget_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridBuget.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy Then Return
            If Not gridBuget.CommitPendingEdit() Then Return
            Dim row As KBotDataRow = gridBuget.AddRow()
            ' The first version of a year starts on 01.01; the next ones default to today (inside
            ' the year), the day a budget is usually changed.
            row(ColStart) = If(gridBuget.RowCount > 1 AndAlso Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
            row(ColDelete) = DeleteCaption
            row.IsDirty = True
            SetDirty(True)
            Dim index As Integer = gridBuget.RowCount - 1
            gridBuget.EnsureVisible(index)
            gridBuget.EditCell(ColStart, index)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridBuget_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridBuget_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridBuget.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridBuget.RowCount Then Return
            Dim id As Object = gridBuget(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedBudgetIds.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridBuget.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridBuget_ButtonClick", ex)
        End Try
    End Sub

    Private Sub GridRectificari_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridRectificari.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy Then Return
            If Not gridRectificari.CommitPendingEdit() Then Return
            Dim row As KBotDataRow = gridRectificari.AddRow()
            row(ColDocument) = String.Empty
            row(ColData) = If(Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
            row(ColTotal) = 0D
            row(ColDelete) = DeleteCaption
            row.IsDirty = True
            SetDirty(True)
            Dim index As Integer = gridRectificari.RowCount - 1
            gridRectificari.EnsureVisible(index)
            gridRectificari.EditCell(ColDocument, index)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridRectificari_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridRectificari_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridRectificari.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridRectificari.RowCount Then Return
            Dim id As Object = gridRectificari(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedIds.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridRectificari.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.GridRectificari_ButtonClick", ex)
        End Try
    End Sub

    ' ── Save / close ────────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Writes the budget and the corrections of the current classification. True = saved (or
    ''' nothing to save). Every failure is shown to the operator here.
    ''' </summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        If _current Is Nothing OrElse _api Is Nothing Then Return True
        Try
            If Not gridBuget.CommitPendingEdit() OrElse Not gridRectificari.CommitPendingEdit() Then Return False

            Dim problem As String = Nothing
            Dim budgets As List(Of BudgetVersion) = ReadBudgets(problem)
            If problem IsNot Nothing Then
                KBotMessage.Show(Me, problem, "Buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If
            Dim corrections As List(Of RectificareBugetara) = ReadCorrections(problem)
            If problem IsNot Nothing Then
                KBotMessage.Show(Me, problem, "Rectificări bugetare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim deletedBudgets As New List(Of Integer)(_deletedBudgetIds)
            Dim deleted As New List(Of Integer)(_deletedIds)
            Dim idClsf As Integer = _current.IdClsf

            SetBusy(True, "Se salvează…")
            Dim saved As BugetClasificatie = Await _gate.RunAsync(
                Function() _api.SaveBugetClasificatieAsync(idClsf, _an, budgets, deletedBudgets, corrections,
                                                           deleted, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return True
            FillGrids(saved)
            If HasMovement(saved) Then _activeIds.Add(idClsf) Else _activeIds.Remove(idClsf)
            SetStatus($"Salvat: {_current.Clsf}.")
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Bugetul nu a putut fi salvat. Detalii în jurnalul de erori.",
                                 "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The budget versions as the grid shows them; the first incomplete or repeated row is reported and
    ' selected. «Început» is required, inside the working year, and one per day.
    Private Function ReadBudgets(ByRef problem As String) As List(Of BudgetVersion)
        Dim list As New List(Of BudgetVersion)()
        Dim seen As New HashSet(Of Date)()
        For i As Integer = 0 To gridBuget.RowCount - 1
            Dim row As KBotDataRow = gridBuget.Rows(i)
            Dim startValue As Object = row(ColStart)
            If Not TypeOf startValue Is Date Then
                problem = $"Bugetul de la rândul {i + 1}: lipsește data de început."
            ElseIf DirectCast(startValue, Date).Year <> _an Then
                problem = $"Bugetul de la rândul {i + 1}: data de început {DirectCast(startValue, Date):dd.MM.yyyy} nu este în anul {_an}."
            ElseIf Not seen.Add(DirectCast(startValue, Date).Date) Then
                problem = $"Bugetul de la rândul {i + 1}: există deja o versiune care începe la {DirectCast(startValue, Date):dd.MM.yyyy}."
            End If
            If problem IsNot Nothing Then
                gridBuget.EnsureVisible(i)
                gridBuget.EditCell(ColStart, i)
                Return list
            End If
            Dim idValue As Object = row(ColId)
            list.Add(New BudgetVersion() With {
                .Id = If(idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(idValue, CultureInfo.InvariantCulture)),
                .StartDate = DirectCast(startValue, Date).Date,
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ReadAmount(row, ColTrim1), .Trim2 = ReadAmount(row, ColTrim2),
                    .Trim3 = ReadAmount(row, ColTrim3), .Trim4 = ReadAmount(row, ColTrim4)}})
        Next
        Return list
    End Function

    ' The corrections as the grid shows them; the first incomplete row is reported and selected.
    Private Function ReadCorrections(ByRef problem As String) As List(Of RectificareBugetara)
        Dim list As New List(Of RectificareBugetara)()
        For i As Integer = 0 To gridRectificari.RowCount - 1
            Dim row As KBotDataRow = gridRectificari.Rows(i)
            Dim document As String = Convert.ToString(row(ColDocument), CultureInfo.CurrentCulture).Trim()
            Dim dataValue As Object = row(ColData)
            If document.Length = 0 Then
                problem = $"Rândul {i + 1}: lipsește numărul documentului."
            ElseIf Not TypeOf dataValue Is Date Then
                problem = $"Rândul {i + 1}: lipsește data."
            ElseIf DirectCast(dataValue, Date).Year <> _an Then
                problem = $"Rândul {i + 1}: data {DirectCast(dataValue, Date):dd.MM.yyyy} nu este în anul {_an}."
            End If
            If problem IsNot Nothing Then
                gridRectificari.EnsureVisible(i)
                gridRectificari.EditCell(If(document.Length = 0, ColDocument, ColData), i)
                Return list
            End If
            Dim idValue As Object = row(ColId)
            list.Add(New RectificareBugetara() With {
                .Id = If(idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(idValue, CultureInfo.InvariantCulture)),
                .Document = document,
                .Data = DirectCast(dataValue, Date),
                .Amounts = New QuarterAmounts() With {
                    .Trim1 = ReadAmount(row, ColTrim1), .Trim2 = ReadAmount(row, ColTrim2),
                    .Trim3 = ReadAmount(row, ColTrim3), .Trim4 = ReadAmount(row, ColTrim4)}})
        Next
        Return list
    End Function

    Private Function AskToSave() As DialogResult
        Return KBotMessage.Show(Me, $"Bugetul clasificației {_current?.Clsf} are modificări nesalvate." & vbLf &
                                "Le salvați?", "Clasificații", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
    End Function

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _dirty AndAlso Not _closeAfterSave AndAlso e.CloseReason = CloseReason.UserClosing Then
                Dim answer As DialogResult = AskToSave()
                If answer = DialogResult.Cancel Then
                    e.Cancel = True
                ElseIf answer = DialogResult.Yes Then
                    e.Cancel = True
                    SaveThenClose()
                End If
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.OnFormClosing", ex)
        End Try
    End Sub

    ' UI boundary: the save cannot be awaited inside FormClosing; the window closes after it.
    Private Async Sub SaveThenClose()
        Try
            If Await SaveAsync().ConfigureAwait(True) AndAlso Not IsDisposed Then
                _closeAfterSave = True
                Close()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.SaveThenClose", ex)
        End Try
    End Sub

    ' ── Budget check + «Trimite in Access» (slice 0103-04 / 0103-06) ─────────────────

    ' UI boundary: the check shows its own errors.
    Private Async Sub BtnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            If _busy OrElse _api Is Nothing Then Return
            SetBusy(True, "Se verifică bugetul față de FOREXE…")
            Try
                Await BudgetCheckForm.RunAsync(Me, _api, _gate, showWhenEqual:=True).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, String.Empty)
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.BtnVerifica_Click", ex)
        End Try
    End Sub

    ' UI boundary: writes the SAVED budget + rectifications of the chosen classification into the
    ' Access file of its unit; every failure is told to the operator.
    Private Async Sub BtnTrimiteAccess_Click(sender As Object, e As EventArgs) Handles btnTrimiteAccess.Click
        Try
            Dim c As Clasificatie = _current
            If _busy OrElse _api Is Nothing OrElse c Is Nothing OrElse _dirty Then Return
            ' Slice 0104: the Access code lives in KBot.Access, which only the package for Access clients
            ' contains. The button is hidden without it; this guards a call that still gets here.
            Dim bridge As IAccessBridge = AccessBridge.Instance
            If bridge Is Nothing OrElse Not AccessFeature.Enabled Then
                KBotMessage.Show(Me, "Această instalare nu are componenta Access.",
                                 "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If Not c.IdUnitate.HasValue OrElse c.IdClsfAcc = 0 Then
                KBotMessage.Show(Me, "Clasificația nu are unitate sau id Access; nu se poate trimite în Access.",
                                 "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If KBotMessage.Show(Me,
                    $"Se scriu în baza Access a unității bugetul în vigoare azi și rectificările anului {_an} " &
                    $"ale clasificației {c.Clsf}. Rectificările șterse aici nu se șterg din Access." &
                    Environment.NewLine & "Continuați?",
                    "Trimite în Access", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            SetBusy(True, "Se trimite în Access…")
            Try
                ' Read back from the server: what was SAVED is what is sent.
                Dim data As BugetClasificatie = Await _gate.RunAsync(
                    Function() _api.GetBugetClasificatieAsync(c.IdClsf, _an, CancellationToken.None)).ConfigureAwait(True)
                Dim today As Date = Date.Today
                Dim inForce As BudgetVersion = data.Budgets.
                    Where(Function(v) v.StartDate.HasValue AndAlso v.StartDate.Value <= today).
                    OrderByDescending(Function(v) v.StartDate.Value).
                    FirstOrDefault()
                Dim idUnitate As Integer = c.IdUnitate.Value
                Dim idAcc As Integer = c.IdClsfAcc
                Dim registry As String = AppSettings.Current.AccessRegistryPath   ' the operator's, «Setari ▸ Access»
                Dim result As AccessSendResult = Await Task.Run(
                    Function()
                        Dim file As String = bridge.ResolveUnitFile(registry, idUnitate)
                        Return bridge.SendBudget(file, idAcc, inForce, data.Corrections)
                    End Function).ConfigureAwait(True)
                If IsDisposed Then Return
                Dim lines As String = $"Trimis în «{result.File}»:" & Environment.NewLine &
                    If(inForce Is Nothing, "bugetul: nicio versiune în vigoare azi, nu s-a scris",
                       If(result.BudgetUpdated = 0, "bugetul: clasificația nu există în Access",
                          $"bugetul versiunii din {inForce.StartDate.Value:dd.MM.yyyy}: scris")) & Environment.NewLine &
                    $"rectificări adăugate: {result.RectificariInserted}, actualizate: {result.RectificariUpdated}."
                KBotMessage.Show(Me, lines, "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Finally
                If Not IsDisposed Then SetBusy(False, String.Empty)
            End Try
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiForm.BtnTrimiteAccess_Click", ex)
            KBotMessage.Show(Me, ex.Message, "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As InvalidOperationException
            GlobalErrorLog.Write("ClasificatiiForm.BtnTrimiteAccess_Click", ex)
            KBotMessage.Show(Me, ex.Message, "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.BtnTrimiteAccess_Click", ex)
            KBotMessage.Show(Me, "Trimiterea în Access a eșuat. Detalii în jurnalul de erori.",
                             "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetDirty(value As Boolean)
        _dirty = value
        btnSalveaza.Enabled = value AndAlso Not _busy AndAlso _current IsNot Nothing
        btnTrimiteAccess.Enabled = Not value AndAlso Not _busy AndAlso _current IsNot Nothing
    End Sub

    Private Sub SetBusy(busy As Boolean, status As String)
        _busy = busy
        UseWaitCursor = busy
        btnSalveaza.Enabled = _dirty AndAlso Not busy AndAlso _current IsNot Nothing
        btnTrimiteAccess.Enabled = Not _dirty AndAlso Not busy AndAlso _current IsNot Nothing
        btnVerifica.Enabled = Not busy
        If status IsNot Nothing Then SetStatus(status)
    End Sub

    Private Sub SetStatus(text As String)
        lblStare.Text = If(text, String.Empty)
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            Dim p As ThemePalette = scheme.Palette
            ' The form background IS the 1px outline of the window (Padding(1)).
            BackColor = p.BorderColor
            tlyMain.BackColor = p.SurfaceAltColor
            pnlCard.BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyRight.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            chkToate.BackColor = p.SurfaceAltColor
            chkToate.ForeColor = p.TextColor
            lblBuget.BackColor = p.SurfaceAltColor
            lblBuget.ForeColor = p.TextColor
            lblRectificari.BackColor = p.SurfaceAltColor
            lblRectificari.ForeColor = p.TextColor
            lblStare.ForeColor = p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            ButtonStyles.ApplySecondary(btnInchide, scheme)
            ButtonStyles.ApplySecondary(btnVerifica, scheme)
            ButtonStyles.ApplySecondary(btnTrimiteAccess, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
