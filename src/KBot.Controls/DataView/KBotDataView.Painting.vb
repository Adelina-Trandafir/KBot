Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Partea de PICTARE a <see cref="KBotDataView"/>. Boundary UI: <c>OnPaint</c> prinde tot,
''' loghează și ÎNGHITE (un throw dintr-un corp de pictare ar prăbuși procesul). Ajutoarele
''' de desen de mai jos sunt acoperite TRANZITIV de acel boundary — nu-și pun Try propriu
''' (regula casei: altfel s-ar loga o dată pe fiecare nivel).
''' </summary>
Partial Class KBotDataView

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics

            ' Recalcul pur (nu atinge controale) — garantează coerența cu starea curentă.
            RecalcColumnLayout()

            g.FillRectangle(_bRowBack, ClientRectangle)

            DrawRows(g)
            If _showTotalsRow AndAlso TotalsBandHeight() > 0 Then DrawTotalsRow(g)
            If _showHeader AndAlso _headerHeight > 0 Then DrawHeader(g)

            g.DrawRectangle(_pBorder, New Rectangle(0, 0, Width - 1, Height - 1))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnPaint", ex)
        End Try
    End Sub

    ' ── Antet ───────────────────────────────────────────────────────────────────

    ' Banda de antet: fundal, textul coloanelor (înghețate + derulate), separatoare, bază.
    Private Sub DrawHeader(g As Graphics)
        Dim headerRect As New Rectangle(0, 0, ClientSize.Width, _headerHeight)
        g.FillRectangle(_bHeaderBack, headerRect)

        Dim hf As Font = HeaderFont()
        Dim viewW As Integer = ViewportWidth()

        ' Banda derulată — decupată ca să nu treacă peste coloanele înghețate.
        Dim scrollClip As New Rectangle(_frozenBandWidth, 0,
                                        Math.Max(0, viewW - _frozenBandWidth), _headerHeight)
        g.SetClip(scrollClip)
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            DrawHeaderCell(g, cl.Column, _frozenBandWidth + cl.X - hOffset, hf)
        Next
        g.ResetClip()

        ' Banda înghețată — desenată PESTE cea derulată.
        ' English: repaint the frozen header band opaquely first, so an H-scrolled header cell
        ' can never bleed under the static column header (the frozen band is always on top).
        If _frozenBandWidth > 0 Then
            g.FillRectangle(_bHeaderBack, New Rectangle(0, 0, _frozenBandWidth, _headerHeight))
        End If
        For Each cl In _frozenLayout
            DrawHeaderCell(g, cl.Column, cl.X, hf)
        Next

        ' Linia de bază + accentul de sub antet.
        g.DrawLine(_pHeaderSep, 0, _headerHeight - 1, ClientSize.Width - 1, _headerHeight - 1)
        g.DrawLine(_pHeaderBaseline, 0, _headerHeight - 1, ClientSize.Width - 1, _headerHeight - 1)
    End Sub

    Private Sub DrawHeaderCell(g As Graphics, col As KBotDataColumn, x As Integer, hf As Font)
        Dim cellRect As New Rectangle(x, 0, col.Width, _headerHeight)
        If cellRect.Right < 0 OrElse cellRect.Left > ClientSize.Width Then Return

        Dim padX As Integer = ScaleDpi(8)
        Dim textRect As New Rectangle(cellRect.Left + padX, cellRect.Top,
                                      Math.Max(0, cellRect.Width - 2 * padX), cellRect.Height)
        TextRenderer.DrawText(g, col.HeaderText, hf, textRect, _cHeaderText,
            HorizontalFlags(col.TextAlign) Or TextFormatFlags.VerticalCenter Or
            TextFormatFlags.EndEllipsis)

        Dim sepX As Integer = cellRect.Right - 1
        g.DrawLine(_pHeaderSep, sepX, 0, sepX, _headerHeight - 1)
    End Sub

    ' ── Bandă de totaluri (slice 0017-01) ────────────────────────────────────────

    ' English: the pinned totals band, drawn between the body and the horizontal scrollbar. It
    ' reuses the header's band styling (same fill / separators / baseline reads — no literals),
    ' and mirrors the header's frozen-over-scroll layering so a totals cell always sits under its
    ' column, including with ScrollByColumn engaged. Not a row: no selection, no hit-testing.
    Private Sub DrawTotalsRow(g As Graphics)
        Dim bandH As Integer = TotalsBandHeight()
        Dim bandTop As Integer = HeaderBandHeight() + ViewportHeight()
        Dim bandRect As New Rectangle(0, bandTop, ClientSize.Width, bandH)
        g.FillRectangle(_bHeaderBack, bandRect)

        Dim tf As Font = HeaderFont()
        Dim viewW As Integer = ViewportWidth()

        ' Scroll band — clipped so it cannot bleed under the frozen columns.
        Dim scrollClip As New Rectangle(_frozenBandWidth, bandTop,
                                        Math.Max(0, viewW - _frozenBandWidth), bandH)
        g.SetClip(scrollClip)
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            DrawTotalsCell(g, cl.Column, _frozenBandWidth + cl.X - hOffset, bandTop, bandH, tf)
        Next
        g.ResetClip()

        ' Frozen band — opaque repaint, then its cells, drawn PESTE the scroll band.
        If _frozenBandWidth > 0 Then
            g.FillRectangle(_bHeaderBack, New Rectangle(0, bandTop, _frozenBandWidth, bandH))
        End If
        For Each cl In _frozenLayout
            DrawTotalsCell(g, cl.Column, cl.X, bandTop, bandH, tf)
        Next

        ' Separating rule + accent along the TOP edge (between body and totals), mirroring the
        ' header's baseline reads.
        g.DrawLine(_pHeaderSep, 0, bandTop, ClientSize.Width - 1, bandTop)
        g.DrawLine(_pHeaderBaseline, 0, bandTop, ClientSize.Width - 1, bandTop)
    End Sub

    Private Sub DrawTotalsCell(g As Graphics, col As KBotDataColumn, x As Integer,
                               bandTop As Integer, bandH As Integer, tf As Font)
        Dim cellRect As New Rectangle(x, bandTop, col.Width, bandH)
        If cellRect.Right < 0 OrElse cellRect.Left > ClientSize.Width Then Return

        Dim text As String = TotalsTextFor(col)
        Dim padX As Integer = ScaleDpi(8)
        Dim textRect As New Rectangle(cellRect.Left + padX, cellRect.Top,
                                      Math.Max(0, cellRect.Width - 2 * padX), cellRect.Height)
        TextRenderer.DrawText(g, text, tf, textRect, _cHeaderText,
            HorizontalFlags(col.TextAlign) Or TextFormatFlags.VerticalCenter Or
            TextFormatFlags.EndEllipsis)

        Dim sepX As Integer = cellRect.Right - 1
        g.DrawLine(_pHeaderSep, sepX, bandTop, sepX, bandTop + bandH - 1)
    End Sub

    ' ── Rânduri (virtualizat) ───────────────────────────────────────────────────

    ' Pictează DOAR rândurile vizibile. Numărul lor ajunge în DebugLastPaintedDataRows,
    ' poarta de verificare headless a virtualizării.
    Private Sub DrawRows(g As Graphics)
        Dim painted As Integer = 0
        Dim bodyTop As Integer = HeaderBandHeight()
        Dim bodyH As Integer = ViewportHeight()
        Dim viewW As Integer = ViewportWidth()

        If bodyH <= 0 OrElse _rows.Count = 0 Then
            DebugLastPaintedDataRows = 0
            Return
        End If

        Dim first As Integer = FirstVisibleRow()
        Dim last As Integer = LastVisibleRow()

        ' Decupăm zona de date, ca rândurile parțiale să nu deseneze peste antet/bare.
        Dim bodyClip As New Rectangle(0, bodyTop, viewW, bodyH)
        g.SetClip(bodyClip)

        For i As Integer = first To last
            Dim y As Integer = RowTop(i)
            If y + _rowHeight <= bodyTop OrElse y >= bodyTop + bodyH Then Continue For
            DrawRow(g, i, y, viewW)
            painted += 1
        Next

        g.ResetClip()
        DebugLastPaintedDataRows = painted
    End Sub

    Private Sub DrawRow(g As Graphics, rowIndex As Integer, y As Integer, viewW As Integer)
        Dim row As KBotDataRow = _rows(rowIndex)
        Dim isAlt As Boolean = _alternatingRows AndAlso (rowIndex Mod 2 = 1)
        Dim isSelected As Boolean = (rowIndex = _currentRowIndex)

        ' Fundalul implicit al rândului: normal / alternant, iar dacă e selectat, spălarea
        ' de accent peste fundalul REAL (două variante precalculate => zero alocări aici).
        Dim backBrush As SolidBrush
        If isSelected Then
            backBrush = If(isAlt, _bSelAltBack, _bSelBack)
        Else
            backBrush = If(isAlt, _bRowAltBack, _bRowBack)
        End If
        Dim backColor As Color = backBrush.Color
        Dim foreColor As Color = If(isSelected, _cSelText, _cCellText)

        ' RowFormatting — argumente REFOLOSITE (fără alocări la mii de rânduri).
        _rowArgs.Reset(rowIndex, row, backColor, foreColor, row.Enabled)
        RaiseEvent RowFormatting(Me, _rowArgs)
        backColor = _rowArgs.BackColor
        foreColor = _rowArgs.ForeColor
        Dim rowEnabled As Boolean = _rowArgs.Enabled

        Dim rowRect As New Rectangle(0, y, viewW, _rowHeight)
        If backColor = backBrush.Color Then
            g.FillRectangle(backBrush, rowRect)          ' calea rapidă: pensulă cache-uită
        Else
            Using b As New SolidBrush(backColor)          ' doar când handler-ul a suprascris
                g.FillRectangle(b, rowRect)
            End Using
        End If

        ' Banda derulată (decupată), apoi cea înghețată desenată peste ea.
        Dim scrollClip As New Rectangle(_frozenBandWidth, y,
                                        Math.Max(0, viewW - _frozenBandWidth), _rowHeight)
        Dim previousClip As Region = g.Clip
        g.SetClip(scrollClip, CombineMode.Intersect)
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            DrawCell(g, cl.Column, row, rowIndex,
                     New Rectangle(_frozenBandWidth + cl.X - hOffset, y, cl.Column.Width, _rowHeight),
                     backColor, foreColor, rowEnabled)
        Next
        g.Clip = previousClip
        previousClip.Dispose()

        ' English: repaint the frozen band opaquely before its cells, so an H-scrolled scroll
        ' cell can never bleed under the static column — the frozen column is always on top,
        ' regardless of the scroll-band clip above. Uses the row background (custom per-cell
        ' backgrounds on frozen cells are re-applied inside DrawCell below).
        If _frozenBandWidth > 0 Then
            Dim frozenRect As New Rectangle(0, y, _frozenBandWidth, _rowHeight)
            If backColor = backBrush.Color Then
                g.FillRectangle(backBrush, frozenRect)
            Else
                Using b As New SolidBrush(backColor)
                    g.FillRectangle(b, frozenRect)
                End Using
            End If
        End If

        For Each cl In _frozenLayout
            DrawCell(g, cl.Column, row, rowIndex,
                     New Rectangle(cl.X, y, cl.Column.Width, _rowHeight),
                     backColor, foreColor, rowEnabled)
        Next

        ' Linia orizontală de grilă, sub rând.
        g.DrawLine(_pGridLine, 0, y + _rowHeight - 1, viewW, y + _rowHeight - 1)
    End Sub

    ' ── Celule ──────────────────────────────────────────────────────────────────

    Private Sub DrawCell(g As Graphics, col As KBotDataColumn, row As KBotDataRow, rowIndex As Integer,
                         cellRect As Rectangle, rowBack As Color, rowFore As Color, rowEnabled As Boolean)
        If cellRect.Right < 0 OrElse cellRect.Left > ClientSize.Width Then Return

        Dim value As Object = row(col.Key)

        ' CellFormatting — argumente REFOLOSITE, pre-umplute cu valorile implicite din temă.
        _cellArgs.Reset(col, row, rowIndex, value, FormatValue(value, col),
                        rowBack, rowFore, Font, col.TextAlign,
                        col.Enabled AndAlso rowEnabled)
        RaiseEvent CellFormatting(Me, _cellArgs)

        Dim enabled As Boolean = _cellArgs.Enabled
        Dim customBack As Boolean = (_cellArgs.BackColor <> rowBack)

        ' Fundal per-celulă doar dacă handler-ul l-a schimbat față de cel al rândului.
        If customBack Then
            Using b As New SolidBrush(_cellArgs.BackColor)
                g.FillRectangle(b, cellRect)
            End Using
        ElseIf Not enabled Then
            ' Spălarea de „dezactivat” se aplică doar când nimeni n-a impus alt fundal —
            ' altfel am călca peste o regulă de formatare condiționată a caller-ului.
            g.FillRectangle(_bDisabledWash, cellRect)
        End If

        ' Textul dezactivat trece pe culoarea ștearsă, indiferent ce a cerut handler-ul:
        ' „inert” trebuie să se și VADĂ inert.
        Dim fore As Color = If(enabled, _cellArgs.ForeColor, _cDisabledText)

        Select Case col.ColumnType
            Case KBotColumnType.CheckBox
                DrawCheckCell(g, cellRect, ToBool(value), enabled)
            Case KBotColumnType.OptionButton
                DrawOptionCell(g, cellRect, ToBool(value), enabled)
            Case KBotColumnType.Button
                ' Butonul nu ține valoare: eticheta e textul celulei, iar dacă lipsește,
                ' antetul coloanei (ex. o coloană «Detalii» cu același buton pe fiecare rând).
                Dim caption As String = If(String.IsNullOrEmpty(_cellArgs.Text), col.HeaderText, _cellArgs.Text)
                DrawButtonCell(g, cellRect, caption, _cellArgs.Font, enabled)
            Case KBotColumnType.ProgressBar
                DrawProgressCell(g, cellRect, ProgressFraction(value, col), enabled)
            Case KBotColumnType.Combo
                DrawComboCell(g, cellRect, _cellArgs.Text, _cellArgs.Font,
                              fore, _cellArgs.Alignment, enabled)
            Case Else
                DrawTextCell(g, cellRect, _cellArgs.Text, _cellArgs.Font,
                             fore, _cellArgs.Alignment)
        End Select

        ' Separatorul vertical de grilă, la marginea dreaptă a celulei.
        g.DrawLine(_pGridLine, cellRect.Right - 1, cellRect.Top, cellRect.Right - 1, cellRect.Bottom - 1)
    End Sub

    Private Sub DrawTextCell(g As Graphics, cellRect As Rectangle, text As String, font As Font,
                             fore As Color, align As ContentAlignment)
        If String.IsNullOrEmpty(text) Then Return
        Dim padX As Integer = ScaleDpi(6)
        Dim textRect As New Rectangle(cellRect.Left + padX, cellRect.Top,
                                      Math.Max(0, cellRect.Width - 2 * padX), cellRect.Height)
        TextRenderer.DrawText(g, text, font, textRect, fore,
            HorizontalFlags(align) Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub

    ' Bifă centrată. Geometria e cea din AdvancedTreeControl (dreptunghi rotunjit + bifă),
    ' dar culorile vin din paletă, nu hardcodate ca acolo (acel control e deliberat ne-tematizat).
    Private Sub DrawCheckCell(g As Graphics, cellRect As Rectangle, checked As Boolean, enabled As Boolean)
        Dim size As Integer = ScaleDpi(14)
        Dim box As New Rectangle(cellRect.Left + (cellRect.Width - size) \ 2,
                                 cellRect.Top + (cellRect.Height - size) \ 2,
                                 size, size)

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias

        Using path As GraphicsPath = RoundedRect(box, ScaleDpi(3))
            If checked Then
                g.FillPath(If(enabled, _bCheckFill, _bDisabledMark), path)
                g.DrawPath(If(enabled, _pCheckFill, _pDisabledMark), path)
                Using penTick As New Pen(_cCheckMark, 2.0F)
                    penTick.StartCap = LineCap.Round
                    penTick.EndCap = LineCap.Round
                    penTick.LineJoin = LineJoin.Round
                    g.DrawLines(penTick, {
                        New PointF(box.X + size * 0.22F, box.Y + size * 0.52F),
                        New PointF(box.X + size * 0.42F, box.Y + size * 0.72F),
                        New PointF(box.X + size * 0.78F, box.Y + size * 0.28F)
                    })
                End Using
            Else
                g.DrawPath(If(enabled, _pCheckBorder, _pDisabledMark), path)
            End If
        End Using

        g.SmoothingMode = oldSmooth
    End Sub

    ' Buton radio centrat: elipsă + punct central (geometria din AdvancedTreeControl,
    ' culorile din paletă).
    Private Sub DrawOptionCell(g As Graphics, cellRect As Rectangle, selected As Boolean, enabled As Boolean)
        Dim size As Integer = ScaleDpi(14)
        Dim box As New Rectangle(cellRect.Left + (cellRect.Width - size) \ 2,
                                 cellRect.Top + (cellRect.Height - size) \ 2,
                                 size, size)

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias

        If selected Then
            g.FillEllipse(If(enabled, _bOptionFill, _bDisabledMark), box)
            g.DrawEllipse(If(enabled, _pOptionFill, _pDisabledMark), box)
            Dim dotMargin As Integer = CInt(size * 0.28F)
            Dim dot As New Rectangle(box.X + dotMargin, box.Y + dotMargin,
                                     size - dotMargin * 2, size - dotMargin * 2)
            g.FillEllipse(_bOptionDot, dot)
        Else
            g.DrawEllipse(If(enabled, _pOptionBorder, _pDisabledMark), box)
        End If

        g.SmoothingMode = oldSmooth
    End Sub

    ' Buton de acțiune: față rotunjită + chenar + etichetă centrată. Stările hover/pressed
    ' vin în 0010-05, odată cu urmărirea mouse-ului.
    Private Sub DrawButtonCell(g As Graphics, cellRect As Rectangle, caption As String, font As Font,
                               enabled As Boolean)
        Dim marginX As Integer = ScaleDpi(4)
        Dim marginY As Integer = ScaleDpi(3)
        Dim face As New Rectangle(cellRect.Left + marginX, cellRect.Top + marginY,
                                  Math.Max(0, cellRect.Width - 2 * marginX),
                                  Math.Max(0, cellRect.Height - 2 * marginY))
        If face.Width <= 0 OrElse face.Height <= 0 Then Return

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias

        Using path As GraphicsPath = RoundedRect(face, ScaleDpi(3))
            g.FillPath(_bButtonFace, path)
            g.DrawPath(If(enabled, _pButtonBorder, _pDisabledMark), path)
        End Using

        g.SmoothingMode = oldSmooth

        TextRenderer.DrawText(g, caption, font, face, If(enabled, _cButtonText, _cDisabledText),
            TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or
            TextFormatFlags.EndEllipsis)
    End Sub

    ' Bară de progres: șină + umplere proporțională. fraction e deja limitat la 0..1.
    Private Sub DrawProgressCell(g As Graphics, cellRect As Rectangle, fraction As Double,
                                 enabled As Boolean)
        Dim marginX As Integer = ScaleDpi(6)
        Dim barH As Integer = ScaleDpi(10)
        Dim track As New Rectangle(cellRect.Left + marginX,
                                   cellRect.Top + (cellRect.Height - barH) \ 2,
                                   Math.Max(0, cellRect.Width - 2 * marginX), barH)
        If track.Width <= 0 OrElse track.Height <= 0 Then Return

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias

        Dim radius As Integer = track.Height \ 2
        Using path As GraphicsPath = RoundedRect(track, radius)
            g.FillPath(_bProgressTrack, path)
        End Using

        Dim fillW As Integer = CInt(track.Width * fraction)
        If fillW > 0 Then
            Dim fill As New Rectangle(track.X, track.Y, fillW, track.Height)
            Dim fillBrush As SolidBrush = If(enabled, _bProgressFill, _bDisabledMark)
            ' Sub o lățime egală cu înălțimea, colțul rotunjit degenerează — umplem drept.
            If fillW >= track.Height Then
                Using path As GraphicsPath = RoundedRect(fill, radius)
                    g.FillPath(fillBrush, path)
                End Using
            Else
                g.FillRectangle(fillBrush, fill)
            End If
        End If

        g.SmoothingMode = oldSmooth
    End Sub

    ' Combo în stare de AFIȘARE: textul formatat + un chevron în dreapta. Editorul real
    ' (ComboBox flotant) apare doar la editare (0010-06).
    Private Sub DrawComboCell(g As Graphics, cellRect As Rectangle, text As String, font As Font,
                              fore As Color, align As ContentAlignment, enabled As Boolean)
        Dim chevronZone As Integer = ScaleDpi(16)
        Dim textRect As New Rectangle(cellRect.Left, cellRect.Top,
                                      Math.Max(0, cellRect.Width - chevronZone), cellRect.Height)
        DrawTextCell(g, textRect, text, font, fore, align)

        Dim cx As Integer = cellRect.Right - ScaleDpi(9)
        Dim cy As Integer = cellRect.Top + cellRect.Height \ 2
        Dim s As Integer = ScaleDpi(3)

        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.FillPolygon(If(enabled, _bComboChevron, _bDisabledMark), New Point() {
            New Point(cx - s, cy - s \ 2),
            New Point(cx + s, cy - s \ 2),
            New Point(cx, cy + s)
        })
        g.SmoothingMode = oldSmooth
    End Sub

    ' ── Ajutoare ────────────────────────────────────────────────────────────────

    ''' <summary>Fracția (0..1) a unei valori de progres față de ProgressMin/ProgressMax.</summary>
    Friend Shared Function ProgressFraction(value As Object, col As KBotDataColumn) As Double
        Dim span As Double = col.ProgressMax - col.ProgressMin
        If span <= 0 Then Return 0
        Dim v As Double = ToDouble(value)
        Return Math.Max(0.0, Math.Min(1.0, (v - col.ProgressMin) / span))
    End Function

    ' Coerciție tolerantă la Double (fără excepții).
    Private Shared Function ToDouble(value As Object) As Double
        If value Is Nothing Then Return 0
        If TypeOf value Is Double Then Return CDbl(value)
        Dim d As Double
        If Double.TryParse(value.ToString().Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, d) Then Return d
        Return 0
    End Function

    ''' <summary>Valoarea formatată pentru afișare (aplică <c>Column.FormatString</c>).</summary>
    Private Shared Function FormatValue(value As Object, col As KBotDataColumn) As String
        If value Is Nothing Then Return String.Empty
        If Not String.IsNullOrEmpty(col.FormatString) Then
            Dim f As IFormattable = TryCast(value, IFormattable)
            If f IsNot Nothing Then Return f.ToString(col.FormatString, CultureInfo.CurrentCulture)
        End If
        Return value.ToString()
    End Function

    ' Coerciție tolerantă la Boolean pentru celulele de tip bifă (fără excepții).
    Private Shared Function ToBool(value As Object) As Boolean
        If value Is Nothing Then Return False
        If TypeOf value Is Boolean Then Return CBool(value)
        Dim s As String = value.ToString().Trim()
        Dim b As Boolean
        If Boolean.TryParse(s, b) Then Return b
        Dim d As Double
        If Double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, d) Then Return d <> 0
        Return False
    End Function

    ' Alinierea orizontală a textului dintr-un ContentAlignment.
    Private Shared Function HorizontalFlags(align As ContentAlignment) As TextFormatFlags
        Select Case align
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                Return TextFormatFlags.Right
            Case ContentAlignment.TopCenter, ContentAlignment.MiddleCenter, ContentAlignment.BottomCenter
                Return TextFormatFlags.HorizontalCenter
            Case Else
                Return TextFormatFlags.Left
        End Select
    End Function

End Class
