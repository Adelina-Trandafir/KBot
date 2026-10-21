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
''' Slice 00EF-48 -- the page «Trimite la Vercon» of the E-Factura ribbon: the accountant sends received invoices to VERCON, where the Administrator takes them over in «Adaugare NIR».
''' It opens from the ribbon like «Cumpărare» and «Recepții» and stays open. On the left, the received invoices of the year, a tick each, with what became of each in VERCON (sent and
''' waiting, taken over in a NIR, sent back with a reason). On the right, the lines of the ticked invoices, grouped by invoice: ALL the lines of an invoice are sent (nothing is ticked
''' per line); on each line she can PROPOSE the account of VERCON it goes to, from a tree (gestiune, then its accounts, whichever subunit), or leave it to the Administrator
''' («fără cont»). The account picked on the header of an invoice is written on all its lines. The account is a proposal: the Administrator can change it.
''' One ticked invoice that was sent and still waits opens in the mode that changes its proposed accounts or takes it back. Nothing is saved until a button is pressed.
''' The rules (who may write, what can be sent, when it can be taken back) are the server's (PYTHON/routes/efactura/primite_vercon.py). Plan: docs/EV_PLAN.md.
''' </summary>
Public Class VerconForm

    Private Const OptPeFurnizor As String = "grup_furnizor"
    Private Const OptPeLuna As String = "grup_luna"
    Private Const ColFactura As String = "factura"
    Private Const ColNr As String = "nr"
    Private Const ColDenumire As String = "denumire"
    Private Const ColCant As String = "cant"
    Private Const ColCuTva As String = "cuTva"
    Private Const ColCont As String = "cont"
    Private Const FaraCont As String = "(fără cont)"
    Private Const FaraContGrup As String = "Fără cont"
    Private Const NotaInitiala As String = "Bifează facturile de trimis (sau o lună, sau un furnizor întreg): merg la Vercon cu TOATE liniile lor. Pe fiecare linie poți propune contul din Vercon; " &
                                           "ce alegi pe rândul unei facturi se scrie pe toate liniile ei. O linie fără cont rămâne pentru Administrator. Contul este doar o propunere. " &
                                           "Nu se trimite nimic până nu apeși butonul."
    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    ' What a row of the lines grid stands for: the invoice and the line.
    Private NotInheritable Class LineTag
        Public Property IdPrimita As Integer
        Public Property IdLinie As Integer
    End Class

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _year As Integer
    Private _invoices As New List(Of EFacturaPrimita)()
    ' The leaf of each invoice in the tree, and the invoices in the order the tree shows them (the order of the groups in the lines grid).
    Private ReadOnly _nodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
    Private ReadOnly _order As New List(Of Integer)()
    ' The tree groups by month and supplier (the usual one) or by supplier alone.
    Private _groupBySupplier As Boolean
    ' What became of each invoice that was ever sent ({IdPrimita -> its short line}).
    Private _vercon As New Dictionary(Of Integer, EFacturaVerconSumar)()
    ' The state of each ticked invoice as the server gave it, kept while the page is open.
    Private ReadOnly _states As New Dictionary(Of Integer, EFacturaVerconStareFactura)()
    ' Face text of an account -> IdMasina, and back; the group (header) of an account.
    Private ReadOnly _idByLabel As New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)
    Private ReadOnly _labelById As New Dictionary(Of Integer, String)()
    Private ReadOnly _groupById As New Dictionary(Of Integer, String)()
    ' What the accountant picked for a line ({IdLinie -> face text}), kept when the lines grid is filled again.
    Private ReadOnly _choices As New Dictionary(Of Integer, String)()
    Private _items As New List(Of Object)()
    Private _modify As Boolean
    Private _wasModify As Boolean
    Private _canWrite As Boolean = True
    Private _filling As Boolean
    Private _suppress As Boolean
    Private _busy As Boolean
    Private _seq As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_api">The E-Factura routes; the shell passes its API client.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_unitName">The open unit's name, shown in the title.</param>
    ''' <param name="k_year">The year chosen in K-BOT; only the invoices dated in it are listed (0 = all years).</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_unitName As String, k_year As Integer)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _year = k_year
        capBar.Text = "Trimite la Vercon" & If(k_year > 0, $" {k_year}", String.Empty) &
                      If(String.IsNullOrWhiteSpace(k_unitName), String.Empty, " — " & k_unitName)
        lblAntet.Text = NotaInitiala
        AddHandler gridLinii.CellValidating, AddressOf GridLinii_CellValidating
        AddHandler gridLinii.CellValueChanged, AddressOf GridLinii_CellValueChanged
        AddHandler gridLinii.GroupHeaderValuePicked, AddressOf GridLinii_GroupHeaderValuePicked
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice. The invoices are shown even when the VERCON routes do not answer.
    Private Async Sub VerconForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            SetBusy(True)
            Dim k_year As Integer? = If(_year > 0, CType(_year, Integer?), Nothing)
            Dim k_list As List(Of EFacturaPrimita) = Await _gate.RunAsync(
                Function() _api.GetPrimiteAsync(k_year, Nothing, Nothing, Nothing, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            ' A credit note (storno) is not sent, and an invoice with no value has nothing to take over.
            _invoices = k_list.Where(Function(k_p) Not String.Equals(k_p.Tip, "NC", StringComparison.Ordinal) AndAlso Math.Abs(k_p.Total) >= 0.01D).
                OrderByDescending(Function(k_p) k_p.DataFact).ThenByDescending(Function(k_p) k_p.IdPrimita).ToList()
            Await ReadVerconAsync().ConfigureAwait(True)
            If IsDisposed Then Return
            BuildTree(Nothing)
            If _invoices.Count = 0 Then ShowNotice("Nu există facturi primite de trimis în această perioadă.", NoticeKind.Warning)
            Dim k_tree As List(Of EFacturaVerconGestiune) = Await _gate.RunAsync(Function() _api.GetVerconConturiAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            BuildChoices(k_tree)
            SetBusy(False)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("VerconForm.VerconForm_Load", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
            SetBusy(False)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.VerconForm_Load", ex)
            ShowNotice("Datele pentru Vercon nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
            SetBusy(False)
        End Try
    End Sub

    Private Sub VerconForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.VerconForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind)
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        tree.Enabled = Not k_busy
        gridLinii.Enabled = Not k_busy
        txtNota.Enabled = Not k_busy AndAlso Not _modify
        RefreshButtons()
    End Sub

    Private Shared Function Fmt(k_value As Decimal) As String
        Return k_value.ToString("N2", CultureInfo.CurrentCulture)
    End Function

    ' ── What became of the invoices in VERCON ───────────────────────────────────

    ' Reads the state of the sendings. A server that does not know the route yet (not deployed) or any failure leaves the column empty and is logged; the list itself is not held back.
    Private Async Function ReadVerconAsync() As Task
        Try
            Dim k_list As List(Of EFacturaVerconSumar) = Await _gate.RunAsync(Function() _api.GetVerconStariAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _vercon = k_list.GroupBy(Function(k_s) k_s.IdPrimita).ToDictionary(Function(k_g) k_g.Key, Function(k_g) k_g.First())
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.ReadVerconAsync", ex)
            _vercon = New Dictionary(Of Integer, EFacturaVerconSumar)()
        End Try
    End Function

    ' What the column «Vercon» says about an invoice.
    Private Function VerconText(k_idPrimita As Integer) As String
        Dim k_s As EFacturaVerconSumar = Nothing
        If Not _vercon.TryGetValue(k_idPrimita, k_s) Then Return String.Empty
        Select Case k_s.Stare
            Case EFacturaVerconStare.Trimisa : Return If(k_s.Revenita, "Trimisă din nou (NIR anulat)", "Trimisă, așteaptă Administratorul")
            Case EFacturaVerconStare.Preluata : Return "Preluată în NIR " & String.Join(", ", k_s.Nir)
            Case EFacturaVerconStare.Returnata : Return "Returnată: " & k_s.MotivReturnare
            Case Else : Return String.Empty
        End Select
    End Function

    ' ── The invoices (left): the tree ───────────────────────────────────────────

    ' The invoices ticked in the tree, in the order the tree shows them. A taken-over invoice is locked (its tick is grey and cannot be changed): it is never sent again.
    Private Function TickedIds() As List(Of Integer)
        Return _order.Where(Function(k_x) _nodes.ContainsKey(k_x) AndAlso Not _nodes(k_x).CheckLocked AndAlso
                                          _nodes(k_x).CheckState = AdvancedTreeControl.TreeCheckState.Checked).ToList()
    End Function

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

    Private Shared Function MonthKey(k_p As EFacturaPrimita) As Integer
        Return If(k_p.DataFact.HasValue, k_p.DataFact.Value.Year * 100 + k_p.DataFact.Value.Month, 0)
    End Function

    ' «septembrie» (the year only when the list holds more than one, so two Septembers cannot be mistaken).
    Private Shared Function MonthLabel(k_key As Integer, k_withYear As Boolean) As String
        If k_key = 0 Then Return "Fără dată"
        Dim k_name As String = _ro.DateTimeFormat.GetMonthName(k_key Mod 100)
        k_name = Char.ToUpper(k_name(0), _ro) & k_name.Substring(1)
        Return If(k_withYear, $"{k_name} {(k_key \ 100).ToString(CultureInfo.InvariantCulture)}", k_name)
    End Function

    ' The short word that follows an invoice in the tree, for an invoice that was sent: «trimisă», «preluată», «returnată».
    Private Function VerconWord(k_idPrimita As Integer) As String
        Dim k_s As EFacturaVerconSumar = Nothing
        If Not _vercon.TryGetValue(k_idPrimita, k_s) Then Return String.Empty
        Select Case k_s.Stare
            Case EFacturaVerconStare.Trimisa : Return If(k_s.Revenita, "trimisă din nou", "trimisă")
            Case EFacturaVerconStare.Preluata : Return "preluată"
            Case EFacturaVerconStare.Returnata : Return "returnată"
            Case Else : Return String.Empty
        End Select
    End Function

    ' Builds the tree again from the invoices read; the ids in k_keep stay ticked (a tick on a leaf is set with the tree's own propagation, so the months and suppliers above follow).
    Private Sub BuildTree(k_keep As ICollection(Of Integer))
        tree.Clear()
        _nodes.Clear()
        _order.Clear()
        Dim k_keepSet As New HashSet(Of Integer)(If(k_keep, New List(Of Integer)()))
        If _groupBySupplier Then
            For Each k_group As IGrouping(Of String, EFacturaPrimita) In _invoices.GroupBy(Function(k_x) SupplierKey(k_x)).
                    OrderBy(Function(k_g) SupplierName(k_g), StringComparer.CurrentCultureIgnoreCase)
                AddSupplierNode(k_group, "S_" & k_group.Key, Nothing, k_keepSet, True)
            Next
        Else
            Dim k_severalYears As Boolean = _invoices.Where(Function(k_x) k_x.DataFact.HasValue).Select(Function(k_x) k_x.DataFact.Value.Year).Distinct().Count() > 1
            For Each k_months As IGrouping(Of Integer, EFacturaPrimita) In _invoices.GroupBy(Function(k_x) MonthKey(k_x)).OrderByDescending(Function(k_g) k_g.Key)
                Dim k_monthId As String = k_months.Key.ToString(CultureInfo.InvariantCulture)
                Dim k_sum As Decimal = k_months.Sum(Function(k_x) k_x.Total)
                Dim k_month As AdvancedTreeControl.TreeItem = tree.AddItem(
                    "M_" & k_monthId, $"{MonthLabel(k_months.Key, k_severalYears)}~~~{k_sum.ToString("N2", _ro)}",
                    pExpanded:=k_months.Any(Function(k_x) k_keepSet.Contains(k_x.IdPrimita)))
                k_month.Bold = True
                k_month.HasCheckBox = True
                For Each k_group As IGrouping(Of String, EFacturaPrimita) In k_months.GroupBy(Function(k_x) SupplierKey(k_x)).
                        OrderBy(Function(k_g) SupplierName(k_g), StringComparer.CurrentCultureIgnoreCase)
                    AddSupplierNode(k_group, "S_" & k_monthId & "_" & k_group.Key, k_month, k_keepSet, False)
                Next
            Next
        End If
        For Each k_id As Integer In _order
            Dim k_leaf As AdvancedTreeControl.TreeItem = _nodes(k_id)
            If k_keepSet.Contains(k_id) AndAlso Not k_leaf.CheckLocked Then tree.SetItemCheckState(k_leaf, AdvancedTreeControl.TreeCheckState.Checked)
        Next
        tree.FooterCaption = "TOTAL  " & _invoices.Sum(Function(k_x) k_x.Total).ToString("N2", _ro) & "  "
        tree.Invalidate()
        UpdateStatus()
    End Sub

    ' A supplier row and its invoices (newest first). k_top: the supplier is the root of the tree (bold), not under a month.
    Private Sub AddSupplierNode(k_group As IEnumerable(Of EFacturaPrimita), k_key As String, k_parent As AdvancedTreeControl.TreeItem,
                                k_keep As HashSet(Of Integer), k_top As Boolean)
        Dim k_list As List(Of EFacturaPrimita) = k_group.OrderByDescending(Function(k_x) k_x.DataFact).ThenByDescending(Function(k_x) k_x.IdPrimita).ToList()
        Dim k_sum As Decimal = k_list.Sum(Function(k_x) k_x.Total)
        Dim k_supplier As AdvancedTreeControl.TreeItem = tree.AddItem(
            k_key, $"{SupplierName(k_list)}~~~{k_sum.ToString("N2", _ro)}", k_parent,
            pExpanded:=k_list.Any(Function(k_x) k_keep.Contains(k_x.IdPrimita)))
        k_supplier.Bold = k_top
        k_supplier.HasCheckBox = True
        For Each k_p As EFacturaPrimita In k_list
            Dim k_date As String = If(k_p.DataFact.HasValue, k_p.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
            Dim k_word As String = VerconWord(k_p.IdPrimita)
            Dim k_caption As String = $"{k_date} · {k_p.NrFact}{If(k_word.Length > 0, " · " & k_word, String.Empty)}~~~{k_p.Total.ToString("N2", _ro)}"
            Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem("P_" & k_p.IdPrimita.ToString(CultureInfo.InvariantCulture), k_caption, k_supplier)
            k_leaf.Tag = k_p
            k_leaf.HasCheckBox = True
            k_leaf.Tooltip = VerconText(k_p.IdPrimita)
            _nodes(k_p.IdPrimita) = k_leaf
            _order.Add(k_p.IdPrimita)
            ' Taken over in a NIR: ticked and locked (grey), never sent again; the Administrator's NIR is the end of the road.
            Dim k_sent As EFacturaVerconSumar = Nothing
            If _vercon.TryGetValue(k_p.IdPrimita, k_sent) AndAlso String.Equals(k_sent.Stare, EFacturaVerconStare.Preluata, StringComparison.Ordinal) Then
                k_leaf.CheckLocked = True
                k_leaf.NodeForeColor = ThemeManager.Current.Palette.TextDimColor
                tree.SetItemCheckState(k_leaf, AdvancedTreeControl.TreeCheckState.Checked)
            End If
        Next
    End Sub

    Private Sub SetTick(k_idPrimita As Integer, k_ticked As Boolean)
        Dim k_leaf As AdvancedTreeControl.TreeItem = Nothing
        If _nodes.TryGetValue(k_idPrimita, k_leaf) AndAlso Not k_leaf.CheckLocked Then
            tree.SetItemCheckState(k_leaf, If(k_ticked, AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked))
        End If
    End Sub

    Private Sub UpdateStatus()
        If _invoices.Count = 0 Then
            lblStatus.Text = "Nu sunt facturi de afișat."
            Return
        End If
        Dim k_n As Integer = TickedIds().Count
        lblStatus.Text = If(k_n = 0, "Nimic bifat.", $"{k_n} din {_invoices.Count} facturi bifate.")
    End Sub

    ' UI boundary (async Sub): a tick changed (on an invoice, or on a month / supplier that ticks all its invoices) -> the state of the newly ticked invoices is read and the lines are put on screen.
    Private Async Sub Tree_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles tree.NodeChecked
        Try
            If _busy Then Return
            UpdateStatus()
            Await SyncTicksAsync().ConfigureAwait(True)
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.Tree_NodeChecked", ex)
            ShowNotice("Facturile bifate nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' The options button of the header: the grouping of the tree (only the one that is NOT in force is offered: it is the switch).
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            ' A second press on the button CLOSES the menu (the press already closed it, so without this guard it would reopen at once).
            If CustomPopup.ClosedJustNow Then Return
            Dim k_anchor As System.Drawing.Rectangle = tree.HeaderRightIconRect
            If k_anchor.IsEmpty Then Return
            Dim k_items As New List(Of CustomPopupItem) From {
                If(_groupBySupplier, New CustomPopupItem(OptPeLuna, "Grupare după lună"), New CustomPopupItem(OptPeFurnizor, "Grupare după furnizor"))}
            Dim k_menu As New CustomPopup(k_items)
            AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) GroupingChosen(k_args.Item.Key)
            k_menu.ShowBelow(tree, k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ' UI boundary: the grouping changed -> the tree is built again with the same invoices ticked (the lines on the right do not change).
    Private Sub GroupingChosen(k_key As String)
        Try
            Select Case k_key
                Case OptPeFurnizor
                    _groupBySupplier = True
                Case OptPeLuna
                    _groupBySupplier = False
                Case Else
                    Throw New ArgumentException($"Rând necunoscut în meniul de opțiuni: {k_key}", NameOf(k_key))
            End Select
            BuildTree(TickedIds())
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.GroupingChosen", ex)
        End Try
    End Sub

    ' Reads the state of every ticked invoice not known yet, then fills the lines grid. A newer tick makes this run stop (the newer one finishes the work).
    Private Async Function SyncTicksAsync() As Task
        Dim k_mySeq As Integer = Interlocked.Increment(_seq)
        CaptureChoices()
        For Each k_id As Integer In TickedIds()
            If _states.ContainsKey(k_id) Then Continue For
            Dim k_one As Integer = k_id
            Try
                Dim k_state As EFacturaVerconStareFactura = Await _gate.RunAsync(Function() _api.GetVerconAsync(k_one, _cts.Token)).ConfigureAwait(True)
                If IsDisposed OrElse k_mySeq <> _seq Then Return
                _states(k_id) = k_state
            Catch ex As ApiException
                GlobalErrorLog.Write("VerconForm.SyncTicksAsync", ex)
                SetTick(k_id, False)
                ShowNotice(ex.Message, NoticeKind.Error)
            End Try
        Next
        If IsDisposed OrElse k_mySeq <> _seq Then Return
        UpdateStatus()
        FillGrid()
    End Function

    ' ── The tree of accounts ────────────────────────────────────────────────────

    ' The items of the combo: first «fără cont» (plain), then the accounts under their gestiune. The face text of an account says where it is (it is also what the typed search matches).
    Private Sub BuildChoices(k_tree As List(Of EFacturaVerconGestiune))
        _idByLabel.Clear()
        _labelById.Clear()
        _groupById.Clear()
        _items = New List(Of Object) From {New KBotComboGroupedItem(FaraCont, String.Empty, String.Empty, Nothing)}
        For Each k_g As EFacturaVerconGestiune In k_tree
            Dim k_group As String = If(String.IsNullOrWhiteSpace(k_g.Subunitate) OrElse String.Equals(k_g.Subunitate, k_g.Gestiune, StringComparison.CurrentCultureIgnoreCase),
                                       k_g.Gestiune, $"{k_g.Subunitate} / {k_g.Gestiune}")
            For Each k_c As EFacturaVerconCont In k_g.Conturi
                Dim k_label As String = $"{k_group} / {k_c.Masina}"
                If _idByLabel.ContainsKey(k_label) Then k_label &= $" (#{k_c.IdMasina.ToString(CultureInfo.InvariantCulture)})"
                _idByLabel(k_label) = k_c.IdMasina
                _labelById(k_c.IdMasina) = k_label
                _groupById(k_c.IdMasina) = k_group
                _items.Add(New KBotComboGroupedItem(k_label, k_group, k_c.Masina, k_c.IdMasina))
            Next
        Next
        gridLinii.Column(ColCont).ComboItems = _items
    End Sub

    ' The IdMasina a cell stands for: Nothing for «fără cont» (or text that is not an account).
    Private Function AccountOf(k_cell As Object) As Integer?
        Dim k_text As String = Convert.ToString(k_cell, CultureInfo.InvariantCulture)
        Dim k_id As Integer
        If Not String.IsNullOrWhiteSpace(k_text) AndAlso _idByLabel.TryGetValue(k_text, k_id) Then Return k_id
        Return Nothing
    End Function

    ' ── The lines (right) ───────────────────────────────────────────────────────

    Private Shared Function GroupText(k_f As EFacturaVerconFactura) As String
        Dim k_day As String = If(k_f.DataFact.HasValue, k_f.DataFact.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată")
        Return $"{k_f.NrFact} / {k_day} · {k_f.Furnizor}"
    End Function

    ' Keeps what is in the cells (the accounts chosen so far), so the grid can be filled again without losing them.
    Private Sub CaptureChoices()
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_tag As LineTag = TryCast(k_row.Tag, LineTag)
            If k_tag IsNot Nothing Then _choices(k_tag.IdLinie) = Convert.ToString(k_row(ColCont), CultureInfo.InvariantCulture)
        Next
    End Sub

    Private Shared Function NirNumber(k_d As EFacturaVerconDuplicat) As String
        Dim k_number As String = If(k_d.Numar.HasValue, k_d.Numar.Value.ToString(CultureInfo.InvariantCulture), "?")
        If k_d.NumarSub.HasValue AndAlso k_d.NumarSub.Value <> 0 Then k_number &= "." & k_d.NumarSub.Value.ToString("00", CultureInfo.InvariantCulture).TrimEnd("0"c)
        Return k_number
    End Function

    Private Shared Function StateText(k_t As EFacturaVerconTrimitere) As String
        Select Case k_t.Stare
            Case EFacturaVerconStare.Trimisa : Return If(k_t.Revenita, "trimisă din nou (NIR anulat)", "trimisă")
            Case EFacturaVerconStare.Preluata : Return "preluată într-un NIR"
            Case EFacturaVerconStare.Returnata : Return "returnată"
            Case Else : Return "retrasă"
        End Select
    End Function

    ' Puts the lines of the ticked invoices on screen. ONE ticked invoice that was sent and still waits opens in the mode that changes its accounts (or takes it back); an invoice
    ' that cannot be sent (taken over already, no supplier among the partners, ...) is named in the notice and left out.
    Private Sub FillGrid()
        Dim k_notes As New List(Of String)()
        Dim k_ids As List(Of Integer) = TickedIds().Where(Function(k_x) _states.ContainsKey(k_x)).ToList()
        _canWrite = k_ids.All(Function(k_x) _states(k_x).PoateScrie)
        _modify = k_ids.Count = 1 AndAlso _states(k_ids(0)).Trimitere IsNot Nothing AndAlso
                  String.Equals(_states(k_ids(0)).Trimitere.Stare, EFacturaVerconStare.Trimisa, StringComparison.Ordinal)
        _filling = True
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            If Not _canWrite Then k_notes.Add("Numai un utilizator cu rolul «Contabil» poate trimite facturi la Vercon.")
            For Each k_id As Integer In k_ids
                Dim k_state As EFacturaVerconStareFactura = _states(k_id)
                Dim k_name As String = k_state.Factura.NrFact
                If Not _modify Then
                    If k_state.Trimitere IsNot Nothing Then
                        k_notes.Add($"Factura {k_name} este deja trimisă la Vercon (stare: {StateText(k_state.Trimitere)}); nu se trimite din nou.")
                        Continue For
                    End If
                    If Not k_state.PoateTrimite Then
                        k_notes.Add($"Factura {k_name} nu se poate trimite: {k_state.Motiv}")
                        Continue For
                    End If
                End If
                For Each k_d As EFacturaVerconDuplicat In k_state.Duplicate
                    k_notes.Add($"Atenție: factura {k_name} pare deja înregistrată în Vercon (NIR {NirNumber(k_d)}, {k_d.Gestiune}).")
                Next
                Dim k_group As String = GroupText(k_state.Factura)
                For Each k_l As EFacturaVerconLinie In k_state.Linii
                    Dim k_row As KBotDataRow = gridLinii.AddRow()
                    k_row.Tag = New LineTag() With {.IdPrimita = k_id, .IdLinie = k_l.IdLinie}
                    k_row(ColFactura) = k_group
                    k_row(ColNr) = k_l.NrLinie
                    k_row(ColDenumire) = k_l.Denumire
                    k_row(ColCant) = $"{k_l.Cant.ToString("N2", CultureInfo.CurrentCulture)} {k_l.Unit}".Trim()
                    k_row(ColCuTva) = CDbl(k_l.ValoareCuTva)
                    Dim k_label As String = Nothing
                    If _choices.TryGetValue(k_l.IdLinie, k_label) Then
                        k_row(ColCont) = k_label
                    ElseIf _modify AndAlso k_l.IdMasina.HasValue AndAlso _labelById.TryGetValue(k_l.IdMasina.Value, k_label) Then
                        k_row(ColCont) = k_label
                    Else
                        k_row(ColCont) = FaraCont
                    End If
                Next
            Next
        Finally
            gridLinii.EndUpdate()
            _filling = False
        End Try
        ApplyMode(k_ids)
        If k_notes.Count > 0 Then
            ShowNotice(String.Join(vbCrLf, k_notes), If(gridLinii.RowCount = 0 OrElse Not _canWrite, NoticeKind.Error, NoticeKind.Warning))
        Else
            ntfMesaj.Visible = False
        End If
        UpdateSummary()
        RefreshButtons()
    End Sub

    ' What the mode shows: the button, the one that takes the invoice back, the note (the one of the sending, read-only, while its accounts are changed).
    Private Sub ApplyMode(k_ids As List(Of Integer))
        btnRetrage.Visible = _modify
        If _modify Then
            txtNota.Text = _states(k_ids(0)).Trimitere.Observatie
            txtNota.ReadOnly = True
            txtNota.Enabled = Not _busy
        ElseIf _wasModify Then
            txtNota.Text = String.Empty
            txtNota.ReadOnly = False
            txtNota.Enabled = Not _busy
        End If
        _wasModify = _modify
    End Sub

    ' The line at the bottom: where the lines go (per gestiune of the proposed accounts) and how many have no account.
    Private Sub UpdateSummary()
        Dim k_order As New List(Of String)()
        Dim k_count As New Dictionary(Of String, Integer)(StringComparer.CurrentCultureIgnoreCase)
        Dim k_sum As New Dictionary(Of String, Decimal)(StringComparer.CurrentCultureIgnoreCase)
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_id As Integer? = AccountOf(k_row(ColCont))
            Dim k_group As String = FaraContGrup
            If k_id.HasValue Then
                Dim k_g As String = Nothing
                If _groupById.TryGetValue(k_id.Value, k_g) Then k_group = k_g
            End If
            If Not k_count.ContainsKey(k_group) Then
                k_order.Add(k_group)
                k_count(k_group) = 0
                k_sum(k_group) = 0D
            End If
            k_count(k_group) += 1
            Dim k_value As Double
            If Double.TryParse(Convert.ToString(k_row(ColCuTva), CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, k_value) Then k_sum(k_group) += CDec(k_value)
        Next
        If k_order.Count = 0 Then
            lblRezumat.Text = String.Empty
            Return
        End If
        k_order.Sort(Function(k_a, k_b)
                         If String.Equals(k_a, FaraContGrup, StringComparison.Ordinal) Then Return 1
                         If String.Equals(k_b, FaraContGrup, StringComparison.Ordinal) Then Return -1
                         Return String.Compare(k_a, k_b, StringComparison.CurrentCultureIgnoreCase)
                     End Function)
        lblRezumat.Text = "Unde merg liniile: " & String.Join("  ·  ", k_order.Select(
            Function(k_g) $"{k_g}: {k_count(k_g)} {If(k_count(k_g) = 1, "linie", "linii")}, {Fmt(k_sum(k_g))} lei"))
    End Sub

    ' How many invoices have lines in the grid now.
    Private Function InvoiceCount() As Integer
        Dim k_set As New HashSet(Of Integer)()
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_tag As LineTag = TryCast(k_row.Tag, LineTag)
            If k_tag IsNot Nothing Then k_set.Add(k_tag.IdPrimita)
        Next
        Return k_set.Count
    End Function

    Private Sub RefreshButtons()
        Dim k_count As Integer = If(_busy, 0, InvoiceCount())
        btnTrimite.Enabled = k_count > 0 AndAlso _canWrite AndAlso Not _busy
        btnRetrage.Enabled = _modify AndAlso _canWrite AndAlso Not _busy
        If _modify Then
            btnTrimite.Text = "Salvează conturile"
        Else
            btnTrimite.Text = If(k_count > 0, $"Trimite la Vercon ({k_count})", "Trimite la Vercon")
        End If
    End Sub

    ' ── Editing ─────────────────────────────────────────────────────────────────

    ' «Cont Vercon» takes «fără cont» or one of the accounts of the tree; typed text that is neither is refused.
    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs)
        Try
            If Not String.Equals(e.ColumnKey, ColCont, StringComparison.Ordinal) Then Return
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) OrElse String.Equals(k_text, FaraCont, StringComparison.CurrentCultureIgnoreCase) Then
                e.ProposedValue = FaraCont
            ElseIf Not _idByLabel.ContainsKey(k_text) Then
                e.Cancel = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.GridLinii_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs)
        Try
            If _filling OrElse Not String.Equals(e.ColumnKey, ColCont, StringComparison.Ordinal) Then Return
            UpdateSummary()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

    ' UI boundary: an account picked on the header of an invoice goes on ALL its lines, whatever they had.
    Private Sub GridLinii_GroupHeaderValuePicked(sender As Object, e As KBotGroupHeaderValueEventArgs)
        Try
            If Not String.Equals(e.ColumnKey, ColCont, StringComparison.Ordinal) Then Return
            Dim k_text As String = Convert.ToString(e.Value, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) OrElse (Not String.Equals(k_text, FaraCont, StringComparison.CurrentCultureIgnoreCase) AndAlso Not _idByLabel.ContainsKey(k_text)) Then Return
            _filling = True
            Try
                For Each k_index As Integer In e.RowIndexes
                    gridLinii.Rows(k_index)(ColCont) = k_text
                Next
            Finally
                _filling = False
            End Try
            gridLinii.Invalidate()
            UpdateSummary()
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.GridLinii_GroupHeaderValuePicked", ex)
        End Try
    End Sub

    ' ── Writing ─────────────────────────────────────────────────────────────────

    Private Function AccountsOf(k_idPrimita As Integer) As List(Of EFacturaVerconContLinie)
        Dim k_list As New List(Of EFacturaVerconContLinie)()
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_tag As LineTag = TryCast(k_row.Tag, LineTag)
            If k_tag Is Nothing OrElse k_tag.IdPrimita <> k_idPrimita Then Continue For
            k_list.Add(New EFacturaVerconContLinie() With {.IdLinie = k_tag.IdLinie, .IdMasina = AccountOf(k_row(ColCont))})
        Next
        Return k_list
    End Function

    ' After a write: the invoices written leave the lines grid (unticked, their state is read again), the others stay as they are; the column «Vercon» shows the new state.
    Private Async Function AfterWriteAsync(k_done As List(Of Integer)) As Task
        For Each k_id As Integer In k_done
            Dim k_state As EFacturaVerconStareFactura = Nothing
            If _states.TryGetValue(k_id, k_state) Then
                For Each k_l As EFacturaVerconLinie In k_state.Linii
                    _choices.Remove(k_l.IdLinie)
                Next
            End If
            _states.Remove(k_id)
            SetTick(k_id, False)
        Next
        Await ReadVerconAsync().ConfigureAwait(True)
        If IsDisposed Then Return
        BuildTree(TickedIds())
        FillGrid()
    End Function

    ' UI boundary (async Sub): logs and swallows; a failed invoice is named in a notice and stays with its accounts (the server writes one invoice all or none).
    Private Async Sub BtnTrimite_Click(sender As Object, e As EventArgs) Handles btnTrimite.Click
        Try
            If _api Is Nothing OrElse _busy OrElse Not _canWrite Then Return
            CaptureChoices()
            Dim k_todo As New List(Of Integer)()
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_tag As LineTag = TryCast(k_row.Tag, LineTag)
                If k_tag IsNot Nothing AndAlso Not k_todo.Contains(k_tag.IdPrimita) Then k_todo.Add(k_tag.IdPrimita)
            Next
            If k_todo.Count = 0 Then Return
            ntfMesaj.Visible = False
            SetBusy(True)
            Dim k_failures As New List(Of String)()
            Dim k_done As New List(Of Integer)()
            Dim k_note As String = txtNota.Text
            Dim k_changeAccounts As Boolean = _modify
            Try
                For Each k_id As Integer In k_todo
                    Dim k_one As Integer = k_id
                    Dim k_name As String = _states(k_id).Factura.NrFact
                    Try
                        Dim k_accounts As List(Of EFacturaVerconContLinie) = AccountsOf(k_one)
                        If k_changeAccounts Then
                            Await _gate.RunAsync(Function() _api.SetVerconConturiAsync(k_one, k_accounts, _cts.Token)).ConfigureAwait(True)
                        Else
                            Await _gate.RunAsync(Function() _api.SendVerconAsync(k_one, k_accounts, k_note, _cts.Token)).ConfigureAwait(True)
                        End If
                        If IsDisposed Then Return
                        k_done.Add(k_id)
                    Catch ex As OperationCanceledException
                        Throw
                    Catch ex As ApiException
                        GlobalErrorLog.Write("VerconForm.BtnTrimite_Click", ex)
                        k_failures.Add($"Factura {k_name}: {ex.Message}")
                    End Try
                Next
                Await AfterWriteAsync(k_done).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
            If IsDisposed Then Return
            If k_failures.Count > 0 Then
                ShowNotice(String.Join(vbCrLf, k_failures), NoticeKind.Error)
            ElseIf k_done.Count > 0 Then
                ShowNotice(If(k_changeAccounts, "Conturile propuse au fost salvate.",
                              If(k_done.Count = 1, "O factură a fost trimisă la Vercon.", $"{k_done.Count} facturi au fost trimise la Vercon.")), NoticeKind.Success)
            End If
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.BtnTrimite_Click", ex)
            If Not IsDisposed Then
                ShowNotice("Trimiterea nu a putut fi făcută. Detalii în jurnalul de erori.", NoticeKind.Error)
                SetBusy(False)
            End If
        End Try
    End Sub

    ' UI boundary (async Sub): logs and swallows; a refusal of the server (e.g. the Administrator took the invoice meanwhile) is said to the operator and the state is read again.
    Private Async Sub BtnRetrage_Click(sender As Object, e As EventArgs) Handles btnRetrage.Click
        Try
            If _api Is Nothing OrElse _busy OrElse Not _modify OrElse Not _canWrite Then Return
            Dim k_ids As List(Of Integer) = TickedIds().Where(Function(k_x) _states.ContainsKey(k_x)).ToList()
            If k_ids.Count <> 1 Then Return
            Dim k_id As Integer = k_ids(0)
            Dim k_state As EFacturaVerconStareFactura = _states(k_id)
            If k_state.Trimitere Is Nothing Then Return
            If KBotMessage.Show(Me, $"Retragi factura {k_state.Factura.NrFact} de la Vercon? Administratorul nu o va mai vedea în lista lui. O poți trimite din nou oricând.",
                                "Retrage de la Vercon", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            Dim k_sending As Integer = k_state.Trimitere.IdTrimitere
            ntfMesaj.Visible = False
            SetBusy(True)
            Try
                Dim k_withdrawn As Boolean
                Try
                    Await _gate.RunAsync(Function() _api.WithdrawVerconAsync(k_id, k_sending, _cts.Token)).ConfigureAwait(True)
                    k_withdrawn = True
                Catch ex As ApiException
                    GlobalErrorLog.Write("VerconForm.BtnRetrage_Click", ex)
                    If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Retrage de la Vercon", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End Try
                If IsDisposed Then Return
                ' Withdrawn or refused, the state of the invoice changed on the server: it is read again either way.
                Await AfterWriteAsync(New List(Of Integer) From {k_id}).ConfigureAwait(True)
                If Not IsDisposed AndAlso k_withdrawn Then ShowNotice("Factura a fost retrasă de la Vercon.", NoticeKind.Success)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
        Catch ex As OperationCanceledException
            ' The page was closed.
        Catch ex As Exception
            GlobalErrorLog.Write("VerconForm.BtnRetrage_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Retragerea nu a putut fi făcută: " & ex.Message, "Retrage de la Vercon", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                SetBusy(False)
            End If
        End Try
    End Sub

End Class
