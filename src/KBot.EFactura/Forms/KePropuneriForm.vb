Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Slice KE16 -- «Propuneri»: opened from the tree of the page «Recepții» (the button that shows on a supplier when the mouse is over it). It lists the supplier's invoices that
''' have no link to a reception and, for each, the reception the server PROPOSES: the one that has, whole, the value of the invoice (one invoice = one reception, nothing partial,
''' nothing picked apart by lines). The accountant looks, ticks what she agrees with and presses «Leagă»; until then nothing is saved. An invoice with several receptions of the
''' same value and no clear one (the date rule of the server) is listed unticked: pressing a row shows its candidates below and a double click on one chooses it.
''' </summary>
Public Class KePropuneriForm

    ' What a row of the invoices grid stands for: the invoice (with its candidates) and the reception chosen for it (Nothing = none yet).
    Private NotInheritable Class RowState
        Public Property Factura As EFacturaKePropunereFactura
        Public Property Chosen As EFacturaKePropunereReceptie

        ' The accountant picked the reception herself (it was not the proposal).
        Public Property ByHand As Boolean
    End Class

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _ids As List(Of Integer)
    Private ReadOnly _angajamente As List(Of String)
    Private _answer As EFacturaKePropuneri
    Private _filling As Boolean
    Private _busy As Boolean

    ''' <summary>How many links were written (valid when the window closed with OK).</summary>
    Public ReadOnly Property Legate As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
        _ids = New List(Of Integer)()
        _angajamente = New List(Of String)()
    End Sub

    ''' <param name="k_supplier">The supplier's name, shown in the header.</param>
    ''' <param name="k_ids">The invoices of the supplier that have no reception linked.</param>
    ''' <param name="k_angajamente">The angajamente picked in the page for this supplier (the server adds the ones it knows from the DDFs and from the supplier's invoices).</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_supplier As String, k_ids As IEnumerable(Of Integer), k_angajamente As IEnumerable(Of String))
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_ids)
        ArgumentNullException.ThrowIfNull(k_angajamente)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _ids = k_ids.ToList()
        _angajamente = k_angajamente.ToList()
        lblAntet.Text = $"Facturile neasociate ale furnizorului {k_supplier}. K-BOT propune, pentru fiecare, recepția care are exact valoarea facturii (o factură = o recepție întreagă, " &
                        "fără facturi împărțite pe produse sau recepții parțiale). Bifează ce ești de acord să legi și apasă «Leagă»; până atunci nu se salvează nimic."
        AddHandler gridFact.SelectionChanged, AddressOf GridFact_SelectionChanged
        AddHandler gridFact.RowChecked, AddressOf GridFact_RowChecked
        AddHandler gridCand.CellDoubleClick, AddressOf GridCand_CellDoubleClick
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub KePropuneriForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            SetBusy(True)
            _answer = Await _gate.RunAsync(Function() _api.GetKePropuneriAsync(_ids, _angajamente, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            FillFacturi()
            SetBusy(False)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KePropuneriForm.KePropuneriForm_Load", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
            SetBusy(False)
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.KePropuneriForm_Load", ex)
            ShowNotice("Propunerile nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
            SetBusy(False)
        End Try
    End Sub

    Private Sub KePropuneriForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.KePropuneriForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind)
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        gridFact.Enabled = Not k_busy
        gridCand.Enabled = Not k_busy
        btnRenunta.Enabled = Not k_busy OrElse _answer Is Nothing
        RefreshButton()
    End Sub

    Private Shared Function DayText(k_day As Date?) As String
        Return If(k_day.HasValue, k_day.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
    End Function

    ' ── The invoices ────────────────────────────────────────────────────────────

    Private Sub FillFacturi()
        _filling = True
        gridFact.BeginUpdate()
        Try
            gridFact.ClearRows()
            For Each k_f As EFacturaKePropunereFactura In _answer.Facturi
                Dim k_state As New RowState() With {.Factura = k_f, .Chosen = k_f.Candidate.FirstOrDefault(Function(k_c) k_f.Propunere.HasValue AndAlso k_c.Idrr = k_f.Propunere.Value)}
                Dim k_row As KBotDataRow = gridFact.AddRow()
                k_row.Tag = k_state
                ShowRow(k_row)
                k_row(KBotDataView.CheckColumnKey) = k_state.Chosen IsNot Nothing
            Next
        Finally
            gridFact.EndUpdate()
            _filling = False
        End Try
        If gridFact.RowCount = 0 Then
            ShowNotice("Nu este nicio factură neasociată de verificat.", NoticeKind.Warning)
        ElseIf _answer.Omise > 0 Then
            ShowNotice($"{_answer.Omise} din facturi au deja o recepție legată și nu sunt aici.", NoticeKind.Warning)
        End If
        FillCandidates()
        RefreshButton()
    End Sub

    ' The cells of a row from its state: the reception chosen, or why there is none.
    Private Sub ShowRow(k_row As KBotDataRow)
        Dim k_state As RowState = CType(k_row.Tag, RowState)
        Dim k_f As EFacturaKePropunereFactura = k_state.Factura
        k_row("factura") = k_f.NrFact
        k_row("data") = DayText(k_f.DataFact)
        k_row("total") = CDbl(k_f.Total)
        If k_state.Chosen IsNot Nothing Then
            k_row("receptie") = k_state.Chosen.Descriere
            k_row("dataR") = DayText(k_state.Chosen.DataR)
            k_row("obs") = If(k_state.ByHand, "Aleasă de dumneavoastră", $"{k_state.Chosen.CodAngajament} · {k_state.Chosen.Clsf}")
        Else
            k_row("receptie") = String.Empty
            k_row("dataR") = String.Empty
            k_row("obs") = If(k_f.Candidate.Count > 1, $"{k_f.Motiv} ({k_f.Candidate.Count} recepții posibile)", k_f.Motiv)
        End If
    End Sub

    Private Function Ticked() As List(Of KBotDataRow)
        Dim k_list As New List(Of KBotDataRow)()
        For Each k_index As Integer In gridFact.GetCheckedRows()
            k_list.Add(gridFact.Rows(k_index))
        Next
        Return k_list
    End Function

    Private Sub RefreshButton()
        Dim k_count As Integer = If(_busy, 0, Ticked().Count)
        btnLeaga.Enabled = k_count > 0 AndAlso Not _busy
        btnLeaga.Text = If(k_count > 0, $"Leagă ({k_count})", "Leagă")
    End Sub

    ' The invoice a ticked row (other than k_row) already gives to this reception.
    Private Function OtherUsing(k_row As KBotDataRow, k_idrr As Integer) As KBotDataRow
        For Each k_other As KBotDataRow In Ticked()
            Dim k_chosen As EFacturaKePropunereReceptie = CType(k_other.Tag, RowState).Chosen
            If Not Object.ReferenceEquals(k_other, k_row) AndAlso k_chosen IsNot Nothing AndAlso k_chosen.Idrr = k_idrr Then Return k_other
        Next
        Return Nothing
    End Function

    ' UI boundary: the operator ticked or cleared a row. A tick needs a reception, and one reception goes to ONE invoice.
    Private Sub GridFact_RowChecked(sender As Object, e As KBotRowCheckedEventArgs)
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridFact.RowCount Then Return
            If e.Checked Then
                Dim k_row As KBotDataRow = gridFact.Rows(e.RowIndex)
                Dim k_chosen As EFacturaKePropunereReceptie = CType(k_row.Tag, RowState).Chosen
                If k_chosen Is Nothing Then
                    gridFact.SetRowChecked(e.RowIndex, False)
                    ShowNotice("Factura nu are o recepție aleasă: alege una în tabelul de jos (dublu clic), apoi bifeaz-o.", NoticeKind.Warning)
                ElseIf OtherUsing(k_row, k_chosen.Idrr) IsNot Nothing Then
                    gridFact.SetRowChecked(e.RowIndex, False)
                    ShowNotice("Altă factură bifată folosește deja această recepție: o recepție se leagă de o singură factură.", NoticeKind.Warning)
                Else
                    ntfMesaj.Visible = False
                End If
            End If
            RefreshButton()
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridFact_RowChecked", ex)
        End Try
    End Sub

    ' ── The candidates of the current invoice ───────────────────────────────────

    Private Function CurrentState() As RowState
        Return TryCast(gridFact.CurrentRow?.Tag, RowState)
    End Function

    Private Sub GridFact_SelectionChanged(sender As Object, e As EventArgs)
        Try
            FillCandidates()
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridFact_SelectionChanged", ex)
        End Try
    End Sub

    Private Sub FillCandidates()
        Dim k_state As RowState = CurrentState()
        gridCand.BeginUpdate()
        Try
            gridCand.ClearRows()
            If k_state Is Nothing Then Return
            For Each k_c As EFacturaKePropunereReceptie In k_state.Factura.Candidate
                Dim k_row As KBotDataRow = gridCand.AddRow()
                k_row.Tag = k_c
                k_row("dataR") = DayText(k_c.DataR)
                k_row("descriere") = k_c.Descriere
                k_row("cod") = k_c.CodAngajament
                k_row("clsf") = k_c.Clsf
                k_row("zile") = If(k_c.Zile.HasValue, k_c.Zile.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                k_row("aleasa") = If(k_state.Chosen IsNot Nothing AndAlso k_state.Chosen.Idrr = k_c.Idrr, "da", String.Empty)
            Next
        Finally
            gridCand.EndUpdate()
        End Try
    End Sub

    ' UI boundary: a double click on a candidate gives that reception to the current invoice (it is ticked too: the accountant chose it).
    Private Sub GridCand_CellDoubleClick(sender As Object, e As KBotCellEventArgs)
        Try
            Dim k_state As RowState = CurrentState()
            Dim k_candidate As EFacturaKePropunereReceptie = TryCast(gridCand.CurrentRow?.Tag, EFacturaKePropunereReceptie)
            If k_state Is Nothing OrElse k_candidate Is Nothing OrElse gridFact.CurrentRow Is Nothing Then Return
            Dim k_row As KBotDataRow = gridFact.CurrentRow
            Dim k_other As KBotDataRow = OtherUsing(k_row, k_candidate.Idrr)
            If k_other IsNot Nothing Then
                ShowNotice($"Recepția e folosită deja pentru factura {CType(k_other.Tag, RowState).Factura.NrFact}, bifată: debifeaz-o întâi.", NoticeKind.Warning)
                Return
            End If
            k_state.Chosen = k_candidate
            k_state.ByHand = Not (k_state.Factura.Propunere.HasValue AndAlso k_state.Factura.Propunere.Value = k_candidate.Idrr)
            _filling = True
            Try
                ShowRow(k_row)
                k_row(KBotDataView.CheckColumnKey) = True
            Finally
                _filling = False
            End Try
            gridFact.Invalidate()
            ntfMesaj.Visible = False
            FillCandidates()
            RefreshButton()
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridCand_CellDoubleClick", ex)
        End Try
    End Sub

    ' ── Writing ─────────────────────────────────────────────────────────────────

    ' UI boundary (async Sub): logs and swallows; a failure becomes a notice and the window stays open (nothing was written: the server writes all or none).
    Private Async Sub BtnLeaga_Click(sender As Object, e As EventArgs) Handles btnLeaga.Click
        Try
            If _api Is Nothing OrElse _busy Then Return
            Dim k_links As New List(Of EFacturaKeLegaturaMasa)()
            For Each k_row As KBotDataRow In Ticked()
                Dim k_state As RowState = CType(k_row.Tag, RowState)
                If k_state.Chosen Is Nothing Then Continue For
                k_links.Add(New EFacturaKeLegaturaMasa() With {
                    .IdPrimita = k_state.Factura.IdPrimita, .CodAngajament = k_state.Chosen.CodAngajament, .Idrr = k_state.Chosen.Idrr})
            Next
            If k_links.Count = 0 Then Return
            ntfMesaj.Visible = False
            SetBusy(True)
            Dim k_done As Integer = Await _gate.RunAsync(Function() _api.ApplyKePropuneriAsync(k_links, _angajamente, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _Legate = k_done
            DialogResult = DialogResult.OK
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KePropuneriForm.BtnLeaga_Click", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
            SetBusy(False)
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.BtnLeaga_Click", ex)
            ShowNotice("Legarea nu a putut fi făcută. Detalii în jurnalul de erori.", NoticeKind.Error)
            SetBusy(False)
        End Try
    End Sub

End Class
