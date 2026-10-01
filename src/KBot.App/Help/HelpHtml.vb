Option Strict On
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports KBot.Theming
Imports Markdig
Imports Markdig.Renderers
Imports Markdig.Renderers.Html
Imports Markdig.Syntax
Imports Markdig.Syntax.Inlines

''' <summary>
''' Turns help topics into HTML (slice 0000-01): one page for the help window, one document for
''' the manual. Both read the same Markdown, so the manual can never say something the window
''' does not.
'''
''' <para><b>The window's page is the whole help</b> (slice 0000-32): <see cref="Book"/> puts the
''' start page and every visible topic, in contents order, in ONE document, each in its own
''' section with an id. The window scrolls to a section instead of loading a page, and the same
''' document prints (its <c>@media print</c> rules are the paper look; a section marked
''' <see cref="NoPrintClass"/> stays out of the print).</para>
'''
''' <para><b>Links.</b> <c>[text](topic:some.id)</c> points at another topic. In the window it
''' stays <c>topic:some.id</c> (the window catches the navigation); in the manual it becomes an
''' anchor inside the document.</para>
'''
''' <para><b>Pictures.</b> <c>![caption](img/name.png)</c>, relative to the Help folder, embedded
''' in the page as data, so the exported manual is one file that can be sent as is. A picture
''' that is not on disk yet shows a visible «picture missing» box with its name.</para>
''' </summary>
Public NotInheritable Class HelpHtml

    Public Const TopicScheme As String = "topic:"

    ''' <summary>Link to a guided tour (slice 0000-04); the help window starts it.</summary>
    Public Const TourScheme As String = "tour:"

    ''' <summary>Slice 0000-32: id of the start section of <see cref="Book"/>.</summary>
    Public Const HomeAnchor As String = "home"

    ''' <summary>Slice 0000-32: between a topic's anchor and the anchor of one of its sections.</summary>
    Public Const SectionSeparator As String = "--"

    ''' <summary>Slice 0000-32: the class that keeps a section of <see cref="Book"/> out of the print.</summary>
    Public Const NoPrintClass As String = "noprint"

    ''' <summary>Slice 0000-32: the classes of a topic's section in <see cref="Book"/>.</summary>
    Public Const TopicClass As String = "sec topic"

    ''' <summary>
    ''' Slice 0000-32: the classes of a part's heading section in <see cref="Book"/>; every part
    ''' but the first starts a new printed page.
    ''' </summary>
    Friend Shared Function PartClass(first As Boolean) As String
        Return If(first, "sec partsec", "sec partsec pb")
    End Function

    ''' <summary>Id of a topic's section (the manual, <see cref="Book"/>).</summary>
    Friend Shared Function TopicAnchor(id As String) As String
        Return "t-" & id.Replace(".", "-")
    End Function

    ''' <summary>Slice 0000-32: id of a part's heading section in <see cref="Book"/>.</summary>
    Friend Shared Function PartAnchor(part As HelpPart) As String
        Return "p-" & part.ToString().ToLowerInvariant()
    End Function

    ''' <summary>Slice 0000-32: id of one <c>## </c> section of a topic in <see cref="Book"/>.</summary>
    Friend Shared Function SectionAnchor(topicId As String, section As String) As String
        Return TopicAnchor(topicId) & SectionSeparator & section
    End Function

    ' «▶ Tur ghidat: ...» links for the tours offered by one topic (or, with Nothing, all visible).
    Private Shared Sub AppendTourLinks(sb As StringBuilder, tours As IEnumerable(Of HelpTour))
        For Each t As HelpTour In tours
            sb.Append("<p class='tour'><a href='").Append(TourScheme).Append(WebUtility.HtmlEncode(t.Id)).Append("'>▶ Tur ghidat: ") _
              .Append(WebUtility.HtmlEncode(t.Title)).Append("</a></p>")
        Next
    End Sub

    Private Shared ReadOnly Pipeline As MarkdownPipeline =
        New MarkdownPipelineBuilder().UsePipeTables().UseEmphasisExtras().UseListExtras().UseAutoLinks().Build()

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Slice 0000-32: the help window's document -- the start page, then every part in
    ''' <paramref name="parts"/> with its topics in contents order, each in a section of its own
    ''' (<see cref="HomeAnchor"/>, <see cref="PartAnchor"/>, <see cref="TopicAnchor"/>). The
    ''' sections touch (padding, no margins between them), so any point of the page is inside one.
    ''' </summary>
    Public Shared Function Book(library As HelpLibrary, parts As IReadOnlyCollection(Of HelpPart)) As String
        Dim sb As New StringBuilder()
        sb.Append(PageHead("Ajutor K-BOT", book:=True))
        sb.Append("<div class='sec' id='").Append(HomeAnchor).Append("'>")
        AppendHome(sb, library, parts)
        sb.Append("</div>")
        Dim first As Boolean = True
        For Each part As HelpPart In parts
            sb.Append("<div class='").Append(PartClass(first)).Append("' id='").Append(PartAnchor(part)).Append("'><h1>") _
              .Append(WebUtility.HtmlEncode(HelpTopic.PartTitle(part))).Append("</h1></div>")
            first = False
            For Each t As HelpTopic In library.InReadingOrder(part)
                sb.Append("<div class='").Append(TopicClass).Append("' id='").Append(TopicAnchor(t.Id)).Append("'>")
                AppendTopic(sb, library, t)
                sb.Append("</div>")
            Next
        Next
        sb.Append("</body></html>")
        Return sb.ToString()
    End Function

    ' One topic of the book: where it is, heading, its tours, body, then its sub-topics.
    Private Shared Sub AppendTopic(sb As StringBuilder, library As HelpLibrary, topic As HelpTopic)
        sb.Append("<div class='crumbs'>").Append(Crumbs(library, topic)).Append("</div>")
        sb.Append("<h1>").Append(WebUtility.HtmlEncode(topic.Title)).Append("</h1>")
        AppendTourLinks(sb, library.Tours.Where(Function(t) String.Equals(t.TopicId, topic.Id, StringComparison.OrdinalIgnoreCase)))
        sb.Append(RenderBody(library, topic.Body, False, TopicAnchor(topic.Id) & SectionSeparator, Nothing))

        Dim kids As List(Of HelpTopic) = library.Children(topic.Part, topic.Id)
        If kids.Count > 0 Then
            sb.Append("<div class='kids'><h3>În acest capitol</h3><ul>")
            For Each k As HelpTopic In kids
                sb.Append("<li><a href='").Append(TopicScheme).Append(WebUtility.HtmlEncode(k.Id)).Append("'>") _
                  .Append(WebUtility.HtmlEncode(k.Title)).Append("</a></li>")
            Next
            sb.Append("</ul></div>")
        End If
    End Sub

    ' The start page: each visible part with its first-level chapters, then the tours.
    Private Shared Sub AppendHome(sb As StringBuilder, library As HelpLibrary, parts As IReadOnlyCollection(Of HelpPart))
        sb.Append("<h1>Ajutor K-BOT</h1>")
        sb.Append("<p>Apasă <b>F1</b> în orice fereastră ca să ajungi direct la pagina despre ce ai pe ecran; butonul <b>?</b> din bara de titlu ")
        sb.Append("deschide un mic meniu cu o căsuță de căutare, pagina ecranului și tururile ghidate. ")
        sb.Append("Cuprinsul din stânga are tot ajutorul; în căsuța de deasupra lui poți scrie o întrebare, cu cuvintele tale. ")
        sb.Append("Tot ajutorul e pe pagina aceasta: derulează în jos și treci dintr-un subiect în următorul.</p>")
        For Each part As HelpPart In parts
            sb.Append("<h2>").Append(WebUtility.HtmlEncode(HelpTopic.PartTitle(part))).Append("</h2><ul>")
            Dim roots As List(Of HelpTopic) = library.Children(part, String.Empty)
            If roots.Count = 0 Then sb.Append("<li class='dim'>(încă nescrisă)</li>")
            For Each t As HelpTopic In roots
                sb.Append("<li><a href='").Append(TopicScheme).Append(WebUtility.HtmlEncode(t.Id)).Append("'>") _
                  .Append(WebUtility.HtmlEncode(t.Title)).Append("</a></li>")
            Next
            sb.Append("</ul>")
        Next
        Dim tours As List(Of HelpTour) = library.Tours.Where(Function(t) parts.Contains(t.Part)).ToList()
        If tours.Count > 0 Then
            sb.Append("<h2>Tururi ghidate</h2><p>K-BOT îți arată pe ecran, pas cu pas, unde e fiecare lucru.</p>")
            AppendTourLinks(sb, tours)
        End If
        If library.Problems.Count > 0 Then
            sb.Append("<blockquote><b>Unele pagini de ajutor n-au putut fi citite</b> (").Append(library.Problems.Count) _
              .Append("). Detaliile sunt în jurnalul de erori.</blockquote>")
        End If
    End Sub

    ''' <summary>A plain message page (no topic found, help folder missing...).</summary>
    Public Shared Function MessagePage(title As String, paragraphs As IEnumerable(Of String)) As String
        Dim sb As New StringBuilder()
        sb.Append(PageHead(WebUtility.HtmlEncode(title)))
        sb.Append("<h1>").Append(WebUtility.HtmlEncode(title)).Append("</h1>")
        For Each p As String In paragraphs
            sb.Append("<p>").Append(WebUtility.HtmlEncode(p)).Append("</p>")
        Next
        sb.Append("</body></html>")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' The whole manual: cover, contents, then every visible part in reading order, each part on
    ''' a new printed page. Printed from a browser it gives the PDF.
    ''' </summary>
    Public Shared Function Manual(library As HelpLibrary, parts As IReadOnlyCollection(Of HelpPart), version As String) As String
        Dim sb As New StringBuilder()
        sb.Append(PageHead("Manualul K-BOT", forPrint:=True))
        sb.Append("<div class='cover'><h1 class='big'>K-BOT</h1><p class='sub'>Manual de utilizare</p>")
        sb.Append("<p class='dim'>Versiunea ").Append(WebUtility.HtmlEncode(version)).Append(" · ") _
          .Append(DateTime.Now.ToString("dd.MM.yyyy")).Append("</p></div>")

        sb.Append("<div class='toc'><h2>Cuprins</h2>")
        For Each part As HelpPart In parts
            sb.Append("<p class='tocpart'>").Append(WebUtility.HtmlEncode(HelpTopic.PartTitle(part))).Append("</p><ul>")
            For Each t As HelpTopic In library.InReadingOrder(part)
                sb.Append("<li style='margin-left:").Append(library.Depth(t) * 18).Append("px'><a href='#").Append(TopicAnchor(t.Id)).Append("'>") _
                  .Append(WebUtility.HtmlEncode(t.Title)).Append("</a></li>")
            Next
            sb.Append("</ul>")
        Next
        sb.Append("</div>")

        For Each part As HelpPart In parts
            sb.Append("<h1 class='part'>").Append(WebUtility.HtmlEncode(HelpTopic.PartTitle(part))).Append("</h1>")
            For Each t As HelpTopic In library.InReadingOrder(part)
                Dim level As Integer = Math.Min(4, 2 + library.Depth(t))
                sb.Append("<h").Append(level).Append(" id='").Append(TopicAnchor(t.Id)).Append("'>") _
                  .Append(WebUtility.HtmlEncode(t.Title)).Append("</h").Append(level).Append(">")
                sb.Append(RenderBody(library, t.Body, True, Nothing, Nothing))
            Next
        Next
        sb.Append("</body></html>")
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Slice 0000-32: some topics as one printable document (the export of one topic or of one
    ''' chapter), in the order given; the first one is the document's title. A link to a topic
    ''' that is not among them stays as plain text.
    ''' </summary>
    Public Shared Function Excerpt(library As HelpLibrary, topics As IReadOnlyList(Of HelpTopic), version As String) As String
        If topics Is Nothing OrElse topics.Count = 0 Then Throw New ArgumentException("No topics to export.", NameOf(topics))
        Dim sb As New StringBuilder()
        sb.Append(PageHead(WebUtility.HtmlEncode(topics(0).Title), forPrint:=True))
        sb.Append("<p class='dim'>K-BOT · Ajutor · versiunea ").Append(WebUtility.HtmlEncode(version)).Append(" · ") _
          .Append(DateTime.Now.ToString("dd.MM.yyyy")).Append("</p>")
        Dim inDocument As New HashSet(Of String)(topics.Select(Function(t) t.Id), StringComparer.OrdinalIgnoreCase)
        Dim baseDepth As Integer = library.Depth(topics(0))
        For Each t As HelpTopic In topics
            Dim level As Integer = Math.Min(4, 1 + Math.Max(0, library.Depth(t) - baseDepth))
            sb.Append("<h").Append(level).Append(" id='").Append(TopicAnchor(t.Id)).Append("'>") _
              .Append(WebUtility.HtmlEncode(t.Title)).Append("</h").Append(level).Append(">")
            sb.Append(RenderBody(library, t.Body, True, Nothing, inDocument))
        Next
        sb.Append("</body></html>")
        Return sb.ToString()
    End Function

    Private Shared Function Crumbs(library As HelpLibrary, topic As HelpTopic) As String
        Dim chain As New List(Of HelpTopic)()
        Dim cur As HelpTopic = library.Find(topic.Parent)
        While cur IsNot Nothing AndAlso chain.Count < 32
            chain.Insert(0, cur)
            cur = library.Find(cur.Parent)
        End While
        Dim sb As New StringBuilder(WebUtility.HtmlEncode(HelpTopic.PartTitle(topic.Part)))
        For Each c As HelpTopic In chain
            sb.Append(" › <a href='").Append(TopicScheme).Append(WebUtility.HtmlEncode(c.Id)).Append("'>") _
              .Append(WebUtility.HtmlEncode(c.Title)).Append("</a>")
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Markdown to HTML, with topic links and pictures rewritten. <paramref name="forManual"/>
    ''' turns topic links into anchors of the single exported document; with
    ''' <paramref name="inDocument"/>, a link to a topic outside it becomes its plain text.
    ''' <paramref name="sectionPrefix"/> (Nothing = no ids) is put before the id of every
    ''' <c>## </c> heading.
    ''' </summary>
    Private Shared Function RenderBody(library As HelpLibrary, markdown As String, forManual As Boolean,
                                       sectionPrefix As String, inDocument As ICollection(Of String)) As String
        ' Slice 0000-02: a capture tag becomes its picture, as a paragraph of its own. A malformed
        ' tag stays an HTML comment (invisible); HelpLibrary.Load has already logged why.
        markdown = HelpCapture.TagPattern.Replace(markdown,
            Function(m As Match)
                Try
                    Dim c As HelpCapture = HelpCapture.Parse(m.Groups("body").Value)
                    Return vbLf & vbLf & "![" & c.Caption.Replace("[", "(").Replace("]", ")") & "](" & c.RelativePath & ")" & vbLf & vbLf
                Catch ex As ArgumentException
                    Return m.Value
                End Try
            End Function)

        Dim doc As MarkdownDocument = Markdig.Markdown.Parse(markdown, Pipeline)
        If sectionPrefix IsNot Nothing Then AddSectionAnchors(doc, sectionPrefix)
        For Each link As LinkInline In MarkdownObjectExtensions.Descendants(Of LinkInline)(CType(doc, MarkdownObject)).ToList()
            Dim url As String = If(link.Url, String.Empty)
            If link.IsImage Then
                Dim full As String = Path.GetFullPath(Path.Combine(library.Root, url.Replace("/"c, Path.DirectorySeparatorChar)))
                If File.Exists(full) Then
                    link.Url = DataUri(full)
                Else
                    link.ReplaceBy(New HtmlInline("<span class='imgmissing'>Imagine lipsă: " & WebUtility.HtmlEncode(PlainText(link)) &
                                                  " <span class='dim'>(" & WebUtility.HtmlEncode(url) & ")</span></span>"))
                End If
            ElseIf url.StartsWith(TopicScheme, StringComparison.OrdinalIgnoreCase) Then
                Dim id As String = url.Substring(TopicScheme.Length)
                Dim hash As Integer = id.IndexOf("#"c)   ' slice 0000-18: topic:id#section
                If hash >= 0 Then id = id.Substring(0, hash)
                If library.Find(id) Is Nothing Then
                    ' A link to a topic that does not exist is visible, not a dead click.
                    link.ReplaceBy(New HtmlInline("<span class='deadlink'>[subiect lipsă: " & WebUtility.HtmlEncode(id) & "]</span>"))
                ElseIf forManual AndAlso inDocument IsNot Nothing AndAlso Not inDocument.Contains(id) Then
                    link.ReplaceBy(New HtmlInline(WebUtility.HtmlEncode(PlainText(link))))
                ElseIf forManual Then
                    link.Url = "#" & TopicAnchor(id)
                End If
            End If
        Next
        Using sw As New StringWriter()
            Dim renderer As New HtmlRenderer(sw)
            Pipeline.Setup(renderer)
            renderer.Render(doc)
            sw.Flush()
            Return sw.ToString()
        End Using
    End Function

    ' The words of a link (or of a picture's caption), without their formatting.
    Private Shared Function PlainText(link As LinkInline) As String
        Return String.Concat(MarkdownObjectExtensions.Descendants(Of LiteralInline)(CType(link, MarkdownObject)).Select(Function(l) l.Content.ToString()))
    End Function

    ''' <summary>
    ''' Slice 0000-18: every <c>## </c> heading gets the id its search section has
    ''' (<see cref="HelpSearch.AnchorFor"/>, same text, same order), so a search hit opens the
    ''' page at its section. Slice 0000-32: after <paramref name="prefix"/>, the topic's own
    ''' anchor, because the page now holds every topic.
    ''' </summary>
    Private Shared Sub AddSectionAnchors(doc As MarkdownDocument, prefix As String)
        Dim used As New HashSet(Of String)(StringComparer.Ordinal)
        For Each h As HeadingBlock In MarkdownObjectExtensions.Descendants(Of HeadingBlock)(CType(doc, MarkdownObject)).ToList()
            If h.Level <> 2 OrElse h.Inline Is Nothing Then Continue For
            Dim text As New StringBuilder()
            For Each piece As Inline In MarkdownObjectExtensions.Descendants(Of Inline)(CType(h.Inline, MarkdownObject))
                If TypeOf piece Is LiteralInline Then
                    text.Append(DirectCast(piece, LiteralInline).Content.ToString())
                ElseIf TypeOf piece Is CodeInline Then
                    text.Append(DirectCast(piece, CodeInline).Content)
                End If
            Next
            h.GetAttributes().Id = prefix & HelpSearch.AnchorFor(text.ToString(), used)
        Next
    End Sub

    Private Shared Function DataUri(file As String) As String
        Dim mime As String
        Select Case Path.GetExtension(file).ToLowerInvariant()
            Case ".png" : mime = "image/png"
            Case ".jpg", ".jpeg" : mime = "image/jpeg"
            Case ".gif" : mime = "image/gif"
            Case Else : Throw New ArgumentException("Help picture type not supported: " & file, NameOf(file))
        End Select
        Return "data:" & mime & ";base64," & Convert.ToBase64String(System.IO.File.ReadAllBytes(file))
    End Function

    ''' <summary>Slice 0000-23: the help window's text size (settings file, the two magnifier buttons), as a factor.</summary>
    Friend Shared Function HelpTextFactor() As Double
        Return KBot.Common.AppSettings.Current.HelpTextPercent / 100.0
    End Function

    ' The colours of a page: the active theme on screen, fixed black-on-white on paper.
    Private NotInheritable Class PageColors
        Public Back As String
        Public Fore As String
        Public Faint As String
        Public Border As String
        Public Accent As String
        Public Warn As String
        Public Shade As String
        Public Bad As String

        Public Shared Function OfTheme() As PageColors
            Dim p As ThemePalette = ThemeManager.Current.Palette
            Return New PageColors() With {.Back = p.SurfaceAlt, .Fore = p.Text, .Faint = p.TextDim, .Border = p.Border,
                                          .Accent = p.Accent, .Warn = p.Warning, .Shade = p.Surface, .Bad = p.Error}
        End Function

        Public Shared Function OfPaper() As PageColors
            Return New PageColors() With {.Back = "#FFFFFF", .Fore = "#000000", .Faint = "#555555", .Border = "#BBBBBB",
                                          .Accent = "#1F4E79", .Warn = "#B06000", .Shade = "#F2F2F2",
                                          .Bad = ThemeManager.Current.Palette.Error}
        End Function
    End Class

    ' What only paper needs: a heading stays with its text, a picture is not cut in two.
    Private Const PaperOnly As String = "h1,h2,h3,h4{page-break-after:avoid}img{page-break-inside:avoid}"

    ' The book on paper: no start page, no links that only work on screen, nothing marked as left
    ' out; every part but the first on a new page. The text size is fixed (the «A-» / «A+» size
    ' is set on the body itself, hence !important in BookPaperBody).
    Private Const BookPaperOnly As String =
        "#home,.kids,.tour,.noprint{display:none}.pb{page-break-before:always}.topic,.partsec{border-top:0;padding-top:0}a{text-decoration:none}"

    Private Shared Function Rules(c As PageColors, bodySize As String, bodyMargin As String) As String
        Dim css As New StringBuilder()
        css.Append("body{font-family:'Segoe UI',Arial,sans-serif;font-size:").Append(bodySize) _
           .Append(";background:").Append(c.Back).Append(";color:").Append(c.Fore).Append(";margin:").Append(bodyMargin).Append(";line-height:1.45}")
        css.Append("a{color:").Append(c.Accent).Append("}")
        css.Append("h1{font-size:1.6em;margin:.2em 0 .6em;font-weight:600}")
        css.Append("h2{font-size:1.3em;margin:1.2em 0 .4em;font-weight:600}")
        css.Append("h3{font-size:1.1em;margin:1em 0 .3em;font-weight:600}")
        css.Append("h4{font-size:1em;margin:1em 0 .3em;font-weight:600}")
        css.Append(".crumbs,.dim{color:").Append(c.Faint).Append(";font-size:.9em}")
        css.Append("table{border-collapse:collapse;margin:.6em 0}")
        css.Append("td,th{border:1px solid ").Append(c.Border).Append(";padding:4px 8px;vertical-align:top;text-align:left}")
        css.Append("th{background:").Append(c.Shade).Append("}")
        css.Append("code{background:").Append(c.Shade).Append(";padding:0 3px;font-family:Consolas,monospace}")
        css.Append("blockquote{margin:.8em 0;padding:.4em .9em;border-left:4px solid ").Append(c.Warn).Append(";background:").Append(c.Shade).Append("}")
        css.Append("img{max-width:100%;border:1px solid ").Append(c.Border).Append(";margin:.4em 0}")
        css.Append(".imgmissing{display:inline-block;border:1px dashed ").Append(c.Warn).Append(";color:").Append(c.Warn) _
           .Append(";padding:6px 10px;margin:.4em 0;font-size:.9em}")
        css.Append(".deadlink{color:").Append(c.Bad).Append("}")
        css.Append(".kids{margin-top:1.4em;border-top:1px solid ").Append(c.Border).Append("}")
        css.Append(".tour{margin:.2em 0 .8em;font-weight:600}.tour a{text-decoration:none}")
        css.Append(".cover{text-align:center;margin-top:30%}.cover .big{font-size:3em}.cover .sub{font-size:1.5em}")
        css.Append(".tocpart{font-weight:600;margin:.8em 0 .2em}.toc ul{list-style:none;padding-left:0;margin:0}")
        ' Slice 0000-32: the sections of the book. Padding, not margins: the sections must touch.
        css.Append(".sec{padding:1em 0 1.4em}.topic,.partsec{border-top:1px solid ").Append(c.Border).Append("}")
        css.Append(".partsec h1{font-size:2em;margin:.2em 0}")
        Return css.ToString()
    End Function

    ''' <summary>
    ''' The page head with the CSS built from the active theme, so the help window reads like the
    ''' rest of K-BOT. The printed manual (<paramref name="forPrint"/>) uses fixed black-on-white
    ''' instead: paper has no theme. The window's document (<paramref name="book"/>) carries both:
    ''' the theme on screen, the paper look under <c>@media print</c>.
    ''' </summary>
    Private Shared Function PageHead(title As String, Optional forPrint As Boolean = False, Optional book As Boolean = False) As String
        Dim css As New StringBuilder()
        If forPrint Then
            css.Append(Rules(PageColors.OfPaper(), "10.5pt", "18px 26px")).Append(PaperOnly)
            css.Append(".cover,.toc{page-break-after:always}h1.part{page-break-before:always}")
        Else
            ' Slice 0000-23: on screen, also the help window's own «A- / A+» size.
            Dim sizePt As Double = 10.5 * AppScaling.TextScale * HelpTextFactor()
            Dim size As String = sizePt.ToString("0.0", Globalization.CultureInfo.InvariantCulture) & "pt"
            ' The book has no margin above: its first section starts at the very top of the page.
            css.Append(Rules(PageColors.OfTheme(), size, If(book, "0 26px 18px", "18px 26px")))
            If book Then
                css.Append("@media print{").Append(Rules(PageColors.OfPaper(), "10.5pt !important", "0")) _
                   .Append(PaperOnly).Append(BookPaperOnly).Append("}")
            End If
        End If

        Return "<!DOCTYPE html><html><head><meta http-equiv='X-UA-Compatible' content='IE=edge'><meta charset='utf-8'><title>" &
               title & "</title><style>" & css.ToString() & "</style></head><body>"
    End Function

End Class
