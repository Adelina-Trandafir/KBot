Option Strict On
Imports System.ComponentModel
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

    Private _arrowKeyEditing As Boolean = True
    Private _enterKeyMode As KBotEnterKeyMode = KBotEnterKeyMode.NextRow

    ''' <summary>Ridicat înaintea scrierii valorii; handler-ul poate respinge sau corecta.</summary>
    Public Event CellValidating As EventHandler(Of KBotCellValidatingEventArgs)

    ''' <summary>True cât o celulă e în editare.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property IsEditing As Boolean
        Get
            Return _editing
        End Get
    End Property

    ''' <summary>
    ''' Săgețile mută EDITAREA din celulă în celulă, fără a mai închide editorul: sus/jos schimbă
    ''' rândul (aceeași coloană), stânga/dreapta trec la următoarea celulă EDITABILĂ a rândului,
    ''' sărind peste cele read-only. Implicit True.
    '''
    ''' <para>Stânga/dreapta lucrează doar de la CAPĂTUL textului: cu cursorul în mijlocul unui
    ''' cuvânt ele rămân ce au fost dintotdeauna, o mutare de cursor — altfel corectarea unei
    ''' litere greșite ar arunca operatorul în altă celulă. La un combo desfășurat, săgețile
    ''' rămân tot ale lui, fiindcă acolo aleg o valoare.</para>
    '''
    ''' <para>Stinsă, săgețile din editor își păstrează purtarea de casetă de text, iar mutarea
    ''' între celule rămâne pe seama lui Tab și Enter.</para>
    ''' </summary>
    <Category("K-BOT")>
    <Description("Săgețile mută editarea între celulele editabile. Stinsă = săgețile mută doar cursorul din text.")>
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
    ''' Unde duce Enter: pe rândul următor în aceeași coloană (implicit, formularul continuu
    ''' clasic) sau pe următoarea celulă editabilă din același rând. Vezi
    ''' <see cref="KBotEnterKeyMode"/> — alegerea ține de felul în care se completează tabelul,
    ''' pe coloane sau pe rânduri.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Unde duce Enter: rândul următor (aceeași coloană) sau următoarea celulă editabilă a rândului.")>
    <DefaultValue(KBotEnterKeyMode.NextRow)>
    Public Property EnterKeyMode As KBotEnterKeyMode
        Get
            Return _enterKeyMode
        End Get
        Set(value As KBotEnterKeyMode)
            If Not [Enum].IsDefined(GetType(KBotEnterKeyMode), value) Then
                Throw New ArgumentException("Mod de tastă Enter necunoscut: " & value.ToString(), NameOf(value))
            End If
            _enterKeyMode = value
        End Set
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
    ' de derulare și de antet. Empty dacă coloana nu e vizibilă SAU dacă rândul e filtrat afară —
    ' iar asta oprește editarea din BeginEdit, care refuză un dreptunghi gol.
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
    ' EVENIMENTELE EDITORILOR
    ' ========================================================================

    ' Boundary UI: loghează și înghite.
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
                    ' Combo desfășurat: săgețile aleg o valoare, nu mută celula.
                    If editCombo.Visible AndAlso editCombo.DroppedDown Then Return
                    If CommitEdit() Then
                        MoveRow(If(e.KeyCode = Keys.Down, 1, -1))
                        ReopenEditorAtCurrentCell()
                    End If
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
    ' NAVIGAȚIA DE LA TASTATURĂ PRIN CELULELE EDITABILE
    ' ========================================================================

    ''' <summary>
    ''' Următoarea coloană EDITABILĂ pe rândul dat, în direcția dată (+1/-1). Sare peste coloanele
    ''' read-only, dezactivate sau de alt tip (bifă, buton, bară); fără wrap. <c>Nothing</c> =
    ''' nu există niciuna. Cheie de plecare <c>Nothing</c> = se caută de la capătul potrivit.
    '''
    ''' Friend: e și poarta prin care testele pot verifica ordinea de completare fără tastatură.
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

            ' Fără punct de plecare, prima celulă editabilă a rândului (ultima, la mers înapoi).
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

    ' Trece editarea pe următoarea celulă editabilă a rândului. La capăt de rând nu se pierde
    ' editarea: se redeschide pe celula de unde am plecat, ca operatorul să nu rămână cu grila
    ' focalizată și cu textul pe jumătate scris în cap.
    Private Sub MoveEditableColumn(direction As Integer)
        Dim tinta As KBotDataColumn = NextEditableColumn(_currentColumnKey, direction, _currentRowIndex)
        If tinta Is Nothing Then
            BeginEdit(_currentColumnKey, _currentRowIndex)
            Return
        End If
        SetCurrentCell(_currentRowIndex, tinta.Key)
        ReopenEditorAtCurrentCell()
    End Sub

    ''' <summary>
    ''' Unde duce Enter, după <see cref="EnterKeyMode"/>. <paramref name="reopen"/> = apăsarea a
    ''' venit dintr-un editor, deci celula nouă intră la rândul ei în editare — asta face ca un
    ''' tabel întreg să se completeze din tastatură, fără nicio apăsare de mouse.
    ''' </summary>
    Private Sub MoveAfterEnter(reopen As Boolean)
        If _enterKeyMode = KBotEnterKeyMode.NextEditableCell Then
            Dim urmatoarea As KBotDataColumn = NextEditableColumn(_currentColumnKey, 1, _currentRowIndex)
            If urmatoarea IsNot Nothing Then
                SetCurrentCell(_currentRowIndex, urmatoarea.Key)
                If reopen Then ReopenEditorAtCurrentCell()
                Return
            End If

            ' Capăt de rând: se coboară și se ia primul câmp editabil al rândului următor. Dacă
            ' rândul nu s-a schimbat (eram pe ultimul), nu ne întoarcem la începutul aceluiași
            ' rând — asta ar rescrie ce tocmai s-a completat.
            Dim randVechi As Integer = _currentRowIndex
            MoveRow(1)
            If _currentRowIndex <> randVechi Then
                Dim primul As KBotDataColumn = NextEditableColumn(Nothing, 1, _currentRowIndex)
                If primul IsNot Nothing Then SetCurrentCell(_currentRowIndex, primul.Key)
            End If
            If reopen Then ReopenEditorAtCurrentCell()
            Return
        End If

        MoveRow(1)                                   ' senzația de formular continuu Access
        If reopen Then ReopenEditorAtCurrentCell()
    End Sub

    ' Redeschide editorul pe celula curentă. Nu e editabilă (rând dezactivat, coloană read-only)?
    ' Focusul se întoarce la grilă, ca săgețile să navigheze mai departe.
    Private Sub ReopenEditorAtCurrentCell()
        If _currentRowIndex < 0 OrElse String.IsNullOrEmpty(_currentColumnKey) Then Return
        If CanEdit(_currentColumnKey, _currentRowIndex) Then
            BeginEdit(_currentColumnKey, _currentRowIndex)
        Else
            Focus()
        End If
    End Sub

    ' Săgeata stânga/dreapta mută CELULA doar de la capătul textului; în rest rămâne mutare de
    ' cursor. O selecție în curs (Shift+săgeți) ține tot de text, deci nu mută nimic.
    Private Function CaretAtTextEdge(spreStanga As Boolean) As Boolean
        If editCombo.Visible Then Return Not editCombo.DroppedDown
        If Not editText.Visible Then Return True
        If editText.SelectionLength > 0 Then Return False
        If spreStanga Then Return editText.SelectionStart <= 0
        Return editText.SelectionStart >= editText.TextLength
    End Function

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
