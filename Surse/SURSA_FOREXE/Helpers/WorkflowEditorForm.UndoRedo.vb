Imports System.Windows.Forms

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm — UndoRedo                                          │
' │  Stack manual Undo/Redo — push pe boundary tokens (Space/Enter/etc.)    │
' │  Ctrl+Z / Ctrl+Y interceptate în ProcessCmdKey din fișierul principal.  │
' └─────────────────────────────────────────────────────────────────────────┘
Partial Public Class WorkflowEditorForm

    ''' <summary>
    ''' Salvează snapshot-ul curent al textului în undo stack.
    ''' Apelat ÎNAINTE ca tasta boundary să modifice textul.
    ''' Nu face nimic dacă textul nu s-a schimbat față de ultimul snapshot.
    ''' </summary>
    Friend Sub PushUndo()
        Dim current = rtbEditor.Text
        If current = _lastUndoText Then Return
        _undoStack.Push(current)
        _lastUndoText = current
        _redoStack.Clear()
    End Sub

    Friend Sub DoUndo()
        If _undoStack.Count = 0 Then Return

        _suppressUndo = True
        _suppressHighlight = True

        Dim savedPos = rtbEditor.SelectionStart
        _redoStack.Push(rtbEditor.Text)
        Dim restored = _undoStack.Pop()
        rtbEditor.Text = restored
        _lastUndoText = restored

        _suppressHighlight = False
        _suppressUndo = False

        rtbEditor.SelectionStart = Math.Min(savedPos, rtbEditor.TextLength)
        _lastLineIndex = rtbEditor.GetLineFromCharIndex(rtbEditor.SelectionStart)
        _lineWasModified = False

        RebuildLineCache()
        ApplyHighlightingFull()

        _isDirty = True
        UpdateStatus()
    End Sub

    Friend Sub DoRedo()
        If _redoStack.Count = 0 Then Return

        _suppressUndo = True
        _suppressHighlight = True

        Dim savedPos = rtbEditor.SelectionStart
        _undoStack.Push(rtbEditor.Text)
        Dim restored = _redoStack.Pop()
        rtbEditor.Text = restored
        _lastUndoText = restored

        _suppressHighlight = False
        _suppressUndo = False

        rtbEditor.SelectionStart = Math.Min(savedPos, rtbEditor.TextLength)
        _lastLineIndex = rtbEditor.GetLineFromCharIndex(rtbEditor.SelectionStart)
        _lineWasModified = False

        RebuildLineCache()
        ApplyHighlightingFull()

        _isDirty = True
        UpdateStatus()
    End Sub

End Class