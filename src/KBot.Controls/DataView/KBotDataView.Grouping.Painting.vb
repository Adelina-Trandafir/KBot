Option Strict On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Globalization
Imports System.Windows.Forms

''' <summary>
''' PICTAREA benzilor de grup (slice 0029). Acoperită TRANZITIV de <c>Try</c>-ul din
''' <c>OnPaint</c> — regula casei: un ajutor de desen chemat doar dintr-un boundary deja
''' împachetat nu-și pune al lui, altfel s-ar loga o dată pe fiecare nivel.
'''
''' <para><b>O bandă de grup se pictează ca subsolul grilei, nu ca un rând.</b> Aceleași două
''' straturi (banda derulată, apoi cea înghețată desenată peste ea), aceeași regulă pentru titlu
''' — se OPREȘTE la prima coloană agregată, fiindcă un text care ar curge pe sub totalul cuiva
''' s-ar citi ca eticheta acelui total. Ce e în plus față de subsol: retragerea pe niveluri,
''' triunghiul de strângere și faptul că fondul se închide/deschide după adâncime.</para>
''' </summary>
Partial Class KBotDataView

    ''' <summary>Retragerea (px) a unei benzi de nivel dat: suma retragerilor nivelurilor de deasupra.</summary>
    Friend Function GroupIndentFor(level As Integer) As Integer
        Dim total As Integer = 0
        For d As Integer = 0 To Math.Min(level, _activeLevels.Count) - 1
            total += _activeLevels(d).Indent
        Next
        Return ScaleDpi(total)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' CULORI ȘI FONTURI — pe nivel, cu ultimul cuvânt la operator
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Fundalul unei benzi de grup. Ordinea: culoarea fixată pe nivel (dacă e vreuna, și dacă
    ''' schema nu e întunecată — vezi <see cref="DarkOverridesDesignerColors"/>), altfel cea din
    ''' temă, ȘTEARSĂ cu adâncimea: nivelul 0 e cel mai apăsat, fiecare nivel de sub el se apropie
    ''' de fundalul rândurilor. Așa ierarhia se citește fără ca nimeni să aleagă cinci culori.
    ''' </summary>
    Friend Function GroupBandBackFor(nivel As KBotGroupLevel, level As Integer, antet As Boolean) As Color
        If Not _schemeIsDark AndAlso nivel IsNot Nothing Then
            Dim fixata As Color = If(antet, nivel.HeaderBackColor, nivel.FooterBackColor)
            If fixata <> Color.Empty Then Return fixata
        End If
        Dim baza As Color = If(antet, _cGroupHeaderBack, _cGroupFooterBack)
        Dim t As Double = Math.Min(0.6, 0.22 * Math.Max(0, level))
        Return Blend(baza, _cRowBack, t)
    End Function

    ''' <summary>Culoarea textului dintr-o bandă de grup (aceeași precedență ca fundalul).</summary>
    Friend Function GroupBandForeFor(nivel As KBotGroupLevel, antet As Boolean) As Color
        If Not _schemeIsDark AndAlso nivel IsNot Nothing Then
            Dim fixata As Color = If(antet, nivel.HeaderForeColor, nivel.FooterForeColor)
            If fixata <> Color.Empty Then Return fixata
        End If
        Return If(antet, _cGroupHeaderText, _cGroupFooterText)
    End Function

    ''' <summary>
    ''' Fontul unei benzi de grup: al nivelului dacă și l-a cerut, altfel fontul de bandă al
    ''' schemei (semibold). Fontul fixat rămâne al operatorului în ORICE schemă — spre deosebire de
    ''' culori: un font nu devine ilizibil pe fundal închis, deci n-are de ce să fie luat înapoi.
    ''' </summary>
    Friend Function GroupBandFontFor(nivel As KBotGroupLevel, antet As Boolean) As Font
        If nivel IsNot Nothing Then
            Dim fixat As Font = If(antet, nivel.HeaderFont, nivel.FooterFont)
            If fixat IsNot Nothing Then Return fixat
        End If
        Return If(antet, ResolvedHeaderFont(), ResolvedFooterFont())
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' TITLUL
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Titlul unei benzi, compus din șablonul nivelului: <c>{0}</c> = titlul coloanei de grupare,
    ''' <c>{1}</c> = valoarea grupului, <c>{2}</c> = numărul de rânduri.
    '''
    ''' <para>Un șablon stricat (acolade nepereche) NU aruncă din mijlocul unei pictări: se cade pe
    ''' valoarea goală a grupului, care e oricum informația de bază. Un raport care refuză să se
    ''' deseneze din cauza unui șablon e mai rău decât unul cu titluri sărace.</para>
    ''' </summary>
    Friend Function GroupCaptionFor(nod As KBotGroupNode, nivel As KBotGroupLevel, antet As Boolean) As String
        If nod Is Nothing OrElse nivel Is Nothing Then Return String.Empty
        Dim sablon As String = If(antet, nivel.HeaderCaptionFormat, nivel.FooterCaptionFormat)
        Dim valoare As String = If(String.IsNullOrEmpty(nod.Key), If(nivel.EmptyCaption, String.Empty), nod.Key)
        If String.IsNullOrEmpty(sablon) Then Return String.Empty

        Dim col As KBotDataColumn = Nothing
        Dim titluColoana As String = If(_columnIndex.TryGetValue(nivel.ColumnKey, col),
                                        If(col.HeaderText, nivel.ColumnKey), nivel.ColumnKey)
        Try
            Return String.Format(CultureInfo.CurrentCulture, sablon, titluColoana, valoare, nod.RowCount)
        Catch ex As FormatException
            Return valoare
        End Try
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' PICTAREA UNEI BENZI
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub DrawGroupBand(g As Graphics, banda As KBotBand, y As Integer, viewW As Integer)
        Dim nod As KBotGroupNode = GroupAt(banda.GroupIndex)
        If nod Is Nothing Then Return
        If nod.Level < 0 OrElse nod.Level >= _activeLevels.Count Then Return
        Dim nivel As KBotGroupLevel = _activeLevels(nod.Level)
        Dim antet As Boolean = (banda.Kind = KBotGroupBandKind.GroupHeader)
        Dim strans As Boolean = antet AndAlso IsGroupCollapsedNode(nod)

        ' GroupFormatting — argumente REFOLOSITE. Handler-ul poate rescrie titlul și culorile
        ' pentru UN grup anume (luna cu depășire pe roșu), acolo unde nivelul le dă pe toate.
        _groupArgs.Reset(banda.Kind, nod.Level, nivel, nod.Value, nod.Key, nod.RowCount, strans,
                         GroupCaptionFor(nod, nivel, antet),
                         GroupBandBackFor(nivel, nod.Level, antet),
                         GroupBandForeFor(nivel, antet),
                         GroupBandFontFor(nivel, antet))
        RaiseEvent GroupFormatting(Me, _groupArgs)

        Dim bandRect As New Rectangle(0, y, viewW, banda.Height)
        Using b As New SolidBrush(_groupArgs.BackColor)
            g.FillRectangle(b, bandRect)
        End Using

        ' Agregatele, dacă nivelul le cere pe banda asta — stratificate ca la subsolul grilei.
        Dim cuAgregate As Boolean = If(antet, nivel.ShowHeaderAggregates, nivel.ShowFooterAggregates)
        If cuAgregate Then DrawGroupAggregates(g, nod, bandRect, _groupArgs.Font, _groupArgs.ForeColor)

        ' Titlul, după agregate (stă peste banda deja umplută) și înaintea liniei de despărțire.
        DrawGroupCaption(g, nod, nivel, bandRect, cuAgregate, strans, antet)

        ' Linia de despărțire sub bandă — aceeași cu cea dintre rânduri, ca banda să nu pară
        ' lipită de rândul de sub ea.
        g.DrawLine(_pGroupSep, 0, bandRect.Bottom - 1, viewW, bandRect.Bottom - 1)
    End Sub

    ' Titlul + triunghiul de strângere, în zona din stânga primei coloane agregate.
    Private Sub DrawGroupCaption(g As Graphics, nod As KBotGroupNode, nivel As KBotGroupLevel,
                                 bandRect As Rectangle, cuAgregate As Boolean, strans As Boolean,
                                 antet As Boolean)
        Dim capat As Integer = If(cuAgregate, FirstAggregatedColumnLeft(), Integer.MaxValue)
        Dim stanga As Integer = GroupIndentFor(nod.Level) + ScaleDpi(KBotDataColumn.HeaderTextPad)
        Dim dreapta As Integer = Math.Min(bandRect.Right, capat)
        If dreapta <= stanga Then Return

        ' Triunghiul de strângere își ia locul din stânga titlului, ca pictogramele de antet.
        If antet AndAlso nivel.EffectiveCollapsible Then
            Dim glif As Rectangle = GroupExpanderRect(bandRect, nod.Level)
            If Not glif.IsEmpty AndAlso glif.Right <= dreapta Then
                DrawGroupExpander(g, glif, strans, _groupArgs.ForeColor)
                stanga = glif.Right + ScaleDpi(KBotDataColumn.HeaderIconGap)
            End If
        End If

        If String.IsNullOrEmpty(_groupArgs.Caption) Then Return
        Dim textRect As New Rectangle(stanga, bandRect.Top,
                                      Math.Max(0, dreapta - ScaleDpi(KBotDataColumn.HeaderTextPad) - stanga),
                                      bandRect.Height)
        If textRect.Width <= 0 Then Return
        TextRenderer.DrawText(g, _groupArgs.Caption, _groupArgs.Font, textRect, _groupArgs.ForeColor,
            TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub

    ''' <summary>
    ''' Dreptunghiul triunghiului de strângere al unei benzi de antet. Funcție PURĂ, folosită și de
    ''' desen — hit-testul nu-l cere, fiindcă TOATĂ banda de antet e apăsabilă (vezi
    ''' <c>HandleGroupBandMouseDown</c>): o țintă de 9px într-un raport care se citește dintr-o
    ''' privire e o țintă ratată.
    ''' </summary>
    Friend Function GroupExpanderRect(bandRect As Rectangle, level As Integer) As Rectangle
        Dim latura As Integer = ScaleDpi(9)
        If bandRect.Height < latura Then Return Rectangle.Empty
        Dim stanga As Integer = bandRect.Left + GroupIndentFor(level) + ScaleDpi(KBotDataColumn.HeaderTextPad)
        Return New Rectangle(stanga, bandRect.Top + (bandRect.Height - latura) \ 2, latura, latura)
    End Function

    ' Triunghiul: spre dreapta când grupul e strâns («se deschide într-acolo»), în jos când e
    ' desfăcut — aceeași convenție ca butonul de strângere al grilei.
    Private Sub DrawGroupExpander(g As Graphics, r As Rectangle, strans As Boolean, fore As Color)
        Dim oldSmooth As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using b As New SolidBrush(fore)
            If strans Then
                g.FillPolygon(b, New Point() {
                    New Point(r.Left + r.Width \ 4, r.Top),
                    New Point(r.Right - r.Width \ 4, r.Top + r.Height \ 2),
                    New Point(r.Left + r.Width \ 4, r.Bottom)})
            Else
                g.FillPolygon(b, New Point() {
                    New Point(r.Left, r.Top + r.Height \ 4),
                    New Point(r.Right, r.Top + r.Height \ 4),
                    New Point(r.Left + r.Width \ 2, r.Bottom - r.Height \ 4)})
            End If
        End Using
        g.SmoothingMode = oldSmooth
    End Sub

    ' Agregatele grupului, fiecare sub coloana lui. Oglindește stratificarea subsolului grilei
    ' (înghețat PESTE derulat), deci un total de grup stă întotdeauna sub coloana lui, inclusiv cu
    ' ScrollByColumn pornit.
    Private Sub DrawGroupAggregates(g As Graphics, nod As KBotGroupNode, bandRect As Rectangle,
                                    font As Font, fore As Color)
        Dim viewW As Integer = ViewportWidth()
        Dim hOffset As Integer = HScrollOffset()

        Dim scrollClip As New Rectangle(_frozenBandWidth, bandRect.Top,
                                        Math.Max(0, viewW - _frozenBandWidth), bandRect.Height)
        Dim previousClip As Region = g.Clip
        g.SetClip(scrollClip, CombineMode.Intersect)
        For Each cl In _scrollLayout
            DrawGroupAggregateCell(g, nod, cl.Column, _frozenBandWidth + cl.X - hOffset,
                                   bandRect.Top, bandRect.Height, font, fore)
        Next
        g.Clip = previousClip
        previousClip.Dispose()

        For Each cl In _frozenLayout
            DrawGroupAggregateCell(g, nod, cl.Column, cl.X, bandRect.Top, bandRect.Height, font, fore)
        Next
    End Sub

    Private Sub DrawGroupAggregateCell(g As Graphics, nod As KBotGroupNode, col As KBotDataColumn,
                                       x As Integer, top As Integer, height As Integer,
                                       font As Font, fore As Color)
        If col.Aggregate = KBotAggregate.None Then Return
        Dim cellRect As New Rectangle(x, top, col.Width, height)
        If cellRect.Right < 0 OrElse cellRect.Left > ClientSize.Width Then Return

        Dim padX As Integer = ScaleDpi(8)
        Dim textRect As New Rectangle(cellRect.Left + padX, cellRect.Top,
                                      Math.Max(0, cellRect.Width - 2 * padX), cellRect.Height)
        TextRenderer.DrawText(g, GroupAggregateText(nod, col), font, textRect, fore,
            HorizontalFlags(col.TextAlign) Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
    End Sub

End Class
