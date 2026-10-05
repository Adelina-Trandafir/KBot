Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' One level of a <see cref="KBotDropDownMenu"/> on screen (slice 0087): the root list or one
''' submenu. Built and owned by the menu, never by a host. The window never activates
''' (<c>WS_EX_NOACTIVATE</c> + <c>MA_NOACTIVATE</c>), so several can be open at once and the form
''' underneath keeps the focus; the keyboard reaches it through the menu's message filter.
''' </summary>
<ToolboxItem(False)>
<DesignerCategory("Code")>
Friend NotInheritable Class KBotMenuWindow
    Inherits Form
    Implements IKBotCaptureRedaction

    ''' <summary>
    ''' Slice 0000-23: the TEXT of the rows whose text is sensitive (not the row, not its icon),
    ''' for the help capture's blur. Same left edge as the paint (after the icon bar and its gap).
    ''' </summary>
    Public Function SensitiveRegions(isSensitive As Func(Of String, String, Boolean)) As IEnumerable(Of Rectangle) Implements IKBotCaptureRedaction.SensitiveRegions
        ArgumentNullException.ThrowIfNull(isSensitive)
        Dim result As New List(Of Rectangle)()
        Dim textLeft As Integer = 1 + S(_menu.IconBarWidth) + S(TextGapLogical)
        For Each row As RowSlot In _rows
            Dim it As KBotMenuItem = row.Item
            If it Is Nothing OrElse it.IsSeparator OrElse Not isSensitive(String.Empty, If(it.Text, String.Empty)) Then Continue For
            Dim f As Font = FontOf(it)
            Dim w As Integer = Math.Min(TextRenderer.MeasureText(it.Text, f).Width, Math.Max(0, row.Bounds.Right - textLeft))
            Dim h As Integer = Math.Min(row.Bounds.Height, f.Height + 2)
            If w > 0 Then result.Add(New Rectangle(textLeft, row.Bounds.Top + (row.Bounds.Height - h) \ 2, w, h))
        Next
        Return result
    End Function

    ''' <summary>
    ''' Slice 000T-09: the screen rectangle of the command row with <paramref name="k_key"/> in THIS window
    ''' (the interactive tutorial rings it); Empty when the row is not here or not shown.
    ''' </summary>
    Friend Function RowScreenBounds(k_key As String) As Rectangle
        If Not IsHandleCreated OrElse IsDisposed Then Return Rectangle.Empty
        For Each k_row As RowSlot In _rows
            Dim k_item As KBotMenuItem = k_row.Item
            If k_item IsNot Nothing AndAlso Not k_item.IsSeparator AndAlso String.Equals(k_item.Key, k_key, StringComparison.Ordinal) Then
                Return RectangleToScreen(k_row.Bounds)
            End If
        Next
        Return Rectangle.Empty
    End Function

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const CS_DROPSHADOW As Integer = &H20000
    Private Const WM_MOUSEACTIVATE As Integer = &H21
    Private Const MA_NOACTIVATE As Integer = 3

    ' Logical metrics (96 dpi).
    Private Const PadYLogical As Integer = 3
    Private Const SeparatorLogical As Integer = 9
    Private Const TextGapLogical As Integer = 10
    Private Const ShortcutGapLogical As Integer = 28
    Private Const ArrowAreaLogical As Integer = 24
    Private Const RowInsetLogical As Integer = 2

    Private Structure RowSlot
        Public Item As KBotMenuItem
        Public Bounds As Rectangle
    End Structure

    Private ReadOnly _menu As KBotDropDownMenu
    Private ReadOnly _items As KBotMenuItemCollection
    Private ReadOnly _parentWindow As KBotMenuWindow
    Private ReadOnly _scaleSource As Control
    Private ReadOnly _colors As KBotMenuColors
    Private ReadOnly _baseFont As Font
    Private ReadOnly _rows As New List(Of RowSlot)()
    Private ReadOnly _submenuTimer As New Timer()
    Private _highlight As Integer = -1
    Private _pendingRow As Integer = -1
    Private _childFor As KBotMenuItem
    Private _closing As Boolean

    Public Sub New(menu As KBotDropDownMenu, items As KBotMenuItemCollection,
                   parentWindow As KBotMenuWindow, scaleSource As Control)
        ArgumentNullException.ThrowIfNull(menu)
        ArgumentNullException.ThrowIfNull(items)
        ArgumentNullException.ThrowIfNull(scaleSource)
        _menu = menu
        _items = items
        _parentWindow = parentWindow
        _scaleSource = scaleSource
        _colors = menu.ResolveColors()
        Dim host As Form = scaleSource.FindForm()
        _baseFont = If(menu.Font, If(host IsNot Nothing, host.Font, SystemFonts.MenuFont))

        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        ControlBox = False
        MinimizeBox = False
        MaximizeBox = False
        Text = String.Empty
        AutoScaleMode = AutoScaleMode.None
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        MyBase.BackColor = _colors.Back
        _submenuTimer.Interval = Math.Max(1, menu.SubmenuDelay)
        AddHandler _submenuTimer.Tick, AddressOf SubmenuTimer_Tick
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW Or WS_EX_NOACTIVATE
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW
            Return cp
        End Get
    End Property

    ' Left to Application.ThreadException, like every WndProc override in K-BOT (house rule).
    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_MOUSEACTIVATE Then
            m.Result = New IntPtr(MA_NOACTIVATE)
            Return
        End If
        MyBase.WndProc(m)
    End Sub

    Private Function S(logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(_scaleSource, logical)
    End Function

    ' ── Placement ───────────────────────────────────────────────────────────────

    ''' <summary>
    ''' Shows the window with its top-left corner at <paramref name="topLeft"/>; when it does not
    ''' fit below, it opens upwards so that its bottom meets <paramref name="flipBottom"/>.
    ''' </summary>
    Friend Sub ShowAt(topLeft As Point, flipBottom As Integer, flipsLeft As Boolean)
        Dim size As Size = MeasureLayout()
        Dim area As Rectangle = Screen.FromPoint(topLeft).WorkingArea
        Dim x As Integer = topLeft.X
        If x + size.Width > area.Right Then x = Math.Max(area.Left, area.Right - size.Width)
        Dim y As Integer = topLeft.Y
        If y + size.Height > area.Bottom Then y = flipBottom - size.Height
        y = Math.Max(area.Top, Math.Min(y, area.Bottom - size.Height))
        Bounds = New Rectangle(x, y, size.Width, size.Height)
        ShowOverHost()
    End Sub

    ''' <summary>Shows a submenu to the right of its row (to the left when there is no room).</summary>
    Friend Sub ShowNextTo(rowScreenRect As Rectangle)
        Dim size As Size = MeasureLayout()
        Dim area As Rectangle = Screen.FromPoint(rowScreenRect.Location).WorkingArea
        Dim overlap As Integer = S(3)
        Dim x As Integer = rowScreenRect.Right + overlap
        If x + size.Width > area.Right Then x = rowScreenRect.Left - size.Width - overlap
        x = Math.Max(area.Left, x)
        Dim y As Integer = rowScreenRect.Top - 1 - S(PadYLogical)
        y = Math.Max(area.Top, Math.Min(y, area.Bottom - size.Height))
        Bounds = New Rectangle(x, y, size.Width, size.Height)
        ShowOverHost()
    End Sub

    Private Sub ShowOverHost()
        Dim host As Form = _scaleSource.FindForm()
        If host IsNot Nothing AndAlso Not host.IsDisposed Then
            Show(host)
        Else
            Show()
        End If
    End Sub

    ''' <summary>Rows + window size, from the texts and the menu's metrics at the current DPI.</summary>
    Private Function MeasureLayout() As Size
        _rows.Clear()
        Dim border As Integer = 1
        Dim bar As Integer = S(_menu.IconBarWidth)
        Dim textWidth As Integer = 0
        Dim shortcutWidth As Integer = 0
        Dim anySubmenu As Boolean = False

        Using g As Graphics = _scaleSource.CreateGraphics()
            For Each it As KBotMenuItem In _items
                If Not it.Visible OrElse it.IsSeparator Then Continue For
                textWidth = Math.Max(textWidth, MeasureText(g, it))
                If it.HasSubmenu Then
                    anySubmenu = True
                ElseIf Not String.IsNullOrEmpty(it.ShortcutText) Then
                    shortcutWidth = Math.Max(shortcutWidth,
                        TextRenderer.MeasureText(g, it.ShortcutText, FontOf(it)).Width)
                End If
            Next
        End Using

        Dim width As Integer = border * 2 + bar + S(TextGapLogical) + textWidth +
                               If(shortcutWidth > 0, S(ShortcutGapLogical) + shortcutWidth, 0) +
                               S(ArrowAreaLogical)
        If Not anySubmenu AndAlso shortcutWidth = 0 Then width -= S(ArrowAreaLogical) \ 2
        width = Math.Max(S(_menu.MinimumWidth), Math.Min(S(_menu.MaximumWidth), width))

        Dim y As Integer = border + S(PadYLogical)
        For Each it As KBotMenuItem In _items
            If Not it.Visible Then Continue For
            Dim h As Integer
            If it.IsSeparator Then
                h = S(SeparatorLogical)
            Else
                h = S(If(it.Height > 0, it.Height, _menu.ItemHeight))
            End If
            _rows.Add(New RowSlot() With {.Item = it, .Bounds = New Rectangle(border, y, width - 2 * border, h)})
            y += h
        Next
        Return New Size(width, y + S(PadYLogical) + border)
    End Function

    Private Function FontOf(it As KBotMenuItem) As Font
        Return If(it.Font, _baseFont)
    End Function

    Private Function MeasureText(g As Graphics, it As KBotMenuItem) As Integer
        Dim runs As List(Of KBotRichText.RichRun) = KBotRichText.Parse(it.Text, FontOf(it), _colors.Fore)
        Try
            Return KBotRichText.Layout(runs, g, Integer.MaxValue \ 4).Width
        Finally
            KBotRichText.DisposeDerivedFonts(runs, FontOf(it))
        End Try
    End Function

    ' ── Painting ────────────────────────────────────────────────────────────────

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
            Dim client As Rectangle = ClientRectangle
            Using b As New SolidBrush(_colors.Back)
                g.FillRectangle(b, client)
            End Using
            Dim bar As New Rectangle(1, 1, S(_menu.IconBarWidth), Math.Max(0, client.Height - 2))
            Using b As New SolidBrush(_colors.IconBar)
                g.FillRectangle(b, bar)
            End Using

            For i As Integer = 0 To _rows.Count - 1
                PaintRow(g, _rows(i), i = _highlight, bar)
            Next

            Using p As New Pen(_colors.Border)
                g.DrawRectangle(p, 0, 0, client.Width - 1, client.Height - 1)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuWindow.OnPaint", ex)
        End Try
    End Sub

    Private Sub PaintRow(g As Graphics, row As RowSlot, highlighted As Boolean, bar As Rectangle)
        Dim it As KBotMenuItem = row.Item
        Dim r As Rectangle = row.Bounds

        If it.IsSeparator Then
            Dim y As Integer = r.Top + r.Height \ 2
            Using p As New Pen(_colors.Separator)
                g.DrawLine(p, bar.Right + S(6), y, r.Right - S(6), y)
            End Using
            Return
        End If

        If highlighted AndAlso it.Enabled Then
            Dim inset As Integer = S(RowInsetLogical)
            Dim hr As New Rectangle(r.Left + inset, r.Top, r.Width - 2 * inset - 1, r.Height - 1)
            Using b As New SolidBrush(_colors.HighlightBack)
                g.FillRectangle(b, hr)
            End Using
            Using p As New Pen(_colors.HighlightBorder)
                g.DrawRectangle(p, hr)
            End Using
        End If

        If it.Image IsNot Nothing Then
            Dim side As Integer = Math.Min(S(_menu.ImageSize), r.Height - 2)
            Dim ir As New Rectangle(bar.Left + (bar.Width - side) \ 2, r.Top + (r.Height - side) \ 2, side, side)
            Dim oldMode As InterpolationMode = g.InterpolationMode
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            If it.Enabled Then
                g.DrawImage(it.Image, ir)
            Else
                Using attrs As New ImageAttributes()
                    Dim cm As New ColorMatrix() With {.Matrix33 = 0.35F}
                    attrs.SetColorMatrix(cm)
                    g.DrawImage(it.Image, ir, 0, 0, it.Image.Width, it.Image.Height, GraphicsUnit.Pixel, attrs)
                End Using
            End If
            g.InterpolationMode = oldMode
        End If

        Dim fore As Color = If(Not it.Enabled, _colors.Disabled, If(it.ForeColor.IsEmpty, _colors.Fore, it.ForeColor))
        Dim textLeft As Integer = bar.Right + S(TextGapLogical)
        Dim arrowLeft As Integer = r.Right - S(ArrowAreaLogical)

        Dim shortcutWidth As Integer = 0
        If Not it.HasSubmenu AndAlso Not String.IsNullOrEmpty(it.ShortcutText) Then
            Dim sr As New Rectangle(textLeft, r.Top, Math.Max(0, arrowLeft - textLeft), r.Height)
            Dim dimmed As Color = If(it.Enabled, ThemeShapes.Blend(fore, _colors.Back, 0.35), _colors.Disabled)
            TextRenderer.DrawText(g, it.ShortcutText, FontOf(it), sr, dimmed,
                                  TextFormatFlags.Right Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine)
            shortcutWidth = TextRenderer.MeasureText(g, it.ShortcutText, FontOf(it)).Width + S(ShortcutGapLogical)
        End If

        Dim textRect As New Rectangle(textLeft, r.Top, Math.Max(0, arrowLeft - textLeft - shortcutWidth), r.Height)
        If textRect.Width > 0 Then
            Dim runs As List(Of KBotRichText.RichRun) = KBotRichText.Parse(it.Text, FontOf(it), fore)
            Try
                Dim layout As KBotRichText.RichLayout = KBotRichText.Layout(runs, g, Integer.MaxValue \ 4)
                Dim state As GraphicsState = g.Save()
                g.SetClip(textRect)
                KBotRichText.Draw(g, layout, textRect, ContentAlignment.MiddleLeft)
                g.Restore(state)
            Finally
                KBotRichText.DisposeDerivedFonts(runs, FontOf(it))
            End Try
        End If

        If it.HasSubmenu Then
            Dim cx As Integer = r.Right - S(ArrowAreaLogical) \ 2
            Dim cy As Integer = r.Top + r.Height \ 2
            Dim a As Integer = S(4)
            Dim oldSmooth As SmoothingMode = g.SmoothingMode
            g.SmoothingMode = SmoothingMode.AntiAlias
            Using b As New SolidBrush(fore)
                g.FillPolygon(b, {New Point(cx - a \ 2, cy - a), New Point(cx + a \ 2 + 1, cy), New Point(cx - a \ 2, cy + a)})
            End Using
            g.SmoothingMode = oldSmooth
        End If
    End Sub

    ' ── Highlight and submenus ──────────────────────────────────────────────────

    Private Function IsSelectable(index As Integer) As Boolean
        If index < 0 OrElse index >= _rows.Count Then Return False
        Dim it As KBotMenuItem = _rows(index).Item
        Return Not it.IsSeparator AndAlso it.Enabled
    End Function

    Private Sub SetHighlight(index As Integer)
        If index = _highlight Then Return
        _highlight = index
        Invalidate()
    End Sub

    Private Function RowAt(pt As Point) As Integer
        For i As Integer = 0 To _rows.Count - 1
            If _rows(i).Bounds.Contains(pt) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>The keyboard step: next selectable row in <paramref name="direction"/>, wrapping.</summary>
    Friend Sub MoveHighlight(direction As Integer)
        If _rows.Count = 0 Then Return
        Dim i As Integer = _highlight
        For n As Integer = 1 To _rows.Count
            i += direction
            If i < 0 Then i = _rows.Count - 1
            If i >= _rows.Count Then i = 0
            If IsSelectable(i) Then
                SetHighlight(i)
                Return
            End If
        Next
    End Sub

    ''' <summary>Home / End.</summary>
    Friend Sub HighlightEdge(first As Boolean)
        _highlight = If(first, -1, _rows.Count)
        MoveHighlight(If(first, 1, -1))
    End Sub

    ''' <summary>Opens the submenu of the highlighted row (Right arrow / Enter).</summary>
    Friend Sub OpenHighlightedSubmenu(selectFirst As Boolean)
        If Not IsSelectable(_highlight) Then Return
        Dim it As KBotMenuItem = _rows(_highlight).Item
        If Not it.HasSubmenu Then Return
        Dim child As KBotMenuWindow = OpenChild(_highlight)
        If child IsNot Nothing AndAlso selectFirst Then child.HighlightEdge(first:=True)
    End Sub

    ''' <summary>Enter / Space: opens a submenu or chooses the command.</summary>
    Friend Sub ActivateHighlighted()
        If Not IsSelectable(_highlight) Then Return
        Dim it As KBotMenuItem = _rows(_highlight).Item
        If it.HasSubmenu Then
            OpenHighlightedSubmenu(selectFirst:=True)
        Else
            _menu.Choose(it)
        End If
    End Sub

    Private Function OpenChild(index As Integer) As KBotMenuWindow
        _submenuTimer.Stop()
        Dim it As KBotMenuItem = _rows(index).Item
        If _childFor Is it Then Return NextWindow()
        Dim rowScreen As Rectangle = RectangleToScreen(_rows(index).Bounds)
        ' Open first: opening closes the previous submenu, whose closing clears _childFor.
        Dim child As KBotMenuWindow = _menu.OpenSubmenu(Me, it, rowScreen)
        _childFor = it
        Return child
    End Function

    ' The window opened from this one, if any.
    Private Function NextWindow() As KBotMenuWindow
        Dim windows As IReadOnlyList(Of KBotMenuWindow) = _menu.OpenWindows
        For i As Integer = 0 To windows.Count - 2
            If windows(i) Is Me Then Return windows(i + 1)
        Next
        Return Nothing
    End Function

    Private Sub CloseChild()
        _submenuTimer.Stop()
        If _childFor Is Nothing Then Return
        _childFor = Nothing
        Dim windows As IReadOnlyList(Of KBotMenuWindow) = _menu.OpenWindows
        Dim level As Integer = -1
        For i As Integer = 0 To windows.Count - 1
            If windows(i) Is Me Then level = i
        Next
        If level >= 0 Then _menu.CloseFrom(level + 1)
    End Sub

    ''' <summary>Called by the menu when a deeper level is closed from outside (Esc / Left).</summary>
    Friend Sub ForgetChild()
        _childFor = Nothing
    End Sub

    ''' <summary>Closes without asking anything (the menu already removed it from its chain).</summary>
    Friend Sub CloseQuietly()
        If _closing Then Return
        _closing = True
        _submenuTimer.Stop()
        _parentWindow?.ForgetChild()
        If Not IsDisposed Then Close()
    End Sub

    ' ── Mouse (UI boundaries: log and swallow) ──────────────────────────────────

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        Try
            MyBase.OnMouseMove(e)
            Dim i As Integer = RowAt(e.Location)
            If i = _highlight Then Return
            If Not IsSelectable(i) Then
                If _childFor Is Nothing Then SetHighlight(-1)
                Return
            End If
            SetHighlight(i)
            ' The submenu opens (or the open one closes) after the delay, so crossing a row on the
            ' way to an open submenu does not close it.
            _pendingRow = i
            _submenuTimer.Stop()
            If _rows(i).Item.HasSubmenu OrElse _childFor IsNot Nothing Then
                If _menu.SubmenuDelay = 0 Then
                    SubmenuTimer_Tick(Me, EventArgs.Empty)
                Else
                    _submenuTimer.Start()
                End If
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuWindow.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        Try
            MyBase.OnMouseLeave(e)
            If _childFor Is Nothing Then
                _submenuTimer.Stop()
                SetHighlight(-1)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuWindow.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        Try
            MyBase.OnMouseUp(e)
            If e.Button <> MouseButtons.Left Then Return
            Dim i As Integer = RowAt(e.Location)
            If Not IsSelectable(i) Then Return
            SetHighlight(i)
            Dim it As KBotMenuItem = _rows(i).Item
            If it.HasSubmenu Then
                OpenChild(i)
            Else
                _menu.Choose(it)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuWindow.OnMouseUp", ex)
        End Try
    End Sub

    Private Sub SubmenuTimer_Tick(sender As Object, e As EventArgs)
        Try
            _submenuTimer.Stop()
            Dim i As Integer = _pendingRow
            If i <> _highlight OrElse Not IsSelectable(i) Then Return
            Dim it As KBotMenuItem = _rows(i).Item
            If it.HasSubmenu Then
                OpenChild(i)
            Else
                CloseChild()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotMenuWindow.SubmenuTimer_Tick", ex)
        End Try
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then _submenuTimer.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
