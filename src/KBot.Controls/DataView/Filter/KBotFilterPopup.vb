Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The COLUMN MENU of a <see cref="KBotDataView"/> -- the counterpart of the arrow in the header
''' of an Access datasheet, with THREE TABS chosen from a horizontal <see cref="KBotNavList"/>
''' (slice 0030):
'''
''' <list type="number">
''' <item><description><b>Sortare</b> (<see cref="KBotFilterPopupSortView"/>) -- ascending /
''' descending, plus «reseteaza sortarea»;</description></item>
''' <item><description><b>Filtrare</b> (<see cref="KBotFilterPopupFilterView"/>) -- the list of
''' tickable values with «(Selecteaza tot)» and a search box, the CONDITIONS submenu («Filtre
''' text / numerice / de data») and «Sterge filtrul»;</description></item>
''' <item><description><b>Grupare</b> (<see cref="KBotFilterPopupGroupView"/>) -- the level
''' options of slice 0029 for THIS column, plus the grid's hierarchy of levels. The tab shows only
''' when the grid has <see cref="KBotDataView.EnableGrouping"/> on.</description></item>
''' </list>
'''
''' <para><b>Each tab is a view of its own.</b> Until this split the three tabs were three
''' Panels stacked in one designer file, and this class held the behaviour of all of them. Now
''' every tab is a <c>KBotThemedUserControl</c> with its own designer, created lazily at first
''' activation and hosted one at a time in <c>viewHost</c> -- the same shape as <c>KbotForm</c>
''' and <c>SetariForm</c> behind their nav lists. This class keeps only what the tabs share: the
''' frame (nav bar, separator, command bar), the window's height, the menu's window behaviour
''' (deactivate closes, Esc, Enter, the drop shadow) and the two things a tab cannot do from
''' inside a UserControl: open the conditions submenu and the modal operand dialog, which need
''' the popup's own window hidden and its deactivation suppressed.</para>
'''
''' <para><b>Three ways of handing a decision over, and the difference matters:</b></para>
''' <list type="bullet">
''' <item><description><b>The FILTER is handed over at OK.</b> The filter tab works on a COPY
''' (<see cref="KBotColumnFilter.Clone"/>) and <see cref="FilterAccepted"/> is raised only at OK;
''' «Anuleaza» and Esc leave nothing behind.</description></item>
''' <item><description><b>SORTING applies at once AND closes the menu</b> -- it is a command, not
''' a choice to confirm, exactly as in Access.</description></item>
''' <item><description><b>GROUPING applies at once but does NOT close the menu.</b> It is a
''' command too (the grid rearranges on the spot), only it has seven options: a tab that closed
''' at the first tick would have to be reopened six times.</description></item>
''' </list>
''' </summary>
<ToolboxItem(False)>
Partial Friend NotInheritable Class KBotFilterPopup

    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const CS_DROPSHADOW As Integer = &H20000

    ''' <summary>The window never goes below this, however short the active tab is.</summary>
    Private Const MinHeight As Integer = 160

    ' ── What the menu is for ─────────────────────────────────────────────────
    Private ReadOnly _columnKey As String
    Private ReadOnly _columnCaption As String
    Private ReadOnly _valueType As KBotValueType
    Private ReadOnly _distinctValues As New List(Of String)()  ' handed to the filter tab when it is created
    Private ReadOnly _currentFilter As KBotColumnFilter
    Private ReadOnly _currentSort As KBotSortDirection

    ' The .NET format of the condition dialog's date fields (only read on a DateTime column):
    ' the column's own format, so the field shows the same hour/second/millisecond the cell does.
    Private ReadOnly _dateOperandFormat As String

    ' The grid that opened the menu -- ONLY so the grouping tab can read the grouping state
    ' (active levels and the level of this column). Nothing is written through it: decisions go
    ' out on events, like the filter and the sort. Nothing = no host (tests, the harness) =>
    ' no grouping tab.
    Private ReadOnly _grid As KBotDataView

    ' Tabs created lazily (key -> instance); one is visible.
    Private ReadOnly _views As New Dictionary(Of String, IKBotFilterMenuView)(StringComparer.Ordinal)
    Private _activeView As IKBotFilterMenuView

    ' The open tab. Kept IN A FIELD, not read from a control's Visible, and that is not a
    ' preference: the getter of Control.Visible answers about the PARENT CHAIN, so on a form not
    ' yet shown every tab reports False -- the measurement would always size the window on the
    ' same tab, and any headless check would measure something other than what is on screen.
    Private _activeKey As String = "filtrare"

    Private _suppressDeactivate As Boolean = False
    Private _closing As Boolean = False

    ' False until the end of the constructor: the nav bar's EndInit raises SelectionChanged from
    ' the middle of InitializeComponent, long before any field above exists.
    Private _built As Boolean = False

    ''' <summary>
    ''' The operator pressed OK: the filter in the argument is the one to put on the column
    ''' (it may be inactive, i.e. «no filter»).
    ''' </summary>
    Friend Event FilterAccepted As EventHandler(Of KBotFilterAcceptedEventArgs)

    ''' <summary>The operator asked for a sort. It applies at once and the menu closes.</summary>
    Friend Event SortRequested As EventHandler(Of KBotSortRequestedEventArgs)

    ''' <summary>
    ''' The operator changed the column's grouping. It applies at once but the menu STAYS open --
    ''' see the class summary.
    ''' </summary>
    Friend Event GroupingRequested As EventHandler(Of KBotGroupingRequestedEventArgs)

    ''' <summary>
    ''' Builds the menu for a column: the displayed caption, the type of the values, the distinct
    ''' values (already formatted, in sort order), the current filter (<c>Nothing</c> = none) and
    ''' the column's sort direction. The grid is optional: without it the menu has no grouping tab.
    ''' <paramref name="dateOperandFormat"/> is the format of the condition dialog's date fields
    ''' (see <see cref="KBotColumnFormat.DateOperandFormat"/>); Nothing = the culture's short date.
    ''' </summary>
    Friend Sub New(columnKey As String, columnCaption As String, valueType As KBotValueType,
                   distinctValues As IEnumerable(Of String), currentFilter As KBotColumnFilter,
                   currentSort As KBotSortDirection, Optional grid As KBotDataView = Nothing,
                   Optional dateOperandFormat As String = Nothing)
        InitializeComponent()

        ' A menu, not a dialog: it already has the menu shadow (CS_DROPSHADOW, in CreateParams
        ' below), so the window shadow of the base (slice 0069) would only double it.
        BorderlessShadow = False

        _columnKey = columnKey
        _columnCaption = If(columnCaption, String.Empty)
        _valueType = valueType
        _grid = grid
        If distinctValues IsNot Nothing Then _distinctValues.AddRange(distinctValues)
        _currentFilter = currentFilter
        _currentSort = currentSort
        _dateOperandFormat = dateOperandFormat

        ' The grouping tab exists only if the grid offers it (KBotDataView.EnableGrouping).
        navFile.SetItemVisible("grupare", HasGrouping)

        _built = True
        ActivateView(navFile.SelectedKey)
    End Sub

    ''' <summary>The key of the column the menu was opened for.</summary>
    Friend ReadOnly Property ColumnKey As String
        Get
            Return _columnKey
        End Get
    End Property

    ''' <summary>The tab open now: «sortare», «filtrare» or «grupare».</summary>
    Friend ReadOnly Property FilaCurenta As String
        Get
            Return _activeKey
        End Get
    End Property

    ' Does the grid offer the operator the grouping tab, on this column? (Without a host, never.)
    ' The column's own AllowGrouping narrows the grid-wide switch (slice 0080-02).
    Private ReadOnly Property HasGrouping As Boolean
        Get
            If _grid Is Nothing OrElse Not _grid.EnableGrouping Then Return False
            Dim col As KBotDataColumn = _grid.Columns.FirstOrDefault(
                Function(c) String.Equals(c.Key, _columnKey, StringComparison.Ordinal))
            Return col Is Nothing OrElse col.AllowGrouping
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TOOLWINDOW      ' no button on the task bar
            cp.ClassStyle = cp.ClassStyle Or CS_DROPSHADOW   ' the shadow every menu has
            Return cp
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' THEME
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The SEMANTIC colours of the frame, the ones the generic per-type rules cannot know: the
    ''' menu border (the form's margin), the line under the navigation and the surface the frame
    ''' sits on. Each tab colours its own content in its <c>ApplyTheme</c>; the two command
    ''' buttons and the nav bar come from <c>ThemeManager.Apply</c>, through
    ''' <see cref="KBotThemedForm"/>.
    ''' </summary>
    Protected Overrides Sub OnThemeChanged()
        Try
            Dim p As ThemePalette = ThemeManager.Current.Palette
            BackColor = p.BorderColor                ' the 1px frame = the form's Padding
            pnlBody.BackColor = p.SurfaceAltColor
            viewHost.BackColor = p.SurfaceAltColor
            pnlCommands.BackColor = p.SurfaceAltColor
            navFile.BackColor = p.SurfaceAltColor
            ButtonStyles.ApplyTrans(btnCancel, ThemeManager.Current)
            ButtonStyles.ApplyTrans(btnOk, ThemeManager.Current)
            btnCancel.Padding = Padding.Empty
            btnOk.Padding = Padding.Empty

            ' The separator is a PANEL, not a label: the generic Label rule sets
            ' BackColor = Transparent, i.e. a 1px line that no longer shows at all.
            sepNav.BackColor = p.BorderColor

            ' A scheme may ask for other air around the text (Modern: 12,8,12,8) and another
            ' font, and the designer authored everything on Classic. The tab re-measures its
            ' rows, then the window re-measures over them.
            AdjustHeight()
        Catch ex As Exception
            ' Theme boundary: log and swallow -- a throw here would break the scheme switch.
            GlobalErrorLog.Write("KBotFilterPopup.OnThemeChanged", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' THE TABS
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub NavFile_SelectionChanged(key As String) Handles navFile.SelectionChanged
        Try
            If Not _built Then Return
            ActivateView(key)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.NavFile_SelectionChanged", ex)
        End Try
    End Sub

    ' Shows the tab asked for and hides the previous one. An unknown key falls on «filtrare» --
    ' that is the tab the funnel in the header is pressed for.
    Private Sub ActivateView(key As String)
        Try
            Dim group As Boolean = String.Equals(key, "grupare", StringComparison.Ordinal) AndAlso HasGrouping
            Dim sort As Boolean = String.Equals(key, "sortare", StringComparison.Ordinal)
            _activeKey = If(group, "grupare", If(sort, "sortare", "filtrare"))

            Dim view As IKBotFilterMenuView = ViewFor(_activeKey)
            Dim previous As IKBotFilterMenuView = _activeView
            _activeView = view
            DirectCast(view, Control).Visible = True
            If previous IsNot Nothing AndAlso Not ReferenceEquals(previous, view) Then
                DirectCast(previous, Control).Visible = False
            End If

            ' The OK / «Anuleaza» buttons belong to the FILTER: it is the only tab that hands
            ' anything over at the end (IKBotFilterMenuView.ShowsCommandBar).
            pnlCommands.Visible = view.ShowsCommandBar

            AdjustHeight()
            view.Activated()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.ActivateView", ex)
            Throw
        End Try
    End Sub

    ' The tab for a key, created at first request (lazy, like the shell's views) and added to
    ' the host hidden; the caller decides what shows. Every tab lives until the menu closes.
    Private Function ViewFor(key As String) As IKBotFilterMenuView
        Dim view As IKBotFilterMenuView = Nothing
        If _views.TryGetValue(key, view) Then Return view

        view = CreateView(key)
        Dim ctrl As Control = DirectCast(view, Control)
        ctrl.Dock = DockStyle.Fill
        ctrl.Visible = False
        viewHost.Controls.Add(ctrl)
        ThemeManager.Apply(ctrl)
        _views(key) = view
        Return view
    End Function

    Private Function CreateView(key As String) As IKBotFilterMenuView
        Try
            Select Case key
                Case "sortare"
                    Dim v As New KBotFilterPopupSortView(_valueType, _currentSort)
                    AddHandler v.SortRequested, AddressOf SortView_SortRequested
                    Return v
                Case "filtrare"
                    Dim v As New KBotFilterPopupFilterView(_columnKey, _columnCaption, _valueType,
                                                           _distinctValues, _currentFilter)
                    AddHandler v.ClearRequested, AddressOf FilterView_ClearRequested
                    AddHandler v.ConditionMenuRequested, AddressOf FilterView_ConditionMenuRequested
                    AddHandler v.ContentChanged, AddressOf View_ContentChanged
                    Return v
                Case "grupare"
                    Dim v As New KBotFilterPopupGroupView(_columnKey, _columnCaption, _grid)
                    AddHandler v.GroupingRequested, AddressOf GroupView_GroupingRequested
                    AddHandler v.ContentChanged, AddressOf View_ContentChanged
                    Return v
                Case Else
                    Throw New ArgumentException($"Filă necunoscută în meniul de coloană: '{key}'.", NameOf(key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.CreateView", ex)
            Throw
        End Try
    End Function

    ' Typed access to the two tabs that answer questions (BuildFilter, BuildGroupLevel, the
    ' Debug* gates). They are created on demand: a check may ask before the tab was ever shown.
    Private ReadOnly Property FilterView As KBotFilterPopupFilterView
        Get
            Return DirectCast(ViewFor("filtrare"), KBotFilterPopupFilterView)
        End Get
    End Property

    Private ReadOnly Property GroupView As KBotFilterPopupGroupView
        Get
            Return DirectCast(ViewFor("grupare"), KBotFilterPopupGroupView)
        End Get
    End Property

    ' ══════════════════════════════════════════════════════════════════════════
    ' THE WINDOW'S MEASURE
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The only measure left in code: the window's HEIGHT. The width and every other size are
    ''' the designer's -- a window that re-measured its own width would make everything the
    ''' operator lays out there pointless.
    '''
    ''' <para>One formula for all three tabs: <b>the frame</b> (navigation, line, command bar,
    ''' margins -- everything that is not the tab) plus <b>what the active tab asks for</b>
    ''' (<see cref="IKBotFilterMenuView.RequiredHeight"/>). Measured AFTER a layout, not before:
    ''' on a control tree not yet laid out the heights read are the designer's, and the
    ''' difference would add up on every call.</para>
    ''' </summary>
    Private Sub AdjustHeight()
        If _activeView Is Nothing Then Return
        Dim wanted As Integer = _activeView.RequiredHeight()
        If wanted <= 0 Then Return
        PerformLayout()
        Dim frame As Integer = ClientSize.Height - viewHost.Height
        Dim target As Integer = Math.Max(MinHeight, frame + wanted)
        If ClientSize.Height = target Then Return
        ClientSize = New Size(ClientSize.Width, target)
        PerformLayout()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' OPENING
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Opens the menu under a rectangle inside the host (client coordinates) -- the filter icon
    ''' that was pressed. When it does not fit below or to the right, it flips over the other two
    ''' sides of the icon, like any system menu.
    ''' </summary>
    Friend Sub ShowBelow(anchor As Control, anchorRect As Rectangle)
        Try
            ArgumentNullException.ThrowIfNull(anchor)
            AdjustHeight()

            Dim top As Point = anchor.PointToScreen(New Point(anchorRect.Left, anchorRect.Top))
            Location = LocationRelativeTo(top, anchorRect)

            Dim before As Integer = Height
            Show(anchor.FindForm())

            ' The theme applies only NOW (KBotThemedForm.OnLoad), and with it the fixed rows get
            ' the scheme's measure -- on Modern the menu can come out taller than the one placed
            ' two lines above. Re-checked once: a menu that runs under the bottom edge of the
            ' screen cannot be read to the end.
            If Height <> before Then Location = LocationRelativeTo(top, anchorRect)

            Activate()
            If _activeView IsNot Nothing Then _activeView.Activated()
        Catch ex As Exception
            ' Entry point (window creation, screen geometry) => log and RE-THROW.
            GlobalErrorLog.Write("KBotFilterPopup.ShowBelow", ex)
            Throw
        End Try
    End Sub

    ' The menu's top-left corner relative to the pressed icon (SCREEN coordinates). When it does
    ' not fit below or to the right, it flips over the other two sides of the icon.
    Private Function LocationRelativeTo(topScreen As Point, anchorRect As Rectangle) As Point
        Dim at As New Point(topScreen.X, topScreen.Y + anchorRect.Height)
        Dim area As Rectangle = Screen.FromPoint(at).WorkingArea
        If at.X + Width > area.Right Then at.X = Math.Max(area.Left, topScreen.X + anchorRect.Width - Width)
        If at.Y + Height > area.Bottom Then at.Y = Math.Max(area.Top, topScreen.Y - Height)
        Return at
    End Function

    Protected Overrides Sub OnDeactivate(e As EventArgs)
        Try
            MyBase.OnDeactivate(e)
            ' While a child is open (the conditions submenu), losing activation does not mean the
            ' operator clicked elsewhere -- it means they are looking at the child.
            If _suppressDeactivate OrElse _closing Then Return
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnDeactivate", ex)
        End Try
    End Sub

    ' Esc closes leaving nothing behind; Enter hands the filter over. KeyPreview is set in the
    ' designer, so the two keys work whatever control has the focus.
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Try
            MyBase.OnKeyDown(e)
            Select Case e.KeyCode
                Case Keys.Escape
                    e.SuppressKeyPress = True
                    Close()
                Case Keys.Enter
                    e.SuppressKeyPress = True
                    ' Enter confirms the FILTER. On the other two tabs there is nothing to
                    ' confirm (they applied already), so the key just closes the menu.
                    If String.Equals(_activeKey, "filtrare", StringComparison.Ordinal) Then
                        AcceptFilter()
                    Else
                        Close()
                    End If
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.OnKeyDown", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' WHAT THE TABS ASK FOR
    ' ══════════════════════════════════════════════════════════════════════════

    ' Applies the sort asked for and closes -- sorting is a command, not a choice to confirm.
    Private Sub SortView_SortRequested(direction As KBotSortDirection)
        Try
            _closing = True
            RaiseEvent SortRequested(Me, New KBotSortRequestedEventArgs(_columnKey, direction))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.SortView_SortRequested", ex)
        End Try
    End Sub

    ' «Sterge filtrul»: an inactive filter goes out, i.e. «lift the filter», and the menu closes.
    Private Sub FilterView_ClearRequested()
        Try
            _closing = True
            RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(New KBotColumnFilter(_columnKey)))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.FilterView_ClearRequested", ex)
        End Try
    End Sub

    Private Sub FilterView_ConditionMenuRequested(anchorScreen As Rectangle)
        Try
            ' The submenu anchors in the BODY's coordinates: the tab handed the row in screen
            ' coordinates because its own Bounds are relative to its table, not to this window.
            OpenConditionMenu(pnlBody.RectangleToClient(anchorScreen))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.FilterView_ConditionMenuRequested", ex)
        End Try
    End Sub

    ' Hands the grouping over and STAYS open (see the class summary); the tab refreshes its own
    ' hierarchy and then asks for a re-measure through ContentChanged.
    Private Sub GroupView_GroupingRequested(level As KBotGroupLevel)
        Try
            RaiseEvent GroupingRequested(Me, New KBotGroupingRequestedEventArgs(_columnKey, level))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.GroupView_GroupingRequested", ex)
        End Try
    End Sub

    ' A list changed length (search, a level added): the window follows.
    Private Sub View_ContentChanged()
        Try
            AdjustHeight()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.View_ContentChanged", ex)
        End Try
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        AcceptFilter()
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Try
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.BtnCancel_Click", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' THE FILTER
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The filter an OK pressed NOW would hand over (<see cref="KBotFilterPopupFilterView.BuildFilter"/>).
    ''' Separate from <see cref="AcceptFilter"/> so the rule can be checked without a screen --
    ''' the menu is a window, its decisions are not.
    ''' </summary>
    Friend Function BuildFilter() As KBotColumnFilter
        Return FilterView.BuildFilter()
    End Function

    ''' <summary>
    ''' The level the grouping tab would hand over NOW (<c>Nothing</c> = the column no longer
    ''' groups) -- <see cref="KBotFilterPopupGroupView.BuildGroupLevel"/>.
    ''' </summary>
    Friend Function BuildGroupLevel() As KBotGroupLevel
        Return GroupView.BuildGroupLevel()
    End Function

    ''' <summary>What a value's row SAYS -- <see cref="KBotFilterPopupFilterView.ValueLabel"/>.</summary>
    Friend Shared Function EtichetaValorii(value As String) As String
        Return KBotFilterPopupFilterView.ValueLabel(value)
    End Function

    ' Hands over the filter built from the current state and closes.
    Private Sub AcceptFilter()
        Try
            _closing = True
            RaiseEvent FilterAccepted(Me, New KBotFilterAcceptedEventArgs(BuildFilter()))
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopup.AcceptFilter", ex)
        End Try
    End Sub

    ' Opens the conditions submenu. While it is up, deactivate does NOT close the parent menu.
    Private Sub OpenConditionMenu(anchorRow As Rectangle)
        Dim operators As KBotFilterOperator() = KBotFilterEngine.AllowedOperators(_valueType)
        If operators.Length = 0 Then Return

        Dim menu As New CustomPopup()
        For Each op In operators
            menu.Items.Add(New CustomPopupItem(op.ToString(), KBotFilterEngine.OperatorCaption(op, _valueType)))
        Next

        _suppressDeactivate = True
        AddHandler menu.ItemClicked,
            Sub(s As Object, ev As CustomPopupItemEventArgs)
                Dim chosen As KBotFilterOperator
                If Not [Enum].TryParse(Of KBotFilterOperator)(ev.Item.Key, chosen) Then Return
                ' The submenu goes AWAY FIRST. CustomPopup raises ItemClicked BEFORE Close (see
                ' CustomPopup.CloseWith) and the condition dialog is modal: without the line
                ' below the menu would stay on screen, alive and useless, until the dialog
                ' closed. Its Close comes anyway, as soon as we return from here.
                menu.Hide()
                ApplyCondition(chosen)
            End Sub
        AddHandler menu.FormClosed,
            Sub(s As Object, ev As FormClosedEventArgs)
                _suppressDeactivate = False
                ' If the choice in the submenu did not close the parent menu (the operator
                ' pressed Esc), the focus comes back here -- otherwise a visible, dead window
                ' would remain.
                If Not _closing AndAlso Not IsDisposed Then Activate()
            End Sub

        ' The anchor is in the BODY's coordinates (see FilterView_ConditionMenuRequested).
        menu.ShowBelow(pnlBody, anchorRow)
    End Sub

    ' Asks for the operands (if it has any) and puts the condition on the working filter.
    Private Sub ApplyCondition(op As KBotFilterOperator)
        If KBotFilterEngine.OperandCount(op) = 0 Then
            FilterView.SetCondition(op, Nothing, Nothing)
            AcceptFilter()
            Return
        End If

        ' The dialog is MODAL, so the menu steps aside first: two windows on top of each other,
        ' one of them asking for a value, are one window more than the operator asked for.
        ' The guard goes BEFORE Hide: hiding the active window moves activation elsewhere,
        ' i.e. raises OnDeactivate -- which would otherwise close the menu right now.
        _suppressDeactivate = True
        Hide()
        Dim dlg As New KBotFilterConditionDialog(op, _valueType, _columnCaption,
                                                 FilterView.Operand1, FilterView.Operand2,
                                                 _dateOperandFormat)
        Try
            If dlg.ShowDialog(Owner) = DialogResult.OK Then
                FilterView.SetCondition(op, dlg.Operand1, dlg.Operand2)
                AcceptFilter()
            Else
                _closing = True
                Close()
            End If
        Finally
            dlg.Dispose()
            _suppressDeactivate = False
        End Try
    End Sub

    ' ── Headless check gates (the house Debug* convention) ────────────────────

    ''' <summary>How many distinct values the list shows (after the search).</summary>
    Friend Function DebugShownCount() As Integer
        Return FilterView.DebugShownCount()
    End Function

    ''' <summary>How many values are ticked now.</summary>
    Friend Function DebugCheckedCount() As Integer
        Return FilterView.DebugCheckedCount()
    End Function

    ''' <summary>Toggles a value's tick by its TEXT -- the path a click would take.</summary>
    Friend Sub DebugToggleValue(displayText As String)
        FilterView.DebugToggleValue(displayText)
    End Sub

    ''' <summary>Types into the search box, as the operator would.</summary>
    Friend Sub DebugSearch(text As String)
        FilterView.DebugSearch(text)
    End Sub

    ''' <summary>Switches to a tab, like a click on the top bar.</summary>
    Friend Sub DebugSelectTab(key As String)
        navFile.SelectedKey = key
    End Sub

    ''' <summary>What the rows of the hierarchy list say (the grouping tab).</summary>
    Friend Function DebugLevelLines() As String()
        Return GroupView.DebugLevelLines()
    End Function

    ''' <summary>Lays the geometry out and returns the size the window came out at.</summary>
    Friend Function DebugMeasure() As Size
        AdjustHeight()
        PerformLayout()
        Return Size
    End Function
End Class

''' <summary>The arguments of <c>KBotFilterPopup.FilterAccepted</c>.</summary>
Friend NotInheritable Class KBotFilterAcceptedEventArgs
    Inherits EventArgs

    Public Sub New(filter As KBotColumnFilter)
        Me.Filter = filter
    End Sub

    ''' <summary>The filter to put on the column; inactive means «lift the filter».</summary>
    Public ReadOnly Property Filter As KBotColumnFilter

End Class

''' <summary>The arguments of <c>KBotFilterPopup.SortRequested</c>.</summary>
Friend NotInheritable Class KBotSortRequestedEventArgs
    Inherits EventArgs

    Public Sub New(columnKey As String, direction As KBotSortDirection)
        Me.ColumnKey = columnKey
        Me.Direction = direction
    End Sub

    Public ReadOnly Property ColumnKey As String
    Public ReadOnly Property Direction As KBotSortDirection

End Class

''' <summary>The arguments of <c>KBotFilterPopup.GroupingRequested</c> (slice 0030).</summary>
Friend NotInheritable Class KBotGroupingRequestedEventArgs
    Inherits EventArgs

    Public Sub New(columnKey As String, level As KBotGroupLevel)
        Me.ColumnKey = columnKey
        Me.Level = level
    End Sub

    ''' <summary>The column whose grouping changed.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>The level to put on the column; <c>Nothing</c> = the column no longer groups.</summary>
    Public ReadOnly Property Level As KBotGroupLevel

End Class
