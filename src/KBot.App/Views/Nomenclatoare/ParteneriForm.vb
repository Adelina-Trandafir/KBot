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
''' «Adaugare / editare parteneri» (slice 0087-02, operator 26.09.2026): the Access partner form
''' with the list turned into a tree on the left (code in the first column, name in the second,
''' search in the header) and the partner's details on the right, with its «Coduri angajament»
''' typed straight in the grid («+» in the footer adds a row, «✕» removes one). The filter
''' «Ascunde partenerii fara activitate» is kept; the Burse one is not. A new partner goes into the
''' unit of the working sector-source.
''' <para>Slice 0093: the server sends only Tip "1" partners with a fiscal code other than the
''' unit's own, not hidden, one per fiscal code (so the «show hidden» filter and the Tip field are
''' gone). Leaving the fiscal code (or Enter) asks ANAF and fills the name and the address; the bank
''' follows the IBAN (AVACONT_COMUN.BIC) while it is empty or was filled that way; a fiscal code
''' another partner already has is refused.</para>
''' </summary>
Public Class ParteneriForm

    Private Const ColClsf As String = "clasificatie"
    Private Const ColCont As String = "cont_bancar"
    Private Const ColCodAng As String = "cod_ang"
    Private Const ColCodInd As String = "cod_ind"
    Private Const ColDelete As String = "sterge"
    Private Const ColId As String = "id"
    Private Const ColIdLeg As String = "id_leg"
    Private Const DeleteCaption As String = "✕"
    ' Slice AV01-08: the balances grid (gridSolduri).
    Private Const ColSCont As String = "cont"
    Private Const ColSSid As String = "sid"
    Private Const ColSSic As String = "sic"
    Private Const ColSRpd As String = "rpd"
    Private Const ColSRpc As String = "rpc"
    Private Const ColSIdCont As String = "id_cont"

    Private ReadOnly _api As INomenclatoareApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _ss As String
    Private ReadOnly _deletedCodes As New List(Of Integer)()
    Private ReadOnly _deletedLegs As New List(Of Integer)()
    Private _catalog As ParteneriCatalog
    Private _current As Partener
    Private _currentNode As AdvancedTreeControl.TreeItem
    Private _dirty As Boolean
    Private _loading As Boolean
    Private _busy As Boolean
    Private _closeAfterSave As Boolean
    ' The fiscal code (digits only) last shown or looked up: leaving the field unchanged asks nothing.
    Private _cfChecked As String = String.Empty
    ' The bank name last derived from the IBAN: while «Banca» still holds it, a new IBAN replaces it.
    Private _autoBanca As String = String.Empty
    ' Slice AV01-08: the opening balances of the partner (the accounts API is Nothing = no balances section).
    Private ReadOnly _conturiApi As IConturiApi
    Private ReadOnly _session As SessionContext
    Private _conturi As List(Of ContDisponibil)
    Private _severalChapters As Boolean
    Private ReadOnly _contByLabel As New Dictionary(Of String, ContDisponibil)(StringComparer.Ordinal)
    Private _soldFilling As Boolean
    ' True once the partner's balances are on screen (a failed read keeps the grid off, so an empty grid is never saved over the stored rows).
    Private _soldReady As Boolean
    Private _soldTouched As Boolean
    Private _soldVersion As Integer
    ' The three choices of the popup of the list header (slice AV01-09). The list always has the partners that appear on a document (DDF / ORD);
    ' each choice ADDS a kind: the ones without activity, the ones with an opening balance this year, the ones with commitment codes.
    Private _showNoActivity As Boolean
    Private _showWithSold As Boolean
    Private _showWithCodes As Boolean
    Private _withSold As New HashSet(Of Integer)()
    Private Const MenuNoActivity As String = "fara_activitate"
    Private Const MenuWithSold As String = "cu_sold"
    Private Const MenuWithCodes As String = "cu_angajamente"

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
        _ss = String.Empty
    End Sub

    ''' <param name="ss">The working sector-source; a new partner goes into its unit.</param>
    ''' <param name="conturiApi">The accounts API of the balances grid; Nothing = the window has no balances.</param>
    ''' <param name="session">The working year of the balances.</param>
    Public Sub New(api As INomenclatoareApi, gate As ReauthGate, ss As String,
                   Optional conturiApi As IConturiApi = Nothing, Optional session As SessionContext = Nothing)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(gate)
        InitializeComponent()
        _api = api
        _gate = gate
        _ss = If(ss, String.Empty)
        _conturiApi = conturiApi
        _session = session
        If conturiApi Is Nothing OrElse session Is Nothing Then HideSolduri()
    End Sub

    Private Sub ParteneriForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            ' KBotTextField forwards TextChanged but NOT Leave (see DdfEditForm_Load).
            AddHandler txtCodFiscal.InnerTextBox.Leave, AddressOf TxtCodFiscal_Leave
            ShowPartner(Nothing)
            LoadCatalog(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.ParteneriForm_Load", ex)
        End Try
    End Sub

    ' ── List ────────────────────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadCatalog(selectId As Integer?)
        Try
            SetBusy(True, "Se încarcă partenerii…")
            Dim catalog As ParteneriCatalog = Await _gate.RunAsync(
                Function() _api.GetParteneriAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _catalog = catalog
            Await ReadWithSoldAsync().ConfigureAwait(True)
            If IsDisposed Then Return
            BuildTree(selectId)
            Dim selected As Partener = If(selectId.HasValue,
                catalog.Partners.FirstOrDefault(Function(p) Nullable.Equals(p.IdPartener, selectId)), Nothing)
            ShowPartner(selected)
            SetStatus($"{catalog.Partners.Count} parteneri.")
        Catch ex As ApiException
            GlobalErrorLog.Write("ParteneriForm.LoadCatalog", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.LoadCatalog", ex)
            If Not IsDisposed Then SetStatus("Partenerii nu au putut fi încărcați. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    ' The partners with an opening balance this year, for the «cu sold inițial» choice. A failed read leaves the choice empty and says so (the list itself still loads).
    Private Async Function ReadWithSoldAsync() As Task
        _withSold.Clear()
        If Not SoldAvailable() Then Return
        Try
            Dim k_an As Integer = _session.An
            Dim k_ids As IReadOnlyList(Of Integer) = Await _gate.RunAsync(
                Function() _conturiApi.GetParteneriCuSolduriAsync(k_an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _withSold = New HashSet(Of Integer)(k_ids)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.ReadWithSoldAsync", ex)
            If Not IsDisposed Then SetStatus("Partenerii cu sold inițial nu au putut fi aflați. Detalii în jurnalul de erori.")
        End Try
    End Function

    Private Sub BuildTree(selectId As Integer?)
        tree.Clear()
        _currentNode = Nothing
        If _catalog Is Nothing Then Return
        Dim dimColor As Color = ThemeManager.Current.Palette.TextDimColor
        Dim severalUnits As Boolean = _catalog.Partners.Select(Function(p) p.Ss).Distinct(StringComparer.Ordinal).Count() > 1
        Dim shown As Integer = 0
        For Each p As Partener In _catalog.Partners.OrderBy(Function(x) x.Denumire, StringComparer.CurrentCultureIgnoreCase)
            If Not IsListed(p, selectId) Then Continue For
            Dim name As String = Escape(p.Denumire)
            If severalUnits Then name &= $" ({p.Ss})"
            Dim node As AdvancedTreeControl.TreeItem = tree.AddItem(
                $"P|{p.IdPartener}", $"<b>{Escape(p.CodPartener)}</b>~~~{name}")
            node.Tag = p
            node.Tooltip = If(String.IsNullOrEmpty(p.CodFiscal), p.Denumire, $"{p.Denumire}{vbLf}CF {p.CodFiscal}")
            If p.Ascuns Then
                node.Italic = True
                node.NodeForeColor = dimColor
            End If
            If selectId.HasValue AndAlso p.IdPartener = selectId.Value Then
                tree.SelectAndReveal(node)
                _currentNode = node
            End If
            shown += 1
        Next
        tree.HeaderCaption = $" PARTENERI ({shown})"
        tree.Invalidate()
    End Sub

    Private Shared Function Escape(text As String) As String
        Return If(text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    ' Is the partner in the list? The ones on a document always are; the popup of the header adds the other kinds. The partner being shown (just
    ' saved, or picked) never disappears under the operator.
    Private Function IsListed(k_p As Partener, k_selectId As Integer?) As Boolean
        If k_p.Activ OrElse _showNoActivity Then Return True
        If k_selectId.HasValue AndAlso Nullable.Equals(k_p.IdPartener, k_selectId) Then Return True
        If _showWithCodes AndAlso k_p.Coduri.Count > 0 Then Return True
        Return _showWithSold AndAlso k_p.IdPartener.HasValue AndAlso _withSold.Contains(k_p.IdPartener.Value)
    End Function

    ' The icon of the list header opens the menu of the three kinds. A second press on the icon closes it (the press already did, so without
    ' the guard it would reopen at once).
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            If CustomPopup.ClosedJustNow Then Return
            Dim k_anchor As Rectangle = tree.HeaderRightIconRect
            If k_anchor.IsEmpty Then Return
            Dim k_rows As New List(Of CustomPopupItem) From {
                New CustomPopupItem(MenuNoActivity, "Arată partenerii fără &activitate") With {.Checked = _showNoActivity},
                New CustomPopupItem(MenuWithSold, "Arată partenerii cu &sold inițial") With {.Checked = _showWithSold},
                New CustomPopupItem(MenuWithCodes, "Arată partenerii cu a&ngajamente") With {.Checked = _showWithCodes}}
            ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
            Dim k_menu As New CustomPopup(k_rows)
            AddHandler k_menu.ItemClicked, AddressOf TreeMenu_ItemClicked
            k_menu.ShowBelow(tree, k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    Private Sub TreeMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            Select Case e.Item.Key
                Case MenuNoActivity
                    _showNoActivity = Not _showNoActivity
                Case MenuWithSold
                    _showWithSold = Not _showWithSold
                Case MenuWithCodes
                    _showWithCodes = Not _showWithCodes
                Case Else
                    Return
            End Select
            BuildTree(_current?.IdPartener)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.TreeMenu_ItemClicked", ex)
        End Try
    End Sub

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If pNode Is Nothing OrElse pNode Is _currentNode Then Return
            Dim previous As AdvancedTreeControl.TreeItem = _currentNode
            ChangeSelection(pNode, previous)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' UI boundary: asks about unsaved changes, then shows the partner.
    Private Async Sub ChangeSelection(node As AdvancedTreeControl.TreeItem, previous As AdvancedTreeControl.TreeItem)
        Try
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then
                If previous IsNot Nothing Then tree.SelectAndReveal(previous)
                Return
            End If
            _currentNode = node
            ShowPartner(TryCast(node.Tag, Partener))
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.ChangeSelection", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Unsaved changes: Yes saves (False when the save fails), No drops them, Cancel stays. True =
    ''' the caller may go on.
    ''' </summary>
    Private Async Function ConfirmLeaveAsync() As Task(Of Boolean)
        If _busy Then Return False
        If Not _dirty Then Return True
        Dim answer As DialogResult = KBotMessage.Show(Me, "Partenerul are modificări nesalvate." & vbLf & "Le salvați?",
                                                      "Parteneri", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
        If answer = DialogResult.Cancel Then Return False
        If answer = DialogResult.No Then Return True
        Return Await SaveAsync().ConfigureAwait(True)
    End Function

    ' ── Details ─────────────────────────────────────────────────────────────────

    ''' <summary>Fills the right side from a partner (Nothing = empty, nothing selected).</summary>
    Private Sub ShowPartner(p As Partener)
        _loading = True
        Try
            _current = p
            _deletedCodes.Clear()
            _deletedLegs.Clear()
            txtCod.Text = If(p?.CodPartener, String.Empty)
            txtCodFiscal.Text = If(p?.CodFiscal, String.Empty)
            _cfChecked = NormalizeCf(p?.CodFiscal)
            Dim derived As String = BankFromIban(p?.ContIban)
            _autoBanca = If(derived.Length > 0 AndAlso String.Equals(derived, p?.Banca, StringComparison.Ordinal), derived, String.Empty)
            txtDenumire.Text = If(p?.Denumire, String.Empty)
            txtIban.Text = If(p?.ContIban, String.Empty)
            txtBanca.Text = If(p?.Banca, String.Empty)
            txtAdresa.Text = If(p?.Adresa, String.Empty)
            chkAscuns.Checked = p IsNot Nothing AndAlso p.Ascuns
            FillCodes(p)
            ShowSolduri(p)
            tlyDetalii.Enabled = p IsNot Nothing
            UpdateCodesState()
        Finally
            _loading = False
        End Try
        SetDirty(p IsNot Nothing AndAlso Not p.IdPartener.HasValue)
    End Sub

    Private Sub FillCodes(p As Partener)
        gridCoduri.BeginUpdate()
        Try
            gridCoduri.ClearRows()
            If p Is Nothing Then Return
            For Each c As PartenerCod In p.Coduri
                Dim row As KBotDataRow = gridCoduri.AddRow()
                row(ColId) = If(c.Id.HasValue, CObj(c.Id.Value), Nothing)
                row(ColIdLeg) = If(c.IdLeg.HasValue, CObj(c.IdLeg.Value), Nothing)
                row(ColClsf) = c.Clsf
                row(ColCont) = c.ContBancar
                row(ColCodAng) = c.CodAng
                row(ColCodInd) = c.CodInd
                row(ColDelete) = DeleteCaption
            Next
        Finally
            gridCoduri.EndUpdate()
        End Try
        gridCoduri.ClearDirty()
    End Sub

    ' Slice 0121-03: the codes belong to the supplier's fiscal code. They can be edited only while a fiscal code is typed
    ' and, for a saved partner, is still the saved one (a changed code is saved first; the codes of the new code are then read).
    Private Function CodesAllowed() As Boolean
        If _current Is Nothing Then Return False
        Dim k_cf As String = NormalizeCf(txtCodFiscal.Text)
        If k_cf.Length = 0 Then Return False
        Return Not _current.IdPartener.HasValue OrElse
               String.Equals(k_cf, NormalizeCf(_current.CodFiscal), StringComparison.Ordinal)
    End Function

    Private Sub UpdateCodesState()
        gridCoduri.Enabled = CodesAllowed()
        If _current Is Nothing Then
            lblCoduri.Text = "Coduri angajament"
        ElseIf NormalizeCf(txtCodFiscal.Text).Length = 0 Then
            lblCoduri.Text = "Coduri angajament — introduceți întâi codul fiscal"
        ElseIf Not CodesAllowed() Then
            lblCoduri.Text = "Coduri angajament — salvați întâi noul cod fiscal"
        ElseIf _current.IdPartener.HasValue Then
            lblCoduri.Text = $"Coduri angajament — {_current.Denumire}"
        Else
            lblCoduri.Text = "Coduri angajament — partener nou"
        End If
    End Sub

    Private Sub Field_TextChanged(sender As Object, e As EventArgs) _
        Handles txtCod.TextChanged, txtCodFiscal.TextChanged, txtDenumire.TextChanged,
                txtIban.TextChanged, txtBanca.TextChanged, txtAdresa.TextChanged, chkAscuns.CheckedChanged
        Try
            If Not _loading AndAlso _current IsNot Nothing Then SetDirty(True)
            If Not _loading Then UpdateCodesState()
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.Field_TextChanged", ex)
        End Try
    End Sub

    ' ── Fiscal code (ANAF) and bank (IBAN) ──────────────────────────────────────

    ''' <summary>The digits of a fiscal code ("RO 21015411" = "21015411"), as the server compares them.</summary>
    Private Shared Function NormalizeCf(raw As String) As String
        Dim text As String = If(raw, String.Empty).Trim().ToUpperInvariant()
        If text.StartsWith("RO", StringComparison.Ordinal) Then text = text.Substring(2)
        Return New String(text.Where(Function(ch) ch >= "0"c AndAlso ch <= "9"c).ToArray())
    End Function

    ''' <summary>The bank of a Romanian IBAN from AVACONT_COMUN.BIC (characters 5-8), or "".</summary>
    Private Function BankFromIban(iban As String) As String
        If _catalog Is Nothing Then Return String.Empty
        Dim compact As String = New String(If(iban, String.Empty).Where(Function(ch) Not Char.IsWhiteSpace(ch)).ToArray()).ToUpperInvariant()
        If compact.Length < 8 Then Return String.Empty
        Dim bank As String = Nothing
        Return If(_catalog.Bic.TryGetValue(compact.Substring(4, 4), bank), If(bank, String.Empty), String.Empty)
    End Function

    ''' <summary>
    ''' Why <paramref name="cf"/> cannot be this partner's code (the unit's own, or another listed
    ''' partner's), or Nothing. The server checks again against every partner, hidden ones too.
    ''' </summary>
    Private Function FiscalCodeProblem(cf As String) As String
        If _catalog Is Nothing OrElse cf.Length = 0 Then Return Nothing
        If String.Equals(cf, _catalog.CfUnitate, StringComparison.Ordinal) Then
            Return $"Codul fiscal {cf} este chiar al unității; unitatea nu poate fi propriul partener."
        End If
        Dim other As Partener = _catalog.Partners.FirstOrDefault(
            Function(x) String.Equals(NormalizeCf(x.CodFiscal), cf, StringComparison.Ordinal) AndAlso
                        Not (_current IsNot Nothing AndAlso _current.IdPartener.HasValue AndAlso
                             Nullable.Equals(x.IdPartener, _current.IdPartener)))
        If other Is Nothing Then Return Nothing
        Return $"Există deja un partener cu codul fiscal {other.CodFiscal}: {other.CodPartener} — {other.Denumire}." & vbLf &
               "Poate exista un singur partener pentru un cod fiscal."
    End Function

    Private Sub TxtIban_TextChanged(sender As Object, e As EventArgs) Handles txtIban.TextChanged
        Try
            If _loading OrElse _current Is Nothing Then Return
            Dim bank As String = BankFromIban(txtIban.Text)
            If bank.Length = 0 Then Return
            Dim current As String = txtBanca.Text.Trim()
            If current.Length = 0 OrElse String.Equals(current, _autoBanca, StringComparison.Ordinal) Then
                txtBanca.Text = bank
                _autoBanca = bank
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.TxtIban_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtCodFiscal_Leave(sender As Object, e As EventArgs)
        Try
            LookupFiscalCode()
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.TxtCodFiscal_Leave", ex)
        End Try
    End Sub

    Private Sub TxtCodFiscal_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtCodFiscal.FieldKeyDown
        Try
            If e.KeyCode <> Keys.Enter Then Return
            e.SuppressKeyPress = True
            LookupFiscalCode()
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.TxtCodFiscal_FieldKeyDown", ex)
        End Try
    End Sub

    ' UI boundary: a changed fiscal code is checked, then ANAF fills the name and the address (new
    ' partner or existing one). The same code left twice asks only once.
    Private Async Sub LookupFiscalCode()
        Try
            If _current Is Nothing OrElse _loading OrElse _busy OrElse _api Is Nothing Then Return
            Dim cf As String = NormalizeCf(txtCodFiscal.Text)
            If String.Equals(cf, _cfChecked, StringComparison.Ordinal) Then Return
            _cfChecked = cf
            If cf.Length = 0 Then Return
            If cf.Length < 2 OrElse cf.Length > 10 Then
                KBotMessage.Show(Me, $"«{txtCodFiscal.Text.Trim()}» nu este un cod fiscal valid.",
                                 "Cod fiscal", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Dim problem As String = FiscalCodeProblem(cf)
            If problem IsNot Nothing Then
                KBotMessage.Show(Me, problem, "Cod fiscal", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            Dim info As PartenerAnaf
            SetBusy(True, "Se caută codul fiscal la ANAF…")
            Try
                info = Await _gate.RunAsync(Function() _api.GetPartenerAnafAsync(cf, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed OrElse _current Is Nothing Then Return
            ' The operator may have typed another code while ANAF answered.
            If Not String.Equals(NormalizeCf(txtCodFiscal.Text), cf, StringComparison.Ordinal) Then Return
            txtDenumire.Text = info.Denumire
            txtAdresa.Text = info.Adresa
            SetStatus($"Denumirea și adresa au fost completate de la ANAF (CUI {info.Cui}).")
        Catch ex As ApiException
            GlobalErrorLog.Write("ParteneriForm.LookupFiscalCode", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Cod fiscal", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.LookupFiscalCode", ex)
            If Not IsDisposed Then SetStatus("Căutarea la ANAF a eșuat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Private Sub GridCoduri_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridCoduri.CellValidating
        Try
            e.ProposedValue = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridCoduri_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridCoduri.CellValueChanged
        Try
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridCoduri_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridCoduri.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy OrElse Not CodesAllowed() Then Return
            If Not gridCoduri.CommitPendingEdit() Then Return
            Dim row As KBotDataRow = gridCoduri.AddRow()
            row(ColDelete) = DeleteCaption
            row.IsDirty = True
            SetDirty(True)
            Dim index As Integer = gridCoduri.RowCount - 1
            gridCoduri.EnsureVisible(index)
            gridCoduri.EditCell(ColCodAng, index)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_FooterRightIconClicked", ex)
        End Try
    End Sub

    ' Esc twice in a row: a row with neither an id nor a link is one the operator just added with «+» and it goes away; the others go back as they were (the grid's default).
    Private Sub GridCoduri_RowCancelRequested(sender As Object, e As KBotRowCancelEventArgs) Handles gridCoduri.RowCancelRequested
        Try
            If _busy OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridCoduri.RowCount Then Return
            If gridCoduri(ColId, e.RowIndex) IsNot Nothing OrElse gridCoduri(ColIdLeg, e.RowIndex) IsNot Nothing Then Return
            e.Handled = True
            gridCoduri.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_RowCancelRequested", ex)
        End Try
    End Sub

    Private Sub GridCoduri_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridCoduri.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridCoduri.RowCount Then Return
            Dim id As Object = gridCoduri(ColId, e.RowIndex)
            Dim k_idLeg As Object = gridCoduri(ColIdLeg, e.RowIndex)
            If id IsNot Nothing Then
                _deletedCodes.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            ElseIf k_idLeg IsNot Nothing Then
                _deletedLegs.Add(Convert.ToInt32(k_idLeg, CultureInfo.InvariantCulture))
            End If
            gridCoduri.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_ButtonClick", ex)
        End Try
    End Sub

    ' ── Opening balances of the partner (slice AV01-08) ─────────────────────────

    ' Without the accounts API the window has no balances section: its two rows take no room.
    Private Sub HideSolduri()
        tlyDetalii.RowStyles(9).Height = 0
        tlyDetalii.RowStyles(10).Height = 0
        lblSolduri.Visible = False
        gridSolduri.Visible = False
    End Sub

    Private Function SoldAvailable() As Boolean
        Return _conturiApi IsNot Nothing AndAlso _session IsNot Nothing
    End Function

    ' What the combo shows for an account: the symbol and the name, with the chapter when the unit has several subunits.
    Private Function ContLabel(k_simbol As String, k_denumire As String, k_capitol As String) As String
        Dim k_text As String = If(String.IsNullOrWhiteSpace(k_denumire), k_simbol, $"{k_simbol} — {k_denumire}")
        Return If(_severalChapters, $"{k_text} ({k_capitol})", k_text)
    End Function

    ' The active accounts of every subunit, read once; they fill the list of the «Cont» column.
    Private Async Function EnsureConturiAsync() As Task
        If _conturi IsNot Nothing Then Return
        Dim k_list As IReadOnlyList(Of ContDisponibil) = Await _gate.RunAsync(
            Function() _conturiApi.GetConturiDisponibileAsync(CancellationToken.None)).ConfigureAwait(True)
        If IsDisposed Then Return
        _conturi = New List(Of ContDisponibil)(k_list)
        _severalChapters = _conturi.Select(Function(k_c) k_c.Capitol).Distinct(StringComparer.Ordinal).Count() > 1
        _contByLabel.Clear()
        Dim k_items As New List(Of Object)()
        For Each k_c As ContDisponibil In _conturi
            Dim k_label As String = ContLabel(k_c.Simbol, k_c.Denumire, k_c.Capitol)
            If _contByLabel.ContainsKey(k_label) Then Continue For
            _contByLabel(k_label) = k_c
            k_items.Add(k_label)
        Next
        gridSolduri.Column(ColSCont).ComboItems = k_items
    End Function

    ' UI boundary: shows the balances of the partner for the working year. A stale answer (another partner was picked meanwhile) is dropped.
    Private Async Sub ShowSolduri(k_p As Partener)
        Dim k_version As Integer = Interlocked.Increment(_soldVersion)
        Try
            _soldReady = False
            _soldTouched = False
            gridSolduri.Enabled = False
            _soldFilling = True
            Try
                gridSolduri.ClearRows()
            Finally
                _soldFilling = False
            End Try
            If Not SoldAvailable() Then Return
            lblSolduri.Text = If(k_p Is Nothing, "Solduri inițiale", $"Solduri inițiale — anul {_session.An.ToString(CultureInfo.InvariantCulture)}")
            If k_p Is Nothing Then Return
            Await EnsureConturiAsync().ConfigureAwait(True)
            Dim k_rows As IReadOnlyList(Of SoldPartener) = New List(Of SoldPartener)()
            If k_p.IdPartener.HasValue Then
                Dim k_id As Integer = k_p.IdPartener.Value
                Dim k_an As Integer = _session.An
                k_rows = Await _gate.RunAsync(
                    Function() _conturiApi.GetSolduriPartenerAsync(k_id, k_an, CancellationToken.None)).ConfigureAwait(True)
            End If
            If IsDisposed OrElse k_version <> _soldVersion Then Return
            FillSolduri(k_rows)
            _soldReady = True
            gridSolduri.Enabled = True
        Catch ex As ApiException
            GlobalErrorLog.Write("ParteneriForm.ShowSolduri", ex)
            If Not IsDisposed AndAlso k_version = _soldVersion Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.ShowSolduri", ex)
            If Not IsDisposed AndAlso k_version = _soldVersion Then SetStatus("Soldurile inițiale nu au putut fi încărcate. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Private Sub FillSolduri(k_rows As IEnumerable(Of SoldPartener))
        _soldFilling = True
        gridSolduri.BeginUpdate()
        Try
            gridSolduri.ClearRows()
            For Each k_s As SoldPartener In k_rows
                Dim k_label As String = ContLabel(k_s.Simbol, k_s.Denumire, k_s.Capitol)
                ' A balance kept on an account that is out of use is not in the list: it still shows, and keeps its account.
                If Not _contByLabel.ContainsKey(k_label) Then
                    _contByLabel(k_label) = New ContDisponibil() With {.IdCont = k_s.IdCont, .Simbol = k_s.Simbol, .Denumire = k_s.Denumire, .Capitol = k_s.Capitol}
                End If
                Dim k_row As KBotDataRow = gridSolduri.AddRow()
                k_row(ColSIdCont) = k_s.IdCont
                k_row(ColSCont) = k_label
                k_row(ColSSid) = k_s.SoldDebitor
                k_row(ColSSic) = k_s.SoldCreditor
                k_row(ColSRpd) = k_s.RulajDebitor
                k_row(ColSRpc) = k_s.RulajCreditor
                k_row(ColDelete) = DeleteCaption
            Next
        Finally
            gridSolduri.EndUpdate()
            _soldFilling = False
        End Try
        gridSolduri.ClearDirty()
    End Sub

    ' An amount typed by the operator: empty = 0, two decimals, never negative.
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

    Private Shared Function CellAmount(k_cell As Object) As Decimal
        If k_cell Is Nothing Then Return 0D
        If TypeOf k_cell Is Decimal Then Return CDec(k_cell)
        Dim k_v As Decimal
        Return If(TryAmount(Convert.ToString(k_cell, CultureInfo.CurrentCulture), k_v), k_v, 0D)
    End Function

    Private Sub GridSolduri_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridSolduri.CellValidating
        Try
            If _soldFilling Then Return
            If String.Equals(e.ColumnKey, ColSCont, StringComparison.Ordinal) Then
                Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
                If k_text.Length = 0 OrElse Not _contByLabel.ContainsKey(k_text) Then
                    e.Cancel = True
                    SetStatus("Alegeți un cont din listă.")
                Else
                    e.ProposedValue = k_text
                    SetStatus(String.Empty)
                End If
                Return
            End If
            Dim k_value As Decimal
            If TryAmount(Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture), k_value) Then
                e.ProposedValue = k_value
                SetStatus(String.Empty)
            Else
                e.Cancel = True
                SetStatus("Introduceți o sumă: un număr, cel puțin zero.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridSolduri_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridSolduri_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridSolduri.CellValueChanged
        Try
            If _soldFilling Then Return
            If String.Equals(e.ColumnKey, ColSCont, StringComparison.Ordinal) Then
                Dim k_account As ContDisponibil = Nothing
                If _contByLabel.TryGetValue(Convert.ToString(e.NewValue, CultureInfo.CurrentCulture), k_account) Then
                    _soldFilling = True
                    Try
                        gridSolduri.Rows(e.RowIndex)(ColSIdCont) = k_account.IdCont
                    Finally
                        _soldFilling = False
                    End Try
                End If
            End If
            _soldTouched = True
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridSolduri_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridSolduri_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridSolduri.FooterRightIconClicked
        Try
            If _current Is Nothing OrElse _busy OrElse Not _soldReady Then Return
            If _conturi Is Nothing OrElse _conturi.Count = 0 Then
                SetStatus("Planul de conturi nu are conturi active.")
                Return
            End If
            If Not gridSolduri.CommitPendingEdit() Then Return
            Dim k_row As KBotDataRow = gridSolduri.AddRow()
            k_row(ColDelete) = DeleteCaption
            k_row(ColSSid) = 0D
            k_row(ColSSic) = 0D
            k_row(ColSRpd) = 0D
            k_row(ColSRpc) = 0D
            k_row.IsDirty = True
            _soldTouched = True
            SetDirty(True)
            Dim k_index As Integer = gridSolduri.RowCount - 1
            gridSolduri.EnsureVisible(k_index)
            gridSolduri.EditCell(ColSCont, k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridSolduri_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridSolduri_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridSolduri.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy OrElse Not _soldReady Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridSolduri.RowCount Then Return
            gridSolduri.RemoveRowAt(e.RowIndex)
            _soldTouched = True
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridSolduri_ButtonClick", ex)
        End Try
    End Sub

    ' The balances as the grid shows them; the first row without an account (or an account used twice) is reported and opened.
    Private Function ReadSolduri(ByRef k_problem As String) As List(Of SoldPartener)
        Dim k_list As New List(Of SoldPartener)()
        Dim k_seen As New HashSet(Of Integer)()
        For k_i As Integer = 0 To gridSolduri.RowCount - 1
            Dim k_row As KBotDataRow = gridSolduri.Rows(k_i)
            Dim k_label As String = Convert.ToString(k_row(ColSCont), CultureInfo.CurrentCulture).Trim()
            Dim k_account As ContDisponibil = Nothing
            If k_label.Length = 0 OrElse Not _contByLabel.TryGetValue(k_label, k_account) Then
                k_problem = $"Soldurile inițiale, rândul {k_i + 1}: alegeți contul."
            ElseIf Not k_seen.Add(k_account.IdCont) Then
                k_problem = $"Soldurile inițiale, rândul {k_i + 1}: contul {k_account.Simbol} apare de două ori."
            End If
            If k_problem IsNot Nothing Then
                gridSolduri.EnsureVisible(k_i)
                gridSolduri.EditCell(ColSCont, k_i)
                Return k_list
            End If
            k_list.Add(New SoldPartener() With {
                .IdCont = k_account.IdCont, .Simbol = k_account.Simbol, .Denumire = k_account.Denumire, .Capitol = k_account.Capitol,
                .SoldDebitor = CellAmount(k_row(ColSSid)), .SoldCreditor = CellAmount(k_row(ColSSic)),
                .RulajDebitor = CellAmount(k_row(ColSRpd)), .RulajCreditor = CellAmount(k_row(ColSRpc))})
        Next
        Return k_list
    End Function

    ' ── Buttons ─────────────────────────────────────────────────────────────────

    Private Async Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            Dim unit As Integer = If(_catalog?.Partners.FirstOrDefault(Function(p) String.Equals(p.Ss, _ss, StringComparison.Ordinal))?.IdUnitate, 0)
            Dim fresh As New Partener() With {.Ss = _ss, .IdUnitate = unit, .CodPartener = NextCode(_ss)}
            _currentNode = Nothing
            ShowPartner(fresh)
            txtCodFiscal.Focus()
            SetStatus("Partener nou: tastați codul fiscal (denumirea și adresa vin de la ANAF), apoi «Salvare».")
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.BtnAdauga_Click", ex)
        End Try
    End Sub

    ' The next free numeric code among the partners of the sector-source, three digits at least.
    Private Function NextCode(ss As String) As String
        If _catalog Is Nothing Then Return "001"
        Dim max As Integer = 0
        For Each p As Partener In _catalog.Partners.Where(Function(x) String.Equals(x.Ss, ss, StringComparison.Ordinal))
            Dim n As Integer
            If Integer.TryParse(p.CodPartener.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, n) Then max = Math.Max(max, n)
        Next
        Return (max + 1).ToString("000", CultureInfo.InvariantCulture)
    End Function

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            If _current Is Nothing Then Return
            Dim original As Partener = If(_current.IdPartener.HasValue,
                _catalog?.Partners.FirstOrDefault(Function(p) Nullable.Equals(p.IdPartener, _current.IdPartener)), Nothing)
            ShowPartner(original)
            SetStatus(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.BtnRenunta_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            If _current Is Nothing OrElse Not _current.IdPartener.HasValue OrElse _busy Then Return
            If _current.Activ Then
                KBotMessage.Show(Me, "Partenerul apare pe documente (DDF / ORD) și nu se poate șterge." & vbLf &
                                 "Bifați «Partener ascuns» ca să nu mai apară în liste.",
                                 "Ștergere partener", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If KBotMessage.Show(Me, $"Ștergeți partenerul «{_current.Denumire}»?" & vbLf & "Codurile de angajament legate de codul lui fiscal rămân.",
                                "Ștergere partener", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            Dim id As Integer = _current.IdPartener.Value
            SetBusy(True, "Se șterge partenerul…")
            Try
                Await _gate.RunAsync(Function() _api.DeletePartenerAsync(id, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return
            _dirty = False
            LoadCatalog(Nothing)
        Catch ex As ApiException
            GlobalErrorLog.Write("ParteneriForm.BtnSterge_Click", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Ștergere partener", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.BtnSterge_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Partenerul nu a putut fi șters. Detalii în jurnalul de erori.",
                                 "Ștergere partener", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _busy Then Return
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>Writes the partner and its codes. True = saved. Every failure is shown here.</summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        If _current Is Nothing OrElse _api Is Nothing Then Return True
        Try
            If Not gridCoduri.CommitPendingEdit() Then Return False
            If Not gridSolduri.CommitPendingEdit() Then Return False
            Dim problem As String = Nothing
            Dim cf As String = NormalizeCf(txtCodFiscal.Text)
            ' A legacy duplicate (brought over from Access) stays savable while its code is unchanged.
            Dim cfChanged As Boolean = Not _current.IdPartener.HasValue OrElse
                                       Not String.Equals(cf, NormalizeCf(_current.CodFiscal), StringComparison.Ordinal)
            If txtCod.Text.Trim().Length = 0 Then
                problem = "Codul partenerului este obligatoriu."
                txtCod.Focus()
            ElseIf cf.Length = 0 Then
                problem = "Codul fiscal al partenerului este obligatoriu."
                txtCodFiscal.Focus()
            ElseIf cfChanged AndAlso FiscalCodeProblem(cf) IsNot Nothing Then
                problem = FiscalCodeProblem(cf)
                txtCodFiscal.Focus()
            ElseIf txtDenumire.Text.Trim().Length = 0 Then
                problem = "Denumirea partenerului este obligatorie."
                txtDenumire.Focus()
            End If
            Dim codes As List(Of PartenerCod) = If(problem Is Nothing, ReadCodes(problem), Nothing)
            ' Slice AV01-08: the balances are sent only when the operator touched them (and they were read well).
            Dim solduri As List(Of SoldPartener) = If(problem Is Nothing AndAlso _soldReady AndAlso _soldTouched, ReadSolduri(problem), Nothing)
            If problem IsNot Nothing Then
                KBotMessage.Show(Me, problem, "Salvare partener", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            Dim p As New Partener() With {
                .IdPartener = _current.IdPartener, .IdUnitate = _current.IdUnitate,
                .Ss = If(String.IsNullOrEmpty(_current.Ss), _ss, _current.Ss),
                .CodPartener = txtCod.Text.Trim(), .Tip = "1",
                .CodFiscal = txtCodFiscal.Text.Trim(), .Denumire = txtDenumire.Text.Trim(),
                .ContIban = txtIban.Text.Trim(), .Banca = txtBanca.Text.Trim(), .Adresa = txtAdresa.Text.Trim(),
                .Ascuns = chkAscuns.Checked}
            Dim request As New PartenerSaveRequest() With {.Partner = p, .Coduri = codes,
                                                            .CoduriSterse = New List(Of Integer)(_deletedCodes),
                                                            .LegaturiSterse = New List(Of Integer)(_deletedLegs)}
            SetBusy(True, "Se salvează partenerul…")
            Dim id As Integer
            Try
                id = Await _gate.RunAsync(Function() _api.SavePartenerAsync(request, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return True
            If solduri IsNot Nothing Then
                Dim k_an As Integer = _session.An
                Try
                    SetBusy(True, "Se salvează soldurile inițiale…")
                    Await _gate.RunAsync(Function() _conturiApi.SaveSolduriPartenerAsync(id, k_an, solduri, CancellationToken.None)).ConfigureAwait(True)
                Catch ex As ApiException
                    GlobalErrorLog.Write("ParteneriForm.SaveAsync.Solduri", ex)
                    If Not IsDisposed Then
                        KBotMessage.Show(Me, "Partenerul a fost salvat, dar soldurile inițiale nu:" & vbLf & ex.Message,
                                         "Salvare partener", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        _dirty = False
                        LoadCatalog(id)
                    End If
                    Return False
                Finally
                    If Not IsDisposed Then SetBusy(False, Nothing)
                End Try
                If IsDisposed Then Return True
            End If
            _dirty = False
            LoadCatalog(id)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("ParteneriForm.SaveAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Salvare partener", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Partenerul nu a putut fi salvat. Detalii în jurnalul de erori.",
                                 "Salvare partener", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        End Try
    End Function

    ' The codes as the grid shows them; the first row without «Cod ang.» (or a code used twice) is reported and opened.
    Private Function ReadCodes(ByRef problem As String) As List(Of PartenerCod)
        Dim k_list As New List(Of PartenerCod)()
        Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To gridCoduri.RowCount - 1
            Dim row As KBotDataRow = gridCoduri.Rows(i)
            Dim k_codAng As String = Convert.ToString(row(ColCodAng), CultureInfo.CurrentCulture).Trim()
            Dim k_codInd As String = Convert.ToString(row(ColCodInd), CultureInfo.CurrentCulture).Trim()
            If k_codAng.Length = 0 Then
                problem = $"Codurile de angajament, rândul {i + 1}: completați «Cod ang.»."
            ElseIf Not k_seen.Add(k_codAng & "|" & k_codInd) Then
                problem = $"Codurile de angajament, rândul {i + 1}: " &
                          $"{If(k_codInd.Length = 0, k_codAng, k_codAng & "-" & k_codInd)} apare de două ori."
            End If
            If problem IsNot Nothing Then
                gridCoduri.EnsureVisible(i)
                gridCoduri.EditCell(ColCodAng, i)
                Return k_list
            End If
            Dim k_id As Object = row(ColId)
            Dim k_idLeg As Object = row(ColIdLeg)
            k_list.Add(New PartenerCod() With {
                .Id = If(k_id Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(k_id, CultureInfo.InvariantCulture)),
                .IdLeg = If(k_idLeg Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(k_idLeg, CultureInfo.InvariantCulture)),
                .ContBancar = Convert.ToString(row(ColCont), CultureInfo.CurrentCulture).Trim(),
                .CodAng = k_codAng,
                .CodInd = k_codInd})
        Next
        Return k_list
    End Function

    Private Sub BtnIesire_Click(sender As Object, e As EventArgs) Handles btnIesire.Click
        Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If _dirty AndAlso Not _closeAfterSave AndAlso e.CloseReason = CloseReason.UserClosing Then
                Dim answer As DialogResult = KBotMessage.Show(Me, "Partenerul are modificări nesalvate." & vbLf & "Le salvați?",
                                                              "Parteneri", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
                If answer = DialogResult.Cancel Then
                    e.Cancel = True
                ElseIf answer = DialogResult.Yes Then
                    e.Cancel = True
                    SaveThenClose()
                End If
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.OnFormClosing", ex)
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
            GlobalErrorLog.Write("ParteneriForm.SaveThenClose", ex)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetDirty(value As Boolean)
        _dirty = value
        RefreshButtons()
    End Sub

    Private Sub SetBusy(busy As Boolean, status As String)
        _busy = busy
        UseWaitCursor = busy
        RefreshButtons()
        If status IsNot Nothing Then SetStatus(status)
    End Sub

    Private Sub RefreshButtons()
        Dim has As Boolean = _current IsNot Nothing
        btnSalveaza.Enabled = has AndAlso _dirty AndAlso Not _busy
        btnRenunta.Enabled = has AndAlso _dirty AndAlso Not _busy
        btnSterge.Enabled = has AndAlso _current.IdPartener.HasValue AndAlso Not _busy
        btnAdauga.Enabled = Not _busy
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
            For Each c As Control In {tlyMain, pnlCard, tlyBody, tlyLeft, tlyDetalii, tlySubsol}
                c.BackColor = p.SurfaceAltColor
            Next
            lblCoduri.ForeColor = p.TextColor
            lblSolduri.ForeColor = p.TextColor
            lblStare.ForeColor = p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            ButtonStyles.ApplySecondary(btnAdauga, scheme)
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplySecondary(btnSterge, scheme)
            ButtonStyles.ApplySecondary(btnIesire, scheme)
            If _catalog IsNot Nothing Then BuildTree(_current?.IdPartener)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
