Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' A side panel of foldable sections, each a titled list of rows (recent documents, recent actions,
''' session information, session errors...). Sections are authored in the designer (see
''' <see cref="KBotSidePanelSection"/>); rows are normally filled from code. The whole panel folds to
''' a narrow strip that keeps the sections' icons. Everything is drawn here, so it themes through
''' <see cref="IThemedControl"/> only.
'''
''' <para><b>Width.</b> Like the ribbon owns its height, the panel owns its width (the strip when
''' folded, <see cref="ExpandedWidth"/> otherwise, both logical and scaled with
''' <c>ThemeShapes.ScaleDpi</c>) as long as it is docked left or right.</para>
'''
''' <para><b>Scrolling.</b> When the sections are taller than the panel the mouse wheel scrolls them
''' (a thin indicator shows the position). No scroll bar control, nothing takes the keyboard focus.</para>
'''
''' <para>Not done on purpose (first version): keyboard navigation, drag to reorder, row icons.</para>
''' </summary>
<DefaultEvent("RowClicked")>
Public NotInheritable Class KBotSidePanel
    Inherits Control
    Implements IThemedControl

    ' Logical (96 dpi) measures.
    Private Const BarHeight As Integer = 28
    Private Const HeaderHeight As Integer = 30
    Private Const LineRowHeight As Integer = 24
    Private Const StackedRowHeight As Integer = 42
    Private Const StripWidth As Integer = 44
    Private Const StripSlotHeight As Integer = 38
    Private Const IconSize As Integer = 16
    Private Const SidePad As Integer = 10

    Private ReadOnly _sections As New KBotSidePanelSectionCollection()
    Private _collapsed As Boolean
    Private _expandedWidth As Integer = 270
    Private _tooltip As KBotToolTip

    Private _scheme As ThemeScheme
    Private _layoutValid As Boolean
    Private _layoutKey As Integer
    Private _contentHeight As Integer
    Private _scroll As Integer

    Private _hotRow As KBotSidePanelRow
    Private _hotHeader As KBotSidePanelSection
    Private _hotBar As Boolean
    Private _hotSlot As Integer = -1

    Private _boldFont As Font
    Private _smallFont As Font
    Private _italicFont As Font

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)
        SetStyle(ControlStyles.Selectable, False)
        _sections.Owner = Me
    End Sub

    ' ---- Model ------------------------------------------------------------------------------

    <Category("K-BOT")>
    <Description("The sections of the panel; each has a title and a list of rows.")>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Content)>
    Public ReadOnly Property Sections As KBotSidePanelSectionCollection
        Get
            Return _sections
        End Get
    End Property

    <Category("K-BOT")>
    <Description("True = folded to a narrow strip with the sections' icons.")>
    <DefaultValue(False)>
    Public Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
        Set(k_value As Boolean)
            If k_value = _collapsed Then Return
            _collapsed = k_value
            _scroll = 0
            InvalidateLayout()
            UpdateWidth()
            RaiseEvent CollapsedChanged(Me, EventArgs.Empty)
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Width when unfolded, in logical pixels (96 dpi).")>
    <DefaultValue(270)>
    Public Property ExpandedWidth As Integer
        Get
            Return _expandedWidth
        End Get
        Set(k_value As Integer)
            _expandedWidth = Math.Max(120, k_value)
            InvalidateLayout()
            UpdateWidth()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("The tooltip component that shows the rows' tooltips. Without it no tooltip is shown.")>
    Public Property Tooltip As KBotToolTip
        Get
            Return _tooltip
        End Get
        Set(k_value As KBotToolTip)
            _tooltip = k_value
        End Set
    End Property

    Private Function ShouldSerializeTooltip() As Boolean
        Return _tooltip IsNot Nothing
    End Function

    Private Sub ResetTooltip()
        _tooltip = Nothing
    End Sub

    <Category("K-BOT")>
    <Description("Raised when a row that has a key is clicked.")>
    Public Event RowClicked As EventHandler(Of KBotSidePanelRowClickedEventArgs)

    <Category("K-BOT")>
    <Description("Raised when the panel is folded or unfolded.")>
    Public Event CollapsedChanged As EventHandler

    ''' <summary>The section with this key; Nothing when there is none.</summary>
    Public Function FindSection(k_key As String) As KBotSidePanelSection
        For Each k_section As KBotSidePanelSection In _sections
            If String.Equals(k_section.Key, k_key, StringComparison.Ordinal) Then Return k_section
        Next
        Return Nothing
    End Function

    ''' <summary>Call after changing rows or sections from code in a way the collections cannot see (a row's text or colour).</summary>
    Public Sub InvalidateLayout()
        _layoutValid = False
        If IsHandleCreated Then Invalidate()
    End Sub

    ' ---- Width ------------------------------------------------------------------------------

    Private Function Px(k_logical As Integer) As Integer
        Return ThemeShapes.ScaleDpi(Me, k_logical)
    End Function

    Private Function DesiredWidth() As Integer
        Return If(_collapsed, Px(StripWidth), Px(_expandedWidth))
    End Function

    Private Function OwnsWidth() As Boolean
        Return Dock = DockStyle.Left OrElse Dock = DockStyle.Right OrElse Dock = DockStyle.None
    End Function

    Protected Overrides Sub SetBoundsCore(k_x As Integer, k_y As Integer, k_width As Integer, k_height As Integer, k_specified As BoundsSpecified)
        MyBase.SetBoundsCore(k_x, k_y, If(OwnsWidth(), DesiredWidth(), k_width), k_height, k_specified)
    End Sub

    Private Sub UpdateWidth()
        If Not OwnsWidth() Then Return
        If Width <> DesiredWidth() Then SetBounds(Left, Top, DesiredWidth(), Height, BoundsSpecified.Width)
    End Sub

    Protected Overrides Sub OnHandleCreated(k_e As EventArgs)
        MyBase.OnHandleCreated(k_e)
        UpdateWidth()
    End Sub

    Protected Overrides Sub OnDpiChangedAfterParent(k_e As EventArgs)
        MyBase.OnDpiChangedAfterParent(k_e)
        InvalidateLayout()
        UpdateWidth()
    End Sub

    Protected Overrides Sub OnFontChanged(k_e As EventArgs)
        MyBase.OnFontChanged(k_e)
        _boldFont?.Dispose()
        _boldFont = Nothing
        _smallFont?.Dispose()
        _smallFont = Nothing
        _italicFont?.Dispose()
        _italicFont = Nothing
        InvalidateLayout()
    End Sub

    Protected Overrides Sub OnSizeChanged(k_e As EventArgs)
        MyBase.OnSizeChanged(k_e)
        _layoutValid = False
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing Then
            _boldFont?.Dispose()
            _smallFont?.Dispose()
            _italicFont?.Dispose()
        End If
        MyBase.Dispose(k_disposing)
    End Sub

    ' ---- Theme ------------------------------------------------------------------------------

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        If k_scheme Is Nothing Then Return
        _scheme = k_scheme
        BackColor = k_scheme.Palette.SurfaceColor
        InvalidateLayout()
        UpdateWidth()
    End Sub

    Private Function Palette() As ThemePalette
        Return If(_scheme, ThemeManager.Current).Palette
    End Function

    Private Function CornerRadius() As Integer
        Dim k_scheme As ThemeScheme = If(_scheme, ThemeManager.Current)
        Return Math.Max(Px(3), Math.Min(Px(8), Px(k_scheme.Style.CornerRadius)))
    End Function

    Private Function BoldFont() As Font
        If _boldFont Is Nothing Then _boldFont = New Font(Font, FontStyle.Bold)
        Return _boldFont
    End Function

    Private Function SmallFont() As Font
        If _smallFont Is Nothing Then _smallFont = New Font(Font.FontFamily, Math.Max(6.0F, Font.Size - 1.0F), FontStyle.Regular)
        Return _smallFont
    End Function

    Private Function ItalicFont() As Font
        If _italicFont Is Nothing Then _italicFont = New Font(Font, FontStyle.Italic)
        Return _italicFont
    End Function

    ' ---- Layout (content coordinates: y = 0 is the top of the first header) ----------------------

    Private Function ViewTop() As Integer
        Return Px(BarHeight)
    End Function

    Private Function ViewHeight() As Integer
        Return Math.Max(0, Height - ViewTop())
    End Function

    Private Function MaxScroll() As Integer
        Return Math.Max(0, _contentHeight - ViewHeight())
    End Function

    Private Sub EnsureLayout()
        Dim k_key As Integer = Px(1000) Xor (Width << 3) Xor (Height << 17) Xor If(_collapsed, 1, 0)
        If _layoutValid AndAlso k_key = _layoutKey Then Return
        _layoutKey = k_key
        _layoutValid = True

        Dim k_y As Integer = 0
        Dim k_w As Integer = Width
        For Each k_section As KBotSidePanelSection In _sections
            k_section.HeaderBounds = Rectangle.Empty
            For Each k_row As KBotSidePanelRow In k_section.Rows
                k_row.Bounds = Rectangle.Empty
            Next
            If Not k_section.Visible Then Continue For
            k_section.HeaderBounds = New Rectangle(0, k_y, k_w, Px(HeaderHeight))
            k_y += Px(HeaderHeight)
            If Not k_section.Collapsed Then
                If k_section.Rows.Count = 0 Then
                    k_y += If(String.IsNullOrEmpty(k_section.EmptyText), 0, Px(LineRowHeight))
                Else
                    Dim k_rowH As Integer = If(k_section.RowStyle = KBotSidePanelRowStyle.Stacked, Px(StackedRowHeight), Px(LineRowHeight))
                    For Each k_row As KBotSidePanelRow In k_section.Rows
                        k_row.Bounds = New Rectangle(0, k_y, k_w, k_rowH)
                        k_y += k_rowH
                    Next
                End If
            End If
            k_y += Px(6)
        Next
        _contentHeight = k_y
        _scroll = Math.Min(_scroll, MaxScroll())
    End Sub

    ' Content rectangle -> client rectangle (scrolled, below the top bar).
    Private Function ToView(k_rect As Rectangle) As Rectangle
        Return New Rectangle(k_rect.Left, k_rect.Top + ViewTop() - _scroll, k_rect.Width, k_rect.Height)
    End Function

    Private Function BarRect() As Rectangle
        Return New Rectangle(0, 0, Width, ViewTop())
    End Function

    Private Function StripSlot(k_index As Integer) As Rectangle
        Return New Rectangle(0, ViewTop() + k_index * Px(StripSlotHeight), Width, Px(StripSlotHeight))
    End Function

    ' ---- Painting ---------------------------------------------------------------------------

    Protected Overrides Sub OnPaint(k_e As PaintEventArgs)
        Try
            EnsureLayout()
            Dim k_g As Graphics = k_e.Graphics
            Dim k_p As ThemePalette = Palette()
            k_g.SmoothingMode = SmoothingMode.AntiAlias
            k_g.Clear(k_p.SurfaceColor)

            PaintBar(k_g, k_p)
            If _collapsed Then
                PaintStrip(k_g, k_p)
            Else
                k_g.SetClip(New Rectangle(0, ViewTop(), Width, ViewHeight()))
                For Each k_section As KBotSidePanelSection In _sections
                    If Not k_section.Visible Then Continue For
                    PaintSection(k_g, k_p, k_section)
                Next
                k_g.ResetClip()
                PaintScrollIndicator(k_g, k_p)
            End If
            Using k_pen As New Pen(k_p.BorderColor)
                k_g.DrawLine(k_pen, Width - 1, 0, Width - 1, Height)
            End Using
        Catch ex As Exception
            ' UI boundary (OnPaint): log and swallow.
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotSidePanel.OnPaint", ex)
        End Try
    End Sub

    Private Sub PaintBar(k_g As Graphics, k_p As ThemePalette)
        Dim k_bar As Rectangle = BarRect()
        If _hotBar Then FillRounded(k_g, Rectangle.Inflate(New Rectangle(k_bar.Right - Px(30), k_bar.Top, Px(28), k_bar.Height), -Px(2), -Px(3)), k_p.ButtonHoverColor)
        ' Chevron: points left to fold, right to unfold.
        Dim k_cx As Integer = If(_collapsed, k_bar.Width \ 2, k_bar.Right - Px(16))
        Dim k_cy As Integer = k_bar.Top + k_bar.Height \ 2
        Dim k_s As Integer = Px(4)
        Using k_pen As New Pen(k_p.TextDimColor, Math.Max(1.0F, Px(2) / 2.0F))
            If _collapsed Then
                k_g.DrawLines(k_pen, New Point() {New Point(k_cx - k_s \ 2, k_cy - k_s), New Point(k_cx + k_s \ 2, k_cy), New Point(k_cx - k_s \ 2, k_cy + k_s)})
            Else
                k_g.DrawLines(k_pen, New Point() {New Point(k_cx + k_s \ 2, k_cy - k_s), New Point(k_cx - k_s \ 2, k_cy), New Point(k_cx + k_s \ 2, k_cy + k_s)})
            End If
        End Using
        Using k_pen As New Pen(k_p.BorderColor)
            k_g.DrawLine(k_pen, 0, k_bar.Bottom - 1, Width, k_bar.Bottom - 1)
        End Using
    End Sub

    Private Sub PaintStrip(k_g As Graphics, k_p As ThemePalette)
        Dim k_index As Integer = 0
        For Each k_section As KBotSidePanelSection In _sections
            If Not k_section.Visible Then Continue For
            Dim k_slot As Rectangle = StripSlot(k_index)
            If k_index = _hotSlot Then FillRounded(k_g, Rectangle.Inflate(k_slot, -Px(4), -Px(3)), k_p.ButtonHoverColor)
            If k_section.Icon IsNot Nothing Then
                Dim k_icon As Integer = Px(IconSize)
                k_g.InterpolationMode = InterpolationMode.HighQualityBicubic
                k_g.DrawImage(k_section.Icon, New Rectangle(k_slot.Left + (k_slot.Width - k_icon) \ 2, k_slot.Top + (k_slot.Height - k_icon) \ 2, k_icon, k_icon))
            End If
            k_index += 1
        Next
    End Sub

    Private Sub PaintSection(k_g As Graphics, k_p As ThemePalette, k_section As KBotSidePanelSection)
        Dim k_head As Rectangle = ToView(k_section.HeaderBounds)
        If k_head.Bottom < ViewTop() OrElse k_head.Top > Height Then
            ' Header out of view; its rows may still show.
        Else
            Using k_brush As New SolidBrush(ThemeShapes.Blend(k_p.SurfaceColor, k_p.BorderColor, 0.45))
                k_g.FillRectangle(k_brush, New Rectangle(k_head.Left, k_head.Top, k_head.Width - 1, k_head.Height))
            End Using
            If ReferenceEquals(k_section, _hotHeader) Then
                Using k_brush As New SolidBrush(Color.FromArgb(40, k_p.TextColor))
                    k_g.FillRectangle(k_brush, New Rectangle(k_head.Left, k_head.Top, k_head.Width - 1, k_head.Height))
                End Using
            End If
            Dim k_x As Integer = k_head.Left + Px(SidePad)
            If k_section.Icon IsNot Nothing Then
                Dim k_icon As Integer = Px(IconSize)
                k_g.InterpolationMode = InterpolationMode.HighQualityBicubic
                k_g.DrawImage(k_section.Icon, New Rectangle(k_x, k_head.Top + (k_head.Height - k_icon) \ 2, k_icon, k_icon))
                k_x += k_icon + Px(6)
            End If
            TextRenderer.DrawText(k_g, If(k_section.Title, String.Empty).ToUpperInvariant(), BoldFont(),
                                  New Rectangle(k_x, k_head.Top, k_head.Right - k_x - Px(24), k_head.Height), k_p.TextColor,
                                  TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or
                                  TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis)
            ' Section chevron: down = open, right = folded.
            Dim k_cx As Integer = k_head.Right - Px(16)
            Dim k_cy As Integer = k_head.Top + k_head.Height \ 2
            Dim k_s As Integer = Px(3)
            Using k_pen As New Pen(k_p.TextDimColor, Math.Max(1.0F, Px(2) / 2.0F))
                If k_section.Collapsed Then
                    k_g.DrawLines(k_pen, New Point() {New Point(k_cx - k_s \ 2, k_cy - k_s), New Point(k_cx + k_s \ 2, k_cy), New Point(k_cx - k_s \ 2, k_cy + k_s)})
                Else
                    k_g.DrawLines(k_pen, New Point() {New Point(k_cx - k_s, k_cy - k_s \ 2), New Point(k_cx, k_cy + k_s \ 2), New Point(k_cx + k_s, k_cy - k_s \ 2)})
                End If
            End Using
        End If
        If k_section.Collapsed Then Return

        If k_section.Rows.Count = 0 Then
            If Not String.IsNullOrEmpty(k_section.EmptyText) Then
                Dim k_empty As New Rectangle(k_head.Left + Px(SidePad), k_head.Bottom, k_head.Width - Px(SidePad) * 2, Px(LineRowHeight))
                TextRenderer.DrawText(k_g, k_section.EmptyText, ItalicFont(), k_empty, k_p.DisabledTextColor,
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or
                                      TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis)
            End If
            Return
        End If
        For Each k_row As KBotSidePanelRow In k_section.Rows
            Dim k_rect As Rectangle = ToView(k_row.Bounds)
            If k_rect.Bottom < ViewTop() OrElse k_rect.Top > Height Then Continue For
            PaintRow(k_g, k_p, k_section, k_row, k_rect)
        Next
    End Sub

    Private Sub PaintRow(k_g As Graphics, k_p As ThemePalette, k_section As KBotSidePanelSection, k_row As KBotSidePanelRow, k_rect As Rectangle)
        If ReferenceEquals(k_row, _hotRow) AndAlso Not String.IsNullOrEmpty(k_row.Key) Then
            FillRounded(k_g, Rectangle.Inflate(New Rectangle(k_rect.Left, k_rect.Top, k_rect.Width - 1, k_rect.Height), -Px(3), -Px(1)), k_p.ButtonHoverColor)
        End If
        Dim k_color As Color = If(k_row.ForeColor.IsEmpty, k_p.TextColor, k_row.ForeColor)
        Dim k_x As Integer = k_rect.Left + Px(SidePad)
        Dim k_w As Integer = k_rect.Width - Px(SidePad) * 2 - 1
        Const k_flags As TextFormatFlags = TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or
                                           TextFormatFlags.SingleLine Or TextFormatFlags.EndEllipsis
        If k_section.RowStyle = KBotSidePanelRowStyle.Stacked Then
            Dim k_half As Integer = k_rect.Height \ 2
            TextRenderer.DrawText(k_g, k_row.DetailText, SmallFont(), New Rectangle(k_x, k_rect.Top + Px(3), k_w, k_half - Px(3)), k_p.TextDimColor, k_flags)
            TextRenderer.DrawText(k_g, k_row.Text, BoldFont(), New Rectangle(k_x, k_rect.Top + k_half - Px(2), k_w, k_half), k_color, k_flags)
        Else
            Dim k_detailW As Integer = 0
            If Not String.IsNullOrEmpty(k_row.DetailText) Then
                k_detailW = TextRenderer.MeasureText(k_row.DetailText, SmallFont(), New Size(Integer.MaxValue, 0),
                                                     TextFormatFlags.NoPadding Or TextFormatFlags.SingleLine).Width + Px(6)
                TextRenderer.DrawText(k_g, k_row.DetailText, SmallFont(), New Rectangle(k_x + k_w - k_detailW, k_rect.Top, k_detailW, k_rect.Height),
                                      k_p.TextDimColor, TextFormatFlags.Right Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPadding Or TextFormatFlags.SingleLine)
            End If
            TextRenderer.DrawText(k_g, k_row.Text, If(k_row.Italic, ItalicFont(), Font), New Rectangle(k_x, k_rect.Top, Math.Max(0, k_w - k_detailW), k_rect.Height), k_color, k_flags)
        End If
    End Sub

    Private Sub PaintScrollIndicator(k_g As Graphics, k_p As ThemePalette)
        Dim k_max As Integer = MaxScroll()
        If k_max <= 0 Then Return
        Dim k_trackH As Integer = ViewHeight()
        Dim k_thumbH As Integer = Math.Max(Px(24), CInt(k_trackH * (k_trackH / CDbl(_contentHeight))))
        Dim k_thumbY As Integer = ViewTop() + CInt((k_trackH - k_thumbH) * (_scroll / CDbl(k_max)))
        Using k_brush As New SolidBrush(Color.FromArgb(110, k_p.TextDimColor))
            Dim k_thumb As New Rectangle(Width - Px(5), k_thumbY, Px(3), k_thumbH)
            Using k_path As GraphicsPath = ThemeShapes.RoundedRect(k_thumb, Px(1))
                k_g.FillPath(k_brush, k_path)
            End Using
        End Using
    End Sub

    Private Sub FillRounded(k_g As Graphics, k_rect As Rectangle, k_color As Color)
        Using k_path As GraphicsPath = ThemeShapes.RoundedRect(k_rect, CornerRadius())
            Using k_brush As New SolidBrush(k_color)
                k_g.FillPath(k_brush, k_path)
            End Using
        End Using
    End Sub

    ' ---- Input ------------------------------------------------------------------------------

    Private Function SectionHeaderAt(k_point As Point) As KBotSidePanelSection
        If _collapsed OrElse k_point.Y < ViewTop() Then Return Nothing
        For Each k_section As KBotSidePanelSection In _sections
            If k_section.Visible AndAlso ToView(k_section.HeaderBounds).Contains(k_point) Then Return k_section
        Next
        Return Nothing
    End Function

    Private Function RowAt(k_point As Point, ByRef k_owner As KBotSidePanelSection) As KBotSidePanelRow
        k_owner = Nothing
        If _collapsed OrElse k_point.Y < ViewTop() Then Return Nothing
        For Each k_section As KBotSidePanelSection In _sections
            If Not k_section.Visible OrElse k_section.Collapsed Then Continue For
            For Each k_row As KBotSidePanelRow In k_section.Rows
                If Not k_row.Bounds.IsEmpty AndAlso ToView(k_row.Bounds).Contains(k_point) Then
                    k_owner = k_section
                    Return k_row
                End If
            Next
        Next
        Return Nothing
    End Function

    Private Function SlotAt(k_point As Point) As Integer
        If Not _collapsed OrElse k_point.Y < ViewTop() Then Return -1
        Dim k_index As Integer = 0
        For Each k_section As KBotSidePanelSection In _sections
            If Not k_section.Visible Then Continue For
            If StripSlot(k_index).Contains(k_point) Then Return k_index
            k_index += 1
        Next
        Return -1
    End Function

    Private Function BarButtonHit(k_point As Point) As Boolean
        Return BarRect().Contains(k_point) AndAlso (_collapsed OrElse k_point.X >= Width - Px(32))
    End Function

    Protected Overrides Sub OnMouseMove(k_e As MouseEventArgs)
        MyBase.OnMouseMove(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            EnsureLayout()
            Dim k_owner As KBotSidePanelSection = Nothing
            Dim k_row As KBotSidePanelRow = RowAt(k_e.Location, k_owner)
            Dim k_header As KBotSidePanelSection = SectionHeaderAt(k_e.Location)
            Dim k_bar As Boolean = BarButtonHit(k_e.Location)
            Dim k_slot As Integer = SlotAt(k_e.Location)
            If ReferenceEquals(k_row, _hotRow) AndAlso ReferenceEquals(k_header, _hotHeader) AndAlso k_bar = _hotBar AndAlso k_slot = _hotSlot Then Return
            Dim k_rowChanged As Boolean = Not ReferenceEquals(k_row, _hotRow)
            _hotRow = k_row
            _hotHeader = k_header
            _hotBar = k_bar
            _hotSlot = k_slot
            Cursor = If(k_bar OrElse k_header IsNot Nothing OrElse k_slot >= 0 OrElse (k_row IsNot Nothing AndAlso Not String.IsNullOrEmpty(k_row.Key)),
                        Cursors.Hand, Cursors.Default)
            Invalidate()
            If k_rowChanged Then ShowRowTip(k_row)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSidePanel.OnMouseMove", ex)
        End Try
    End Sub

    Private Sub ShowRowTip(k_row As KBotSidePanelRow)
        If _tooltip Is Nothing Then Return
        _tooltip.HideNow()
        If k_row Is Nothing OrElse String.IsNullOrEmpty(k_row.ToolTipText) Then Return
        Dim k_view As Rectangle = ToView(k_row.Bounds)
        _tooltip.ShowAt(Me, New KBotToolTipContent() With {.Text = k_row.ToolTipText}, PointToScreen(New Point(k_view.Left + Px(SidePad), k_view.Bottom)))
    End Sub

    Protected Overrides Sub OnMouseLeave(k_e As EventArgs)
        MyBase.OnMouseLeave(k_e)
        Try
            _hotRow = Nothing
            _hotHeader = Nothing
            _hotBar = False
            _hotSlot = -1
            Cursor = Cursors.Default
            _tooltip?.HideNow()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSidePanel.OnMouseLeave", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseWheel(k_e As MouseEventArgs)
        MyBase.OnMouseWheel(k_e)
        Try
            If _collapsed Then Return
            EnsureLayout()
            Dim k_max As Integer = MaxScroll()
            If k_max <= 0 Then Return
            Dim k_new As Integer = Math.Max(0, Math.Min(k_max, _scroll - Math.Sign(k_e.Delta) * Px(48)))
            If k_new = _scroll Then Return
            _scroll = k_new
            _hotRow = Nothing
            _tooltip?.HideNow()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSidePanel.OnMouseWheel", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseClick(k_e As MouseEventArgs)
        MyBase.OnMouseClick(k_e)
        Try
            If KBotDesignTime.IsDesignTime(Me) OrElse k_e.Button <> MouseButtons.Left Then Return
            EnsureLayout()
            _tooltip?.HideNow()
            If BarButtonHit(k_e.Location) Then
                Collapsed = Not _collapsed
                Return
            End If
            If _collapsed Then
                If SlotAt(k_e.Location) >= 0 Then Collapsed = False
                Return
            End If
            Dim k_header As KBotSidePanelSection = SectionHeaderAt(k_e.Location)
            If k_header IsNot Nothing Then
                k_header.Collapsed = Not k_header.Collapsed
                Return
            End If
            Dim k_owner As KBotSidePanelSection = Nothing
            Dim k_row As KBotSidePanelRow = RowAt(k_e.Location, k_owner)
            If k_row IsNot Nothing AndAlso Not String.IsNullOrEmpty(k_row.Key) Then
                RaiseEvent RowClicked(Me, New KBotSidePanelRowClickedEventArgs(k_owner, k_row))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotSidePanel.OnMouseClick", ex)
        End Try
    End Sub

End Class
