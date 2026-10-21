Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The K-BOT mark in the corner of the opening page, and the face of the chat (slice CHAT-08). With no unread
''' message it is the picture and nothing else. With unread messages it does a short hop-and-wobble now and
''' then, and a round badge with the number sits on its corner (the badge stays still while the picture moves).
''' A click is the operator asking to read: the host opens the speech bubble.
''' </summary>
''' <remarks>
''' One timer, and only while the robot is on screen with something unread: a 33 ms tick during a burst, a
''' long wait between bursts. If Windows has animations switched off (<see cref="SystemInformation.UIEffectsEnabled"/>)
''' the picture never moves; the badge still tells the news.
''' </remarks>
<ToolboxItem(True)>
<DefaultEvent("Click")>
Public NotInheritable Class KBotChatRobot
    Inherits Control
    Implements IThemedControl

    Private Const BurstMs As Integer = 1400
    Private Const PauseMs As Integer = 5000
    Private Const FrameMs As Integer = 33
    Private Const WobbleHz As Double = 2.2          ' wobbles per second
    Private Const MaxAngle As Double = 11.0         ' degrees
    Private Const MaxHop As Double = 0.07           ' of the control's height
    Private Const PictureShare As Single = 0.8F     ' how much of the control the picture fills (the rest is room to move)

    Private _image As Image
    Private _unread As Integer
    Private _hover As Boolean
    Private _bursting As Boolean
    Private _burstStart As Long
    Private _scheme As ThemeScheme
    Private ReadOnly _timer As New System.Windows.Forms.Timer()

    Public Sub New()
        SetStyle(ControlStyles.Selectable, False)
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or ControlStyles.SupportsTransparentBackColor, True)
        BackColor = Color.Transparent
        Cursor = Cursors.Hand
        TabStop = False
        Size = New Size(160, 160)
        _timer.Interval = PauseMs
        AddHandler _timer.Tick, AddressOf Timer_Tick
    End Sub

    ''' <summary>The picture (the K-BOT mark).</summary>
    <Category("Appearance")>
    <Description("The picture of the robot.")>
    Public Property Image As Image
        Get
            Return _image
        End Get
        Set(value As Image)
            _image = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeImage() As Boolean
        Return _image IsNot Nothing
    End Function

    Public Sub ResetImage()
        Image = Nothing
    End Sub

    ''' <summary>Unread messages. Above zero the robot moves now and then and wears the badge; zero = quiet.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property UnreadCount As Integer
        Get
            Return _unread
        End Get
        Set(value As Integer)
            Dim k_new As Integer = Math.Max(0, value)
            If k_new = _unread Then Return
            Dim k_wasQuiet As Boolean = (_unread = 0)
            _unread = k_new
            ' New news after a quiet spell: start with a hop, not after the wait.
            If k_wasQuiet AndAlso k_new > 0 Then StartBurst()
            RefreshTimer()
            Invalidate()
        End Set
    End Property

    ''' <summary>Plays one hop-and-wobble now (the host calls it when a message arrives while the page is on screen).</summary>
    Public Sub Nudge()
        StartBurst()
        RefreshTimer()
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        _scheme = k_scheme
        Invalidate()
    End Sub

    ' ---- Animation -----------------------------------------------------------------------------

    Private ReadOnly Property CanMove As Boolean
        Get
            Return _unread > 0 AndAlso _image IsNot Nothing AndAlso Visible AndAlso IsHandleCreated AndAlso
                   Not DesignMode AndAlso SystemInformation.UIEffectsEnabled
        End Get
    End Property

    Private Sub StartBurst()
        If Not SystemInformation.UIEffectsEnabled Then Return
        _bursting = True
        _burstStart = Environment.TickCount64
    End Sub

    Private Sub RefreshTimer()
        Try
            If CanMove Then
                _timer.Interval = If(_bursting, FrameMs, PauseMs)
                If Not _timer.Enabled Then _timer.Start()
            Else
                _timer.Stop()
                If _bursting Then
                    _bursting = False
                    Invalidate()
                End If
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatRobot.RefreshTimer", ex)
        End Try
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        Try
            If Not CanMove Then
                RefreshTimer()
                Return
            End If
            If Not _bursting Then
                StartBurst()
                _timer.Interval = FrameMs
            ElseIf Environment.TickCount64 - _burstStart >= BurstMs Then
                _bursting = False
                _timer.Interval = PauseMs
            End If
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotChatRobot.Timer_Tick", ex)
        End Try
    End Sub

    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)
        RefreshTimer()
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        RefreshTimer()
    End Sub

    Protected Overrides Sub OnHandleDestroyed(e As EventArgs)
        _timer.Stop()
        MyBase.OnHandleDestroyed(e)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then _timer.Dispose()
        MyBase.Dispose(disposing)
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _hover = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _hover = False
        Invalidate()
    End Sub

    ' ---- Painting ------------------------------------------------------------------------------

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            MyBase.OnPaint(e)
            Dim k_g As Graphics = e.Graphics
            k_g.SmoothingMode = SmoothingMode.AntiAlias
            k_g.InterpolationMode = InterpolationMode.HighQualityBicubic
            k_g.PixelOffsetMode = PixelOffsetMode.HighQuality
            If _image IsNot Nothing Then DrawPicture(k_g)
            If _unread > 0 Then DrawBadge(k_g)
        Catch ex As Exception
            ' UI boundary (OnPaint): log and swallow.
            GlobalErrorLog.Write("KBotChatRobot.OnPaint", ex)
        End Try
    End Sub

    Private Sub DrawPicture(k_g As Graphics)
        Dim k_box As Single = Math.Min(Width, Height) * PictureShare * If(_hover, 1.05F, 1.0F)
        Dim k_ratio As Single = CSng(_image.Width) / _image.Height
        Dim k_w As Single = If(k_ratio >= 1.0F, k_box, k_box * k_ratio)
        Dim k_h As Single = If(k_ratio >= 1.0F, k_box / k_ratio, k_box)

        Dim k_angle As Double = 0
        Dim k_hop As Double = 0
        If _bursting Then
            Dim k_t As Double = (Environment.TickCount64 - _burstStart) / 1000.0
            Dim k_fade As Double = Math.Max(0.0, 1.0 - (k_t * 1000.0 / BurstMs))   ' dies away over the burst
            Dim k_wave As Double = Math.Sin(2.0 * Math.PI * WobbleHz * k_t)
            k_angle = MaxAngle * k_wave * k_fade
            k_hop = -MaxHop * Height * Math.Abs(k_wave) * k_fade
        End If

        Dim k_state As GraphicsState = k_g.Save()
        ' Pivot at the bottom middle of the picture: it rocks on its feet instead of spinning about its middle.
        Dim k_cx As Single = Width / 2.0F
        Dim k_base As Single = (Height + k_h) / 2.0F
        k_g.TranslateTransform(k_cx, k_base + CSng(k_hop))
        k_g.RotateTransform(CSng(k_angle))
        k_g.DrawImage(_image, -k_w / 2.0F, -k_h, k_w, k_h)
        k_g.Restore(k_state)
    End Sub

    Private Sub DrawBadge(k_g As Graphics)
        Dim k_palette As ThemePalette = If(_scheme IsNot Nothing, _scheme.Palette, ThemeManager.Current.Palette)
        Dim k_side As Integer = Math.Max(20, CInt(Math.Min(Width, Height) * 0.26))
        Dim k_text As String = If(_unread > 99, "99+", _unread.ToString(Globalization.CultureInfo.CurrentCulture))
        Using k_font As New Font(Font.FontFamily, k_side * 0.42F, FontStyle.Bold, GraphicsUnit.Pixel)
            Dim k_size As SizeF = k_g.MeasureString(k_text, k_font)
            Dim k_wide As Single = Math.Max(k_side, k_size.Width + k_side * 0.4F)
            Dim k_rect As New RectangleF(Width - k_wide - 2, 2, k_wide, k_side)
            Using k_path As New GraphicsPath()
                Dim k_d As Single = k_rect.Height
                k_path.AddArc(k_rect.X, k_rect.Y, k_d, k_d, 90, 180)
                k_path.AddArc(k_rect.Right - k_d, k_rect.Y, k_d, k_d, 270, 180)
                k_path.CloseFigure()
                Using k_fill As New SolidBrush(k_palette.ErrorColor)
                    k_g.FillPath(k_fill, k_path)
                End Using
                Using k_pen As New Pen(k_palette.SurfaceColor, Math.Max(1.5F, k_side / 12.0F))
                    k_g.DrawPath(k_pen, k_path)
                End Using
            End Using
            Using k_brush As New SolidBrush(k_palette.AccentTextColor)
                Dim k_format As New StringFormat() With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
                k_g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
                k_g.DrawString(k_text, k_font, k_brush, k_rect, k_format)
                k_format.Dispose()
            End Using
        End Using
    End Sub

End Class
