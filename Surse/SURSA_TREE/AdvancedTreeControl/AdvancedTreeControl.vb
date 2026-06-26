Imports System.Linq

Partial Public Class AdvancedTreeControl
    Inherits ScrollableControl

    ' STARE INTERNĂ (STATE)
    Private pHoveredItem As TreeItem = Nothing
    Private pSelectedItem As TreeItem = Nothing
    Private pOldSelectedItem As TreeItem = Nothing

    Private pTooltipItem As TreeItem = Nothing
    Private pTooltipPopup As TooltipPopup = Nothing
    Private _lastMouseX As Integer = -1

    ' Timer pentru a diferenția Click de DoubleClick
    Private WithEvents ClickDelayTimer As New Timer()
    Private _pendingClickItem As TreeItem = Nothing
    Private _pendingMouseArgs As MouseEventArgs = Nothing

    ' Marginea globală din stânga a întregului arbore (să nu fie lipit de margine)
    Private Const PADDING_TREE_START As Integer = 10

    ' Marginea globală din stânga pentru zona de selecție (checkbox + text)
    Private Const PADDING_SELECTION_LEFT As Integer = 4

    ' Marginea globală din VÂRFUL arborelui (spațiu înainte de primul nod)
    Private Const PADDING_TREE_TOP As Integer = 5

    ' Marginea globală din DREAPTA a întregului arbore
    Private Const PADDING_TREE_END As Integer = 4

    ' Raza colțurilor pentru selecție și hover
    Private Const SELECTION_CORNER_RADIUS As Integer = 1

    ' SPAȚIUL DINTRE EXPANDER/LINIE ȘI CONȚINUT (Checkbox sau Icon)
    ' Mărește această valoare pentru a depărta bifa de liniile punctate!
    Private Const PADDING_EXPANDER_GAP As Integer = 12

    ' Spațiu dintre capătul liniei orizontale și conținut (Checkbox/Icon)
    ' Mărind această valoare, linia se oprește mai devreme față de conținut
    Private Const TREE_LINE_H_MARGIN As Integer = 4

    ' Spațiu între Checkbox și următorul element (Icon/Text)
    Private Const PADDING_CHECKBOX_GAP As Integer = 8

    ' Spațiu între Iconiță (stânga) și Text
    Private Const PADDING_ICON_GAP As Integer = 4

    ' Separator pentru comanda de procesare venita din VBA
    Private Shared ReadOnly separator As String() = New String() {"||"}

    ' Timer pentru animația de încărcare / Nod
    Private WithEvents LoadingTimer As New Timer() With {.Interval = 50} ' 20 FPS
    Private loadingAngle As Single = 0

    ' Gap minim între capătul textului stâng și începutul textului drept (separator ~~~)
    Private Const PADDING_SEPARATOR_GAP As Integer = 8

    Private _vScroll As New VScrollBar()

    Private ReadOnly TooltipTimer As New Timer()

    <System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet:=System.Runtime.InteropServices.CharSet.Unicode)>
    Private Shared Function SetWindowTheme(hWnd As IntPtr, pszSubAppName As String, pszSubIdList As String) As Integer
    End Function

    ' Proprietate publică - folosită de Tree.vb pentru whitelist în MonitorTimer
    Public ReadOnly Property TooltipPopupHandle As IntPtr
        Get
            If pTooltipPopup IsNot Nothing AndAlso Not pTooltipPopup.IsDisposed AndAlso pTooltipPopup.Visible Then
                Return pTooltipPopup.Handle
            End If
            Return IntPtr.Zero
        End Get
    End Property

    ' Padding aplicat în jurul zonei iconului stânga pentru hit-testul tooltip-ului
    ' (când TooltipShowOnlyOnLeftIcon = True)
    Private Const TOOLTIP_ICON_HIT_PADDING As Integer = 3

    ' ══════════════ HEADER ══════════════
    Private _headerSearchIconRect As Rectangle = Rectangle.Empty
    Private _headerRightIconRect As Rectangle = Rectangle.Empty

    ' ══════════════ TREE LIST VIEW ══════════════
    Private _treeListView As Boolean = False
    Private _columns As New List(Of ColumnDef)
    Private Const COLUMN_HEADER_HEIGHT As Integer = 24
    Private Const COLUMN_SEPARATOR_COLOR_ALPHA As Integer = 60
    Private Const MIN_CAPTION_WIDTH As Integer = 120    ' latimea minima garantata zonei caption
    Private _captionColumnEndX As Integer = 0   ' X unde se termina zona caption; actualizat in DrawContent

    ' ── TreeListView master switch + dynamic columns + column filter ──────────
    Private _baseColumns As New List(Of ColumnDef)           ' copie imutabilă din XML
    Private _colFilterActive As Boolean = False
    Private _colFilterSet As New HashSet(Of TreeItem)
    Private _activeColFilters As New Dictionary(Of String, String)  ' colName → filterText
    Private _activeColFilterPopup As Form = Nothing

    ' ══════════════ SEARCH ══════════════
    Private _isSearchMode As Boolean = False
    Private _searchResults As New List(Of SearchResultItem)()
    Private _searchResultHoveredIdx As Integer = -1
    Private _searchTextBox As TextBox = Nothing
    Private WithEvents SearchDebounceTimer As New Timer() With {.Interval = 300}

    Friend Structure SearchResultItem
        Public Item As TreeItem
        Public IsDimmed As Boolean
        Public Sub New(item As TreeItem, dimmed As Boolean)
            Me.Item = item
            Me.IsDimmed = dimmed
        End Sub
    End Structure

    Friend Structure RichTextPart
        Public Text As String
        Public Font As Font
        Public ForeColor As Color
        Public BackColor As Color
        Public HasBackColor As Boolean
    End Structure

    Public Enum En_Tree_SearchType
        SearchType_Contains = 0
        SearchType_StartsWith = 1
    End Enum

    Public Enum En_Tree_SearchIn
        SearchIn_Caption = 0
        SearchIn_Tag = 1
        SearchIn_Both = 2
    End Enum

    Public Enum En_Tree_SearchMode
        SearchMode_Tree = 0
        SearchMode_List = 1
    End Enum

    Public Enum En_ScrollBarTheme
        [Default] = 0
        Explorer = 1
        DarkMode = 2
    End Enum

    ' INIȚIALIZARE
    Public Sub New()
        Me.DoubleBuffered = True
        Me.AutoScroll = False
        Me.BackColor = Color.White
        Me.Cursor = Cursors.Default
        Me.Font = New Font("Segoe UI", 9)
        Me.Enabled = True

        ' ── VScrollBar manual — imun la layout engine ─────────────────────
        _vScroll.Minimum = 0
        _vScroll.Maximum = 0
        _vScroll.SmallChange = ItemHeight
        _vScroll.LargeChange = 1
        _vScroll.Visible = False
        _vScroll.Width = SystemInformation.VerticalScrollBarWidth
        _vScroll.Left = Math.Max(0, Me.Width - _vScroll.Width)
        _vScroll.Top = 0
        _vScroll.Height = Me.Height
        AddHandler _vScroll.Scroll, AddressOf OnVScrollScroll
        Me.Controls.Add(_vScroll)

        AddHandler _vScroll.HandleCreated, Sub(s, e) ApplyScrollBarTheme()

        TooltipTimer.Interval = TooltipDelayMs
        AddHandler TooltipTimer.Tick, AddressOf TooltipTimerTick

        RecalculateItemHeight()

        ClickDelayTimer.Interval = 50

        pTooltipPopup = New TooltipPopup()
        Dim _forceHandle = pTooltipPopup.Handle

    End Sub

    Private Sub OnClickDelayTimerTick(sender As Object, e As EventArgs) Handles ClickDelayTimer.Tick
        ClickDelayTimer.Stop()

        If _pendingClickItem IsNot Nothing AndAlso _pendingMouseArgs IsNot Nothing Then
            RaiseEvent NodeMouseUp(_pendingClickItem, _pendingMouseArgs)
            pOldSelectedItem = pSelectedItem
        End If

        _pendingClickItem = Nothing
        _pendingMouseArgs = Nothing
    End Sub

    Private Sub RecalculateItemHeight()
        ' Dacă utilizatorul a setat manual înălțimea, NU mai recalculăm
        If Not _autoHeight Then Return

        ' Înălțimea = Maximul dintre Font și Iconițe + Padding
        Dim hFont As Integer = CInt(Me.Font.Height)
        Dim hIcon As Integer = Math.Max(_leftIconSize.Height, _rightIconSize.Height)
        Dim hMax As Integer = Math.Max(hFont, hIcon)

        _itemHeight = hMax + 6
        Me.Invalidate()
    End Sub

    ' Opțional: Adaugă o metodă pentru a reveni la Auto
    Public Sub SetAutoHeight()
        _autoHeight = True
        RecalculateItemHeight()
    End Sub

    Private Function HitTestItem(p As Point) As TreeItem
        Dim headerOff As Integer = TotalHeaderOffset

        If p.Y < headerOff Then Return Nothing

        Dim yRel = p.Y - Me.AutoScrollPosition.Y - PADDING_TREE_TOP - headerOff
        Dim idx As Integer = yRel \ ItemHeight
        Dim visible = GetVisibleItems()
        If idx < 0 OrElse idx >= visible.Count Then Return Nothing
        Return visible(idx)
    End Function

    Private Function GetCheckBoxRect(it As TreeItem) As Rectangle
        If Not NodeHasCheckControl(it) Then Return Rectangle.Empty

        Dim y As Integer = GetItemY(it)
        If y = -1 Then Return Rectangle.Empty

        Dim gridLeft As Integer = (it.Level * Indent) + Me.AutoScrollPosition.X + PADDING_TREE_START

        ' Level=0 fără RootExpander → checkbox direct de la gridLeft (fără Indent/Expander gap)
        Dim xChk As Integer
        If it.Level = 0 AndAlso Not _RootExpander Then
            xChk = gridLeft
        Else
            xChk = gridLeft + Indent + PADDING_EXPANDER_GAP
        End If

        Dim midY As Integer = y + (ItemHeight \ 2)
        Dim chkSize As Integer = _checkBoxSize

        Return New Rectangle(xChk, midY - (chkSize \ 2), chkSize, chkSize)
    End Function

    Private Function GetExpanderRect(it As TreeItem) As Rectangle
        Dim y As Integer = GetItemY(it)
        If y = -1 Then Return Rectangle.Empty

        ' --- ACTUALIZARE LOGICĂ POZIȚIONARE ---
        ' 1. Punctul de start al grilei
        Dim gridLeft As Integer = (it.Level * Indent) + Me.AutoScrollPosition.X + PADDING_TREE_START

        ' 2. Centrul expanderului este la jumătatea indentării curente
        Dim cx As Integer = gridLeft + (Indent \ 2)
        Dim cy As Integer = y + (ItemHeight \ 2)

        Return New Rectangle(cx - (ExpanderSize \ 2), cy - (ExpanderSize \ 2), ExpanderSize, ExpanderSize)
    End Function

    ''' <summary>
    ''' Offset vertical total al zonei de noduri fata de top-ul controlului.
    ''' Include: header principal + search bar + header coloane (TreeListView).
    ''' Apelat din HitTestItem, GetItemY, OnPaint.
    ''' </summary>
    Private ReadOnly Property TotalHeaderOffset As Integer
        Get
            Dim columnHdrH As Integer = If(_treeListViewEnabled AndAlso _treeListView AndAlso GetVisibleColumnCount() > 0,
                                           COLUMN_HEADER_HEIGHT, 0)
            Return If(_headerVisible, _headerHeight, 0) +
                   If(_isSearchMode, _searchBarHeight, 0) +
                   columnHdrH
        End Get
    End Property

    ''' <summary>Configureaza modul TreeListView. Apelata din Tree.Builder dupa parsarea XML.</summary>
    Friend Sub SetTreeListView(active As Boolean, cols As List(Of ColumnDef))
        Try
            _treeListView = active
            _columns = cols
            _baseColumns = New List(Of ColumnDef)(cols)   ' shallow copy — ColumnDef nu e mutat după creare
            Me.Invalidate()
        Catch ex As Exception
            TreeLogger.Ex(ex, "SetTreeListView")
        End Try
    End Sub

    ''' <summary>
    ''' Returneaza indexul 0-based al coloanei la coordonata X data.
    ''' Returneaza -1 daca X este in zona caption sau in afara.
    ''' </summary>
    Private Function GetColumnAtX(x As Integer) As Integer
        Return GetColumnAtX(x, _columns, GetColumnStartX(_columns))
    End Function

    Private Function GetColumnAtX(x As Integer, cols As List(Of ColumnDef)) As Integer
        Return GetColumnAtX(x, cols, GetColumnStartX(cols))
    End Function

    Private Function GetColumnAtX(x As Integer, cols As List(Of ColumnDef), colStartX As Integer) As Integer
        Try
            If Not _treeListViewEnabled OrElse Not _treeListView Then Return -1
            Dim available As Integer = Me.Width - ScrollBarWidth - PADDING_TREE_END - colStartX
            Dim visCols As Integer = GetVisibleColumnCount(cols, available)
            If visCols = 0 Then Return -1
            Dim cx As Integer = colStartX
            For i As Integer = 0 To visCols - 1
                Dim right As Integer = cx + cols(i).Width
                If x >= cx AndAlso x < right Then Return i
                cx = right
            Next
            Return -1
        Catch
            Return -1
        End Try
    End Function

    ''' <summary>
    ''' Numarul de coloane vizibile: prefixul din _columns care incape pastrand
    ''' minim MIN_CAPTION_WIDTH px pentru zona caption (colStartX >= MIN_CAPTION_WIDTH).
    ''' Coloanele se ascund de la coada (ultima definita dispare prima).
    ''' 0 = nu incape nicio coloana — dispare si banda de header de coloane.
    ''' </summary>
    Friend Function GetVisibleColumnCount() As Integer
        Return GetVisibleColumnCount(_columns)
    End Function

    Friend Function GetVisibleColumnCount(cols As List(Of ColumnDef)) As Integer
        Dim available As Integer = Me.Width - ScrollBarWidth - PADDING_TREE_END - MIN_CAPTION_WIDTH
        Return GetVisibleColumnCount(cols, available)
    End Function

    Friend Function GetVisibleColumnCount(cols As List(Of ColumnDef), available As Integer) As Integer
        Try
            If cols Is Nothing OrElse cols.Count = 0 Then Return 0
            If available <= 0 Then Return 0
            Dim cum As Integer = 0
            Dim n As Integer = 0
            For Each cd In cols
                cum += cd.Width
                If cum > available Then Exit For
                n += 1
            Next
            Return n
        Catch ex As Exception
            TreeLogger.Ex(ex, "GetVisibleColumnCount")
            Return If(cols IsNot Nothing, cols.Count, 0)
        End Try
    End Function

    Private Function GetColumnStartX(cols As List(Of ColumnDef)) As Integer
        Dim totalColsW As Integer = 0
        Dim visCols As Integer = GetVisibleColumnCount(cols)
        For i As Integer = 0 To visCols - 1
            totalColsW += cols(i).Width
        Next
        Return Me.Width - ScrollBarWidth - PADDING_TREE_END - totalColsW
    End Function

    Private Function GetContentStartX(it As TreeItem) As Integer
        If it Is Nothing Then Return PADDING_TREE_START

        Dim gridLeft As Integer = (it.Level * Indent) + Me.AutoScrollPosition.X + PADDING_TREE_START
        Dim xBase As Integer = If(it.Level = 0 AndAlso Not _RootExpander,
                                  gridLeft,
                                  gridLeft + Indent + PADDING_EXPANDER_GAP)

        If NodeHasCheckControl(it) Then
            xBase += _checkBoxSize + PADDING_CHECKBOX_GAP
        End If

        If _hasNodeIcons AndAlso (it.LeftIconClosed IsNot Nothing OrElse it.LeftIconOpen IsNot Nothing) Then
            xBase += LeftIconSize.Width + PADDING_ICON_GAP
        End If

        Return xBase
    End Function

    Private Function GetItemY(it As TreeItem) As Integer
        Dim idx = GetVisibleItems().IndexOf(it)
        If idx < 0 Then Return -1
        Return Me.AutoScrollPosition.Y + PADDING_TREE_TOP + TotalHeaderOffset + idx * ItemHeight
    End Function

    ' Găsește ancestorul de pe RadioButtonLevel al unui nod
    Private Function GetRadioAncestor(it As TreeItem) As TreeItem
        Dim current As TreeItem = it.Parent
        While current IsNot Nothing
            If current.Level = _radioButtonLevel Then Return current
            current = current.Parent
        End While
        Return Nothing
    End Function

    ' Determină dacă un nod trebuie să aibă checkbox/radio desenat și activ
    Private Function NodeHasCheckControl(it As TreeItem) As Boolean
        If _radioButtonLevel >= 0 Then
            If it.Level < _radioButtonLevel Then Return False                    ' deasupra: niciodată
            If it.Level = _radioButtonLevel Then Return True                     ' nivelul radio: întotdeauna
            ' sub nivel radio: doar dacă ancestorul radio e selectat
            Dim radioAnc As TreeItem = GetRadioAncestor(it)
            Return radioAnc IsNot Nothing AndAlso radioAnc.IsRadioSelected
        Else
            Return _checkBoxes And it.HasCheckBox                                               ' mod normal
        End If
    End Function

    ' Returnează lista plată a nodurilor vizibile (ținând cont de expandare)
    Private Function GetVisibleItems() As List(Of TreeItem)
        Dim result As New List(Of TreeItem)
        For Each it In Items
            AddVisible(it, result)
        Next
        Return result
    End Function

    Private Sub AddVisible(it As TreeItem, list As List(Of TreeItem))
        ' AND logic: nodul trebuie să treacă AMBELE filtre (dacă sunt active)
        Dim passSearch As Boolean = Not _filterActive OrElse _filterSet.Contains(it)
        Dim passColFilter As Boolean = Not _colFilterActive OrElse _colFilterSet.Contains(it)
        If Not passSearch OrElse Not passColFilter Then Return
        list.Add(it)
        If _filterActive OrElse _colFilterActive Then
            ' Orice filtru activ → force-expand pentru a expune nodurile relevante
            For Each c In it.Children
                AddVisible(c, list)
            Next
        Else
            If it.Expanded Then
                For Each c In it.Children
                    AddVisible(c, list)
                Next
            End If
        End If
    End Sub

    ' ======================================================
    ' TOOLTIP LOGIC
    ' ======================================================
    Private Sub HideAllTooltips()
        If pTooltipPopup IsNot Nothing AndAlso Not pTooltipPopup.IsDisposed Then
            pTooltipPopup.Hide()
        End If
    End Sub

    Private Sub ResetTooltip(it As TreeItem, Optional mouseX As Integer = -1)
        HideAllTooltips()
        TooltipTimer.Stop()
        pTooltipItem = Nothing

        If Not TooltipShow Then Return
        If it Is Nothing Then Return

        ' Nu afișăm tooltip dacă mouse-ul e pe zona RightIcon
        If it.RightIcon IsNot Nothing AndAlso mouseX >= 0 Then
            Dim scrollW As Integer = ScrollBarWidth 'If(Me.VerticalScroll.Visible, SystemInformation.VerticalScrollBarWidth, 0)
            Dim rightIconMinX As Integer = Me.Width - RightIconSize.Width - _rightIconRightPadding - scrollW
            If mouseX >= rightIconMinX Then Return
        End If

        ' TooltipShowOnlyOnLeftIcon: verificare zonă icon stânga
        If _tooltipShowOnlyOnLeftIcon AndAlso mouseX >= 0 Then
            Dim iconRect As Rectangle = GetLeftIconRect(it)
            If iconRect = Rectangle.Empty Then
                ' Nodul nu are icon stânga → fallback la comportament normal, continuăm
            Else
                ' Extindem zona cu padding pentru un hit-test mai generos
                Dim hitRect As New Rectangle(
                    iconRect.X - TOOLTIP_ICON_HIT_PADDING,
                    iconRect.Y - TOOLTIP_ICON_HIT_PADDING,
                    iconRect.Width + TOOLTIP_ICON_HIT_PADDING * 2,
                    iconRect.Height + TOOLTIP_ICON_HIT_PADDING * 2)
                ' Verificăm doar X (Y-ul îl garantează HitTest că suntem pe rândul corect)
                If mouseX < hitRect.Left OrElse mouseX > hitRect.Right Then Return
            End If
        End If

        ' Dacă are Tooltip custom → afișăm ÎNTOTDEAUNA (ignorăm TextFits)
        ' Dacă NU are Tooltip → afișăm doar dacă textul nu încape (comportamentul vechi)
        If String.IsNullOrEmpty(it.Tooltip) Then
            If TextFits(it) Then Return
        End If

        pTooltipItem = it
        TooltipTimer.Start()
    End Sub

    Private Sub TooltipTimerTick(sender As Object, e As EventArgs)
        TooltipTimer.Stop()
        If pTooltipItem Is Nothing OrElse pTooltipItem IsNot pHoveredItem Then Return

        ' Verificare suplimentară: dacă cursorul s-a mutat pe RightIcon între timp
        If pTooltipItem.RightIcon IsNot Nothing Then
            Dim scrollW As Integer = ScrollBarWidth 'If(_vScroll.Visible, _vScroll.Width, 0)
            Dim rightIconMinX As Integer = Me.Width - RightIconSize.Width - _rightIconRightPadding - scrollW
            If _lastMouseX >= rightIconMinX Then Return
        End If

        ' Verificare suplimentară: TooltipShowOnlyOnLeftIcon — dacă între timp
        ' cursorul a ieșit din zona iconului, anulăm afișarea
        If _tooltipShowOnlyOnLeftIcon Then
            Dim iconRect As Rectangle = GetLeftIconRect(pTooltipItem)
            If iconRect <> Rectangle.Empty Then
                Dim hitRect As New Rectangle(
                    iconRect.X - TOOLTIP_ICON_HIT_PADDING,
                    iconRect.Y - TOOLTIP_ICON_HIT_PADDING,
                    iconRect.Width + TOOLTIP_ICON_HIT_PADDING * 2,
                    iconRect.Height + TOOLTIP_ICON_HIT_PADDING * 2)
                If _lastMouseX < hitRect.Left OrElse _lastMouseX > hitRect.Right Then Return
            End If
        End If

        Try
            Dim screenPt As Point = Cursor.Position

            If pTooltipPopup.IsDisposed Then
                pTooltipPopup = New TooltipPopup()
                Dim _forceHandle = pTooltipPopup.Handle
            End If

            pTooltipPopup.TT_BackColor = TooltipBackColor
            pTooltipPopup.TT_ForeColor = TooltipForeColor
            pTooltipPopup.ShowTooltip(pTooltipItem.Tooltip, Me.Font, screenPt, AutoHideTooltipMs)

        Catch ex As Exception
            TreeLogger.Ex(ex, "TooltipTimerTick")
        End Try
    End Sub

    Private Function TextFits(it As TreeItem) As Boolean
        Using g As Graphics = Me.CreateGraphics()
            Dim textSize = g.MeasureString(it.Caption, Me.Font)

            ' 1. Calculăm punctul de start al grilei (Sincronizat cu DrawItem / Helpers)
            Dim gridLeft As Integer = (it.Level * Indent) + Me.AutoScrollPosition.X + PADDING_TREE_START

            ' 2. Calculăm poziția curentă X (cursorul virtual de desenare)
            '    Pornim de la zona de după Expander
            ' Level=0 fără RootExpander → pornim direct de la gridLeft
            Dim currentX As Integer
            If it.Level = 0 AndAlso Not _RootExpander Then
                currentX = gridLeft
            Else
                currentX = gridLeft + Indent + PADDING_EXPANDER_GAP
            End If

            ' 3. Adăugăm lățimea Checkbox-ului + Spațiul de după el (dacă e activ)
            If NodeHasCheckControl(it) Then
                currentX += _checkBoxSize + PADDING_CHECKBOX_GAP
            End If

            ' 4. Adăugăm lățimea Iconiței din stânga + Spațiul de după ea
            '    Verificăm dacă există iconiță (Closed sau Open, dimensiunea e dată de LeftIconSize)
            If it.LeftIconClosed IsNot Nothing OrElse it.LeftIconOpen IsNot Nothing Then
                currentX += LeftIconSize.Width + PADDING_ICON_GAP
            End If

            ' 5. Adăugăm lățimea Textului pentru a afla punctul final
            Dim endX As Integer = currentX + CInt(textSize.Width)

            ' 6. Calculăm limita vizibilă a ferestrei
            '    Scădem zona rezervată iconiței din dreapta și o marjă de siguranță (20px)
            Dim visibleWidth As Integer = Me.Width - RightIconSize.Width - 20

            '    Scădem și lățimea barei de scroll vertical dacă este vizibilă
            If Me.VerticalScroll.Visible Then visibleWidth -= SystemInformation.VerticalScrollBarWidth

            ' Verificăm dacă textul încape
            Return endX <= visibleWidth
        End Using
    End Function

    ''' <summary>
    ''' Calculează dreptunghiul iconului stânga pentru nodul dat, identic cu DrawContent.
    ''' Returnează Rectangle.Empty dacă nodul nu are icon stânga sau HasNodeIcons = False.
    ''' </summary>
    Private Function GetLeftIconRect(it As TreeItem) As Rectangle
        If Not _hasNodeIcons Then Return Rectangle.Empty
        If it.LeftIconClosed Is Nothing AndAlso it.LeftIconOpen Is Nothing Then Return Rectangle.Empty

        Dim y As Integer = GetItemY(it)
        If y < 0 Then Return Rectangle.Empty

        Dim gridLeft As Integer = (it.Level * Indent) + Me.AutoScrollPosition.X + PADDING_TREE_START
        Dim xBase As Integer = If(it.Level = 0 AndAlso Not _RootExpander,
                                  gridLeft,
                                  gridLeft + Indent + PADDING_EXPANDER_GAP)

        ' Checkbox deplasează xBase
        If NodeHasCheckControl(it) Then
            xBase += _checkBoxSize + PADDING_CHECKBOX_GAP
        End If

        Return New Rectangle(xBase,
                             y + (ItemHeight - LeftIconSize.Height) \ 2,
                             LeftIconSize.Width,
                             LeftIconSize.Height)
    End Function

    ' Setează starea unui nod, a copiilor săi și actualizează părinții
    Private Shared Sub SetNodeStateWithPropagation(node As TreeItem, newState As TreeCheckState)
        ' 1. Setează starea nodului curent
        node.CheckState = newState

        ' 2. Propagă în jos (toți copiii iau aceeași stare)
        SetChildrenStateRecursive(node, newState)

        ' 3. Propagă în sus (părinții își recalculează starea)
        UpdateParentStateRecursive(node.Parent)
    End Sub

    ' Setează recursiv toți descendenții la o anumită stare
    Private Shared Sub SetChildrenStateRecursive(node As TreeItem, state As TreeCheckState)
        For Each child In node.Children
            child.CheckState = state
            SetChildrenStateRecursive(child, state)
        Next
    End Sub

    ' Verifică starea fraților și actualizează părintele
    Private Shared Sub UpdateParentStateRecursive(parent As TreeItem)
        If parent Is Nothing Then Return

        Dim anyChecked As Boolean = False
        Dim anyUnchecked As Boolean = False
        Dim anyIndeterminate As Boolean = False

        For Each child In parent.Children
            Select Case child.CheckState
                Case TreeCheckState.Checked
                    anyChecked = True
                Case TreeCheckState.Unchecked
                    anyUnchecked = True
                Case TreeCheckState.Indeterminate
                    anyIndeterminate = True
            End Select
        Next

        ' Reguli pentru starea părintelui:
        If anyIndeterminate Then
            parent.CheckState = TreeCheckState.Indeterminate
        ElseIf anyChecked AndAlso anyUnchecked Then
            parent.CheckState = TreeCheckState.Indeterminate ' Mixt -> Nedefinit
        ElseIf anyChecked Then
            parent.CheckState = TreeCheckState.Checked       ' Toți bifati
        Else
            parent.CheckState = TreeCheckState.Unchecked     ' Nimeni bifat
        End If

        ' Continuăm urcarea spre rădăcină
        UpdateParentStateRecursive(parent.Parent)
    End Sub

    ' Funcție recursivă pentru a găsi un nod după ID
    Private Function FindNodeByID(id As String) As TreeItem
        Return FindNodeRecursive(Me.Items, id)
    End Function

    Private Shared Function FindNodeRecursive(collection As List(Of TreeItem), id As String) As TreeItem
        For Each it As TreeItem In collection
            ' Verificăm ID-ul (care corespunde cu Key din VBA)
            ' Asigură-te că ai proprietatea ID definită în TreeItem (sau folosește _tag dacă acolo ții ID-ul)
            ' Presupunând că ai: Public ID As String
            If it.Key = id Then
                Return it
            End If

            ' Căutare în adâncime
            Dim foundChild = FindNodeRecursive(it.Children, id)
            If foundChild IsNot Nothing Then Return foundChild
        Next
        Return Nothing
    End Function

    ' Funcție pentru a converti orice obiect (Boolean, Color, Enum) în String pentru VBA
    Private Shared Function FormatValue(val As Object) As String
        If val Is Nothing Then Return ""

        If TypeOf val Is Boolean Then
            ' Returnăm "True"/"False" sau "-1"/"0" cum preferă VBA
            Return If(DirectCast(val, Boolean), "True", "False")

        ElseIf TypeOf val Is Color Then
            ' Pentru culori, returnăm codul ARGB sau numele
            Return DirectCast(val, Color).Name

        ElseIf TypeOf val Is [Enum] Then
            ' Pentru Enum-uri (ex: CheckState), returnăm valoarea numerică (0, 1, 2)
            Return CInt(val).ToString()

        Else
            ' Pentru String, Integer, etc.
            Return val.ToString()
        End If
    End Function

    ' Resetează recursiv checkboxurile tuturor descendenților unui nod
    Private Sub ClearChildrenCheckboxes(node As TreeItem)
        For Each child In node.Children
            child.CheckState = TreeCheckState.Unchecked
            ClearChildrenCheckboxes(child)
        Next
    End Sub

    ' Bifează recursiv toți descendenții unui nod
    Private Sub CheckChildrenRecursive(node As TreeItem)
        For Each child In node.Children
            child.CheckState = TreeCheckState.Checked
            CheckChildrenRecursive(child)
        Next
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' VSCROLL — shadow properties pentru compatibilitate Keyboard.vb + Tree.Helpers.vb
    ' ══════════════════════════════════════════════════════════════════

    ' WinForms convention: getter returnează Y negativ, setter primește Y pozitiv
    Public Shadows Property AutoScrollPosition As Point
        Get
            Return New Point(0, -_vScroll.Value)
        End Get
        Set(value As Point)
            Dim clamped As Integer = Math.Max(0,
            Math.Min(value.Y, Math.Max(0, _vScroll.Maximum - _vScroll.LargeChange + 1)))
            If _vScroll.Value <> clamped Then
                _vScroll.Value = clamped
                Me.Invalidate()
            End If
        End Set
    End Property

    Public Shadows Property AutoScrollMinSize As Size
        Get
            Return New Size(0, _vScroll.Maximum)
        End Get
        Set(value As Size)
            UpdateVScrollMaximum(value.Height)
        End Set
    End Property

    Private Sub UpdateVScrollMaximum(contentHeight As Integer)
        Dim headerOff As Integer = If(_headerVisible, _headerHeight, 0) +
                               If(_isSearchMode, _searchBarHeight, 0)
        Dim viewport As Integer = Math.Max(1, Me.Height - headerOff)
        _vScroll.LargeChange = viewport
        _vScroll.SmallChange = ItemHeight

        If contentHeight <= viewport Then
            If _vScroll.Visible Then
                _vScroll.Value = 0
                _vScroll.Visible = False
                If _isSearchMode Then PositionSearchTextBox()
            End If
        Else
            _vScroll.Maximum = contentHeight + viewport - 1   ' WinForms: Value max = Maximum - LargeChange + 1 = contentHeight
            If _vScroll.Value > contentHeight - viewport Then
                _vScroll.Value = Math.Max(0, contentHeight - viewport)
            End If
            If Not _vScroll.Visible Then
                _vScroll.Visible = True
                If _isSearchMode Then PositionSearchTextBox()
            End If
        End If

        ' Poziție fizică scrollbar — manual, fără Dock
        '_vScroll.Left = Me.Width - SystemInformation.VerticalScrollBarWidth
        '_vScroll.Top = 0
        '_vScroll.Width = SystemInformation.VerticalScrollBarWidth
        '_vScroll.Height = Me.Height
    End Sub

    ' Apelat din OnResize și din API (ClearTree, după rebuild)
    Friend Sub RefreshScrollVisibility()
        Dim headerOff As Integer = If(_headerVisible, _headerHeight, 0) +
                               If(_isSearchMode, _searchBarHeight, 0)
        Dim viewport As Integer = Math.Max(1, Me.Height - headerOff)
        Dim contentH As Integer = GetVisibleItems().Count * ItemHeight + PADDING_TREE_TOP

        _vScroll.Width = SystemInformation.VerticalScrollBarWidth
        _vScroll.Left = Math.Max(0, Me.Width - _vScroll.Width)
        _vScroll.Top = headerOff
        _vScroll.Height = Math.Max(1, Me.Height - headerOff)
        _vScroll.SmallChange = ItemHeight
        _vScroll.LargeChange = viewport

        If contentH > viewport Then
            _vScroll.Maximum = Math.Max(viewport, contentH - 1)
            Dim maxVal As Integer = Math.Max(0, contentH - viewport)
            If _vScroll.Value > maxVal Then _vScroll.Value = maxVal
            _vScroll.Visible = True
        Else
            _vScroll.Value = 0
            _vScroll.Visible = False
        End If

        Me.Invalidate()                           ' ← garantează repaint curat după orice schimbare
    End Sub
    Private Sub ApplyScrollBarTheme()
        If _vScroll Is Nothing OrElse Not _vScroll.IsHandleCreated Then Return
        Select Case _scrollBarTheme
            Case En_ScrollBarTheme.Explorer
                Dim v = SetWindowTheme(_vScroll.Handle, "Explorer", Nothing)
            Case En_ScrollBarTheme.DarkMode
                Dim unused = SetWindowTheme(_vScroll.Handle, "DarkMode_Explorer", Nothing)
            Case En_ScrollBarTheme.Default
                Dim unused1 = SetWindowTheme(_vScroll.Handle, "", Nothing)
        End Select
    End Sub

    Private Sub OnVScrollScroll(sender As Object, e As ScrollEventArgs)
        Me.Invalidate()
    End Sub

    Private Sub LoadingTimer_Tick(sender As Object, e As EventArgs) Handles LoadingTimer.Tick
        loadingAngle += 15
        If loadingAngle >= 360 Then loadingAngle = 0

        ' Invalidăm doar zona vizibilă pentru a redesena animația
        ' Optimizare: Am putea invalida doar nodurile loader, dar Invalidate() e suficient pentru început
        Me.Invalidate()
    End Sub

    ' ════════════════════════════════════════════════════════════════════
    ' COLUMN FILTER — CORE
    ' ════════════════════════════════════════════════════════════════════
    Friend Sub ApplyColumnFilters()
        Try
            _colFilterSet.Clear()
            _colFilterActive = _activeColFilters.Count > 0
            If Not _colFilterActive Then
                Me.Invalidate()
                Return
            End If
            Dim matchSet As New HashSet(Of TreeItem)()
            CollectColFilterMatches(Items, matchSet)
            For Each node In matchSet
                _colFilterSet.Add(node)
                Dim p = node.Parent
                While p IsNot Nothing
                    _colFilterSet.Add(p)
                    p = p.Parent
                End While
            Next
            _vScroll.Value = 0
            Me.BeginInvoke(New Action(AddressOf RefreshScrollVisibility))
            Me.Invalidate()

        Catch ex As Exception
            TreeLogger.Ex(ex, "ApplyColumnFilters")
        End Try
    End Sub

    Private Sub CollectColFilterMatches(nodes As List(Of TreeItem), result As HashSet(Of TreeItem))
        For Each it In nodes
            If RowHasColumns(it) AndAlso NodeMatchesColFilters(it) Then result.Add(it)
            CollectColFilterMatches(it.Children, result)
        Next
    End Sub

    Private Function NodeMatchesColFilters(it As TreeItem) As Boolean
        For Each kvp In _activeColFilters
            Dim cellData As TreeItem.CellData = Nothing
            it.Cells.TryGetValue(kvp.Key, cellData)
            Dim cellVal As String = If(cellData IsNot Nothing, cellData.Value, "").ToLowerInvariant()
            If Not cellVal.Contains(kvp.Value, StringComparison.InvariantCultureIgnoreCase) Then Return False
        Next
        Return True
    End Function

    ' ════════════════════════════════════════════════════════════════════
    ' COLUMN FILTER — GEOMETRY HELPERS
    ' ════════════════════════════════════════════════════════════════════
    Friend Function GetColumnRect(colIdx As Integer) As Rectangle
        Try
            If colIdx < 0 OrElse colIdx >= GetVisibleColumnCount() Then Return Rectangle.Empty
            Dim hdrOff As Integer = If(_headerVisible, _headerHeight, 0) +
                                    If(_isSearchMode, _searchBarHeight, 0)
            Dim cx As Integer = GetColumnStartX(_columns)
            For i As Integer = 0 To colIdx - 1
                cx += _columns(i).Width
            Next
            Return New Rectangle(cx, hdrOff, _columns(colIdx).Width, COLUMN_HEADER_HEIGHT)
        Catch
            Return Rectangle.Empty
        End Try
    End Function

    Friend Function GetColFilterIndicatorRect(colIdx As Integer) As Rectangle
        Try
            If colIdx < 0 OrElse colIdx >= _columns.Count Then Return Rectangle.Empty
            If Not _activeColFilters.ContainsKey(_columns(colIdx).Name) Then Return Rectangle.Empty
            Dim colRect = GetColumnRect(colIdx)
            If colRect.IsEmpty Then Return Rectangle.Empty
            Return New Rectangle(colRect.Right - 13,
                                 colRect.Top + (COLUMN_HEADER_HEIGHT - 8) \ 2,
                                 8, 8)
        Catch
            Return Rectangle.Empty
        End Try
    End Function

    Friend Function GetDistinctColumnValues(colName As String) As List(Of String)
        Dim result As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        CollectDistinctValues(Items, colName, result)
        Return result.OrderBy(Function(s) s).ToList()
    End Function

    Private Sub CollectDistinctValues(nodes As List(Of TreeItem), colName As String,
                                       result As HashSet(Of String))
        For Each it In nodes
            Dim cellData As TreeItem.CellData = Nothing
            If RowHasColumns(it) AndAlso
               it.Cells.TryGetValue(colName, cellData) AndAlso
               cellData IsNot Nothing AndAlso
               Not String.IsNullOrEmpty(cellData.Value) Then
                result.Add(cellData.Value)
            End If
            CollectDistinctValues(it.Children, colName, result)
        Next
    End Sub

    ' ════════════════════════════════════════════════════════════════════
    ' DYNAMIC COLUMNS — ColHeaderText per nod
    ' ════════════════════════════════════════════════════════════════════
    ' ════════════════════════════════════════════════════════════════════
    ' DYNAMIC COLUMNS — ColHeaderText per nod (cu span pe grila generica)
    ' Token: Header[~span[~align[~type]]] ; coloane separate prin "|".
    ' Span = cate sloturi fizice (_baseColumns) acopera coloana logica.
    ' Width-ul rezulta din suma latimilor sloturilor acoperite.
    ' Token fara "~" -> mod legacy: match pe Name in _baseColumns.
    ' ════════════════════════════════════════════════════════════════════
    Friend Sub ApplyDynamicColumns(it As TreeItem)
        If Not _treeListViewEnabled Then Return
        Try
            _activeColFilters.Clear()
            _colFilterActive = False
            _colFilterSet.Clear()
            _activeColFilterPopup?.Close()
            _activeColFilterPopup = Nothing

            Dim source As TreeItem = it
            Dim colHeaderText As String = ""
            While source IsNot Nothing
                If Not String.IsNullOrEmpty(source.ColHeaderText) Then
                    colHeaderText = source.ColHeaderText
                    Exit While
                End If
                source = source.Parent
            End While

            If String.IsNullOrEmpty(colHeaderText) Then
                _columns.Clear()
                _treeListView = False
            Else
                _columns = ResolveBandColumns(colHeaderText)
                _treeListView = (_columns.Count > 0)
            End If

            Me.Invalidate()
        Catch ex As Exception
            TreeLogger.Ex(ex, "ApplyDynamicColumns")
        End Try
    End Sub

    ' Acelasi parser ca in ApplyDynamicColumns, dar intoarce lista (reutilizat si per-rand).
    Friend Function ResolveBandColumns(colHeaderText As String) As List(Of ColumnDef)
        Dim newCols As New List(Of ColumnDef)()
        If String.IsNullOrEmpty(colHeaderText) Then Return newCols

        Dim labels() As String = colHeaderText.Split("|"c)
        Dim slotIdx As Integer = 0

        For Each rawLbl In labels
            Dim tok As String = rawLbl.Trim()
            If String.IsNullOrEmpty(tok) Then Continue For

            ' ── MOD LEGACY (fara "~"): match pe Name ──────────────────────────────
            If tok.IndexOf("~"c) < 0 Then
                Dim baseDef As ColumnDef = _baseColumns.FirstOrDefault(Function(c) c.Name = tok)
                Dim cdL As New ColumnDef With {.Name = tok, .Header = tok}
                If baseDef.Name IsNot Nothing AndAlso baseDef.Name <> "" Then
                    cdL.Width = baseDef.Width
                    cdL.ColType = baseDef.ColType
                    cdL.Align = baseDef.Align
                    cdL.Format = baseDef.Format
                    cdL.HeaderBackColor = baseDef.HeaderBackColor
                    cdL.HeaderForeColor = baseDef.HeaderForeColor
                    cdL.HeaderBold = baseDef.HeaderBold
                    cdL.HeaderItalic = baseDef.HeaderItalic
                    cdL.HeaderUnderline = baseDef.HeaderUnderline
                    cdL.HeaderAlign = baseDef.HeaderAlign
                Else
                    cdL.Width = 100
                    cdL.ColType = En_ColType.ColType_Text
                    cdL.Align = En_ColAlign.ColAlign_Left
                    cdL.HeaderBackColor = Color.Empty
                    cdL.HeaderForeColor = Color.Empty
                    cdL.HeaderAlign = En_ColAlign.ColAlign_Inherit
                End If
                newCols.Add(cdL)
                slotIdx += 1
                Continue For
            End If

            ' ── MOD NOU (cu "~"): pozitional + span ───────────────────────────────
            Dim parts() As String = tok.Split("~"c)
            Dim hdr As String = parts(0).Trim()
            If String.IsNullOrEmpty(hdr) Then Continue For

            Dim span As Integer = 1
            If parts.Length > 1 Then Dim v = Integer.TryParse(parts(1).Trim(), span)
            If span < 1 Then span = 1

            Dim hasBase As Boolean = (slotIdx >= 0 AndAlso slotIdx < _baseColumns.Count)
            Dim baseFirst As ColumnDef = If(hasBase, _baseColumns(slotIdx), Nothing)

            Dim wsum As Integer = 0
            For k As Integer = 0 To span - 1
                Dim gi As Integer = slotIdx + k
                If gi >= 0 AndAlso gi < _baseColumns.Count Then wsum += _baseColumns(gi).Width
            Next
            If wsum <= 0 Then wsum = 100

            Dim cd As New ColumnDef With {.Name = hdr, .Header = hdr, .Width = wsum}

            If parts.Length > 2 AndAlso parts(2).Trim().Length > 0 Then
                Dim aVal As Integer = 0
                If Integer.TryParse(parts(2).Trim(), aVal) Then
                    cd.Align = CType(aVal, En_ColAlign)
                Else
                    cd.Align = If(hasBase, baseFirst.Align, En_ColAlign.ColAlign_Left)
                End If
            Else
                cd.Align = If(hasBase, baseFirst.Align, En_ColAlign.ColAlign_Left)
            End If

            If parts.Length > 3 AndAlso parts(3).Trim().Length > 0 Then
                Dim tVal As Integer = 0
                If Integer.TryParse(parts(3).Trim(), tVal) Then
                    cd.ColType = CType(tVal, En_ColType)
                Else
                    cd.ColType = If(hasBase, baseFirst.ColType, En_ColType.ColType_Text)
                End If
            Else
                cd.ColType = If(hasBase, baseFirst.ColType, En_ColType.ColType_Text)
            End If

            If hasBase Then
                cd.Format = baseFirst.Format
                cd.HeaderBackColor = baseFirst.HeaderBackColor
                cd.HeaderForeColor = baseFirst.HeaderForeColor
                cd.HeaderBold = baseFirst.HeaderBold
                cd.HeaderItalic = baseFirst.HeaderItalic
                cd.HeaderUnderline = baseFirst.HeaderUnderline
                cd.HeaderAlign = baseFirst.HeaderAlign
            Else
                cd.Format = ""
                cd.HeaderBackColor = Color.Empty
                cd.HeaderForeColor = Color.Empty
                cd.HeaderAlign = En_ColAlign.ColAlign_Inherit
            End If

            newCols.Add(cd)
            slotIdx += span
        Next

        Return newCols
    End Function

    ' Banda PROPRIE a randului (cache). Urca la parinte daca nodul nu are ColHeaderText.
    Private Function GetRowColumns(it As TreeItem) As List(Of ColumnDef)
        If it Is Nothing Then Return _columns
        If it.ResolvedCols IsNot Nothing Then Return it.ResolvedCols

        Dim src As TreeItem = it
        Dim cht As String = ""
        While src IsNot Nothing
            If Not String.IsNullOrEmpty(src.ColHeaderText) Then
                cht = src.ColHeaderText
                Exit While
            End If
            src = src.Parent
        End While

        it.ResolvedCols = ResolveBandColumns(cht)
        Return it.ResolvedCols
    End Function

    ''' <summary>
    ''' DFS: returnează primul ColHeaderText găsit în subarborele nodului (inclusiv nodul însuși).
    ''' </summary>
    Private Function FindColHeaderTextInSubtree(node As TreeItem) As String
        If node Is Nothing Then Return ""
        If Not String.IsNullOrEmpty(node.ColHeaderText) Then Return node.ColHeaderText
        For Each child In node.Children
            Dim found As String = FindColHeaderTextInSubtree(child)
            If Not String.IsNullOrEmpty(found) Then Return found
        Next
        Return ""
    End Function

    ''' <summary>
    ''' True daca randul nodului primeste celule de coloane.
    ''' Mod dinamic: toate randurile. Mod static: doar Level = _columnsLevel (strict).
    ''' </summary>
    Friend Function RowHasColumns(it As TreeItem) As Boolean
        If it Is Nothing Then Return False
        If _dynamicColumns Then Return True
        Return it.Level = _columnsLevel
    End Function

    ' ════════════════════════════════════════════════════════════════════
    ' COLUMN FILTER — POPUP
    ' ════════════════════════════════════════════════════════════════════
    Friend Sub ShowColumnFilterPopup(colIdx As Integer, screenPos As Point)
        Try
            If colIdx < 0 OrElse colIdx >= _columns.Count Then Return
            _activeColFilterPopup?.Close()
            _activeColFilterPopup = Nothing
            Dim popup As New ColFilterPopup(Me, _columns(colIdx).Name, screenPos)
            _activeColFilterPopup = popup
            popup.Show(Me.FindForm())
        Catch ex As Exception
            TreeLogger.Ex(ex, "ShowColumnFilterPopup")
        End Try
    End Sub
End Class
