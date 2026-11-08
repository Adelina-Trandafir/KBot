Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The opening page of the main window: a foldable «Ce este nou» card at the top right (opened again
''' after every update, until the operator folds it; the last versions of <c>NOUTATI.md</c>, the same text as «Setari -> Istoric
''' versiuni»; read only, no selection, no focus) and the K-BOT mark in the corner. The side panel
''' (recent machete, recent actions, session information, session errors) belongs to the main
''' window, so it stays beside every page, this one included. A page of <c>AvacontMain</c>: it stays
''' loaded and is only shown or hidden.
''' </summary>
Public Class AcasaPage

    Private ReadOnly _session As SessionContext
    Private _newsFilter As NoInteractionFilter
    Private _newsFullHeight As Integer
    Private _newsSignature As String   ' what the news box was last filled with (see RefreshState)
    Private _arrowDown As Bitmap
    Private _arrowUp As Bitmap
    Private ReadOnly _chat As ChatService
    Private _outsideClick As OutsideClickFilter

    Public Sub New(k_session As SessionContext, k_chat As ChatService)
        ArgumentNullException.ThrowIfNull(k_session)
        ArgumentNullException.ThrowIfNull(k_chat)
        InitializeComponent()
        _session = k_session
        _chat = k_chat
        HookChat()
        edNoutati.Editabil = False
        MakeNewsReadOnly()
        ' After an update the news open by themselves, once; from then on the button decides.
        Dim k_version As String = CurrentVersionText()
        If Not String.Equals(AppSettings.Current.NewsSeenVersion, k_version, StringComparison.Ordinal) Then
            AppSettings.Current.NewsSeenVersion = k_version
            AppSettings.Current.HomeNewsCollapsed = False
            AppSettings.Current.Save()
        End If
        ApplyNewsFolding(AppSettings.Current.HomeNewsCollapsed)
    End Sub

    Private Shared Function CurrentVersionText() As String
        Try
            Return AppUpdateService.CurrentVersion.ToString()
        Catch ex As Exception
            ' Already logged by the service.
            Return String.Empty
        End Try
    End Function

    ''' <summary>
    ''' Reads everything again. Called each time the page is brought back on screen. The news box is written only when what it
    ''' would show differs from what it shows now (text, font, colour, dpi): refilling the same text made it flash and jump back
    ''' to the top every time the operator came back to this page.
    ''' </summary>
    Public Sub RefreshState()
        Try
            Dim k_blocks As List(Of NoteBlock) = ReleaseNotesText.HistoryBlocks(ReleaseNotesText.RecentVersions)
            Dim k_box As RichTextBox = edNoutati.TextBox
            Dim k_sign As String = NewsSignature(k_box, k_blocks)
            If String.Equals(k_sign, _newsSignature, StringComparison.Ordinal) Then Return
            ReleaseNotesText.Fill(k_box, k_blocks)
            _newsSignature = k_sign
        Catch ex As Exception
            ' UI boundary (called from the host when the page is shown): log and swallow.
            GlobalErrorLog.Write("AcasaPage.RefreshState", ex)
        End Try
    End Sub

    ' Everything that decides how the news box looks: the paragraphs, and the font / colour / dpi they are drawn with.
    Private Shared Function NewsSignature(k_box As RichTextBox, k_blocks As IEnumerable(Of NoteBlock)) As String
        Dim k_sb As New System.Text.StringBuilder()
        k_sb.Append(k_box.Font.ToString()).Append("|"c).Append(k_box.ForeColor.ToArgb()).Append("|"c).Append(k_box.DeviceDpi).Append("|"c)
        For Each k_block As NoteBlock In k_blocks
            k_sb.Append(CInt(k_block.Kind)).Append(":"c).Append(k_block.Text).Append(ControlChars.Lf)
        Next
        Return k_sb.ToString()
    End Function

    ' ---- «Ce este nou»: folding, and a text that can only be looked at -------------------------------

    Private Sub BtnNoutati_Click(sender As Object, e As EventArgs) Handles btnNoutati.Click
        Try
            Dim k_fold As Boolean = edNoutati.Visible
            ApplyNewsFolding(k_fold)
            AppSettings.Current.HomeNewsCollapsed = k_fold
            AppSettings.Current.Save()
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.BtnNoutati_Click", ex)
        End Try
    End Sub

    ' Folded = the title bar only; unfolded = the text box under it.
    Private Sub ApplyNewsFolding(k_folded As Boolean)
        If _newsFullHeight = 0 Then _newsFullHeight = pnlNoutati.Height
        edNoutati.Visible = Not k_folded
        pnlNoutati.Height = If(k_folded, pnlNoutatiHead.Height, _newsFullHeight)
        ShowNewsArrow(k_folded)
    End Sub

    ' The fold button carries an icon: the "down" arrow of the resources, painted in the text colour of the theme
    ' (the stock one is blue) and turned over when the card is open (click = fold).
    Private Sub BuildNewsArrows()
        _arrowDown?.Dispose()
        _arrowUp?.Dispose()
        Dim k_color As Color = ThemeManager.Current.Palette.TextColor
        Dim k_side As Integer = ThemeShapes.ScaleDpi(Me, 18)
        _arrowDown = Tint(My.Resources.Resources.down, k_color, k_side)
        _arrowUp = DirectCast(_arrowDown.Clone(), Bitmap)
        _arrowUp.RotateFlip(RotateFlipType.Rotate180FlipNone)
    End Sub

    ' Same alpha as the source, every pixel in one colour.
    Private Shared Function Tint(k_source As Image, k_color As Color, k_side As Integer) As Bitmap
        Dim k_result As New Bitmap(k_side, k_side)
        Using k_g As Graphics = Graphics.FromImage(k_result)
            Dim k_matrix As New Imaging.ColorMatrix(New Single()() {
                New Single() {0, 0, 0, 0, 0},
                New Single() {0, 0, 0, 0, 0},
                New Single() {0, 0, 0, 0, 0},
                New Single() {0, 0, 0, 1, 0},
                New Single() {k_color.R / 255.0F, k_color.G / 255.0F, k_color.B / 255.0F, 0, 1}})
            Using k_attrs As New Imaging.ImageAttributes()
                k_attrs.SetColorMatrix(k_matrix)
                k_g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                k_g.DrawImage(k_source, New Rectangle(0, 0, k_side, k_side), 0, 0, k_source.Width, k_source.Height, GraphicsUnit.Pixel, k_attrs)
            End Using
        End Using
        Return k_result
    End Function

    Private Sub ShowNewsArrow(k_folded As Boolean)
        If _arrowDown Is Nothing Then BuildNewsArrows()
        btnNoutati.Image = If(k_folded, _arrowDown, _arrowUp)
    End Sub

    ' No caret, no selection, no focus: the text is only read (the wheel still scrolls it).
    Private Sub MakeNewsReadOnly()
        Dim k_box As RichTextBox = edNoutati.TextBox
        k_box.TabStop = False
        k_box.Cursor = Cursors.Default
        k_box.ShortcutsEnabled = False
        _newsFilter = New NoInteractionFilter()
        If k_box.IsHandleCreated Then _newsFilter.AssignHandle(k_box.Handle)
        AddHandler k_box.HandleCreated, Sub(k_s, k_e) _newsFilter.AssignHandle(k_box.Handle)
        AddHandler k_box.HandleDestroyed, Sub(k_s, k_e) _newsFilter.ReleaseHandle()
    End Sub

    ''' <summary>Swallows the mouse buttons, the context menu and the focus message of a control: it can be scrolled, nothing more.</summary>
    Private NotInheritable Class NoInteractionFilter
        Inherits NativeWindow

        Private Const WM_SETFOCUS As Integer = &H7
        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_LBUTTONDBLCLK As Integer = &H203
        Private Const WM_RBUTTONDOWN As Integer = &H204
        Private Const WM_RBUTTONDBLCLK As Integer = &H206
        Private Const WM_MBUTTONDOWN As Integer = &H207
        Private Const WM_CONTEXTMENU As Integer = &H7B

        Protected Overrides Sub WndProc(ByRef k_m As Message)
            Select Case k_m.Msg
                Case WM_SETFOCUS, WM_LBUTTONDOWN, WM_LBUTTONDBLCLK, WM_RBUTTONDOWN, WM_RBUTTONDBLCLK, WM_MBUTTONDOWN, WM_CONTEXTMENU
                    k_m.Result = IntPtr.Zero
                Case Else
                    MyBase.WndProc(k_m)
            End Select
        End Sub
    End Class

    ' ---- Discutii: the robot and its speech bubble (slice CHAT-08) ---------------------------------

    Private Sub HookChat()
        AddHandler _chat.StateChanged, AddressOf Chat_StateChanged
        AddHandler _chat.MessagesReceived, AddressOf Chat_MessagesReceived
        AddHandler _chat.Failed, AddressOf Chat_Failed
        AddHandler _chat.BusyChanged, AddressOf Chat_BusyChanged
        ' Once, for the whole run; a second page asking for it changes nothing.
        _chat.Start()
        If _chat.State IsNot Nothing Then Chat_StateChanged(_chat, _chat.State)
    End Sub

    Private Sub UnhookChat()
        RemoveHandler _chat.StateChanged, AddressOf Chat_StateChanged
        RemoveHandler _chat.MessagesReceived, AddressOf Chat_MessagesReceived
        RemoveHandler _chat.Failed, AddressOf Chat_Failed
        RemoveHandler _chat.BusyChanged, AddressOf Chat_BusyChanged
    End Sub

    ' The numbers of the news: the badge on the robot, who the operator is in the bubble.
    Private Sub Chat_StateChanged(sender As Object, e As ChatState)
        Try
            Dim k_more As Boolean = e.unread > picLogo.UnreadCount AndAlso picLogo.UnreadCount > 0
            picLogo.UnreadCount = e.unread
            If k_more AndAlso Visible Then picLogo.Nudge()
            bubble.SetState(e)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Chat_StateChanged", ex)
        End Try
    End Sub

    Private Sub Chat_MessagesReceived(sender As Object, e As ChatMessagesEventArgs)
        Try
            Select Case e.Mode
                Case ChatLoadMode.Latest
                    bubble.ResetMessages()
                    bubble.AddMessages(e.Messages, False)
                    bubble.SetHasOlder(e.HasMore)
                Case ChatLoadMode.Newer
                    bubble.AddMessages(e.Messages, False)
                Case ChatLoadMode.Older
                    bubble.AddMessages(e.Messages, True)
                    bubble.SetHasOlder(e.HasMore)
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Chat_MessagesReceived", ex)
        End Try
    End Sub

    Private Sub Chat_Failed(sender As Object, e As String)
        Try
            If bubble.Visible Then bubble.SetStatus(e, True)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Chat_Failed", ex)
        End Try
    End Sub

    Private Sub Chat_BusyChanged(sender As Object, e As Boolean)
        Try
            bubble.SetBusy(e)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Chat_BusyChanged", ex)
        End Try
    End Sub

    Private Sub PicLogo_Click(sender As Object, e As EventArgs) Handles picLogo.Click
        If bubble.Visible Then CloseBubble() Else OpenBubble()
    End Sub

    Private Async Sub OpenBubble()
        Try
            bubble.SetStatus(String.Empty, False)
            PlaceBubble()
            bubble.Visible = True
            bubble.BringToFront()
            ListenForOutsideClicks()
            _chat.SetOpen(True)
            Await _chat.OpenAsync()
            bubble.ScrollToEnd()
            bubble.FocusComposer()
        Catch ex As Exception
            ' UI boundary (async void): log and swallow.
            GlobalErrorLog.Write("AcasaPage.OpenBubble", ex)
        End Try
    End Sub

    Private Sub CloseBubble()
        Try
            bubble.Visible = False
            StopListeningForOutsideClicks()
            _chat.SetOpen(False)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.CloseBubble", ex)
        End Try
    End Sub

    ' The bubble stands above the robot, its tail's tip on the robot's head. Sizes are logical (96 dpi) scaled to this screen.
    Private Sub PlaceBubble()
        Dim k_scale As Single = DeviceDpi / 96.0F
        Dim k_margin As Integer = CInt(8 * k_scale)
        Dim k_width As Integer = CInt(380 * k_scale)
        Dim k_tipX As Integer = picLogo.Left + picLogo.Width \ 2
        Dim k_tipY As Integer = picLogo.Top + CInt(picLogo.Height * 0.12)
        Dim k_height As Integer = Math.Max(CInt(240 * k_scale), Math.Min(CInt(520 * k_scale), k_tipY - k_margin))
        Dim k_left As Integer = Math.Max(k_margin, k_tipX + bubble.TailFromRight - k_width)
        Dim k_top As Integer = Math.Max(k_margin, k_tipY - k_height)
        bubble.SetBounds(k_left, k_top, k_width, k_height)
    End Sub

    Private Sub PnlCentru_Resize(sender As Object, e As EventArgs) Handles pnlCentru.Resize
        If bubble.Visible Then PlaceBubble()
    End Sub

    Private Sub Bubble_CloseRequested(sender As Object, e As EventArgs) Handles bubble.CloseRequested
        CloseBubble()
    End Sub

    Private Async Sub Bubble_OlderRequested(sender As Object, e As EventArgs) Handles bubble.OlderRequested
        Try
            Await _chat.OlderAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Bubble_OlderRequested", ex)
        End Try
    End Sub

    Private Async Sub Bubble_SendRequested(sender As Object, e As ChatSendEventArgs) Handles bubble.SendRequested
        Try
            If Await _chat.SendAsync(e.Text, e.Anonymous, e.DisplayName, e.ReplyTo) Then
                bubble.ClearDraft()
                bubble.ScrollToEnd()
            End If
            bubble.FocusComposer()
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Bubble_SendRequested", ex)
        End Try
    End Sub

    Private Async Sub Bubble_FilterChanged(sender As Object, e As IReadOnlyList(Of String)) Handles bubble.FilterChanged
        Try
            Await _chat.SaveApplicationsAsync(e)
            ' A refusal leaves the server's set as it was: show that one again, not the box the operator just ticked.
            If _chat.State IsNot Nothing Then bubble.SetState(_chat.State)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Bubble_FilterChanged", ex)
        End Try
    End Sub

    Private Async Sub Bubble_AnonymousChanged(sender As Object, e As EventArgs) Handles bubble.AnonymousChanged
        Try
            Await _chat.SaveAnonymousAsync(bubble.Anonymous)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.Bubble_AnonymousChanged", ex)
        End Try
    End Sub

    ' A press anywhere in K-BOT outside the bubble closes it. The press still goes on to what was under it (a button, a
    ' list), and the robot is left out: its own click opens or closes the bubble.
    Private Sub ListenForOutsideClicks()
        If _outsideClick IsNot Nothing Then Return
        _outsideClick = New OutsideClickFilter(AddressOf IsOutsideBubble, Sub() BeginInvoke(New Action(AddressOf CloseBubble)))
        Application.AddMessageFilter(_outsideClick)
    End Sub

    Private Sub StopListeningForOutsideClicks()
        If _outsideClick Is Nothing Then Return
        Application.RemoveMessageFilter(_outsideClick)
        _outsideClick = Nothing
    End Sub

    Private Function IsOutsideBubble() As Boolean
        Dim k_point As Point = Control.MousePosition
        If bubble.RectangleToScreen(bubble.ClientRectangle).Contains(k_point) Then Return False
        If picLogo.RectangleToScreen(picLogo.ClientRectangle).Contains(k_point) Then Return False
        Return True
    End Function

    ''' <summary>Looks at the mouse presses of the whole application (never swallows one) and says when one falls outside.</summary>
    Private NotInheritable Class OutsideClickFilter
        Implements IMessageFilter

        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_RBUTTONDOWN As Integer = &H204
        Private Const WM_MBUTTONDOWN As Integer = &H207
        Private Const WM_NCLBUTTONDOWN As Integer = &HA1
        Private Const WM_NCRBUTTONDOWN As Integer = &HA4

        Private ReadOnly _isOutside As Func(Of Boolean)
        Private ReadOnly _onOutside As Action

        Public Sub New(k_isOutside As Func(Of Boolean), k_onOutside As Action)
            _isOutside = k_isOutside
            _onOutside = k_onOutside
        End Sub

        Public Function PreFilterMessage(ByRef k_m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            Select Case k_m.Msg
                Case WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN, WM_NCLBUTTONDOWN, WM_NCRBUTTONDOWN
                    Try
                        If _isOutside() Then _onOutside()
                    Catch ex As Exception
                        GlobalErrorLog.Write("AcasaPage.OutsideClickFilter", ex)
                    End Try
            End Select
            Return False
        End Function
    End Class

    ' A page that is not on screen has no open bubble: the service goes back to asking only for the count.
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        Try
            If Not Visible AndAlso bubble IsNot Nothing AndAlso bubble.Visible Then CloseBubble()
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.OnVisibleChanged", ex)
        End Try
    End Sub

    ' ---- Theme -------------------------------------------------------------------------------

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_palette As ThemePalette = ThemeManager.Current.Palette
            pnlNoutati.BackColor = k_palette.SurfaceColor
            pnlNoutatiHead.BackColor = k_palette.SurfaceColor
            lblNoutati.BackColor = k_palette.SurfaceColor
            lblNoutati.ForeColor = k_palette.TextColor
            ButtonStyles.ApplyTransparent(btnNoutati, ThemeManager.Current, k_palette.SurfaceColor, k_palette.TextColor)
            BuildNewsArrows()
            ShowNewsArrow(Not edNoutati.Visible)
        Catch ex As Exception
            GlobalErrorLog.Write("AcasaPage.OnThemeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        UnhookChat()
        StopListeningForOutsideClicks()
        _arrowDown?.Dispose()
        _arrowUp?.Dispose()
        MyBase.OnFormClosed(e)
    End Sub
End Class
