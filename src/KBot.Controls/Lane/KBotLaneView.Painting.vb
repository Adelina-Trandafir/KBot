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

    ' =====================================================================
    ' SCRATCH GDI+ OBJECTS
    ' =====================================================================

    ''' <summary>
    ''' One of the four scratch objects, set up for the shape about to be drawn.
    ''' </summary>
    ''' <remarks>
    ''' EVERY property is written on every call, never only the ones this caller cares about: a
    ''' round cap or a dashed style left behind by the previous shape would surface as a defect
    ''' in some unrelated method three shapes later. See the field declarations for why these
    ''' exist at all.
    ''' </remarks>
    Private Shared Function ScratchPen(ByRef p As Pen, c As Color, w As Single,
                                       Optional cap As LineCap = LineCap.Flat,
                                       Optional dash As DashStyle = DashStyle.Solid) As Pen
        If p Is Nothing Then p = New Pen(c, w)
        p.Color = c
        p.Width = w
        p.StartCap = cap
        p.EndCap = cap
        p.DashStyle = dash
        Return p
    End Function

    ''' <summary>The brush twin of <see cref="ScratchPen"/>.</summary>
    Private Shared Function ScratchBrush(ByRef b As SolidBrush, c As Color) As SolidBrush
        If b Is Nothing Then b = New SolidBrush(c)
        b.Color = c
        Return b
    End Function

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
        Dim mark As Long = KBotLaneLog.Mark()
        _layoutValid = True

        ' First, because everything under it measures the collection this decides on: inside the
        ' designer with no lanes authored, the surface lays out the SAMPLE (KBotLaneView.Preview).
        RefreshPreviewState()

        _headerRect = Rectangle.Empty
        _enlargeRect = Rectangle.Empty
        _plotRect = Rectangle.Empty

        ' The three hairlines the painter uses inside its per-marker loops. Here, once, because
        ' DPI cannot change without the layout being invalidated (OnDpiChangedAfterParent).
        _px1 = Math.Max(1, ThemeShapes.ScaleDpi(Me, 1))
        _px2 = Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))
        _px3 = Math.Max(2, ThemeShapes.ScaleDpi(Me, 3))

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
        If vScroll.Visible Then surface.Width = Math.Max(0, surface.Width - vScroll.Width)

        _plotRect = New Rectangle(surface.Left + leftGutter, surface.Top,
                                  Math.Max(0, surface.Width - leftGutter - rightGutter),
                                  surface.Height)

        LayoutLanes(surface)
        ProjectMarkers()
        ProjectGuides()

        KBotLaneLog.Done("LAYOUT", mark,
                         $"lanes={VisibleLanes.Count} markers={CountVisibleMarkers()} " &
                         $"guides={VisibleGuides.Count} cols={_guideColumns.Count} " &
                         $"client={client.Width}x{client.Height} plot={_plotRect.Width}x{_plotRect.Height} " &
                         $"content={_contentHeight} bar={If(vScroll.Visible, "yes", "no")}" &
                         If(_previewActive, " preview", String.Empty))
    End Sub

    ''' <summary>How many markers the last layout actually put on the surface. For the journal.</summary>
    Private Function CountVisibleMarkers() As Integer
        Dim n As Integer = 0
        For Each ln As KBotLane In VisibleLanes
            n += ln.PlottedInOrder.Count
        Next
        Return n
    End Function

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

        For Each ln As KBotLane In VisibleLanes
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

        Dim y As Integer = surface.Top - vScroll.Value
        Dim total As Integer = 0
        For Each ln As KBotLane In VisibleLanes
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
            vScroll.Visible = False
            Return
        End If

        Dim viewport As Integer = Math.Max(1, surface.Height)
        Dim needed As Boolean = _contentHeight > viewport
        If Not needed Then
            If vScroll.Visible Then
                vScroll.Value = 0
                vScroll.Visible = False
            End If
            Return
        End If

        vScroll.Width = SystemInformation.VerticalScrollBarWidth
        vScroll.Left = Math.Max(0, surface.Right - vScroll.Width)
        vScroll.Top = surface.Top
        vScroll.Height = viewport
        vScroll.SmallChange = Math.Max(1, ThemeShapes.ScaleDpi(Me, _laneHeight + _laneSpacing))
        vScroll.LargeChange = viewport
        ' WinForms: the largest reachable Value is Maximum - LargeChange + 1, so Maximum has to be
        ' the content height plus the viewport minus one for the last lane to come fully into view.
        vScroll.Maximum = _contentHeight + viewport - 1
        If vScroll.Value > _contentHeight - viewport Then
            vScroll.Value = Math.Max(0, _contentHeight - viewport)
        End If
        vScroll.Visible = True
    End Sub

    Private Sub ApplyScrollBarTheme()
        If vScroll Is Nothing OrElse Not vScroll.IsHandleCreated Then Return
        Dim unused As Integer = SetWindowTheme(vScroll.Handle,
                                               If(_isDarkScheme, "DarkMode_Explorer", "Explorer"), Nothing)
    End Sub

    Private Sub OnVScrollScroll(sender As Object, e As ScrollEventArgs) Handles vScroll.Scroll
        Dim mark As Long = KBotLaneLog.Mark()
        Try
            ' The bar moved, so every lane rectangle is now wrong. Nothing else changed, but the
            ' whole stack is recomputed anyway: a surface patched by an offset and a surface laid
            ' out from scratch drift apart, and here the one that lies is the screen.
            InvalidateLaneLayout()
            KBotLaneLog.Done("SCROLL", mark, $"type={e.Type} value={e.NewValue}")
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
        For Each ln As KBotLane In VisibleLanes
            ln.PlottedInOrder.Clear()
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
                ln.PlottedInOrder.Add(m)
            Next
            ' Left to right, ONCE per layout. See KBotLane.PlottedInOrder for why this cannot
            ' stay inside the painter: the collection is deliberately unsorted, and a stretch
            ' drawn to a marker on its left would run backwards over the one before it.
            ln.PlottedInOrder.Sort(Function(a, b) a.PlotLocation.X.CompareTo(b.PlotLocation.X))
        Next
    End Sub

    ''' <summary>
    ''' One dated line as the painter needs it: a column and a colour, nothing else.
    ''' </summary>
    ''' <remarks>
    ''' <para>A guide and a drawn line are NOT the same count. Guides land on the axis by date, and
    ''' the axis is only ever as wide as the surface: a thousand payments spread over a year fall
    ''' on a few hundred distinct pixel columns, and everything past the first line in a column is
    ''' drawing over a line that is already there.</para>
    ''' <para>The colour is part of the identity, not just the position — two guides on the same
    ''' column in two colours are two different lines, and only the second one would be visible.
    ''' <c>0</c> means "the dimmed text colour of the active scheme", so a theme change does not
    ''' make this list stale.</para>
    ''' </remarks>
    Private Structure GuideColumn
        Public X As Integer
        Public Argb As Integer
    End Structure

    Private Sub ProjectGuides()
        Dim plotted As Boolean = _plotRect.Width > 0 AndAlso _plotRect.Height > 0
        _guideColumns.Clear()
        For Each gd As KBotChartGuide In VisibleGuides
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

            Dim col As GuideColumn
            col.X = gd.PlotX
            col.Argb = If(gd.LineColor = Color.Empty, 0, gd.LineColor.ToArgb())
            _guideColumns.Add(col)
        Next
        FoldGuideColumns()
    End Sub

    ''' <summary>
    ''' Sorts the columns and drops the ones that would land on top of each other.
    ''' </summary>
    ''' <remarks>
    ''' Left to right, because the painter walks the list in axis order: sorted, it can skip
    ''' everything to the left of the invalidated strip and STOP at the first column past its right
    ''' edge, instead of testing all thousand every time a lane band repaints.
    ''' </remarks>
    Private Sub FoldGuideColumns()
        If _guideColumns.Count < 2 Then Return
        _guideColumns.Sort(Function(a, b)
                               Dim byX As Integer = a.X.CompareTo(b.X)
                               If byX <> 0 Then Return byX
                               Return a.Argb.CompareTo(b.Argb)
                           End Function)

        Dim kept As Integer = 1
        For i As Integer = 1 To _guideColumns.Count - 1
            Dim col As GuideColumn = _guideColumns(i)
            Dim last As GuideColumn = _guideColumns(kept - 1)
            If col.X = last.X AndAlso col.Argb = last.Argb Then Continue For
            _guideColumns(kept) = col
            kept += 1
        Next
        _guideColumns.RemoveRange(kept, _guideColumns.Count - kept)
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
        For Each ln As KBotLane In VisibleLanes
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
        Dim mark As Long = KBotLaneLog.Mark()
        Dim asked As Integer = _invalidateCount
        _invalidateCount = 0
        Try
            If _updateDepth > 0 Then
                ' Mid-rebuild: nothing is drawn, but the surface still has to be COVERED. With
                ' ControlStyles.Opaque set, WinForms no longer erases it for us, so a bare
                ' Return here would leave whatever the buffer happened to hold.
                e.Graphics.Clear(BackColor)
                Return
            End If
            EnsureLayout()

            Dim g As Graphics = e.Graphics
            ' SMOOTHING OFF BY DEFAULT, and switched on only around the round and the slanted —
            ' the markers, the end marks, the frame, the enlarge glyph. Everything else here is
            ' axis-aligned: the fills, the rails, the dated lines, the separators. Antialiasing
            ' those buys no pixel anyone can see and is paid for by the surface, so it is the one
            ' cost that grows with the WINDOW rather than with the data — which is exactly why
            ' the big window used to crawl while the narrow one felt fine (slice 0058).
            g.SmoothingMode = SmoothingMode.None
            g.Clear(BackColor)

            DrawHeaderBand(g, designTime)
            DrawPlotBackground(g)

            If VisibleLanes.Count > 0 Then
                ' Everything below the band is clipped to the surface, so a lane scrolled halfway
                ' out is cut cleanly instead of spilling over the band or the axis.
                Dim oldClip As Region = g.Clip.Clone()
                Try
                    g.SetClip(SurfaceClip(), CombineMode.Intersect)
                    ' Each phase timed separately. The whole reason this journal exists is that
                    ' "the surface is slow" is not an answer: the guides, the rails and the
                    ' markers grow with three different numbers, and only one of them is ever
                    ' the one eating the frame.
                    Dim phase As Long = KBotLaneLog.Mark()
                    DrawGuides(g, e.ClipRectangle)
                    KBotLaneLog.Done("  guides", phase)

                    phase = KBotLaneLog.Mark()
                    DrawLanes(g)
                    KBotLaneLog.Done("  lanes", phase)

                    phase = KBotLaneLog.Mark()
                    DrawMarkers(g)
                    KBotLaneLog.Done("  markers", phase)

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

            ' `asked` is the number of repaint REQUESTS this one paint answers. It is the number
            ' that explains a surface feeling stuck: a hover that asks for four invalidations and
            ' gets four full paints is a different defect from one paint that is simply slow.
            KBotLaneLog.Done("PAINT", mark,
                             $"clip={e.ClipRectangle.Width}x{e.ClipRectangle.Height}@{e.ClipRectangle.X},{e.ClipRectangle.Y} " &
                             $"lanes={VisibleLanes.Count} markers={CountVisibleMarkers()} asked={asked}" &
                             If(_previewActive, " preview", String.Empty))
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
        ' Rounded corners, so this one does want smoothing — see the note in OnPaint.
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                If broken Then
                    Using pen As New Pen(Palette().ErrorColor, CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2))))
                        g.DrawPath(pen, path)
                    End Using
                Else
                    g.DrawPath(BorderPen, path)
                End If
            End Using
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    ''' <summary>
    ''' Reads the HOST's lanes, never the design-time sample: the red frame reports a defect in
    ''' the data somebody authored, and a sample of ours has no business either raising it or
    ''' hiding it.
    ''' </summary>
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

        ' On the design surface a control with no caption of its own borrows its type name, so a
        ' band sitting above the sample says what it belongs to instead of being a blank strip.
        Dim caption As String = If(String.IsNullOrEmpty(_headerCaption), PreviewCaption(), _headerCaption)
        If Not String.IsNullOrEmpty(caption) Then
            Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 8)
            Dim right As Integer = If(_enlargeRect.Width > 0, _enlargeRect.Left - pad, _headerRect.Right - pad)
            Dim r As New Rectangle(_headerRect.Left + pad, _headerRect.Top,
                                   Math.Max(0, right - _headerRect.Left - pad), _headerRect.Height)
            TextRenderer.DrawText(g, caption, EffectiveHeaderFont(), r, EffectiveHeaderTextColor(),
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
        ' A rounded wash and a diagonal — smoothing on, over a few dozen pixels. See OnPaint.
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            DrawEnlargeGlyph(g, designTime)
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    Private Sub DrawEnlargeGlyph(g As Graphics, designTime As Boolean)
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
        g.FillRectangle(ScratchBrush(_fillBrush, EffectivePlotBackColor()), r)
    End Sub

    ''' <summary>
    ''' The dated lines, top to bottom of the whole surface, one pixel wide and dimmed.
    ''' </summary>
    ''' <remarks>
    ''' <para>Across ALL lanes rather than per lane, because that is the whole point of them: the
    ''' operator is asking "is this marker before or after that payment", and the answer is only
    ''' free to read when the line runs past every lane the marker could be dropped on.</para>
    '''
    ''' <para><b>SOLID, and <see cref="KBotChartGuide.DashStyle"/> is deliberately ignored here.</b>
    ''' A dotted line is not one line to GDI+, it is one segment per dot — a surface holding a
    ''' thousand payments down a full-screen window was rasterizing something like half a million
    ''' segments per frame, and the journal had guides at 198 ms of every 205 ms paint while the
    ''' rails and the markers together cost under 2 ms. Dimmed instead of dotted reads the same at
    ''' one pixel and costs a fraction. The property stays on the guide only because it is public
    ''' and serialized; <c>KBotChartView</c> stopped painting from it too.</para>
    '''
    ''' <para><b>Three things keep this cheap</b>, and all three matter: the columns are folded
    ''' (see <see cref="GuideColumn"/>), only the columns inside <paramref name="clip"/> are
    ''' walked and only the invalidated stretch of each is drawn, and the pen is rewritten once per
    ''' RUN of one colour rather than once per line — every write to a <c>Pen</c> throws away what
    ''' GDI+ built from the last one.</para>
    ''' </remarks>
    Private Sub DrawGuides(g As Graphics, clip As Rectangle)
        If _guideColumns.Count = 0 Then Return
        Dim surface As Rectangle = SurfaceClip()
        If surface.Height <= 0 Then Return

        ' The strip that was actually asked for. A hover moving between two lanes invalidates two
        ' bands out of a 1300-pixel surface, and drawing the full height of every line to have GDI+
        ' throw all but 69 rows of it away is the whole cost paid for nothing.
        Dim top As Integer = Math.Max(surface.Top, clip.Top)
        Dim bottom As Integer = Math.Min(surface.Bottom, clip.Bottom)
        If bottom <= top Then Return
        ' Opened by a pixel on each side: a line standing exactly on the edge of the strip still
        ' has to be repainted, or the band leaves a gap in it.
        Dim left As Integer = clip.Left - 1
        Dim right As Integer = clip.Right + 1

        Dim fallback As Color = Palette().TextDimColor
        Dim width As Single = CSng(_px1)
        Dim pen As Pen = Nothing
        Dim penArgb As Integer = Integer.MinValue

        For i As Integer = 0 To _guideColumns.Count - 1
            Dim col As GuideColumn = _guideColumns(i)
            If col.X < left Then Continue For
            ' Sorted left to right, so the first column past the strip ends the walk.
            If col.X > right Then Exit For
            If col.Argb <> penArgb Then
                pen = ScratchPen(_detailPen,
                                 DimmedGuideColor(If(col.Argb = 0, fallback, Color.FromArgb(col.Argb))),
                                 width)
                penArgb = col.Argb
            End If
            g.DrawLine(pen, col.X, top, col.X, bottom)
        Next

        DrawHoveredGuide(g, top, bottom, fallback)
    End Sub

    ''' <summary>
    ''' How far a dated line is taken back toward the plot background. Half.
    ''' </summary>
    ''' <remarks>
    ''' A dotted line covers about half the pixels of the column it stands in, so half is what the
    ''' operator was already reading — one dated line looked like a quiet mark, and a surface
    ''' carrying a thousand of them looked like a wash with the background showing through. A solid
    ''' line at full strength is neither: it turns a busy surface into one flat block, because at a
    ''' thousand payments over a few hundred pixels there IS a line in every column. Dimming brings
    ''' back the exact reading the dots gave, without the dots.
    ''' </remarks>
    Private Const GuideDimming As Double = 0.5

    ''' <summary>
    ''' A guide colour, mixed toward the plot background — see <see cref="GuideDimming"/>.
    ''' </summary>
    ''' <remarks>
    ''' Mixed here rather than drawn with an alpha, and the two are the same picture: guides go
    ''' down BEFORE the rails and the hover wash, so a dated line never has anything under it but
    ''' the plot background. An opaque pen over one known colour is the fast path, and blending a
    ''' thousand lines per frame is exactly the cost this pass exists to get rid of.
    ''' </remarks>
    Private Function DimmedGuideColor(c As Color) As Color
        Dim back As Color = EffectivePlotBackColor()
        Return Color.FromArgb(Mix(c.R, back.R), Mix(c.G, back.G), Mix(c.B, back.B))
    End Function

    Private Shared Function Mix(from As Byte, toward As Byte) As Integer
        Dim v As Integer = CInt(Math.Round(from + (CInt(toward) - CInt(from)) * GuideDimming))
        Return Math.Max(0, Math.Min(255, v))
    End Function

    ''' <summary>
    ''' The line under the pointer, drawn over the top of the others and thicker.
    ''' </summary>
    ''' <remarks>
    ''' Hover used to be the one guide drawn solid among dotted ones. Now that they are all solid
    ''' the difference is carried by weight instead: this one line is drawn at its FULL colour
    ''' rather than dimmed (see <see cref="GuideDimming"/>) and two pixels wide rather than one.
    ''' Its own colour, though, never another — a payment line that changed hue under the pointer
    ''' would read as a different KIND of line, which is the one thing it must not do.
    ''' </remarks>
    Private Sub DrawHoveredGuide(g As Graphics, top As Integer, bottom As Integer, fallback As Color)
        If _hoverGuideIndex < 0 OrElse _hoverGuideIndex >= VisibleGuides.Count Then Return
        Dim gd As KBotChartGuide = VisibleGuides(_hoverGuideIndex)
        If gd.PlotX < 0 Then Return
        g.DrawLine(ScratchPen(_detailPen, If(gd.LineColor = Color.Empty, fallback, gd.LineColor),
                              CSng(_px2)),
                   gd.PlotX, top, gd.PlotX, bottom)
    End Sub

    ''' <summary>The rails, the separators, the captions and the end marks.</summary>
    Private Sub DrawLanes(g As Graphics)
        Dim surface As Rectangle = SurfaceClip()
        Dim capFont As Font = EffectiveAxisFont()
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)
        Dim sepW As Integer = ThemeShapes.ScaleDpi(Me, Math.Max(1, _separatorWidth))
        Dim railW As Integer = Math.Max(1, ThemeShapes.ScaleDpi(Me, _laneLineWidth))

        For i As Integer = 0 To VisibleLanes.Count - 1
            Dim ln As KBotLane = VisibleLanes(i)
            If Not ln.Visible OrElse ln.Bounds.Height <= 0 Then Continue For
            If ln.Bounds.Bottom < surface.Top OrElse ln.Bounds.Top > surface.Bottom Then Continue For

            If ln.SeparatorAbove AndAlso _separatorWidth > 0 Then
                Dim y As Integer = ln.Bounds.Top - ThemeShapes.ScaleDpi(Me, _laneSpacing) - sepW \ 2
                g.DrawLine(ScratchPen(_detailPen, SeparatorPen.Color, CSng(sepW)),
                           ln.Bounds.Left, y, ln.Bounds.Right, y)
            End If

            ' The lane under the pointer gets a wash, so a drag has something to aim at even where
            ' the lane happens to hold no marker at all.
            If i = _hoverLaneIndex Then
                g.FillRectangle(ScratchBrush(_fillBrush, EffectiveLaneHoverBackColor()), ln.Bounds)
            End If

            If _laneLineWidth > 0 Then
                Dim mid As Integer = ln.Bounds.Top + ln.Bounds.Height \ 2
                ' The plain rail, always, full width and underneath: a lane holding no marker has
                ' to stay visible as somewhere to drop, and the run before the first marker has to
                ' read as empty rather than as absent.
                g.DrawLine(ScratchPen(_detailPen, LaneLinePen.Color, CSng(railW)),
                           _plotRect.Left, mid, _plotRect.Right, mid)
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
        ' Already plotted and already in X order — the layout pass did both. See
        ' KBotLane.PlottedInOrder.
        Dim drawn As List(Of KBotLaneMarker) = ln.PlottedInOrder
        If drawn.Count = 0 Then Return
        Dim laneColor As Color = EffectiveLaneColor(ln, laneIndex)

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
            g.DrawLine(ScratchPen(_edgePen, If(m.MarkerColor = Color.Empty, laneColor, m.MarkerColor), w),
                       x1, mid, x2, mid)
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
        ' A tick is two slanted strokes, so smoothing goes on for the one glyph. See OnPaint.
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            DrawEndMarkGlyph(g, ln)
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    Private Sub DrawEndMarkGlyph(g As Graphics, ln As KBotLane)
        Dim side As Integer = ThemeShapes.ScaleDpi(Me, _endMarkSize)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)
        Dim r As New Rectangle(_plotRect.Right + gap,
                               ln.Bounds.Top + (ln.Bounds.Height - side) \ 2, side, side)
        Dim c As Color = If(ln.EndMark = KBotLaneEndMark.Ok, Palette().SuccessColor, Palette().WarningColor)
        Dim pen As Pen = ScratchPen(_detailPen, c, CSng(_px2), LineCap.Round)
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
    End Sub

    Private Sub DrawMarkers(g As Graphics)
        ' The one pass that is all circles and diamonds, so smoothing goes on ONCE for the whole
        ' pass rather than per marker — setting it costs a state change each time. See OnPaint.
        Dim oldMode As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            DrawMarkerPass(g)
        Finally
            g.SmoothingMode = oldMode
        End Try
    End Sub

    Private Sub DrawMarkerPass(g As Graphics)
        Dim surface As Rectangle = SurfaceClip()
        Dim side As Integer = ThemeShapes.ScaleDpi(Me, _markerSize)
        Dim labelFont As Font = EffectiveAxisFont()
        ' Read ONCE, not once per marker: it resolves the scheme and the palette every time, and
        ' every marker asked for it two or three times.
        Dim plotBack As Color = EffectivePlotBackColor()

        For i As Integer = 0 To VisibleLanes.Count - 1
            Dim ln As KBotLane = VisibleLanes(i)
            If Not ln.Visible OrElse ln.Bounds.Height <= 0 Then Continue For
            If ln.Bounds.Bottom < surface.Top OrElse ln.Bounds.Top > surface.Bottom Then Continue For
            Dim laneColor As Color = EffectiveLaneColor(ln, i)

            For j As Integer = 0 To ln.Markers.Count - 1
                Dim m As KBotLaneMarker = ln.Markers(j)
                If Not m.Plotted Then Continue For
                Dim c As Color = If(m.MarkerColor = Color.Empty, laneColor, m.MarkerColor)
                Dim hovered As Boolean = (i = _hoverLaneIndex AndAlso j = _hoverMarkerIndex)
                DrawMarker(g, m, m.PlotLocation, side, c, hovered, plotBack)

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
                           c As Color, hovered As Boolean, plotBack As Color)
        Dim r As New Rectangle(center.X - side \ 2, center.Y - side \ 2, side, side)
        Dim thin As Single = CSng(_px1)

        ' Scratch objects rather than `Using New …`: see the field declarations. `fill` and
        ' `edge` are set up first because three of the four branches use them.
        Dim fill As SolidBrush = ScratchBrush(_fillBrush, c)
        Dim edge As Pen = ScratchPen(_edgePen, plotBack, thin)

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
                g.FillEllipse(ScratchBrush(_backBrush, plotBack), r)
                Dim pen As Pen = ScratchPen(_detailPen, c, thin)
                g.DrawEllipse(pen, r)
                Dim x1 As Integer = r.Left + r.Width \ 4
                Dim x2 As Integer = r.Right - r.Width \ 4
                Dim dy As Integer = Math.Max(1, r.Height \ 6)
                g.DrawLine(pen, x1, center.Y - dy, x2, center.Y - dy)
                g.DrawLine(pen, x1, center.Y + dy, x2, center.Y + dy)

            Case KBotLaneMarkerStyle.Deletion
                ' A cross cap: the end of a chain has to read as an end, not as one more entry.
                Dim pen As Pen = ScratchPen(_detailPen, c, CSng(_px2), LineCap.Round)
                g.DrawLine(pen, r.Left, r.Top, r.Right, r.Bottom)
                g.DrawLine(pen, r.Right, r.Top, r.Left, r.Bottom)

            Case KBotLaneMarkerStyle.Locked
                g.FillEllipse(fill, r)
                g.DrawEllipse(edge, r)
                DrawPadlock(g, r, plotBack)

            Case Else
                g.FillEllipse(fill, r)
                g.DrawEllipse(edge, r)
        End Select

        If hovered Then
            Dim grow As Integer = _px3
            Dim ring As New Rectangle(r.X - grow, r.Y - grow, r.Width + grow * 2, r.Height + grow * 2)
            g.DrawEllipse(ScratchPen(_detailPen, c, CSng(_px2)), ring)
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
        g.FillRectangle(ScratchBrush(_backBrush, c), body)
        Dim shackle As New Rectangle(body.Left + body.Width \ 4, body.Top - body.Height \ 2,
                                     Math.Max(1, body.Width \ 2), Math.Max(1, body.Height))
        g.DrawArc(ScratchPen(_detailPen, c, CSng(_px1)), shackle, 180, 180)
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
        Dim mark As Long = KBotLaneLog.Mark()
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
            If li >= 0 Then ArmDrag(VisibleLanes(li).Markers(mi), e.Location, e.Button)
            KBotLaneLog.Done("DOWN", mark, $"pt={e.X},{e.Y} lane={li} marker={mi}")
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Dim mark As Long = KBotLaneLog.Mark()
        Try
            CancelDragArming()
            KBotLaneLog.Done("UP", mark, $"pt={e.X},{e.Y}")
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim mark As Long = KBotLaneLog.Mark()
        Try
            ' The drag takes over the whole modal loop, so the rest of this handler has nothing
            ' left to do when it returns — the pointer is somewhere else entirely by then.
            If MaybeBeginDrag(e.Location, e.Button) Then
                ' NOT counted as a move: the elapsed time would be the whole drag, and one line
                ' of eleven seconds in a column of tenths of a millisecond would wreck the rollup.
                Return
            End If

            EnsureLayout()

            ' For the journal only: did this move actually change anything on the surface? A run
            ' of "changed=no" moves that still cost time is the signature of a hit-test doing too
            ' much work, and it looks nothing like a run of moves that each repaint two bands.
            Dim changed As Boolean = False

            Dim overEnlarge As Boolean = _enlargeRect.Width > 0 AndAlso _enlargeRect.Contains(e.Location)
            If overEnlarge <> _hoverEnlarge Then
                _hoverEnlarge = overEnlarge
                InvalidateEnlargeButton()
                changed = True
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
                Dim wasLane As Integer = _hoverLaneIndex
                Dim wasGuide As Integer = _hoverGuideIndex
                _hoverLaneIndex = laneHover
                _hoverMarkerIndex = mi
                _hoverGuideIndex = gi
                ' Only what CHANGED, never the whole surface. A hover moves a wash from one band to
                ' another and a ring from one marker to another, and both of those live inside the
                ' two bands — so two strips are repainted instead of the entire window. On the
                ' enlarged window the whole surface costs about sixteen milliseconds, and the mouse
                ' sends moves far faster than that: repainting all of it per move is what made the
                ' big benzi feel stuck while the narrow ones felt fine (slice 0058).
                InvalidateLaneBand(wasLane)
                InvalidateLaneBand(laneHover)
                InvalidateGuideColumn(wasGuide)
                InvalidateGuideColumn(gi)
                If markerChanged Then
                    RaiseEvent MarkerHovered(If(mi >= 0 AndAlso laneHover >= 0, VisibleLanes(laneHover).Key, Nothing), mi)
                End If
                changed = True
            End If

            RefreshLaneTip()
            KBotLaneLog.Done("MOVE", mark,
                             $"pt={e.X},{e.Y} lane={laneHover} marker={mi} guide={gi} " &
                             $"btn={If(_hoverEnlarge, "yes", "no")} changed={If(changed, "yes", "no")}")
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Dim mark As Long = KBotLaneLog.Mark()
        Try
            Dim hadMarker As Boolean = _hoverMarkerIndex <> -1
            Dim wasLane As Integer = _hoverLaneIndex
            Dim wasGuide As Integer = _hoverGuideIndex
            Dim wasEnlarge As Boolean = _hoverEnlarge
            _hoverLaneIndex = -1
            _hoverMarkerIndex = -1
            _hoverGuideIndex = -1
            _hoverEnlarge = False
            HideLaneTip()
            ' Same as the move: only the strips that were lit. See OnMouseMove.
            InvalidateLaneBand(wasLane)
            InvalidateGuideColumn(wasGuide)
            If wasEnlarge Then InvalidateEnlargeButton()
            If hadMarker Then RaiseEvent MarkerHovered(Nothing, -1)
            KBotLaneLog.Done("LEAVE", mark, $"was lane={wasLane} guide={wasGuide}")
            ' The pointer leaving the surface is exactly the moment somebody goes to READ the
            ' journal, so the batch is written out here rather than waiting for the next event
            ' or for the form to close. Off the measured path: nothing is being drawn now.
            KBotLaneLog.Flush()
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Dim mark As Long = KBotLaneLog.Mark()
        Try
            If Not vScroll.Visible Then Return
            Dim lines As Integer = SystemInformation.MouseWheelScrollLines
            If lines <= 0 Then lines = 3
            Dim delta As Integer = -(e.Delta \ 120) * lines * vScroll.SmallChange
            Dim top As Integer = Math.Max(0, vScroll.Maximum - vScroll.LargeChange + 1)
            vScroll.Value = Math.Max(0, Math.Min(top, vScroll.Value + delta))
            InvalidateLaneLayout()
            KBotLaneLog.Done("WHEEL", mark, $"delta={e.Delta} value={vScroll.Value}")
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotLaneView.OnMouseWheel", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Repaints ONE lane's strip — the band plus the room a marker sticks out into.
    ''' </summary>
    ''' <remarks>
    ''' Full width on purpose, and not the marker's own little square: the caption sits left of the
    ''' plot, the end mark sits right of it, and the hover wash runs the length of the band, so a
    ''' strip is the smallest shape that holds everything one lane can change. It is also cheap —
    ''' a band is a couple of dozen rows of pixels against the eleven hundred the surface has.
    ''' </remarks>
    Private Sub InvalidateLaneBand(index As Integer)
        If index < 0 OrElse index >= VisibleLanes.Count Then Return
        InvalidateLaneBand(VisibleLanes(index))
    End Sub

    ''' <summary>The same strip, for the callers that hold the lane rather than its index.</summary>
    Friend Sub InvalidateLaneBand(ln As KBotLane)
        If ln Is Nothing Then Return
        Dim b As Rectangle = ln.Bounds
        If b.Height <= 0 Then Return
        ' A marker is centred on the rail and its hover ring grows past it, so the strip is opened
        ' by half a marker plus the ring — otherwise the ring would leave a crumb behind it.
        Dim pad As Integer = ThemeShapes.ScaleDpi(Me, _markerSize) \ 2 + _px3 + _px2
        _invalidateCount += 1
        KBotLaneLog.Note("inval-band", $"lane={If(ln.Key, "?")} y={b.Top} h={b.Height + pad * 2}")
        Invalidate(New Rectangle(0, b.Top - pad, Width, b.Height + pad * 2))
    End Sub

    ''' <summary>Repaints the column one dated line stands in, top to bottom of the surface.</summary>
    Private Sub InvalidateGuideColumn(index As Integer)
        If index < 0 OrElse index >= VisibleGuides.Count Then Return
        Dim gd As KBotChartGuide = VisibleGuides(index)
        If gd.PlotX < 0 Then Return
        Dim surface As Rectangle = SurfaceClip()
        If surface.Height <= 0 Then Return
        Dim half As Integer = _px2 + _px1
        _invalidateCount += 1
        KBotLaneLog.Note("inval-guide", $"guide={index} x={gd.PlotX}")
        Invalidate(New Rectangle(gd.PlotX - half, surface.Top, half * 2 + 1, surface.Height))
    End Sub

    ''' <summary>Repaints the enlarge button and the wash that appears behind it.</summary>
    Private Sub InvalidateEnlargeButton()
        If _enlargeRect.Width <= 0 OrElse _enlargeRect.Height <= 0 Then Return
        Dim pad As Integer = ThemeShapes.ScaleDpi(Me, 4)
        _invalidateCount += 1
        KBotLaneLog.Note("inval-btn", $"hover={If(_hoverEnlarge, "yes", "no")}")
        Invalidate(New Rectangle(_enlargeRect.X - pad, _enlargeRect.Y - pad,
                                 _enlargeRect.Width + pad * 2, _enlargeRect.Height + pad * 2))
    End Sub

    ''' <summary>The lane whose band contains <paramref name="location"/>, or -1.</summary>
    Friend Function LaneIndexAt(location As Point) As Integer
        If Not SurfaceClip().Contains(location) Then Return -1
        For i As Integer = 0 To VisibleLanes.Count - 1
            Dim ln As KBotLane = VisibleLanes(i)
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

        For i As Integer = 0 To VisibleLanes.Count - 1
            Dim ln As KBotLane = VisibleLanes(i)
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
        If VisibleGuides.Count = 0 OrElse Not SurfaceClip().Contains(location) Then Return -1
        Dim reach As Integer = ThemeShapes.ScaleDpi(Me, _hoverRadius)
        Dim best As Integer = reach + 1
        Dim found As Integer = -1
        For i As Integer = 0 To VisibleGuides.Count - 1
            Dim gd As KBotChartGuide = VisibleGuides(i)
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

        If _hoverLaneIndex >= 0 AndAlso _hoverLaneIndex < VisibleLanes.Count Then
            Dim ln As KBotLane = VisibleLanes(_hoverLaneIndex)
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

        If _hoverGuideIndex >= 0 AndAlso _hoverGuideIndex < VisibleGuides.Count Then
            Dim gd As KBotChartGuide = VisibleGuides(_hoverGuideIndex)
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
        Dim mark As Long = KBotLaneLog.Mark()
        _currentTipKey = key

        If String.IsNullOrEmpty(key) OrElse
           (String.IsNullOrEmpty(header) AndAlso String.IsNullOrEmpty(body) AndAlso String.IsNullOrEmpty(footer)) Then
            markerTip?.HideNow()
            KBotLaneLog.Done("TIP-HIDE", mark)
            Return
        End If

        _tipContent.HeaderText = If(header, String.Empty)
        _tipContent.Text = If(body, String.Empty)
        _tipContent.FooterText = If(footer, String.Empty)
        MarkerTooltip.ShowAt(Me, _tipContent, Cursor.Position)
        KBotLaneLog.Done("TIP-SHOW", mark, $"key={key}")
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
        Dim mark As Long = KBotLaneLog.Mark()
        Try
            Select Case e.KeyCode
                Case Keys.Space, Keys.Enter
                    If _enlargeButtonVisible Then
                        RaiseEvent EnlargeRequested()
                        e.Handled = True
                    End If
                Case Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End
                    If Not vScroll.Visible Then Return
                    Dim top As Integer = Math.Max(0, vScroll.Maximum - vScroll.LargeChange + 1)
                    Dim v As Integer = vScroll.Value
                    Select Case e.KeyCode
                        Case Keys.Up : v -= vScroll.SmallChange
                        Case Keys.Down : v += vScroll.SmallChange
                        Case Keys.PageUp : v -= vScroll.LargeChange
                        Case Keys.PageDown : v += vScroll.LargeChange
                        Case Keys.Home : v = 0
                        Case Else : v = top
                    End Select
                    vScroll.Value = Math.Max(0, Math.Min(top, v))
                    InvalidateLaneLayout()
                    e.Handled = True
            End Select
            KBotLaneLog.Done("KEY", mark, $"key={e.KeyCode} handled={If(e.Handled, "yes", "no")}")
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
        Return VisibleLanes(index).Bounds
    End Function

    ''' <summary>Friend test hook: where a marker landed (Empty if it was not drawn).</summary>
    Friend Function DebugMarkerLocation(laneIndex As Integer, markerIndex As Integer) As Point
        EnsureLayout()
        Return VisibleLanes(laneIndex).Markers(markerIndex).PlotLocation
    End Function

    ''' <summary>Friend test hook: where a guide landed on the horizontal axis (-1 if not drawn).</summary>
    Friend Function DebugGuideX(index As Integer) As Integer
        EnsureLayout()
        Return VisibleGuides(index).PlotX
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
