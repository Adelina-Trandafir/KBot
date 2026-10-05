Option Strict On
Imports System.Drawing
Imports System.Globalization
Imports System.Net
Imports System.Text.RegularExpressions
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Reads a small, forgiving subset of HTML into the runs <see cref="KBotRichText"/> already knows how
''' to wrap and draw (slice 000T-06). It is NOT a browser: it understands what an HTML editor writes for
''' «pretty text» and ignores the rest (the text inside an unknown tag stays, the tag goes).
'''
''' <para><b>Tags.</b> <c>b strong i em u s strike del small big mark code tt kbd span font div p br
''' h1..h6 ul ol li blockquote</c>; <c>style</c> / <c>script</c> / <c>head</c> / <c>title</c> are skipped with their
''' content, comments vanish. <b>Style:</b> <c>color</c>, <c>background-color</c>, <c>font-weight</c>,
''' <c>font-style</c>, <c>text-decoration</c>, <c>font-size</c> (px, pt, em, %, keywords) and
''' <c>font-family</c> (only «is it monospace»), on ANY tag via <c>style="..."</c>; <c>&lt;font color size&gt;</c>.
''' <b>Colours:</b> <c>#rgb</c>, <c>#rrggbb</c>, <c>rgb()</c>, a .NET colour name, or a THEME word
''' (<c>accent dim warning error success text</c>) that follows the scheme on screen.</para>
'''
''' <para><b>Whitespace</b> is collapsed like in a browser (line breaks in the source are spaces);
''' a break comes only from <c>br</c> or a block tag. A text with none of the tags above is PLAIN: shown as
''' written (its line breaks are line breaks, <c>&amp;</c> stays <c>&amp;</c>), so the older tutorials look as
''' before.</para>
'''
''' <para>Lists are one line per item with «•» or «1.» in front; a wrapped item line is not indented
''' (<see cref="KBotRichText"/> has no hanging indent). Alignment, tables and images are not drawn.</para>
'''
''' <para><b>Links (slice 000T-09).</b> <c>&lt;link tutorial="id"&gt;text&lt;/link&gt;</c> (or <c>&lt;a tutorial="id"&gt;</c>) is drawn
''' underlined in the accent colour and carries <c>id</c> in <see cref="KBotRichText.RichRun.Link"/>; what a click
''' does is up to the host (<see cref="KBotHtmlLabel.LinkClicked"/>). A link without <c>tutorial</c> is only drawn.</para>
''' </summary>
Public Module KBotHtmlText

    ' The tags that make a text HTML. A text that only HAS a «<» («<Enter>», «a < b») stays plain.
    Private ReadOnly KnownTag As New Regex(
        "</?(?:b|strong|i|em|u|s|strike|del|br|p|div|span|font|ul|ol|li|h[1-6]|small|big|mark|code|tt|kbd|blockquote|link|a)(?:\s[^>]*)?/?>",
        RegexOptions.IgnoreCase Or RegexOptions.Compiled)

    ''' <summary>True when the text holds at least one tag this reader understands.</summary>
    Public Function LooksLikeHtml(k_text As String) As Boolean
        Return Not String.IsNullOrEmpty(k_text) AndAlso KnownTag.IsMatch(k_text)
    End Function

    ''' <summary>
    ''' The runs of <paramref name="k_text"/> (HTML or plain). The fonts the runs use are created into
    ''' <paramref name="k_fonts"/>, which the CALLER owns and disposes; <paramref name="k_baseFont"/> is only
    ''' read. <paramref name="k_palette"/> resolves the theme colour words and may be Nothing.
    ''' </summary>
    Public Function ToRuns(k_text As String, k_baseFont As Font, k_baseColor As Color, k_palette As ThemePalette,
                           k_fonts As Dictionary(Of String, Font)) As List(Of KBotRichText.RichRun)
        Try
            If k_baseFont Is Nothing Then Throw New ArgumentNullException(NameOf(k_baseFont))
            If k_fonts Is Nothing Then Throw New ArgumentNullException(NameOf(k_fonts))
            If String.IsNullOrEmpty(k_text) Then Return New List(Of KBotRichText.RichRun)()
            Dim k_builder As New HtmlRunBuilder(k_baseFont, k_baseColor, k_palette, k_fonts)
            Return If(LooksLikeHtml(k_text), k_builder.BuildHtml(k_text), k_builder.BuildPlain(k_text))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHtmlText.ToRuns", ex)
            Throw
        End Try
    End Function

End Module

''' <summary>The formatting in force at one point of the HTML; copied when a tag opens, put back when it closes.</summary>
Friend Structure HtmlStyle
    Public Bold As Boolean
    Public Italic As Boolean
    Public Underline As Boolean
    Public Strike As Boolean
    Public Mono As Boolean
    ''' <summary>Multiplier of the base font size.</summary>
    Public Size As Single
    ''' <summary>Color.Empty = the base colour.</summary>
    Public Fore As Color
    ''' <summary>Color.Empty = no background.</summary>
    Public Back As Color
    ''' <summary>Slice 000T-09: the id a click asks for inside a link element; Nothing = not in a link.</summary>
    Public Link As String
End Structure

''' <summary>One pass over the tags and text of an HTML fragment (see <see cref="KBotHtmlText"/>).</summary>
Friend NotInheritable Class HtmlRunBuilder

    Private Shared ReadOnly Nbsp As String = ChrW(160)

    ' A comment, or a tag: «</», name, then the attributes (quoted values may hold «>»).
    Private Shared ReadOnly TokenPattern As New Regex(
        "<!--.*?-->|<(/?)([A-Za-z][A-Za-z0-9]*)((?:""[^""]*""|'[^']*'|[^>""'])*)>",
        RegexOptions.Singleline Or RegexOptions.Compiled)
    Private Shared ReadOnly AttrPattern As New Regex(
        "([A-Za-z][A-Za-z0-9_-]*)\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s""'>]+))", RegexOptions.Compiled)
    Private Shared ReadOnly SpacePattern As New Regex("[ \t\r\n\f]+", RegexOptions.Compiled)

    Private NotInheritable Class ListState
        Public Ordered As Boolean
        Public Counter As Integer
    End Class

    Private ReadOnly _baseFont As Font
    Private ReadOnly _baseColor As Color
    Private ReadOnly _palette As ThemePalette
    Private ReadOnly _fonts As Dictionary(Of String, Font)
    Private ReadOnly _runs As New List(Of KBotRichText.RichRun)()
    Private ReadOnly _stack As New List(Of (Name As String, Saved As HtmlStyle))()
    Private ReadOnly _lists As New Stack(Of ListState)()

    Private _style As HtmlStyle
    Private _skipName As String = String.Empty
    Private _hasContent As Boolean
    Private _spaceSuppressed As Boolean
    Private _pendingSpace As Boolean
    Private _brCount As Integer
    Private _blockPending As Integer

    Public Sub New(k_baseFont As Font, k_baseColor As Color, k_palette As ThemePalette, k_fonts As Dictionary(Of String, Font))
        _baseFont = k_baseFont
        _baseColor = k_baseColor
        _palette = k_palette
        _fonts = k_fonts
        _style = PlainStyle()
    End Sub

    Private Shared Function PlainStyle() As HtmlStyle
        Return New HtmlStyle With {.Size = 1.0F}
    End Function

    ' ── entry points ─────────────────────────────────────────────────────────────

    ''' <summary>Plain text as written; each blank line gets a non-breaking space so it keeps a full line's height.</summary>
    Public Function BuildPlain(k_text As String) As List(Of KBotRichText.RichRun)
        Dim k_t As String = Regex.Replace(k_text.Replace(vbCrLf, vbLf), vbLf & "(?=" & vbLf & ")", vbLf & Nbsp)
        Emit(k_t)
        Return _runs
    End Function

    Public Function BuildHtml(k_html As String) As List(Of KBotRichText.RichRun)
        Dim k_pos As Integer = 0
        For Each k_m As Match In TokenPattern.Matches(k_html)
            If k_m.Index > k_pos Then AddText(k_html.Substring(k_pos, k_m.Index - k_pos))
            k_pos = k_m.Index + k_m.Length
            If k_m.Value.StartsWith("<!--", StringComparison.Ordinal) Then Continue For
            HandleTag(k_m.Groups(2).Value.ToLowerInvariant(), k_m.Groups(1).Value = "/", k_m.Groups(3).Value)
        Next
        If k_pos < k_html.Length Then AddText(k_html.Substring(k_pos))
        Return _runs
    End Function

    ' ── text and breaks ──────────────────────────────────────────────────────────

    Private Sub AddText(k_raw As String)
        If _skipName.Length > 0 Then Return
        Dim k_text As String = SpacePattern.Replace(WebUtility.HtmlDecode(k_raw), " ")
        If k_text.Length = 0 Then Return
        Dim k_lead As Boolean = k_text(0) = " "c
        Dim k_trail As Boolean = k_text(k_text.Length - 1) = " "c
        Dim k_core As String = k_text.Trim(" "c)
        If k_core.Length = 0 Then
            If _hasContent Then _pendingSpace = True
            Return
        End If
        FlushBreaks()
        Dim k_space As Boolean = (k_lead OrElse _pendingSpace) AndAlso _hasContent AndAlso Not _spaceSuppressed
        Emit(If(k_space, " ", String.Empty) & k_core)
        _pendingSpace = k_trail
    End Sub

    ' Writes the line ends asked for since the last text. Nothing before the first text or after the last one.
    Private Sub FlushBreaks()
        Dim k_need As Integer = Math.Max(_brCount, _blockPending)
        Dim k_fullBlank As Boolean = _brCount >= 2
        _brCount = 0
        _blockPending = 0
        If Not _hasContent OrElse k_need <= 0 Then Return
        _runs.Add(MakeRun(vbLf, PlainStyle()))
        If k_need >= 2 Then
            ' A blank line is a non-breaking space in its own font: a full line after «br br», a smaller gap
            ' between paragraphs and around lists.
            Dim k_gap As HtmlStyle = PlainStyle()
            If k_fullBlank Then k_gap.Size = _style.Size Else k_gap.Size = 0.55F
            For i As Integer = 2 To k_need
                _runs.Add(MakeRun(Nbsp & vbLf, k_gap))
            Next
        End If
        _spaceSuppressed = True
        _pendingSpace = False
    End Sub

    Private Sub Emit(k_text As String)
        _runs.Add(MakeRun(k_text, _style))
        _hasContent = True
        _spaceSuppressed = False
    End Sub

    Private Function MakeRun(k_text As String, k_style As HtmlStyle) As KBotRichText.RichRun
        Return New KBotRichText.RichRun With {
            .Text = k_text,
            .Font = FontFor(k_style),
            .ForeColor = If(k_style.Fore <> Color.Empty, k_style.Fore, _baseColor),
            .BackColor = k_style.Back,
            .HasBackColor = k_style.Back <> Color.Empty,
            .Link = k_style.Link}
    End Function

    Private Function FontFor(k_style As HtmlStyle) As Font
        Dim k_fs As FontStyle = _baseFont.Style
        If k_style.Bold Then k_fs = k_fs Or FontStyle.Bold
        If k_style.Italic Then k_fs = k_fs Or FontStyle.Italic
        If k_style.Underline Then k_fs = k_fs Or FontStyle.Underline
        If k_style.Strike Then k_fs = k_fs Or FontStyle.Strikeout
        Dim k_size As Single = CSng(Math.Max(5.0, _baseFont.SizeInPoints * k_style.Size))
        Dim k_key As String = CInt(k_fs).ToString(CultureInfo.InvariantCulture) & "|" &
                              k_size.ToString("F2", CultureInfo.InvariantCulture) & "|" & k_style.Mono
        Dim k_font As Font = Nothing
        If _fonts.TryGetValue(k_key, k_font) Then Return k_font
        Dim k_name As String = If(k_style.Mono, "Consolas", _baseFont.FontFamily.Name)
        Using k_family As New FontFamily(k_name)
            If Not k_family.IsStyleAvailable(k_fs) Then k_fs = FontStyle.Regular
            k_font = New Font(k_family, k_size, k_fs, GraphicsUnit.Point)
        End Using
        _fonts.Add(k_key, k_font)
        Return k_font
    End Function

    ' ── tags ─────────────────────────────────────────────────────────────────────

    Private Sub HandleTag(k_name As String, k_closing As Boolean, k_attrs As String)
        If _skipName.Length > 0 Then
            If k_closing AndAlso k_name = _skipName Then _skipName = String.Empty
            Return
        End If
        Select Case k_name
            Case "style", "script", "head", "title"
                If Not k_closing AndAlso Not k_attrs.TrimEnd().EndsWith("/", StringComparison.Ordinal) Then _skipName = k_name
                Return
            Case "br"
                If Not k_closing Then _brCount += 1
                Return
            Case "hr"
                RequestBlock(2)
                Return
            Case "img", "meta", "input"
                Return
        End Select
        If k_closing Then CloseElement(k_name) Else OpenElement(k_name, k_attrs)
    End Sub

    Private Sub RequestBlock(k_lines As Integer)
        _blockPending = Math.Max(_blockPending, k_lines)
    End Sub

    Private Sub OpenElement(k_name As String, k_attrs As String)
        Dim k_selfClosed As Boolean = k_attrs.TrimEnd().EndsWith("/", StringComparison.Ordinal)
        Dim k_saved As HtmlStyle = _style
        Select Case k_name
            Case "b", "strong" : _style.Bold = True
            Case "i", "em" : _style.Italic = True
            Case "u" : _style.Underline = True
            Case "s", "strike", "del" : _style.Strike = True
            Case "small" : _style.Size *= 0.85F
            Case "big" : _style.Size *= 1.2F
            Case "mark" : _style.Back = MarkColor()
            Case "code", "tt", "kbd" : _style.Mono = True
            Case "link", "a"
                _style.Underline = True
                If _palette IsNot Nothing Then _style.Fore = _palette.AccentColor
            Case "h1" : RequestBlock(2) : _style.Bold = True : _style.Size *= 1.5F
            Case "h2" : RequestBlock(2) : _style.Bold = True : _style.Size *= 1.3F
            Case "h3" : RequestBlock(2) : _style.Bold = True : _style.Size *= 1.15F
            Case "h4", "h5", "h6" : RequestBlock(2) : _style.Bold = True
            Case "p" : RequestBlock(2)
            Case "div", "blockquote" : RequestBlock(1)
            Case "ul", "ol"
                RequestBlock(If(_lists.Count = 0, 2, 1))
                _lists.Push(New ListState With {.Ordered = (k_name = "ol")})
            Case "li"
                RequestBlock(1)
                WriteMarker()
        End Select

        Dim k_values As Dictionary(Of String, String) = ReadAttributes(k_attrs)
        If k_name = "font" Then
            Dim k_value As String = Nothing
            If k_values.TryGetValue("color", k_value) Then ApplyColor(k_value, True)
            If k_values.TryGetValue("size", k_value) Then ApplyFontSizeAttribute(k_value)
        End If
        If k_name = "link" OrElse k_name = "a" Then
            Dim k_target As String = Nothing
            If k_values.TryGetValue("tutorial", k_target) AndAlso k_target.Trim().Length > 0 Then _style.Link = k_target.Trim()
        End If
        Dim k_css As String = Nothing
        If k_values.TryGetValue("style", k_css) Then ApplyCss(k_css)

        If Not k_selfClosed Then _stack.Add((k_name, k_saved))
    End Sub

    Private Sub CloseElement(k_name As String)
        For i As Integer = _stack.Count - 1 To 0 Step -1
            If _stack(i).Name <> k_name Then Continue For
            _style = _stack(i).Saved
            _stack.RemoveRange(i, _stack.Count - i)
            Exit For
        Next
        Select Case k_name
            Case "h1", "h2", "h3", "h4", "h5", "h6", "p" : RequestBlock(2)
            Case "div", "blockquote", "li" : RequestBlock(1)
            Case "ul", "ol"
                If _lists.Count > 0 Then _lists.Pop()
                RequestBlock(If(_lists.Count = 0, 2, 1))
        End Select
    End Sub

    ' «•» or «1.» in front of the item, indented four spaces per nesting level.
    Private Sub WriteMarker()
        Dim k_list As ListState = If(_lists.Count > 0, _lists.Peek(), Nothing)
        Dim k_indent As String = New String(ChrW(160), 4 * Math.Max(0, _lists.Count - 1))
        Dim k_mark As String
        If k_list IsNot Nothing AndAlso k_list.Ordered Then
            k_list.Counter += 1
            k_mark = k_list.Counter.ToString(CultureInfo.InvariantCulture) & "." & Nbsp
        Else
            k_mark = ChrW(&H2022) & Nbsp & Nbsp
        End If
        FlushBreaks()
        Emit(k_indent & k_mark)
        _spaceSuppressed = True
        _pendingSpace = False
    End Sub

    ' ── attributes and styles ────────────────────────────────────────────────────

    Private Shared Function ReadAttributes(k_attrs As String) As Dictionary(Of String, String)
        Dim k_result As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each k_m As Match In AttrPattern.Matches(k_attrs)
            Dim k_value As String = If(k_m.Groups(2).Success, k_m.Groups(2).Value, If(k_m.Groups(3).Success, k_m.Groups(3).Value, k_m.Groups(4).Value))
            k_result(k_m.Groups(1).Value) = WebUtility.HtmlDecode(k_value)
        Next
        Return k_result
    End Function

    Private Sub ApplyCss(k_css As String)
        For Each k_decl As String In k_css.Split(";"c)
            Dim k_at As Integer = k_decl.IndexOf(":"c)
            If k_at <= 0 Then Continue For
            Dim k_prop As String = k_decl.Substring(0, k_at).Trim().ToLowerInvariant()
            Dim k_value As String = k_decl.Substring(k_at + 1).Trim().ToLowerInvariant()
            Select Case k_prop
                Case "color" : ApplyColor(k_value, True)
                Case "background-color", "background" : ApplyColor(k_value, False)
                Case "font-weight" : ApplyWeight(k_value)
                Case "font-style" : ApplyFontStyle(k_value)
                Case "text-decoration", "text-decoration-line" : ApplyDecoration(k_value)
                Case "font-size" : ApplyFontSize(k_value)
                Case "font-family"
                    If k_value.Contains("consolas") OrElse k_value.Contains("courier") OrElse k_value.Contains("monospace") Then _style.Mono = True
            End Select
        Next
    End Sub

    Private Sub ApplyColor(k_value As String, k_foreground As Boolean)
        Dim k_color As Color = ParseColor(k_value)
        If k_color = Color.Empty Then Return
        If k_foreground Then _style.Fore = k_color Else _style.Back = k_color
    End Sub

    Private Sub ApplyWeight(k_value As String)
        Dim k_number As Integer
        If k_value = "bold" OrElse k_value = "bolder" Then
            _style.Bold = True
        ElseIf k_value = "normal" OrElse k_value = "lighter" Then
            _style.Bold = False
        ElseIf Integer.TryParse(k_value, NumberStyles.Integer, CultureInfo.InvariantCulture, k_number) Then
            _style.Bold = k_number >= 600
        End If
    End Sub

    Private Sub ApplyFontStyle(k_value As String)
        If k_value = "italic" OrElse k_value = "oblique" Then
            _style.Italic = True
        ElseIf k_value = "normal" Then
            _style.Italic = False
        End If
    End Sub

    Private Sub ApplyDecoration(k_value As String)
        If k_value.Contains("none") Then
            _style.Underline = False
            _style.Strike = False
            Return
        End If
        If k_value.Contains("underline") Then _style.Underline = True
        If k_value.Contains("line-through") Then _style.Strike = True
    End Sub

    ' px and pt are absolute (against the base font); em, rem and % are against the size in force.
    Private Sub ApplyFontSize(k_value As String)
        Dim k_number As Double
        Select Case k_value
            Case "xx-small" : _style.Size = 0.6F : Return
            Case "x-small" : _style.Size = 0.7F : Return
            Case "small" : _style.Size = 0.85F : Return
            Case "medium" : _style.Size = 1.0F : Return
            Case "large" : _style.Size = 1.2F : Return
            Case "x-large" : _style.Size = 1.5F : Return
            Case "xx-large" : _style.Size = 2.0F : Return
            Case "larger" : _style.Size = ClampSize(_style.Size * 1.2F) : Return
            Case "smaller" : _style.Size = ClampSize(_style.Size * 0.85F) : Return
        End Select
        If k_value.EndsWith("px", StringComparison.Ordinal) AndAlso NumberOf(k_value, 2, k_number) Then
            _style.Size = ClampSize(CSng(k_number * 0.75 / _baseFont.SizeInPoints))
        ElseIf k_value.EndsWith("pt", StringComparison.Ordinal) AndAlso NumberOf(k_value, 2, k_number) Then
            _style.Size = ClampSize(CSng(k_number / _baseFont.SizeInPoints))
        ElseIf k_value.EndsWith("rem", StringComparison.Ordinal) AndAlso NumberOf(k_value, 3, k_number) Then
            _style.Size = ClampSize(CSng(k_number))
        ElseIf k_value.EndsWith("em", StringComparison.Ordinal) AndAlso NumberOf(k_value, 2, k_number) Then
            _style.Size = ClampSize(CSng(_style.Size * k_number))
        ElseIf k_value.EndsWith("%", StringComparison.Ordinal) AndAlso NumberOf(k_value, 1, k_number) Then
            _style.Size = ClampSize(CSng(_style.Size * k_number / 100.0))
        End If
    End Sub

    ' <font size="1".."7"> (3 = the normal size), or "+1" / "-1" against 3.
    Private Sub ApplyFontSizeAttribute(k_value As String)
        Dim k_steps As Single() = {0.63F, 0.82F, 1.0F, 1.17F, 1.5F, 2.0F, 3.0F}
        Dim k_number As Integer
        If Not Integer.TryParse(k_value.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, k_number) Then Return
        Dim k_index As Integer = If(k_value.Trim().StartsWith("+", StringComparison.Ordinal) OrElse k_value.Trim().StartsWith("-", StringComparison.Ordinal), 3 + k_number, k_number)
        _style.Size = k_steps(Math.Max(1, Math.Min(7, k_index)) - 1)
    End Sub

    Private Shared Function NumberOf(k_value As String, k_unitLength As Integer, ByRef k_number As Double) As Boolean
        Return Double.TryParse(k_value.Substring(0, k_value.Length - k_unitLength).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, k_number)
    End Function

    Private Shared Function ClampSize(k_factor As Single) As Single
        Return Math.Max(0.5F, Math.Min(4.0F, k_factor))
    End Function

    ' The highlighter: the scheme's warning colour thinned into the surface, so it reads on every scheme.
    Private Function MarkColor() As Color
        If _palette Is Nothing Then Return Color.Empty
        Return ThemeShapes.Blend(_palette.SurfaceColor, _palette.WarningColor, 0.45)
    End Function

    ' Never throws: a colour that is not understood is no colour (Color.Empty), and the text keeps its own.
    Private Function ParseColor(k_value As String) As Color
        Dim k_v As String = k_value.Trim().ToLowerInvariant()
        If k_v.Length = 0 OrElse k_v = "transparent" OrElse k_v = "inherit" OrElse k_v = "initial" Then Return Color.Empty
        If _palette IsNot Nothing Then
            Select Case k_v
                Case "accent" : Return _palette.AccentColor
                Case "dim" : Return _palette.TextDimColor
                Case "warning" : Return _palette.WarningColor
                Case "error" : Return _palette.ErrorColor
                Case "success" : Return _palette.SuccessColor
                Case "text" : Return _palette.TextColor
            End Select
        End If
        If k_v.StartsWith("#", StringComparison.Ordinal) Then
            Dim k_hex As String = k_v.Substring(1)
            If k_hex.Length = 3 Then k_hex = String.Concat(k_hex.Select(Function(k_digit) New String(k_digit, 2)))
            Dim k_rgb As Integer
            If k_hex.Length = 6 AndAlso Integer.TryParse(k_hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, k_rgb) Then
                Return Color.FromArgb(255, Color.FromArgb(k_rgb))
            End If
            Return Color.Empty
        End If
        If k_v.StartsWith("rgb", StringComparison.Ordinal) Then
            Dim k_open As Integer = k_v.IndexOf("("c)
            Dim k_close As Integer = k_v.IndexOf(")"c)
            If k_open < 0 OrElse k_close < k_open Then Return Color.Empty
            Dim k_parts As String() = k_v.Substring(k_open + 1, k_close - k_open - 1).Split({","c, " "c, "/"c}, StringSplitOptions.RemoveEmptyEntries)
            Dim k_r, k_g, k_b As Integer
            If k_parts.Length >= 3 AndAlso Integer.TryParse(k_parts(0), NumberStyles.Integer, CultureInfo.InvariantCulture, k_r) AndAlso
               Integer.TryParse(k_parts(1), NumberStyles.Integer, CultureInfo.InvariantCulture, k_g) AndAlso
               Integer.TryParse(k_parts(2), NumberStyles.Integer, CultureInfo.InvariantCulture, k_b) Then
                Return Color.FromArgb(255, Math.Max(0, Math.Min(255, k_r)), Math.Max(0, Math.Min(255, k_g)), Math.Max(0, Math.Min(255, k_b)))
            End If
            Return Color.Empty
        End If
        Dim k_named As Color = Color.FromName(k_v)
        Return If(k_named.IsKnownColor, k_named, Color.Empty)
    End Function

End Class
