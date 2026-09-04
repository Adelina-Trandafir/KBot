Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' THE COLOURS of <see cref="KBotRichTextEditor"/>, and the one method that writes them.
'''
''' <para><b>The contract is the house one (C1).</b> Every colour property starts at
''' <c>Color.Empty</c>, which means «take it from the active scheme»; a colour set explicitly
''' wins and keeps winning across a scheme switch. Each one therefore carries its
''' <c>ShouldSerialize*</c>/<c>Reset*</c> pair, because <c>Empty</c> is a real value that has to
''' survive a designer round-trip -- without the pair, Visual Studio would freeze into the host
''' form whatever <see cref="ApplyTheme"/> happened to write, and nobody could tell an operator's
''' choice from an accident.</para>
'''
''' <para><b>Why there is a fallback that is not the theme.</b> The property grid, the DevHarness
''' bench and the Visual Studio surface all render this control with no scheme loaded. Resolving
''' straight to <c>ThemeManager.Current.Palette</c> would then throw or paint black-on-black, so
''' every resolver ends on an ordinary light value instead.</para>
'''
''' <para><b>The editing surface follows the INPUT colours</b>, not the surface ones: it is a
''' field the operator types into and it should read as one. The two bands follow the ALT
''' surface, closed by a baseline in the border colour -- the same recipe as the grid's header,
''' so a form carrying both does not show two different ideas of what a band is.</para>
''' </summary>
Partial Public Class KBotRichTextEditor

    ' The «no scheme loaded» values -- an ordinary light look, so the designer surface and the
    ' bench are readable before any theme exists.
    Private Shared ReadOnly AUTO_BAND_BACK As Color = Color.FromArgb(240, 240, 240)
    Private Shared ReadOnly AUTO_BAND_SEPARATOR As Color = Color.FromArgb(180, 180, 180)
    Private Shared ReadOnly AUTO_EDITOR_BACK As Color = Color.White
    Private Shared ReadOnly AUTO_EDITOR_FORE As Color = Color.FromArgb(30, 30, 30)
    Private Shared ReadOnly AUTO_EDITOR_BORDER As Color = Color.FromArgb(180, 180, 180)
    Private Shared ReadOnly AUTO_FOOTER_FORE As Color = Color.FromArgb(115, 115, 115)
    Private Shared ReadOnly AUTO_BUTTON_BACK As Color = Color.FromArgb(225, 225, 225)
    Private Shared ReadOnly AUTO_BUTTON_BORDER As Color = Color.FromArgb(173, 173, 173)
    Private Shared ReadOnly AUTO_BUTTON_TEXT As Color = Color.FromArgb(30, 30, 30)
    Private Shared ReadOnly AUTO_BUTTON_PRESSED As Color = Color.FromArgb(204, 228, 247)
    Private Shared ReadOnly AUTO_ACCENT As Color = Color.FromArgb(0, 122, 204)

    ' «The operator pinned it» -- see ShouldSerializeBackColor.
    Private _backColorPinned As Boolean = False

    Private _headerBackColor As Color = Color.Empty
    Private _headerSeparatorColor As Color = Color.Empty
    Private _footerBackColor As Color = Color.Empty
    Private _footerSeparatorColor As Color = Color.Empty
    Private _footerForeColor As Color = Color.Empty
    Private _editorBackColor As Color = Color.Empty
    Private _editorForeColor As Color = Color.Empty
    Private _editorBorderColor As Color = Color.Empty

    ' ══════════════════════════════════════════════════════════════════════════
    ' CULORI
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>The surface behind the bands; not pinned here, it follows the scheme.</summary>
    <Category("K-BOT Colors")>
    <Description("Fundalul controlului; nesetat aici, urmează tema.")>
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
    ''' CRITICAL. <c>Control.ShouldSerializeBackColor</c> answers True the moment the property
    ''' has ever been WRITTEN -- including by <see cref="ApplyResolvedColors"/>, which runs in
    ''' the constructor so that an unthemed bench is readable. Without this pair, Visual Studio
    ''' would write a <c>BackColor</c> line nobody chose into every host form, and on reload
    ''' that line would come back through the setter and pin the colour for good. The flag is
    ''' the truth, not <c>Control</c>'s property bag (C4).
    ''' </summary>
    Public Function ShouldSerializeBackColor() As Boolean
        Return _backColorPinned
    End Function

    ' The flag is cleared AFTER the write: ResetBackColor goes through the VIRTUAL setter, which
    ' is ours, and that would light it again.
    Public Overrides Sub ResetBackColor()
        MyBase.BackColor = AUTO_BAND_BACK
        _backColorPinned = False
        ApplyResolvedColors()
    End Sub

    <Category("K-BOT Colors")>
    <Description("Fundalul benzii de antet; nesetat = suprafața secundară a schemei.")>
    Public Property HeaderBackColor As Color
        Get
            Return _headerBackColor
        End Get
        Set(value As Color)
            If _headerBackColor = value Then Return
            _headerBackColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeHeaderBackColor() As Boolean
        Return Not _headerBackColor.IsEmpty
    End Function

    Private Sub ResetHeaderBackColor()
        HeaderBackColor = Color.Empty
    End Sub

    ''' <summary>The baseline under the toolbar -- the grid header's line, by the same name.</summary>
    <Category("K-BOT Colors")>
    <Description("Culoarea liniei de sub antet; nesetată = culoarea de contur a schemei.")>
    Public Property HeaderSeparatorColor As Color
        Get
            Return _headerSeparatorColor
        End Get
        Set(value As Color)
            If _headerSeparatorColor = value Then Return
            _headerSeparatorColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeHeaderSeparatorColor() As Boolean
        Return Not _headerSeparatorColor.IsEmpty
    End Function

    Private Sub ResetHeaderSeparatorColor()
        HeaderSeparatorColor = Color.Empty
    End Sub

    <Category("K-BOT Colors")>
    <Description("Fundalul benzii de subsol; nesetat = suprafața secundară a schemei.")>
    Public Property FooterBackColor As Color
        Get
            Return _footerBackColor
        End Get
        Set(value As Color)
            If _footerBackColor = value Then Return
            _footerBackColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeFooterBackColor() As Boolean
        Return Not _footerBackColor.IsEmpty
    End Function

    Private Sub ResetFooterBackColor()
        FooterBackColor = Color.Empty
    End Sub

    <Category("K-BOT Colors")>
    <Description("Culoarea liniei de deasupra subsolului; nesetată = culoarea de contur a schemei.")>
    Public Property FooterSeparatorColor As Color
        Get
            Return _footerSeparatorColor
        End Get
        Set(value As Color)
            If _footerSeparatorColor = value Then Return
            _footerSeparatorColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeFooterSeparatorColor() As Boolean
        Return Not _footerSeparatorColor.IsEmpty
    End Function

    Private Sub ResetFooterSeparatorColor()
        FooterSeparatorColor = Color.Empty
    End Sub

    ''' <summary>The three counters. Dim by default: they are a reading, not a message.</summary>
    <Category("K-BOT Colors")>
    <Description("Culoarea numerelor din subsol; nesetată = textul estompat al schemei.")>
    Public Property FooterForeColor As Color
        Get
            Return _footerForeColor
        End Get
        Set(value As Color)
            If _footerForeColor = value Then Return
            _footerForeColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeFooterForeColor() As Boolean
        Return Not _footerForeColor.IsEmpty
    End Function

    Private Sub ResetFooterForeColor()
        FooterForeColor = Color.Empty
    End Sub

    ''' <summary>The paper the operator types on; nothing set = the scheme's INPUT background.</summary>
    <Category("K-BOT Colors")>
    <Description("Fundalul casetei de scris; nesetat = fundalul de câmp al schemei.")>
    Public Property EditorBackColor As Color
        Get
            Return _editorBackColor
        End Get
        Set(value As Color)
            If _editorBackColor = value Then Return
            _editorBackColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeEditorBackColor() As Boolean
        Return Not _editorBackColor.IsEmpty
    End Function

    Private Sub ResetEditorBackColor()
        EditorBackColor = Color.Empty
    End Sub

    ''' <summary>
    ''' The ink. It is the surface's colour, so it only shows through where the document says
    ''' nothing else: a run the operator coloured from the toolbar keeps ITS colour, because
    ''' that choice is stored in the RTF and is part of the document, not of the theme.
    ''' </summary>
    <Category("K-BOT Colors")>
    <Description("Culoarea textului din caseta de scris; nesetată = textul de câmp al schemei.")>
    Public Property EditorForeColor As Color
        Get
            Return _editorForeColor
        End Get
        Set(value As Color)
            If _editorForeColor = value Then Return
            _editorForeColor = value
            ApplyResolvedColors()
        End Set
    End Property

    Private Function ShouldSerializeEditorForeColor() As Boolean
        Return Not _editorForeColor.IsEmpty
    End Function

    Private Sub ResetEditorForeColor()
        EditorForeColor = Color.Empty
    End Sub

    <Category("K-BOT Colors")>
    <Description("Culoarea chenarului casetei de scris; nesetată = conturul de câmp al schemei.")>
    Public Property EditorBorderColor As Color
        Get
            Return _editorBorderColor
        End Get
        Set(value As Color)
            If _editorBorderColor = value Then Return
            _editorBorderColor = value
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeEditorBorderColor() As Boolean
        Return Not _editorBorderColor.IsEmpty
    End Function

    Private Sub ResetEditorBorderColor()
        EditorBorderColor = Color.Empty
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' REZOLVARE -- «ce se pictează de fapt»
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>The active palette, or <c>Nothing</c> when no scheme is loaded.</summary>
    Private Shared Function CurrentPalette() As ThemePalette
        Dim scheme As ThemeScheme = ThemeManager.Current
        If scheme Is Nothing Then Return Nothing
        Return scheme.Palette
    End Function

    ''' <summary>Explicit wins; then the scheme; then the «no scheme» value.</summary>
    Private Shared Function Resolve(pinned As Color, fromTheme As Func(Of ThemePalette, Color),
                                    auto As Color) As Color
        If Not pinned.IsEmpty Then Return pinned
        Dim p As ThemePalette = CurrentPalette()
        If p Is Nothing Then Return auto
        Return fromTheme(p)
    End Function

    ''' <summary>The header background actually painted.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveHeaderBackColor As Color
        Get
            Return Resolve(_headerBackColor, Function(p) p.SurfaceAltColor, AUTO_BAND_BACK)
        End Get
    End Property

    ''' <summary>The footer background actually painted.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveFooterBackColor As Color
        Get
            Return Resolve(_footerBackColor, Function(p) p.SurfaceAltColor, AUTO_BAND_BACK)
        End Get
    End Property

    ''' <summary>The editing surface's background actually painted.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveEditorBackColor As Color
        Get
            Return Resolve(_editorBackColor, Function(p) p.InputBackColor, AUTO_EDITOR_BACK)
        End Get
    End Property

    ''' <summary>The editing surface's ink actually painted.</summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property EffectiveEditorForeColor As Color
        Get
            Return Resolve(_editorForeColor, Function(p) p.InputTextColor, AUTO_EDITOR_FORE)
        End Get
    End Property

    Private Function ResolvedEditorBorderColor() As Color
        Return Resolve(_editorBorderColor, Function(p) p.InputBorderColor, AUTO_EDITOR_BORDER)
    End Function

    Private Function ResolvedHeaderSeparatorColor() As Color
        Return Resolve(_headerSeparatorColor, Function(p) p.BorderColor, AUTO_BAND_SEPARATOR)
    End Function

    Private Function ResolvedFooterSeparatorColor() As Color
        Return Resolve(_footerSeparatorColor, Function(p) p.BorderColor, AUTO_BAND_SEPARATOR)
    End Function

    Private Function ResolvedFooterForeColor() As Color
        Return Resolve(_footerForeColor, Function(p) p.TextDimColor, AUTO_FOOTER_FORE)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' SCRIEREA CULORILOR
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Pushes the separators' colour AND their scaled width into both bands.</summary>
    Private Sub ApplyBandSeparators()
        Try
            If pnlHeader Is Nothing OrElse pnlFooter Is Nothing Then Return
            pnlHeader.SeparatorColor = ResolvedHeaderSeparatorColor()
            pnlHeader.SeparatorWidth = ThemeShapes.ScaleDpi(Me, _headerSeparatorWidth)
            pnlFooter.SeparatorColor = ResolvedFooterSeparatorColor()
            pnlFooter.SeparatorWidth = ThemeShapes.ScaleDpi(Me, _footerSeparatorWidth)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyBandSeparators", ex)
        End Try
    End Sub

    ''' <summary>Writes every resolved colour onto the children it belongs to.</summary>
    Private Sub ApplyResolvedColors()
        Try
            If pnlHeader Is Nothing OrElse rtb Is Nothing Then Return

            Dim p As ThemePalette = CurrentPalette()
            ' MyBase, and only when the operator has not pinned one: going through our own
            ' setter would light the flag that keeps the colour out of the host's designer file.
            If Not _backColorPinned Then
                MyBase.BackColor = If(p Is Nothing, AUTO_BAND_BACK, p.SurfaceAltColor)
            End If

            pnlHeader.BackColor = EffectiveHeaderBackColor
            pnlFooter.BackColor = EffectiveFooterBackColor
            ApplyBandSeparators()

            rtb.BackColor = EffectiveEditorBackColor
            rtb.ForeColor = EffectiveEditorForeColor

            Dim footerInk As Color = ResolvedFooterForeColor()
            For Each l As Label In New Label() {lblChars, lblWords, lblSize}
                ' Transparent so the band's own paint (background + baseline) shows through.
                l.BackColor = Color.Transparent
                l.ForeColor = footerInk
            Next

            ' Re-derive the pressed / unpressed look so a scheme switch does not leave a button
            ' showing the previous palette's "on" colour.
            RefreshToolbarState()
            ApplyPressedLook(btnCollapse, False)
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyResolvedColors", ex)
        End Try
    End Sub

    ''' <summary>The "this style is on" look. Theme colours, never literals -- the original
    ''' hardcoded LightSteelBlue / DodgerBlue, which is invisible under a dark scheme.</summary>
    Private Sub ApplyPressedLook(btn As KBotNoFocusButton, pressed As Boolean)
        If btn Is Nothing Then Return
        Dim p As ThemePalette = CurrentPalette()

        If pressed Then
            btn.BackColor = If(p Is Nothing, AUTO_BUTTON_PRESSED, p.ButtonPressedColor)
            btn.ForeColor = If(p Is Nothing, AUTO_BUTTON_TEXT, p.TextColor)
            btn.FlatAppearance.BorderColor = If(p Is Nothing, AUTO_ACCENT, p.AccentColor)
            btn.FlatAppearance.BorderSize = 2
        Else
            btn.BackColor = If(p Is Nothing, AUTO_BUTTON_BACK, p.ButtonBackColor)
            btn.ForeColor = If(p Is Nothing, AUTO_BUTTON_TEXT, p.ButtonTextColor)
            btn.FlatAppearance.BorderColor = If(p Is Nothing, AUTO_BUTTON_BORDER, p.ButtonBorderColor)
            btn.FlatAppearance.BorderSize = 1
        End If
    End Sub

    ''' <summary>Repaints the control from the given scheme. Required because this control owns
    ''' child controls -- see the class remarks.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            ApplyResolvedColors()
            ' The base font may have moved with the scheme, so anything measured from it
            ' (the footer counters) has to be placed again.
            RebuildLayout()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyTheme", ex)
        End Try
    End Sub
End Class
