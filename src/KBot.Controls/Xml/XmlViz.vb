Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms
Imports System.Xml
Imports System.Xml.Linq
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Slice 00EF-36 -- shows an XML read only: on the left a MAP of its elements, on the right the text, coloured by what each part is.
''' Built for the e-invoice (UBL, CIUS-RO) but it reads any well-formed XML. Nothing is edited, nothing is validated: what it
''' is given is shown.
'''
''' <para><b>The map.</b> One node per element, in ROMANIAN (<see cref="XmlVizLabels"/>: «cbc:IssueDate» is «Data emiterii», «cac:Party»
''' under the supplier is «Date furnizor»); an element nobody translated keeps its name WITHOUT the prefix. The name as the file has it
''' is in the tooltip of the node. In the UBL files the prefix
''' only says whether the element has children (<c>cac:</c> = an aggregate, <c>cbc:</c> = a value), which the tree already shows by its
''' expander, and an aggregate is also bold. A value element shows its text on the right of the name. A click on a node selects that
''' element in the text.</para>
'''
''' <para><b>The text.</b> Written by this control from the parsed XML (two spaces per level, one element per line when it has
''' children), as RTF, so the position of every element is known and the colouring costs one assignment. A very long value (the
''' base64 of an attachment) is cut with the number of characters left out: it would freeze a RichTextBox.</para>
''' </summary>
Public Class XmlViz
    Implements IThemedContainer

    ' The colour table of the RTF: 1 punctuation, 2 element name, 3 attribute name, 4 attribute value, 5 text, 6 comment.
    Private Const CPunct As Integer = 1
    Private Const CTag As Integer = 2
    Private Const CAttrName As Integer = 3
    Private Const CAttrValue As Integer = 4
    Private Const CText As Integer = 5
    Private Const CComment As Integer = 6

    Private Const MaxValueChars As Integer = 400
    Private Const KeptValueChars As Integer = 200
    Private Const MaxCaptionValue As Integer = 60

    ' key of a map node -> (start, length) of the element's opening tag in the text.
    Private ReadOnly _spans As New Dictionary(Of String, Tuple(Of Integer, Integer))(StringComparer.Ordinal)
    Private _xml As String = String.Empty
    Private _notice As String = String.Empty

    ' While the text is built.
    Private _body As StringBuilder
    Private _pos As Integer
    Private _count As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Nothing shown (no invoice).</summary>
    Public Sub Clear()
        _xml = String.Empty
        _notice = String.Empty
        tree.Clear()
        _spans.Clear()
        rtb.Clear()
    End Sub

    ''' <summary>A plain line instead of a document («Se aduce XML-ul…»).</summary>
    Public Sub ShowNotice(k_text As String)
        _xml = String.Empty
        _notice = If(k_text, String.Empty)
        tree.Clear()
        _spans.Clear()
        rtb.Text = _notice
    End Sub

    ''' <summary>Shows <paramref name="k_xml"/>: the map and the coloured text. A text that is not well-formed is shown as it is.</summary>
    Public Sub ShowXml(k_xml As String)
        ArgumentNullException.ThrowIfNull(k_xml)
        _xml = k_xml
        _notice = String.Empty
        Build()
    End Sub

    ' ── Building ────────────────────────────────────────────────────────────────

    Private Sub Build()
        Try
            tree.Clear()
            _spans.Clear()
            If _xml.Length = 0 Then
                rtb.Clear()
                Return
            End If
            Dim k_doc As XDocument
            Try
                k_doc = XDocument.Parse(_xml)
            Catch ex As XmlException
                ' Not well-formed: nothing to map; the text is shown as it came.
                rtb.Text = "XML-ul nu este bine format: " & ex.Message & vbLf & vbLf & _xml
                Return
            End Try
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            Dim k_palette As ThemePalette = k_scheme.Palette
            _body = New StringBuilder(_xml.Length * 2)
            _pos = 0
            _count = 0
            For Each k_node As XNode In k_doc.Nodes()
                EmitNode(k_node, 0, Nothing)
            Next
            rtb.BackColor = k_palette.SurfaceColor
            rtb.ForeColor = k_palette.TextColor
            rtb.Rtf = BuildRtf(k_palette, _body.ToString())
            rtb.SelectionStart = 0
            rtb.ScrollToCaret()
            tree.Invalidate()
        Catch ex As Exception
            ' UI boundary (called from the host's handlers): the viewer is a convenience, so a failure is logged and the
            ' text is shown plain rather than nothing.
            GlobalErrorLog.Write("XmlViz.Build", ex)
            rtb.Text = _xml
        End Try
    End Sub

    Private Shared Function BuildRtf(k_palette As ThemePalette, k_body As String) As String
        Dim k_rtf As New StringBuilder(k_body.Length + 400)
        k_rtf.Append("{\rtf1\ansi\deff0{\fonttbl{\f0\fmodern Consolas;}}{\colortbl ;")
        For Each k_color As Color In {k_palette.TextDimColor, k_palette.AccentColor, k_palette.WarningColor, k_palette.SuccessColor,
                                      k_palette.TextColor, k_palette.TextDimColor}
            k_rtf.Append("\red").Append(k_color.R).Append("\green").Append(k_color.G).Append("\blue").Append(k_color.B).Append(";"c)
        Next
        k_rtf.Append("}\f0\fs20 ").Append(k_body).Append("}"c)
        Return k_rtf.ToString()
    End Function

    ' Appends text in a colour: the RTF is escaped, the position (what the RichTextBox counts) advances by the text's length.
    Private Sub Put(k_text As String, k_color As Integer)
        If k_text.Length = 0 Then Return
        _body.Append("\cf").Append(k_color).Append(" "c)
        For Each k_c As Char In k_text
            Select Case k_c
                Case "\"c : _body.Append("\\")
                Case "{"c : _body.Append("\{")
                Case "}"c : _body.Append("\}")
                Case Else
                    If AscW(k_c) > 127 Then
                        _body.Append("\u").Append(CShort(AscW(k_c))).Append("?"c)
                    ElseIf AscW(k_c) >= 32 Then
                        _body.Append(k_c)
                    End If
            End Select
        Next
        _pos += k_text.Length
    End Sub

    Private Sub NewLine(k_depth As Integer)
        _body.Append("\par ")
        _pos += 1
        If k_depth > 0 Then Put(New String(" "c, k_depth * 2), CText)
    End Sub

    ' A node of the document at the given depth; k_parent is the map node of its element (Nothing at the top).
    Private Sub EmitNode(k_node As XNode, k_depth As Integer, k_parent As AdvancedTreeControl.TreeItem)
        Dim k_element As XElement = TryCast(k_node, XElement)
        If k_element IsNot Nothing Then
            If _pos > 0 Then NewLine(k_depth)
            EmitElement(k_element, k_depth, k_parent)
            Return
        End If
        Dim k_comment As XComment = TryCast(k_node, XComment)
        If k_comment IsNot Nothing Then
            If _pos > 0 Then NewLine(k_depth)
            Put("<!--" & Shorten(k_comment.Value) & "-->", CComment)
        End If
    End Sub

    Private Sub EmitElement(k_el As XElement, k_depth As Integer, k_parent As AdvancedTreeControl.TreeItem)
        Dim k_prefix As String = k_el.GetPrefixOfNamespace(k_el.Name.Namespace)
        Dim k_name As String = If(String.IsNullOrEmpty(k_prefix), k_el.Name.LocalName, k_prefix & ":" & k_el.Name.LocalName)
        Dim k_hasChildren As Boolean = k_el.HasElements
        Dim k_textOnly As String = If(k_hasChildren, String.Empty, k_el.Value)

        ' The map: the name without its prefix; a value element shows its text.
        _count += 1
        Dim k_key As String = "X" & _count.ToString(CultureInfo.InvariantCulture)
        Dim k_caption As String = Clean(XmlVizLabels.LabelOf(k_el))
        If Not k_hasChildren AndAlso k_textOnly.Trim().Length > 0 Then
            Dim k_short As String = k_textOnly.Trim().Replace(vbCr, " ").Replace(vbLf, " ")
            If k_short.Length > MaxCaptionValue Then k_short = k_short.Substring(0, MaxCaptionValue) & "…"
            k_caption &= "~~~" & Clean(k_short)
        End If
        Dim k_item As AdvancedTreeControl.TreeItem = tree.AddItem(k_key, k_caption, k_parent, pExpanded:=(k_depth = 0))
        If k_hasChildren Then k_item.Bold = True
        ' The map speaks Romanian; the name as the file has it is in the tooltip.
        k_item.Tooltip = k_name

        ' The text: the opening tag with its attributes.
        Dim k_start As Integer = _pos
        Put("<", CPunct)
        Put(k_name, CTag)
        For Each k_attr As XAttribute In k_el.Attributes()
            Put(" ", CText)
            Dim k_attrName As String = If(k_attr.IsNamespaceDeclaration,
                                          If(k_attr.Name.LocalName = "xmlns" AndAlso k_attr.Name.NamespaceName.Length = 0, "xmlns", "xmlns:" & k_attr.Name.LocalName),
                                          If(String.IsNullOrEmpty(k_el.GetPrefixOfNamespace(k_attr.Name.Namespace)) OrElse k_attr.Name.Namespace = XNamespace.None,
                                             k_attr.Name.LocalName, k_el.GetPrefixOfNamespace(k_attr.Name.Namespace) & ":" & k_attr.Name.LocalName))
            Put(k_attrName, CAttrName)
            Put("=", CPunct)
            Put("""" & Shorten(EscapeValue(k_attr.Value)) & """", CAttrValue)
        Next
        If Not k_el.Nodes().Any() Then
            Put("/>", CPunct)
            _spans(k_key) = Tuple.Create(k_start, _pos - k_start)
            Return
        End If
        Put(">", CPunct)
        _spans(k_key) = Tuple.Create(k_start, _pos - k_start)

        If Not k_hasChildren Then
            Put(Shorten(EscapeValue(k_textOnly)), CText)
        Else
            For Each k_child As XNode In k_el.Nodes()
                If TypeOf k_child Is XText Then
                    Dim k_t As String = DirectCast(k_child, XText).Value.Trim()
                    If k_t.Length > 0 Then
                        NewLine(k_depth + 1)
                        Put(Shorten(EscapeValue(k_t)), CText)
                    End If
                Else
                    EmitNode(k_child, k_depth + 1, k_item)
                End If
            Next
            NewLine(k_depth)
        End If
        Put("</", CPunct)
        Put(k_name, CTag)
        Put(">", CPunct)
    End Sub

    Private Shared Function EscapeValue(k_value As String) As String
        Return k_value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
    End Function

    ' A value too long for a RichTextBox (the base64 of an attachment) is cut, and says how much was left out.
    Private Shared Function Shorten(k_value As String) As String
        If k_value.Length <= MaxValueChars Then Return k_value
        Return k_value.Substring(0, KeptValueChars) & $"… [+{(k_value.Length - KeptValueChars).ToString("N0", CultureInfo.CurrentCulture)} caractere]"
    End Function

    ' The map captions are mini-html: a «<» would start a tag.
    Private Shared Function Clean(k_text As String) As String
        Return k_text.Replace("<", "‹").Replace(">", "›")
    End Function

    ' ── The map selects in the text ─────────────────────────────────────────────

    ''' <summary>A click on a node of the map selects that element in the text. UI boundary: logs and swallows.</summary>
    Private Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If k_node Is Nothing OrElse e Is Nothing OrElse e.Button <> MouseButtons.Left Then Return
            Dim k_span As Tuple(Of Integer, Integer) = Nothing
            If Not _spans.TryGetValue(k_node.Key, k_span) Then Return
            rtb.Select(k_span.Item1, k_span.Item2)
            rtb.ScrollToCaret()
        Catch ex As Exception
            GlobalErrorLog.Write("XmlViz.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' ── Theme ───────────────────────────────────────────────────────────────────

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            BackColor = k_scheme.Palette.SurfaceAltColor
            rtb.BackColor = k_scheme.Palette.SurfaceColor
            rtb.ForeColor = k_scheme.Palette.TextColor
            ' The colours of the text are in the RTF: it is written again with the new palette.
            If _xml.Length > 0 Then Build()
        Catch ex As Exception
            GlobalErrorLog.Write("XmlViz.ApplyTheme", ex)
        End Try
    End Sub

End Class
