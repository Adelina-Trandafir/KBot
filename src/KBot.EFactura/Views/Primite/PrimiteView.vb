Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-18 -- the received e-invoices as one control, used by the «E-Factura» view of the DDF (KBot.App) and by the window with
''' all the received invoices. On the left a TREE: a root «Toate facturile», under it the months, under each month the suppliers
''' (grouped by tax code), under each supplier the invoices (date, number, amount; the dot is neutral for now, its colour will carry a meaning to be decided).
''' The header of the tree has the search button and the filter-options button. On the right the chosen invoice, in VIEWS chosen from
''' a horizontal bar (the pattern of the DDF view): the first view (the supplier and the notes above, the lines below), «Factură PDF» (its own bar: the classic invoice K-BOT draws, the one ANAF
''' draws, and a popup with the attached files), «Atașamente» (the list, to save a file) and «Mesaje» (the last two only when the invoice has some). A right click on an invoice
''' offers «Salvează ca ZIP» and «Salvează ca XML» and, in the DDF view, the manual link.
'''
''' <para>Two modes. WITHOUT a DDF (<see cref="LoadAsync"/> with Nothing): every received invoice of the year. WITH a DDF: ONLY the invoices
''' of the DDF — the supplier is one of its partners (compared by tax code, «RO» and spaces ignored) or the operator linked it by hand
''' (slice 0114-17: the tick that showed all the invoices, with the unlinked ones in grey, is gone from the view of the angajament; an
''' invoice is linked to a DDF from the window with all the received invoices).</para>
'''
''' <para>The control holds no rules: what is linked to what, the amounts and the files come from the server (slice 00EF-17).
''' The embedded PDF viewer lives in KBot.App; the host passes a factory (as for the issued invoices).</para>
''' </summary>
Public Class PrimiteView
    Implements IThemedContainer, IPutAway

    ' The views of the horizontal bar (the keys of navDetaliu's items, written in the designer).
    Private Const ViewLinii As String = "linii"
    Private Const ViewPdf As String = "pdf"
    Private Const ViewMesaje As String = "mesaje"
    ' The view «XML Factura» (aligned right in the bar): the XML of the invoice, in the XmlViz control (slice 00EF-36).
    Private Const ViewXml As String = "xml"

    Private Const MenuZip As String = "zip"
    Private Const MenuXml As String = "xml"
    Private Const MenuLega As String = "lega"
    Private Const MenuScoate As String = "scoate"
    Private Const MenuLegaAlege As String = "legaalege"
    Private Const MenuScoatePrefix As String = "scoate:"

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")
    Private Shared ReadOnly _monthNames As String() = {"Ianuarie", "Februarie", "Martie", "Aprilie", "Mai", "Iunie", "Iulie",
                                                       "August", "Septembrie", "Octombrie", "Noiembrie", "Decembrie"}

    Private _api As IEFacturaApi
    Private _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _nodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

    Private _year As Integer
    Private _idDdf As Integer?
    Private _items As New List(Of EFacturaPrimita)()
    Private _linked As New Dictionary(Of Integer, String)()
    Private _detail As EFacturaPrimitaDetaliu
    Private _shownId As Integer
    Private _treeLoading As Boolean
    Private _seq As Integer
    Private _pdfSeq As Integer
    Private _syncing As Boolean
    Private _xmlSeq As Integer
    Private _xmlShownId As Integer

    ' Slice 00EF-35 -- the options of the header menu (the session's, not stored): what is shown, how the tree is grouped.
    Private _onlyWithoutOperations As Boolean
    Private _onlyNew As Boolean
    Private _groupBySupplier As Boolean
    Private _ascending As Boolean
    ' Slice 0114-22 -- the view of an angajament (see AngajamentView) and the filter icon the designer gave the header.
    Private _angajamentView As Boolean
    Private _filterIcon As Image
    ' The invoices this session brought from ANAF: only those are known to be new.
    Private ReadOnly _newIds As New HashSet(Of Integer)()

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <param name="k_api">The received-invoice routes; the shell passes its API client.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_viewerFactory">Makes the embedded PDF viewer (it lives in the shell's project); Nothing = the PDF view says there is none.</param>
    ''' <param name="k_year">Only the invoices dated in this year are listed when no DDF is given (0 = all years).</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_viewerFactory As Func(Of IFacturaPdfViewer), k_year As Integer)
        InitializeComponent()
        Initialize(k_api, k_gate, k_viewerFactory, k_year)
    End Sub

    ''' <summary>
    ''' Gives a control made by a designer (the host declares it in its own designer file) what it needs to work. Once, before the first
    ''' <see cref="LoadAsync"/>; parameters as in the constructor.
    ''' </summary>
    Public Sub Initialize(k_api As IEFacturaApi, k_gate As ReauthGate, k_viewerFactory As Func(Of IFacturaPdfViewer), k_year As Integer)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        If _api IsNot Nothing Then Throw New InvalidOperationException("PrimiteView is already initialised.")
        _api = k_api
        _gate = k_gate
        _year = k_year
        pgPdf.ViewerFactory = k_viewerFactory
        ' While a document is still opening in Adobe no other row may be clicked (the same gate the other trees use).
        AdobeOpenGate.LockWhileOpening(tree)
    End Sub

    ''' <summary>The invoices the list holds now (empty before the first successful read). A snapshot: a new read replaces it.</summary>
    Public ReadOnly Property Items As IReadOnlyList(Of EFacturaPrimita)
        Get
            Return _items
        End Get
    End Property

    ''' <summary>
    ''' Slice 0114-22: True when the control is the VIEW of an angajament (the shell's «E-Factura» view), False when it is the window with all
    ''' the received invoices. The view has no options button in the header of its tree and no supplier level: «Toate facturile» / month / invoice.
    ''' </summary>
    <System.ComponentModel.Category("K-BOT")>
    <System.ComponentModel.DefaultValue(False)>
    <System.ComponentModel.Description("Vederea unui angajament: fără butonul de opțiuni din antetul arborelui și fără nivelul Furnizor.")>
    Public Property AngajamentView As Boolean
        Get
            Return _angajamentView
        End Get
        Set(k_value As Boolean)
            If _angajamentView = k_value Then Return
            _angajamentView = k_value
            ' The designer sets the icon of the header before it sets this property: keep it, to give it back.
            If k_value AndAlso tree.HeaderRightIcon IsNot Nothing Then _filterIcon = tree.HeaderRightIcon
            tree.HeaderRightIcon = If(k_value, Nothing, _filterIcon)
        End Set
    End Property

    ''' <summary>True once the list was read successfully at least once (slice 00EF-32: «what is new» needs a list to compare with).</summary>
    Public ReadOnly Property ListLoaded As Boolean

    ''' <summary>The DDF the list is for, or Nothing for all the invoices of the year.</summary>
    Public ReadOnly Property IdDdf As Integer?
        Get
            Return _idDdf
        End Get
    End Property

    ''' <summary>Shows nothing: no list, no invoice (the host has no DDF to show invoices for).</summary>
    Public Sub ClearAll()
        _seq += 1
        _idDdf = Nothing
        _items = New List(Of EFacturaPrimita)()
        _linked = New Dictionary(Of Integer, String)()
        ntfMesaj.Visible = False
        tree.Clear()
        _nodes.Clear()
        ClearDetail()
        ShowDetailSurface()
        pgLista.SetItems(Nothing)
    End Sub

    ''' <summary>Lets go of the PDF on screen (the view is put away).</summary>
    Public Sub ReleaseDocument()
        pgPdf.ReleaseDocument()
    End Sub

    ''' <summary>
    ''' Slice 0113-01: the window is about to be put away (another page or a modal window takes the work area): the bar moves to the
    ''' first view (the lines, no PDF) and the PDF is let go, so Adobe is closed while nobody looks at it. The bar stays shown.
    ''' </summary>
    Public Sub PutAway() Implements IPutAway.PutAway
        Try
            If String.Equals(navDetaliu.SelectedKey, ViewPdf, StringComparison.Ordinal) Then navDetaliu.SelectedKey = ViewLinii
            pgPdf.ReleaseDocument()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PutAway", ex)
            Throw
        End Try
    End Sub

    Private Sub PrimiteView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            navDetaliu.SelectedKey = ViewLinii
            ShowPage(ViewLinii)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PrimiteView_Load", ex)
        End Try
    End Sub

    Private Sub PrimiteView_HandleDestroyed(sender As Object, e As EventArgs) Handles MyBase.HandleDestroyed
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PrimiteView_HandleDestroyed", ex)
        End Try
    End Sub

    ' ── Loading ─────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' (Re)reads the list: of the DDF <paramref name="k_idDdf"/>, or Nothing for every received invoice of the year. The shown invoice
    ''' stays shown when it is still in the list. A failure becomes a line in the notice; nothing is thrown.
    ''' </summary>
    Public Async Function LoadAsync(k_idDdf As Integer?) As Task
        Try
            If _api Is Nothing Then Return
            Dim k_ddfChanged As Boolean = Not Nullable.Equals(_idDdf, k_idDdf)
            _idDdf = k_idDdf
            If k_ddfChanged Then
                ClearDetail()
                ShowDetailSurface()
            End If
            ntfMesaj.Clear()
            ntfMesaj.Visible = False
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            ' With a DDF the server answers only the invoices linked to it (by the supplier or by hand); without one, those of the year.
            Dim k_list As List(Of EFacturaPrimita) = Await _gate.RunAsync(
                Function() _api.GetPrimiteAsync(If(_idDdf.HasValue, Nothing, YearFilter()), Nothing, Nothing, _idDdf, _cts.Token)).ConfigureAwait(True)
            Dim k_linkedIds As New Dictionary(Of Integer, String)()
            If _idDdf.HasValue Then
                For Each k_p As EFacturaPrimita In k_list
                    k_linkedIds(k_p.IdPrimita) = k_p.Legatura
                Next
            End If
            If IsDisposed OrElse k_mySeq <> _seq Then Return
            _items = k_list
            _ListLoaded = True
            _linked = k_linkedIds
            BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
            If _shownId > 0 AndAlso Not _nodes.ContainsKey(_shownId) Then ClearDetail()
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.LoadAsync", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            ' UI boundary (called from handlers): log and tell the operator.
            GlobalErrorLog.Write("PrimiteView.LoadAsync", ex)
            ShowProblem("Facturile primite nu au putut fi citite. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Function YearFilter() As Integer?
        Return If(_year > 0, CType(_year, Integer?), Nothing)
    End Function

    Private Sub ShowProblem(k_text As String)
        ntfMesaj.Show(k_text, NoticeKind.Error)
        ntfMesaj.Visible = True
    End Sub

    ' ── The tree ────────────────────────────────────────────────────────────────

    ' The invoices the tree shows: all, minus what the header menu filtered out.
    Private Function ShownItems() As List(Of EFacturaPrimita)
        Dim k_list As IEnumerable(Of EFacturaPrimita) = _items
        If _onlyWithoutOperations Then k_list = k_list.Where(Function(k_x) k_x.NrOperatii = 0)
        If _onlyNew Then k_list = k_list.Where(Function(k_x) _newIds.Contains(k_x.IdPrimita))
        Return k_list.ToList()
    End Function

    ''' <summary>
    ''' Rebuilds the tree from <c>_items</c> (less what the header menu filters out). Grouped by month (the default): the root «Toate
    ''' facturile», the months newest first, under a month the suppliers (by tax code, in name order), under a supplier its invoices;
    ''' in the view of an angajament there is no supplier level: the invoices are directly under their month (slice 0114-22).
    ''' Grouped by supplier: the suppliers are the roots, in name order, and their invoices are under them, ascending or descending by
    ''' date. The search of the tree's header narrows it; the tree is not rebuilt when an invoice is opened.
    ''' </summary>
    Private Sub BuildTree(k_selectId As Integer?)
        _treeLoading = True
        Try
            tree.Clear()
            _nodes.Clear()
            _rootNode = Nothing
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            Dim k_icon As Image = FacturaIcons.Client(k_palette, tree.LeftIconSize.Width)
            Dim k_shown As List(Of EFacturaPrimita) = ShownItems()
            If _groupBySupplier Then
                BuildSupplierTree(k_shown, k_selectId, k_icon, k_palette)
            Else
                BuildMonthTree(k_shown, k_selectId, k_icon, k_palette)
            End If
            Dim k_node As AdvancedTreeControl.TreeItem = Nothing
            If _listShown Then
                ' The list is on screen: it follows the new tree and the root stays the chosen row (no root = the list goes away).
                If _rootNode Is Nothing Then
                    ShowDetailSurface()
                Else
                    FillList()
                    tree.SelectAndReveal(_rootNode)
                End If
            ElseIf k_selectId.HasValue AndAlso _nodes.TryGetValue(k_selectId.Value, k_node) Then
                tree.SelectAndReveal(k_node)
            End If
            tree.Invalidate()
        Finally
            _treeLoading = False
        End Try
    End Sub

    Private Sub BuildMonthTree(k_shown As List(Of EFacturaPrimita), k_selectId As Integer?, k_icon As Image, k_palette As ThemePalette)
        Dim k_total As Decimal = k_shown.Sum(Function(k_x) k_x.Total)
        Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem(
            RootKey, $"Toate facturile~~~{k_total.ToString("N2", _ro)}",
            pLeftIconClosed:=k_icon, pLeftIconOpen:=k_icon, pExpanded:=True)
        k_root.Bold = True
        _rootNode = k_root
        ' The year is left out of a month's name (slice 00EF-40); it comes back only when the list holds more than one year, so two Septembers cannot be mistaken.
        Dim k_severalYears As Boolean = k_shown.Where(Function(k_x) k_x.DataFact.HasValue).Select(Function(k_x) k_x.DataFact.Value.Year).Distinct().Count() > 1
        Dim k_months As IEnumerable(Of IGrouping(Of Integer, EFacturaPrimita)) =
            k_shown.GroupBy(Function(k_x) MonthKey(k_x)).OrderByDescending(Function(k_g) k_g.Key)
        For Each k_group As IGrouping(Of Integer, EFacturaPrimita) In k_months
            Dim k_monthId As String = k_group.Key.ToString(CultureInfo.InvariantCulture)
            Dim k_hasSelected As Boolean = k_selectId.HasValue AndAlso k_group.Any(Function(k_x) k_x.IdPrimita = k_selectId.Value)
            Dim k_sum As Decimal = k_group.Sum(Function(k_x) k_x.Total)
            Dim k_month As AdvancedTreeControl.TreeItem = tree.AddItem(
                "M_" & k_monthId, $"{MonthLabel(k_group.Key, k_severalYears)}~~~{k_sum.ToString("N2", _ro)}", k_root,
                pLeftIconClosed:=k_icon, pLeftIconOpen:=k_icon, pExpanded:=k_hasSelected)
            k_month.Bold = True
            If _angajamentView Then
                AddInvoiceNodes(k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita), k_month, k_palette)
                Continue For
            End If
            Dim k_suppliers As IEnumerable(Of IGrouping(Of String, EFacturaPrimita)) =
                k_group.GroupBy(Function(k_x) SupplierKey(k_x)).
                        OrderBy(Function(k_g) SupplierName(k_g), StringComparer.CurrentCultureIgnoreCase)
            For Each k_supGroup As IGrouping(Of String, EFacturaPrimita) In k_suppliers
                Dim k_supHasSelected As Boolean = k_selectId.HasValue AndAlso k_supGroup.Any(Function(k_x) k_x.IdPrimita = k_selectId.Value)
                AddSupplierNode(k_supGroup, "S_" & k_monthId & "_" & k_supGroup.Key, k_month, k_supHasSelected, True, k_icon, k_palette)
            Next
        Next
    End Sub

    Private Sub BuildSupplierTree(k_shown As List(Of EFacturaPrimita), k_selectId As Integer?, k_icon As Image, k_palette As ThemePalette)
        Dim k_suppliers As IEnumerable(Of IGrouping(Of String, EFacturaPrimita)) =
            k_shown.GroupBy(Function(k_x) SupplierKey(k_x)).
                    OrderBy(Function(k_g) SupplierName(k_g), StringComparer.CurrentCultureIgnoreCase)
        For Each k_supGroup As IGrouping(Of String, EFacturaPrimita) In k_suppliers
            Dim k_supHasSelected As Boolean = k_selectId.HasValue AndAlso k_supGroup.Any(Function(k_x) k_x.IdPrimita = k_selectId.Value)
            AddSupplierNode(k_supGroup, "S_" & k_supGroup.Key, Nothing, k_supHasSelected, False, k_icon, k_palette)
        Next
    End Sub

    ' A supplier row and its invoices. Under a month the invoices are newest first; with the supplier as the root they follow the
    ' operator's choice (ascending / descending by date).
    Private Sub AddSupplierNode(k_supGroup As IGrouping(Of String, EFacturaPrimita), k_key As String,
                                k_parent As AdvancedTreeControl.TreeItem, k_expanded As Boolean, k_underMonth As Boolean,
                                k_icon As Image, k_palette As ThemePalette)
        Dim k_supSum As Decimal = k_supGroup.Sum(Function(k_x) k_x.Total)
        Dim k_supplier As AdvancedTreeControl.TreeItem = tree.AddItem(
            k_key, $"{SupplierName(k_supGroup)}~~~{k_supSum.ToString("N2", _ro)}", k_parent,
            pLeftIconClosed:=k_icon, pLeftIconOpen:=k_icon, pExpanded:=k_expanded)
        k_supplier.Tooltip = SupplierTooltip(k_supGroup)
        If Not k_underMonth Then k_supplier.Bold = True
        Dim k_invoices As IEnumerable(Of EFacturaPrimita)
        If Not k_underMonth AndAlso _ascending Then
            k_invoices = k_supGroup.OrderBy(Function(k_x) k_x.DataFact).ThenBy(Function(k_x) k_x.IdPrimita)
        Else
            k_invoices = k_supGroup.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita)
        End If
        AddInvoiceNodes(k_invoices, k_supplier, k_palette)
    End Sub

    ' The invoice rows (leaves) under a parent: a supplier, or in the view of an angajament a month.
    Private Sub AddInvoiceNodes(k_invoices As IEnumerable(Of EFacturaPrimita), k_parent As AdvancedTreeControl.TreeItem, k_palette As ThemePalette)
        For Each k_p As EFacturaPrimita In k_invoices
            Dim k_dot As Image = FacturaIcons.StateDot(DotOf(k_p), k_palette, tree.LeftIconSize.Width)
            Dim k_date As String = If(k_p.DataFact.HasValue, k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
            Dim k_caption As String = $"{k_date} · {If(k_p.Tip = "NC", "NC ", String.Empty)}{k_p.NrFact}~~~{k_p.Total.ToString("N2", _ro)}"
            Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                "P_" & k_p.IdPrimita.ToString(CultureInfo.InvariantCulture), k_caption, k_parent,
                pLeftIconClosed:=k_dot, pLeftIconOpen:=k_dot)
            k_leaf.Tag = k_p
            k_leaf.Tooltip = TooltipOf(k_p)
            _nodes(k_p.IdPrimita) = k_leaf
        Next
    End Sub

    ' The supplier of an invoice by its tax code (the key of the group); an invoice with no code falls back to the name written on it.
    Private Shared Function SupplierKey(k_p As EFacturaPrimita) As String
        If Not String.IsNullOrWhiteSpace(k_p.CuiNormalizat) Then Return k_p.CuiNormalizat.Trim()
        Return "N" & k_p.Furnizor.Trim().ToUpperInvariant()
    End Function

    ' The name shown for a supplier: the one on its newest invoice (the same code may be written with different names).
    Private Shared Function SupplierName(k_group As IEnumerable(Of EFacturaPrimita)) As String
        Dim k_newest As EFacturaPrimita = k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita).First()
        If Not String.IsNullOrWhiteSpace(k_newest.Furnizor) Then Return k_newest.Furnizor.Trim()
        If Not String.IsNullOrWhiteSpace(k_newest.Cui) Then Return k_newest.Cui.Trim()
        Return "Furnizor necunoscut"
    End Function

    Private Shared Function SupplierTooltip(k_group As IEnumerable(Of EFacturaPrimita)) As String
        Dim k_list As List(Of EFacturaPrimita) = k_group.ToList()
        Dim k_cui As String = k_list.Select(Function(k_x) k_x.Cui).FirstOrDefault(Function(k_c) Not String.IsNullOrWhiteSpace(k_c))
        Return $"{SupplierName(k_list)}" & If(String.IsNullOrWhiteSpace(k_cui), String.Empty, $" ({k_cui})") & vbLf &
               $"{k_list.Count} facturi · {k_list.Sum(Function(k_x) k_x.Total).ToString("N2", _ro)}"
    End Function

    ' yyyymm of the invoice date; 0 = no date.
    Private Shared Function MonthKey(k_p As EFacturaPrimita) As Integer
        Return If(k_p.DataFact.HasValue, k_p.DataFact.Value.Year * 100 + k_p.DataFact.Value.Month, 0)
    End Function

    Private Shared Function MonthLabel(k_key As Integer, k_withYear As Boolean) As String
        If k_key = 0 Then Return "Fără dată"
        Return If(k_withYear, $"{_monthNames(k_key Mod 100 - 1)} {k_key \ 100}", _monthNames(k_key Mod 100 - 1))
    End Function

    ' The dot: a grey ring = not linked to this DDF (cannot happen now that the list holds only the linked ones); otherwise a neutral dot. Whether a message was opened is NOT
    ' shown (operator, 15.10.2026): the colour of the dot will mean something else, to be decided.
    Private Function DotOf(k_p As EFacturaPrimita) As String
        If _idDdf.HasValue AndAlso Not _linked.ContainsKey(k_p.IdPrimita) Then Return EFacturaStare.Ciorna
        Return FacturaIcons.StareNeutra
    End Function

    Private Function TooltipOf(k_p As EFacturaPrimita) As String
        Dim k_lines As New List(Of String) From {
            $"{If(k_p.Tip = "NC", "Notă de credit", "Factura")} {k_p.NrFact}" & If(k_p.DataFact.HasValue, $" din {k_p.DataFact:dd.MM.yyyy}", String.Empty),
            $"Furnizor: {k_p.Furnizor} ({k_p.Cui})",
            $"Total: {k_p.Total.ToString("N2", _ro)} · TVA: {k_p.Tva.ToString("N2", _ro)}"}
        If k_p.DataScad.HasValue Then k_lines.Add($"Scadentă: {k_p.DataScad:dd.MM.yyyy}")
        If _idDdf.HasValue Then
            Dim k_how As String = Nothing
            If _linked.TryGetValue(k_p.IdPrimita, k_how) Then
                k_lines.Add(If(k_how = "manual", "Legată de acest DDF de către operator.", "Furnizorul este partener al acestui DDF."))
            Else
                k_lines.Add("Nelegată de acest DDF (clic dreapta pentru a o lega).")
            End If
        End If
        Return String.Join(vbLf, k_lines)
    End Function

    Private Sub ClearDetail()
        _shownId = 0
        _detail = Nothing
        gridLinii().ClearRows()
        pgLinii.ClearSupplier()
        gridMesaje().ClearRows()
        navDetaliu.SetItemVisible(ViewMesaje, False)
        navDetaliu.SetItemVisible(ViewXml, False)
        ClearKe()   ' slice KE03: the links with receptions
        _xmlShownId = 0
        pgXml.Clear()
        pgPdf.SetAttachments(Nothing)
        pgPdf.ReleaseDocument()
    End Sub

    Private Function gridLinii() As KBotDataView
        Return pgLinii.gridLinii
    End Function

    Private Function gridMesaje() As KBotDataView
        Return pgMesaje.gridMesaje
    End Function

    ' ── Choosing an invoice ─────────────────────────────────────────────────────

    Private Async Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If _treeLoading OrElse k_node Is Nothing Then Return
            ' The root «Toate facturile» shows the list of all the invoices (slice 00EF-41); a month has no page of its own.
            If String.Equals(k_node.Key, RootKey, StringComparison.Ordinal) Then
                If e Is Nothing OrElse e.Button = MouseButtons.Left Then Await ShowListSurfaceAsync().ConfigureAwait(True)
                Return
            End If
            Dim k_p As EFacturaPrimita = TryCast(k_node.Tag, EFacturaPrimita)
            If k_p Is Nothing Then Return
            If e IsNot Nothing AndAlso e.Button = MouseButtons.Right Then
                ShowMenu(k_p, tree.PointToScreen(New Point(e.X, e.Y)))
                Return
            End If
            If e IsNot Nothing AndAlso e.Button <> MouseButtons.Left Then Return
            ' Coming back from the list: the bar and the pages return, and the invoice that was shown stays shown.
            If _listShown Then ShowDetailSurface()
            If k_p.IdPrimita = _shownId Then Return
            Await ShowInvoiceAsync(k_p.IdPrimita).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ''' <summary>Reads the invoice and fills the views; a failure becomes a line in the notice. Never throws.</summary>
    Private Async Function ShowInvoiceAsync(k_id As Integer) As Task
        Try
            ntfMesaj.Visible = False
            If k_id <> _shownId Then _keCod = String.Empty   ' slice KE02: the angajament chosen last belongs to the previous invoice
            _shownId = k_id
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            Dim k_detail As EFacturaPrimitaDetaliu = Await _gate.RunAsync(
                Function() _api.GetPrimitaAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _seq OrElse k_id <> _shownId Then Return
            _detail = k_detail
            FillLines(k_detail)
            pgPdf.SetAttachments(k_detail.Atasamente)
            FillMessages(k_detail)
            ' «Mesaje» is always there: it is where a message to the supplier is written.
            navDetaliu.SetItemVisible(ViewMesaje, True)
            navDetaliu.SetItemVisible(ViewXml, True)
            If navDetaliu.SelectedKey = ViewXml Then RefreshXml()
            ' Slice KE03: the links with receptions (and the value with VAT of every line) come from the server in their own call.
            Await LoadKeAsync(k_id).ConfigureAwait(True)
            ' The attached files and the notes of the XML have no view of their own any more (operator, 15.10.2026): the files
            ' are in the popup of «Factura PDF», the notes are above the lines of «Continut».
            If navDetaliu.SelectedKey = ViewPdf Then RefreshPdf()
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.ShowInvoiceAsync", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.ShowInvoiceAsync", ex)
            ShowProblem("Factura nu a putut fi citită. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Sub FillLines(k_detail As EFacturaPrimitaDetaliu)
        Dim k_lines As KBotDataView = gridLinii()
        k_lines.BeginUpdate()
        Try
            k_lines.ClearRows()
            For Each k_l As EFacturaPrimitaLinie In k_detail.Linii
                Dim k_row As KBotDataRow = k_lines.AddRow()
                k_row.Tag = k_l
                k_row("nr") = k_l.NrLinie
                k_row("denumire") = k_l.Denumire
                k_row("cpv") = k_l.Cpv
                k_row("um") = k_l.Unit
                k_row("cant") = k_l.Cant
                k_row("pret") = k_l.Pret
                k_row("valoare") = k_l.Valoare
            Next
        Finally
            k_lines.EndUpdate()
        End Try
        pgLinii.ShowSupplier(k_detail)
    End Sub

    ' «Mesaje»: only the messages of the E-Factura system about the invoice (the notes of the XML are above the lines of «Continut»).
    Private Sub FillMessages(k_detail As EFacturaPrimitaDetaliu)
        Dim k_grid As KBotDataView = gridMesaje()
        k_grid.BeginUpdate()
        Try
            k_grid.ClearRows()
            For Each k_m As EFacturaPrimitaMesaj In k_detail.Mesaje
                Dim k_row As KBotDataRow = k_grid.AddRow()
                k_row("data") = If(k_m.DataMesaj.HasValue, k_m.DataMesaj.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture), String.Empty)
                k_row("text") = k_m.Mesaj
            Next
        Finally
            k_grid.EndUpdate()
        End Try
    End Sub

    ' ── The views ───────────────────────────────────────────────────────────────

    ' Reads the XML of the invoice shown and gives it to the view (only when it is the view on screen, and once per invoice).
    ' UI boundary (async Sub): logs and tells the operator in the view.
    Private Async Sub RefreshXml()
        Try
            Dim k_d As EFacturaPrimitaDetaliu = _detail
            If k_d Is Nothing OrElse _shownId = 0 Then
                pgXml.Clear()
                Return
            End If
            Dim k_id As Integer = k_d.Factura.IdPrimita
            If _xmlShownId = k_id Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_xmlSeq)
            pgXml.ShowNotice("Se aduce XML-ul facturii…")
            Dim k_bytes As Byte() = Await _gate.RunAsync(Function() _api.GetPrimitaXmlAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _xmlSeq Then Return
            pgXml.ShowXml(System.Text.Encoding.UTF8.GetString(k_bytes).TrimStart(ChrW(&HFEFF)))
            _xmlShownId = k_id
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.RefreshXml", ex)
            If Not IsDisposed Then pgXml.ShowNotice(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.RefreshXml", ex)
            If Not IsDisposed Then pgXml.ShowNotice("XML-ul facturii nu a putut fi citit. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Private Sub NavDetaliu_SelectionChanged(k_key As String) Handles navDetaliu.SelectionChanged
        Try
            ShowPage(k_key)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.NavDetaliu_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub ShowPage(k_key As String)
        pgLinii.Visible = String.Equals(k_key, ViewLinii, StringComparison.Ordinal)
        pgPdf.Visible = String.Equals(k_key, ViewPdf, StringComparison.Ordinal)
        pgMesaje.Visible = String.Equals(k_key, ViewMesaje, StringComparison.Ordinal)
        pgReceptii.Visible = String.Equals(k_key, ViewReceptii, StringComparison.Ordinal)
        pgXml.Visible = String.Equals(k_key, ViewXml, StringComparison.Ordinal)
        If pgXml.Visible Then RefreshXml()
        If pgPdf.Visible Then RefreshPdf()
    End Sub

    ''' <summary>
    ''' Opens the document the bar of «Factură PDF» asks for: the classic invoice K-BOT draws from the stored data, or the one ANAF's
    ''' service draws from the XML (an attachment is opened by <see cref="PgPdf_AttachmentChosen"/>). A failure becomes a line inside
    ''' the viewer. UI boundary (async Sub): logs and swallows.
    ''' </summary>
    Private Async Sub RefreshPdf()
        Try
            Dim k_d As EFacturaPrimitaDetaliu = _detail
            If k_d Is Nothing OrElse _shownId = 0 Then
                pgPdf.ShowNotice("Alegeți o factură din arbore.")
                Return
            End If
            Dim k_mode As String = pgPdf.Mode
            If String.Equals(k_mode, PrimitePdfPage.ModeAttachments, StringComparison.Ordinal) Then Return
            Dim k_id As Integer = k_d.Factura.IdPrimita
            Dim k_stamp As String = $"{k_id}|{k_mode}"
            If String.Equals(k_stamp, pgPdf.Stamp, StringComparison.Ordinal) Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_pdfSeq)
            Dim k_label As String = Regex.Replace(k_d.Factura.NrFact, "[^A-Za-z0-9_.-]", "_")
            Dim k_path As String
            If String.Equals(k_mode, PrimitePdfPage.ModeAnaf, StringComparison.Ordinal) Then
                pgPdf.ShowNotice("Se aduce factura de la ANAF…")
                Dim k_bytes As Byte() = Await _gate.RunAsync(Function() _api.GetPrimitaPdfAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _pdfSeq Then Return
                TempPdfStore.EnsureRoot()
                k_path = TempPdfStore.PathFor($"primita_anaf_{k_label}_{DateTime.Now:HHmmssfff}.pdf")
                File.WriteAllBytes(k_path, k_bytes)
            Else
                TempPdfStore.EnsureRoot()
                k_path = TempPdfStore.PathFor($"primita_{k_label}_{DateTime.Now:HHmmssfff}.pdf")
                FacturaPrimitaPdf.Write(k_path, k_d)
            End If
            pgPdf.ShowDocument(k_path)
            pgPdf.Stamp = k_stamp
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.RefreshPdf", ex)
            If Not IsDisposed Then pgPdf.ShowNotice(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.RefreshPdf", ex)
            If Not IsDisposed Then pgPdf.ShowNotice("Documentul nu a putut fi pregătit. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' The bar of the «Factură PDF» view moved to «Factură clasică» / «Factură ANAF». UI boundary: logs and swallows.
    Private Sub PgPdf_ModeChanged(k_mode As String) Handles pgPdf.ModeChanged
        Try
            If pgPdf.Visible Then RefreshPdf()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgPdf_ModeChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The operator chose an attachment in the popup: it opens in the same document area (a PDF in the viewer, an image in the
    ''' picture box; any other kind is only named, the «Atașamente» view saves it). UI boundary (async Sub): logs and swallows.
    ''' </summary>
    Private Async Sub PgPdf_AttachmentChosen(k_index As Integer) Handles pgPdf.AttachmentChosen
        Try
            Dim k_d As EFacturaPrimitaDetaliu = _detail
            If k_d Is Nothing Then Return
            Dim k_file As EFacturaPrimitaAtasament = k_d.Atasamente.FirstOrDefault(Function(k_a) k_a.Index = k_index)
            If k_file Is Nothing Then Return
            Dim k_id As Integer = k_d.Factura.IdPrimita
            Dim k_stamp As String = $"{k_id}|atas{k_index.ToString(CultureInfo.InvariantCulture)}"
            If String.Equals(k_stamp, pgPdf.Stamp, StringComparison.Ordinal) Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_pdfSeq)
            Dim k_ext As String = Path.GetExtension(k_file.Nume).ToLowerInvariant()
            Dim k_isPdf As Boolean = k_file.Mime.Contains("pdf", StringComparison.OrdinalIgnoreCase) OrElse k_ext = ".pdf"
            Dim k_isImage As Boolean = k_file.Mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase) OrElse
                                       {".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tif", ".tiff"}.Contains(k_ext)
            If Not k_isPdf AndAlso Not k_isImage Then
                pgPdf.ShowNotice($"«{k_file.Nume}» nu se poate afișa aici.")
                Return
            End If
            pgPdf.ShowNotice("Se deschide atașamentul…")
            Dim k_bytes As Byte() = Await _gate.RunAsync(
                Function() _api.GetPrimitaAtasamentAsync(k_id, k_index, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _pdfSeq Then Return
            If k_isPdf Then
                Dim k_label As String = Regex.Replace(k_d.Factura.NrFact, "[^A-Za-z0-9_.-]", "_")
                TempPdfStore.EnsureRoot()
                Dim k_path As String = TempPdfStore.PathFor($"primita_atas_{k_label}_{k_index.ToString(CultureInfo.InvariantCulture)}_{DateTime.Now:HHmmssfff}.pdf")
                File.WriteAllBytes(k_path, k_bytes)
                pgPdf.ShowDocument(k_path)
            Else
                pgPdf.ShowImage(k_bytes)
            End If
            pgPdf.Stamp = k_stamp
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.PgPdf_AttachmentChosen", ex)
            If Not IsDisposed Then pgPdf.ShowNotice(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgPdf_AttachmentChosen", ex)
            If Not IsDisposed Then pgPdf.ShowNotice("Atașamentul nu a putut fi deschis. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ''' <summary>
    ''' The operator wrote a message for the supplier and pressed «Trimite»: after a confirmation it goes through ANAF, the invoice is read
    ''' again and the new message shows in the list. A failure becomes a line in the notice. UI boundary (async Sub): logs and swallows.
    ''' </summary>
    Private Async Sub PgMesaje_SendRequested(k_text As String) Handles pgMesaje.SendRequested
        Try
            Dim k_id As Integer = _shownId
            If k_id = 0 Then Return
            If KBotMessage.Show(FindForm(), "Trimiteți mesajul furnizorului, prin ANAF? Mesajul nu mai poate fi retras.", "Mesaj către furnizor",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            ntfMesaj.Visible = False
            pgMesaje.SetBusy(True)
            Try
                Await _gate.RunAsync(Function() _api.SendPrimitaMesajAsync(k_id, k_text, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                pgMesaje.ClearInput()
            Finally
                If Not IsDisposed Then pgMesaje.SetBusy(False)
            End Try
            If _shownId = k_id Then Await ShowInvoiceAsync(k_id).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.PgMesaje_SendRequested", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PgMesaje_SendRequested", ex)
            ShowProblem("Mesajul nu a putut fi trimis. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Tree header ─────────────────────────────────────────────────────────────

    Private Const OptFaraOperatii As String = "fara_operatii"
    Private Const OptNoi As String = "noi"
    Private Const OptPeFurnizor As String = "grup_furnizor"
    Private Const OptPeLuna As String = "grup_luna"
    Private Const OptCrescator As String = "ord_cresc"
    Private Const OptDescrescator As String = "ord_desc"

    ''' <summary>
    ''' The options button of the tree's header opens the menu of what is shown and how the tree is grouped (slice 00EF-35):
    ''' two filters (the invoices with no operations in AVACONT; the invoices brought in this session -- offered only when this session
    ''' brought some, otherwise nothing tells which are new, and never in the view of an angajament, slice 0114-17), a separator, the grouping (only the one that is NOT in force is
    ''' offered: it is the switch), and, with the supplier as the root, a separator and the two orders of its invoices.
    ''' UI boundary: logs and swallows.
    ''' </summary>
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            If _angajamentView Then Return   ' the view has no button; a stray click does nothing
            ' A second press on the button CLOSES the menu (the press already closed it, so without this guard it would reopen at once).
            If CustomPopup.ClosedJustNow Then Return
            Dim k_anchor As Rectangle = tree.HeaderRightIconRect
            If k_anchor.IsEmpty Then Return
            Dim k_items As New List(Of CustomPopupItem) From {
                New CustomPopupItem(OptFaraOperatii, "Afișare facturi fără operații în AVACONT") With {.Checked = _onlyWithoutOperations}}
            If _newIds.Count > 0 AndAlso Not _idDdf.HasValue Then
                k_items.Add(New CustomPopupItem(OptNoi, "Afișare facturi noi") With {.Checked = _onlyNew})
            End If
            k_items.Add(CustomPopupItem.Separator())
            If _groupBySupplier Then
                k_items.Add(New CustomPopupItem(OptPeLuna, "Grupare după lună"))
                k_items.Add(CustomPopupItem.Separator())
                k_items.Add(New CustomPopupItem(OptCrescator, "Ordonare crescătoare facturi") With {.Checked = _ascending})
                k_items.Add(New CustomPopupItem(OptDescrescator, "Ordonare descrescătoare facturi") With {.Checked = Not _ascending})
            Else
                k_items.Add(New CustomPopupItem(OptPeFurnizor, "Grupare după furnizor"))
            End If
            Dim k_menu As New CustomPopup(k_items)
            AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) OptionChosen(k_args.Item.Key)
            k_menu.ShowBelow(tree, k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ' A row of the options menu: the option changes and the tree is built again with the shown invoice kept. UI boundary: logs and swallows.
    Private Sub OptionChosen(k_key As String)
        Try
            Select Case k_key
                Case OptFaraOperatii
                    _onlyWithoutOperations = Not _onlyWithoutOperations
                Case OptNoi
                    _onlyNew = Not _onlyNew
                Case OptPeFurnizor
                    _groupBySupplier = True
                Case OptPeLuna
                    _groupBySupplier = False
                Case OptCrescator
                    _ascending = True
                Case OptDescrescator
                    _ascending = False
                Case Else
                    Throw New ArgumentException($"Rând necunoscut în meniul de opțiuni: {k_key}", NameOf(k_key))
            End Select
            BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.OptionChosen", ex)
        End Try
    End Sub

    ' ── «Actualizeaza»: the footer of the tree (slice 00EF-33; the caption and the icon are those of the angajament tree, slice 0114-17) ──

    Private Const FooterIdleText As String = "Actualizează"
    Private Const SyncCaption As String = "Sincronizează facturi primite"
    Private Const SyncBatch As Integer = 20

    ' The rows of the popup's list, and the days each one stands for (the first is the usual one).
    Private Shared ReadOnly SyncDayChoices As Integer() = {7, 15, 30, 45, 60}

    ''' <summary>Raised with the line that tells how an update goes (the same text the footer shows); a host may show it too.</summary>
    Public Event SyncStatus(k_text As String)

    Private Sub Tree_FooterRightIconClicked(e As MouseEventArgs) Handles tree.FooterRightIconClicked
        OpenSyncPopup()
    End Sub

    Private Sub Tree_FooterCaptionClicked(e As MouseEventArgs) Handles tree.FooterCaptionClicked
        OpenSyncPopup()
    End Sub

    Private Const MenuZileCombo As String = "zile_combo"
    Private Const MenuZileText As String = "zile_text"
    Private Const MenuServer As String = "server"

    ' The button of the footer opens the standard popup menu with two input rows: a list of the preset periods (picking one starts the
    ' update) and a box for a number of days (Enter starts it). UI boundary: logs and swallows.
    Private Sub OpenSyncPopup()
        Try
            If _api Is Nothing OrElse _syncing Then Return
            If CustomPopup.ClosedJustNow Then Return
            Dim k_band As Rectangle = tree.FooterBandRect
            If k_band.IsEmpty Then Return
            Dim k_combo As CustomPopupItem = CustomPopupItem.Combo(MenuZileCombo, "Perioada de adus",
                SyncDayChoices.Select(Function(k_d) $"Ultimele {k_d.ToString(CultureInfo.InvariantCulture)} zile"))
            k_combo.Image = My.Resources.Resources.anaf
            Dim k_text As CustomPopupItem = CustomPopupItem.TextInput(MenuZileText, "Între 1 și 60 de zile", "Între 1 și 60 de zile", 2, True)
            k_text.Image = My.Resources.Resources.anaf
            ' Under the two ANAF rows, a separator, then the other way to update: read the list again from the K-BOT server.
            Dim k_items As New List(Of CustomPopupItem) From {
                k_combo, k_text, CustomPopupItem.Separator(),
                New CustomPopupItem(MenuServer, "Sincronizează cu serverul", My.Resources.Resources.refresh)}
            Dim k_menu As New CustomPopup(k_items) With {.IconBarSide = PopupIconBarSide.Right}   ' bar and text on the right, by the button
            AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) SyncMenuAnswered(k_args.Item)
            ' Over the button: above the footer, its right edge on the button's right edge.
            Dim k_icon As Rectangle = tree.FooterRightIconRect
            k_menu.ShowAboveRightAligned(tree, k_band, If(k_icon.IsEmpty, k_band.Right, k_icon.Right))
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.OpenSyncPopup", ex)
        End Try
    End Sub

    ' The answer of the popup: the list (a preset period) or the box (days written by hand).
    Private Sub SyncMenuAnswered(k_item As CustomPopupItem)
        If String.Equals(k_item.Key, MenuZileCombo, StringComparison.Ordinal) Then
            PickedDays(k_item.ComboSelectedIndex)
        ElseIf String.Equals(k_item.Key, MenuZileText, StringComparison.Ordinal) Then
            TypedDays(k_item.InputText)
        ElseIf String.Equals(k_item.Key, MenuServer, StringComparison.Ordinal) Then
            ReloadFromServer()
        Else
            Throw New ArgumentException($"Rând necunoscut în meniul de actualizare: {k_item.Key}", NameOf(k_item))
        End If
    End Sub

    ' «Sincronizează cu serverul»: the list is read again from the K-BOT server (what ANAF brought is already there); the shown
    ' invoice stays shown. UI boundary (async Sub): LoadAsync tells the operator of a failure itself.
    Private Async Sub ReloadFromServer()
        Try
            If _syncing Then Return
            Await LoadAsync(_idDdf).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.ReloadFromServer", ex)
        End Try
    End Sub

    ' The list: the row is one of the preset periods. UI boundary (async Sub): logs and swallows.
    Private Async Sub PickedDays(k_index As Integer)
        Try
            Dim k_days As Integer = SyncDayChoices(Math.Max(0, Math.Min(SyncDayChoices.Length - 1, k_index)))
            Await SyncFromAnafAsync(k_days).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.PickedDays", ex)
            If Not IsDisposed Then
                KBotMessage.Show(FindForm(), "Sincronizarea facturilor primite nu a putut fi dusă la capăt. Detalii în jurnalul de erori.",
                                 SyncCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    ' The box: Enter with a number of days. Empty or out of 1-60 = nothing starts, the operator is told. UI boundary: logs and swallows.
    Private Async Sub TypedDays(k_text As String)
        Try
            Dim k_days As Integer
            If Not Integer.TryParse(k_text, NumberStyles.None, CultureInfo.InvariantCulture, k_days) OrElse k_days < 1 OrElse k_days > 60 Then
                KBotMessage.Show(FindForm(), "Numărul de zile trebuie să fie între 1 și 60.", SyncCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Await SyncFromAnafAsync(k_days).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.TypedDays", ex)
            If Not IsDisposed Then
                KBotMessage.Show(FindForm(), "Sincronizarea facturilor primite nu a putut fi dusă la capăt. Detalii în jurnalul de erori.",
                                 SyncCaption, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Sub SetSyncStatus(k_text As String, k_footer As String)
        If IsDisposed Then Return
        tree.FooterCaption = k_footer
        tree.Invalidate()
        RaiseEvent SyncStatus(k_text)
    End Sub

    ''' <summary>
    ''' Brings the received invoices of the last <paramref name="k_days"/> days (1 to 60) from ANAF, a batch at a time, until the server
    ''' says nothing is left; the progress is the footer's caption (and <see cref="SyncStatus"/>), then the list is read again when
    ''' something was added, and the end tells what came: a window with the NEW invoices, or «Nu există facturi noi pe serverul ANAF.».
    ''' The server skips what it already has, so it can be run again and again. One at a time (a second call returns at once).
    ''' A failure is logged and thrown to the caller.
    ''' </summary>
    Public Async Function SyncFromAnafAsync(k_days As Integer) As Task
        Try
            If k_days < 1 OrElse k_days > 60 Then Throw New ArgumentOutOfRangeException(NameOf(k_days), "Zile între 1 și 60.")
            If _api Is Nothing OrElse _syncing Then Return
            _syncing = True
            Try
                Dim k_problems As New List(Of String)()
                Dim k_done As Integer
                Dim k_total As Integer
                Dim k_added As Boolean
                Dim k_addedCount As Integer
                ' What is new = what was not in the list before: the list is read first if it never was.
                If Not ListLoaded Then Await LoadAsync(_idDdf).ConfigureAwait(True)
                Dim k_before As New HashSet(Of Integer)(_items.Select(Function(k_x) k_x.IdPrimita))
                Dim k_baseline As Boolean = ListLoaded
                SetSyncStatus("Se citește lista de mesaje de la ANAF…", "Se citește lista de la ANAF…")
                Do
                    Dim k_answer As EFacturaSincronizare = Await _gate.RunAsync(
                        Function() _api.SyncPrimiteAsync(k_days, SyncBatch, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    If k_total = 0 Then k_total = k_answer.Adaugate + k_answer.Ramase + k_answer.Erori.Count
                    k_done += k_answer.Adaugate + k_answer.Erori.Count
                    If k_answer.Adaugate > 0 Then k_added = True
                    k_addedCount += k_answer.Adaugate
                    k_problems.AddRange(k_answer.Erori)
                    If k_total > 0 Then
                        Dim k_of As String = $"{Math.Min(k_done, k_total).ToString(CultureInfo.CurrentCulture)} din {k_total.ToString(CultureInfo.CurrentCulture)}"
                        SetSyncStatus("Se aduc facturile de la ANAF… " & k_of, "Se aduc… " & k_of)
                    End If
                    ' Stop when nothing is left, or when a whole batch brought nothing (the same messages would come back forever).
                    If k_answer.Ramase = 0 OrElse k_answer.Adaugate = 0 Then Exit Do
                Loop
                SetSyncStatus(If(k_problems.Count = 0, "Facturile primite sunt la zi.",
                                 "Sincronizarea s-a încheiat, dar unele mesaje nu au putut fi citite."), FooterIdleText)
                If k_added Then Await LoadAsync(_idDdf).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_problems.Count > 0 Then
                    KBotMessage.Show(FindForm(), "Nu s-au putut citi:" & vbLf & String.Join(vbLf, k_problems.Take(4)) &
                                     If(k_problems.Count > 4, vbLf & "…", String.Empty),
                                     SyncCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
                ShowSyncResult(k_added, k_addedCount, k_baseline, k_before, k_problems.Count > 0)
            Finally
                _syncing = False
                If Not IsDisposed Then tree.FooterCaption = FooterIdleText
            End Try
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            If Not IsDisposed Then
                If NoMessagesAnswer(ex.Message) Then
                    ' ANAF answers «there are no messages in the last N days» as an error text: that is NOT a problem, only news
                    ' (operator, 15.10.2026) -- an info message, nothing in the error log.
                    RaiseEvent SyncStatus("Facturile primite sunt la zi.")
                    KBotMessage.Show(FindForm(), "Nu există facturi noi pe serverul ANAF.", SyncCaption,
                                     MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    GlobalErrorLog.Write("PrimiteView.SyncFromAnafAsync", ex)
                    RaiseEvent SyncStatus("Sincronizarea s-a oprit.")
                    KBotMessage.Show(FindForm(), ex.Message, SyncCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            Else
                GlobalErrorLog.Write("PrimiteView.SyncFromAnafAsync", ex)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.SyncFromAnafAsync", ex)
            If Not IsDisposed Then RaiseEvent SyncStatus("Sincronizarea s-a oprit.")
            Throw
        End Try
    End Function

    ' ANAF's text for «no messages in the period» («Nu exista mesaje in ultimele N zile pentru cif=...»), in any case or with diacritics.
    Private Shared Function NoMessagesAnswer(k_text As String) As Boolean
        Return Not String.IsNullOrEmpty(k_text) AndAlso
               Regex.IsMatch(k_text, "nu\s+exist\w*\s+mesaje", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant)
    End Function

    ' What the end of an update tells: a window with the NEW invoices (those not in the list before), or an info message. Nothing new
    ' and no unreadable message = «Nu există facturi noi pe serverul ANAF.»; brought but not in this list (another year than the one
    ' chosen in K-BOT, or the list could not be compared) = a message with the count.
    Private Sub ShowSyncResult(k_added As Boolean, k_addedCount As Integer, k_baseline As Boolean,
                               k_before As HashSet(Of Integer), k_hadProblems As Boolean)
        If Not k_added Then
            If Not k_hadProblems Then
                KBotMessage.Show(FindForm(), "Nu există facturi noi pe serverul ANAF.", SyncCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            Return
        End If
        Dim k_new As List(Of EFacturaPrimita) = If(k_baseline,
            _items.Where(Function(k_x) Not k_before.Contains(k_x.IdPrimita)).ToList(), New List(Of EFacturaPrimita)())
        If k_new.Count = 0 Then
            KBotMessage.Show(FindForm(), $"S-au descărcat {k_addedCount.ToString(CultureInfo.CurrentCulture)} facturi noi de la ANAF" &
                             If(k_baseline, ", dar nu sunt din anul ales în K-BOT.", "."),
                             SyncCaption, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        ' Known to be new for the rest of the session: the header menu can show only them.
        For Each k_x As EFacturaPrimita In k_new
            _newIds.Add(k_x.IdPrimita)
        Next
        ' The same sentence the window shows is kept in the operator's message log (nothing the operator read is lost).
        OperatorLog.Write("PrimiteView.SyncFromAnafAsync", SyncCaption,
                          $"S-au descărcat {k_new.Count.ToString(CultureInfo.CurrentCulture)} facturi noi: " &
                          String.Join(", ", k_new.Take(20).Select(Function(k_x) $"{k_x.NrFact} ({k_x.Furnizor})")),
                          KBotLogLevel.Info)
        Using k_dialog As New SincronizareNoiForm(k_new)
            k_dialog.ShowDialog(FindForm())
        End Using
    End Sub

    ' ── Menu and files ──────────────────────────────────────────────────────────

    Private Sub ShowMenu(k_p As EFacturaPrimita, k_screen As Point)
        Dim k_items As New List(Of CustomPopupItem) From {
            New CustomPopupItem(MenuZip, "Salvează ca &ZIP"),
            New CustomPopupItem(MenuXml, "Salvează ca &XML")}
        If _idDdf.HasValue Then
            Dim k_how As String = Nothing
            If Not _linked.TryGetValue(k_p.IdPrimita, k_how) Then
                k_items.Add(CustomPopupItem.Separator())
                k_items.Add(New CustomPopupItem(MenuLega, "&Leagă de acest DDF"))
            ElseIf k_how = "manual" Then
                k_items.Add(CustomPopupItem.Separator())
                k_items.Add(New CustomPopupItem(MenuScoate, "Scoate legătura cu acest &DDF"))
            End If
        Else
            ' Without a DDF (the window of all the invoices): the link is chosen from a list of fundamentari (slice 00EF-20). The links
            ' the operator made are known once the invoice is shown (a click on it), so they are offered for removal then.
            k_items.Add(CustomPopupItem.Separator())
            k_items.Add(New CustomPopupItem(MenuLegaAlege, "&Leagă de un DDF…"))
            Dim k_d As EFacturaPrimitaDetaliu = _detail
            If k_d IsNot Nothing AndAlso k_d.Factura.IdPrimita = k_p.IdPrimita Then
                For Each k_link As EFacturaPrimitaDdf In k_d.LegaturiManuale
                    k_items.Add(New CustomPopupItem(MenuScoatePrefix & k_link.IdDdf.ToString(CultureInfo.InvariantCulture),
                                                    $"Scoate legătura cu {k_link.CodAngajament}"))
                Next
            End If
        End If
        Dim k_menu As New CustomPopup(k_items)
        AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) RunMenuAction(k_args.Item.Key, k_p)
        k_menu.ShowAt(tree, k_screen)
    End Sub

    ' UI boundary (async Sub): logs and swallows; a failure becomes a line in the notice.
    Private Async Sub RunMenuAction(k_key As String, k_p As EFacturaPrimita)
        Try
            If k_key.StartsWith(MenuScoatePrefix, StringComparison.Ordinal) Then
                Dim k_idDdf As Integer = Integer.Parse(k_key.Substring(MenuScoatePrefix.Length), CultureInfo.InvariantCulture)
                Await _gate.RunAsync(Function() _api.UnlinkPrimitaAsync(k_p.IdPrimita, k_idDdf, _cts.Token)).ConfigureAwait(True)
                Await ReloadKeepingSelectionAsync(k_p.IdPrimita).ConfigureAwait(True)
                Return
            End If
            Select Case k_key
                Case MenuLegaAlege
                    Using k_dialog As New AlegeDdfForm(_api, _gate, $"{k_p.NrFact} ({k_p.Furnizor})")
                        If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK OrElse k_dialog.Ales Is Nothing Then Return
                        Dim k_target As Integer = k_dialog.Ales.IdDdf
                        Await _gate.RunAsync(Function() _api.LinkPrimitaAsync(k_p.IdPrimita, k_target, _cts.Token)).ConfigureAwait(True)
                    End Using
                    Await ReloadKeepingSelectionAsync(k_p.IdPrimita).ConfigureAwait(True)
                Case MenuZip
                    Await SaveFileAsync(k_p, "zip", "Arhivă ZIP (*.zip)|*.zip",
                                        Function() _api.GetPrimitaZipAsync(k_p.IdPrimita, _cts.Token)).ConfigureAwait(True)
                Case MenuXml
                    Await SaveFileAsync(k_p, "xml", "Fișier XML (*.xml)|*.xml",
                                        Function() _api.GetPrimitaXmlAsync(k_p.IdPrimita, _cts.Token)).ConfigureAwait(True)
                Case MenuLega
                    Await _gate.RunAsync(Function() _api.LinkPrimitaAsync(k_p.IdPrimita, _idDdf.Value, _cts.Token)).ConfigureAwait(True)
                    Await LoadAsync(_idDdf).ConfigureAwait(True)
                Case MenuScoate
                    Await _gate.RunAsync(Function() _api.UnlinkPrimitaAsync(k_p.IdPrimita, _idDdf.Value, _cts.Token)).ConfigureAwait(True)
                    Await LoadAsync(_idDdf).ConfigureAwait(True)
                Case Else
                    ' No silent no-ops: an unknown key is a programming defect.
                    Throw New ArgumentException($"Comandă de meniu necunoscută: {k_key}", NameOf(k_key))
            End Select
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.RunMenuAction", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.RunMenuAction", ex)
            ShowProblem("Acțiunea nu a putut fi dusă la capăt. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' The list is read again and the invoice shown again (its links changed).
    Private Async Function ReloadKeepingSelectionAsync(k_id As Integer) As Task
        Await LoadAsync(_idDdf).ConfigureAwait(True)
        If _shownId = k_id Then Await ShowInvoiceAsync(k_id).ConfigureAwait(True)
    End Function

    ' Asks where to save, then fetches and writes. Nothing is fetched when the operator cancels the dialog.
    Private Async Function SaveFileAsync(k_p As EFacturaPrimita, k_extension As String, k_filter As String,
                                         k_fetch As Func(Of Task(Of Byte()))) As Task
        Using k_dialog As New SaveFileDialog() With {
            .Filter = k_filter, .DefaultExt = k_extension, .AddExtension = True, .OverwritePrompt = True,
            .FileName = SuggestedName(k_p, k_extension)}
            If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            Dim k_path As String = k_dialog.FileName
            Dim k_bytes As Byte() = Await _gate.RunAsync(k_fetch).ConfigureAwait(True)
            File.WriteAllBytes(k_path, k_bytes)
        End Using
    End Function

    Private Shared Function SuggestedName(k_p As EFacturaPrimita, k_extension As String) As String
        Dim k_base As String = String.Join("_", {k_p.CuiNormalizat, k_p.NrFact}.Where(Function(k_s) Not String.IsNullOrWhiteSpace(k_s)))
        If k_base.Length = 0 Then k_base = "factura_" & k_p.IdPrimita.ToString(CultureInfo.InvariantCulture)
        Return Regex.Replace(k_base, "[^A-Za-z0-9_.-]", "_") & "." & k_extension
    End Function

    ' ── Theme ───────────────────────────────────────────────────────────────────

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            If _items.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.ApplyTheme", ex)
        End Try
    End Sub

End Class
