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
''' Slices KE04..KE07, 00EF-39 -- the window where the accountant says how an invoice is covered by a reception. One window, three modes
''' (<see cref="EFacturaKeTip"/>): link to an EXISTING reception that already has the value, make a NEW reception for the invoice (type, date, description
''' and the value on each classification), or MODIFY an existing reception that is too small (its values grow by the allocated amounts). The window only
''' COLLECTS: it reads the angajament's receptions and indicators from the server, proposes the amounts (the rest of the invoice on each classification,
''' cut to what the reception still has when the reception exists) and, on OK, hands back <see cref="Cerere"/> and the fields of the form.
''' It also collects the classification of each LINE of the invoice (slice 00EF-39; it used to be a window of its own): the choices are the indicators of the
''' angajament, and the amounts to allocate are the lines' values added up per chosen classification. The choices are handed back in <see cref="Clasificari"/>.
''' What follows (saving the choices, the server's check, the robot, the write of the link) is the work of <see cref="PrimiteView"/>.
''' </summary>
Public Class KeLegaForm

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")
    Private Const ColClsf As String = "clsf"
    Private Const ColIndicator As String = "indicator"
    Private Const ColRestFactura As String = "restFactura"
    Private Const ColValRecep As String = "valRecep"
    Private Const ColRestRecep As String = "restRecep"
    Private Const ColSuma As String = "suma"
    Private Const ColRData As String = "data"
    Private Const ColRTip As String = "tip"
    Private Const ColRDescriere As String = "descriere"
    Private Const ColRTotal As String = "total"
    Private Const ColRRest As String = "rest"
    Private Const NeAleasa As String = "(nealeasă)"
    Private Const ColLNr As String = "nr"
    Private Const ColLDenumire As String = "denumire"
    Private Const ColLCuTva As String = "cuTva"
    Private Const ColLClsf As String = "clsf"

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _idPrimita As Integer
    Private ReadOnly _invoiceLabel As String
    Private ReadOnly _mode As String
    Private ReadOnly _stare As EFacturaKeStare
    Private ReadOnly _tipuri As String() = {"Avans", "Partial", "Final"}
    Private ReadOnly _byLabel As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _labelOf As New Dictionary(Of Integer, String)()
    Private _data As EFacturaKeReceptiiAngajament
    Private _seq As Integer
    Private _filling As Boolean

    ''' <summary>What was asked (valid when the window closed with OK).</summary>
    Public ReadOnly Property Cerere As EFacturaKeCerere

    ''' <summary>The reception chosen (existing or to modify); Nothing for a new one.</summary>
    Public ReadOnly Property Receptie As EFacturaKeReceptie

    ''' <summary>The type of a new reception: «Avans», «Partial» or «Final».</summary>
    Public ReadOnly Property TipReceptie As String = "Final"

    ''' <summary>The date of a new reception.</summary>
    Public ReadOnly Property DataReceptie As Date = Date.Today

    ''' <summary>The description of a new reception, without the K-BOT marker (the marker is appended later).</summary>
    Public ReadOnly Property DescriereReceptie As String = String.Empty

    ''' <summary>The classification of every line of the invoice as chosen in the window (<c>IdClsf</c> Nothing = not chosen); valid when the window closed with OK.</summary>
    Public ReadOnly Property Clasificari As New List(Of EFacturaKeLinie)()

    ''' <summary>At least one line got a classification different from the one it had when the window opened.</summary>
    Public ReadOnly Property ClasificariSchimbate As Boolean

    ''' <summary>The indicators of the angajament chosen (the robot needs their codes).</summary>
    Public ReadOnly Property Indicatori As IReadOnlyList(Of EFacturaKeIndicator)
        Get
            Return If(_data Is Nothing, New List(Of EFacturaKeIndicator)(), CType(_data.Indicatori, IReadOnlyList(Of EFacturaKeIndicator)))
        End Get
    End Property

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_mode">One of <see cref="EFacturaKeTip"/>.</param>
    ''' <param name="k_stare">The state read from the server: the classifications with their rest, the angajamente the invoice can go to.</param>
    ''' <param name="k_cod">An angajament to start with (the one already worked on), or empty.</param>
    ''' <param name="k_dataImplicita">The date proposed for a new reception.</param>
    ''' <param name="k_descriereImplicita">The description proposed for a new reception.</param>
    Public Sub New(k_api As IEFacturaApi, k_gate As ReauthGate, k_idPrimita As Integer, k_invoiceLabel As String, k_mode As String,
                   k_stare As EFacturaKeStare, k_cod As String, k_dataImplicita As Date, k_descriereImplicita As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentNullException.ThrowIfNull(k_stare)
        If k_mode <> EFacturaKeTip.Existenta AndAlso k_mode <> EFacturaKeTip.Noua AndAlso k_mode <> EFacturaKeTip.Modificata Then
            Throw New ArgumentException("Unknown kind of link: " & k_mode, NameOf(k_mode))
        End If
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _idPrimita = k_idPrimita
        _invoiceLabel = k_invoiceLabel
        _mode = k_mode
        _stare = k_stare
        cmbAngajament.CaptionSelector = Function(k_o) CaptionOf(k_o)
        FillLines()
        cmbTip.Items.AddRange(_tipuri.Cast(Of Object)())
        cmbTip.SelectedIndex = Array.IndexOf(_tipuri, "Final")
        dtpData.Value = k_dataImplicita
        txtDescriere.Text = If(k_descriereImplicita, String.Empty)
        For Each k_a As EFacturaKeAngajament In k_stare.Angajamente
            cmbAngajament.Items.Add(k_a)
        Next
        If Not String.IsNullOrWhiteSpace(k_cod) Then SelectOrAdd(k_cod, String.Empty)
        ApplyMode()
    End Sub

    Private Shared Function CaptionOf(k_item As Object) As String
        Dim k_a As EFacturaKeAngajament = TryCast(k_item, EFacturaKeAngajament)
        If k_a Is Nothing Then Return Convert.ToString(k_item, CultureInfo.InvariantCulture)
        Return If(String.IsNullOrWhiteSpace(k_a.Descriere), k_a.CodAngajament, $"{k_a.CodAngajament} — {k_a.Descriere}")
    End Function

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

    ' What the mode shows: a list of receptions (existing / modified) or the form of a new one; the title and the button follow.
    Private Sub ApplyMode()
        Dim k_nou As Boolean = (_mode = EFacturaKeTip.Noua)
        gridReceptii.Visible = Not k_nou
        pnlNoua.Visible = k_nou
        Select Case _mode
            Case EFacturaKeTip.Existenta
                capBar.Text = "E-Factura — legare de o recepție existentă"
                Text = capBar.Text
                btnOk.Text = "Leagă"
                tips.SetToolTipText(btnOk, "Leagă factura de recepția aleasă, pe sumele de mai jos. Recepția nu se schimbă.")
                lblAntet.Text = $"Factura {_invoiceLabel}: alege clasificația fiecărei linii, recepția care o acoperă și sumele alocate pe fiecare clasificație."
            Case EFacturaKeTip.Noua
                capBar.Text = "E-Factura — recepție nouă"
                Text = capBar.Text
                btnOk.Text = "Creează recepția"
                tips.SetToolTipText(btnOk, "Creează în FOREXE recepția cu valorile de mai jos și o leagă de factură.")
                lblAntet.Text = $"Factura {_invoiceLabel}: alege clasificația fiecărei linii; recepția nouă se face cu valoarea facturii (sau doar cu o parte din ea)."
            Case Else
                capBar.Text = "E-Factura — modificarea unei recepții"
                Text = capBar.Text
                btnOk.Text = "Modifică recepția"
                tips.SetToolTipText(btnOk, "Mărește în FOREXE recepția aleasă cu sumele de mai jos și o leagă de factură.")
                lblAntet.Text = $"Factura {_invoiceLabel}: alege clasificația fiecărei linii și recepția care nu ajunge; valorile ei cresc cu sumele alocate."
        End Select
        gridAloc.Column(ColValRecep).Visible = If(k_nou, KBotColumnVisibility.Hidden, KBotColumnVisibility.Visible)
        gridAloc.Column(ColRestRecep).Visible = If(_mode = EFacturaKeTip.Existenta, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
        btnOk.Enabled = False
    End Sub

    ' UI boundary (Load): the angajament is chosen when there is exactly one.
    Private Sub KeLegaForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            If cmbAngajament.SelectedIndex < 0 AndAlso cmbAngajament.Items.Count = 1 Then cmbAngajament.SelectedIndex = 0
            If cmbAngajament.Items.Count = 0 Then
                ShowNotice("Furnizorul nu are niciun DDF legat: alegeți angajamentul cu «Alt angajament…».", NoticeKind.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.KeLegaForm_Load", ex)
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

    ' UI boundary (async Sub): the angajament changed: read its receptions and indicators.
    Private Async Sub CmbAngajament_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAngajament.SelectedIndexChanged
        Try
            Dim k_a As EFacturaKeAngajament = TryCast(cmbAngajament.SelectedItem, EFacturaKeAngajament)
            If k_a Is Nothing OrElse _api Is Nothing Then Return
            Dim k_mySeq As Integer = Interlocked.Increment(_seq)
            ntfMesaj.Visible = False
            btnOk.Enabled = False
            Dim k_data As EFacturaKeReceptiiAngajament = Await _gate.RunAsync(
                Function() _api.GetKeReceptiiAsync(k_a.CodAngajament, _cts.Token)).ConfigureAwait(True)
            If IsDisposed OrElse k_mySeq <> _seq Then Return
            _data = k_data
            ApplyIndicators()
            FillReceptii()
            FillAlocari()
        Catch ex As OperationCanceledException
            ' The window was closed.
        Catch ex As ApiException
            GlobalErrorLog.Write("KeLegaForm.CmbAngajament_SelectedIndexChanged", ex)
            ShowNotice(ex.Message, NoticeKind.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.CmbAngajament_SelectedIndexChanged", ex)
            ShowNotice("Recepțiile angajamentului nu au putut fi citite. Detalii în jurnalul de erori.", NoticeKind.Error)
        End Try
    End Sub

    Private Sub FillReceptii()
        gridReceptii.BeginUpdate()
        Try
            gridReceptii.ClearRows()
            If _data Is Nothing Then Return
            For Each k_r As EFacturaKeReceptie In _data.Receptii
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
        Return TryCast(gridReceptii.CurrentRow?.Tag, EFacturaKeReceptie)
    End Function

    Private Sub GridReceptii_SelectionChanged(sender As Object, e As EventArgs) Handles gridReceptii.SelectionChanged
        Try
            FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridReceptii_SelectionChanged", ex)
        End Try
    End Sub

    Private Function IndicatorOf(k_idClsf As Integer) As EFacturaKeIndicator
        If _data Is Nothing Then Return Nothing
        Return _data.Indicatori.FirstOrDefault(Function(k_i) k_i.IdClsf.HasValue AndAlso k_i.IdClsf.Value = k_idClsf)
    End Function

    ' One row per classification of the invoice that still has a rest: the amount proposed is the rest of the invoice, cut to what an existing reception still has.
    Private Sub FillAlocari()
        _filling = True
        gridAloc.BeginUpdate()
        Try
            gridAloc.ClearRows()
            Dim k_rec As EFacturaKeReceptie = If(_mode = EFacturaKeTip.Noua, Nothing, ChosenReceptie())
            For Each k_c As EFacturaKeClasificatie In ClassesOfLines()
                If Math.Abs(k_c.Rest) < 0.005D Then Continue For
                Dim k_ind As EFacturaKeIndicator = IndicatorOf(k_c.IdClsf)
                Dim k_row As KBotDataRow = gridAloc.AddRow()
                k_row.Tag = k_c
                k_row(ColClsf) = k_c.Clsf
                k_row(ColIndicator) = If(k_ind IsNot Nothing, k_ind.CodIndicator, "lipsește")
                k_row(ColRestFactura) = CDbl(k_c.Rest)
                Dim k_sum As Decimal = k_c.Rest
                If k_rec IsNot Nothing Then
                    Dim k_pc As EFacturaKeReceptieClasificatie = k_rec.PeClasificatie.FirstOrDefault(Function(k_x) k_x.IdClsf = k_c.IdClsf)
                    k_row(ColValRecep) = If(k_pc IsNot Nothing, CDbl(k_pc.Valoare), 0.0R)
                    k_row(ColRestRecep) = If(k_pc IsNot Nothing, CDbl(k_pc.Rest), 0.0R)
                    If _mode = EFacturaKeTip.Existenta Then
                        Dim k_left As Decimal = If(k_pc IsNot Nothing, k_pc.Rest, 0D)
                        k_sum = If(Math.Sign(k_left) <> Math.Sign(k_c.Rest), 0D, If(Math.Abs(k_left) < Math.Abs(k_c.Rest), k_left, k_c.Rest))
                    End If
                End If
                k_row(ColSuma) = CDbl(k_sum)
            Next
        Finally
            gridAloc.EndUpdate()
            _filling = False
        End Try
        RefreshOk()
    End Sub

    ' ── The lines of the invoice and their classifications (slice 00EF-39; the logic of the old window «Clasificatiile liniilor») ──

    Private Sub FillLines()
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            For Each k_l As EFacturaKeLinie In _stare.Linii
                Dim k_row As KBotDataRow = gridLinii.AddRow()
                k_row.Tag = k_l
                k_row(ColLNr) = k_l.NrLinie
                k_row(ColLDenumire) = k_l.Denumire
                k_row(ColLCuTva) = CDbl(k_l.ValoareCuTva)
                k_row(ColLClsf) = If(k_l.IdClsf.HasValue, If(k_l.Clsf, String.Empty), NeAleasa)
            Next
        Finally
            gridLinii.EndUpdate()
        End Try
    End Sub

    ' The indicators of the angajament are the choices; a classification a line already has that is not one of them stays visible.
    Private Sub ApplyIndicators()
        _byLabel.Clear()
        _labelOf.Clear()
        Dim k_choices As New List(Of Object) From {NeAleasa}
        For Each k_i As EFacturaKeIndicator In _data.Indicatori
            If Not k_i.IdClsf.HasValue OrElse _labelOf.ContainsKey(k_i.IdClsf.Value) Then Continue For
            Dim k_label As String = If(String.IsNullOrWhiteSpace(k_i.Denumire), k_i.Clsf, $"{k_i.Clsf}  {k_i.Denumire}")
            _byLabel(k_label) = k_i.IdClsf.Value
            _labelOf(k_i.IdClsf.Value) = k_label
            k_choices.Add(k_label)
        Next
        If _data.Indicatori.Count = 0 Then
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
        _filling = True
        Try
            gridLinii.Column(ColLClsf).ComboItems = k_choices
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_line As EFacturaKeLinie = TryCast(k_row.Tag, EFacturaKeLinie)
                Dim k_current As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
                If k_line IsNot Nothing AndAlso k_line.IdClsf.HasValue AndAlso _labelOf.ContainsKey(k_line.IdClsf.Value) Then
                    k_row(ColLClsf) = _labelOf(k_line.IdClsf.Value)
                ElseIf String.IsNullOrEmpty(k_current) OrElse
                       (Not String.Equals(k_current, NeAleasa, StringComparison.Ordinal) AndAlso Not _byLabel.ContainsKey(k_current)) Then
                    ' a choice made from another angajament's list is not one of these: the cell goes back to «not chosen»
                    k_row(ColLClsf) = NeAleasa
                End If
            Next
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' The classification the row stands for: the one picked from the list; before any angajament was opened the line keeps what it has.
    Private Function ChosenClass(k_row As KBotDataRow) As Integer?
        Dim k_line As EFacturaKeLinie = TryCast(k_row.Tag, EFacturaKeLinie)
        If k_line Is Nothing Then Return Nothing
        Dim k_text As String = Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture)
        Dim k_id As Integer = 0
        If Not String.IsNullOrWhiteSpace(k_text) AndAlso Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso
           _byLabel.TryGetValue(k_text, k_id) Then Return k_id
        If _byLabel.Count = 0 AndAlso k_line.IdClsf.HasValue Then Return k_line.IdClsf
        Return Nothing
    End Function

    ' The classifications the lines now stand for, with the value of their lines (WITH VAT, the invoice's sign), what links already allocate
    ' and what is left. This replaces what the server used to answer from the saved classifications of the lines.
    Private Function ClassesOfLines() As List(Of EFacturaKeClasificatie)
        Dim k_classes As New List(Of EFacturaKeClasificatie)()
        For Each k_row As KBotDataRow In gridLinii.Rows
            Dim k_line As EFacturaKeLinie = TryCast(k_row.Tag, EFacturaKeLinie)
            Dim k_id As Integer? = ChosenClass(k_row)
            If k_line Is Nothing OrElse Not k_id.HasValue Then Continue For
            Dim k_idValue As Integer = k_id.Value
            Dim k_c As EFacturaKeClasificatie = k_classes.FirstOrDefault(Function(k_x) k_x.IdClsf = k_idValue)
            If k_c Is Nothing Then
                Dim k_ind As EFacturaKeIndicator = IndicatorOf(k_idValue)
                k_c = New EFacturaKeClasificatie() With {
                    .IdClsf = k_idValue,
                    .Clsf = If(k_ind IsNot Nothing, k_ind.Clsf, If(String.IsNullOrWhiteSpace(k_line.Clsf), Convert.ToString(k_row(ColLClsf), CultureInfo.InvariantCulture), k_line.Clsf)),
                    .Alocat = _stare.Legaturi.Where(Function(k_g) k_g.IdClsf = k_idValue).Sum(Function(k_g) k_g.Suma)}
                k_classes.Add(k_c)
            End If
            k_c.Valoare += k_line.ValoareCuTva
        Next
        For Each k_c As EFacturaKeClasificatie In k_classes
            k_c.Rest = k_c.Valoare - k_c.Alocat
        Next
        Return k_classes
    End Function

    ' Only a choice from the list is accepted in a cell.
    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridLinii.CellValidating
        Try
            If e.ColumnKey <> ColLClsf Then Return
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(k_text) Then
                e.ProposedValue = NeAleasa
            ElseIf Not String.Equals(k_text, NeAleasa, StringComparison.Ordinal) AndAlso Not _byLabel.ContainsKey(k_text) Then
                e.Cancel = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    ' A classification changed: the amounts to allocate follow.
    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridLinii.CellValueChanged
        Try
            If Not _filling AndAlso e.ColumnKey = ColLClsf Then FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

    ' The first line's choice goes on all the lines (the usual invoice has one classification).
    Private Sub BtnToate_Click(sender As Object, e As EventArgs) Handles btnToate.Click
        Try
            If gridLinii.RowCount = 0 Then Return
            Dim k_first As String = Convert.ToString(gridLinii.Rows(0)(ColLClsf), CultureInfo.InvariantCulture)
            _filling = True
            Try
                For Each k_row As KBotDataRow In gridLinii.Rows
                    k_row(ColLClsf) = k_first
                Next
            Finally
                _filling = False
            End Try
            gridLinii.Invalidate()
            FillAlocari()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BtnToate_Click", ex)
        End Try
    End Sub

    ' A typed amount must be a number (ro-RO: «1.234,56»); empty = 0.
    Private Sub GridAloc_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridAloc.CellValidating
        Try
            If e.ColumnKey <> ColSuma Then Return
            Dim k_number As Decimal
            If Not ReadNumber(e.ProposedValue, k_number) Then
                ShowNotice("Suma nu este un număr.", NoticeKind.Warning)
                e.Cancel = True
                Return
            End If
            e.ProposedValue = CDbl(k_number)
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridAloc_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    Private Sub GridAloc_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridAloc.CellValueChanged
        Try
            If Not _filling Then RefreshOk()
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.GridAloc_CellValueChanged", ex)
        End Try
    End Sub

    Private Shared Function ReadNumber(k_value As Object, ByRef k_number As Decimal) As Boolean
        If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then
            k_number = 0D
            Return True
        End If
        If TypeOf k_value Is Double Then
            k_number = Math.Round(CDec(DirectCast(k_value, Double)), 2)
            Return True
        End If
        Dim k_text As String = Convert.ToString(k_value, _ro).Trim()
        If k_text.Length = 0 Then
            k_number = 0D
            Return True
        End If
        Dim k_parsed As Decimal
        If Decimal.TryParse(k_text, NumberStyles.Number Or NumberStyles.AllowLeadingSign, _ro, k_parsed) Then
            k_number = Math.Round(k_parsed, 2)
            Return True
        End If
        Return False
    End Function

    ' The amounts typed, per classification, zero ones left out.
    Private Function TypedAmounts() As List(Of EFacturaKeAlocare)
        Dim k_list As New List(Of EFacturaKeAlocare)()
        For Each k_row As KBotDataRow In gridAloc.Rows
            Dim k_c As EFacturaKeClasificatie = TryCast(k_row.Tag, EFacturaKeClasificatie)
            Dim k_number As Decimal
            If k_c Is Nothing OrElse Not ReadNumber(k_row(ColSuma), k_number) OrElse k_number = 0D Then Continue For
            k_list.Add(New EFacturaKeAlocare() With {.IdClsf = k_c.IdClsf, .Suma = k_number})
        Next
        Return k_list
    End Function

    Private Sub RefreshOk()
        Dim k_ok As Boolean = TypedAmounts().Count > 0 AndAlso _data IsNot Nothing AndAlso _stare.PoateScrie
        If _mode <> EFacturaKeTip.Noua Then k_ok = k_ok AndAlso ChosenReceptie() IsNot Nothing
        btnOk.Enabled = k_ok
    End Sub

    ' Another angajament, picked from the list of all.
    Private Sub BtnAltul_Click(sender As Object, e As EventArgs) Handles btnAltul.Click
        Try
            Using k_f As New KeAngajamentForm(_api, _gate, _invoiceLabel)
                If k_f.ShowDialog(Me) <> DialogResult.OK OrElse k_f.Ales Is Nothing Then Return
                SelectOrAdd(k_f.Ales.CodAngajament, k_f.Ales.Eticheta)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BtnAltul_Click", ex)
        End Try
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            Dim k_a As EFacturaKeAngajament = TryCast(cmbAngajament.SelectedItem, EFacturaKeAngajament)
            If k_a Is Nothing OrElse _data Is Nothing Then Return
            Dim k_amounts As List(Of EFacturaKeAlocare) = TypedAmounts()
            If k_amounts.Count = 0 Then Return
            ' A classification that is not an indicator of the angajament cannot be received: the law says the indicator comes first.
            For Each k_x As EFacturaKeAlocare In k_amounts
                If IndicatorOf(k_x.IdClsf) Is Nothing Then
                    ShowNotice("O clasificație nu este indicator al angajamentului: este nevoie întâi de o revizie nouă de rezervare " &
                               "(documentul de fundamentare), apoi de recepție.", NoticeKind.Error)
                    Return
                End If
            Next
            Dim k_rec As EFacturaKeReceptie = If(_mode = EFacturaKeTip.Noua, Nothing, ChosenReceptie())
            If _mode <> EFacturaKeTip.Noua AndAlso k_rec Is Nothing Then Return
            If _mode = EFacturaKeTip.Noua AndAlso (cmbTip.SelectedIndex < 0 OrElse String.IsNullOrWhiteSpace(txtDescriere.Text)) Then
                ShowNotice("Alegeți tipul și scrieți descrierea recepției.", NoticeKind.Warning)
                Return
            End If
            ' Optional (Setari): one reception per day on an angajament. K-BOT's own receptions carry their numbers, so this is only a wish.
            If _mode = EFacturaKeTip.Noua AndAlso AppSettings.Current.KeBlockSameDayReception Then
                Dim k_day As Date = dtpData.Value.Date
                If _data.Receptii.Any(Function(k_x) k_x.DataR.HasValue AndAlso k_x.DataR.Value.Date = k_day) Then
                    ShowNotice($"Angajamentul are deja o recepție pe {k_day:dd.MM.yyyy}. Modificați-o (butonul «Modifică recepția») sau alegeți altă dată.",
                               NoticeKind.Warning)
                    Return
                End If
            End If
            Dim k_chosen As New List(Of EFacturaKeLinie)()
            Dim k_changed As Boolean = False
            For Each k_row As KBotDataRow In gridLinii.Rows
                Dim k_line As EFacturaKeLinie = TryCast(k_row.Tag, EFacturaKeLinie)
                If k_line Is Nothing Then Continue For
                Dim k_id As Integer? = ChosenClass(k_row)
                If Not Nullable.Equals(k_id, k_line.IdClsf) Then k_changed = True
                k_chosen.Add(New EFacturaKeLinie() With {.IdLinie = k_line.IdLinie, .IdClsf = k_id})
            Next
            Dim k_cerere As New EFacturaKeCerere() With {.CodAngajament = k_a.CodAngajament, .Tip = _mode}
            If k_rec IsNot Nothing Then k_cerere.Idrr = k_rec.Idrr
            k_cerere.Alocari.AddRange(k_amounts)
            _Cerere = k_cerere
            _Clasificari.Clear()
            _Clasificari.AddRange(k_chosen)
            _ClasificariSchimbate = k_changed
            _Receptie = k_rec
            If _mode = EFacturaKeTip.Noua Then
                _TipReceptie = Convert.ToString(cmbTip.SelectedItem, CultureInfo.InvariantCulture)
                _DataReceptie = dtpData.Value.Date
                _DescriereReceptie = txtDescriere.Text.Trim()
            End If
            DialogResult = DialogResult.OK
        Catch ex As Exception
            GlobalErrorLog.Write("KeLegaForm.BtnOk_Click", ex)
        End Try
    End Sub

End Class
