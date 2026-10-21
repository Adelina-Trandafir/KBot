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
''' have no link to a reception and, for each, the reception the server PROPOSES: the one that has, whole, the value of the invoice. The accountant looks, ticks what she agrees
''' with and presses «Leagă»; until then nothing is saved.
''' Since 20.10.2026 she can also: pick the angajament of an invoice (when the supplier has several), write in «Recepție» how much she receives from the invoice (not above its
''' value), and pick another reception in the table below. A reception may serve several invoices while its rest lasts: what the ticked invoices already take from it is
''' subtracted before the table below is drawn («Rămas»), and a reception with nothing left is not listed. The table below has two groups: the receptions whose rest equals the
''' sum to receive, then the rest of the receptions that are not fully associated.
''' </summary>
Public Class KePropuneriForm

    Private Const Tolerance As Decimal = 0.005D
    Private Const ColSuma As String = "suma"
    Private Const ColAng As String = "ang"
    Private Const GrupAceeasi As String = "1 · Aceeași valoare ca suma de recepționat"
    Private Const GrupAlte As String = "2 · Neasociate integral"

    ' What a row of the invoices grid stands for: the invoice, the angajament and the reception row chosen for it (Nothing = none yet) and the sum it takes from it.
    Private NotInheritable Class RowState
        Public Property Factura As EFacturaKePropunereFactura
        Public Property Cod As String = String.Empty
        Public Property Chosen As EFacturaKePropunereReceptie

        ''' <summary>What the invoice takes from the reception chosen (with the invoice's sign); 0 while no reception is chosen.</summary>
        Public Property Suma As Decimal

        ''' <summary>The accountant wrote the sum herself: it is the one looked for when she picks another reception.</summary>
        Public Property SumaScrisa As Boolean

        ' The accountant picked the reception herself (it was not the proposal).
        Public Property ByHand As Boolean
    End Class

    Private ReadOnly _api As IEFacturaApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _ids As List(Of Integer)
    Private ReadOnly _angajamente As List(Of String)
    Private ReadOnly _labelOf As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _codOf As New Dictionary(Of String, String)(StringComparer.Ordinal)
    Private _answer As EFacturaKePropuneri
    Private _filling As Boolean
    Private _busy As Boolean

    ''' <summary>How many invoices were linked (valid when the window closed with OK).</summary>
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
        lblAntet.Text = $"Facturile neasociate ale furnizorului {k_supplier}. K-BOT propune, pentru fiecare, recepția care are exact valoarea facturii. Poți schimba angajamentul " &
                        "(când furnizorul are mai multe), alege altă recepție în tabelul de jos (dublu clic) și scrie în «Recepție» cât primești din factură; o recepție poate " &
                        "acoperi mai multe facturi cât îi rămâne. Bifează ce ești de acord să legi și apasă «Leagă»; până atunci nu se salvează nimic."
        AddHandler gridFact.SelectionChanged, AddressOf GridFact_SelectionChanged
        AddHandler gridFact.RowChecked, AddressOf GridFact_RowChecked
        AddHandler gridFact.CellFormatting, AddressOf GridFact_CellFormatting
        AddHandler gridFact.CellValidating, AddressOf GridFact_CellValidating
        AddHandler gridFact.CellValueChanged, AddressOf GridFact_CellValueChanged
        AddHandler gridCand.CellDoubleClick, AddressOf GridCand_CellDoubleClick
    End Sub

    ' UI boundary (Load, async Sub): logs and swallows; a failure becomes a notice.
    Private Async Sub KePropuneriForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If _api Is Nothing Then Return
            SetBusy(True)
            _answer = Await _gate.RunAsync(Function() _api.GetKePropuneriAsync(_ids, _angajamente, _cts.Token)).ConfigureAwait(True)
            If IsDisposed Then Return
            FillLabels()
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

    Private Shared Function Fmt(k_value As Decimal) As String
        Return k_value.ToString("N2", CultureInfo.CurrentCulture)
    End Function

    ' A typed amount must be a number in the operator's own regional format (the cells are painted in it); empty = 0.
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

    Private Shared Function SameCod(k_a As String, k_b As String) As Boolean
        Return String.Equals(k_a, k_b, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function SameReception(k_a As EFacturaKePropunereReceptie, k_b As EFacturaKePropunereReceptie) As Boolean
        Return k_a.Idrr = k_b.Idrr AndAlso k_a.IdClsf = k_b.IdClsf AndAlso SameCod(k_a.CodAngajament, k_b.CodAngajament)
    End Function

    Private Shared Function KeyOf(k_c As EFacturaKePropunereReceptie) As String
        Return $"{k_c.CodAngajament.ToUpperInvariant()}|{k_c.Idrr}|{k_c.IdClsf}"
    End Function

    ' ── The angajamente ─────────────────────────────────────────────────────────

    Private Sub FillLabels()
        _labelOf.Clear()
        _codOf.Clear()
        For Each k_a As EFacturaKePropunereAngajament In _answer.Angajamente
            Dim k_label As String = If(String.IsNullOrWhiteSpace(k_a.Descriere), k_a.CodAngajament, $"{k_a.CodAngajament} — {k_a.Descriere}")
            _labelOf(k_a.CodAngajament) = k_label
            _codOf(k_label) = k_a.CodAngajament
        Next
    End Sub

    Private Function LabelOf(k_cod As String) As String
        Dim k_label As String = Nothing
        Return If(_labelOf.TryGetValue(k_cod, k_label), k_label, k_cod)
    End Function

    ' ── The invoices ────────────────────────────────────────────────────────────

    Private Sub FillFacturi()
        _filling = True
        gridFact.BeginUpdate()
        Try
            gridFact.ClearRows()
            For Each k_f As EFacturaKePropunereFactura In _answer.Facturi
                Dim k_state As New RowState() With {.Factura = k_f}
                k_state.Cod = If(Not String.IsNullOrWhiteSpace(k_f.Angajament), k_f.Angajament, If(k_f.Angajamente.Count > 0, k_f.Angajamente(0), String.Empty))
                If k_f.Propunere IsNot Nothing Then
                    k_state.Chosen = _answer.Receptii.FirstOrDefault(
                        Function(k_c) SameCod(k_c.CodAngajament, k_f.Propunere.CodAngajament) AndAlso k_c.Idrr = k_f.Propunere.Idrr AndAlso k_c.IdClsf = k_f.Propunere.IdClsf)
                    If k_state.Chosen IsNot Nothing Then
                        k_state.Cod = k_state.Chosen.CodAngajament
                        k_state.Suma = k_f.Total
                    End If
                End If
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

    ' The cells of a row from its state: the angajament, the sum, the reception chosen, or why there is none.
    Private Sub ShowRow(k_row As KBotDataRow)
        Dim k_state As RowState = CType(k_row.Tag, RowState)
        Dim k_f As EFacturaKePropunereFactura = k_state.Factura
        k_row("factura") = k_f.NrFact
        k_row("data") = DayText(k_f.DataFact)
        k_row("total") = CDbl(k_f.Total)
        k_row(ColSuma) = CDbl(k_state.Suma)
        k_row(ColAng) = If(String.IsNullOrEmpty(k_state.Cod), String.Empty, LabelOf(k_state.Cod))
        If k_state.Chosen IsNot Nothing Then
            k_row("receptie") = k_state.Chosen.Descriere
            k_row("dataR") = DayText(k_state.Chosen.DataR)
            k_row("obs") = Remarks(k_state)
        Else
            k_row("receptie") = String.Empty
            k_row("dataR") = String.Empty
            k_row("obs") = If(k_f.Angajamente.Count = 0 OrElse Not String.IsNullOrEmpty(k_f.Motiv), k_f.Motiv, "Alege o recepție în tabelul de jos (dublu clic).")
        End If
    End Sub

    Private Shared Function Remarks(k_state As RowState) As String
        Dim k_parts As New List(Of String)()
        If Not String.IsNullOrEmpty(k_state.Chosen.Clsf) Then k_parts.Add(k_state.Chosen.Clsf)
        If k_state.ByHand Then k_parts.Add("aleasă de dumneavoastră")
        Dim k_left As Decimal = k_state.Factura.Total - k_state.Suma
        If Math.Abs(k_left) > Tolerance Then k_parts.Add($"se recepționează o parte, rămân {Fmt(k_left)} din factură")
        Return String.Join(" · ", k_parts)
    End Function

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

    ' What the ticked invoices (other than k_except) already take from each reception row: {key -> sum}.
    Private Function TakenMap(k_except As KBotDataRow) As Dictionary(Of String, Decimal)
        Dim k_map As New Dictionary(Of String, Decimal)(StringComparer.Ordinal)
        For Each k_row As KBotDataRow In Ticked()
            If Object.ReferenceEquals(k_row, k_except) Then Continue For
            Dim k_state As RowState = CType(k_row.Tag, RowState)
            If k_state.Chosen Is Nothing Then Continue For
            Dim k_key As String = KeyOf(k_state.Chosen)
            Dim k_sum As Decimal = 0D
            k_map.TryGetValue(k_key, k_sum)
            k_map(k_key) = k_sum + k_state.Suma
        Next
        Return k_map
    End Function

    ' What the reception row still has for the invoice of k_except: its rest minus what the OTHER ticked invoices take.
    Private Function Available(k_reception As EFacturaKePropunereReceptie, k_except As KBotDataRow) As Decimal
        Dim k_taken As Decimal = 0D
        TakenMap(k_except).TryGetValue(KeyOf(k_reception), k_taken)
        Return k_reception.Rest - k_taken
    End Function

    ' UI boundary: the operator ticked or cleared a row. A tick needs a reception and a sum, and the reception must still have that much for it.
    Private Sub GridFact_RowChecked(sender As Object, e As KBotRowCheckedEventArgs)
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridFact.RowCount Then Return
            If e.Checked Then
                Dim k_row As KBotDataRow = gridFact.Rows(e.RowIndex)
                Dim k_state As RowState = CType(k_row.Tag, RowState)
                If k_state.Chosen Is Nothing OrElse k_state.Suma = 0D Then
                    gridFact.SetRowChecked(e.RowIndex, False)
                    ShowNotice("Factura nu are o recepție aleasă: alege una în tabelul de jos (dublu clic), apoi bifeaz-o.", NoticeKind.Warning)
                Else
                    Dim k_avail As Decimal = Available(k_state.Chosen, k_row)
                    If Math.Abs(k_state.Suma) > Math.Abs(k_avail) + Tolerance Then
                        gridFact.SetRowChecked(e.RowIndex, False)
                        ShowNotice($"Recepția nu mai are destul rest pentru factura {k_state.Factura.NrFact} (au rămas {Fmt(k_avail)}, vrei să iei {Fmt(k_state.Suma)}): " &
                                   "scrie o sumă mai mică în «Recepție» sau alege altă recepție.", NoticeKind.Warning)
                    Else
                        ntfMesaj.Visible = False
                    End If
                End If
            End If
            FillCandidates()
            RefreshButton()
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridFact_RowChecked", ex)
        End Try
    End Sub

    ' «Angajament» is a list only when the invoice has several (one is just shown); a row that cannot be linked takes no sum.
    Private Sub GridFact_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs)
        Try
            If e.Row Is Nothing Then Return
            Dim k_state As RowState = TryCast(e.Row.Tag, RowState)
            If k_state Is Nothing Then Return
            If String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) AndAlso k_state.Factura.Angajamente.Count <= 1 Then
                e.ReadOnlyCell = True
            ElseIf String.Equals(e.ColumnKey, ColSuma, StringComparison.Ordinal) AndAlso k_state.Factura.Angajamente.Count = 0 Then
                e.ReadOnlyCell = True
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridFact_CellFormatting", ex)
        End Try
    End Sub

    ' «Angajament»: only one of the invoice's own. «Recepție»: a number with the sign of the invoice, not above its value and, once a reception is chosen and the row ticked,
    ' not above what the reception still has for it.
    Private Sub GridFact_CellValidating(sender As Object, e As KBotCellValidatingEventArgs)
        Try
            If e.RowIndex < 0 OrElse e.RowIndex >= gridFact.RowCount Then Return
            Dim k_row As KBotDataRow = gridFact.Rows(e.RowIndex)
            Dim k_state As RowState = TryCast(k_row.Tag, RowState)
            If k_state Is Nothing Then Return
            If String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) Then
                Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture)
                If String.IsNullOrWhiteSpace(k_text) OrElse Not k_state.Factura.Angajamente.Any(Function(k_c) String.Equals(LabelOf(k_c), k_text, StringComparison.Ordinal)) Then
                    e.Cancel = True
                End If
            ElseIf String.Equals(e.ColumnKey, ColSuma, StringComparison.Ordinal) Then
                Dim k_number As Decimal
                If Not ReadNumber(e.ProposedValue, k_number) Then
                    ShowNotice("Suma recepționată nu este un număr.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
                Dim k_total As Decimal = k_state.Factura.Total
                If k_number <> 0D AndAlso (Math.Sign(k_number) <> Math.Sign(k_total) OrElse Math.Abs(k_number) > Math.Abs(k_total) + Tolerance) Then
                    ShowNotice($"Suma recepționată nu poate depăși valoarea facturii ({Fmt(k_total)}) și are semnul facturii.", NoticeKind.Warning)
                    e.Cancel = True
                    Return
                End If
                If k_number <> 0D AndAlso k_state.Chosen IsNot Nothing AndAlso gridFact.GetRowChecked(e.RowIndex) Then
                    Dim k_avail As Decimal = Available(k_state.Chosen, k_row)
                    If Math.Abs(k_number) > Math.Abs(k_avail) + Tolerance Then
                        ShowNotice($"Recepția aleasă mai are pentru această factură doar {Fmt(k_avail)}.", NoticeKind.Warning)
                        e.Cancel = True
                        Return
                    End If
                End If
                e.ProposedValue = CDbl(k_number)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridFact_CellValidating", ex)
            e.Cancel = True
        End Try
    End Sub

    ' UI boundary: a sum was written or an angajament picked. The state is updated here; the cells and the table below are redrawn after the grid's own commit.
    Private Sub GridFact_CellValueChanged(sender As Object, e As KBotCellValueEventArgs)
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= gridFact.RowCount Then Return
            Dim k_row As KBotDataRow = gridFact.Rows(e.RowIndex)
            Dim k_state As RowState = TryCast(k_row.Tag, RowState)
            If k_state Is Nothing Then Return
            If String.Equals(e.ColumnKey, ColSuma, StringComparison.Ordinal) Then
                Dim k_number As Decimal
                If Not ReadNumber(e.NewValue, k_number) Then Return
                k_state.Suma = k_number
                k_state.SumaScrisa = k_number <> 0D
                If k_number = 0D Then DropChoice(k_row, e.RowIndex, k_state)
            ElseIf String.Equals(e.ColumnKey, ColAng, StringComparison.Ordinal) Then
                Dim k_cod As String = Nothing
                If Not _codOf.TryGetValue(Convert.ToString(e.NewValue, CultureInfo.InvariantCulture), k_cod) OrElse SameCod(k_cod, k_state.Cod) Then Return
                k_state.Cod = k_cod
                DropChoice(k_row, e.RowIndex, k_state)
            Else
                Return
            End If
            Dim k_index As Integer = e.RowIndex
            BeginInvoke(New Action(Sub() RedrawAfterEdit(k_index)))
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.GridFact_CellValueChanged", ex)
        End Try
    End Sub

    ' The reception chosen no longer stands (another angajament, or the sum went to 0): the row is cleared and unticked; a sum the accountant did not write goes back to 0.
    Private Sub DropChoice(k_row As KBotDataRow, k_index As Integer, k_state As RowState)
        If k_state.Chosen Is Nothing Then Return
        k_state.Chosen = Nothing
        k_state.ByHand = False
        If Not k_state.SumaScrisa Then k_state.Suma = 0D
        gridFact.SetRowChecked(k_index, False)
    End Sub

    Private Sub RedrawAfterEdit(k_index As Integer)
        Try
            If IsDisposed OrElse k_index < 0 OrElse k_index >= gridFact.RowCount Then Return
            _filling = True
            Try
                ShowRow(gridFact.Rows(k_index))
            Finally
                _filling = False
            End Try
            gridFact.Invalidate()
            ntfMesaj.Visible = False
            FillCandidates()
            RefreshButton()
        Catch ex As Exception
            GlobalErrorLog.Write("KePropuneriForm.RedrawAfterEdit", ex)
        End Try
    End Sub

    ' ── The receptions of the current invoice ───────────────────────────────────

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

    ' The sum looked for in the table below: the one the accountant wrote, else the whole invoice.
    Private Shared Function Wanted(k_state As RowState) As Decimal
        Return Math.Abs(If(k_state.SumaScrisa AndAlso k_state.Suma <> 0D, k_state.Suma, k_state.Factura.Total))
    End Function

    Private Sub FillCandidates()
        Dim k_row As KBotDataRow = gridFact.CurrentRow
        Dim k_state As RowState = TryCast(k_row?.Tag, RowState)
        If k_state IsNot Nothing Then
            gridFact.Column(ColAng).ComboItems = k_state.Factura.Angajamente.Select(Function(k_c) CType(LabelOf(k_c), Object)).ToList()
        End If
        gridCand.BeginUpdate()
        Try
            gridCand.ClearRows()
            If k_state Is Nothing OrElse String.IsNullOrEmpty(k_state.Cod) OrElse _answer Is Nothing Then Return
            Dim k_taken As Dictionary(Of String, Decimal) = TakenMap(k_row)
            Dim k_ticked As Boolean = gridFact.GetRowChecked(gridFact.CurrentRowIndex)
            Dim k_total As Decimal = k_state.Factura.Total
            Dim k_wanted As Decimal = Wanted(k_state)
            Dim k_same As New List(Of EFacturaKePropunereReceptie)()
            Dim k_other As New List(Of EFacturaKePropunereReceptie)()
            Dim k_left As New Dictionary(Of EFacturaKePropunereReceptie, Decimal)()
            For Each k_c As EFacturaKePropunereReceptie In _answer.Receptii.OrderBy(Function(k_x) k_x.DataR).ThenBy(Function(k_x) k_x.Idrr)
                If Not SameCod(k_c.CodAngajament, k_state.Cod) OrElse Math.Sign(k_c.Rest) <> Math.Sign(k_total) Then Continue For
                If k_state.Factura.IdClsfLinii.HasValue AndAlso k_state.Factura.IdClsfLinii.Value <> k_c.IdClsf Then Continue For
                Dim k_others As Decimal = 0D
                k_taken.TryGetValue(KeyOf(k_c), k_others)
                Dim k_avail As Decimal = k_c.Rest - k_others
                ' nothing left for this invoice: the reception is not listed
                If Math.Abs(k_avail) <= Tolerance Then Continue For
                Dim k_own As Decimal = If(k_ticked AndAlso k_state.Chosen IsNot Nothing AndAlso SameReception(k_state.Chosen, k_c), k_state.Suma, 0D)
                k_left(k_c) = k_avail - k_own
                If Math.Abs(Math.Abs(k_avail) - k_wanted) <= Tolerance Then k_same.Add(k_c) Else k_other.Add(k_c)
            Next
            AddCandidates(k_same, GrupAceeasi, k_state, k_left)
            AddCandidates(k_other, GrupAlte, k_state, k_left)
        Finally
            gridCand.EndUpdate()
        End Try
    End Sub

    Private Sub AddCandidates(k_list As List(Of EFacturaKePropunereReceptie), k_group As String, k_state As RowState, k_left As Dictionary(Of EFacturaKePropunereReceptie, Decimal))
        For Each k_c As EFacturaKePropunereReceptie In k_list
            Dim k_row As KBotDataRow = gridCand.AddRow()
            k_row.Tag = k_c
            k_row("grup") = k_group
            k_row("dataR") = DayText(k_c.DataR)
            k_row("descriere") = k_c.Descriere
            k_row("cod") = k_c.CodAngajament
            k_row("clsf") = k_c.Clsf
            k_row("zile") = If(k_c.DataR.HasValue AndAlso k_state.Factura.DataFact.HasValue,
                               CInt((k_c.DataR.Value.Date - k_state.Factura.DataFact.Value.Date).TotalDays).ToString(CultureInfo.InvariantCulture), String.Empty)
            k_row("ramas") = CDbl(k_left(k_c))
            k_row("aleasa") = If(k_state.Chosen IsNot Nothing AndAlso SameReception(k_state.Chosen, k_c), "da", String.Empty)
        Next
    End Sub

    ' UI boundary: a double click on a reception gives it to the current invoice, with the sum looked for (cut to what the reception still has); the row is ticked too.
    Private Sub GridCand_CellDoubleClick(sender As Object, e As KBotCellEventArgs)
        Try
            Dim k_state As RowState = CurrentState()
            Dim k_candidate As EFacturaKePropunereReceptie = TryCast(gridCand.CurrentRow?.Tag, EFacturaKePropunereReceptie)
            If k_state Is Nothing OrElse k_candidate Is Nothing OrElse gridFact.CurrentRow Is Nothing Then Return
            Dim k_row As KBotDataRow = gridFact.CurrentRow
            Dim k_index As Integer = gridFact.CurrentRowIndex
            Dim k_avail As Decimal = Available(k_candidate, k_row)
            If Math.Abs(k_avail) <= Tolerance Then
                ShowNotice("Recepția nu mai are rest pentru această factură.", NoticeKind.Warning)
                FillCandidates()
                Return
            End If
            Dim k_wanted As Decimal = Wanted(k_state)
            Dim k_take As Decimal = Math.Sign(k_state.Factura.Total) * Math.Min(k_wanted, Math.Abs(k_avail))
            k_state.Chosen = k_candidate
            k_state.Cod = k_candidate.CodAngajament
            k_state.Suma = k_take
            k_state.ByHand = Not (k_state.Factura.Propunere IsNot Nothing AndAlso k_state.Factura.Propunere.Idrr = k_candidate.Idrr AndAlso
                                  k_state.Factura.Propunere.IdClsf = k_candidate.IdClsf AndAlso SameCod(k_state.Factura.Propunere.CodAngajament, k_candidate.CodAngajament))
            _filling = True
            Try
                ShowRow(k_row)
                gridFact.SetRowChecked(k_index, True)
            Finally
                _filling = False
            End Try
            gridFact.Invalidate()
            If Math.Abs(k_take) < k_wanted - Tolerance Then
                ShowNotice($"Recepția are doar {Fmt(Math.Abs(k_avail))} rest: din factură se recepționează atât (poți scrie altă sumă în «Recepție»).", NoticeKind.Warning)
            Else
                ntfMesaj.Visible = False
            End If
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
            Dim k_used As New Dictionary(Of String, Decimal)(StringComparer.Ordinal)
            For Each k_row As KBotDataRow In Ticked()
                Dim k_state As RowState = CType(k_row.Tag, RowState)
                If k_state.Chosen Is Nothing OrElse k_state.Suma = 0D Then Continue For
                Dim k_key As String = KeyOf(k_state.Chosen)
                Dim k_sum As Decimal = 0D
                k_used.TryGetValue(k_key, k_sum)
                k_sum += k_state.Suma
                k_used(k_key) = k_sum
                If Math.Abs(k_sum) > Math.Abs(k_state.Chosen.Rest) + Tolerance Then
                    ShowNotice($"Recepția «{k_state.Chosen.Descriere}» nu are destul rest pentru toate facturile bifate (are {Fmt(k_state.Chosen.Rest)}).", NoticeKind.Warning)
                    Return
                End If
                k_links.Add(New EFacturaKeLegaturaMasa() With {
                    .IdPrimita = k_state.Factura.IdPrimita, .CodAngajament = k_state.Chosen.CodAngajament, .Idrr = k_state.Chosen.Idrr,
                    .IdClsf = k_state.Chosen.IdClsf, .Suma = k_state.Suma})
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
