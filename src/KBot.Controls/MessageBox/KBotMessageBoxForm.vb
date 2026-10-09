Option Strict On
Imports System.Drawing
Imports System.Media
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The window that stands in for a native message box -- and has nothing native left in it: its own title
''' bar (<see cref="KBotCaptionBar"/>, an X and nothing else, hidden when the message has no way out), a
''' heading line, the message as simple HTML (<see cref="KBotHtmlLabel"/>), a drawn glyph and
''' <see cref="KBotButton"/>s. Built from a <see cref="KBotMessageSpec"/>; opened through
''' <see cref="KBotMessageBox"/>.
'''
''' <para><b>Sizing.</b> The window hugs its message: widths and heights come from the measured text and the
''' height of the font (so the box follows text size, dpi and zoom), with the designer's own numbers as the
''' margins and minimums -- see the comment in the .Designer.vb. It is laid out in <see cref="OnLoad"/> AFTER
''' the base class applied theme and zoom (otherwise zoom would scale the sizes twice), then centred again.</para>
'''
''' <para><b>Native behaviour kept.</b> The system sound of the kind, Esc = Cancel (or OK when it is the only
''' button), Enter = the default button, and a dead close button on sets with no way out.</para>
''' </summary>
Public Class KBotMessageBoxForm

    Private ReadOnly _spec As KBotMessageSpec
    Private ReadOnly _standard As New List(Of KBotButton)()
    Private _extraClicked As Boolean
    Private _mayDismiss As Boolean
    Private _sending As Boolean
    Private _reportGlyph As Bitmap
    Private ReadOnly _shownUtc As DateTime = DateTime.UtcNow
    Private ReadOnly _tipContent As New KBotToolTipContent()

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _spec = New KBotMessageSpec()
    End Sub

    Public Sub New(k_spec As KBotMessageSpec)
        ArgumentNullException.ThrowIfNull(k_spec)
        InitializeComponent()
        _spec = k_spec
        BuildContent()
    End Sub

    ''' <summary>True when the operator pressed the extra button (the standard answer is then None).</summary>
    Public ReadOnly Property ExtraClicked As Boolean
        Get
            Return _extraClicked
        End Get
    End Property

    ' ---------------- content ----------------

    Private Sub BuildContent()
        Try
            Text = _spec.Caption
            capBar.Text = _spec.Caption
            TopMost = _spec.TopMost
            picIcon.Kind = _spec.Kind
            picIcon.Visible = _spec.Kind <> KBotMsgKind.None
            lblHeader.Visible = Not String.IsNullOrWhiteSpace(_spec.Header)
            If lblHeader.Visible Then lblHeader.Html = _spec.Header   ' its look (bold, size) is the designer's font
            lblText.Html = _spec.Text

            Dim k_slots() As KBotButton = {btnOK, btnNOK, btn3}
            Dim k_defs As List(Of (Caption As String, Result As DialogResult)) = StandardButtons(_spec.Buttons)
            For k_i As Integer = 0 To k_slots.Length - 1
                Dim k_b As KBotButton = k_slots(k_i)
                If k_i < k_defs.Count Then
                    k_b.Text = k_defs(k_i).Caption
                    k_b.DialogResult = k_defs(k_i).Result
                    k_b.Visible = True
                    k_b.Primary = False
                    _standard.Add(k_b)
                Else
                    k_b.Visible = False
                End If
            Next

            btnExtra.Visible = Not String.IsNullOrWhiteSpace(_spec.ExtraButton)
            If btnExtra.Visible Then btnExtra.Text = _spec.ExtraButton

            ' Esc: Cancel when there is one, OK when it is the only button.
            For Each k_b As KBotButton In _standard
                If k_b.DialogResult = DialogResult.Cancel Then CancelButton = k_b
            Next
            If CancelButton Is Nothing AndAlso _standard.Count = 1 AndAlso _standard(0).DialogResult = DialogResult.OK Then
                CancelButton = _standard(0)
            End If

            ' The X: shown when the message has a way out (Auto), or when the message asks for it; a shown X on
            ' a set with no Cancel answers Cancel.
            Dim k_wayOut As Boolean = CancelButton IsNot Nothing
            Select Case _spec.CloseButton
                Case KBotMsgClose.Show : capBar.ShowClose = True
                Case KBotMsgClose.Hide : capBar.ShowClose = False
                Case Else : capBar.ShowClose = k_wayOut
            End Select
            _mayDismiss = capBar.ShowClose

            ' Slice 0112-04: an error can be sent to the server from the title bar (the application has to have
            ' installed the sender, KBotMessage.ErrorReporter, and the operator to be logged in -- it says so if not).
            Dim k_canReport As Boolean = _spec.Kind = KBotMsgKind.Error AndAlso KBotMessage.ErrorReporter IsNot Nothing
            If k_canReport Then
                _reportGlyph = DrawReportGlyph()
                capBar.OptionButtonImage = _reportGlyph
                _tipContent.HeaderText = "Trimite eroarea"
                _tipContent.Text = "Trimite acest mesaj, cu detaliile necesare (aplicația, calculatorul, jurnalele), la serverul K-BOT, ca să poată fi analizat."
            End If
            capBar.ShowOptionsButton = k_canReport

            Dim k_def As Integer = Math.Min(Math.Max(_spec.DefaultButton, 1), _standard.Count) - 1
            _standard(k_def).Primary = True
            AcceptButton = _standard(k_def)
            ActiveControl = _standard(k_def)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.BuildContent", ex)
        End Try
    End Sub

    Private Shared Function StandardButtons(k_set As KBotMsgButtons) As List(Of (Caption As String, Result As DialogResult))
        Dim k_list As New List(Of (Caption As String, Result As DialogResult))()
        Select Case k_set
            Case KBotMsgButtons.OKCancel
                k_list.Add(("OK", DialogResult.OK))
                k_list.Add(("Anulare", DialogResult.Cancel))
            Case KBotMsgButtons.AbortRetryIgnore
                k_list.Add(("Renunță", DialogResult.Abort))
                k_list.Add(("Reîncearcă", DialogResult.Retry))
                k_list.Add(("Ignoră", DialogResult.Ignore))
            Case KBotMsgButtons.YesNoCancel
                k_list.Add(("Da", DialogResult.Yes))
                k_list.Add(("Nu", DialogResult.No))
                k_list.Add(("Anulare", DialogResult.Cancel))
            Case KBotMsgButtons.YesNo
                k_list.Add(("Da", DialogResult.Yes))
                k_list.Add(("Nu", DialogResult.No))
            Case KBotMsgButtons.RetryCancel
                k_list.Add(("Reîncearcă", DialogResult.Retry))
                k_list.Add(("Anulare", DialogResult.Cancel))
            Case Else
                k_list.Add(("OK", DialogResult.OK))
        End Select
        Return k_list
    End Function

    Private Sub btnExtra_Click(sender As Object, e As EventArgs) Handles btnExtra.Click
        Try
            _extraClicked = True
            DialogResult = DialogResult.OK
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.btnExtra_Click", ex)
        End Try
    End Sub

    ' ---------------- send the error (slice 0112-04) ----------------

    ''' <summary>The glyph of the button: an arrow leaving a tray. A silhouette, so the bar recolours it with the theme.</summary>
    Private Shared Function DrawReportGlyph() As Bitmap
        Dim k_bmp As New Bitmap(64, 64)
        Using k_g As Graphics = Graphics.FromImage(k_bmp)
            k_g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            Using k_brush As New SolidBrush(Color.Black)
                k_g.FillPolygon(k_brush, New Point() {
                    New Point(32, 6), New Point(12, 27), New Point(24, 27), New Point(24, 40),
                    New Point(40, 40), New Point(40, 27), New Point(52, 27)})
            End Using
            Using k_pen As New Pen(Color.Black, 6.0F)
                k_pen.LineJoin = Drawing2D.LineJoin.Round
                k_g.DrawLines(k_pen, New Point() {New Point(9, 42), New Point(9, 57), New Point(55, 57), New Point(55, 42)})
            End Using
        End Using
        Return k_bmp
    End Function

    Private Sub capBar_OptionButtonHoverChanged(sender As Object, e As EventArgs) Handles capBar.OptionButtonHoverChanged
        Try
            If capBar.OptionButtonHot AndAlso Not _sending Then
                Dim k_r As Rectangle = capBar.OptionButtonBounds
                ttip.ShowAt(capBar, _tipContent, capBar.PointToScreen(New Point(k_r.Left, k_r.Bottom)))
            Else
                ttip.HideNow()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.capBar_OptionButtonHoverChanged", ex)
        End Try
    End Sub

    Private Async Sub capBar_OptionButtonClick(sender As Object, e As EventArgs) Handles capBar.OptionButtonClick
        Try
            Dim k_send As KBotMessage.ErrorReportHandler = KBotMessage.ErrorReporter
            If _sending OrElse k_send Is Nothing Then Return
            _sending = True
            ttip.HideNow()
            Dim k_owner As Form = Owner
            Dim k_report As New MessageErrorReport() With {
                .ShownUtc = _shownUtc,
                .Source = If(_spec.Source, String.Empty),
                .SourceLine = _spec.SourceLine,
                .Caption = If(_spec.Caption, String.Empty),
                .Header = If(_spec.Header, String.Empty),
                .Text = If(_spec.Text, String.Empty),
                .Buttons = _spec.Buttons.ToString(),
                .OwnerForm = If(k_owner Is Nothing, String.Empty, k_owner.GetType().Name & " | " & k_owner.Text)
            }
            ' No message either way (operator, 09.10.2026): the button is dimmed while the server answers, hidden for good
            ' when it took the report, and live again when it did not (the reason is in the error log).
            capBar.OptionButtonEnabled = False
            Try
                Await k_send(k_report)
                capBar.ShowOptionsButton = False
            Catch ex As Exception
                GlobalErrorLog.Write("KBotMessageBoxForm.capBar_OptionButtonClick", ex)
                capBar.OptionButtonEnabled = True
            Finally
                _sending = False
            End Try
        Catch ex As Exception
            _sending = False
            GlobalErrorLog.Write("KBotMessageBoxForm.capBar_OptionButtonClick", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Try
            ttip.HideNow()
            capBar.OptionButtonImage = Nothing
            _reportGlyph?.Dispose()
            _reportGlyph = Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.OnFormClosed", ex)
        End Try
    End Sub

    ' ---------------- layout ----------------

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            LayoutContent()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.OnLoad", ex)
        End Try
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            pnlButtons.BackColor = ThemeManager.Current.Palette.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.OnThemeChanged", ex)
        End Try
    End Sub

    Private Sub LayoutContent()
        Dim k_u As Integer = Font.Height
        Dim k_area As Rectangle = AppScreen.Reference(Me).WorkingArea

        ' margins and minimums come from the designer (already scaled to this screen by the platform)
        Dim k_padX As Integer = If(pnlBody.Padding.Left > 0, pnlBody.Padding.Left, k_u)
        Dim k_padY As Integer = If(pnlBody.Padding.Top > 0, pnlBody.Padding.Top, k_u)
        Dim k_gapIcon As Integer = If(picIcon.Margin.Right > 0, picIcon.Margin.Right, k_u * 3 \ 4)
        Dim k_gapHeader As Integer = If(lblHeader.Margin.Bottom > 0, lblHeader.Margin.Bottom, k_u \ 2)
        Dim k_btnH As Integer = Math.Max(btn3.Height, k_u * 2)
        Dim k_btnMinW As Integer = Math.Max(btn3.Width, k_u * 5)
        Dim k_btnGap As Integer = k_u * 6 \ 10
        Dim k_iconSide As Integer = If(picIcon.Visible, Math.Max(picIcon.Width, k_u * 2), 0)

        ' The width range is the designer's (MinimumSize / MaximumSize, 0 = free): the text wraps so that the
        ' window never gets wider than the maximum, and never narrower than the minimum.
        ' Read the designer's range, then DROP it from the form: a Form treats a 0 height in MaximumSize as a limit of
        ' zero (the window collapses to its chrome), and the width range has done its job once it is in these numbers.
        Dim k_designMax As Integer = MaximumSize.Width
        Dim k_designMin As Integer = MinimumSize.Width
        MaximumSize = Size.Empty
        MinimumSize = Size.Empty
        Dim k_maxClientW As Integer = If(k_designMax > 0, k_designMax, k_u * 50)
        k_maxClientW = Math.Min(k_maxClientW, k_area.Width - k_u * 2)
        Dim k_minClientW As Integer = Math.Min(k_designMin, k_maxClientW)
        Dim k_maxText As Integer = Math.Max(k_u * 8, k_maxClientW - 2 * k_padX - If(k_iconSide > 0, k_iconSide + k_gapIcon, 0))
        Dim k_hdr As Size = If(lblHeader.Visible, lblHeader.MeasureHtml(k_maxText), Size.Empty)
        Dim k_txt As Size = lblText.MeasureHtml(k_maxText)
        Dim k_textW As Integer = Math.Max(Math.Max(k_hdr.Width, k_txt.Width), k_u * 8)
        Dim k_textH As Integer = k_hdr.Height + If(k_hdr.Height > 0 AndAlso k_txt.Height > 0, k_gapHeader, 0) + k_txt.Height

        ' the buttons: extra one flush left, standard ones flush right
        Dim k_shown As New List(Of KBotButton)()
        If btnExtra.Visible Then k_shown.Add(btnExtra)
        ' House rule (operator, 08.10.2026): Yes / OK / Retry-style answers ALWAYS on the right, No / Cancel / Abort-style
        ' ALWAYS on the left. _standard keeps the logical order (the default button is chosen in it); the screen order is by rank.
        k_shown.AddRange(_standard.OrderBy(Function(k_b) SideRank(k_b.DialogResult)))
        Dim k_widths As New List(Of Integer)()
        Dim k_btnTotal As Integer = 2 * k_padX + k_btnGap * Math.Max(0, k_shown.Count - 1)
        For Each k_b As KBotButton In k_shown
            Dim k_w As Integer = Math.Max(TextRenderer.MeasureText(k_b.Text, k_b.Font).Width + k_u * 2, k_btnMinW)
            k_widths.Add(k_w)
            k_btnTotal += k_w
        Next
        If btnExtra.Visible Then k_btnTotal += k_u * 2

        ' sizes
        Dim k_bodyW As Integer = 2 * k_padX + If(k_iconSide > 0, k_iconSide + k_gapIcon, 0) + k_textW
        Dim k_capW As Integer = TextRenderer.MeasureText(capBar.Text, capBar.Font).Width + k_u * 6
        Dim k_clientW As Integer = Math.Max(Math.Max(Math.Max(k_bodyW, k_btnTotal), Math.Min(k_capW, k_u * 40)), k_minClientW)
        k_clientW = Math.Min(k_clientW, Math.Max(k_maxClientW, k_btnTotal))
        Dim k_capH As Integer = capBar.Height
        Dim k_buttonsH As Integer = Math.Max(pnlButtons.Height, k_btnH + k_u * 14 \ 10)
        Dim k_bodyH As Integer = Math.Max(k_iconSide, k_textH) + 2 * k_padY
        Dim k_maxClientH As Integer = CInt(k_area.Height * 0.8)
        Dim k_clientH As Integer = Math.Min(k_capH + k_bodyH + k_buttonsH, Math.Max(k_maxClientH, k_capH + k_buttonsH + k_u * 6))
        ClientSize = New Size(k_clientW, k_clientH)
        pnlButtons.Height = k_buttonsH

        ' body: glyph left, then heading + text; the text block is centred on the glyph when it is shorter
        Dim k_textX As Integer = k_padX + If(k_iconSide > 0, k_iconSide + k_gapIcon, 0)
        picIcon.Dock = DockStyle.None
        picIcon.SetBounds(k_padX, k_padY, k_iconSide, k_iconSide)
        Dim k_y As Integer = k_padY + Math.Max(0, (k_iconSide - k_textH) \ 2)
        If lblHeader.Visible Then
            lblHeader.Dock = DockStyle.None
            lblHeader.SetBounds(k_textX, k_y, k_textW, k_hdr.Height)
            k_y += k_hdr.Height + If(k_txt.Height > 0, k_gapHeader, 0)
        End If
        lblText.Dock = DockStyle.None
        lblText.SetBounds(k_textX, k_y, k_textW, k_txt.Height)

        ' buttons
        Dim k_by As Integer = (k_buttonsH - k_btnH) \ 2
        Dim k_x As Integer = k_clientW - k_padX
        For k_i As Integer = k_shown.Count - 1 To 0 Step -1
            Dim k_b As KBotButton = k_shown(k_i)
            k_b.Dock = DockStyle.None
            If k_b Is btnExtra Then
                k_b.SetBounds(k_padX, k_by, k_widths(k_i), k_btnH)
            Else
                k_x -= k_widths(k_i)
                k_b.SetBounds(k_x, k_by, k_widths(k_i), k_btnH)
                k_x -= k_btnGap
            End If
        Next

        PlaceOverOwner()
    End Sub

    ''' <summary>Left-to-right rank of an answer: 0 = leftmost (Cancel, Abort), 1 = No / Ignore, 2 = rightmost (Yes, OK, Retry).</summary>
    Private Shared Function SideRank(k_result As DialogResult) As Integer
        Select Case k_result
            Case DialogResult.Cancel, DialogResult.Abort : Return 0
            Case DialogResult.No, DialogResult.Ignore : Return 1
            Case Else : Return 2
        End Select
    End Function

    Private Sub PlaceOverOwner()
        Dim k_over As Form = If(Owner, Nothing)
        If k_over Is Nothing AndAlso Form.ActiveForm IsNot Nothing AndAlso Form.ActiveForm IsNot Me Then k_over = Form.ActiveForm
        If k_over IsNot Nothing AndAlso k_over.Visible AndAlso k_over.WindowState = FormWindowState.Normal Then
            Location = AppScreen.CenteredIn(k_over.Bounds, Size)
        Else
            AppScreen.Center(Me)
        End If
        AppScreen.KeepOnScreen(Me)
    End Sub

    ' ---------------- behaviour ----------------

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Select Case _spec.Kind
                Case KBotMsgKind.Error : SystemSounds.Hand.Play()
                Case KBotMsgKind.Warning : SystemSounds.Exclamation.Play()
                Case KBotMsgKind.Question : SystemSounds.Question.Play()
                Case KBotMsgKind.Info : SystemSounds.Asterisk.Play()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.OnShown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If e.CloseReason = CloseReason.UserClosing AndAlso DialogResult = DialogResult.None Then
                If Not _mayDismiss Then
                    ' No X, so Alt+F4 does nothing either: the question must be answered.
                    e.Cancel = True
                    Return
                End If
                ' The X (or Alt+F4) answers Cancel.
                DialogResult = DialogResult.Cancel
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMessageBoxForm.OnFormClosing", ex)
        End Try
        MyBase.OnFormClosing(e)
    End Sub

End Class
