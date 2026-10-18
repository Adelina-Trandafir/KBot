Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Markdig

''' <summary>Formats already available report text; never reads Access or MariaDB.</summary>
Friend NotInheritable Class VrcReportHtml

    Private Shared ReadOnly Pipeline As MarkdownPipeline =
        New MarkdownPipelineBuilder().UsePipeTables().UseEmphasisExtras().DisableHtml().Build()

    Private Sub New()
    End Sub

    Friend Shared Function Create(k_report As String, k_status As String) As String
        Dim k_body = Markdown.ToHtml(PrepareMarkdown(k_report), Pipeline)
        ' Report values are text. Images in their Markdown must never request external content.
        k_body = Regex.Replace(k_body, "<img\b[^>]*>", String.Empty, RegexOptions.IgnoreCase)
        ' Only «open folder» links (file:///) stay; every other link keeps its text and loses the address.
        k_body = Regex.Replace(k_body, "<a\b([^>]*)>(.*?)</a>",
                               Function(k_m As Match) As String
                                   Dim k_href = Regex.Match(k_m.Groups(1).Value, "href=""(file:///[^""]*)""", RegexOptions.IgnoreCase)
                                   Return If(k_href.Success, $"<a href=""{k_href.Groups(1).Value}"">{k_m.Groups(2).Value}</a>", k_m.Groups(2).Value)
                               End Function, RegexOptions.IgnoreCase Or RegexOptions.Singleline)
        k_body = k_body.Replace("<h2>BLOCAJE", "<h2 class=""blocking"">BLOCAJE").
                        Replace("<h2>AVERTIZĂRI", "<h2 class=""warning"">AVERTIZĂRI").
                        Replace("<h2>SERVER — gata", "<h2 class=""success"">SERVER — gata")

        Dim k_palette = KBot.Theming.ThemeManager.Current.Palette
        Dim k_css As String
        Using k_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VRC.Migrator.VrcReportHtml.css")
            If k_stream Is Nothing Then Throw New InvalidOperationException("Stilul raportului HTML lipsește din executabil.")
            Using k_reader As New StreamReader(k_stream, Encoding.UTF8)
                k_css = k_reader.ReadToEnd()
            End Using
        End Using
        k_css = k_css.Replace("$SURFACE$", ColorTranslator.ToHtml(k_palette.SurfaceColor)).
                      Replace("$CARD$", ColorTranslator.ToHtml(k_palette.SurfaceAltColor)).
                      Replace("$TEXT$", ColorTranslator.ToHtml(k_palette.TextColor)).
                      Replace("$DIM$", ColorTranslator.ToHtml(k_palette.TextDimColor)).
                      Replace("$BORDER$", ColorTranslator.ToHtml(k_palette.BorderColor)).
                      Replace("$ERROR$", ColorTranslator.ToHtml(k_palette.ErrorColor)).
                      Replace("$WARNING$", ColorTranslator.ToHtml(k_palette.WarningColor)).
                      Replace("$SUCCESS$", ColorTranslator.ToHtml(k_palette.SuccessColor))

        Dim k_page As New StringBuilder()
        k_page.AppendLine("<!DOCTYPE html><html lang=""ro""><head><meta charset=""utf-8"">")
        k_page.AppendLine("<meta http-equiv=""X-UA-Compatible"" content=""IE=edge""><title>VRC.Migrator — raport</title>")
        k_page.Append("<style>").Append(k_css).AppendLine("</style></head><body>")
        k_page.AppendLine("<div class=""report-header""><h1>VRC.Migrator — rezultate</h1>")
        k_page.Append("<p class='status ")
        Dim k_statusClass = If(k_status.Contains("[BLOCAT]", StringComparison.Ordinal), "blocking",
                              If(k_status.Contains("[TESTAT]", StringComparison.Ordinal) OrElse
                                 k_status.Contains("[COMMIT_CONFIRMAT]", StringComparison.Ordinal), "success", String.Empty))
        k_page.Append(k_statusClass).Append("'>").Append(WebUtility.HtmlEncode(k_status)).AppendLine("</p>")
        k_page.Append("<p class=""meta"">Rezultate deja citite · afișat la ").
               Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).AppendLine("</p></div>")
        k_page.Append("<div class=""report-content"">").Append(k_body).AppendLine("</div></body></html>")
        Return k_page.ToString()
    End Function

    Private Shared Function PrepareMarkdown(k_report As String) As String
        Dim k_text As New StringBuilder()
        For Each k_line In k_report.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(vbLf(0))
            Dim k_trimmed = k_line.Trim()
            If k_trimmed.StartsWith("PLAN —", StringComparison.Ordinal) Then
                k_text.AppendLine("## Verificarea planului").AppendLine().AppendLine(k_line).AppendLine()
            ElseIf k_trimmed.StartsWith("BLOCAJE (", StringComparison.Ordinal) OrElse
                   k_trimmed.StartsWith("AVERTIZĂRI (", StringComparison.Ordinal) OrElse
                   k_trimmed.StartsWith("SERVER —", StringComparison.Ordinal) Then
                k_text.AppendLine().Append("## ").AppendLine(k_trimmed).AppendLine()
            Else
                k_text.AppendLine(k_line)
            End If
        Next
        Return k_text.ToString()
    End Function

End Class
