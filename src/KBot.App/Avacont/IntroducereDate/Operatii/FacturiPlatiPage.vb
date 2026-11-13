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
''' unpaid invoices already in the books (current year, and the previous year when the database holds it), and the payment of any of them with its order (OP).
''' Layout (operator's sketch, 10.11.2026): classification | partner | client code | bank account on top; the evidence templates on the left and ONE view on the
''' right (a nav list switches between three stand-alone grids: new invoices, current year, previous year); the payment templates on the left of the payment
''' grid, whose first row is the payment being made and whose other rows are the orders of the partner in the year. Stand-alone: nothing is shared with the
''' E-Factura pages. The grid of new invoices always ends with a blank row (the place to add): the blank row is never saved.
''' The number of the order shown before the save is PROVISIONAL (tooltip on the column); the save answers the real one and the message says it.
''' </summary>
Public Class FacturiPlatiPage
    Implements IThemedControl, ISablonPagina

    Private Shared ReadOnly _kinds As New List(Of Object) From {"Factură", "Notă credit"}
    Private Const DateFormat As String = "dd.MM.yyyy"
    Private Const ViewNoi As String = "noi"
    Private Const ViewCurent As String = "curent"
    Private Const ViewAnterior As String = "anterior"
    Private Const ColDelete As String = "sterge"

    ' The numbers of the notes of the year (the choices of the «Nota contabila» cell of the payment row).
    Private ReadOnly _noteItems As New List(Of Object)()

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
        gridPlata.ReadOnlyGrid = False
        AddBlankInvoiceRow()
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
            Finally
                _filling = False
            End Try
            _loaded = True
            cmbPartener.Enabled = True
            cmbClsf.Enabled = True
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
        _noteItems.Clear()
        For Each k_n As OperatiiNotaInfo In _context.Baza.Note
            _noteItems.Add(CType(k_n.NumarNC.ToString(CultureInfo.InvariantCulture), Object))
        Next
        ' The spare row of the new invoices starts with the next number of the year.
        For Each k_row As KBotDataRow In gridNoi.Rows
            If IsBlankInvoice(k_row) Then k_row("nota") = DefaultNote()
        Next
        gridNoi.Invalidate()
    End Sub

    ' The note a new payment starts with: the next number of the year.
    Private Function DefaultNote() As String
        Return If(_context IsNot Nothing AndAlso _context.Baza.Urmatoare > 0, _context.Baza.Urmatoare.ToString(CultureInfo.InvariantCulture), String.Empty)
    End Function

    ' The date a new payment starts with: today when it is in the year of work, else the first day of that year.
    Private Function DefaultOpDate() As String
        Return If(Date.Today.Year = YearOfWork(), Date.Today, New Date(YearOfWork(), 1, 1)).ToString(DateFormat, CultureInfo.InvariantCulture)
    End Function

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
                txtCodClient.Text = _partner.CodClient
                gridCurent.Enabled = True
                gridAnterior.Enabled = True
                gridPlata.Enabled = True
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

    ' The payment grid: the FIRST row (no Tag) is the payment being made -- what the operator typed in it survives a re-read of the partner -- and the rows below
    ' (Tag = the order) are the orders of the partner in the year, read-only.
    Private Sub FillOrders(k_orders As IReadOnlyList(Of PlataOrdin))
        Dim k_old As KBotDataRow = PaymentRow()
        Dim k_date As String = If(k_old Is Nothing, DefaultOpDate(), Convert.ToString(k_old("data"), CultureInfo.InvariantCulture))
        Dim k_note As String = If(k_old Is Nothing, DefaultNote(), Convert.ToString(k_old("nota"), CultureInfo.InvariantCulture))
        Dim k_explanation As String = If(k_old Is Nothing, String.Empty, Convert.ToString(k_old("explicatie"), CultureInfo.InvariantCulture))
        gridPlata.BeginUpdate()
        Try
            gridPlata.ClearRows()
            Dim k_new As KBotDataRow = gridPlata.AddRow()
            k_new("nrop") = String.Empty
            k_new("data") = k_date
            k_new("nota") = k_note
            k_new("documente") = String.Empty
            k_new("explicatie") = k_explanation
            k_new("valoare") = 0D
            For Each k_o As PlataOrdin In k_orders
                Dim k_row As KBotDataRow = gridPlata.AddRow()
                k_row.Tag = k_o
                k_row("nrop") = k_o.NrOP.ToString(CultureInfo.InvariantCulture)
                k_row("data") = If(k_o.DataOP.HasValue, k_o.DataOP.Value.ToString(DateFormat, CultureInfo.InvariantCulture), String.Empty)
                k_row("nota") = String.Empty
                k_row("documente") = k_o.Documente
                k_row("explicatie") = k_o.Explicatie
                k_row("valoare") = k_o.Valoare
            Next
        Finally
            gridPlata.EndUpdate()
        End Try
    End Sub

    ' The payment being made (first row, no Tag); Nothing while no partner is read.
    Private Function PaymentRow() As KBotDataRow
        Return If(gridPlata.Rows.Count > 0 AndAlso gridPlata.Rows(0).Tag Is Nothing, gridPlata.Rows(0), Nothing)
    End Function

    Private Shared Function TextOf(k_row As KBotDataRow, k_key As String) As String
        Return Convert.ToString(k_row(k_key), CultureInfo.InvariantCulture)
    End Function

    ' The spare row at the end of the grid of new invoices: the place where the next invoice is typed.
    Private Sub AddBlankInvoiceRow(Optional k_note As String = Nothing)
        Dim k_row As KBotDataRow = gridNoi.AddRow()
        k_row("numar") = String.Empty
        k_row("data") = Date.Today.ToString(DateFormat, CultureInfo.InvariantCulture)
        k_row("fel") = "Factură"
        k_row("nota") = If(String.IsNullOrWhiteSpace(k_note), DefaultNote(), k_note)
        k_row("suma") = 0D
        k_row("platit") = 0D
    End Sub

    Private Shared Function IsBlankInvoice(k_row As KBotDataRow) As Boolean
        Return TextOf(k_row, "numar").Trim().Length = 0 AndAlso ParseAmount(k_row("suma")) = 0D AndAlso ParseAmount(k_row("platit")) = 0D
    End Function

    ' The invoices typed by hand, without the spare row.
    Private Function TypedInvoices() As List(Of KBotDataRow)
        Dim k_rows As New List(Of KBotDataRow)()
        For Each k_row As KBotDataRow In gridNoi.Rows
            If Not IsBlankInvoice(k_row) Then k_rows.Add(k_row)
        Next
        Return k_rows
    End Function

    ' Gives up everything typed for the previous partner.
    Private Sub ClearEdits()
        _filling = True
        Try
            gridNoi.ClearRows()
            AddBlankInvoiceRow()
            gridPlata.ClearRows()
            _dirty = False
            _explanationTyped = False
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

    ' The nav list shows ONE of the three grids at a time.
    Private Sub NavFacturi_SelectionChanged(k_key As String) Handles navFacturi.SelectionChanged
        Try
            gridNoi.Visible = String.Equals(k_key, ViewNoi, StringComparison.Ordinal)
            gridCurent.Visible = String.Equals(k_key, ViewCurent, StringComparison.Ordinal)
            gridAnterior.Visible = String.Equals(k_key, ViewAnterior, StringComparison.Ordinal)
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.NavFacturi_SelectionChanged", ex)
        End Try
    End Sub

    ' The spare row is what the operator types in; once it holds something a new spare row follows it.
    Private Sub KeepBlankRowLast()
        If gridNoi.Rows.Count = 0 OrElse Not IsBlankInvoice(gridNoi.Rows(gridNoi.Rows.Count - 1)) Then
            Dim k_was As Boolean = _filling
            _filling = True
            Try
                ' The next invoice starts in the same note as the one just typed (the usual case: a batch of invoices in one note).
                AddBlankInvoiceRow(If(gridNoi.Rows.Count = 0, Nothing, TextOf(gridNoi.Rows(gridNoi.Rows.Count - 1), "nota")))
            Finally
                _filling = k_was
            End Try
            gridNoi.Invalidate()
        End If
    End Sub

    ' The delete column of the grid (the standard one): takes the typed invoice out. The spare row has no button.
    Private Sub GridNoi_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridNoi.ButtonClick
        Try
            If Not String.Equals(e.ColumnKey, ColDelete, StringComparison.Ordinal) OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridNoi.Rows.Count OrElse IsBlankInvoice(gridNoi.Rows(e.RowIndex)) Then Return
            gridNoi.RemoveRowAt(e.RowIndex)
            KeepBlankRowLast()
            Recompose()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridNoi_ButtonClick", ex)
        End Try
    End Sub

    Private Sub GridNoi_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridNoi.CellFormatting
        If String.Equals(e.ColumnKey, ColDelete, StringComparison.Ordinal) AndAlso e.Row IsNot Nothing AndAlso IsBlankInvoice(e.Row) Then
            e.HideButton = True
            e.Enabled = False
        End If
    End Sub

    Private Sub GridNoi_ComboItemsRequested(sender As Object, e As KBotComboItemsEventArgs) Handles gridNoi.ComboItemsRequested
        e.Items = If(String.Equals(e.ColumnKey, "nota", StringComparison.Ordinal), _noteItems, _kinds)
    End Sub

    Private Sub GridPlata_ComboItemsRequested(sender As Object, e As KBotComboItemsEventArgs) Handles gridPlata.ComboItemsRequested
        e.Items = _noteItems
    End Sub

    ' Only the payment row is typed in; the orders of the year below it are shown as they were posted. The payment row has no delete button (nothing to delete yet).
    Private Sub GridPlata_CellFormatting(sender As Object, e As KBotCellFormattingEventArgs) Handles gridPlata.CellFormatting
        If e.Row Is Nothing Then Return
        If e.Row.Tag IsNot Nothing Then e.ReadOnlyCell = True
        If String.Equals(e.ColumnKey, ColDelete, StringComparison.Ordinal) AndAlso e.Row.Tag Is Nothing Then
            e.HideButton = True
            e.Enabled = False
        End If
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
            If sender Is gridNoi Then KeepBlankRowLast()
            Recompose()
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.Grid_CellValueChanged", ex)
        End Try
    End Sub

    ' What the operator types in the explanation is made valid (ASCII, no comma, at most 75 characters) and from then on is no longer composed.
    Private Sub GridPlata_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridPlata.CellValidating
        Try
            If Not String.Equals(e.ColumnKey, "explicatie", StringComparison.Ordinal) Then Return
            Dim k_clean As String = OpExplicatie.Clean(Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture))
            If k_clean.Length > OpExplicatie.MaxLength Then k_clean = k_clean.Substring(0, OpExplicatie.MaxLength)
            e.ProposedValue = k_clean
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridPlata_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridPlata_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridPlata.CellValueChanged
        Try
            If _filling OrElse e.RowIndex <> 0 Then Return
            If String.Equals(e.ColumnKey, "explicatie", StringComparison.Ordinal) Then
                _explanationTyped = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture).Length > 0
            End If
            _dirty = True
            RefreshState()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridPlata_CellValueChanged", ex)
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
        Dim k_row As KBotDataRow = PaymentRow()
        If k_row Is Nothing Then Return
        Dim k_cut As Boolean
        Dim k_was As Boolean = _filling
        _filling = True
        Try
            k_row("explicatie") = OpExplicatie.Compose(PaidNumbers(), String.Empty, If(k_template Is Nothing, String.Empty, OpExplicatie.Fit(k_template.FelOP)), k_cut)
        Finally
            _filling = k_was
        End Try
        gridPlata.Invalidate()
    End Sub

    ' ── Delete a payment ────────────────────────────────────────────────────────

    ' The delete column of the payment grid (the standard one), on the rows of the orders of the year. An order that sits in an OPME or is confirmed by a bank
    ' statement is not deleted: the message says why.
    Private Async Sub GridPlata_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridPlata.ButtonClick
        Try
            If Not String.Equals(e.ColumnKey, ColDelete, StringComparison.Ordinal) OrElse _busy Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridPlata.Rows.Count Then Return
            Dim k_order As PlataOrdin = TryCast(gridPlata.Rows(e.RowIndex).Tag, PlataOrdin)
            If k_order Is Nothing Then Return
            If k_order.Blocat Then
                KBotMessage.Show(Me, k_order.Motiv & " Plata și ordinul nu se mai pot șterge.", "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If KBotMessage.Show(Me, $"Ștergeți plata cu ordinul de plată {k_order.NrOP.ToString(CultureInfo.InvariantCulture)}, împreună cu ordinul?" & vbLf & "Operația nu poate fi anulată.",
                                "Facturi și plăți", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            SetBusy(True)
            Await _gate.RunAsync(Function() _api.StergePlataFacturiAsync(k_order.IdOP, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            KBotMessage.Show(Me, $"Plata cu ordinul {k_order.NrOP.ToString(CultureInfo.InvariantCulture)} a fost ștearsă.", "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Information)
            SetBusy(False)
            Await LoadPartnerAsync().ConfigureAwait(True)
        Catch ex As ApiException
            GlobalErrorLog.Write("FacturiPlatiPage.GridPlata_ButtonClick", ex)
            SetBusy(False)
            SetStatus(ex.Message)
            KBotMessage.Show(Me, ex.Message, "Facturi și plăți", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.GridPlata_ButtonClick", ex)
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
        Dim k_payRow As KBotDataRow = PaymentRow()
        If k_payRow Is Nothing Then Return "Alegeți partenerul."
        Dim k_paying As Boolean = _idSablonP > 0
        ' The note of the payment (its order) is mandatory only when something is paid; every new invoice carries its own mandatory note.
        Dim k_note As Integer
        If Not Integer.TryParse(TextOf(k_payRow, "nota").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_note) OrElse k_note <= 0 Then
            If k_paying Then Return "Nota contabilă a plății trebuie să fie un număr întreg pozitiv."
            k_note = 0
        End If
        Dim k_new As New List(Of FacturaNoua)()
        For Each k_row As KBotDataRow In TypedInvoices()
            Dim k_number As String = Convert.ToString(k_row("numar"), CultureInfo.InvariantCulture).Trim()
            If k_number.Length = 0 Then Return "O factură nouă nu are număr."
            Dim k_date As Date
            If Not ParseDate(k_row("data"), k_date) Then Return $"Data facturii {k_number} nu este validă (zz.ll.aaaa)."
            Dim k_amount As Decimal = ParseAmount(k_row("suma"))
            If k_amount = 0D Then Return $"Factura {k_number} are valoarea zero."
            Dim k_invoiceNote As Integer
            If Not Integer.TryParse(TextOf(k_row, "nota").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, k_invoiceNote) OrElse k_invoiceNote <= 0 Then
                Return $"Factura {k_number} nu are număr de notă contabilă (obligatoriu, număr întreg pozitiv)."
            End If
            k_new.Add(New FacturaNoua() With {.Numar = k_number, .Data = k_date, .Suma = k_amount, .Fel = Convert.ToString(k_row("fel"), CultureInfo.InvariantCulture),
                                              .NumarNC = k_invoiceNote, .Platit = ParseAmount(k_row("platit"))})
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
        Dim k_dateOp As Date
        If Not ParseDate(TextOf(k_payRow, "data"), k_dateOp) Then
            If k_paying Then Return "Data ordinului de plată nu este validă (zz.ll.aaaa)."
            k_dateOp = New Date(YearOfWork(), 1, 1)
        End If
        If k_paying AndAlso k_dateOp.Year <> YearOfWork() Then Return $"Data ordinului de plată trebuie să fie în anul de lucru ({YearOfWork()})."
        k_request = New FacturiPlatiCerere() With {
            .IdSubunitate = _idSubunitate, .An = YearOfWork(), .IdPartener = _partner.IdPartener, .IdClsf = _classification.IdClsf,
            .IdSablonE = _idSablonE, .IdSablonP = If(k_paying, _idSablonP, 0), .NumarNC = k_note, .DataOP = k_dateOp,
            .ExplicatieOP = TextOf(k_payRow, "explicatie").Trim(), .Noi = k_new, .Existente = k_chosen}
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
            Dim k_notes As String = String.Join(", ", k_result.Note.Select(Function(k_n) k_n.ToString(CultureInfo.InvariantCulture)))
            Dim k_text As String = $"{k_result.Facturi.ToString(CultureInfo.InvariantCulture)} facturi noi înregistrate" & If(k_notes.Length > 0, $" (nota contabilă {k_notes})", String.Empty)
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
            Return _partner IsNot Nothing AndAlso _dirty AndAlso (TypedInvoices().Count > 0 OrElse TotalToPay() <> 0D)
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
        Dim k_typed As Integer = TypedInvoices().Count
        btnSalveaza.Enabled = k_active AndAlso _classification IsNot Nothing AndAlso (k_typed > 0 OrElse k_total <> 0D)
        btnRenunta.Enabled = k_active
        ' The payment row shows what is paid now and the order number that would follow (PROVISIONAL).
        Dim k_pay As KBotDataRow = PaymentRow()
        If k_pay IsNot Nothing Then
            Dim k_was As Boolean = _filling
            _filling = True
            Try
                k_pay("valoare") = k_total
                k_pay("nrop") = If(_partnerData IsNot Nothing AndAlso k_paying AndAlso k_total <> 0D, $"{_partnerData.NumarOP.ToString(CultureInfo.InvariantCulture)} (provizoriu)", String.Empty)
            Finally
                _filling = k_was
            End Try
            gridPlata.Invalidate()
        End If
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.PanelBack()
            For Each k_c As Control In {tlyPagina, tlyClsf, tlyAntet, tlyView, pnlView, tlyJos}
                k_c.BackColor = k_scheme.PanelBack()
            Next
            lblStare.ForeColor = k_scheme.Palette.TextDimColor
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiPlatiPage.ApplyTheme", ex)
        End Try
    End Sub

End Class
