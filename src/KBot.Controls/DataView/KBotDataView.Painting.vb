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

            ' Strâns pe verticală, corpul nu se mai desenează deloc: rămân doar cele două benzi
            ' (vezi partiala .Collapse). Rândurile nu sunt „ascunse prin decupare”, ci sărite —
            ' altfel virtualizarea ar continua să măsoare și să picteze sub bandă.
            If BodyIsCollapsed() Then
                DebugLastPaintedDataRows = 0
            Else
                DrawRows(g)
            End If
            If _showFooter AndAlso FooterBandHeight() > 0 Then DrawFooterBand(g)
            If _showHeader Then DrawHeader(g)

            g.DrawRectangle(_pBorder, New Rectangle(0, 0, Width - 1, Height - 1))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnPaint", ex)
        End Try
    End Sub

    ' ── Antet ───────────────────────────────────────────────────────────────────

    ' Banda de antet: fundal, textul coloanelor (înghețate + derulate), separatoare, bază.
    Private Sub DrawHeader(g As Graphics)
        ' Aceeași înălțime pe care o folosesc geometria și hit-testul (vezi partiala .Layout):
        ' o bandă desenată mai înaltă decât cea socotită ar acoperi primele rânduri.
        Dim headerH As Integer = HeaderBandHeight()
        If headerH <= 0 Then Return

        Dim headerRect As New Rectangle(0, 0, ClientSize.Width, headerH)
        FillBand(g, headerRect, _bHeaderBack, _cHeaderGradientEnd)

        Dim viewW As Integer = ViewportWidth()
        Dim hOffset As Integer = HScrollOffset()

        ' Banda derulată — decupată ca să nu treacă peste coloanele înghețate.
        Dim scrollClip As New Rectangle(_frozenBandWidth, 0,
                                        Math.Max(0, viewW - _frozenBandWidth), headerH)
        g.SetClip(scrollClip)
        For Each cl In _scrollLayout
            DrawHeaderCell(g, cl.Column, _frozenBandWidth + cl.X - hOffset, headerH)
        Next
        g.ResetClip()

        ' Banda înghețată — desenată PESTE cea derulată.
        ' English: repaint the frozen header band opaquely first, so an H-scrolled header cell
        ' can never bleed under the static column header (the frozen band is always on top).
        If _frozenBandWidth > 0 Then
            FillBand(g, New Rectangle(0, 0, _frozenBandWidth, headerH), _bHeaderBack, _cHeaderGradientEnd)
        End If
        For Each cl In _frozenLayout
            DrawHeaderCell(g, cl.Column, cl.X, headerH)
        Next

        ' Linia de bază + accentul de sub antet.
        g.DrawLine(_pHeaderSep, 0, headerH - 1, ClientSize.Width - 1, headerH - 1)
        g.DrawLine(_pHeaderBaseline, 0, headerH - 1, ClientSize.Width - 1, headerH - 1)
    End Sub

    ' Titlul + perechea de pictograme (slice 0028-02). Așezarea vine din partiala .HeaderIcons,
    ' aceeași funcție pe care o folosește hit-testul — desenul și apăsarea nu au voie să difere.
    '
    ' Fontul se cere PE COLOANĂ, nu o dată pe bandă: fiecare coloană poate purta al ei
    ' (KBotDataColumn.HeaderFont), iar banda e doar ce se folosește când ea n-a cerut nimic.
    Private Sub DrawHeaderCell(g As Graphics, col As KBotDataColumn, x As Integer, headerH As Integer)
        Dim cellRect As New Rectangle(x, 0, col.WidthPx, headerH)
        If cellRect.Right < 0 OrElse cellRect.Left > ClientSize.Width Then Return

        Dim textRect As Rectangle = DrawHeaderIcons(g, col, cellRect)
        If textRect.Width > 0 Then
            TextRenderer.DrawText(g, col.HeaderText, HeaderFontFor(col), HeaderTextRect(col, textRect),
                                  HeaderForeResolved(), HeaderTextFlags(col))
        End If

        Dim sepX As Integer = cellRect.Right - 1
        g.DrawLine(_pHeaderSep, sepX, 0, sepX, headerH - 1)
    End Sub

    ''' <summary>
    ''' Titlul coloanei ajunge pe mai multe rânduri? Două motive independente, și AMÂNDOUĂ contează:
    ''' <see cref="KBotDataColumn.MultiLine"/> (rupere automată între cuvinte, la lățimea coloanei)
    ''' și o ruptură SCRISĂ cu Enter în text.
    '''
    ''' <para>Ruptura scrisă se respectă chiar și fără <c>MultiLine</c>, și nu din îngăduință:
    ''' <c>DrawText</c> o desenează oricum, cât timp nu i se cere <c>SingleLine</c>. Cât timp banda
    ''' nu creștea decât pentru <c>MultiLine</c>, un titlu cu Enter în el se picta pe două rânduri
    ''' într-o bandă de un rând — adică al doilea rând dispărea cu totul sub linia de bază. Ori se
    ''' respectă și se face loc pentru ea, ori nu se desenează deloc; jumătatea era o dispariție
    ''' tăcută. Aici se alege prima variantă: cine a apăsat Enter în titlu asta a cerut.</para>
    ''' </summary>
    Friend Shared Function HeaderIsMultiLine(col As KBotDataColumn) As Boolean
        If col Is Nothing Then Return False
        If col.MultiLine Then Return True
        Return HasHardBreak(col.HeaderText)
    End Function

    Private Shared Function HasHardBreak(text As String) As Boolean
        If String.IsNullOrEmpty(text) Then Return False
        Return text.IndexOf(ControlChars.Lf) >= 0 OrElse text.IndexOf(ControlChars.Cr) >= 0
    End Function

    ''' <summary>
    ''' Steagurile cu care se scrie titlul unei coloane. UNA singură pentru desen ȘI pentru
    ''' măsurarea benzii (<c>MeasureHeaderTextHeight</c>) — două formule ar da o bandă înaltă cât
    ''' patru rânduri și un text rupt în trei, sau invers, un rând tăiat sub linia de bază.
    '''
    ''' <para>Pe mai multe rânduri NU se cere <c>VerticalCenter</c>: în Win32 el merge doar
    ''' împreună cu <c>SingleLine</c>, iar textul ar rămâne lipit de marginea de sus. Centrarea o
    ''' face <see cref="HeaderTextRect"/>, mutând dreptunghiul. <c>EndEllipsis</c> rămâne cerut și
    ''' acolo, ca supapă: un cuvânt mai lat decât coloana, sau un titlu tăiat de
    ''' <c>MaxHeaderHeight</c>, se termină cu trei puncte, nu retezat la mijlocul literelor.</para>
    '''
    ''' <para><c>WordBreak</c> se cere doar pentru <c>MultiLine</c>: un titlu care are DOAR o
    ''' ruptură scrisă cu Enter se rupe exact acolo unde a cerut operatorul și nicăieri altundeva.
    ''' Pe un singur rând se cere <c>SingleLine</c> — el e ce face <c>VerticalCenter</c> să
    ''' funcționeze cu adevărat.</para>
    ''' </summary>
    Private Shared Function HeaderTextFlags(col As KBotDataColumn) As TextFormatFlags
        Dim flags As TextFormatFlags = HorizontalFlags(col.HeaderTextAlign) Or TextFormatFlags.EndEllipsis
        If Not HeaderIsMultiLine(col) Then Return flags Or TextFormatFlags.SingleLine Or TextFormatFlags.VerticalCenter
        If col.MultiLine Then flags = flags Or TextFormatFlags.WordBreak
        Return flags
    End Function

    ''' <summary>
    ''' Dreptunghiul în care se scrie efectiv titlul. Pe o singură linie e chiar cel primit
    ''' (centrarea o face steagul); pe mai multe linii se măsoară blocul de text și se AȘAZĂ
    ''' centrat pe verticală în celulă — vezi <see cref="HeaderTextFlags"/> pentru de ce nu poate
    ''' face steagul asta.
    ''' </summary>
    Private Function HeaderTextRect(col As KBotDataColumn, textRect As Rectangle) As Rectangle
        If Not HeaderIsMultiLine(col) Then Return textRect
        Dim h As Integer = TextRenderer.MeasureText(col.HeaderText, HeaderFontFor(col),
                                                    New Size(textRect.Width, Integer.MaxValue),
                                                    HeaderTextFlags(col)).Height
        If h <= 0 OrElse h >= textRect.Height Then Return textRect
        Return New Rectangle(textRect.Left, textRect.Top + (textRect.Height - h) \ 2, textRect.Width, h)
    End Function

    ' ── Banda de subsol (slice 0017-01; separatoare + buton în 0028) ──────────

    ''' <summary>
    ''' Banda fixată de subsol, între corp și bara orizontală de derulare. Oglindește stratificarea
    ''' antetului (înghețat PESTE derulat), deci o celulă de subsol stă întotdeauna sub coloana ei,
    ''' inclusiv cu <c>ScrollByColumn</c> pornit. Nu e un rând: fără selecție, fără hit-testing.
    '''
    ''' <para><b>Liniile verticale.</b> Spre deosebire de antet, subsolul NU desparte toate
    ''' coloanele: separatorul se desenează doar în jurul celor AGREGATE (slice 0028). Un șir de
    ''' cutii goale despărțite cu linii arată ca niște totaluri care lipsesc; fără linii, banda
    ''' arată ce este — câteva valori, sub coloanele lor.</para>
    ''' </summary>
    Private Sub DrawFooterBand(g As Graphics)
        Dim bandH As Integer = FooterBandHeight()
        Dim bandTop As Integer = FooterBandTop()
        Dim bandRect As New Rectangle(0, bandTop, ClientSize.Width, bandH)
        FillBand(g, bandRect, _bFooterBack, _cFooterGradientEnd)

        Dim tf As Font = ResolvedFooterFont()
        Dim viewW As Integer = ViewportWidth()

        ' Butonul de strângere își ia latura lui din bandă: textul agregatelor se decupează
        ' înaintea lui, ca o sumă lungă să nu curgă pe sub buton. Coloanele NU se re-așază —
        ' X-urile rămân cele din antet, altfel subsolul s-ar desprinde de coloanele lui.
        Dim contentRect As Rectangle = FooterContentRect(bandRect)

        ' Banda derulată — decupată ca să nu treacă peste coloanele înghețate.
        Dim scrollLeft As Integer = Math.Max(_frozenBandWidth, contentRect.Left)
        Dim scrollClip As New Rectangle(scrollLeft, bandTop,
                                        Math.Max(0, Math.Min(viewW, contentRect.Right) - scrollLeft), bandH)
        g.SetClip(scrollClip)
        Dim hOffset As Integer = HScrollOffset()
        For i As Integer = 0 To _scrollLayout.Count - 1
            Dim cl As ColLayout = _scrollLayout(i)
            ' Vecinul din STÂNGA celei dintâi coloane derulate e ultima coloană înghețată.
            Dim stanga As KBotDataColumn = If(i > 0, _scrollLayout(i - 1).Column,
                                              If(_frozenLayout.Count > 0, _frozenLayout(_frozenLayout.Count - 1).Column, Nothing))
            DrawFooterCell(g, cl.Column, stanga, _frozenBandWidth + cl.X - hOffset, bandTop, bandH, tf)
        Next
        g.ResetClip()

        ' Banda înghețată — repictare opacă, apoi celulele ei, desenate PESTE cea derulată.
        If _frozenBandWidth > 0 Then
            FillBand(g, New Rectangle(0, bandTop, _frozenBandWidth, bandH), _bFooterBack, _cFooterGradientEnd)
        End If
        Dim frozenClip As New Rectangle(contentRect.Left, bandTop,
                                        Math.Max(0, Math.Min(_frozenBandWidth, contentRect.Right) - contentRect.Left), bandH)
        g.SetClip(frozenClip)
        For i As Integer = 0 To _frozenLayout.Count - 1
            Dim cl As ColLayout = _frozenLayout(i)
            Dim stanga As KBotDataColumn = If(i > 0, _frozenLayout(i - 1).Column, Nothing)
            DrawFooterCell(g, cl.Column, stanga, cl.X, bandTop, bandH, tf)
        Next
        g.ResetClip()

        ' Titlul din stânga (slice 0028-02) — după celule, ca să stea peste banda deja umplută, dar
        ' înaintea liniilor: zona lui se oprește oricum la prima coloană agregată.
        DrawFooterCaption(g, bandRect, tf)

        ' Linia de despărțire + accentul pe muchia de SUS (între corp și subsol), perechea
        ' liniei de sub antet.
        g.DrawLine(_pFooterSep, 0, bandTop, ClientSize.Width - 1, bandTop)
        g.DrawLine(_pFooterBaseline, 0, bandTop, ClientSize.Width - 1, bandTop)

        ' Butonul de strângere, ultimul: stă PESTE bandă, în colțul care îi aparține.
        Dim butonRect As Rectangle = ComputeCollapseButtonRect(bandRect)
        If Not butonRect.IsEmpty Then DrawCollapseButton(g, butonRect)
    End Sub

    ' O celulă de subsol. «stanga» = coloana vecină din stânga (Nothing = nu există): din ea se
    ' deduce dacă muchia stângă a unei coloane agregate a fost deja desenată de vecin.
    Private Sub DrawFooterCell(g As Graphics, col As KBotDataColumn, stanga As KBotDataColumn,
                               x As Integer, bandTop As Integer, bandH As Integer, tf As Font)
        Dim cellRect As New Rectangle(x, bandTop, col.WidthPx, bandH)

        If cellRect.Right < 0 OrElse cellRect.Left > ClientSize.Width Then Return

        Dim agregata As Boolean = col.Aggregate <> KBotAggregate.None
        Dim contentRect As Rectangle = CellContentRect(col, cellRect)

        If agregata Then
            Dim text As String = FooterTextFor(col)
            Dim padX As Integer = ScaleDpi(8)
            Dim textRect As New Rectangle(cellRect.Left + padX, cellRect.Top,
                                          Math.Max(0, cellRect.Width - 2 * padX), cellRect.Height)
            TextRenderer.DrawText(g, text, tf, contentRect, FooterForeResolved(),
                HorizontalFlags(col.TextAlign) Or TextFormatFlags.VerticalCenter Or
                TextFormatFlags.EndEllipsis)
        End If

        If FooterDrawsRightSeparator(col) Then
            Dim sepX As Integer = cellRect.Right - 1
            g.DrawLine(_pFooterSep, sepX, bandTop, sepX, bandTop + bandH - 1)
        End If
        If FooterDrawsLeftSeparator(col, stanga) Then
            g.DrawLine(_pFooterSep, cellRect.Left, bandTop, cellRect.Left, bandTop + bandH - 1)
        End If
    End Sub

    ''' <summary>
    ''' Muchia DREAPTĂ a unei celule de subsol se desenează? Doar pentru coloanele agregate —
    ''' regula cerută în 0028. Funcție pură: o folosește pictarea, o verifică testul (o regulă
    ''' de desen ascunsă într-un <c>OnPaint</c> nu se poate verifica decât cu ochiul).
    ''' </summary>
    Friend Shared Function FooterDrawsRightSeparator(col As KBotDataColumn) As Boolean
        Return col IsNot Nothing AndAlso col.Aggregate <> KBotAggregate.None
    End Function

    ''' <summary>
    ''' Muchia STÂNGĂ se desenează doar când coloana e agregată IAR vecinul din stânga nu e
    ''' (altfel el a desenat-o deja, ca muchie a lui dreaptă). Așa o valoare rămâne închisă între
    ''' două linii, fără ca vecinele goale să capete vreuna. <c>stanga = Nothing</c> înseamnă
    ''' «nu există vecin» — marginea controlului, unde chenarul e deja desenat.
    ''' </summary>
    Friend Shared Function FooterDrawsLeftSeparator(col As KBotDataColumn, stanga As KBotDataColumn) As Boolean
        If col Is Nothing OrElse col.Aggregate = KBotAggregate.None Then Return False
        If stanga Is Nothing Then Return False
        Return stanga.Aggregate = KBotAggregate.None
    End Function

    ' Fundalul unei benzi: plin, sau în degrade când SCHEMA cere așa (Modern) — vezi _bandGradient.
    Private Sub FillBand(g As Graphics, rect As Rectangle, plin As SolidBrush, capat As Color)
        If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
        If Not _bandGradient Then
            g.FillRectangle(plin, rect)
            Return
        End If
        Using lg As New LinearGradientBrush(rect, plin.Color, capat, LinearGradientMode.Vertical)
            g.FillRectangle(lg, rect)
        End Using
    End Sub

    ' ── Rânduri (virtualizat) ───────────────────────────────────────────────────

    ' Pictează DOAR benzile vizibile. Numărul RÂNDURILOR DE DATE dintre ele ajunge în
    ' DebugLastPaintedDataRows, poarta de verificare headless a virtualizării — antetele și
    ' subsolurile de grup nu se numără acolo, ca proba să însemne același lucru ca înainte de
    ' slice 0029 („costul unei pictări nu depinde de RowCount”).
    Private Sub DrawRows(g As Graphics)
        Dim painted As Integer = 0
        Dim bodyTop As Integer = HeaderBandHeight()
        Dim bodyH As Integer = ViewportHeight()
        Dim viewW As Integer = ViewportWidth()

        Dim first As Integer = FirstVisibleBand()
        Dim last As Integer = LastVisibleBand()
        If bodyH <= 0 OrElse first < 0 OrElse last < first Then
            DebugLastPaintedDataRows = 0
            Return
        End If

        ' Decupăm zona de date, ca benzile parțiale să nu deseneze peste antet/bare.
        Dim bodyClip As New Rectangle(0, bodyTop, viewW, bodyH)
        g.SetClip(bodyClip)

        For i As Integer = first To last
            Dim banda As KBotBand = BandAt(i)
            Dim y As Integer = bodyTop + banda.Top - VScrollOffset()
            If y + banda.Height <= bodyTop OrElse y >= bodyTop + bodyH Then Continue For
            If banda.Kind = KBotGroupBandKind.Data Then
                DrawRow(g, banda.ViewPosition, y, viewW)
                painted += 1
            Else
                DrawGroupBand(g, banda, y, viewW)
            End If
        Next

        g.ResetClip()
        DebugLastPaintedDataRows = painted
    End Sub

    ' English (slice 0028-03): viewPosition is the ON-SCREEN slot; rowIndex below is the MODEL
    ' index, and that is the one every event argument carries — a handler must never have to know
    ' that a filter is on. The stripe, by contrast, follows the VIEW position: alternating colours
    ' describe the printed page, so they have to stay alternating after rows are filtered out.
    Private Sub DrawRow(g As Graphics, viewPosition As Integer, y As Integer, viewW As Integer)
        Dim rowIndex As Integer = ModelIndexAt(viewPosition)
        If rowIndex < 0 Then Return
        Dim row As KBotDataRow = _rows(rowIndex)
        Dim isAlt As Boolean = _alternatingRows AndAlso (viewPosition Mod 2 = 1)
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
                     New Rectangle(_frozenBandWidth + cl.X - hOffset, y, cl.Column.WidthPx, _rowHeight),
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
                     New Rectangle(cl.X, y, cl.Column.WidthPx, _rowHeight),
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
                        rowBack, rowFore, CellFontFor(col), col.TextAlign,
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

        ' Dreptunghiul de CONȚINUT: celula minus retragerea cerută pe coloană. Butonul și bara de
        ' progres primesc mai jos cellRect întreg — ele desenează o formă cu marginile ei, nu un
        ' conținut retras (vezi KBotDataColumn.CellPadding).
        Dim contentRect As Rectangle = CellContentRect(col, cellRect)

        Select Case col.ColumnType
            Case KBotColumnType.CheckBox
                DrawCheckCell(g, contentRect, ToBool(value), enabled)
            Case KBotColumnType.OptionButton
                DrawOptionCell(g, contentRect, ToBool(value), enabled)
            Case KBotColumnType.Button
                ' Butonul nu ține valoare: eticheta e textul celulei, iar dacă lipsește,
                ' antetul coloanei (ex. o coloană «Detalii» cu același buton pe fiecare rând).
                Dim caption As String = If(String.IsNullOrEmpty(_cellArgs.Text), col.HeaderText, _cellArgs.Text)
                DrawButtonCell(g, cellRect, caption, _cellArgs.Font, enabled)
            Case KBotColumnType.ProgressBar
                DrawProgressCell(g, cellRect, ProgressFraction(value, col), enabled)
            Case KBotColumnType.Combo
                DrawComboCell(g, contentRect, _cellArgs.Text, _cellArgs.Font,
                              fore, _cellArgs.Alignment, enabled)
            Case Else
                DrawTextCell(g, contentRect, _cellArgs.Text, _cellArgs.Font,
                             fore, _cellArgs.Alignment)
        End Select

        ' Separatorul vertical de grilă, la marginea dreaptă a celulei.
        g.DrawLine(_pGridLine, cellRect.Right - 1, cellRect.Top, cellRect.Right - 1, cellRect.Bottom - 1)
    End Sub

    ''' <summary>
    ''' Dreptunghiul în care intră CONȚINUTUL unei celule: celula, minus
    ''' <see cref="KBotDataColumn.CellPadding"/> (scalată la DPI-ul controlului). Un singur loc,
    ''' folosit și de pictare și de măsurarea la conținut — două formule ar însemna o coloană
    ''' măsurată pe o lățime și scrisă pe alta, adică elipsă exact pe textul pentru care fusese
    ''' lărgită. Nu se lasă niciodată sub zero pe niciuna dintre axe.
    ''' </summary>
    Private Function CellContentRect(col As KBotDataColumn, cellRect As Rectangle) As Rectangle
        Dim p As Padding = col.CellPadding
        Dim st As Integer = ScaleDpi(p.Left)
        Dim sus As Integer = ScaleDpi(p.Top)
        Dim dr As Integer = ScaleDpi(p.Right)
        Dim jos As Integer = ScaleDpi(p.Bottom)
        Return New Rectangle(cellRect.Left + st, cellRect.Top + sus,
                             Math.Max(0, cellRect.Width - st - dr),
                             Math.Max(0, cellRect.Height - sus - jos))
    End Function

    ' Retragerea e deja scăzută de CellContentRect, în apelant — aici se primește dreptunghiul
    ' de conținut, nu celula.
    Private Sub DrawTextCell(g As Graphics, contentRect As Rectangle, text As String, font As Font,
                             fore As Color, align As ContentAlignment)
        If String.IsNullOrEmpty(text) Then Return
        TextRenderer.DrawText(g, text, font, contentRect, fore,
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

        ' Zecimalele fixate (slice 0028) taie ÎNAINTEA formatării: rotunjim numărul, apoi îl
        ' formatăm. Invers — «N4» peste o valoare deja formatată — n-ar mai avea ce rotunji.
        Dim rotunjit As Object = RoundForDisplay(value, col)

        ' Formatul NUMIT (slice 0028-02) merge înaintea lui FormatString fiindcă el știe și să
        ' citească valoarea, nu doar s-o formateze. Cele două nu pot fi setate amândouă (vezi
        ' KBotDataColumn.Format), deci nu e o precedență, e singura cale disponibilă.
        Dim numit As String = Nothing
        If KBotColumnFormat.TryFormat(rotunjit, col.Format, col.DecimalPlaces, numit) Then Return numit

        If Not String.IsNullOrEmpty(col.FormatString) Then
            Dim f As IFormattable = TryCast(rotunjit, IFormattable)
            If f IsNot Nothing Then Return f.ToString(col.FormatString, CultureInfo.CurrentCulture)
        End If

        ' Fără FormatString, zecimalele fixate SPUN singure formatul: 2 zecimale înseamnă două
        ' zecimale scrise, nu «2,5» pentru 2,50. Altfel proprietatea ar rotunji fără să se vadă.
        If col.HasDecimalPlaces Then
            Dim d As Double
            If TryNumeric(rotunjit, d) Then Return d.ToString("F" & col.DecimalPlaces.ToString(CultureInfo.InvariantCulture),
                                                              CultureInfo.CurrentCulture)
        End If

        Return rotunjit.ToString()
    End Function

    ''' <summary>
    ''' Valoarea rotunjită la <see cref="KBotDataColumn.DecimalPlaces"/>, dacă sunt fixate și dacă
    ''' valoarea e numerică; altfel valoarea neatinsă. Rotunjire NORMALĂ (0,5 în sus) — nu
    ''' implicitul .NET, care rotunjește „la par” și ar da 2 pentru 2,5.
    '''
    ''' Friend: o folosesc și pictarea (prin <c>FormatValue</c>) și agregatele din subsol, ca ce se
    ''' adună să fie exact ce se vede.
    ''' </summary>
    Friend Shared Function RoundForDisplay(value As Object, col As KBotDataColumn) As Object
        If value Is Nothing OrElse col Is Nothing OrElse Not col.HasDecimalPlaces Then Return value

        ' Decimal se rotunjește ca Decimal: trecerea prin Double ar pierde exact precizia pentru
        ' care cineva a ales Decimal la coloane de bani.
        If TypeOf value Is Decimal Then
            Return Math.Round(CDec(value), col.DecimalPlaces, MidpointRounding.AwayFromZero)
        End If

        Dim d As Double
        If Not TryNumeric(value, d) Then Return value        ' text ne-numeric etc.: neatins
        Return Math.Round(d, col.DecimalPlaces, MidpointRounding.AwayFromZero)
    End Function

    ' Coerciție tolerantă la Boolean pentru celulele de tip bifă (fără excepții).
    ''' <summary>Friend: o reia și <see cref="KBotColumnFormat"/>, ca „bifat” să însemne același
    ''' lucru în celulă și în formatul numit «Yes/No».</summary>
    Friend Shared Function ToBool(value As Object) As Boolean
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
