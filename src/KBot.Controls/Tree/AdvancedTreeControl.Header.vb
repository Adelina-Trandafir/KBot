Imports System.Drawing.Drawing2D

Partial Public Class AdvancedTreeControl
    ' ══════════════════════════════════════════════════════════════════
    ' HEADER — DRAWING
    ' ══════════════════════════════════════════════════════════════════
    Friend Sub DrawHeader(g As Graphics)
        ' Background — plin sau în degrade din HeaderBackColor spre HeaderGradientEndColor
        ' (implicit: spre alb dacă baza e deschisă, spre negru dacă e închisă — vezi .Theming).
        Dim bandRect As New Rectangle(0, 0, Math.Max(1, Me.Width), Math.Max(1, _headerHeight))
        If _headerBackStyle = En_HeaderBackStyle.Solid Then
            Using bg As New SolidBrush(HeaderBackColor)
                g.FillRectangle(bg, bandRect)
            End Using
        Else
            Dim directie As LinearGradientMode =
                If(_headerBackStyle = En_HeaderBackStyle.GradientHorizontal,
                   LinearGradientMode.Horizontal, LinearGradientMode.Vertical)
            Using bg As New LinearGradientBrush(bandRect, HeaderBackColor,
                                                HeaderGradientEndColor, directie)
                g.FillRectangle(bg, bandRect)
            End Using
        End If

        Dim midY As Integer = _headerHeight \ 2

        ' ── Left icon ────────────────────────────────────────────────
        Dim x As Integer = PaddingHeaderLeft
        If _headerLeftIcon IsNot Nothing Then
            Dim iy = midY - (_headerIconSize.Height \ 2)
            g.DrawImage(_headerLeftIcon, x, iy, _headerIconSize.Width, _headerIconSize.Height)
            x += _headerIconSize.Width + PaddingIconGap
        End If

        ' ── Right side: RightIcon then SearchIcon (built right-to-left) ──
        Dim scrollW As Integer = ScrollBarWidth 'If(_vScroll.Visible, _vScroll.Width, 0)
        Dim rx As Integer = Me.Width - PaddingTreeEnd - scrollW

        _headerRightIconRect = Rectangle.Empty
        If _headerRightIcon IsNot Nothing Then
            rx -= _headerIconSize.Width
            Dim iy = midY - (_headerIconSize.Height \ 2)
            _headerRightIconRect = New Rectangle(rx, iy, _headerIconSize.Width, _headerIconSize.Height)
            If _headerRightIconHover Then
                DrawButtonHover(g, _headerRightIconRect, HeaderRightIconHoverColor)
            End If
            g.DrawImage(_headerRightIcon, _headerRightIconRect)
            rx -= PaddingIconGap
        End If

        _headerSearchIconRect = Rectangle.Empty
        If _headerSearchIcon IsNot Nothing Then
            rx -= _headerIconSize.Width
            Dim iy = midY - (_headerIconSize.Height \ 2)
            _headerSearchIconRect = New Rectangle(rx, iy, _headerIconSize.Width, _headerIconSize.Height)
            If _headerSearchIconHover Then
                DrawButtonHover(g, _headerSearchIconRect, HeaderSearchIconHoverColor)
            End If
            g.DrawImage(_headerSearchIcon, _headerSearchIconRect)
            rx -= PaddingIconGap
        End If

        ' ── Caption (rich text, in remaining space) ───────────────────
        Dim captionRight As Integer = rx
        Dim availW As Integer = Math.Max(0, captionRight - x)
        If Not String.IsNullOrEmpty(_headerCaption) AndAlso availW > 0 Then
            Dim fmt = StringFormat.GenericTypographic
            fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.MeasureTrailingSpaces
            Dim parts = ParseRichText(_headerCaption, Me.HeaderFont, HeaderForeColor)
            Dim oldClip = g.Clip.Clone()
            g.SetClip(New Rectangle(x, 0, availW, _headerHeight))

            ' HeaderTextAlign se aplică pe TOT șirul de fragmente (mini-html-ul rămâne
            ' desenat bucată cu bucată): măsurăm întâi lățimea/înălțimea totală, apoi
            ' calculăm punctul de plecare în spațiul rămas între iconițe.
            Dim latimeTotala As Single = 0
            Dim inaltimeMax As Single = 0
            For Each part In parts
                Dim sz = g.MeasureString(part.Text, part.Font, PointF.Empty, fmt)
                latimeTotala += sz.Width
                inaltimeMax = Math.Max(inaltimeMax, sz.Height)
            Next

            Dim cx As Single = AlignStartX(_headerTextAlign, x, availW, latimeTotala)
            Dim cy As Single = AlignStartY(_headerTextAlign, _headerHeight, inaltimeMax)

            For Each part In parts
                Dim sz = g.MeasureString(part.Text, part.Font, PointF.Empty, fmt)
                If cx + sz.Width > x + availW Then Exit For
                If part.HasBackColor Then
                    Using b As New SolidBrush(part.BackColor)
                        g.FillRectangle(b, cx, 0, sz.Width, _headerHeight)
                    End Using
                End If
                Using b As New SolidBrush(part.ForeColor)
                    g.DrawString(part.Text, part.Font, b, cx, cy, fmt)
                End Using
                cx += sz.Width
            Next
            g.Clip = oldClip
        End If

        ' ── Bottom separator ─────────────────────────────────────────
        Using sep As New Pen(Color.FromArgb(60, HeaderForeColor))
            g.DrawLine(sep, 0, _headerHeight - 1, Me.Width, _headerHeight - 1)
        End Using
    End Sub

    ' Punctul de plecare al caption-ului (orizontal/vertical) după HeaderTextAlign: AlignStartX /
    ' AlignStartY din partiala .Footer — o singură interpretare a lui ContentAlignment pentru
    ' amândouă benzile.

    ' ══════════════════════════════════════════════════════════════════
    ' HEADER — ICON KEY RESOLUTION (called from Tree.Builder after cache load)
    ' ══════════════════════════════════════════════════════════════════

    Public Sub ResolveHeaderIcons(cache As Dictionary(Of String, Image))
        Dim img As Image = Nothing
        If Not String.IsNullOrEmpty(_headerLeftIconKey) Then
            If cache.TryGetValue(_headerLeftIconKey, img) Then _headerLeftIcon = img
        End If
        If Not String.IsNullOrEmpty(_headerRightIconKey) Then
            If cache.TryGetValue(_headerRightIconKey, img) Then _headerRightIcon = img
        End If
        If Not String.IsNullOrEmpty(_headerSearchIconKey) Then
            If cache.TryGetValue(_headerSearchIconKey, img) Then _headerSearchIcon = img
        End If
        If Not String.IsNullOrEmpty(_footerLeftIconKey) Then
            If cache.TryGetValue(_footerLeftIconKey, img) Then _footerLeftIcon = img
        End If
        If Not String.IsNullOrEmpty(_footerRightIconKey) Then
            If cache.TryGetValue(_footerRightIconKey, img) Then _footerRightIcon = img
        End If

        ' Auto-open: SearchShow = True și nu există iconiță toggle
        ApplySearchShow()

        Me.Invalidate()
    End Sub
End Class
