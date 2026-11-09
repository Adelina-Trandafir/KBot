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
''' Slice AV02-04 -- the «Facturi si plati» page (the Access form OPuri2017): for ONE partner, the invoices typed by hand (evidence with the chosen template), the
''' unpaid invoices already in the books (current year, and the previous year when the database holds it), and the payment of any of them with its order (OP). It does
''' the same job as the page «Contari» of the received e-invoices, over invoices that do not come from the e-invoice list; the server uses the same services.
''' The number of the order shown before the save is PROVISIONAL (tooltip on the label); the save answers the real one and the message says it.
''' </summary>
Public Class FacturiPlatiPage
    Implements IThemedControl, ISablonPagina

    Private Shared ReadOnly _kinds As New List(Of Object) From {"Factură", "Notă credit"}
    Private Const DateFormat As String = "dd.MM.yyyy"

    Private _api As IOperatiiApi
    Private _gate As ReauthGate
    Private _session As SessionContext
    Private _idSubunitate As Integer
    Private _bound As Boolean
    Private _loaded As Boolean
    Private _busy As Boolean
    Private _filling As Boolean
    Private _dirty As Boolean
    Private _explanationTyped As Boolean
    Private _context As FacturiPlatiContext
    Private _partner As FacturiPlatiPartener
    Private _classification As FacturiPlatiClasificatie
    Private _partnerData As FacturiPartener
    Private _idSablonE As Integer
    Private _idSablonP As Integer

    Public Sub New()
        InitializeComponent()
        cmbPartener.CaptionSelector = Function(k_o) CaptionOfItem(k_o)
        cmbClsf.CaptionSelector = Function(k_o) CaptionOfItem(k_o)
        gridNoi.ReadOnlyGrid = False
        gridCurent.ReadOnlyGrid = False
        gridAnterior.ReadOnlyGrid = False
    End Sub

    ''' <summary>Hands the page what it needs and reads the notes, the partners and the classifications. UI boundary: a failure is shown in the status line.</summary>
    Public Async Sub Bind(k_api As IOperatiiApi, k_gate As ReauthGate, k_session As SessionContext, k_idSubunitate As Integer?)
        Try
            ArgumentNullException.ThrowIfNull(k_api)
            ArgumentNullException.ThrowIfNull(k_gate)
            ArgumentNullException.ThrowIfNull(k_session)
            _api = k_api
            _gate = k_gate
            _session = k_session
            If Not k_idSubunitate.HasValue Then
                SetStatus("Alegeți o subunitate în bara de titlu: facturile și plățile aparțin unei subunități.")
                Return
            End If
            _idSubunitate = k_idSubunitate.Value
            _bound = True
            Await LoadAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.Bind", ex)
        End Try
    End Sub

    Private Async Function LoadAsync() As Task(Of Boolean)
        Try
            SetBusy(True)
            SetStatus("Se încarcă…")
            Dim k_context As FacturiPlatiContext = Await _gate.RunAsync(
                Function() _api.GetFacturiPlatiContextAsync(_idSubunitate, _session.An, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return False
            _context = k_context
            _filling = True
            Try
                cmbPartener.Items.Clear()
                For Each k_p As FacturiPlatiPartener In k_context.Parteneri
                    cmbPartener.Items.Add(CType(k_p, Object))
                Next
                cmbClsf.Items.Clear()
                For Each k_c As FacturiPlatiClasificatie In k_context.Clasificatii
                    cmbClsf.Items.Add(CType(k_c, Object))
                Next
                FillNotes()
                dtpDataOp.Value = If(Date.Today.Year = YearOfWork(), Date.Today, New Date(YearOfWork(), 1, 1))
            Finally
                _filling = False
            End Try
            _loaded = True
            cmbPartener.Enabled = True
            cmbClsf.Enabled = True
            cmbNota.Enabled = True
            SetStatus(If(k_context.Clasificatii.Count = 0, "Subunitatea nu are clasificații cu șabloane de operații. Adăugați șabloane în «Șabloane».", String.Empty))
            RefreshState()
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiPlatiPage.LoadAsync", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.LoadAsync", ex)
            If Not IsDisposed Then SetStatus("Datele nu au putut fi citite. Detalii în jurnalul de erori.")
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    Private Function YearOfWork() As Integer
        Return If(_session Is Nothing OrElse _session.An = 0, Date.Today.Year, _session.An)
    End Function

    Private Shared Function CaptionOfItem(k_item As Object) As String
        Dim k_p As FacturiPlatiPartener = TryCast(k_item, FacturiPlatiPartener)
        If k_p IsNot Nothing Then Return If(k_p.Cui.Length > 0, $"{k_p.Denumire} ({k_p.Cui})", k_p.Denumire)
        Dim k_c As FacturiPlatiClasificatie = TryCast(k_item, FacturiPlatiClasificatie)
        If k_c IsNot Nothing Then Return $"{k_c.Clsf}  {k_c.Denumire}"
        Return Convert.ToString(k_item, CultureInfo.InvariantCulture)
    End Function

    Private Sub FillNotes()
        cmbNota.Items.Clear()
        For Each k_n As OperatiiNotaInfo In _context.Baza.Note
            cmbNota.Items.Add(CType(k_n.NumarNC.ToString(CultureInfo.InvariantCulture), Object))
        Next
        cmbNota.Text = If(_context.Baza.Urmatoare > 0, _context.Baza.Urmatoare.ToString(CultureInfo.InvariantCulture), String.Empty)
    End Sub

    ' ── The partner and the classification ──────────────────────────────────────

    Private Async Sub CmbPartener_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbPartener.SelectedIndexChanged
        Try
            If _filling OrElse Not _loaded Then Return
            Dim k_partner As FacturiPlatiPartener = TryCast(cmbPartener.SelectedItem, FacturiPlatiPartener)
            If k_partner Is Nothing OrElse (_partner IsNot Nothing AndAlso k_partner.IdPartener = _partner.IdPartener) Then Return
            If _dirty AndAlso KBotMessage.Show(Me, "Ce ați completat pentru partenerul curent nu este salvat." & vbLf & "Renunțați la el și treceți la alt partener?", "Facturi și plăți",
                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                _filling = True
                Try
                    cmbPartener.SelectedItem = CType(_partner, Object)
                Finally
                    _filling = False
                End Try
                Return
            End If
            _partner = k_partner
            ClearEdits()
            Await LoadPartnerAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.CmbPartener_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Async Sub CmbClsf_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbClsf.SelectedIndexChanged
        Try
            If _filling OrElse Not _loaded Then Return
            Dim k_clsf As FacturiPlatiClasificatie = TryCast(cmbClsf.SelectedItem, FacturiPlatiClasificatie)
            If k_clsf Is Nothing Then Return
            _classification = k_clsf
            _idSablonE = 0
            _idSablonP = 0
            FillTemplates()
            If _partner IsNot Nothing Then Await LoadPartnerAsync().ConfigureAwait(True)
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.CmbClsf_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The unpaid invoices, the orders of the year, the account and the provisional order number of the partner (with the account of the classification when one is chosen).
    Private Async Function LoadPartnerAsync() As Task
        If _partner Is Nothing Then Return
        Try
            SetBusy(True)
            Dim k_data As FacturiPartener = Await _gate.RunAsync(
                Function() _api.GetFacturiPartenerAsync(_idSubunitate, YearOfWork(), _partner.IdPartener, If(_classification Is Nothing, 0, _classification.IdClsf), CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            _partnerData = k_data
            _filling = True
            Try
                FillOpen(gridCurent, k_data.Curent)
                FillOpen(gridAnterior, k_data.Anterior)
                FillOrders(k_data.Plati)
                txtIban.Text = If(k_data.ContIban.Length > 0, k_data.ContIban, _partner.Iban)
                gridCurent.Enabled = True
                gridAnterior.Enabled = True
                gridPlati.Enabled = True
                gridNoi.Enabled = True
            Finally
                _filling = False
            End Try
            SetStatus(String.Empty)
            RefreshState()
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiPlatiPage.LoadPartnerAsync", ex)
            SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.LoadPartnerAsync", ex)
            SetStatus("Facturile partenerului nu au putut fi citite. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    Private Shared Sub FillOpen(k_grid As KBotDataView, k_invoices As IReadOnlyList(Of FacturaDeschisa))
        k_grid.BeginUpdate()
        Try
            k_grid.ClearRows()
            For Each k_f As FacturaDeschisa In k_invoices
                Dim k_row As KBotDataRow = k_grid.AddRow()
                k_row.Tag = k_f
                k_row("numar") = k_f.Numar
                k_row("data") = If(k_f.Data.HasValue, k_f.Data.Value.ToString(DateFormat, CultureInfo.InvariantCulture), String.Empty)
                k_row("fel") = k_f.Fel
                k_row("valoare") = k_f.Valoare
                k_row("rest") = k_f.Rest
                k_row("platit") = 0D
            Next
        Finally
            k_grid.EndUpdate()
        End Try
    End Sub

    Private Sub FillOrders(k_orders As IReadOnlyList(Of PlataOrdin))
        gridPlati.BeginUpdate()
        Try
            gridPlati.ClearRows()
            For Each k_o As PlataOrdin In k_orders
                Dim k_row As KBotDataRow = gridPlati.AddRow()
                k_row.Tag = k_o
                k_row("nrop") = k_o.NrOP.ToString(CultureInfo.InvariantCulture)
                k_row("data") = If(k_o.DataOP.HasValue, k_o.DataOP.Value.ToString(DateFormat, CultureInfo.InvariantCulture), String.Empty)
                k_row("documente") = k_o.Documente
                k_row("explicatie") = k_o.Explicatie
                k_row("valoare") = k_o.Valoare
                k_row("stare") = If(k_o.Blocat, "Blocat", "Deschis")
            Next
        Finally
            gridPlati.EndUpdate()
        End Try
    End Sub

    ' Gives up everything typed for the previous partner.
    Private Sub ClearEdits()
        _filling = True
        Try
            gridNoi.ClearRows()
            _dirty = False
            _explanationTyped = False
            txtExplicatie.Text = String.Empty
        Finally
            _filling = False
        End Try
    End Sub

    ' ── The templates ───────────────────────────────────────────────────────────

    Private Sub FillTemplates()
        FillTemplateList(lstEvidentiere, If(_classification Is Nothing, Nothing, _classification.SabloaneE), _idSablonE)
        FillTemplateList(lstPlata, If(_classification Is Nothing, Nothing, _classification.SabloaneP), _idSablonP)
    End Sub

    Private Shared Sub FillTemplateList(k_list As KBotDataView, k_templates As IReadOnlyList(Of FacturiPlatiSablon), k_chosen As Integer)
        k_list.BeginUpdate()
        Try
            k_list.ClearRows()
            Dim k_chosenRow As Integer = -1
            If k_templates IsNot Nothing Then
                For Each k_s As FacturiPlatiSablon In k_templates
                    Dim k_row As KBotDataRow = k_list.AddRow()
                    k_row.Tag = k_s
                    k_row("nume") = k_s.Explicatie
                    If k_s.IdSablon = k_chosen Then k_chosenRow = k_list.Rows.Count - 1
                Next
            End If
            k_list.CurrentRowIndex = k_chosenRow
        Finally
            k_list.EndUpdate()
        End Try
        k_list.Enabled = k_templates IsNot Nothing AndAlso k_templates.Count > 0
    End Sub

    ' A click on a template chooses it; a click on the chosen one takes it off (no evidence template = no new invoices; none for payment = no payment).
    Private Sub LstEvidentiere_CellClick(sender As Object, e As KBotCellEventArgs) Handles lstEvidentiere.CellClick
        Try
            Dim k_template As FacturiPlatiSablon = TemplateAt(lstEvidentiere, e.RowIndex)
            If k_template Is Nothing Then Return
            _idSablonE = If(_idSablonE = k_template.IdSablon, 0, k_template.IdSablon)
            FillTemplates()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.LstEvidentiere_CellClick", ex)
        End Try
    End Sub

    Private Sub LstPlata_CellClick(sender As Object, e As KBotCellEventArgs) Handles lstPlata.CellClick
        Try
            Dim k_template As FacturiPlatiSablon = TemplateAt(lstPlata, e.RowIndex)
            If k_template Is Nothing Then Return
            _idSablonP = If(_idSablonP = k_template.IdSablon, 0, k_template.IdSablon)
            FillTemplates()
            Recompose()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.LstPlata_CellClick", ex)
        End Try
    End Sub

    Private Function TemplateAt(k_list As KBotDataView, k_index As Integer) As FacturiPlatiSablon
        If _filling OrElse _busy OrElse k_index < 0 OrElse k_index >= k_list.Rows.Count Then Return Nothing
        Return TryCast(k_list.Rows(k_index).Tag, FacturiPlatiSablon)
    End Function

    ' ── Typed invoices ──────────────────────────────────────────────────────────

    Private Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _partner Is Nothing Then Return
            Dim k_row As KBotDataRow = gridNoi.AddRow()
            k_row("numar") = String.Empty
            k_row("data") = Date.Today.ToString(DateFormat, CultureInfo.InvariantCulture)
            k_row("fel") = "Factură"
            k_row("suma") = 0D
            k_row("platit") = 0D
            _dirty = True
            gridNoi.CurrentRowIndex = gridNoi.Rows.Count - 1
            gridNoi.Invalidate()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.BtnAdauga_Click", ex)
        End Try
    End Sub

    Private Sub BtnStergeRand_Click(sender As Object, e As EventArgs) Handles btnStergeRand.Click
        Try
            Dim k_index As Integer = gridNoi.CurrentRowIndex
            If k_index < 0 OrElse k_index >= gridNoi.Rows.Count Then Return
            gridNoi.RemoveRowAt(k_index)
            Recompose()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.BtnStergeRand_Click", ex)
        End Try
    End Sub

    Private Sub GridNoi_ComboItemsRequested(sender As Object, e As KBotComboItemsEventArgs) Handles gridNoi.ComboItemsRequested
        e.Items = _kinds
    End Sub

    ' ── Amounts to pay ──────────────────────────────────────────────────────────

    ' A payment never goes over the rest, and keeps the sign of the rest (a credit note is negative).
    Private Sub GridOpen_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridCurent.CellValidating, gridAnterior.CellValidating
        Try
            If Not String.Equals(e.ColumnKey, "platit", StringComparison.Ordinal) Then Return
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            Dim k_invoice As FacturaDeschisa = If(e.RowIndex >= 0 AndAlso e.RowIndex < k_grid.Rows.Count, TryCast(k_grid.Rows(e.RowIndex).Tag, FacturaDeschisa), Nothing)
            If k_invoice Is Nothing Then Return
            Dim k_value As Decimal = ParseAmount(e.ProposedValue)
            e.ProposedValue = If(k_invoice.Rest >= 0D, Math.Min(Math.Max(k_value, 0D), k_invoice.Rest), Math.Max(Math.Min(k_value, 0D), k_invoice.Rest))
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridOpen_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridNoi_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridNoi.CellValidating
        Try
            If Not String.Equals(e.ColumnKey, "platit", StringComparison.Ordinal) Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridNoi.Rows.Count Then Return
            Dim k_total As Decimal = ParseAmount(gridNoi.Rows(e.RowIndex)("suma"))
            Dim k_value As Decimal = ParseAmount(e.ProposedValue)
            e.ProposedValue = If(k_total >= 0D, Math.Min(Math.Max(k_value, 0D), k_total), Math.Max(Math.Min(k_value, 0D), k_total))
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridNoi_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridNoi.CellValueChanged, gridCurent.CellValueChanged, gridAnterior.CellValueChanged
        Try
            If _filling Then Return
            ' A typed invoice: when the value changes the amount to pay follows it down to the new value if it was above it.
            If sender Is gridNoi AndAlso String.Equals(e.ColumnKey, "suma", StringComparison.Ordinal) Then
                Dim k_total As Decimal = ParseAmount(e.NewValue)
                Dim k_paid As Decimal = ParseAmount(gridNoi.Rows(e.RowIndex)("platit"))
                If (k_total >= 0D AndAlso k_paid > k_total) OrElse (k_total < 0D AndAlso k_paid < k_total) Then gridNoi.Rows(e.RowIndex)("platit") = k_total
            End If
            _dirty = True
            Recompose()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.Grid_CellValueChanged", ex)
        End Try
    End Sub

    Private Shared Function ParseAmount(k_value As Object) As Decimal
        If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then Return 0D
        If TypeOf k_value Is Decimal Then Return CDec(k_value)
        Dim k_text As String = Convert.ToString(k_value, CultureInfo.InvariantCulture).Trim().Replace(" "c, String.Empty)
        Dim k_result As Decimal
        If Decimal.TryParse(k_text, NumberStyles.Number, CultureInfo.InvariantCulture, k_result) Then Return k_result
        If Decimal.TryParse(k_text.Replace(","c, "."c), NumberStyles.Number, CultureInfo.InvariantCulture, k_result) Then Return k_result
        If Decimal.TryParse(k_text, NumberStyles.Number, New CultureInfo("ro-RO"), k_result) Then Return k_result
        Return 0D
    End Function

    ' The sum of everything to pay now (typed invoices + the two open grids).
    Private Function TotalToPay() As Decimal
        Dim k_total As Decimal = 0D
        For Each k_grid As KBotDataView In {gridNoi, gridCurent, gridAnterior}
            For Each k_row As KBotDataRow In k_grid.Rows
                k_total += ParseAmount(k_row("platit"))
            Next
        Next
        Return k_total
    End Function

    ' The numbers of the documents that are paid now: the explanation of the order is composed from them and from the kind written on the payment template.
    Private Function PaidNumbers() As List(Of String)
        Dim k_numbers As New List(Of String)()
        For Each k_grid As KBotDataView In {gridNoi, gridCurent, gridAnterior}
            For Each k_row As KBotDataRow In k_grid.Rows
                If ParseAmount(k_row("platit")) <> 0D Then
                    Dim k_number As String = Convert.ToString(k_row("numar"), CultureInfo.InvariantCulture).Trim()
                    If k_number.Length > 0 Then k_numbers.Add(k_number)
                End If
            Next
        Next
        Return k_numbers
    End Function

    Private Sub Recompose()
        If _explanationTyped OrElse _idSablonP = 0 Then Return
        Dim k_template As FacturiPlatiSablon = If(_classification Is Nothing, Nothing, _classification.SabloaneP.FirstOrDefault(Function(k_s) k_s.IdSablon = _idSablonP))
        Dim k_cut As Boolean
        _filling = True
        Try
            txtExplicatie.Text = OpExplicatie.Compose(PaidNumbers(), String.Empty, If(k_template Is Nothing, String.Empty, OpExplicatie.Fit(k_template.FelOP)), k_cut)
        Finally
            _filling = False
        End Try
    End Sub

    ' What the operator types in the explanation is made valid (ASCII, no comma, at most 75 characters) and from then on is no longer composed.
    Private Sub TxtExplicatie_TextChanged(sender As Object, e As EventArgs) Handles txtExplicatie.TextChanged
        Try
            If _filling Then Return
            Dim k_clean As String = OpExplicatie.Clean(txtExplicatie.Text)
            If k_clean.Length > OpExplicatie.MaxLength Then k_clean = k_clean.Substring(0, OpExplicatie.MaxLength)
            If Not String.Equals(k_clean, txtExplicatie.Text, StringComparison.Ordinal) Then
                _filling = True
                Try
                    txtExplicatie.Text = k_clean
                Finally
                    _filling = False
                End Try
            End If
            _explanationTyped = k_clean.Length > 0
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.TxtExplicatie_TextChanged", ex)
        End Try
    End Sub

    ' ── Delete a payment ────────────────────────────────────────────────────────

    Private Sub GridPlati_CellClick(sender As Object, e As KBotCellEventArgs) Handles gridPlati.CellClick
        Try
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridPlati_CellClick", ex)
        End Try
    End Sub

    Private Function SelectedOrder() As PlataOrdin
        Dim k_index As Integer = gridPlati.CurrentRowIndex
        If k_index < 0 OrElse k_index >= gridPlati.Rows.Count Then Return Nothing
        Return TryCast(gridPlati.Rows(k_index).Tag, PlataOrdin)
    End Function

    Private Async Sub BtnStergePlata_Click(sender As Object, e As EventArgs) Handles btnStergePlata.Click
        Try
            Dim k_order As PlataOrdin = SelectedOrder()
            If k_order Is Nothing OrElse k_order.Blocat OrElse _busy Then Return
            If KBotMessage.Show(Me, $"Ștergeți plata cu ordinul de plată {k_order.NrOP.ToString(CultureInfo.InvariantCulture)}, împreună cu ordinul?" & vbLf & "Operația nu poate fi anulată.",
                                "Facturi și plăți", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            SetBusy(True)
            Await _gate.RunAsync(Function() _api.StergePlataFacturiAsync(k_order.IdOP, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            KBotMessage.Show(Me, $"Plata cu ordinul {k_order.NrOP.ToString(CultureInfo.InvariantCulture)} a fost ștearsă.", "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Information)
            SetBusy(False)
            Await LoadPartnerAsync().ConfigureAwait(True)
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiPlatiPage.BtnStergePlata_Click", ex)
            SetBusy(False)
            SetStatus(ex.Message)
            KBotMessage.Show(Me, ex.Message, "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.BtnStergePlata_Click", ex)
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Sub

    ' ── Save and give up ────────────────────────────────────────────────────────

    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.BtnSalveaza_Click", ex)
        End Try
    End Sub

    Private Async Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            If _partner Is Nothing Then Return
            If _dirty AndAlso KBotMessage.Show(Me, "Renunțați la ce ați completat?", "Facturi și plăți", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            ClearEdits()
            Await LoadPartnerAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.BtnRenunta_Click", ex)
        End Try
    End Sub

    Private Shared Function ParseDate(k_text As Object, ByRef k_date As Date) As Boolean
        Return Date.TryParseExact(Convert.ToString(k_text, CultureInfo.InvariantCulture).Trim(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, k_date)
    End Function

    ' Checks what the server also checks, so the operator is told at once; the server stays the judge.
    Private Function ValidateRequest(ByRef k_request As FacturiPlatiCerere) As String
        If _partner Is Nothing Then Return "Alegeți partenerul."
        If _classification Is Nothing Then Return "Alegeți clasificația bugetară."
        Dim k_note As Integer
        If Not Integer.TryParse(cmbNota.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_note) OrElse k_note <= 0 Then Return "Nota contabilă trebuie să fie un număr întreg pozitiv."
        Dim k_paying As Boolean = _idSablonP > 0
        Dim k_new As New List(Of FacturaNoua)()
        For Each k_row As KBotDataRow In gridNoi.Rows
            Dim k_number As String = Convert.ToString(k_row("numar"), CultureInfo.InvariantCulture).Trim()
            If k_number.Length = 0 Then Return "O factură nouă nu are număr."
            Dim k_date As Date
            If Not ParseDate(k_row("data"), k_date) Then Return $"Data facturii {k_number} nu este validă (zz.ll.aaaa)."
            Dim k_amount As Decimal = ParseAmount(k_row("suma"))
            If k_amount = 0D Then Return $"Factura {k_number} are valoarea zero."
            k_new.Add(New FacturaNoua() With {.Numar = k_number, .Data = k_date, .Suma = k_amount, .Fel = Convert.ToString(k_row("fel"), CultureInfo.InvariantCulture), .Platit = ParseAmount(k_row("platit"))})
        Next
        If k_new.Count > 0 AndAlso _idSablonE = 0 Then Return "Alegeți șablonul de evidențiere pentru facturile noi."
        Dim k_chosen As New List(Of FacturaAleasa)()
        For Each k_grid As KBotDataView In {gridCurent, gridAnterior}
            For Each k_row As KBotDataRow In k_grid.Rows
                Dim k_invoice As FacturaDeschisa = TryCast(k_row.Tag, FacturaDeschisa)
                Dim k_paid As Decimal = ParseAmount(k_row("platit"))
                If k_invoice IsNot Nothing AndAlso k_paid <> 0D Then k_chosen.Add(New FacturaAleasa() With {.IdOper = k_invoice.IdOper, .Platit = k_paid})
            Next
        Next
        Dim k_total As Decimal = k_new.Sum(Function(k_f) k_f.Platit) + k_chosen.Sum(Function(k_f) k_f.Platit)
        If k_new.Count = 0 AndAlso k_total = 0D Then Return "Nu este nimic de înregistrat: adăugați o factură nouă sau scrieți o sumă de plată."
        If k_total <> 0D AndAlso Not k_paying Then Return "Alegeți șablonul de plată: există sume de plată."
        If k_paying AndAlso k_total <= 0D Then Return "Suma de plată trebuie să fie pozitivă."
        If k_paying AndAlso txtIban.Text.Trim().Length = 0 Then Return "Partenerul nu are niciun cont bancar configurat: nu se poate face ordinul de plată."
        Dim k_dateOp As Date = dtpDataOp.Value.Date
        If k_paying AndAlso k_dateOp.Year <> YearOfWork() Then Return $"Data ordinului de plată trebuie să fie în anul de lucru ({YearOfWork()})."
        k_request = New FacturiPlatiCerere() With {
            .IdSubunitate = _idSubunitate, .An = YearOfWork(), .IdPartener = _partner.IdPartener, .IdClsf = _classification.IdClsf,
            .IdSablonE = _idSablonE, .IdSablonP = If(k_paying, _idSablonP, 0), .NumarNC = k_note, .DataOP = k_dateOp,
            .ExplicatieOP = txtExplicatie.Text.Trim(), .Noi = k_new, .Existente = k_chosen}
        Return String.Empty
    End Function

    Private Async Function SaveAsync() As Task(Of Boolean)
        If Not _bound OrElse _busy Then Return False
        Dim k_request As FacturiPlatiCerere = Nothing
        Dim k_problem As String = ValidateRequest(k_request)
        If k_problem.Length > 0 Then
            SetStatus(k_problem)
            KBotMessage.Show(Me, k_problem, "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        Dim k_shown As Integer = If(_partnerData Is Nothing, 0, _partnerData.NumarOP)
        Try
            SetBusy(True)
            Dim k_result As FacturiPlatiRezultat = Await _gate.RunAsync(
                Function() _api.SalveazaFacturiPlatiAsync(k_request, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return True
            _dirty = False
            Dim k_text As String = $"Nota contabilă {k_result.NumarNC.ToString(CultureInfo.InvariantCulture)}: {k_result.Facturi.ToString(CultureInfo.InvariantCulture)} facturi noi înregistrate"
            If k_result.NrOP > 0 Then
                k_text &= $", plată de {k_result.ValoarePlata.ToString("N2", New CultureInfo("ro-RO"))} cu ordinul de plată numărul {k_result.NrOP.ToString(CultureInfo.InvariantCulture)}."
                If k_shown > 0 AndAlso k_shown <> k_result.NrOP Then
                    k_text &= vbLf & $"Numărul afișat înainte ({k_shown.ToString(CultureInfo.InvariantCulture)}) era provizoriu; numărul real al ordinului este {k_result.NrOP.ToString(CultureInfo.InvariantCulture)}."
                End If
            Else
                k_text &= "."
            End If
            KBotMessage.Show(Me, k_text, "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ClearEdits()
            SetBusy(False)
            Await RefreshNotesAsync().ConfigureAwait(True)
            Await LoadPartnerAsync().ConfigureAwait(True)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiPlatiPage.SaveAsync", ex)
            SetStatus(ex.Message)
            KBotMessage.Show(Me, ex.Message, "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.SaveAsync", ex)
            SetStatus("Datele nu au putut fi salvate. Detalii în jurnalul de erori.")
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False)
        End Try
    End Function

    ' After a save the notes (and the next number) are read again.
    Private Async Function RefreshNotesAsync() As Task
        Dim k_context As FacturiPlatiContext = Await _gate.RunAsync(
            Function() _api.GetFacturiPlatiContextAsync(_idSubunitate, YearOfWork(), CancellationToken.None)).ConfigureAwait(True)
        _context = k_context
        _filling = True
        Try
            FillNotes()
        Finally
            _filling = False
        End Try
    End Function

    ' ── The window's questions (ISablonPagina) ──────────────────────────────────

    Public ReadOnly Property HasPending As Boolean Implements ISablonPagina.HasPending
        Get
            Return _partner IsNot Nothing AndAlso _dirty AndAlso (gridNoi.Rows.Count > 0 OrElse TotalToPay() <> 0D)
        End Get
    End Property

    Public Async Function CommitAsync() As Task(Of Boolean)  Implements ISablonPagina.CommitAsync
        Try
            If Not HasPending Then Return True
            Dim k_answer As DialogResult = KBotMessage.Show(Me, "Ce ați completat nu este salvat." & vbLf & "Salvați?", "Facturi și plăți",
                                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If k_answer = DialogResult.Cancel Then Return False
            If k_answer = DialogResult.Yes Then Return Await SaveAsync().ConfigureAwait(True)
            _dirty = False
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.CommitAsync", ex)
            Return False
        End Try
    End Function

    ' ── State of the controls ───────────────────────────────────────────────────

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        RefreshState()
    End Sub

    Private Sub RefreshState()
        If Not _loaded Then Return
        Dim k_paying As Boolean = _idSablonP > 0
        Dim k_total As Decimal = TotalToPay()
        Dim k_active As Boolean = Not _busy AndAlso _partner IsNot Nothing
        btnAdauga.Enabled = k_active
        btnStergeRand.Enabled = k_active AndAlso gridNoi.Rows.Count > 0
        btnSalveaza.Enabled = k_active AndAlso _classification IsNot Nothing AndAlso (gridNoi.Rows.Count > 0 OrElse k_total <> 0D)
        btnRenunta.Enabled = k_active
        Dim k_order As PlataOrdin = SelectedOrder()
        btnStergePlata.Enabled = k_active AndAlso k_order IsNot Nothing AndAlso Not k_order.Blocat
        tips.SetToolTipText(btnStergePlata, If(k_order IsNot Nothing AndAlso k_order.Blocat, k_order.Motiv & " Plata și ordinul nu se mai pot șterge.",
                                               "Șterge plata aleasă în tabelul ordinelor, împreună cu ordinul ei. Nu merge dacă ordinul e într-un OPME sau confirmat prin extras."))
        txtExplicatie.Enabled = k_active AndAlso k_paying
        If _partnerData IsNot Nothing AndAlso k_paying AndAlso k_total <> 0D Then
            lblNumarOp.Text = $"Număr O.P.: {_partnerData.NumarOP.ToString(CultureInfo.InvariantCulture)} (provizoriu)"
        Else
            lblNumarOp.Text = String.Empty
        End If
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.PanelBack()
            For Each k_c As Control In {tlyPagina, tlyAntet, tlyMijloc, tlySabloane, tlyGrile, tlyJos}
                k_c.BackColor = k_scheme.PanelBack()
            Next
            lblStare.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
