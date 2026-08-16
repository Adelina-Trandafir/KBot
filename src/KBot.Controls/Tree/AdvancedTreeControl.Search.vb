Imports System.ComponentModel
Imports System.DirectoryServices
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions

Partial Public Class AdvancedTreeControl

    ' ── Filter state (inline search — no overlay) ────────────────────────
    Private _filterActive As Boolean = False
    Private _filterSet As New HashSet(Of TreeItem)()

    ' Search bar row state
    Private _searchBarHeight As Integer = 0
    Private _searchBarLabel As Label = Nothing
    Private _searchPlaceholderActive As Boolean = False

    Private _searchClearBtn As Label = Nothing
    Private Const CLEAR_BTN_WIDTH As Integer = 18

    ' ── Win32 CueBanner — placeholder nativ, fără race conditions ────────────
    Private Const EM_SETCUEBANNER As Integer = &H1501

    <System.Runtime.InteropServices.DllImport("user32.dll", CharSet:=System.Runtime.InteropServices.CharSet.Unicode)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As String) As IntPtr
    End Function

    Private Sub SetSearchCueBanner()
        Try
            If _searchTextBox Is Nothing OrElse String.IsNullOrEmpty(_searchDefaultText) Then Return
            If _searchTextBox.IsHandleCreated Then
                ' wParam=0: banner dispare când textbox-ul primește focus (comportament standard)
                SendMessage(_searchTextBox.Handle, EM_SETCUEBANNER,
                        New IntPtr(0), _searchDefaultText)
            Else
                ' Handle nu e creat încă — aplicăm la HandleCreated
                AddHandler _searchTextBox.HandleCreated, AddressOf OnSearchTextBoxHandleCreated
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.SetSearchCueBanner", ex)
            Throw
        End Try
    End Sub

    Private Sub OnSearchTextBoxHandleCreated(sender As Object, e As EventArgs)
        Try
            RemoveHandler _searchTextBox.HandleCreated, AddressOf OnSearchTextBoxHandleCreated
            If Not String.IsNullOrEmpty(_searchDefaultText) Then
                SendMessage(_searchTextBox.Handle, EM_SETCUEBANNER,
                        New IntPtr(0), _searchDefaultText)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchTextBoxHandleCreated", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' SEARCH
    ' ══════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Aplică <see cref="SearchShow"/>: banda de căutare e permanentă când NU există iconiță
    ''' de toggle în antet (cu iconiță, banda se deschide/închide din ea). Se cheamă din setter,
    ''' din <see cref="ResolveHeaderIcons"/> și din OnHandleCreated — setterul poate rula în
    ''' mijlocul lui InitializeComponent, înainte ca fontul/dimensiunile să fie stabilite.
    ''' </summary>
    Friend Sub ApplySearchShow()
        Try
            If _searchShow Then
                If _headerSearchIcon Is Nothing AndAlso Not _isSearchMode Then OpenSearchMode(focusTree:=False)
            ElseIf _isSearchMode Then
                ForceCloseSearchMode()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.ApplySearchShow", ex)
        End Try
    End Sub

    ''' <summary>
    ''' True în designerul Visual Studio. Folosim ajutorul casei (<see cref="KBotDesignTime"/>),
    ''' nu Control.DesignMode: pe net8.0-windows designerul rulează ÎN AFARA procesului
    ''' (DesignToolsServer.exe), iar un control imbricat nici măcar nu e «sitat».
    ''' </summary>
    Private ReadOnly Property InDesigner As Boolean
        Get
            Return KBotDesignTime.IsDesignTime(Me)
        End Get
    End Property

    Private Sub RecomputeSearchBarHeight()
        ' Banda de search e ÎNTOTDEAUNA o bandă separată, dimensionată după rând/font.
        _searchBarHeight = Math.Max(_itemHeight + 8, Me.Font.Height + 10)
    End Sub

    ''' <summary>Re-dimensionează banda după o schimbare de font / _itemHeight (no-op dacă e închisă).</summary>
    Friend Sub RefreshSearchBarMetrics()
        If Not _isSearchMode Then Return
        RecomputeSearchBarHeight()
        If _searchTextBox IsNot Nothing Then PositionSearchTextBox()
        Me.Invalidate()
    End Sub

    Friend Sub DrawSearchBar(g As Graphics)
        Dim barTop As Integer = If(_headerVisible, _headerHeight, 0)

        ' Background cu culoarea proprie a benzii de search
        Using bg As New SolidBrush(SearchBackColor)
            g.FillRectangle(bg, 0, barTop, Me.Width, _searchBarHeight)
        End Using

        ' Fără controale copil reale (design-time) banda se desenează integral — etichetă,
        ' casetă, placeholder, ✕ — ca designerul să arate exact ce va fi la runtime.
        If _searchTextBox Is Nothing Then DrawSearchBarPreview(g, barTop)

        ' Separator inferior
        Using sep As New Pen(Color.FromArgb(80, Color.Black))
            g.DrawLine(sep, 0, barTop + _searchBarHeight - 1,
                   Me.Width, barTop + _searchBarHeight - 1)
        End Using
    End Sub

    ' Replică desenată a benzii. Geometria urmează pas cu pas PositionSearchTextBox
    ' (etichetă la PaddingTreeStart, casetă până la PaddingTreeEnd, ✕ lipit în dreapta),
    ' ca trecerea design-time → runtime să nu mute nimic.
    Private Sub DrawSearchBarPreview(g As Graphics, barTop As Integer)
        Dim labelFont As Font = Me.SearchBarLabelFont
        Dim boxFont As Font = Me.SearchBarFont
        Dim x As Integer = PaddingTreeStart

        If Not String.IsNullOrEmpty(_searchBarLabelText) Then
            Dim latime As Integer = CInt(Math.Ceiling(g.MeasureString(_searchBarLabelText, labelFont).Width))
            Dim inaltime As Integer = labelFont.Height
            Using b As New SolidBrush(SearchBarLabelForeColor)
                g.DrawString(_searchBarLabelText, labelFont, b,
                             CSng(x), CSng(barTop + (_searchBarHeight - inaltime) \ 2))
            End Using
            x += latime + 4
        End If

        Dim clearW As Integer = If(_searchClearButton, SearchClearButtonWidth, 0)
        Dim boxW As Integer = Math.Max(40, Me.Width - x - PaddingTreeEnd - clearW)
        Dim boxH As Integer = boxFont.Height + 2
        Dim boxRect As New Rectangle(x, barTop + (_searchBarHeight - boxH) \ 2, boxW, boxH)
        Dim boxBack As Color = SearchBoxBackColor

        Using b As New SolidBrush(boxBack)
            g.FillRectangle(b, boxRect)
        End Using

        ' Placeholder-ul (la runtime îl pune Win32 EM_SETCUEBANNER, gri, centrat).
        If Not String.IsNullOrEmpty(_searchDefaultText) Then
            Using b As New SolidBrush(Color.FromArgb(140, Me.ForeColor)),
                  fmt As New StringFormat With {
                      .Alignment = StringAlignment.Center,
                      .LineAlignment = StringAlignment.Center,
                      .Trimming = StringTrimming.EllipsisCharacter,
                      .FormatFlags = StringFormatFlags.NoWrap}
                g.DrawString(_searchDefaultText, boxFont, b,
                             New RectangleF(boxRect.X, boxRect.Y, boxRect.Width, boxRect.Height), fmt)
            End Using
        End If

        If _searchClearButton Then
            Dim clearRect As New Rectangle(boxRect.Right, boxRect.Top, clearW, boxRect.Height)
            Using b As New SolidBrush(boxBack)
                g.FillRectangle(b, clearRect)
            End Using
            If _searchClearButtonImage IsNot Nothing Then
                Dim img As Image = _searchClearButtonImage
                g.DrawImage(img,
                            clearRect.X + _searchClearButtonPadding.Left,
                            clearRect.Y + Math.Max(0, (clearRect.Height - img.Height) \ 2),
                            img.Width, img.Height)
            Else
                Using b As New SolidBrush(Me.ForeColor),
                      fmt As New StringFormat With {
                          .Alignment = StringAlignment.Center,
                          .LineAlignment = StringAlignment.Center}
                    g.DrawString("✕", boxFont, b,
                                 New RectangleF(clearRect.X, clearRect.Y, clearRect.Width, clearRect.Height), fmt)
                End Using
            End If
        End If
    End Sub

    Private Sub OpenSearchMode(Optional focusTree As Boolean = True)
        RecomputeSearchBarHeight()
        _isSearchMode = True
        _searchResults.Clear()
        _searchPlaceholderActive = False

        ' Design-time: NU creăm controale copil reale (un TextBox viu în designer fură
        ' click-urile și apare ca element ne-selectabil). Banda e desenată — DrawSearchBar.
        If InDesigner Then
            Me.Invalidate()
            Return
        End If

        If _searchTextBox Is Nothing Then
            _searchTextBox = New TextBox() With {
            .BorderStyle = BorderStyle.None,
            .Font = Me.Font,
            .TabStop = False,
            .TextAlign = HorizontalAlignment.Center
        }
            AddHandler _searchTextBox.TextChanged, AddressOf OnSearchTextChanged
            AddHandler _searchTextBox.KeyDown, AddressOf OnSearchTextBoxKeyDown
            Me.Controls.Add(_searchTextBox)
        End If
        UpdateSearchTextBoxFont()
        _searchTextBox.Text = ""

        If Not String.IsNullOrEmpty(_searchBarLabelText) Then
            If _searchBarLabel Is Nothing Then
                _searchBarLabel = New Label() With {
                    .AutoSize = True,
                    .Text = _searchBarLabelText,
                    .TabStop = False
                }
                UpdateSearchBarLabelFont()
                Me.Controls.Add(_searchBarLabel)
            End If
            _searchBarLabel.Visible = True
            _searchBarLabel.BringToFront()
        End If

        ' ── Clear button (✕) — opțional, vizual în interiorul textbox-ului ────
        EnsureClearButton()
        RestyleSearchChildren()

        PositionSearchTextBox()
        _searchTextBox.Visible = True
        _searchTextBox.BringToFront()

        ApplySearchPlaceholder()
        Me.Invalidate()
        If focusTree AndAlso Me.IsHandleCreated Then Me.Focus()
    End Sub

    ' Creează (o singură dată) și re-stilizează butonul ✕. Separat de OpenSearchMode ca
    ' SearchClearButton să poată fi comutat și după deschiderea benzii (playground/runtime).
    Private Sub EnsureClearButton()
        If Not _searchClearButton Then Return
        If _searchClearBtn Is Nothing Then
            _searchClearBtn = New Label() With {
                .AutoSize = False,
                .TextAlign = ContentAlignment.MiddleCenter,
                .ImageAlign = ContentAlignment.MiddleCenter,
                .Cursor = Cursors.Hand,
                .Visible = False,
                .TabStop = False
            }
            AddHandler _searchClearBtn.Click, AddressOf OnSearchClearBtnClick
            HookClearButtonHover(_searchClearBtn)   ' vezi .ButtonHover
            Me.Controls.Add(_searchClearBtn)
        End If
        ApplyClearButtonLook()
        _searchClearBtn.BringToFront()
    End Sub

    ''' <summary>Glifă sau imagine + padding pe butonul de golire.</summary>
    Friend Sub ApplyClearButtonLook()
        If _searchClearBtn Is Nothing Then Return
        _searchClearBtn.Width = SearchClearButtonWidth
        _searchClearBtn.Padding = _searchClearButtonPadding
        If _searchClearButtonImage IsNot Nothing Then
            _searchClearBtn.Image = _searchClearButtonImage
            _searchClearBtn.Text = String.Empty
        Else
            _searchClearBtn.Image = Nothing
            _searchClearBtn.Text = "✕"
        End If
        _searchClearBtn.Font = SearchBarFont
        _searchClearBtn.ForeColor = Me.ForeColor
        ApplyClearButtonHoverColor()   ' el scrie BackColor-ul, ca hover-ul să nu fie rescris
    End Sub

    ''' <summary>
    ''' Sincronizează eticheta cu proprietățile ei (text/culoare/font), inclusiv apariția sau
    ''' dispariția ei când SearchBarLabelText devine gol / nevid cu banda deja deschisă.
    ''' </summary>
    Friend Sub RefreshSearchBarLabel()
        If Not _isSearchMode OrElse _searchTextBox Is Nothing Then Return
        If String.IsNullOrEmpty(_searchBarLabelText) Then
            If _searchBarLabel IsNot Nothing Then _searchBarLabel.Visible = False
        Else
            If _searchBarLabel Is Nothing Then
                _searchBarLabel = New Label() With {.AutoSize = True, .TabStop = False}
                Me.Controls.Add(_searchBarLabel)
            End If
            _searchBarLabel.Text = _searchBarLabelText
            UpdateSearchBarLabelFont()
            RestyleSearchChildren()
            _searchBarLabel.Visible = True
            _searchBarLabel.BringToFront()
        End If
        PositionSearchTextBox()
        Me.Invalidate()
    End Sub

    ''' <summary>Comutat de SearchClearButton la runtime, cu banda deja deschisă.</summary>
    Friend Sub RefreshClearButton()
        If Not _isSearchMode OrElse _searchTextBox Is Nothing Then Return
        If _searchClearButton Then
            EnsureClearButton()
            UpdateClearBtnVisibility()
        ElseIf _searchClearBtn IsNot Nothing Then
            _searchClearBtn.Visible = False
        End If
        PositionSearchTextBox()
        Me.Invalidate()
    End Sub

    ''' <summary>
    ''' Închidere din interacțiune (iconița de search, butonul ✕). Când banda e permanentă
    ''' — SearchShow fără iconiță de toggle — nu se închide.
    ''' </summary>
    Friend Sub CloseSearchMode()
        If _searchShow AndAlso _headerSearchIcon Is Nothing Then Return
        ForceCloseSearchMode()
    End Sub

    ''' <summary>Închidere necondiționată (SearchShow = False).</summary>
    Friend Sub ForceCloseSearchMode()
        If _searchClearBtn IsNot Nothing Then _searchClearBtn.Visible = False
        If _searchTextBox IsNot Nothing Then _searchTextBox.Visible = False
        If _searchBarLabel IsNot Nothing Then _searchBarLabel.Visible = False
        _filterActive = False
        _filterSet.Clear()
        _isSearchMode = False
        _searchPlaceholderActive = False
        _searchResults.Clear()
        _searchBarHeight = 0    ' ← reset explicit — headerOff din OnPaint/GetItemY devine corect imediat
        Me.Invalidate()
    End Sub

    Private Sub PositionSearchTextBox()
        If _searchTextBox Is Nothing Then Return
        'Dim scrollW As Integer = ScrollBarWidth 'If(_vScroll.Visible, _vScroll.Width, 0)
        ' barTop vizual REAL = poziție fixă + compensare scroll
        Dim barTop As Integer = If(_headerVisible, _headerHeight, 0)
        Dim tbTop As Integer = barTop + (_searchBarHeight - _searchTextBox.PreferredHeight) \ 2

        ' Spațiu rezervat pentru ✕ — DOAR când butonul e vizibil
        Dim clearW As Integer = If(_searchClearButton AndAlso
                                _searchClearBtn IsNot Nothing AndAlso
                                _searchClearBtn.Visible, SearchClearButtonWidth, 0)

        Dim tbLeft As Integer
        Dim tbWidth As Integer

        If _searchBarLabel IsNot Nothing AndAlso _searchBarLabel.Visible Then
            _searchBarLabel.Left = PaddingTreeStart
            _searchBarLabel.Top = barTop + (_searchBarHeight - _searchBarLabel.Height) \ 2
            tbLeft = _searchBarLabel.Right + 4
            tbWidth = Math.Max(40, Me.Width - tbLeft - PaddingTreeEnd - clearW)
        Else
            tbLeft = PaddingTreeStart
            tbWidth = Math.Max(40, Me.Width - PaddingTreeStart - PaddingTreeEnd - clearW)
        End If

        _searchTextBox.Left = tbLeft
        _searchTextBox.Top = tbTop
        _searchTextBox.Width = tbWidth
        _searchTextBox.Height = _searchTextBox.PreferredHeight

        ' ── Poziționare ✕ imediat la dreapta textbox-ului, aceeași înălțime ──
        If _searchClearButton AndAlso _searchClearBtn IsNot Nothing AndAlso _searchClearBtn.Visible Then
            _searchClearBtn.Left = _searchTextBox.Right
            _searchClearBtn.Top = _searchTextBox.Top - _searchClearButtonPadding.Top
            _searchClearBtn.Width = SearchClearButtonWidth
            _searchClearBtn.Height = _searchTextBox.Height +
                                     _searchClearButtonPadding.Vertical
        End If
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' SEARCH — TEXTBOX EVENTS
    ' ══════════════════════════════════════════════════════════════════

    Private Sub OnSearchTextChanged(sender As Object, e As EventArgs)
        Try
            If _searchPlaceholderActive Then Return
            SearchDebounceTimer.Stop()
            If _searchTextBox Is Nothing Then Return
            Dim txt = _searchTextBox.Text
            UpdateClearBtnVisibility()      ' ← adăugat
            If txt.Length < 3 Then
                _filterActive = False
                _filterSet.Clear()
                _searchResults.Clear()
                Me.Invalidate()
            Else
                SearchDebounceTimer.Start()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchTextChanged", ex)
        End Try
    End Sub

    Private Sub OnSearchDebounceTimerTick(sender As Object, e As EventArgs) Handles SearchDebounceTimer.Tick
        Try
            SearchDebounceTimer.Stop()
            If _searchTextBox IsNot Nothing Then
                PerformSearch(_searchTextBox.Text)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchDebounceTimerTick", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' SEARCH — CORE LOGIC
    ' ══════════════════════════════════════════════════════════════════

    Private Sub PerformSearch(searchText As String)
        _searchResults.Clear()
        _filterSet.Clear()

        If String.IsNullOrEmpty(searchText) OrElse searchText.Length < 3 Then
            _filterActive = False
            _vScroll.Value = 0                    ' ← reset
            Me.Invalidate()
            Return
        End If

        Dim matchSet As New HashSet(Of TreeItem)()
        CollectMatchingNodes(Items, searchText, matchSet)

        For Each node In matchSet
            _filterSet.Add(node)
            Dim p = node.Parent
            While p IsNot Nothing
                _filterSet.Add(p)
                p = p.Parent
            End While
        Next

        BuildTreeSearchResults(searchText)
        _filterActive = (_filterSet.Count > 0)
        RaiseEvent SearchFinished(matchSet.ToList(), searchText)

        _vScroll.Value = 0                        ' ← reset după filter nou
        Me.BeginInvoke(New Action(AddressOf RefreshScrollVisibility))
        Me.Invalidate()
    End Sub

    Private Function MatchesSearch(it As TreeItem, searchText As String) As Boolean
        Dim lower As String = searchText.ToLowerInvariant()

        ' Strip mini-html tags for caption search
        Dim plainCaption As String = Regex.Replace(
            If(it.Caption, ""), "<[^>]+>", "", RegexOptions.IgnoreCase
        ).ToLowerInvariant()

        Dim tagText As String = If(it.Tag IsNot Nothing, it.Tag.ToString(), "").ToLowerInvariant()

        Dim toSearch As String
        Select Case _searchIn
            Case En_Tree_SearchIn.SearchIn_Tag : toSearch = tagText
            Case En_Tree_SearchIn.SearchIn_Both : toSearch = plainCaption & " " & tagText
            Case Else : toSearch = plainCaption
        End Select

        Return If(_searchType = En_Tree_SearchType.SearchType_StartsWith,
                  toSearch.StartsWith(lower),
                  toSearch.Contains(lower))
    End Function

    ' ── List mode ────────────────────────────────────────────────────
    Private Sub BuildListSearchResults(searchText As String)
        CollectListResultsRecursive(Items, searchText)
    End Sub

    Private Sub CollectListResultsRecursive(nodes As List(Of TreeItem), searchText As String)
        For Each it In nodes
            If MatchesSearch(it, searchText) Then
                _searchResults.Add(New SearchResultItem(it, False))
            End If
            CollectListResultsRecursive(it.Children, searchText)
        Next
    End Sub

    ' ── Tree mode ────────────────────────────────────────────────────
    Private Sub BuildTreeSearchResults(searchText As String)
        Dim matchSet As New HashSet(Of TreeItem)()
        CollectMatchingNodes(Items, searchText, matchSet)
        If matchSet.Count = 0 Then Return

        ' Collect all ancestors
        Dim ancestorSet As New HashSet(Of TreeItem)()
        For Each node In matchSet
            Dim p = node.Parent
            While p IsNot Nothing
                ancestorSet.Add(p)
                p = p.Parent
            End While
        Next

        ' DFS traversal, same order as tree rendering, keeping only relevant nodes
        BuildTreeResultsOrdered(Items, matchSet, ancestorSet)
    End Sub

    Private Sub CollectMatchingNodes(nodes As List(Of TreeItem),
                                     searchText As String,
                                     result As HashSet(Of TreeItem))
        For Each it In nodes
            If MatchesSearch(it, searchText) Then result.Add(it)
            CollectMatchingNodes(it.Children, searchText, result)
        Next
    End Sub

    Private Sub BuildTreeResultsOrdered(nodes As List(Of TreeItem),
                                        matchSet As HashSet(Of TreeItem),
                                        ancestorSet As HashSet(Of TreeItem))
        For Each it In nodes
            Dim isMatch = matchSet.Contains(it)
            Dim isAncestor = ancestorSet.Contains(it)
            If Not isMatch AndAlso Not isAncestor Then Continue For

            ' Dimmed = ancestor-only (not itself a match)
            _searchResults.Add(New SearchResultItem(it, isAncestor AndAlso Not isMatch))

            ' Always recurse into children of ancestors (forced expand)
            If isAncestor Then
                BuildTreeResultsOrdered(it.Children, matchSet, ancestorSet)
            End If
        Next
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' SEARCH — PLACEHOLDER
    ' ══════════════════════════════════════════════════════════════════

    Friend Sub ApplySearchPlaceholder()
        SetSearchCueBanner()
    End Sub

    Private Sub RemoveSearchPlaceholder()
        _searchPlaceholderActive = False
    End Sub

    Private Sub UpdateClearBtnVisibility()
        If Not _searchClearButton OrElse _searchClearBtn Is Nothing Then Return
        Dim shouldShow As Boolean = _searchTextBox IsNot Nothing AndAlso
                                Not _searchPlaceholderActive AndAlso
                                _searchTextBox.Text.Length > 0
        If _searchClearBtn.Visible = shouldShow Then Return
        _searchClearBtn.Visible = shouldShow

        ' Poziționare doar a butonului × — TextBox rămâne neatins
        If shouldShow AndAlso _searchTextBox IsNot Nothing Then
            _searchClearBtn.Left = _searchTextBox.Right - SearchClearButtonWidth
            _searchClearBtn.Top = _searchTextBox.Top - _searchClearButtonPadding.Top
            _searchClearBtn.Width = SearchClearButtonWidth
            _searchClearBtn.Height = _searchTextBox.Height + _searchClearButtonPadding.Vertical
            _searchClearBtn.BackColor = _searchTextBox.BackColor
            _searchClearBtn.BringToFront()
        End If
    End Sub

    ''' <summary>Golește caseta (OnSearchTextChanged ridică filtrul) și îi redă focusul.</summary>
    Friend Sub ClearSearchText()
        If _searchTextBox Is Nothing Then Return
        _searchTextBox.Text = ""
        If Me.IsHandleCreated Then _searchTextBox.Focus()
    End Sub

    Private Sub OnSearchClearBtnClick(sender As Object, e As EventArgs)
        Try
            If _headerSearchIcon IsNot Nothing Then
                ' Se comportă identic cu click pe icona de search (toggle)
                CloseSearchMode()
            Else
                ' Curăță textul — OnSearchTextChanged resetează filtrul automat
                ' UpdateClearBtnVisibility ascunde × și relărgește textbox-ul
                ClearSearchText()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchClearBtnClick", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════
    ' SEARCH — KEYBOARD NAVIGATION
    ' ══════════════════════════════════════════════════════════════════

    Private Sub OnSearchTextBoxKeyDown(sender As Object, e As KeyEventArgs)
        Try
            ' ── ESC = golire ────────────────────────────────────────────────────
            ' Prima apăsare curăță textul (OnSearchTextChanged ridică filtrul). Pe o casetă
            ' deja goală, ESC închide banda — dar CloseSearchMode e no-op pentru banda
            ' permanentă, deci acolo ESC nu face decât să golească.
            If e.KeyCode = Keys.Escape Then
                e.Handled = True
                e.SuppressKeyPress = True
                If _searchTextBox IsNot Nothing AndAlso _searchTextBox.Text.Length > 0 Then
                    ClearSearchText()
                Else
                    CloseSearchMode()
                End If
                Return
            End If

            If e.KeyCode <> Keys.Down AndAlso e.KeyCode <> Keys.Up Then Return

            Dim visible = GetVisibleItems()
            If visible.Count = 0 Then Return

            pSelectedItem = If(e.KeyCode = Keys.Down, visible.First(), visible.Last())

            e.Handled = True
            Me.Focus()
            Dim itemY = GetItemY(pSelectedItem)
            If itemY >= 0 Then
                Me.AutoScrollPosition = New Point(0, itemY - _headerHeight - _searchBarHeight)
            End If
            Me.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("AdvancedTreeControl.OnSearchTextBoxKeyDown", ex)
        End Try
    End Sub

End Class
