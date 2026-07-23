Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Partea de EDITARE a <see cref="KBotDataView"/> (slice 0010-06) — motivul pentru care grila
''' e nelegată de date. Peste celula activă plutește UN SINGUR editor real (TextBox sau
''' ComboBox, declarați în Designer din 0010-01), deci numărul de handle-uri rămâne constant
''' indiferent de câte rânduri sunt.
'''
''' Ciclul: <c>BeginEdit</c> → (Enter/Tab/pierdere focus/mutare/derulare) → <c>CommitEdit</c>
''' cu veto prin <c>CellValidating</c>, sau Esc → <c>CancelEdit</c>. Cele trei sunt
''' <c>Friend</c>, ca testele să le poată conduce headless (nu există buclă de mesaje).
''' </summary>
Partial Class KBotDataView

    Private _editing As Boolean = False
    Private _editColumnKey As String
    Private _editRowIndex As Integer = -1

    ' Cât e True, evenimentele editorilor (Leave etc.) se ignoră — ascunderea unui editor
    ' declanșează Leave, care altfel ar re-intra în CommitEdit.
    Private _suppressEditorEvents As Boolean = False

    ''' <summary>Ridicat înaintea scrierii valorii; handler-ul poate respinge sau corecta.</summary>
    Public Event CellValidating As EventHandler(Of KBotCellValidatingEventArgs)

    ''' <summary>True cât o celulă e în editare.</summary>
    Public ReadOnly Property IsEditing As Boolean
        Get
            Return _editing
        End Get
    End Property

    ' Legarea evenimentelor celor doi editori (din constructor).
    Private Sub WireEditors()
        AddHandler editText.KeyDown, AddressOf OnEditorKeyDown
        AddHandler editCombo.KeyDown, AddressOf OnEditorKeyDown
        AddHandler editText.Leave, AddressOf OnEditorLeave
        AddHandler editCombo.Leave, AddressOf OnEditorLeave
    End Sub

    ' ========================================================================
    ' POATE FI EDITATĂ?
    ' ========================================================================

    ''' <summary>
    ''' Regula de editabilitate: grila nu e read-only, coloana nu e read-only, celula e
    ''' EFECTIV activă (0010-04) și tipul are editor (Text sau Combo).
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
    ''' Intră în editare pe celula dată. Un editor deschis se comite întâi (un singur editor
    ''' viu). Întoarce False dacă celula nu e editabilă sau dacă commit-ul precedent a fost
    ''' respins.
    ''' </summary>
    Friend Function BeginEdit(colKey As String, rowIndex As Integer) As Boolean
        Try
            If _editing Then
                If Not CommitEdit() Then Return False     ' commit respins => rămânem unde eram
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
                If col.ColumnType = KBotColumnType.Text Then
                    editText.Bounds = rect
                    editText.Text = FormatValue(value, col)
                    editText.Visible = True
                    editText.BringToFront()
                    editText.Focus()
                    editText.SelectAll()
                Else
                    editCombo.Bounds = rect
                    editCombo.Items.Clear()
                    If col.ComboItems IsNot Nothing Then
                        ' NU numi variabila „item”: VB e case-insensitive, iar „Item” e
                        ' proprietatea Default a acestei clase — s-ar lega la ea, nu la buclă.
                        For Each comboItem In col.ComboItems
                            editCombo.Items.Add(comboItem)
                        Next
                    End If
                    editCombo.Text = FormatValue(value, col)
                    If value IsNot Nothing Then
                        Dim idx As Integer = editCombo.Items.IndexOf(value)
                        If idx >= 0 Then editCombo.SelectedIndex = idx
                    End If
                    editCombo.Visible = True
                    editCombo.BringToFront()
                    editCombo.Focus()
                End If
            Finally
                _suppressEditorEvents = False
            End Try

            _editing = True
            _editColumnKey = colKey
            _editRowIndex = rowIndex
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.BeginEdit", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Închide editarea scriind valoarea, dacă <c>CellValidating</c> n-o respinge. Întoarce
    ''' False DOAR când handler-ul a respins-o (editorul rămâne deschis și focalizat).
    ''' </summary>
    Friend Function CommitEdit() As Boolean
        Try
            If Not _editing Then Return True

            Dim col As KBotDataColumn = Column(_editColumnKey)
            Dim proposed As Object = CurrentEditorValue(col)

            Dim args As New KBotCellValidatingEventArgs(_editColumnKey, _editRowIndex, proposed)
            RaiseEvent CellValidating(Me, args)
            If args.Cancel Then
                FocusActiveEditor()
                Return False
            End If

            Dim row As KBotDataRow = _rows(_editRowIndex)
            Dim oldValue As Object = row(_editColumnKey)
            row(_editColumnKey) = args.ProposedValue      ' handler-ul poate fi corectat valoarea
            row.IsDirty = True                            ' editare de operator => „editat”

            Dim changedKey As String = _editColumnKey
            Dim changedRow As Integer = _editRowIndex
            EndEditState()
            ' English (slice 0017-01): a committed edit can change an aggregated cell — refresh
            ' the totals band (guarded internally against BeginUpdate batches).
            RecomputeTotals()
            InvalidateRow(changedRow)

            RaiseEvent CellValueChanged(Me, New KBotCellValueEventArgs(
                changedKey, changedRow, oldValue, args.ProposedValue))
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CommitEdit", ex)
            Throw
        End Try
    End Function

    ''' <summary>Abandonează editarea: nimic nu se scrie, niciun eveniment de valoare.</summary>
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

    ' Ascunde editorii și golește starea de editare.
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

    ' Valoarea curentă din editorul activ.
    Private Function CurrentEditorValue(col As KBotDataColumn) As Object
        If col.ColumnType = KBotColumnType.Text Then Return editText.Text
        If editCombo.SelectedIndex >= 0 Then Return editCombo.SelectedItem
        Return editCombo.Text
    End Function

    Private Sub FocusActiveEditor()
        If editText.Visible Then editText.Focus()
        If editCombo.Visible Then editCombo.Focus()
    End Sub

    ' Dreptunghiul (coordonate client) al unei celule, ținând cont de banda înghețată,
    ' de derulare și de antet. Empty dacă coloana nu e vizibilă.
    Private Function CellRect(col As KBotDataColumn, rowIndex As Integer) As Rectangle
        Dim y As Integer = RowTop(rowIndex)
        For Each cl In _frozenLayout
            If ReferenceEquals(cl.Column, col) Then Return New Rectangle(cl.X, y, col.Width, _rowHeight)
        Next
        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            If ReferenceEquals(cl.Column, col) Then
                Return New Rectangle(_frozenBandWidth + cl.X - hOffset, y, col.Width, _rowHeight)
            End If
        Next
        Return Rectangle.Empty
    End Function

    ' ========================================================================
    ' EVENIMENTELE EDITORILOR
    ' ========================================================================

    ' Boundary UI: loghează și înghite.
    Private Sub OnEditorKeyDown(sender As Object, e As KeyEventArgs)
        Try
            Select Case e.KeyCode
                Case Keys.Enter
                    If CommitEdit() Then MoveRow(1)          ' Enter => commit + un rând mai jos
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
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnEditorKeyDown", ex)
        End Try
    End Sub

    ' Pierderea focusului comite (comportament de formular continuu).
    Private Sub OnEditorLeave(sender As Object, e As EventArgs)
        Try
            If _suppressEditorEvents Then Return
            CommitEdit()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnEditorLeave", ex)
        End Try
    End Sub

End Class
