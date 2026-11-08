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
''' payment (Access EF_P: one row, awake ONLY while a payment template is chosen), with the length of its explanation under it; «Renunță» and «Salvează» at the foot.
''' The page only OWNS its controls and shows what <see cref="PrimiteView"/> hands it (the server's calculation); it asks the view, through events, to calculate again.
''' The text of the order of payment is ASCII without commas (<see cref="OpExplicatie"/>).
''' </summary>
Public Class PrimiteContarePage
    Implements IThemedContainer

    ''' <summary>The operator chose a classification.</summary>
    Public Event ClasificatieAleasa(k_idClsf As Integer)

    ''' <summary>The operator chose a template: <paramref name="k_fel"/> «E» (evidence) or «P» (payment); <paramref name="k_idSablon"/> 0 = the payment template was taken off.</summary>
    Public Event SablonAles(k_fel As String, k_idSablon As Integer)

    ''' <summary>The operator changed the value to pay of an invoice.</summary>
    Public Event PlatitSchimbat(k_idPrimita As Integer, k_valoare As Decimal)

    ''' <summary>«Renunță»: the host takes the ticks off.</summary>
    Public Event RenuntaCerut()

    ''' <summary>«Salvează»: the host confirms the warnings and asks the server to book.</summary>
    Public Event SalveazaCerut()

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
    Private _explicatieLen As Integer

    Public Sub New()
        InitializeComponent()
        gridEvidenta.ReadOnlyGrid = False
        gridPlata.ReadOnlyGrid = False
        ShowEmpty("Clic dreapta pe o factură și alegeți «Adaugă operații contabile».")
    End Sub

    ''' <summary>Nothing is ticked: the page is emptied and says <paramref name="k_text"/>.</summary>
    Public Sub ShowEmpty(k_text As String)
        _filling = True
        Try
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
            cmbClsf.Enabled = False
            txtIban.Text = String.Empty
            txtCodClient.Text = String.Empty
            txtFelOp.Text = String.Empty
            txtCodClient.Enabled = False
            txtFelOp.Enabled = False
            lblInfo.Html = WebUtility.HtmlEncode(k_text)
            tips.SetToolTipText(lblInfo, String.Empty)
            _canSave = False
            UpdateSave()
            UpdateBar(0, False)
        Finally
            _filling = False
        End Try
    End Sub

    ''' <summary>Puts the server's calculation on screen.</summary>
    Public Sub ShowResult(k_r As EFacturaContariRaspuns)
        ArgumentNullException.ThrowIfNull(k_r)
        _filling = True
        Try
            ' Another supplier: what was typed for the previous one (client code, note, order number, kind of invoice) goes.
            If Not String.Equals(k_r.FurnizorCui, _supplierCui, StringComparison.Ordinal) Then
                _supplierCui = k_r.FurnizorCui
                _nota = String.Empty
                _nrOp = String.Empty
                _felOpFor = Nothing
                txtCodClient.Text = String.Empty
            End If
            _ultim = k_r
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
            lblInfo.Html = FooterHtml(k_r)
            tips.SetToolTipHeader(lblInfo, "Detalii")
            tips.SetToolTipText(lblInfo, DetailsText(k_r))
            _canSave = CanSave(k_r)
            UpdateSave()
        Finally
            _filling = False
        End Try
    End Sub

    ' The evidence can be booked: the role allows it, the classification and the evidence template are chosen, nothing blocks and something is left to book.
    Private Shared Function CanSave(k_r As EFacturaContariRaspuns) As Boolean
        Return k_r.PoateScrie AndAlso k_r.Clasificatie IsNot Nothing AndAlso k_r.IdSablonE.HasValue AndAlso
               Not k_r.Avertismente.Any(Function(k_a) String.Equals(k_a.Grav, "eroare", StringComparison.Ordinal)) AndAlso
               k_r.Evidenta.Any(Function(k_e) k_e.Suma <> 0D)
    End Function

    ' «Salvează» also needs a valid explanation while an order of payment is being made.
    Private Sub UpdateSave()
        btnSalveaza.Enabled = _canSave AndAlso (gridPlata.Rows.Count = 0 OrElse _explicatieLen >= OpExplicatie.MinLength)
    End Sub

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
                If _nrOp.Length = 0 AndAlso k_p.NumarOP.HasValue Then _nrOp = k_p.NumarOP.Value.ToString(CultureInfo.InvariantCulture)
                Dim k_row As KBotDataRow = gridPlata.AddRow()
                k_row("nc") = _nota
                k_row("data") = If(k_p.DataMinima.HasValue, k_p.DataMinima.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_row("nrop") = _nrOp
                k_row("explicatie") = Explanation(k_r)
                k_row("valoare") = k_p.Valoare
            End If
        Finally
            gridPlata.EndUpdate()
        End Try
        gridPlata.Enabled = k_p.Activa
        If k_p.Activa Then
            _explicatieLen = Convert.ToString(gridPlata.Rows(0)("explicatie"), CultureInfo.InvariantCulture).Length
            UpdateBar(_explicatieLen, True)
        Else
            _explicatieLen = 0
            UpdateBar(0, False)
        End If
    End Sub

    ' «F <numbers of the invoices paid> C <client code> <kind of invoice>», ASCII, no comma, at most 75 characters.
    Private Function Explanation(k_r As EFacturaContariRaspuns) As String
        Dim k_cut As Boolean
        Dim k_numbers As IEnumerable(Of String) = k_r.Evidenta.Where(Function(k_e) k_e.Platit <> 0D).Select(Function(k_e) k_e.NumarDocument)
        Return OpExplicatie.Compose(k_numbers, txtCodClient.Text, txtFelOp.Text, k_cut)
    End Function

    ' The length of the explanation: the bar runs from 1 to 75 characters; red when empty, amber near the limit.
    Private Sub UpdateBar(k_length As Integer, k_active As Boolean)
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        barExplicatie.Maximum = OpExplicatie.MaxLength
        barExplicatie.Value = If(k_active, Math.Min(k_length, OpExplicatie.MaxLength), 0)
        If Not k_active Then
            lblLungime.Text = String.Empty
            barExplicatie.BarColor = Color.Empty
            Return
        End If
        lblLungime.Text = $"{k_length.ToString(CultureInfo.InvariantCulture)} / {OpExplicatie.MaxLength.ToString(CultureInfo.InvariantCulture)}"
        barExplicatie.BarColor = If(k_length < OpExplicatie.MinLength, k_palette.ErrorColor,
                                    If(k_length >= OpExplicatie.MaxLength - 5, k_palette.WarningColor, k_palette.SuccessColor))
    End Sub

    ' The supplier and the most serious warning (errors first); the rest, and the bookings, are in the tooltip.
    Private Shared Function FooterHtml(k_r As EFacturaContariRaspuns) As String
        Dim k_html As New StringBuilder()
        k_html.Append("<b>").Append(WebUtility.HtmlEncode(If(String.IsNullOrWhiteSpace(k_r.FurnizorDenumire), "Furnizor necunoscut", k_r.FurnizorDenumire))).Append("</b>")
        Dim k_sorted As List(Of EFacturaContariAvertisment) = SortedWarnings(k_r)
        If k_sorted.Count > 0 Then
            Dim k_text As String = WebUtility.HtmlEncode(k_sorted(0).Text)
            k_html.Append("  ·  ").Append(If(String.Equals(k_sorted(0).Grav, "eroare", StringComparison.Ordinal), "<b>" & k_text & "</b>", k_text))
            If k_sorted.Count > 1 Then k_html.Append($"  (+{(k_sorted.Count - 1).ToString(CultureInfo.InvariantCulture)})")
        End If
        Return k_html.ToString()
    End Function

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
            GlobalErrorLog.Write("PrimiteContarePage.CmbClsf_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' A click on a template chooses it; a click on the chosen payment template takes it off (without one there is no order of payment).
    Private Sub LstEvidentiere_CellClick(sender As Object, e As KBotCellEventArgs) Handles lstEvidentiere.CellClick
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= lstEvidentiere.Rows.Count Then Return
            Dim k_template As EFacturaContariSablon = TryCast(lstEvidentiere.Rows(e.RowIndex).Tag, EFacturaContariSablon)
            If k_template Is Nothing OrElse (_ultim IsNot Nothing AndAlso _ultim.IdSablonE.HasValue AndAlso _ultim.IdSablonE.Value = k_template.IdSablon) Then Return
            RaiseEvent SablonAles("E", k_template.IdSablon)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.LstEvidentiere_CellClick", ex)
        End Try
    End Sub

    Private Sub LstPlata_CellClick(sender As Object, e As KBotCellEventArgs) Handles lstPlata.CellClick
        Try
            If _filling OrElse e.RowIndex < 0 OrElse e.RowIndex >= lstPlata.Rows.Count Then Return
            Dim k_template As EFacturaContariSablon = TryCast(lstPlata.Rows(e.RowIndex).Tag, EFacturaContariSablon)
            If k_template Is Nothing Then Return
            Dim k_same As Boolean = _ultim IsNot Nothing AndAlso _ultim.IdSablonP.HasValue AndAlso _ultim.IdSablonP.Value = k_template.IdSablon
            RaiseEvent SablonAles("P", If(k_same, 0, k_template.IdSablon))
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.LstPlata_CellClick", ex)
        End Try
    End Sub

    ' The client code and the kind of invoice end up in the explanation of the order: it follows them (and the text is kept ASCII, without commas).
    Private Sub TxtCodClient_TextChanged(sender As Object, e As EventArgs) Handles txtCodClient.TextChanged
        Try
            If _filling Then Return
            CleanBox(txtCodClient)
            RecomposeExplanation()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.TxtCodClient_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtFelOp_TextChanged(sender As Object, e As EventArgs) Handles txtFelOp.TextChanged
        Try
            If _filling Then Return
            CleanBox(txtFelOp)
            RecomposeExplanation()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.TxtFelOp_TextChanged", ex)
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
            UpdateBar(_explicatieLen, True)
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
            GlobalErrorLog.Write("PrimiteContarePage.GridEvidenta_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridPlata_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridPlata.CellValidating
        Try
            Select Case e.ColumnKey
                Case "nc", "nrop"
                    e.ProposedValue = Digits(e.ProposedValue)
                Case "explicatie"
                    e.ProposedValue = OpExplicatie.Fit(Convert.ToString(e.ProposedValue, CultureInfo.InvariantCulture))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.GridPlata_CellValidating", ex)
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
                    UpdateBar(_explicatieLen, True)
                Case "nrop"
                    e.Text = Regex.Replace(e.Text, "\D", String.Empty)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.GridPlata_EditorTextChanged", ex)
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
            GlobalErrorLog.Write("PrimiteContarePage.GridEvidenta_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GridPlata_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridPlata.CellValueChanged
        Try
            If _filling Then Return
            Select Case e.ColumnKey
                Case "nc"
                    ShareNote(Convert.ToString(e.NewValue, CultureInfo.InvariantCulture))
                Case "nrop"
                    _nrOp = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture)
                Case "explicatie"
                    _explicatieLen = Convert.ToString(e.NewValue, CultureInfo.InvariantCulture).Length
                    UpdateBar(_explicatieLen, True)
                    UpdateSave()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.GridPlata_CellValueChanged", ex)
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
    End Sub

    Private Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            RaiseEvent SalveazaCerut()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.BtnSalveaza_Click", ex)
        End Try
    End Sub

    Private Sub BtnRenunta_Click(sender As Object, e As EventArgs) Handles btnRenunta.Click
        Try
            RaiseEvent RenuntaCerut()
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.BtnRenunta_Click", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.PanelBack()
            'For Each k_header As KBotLabel In New KBotLabel() {lblClsf, lblCodClient, lblIban, lblFelOp}
            '    k_header.BackColor = k_scheme.Palette.ButtonBackColor
            '    k_header.ForeColor = k_scheme.Palette.ButtonTextColor
            'Next
            UpdateBar(_explicatieLen, gridPlata.Rows.Count > 0)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
