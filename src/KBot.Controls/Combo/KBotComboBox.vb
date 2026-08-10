Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Lista derulantă K-BOT: fața ÎNCHISĂ e pictată de noi (dreptunghi rotunjit, contur de 1 px,
''' săgeată desenată cu GDI+), iar rândurile listei sunt owner-drawn. Închide firul rămas deschis
''' din felia de tematizare — «ComboBox theming retrofit» — fiindcă un <c>ComboBox</c> obișnuit
''' ignoră <c>BackColor</c> pe fața închisă: sub Windows o desenează tema sistemului, iar pe o
''' schemă întunecată rămânea un dreptunghi alb.
'''
''' MOȘTENEȘTE <c>ComboBox</c>, nu <c>Control</c>. Deliberat: gazdele existente leagă
''' <c>DataSource</c>, <c>Items</c>, <c>SelectedItem</c>, <c>DisplayMember</c> și
''' <c>SelectedIndexChanged</c> (vezi cele două combo-uri An/SS din MainForm). O rescriere de la
''' zero ar fi însemnat reimplementarea legării de date ca să se ajungă în ACELAȘI loc.
'''
''' Contractul de culoare e cel al casei (vezi AdvancedTreeControl): <c>Color.Empty</c> = «din
''' temă», orice culoare pusă în designer câștigă. <c>BackColor</c>/<c>ForeColor</c>/<c>Font</c>
''' au steag de fixare + perechea <c>ShouldSerialize*</c>/<c>Reset*</c>, altfel designerul ar
''' îngheța în .Designer.vb valoarea scrisă de <see cref="ApplyTheme"/> și nimeni n-ar mai putea
''' distinge o alegere de un accident.
'''
''' O singură constrângere: stilul e ÎNTOTDEAUNA <c>DropDownList</c>. O casetă editabilă are un
''' EDIT nativ copil, pe care pictura noastră nu-l atinge; a-l accepta ar însemna un control care
''' arată tematizat pe jumătate. Orice altă valoare ARUNCĂ — fără no-op tăcut.
''' </summary>
<ToolboxItem(True)>
<DefaultEvent("SelectedIndexChanged")>
Public Class KBotComboBox
    Inherits ComboBox
    Implements IThemedControl

    ' ── Culorile «auto» (fallback pentru orice proprietate lăsată Empty) ──────────
    ' Valorile inițiale = look-ul light implicit, ca un host NEtematizat (bancul de probă,
    ' designerul Visual Studio) să arate rezonabil fără nicio schemă aplicată.
    Private _autoBack As Color = Color.White
    Private _autoFore As Color = Color.FromArgb(30, 30, 30)
    Private _autoBorder As Color = Color.FromArgb(170, 170, 170)
    Private _autoHover As Color = Color.FromArgb(232, 241, 251)
    Private _autoArrow As Color = Color.FromArgb(90, 90, 90)
    Private _autoSelBack As Color = Color.FromArgb(200, 220, 255)
    Private _autoSelFore As Color = Color.FromArgb(30, 30, 30)

    Private _hoverColor As Color = Color.Empty
    Private _borderColor As Color = Color.Empty
    Private _arrowColor As Color = Color.Empty
    Private _selectionBackColor As Color = Color.Empty
    Private _selectionForeColor As Color = Color.Empty
    Private _cornerRadius As Integer = -1

    ' Steaguri «operatorul a fixat asta» — vezi ShouldSerializeBackColor.
    Private _backColorPinned As Boolean = False
    Private _foreColorPinned As Boolean = False
    Private _fontPinned As Boolean = False

    Private _hovered As Boolean = False
    Private _darkList As Boolean = False

    Public Sub New()
        SetStyle(ControlStyles.UserPaint Or ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw, True)

        ' Cele două sunt fixate din constructor, deci ar fi ajuns în .Designer.vb — de aceea
        ' perechea ShouldSerialize* de mai jos le ține în afara serializării.
        MyBase.DropDownStyle = ComboBoxStyle.DropDownList
        MyBase.DrawMode = DrawMode.OwnerDrawFixed
        MyBase.FlatStyle = FlatStyle.Flat
    End Sub

    ' =====================================================================
    ' PROPRIETĂȚI MOȘTENITE CU STEAG DE FIXARE
    ' =====================================================================

    ''' <summary>Fundalul feței închise ȘI al listei derulante; nefixat aici, urmează tema.</summary>
    <Category("K-BOT Combo - Culori")>
    <Description("Fundalul feței închise și al listei; nefixat aici, urmează tema.")>
    Public Overrides Property BackColor As Color
        Get
            Return MyBase.BackColor
        End Get
        Set(value As Color)
            _backColorPinned = True
            MyBase.BackColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>
    ''' CRITIC. <c>Control.ShouldSerializeBackColor</c> răspunde True de îndată ce proprietatea a
    ''' fost SCRISĂ vreodată — inclusiv de <see cref="ApplyTheme"/>. Fără perechea asta, Visual
    ''' Studio ar scrie în formularul gazdă un «cbo.BackColor = …» pe care nimeni nu l-a ales, iar
    ''' la reîncărcare linia ar trece prin setterul de mai sus și ar FIXA culoarea pentru
    ''' totdeauna. Adevărul e steagul, nu punga de proprietăți a lui Control.
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    ' Steagul se stinge DUPĂ scrierea culorii: ResetBackColor trece prin setterul VIRTUAL, adică
    ' prin al nostru, care l-ar aprinde la loc (capcana prinsă în 0027 la ResetFont).
    Public Overrides Sub ResetBackColor()
        MyBase.BackColor = _autoBack
        _backColorPinned = False
        Invalidate()
    End Sub

    <Category("K-BOT Combo - Culori")>
    <Description("Culoarea textului; nefixată aici, urmează tema.")>
    Public Overrides Property ForeColor As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(value As Color)
            _foreColorPinned = True
            MyBase.ForeColor = value
            Invalidate()
        End Set
    End Property

    ''' <summary>Perechea lui <see cref="ShouldSerializeBackColor"/>, din același motiv.</summary>
    Public Function ShouldSerializeForeColor() As Boolean
        Return _foreColorPinned
    End Function

    Public Overrides Sub ResetForeColor()
        MyBase.ForeColor = _autoFore
        _foreColorPinned = False
        Invalidate()
    End Sub

    ''' <summary>Fontul; nefixat aici, moștenit ambiant (deci ascultător la ApplyBaseFont).</summary>
    <Category("K-BOT Combo")>
    <Description("Fontul controlului; nefixat aici, urmează fontul ambiant al schemei.")>
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

    ''' <summary>Steagul se stinge DUPĂ resetul bazei — <c>Control.ResetFont</c> scrie prin setterul virtual.</summary>
    Public Overrides Sub ResetFont()
        MyBase.ResetFont()
        _fontPinned = False
        Invalidate()
    End Sub

    ' =====================================================================
    ' PROPRIETĂȚI PROPRII (Color.Empty = «din temă»)
    ' =====================================================================

    <Category("K-BOT Combo - Culori")>
    <Description("Fundalul feței închise sub cursor. Gol = din temă.")>
    Public Property HoverColor As Color
        Get
            Return _hoverColor
        End Get
        Set(value As Color)
            _hoverColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeHoverColor() As Boolean
        Return _hoverColor <> Color.Empty
    End Function

    Public Sub ResetHoverColor()
        HoverColor = Color.Empty
    End Sub

    <Category("K-BOT Combo - Culori")>
    <Description("Conturul de 1 px al feței închise. Gol = din temă.")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(value As Color)
            _borderColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeBorderColor() As Boolean
        Return _borderColor <> Color.Empty
    End Function

    Public Sub ResetBorderColor()
        BorderColor = Color.Empty
    End Sub

    <Category("K-BOT Combo - Culori")>
    <Description("Săgeata de deschidere. Gol = din temă.")>
    Public Property ArrowColor As Color
        Get
            Return _arrowColor
        End Get
        Set(value As Color)
            _arrowColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeArrowColor() As Boolean
        Return _arrowColor <> Color.Empty
    End Function

    Public Sub ResetArrowColor()
        ArrowColor = Color.Empty
    End Sub

    <Category("K-BOT Combo - Culori")>
    <Description("Fundalul rândului evidențiat din listă. Gol = din temă.")>
    Public Property SelectionBackColor As Color
        Get
            Return _selectionBackColor
        End Get
        Set(value As Color)
            _selectionBackColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeSelectionBackColor() As Boolean
        Return _selectionBackColor <> Color.Empty
    End Function

    Public Sub ResetSelectionBackColor()
        SelectionBackColor = Color.Empty
    End Sub

    <Category("K-BOT Combo - Culori")>
    <Description("Textul rândului evidențiat din listă. Gol = din temă.")>
    Public Property SelectionForeColor As Color
        Get
            Return _selectionForeColor
        End Get
        Set(value As Color)
            _selectionForeColor = value
            Invalidate()
        End Set
    End Property

    Public Function ShouldSerializeSelectionForeColor() As Boolean
        Return _selectionForeColor <> Color.Empty
    End Function

    Public Sub ResetSelectionForeColor()
        SelectionForeColor = Color.Empty
    End Sub

    ''' <summary>Raza colțurilor feței închise, în px logici. -1 = din temă (Style.CornerRadius).</summary>
    <Category("K-BOT Combo")>
    <Description("Raza colțurilor feței închise, px @96dpi. -1 = din temă, 0 = pătrat.")>
    <DefaultValue(-1)>
    Public Property CornerRadius As Integer
        Get
            Return _cornerRadius
        End Get
        Set(value As Integer)
            _cornerRadius = Math.Max(-1, value)
            Invalidate()
        End Set
    End Property

    ' ── Culorile efective (proprietatea dacă e aleasă, altfel «auto» din temă) ────
    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHoverColor As Color
        Get
            Return If(_hoverColor = Color.Empty, _autoHover, _hoverColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveBorderColor As Color
        Get
            Return If(_borderColor = Color.Empty, _autoBorder, _borderColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveArrowColor As Color
        Get
            Return If(_arrowColor = Color.Empty, _autoArrow, _arrowColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveSelectionBackColor As Color
        Get
            Return If(_selectionBackColor = Color.Empty, _autoSelBack, _selectionBackColor)
        End Get
    End Property

    <Browsable(False)> <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveSelectionForeColor As Color
        Get
            Return If(_selectionForeColor = Color.Empty, _autoSelFore, _selectionForeColor)
        End Get
    End Property

    ' =====================================================================
    ' PROPRIETĂȚI MOȘTENITE PE CARE LE FIXĂM NOI — ținute în afara serializării
    ' =====================================================================

    ''' <summary>
    ''' Numai <c>DropDownList</c>. Vezi rezumatul de clasă: o casetă editabilă are un EDIT nativ
    ''' copil pe care pictura noastră nu-l atinge. Orice altceva ARUNCĂ — regula casei interzice
    ''' no-op-ul tăcut.
    ''' </summary>
    Public Shadows Property DropDownStyle As ComboBoxStyle
        Get
            Return MyBase.DropDownStyle
        End Get
        Set(value As ComboBoxStyle)
            If value <> ComboBoxStyle.DropDownList Then
                Throw New ArgumentException(
                    "KBotComboBox acceptă doar ComboBoxStyle.DropDownList (fața editabilă nu poate fi tematizată).",
                    NameOf(value))
            End If
            MyBase.DropDownStyle = value
        End Set
    End Property

    ''' <summary>Fixat de constructor ⇒ nu se serializează (nu e o alegere a operatorului).</summary>
    Public Function ShouldSerializeDropDownStyle() As Boolean
        Return False
    End Function

    ''' <summary>Owner-draw obligatoriu (pictăm rândurile) ⇒ nu se serializează.</summary>
    Public Function ShouldSerializeDrawMode() As Boolean
        Return False
    End Function

    ''' <summary>Derivat din font (vezi <see cref="OnFontChanged"/>) ⇒ nu se serializează.</summary>
    Public Function ShouldSerializeItemHeight() As Boolean
        Return False
    End Function

    ''' <summary>Fixat de constructor ⇒ nu se serializează.</summary>
    Public Function ShouldSerializeFlatStyle() As Boolean
        Return False
    End Function

    ' =====================================================================
    ' TEMĂ
    ' =====================================================================

    ''' <summary>Reaplică schema. Culorile fixate în designer nu se ating; restul iau paleta.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            _autoBack = p.InputBackColor
            _autoFore = p.InputTextColor
            _autoBorder = p.InputBorderColor
            _autoHover = p.ButtonHoverColor
            _autoArrow = p.TextDimColor
            _autoSelBack = p.AccentColor
            _autoSelFore = p.AccentTextColor
            _darkList = scheme.IsDark

            ' MyBase, nu Me: scrisul temei nu are voie să treacă drept alegere a operatorului.
            If Not _backColorPinned Then MyBase.BackColor = _autoBack
            If Not _foreColorPinned Then MyBase.ForeColor = _autoFore

            ' Fereastra listei derulante e un HWND separat: nici pictura noastră, nici traversarea
            ' temei nu ajung la ea. Doar uxtheme îi întunecă bara de derulare și rama.
            NativeMethods.ApplyWindowTheme(Me, If(_darkList, "DarkMode_CFD", "Explorer"))

            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.ApplyTheme", ex)
        End Try
    End Sub

    ' Raza efectivă, în px scalați la DPI.
    Private Function EffectiveRadius() As Integer
        Dim logical As Integer = If(_cornerRadius >= 0, _cornerRadius, ThemeManager.Current.Style.CornerRadius)
        Return ThemeShapes.ScaleDpi(Me, Math.Max(0, logical))
    End Function

    ' =====================================================================
    ' PICTURĂ
    ' =====================================================================

    ''' <summary>Fața ÎNCHISĂ: fundal rotunjit + contur + text + săgeată.</summary>
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Try
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Dim fill As Color = If(_hovered OrElse DroppedDown, EffectiveHoverColor, BackColor)
            Dim rect As New Rectangle(0, 0, Width - 1, Height - 1)

            Using path As GraphicsPath = ThemeShapes.RoundedRect(rect, EffectiveRadius())
                Using b As New SolidBrush(fill)
                    g.FillPath(b, path)
                End Using
                Using pen As New Pen(If(Focused, ThemeManager.Current.Palette.FocusRingColor, EffectiveBorderColor))
                    g.DrawPath(pen, path)
                End Using
            End Using

            Dim arrowArea As Rectangle = ArrowRect()
            DrawArrow(g, arrowArea)

            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 8)
            Dim textArea As New Rectangle(padX, 0, Math.Max(0, arrowArea.Left - padX), Height)
            Dim caption As String = SelectedCaption()
            If caption.Length > 0 Then
                TextRenderer.DrawText(g, caption, Font, textArea,
                                      If(Enabled, ForeColor, ThemeManager.Current.Palette.DisabledTextColor),
                                      TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or
                                      TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            End If
        Catch ex As Exception
            ' Frontieră de pictură: un throw de aici ar dărâma procesul.
            GlobalErrorLog.Write("KBotComboBox.OnPaint", ex)
        End Try
    End Sub

    ''' <summary>Un rând din listă: fundal (evidențiat sau nu) + text.</summary>
    Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
        Try
            If e.Index < 0 OrElse e.Index >= Items.Count Then Return

            Dim selected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
            Dim back As Color = If(selected, EffectiveSelectionBackColor, BackColor)
            Dim fore As Color = If(selected, EffectiveSelectionForeColor, ForeColor)

            Using b As New SolidBrush(back)
                e.Graphics.FillRectangle(b, e.Bounds)
            End Using

            Dim padX As Integer = ThemeShapes.ScaleDpi(Me, 8)
            Dim area As New Rectangle(e.Bounds.Left + padX, e.Bounds.Top,
                                      Math.Max(0, e.Bounds.Width - padX), e.Bounds.Height)
            TextRenderer.DrawText(e.Graphics, CaptionOf(Items(e.Index)), Font, area, fore,
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or
                                  TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnDrawItem", ex)
        End Try
    End Sub

    ' Zona săgeții: un pătrat la dreapta, lat cât înălțimea controlului (limitat).
    Private Function ArrowRect() As Rectangle
        Dim w As Integer = Math.Min(ThemeShapes.ScaleDpi(Me, 24), Math.Max(1, Width \ 3))
        Return New Rectangle(Width - w, 0, w, Height)
    End Function

    ' Săgeata: un „v” din două linii, nu un triunghi umplut — se citește la fel pe orice DPI.
    Private Sub DrawArrow(g As Graphics, area As Rectangle)
        Dim half As Integer = ThemeShapes.ScaleDpi(Me, 4)
        Dim cx As Single = area.Left + area.Width / 2.0F
        Dim cy As Single = area.Top + area.Height / 2.0F
        Using pen As New Pen(If(Enabled, EffectiveArrowColor, ThemeManager.Current.Palette.DisabledTextColor),
                             ThemeShapes.ScaleDpi(Me, 2))
            pen.StartCap = LineCap.Round
            pen.EndCap = LineCap.Round
            g.DrawLine(pen, cx - half, cy - half \ 2, cx, cy + half \ 2)
            g.DrawLine(pen, cx, cy + half \ 2, cx + half, cy - half \ 2)
        End Using
    End Sub

    ' Textul afișat pe fața închisă. Merge și pe DataSource (DisplayMember), nu doar pe Items.
    Private Function SelectedCaption() As String
        If SelectedIndex < 0 Then Return If(Text, String.Empty)
        Return CaptionOf(SelectedItem)
    End Function

    ' Respectă DisplayMember când e legat de date; altfel ToString().
    Private Function CaptionOf(item As Object) As String
        If item Is Nothing Then Return String.Empty
        If Not String.IsNullOrEmpty(DisplayMember) Then
            Dim prop = TypeDescriptor.GetProperties(item)(DisplayMember)
            If prop IsNot Nothing Then
                Dim value As Object = prop.GetValue(item)
                Return If(value Is Nothing, String.Empty, value.ToString())
            End If
        End If
        Return item.ToString()
    End Function

    ' =====================================================================
    ' STARE / INVALIDĂRI
    ' =====================================================================
    ' Fața închisă e pictată de noi, deci trebuie repictată la fiecare schimbare pe care controlul
    ' nativ ar fi tratat-o singur: hover, focus, deschidere/închidere, selecție nouă.

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        _hovered = True
        Invalidate()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        _hovered = False
        Invalidate()
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        MyBase.OnGotFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnLostFocus(e As EventArgs)
        MyBase.OnLostFocus(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnDropDown(e As EventArgs)
        MyBase.OnDropDown(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnDropDownClosed(e As EventArgs)
        MyBase.OnDropDownClosed(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)
        MyBase.OnSelectedIndexChanged(e)
        Invalidate()
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        MyBase.OnEnabledChanged(e)
        Invalidate()
    End Sub

    ''' <summary>Înălțimea rândurilor urmează fontul — altfel owner-draw-ul taie textul.</summary>
    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Try
            MyBase.ItemHeight = Math.Max(1, Font.Height + ThemeShapes.ScaleDpi(Me, 6))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnFontChanged", ex)
        End Try
    End Sub

    ''' <summary>Tema uxtheme a listei se poate cere doar după ce HWND-ul există.</summary>
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Try
            NativeMethods.ApplyWindowTheme(Me, If(_darkList, "DarkMode_CFD", "Explorer"))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotComboBox.OnHandleCreated", ex)
        End Try
    End Sub

End Class
