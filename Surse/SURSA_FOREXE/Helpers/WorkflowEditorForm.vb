Imports System.Drawing
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

' ┌─────────────────────────────────────────────────────────────────────────┐
' │  WorkflowEditorForm  — fișierul principal                               │
' │                                                                         │
' │  Partial classes:                                                        │
' │    WorkflowEditorForm.vb             ← ești aici (state, UI, events)    │
' │    WorkflowEditorForm.TagMap.vb      ← culori + Tag→Atribute            │
' │    WorkflowEditorForm.WflLine.vb     ← model linie parsată              │
' │    WorkflowEditorForm.Highlighting.vb ← colorare syntax                 │
' │    WorkflowEditorForm.Autocomplete.vb ← dropdown autocomplete           │
' │    WorkflowEditorForm.UndoRedo.vb    ← stack Undo/Redo manual           │
' └─────────────────────────────────────────────────────────────────────────┘

Public Class WorkflowEditorForm
    Inherits Form

    Private Const WM_HSCROLL As Integer = &H114
    Private Const SB_LEFT As Integer = 6

    ' =========================================================================
    ' STATE
    ' =========================================================================
    Private ReadOnly _filePath As String
    Friend _isDirty As Boolean = False

    ' Guards re-intrare
    Friend _suppressHighlight As Boolean = False
    Friend _suppressUndo As Boolean = False

    ' ── Tracking linie curentă ──
    ' _lastLineIndex = linia pe care se afla cursorul la ultimul event
    ' _lineWasModified = True dacă pe linia respectivă s-a tastat ceva
    '   → folosit în SelectionChanged pentru a recolora linia când cursorul pleacă
    Friend _lastLineIndex As Integer = 0
    Friend _lineWasModified As Boolean = False

    ' ── Undo / Redo ──
    Friend _undoStack As New Stack(Of String)()
    Friend _redoStack As New Stack(Of String)()
    Friend _lastUndoText As String = ""

    ' ── Line cache ──
    Friend _lineCache As New List(Of WflLine)()

    ' ── Autocomplete ──
    Friend _acMode As String = ""         ' "tag" | "attr" | ""
    Friend _acAllItems As String() = Array.Empty(Of String)()   ' lista curentă (pentru filtrare)

    ' 
    Private _lastTooltipLine As Integer = -1  ' evităm refresh la fiecare MouseMove
    Friend _typingInProgress As Boolean = False

    ' =========================================================================
    ' CONSTRUCTOR
    ' =========================================================================
    Public Sub New(filePath As String)
        If String.IsNullOrEmpty(filePath) Then Throw New ArgumentNullException(NameOf(filePath))
        _filePath = filePath
        InitializeComponent()
        InitHelpPanel()        ' ← ADAUGĂ ASTA
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
        LoadFile()
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        ' Marchează editorul ca RTB cu sintaxă — KBotTheme va seta doar BackColor, nu ForeColor
        rtbEditor.Tag = "SyntaxRTB"
        ApplyEditorTheme()
        rtbEditor.Focus()
        ' Aplică tema pe restul formularului (panouri, butoane etc.) — nu atinge rtbEditor
        KBotTheme.ApplyTheme(Me)
    End Sub

    ''' <summary>
    ''' Schimbă schema de culori a editorului (dark/light) și re-aplică highlighting-ul.
    ''' Apelat și extern de KBotTheme când tema se schimbă global.
    ''' </summary>
    Friend Sub ApplyEditorTheme()
        SetColorScheme(KBotTheme.IsDark)
        rtbEditor.BackColor = CLR_BACKGROUND
        ApplyHighlightingFull()
        ApplyDocMapTheme()
        ApplyHelpPanelTheme()
    End Sub

    ' =========================================================================
    ' LOAD FILE
    ' =========================================================================
    Private Sub LoadFile()
        Try
            Dim content = File.ReadAllText(_filePath, System.Text.Encoding.UTF8)
            _suppressHighlight = True
            _suppressUndo = True
            rtbEditor.Text = content
            _suppressHighlight = False
            _suppressUndo = False
            _lastUndoText = content
            _lastLineIndex = 0
            _lineWasModified = False
            RebuildLineCache()          ' ← rebuild cache + docmap (apelat din Highlighting)
            ApplyHighlightingFull()
            _isDirty = False
            UpdateStatus()
        Catch ex As Exception
            MessageBox.Show($"Eroare la citirea fișierului:{Environment.NewLine}{ex.Message}",
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ChkWordWrap_CheckedChanged(sender As Object, e As EventArgs) Handles chkWordWrap.CheckedChanged
        rtbEditor.WordWrap = chkWordWrap.Checked
    End Sub

    Public Sub ReloadFromDisk()
        If _isDirty Then
            Dim r = MessageBox.Show(
            $"'{Path.GetFileName(_filePath)}' are modificări nesalvate. Reîncarci și pierzi modificările?",
            "Confirmare reload", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If r = DialogResult.No Then Return
        End If
        _undoStack.Clear()
        _redoStack.Clear()
        _isDirty = False
        LoadFile()
    End Sub

    ' =========================================================================
    ' EDITOR EVENTS
    ' =========================================================================
    Private Sub RtbEditor_TextChanged(sender As Object, e As EventArgs) Handles rtbEditor.TextChanged
        If _suppressHighlight Then Return

        _isDirty = True
        _lineWasModified = True
        _typingInProgress = True
        UpdateStatus()

        Dim cursorPos = rtbEditor.SelectionStart
        Dim curLineIdx = rtbEditor.GetLineFromCharIndex(cursorPos)
        Dim lineCountChanged = rtbEditor.Lines.Length <> _lineCache.Count

        If curLineIdx = _lastLineIndex AndAlso Not lineCountChanged Then
            ' Aceeași linie, același număr de linii → word highlight + update cache
            UpdateLineCacheAt(curLineIdx)
            HighlightCurrentWord(cursorPos)
            UpdateDocMapNode(curLineIdx)
        Else
            ' Linie diferită sau nr. linii schimbat (Enter/Delete cross-line)
            If lineCountChanged Then
                RebuildLineCache()      ' ← include RebuildDocMap()
            Else
                UpdateLineCacheAt(_lastLineIndex)
                UpdateDocMapNode(_lastLineIndex)
            End If
            HighlightLine(_lastLineIndex)
            _lineWasModified = False
            _lastLineIndex = curLineIdx
        End If

        CheckAutocompleteOnTextChanged(cursorPos)
    End Sub

    Private Sub RtbEditor_SelectionChanged(sender As Object, e As EventArgs) Handles rtbEditor.SelectionChanged
        If _suppressHighlight Then Return
        UpdateStatus()

        Dim curLineIdx = rtbEditor.GetLineFromCharIndex(rtbEditor.SelectionStart)

        If curLineIdx <> _lastLineIndex Then
            If _lineWasModified Then
                UpdateLineCacheAt(_lastLineIndex)
                HighlightLine(_lastLineIndex)
                UpdateDocMapNode(_lastLineIndex)
                _lineWasModified = False
            End If
            _lastLineIndex = curLineIdx

            ' Afișează eroarea liniei noi în status bar
            Dim wfl = GetCachedLine(curLineIdx)
            If wfl IsNot Nothing AndAlso Not wfl.IsLineOK Then
                UpdateStatus($"⚠  {wfl.ErrorSummary}")
            End If

            ' Sincronizează selecția în DocMap
            SyncDocMapSelection(curLineIdx)
            ' Sincro cu panoul de ajutor
            UpdateHelpForLine(curLineIdx)

        End If

        ' Ascunde autocomplete doar dacă nu tastăm (click, săgeți etc.)
        If _acListBox.Visible AndAlso Not _typingInProgress Then
            HideAutocomplete()
        End If

        _typingInProgress = False
    End Sub

    Private Sub RtbEditor_KeyDown(sender As Object, e As KeyEventArgs) Handles rtbEditor.KeyDown
        _typingInProgress = True

        ' ── Navigare în lista autocomplete ──
        If _acListBox.Visible Then
            Select Case e.KeyCode
                Case Keys.Down
                    If _acListBox.SelectedIndex < _acListBox.Items.Count - 1 Then
                        _acListBox.SelectedIndex += 1
                    End If
                    e.Handled = True : e.SuppressKeyPress = True : Return

                Case Keys.Up
                    If _acListBox.SelectedIndex > 0 Then
                        _acListBox.SelectedIndex -= 1
                    End If
                    e.Handled = True : e.SuppressKeyPress = True : Return

                Case Keys.Return, Keys.Tab
                    PushUndo()
                    CommitAutocomplete()
                    e.Handled = True : e.SuppressKeyPress = True : Return

                Case Keys.Space
                    If _acMode = "tag" Then
                        PushUndo()
                        CommitAutocomplete()
                        e.Handled = True : e.SuppressKeyPress = True : Return
                    End If
                    HideAutocomplete()

                Case Keys.Escape
                    HideAutocomplete()
                    e.Handled = True : e.SuppressKeyPress = True : Return

                Case Else
                    If e.KeyCode >= Keys.F1 AndAlso e.KeyCode <= Keys.F24 Then HideAutocomplete()
            End Select
        End If

        ' ── Push undo pe boundary tokens ──
        If Not _suppressUndo Then
            Select Case e.KeyCode
                Case Keys.Space, Keys.Return, Keys.Tab, Keys.Delete, Keys.Back
                    PushUndo()
            End Select
        End If

        ' ── Auto-indent + reset HScroll pe Enter ──────────────────────────────────
        If e.KeyCode = Keys.Return AndAlso Not _acListBox.Visible Then
            ' PushUndo e deja apelat mai sus în blocul "boundary tokens"

            Dim curPos = rtbEditor.SelectionStart
            Dim rawText = rtbEditor.Text

            ' Găsim începutul liniei LOGICE prin walk-back până la \n sau start
            ' (ignoră wrap — \n e singurul separator real de linie logică în RTB)
            Dim logLineStart As Integer = 0
            For i As Integer = curPos - 1 To 0 Step -1
                If rawText(i) = vbLf(0) Then
                    logLineStart = i + 1
                    Exit For
                End If
            Next

            ' Extragem leading whitespace de la începutul liniei logice
            ' Ne oprim la curPos ca să nu citim dincolo de cursor (linie nouă goală)
            Dim indent As New System.Text.StringBuilder()
            Dim j As Integer = logLineStart
            While j < curPos AndAlso (rawText(j) = " "c OrElse rawText(j) = vbTab(0))
                indent.Append(rawText(j))
                j += 1
            End While

            rtbEditor.SelectedText = vbCrLf & indent.ToString()

            SendMessage(rtbEditor.Handle, WM_HSCROLL, CType(SB_LEFT, IntPtr), IntPtr.Zero)

            e.Handled = True
            e.SuppressKeyPress = True
            Return
        End If
    End Sub

    Private Sub RtbEditor_MouseMove(sender As Object, e As MouseEventArgs) Handles rtbEditor.MouseMove
        Dim charIdx = rtbEditor.GetCharIndexFromPosition(e.Location)
        Dim lineIdx = rtbEditor.GetLineFromCharIndex(charIdx)

        If lineIdx = _lastTooltipLine Then Return
        _lastTooltipLine = lineIdx

        Dim wfl = GetCachedLine(lineIdx)
        If wfl Is Nothing OrElse wfl.IsLineOK OrElse wfl.TagName Is Nothing Then
            ErrTooltip.Hide(rtbEditor) : Return
        End If

        ' Tooltip doar dacă mouse-ul e peste tag name
        If lineIdx >= rtbEditor.Lines.Length Then Return
        Dim lineText = rtbEditor.Lines(lineIdx)
        Dim lineStart = rtbEditor.GetFirstCharIndexFromLine(lineIdx)
        Dim tagM = Regex.Match(lineText, "(?<=</?)\w+")
        If tagM.Success Then
            Dim tagAbsStart = lineStart + tagM.Index
            Dim tagAbsEnd = tagAbsStart + tagM.Length
            If charIdx >= tagAbsStart AndAlso charIdx <= tagAbsEnd Then
                ErrTooltip.Show(wfl.ErrorSummary, rtbEditor, e.Location.X + 12, e.Location.Y + 16)
                Return
            End If
        End If

        ErrTooltip.Hide(rtbEditor)
    End Sub

    ' =========================================================================
    ' DOCMAP SYNC — selectează nodul corespunzător liniei curente
    ' =========================================================================
    Private Sub SyncDocMapSelection(lineIdx As Integer)
        If _DocMapTree Is Nothing OrElse _DocMapTree.IsDisposed Then Return
        Dim node = FindNodeByLine(lineIdx, _DocMapTree.Nodes)
        If node IsNot Nothing Then
            _DocMapTree.SelectedNode = node
        End If
    End Sub

    ' =========================================================================
    ' SAVE / CANCEL / CLOSE
    ' =========================================================================

    Private Sub BtnSaveAs_Click(sender As Object, e As EventArgs) Handles btnSaveAs.Click
        Using dlg As New SaveFileDialog With {
            .Title = "Salvează workflow",
            .Filter = "Workflow files (*.wfl)|*.wfl|XML files (*.xml)|*.xml|All files (*.*)|*.*",
            .DefaultExt = "wfl",
            .FileName = Path.GetFileName(_filePath),
            .InitialDirectory = Path.GetDirectoryName(_filePath)
        }
            If dlg.ShowDialog() = DialogResult.OK Then
                Try
                    File.WriteAllText(dlg.FileName, rtbEditor.Text, System.Text.Encoding.UTF8)
                    _isDirty = False
                    UpdateStatus($"Salvat: {dlg.FileName}")
                    Me.Text = $"Editor Workflow — {Path.GetFileName(dlg.FileName)}"
                    lblPath.Text = dlg.FileName
                Catch ex As Exception
                    MessageBox.Show($"Eroare la salvare:{Environment.NewLine}{ex.Message}",
                                    "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        If _isDirty Then
            Dim r = MessageBox.Show("Există modificări nesalvate. Sigur vrei să închizi?",
                                    "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If r = DialogResult.No Then Return
        End If
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _isDirty AndAlso Me.DialogResult <> DialogResult.Cancel Then
            Dim r = MessageBox.Show("Există modificări nesalvate. Sigur vrei să închizi?",
                                    "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If r = DialogResult.No Then
                e.Cancel = True
                Return
            End If
        End If
        ErrTooltip.Dispose()
        MyBase.OnFormClosing(e)
    End Sub

    ' =========================================================================
    ' STATUS BAR
    ' =========================================================================

    Friend Sub UpdateStatus(Optional msg As String = "")
        Dim dirty = If(_isDirty, "  ●", "")
        Dim undoCount = If(_undoStack.Count > 0, $"  |  Undo: {_undoStack.Count}", "")
        lblStatus.Text = If(String.IsNullOrEmpty(msg),
                            $"Ln {GetCurrentLine()}  Col {GetCurrentCol()}{dirty}{undoCount}",
                            msg & dirty)
    End Sub

    Private Function GetCurrentLine() As Integer
        Return rtbEditor.GetLineFromCharIndex(rtbEditor.SelectionStart) + 1
    End Function

    Private Function GetCurrentCol() As Integer
        Dim li = rtbEditor.GetLineFromCharIndex(rtbEditor.SelectionStart)
        Return rtbEditor.SelectionStart - rtbEditor.GetFirstCharIndexFromLine(li) + 1
    End Function

    ' =========================================================================
    ' SHORTCUTS GLOBALE
    ' =========================================================================
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.Control Or Keys.S
                BtnSaveAs_Click(Nothing, EventArgs.Empty)
                Return True
            Case Keys.Control Or Keys.Z
                DoUndo()
                Return True
            Case Keys.Control Or Keys.Y
                DoRedo()
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class