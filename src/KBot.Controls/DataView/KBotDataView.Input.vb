Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' Partea de INPUT + SELECȚIE a <see cref="KBotDataView"/> (slice 0010-05): hit-testing,
''' celula curentă, navigație de la tastatură în stil Access, click/dublu-click, comutarea
''' bifelor și a opțiunilor, acționarea butoanelor și redimensionarea coloanelor din antet.
'''
''' Toți handler-ii de input sunt boundary UI: loghează și ÎNGHIT (nu aruncă în bucla de
''' mesaje). Logica de date pe care o cheamă (SetOptionValue etc.) își face propriul
''' log + rethrow, deci eroarea nu se pierde.
''' </summary>
Partial Class KBotDataView

    ' Starea redimensionării de coloană (drag pe marginea din antet).
    Private _resizingColumn As KBotDataColumn
    Private _resizeStartX As Integer
    Private _resizeStartWidth As Integer

    ' ========================================================================
    ' SELECȚIE
    ' ========================================================================

    ''' <summary>Indexul rândului curent (-1 = fără selecție). Setarea derulează la el.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CurrentRowIndex As Integer
        Get
            Return _currentRowIndex
        End Get
        Set(value As Integer)
            Dim clamped As Integer = If(value < 0 OrElse value >= _rows.Count, -1, value)
            SetCurrentCell(clamped, _currentColumnKey)
        End Set
    End Property

    ''' <summary>Cheia coloanei curente (Nothing = niciuna).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property CurrentColumnKey As String
        Get
            Return _currentColumnKey
        End Get
        Set(value As String)
            ' Cheie necunoscută => excepție (fără no-op tăcut); Nothing e permis (deselectare).
            If value IsNot Nothing Then Column(value)
            SetCurrentCell(_currentRowIndex, value)
        End Set
    End Property

    ''' <summary>Rândul curent, sau Nothing dacă nu e niciunul selectat.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CurrentRow As KBotDataRow
        Get
            If _currentRowIndex < 0 OrElse _currentRowIndex >= _rows.Count Then Return Nothing
            Return _rows(_currentRowIndex)
        End Get
    End Property

    ' Punctul UNIC prin care se schimbă selecția: derulează, repictează și ridică evenimentul
    ' o singură dată, doar la o schimbare reală.
    Private Sub SetCurrentCell(rowIndex As Integer, colKey As String)
        If rowIndex = _currentRowIndex AndAlso String.Equals(colKey, _currentColumnKey, StringComparison.Ordinal) Then Return
        ' Mutarea celulei curente comite editarea deschisă. Dacă handler-ul de validare a
        ' respins valoarea, mutarea NU are loc — editorul rămâne deschis pe celula lui.
        If _editing AndAlso Not CommitEdit() Then Return
        _currentRowIndex = rowIndex
        _currentColumnKey = colKey
        If rowIndex >= 0 Then EnsureVisible(rowIndex)
        Invalidate()
        RaiseEvent SelectionChanged(Me, EventArgs.Empty)
    End Sub

    ' ========================================================================
    ' HIT-TESTING
    ' ========================================================================

    ''' <summary>
    ''' Indexul de MODEL al rândului de sub un punct client, sau -1 (antet/gol/în afara zonei).
    ''' Punctul dă o POZIȚIE DE VEDERE — traducerea se face aici, o singură dată, ca tot restul
    ''' input-ului să lucreze în indici de model ca și până acum.
    ''' </summary>
    Private Function RowAtPoint(pt As Point) As Integer
        Dim top As Integer = HeaderBandHeight()
        If pt.Y < top Then Return -1
        If pt.Y >= top + ViewportHeight() Then Return -1
        Dim viewPosition As Integer = (pt.Y - top + VScrollOffset()) \ _rowHeight
        Return ModelIndexAt(viewPosition)
    End Function

    ''' <summary>Coloana de sub un X client, sau Nothing. Ține cont de banda înghețată.</summary>
    Private Function ColumnAtX(x As Integer) As KBotDataColumn
        If x < _frozenBandWidth Then
            For Each cl In _frozenLayout
                If x >= cl.X AndAlso x < cl.X + cl.Column.Width Then Return cl.Column
            Next
            Return Nothing
        End If
        Dim vx As Integer = x - _frozenBandWidth + HScrollOffset()
        For Each cl In _scrollLayout
            If vx >= cl.X AndAlso vx < cl.X + cl.Column.Width Then Return cl.Column
        Next
        Return Nothing
    End Function

    ' Coloanele vizibile în ordinea VIZUALĂ. Cum banda înghețată e formată din primele
    ' FrozenColumnCount coloane vizibile, ordinea din _columns e deja cea corectă.
    Private Function VisibleColumns() As List(Of KBotDataColumn)
        Dim list As New List(Of KBotDataColumn)()
        For Each c In _columns
            If c.IsEffectivelyVisible Then list.Add(c)
        Next
        Return list
    End Function

    ' ========================================================================
    ' NAVIGAȚIE
    ' ========================================================================

    ''' <summary>
    ''' Următoarea coloană ACTIVĂ în direcția dată (+1/-1), pornind de la cea curentă.
    ''' Sare peste coloanele dezactivate; fără wrap. Nothing => nu există.
    ''' </summary>
    Friend Function NextEnabledColumn(fromKey As String, direction As Integer) As KBotDataColumn
        Dim cols As List(Of KBotDataColumn) = VisibleColumns()
        If cols.Count = 0 Then Return Nothing

        Dim start As Integer = -1
        For i As Integer = 0 To cols.Count - 1
            If String.Equals(cols(i).Key, fromKey, StringComparison.Ordinal) Then
                start = i
                Exit For
            End If
        Next

        ' Fără punct de plecare: prima/ultima coloană activă, după direcție.
        If start < 0 Then
            If direction >= 0 Then
                For Each c In cols
                    If c.Enabled Then Return c
                Next
            Else
                For i As Integer = cols.Count - 1 To 0 Step -1
                    If cols(i).Enabled Then Return cols(i)
                Next
            End If
            Return Nothing
        End If

        Dim idx As Integer = start + direction
        While idx >= 0 AndAlso idx < cols.Count
            If cols(idx).Enabled Then Return cols(idx)
            idx += direction
        End While
        Return Nothing
    End Function

    ' Prima / ultima coloană activă (Home / End).
    Private Function EdgeEnabledColumn(first As Boolean) As KBotDataColumn
        Dim cols As List(Of KBotDataColumn) = VisibleColumns()
        If first Then
            For Each c In cols
                If c.Enabled Then Return c
            Next
        Else
            For i As Integer = cols.Count - 1 To 0 Step -1
                If cols(i).Enabled Then Return cols(i)
            Next
        End If
        Return Nothing
    End Function

    ' Mută rândul curent cu delta, limitat la intervalul valid.
    '
    ' English (slice 0028-03): the step is taken in VIEW positions, not model indices. Down-arrow
    ' means "the row drawn under this one" — under a filter or a sort, that is almost never the
    ' next model index, and stepping through the model would make the selection jump around the
    ' screen and stop on rows nobody can see.
    Private Sub MoveRow(delta As Integer)
        Dim total As Integer = ViewCount()
        If total = 0 Then Return
        Dim pozitie As Integer = ViewPositionOf(_currentRowIndex)
        Dim target As Integer = If(pozitie < 0, 0, pozitie + delta)
        target = Math.Max(0, Math.Min(target, total - 1))
        SetCurrentCell(ModelIndexAt(target), _currentColumnKey)
    End Sub

    ' Mută coloana curentă în direcția dată, dacă există o coloană activă acolo.
    Private Sub MoveColumn(direction As Integer)
        Dim target As KBotDataColumn = NextEnabledColumn(_currentColumnKey, direction)
        If target Is Nothing Then Return
        ' Fără rând curent, coloana se mută pe PRIMUL rând vizibil, nu pe modelul 0 — acela poate
        ' fi tocmai unul filtrat afară.
        Dim rand As Integer = If(_currentRowIndex < 0, ModelIndexAt(0), _currentRowIndex)
        SetCurrentCell(rand, target.Key)
    End Sub

    ' Câte rânduri intră într-o „pagină” (PageUp/PageDown).
    Private Function PageRows() As Integer
        Return Math.Max(1, ViewportHeight() \ _rowHeight)
    End Function

    ' ========================================================================
    ' TASTATURĂ
    ' ========================================================================

    ' Fără asta, WinForms ar da săgețile/Tab/Enter mai departe (schimbare de focus).
    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case (keyData And Keys.KeyCode)
            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Tab, Keys.Enter,
                 Keys.F2, Keys.PageUp, Keys.PageDown, Keys.Home, Keys.End, Keys.Space
                Return True
        End Select
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Try
            Dim ctrl As Boolean = (e.Modifiers And Keys.Control) = Keys.Control
            Dim shift As Boolean = (e.Modifiers And Keys.Shift) = Keys.Shift

            Select Case e.KeyCode
                Case Keys.Up
                    MoveRow(-1)
                Case Keys.Down
                    MoveRow(1)
                Case Keys.Left
                    MoveColumn(-1)
                Case Keys.Right
                    MoveColumn(1)
                Case Keys.Enter
                    MoveRow(1)                       ' senzația de formular continuu Access
                Case Keys.Tab
                    MoveColumn(If(shift, -1, 1))
                Case Keys.PageUp
                    MoveRow(-PageRows())
                Case Keys.PageDown
                    MoveRow(PageRows())
                Case Keys.Home
                    If ctrl Then
                        MoveRow(-ViewCount())        ' Ctrl+Home => primul rând
                    Else
                        Dim c = EdgeEnabledColumn(True)
                        If c IsNot Nothing Then SetCurrentCell(_currentRowIndex, c.Key)
                    End If
                Case Keys.End
                    If ctrl Then
                        MoveRow(ViewCount())         ' Ctrl+End => ultimul rând
                    Else
                        Dim c = EdgeEnabledColumn(False)
                        If c IsNot Nothing Then SetCurrentCell(_currentRowIndex, c.Key)
                    End If
                Case Keys.F2
                    If _currentRowIndex >= 0 AndAlso Not String.IsNullOrEmpty(_currentColumnKey) Then
                        BeginEdit(_currentColumnKey, _currentRowIndex)
                    End If
                Case Keys.Space
                    ActivateCurrentCell()
                Case Else
                    Return
            End Select
            e.Handled = True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnKeyDown", ex)
        End Try
    End Sub

    ' ========================================================================
    ' COMUTARE / ACȚIONARE
    ' ========================================================================

    ' Space pe celula curentă: comută bifa/opțiunea, sau apasă butonul.
    Private Sub ActivateCurrentCell()
        If _currentRowIndex < 0 OrElse String.IsNullOrEmpty(_currentColumnKey) Then Return
        ActivateCell(_currentColumnKey, _currentRowIndex)
    End Sub

    ''' <summary>
    ''' Comută/acționează o celulă, respectând activarea EFECTIVĂ (celulă inertă => nimic).
    ''' Punct comun pentru click și pentru Space. Friend: testele îl folosesc ca punct de
    ''' intrare headless (nu se pot trimite taste fără buclă de mesaje).
    ''' </summary>
    Friend Sub ActivateCell(colKey As String, rowIndex As Integer)
        If Not IsCellEnabled(colKey, rowIndex) Then Return
        Dim col As KBotDataColumn = Column(colKey)

        ' English: a CheckBox/OptionButton toggle mutates the row's value, so it is blocked when
        ' the grid or the column is read-only — same contract as text/combo editing (see CanEdit).
        ' A Button is a pure action (no value, no dirty), so it stays active even when read-only.
        Dim valueMutating As Boolean =
            col.ColumnType = KBotColumnType.CheckBox OrElse col.ColumnType = KBotColumnType.OptionButton
        If valueMutating AndAlso (_readOnlyGrid OrElse col.ReadOnly) Then Return

        Select Case col.ColumnType
            Case KBotColumnType.CheckBox
                Dim oldValue As Object = _rows(rowIndex)(colKey)
                Dim newValue As Boolean = Not ToBool(oldValue)
                _rows(rowIndex)(colKey) = newValue
                _rows(rowIndex).IsDirty = True      ' comutare de operator => „editat”
                InvalidateRow(rowIndex)
                RaiseEvent CellValueChanged(Me, New KBotCellValueEventArgs(colKey, rowIndex, oldValue, newValue))

            Case KBotColumnType.OptionButton
                Dim oldValue As Object = _rows(rowIndex)(colKey)
                If ToBool(oldValue) Then Return          ' deja bifată: radio nu se de-bifează
                SetOptionValue(colKey, rowIndex, True)   ' stinge și surorile din grup
                RaiseEvent CellValueChanged(Me, New KBotCellValueEventArgs(colKey, rowIndex, oldValue, True))

            Case KBotColumnType.Button
                RaiseEvent ButtonClick(Me, New KBotButtonClickEventArgs(colKey, rowIndex))
        End Select
    End Sub

    ' ========================================================================
    ' MOUSE
    ' ========================================================================

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Try
            Focus()
            ' Orice apăsare închide eticheta: operatorul a trecut la treabă, nu mai citește.
            CancelCellTooltip()
            If e.Button <> MouseButtons.Left Then Return

            ' 0) Banda de subsol (butonul de strângere) — nu e un rând, deci consumă apăsarea.
            If HandleFooterMouseDown(e.Location) Then Return

            ' 0b) Pictograma din dreapta unui antet de coloană (slice 0028-02). Se caută ÎNAINTEA
            ' redimensionării, deși cele două zone nu se ating: pictograma e o acțiune, iar o
            ' apăsare pe ea nu are voie să pornească o tragere de margine.
            If HandleHeaderIconMouseDown(e.Location) Then Return

            ' 0c) Pictograma de FILTRARE (slice 0028-03), din același motiv: e o acțiune, nu o
            ' margine de tras. Se caută după cea din dreapta, fiindcă ele nu se suprapun niciodată
            ' (așezarea le dă sloturi separate) — ordinea aici e doar cea a citirii.
            If HandleFilterIconMouseDown(e.Location) Then Return

            ' 1) Început de redimensionare pe marginea unei coloane din antet.
            Dim resizeTarget As KBotDataColumn = HeaderResizeTarget(e.Location)
            If resizeTarget IsNot Nothing Then
                _resizingColumn = resizeTarget
                _resizeStartX = e.X
                _resizeStartWidth = resizeTarget.Width
                Return
            End If

            ' 2) Selecție în zona de date.
            Dim rowIndex As Integer = RowAtPoint(e.Location)
            If rowIndex < 0 Then Return
            Dim col As KBotDataColumn = ColumnAtX(e.X)
            If col Is Nothing Then Return
            SetCurrentCell(rowIndex, col.Key)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnMouseDown", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        Try
            ' Banda de subsol: hover-ul butonului de strângere. Cât timp cursorul e acolo, nicio
            ' celulă nu e survolată, deci nici etichetă.
            If HandleFooterMouseMove(e.Location) Then
                CancelCellTooltip()
                Cursor = If(FooterIconHovered, Cursors.Hand, Cursors.Default)
                Return
            End If

            ' Redimensionare în curs: lățimea urmărește mouse-ul (limitată de MinWidth).
            If _resizingColumn IsNot Nothing Then
                _resizingColumn.Width = _resizeStartWidth + (e.X - _resizeStartX)
                ' English (slice 0013): a manual drag pins this column — a ToContent pass must
                ' not undo it. Fill/shrink still applies (via ResetColumnSizing to restore auto).
                _resizingColumn.UserSized = True
                LayoutChanged()
                Return
            End If

            ' Pictogramele din antet: hover + cursor de mână. Cât timp cursorul e peste una, nu
            ' se mai caută nici margine de redimensionare, nici etichetă de celulă.
            If UpdateHeaderIconHover(e.Location) Then
                ' Cele două pictograme sunt VECINE: trecând direct de pe una pe alta, cea părăsită
                ' n-ar mai apuca să afle că a rămas fără cursor și ar rămâne aprinsă.
                ClearFilterIconHover()
                CancelCellTooltip()
                Cursor = Cursors.Hand
                Return
            End If

            ' Pictograma de filtrare — aceeași regulă.
            If UpdateFilterIconHover(e.Location) Then
                CancelCellTooltip()
                Cursor = Cursors.Hand
                Return
            End If

            ' Cursor de redimensionare când suntem pe o margine redimensionabilă din antet.
            Dim peMargine As Boolean = HeaderResizeTarget(e.Location) IsNot Nothing
            Cursor = If(peMargine, Cursors.SizeWE, Cursors.Default)

            ' Eticheta celulei de sub cursor (doar dacă textul ei chiar nu încape). Pe marginea
            ' de redimensionare nu se cere: acolo operatorul trage, nu citește.
            If peMargine Then
                CancelCellTooltip()
                Return
            End If
            Dim tipRow As Integer = RowAtPoint(e.Location)
            Dim tipCol As KBotDataColumn = If(tipRow < 0, Nothing, ColumnAtX(e.X))
            UpdateCellTooltip(If(tipCol Is Nothing, Nothing, tipCol.Key), If(tipCol Is Nothing, -1, tipRow))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnMouseMove", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        Try
            If _resizingColumn IsNot Nothing Then
                _resizingColumn = Nothing
                Return
            End If

            If e.Button <> MouseButtons.Left Then Return
            Dim rowIndex As Integer = RowAtPoint(e.Location)
            If rowIndex < 0 Then Return
            Dim col As KBotDataColumn = ColumnAtX(e.X)
            If col Is Nothing Then Return

            ' Comutare/acționare (respectă dezactivarea), apoi evenimentul de click.
            ActivateCell(col.Key, rowIndex)
            RaiseEvent CellClick(Me, New KBotCellEventArgs(col.Key, rowIndex))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnMouseUp", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseDoubleClick(e As MouseEventArgs)
        MyBase.OnMouseDoubleClick(e)
        Try
            If e.Button <> MouseButtons.Left Then Return
            Dim rowIndex As Integer = RowAtPoint(e.Location)
            If rowIndex < 0 Then Return
            Dim col As KBotDataColumn = ColumnAtX(e.X)
            If col Is Nothing Then Return
            RaiseEvent CellDoubleClick(Me, New KBotCellEventArgs(col.Key, rowIndex))
            ' Dublu-click intră în editare (celulele needitabile sunt refuzate de CanEdit).
            BeginEdit(col.Key, rowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnMouseDoubleClick", ex)
        End Try
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        Cursor = Cursors.Default
        HandleFooterMouseLeave()
        ClearHeaderIconHover()
        ClearFilterIconHover()
        CancelCellTooltip()
    End Sub

    ' Coloana a cărei margine dreaptă din antet e sub punct (toleranță ~4px), dacă e
    ' redimensionabilă. Nothing în rest.
    Private Function HeaderResizeTarget(pt As Point) As KBotDataColumn
        If Not _showHeader OrElse pt.Y >= _headerHeight Then Return Nothing
        Dim tol As Integer = ScaleDpi(4)

        For Each cl In _frozenLayout
            Dim edge As Integer = cl.X + cl.Column.Width
            If Math.Abs(pt.X - edge) <= tol Then Return If(cl.Column.Resizable, cl.Column, Nothing)
        Next

        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            Dim edge As Integer = _frozenBandWidth + cl.X + cl.Column.Width - hOffset
            If Math.Abs(pt.X - edge) <= tol Then Return If(cl.Column.Resizable, cl.Column, Nothing)
        Next

        Return Nothing
    End Function

End Class
