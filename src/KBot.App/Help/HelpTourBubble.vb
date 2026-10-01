Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The bubble of a guided tour (slice 0000-04): the tour's name and step «n din m» (no caption
''' bar since slice 0000-24), the step's title and text, a note
''' when K-BOT could not do something by itself, and «Înapoi» / «Înainte» / «Închide». Top-most,
''' placed next to the control the step talks about -- since slice 0000-23 a callout whose point
''' touches the ring. Keys: → or Enter = next, ← = back, Esc = close.
''' <see cref="HelpTourRunner"/> drives it.
''' </summary>
Public Class HelpTourBubble

    Public Event NextRequested()
    Public Event BackRequested()
    Public Event CloseRequested()

    Private _closingByRunner As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Fills the bubble for one step and sizes it to its text.</summary>
    Public Sub ShowStep(tourTitle As String, stepTitle As String, text As String, note As String,
                        index As Integer, count As Integer)
        Try
            ResetArrow()   ' measured as a plain rectangle; PlaceNear adds the point again
            ' Slice 0000-24: no caption bar -- the tour's name leads the step line.
            lblPas.Text = If(String.IsNullOrEmpty(tourTitle), String.Empty, tourTitle & "  ·  ") &
                          "Pasul " & (index + 1) & " din " & count
            lblTitlu.Text = stepTitle
            lblText.Text = text
            lblNota.Text = If(note, String.Empty)
            lblNota.Visible = Not String.IsNullOrEmpty(note)
            btnInapoi.Enabled = index > 0
            btnInainte.Text = If(index = count - 1, "Gata", "Înainte ►")
            FitToText()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.ShowStep", ex)
            Throw
        End Try
    End Sub

    ' Height = the chrome + what the labels need at the bubble's width. Device pixels throughout.
    Private Sub FitToText()
        Dim w As Integer = tlyCorp.ClientSize.Width - tlyCorp.Padding.Horizontal
        Dim h As Integer = tlyCorp.Padding.Vertical
        h += lblPas.GetPreferredSize(New Size(w, 0)).Height + lblPas.Margin.Vertical
        h += lblTitlu.GetPreferredSize(New Size(w, 0)).Height + lblTitlu.Margin.Vertical
        h += lblText.GetPreferredSize(New Size(w, 0)).Height + lblText.Margin.Vertical
        If lblNota.Visible Then h += lblNota.GetPreferredSize(New Size(w, 0)).Height + lblNota.Margin.Vertical
        h += CInt(Math.Round(44 * DeviceDpi / 96.0)) + tlyButoane.Margin.Vertical
        ClientSize = New Size(ClientSize.Width, h + Padding.Vertical + 4)
    End Sub

    ' ── Slice 0000-23: the callout ─────────────────────────────────────────────────
    ' The bubble is a callout: a triangle on one side whose point touches the ring around the
    ' thing the step talks about. The window grows by the triangle's depth on that side, keeps the
    ' body where the labels are (the form's Padding gives the strip back), and its Region cuts
    ' the strip down to the triangle. The accent BackColor that draws the 1 px outline fills the
    ' triangle too, so the point reads as part of the outline. A window with a Region gets no DWM
    ' shadow, hence BorderlessShadow = False in the designer.
    ' Slice 0000-24: Windows 11 still rounds the whole window rectangle and draws its grey border
    ' round it, Region or not -- the strip beside the triangle then read as a hole in the bubble.
    ' PlainFrame turns both off; the theme asks for rounding again on every Apply, so it runs after
    ' each placement, once shown, and after each theme change.

    Private Enum ArrowSide
        None
        Left
        Right
        Top
        Bottom
    End Enum

    Private _arrowSide As ArrowSide = ArrowSide.None
    Private _arrowDepth As Integer
    Private _bodyPadding As Padding

    Private Function Px(logical As Integer) As Integer
        Return CInt(Math.Round(logical * DeviceDpi / 96.0))
    End Function

    ' Back to the plain rectangle (the body alone), before a step is measured or placed again.
    Private Sub ResetArrow()
        If _arrowSide = ArrowSide.None Then Return
        Dim s As Size = Size
        If _arrowSide = ArrowSide.Left OrElse _arrowSide = ArrowSide.Right Then
            s.Width -= _arrowDepth
        Else
            s.Height -= _arrowDepth
        End If
        _arrowSide = ArrowSide.None
        Dim old As Region = Region
        Region = Nothing
        old?.Dispose()
        Padding = _bodyPadding
        Size = s
    End Sub

    ''' <summary>
    ''' Places the bubble next to <paramref name="target"/> (screen coordinates; the ring's outer
    ''' edge), its point on the target: right of it, else left, else below, else above -- where the
    ''' whole bubble fits in the working area. A target too big for any side (a whole view) gets the
    ''' bubble inside it, pointing up at its top edge. Empty target = centre of the screen the cursor
    ''' is on, no point.
    ''' </summary>
    Public Sub PlaceNear(target As Rectangle)
        Try
            ResetArrow()
            Dim body As Size = Size
            Dim area As Rectangle = If(target.IsEmpty, Screen.FromPoint(Cursor.Position), Screen.FromRectangle(target)).WorkingArea
            If target.IsEmpty Then
                Location = New Point(area.Left + (area.Width - body.Width) \ 2, area.Top + (area.Height - body.Height) \ 2)
                Return
            End If

            Dim gap As Integer = Px(2)
            Dim depth As Integer = Px(12)
            ' The point aims at the middle of the target's visible part.
            Dim visible As Rectangle = Rectangle.Intersect(target, area)
            If visible.IsEmpty Then visible = target
            Dim cx As Integer = visible.Left + visible.Width \ 2
            Dim cy As Integer = visible.Top + visible.Height \ 2

            If target.Right + gap + depth + body.Width <= area.Right Then
                PointFrom(ArrowSide.Left, New Point(target.Right + gap, cy), body, depth, area)
            ElseIf target.Left - gap - depth - body.Width >= area.Left Then
                PointFrom(ArrowSide.Right, New Point(target.Left - gap, cy), body, depth, area)
            ElseIf target.Bottom + gap + depth + body.Height <= area.Bottom Then
                PointFrom(ArrowSide.Top, New Point(cx, target.Bottom + gap), body, depth, area)
            ElseIf target.Top - gap - depth - body.Height >= area.Top Then
                PointFrom(ArrowSide.Bottom, New Point(cx, target.Top - gap), body, depth, area)
            Else
                ' Inside, just under the ring's top edge, pointing up at it.
                PointFrom(ArrowSide.Top, New Point(cx, Math.Max(area.Top, target.Top) + Px(10)), body, depth, area)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.PlaceNear", ex)
            Throw
        End Try
    End Sub

    ' Grows the window by the triangle on <side> and puts the point at <tip> (screen), sliding
    ' the body along that side to stay inside <area>; the point follows as far as the corners allow.
    Private Sub PointFrom(side As ArrowSide, tip As Point, body As Size, depth As Integer, area As Rectangle)
        Dim horizontal As Boolean = side = ArrowSide.Left OrElse side = ArrowSide.Right
        Dim size As Size = If(horizontal, New Size(body.Width + depth, body.Height), New Size(body.Width, body.Height + depth))
        Dim loc As Point
        Select Case side
            Case ArrowSide.Left : loc = New Point(tip.X, tip.Y - size.Height \ 2)
            Case ArrowSide.Right : loc = New Point(tip.X - size.Width, tip.Y - size.Height \ 2)
            Case ArrowSide.Top : loc = New Point(tip.X - size.Width \ 2, tip.Y)
            Case Else : loc = New Point(tip.X - size.Width \ 2, tip.Y - size.Height)
        End Select
        loc.X = Math.Max(area.Left, Math.Min(loc.X, area.Right - size.Width))
        loc.Y = Math.Max(area.Top, Math.Min(loc.Y, area.Bottom - size.Height))

        Dim half As Integer = Px(10)
        Dim corner As Integer = Px(8)
        Dim along As Integer = If(horizontal, tip.Y - loc.Y, tip.X - loc.X)
        Dim length As Integer = If(horizontal, size.Height, size.Width)
        along = Math.Max(corner + half, Math.Min(along, length - corner - half))

        _bodyPadding = Padding
        Dim p As Padding = _bodyPadding
        Select Case side
            Case ArrowSide.Left : p.Left += depth
            Case ArrowSide.Right : p.Right += depth
            Case ArrowSide.Top : p.Top += depth
            Case Else : p.Bottom += depth
        End Select
        Padding = p
        Bounds = New Rectangle(loc, size)
        _arrowSide = side
        _arrowDepth = depth

        Dim w As Integer = size.Width
        Dim h As Integer = size.Height
        Dim bodyRect As Rectangle
        Dim tri As Point()
        Select Case side
            Case ArrowSide.Left
                bodyRect = New Rectangle(depth, 0, w - depth, h)
                tri = {New Point(0, along), New Point(depth + 1, along - half), New Point(depth + 1, along + half)}
            Case ArrowSide.Right
                bodyRect = New Rectangle(0, 0, w - depth, h)
                tri = {New Point(w, along), New Point(w - depth - 1, along - half), New Point(w - depth - 1, along + half)}
            Case ArrowSide.Top
                bodyRect = New Rectangle(0, depth, w, h - depth)
                tri = {New Point(along, 0), New Point(along - half, depth + 1), New Point(along + half, depth + 1)}
            Case Else
                bodyRect = New Rectangle(0, 0, w, h - depth)
                tri = {New Point(along, h), New Point(along - half, h - depth - 1), New Point(along + half, h - depth - 1)}
        End Select
        Dim shape As New Region(bodyRect)
        Using path As New Drawing2D.GraphicsPath()
            path.AddPolygon(tri)
            shape.Union(path)
        End Using
        Dim old As Region = Region
        Region = shape
        old?.Dispose()
        PlainFrame()
    End Sub

    Private Sub PlainFrame()
        If IsHandleCreated AndAlso Not IsDisposed Then HelpWindowNative.PlainFrame(Handle)
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            PlainFrame()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnShown", ex)
        End Try
    End Sub

    ''' <summary>Closes without raising <see cref="CloseRequested"/> (the runner is already ending the tour).</summary>
    Public Sub CloseByRunner()
        _closingByRunner = True
        Close()
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.AccentColor   ' the 1px outline in the accent colour: the bubble belongs to the ring
            lblPas.ForeColor = p.TextDimColor
            lblNota.ForeColor = p.WarningColor
            ' ThemeManager.Apply rounds a borderless form AFTER this; undo it once Apply is done.
            If IsHandleCreated Then BeginInvoke(New Action(AddressOf PlainFrame))
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnThemeChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Escape : RaiseEvent CloseRequested() : e.Handled = True
                Case Keys.Right, Keys.Enter : RaiseEvent NextRequested() : e.Handled = True
                Case Keys.Left : If btnInapoi.Enabled Then RaiseEvent BackRequested()
                    e.Handled = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnKeyDown", ex)
        End Try
    End Sub

    Private Sub BtnInainte_Click(sender As Object, e As EventArgs) Handles btnInainte.Click
        RaiseEvent NextRequested()
    End Sub

    Private Sub BtnInapoi_Click(sender As Object, e As EventArgs) Handles btnInapoi.Click
        RaiseEvent BackRequested()
    End Sub

    Private Sub BtnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        RaiseEvent CloseRequested()
    End Sub

    ' Any other close (Alt+F4) is «Închide» too.
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        Try
            If Not _closingByRunner Then
                e.Cancel = True
                RaiseEvent CloseRequested()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourBubble.OnFormClosing", ex)
        End Try
    End Sub

End Class
