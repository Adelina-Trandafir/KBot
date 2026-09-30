Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The frozen screen of a help capture (slice 0000-02): a picture of one monitor taken the moment
''' «Capturează» was pressed, shown dimmed over that monitor. The operator:
''' <list type="bullet">
''' <item>drags a rectangle — that part is the picture;</item>
''' <item>clicks without dragging — the window under the cursor (as it looks, no shadow);</item>
''' <item>Ctrl + click — only the control under the cursor (a grid, a panel, a toolbar);</item>
''' <item>Esc or right click — gives up.</item>
''' </list>
''' It draws the picture itself and has no child controls, so it is a plain <see cref="Form"/>, not a
''' themed dialog: nothing on it could be themed, and theming would repaint its background.
''' </summary>
Friend NotInheritable Class HelpCaptureOverlay
    Inherits Form

    Private Const DragThreshold As Integer = 4

    Private ReadOnly _shot As Bitmap
    Private ReadOnly _origin As Point              ' screen position of the picture's (0,0)
    Private ReadOnly _windows As List(Of HelpCaptureNative.WindowBox)
    Private _down As Boolean
    Private _downAt As Point
    Private _current As Point
    Private _hover As Rectangle                   ' picture coordinates
    Private _result As Rectangle = Rectangle.Empty

    ''' <summary>The chosen area in picture coordinates; Empty when the operator gave up.</summary>
    Public ReadOnly Property Selection As Rectangle
        Get
            Return _result
        End Get
    End Property

    Public Sub New(shot As Bitmap, screenBounds As Rectangle, windows As List(Of HelpCaptureNative.WindowBox))
        _shot = shot
        _origin = screenBounds.Location
        _windows = windows
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        TopMost = True
        KeyPreview = True
        Cursor = Cursors.Cross
        DoubleBuffered = True
        AutoScaleMode = AutoScaleMode.None
        Bounds = screenBounds
    End Sub

    ''' <summary>Freezes <paramref name="screen"/> and lets the operator choose; Empty = gave up.</summary>
    Public Shared Function Choose(owner As IWin32Window, screen As Screen, skip As ICollection(Of IntPtr)) As Bitmap
        Try
            Dim b As Rectangle = screen.Bounds
            Dim shot As New Bitmap(b.Width, b.Height)
            Using g As Graphics = Graphics.FromImage(shot)
                g.CopyFromScreen(b.Location, Point.Empty, b.Size)
            End Using
            Dim windows As List(Of HelpCaptureNative.WindowBox) = HelpCaptureNative.TopLevelWindows(skip)
            Using overlay As New HelpCaptureOverlay(shot, b, windows)
                overlay.ShowDialog(owner)
                If overlay.Selection.IsEmpty Then
                    shot.Dispose()
                    Return Nothing
                End If
                Dim picture As Bitmap = shot.Clone(overlay.Selection, shot.PixelFormat)
                shot.Dispose()
                Return picture
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.Choose", ex)
            Throw
        End Try
    End Function

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            Activate()
            _current = PointToClient(Cursor.Position)
            UpdateHover(False)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnShown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            If e.KeyCode = Keys.Escape Then
                _result = Rectangle.Empty
                Close()
            ElseIf e.KeyCode = Keys.ControlKey Then
                UpdateHover(True)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnKeyDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)
        Try
            If e.KeyCode = Keys.ControlKey Then UpdateHover(False)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnKeyUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button = MouseButtons.Right Then
                _result = Rectangle.Empty
                Close()
                Return
            End If
            If e.Button <> MouseButtons.Left Then Return
            _down = True
            _downAt = e.Location
            _current = e.Location
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            _current = e.Location
            If Not _down Then UpdateHover((ModifierKeys And Keys.Control) = Keys.Control)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            If e.Button <> MouseButtons.Left OrElse Not _down Then Return
            _down = False
            Dim r As Rectangle = If(IsDrag(), DragRect(), _hover)
            r.Intersect(New Rectangle(Point.Empty, _shot.Size))
            If r.Width < 4 OrElse r.Height < 4 Then
                Invalidate()
                Return
            End If
            _result = r
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnMouseUp", ex)
        End Try
    End Sub

    Private Function IsDrag() As Boolean
        Return Math.Abs(_current.X - _downAt.X) > DragThreshold OrElse Math.Abs(_current.Y - _downAt.Y) > DragThreshold
    End Function

    Private Function DragRect() As Rectangle
        Return Rectangle.FromLTRB(Math.Min(_downAt.X, _current.X), Math.Min(_downAt.Y, _current.Y),
                                  Math.Max(_downAt.X, _current.X), Math.Max(_downAt.Y, _current.Y))
    End Function

    ' The window (or, with Ctrl, the control) under the cursor, in picture coordinates.
    Private Sub UpdateHover(deepest As Boolean)
        Dim screenPt As New Point(_current.X + _origin.X, _current.Y + _origin.Y)
        Dim found As Rectangle = Rectangle.Empty
        For Each w As HelpCaptureNative.WindowBox In _windows
            If Not w.Bounds.Contains(screenPt) Then Continue For
            found = If(deepest, HelpCaptureNative.DeepestChildBounds(w.Handle, screenPt), w.Bounds)
            Exit For
        Next
        If Not found.IsEmpty Then found.Offset(-_origin.X, -_origin.Y)
        If found <> _hover Then
            _hover = found
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        ' Everything is painted in OnPaint (no flicker).
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.DrawImageUnscaled(_shot, 0, 0)
            Dim sel As Rectangle = If(_down AndAlso IsDrag(), DragRect(), _hover)

            ' The mask darkens a PHOTO of the screen whatever the theme, so it is plain black, not a
            ' palette colour (a light theme colour would wash the picture out instead of dimming it).
            Using shade As New SolidBrush(Color.FromArgb(110, Color.Black))
                Using rgn As New Region(ClientRectangle)
                    If Not sel.IsEmpty Then rgn.Exclude(sel)
                    g.FillRegion(shade, rgn)
                End Using
            End Using

            Dim accent As Color = ThemeManager.Current.Palette.AccentColor
            If Not sel.IsEmpty Then
                Using pen As New Pen(accent, 2) With {.DashStyle = If(_down, DashStyle.Solid, DashStyle.Dash)}
                    g.DrawRectangle(pen, sel)
                End Using
                DrawLabel(g, sel.Width & " × " & sel.Height, New Point(sel.Left, Math.Max(0, sel.Top - 26)), accent)
            End If

            DrawLabel(g, "Trage un dreptunghi  ·  clic = fereastra  ·  Ctrl + clic = doar controlul  ·  Esc = renunță",
                      New Point(12, 12), accent)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpCaptureOverlay.OnPaint", ex)
        End Try
    End Sub

    ' Transitive coverage: called only from OnPaint.
    Private Sub DrawLabel(g As Graphics, text As String, at As Point, back As Color)
        Using f As New Font("Segoe UI", 10.0F, FontStyle.Regular, GraphicsUnit.Point)
            Dim size As Size = TextRenderer.MeasureText(g, text, f)
            Dim box As New Rectangle(at.X, at.Y, size.Width + 12, size.Height + 6)
            Using b As New SolidBrush(back)
                g.FillRectangle(b, box)
            End Using
            TextRenderer.DrawText(g, text, f, box, ThemeManager.Current.Palette.AccentTextColor, TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        End Using
    End Sub

End Class
