Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' LAYOUT, PAINTING, MOUSE AND KEYBOARD of <see cref="KBotChartView"/>.
'''
''' <para>Split off from the property file for the same reason the tree is split: the settings a
''' host reads in the property grid and the arithmetic that turns them into pixels are two
''' different jobs, and keeping them apart is what stops a measure from being scaled in one place
''' and used raw in another.</para>
'''
''' <para><b>One source for the scale.</b> Every logical measure goes through
''' <c>ThemeShapes.ScaleDpi(Me, …)</c>, which resolves to <c>DeviceDpi / 96</c>. Nothing here reads
''' <c>AutoScaleMode.Font</c>'s factor: two sources would make the floor of the control and its
''' drawing disagree, which is exactly the defect slice 0035 went to fix.</para>
'''
''' <para><b>Try/Catch classification.</b> <c>OnPaint</c>, the mouse handlers and the keyboard
''' handlers are UI boundaries: they log and swallow, because a throw out of a paint body kills the
''' process. The helpers below are reached ONLY through those boundaries, so under the house rule
''' they carry no Try of their own — the boundary is the sink.</para>
''' </summary>
Partial Public NotInheritable Class KBotChartView

    ' The horizontal axis is stored as ticks, not as Date, once the layout is computed: the
    ' arithmetic that maps a moment to a pixel is a ratio of spans, and doing it on Date would mean
    ' converting on every single point of every single repaint.
    Private _minTicks As Double
    Private _maxTicks As Double

    ' The value axis after it has been rounded out to whole ticks (see NiceNumber).
    Private _axisMin As Double
    Private _axisMax As Double
    Private _axisStep As Double

    ' =====================================================================
    ' SCRATCH GDI+ OBJECTS
    ' =====================================================================

    ''' <summary>
    ''' One of the scratch objects, set up for the shape about to be drawn.
    ''' </summary>
    ''' <remarks>
    ''' EVERY property is written on every call, never only the ones this caller cares about: a
    ''' round cap or a dashed style left behind by the previous shape would surface as a defect
    ''' in some unrelated method three shapes later. See the field declarations for why these
    ''' exist at all.
    ''' </remarks>
    Private Shared Function ScratchPen(ByRef p As Pen, c As Color, w As Single,
                                       Optional cap As LineCap = LineCap.Flat) As Pen
        If p Is Nothing Then p = New Pen(c, w)
        p.Color = c
        p.Width = w
        p.StartCap = cap
        p.EndCap = cap
        ' Round, always: a run of segments drawn as ONE polyline gets joins where it used to get
        ' caps, and a mitred join on a sharp turn of the data would grow a spike the data has not
        ' got. See DrawOneSeries for why the segments are drawn in runs.
        p.LineJoin = LineJoin.Round
        p.DashStyle = DashStyle.Solid
        Return p
    End Function

    ''' <summary>The brush twin of <see cref="ScratchPen"/>.</summary>
    Private Shared Function ScratchBrush(ByRef b As SolidBrush, c As Color) As SolidBrush
        If b Is Nothing Then b = New SolidBrush(c)
        b.Color = c
        Return b
    End Function

    ' =====================================================================
    ' LAYOUT
    ' =====================================================================

    Private Sub EnsureLayout()
        If Not _layoutValid Then RecalcLayout()
    End Sub

    ''' <summary>
    ''' Cuts the control into its three bands and works out both ranges.
    '''
    ''' <para>The order matters: the value range has to be known BEFORE the plot rectangle, because
    ''' the width of the widest value label is what the plot gives up on its left. Doing it the
    ''' other way round makes the labels of a large scale run off the control.</para>
    ''' </summary>
    Private Sub RecalcLayout()
        _layoutValid = True
        _headerRect = Rectangle.Empty
        _legendRect = Rectangle.Empty
        _plotRect = Rectangle.Empty

        Dim client As Rectangle = ClientRectangle
        If client.Width <= 0 OrElse client.Height <= 0 Then Return

        Dim rest As Rectangle = client
        If _headerVisible AndAlso _headerHeight > 0 Then
            Dim hh As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _headerHeight), rest.Height)
            _headerRect = New Rectangle(rest.Left, rest.Top, rest.Width, hh)
            rest = New Rectangle(rest.Left, rest.Top + hh, rest.Width, rest.Height - hh)
            LayoutTabs()
        End If

        ComputeRanges()

        Dim margin As Integer = ThemeShapes.ScaleDpi(Me, _plotMargin)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)

        If _legendVisible AndAlso _legendHeight > 0 AndAlso HasAnyVisibleSeries() Then
            Dim lh As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _legendHeight), Math.Max(0, rest.Height - margin))
            _legendRect = New Rectangle(rest.Left + margin, rest.Bottom - lh - margin \ 2,
                                        Math.Max(0, rest.Width - margin * 2), lh)
            rest = New Rectangle(rest.Left, rest.Top, rest.Width, Math.Max(0, rest.Height - lh - margin \ 2))
        End If

        Dim leftGutter As Integer = 0
        Dim bottomGutter As Integer = 0
        If _axisVisible Then
            leftGutter = WidestValueLabelWidth() + gap
            bottomGutter = TextRenderer.MeasureText("0", EffectiveAxisFont()).Height + gap
        End If

        Dim plotLeft As Integer = rest.Left + margin + leftGutter
        Dim plotTop As Integer = rest.Top + margin
        Dim plotRight As Integer = rest.Right - margin
        Dim plotBottom As Integer = rest.Bottom - margin - bottomGutter
        _plotRect = New Rectangle(plotLeft, plotTop,
                                  Math.Max(0, plotRight - plotLeft),
                                  Math.Max(0, plotBottom - plotTop))

        ProjectPoints()
        ProjectGuides()
    End Sub

    ''' <summary>
    ''' Works out the time span and the value span of everything that is visible, then rounds the
    ''' value span out to whole ticks.
    ''' </summary>
    ''' <remarks>
    ''' A single point, or several points sharing one moment, gives a span of zero. That is not an
    ''' error and it is not padded away: the point is simply drawn in the middle of the plot, which
    ''' is the only honest place for a measurement that has nothing to be earlier or later than.
    ''' </remarks>
    Private Sub ComputeRanges()
        Dim any As Boolean = False
        Dim minTicks As Double = 0
        Dim maxTicks As Double = 0
        Dim minVal As Double = 0
        Dim maxVal As Double = 0

        For Each s As KBotChartSeries In _series
            If Not s.Visible Then Continue For
            For Each p As KBotChartPoint In s.Points
                Dim t As Double = CDbl(p.Moment.Ticks)
                If Not any Then
                    minTicks = t : maxTicks = t
                    minVal = p.Value : maxVal = p.Value
                    any = True
                Else
                    If t < minTicks Then minTicks = t
                    If t > maxTicks Then maxTicks = t
                    If p.Value < minVal Then minVal = p.Value
                    If p.Value > maxVal Then maxVal = p.Value
                End If
            Next
        Next

        If Not any Then
            _minTicks = 0 : _maxTicks = 0
            _minValue = 0 : _maxValue = 0
            _axisMin = 0 : _axisMax = 0 : _axisStep = 0
            _minMoment = Date.MinValue : _maxMoment = Date.MinValue
            Return
        End If

        _minTicks = minTicks
        _maxTicks = maxTicks
        _minMoment = New Date(CLng(minTicks))
        _maxMoment = New Date(CLng(maxTicks))
        _minValue = minVal
        _maxValue = maxVal

        Dim lo As Double = If(_valueAxisMode = KBotChartValueAxisMode.FromZero, Math.Min(0.0, minVal), minVal)
        Dim hi As Double = Math.Max(maxVal, lo)
        If hi <= lo Then
            ' A flat line still needs a scale to sit on. One unit either side keeps the line in the
            ' middle instead of collapsing it onto the baseline.
            lo -= 1 : hi += 1
        End If

        Dim ticks As Integer = Math.Max(1, _valueTickCount)
        Dim tickStep As Double = NiceNumber((hi - lo) / ticks, True)
        If tickStep <= 0 Then tickStep = 1
        _axisMin = Math.Floor(lo / tickStep) * tickStep
        _axisMax = Math.Ceiling(hi / tickStep) * tickStep
        If _axisMax <= _axisMin Then _axisMax = _axisMin + tickStep
        _axisStep = tickStep
    End Sub

    ''' <summary>
    ''' The "nice" number at or near <paramref name="range"/>: 1, 2, 5 or 10 times a power of ten.
    ''' It is what turns an axis of 0 / 3.271 / 6.542 into one of 0 / 2.500 / 5.000 — the second is
    ''' the one an operator can read a value off without arithmetic.
    ''' </summary>
    Private Shared Function NiceNumber(range As Double, roundIt As Boolean) As Double
        If range <= 0 OrElse Double.IsNaN(range) OrElse Double.IsInfinity(range) Then Return 1
        Dim exponent As Double = Math.Floor(Math.Log10(range))
        Dim fraction As Double = range / Math.Pow(10, exponent)
        Dim nice As Double
        If roundIt Then
            If fraction < 1.5 Then
                nice = 1
            ElseIf fraction < 3 Then
                nice = 2
            ElseIf fraction < 7 Then
                nice = 5
            Else
                nice = 10
            End If
        Else
            If fraction <= 1 Then
                nice = 1
            ElseIf fraction <= 2 Then
                nice = 2
            ElseIf fraction <= 5 Then
                nice = 5
            Else
                nice = 10
            End If
        End If
        Return nice * Math.Pow(10, exponent)
    End Function

    Private Function HasAnyVisibleSeries() As Boolean
        For Each s As KBotChartSeries In _series
            If s.Visible AndAlso s.Points.Count > 0 Then Return True
        Next
        Return False
    End Function

    Private Function WidestValueLabelWidth() As Integer
        If _valueTickCount <= 0 OrElse _axisStep <= 0 Then Return 0
        Dim widest As Integer = 0
        Dim f As Font = EffectiveAxisFont()
        Dim v As Double = _axisMin
        While v <= _axisMax + _axisStep / 2.0
            Dim w As Integer = TextRenderer.MeasureText(v.ToString(_valueFormat), f).Width
            If w > widest Then widest = w
            v += _axisStep
        End While
        Return widest
    End Function

    ''' <summary>Turns every point into a client-pixel location, once per layout pass.</summary>
    Private Sub ProjectPoints()
        Dim plotted As Boolean = _plotRect.Width > 0 AndAlso _plotRect.Height > 0 AndAlso _axisMax > _axisMin
        For Each s As KBotChartSeries In _series
            For Each p As KBotChartPoint In s.Points
                If Not plotted OrElse Not s.Visible Then
                    p.Plotted = False
                    p.PlotLocation = Point.Empty
                    Continue For
                End If
                p.PlotLocation = New Point(MomentToX(p.Moment), ValueToY(p.Value))
                p.Plotted = True
            Next
        Next
    End Sub

    ''' <summary>
    ''' One dated line as the painter sees it: a pixel column and a colour, nothing else.
    ''' </summary>
    ''' <remarks>
    ''' <para>A guide and a drawn line are NOT the same count. Guides land on the axis by date, and
    ''' the axis is only ever as wide as the plot: a thousand payments spread over a year fall on
    ''' a few hundred distinct pixel columns, and everything past the first line in a column is
    ''' drawing over a line that is already there.</para>
    ''' <para>The colour is part of the identity, not just the position - two guides on the same
    ''' column in two colours are two different lines, and only the second one would be visible.
    ''' <c>0</c> means "the dimmed text colour of the active scheme", so a theme change does not
    ''' make this list stale.</para>
    ''' </remarks>
    Private Structure GuideColumn
        Public X As Integer
        Public Argb As Integer
    End Structure

    ''' <summary>
    ''' Puts each guide on the horizontal axis, or takes it off the plot entirely, and builds the
    ''' folded column list the painter walks.
    ''' </summary>
    ''' <remarks>
    ''' A guide OUTSIDE the time span of the points gets <c>PlotX = -1</c> and is not drawn. It
    ''' deliberately does not stretch the axis to reach itself: the axis belongs to the
    ''' measurements, and a payment made months after the last snapshot would otherwise squash
    ''' the whole chain against one edge to make room for a line that says nothing about it.
    ''' </remarks>
    Private Sub ProjectGuides()
        Dim plotted As Boolean = _plotRect.Width > 0 AndAlso _plotRect.Height > 0 AndAlso _axisMax > _axisMin
        _guideColumns.Clear()
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
    ''' edge, instead of testing every guide each time a corner of the plot repaints.
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

    Private Function MomentToX(moment As Date) As Integer
        Dim span As Double = _maxTicks - _minTicks
        ' Everything at the same instant: one column in the middle, not a line pinned to the left.
        If span <= 0 Then Return _plotRect.Left + _plotRect.Width \ 2
        Dim ratio As Double = (CDbl(moment.Ticks) - _minTicks) / span
        Return _plotRect.Left + CInt(Math.Round(ratio * _plotRect.Width))
    End Function

    Private Function ValueToY(value As Double) As Integer
        Dim span As Double = _axisMax - _axisMin
        If span <= 0 Then Return _plotRect.Top + _plotRect.Height \ 2
        Dim ratio As Double = (value - _axisMin) / span
        Return _plotRect.Bottom - CInt(Math.Round(ratio * _plotRect.Height))
    End Function

    ''' <summary>
    ''' Lays the buttons of the band out from the end named by <see cref="TabAlign"/> inwards.
    ''' Hidden buttons get <see cref="Rectangle.Empty"/> and take no slot.
    ''' </summary>
    Private Sub LayoutTabs()
        For Each t As KBotChartTab In _tabs
            t.Bounds = Rectangle.Empty
        Next
        If _headerRect.Width <= 0 OrElse _headerRect.Height <= 0 Then Return

        Dim h As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _tabHeight), _headerRect.Height)
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _tabSpacing)
        Dim edge As Integer = ThemeShapes.ScaleDpi(Me, _plotMargin)
        Dim y As Integer = _headerRect.Top + (_headerRect.Height - h) \ 2

        If _tabAlign = KBotChartTabAlign.Right Then
            Dim x As Integer = _headerRect.Right - edge
            For i As Integer = _tabs.Count - 1 To 0 Step -1
                Dim t As KBotChartTab = _tabs(i)
                If Not t.Visible Then Continue For
                Dim w As Integer = TabWidth(t)
                x -= w
                t.Bounds = New Rectangle(x, y, w, h)
                x -= gap
            Next
        Else
            Dim x As Integer = _headerRect.Left + edge
            For i As Integer = 0 To _tabs.Count - 1
                Dim t As KBotChartTab = _tabs(i)
                If Not t.Visible Then Continue For
                Dim w As Integer = TabWidth(t)
                t.Bounds = New Rectangle(x, y, w, h)
                x += w + gap
            Next
        End If
    End Sub

    ' The width a button needs: air + optional icon + caption. ONE function, used both by the
    ' layout and by the painting, so the two cannot drift apart.
    Private Function TabWidth(t As KBotChartTab) As Integer
        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, _tabPadding)
        Dim w As Integer = TextRenderer.MeasureText(If(t.Text, String.Empty), EffectiveHeaderFont()).Width + padX * 2
        If t.Icon IsNot Nothing AndAlso _tabIconSize.Width > 0 Then
            w += ThemeShapes.ScaleDpi(Me, _tabIconSize.Width) + ThemeShapes.ScaleDpi(Me, 4)
        End If
        Return w
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
            ' SMOOTHING OFF BY DEFAULT, and switched on only around the round and the slanted -
            ' the band, the buttons, the series, the markers, the hover ring, the frame.
            ' Everything else here is axis-aligned: the plot fill, the grid, the axes, the dated
            ' lines, the legend swatches. Antialiasing those buys no pixel anyone can see and is
            ' paid for by the SURFACE, so it is the one cost that grows with the window rather
            ' than with the data - which is why a full-screen chart crawled while a narrow one
            ' felt fine.
            g.SmoothingMode = SmoothingMode.None
            g.Clear(BackColor)

            DrawHeaderBand(g, designTime)
            DrawPlotBackground(g)

            If HasAnyVisibleSeries() AndAlso _axisMax > _axisMin Then
                DrawGridAndAxes(g)
                ' BEHIND the series, deliberately: a guide is context to read the lines against,
                ' so it must never hide one.
                DrawGuides(g, e.ClipRectangle)
                DrawAllSeries(g, e.ClipRectangle)
                DrawHoverHighlight(g)
                DrawLegend(g)
            Else
                DrawEmptyState(g)
            End If

            DrawOuterBorder(g)
        Catch ex As Exception
            ' UI boundary: a throw out of a paint body kills the process, so it logs and returns.
            ' Nothing is logged from inside the designer process (see KBotDesignTime).
            If Not designTime Then GlobalErrorLog.Write("KBotChartView.OnPaint", ex)
        End Try
    End Sub

    Private Sub DrawOuterBorder(g As Graphics)
        If Not _borderVisible OrElse _borderWidth <= 0 Then Return
        Dim radius As Integer = EffectiveCornerRadius()
        Dim r As New Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1))
        ' Rounded corners, so this one does want smoothing - see the note in OnPaint.
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                g.DrawPath(BorderPen, path)
            End Using
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    Private Function EffectiveCornerRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius,
                                    If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    Private Function EffectiveTabRadius() As Integer
        Dim logical As Integer = If(_tabCornerRadius >= 0, _tabCornerRadius,
                                    If(_scheme IsNot Nothing, _scheme.Style.CornerRadius, 0))
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ''' <summary>
    ''' The band, its separator, its caption and its buttons.
    ''' </summary>
    ''' <remarks>
    ''' Smoothing is switched on for the whole band and back off after it - see the note in
    ''' <c>OnPaint</c>. It is a strip a few dozen pixels tall carrying rounded buttons, so the
    ''' cost is bounded by the band rather than by the window, and the corners need it.
    ''' </remarks>
    Private Sub DrawHeaderBand(g As Graphics, designTime As Boolean)
        If _headerRect.Width <= 0 OrElse _headerRect.Height <= 0 Then Return
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using path As GraphicsPath = ThemeShapes.RoundedRect(_headerRect, 0)
                ThemeShapes.FillModern(g, path, _headerRect, EffectiveHeaderBackColor(), _headerGradient)
            End Using

            If _headerSeparatorWidth > 0 Then
                Using pen As New Pen(EffectiveHeaderSeparatorColor(), ThemeShapes.ScaleDpi(Me, _headerSeparatorWidth))
                    g.DrawLine(pen, _headerRect.Left, _headerRect.Bottom - 1, _headerRect.Right, _headerRect.Bottom - 1)
                End Using
            End If

            DrawHeaderCaption(g)
            DrawTabs(g, designTime)
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    ' The title takes the end the buttons did not take, and stops where the first button starts.
    Private Sub DrawHeaderCaption(g As Graphics)
        If String.IsNullOrEmpty(_headerCaption) Then Return
        Dim edge As Integer = ThemeShapes.ScaleDpi(Me, _plotMargin)
        Dim captionRect As Rectangle
        Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis

        If _tabAlign = KBotChartTabAlign.Right Then
            Dim limit As Integer = _headerRect.Right - edge
            For Each t As KBotChartTab In _tabs
                If t.Bounds.Width > 0 AndAlso t.Bounds.Left < limit Then limit = t.Bounds.Left
            Next
            captionRect = New Rectangle(_headerRect.Left + edge, _headerRect.Top,
                                        Math.Max(0, limit - edge - (_headerRect.Left + edge)), _headerRect.Height)
            flags = flags Or TextFormatFlags.Left
        Else
            Dim start As Integer = _headerRect.Left + edge
            For Each t As KBotChartTab In _tabs
                If t.Bounds.Width > 0 AndAlso t.Bounds.Right > start Then start = t.Bounds.Right
            Next
            start += edge
            captionRect = New Rectangle(start, _headerRect.Top,
                                        Math.Max(0, _headerRect.Right - edge - start), _headerRect.Height)
            flags = flags Or TextFormatFlags.Right
        End If

        If captionRect.Width <= 0 Then Return
        TextRenderer.DrawText(g, _headerCaption, EffectiveHeaderFont(), captionRect,
                              EffectiveHeaderTextColor(), flags)
    End Sub

    Private Sub DrawTabs(g As Graphics, designTime As Boolean)
        Dim p As ThemePalette = Palette()
        Dim radius As Integer = EffectiveTabRadius()
        Dim padX As Integer = ThemeShapes.ScaleDpi(Me, _tabPadding)
        Dim badKeys As HashSet(Of String) = If(designTime, DuplicateTabKeys(), Nothing)

        For i As Integer = 0 To _tabs.Count - 1
            Dim t As KBotChartTab = _tabs(i)
            Dim r As Rectangle = t.Bounds
            If r.Width <= 0 OrElse r.Height <= 0 Then Continue For

            Dim isCurrent As Boolean = String.Equals(t.Key, _selectedTabKey, StringComparison.Ordinal)
            Dim isHover As Boolean = (i = _hoverTabIndex) AndAlso t.Enabled
            Dim fill As Color
            Dim fore As Color
            If isCurrent Then
                fill = p.AccentColor
                fore = p.AccentTextColor
            ElseIf isHover Then
                fill = p.ButtonHoverColor
                fore = p.ButtonTextColor
            Else
                fill = p.ButtonBackColor
                fore = p.ButtonTextColor
            End If
            If Not t.Enabled Then fore = p.DisabledTextColor

            Using path As GraphicsPath = ThemeShapes.RoundedRect(r, radius)
                ThemeShapes.FillModern(g, path, r, fill, _tabGradient)
                ' The current button has no separate outline: its outline IS its fill, otherwise a
                ' grey frame would cut all the way around the accent.
                Using pen As New Pen(If(isCurrent, fill, p.ButtonBorderColor))
                    g.DrawPath(pen, path)
                End Using
            End Using

            Dim textLeft As Integer = r.Left + padX
            If t.Icon IsNot Nothing AndAlso _tabIconSize.Width > 0 AndAlso _tabIconSize.Height > 0 Then
                Dim iw As Integer = ThemeShapes.ScaleDpi(Me, _tabIconSize.Width)
                Dim ih As Integer = ThemeShapes.ScaleDpi(Me, _tabIconSize.Height)
                g.DrawImage(t.Icon, New Rectangle(textLeft, r.Top + (r.Height - ih) \ 2, iw, ih))
                textLeft += iw + ThemeShapes.ScaleDpi(Me, 4)
            End If

            Dim tr As New Rectangle(textLeft, r.Top, Math.Max(0, r.Right - padX - textLeft), r.Height)
            TextRenderer.DrawText(g, If(t.Text, String.Empty), EffectiveHeaderFont(), tr, fore,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)

            ' The keyboard focus ring: the band is a single Tab stop, so it has to show WHICH
            ' button Space would act on.
            If Focused AndAlso i = _focusTabIndex Then
                Using pen As New Pen(p.FocusRingColor)
                    pen.DashStyle = DashStyle.Dot
                    Using fpath As GraphicsPath = ThemeShapes.RoundedRect(
                            New Rectangle(r.Left + 2, r.Top + 2, Math.Max(1, r.Width - 5), Math.Max(1, r.Height - 5)),
                            Math.Max(0, radius - 1))
                        g.DrawPath(pen, fpath)
                    End Using
                End Using
            End If

            ' Designer error mark: empty or duplicate key.
            If designTime AndAlso (String.IsNullOrWhiteSpace(t.Key) OrElse badKeys.Contains(t.Key)) Then
                Using pen As New Pen(Color.Red, 2)
                    g.DrawRectangle(pen, r.Left + 1, r.Top + 1, Math.Max(1, r.Width - 3), Math.Max(1, r.Height - 3))
                End Using
            End If
        Next
    End Sub

    ' The keys that appear more than once. Design time only.
    Private Function DuplicateTabKeys() As HashSet(Of String)
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        Dim dup As New HashSet(Of String)(StringComparer.Ordinal)
        For Each t As KBotChartTab In _tabs
            If String.IsNullOrWhiteSpace(t.Key) Then Continue For
            If Not seen.Add(t.Key) Then dup.Add(t.Key)
        Next
        Return dup
    End Function

    Private Sub DrawPlotBackground(g As Graphics)
        If _plotRect.Width <= 0 OrElse _plotRect.Height <= 0 Then Return
        Using b As New SolidBrush(EffectivePlotBackColor())
            g.FillRectangle(b, _plotRect)
        End Using
    End Sub

    Private Sub DrawGridAndAxes(g As Graphics)
        If Not _axisVisible OrElse _plotRect.Width <= 0 OrElse _plotRect.Height <= 0 Then Return
        Dim fore As Color = If(_axisTextColor = Color.Empty, Palette().TextDimColor, _axisTextColor)
        Dim f As Font = EffectiveAxisFont()
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _axisLabelGap)

        ' Value ticks, from the bottom up.
        If _valueTickCount > 0 AndAlso _axisStep > 0 Then
            Dim v As Double = _axisMin
            While v <= _axisMax + _axisStep / 2.0
                Dim y As Integer = ValueToY(v)
                If _horizontalGridLines Then g.DrawLine(GridPen, _plotRect.Left, y, _plotRect.Right, y)
                Dim label As String = v.ToString(_valueFormat)
                Dim sz As Size = TextRenderer.MeasureText(label, f)
                TextRenderer.DrawText(g, label, f,
                                      New Rectangle(_plotRect.Left - gap - sz.Width, y - sz.Height \ 2, sz.Width, sz.Height),
                                      fore, TextFormatFlags.Right Or TextFormatFlags.VerticalCenter)
                v += _axisStep
            End While
        End If

        ' The two axis lines.
        g.DrawLine(AxisPen, _plotRect.Left, _plotRect.Top, _plotRect.Left, _plotRect.Bottom)
        g.DrawLine(AxisPen, _plotRect.Left, _plotRect.Bottom, _plotRect.Right, _plotRect.Bottom)

        ' Time labels: only the two ends. A real time axis is not regular, so evenly spaced labels
        ' in between would name moments at which nothing happened.
        Dim leftText As String = _minMoment.ToString(_momentFormat)
        Dim rightText As String = _maxMoment.ToString(_momentFormat)
        Dim th As Integer = TextRenderer.MeasureText(leftText, f).Height
        Dim ty As Integer = _plotRect.Bottom + gap
        If _verticalGridLines Then
            g.DrawLine(GridPen, _plotRect.Left, _plotRect.Top, _plotRect.Left, _plotRect.Bottom)
            g.DrawLine(GridPen, _plotRect.Right, _plotRect.Top, _plotRect.Right, _plotRect.Bottom)
        End If
        TextRenderer.DrawText(g, leftText, f,
                              New Rectangle(_plotRect.Left, ty, _plotRect.Width \ 2, th), fore,
                              TextFormatFlags.Left)
        If _maxTicks > _minTicks Then
            TextRenderer.DrawText(g, rightText, f,
                                  New Rectangle(_plotRect.Left + _plotRect.Width \ 2, ty, _plotRect.Width \ 2, th), fore,
                                  TextFormatFlags.Right)
        End If
    End Sub

    ''' <summary>
    ''' The dated lines, floor to ceiling of the plot, thin and dimmed.
    ''' </summary>
    ''' <remarks>
    ''' <para>One logical pixel wide whatever the emphasis of the series around it: a guide that
    ''' competes with a line for attention has stopped being background.</para>
    '''
    ''' <para><b>SOLID, and <see cref="KBotChartGuide.DashStyle"/> is deliberately ignored here.</b>
    ''' A dotted line is not one line to GDI+, it is one segment per dot, and a plot carrying a
    ''' payment line per instalment was rasterizing tens of thousands of segments per frame for
    ''' something the operator reads as a faint mark. Dimmed instead of dotted reads the same at
    ''' one pixel and costs a fraction. The property stays on the guide because it is public and
    ''' serialized; nothing paints from it any more, on either surface.</para>
    '''
    ''' <para><b>Never red</b>, and never from the automatic set either: <c>Color.Empty</c>
    ''' resolves to the dimmed text colour, the same colour the axis labels already use for
    ''' "quiet fact about the plot rather than a measurement in it".</para>
    '''
    ''' <para><b>Three things keep this cheap</b>, and all three matter: the columns are folded
    ''' (see <see cref="GuideColumn"/>), only the columns inside <paramref name="clip"/> are walked
    ''' and only the invalidated stretch of each is drawn, and the pen is rewritten once per RUN of
    ''' one colour rather than once per line - every write to a <c>Pen</c> throws away what GDI+
    ''' built from the last one.</para>
    ''' </remarks>
    Private Sub DrawGuides(g As Graphics, clip As Rectangle)
        If _guideColumns.Count = 0 OrElse _plotRect.Width <= 0 OrElse _plotRect.Height <= 0 Then Return

        ' The strip that was actually asked for, rather than the whole plot every time.
        Dim top As Integer = Math.Max(_plotRect.Top, clip.Top)
        Dim bottom As Integer = Math.Min(_plotRect.Bottom, clip.Bottom)
        If bottom <= top Then Return
        ' Opened by a pixel on each side: a line standing exactly on the edge of the strip still
        ' has to be repainted, or the band leaves a gap in it.
        Dim left As Integer = clip.Left - 1
        Dim right As Integer = clip.Right + 1

        Dim fallback As Color = Palette().TextDimColor
        Dim width As Single = CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 1)))
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
    ''' operator was already reading. A solid line at full strength is a different thing entirely:
    ''' it stops being background and starts competing with the series drawn over it. Dimming
    ''' brings back the exact reading the dots gave, without the dots.
    ''' </remarks>
    Private Const GuideDimming As Double = 0.5

    ''' <summary>
    ''' A guide colour, mixed toward the plot background - see <see cref="GuideDimming"/>.
    ''' </summary>
    ''' <remarks>
    ''' Mixed here rather than drawn with an alpha, and the two are the same picture: guides go
    ''' down BEFORE the series and the hover ring, so a dated line never has anything under it but
    ''' the plot background. An opaque pen over one known colour is the fast path, and blending
    ''' every line of every frame is exactly the cost this pass exists to get rid of.
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
    ''' Its own colour, though, never another - a payment line that changed hue under the pointer
    ''' would read as a different KIND of line, which is the one thing it must not do.
    ''' </remarks>
    Private Sub DrawHoveredGuide(g As Graphics, top As Integer, bottom As Integer, fallback As Color)
        If _hoverGuideIndex < 0 OrElse _hoverGuideIndex >= _guides.Count Then Return
        Dim gd As KBotChartGuide = _guides(_hoverGuideIndex)
        If gd.PlotX < 0 Then Return
        g.DrawLine(ScratchPen(_detailPen, If(gd.LineColor = Color.Empty, fallback, gd.LineColor),
                              CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 2)))),
                   gd.PlotX, top, gd.PlotX, bottom)
    End Sub

    ''' <summary>
    ''' Draws every visible series, ordinary ones first and the emphasised ones last, so a total
    ''' line is never buried under the parts it sums.
    ''' </summary>
    ''' <remarks>
    ''' Smoothing is on for the series and off again after them - see the note in <c>OnPaint</c>.
    ''' A series is the one thing on this surface that is genuinely slanted, and a staircase where
    ''' a slope belongs would be read as data.
    ''' </remarks>
    Private Sub DrawAllSeries(g As Graphics, clip As Rectangle)
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            For pass As Integer = 0 To 1
                For i As Integer = 0 To _series.Count - 1
                    Dim s As KBotChartSeries = _series(i)
                    If Not s.Visible OrElse s.Points.Count = 0 Then Continue For
                    If (pass = 0) = s.Emphasis Then Continue For
                    DrawOneSeries(g, s, SeriesColor(s, i), clip)
                Next
            Next
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    ''' <summary>
    ''' One series, drawn segment by segment because a point may carry its own colour.
    ''' </summary>
    ''' <remarks>
    ''' <para>A segment takes the colour of the point on its LEFT — the point it leaves, not the
    ''' one it arrives at. That is the only reading that lets the eye follow a value forward: the
    ''' stretch after a snapshot is what that snapshot was worth until the next one, so it belongs
    ''' to the snapshot that started it.</para>
    ''' <para>The tinted area under the line is cut the same way, one strip per segment, so the
    ''' wash keeps agreeing with the line above it instead of flooding the whole series in the
    ''' colour of whichever point happened to be first.</para>
    ''' <para>When no point names a colour, every segment gets <paramref name="lineColor"/> and
    ''' this draws exactly what a single <c>DrawLines</c> used to draw.</para>
    ''' <para><b>Step mode</b> (<see cref="KBotChartSeries.LineMode"/>) replaces each segment with
    ''' two — flat across, then straight up or down at the moment of the change — and BOTH keep the
    ''' left point's colour, because both belong to the stretch that point started. The corner
    ''' between them is the only vertex the data does not contain, and it carries no marker: a
    ''' marker there would claim a measurement nobody took.</para>
    ''' </remarks>
    Private Sub DrawOneSeries(g As Graphics, s As KBotChartSeries, lineColor As Color, clip As Rectangle)
        Dim pts As New List(Of Point)()
        Dim cols As New List(Of Color)()
        For Each p As KBotChartPoint In s.Points
            If Not p.Plotted Then Continue For
            pts.Add(p.PlotLocation)
            cols.Add(If(p.PointColor = Color.Empty, lineColor, p.PointColor))
        Next
        If pts.Count = 0 Then Return

        Dim stepped As Boolean = s.LineMode = KBotChartLineMode.Step
        Dim alpha As Integer = CInt(255.0 * _areaFillOpacity / 100.0)
        Dim fills As Boolean = s.FillArea AndAlso alpha > 0 AndAlso pts.Count > 1
        Dim width As Integer = ThemeShapes.ScaleDpi(Me, If(s.Emphasis, _emphasisLineWidth, _lineWidth))

        ' Segments are drawn in SLICES: a stretch of consecutive segments that share one colour AND
        ' reach into the invalidated strip. Two things come out of that.
        '
        ' One call per stretch instead of one per segment - a chain of twenty snapshots nobody
        ' coloured by hand is a single stretch, so it is one DrawLines and one FillPolygon rather
        ' than twenty of each, and each of those calls was building a Pen or a Brush and setting up
        ' a path of its own. Merging also takes away the faint seam the old strips left where two
        ' antialiased edges met over the same column.
        '
        ' And the ones outside the strip are never handed over at all. GDI+ does not cheaply reject
        ' a polyline that runs the width of the plot: it rasterizes the whole thing and throws away
        ' what falls outside the clip, which is why a hover asking for one narrow column was paying
        ' for every line on the surface (measured: 70 ms of a 70 ms strip repaint).
        Dim side As Integer = If(_markerStyle = KBotChartMarkerStyle.None, 0, ThemeShapes.ScaleDpi(Me, _markerSize))
        Dim padX As Integer = width + side \ 2 + 4
        Dim lo As Integer = clip.Left - padX
        Dim hi As Integer = clip.Right + padX

        Dim first As Integer = 0
        While first < pts.Count - 1
            Dim runArgb As Integer = cols(first).ToArgb()
            Dim last As Integer = first
            While last < pts.Count - 1 AndAlso cols(last).ToArgb() = runArgb
                last += 1
            End While

            Dim sliceStart As Integer = -1
            For i As Integer = first To last - 1
                If Math.Max(pts(i).X, pts(i + 1).X) >= lo AndAlso Math.Min(pts(i).X, pts(i + 1).X) <= hi Then
                    If sliceStart < 0 Then sliceStart = i
                ElseIf sliceStart >= 0 Then
                    DrawRunSlice(g, pts, sliceStart, i, cols(first), width, stepped, fills, alpha)
                    sliceStart = -1
                End If
            Next
            If sliceStart >= 0 Then
                DrawRunSlice(g, pts, sliceStart, last, cols(first), width, stepped, fills, alpha)
            End If
            first = last
        End While

        If _markerStyle = KBotChartMarkerStyle.None OrElse _markerSize <= 0 Then Return
        Dim edge As Pen = ScratchPen(_edgePen, EffectivePlotBackColor(),
                                     CSng(Math.Max(1, ThemeShapes.ScaleDpi(Me, 1))))
        Dim fill As SolidBrush = Nothing
        Dim fillArgb As Integer = 0
        Dim reach As Integer = side \ 2 + 2
        For i As Integer = 0 To pts.Count - 1
            ' A marker outside the invalidated strip is drawn for GDI+ to throw away again. On a
            ' hover, which asks for one narrow column, that is nearly every marker on the plot.
            If pts(i).X + reach < clip.Left OrElse pts(i).X - reach > clip.Right Then Continue For
            If pts(i).Y + reach < clip.Top OrElse pts(i).Y - reach > clip.Bottom Then Continue For
            If fill Is Nothing OrElse cols(i).ToArgb() <> fillArgb Then
                fill = ScratchBrush(_fillBrush, cols(i))
                fillArgb = cols(i).ToArgb()
            End If
            DrawMarker(g, pts(i), side, fill, edge)
        Next
    End Sub

    ''' <summary>
    ''' One stretch of the line, and the wash under it, in a single pair of calls.
    ''' </summary>
    ''' <remarks>
    ''' The wash is closed by dropping straight to the floor of the plot at each end of the
    ''' stretch. Those two vertical edges are the only part of the shape the data does not
    ''' describe, and a stretch is only ever cut where it is outside the invalidated strip, so
    ''' neither edge can appear on screen as a seam.
    ''' </remarks>
    Private Sub DrawRunSlice(g As Graphics, pts As List(Of Point), first As Integer, last As Integer,
                             c As Color, width As Integer, stepped As Boolean,
                             fills As Boolean, alpha As Integer)
        Dim poly As Point() = RunPolyline(pts, first, last, stepped)
        If poly.Length < 2 Then Return
        If fills Then
            Dim under(poly.Length + 1) As Point
            Array.Copy(poly, under, poly.Length)
            under(poly.Length) = New Point(poly(poly.Length - 1).X, _plotRect.Bottom)
            under(poly.Length + 1) = New Point(poly(0).X, _plotRect.Bottom)
            g.FillPolygon(ScratchBrush(_fillBrush, Color.FromArgb(alpha, c)), under)
        End If
        g.DrawLines(ScratchPen(_detailPen, c, CSng(width), LineCap.Round), poly)
    End Sub

    ''' <summary>
    ''' The points of one stretch as a single polyline, ready for <c>DrawLines</c>.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Step mode</b> (<see cref="KBotChartSeries.LineMode"/>) puts a corner between every
    ''' pair - flat across, then straight up or down at the moment of the change - and both halves
    ''' keep the left point's colour, because both belong to the stretch that point started.</para>
    ''' <para>A vertex identical to the one before it is dropped rather than emitted: two snapshots
    ''' at the same moment give no horizontal run and two equal values give no riser, and a
    ''' zero-length segment under a round cap would paint a dot nobody measured.</para>
    ''' </remarks>
    Private Shared Function RunPolyline(pts As List(Of Point), first As Integer, last As Integer,
                                        stepped As Boolean) As Point()
        Dim outp As New List(Of Point)((last - first + 1) * If(stepped, 2, 1))
        outp.Add(pts(first))
        For i As Integer = first To last - 1
            If stepped Then
                Dim corner As New Point(pts(i + 1).X, pts(i).Y)
                If corner <> outp(outp.Count - 1) Then outp.Add(corner)
            End If
            If pts(i + 1) <> outp(outp.Count - 1) Then outp.Add(pts(i + 1))
        Next
        Return outp.ToArray()
    End Function

    Private Sub DrawMarker(g As Graphics, center As Point, side As Integer, fill As Brush, edge As Pen)
        Dim r As New Rectangle(center.X - side \ 2, center.Y - side \ 2, side, side)
        Select Case _markerStyle
            Case KBotChartMarkerStyle.Square
                g.FillRectangle(fill, r)
                g.DrawRectangle(edge, r)
            Case KBotChartMarkerStyle.Diamond
                Dim pts() As Point = {
                    New Point(center.X, r.Top),
                    New Point(r.Right, center.Y),
                    New Point(center.X, r.Bottom),
                    New Point(r.Left, center.Y)}
                g.FillPolygon(fill, pts)
                g.DrawPolygon(edge, pts)
            Case Else
                g.FillEllipse(fill, r)
                g.DrawEllipse(edge, r)
        End Select
    End Sub

    ''' <summary>Puts a ring around the marker under the pointer, so the label names something visible.</summary>
    Private Sub DrawHoverHighlight(g As Graphics)
        If _hoverSeriesIndex < 0 OrElse _hoverSeriesIndex >= _series.Count Then Return
        Dim s As KBotChartSeries = _series(_hoverSeriesIndex)
        If _hoverPointIndex < 0 OrElse _hoverPointIndex >= s.Points.Count Then Return
        Dim p As KBotChartPoint = s.Points(_hoverPointIndex)
        If Not p.Plotted Then Return

        Dim side As Integer = ThemeShapes.ScaleDpi(Me, Math.Max(4, _markerSize) + 4)
        Dim r As New Rectangle(p.PlotLocation.X - side \ 2, p.PlotLocation.Y - side \ 2, side, side)
        Dim ring As Color = If(p.PointColor = Color.Empty, SeriesColor(s, _hoverSeriesIndex), p.PointColor)
        ' A circle, so this one does want smoothing - see the note in OnPaint.
        Dim old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Try
            Using pen As New Pen(ring, Math.Max(1, ThemeShapes.ScaleDpi(Me, 2)))
                g.DrawEllipse(pen, r)
            End Using
        Finally
            g.SmoothingMode = old
        End Try
    End Sub

    Private Sub DrawLegend(g As Graphics)
        If _legendRect.Width <= 0 OrElse _legendRect.Height <= 0 Then Return
        Dim fore As Color = If(_legendTextColor = Color.Empty, Palette().TextDimColor, _legendTextColor)
        Dim f As Font = EffectiveAxisFont()
        Dim gap As Integer = ThemeShapes.ScaleDpi(Me, _legendSpacing)
        Dim swatch As Integer = ThemeShapes.ScaleDpi(Me, 12)
        Dim x As Integer = _legendRect.Left

        For i As Integer = 0 To _series.Count - 1
            Dim s As KBotChartSeries = _series(i)
            If Not s.Visible OrElse s.Points.Count = 0 Then Continue For
            Dim label As String = If(s.Text, String.Empty)
            Dim tw As Integer = TextRenderer.MeasureText(label, f).Width
            Dim need As Integer = swatch + ThemeShapes.ScaleDpi(Me, 4) + tw
            ' What does not fit is dropped, not squeezed: half a name is worse than no name.
            If x + need > _legendRect.Right Then Return

            Dim y As Integer = _legendRect.Top + _legendRect.Height \ 2
            Using pen As New Pen(SeriesColor(s, i), ThemeShapes.ScaleDpi(Me, If(s.Emphasis, _emphasisLineWidth, _lineWidth)))
                g.DrawLine(pen, x, y, x + swatch, y)
            End Using
            TextRenderer.DrawText(g, label, f,
                                  New Rectangle(x + swatch + ThemeShapes.ScaleDpi(Me, 4), _legendRect.Top, tw, _legendRect.Height),
                                  fore, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)
            x += need + gap
        Next
    End Sub

    Private Sub DrawEmptyState(g As Graphics)
        If String.IsNullOrEmpty(_emptyText) OrElse _plotRect.Width <= 0 Then Return
        Dim fore As Color = If(_emptyTextColor = Color.Empty, Palette().TextDimColor, _emptyTextColor)
        TextRenderer.DrawText(g, _emptyText, Font, _plotRect, fore,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.WordBreak)
    End Sub
    ''' <summary>
    ''' The colour of a series: the one it was given, or the index-th of the automatic set.
    ''' </summary>
    Private Function SeriesColor(s As KBotChartSeries, index As Integer) As Color
        If s.LineColor <> Color.Empty Then Return s.LineColor
        Return AutoColor(index)
    End Function
    ''' <summary>
    ''' The <paramref name="index"/>-th colour of the automatic set — what a series or a point left
    ''' at <see cref="Color.Empty"/> is drawn in.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Public</b> because a host that colours something OUTSIDE the chart — a list beside
    ''' it, or the lane view under it, whose rows are the same facts as the points — has to be able
    ''' to ask for the same colours rather than invent a parallel set that drifts on the next theme
    ''' change.</para>
    ''' <para>The set itself lives in <c>KBotAutoPalette</c>, shared with <c>KBotLaneView</c> so the
    ''' two surfaces cannot disagree about what the n-th colour is. Read it there for why the
    ''' sequence is built the way it is, and for why it never contains red.</para>
    ''' </remarks>
    Public Function AutoColor(index As Integer) As Color
        Return KBotAutoPalette.ColorAt(Palette().AccentColor, _isDarkScheme, index)
    End Function


    ' =====================================================================
    ' MOUSE
    ' =====================================================================

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            EnsureLayout()
            Dim tabIndex As Integer = TabIndexAt(e.Location)
            If tabIndex >= 0 AndAlso Not _tabs(tabIndex).Enabled Then tabIndex = -1
            If tabIndex <> _hoverTabIndex Then
                _hoverTabIndex = tabIndex
                Invalidate()
            End If

            Dim si As Integer = -1
            Dim pi As Integer = -1
            If tabIndex < 0 Then HitTestPoint(e.Location, si, pi)

            ' Only once nothing else has the pointer: a marker can be clicked, a guide cannot, so
            ' the marker always wins the pixel they share.
            Dim gi As Integer = -1
            If tabIndex < 0 AndAlso si < 0 Then gi = HitTestGuide(e.Location)
            If gi <> _hoverGuideIndex Then
                ' Only the two columns, not the whole plot: the pointer crossing a surface full of
                ' payment lines used to ask for a full repaint per line it passed.
                Dim wasGuide As Integer = _hoverGuideIndex
                _hoverGuideIndex = gi
                InvalidateGuideColumn(wasGuide)
                InvalidateGuideColumn(gi)
            End If

            If si <> _hoverSeriesIndex OrElse pi <> _hoverPointIndex Then
                _hoverSeriesIndex = si
                _hoverPointIndex = pi
                Invalidate()
                RaiseEvent PointHovered(If(si >= 0, _series(si).Key, Nothing), pi)
            End If

            RefreshChartTip()
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChartView.OnMouseMove", ex)
        End Try
    End Sub

    ''' <summary>Repaints the column one dated line stands in, top to bottom of the plot.</summary>
    ''' <remarks>
    ''' Wide enough for the hovered line at its two-pixel weight plus a pixel each side, so the
    ''' line that stops being hovered leaves no crumb of its thicker self behind.
    ''' </remarks>
    Private Sub InvalidateGuideColumn(index As Integer)
        If index < 0 OrElse index >= _guides.Count Then Return
        Dim gd As KBotChartGuide = _guides(index)
        If gd.PlotX < 0 OrElse _plotRect.Height <= 0 Then Return
        Dim half As Integer = Math.Max(1, ThemeShapes.ScaleDpi(Me, 3))
        Invalidate(New Rectangle(gd.PlotX - half, _plotRect.Top, half * 2 + 1, _plotRect.Height))
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Try
            Dim changed As Boolean = _hoverTabIndex <> -1 OrElse _hoverSeriesIndex <> -1 OrElse
                                     _hoverGuideIndex <> -1
            Dim hadPoint As Boolean = _hoverSeriesIndex <> -1
            _hoverTabIndex = -1
            _hoverSeriesIndex = -1
            _hoverPointIndex = -1
            _hoverGuideIndex = -1
            HideChartTip()
            If changed Then Invalidate()
            If hadPoint Then RaiseEvent PointHovered(Nothing, -1)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChartView.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Focus()
            EnsureLayout()

            Dim tabIndex As Integer = TabIndexAt(e.Location)
            If tabIndex >= 0 Then
                ' In the designer a click does NOT switch: it would dirty somebody's form with a
                ' state they did not choose (the same rule as the tree's collapse button).
                If KBotDesignTime.IsDesignTime(Me) Then Return
                If Not _tabs(tabIndex).Enabled Then Return
                _focusTabIndex = tabIndex
                ActivateTab(tabIndex)
                Return
            End If

            If KBotDesignTime.IsDesignTime(Me) Then Return
            Dim si As Integer = -1
            Dim pi As Integer = -1
            HitTestPoint(e.Location, si, pi)
            If si >= 0 Then RaiseEvent PointClicked(_series(si).Key, pi)
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChartView.OnMouseDown", ex)
        End Try
    End Sub

    ' The common road of the mouse and the keyboard: move the band, raise once, and only on a real
    ' change — pressing the button that is already current is not a choice.
    Private Sub ActivateTab(index As Integer)
        If index < 0 OrElse index >= _tabs.Count Then Return
        Dim t As KBotChartTab = _tabs(index)
        If Not t.Visible OrElse Not t.Enabled Then Return
        If String.Equals(t.Key, _selectedTabKey, StringComparison.Ordinal) Then Return
        _selectedTabKey = t.Key
        HideChartTip()
        Invalidate()
        RaiseEvent TabSelected(t.Key)
    End Sub

    Private Function TabIndexAt(location As Point) As Integer
        For i As Integer = 0 To _tabs.Count - 1
            Dim t As KBotChartTab = _tabs(i)
            If t.Visible AndAlso t.Bounds.Width > 0 AndAlso t.Bounds.Contains(location) Then Return i
        Next
        Return -1
    End Function

    ''' <summary>
    ''' The marker nearest the pointer, within <see cref="HoverRadius"/>. Emphasised series win a
    ''' tie: the operator reached for the line that is drawn on top.
    ''' </summary>
    Private Sub HitTestPoint(location As Point, ByRef seriesIndex As Integer, ByRef pointIndex As Integer)
        seriesIndex = -1
        pointIndex = -1
        If _markerStyle = KBotChartMarkerStyle.None OrElse _markerSize <= 0 Then Return
        If Not _plotRect.Contains(location) Then Return

        Dim reach As Integer = ThemeShapes.ScaleDpi(Me, _hoverRadius)
        Dim best As Double = CDbl(reach) * reach + 1
        Dim bestEmphasis As Boolean = False

        For i As Integer = 0 To _series.Count - 1
            Dim s As KBotChartSeries = _series(i)
            If Not s.Visible Then Continue For
            For j As Integer = 0 To s.Points.Count - 1
                Dim p As KBotChartPoint = s.Points(j)
                If Not p.Plotted Then Continue For
                Dim dx As Double = p.PlotLocation.X - location.X
                Dim dy As Double = p.PlotLocation.Y - location.Y
                Dim d2 As Double = dx * dx + dy * dy
                If d2 > CDbl(reach) * reach Then Continue For
                If d2 < best OrElse (d2 = best AndAlso s.Emphasis AndAlso Not bestEmphasis) Then
                    best = d2
                    bestEmphasis = s.Emphasis
                    seriesIndex = i
                    pointIndex = j
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' The guide nearest the pointer, within <see cref="HoverRadius"/>. Returns -1 for none.
    ''' </summary>
    ''' <remarks>
    ''' <b>Horizontal distance only.</b> A guide is a whole column of the plot, not a spot on it:
    ''' the operator points at "that payment", and where their pointer sits vertically says
    ''' nothing about which one they mean. Measuring the real distance would make a guide
    ''' unreachable near the top and bottom of a tall plot for no reason the operator could guess.
    ''' </remarks>
    Private Function HitTestGuide(location As Point) As Integer
        If _guides.Count = 0 OrElse Not _plotRect.Contains(location) Then Return -1
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
    ''' Decides, from the hover state already computed, which label is due. One place: a button and
    ''' a marker cannot be hovered at the same time, so they cannot ask for two labels at once.
    ''' </summary>
    Private Sub RefreshChartTip()
        If Not _pointTooltipEnabled Then Return
        If _hoverTabIndex >= 0 Then
            Dim t As KBotChartTab = _tabs(_hoverTabIndex)
            ShowChartTip($"tab:{_hoverTabIndex}", Nothing, t.Tooltip, Nothing)
            Return
        End If
        If _hoverSeriesIndex >= 0 AndAlso _hoverPointIndex >= 0 Then
            Dim s As KBotChartSeries = _series(_hoverSeriesIndex)
            Dim p As KBotChartPoint = s.Points(_hoverPointIndex)
            Dim header As String = If(String.IsNullOrEmpty(p.TooltipHeader), If(s.Text, String.Empty), p.TooltipHeader)
            Dim body As String = If(String.IsNullOrEmpty(p.TooltipText),
                                    $"{p.Moment.ToString(_momentFormat)} · {p.Value.ToString(_valueFormat)}",
                                    p.TooltipText)
            ShowChartTip($"pt:{_hoverSeriesIndex}:{_hoverPointIndex}", header, body, p.TooltipFooter)
            Return
        End If
        If _hoverGuideIndex >= 0 AndAlso _hoverGuideIndex < _guides.Count Then
            Dim gd As KBotChartGuide = _guides(_hoverGuideIndex)
            ' A guide with NO text at all opens nothing: an unnamed line is a mark on the plot the
            ' host chose not to explain, and a label saying only its date would add nothing.
            If Not String.IsNullOrEmpty(gd.Text) OrElse Not String.IsNullOrEmpty(gd.Tooltip) Then
                Dim body As String = If(String.IsNullOrEmpty(gd.Tooltip),
                                        gd.Moment.ToString(_momentFormat), gd.Tooltip)
                ShowChartTip($"gd:{_hoverGuideIndex}", gd.Text, body, Nothing)
                Return
            End If
        End If
        HideChartTip()
    End Sub

    ''' <summary>
    ''' Asks for the label of the thing identified by <paramref name="key"/> (a stable internal
    ''' handle, not the text). The same key twice in a row does nothing — the label stays where it
    ''' is. A new key, or <c>Nothing</c>, puts out what was there.
    ''' </summary>
    Private Sub ShowChartTip(key As String, header As String, body As String, footer As String)
        If KBotDesignTime.IsDesignTime(Me) Then Return
        If String.Equals(key, _currentTipKey, StringComparison.Ordinal) Then Return
        _currentTipKey = key

        If String.IsNullOrEmpty(key) OrElse
           (String.IsNullOrEmpty(header) AndAlso String.IsNullOrEmpty(body) AndAlso String.IsNullOrEmpty(footer)) Then
            _pointTooltip?.HideNow()
            Return
        End If

        _tipContent.HeaderText = If(header, String.Empty)
        _tipContent.Text = If(body, String.Empty)
        _tipContent.FooterText = If(footer, String.Empty)
        PointTooltip.ShowAt(Me, _tipContent, Cursor.Position)
    End Sub

    ''' <summary>Puts out the label (the pointer left everything that has one).</summary>
    Private Sub HideChartTip()
        ShowChartTip(Nothing, Nothing, Nothing, Nothing)
    End Sub

    ' =====================================================================
    ' KEYBOARD
    ' =====================================================================

    ' Without this the form eats the arrows and the space before they reach the band.
    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        If keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Space Then Return True
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Select Case e.KeyCode
                Case Keys.Left
                    MoveTabFocus(-1)
                    e.Handled = True
                Case Keys.Right
                    MoveTabFocus(1)
                    e.Handled = True
                Case Keys.Space, Keys.Enter
                    ' Space is the keyboard's road to EXACTLY what the click does.
                    If _focusTabIndex < 0 Then MoveTabFocus(1)
                    ActivateTab(_focusTabIndex)
                    e.Handled = True
            End Select
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotChartView.OnKeyDown", ex)
        End Try
    End Sub

    ' The next VISIBLE and ENABLED button in that direction, without wrapping (as in KBotNavList).
    Private Sub MoveTabFocus(direction As Integer)
        If _tabs.Count = 0 Then Return
        Dim idx As Integer = _focusTabIndex + direction
        If _focusTabIndex < 0 Then idx = If(direction > 0, 0, _tabs.Count - 1)
        While idx >= 0 AndAlso idx < _tabs.Count
            Dim t As KBotChartTab = _tabs(idx)
            If t.Visible AndAlso t.Enabled Then
                _focusTabIndex = idx
                Invalidate()
                Return
            End If
            idx += direction
        End While
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        ' The first Tab into the chart has to light a button, otherwise Space has nothing to act on.
        If _focusTabIndex < 0 Then MoveTabFocus(1)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    ' =====================================================================
    ' FRIEND HOOKS FOR TESTS (headless, no screen)
    ' =====================================================================

    ''' <summary>Friend test hook: force a layout pass, without painting.</summary>
    Friend Sub DebugEnsureLayout()
        EnsureLayout()
    End Sub

    ''' <summary>Friend test hook: the computed plot rectangle.</summary>
    Friend Function DebugPlotRect() As Rectangle
        EnsureLayout()
        Return _plotRect
    End Function

    ''' <summary>Friend test hook: the rounded value axis, as (min, max, step).</summary>
    Friend Function DebugValueAxis() As Double()
        EnsureLayout()
        Return New Double() {_axisMin, _axisMax, _axisStep}
    End Function

    ''' <summary>Friend test hook: the computed slot of a button (Empty if hidden).</summary>
    Friend Function DebugTabBounds(index As Integer) As Rectangle
        EnsureLayout()
        Return _tabs(index).Bounds
    End Function

    ''' <summary>Friend test hook: where a point landed (Empty if it was not plotted).</summary>
    Friend Function DebugPointLocation(seriesIndex As Integer, pointIndex As Integer) As Point
        EnsureLayout()
        Return _series(seriesIndex).Points(pointIndex).PlotLocation
    End Function

    ''' <summary>Friend test hook: left click on the real road (band and markers included).</summary>
    Friend Sub DebugClickAt(location As Point)
        OnMouseDown(New MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0))
    End Sub

    ''' <summary>Friend test hook: where a guide landed on the horizontal axis (-1 if not drawn).</summary>
    Friend Function DebugGuideX(index As Integer) As Integer
        EnsureLayout()
        Return _guides(index).PlotX
    End Function

    ''' <summary>Friend test hook: the guide nearest a client point, or -1.</summary>
    Friend Function DebugHitTestGuide(location As Point) As Integer
        EnsureLayout()
        Return HitTestGuide(location)
    End Function

    ''' <summary>Friend test hook: the marker nearest a client point, as (seriesIndex, pointIndex).</summary>
    Friend Function DebugHitTest(location As Point) As Integer()
        EnsureLayout()
        Dim si As Integer = -1
        Dim pi As Integer = -1
        HitTestPoint(location, si, pi)
        Return New Integer() {si, pi}
    End Function
End Class
