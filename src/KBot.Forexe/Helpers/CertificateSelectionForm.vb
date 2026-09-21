Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Security.Cryptography.X509Certificates
Imports System.Windows.Forms
Imports KBot.Common

Public Class CertificateSelectionForm

    ' Proprietăți publice pentru a recupera datele
    Public Property SelectedCertificate As X509Certificate2
    Public Property PinEntered As String

    ''' <summary>
    ''' True dacă utilizatorul a ales să continue fără token (mod Resend Only).
    ''' </summary>
    Public Property IsResendOnlyMode As Boolean = False

    Private _manualPin As Boolean

    Public Sub New(manualPin As Boolean)
        InitializeComponent()
        _manualPin = manualPin
        'grpPin.Visible = False
        ' No ApplyTheme call: the form now inherits KBotThemedForm, which applies and re-applies
        ' the scheme from OnLoad. This is the migration KBotThemedForm's own summary describes --
        ' change the Inherits line, drop the hand-rolled call. Slice 0052 moved it here so the
        ' form would pick up the Calibri base font from the base constructor.
    End Sub

    ''' <summary>
    ''' On load the list offers the certificate the operator confirmed last time (a file under
    ''' %APPDATA%\AVACONT\KBot, written by btnSelect). Nothing is looked up, nothing is probed:
    ''' if the token is not plugged in, that surfaces later, when the certificate is used.
    ''' Only when nothing was saved yet does the full token refresh run. btnRefresh always runs
    ''' the full refresh.
    ''' </summary>
    Private Sub CertificateSelectionForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadCertificates()

        'Try
        '    Dim lastUsed As X509Certificate2 = CertificateService.LoadLastUsedCertificate()
        '    If lastUsed IsNot Nothing Then
        '        lstCertificates.BeginUpdate()
        '        Try
        '            lstCertificates.Items.Clear()
        '            lstCertificates.Items.Add(lastUsed)
        '        Finally
        '            lstCertificates.EndUpdate()
        '        End Try
        '        lstCertificates.SelectedIndex = 0
        '    Else
        '        LoadCertificates()
        '    End If
        'Catch ex As Exception
        '    GlobalErrorLog.Write("CertificateSelectionForm.CertificateSelectionForm_Load", ex)
        'End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        Try
            LoadCertificates()
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.btnRefresh_Click", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Anuleaza is a plain secondary button: the modern scheme would otherwise round it and
    ''' pad it like every other stock button it meets. Runs after every theme apply.
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
#If DEBUG Then
#Else
            btnresendOnly.Visible = False
#End If
            ButtonStyles.ApplyNormal(btnClose, ThemeManager.Current)
            ButtonStyles.ApplyNormal(btnRefresh, ThemeManager.Current)
            btnClose.Padding = Padding.Empty
            btnRefresh.Padding = Padding.Empty
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.OnThemeChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The full refresh: enumerates the stores AND touches every private key
    ''' (CertificateService.IsValidHardwareCertificate), which is what actually asks the tokens.
    ''' </summary>
    Private Sub LoadCertificates()
        lstCertificates.Items.Clear()
        lstCertificates.BeginUpdate()

        Try
            Dim certs As List(Of X509Certificate2) = CertificateService.GetSmartcardCertificates()

            If certs.Count = 0 Then
                KBotMessage.Show("Nu a fost detectat niciun certificat pe Token/SmartCard." & vbCrLf &
                                "Te rog introdu token-ul și apasă OK pentru a reîncerca.",
                                "Lipsă Token", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                For Each cert As X509Certificate2 In certs
                    lstCertificates.Items.Add(cert)
                Next
            End If

        Catch ex As Exception
            KBotMessage.Show("Eroare la citirea certificatelor: " & ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            lstCertificates.EndUpdate()
        End Try

        If lstCertificates.Items.Count > 0 Then
            lstCertificates.SelectedIndex = 0
        End If
    End Sub

    ' ==========================================================
    ' DESENARE
    ' ==========================================================

    ' Item layout, in LOGICAL pixels (96 dpi); scaled at use with DeviceDpi (house DPI rule).
    Private Const ItemPadLogic As Integer = 8          ' top / bottom of the item
    Private Const IconSizeLogic As Integer = 40
    Private Const IconGapLogic As Integer = 15         ' circle -> text
    Private Const ParagraphGapLogic As Integer = 6     ' name -> details (the "paragraph spacing")
    Private Const LineGapLogic As Integer = 2          ' between the two detail lines

    Private _titleFont As Font
    Private _detailFont As Font
    Private _initialFont As Font

    Private Function Px(logic As Integer) As Integer
        Return CInt(Math.Round(logic * DeviceDpi / 96.0))
    End Function

    ''' <summary>Fonts for the item, built once from the list's own family (theme font).</summary>
    Private Sub EnsureItemFonts()
        If _titleFont IsNot Nothing Then Return
        Dim family As FontFamily = lstCertificates.Font.FontFamily
        _titleFont = New Font(family, 12, FontStyle.Bold)
        _detailFont = New Font(family, 10, FontStyle.Regular)
        _initialFont = New Font(family, 14, FontStyle.Bold)
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            _titleFont?.Dispose() : _titleFont = Nothing
            _detailFont?.Dispose() : _detailFont = Nothing
            _initialFont?.Dispose() : _initialFont = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ''' <summary>
    ''' Height of one item = padding + name line + paragraph gap + two detail lines + padding,
    ''' measured with the real fonts, so the item grows with the DPI instead of clipping.
    ''' </summary>
    Private Sub lstCertificates_MeasureItem(sender As Object, e As MeasureItemEventArgs) Handles lstCertificates.MeasureItem
        Try
            EnsureItemFonts()
            Dim titleH As Integer = CInt(Math.Ceiling(_titleFont.GetHeight(e.Graphics)))
            Dim detailH As Integer = CInt(Math.Ceiling(_detailFont.GetHeight(e.Graphics)))
            Dim textH As Integer = titleH + Px(ParagraphGapLogic) + detailH + Px(LineGapLogic) + detailH
            e.ItemHeight = Math.Min(255, 2 * Px(ItemPadLogic) + Math.Max(textH, Px(IconSizeLogic)))
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.lstCertificates_MeasureItem", ex)
        End Try
    End Sub

    Private Sub lstCertificates_DrawItem(sender As Object, e As DrawItemEventArgs) Handles lstCertificates.DrawItem
        ' Boundary de owner-draw: un throw aici pică pictarea listei — logăm și înghițim.
        Try
            If e.Index < 0 Then Return
            EnsureItemFonts()

            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim cert As X509Certificate2 = DirectCast(lstCertificates.Items(e.Index), X509Certificate2)
            Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected

            Dim backColor As Color = If(isSelected, Color.FromArgb(235, 245, 255), Color.White)
            Dim borderColor As Color = If(isSelected, Color.FromArgb(0, 120, 215), Color.WhiteSmoke)
            Dim nameColor As Color = If(isSelected, Color.Black, Color.FromArgb(50, 50, 50))
            Dim detailsColor As Color = Color.Gray

            Using br As New SolidBrush(backColor)
                g.FillRectangle(br, e.Bounds)
            End Using

            Using p As New Pen(Color.WhiteSmoke)
                g.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
            End Using

            Dim subjectName As String = CertificateService.GetCommonName(cert)

            ' Circle with the initial, vertically centred in the item.
            Dim iconSize As Integer = Px(IconSizeLogic)
            Dim iconRect As New Rectangle(e.Bounds.X + Px(10), e.Bounds.Y + (e.Bounds.Height - iconSize) \ 2, iconSize, iconSize)
            Using bIcon As New SolidBrush(If(isSelected, Color.FromArgb(0, 120, 215), Color.LightGray))
                g.FillEllipse(bIcon, iconRect)
            End Using
            DrawInitialCentered(g, If(String.IsNullOrEmpty(subjectName), "?", subjectName.Substring(0, 1).ToUpperInvariant()), iconRect)

            Dim textLeft As Integer = iconRect.Right + Px(IconGapLogic)
            Dim textTop As Integer = e.Bounds.Y + Px(ItemPadLogic)
            Dim titleH As Integer = CInt(Math.Ceiling(_titleFont.GetHeight(g)))
            Dim detailH As Integer = CInt(Math.Ceiling(_detailFont.GetHeight(g)))

            Using nameBrush As New SolidBrush(nameColor), detailBrush As New SolidBrush(detailsColor)
                g.DrawString(subjectName, _titleFont, nameBrush, textLeft, textTop)

                ' Paragraph spacing between the name and the details below it.
                Dim issuerTop As Integer = textTop + titleH + Px(ParagraphGapLogic)
                Dim issuerName As String = "Emitent: " & cert.GetNameInfo(X509NameType.SimpleName, True)
                g.DrawString(issuerName, _detailFont, detailBrush, textLeft, issuerTop)

                Dim expireTop As Integer = issuerTop + detailH + Px(LineGapLogic)
                Dim expireText As String = $"Expiră: {cert.NotAfter:dd.MM.yyyy}  |  SN: {cert.SerialNumber}"
                g.DrawString(expireText, _detailFont, If(cert.NotAfter < DateTime.Now, Brushes.Red, detailBrush), textLeft, expireTop)
            End Using

            If isSelected Then
                Using p As New Pen(borderColor, 2)
                    Dim borderRect As Rectangle = e.Bounds
                    borderRect.Inflate(-1, -1)
                    g.DrawRectangle(p, borderRect)
                End Using
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.lstCertificates_DrawItem", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Paints <paramref name="letter"/> with its GLYPH centred on <paramref name="circle"/>.
    ''' Text renderers centre the font's line box (ascent + descent + internal leading), which
    ''' puts a capital letter visibly above the middle; the glyph outline has no such slack.
    ''' </summary>
    Private Sub DrawInitialCentered(g As Graphics, letter As String, circle As Rectangle)
        Using path As New GraphicsPath()
            Dim emPixels As Single = _initialFont.SizeInPoints * g.DpiY / 72.0F
            path.AddString(letter, _initialFont.FontFamily, CInt(_initialFont.Style), emPixels, PointF.Empty, StringFormat.GenericTypographic)
            Dim glyph As RectangleF = path.GetBounds()
            If glyph.IsEmpty Then Return
            Dim dx As Single = circle.X + (circle.Width - glyph.Width) / 2.0F - glyph.X
            Dim dy As Single = circle.Y + (circle.Height - glyph.Height) / 2.0F - glyph.Y
            Using m As New Matrix()
                m.Translate(dx, dy)
                path.Transform(m)
            End Using
            g.FillPath(Brushes.White, path)
        End Using
    End Sub

    ' ==========================================================
    ' BUTOANE
    ' ==========================================================

    Private Sub btnSelect_Click(sender As Object, e As EventArgs) Handles btnSelect.Click
        Try
            If lstCertificates.SelectedIndex < 0 Then
                KBotMessage.Show("Te rog selectează un certificat din listă.", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            SelectedCertificate = DirectCast(lstCertificates.SelectedItem, X509Certificate2)

            If Not _manualPin Then
                ' The saved certificate carries no private key: the PIN test needs the store copy,
                ' and this is the moment a missing token is found out, not on load.
                Dim withKey As X509Certificate2 = CertificateService.ResolveFromStore(SelectedCertificate)
                If withKey Is Nothing Then
                    KBotMessage.Show("Token-ul cu certificatul ales nu este introdus sau nu poate fi accesat." & vbCrLf &
                                     "Introdu token-ul și apasă butonul de reîmprospătare.",
                                     "Lipsă Token", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                SelectedCertificate = withKey
                Dim validation = CertificateService.ValidatePin(SelectedCertificate)
                If Not validation.Success Then
                    KBotMessage.Show(validation.Message, "Eroare PIN", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    'txtPin.Clear()
                    Return
                End If
            Else
                PinEntered = ""
            End If

            ' Remembered for next time, so the next opening offers it without any token probe.
            Try
                CertificateService.SaveLastUsedCertificate(SelectedCertificate)
            Catch ex As Exception
                ' Already logged by the service; a failed save must not block the selection.
            End Try

            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.btnSelect_Click", ex)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    ''' <summary>
    ''' Utilizatorul alege să continue fără token — mod Resend Only.
    ''' Niciun certificat nu este selectat, executorul nu va fi disponibil.
    ''' </summary>
    Private Sub btnResendOnly_Click(sender As Object, e As EventArgs) Handles btnResendOnly.Click
        IsResendOnlyMode = True
        SelectedCertificate = Nothing
        Me.DialogResult = DialogResult.No
        Me.Close()
    End Sub

    Private Sub lstCertificates_DoubleClick(sender As Object, e As EventArgs) Handles lstCertificates.DoubleClick
        btnSelect_Click(sender, e)
    End Sub
End Class