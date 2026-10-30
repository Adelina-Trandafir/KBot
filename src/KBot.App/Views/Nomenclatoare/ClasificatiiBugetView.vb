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
''' «Buget» page of the «Clasificatii» window (slice AV01-11; the window itself is <see cref="ClasificatiiForm"/>, which hosts this view and the
''' revenue view). Before that it was the whole «Clasificatii bugetare» window (slice 0087-01, operator 26.09.2026): the classifications of the
''' database as a tree Capitol > Subcapitol > Articol > Alineat (code in the first column, name in
''' the second), and for the chosen alineat its budget for the working year as VERSIONS (slice 0102:
''' one row = the budget from its «Inceput» date on, quarters 1-4 typed, no total; «+» in the footer
''' adds a version, «✕» removes one) and its corrections (typed straight in the grid, «+» adds a row,
''' «✕» removes one, totals in the footer). «Salveaza» writes both in one transaction. The «+»
''' in the tree footer opens <see cref="ClasificatiiAddForm"/>. A node ABOVE the leaves (slice 0105) shows
''' the same two grids as a read-only summary: a «Clsf» column that stretches, the LAST budget of each
''' classification under the node and the TOTAL of its corrections; no «Inceput», «Nr. doc.», «Data»,
''' «✕» or footer «+». Slice 0107-03: under both grids a one-row grid (<c>gridTotal</c>, no header or footer) adds the
''' LAST budget (newest «Inceput») and ALL the corrections; the three grids keep the same column widths
''' (the budget grid has «Clsf» first for a leaf too, every stretching column is authored at 150), so their
''' columns line up; the strip of the scroll bar is kept free on all three while one of the two scrolls.
''' </summary>
Public Class ClasificatiiBugetView
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
    Private Const ColAngajament As String = "angajament"
    Private Const ColIndicator As String = "indicator"
    Private Const ColRezervari As String = "rezervari"
    Private Const ColReceptii As String = "receptii"
    Private Const ColPlati As String = "plati"
    ' Slice 0126-01: the data rows each grid holds with no vertical bar; the spare row at the end comes on top of them.
    Private Const BudgetRowsFit As Integer = 3
    Private Const CorrectionsRowsFit As Integer = 6
    ' The Tag of the spare row at the end of the budget and the corrections grid (a real row has Tag Nothing).
    Private Shared ReadOnly SpareTag As New Object()
    Private Shared ReadOnly QuarterColumns As String() = {ColTrim1, ColTrim2, ColTrim3, ColTrim4}
    Private Shared ReadOnly DateFormats As String() = {
        "d.M.yyyy", "dd.MM.yyyy", "d/M/yyyy", "dd/MM/yyyy", "d-M-yyyy", "yyyy-MM-dd", "ddMMyyyy"}

    Private _api As INomenclatoareApi
    Private _gate As ReauthGate
    Private _an As Integer
    Private _session As SessionContext
    ' The source the tree on screen is narrowed to (the title bar's, slice AV01-12); Nothing = every source («Toate»).
    Private _shownSs As String
    Private ReadOnly _deletedIds As New List(Of Integer)()
    Private ReadOnly _deletedBudgetIds As New List(Of Integer)()
    Private _catalog As ClasificatiiCatalog
    Private _current As Clasificatie
    Private _currentNode As AdvancedTreeControl.TreeItem
    ' Slice 0107: the leaf of each classification in the tree now shown (the check window sends the operator to one).
    Private ReadOnly _leafNodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
    ' Slice 0105: the classifications with movement in the year (some quarter of a budget version or of a
    ' correction is not zero); the tree shows only them until «Arata toate clasificatiile» is ticked.
    Private _activeIds As New HashSet(Of Integer)()
    Private _suppressToggle As Boolean
    Private _dirty As Boolean
    Private _busy As Boolean
    ' Slice 0105: a node above the leaves shows the read-only summary (Clsf column, last budget, corrections
    ' total); the designer's «+» icons and captions are kept here to put back for a leaf.
    Private _childNote As String = String.Empty
    Private _summaryMode As Boolean
    Private _budgetCaption As String
    Private _correctionsCaption As String
    ' Slice 0126-01: the figure shown at the right of each tree row = the last budget + all the corrections of the classification
    ' (summed under a node); the rows keep their caption without the figure so it can be redone after a save.
    Private ReadOnly _figures As New Dictionary(Of Integer, Decimal)()
    Private ReadOnly _figureNodes As New List(Of FigureNode)()
    ' Slice 0126-01: only the newest answer of the «Angajamente» grid is shown (the operator can click through the tree faster than the server answers).
    Private _angajamenteAsk As Integer

    ''' <summary>A tree row whose caption ends with a figure: the row, its caption before the figure, and the classifications the figure adds up.</summary>
    Private NotInheritable Class FigureNode
        Public ReadOnly Node As AdvancedTreeControl.TreeItem
        Public ReadOnly Prefix As String
        Public ReadOnly Ids As List(Of Integer)

        Public Sub New(k_node As AdvancedTreeControl.TreeItem, k_prefix As String, k_ids As List(Of Integer))
            Node = k_node
            Prefix = k_prefix
            Ids = k_ids
        End Sub
    End Class

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <summary>
    ''' Gives the view its API, working year and the session, and loads the tree. The window calls it once, when it opens. The tree follows the
    ''' source of the title bar (slice AV01-12): one source, or every source on «Toate».
    ''' </summary>
    Public Sub Bind(k_api As INomenclatoareApi, k_gate As ReauthGate, k_an As Integer, k_session As SessionContext)
        Try
            ArgumentNullException.ThrowIfNull(k_api)
            ArgumentNullException.ThrowIfNull(k_gate)
            ArgumentNullException.ThrowIfNull(k_session)
            _api = k_api
            _gate = k_gate
            _an = k_an
            _session = k_session
            AddHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
            AddHandler Disposed, AddressOf ViewDisposed
            _budgetCaption = gridBuget.FooterCaption
            _correctionsCaption = gridRectificari.FooterCaption
            ' Slice 0104: «Trimite in Access» exists only for a unit that has the Access application
            ' (Setari.Access = 1) AND only where the Access component is installed.
            btnTrimiteAccess.Visible = AccessFeature.Enabled
            ApplyGridHeights()
            ShowNoSelection()
            LoadTree(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Bind", ex)
            Throw
        End Try
    End Sub

    Private Sub ViewDisposed(sender As Object, e As EventArgs)
        Try
            If _session IsNot Nothing Then RemoveHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.ViewDisposed", ex)
        End Try
    End Sub

    ' The source of the title bar, or Nothing on «Toate» / with none: then no classification is left out.
    Private Function CurrentSs() As String
        If _session Is Nothing OrElse _session.ToateSursele OrElse String.IsNullOrWhiteSpace(_session.SectorSursa) Then Return Nothing
        Return _session.SectorSursa
    End Function

    ' UI boundary: another source in the title bar = another list. What is typed is settled first (Yes / No / Cancel); Cancel keeps the screen as
    ' it is. A change of the year alone does not touch the tree (the window keeps the year it opened with).
    Private Async Sub SessionPeriodChanged(sender As Object, e As PeriodChangedEventArgs)
        Try
            If IsDisposed OrElse _catalog Is Nothing Then Return
            Dim k_ss As String = CurrentSs()
            If String.Equals(k_ss, _shownSs, StringComparison.OrdinalIgnoreCase) Then Return
            If Not Await CommitAsync().ConfigureAwait(True) Then
                SetStatus("Modificările nesalvate au rămas: lista arătată este cea a sursei dinainte.")
                Return
            End If
            If IsDisposed Then Return
            Dim k_keep As Integer? = If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf)
            _shownSs = CurrentSs()
            BuildTree(_catalog, k_keep)
            If _current Is Nothing OrElse Not _leafNodes.ContainsKey(_current.IdClsf) Then ShowNoSelection()
            SetStatus(CountText(_catalog))
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.SessionPeriodChanged", ex)
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
            _shownSs = CurrentSs()
            _activeIds = New HashSet(Of Integer)(summary.Where(Function(r) r.Active).Select(Function(r) r.IdClsf))
            _figures.Clear()
            For Each k_s As BudgetSummaryRow In summary
                _figures(k_s.IdClsf) = FigureOf(k_s)
            Next
            BuildTree(catalog, selectIdClsf)
            SetStatus(CountText(catalog))
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiBugetView.LoadTree", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.LoadTree", ex)
            If Not IsDisposed Then SetStatus("Clasificațiile nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    Private Sub BuildTree(catalog As ClasificatiiCatalog, selectIdClsf As Integer?)
        tree.Clear()
        _leafNodes.Clear()
        _figureNodes.Clear()
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
            Dim k_rootPrefix As String = $"{capitol} ({ss})~~~{Escape(JoinNames(chapterName, ssName))}"
            Dim k_rootIds As List(Of Integer) = chapter.Select(Function(c) c.IdClsf).ToList()
            Dim root As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"C|{capitol}|{ss}",
                k_rootPrefix & FigureSuffix(k_rootIds),
                pLeftIconClosed:=folder, pLeftIconOpen:=folder, pExpanded:=True)
            _figureNodes.Add(New FigureNode(root, k_rootPrefix, k_rootIds))
            root.Bold = True

            For Each sub1 In chapter.GroupBy(Function(c) c.Subcapitol).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                Dim subName As String = NameOr(catalog.SubcapitolNames, Left2(capitol) & sub1.Key.Replace(".", String.Empty))
                Dim k_subPrefix As String = $"{sub1.Key}~~~{Escape(subName)}"
                Dim k_subIds As List(Of Integer) = sub1.Select(Function(c) c.IdClsf).ToList()
                Dim subNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                    $"S|{capitol}|{ss}|{sub1.Key}", k_subPrefix & FigureSuffix(k_subIds), root,
                    pLeftIconClosed:=folder, pLeftIconOpen:=folder)
                _figureNodes.Add(New FigureNode(subNode, k_subPrefix, k_subIds))

                ' Title level (the first two digits of the articol, «10» for «10.01») between the sub-chapter and the article.
                For Each k_title In sub1.GroupBy(Function(c) Left2(c.Articol)).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                    Dim k_titlePrefix As String = $"{k_title.Key}~~~{Escape(NameOr(catalog.TitluNames, k_title.Key))}"
                    Dim k_titleIds As List(Of Integer) = k_title.Select(Function(c) c.IdClsf).ToList()
                    Dim k_titleNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                        $"T|{capitol}|{ss}|{sub1.Key}|{k_title.Key}",
                        k_titlePrefix & FigureSuffix(k_titleIds), subNode,
                        pLeftIconClosed:=folder, pLeftIconOpen:=folder)
                    _figureNodes.Add(New FigureNode(k_titleNode, k_titlePrefix, k_titleIds))

                    For Each art In k_title.GroupBy(Function(c) c.Articol).OrderBy(Function(gp) gp.Key, StringComparer.Ordinal)
                        Dim k_artPrefix As String = $"{art.Key}~~~{Escape(NameOr(catalog.ArticolNames, art.Key))}"
                        Dim k_artIds As List(Of Integer) = art.Select(Function(c) c.IdClsf).ToList()
                        Dim artNode As AdvancedTreeControl.TreeItem = tree.AddItem(
                            $"A|{capitol}|{ss}|{sub1.Key}|{art.Key}",
                            k_artPrefix & FigureSuffix(k_artIds), k_titleNode,
                            pLeftIconClosed:=folder, pLeftIconOpen:=folder)
                        _figureNodes.Add(New FigureNode(artNode, k_artPrefix, k_artIds))

                        For Each c As Clasificatie In art.OrderBy(Function(x) x.Alineat, StringComparer.Ordinal)
                            Dim k_leafPrefix As String = $"{c.Articol}.{c.Alineat}~~~{Escape(c.Denumire)}"
                            Dim k_leafIds As New List(Of Integer) From {c.IdClsf}
                            Dim leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                                $"L|{c.IdClsf}", k_leafPrefix & FigureSuffix(k_leafIds), artNode)
                            _figureNodes.Add(New FigureNode(leaf, k_leafPrefix, k_leafIds))
                            leaf.Tag = c
                            _leafNodes(c.IdClsf) = leaf
                            leaf.Tooltip = c.Clsf
                            If selectIdClsf.HasValue AndAlso c.IdClsf = selectIdClsf.Value Then toSelect = leaf
                        Next
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

    ' The classifications of the source the tree is narrowed to (every one on «Toate»).
    Private Function ScopeItems(catalog As ClasificatiiCatalog) As List(Of Clasificatie)
        If _shownSs Is Nothing Then Return catalog.Items.ToList()
        Return catalog.Items.Where(Function(c) String.Equals(c.Ss, _shownSs, StringComparison.OrdinalIgnoreCase)).ToList()
    End Function

    ' The classifications the tree shows: those of the source, all of them, or only those with movement in the year.
    Private Function VisibleItems(catalog As ClasificatiiCatalog) As List(Of Clasificatie)
        Dim inScope As List(Of Clasificatie) = ScopeItems(catalog)
        ' Slice 0107: «Arata DOAR clasificatiile folosite in FOREXE» wins over the movement filter.
        If chkForexe.Checked Then Return inScope.Where(Function(c) c.InForexe).ToList()
        If chkToate.Checked Then Return inScope
        Return inScope.Where(Function(c) _activeIds.Contains(c.IdClsf)).ToList()
    End Function

    Private Function CountText(catalog As ClasificatiiCatalog) As String
        Dim shown As Integer = VisibleItems(catalog).Count
        Dim total As Integer = ScopeItems(catalog).Count
        If _shownSs IsNot Nothing AndAlso total = 0 Then Return $"Sursa {_shownSs} nu are clasificații de cheltuieli."
        Dim ofSource As String = If(_shownSs Is Nothing, String.Empty, $" ({_shownSs})")
        If chkForexe.Checked Then Return $"{shown} din {total} clasificații{ofSource}, cele folosite în FOREXE."
        Return If(chkToate.Checked, $"{shown} clasificații{ofSource}.",
                  $"{shown} din {total} clasificații{ofSource}, cele cu mișcare în {_an}.")
    End Function

    ' UI boundary: the tree is rebuilt from the catalog already read; the open budget stays on screen.
    Private Sub ChkToate_CheckedChanged(sender As Object, e As EventArgs) Handles chkToate.CheckedChanged, chkForexe.CheckedChanged
        Try
            If _suppressToggle OrElse _catalog Is Nothing OrElse _busy Then Return
            BuildTree(_catalog, If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf))
            SetStatus(CountText(_catalog))
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.ChkToate_CheckedChanged", ex)
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

    ' ── The figure at the right of each tree row (slice 0126-01) ────────────────

    ' The last budget + all the corrections of the year, with no quarter cut-off (the same figure the «Buget + rectificări» row shows).
    Private Shared Function FigureOf(k_summary As BudgetSummaryRow) As Decimal
        Return If(k_summary.LastBudget Is Nothing, 0D, k_summary.LastBudget.Amounts.Total) +
               If(k_summary.CorrectionsTotal Is Nothing, 0D, k_summary.CorrectionsTotal.Total)
    End Function

    Private Shared Function FigureOf(k_data As BugetClasificatie) As Decimal
        Dim k_last As BudgetVersion = k_data.Budgets.
            Where(Function(b) b.StartDate.HasValue).
            OrderByDescending(Function(b) b.StartDate.Value).
            FirstOrDefault()
        Return If(k_last Is Nothing, 0D, k_last.Amounts.Total) + k_data.Corrections.Sum(Function(r) r.Amounts.Total)
    End Function

    ' « ~~~ figure »: the tree keeps a THIRD «~~~» part at the right edge, right-aligned. Standard format, no decimals.
    Private Function FigureSuffix(k_ids As List(Of Integer)) As String
        Dim k_sum As Decimal = 0D
        For Each k_id As Integer In k_ids
            Dim k_value As Decimal
            If _figures.TryGetValue(k_id, k_value) Then k_sum += k_value
        Next
        Return "~~~" & k_sum.ToString("N0", CultureInfo.CurrentCulture)
    End Function

    ' After a save the figures of the saved classification and of every row above it are redone, the rows staying where they are.
    Private Sub RefreshFigures()
        For Each k_n As FigureNode In _figureNodes
            k_n.Node.Caption = k_n.Prefix & FigureSuffix(k_n.Ids)
        Next
        tree.Invalidate()
    End Sub

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing OrElse pNode Is _currentNode Then Return
            Dim previous As AdvancedTreeControl.TreeItem = _currentNode
            ChangeSelection(pNode, previous)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Tree_NodeMouseUp", ex)
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
            GlobalErrorLog.Write("ClasificatiiBugetView.ChangeSelection", ex)
        End Try
    End Sub

    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        Try
            If _busy OrElse _api Is Nothing Then Return
            ' The classifications go to the source of the title bar (Nothing on «Toate»: the dialog lets the operator pick).
            Using f As New ClasificatiiAddForm(_api, _gate, _an, CurrentSs())
                If f.ShowDialog(Me) <> DialogResult.OK Then Return
                If f.Result IsNot Nothing AndAlso f.Result.Inserted > 0 Then
                    ' New classifications have no movement yet: show them, or the operator would not find them.
                    _suppressToggle = True
                    chkToate.Checked = True
                    chkForexe.Checked = False
                    _suppressToggle = False
                    ' The whole catalog is read again from the server, so the tree, the counts and the movement list all include the new rows.
                    LoadTree(If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf))
                End If
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Tree_FooterRightIconClicked", ex)
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
            ShowAngajamente(New List(Of Integer) From {c.IdClsf}, $"clasificația {c.Clsf}")
            _childNote = ChildNoteFor(c.Ss)
            SetStatus(String.Empty)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiBugetView.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.LoadBudgetAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus("Bugetul nu a putut fi încărcat. Detalii în jurnalul de erori.")
            End If
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The classifications under a chapter («C|capitol|ss»), sub-chapter («S|…|sub»), title («T|…|titlu») or article («A|…|art»)
    ' node, in tree order; Nothing for any other node.
    Private Function LeavesUnder(node As AdvancedTreeControl.TreeItem) As List(Of Clasificatie)
        If _catalog Is Nothing OrElse String.IsNullOrEmpty(node.Key) Then Return Nothing
        Dim parts As String() = node.Key.Split("|"c)
        Dim level As String = parts(0)
        Dim minParts As Integer = If(level = "C", 3, If(level = "S", 4, If(level = "T" OrElse level = "A", 5, Integer.MaxValue)))
        If parts.Length <> minParts Then Return Nothing
        ' «T|cap|ss|sub|titlu» holds the articles of that title; «A|cap|ss|sub|articol» one article.
        Dim k_isTitle As Boolean = level = "T"
        Return VisibleItems(_catalog).
            Where(Function(x) x.Capitol = parts(1) AndAlso x.Ss = parts(2) AndAlso
                              (parts.Length < 4 OrElse x.Subcapitol = parts(3)) AndAlso
                              (parts.Length < 5 OrElse If(k_isTitle, Left2(x.Articol) = parts(4), x.Articol = parts(4)))).
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
            Dim k_parts As String() = node.Key.Split("|"c)   ' «C|capitol|ss», «S|capitol|ss|sub», «A|capitol|ss|sub|art»
            ShowAngajamente(leaves.Select(Function(k_l) k_l.IdClsf).ToList(),
                            $"clasificațiile de sub {String.Join(".", k_parts.Skip(1).Where(Function(k_p, k_i) k_i <> 1))} ({k_parts(2)})")
            _childNote = If(k_parts.Length > 2, ChildNoteFor(k_parts(2)), String.Empty)
            SetStatus(String.Empty)
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiBugetView.LoadSummaryAsync", ex)
            If Not IsDisposed Then
                ShowNoSelection()
                SetStatus(ex.Message)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.LoadSummaryAsync", ex)
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
        SetCorrectionsTotal(False)
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
                    row(ColTotal) = s.LastBudget.Amounts.Total
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
        gridTotal.Enabled = True
        UpdateTotalRow()
        Dim parts As String() = node.Key.Split("|"c)
        Dim code As String = String.Join(".", parts.Skip(1).Where(Function(p, i) i <> 1))   ' capitol[.sub[.articol]], without the source
        lblBuget.Text = $"Buget {_an} — {code} ({parts(2)}): {leaves.Count} clasificații"
        lblRectificari.Text = $"Rectificări bugetare {_an} — total pe clasificație"
        Dim split As Integer = If(node.Caption, String.Empty).IndexOf("~~~", StringComparison.Ordinal)
        tips.SetToolTipText(lblBuget, If(split >= 0, node.Caption.Substring(split + 3), code))
        SetDirty(False)
    End Sub

    ' The summary is read-only: it has the «Clsf» column (the one that stretches) and no «Inceput»,
    ' «Nr. doc.», «Data», «✕» or footer «+»; a leaf has the editable grids as the designer made them.
    Private Sub ApplyMode(summary As Boolean)
        If summary = _summaryMode Then Return
        _summaryMode = summary
        SetColumn(gridBuget, ColStart, Not summary)
        SetColumn(gridBuget, ColDelete, Not summary)
        SetColumn(gridRectificari, ColClsf, summary)
        SetColumn(gridRectificari, ColDocument, Not summary)
        SetColumn(gridRectificari, ColData, Not summary)
        SetColumn(gridRectificari, ColDelete, Not summary)
        SetColumn(gridTotal, ColStart, Not summary)
        SetColumn(gridTotal, ColDelete, Not summary)
        For Each grid As KBotDataView In {gridBuget, gridRectificari}
            grid.ReadOnlyGrid = summary
        Next
        gridRectificari.FillColumnKey = If(summary, ColClsf, ColDocument)
        gridBuget.FooterCaption = If(summary, "Ultimul buget al fiecărei clasificații", _budgetCaption)
        gridRectificari.FooterCaption = If(summary, "Rectificările anului, pe clasificație", _correctionsCaption)
    End Sub

    ' Slice 0107: the footer sums belong to ONE classification's corrections; with a node (or nothing) chosen
    ' there is no leaf, and summing the corrections of every classification means nothing.
    Private Sub SetCorrectionsTotal(shown As Boolean)
        For Each k_key As String In {ColTrim1, ColTrim2, ColTrim3, ColTrim4, ColTotal}
            gridRectificari.Columns.First(Function(k_col) String.Equals(k_col.Key, k_key, StringComparison.Ordinal)).Aggregate =
                If(shown, KBotAggregate.Sum, KBotAggregate.None)
        Next
        If Not shown AndAlso Not _summaryMode Then gridRectificari.FooterCaption = String.Empty
    End Sub

    Private Shared Sub SetColumn(grid As KBotDataView, key As String, shown As Boolean)
        Dim column As KBotDataColumn = grid.Columns.First(Function(c) String.Equals(c.Key, key, StringComparison.Ordinal))
        column.Visible = If(shown, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
    End Sub

    ' Slice 0107-03: the row under both grids = the LAST budget (newest «Inceput») + ALL the corrections, quarter
    ' by quarter, read from what the two grids show now, so it follows every edit. A node adds up every
    ' classification under it (the budget grid holds the last budget of each).
    Private Sub UpdateTotalRow()
        gridTotal.ClearRows()
        If _current Is Nothing AndAlso Not _summaryMode Then Return
        Dim sums(QuarterColumns.Length - 1) As Decimal
        Dim lastStart As Date? = Nothing
        Dim lastRow As KBotDataRow = Nothing
        For i As Integer = 0 To gridBuget.RowCount - 1
            Dim row As KBotDataRow = gridBuget.Rows(i)
            If _summaryMode Then
                AddQuarters(sums, row)
                Continue For
            End If
            Dim startValue As Object = row(ColStart)
            If TypeOf startValue Is Date AndAlso (Not lastStart.HasValue OrElse DirectCast(startValue, Date) > lastStart.Value) Then
                lastStart = DirectCast(startValue, Date)
                lastRow = row
            End If
        Next
        If lastRow IsNot Nothing Then AddQuarters(sums, lastRow)
        For i As Integer = 0 To gridRectificari.RowCount - 1
            AddQuarters(sums, gridRectificari.Rows(i))
        Next

        Dim total As KBotDataRow = gridTotal.AddRow()
        total(ColClsf) = "Buget + rectificări"
        Dim sum As Decimal = 0D
        For q As Integer = 0 To sums.Length - 1
            total(QuarterColumns(q)) = sums(q)
            sum += sums(q)
        Next
        total(ColTotal) = sum
    End Sub

    ' Slice 0107: the three grids must keep their columns on the same vertical lines. A grid that has more rows than
    ' fit shows its scroll bar and loses that strip on the right; the others keep the strip free only then, so there
    ' is no empty gutter when nobody scrolls.
    Private Sub Grid_VScrollShownChanged(sender As Object, e As EventArgs) _
        Handles gridBuget.VScrollShownChanged, gridRectificari.VScrollShownChanged
        Try
            Dim k_reserve As Boolean = gridBuget.VScrollShown OrElse gridRectificari.VScrollShown
            gridBuget.ReserveVScrollSpace = k_reserve
            gridRectificari.ReserveVScrollSpace = k_reserve
            gridTotal.ReserveVScrollSpace = k_reserve
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Grid_VScrollShownChanged", ex)
        End Try
    End Sub

    Private Shared Sub AddQuarters(k_sums As Decimal(), k_row As KBotDataRow)
        For q As Integer = 0 To k_sums.Length - 1
            k_sums(q) += ReadAmount(k_row, QuarterColumns(q)).GetValueOrDefault()
        Next
    End Sub

    Private Sub FillGrids(data As BugetClasificatie)
        ApplyMode(False)
        SetCorrectionsTotal(True)
        gridRectificari.FooterCaption = _correctionsCaption
        gridBuget.BeginUpdate()
        Try
            gridBuget.ClearRows()
            For Each v As BudgetVersion In data.Budgets
                Dim row As KBotDataRow = gridBuget.AddRow()
                row(ColClsf) = _current.Clsf
                row(ColId) = If(v.Id.HasValue, CObj(v.Id.Value), Nothing)
                row(ColStart) = If(v.StartDate.HasValue, CObj(v.StartDate.Value), Nothing)
                row(ColTrim1) = Box(v.Amounts.Trim1)
                row(ColTrim2) = Box(v.Amounts.Trim2)
                row(ColTrim3) = Box(v.Amounts.Trim3)
                row(ColTrim4) = Box(v.Amounts.Trim4)
                row(ColTotal) = v.Amounts.Total
                row(ColDelete) = DeleteCaption
            Next
            AddSpareRow(gridBuget)
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
            AddSpareRow(gridRectificari)
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
        lblBuget.Text = $"Buget {_an} — {_current.Clsf}"
        lblRectificari.Text = $"Rectificări bugetare {_an}"
        tips.SetToolTipText(lblBuget, _current.Denumire)
        SetDirty(False)
    End Sub

    Private Sub ShowNoSelection()
        ApplyMode(False)
        SetCorrectionsTotal(False)
        _current = Nothing
        _childNote = String.Empty
        gridBuget.ClearRows()
        gridRectificari.ClearRows()
        gridTotal.ClearRows()
        _angajamenteAsk += 1
        gridAngajamente.ClearRows()
        lblAngajamente.Text = "Angajamente"
        gridBuget.Enabled = False
        gridRectificari.Enabled = False
        gridTotal.Enabled = False
        _deletedIds.Clear()
        _deletedBudgetIds.Clear()
        lblBuget.Text = "Buget — alegeți un alineat din arbore"
        lblRectificari.Text = "Rectificări bugetare"
        SetDirty(False)
    End Sub

    ' ── The angajamente that lead to the chosen classification(s) (slice 0126-01) ──

    ' UI boundary: started without await; the budget never waits for it. Only the newest answer is shown.
    Private Async Sub ShowAngajamente(k_ids As List(Of Integer), k_what As String)
        Try
            _angajamenteAsk += 1
            Dim k_ask As Integer = _angajamenteAsk
            gridAngajamente.ClearRows()
            lblAngajamente.Text = $"Angajamente — {k_what}"
            Dim k_rows As IReadOnlyList(Of ClasificatieAngajament) = Await _gate.RunAsync(
                Function() _api.GetClasificatiiAngajamenteAsync(k_ids, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed OrElse k_ask <> _angajamenteAsk Then Return
            gridAngajamente.BeginUpdate()
            Try
                gridAngajamente.ClearRows()
                For Each k_a As ClasificatieAngajament In k_rows
                    Dim k_row As KBotDataRow = gridAngajamente.AddRow()
                    k_row(ColAngajament) = If(String.IsNullOrWhiteSpace(k_a.Descriere), k_a.CodAngajament, k_a.CodAngajament & " — " & k_a.Descriere)
                    k_row(ColIndicator) = k_a.CodIndicator
                    k_row(ColRezervari) = k_a.TotalRezervari
                    k_row(ColReceptii) = k_a.TotalReceptii
                    k_row(ColPlati) = k_a.TotalPlati
                Next
            Finally
                gridAngajamente.EndUpdate()
            End Try
            gridAngajamente.ClearDirty()
            lblAngajamente.Text = $"Angajamente — {k_what}: {k_rows.Count}"
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiBugetView.ShowAngajamente", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.ShowAngajamente", ex)
            If Not IsDisposed Then SetStatus("Angajamentele clasificației nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Sizes and the spare row (slice 0126-01) ─────────────────────────────────

    ' The budget grid holds BudgetRowsFit rows and the corrections grid CorrectionsRowsFit, each plus the spare row, with no vertical bar;
    ' the «Angajamente» grid takes what is left. The table wants LOGICAL heights, the grid knows device pixels.
    Private Sub ApplyGridHeights()
        Try
            Dim k_scale As Single = Math.Max(0.5F, AppScaling.FactorFor(tlyRight))
            tlyRight.SetRowHeight(1, gridBuget.HeightForRows(BudgetRowsFit + 1) / k_scale)
            tlyRight.SetRowHeight(3, gridRectificari.HeightForRows(CorrectionsRowsFit + 1) / k_scale)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.ApplyGridHeights", ex)
        End Try
    End Sub

    ' UI boundary (handler): another DPI or zoom = other pixel heights for the same number of rows.
    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        Try
            MyBase.OnDpiChangedAfterParent(e)
            If _api IsNot Nothing AndAlso IsHandleCreated Then BeginInvoke(New Action(AddressOf ApplyGridHeights))
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.OnDpiChangedAfterParent", ex)
        End Try
    End Sub

    Private Shared Function IsSpare(k_row As KBotDataRow) As Boolean
        Return k_row IsNot Nothing AndAlso ReferenceEquals(k_row.Tag, SpareTag)
    End Function

    Private Shared Sub AddSpareRow(k_grid As KBotDataView)
        Dim k_row As KBotDataRow = k_grid.AddRow()
        k_row.Tag = SpareTag
    End Sub

    ''' <summary>
    ''' The spare row becomes a real one: the cells not typed get what the «+» of the footer always gave (the version starts 01.01 or today;
    ''' the correction has today's date), and a new spare row follows it.
    ''' </summary>
    Private Sub PromoteSpareRow(k_grid As KBotDataView, k_row As KBotDataRow)
        Dim k_index As Integer = k_grid.RowCount - 1
        If k_grid Is gridBuget Then
            k_row(ColClsf) = _current.Clsf
            If k_row(ColStart) Is Nothing Then
                k_row(ColStart) = If(k_index > 0 AndAlso Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
            End If
        Else
            If k_row(ColDocument) Is Nothing Then k_row(ColDocument) = String.Empty
            If k_row(ColData) Is Nothing Then k_row(ColData) = If(Date.Today.Year = _an, Date.Today, New Date(_an, 1, 1))
        End If
        k_row(ColTotal) = RowTotal(k_row)
        k_row(ColDelete) = DeleteCaption
        k_row.Tag = Nothing
        k_row.IsDirty = True
        AddSpareRow(k_grid)
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
            GlobalErrorLog.Write("ClasificatiiBugetView.Grid_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) _
        Handles gridBuget.CellValueChanged, gridRectificari.CellValueChanged
        Try
            Dim grid As KBotDataView = DirectCast(sender, KBotDataView)
            ' Slice 0126-01: typing into the spare row makes it a real row (and a new spare row follows).
            If _current IsNot Nothing AndAlso e.RowIndex >= 0 AndAlso e.RowIndex < grid.RowCount AndAlso IsSpare(grid.Rows(e.RowIndex)) Then
                If String.IsNullOrWhiteSpace(Convert.ToString(grid(e.ColumnKey, e.RowIndex), CultureInfo.CurrentCulture)) Then Return
                PromoteSpareRow(grid, grid.Rows(e.RowIndex))
            End If
            ' Both grids show the row «Total» (slice 0107); neither has a footer total for the budget.
            If QuarterColumns.Contains(e.ColumnKey) Then
                grid(ColTotal, e.RowIndex) = RowTotal(grid.Rows(e.RowIndex))
            End If
            If QuarterColumns.Contains(e.ColumnKey) OrElse e.ColumnKey = ColStart Then UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Grid_CellValueChanged", ex)
        End Try
    End Sub

    ' Esc twice in a row: a row without an id is one the operator just typed in the spare row and it goes away; a row from the server goes back as it was (the grid's default).
    Private Sub Grid_RowCancelRequested(sender As Object, e As KBotRowCancelEventArgs) Handles gridBuget.RowCancelRequested, gridRectificari.RowCancelRequested
        Try
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            If _busy OrElse e.RowIndex < 0 OrElse e.RowIndex >= k_grid.RowCount Then Return
            ' The spare row is not a row of the budget: there is nothing to take away.
            If IsSpare(k_grid.Rows(e.RowIndex)) Then
                e.Handled = True
                Return
            End If
            If k_grid(ColId, e.RowIndex) IsNot Nothing Then Return
            e.Handled = True
            k_grid.RemoveRowAt(e.RowIndex)
            UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Grid_RowCancelRequested", ex)
        End Try
    End Sub

    ' Slice 0126-01: the spare row shows no «✕»; a hint in its first text cell says what typing there does.
    Private Sub Grid_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) _
        Handles gridBuget.CellFormatting, gridRectificari.CellFormatting
        Try
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            If e.RowIndex < 0 OrElse e.RowIndex >= k_grid.RowCount OrElse Not IsSpare(k_grid.Rows(e.RowIndex)) Then Return
            If e.ColumnKey = ColDelete Then
                e.HideButton = True
            ElseIf String.IsNullOrEmpty(e.Text) AndAlso
                   ((k_grid Is gridBuget AndAlso e.ColumnKey = ColClsf) OrElse (k_grid Is gridRectificari AndAlso e.ColumnKey = ColDocument)) Then
                e.Text = If(k_grid Is gridBuget, "Versiune nouă…", "Rectificare nouă…")
                e.ForeColor = ThemeManager.Current.Palette.TextDimColor
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.Grid_CellFormatting", ex)
        End Try
    End Sub

    ' Slice 0126-01: the row «Buget + rectificări» looks like the footer of the grids above it.
    Private Sub GridTotal_RowFormatting(sender As Object, e As KBotRowFormattingEventArgs) Handles gridTotal.RowFormatting
        Try
            e.BackColor = gridBuget.FooterBackColorInUse
            e.ForeColor = gridBuget.FooterForeColorInUse
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.GridTotal_RowFormatting", ex)
        End Try
    End Sub

    Private Sub GridBuget_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridBuget.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridBuget.RowCount OrElse IsSpare(gridBuget.Rows(e.RowIndex)) Then Return
            Dim id As Object = gridBuget(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedBudgetIds.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridBuget.RemoveRowAt(e.RowIndex)
            UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.GridBuget_ButtonClick", ex)
        End Try
    End Sub

    Private Sub GridRectificari_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridRectificari.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridRectificari.RowCount OrElse IsSpare(gridRectificari.Rows(e.RowIndex)) Then Return
            Dim id As Object = gridRectificari(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedIds.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridRectificari.RemoveRowAt(e.RowIndex)
            UpdateTotalRow()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.GridRectificari_ButtonClick", ex)
        End Try
    End Sub

    ' ── Save / close ────────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.BtnSalveaza_Click", ex)
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
            _figures(idClsf) = FigureOf(saved)
            RefreshFigures()
            If HasMovement(saved) Then _activeIds.Add(idClsf) Else _activeIds.Remove(idClsf)
            SetStatus($"Salvat: {_current.Clsf}.")
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiBugetView.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "Salvare buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.SaveAsync", ex)
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
    ' selected. «Inceput» is required, inside the working year, and one per day.
    Private Function ReadBudgets(ByRef problem As String) As List(Of BudgetVersion)
        Dim list As New List(Of BudgetVersion)()
        Dim seen As New HashSet(Of Date)()
        For i As Integer = 0 To gridBuget.RowCount - 1
            Dim row As KBotDataRow = gridBuget.Rows(i)
            If IsSpare(row) Then Continue For
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
            If IsSpare(row) Then Continue For
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

    ''' <summary>True when a budget or a correction was typed and not saved yet.</summary>
    Public ReadOnly Property HasPending As Boolean Implements IClasificatiiPage.HasPending
        Get
            Return _dirty
        End Get
    End Property

    ''' <summary>
    ''' Settles the unsaved changes before the window closes or the other page takes over: asks (Yes saves, No drops them and
    ''' shows the saved budget again, Cancel stays). True = the caller may go on.
    ''' </summary>
    Public Async Function CommitAsync() As Task(Of Boolean) Implements IClasificatiiPage.CommitAsync
        Try
            If Not _dirty Then Return True
            Dim k_answer As DialogResult = AskToSave()
            If k_answer = DialogResult.Cancel Then Return False
            If k_answer = DialogResult.Yes Then Return Await SaveAsync().ConfigureAwait(True)
            Dim k_current As Clasificatie = _current
            SetDirty(False)
            If k_current IsNot Nothing Then Await LoadBudgetAsync(k_current).ConfigureAwait(True)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.CommitAsync", ex)
            Return False
        End Try
    End Function

    ' ── Budget check + «Trimite in Access» (slice 0103-04 / 0103-06) ─────────────────

    ' UI boundary: the check shows its own errors.
    Private Async Sub BtnVerifica_Click(sender As Object, e As EventArgs) Handles btnVerifica.Click
        Try
            If _busy OrElse _api Is Nothing Then Return
            SetBusy(True, "Se verifică bugetul față de FOREXE…")
            Dim k_chosen As Integer? = Nothing
            Try
                Await BudgetCheckForm.RunAsync(Me, _api, _gate, showWhenEqual:=True,
                                               onPick:=Sub(k_id) k_chosen = k_id).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, String.Empty)
            End Try
            ' Slice 0107: double click in the check window = show that classification here.
            If k_chosen.HasValue AndAlso Not IsDisposed Then GoToClassification(k_chosen.Value)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.BtnVerifica_Click", ex)
        End Try
    End Sub

    ' UI boundary (slice 0107): selects the leaf of a classification and loads its budget, through the same path
    ' as a click on it (unsaved changes are asked about). A leaf the filters hide is made visible first.
    Private Sub GoToClassification(k_idClsf As Integer)
        Try
            If _catalog Is Nothing OrElse _busy Then Return
            If Not _leafNodes.ContainsKey(k_idClsf) Then
                If Not _catalog.Items.Any(Function(k_x) k_x.IdClsf = k_idClsf) Then Return
                If Not ScopeItems(_catalog).Any(Function(k_x) k_x.IdClsf = k_idClsf) Then
                    SetStatus("Clasificația aparține altei surse decât cea din bara de titlu; alegeți sursa ei sau «Toate».")
                    Return
                End If
                _suppressToggle = True
                chkForexe.Checked = False
                chkToate.Checked = True
                _suppressToggle = False
                BuildTree(_catalog, If(_current Is Nothing, CType(Nothing, Integer?), _current.IdClsf))
                SetStatus(CountText(_catalog))
                If Not _leafNodes.ContainsKey(k_idClsf) Then Return
            End If
            Dim k_node As AdvancedTreeControl.TreeItem = _leafNodes(k_idClsf)
            If k_node Is _currentNode Then
                tree.SelectAndReveal(k_node)
                Return
            End If
            Dim k_previous As AdvancedTreeControl.TreeItem = _currentNode
            tree.SelectAndReveal(k_node)
            ChangeSelection(k_node, k_previous)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.GoToClassification", ex)
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
            GlobalErrorLog.Write("ClasificatiiBugetView.BtnTrimiteAccess_Click", ex)
            KBotMessage.Show(Me, ex.Message, "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As InvalidOperationException
            GlobalErrorLog.Write("ClasificatiiBugetView.BtnTrimiteAccess_Click", ex)
            KBotMessage.Show(Me, ex.Message, "Trimite în Access", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.BtnTrimiteAccess_Click", ex)
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

    ' An empty text shows the note about child subunits of the chosen source when there is one (slice AV01-11).
    Private Sub SetStatus(text As String)
        lblStare.Text = If(String.IsNullOrEmpty(text), _childNote, text)
    End Sub

    ' Slice AV01-11: a source that has child subunits (same source as their parent, so they work on the parent's data): nothing is blocked,
    ' the footer says that the work goes straight into the parent's data.
    Private Function ChildNoteFor(k_ss As String) As String
        Dim k_info As ChildSourceInfo = Nothing
        If _catalog IsNot Nothing AndAlso Not String.IsNullOrEmpty(k_ss) AndAlso _catalog.ChildSources.TryGetValue(k_ss, k_info) Then
            Return k_info.NoteFor(k_ss)
        End If
        Return String.Empty
    End Function

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyMain.BackColor = p.SurfaceAltColor
            pnlCard.BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyBife.BackColor = p.SurfaceAltColor
            tlyRight.BackColor = p.SurfaceAltColor
            tlySubsol.BackColor = p.SurfaceAltColor
            chkToate.BackColor = p.SurfaceAltColor
            chkToate.ForeColor = p.TextColor
            chkForexe.BackColor = p.SurfaceAltColor
            chkForexe.ForeColor = p.TextColor
            lblBuget.BackColor = p.SurfaceAltColor
            lblBuget.ForeColor = p.TextColor
            lblRectificari.BackColor = p.SurfaceAltColor
            lblRectificari.ForeColor = p.TextColor
            lblAngajamente.BackColor = p.SurfaceAltColor
            lblAngajamente.ForeColor = p.TextColor
            lblStare.ForeColor = p.TextDimColor
            ' Slice 0126-02: the «✕» buttons are flat (they show the row colour); only the hover takes a wash of the error colour.
            Dim k_hover As Color = Color.FromArgb(70, p.ErrorColor)
            gridBuget.Column(ColDelete).ButtonHoverBackColor = k_hover
            gridRectificari.Column(ColDelete).ButtonHoverBackColor = k_hover
            ' The footer colour of the grids is part of the theme: the heights and the «footer-like» row follow it.
            If IsHandleCreated AndAlso _api IsNot Nothing Then BeginInvoke(New Action(AddressOf ApplyGridHeights))
            gridTotal.Invalidate()
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            ButtonStyles.ApplySecondary(btnVerifica, scheme)
            ButtonStyles.ApplySecondary(btnTrimiteAccess, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiBugetView.ApplyTheme", ex)
        End Try
    End Sub

End Class
