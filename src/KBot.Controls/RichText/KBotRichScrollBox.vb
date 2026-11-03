Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' A <c>RichTextBox</c> whose scroll bars are <see cref="KBotScrollBar"/>s (themed, dark scheme
''' included) instead of the native ones: the base of <see cref="KBotLogBox"/> and
''' <see cref="KBotSqlBox"/>.
'''
''' <para><b>How.</b> The native bars are switched off (a native bar is a window Windows paints
''' itself, so no palette colour could reach it). The two bars are child controls of the box,
''' standing in a strip along its right and bottom edge; the formatting rectangle
''' (<c>EM_SETRECT</c>) is shortened by those strips so the text never goes under them. The scroll
''' position is read and written in PIXELS (<c>EM_GETSCROLLPOS</c> / <c>EM_SETSCROLLPOS</c>).</para>
'''
''' <para><b>The range is measured, not computed.</b> The document's real extent is found by
''' scrolling the view to the far corner and reading where it stopped (painting off, the view put
''' back, one repaint): that stays exact with mixed fonts, wrapped lines and tabs, and needs no walk
''' over the lines. It runs a moment after the last text or size change, not on every scroll.</para>
'''
''' <para><see cref="ScrollBars"/> and <see cref="WordWrap"/> keep their meaning (the horizontal bar
''' only exists without word wrap); the native bars are never used. The box also takes over the
''' mouse wheel (Shift = sideways); Ctrl + wheel no longer zooms.</para>
''' </summary>
<ToolboxItem(False)>
Public Class KBotRichScrollBox
    Inherits RichTextBox

    Private Const WM_SETREDRAW As Integer = &HB
    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_KEYUP As Integer = &H101
    Private Const WM_CHAR As Integer = &H102
    Private Const WM_MOUSEMOVE As Integer = &H200
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_LBUTTONUP As Integer = &H202
    Private Const WM_MOUSEWHEEL As Integer = &H20A
    Private Const WM_PASTE As Integer = &H302
    Private Const WM_CUT As Integer = &H300
    Private Const MK_LBUTTON As Long = &H1L
    Private Const MK_SHIFT As Long = &H4L
    Private Const EM_GETRECT As Integer = &HB2
    Private Const EM_SETRECT As Integer = &HB3
    Private Const EM_LINESCROLL As Integer = &HB6
    Private Const EM_SCROLLCARET As Integer = &HB7
    Private Const EM_GETSCROLLPOS As Integer = &H4DD   ' WM_USER + 221
    Private Const EM_SETSCROLLPOS As Integer = &H4DE   ' WM_USER + 222
    Private Const FarAway As Integer = 100000000

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NativeRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(k_handle As IntPtr, k_msg As Integer, k_wParam As Integer, ByRef k_lParam As Point) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(k_handle As IntPtr, k_msg As Integer, k_wParam As Integer, ByRef k_lParam As NativeRect) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(k_handle As IntPtr, k_msg As Integer, k_wParam As Integer, k_lParam As Integer) As Integer
    End Function

    Private ReadOnly _vBar As New KBotScrollBar()
    Private ReadOnly _hBar As New KBotScrollBar()
    Private ReadOnly _rangeTimer As New Timer() With {.Interval = 60}
    Private _mode As RichTextBoxScrollBars = RichTextBoxScrollBars.Both
    Private _busy As Boolean
    Private _autoHide As Boolean = True
    ' What the default formatting rectangle leaves between itself and the client edge (read once).
    Private _insetRight As Integer
    Private _insetBottom As Integer
    Private _insetsKnown As Boolean
    ' Where the bars belong. The box scrolls its window with ScrollWindow, which MOVES child windows
    ' along, so after every message the bars are put back (KeepBarsInPlace).
    Private _vSlot As Rectangle
    Private _hSlot As Rectangle

    Public Sub New()
        MyBase.New()
        MyBase.ScrollBars = RichTextBoxScrollBars.None
        _vBar.Orientation = Orientation.Vertical
        _hBar.Orientation = Orientation.Horizontal
        _vBar.Visible = False
        _hBar.Visible = False
        AddHandler _vBar.Scroll, AddressOf OnVBarScroll
        AddHandler _hBar.Scroll, AddressOf OnHBarScroll
        AddHandler _rangeTimer.Tick, AddressOf OnRangeTimerTick
        Controls.Add(_vBar)
        Controls.Add(_hBar)
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing Then _rangeTimer.Dispose()
        MyBase.Dispose(k_disposing)
    End Sub

    ' ------------------------------------------------------------------
    ' Properties (the same names and meaning as the native ones)
    ' ------------------------------------------------------------------

    ''' <summary>Which bars the box has: the vertical one is ours, never the native one.</summary>
    <Category("Appearance")>
    <Description("Barele de derulare ale casetei.")>
    <DefaultValue(RichTextBoxScrollBars.Both)>
    Public Shadows Property ScrollBars As RichTextBoxScrollBars
        Get
            Return _mode
        End Get
        Set(k_value As RichTextBoxScrollBars)
            If _mode = k_value Then Return
            _mode = k_value
            QueueRange()
        End Set
    End Property

    ''' <summary>Word wrap; without it the horizontal bar appears.</summary>
    <Category("Behavior")>
    <Description("Trece textul pe rândul următor. Fără el apare bara orizontală.")>
    <DefaultValue(True)>
    Public Shadows Property WordWrap As Boolean
        Get
            Return MyBase.WordWrap
        End Get
        Set(k_value As Boolean)
            If MyBase.WordWrap = k_value Then Return
            MyBase.WordWrap = k_value
            QueueRange()
        End Set
    End Property

    ''' <summary>A bar is shown only while there is something to scroll; the Forced* modes of
    ''' <see cref="ScrollBars"/> then behave like their plain ones. Off: the Forced* modes keep the bar always.</summary>
    <Category("Behavior")>
    <Description("Barea apare doar când textul nu încape; oprit, modurile Forced* o țin mereu.")>
    <DefaultValue(True)>
    Public Property AutoHideScrollBars As Boolean
        Get
            Return _autoHide
        End Get
        Set(k_value As Boolean)
            If _autoHide = k_value Then Return
            _autoHide = k_value
            QueueRange()
        End Set
    End Property

    Private Function HasVertical() As Boolean
        Return _mode = RichTextBoxScrollBars.Vertical OrElse _mode = RichTextBoxScrollBars.ForcedVertical OrElse
               _mode = RichTextBoxScrollBars.Both OrElse _mode = RichTextBoxScrollBars.ForcedBoth
    End Function

    Private Function HasHorizontal() As Boolean
        Return (_mode = RichTextBoxScrollBars.Horizontal OrElse _mode = RichTextBoxScrollBars.ForcedHorizontal OrElse
                _mode = RichTextBoxScrollBars.Both OrElse _mode = RichTextBoxScrollBars.ForcedBoth) AndAlso Not MyBase.WordWrap
    End Function

    Private Function ForcesVertical() As Boolean
        Return Not _autoHide AndAlso (_mode = RichTextBoxScrollBars.ForcedVertical OrElse _mode = RichTextBoxScrollBars.ForcedBoth)
    End Function

    Private Function ForcesHorizontal() As Boolean
        Return Not _autoHide AndAlso (_mode = RichTextBoxScrollBars.ForcedHorizontal OrElse _mode = RichTextBoxScrollBars.ForcedBoth)
    End Function

    ''' <summary>Hands the scheme to both bars. The host calls it from its own <c>ApplyTheme</c> (a derived box or the control that holds the box).</summary>
    Public Sub ApplyBarTheme(k_scheme As ThemeScheme)
        _vBar.ApplyTheme(k_scheme)
        _hBar.ApplyTheme(k_scheme)
    End Sub

    ' ------------------------------------------------------------------
    ' Layout
    ' ------------------------------------------------------------------

    Private Function BarThickness() As Integer
        Return ThemeShapes.ScaleDpi(Me, KBotScrollBar.GrosimeImplicita)
    End Function

    Protected Overrides Sub OnHandleCreated(k_e As EventArgs)
        MyBase.OnHandleCreated(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            ReadDefaultInsets()
            PlaceBars()
            QueueRange()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.OnHandleCreated", ex)
        End Try
    End Sub

    Protected Overrides Sub OnSizeChanged(k_e As EventArgs)
        MyBase.OnSizeChanged(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) OrElse Not IsHandleCreated Then Return
            PlaceBars()
            QueueRange()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.OnSizeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnTextChanged(k_e As EventArgs)
        MyBase.OnTextChanged(k_e)
        QueueRange()
    End Sub

    Protected Overrides Sub OnFontChanged(k_e As EventArgs)
        MyBase.OnFontChanged(k_e)
        QueueRange()
    End Sub

    ' The default formatting rectangle may sit a little inside the client edge; keep that gap, so
    ' a box with no bar looks exactly as before.
    Private Sub ReadDefaultInsets()
        If _insetsKnown OrElse Not IsHandleCreated Then Return
        Dim k_rect As New NativeRect()
        SendMessage(Handle, EM_GETRECT, 0, k_rect)
        If k_rect.Right <= 0 Then Return
        _insetRight = Math.Max(0, ClientSize.Width - k_rect.Right)
        _insetBottom = Math.Max(0, ClientSize.Height - k_rect.Bottom)
        _insetsKnown = True
    End Sub

    ''' <summary>Puts the bars in their strips and shortens the formatting rectangle to match.</summary>
    Private Sub PlaceBars()
        If Not IsHandleCreated Then Return
        Dim k_w As Integer = If(_vBar.Visible, BarThickness(), 0)
        Dim k_h As Integer = If(_hBar.Visible, BarThickness(), 0)
        _vSlot = New Rectangle(ClientSize.Width - k_w, 0, k_w, Math.Max(0, ClientSize.Height - k_h))
        _hSlot = New Rectangle(0, ClientSize.Height - k_h, Math.Max(0, ClientSize.Width - k_w), k_h)
        _vBar.Bounds = _vSlot
        _hBar.Bounds = _hSlot
        If k_w = 0 AndAlso k_h = 0 Then
            ' No strip: back to the default rectangle.
            SendMessage(Handle, EM_SETRECT, 0, 0)
        Else
            ' EM_SETRECT does not repaint, and a resize resets it: it is sent after every placement.
            Dim k_rect As New NativeRect With {
                .Left = 0, .Top = 0,
                .Right = Math.Max(1, ClientSize.Width - _insetRight - k_w),
                .Bottom = Math.Max(1, ClientSize.Height - _insetBottom - k_h)}
            SendMessage(Handle, EM_SETRECT, 0, k_rect)
        End If
        Invalidate()
    End Sub

    ' ------------------------------------------------------------------
    ' Range (measured) and position
    ' ------------------------------------------------------------------

    Private Sub QueueRange()
        If KBotDesignTime.IsDesignTime(Me) OrElse Not IsHandleCreated Then Return
        _rangeTimer.Stop()
        _rangeTimer.Start()
    End Sub

    Private Sub OnRangeTimerTick(k_sender As Object, k_e As EventArgs)
        _rangeTimer.Stop()
        RecomputeRange()
    End Sub

    ''' <summary>Measures the document and puts range, position and visibility on the bars. UI boundary: logs and swallows.</summary>
    Public Sub RecomputeRange()
        Try
            If _busy OrElse Not IsHandleCreated OrElse KBotDesignTime.IsDesignTime(Me) Then Return
            _busy = True
            Try
                ' A bar that appears or goes away changes the room the text has: place the bars and
                ' measure again (it settles; three passes are the most it ever needs).
                Dim k_settled As Boolean = False
                For k_pass As Integer = 1 To 3
                    If Not ReadRangeIntoBars() Then
                        k_settled = True
                        Exit For
                    End If
                    PlaceBars()
                Next
                If Not k_settled Then ReadRangeIntoBars()
            Finally
                _busy = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.RecomputeRange", ex)
        End Try
    End Sub

    ''' <summary>True when a bar's visibility changed (so the strips must be placed again).</summary>
    Private Function ReadRangeIntoBars() As Boolean
        Dim k_pos As Point = ReadScrollPos()
        Dim k_far As Point = MeasureFarCorner(k_pos)

        Dim k_viewW As Integer = Math.Max(1, ClientSize.Width - If(_vBar.Visible, BarThickness(), 0))
        Dim k_viewH As Integer = Math.Max(1, ClientSize.Height - If(_hBar.Visible, BarThickness(), 0))

        ' The far position is NOT "document minus window": the box lets the view run on until the
        ' last line (the end of the longest line) reaches the top (left) edge. So the far position
        ' plus one line (one character) is the size of the document itself, and only a document
        ' larger than the window has anything to scroll. Nothing measured (empty box) = no document.
        Dim k_line As Integer = Math.Max(1, Font.Height)
        Dim k_char As Integer = Math.Max(1, TextRenderer.MeasureText("0123456789", Font).Width \ 10)
        Dim k_contentH As Integer = If(k_far.Y > 0, k_far.Y + k_line, 0)
        Dim k_contentW As Integer = If(k_far.X > 0, k_far.X + k_char, 0)

        Dim k_showV As Boolean = HasVertical() AndAlso (ForcesVertical() OrElse k_contentH > k_viewH)
        Dim k_showH As Boolean = HasHorizontal() AndAlso (ForcesHorizontal() OrElse k_contentW > k_viewW)

        _vBar.SmallChange = k_line
        _vBar.SetRange(0, Math.Max(k_contentH, k_viewH) - 1, k_viewH, k_pos.Y)
        _hBar.SmallChange = k_char
        _hBar.SetRange(0, Math.Max(k_contentW, k_viewW) - 1, k_viewW, k_pos.X)

        Dim k_changed As Boolean = (_vBar.Visible <> k_showV) OrElse (_hBar.Visible <> k_showH)
        _vBar.Visible = k_showV
        _hBar.Visible = k_showH
        Return k_changed
    End Function

    ' Scrolls the view as far as it will go, reads where it stopped (that IS the extent: how far the
    ' document can be scrolled) and puts it back. Painting is off meanwhile, one repaint after.
    Private Function MeasureFarCorner(k_back As Point) As Point
        Dim k_far As New Point(FarAway, FarAway)
        Dim k_end As New Point()
        SendMessage(Handle, WM_SETREDRAW, 0, 0)
        Try
            SendMessage(Handle, EM_SETSCROLLPOS, 0, k_far)
            SendMessage(Handle, EM_GETSCROLLPOS, 0, k_end)
        Finally
            Dim k_restore As Point = k_back
            SendMessage(Handle, EM_SETSCROLLPOS, 0, k_restore)
            SendMessage(Handle, WM_SETREDRAW, 1, 0)
            Invalidate()
        End Try
        ' A box with nothing in it may not clamp the far position at all: that is "nothing to scroll".
        Return New Point(If(k_end.X >= FarAway \ 2, 0, Math.Max(0, k_end.X)), If(k_end.Y >= FarAway \ 2, 0, Math.Max(0, k_end.Y)))
    End Function

    Private Function ReadScrollPos() As Point
        Dim k_pos As New Point()
        SendMessage(Handle, EM_GETSCROLLPOS, 0, k_pos)
        Return k_pos
    End Function

    Private Sub WriteScrollPos(k_pos As Point)
        Dim k_value As Point = k_pos
        SendMessage(Handle, EM_SETSCROLLPOS, 0, k_value)
    End Sub

    ''' <summary>Puts the bars back in their strips if a scroll of the box carried them along.</summary>
    Private Sub KeepBarsInPlace()
        If _vBar.Visible AndAlso _vBar.Bounds <> _vSlot Then _vBar.Bounds = _vSlot
        If _hBar.Visible AndAlso _hBar.Bounds <> _hSlot Then _hBar.Bounds = _hSlot
    End Sub

    ''' <summary>Cheap: puts the thumbs where the view is now (no measuring).</summary>
    Private Sub SyncPosition()
        Try
            If _busy OrElse Not IsHandleCreated Then Return
            Dim k_pos As Point = ReadScrollPos()
            If _vBar.Visible Then _vBar.Value = k_pos.Y
            If _hBar.Visible Then _hBar.Value = k_pos.X
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.SyncPosition", ex)
        End Try
    End Sub

    ' The operator moved a bar: scroll the box to it, then read back where it really went (the box
    ' clamps), so the thumb never stays beside the text.
    Private Sub OnVBarScroll(k_sender As Object, k_e As ScrollEventArgs)
        Try
            If _busy OrElse Not IsHandleCreated Then Return
            Dim k_pos As Point = ReadScrollPos()
            WriteScrollPos(New Point(k_pos.X, _vBar.Value))
            SyncPosition()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.OnVBarScroll", ex)
        End Try
    End Sub

    Private Sub OnHBarScroll(k_sender As Object, k_e As ScrollEventArgs)
        Try
            If _busy OrElse Not IsHandleCreated Then Return
            Dim k_pos As Point = ReadScrollPos()
            WriteScrollPos(New Point(_hBar.Value, k_pos.Y))
            SyncPosition()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.OnHBarScroll", ex)
        End Try
    End Sub

    Private Sub ScrollByWheel(k_delta As Integer, k_sideways As Boolean)
        Try
            Dim k_notches As Integer = k_delta \ SystemInformation.MouseWheelScrollDelta
            If k_notches = 0 OrElse Not IsHandleCreated Then Return
            Dim k_lines As Integer = SystemInformation.MouseWheelScrollLines
            If k_lines <= 0 Then k_lines = 3
            Dim k_pos As Point = ReadScrollPos()
            If k_sideways Then
                If Not _hBar.Visible Then Return
                WriteScrollPos(New Point(Math.Max(0, Math.Min(_hBar.MaxValue, k_pos.X - k_notches * k_lines * _hBar.SmallChange)), k_pos.Y))
            Else
                If Not _vBar.Visible Then Return
                WriteScrollPos(New Point(k_pos.X, Math.Max(0, Math.Min(_vBar.MaxValue, k_pos.Y - k_notches * k_lines * _vBar.SmallChange))))
            End If
            SyncPosition()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichScrollBox.ScrollByWheel", ex)
        End Try
    End Sub

    ' WndProc stays unwrapped (house rule: the window message contract is not broken; the net is
    ' Application.ThreadException).
    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_MOUSEWHEEL Then
            ' Taken over (not passed on): a native box without bars ignores the wheel, and Ctrl + wheel zoom is unwanted.
            ScrollByWheel(((CInt((m.WParam.ToInt64() >> 16) And &HFFFFL) Xor &H8000) - &H8000), (m.WParam.ToInt64() And MK_SHIFT) <> 0)
            Return
        End If
        MyBase.WndProc(m)
        KeepBarsInPlace()
        ' A mouse move moves the view only while selecting (left button held).
        If m.Msg = WM_MOUSEMOVE AndAlso (m.WParam.ToInt64() And MK_LBUTTON) = 0 Then Return
        Select Case m.Msg
            Case WM_KEYDOWN, WM_KEYUP, WM_CHAR, WM_LBUTTONDOWN, WM_LBUTTONUP, WM_MOUSEMOVE,
                 WM_PASTE, WM_CUT, EM_SCROLLCARET, EM_LINESCROLL
                SyncPosition()
        End Select
    End Sub

End Class
