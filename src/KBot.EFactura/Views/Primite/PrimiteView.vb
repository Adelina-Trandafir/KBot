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
''' all the received invoices. On the left a TREE: a root «Toate facturile», under it the months, under each month the invoices
''' (the dot says whether it was opened: orange = new). On the right the chosen invoice, in VIEWS chosen from a horizontal bar (the
''' pattern of the DDF view): «Linii» (the lines and the VAT per rate), «Factură PDF» (the classic PDF the supplier embedded in the XML,
''' else the one ANAF draws), «Atașamente» and «Mesaje» (the last two only when the invoice has some). A right click on an invoice
''' offers «Salvează ca ZIP» and «Salvează ca XML» and, in the DDF view, the manual link.
'''
''' <para>Two modes. WITHOUT a DDF (<see cref="LoadAsync"/> with Nothing): every received invoice of the year. WITH a DDF: the invoices of
''' the DDF — the supplier is one of its partners (compared by tax code, «RO» and spaces ignored) or the operator linked it by hand; a
''' tick shows ALL the invoices, the unlinked ones with a grey dot, so one can be linked to this DDF from its menu.</para>
'''
''' <para>The control holds no rules: what is linked to what, the amounts and the files come from the server (slice 00EF-17).
''' The embedded PDF viewer lives in KBot.App; the host passes a factory (as for the issued invoices).</para>
''' </summary>
Public Class PrimiteView
    Implements IThemedContainer

    ' The views of the horizontal bar (the keys of navDetaliu's items, written in the designer).
    Private Const ViewLinii As String = "linii"
    Private Const ViewPdf As String = "pdf"
    Private Const ViewAtasamente As String = "atasamente"
    Private Const ViewMesaje As String = "mesaje"

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
    Private _filter As String = String.Empty

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
        AddHandler pgAtasamente.gridAtasamente.CellDoubleClick, AddressOf GridAtasamente_CellDoubleClick
    End Sub

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
        chkToate.Visible = False
        ntfMesaj.Visible = False
        tree.Clear()
        _nodes.Clear()
        ClearDetail()
    End Sub

    ''' <summary>Lets go of the PDF on screen (the view is put away).</summary>
    Public Sub ReleaseDocument()
        pgPdf.ReleaseDocument()
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
            chkToate.Visible = _idDdf.HasValue
            txtCauta.Visible = Not _idDdf.HasValue
            If k_ddfChanged Then
                chkToate.Checked = False
                ClearDetail()
            End If
            ntfMesaj.Clear()
            ntfMesaj.Visible = False
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            Dim k_all As Boolean = Not _idDdf.HasValue OrElse chkToate.Checked
            Dim k_list As List(Of EFacturaPrimita) = Await _gate.RunAsync(
                Function() _api.GetPrimiteAsync(If(_idDdf.HasValue, Nothing, YearFilter()), Nothing, Nothing,
                                                If(k_all AndAlso _idDdf.HasValue, Nothing, _idDdf), _cts.Token)).ConfigureAwait(True)
            Dim k_linkedIds As New Dictionary(Of Integer, String)()
            If _idDdf.HasValue Then
                If k_all Then
                    Dim k_mine As List(Of EFacturaPrimita) = Await _gate.RunAsync(
                        Function() _api.GetPrimiteAsync(Nothing, Nothing, Nothing, _idDdf, _cts.Token)).ConfigureAwait(True)
                    For Each k_p As EFacturaPrimita In k_mine
                        k_linkedIds(k_p.IdPrimita) = k_p.Legatura
                    Next
                Else
                    For Each k_p As EFacturaPrimita In k_list
                        k_linkedIds(k_p.IdPrimita) = k_p.Legatura
                    Next
                End If
            End If
            If IsDisposed OrElse k_mySeq <> _seq Then Return
            _items = k_list
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

    Private Async Sub ChkToate_CheckedChanged(sender As Object, e As EventArgs) Handles chkToate.CheckedChanged
        Try
            If _treeLoading Then Return
            Await LoadAsync(_idDdf).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.ChkToate_CheckedChanged", ex)
        End Try
    End Sub

    ''' <summary>The text in the search box narrows the tree (supplier, tax code or number). UI boundary: logs and swallows.</summary>
    Private Sub TxtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        Try
            _filter = txtCauta.Text.Trim()
            If _items.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.TxtCauta_TextChanged", ex)
        End Try
    End Sub

    ' The invoices the tree shows: all, or those matching the search text.
    Private Function ShownItems() As List(Of EFacturaPrimita)
        If _filter.Length = 0 Then Return _items
        Return _items.Where(Function(k_x) k_x.Furnizor.Contains(_filter, StringComparison.CurrentCultureIgnoreCase) OrElse
                                          k_x.Cui.Contains(_filter, StringComparison.CurrentCultureIgnoreCase) OrElse
                                          k_x.NrFact.Contains(_filter, StringComparison.CurrentCultureIgnoreCase)).ToList()
    End Function

    Private Sub ShowProblem(k_text As String)
        ntfMesaj.Show(k_text, NoticeKind.Error)
        ntfMesaj.Visible = True
    End Sub

    ' ── The tree ────────────────────────────────────────────────────────────────

    ''' <summary>Rebuilds the tree from <c>_items</c>: the root «Toate facturile», the months newest first, the invoices of a month newest first.</summary>
    Private Sub BuildTree(k_selectId As Integer?)
        _treeLoading = True
        Try
            tree.Clear()
            _nodes.Clear()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            Dim k_folder As Image = FacturaIcons.Client(k_palette, tree.LeftIconSize.Width)
            Dim k_shown As List(Of EFacturaPrimita) = ShownItems()
            Dim k_total As Decimal = k_shown.Sum(Function(k_x) k_x.Total)
            Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem(
                "R", $"Toate facturile ({k_shown.Count})~~~{k_total.ToString("N2", _ro)}",
                pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
            k_root.Bold = True
            Dim k_months As IEnumerable(Of IGrouping(Of Integer, EFacturaPrimita)) =
                k_shown.GroupBy(Function(k_x) MonthKey(k_x)).OrderByDescending(Function(k_g) k_g.Key)
            For Each k_group As IGrouping(Of Integer, EFacturaPrimita) In k_months
                Dim k_hasSelected As Boolean = k_selectId.HasValue AndAlso k_group.Any(Function(k_x) k_x.IdPrimita = k_selectId.Value)
                Dim k_sum As Decimal = k_group.Sum(Function(k_x) k_x.Total)
                Dim k_month As AdvancedTreeControl.TreeItem = tree.AddItem(
                    "M_" & k_group.Key.ToString(CultureInfo.InvariantCulture),
                    $"{MonthLabel(k_group.Key)}~~~{k_sum.ToString("N2", _ro)}", k_root,
                    pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=k_hasSelected)
                k_month.Bold = True
                For Each k_p As EFacturaPrimita In k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita)
                    Dim k_dot As Image = FacturaIcons.StateDot(DotOf(k_p), k_palette, tree.LeftIconSize.Width)
                    Dim k_caption As String = $"{If(k_p.Tip = "NC", "NC ", String.Empty)}{k_p.NrFact} · {k_p.Furnizor}~~~{k_p.Total.ToString("N2", _ro)}"
                    Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                        "P_" & k_p.IdPrimita.ToString(CultureInfo.InvariantCulture), k_caption, k_month,
                        pLeftIconClosed:=k_dot, pLeftIconOpen:=k_dot)
                    k_leaf.Tag = k_p
                    k_leaf.Tooltip = TooltipOf(k_p)
                    _nodes(k_p.IdPrimita) = k_leaf
                Next
            Next
            Dim k_node As AdvancedTreeControl.TreeItem = Nothing
            If k_selectId.HasValue AndAlso _nodes.TryGetValue(k_selectId.Value, k_node) Then tree.SelectAndReveal(k_node)
            tree.Invalidate()
        Finally
            _treeLoading = False
        End Try
    End Sub

    ' yyyymm of the invoice date; 0 = no date.
    Private Shared Function MonthKey(k_p As EFacturaPrimita) As Integer
        Return If(k_p.DataFact.HasValue, k_p.DataFact.Value.Year * 100 + k_p.DataFact.Value.Month, 0)
    End Function

    Private Shared Function MonthLabel(k_key As Integer) As String
        If k_key = 0 Then Return "Fără dată"
        Return $"{_monthNames(k_key Mod 100 - 1)} {k_key \ 100}"
    End Function

    ' The dot: orange = never opened; grey ring = not linked to this DDF (the «all» list); green = opened.
    Private Function DotOf(k_p As EFacturaPrimita) As String
        If k_p.Nou Then Return EFacturaStare.Incarcata
        If _idDdf.HasValue AndAlso Not _linked.ContainsKey(k_p.IdPrimita) Then Return EFacturaStare.Ciorna
        Return EFacturaStare.Acceptata
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
        If k_p.Nou Then k_lines.Add("Nouă: nu a fost deschisă încă.")
        Return String.Join(vbLf, k_lines)
    End Function

    Private Sub ClearDetail()
        _shownId = 0
        _detail = Nothing
        gridLinii().ClearRows()
        gridCote().ClearRows()
        gridAtasamente().ClearRows()
        gridMesaje().ClearRows()
        navDetaliu.SetItemVisible(ViewAtasamente, False)
        navDetaliu.SetItemVisible(ViewMesaje, False)
        pgPdf.ReleaseDocument()
    End Sub

    Private Function gridLinii() As KBotDataView
        Return pgLinii.gridLinii
    End Function

    Private Function gridCote() As KBotDataView
        Return pgLinii.gridCote
    End Function

    Private Function gridAtasamente() As KBotDataView
        Return pgAtasamente.gridAtasamente
    End Function

    Private Function gridMesaje() As KBotDataView
        Return pgMesaje.gridMesaje
    End Function

    ' ── Choosing an invoice ─────────────────────────────────────────────────────

    Private Async Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If _treeLoading OrElse k_node Is Nothing Then Return
            Dim k_p As EFacturaPrimita = TryCast(k_node.Tag, EFacturaPrimita)
            If k_p Is Nothing Then Return
            If e IsNot Nothing AndAlso e.Button = MouseButtons.Right Then
                ShowMenu(k_p, tree.PointToScreen(New Point(e.X, e.Y)))
                Return
            End If
            If e IsNot Nothing AndAlso e.Button <> MouseButtons.Left Then Return
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
            _shownId = k_id
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            Dim k_detail As EFacturaPrimitaDetaliu = Await _gate.RunAsync(
                Function() _api.GetPrimitaAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _seq OrElse k_id <> _shownId Then Return
            _detail = k_detail
            FillLines(k_detail)
            FillAttachments(k_detail)
            FillMessages(k_detail)
            navDetaliu.SetItemVisible(ViewAtasamente, k_detail.Atasamente.Count > 0)
            navDetaliu.SetItemVisible(ViewMesaje, k_detail.Note.Count > 0 OrElse k_detail.Mesaje.Count > 0)
            Dim k_key As String = navDetaliu.SelectedKey
            If (k_key = ViewAtasamente AndAlso k_detail.Atasamente.Count = 0) OrElse
               (k_key = ViewMesaje AndAlso k_detail.Note.Count = 0 AndAlso k_detail.Mesaje.Count = 0) Then
                navDetaliu.SelectedKey = ViewLinii
                ShowPage(ViewLinii)
            ElseIf k_key = ViewPdf Then
                RefreshPdf()
            End If
            If k_detail.Factura.Nou Then Await MarkReadAsync(k_id).ConfigureAwait(True)
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

    ' The message is no longer new: stored on the server, the dot turns green.
    Private Async Function MarkReadAsync(k_id As Integer) As Task
        Try
            Await _gate.RunAsync(Function() _api.MarkPrimitaCititaAsync(k_id, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            For Each k_p As EFacturaPrimita In _items
                If k_p.IdPrimita = k_id Then k_p.Nou = False
            Next
            BuildTree(k_id)
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As Exception
            ' Only the dot is lost: the invoice itself was shown.
            GlobalErrorLog.Write("PrimiteView.MarkReadAsync", ex)
        End Try
    End Function

    Private Sub FillLines(k_detail As EFacturaPrimitaDetaliu)
        Dim k_lines As KBotDataView = gridLinii()
        k_lines.BeginUpdate()
        Try
            k_lines.ClearRows()
            For Each k_l As EFacturaPrimitaLinie In k_detail.Linii
                Dim k_row As KBotDataRow = k_lines.AddRow()
                k_row("nr") = k_l.NrLinie
                k_row("denumire") = k_l.Denumire
                k_row("explicatie") = k_l.Explicatie
                k_row("um") = k_l.Unit
                k_row("cant") = k_l.Cant
                k_row("pret") = k_l.Pret
                k_row("valoare") = k_l.Valoare
            Next
        Finally
            k_lines.EndUpdate()
        End Try
        Dim k_rates As KBotDataView = gridCote()
        k_rates.BeginUpdate()
        Try
            k_rates.ClearRows()
            For Each k_c As EFacturaPrimitaCota In k_detail.Cote
                Dim k_row As KBotDataRow = k_rates.AddRow()
                k_row("categorie") = k_c.Categorie
                k_row("cota") = k_c.CotaTva
                k_row("baza") = k_c.Baza
                k_row("tva") = k_c.Tva
            Next
        Finally
            k_rates.EndUpdate()
        End Try
    End Sub

    Private Sub FillAttachments(k_detail As EFacturaPrimitaDetaliu)
        Dim k_grid As KBotDataView = gridAtasamente()
        k_grid.BeginUpdate()
        Try
            k_grid.ClearRows()
            For Each k_a As EFacturaPrimitaAtasament In k_detail.Atasamente
                Dim k_row As KBotDataRow = k_grid.AddRow()
                k_row.Tag = k_a
                k_row("nume") = k_a.Nume
                k_row("mime") = k_a.Mime
                k_row("octeti") = k_a.Octeti
            Next
        Finally
            k_grid.EndUpdate()
        End Try
    End Sub

    Private Sub FillMessages(k_detail As EFacturaPrimitaDetaliu)
        Dim k_grid As KBotDataView = gridMesaje()
        k_grid.BeginUpdate()
        Try
            k_grid.ClearRows()
            For Each k_n As String In k_detail.Note
                Dim k_row As KBotDataRow = k_grid.AddRow()
                k_row("fel") = "Notă din factură"
                k_row("data") = String.Empty
                k_row("text") = k_n
            Next
            For Each k_m As EFacturaPrimitaMesaj In k_detail.Mesaje
                Dim k_row As KBotDataRow = k_grid.AddRow()
                k_row("fel") = "Mesaj"
                k_row("data") = If(k_m.DataMesaj.HasValue, k_m.DataMesaj.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_row("text") = k_m.Mesaj
            Next
        Finally
            k_grid.EndUpdate()
        End Try
    End Sub

    ' ── The views ───────────────────────────────────────────────────────────────

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
        pgAtasamente.Visible = String.Equals(k_key, ViewAtasamente, StringComparison.Ordinal)
        pgMesaje.Visible = String.Equals(k_key, ViewMesaje, StringComparison.Ordinal)
        If pgPdf.Visible Then RefreshPdf()
    End Sub

    ''' <summary>
    ''' Opens the PDF of the shown invoice: the classic PDF the supplier embedded in the XML when there is one, else the one ANAF's service
    ''' draws from the XML. A failure becomes a line inside the viewer. UI boundary (async Sub): logs and swallows.
    ''' </summary>
    Private Async Sub RefreshPdf()
        Try
            Dim k_d As EFacturaPrimitaDetaliu = _detail
            If k_d Is Nothing OrElse _shownId = 0 Then
                pgPdf.ShowNotice("Alegeți o factură din arbore.")
                Return
            End If
            Dim k_embedded As EFacturaPrimitaAtasament = k_d.Atasamente.FirstOrDefault(
                Function(k_a) k_a.Mime.Contains("pdf", StringComparison.OrdinalIgnoreCase) OrElse k_a.Nume.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            Dim k_stamp As String = $"{k_d.Factura.IdPrimita}|{If(k_embedded Is Nothing, "anaf", "atas" & k_embedded.Index.ToString(CultureInfo.InvariantCulture))}"
            If String.Equals(k_stamp, pgPdf.Stamp, StringComparison.Ordinal) Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_pdfSeq)
            Dim k_id As Integer = k_d.Factura.IdPrimita
            pgPdf.ShowNotice(If(k_embedded Is Nothing, "Se aduce factura de la ANAF…", "Se deschide factura atașată…"))
            Dim k_bytes As Byte() = Await _gate.RunAsync(
                Function() If(k_embedded Is Nothing, _api.GetPrimitaPdfAsync(k_id, _cts.Token),
                              _api.GetPrimitaAtasamentAsync(k_id, k_embedded.Index, _cts.Token))).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _pdfSeq Then Return
            TempPdfStore.EnsureRoot()
            Dim k_label As String = Regex.Replace(k_d.Factura.NrFact, "[^A-Za-z0-9_.-]", "_")
            Dim k_path As String = TempPdfStore.PathFor($"primita_{k_label}_{DateTime.Now:HHmmssfff}.pdf")
            File.WriteAllBytes(k_path, k_bytes)
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

    ''' <summary>Double click on an attached file saves it. UI boundary (async Sub): logs and swallows.</summary>
    Private Async Sub GridAtasamente_CellDoubleClick(sender As Object, e As KBotCellEventArgs)
        Try
            Dim k_d As EFacturaPrimitaDetaliu = _detail
            If k_d Is Nothing OrElse e Is Nothing OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridAtasamente().RowCount Then Return
            Dim k_a As EFacturaPrimitaAtasament = TryCast(gridAtasamente().Rows(e.RowIndex).Tag, EFacturaPrimitaAtasament)
            If k_a Is Nothing Then Return
            Dim k_id As Integer = k_d.Factura.IdPrimita
            Dim k_ext As String = Path.GetExtension(k_a.Nume).TrimStart("."c)
            Using k_dialog As New SaveFileDialog() With {
                .FileName = Regex.Replace(k_a.Nume, "[^A-Za-z0-9_.-]", "_"), .OverwritePrompt = True}
                If k_dialog.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                Dim k_path As String = k_dialog.FileName
                Dim k_bytes As Byte() = Await _gate.RunAsync(
                    Function() _api.GetPrimitaAtasamentAsync(k_id, k_a.Index, _cts.Token)).ConfigureAwait(True)
                File.WriteAllBytes(k_path, k_bytes)
            End Using
        Catch ex As OperationCanceledException
            ' The host was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("PrimiteView.GridAtasamente_CellDoubleClick", ex)
            ShowProblem(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.GridAtasamente_CellDoubleClick", ex)
            ShowProblem("Fișierul nu a putut fi salvat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' ── Theme ───────────────────────────────────────────────────────────────────

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            chkToate.ForeColor = k_scheme.Palette.TextColor
            chkToate.BackColor = k_scheme.Palette.SurfaceAltColor
            If _items.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteView.ApplyTheme", ex)
        End Try
    End Sub

End Class
