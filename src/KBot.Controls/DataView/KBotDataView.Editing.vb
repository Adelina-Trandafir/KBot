Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' The EDITING part of <see cref="KBotDataView"/> (slice 0010-06) -- the reason the grid is
''' unbound. ONE real editor (a TextBox or a ComboBox, both declared in the Designer since
''' 0010-01) floats over the active cell, so the handle count stays constant however many rows
''' there are.
'''
''' Cycle: <c>BeginEdit</c> -> (Enter/Tab/focus loss/move/scroll) -> <c>CommitEdit</c> with a
''' veto through <c>CellValidating</c>, or Esc -> <c>CancelEdit</c>. The three are
''' <c>Friend</c> so the tests can drive them headless (there is no message loop).
'''
''' <para><b>Slice 0085: the editor is invisible as a control.</b> The text editor has no border,
''' sits in the cell's CONTENT rectangle (the cell minus <see cref="KBotDataColumn.CellPadding"/>,
''' DPI-scaled), uses the cell's resolved font, colours and horizontal alignment (the same
''' <c>RowFormatting</c>/<c>CellFormatting</c> pass the painter runs), and its text starts on the
''' exact pixel where <c>TextRenderer</c> draws the cell text: the edit control's own margins are
''' zeroed and replaced by the renderer's glyph padding, and it is vertically centred on one line
''' of text like <c>TextFormatFlags.VerticalCenter</c>. The painter skips the text of the cell
''' being edited, so nothing shows through. Everything is re-applied on every layout pass
''' (resize, theme, DPI change), so the editor follows the cell wherever the cell goes.</para>
''' </summary>
Partial Class KBotDataView

    Private _editing As Boolean = False
    Private _editColumnKey As String
    Private _editRowIndex As Integer = -1

    ' While True, editor events (Leave etc.) are ignored -- hiding an editor raises Leave,
    ' which would otherwise re-enter CommitEdit.
    Private _suppressEditorEvents As Boolean = False

    ' Background the painter fills the edited cell with (the column's UNSELECTED look, slice
    ' 0085): the whole cell, padding included, not just the one-line editor.
    Private _editBackColor As Color = Color.Empty

    Private _arrowKeyEditing As Boolean = True
    Private _enterKeyMode As KBotEnterKeyMode = KBotEnterKeyMode.NextRow

    Private Const EM_SETMARGINS As Integer = &HD3
    Private Const EC_LEFTMARGIN As Integer = &H1
    Private Const EC_RIGHTMARGIN As Integer = &H2

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    ''' <summary>Raised before the value is written; the handler may reject or correct it.</summary>
    Public Event CellValidating As EventHandler(Of KBotCellValidatingEventArgs)

    ''' <summary>True while a cell is being edited.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsEditing As Boolean
        Get
            Return _editing
        End Get
    End Property

    ''' <summary>
    ''' The arrows move the EDIT from cell to cell without closing the editor: up/down go to the
    ''' nearest EDITABLE cell of the same column on the row drawn above/below (group bands are
    ''' skipped, the footer band is never a row), left/right go to the next EDITABLE cell of the
    ''' row, skipping read-only ones. Default True.
    '''
    ''' <para>Left/right only act from the END of the text: with the caret in the middle of a word
    ''' they stay a caret move -- otherwise fixing a typo would throw the operator into another
    ''' cell. On a dropped-down combo the arrows stay the combo's, because there they pick a
    ''' value.</para>
    '''
    ''' <para>Off: the arrows in the editor behave like a plain text box, and moving between cells
    ''' is left to Tab and Enter.</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Arrows move the edit between editable cells. Off = arrows only move the caret in the text.")>
    <DefaultValue(True)>
    Public Property ArrowKeyEditing As Boolean
        Get
            Return _arrowKeyEditing
        End Get
        Set(value As Boolean)
            _arrowKeyEditing = value
        End Set
    End Property

    ''' <summary>
    ''' Where Enter goes: to the next row in the same column (default, the classic continuous
    ''' form) or to the next editable cell of the same row. See <see cref="KBotEnterKeyMode"/> --
    ''' the choice depends on whether the table is filled in by columns or by rows.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Where Enter goes: the next row (same column) or the next editable cell of the row.")>
    <DefaultValue(KBotEnterKeyMode.NextRow)>
    Public Property EnterKeyMode As KBotEnterKeyMode
        Get
            Return _enterKeyMode
        End Get
        Set(value As KBotEnterKeyMode)
            If Not [Enum].IsDefined(GetType(KBotEnterKeyMode), value) Then
                Throw New ArgumentException("Unknown Enter key mode: " & value.ToString(), NameOf(value))
            End If
            _enterKeyMode = value
        End Set
    End Property

    ' Wires the two editors' events (from the constructor).
    Private Sub WireEditors()
        AddHandler editText.KeyDown, AddressOf OnEditorKeyDown
        AddHandler editCombo.KeyDown, AddressOf OnEditorKeyDown
        AddHandler editText.Leave, AddressOf OnEditorLeave
        AddHandler editCombo.Leave, AddressOf OnEditorLeave
    End Sub

    ' ========================================================================
    ' CAN IT BE EDITED?
    ' ========================================================================

    ''' <summary>
    ''' The editability rule: the grid is not read-only, the column is not read-only, the cell
    ''' is EFFECTIVELY enabled (0010-04) and the type has an editor (Text or Combo).
    ''' </summary>
    Public Function CanEdit(colKey As String, rowIndex As Integer) As Boolean
        Try
            If _readOnlyGrid Then Return False
            If rowIndex < 0 OrElse rowIndex >= _rows.Count Then Return False
            Dim col As KBotDataColumn = Column(colKey)
            If col.ReadOnly Then Return False
            If col.ColumnType <> KBotColumnType.Text AndAlso col.ColumnType <> KBotColumnType.Combo Then Return False
            Return IsCellEnabled(colKey, rowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CanEdit", ex)
            Throw
        End Try
    End Function

    ' ========================================================================
    ' START / COMMIT / CANCEL
    ' ========================================================================

    ''' <summary>
    ''' Enters edit mode on the given cell. An open editor is committed first (one live editor
    ''' only). Returns False when the cell is not editable or the previous commit was rejected.
    ''' </summary>
    Friend Function BeginEdit(colKey As String, rowIndex As Integer) As Boolean
        Try
            If _editing Then
                If Not CommitEdit() Then Return False     ' commit rejected => stay where we were
            End If
            If Not CanEdit(colKey, rowIndex) Then Return False

            EnsureVisible(rowIndex)
            RecalcColumnLayout()

            Dim col As KBotDataColumn = Column(colKey)
            Dim rect As Rectangle = CellRect(col, rowIndex)
            If rect.Width <= 0 OrElse rect.Height <= 0 Then Return False

            Dim value As Object = _rows(rowIndex)(colKey)
            _suppressEditorEvents = True
            Try
                ' The edit state goes first: PlaceEditor and the painter both read it.
                _editing = True
                _editColumnKey = colKey
                _editRowIndex = rowIndex

                If col.ColumnType = KBotColumnType.Text Then
                    editText.Text = FormatValue(value, col)
                    PlaceEditor()
                    editText.Visible = True
                    editText.BringToFront()
                    editText.Focus()
                    editText.SelectAll()
                Else
                    editCombo.Items.Clear()
                    If col.ComboItems IsNot Nothing Then
                        ' Do NOT name the variable "item": VB is case-insensitive and "Item" is
                        ' this class's Default property -- it would bind to that, not the loop.
                        For Each comboItem In col.ComboItems
                            editCombo.Items.Add(comboItem)
                        Next
                    End If
                    editCombo.Text = FormatValue(value, col)
                    If value IsNot Nothing Then
                        Dim idx As Integer = editCombo.Items.IndexOf(value)
                        If idx >= 0 Then editCombo.SelectedIndex = idx
                    End If
                    PlaceEditor()
                    editCombo.Visible = True
                    editCombo.BringToFront()
                    editCombo.Focus()
                End If
            Finally
                _suppressEditorEvents = False
            End Try

            ' The painter now skips this cell's text; repaint so the old glyphs are gone.
            InvalidateRow(rowIndex)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.BeginEdit", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Closes the edit writing the value, unless <c>CellValidating</c> rejects it. Returns
    ''' False ONLY when the handler rejected it (the editor stays open and focused).
    ''' </summary>
    Friend Function CommitEdit() As Boolean
        Try
            If Not _editing Then Return True

            Dim col As KBotDataColumn = Column(_editColumnKey)

            ' Slice 0085: a single click opens the editor, so most commits are the operator just
            ' passing through a cell. Nothing typed => nothing written: no validation, no dirty
            ' flag, no CellValueChanged -- otherwise clicking around would mark rows as edited.
            If EditorUnchanged(col, _rows(_editRowIndex)(_editColumnKey)) Then
                Dim passedRow As Integer = _editRowIndex
                EndEditState()
                InvalidateRow(passedRow)
                Return True
            End If

            Dim proposed As Object = CurrentEditorValue(col)

            Dim args As New KBotCellValidatingEventArgs(_editColumnKey, _editRowIndex, proposed)
            RaiseEvent CellValidating(Me, args)
            If args.Cancel Then
                FocusActiveEditor()
                Return False
            End If

            Dim row As KBotDataRow = _rows(_editRowIndex)
            Dim oldValue As Object = row(_editColumnKey)
            row(_editColumnKey) = args.ProposedValue      ' the handler may have corrected the value
            row.IsDirty = True                            ' operator edit => "edited"

            Dim changedKey As String = _editColumnKey
            Dim changedRow As Integer = _editRowIndex
            EndEditState()
            ' Slice 0017-01: a committed edit can change an aggregated cell -- refresh the
            ' totals band (guarded internally against BeginUpdate batches).
            RecomputeDerived()
            InvalidateRow(changedRow)

            RaiseEvent CellValueChanged(Me, New KBotCellValueEventArgs(
                changedKey, changedRow, oldValue, args.ProposedValue))
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CommitEdit", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' For a HOST that reads the grid from a button the operator can reach without leaving the
    ''' editor -- Enter on a form's AcceptButton never reaches the editor's KeyDown, and a
    ''' PerformClick moves no focus, so nothing commits the typed text. Commits it now (with the
    ''' <c>CellValidating</c> veto); True when nothing is left pending.
    ''' </summary>
    Public Function CommitPendingEdit() As Boolean
        Return CommitEdit()
    End Function

    ''' <summary>Opens the editor on a cell from the host (e.g. straight on the one editable value
    ''' when a window opens). False when the cell cannot be edited.</summary>
    Public Function EditCell(colKey As String, rowIndex As Integer) As Boolean
        If rowIndex >= 0 AndAlso rowIndex < _rows.Count Then SetCurrentCell(rowIndex, colKey)
        Return BeginEdit(colKey, rowIndex)
    End Function

    ''' <summary>Abandons the edit: nothing is written, no value event.</summary>
    Friend Sub CancelEdit()
        Try
            If Not _editing Then Return
            Dim rowIndex As Integer = _editRowIndex
            EndEditState()
            InvalidateRow(rowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CancelEdit", ex)
            Throw
        End Try
    End Sub

    ' Hides the editors and clears the edit state.
    Private Sub EndEditState()
        _suppressEditorEvents = True
        Try
            editText.Visible = False
            editCombo.Visible = False
        Finally
            _suppressEditorEvents = False
        End Try
        _editing = False
        _editColumnKey = Nothing
        _editRowIndex = -1
    End Sub

    ' The current value in the active editor.
    Private Function CurrentEditorValue(col As KBotDataColumn) As Object
        If col.ColumnType = KBotColumnType.Text Then Return editText.Text
        If editCombo.SelectedIndex >= 0 Then Return editCombo.SelectedItem
        Return editCombo.Text
    End Function

    ' True when the editor still shows exactly what BeginEdit put in it for this value.
    Private Function EditorUnchanged(col As KBotDataColumn, currentValue As Object) As Boolean
        Dim shown As String = FormatValue(currentValue, col)
        If col.ColumnType = KBotColumnType.Text Then
            Return String.Equals(editText.Text, shown, StringComparison.Ordinal)
        End If
        If editCombo.SelectedIndex >= 0 Then Return Object.Equals(editCombo.SelectedItem, currentValue)
        Return String.Equals(editCombo.Text, shown, StringComparison.Ordinal)
    End Function

    Private Sub FocusActiveEditor()
        If editText.Visible Then editText.Focus()
        If editCombo.Visible Then editCombo.Focus()
    End Sub

    ''' <summary>True when (colKey, rowIndex) is the cell currently being edited. The painter asks
    ''' this to leave the cell's text to the editor.</summary>
    Private Function IsEditingCell(colKey As String, rowIndex As Integer) As Boolean
        Return _editing AndAlso rowIndex = _editRowIndex AndAlso
               String.Equals(colKey, _editColumnKey, StringComparison.Ordinal)
    End Function

    ' The client rectangle of a cell, accounting for the frozen band, scrolling and the header.
    ' Empty when the column is not visible OR the row is filtered out -- which stops BeginEdit,
    ' since it refuses an empty rectangle.
    Private Function CellRect(col As KBotDataColumn, rowIndex As Integer) As Rectangle
        Dim y As Integer = RowTopForModel(rowIndex)
        If y = Integer.MinValue Then Return Rectangle.Empty
        For Each cl In _frozenLayout
            If ReferenceEquals(cl.Column, col) Then Return New Rectangle(cl.X, y, col.WidthPx, _rowHeight)
        Next
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            If ReferenceEquals(cl.Column, col) Then
                Return New Rectangle(_frozenBandWidth + cl.X - hOffset, y, col.WidthPx, _rowHeight)
            End If
        Next
        Return Rectangle.Empty
    End Function

    ' ========================================================================
    ' EDITOR PLACEMENT (slice 0085)
    ' ========================================================================

    ''' <summary>
    ''' Puts the open editor exactly over its cell's text and dresses it like the cell. Called by
    ''' <c>BeginEdit</c> and by every layout pass (resize, column width, theme, DPI), so the
    ''' editor never lags behind the cell it edits. No-op when nothing is being edited.
    ''' </summary>
    Private Sub PlaceEditor()
        Try
            If Not _editing Then Return
            Dim col As KBotDataColumn = Column(_editColumnKey)
            Dim cellBox As Rectangle = CellRect(col, _editRowIndex)
            If cellBox.Width <= 0 OrElse cellBox.Height <= 0 Then Return

            Dim back As Color
            Dim fore As Color
            Dim cellFont As Font = Nothing
            Dim align As ContentAlignment
            ResolveCellLook(col, _editRowIndex, back, fore, cellFont, align)
            _editBackColor = back

            ' Same rectangle the painter hands to DrawTextCell (padding scaled by ScaleDpi).
            Dim content As Rectangle = CellContentRect(col, cellBox)

            If col.ColumnType = KBotColumnType.Text Then
                If Not ReferenceEquals(editText.Font, cellFont) Then editText.Font = cellFont
                editText.BackColor = back
                editText.ForeColor = fore
                editText.TextAlign = HorizontalFrom(align)

                ' Glyph padding TextRenderer adds around the text (device pixels, so already at
                ' the control's DPI), and the height of one line of that font.
                Dim lineH As Integer
                Dim padLeft As Integer
                Dim padRight As Integer
                MeasureTextPadding(cellFont, lineH, padLeft, padRight)

                ' The editor spans the whole content width and TextRenderer's padding becomes the
                ' edit control's own margins, so the text starts on the painted pixel and a glyph
                ' that overhangs into the padding (Tahoma's «T») is not clipped. WM_SETFONT resets
                ' the margins, so this runs after the font assignment above, every time.
                Dim margins As Integer = (padRight << 16) Or (padLeft And &HFFFF)
                SendMessage(editText.Handle, EM_SETMARGINS,
                            New IntPtr(EC_LEFTMARGIN Or EC_RIGHTMARGIN), New IntPtr(margins))

                Dim h As Integer = Math.Min(lineH, cellBox.Height)
                ' VerticalCenter: GDI centres one line in the content rect rounding the spare
                ' height UP (measured: plain integer division left the editor 1 px high).
                Dim edTop As Integer = content.Top + (content.Height - h + 1) \ 2
                edTop = Math.Max(cellBox.Top, Math.Min(edTop, cellBox.Bottom - h))
                Dim w As Integer = Math.Max(1, content.Width)
                editText.Bounds = New Rectangle(content.Left, edTop, w, h)
            Else
                ' A ComboBox draws its own face and fixes its own height from the font, so it
                ' keeps the whole cell; it only takes the cell's font and colours.
                If Not ReferenceEquals(editCombo.Font, cellFont) Then editCombo.Font = cellFont
                editCombo.BackColor = back
                editCombo.ForeColor = fore
                editCombo.Bounds = cellBox
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.PlaceEditor", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The look of this cell as the painter would give it on an UNSELECTED row: row colours
    ''' (normal / alternating), then <c>RowFormatting</c>, then <c>CellFormatting</c> -- the same chain as
    ''' <c>DrawRow</c>/<c>DrawCell</c>, with fresh argument instances so a paint in progress never
    ''' sees its reused ones overwritten. Never the selected-row wash (operator, 26.09.2026): the
    ''' cell being edited keeps its column's default colours, so it stands out from the selected
    ''' row around it and it is clear which cell is being edited.
    ''' </summary>
    Private Sub ResolveCellLook(col As KBotDataColumn, rowIndex As Integer,
                                ByRef back As Color, ByRef fore As Color,
                                ByRef cellFont As Font, ByRef align As ContentAlignment)
        Dim row As KBotDataRow = _rows(rowIndex)
        Dim viewPos As Integer = ViewPositionOf(rowIndex)
        Dim isAlt As Boolean = _alternatingRows AndAlso viewPos >= 0 AndAlso (viewPos Mod 2 = 1)

        Dim rowBack As Color = If(isAlt, _cRowAltBack, _cRowBack)
        Dim rowFore As Color = _cCellText

        Dim rowArgs As New KBotRowFormattingEventArgs()
        rowArgs.Reset(rowIndex, row, rowBack, rowFore, row.Enabled)
        RaiseEvent RowFormatting(Me, rowArgs)

        Dim value As Object = row(col.Key)
        Dim cellArgs As New KBotCellFormattingEventArgs()
        cellArgs.Reset(col, row, rowIndex, value, FormatValue(value, col),
                       rowArgs.BackColor, rowArgs.ForeColor, CellFontFor(col), col.TextAlign,
                       col.Enabled AndAlso rowArgs.Enabled)
        RaiseEvent CellFormatting(Me, cellArgs)

        back = cellArgs.BackColor
        fore = cellArgs.ForeColor
        cellFont = If(cellArgs.Font, CellFontFor(col))
        align = cellArgs.Alignment
    End Sub

    ''' <summary>
    ''' Line height and the left/right glyph padding <c>TextRenderer.DrawText</c> puts around a
    ''' single line when <c>NoPadding</c> is NOT set (which is how cells are painted). Measured on
    ''' this control's own device context, so it is in device pixels at the current DPI. The
    ''' renderer's rule is left = ceil(h/6), right = ceil(h/6 * 1.5); the total is measured and the
    ''' split follows that rule, so a rounding difference lands on the right edge, never under the
    ''' first glyph.
    ''' </summary>
    Private Sub MeasureTextPadding(textFont As Font, ByRef lineH As Integer,
                                   ByRef padLeft As Integer, ByRef padRight As Integer)
        Dim flags As TextFormatFlags = TextFormatFlags.SingleLine
        Dim big As New Size(Integer.MaxValue, Integer.MaxValue)
        Using g As Graphics = CreateGraphics()
            Dim bare As Size = TextRenderer.MeasureText(g, "Wg", textFont, big, flags Or TextFormatFlags.NoPadding)
            Dim padded As Size = TextRenderer.MeasureText(g, "Wg", textFont, big, flags)
            lineH = Math.Max(1, bare.Height)
            Dim total As Integer = Math.Max(0, padded.Width - bare.Width)
            padLeft = Math.Min(total, CInt(Math.Ceiling(lineH / 6.0)))
            padRight = total - padLeft
        End Using
    End Sub

    ' The TextBox alignment matching a cell's ContentAlignment (horizontal part only).
    Private Shared Function HorizontalFrom(align As ContentAlignment) As HorizontalAlignment
        Select Case align
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                Return HorizontalAlignment.Right
            Case ContentAlignment.TopCenter, ContentAlignment.MiddleCenter, ContentAlignment.BottomCenter
                Return HorizontalAlignment.Center
            Case Else
                Return HorizontalAlignment.Left
        End Select
    End Function

    ' ========================================================================
    ' EDITOR EVENTS
    ' ========================================================================

    ' UI boundary: log and swallow.
    Private Sub OnEditorKeyDown(sender As Object, e As KeyEventArgs)
        Try
            Select Case e.KeyCode
                Case Keys.Enter
                    If CommitEdit() Then MoveAfterEnter(True)
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Escape
                    CancelEdit()
                    Focus()
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Tab
                    Dim shift As Boolean = (e.Modifiers And Keys.Shift) = Keys.Shift
                    If CommitEdit() Then MoveColumn(If(shift, -1, 1))
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Up, Keys.Down
                    If Not _arrowKeyEditing Then Return
                    ' Dropped-down combo: the arrows pick a value, they do not move the cell.
                    If editCombo.Visible AndAlso editCombo.DroppedDown Then Return
                    MoveEditVertical(If(e.KeyCode = Keys.Down, 1, -1))
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Left, Keys.Right
                    If Not _arrowKeyEditing Then Return
                    If Not CaretAtTextEdge(e.KeyCode = Keys.Left) Then Return
                    If CommitEdit() Then MoveEditableColumn(If(e.KeyCode = Keys.Left, -1, 1))
                    e.Handled = True
                    e.SuppressKeyPress = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnEditorKeyDown", ex)
        End Try
    End Sub

    ' ========================================================================
    ' KEYBOARD NAVIGATION THROUGH THE EDITABLE CELLS
    ' ========================================================================

    ''' <summary>
    ''' The model index of the nearest row, in DRAWN order, above (-1) or below (+1) the given
    ''' one whose cell in <paramref name="colKey"/> can be edited; -1 when there is none. Only
    ''' data bands count: group headers/footers are stepped over, rows inside a collapsed group
    ''' have no band and are never reached, and the grid's footer band is not a band at all.
    '''
    ''' Friend: the tests' entry point for the up/down edit order without a keyboard.
    ''' </summary>
    Friend Function NextEditableRow(colKey As String, fromRowIndex As Integer, direction As Integer) As Integer
        Try
            If direction = 0 OrElse fromRowIndex < 0 Then Return -1
            Dim n As Integer = BandCount()
            Dim start As Integer = AnchorBandOfRow(fromRowIndex)
            If start < 0 Then Return -1
            Dim stepDir As Integer = If(direction > 0, 1, -1)
            Dim i As Integer = start + stepDir
            While i >= 0 AndAlso i < n
                Dim band As KBotBand = BandAt(i)
                If band.Kind = KBotGroupBandKind.Data Then
                    Dim modelIdx As Integer = ModelIndexAt(band.ViewPosition)
                    If modelIdx >= 0 AndAlso CanEdit(colKey, modelIdx) Then Return modelIdx
                End If
                i += stepDir
            End While
            Return -1
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.NextEditableRow", ex)
            Throw
        End Try
    End Function

    ' Up/Down from inside the editor: the edit jumps to the nearest editable cell of the same
    ' column. With nowhere to go (first/last editable row, or a single row) nothing happens --
    ' the editor stays open with the caret where it was, nothing is committed.
    Private Sub MoveEditVertical(direction As Integer)
        If Not _editing Then Return
        Dim colKey As String = _editColumnKey
        Dim target As Integer = NextEditableRow(colKey, _editRowIndex, direction)
        If target < 0 Then Return
        If Not CommitEdit() Then Return               ' rejected => the editor stays where it is
        SetCurrentCell(target, colKey)
        BeginEdit(colKey, target)
    End Sub

    ''' <summary>
    ''' The next EDITABLE column on the given row, in the given direction (+1/-1). Skips
    ''' read-only, disabled or other-type columns (check, button, bar); no wrap. <c>Nothing</c> =
    ''' there is none. A <c>Nothing</c> start key searches from the matching end.
    '''
    ''' Friend: also the gate through which the tests check the fill order without a keyboard.
    ''' </summary>
    Friend Function NextEditableColumn(fromKey As String, direction As Integer, rowIndex As Integer) As KBotDataColumn
        Try
            Dim cols As List(Of KBotDataColumn) = VisibleColumns()
            If cols.Count = 0 OrElse direction = 0 Then Return Nothing

            Dim start As Integer = -1
            If fromKey IsNot Nothing Then
                For i As Integer = 0 To cols.Count - 1
                    If String.Equals(cols(i).Key, fromKey, StringComparison.Ordinal) Then
                        start = i
                        Exit For
                    End If
                Next
            End If

            ' No start point: the row's first editable cell (the last one when going back).
            Dim idx As Integer = If(start < 0, If(direction > 0, 0, cols.Count - 1), start + direction)
            While idx >= 0 AndAlso idx < cols.Count
                If CanEdit(cols(idx).Key, rowIndex) Then Return cols(idx)
                idx += direction
            End While
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.NextEditableColumn", ex)
            Throw
        End Try
    End Function

    ' Moves the edit to the row's next editable cell. At the end of the row the edit is not
    ' lost: it reopens on the cell we left, so the operator is not left with a focused grid and
    ' half-typed text in their head.
    Private Sub MoveEditableColumn(direction As Integer)
        Dim target As KBotDataColumn = NextEditableColumn(_currentColumnKey, direction, _currentRowIndex)
        If target Is Nothing Then
            BeginEdit(_currentColumnKey, _currentRowIndex)
            Return
        End If
        SetCurrentCell(_currentRowIndex, target.Key)
        ReopenEditorAtCurrentCell()
    End Sub

    ''' <summary>
    ''' Where Enter goes, per <see cref="EnterKeyMode"/>. <paramref name="reopen"/> = the key came
    ''' from an editor, so the new cell enters edit mode in turn -- that is what lets a whole
    ''' table be filled from the keyboard without a single mouse click.
    ''' </summary>
    Private Sub MoveAfterEnter(reopen As Boolean)
        If _enterKeyMode = KBotEnterKeyMode.NextEditableCell Then
            Dim nextCol As KBotDataColumn = NextEditableColumn(_currentColumnKey, 1, _currentRowIndex)
            If nextCol IsNot Nothing Then
                SetCurrentCell(_currentRowIndex, nextCol.Key)
                If reopen Then ReopenEditorAtCurrentCell()
                Return
            End If

            ' End of row: go down and take the next row's first editable field. If the row did
            ' not change (we were on the last one), do not go back to the start of the same row --
            ' that would overwrite what was just filled in.
            Dim oldRow As Integer = _currentRowIndex
            MoveRow(1)
            If _currentRowIndex <> oldRow Then
                Dim firstCol As KBotDataColumn = NextEditableColumn(Nothing, 1, _currentRowIndex)
                If firstCol IsNot Nothing Then SetCurrentCell(_currentRowIndex, firstCol.Key)
            End If
            If reopen Then ReopenEditorAtCurrentCell()
            Return
        End If

        MoveRow(1)                                   ' the Access continuous-form feel
        If reopen Then ReopenEditorAtCurrentCell()
    End Sub

    ' Reopens the editor on the current cell. Not editable (disabled row, read-only column)?
    ' Focus goes back to the grid so the arrows keep navigating.
    Private Sub ReopenEditorAtCurrentCell()
        If _currentRowIndex < 0 OrElse String.IsNullOrEmpty(_currentColumnKey) Then Return
        If CanEdit(_currentColumnKey, _currentRowIndex) Then
            BeginEdit(_currentColumnKey, _currentRowIndex)
        Else
            Focus()
        End If
    End Sub

    ' Left/right move the CELL only from the edge of the text; otherwise they are a caret move.
    ' A selection in progress (Shift+arrows) belongs to the text too, so it moves nothing.
    Private Function CaretAtTextEdge(towardLeft As Boolean) As Boolean
        If editCombo.Visible Then Return Not editCombo.DroppedDown
        If Not editText.Visible Then Return True
        If editText.SelectionLength > 0 Then Return False
        If towardLeft Then Return editText.SelectionStart <= 0
        Return editText.SelectionStart >= editText.TextLength
    End Function

    ' Losing focus commits (continuous-form behaviour).
    Private Sub OnEditorLeave(sender As Object, e As EventArgs)
        Try
            If _suppressEditorEvents Then Return
            CommitEdit()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnEditorLeave", ex)
        End Try
    End Sub

End Class
