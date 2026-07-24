Option Strict On
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Forexe          ' PdfXmlExtractor
Imports KBot.Theming

''' <summary>
''' Previzualizare IMPLICITĂ a documentului DDF (felia 0020-03, opțiunea C a planului §6):
''' redă din XML-ul XFA o suprafață WinForms tematizată, read-only — antetul, apoi tabelul
''' secțiunii A, apoi nota. NU convertește PDF-ul și NU deschide alt proces: e singura cale
''' care merge fără o dependență externă (planul §2.10 arată că aplatizarea unui XFA într-un
''' PDF static nu ne e disponibilă gratuit).
'''
''' Sursa XML-ului, încercată în ordine (planul §6):
'''   1. siblingul .xml scris de mdl_FX_DDF_PDF lângă fiecare PDF generat — citit dacă există;
'''   2. altfel PdfXmlExtractor pe PDF (gestionează deja cazurile brut și FlateDecode).
'''
''' NU este un randor XFA general: forma e cunoscută fiindcă noi o producem (felia 05).
''' </summary>
Public Class XfaXmlPreview
    Implements IDdfPreview, IThemedControl

    Private Const COL_ELEMENT As String = "element"
    Private Const COL_CLSF As String = "clsf"
    Private Const COL_VALPREC As String = "valprec"
    Private Const COL_VALCUR As String = "valcur"

    Public Event GenerateRequested As EventHandler Implements IDdfPreview.GenerateRequested

    Public Sub New()
        InitializeComponent()
        BuildColumns()
        ShowMessage("Selectați o revizie din arbore.")
    End Sub

    Public ReadOnly Property Surface As Control Implements IDdfPreview.Surface
        Get
            Return Me
        End Get
    End Property

    Private Sub BuildColumns()
        Try
            grid.AddColumn(COL_ELEMENT, "Element de fundamentare", KBotColumnType.Text, 240)
            grid.AddColumn(COL_CLSF, "Clasificație", KBotColumnType.Text, 160)
            Dim colPrec As KBotDataColumn = grid.AddColumn(COL_VALPREC, "Valoare precedentă", KBotColumnType.Text, 130)
            colPrec.TextAlign = ContentAlignment.MiddleRight
            Dim colCur As KBotDataColumn = grid.AddColumn(COL_VALCUR, "Valoare curentă", KBotColumnType.Text, 130)
            colCur.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            GlobalErrorLog.Write("XfaXmlPreview.BuildColumns", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Afișează documentul de la calea dată. Fișier lipsă -> suprafața „document lipsă"
    ''' (mesaj + «Generează documentul»), niciodată o excepție (contract IDdfPreview).
    ''' </summary>
    Public Sub ShowDocument(pdfPath As String, exists As Boolean) Implements IDdfPreview.ShowDocument
        Try
            If String.IsNullOrWhiteSpace(pdfPath) Then
                ShowMessage("Selectați o revizie din arbore.")
                Return
            End If
            If Not exists Then
                ShowMissing()
                Return
            End If

            Dim xml As String = CitesteXfa(pdfPath)
            If String.IsNullOrWhiteSpace(xml) Then
                ShowMessage("Documentul nu conține XML XFA lizibil.")
                Return
            End If

            Dim model As DdfXfaModel = DdfXfaParser.Parse(xml)
            If model Is Nothing OrElse model.EsteGol Then
                ShowMessage("Documentul nu a putut fi interpretat.")
                Return
            End If

            Render(model)
            ShowContent()
        Catch ex As Exception
            ' Boundary UI: logăm și arătăm mesajul, nu aruncăm (contractul cere să nu arunce).
            GlobalErrorLog.Write("XfaXmlPreview.ShowDocument", ex)
            ShowMessage("Documentul nu a putut fi afișat. Detalii în jurnalul de erori.")
        End Try
    End Sub

    ' XML-ul XFA: întâi siblingul .xml (scris lângă PDF), altfel extras din PDF.
    Private Shared Function CitesteXfa(pdfPath As String) As String
        Dim xmlPath As String = Path.ChangeExtension(pdfPath, ".xml")
        If File.Exists(xmlPath) Then
            Try
                Return File.ReadAllText(xmlPath)
            Catch ex As Exception
                ' Siblingul există dar nu se poate citi — cădem pe extragerea din PDF.
                GlobalErrorLog.Write("XfaXmlPreview.CitesteXfa", ex)
            End Try
        End If

        Dim xml As String = Nothing
        Dim err As String = Nothing
        If PdfXmlExtractor.TryExtract(pdfPath, xml, err) Then Return xml
        Return Nothing
    End Function

    Private Sub Render(model As DdfXfaModel)
        ' --- Antet: perechi etichetă/valoare ---
        tblHeader.SuspendLayout()
        Try
            tblHeader.Controls.Clear()
            tblHeader.RowStyles.Clear()
            tblHeader.RowCount = model.AntetFields.Count
            Dim palette As ThemePalette = TryGetPalette()
            Dim rowIndex As Integer = 0
            For Each pair In model.AntetFields
                tblHeader.RowStyles.Add(New RowStyle(SizeType.AutoSize))
                Dim cap As New Label() With {
                    .Text = pair.Key & ":", .AutoSize = True, .Margin = New Padding(0, 2, 8, 2)}
                Dim val As New Label() With {
                    .Text = pair.Value, .AutoSize = True, .Margin = New Padding(0, 2, 0, 2),
                    .Font = New Font(Font, FontStyle.Bold)}
                If palette IsNot Nothing Then
                    cap.ForeColor = palette.TextDimColor
                    val.ForeColor = palette.TextColor
                End If
                tblHeader.Controls.Add(cap, 0, rowIndex)
                tblHeader.Controls.Add(val, 1, rowIndex)
                rowIndex += 1
            Next
        Finally
            tblHeader.ResumeLayout(True)
        End Try

        ' --- Nota: descrierea (scurtă preferată, altfel lungă) ---
        lblNota.Text = If(Not String.IsNullOrWhiteSpace(model.DescScurt), model.DescScurt, model.DescLung)

        ' --- Grila: liniile secțiunii A ---
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each l As DdfXfaLinie In model.Linii
                Dim row As KBotDataRow = grid.AddRow()
                row(COL_ELEMENT) = l.ElementFund
                row(COL_CLSF) = l.Clsf
                row(COL_VALPREC) = l.ValPrec
                row(COL_VALCUR) = l.ValCur
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Public Sub Clear() Implements IDdfPreview.Clear
        Try
            grid.ClearRows()
            ShowMessage("Selectați o revizie din arbore.")
        Catch ex As Exception
            GlobalErrorLog.Write("XfaXmlPreview.Clear", ex)
        End Try
    End Sub

    Private Sub btnGenereaza_Click(sender As Object, e As EventArgs) Handles btnGenereaza.Click
        ' Trivial: doar ridică evenimentul spre DdfView (care apelează felia 05).
        RaiseEvent GenerateRequested(Me, EventArgs.Empty)
    End Sub

    ' ── Stări ─────────────────────────────────────────────────────────────────
    Private Sub ShowContent()
        pnlContent.Visible = True
        pnlMissing.Visible = False
        lblMessage.Visible = False
    End Sub

    Private Sub ShowMissing()
        pnlContent.Visible = False
        pnlMissing.Visible = True
        lblMessage.Visible = False
    End Sub

    Private Sub ShowMessage(message As String)
        lblMessage.Text = message
        pnlContent.Visible = False
        pnlMissing.Visible = False
        lblMessage.Visible = True
    End Sub

    Private Shared Function TryGetPalette() As ThemePalette
        Dim current As ThemeScheme = ThemeManager.Current
        Return If(current Is Nothing, Nothing, current.Palette)
    End Function

    ''' <summary>Reaplică schema (grila se auto-temează; aici colorăm chrome-ul și butonul).</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            For Each pnl As Control In New Control() {pnlContent, pnlHeader, tblHeader, pnlMissing, tblMissing}
                pnl.BackColor = p.SurfaceAltColor
            Next
            lblNota.ForeColor = p.TextDimColor
            lblNota.BackColor = Color.Transparent
            lblMissing.ForeColor = p.TextDimColor
            lblMissing.BackColor = Color.Transparent
            lblMessage.ForeColor = p.TextDimColor
            lblMessage.BackColor = p.SurfaceAltColor

            btnGenereaza.BackColor = p.AccentColor
            btnGenereaza.ForeColor = p.AccentTextColor
            btnGenereaza.FlatAppearance.BorderColor = p.AccentColor

            ' Re-colorarea perechilor de antet deja randate (dacă există).
            For Each c As Control In tblHeader.Controls
                Dim lbl As Label = TryCast(c, Label)
                If lbl Is Nothing Then Continue For
                lbl.ForeColor = If(lbl.Font.Bold, p.TextColor, p.TextDimColor)
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("XfaXmlPreview.ApplyTheme", ex)
        End Try
    End Sub

End Class
