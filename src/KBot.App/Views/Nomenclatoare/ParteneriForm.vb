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
    Private Const DeleteCaption As String = "✕"

    ''' <summary>One classification offered in the «Clasificatie» combo of the codes grid.</summary>
    Private NotInheritable Class ClsfChoice
        Public Property IdClsf As Integer
        Public Property Clsf As String = String.Empty
        Public Property Ss As String = String.Empty
        Public Property Denumire As String = String.Empty
        Public Property IdUnitate As Integer?

        Public Overrides Function ToString() As String
            Return If(String.IsNullOrEmpty(Ss), Clsf, $"{Clsf} ({Ss})")
        End Function
    End Class

    Private ReadOnly _api As INomenclatoareApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _ss As String
    Private ReadOnly _deletedCodes As New List(Of Integer)()
    Private _catalog As ParteneriCatalog
    Private _choices As New List(Of ClsfChoice)()
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

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
        _ss = String.Empty
    End Sub

    ''' <param name="ss">The working sector-source; a new partner goes into its unit.</param>
    Public Sub New(api As INomenclatoareApi, gate As ReauthGate, ss As String)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(gate)
        InitializeComponent()
        _api = api
        _gate = gate
        _ss = If(ss, String.Empty)
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
            _choices = catalog.Clasificatii.Select(Function(c) New ClsfChoice() With {
                .IdClsf = c.IdClsf, .Clsf = c.Clsf, .Ss = c.Ss, .Denumire = c.Denumire, .IdUnitate = c.IdUnitate}).ToList()
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

    Private Sub BuildTree(selectId As Integer?)
        tree.Clear()
        _currentNode = Nothing
        If _catalog Is Nothing Then Return
        Dim dimColor As Color = ThemeManager.Current.Palette.TextDimColor
        Dim severalUnits As Boolean = _catalog.Partners.Select(Function(p) p.Ss).Distinct(StringComparer.Ordinal).Count() > 1
        Dim shown As Integer = 0
        For Each p As Partener In _catalog.Partners.OrderBy(Function(x) x.Denumire, StringComparer.CurrentCultureIgnoreCase)
            If Not p.Activ AndAlso chkFaraActivitate.Checked Then Continue For
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

    Private Sub Filters_CheckedChanged(sender As Object, e As EventArgs) Handles chkFaraActivitate.CheckedChanged
        Try
            BuildTree(_current?.IdPartener)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.Filters_CheckedChanged", ex)
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
            tlyDetalii.Enabled = p IsNot Nothing
            lblCoduri.Text = If(p Is Nothing, "Coduri angajament",
                                If(p.IdPartener.HasValue, $"Coduri angajament — {p.Denumire}", "Coduri angajament — partener nou"))
        Finally
            _loading = False
        End Try
        SetDirty(p IsNot Nothing AndAlso Not p.IdPartener.HasValue)
    End Sub

    Private Sub FillCodes(p As Partener)
        gridCoduri.Column(ColClsf).ComboItems = ChoicesFor(p).Cast(Of Object)().ToList()
        gridCoduri.BeginUpdate()
        Try
            gridCoduri.ClearRows()
            If p Is Nothing Then Return
            For Each c As PartenerCod In p.Coduri
                Dim row As KBotDataRow = gridCoduri.AddRow()
                row(ColId) = If(c.Id.HasValue, CObj(c.Id.Value), Nothing)
                row(ColClsf) = If(CObj(_choices.FirstOrDefault(Function(x) x.IdClsf = c.IdClsf)),
                                  New ClsfChoice() With {.IdClsf = c.IdClsf, .Clsf = c.Clsf, .Denumire = c.DenumireClsf})
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

    ' The classifications of the partner's own unit; all of them when the unit has none.
    Private Function ChoicesFor(p As Partener) As List(Of ClsfChoice)
        If p Is Nothing Then Return _choices
        Dim own As List(Of ClsfChoice)
        If p.IdPartener.HasValue Then
            own = _choices.Where(Function(c) c.IdUnitate.HasValue AndAlso c.IdUnitate.Value = p.IdUnitate).ToList()
        Else
            own = _choices.Where(Function(c) String.Equals(c.Ss, p.Ss, StringComparison.Ordinal)).ToList()
        End If
        Return If(own.Count > 0, own, _choices)
    End Function

    Private Sub Field_TextChanged(sender As Object, e As EventArgs) _
        Handles txtCod.TextChanged, txtCodFiscal.TextChanged, txtDenumire.TextChanged,
                txtIban.TextChanged, txtBanca.TextChanged, txtAdresa.TextChanged, chkAscuns.CheckedChanged
        Try
            If Not _loading AndAlso _current IsNot Nothing Then SetDirty(True)
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
            If e.ColumnKey = ColClsf Then
                If TypeOf e.ProposedValue Is ClsfChoice Then Return
                Dim text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
                Dim match As ClsfChoice = ChoicesFor(_current).FirstOrDefault(
                    Function(c) String.Equals(c.ToString(), text, StringComparison.OrdinalIgnoreCase) OrElse
                                String.Equals(c.Clsf, text, StringComparison.OrdinalIgnoreCase))
                If match Is Nothing Then
                    e.Cancel = True
                    SetStatus($"«{text}» nu este o clasificație a unității; alegeți din listă.")
                    Return
                End If
                e.ProposedValue = match
            Else
                e.ProposedValue = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
            End If
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
            If _current Is Nothing OrElse _busy Then Return
            If Not gridCoduri.CommitPendingEdit() Then Return
            Dim row As KBotDataRow = gridCoduri.AddRow()
            row(ColDelete) = DeleteCaption
            row.IsDirty = True
            SetDirty(True)
            Dim index As Integer = gridCoduri.RowCount - 1
            gridCoduri.EnsureVisible(index)
            gridCoduri.EditCell(ColClsf, index)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_FooterRightIconClicked", ex)
        End Try
    End Sub

    Private Sub GridCoduri_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridCoduri.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridCoduri.RowCount Then Return
            Dim id As Object = gridCoduri(ColId, e.RowIndex)
            If id IsNot Nothing Then _deletedCodes.Add(Convert.ToInt32(id, CultureInfo.InvariantCulture))
            gridCoduri.RemoveRowAt(e.RowIndex)
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.GridCoduri_ButtonClick", ex)
        End Try
    End Sub

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
            If KBotMessage.Show(Me, $"Ștergeți partenerul «{_current.Denumire}» și codurile lui de angajament?",
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
                                                            .CoduriSterse = New List(Of Integer)(_deletedCodes)}
            SetBusy(True, "Se salvează partenerul…")
            Dim id As Integer
            Try
                id = Await _gate.RunAsync(Function() _api.SavePartenerAsync(request, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return True
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

    ' The codes as the grid shows them; the first row without a classification (or a classification
    ' used twice) is reported and opened.
    Private Function ReadCodes(ByRef problem As String) As List(Of PartenerCod)
        Dim list As New List(Of PartenerCod)()
        Dim seen As New HashSet(Of Integer)()
        For i As Integer = 0 To gridCoduri.RowCount - 1
            Dim row As KBotDataRow = gridCoduri.Rows(i)
            Dim choice As ClsfChoice = TryCast(row(ColClsf), ClsfChoice)
            If choice Is Nothing Then
                problem = $"Codurile de angajament, rândul {i + 1}: alegeți clasificația."
            ElseIf Not seen.Add(choice.IdClsf) Then
                problem = $"Codurile de angajament, rândul {i + 1}: clasificația {choice.Clsf} apare de două ori."
            End If
            If problem IsNot Nothing Then
                gridCoduri.EnsureVisible(i)
                gridCoduri.EditCell(ColClsf, i)
                Return list
            End If
            Dim idValue As Object = row(ColId)
            list.Add(New PartenerCod() With {
                .Id = If(idValue Is Nothing, CType(Nothing, Integer?), Convert.ToInt32(idValue, CultureInfo.InvariantCulture)),
                .IdClsf = choice.IdClsf,
                .ContBancar = Convert.ToString(row(ColCont), CultureInfo.CurrentCulture).Trim(),
                .CodAng = Convert.ToString(row(ColCodAng), CultureInfo.CurrentCulture).Trim(),
                .CodInd = Convert.ToString(row(ColCodInd), CultureInfo.CurrentCulture).Trim()})
        Next
        Return list
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
            For Each c As Control In {tlyMain, pnlCard, tlyBody, tlyLeft, flowFiltre, tlyDetalii, tlySubsol}
                c.BackColor = p.SurfaceAltColor
            Next
            lblCoduri.ForeColor = p.TextColor
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
