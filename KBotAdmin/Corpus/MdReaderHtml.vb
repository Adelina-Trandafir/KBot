Option Strict On
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports Markdig
Imports Markdig.Extensions.AutoIdentifiers
Imports Markdig.Renderers
Imports Markdig.Renderers.Html
Imports Markdig.Syntax
Imports Markdig.Syntax.Inlines

''' <summary>SLICE-ME01-26. Turns repository Markdown into a self-contained page for the desktop reader.</summary>
Friend NotInheritable Class MdReaderHtml

    Friend Const ContentId As String = "md-reader-content"
    ' SLICE-ME01-28. The reader opens local destinations without navigating the WebBrowser to file:.
    Friend Const LocalLinkAttribute As String = "data-md-reader-target"

    Private Shared ReadOnly Pipeline As MarkdownPipeline =
        New MarkdownPipelineBuilder().UseYamlFrontMatter().UsePipeTables().UseTaskLists().
            UseEmphasisExtras().UseAutoIdentifiers(AutoIdentifierOptions.GitHub).DisableHtml().Build()
    Private Shared ReadOnly CaptureTag As New Regex(
        "^<!--\s*capture:\s*(?<id>[a-z0-9][a-z0-9-]*)\s*\|\s*caption:\s*(?<caption>[^|\r\n]+?)(?:\s*\||\s*-->)",
        RegexOptions.CultureInvariant)

    Private Sub New()
    End Sub

    Friend Shared Function Render(k_root As String, k_file As Uri, k_scheme As ThemeScheme) As String
        Dim k_helpRoot As String = Path.Combine(k_root, "src", "KBot.App", "HelpContent")
        Dim k_source As String = PrepareSource(File.ReadAllText(k_file.LocalPath), k_file, k_helpRoot)
        Dim k_document As MarkdownDocument = Markdown.Parse(k_source, Pipeline)
        Dim k_links = MarkdownObjectExtensions.Descendants(Of LinkInline)(CType(k_document, MarkdownObject)).ToList()
        For Each k_link As LinkInline In k_links
            Dim k_url As String = If(k_link.GetDynamicUrl?.Invoke(), k_link.Url)
            k_link.GetDynamicUrl = Nothing
            Dim k_target As Uri = ResolveLink(k_root, k_file, k_helpRoot, k_url)
            Dim k_local As String = LocalPathInProject(k_root, k_target)
            If k_link.IsImage Then
                Dim k_image As String = ImageData(k_local)
                If k_image IsNot Nothing Then
                    k_link.Url = k_image
                Else
                    k_link.ReplaceBy(New LiteralInline("[Imagine indisponibilă: " & LinkLabel(k_link, k_url) & "]"))
                End If
            ElseIf IsMarkdownPath(k_local) Then
                k_link.GetAttributes().AddProperty(LocalLinkAttribute, k_target.AbsoluteUri)
                k_link.Url = "#" & ContentId
            ElseIf IsWebLink(k_target) Then
                k_link.Url = k_target.AbsoluteUri
            Else
                k_link.ReplaceBy(New LiteralInline(LinkLabel(k_link, k_url)))
            End If
        Next

        Using k_writer As New StringWriter()
            Dim k_renderer As New HtmlRenderer(k_writer)
            Pipeline.Setup(k_renderer)
            k_renderer.Render(k_document)
            Return WrapPage(Path.GetFileName(k_file.LocalPath), k_writer.ToString(), k_scheme.Palette)
        End Using
    End Function

    Private Shared Function ResolveLink(k_root As String, k_file As Uri, k_helpRoot As String, k_url As String) As Uri
        If String.IsNullOrWhiteSpace(k_url) Then Return Nothing
        If k_url.StartsWith("topic:", StringComparison.OrdinalIgnoreCase) Then Return ResolveHelpTopic(k_helpRoot, k_url)

        Dim k_base As Uri = k_file
        If k_url.StartsWith("/", StringComparison.Ordinal) AndAlso Not k_url.StartsWith("//", StringComparison.Ordinal) Then
            k_base = FileUri(Path.TrimEndingDirectorySeparator(k_root) & Path.DirectorySeparatorChar)
            k_url = k_url.Substring(1)
        End If
        Dim k_target As Uri = Nothing
        Uri.TryCreate(k_base, k_url, k_target)
        Return k_target
    End Function

    Private Shared Function ResolveHelpTopic(k_helpRoot As String, k_url As String) As Uri
        Dim k_topic As String() = k_url.Substring("topic:".Length).Split("#"c, 2)
        If Not Regex.IsMatch(k_topic(0), "^[a-z0-9_-]+(?:\.[a-z0-9_-]+)*$", RegexOptions.IgnoreCase Or RegexOptions.CultureInvariant) Then Return Nothing
        Dim k_stem As String = Path.Combine(k_helpRoot, k_topic(0).Replace("."c, Path.DirectorySeparatorChar))
        Dim k_path As String = k_stem & ".md"
        If Not File.Exists(k_path) Then k_path = Path.Combine(k_stem, "index.md")
        If Not File.Exists(k_path) Then Return Nothing
        Dim k_target As Uri = FileUri(k_path)
        If k_topic.Length = 1 Then Return k_target
        Return New Uri(k_target, "#" & k_topic(1))
    End Function

    Friend Shared Function FileUri(k_path As String) As Uri
        ' Use an explicit file URI as the base so that # in a relative link is an anchor.
        Return New Uri(New Uri(k_path).AbsoluteUri, UriKind.Absolute)
    End Function

    Private Shared Function PrepareSource(k_source As String, k_file As Uri, k_helpRoot As String) As String
        If Not k_source.Contains("<!--", StringComparison.Ordinal) Then Return k_source
        ' Recognize real Markdown comments first so HTML examples inside code fences stay intact.
        Dim k_parsed As MarkdownDocument = Markdown.Parse(k_source)
        Dim k_comments = MarkdownObjectExtensions.Descendants(Of HtmlBlock)(CType(k_parsed, MarkdownObject)).
            Where(Function(k_block) k_block.Type = HtmlBlockType.Comment).OrderByDescending(Function(k_block) k_block.Span.Start)
        Dim k_output As New StringBuilder(k_source)
        Dim k_isHelp As Boolean = LocalPathInProject(k_helpRoot, k_file) IsNot Nothing
        For Each k_block As HtmlBlock In k_comments
            Dim k_span As SourceSpan = k_block.Span
            If k_span.Start < 0 OrElse k_span.End >= k_source.Length OrElse k_span.Length <= 0 Then Continue For
            Dim k_comment As String = k_source.Substring(k_span.Start, k_span.Length)
            If Not k_comment.TrimEnd().EndsWith("-->", StringComparison.Ordinal) Then Continue For
            Dim k_replacement As String = String.Empty
            If k_isHelp Then
                Dim k_capture As Match = CaptureTag.Match(k_comment)
                If k_capture.Success Then
                    Dim k_picture As String = Path.Combine(k_helpRoot, "img", k_capture.Groups("id").Value & ".png")
                    Dim k_relative As String = Path.GetRelativePath(Path.GetDirectoryName(k_file.LocalPath), k_picture).Replace("\"c, "/"c)
                    Dim k_caption As String = k_capture.Groups("caption").Value.Trim().Replace("\", "\\").Replace("[", "\[").Replace("]", "\]")
                    k_replacement = vbLf & "![" & k_caption & "](" & k_relative & ")" & vbLf
                End If
            End If
            k_output.Remove(k_span.Start, k_span.Length).Insert(k_span.Start, k_replacement)
        Next
        Return k_output.ToString()
    End Function

    Friend Shared Function LocalPathInProject(k_root As String, k_target As Uri) As String
        If k_target Is Nothing OrElse Not k_target.IsFile OrElse k_target.IsUnc Then Return Nothing
        Try
            Dim k_full As String = Path.GetFullPath(k_target.LocalPath)
            Dim k_relative As String = Path.GetRelativePath(k_root, k_full)
            If k_relative = ".." OrElse k_relative.StartsWith(".." & Path.DirectorySeparatorChar, StringComparison.Ordinal) OrElse
               Path.IsPathRooted(k_relative) Then Return Nothing
            Return k_full
        Catch ex As ArgumentException
            Return Nothing
        Catch ex As NotSupportedException
            Return Nothing
        Catch ex As IOException
            Return Nothing
        End Try
    End Function

    Friend Shared Function IsMarkdownPath(k_path As String) As Boolean
        Return k_path IsNot Nothing AndAlso String.Equals(Path.GetExtension(k_path), ".md", StringComparison.OrdinalIgnoreCase)
    End Function

    Friend Shared Function IsWebLink(k_target As Uri) As Boolean
        Return k_target IsNot Nothing AndAlso (k_target.Scheme = Uri.UriSchemeHttp OrElse k_target.Scheme = Uri.UriSchemeHttps)
    End Function

    Private Shared Function LinkLabel(k_link As LinkInline, k_url As String) As String
        Dim k_label As String = String.Concat(MarkdownObjectExtensions.Descendants(Of LiteralInline)(CType(k_link, MarkdownObject)).
                                                   Select(Function(k_piece) k_piece.Content.ToString()))
        Return If(k_label.Length > 0, k_label, If(k_url, String.Empty))
    End Function

    Private Shared Function ImageData(k_path As String) As String
        If k_path Is Nothing Then Return Nothing
        Dim k_mime As String
        Select Case Path.GetExtension(k_path).ToLowerInvariant()
            Case ".png" : k_mime = "image/png"
            Case ".jpg", ".jpeg" : k_mime = "image/jpeg"
            Case ".gif" : k_mime = "image/gif"
            Case ".bmp" : k_mime = "image/bmp"
            Case Else : Return Nothing
        End Select
        Try
            Return "data:" & k_mime & ";base64," & Convert.ToBase64String(File.ReadAllBytes(k_path))
        Catch ex As IOException
            Return Nothing
        Catch ex As UnauthorizedAccessException
            Return Nothing
        End Try
    End Function

    Private Shared Function WrapPage(k_title As String, k_body As String, k_palette As ThemePalette) As String
        Dim k_surface As String = CssColor(k_palette.SurfaceColor)
        Dim k_paper As String = CssColor(k_palette.SurfaceAltColor)
        Dim k_text As String = CssColor(k_palette.TextColor)
        Dim k_dim As String = CssColor(k_palette.TextDimColor)
        Dim k_border As String = CssColor(k_palette.BorderColor)
        Dim k_accent As String = CssColor(k_palette.AccentColor)
        Dim k_css As String =
            "html{overflow-y:scroll}*{box-sizing:border-box}" &
            $"body{{margin:0;padding:18px;background:{k_surface};color:{k_text};font:16px/1.55 'Segoe UI',Arial,sans-serif}}" &
            $"#document{{max-width:1080px;margin:0 auto;padding:24px 5%;background:{k_paper};border:1px solid {k_border};word-wrap:break-word}}" &
            "h1,h2,h3,h4,h5,h6{line-height:1.2;margin:1.35em 0 .55em}h1:first-child{margin-top:0}" &
            $"h1,h2{{padding-bottom:.25em;border-bottom:1px solid {k_border}}}" &
            "p,ul,ol,pre,table,blockquote{margin:.65em 0 1em}li{margin:.18em 0}" &
            $"a{{color:{k_accent}}}a:hover{{text-decoration:underline}}hr{{border:0;border-top:1px solid {k_border};margin:1.4em 0}}" &
            $"code,pre{{font:14px/1.5 Consolas,monospace;background:{k_surface}}}code{{padding:.08em .3em}}" &
            $"pre{{padding:.8em 1em;border-left:3px solid {k_accent};white-space:pre-wrap;word-wrap:break-word}}pre code{{padding:0}}" &
            $"blockquote{{padding:.15em .85em;border-left:3px solid {k_accent};background:{k_surface};color:{k_dim}}}" &
            $"table{{border-collapse:collapse;width:100%;table-layout:fixed;font-size:14px}}th,td{{border:1px solid {k_border};padding:.45em .6em;vertical-align:top;word-wrap:break-word}}" &
            $"th{{background:{k_accent};color:{CssColor(k_palette.AccentTextColor)};text-align:left}}tr:nth-child(even) td{{background:{k_surface}}}" &
            $"img{{max-width:100%;height:auto;border:1px solid {k_border}}}"
        Return "<!DOCTYPE html><html lang='ro'><head><meta charset='utf-8'><meta http-equiv='X-UA-Compatible' content='IE=edge'>" &
               "<title>" & WebUtility.HtmlEncode(k_title) & "</title><style>" & k_css & "</style></head><body>" &
               "<div id='" & ContentId & "'><div id='document'>" & k_body & "</div></div></body></html>"
    End Function

    Private Shared Function CssColor(k_color As Color) As String
        Return $"#{k_color.R:X2}{k_color.G:X2}{k_color.B:X2}"
    End Function

End Class
