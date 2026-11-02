Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Net
Imports System.Text
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 01EF-02 -- the «Contari» page, the FIRST view of the received invoices (window with all the invoices only): the classification and the two
''' templates (evidence, payment) on top, then the supplier's data and the warnings, the evidence grid (Access EF_F: one row per ticked invoice, only «De plătit»
''' is edited), the payment grid (Access EF_P) and the bookings the templates would produce. The page only OWNS its controls and shows what
''' <see cref="PrimiteView"/> hands it (the server's calculation); it asks the view, through events, to calculate again. Nothing is saved yet (01EF-04/05).
''' </summary>
Public Class PrimiteContarePage
    Implements IThemedContainer

    ''' <summary>The operator chose a classification.</summary>
    Public Event ClasificatieAleasa(k_idClsf As Integer)

    ''' <summary>The operator chose a template: <paramref name="k_fel"/> «E» (evidence) or «P» (payment).</summary>
    Public Event SablonAles(k_fel As String, k_idSablon As Integer)

    ''' <summary>The operator changed the value to pay of an invoice.</summary>
    Public Event PlatitSchimbat(k_idPrimita As Integer, k_valoare As Decimal)

    ''' <summary>«Renunță»: the host takes the ticks off.</summary>
    Public Event RenuntaCerut()

    ''' <summary>«Salvează evidențierea»: the host confirms the warnings and asks the server to book.</summary>
    Public Event SalveazaCerut()

    ' An item of a combo: the id and what it shows.
    Private NotInheritable Class Optiune
        Public ReadOnly Property Id As Integer
        Public ReadOnly Property Text As String

        Public Sub New(k_id As Integer, k_text As String)
            Id = k_id
            Text = k_text
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    Private _filling As Boolean
    Private _clsfSignature As String = String.Empty

    Public Sub New()
        InitializeComponent()
        cmbClsf.CaptionSelector = Function(k_o As Object) k_o.ToString()
        cmbSablonE.CaptionSelector = Function(k_o As Object) k_o.ToString()
        cmbSablonP.CaptionSelector = Function(k_o As Object) k_o.ToString()
        gridEvidenta.ReadOnlyGrid = False
        ShowEmpty("Bifați în arbore facturile aceluiași furnizor.")
    End Sub

    ''' <summary>Nothing is ticked: the page is emptied and says <paramref name="k_text"/>.</summary>
    Public Sub ShowEmpty(k_text As String)
        _filling = True
        Try
            gridEvidenta.ClearRows()
            gridPlata.ClearRows()
            cmbClsf.Items.Clear()
            cmbSablonE.Items.Clear()
            cmbSablonP.Items.Clear()
            _clsfSignature = String.Empty
            cmbClsf.Enabled = False
            cmbSablonE.Enabled = False
            cmbSablonP.Enabled = False
            cmbNota.Items.Clear()
            cmbNota.Enabled = False
            lblInfo.Html = WebUtility.HtmlEncode(k_text)
            lblOperatii.Html = String.Empty
            btnSalveaza.Enabled = False
        Finally
            _filling = False
        End Try
    End Sub

    ''' <summary>Puts the server's calculation on screen.</summary>
    Public Sub ShowResult(k_r As EFacturaContariRaspuns)
        ArgumentNullException.ThrowIfNull(k_r)
        _filling = True
        Try
            FillClassifications(k_r)
            FillTemplates(cmbSablonE, k_r.SabloaneEvidentiere, k_r.IdSablonE, k_r.Clasificatie IsNot Nothing)
            FillTemplates(cmbSablonP, k_r.SabloanePlata, k_r.IdSablonP, k_r.Clasificatie IsNot Nothing)
            FillNotes(k_r)
            FillInvoices(k_r)
            FillPayment(k_r.Plata)
            lblInfo.Html = InfoHtml(k_r)
            lblOperatii.Html = OperationsHtml(k_r)
            btnSalveaza.Enabled = CanSave(k_r)
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

    ' The classifications change only with the supplier: the list is rebuilt when its content changed, otherwise only the selection follows.
    Private Sub FillClassifications(k_r As EFacturaContariRaspuns)
        Dim k_signature As String = String.Join(",", k_r.Clasificatii.Select(Function(k_c) k_c.IdClsf.ToString(CultureInfo.InvariantCulture)))
        cmbClsf.Enabled = True
        If Not String.Equals(k_signature, _clsfSignature, StringComparison.Ordinal) Then
            _clsfSignature = k_signature
            cmbClsf.BeginUpdate()
            Try
                cmbClsf.Items.Clear()
                For Each k_c As EFacturaContariClsf In k_r.Clasificatii.OrderByDescending(Function(k_x) k_x.AlePartenerului)
                    cmbClsf.Items.Add(New Optiune(k_c.IdClsf, $"{k_c.Clsf}  {k_c.Denumire}"))
                Next
            Finally
                cmbClsf.EndUpdate()
            End Try
        End If
        cmbClsf.SelectedIndex = -1
        If k_r.Clasificatie IsNot Nothing Then
            For k_i As Integer = 0 To cmbClsf.Items.Count - 1
                If DirectCast(cmbClsf.Items(k_i), Optiune).Id = k_r.Clasificatie.IdClsf Then
                    cmbClsf.SelectedIndex = k_i
                    Exit For
                End If
            Next
        End If
    End Sub

    Private Shared Sub FillTemplates(k_combo As KBotComboBox, k_list As List(Of EFacturaContariSablon), k_chosen As Integer?, k_enabled As Boolean)
        k_combo.Items.Clear()
        k_combo.Enabled = k_enabled
        k_combo.SelectedIndex = -1
        For Each k_s As EFacturaContariSablon In k_list
            k_combo.Items.Add(New Optiune(k_s.IdSablon, $"{k_s.Explicatie}  [{k_s.Jurnal}]"))
        Next
        If Not k_chosen.HasValue Then Return
        For k_i As Integer = 0 To k_combo.Items.Count - 1
            If DirectCast(k_combo.Items(k_i), Optiune).Id = k_chosen.Value Then
                k_combo.SelectedIndex = k_i
                Return
            End If
        Next
    End Sub

    ' The notes of the subunit and year; what the operator chose or typed stays (an existing number is selected again, a typed one is written back).
    Private Sub FillNotes(k_r As EFacturaContariRaspuns)
        Dim k_chosen As Integer? = If(cmbNota.SelectedIndex >= 0, CType(DirectCast(cmbNota.SelectedItem, Optiune).Id, Integer?), Nothing)
        Dim k_typed As String = If(cmbNota.SelectedIndex < 0, cmbNota.Text, String.Empty)
        cmbNota.Items.Clear()
        cmbNota.Enabled = k_r.Subunitate IsNot Nothing
        cmbNota.PlaceholderText = If(k_r.NotaUrmatoare > 0, $"Nouă (numărul următor: {k_r.NotaUrmatoare.ToString(CultureInfo.InvariantCulture)})", "Nouă")
        For Each k_n As EFacturaContariNota In k_r.Note
            cmbNota.Items.Add(New Optiune(k_n.NumarNC, $"{k_n.NumarNC.ToString(CultureInfo.InvariantCulture)} · " &
                              If(k_n.DataNc.HasValue, k_n.DataNc.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "fără dată") & $" · {k_n.Documente.ToString(CultureInfo.InvariantCulture)} documente"))
        Next
        cmbNota.SelectedIndex = -1
        If k_chosen.HasValue Then
            For k_i As Integer = 0 To cmbNota.Items.Count - 1
                If DirectCast(cmbNota.Items(k_i), Optiune).Id = k_chosen.Value Then
                    cmbNota.SelectedIndex = k_i
                    Exit For
                End If
            Next
        ElseIf k_typed.Length > 0 Then
            cmbNota.Text = k_typed
        End If
    End Sub

    ''' <summary>The note the operator chose (an item) or typed (a number); Nothing = none (a new note with the next number). <paramref name="k_valid"/> is False when the text is not a positive whole number.</summary>
    Public Function NotaAleasa(ByRef k_valid As Boolean) As Integer?
        k_valid = True
        If cmbNota.SelectedIndex >= 0 Then Return DirectCast(cmbNota.SelectedItem, Optiune).Id
        Dim k_text As String = cmbNota.Text.Trim()
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
                k_row("doc") = If(String.Equals(k_e.NumarDocument, k_e.NrFact, StringComparison.Ordinal), k_e.NrFact, $"{k_e.NumarDocument} ({k_e.NrFact})")
                k_row("data") = If(k_e.DataDocument.HasValue, k_e.DataDocument.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
                k_row("fel") = k_e.FelDocument
                k_row("total") = k_e.Total
                k_row("salvat") = k_e.Salvat
                k_row("suma") = k_e.Suma
                k_row("platit") = k_e.Platit
            Next
        Finally
            gridEvidenta.EndUpdate()
        End Try
    End Sub

    Private Sub FillPayment(k_p As EFacturaContariPlata)
        gridPlata.BeginUpdate()
        Try
            gridPlata.ClearRows()
            If Not k_p.Activa Then Return
            Dim k_row As KBotDataRow = gridPlata.AddRow()
            k_row("explicatie") = k_p.ExplicatieOP
            k_row("valoare") = k_p.Valoare
            k_row("dataMin") = If(k_p.DataMinima.HasValue, k_p.DataMinima.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), String.Empty)
        Finally
            gridPlata.EndUpdate()
        End Try
    End Sub

    ' The supplier, its account and codes, then the warnings (errors first).
    Private Shared Function InfoHtml(k_r As EFacturaContariRaspuns) As String
        Dim k_html As New StringBuilder()
        k_html.Append("<b>").Append(WebUtility.HtmlEncode(If(String.IsNullOrWhiteSpace(k_r.FurnizorDenumire), "Furnizor necunoscut", k_r.FurnizorDenumire))).Append("</b>")
        If Not String.IsNullOrWhiteSpace(k_r.ContIban) Then k_html.Append("  ·  Cont: ").Append(WebUtility.HtmlEncode(k_r.ContIban))
        If Not String.IsNullOrWhiteSpace(k_r.CodAng) Then
            k_html.Append("  ·  Angajament: ").Append(WebUtility.HtmlEncode(k_r.CodAng))
            If Not String.IsNullOrWhiteSpace(k_r.CodInd) Then k_html.Append(" / Indicator: ").Append(WebUtility.HtmlEncode(k_r.CodInd))
        End If
        Dim k_order As New Dictionary(Of String, Integer)(StringComparer.Ordinal) From {{"eroare", 0}, {"confirmare", 1}, {"atentie", 2}, {"info", 3}}
        For Each k_a As EFacturaContariAvertisment In k_r.Avertismente.OrderBy(Function(k_x) If(k_order.ContainsKey(k_x.Grav), k_order(k_x.Grav), 9))
            Dim k_text As String = WebUtility.HtmlEncode(k_a.Text)
            k_html.Append("<br>• ").Append(If(String.Equals(k_a.Grav, "eroare", StringComparison.Ordinal), "<b>" & k_text & "</b>", k_text))
        Next
        Return k_html.ToString()
    End Function

    ' What each chosen template would book: title and amount, then its debit / credit lines.
    Private Shared Function OperationsHtml(k_r As EFacturaContariRaspuns) As String
        If k_r.Operatii.Count = 0 Then Return String.Empty
        Dim k_html As New StringBuilder("<b>Operațiile care se înregistrează la «Salvează evidențierea»</b>")
        For Each k_op As EFacturaContariOperatie In k_r.Operatii
            k_html.Append("<br><b>").Append(WebUtility.HtmlEncode(k_op.Titlu)).Append("</b>  ·  ").Append(k_op.Suma.ToString("N2", _ro))
            For Each k_l As EFacturaContariLinie In k_op.Linii
                k_html.Append("<br>&nbsp;&nbsp;D ").Append(WebUtility.HtmlEncode(k_l.Debit))
                If Not String.IsNullOrEmpty(k_l.PartenerDebit) Then k_html.Append(" (").Append(WebUtility.HtmlEncode(k_l.PartenerDebit)).Append(")")
                k_html.Append("  =  C ").Append(WebUtility.HtmlEncode(k_l.Credit))
                If Not String.IsNullOrEmpty(k_l.PartenerCredit) Then k_html.Append(" (").Append(WebUtility.HtmlEncode(k_l.PartenerCredit)).Append(")")
                Dim k_flags As New List(Of String)()
                If k_l.PA Then k_flags.Add("propunere + angajament")
                If k_l.Ordonantare Then k_flags.Add("ordonanțare")
                If k_l.Chelt Then k_flags.Add("cheltuială efectivă")
                If k_flags.Count > 0 Then k_html.Append("  [").Append(String.Join(", ", k_flags)).Append("]")
            Next
        Next
        Return k_html.ToString()
    End Function

    ' ── The operator's choices ──────────────────────────────────────────────────

    Private Sub CmbClsf_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbClsf.SelectedIndexChanged
        Try
            If _filling OrElse cmbClsf.SelectedIndex < 0 Then Return
            RaiseEvent ClasificatieAleasa(DirectCast(cmbClsf.SelectedItem, Optiune).Id)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.CmbClsf_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub CmbSablonE_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSablonE.SelectedIndexChanged
        Try
            If _filling OrElse cmbSablonE.SelectedIndex < 0 Then Return
            RaiseEvent SablonAles("E", DirectCast(cmbSablonE.SelectedItem, Optiune).Id)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.CmbSablonE_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub CmbSablonP_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSablonP.SelectedIndexChanged
        Try
            If _filling OrElse cmbSablonP.SelectedIndex < 0 Then Return
            RaiseEvent SablonAles("P", DirectCast(cmbSablonP.SelectedItem, Optiune).Id)
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.CmbSablonP_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub GridEvidenta_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridEvidenta.CellValueChanged
        Try
            If _filling OrElse Not String.Equals(e.ColumnKey, "platit", StringComparison.Ordinal) Then Return
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
            BackColor = k_scheme.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("PrimiteContarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
