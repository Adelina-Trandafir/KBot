Option Strict On
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>
''' A short, lightly formatted look at the start of a markdown file, written in the markup of <c>KBotToolTip</c>
''' (<c>&lt;b&gt;</c>, <c>&lt;i&gt;</c>): headings and **bold** become bold, `code` becomes italic, bullets get a dot,
''' table rows become «a · b · c», links keep only their text. No colour is written: the tooltip's own scheme colours apply.
''' </summary>
Public Module MdPreview

    Private Const MaxLines As Integer = 14
    Private Const MaxLineChars As Integer = 110
    Private Const MaxTotalChars As Integer = 1200

    Private ReadOnly BoldPattern As New Regex("\*\*(.+?)\*\*", RegexOptions.Compiled)
    Private ReadOnly BoldUnderscore As New Regex("__(.+?)__", RegexOptions.Compiled)
    Private ReadOnly CodePattern As New Regex("`([^`]+)`", RegexOptions.Compiled)
    Private ReadOnly LinkPattern As New Regex("!?\[([^\]]*)\]\([^)]*\)", RegexOptions.Compiled)
    Private ReadOnly HeadingPattern As New Regex("^(#{1,6})\s+(.*)$", RegexOptions.Compiled)
    Private ReadOnly BulletPattern As New Regex("^(\s*)[-*+]\s+(.*)$", RegexOptions.Compiled)
    Private ReadOnly TableRulePattern As New Regex("^\|?[\s:|\-]+\|?$", RegexOptions.Compiled)

    ''' <summary>The preview of <paramref name="k_text"/>. The first «# » heading is left out: the label already shows the title.</summary>
    Public Function Build(k_text As String) As String
        If String.IsNullOrEmpty(k_text) Then Return String.Empty
        Dim k_result As New StringBuilder()
        Dim k_lines As Integer = 0
        Dim k_skippedTitle As Boolean = False
        Dim k_inFence As Boolean = False
        Dim k_inComment As Boolean = False
        Dim k_truncated As Boolean = False

        For Each k_raw As String In k_text.Replace(vbCr, String.Empty).Split(ControlChars.Lf)
            Dim k_line As String = k_raw.TrimEnd()

            If k_inComment Then
                If k_line.Contains("-->", StringComparison.Ordinal) Then k_inComment = False
                Continue For
            End If
            If k_line.TrimStart().StartsWith("<!--", StringComparison.Ordinal) Then
                If Not k_line.Contains("-->", StringComparison.Ordinal) Then k_inComment = True
                Continue For
            End If
            If k_line.TrimStart().StartsWith("```", StringComparison.Ordinal) Then
                k_inFence = Not k_inFence
                Continue For
            End If

            If k_line.Length = 0 Then
                If k_result.Length > 0 AndAlso Not EndsWithBlank(k_result) Then
                    k_result.Append(ControlChars.Lf)
                    k_lines += 1
                End If
                Continue For
            End If

            Dim k_formatted As String
            If k_inFence Then
                k_formatted = "<i>" & Escape(Clip(k_line)) & "</i>"
            Else
                Dim k_heading As Match = HeadingPattern.Match(k_line)
                Dim k_bullet As Match = BulletPattern.Match(k_line)
                If k_heading.Success Then
                    If k_heading.Groups(1).Value.Length = 1 AndAlso Not k_skippedTitle Then
                        k_skippedTitle = True
                        Continue For
                    End If
                    k_formatted = "<b>" & Escape(Clip(Inline(k_heading.Groups(2).Value, False))) & "</b>"
                ElseIf k_line.StartsWith("|", StringComparison.Ordinal) Then
                    If TableRulePattern.IsMatch(k_line) AndAlso k_line.Contains("-", StringComparison.Ordinal) Then Continue For
                    Dim k_cells As IEnumerable(Of String) = k_line.Trim("|"c).Split("|"c).Select(Function(k_c) Inline(k_c.Trim(), False)).Where(Function(k_c) k_c.Length > 0)
                    k_formatted = FormatLine(String.Join(" · ", k_cells))
                ElseIf k_bullet.Success Then
                    k_formatted = If(k_bullet.Groups(1).Value.Length >= 2, "    ", String.Empty) & "• " & FormatLine(k_bullet.Groups(2).Value)
                ElseIf k_line.StartsWith(">", StringComparison.Ordinal) Then
                    k_formatted = "<i>" & Escape(Clip(Inline(k_line.TrimStart(">"c).Trim(), False))) & "</i>"
                Else
                    k_formatted = FormatLine(k_line)
                End If
            End If

            k_result.Append(k_formatted).Append(ControlChars.Lf)
            k_lines += 1
            If k_lines >= MaxLines OrElse k_result.Length >= MaxTotalChars Then
                k_truncated = True
                Exit For
            End If
        Next

        Dim k_out As String = k_result.ToString().Trim(ControlChars.Lf)
        If k_truncated Then k_out &= ControlChars.Lf & "<i>...</i>"
        Return k_out
    End Function

    ''' <summary>One body line: plain text first (clipped, markup characters taken out), then the markers turned into tags.</summary>
    Private Function FormatLine(k_line As String) As String
        Dim k_plain As String = Clip(LinkPattern.Replace(k_line, "$1"))
        Return Inline(Escape(k_plain), True)
    End Function

    Private Function Inline(k_text As String, k_escaped As Boolean) As String
        Dim k_result As String = LinkPattern.Replace(k_text, "$1")
        If Not k_escaped Then Return k_result.Replace("**", String.Empty).Replace("`", String.Empty)
        k_result = BoldPattern.Replace(k_result, "<b>$1</b>")
        k_result = BoldUnderscore.Replace(k_result, "<b>$1</b>")
        k_result = CodePattern.Replace(k_result, "<i>$1</i>")
        Return k_result.Replace("**", String.Empty)
    End Function

    ''' <summary>The tag characters of the text itself would be read as markup.</summary>
    Private Function Escape(k_text As String) As String
        Return k_text.Replace("<"c, "("c).Replace(">"c, ")"c)
    End Function

    Private Function Clip(k_text As String) As String
        Return If(k_text.Length > MaxLineChars, k_text.Substring(0, MaxLineChars).TrimEnd() & "...", k_text)
    End Function

    Private Function EndsWithBlank(k_builder As StringBuilder) As Boolean
        Return k_builder.Length >= 2 AndAlso k_builder(k_builder.Length - 1) = ControlChars.Lf AndAlso k_builder(k_builder.Length - 2) = ControlChars.Lf
    End Function

End Module
