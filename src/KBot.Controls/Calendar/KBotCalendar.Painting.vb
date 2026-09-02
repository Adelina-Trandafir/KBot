Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' <see cref="KBotCalendar"/> — geometry and painting. Split out for the same reason the tree and
''' the chart are split: the API half stays readable, and the half that runs on every repaint sits
''' together in one file.
'''
''' <para>Layout is computed ONCE per change (<c>_layoutDirty</c>) and read by both the painter and
''' the hit test, so what the operator sees and what the click lands on can never drift apart.</para>
''' </summary>
Partial Public NotInheritable Class KBotCalendar

    ' Six week rows x seven columns is the largest page; the month and year pages use the first 12.
    Private Const MaxCells As Integer = 42

    Private ReadOnly _cells(MaxCells - 1) As Rectangle
    Private ReadOnly _cellDate(MaxCells - 1) As Date
    Private ReadOnly _cellEnabled(MaxCells - 1) As Boolean
    Private ReadOnly _cellTrailing(MaxCells - 1) As Boolean
    Private _cellCount As Integer

    Private _layoutDirty As Boolean = True
    Private _headerRect As Rectangle
    Private _prevRect As Rectangle
    Private _nextRect As Rectangle
    Private _titleRect As Rectangle
    Private _dayNamesRect As Rectangle
    Private _bodyRect As Rectangle
    Private _footerRect As Rectangle
    Private _footerTextRect As Rectangle
    Private _weekColWidth As Integer

    ' Derived from Font (bold title, smaller day names). Cached: deriving a font on every repaint
    ' allocates a GDI handle per paint.
    Private _titleFont As Font
    Private _smallFont As Font

    ''' <summary>Asks for a fresh layout at the next use. Cheap and idempotent.</summary>
    Private Sub InvalidateLayout()
        _layoutDirty = True
        Invalidate()
    End Sub

    ''' <summary>
    ''' The size the calendar needs to show a whole month without squeezing: seven columns wide
    ''' enough for the widest day name, six week rows tall enough for the font, plus header,
    ''' day-name strip and today row. This is what the drop-down window is sized from.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property NaturalSize As Size
        Get
            Try
                Dim latime As Integer = ThemeShapes.ScaleDpi(Me, 34)
                Dim numeZile As String() = ShortDayNames()
                For Each n As String In numeZile
                    latime = Math.Max(latime, TextRenderer.MeasureText(n, Font).Width + ThemeShapes.ScaleDpi(Me, 10))
                Next
                latime = Math.Max(latime, TextRenderer.MeasureText("30", Font).Width + ThemeShapes.ScaleDpi(Me, 16))

                Dim inaltime As Integer = Math.Max(ThemeShapes.ScaleDpi(Me, 28), Font.Height + ThemeShapes.ScaleDpi(Me, 10))
                Dim margine As Integer = ThemeShapes.ScaleDpi(Me, _borderWidth) * 2

                ' The air counts: a calendar sized without it comes out squeezed by exactly the
                ' amount the operator asked to be left empty.
                Dim aer As Padding = ScalePad(Padding)
                Dim aerGrila As Padding = ScalePad(_gridPadding)

                Dim w As Integer = latime * 7 + WeekColumnWidth() + margine +
                                   aer.Horizontal + aerGrila.Horizontal
                Dim h As Integer = ThemeShapes.ScaleDpi(Me, _headerHeight) +
                                   If(_dayNamesHeight > 0, ThemeShapes.ScaleDpi(Me, _dayNamesHeight), 0) +
                                   inaltime * 6 +
                                   If(_showToday, ThemeShapes.ScaleDpi(Me, _footerHeight), 0) +
                                   margine + aer.Vertical + aerGrila.Vertical
                Return New Size(w, h)
            Catch ex As Exception
                GlobalErrorLog.Write("KBotCalendar.NaturalSize", ex)
                Return New Size(240, 220)
            End Try
        End Get
    End Property

    ''' <summary>A host that asks for the preferred size gets <see cref="NaturalSize"/>.</summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Return NaturalSize
    End Function

    ' =====================================================================
    ' LAYOUT
    ' =====================================================================

    ' Logical px -> device px, one side at a time (C2).
    Private Function ScalePad(p As Padding) As Padding
        Return New Padding(ThemeShapes.ScaleDpi(Me, p.Left), ThemeShapes.ScaleDpi(Me, p.Top),
                           ThemeShapes.ScaleDpi(Me, p.Right), ThemeShapes.ScaleDpi(Me, p.Bottom))
    End Function

    ' A rectangle with the air taken out of it, never smaller than nothing.
    Private Shared Function Shrink(r As Rectangle, p As Padding) As Rectangle
        Return New Rectangle(r.Left + p.Left, r.Top + p.Top,
                             Math.Max(0, r.Width - p.Horizontal), Math.Max(0, r.Height - p.Vertical))
    End Function

    Private Function WeekColumnWidth() As Integer
        If Not _showWeekNumbers Then Return 0
        Return Math.Max(ThemeShapes.ScaleDpi(Me, 24),
                        TextRenderer.MeasureText("52", Font).Width + ThemeShapes.ScaleDpi(Me, 8))
    End Function

    ''' <summary>Recomputes every rectangle and every cell date. One writer, two readers.</summary>
    Private Sub EnsureLayout()
        If Not _layoutDirty Then Return
        _layoutDirty = False

        Dim b As Integer = ThemeShapes.ScaleDpi(Me, _borderWidth)
        ' Inside the border, then inside the OUTER air: everything below is laid out in what is left.
        Dim inner As Rectangle = Shrink(Rectangle.Inflate(ClientRectangle, -b, -b), ScalePad(Padding))
        If inner.Width <= 0 OrElse inner.Height <= 0 Then
            _headerRect = Rectangle.Empty
            _prevRect = Rectangle.Empty
            _nextRect = Rectangle.Empty
            _titleRect = Rectangle.Empty
            _dayNamesRect = Rectangle.Empty
            _bodyRect = Rectangle.Empty
            _footerRect = Rectangle.Empty
            _footerTextRect = Rectangle.Empty
            _cellCount = 0
            Return
        End If

        RebuildFonts()

        ' Header band, with a square button at each end. The band keeps the full width — it is a
        ' painted surface — while the arrows and the title live inside HeaderPadding.
        Dim hh As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _headerHeight), inner.Height)
        _headerRect = New Rectangle(inner.Left, inner.Top, inner.Width, hh)
        Dim capInterior As Rectangle = Shrink(_headerRect, ScalePad(_headerPadding))
        Dim buton As Integer = Math.Min(Math.Max(capInterior.Height, ThemeShapes.ScaleDpi(Me, 24)),
                                        capInterior.Width \ 4)
        _prevRect = New Rectangle(capInterior.Left, capInterior.Top, buton, capInterior.Height)
        _nextRect = New Rectangle(capInterior.Right - buton, capInterior.Top, buton, capInterior.Height)
        _titleRect = New Rectangle(_prevRect.Right, capInterior.Top,
                                   Math.Max(0, _nextRect.Left - _prevRect.Right), capInterior.Height)

        Dim ramas As Rectangle = New Rectangle(inner.Left, _headerRect.Bottom,
                                               inner.Width, Math.Max(0, inner.Bottom - _headerRect.Bottom))

        ' Today row at the bottom. Same rule as the header: the band is full width, the text sits
        ' inside FooterPadding.
        If _showToday Then
            Dim fh As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _footerHeight), ramas.Height)
            _footerRect = New Rectangle(ramas.Left, ramas.Bottom - fh, ramas.Width, fh)
            _footerTextRect = Shrink(_footerRect, ScalePad(_footerPadding))
            ramas = New Rectangle(ramas.Left, ramas.Top, ramas.Width, Math.Max(0, ramas.Height - fh))
        Else
            _footerRect = Rectangle.Empty
            _footerTextRect = Rectangle.Empty
        End If

        ' The grid air is taken ONCE, before the day names are split off, so the headings and the
        ' columns underneath them are cut from the same width and cannot drift apart.
        ramas = Shrink(ramas, ScalePad(_gridPadding))

        ' Day-name strip: only the day page has one.
        If _view = KBotCalendarView.Days AndAlso _dayNamesHeight > 0 Then
            Dim dh As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, _dayNamesHeight), ramas.Height)
            _dayNamesRect = New Rectangle(ramas.Left, ramas.Top, ramas.Width, dh)
            ramas = New Rectangle(ramas.Left, ramas.Top + dh, ramas.Width, Math.Max(0, ramas.Height - dh))
        Else
            _dayNamesRect = Rectangle.Empty
        End If

        _bodyRect = ramas
        _weekColWidth = If(_view = KBotCalendarView.Days, WeekColumnWidth(), 0)

        Select Case _view
            Case KBotCalendarView.Days
                BuildDayCells()
            Case KBotCalendarView.Months
                BuildMonthCells()
            Case Else
                BuildYearCells()
        End Select
    End Sub

    ' The grid area, i.e. the body minus the week-number column.
    Private Function GridArea() As Rectangle
        Return New Rectangle(_bodyRect.Left + _weekColWidth, _bodyRect.Top,
                             Math.Max(0, _bodyRect.Width - _weekColWidth), _bodyRect.Height)
    End Function

    ''' <summary>
    ''' One cell of a <paramref name="cols"/> x <paramref name="rows"/> grid. Both edges are
    ''' computed from the area, never from a rounded cell width — that is what keeps the last
    ''' column flush with the border instead of one or two pixels short of it.
    ''' </summary>
    Private Shared Function CellRect(area As Rectangle, col As Integer, row As Integer,
                                     cols As Integer, rows As Integer) As Rectangle
        If cols <= 0 OrElse rows <= 0 Then Return Rectangle.Empty
        Dim x1 As Integer = area.Left + CInt(area.Width * (col / CDbl(cols)))
        Dim x2 As Integer = area.Left + CInt(area.Width * ((col + 1) / CDbl(cols)))
        Dim y1 As Integer = area.Top + CInt(area.Height * (row / CDbl(rows)))
        Dim y2 As Integer = area.Top + CInt(area.Height * ((row + 1) / CDbl(rows)))
        Return New Rectangle(x1, y1, Math.Max(0, x2 - x1), Math.Max(0, y2 - y1))
    End Function

    ' The first cell of the day page: the displayed month walked back to the start of its week.
    Private Function FirstVisibleDay() As Date
        Dim delta As Integer = (CInt(_displayMonth.DayOfWeek) - CInt(_firstDayOfWeek) + 7) Mod 7
        If _displayMonth <= Date.MinValue.AddDays(delta) Then Return _displayMonth
        Return _displayMonth.AddDays(-delta)
    End Function

    Private Sub BuildDayCells()
        Dim area As Rectangle = GridArea()
        Dim start As Date = FirstVisibleDay()
        _cellCount = 42
        For i As Integer = 0 To _cellCount - 1
            Dim zi As Date
            Try
                zi = start.AddDays(i)
            Catch ex As ArgumentOutOfRangeException
                ' Past Date.MaxValue at the very end of the supported range: the tail cells stay
                ' on the last day and are simply not selectable.
                GlobalErrorLog.Write("KBotCalendar.BuildDayCells", ex)
                zi = Date.MaxValue.Date
            End Try
            _cellDate(i) = zi
            _cellTrailing(i) = (zi.Month <> _displayMonth.Month OrElse zi.Year <> _displayMonth.Year)
            _cellEnabled(i) = (zi >= _minDate AndAlso zi <= _maxDate)
            _cells(i) = CellRect(area, i Mod 7, i \ 7, 7, 6)
        Next
    End Sub

    Private Sub BuildMonthCells()
        Dim area As Rectangle = GridArea()
        _cellCount = 12
        For i As Integer = 0 To _cellCount - 1
            Dim luna As New Date(_displayMonth.Year, i + 1, 1)
            Dim ultima As Date = luna.AddDays(Date.DaysInMonth(luna.Year, luna.Month) - 1)
            _cellDate(i) = luna
            _cellTrailing(i) = False
            _cellEnabled(i) = (ultima >= _minDate AndAlso luna <= _maxDate)
            _cells(i) = CellRect(area, i Mod 4, i \ 4, 4, 3)
        Next
    End Sub

    Private Sub BuildYearCells()
        Dim area As Rectangle = GridArea()
        Dim decada As Integer = (_displayMonth.Year \ 10) * 10
        _cellCount = 12
        For i As Integer = 0 To _cellCount - 1
            Dim an As Integer = decada - 1 + i
            Dim inRange As Boolean = (an >= 1 AndAlso an <= 9999)
            Dim anClamp As Integer = Math.Max(1, Math.Min(9999, an))
            Dim primaZi As New Date(anClamp, 1, 1)
            Dim ultimaZi As New Date(anClamp, 12, 31)
            _cellDate(i) = primaZi
            ' The two neighbours of the decade are drawn dim, exactly like the trailing days.
            _cellTrailing(i) = (an < decada OrElse an > decada + 9)
            _cellEnabled(i) = inRange AndAlso ultimaZi >= _minDate AndAlso primaZi <= _maxDate
            _cells(i) = CellRect(area, i Mod 4, i \ 4, 4, 3)
        Next
    End Sub

    ''' <summary>Which region is under the point: a cell index, or one of the Hit* answers.</summary>
    Private Function HitTest(p As Point) As Integer
        EnsureLayout()
        If _prevRect.Contains(p) Then Return HitPrev
        If _nextRect.Contains(p) Then Return HitNext
        If _titleRect.Contains(p) Then Return HitTitle
        If _showToday AndAlso _footerRect.Contains(p) Then Return HitToday
        For i As Integer = 0 To _cellCount - 1
            If _cells(i).Contains(p) Then Return i
        Next
        Return HitNone
    End Function

    ' =====================================================================
    ' WORDING
    ' =====================================================================

    ' The column headings, already rotated so column 0 is FirstDayOfWeek.
    Private Function ShortDayNames() As String()
        Dim scurte As String() = _culture.DateTimeFormat.ShortestDayNames
        If scurte Is Nothing OrElse scurte.Length < 7 Then scurte = _culture.DateTimeFormat.AbbreviatedDayNames
        Dim rezultat(6) As String
        For i As Integer = 0 To 6
            Dim idx As Integer = (CInt(_firstDayOfWeek) + i) Mod 7
            rezultat(i) = Capitalize(If(scurte(idx), String.Empty))
        Next
        Return rezultat
    End Function

    ' Romanian month names come out of the culture in lower case; a heading reads better capitalised.
    Private Function Capitalize(s As String) As String
        If String.IsNullOrEmpty(s) Then Return String.Empty
        Return Char.ToUpper(s(0), _culture) & s.Substring(1)
    End Function

    ''' <summary>The header title: month + year, year, or the decade span.</summary>
    Private Function TitleText() As String
        Select Case _view
            Case KBotCalendarView.Days
                Return Capitalize(_culture.DateTimeFormat.GetMonthName(_displayMonth.Month)) & " " &
                       _displayMonth.Year.ToString(CultureInfo.InvariantCulture)
            Case KBotCalendarView.Months
                Return _displayMonth.Year.ToString(CultureInfo.InvariantCulture)
            Case Else
                Dim decada As Integer = (_displayMonth.Year \ 10) * 10
                Return decada.ToString(CultureInfo.InvariantCulture) & " - " &
                       (decada + 9).ToString(CultureInfo.InvariantCulture)
        End Select
    End Function

    ' What a cell says.
    Private Function CellText(index As Integer) As String
        Select Case _view
            Case KBotCalendarView.Days
                Return _cellDate(index).Day.ToString(CultureInfo.InvariantCulture)
            Case KBotCalendarView.Months
                Return Capitalize(_culture.DateTimeFormat.GetAbbreviatedMonthName(_cellDate(index).Month))
            Case Else
                Dim decada As Integer = (_displayMonth.Year \ 10) * 10
                Return (decada - 1 + index).ToString(CultureInfo.InvariantCulture)
        End Select
    End Function

    ' Is this cell the current value?
    Private Function IsSelectedCell(index As Integer) As Boolean
        Select Case _view
            Case KBotCalendarView.Days
                Return _cellDate(index).Date = _value.Date
            Case KBotCalendarView.Months
                Return _cellDate(index).Year = _value.Year AndAlso _cellDate(index).Month = _value.Month
            Case Else
                Return (( _displayMonth.Year \ 10) * 10 - 1 + index) = _value.Year
        End Select
    End Function

    ' Is this cell today?
    Private Function IsTodayCell(index As Integer) As Boolean
        Dim azi As Date = Date.Today
        Select Case _view
            Case KBotCalendarView.Days
                Return _cellDate(index).Date = azi
            Case KBotCalendarView.Months
                Return _cellDate(index).Year = azi.Year AndAlso _cellDate(index).Month = azi.Month
            Case Else
                Return ((_displayMonth.Year \ 10) * 10 - 1 + index) = azi.Year
        End Select
    End Function

    ' =====================================================================
    ' FONTS
    ' =====================================================================

    Private Sub RebuildFonts()
        Try
            Dim vechiTitlu As Font = _titleFont
            Dim vechiMic As Font = _smallFont
            _titleFont = New Font(Font, FontStyle.Bold)
            _smallFont = New Font(Font.FontFamily, Math.Max(6.0F, Font.Size - 0.5F), FontStyle.Regular)
            vechiTitlu?.Dispose()
            vechiMic?.Dispose()
        Catch ex As Exception
            ' A broken font family must not take the paint down: fall back to the ambient font.
            GlobalErrorLog.Write("KBotCalendar.RebuildFonts", ex)
            _titleFont = Nothing
            _smallFont = Nothing
        End Try
    End Sub

    Private ReadOnly Property TitleFont As Font
        Get
            Return If(_titleFont, Font)
        End Get
    End Property

    Private ReadOnly Property SmallFont As Font
        Get
            Return If(_smallFont, Font)
        End Get
    End Property

    ' The corner radius actually used, in scaled px.
    Private Function EffectiveOuterRadius() As Integer
        Dim logic As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logic))
    End Function

    Private Function EffectiveCellRadius() As Integer
        Dim logic As Integer = If(_cellCornerRadius >= 0, _cellCornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logic))
    End Function

    ' =====================================================================
    ' PAINT
    ' =====================================================================

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            EnsureLayout()
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim afara As New Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1))
            If afara.Width <= 0 OrElse afara.Height <= 0 Then Return

            Using cale As GraphicsPath = ThemeShapes.RoundedRect(afara, EffectiveOuterRadius())
                Using b As New SolidBrush(BackColor)
                    g.FillPath(b, cale)
                End Using

                ' The bands are clipped to the rounded outline, otherwise a square header corner
                ' would poke out of a rounded card.
                Dim stare As GraphicsState = g.Save()
                g.SetClip(cale, CombineMode.Intersect)
                PaintHeader(g)
                PaintDayNames(g)
                PaintFooter(g)
                g.Restore(stare)

                PaintCells(g)
                PaintWeekNumbers(g)

                If _borderWidth > 0 Then
                    Using p As New Pen(EffectiveBorderColor, ThemeShapes.ScaleDpi(Me, _borderWidth))
                        g.DrawPath(p, cale)
                    End Using
                End If
            End Using
        Catch ex As Exception
            ' Paint boundary: a throw from here would take the process down.
            GlobalErrorLog.Write("KBotCalendar.OnPaint", ex)
        End Try
    End Sub

    Private Sub PaintHeader(g As Graphics)
        If _headerRect.Height <= 0 Then Return

        Using b As New SolidBrush(EffectiveHeaderBackColor)
            g.FillRectangle(b, _headerRect)
        End Using

        ' The title is a button: it lights up under the pointer, because it is what zooms out.
        If Enabled AndAlso _hot = HitTitle AndAlso _titleRect.Width > 0 Then
            Using b As New SolidBrush(EffectiveHoverColor)
                g.FillRectangle(b, _titleRect)
            End Using
        End If

        TextRenderer.DrawText(g, TitleText(), TitleFont, _titleRect,
                              If(Enabled, EffectiveHeaderForeColor, EffectiveDisabledColor),
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)

        PaintArrow(g, _prevRect, pointingLeft:=True, hot:=(_hot = HitPrev))
        PaintArrow(g, _nextRect, pointingLeft:=False, hot:=(_hot = HitNext))

        Using p As New Pen(EffectiveGridColor)
            g.DrawLine(p, _headerRect.Left, _headerRect.Bottom - 1, _headerRect.Right, _headerRect.Bottom - 1)
        End Using
    End Sub

    ' A chevron drawn from two lines, not a filled triangle: it reads the same at every DPI.
    Private Sub PaintArrow(g As Graphics, area As Rectangle, pointingLeft As Boolean, hot As Boolean)
        If area.Width <= 0 OrElse area.Height <= 0 Then Return
        If Enabled AndAlso hot Then
            Using b As New SolidBrush(EffectiveHoverColor)
                g.FillRectangle(b, area)
            End Using
        End If

        Dim jum As Single = ThemeShapes.ScaleDpi(Me, 4)
        Dim cx As Single = area.Left + area.Width / 2.0F
        Dim cy As Single = area.Top + area.Height / 2.0F
        Using p As New Pen(If(Enabled, EffectiveArrowColor, EffectiveDisabledColor),
                           ThemeShapes.ScaleDpi(Me, 2))
            p.StartCap = LineCap.Round
            p.EndCap = LineCap.Round
            Dim semn As Single = If(pointingLeft, 1.0F, -1.0F)
            g.DrawLine(p, cx + semn * jum / 2.0F, cy - jum, cx - semn * jum / 2.0F, cy)
            g.DrawLine(p, cx - semn * jum / 2.0F, cy, cx + semn * jum / 2.0F, cy + jum)
        End Using
    End Sub

    Private Sub PaintDayNames(g As Graphics)
        If _dayNamesRect.Height <= 0 OrElse _view <> KBotCalendarView.Days Then Return
        Dim nume As String() = ShortDayNames()
        Dim area As New Rectangle(_dayNamesRect.Left + _weekColWidth, _dayNamesRect.Top,
                                  Math.Max(0, _dayNamesRect.Width - _weekColWidth), _dayNamesRect.Height)
        For i As Integer = 0 To 6
            Dim celula As Rectangle = CellRect(area, i, 0, 7, 1)
            Dim ziua As DayOfWeek = CType((CInt(_firstDayOfWeek) + i) Mod 7, DayOfWeek)
            Dim culoare As Color = EffectiveDayNameColor
            If _highlightWeekend AndAlso (ziua = DayOfWeek.Saturday OrElse ziua = DayOfWeek.Sunday) Then
                culoare = EffectiveWeekendForeColor
            End If
            TextRenderer.DrawText(g, nume(i), SmallFont, celula, culoare,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.NoPrefix)
        Next
    End Sub

    Private Sub PaintWeekNumbers(g As Graphics)
        If _weekColWidth <= 0 OrElse _view <> KBotCalendarView.Days OrElse _cellCount < 42 Then Return
        For rand As Integer = 0 To 5
            Dim celula As Rectangle = _cells(rand * 7)
            Dim zona As New Rectangle(_bodyRect.Left, celula.Top, _weekColWidth, celula.Height)
            Dim saptamana As Integer
            Try
                saptamana = ISOWeek.GetWeekOfYear(_cellDate(rand * 7))
            Catch ex As ArgumentOutOfRangeException
                GlobalErrorLog.Write("KBotCalendar.PaintWeekNumbers", ex)
                Continue For
            End Try
            TextRenderer.DrawText(g, saptamana.ToString(CultureInfo.InvariantCulture), SmallFont, zona,
                                  EffectiveWeekNumberColor,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.NoPrefix)
        Next
        Using p As New Pen(EffectiveGridColor)
            g.DrawLine(p, _bodyRect.Left + _weekColWidth, _bodyRect.Top,
                       _bodyRect.Left + _weekColWidth, _bodyRect.Bottom)
        End Using
    End Sub

    Private Sub PaintCells(g As Graphics)
        Dim raza As Integer = EffectiveCellRadius()
        For i As Integer = 0 To _cellCount - 1
            Dim celula As Rectangle = _cells(i)
            If celula.Width <= 1 OrElse celula.Height <= 1 Then Continue For

            Dim ales As Boolean = IsSelectedCell(i)
            Dim activ As Boolean = Enabled AndAlso _cellEnabled(i)
            Dim zona2 As Rectangle = Shrink(celula, ScalePad(_cellPadding))
            If zona2.Width <= 0 OrElse zona2.Height <= 0 Then zona2 = celula

            If ales Then
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona2, raza)
                    ThemeShapes.FillModern(g, cale, zona2, EffectiveSelectionBackColor, _cellGradient)
                End Using
            ElseIf activ AndAlso _hot = i Then
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(zona2, raza)
                    ThemeShapes.FillModern(g, cale, zona2, EffectiveHoverColor, _cellGradient)
                End Using
            End If

            If IsTodayCell(i) AndAlso Not ales Then
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(New Rectangle(zona2.X, zona2.Y,
                                                                                   Math.Max(0, zona2.Width - 1),
                                                                                   Math.Max(0, zona2.Height - 1)), raza)
                    Using p As New Pen(EffectiveTodayColor, ThemeShapes.ScaleDpi(Me, 1))
                        g.DrawPath(p, cale)
                    End Using
                End Using
            End If

            ' The focus ring sits on the value, so a keyboard-only operator can see where they are.
            If ales AndAlso Focused Then
                Dim inel As Rectangle = Rectangle.Inflate(zona2, ThemeShapes.ScaleDpi(Me, 1), ThemeShapes.ScaleDpi(Me, 1))
                inel = New Rectangle(inel.X, inel.Y, Math.Max(0, inel.Width - 1), Math.Max(0, inel.Height - 1))
                Using cale As GraphicsPath = ThemeShapes.RoundedRect(inel, raza)
                    Using p As New Pen(ThemeManager.Current.Palette.FocusRingColor, ThemeShapes.ScaleDpi(Me, 1))
                        g.DrawPath(p, cale)
                    End Using
                End Using
            End If

            TextRenderer.DrawText(g, CellTextOrBlank(i), Font, celula, CellForeColor(i, ales, activ),
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                                  TextFormatFlags.NoPrefix)
        Next
    End Sub

    ' Trailing days can be hidden entirely; everything else always says something.
    Private Function CellTextOrBlank(index As Integer) As String
        If _view = KBotCalendarView.Days AndAlso _cellTrailing(index) AndAlso Not _showTrailingDays Then
            Return String.Empty
        End If
        Return CellText(index)
    End Function

    ' The text colour of one cell: selection wins, then disabled, then trailing, then weekend.
    Private Function CellForeColor(index As Integer, selected As Boolean, usable As Boolean) As Color
        If selected Then Return EffectiveSelectionForeColor
        If Not usable Then Return EffectiveDisabledColor
        If _cellTrailing(index) Then Return EffectiveTrailingForeColor
        If _view = KBotCalendarView.Days AndAlso _highlightWeekend Then
            Dim zi As DayOfWeek = _cellDate(index).DayOfWeek
            If zi = DayOfWeek.Saturday OrElse zi = DayOfWeek.Sunday Then Return EffectiveWeekendForeColor
        End If
        Return ForeColor
    End Function

    Private Sub PaintFooter(g As Graphics)
        If Not _showToday OrElse _footerRect.Height <= 0 Then Return

        If Enabled AndAlso _hot = HitToday Then
            Using b As New SolidBrush(EffectiveHoverColor)
                g.FillRectangle(b, _footerRect)
            End Using
        End If

        Using p As New Pen(EffectiveGridColor)
            g.DrawLine(p, _footerRect.Left, _footerRect.Top, _footerRect.Right, _footerRect.Top)
        End Using

        ' Operator-facing text, therefore Romanian, with its diacritics.
        Dim text As String = "Astăzi: " & Date.Today.ToString(_todayFormat, _culture)
        TextRenderer.DrawText(g, text, Font, If(_footerTextRect.IsEmpty, _footerRect, _footerTextRect),
                              If(Enabled, EffectiveFooterForeColor, EffectiveDisabledColor),
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
                              TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                _titleFont?.Dispose()
                _smallFont?.Dispose()
                _titleFont = Nothing
                _smallFont = Nothing
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotCalendar.Dispose", ex)
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

End Class
