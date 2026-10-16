Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports KBot.Theming

''' <summary>
''' Slice 00EF-34 -- the coloured vertical ICON BAR of <see cref="CustomPopup"/> (as the Windows 7 menus had): the icons of the rows
''' sit on a band along the whole height of the popup, in a colour a little off the background. The bar can be on the
''' <see cref="IconBarSide"/> left or right (set in the designer file of the host or in code); with the bar on the right the text of
''' the rows is aligned right and the check mark / chevron move to the left end. The bar exists only while some row has an icon.
''' </summary>
Partial Public Class CustomPopup

    Private _iconBarVisible As Boolean = True
    Private _iconBarSide As PopupIconBarSide = PopupIconBarSide.Left
    Private _iconBarColor As Color = Color.Empty
    Private _autoIconBar As Color = Color.FromArgb(236, 241, 248)

    ''' <summary>Draw the icons on a coloured bar. Off = the icons sit on the plain background, as before.</summary>
    <Category("K-BOT Popup")>
    <Description("Draw the icons of the rows on a coloured vertical bar.")>
    <DefaultValue(True)>
    Public Property IconBarVisible As Boolean
        Get
            Return _iconBarVisible
        End Get
        Set(value As Boolean)
            If value = _iconBarVisible Then Return
            _iconBarVisible = value
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>The side of the bar. Right = the text of the rows is aligned right too.</summary>
    <Category("K-BOT Popup")>
    <Description("Side of the icon bar. Right = the text of the rows is aligned right.")>
    <DefaultValue(PopupIconBarSide.Left)>
    Public Property IconBarSide As PopupIconBarSide
        Get
            Return _iconBarSide
        End Get
        Set(value As PopupIconBarSide)
            If Not [Enum].IsDefined(GetType(PopupIconBarSide), value) Then
                Throw New ArgumentOutOfRangeException(NameOf(value), value, "Latura barei de pictograme nu e cunoscută.")
            End If
            If value = _iconBarSide Then Return
            _iconBarSide = value
            InvalidateLayout()
        End Set
    End Property

    ''' <summary>The colour of the bar. Empty = from the theme (the background with a little of the accent).</summary>
    <Category("K-BOT Popup")>
    <Description("Colour of the icon bar. Empty = from the theme.")>
    Public Property IconBarColor As Color
        Get
            Return _iconBarColor
        End Get
        Set(value As Color)
            _iconBarColor = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeIconBarColor() As Boolean
        Return _iconBarColor <> Color.Empty
    End Function

    Private Sub ResetIconBarColor()
        IconBarColor = Color.Empty
    End Sub

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveIconBarColor As Color
        Get
            Return If(_iconBarColor = Color.Empty, _autoIconBar, _iconBarColor)
        End Get
    End Property

    ' ── Geometry shared by the painting, the sliders, the input rows and the capture redaction ─────

    ''' <summary>True while the bar is shown on the right (then the text is right-aligned).</summary>
    Friend Function BarOnRight() As Boolean
        Return _iconBarVisible AndAlso _iconBarSide = PopupIconBarSide.Right AndAlso IconGutter() > 0
    End Function

    ''' <summary>Width of the bar (0 = no bar: switched off, or no row has an icon).</summary>
    Friend Function IconBarWidth() As Integer
        If Not _iconBarVisible OrElse IconGutter() <= 0 Then Return 0
        Return 2 * ThemeShapes.ScaleDpi(ScaleRef, PadXLogical) + ThemeShapes.ScaleDpi(ScaleRef, _imageSize)
    End Function

    ''' <summary>What the icon band takes at the LEFT of a row, after the row padding (0 when the bar is on the right).</summary>
    Friend Function LeftInset() As Integer
        Return If(BarOnRight(), 0, IconGutter())
    End Function

    ''' <summary>What the icon band takes at the RIGHT of a row, before the row padding (0 when the bar is on the left).</summary>
    Friend Function RightInset() As Integer
        Return If(BarOnRight(), IconGutter(), 0)
    End Function

    ''' <summary>The square of the icon of the row <paramref name="k_row"/>.</summary>
    Friend Function IconRectOf(k_row As Rectangle) As Rectangle
        Dim k_padX As Integer = ThemeShapes.ScaleDpi(ScaleRef, PadXLogical)
        Dim k_side As Integer = ThemeShapes.ScaleDpi(ScaleRef, _imageSize)
        Dim k_x As Integer = If(BarOnRight(), k_row.Right - k_padX - k_side, k_row.Left + k_padX)
        Return New Rectangle(k_x, k_row.Top + (k_row.Height - k_side) \ 2, k_side, k_side)
    End Function

    ''' <summary>
    ''' The area of the text of a plain row: between the icon band and the check / chevron band. With the bar on the right the band of the
    ''' check mark is at the left end, so the text is what remains on the right.
    ''' </summary>
    Friend Function TextRectOf(k_row As Rectangle, k_checkW As Integer) As Rectangle
        Dim k_padX As Integer = ThemeShapes.ScaleDpi(ScaleRef, PadXLogical)
        Dim k_left As Integer
        Dim k_right As Integer
        If BarOnRight() Then
            k_left = k_row.Left + k_padX + k_checkW
            k_right = k_row.Right - k_padX - RightInset()
        Else
            k_left = k_row.Left + k_padX + LeftInset()
            k_right = k_row.Right - k_padX - k_checkW
        End If
        Return New Rectangle(k_left, k_row.Top, Math.Max(0, k_right - k_left), k_row.Height)
    End Function

    ''' <summary>Paints the bar over the whole height of the popup (clipped by the caller to the rounded frame).</summary>
    Private Sub DrawIconBar(k_g As Graphics)
        Dim k_w As Integer = IconBarWidth()
        If k_w <= 0 Then Return
        Dim k_x As Integer = If(BarOnRight(), ClientSize.Width - k_w, 0)
        Using k_b As New SolidBrush(EffectiveIconBarColor)
            k_g.FillRectangle(k_b, k_x, 0, k_w, ClientSize.Height)
        End Using
        ' The fine line on the inner edge that Windows 7 drew.
        Dim k_lineX As Integer = If(BarOnRight(), k_x, k_x + k_w - 1)
        Using k_pen As New Pen(EffectiveSeparatorColor)
            k_g.DrawLine(k_pen, k_lineX, 0, k_lineX, ClientSize.Height)
        End Using
    End Sub

End Class
