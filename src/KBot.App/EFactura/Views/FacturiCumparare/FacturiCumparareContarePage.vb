Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 01EF-02 / 01EF-07 -- the «Contari» page, laid out like the Access form: the classification, the client code, the account and the kind of invoice on top;
''' the evidence templates and the evidence grid (Access EF_F: one row per ticked invoice, only the value to pay is edited); the payment templates and the order of
''' payment (Access EF_P: one row, awake ONLY while a payment template is chosen), with the length of its explanation in a small progress column right after it (slice 03EF-06; it was a bar under the grid).
''' Slice 03EF-03 / 03EF-04: «Renunță» and «Salvează», and the line with the supplier and the warnings, left the page: the buttons are in the ribbon (the view group) and the
''' warnings are the tooltip of «Salvează». The page only OWNS its controls and shows what <see cref="FacturiCumparareView"/> hands it (the server's calculation); it asks the view, through events, to calculate again.
''' The text of the order of payment is ASCII without commas (<see cref="OpExplicatie"/>).
''' </summary>
Public Class FacturiCumparareContarePage
    Implements IThemedContainer

    ''' <summary>The operator chose a classification.</summary>
    Public Event ClasificatieAleasa(k_idClsf As Integer)

    ''' <summary>The operator chose a template: <paramref name="k_fel"/> «E» (evidence) or «P» (payment); <paramref name="k_idSablon"/> 0 = the payment template was taken off.</summary>
    Public Event SablonAles(k_fel As String, k_idSablon As Integer)

    ''' <summary>The operator changed the value to pay of an invoice.</summary>
    Public Event PlatitSchimbat(k_idPrimita As Integer, k_valoare As Decimal)

    ''' <summary>Slice 03EF-04: what decides whether «Salvează» (a button of the ribbon) can be pressed changed; the host reads <see cref="PoateSalva"/> again.</summary>
    Public Event SalvareStareSchimbata()

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")
    Private Shared ReadOnly _clsfNameComparer As StringComparer = StringComparer.Create(_ro, True)
    ' Clsf is Capitol.Subcapitol.Articol.Alineat, with an article code of the form NN.NN.
    Private Shared ReadOnly _clsfArticle As New Regex("\.([0-9]{2}\.[0-9]{2})\.[0-9]{0,2}$", RegexOptions.CultureInvariant)

    Private _filling As Boolean
    Private _ultim As EFacturaContariRaspuns
    Private _supplierCui As String = String.Empty
    Private _nota As String = String.Empty
    Private _noteList As New List(Of Object)()
    Private _nrOp As String = String.Empty
    Private _felOpFor As Integer?
    Private _canSave As Boolean
    Private _saveEnabled As Boolean
    Private _complete As Boolean
    Private _detalii As String = String.Empty
    Private _explicatieLen As Integer

    ' Slice 03EF-13: the page shows what an invoice ALREADY has booked (read only): the template rows chosen at that time are kept to be put back when a click moves them.
    Private _booked As Boolean
    Private _nrOpProvisional As Boolean
    Private _bookedRowE As Integer = -1
    Private _bookedRowP As Integer = -1

    ' What the combo of the classification says while nothing is ticked (slice 03EF-04: the line of the foot that said it is gone).
    Private Const ClsfIdle As String = "Apăsați «Adaugă operații contabile»"
    Private Const ClsfPlaceholder As String = "Alegeți clasificația bugetară"

    Public Sub New()
        InitializeComponent()
        gridEvidenta.ReadOnlyGrid = False
        gridPlata.ReadOnlyGrid = False
        gridPlata.CellTooltipTableProvider = AddressOf PlataTooltip   ' slice 04EF-01: the order number before saving is provisional
        ShowEmpty()
    End Sub

    ''' <summary>Nothing is ticked: the page is emptied; the combo of the classification says how to start.</summary>
    Public Sub ShowEmpty()
        _filling = True
        Try
            _booked = False
            gridEvidenta.ReadOnlyGrid = False
            gridPlata.ReadOnlyGrid = False
            _ultim = Nothing
            _supplierCui = String.Empty
            _nota = String.Empty
            _nrOp = String.Empty
            _felOpFor = Nothing
            gridEvidenta.ClearRows()
            gridPlata.ClearRows()
            gridPlata.Enabled = False
            lstEvidentiere.ClearRows()
            lstPlata.ClearRows()
            lstEvidentiere.Enabled = False
            lstPlata.Enabled = False
            cmbClsf.Items.Clear()
            cmbClsf.Text = String.Empty
            cmbClsf.PlaceholderText = ClsfIdle
            cmbClsf.Enabled = False
            txtIban.Text = String.Empty
            txtCodClient.Text = String.Empty
            txtFelOp.Text = String.Empty
            txtCodClient.Enabled = False
            txtFelOp.Enabled = False
            _detalii = String.Empty
            _canSave = False
            UpdateSave()
        Finally
            _filling = False
        End Try
    End Sub

    ''' <summary>Puts the server's calculation on screen.</summary>
    Public Sub ShowResult(k_r As EFacturaContariRaspuns)
        ArgumentNullException.ThrowIfNull(k_r)
        _filling = True
        Try
            _booked = False
            gridEvidenta.ReadOnlyGrid = False
            gridPlata.ReadOnlyGrid = False
            ' Another supplier: what was typed for the previous one (client code, note, order number, kind of invoice) goes.
            If Not String.Equals(k_r.FurnizorCui, _supplierCui, StringComparison.Ordinal) Then
                _supplierCui = k_r.FurnizorCui
                _nota = String.Empty
                _nrOp = String.Empty
                _felOpFor = Nothing
                txtCodClient.Text = String.Empty
            End If
            _ultim = k_r
            cmbClsf.PlaceholderText = ClsfPlaceholder
            FillClassifications(k_r)
            FillList(lstEvidentiere, k_r.SabloaneEvidentiere, k_r.IdSablonE, k_r.Clasificatie IsNot Nothing)
            FillList(lstPlata, k_r.SabloanePlata, k_r.IdSablonP, k_r.Clasificatie IsNot Nothing)
            _noteList = k_r.Note.Select(Function(k_n) CType(k_n.NumarNC.ToString(CultureInfo.InvariantCulture), Object)).ToList()
            FillInvoices(k_r)
            txtIban.Text = k_r.ContIban
            txtCodClient.Enabled = True
            txtFelOp.Enabled = k_r.IdSablonP.HasValue
            If Not Nullable.Equals(k_r.IdSablonP, _felOpFor) Then
                txtFelOp.Text = If(k_r.IdSablonP.HasValue, OpExplicatie.Fit(k_r.Plata.FelOP), String.Empty)
                _felOpFor = k_r.IdSablonP
            End If
            FillPayment(k_r)
            _detalii = If(String.IsNullOrWhiteSpace(k_r.FurnizorDenumire), "Furnizor necunoscut", k_r.FurnizorDenumire) & vbLf & DetailsText(k_r)
            _canSave = CanSave(k_r)
            UpdateSave()
        Finally
            _filling = False
        End Try
    End Sub

    ''' <summary>
    ''' Slice 03EF-13: the invoice that was clicked already has accounting operations: the page shows them, read only. The classification and the templates they
    ''' were made from are chosen in the lists «Documente evidențiere» / «Documente plată», the evidence operations are written in the evidence grid and the payment ones in the payment grid.
    ''' </summary>
    Public Sub ShowBooked(k_b As EFacturaContariExistente)
        ArgumentNullException.ThrowIfNull(k_b)
        ShowEmpty()
        _filling = True
        Try
            _booked = True
            Dim k_evidence As List(Of EFacturaContariExistenta) = k_b.Operatii.Where(Function(k_o) String.Equals(k_o.Faza, "E", StringComparison.Ordinal)).ToList()
            Dim k_payments As List(Of EFacturaContariExistenta) = k_b.Operatii.Where(Function(k_o) String.Equals(k_o.Faza, "P", StringComparison.Ordinal)).ToList()
            If k_b.Clasificatie IsNot Nothing Then
                Dim k_text As String = $"{k_b.Clasificatie.Clsf}  {k_b.Clasificatie.Denumire}"
                cmbClsf.Items.Add(New KBotComboGroupedItem(k_text, "Clasificația operațiilor înregistrate", k_text, k_b.Clasificatie.IdClsf))
                cmbClsf.SelectedIndex = 0
            Else
                cmbClsf.PlaceholderText = "Fără clasificație"
            End If
            _bookedRowE = FillBookedList(lstEvidentiere, k_b.SabloaneEvidentiere, k_evidence)
            _bookedRowP = FillBookedList(lstPlata, k_b.SabloanePlata, k_payments)

            gridEvidenta.BeginUpdate()
            Try
                Dim k_paid As Decimal = k_payments.Sum(Function(k_o) k_o.Suma)
                For Each k_o As EFacturaContariExistenta In k_evidence
                    Dim k_row As KBotDataRow = gridEvidenta.AddRow()
                    k_row("nc") = If(k_o.NumarNC.HasValue, k_o.NumarNC.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                    k_row("doc") = If(String.IsNullOrEmpty(k_o.NumarDocument) OrElse String.Equals(k_o.NumarDocument, k_b.NrFact, StringComparison.Ordinal), k_b.NrFact, $"{k_o.NumarDocument} ({k_b.NrFact})")
                    k_row("data") = If(k_o.DataDocument.HasValue, k_o.DataDocument.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                    k_row("fel") = If(String.IsNullOrEmpty(k_o.FelDocument), k_b.FelDocument, k_o.FelDocument)
                    k_row("suma") = k_o.Suma
                    k_row("platit") = If(gridEvidenta.Rows.Count = 1, k_paid, 0D)   ' what was paid is on the first row of the invoice
                Next
            Finally
                gridEvidenta.EndUpdate()
            End Try
            gridEvidenta.ReadOnlyGrid = True

            gridPlata.BeginUpdate()
            Try
                For Each k_o As EFacturaContariExistenta In k_payments
                    Dim k_row As KBotDataRow = gridPlata.AddRow()
                    k_row("nc") = If(k_o.NumarNC.HasValue, k_o.NumarNC.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                    k_row("data") = If(k_o.DataDocument.HasValue, k_o.DataDocument.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                    k_row("nrop") = k_o.NumarDocument
                    k_row("explicatie") = k_o.Explicatie
                    k_row("lungime") = Math.Min(k_o.Explicatie.Length, OpExplicatie.MaxLength)
                    k_row("valoare") = k_o.Suma
                Next
            Finally
                gridPlata.EndUpdate()
            End Try
            ' Slice 04EF-01: the payments made in K-BOT (they live in the allocations, not among the evidence rows): the order number is the REAL one here.
            gridPlata.BeginUpdate()
            Try
                For Each k_pl As EFacturaContariPlataExistenta In k_b.Plati
                    Dim k_row As KBotDataRow = gridPlata.AddRow()
                    k_row.Tag = k_pl
                    k_row("nc") = If(k_pl.NumarNC.HasValue, k_pl.NumarNC.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                    k_row("data") = If(k_pl.DataPlata.HasValue, k_pl.DataPlata.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                    k_row("nrop") = If(k_pl.NrOP.HasValue, k_pl.NrOP.Value.ToString(CultureInfo.InvariantCulture), String.Empty)
                    k_row("explicatie") = k_pl.Explicatie
                    k_row("lungime") = Math.Min(k_pl.Explicatie.Length, OpExplicatie.MaxLength)
                    k_row("valoare") = k_pl.Platit
                Next
            Finally
                gridPlata.EndUpdate()
            End Try
            gridPlata.ReadOnlyGrid = True
            gridPlata.Enabled = gridPlata.Rows.Count > 0
            Dim k_sub As String = k_b.Operatii.Select(Function(k_o) k_o.Subunitate).FirstOrDefault(Function(k_s) k_s.Length > 0)
            _detalii = $"Factura {k_b.NrFact} are deja operații contabile înregistrate" & If(String.IsNullOrEmpty(k_sub), String.Empty, $" ({k_sub})") & "."
            If k_b.Operatii.Any(Function(k_o) Not k_o.Gasita) Then
                _detalii &= " Factura are o legătură fără operație contabilă în baza de date: rândul afișează doar suma legăturii, fără notă și dată."
            End If
            _canSave = False
            UpdateSave()
        Finally
            _filling = False
        End Try
    End Sub

    ' The templates that fit the classification, the one the booked operations were made from chosen (by its id; a migrated operation has none, so by its explanation).
    ' Without templates the explanations of the operations stand in their place. Returns the chosen row (-1 = none).
    Private Shared Function FillBookedList(k_list As KBotDataView, k_templates As List(Of EFacturaContariSablon), k_operations As List(Of EFacturaContariExistenta)) As Integer
        k_list.BeginUpdate()
        Dim k_chosen As Integer = -1
        Try
            k_list.ClearRows()
            For Each k_s As EFacturaContariSablon In k_templates
                Dim k_row As KBotDataRow = k_list.AddRow()
                k_row("nume") = k_s.Explicatie
                If k_chosen < 0 AndAlso k_operations.Any(Function(k_o) (k_o.IdSablon.HasValue AndAlso k_o.IdSablon.Value = k_s.IdSablon) OrElse
                    (Not k_o.IdSablon.HasValue AndAlso String.Equals(k_o.Explicatie, k_s.Explicatie, StringComparison.CurrentCultureIgnoreCase))) Then k_chosen = k_list.Rows.Count - 1
            Next
            If k_templates.Count = 0 Then
                For Each k_text As String In k_operations.Select(Function(k_o) k_o.Explicatie).Where(Function(k_t) k_t.Length > 0).Distinct()
                    k_list.AddRow()("nume") = k_text
                    If k_chosen < 0 Then k_chosen = 0
                Next
            End If
            k_list.CurrentRowIndex = k_chosen
        Finally
            k_list.EndUpdate()
        End Try
        k_list.Enabled = k_list.Rows.Count > 0
        Return k_chosen
    End Function

    ' The evidence can be booked: the role allows it, the classification and the evidence template are chosen, nothing blocks and something is left to book.
    ' Slice 04EF-01: the mode decides what must be chosen -- evidence only (the evidence template), payment only (the payment template and something to pay) or both.
    Private Shared Function CanSave(k_r As EFacturaContariRaspuns) As Boolean
        Dim k_paying As Boolean = Not String.Equals(k_r.[Mod], "evidentiere", StringComparison.Ordinal)
        Dim k_evidence As Boolean = Not String.Equals(k_r.[Mod], "plata", StringComparison.Ordinal)
        Return k_r.PoateScrie AndAlso k_r.Clasificatie IsNot Nothing AndAlso (Not k_evidence OrElse k_r.IdSablonE.HasValue) AndAlso
               (Not k_paying OrElse k_r.IdSablonP.HasValue) AndAlso
               Not k_r.Avertismente.Any(Function(k_a) String.Equals(k_a.Grav, "eroare", StringComparison.Ordinal)) AndAlso
               If(k_paying, k_r.Evidenta.Any(Function(k_e) k_e.Platit <> 0D), k_r.Evidenta.Any(Function(k_e) k_e.Suma <> 0D))
    End Function

    ''' <summary>Slice 04EF-01: the order number the page showed before saving (provisional); empty when none.</summary>
    Public ReadOnly Property NumarOpAfisat As String
        Get
            Return _nrOp
        End Get
    End Property

    ''' <summary>Slice 04EF-01: the date of the payment and of its order, as typed in the payment row; Nothing = the server uses today.</summary>
    Public Function DataOP() As Date?
        If gridPlata.Rows.Count = 0 Then Return Nothing
        Dim k_date As Date
        Return If(Date.TryParseExact(Convert.ToString(gridPlata.Rows(0)("data"), CultureInfo.InvariantCulture), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, k_date),
                  CType(k_date, Date?), Nothing)
    End Function

    ''' <summary>Slice 04EF-01: the explanation of the order of payment as shown in the payment row (empty when there is none).</summary>
    Public Function ExplicatieOP() As String
        Return If(gridPlata.Rows.Count = 0, String.Empty, Convert.ToString(gridPlata.Rows(0)("explicatie"), CultureInfo.InvariantCulture))
    End Function

    ''' <summary>Slice 04EF-01: the payment (read only view of a booked invoice) the operator stands on; Nothing when none.</summary>
    Public ReadOnly Property PlataAleasa As EFacturaContariPlataExistenta
        Get
            If Not _booked OrElse gridPlata.CurrentRowIndex < 0 OrElse gridPlata.CurrentRowIndex >= gridPlata.Rows.Count Then Return Nothing
            Return TryCast(gridPlata.Rows(gridPlata.CurrentRowIndex).Tag, EFacturaContariPlataExistenta)
        End Get
    End Property

    ' The number of the order before saving is only a PROVISIONAL one: the server gives the real number on save and the page shows it afterwards.
    Private Function PlataTooltip(k_column As String, k_row As KBotDataRow) As String
        If _booked OrElse Not _nrOpProvisional OrElse Not String.Equals(k_column, "nrop", StringComparison.Ordinal) Then Return Nothing
        Return "Număr provizoriu. Numărul final al ordinului de plată se stabilește la salvare și poate fi altul, dacă între timp s-a emis un alt ordin din aceeași plajă de numere. După salvare vezi numărul real."
    End Function

    ' «Salvează» (the ribbon button) also needs a valid explanation while an order of payment is being made. The host is told when the answer changes.
    Private Sub UpdateSave()
        _complete = GridFilled(gridEvidenta) AndAlso GridFilled(gridPlata)
        Dim k_enabled As Boolean = _canSave AndAlso _complete AndAlso (gridPlata.Rows.Count = 0 OrElse _explicatieLen >= OpExplicatie.MinLength)
        If k_enabled = _saveEnabled Then Return
        _saveEnabled = k_enabled
        RaiseEvent SalvareStareSchimbata()
    End Sub

    ' Slice 03EF-10: every cell of the rows of a grid holds something (the progress column always does). A number, even 0, is something.
    Private Shared Function GridFilled(k_grid As KBotDataView) As Boolean
        For Each k_row As KBotDataRow In k_grid.Rows
            For Each k_col As KBotDataColumn In k_grid.Columns
                If k_col.ColumnType = KBotColumnType.ProgressBar Then Continue For
                Dim k_value As Object = k_row(k_col.Key)
                If k_value Is Nothing OrElse TypeOf k_value Is DBNull Then Return False
                Dim k_text As String = TryCast(k_value, String)
                If k_text IsNot Nothing AndAlso String.IsNullOrWhiteSpace(k_text) Then Return False
            Next
        Next
        Return True
    End Function

    ''' <summary>Slice 03EF-04: «Salvează» can be pressed (the role allows it, nothing blocks, something is left to book, the explanation is valid).</summary>
    Public ReadOnly Property PoateSalva As Boolean
        Get
            Return _saveEnabled
        End Get
    End Property

    ''' <summary>The supplier, the warnings and the bookings the server calculated (what the line at the foot of the page used to say); empty when nothing is ticked.</summary>
    Public ReadOnly Property Detalii As String
        Get
            ' Slice 03EF-10: why «Salvează» is dimmed when only the cells are missing.
            Return If(_canSave AndAlso Not _complete, "Pentru «Salvează» completează toate celulele din tabele (nota contabilă, numărul O.P., explicația ...)." & vbLf, String.Empty) & _detalii
        End Get
    End Property

    ''' <summary>True while an order of payment is being made (a payment template is chosen and there is something to pay).</summary>
    Public ReadOnly Property PlataActiva As Boolean
        Get
            Return gridPlata.Rows.Count > 0
        End Get
    End Property

    ' SLICE-01EF-08: article headers only expand or collapse. Each selectable leaf carries its classification ID.
    Private Sub FillClassifications(k_r As EFacturaContariRaspuns)
        Dim k_leaves As New List(Of KBotComboGroupedItem)()
        Dim k_ordered = k_r.Clasificatii.
            Select(Function(k_c) New With {.Classification = k_c, .Article = _clsfArticle.Match(k_c.Clsf).Groups(1).Value}).
            OrderByDescending(Function(k_x) k_x.Classification.AlePartenerului).
            ThenBy(Function(k_x) k_x.Article.Length = 0).
            ThenBy(Function(k_x) k_x.Article, StringComparer.Ordinal).
            ThenBy(Function(k_x) k_x.Classification.Denumire, _clsfNameComparer).
            ThenBy(Function(k_x) k_x.Classification.Clsf, StringComparer.Ordinal)
        For Each k_entry In k_ordered
            Dim k_c As EFacturaContariClsf = k_entry.Classification
            Dim k_branch As String = If(k_entry.Article.Length > 0, $"Articol {k_entry.Article}", "Alte clasificații")
            If k_c.AlePartenerului Then k_branch &= " · configurate pentru furnizor"
            Dim k_text As String = $"{k_c.Clsf}  {k_c.Denumire}"
            k_leaves.Add(New KBotComboGroupedItem(k_text, k_branch, k_text, k_c.IdClsf))
        Next

        If Not k_leaves.SequenceEqual(cmbClsf.Items.Cast(Of KBotComboGroupedItem)()) Then
            cmbClsf.BeginUpdate()
            Try
                cmbClsf.Items.Clear()
                cmbClsf.Items.AddRange(k_leaves)
            Finally
                cmbClsf.EndUpdate()
            End Try
        End If
        cmbClsf.Enabled = k_leaves.Count > 0
        Dim k_selected As Integer = If(k_r.Clasificatie Is Nothing, -1,
            k_leaves.FindIndex(Function(k_leaf) CInt(k_leaf.Tag) = k_r.Clasificatie.IdClsf))
        If k_selected < 0 Then
            cmbClsf.Text = String.Empty
        Else
            cmbClsf.SelectedIndex = k_selected
        End If
    End Sub

    Private Sub Clsf_UpdateDropDownWidth(sender As Object, e As EventArgs) Handles cmbClsf.HandleCreated, cmbClsf.SizeChanged, cmbClsf.DpiChangedAfterParent
        If Not cmbClsf.IsHandleCreated Then Return
        Dim k_scale As Single = AppScaling.FactorFor(cmbClsf)
        Dim k_width As Integer = CInt(Math.Ceiling(cmbClsf.Width / k_scale))
        cmbClsf.DropDownWidth = Math.Max(680, k_width + 240)
    End Sub

    ' The templates of one kind: a row each, the chosen one is the current row. The explanation of a template is shown; the journal only when two templates would look alike.
    Private Shared Sub FillList(k_list As KBotDataView, k_templates As List(Of EFacturaContariSablon), k_chosen As Integer?, k_enabled As Boolean)
        k_list.BeginUpdate()
        Try
            k_list.ClearRows()
            Dim k_chosenRow As Integer = -1
            For Each k_s As EFacturaContariSablon In k_templates
                Dim k_row As KBotDataRow = k_list.AddRow()
                k_row.Tag = k_s
                Dim k_same As Boolean = k_templates.Where(Function(k_t) String.Equals(k_t.Explicatie, k_s.Explicatie, StringComparison.CurrentCultureIgnoreCase)).Count() > 1
                k_row("nume") = If(k_same, $"{k_s.Explicatie}  [{k_s.Jurnal}]", k_s.Explicatie)
                If k_chosen.HasValue AndAlso k_s.IdSablon = k_chosen.Value Then k_chosenRow = k_list.Rows.Count - 1
            Next
            k_list.CurrentRowIndex = k_chosenRow
        Finally
            k_list.EndUpdate()
        End Try
        k_list.Enabled = k_enabled
    End Sub

    ''' <summary>The note the operator chose (an item) or typed (a number); Nothing = none (a new note with the next number). <paramref name="k_valid"/> is False when the text is not a positive whole number.</summary>
    Public Function NotaAleasa(ByRef k_valid As Boolean) As Integer?
        k_valid = True
        Dim k_text As String = _nota.Trim()
        If k_text.Length = 0 Then Return Nothing
        Dim k_number As Integer
        If Integer.TryParse(k_text, NumberStyles.None, CultureInfo.InvariantCulture, k_number) AndAlso k_number > 0 Then Return k_number
        k_valid = False
        Return Nothing
    End Function

    Private Sub FillInvoices(k_r As EFacturaContariRaspuns)
        gridEvidenta.BeginUpdate()
        Try
            gridEvidenta.ClearRows()
            For Each k_e As EFacturaContariRand In k_r.Evidenta
                Dim k_row As KBotDataRow = gridEvidenta.AddRow()
                k_row.Tag = k_e
                k_row("nc") = _nota
                k_row("doc") = If(String.Equals(k_e.NumarDocument, k_e.NrFact, StringComparison.Ordinal), k_e.NrFact, $"{k_e.NumarDocument} ({k_e.NrFact})")
                k_row("data") = If(k_e.DataDocument.HasValue, k_e.DataDocument.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_row("fel") = k_e.FelDocument
                k_row("suma") = k_e.Suma
                k_row("platit") = k_e.Platit
            Next
        Finally
            gridEvidenta.EndUpdate()
        End Try
    End Sub

    ' The order of payment: only the value and the explanation follow the ticks; the note, the date and the number the operator chose stay. The row exists only while a
    ' payment template is chosen and something is to be paid.
    Private Sub FillPayment(k_r As EFacturaContariRaspuns)
        Dim k_p As EFacturaContariPlata = k_r.Plata
        gridPlata.BeginUpdate()
        Try
            gridPlata.ClearRows()
            If k_p.Activa Then
                _nrOpProvisional = k_p.NumarOPProvizoriu
                If k_p.NumarOP.HasValue Then _nrOp = k_p.NumarOP.Value.ToString(CultureInfo.InvariantCulture)   ' slice 04EF-01: the server's number, read again on every calculation (not typed)
                Dim k_row As KBotDataRow = gridPlata.AddRow()
                k_row("nc") = _nota
                k_row("data") = If(k_p.DataMinima.HasValue, k_p.DataMinima.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_row("nrop") = _nrOp
                k_row("explicatie") = Explanation(k_r)
                k_row("lungime") = Math.Min(Convert.ToString(k_row("explicatie"), CultureInfo.InvariantCulture).Length, OpExplicatie.MaxLength)
                k_row("valoare") = k_p.Valoare
            End If
        Finally
            gridPlata.EndUpdate()
        End Try
        gridPlata.Enabled = k_p.Activa
        _explicatieLen = If(k_p.Activa, Convert.ToString(gridPlata.Rows(0)("explicatie"), CultureInfo.InvariantCulture).Length, 0)
    End Sub

    ' «F <numbers of the invoices paid> C <client code> <kind of invoice>», ASCII, no comma, at most 75 characters.
    Private Function Explanation(k_r As EFacturaContariRaspuns) As String
        Dim k_cut As Boolean
        Dim k_numbers As IEnumerable(Of String) = k_r.Evidenta.Where(Function(k_e) k_e.Platit <> 0D).Select(Function(k_e) k_e.NumarDocument)
        Return OpExplicatie.Compose(k_numbers, txtCodClient.Text, txtFelOp.Text, k_cut)
    End Function

    ' The length of the explanation: the progress column runs from 0 to 75 characters. It follows the text live (while typing, and when the client code or the kind of invoice
    ' recompose it). Cells are set under _filling, so no change event comes out of it.
    Private Sub UpdateBar(k_length As Integer)
        If gridPlata.Rows.Count = 0 Then Return
        Dim k_was As Boolean = _filling
        _filling = True
        Try
            gridPlata.Rows(0)("lungime") = Math.Min(k_length, OpExplicatie.MaxLength)
        Finally
            _filling = k_was
        End Try
        gridPlata.Invalidate()
    End Sub

    Private Shared Function SortedWarnings(k_r As EFacturaContariRaspuns) As List(Of EFacturaContariAvertisment)
        Dim k_order As New Dictionary(Of String, Integer)(StringComparer.Ordinal) From {{"eroare", 0}, {"confirmare", 1}, {"atentie", 2}, {"info", 3}}
        Return k_r.Avertismente.OrderBy(Function(k_x) If(k_order.ContainsKey(k_x.Grav), k_order(k_x.Grav), 9)).ToList()
    End Function

    ' Everything the footer line leaves out: the account and codes, every warning, what the templates would book, the next note number.
    Private Shared Function DetailsText(k_r As EFacturaContariRaspuns) As String
        Dim k_lines As New List(Of String)()
        If Not String.IsNullOrWhiteSpace(k_r.CodAng) Then
            k_lines.Add("Angajament: " & k_r.CodAng & If(String.IsNullOrWhiteSpace(k_r.CodInd), String.Empty, " / Indicator: " & k_r.CodInd))
        End If
        If k_r.NotaUrmatoare > 0 Then k_lines.Add("Nota contabilă: numărul următor este " & k_r.NotaUrmatoare.ToString(CultureInfo.InvariantCulture))
        For Each k_a As EFacturaContariAvertisment In SortedWarnings(k_r)
            k_lines.Add("• " & k_a.Text)
        Next
        If k_r.Operatii.Count > 0 Then
            k_lines.Add(String.Empty)
            k_lines.Add("Operațiile care se înregistrează la «Salvează»:")
            For Each k_op As EFacturaContariOperatie In k_r.Operatii
                k_lines.Add($"{k_op.Titlu}  ·  {k_op.Suma.ToString("N2", _ro)}")
                For Each k_l As EFacturaContariLinie In k_op.Linii
                    k_lines.Add($"   D {k_l.Debit}" & If(String.IsNullOrEmpty(k_l.PartenerDebit), String.Empty, $" ({k_l.PartenerDebit})") &
                                $"  =  C {k_l.Credit}" & If(String.IsNullOrEmpty(k_l.PartenerCredit), String.Empty, $" ({k_l.PartenerCredit})"))
                Next
            Next
        End If
        Return String.Join(vbLf, k_lines)
    End Function

    ' ── The operator's choices ──────────────────────────────────────────────────

    Private Sub CmbClsf_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbClsf.SelectedIndexChanged
        Try
            If _filling OrElse cmbClsf.SelectedIndex < 0 Then Return
            Dim k_leaf As KBotComboGroupedItem = DirectCast(cmbClsf.SelectedItem, KBotComboGroupedItem)
            RaiseEvent ClasificatieAleasa(CInt(k_leaf.Tag))
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.CmbClsf_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' A click on a template chooses it; a click on the chosen payment template takes it off (without one there is no order of payment).
    Private Sub LstEvidentiere_CellClick(sender As Object, e As KBotCellEventArgs) Handles lstEvidentiere.CellClick
        Try
            If _booked Then
                lstEvidentiere.CurrentRowIndex = _bookedRowE   ' slice 03EF-13: read only, the template used stays chosen
                Return
            End If
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= lstEvidentiere.Rows.Count Then Return
            Dim k_template As EFacturaContariSablon = TryCast(lstEvidentiere.Rows(e.RowIndex).Tag, EFacturaContariSablon)
            If k_template Is Nothing Then Return
            ' Slice 04EF-01: a click on the chosen evidence template takes it off (payment only), like the payment template; a click on another one chooses it.
            Dim k_same As Boolean = _ultim IsNot Nothing AndAlso _ultim.IdSablonE.HasValue AndAlso _ultim.IdSablonE.Value = k_template.IdSablon
            RaiseEvent SablonAles("E", If(k_same, 0, k_template.IdSablon))
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.LstEvidentiere_CellClick", ex)
        End Try
    End Sub

    Private Sub LstPlata_CellClick(sender As Object, e As KBotCellEventArgs) Handles lstPlata.CellClick
        Try
            If _booked Then
                lstPlata.CurrentRowIndex = _bookedRowP
                Return
            End If
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= lstPlata.Rows.Count Then Return
            Dim k_template As EFacturaContariSablon = TryCast(lstPlata.Rows(e.RowIndex).Tag, EFacturaContariSablon)
            If k_template Is Nothing Then Return
            Dim k_same As Boolean = _ultim IsNot Nothing AndAlso _ultim.IdSablonP.HasValue AndAlso _ultim.IdSablonP.Value = k_template.IdSablon
            RaiseEvent SablonAles("P", If(k_same, 0, k_template.IdSablon))
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.LstPlata_CellClick", ex)
        End Try
    End Sub

    ' The client code and the kind of invoice end up in the explanation of the order: it follows them (and the text is kept ASCII, without commas).
    Private Sub TxtCodClient_TextChanged(sender As Object, e As EventArgs) Handles txtCodClient.TextChanged
        Try
            If _filling Then Return
            CleanBox(txtCodClient)
            RecomposeExplanation()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.TxtCodClient_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtFelOp_TextChanged(sender As Object, e As EventArgs) Handles txtFelOp.TextChanged
        Try
            If _filling Then Return
            CleanBox(txtFelOp)
            RecomposeExplanation()
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.TxtFelOp_TextChanged", ex)
        End Try
    End Sub

    Private Sub CleanBox(k_box As KBotTextBox)
        Dim k_clean As String = OpExplicatie.Clean(k_box.Text)
        If Not String.Equals(k_clean, k_box.Text, StringComparison.Ordinal) Then k_box.Text = k_clean
    End Sub

    Private Sub RecomposeExplanation()
        If _ultim Is Nothing OrElse gridPlata.Rows.Count = 0 Then Return
        _filling = True
        Try
            Dim k_text As String = Explanation(_ultim)
            gridPlata.Rows(0)("explicatie") = k_text
            _explicatieLen = k_text.Length
            UpdateBar(_explicatieLen)
            UpdateSave()
        Finally
            _filling = False
        End Try
    End Sub

    ' The notes: the combo of a cell offers the existing numbers of the subunit and year; a new number may be typed.
    Private Sub GridEvidenta_ComboItemsRequested(sender As Object, e As KBotComboItemsEventArgs) Handles gridEvidenta.ComboItemsRequested
        e.Items = _noteList
    End Sub

    Private Sub GridPlata_ComboItemsRequested(sender As Object, e As KBotComboItemsEventArgs) Handles gridPlata.ComboItemsRequested
        e.Items = _noteList
    End Sub

    ' What the operator typed in a cell is made valid before it is written: the note and the order number are whole numbers, the explanation is ASCII without commas.
    Private Sub GridEvidenta_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridEvidenta.CellValidating
        Try
            If String.Equals(e.ColumnKey, "nc", StringComparison.Ordinal) Then e.ProposedValue = Digits(e.ProposedValue)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.GridEvidenta_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridPlata_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridPlata.CellValidating
        Try
            Select Case e.ColumnKey
                Case "nc"
                    e.ProposedValue = Digits(e.ProposedValue)
                Case "explicatie"
                    e.ProposedValue = OpExplicatie.Fit(Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.GridPlata_CellValidating", ex)
        End Try
    End Sub

    Private Shared Function Digits(k_value As Object) As String
        Dim k_text As String = Regex.Replace(Convert.ToString(k_value, CultureInfo.InvariantCulture), "\D", String.Empty)
        Return k_text.TrimStart("0"c)
    End Function

    ' While the explanation is typed: diacritics become plain letters, commas go, nothing goes past 75 characters, and the bar follows the length live.
    Private Sub GridPlata_EditorTextChanged(sender As Object, e As KBotEditorTextEventArgs) Handles gridPlata.EditorTextChanged
        Try
            Select Case e.ColumnKey
                Case "explicatie"
                    Dim k_text As String = OpExplicatie.Clean(e.Text)
                    If k_text.Length > OpExplicatie.MaxLength Then k_text = k_text.Substring(0, OpExplicatie.MaxLength)
                    e.Text = k_text
                    _explicatieLen = k_text.Trim().Length
                    UpdateBar(_explicatieLen)
                    UpdateSave()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.GridPlata_EditorTextChanged", ex)
        End Try
    End Sub

    Private Sub GridEvidenta_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridEvidenta.CellValueChanged
        Try
            If _filling Then Return
            If String.Equals(e.ColumnKey, "nc", StringComparison.Ordinal) Then
                ShareNote(Convert.ToString(e.NewValue, CultureInfo.InvariantCulture))
                Return
            End If
            If Not String.Equals(e.ColumnKey, "platit", StringComparison.Ordinal) Then Return
            Dim k_rand As EFacturaContariRand = TryCast(gridEvidenta.Rows(e.RowIndex).Tag, EFacturaContariRand)
            If k_rand Is Nothing Then Return
            Dim k_value As Decimal
            If Not Decimal.TryParse(Convert.ToString(e.NewValue, CultureInfo.InvariantCulture), NumberStyles.Number, CultureInfo.InvariantCulture, k_value) AndAlso
               Not Decimal.TryParse(Convert.ToString(e.NewValue, CultureInfo.CurrentCulture), NumberStyles.Number, CultureInfo.CurrentCulture, k_value) Then Return
            RaiseEvent PlatitSchimbat(k_rand.IdPrimita, k_value)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.GridEvidenta_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridPlata_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridPlata.CellValueChanged
        Try
            If _filling Then Return
            Select Case e.ColumnKey
                Case "nc"
                    ShareNote(Convert.ToString(e.NewValue, CultureInfo.InvariantCulture))
                Case "explicatie"
                    _explicatieLen = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture).Length
                    UpdateBar(_explicatieLen)
                    UpdateSave()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.GridPlata_CellValueChanged", ex)
        End Try
    End Sub

    ' One accounting note for the whole save: the NC column of every row (evidence and payment) shows the same note.
    Private Sub ShareNote(k_note As String)
        _nota = k_note
        _filling = True
        Try
            For Each k_row As KBotDataRow In gridEvidenta.Rows
                k_row("nc") = _nota
            Next
            For Each k_row As KBotDataRow In gridPlata.Rows
                k_row("nc") = _nota
            Next
        Finally
            _filling = False
        End Try
        gridEvidenta.Invalidate()
        gridPlata.Invalidate()
        UpdateSave()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.PanelBack()
            'For Each k_header As KBotLabel In New KBotLabel() {lblClsf, lblCodClient, lblIban, lblFelOp}
            '    k_header.BackColor = k_scheme.Palette.ButtonBackColor
            '    k_header.ForeColor = k_scheme.Palette.ButtonTextColor
            'Next
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiCumparareContarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
