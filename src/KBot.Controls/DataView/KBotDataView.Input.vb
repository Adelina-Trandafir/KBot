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
    ''' Indexul BENZII de sub un punct client, sau -1 (antet/subsol/gol/în afara zonei de date).
    ''' Punctul dă un offset în conținut — traducerea se face aici, o singură dată.
    ''' </summary>
    Private Function BandAtPoint(pt As Point) As Integer
        Dim top As Integer = HeaderBandHeight()
        If pt.Y < top Then Return -1
        If pt.Y >= top + ViewportHeight() Then Return -1
        Return BandIndexAtOffset(pt.Y - top + VScrollOffset())
    End Function

    ''' <summary>
    ''' Indexul de MODEL al rândului de sub un punct client, sau -1. De la slice 0029, «-1»
    ''' înseamnă și «acolo e o bandă de grup, nu un rând» — o bandă de grup nu are index de model,
    ''' deci nu se selectează, nu se editează și nu ridică <c>CellClick</c>.
    ''' </summary>
    Private Function RowAtPoint(pt As Point) As Integer
        Dim bi As Integer = BandAtPoint(pt)
        If bi < 0 Then Return -1
        Dim banda As KBotBand = BandAt(bi)
        If banda.Kind <> KBotGroupBandKind.Data Then Return -1
        Return ModelIndexAt(banda.ViewPosition)
    End Function

    ''' <summary>
    ''' Indexul de MODEL al randului de sub un punct client, sau -1 (antet, subsol, banda de
    ''' grup, gol). Versiunea PUBLICA a hit-testului pe randuri, pentru gazdele care conduc
    ''' singure mouse-ul peste grila — tragerea unui rand ca sa-i schimbe locul, de pilda
    ''' (migratorul rearanjeaza asa ordinea de scriere a tabelelor). Grila nu muta randuri
    ''' singura: modelul e al gazdei, deci si rearanjarea.
    ''' </summary>
    Public Function RowIndexAt(pt As Point) As Integer
        Try
            Return RowAtPoint(pt)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.RowIndexAt", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Apăsare pe o bandă de ANTET de grup: strânge/desface. True = a fost consumată, deci grila
    ''' nu mai caută nicio celulă sub ea.
    '''
    ''' <para>Ținta e TOATĂ banda, nu doar triunghiul: într-un raport care se citește dintr-o
    ''' privire, o țintă de nouă pixeli e o țintă ratată. Subsolul de grup nu e apăsabil — el nu are
    ''' ce comuta, dar tot consumă apăsarea, ca un click alături de un total să nu mute selecția
    ''' pe un rând pe care operatorul nu-l țintea.</para>
    ''' </summary>
    Private Function HandleGroupBandMouseDown(location As Point) As Boolean
        Dim bi As Integer = BandAtPoint(location)
        If bi < 0 Then Return False
        Dim banda As KBotBand = BandAt(bi)
        If banda.Kind = KBotGroupBandKind.Data Then Return False
        If banda.Kind = KBotGroupBandKind.GroupHeader AndAlso Not KBotDesignTime.IsDesignTime(Me) Then
            ToggleBandCollapse(bi)
        End If
        Return True
    End Function

    ''' <summary>Coloana de sub un X client, sau Nothing. Ține cont de banda înghețată.</summary>
    Private Function ColumnAtX(x As Integer) As KBotDataColumn
        If x < _frozenBandWidth Then
            For Each cl In _frozenLayout
                If x >= cl.X AndAlso x < cl.X + cl.Column.WidthPx Then Return cl.Column
            Next
            Return Nothing
        End If
        Dim vx As Integer = x - _frozenBandWidth + HScrollOffset()
        For Each cl In _scrollLayout
            If vx >= cl.X AndAlso vx < cl.X + cl.Column.WidthPx Then Return cl.Column
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
    '
    ' English (slice 0029): and now the step is taken over BANDS, skipping the ones that are not
    ' data rows. Two things fall out of that, both of them the point: a group header/footer is
    ' never "selected" (it has no model row to select), and a row inside a collapsed group is
    ' never reached — it has no band at all, so Down-arrow walks past the whole group in one step,
    ' exactly like the eye does.
    Private Sub MoveRow(delta As Integer)
        Dim n As Integer = BandCount()
        If n = 0 OrElse delta = 0 Then Return

        Dim pas As Integer = If(delta > 0, 1, -1)
        Dim ramase As Integer = Math.Abs(delta)

        ' Ancora, nu banda: dacă rândul curent e închis într-un grup strâns, pasul pleacă de la
        ' antetul acelui grup — adică sare peste tot grupul, exact ca ochiul.
        Dim curent As Integer = -1
        If _currentRowIndex >= 0 Then curent = AnchorBandOfRow(_currentRowIndex)
        ' Fără selecție (sau cu una tocmai ascunsă) se pleacă din afara capătului potrivit, ca
        ' primul pas să cadă chiar pe primul / ultimul rând de date.
        Dim i As Integer = If(curent >= 0, curent, If(pas > 0, -1, n))

        ' Un salt mai mare decât grila (Ctrl+Home / Ctrl+End) e chiar «du-te la capăt»: se merge
        ' direct acolo, ca o singură apăsare să nu plimbe o buclă prin sute de mii de benzi.
        If ramase >= n Then
            i = If(pas > 0, -1, n)
            ramase = 1
            Dim capat As Integer = EdgeDataBand(pas > 0)
            If capat >= 0 Then
                SetCurrentCell(ModelIndexAt(BandAt(capat).ViewPosition), _currentColumnKey)
            End If
            Return
        End If

        Dim ultimaData As Integer = -1
        While ramase > 0
            i += pas
            If i < 0 OrElse i >= n Then Exit While
            If BandAt(i).Kind <> KBotGroupBandKind.Data Then Continue While
            ultimaData = i
            ramase -= 1
        End While

        If ultimaData < 0 Then Return
        SetCurrentCell(ModelIndexAt(BandAt(ultimaData).ViewPosition), _currentColumnKey)
    End Sub

    ' Prima (sau ultima) bandă de DATE, sau -1 dacă grila n-are niciun rând desenat — se poate
    ' întâmpla cu tot ce e vizibil strâns, caz în care săgețile n-au unde să ducă.
    Private Function EdgeDataBand(prima As Boolean) As Integer
        Dim n As Integer = BandCount()
        If prima Then
            For i As Integer = 0 To n - 1
                If BandAt(i).Kind = KBotGroupBandKind.Data Then Return i
            Next
        Else
            For i As Integer = n - 1 To 0 Step -1
                If BandAt(i).Kind = KBotGroupBandKind.Data Then Return i
            Next
        End If
        Return -1
    End Function

    ' Mută coloana curentă în direcția dată, dacă există o coloană activă acolo.
    Private Sub MoveColumn(direction As Integer)
        Dim target As KBotDataColumn = NextEnabledColumn(_currentColumnKey, direction)
        If target Is Nothing Then Return
        ' Fără rând curent, coloana se mută pe PRIMUL rând DESENAT, nu pe modelul 0 — acela poate
        ' fi tocmai unul filtrat afară sau închis într-un grup strâns.
        Dim rand As Integer = _currentRowIndex
        If rand < 0 Then
            Dim capat As Integer = EdgeDataBand(True)
            If capat < 0 Then Return
            rand = ModelIndexAt(BandAt(capat).ViewPosition)
        End If
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
                    ' Ctrl+Stânga strânge grupul rândului curent — echivalentul de la tastatură al
                    ' apăsării pe antetul lui. Fără Ctrl rămâne mutarea de coloană, ca până acum:
                    ' săgețile simple sunt navigație prin celule și n-au voie să-și schimbe rostul
                    ' pentru că grila s-a întâmplat să fie grupată.
                    If ctrl Then
                        If _currentRowIndex >= 0 Then SetGroupCollapsedForRow(_currentRowIndex, True)
                    Else
                        MoveColumn(-1)
                    End If
                Case Keys.Right
                    If ctrl Then
                        If _currentRowIndex >= 0 Then SetGroupCollapsedForRow(_currentRowIndex, False)
                    Else
                        MoveColumn(1)
                    End If
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

#If DEBUG Then
            ' Sonda de lățimi (doar în Debug): click dreapta pe un antet spune cât are coloana.
            ' Vezi KBotDataView.WidthProbe.vb.
            If e.Button = MouseButtons.Right AndAlso HandleHeaderWidthProbe(e.Location) Then Return
#End If

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
                ' Lățimea PICTATĂ, nu cea logică: tragerea se măsoară în pixeli de ecran (`e.X`),
                ' deci reperul de pornire trebuie să fie în aceleași unități. Conversia înapoi la
                ' logic se face la scriere — vezi mai jos (felia 0035-01).
                _resizeStartWidth = resizeTarget.WidthPx
                Return
            End If

            ' 1b) O bandă de grup (slice 0029) — se strânge/desface și consumă apăsarea; nu e
            ' un rând, deci nu are ce selecta sub ea.
            If HandleGroupBandMouseDown(e.Location) Then Return

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
                ' Tragerea e în pixeli de ECRAN; `Width` e în pixeli LOGICI (ea e ce a cerut
                ' operatorul și ce s-ar serializa). Se scrie deci lățimea nescalată — altfel, la
                ' 150%, o coloană trasă la 300 px pe ecran s-ar ține minte ca 300 logici, adică
                ' 450 pe ecran la următoarea așezare, și ar sări de sub cursor.
                Dim nouaPx As Integer = _resizeStartWidth + (e.X - _resizeStartX)
                _resizingColumn.Width = UnscaleX(nouaPx)
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

            ' O bandă de grup (slice 0029): cursor de mână peste antetele care se pot strânge,
            ' și nicio etichetă de celulă — acolo nu e nicio celulă.
            Dim bandaGrup As Integer = BandAtPoint(e.Location)
            If bandaGrup >= 0 AndAlso BandAt(bandaGrup).Kind <> KBotGroupBandKind.Data Then
                CancelCellTooltip()
                Cursor = If(BandAt(bandaGrup).Kind = KBotGroupBandKind.GroupHeader AndAlso
                            GroupBandIsCollapsible(bandaGrup), Cursors.Hand, Cursors.Default)
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
        ' Banda EFECTIVĂ: cu un titlu pe mai multe linii ea e mai înaltă, iar marginea trebuie să
        ' se poată apuca pe toată înălțimea ei, nu doar pe primii 30 de pixeli.
        If Not _showHeader OrElse pt.Y >= HeaderBandHeight() Then Return Nothing
        Dim tol As Integer = ScaleDpi(4)

        For Each cl In _frozenLayout
            Dim edge As Integer = cl.X + cl.Column.WidthPx
            If Math.Abs(pt.X - edge) <= tol Then Return If(cl.Column.Resizable, cl.Column, Nothing)
        Next

        Dim hOffset As Integer = HScrollOffset()
        For Each cl In _scrollLayout
            Dim edge As Integer = _frozenBandWidth + cl.X + cl.Column.WidthPx - hOffset
            If Math.Abs(pt.X - edge) <= tol Then Return If(cl.Column.Resizable, cl.Column, Nothing)
        Next

        Return Nothing
    End Function

End Class
