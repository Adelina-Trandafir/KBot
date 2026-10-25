Imports System.Globalization
Imports System.Net
Imports System.Text
Imports KBot.Migrator

''' <summary>One unit line of the report (the unit grid as it was when the report was made).</summary>
Friend NotInheritable Class AvcReportUnit
    Public Property Ticked As Boolean
    Public Property Id As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property Source As String = String.Empty
    Public Property Denomination As String = String.Empty
    Public Property Chapter As String = String.Empty
    Public Property Decimals As String = String.Empty
    Public Property Parent As String = String.Empty
    Public Property AnDate As String = String.Empty
    Public Property State As String = String.Empty
End Class

''' <summary>One step line of the report (the step grid as it was when the report was made).</summary>
Friend NotInheritable Class AvcReportStep
    Public Property Selected As Boolean
    Public Property Stage As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property AccessTable As String = String.Empty
    Public Property AccessRows As String = String.Empty
    Public Property ToWrite As String = String.Empty
    Public Property InDatabase As String = String.Empty
End Class

''' <summary>Everything the report shows, already read: the report never touches Access or MariaDB.</summary>
Friend NotInheritable Class AvcReportData
    Public Property Dc As String = String.Empty
    Public Property Server As String = String.Empty
    Public Property CodFiscal As String = String.Empty
    ''' <summary>True once the unit files were read (the findings of the reading are in <see cref="Findings"/>).</summary>
    Public Property WasRead As Boolean
    ''' <summary>True once the target was verified (the findings of the verification are in <see cref="Findings"/>).</summary>
    Public Property WasVerified As Boolean
    Public Property CanRun As Boolean
    Public Property FindingsAt As DateTime?
    Public ReadOnly Property Units As New List(Of AvcReportUnit)()
    Public ReadOnly Property Steps As New List(Of AvcReportStep)()
    Public ReadOnly Property Findings As New List(Of Finding)()

    ''' <summary>True after a migration was attempted; <see cref="Outcome"/> says how it ended.</summary>
    Public Property WasRun As Boolean
    Public Property Outcome As String = String.Empty
    ''' <summary>blocking / warning / success.</summary>
    Public Property OutcomeClass As String = "success"
    Public Property RunAt As DateTime?
    Public Property JournalFolder As String = String.Empty
    Public ReadOnly Property Written As New List(Of AvcWritten)()
End Class

''' <summary>
''' The HTML report of AVC.Migrator, for the operator: the verification (blocks, warnings, information, each in its own colour) and,
''' after a migration, what was written. One self-contained page; the colours come from the theme like the VRC report.
''' </summary>
Friend NotInheritable Class AvcReportHtml

    Private Sub New()
    End Sub

    ''' <summary>The style sheet; <c>$NAME$</c> tokens are the theme colours. Old-IE safe: no grid, no variables, no details element.</summary>
    Private Shared ReadOnly Css As String = String.Join(vbLf, {
        "body { margin: 0; padding: 20px; background: $SURFACE$; color: $TEXT$; font-family: ""Segoe UI"", Arial, sans-serif; font-size: 13.25pt; line-height: 1.5; }",
        ".card { padding: 16px 20px; margin-bottom: 16px; background: $CARD$; border: 1px solid $BORDER$; border-radius: 6px; }",
        "h1 { margin: 0 0 6px; font-size: 21.5pt; }",
        "h2 { margin: 0 0 12px; font-size: 16.25pt; padding-bottom: 6px; border-bottom: 2px solid $BORDER$; }",
        "h2.blocking { color: $ERROR$; border-bottom-color: $ERROR$; }",
        "h2.warning { color: $WARNING$; border-bottom-color: $WARNING$; }",
        "h2.info { color: $ACCENT$; border-bottom-color: $ACCENT$; }",
        "h2.success { color: $SUCCESS$; border-bottom-color: $SUCCESS$; }",
        ".meta { color: $DIM$; font-size: 11.75pt; margin: 2px 0; }",
        ".status { display: inline-block; margin: 10px 0 4px; padding: 6px 14px; font-size: 14.75pt; font-weight: 600; border: 2px solid $BORDER$; border-radius: 5px; }",
        ".status.blocking { color: $ERROR$; border-color: $ERROR$; }",
        ".status.warning { color: $WARNING$; border-color: $WARNING$; }",
        ".status.success { color: $SUCCESS$; border-color: $SUCCESS$; }",
        ".counts { margin-top: 12px; }",
        ".count { display: inline-block; min-width: 130px; margin: 0 10px 6px 0; padding: 8px 14px; border: 1px solid $BORDER$; border-left-width: 6px; border-radius: 4px; background: $SURFACE$; }",
        ".count b { display: block; font-size: 20pt; line-height: 1.1; }",
        ".count.blocking { border-left-color: $ERROR$; } .count.blocking b { color: $ERROR$; }",
        ".count.warning { border-left-color: $WARNING$; } .count.warning b { color: $WARNING$; }",
        ".count.info { border-left-color: $ACCENT$; } .count.info b { color: $ACCENT$; }",
        ".finding { margin: 0 0 12px; padding: 10px 14px; background: $SURFACE$; border: 1px solid $BORDER$; border-left-width: 6px; border-radius: 4px; }",
        ".finding.blocking { border-left-color: $ERROR$; }",
        ".finding.warning { border-left-color: $WARNING$; }",
        ".finding.info { border-left-color: $ACCENT$; }",
        ".finding .msg { font-weight: 600; }",
        ".finding .where { color: $DIM$; font-size: 11pt; margin-top: 2px; }",
        ".finding pre { margin: 8px 0 0; padding: 8px 10px; max-height: 340px; overflow: auto; background: $CARD$; border: 1px solid $BORDER$; font-family: Consolas, monospace; font-size: 11.75pt; white-space: pre-wrap; word-wrap: break-word; }",
        ".none { color: $SUCCESS$; font-weight: 600; }",
        ".hint { color: $DIM$; }",
        "table { border-collapse: collapse; width: 100%; font-size: 12.5pt; }",
        "th, td { border: 1px solid $BORDER$; padding: 6px 9px; vertical-align: top; text-align: left; }",
        "th { background: $SURFACE$; font-weight: 600; }",
        "td.num { text-align: right; white-space: nowrap; }",
        "tr.off td { color: $DIM$; }",
        "tr.total td { font-weight: 600; background: $SURFACE$; }",
        "a { color: $ACCENT$; }",
        "@media print {",
        "  body { padding: 0; color: #000; background: #fff; }",
        "  .card { border: 0; padding: 0; background: #fff; }",
        "  .finding pre { max-height: none; overflow: visible; }",
        "  .finding, tr { page-break-inside: avoid; }",
        "}"})

    Public Shared Function Create(k_data As AvcReportData) As String
        Dim k_palette = KBot.Theming.ThemeManager.Current.Palette
        Dim k_css = Css.Replace("$SURFACE$", ColorTranslator.ToHtml(k_palette.SurfaceColor)).
                        Replace("$CARD$", ColorTranslator.ToHtml(k_palette.SurfaceAltColor)).
                        Replace("$TEXT$", ColorTranslator.ToHtml(k_palette.TextColor)).
                        Replace("$DIM$", ColorTranslator.ToHtml(k_palette.TextDimColor)).
                        Replace("$BORDER$", ColorTranslator.ToHtml(k_palette.BorderColor)).
                        Replace("$ERROR$", ColorTranslator.ToHtml(k_palette.ErrorColor)).
                        Replace("$WARNING$", ColorTranslator.ToHtml(k_palette.WarningColor)).
                        Replace("$SUCCESS$", ColorTranslator.ToHtml(k_palette.SuccessColor)).
                        Replace("$ACCENT$", ColorTranslator.ToHtml(k_palette.AccentColor))

        Dim k_blocking = k_data.Findings.Where(Function(k_f) k_f.Severity = FindingClass.Blocant).ToList()
        Dim k_warnings = k_data.Findings.Where(Function(k_f) k_f.Severity = FindingClass.Atentie).ToList()
        Dim k_info = k_data.Findings.Where(Function(k_f) k_f.Severity = FindingClass.Informativ).ToList()

        Dim k_page As New StringBuilder()
        k_page.AppendLine("<!DOCTYPE html><html lang=""ro""><head><meta charset=""utf-8"">")
        k_page.AppendLine("<meta http-equiv=""X-UA-Compatible"" content=""IE=edge""><title>AVC.Migrator — raport</title>")
        k_page.Append("<style>").Append(k_css).AppendLine("</style></head><body>")

        AppendHeader(k_page, k_data, k_blocking.Count, k_warnings.Count, k_info.Count)
        If k_data.WasRun Then AppendRun(k_page, k_data)
        If k_data.WasRead Then
            AppendFindings(k_page, k_data, k_blocking, k_warnings, k_info)
            AppendSteps(k_page, k_data)
            AppendUnits(k_page, k_data)
        End If
        k_page.AppendLine("</body></html>")
        Return k_page.ToString()
    End Function

    ' ---- sections ------------------------------------------------------------------------------------------------

    Private Shared Sub AppendHeader(k_page As StringBuilder, k_data As AvcReportData, k_blockingCount As Integer, k_warningCount As Integer, k_infoCount As Integer)
        Dim k_class As String
        Dim k_status As String
        If k_data.WasRun Then
            k_status = k_data.Outcome
            k_class = k_data.OutcomeClass
        ElseIf k_data.WasVerified Then
            k_class = If(k_data.CanRun, If(k_warningCount > 0, "warning", "success"), "blocking")
            k_status = If(k_data.CanRun, "Verificat — migrarea poate porni", $"Blocat — {k_blockingCount} blocaje opresc migrarea")
        ElseIf k_data.WasRead Then
            k_class = If(k_blockingCount > 0, "blocking", "warning")
            k_status = If(k_blockingCount > 0, "Citire cu erori — vezi blocajele", "Citit — încă neverificat pe server")
        Else
            k_class = "warning"
            k_status = "Nimic citit încă"
        End If

        k_page.AppendLine("<div class=""card"">")
        k_page.AppendLine("<h1>AVC.Migrator — raport</h1>")
        k_page.Append("<div class=""status ").Append(k_class).Append(""">").Append(Encode(k_status)).AppendLine("</div>")
        k_page.Append("<p class=""meta"">Baza-țintă (DC): <b>").Append(Encode(Blank(k_data.Dc))).Append("</b> · Server: ").Append(Encode(Blank(k_data.Server))).
               Append(" · Cod fiscal: ").Append(Encode(Blank(k_data.CodFiscal))).AppendLine("</p>")
        k_page.Append("<p class=""meta"">Raport generat la ").Append(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture))
        If k_data.FindingsAt.HasValue Then
            k_page.Append(" · constatările sunt de la ").Append(k_data.FindingsAt.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture))
        End If
        k_page.AppendLine(". Datele sunt cele deja citite; raportul nu interoghează nimic.</p>")
        If k_data.WasRead Then
            k_page.AppendLine("<div class=""counts"">")
            k_page.Append("<div class=""count blocking""><b>").Append(k_blockingCount).AppendLine("</b>blocaje</div>")
            k_page.Append("<div class=""count warning""><b>").Append(k_warningCount).AppendLine("</b>atenționări</div>")
            k_page.Append("<div class=""count info""><b>").Append(k_infoCount).AppendLine("</b>informații</div>")
            k_page.AppendLine("</div>")
        End If
        k_page.AppendLine("</div>")
    End Sub

    Private Shared Sub AppendRun(k_page As StringBuilder, k_data As AvcReportData)
        k_page.AppendLine("<div class=""card"">")
        k_page.Append("<h2 class=""").Append(k_data.OutcomeClass).AppendLine(""">Rezultatul migrării</h2>")
        k_page.Append("<p>").Append(Encode(k_data.Outcome))
        If k_data.RunAt.HasValue Then k_page.Append(" <span class=""hint"">(").Append(k_data.RunAt.Value.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)).Append(")</span>")
        k_page.AppendLine("</p>")
        If k_data.Written.Count > 0 Then
            k_page.AppendLine("<table><thead><tr><th>Tabel</th><th>Rânduri scrise</th></tr></thead><tbody>")
            Dim k_total As Long = 0
            For Each k_written In k_data.Written
                k_page.Append("<tr><td>").Append(Encode(k_written.Table)).Append("</td><td class=""num"">").Append(k_written.Rows.ToString("N0", CultureInfo.InvariantCulture)).AppendLine("</td></tr>")
                k_total += k_written.Rows
            Next
            k_page.Append("<tr class=""total""><td>Total</td><td class=""num"">").Append(k_total.ToString("N0", CultureInfo.InvariantCulture)).AppendLine("</td></tr>")
            k_page.AppendLine("</tbody></table>")
        End If
        If k_data.JournalFolder.Length > 0 Then
            k_page.Append("<p class=""meta"">Jurnalul SQL al rulării: ")
            Dim k_link = FolderLink(k_data.JournalFolder)
            If k_link IsNot Nothing Then
                k_page.Append("<a href=""").Append(Encode(k_link)).Append(""">").Append(Encode(k_data.JournalFolder)).Append("</a>")
            Else
                k_page.Append(Encode(k_data.JournalFolder))
            End If
            k_page.AppendLine("</p>")
        End If
        k_page.AppendLine("</div>")
    End Sub

    Private Shared Sub AppendFindings(k_page As StringBuilder, k_data As AvcReportData,
                                      k_blocking As List(Of Finding), k_warnings As List(Of Finding), k_info As List(Of Finding))
        AppendGroup(k_page, "blocking", $"Blocaje — opresc migrarea ({k_blocking.Count})", k_blocking,
                    If(k_data.WasVerified, "Niciun blocaj.", "Nu s-a făcut încă verificarea pe server."))
        AppendGroup(k_page, "warning", $"Atenționări — de citit înainte ({k_warnings.Count})", k_warnings, "Nicio atenționare.")
        AppendGroup(k_page, "info", $"Informații ({k_info.Count})", k_info, "Nicio informație.")
    End Sub

    Private Shared Sub AppendGroup(k_page As StringBuilder, k_class As String, k_title As String, k_findings As List(Of Finding), k_emptyText As String)
        k_page.AppendLine("<div class=""card"">")
        k_page.Append("<h2 class=""").Append(k_class).Append(""">").Append(Encode(k_title)).AppendLine("</h2>")
        If k_findings.Count = 0 Then
            k_page.Append("<p class=""").Append(If(k_class = "blocking", "none", "hint")).Append(""">").Append(Encode(k_emptyText)).AppendLine("</p>")
        End If
        For Each k_finding In k_findings
            k_page.Append("<div class=""finding ").Append(k_class).AppendLine(""">")
            k_page.Append("<div class=""msg"">").Append(Encode(k_finding.Message)).AppendLine("</div>")
            Dim k_where As String = k_finding.Table
            If k_finding.Column.Length > 0 Then k_where &= "." & k_finding.Column
            k_where = If(k_where.Length > 0, k_where & " · ", String.Empty) & k_finding.Kind
            k_page.Append("<div class=""where"">").Append(Encode(k_where)).AppendLine("</div>")
            If k_finding.Detail.Length > 0 Then k_page.Append("<pre>").Append(Encode(k_finding.Detail)).AppendLine("</pre>")
            k_page.AppendLine("</div>")
        Next
        k_page.AppendLine("</div>")
    End Sub

    Private Shared Sub AppendSteps(k_page As StringBuilder, k_data As AvcReportData)
        If k_data.Steps.Count = 0 Then Return
        k_page.AppendLine("<div class=""card"">")
        k_page.AppendLine("<h2>Ce se migrează (în ordinea rulării)</h2>")
        k_page.AppendLine("<table><thead><tr><th>Bifat</th><th>Etapa</th><th>Pas</th><th>Tabel Access</th><th>Rânduri Access</th><th>De scris</th><th>Acum în MariaDB</th></tr></thead><tbody>")
        For Each k_step In k_data.Steps
            k_page.Append("<tr").Append(If(k_step.Selected, String.Empty, " class=""off""")).Append("><td>").Append(If(k_step.Selected, "Da", "Nu")).Append("</td><td>").
                   Append(Encode(k_step.Stage)).Append("</td><td>").Append(Encode(k_step.Title)).Append("</td><td>").Append(Encode(k_step.AccessTable)).
                   Append("</td><td class=""num"">").Append(Encode(k_step.AccessRows)).Append("</td><td class=""num"">").Append(Encode(k_step.ToWrite)).
                   Append("</td><td class=""num"">").Append(Encode(k_step.InDatabase)).AppendLine("</td></tr>")
        Next
        k_page.AppendLine("</tbody></table></div>")
    End Sub

    Private Shared Sub AppendUnits(k_page As StringBuilder, k_data As AvcReportData)
        If k_data.Units.Count = 0 Then Return
        k_page.AppendLine("<div class=""card"">")
        k_page.AppendLine("<h2>Unitățile DC-ului și subunitățile lor</h2>")
        k_page.AppendLine("<table><thead><tr><th>Transferă</th><th>IdUnitate</th><th>Unitate</th><th>Denumire (subunitate)</th><th>Capitol</th><th>Sursă</th><th>Zecimale salarii</th><th>Părinte</th><th>AnDate</th><th>Stare</th></tr></thead><tbody>")
        For Each k_unit In k_data.Units
            k_page.Append("<tr").Append(If(k_unit.Ticked, String.Empty, " class=""off""")).Append("><td>").Append(If(k_unit.Ticked, "Da", "Nu")).Append("</td><td>").
                   Append(Encode(k_unit.Id)).Append("</td><td>").Append(Encode(k_unit.Name)).Append("</td><td>").Append(Encode(k_unit.Denomination)).Append("</td><td>").
                   Append(Encode(k_unit.Chapter)).Append("</td><td>").Append(Encode(k_unit.Source)).Append("</td><td>").Append(Encode(k_unit.Decimals)).Append("</td><td>").
                   Append(Encode(k_unit.Parent)).Append("</td><td>").Append(Encode(k_unit.AnDate)).Append("</td><td>").Append(Encode(k_unit.State)).AppendLine("</td></tr>")
        Next
        k_page.AppendLine("</tbody></table></div>")
    End Sub

    ' ---- helpers -------------------------------------------------------------------------------------------------

    Private Shared Function Encode(k_text As String) As String
        Return WebUtility.HtmlEncode(If(k_text, String.Empty))
    End Function

    Private Shared Function Blank(k_text As String) As String
        Return If(String.IsNullOrWhiteSpace(k_text), "—", k_text)
    End Function

    ''' <summary>A file:/// link to an existing folder (the report form opens it in Explorer), or Nothing.</summary>
    Private Shared Function FolderLink(k_folder As String) As String
        If Not IO.Directory.Exists(k_folder) Then Return Nothing
        Return New Uri(IO.Path.GetFullPath(k_folder).TrimEnd("\"c) & "\").AbsoluteUri
    End Function

End Class
