Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The «Grupare» tab of the column menu (slice 0030): the level options of slice 0029 for THIS
''' column -- group on/off, direction, header and footer bands, aggregates in the header,
''' collapsible, start collapsed -- plus the grid's hierarchy of levels underneath.
'''
''' <para><b>Grouping applies at once but does NOT close the menu.</b> It is a command, like
''' sorting (the grid rearranges on the spot and the operator sees the result behind), only it
''' has seven options: a tab that closed at the first tick would have to be reopened six
''' times. Every touch goes out as <see cref="GroupingRequested"/> with the level built from
''' the controls; the popup hands it to the grid and this tab then refreshes its own
''' hierarchy, so the level just added is visible -- otherwise the operator has no
''' confirmation.</para>
'''
''' <para>The grid is read ONLY (the level on this column and the active levels, through
''' <see cref="KBotDataView.GroupLevelFor"/> / <see cref="KBotDataView.ActiveLevels"/>);
''' nothing is written through it. <c>Nothing</c> = no host (tests, the harness) = an empty
''' hierarchy.</para>
''' </summary>
Friend Class KBotFilterPopupGroupView
    Implements IKBotFilterMenuView, IThemedContainer

    Private ReadOnly _columnKey As String
    Private ReadOnly _grid As KBotDataView

    ' While True, control events are our own echo, not the operator's.
    Private _syncing As Boolean = False

    ''' <summary>The operator changed the column's grouping; <c>Nothing</c> = the column no longer groups.</summary>
    Friend Event GroupingRequested(level As KBotGroupLevel)

    ''' <summary>The hierarchy list changed length, so the host must re-measure the window.</summary>
    Friend Event ContentChanged()

    ''' <summary>
    ''' Builds the tab for a column from the REAL state of the grid: the level on this column
    ''' (if any) and the hierarchy of levels, in order. Nothing shown here is written in the
    ''' designer except the controls -- the keys, captions and directions are the grid's.
    ''' </summary>
    Friend Sub New(columnKey As String, columnCaption As String, grid As KBotDataView)
        InitializeComponent()
        _columnKey = columnKey
        _grid = grid

        _syncing = True
        Try
            chkGroupBy.Text = $"Grupează după «{If(columnCaption, String.Empty)}»"

            Dim n As KBotGroupLevel = If(_grid Is Nothing, Nothing, _grid.GroupLevelFor(_columnKey))
            chkGroupBy.Checked = n IsNot Nothing
            rbGroupDesc.Checked = n IsNot Nothing AndAlso n.SortDirection = KBotSortDirection.Descending
            rbGroupAsc.Checked = Not rbGroupDesc.Checked
            chkGroupHeader.Checked = If(n Is Nothing, True, n.ShowHeader)
            chkGroupFooter.Checked = If(n Is Nothing, True, n.ShowFooter)
            chkGroupHeaderAggregates.Checked = If(n Is Nothing, False, n.ShowHeaderAggregates)
            chkGroupCollapsible.Checked = If(n Is Nothing, True, n.Collapsible)
            chkGroupStartCollapsed.Checked = If(n Is Nothing, False, n.CollapsedByDefault)

            RefreshLevels()
            EnableOptions()
        Finally
            _syncing = False
        End Try
    End Sub

    Public ReadOnly Property ViewKey As String Implements IKBotFilterMenuView.ViewKey
        Get
            Return "grupare"
        End Get
    End Property

    Public ReadOnly Property ShowsCommandBar As Boolean Implements IKBotFilterMenuView.ShowsCommandBar
        Get
            Return False
        End Get
    End Property

    Public Sub Activated() Implements IKBotFilterMenuView.Activated
        ' Nothing to focus: every option applies the moment it is ticked.
    End Sub

    Public Function RequiredHeight() As Integer Implements IKBotFilterMenuView.RequiredHeight
        ' The fixed rows take the scheme's measure first (its padding and font); measuring
        ' before that would size the window on rows that change right after.
        tlyGroup.RefitToTheme()
        PerformLayout()
        Return KBotFilterMenuLayout.FixedRowsHeight(tlyGroup) +
               KBotFilterMenuLayout.ListHeight(lstLevels, lstLevels.Items.Count)
    End Function

    ''' <summary>
    ''' The level this tab would hand over NOW (<c>Nothing</c> = the column no longer groups).
    ''' Separate from the request path so the rule can be checked without a screen.
    '''
    ''' <para>The EXISTING level is reused, not replaced with a new one: colours and fonts put
    ''' from the designer may sit on it (<c>HeaderBackColor</c>, <c>FooterFont</c>...), and a
    ''' tick in the menu must not wipe them.</para>
    ''' </summary>
    Friend Function BuildGroupLevel() As KBotGroupLevel
        If Not chkGroupBy.Checked Then Return Nothing

        Dim n As KBotGroupLevel = If(_grid Is Nothing, Nothing, _grid.GroupLevelFor(_columnKey))
        If n Is Nothing Then n = New KBotGroupLevel()

        n.ColumnKey = _columnKey
        n.SortDirection = If(rbGroupDesc.Checked, KBotSortDirection.Descending, KBotSortDirection.Ascending)
        n.ShowHeader = chkGroupHeader.Checked
        n.ShowFooter = chkGroupFooter.Checked
        n.ShowHeaderAggregates = chkGroupHeaderAggregates.Checked
        n.Collapsible = chkGroupCollapsible.Checked
        n.CollapsedByDefault = chkGroupStartCollapsed.Checked
        Return n
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' THEME
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The semantic colours the generic per-type rules cannot know: the surfaces, the
    ''' separator (a Panel, because the Label rule would make a 1px line transparent), the
    ''' hierarchy list that continues the menu surface and its dimmed caption. The rest comes
    ''' from <c>ThemeManager.Apply</c>.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyGroup.BackColor = p.SurfaceAltColor
            pnlGroupDirection.BackColor = p.SurfaceAltColor
            sepGroup.BackColor = p.BorderColor
            lstLevels.BackColor = p.SurfaceAltColor
            lstLevels.ForeColor = p.TextColor
            lblLevels.ForeColor = p.TextDimColor
        Catch ex As Exception
            ' Theme boundary: log and swallow -- a throw here would break the scheme switch.
            GlobalErrorLog.Write("KBotFilterPopupGroupView.ApplyTheme", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' THE HIERARCHY
    ' ══════════════════════════════════════════════════════════════════════════

    ' The grid's hierarchy, one line per level, with the current column marked -- otherwise the
    ' operator cannot tell on which floor what they just ticked landed.
    Private Sub RefreshLevels()
        lstLevels.BeginUpdate()
        Try
            lstLevels.Items.Clear()
            If _grid Is Nothing Then Return
            Dim levels As IReadOnlyList(Of KBotGroupLevel) = _grid.ActiveLevels()
            If levels.Count = 0 Then
                lstLevels.Items.Add("(grila nu e grupată)")
                Return
            End If
            For i As Integer = 0 To levels.Count - 1
                Dim lv As KBotGroupLevel = levels(i)
                Dim caption As String = ColumnCaption(lv.ColumnKey)
                Dim direction As String = If(lv.SortDirection = KBotSortDirection.Descending,
                                             "descrescător", "crescător")
                Dim here As String = If(String.Equals(lv.ColumnKey, _columnKey, StringComparison.Ordinal),
                                        "   ← coloana aceasta", String.Empty)
                lstLevels.Items.Add($"{i + 1}. {caption} ({direction}){here}")
            Next
        Finally
            lstLevels.EndUpdate()
        End Try
    End Sub

    ' The column's caption in the grid; a key without a caption stays a key (better than a blank row).
    Private Function ColumnCaption(colKey As String) As String
        Try
            Dim col As KBotDataColumn = _grid.Column(colKey)
            If Not String.IsNullOrWhiteSpace(col.HeaderText) Then Return col.HeaderText
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupGroupView.ColumnCaption", ex)
        End Try
        Return colKey
    End Function

    ' Without grouping on the column its options have nothing to talk about: they dim, they do
    ' not hide -- a row that disappears and reappears at every tick makes the tab jump under the cursor.
    Private Sub EnableOptions()
        Dim grouped As Boolean = chkGroupBy.Checked
        rbGroupAsc.Enabled = grouped
        rbGroupDesc.Enabled = grouped
        chkGroupHeader.Enabled = grouped
        chkGroupFooter.Enabled = grouped
        chkGroupHeaderAggregates.Enabled = grouped AndAlso chkGroupHeader.Checked
        chkGroupCollapsible.Enabled = grouped AndAlso chkGroupHeader.Checked
        chkGroupStartCollapsed.Enabled = chkGroupCollapsible.Enabled AndAlso chkGroupCollapsible.Checked
    End Sub

    ' Any touch on the tab asks for the same work: build the level from the controls and hand
    ' it over. One handler for all seven -- seven identical handlers would be seven places to
    ' forget a line in.
    Private Sub Options_Changed(sender As Object, e As EventArgs) _
        Handles chkGroupBy.CheckedChanged, rbGroupAsc.CheckedChanged, rbGroupDesc.CheckedChanged,
                chkGroupHeader.CheckedChanged, chkGroupFooter.CheckedChanged,
                chkGroupHeaderAggregates.CheckedChanged, chkGroupCollapsible.CheckedChanged,
                chkGroupStartCollapsed.CheckedChanged
        Try
            If _syncing Then Return
            EnableOptions()
            RequestGrouping()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupGroupView.Options_Changed", ex)
        End Try
    End Sub

    ' Hands the grouping over and STAYS open (see the class summary), then rebuilds its own
    ' hierarchy: the level just added must show in the list.
    Private Sub RequestGrouping()
        RaiseEvent GroupingRequested(BuildGroupLevel())
        _syncing = True
        Try
            RefreshLevels()
        Finally
            _syncing = False
        End Try
        RaiseEvent ContentChanged()
    End Sub

    ' ── Headless check gates (the house Debug* convention) ────────────────────

    ''' <summary>What the rows of the hierarchy list say.</summary>
    Friend Function DebugLevelLines() As String()
        Dim lines(lstLevels.Items.Count - 1) As String
        For i As Integer = 0 To lstLevels.Items.Count - 1
            lines(i) = CStr(lstLevels.Items(i))
        Next
        Return lines
    End Function

End Class
