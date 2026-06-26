Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm — DocMap                                            │
' │                                                                         │
' │  TreeView în dreapta editorului, rebuilt din _lineCache.                │
' │  Structură ierarhică: tag-urile cu Children apar ca noduri părinte.     │
' │  Nod roșu = linie cu erori (wfl.IsLineOK = False).                      │
' │  Click pe nod = salt la linia respectivă în editor.                     │
' │  Fix-uri față de versiunea anterioară:                                  │
' │    • Else apare ca nod container cu ramuri, afișat ca "<Else>"          │
' │    • BuildNodeLabel caută atributul cheie și pe liniile de continuare   │
' │      (IsTagContinuation=True) din _lineCache                            │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm
    ' Adaugă ca clasă separată în proiect (ex: NoHScrollTreeView.vb):
    Public Class NoHScrollTreeView
        Inherits TreeView

        <System.Runtime.InteropServices.DllImport("user32.dll")>
        Private Shared Function ShowScrollBar(hWnd As IntPtr, wBar As Integer, bShow As Boolean) As Boolean
        End Function

        Private Const SB_HORZ As Integer = 0

        Protected Overrides Sub WndProc(ByRef m As Message)
            MyBase.WndProc(m)
            ' Ascunde scrollbar-ul orizontal după ORICE mesaj care ar putea să-l arate
            If m.Msg = &HF OrElse    ' WM_PAINT
           m.Msg = &H211 OrElse  ' WM_EXITSIZEMOVE  
           m.Msg = &H215 Then    ' WM_CAPTURECHANGED
                ShowScrollBar(Me.Handle, SB_HORZ, False)
            End If
        End Sub

        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)
            ShowScrollBar(Me.Handle, SB_HORZ, False)
        End Sub

        Protected Overrides Sub OnLayout(levent As LayoutEventArgs)
            MyBase.OnLayout(levent)
            ShowScrollBar(Me.Handle, SB_HORZ, False)
        End Sub
    End Class

    ' ── Tag-uri container (pot conține copii) ────────────────────────────────
    Private Shared ReadOnly _containerTags As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
        "IfExists", "IfUnique", "IfVar",
        "Repeat", "ForEach", "ForEachVar", "While",
        "FindInTable", "Workflow", "SwitchTab",
        "Else"          ' ← Else e container: are ramuri proprii
    }

    ' ── Atribute cheie afișate în label (primul găsit câștigă) ──────────────
    Private Shared ReadOnly _keyAttrs As String() = {
        "selector", "message", "name", "source", "workflowPath",
        "value", "fieldName", "iterations", "seconds"
    }

    ' ── Culori nod (mutable — se schimbă cu tema) ────────────────────────────
    Private Shared CLR_NODE_OK As Color = Color.FromArgb(78, 201, 176)
    Private Shared CLR_NODE_ERROR As Color = Color.FromArgb(252, 57, 57)
    Private Shared CLR_NODE_COMMENT As Color = Color.FromArgb(106, 153, 85)
    Private Shared CLR_NODE_ELSE As Color = Color.FromArgb(197, 134, 192)  ' violet — distinct

    ' =========================================================================
    ' REBUILD — reconstruit complet din _lineCache
    ' =========================================================================
    Friend Sub RebuildDocMap()
        If _DocMapTree Is Nothing OrElse _DocMapTree.IsDisposed Then Return

        _DocMapTree.BeginUpdate()
        _DocMapTree.Nodes.Clear()

        Dim nodeStack As New Stack(Of (Nodes As TreeNodeCollection, TagName As String))
        nodeStack.Push((_DocMapTree.Nodes, "ROOT"))

        For Each wfl In _lineCache
            If wfl.IsEmpty Then Continue For

            ' Liniile de continuare NU creează noduri — sunt absorbite în nodul părintelui
            If wfl.IsTagContinuation Then Continue For

            If wfl.IsComment Then
                Dim commentText = wfl.RawText.Trim()
                If commentText.Length > 60 Then commentText = String.Concat(commentText.AsSpan(0, 57), "...")
                Dim commentNode = CreateNode(commentText, wfl.LineIndex, False, True, False)
                nodeStack.Peek().Nodes.Add(commentNode)
                Continue For
            End If

            If wfl.TagName Is Nothing Then Continue For

            If wfl.IsClosingTag Then
                ' Urcăm în stack până găsim tagul corespunzător
                While nodeStack.Count > 1
                    Dim top = nodeStack.Pop()
                    If String.Equals(top.TagName, wfl.TagName, StringComparison.OrdinalIgnoreCase) Then
                        Exit While
                    End If
                End While
                Continue For
            End If

            ' ── Self-closing sau opening tag ──
            Dim isError = Not wfl.IsLineOK
            Dim isElse = String.Equals(wfl.TagName, "Else", StringComparison.OrdinalIgnoreCase)
            Dim label = BuildNodeLabel(wfl)
            Dim node = CreateNode(label, wfl.LineIndex, isError, False, isElse)

            nodeStack.Peek().Nodes.Add(node)

            ' Container tag (opening, nu self-closing) → push în stack
            If (wfl.IsOpeningTag OrElse (isElse AndAlso Not wfl.IsSelfClosing)) AndAlso
               _containerTags.Contains(wfl.TagName) Then
                nodeStack.Push((node.Nodes, wfl.TagName))
            End If
        Next

        _DocMapTree.ExpandAll()
        _DocMapTree.EndUpdate()
    End Sub

    ' =========================================================================
    ' UPDATE NOD SINGUR
    ' =========================================================================
    Friend Sub UpdateDocMapNode(lineIdx As Integer)
        If _DocMapTree Is Nothing OrElse _DocMapTree.IsDisposed Then Return

        ' Liniile de continuare nu au nod propriu — găsim nodul tag-ului proprietar
        Dim wfl = GetCachedLine(lineIdx)
        If wfl Is Nothing Then Return

        Dim targetIdx = lineIdx
        If wfl.IsTagContinuation Then
            ' Căutăm înapoi primul nod care nu e continuare
            Dim i = lineIdx - 1
            While i >= 0
                Dim prev = GetCachedLine(i)
                If prev Is Nothing Then Exit While
                If Not prev.IsTagContinuation Then targetIdx = i : Exit While
                i -= 1
            End While
        End If

        Dim node = FindNodeByLine(targetIdx, _DocMapTree.Nodes)
        If node Is Nothing Then
            RebuildDocMap() : Return
        End If

        Dim ownerWfl = GetCachedLine(targetIdx)
        If ownerWfl Is Nothing Then Return

        Dim isElse = String.Equals(ownerWfl.TagName, "Else", StringComparison.OrdinalIgnoreCase)
        node.Text = BuildNodeLabel(ownerWfl)
        node.ForeColor = If(isElse, CLR_NODE_ELSE,
                         If(Not ownerWfl.IsLineOK, CLR_NODE_ERROR, CLR_NODE_OK))
        node.ToolTipText = If(Not ownerWfl.IsLineOK, ownerWfl.ErrorSummary, "")
    End Sub

    ' =========================================================================
    ' NAVIGATE — click pe nod → salt la linie
    ' =========================================================================
    Private Sub DocMapTree_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles DocMapTree.NodeMouseClick
        Dim lineIdx = CInt(e.Node.Tag)
        If lineIdx < 0 OrElse lineIdx >= rtbEditor.Lines.Length Then Return

        Dim charIdx = rtbEditor.GetFirstCharIndexFromLine(lineIdx)
        rtbEditor.SelectionStart = charIdx
        rtbEditor.SelectionLength = 0
        rtbEditor.ScrollToCaret()
        rtbEditor.Focus()
    End Sub

    ' =========================================================================
    ' HELPERS
    ' =========================================================================

    ''' <summary>
    ''' Construiește label-ul nodului: TagName + atribut cheie.
    ''' Dacă atributul cheie nu e pe linia curentă, caută pe liniile de continuare
    ''' din _lineCache (IsTagContinuation = True, TagName identic).
    ''' </summary>
    Private Function BuildNodeLabel(wfl As WflLine) As String
        If wfl.TagName Is Nothing Then Return wfl.RawText.Trim()

        ' Else — afișat explicit ca "<Else>"
        If String.Equals(wfl.TagName, "Else", StringComparison.OrdinalIgnoreCase) Then
            Return "<Else>"
        End If

        ' Caută atributul cheie: mai întâi pe linia curentă
        Dim keyAttr = _keyAttrs.FirstOrDefault(Function(a) wfl.Attributes.ContainsKey(a))
        Dim keyVal As String = Nothing

        If keyAttr IsNot Nothing Then
            keyVal = wfl.Attributes(keyAttr)
        Else
            ' Nu e pe linia curentă → caută pe liniile de continuare imediat următoare
            Dim nextIdx = wfl.LineIndex + 1
            While nextIdx < _lineCache.Count
                Dim cont = _lineCache(nextIdx)
                If Not cont.IsTagContinuation OrElse
                   Not String.Equals(cont.TagName, wfl.TagName, StringComparison.OrdinalIgnoreCase) Then
                    Exit While
                End If
                keyAttr = _keyAttrs.FirstOrDefault(Function(a) cont.Attributes.ContainsKey(a))
                If keyAttr IsNot Nothing Then
                    keyVal = cont.Attributes(keyAttr)
                    Exit While
                End If
                nextIdx += 1
            End While
        End If

        If keyVal IsNot Nothing Then
            If keyVal.Length > 35 Then keyVal = String.Concat(keyVal.AsSpan(0, 32), "...")
            Return $"{wfl.TagName}  [{keyVal}]"
        End If

        Return wfl.TagName
    End Function

    ''' <summary>Creează un TreeNode stilizat.</summary>
    Private Shared Function CreateNode(text As String, lineIdx As Integer,
                                       isError As Boolean, isComment As Boolean,
                                       isElse As Boolean) As TreeNode
        Dim clr = If(isError, CLR_NODE_ERROR,
                  If(isElse, CLR_NODE_ELSE,
                  If(isComment, CLR_NODE_COMMENT, CLR_NODE_OK)))

        Return New TreeNode(text) With {
            .Tag = lineIdx,
            .ForeColor = clr
        }
    End Function

    ''' <summary>Caută recursiv un nod cu Tag = lineIdx.</summary>
    Friend Shared Function FindNodeByLine(lineIdx As Integer,
                                          nodes As TreeNodeCollection) As TreeNode
        For Each node As TreeNode In nodes
            If CInt(node.Tag) = lineIdx Then Return node
            Dim found = FindNodeByLine(lineIdx, node.Nodes)
            If found IsNot Nothing Then Return found
        Next
        Return Nothing
    End Function

    ' =========================================================================
    ' TEMĂ — re-aplică culorile nodurilor și ale arborelui la switch dark/light
    ' =========================================================================

    ''' <summary>
    ''' Comută paleta de culori a DocMap-ului în funcție de tema curentă și
    ''' re-colorează toți nodurile existente. Apelat din ApplyEditorTheme().
    ''' </summary>
    Friend Sub ApplyDocMapTheme()
        Dim isDark = KBotTheme.IsDark

        ' Actualizăm paleta
        CLR_NODE_OK = If(isDark, Color.FromArgb(78, 201, 176), Color.FromArgb(0, 130, 100))
        CLR_NODE_ERROR = Color.FromArgb(252, 57, 57)   ' roșu — identic în ambele teme
        CLR_NODE_COMMENT = If(isDark, Color.FromArgb(106, 153, 85), Color.FromArgb(0, 100, 0))
        CLR_NODE_ELSE = If(isDark, Color.FromArgb(197, 134, 192), Color.FromArgb(130, 50, 140))

        ' Actualizăm culorile controlului TreeView
        DocMapTree.BackColor = If(isDark, KBotTheme.CLR_BG, SystemColors.Window)
        DocMapTree.ForeColor = If(isDark, KBotTheme.CLR_FG, SystemColors.WindowText)

        ' Re-colorăm nodurile existente
        DocMapTree.BeginUpdate()
        RecolorAllNodes(DocMapTree.Nodes)
        DocMapTree.EndUpdate()
        DocMapTree.Invalidate()
    End Sub

    ''' <summary>Parcurge recursiv toți nodurile și aplică culoarea corectă conform temei.</summary>
    Private Sub RecolorAllNodes(nodes As TreeNodeCollection)
        For Each node As TreeNode In nodes
            Dim lineIdx = CInt(node.Tag)
            Dim wfl = GetCachedLine(lineIdx)
            If wfl IsNot Nothing Then
                If wfl.IsComment Then
                    node.ForeColor = CLR_NODE_COMMENT
                ElseIf Not wfl.IsLineOK Then
                    node.ForeColor = CLR_NODE_ERROR
                ElseIf String.Equals(wfl.TagName, "Else", StringComparison.OrdinalIgnoreCase) Then
                    node.ForeColor = CLR_NODE_ELSE
                Else
                    node.ForeColor = CLR_NODE_OK
                End If
            End If
            RecolorAllNodes(node.Nodes)
        Next
    End Sub

End Class