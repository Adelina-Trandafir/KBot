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
''' Slice KE02 -- «Clasificatiile liniilor»: for each line of the invoice the accountant chooses the budget classification it belongs to. The
''' choices are the INDICATORS of an angajament (the law: a reception can only take a value on a classification the angajament already has as
''' an indicator), so the angajament comes first: the ones the invoice can go to are in the list, any other one is picked from the list of all.
''' The window only collects; the choices are written by the server on «Salveaza» (<see cref="Salvata"/> brings the new state back).
''' </summary>
Public Class KeClasificariForm

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")
    Private Const NeAleasa As String = "(nealeasă)"
    Private Const ColNr As String = "nr"
    Private Const ColDenumire As String = "denumire"
    Private Const ColCuTva As String = "cuTva"
    Private Const ColClsf As String = "clsf"

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _idPrimita As Integer
    Private ReadOnly _invoiceLabel As String
    Private ReadOnly _stare As EFacturaKeStare
    Private ReadOnly _byLabel As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _labelOf As New Dictionary(Of Integer, String)()
    Private _seq As Integer
    Private _loading As Boolean

    ''' <summary>The state of the invoice after the choices were saved (valid when the window closed with OK).</summary>
    Public ReadOnly Property Salvata As EFacturaKeStare

    ''' <summary>The angajament whose indicators were offered when the choices were saved; empty when none was chosen.</summary>
    Public ReadOnly Property CodAngajament As String = String.Empty

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_idPrimita">The invoice.</param>
    ''' <param name="k_invoiceLabel">Number and supplier, shown in the header.</param>
    ''' <param name="k_stare">The state read from the server: the lines and the angajamente the invoice can go to.</param>
    ''' <param name="k_cod">An angajament to start with (the one the accountant already works on), or empty.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_idPrimita As Integer, k_invoiceLabel As String,
                   k_stare As EFacturaKeStare, k_cod As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_stare)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _idPrimita = k_idPrimita
        _invoiceLabel = k_invoiceLabel
        _stare = k_stare
        lblAntet.Text = $"Alege, pentru fiecare linie a facturii {k_invoiceLabel}, clasificația din angajament la care aparține."
        cmbAngajament.CaptionSelector = Function(k_o) CaptionOf(k_o)
        For Each k_a As EFacturaKeAngajament In k_stare.Angajamente
            cmbAngajament.Items.Add(k_a)
        Next
        If Not String.IsNullOrWhiteSpace(k_cod) Then SelectOrAdd(k_cod, String.Empty)
        FillLines()
        btnSalveaza.Enabled = _stare.PoateScrie
    End Sub

    Private Shared Function CaptionOf(k_item As Object) As String
        Dim k_a As EFacturaKeAngajament = TryCast(k_item, EFacturaKeAngajament)
        If k_a Is Nothing Then Return Convert.ToString(k_item, CultureInfo.InvariantCulture)
        Return If(String.IsNullOrWhiteSpace(k_a.Descriere), k_a.CodAngajament, $"{k_a.CodAngajament} — {k_a.Descriere}")
    End Function

    ' Puts the angajament on the list when it is not there, and selects it.
    Private Sub SelectOrAdd(k_cod As String, k_descriere As String)
        For k_i As Integer = 0 To cmbAngajament.Items.Count - 1
            Dim k_a As EFacturaKeAngajament = TryCast(cmbAngajament.Items(k_i), EFacturaKeAngajament)
            If k_a IsNot Nothing AndAlso String.Equals(k_a.CodAngajament, k_cod, StringComparison.OrdinalIgnoreCase) Then
                cmbAngajament.SelectedIndex = k_i
                Return
            End If
        Next
        cmbAngajament.Items.Add(New EFacturaKeAngajament() With {.CodAngajament = k_cod, .Descriere = k_descriere, .Sursa = "alegere"})
        cmbAngajament.SelectedIndex = cmbAngajament.Items.Count - 1
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; the first angajament is chosen when there is exactly one.
    Private Sub KeClasificariForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            If cmbAngajament.SelectedIndex < 0 AndAlso cmbAngajament.Items.Count = 1 Then cmbAngajament.SelectedIndex = 0
            If cmbAngajament.Items.Count = 0 Then
                ShowNotice("Furnizorul nu are niciun DDF legat: alegeți angajamentul cu «Alt angajament…».", NoticeKind.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.KeClasificariForm_Load", ex)
        End Try
    End Sub

    Private Sub KeClasificariForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            _cts.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.KeClasificariForm_FormClosing", ex)
        End Try
    End Sub

    Private Sub ShowNotice(k_text As String, k_kind As NoticeKind)
        ntfMesaj.Show(k_text, k_kind)
        ntfMesaj.Visible = True
    End Sub

    Private Sub FillLines()
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            For Each k_l As EFacturaKeLinie In _stare.Linii
                Dim k_row As KBotDataRow = gridLinii.AddRow()
                k_row.Tag = k_l
                k_row(ColNr) = k_l.NrLinie
                k_row(ColDenumire) = k_l.Denumire
                k_row(ColCuTva) = CDbl(k_l.ValoareCuTva)
                k_row(ColClsf) = If(k_l.IdClsf.HasValue, If(k_l.Clsf, String.Empty), NeAleasa)
            Next
        Finally
            gridLinii.EndUpdate()
        End Try
    End Sub

    ' UI boundary (async Sub): the angajament changed: its indicators become the choices of the lines.
    Private Async Sub CmbAngajament_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAngajament.SelectedIndexChanged
        Try
            Dim k_a As EFacturaKeAngajament = TryCast(cmbAngajament.SelectedItem, EFacturaKeAngajament)
            If k_a Is Nothing OrElse _api Is Nothing Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            ntfMesaj.Visible = False
            Dim k_data As EFacturaKeReceptiiAngajament = Await _gate.RunAsync(
                Function() _api.GetKeReceptiiAsync(k_a.CodAngajament, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _seq Then Return
            ApplyIndicators(k_data)
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeClasificariForm.CmbAngajament_SelectedIndexChanged", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.CmbAngajament_SelectedIndexChanged", ex)
            ShowNotice("Indicatorii angajamentului nu au putut fi citiți. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    ' The indicators of the angajament are the choices; a classification a line already has that is not one of them stays visible.
    Private Sub ApplyIndicators(k_data As EFacturaKeReceptiiAngajament)
        _byLabel.Clear()
        _labelOf.Clear()
        Dim k_choices As New List(Of Object) From {NeAleasa}
        For Each k_i As EFacturaKeIndicator In k_data.Indicatori
            If Not k_i.IdClsf.HasValue OrElse _labelOf.ContainsKey(k_i.IdClsf.Value) Then Continue For
            Dim k_label As String = If(String.IsNullOrWhiteSpace(k_i.Denumire), k_i.Clsf, $"{k_i.Clsf}  {k_i.Denumire}")
            _byLabel(k_label) = k_i.IdClsf.Value
            _labelOf(k_i.IdClsf.Value) = k_label
            k_choices.Add(k_label)
        Next
        If k_data.Indicatori.Count = 0 Then
            ShowNotice("Angajamentul nu are indicatori: o recepție nu poate fi făcută până nu are rezervări (documentul de fundamentare).", NoticeKind.Warning)
        End If
        For Each k_l As EFacturaKeLinie In _stare.Linii
            If k_l.IdClsf.HasValue AndAlso Not _labelOf.ContainsKey(k_l.IdClsf.Value) Then
                Dim k_label As String = $"{k_l.Clsf} (nu e indicator al angajamentului)"
                _byLabel(k_label) = k_l.IdClsf.Value
                _labelOf(k_l.IdClsf.Value) = k_label
                k_choices.Add(k_label)
            End If
        Next
        _loading = True
        Try
            gridLinii.Column(ColClsf).ComboItems = k_choices
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_line As EFacturaKeLinie = TryCast(k_row.Tag, EFacturaKeLinie)
                Dim k_current As String = Convert.ToString(k_row(ColClsf), CultureInfo.InvariantCulture)
                If k_line IsNot Nothing AndAlso k_line.IdClsf.HasValue AndAlso _labelOf.ContainsKey(k_line.IdClsf.Value) Then
                    k_row(ColClsf) = _labelOf(k_line.IdClsf.Value)
                ElseIf String.IsNullOrEmpty(k_current) Then
                    k_row(ColClsf) = NeAleasa
                End If
            Next
        Finally
            _loading = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' Only a choice from the list is accepted in a cell.
    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridLinii.CellValidating
        Try
            If e.ColumnKey <> ColClsf Then Return
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) Then
                e.ProposedValue = NeAleasa
            ElseIf Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso Not _byLabel.ContainsKey(k_text) Then
                e.Cancel = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.GridLinii_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    ' The first line's choice goes on all the lines (the usual invoice has one classification).
    Private Sub BtnToate_Click(sender As Object, e As EventArgs) Handles btnToate.Click
        Try
            If gridLinii.RowCount = 0 Then Return
            Dim k_first As String = Convert.ToString(gridLinii.Rows(0)(ColClsf), CultureInfo.InvariantCulture)
            For Each k_row As KBotDataRow In gridLinii.Rows
                k_row(ColClsf) = k_first
            Next
            gridLinii.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.BtnToate_Click", ex)
        End Try
    End Sub

    ' Another angajament, picked from the list of all.
    Private Sub BtnAltul_Click(sender As Object, e As EventArgs) Handles btnAltul.Click
        Try
            Using k_f As New KeAngajamentForm(_api, _gate, _invoiceLabel)
                If k_f.ShowDialog(Me) <> DialogResult.OK OrElse k_f.Ales Is Nothing Then Return
                SelectOrAdd(k_f.Ales.CodAngajament, k_f.Ales.Eticheta)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.BtnAltul_Click", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): writes the choices; a failure stays in the window as a notice.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            If _api Is Nothing Then Return
            Dim k_items As New List(Of EFacturaKeLinie)()
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_line As EFacturaKeLinie = TryCast(k_row.Tag, EFacturaKeLinie)
                If k_line Is Nothing Then Continue For
                Dim k_text As String = Convert.ToString(k_row(ColClsf), CultureInfo.InvariantCulture)
                Dim k_id As Integer = 0
                Dim k_chosen As Boolean = Not String.IsNullOrWhiteSpace(k_text) AndAlso
                    Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso _byLabel.TryGetValue(k_text, k_id)
                If Not k_chosen AndAlso k_line.IdClsf.HasValue AndAlso _byLabel.Count = 0 Then
                    ' no angajament was opened: the lines keep what they have
                    k_id = k_line.IdClsf.Value
                    k_chosen = True
                End If
                k_items.Add(New EFacturaKeLinie() With {.IdLinie = k_line.IdLinie, .IdClsf = If(k_chosen, CType(k_id, Integer?), Nothing)})
            Next
            btnSalveaza.Enabled = False
            Dim k_a As EFacturaKeAngajament = TryCast(cmbAngajament.SelectedItem, EFacturaKeAngajament)
            Dim k_state As EFacturaKeStare = Await _gate.RunAsync(
                Function() _api.SetKeClasificatiiAsync(_idPrimita, k_items, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            _Salvata = k_state
            _CodAngajament = If(k_a IsNot Nothing, k_a.CodAngajament, String.Empty)
            DialogResult = DialogResult.OK
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeClasificariForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then
                btnSalveaza.Enabled = _stare.PoateScrie
                ShowNotice(ex.Message, NoticeKind.Error)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeClasificariForm.BtnSalveaza_Click", ex)
            If Not IsDisposed Then
                btnSalveaza.Enabled = _stare.PoateScrie
                ShowNotice("Clasificațiile nu au putut fi salvate. Detalii în jurnalul de erori.", NoticeKind.Error)
            End If
        End Try
    End Sub

End Class
