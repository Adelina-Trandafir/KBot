Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming

''' <summary>
''' THE PUBLISHED SURFACE of <see cref="KBotRichTextEditor"/>: every number, padding, font and
''' picture the operator is allowed to change from the property grid.
'''
''' <para><b>Two rules run through the whole file.</b> Every pixel number is LOGICAL at 96 dpi
''' and is scaled once, at layout time (C2) -- no setter ever writes a scaled value back.
''' Everything that can come from the theme (<c>Color.Empty</c>, <c>Nothing</c> for a font or a
''' picture) means «take it from the active scheme», and anything set explicitly wins and keeps
''' winning across a scheme switch (C1). The colours themselves live next door, in
''' <c>KBotRichTextEditor.Theming.vb</c>.</para>
'''
''' <para><b>Icons: two ways in, on purpose.</b> A <c>*Image</c> is a picture chosen from disk
''' through the designer's image picker -- it lands in the host form's <c>.resx</c>. A
''' <c>*ImageKey</c> points into <see cref="Images"/>, one shared list for the whole toolbar, so
''' swapping that list re-skins every button at once. The explicit picture wins over the key,
''' and with neither the button keeps its lettered glyph (B / I / U / A / ▨) -- the toolbar is
''' never blank.</para>
''' </summary>
Partial Public Class KBotRichTextEditor

    ' ── Header metrics ──────────────────────────────────────────────────────────
    Private _headerVisible As Boolean = True
    Private _headerHeight As Integer = 38
    Private _headerPadding As New Padding(4)
    Private _headerSeparatorWidth As Integer = 1

    ' ── Header contents ─────────────────────────────────────────────────────────
    Private _buttonSize As New Size(30, 30)
    Private _buttonSpacing As Integer = 2
    Private _buttonPadding As Padding = Padding.Empty
    Private _groupSpacing As Integer = 10
    Private _fontComboWidth As Integer = 186
    Private _sizeComboWidth As Integer = 76
    Private _comboSpacing As Integer = 4
    Private _comboHeight As Integer = 0
    Private _comboFont As Font = Nothing

    ' ── The editing surface ─────────────────────────────────────────────────────
    Private _editorPadding As New Padding(4)
    Private _editorBorderWidth As Integer = 1
    Private _editorFont As Font = Nothing

    ' ── Footer ──────────────────────────────────────────────────────────────────
    Private _footerVisible As Boolean = True
    Private _footerHeight As Integer = 24
    Private _footerPadding As New Padding(8, 0, 8, 0)
    Private _footerSeparatorWidth As Integer = 1
    Private _footerItemSpacing As Integer = 16
    Private _footerFont As Font = Nothing
    Private _footerCharactersFormat As String = "{0:N0} caractere"
    Private _footerWordsFormat As String = "{0:N0} cuvinte"
    Private _footerSizeFormat As String = "{0:N1} KB"

    ' ── Icons ───────────────────────────────────────────────────────────────────
    Private _images As ImageList = Nothing
    Private _buttonImageLayout As RichTextImageLayout = RichTextImageLayout.Original
    Private _boldImage As Image = Nothing
    Private _boldImageKey As String = String.Empty
    Private _italicImage As Image = Nothing
    Private _italicImageKey As String = String.Empty
    Private _underlineImage As Image = Nothing
    Private _underlineImageKey As String = String.Empty
    Private _textColorImage As Image = Nothing
    Private _textColorImageKey As String = String.Empty
    Private _highlightImage As Image = Nothing
    Private _highlightImageKey As String = String.Empty
    Private _collapseExpandedImage As Image = Nothing
    Private _collapseExpandedImageKey As String = String.Empty
    Private _collapseCollapsedImage As Image = Nothing
    Private _collapseCollapsedImageKey As String = String.Empty

    ' ── Collapse ────────────────────────────────────────────────────────────────
    Private _collapseButton As Boolean = False
    Private _collapsed As Boolean = False
    Private _expandedHeight As Integer = 0
    Private _applyingCollapseExtent As Boolean = False

    ' The lettered fallbacks. They are what the operator reads when no picture is bound, so
    ' they are the one place a non-ASCII character is allowed (RULE 0's exception).
    Private Const GLYPH_BOLD As String = "B"
    Private Const GLYPH_ITALIC As String = "I"
    Private Const GLYPH_UNDERLINE As String = "U"
    Private Const GLYPH_TEXT_COLOR As String = "A"
    Private Const GLYPH_HIGHLIGHT As String = "▨"
    Private Const GLYPH_EXPANDED As String = "▴"
    Private Const GLYPH_COLLAPSED As String = "▾"

    ''' <summary>The editor folded or unfolded -- the host can move its splitter.</summary>
    Public Event CollapsedChanged(collapsed As Boolean)

    ' ══════════════════════════════════════════════════════════════════════════
    ' ANTET -- geometria benzii
    ' ══════════════════════════════════════════════════════════════════════════

    <Category("K-BOT Header")>
    <Description("Arată bara de instrumente. Stinsă, editorul e o simplă suprafață de scris.")>
    <DefaultValue(True)>
    Public Property HeaderVisible As Boolean
        Get
            Return _headerVisible
        End Get
        Set(value As Boolean)
            If _headerVisible = value Then Return
            _headerVisible = value
            RebuildLayout()
        End Set
    End Property

    ''' <summary>The band height in LOGICAL pixels -- the grid's <c>HeaderHeight</c> by another
    ''' name, and on purpose: the two bands sit on the same forms.</summary>
    <Category("K-BOT Header")>
    <Description("Înălțimea benzii de antet (px la 96 dpi).")>
    <DefaultValue(38)>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _headerHeight = wanted Then Return
            _headerHeight = wanted
            If _collapsed Then ApplyCollapseExtent()
            RebuildLayout()
        End Set
    End Property

    ''' <summary>The inset around everything in the band.</summary>
    <Category("K-BOT Header")>
    <Description("Marginea interioară a benzii de antet (px la 96 dpi).")>
    Public Property HeaderPadding As Padding
        Get
            Return _headerPadding
        End Get
        Set(value As Padding)
            If _headerPadding = value Then Return
            _headerPadding = value
            RebuildLayout()
        End Set
    End Property

    Private Function ShouldSerializeHeaderPadding() As Boolean
        Return _headerPadding <> New Padding(4)
    End Function

    Private Sub ResetHeaderPadding()
        HeaderPadding = New Padding(4)
    End Sub

    ''' <summary>The baseline thickness under the band. 0 = no line.</summary>
    <Category("K-BOT Header")>
    <Description("Grosimea liniei de sub antet (px la 96 dpi). 0 = fără linie.")>
    <DefaultValue(1)>
    Public Property HeaderSeparatorWidth As Integer
        Get
            Return _headerSeparatorWidth
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _headerSeparatorWidth = wanted Then Return
            _headerSeparatorWidth = wanted
            ApplyBandSeparators()
            RebuildLayout()
        End Set
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' ANTET -- butoanele si selectoarele
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>The side of every toolbar button, collapse button included.</summary>
    <Category("K-BOT Header")>
    <Description("Dimensiunea unui buton din bară (px la 96 dpi).")>
    Public Property ButtonSize As Size
        Get
            Return _buttonSize
        End Get
        Set(value As Size)
            Dim wanted As New Size(Math.Max(1, value.Width), Math.Max(1, value.Height))
            If _buttonSize = wanted Then Return
            _buttonSize = wanted
            RebuildLayout()
        End Set
    End Property

    Private Function ShouldSerializeButtonSize() As Boolean
        Return _buttonSize <> New Size(30, 30)
    End Function

    Private Sub ResetButtonSize()
        ButtonSize = New Size(30, 30)
    End Sub

    ''' <summary>The gap BETWEEN two neighbouring buttons.</summary>
    <Category("K-BOT Header")>
    <Description("Spațiul dintre două butoane vecine (px la 96 dpi).")>
    <DefaultValue(2)>
    Public Property ButtonSpacing As Integer
        Get
            Return _buttonSpacing
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _buttonSpacing = wanted Then Return
            _buttonSpacing = wanted
            RebuildLayout()
        End Set
    End Property

    ''' <summary>The inset INSIDE a button -- how far its glyph or picture sits from the edge.</summary>
    <Category("K-BOT Header")>
    <Description("Marginea interioară a fiecărui buton (px la 96 dpi).")>
    Public Property ButtonPadding As Padding
        Get
            Return _buttonPadding
        End Get
        Set(value As Padding)
            If _buttonPadding = value Then Return
            _buttonPadding = value
            RebuildLayout()
        End Set
    End Property

    Private Function ShouldSerializeButtonPadding() As Boolean
        Return _buttonPadding <> Padding.Empty
    End Function

    Private Sub ResetButtonPadding()
        ButtonPadding = Padding.Empty
    End Sub

    ''' <summary>The gap between the BUTTON group and the PICKER group -- what separates
    ''' "commands" from "choices" for the eye.</summary>
    <Category("K-BOT Header")>
    <Description("Spațiul dintre grupul de butoane și grupul de selectoare (px la 96 dpi).")>
    <DefaultValue(10)>
    Public Property GroupSpacing As Integer
        Get
            Return _groupSpacing
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _groupSpacing = wanted Then Return
            _groupSpacing = wanted
            RebuildLayout()
        End Set
    End Property

    <Category("K-BOT Header")>
    <Description("Lățimea selectorului de font (px la 96 dpi).")>
    <DefaultValue(186)>
    Public Property FontComboWidth As Integer
        Get
            Return _fontComboWidth
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _fontComboWidth = wanted Then Return
            _fontComboWidth = wanted
            RebuildLayout()
        End Set
    End Property

    <Category("K-BOT Header")>
    <Description("Lățimea selectorului de mărime (px la 96 dpi).")>
    <DefaultValue(76)>
    Public Property SizeComboWidth As Integer
        Get
            Return _sizeComboWidth
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _sizeComboWidth = wanted Then Return
            _sizeComboWidth = wanted
            RebuildLayout()
        End Set
    End Property

    <Category("K-BOT Header")>
    <Description("Spațiul dintre cele două selectoare (px la 96 dpi).")>
    <DefaultValue(4)>
    Public Property ComboSpacing As Integer
        Get
            Return _comboSpacing
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _comboSpacing = wanted Then Return
            _comboSpacing = wanted
            RebuildLayout()
        End Set
    End Property

    ''' <summary>The height of both pickers. 0 = fill the band between its paddings, which is
    ''' what makes a taller header simply produce taller pickers.</summary>
    <Category("K-BOT Header")>
    <Description("Înălțimea selectoarelor (px la 96 dpi). 0 = umplu banda între margini.")>
    <DefaultValue(0)>
    Public Property ComboHeight As Integer
        Get
            Return _comboHeight
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _comboHeight = wanted Then Return
            _comboHeight = wanted
            RebuildLayout()
        End Set
    End Property

    ''' <summary>
    ''' The font of BOTH pickers. <c>Nothing</c> = the scheme's ambient font.
    '''
    ''' <para>Assigning it pins the font on the two <see cref="KBotComboBox"/>es, so a later
    ''' <c>ApplyTheme</c> leaves it alone -- that is exactly what their own pinning flag is
    ''' for, and why this property does not have to fight the theme engine.</para>
    ''' </summary>
    <Category("K-BOT Header")>
    <Description("Fontul celor două selectoare; nesetat = fontul schemei.")>
    Public Property ComboFont As Font
        Get
            Return _comboFont
        End Get
        Set(value As Font)
            If _comboFont Is value Then Return
            _comboFont = value
            ApplyComboFont()
            RebuildLayout()
        End Set
    End Property

    Private Function ShouldSerializeComboFont() As Boolean
        Return _comboFont IsNot Nothing
    End Function

    Private Sub ResetComboFont()
        ComboFont = Nothing
    End Sub

    ''' <summary>Pushes <see cref="ComboFont"/> onto the pickers, or hands them back to the
    ''' theme when it is cleared.</summary>
    Private Sub ApplyComboFont()
        Try
            For Each c As KBotComboBox In New KBotComboBox() {cmbFont, cmbSize}
                If _comboFont Is Nothing Then
                    c.ResetFont()
                Else
                    c.Font = _comboFont
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyComboFont", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' SUPRAFATA DE SCRIS
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The padding INSIDE the editing surface -- how far the first character sits from the
    ''' frame. A <c>RichTextBox</c> ignores <c>Padding</c>, so this is applied to its
    ''' formatting rectangle (<c>EM_SETRECT</c>); see <c>ApplyEditorPadding</c>.
    ''' </summary>
    <Category("K-BOT Editor")>
    <Description("Marginea interioară a casetei de scris (px la 96 dpi).")>
    Public Property EditorPadding As Padding
        Get
            Return _editorPadding
        End Get
        Set(value As Padding)
            If _editorPadding = value Then Return
            _editorPadding = value
            ApplyEditorPadding()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeEditorPadding() As Boolean
        Return _editorPadding <> New Padding(4)
    End Function

    Private Sub ResetEditorPadding()
        EditorPadding = New Padding(4)
    End Sub

    ''' <summary>The frame around the editing surface. 0 = no frame.</summary>
    <Category("K-BOT Editor")>
    <Description("Grosimea chenarului casetei de scris (px la 96 dpi). 0 = fără chenar.")>
    <DefaultValue(1)>
    Public Property EditorBorderWidth As Integer
        Get
            Return _editorBorderWidth
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _editorBorderWidth = wanted Then Return
            _editorBorderWidth = wanted
            RebuildLayout()
        End Set
    End Property

    ''' <summary>
    ''' The BASE font of the document -- what unformatted text is written in, and what a mixed
    ''' selection falls back to. <c>Nothing</c> = the scheme's ambient font.
    '''
    ''' <para>It is not the same thing as the font of a run: the operator picks those from the
    ''' toolbar and they are stored in the RTF. This one is the starting point.</para>
    ''' </summary>
    <Category("K-BOT Editor")>
    <Description("Fontul de bază al documentului; nesetat = fontul schemei.")>
    Public Property EditorFont As Font
        Get
            Return _editorFont
        End Get
        Set(value As Font)
            If _editorFont Is value Then Return
            _editorFont = value
            If value IsNot Nothing Then rtb.Font = value
            RefreshToolbarState()
            Invalidate()
        End Set
    End Property

    Private Function ShouldSerializeEditorFont() As Boolean
        Return _editorFont IsNot Nothing
    End Function

    Private Sub ResetEditorFont()
        EditorFont = Nothing
    End Sub

    ''' <summary>Which scrollbars the editing surface shows.</summary>
    <Category("K-BOT Editor")>
    <Description("Barele de derulare ale casetei de scris.")>
    <DefaultValue(RichTextBoxScrollBars.Vertical)>
    Public Property ScrollBars As RichTextBoxScrollBars
        Get
            Return rtb.ScrollBars
        End Get
        Set(value As RichTextBoxScrollBars)
            If rtb.ScrollBars = value Then Return
            rtb.ScrollBars = value
            RebuildLayout()
        End Set
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' SUBSOL
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>Shows the band that counts characters, words and KB.</summary>
    <Category("K-BOT Footer")>
    <Description("Arată banda de jos, cu numărul de caractere, de cuvinte și mărimea în KB.")>
    <DefaultValue(True)>
    Public Property FooterVisible As Boolean
        Get
            Return _footerVisible
        End Get
        Set(value As Boolean)
            If _footerVisible = value Then Return
            _footerVisible = value
            If _footerVisible Then RefreshStatistics()
            RebuildLayout()
        End Set
    End Property

    <Category("K-BOT Footer")>
    <Description("Înălțimea benzii de subsol (px la 96 dpi).")>
    <DefaultValue(24)>
    Public Property FooterHeight As Integer
        Get
            Return _footerHeight
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _footerHeight = wanted Then Return
            _footerHeight = wanted
            RebuildLayout()
        End Set
    End Property

    <Category("K-BOT Footer")>
    <Description("Marginea interioară a benzii de subsol (px la 96 dpi).")>
    Public Property FooterPadding As Padding
        Get
            Return _footerPadding
        End Get
        Set(value As Padding)
            If _footerPadding = value Then Return
            _footerPadding = value
            RebuildLayout()
        End Set
    End Property

    Private Function ShouldSerializeFooterPadding() As Boolean
        Return _footerPadding <> New Padding(8, 0, 8, 0)
    End Function

    Private Sub ResetFooterPadding()
        FooterPadding = New Padding(8, 0, 8, 0)
    End Sub

    <Category("K-BOT Footer")>
    <Description("Grosimea liniei de deasupra subsolului (px la 96 dpi). 0 = fără linie.")>
    <DefaultValue(1)>
    Public Property FooterSeparatorWidth As Integer
        Get
            Return _footerSeparatorWidth
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _footerSeparatorWidth = wanted Then Return
            _footerSeparatorWidth = wanted
            ApplyBandSeparators()
            RebuildLayout()
        End Set
    End Property

    <Category("K-BOT Footer")>
    <Description("Spațiul dintre cele trei numere din subsol (px la 96 dpi).")>
    <DefaultValue(16)>
    Public Property FooterItemSpacing As Integer
        Get
            Return _footerItemSpacing
        End Get
        Set(value As Integer)
            Dim wanted As Integer = Math.Max(0, value)
            If _footerItemSpacing = wanted Then Return
            _footerItemSpacing = wanted
            RebuildLayout()
        End Set
    End Property

    ''' <summary>The font of the three counters. <c>Nothing</c> = the scheme's ambient font.</summary>
    <Category("K-BOT Footer")>
    <Description("Fontul numerelor din subsol; nesetat = fontul schemei.")>
    Public Property FooterFont As Font
        Get
            Return _footerFont
        End Get
        Set(value As Font)
            If _footerFont Is value Then Return
            _footerFont = value
            ApplyFooterFont()
            RebuildLayout()
        End Set
    End Property

    Private Function ShouldSerializeFooterFont() As Boolean
        Return _footerFont IsNot Nothing
    End Function

    Private Sub ResetFooterFont()
        FooterFont = Nothing
    End Sub

    Private Sub ApplyFooterFont()
        Try
            For Each l As Label In New Label() {lblChars, lblWords, lblSize}
                If _footerFont Is Nothing Then
                    l.ResetFont()
                Else
                    l.Font = _footerFont
                End If
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyFooterFont", ex)
        End Try
    End Sub

    ''' <summary>How the character count reads. <c>{0}</c> is the number.</summary>
    <Category("K-BOT Footer")>
    <Description("Textul numărului de caractere. {0} e numărul.")>
    <DefaultValue("{0:N0} caractere")>
    Public Property FooterCharactersFormat As String
        Get
            Return _footerCharactersFormat
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_footerCharactersFormat, wanted, StringComparison.Ordinal) Then Return
            _footerCharactersFormat = wanted
            ApplyFooterTexts(_chars, _words, _kilobytes)
            RebuildLayout()
        End Set
    End Property

    ''' <summary>How the word count reads. <c>{0}</c> is the number.</summary>
    <Category("K-BOT Footer")>
    <Description("Textul numărului de cuvinte. {0} e numărul.")>
    <DefaultValue("{0:N0} cuvinte")>
    Public Property FooterWordsFormat As String
        Get
            Return _footerWordsFormat
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_footerWordsFormat, wanted, StringComparison.Ordinal) Then Return
            _footerWordsFormat = wanted
            ApplyFooterTexts(_chars, _words, _kilobytes)
            RebuildLayout()
        End Set
    End Property

    ''' <summary>How the size reads. <c>{0}</c> is the size in KB.</summary>
    <Category("K-BOT Footer")>
    <Description("Textul mărimii. {0} e mărimea în KB a documentului RTF.")>
    <DefaultValue("{0:N1} KB")>
    Public Property FooterSizeFormat As String
        Get
            Return _footerSizeFormat
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_footerSizeFormat, wanted, StringComparison.Ordinal) Then Return
            _footerSizeFormat = wanted
            ApplyFooterTexts(_chars, _words, _kilobytes)
            RebuildLayout()
        End Set
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' PICTOGRAME
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The picture source for every <c>*ImageKey</c>. The editor does NOT own the list and
    ''' never disposes it -- it belongs to the host form, exactly as with the tree's
    ''' <c>NodeImages</c>.
    ''' </summary>
    <Category("K-BOT Icons")>
    <Description("Lista de imagini din care se rezolvă cheile de pictograme ale butoanelor.")>
    <DefaultValue(GetType(ImageList), Nothing)>
    Public Property Images As ImageList
        Get
            Return _images
        End Get
        Set(value As ImageList)
            If ReferenceEquals(_images, value) Then Return
            DetachImages()
            _images = value
            AttachImages()
            ApplyButtonIcons()
        End Set
    End Property

    ''' <summary>
    ''' How the pictures meet the buttons -- ONE property for the whole band, on purpose: a
    ''' toolbar whose icons were fitted six different ways would look broken, and the operator
    ''' binds one icon set, not six.
    '''
    ''' <para><see cref="RichTextImageLayout.Original"/> keeps the picture at its own size and
    ''' lets the framework place it, which is the toolbar's old look to the pixel. The other
    ''' three are painted by <see cref="KBotNoFocusButton"/> inside what the flat border and
    ''' <see cref="ButtonPadding"/> leave -- so the inset is honoured by all of them.</para>
    '''
    ''' <para>The lettered glyphs (B / I / U / A / ▨) are TEXT and are never touched by this:
    ''' with no picture bound there is nothing to lay out.</para>
    ''' </summary>
    <Category("K-BOT Icons")>
    <Description("Cum sunt desenate pictogramele pe toate butoanele din antet: la mărimea lor, întinse, încadrate sau repetate.")>
    <DefaultValue(RichTextImageLayout.Original)>
    Public Property ButtonImageLayout As RichTextImageLayout
        Get
            Return _buttonImageLayout
        End Get
        Set(value As RichTextImageLayout)
            If Not [Enum].IsDefined(GetType(RichTextImageLayout), value) Then
                Throw New ArgumentException("Mod de desenare necunoscut pentru pictogramele butoanelor.", NameOf(value))
            End If
            If _buttonImageLayout = value Then Return
            _buttonImageLayout = value
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Icons")>
    <Description("Pictograma butonului «Îngroșat»; nesetată = litera B.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property BoldImage As Image
        Get
            Return _boldImage
        End Get
        Set(value As Image)
            If _boldImage Is value Then Return
            _boldImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeBoldImage() As Boolean
        Return _boldImage IsNot Nothing
    End Function

    Private Sub ResetBoldImage()
        BoldImage = Nothing
    End Sub

    <Category("K-BOT Icons")>
    <Description("Cheia pictogramei butonului «Îngroșat», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property BoldImageKey As String
        Get
            Return _boldImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_boldImageKey, wanted, StringComparison.Ordinal) Then Return
            _boldImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Icons")>
    <Description("Pictograma butonului «Înclinat»; nesetată = litera I.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property ItalicImage As Image
        Get
            Return _italicImage
        End Get
        Set(value As Image)
            If _italicImage Is value Then Return
            _italicImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeItalicImage() As Boolean
        Return _italicImage IsNot Nothing
    End Function

    Private Sub ResetItalicImage()
        ItalicImage = Nothing
    End Sub

    <Category("K-BOT Icons")>
    <Description("Cheia pictogramei butonului «Înclinat», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property ItalicImageKey As String
        Get
            Return _italicImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_italicImageKey, wanted, StringComparison.Ordinal) Then Return
            _italicImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Icons")>
    <Description("Pictograma butonului «Subliniat»; nesetată = litera U.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property UnderlineImage As Image
        Get
            Return _underlineImage
        End Get
        Set(value As Image)
            If _underlineImage Is value Then Return
            _underlineImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeUnderlineImage() As Boolean
        Return _underlineImage IsNot Nothing
    End Function

    Private Sub ResetUnderlineImage()
        UnderlineImage = Nothing
    End Sub

    <Category("K-BOT Icons")>
    <Description("Cheia pictogramei butonului «Subliniat», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property UnderlineImageKey As String
        Get
            Return _underlineImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_underlineImageKey, wanted, StringComparison.Ordinal) Then Return
            _underlineImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Icons")>
    <Description("Pictograma butonului «Culoarea textului»; nesetată = litera A.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property TextColorImage As Image
        Get
            Return _textColorImage
        End Get
        Set(value As Image)
            If _textColorImage Is value Then Return
            _textColorImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeTextColorImage() As Boolean
        Return _textColorImage IsNot Nothing
    End Function

    Private Sub ResetTextColorImage()
        TextColorImage = Nothing
    End Sub

    <Category("K-BOT Icons")>
    <Description("Cheia pictogramei butonului «Culoarea textului», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property TextColorImageKey As String
        Get
            Return _textColorImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_textColorImageKey, wanted, StringComparison.Ordinal) Then Return
            _textColorImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Icons")>
    <Description("Pictograma butonului «Culoarea fundalului»; nesetată = semnul hașurat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property HighlightImage As Image
        Get
            Return _highlightImage
        End Get
        Set(value As Image)
            If _highlightImage Is value Then Return
            _highlightImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeHighlightImage() As Boolean
        Return _highlightImage IsNot Nothing
    End Function

    Private Sub ResetHighlightImage()
        HighlightImage = Nothing
    End Sub

    <Category("K-BOT Icons")>
    <Description("Cheia pictogramei butonului «Culoarea fundalului», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property HighlightImageKey As String
        Get
            Return _highlightImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_highlightImageKey, wanted, StringComparison.Ordinal) Then Return
            _highlightImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Collapse")>
    <Description("Pictograma butonului cât timp editorul e DESFĂȘURAT; nesetată = unghiul desenat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property CollapseExpandedImage As Image
        Get
            Return _collapseExpandedImage
        End Get
        Set(value As Image)
            If _collapseExpandedImage Is value Then Return
            _collapseExpandedImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeCollapseExpandedImage() As Boolean
        Return _collapseExpandedImage IsNot Nothing
    End Function

    Private Sub ResetCollapseExpandedImage()
        CollapseExpandedImage = Nothing
    End Sub

    <Category("K-BOT Collapse")>
    <Description("Cheia pictogramei de «desfășurat», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property CollapseExpandedImageKey As String
        Get
            Return _collapseExpandedImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_collapseExpandedImageKey, wanted, StringComparison.Ordinal) Then Return
            _collapseExpandedImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    <Category("K-BOT Collapse")>
    <Description("Pictograma butonului cât timp editorul e STRÂNS; nesetată = unghiul desenat.")>
    <DefaultValue(GetType(Image), Nothing)>
    Public Property CollapseCollapsedImage As Image
        Get
            Return _collapseCollapsedImage
        End Get
        Set(value As Image)
            If _collapseCollapsedImage Is value Then Return
            _collapseCollapsedImage = value
            ApplyButtonIcons()
        End Set
    End Property

    Private Function ShouldSerializeCollapseCollapsedImage() As Boolean
        Return _collapseCollapsedImage IsNot Nothing
    End Function

    Private Sub ResetCollapseCollapsedImage()
        CollapseCollapsedImage = Nothing
    End Sub

    <Category("K-BOT Collapse")>
    <Description("Cheia pictogramei de «strâns», aleasă din lista legată la Images.")>
    <TypeConverter(GetType(RichTextImageKeyConverter))>
    <DefaultValue("")>
    Public Property CollapseCollapsedImageKey As String
        Get
            Return _collapseCollapsedImageKey
        End Get
        Set(value As String)
            Dim wanted As String = If(value, String.Empty)
            If String.Equals(_collapseCollapsedImageKey, wanted, StringComparison.Ordinal) Then Return
            _collapseCollapsedImageKey = wanted
            ApplyButtonIcons()
        End Set
    End Property

    ''' <summary>
    ''' Re-derives every button's picture. The explicit <c>*Image</c> wins, then the key
    ''' resolved against <see cref="Images"/>, and with neither the button falls back to its
    ''' lettered glyph -- so a mistyped key shows a readable toolbar, not five blank squares.
    ''' </summary>
    Private Sub ApplyButtonIcons()
        Try
            ApplyIcon(btnBold, _boldImage, _boldImageKey, GLYPH_BOLD)
            ApplyIcon(btnItalic, _italicImage, _italicImageKey, GLYPH_ITALIC)
            ApplyIcon(btnUnderline, _underlineImage, _underlineImageKey, GLYPH_UNDERLINE)
            ApplyIcon(btnTextColor, _textColorImage, _textColorImageKey, GLYPH_TEXT_COLOR)
            ApplyIcon(btnHighlight, _highlightImage, _highlightImageKey, GLYPH_HIGHLIGHT)

            If _collapsed Then
                ApplyIcon(btnCollapse, _collapseCollapsedImage, _collapseCollapsedImageKey, GLYPH_COLLAPSED)
            Else
                ApplyIcon(btnCollapse, _collapseExpandedImage, _collapseExpandedImageKey, GLYPH_EXPANDED)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyButtonIcons", ex)
        End Try
    End Sub

    Private Sub ApplyIcon(btn As KBotNoFocusButton, explicitImage As Image, key As String, glyph As String)
        ' An explicit picture belongs to the HOST; one resolved from a key is a fresh copy the
        ' list just handed us, so the button is told which of the two it got (see SetPicture).
        Dim picture As Image = explicitImage
        Dim owned As Boolean = False
        If picture Is Nothing Then
            picture = ImageForKey(key)
            owned = picture IsNot Nothing
        End If

        ' The layout goes on FIRST: the button decides from it whether the framework or its own
        ' OnPaint gets the picture, so setting the picture afterwards lands on the right path.
        btn.ImageLayout = _buttonImageLayout
        btn.SetPicture(picture, owned)
        ' Text and picture never share a button: the glyph is the FALLBACK, so it disappears
        ' the moment there is something better to show.
        btn.Text = If(picture Is Nothing, glyph, String.Empty)
        btn.ImageAlign = ContentAlignment.MiddleCenter
        btn.TextAlign = ContentAlignment.MiddleCenter
    End Sub

    ''' <summary>The picture behind a key, or <c>Nothing</c> when no list holds it.</summary>
    Private Function ImageForKey(key As String) As Image
        Try
            If _images Is Nothing OrElse String.IsNullOrEmpty(key) Then Return Nothing
            Dim i As Integer = _images.Images.IndexOfKey(key)
            If i < 0 Then Return Nothing
            Return _images.Images(i)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ImageForKey", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Starts listening to the bound list, because a list can be FILLED after it is bound.
    '''
    ''' <para>That is not a corner case, it is what every generated <c>.Designer.vb</c> does:
    ''' the component is created empty at the top of <c>InitializeComponent</c>, the editor's
    ''' properties are written in alphabetical order right after, and the list's
    ''' <c>ImageStream</c> and key names only arrive further down. <c>RecreateHandle</c> is
    ''' raised when the contents are replaced wholesale (an ImageStream loaded, ColorDepth or
    ''' ImageSize changed), which is exactly that moment.</para>
    '''
    ''' <para>It is not the whole answer on its own -- <c>SetKeyName</c> raises nothing -- so
    ''' the last word belongs to <c>OnHandleCreated</c>, by which time the designer file has
    ''' run to its end.</para>
    ''' </summary>
    Private Sub AttachImages()
        If _images Is Nothing Then Return
        AddHandler _images.RecreateHandle, AddressOf HandleImagesChanged
        AddHandler _images.Disposed, AddressOf HandleImagesDisposed
    End Sub

    Private Sub DetachImages()
        If _images Is Nothing Then Return
        RemoveHandler _images.RecreateHandle, AddressOf HandleImagesChanged
        RemoveHandler _images.Disposed, AddressOf HandleImagesDisposed
    End Sub

    Private Sub HandleImagesChanged(sender As Object, e As EventArgs)
        ApplyButtonIcons()
    End Sub

    ''' <summary>The host threw the list away: let go of it, so the buttons fall back to their
    ''' letters instead of pointing at a source that no longer exists.</summary>
    Private Sub HandleImagesDisposed(sender As Object, e As EventArgs)
        Images = Nothing
    End Sub

    ''' <summary>
    ''' Every button's picture, resolved again from <see cref="Images"/> as it stands NOW.
    '''
    ''' <para>The editor does this on its own when the list is bound, when it is replaced
    ''' wholesale and when the control gets its handle, which covers every host that binds a
    ''' list in the designer. A host that ADDS pictures to an already-bound list while the form
    ''' is on screen gets no such signal from <c>ImageList</c> -- this is its way to ask.</para>
    ''' </summary>
    Public Sub RefreshButtonIcons()
        ApplyButtonIcons()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' STRANGERE -- same contract as the grid, the tree and the nav bar
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Shows the button in the header that folds the editor away, leaving the toolbar. Off by
    ''' default, so no existing host grows a control it did not ask for.
    ''' </summary>
    <Category("K-BOT Collapse")>
    <Description("Arată în antet butonul care strânge/desfășoară suprafața de scris.")>
    <DefaultValue(False)>
    Public Property CollapseButton As Boolean
        Get
            Return _collapseButton
        End Get
        Set(value As Boolean)
            If _collapseButton = value Then Return
            _collapseButton = value
            ' Without the button there is no way back out of the folded state, so we unfold it
            ' ourselves -- the same care the grid and the nav bar take.
            If Not _collapseButton AndAlso _collapsed Then ApplyCollapsedState(False)
            RebuildLayout()
        End Set
    End Property

    ''' <summary>
    ''' Folded or not. RUNTIME STATE, not a designer value (as on the grid and the tree):
    ''' serialised, it would freeze the form folded and fight the <c>Size</c> the designer
    ''' writes. Setting it True without the button THROWS -- there would be no way back.
    ''' </summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Collapsed As Boolean
        Get
            Return _collapsed
        End Get
        Set(value As Boolean)
            If value = _collapsed Then Return
            If value AndAlso Not _collapseButton Then
                Throw New InvalidOperationException(
                    "Editorul nu se poate strânge cât timp CollapseButton e False.")
            End If
            ApplyCollapsedState(value)
        End Set
    End Property

    ''' <summary>
    ''' Flips the state. Unlike the <see cref="Collapsed"/> setter it does NOT throw when the
    ''' button is missing: this is a button press, not a request from code.
    ''' </summary>
    Public Sub ToggleCollapse()
        Try
            If Not _collapseButton Then Return
            ApplyCollapsedState(Not _collapsed)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ToggleCollapse", ex)
        End Try
    End Sub

    ''' <summary>True when the HOST decides our height: any dock, or anchoring on both edges.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HostOwnsHeight As Boolean
        Get
            If Dock <> DockStyle.None Then Return True
            Return (Anchor And AnchorStyles.Top) = AnchorStyles.Top AndAlso
                   (Anchor And AnchorStyles.Bottom) = AnchorStyles.Bottom
        End Get
    End Property

    ''' <summary>The height of the folded editor: the header band and nothing else. The host
    ''' that owns the height reads it to know where to put its splitter.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CollapsedHeight As Integer
        Get
            Return Math.Max(1, If(_headerVisible, ThemeShapes.ScaleDpi(Me, _headerHeight), 1))
        End Get
    End Property

    ''' <summary>The last height the editor had UNFOLDED -- read by a host that owns it.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ExpandedHeight As Integer
        Get
            Return _expandedHeight
        End Get
    End Property

    Private Sub ApplyCollapsedState(value As Boolean)
        If value = _collapsed Then Return
        ' The unfolded height is remembered BEFORE folding: afterwards it is already the folded one.
        If value AndAlso Height > 0 Then _expandedHeight = Height
        _collapsed = value
        ApplyButtonIcons()
        ApplyCollapseTooltip()
        ApplyCollapseExtent()
        RebuildLayout()
        RaiseEvent CollapsedChanged(_collapsed)
    End Sub

    ''' <summary>
    ''' Writes the height of the current state -- but NEVER fights the host's layout: if the
    ''' height is not ours, nothing is written. The state still changes and
    ''' <see cref="CollapsedChanged"/> still fires, so the host can move its splitter.
    ''' </summary>
    Private Sub ApplyCollapseExtent()
        Try
            If HostOwnsHeight Then Return
            Dim target As Integer = If(_collapsed, CollapsedHeight, _expandedHeight)
            If target <= 0 OrElse target = Height Then Return
            _applyingCollapseExtent = True
            Try
                Height = target
            Finally
                _applyingCollapseExtent = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyCollapseExtent", ex)
        End Try
    End Sub

    ''' <summary>The hover label has to say what the NEXT press does, not what the state is.</summary>
    Private Sub ApplyCollapseTooltip()
        Try
            If _collapsed Then
                tips.SetToolTipHeader(btnCollapse, "Desfășoară editorul")
                tips.SetToolTipText(btnCollapse, "Aduce înapoi suprafața de scris.")
            Else
                tips.SetToolTipHeader(btnCollapse, "Strânge editorul")
                tips.SetToolTipText(btnCollapse, "Pliază suprafața de scris și lasă doar bara de sus.")
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichTextEditor.ApplyCollapseTooltip", ex)
        End Try
    End Sub
End Class
