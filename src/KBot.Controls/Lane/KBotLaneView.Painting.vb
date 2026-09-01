Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' LAYOUT, PAINTING, SCROLLING, MOUSE AND KEYBOARD of <see cref="KBotLaneView"/>.
'''
''' <para>Split off from the property file for the same reason the chart and the tree are split:
''' the settings a host reads in the property grid and the arithmetic that turns them into pixels
''' are two different jobs, and keeping them apart is what stops a measure from being scaled in one
''' place and used raw in another.</para>
'''
''' <para><b>One source for the scale.</b> Every logical measure goes through
''' <c>ThemeShapes.ScaleDpi(Me, …)</c>, which resolves to <c>DeviceDpi / 96</c>. Nothing here reads
''' <c>AutoScaleMode.Font</c>'s factor: two sources would make the floor of the control and its
''' drawing disagree, which is exactly the defect slice 0035 went to fix.</para>
'''
''' <para><b>Try/Catch classification.</b> <c>OnPaint</c>, the mouse handlers, the scroll handler
''' and the keyboard handlers are UI boundaries: they log and swallow, because a throw out of a
''' paint body kills the process. The helpers below are reached ONLY through those boundaries, so
''' under the house rule they carry no Try of their own — the boundary is the sink.</para>
''' </summary>
Partial Public NotInheritable Class KBotLaneView

    ' Same theming road as AdvancedTreeControl's scrollbar: a stock VScrollBar cannot be painted,
    ' but it CAN be told which visual style to use, and "DarkMode_Explorer" is the only way a dark
    ' scheme does not end up with a bright white bar down its right edge.
    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
    End Function

    ' =====================================================================
    ' LAYOUT
    ' =====================================================================

    Private Sub EnsureLayout()
        If Not _layoutValid Then RecalcLayout()
    End Sub

    ''' <summary>
    ''' Cuts the control into its band and its surface, works out the time range, then stacks the
    ''' lanes and puts every marker on the axis.
    ''' </summary>
    ''' <remarks>
    ''' The order matters: the gutters have to be known BEFORE the surface rectangle, because the
    ''' caption gutter on the left and the end-mark gutter on the right are what the time axis
    ''' gives up. Doing it the other way round makes the last marker of every lane sit under the
    ''' end mark.
    ''' </remarks>
    Private Sub RecalcLayout()
        _layoutValid = True
        _headerRect = Rectangle.Empty
        _enlargeRect = Rectangle.Empty
        _plotRect = Rectangle.Empty

        Dim client As Rectangle = ClientRectangle
        If client.Width <= 0 OrElse client.Height <= 0 Then Return

        Dim rest As Rectangle = client
        If _headerVisible AndAlso _headerHeight > 0 Then
            Dim hh As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _headerHeight), rest.Height)
            _headerRect = New Rectangle(rest.Left, rest.Top, rest.Width, hh)
            rest = New Rectangle(rest.Left, rest.Top + hh, rest.Width, rest.Height - hh)
            LayoutEnlargeButton()
        End If

        ComputeRanges()

        Dim margin As Integer = ThemeShapes.ScaleDpi(Me, _plotMargin)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)

        Dim bottomGutter As Integer = 0
        If _axisVisible Then
            bottomGutter = TextRenderer.MeasureText("0", EffectiveAxisFont()).Height + gap
        End If

        ' The caption gutter and the end-mark gutter are BOTH reserved unconditionally whenever
        ' they are switched on — not "only while something needs them". A gutter that appears the
        ' moment one lane gains an end mark would slide the whole time axis sideways under the
        ' operator's pointer, mid-drag, which is a far worse trade than a few unused pixels.
        Dim leftGutter As Integer = If(_laneCaptionsVisible, ThemeShapes.ScaleDpi(Me, _laneCaptionWidth) + gap, 0)
        Dim rightGutter As Integer = If(_endMarkSize > 0, ThemeShapes.ScaleDpi(Me, _endMarkSize) + gap, 0)

        Dim surface As New Rectangle(rest.Left + margin, rest.Top + margin,
                                     Math.Max(0, rest.Width - margin * 2),
                                     Math.Max(0, rest.Height - margin * 2 - bottomGutter))

        LayoutScrollBar(surface)
        If _vScroll.Visible Then surface.Width = Math.Max(0, surface.Width - _vScroll.Width)

        _plotRect = New Rectangle(surface.Left + leftGutter, surface.Top,
                                  Math.Max(0, surface.Width - leftGutter - rightGutter),
                                  surface.Height)

        LayoutLanes(surface)
        ProjectMarkers()
        ProjectGuides()
    End Sub

    ''' <summary>
    ''' The time span of everything drawn — from the markers, unless the host pinned it.
    ''' </summary>
    ''' <remarks>
    ''' A single marker, or several sharing one moment, gives a span of zero. That is not an error
    ''' and it is not padded away: the markers are drawn in the middle of the surface, which is the
    ''' only honest place for events that have nothing to be earlier or later than.
    ''' </remarks>
    Private Sub ComputeRanges()
        Dim any As Boolean = False
        Dim minTicks As Double = 0
        Dim maxTicks As Double = 0

        For Each ln As KBotLane In _lanes
            If Not ln.Visible Then Continue For
            For Each m As KBotLaneMarker In ln.Markers
                If Not m.Visible Then Continue For
                Dim t As Double = CDbl(m.Moment.Ticks)
                If Not any Then
                    minTicks = t : maxTicks = t
                    any = True
                Else
                    If t < minTicks Then minTicks = t
                    If t > maxTicks Then maxTicks = t
                End If
            Next
        Next

        ' A pinned range wins outright, even over an empty surface: a host that pinned it did so
        ' to line this control up with another one, and an axis that quietly reverted would break
        ' exactly the alignment it was pinned for.
        If _rangeStart <> Date.MinValue Then
            minTicks = CDbl(_rangeStart.Ticks)
            any = True
        End If
        If _rangeEnd <> Date.MinValue Then
            maxTicks = CDbl(_rangeEnd.Ticks)
            any = True
        End If

        If Not any OrElse maxTicks < minTicks Then
            _minTicks = 0 : _maxTicks = 0
            _minMoment = Date.MinValue : _maxMoment = Date.MinValue
            Return
        End If

        _minTicks = minTicks
        _maxTicks = maxTicks
        _minMoment = New Date(CLng(minTicks))
        _maxMoment = New Date(CLng(maxTicks))
    End Sub

    ''' <summary>
    ''' Stacks the visible lanes top to bottom inside <paramref name="surface"/>, already shifted
    ''' by the scroll position, and records how tall the whole stack is.
    ''' </summary>
    ''' <remarks>
    ''' A lane scrolled out of view still gets a rectangle — one that falls outside the surface.
    ''' That is deliberate: painting and hit-testing both clip to the surface, so there is exactly
    ''' one rule ("is this rectangle inside?") instead of two ways of being absent.
    ''' </remarks>
    Private Sub LayoutLanes(surface As Rectangle)
        Dim lh As Integer = ThemeShapes.ScaleDpi(Me, _laneHeight)
        Dim spacing As Integer = ThemeShapes.ScaleDpi(Me, _laneSpacing)
        Dim sep As Integer = If(_separatorWidth > 0, ThemeShapes.ScaleDpi(Me, _separatorWidth) + spacing, 0)

        Dim y As Integer = surface.Top - _vScroll.Value
        Dim total As Integer = 0
        For Each ln As KBotLane In _lanes
            If Not ln.Visible Then
                ln.Bounds = Rectangle.Empty
                Continue For
            End If
            If ln.SeparatorAbove AndAlso total > 0 Then
                y += sep
                total += sep
            End If
            ln.Bounds = New Rectangle(surface.Left, y, surface.Width, lh)
            y += lh + spacing
            total += lh + spacing
        Next
        _contentHeight = Math.Max(0, total - spacing)
    End Sub

    ''' <summary>
    ''' Shows or hides the scrollbar and sets its range. Called BEFORE the lanes are stacked, so
    ''' the surface they are stacked into is already the narrower one when the bar is up.
    ''' </summary>
    ''' <remarks>
    ''' The content height used here is the one measured on the PREVIOUS pass. That is not a bug
    ''' waiting to happen: the stack only changes when the lanes change, and every path that
    ''' changes them invalidates the layout, so the next pass corrects itself. Measuring the stack
    ''' twice per layout to avoid a one-frame lag on a scrollbar is not a trade worth making.
    ''' </remarks>
    Private Sub LayoutScrollBar(surface As Rectangle)
        If KBotDesignTime.IsDesignTime(Me) Then
            _vScroll.Visible = False
            Return
        End If

        Dim viewport As Integer = Math.Max(1, surface.Height)
        Dim needed As Boolean = _contentHeight > viewport
        If Not needed Then
            If _vScroll.Visible Then
                _vScroll.Value = 0
                _vScroll.Visible = False
            End If
            Return
        End If

        _vScroll.Width = SystemInformation.VerticalScrollBarWidth
        _vScroll.Left = Math.Max(0, surface.Right - _vScroll.Width)
        _vScroll.Top = surface.Top
        _vScroll.Height = viewport
        _vScroll.SmallChange = Math.Max(1, ThemeShapes.ScaleDpi(Me, _laneHeight + _laneSpacing))
        _vScroll.LargeChange = viewport
        ' WinForms: the largest reachable Value is Maximum - LargeChange + 1, so Maximum has to be
        ' the content height plus the viewport minus one for the last lane to come fully into view.
        _vScroll.Maximum = _contentHeight + viewport - 1
        If _vScroll.Value > _contentHeight - viewport Then
            _vScroll.Value = Math.Max(0, _contentHeight - viewport)
        End If
        _vScroll.Visible = True
    End Sub

    Private Sub ApplyScrollBarTheme()
        If _vScroll Is Nothing OrElse Not _vScroll.IsHandleCreated Then Return
        Dim unused As Integer = SetWindowTheme(_vScroll.Handle,
                                               If(_isDarkScheme, "DarkMode_Explorer", "Explorer"), Nothing)
    End Sub

    Private Sub OnVScrollScroll(sender As Object, e As ScrollEventArgs)
        Try
            ' The bar moved, so every lane rectangle is now wrong. Nothing else changed, but the
            ' whole stack is recomputed anyway: a surface patched by an offset and a surface laid
            ' out from scratch drift apart, and here the one that lies is the screen.
            InvalidateLaneLayout()
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnVScrollScroll", ex)
        End Try
    End Sub

    ''' <summary>Lays the enlarge button out at the right end of the band.</summary>
    Private Sub LayoutEnlargeButton()
        _enlargeRect = Rectangle.Empty
        If Not _enlargeButtonVisible Then Return
        If _headerRect.Width <= 0 OrElse _headerRect.Height <= 0 Then Return

        Dim w As Integer = ThemeShapes.ScaleDpi(Me, _enlargeButtonSize.Width)
        Dim h As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _enlargeButtonSize.Height), _headerRect.Height)
        Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 8)
        If w + pad * 2 > _headerRect.Width Then Return

        _enlargeRect = New Rectangle(_headerRect.Right - pad - w,
                                     _headerRect.Top + (_headerRect.Height - h) \ 2, w, h)
    End Sub

    Private Sub ProjectMarkers()
        Dim plotted As Boolean = _plotRect.Width > 0 AndAlso _plotRect.Height > 0
        For Each ln As KBotLane In _lanes
            Dim laneDrawn As Boolean = plotted AndAlso ln.Visible AndAlso ln.Bounds.Height > 0
            For Each m As KBotLaneMarker In ln.Markers
                If Not laneDrawn OrElse Not m.Visible Then
                    m.Plotted = False
                    m.PlotLocation = Point.Empty
                    Continue For
                End If
                Dim t As Double = CDbl(m.Moment.Ticks)
                ' Only reachable when the host PINNED a range narrower than its own data. Left
                ' undrawn rather than clamped to the edge: a marker parked on the boundary would
                ' claim a date it does not have.
                If _maxTicks > _minTicks AndAlso (t < _minTicks OrElse t > _maxTicks) Then
                    m.Plotted = False
                    m.PlotLocation = Point.Empty
                    Continue For
                End If
                m.PlotLocation = New Point(MomentToX(m.Moment), ln.Bounds.Top + ln.Bounds.Height \ 2)
                m.Plotted = True
            Next
        Next
    End Sub

    Private Sub ProjectGuides()
        Dim plotted As Boolean = _plotRect.Width > 0 AndAlso _plotRect.Height > 0
        For Each gd As KBotChartGuide In _guides
            If Not plotted OrElse Not gd.Visible Then
                gd.PlotX = -1
                Continue For
            End If
            Dim t As Double = CDbl(gd.Moment.Ticks)
            If t < _minTicks OrElse t > _maxTicks Then
                gd.PlotX = -1
                Continue For
            End If
            gd.PlotX = MomentToX(gd.Moment)
        Next
    End Sub

    ''' <summary>
    ''' The run of the time axis in device pixels: the surface, less whatever
    ''' <see cref="TrailingSpace"/> keeps free past the latest moment.
    ''' </summary>
    ''' <remarks>
    ''' The trailing room is taken out of the AXIS, not out of the surface, so the stretch owned by
    ''' the latest marker runs into it and everything else — markers, guides — stays on the same
    ''' relative dates. Clamped to a quarter of the surface: a trailing space large enough to
    ''' squash the axis has stopped being room for the last stretch and become a second, empty
    ''' surface.
    ''' </remarks>
    Private Function AxisRun() As Integer
        If _trailingSpace <= 0 Then Return _plotRect.Width
        Dim trailing As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _trailingSpace), _plotRect.Width \ 4)
        Return Math.Max(0, _plotRect.Width - trailing)
    End Function

    Private Function MomentToX(moment As Date) As Integer
        Dim span As Double = _maxTicks - _minTicks
        ' Everything at the same instant: one column in the middle, not a stack pinned to the left.
        If span <= 0 Then Return _plotRect.Left + AxisRun() \ 2
        Dim ratio As Double = (CDbl(moment.Ticks) - _minTicks) / span
        Return _plotRect.Left + CInt(Math.Round(ratio * AxisRun()))
    End Function

    Private Function HasAnyVisibleMarker() As Boolean
        For Each ln As KBotLane In _lanes
            If Not ln.Visible Then Continue For
            For Each m As KBotLaneMarker In ln.Markers
                If m.Visible Then Return True
            Next
        Next
        Return False
    End Function

    ' =====================================================================
    ' PAINTING
    ' =====================================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim designTime As Boolean = KBotDesignTime.IsDesignTime(Me)
        Try
            If _updateDepth > 0 Then Return
            EnsureLayout()

            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(BackColor)

            DrawHeaderBand(g, designTime)
            DrawPlotBackground(g)

            If _lanes.Count > 0 Then
                ' Everything below the band is clipped to the surface, so a lane scrolled halfway
                ' out is cut cleanly instead of spilling over the band or the axis.
                Dim oldClip As Region = g.Clip.Clone()
                Try
                    g.SetClip(SurfaceClip(), CombineMode.Intersect)
                    DrawGuides(g)
                    DrawLanes(g)
                    DrawMarkers(g)
                    DrawDropTarget(g)
                Finally
                    g.Clip = oldClip
                    oldClip.Dispose()
                End Try
                If Not HasAnyVisibleMarker() Then DrawEmptyState(g)
                DrawAxis(g)
            Else
                DrawEmptyState(g)
            End If

            DrawOuterBorder(g, designTime)
        Catch ex As Exception
            ' UI boundary: a throw out of a paint body kills the process, so it logs and returns.
            ' Nothing is logged from inside the designer process (see KBotDesignTime).
            If Not designTime Then GlobalErrorLog.Write("KBotLaneView.OnPaint", ex)
        End Try
    End Sub

    ''' <summary>The rectangle everything under the band is clipped to.</summary>
    Private Function SurfaceClip() As Rectangle
        Dim top As Integer = If(_headerRect.Height > 0, _headerRect.Bottom, 0)
        Dim bottom As Integer = _plotRect.Bottom
        Return New Rectangle(0, top, Width, Math.Max(0, bottom - top))
    End Function

    ''' <summary>
    ''' The frame. A RED frame in the designer when two lanes share a key — the defect the runtime
    ''' would have thrown for, reported where a throw would instead stop the form from opening.
    ''' </summary>
    Private Sub DrawOuterBorder(g As Graphics, designTime As Boolean)
        Dim broken As Boolean = designTime AndAlso HasDuplicateOrEmptyKeys()
        If (Not _borderVisible OrElse _borderWidth <= 0) AndAlso Not broken Then Return
        Dim radius As Integer = EffectiveCornerRadius()
        Dim r As New Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1))
        Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
            If broken Then
                Using pen As New Pen(Palette().ErrorColor, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
                    g.DrawPath(pen, path)
                End Using
            Else
                g.DrawPath(BorderPen, path)
            End If
        End Using
    End Sub

    Private Function HasDuplicateOrEmptyKeys() As Boolean
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each ln As KBotLane In _lanes
            If String.IsNullOrWhiteSpace(ln.Key) Then Return True
            If Not seen.Add(ln.Key) Then Return True
        Next
        Return False
    End Function

    Private Function EffectiveCornerRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius,
                                    If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    Private Sub DrawHeaderBand(g As Graphics, designTime As Boolean)
        If _headerRect.Width <= 0 OrElse _headerRect.Height <= 0 Then Return

        Using path As GraphicsPath = ThemeShapes.RoundedRect(_headerRect, 0)
            ThemeShapes.FillModern(g, path, _headerRect, EffectiveHeaderBackColor(), _headerGradient)
        End Using

        If _headerSeparatorWidth > 0 Then
            Using pen As New Pen(EffectiveHeaderSeparatorColor(),
                                 CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, _headerSeparatorWidth))))
                g.DrawLine(pen, _headerRect.Left, _headerRect.Bottom - 1, _headerRect.Right, _headerRect.Bottom - 1)
            End Using
        End If

        If Not String.IsNullOrEmpty(_headerCaption) Then
            Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 8)
            Dim right As Integer = If(_enlargeRect.Width > 0, _enlargeRect.Left - pad, _headerRect.Right - pad)
            Dim r As New Rectangle(_headerRect.Left + pad, _headerRect.Top,
                                   Math.Max(0, right - _headerRect.Left - pad), _headerRect.Height)
            TextRenderer.DrawText(g, _headerCaption, EffectiveHeaderFont(), r, EffectiveHeaderTextColor(),
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.EndEllipsis)
        End If

        DrawEnlargeButton(g, designTime)
    End Sub

    ''' <summary>
    ''' The enlarge button: the host's image, or — when it gave none — two arrows pushing apart,
    ''' drawn from the palette so the button exists whether or not anybody supplied an icon.
    ''' </summary>
    Private Sub DrawEnlargeButton(g As Graphics, designTime As Boolean)
        If _enlargeRect.Width <= 0 OrElse _enlargeRect.Height <= 0 Then Return

        If _hoverEnlarge AndAlso Not designTime Then
            Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 3)
            Dim back As New Rectangle(_enlargeRect.X - pad, _enlargeRect.Y - pad,
                                      _enlargeRect.Width + pad * 2, _enlargeRect.Height + pad * 2)
            Using path As GraphicsPath = ThemeShapes.RoundedRect(back, ThemeShapes.ScaleDpi(Me, 3))
                Using b As New SolidBrush(Palette().ButtonHoverColor)
                    g.FillPath(b, path)
                End Using
            End Using
        End If

        If _enlargeButtonImage IsNot Nothing Then
            g.DrawImage(_enlargeButtonImage, _enlargeRect)
            Return
        End If

        ' The drawn fallback: a corner bracket at the top left and one at the bottom right, with a
        ' diagonal between them — the ordinary "open this bigger" mark.
        Dim c As Color = EffectiveHeaderTextColor()
        Dim w As Single = CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 1)))
        Dim r2 As Rectangle = _enlargeRect
        Dim arm As Integer = Math.Max(2, r2.Width \ 3)
        Using pen As New Pen(c, w)
            g.DrawLine(pen, r2.Left, r2.Top, r2.Left + arm, r2.Top)
            g.DrawLine(pen, r2.Left, r2.Top, r2.Left, r2.Top + arm)
            g.DrawLine(pen, r2.Right - arm, r2.Bottom, r2.Right, r2.Bottom)
            g.DrawLine(pen, r2.Right, r2.Bottom - arm, r2.Right, r2.Bottom)
            g.DrawLine(pen, r2.Left + 1, r2.Top + 1, r2.Right - 1, r2.Bottom - 1)
        End Using
    End Sub

    Private Sub DrawPlotBackground(g As Graphics)
        If _plotRect.Width <= 0 OrElse _plotRect.Height <= 0 Then Return
        Dim r As Rectangle = SurfaceClip()
        If r.Height <= 0 Then Return
        Using b As New SolidBrush(EffectivePlotBackColor())
            g.FillRectangle(b, r)
        End Using
    End Sub

    ''' <summary>
    ''' The dated lines, top to bottom of the whole surface, thin and dotted.
    ''' </summary>
    ''' <remarks>
    ''' Across ALL lanes rather than per lane, because that is the whole point of them: the
    ''' operator is asking "is this marker before or after that payment", and the answer is only
    ''' free to read when the line runs past every lane the marker could be dropped on.
    ''' </remarks>
    Private Sub DrawGuides(g As Graphics)
        If _guides.Count = 0 Then Return
        Dim surface As Rectangle = SurfaceClip()
        If surface.Height <= 0 Then Return
        Dim fallback As Color = Palette().TextDimColor
        Dim width As Single = CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 1)))
        For i As Integer = 0 To _guides.Count - 1
            Dim gd As KBotChartGuide = _guides(i)
            If gd.PlotX < 0 Then Continue For
            Using pen As New Pen(If(gd.LineColor = Color.Empty, fallback, gd.LineColor), width)
                pen.DashStyle = If(i = _hoverGuideIndex, DashStyle.Solid, gd.DashStyle)
                g.DrawLine(pen, gd.PlotX, surface.Top, gd.PlotX, surface.Bottom)
            End Using
        Next
    End Sub

    ''' <summary>The rails, the separators, the captions and the end marks.</summary>
    Private Sub DrawLanes(g As Graphics)
        Dim surface As Rectangle = SurfaceClip()
        Dim capFont As Font = EffectiveAxisFont()
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)
        Dim sepW As Integer = ThemeShapes.ScaleDpi(Me, Math.Max(1, _separatorWidth))

        For i As Integer = 0 To _lanes.Count - 1
            Dim ln As KBotLane = _lanes(i)
            If Not ln.Visible OrElse ln.Bounds.Height <= 0 Then Continue For
            If ln.Bounds.Bottom < surface.Top OrElse ln.Bounds.Top > surface.Bottom Then Continue For

            If ln.SeparatorAbove AndAlso _separatorWidth > 0 Then
                Dim y As Integer = ln.Bounds.Top - ThemeShapes.ScaleDpi(Me, _laneSpacing) - sepW \ 2
                Using pen As New Pen(SeparatorPen.Color, CSng(sepW))
                    g.DrawLine(pen, ln.Bounds.Left, y, ln.Bounds.Right, y)
                End Using
            End If

            ' The lane under the pointer gets a wash, so a drag has something to aim at even where
            ' the lane happens to hold no marker at all.
            If i = _hoverLaneIndex Then
                Using b As New SolidBrush(EffectiveLaneHoverBackColor())
                    g.FillRectangle(b, ln.Bounds)
                End Using
            End If

            If _laneLineWidth > 0 Then
                Dim mid As Integer = ln.Bounds.Top + ln.Bounds.Height \ 2
                ' The plain rail, always, full width and underneath: a lane holding no marker has
                ' to stay visible as somewhere to drop, and the run before the first marker has to
                ' read as empty rather than as absent.
                Using pen As New Pen(LaneLinePen.Color, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, _laneLineWidth))))
                    g.DrawLine(pen, _plotRect.Left, mid, _plotRect.Right, mid)
                End Using
                If _segmentedRail Then DrawLaneSegments(g, ln, i, mid)
            End If

            If _laneCaptionsVisible AndAlso _laneCaptionWidth > 0 AndAlso Not String.IsNullOrEmpty(ln.Text) Then
                Dim r As New Rectangle(ln.Bounds.Left, ln.Bounds.Top,
                                       Math.Max(0, _plotRect.Left - ln.Bounds.Left - gap), ln.Bounds.Height)
                TextRenderer.DrawText(g, ln.Text, capFont, r, EffectiveLaneColor(ln, i),
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                      TextFormatFlags.EndEllipsis)
            End If

            DrawEndMark(g, ln)
        Next
    End Sub

    ''' <summary>
    ''' The stretch each marker OWNS: from itself to the next marker along, and — for the last one
    ''' — to the right-hand end of the surface, in that marker's own colour.
    ''' </summary>
    ''' <remarks>
    ''' <para>A statement about the data, not decoration: what a marker records holds until the
    ''' next marker changes it. That is the same truth <c>KBotChartView</c> draws as a step line,
    ''' and drawing it here too is what lets the operator see, at the moment of a drop, which
    ''' stretch of the lane a snapshot has just taken over.</para>
    ''' <para>Ordered by X and not by the host's order: the collection is deliberately left
    ''' unsorted (the doc says so), and a stretch drawn to a marker that is to its LEFT would run
    ''' backwards over the one before it.</para>
    ''' <para>Two markers on the same pixel column own nothing, and nothing is drawn for them —
    ''' the same answer the surface already gives for several saves inside one minute.</para>
    ''' </remarks>
    Private Sub DrawLaneSegments(g As Graphics, ln As KBotLane, laneIndex As Integer, mid As Integer)
        If ln.Markers.Count = 0 Then Return
        Dim laneColor As Color = EffectiveLaneColor(ln, laneIndex)

        Dim drawn As New List(Of KBotLaneMarker)()
        For Each m As KBotLaneMarker In ln.Markers
            If m.Plotted Then drawn.Add(m)
        Next
        If drawn.Count = 0 Then Return
        drawn.Sort(Function(a, b) a.PlotLocation.X.CompareTo(b.PlotLocation.X))

        Dim logical As Integer = If(_segmentWidth > 0, _segmentWidth, _laneLineWidth)
        Dim w As Single = CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, logical)))

        For k As Integer = 0 To drawn.Count - 1
            Dim m As KBotLaneMarker = drawn(k)
            ' A Loose marker owns NOTHING. It is not placed on anything, so a stretch running from
            ' it to the next one would draw a chain out of a row of things that are precisely not a
            ' chain — the one claim the unplaced lane must never make.
            If m.Style = KBotLaneMarkerStyle.Loose Then Continue For
            Dim x1 As Integer = m.PlotLocation.X
            Dim x2 As Integer = If(k < drawn.Count - 1, drawn(k + 1).PlotLocation.X, _plotRect.Right)
            If x2 <= x1 Then Continue For
            Using pen As New Pen(If(m.MarkerColor = Color.Empty, laneColor, m.MarkerColor), w)
                g.DrawLine(pen, x1, mid, x2, mid)
            End Using
        Next
    End Sub

    ''' <summary>
    ''' The mark at the closed end of a lane — F15 as a SIGN, in the right-hand gutter.
    ''' </summary>
    ''' <remarks>
    ''' Success green for "closes", warning amber for "does not". Amber and not red, deliberately:
    ''' a chain that does not close is something to look at, not something that has gone wrong, and
    ''' red in this application is reserved for the second meaning.
    ''' </remarks>
    Private Sub DrawEndMark(g As Graphics, ln As KBotLane)
        If ln.EndMark = KBotLaneEndMark.None OrElse _endMarkSize <= 0 Then Return
        Dim side As Integer = ThemeShapes.ScaleDpi(Me, _endMarkSize)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)
        Dim r As New Rectangle(_plotRect.Right + gap,
                               ln.Bounds.Top + (ln.Bounds.Height - side) \ 2, side, side)
        Dim c As Color = If(ln.EndMark = KBotLaneEndMark.Ok, Palette().SuccessColor, Palette().WarningColor)
        Using pen As New Pen(c, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            If ln.EndMark = KBotLaneEndMark.Ok Then
                ' A tick.
                g.DrawLine(pen, r.Left, r.Top + r.Height \ 2, r.Left + r.Width \ 3, r.Bottom - 1)
                g.DrawLine(pen, r.Left + r.Width \ 3, r.Bottom - 1, r.Right - 1, r.Top)
            Else
                ' An exclamation: a stroke and a dot under it.
                Dim x As Integer = r.Left + r.Width \ 2
                g.DrawLine(pen, x, r.Top, x, r.Top + CInt(r.Height * 0.6))
                g.DrawLine(pen, x, r.Bottom - 1, x, r.Bottom - 1)
            End If
        End Using
    End Sub

    Private Sub DrawMarkers(g As Graphics)
        Dim surface As Rectangle = SurfaceClip()
        Dim side As Integer = ThemeShapes.ScaleDpi(Me, _markerSize)
        Dim labelFont As Font = EffectiveAxisFont()

        For i As Integer = 0 To _lanes.Count - 1
            Dim ln As KBotLane = _lanes(i)
            If Not ln.Visible OrElse ln.Bounds.Height <= 0 Then Continue For
            If ln.Bounds.Bottom < surface.Top OrElse ln.Bounds.Top > surface.Bottom Then Continue For
            Dim laneColor As Color = EffectiveLaneColor(ln, i)

            For j As Integer = 0 To ln.Markers.Count - 1
                Dim m As KBotLaneMarker = ln.Markers(j)
                If Not m.Plotted Then Continue For
                Dim c As Color = If(m.MarkerColor = Color.Empty, laneColor, m.MarkerColor)
                Dim hovered As Boolean = (i = _hoverLaneIndex AndAlso j = _hoverMarkerIndex)
                DrawMarker(g, m, m.PlotLocation, side, c, hovered)

                If _markerLabelsVisible AndAlso Not String.IsNullOrEmpty(m.Text) Then
                    Dim r As New Rectangle(m.PlotLocation.X + side, ln.Bounds.Top,
                                           Math.Max(0, _plotRect.Right - m.PlotLocation.X - side), ln.Bounds.Height)
                    TextRenderer.DrawText(g, m.Text, labelFont, r, c,
                                          TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or
                                          TextFormatFlags.NoPadding Or TextFormatFlags.EndEllipsis)
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' One marker, in the shape its <see cref="KBotLaneMarker.Style"/> asks for.
    ''' </summary>
    ''' <remarks>
    ''' <b>Nothing is greyed out.</b> A <c>Locked</c> marker is drawn in FULL colour with a padlock
    ''' over it: dimming was tried on the chart in slice 0048-06 and, on a chain where most links
    ''' are locked, turned the whole surface grey — it stopped saying anything, and the row it was
    ''' meant to be paired with had nothing left to pair with. "Out of play" is written by the
    ''' glyph; the colour is left free for the one job nothing else can do, which is tying a marker
    ''' to the same fact somewhere else on screen.
    ''' </remarks>
    Private Sub DrawMarker(g As Graphics, m As KBotLaneMarker, center As Point, side As Integer,
                           c As Color, hovered As Boolean)
        Dim r As New Rectangle(center.X - side \ 2, center.Y - side \ 2, side, side)
        Dim thin As Single = CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 1)))

        Using fill As New SolidBrush(c), edge As New Pen(EffectivePlotBackColor(), thin)
            Select Case m.Style
                Case KBotLaneMarkerStyle.Loose
                    Dim pts() As Point = {
                        New Point(center.X, r.Top),
                        New Point(r.Right, center.Y),
                        New Point(center.X, r.Bottom),
                        New Point(r.Left, center.Y)}
                    g.FillPolygon(fill, pts)
                    g.DrawPolygon(edge, pts)

                Case KBotLaneMarkerStyle.NoChange
                    ' Hollow, with an "=" inside: the shape says "this recorded nothing", so the
                    ' operator is not left explaining a duplicate number to themselves.
                    Using back As New SolidBrush(EffectivePlotBackColor())
                        g.FillEllipse(back, r)
                    End Using
                    Using pen As New Pen(c, thin)
                        g.DrawEllipse(pen, r)
                        Dim x1 As Integer = r.Left + r.Width \ 4
                        Dim x2 As Integer = r.Right - r.Width \ 4
                        g.DrawLine(pen, x1, center.Y - Math.Max(1, r.Height \ 6), x2, center.Y - Math.Max(1, r.Height \ 6))
                        g.DrawLine(pen, x1, center.Y + Math.Max(1, r.Height \ 6), x2, center.Y + Math.Max(1, r.Height \ 6))
                    End Using

                Case KBotLaneMarkerStyle.Deletion
                    ' A cross cap: the end of a chain has to read as an end, not as one more entry.
                    Using pen As New Pen(c, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
                        pen.StartCap = LineCap.Round
                        pen.EndCap = LineCap.Round
                        g.DrawLine(pen, r.Left, r.Top, r.Right, r.Bottom)
                        g.DrawLine(pen, r.Right, r.Top, r.Left, r.Bottom)
                    End Using

                Case KBotLaneMarkerStyle.Locked
                    g.FillEllipse(fill, r)
                    g.DrawEllipse(edge, r)
                    DrawPadlock(g, r, EffectivePlotBackColor())

                Case Else
                    g.FillEllipse(fill, r)
                    g.DrawEllipse(edge, r)
            End Select
        End Using

        If hovered Then
            Dim grow As Integer = Math.Max(2, ThemeShapes.ScaleDpi(Me, 3))
            Dim ring As New Rectangle(r.X - grow, r.Y - grow, r.Width + grow * 2, r.Height + grow * 2)
            Using pen As New Pen(c, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
                g.DrawEllipse(pen, ring)
            End Using
        End If
    End Sub

    ''' <summary>
    ''' A padlock inside <paramref name="r"/>: a body and a shackle over it.
    ''' </summary>
    ''' <remarks>
    ''' At the compact marker size this is two or three pixels of detail and reads as "something is
    ''' on this one" rather than as a recognisable padlock. That is accepted: the fact is already
    ''' written twice over on the row beside it, and the shape only has to be different enough that
    ''' the operator does not try to drag it. Enlarged, it is a padlock.
    ''' </remarks>
    Private Sub DrawPadlock(g As Graphics, r As Rectangle, c As Color)
        Dim bodyH As Integer = Math.Max(2, r.Height \ 2)
        Dim bodyW As Integer = Math.Max(2, r.Width - r.Width \ 3)
        Dim body As New Rectangle(r.Left + (r.Width - bodyW) \ 2, r.Bottom - bodyH - Math.Max(1, r.Height \ 8),
                                  bodyW, bodyH)
        Using b As New SolidBrush(c)
            g.FillRectangle(b, body)
        End Using
        Using pen As New Pen(c, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 1))))
            Dim shackle As New Rectangle(body.Left + body.Width \ 4, body.Top - body.Height \ 2,
                                         Math.Max(1, body.Width \ 2), Math.Max(1, body.Height))
            g.DrawArc(pen, shackle, 180, 180)
        End Using
    End Sub

    ''' <summary>The two end dates, under the surface.</summary>
    ''' <remarks>
    ''' Only the two ends. A real time axis is not regular, so evenly spaced labels in between
    ''' would name moments at which nothing happened — the same rule the chart follows.
    ''' </remarks>
    Private Sub DrawAxis(g As Graphics)
        If Not _axisVisible OrElse _plotRect.Width <= 0 Then Return
        If _minMoment = Date.MinValue Then Return
        Dim f As Font = EffectiveAxisFont()
        Dim fore As Color = If(_axisTextColor = Color.Empty, Palette().TextDimColor, _axisTextColor)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)
        Dim h As Integer = TextRenderer.MeasureText("0", f).Height
        Dim y As Integer = _plotRect.Bottom + gap

        ' The axis names the ends of the TIME RUN, not the ends of the rectangle: with a trailing
        ' space the two are no longer the same place, and a date written under empty room would
        ' name a moment nothing on the surface stands at.
        Dim run As Integer = AxisRun()
        TextRenderer.DrawText(g, _minMoment.ToString(_momentFormat), f,
                              New Rectangle(_plotRect.Left, y, run \ 2, h), fore,
                              TextFormatFlags.Left)
        If _maxTicks > _minTicks Then
            TextRenderer.DrawText(g, _maxMoment.ToString(_momentFormat), f,
                                  New Rectangle(_plotRect.Left + run \ 2, y, run - run \ 2, h),
                                  fore, TextFormatFlags.Right)
        End If
    End Sub

    Private Sub DrawEmptyState(g As Graphics)
        If String.IsNullOrEmpty(_emptyText) Then Return
        Dim r As Rectangle = SurfaceClip()
        If r.Width <= 0 OrElse r.Height <= 0 Then Return
        Dim fore As Color = If(_emptyTextColor = Color.Empty, Palette().TextDimColor, _emptyTextColor)
        TextRenderer.DrawText(g, _emptyText, Font, r, fore,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.WordBreak)
    End Sub

    ''' <summary>
    ''' The colour of a lane: the one it was given, or the index-th of the automatic set.
    ''' </summary>
    Private Function EffectiveLaneColor(ln As KBotLane, index As Integer) As Color
        If ln.LaneColor <> Color.Empty Then Return ln.LaneColor
        Return AutoColor(index)
    End Function

    ' =====================================================================
    ' MOUSE
    ' =====================================================================

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
            EnsureLayout()

            If _enlargeRect.Width > 0 AndAlso _enlargeRect.Contains(e.Location) Then
                ' In the designer a press does NOT act: opening a window from inside Visual Studio
                ' is the one thing a drawn button must never do (same rule as the chart's tabs).
                If KBotDesignTime.IsDesignTime(Me) Then Return
                RaiseEvent EnlargeRequested()
                Return
            End If

            If KBotDesignTime.IsDesignTime(Me) Then Return
            Dim li As Integer = -1
            Dim mi As Integer = -1
            HitTestMarker(e.Location, li, mi)
            If li >= 0 Then ArmDrag(_lanes(li).Markers(mi), e.Location, e.Button)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            CancelDragArming()
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            ' The drag takes over the whole modal loop, so the rest of this handler has nothing
            ' left to do when it returns — the pointer is somewhere else entirely by then.
            If MaybeBeginDrag(e.Location, e.Button) Then Return

            EnsureLayout()

            Dim overEnlarge As Boolean = _enlargeRect.Width > 0 AndAlso _enlargeRect.Contains(e.Location)
            If overEnlarge <> _hoverEnlarge Then
                _hoverEnlarge = overEnlarge
                Invalidate()
            End If

            Dim li As Integer = -1
            Dim mi As Integer = -1
            If Not overEnlarge Then HitTestMarker(e.Location, li, mi)

            ' A lane is hovered whenever the pointer is over its band, marker or not: the wash it
            ' gets is what tells the operator that an empty lane is still somewhere to drop.
            Dim laneOnly As Integer = If(overEnlarge, -1, LaneIndexAt(e.Location))
            Dim laneHover As Integer = If(li >= 0, li, laneOnly)

            ' Only once nothing else has the pointer: a marker can be dragged, a guide cannot, so
            ' the marker always wins the pixel they share.
            Dim gi As Integer = -1
            If Not overEnlarge AndAlso li < 0 Then gi = HitTestGuide(e.Location)

            If laneHover <> _hoverLaneIndex OrElse mi <> _hoverMarkerIndex OrElse gi <> _hoverGuideIndex Then
                Dim markerChanged As Boolean = (If(li >= 0, li, -1) <> If(_hoverMarkerIndex >= 0, _hoverLaneIndex, -1)) OrElse
                                               mi <> _hoverMarkerIndex
                _hoverLaneIndex = laneHover
                _hoverMarkerIndex = mi
                _hoverGuideIndex = gi
                Invalidate()
                If markerChanged Then
                    RaiseEvent MarkerHovered(If(mi >= 0 AndAlso laneHover >= 0, _lanes(laneHover).Key, Nothing), mi)
                End If
            End If

            RefreshLaneTip()
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Try
            Dim changed As Boolean = _hoverLaneIndex <> -1 OrElse _hoverGuideIndex <> -1 OrElse _hoverEnlarge
            Dim hadMarker As Boolean = _hoverMarkerIndex <> -1
            _hoverLaneIndex = -1
            _hoverMarkerIndex = -1
            _hoverGuideIndex = -1
            _hoverEnlarge = False
            HideLaneTip()
            If changed Then Invalidate()
            If hadMarker Then RaiseEvent MarkerHovered(Nothing, -1)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Try
            If Not _vScroll.Visible Then Return
            Dim lines As Integer = SystemInformation.MouseWheelScrollLines
            If lines <= 0 Then lines = 3
            Dim delta As Integer = -(e.Delta \ 120) * lines * _vScroll.SmallChange
            Dim top As Integer = Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1)
            _vScroll.Value = Math.Max(0, Math.Min(top, _vScroll.Value + delta))
            InvalidateLaneLayout()
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseWheel", ex)
        End Try
    End Sub

    ''' <summary>The lane whose band contains <paramref name="location"/>, or -1.</summary>
    Friend Function LaneIndexAt(location As Point) As Integer
        If Not SurfaceClip().Contains(location) Then Return -1
        For i As Integer = 0 To _lanes.Count - 1
            Dim ln As KBotLane = _lanes(i)
            If ln.Visible AndAlso ln.Bounds.Height > 0 AndAlso ln.Bounds.Contains(location) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>
    ''' The marker nearest the pointer, within <see cref="HoverRadius"/>.
    ''' </summary>
    ''' <remarks>
    ''' Several markers a minute apart land on the same pixel column and are ALL drawn — the
    ''' nearest simply wins the hunt. Nothing is hidden and nothing is merged: the enlarged window
    ''' is the answer for a cluster that has to be worked on, and the lane's own label carries how
    ''' many markers it holds.
    ''' </remarks>
    Private Sub HitTestMarker(location As Point, ByRef laneIndex As Integer, ByRef markerIndex As Integer)
        laneIndex = -1
        markerIndex = -1
        If _markerSize <= 0 Then Return
        If Not SurfaceClip().Contains(location) Then Return

        Dim reach As Integer = ThemeShapes.ScaleDpi(Me, _hoverRadius)
        Dim best As Double = CDbl(reach) * reach + 1

        For i As Integer = 0 To _lanes.Count - 1
            Dim ln As KBotLane = _lanes(i)
            If Not ln.Visible Then Continue For
            For j As Integer = 0 To ln.Markers.Count - 1
                Dim m As KBotLaneMarker = ln.Markers(j)
                If Not m.Plotted Then Continue For
                Dim dx As Double = m.PlotLocation.X - location.X
                Dim dy As Double = m.PlotLocation.Y - location.Y
                Dim d2 As Double = dx * dx + dy * dy
                If d2 > CDbl(reach) * reach Then Continue For
                If d2 < best Then
                    best = d2
                    laneIndex = i
                    markerIndex = j
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' The guide nearest the pointer, within <see cref="HoverRadius"/>. Returns -1 for none.
    ''' </summary>
    ''' <remarks>
    ''' <b>Horizontal distance only.</b> A guide is a whole column of the surface, not a spot on
    ''' it: the operator points at "that payment", and where their pointer sits vertically says
    ''' nothing about which one they mean.
    ''' </remarks>
    Private Function HitTestGuide(location As Point) As Integer
        If _guides.Count = 0 OrElse Not SurfaceClip().Contains(location) Then Return -1
        Dim reach As Integer = ThemeShapes.ScaleDpi(Me, _hoverRadius)
        Dim best As Integer = reach + 1
        Dim found As Integer = -1
        For i As Integer = 0 To _guides.Count - 1
            Dim gd As KBotChartGuide = _guides(i)
            If gd.PlotX < 0 Then Continue For
            Dim d As Integer = Math.Abs(gd.PlotX - location.X)
            If d <= reach AndAlso d < best Then
                best = d
                found = i
            End If
        Next
        Return found
    End Function

    ' =====================================================================
    ' THE FLOATING LABEL
    ' =====================================================================

    ''' <summary>
    ''' Decides, from the hover state already computed, which label is due. One place: the enlarge
    ''' button, a marker, a lane and a guide cannot be hovered at the same time, so they cannot ask
    ''' for two labels at once.
    ''' </summary>
    Private Sub RefreshLaneTip()
        If Not _markerTooltipEnabled Then Return

        If _hoverEnlarge Then
            ShowLaneTip("enlarge", Nothing, _enlargeButtonTooltip, Nothing)
            Return
        End If

        If _hoverLaneIndex >= 0 AndAlso _hoverLaneIndex < _lanes.Count Then
            Dim ln As KBotLane = _lanes(_hoverLaneIndex)
            If _hoverMarkerIndex >= 0 AndAlso _hoverMarkerIndex < ln.Markers.Count Then
                Dim m As KBotLaneMarker = ln.Markers(_hoverMarkerIndex)
                Dim header As String = If(String.IsNullOrEmpty(m.Text), If(ln.Text, String.Empty), m.Text)
                Dim body As String = If(String.IsNullOrEmpty(m.Tooltip),
                                        m.Moment.ToString(_momentFormat), m.Tooltip)
                ShowLaneTip($"mk:{_hoverLaneIndex}:{_hoverMarkerIndex}", header, body, Nothing)
                Return
            End If
            If Not String.IsNullOrEmpty(ln.Text) OrElse Not String.IsNullOrEmpty(ln.Tooltip) Then
                ShowLaneTip($"ln:{_hoverLaneIndex}", ln.Text, ln.Tooltip, Nothing)
                Return
            End If
        End If

        If _hoverGuideIndex >= 0 AndAlso _hoverGuideIndex < _guides.Count Then
            Dim gd As KBotChartGuide = _guides(_hoverGuideIndex)
            ' A guide with NO text at all opens nothing: an unnamed line is a mark the host chose
            ' not to explain, and a label saying only its date would add nothing.
            If Not String.IsNullOrEmpty(gd.Text) OrElse Not String.IsNullOrEmpty(gd.Tooltip) Then
                Dim body As String = If(String.IsNullOrEmpty(gd.Tooltip),
                                        gd.Moment.ToString(_momentFormat), gd.Tooltip)
                ShowLaneTip($"gd:{_hoverGuideIndex}", gd.Text, body, Nothing)
                Return
            End If
        End If

        HideLaneTip()
    End Sub

    ''' <summary>
    ''' Asks for the label of the thing identified by <paramref name="key"/> (a stable internal
    ''' handle, not the text). The same key twice in a row does nothing — the label stays where it
    ''' is. A new key, or <c>Nothing</c>, puts out what was there.
    ''' </summary>
    Friend Sub ShowLaneTip(key As String, header As String, body As String, footer As String)
        If KBotDesignTime.IsDesignTime(Me) Then Return
        If String.Equals(key, _currentTipKey, StringComparison.Ordinal) Then Return
        _currentTipKey = key

        If String.IsNullOrEmpty(key) OrElse
           (String.IsNullOrEmpty(header) AndAlso String.IsNullOrEmpty(body) AndAlso String.IsNullOrEmpty(footer)) Then
            _markerTooltip?.HideNow()
            Return
        End If

        _tipContent.HeaderText = If(header, String.Empty)
        _tipContent.Text = If(body, String.Empty)
        _tipContent.FooterText = If(footer, String.Empty)
        MarkerTooltip.ShowAt(Me, _tipContent, Cursor.Position)
    End Sub

    ''' <summary>Puts out the label (the pointer left everything that has one).</summary>
    Friend Sub HideLaneTip()
        ShowLaneTip(Nothing, Nothing, Nothing, Nothing)
    End Sub

    ' =====================================================================
    ' KEYBOARD
    ' =====================================================================

    ' Without this the form eats the space before it reaches the control.
    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        If keyData = Keys.Space Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    ''' <summary>
    ''' Space and Enter reach the enlarge button; the arrows scroll.
    ''' </summary>
    ''' <remarks>
    ''' There is no keyboard road to DRAGGING a marker, and that is not an oversight to be filled
    ''' in later: choosing a marker and choosing a lane are two selections this control does not
    ''' have, and inventing them for a gesture the operator performs with the mouse anyway would be
    ''' a second mechanism to keep true.
    ''' </remarks>
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Space, Keys.Enter
                    If _enlargeButtonVisible Then
                        RaiseEvent EnlargeRequested()
                        e.Handled = True
                    End If
                Case Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End
                    If Not _vScroll.Visible Then Return
                    Dim top As Integer = Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1)
                    Dim v As Integer = _vScroll.Value
                    Select Case e.KeyCode
                        Case Keys.Up : v -= _vScroll.SmallChange
                        Case Keys.Down : v += _vScroll.SmallChange
                        Case Keys.PageUp : v -= _vScroll.LargeChange
                        Case Keys.PageDown : v += _vScroll.LargeChange
                        Case Keys.Home : v = 0
                        Case Else : v = top
                    End Select
                    _vScroll.Value = Math.Max(0, Math.Min(top, v))
                    InvalidateLaneLayout()
                    e.Handled = True
            End Select
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnKeyDown", ex)
        End Try
    End Sub

    ' =====================================================================
    ' FRIEND HOOKS FOR TESTS (headless, no screen)
    ' =====================================================================

    ''' <summary>Friend test hook: force a layout pass, without painting.</summary>
    Friend Sub DebugEnsureLayout()
        EnsureLayout()
    End Sub

    ''' <summary>Friend test hook: the computed surface rectangle of the lanes.</summary>
    Friend Function DebugPlotRect() As Rectangle
        EnsureLayout()
        Return _plotRect
    End Function

    ''' <summary>Friend test hook: the computed band of one lane (Empty if hidden).</summary>
    Friend Function DebugLaneBounds(index As Integer) As Rectangle
        EnsureLayout()
        Return _lanes(index).Bounds
    End Function

    ''' <summary>Friend test hook: where a marker landed (Empty if it was not drawn).</summary>
    Friend Function DebugMarkerLocation(laneIndex As Integer, markerIndex As Integer) As Point
        EnsureLayout()
        Return _lanes(laneIndex).Markers(markerIndex).PlotLocation
    End Function

    ''' <summary>Friend test hook: where a guide landed on the horizontal axis (-1 if not drawn).</summary>
    Friend Function DebugGuideX(index As Integer) As Integer
        EnsureLayout()
        Return _guides(index).PlotX
    End Function

    ''' <summary>Friend test hook: the marker nearest a client point, as (laneIndex, markerIndex).</summary>
    Friend Function DebugHitTest(location As Point) As Integer()
        EnsureLayout()
        Dim li As Integer = -1
        Dim mi As Integer = -1
        HitTestMarker(location, li, mi)
        Return New Integer() {li, mi}
    End Function

    ''' <summary>Friend test hook: the guide nearest a client point, or -1.</summary>
    Friend Function DebugHitTestGuide(location As Point) As Integer
        EnsureLayout()
        Return HitTestGuide(location)
    End Function

    ''' <summary>Friend test hook: the whole stack's height in device pixels.</summary>
    Friend Function DebugContentHeight() As Integer
        EnsureLayout()
        Return _contentHeight
    End Function

    ''' <summary>Friend test hook: left click on the real road (band included).</summary>
    Friend Sub DebugClickAt(location As Point)
        OnMouseDown(New MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0))
    End Sub
End Class
