Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

' Painting, mouse and keyboard of KBotGraphView. The model and the public surface are in KBotGraphView.vb.
Partial Public NotInheritable Class KBotGraphView

    ' =====================================================================
    ' WORLD <-> SCREEN
    ' =====================================================================

    Private Function ScreenX(k_world As Double) As Single
        Return CSng(k_world * _zoom + _panX)
    End Function

    Private Function ScreenY(k_world As Double) As Single
        Return CSng(k_world * _zoom + _panY)
    End Function

    Private Function RadiusPx(k_node As KBotGraphNode) As Single
        Return CSng(ThemeShapes.ScaleDpi(Me, If(k_node.Radius > 0, k_node.Radius, _nodeRadius)))
    End Function

    ' =====================================================================
    ' PAINT
    ' =====================================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim k_g As Graphics = e.Graphics
            k_g.Clear(MyBase.BackColor)
            Dim k_bounds As Rectangle = ClientRectangle
            Using k_back As New SolidBrush(EffectivePlotBackColor())
                k_g.FillRectangle(k_back, k_bounds)
            End Using
            If _needsFit AndAlso _nodes.Count > 0 Then FitToView()

            k_g.SmoothingMode = SmoothingMode.AntiAlias
            Dim k_p As ThemePalette = Palette()
            Dim k_visibleCount As Integer = 0
            For Each k_node As KBotGraphNode In _nodes
                If k_node.Visible Then k_visibleCount += 1
            Next

            If k_visibleCount = 0 Then
                If _emptyText.Length > 0 Then
                    TextRenderer.DrawText(k_g, _emptyText, Font, k_bounds, k_p.TextDimColor,
                                          TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.WordBreak)
                End If
                DrawBorder(k_g, k_bounds)
                Return
            End If

            ' Who is in focus: the hovered dot, else the selected one. Everything else fades.
            Dim k_focus As Integer = If(_hoverIndex >= 0, _hoverIndex, _selectedIndex)
            If k_focus >= _nodes.Count OrElse (k_focus >= 0 AndAlso Not _nodes(k_focus).Visible) Then k_focus = -1
            Dim k_related(_nodes.Count - 1) As Boolean
            If k_focus >= 0 Then
                k_related(k_focus) = True
                For Each k_i As Integer In Neighbours()(k_focus)
                    k_related(k_i) = True
                Next
            End If

            Dim k_baseAlpha As Integer = CInt(255 * _edgeOpacity / 100.0)
            Dim k_defaultEdge As Color = If(_edgeColor = Color.Empty, ThemeShapes.Blend(EffectivePlotBackColor(), k_p.TextColor, 0.6), _edgeColor)
            If _edgePen Is Nothing Then RebuildThemeResources()

            ' ---- lines ----
            For Each k_edge As KBotGraphEdge In _edges
                If Not (k_edge.FromNode.Visible AndAlso k_edge.ToNode.Visible) Then Continue For
                If k_focus >= 0 AndAlso (k_edge.FromNode.Index = k_focus OrElse k_edge.ToNode.Index = k_focus) Then Continue For
                Dim k_x1 As Single = ScreenX(k_edge.FromNode.X)
                Dim k_y1 As Single = ScreenY(k_edge.FromNode.Y)
                Dim k_x2 As Single = ScreenX(k_edge.ToNode.X)
                Dim k_y2 As Single = ScreenY(k_edge.ToNode.Y)
                If (k_x1 < 0 AndAlso k_x2 < 0) OrElse (k_y1 < 0 AndAlso k_y2 < 0) OrElse
                   (k_x1 > k_bounds.Width AndAlso k_x2 > k_bounds.Width) OrElse (k_y1 > k_bounds.Height AndAlso k_y2 > k_bounds.Height) Then Continue For
                Dim k_alpha As Integer = If(k_edge.Faint, k_baseAlpha \ 2, k_baseAlpha)
                If k_focus >= 0 Then k_alpha = k_alpha \ 4
                _edgePen.Color = Color.FromArgb(Math.Max(8, k_alpha), If(k_edge.EdgeColor = Color.Empty, k_defaultEdge, k_edge.EdgeColor))
                k_g.DrawLine(_edgePen, k_x1, k_y1, k_x2, k_y2)
            Next

            ' The lines of the dot in focus, on top of the faded ones.
            If k_focus >= 0 Then
                Dim k_focusPen As New Pen(k_p.AccentColor, _edgePen.Width * 1.8F)
                Try
                    For Each k_edge As KBotGraphEdge In _edges
                        If Not (k_edge.FromNode.Visible AndAlso k_edge.ToNode.Visible) Then Continue For
                        If k_edge.FromNode.Index <> k_focus AndAlso k_edge.ToNode.Index <> k_focus Then Continue For
                        k_g.DrawLine(k_focusPen, ScreenX(k_edge.FromNode.X), ScreenY(k_edge.FromNode.Y), ScreenX(k_edge.ToNode.X), ScreenY(k_edge.ToNode.Y))
                    Next
                Finally
                    k_focusPen.Dispose()
                End Try
            End If

            ' ---- dots ----
            If _fillBrush Is Nothing Then _fillBrush = New SolidBrush(Color.Gray)
            For Each k_node As KBotGraphNode In _nodes
                If Not k_node.Visible Then Continue For
                Dim k_cx As Single = ScreenX(k_node.X)
                Dim k_cy As Single = ScreenY(k_node.Y)
                Dim k_r As Single = RadiusPx(k_node)
                If k_cx < -k_r - 4 OrElse k_cy < -k_r - 4 OrElse k_cx > k_bounds.Width + k_r + 4 OrElse k_cy > k_bounds.Height + k_r + 4 Then Continue For
                Dim k_color As Color = If(k_node.NodeColor = Color.Empty, k_p.AccentColor, k_node.NodeColor)
                If k_focus >= 0 AndAlso Not k_related(k_node.Index) Then k_color = Color.FromArgb(70, k_color)
                _fillBrush.Color = k_color
                k_g.FillEllipse(_fillBrush, k_cx - k_r, k_cy - k_r, 2 * k_r, 2 * k_r)
                If k_node.Highlighted AndAlso (k_focus < 0 OrElse k_related(k_node.Index)) Then
                    k_g.DrawEllipse(_ringPen, k_cx - k_r - 3, k_cy - k_r - 3, 2 * (k_r + 3), 2 * (k_r + 3))
                End If
            Next

            ' Hover and selection rings, with the scheme's text colour so they read on any dot colour.
            Using k_textPen As New Pen(k_p.TextColor, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
                For Each k_i As Integer In New Integer() {_selectedIndex, _hoverIndex}
                    If k_i < 0 OrElse k_i >= _nodes.Count OrElse Not _nodes(k_i).Visible Then Continue For
                    Dim k_node As KBotGraphNode = _nodes(k_i)
                    Dim k_r As Single = RadiusPx(k_node) + 2
                    k_g.DrawEllipse(k_textPen, ScreenX(k_node.X) - k_r, ScreenY(k_node.Y) - k_r, 2 * k_r, 2 * k_r)
                Next
            End Using

            ' ---- names ----
            If _labelBrush Is Nothing Then _labelBrush = New SolidBrush(k_p.TextColor)
            _labelBrush.Color = k_p.TextColor
            For Each k_node As KBotGraphNode In _nodes
                If Not k_node.Visible Then Continue For
                If Not (_labelsVisible OrElse (k_focus >= 0 AndAlso k_related(k_node.Index))) Then Continue For
                If k_node.Text.Length = 0 Then Continue For
                Dim k_cx As Single = ScreenX(k_node.X)
                Dim k_cy As Single = ScreenY(k_node.Y)
                If k_cx < 0 OrElse k_cy < 0 OrElse k_cx > k_bounds.Width OrElse k_cy > k_bounds.Height Then Continue For
                If k_focus >= 0 AndAlso Not k_related(k_node.Index) Then Continue For
                k_g.DrawString(k_node.Text, Font, _labelBrush, k_cx + RadiusPx(k_node) + 3, k_cy - Font.Height / 2.0F)
            Next

            DrawBorder(k_g, k_bounds)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnPaint", ex)
        End Try
    End Sub

    Private Sub DrawBorder(k_g As Graphics, k_bounds As Rectangle)
        If _borderPen Is Nothing Then RebuildThemeResources()
        k_g.SmoothingMode = SmoothingMode.None
        k_g.DrawRectangle(_borderPen, 0, 0, k_bounds.Width - 1, k_bounds.Height - 1)
    End Sub

    ' =====================================================================
    ' HIT TEST
    ' =====================================================================

    ''' <summary>The nearest visible dot within its radius plus <see cref="HoverRadius"/>, or -1.</summary>
    Private Function NodeAt(k_point As Point) As Integer
        Dim k_best As Integer = -1
        Dim k_bestDistance As Double = Double.MaxValue
        Dim k_extra As Single = CSng(ThemeShapes.ScaleDpi(Me, _hoverRadius))
        For Each k_node As KBotGraphNode In _nodes
            If Not k_node.Visible Then Continue For
            Dim k_dx As Double = ScreenX(k_node.X) - k_point.X
            Dim k_dy As Double = ScreenY(k_node.Y) - k_point.Y
            Dim k_reach As Double = RadiusPx(k_node) + k_extra
            Dim k_d2 As Double = k_dx * k_dx + k_dy * k_dy
            If k_d2 <= k_reach * k_reach AndAlso k_d2 < k_bestDistance Then
                k_best = k_node.Index
                k_bestDistance = k_d2
            End If
        Next
        Return k_best
    End Function

    ' =====================================================================
    ' MOUSE
    ' =====================================================================

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            Focus()
            If e.Button <> MouseButtons.Left Then Return
            _pressed = True
            _moved = False
            _pressPoint = e.Location
            _panStartX = _panX
            _panStartY = _panY
            _pressedIndex = NodeAt(e.Location)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            If _pressed Then
                Dim k_dx As Integer = e.X - _pressPoint.X
                Dim k_dy As Integer = e.Y - _pressPoint.Y
                If Not _moved AndAlso (Math.Abs(k_dx) > 4 OrElse Math.Abs(k_dy) > 4) Then
                    _moved = True
                    _panning = True
                    HideTip()
                    Cursor = Cursors.SizeAll
                End If
                If _moved Then
                    _panX = _panStartX + k_dx
                    _panY = _panStartY + k_dy
                    Invalidate()
                End If
                Return
            End If
            SetHover(NodeAt(e.Location))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            If e.Button <> MouseButtons.Left OrElse Not _pressed Then Return
            _pressed = False
            If _panning Then
                _panning = False
                Cursor = Cursors.Default
                SetHover(NodeAt(e.Location))
                Return
            End If
            ' A click, not a drag.
            If _pressedIndex >= 0 Then
                _selectedIndex = _pressedIndex
                Invalidate()
                RaiseEvent NodeClicked(_nodes(_pressedIndex).Key)
            Else
                _selectedIndex = -1
                Invalidate()
                RaiseEvent NodeClicked(Nothing)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDoubleClick(e As MouseEventArgs)
        MyBase.OnMouseDoubleClick(e)
        Try
            Dim k_index As Integer = NodeAt(e.Location)
            If k_index >= 0 Then RaiseEvent NodeDoubleClicked(_nodes(k_index).Key)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnMouseDoubleClick", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Try
            If Not _pressed Then SetHover(-1)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            Dim k_factor As Double = Math.Pow(1.0015, e.Delta)
            Dim k_newZoom As Double = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * k_factor))
            Dim k_ratio As Double = k_newZoom / _zoom
            _panX = e.X - (e.X - _panX) * k_ratio
            _panY = e.Y - (e.Y - _panY) * k_ratio
            _zoom = k_newZoom
            HideTip()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnMouseWheel", ex)
        End Try
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Home
                    FitToView()
                    e.Handled = True
                Case Keys.Escape
                    If _selectedIndex >= 0 Then
                        _selectedIndex = -1
                        Invalidate()
                        RaiseEvent NodeClicked(Nothing)
                    End If
                    e.Handled = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphView.OnKeyDown", ex)
        End Try
    End Sub

    ' =====================================================================
    ' HOVER AND THE FLOATING LABEL
    ' =====================================================================

    Private Sub SetHover(k_index As Integer)
        If k_index = _hoverIndex Then Return
        _hoverIndex = k_index
        Cursor = If(k_index >= 0, Cursors.Hand, Cursors.Default)
        Invalidate()
        If k_index < 0 Then
            HideTip()
            RaiseEvent NodeHovered(Nothing)
            Return
        End If
        Dim k_node As KBotGraphNode = _nodes(k_index)
        ShowTip("n:" & k_index.ToString(Globalization.CultureInfo.InvariantCulture), k_node.Text, k_node.Tooltip)
        RaiseEvent NodeHovered(k_node.Key)
    End Sub

    Private Sub ShowTip(k_key As String, k_header As String, k_body As String)
        If KBotDesignTime.IsDesignTime(Me) OrElse Not _pointTooltipEnabled Then Return
        If String.Equals(k_key, _currentTipKey, StringComparison.Ordinal) Then Return
        _currentTipKey = k_key
        If String.IsNullOrEmpty(k_header) AndAlso String.IsNullOrEmpty(k_body) Then
            _pointTooltip?.HideNow()
            Return
        End If
        _tipContent.HeaderText = If(k_header, String.Empty)
        _tipContent.Text = If(k_body, String.Empty)
        _tipContent.FooterText = String.Empty
        PointTooltip.ShowAt(Me, _tipContent, Cursor.Position)
    End Sub

    Private Sub HideTip()
        _currentTipKey = Nothing
        _pointTooltip?.HideNow()
    End Sub

End Class
