Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Conturi» view of the «Plan de conturi» window (slice AV01-04): the tree Clasa &gt; Cont de baza &gt; Clasificatie
''' functionala &gt; Clasificatie economica &gt; cont complet (search in its header) and, on the right, the fields of the
''' selected account with its opening balance and cumulated turnovers. The base account comes from the common nomenclator and
''' says whether the account MAY have a functional and / or economic classification (optional: the box enables its combo, an
''' unticked box clears it; refused when the base account does not allow it). The account is READ-ONLY until «Modificare» (or
''' «Adaugare») is pressed. The sector and source follow the title bar: editable only while it is on «Toate».
''' The symbol is composed by the SERVER (here only previewed). A used account is never deleted.
''' </summary>
Public Class PlanContConturiView
    Implements IPlanContView, IThemedControl

    Private Shared ReadOnly AnaliticRegex As New Regex("^[0-9A-Z_]{1,10}$", RegexOptions.CultureInvariant)
    Private Const Caption As String = "Plan de conturi"

    Private _ctx As PlanContContext
    Private _current As ContPlan
    Private _currentNode As AdvancedTreeControl.TreeItem
    Private _dirty As Boolean
    Private _loading As Boolean
    Private _flagging As Boolean
    Private _busy As Boolean
    ' The fields work only while the operator edits (after «Modificare» / «Adaugare»); otherwise they only show the account.
    Private _editing As Boolean
    ' What the base account of the shown account allows (the boxes work only inside it).
    Private _allowF As Boolean
    Private _allowE As Boolean

    Public Event StatusMessage As Action(Of String) Implements IPlanContView.StatusMessage
    Public Event PlanChanged As Action(Of Integer?) Implements IPlanContView.PlanChanged
    Public Event BusyChanged As Action(Of Boolean) Implements IPlanContView.BusyChanged

    Public Sub New()
        InitializeComponent()
        cboFel.CaptionSelector = Function(k_item) FelCaption(CStr(k_item))
        cboFel.Items.Add("A")
        cboFel.Items.Add("B")
        cboFel.Items.Add("P")
        cboCont.CaptionSelector = Function(k_item) CType(k_item, ContNomenclator).Simbol.ToString(CultureInfo.InvariantCulture) &
                                                   " — " & CType(k_item, ContNomenclator).Denumire
        cboClsfF.CaptionSelector = Function(k_item) ClsfCaption(CType(k_item, ClsfCont))
        cboClsfE.CaptionSelector = Function(k_item) ClsfCaption(CType(k_item, ClsfCont))
    End Sub

    Public ReadOnly Property HasPending As Boolean Implements IPlanContView.HasPending
        Get
            Return _dirty
        End Get
    End Property

    Public ReadOnly Property CurrentContId As Integer? Implements IPlanContView.CurrentContId
        Get
            Return If(_current IsNot Nothing AndAlso _current.IdCont > 0, CType(_current.IdCont, Integer?), Nothing)
        End Get
    End Property

    Private Shared Function ClsfCaption(k_c As ClsfCont) As String
        Return If(String.IsNullOrEmpty(k_c.Denumire), k_c.Cod, k_c.Cod & " — " & k_c.Denumire)
    End Function

    Private Shared Function FelCaption(k_fel As String) As String
        Select Case k_fel
            Case "A" : Return "A — Activ"
            Case "B" : Return "B — Bifuncțional"
            Case "P" : Return "P — Pasiv"
            Case Else : Return k_fel
        End Select
    End Function

    ' ── Binding ─────────────────────────────────────────────────────────────────

    Public Sub Bind(k_context As PlanContContext, k_selectCont As Integer?) Implements IPlanContView.Bind
        Try
            ArgumentNullException.ThrowIfNull(k_context)
            _ctx = k_context
            FillListsOnce()
            BuildTree(k_selectCont)
            Dim k_selected As ContPlan = If(k_selectCont.HasValue,
                _ctx.Plan.Conturi.FirstOrDefault(Function(k_c) k_c.IdCont = k_selectCont.Value), Nothing)
            ShowCont(k_selected)
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.Bind", ex)
            Throw
        End Try
    End Sub

    Private Sub FillListsOnce()
        If _ctx.Nomenclator Is Nothing OrElse cboCont.Items.Count > 0 Then Return
        cboCont.BeginUpdate()
        cboClsfF.BeginUpdate()
        cboClsfE.BeginUpdate()
        Try
            For Each k_c As ContNomenclator In _ctx.Nomenclator.Conturi
                cboCont.Items.Add(k_c)
            Next
            ' Sector = the first two characters of a sector+source code (01A), source = the rest.
            For Each k_sector As String In _ctx.Nomenclator.SurseSector.Where(Function(k_s) k_s.Cod.Length >= 3).
                                               Select(Function(k_s) k_s.Cod.Substring(0, 2)).Distinct().OrderBy(Function(k_s) k_s, StringComparer.Ordinal)
                cboSector.Items.Add(k_sector)
            Next
            For Each k_sursa As String In _ctx.Nomenclator.SurseSector.Where(Function(k_s) k_s.Cod.Length >= 3).
                                              Select(Function(k_s) k_s.Cod.Substring(2)).Distinct().OrderBy(Function(k_s) k_s, StringComparer.Ordinal)
                cboSursa.Items.Add(k_sursa)
            Next
            For Each k_c As ClsfCont In _ctx.Nomenclator.ClsfF
                cboClsfF.Items.Add(k_c)
            Next
            For Each k_c As ClsfCont In _ctx.Nomenclator.ClsfE
                cboClsfE.Items.Add(k_c)
            Next
        Finally
            cboCont.EndUpdate()
            cboClsfF.EndUpdate()
            cboClsfE.EndUpdate()
        End Try
    End Sub

    ' The tree: Clasa > base account > ClsfF > ClsfE > full account. A level exists only when some account uses it.
    Private Sub BuildTree(k_selectId As Integer?)
        tree.Clear()
        _currentNode = Nothing
        If _ctx Is Nothing OrElse _ctx.Nomenclator Is Nothing Then Return
        Dim k_folder As Image = My.Resources.Resources.folder_open
        Dim k_dim As Color = ThemeManager.Current.Palette.TextDimColor
        Dim k_shown As Integer = 0
        Dim k_visible As List(Of ContPlan) = _ctx.VisibleAccounts()

        Dim k_clase As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
        Dim k_baze As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
        Dim k_fNodes As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.Ordinal)
        Dim k_eNodes As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.Ordinal)

        For Each k_c As ContPlan In k_visible.OrderBy(Function(k_x) k_x.Cont).ThenBy(Function(k_x) k_x.ClsfF, StringComparer.Ordinal).
                                          ThenBy(Function(k_x) k_x.ClsfE, StringComparer.Ordinal).ThenBy(Function(k_x) k_x.Simbol, StringComparer.Ordinal)
            Dim k_clasa As Integer = CInt(k_c.Cont.ToString(CultureInfo.InvariantCulture).Substring(0, 1))
            If Not k_clase.ContainsKey(k_clasa) Then
                Dim k_nume As String = _ctx.Nomenclator.Clase.FirstOrDefault(Function(k_x) k_x.Clasa = k_clasa)?.Denumire
                k_clase(k_clasa) = tree.AddItem($"K|{k_clasa}", $"CLASA {k_clasa}~~~{Escape(k_nume)}",
                                                pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
                k_clase(k_clasa).Bold = True   ' the top level is bold, the rest plain: same look as the «Clasificatii» tree
            End If
            If Not k_baze.ContainsKey(k_c.Cont) Then
                Dim k_nom As ContNomenclator = Nothing
                _ctx.ContByCode.TryGetValue(k_c.Cont, k_nom)
                k_baze(k_c.Cont) = tree.AddItem($"B|{k_c.Cont}", $"{k_c.Cont}~~~{Escape(k_nom?.Denumire)}", k_clase(k_clasa),
                                                pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
            End If
            Dim k_parent As AdvancedTreeControl.TreeItem = k_baze(k_c.Cont)
            If k_c.ClsfF.Length > 0 Then
                Dim k_keyF As String = $"{k_c.Cont}|{k_c.ClsfF}"
                If Not k_fNodes.ContainsKey(k_keyF) Then
                    Dim k_name As String = Nothing
                    _ctx.ClsfFNames.TryGetValue(k_c.ClsfF, k_name)
                    k_fNodes(k_keyF) = tree.AddItem($"F|{k_keyF}", $"{Escape(k_c.ClsfF)}~~~{Escape(k_name)}", k_parent,
                                                    pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
                End If
                k_parent = k_fNodes(k_keyF)
                If k_c.ClsfE.Length > 0 Then
                    Dim k_keyE As String = $"{k_keyF}|{k_c.ClsfE}"
                    If Not k_eNodes.ContainsKey(k_keyE) Then
                        Dim k_name As String = Nothing
                        _ctx.ClsfENames.TryGetValue(k_c.ClsfE, k_name)
                        k_eNodes(k_keyE) = tree.AddItem($"E|{k_keyE}", $"{Escape(k_c.ClsfE)}~~~{Escape(k_name)}", k_parent,
                                                        pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
                    End If
                    k_parent = k_eNodes(k_keyE)
                End If
            End If
            Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"L|{k_c.IdCont}", $"{Escape(k_c.Simbol)}~~~{Escape(k_c.Denumire)}", k_parent)
            k_leaf.Tag = k_c
            k_leaf.Tooltip = k_c.Denumire
            If Not k_c.Activ Then
                k_leaf.Italic = True
                k_leaf.NodeForeColor = k_dim
            End If
            If k_selectId.HasValue AndAlso k_c.IdCont = k_selectId.Value Then
                tree.SelectAndReveal(k_leaf)
                _currentNode = k_leaf
            End If
            k_shown += 1
        Next
        tree.HeaderCaption = $" CONTURI ({k_shown})"
        tree.Invalidate()
    End Sub

    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    ' ── Options of the tree (the right icon of its header) ─────────────────────────────

    Private Const MenuInactive As String = "arata_inactive"

    ''' <summary>The right icon of the tree header opens its options under it: for now only «Arata si conturile inactive».</summary>
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            ' A second press on the icon CLOSES the menu (the press already closed it, so without this guard it would reopen at once).
            If CustomPopup.ClosedJustNow OrElse _ctx Is Nothing Then Return
            Dim k_anchor As Rectangle = tree.HeaderRightIconRect
            If k_anchor.IsEmpty Then Return
            Dim k_rows As New List(Of CustomPopupItem) From {
                New CustomPopupItem(MenuInactive, "Arată și conturile &inactive", My.Resources.Resources.cells) With {
                    .Checked = _ctx.ShowInactive}}
            ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
            Dim k_menu As New CustomPopup(k_rows)
            AddHandler k_menu.ItemClicked, AddressOf TreeMenu_ItemClicked
            k_menu.ShowBelow(tree, k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    ' Inactive accounts show (or not) in the tree and in the balances grid; what is pending is settled first. UI boundary.
    Private Async Sub TreeMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            If _ctx Is Nothing OrElse Not String.Equals(e.Item.Key, MenuInactive, StringComparison.Ordinal) Then Return
            Dim k_cont As Integer? = CurrentContId
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            _ctx.ShowInactive = Not _ctx.ShowInactive
            Bind(_ctx, k_cont)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.TreeMenu_ItemClicked", ex)
        End Try
    End Sub

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing OrElse pNode Is _currentNode OrElse TryCast(pNode.Tag, ContPlan) Is Nothing Then Return
            ChangeSelection(pNode, _currentNode)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' UI boundary: asks about unsaved changes, then shows the account.
    Private Async Sub ChangeSelection(k_node As AdvancedTreeControl.TreeItem, k_previous As AdvancedTreeControl.TreeItem)
        Try
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then
                If k_previous IsNot Nothing Then tree.SelectAndReveal(k_previous)
                Return
            End If
            _currentNode = k_node
            ShowCont(TryCast(k_node.Tag, ContPlan))
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.ChangeSelection", ex)
        End Try
    End Sub

    ''' <summary>Unsaved changes: Yes saves (False when the save fails), No drops them, Cancel stays. True = go on.</summary>
    Private Async Function ConfirmLeaveAsync() As Task(Of Boolean)
        If _busy Then Return False
        If Not _dirty Then Return True
        Dim k_answer As DialogResult = KBotMessage.Show(Me, "Contul are modificări nesalvate." & vbLf & "Le salvați?",
                                                        Caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        If k_answer = DialogResult.Cancel Then Return False
        If k_answer = DialogResult.No Then
            _dirty = False
            Return True
        End If
        Return Await SaveAsync().ConfigureAwait(True)
    End Function

    Public Function CommitAsync() As Task(Of Boolean) Implements IPlanContView.CommitAsync
        Return ConfirmLeaveAsync()
    End Function

    ' ── Details ─────────────────────────────────────────────────────────────────

    ''' <summary>Fills the right side from an account (Nothing = empty, nothing selected).</summary>
    Private Sub ShowCont(k_cont As ContPlan)
        _loading = True
        Try
            _current = k_cont
            Dim k_nom As ContNomenclator = Nothing
            If k_cont IsNot Nothing AndAlso _ctx IsNot Nothing Then _ctx.ContByCode.TryGetValue(k_cont.Cont, k_nom)
            ' A new account (not saved yet) is edited at once; an existing one only shows until «Modificare».
            _editing = k_cont IsNot Nothing AndAlso k_cont.IdCont = 0
            cboCont.SelectedItem = If(k_nom Is Nothing, Nothing, CObj(k_nom))
            SetSs(If(k_cont IsNot Nothing AndAlso k_cont.SursaSector.Length > 0, k_cont.SursaSector,
                     If(k_cont Is Nothing OrElse _ctx Is Nothing, String.Empty, _ctx.DefaultSs())))
            chkClsfF.Checked = k_cont IsNot Nothing AndAlso k_cont.ClsfF.Length > 0
            chkClsfE.Checked = k_cont IsNot Nothing AndAlso k_cont.ClsfE.Length > 0
            ApplyBaseFlags(k_nom)
            If chkClsfF.Checked Then SelectClsf(cboClsfF, k_cont.ClsfF)
            If chkClsfE.Checked Then SelectClsf(cboClsfE, k_cont.ClsfE)
            txtAnalitic.Text = If(k_cont?.Analitic, String.Empty)
            txtDenumire.Text = If(k_cont?.Denumire, String.Empty)
            cboFel.SelectedIndex = If(k_cont Is Nothing, -1, Math.Max(-1, "ABP".IndexOf(k_cont.Fel, StringComparison.Ordinal)))
            Dim k_sold As SoldCont = If(k_cont Is Nothing OrElse _ctx Is Nothing, New SoldCont(), _ctx.SoldOf(k_cont.IdCont))
            txtSoldD.Text = FormatAmount(k_sold.SoldDebitor)
            txtSoldC.Text = FormatAmount(k_sold.SoldCreditor)
            txtRulajD.Text = FormatAmount(k_sold.RulajDebitor)
            txtRulajC.Text = FormatAmount(k_sold.RulajCreditor)
            UpdateSimbol()
        Finally
            _loading = False
        End Try
        ApplyEditState()
        SetDirty(k_cont IsNot Nothing AndAlso k_cont.IdCont = 0)
    End Sub

    ' Sector and source of the account in the two combos (01A = 01 + A); an unknown or empty code leaves them empty.
    Private Sub SetSs(k_ss As String)
        Dim k_text As String = If(k_ss, String.Empty)
        SelectText(cboSector, If(k_text.Length >= 3, k_text.Substring(0, 2), String.Empty))
        SelectText(cboSursa, If(k_text.Length >= 3, k_text.Substring(2), String.Empty))
    End Sub

    Private Shared Sub SelectText(k_combo As KBotComboBox, k_value As String)
        k_combo.SelectedIndex = -1
        For k_i As Integer = 0 To k_combo.Items.Count - 1
            If String.Equals(CStr(k_combo.Items(k_i)), k_value, StringComparison.Ordinal) Then
                k_combo.SelectedIndex = k_i
                Return
            End If
        Next
    End Sub

    Private Function CurrentSs() As String
        Dim k_sector As String = TryCast(cboSector.SelectedItem, String)
        Dim k_sursa As String = TryCast(cboSursa.SelectedItem, String)
        If String.IsNullOrEmpty(k_sector) OrElse String.IsNullOrEmpty(k_sursa) Then Return String.Empty
        Return k_sector & k_sursa
    End Function

    ''' <summary>The title bar changed its sector/source or went to / left «Toate»: a new account takes the new choice, the fields follow.</summary>
    Public Sub RefreshSs()
        Try
            If _ctx Is Nothing OrElse _current Is Nothing Then Return
            _loading = True
            Try
                If _current.IdCont = 0 AndAlso Not _ctx.SsEditable Then SetSs(_ctx.DefaultSs())
            Finally
                _loading = False
            End Try
            ApplyEditState()
            UpdateSimbol()
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.RefreshSs", ex)
        End Try
    End Sub

    ' What can be typed: nothing before «Modificare» / «Adaugare»; then every field, the sector and source only on «Toate», each
    ' classification only inside what the base account allows.
    Private Sub ApplyEditState()
        Dim k_has As Boolean = _current IsNot Nothing
        Dim k_edit As Boolean = k_has AndAlso _editing
        cboCont.Enabled = k_edit
        Dim k_ss As Boolean = k_edit AndAlso _ctx IsNot Nothing AndAlso _ctx.SsEditable
        cboSector.Enabled = k_ss
        cboSursa.Enabled = k_ss
        SyncClsfState()
        txtAnalitic.ReadOnly = Not k_edit
        txtDenumire.ReadOnly = Not k_edit
        cboFel.Enabled = k_edit
        txtSoldD.ReadOnly = Not k_edit
        txtSoldC.ReadOnly = Not k_edit
        txtRulajD.ReadOnly = Not k_edit
        txtRulajC.ReadOnly = Not k_edit
        RefreshButtons()
    End Sub

    Private Shared Function FormatAmount(k_value As Decimal) As String
        Return k_value.ToString("N2", CultureInfo.CurrentCulture)
    End Function

    ''' <summary>An amount typed by the operator: empty = 0, two decimals, never negative.</summary>
    Private Shared Function TryAmount(k_text As String, ByRef k_value As Decimal) As Boolean
        Dim k_t As String = If(k_text, String.Empty).Trim()
        If k_t.Length = 0 Then
            k_value = 0D
            Return True
        End If
        If Not Decimal.TryParse(k_t, NumberStyles.Number, CultureInfo.CurrentCulture, k_value) AndAlso
           Not Decimal.TryParse(k_t, NumberStyles.Number, CultureInfo.InvariantCulture, k_value) Then Return False
        k_value = Math.Round(k_value, 2, MidpointRounding.AwayFromZero)
        Return k_value >= 0D
    End Function

    Private Shared Sub SelectClsf(k_combo As KBotComboBox, k_cod As String)
        k_combo.SelectedIndex = -1
        If k_cod.Length = 0 Then Return
        For k_i As Integer = 0 To k_combo.Items.Count - 1
            If String.Equals(CType(k_combo.Items(k_i), ClsfCont).Cod, k_cod, StringComparison.Ordinal) Then
                k_combo.SelectedIndex = k_i
                Return
            End If
        Next
    End Sub

    ' The base account says which classifications the account MAY have; the others are forbidden (box off, combo empty). An
    ' allowed one is optional and NOT suggested: the operator ticks the box, then picks the code. The economic one never goes
    ' without the functional one (an account that has it has the functional one too).
    Private Sub ApplyBaseFlags(k_nom As ContNomenclator)
        Dim k_before As Boolean = _flagging
        _flagging = True
        Try
            _allowF = k_nom IsNot Nothing AndAlso k_nom.CereClsfF
            _allowE = k_nom IsNot Nothing AndAlso k_nom.CereClsfE
            If Not _allowF Then chkClsfF.Checked = False
            If Not _allowE OrElse Not chkClsfF.Checked Then chkClsfE.Checked = False
            SyncClsfState()
        Finally
            _flagging = k_before
        End Try
    End Sub

    ' A box works only while editing and inside what the base account allows; its combo only while the box is ticked, and an
    ' unticked box leaves its combo empty (null). Reading mode only disables, it never clears a shown value.
    Private Sub SyncClsfState()
        Dim k_before As Boolean = _flagging
        _flagging = True
        Try
            Dim k_edit As Boolean = _current IsNot Nothing AndAlso _editing
            chkClsfF.Enabled = k_edit AndAlso _allowF
            chkClsfE.Enabled = k_edit AndAlso _allowE AndAlso chkClsfF.Checked
            cboClsfF.Enabled = chkClsfF.Enabled AndAlso chkClsfF.Checked
            cboClsfE.Enabled = chkClsfE.Enabled AndAlso chkClsfE.Checked
            If Not chkClsfF.Checked Then cboClsfF.SelectedIndex = -1
            If Not chkClsfE.Checked Then cboClsfE.SelectedIndex = -1
            cboClsfF.PlaceholderText = If(_allowF OrElse _current Is Nothing, String.Empty, "nu se aplică acestui cont")
            cboClsfE.PlaceholderText = If(_allowE OrElse _current Is Nothing, String.Empty, "nu se aplică acestui cont")
        Finally
            _flagging = k_before
        End Try
    End Sub

    ' The symbol the server will compose, shown while the operator types.
    Private Sub UpdateSimbol()
        Dim k_nom As ContNomenclator = TryCast(cboCont.SelectedItem, ContNomenclator)
        If k_nom Is Nothing OrElse _ctx Is Nothing OrElse _ctx.Subunitate Is Nothing Then
            lblSimbol.Text = If(_current Is Nothing, String.Empty, _current.Simbol)
            Return
        End If
        Dim k_text As String = k_nom.Simbol.ToString(CultureInfo.InvariantCulture) & CurrentSs()
        k_text &= TryCast(cboClsfF.SelectedItem, ClsfCont)?.Cod
        k_text &= TryCast(cboClsfE.SelectedItem, ClsfCont)?.Cod
        Dim k_analitic As String = txtAnalitic.Text.Trim().ToUpperInvariant()
        If k_analitic.Length > 0 Then k_text &= "." & k_analitic
        lblSimbol.Text = k_text
    End Sub

    Private Sub CboCont_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboCont.SelectedIndexChanged
        Try
            If _loading OrElse _current Is Nothing Then Return
            Dim k_nom As ContNomenclator = TryCast(cboCont.SelectedItem, ContNomenclator)
            ApplyBaseFlags(k_nom)
            ' A new account takes the name and the type of the base account until the operator writes its own.
            If k_nom IsNot Nothing AndAlso _current.IdCont = 0 Then
                If txtDenumire.Text.Trim().Length = 0 Then txtDenumire.Text = k_nom.Denumire
                If cboFel.SelectedIndex < 0 Then cboFel.SelectedIndex = Math.Max(-1, "ABP".IndexOf(k_nom.Tip, StringComparison.Ordinal))
            End If
            UpdateSimbol()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.CboCont_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' A box ticked: its combo works and the operator fills it (nothing is proposed). A box unticked: its combo is switched off and
    ' emptied; unticking the functional one drops the economic one too.
    Private Sub ChkClsf_CheckedChanged(sender As Object, e As EventArgs) Handles chkClsfF.CheckedChanged, chkClsfE.CheckedChanged
        Try
            If _loading OrElse _flagging OrElse _current Is Nothing Then Return
            _flagging = True
            Try
                If Not chkClsfF.Checked Then chkClsfE.Checked = False
                SyncClsfState()
            Finally
                _flagging = False
            End Try
            UpdateSimbol()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.ChkClsf_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub Field_Changed(sender As Object, e As EventArgs) _
        Handles cboClsfF.SelectedIndexChanged, cboClsfE.SelectedIndexChanged, cboSector.SelectedIndexChanged,
                cboSursa.SelectedIndexChanged, txtAnalitic.TextChanged,
                txtDenumire.TextChanged, cboFel.SelectedIndexChanged,
                txtSoldD.TextChanged, txtSoldC.TextChanged, txtRulajD.TextChanged, txtRulajC.TextChanged
        Try
            If _loading OrElse _flagging OrElse _current Is Nothing Then Return
            UpdateSimbol()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.Field_Changed", ex)
        End Try
    End Sub

    ' ── Buttons ─────────────────────────────────────────────────────────────────

    Private Async Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _ctx Is Nothing OrElse _ctx.Subunitate Is Nothing Then Return
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            _currentNode = Nothing
            ShowCont(New ContPlan())
            cboCont.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.BtnAdauga_Click", ex)
        End Try
    End Sub

    ' «Modificare»: only now the fields of the shown account can be changed (an account was once edited by accident).
    Private Sub BtnModifica_Click(sender As Object, e As EventArgs) Handles btnModifica.Click
        Try
            If _current Is Nothing OrElse _current.IdCont = 0 OrElse _busy OrElse _editing Then Return
            _editing = True
            ApplyEditState()
            cboCont.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.BtnModifica_Click", ex)
        End Try
    End Sub

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            If _current Is Nothing OrElse _ctx Is Nothing Then Return
            Dim k_original As ContPlan = If(_current.IdCont > 0, _ctx.Plan.Conturi.FirstOrDefault(Function(k_c) k_c.IdCont = _current.IdCont), Nothing)
            ShowCont(k_original)
            RaiseEvent StatusMessage(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.BtnRenunta_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            If _current Is Nothing OrElse _current.IdCont = 0 OrElse _busy OrElse _ctx Is Nothing Then Return
            If KBotMessage.Show(Me, $"Ștergeți contul «{_current.Simbol}»?" & vbLf &
                                "Un cont cu operații, în șabloane sau cu sold inițial nu se șterge; atunci îl marcați inactiv.",
                                "Ștergere cont", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            Dim k_id As Integer = _current.IdCont
            SetBusy(True, Nothing)
            Try
                Await _ctx.Gate.RunAsync(Function() _ctx.Api.DeleteContAsync(k_id, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return
            _dirty = False
            RaiseEvent PlanChanged(Nothing)
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContConturiView.BtnSterge_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Ștergere cont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.BtnSterge_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Contul nu a putut fi șters. Detalii în jurnalul de erori.",
                                 "Ștergere cont", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _busy Then Return
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>Writes the account and its balances. True = saved. Every failure is shown here.</summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        If _current Is Nothing OrElse _ctx Is Nothing OrElse _ctx.Subunitate Is Nothing Then Return True
        Try
            Dim k_nom As ContNomenclator = TryCast(cboCont.SelectedItem, ContNomenclator)
            Dim k_analitic As String = txtAnalitic.Text.Trim().ToUpperInvariant()
            Dim k_sold As New SoldCont()
            Dim k_problem As String = Nothing
            If k_nom Is Nothing Then
                k_problem = "Alegeți contul de bază."
                cboCont.Focus()
            ElseIf CurrentSs().Length = 0 Then
                k_problem = "Alegeți sectorul și sursa contului."
                cboSector.Focus()
            ElseIf chkClsfF.Checked AndAlso cboClsfF.SelectedItem Is Nothing Then
                k_problem = "Alegeți clasificația funcțională sau debifați-o."
                cboClsfF.Focus()
            ElseIf chkClsfE.Checked AndAlso cboClsfE.SelectedItem Is Nothing Then
                k_problem = "Alegeți clasificația economică sau debifați-o."
                cboClsfE.Focus()
            ElseIf k_analitic.Length > 0 AndAlso Not AnaliticRegex.IsMatch(k_analitic) Then
                k_problem = "Analiticul poate conține doar cifre, litere mari și «_» (cel mult 10 caractere)."
                txtAnalitic.Focus()
            ElseIf txtDenumire.Text.Trim().Length = 0 Then
                k_problem = "Denumirea contului este obligatorie."
                txtDenumire.Focus()
            ElseIf cboFel.SelectedItem Is Nothing Then
                k_problem = "Alegeți felul contului."
                cboFel.Focus()
            ElseIf Not TryAmount(txtSoldD.Text, k_sold.SoldDebitor) Then
                k_problem = "Soldul inițial debitor trebuie să fie un număr, cel puțin zero."
                txtSoldD.Focus()
            ElseIf Not TryAmount(txtSoldC.Text, k_sold.SoldCreditor) Then
                k_problem = "Soldul inițial creditor trebuie să fie un număr, cel puțin zero."
                txtSoldC.Focus()
            ElseIf Not TryAmount(txtRulajD.Text, k_sold.RulajDebitor) Then
                k_problem = "Rulajul cumulat debitor trebuie să fie un număr, cel puțin zero."
                txtRulajD.Focus()
            ElseIf Not TryAmount(txtRulajC.Text, k_sold.RulajCreditor) Then
                k_problem = "Rulajul cumulat creditor trebuie să fie un număr, cel puțin zero."
                txtRulajC.Focus()
            End If
            If k_problem IsNot Nothing Then
                KBotMessage.Show(Me, k_problem, "Salvare cont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim k_toSave As New ContSave() With {
                .IdCont = If(_current.IdCont > 0, CType(_current.IdCont, Integer?), Nothing),
                .IdSubunitate = _ctx.Subunitate.IdSubunitate, .Cont = k_nom.Simbol, .SursaSector = CurrentSs(), .Analitic = k_analitic,
                .ClsfF = If(chkClsfF.Checked, If(TryCast(cboClsfF.SelectedItem, ClsfCont)?.Cod, String.Empty), String.Empty),
                .ClsfE = If(chkClsfE.Checked, If(TryCast(cboClsfE.SelectedItem, ClsfCont)?.Cod, String.Empty), String.Empty),
                .Denumire = txtDenumire.Text.Trim(), .Fel = CStr(cboFel.SelectedItem),
                .Extra = _current.Extra, .Activ = _current.Activ}   ' not on screen (no check boxes): an account keeps what it has; a new one is active

            SetBusy(True, Nothing)
            Dim k_saved As ContSalvat
            Try
                k_saved = Await SendSaveAsync(k_toSave).ConfigureAwait(True)
                If k_saved IsNot Nothing Then
                    k_sold.IdCont = k_saved.IdCont
                    Await SaveSoldIfChangedAsync(k_sold).ConfigureAwait(True)
                End If
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed OrElse k_saved Is Nothing Then Return False
            _dirty = False
            RaiseEvent PlanChanged(k_saved.IdCont)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("PlanContConturiView.SaveAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Salvare cont", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Contul nu a putut fi salvat. Detalii în jurnalul de erori.",
                                 "Salvare cont", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        End Try
    End Function

    ' The balances go with the account, but only when they differ from what is stored.
    Private Async Function SaveSoldIfChangedAsync(k_sold As SoldCont) As Task
        Dim k_stored As SoldCont = Nothing
        _ctx.Solduri.TryGetValue(k_sold.IdCont, k_stored)
        If k_sold.SameAmounts(k_stored) Then Return
        Await _ctx.Gate.RunAsync(Function() _ctx.Api.SaveSoldAsync(k_sold, _ctx.An, CancellationToken.None)).ConfigureAwait(True)
        _ctx.Solduri(k_sold.IdCont) = k_sold
    End Function

    ' The save, once; when the server asks to confirm an analytic on a 401* / expense / revenue account, the operator is
    ' asked and the save is sent again confirmed. Nothing = the operator said no.
    Private Async Function SendSaveAsync(k_toSave As ContSave) As Task(Of ContSalvat)
        Dim k_question As String = Nothing
        Try
            Return Await _ctx.Gate.RunAsync(Function() _ctx.Api.SaveContAsync(k_toSave, False, CancellationToken.None)).ConfigureAwait(True)
        Catch ex As ApiException When String.Equals(ex.Reason, ConturiReasons.ConfirmareAnalitic, StringComparison.Ordinal)
            k_question = ex.Message   ' no Await inside a Catch: the question is asked below
        End Try
        If KBotMessage.Show(Me, k_question, "Salvare cont", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return Nothing
        Return Await _ctx.Gate.RunAsync(Function() _ctx.Api.SaveContAsync(k_toSave, True, CancellationToken.None)).ConfigureAwait(True)
    End Function

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetDirty(k_value As Boolean)
        _dirty = k_value
        RefreshButtons()
    End Sub

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        RefreshButtons()
        RaiseEvent BusyChanged(k_busy)
        If k_status IsNot Nothing Then RaiseEvent StatusMessage(k_status)
    End Sub

    Private Sub RefreshButtons()
        Dim k_has As Boolean = _current IsNot Nothing
        Dim k_sub As Boolean = _ctx IsNot Nothing AndAlso _ctx.Subunitate IsNot Nothing
        btnSalveaza.Enabled = k_has AndAlso _editing AndAlso _dirty AndAlso Not _busy
        btnRenunta.Enabled = k_has AndAlso _editing AndAlso Not _busy
        btnModifica.Enabled = k_has AndAlso Not _editing AndAlso _current.IdCont > 0 AndAlso Not _busy
        btnSterge.Enabled = k_has AndAlso Not _editing AndAlso _current.IdCont > 0 AndAlso Not _busy
        btnAdauga.Enabled = k_sub AndAlso Not _busy
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_p As ThemePalette = k_scheme.Palette
            BackColor = k_p.SurfaceAltColor
            For Each k_c As Control In {tlyRoot, tlyCont, tlyDetalii, tlyButoane}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
            lblSimbol.ForeColor = k_p.TextColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            For Each k_b As Button In {btnAdauga, btnModifica, btnRenunta, btnSterge}
                ButtonStyles.ApplySecondary(k_b, k_scheme)
            Next
            If _ctx IsNot Nothing Then BuildTree(If(_current IsNot Nothing AndAlso _current.IdCont > 0, CType(_current.IdCont, Integer?), Nothing))
        Catch ex As Exception
            GlobalErrorLog.Write("PlanContConturiView.ApplyTheme", ex)
        End Try
    End Sub

End Class
