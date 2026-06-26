Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm — Autocomplete                                      │
' │                                                                         │
' │  Fix-uri față de versiunea anterioară:                                  │
' │    • CommitAutocomplete: salvează _acMode ÎNAINTE de HideAutocomplete   │
' │    • Cursor repositionat corect după SelectedText= (RTB pune la start)  │
' │    • Filtrare: Contains în loc de StartsWith                            │
' │    • Atribute disponibile: din WflLine.GetAvailableAttributes           │
' │    • Atribute excluse: caută ÎNAINTE în text până la '<' (cross-line)   │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm

    ' =========================================================================
    ' TRIGGER — apelat din TextChanged
    ' =========================================================================

    Friend Sub CheckAutocompleteOnTextChanged(cursorPos As Integer)
        If cursorPos = 0 OrElse rtbEditor.TextLength = 0 Then
            HideAutocomplete() : Return
        End If

        Dim text = rtbEditor.Text
        Dim ch = text(cursorPos - 1)

        Select Case ch

            Case "<"c
                ' Nu declanșa pentru <!-- (urmează '!')
                If cursorPos < text.Length AndAlso text(cursorPos) = "!"c Then
                    HideAutocomplete() : Return
                End If
                _acMode = "tag"
                _acAllItems = _allTags
                ShowAutocomplete(cursorPos, "")

            Case " "c
                If Not _acListBox.Visible Then
                    Dim tagName = GetCurrentTagContext(cursorPos)
                    If tagName IsNot Nothing AndAlso _tagAttributes.ContainsKey(tagName) Then
                        _acMode = "attr"
                        ' Folosim WflLine din cache dacă e disponibilă; altfel parsăm text
                        Dim lineIdx = rtbEditor.GetLineFromCharIndex(cursorPos)
                        Dim cachedLine = GetCachedLine(lineIdx)
                        Dim available As String()
                        If cachedLine IsNot Nothing AndAlso
                           String.Equals(cachedLine.TagName, tagName, StringComparison.OrdinalIgnoreCase) Then
                            available = cachedLine.GetAvailableAttributes(_tagAttributes)
                        Else
                            ' Fallback: exclude atributele găsite prin căutare în text
                            Dim usedAttrs = GetUsedAttrsInCurrentTag(cursorPos)
                            available = _tagAttributes(tagName).
                                Where(Function(a) Not usedAttrs.Contains(a)).
                                OrderBy(Function(a) a).
                                ToArray()
                        End If
                        _acAllItems = available
                        ShowAutocomplete(cursorPos, "")
                    Else
                        HideAutocomplete()
                    End If
                Else
                    HideAutocomplete()
                End If

            Case Else
                ' Filtrare live cu Contains (nu StartsWith)
                If _acListBox.Visible Then
                    Dim typedPrefix = GetCurrentWordAt(cursorPos)
                    If typedPrefix.Length = 0 Then HideAutocomplete() : Return

                    Dim filtered = _acAllItems.
                        Where(Function(i) i.Contains(typedPrefix, StringComparison.OrdinalIgnoreCase)).
                        OrderBy(Function(i) i.IndexOf(typedPrefix, StringComparison.OrdinalIgnoreCase)).
                        ThenBy(Function(i) i).
                        ToArray()

                    If filtered.Length = 0 Then
                        HideAutocomplete()
                    Else
                        RefreshListItems(filtered, cursorPos)
                    End If
                End If

        End Select
    End Sub

    ' =========================================================================
    ' SHOW / REFRESH / HIDE
    ' =========================================================================

    Private Sub ShowAutocomplete(cursorPos As Integer, filter As String)
        Dim items As String()
        If String.IsNullOrEmpty(filter) Then
            items = _acAllItems
        Else
            items = _acAllItems.
                Where(Function(i) i.Contains(filter, StringComparison.OrdinalIgnoreCase)).
                ToArray()
        End If
        If items.Length = 0 Then HideAutocomplete() : Return

        RefreshListItems(items, cursorPos)
        _acListBox.BringToFront()
        _acListBox.Visible = True
        rtbEditor.Focus()
    End Sub

    Private Sub RefreshListItems(items As String(), cursorPos As Integer)
        _acListBox.Items.Clear()
        For Each item In items
            _acListBox.Items.Add(item)
        Next
        If _acListBox.Items.Count > 0 Then _acListBox.SelectedIndex = 0

        ' Poziționare sub cursor — clampată în limitele formului
        Dim safePos = Math.Min(cursorPos, Math.Max(0, rtbEditor.TextLength - 1))
        Dim curPt = rtbEditor.GetPositionFromCharIndex(safePos)
        curPt.Y += rtbEditor.Font.Height + 4
        Dim formPt = _acListBox.Parent.PointToClient(rtbEditor.PointToScreen(curPt))

        Dim listW = 260
        Dim itemH = If(_acListBox.ItemHeight > 0, _acListBox.ItemHeight, 18)
        Dim listH = Math.Min(200, items.Length * itemH + 4)

        If formPt.X + listW > Me.ClientSize.Width Then
            formPt.X = Math.Max(0, Me.ClientSize.Width - listW)
        End If
        If formPt.Y + listH > Me.ClientSize.Height Then
            ' Arată deasupra cursorului
            formPt.Y = Math.Max(0, formPt.Y - listH - rtbEditor.Font.Height - 8)
        End If

        _acListBox.Width = listW
        _acListBox.Height = listH
        _acListBox.Location = formPt
    End Sub

    Friend Sub HideAutocomplete()
        _acListBox.Visible = False
        _acMode = ""
    End Sub

    ' =========================================================================
    ' COMMIT — FIX: salvăm mode ÎNAINTE de HideAutocomplete (care îl șterge)
    ' =========================================================================

    Friend Sub CommitAutocomplete()
        If Not _acListBox.Visible OrElse _acListBox.SelectedItem Is Nothing Then Return

        Dim selected = _acListBox.SelectedItem.ToString()
        Dim mode = _acMode

        HideAutocomplete()

        Dim cursorPos = rtbEditor.SelectionStart
        Dim typedPrefix = GetCurrentWordAt(cursorPos)
        Dim insertStart = cursorPos - typedPrefix.Length
        Dim finalPos As Integer   ' ← postiția finală dorită

        _suppressUndo = True

        If mode = "tag" Then
            rtbEditor.Select(insertStart, typedPrefix.Length)
            rtbEditor.SelectedText = selected & " "
            finalPos = insertStart + selected.Length + 1   ' după spațiu

        ElseIf mode = "attr" Then
            Dim insertion = selected & "="""""   ' selector=""
            rtbEditor.Select(insertStart, typedPrefix.Length)
            rtbEditor.SelectedText = insertion
            finalPos = insertStart + selected.Length + 2   ' între ghilimele (după =")
        Else
            _suppressUndo = False : Return
        End If

        _suppressUndo = False

        ' Actualizează cache și recolorează linia
        Dim lineIdx = rtbEditor.GetLineFromCharIndex(finalPos)
        UpdateLineCacheAt(lineIdx)
        HighlightLine(lineIdx)   ' ← HighlightLine acum salvează cursorul corect (fix 1)

        ' Setează cursorul la poziția finală dorită — DUPĂ HighlightLine
        rtbEditor.SelectionStart = finalPos
    End Sub

    Private Sub AcListBox_DoubleClick(sender As Object, e As EventArgs) Handles _acListBox.DoubleClick
        PushUndo()
        CommitAutocomplete()
        rtbEditor.Focus()
    End Sub

    ' =========================================================================
    ' HELPERS
    ' =========================================================================

    ''' <summary>
    ''' Dacă cursorul e între &lt;TagName și &gt;, returnează TagName.
    ''' Caută înapoi cross-line nelimitat până la primul &lt; sau &gt;.
    ''' </summary>
    Friend Function GetCurrentTagContext(cursorPos As Integer) As String
        Dim text = rtbEditor.Text
        Dim i = cursorPos - 1
        While i >= 0
            Select Case text(i)
                Case ">"c : Return Nothing
                Case "<"c
                    If i + 1 < text.Length AndAlso (text(i + 1) = "/"c OrElse text(i + 1) = "!"c) Then
                        Return Nothing
                    End If
                    Dim rest = text.Substring(i + 1, cursorPos - i - 1)
                    Dim m = Regex.Match(rest, "^(\w+)")
                    Return If(m.Success, m.Groups(1).Value, Nothing)
            End Select
            i -= 1
        End While
        Return Nothing
    End Function

    ''' <summary>
    ''' Returnează atributele deja prezente în tag-ul curent.
    ''' Caută ÎNAPOI nelimitat de la cursor până la &lt; (cross-line, multiline safe).
    ''' </summary>
    Friend Function GetUsedAttrsInCurrentTag(cursorPos As Integer) As HashSet(Of String)
        Dim text = rtbEditor.Text
        Dim i = cursorPos - 1
        Dim tagContent As New StringBuilder()
        While i >= 0
            Dim c = text(i)
            If c = ">"c OrElse c = "<"c Then Exit While
            tagContent.Insert(0, c)
            i -= 1
        End While

        Dim result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each m As Match In Regex.Matches(tagContent.ToString(), "\b([A-Za-z][A-Za-z0-9_-]*)\s*=")
            result.Add(m.Groups(1).Value)
        Next
        Return result
    End Function

    ''' <summary>Cuvântul parțial tastat înainte de cursor (până la primul delimitor).</summary>
    Friend Function GetCurrentWordAt(cursorPos As Integer) As String
        If cursorPos = 0 Then Return ""
        Dim text = rtbEditor.Text
        Dim sb As New StringBuilder()
        Dim i = cursorPos - 1
        While i >= 0
            If IsWordDelimiter(text(i)) Then Exit While
            sb.Insert(0, text(i))
            i -= 1
        End While
        Return sb.ToString()
    End Function

End Class