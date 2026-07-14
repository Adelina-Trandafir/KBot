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
        grpPin.Visible = False
        KBotTheme.ApplyTheme(Me)
    End Sub

    Private Sub CertificateSelectionForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            LoadCertificates()
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.CertificateSelectionForm_Load", ex)
        End Try
    End Sub

    Private Sub LoadCertificates()
        lstCertificates.Items.Clear()
        lstCertificates.BeginUpdate()

        Try
            Dim certs As List(Of X509Certificate2) = CertificateService.GetSmartcardCertificates()

            If certs.Count = 0 Then
                MessageBox.Show("Nu a fost detectat niciun certificat pe Token/SmartCard." & vbCrLf &
                                "Te rog introdu token-ul și apasă OK pentru a reîncerca.",
                                "Lipsă Token", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                For Each cert As X509Certificate2 In certs
                    lstCertificates.Items.Add(cert)
                Next
            End If

        Catch ex As Exception
            MessageBox.Show("Eroare la citirea certificatelor: " & ex.Message, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
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

    Private Sub lstCertificates_MeasureItem(sender As Object, e As MeasureItemEventArgs) Handles lstCertificates.MeasureItem
        e.ItemHeight = 75
    End Sub

    Private Sub lstCertificates_DrawItem(sender As Object, e As DrawItemEventArgs) Handles lstCertificates.DrawItem
      ' Boundary de owner-draw: un throw aici pică pictarea listei — logăm și înghițim.
      Try
        If e.Index < 0 Then Return

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

        Dim iconRect As New Rectangle(e.Bounds.X + 10, e.Bounds.Y + 15, 40, 40)
        Using bIcon As New SolidBrush(If(isSelected, Color.FromArgb(0, 120, 215), Color.LightGray))
            g.FillEllipse(bIcon, iconRect)
        End Using
        TextRenderer.DrawText(g, CertificateService.GetCommonName(cert).AsSpan(0, 1), New Font("Segoe UI", 14, FontStyle.Bold), iconRect, Color.White, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)

        Dim textLeft As Integer = iconRect.Right + 15
        Dim textTop As Integer = e.Bounds.Y + 8

        Dim subjectName As String = CertificateService.GetCommonName(cert)
        Dim titleFont As New Font("Segoe UI Semibold", 11, FontStyle.Bold)
        g.DrawString(subjectName, titleFont, New SolidBrush(nameColor), textLeft, textTop)

        Dim issuerName As String = "Emitent: " & cert.GetNameInfo(X509NameType.SimpleName, True)
        Dim detailFont As New Font("Segoe UI", 9, FontStyle.Regular)
        g.DrawString(issuerName, detailFont, New SolidBrush(detailsColor), textLeft, textTop + 22)

        Dim expireText As String = $"Expiră: {cert.NotAfter:dd.MM.yyyy}  |  SN: {cert.SerialNumber}"
        Dim expireColor As Brush = If(cert.NotAfter < DateTime.Now, Brushes.Red, New SolidBrush(detailsColor))
        g.DrawString(expireText, detailFont, expireColor, textLeft, textTop + 42)

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

    ' ==========================================================
    ' BUTOANE
    ' ==========================================================

    Private Sub btnSelect_Click(sender As Object, e As EventArgs) Handles btnSelect.Click
        Try
            If lstCertificates.SelectedIndex < 0 Then
                MessageBox.Show("Te rog selectează un certificat din listă.", "Atenție", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            SelectedCertificate = DirectCast(lstCertificates.SelectedItem, X509Certificate2)

            If Not _manualPin Then
                Dim validation = CertificateService.ValidatePin(SelectedCertificate)
                If Not validation.Success Then
                    MessageBox.Show(validation.Message, "Eroare PIN", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    txtPin.Clear()
                    Return
                End If
            Else
                PinEntered = ""
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("CertificateSelectionForm.btnSelect_Click", ex)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
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

End Class