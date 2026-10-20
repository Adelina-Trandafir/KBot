Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
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
''' Slices KE04..KE07, 00EF-39, KE11, KE12 -- the PAGE where the accountant says how received invoices are covered by ONE reception. Since KE12 it is not a modal window any more:
''' it opens from the ribbon (E-Factura, «Recepții»), stays open like «Cumpărare», lists the invoices of the year itself and does the work itself. One page, three modes
''' (<see cref="EFacturaKeTip"/>): link to an EXISTING reception that already has the value, make a NEW reception, or MODIFY an existing reception that is
''' too small (its values grow by the amounts). KE11 (the operator's mock-up): on the left the invoices of the list, by supplier, with a tick each (the ones a
''' reception already covers are not listed); on the right the lines of the ticked invoices, grouped by invoice, each with the amount the reception takes
''' («Recepționată», the value of the line unless the accountant lowers it) and its classification; below, the angajamente the invoices go to (the chosen one is
''' the current row, an empty row at the end adds another), the data of the reception and the amounts by classification.
''' The mode is chosen in the combo above the data of the reception (new / existing / modified). On the button: the choices of the lines are saved, the server checks
''' every invoice, the robot (new / modified) writes ONE reception in FOREXE, then every invoice gets its link; the page then reads the list again.
''' </summary>
Public Class KeLegaForm

    Private Shared ReadOnly _ro As CultureInfo = DdfSendInputs.CulturaSite
    Private Const ColFactura As String = "factura"
    Private Const ColLDenumire As String = "denumire"
    Private Const ColLCuTva As String = "cuTva"
    Private Const ColLRec As String = "rec"
    Private Const ColLClsf As String = "clsf"
    Private Const ColAClsf As String = "clsf"
    Private Const ColABuget As String = "buget"
    Private Const ColADisponibil As String = "disponibil"
    Private Const ColAReceptii As String = "receptii"
    Private Const ColARamas As String = "ramas"
    Private Const ColAValRecep As String = "valRecep"
    Private Const ColARestRecep As String = "restRecep"
    Private Const ColRData As String = "data"
    Private Const ColRTip As String = "tip"
    Private Const ColRDescriere As String = "descriere"
    Private Const ColRTotal As String = "total"
    Private Const ColRRest As String = "rest"
    Private Const ColAng As String = "ang"
    Private Const ColOpt As String = "opt"
    Private Const MenuRemove As String = "elimina"
    Private Const MenuOnlyAssociated As String = "doar-asociate"
    Private Const NeAleasa As String = "(nealeasă)"
    Private Const Tolerance As Decimal = 0.005D

    ' What a row of the lines grid stands for.
    Private NotInheritable Class LineTag
        Public Property Invoice As EFacturaPrimita
        Public Property Linie As EFacturaKeLinie

        ' The accountant typed the amount of this line: it is no longer proposed.
        Public Property Edited As Boolean
    End Class

    ' A row of the angajamente grid.
    Private NotInheritable Class AngItem
        Public Property Cod As String = String.Empty
        Public Property Descriere As String = String.Empty

        ' «ddf», «legatura» or «furnizor» (as the server says), or «fix».
        Public Property Sursa As String = String.Empty

        ' Slice KE14: a reception is linked to an invoice on this angajament: it can no longer be taken off the list.
        Public Property Linked As Boolean

        Public ReadOnly Property Eticheta As String
            Get
                Return If(String.IsNullOrWhiteSpace(Descriere), Cod, $"{Cod} — {Descriere}")
            End Get
        End Property
    End Class

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _mode As String = EFacturaKeTip.Noua
    Private ReadOnly _year As Integer
    Private Const TabNoua As String = "noua"
    Private Const TabExistente As String = "existente"
    Private ReadOnly _actionKeys As String() = {EFacturaKeTip.Existenta, EFacturaKeTip.Modificata}
    Private ReadOnly _actionLabels As String() = {"Leagă facturile de recepție", "Modifică recepția (valorile cresc)"}
    Private _loadingMode As Boolean
    Private _busy As Boolean
    Private ReadOnly _tipuri As String() = {"Avans", "Partial", "Final"}
    Private ReadOnly _byLabel As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _labelOf As New Dictionary(Of Integer, String)()
    Private ReadOnly _states As New Dictionary(Of Integer, EFacturaKeStare)()
    Private ReadOnly _invoices As New Dictionary(Of Integer, EFacturaPrimita)()
    Private ReadOnly _nodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
    Private ReadOnly _order As New List(Of Integer)()
    ' An angajament picked in the table belongs to the SUPPLIER, not to the invoice (operator, 19.10.2026): since slice 0121-05 the pick is saved on the server (the supplier's link by tax code) and comes back as a candidate with the source «furnizor»; nothing is kept in memory.
    Private _browsedKey As String = String.Empty
    Private _browsedId As Integer
    Private ReadOnly _angByLabel As New Dictionary(Of String, EFacturaKeAngajamentAlegere)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _angCache As New Dictionary(Of String, EFacturaKeReceptiiAngajament)(StringComparer.OrdinalIgnoreCase)
    Private _allAng As New List(Of EFacturaKeAngajamentAlegere)()
    Private _checked As New List(Of Integer)()
    Private _items As New List(Of AngItem)()
    Private _data As EFacturaKeReceptiiAngajament
    Private _angCod As String = String.Empty
    Private _syncSeq As Integer
    Private _othersSeq As Integer
    ' Slice KE12-04: the embedded variant (inside a view of the invoices): ONE invoice and, in the view of an angajament, ONE angajament are given; their choosers are not shown.
    Private _embedded As Boolean
    Private _fixedAng As String = String.Empty
    Private _pendingNotice As String = String.Empty
    Private _allAngLoaded As Boolean
    Private _browseSeq As Integer
    ' The state of the supplier pressed in the tree while no invoice is ticked: its angajamente are shown without a tick.
    Private _browsed As EFacturaKeStare
    Private _angSeq As Integer
    Private _filling As Boolean
    Private _angFilling As Boolean
    Private _fitShown As Boolean
    Private _dataAuto As Date = Date.MinValue
    ' Slice KE12-03: the budget of each indicator on the day of the reception and the receptions before that day (by classification), for the angajament and day in _dispoKey.
    Private ReadOnly _dispo As New Dictionary(Of Integer, EFacturaKeDisponibil)()
    Private ReadOnly _dispoCache As New Dictionary(Of String, EFacturaKeDisponibilZi)(StringComparer.OrdinalIgnoreCase)
    Private _dispoKey As String = String.Empty
    Private _dispoWarning As String = String.Empty
    Private _dispoSeq As Integer
    Private _descrAuto As String = String.Empty
    ' Slice KE13: the invoices as last read, and whether the tree shows only the ones a reception is linked to (the option of the header icon).
    Private _lastList As New List(Of EFacturaPrimita)()
    Private _onlyAssociated As Boolean
    ' Slice KE14: the invoice on screen is one a reception already covers (pressed in the tree): its products, the reception it is linked to, nothing can be changed, «Elimină asociere».
    Private _viewing As EFacturaPrimita
    Private _viewState As EFacturaKeStare
    Private _viewSeq As Integer
    Private _recTotalHeader As String = String.Empty
    Private ReadOnly _viewDispo As New Dictionary(Of String, EFacturaKeDisponibilZi)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Embedded variant only: a reception was made or linked; the host reads the invoice again and gives it back through <see cref="ShowInvoice"/>.</summary>
    Public Event Committed As EventHandler

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_unitName">The open unit's name, shown in the title.</param>
    ''' <param name="k_year">The year chosen in K-BOT; only the invoices dated in it are listed (0 = all years).</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_unitName As String, k_year As Integer)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _year = k_year
        capBar.Text = "Recepții facturi primite" & If(k_year > 0, $" {k_year}", String.Empty) &
                      If(String.IsNullOrWhiteSpace(k_unitName), String.Empty, " — " & k_unitName)
        cmbTip.Items.AddRange(_tipuri.Cast(Of Object)())
        cmbTip.SelectedIndex = Array.IndexOf(_tipuri, "Final")
        _loadingMode = True
        cmbMod.Items.AddRange(_actionLabels.Cast(Of Object)())
        cmbMod.SelectedIndex = 0
        navMod.SelectedKey = TabNoua
        _loadingMode = False
        ApplyMode()
        FillAngajamente()
    End Sub

    ' ── Tree of invoices ────────────────────────────────────────────────────────

    ' An invoice that has nothing left to receive: its value is 0, or the receptions it is linked to already cover its whole total (the links of all of them added up).
    Private Shared Function IsCovered(k_p As EFacturaPrimita) As Boolean
        If Math.Abs(k_p.Total) < 0.01D Then Return True   ' nothing to receive
        If k_p.Receptii.Count = 0 Then Return False
        Dim k_sum As Decimal = k_p.Receptii.Sum(Function(k_r) k_r.Suma)
        Return Math.Abs(k_sum) >= Math.Abs(k_p.Total) - 0.01D
    End Function

    Private Shared Function SupplierKey(k_p As EFacturaPrimita) As String
        If Not String.IsNullOrWhiteSpace(k_p.CuiNormalizat) Then Return k_p.CuiNormalizat.Trim()
        Return "N" & k_p.Furnizor.Trim().ToUpperInvariant()
    End Function

    Private Shared Function SupplierName(k_group As IEnumerable(Of EFacturaPrimita)) As String
        Dim k_newest As EFacturaPrimita = k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita).First()
        If Not String.IsNullOrWhiteSpace(k_newest.Furnizor) Then Return k_newest.Furnizor.Trim()
        If Not String.IsNullOrWhiteSpace(k_newest.Cui) Then Return k_newest.Cui.Trim()
        Return "Furnizor necunoscut"
    End Function

    Private Shared Function DateText(k_p As EFacturaPrimita) As String
        Return If(k_p.DataFact.HasValue, k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
    End Function

    ' Slice KE13: what the tree lists. EVERY invoice with a value (the ones a reception covers too: they are ticked and locked); the option of the header icon narrows it to the
    ' invoices a reception is linked to.
    Private Function ShowsInTree(k_p As EFacturaPrimita) As Boolean
        If Math.Abs(k_p.Total) < 0.01D Then Return False   ' nothing to receive
        Return Not _onlyAssociated OrElse k_p.Receptii.Count > 0
    End Function

    ' The suppliers (by name), under each its invoices (newest first, by date), a tick on every row. k_keepAll: the host gave the invoices and wants every one of them listed.
    Private Sub BuildTree(k_candidates As IEnumerable(Of EFacturaPrimita), Optional k_keepAll As Boolean = False)
        tree.Clear()
        _nodes.Clear()
        _order.Clear()
        _invoices.Clear()
        _lastList = k_candidates.ToList()
        Dim k_shown As List(Of EFacturaPrimita) = _lastList.Where(Function(k_x) k_keepAll OrElse ShowsInTree(k_x)).ToList()
        For Each k_p As EFacturaPrimita In k_shown
            _invoices(k_p.IdPrimita) = k_p
        Next
        Dim k_groups As IEnumerable(Of IGrouping(Of String, EFacturaPrimita)) =
            k_shown.GroupBy(Function(k_x) SupplierKey(k_x)).OrderBy(Function(k_g) SupplierName(k_g), StringComparer.CurrentCultureIgnoreCase)
        For Each k_group As IGrouping(Of String, EFacturaPrimita) In k_groups
            Dim k_sum As Decimal = k_group.Sum(Function(k_x) k_x.Total)
            Dim k_sup As AdvancedTreeControl.TreeItem = tree.AddItem(
                "S_" & k_group.Key, $"{SupplierName(k_group)}~~~{k_sum.ToString("N2", _ro)}")
            ' The supplier is only a heading (closed at the start): the ticks are on the invoices (operator, 18.10.2026).
            k_sup.Bold = True
            k_sup.Tag = k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita).First().IdPrimita
            For Each k_p As EFacturaPrimita In k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita)
                Dim k_caption As String = $"{DateText(k_p)} · {If(k_p.Tip = "NC", "NC ", String.Empty)}{k_p.NrFact}~~~{k_p.Total.ToString("N2", _ro)}"
                Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem("P_" & k_p.IdPrimita.ToString(CultureInfo.InvariantCulture), k_caption, k_sup)
                k_leaf.Tag = k_p
                k_leaf.HasCheckBox = True
                StyleLeaf(k_leaf, k_p)
                _nodes(k_p.IdPrimita) = k_leaf
                _order.Add(k_p.IdPrimita)
            Next
            StyleSupplier(k_sup)
        Next
        ShowTreeTotal()
        tree.Invalidate()
    End Sub

    ' The suppliers the angajamente table is about now: those of the ticked invoices, else the one pressed in the tree ("" when none).
    Private Function ContextKeys() As List(Of String)
        Dim k_keys As New List(Of String)()
        For Each k_id As Integer In _checked
            Dim k_inv As EFacturaPrimita = Nothing
            If _invoices.TryGetValue(k_id, k_inv) AndAlso Not k_keys.Contains(SupplierKey(k_inv)) Then k_keys.Add(SupplierKey(k_inv))
        Next
        If k_keys.Count = 0 Then k_keys.Add(If(_browsed IsNot Nothing, _browsedKey, String.Empty))
        Return k_keys
    End Function

    ' The tax code of the supplier a key stands for; empty when its invoices carry none (the key is then built from the name).
    Private Function SupplierCui(k_key As String) As String
        For Each k_inv As EFacturaPrimita In _invoices.Values
            If Not String.IsNullOrWhiteSpace(k_inv.CuiNormalizat) AndAlso String.Equals(SupplierKey(k_inv), k_key, StringComparison.Ordinal) Then
                Return k_inv.CuiNormalizat.Trim()
            End If
        Next
        Return String.Empty
    End Function

    ' The state of the invoices the angajamente table is about (ticked, else the one pressed) is read again, after the supplier's links changed.
    Private Async Function RefreshContextStatesAsync() As Task
        ' The states kept for every invoice of those suppliers are stale now (their candidates); the ones on screen are read again.
        Dim k_keys As List(Of String) = ContextKeys()
        For Each k_inv As EFacturaPrimita In _invoices.Values
            If k_keys.Contains(SupplierKey(k_inv)) Then _states.Remove(k_inv.IdPrimita)
        Next
        For Each k_id As Integer In _checked.ToList()
            Dim k_idNow As Integer = k_id
            _states(k_idNow) = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
        Next
        If _checked.Count = 0 AndAlso _browsed IsNot Nothing AndAlso _browsedId <> 0 Then
            Dim k_idBrowsed As Integer = _browsedId
            _browsed = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idBrowsed, _cts.Token)).ConfigureAwait(True)
            _states(k_idBrowsed) = _browsed
        End If
    End Function

    ' Slice KE16: the invoices of a supplier that have no reception linked at all.
    Private Shared Function FreeInvoices(k_sup As AdvancedTreeControl.TreeItem) As List(Of EFacturaPrimita)
        Return k_sup.Children.Select(Function(k_c) TryCast(k_c.Tag, EFacturaPrimita)).
            Where(Function(k_p) k_p IsNot Nothing AndAlso k_p.Receptii.Count = 0 AndAlso Math.Abs(k_p.Total) >= 0.01D).ToList()
    End Function

    ' Slice KE16: a supplier that has invoices without a reception gets, while the mouse is over its row, the button that opens the window of proposals (bulk link, 1 invoice = 1 reception).
    Private Sub StyleSupplier(k_sup As AdvancedTreeControl.TreeItem)
        Dim k_free As Integer = If(_embedded, 0, FreeInvoices(k_sup).Count)
        If k_free > 0 Then
            k_sup.RightIcon = FacturaIcons.Link(ThemeManager.Current.Palette, tree.RightIconSize.Width)
            k_sup.ShowRightIconOnHover = True
            k_sup.Tooltip = $"{k_free} {If(k_free = 1, "factură fără recepție", "facturi fără recepție")}. Butonul din dreapta rândului propune legarea lor de recepții."
        Else
            k_sup.RightIcon = Nothing
            k_sup.ShowRightIconOnHover = False
            k_sup.Tooltip = String.Empty
        End If
    End Sub

    ' UI boundary (async Sub): the button of a supplier. The window only PROPOSES (nothing is saved until the accountant presses «Leagă» in it); when it wrote links the invoices are read again.
    Private Async Sub Tree_RightIconClicked(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If _api Is Nothing OrElse k_node Is Nothing OrElse _busy Then Return
            Dim k_free As List(Of EFacturaPrimita) = FreeInvoices(k_node)
            If k_free.Count = 0 Then Return
            Dim k_written As Integer
            ' The supplier's angajamente are saved on the server (slice 0121-05): the window needs none sent.
            Using k_dialog As New KePropuneriForm(_api, _gate, SupplierName(k_free), k_free.Select(Function(k_p) k_p.IdPrimita), New List(Of String)())
                If k_dialog.ShowDialog(Me) <> DialogResult.OK Then Return
                k_written = k_dialog.Legate
            End Using
            If IsDisposed Then Return
            Await ReloadAsync(True).ConfigureAwait(True)
            If IsDisposed Then Return
            ShowNotice($"{k_written} {If(k_written = 1, "factură legată", "facturi legate")} de recepții.", NoticeKind.Success)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.Tree_RightIconClicked", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.Tree_RightIconClicked", ex)
            ShowNotice("Propunerile nu au putut fi făcute. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' Slice KE13: an invoice a reception already covers is ticked and locked (it is not received again; it can only be un-associated, from the right-click menu), written in the dim
    ' text colour; the tooltip says by which receptions. Any other invoice is a plain row with an empty tick. The embedded variant works on ONE invoice, ticked by the host: no lock there.
    Private Sub StyleLeaf(k_leaf As AdvancedTreeControl.TreeItem, k_p As EFacturaPrimita)
        Dim k_locked As Boolean = Not _embedded AndAlso IsCovered(k_p) AndAlso Math.Abs(k_p.Total) >= 0.01D
        k_leaf.CheckLocked = k_locked
        k_leaf.NodeForeColor = If(k_locked, ThemeManager.Current.Palette.TextDimColor, System.Drawing.Color.Empty)
        k_leaf.Tooltip = AssociationTip(k_p)
        tree.SetItemCheckState(k_leaf, If(k_locked, AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked))
    End Sub

    Private Shared Function AssociationTip(k_p As EFacturaPrimita) As String
        If k_p.Receptii.Count = 0 Then Return String.Empty
        Dim k_sum As Decimal = k_p.Receptii.Sum(Function(k_r) k_r.Suma)
        Dim k_head As String = If(IsCovered(k_p), "Recepționată integral.", $"Recepționată parțial: {k_sum.ToString("N2", _ro)} din {k_p.Total.ToString("N2", _ro)}.")
        Dim k_lines As IEnumerable(Of String) = k_p.Receptii.Select(
            Function(k_r) $"{If(k_r.DataR.HasValue, k_r.DataR.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "recepție încă nedescărcată")} · {k_r.Suma.ToString("N2", _ro)}")
        Return k_head & vbLf & String.Join(vbLf, k_lines) & vbLf & "Un clic pe factură arată recepția și produsele ei."
    End Function

    ' The footer of the tree: what the listed invoices add up to (it also hides the half row the viewport cuts at the bottom).
    Private Sub ShowTreeTotal()
        tree.FooterCaption = "TOTAL  " & _invoices.Values.Sum(Function(k_x) k_x.Total).ToString("N2", _ro) & "  "
        tree.HeaderCaption = If(_onlyAssociated, " FACTURI ASOCIATE CU RECEPȚII", " FACTURI PRIMITE")
    End Sub

    Private Function CheckedIds() As List(Of Integer)
        Return _order.Where(Function(k_id) _nodes(k_id).CheckState = AdvancedTreeControl.TreeCheckState.Checked AndAlso Not _nodes(k_id).CheckLocked).ToList()
    End Function

    ' UI boundary (async Sub): a row of the tree was pressed. The angajamente belong to the SUPPLIER, not to the invoice: pressing a supplier (or an invoice) shows
    ' the angajamente of that supplier right away, without a tick (KE12-05). Once an invoice is ticked, the ticks decide.
    Private Async Sub Tree_NodeMouseDown(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseDown
        Try
            If _api Is Nothing OrElse k_node Is Nothing Then Return
            ' Slice KE14: a press on an invoice a reception covers shows it (read-only); a press on any other row brings the page back.
            Dim k_pressed As EFacturaPrimita = TryCast(k_node.Tag, EFacturaPrimita)
            If k_pressed IsNot Nothing AndAlso k_node.CheckLocked Then
                BeginView(k_pressed)
                Return
            End If
            If _viewing IsNot Nothing Then LeaveViewing()
            If _checked.Count > 0 Then Return
            Dim k_idPrimita As Integer
            Dim k_invoice As EFacturaPrimita = TryCast(k_node.Tag, EFacturaPrimita)
            If k_invoice IsNot Nothing Then
                k_idPrimita = k_invoice.IdPrimita
            ElseIf TypeOf k_node.Tag Is Integer Then
                k_idPrimita = CInt(k_node.Tag)
            Else
                Return
            End If
            Dim k_mySeq As Integer = Interlocked.Increment(_browseSeq)
            Dim k_state As EFacturaKeStare = Nothing
            If Not _states.TryGetValue(k_idPrimita, k_state) Then
                k_state = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idPrimita, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _browseSeq Then Return
                _states(k_idPrimita) = k_state
            End If
            If IsDisposed OrElse k_mySeq <> _browseSeq OrElse _checked.Count > 0 Then Return
            _browsed = k_state
            Dim k_browsedInvoice As EFacturaPrimita = Nothing
            _browsedId = k_idPrimita
            _browsedKey = If(_invoices.TryGetValue(k_idPrimita, k_browsedInvoice), SupplierKey(k_browsedInvoice), String.Empty)
            FillAngajamente()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.Tree_NodeMouseDown", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.Tree_NodeMouseDown", ex)
            ShowNotice("Angajamentele furnizorului nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' UI boundary (async Sub): a tick changed.
    Private Async Sub Tree_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles tree.NodeChecked
        Try
            If _viewing IsNot Nothing Then LeaveViewing()
            Await SyncCheckedAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.Tree_NodeChecked", ex)
            ShowNotice("Facturile bifate nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' Slice KE13: the options icon of the header -- «Arată facturile asociate» shows ONLY the invoices a reception is linked to (a tick in the menu while it is on).
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            ' A second press on the icon CLOSES the menu (the press already closed it, so without this guard it would reopen at once).
            If CustomPopup.ClosedJustNow Then Return
            Dim k_anchor As System.Drawing.Rectangle = tree.HeaderRightIconRect
            If k_anchor.IsEmpty Then Return
            Dim k_rows As New List(Of CustomPopupItem)()
            k_rows.Add(New CustomPopupItem(MenuOnlyAssociated, "Arată facturile &asociate", My.Resources.Resources.invoice) With {.Checked = _onlyAssociated})
            Dim k_menu As New CustomPopup(k_rows)
            AddHandler k_menu.ItemClicked, AddressOf OptionsMenu_ItemClicked
            k_menu.ShowBelow(tree, k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ' UI boundary: the option changed -> the tree is built again from the invoices already read; nothing is ticked afterwards.
    Private Sub OptionsMenu_ItemClicked(k_sender As Object, k_args As CustomPopupItemEventArgs)
        Try
            If Not String.Equals(k_args.Item.Key, MenuOnlyAssociated, StringComparison.Ordinal) Then Return
            _onlyAssociated = Not _onlyAssociated
            Interlocked.Increment(_syncSeq)
            _browsed = Nothing
            _checked = New List(Of Integer)()
            RebuildTree(_lastList)
            ApplyChecked()
            If _nodes.Count = 0 Then
                ShowNotice(If(_onlyAssociated, "Nicio factură primită nu este asociată cu o recepție în această perioadă.", "Nu există facturi primite în această perioadă."), NoticeKind.Warning)
            Else
                ntfMesaj.Visible = False
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.OptionsMenu_ItemClicked", ex)
            ShowNotice("Opțiunea nu a putut fi aplicată. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── The view of an invoice a reception covers (slice KE14) ──────────────────────

    ' UI boundary (async Sub): a press on an invoice a reception covers. Refused while other invoices are ticked (what was typed on their lines would be lost).
    Private Async Sub BeginView(k_invoice As EFacturaPrimita)
        Try
            If _api Is Nothing OrElse _busy Then Return
            If _viewing IsNot Nothing AndAlso _viewing.IdPrimita = k_invoice.IdPrimita Then Return
            If _checked.Count > 0 Then
                ShowNotice("Sunt facturi bifate. Debifați-le ca să vedeți recepția unei facturi deja rezolvate.", NoticeKind.Warning)
                Return
            End If
            Interlocked.Increment(_browseSeq)
            Interlocked.Increment(_syncSeq)
            Dim k_mySeq As Integer = Interlocked.Increment(_viewSeq)
            Dim k_id As Integer = k_invoice.IdPrimita
            Dim k_state As EFacturaKeStare = Nothing
            If Not _states.TryGetValue(k_id, k_state) Then
                k_state = Await _gate.RunAsync(Function() _api.GetKeAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _viewSeq Then Return
                _states(k_id) = k_state
            End If
            If IsDisposed OrElse k_mySeq <> _viewSeq OrElse _checked.Count > 0 Then Return
            EnterViewing(k_invoice, k_state)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginView", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginView", ex)
            ShowNotice("Recepția facturii nu a putut fi citită. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' The right side shows only what the invoice already has: its products (nothing can be changed), the angajament and the reception it is linked to, the amounts by
    ' classification; no «Recepție nouă», no choice of what to do. Under the receptions: the button «Elimină asociere» (the footer bar is hidden meanwhile).
    Private Sub EnterViewing(k_invoice As EFacturaPrimita, k_state As EFacturaKeStare)
        Interlocked.Increment(_angSeq)
        Interlocked.Increment(_othersSeq)
        _viewing = k_invoice
        _viewState = k_state
        _browsed = Nothing
        _data = Nothing
        _angCod = String.Empty
        ntfMesaj.Visible = False
        If _recTotalHeader.Length = 0 Then _recTotalHeader = gridReceptii.Column(ColRTotal).HeaderText
        gridReceptii.Column(ColRTotal).HeaderText = "Valoare factură"
        gridReceptii.Column(ColRRest).Visible = KBotColumnVisibility.Hidden
        gridLinii.ReadOnlyGrid = True
        gridAng.ReadOnlyGrid = True
        navMod.Visible = False
        pnlMod.Visible = False
        pnlNoua.Visible = False
        gridReceptii.Visible = True
        gridAloc.Column(ColARestRecep).Visible = KBotColumnVisibility.Hidden
        btnElimina.Visible = True
        pnlJos.Visible = False
        FillViewedLines(k_invoice, k_state)
        FillAngajamente()
        FillReceptii()
        _viewDispo.Clear()
        FillViewedAlocari(k_state)
        BeginViewedDispo(k_state, _viewSeq)
        UpdateTitles()
        RefreshOk()
        If Not k_state.PoateScrie Then ShowNotice("Numai un utilizator cu rolul «Contabil» poate elimina asocierea facturilor cu recepțiile.", NoticeKind.Warning)
    End Sub

    ' Back to the page as it works: the tabs, the form of a new reception, the lines of the ticked invoices (none, or what the ticks say).
    Private Sub LeaveViewing()
        If _viewing Is Nothing Then Return
        _viewing = Nothing
        _viewState = Nothing
        Interlocked.Increment(_viewSeq)
        gridLinii.ReadOnlyGrid = False
        gridAng.ReadOnlyGrid = False
        If _recTotalHeader.Length > 0 Then gridReceptii.Column(ColRTotal).HeaderText = _recTotalHeader
        gridReceptii.Column(ColRRest).Visible = KBotColumnVisibility.Visible
        btnElimina.Visible = False
        pnlJos.Visible = True
        navMod.Visible = True
        pnlMod.Visible = String.Equals(navMod.SelectedKey, TabExistente, StringComparison.Ordinal)
        ApplyMode()
        ApplyChecked()
        FillReceptii()
    End Sub

    Private Sub FillViewedLines(k_invoice As EFacturaPrimita, k_state As EFacturaKeStare)
        _filling = True
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            Dim k_group As String = $"{If(k_invoice.Tip = "NC", "NC ", String.Empty)}{k_invoice.NrFact} / {DateText(k_invoice)}"
            For Each k_line As EFacturaKeLinie In k_state.Linii
                Dim k_row As KBotDataRow = gridLinii.AddRow()
                k_row.Tag = New LineTag() With {.Invoice = k_invoice, .Linie = k_line}
                k_row(KBotDataView.CheckColumnKey) = True
                k_row(ColFactura) = k_group
                k_row(ColLDenumire) = k_line.Denumire
                k_row(ColLCuTva) = CDbl(k_line.ValoareCuTva)
                k_row(ColLRec) = CDbl(k_line.ValoareCuTva)
                k_row(ColLClsf) = InitialLabel(k_line)
            Next
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
    End Sub

    ' The angajamente of the links of the invoice (no empty row, no button).
    Private Sub FillViewedAngajamente()
        Dim k_items As New List(Of AngItem)()
        If _viewState IsNot Nothing Then
            For Each k_link As EFacturaKeLegatura In _viewState.Legaturi
                If k_items.Any(Function(k_a) String.Equals(k_a.Cod, k_link.CodAngajament, StringComparison.OrdinalIgnoreCase)) Then Continue For
                Dim k_known As EFacturaKeAngajament = _viewState.Angajamente.FirstOrDefault(
                    Function(k_a) String.Equals(k_a.CodAngajament, k_link.CodAngajament, StringComparison.OrdinalIgnoreCase))
                Dim k_description As String = If(k_known Is Nothing, String.Empty, k_known.Descriere)
                If String.IsNullOrWhiteSpace(k_description) Then
                    Dim k_choice As EFacturaKeAngajamentAlegere = _allAng.FirstOrDefault(
                        Function(k_a) String.Equals(k_a.CodAngajament, k_link.CodAngajament, StringComparison.OrdinalIgnoreCase))
                    If k_choice IsNot Nothing Then k_description = k_choice.Descriere
                End If
                k_items.Add(New AngItem() With {.Cod = k_link.CodAngajament, .Descriere = k_description, .Sursa = "legatura", .Linked = True})
            Next
        End If
        _items = k_items
        _angFilling = True
        gridAng.BeginUpdate()
        Try
            gridAng.ClearRows()
            For Each k_a As AngItem In k_items
                Dim k_row As KBotDataRow = gridAng.AddRow()
                k_row.Tag = k_a
                k_row(ColAng) = k_a.Eticheta
                k_row(ColOpt) = String.Empty
            Next
        Finally
            gridAng.EndUpdate()
            _angFilling = False
        End Try
    End Sub

    ' ONLY the receptions the invoice is linked to, one row each (what the invoice takes of it).
    Private Sub FillViewedReceptii()
        gridReceptii.BeginUpdate()
        Try
            gridReceptii.ClearRows()
            If _viewState Is Nothing Then Return
            Dim k_groups As IEnumerable(Of IGrouping(Of String, EFacturaKeLegatura)) = _viewState.Legaturi.GroupBy(
                Function(k_l) If(k_l.Idrr.HasValue, "R" & k_l.Idrr.Value.ToString(CultureInfo.InvariantCulture),
                                 "H" & k_l.Idrh.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) & "/" & k_l.Idr.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)))
            For Each k_group As IGrouping(Of String, EFacturaKeLegatura) In k_groups
                Dim k_first As EFacturaKeLegatura = k_group.First()
                Dim k_row As KBotDataRow = gridReceptii.AddRow()
                k_row(ColRData) = If(k_first.DataR.HasValue, k_first.DataR.Value.ToString("dd.MM.yyyy", _ro),
                                     If(k_first.AsteaptaDescarcarea, "în așteptare", String.Empty))
                k_row(ColRTip) = k_first.TipReceptie
                k_row(ColRDescriere) = k_first.DescriereR
                k_row(ColRTotal) = CDbl(k_group.Sum(Function(k_l) k_l.Suma))
            Next
            If gridReceptii.RowCount > 0 Then gridReceptii.CurrentRowIndex = 0
        Finally
            gridReceptii.EndUpdate()
        End Try
    End Sub

    ' The key of the budget answer of one angajament on the day of a reception.
    Private Shared Function ViewKey(k_link As EFacturaKeLegatura) As String
        Dim k_day As Date = If(k_link.DataR.HasValue, k_link.DataR.Value.Date, Date.Today)
        Return k_link.CodAngajament & "|" & k_day.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
    End Function

    ' The amounts of the links by classification and angajament; Buget / Receptii / Disponibil / Ramas as in the form of a reception, on the day of the reception
    ' (the answers come in with BeginViewedDispo and the table is laid out again each time).
    Private Sub FillViewedAlocari(k_state As EFacturaKeStare)
        _filling = True
        gridAloc.BeginUpdate()
        Try
            gridAloc.ClearRows()
            For Each k_group As IGrouping(Of String, EFacturaKeLegatura) In k_state.Legaturi.GroupBy(
                    Function(k_l) k_l.CodAngajament & "|" & k_l.IdClsf.ToString(CultureInfo.InvariantCulture)).OrderBy(
                    Function(k_g) k_g.First().Clsf, StringComparer.CurrentCultureIgnoreCase)
                Dim k_first As EFacturaKeLegatura = k_group.First()
                Dim k_row As KBotDataRow = gridAloc.AddRow()
                Dim k_value As Decimal = k_group.Sum(Function(k_l) k_l.Suma)
                k_row(ColAClsf) = k_first.Clsf
                k_row(ColAValRecep) = CDbl(k_value)
                Dim k_answer As EFacturaKeDisponibilZi = Nothing
                If _viewDispo.TryGetValue(ViewKey(k_first), k_answer) Then
                    Dim k_d As EFacturaKeDisponibil = k_answer.Indicatori.FirstOrDefault(
                        Function(k_x) k_x.IdClsf.HasValue AndAlso k_x.IdClsf.Value = k_first.IdClsf)
                    If k_d IsNot Nothing Then
                        k_row(ColAReceptii) = CDbl(k_d.Receptii)
                        If k_d.Buget.HasValue Then
                            Dim k_available As Decimal = k_d.Buget.Value - k_d.Receptii
                            k_row(ColABuget) = CDbl(k_d.Buget.Value)
                            k_row(ColADisponibil) = CDbl(k_available)
                            k_row(ColARamas) = CDbl(k_available - k_value)
                        End If
                    End If
                End If
            Next
        Finally
            gridAloc.EndUpdate()
            _filling = False
        End Try
    End Sub

    ' UI boundary (async Sub): asks the server, once for each angajament and reception day of the links, for the budget and the receptions before that day.
    Private Async Sub BeginViewedDispo(k_state As EFacturaKeStare, k_seq As Integer)
        Try
            If _api Is Nothing Then Return
            For Each k_link As EFacturaKeLegatura In k_state.Legaturi
                Dim k_key As String = ViewKey(k_link)
                If _viewDispo.ContainsKey(k_key) Then Continue For
                Dim k_cod As String = k_link.CodAngajament
                Dim k_day As Date = If(k_link.DataR.HasValue, k_link.DataR.Value.Date, Date.Today)
                Dim k_answer As EFacturaKeDisponibilZi = Nothing
                If Not _dispoCache.TryGetValue(k_key, k_answer) Then
                    k_answer = Await _gate.RunAsync(Function() _api.GetKeDisponibilAsync(k_cod, k_day, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed OrElse k_seq <> _viewSeq Then Return
                    _dispoCache(k_key) = k_answer
                End If
                If IsDisposed OrElse k_seq <> _viewSeq Then Return
                _viewDispo(k_key) = k_answer
                FillViewedAlocari(k_state)
            Next
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginViewedDispo", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginViewedDispo", ex)
            ShowNotice("Bugetul și recepțiile pe indicatori nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' UI boundary: the button under the receptions of a resolved invoice on view.
    Private Sub BtnElimina_Click(sender As Object, e As EventArgs) Handles btnElimina.Click
        Try
            If _viewing IsNot Nothing Then BeginUnlink(_viewing)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BtnElimina_Click", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): every link the invoice has with a reception is removed in K-BOT (the receptions in FOREXE are not touched); the tree is refreshed.
    Private Async Sub BeginUnlink(k_invoice As EFacturaPrimita)
        Try
            If _busy OrElse IsDisposed Then Return
            If KBotMessage.Show(Me, $"Eliminați asocierea facturii {k_invoice.NrFact} cu recepția? Recepția din FOREXE nu se atinge.",
                                "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            SetBusy(True)
            Try
                Dim k_id As Integer = k_invoice.IdPrimita
                Dim k_state As EFacturaKeStare = Await _gate.RunAsync(Function() _api.GetKeAsync(k_id, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                If Not k_state.PoateScrie Then
                    ShowNotice("Numai un utilizator cu rolul «Contabil» poate elimina asocierea facturilor cu recepțiile.", NoticeKind.Warning)
                    Return
                End If
                For Each k_link As EFacturaKeLegatura In k_state.Legaturi.ToList()
                    Dim k_idLink As Integer = k_link.IdLegatura
                    Await _gate.RunAsync(Function() _api.RemoveKeLegaturaAsync(k_id, k_idLink, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                Next
                Await ReloadAsync(True).ConfigureAwait(True)
                If Not IsDisposed Then ShowNotice($"Asocierea facturii {k_invoice.NrFact} cu recepția a fost eliminată.", NoticeKind.Success)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginUnlink", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginUnlink", ex)
            ShowNotice("Asocierea nu a putut fi eliminată. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' Reads the state of every ticked invoice that is not known yet, then puts the ticked ones on screen.
    Private Async Function SyncCheckedAsync() As Task
        Dim k_mySeq As Integer = Interlocked.Increment(_syncSeq)
        For Each k_id As Integer In CheckedIds()
            If _states.ContainsKey(k_id) Then Continue For
            Dim k_idNow As Integer = k_id
            Try
                Dim k_state As EFacturaKeStare = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _syncSeq Then Return
                _states(k_idNow) = k_state
                If Not k_state.PoateScrie Then ShowNotice("Numai un utilizator cu rolul «Contabil» poate lega facturile de recepții.", NoticeKind.Warning)
            Catch ex As ApiException
                GlobalErrorLog.Write("KeLegaForm.SyncCheckedAsync", ex)
                tree.SetItemCheckState(_nodes(k_idNow), AdvancedTreeControl.TreeCheckState.Unchecked)
                ShowNotice(ex.Message, NoticeKind.Error)
            End Try
        Next
        If IsDisposed OrElse k_mySeq <> _syncSeq Then Return
        _checked = CheckedIds()
        ApplyChecked()
    End Function

    ' ── What the ticks change ───────────────────────────────────────────────────

    Private Sub ApplyChecked()
        _filling = True
        gridLinii.BeginUpdate()
        Try
            Dim k_keep As New HashSet(Of Integer)(_checked)
            For k_i As Integer = gridLinii.RowCount - 1 To 0 Step -1
                Dim k_tag As LineTag = TryCast(gridLinii.Rows(k_i).Tag, LineTag)
                If k_tag Is Nothing OrElse Not k_keep.Contains(k_tag.Invoice.IdPrimita) Then gridLinii.RemoveRowAt(k_i)
            Next
            Dim k_have As New HashSet(Of Integer)(gridLinii.Rows.Select(Function(k_r) CType(k_r.Tag, LineTag).Invoice.IdPrimita))
            For Each k_id As Integer In _checked
                If k_have.Contains(k_id) Then Continue For
                For Each k_line As EFacturaKeLinie In _states(k_id).Linii
                    Dim k_row As KBotDataRow = gridLinii.AddRow()
                    k_row.Tag = New LineTag() With {.Invoice = _invoices(k_id), .Linie = k_line}
                    k_row(KBotDataView.CheckColumnKey) = True
                    k_row(ColLDenumire) = k_line.Denumire
                    k_row(ColLCuTva) = CDbl(k_line.ValoareCuTva)
                    k_row(ColLRec) = CDbl(k_line.ValoareCuTva)
                    k_row(ColLClsf) = InitialLabel(k_line)
                Next
            Next
            AssignGroupKeys()
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
        ApplyIndicators()
        ApplySingleIndicator(False)
        FillAngajamente()
        ApplyFieldDefaults()
        RecomputeDefaults()
        FillAlocari()
        UpdateTitles()
    End Sub

    ' What a line shows before the operator touches it: the choice it already has, else «not chosen».
    Private Function InitialLabel(k_line As EFacturaKeLinie) As String
        If Not k_line.IdClsf.HasValue Then Return NeAleasa
        If _labelOf.ContainsKey(k_line.IdClsf.Value) Then Return _labelOf(k_line.IdClsf.Value)
        Return If(String.IsNullOrWhiteSpace(k_line.Clsf), NeAleasa, k_line.Clsf)
    End Function

    ' The group of an invoice is its «number / date»; two ticked invoices that would read the same get the supplier added, so they stay two groups.
    Private Sub AssignGroupKeys()
        Dim k_base As New Dictionary(Of Integer, String)()
        For Each k_id As Integer In _checked
            Dim k_p As EFacturaPrimita = _invoices(k_id)
            k_base(k_id) = $"{If(k_p.Tip = "NC", "NC ", String.Empty)}{k_p.NrFact} / {DateText(k_p)}"
        Next
        Dim k_count As Dictionary(Of String, Integer) = k_base.Values.GroupBy(Function(k_s) k_s).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.Count())
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_tag As LineTag = CType(k_row.Tag, LineTag)
            Dim k_text As String = k_base(k_tag.Invoice.IdPrimita)
            If k_count(k_text) > 1 Then k_text &= " · " & k_tag.Invoice.Furnizor
            k_row(ColFactura) = k_text
        Next
    End Sub

    ' The date and the description of a new reception follow the ticked invoices, until the accountant writes his own.
    Private Sub ApplyFieldDefaults()
        If _mode <> EFacturaKeTip.Noua OrElse _checked.Count = 0 Then Return
        Dim k_list As List(Of EFacturaPrimita) = _checked.Select(Function(k_id) _invoices(k_id)).ToList()
        Dim k_newDate As Date = k_list.Select(Function(k_p) If(k_p.DataFact, Date.Today)).Max()
        If _dataAuto = Date.MinValue OrElse dtpData.Value.Date = _dataAuto.Date Then
            dtpData.Value = k_newDate
            _dataAuto = k_newDate
        End If
        Dim k_text As String = "plata " & If(k_list.Count = 1, "factura ", "facturi ") & String.Join(", ", k_list.Select(Function(k_p) DescriptionPart(k_p)))
        If String.Equals(txtDescriere.Text, _descrAuto, StringComparison.Ordinal) Then
            txtDescriere.Text = k_text
            _descrAuto = k_text
        End If
    End Sub

    Private Shared Function DescriptionPart(k_p As EFacturaPrimita) As String
        Return k_p.NrFact & If(k_p.DataFact.HasValue, "/" & k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
    End Function

    Private Sub UpdateTitles()
        If _viewing IsNot Nothing Then
            lblTitluLinii.Text = $"Produse Factura {_viewing.NrFact} / {DateText(_viewing)}"
            lblTitluDate.Text = "Recepția asociată"
            Return
        End If
        Select Case _checked.Count
            Case 0 : lblTitluLinii.Text = "Produse factură"
            Case 1 : lblTitluLinii.Text = $"Produse Factura {_invoices(_checked(0)).NrFact} / {DateText(_invoices(_checked(0)))}"
            Case Else : lblTitluLinii.Text = $"Produse facturi bifate ({_checked.Count})"
        End Select
        If _mode = EFacturaKeTip.Noua Then
            lblTitluDate.Text = "Date recepție nouă"
        Else
            Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
            lblTitluDate.Text = If(k_rec Is Nothing, "Date recepție",
                $"Date recepție {k_rec.Descriere}" & If(k_rec.DataR.HasValue, $" ({k_rec.DataR.Value:dd.MM.yyyy})", String.Empty))
        End If
    End Sub

    ' What the mode shows: a list of receptions (existing / modified) or the form of a new one; the title and the button follow.
    Private Sub ApplyMode()
        Dim k_nou As Boolean = (_mode = EFacturaKeTip.Noua)
        gridReceptii.Visible = Not k_nou
        pnlNoua.Visible = k_nou
        Select Case _mode
            Case EFacturaKeTip.Existenta
                btnOk.Text = "Leagă"
                tips.SetToolTipText(btnOk, "Leagă facturile bifate de recepția aleasă, pe sumele din «Recepționată». Recepția nu se schimbă.")
            Case EFacturaKeTip.Noua
                btnOk.Text = "Creează recepția"
                tips.SetToolTipText(btnOk, "Creează în FOREXE o recepție cu sumele de mai jos (a tuturor facturilor bifate) și le leagă de ea.")
            Case Else
                btnOk.Text = "Modifică recepția"
                tips.SetToolTipText(btnOk, "Mărește în FOREXE recepția aleasă cu sumele de mai jos și leagă facturile bifate de ea.")
        End Select
        gridAloc.Column(ColARestRecep).Visible = If(_mode = EFacturaKeTip.Existenta, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
        btnOk.Enabled = False
        UpdateTitles()
    End Sub

    ' UI boundary (Load, async Sub): reads the invoices of the year and the angajamente for the empty row.
    Private Async Sub KeLegaForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            ' The embedded variant is given its invoice by the host (ShowInvoice): it reads no list.
            If Not _embedded Then Await ReloadAsync().ConfigureAwait(True)
            If IsDisposed Then Return
            If Not _embedded OrElse _fixedAng.Length = 0 Then Await LoadAllAngajamenteAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.KeLegaForm_Load", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.KeLegaForm_Load", ex)
            ShowNotice("Pagina nu a putut citi datele. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' After a reception (or an un-association): the tree stays as it was (expanded or not, scrolled), every row unticked and with its numbers refreshed; the invoices a reception
    ' covers now become ticked and locked (KE13). An invoice that is gone (or has no value) leaves, a supplier left with none goes too. When the tree shows only the associated
    ' invoices the list can gain rows, so it is built again (the suppliers keep their open / closed state).
    Private Sub RefreshTree(k_list As IEnumerable(Of EFacturaPrimita))
        Dim k_fresh As List(Of EFacturaPrimita) = k_list.ToList()
        If _onlyAssociated Then
            RebuildTree(k_fresh)
            Return
        End If
        _lastList = k_fresh
        Dim k_now As New Dictionary(Of Integer, EFacturaPrimita)()
        For Each k_p As EFacturaPrimita In k_fresh
            k_now(k_p.IdPrimita) = k_p
        Next
        Dim k_suppliers As New List(Of AdvancedTreeControl.TreeItem)()
        For Each k_id As Integer In _order.ToList()
            Dim k_node As AdvancedTreeControl.TreeItem = _nodes(k_id)
            Dim k_p As EFacturaPrimita = Nothing
            If k_node.Parent IsNot Nothing AndAlso Not k_suppliers.Contains(k_node.Parent) Then k_suppliers.Add(k_node.Parent)
            If Not k_now.TryGetValue(k_id, k_p) OrElse Not ShowsInTree(k_p) Then
                Dim k_parent As AdvancedTreeControl.TreeItem = k_node.Parent
                tree.RemoveItem(k_node)
                _nodes.Remove(k_id)
                _order.Remove(k_id)
                _invoices.Remove(k_id)
                If k_parent IsNot Nothing AndAlso k_parent.Children.Count = 0 Then tree.RemoveItem(k_parent)
            Else
                _invoices(k_id) = k_p
                k_node.Tag = k_p
                StyleLeaf(k_node, k_p)
            End If
        Next
        ' The total of a supplier is what its remaining invoices add up to.
        For Each k_sup As AdvancedTreeControl.TreeItem In k_suppliers
            If k_sup.Children.Count = 0 Then Continue For
            Dim k_rest As List(Of EFacturaPrimita) = k_sup.Children.Select(Function(k_c) TryCast(k_c.Tag, EFacturaPrimita)).Where(Function(k_x) k_x IsNot Nothing).ToList()
            If k_rest.Count > 0 Then k_sup.Caption = $"{SupplierName(k_rest)}~~~{k_rest.Sum(Function(k_x) k_x.Total).ToString("N2", _ro)}"
            StyleSupplier(k_sup)
        Next
        ShowTreeTotal()
        tree.Invalidate()
    End Sub

    ' The tree built again from these invoices; the suppliers that were open stay open.
    Private Sub RebuildTree(k_list As IEnumerable(Of EFacturaPrimita))
        Dim k_open As New HashSet(Of String)()
        For Each k_node As AdvancedTreeControl.TreeItem In _nodes.Values
            If k_node.Parent IsNot Nothing AndAlso k_node.Parent.Expanded Then k_open.Add(k_node.Parent.Key)
        Next
        BuildTree(k_list)
        For Each k_node As AdvancedTreeControl.TreeItem In _nodes.Values
            If k_node.Parent IsNot Nothing AndAlso k_open.Contains(k_node.Parent.Key) Then k_node.Parent.Expanded = True
        Next
        tree.Invalidate()
    End Sub

    ' The angajamente the empty row can offer, read once.
    Private Async Function LoadAllAngajamenteAsync() As Task
        If _allAngLoaded Then Return
        _allAngLoaded = True
        _allAng = Await _gate.RunAsync(Function() _api.GetKeAngajamenteAsync(Nothing, _cts.Token)).ConfigureAwait(True)
        If IsDisposed Then Return
        FillAngajamenteChoices()
    End Function

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub BeginLoadAllAngajamente()
        Try
            If _api Is Nothing Then Return
            Await LoadAllAngajamenteAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            _allAngLoaded = False
            GlobalErrorLog.Write("KeLegaForm.BeginLoadAllAngajamente", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            _allAngLoaded = False
            GlobalErrorLog.Write("KeLegaForm.BeginLoadAllAngajamente", ex)
            ShowNotice("Angajamentele nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── The embedded variant (slice KE12-04) ────────────────────────────────────

    ''' <summary>
    ''' Turns the form into a panel of a view of the invoices: no frame, no header, no placement of its own, docked by the host. Once, before it is shown. The invoice is
    ''' then given with <see cref="ShowInvoice"/>.
    ''' </summary>
    Public Sub Embed()
        _embedded = True
        TopLevel = False
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        MinimumSize = System.Drawing.Size.Empty
        Padding = Padding.Empty
        Dock = DockStyle.Fill
        AutoFitToTheme = False
        CenterOnScreen = False
        capBar.Visible = False
    End Sub

    ''' <summary>
    ''' The embedded variant works on THIS invoice (already ticked, no tree shown) and, when <paramref name="k_angajament"/> is not empty (the view of an angajament), on THAT
    ''' angajament (no table of angajamente shown). <paramref name="k_state"/> is the state of the invoice the host already read. Everything kept about a former invoice is dropped.
    ''' UI boundary: logs and swallows.
    ''' </summary>
    Public Sub ShowInvoice(k_invoice As EFacturaPrimita, k_state As EFacturaKeStare, k_angajament As String)
        Try
            ArgumentNullException.ThrowIfNull(k_invoice)
            ArgumentNullException.ThrowIfNull(k_state)
            _embedded = True
            _fixedAng = If(k_angajament, String.Empty).Trim()
            ApplyEmbeddedLayout()
            ResetWork()
            BuildTree(New List(Of EFacturaPrimita) From {k_invoice}, True)
            _states(k_invoice.IdPrimita) = k_state
            tree.SetItemCheckState(_nodes(k_invoice.IdPrimita), AdvancedTreeControl.TreeCheckState.Checked)
            _checked = CheckedIds()
            ApplyChecked()
            FillReceptii()
            If _fixedAng.Length = 0 Then BeginLoadAllAngajamente()
            If _pendingNotice.Length > 0 Then
                ShowNotice(_pendingNotice, NoticeKind.Success)
                _pendingNotice = String.Empty
            ElseIf Not k_state.PoateScrie Then
                ShowNotice("Numai un utilizator cu rolul «Contabil» poate lega facturile de recepții.", NoticeKind.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.ShowInvoice", ex)
            ShowNotice("Recepțiile facturii nu au putut fi afișate. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ''' <summary>The embedded variant shows nothing (the host has no invoice to show). UI boundary: logs and swallows.</summary>
    Public Sub ClearInvoice()
        Try
            ResetWork()
            tree.Clear()
            _nodes.Clear()
            _order.Clear()
            _invoices.Clear()
            _checked = New List(Of Integer)()
            ApplyChecked()
            FillReceptii()
            ntfMesaj.Visible = False
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.ClearInvoice", ex)
        End Try
    End Sub

    ' What was kept about the former invoice / angajament.
    Private Sub ResetWork()
        Interlocked.Increment(_syncSeq)
        Interlocked.Increment(_angSeq)
        _states.Clear()
        _angCache.Clear()
        _dispoCache.Clear()
        _dispoKey = String.Empty
        _dispo.Clear()
        _browsed = Nothing
        _data = Nothing
        _angCod = String.Empty
        txtDescriere.Text = String.Empty
        _descrAuto = String.Empty
        _dataAuto = Date.MinValue
        If _pendingNotice.Length = 0 Then ntfMesaj.Visible = False
    End Sub

    ' Without the tree of invoices; without the table of angajamente when the angajament is given.
    Private Sub ApplyEmbeddedLayout()
        capBar.Visible = False
        tree.Visible = False
        ' No title bands over the lower zones: the navbar and the table say what they are.
        lblTitluDate.Visible = False
        lblTitluAng.Visible = False
        tlySus.ColumnStyles(0).SizeType = SizeType.Absolute
        tlySus.ColumnStyles(0).Width = 0
        tlySus.ColumnStyles(1).SizeType = SizeType.Percent
        tlySus.ColumnStyles(1).Width = 100
        Dim k_fixed As Boolean = _fixedAng.Length > 0
        pnlAng.Visible = Not k_fixed
        If k_fixed Then
            tlyJos.ColumnStyles(0).SizeType = SizeType.Absolute
            tlyJos.ColumnStyles(0).Width = 0
        Else
            tlyJos.ColumnStyles(0).SizeType = SizeType.Percent
            tlyJos.ColumnStyles(0).Width = 24
        End If
    End Sub

    ' Reads the invoices again (the ones a reception covers leave the tree) and forgets what was kept about them; nothing is ticked afterwards.
    Private Async Function ReloadAsync(Optional k_keepTree As Boolean = False) As Task
        Dim k_year As Integer? = If(_year > 0, CType(_year, Integer?), Nothing)
        Dim k_list As List(Of EFacturaPrimita) = Await _gate.RunAsync(
            Function() _api.GetPrimiteAsync(k_year, Nothing, Nothing, Nothing, _cts.Token)).ConfigureAwait(True)
        If IsDisposed Then Return
        Interlocked.Increment(_syncSeq)
        _states.Clear()
        _angCache.Clear()
        _dispoCache.Clear()
        _dispoKey = String.Empty
        _browsed = Nothing
        _data = Nothing
        _checked = New List(Of Integer)()
        LeaveViewing()
        If k_keepTree Then RefreshTree(k_list) Else BuildTree(k_list)
        ApplyChecked()
        FillReceptii()
        If _nodes.Count = 0 Then
            ShowNotice(If(_onlyAssociated, "Nicio factură primită nu este asociată cu o recepție în această perioadă.", "Nu există facturi primite în această perioadă."), NoticeKind.Warning)
        End If
    End Function

    ' UI boundary: the date of a new reception changed -> the budget and the receptions before it are asked again.
    Private Sub DtpData_ValueChanged(sender As Object, e As EventArgs) Handles dtpData.ValueChanged
        Try
            If _mode = EFacturaKeTip.Noua Then BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.DtpData_ValueChanged", ex)
        End Try
    End Sub

    ' The day the budget and the receptions are taken on: the date of the new reception, or the date of the chosen one.
    Private Function DispoDay() As Date
        If _mode = EFacturaKeTip.Noua Then Return dtpData.Value.Date
        Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
        Return If(k_rec IsNot Nothing AndAlso k_rec.DataR.HasValue, k_rec.DataR.Value.Date, Date.Today)
    End Function

    Private Function DispoOf(k_idClsf As Integer) As EFacturaKeDisponibil
        Dim k_d As EFacturaKeDisponibil = Nothing
        Return If(_dispo.TryGetValue(k_idClsf, k_d), k_d, Nothing)
    End Function

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub BeginRefreshDispo()
        Try
            Await RefreshDispoAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginRefreshDispo", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginRefreshDispo", ex)
            ShowNotice("Bugetul și recepțiile pe indicatori nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    Private Async Function RefreshDispoAsync() As Task
        If _api Is Nothing OrElse _data Is Nothing OrElse String.IsNullOrWhiteSpace(_angCod) Then Return
        Dim k_day As Date = DispoDay()
        Dim k_key As String = _angCod & "|" & k_day.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
        If String.Equals(k_key, _dispoKey, StringComparison.OrdinalIgnoreCase) Then Return
        Dim k_mySeq As Integer = Interlocked.Increment(_dispoSeq)
        Dim k_answer As EFacturaKeDisponibilZi = Nothing
        If Not _dispoCache.TryGetValue(k_key, k_answer) Then
            Dim k_cod As String = _angCod
            k_answer = Await _gate.RunAsync(Function() _api.GetKeDisponibilAsync(k_cod, k_day, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _dispoSeq Then Return
            _dispoCache(k_key) = k_answer
        End If
        If IsDisposed OrElse k_mySeq <> _dispoSeq Then Return
        _dispo.Clear()
        For Each k_d As EFacturaKeDisponibil In k_answer.Indicatori
            If k_d.IdClsf.HasValue AndAlso Not _dispo.ContainsKey(k_d.IdClsf.Value) Then _dispo(k_d.IdClsf.Value) = k_d
        Next
        _dispoKey = k_key
        _dispoWarning = String.Join(" ", k_answer.Avertismente)
        FillAlocari()
    End Function

    ' UI boundary: the navbar (new reception / existing receptions) changed.
    Private Sub NavMod_SelectionChanged(k_key As String) Handles navMod.SelectionChanged
        Try
            ChangeMode()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.NavMod_SelectionChanged", ex)
        End Try
    End Sub

    ' UI boundary: what is done with an existing reception (link / modify) was chosen.
    Private Sub CmbMod_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbMod.SelectedIndexChanged
        Try
            ChangeMode()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.CmbMod_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The mode is the navbar (new / existing) and, on «Recepții existente», the action chosen below it (link / modify).
    Private Sub ChangeMode()
        Try
            If _loadingMode OrElse cmbMod.SelectedIndex < 0 Then Return
            Dim k_new As Boolean = Not String.Equals(navMod.SelectedKey, TabExistente, StringComparison.Ordinal)
            pnlMod.Visible = Not k_new
            _mode = If(k_new, EFacturaKeTip.Noua, _actionKeys(cmbMod.SelectedIndex))
            ApplyMode()
            FillReceptii()
            ApplyFieldDefaults()
            RecomputeDefaults()
            FillAlocari()
            UpdateTitles()
            BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.ChangeMode", ex)
        End Try
    End Sub

    ' The embedded variant is never closed: it is let go with the view that holds it.
    Private Sub KeLegaForm_HandleDestroyed(sender As Object, e As EventArgs) Handles MyBase.HandleDestroyed
        Try
            If _embedded Then _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.KeLegaForm_HandleDestroyed", ex)
        End Try
    End Sub

    Private Sub KeLegaForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.KeLegaForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind)
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
    End Sub

    ' ── Angajamente ─────────────────────────────────────────────────────────────

    Private Shared Function LabelOf(k_a As EFacturaKeAngajamentAlegere) As String
        Return If(String.IsNullOrWhiteSpace(k_a.Eticheta), k_a.CodAngajament, $"{k_a.CodAngajament} — {k_a.Eticheta}")
    End Function

    ' The choices of the empty row: every angajament the server lists (typing narrows them: the code or the start of the description).
    Private Sub FillAngajamenteChoices()
        _angByLabel.Clear()
        Dim k_labels As New List(Of Object)()
        ' The list offered does not contain the angajamente already in the grid (the ones of the same invoices / supplier); a typed one is still found in _angByLabel.
        Dim k_used As New HashSet(Of String)(_items.Select(Function(k_i) k_i.Cod), StringComparer.OrdinalIgnoreCase)
        For Each k_a As EFacturaKeAngajamentAlegere In _allAng
            Dim k_label As String = LabelOf(k_a)
            If _angByLabel.ContainsKey(k_label) Then Continue For
            _angByLabel(k_label) = k_a
            If Not k_used.Contains(k_a.CodAngajament) Then k_labels.Add(k_label)
        Next
        gridAng.Column(ColAng).ComboItems = k_labels
    End Sub

    ' The rows: the angajamente the ticked invoices can go to (the server's candidates: DDF of the supplier, links already made) and the ones
    ' added here; then ALWAYS an empty row. The angajament in use stays the current row.
    Private Sub FillAngajamente()
        If _viewing IsNot Nothing Then
            FillViewedAngajamente()
            Return
        End If
        Dim k_items As New List(Of AngItem)()
        Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        ' The view of an angajament: that angajament and no other.
        If _fixedAng.Length > 0 Then
            k_items.Add(New AngItem() With {.Cod = _fixedAng, .Sursa = "fix"})
            k_seen.Add(_fixedAng)
        End If
        Dim k_sources As New List(Of EFacturaKeStare)()
        For Each k_id As Integer In _checked
            Dim k_state As EFacturaKeStare = Nothing
            If _states.TryGetValue(k_id, k_state) Then k_sources.Add(k_state)
        Next
        If _checked.Count = 0 AndAlso _browsed IsNot Nothing Then k_sources.Add(_browsed)
        ' Slice KE14: an angajament a reception of these invoices is linked to cannot be taken off the list (its button is hidden).
        Dim k_linked As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each k_state As EFacturaKeStare In k_sources
            For Each k_link As EFacturaKeLegatura In k_state.Legaturi
                k_linked.Add(k_link.CodAngajament)
            Next
        Next
        If _fixedAng.Length > 0 Then k_sources.Clear()
        For Each k_state As EFacturaKeStare In k_sources
            For Each k_a As EFacturaKeAngajament In k_state.Angajamente
                If k_seen.Add(k_a.CodAngajament) Then
                    k_items.Add(New AngItem() With {.Cod = k_a.CodAngajament, .Descriere = k_a.Descriere, .Sursa = k_a.Sursa})
                End If
            Next
        Next
        For Each k_a As AngItem In k_items
            k_a.Linked = k_linked.Contains(k_a.Cod)
        Next
        _items = k_items
        FillAngajamenteChoices()
        _angFilling = True
        gridAng.BeginUpdate()
        Try
            gridAng.ClearRows()
            For Each k_a As AngItem In k_items
                Dim k_row As KBotDataRow = gridAng.AddRow()
                k_row.Tag = k_a
                ' In a view of the invoices the code of the angajament is not shown: only its description (the code when it has none).
                k_row(ColAng) = If(_embedded AndAlso Not String.IsNullOrWhiteSpace(k_a.Descriere), k_a.Descriere, k_a.Eticheta)
                k_row(ColOpt) = If(k_a.Linked, String.Empty, "…")
            Next
            Dim k_empty As KBotDataRow = gridAng.AddRow()
            k_empty.Tag = Nothing
            k_empty(ColAng) = String.Empty
            k_empty(ColOpt) = String.Empty
        Finally
            gridAng.EndUpdate()
            _angFilling = False
        End Try
        Dim k_index As Integer = k_items.FindIndex(Function(k_a) String.Equals(k_a.Cod, _angCod, StringComparison.OrdinalIgnoreCase))
        If k_index < 0 AndAlso k_items.Count = 1 Then k_index = 0
        If k_index >= 0 Then
            gridAng.CurrentRowIndex = k_index
            BeginUseAngajament(k_items(k_index).Cod)
        ElseIf _checked.Count > 0 OrElse _browsed IsNot Nothing Then
            ' The angajament in use is no longer offered (its invoice was unticked, or another supplier was pressed): nothing is chosen.
            _angCod = String.Empty
            _data = Nothing
            ClearAngajamentData()
        End If
        BeginLoadOtherAngajamente()
    End Sub

    Private Sub ClearAngajamentData()
        _byLabel.Clear()
        _labelOf.Clear()
        gridReceptii.ClearRows()
        gridLinii.Column(ColLClsf).ComboItems = New List(Of Object) From {NeAleasa}
        RecomputeDefaults()
        FillAlocari()
        UpdateTitles()
    End Sub

    ' The existing rows cannot be changed (a click opens no editor, the cell is not greyed); the empty row takes a code / description; its button is hidden.
    Private Sub GridAng_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridAng.CellFormatting
        Try
            If e.Row Is Nothing Then Return
            Dim k_real As Boolean = TypeOf e.Row.Tag Is AngItem
            If String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) Then
                If k_real Then e.ReadOnlyCell = True
            ElseIf String.Equals(e.ColumnKey, ColOpt, StringComparison.Ordinal) Then
                If Not k_real OrElse CType(e.Row.Tag, AngItem).Linked Then e.Text = String.Empty
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridAng_CellFormatting", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): the current row of the angajamente changed: that angajament is the one of the reception.
    Private Sub GridAng_SelectionChanged(sender As Object, e As EventArgs) Handles gridAng.SelectionChanged
        Try
            If _angFilling OrElse _viewing IsNot Nothing Then Return
            Dim k_item As AngItem = TryCast(gridAng.CurrentRow?.Tag, AngItem)
            If k_item IsNot Nothing Then BeginUseAngajament(k_item.Cod)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridAng_SelectionChanged", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub BeginUseAngajament(k_cod As String)
        Try
            Await UseAngajamentAsync(k_cod).ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginUseAngajament", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginUseAngajament", ex)
            ShowNotice("Recepțiile angajamentului nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' Reads the receptions and the indicators of the angajament (once; later they come from memory) and puts them on screen.
    Private Async Function UseAngajamentAsync(k_cod As String) As Task
        If _api Is Nothing OrElse String.IsNullOrWhiteSpace(k_cod) Then Return
        ' Already in use only when the data on screen ARE this angajament's (a code set by the empty row before its data were read must not count).
        Dim k_cached As EFacturaKeReceptiiAngajament = Nothing
        If String.Equals(k_cod, _angCod, StringComparison.OrdinalIgnoreCase) AndAlso _data IsNot Nothing AndAlso
           _angCache.TryGetValue(k_cod, k_cached) AndAlso ReferenceEquals(k_cached, _data) Then Return
        _angCod = k_cod
        _data = Nothing
        _dispo.Clear()
        _dispoKey = String.Empty
        Dim k_mySeq As Integer = Interlocked.Increment(_angSeq)
        ntfMesaj.Visible = False
        btnOk.Enabled = False
        Dim k_data As EFacturaKeReceptiiAngajament = Nothing
        If Not _angCache.TryGetValue(k_cod, k_data) Then
            Dim k_codNow As String = k_cod
            k_data = Await _gate.RunAsync(Function() _api.GetKeReceptiiAsync(k_codNow, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _angSeq Then Return
            _angCache(k_cod) = k_data
        End If
        If IsDisposed OrElse k_mySeq <> _angSeq Then Return
        _data = k_data
        ApplyIndicators()
        ApplySingleIndicator(True)
        FillReceptii()
        RecomputeDefaults()
        FillAlocari()
        UpdateTitles()
        BeginRefreshDispo()
        If _data.Indicatori.Count = 0 Then
            ShowNotice("Angajamentul nu are indicatori: o recepție nu poate fi făcută până nu are rezervări (documentul de fundamentare).", NoticeKind.Warning)
        End If
    End Function

    ' The empty row got a text: the angajament it names is added to the rows and becomes the current one.
    Private Sub GridAng_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridAng.CellValueChanged
        Try
            If _angFilling OrElse Not String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAng.RowCount OrElse TypeOf gridAng.Rows(e.RowIndex).Tag Is AngItem Then Return
            Dim k_text As String = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) Then Return
            ' Not inside the grid's own commit: the rows are rebuilt after it.
            BeginInvoke(New Action(Sub() BeginAddAngajament(k_text.Trim())))
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridAng_CellValueChanged", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): finds the angajament the text names (an item of the list, a code, else the server's search) and adds it.
    Private Async Sub BeginAddAngajament(k_text As String)
        Try
            Dim k_found As EFacturaKeAngajamentAlegere = Nothing
            If Not _angByLabel.TryGetValue(k_text, k_found) Then
                k_found = _allAng.FirstOrDefault(Function(k_a) String.Equals(k_a.CodAngajament, k_text, StringComparison.OrdinalIgnoreCase))
            End If
            If k_found Is Nothing Then
                Dim k_list As List(Of EFacturaKeAngajamentAlegere) = Await _gate.RunAsync(
                    Function() _api.GetKeAngajamenteAsync(k_text, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                If k_list.Count = 1 Then
                    k_found = k_list(0)
                ElseIf k_list.Count = 0 Then
                    ShowNotice($"Niciun angajament nu se potrivește cu «{k_text}».", NoticeKind.Warning)
                Else
                    ShowNotice($"{k_list.Count} angajamente se potrivesc cu «{k_text}»: scrieți mai mult din cod sau din descriere.", NoticeKind.Warning)
                End If
            End If
            If k_found IsNot Nothing Then
                If Not _items.Any(Function(k_i) String.Equals(k_i.Cod, k_found.CodAngajament, StringComparison.OrdinalIgnoreCase)) Then
                    ' Slice 0121-05: the pick is saved for the SUPPLIER (its tax code); it comes back from the server as a candidate.
                    Dim k_saved As Boolean
                    For Each k_key As String In ContextKeys()
                        Dim k_cui As String = SupplierCui(k_key)
                        If k_cui.Length = 0 Then
                            ShowNotice("Furnizorul nu are cod fiscal pe factură: angajamentul nu se poate lega de el.", NoticeKind.Warning)
                            Continue For
                        End If
                        Dim k_codNow As String = k_found.CodAngajament
                        Await _gate.RunAsync(Function() _api.LinkSupplierAngajamentAsync(k_cui, k_codNow, _cts.Token)).ConfigureAwait(True)
                        If IsDisposed Then Return
                        k_saved = True
                    Next
                    If k_saved Then Await RefreshContextStatesAsync().ConfigureAwait(True)
                    If IsDisposed Then Return
                End If
                _angCod = k_found.CodAngajament
            End If
            FillAngajamente()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginAddAngajament", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginAddAngajament", ex)
            ShowNotice("Angajamentul nu a putut fi căutat. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' The button of a row: a menu with «Elimină asociere».
    Private Sub GridAng_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridAng.ButtonClick
        Try
            If Not String.Equals(e.ColumnKey, ColOpt, StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAng.RowCount Then Return
            Dim k_item As AngItem = TryCast(gridAng.Rows(e.RowIndex).Tag, AngItem)
            If k_item Is Nothing OrElse k_item.Linked Then Return
            Dim k_menu As New CustomPopup(New List(Of CustomPopupItem) From {New CustomPopupItem(MenuRemove, "&Elimină asociere")})
            AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) BeginRemoveAssociation(k_item)
            k_menu.ShowAt(gridAng, Control.MousePosition)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridAng_ButtonClick", ex)
        End Try
    End Sub

    ' UI boundary (async Sub). An angajament added here is simply dropped. One that comes from the server is an association of the invoices: the manual
    ' links the operator made with a DDF of that angajament are removed; one that comes from the supplier being a partner of the DDF, or from links with
    ' receptions, is not made here and cannot be removed here.
    Private Async Sub BeginRemoveAssociation(k_item As AngItem)
        Try
            If String.Equals(k_item.Sursa, "furnizor", StringComparison.Ordinal) Then
                ' Slice 0121-05: an angajament picked for the supplier is the supplier's link (with its indicators), removed for the suppliers of the invoices shown.
                If KBotMessage.Show(Me, $"Scoateți legătura furnizorului cu angajamentul {k_item.Cod}? Dispar și indicatorii legați de ea. Recepțiile din FOREXE nu se ating.",
                                    "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
                For Each k_key As String In ContextKeys()
                    Dim k_cui As String = SupplierCui(k_key)
                    If k_cui.Length = 0 Then Continue For
                    Dim k_codNow As String = k_item.Cod
                    Await _gate.RunAsync(Function() _api.UnlinkSupplierAngajamentAsync(k_cui, k_codNow, _cts.Token)).ConfigureAwait(True)
                    If IsDisposed Then Return
                Next
                Await RefreshContextStatesAsync().ConfigureAwait(True)
                If IsDisposed Then Return
                FillAngajamente()
                Return
            End If
            If KBotMessage.Show(Me, $"Scoateți asocierea facturilor bifate cu angajamentul {k_item.Cod}? Recepțiile din FOREXE nu se ating.",
                                "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Dim k_removed As Integer = 0
            For Each k_id As Integer In _checked.ToList()
                Dim k_idNow As Integer = k_id
                Dim k_detail As EFacturaPrimitaDetaliu = Await _gate.RunAsync(Function() _api.GetPrimitaAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
                If IsDisposed Then Return
                For Each k_link As EFacturaPrimitaDdf In k_detail.LegaturiManuale.Where(
                        Function(k_l) String.Equals(k_l.CodAngajament, k_item.Cod, StringComparison.OrdinalIgnoreCase)).ToList()
                    Dim k_idDdf As Integer = k_link.IdDdf
                    Await _gate.RunAsync(Function() _api.UnlinkPrimitaAsync(k_idNow, k_idDdf, _cts.Token)).ConfigureAwait(True)
                    k_removed += 1
                Next
                ' The state of the invoice (its candidate angajamente) is read again.
                _states(k_idNow) = Await _gate.RunAsync(Function() _api.GetKeAsync(k_idNow, _cts.Token)).ConfigureAwait(True)
            Next
            If IsDisposed Then Return
            If k_removed = 0 Then
                KBotMessage.Show(Me, $"Asocierea cu {k_item.Cod} nu a fost făcută de dumneavoastră: vine din DDF-ul la care furnizorul este partener sau din legături cu recepții. " &
                                 "Se scoate din DDF, respectiv din vederea «Recepții» a facturii.", "Recepții", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            FillAngajamente()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BeginRemoveAssociation", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginRemoveAssociation", ex)
            ShowNotice("Asocierea nu a putut fi scoasă. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' ── Receptions of the angajament ────────────────────────────────────────────

    Private Sub FillReceptii()
        If _viewing IsNot Nothing Then
            FillViewedReceptii()
            Return
        End If
        gridReceptii.BeginUpdate()
        Try
            gridReceptii.ClearRows()
            If _data Is Nothing Then Return
            For Each k_r As EFacturaKeReceptie In _data.Receptii
                ' Linking to an existing reception: one whose rest is 0 on every classification can take no invoice, so it is not listed (operator, 18.10.2026).
                If _mode = EFacturaKeTip.Existenta AndAlso k_r.PeClasificatie.All(Function(k_c) Math.Abs(k_c.Rest) < Tolerance) Then Continue For
                Dim k_row As KBotDataRow = gridReceptii.AddRow()
                k_row.Tag = k_r
                k_row(ColRData) = If(k_r.DataR.HasValue, k_r.DataR.Value.ToString("dd.MM.yyyy", _ro), String.Empty)
                k_row(ColRTip) = k_r.TipReceptie
                k_row(ColRDescriere) = k_r.Descriere
                k_row(ColRTotal) = CDbl(k_r.Total)
                k_row(ColRRest) = CDbl(k_r.PeClasificatie.Sum(Function(k_c) k_c.Rest))
            Next
        Finally
            gridReceptii.EndUpdate()
        End Try
    End Sub

    Private Function ChosenReceptie() As EFacturaKeReceptie
        If _mode = EFacturaKeTip.Noua Then Return Nothing
        Return TryCast(gridReceptii.CurrentRow?.Tag, EFacturaKeReceptie)
    End Function

    Private Sub GridReceptii_SelectionChanged(sender As Object, e As EventArgs) Handles gridReceptii.SelectionChanged
        Try
            If _viewing IsNot Nothing Then Return
            RecomputeDefaults()
            FillAlocari()
            UpdateTitles()
            BeginRefreshDispo()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridReceptii_SelectionChanged", ex)
        End Try
    End Sub

    Private Function IndicatorOf(k_idClsf As Integer) As EFacturaKeIndicator
        If _data Is Nothing Then Return Nothing
        Return _data.Indicatori.FirstOrDefault(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf)
    End Function

    ' ── Classifications of the lines ────────────────────────────────────────────

    ' The data (indicators, receptions) of the angajament in use first, then of every other angajament of the grid read so far (KE12-04).
    Private Function KnownData() As List(Of EFacturaKeReceptiiAngajament)
        Dim k_list As New List(Of EFacturaKeReceptiiAngajament)()
        If _data IsNot Nothing Then k_list.Add(_data)
        For Each k_a As AngItem In _items
            Dim k_d As EFacturaKeReceptiiAngajament = Nothing
            If _angCache.TryGetValue(k_a.Cod, k_d) AndAlso Not k_list.Contains(k_d) Then k_list.Add(k_d)
        Next
        Return k_list
    End Function

    ' Slice KE12-04: the list of classifications holds the indicators of ALL the angajamente of the grid. A classification that is not an indicator of the angajament in use
    ' moves the use to the first angajament that has it (the row of the grid follows); the other lines are judged against the new one at the button.
    Private Sub EnsureAngajamentFor(k_idClsf As Integer)
        If _data Is Nothing OrElse _data.Indicatori.Any(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf) Then Return
        For k_index As Integer = 0 To _items.Count - 1
            Dim k_d As EFacturaKeReceptiiAngajament = Nothing
            If _angCache.TryGetValue(_items(k_index).Cod, k_d) AndAlso k_d.Indicatori.Any(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf) Then
                gridAng.CurrentRowIndex = k_index
                Return
            End If
        Next
    End Sub

    ' UI boundary (async Sub): reads the indicators of the other angajamente of the grid, so their classifications can be chosen too.
    Private Async Sub BeginLoadOtherAngajamente()
        Try
            If _api Is Nothing Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_othersSeq)
            Dim k_loaded As Boolean = False
            For Each k_a As AngItem In _items.ToList()
                If _angCache.ContainsKey(k_a.Cod) Then Continue For
                Dim k_cod As String = k_a.Cod
                Dim k_data As EFacturaKeReceptiiAngajament = Await _gate.RunAsync(Function() _api.GetKeReceptiiAsync(k_cod, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _othersSeq Then Return
                _angCache(k_cod) = k_data
                k_loaded = True
            Next
            If k_loaded Then ApplyIndicators()
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BeginLoadOtherAngajamente", ex)
        End Try
    End Sub

    ' Slice KE12-06: an angajament with ONE indicator gives its classification to the lines by itself. k_all = every line (the angajament was just pressed: pressing another
    ' angajament changes them again); otherwise only the lines that have no classification yet (invoices ticked later).
    Private Sub ApplySingleIndicator(k_all As Boolean)
        If _data Is Nothing Then Return
        Dim k_ids As List(Of Integer) = _data.Indicatori.Where(Function(k_i) k_i.IdClsf.HasValue).Select(Function(k_i) k_i.IdClsf.Value).Distinct().ToList()
        Dim k_label As String = Nothing
        If k_ids.Count <> 1 OrElse Not _labelOf.TryGetValue(k_ids(0), k_label) Then Return
        _filling = True
        Try
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_current As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
                If k_all OrElse String.IsNullOrWhiteSpace(k_current) OrElse String.Equals(k_current, NeAleasa, StringComparison.Ordinal) Then k_row(ColLClsf) = k_label
            Next
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The indicators of the angajamente are the choices; a classification a line already has that is not one of them stays visible.
    Private Sub ApplyIndicators()
        _byLabel.Clear()
        _labelOf.Clear()
        Dim k_known As List(Of EFacturaKeReceptiiAngajament) = KnownData()
        If k_known.Count = 0 Then Return
        Dim k_choices As New List(Of Object) From {NeAleasa}
        For Each k_i As EFacturaKeIndicator In k_known.SelectMany(Function(k_d) k_d.Indicatori)
            If Not k_i.IdClsf.HasValue OrElse _labelOf.ContainsKey(k_i.IdClsf.Value) Then Continue For
            ' Only the classification code, no name (operator, 18.10.2026); two indicators with the same code get the id added so the labels stay distinct.
            Dim k_label As String = k_i.Clsf
            If _byLabel.ContainsKey(k_label) Then k_label = $"{k_i.Clsf} ({k_i.IdClsf.Value})"
            _byLabel(k_label) = k_i.IdClsf.Value
            _labelOf(k_i.IdClsf.Value) = k_label
            k_choices.Add(k_label)
        Next
        For Each k_id As Integer In _checked
            For Each k_l As EFacturaKeLinie In _states(k_id).Linii
                If k_l.IdClsf.HasValue AndAlso Not _labelOf.ContainsKey(k_l.IdClsf.Value) Then
                    Dim k_label As String = $"{k_l.Clsf} (nu e indicator al angajamentului)"
                    _byLabel(k_label) = k_l.IdClsf.Value
                    _labelOf(k_l.IdClsf.Value) = k_label
                    k_choices.Add(k_label)
                End If
            Next
        Next
        _filling = True
        Try
            gridLinii.Column(ColLClsf).ComboItems = k_choices
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_line As EFacturaKeLinie = CType(k_row.Tag, LineTag).Linie
                Dim k_current As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
                If String.Equals(k_current, NeAleasa, StringComparison.Ordinal) OrElse _byLabel.ContainsKey(k_current) Then Continue For
                ' a choice made from another angajament's list (or the raw text before any angajament) is not one of these
                k_row(ColLClsf) = If(k_line.IdClsf.HasValue AndAlso _labelOf.ContainsKey(k_line.IdClsf.Value), _labelOf(k_line.IdClsf.Value), NeAleasa)
            Next
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The classification the row stands for: the one picked from the list; before any angajament was opened the line keeps what it has.
    Private Function ChosenClass(k_row As KBotDataRow) As Integer?
        Dim k_line As EFacturaKeLinie = CType(k_row.Tag, LineTag).Linie
        Dim k_text As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
        Dim k_id As Integer = 0
        If Not String.IsNullOrWhiteSpace(k_text) AndAlso Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso
           _byLabel.TryGetValue(k_text, k_id) Then Return k_id
        If _byLabel.Count = 0 AndAlso k_line.IdClsf.HasValue AndAlso Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) Then Return k_line.IdClsf
        Return Nothing
    End Function

    Private Function RowsOf(k_idPrimita As Integer) As List(Of KBotDataRow)
        Return gridLinii.Rows.Where(Function(k_r) CType(k_r.Tag, LineTag).Invoice.IdPrimita = k_idPrimita).ToList()
    End Function

    ' Slice KE12-03: a line whose tick box is cleared takes no part in the reception (its «Recepționată» is 0 and cannot be typed).
    Private Shared Function IsIncluded(k_row As KBotDataRow) As Boolean
        Dim k_v As Object = k_row(KBotDataView.CheckColumnKey)
        Return Not (TypeOf k_v Is Boolean) OrElse CBool(k_v)
    End Function

    ' UI boundary: a line (or a whole invoice, through the tick box of its header) was ticked or cleared. Cleared: «Recepționată» goes to 0 and is locked; ticked again: it
    ' is unlocked and takes the sum from «Valoare cu TVA» again (cut, as always, to what the invoice / the reception still has).
    Private Sub GridLinii_RowChecked(sender As Object, e As KBotRowCheckedEventArgs) Handles gridLinii.RowChecked
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridLinii.RowCount Then Return
            CType(gridLinii.Rows(e.RowIndex).Tag, LineTag).Edited = False
            If e.Pending > 0 Then Return
            RecomputeDefaults()
            FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_RowChecked", ex)
        End Try
    End Sub

    ' A cleared line: its «Recepționată» is inert (not greyed; a click opens no editor).
    Private Sub GridLinii_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridLinii.CellFormatting
        Try
            If e.Row Is Nothing OrElse Not String.Equals(e.ColumnKey, ColLRec, StringComparison.Ordinal) Then Return
            If Not IsIncluded(e.Row) Then e.ReadOnlyCell = True
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_CellFormatting", ex)
        End Try
    End Sub

    ' Only a choice from the list is accepted in a cell; «Recepționată» takes a number of the line's sign, not above the line's value.
    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridLinii.CellValidating
        Try
            If e.ColumnKey = ColLClsf Then
                Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
                If String.IsNullOrWhiteSpace(k_text) Then
                    e.ProposedValue = NeAleasa
                ElseIf Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso Not _byLabel.ContainsKey(k_text) Then
                    e.Cancel = True
                End If
            ElseIf e.ColumnKey = ColLRec Then
                Dim k_number As Decimal
                If Not ReadNumber(e.ProposedValue, k_number) Then
                    ShowNotice("Suma recepționată nu este un număr.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
                Dim k_value As Decimal = CType(gridLinii.Rows(e.RowIndex).Tag, LineTag).Linie.ValoareCuTva
                If k_number <> 0D AndAlso (Math.Sign(k_number) <> Math.Sign(k_value) OrElse Math.Abs(k_number) > Math.Abs(k_value) + Tolerance) Then
                    ShowNotice($"Suma recepționată a liniei nu poate depăși valoarea ei ({Fmt(k_value)}) și are semnul facturii.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
                e.ProposedValue = CDbl(k_number)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    ' UI boundary: a classification picked in the header of an invoice goes on ALL its lines, whatever they had.
    Private Sub GridLinii_GroupHeaderValuePicked(sender As Object, e As KBotGroupHeaderValueEventArgs) Handles gridLinii.GroupHeaderValuePicked
        Try
            If Not String.Equals(e.ColumnKey, ColLClsf, StringComparison.Ordinal) Then Return
            Dim k_text As String = Convert.ToString(e.Value, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) Then Return
            _filling = True
            Try
                For Each k_index As Integer In e.RowIndexes
                    gridLinii.Rows(k_index)(ColLClsf) = k_text
                Next
            Finally
                _filling = False
            End Try
            gridLinii.Invalidate()
            Dim k_pickedId As Integer = 0
            If _byLabel.TryGetValue(k_text, k_pickedId) Then EnsureAngajamentFor(k_pickedId)
            RecomputeDefaults()
            FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_GroupHeaderValuePicked", ex)
        End Try
    End Sub

    ' A classification changed: the lines of the invoice below it that have none take it, and the amounts follow. An amount typed is kept from then on.
    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridLinii.CellValueChanged
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridLinii.RowCount Then Return
            If e.ColumnKey = ColLClsf Then
                FillDown(e.RowIndex)
                Dim k_picked As Integer? = ChosenClass(gridLinii.Rows(e.RowIndex))
                If k_picked.HasValue Then EnsureAngajamentFor(k_picked.Value)
            ElseIf e.ColumnKey = ColLRec Then
                CType(gridLinii.Rows(e.RowIndex).Tag, LineTag).Edited = True
            Else
                Return
            End If
            RecomputeDefaults()
            FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

    ' The choice of a line goes on the lines BELOW it, of the same invoice, that have no classification yet.
    Private Sub FillDown(k_index As Integer)
        Dim k_row As KBotDataRow = gridLinii.Rows(k_index)
        Dim k_text As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
        If String.IsNullOrWhiteSpace(k_text) OrElse String.Equals(k_text, NeAleasa, StringComparison.Ordinal) Then Return
        Dim k_id As Integer = CType(k_row.Tag, LineTag).Invoice.IdPrimita
        _filling = True
        Try
            For k_i As Integer = k_index + 1 To gridLinii.RowCount - 1
                Dim k_next As KBotDataRow = gridLinii.Rows(k_i)
                If CType(k_next.Tag, LineTag).Invoice.IdPrimita <> k_id Then Continue For
                Dim k_current As String = Convert.ToString(k_next(ColLClsf), CultureInfo.InvariantCulture)
                If String.IsNullOrWhiteSpace(k_current) OrElse String.Equals(k_current, NeAleasa, StringComparison.Ordinal) Then k_next(ColLClsf) = k_text
            Next
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' ── The amounts ─────────────────────────────────────────────────────────────

    ' How much of what is left a line takes: the whole value while there is enough, what is left when it is less, nothing when nothing is.
    Private Shared Function TakeFrom(k_value As Decimal, k_left As Decimal) As Decimal
        If k_left = 0D OrElse Math.Sign(k_left) <> Math.Sign(k_value) Then Return 0D
        Return If(Math.Abs(k_left) < Math.Abs(k_value), k_left, k_value)
    End Function

    ' The amounts proposed on the lines the accountant did not type: the value of the line, cut to what the invoice still has on its classification
    ' (partly linked before) and, for an existing reception, to what that reception still has. Typed amounts are taken out first.
    Private Sub RecomputeDefaults()
        Dim k_rec As EFacturaKeReceptie = If(_mode = EFacturaKeTip.Existenta, ChosenReceptie(), Nothing)
        Dim k_recLeft As New Dictionary(Of Integer, Decimal)()
        If k_rec IsNot Nothing Then
            For Each k_pc As EFacturaKeReceptieClasificatie In k_rec.PeClasificatie
                k_recLeft(k_pc.IdClsf) = k_pc.Rest
            Next
        End If
        _filling = True
        gridLinii.BeginUpdate()
        Try
            For Each k_id As Integer In _checked
                Dim k_rows As List(Of KBotDataRow) = RowsOf(k_id)
                Dim k_left As New Dictionary(Of Integer, Decimal)()
                For Each k_row As KBotDataRow In k_rows
                    Dim k_cls As Integer? = ChosenClass(k_row)
                    If Not k_cls.HasValue Then Continue For
                    k_left(k_cls.Value) = k_left.GetValueOrDefault(k_cls.Value) + CType(k_row.Tag, LineTag).Linie.ValoareCuTva
                Next
                For Each k_l As EFacturaKeLegatura In _states(k_id).Legaturi
                    If k_left.ContainsKey(k_l.IdClsf) Then k_left(k_l.IdClsf) -= k_l.Suma
                Next
                For Each k_row As KBotDataRow In k_rows.Where(Function(k_r) CType(k_r.Tag, LineTag).Edited AndAlso IsIncluded(k_r))
                    Dim k_cls As Integer? = ChosenClass(k_row)
                    If Not k_cls.HasValue Then Continue For
                    Dim k_typed As Decimal = 0D
                    ReadNumber(k_row(ColLRec), k_typed)
                    k_left(k_cls.Value) -= k_typed
                    If k_recLeft.ContainsKey(k_cls.Value) Then k_recLeft(k_cls.Value) -= k_typed
                Next
                For Each k_row As KBotDataRow In k_rows.Where(Function(k_r) Not CType(k_r.Tag, LineTag).Edited OrElse Not IsIncluded(k_r))
                    If Not IsIncluded(k_row) Then
                        k_row(ColLRec) = 0.0R
                        Continue For
                    End If
                    Dim k_value As Decimal = CType(k_row.Tag, LineTag).Linie.ValoareCuTva
                    Dim k_cls As Integer? = ChosenClass(k_row)
                    Dim k_take As Decimal = k_value
                    If k_cls.HasValue Then
                        k_take = TakeFrom(k_value, k_left(k_cls.Value))
                        If k_rec IsNot Nothing Then k_take = TakeFrom(k_take, k_recLeft.GetValueOrDefault(k_cls.Value))
                        k_left(k_cls.Value) -= k_take
                        If k_rec IsNot Nothing Then k_recLeft(k_cls.Value) = k_recLeft.GetValueOrDefault(k_cls.Value) - k_take
                    End If
                    k_row(ColLRec) = CDbl(k_take)
                Next
            Next
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The sum of the amounts of the lines of one invoice, per classification (zero ones left out).
    Private Function AmountsOf(k_idPrimita As Integer) As Dictionary(Of Integer, Decimal)
        Dim k_sums As New Dictionary(Of Integer, Decimal)()
        For Each k_row As KBotDataRow In RowsOf(k_idPrimita)
            If Not IsIncluded(k_row) Then Continue For
            Dim k_cls As Integer? = ChosenClass(k_row)
            Dim k_number As Decimal = 0D
            If Not k_cls.HasValue OrElse Not ReadNumber(k_row(ColLRec), k_number) Then Continue For
            k_sums(k_cls.Value) = k_sums.GetValueOrDefault(k_cls.Value) + k_number
        Next
        Return k_sums.Where(Function(k_p) Math.Abs(k_p.Value) >= Tolerance).ToDictionary(Function(k_p) k_p.Key, Function(k_p) Math.Round(k_p.Value, 2))
    End Function

    ' One row per classification of the ticked invoices: Buget, Receptii (before the day), Disponibil, Valoare (the sum of «Recepționată») and Ramas.
    Private Sub FillAlocari()
        _filling = True
        gridAloc.BeginUpdate()
        Try
            gridAloc.ClearRows()
            Dim k_valRecep As New Dictionary(Of Integer, Decimal)()
            Dim k_name As New Dictionary(Of Integer, String)()
            For Each k_row As KBotDataRow In gridLinii.Rows
                If Not IsIncluded(k_row) Then Continue For
                Dim k_cls As Integer? = ChosenClass(k_row)
                If Not k_cls.HasValue Then Continue For
                Dim k_line As EFacturaKeLinie = CType(k_row.Tag, LineTag).Linie
                Dim k_number As Decimal = 0D
                ReadNumber(k_row(ColLRec), k_number)
                k_valRecep(k_cls.Value) = k_valRecep.GetValueOrDefault(k_cls.Value) + k_number
                If Not k_name.ContainsKey(k_cls.Value) Then
                    k_name(k_cls.Value) = If(_labelOf.ContainsKey(k_cls.Value), _labelOf(k_cls.Value), k_line.Clsf)
                End If
            Next
            Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
            For Each k_pair As KeyValuePair(Of Integer, String) In k_name.OrderBy(Function(k_p) k_p.Value, StringComparer.CurrentCultureIgnoreCase)
                Dim k_row As KBotDataRow = gridAloc.AddRow()
                Dim k_label As String = k_pair.Value
                If _data IsNot Nothing AndAlso IndicatorOf(k_pair.Key) Is Nothing Then k_label &= "  (nu e indicator al angajamentului)"
                k_row(ColAClsf) = k_label
                ' As in the fundamentation document: Buget (of the classification on the day), Receptii (before the day), Disponibil = Buget - Receptii,
                ' Valoare (this reception) and Ramas = Disponibil - Valoare.
                Dim k_d As EFacturaKeDisponibil = DispoOf(k_pair.Key)
                If k_d IsNot Nothing Then
                    k_row(ColAReceptii) = CDbl(k_d.Receptii)
                    If k_d.Buget.HasValue Then
                        Dim k_available As Decimal = k_d.Buget.Value - k_d.Receptii
                        k_row(ColABuget) = CDbl(k_d.Buget.Value)
                        k_row(ColADisponibil) = CDbl(k_available)
                        k_row(ColARamas) = CDbl(k_available - k_valRecep(k_pair.Key))
                    End If
                End If
                k_row(ColAValRecep) = CDbl(k_valRecep(k_pair.Key))
                If k_rec IsNot Nothing Then
                    Dim k_pc As EFacturaKeReceptieClasificatie = k_rec.PeClasificatie.FirstOrDefault(Function(k_x) k_x.IdClsf = k_pair.Key)
                    k_row(ColARestRecep) = If(k_pc IsNot Nothing, CDbl(k_pc.Rest), 0.0R)
                End If
            Next
            ' Where the chosen reception still has rest but no line stands: shown too, so «Rest recepție existentă» of the lines (0.00) is not a riddle (operator, 19.10.2026).
            If k_rec IsNot Nothing Then
                For Each k_other As EFacturaKeReceptieClasificatie In k_rec.PeClasificatie.Where(
                        Function(k_x) Math.Abs(k_x.Rest) >= Tolerance AndAlso Not k_name.ContainsKey(k_x.IdClsf)).OrderBy(Function(k_x) k_x.Clsf, StringComparer.Ordinal)
                    Dim k_extra As KBotDataRow = gridAloc.AddRow()
                    k_extra(ColAClsf) = $"{If(_labelOf.ContainsKey(k_other.IdClsf), _labelOf(k_other.IdClsf), k_other.Clsf)}  (recepția are rest aici; nicio linie nu e pe ea)"
                    k_extra(ColARestRecep) = CDbl(k_other.Rest)
                Next
            End If
        Finally
            gridAloc.EndUpdate()
            _filling = False
        End Try
        RefreshOk()
    End Sub

    ' A number as the grids show it: the operator's own regional format (a dot or a comma for the decimals), the same one the cells are painted and typed in.
    Private Shared Function Fmt(k_value As Decimal) As String
        Return k_value.ToString("N2", CultureInfo.CurrentCulture)
    End Function

    ' A typed amount must be a number in the operator's own regional format (the cells are painted in it, so «77.49» is 77.49 for a dot user, not 7749); empty = 0.
    Private Shared Function ReadNumber(k_value As Object, ByRef k_number As Decimal) As Boolean
        If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then
            k_number = 0D
            Return True
        End If
        If TypeOf k_value Is Double Then
            k_number = Math.Round(CDec(DirectCast(k_value, Double)), 2)
            Return True
        End If
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.CurrentCulture).Trim()
        If k_text.Length = 0 Then
            k_number = 0D
            Return True
        End If
        Dim k_parsed As Decimal
        If Decimal.TryParse(k_text, NumberStyles.Number Or NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, k_parsed) Then
            k_number = Math.Round(k_parsed, 2)
            Return True
        End If
        Return False
    End Function

    ' An existing reception takes only what it still has on each classification: the sum over all the ticked invoices is checked here.
    ' k_fromLineValues = nothing is typed or proposed (every amount was cut to 0 because the reception has no rest on the classification of the lines): the check is then
    ' made with what the lines are worth (less what the invoice already has linked), so the operator is told why «Leagă» stays disabled instead of seeing nothing.
    Private Function ReceptionFitProblem(Optional k_fromLineValues As Boolean = False) As String
        If _mode <> EFacturaKeTip.Existenta Then Return Nothing
        Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
        If k_rec Is Nothing Then Return Nothing
        Dim k_total As New Dictionary(Of Integer, Decimal)()
        For Each k_id As Integer In _checked
            If k_fromLineValues Then
                Dim k_wanted As New Dictionary(Of Integer, Decimal)()
                For Each k_row As KBotDataRow In RowsOf(k_id)
                    Dim k_cls As Integer? = ChosenClass(k_row)
                    If Not IsIncluded(k_row) OrElse Not k_cls.HasValue Then Continue For
                    k_wanted(k_cls.Value) = k_wanted.GetValueOrDefault(k_cls.Value) + CType(k_row.Tag, LineTag).Linie.ValoareCuTva
                Next
                For Each k_l As EFacturaKeLegatura In _states(k_id).Legaturi
                    If k_wanted.ContainsKey(k_l.IdClsf) Then k_wanted(k_l.IdClsf) -= k_l.Suma
                Next
                For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_wanted
                    If Math.Abs(k_pair.Value) >= Tolerance Then k_total(k_pair.Key) = k_total.GetValueOrDefault(k_pair.Key) + k_pair.Value
                Next
            Else
                For Each k_pair As KeyValuePair(Of Integer, Decimal) In AmountsOf(k_id)
                    k_total(k_pair.Key) = k_total.GetValueOrDefault(k_pair.Key) + k_pair.Value
                Next
            End If
        Next
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_total
            Dim k_pc As EFacturaKeReceptieClasificatie = k_rec.PeClasificatie.FirstOrDefault(Function(k_x) k_x.IdClsf = k_pair.Key)
            Dim k_left As Decimal = If(k_pc IsNot Nothing, k_pc.Rest, 0D)
            If Math.Sign(k_left) <> Math.Sign(k_pair.Value) OrElse Math.Abs(k_pair.Value) > Math.Abs(k_left) + Tolerance Then
                Dim k_label As String = If(_labelOf.ContainsKey(k_pair.Key), _labelOf(k_pair.Key), k_pair.Key.ToString(CultureInfo.InvariantCulture))
                ' Say where the reception DOES have rest, so a sum in «Rest nealocat» of the list is not a mystery when this classification has none.
                Dim k_elsewhere As List(Of String) = k_rec.PeClasificatie.Where(Function(k_x) k_x.IdClsf <> k_pair.Key AndAlso Math.Abs(k_x.Rest) >= Tolerance).
                    Select(Function(k_x) $"{If(_labelOf.ContainsKey(k_x.IdClsf), _labelOf(k_x.IdClsf), k_x.Clsf)} ({Fmt(k_x.Rest)})").ToList()
                Dim k_hint As String = If(k_elsewhere.Count > 0, $" Recepția are rest pe: {String.Join("; ", k_elsewhere)}. Alegeți clasificația liniilor în acord cu ea sau o altă recepție.",
                                          " Scădeți «Recepționată», modificați recepția sau faceți una nouă.")
                Return $"Recepția aleasă nu are destul rest pe {k_label}: rest {Fmt(k_left)}, de alocat {Fmt(k_pair.Value)}." & k_hint
            End If
        Next
        Return Nothing
    End Function

    ' What FOREXE checks when a reception is saved: on every indicator the sum received (this reception, all the ticked invoices) must fit in the Disponibil
    ' (the budget of the classification on the day of the reception minus the receptions before that day, as in the fundamentation document). Not for a link to an existing reception: its value does not change. A storno (a negative sum) is not checked.
    Private Function AvailabilityProblem() As String
        If _data Is Nothing OrElse _mode = EFacturaKeTip.Existenta Then Return Nothing
        Dim k_total As New Dictionary(Of Integer, Decimal)()
        For Each k_id As Integer In _checked
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In AmountsOf(k_id)
                k_total(k_pair.Key) = k_total.GetValueOrDefault(k_pair.Key) + k_pair.Value
            Next
        Next
        For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_total
            If k_pair.Value <= Tolerance Then Continue For
            Dim k_d As EFacturaKeDisponibil = DispoOf(k_pair.Key)
            If k_d Is Nothing OrElse Not k_d.Buget.HasValue Then Continue For   ' not an indicator of the angajament (the button says so), or the budget is not known yet
            Dim k_ind As EFacturaKeIndicator = IndicatorOf(k_pair.Key)
            Dim k_available As Decimal = k_d.Buget.Value - k_d.Receptii
            If k_pair.Value > k_available + Tolerance Then
                Return $"Nu există rezervare suficientă pe {If(k_ind IsNot Nothing, k_ind.Clsf, k_pair.Key.ToString(CultureInfo.InvariantCulture))}: disponibil {Fmt(k_available)}, " &
                       $"de recepționat {Fmt(k_pair.Value)}. Scădeți «Recepționată» sau faceți întâi o revizie de rezervare (documentul de fundamentare)."
            End If
        Next
        Return Nothing
    End Function

    Private Sub RefreshOk()
        If _viewing IsNot Nothing Then
            btnElimina.Enabled = _viewState IsNot Nothing AndAlso _viewState.PoateScrie AndAlso Not _busy
            Return
        End If
        Dim k_any As Boolean = _checked.Any(Function(k_id) AmountsOf(k_id).Count > 0)
        Dim k_ok As Boolean = k_any AndAlso _data IsNot Nothing AndAlso _checked.All(Function(k_id) _states(k_id).PoateScrie)
        If _mode <> EFacturaKeTip.Noua Then k_ok = k_ok AndAlso ChosenReceptie() IsNot Nothing
        Dim k_kind As NoticeKind = NoticeKind.Error
        Dim k_problem As String = AvailabilityProblem()
        If k_problem Is Nothing AndAlso k_ok Then
            k_problem = ReceptionFitProblem()
            k_kind = NoticeKind.Warning
        ElseIf k_problem Is Nothing AndAlso Not k_any AndAlso _mode = EFacturaKeTip.Existenta AndAlso _data IsNot Nothing Then
            ' Every sum was cut to 0 (the reception has nothing on the classification of the lines): say it, do not just leave «Leagă» grey.
            k_problem = ReceptionFitProblem(True)
            k_kind = NoticeKind.Warning
        End If
        If k_problem IsNot Nothing Then
            k_ok = False
            ShowNotice(k_problem, k_kind)
            _fitShown = True
        ElseIf _dispoWarning.Length > 0 AndAlso _mode <> EFacturaKeTip.Existenta Then
            ShowNotice(_dispoWarning, NoticeKind.Warning)
            _fitShown = True
        ElseIf _fitShown Then
            ntfMesaj.Visible = False
            _fitShown = False
        End If
        btnOk.Enabled = k_ok
    End Sub

    Private Sub Notify(k_text As String, k_icon As MessageBoxIcon)
        KBotMessage.Show(Me, k_text, "Recepții", MessageBoxButtons.OK, k_icon)
    End Sub

    ' While the work runs nothing can be changed and nothing is sent twice.
    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        tlyMain.Enabled = Not k_busy
        pnlMod.Enabled = Not k_busy
        If k_busy Then btnOk.Enabled = False Else RefreshOk()
    End Sub

    ' The request of every ticked invoice that has an amount, or Nothing (a notice says why).
    Private Function BuildRequests() As List(Of KeLegaFactura)
        Dim k_facturi As New List(Of KeLegaFactura)()
        For Each k_id As Integer In _checked
            Dim k_amounts As Dictionary(Of Integer, Decimal) = AmountsOf(k_id)
            If k_amounts.Count = 0 Then Continue For
            ' A classification that is not an indicator of the angajament cannot be received: the law says the indicator comes first.
            If k_amounts.Keys.Any(Function(k_c) IndicatorOf(k_c) Is Nothing) Then
                ShowNotice("O clasificație nu este indicator al angajamentului: este nevoie întâi de o revizie nouă de rezervare " &
                           "(documentul de fundamentare), apoi de recepție.", NoticeKind.Error)
                Return Nothing
            End If
            Dim k_p As EFacturaPrimita = _invoices(k_id)
            Dim k_one As New KeLegaFactura() With {.Factura = k_p, .Eticheta = $"{k_p.NrFact} ({k_p.Furnizor})".Trim()}
            Dim k_cerere As New EFacturaKeCerere() With {.CodAngajament = _angCod, .Tip = _mode}
            Dim k_rec As EFacturaKeReceptie = ChosenReceptie()
            If k_rec IsNot Nothing Then k_cerere.Idrr = k_rec.Idrr
            For Each k_pair As KeyValuePair(Of Integer, Decimal) In k_amounts
                k_cerere.Alocari.Add(New EFacturaKeAlocare() With {.IdClsf = k_pair.Key, .Suma = k_pair.Value})
            Next
            k_one.Cerere = k_cerere
            For Each k_row As KBotDataRow In RowsOf(k_id)
                Dim k_line As EFacturaKeLinie = CType(k_row.Tag, LineTag).Linie
                Dim k_cls As Integer? = ChosenClass(k_row)
                If Not Nullable.Equals(k_cls, k_line.IdClsf) Then k_one.ClasificariSchimbate = True
                k_one.Clasificari.Add(New EFacturaKeLinie() With {.IdLinie = k_line.IdLinie, .IdClsf = k_cls})
            Next
            k_facturi.Add(k_one)
        Next
        Return k_facturi
    End Function

    ' UI boundary (async Sub): the button. Logs and swallows; what the operator must know is said in a box.
    Private Async Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            If _busy OrElse _data Is Nothing OrElse String.IsNullOrWhiteSpace(_angCod) Then Return
            If _mode <> EFacturaKeTip.Existenta AndAlso KeRobot.Current Is Nothing Then
                Notify("Robotul FOREXE nu este disponibil, deci nu se poate face sau modifica o recepție. Puteți lega doar de o recepție existentă.", MessageBoxIcon.Information)
                Return
            End If
            Dim k_facturi As List(Of KeLegaFactura) = BuildRequests()
            If k_facturi Is Nothing OrElse k_facturi.Count = 0 Then Return
            If _mode = EFacturaKeTip.Noua AndAlso (cmbTip.SelectedIndex < 0 OrElse String.IsNullOrWhiteSpace(txtDescriere.Text)) Then
                ShowNotice("Alegeți tipul și scrieți descrierea recepției.", NoticeKind.Warning)
                Return
            End If
            ' Optional (Setari): one reception per day on an angajament. K-BOT's own receptions carry their numbers, so this is only a wish.
            If _mode = EFacturaKeTip.Noua AndAlso AppSettings.Current.KeBlockSameDayReception Then
                Dim k_day As Date = dtpData.Value.Date
                If _data.Receptii.Any(Function(k_x) k_x.DataR.HasValue AndAlso k_x.DataR.Value.Date = k_day) Then
                    ShowNotice($"Angajamentul are deja o recepție pe {k_day:dd.MM.yyyy}. Modificați-o (alegeți «Modifică o recepție existentă») sau alegeți altă dată.",
                               NoticeKind.Warning)
                    Return
                End If
            End If
            ntfMesaj.Visible = False
            SetBusy(True)
            Try
                Dim k_description As String = System.Text.RegularExpressions.Regex.Replace(txtDescriere.Text, "\s*[\r\n]+\s*", " ").Trim()
                Dim k_done As String = Await CommitAsync(k_facturi, Convert.ToString(cmbTip.SelectedItem, CultureInfo.InvariantCulture),
                                                         dtpData.Value.Date, k_description, ChosenReceptie()).ConfigureAwait(True)
                If k_done IsNot Nothing Then
                    If IsDisposed Then Return
                    ' The work is done: the invoices just covered leave the tree (the rest of it stays as it was), the receptions are read again.
                    txtDescriere.Text = String.Empty
                    _descrAuto = String.Empty
                    _dataAuto = Date.MinValue
                    If _embedded Then
                        ' The host reads the invoice again and gives it back (ShowInvoice); the message of success waits for that.
                        _pendingNotice = k_done
                        RaiseEvent Committed(Me, EventArgs.Empty)
                    Else
                        Await ReloadAsync(True).ConfigureAwait(True)
                        If Not IsDisposed AndAlso k_done.Length > 0 Then ShowNotice(k_done, NoticeKind.Success)
                    End If
                End If
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.BtnOk_Click", ex)
            Notify(ex.Message, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BtnOk_Click", ex)
            Notify("Legarea facturilor de recepție nu s-a putut face. Detalii în jurnalul de erori.", MessageBoxIcon.Error)
        End Try
    End Sub

    ' ── The work (slices KE05..KE07, KE10, moved here by KE12) ──────────────────

    ' The numbers the K-BOT marker carries: written in the description of the reception on the site, so the download that follows knows whose it is.
    Private NotInheritable Class KeMarker
        Public Property Text As String = String.Empty
        Public Property Idrh As Integer
        Public Property Idr As Integer
    End Class

    Private Shared ReadOnly _markerPattern As New System.Text.RegularExpressions.Regex("\(IDRH:\s*(\d+);\s*IDR:\s*(\d+)\)", System.Text.RegularExpressions.RegexOptions.CultureInvariant)

    Private Async Function ReserveMarkerAsync(k_cod As String) As Task(Of KeMarker)
        Dim k_marker As IMarcajApi = TryCast(_api, IMarcajApi)
        If k_marker Is Nothing Then Throw New InvalidOperationException("The API client does not implement IMarcajApi.")
        Dim k_text As String = Await _gate.RunAsync(Function() k_marker.RezervaMarcajAsync("receptie", k_cod, _cts.Token)).ConfigureAwait(True)
        Dim k_match As System.Text.RegularExpressions.Match = _markerPattern.Match(k_text)
        If Not k_match.Success Then Throw New ApiException("Serverul a trimis un marcaj necunoscut pentru recepție.")
        Return New KeMarker() With {.Text = k_text, .Idrh = Integer.Parse(k_match.Groups(1).Value, CultureInfo.InvariantCulture),
                                    .Idr = Integer.Parse(k_match.Groups(2).Value, CultureInfo.InvariantCulture)}
    End Function

    ' The pattern of the marker numbers K-BOT may have written in the description of a reception; empty when it never wrote one.
    ' No colon in it: the workflow's IfVar splits its comparison on colons, and the dot stands for the one after «IDRH».
    Private Shared Function MarkerRegex(k_rec As EFacturaKeReceptie) As String
        If k_rec.Idrhs.Count = 0 Then Return String.Empty
        Return "IDRH.\s*(" & String.Join("|", k_rec.Idrhs.Select(Function(k_n) k_n.ToString(CultureInfo.InvariantCulture))) & ")\b"
    End Function

    ' The rows of several checks as the rows of one reception: the amounts of the same indicator are added up. For a reception that is MODIFIED the value
    ' written is its value now plus all the amounts (the server answers the value now of each indicator with every check).
    Private Shared Function SumRows(k_checks As IEnumerable(Of EFacturaKeVerificare), k_modify As Boolean) As List(Of KeReceptieRand)
        Dim k_rows As New List(Of KeReceptieRand)()
        For Each k_check As EFacturaKeVerificare In k_checks
            For Each k_r As EFacturaKeRand In k_check.Randuri
                Dim k_same As KeReceptieRand = k_rows.FirstOrDefault(
                    Function(k_x) String.Equals(k_x.ClsfSal, k_r.ClsfSal, StringComparison.Ordinal) AndAlso
                                  String.Equals(k_x.CodIndicator, k_r.CodIndicator, StringComparison.Ordinal))
                If k_same Is Nothing Then
                    Dim k_start As Decimal = k_r.Suma
                    If k_modify Then k_start = If(k_r.ValoareVeche.HasValue, k_r.ValoareVeche.Value + k_r.Suma, If(k_r.ValoareNoua, k_r.Suma))
                    k_rows.Add(New KeReceptieRand() With {.ClsfSal = k_r.ClsfSal, .Clsf = k_r.Clsf, .CodIndicator = k_r.CodIndicator, .Valoare = k_start})
                Else
                    k_same.Valoare += k_r.Suma
                End If
            Next
        Next
        Return k_rows
    End Function

    ''' <summary>
    ''' Order, so that nothing reaches FOREXE before every check passed: (1) the changed choices of the lines are saved and the server checks every invoice
    ''' (a problem stops everything); (2) new / modified: ONE reception in FOREXE with the rows of all the invoices added up per indicator and ONE marker;
    ''' (3) only then every invoice gets its link (the same IDRH / IDR for a new reception). Nothing = it stopped before writing links (the box said why); empty = done (a box already told what went wrong); else the sentence of success.
    ''' </summary>
    Private Async Function CommitAsync(k_facturi As List(Of KeLegaFactura), k_tipReceptie As String, k_dataReceptie As Date,
                                       k_descriere As String, k_receptie As EFacturaKeReceptie) As Task(Of String)
        Dim k_tip As String = _mode
        Dim k_cod As String = _angCod
        Dim k_warnings As New List(Of String)()
        For Each k_one As KeLegaFactura In k_facturi
            Dim k_id As Integer = k_one.Factura.IdPrimita
            If k_one.ClasificariSchimbate Then
                Dim k_chosen As List(Of EFacturaKeLinie) = k_one.Clasificari.ToList()
                Await _gate.RunAsync(Function() _api.SetKeClasificatiiAsync(k_id, k_chosen, _cts.Token)).ConfigureAwait(True)
            End If
            Dim k_request As EFacturaKeCerere = k_one.Cerere
            k_one.Verificare = Await _gate.RunAsync(Function() _api.VerifyKeAsync(k_id, k_request, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return Nothing
            If Not k_one.Verificare.Ok Then
                Notify($"Factura {k_one.Eticheta}:" & Environment.NewLine & String.Join(Environment.NewLine, k_one.Verificare.Probleme), MessageBoxIcon.Warning)
                Return Nothing
            End If
            k_warnings.AddRange(k_one.Verificare.Avertismente)
        Next
        If k_warnings.Count > 0 AndAlso KBotMessage.Show(Me,
                String.Join(Environment.NewLine, k_warnings.Distinct()) & Environment.NewLine & Environment.NewLine & "Continuați?",
                "Recepții", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return Nothing

        Dim k_marker As KeMarker = Nothing
        Dim k_saved As KeReceptieSalvata = Nothing
        Dim k_label As String = If(k_facturi.Count = 1, k_facturi(0).Eticheta, $"{k_facturi.Count} facturi")
        If k_tip = EFacturaKeTip.Noua Then
            k_marker = Await ReserveMarkerAsync(k_cod).ConfigureAwait(True)
            Dim k_job As New KeReceptieDeSalvat() With {
                .CodAngajament = k_cod, .Modifica = False, .TipReceptie = k_tipReceptie, .Data = k_dataReceptie,
                .Descriere = (k_descriere & " " & k_marker.Text).Trim(), .Marcaj = k_marker.Text, .Eticheta = k_label}
            k_job.Randuri.AddRange(SumRows(k_facturi.Select(Function(k_x) k_x.Verificare), False))
            k_saved = Await KeRobot.Current.SalveazaReceptieAsync(k_job, _cts.Token).ConfigureAwait(True)
        ElseIf k_tip = EFacturaKeTip.Modificata Then
            If k_receptie Is Nothing OrElse Not k_receptie.DataR.HasValue Then
                Notify("Recepția aleasă nu are dată; nu poate fi găsită în FOREXE.", MessageBoxIcon.Warning)
                Return Nothing
            End If
            k_marker = Await ReserveMarkerAsync(k_cod).ConfigureAwait(True)
            Dim k_job As New KeReceptieDeSalvat() With {
                .CodAngajament = k_cod, .Modifica = True, .Marcaj = k_marker.Text, .DataTinta = k_receptie.DataR, .SumaTinta = k_receptie.Total,
                .MarcajRegex = MarkerRegex(k_receptie), .Eticheta = k_label}
            k_job.Randuri.AddRange(SumRows(k_facturi.Select(Function(k_x) k_x.Verificare), True))
            k_saved = Await KeRobot.Current.SalveazaReceptieAsync(k_job, _cts.Token).ConfigureAwait(True)
        End If
        If IsDisposed Then Return Nothing
        If k_tip <> EFacturaKeTip.Existenta AndAlso (k_saved Is Nothing OrElse Not k_saved.Salvata) Then
            Notify(If(k_saved Is Nothing, If(k_tip = EFacturaKeTip.Noua, "Recepția nu s-a putut salva în FOREXE.", "Recepția nu s-a putut modifica în FOREXE."), k_saved.Mesaj),
                   MessageBoxIcon.Warning)
            Return Nothing
        End If

        ' The links: one that fails does not stop the others; the reception exists in FOREXE, so the rest is told.
        Dim k_failed As New List(Of String)()
        For Each k_one As KeLegaFactura In k_facturi
            Try
                Dim k_id As Integer = k_one.Factura.IdPrimita
                Dim k_request As EFacturaKeCerere = k_one.Cerere
                If k_tip = EFacturaKeTip.Noua Then
                    k_request.Idrh = k_marker.Idrh
                    k_request.Idr = k_marker.Idr
                End If
                Await _gate.RunAsync(Function() _api.AddKeLegaturiAsync(k_id, k_request, _cts.Token)).ConfigureAwait(True)
            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                GlobalErrorLog.Write("KeLegaForm.CommitAsync", ex)
                k_failed.Add($"{k_one.Eticheta}: {ex.Message}")
            End Try
        Next
        If IsDisposed Then Return Nothing
        If k_failed.Count > 0 Then
            Notify(If(k_tip = EFacturaKeTip.Existenta, "Legătura nu s-a scris pentru:", "Recepția este în FOREXE, dar legătura nu s-a scris pentru:") &
                   Environment.NewLine & String.Join(Environment.NewLine, k_failed) &
                   Environment.NewLine & Environment.NewLine & "Legați acele facturi de recepția existentă (ultima recepție a angajamentului).", MessageBoxIcon.Warning)
        ElseIf k_saved IsNot Nothing AndAlso Not k_saved.Preluata Then
            Notify(If(k_tip = EFacturaKeTip.Noua,
                      "Recepția este salvată în FOREXE, dar K-BOT nu a citit-o încă. Legăturile așteaptă descărcarea angajamentului.",
                      "Recepția este modificată în FOREXE, dar K-BOT nu a citit-o încă. Descărcați angajamentul ca să vedeți valoarea nouă."), MessageBoxIcon.Information)
            Return String.Empty
        End If
        Return If(k_facturi.Count = 1, "Factura a fost legată de recepție.", $"{k_facturi.Count} facturi au fost legate de recepție.")
    End Function

End Class
