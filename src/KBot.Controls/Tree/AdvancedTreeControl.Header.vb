Imports System.Drawing.Drawing2D

Partial Public Class AdvancedTreeControl
    ' The header caption area and which button (if any) it is the label of. Set while painting.
    Private _headerCaptionRect As Rectangle = Rectangle.Empty
    Private _headerCaptionOwner As CaptionOwner = CaptionOwner.None

    ''' <summary>Area of the header right-icon button: icon plus its caption when the caption belongs to it.</summary>
    Friend ReadOnly Property HeaderRightButtonRect As Rectangle
        Get
            If _headerRightIconRect.IsEmpty OrElse _headerCaptionOwner <> CaptionOwner.HeaderRight OrElse
               _headerCaptionRect.IsEmpty Then Return _headerRightIconRect
            Return Rectangle.Union(_headerRightIconRect, _headerCaptionRect)
        End Get
    End Property

    ''' <summary>Area of the header search-icon button: icon plus its caption when the caption belongs to it.</summary>
    Friend ReadOnly Property HeaderSearchButtonRect As Rectangle
        Get
            If _headerSearchIconRect.IsEmpty OrElse _headerCaptionOwner <> CaptionOwner.HeaderSearch OrElse
               _headerCaptionRect.IsEmpty Then Return _headerSearchIconRect
            Return Rectangle.Union(_headerSearchIconRect, _headerCaptionRect)
        End Get
    End Property

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

        ' ── Geometry first (drawing comes after) ─────────────────────────
        ' A button's hover plate also covers its caption, and the caption sits in the space left
        ' between the icons, so every rectangle is known before anything is painted.
        ' ── Left icon ────────────────────────────────────────────────
        Dim x As Integer = PaddingHeaderLeftPx
        Dim leftIconR As Rectangle = Rectangle.Empty
        If _headerLeftIcon IsNot Nothing Then
            Dim iy = midY - (_headerIconSize.Height \ 2)
            leftIconR = New Rectangle(x, iy, _headerIconSize.Width, _headerIconSize.Height)
            x += _headerIconSize.Width + PaddingIconGapPx
        End If

        ' ── Right side: RightIcon then SearchIcon (built right-to-left) ──
        ' The header spans the full width above the scrollbar (which starts below it), so its
        ' icons ignore the scrollbar: they must not jump left/right as it comes and goes.
        Dim rx As Integer = Me.Width - PaddingTreeEndPx

        _headerRightIconRect = Rectangle.Empty
        If _headerRightIcon IsNot Nothing Then
            rx -= _headerIconSize.Width
            Dim iy = midY - (_headerIconSize.Height \ 2)
            _headerRightIconRect = New Rectangle(rx, iy, _headerIconSize.Width, _headerIconSize.Height)
            rx -= PaddingIconGapPx
        End If

        _headerSearchIconRect = Rectangle.Empty
        If _headerSearchIcon IsNot Nothing Then
            rx -= _headerIconSize.Width
            Dim iy = midY - (_headerIconSize.Height \ 2)
            _headerSearchIconRect = New Rectangle(rx, iy, _headerIconSize.Width, _headerIconSize.Height)
            rx -= PaddingIconGapPx
        End If

        ' ── Caption layout (rich text, in the remaining space) ───────────
        Dim availW As Integer = Math.Max(0, rx - x)
        _headerCaptionRect = Rectangle.Empty
        _headerCaptionOwner = CaptionOwner.None
        Dim fmt = StringFormat.GenericTypographic
        fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.MeasureTrailingSpaces
        Dim parts As List(Of RichTextPart) = Nothing
        Dim cx As Single = 0
        Dim cy As Single = 0
        If Not String.IsNullOrEmpty(_headerCaption) AndAlso availW > 0 Then
            parts = ParseRichText(_headerCaption, Me.HeaderFont, HeaderForeColor)

            ' HeaderTextAlign applies to the WHOLE string of fragments (the mini-html is still drawn
            ' piece by piece): measure the total width/height first, then work out the start point in
            ' the space left between the icons.
            Dim latimeTotala As Single = 0
            Dim inaltimeMax As Single = 0
            For Each part In parts
                Dim sz = g.MeasureString(part.Text, part.Font, PointF.Empty, fmt)
                latimeTotala += sz.Width
                inaltimeMax = Math.Max(inaltimeMax, sz.Height)
            Next

            cx = AlignStartX(_headerTextAlign, x, availW, latimeTotala)
            ' Rounded down to a whole pixel: AlignStartY returns a Single (half a pixel when band and
            ' text differ by an odd number), and ClearType drawn between two pixel rows comes out
            ' soft. The rounding is HERE, not in AlignStartY -- that is a pure function used by tests.
            cy = CSng(Math.Floor(AlignStartY(_headerTextAlign, _headerHeight, inaltimeMax)))

            _headerCaptionRect = New Rectangle(CInt(Math.Floor(cx)), CInt(cy),
                                               CInt(Math.Ceiling(Math.Min(latimeTotala, CSng(availW)))),
                                               CInt(Math.Ceiling(inaltimeMax)))
            ' A title is not a button label. Only a caption aligned to the RIGHT, next to the icons,
            ' is read as the label of the nearest one (BAND RULE, 07.11.2026).
            If IsRightAligned(_headerTextAlign) Then
                If Not _headerSearchIconRect.IsEmpty Then
                    _headerCaptionOwner = CaptionOwner.HeaderSearch
                ElseIf Not _headerRightIconRect.IsEmpty Then
                    _headerCaptionOwner = CaptionOwner.HeaderRight
                End If
            End If
        End If

        ' ── Hover plates + icons (the plate sits UNDER the picture and the text) ──
        If Not leftIconR.IsEmpty Then g.DrawImage(_headerLeftIcon, leftIconR)
        If Not _headerRightIconRect.IsEmpty Then
            If _headerRightIconHover Then DrawButtonHover(g, HeaderRightButtonRect, HeaderRightIconHoverColor)
            g.DrawImage(_headerRightIcon, _headerRightIconRect)
        End If
        If Not _headerSearchIconRect.IsEmpty Then
            If _headerSearchIconHover Then DrawButtonHover(g, HeaderSearchButtonRect, HeaderSearchIconHoverColor)
            g.DrawImage(_headerSearchIcon, _headerSearchIconRect)
        End If

        ' ── Caption text ─────────────────────────────────────────────────
        If parts IsNot Nothing Then
            Dim oldClip = g.Clip.Clone()
            g.SetClip(New Rectangle(x, 0, availW, _headerHeight))

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
        ' Culoarea și grosimea se cer acum din designer (felia 0038); grosimea e LOGICĂ, deci trece
        ' prin SY, iar 0 înseamnă «fără linie» — de aceea nici nu se construiește creionul.
        Dim latimeSep As Integer = SY(HeaderSeparatorWidth)
        If latimeSep > 0 Then
            Using sep As New Pen(HeaderSeparatorColor, latimeSep)
                g.DrawLine(sep, 0, _headerHeight - 1, Me.Width, _headerHeight - 1)
            End Using
        End If
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
        If Not String.IsNullOrEmpty(_footerCollapseExpandedImageKey) Then
            If cache.TryGetValue(_footerCollapseExpandedImageKey, img) Then _footerCollapseExpandedImage = img
        End If
        If Not String.IsNullOrEmpty(_footerCollapseCollapsedImageKey) Then
            If cache.TryGetValue(_footerCollapseCollapsedImageKey, img) Then _footerCollapseCollapsedImage = img
        End If
        If Not String.IsNullOrEmpty(_searchClearButtonImageKey) Then
            If cache.TryGetValue(_searchClearButtonImageKey, img) Then
                _searchClearButtonImage = img
                ApplyClearButtonLook()
                If _isSearchMode Then PositionSearchTextBox()
            End If
        End If

        ' Auto-open: SearchShow = True și nu există iconiță toggle
        ApplySearchShow()

        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' The current rectangle of the header's right icon, in client coordinates (empty = not
    ''' shown). Slice 0777: the host opens a <c>CustomPopup</c> under it, the way the footer
    ''' icons already expose <see cref="FooterRightIconRect"/>. It is the rectangle the last
    ''' paint used, i.e. exactly the one the click was hit-tested against.
    ''' </summary>
    <System.ComponentModel.Browsable(False)>
    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HeaderRightIconRect As Rectangle
        Get
            If Not _headerVisible OrElse _headerRightIcon Is Nothing Then Return Rectangle.Empty
            Return _headerRightIconRect
        End Get
    End Property
End Class
