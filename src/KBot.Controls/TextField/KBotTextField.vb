Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The single-line form field: a borderless <c>TextBox</c> inside a frame that paints a rounded
''' outline (accent colour and its own width while focused), with per-side air around the text
''' and, for passwords, a reveal eye drawn with GDI+ at the right end.
'''
''' <para>The frame is NOT selectable (Tab lands straight on the inner box). Because the frame
''' never receives focus, a <c>KeyDown</c> on it would never fire; the inner box's <c>KeyDown</c>
''' is re-raised as <see cref="FieldKeyDown"/>.</para>
'''
''' <para>Everything an operator can author is in the property grid under the «K-BOT» groups:
''' the text surface (forwarded to the inner box), the paddings, the outline widths and the
''' colours. Colours left empty come from the theme (C1); every pixel number is a logical px at
''' 96 dpi and is scaled at paint/layout time (C2); a fresh drop writes zero property lines into
''' the host (C4).</para>
''' </summary>
<ToolboxItem(True)>
<DefaultProperty("Text")>
<DefaultEvent("TextChanged")>
Public NotInheritable Class KBotTextField
    Inherits Control
    Implements IThemedControl

    Private ReadOnly _inner As New TextBox()

    ' -- Theme colours (what the palette said last) -------------------------------------------
    Private _fillTheme As Color = Color.White
    Private _textTheme As Color = Color.Black
    Private _borderTheme As Color = Color.Gray
    Private _focusTheme As Color = Color.DodgerBlue
    Private _glyphTheme As Color = Color.Gray

    ' -- Operator choices (Empty = follow the theme) -------------------------------------------
    Private _fillPinned As Color = Color.Empty
    Private _borderPinned As Color = Color.Empty
    Private _focusPinned As Color = Color.Empty
    Private _glyphPinned As Color = Color.Empty
    Private _backPinned As Boolean
    Private _forePinned As Boolean
    Private _fontPinned As Boolean

    ' -- Geometry, LOGICAL px. The defaults are the numbers that used to be hardcoded. --------
    Private Shared ReadOnly DefaultTextPadding As New Padding(12, 7, 12, 7)
    Private _textPadding As Padding = DefaultTextPadding
    Private _eyeWidth As Integer = 30
    Private _borderWidth As Integer = 1
    Private _focusBorderWidth As Integer = 2
    Private _cornerRadius As Integer = -1            ' -1 = from the theme (Style.CornerRadius)

    ' -- State ---------------------------------------------------------------------------------
    Private _passwordMode As Boolean = False
    Private _revealed As Boolean = False
    Private _focused As Boolean = False
    Private _hoverEye As Boolean = False

    ''' <summary>The inner box's KeyDown, re-raised on the frame (same signature).</summary>
    Public Event FieldKeyDown As KeyEventHandler

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)
        SetStyle(ControlStyles.Selectable, False)
        ' Through MyBase on purpose: the overrides below would mark these as "operator pinned"
        ' and the designer would then write them into every host.
        MyBase.TabStop = False
        MyBase.BackColor = Color.Transparent

        _inner.BorderStyle = BorderStyle.None
        _inner.Multiline = False
        AddHandler _inner.Enter, AddressOf OnInnerEnter
        AddHandler _inner.Leave, AddressOf OnInnerLeave
        AddHandler _inner.KeyDown, AddressOf OnInnerKeyDown
        ' The frame's own TextChanged must fire too. `Text` is delegated to the inner box, so
        ' without this forward a `Handles txtX.TextChanged` written in a designer-authored form
        ' would silently do nothing.
        AddHandler _inner.TextChanged, AddressOf OnInnerTextChanged
        Controls.Add(_inner)
    End Sub

    ''' <summary>
    ''' The starting size comes from HERE, not from a write to <c>Height</c> in the constructor:
    ''' a write makes <c>ShouldSerializeSize</c> answer True and the designer prints a
    ''' <c>Size</c> line in every host.
    ''' </summary>
    Protected Overrides ReadOnly Property DefaultSize As Size
        Get
            Return New Size(200, 36)
        End Get
    End Property

    ' ===== Text surface (forwarded to the inner box) =========================================

    <Category("K-BOT")>
    <Description("Textul câmpului.")>
    <Browsable(True)>
    <EditorBrowsable(EditorBrowsableState.Always)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
    Public Overrides Property Text As String
        Get
            Return _inner.Text
        End Get
        Set(value As String)
            _inner.Text = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Textul palid arătat cât timp câmpul e gol.")>
    <DefaultValue("")>
    Public Property PlaceholderText As String
        Get
            Return _inner.PlaceholderText
        End Get
        Set(value As String)
            _inner.PlaceholderText = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Câmp de parolă: ascunde textul și arată ochiul de dezvăluire în dreapta.")>
    <DefaultValue(False)>
    Public Property UseSystemPasswordChar As Boolean
        Get
            Return _passwordMode
        End Get
        Set(value As Boolean)
            _passwordMode = value
            _revealed = False
            _inner.UseSystemPasswordChar = value
            LayoutBox()
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Lungimea maximă a textului.")>
    <DefaultValue(32767)>
    Public Property MaxLength As Integer
        Get
            Return _inner.MaxLength
        End Get
        Set(value As Integer)
            _inner.MaxLength = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Câmpul nu poate fi editat.")>
    <DefaultValue(False)>
    Public Property [ReadOnly] As Boolean
        Get
            Return _inner.ReadOnly
        End Get
        Set(value As Boolean)
            _inner.ReadOnly = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Alinierea textului în câmp.")>
    <DefaultValue(HorizontalAlignment.Left)>
    Public Property TextAlign As HorizontalAlignment
        Get
            Return _inner.TextAlign
        End Get
        Set(value As HorizontalAlignment)
            _inner.TextAlign = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Forțează literele mari sau mici la tastare.")>
    <DefaultValue(CharacterCasing.Normal)>
    Public Property CharacterCasing As CharacterCasing
        Get
            Return _inner.CharacterCasing
        End Get
        Set(value As CharacterCasing)
            _inner.CharacterCasing = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Selecția nu se mai vede când câmpul pierde focusul.")>
    <DefaultValue(True)>
    Public Property HideSelection As Boolean
        Get
            Return _inner.HideSelection
        End Get
        Set(value As Boolean)
            _inner.HideSelection = value
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Scurtăturile de tastatură ale câmpului (Ctrl+C, Ctrl+V, Ctrl+A…) și meniul contextual.")>
    <DefaultValue(True)>
    Public Property ShortcutsEnabled As Boolean
        Get
            Return _inner.ShortcutsEnabled
        End Get
        Set(value As Boolean)
            _inner.ShortcutsEnabled = value
        End Set
    End Property

    ''' <summary>The inner box, for what is not surfaced (SelectAll, SelectionStart...).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property InnerTextBox As TextBox
        Get
            Return _inner
        End Get
    End Property

    ''' <summary>
    ''' Focuses the inner box (the frame is not selectable). Uses Select, not Focus: Focus is
    ''' a no-op while the form is not yet visible (Load), whereas Select records the box as
    ''' the form's ActiveControl so it gets the caret when the window is first activated.
    ''' </summary>
    Public Sub FocusInput()
        _inner.Select()
    End Sub

    ' ===== Paddings ===========================================================================

    ''' <summary>
    ''' The air around the text, each side on its own, px @96dpi. Left/Right push the text away
    ''' from the outline (and from the eye); Top/Bottom squeeze the strip the one line of text
    ''' is centred in, and are what <see cref="GetPreferredSize"/> counts above and below it.
    ''' </summary>
    <Category("K-BOT: Paddings")>
    <Description("Aerul din jurul textului: stânga, sus, dreapta, jos (px logici). Sus/jos intră în înălțimea preferată.")>
    Public Property TextPadding As Padding
        Get
            Return _textPadding
        End Get
        Set(value As Padding)
            Dim clamped As Padding = ClampPad(value)
            If _textPadding = clamped Then Return
            _textPadding = clamped
            LayoutBox()
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeTextPadding() As Boolean
        Return _textPadding <> DefaultTextPadding
    End Function
    Public Sub ResetTextPadding()
        TextPadding = DefaultTextPadding
    End Sub

    ''' <summary>The band the reveal eye owns at the right end (password mode only), px @96dpi.</summary>
    <Category("K-BOT: Paddings")>
    <Description("Lățimea benzii ochiului de parolă, la capătul din dreapta (px logici).")>
    <DefaultValue(30)>
    Public Property EyeWidth As Integer
        Get
            Return _eyeWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If _eyeWidth = clamped Then Return
            _eyeWidth = clamped
            LayoutBox()
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' <c>Control.Padding</c> does nothing on this frame (<see cref="TextPadding"/> is the one
    ''' that works) -- hidden from the grid so the two cannot be confused. Same treatment as
    ''' <c>TextBoxBase</c> gives it.
    ''' </summary>
    <Browsable(False)>
    <EditorBrowsable(EditorBrowsableState.Never)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property Padding As Padding
        Get
            Return MyBase.Padding
        End Get
        Set(value As Padding)
            MyBase.Padding = value
        End Set
    End Property

    ''' <summary>
    ''' The frame is never the tab target (the inner box is), so <c>TabStop</c> has no meaning
    ''' here. Shadowed with the right default so hosts stop printing <c>TabStop = False</c>.
    ''' </summary>
    <Browsable(False)>
    <EditorBrowsable(EditorBrowsableState.Never)>
    <DefaultValue(False)>
    Public Shadows Property TabStop As Boolean
        Get
            Return MyBase.TabStop
        End Get
        Set(value As Boolean)
            MyBase.TabStop = value
        End Set
    End Property

    ' ===== Outline ============================================================================

    <Category("K-BOT")>
    <Description("Grosimea chenarului (px logici). 0 = fără chenar.")>
    <DefaultValue(1)>
    Public Property BorderWidth As Integer
        Get
            Return _borderWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If _borderWidth = clamped Then Return
            _borderWidth = clamped
            Invalidate()
        End Set
    End Property

    <Category("K-BOT")>
    <Description("Grosimea chenarului cât timp câmpul are focus (px logici).")>
    <DefaultValue(2)>
    Public Property FocusBorderWidth As Integer
        Get
            Return _focusBorderWidth
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(0, value)
            If _focusBorderWidth = clamped Then Return
            _focusBorderWidth = clamped
            Invalidate()
        End Set
    End Property

    ''' <summary>Corner radius of the frame, in logical px. -1 = from the theme (Style.CornerRadius).</summary>
    <Category("K-BOT")>
    <Description("Raza colțurilor (px logici). -1 = din temă, 0 = colțuri drepte.")>
    <DefaultValue(-1)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            Dim clamped As Integer = Math.Max(-1, value)
            If _cornerRadius = clamped Then Return
            _cornerRadius = clamped
            Invalidate()
        End Set
    End Property

    ' ===== Colours (Empty = from the theme) ===================================================

    <Category("K-BOT: Culori")>
    <Description("Umplerea câmpului; goală = InputBackColor din temă.")>
    Public Property FillColor As Color
        Get
            Return If(_fillPinned <> Color.Empty, _fillPinned, _fillTheme)
        End Get
        Set(value As Color)
            _fillPinned = value
            ApplyInnerColors()
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFillColor() As Boolean
        Return _fillPinned <> Color.Empty
    End Function
    Public Sub ResetFillColor()
        FillColor = Color.Empty
    End Sub

    <Category("K-BOT: Culori")>
    <Description("Culoarea chenarului; goală = InputBorderColor din temă.")>
    Public Property BorderColor As Color
        Get
            Return If(_borderPinned <> Color.Empty, _borderPinned, _borderTheme)
        End Get
        Set(value As Color)
            _borderPinned = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeBorderColor() As Boolean
        Return _borderPinned <> Color.Empty
    End Function
    Public Sub ResetBorderColor()
        BorderColor = Color.Empty
    End Sub

    <Category("K-BOT: Culori")>
    <Description("Culoarea chenarului cât timp câmpul are focus; goală = accentul temei.")>
    Public Property FocusBorderColor As Color
        Get
            Return If(_focusPinned <> Color.Empty, _focusPinned, _focusTheme)
        End Get
        Set(value As Color)
            _focusPinned = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeFocusBorderColor() As Boolean
        Return _focusPinned <> Color.Empty
    End Function
    Public Sub ResetFocusBorderColor()
        FocusBorderColor = Color.Empty
    End Sub

    <Category("K-BOT: Culori")>
    <Description("Culoarea ochiului de parolă; goală = TextDimColor din temă.")>
    Public Property EyeColor As Color
        Get
            Return If(_glyphPinned <> Color.Empty, _glyphPinned, _glyphTheme)
        End Get
        Set(value As Color)
            _glyphPinned = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeEyeColor() As Boolean
        Return _glyphPinned <> Color.Empty
    End Function
    Public Sub ResetEyeColor()
        EyeColor = Color.Empty
    End Sub

    ' ===== Ambient properties: own flag, so the designer does not freeze them ================

    ''' <summary>The frame's own background (what shows OUTSIDE the rounded outline). Default transparent.</summary>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backPinned = True
            MyBase.BackColor = value
            Invalidate()
        End Set
    End Property
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backPinned
    End Function
    Public Overrides Sub ResetBackColor()
        _backPinned = False
        MyBase.BackColor = Color.Transparent
        Invalidate()
    End Sub

    ''' <summary>The text colour; unpinned = InputTextColor from the theme.</summary>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _forePinned = True
            MyBase.ForeColor = value
            _inner.ForeColor = value
        End Set
    End Property
    Public Function ShouldSerializeForeColor() As Boolean
        Return _forePinned
    End Function
    Public Overrides Sub ResetForeColor()
        _forePinned = False
        MyBase.ResetForeColor()
        ApplyInnerColors()
    End Sub

    Public Overrides Property Font As Font
        Get
            Return MyBase.Font
        End Get
        Set(value As Font)
            _fontPinned = True
            MyBase.Font = value
        End Set
    End Property
    Public Function ShouldSerializeFont() As Boolean
        Return _fontPinned
    End Function
    Public Overrides Sub ResetFont()
        _fontPinned = False
        MyBase.ResetFont()
    End Sub

    ' ===== Layout =============================================================================

    ' Negative air is not air: C3 says clamp a number, not throw for it.
    Private Shared Function ClampPad(p As Padding) As Padding
        Return New Padding(Math.Max(0, p.Left), Math.Max(0, p.Top),
                           Math.Max(0, p.Right), Math.Max(0, p.Bottom))
    End Function

    ' Logical px -> device px, one side at a time (C2).
    Private Function ScalePad(p As Padding) As Padding
        Return New Padding(ThemeShapes.ScaleDpi(Me, p.Left), ThemeShapes.ScaleDpi(Me, p.Top),
                           ThemeShapes.ScaleDpi(Me, p.Right), ThemeShapes.ScaleDpi(Me, p.Bottom))
    End Function

    Private Function EyeBandPx() As Integer
        Return If(_passwordMode, ThemeShapes.ScaleDpi(Me, _eyeWidth), 0)
    End Function

    Private Function EyeRect() As Rectangle
        Dim w As Integer = EyeBandPx()
        Return New Rectangle(Width - w, 0, w, Height)
    End Function

    ' The inner box is placed by hand: Left/Right air on the sides (plus the eye band), and
    ' centred in the strip Top/Bottom leave, which is how the text sits high or low in a tall field.
    Private Sub LayoutBox()
        Dim pad As Padding = ScalePad(_textPadding)
        Dim left As Integer = pad.Left
        Dim w As Integer = Math.Max(0, Width - pad.Horizontal - EyeBandPx())
        Dim strip As Integer = Math.Max(0, Height - pad.Vertical)
        Dim top As Integer = pad.Top + Math.Max(0, (strip - _inner.Height) \ 2)
        _inner.SetBounds(left, top, w, _inner.Height)
    End Sub

    ''' <summary>
    ''' How tall the frame must be for one line of text on the CURRENT font plus the air above
    ''' and below it.
    '''
    ''' <para>The frame does NOT resize itself -- its height is the designer's, like any control
    ''' of the house. But somewhere there has to be an honest answer to «does the text still
    ''' fit?», because a scheme can change the base font and a frame authored on the system
    ''' font is left with the letters clipped. That place is <c>GetPreferredSize</c>:
    ''' <c>ThemeTableFit</c> reads it to grow the table row the frame sits in, by exactly what
    ''' does not fit.</para>
    ''' </summary>
    Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
        Try
            Dim pad As Padding = ScalePad(_textPadding)
            Return New Size(AuthoredWidthDemand(), _inner.PreferredHeight + pad.Vertical)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.GetPreferredSize", ex)
            Return MyBase.GetPreferredSize(proposedSize)
        End Try
    End Function

    ' The WIDTH half of the answer (slice 0066). A text box has no intrinsic width: its width is
    ' the designer's, unless docking stretches it to the cell -- and then the current Width IS
    ' the cell, so reporting it would tell the table «I need exactly what you gave me» and a
    ' fixed column could never come back from a growth (measured: a 180px column at 1.65 stayed
    ' at 297 under Fixed 100% because the field in it "wanted" 297). Stretched: nothing; else
    ' the authored width, as the platform scaled it.
    Private Function AuthoredWidthDemand() As Integer
        Dim stretched As Boolean = Dock = DockStyle.Fill OrElse Dock = DockStyle.Top OrElse Dock = DockStyle.Bottom
        Return If(stretched, 0, Width)
    End Function

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        Try
            LayoutBox()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnResize", ex)
        End Try
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            _inner.Font = Font
            LayoutBox()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnFontChanged", ex)
        End Try
    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            ' Only now does DeviceDpi tell the truth (house rule).
            LayoutBox()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnHandleCreated", ex)
        End Try
    End Sub

    ' ===== Theme ==============================================================================

    ''' <summary>Re-applies the scheme's colours; a colour the operator pinned keeps winning.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            _fillTheme = p.InputBackColor
            _textTheme = p.InputTextColor
            _borderTheme = p.InputBorderColor
            _focusTheme = p.AccentColor
            _glyphTheme = p.TextDimColor
            ApplyInnerColors()
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.ApplyTheme", ex)
        End Try
    End Sub

    ' The inner box takes the fill and, unless the operator pinned ForeColor, the theme's text colour.
    Private Sub ApplyInnerColors()
        _inner.BackColor = FillColor
        If Not _forePinned Then
            MyBase.ForeColor = _textTheme
            _inner.ForeColor = _textTheme
        End If
    End Sub

    ' The effective radius, in DPI-scaled px: the property if the operator set it, else the theme's.
    Private Function EffectiveRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ' ===== Painting: rounded fill + outline (accent while focused) + optional eye ==============

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim radius As Integer = EffectiveRadius()
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)
            If rect.Width <= 0 OrElse rect.Height <= 0 Then Return
            Using path As GraphicsPath = ThemeShapes.RoundedRect(rect, radius)
                Using fill As New SolidBrush(FillColor)
                    g.FillPath(fill, path)
                End Using
                Dim thickness As Integer = ThemeShapes.ScaleDpi(Me, If(_focused, _focusBorderWidth, _borderWidth))
                If thickness > 0 Then
                    Using pen As New Pen(If(_focused, FocusBorderColor, BorderColor), thickness)
                        g.DrawPath(pen, path)
                    End Using
                End If
            End Using

            If _passwordMode Then DrawEye(g, EyeRect())
        Catch ex As Exception
            If Not KBotDesignTime.IsDesignTime(Me) Then GlobalErrorLog.Write("KBotTextField.OnPaint", ex)
        End Try
    End Sub

    ' Eye drawn with GDI+: ellipse + pupil; a slash across it while the password is revealed.
    Private Sub DrawEye(g As Graphics, area As Rectangle)
        Dim col As Color = If(_hoverEye, FocusBorderColor, EyeColor)
        Dim cx As Single = area.Left + area.Width / 2.0F
        Dim cy As Single = area.Top + area.Height / 2.0F
        Dim ew As Single = ThemeShapes.ScaleDpi(Me, 16)
        Dim eh As Single = ThemeShapes.ScaleDpi(Me, 9)
        Using pen As New Pen(col, ThemeShapes.ScaleDpi(Me, 1))
            g.DrawEllipse(pen, cx - ew / 2.0F, cy - eh / 2.0F, ew, eh)
            Dim pr As Single = ThemeShapes.ScaleDpi(Me, 2)
            Using b As New SolidBrush(col)
                g.FillEllipse(b, cx - pr, cy - pr, pr * 2, pr * 2)
            End Using
            If _revealed Then
                g.DrawLine(pen, cx - ew / 2.0F, cy + eh / 2.0F, cx + ew / 2.0F, cy - eh / 2.0F)
            End If
        End Using
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            If _passwordMode AndAlso EyeRect().Contains(e.Location) Then
                _revealed = Not _revealed
                _inner.UseSystemPasswordChar = _passwordMode AndAlso Not _revealed
                Invalidate()
            End If
            _inner.Focus()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotTextField.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Dim over As Boolean = _passwordMode AndAlso EyeRect().Contains(e.Location)
        If over <> _hoverEye Then
            _hoverEye = over
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        If _hoverEye Then
            _hoverEye = False
            Invalidate()
        End If
    End Sub

    Private Sub OnInnerEnter(sender As Object, e As EventArgs)
        _focused = True
        Invalidate()
    End Sub

    Private Sub OnInnerLeave(sender As Object, e As EventArgs)
        _focused = False
        Invalidate()
    End Sub

    Private Sub OnInnerKeyDown(sender As Object, e As KeyEventArgs)
        RaiseEvent FieldKeyDown(Me, e)
    End Sub

    ' The inner text changed => so did the frame's, because `Text` is delegated there. The
    ' placeholder repaints the same way, so the repaint lives here too.
    Private Sub OnInnerTextChanged(sender As Object, e As EventArgs)
        OnTextChanged(EventArgs.Empty)
        Invalidate()
    End Sub

End Class
