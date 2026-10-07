Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 00EF-12 -- «E-Factura — conturile unității»: the bank accounts (IBAN) of the open unit as the ISSUER of the invoices,
''' typed straight into a grid. The bank of each account is deduced by the server from the bank code inside the IBAN and shown
''' read only after a save. These are the unit's own accounts, never the accounts of the partners it pays.
'''
''' <para>Everything is written by «Salvează» (the server replaces the whole list in one step, checks every IBAN and refuses a
''' repeated one). The window is a dialog of <see cref="FacturiForm"/>: the same API client and the same re-login net.</para>
''' </summary>
Public Class ConturiForm

    Private Const ColCont As String = "cont"
    Private Const ColBanca As String = "banca"
    Private Const ColSterge As String = "sterge"
    Private Const DeleteCaption As String = "✕"
    ' The longest IBAN there is (the column of the table is varchar(34)).
    Private Const MaxIbanLength As Integer = 34

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _unitName As String
    Private ReadOnly _cts As New CancellationTokenSource()
    Private _dirty As Boolean
    Private _working As Boolean
    Private _loading As Boolean

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
        _unitName = String.Empty
    End Sub

    ''' <param name="k_api">The routes of the accounts; the invoices window passes the one it has.</param>
    ''' <param name="k_gate">The shell's re-login net.</param>
    ''' <param name="k_unitName">The open unit's name, shown in the title bar.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_unitName As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _unitName = If(k_unitName, String.Empty)
    End Sub

    Private Sub ConturiForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            If _unitName.Length > 0 Then capBar.Text = "E-Factura — conturile unității " & _unitName
            ApplyButtons()
            LoadConturi()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow.
            GlobalErrorLog.Write("ConturiForm.ConturiForm_Load", ex)
        End Try
    End Sub

    Private Sub ConturiForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            If _dirty AndAlso Not _working Then
                Dim k_answer As DialogResult = KBotMessage.Show(Me, "Există conturi nesalvate. Le închideți fără să le salvați?", "Conturile unității",
                                                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                If k_answer <> DialogResult.Yes Then e.Cancel = True
            End If
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("ConturiForm.ConturiForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub ConturiForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            _cts.Cancel()
            _cts.Dispose()
        Catch ex As Exception
            ' UI boundary (event handler): log and swallow.
            GlobalErrorLog.Write("ConturiForm.ConturiForm_FormClosed", ex)
        End Try
    End Sub

    ' ── Loading and showing ─────────────────────────────────────────────────────

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadConturi()
        Try
            SetBusy(True)
            ntfMesaj.Clear()
            Dim k_list As List(Of EFacturaCont) = Await _gate.RunAsync(
                Function() _api.GetConturiAsync(_cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            FillGrid(k_list)
        Catch ex As OperationCanceledException
            ' The window was closed while the answer was on its way: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then ntfMesaj.Show(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.LoadConturi", ex)
            If Not IsDisposed Then ntfMesaj.Show("Conturile nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    Private Sub FillGrid(k_list As List(Of EFacturaCont))
        _loading = True
        gridConturi.BeginUpdate()
        Try
            gridConturi.ClearRows()
            For Each k_cont As EFacturaCont In k_list
                Dim k_row As KBotDataRow = gridConturi.AddRow()
                k_row.Tag = k_cont
                k_row(ColCont) = k_cont.Cont
                k_row(ColBanca) = If(k_cont.Banca.Length = 0, "— banca nu este în listă —", k_cont.Banca)
                k_row(ColSterge) = DeleteCaption
            Next
        Finally
            gridConturi.EndUpdate()
            _loading = False
        End Try
        gridConturi.ClearDirty()
        _dirty = False
    End Sub

    Private Sub SetBusy(k_on As Boolean)
        _working = k_on
        busy.Running = k_on
        UseWaitCursor = k_on
        ApplyButtons()
    End Sub

    Private Sub ApplyButtons()
        btnNou.Enabled = Not _working
        btnSalveaza.Enabled = Not _working
        gridConturi.ReadOnlyGrid = _working
    End Sub

    ' ── Editing ─────────────────────────────────────────────────────────────────

    ' An IBAN as it is kept: no spaces, upper case.
    Private Shared Function CleanIban(k_value As Object) As String
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.CurrentCulture)
        If k_text Is Nothing Then Return String.Empty
        Return String.Concat(k_text.Split(New Char() {" "c, Chr(160), ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant()
    End Function

    Private Sub GridConturi_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridConturi.CellValidating
        Try
            If e.ColumnKey <> ColCont Then Return
            Dim k_iban As String = CleanIban(e.ProposedValue)
            If k_iban.Length > MaxIbanLength Then
                e.Cancel = True
                ntfMesaj.Show($"Un IBAN are cel mult {MaxIbanLength} de caractere.", NoticeKind.Warning)
                Return
            End If
            e.ProposedValue = k_iban
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.GridConturi_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridConturi_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridConturi.CellValueChanged
        Try
            If _loading OrElse e.ColumnKey <> ColCont Then Return
            ' The bank belongs to the old account: it is deduced again, by the server, at the next save.
            gridConturi.Rows(e.RowIndex)(ColBanca) = String.Empty
            gridConturi.InvalidateRow(e.RowIndex)
            _dirty = True
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.GridConturi_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridConturi_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridConturi.ButtonClick
        Try
            If e.ColumnKey <> ColSterge OrElse _working Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridConturi.RowCount Then Return
            gridConturi.RemoveRowAt(e.RowIndex)
            _dirty = True
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.GridConturi_ButtonClick", ex)
        End Try
    End Sub

    Private Sub BtnNou_Click(sender As Object, e As EventArgs) Handles btnNou.Click
        Try
            If _working Then Return
            If Not gridConturi.CommitPendingEdit() Then Return
            Dim k_row As KBotDataRow = gridConturi.AddRow()
            k_row(ColCont) = String.Empty
            k_row(ColBanca) = String.Empty
            k_row(ColSterge) = DeleteCaption
            k_row.IsDirty = True
            _dirty = True
            Dim k_index As Integer = gridConturi.RowCount - 1
            gridConturi.EnsureVisible(k_index)
            gridConturi.EditCell(ColCont, k_index)
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.BtnNou_Click", ex)
        End Try
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.BtnInchide_Click", ex)
        End Try
    End Sub

    ' ── Saving ──────────────────────────────────────────────────────────────────

    ' UI boundary (async void): every failure is shown to the operator here.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _working Then Return
            If Not gridConturi.CommitPendingEdit() Then Return
            Dim k_send As New List(Of EFacturaCont)()
            For k_i As Integer = 0 To gridConturi.RowCount - 1
                Dim k_iban As String = CleanIban(gridConturi.Rows(k_i)(ColCont))
                If k_iban.Length = 0 Then
                    gridConturi.EnsureVisible(k_i)
                    gridConturi.EditCell(ColCont, k_i)
                    ntfMesaj.Show($"Rândul {k_i + 1} nu are cont: scrieți IBAN-ul sau ștergeți rândul cu «✕».", NoticeKind.Warning)
                    Return
                End If
                k_send.Add(New EFacturaCont() With {.Cont = k_iban})
            Next
            SetBusy(True)
            ntfMesaj.Clear()
            Dim k_saved As List(Of EFacturaCont) = Await _gate.RunAsync(
                Function() _api.SaveConturiAsync(k_send, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            FillGrid(k_saved)
            ntfMesaj.Show("Conturile au fost salvate.", NoticeKind.Success)
        Catch ex As OperationCanceledException
            ' The window was closed during the save: nothing to show.
        Catch ex As ApiException
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Conturile unității", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("ConturiForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Conturile nu au putut fi salvate. Detalii în jurnalul de erori.", "Conturile unității",
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

End Class
