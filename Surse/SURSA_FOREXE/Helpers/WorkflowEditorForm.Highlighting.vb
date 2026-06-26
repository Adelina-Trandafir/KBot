Imports System.Drawing
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm — Highlighting                                      │
' │                                                                         │
' │  Reguli de highlighting:                                                 │
' │    • Text modificat, aceeași linie → HighlightCurrentWord               │
' │    • Cursor pe linie nouă + linia anterioară a fost modificată          │
' │      → HighlightLine(linia anterioară)                                  │
' │    • Scroll / navigare pură → NIMIC                                     │
' │                                                                         │
' │  Perf: WM_SETREDRAW suspendă redraw-ul GDI complet.                    │
' │  Linii invalide: tag name roșu + underline.                            │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm

    ' =========================================================================
    ' WM_SETREDRAW
    ' =========================================================================
    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(hWnd As IntPtr, Msg As Integer,
                                        wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function
    Private Const WM_SETREDRAW As Integer = &HB

    Private Sub BeginRtbUpdate()
        SendMessage(rtbEditor.Handle, WM_SETREDRAW, New IntPtr(0), IntPtr.Zero)
    End Sub
    Private Sub EndRtbUpdate()
        SendMessage(rtbEditor.Handle, WM_SETREDRAW, New IntPtr(1), IntPtr.Zero)
        rtbEditor.Invalidate()
        rtbEditor.Update()
    End Sub

    ' =========================================================================
    ' LINE CACHE
    ' =========================================================================

    ''' <summary>
    ''' Rebuild complet al cache-ului de linii + DocMap.
    ''' Apelat la: LoadFile, DoUndo, DoRedo, și când se schimbă numărul de linii.
    ''' </summary>
    Friend Sub RebuildLineCache()
        _lineCache.Clear()
        Dim inComment = False
        Dim openTagName As String = Nothing      ' ← adăugat

        For i = 0 To rtbEditor.Lines.Length - 1
            Dim wfl = WflLine.Parse(rtbEditor.Lines(i), i, inComment,
                                _tagAttributes, _tagRequiredAttributes,
                                openTagName)                ' ← pasat
            _lineCache.Add(wfl)

            If wfl.StartsMultilineComment Then inComment = True
            If wfl.EndsMultilineComment Then inComment = False

            ' Dacă tag-ul nu s-a închis pe această linie → continuă pe următoarea
            If wfl.TagName IsNot Nothing AndAlso Not wfl.TagIsClosed Then
                openTagName = wfl.TagName
            Else
                openTagName = Nothing
            End If
        Next

        RebuildDocMap()
    End Sub

    ''' <summary>
    ''' Update incremental pentru o singură linie (fără rebuild complet).
    ''' </summary>
    Friend Sub UpdateLineCacheAt(lineIdx As Integer)
        If lineIdx < 0 OrElse lineIdx >= rtbEditor.Lines.Length Then Return

        Dim inComment = False
        Dim openTagName As String = Nothing

        If lineIdx > 0 AndAlso lineIdx - 1 < _lineCache.Count Then
            Dim prev = _lineCache(lineIdx - 1)
            inComment = prev.IsInsideMultilineComment OrElse prev.StartsMultilineComment
            ' Dacă linia anterioară e un tag deschis neînchis sau o continuare
            If Not prev.TagIsClosed AndAlso prev.TagName IsNot Nothing Then
                openTagName = prev.TagName
            End If
        End If

        Dim wfl = WflLine.Parse(rtbEditor.Lines(lineIdx), lineIdx, inComment,
                            _tagAttributes, _tagRequiredAttributes, openTagName)

        If lineIdx < _lineCache.Count Then
            _lineCache(lineIdx) = wfl
        Else
            While _lineCache.Count <= lineIdx
                _lineCache.Add(New WflLine With {.LineIndex = _lineCache.Count, .IsEmpty = True})
            End While
            _lineCache(lineIdx) = wfl
        End If
    End Sub

    Friend Function GetCachedLine(lineIdx As Integer) As WflLine
        If lineIdx >= 0 AndAlso lineIdx < _lineCache.Count Then Return _lineCache(lineIdx)
        Return Nothing
    End Function

    ' =========================================================================
    ' FULL DOCUMENT — la load și după undo/redo
    ' =========================================================================

    Friend Sub ApplyHighlightingFull()
        If rtbEditor.TextLength = 0 Then Return
        Dim selStart = rtbEditor.SelectionStart

        BeginRtbUpdate()
        _suppressHighlight = True

        rtbEditor.SelectAll()
        rtbEditor.SelectionColor = CLR_DEFAULT

        Dim text = rtbEditor.Text
        ColorPattern(text, "<!--[\s\S]*?-->", CLR_COMMENT)
        ColorPattern(text, "</?|/?>|>", CLR_TAG_BRACKET)
        ColorPattern(text, "(?<=</?)\w+", CLR_TAG_NAME)
        ColorPattern(text, "\b([A-Za-z][A-Za-z0-9_-]*)\s*=", CLR_ATTR_NAME, groupIndex:=1)
        ColorPattern(text, """[^""]*""|'[^']*'", CLR_ATTR_VALUE)
        ColorPattern(text, "\{\{[^}]+\}\}", CLR_VARIABLE)
        ColorPattern(text, "<!--[\s\S]*?-->", CLR_COMMENT)

        For i = 0 To _lineCache.Count - 1
            ApplyInvalidColorForCachedLine(i)
        Next
        For i = 0 To _lineCache.Count - 1
            ApplyUnderlineForCachedLine(i)
        Next

        rtbEditor.Select(selStart, 0)
        rtbEditor.SelectionColor = CLR_DEFAULT

        _suppressHighlight = False
        EndRtbUpdate()
    End Sub

    ' =========================================================================
    ' PER CUVÂNT
    ' =========================================================================

    Friend Sub HighlightCurrentWord(cursorPos As Integer)
        Dim lineIdx = rtbEditor.GetLineFromCharIndex(cursorPos)
        If lineIdx >= rtbEditor.Lines.Length Then Return

        Dim lineText = rtbEditor.Lines(lineIdx)
        Dim lineStart = rtbEditor.GetFirstCharIndexFromLine(lineIdx)
        Dim posInLine = cursorPos - lineStart

        If lineText.Contains("<!--") OrElse lineText.Contains("-->") Then
            HighlightLine(lineIdx) : Return
        End If

        Dim effectiveEnd = posInLine
        If posInLine > 0 AndAlso IsWordDelimiter(lineText(posInLine - 1)) Then
            effectiveEnd = posInLine - 1
        Else
            If IsInsideQuotes(lineText, posInLine) Then Return
        End If

        Dim wordStart = effectiveEnd
        Dim wordEnd = effectiveEnd

        While wordStart > 0
            If IsWordDelimiter(lineText(wordStart - 1)) Then Exit While
            wordStart -= 1
        End While

        If effectiveEnd = posInLine Then
            While wordEnd < lineText.Length
                If IsWordDelimiter(lineText(wordEnd)) Then Exit While
                wordEnd += 1
            End While
        End If

        If wordEnd <= wordStart Then Return

        Dim word = lineText.Substring(wordStart, wordEnd - wordStart)
        Dim absStart = lineStart + wordStart
        Dim wordColor = GetWordColor(lineText, wordStart, word)

        _suppressHighlight = True
        BeginRtbUpdate()

        rtbEditor.Select(absStart, word.Length)
        rtbEditor.SelectionColor = wordColor

        If wordColor = CLR_ATTR_NAME Then
            Dim cachedLine = GetCachedLine(lineIdx)
            If cachedLine IsNot Nothing AndAlso cachedLine.InvalidAttributes.
               Any(Function(a) String.Equals(a, word, StringComparison.OrdinalIgnoreCase)) Then
                rtbEditor.Select(absStart, word.Length)
                rtbEditor.SelectionColor = CLR_INVALID_ATTR
            End If
        End If

        rtbEditor.Select(cursorPos, 0)
        rtbEditor.SelectionColor = CLR_DEFAULT

        _suppressHighlight = False
        EndRtbUpdate()
    End Sub

    ' =========================================================================
    ' PER LINIE
    ' =========================================================================

    Friend Sub HighlightLine(lineIdx As Integer)
        If lineIdx < 0 OrElse lineIdx >= rtbEditor.Lines.Length Then Return

        Dim lineText = rtbEditor.Lines(lineIdx)
        Dim lineStart = rtbEditor.GetFirstCharIndexFromLine(lineIdx)
        Dim lineLen = lineText.Length
        If lineLen = 0 Then Return

        ' ← Salvăm cursorul ÎNAINTE de orice colorare
        Dim cursorSnapshot = rtbEditor.SelectionStart

        _suppressHighlight = True
        BeginRtbUpdate()

        rtbEditor.Select(lineStart, lineLen)
        rtbEditor.SelectionColor = CLR_DEFAULT
        rtbEditor.Select(lineStart, lineLen)
        rtbEditor.SelectionFont = New Font(rtbEditor.Font, FontStyle.Regular)

        Dim cachedLine = GetCachedLine(lineIdx)
        Dim isInsideComment = cachedLine IsNot Nothing AndAlso cachedLine.IsInsideMultilineComment

        If isInsideComment Then
            rtbEditor.Select(lineStart, lineLen)
            rtbEditor.SelectionColor = CLR_COMMENT
        Else
            ColorPatternInRange(lineText, lineStart, "</?|/?>|>", CLR_TAG_BRACKET)
            ColorPatternInRange(lineText, lineStart, "(?<=</?)\w+", CLR_TAG_NAME)
            ColorPatternInRange(lineText, lineStart, "\b([A-Za-z][A-Za-z0-9_-]*)\s*=", CLR_ATTR_NAME, 1)
            ColorPatternInRange(lineText, lineStart, """[^""]*""|'[^']*'", CLR_ATTR_VALUE)
            ColorPatternInRange(lineText, lineStart, "\{\{[^}]+\}\}", CLR_VARIABLE)
            If lineText.Contains("<!--") Then
                ColorPatternInRange(lineText, lineStart, "<!--.*?(?:-->|$)", CLR_COMMENT)
            End If
            ApplyInvalidColorForCachedLine(lineIdx)
            ApplyUnderlineForCachedLine(lineIdx)
        End If

        ' ← Restaurăm cursorul la poziția de la intrare în funcție
        rtbEditor.Select(cursorSnapshot, 0)
        rtbEditor.SelectionColor = CLR_DEFAULT

        _suppressHighlight = False
        EndRtbUpdate()
    End Sub

    ' =========================================================================
    ' INVALID ATTR + UNDERLINE
    ' =========================================================================

    Private Sub ApplyInvalidColorForCachedLine(lineIdx As Integer)
        Dim wfl = GetCachedLine(lineIdx)
        If wfl Is Nothing OrElse wfl.InvalidAttributes.Count = 0 Then Return
        If lineIdx >= rtbEditor.Lines.Length Then Return

        Dim lineText = rtbEditor.Lines(lineIdx)
        Dim lineStart = rtbEditor.GetFirstCharIndexFromLine(lineIdx)

        For Each m As Match In Regex.Matches(lineText, "\b([A-Za-z][A-Za-z0-9_-]*)\s*=", RegexOptions.IgnoreCase)
            Dim attrName = m.Groups(1).Value
            If wfl.InvalidAttributes.Any(Function(a) String.Equals(a, attrName, StringComparison.OrdinalIgnoreCase)) Then
                rtbEditor.Select(lineStart + m.Groups(1).Index, m.Groups(1).Length)
                rtbEditor.SelectionColor = CLR_INVALID_ATTR
            End If
        Next
    End Sub

    Private Sub ApplyUnderlineForCachedLine(lineIdx As Integer)
        Dim wfl = GetCachedLine(lineIdx)
        If wfl Is Nothing OrElse wfl.IsLineOK OrElse wfl.TagName Is Nothing Then Return
        If lineIdx >= rtbEditor.Lines.Length Then Return

        Dim lineText = rtbEditor.Lines(lineIdx)
        Dim lineStart = rtbEditor.GetFirstCharIndexFromLine(lineIdx)

        Dim tagM = Regex.Match(lineText, "(?<=</?)\w+")
        If Not tagM.Success Then Return

        rtbEditor.Select(lineStart + tagM.Index, tagM.Length)
        Dim f = rtbEditor.SelectionFont
        If f IsNot Nothing Then
            rtbEditor.SelectionFont = New Font(f, f.Style Or FontStyle.Underline)
        End If
        ' Culoare roșie DUPĂ font (SelectionFont resetează culoarea în unele versiuni RTB)
        rtbEditor.SelectionColor = CLR_INVALID_ATTR
    End Sub

    ' =========================================================================
    ' HELPERS
    ' =========================================================================

    Friend Shared Function GetWordColor(lineText As String, wordStart As Integer, word As String) As Color
        Dim before = lineText.Substring(0, wordStart).TrimEnd()
        If before.EndsWith("<") OrElse before.EndsWith("</") Then Return CLR_TAG_NAME
        Dim afterPos = wordStart + word.Length
        If afterPos < lineText.Length Then
            If lineText.Substring(afterPos).TrimStart().StartsWith("=") Then Return CLR_ATTR_NAME
        End If
        Return CLR_DEFAULT
    End Function

    Friend Shared Function IsWordDelimiter(c As Char) As Boolean
        Return c = " "c OrElse c = "<"c OrElse c = ">"c OrElse c = """"c OrElse
               c = "'"c OrElse c = "="c OrElse c = "/"c OrElse c = Char.Parse(vbTab)
    End Function

    Friend Shared Function IsInsideQuotes(lineText As String, posInLine As Integer) As Boolean
        Dim count As Integer = 0
        For i = 0 To Math.Min(posInLine - 1, lineText.Length - 1)
            If lineText(i) = """"c Then count += 1
        Next
        Return count Mod 2 = 1
    End Function

    Friend Shared Function GetTagNameFromLine(lineText As String) As String
        Dim m = Regex.Match(lineText.TrimStart(), "(?<=</?)\w+")
        Return If(m.Success, m.Value, Nothing)
    End Function

    Friend Sub ColorPattern(text As String, pattern As String, color As Color,
                            Optional groupIndex As Integer = 0)
        Try
            For Each m As Match In Regex.Matches(text, pattern, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
                Dim grp = m.Groups(groupIndex)
                rtbEditor.Select(grp.Index, grp.Length)
                rtbEditor.SelectionColor = color
            Next
        Catch
        End Try
    End Sub

    Friend Sub ColorPatternInRange(lineText As String, lineStart As Integer,
                                   pattern As String, color As Color,
                                   Optional groupIndex As Integer = 0)
        Try
            For Each m As Match In Regex.Matches(lineText, pattern, RegexOptions.IgnoreCase)
                Dim grp = m.Groups(groupIndex)
                rtbEditor.Select(lineStart + grp.Index, grp.Length)
                rtbEditor.SelectionColor = color
            Next
        Catch
        End Try
    End Sub

End Class