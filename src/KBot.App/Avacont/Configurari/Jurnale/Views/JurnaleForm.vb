Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text.RegularExpressions
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Jurnale» (slice AV01-09): the modal window with the accounting journals of the unit, a code (capitals) and a name each, typed straight in
''' the grid («+» in the footer adds a row, «✕» removes one). Everything is written together at «Salvare». A journal that is used cannot be
''' deleted: the server refuses and says so in the box above the grid.
''' </summary>
Public Class JurnaleForm

    Private Const ColCod As String = "cod"
    Private Const ColSs As String = "ss"
    Private Const ColDenumire As String = "denumire"
    Private Const ColDelete As String = "sterge"
    Private Const ColId As String = "id"
    Private Const DeleteCaption As String = "✕"

    Private Shared ReadOnly CodRegex As New Regex("^[A-Z0-9_]{1,10}$", RegexOptions.CultureInvariant)

    Private ReadOnly _api As IJurnaleApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _deleted As New List(Of Integer)()
    Private _dirty As Boolean
    Private _busy As Boolean
    ' True once the list was read: a failed read keeps «Salvare» off, so a grid that is empty by accident is never written over the journals.
    Private _loaded As Boolean
    ' Slice AV01-09: the journals follow the sector+source of the title bar, like the accounts. On «Toate» (or a source the list does not know) the
    ' column «Sursa» shows and the sector+source is picked on each journal.
    Private ReadOnly _session As SessionContext
    Private _ssEditable As Boolean
    Private ReadOnly _surse As New HashSet(Of String)(StringComparer.Ordinal)
    Private _saved As Boolean

    ''' <summary>True once the journals were written (the caller may need to read them again).</summary>
    Public ReadOnly Property Modificat As Boolean
        Get
            Return _saved
        End Get
    End Property

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_session">Its sector+source (title bar) and the «Toate» flag decide which journals show and the sector+source of a new one.</param>
    Public Sub New(k_api As IJurnaleApi, k_gate As ReauthGate, k_session As SessionContext)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_session)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _session = k_session
        RefreshButtons()
    End Sub

    Private Sub JurnaleForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            LoadJurnale()
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.JurnaleForm_Load", ex)
        End Try
    End Sub

    ' UI boundary: reads the list; a failure is shown in the box and leaves the grid off.
    Private Async Sub LoadJurnale()
        Try
            SetBusy(True)
            Dim k_catalog As JurnaleCatalog = Await _gate.RunAsync(
                Function() _api.GetJurnaleAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _surse.Clear()
            Dim k_items As New List(Of Object)()
            For Each k_s As ClsfCont In k_catalog.Surse
                _surse.Add(k_s.Cod)
                k_items.Add(k_s.Cod)
            Next
            gridJurnale.Column(ColSs).ComboItems = k_items
            Dim k_ss As String = _session.SectorSursa
            _ssEditable = _session.ToateSursele OrElse String.IsNullOrEmpty(k_ss) OrElse Not _surse.Contains(k_ss)
            gridJurnale.Column(ColSs).Visible = If(_ssEditable, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
            capBar.Text = If(_ssEditable, "K-BOT — Jurnale · toate sursele", "K-BOT — Jurnale · " & k_ss)
            Text = capBar.Text
            Dim k_list As IEnumerable(Of JurnalInfo) = If(_ssEditable, k_catalog.Jurnale,
                k_catalog.Jurnale.Where(Function(k_j) String.Equals(k_j.SursaSector, k_ss, StringComparison.Ordinal)))
            gridJurnale.BeginUpdate()
            Try
                gridJurnale.ClearRows()
                For Each k_j As JurnalInfo In k_list
                    Dim k_row As KBotDataRow = gridJurnale.AddRow()
                    k_row(ColId) = k_j.IdJurnal
                    k_row(ColSs) = k_j.SursaSector
                    k_row(ColCod) = k_j.Cod
                    k_row(ColDenumire) = k_j.Denumire
                    k_row(ColDelete) = DeleteCaption
                Next
            Finally
                gridJurnale.EndUpdate()
            End Try
            gridJurnale.ClearDirty()
            _loaded = True
        Catch ex As ApiException
            GlobalErrorLog.Write("JurnaleForm.LoadJurnale", ex)
            If Not IsDisposed Then ntfMesaj.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.LoadJurnale", ex)
            If Not IsDisposed Then ntfMesaj.Show("Jurnalele nu au putut fi încărcate. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    ' ── Typing ──────────────────────────────────────────────────────────────────

    Private Sub GridJurnale_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridJurnale.CellValidating
        Try
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture).Trim()
            If String.Equals(e.ColumnKey, ColSs, StringComparison.Ordinal) Then
                k_text = k_text.ToUpperInvariant()
                If Not _surse.Contains(k_text) Then
                    e.Cancel = True
                    ntfMesaj.Show("Alegeți un sector și o sursă din listă.", NoticeKind.Warning)
                    Return
                End If
            End If
            If String.Equals(e.ColumnKey, ColCod, StringComparison.Ordinal) Then
                k_text = k_text.ToUpperInvariant()
                If k_text.Length > 0 AndAlso Not CodRegex.IsMatch(k_text) Then
                    e.Cancel = True
                    ntfMesaj.Show("Codul poate avea de la 1 la 10 caractere: litere, cifre și _.", NoticeKind.Warning)
                    Return
                End If
            End If
            e.ProposedValue = k_text
            ntfMesaj.Clear()
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.GridJurnale_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridJurnale_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridJurnale.CellValueChanged
        Try
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.GridJurnale_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridJurnale_FooterRightIconClicked(sender As Object, e As EventArgs) Handles gridJurnale.FooterRightIconClicked
        Try
            If _busy OrElse Not _loaded Then Return
            If Not gridJurnale.CommitPendingEdit() Then Return
            Dim k_row As KBotDataRow = gridJurnale.AddRow()
            k_row(ColDelete) = DeleteCaption
            If Not _ssEditable Then k_row(ColSs) = _session.SectorSursa
            k_row.IsDirty = True
            SetDirty(True)
            Dim k_index As Integer = gridJurnale.RowCount - 1
            gridJurnale.EnsureVisible(k_index)
            gridJurnale.EditCell(If(_ssEditable, ColSs, ColCod), k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.GridJurnale_FooterRightIconClicked", ex)
        End Try
    End Sub

    ' Esc twice in a row: a row without an id is one the operator just added with «+» and it goes away; a row from the server goes back as it was (the grid's default).
    Private Sub GridJurnale_RowCancelRequested(sender As Object, e As KBotRowCancelEventArgs) Handles gridJurnale.RowCancelRequested
        Try
            If _busy OrElse Not _loaded OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridJurnale.RowCount OrElse gridJurnale(ColId, e.RowIndex) IsNot Nothing Then Return
            e.Handled = True
            gridJurnale.RemoveRowAt(e.RowIndex)
            ntfMesaj.Clear()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.GridJurnale_RowCancelRequested", ex)
        End Try
    End Sub

    Private Sub GridJurnale_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridJurnale.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy OrElse Not _loaded Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridJurnale.RowCount Then Return
            Dim k_id As Object = gridJurnale(ColId, e.RowIndex)
            If k_id IsNot Nothing Then _deleted.Add(Convert.ToInt32(k_id, CultureInfo.InvariantCulture))
            gridJurnale.RemoveRowAt(e.RowIndex)
            ntfMesaj.Clear()
            SetDirty(True)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.GridJurnale_ButtonClick", ex)
        End Try
    End Sub

    ' ── Buttons ─────────────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _busy OrElse Not _loaded Then Return
            If Not gridJurnale.CommitPendingEdit() Then Return
            If Not _dirty Then
                Close()
                Return
            End If
            Dim k_problem As String = Nothing
            Dim k_list As List(Of JurnalInfo) = ReadRows(k_problem)
            If k_problem IsNot Nothing Then
                ntfMesaj.Show(k_problem, NoticeKind.Warning)
                Return
            End If
            Dim k_deleted As List(Of Integer) = New List(Of Integer)(_deleted)
            SetBusy(True)
            Try
                Await _gate.RunAsync(Function() _api.SaveJurnaleAsync(k_list, k_deleted, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False)
            End Try
            If IsDisposed Then Return
            _saved = True
            _dirty = False
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ApiException
            GlobalErrorLog.Write("JurnaleForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then ntfMesaj.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then ntfMesaj.Show("Jurnalele nu au putut fi salvate. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' The rows as the grid shows them; the first row with a missing code / name, or a code used twice, is reported and opened.
    Private Function ReadRows(ByRef k_problem As String) As List(Of JurnalInfo)
        Dim k_list As New List(Of JurnalInfo)()
        Dim k_seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For k_i As Integer = 0 To gridJurnale.RowCount - 1
            Dim k_row As KBotDataRow = gridJurnale.Rows(k_i)
            Dim k_ss As String = If(_ssEditable, Convert.ToString(k_row(ColSs), CultureInfo.CurrentCulture).Trim().ToUpperInvariant(), _session.SectorSursa)
            Dim k_cod As String = Convert.ToString(k_row(ColCod), CultureInfo.CurrentCulture).Trim().ToUpperInvariant()
            Dim k_den As String = Convert.ToString(k_row(ColDenumire), CultureInfo.CurrentCulture).Trim()
            Dim k_column As String = ColCod
            If k_ss.Length = 0 OrElse Not _surse.Contains(k_ss) Then
                k_problem = $"Jurnalele, rândul {k_i + 1}: alegeți sectorul și sursa."
                k_column = ColSs
            ElseIf k_cod.Length = 0 Then
                k_problem = $"Jurnalele, rândul {k_i + 1}: completați codul."
            ElseIf Not k_seen.Add(k_ss & "|" & k_cod) Then
                k_problem = $"Jurnalele, rândul {k_i + 1}: codul {k_cod} apare de două ori la {k_ss}."
            ElseIf k_den.Length = 0 Then
                k_problem = $"Jurnalul {k_cod}: completați denumirea."
                k_column = ColDenumire
            End If
            If k_problem IsNot Nothing Then
                gridJurnale.EnsureVisible(k_i)
                gridJurnale.EditCell(k_column, k_i)
                Return k_list
            End If
            Dim k_id As Object = k_row(ColId)
            k_list.Add(New JurnalInfo() With {
                .IdJurnal = If(k_id Is Nothing, 0, Convert.ToInt32(k_id, CultureInfo.InvariantCulture)),
                .SursaSector = k_ss, .Cod = k_cod, .Denumire = k_den})
        Next
        Return k_list
    End Function

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.BtnRenunta_Click", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If e.CloseReason = CloseReason.UserClosing AndAlso _dirty AndAlso Not _saved Then
                If KBotMessage.Show(Me, "Jurnalele au modificări nesalvate." & vbLf & "Le renunțați?", "Jurnale",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
                    e.Cancel = True
                End If
            End If
            MyBase.OnFormClosing(e)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.OnFormClosing", ex)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetDirty(k_value As Boolean)
        _dirty = k_value
        RefreshButtons()
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        UseWaitCursor = k_busy
        RefreshButtons()
    End Sub

    Private Sub RefreshButtons()
        btnSalveaza.Enabled = _loaded AndAlso Not _busy
        gridJurnale.Enabled = _loaded AndAlso Not _busy
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            ' The form background IS the 1px outline of the window (Padding(1)).
            BackColor = k_scheme.Palette.BorderColor
            For Each k_c As Control In {tlyMain, pnlCard, tlyCorp, tlySubsol}
                k_c.BackColor = k_scheme.Palette.SurfaceAltColor
            Next
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            ButtonStyles.ApplySecondary(btnRenunta, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("JurnaleForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
