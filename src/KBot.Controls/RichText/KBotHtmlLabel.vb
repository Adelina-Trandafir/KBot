Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' A <see cref="KBotLabel"/> whose text is HTML (slice 000T-06): <c>&lt;b&gt;</c>, <c>&lt;br&gt;</c>, <c>div</c>,
''' colours, lists... see <see cref="KBotHtmlText"/> for the exact subset. The text goes in
''' <see cref="Html"/>, not in <c>Text</c> (which stays empty, so the base label draws nothing of its own and
''' keeps only the background, the border and the theme colours). A text without any of the supported tags is
''' shown as plain text, line breaks included.
'''
''' <para>Everything else is the label's: border, theme, <c>ForeColor</c> as the colour of text that has none of
''' its own. The runs are rebuilt when the text, the font, the colour or the scheme changes, the layout when the
''' width does. <see cref="GetPreferredSize"/> answers the height the text needs at the proposed width, which is
''' what a host that sizes itself to the text (the tutorial bubble) asks.</para>
''' </summary>
<ToolboxItem(True)>
Public Class KBotHtmlLabel
    Inherits KBotLabel

    Private _html As String = String.Empty
    Private ReadOnly _fonts As New Dictionary(Of String, Font)()
    Private _runs As List(Of KBotRichText.RichRun)
    Private _builtPalette As ThemePalette
    Private _builtColor As Color
    Private _builtFont As Font
    Private _layout As KBotRichText.RichLayout
    Private _layoutWidth As Integer = -1

    ''' <summary>The text, as HTML (or plain text when it holds none of the supported tags).</summary>
    <Category("K-BOT")>
    <Description("Textul ca HTML (b, i, u, br, div, p, span, font, culori, liste). Fara etichete = text simplu.")>
    <DefaultValue("")>
    <Editor(GetType(System.ComponentModel.Design.MultilineStringEditor), GetType(System.Drawing.Design.UITypeEditor))>
    Public Property Html As String
        Get
            Return _html
        End Get
        Set(k_value As String)
            Dim k_new As String = If(k_value, String.Empty)
            If String.Equals(_html, k_new, StringComparison.Ordinal) Then Return
            _html = k_new
            DropRuns()
            Invalidate()
        End Set
    End Property

    Private Sub DropRuns()
        _runs = Nothing
        _layoutWidth = -1
    End Sub

    Private Sub DisposeFonts()
        For Each k_font As Font In _fonts.Values
            k_font.Dispose()
        Next
        _fonts.Clear()
    End Sub

    Protected Overrides Sub Dispose(k_disposing As Boolean)
        If k_disposing Then
            DropRuns()
            DisposeFonts()
        End If
        MyBase.Dispose(k_disposing)
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        DropRuns()
    End Sub

    Protected Overrides Sub OnForeColorChanged(e As EventArgs)
        MyBase.OnForeColorChanged(e)
        DropRuns()
    End Sub

    ' Rebuilt when the text, the base font / colour or the scheme (theme colour words) is not the one it was
    ' built for. Fonts of the old runs go with them.
    Private Sub EnsureRuns()
        Dim k_scheme As ThemeScheme = ThemeManager.Current
        Dim k_palette As ThemePalette = If(k_scheme Is Nothing, Nothing, k_scheme.Palette)
        If _runs IsNot Nothing AndAlso ReferenceEquals(_builtPalette, k_palette) AndAlso
           _builtColor = ForeColor AndAlso ReferenceEquals(_builtFont, Font) Then Return
        _layoutWidth = -1
        _runs = Nothing
        DisposeFonts()
        _runs = KBotHtmlText.ToRuns(_html, Font, ForeColor, k_palette, _fonts)
        _builtPalette = k_palette
        _builtColor = ForeColor
        _builtFont = Font
    End Sub

    Private Function BorderPx() As Integer
        Return ThemeShapes.ScaleDpi(Me, BorderWidth)
    End Function

    Private Function ContentRectangle() As Rectangle
        Dim k_border As Integer = BorderPx()
        Return New Rectangle(k_border + Padding.Left, k_border + Padding.Top,
                             Math.Max(0, Width - 2 * k_border - Padding.Horizontal),
                             Math.Max(0, Height - 2 * k_border - Padding.Vertical))
    End Function

    Private Function LayoutAt(k_graphics As Graphics, k_width As Integer) As KBotRichText.RichLayout
        If _layoutWidth <> k_width OrElse _layout.Lines Is Nothing Then
            _layout = KBotRichText.Layout(_runs, k_graphics, k_width)
            _layoutWidth = k_width
        End If
        Return _layout
    End Function

    ''' <summary>The width asked for (or the label's own, when none is asked) and the height the text needs there.</summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Try
            If _html.Length = 0 Then Return MyBase.GetPreferredSize(proposedSize)
            EnsureRuns()
            Dim k_width As Integer = proposedSize.Width
            If k_width <= 0 OrElse k_width = Integer.MaxValue Then k_width = If(MaximumSize.Width > 0, MaximumSize.Width, Width)
            Dim k_border As Integer = BorderPx()
            Dim k_inner As Integer = Math.Max(1, k_width - 2 * k_border - Padding.Horizontal)
            Using k_graphics As Graphics = If(IsHandleCreated, Graphics.FromHwnd(Handle), Graphics.FromHwnd(IntPtr.Zero))
                Dim k_lay As KBotRichText.RichLayout = LayoutAt(k_graphics, k_inner)
                Return New Size(k_width, k_lay.Height + 2 * k_border + Padding.Vertical)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHtmlLabel.GetPreferredSize", ex)
            Return MyBase.GetPreferredSize(proposedSize)
        End Try
    End Function

    ''' <summary>
    ''' Slice 000T-09: the user clicked a link of the text (<c>&lt;link tutorial="id"&gt;</c>); the argument is the id.
    ''' Raised on the left button's release over the link.
    ''' </summary>
    ''' <summary>
    ''' The size the text needs when wrapped at <paramref name="k_maxWidth"/> -- as WIDE as its widest line
    ''' needs, not the whole proposed width (what <see cref="GetPreferredSize"/> answers). A dialog that wants
    ''' to hug its message sizes itself with this. Empty size when there is no <see cref="Html"/>.
    ''' </summary>
    Public Function MeasureHtml(k_maxWidth As Integer) As Size
        Try
            If _html.Length = 0 Then Return Size.Empty
            EnsureRuns()
            Dim k_border As Integer = BorderPx()
            Dim k_frame As Integer = 2 * k_border + Padding.Horizontal
            Dim k_inner As Integer = Math.Max(1, k_maxWidth - k_frame)
            Using k_graphics As Graphics = If(IsHandleCreated, Graphics.FromHwnd(Handle), Graphics.FromHwnd(IntPtr.Zero))
                Dim k_lay As KBotRichText.RichLayout = LayoutAt(k_graphics, k_inner)
                Return New Size(Math.Min(k_maxWidth, k_lay.Width + k_frame + 2), k_lay.Height + 2 * k_border + Padding.Vertical)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHtmlLabel.MeasureHtml", ex)
            Throw
        End Try
    End Function

    Public Event LinkClicked(k_target As String)

    Private _overLink As Boolean

    ' The link under a point of the label (client coordinates), or Nothing.
    Private Function LinkAtPoint(k_point As Point) As String
        If _html.Length = 0 Then Return Nothing
        EnsureRuns()
        Dim k_area As Rectangle = ContentRectangle()
        If k_area.Width <= 0 OrElse k_area.Height <= 0 OrElse Not k_area.Contains(k_point) Then Return Nothing
        Using k_graphics As Graphics = Graphics.FromHwnd(Handle)
            Return KBotRichText.LinkAt(k_graphics, LayoutAt(k_graphics, k_area.Width), k_area, TextAlign, k_point)
        End Using
    End Function

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            Dim k_over As Boolean = LinkAtPoint(e.Location) IsNot Nothing
            If k_over = _overLink Then Return
            _overLink = k_over
            Cursor = If(k_over, Cursors.Hand, Cursors.Default)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHtmlLabel.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _overLink Then
            _overLink = False
            Cursor = Cursors.Default
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            If e.Button <> MouseButtons.Left OrElse KBotDesignTime.IsDesignTime(Me) Then Return
            Dim k_target As String = LinkAtPoint(e.Location)
            If k_target IsNot Nothing Then RaiseEvent LinkClicked(k_target)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHtmlLabel.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            MyBase.OnPaint(e)
            If _html.Length = 0 Then Return
            EnsureRuns()
            Dim k_area As Rectangle = ContentRectangle()
            If k_area.Width <= 0 OrElse k_area.Height <= 0 Then Return
            Dim k_lay As KBotRichText.RichLayout = LayoutAt(e.Graphics, k_area.Width)
            Dim k_state As Drawing2D.GraphicsState = e.Graphics.Save()
            Try
                e.Graphics.SetClip(k_area)
                ' Text on an opaque surface: ClearType, like TextRenderer, instead of GDI+'s soft grey default.
                e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                KBotRichText.Draw(e.Graphics, k_lay, k_area, TextAlign)
            Finally
                e.Graphics.Restore(k_state)
            End Try
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotHtmlLabel.OnPaint", ex)
        End Try
    End Sub

End Class
