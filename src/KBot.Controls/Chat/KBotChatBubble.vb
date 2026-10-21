Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

''' <summary>What the operator asked to send from the bubble.</summary>
Public NotInheritable Class ChatSendEventArgs
    Inherits EventArgs

    Public ReadOnly Property Text As String
    Public ReadOnly Property Anonymous As Boolean
    Public ReadOnly Property DisplayName As String
    ''' <summary>The id of the message this one answers, or Nothing.</summary>
    Public ReadOnly Property ReplyTo As String

    Public Sub New(k_text As String, k_anonymous As Boolean, k_displayName As String, k_replyTo As String)
        Text = k_text
        Anonymous = k_anonymous
        DisplayName = k_displayName
        ReplyTo = k_replyTo
    End Sub
End Class

''' <summary>
''' The window of the shared chat (slice CHAT-08), drawn as a speech bubble: a rounded body with a tail that points
''' at the robot it comes out of. Inside: the title with the close button, the messages, and what the operator types
''' (pseudonym or name, the text, «Trimite»). It only shows and asks; it never talks to the server -- the chat
''' service does, and this control raises <see cref="SendRequested"/>, <see cref="CloseRequested"/>,
''' <see cref="OlderRequested"/> and <see cref="AnonymousChanged"/> for it.
''' </summary>
''' <remarks>
''' Owns child controls, so it implements <see cref="IThemedControl"/> and colours them itself. The tail sits at a
''' fixed distance from the right edge (<see cref="TailFromRight"/>, logical px): the host places the bubble so that
''' the tip lands on the robot.
''' </remarks>
<ToolboxItem(False)>
Partial Public Class KBotChatBubble
    Implements IThemedControl

    Private Const BorderPx As Integer = 1        ' logical
    Private Const AirPx As Integer = 10          ' logical, between the outline and the content
    Private Const RadiusPx As Integer = 12       ' logical
    Private Const TailHeightPx As Integer = 16   ' logical
    Private Const TailWidthPx As Integer = 22    ' logical
    Private Const TailFromRightPx As Integer = 56 ' logical, from the right edge to the tip

    Private _ids As New HashSet(Of String)(StringComparer.Ordinal)
    Private _messages As New List(Of ChatMessage)()
    Private _fontBold As Font
    Private _fontSmall As Font
    Private _fontGap As Font
    Private _fontLink As Font
    Private _replyTo As ChatMessage
    Private ReadOnly _replyLinks As New List(Of ReplyLink)()
    Private _applyingFilter As Boolean
    Private _fontsFor As Single
    Private _ready As Boolean
    Private _busy As Boolean
    Private _settingsApplied As Boolean
    Private _scheme As ThemeScheme

    ''' <summary>Asks to send a message (text already trimmed, never empty).</summary>
    Public Event SendRequested As EventHandler(Of ChatSendEventArgs)
    ''' <summary>Asks to close the bubble (the × button, or Esc).</summary>
    Public Event CloseRequested As EventHandler
    ''' <summary>Asks for the page of messages before the oldest one shown.</summary>
    Public Event OlderRequested As EventHandler
    ''' <summary>The pseudonym box was ticked or cleared (the new value is <see cref="Anonymous"/>).</summary>
    Public Event AnonymousChanged As EventHandler
    ''' <summary>The operator changed which applications' messages are shown (the new set, never empty: V, A, K).</summary>
    Public Event FilterChanged As EventHandler(Of IReadOnlyList(Of String))

    ' Where the word «Răspunde» sits in the text of the list, and the message it answers.
    Private Structure ReplyLink
        Public Start As Integer
        Public Finish As Integer
        Public Message As ChatMessage
    End Structure

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.SupportsTransparentBackColor, True)
        InitializeComponent()
        BackColor = Color.Transparent
        edLog.Editabil = False
        edLog.TextBox.TabStop = False
        AddHandler edLog.TextBox.MouseClick, AddressOf Log_MouseClick
        AddHandler edLog.TextBox.MouseMove, AddressOf Log_MouseMove
        UpdateIdentityRow()
        UpdateComposerHeight()
        SetStatus(String.Empty, False)
        LayoutInner()
    End Sub

    ' ---- Public surface ------------------------------------------------------------------------

    ''' <summary>Distance, in device pixels, from the right edge of the control to the tip of the tail.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property TailFromRight As Integer
        Get
            Return ThemeShapes.ScaleDpi(Me, TailFromRightPx)
        End Get
    End Property

    ''' <summary>Height, in device pixels, of the tail (the bubble's body ends this far above the bottom).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property TailHeight As Integer
        Get
            Return ThemeShapes.ScaleDpi(Me, TailHeightPx)
        End Get
    End Property

    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Anonymous As Boolean
        Get
            Return chkAnonymous.Checked
        End Get
    End Property

    ''' <summary>The state arrived: show who the operator is and, the first time, their saved choices.</summary>
    Public Sub SetState(k_state As ChatState)
        Try
            If k_state Is Nothing Then Throw New ArgumentNullException(NameOf(k_state))
            _ready = True
            _pseudonym = k_state.self.pseudonym
            If Not _settingsApplied Then
                _settingsApplied = True
                RemoveHandler chkAnonymous.CheckedChanged, AddressOf ChkAnonymous_CheckedChanged
                chkAnonymous.Checked = k_state.preferences.anonymous
                AddHandler chkAnonymous.CheckedChanged, AddressOf ChkAnonymous_CheckedChanged
                If Not txtName.InnerTextBox.Focused Then txtName.Text = k_state.self.display_name
                UpdateIdentityRow()
            End If
            lblPseudonym.Text = If(chkAnonymous.Checked, _pseudonym, String.Empty)
            _applyingFilter = True
            Try
                For Each k_box As KBotCheckBox In FilterBoxes()
                    k_box.Checked = k_state.preferences.applications.Contains(CStr(k_box.Tag))
                Next
            Finally
                _applyingFilter = False
            End Try
            ApplyEnabled()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.SetState", ex)
        End Try
    End Sub

    Private _pseudonym As String = String.Empty

    ''' <summary>Forgets every message shown (the list is about to be loaded again).</summary>
    Public Sub ResetMessages()
        _ids.Clear()
        _messages.Clear()
        _replyLinks.Clear()
        edLog.TextBox.Clear()
        lblOlder.Visible = False
    End Sub

    ''' <summary>Adds messages (oldest first). <paramref name="k_older"/> = they come before the ones shown.</summary>
    Public Sub AddMessages(k_added As IEnumerable(Of ChatMessage), k_older As Boolean)
        Try
            If k_added Is Nothing Then Return
            Dim k_fresh As New List(Of ChatMessage)()
            For Each k_m As ChatMessage In k_added
                If k_m IsNot Nothing AndAlso _ids.Add(k_m.id) Then k_fresh.Add(k_m)
            Next
            If k_fresh.Count = 0 Then Return
            Dim k_box As RichTextBox = edLog.TextBox
            If k_older Then
                _messages.InsertRange(0, k_fresh)
                RenderAll(k_fresh.Count)
            Else
                _messages.AddRange(k_fresh)
                Dim k_stick As Boolean = _messages.Count = k_fresh.Count OrElse AtBottom()
                EnsureFonts()
                For Each k_m As ChatMessage In k_fresh
                    AppendMessage(k_box, k_m)
                Next
                If k_stick Then ScrollToEnd()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.AddMessages", ex)
        End Try
    End Sub

    ''' <summary>The id of the newest message shown (a decimal string), or Nothing when there is none.</summary>
    Public Function NewestId() As String
        Dim k_best As String = Nothing
        For Each k_m As ChatMessage In _messages
            If k_best Is Nothing OrElse IsGreater(k_m.id, k_best) Then k_best = k_m.id
        Next
        Return k_best
    End Function

    ''' <summary>The id of the oldest message shown, or Nothing.</summary>
    Public Function OldestId() As String
        Dim k_best As String = Nothing
        For Each k_m As ChatMessage In _messages
            If k_best Is Nothing OrElse IsGreater(k_best, k_m.id) Then k_best = k_m.id
        Next
        Return k_best
    End Function

    Public Sub SetHasOlder(k_value As Boolean)
        lblOlder.Visible = k_value
    End Sub

    ''' <summary>One line under the messages: a hint or an error (red). Empty = hidden.</summary>
    Public Sub SetStatus(k_text As String, k_isError As Boolean)
        lblStatus.Text = If(k_text, String.Empty)
        lblStatus.Visible = lblStatus.Text.Length > 0
        lblStatus.ForeColor = If(k_isError, CurrentPalette().ErrorColor, CurrentPalette().TextDimColor)
    End Sub

    ''' <summary>True while a call is running: the typing area waits.</summary>
    Public Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        ApplyEnabled()
    End Sub

    ''' <summary>Empties what was typed (after a send), and the answer being written.</summary>
    Public Sub ClearDraft()
        txtMessage.Text = String.Empty
        ClearReply()
    End Sub

    ''' <summary>The applications whose messages are ticked, in the order V, A, K.</summary>
    Public Function SelectedApplications() As List(Of String)
        Dim k_list As New List(Of String)()
        For Each k_box As KBotCheckBox In FilterBoxes()
            If k_box.Checked Then k_list.Add(CStr(k_box.Tag))
        Next
        Return k_list
    End Function

    Private Function FilterBoxes() As KBotCheckBox()
        Return New KBotCheckBox() {chkVercon, chkAdechit, chkKbot}
    End Function

    ''' <summary>Puts the caret in the box the operator types the message in.</summary>
    Public Sub FocusComposer()
        If txtMessage.CanFocus Then txtMessage.FocusInput()
    End Sub

    ''' <summary>The messages go to the end (after the operator opened the bubble, or sent one).</summary>
    Public Sub ScrollToEnd()
        Try
            Dim k_box As RichTextBox = edLog.TextBox
            If Not k_box.IsHandleCreated Then Return
            k_box.SelectionStart = k_box.TextLength
            k_box.SelectionLength = 0
            k_box.ScrollToCaret()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.ScrollToEnd", ex)
        End Try
    End Sub

    ' ---- Theme ---------------------------------------------------------------------------------

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            _scheme = k_scheme
            Dim k_p As ThemePalette = CurrentPalette()
            For Each k_panel As Panel In New Panel() {pnlInner, pnlLog, pnlComposer, pnlSend, pnlIdentity, pnlFilter, pnlReply, pnlHeader}
                k_panel.BackColor = k_p.SurfaceColor
            Next
            lblTitle.BackColor = k_p.SurfaceColor
            lblTitle.ForeColor = k_p.TextColor
            lblOlder.BackColor = k_p.SurfaceColor
            lblOlder.ForeColor = k_p.AccentColor
            lblStatus.BackColor = k_p.SurfaceColor
            lblPseudonym.BackColor = k_p.SurfaceColor
            lblPseudonym.ForeColor = k_p.TextDimColor
            lblHint.BackColor = k_p.SurfaceColor
            lblHint.ForeColor = k_p.TextDimColor
            lblFilter.BackColor = k_p.SurfaceColor
            lblFilter.ForeColor = k_p.TextDimColor
            lblReply.BackColor = k_p.SurfaceColor
            lblReply.ForeColor = k_p.TextDimColor
            If k_scheme IsNot Nothing Then
                For Each k_child As Control In New Control() {edLog, btnSend, btnClose, btnReplyCancel, txtMessage, txtName, chkAnonymous, chkVercon, chkAdechit, chkKbot}
                    Dim k_themed As IThemedControl = TryCast(k_child, IThemedControl)
                    If k_themed IsNot Nothing Then k_themed.ApplyTheme(k_scheme)
                Next
            End If
            For Each k_check As KBotCheckBox In New KBotCheckBox() {chkAnonymous, chkVercon, chkAdechit, chkKbot}
                k_check.BackColor = k_p.SurfaceColor
            Next
            SetStatus(lblStatus.Text, lblStatus.ForeColor = k_p.ErrorColor)
            If _messages.Count > 0 Then RenderAll(0)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.ApplyTheme", ex)
        End Try
    End Sub

    Private Function CurrentPalette() As ThemePalette
        Return If(_scheme IsNot Nothing, _scheme.Palette, ThemeManager.Current.Palette)
    End Function

    ' ---- Shape ---------------------------------------------------------------------------------

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            MyBase.OnPaint(e)
            Dim k_p As ThemePalette = CurrentPalette()
            Dim k_g As Graphics = e.Graphics
            k_g.SmoothingMode = SmoothingMode.AntiAlias
            Using k_path As GraphicsPath = BuildOutline()
                Using k_fill As New SolidBrush(k_p.SurfaceColor)
                    k_g.FillPath(k_fill, k_path)
                End Using
                Using k_pen As New Pen(k_p.BorderColor, Math.Max(1, ThemeShapes.ScaleDpi(Me, BorderPx)))
                    k_pen.LineJoin = LineJoin.Round
                    k_g.DrawPath(k_pen, k_path)
                End Using
            End Using
        Catch ex As Exception
            ' UI boundary (OnPaint): log and swallow.
            GlobalErrorLog.Write("KBotChatBubble.OnPaint", ex)
        End Try
    End Sub

    ' Rounded body with the tail cut into the bottom edge: one closed outline, so the border has no seam where the tail joins.
    Private Function BuildOutline() As GraphicsPath
        Dim k_r As Single = ThemeShapes.ScaleDpi(Me, RadiusPx)
        Dim k_d As Single = k_r * 2.0F
        Dim k_tail As Single = ThemeShapes.ScaleDpi(Me, TailHeightPx)
        Dim k_x0 As Single = 0.5F
        Dim k_y0 As Single = 0.5F
        Dim k_x1 As Single = Width - 1.5F
        Dim k_y1 As Single = Height - k_tail - 1.5F
        Dim k_tipX As Single = Width - ThemeShapes.ScaleDpi(Me, TailFromRightPx)
        Dim k_w As Single = ThemeShapes.ScaleDpi(Me, TailWidthPx)
        Dim k_right As Single = Math.Min(k_tipX + k_w * 0.2F, k_x1 - k_r)   ' the tail leans toward the right, toward the robot
        Dim k_left As Single = k_tipX - k_w * 0.8F

        Dim k_path As New GraphicsPath()
        k_path.AddArc(k_x0, k_y0, k_d, k_d, 180, 90)
        k_path.AddArc(k_x1 - k_d, k_y0, k_d, k_d, 270, 90)
        k_path.AddArc(k_x1 - k_d, k_y1 - k_d, k_d, k_d, 0, 90)
        k_path.AddLine(k_right, k_y1, k_tipX + k_w * 0.1F, k_y1 + k_tail)
        k_path.AddLine(k_tipX + k_w * 0.1F, k_y1 + k_tail, k_left, k_y1)
        k_path.AddArc(k_x0, k_y1 - k_d, k_d, k_d, 90, 90)
        k_path.CloseFigure()
        Return k_path
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        LayoutInner()
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        LayoutInner()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(e As EventArgs)
        MyBase.OnDpiChangedAfterParent(e)
        LayoutInner()
    End Sub

    ' The content sits inside the outline and above the tail. Set by hand (not Padding): Padding is scaled by the
    ' form's own passes and would drift; this is recomputed from the logical numbers every time.
    Private Sub LayoutInner()
        Dim k_in As Integer = ThemeShapes.ScaleDpi(Me, BorderPx + AirPx)
        Dim k_tail As Integer = ThemeShapes.ScaleDpi(Me, TailHeightPx)
        pnlInner.SetBounds(k_in, k_in, Math.Max(0, Width - 2 * k_in), Math.Max(0, Height - k_tail - 2 * k_in))
    End Sub

    ' ---- Messages ------------------------------------------------------------------------------

    Private Sub EnsureFonts()
        Dim k_base As Font = edLog.TextBox.Font
        If _fontBold IsNot Nothing AndAlso _fontsFor = k_base.SizeInPoints Then Return
        _fontBold?.Dispose()
        _fontSmall?.Dispose()
        _fontGap?.Dispose()
        _fontLink?.Dispose()
        _fontLink = New Font(k_base.FontFamily, k_base.SizeInPoints * 0.85F, FontStyle.Underline)
        _fontBold = New Font(k_base, FontStyle.Bold)
        _fontSmall = New Font(k_base.FontFamily, k_base.SizeInPoints * 0.85F, FontStyle.Regular)
        _fontGap = New Font(k_base.FontFamily, k_base.SizeInPoints * 0.4F, FontStyle.Regular)
        _fontsFor = k_base.SizeInPoints
    End Sub

    ' Rebuilds the whole list. k_markFirst = how many messages from the top were just added in front: the view then
    ' stays on the message the operator was reading (the old top), instead of jumping to the start or the end.
    Private Sub RenderAll(k_markFirst As Integer)
        Dim k_box As RichTextBox = edLog.TextBox
        EnsureFonts()
        k_box.Clear()
        _replyLinks.Clear()
        Dim k_mark As Integer = 0
        For k_i As Integer = 0 To _messages.Count - 1
            AppendMessage(k_box, _messages(k_i))
            If k_markFirst > 0 AndAlso k_i = k_markFirst - 1 Then k_mark = k_box.TextLength
        Next
        If Not k_box.IsHandleCreated Then Return
        If k_markFirst > 0 Then
            k_box.SelectionStart = k_mark
            k_box.SelectionLength = 0
            k_box.ScrollToCaret()
        Else
            ScrollToEnd()
        End If
    End Sub

    Private Sub AppendMessage(k_box As RichTextBox, k_m As ChatMessage)
        Dim k_p As ThemePalette = CurrentPalette()
        AppendRun(k_box, "[" & k_m.application & "] ", _fontBold, k_p.TextDimColor)
        AppendRun(k_box, k_m.author, _fontBold, If(k_m.mine, k_p.AccentColor, k_p.TextColor))
        AppendRun(k_box, "   " & TimeText(k_m.created_at) & ControlChars.Lf, _fontSmall, k_p.TextDimColor)
        If k_m.reply IsNot Nothing Then
            AppendRun(k_box, "Răspuns pentru " & k_m.reply.author & ": " & k_m.reply.text & ControlChars.Lf, _fontSmall, k_p.TextDimColor)
        End If
        AppendRun(k_box, k_m.text & ControlChars.Lf, k_box.Font, k_p.TextColor)
        ' «Răspunde» is clickable: where it stands is remembered, the click is found by position (Log_MouseClick).
        Dim k_start As Integer = k_box.TextLength
        AppendRun(k_box, "Răspunde", _fontLink, k_p.AccentColor)
        _replyLinks.Add(New ReplyLink With {.Start = k_start, .Finish = k_box.TextLength, .Message = k_m})
        AppendRun(k_box, ControlChars.Lf & ControlChars.Lf, _fontGap, k_p.TextColor)
    End Sub

    Private Shared Sub AppendRun(k_box As RichTextBox, k_text As String, k_font As Font, k_color As Color)
        k_box.SelectionStart = k_box.TextLength
        k_box.SelectionLength = 0
        k_box.SelectionFont = k_font
        k_box.SelectionColor = k_color
        k_box.SelectedText = k_text
    End Sub

    Private Shared Function TimeText(k_iso As String) As String
        Dim k_when As DateTimeOffset
        If DateTimeOffset.TryParse(k_iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, k_when) Then
            Return k_when.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)
        End If
        Return String.Empty
    End Function

    Private Function AtBottom() As Boolean
        Dim k_box As RichTextBox = edLog.TextBox
        If Not k_box.IsHandleCreated OrElse k_box.TextLength = 0 Then Return True
        ' The last character is on screen when the view reaches the end of the text.
        Dim k_last As Point = k_box.GetPositionFromCharIndex(Math.Max(0, k_box.TextLength - 1))
        Return k_last.Y < k_box.ClientSize.Height + 2 * k_box.Font.Height
    End Function

    ' Ids are decimal strings that can pass what a Long holds only in theory; compare by length, then text.
    Private Shared Function IsGreater(k_a As String, k_b As String) As Boolean
        If k_a.Length <> k_b.Length Then Return k_a.Length > k_b.Length
        Return String.CompareOrdinal(k_a, k_b) > 0
    End Function

    ' ---- Typing --------------------------------------------------------------------------------

    Private Sub ApplyEnabled()
        Dim k_on As Boolean = _ready AndAlso Not _busy
        chkAnonymous.Enabled = k_on
        For Each k_box As KBotCheckBox In FilterBoxes()
            k_box.Enabled = k_on
        Next
        txtName.Enabled = k_on
        txtMessage.Enabled = k_on
        btnSend.Enabled = k_on
    End Sub

    Private Sub UpdateIdentityRow()
        Dim k_named As Boolean = Not chkAnonymous.Checked
        If txtName.Visible = k_named Then
            lblPseudonym.Text = If(k_named, String.Empty, _pseudonym)
            Return
        End If
        txtName.Visible = k_named
        lblPseudonym.Text = If(k_named, String.Empty, _pseudonym)
        UpdateComposerHeight()
    End Sub

    ' The typing area is as tall as the rows it shows: answer strip, pseudonym row, name, message, «Trimite».
    Private Sub UpdateComposerHeight()
        Dim k_height As Integer = pnlIdentity.Height + txtMessage.Height + pnlSend.Height
        If txtName.Visible Then k_height += txtName.Height
        If pnlReply.Visible Then k_height += pnlReply.Height
        pnlComposer.Height = k_height
    End Sub

    Private Sub SetReply(k_message As ChatMessage)
        _replyTo = k_message
        lblReply.Text = "Răspuns pentru " & k_message.author & ": " & k_message.text.Replace(ControlChars.Lf, " "c).Replace(ControlChars.Cr, " "c)
        pnlReply.Visible = True
        UpdateComposerHeight()
        FocusComposer()
    End Sub

    Private Sub ClearReply()
        _replyTo = Nothing
        lblReply.Text = String.Empty
        pnlReply.Visible = False
        UpdateComposerHeight()
    End Sub

    ' The message whose «Răspunde» is under the point, or Nothing. The nearest character is not enough (a click in the
    ' empty part of the line also lands on one): the point must be on the word.
    Private Function ReplyLinkAt(k_point As Point) As ChatMessage
        Dim k_box As RichTextBox = edLog.TextBox
        If _replyLinks.Count = 0 OrElse k_box.TextLength = 0 OrElse _fontLink Is Nothing Then Return Nothing
        Dim k_index As Integer = k_box.GetCharIndexFromPosition(k_point)
        For Each k_link As ReplyLink In _replyLinks
            If k_index < k_link.Start OrElse k_index >= k_link.Finish Then Continue For
            Dim k_a As Point = k_box.GetPositionFromCharIndex(k_link.Start)
            Dim k_b As Point = k_box.GetPositionFromCharIndex(k_link.Finish - 1)
            If k_point.Y >= k_a.Y AndAlso k_point.Y <= k_a.Y + CInt(_fontLink.Height * 1.5) AndAlso
               k_point.X >= k_a.X AndAlso k_point.X <= k_b.X + _fontLink.Height Then
                Return k_link.Message
            End If
        Next
        Return Nothing
    End Function

    Private Sub Log_MouseClick(sender As Object, e As MouseEventArgs)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim k_message As ChatMessage = ReplyLinkAt(e.Location)
            If k_message IsNot Nothing Then SetReply(k_message)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.Log_MouseClick", ex)
        End Try
    End Sub

    Private Sub Log_MouseMove(sender As Object, e As MouseEventArgs)
        Try
            edLog.TextBox.Cursor = If(ReplyLinkAt(e.Location) IsNot Nothing, Cursors.Hand, Cursors.IBeam)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.Log_MouseMove", ex)
        End Try
    End Sub

    Private Sub Filter_CheckedChanged(sender As Object, e As EventArgs) Handles chkVercon.CheckedChanged, chkAdechit.CheckedChanged, chkKbot.CheckedChanged
        Try
            If _applyingFilter Then Return
            Dim k_selected As List(Of String) = SelectedApplications()
            If k_selected.Count = 0 Then
                ' At least one application stays ticked: put the one just cleared back.
                _applyingFilter = True
                Try
                    DirectCast(sender, KBotCheckBox).Checked = True
                Finally
                    _applyingFilter = False
                End Try
                SetStatus("Păstrați cel puțin o aplicație bifată.", True)
                Return
            End If
            SetStatus(String.Empty, False)
            RaiseEvent FilterChanged(Me, k_selected)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.Filter_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub BtnReplyCancel_Click(sender As Object, e As EventArgs) Handles btnReplyCancel.Click
        ClearReply()
        FocusComposer()
    End Sub

    Private Sub ChkAnonymous_CheckedChanged(sender As Object, e As EventArgs) Handles chkAnonymous.CheckedChanged
        Try
            UpdateIdentityRow()
            RaiseEvent AnonymousChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.ChkAnonymous_CheckedChanged", ex)
        End Try
    End Sub

    Private Sub BtnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        RequestSend()
    End Sub

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        RaiseEvent CloseRequested(Me, EventArgs.Empty)
    End Sub

    Private Sub LblOlder_Click(sender As Object, e As EventArgs) Handles lblOlder.Click
        RaiseEvent OlderRequested(Me, EventArgs.Empty)
    End Sub

    ' Enter sends, Ctrl+Enter adds a line, Esc closes.
    Private Sub TxtMessage_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtMessage.FieldKeyDown
        Try
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                e.Handled = True
                If e.Control Then
                    txtMessage.InnerTextBox.SelectedText = Environment.NewLine
                Else
                    RequestSend()
                End If
            ElseIf e.KeyCode = Keys.Escape Then
                e.SuppressKeyPress = True
                e.Handled = True
                RaiseEvent CloseRequested(Me, EventArgs.Empty)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.TxtMessage_FieldKeyDown", ex)
        End Try
    End Sub

    Private Sub RequestSend()
        Try
            If Not _ready OrElse _busy Then Return
            Dim k_text As String = txtMessage.Text.Trim()
            If k_text.Length = 0 Then
                SetStatus("Scrieți mesajul.", True)
                txtMessage.FocusInput()
                Return
            End If
            Dim k_name As String = txtName.Text.Trim()
            If Not chkAnonymous.Checked AndAlso k_name.Length = 0 Then
                SetStatus("Completați numele afișat sau bifați «Cu pseudonim».", True)
                txtName.FocusInput()
                Return
            End If
            SetStatus(String.Empty, False)
            RaiseEvent SendRequested(Me, New ChatSendEventArgs(k_text, chkAnonymous.Checked, k_name, _replyTo?.id))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatBubble.RequestSend", ex)
        End Try
    End Sub

End Class
