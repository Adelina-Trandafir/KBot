Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The «Filtrare» tab of the column menu: the list of tickable values with «(Selecteaza tot)»
''' and a search box above it, the CONDITIONS row («Filtre text / numerice / de data») and
''' «Sterge filtrul».
'''
''' <para><b>The filter is handed over at OK, by the popup.</b> This tab works on a COPY of the
''' column filter (<see cref="KBotColumnFilter.Clone"/>) and only ever answers
''' <see cref="BuildFilter"/>; «Anuleaza» and Esc leave nothing behind. Two rows are commands
''' in their own right and go out as events: «Sterge filtrul» (<see cref="ClearRequested"/>)
''' and the conditions row (<see cref="ConditionMenuRequested"/>) -- the submenu and the modal
''' operand dialog need the popup's own window (hide, suppress deactivate, owner), so the
''' popup opens them and writes the result back through <see cref="SetCondition"/>.</para>
'''
''' <para><b>What is set at construction, not in the designer:</b> the caption of «Sterge
''' filtrul» (it names the column), whether the conditions row exists at all (logical columns
''' have no submenu -- the two boxes in the list already say everything there is to say about a
''' tick) and the CONTENT of the list. The controls that show them are the designer's.</para>
''' </summary>
Friend Class KBotFilterPopupFilterView
    Implements IKBotFilterMenuView, IThemedContainer

    ' The row of the conditions button in tlyFilter -- collapsed to zero on logical columns.
    Private Const ConditionsRow As Integer = 4

    Private ReadOnly _columnKey As String
    Private ReadOnly _values As New List(Of String)()          ' distinct display texts, in order
    Private ReadOnly _checked As HashSet(Of String)
    Private ReadOnly _working As KBotColumnFilter

    ' The indices of _values that pass the search -- exactly what lstValues shows, same order.
    Private ReadOnly _shown As New List(Of Integer)()

    ' While True, control events are our own echo, not the operator's.
    Private _syncing As Boolean = False

    ''' <summary>The operator clicked «Sterge filtrul»: the column filter goes away and the menu closes.</summary>
    Friend Event ClearRequested()

    ''' <summary>
    ''' The operator clicked the conditions row; the argument is the row's rectangle in SCREEN
    ''' coordinates, for the host to anchor the submenu under.
    ''' </summary>
    Friend Event ConditionMenuRequested(anchorScreen As Rectangle)

    ''' <summary>The list changed length (search), so the host must re-measure the window.</summary>
    Friend Event ContentChanged()

    ''' <summary>
    ''' Builds the tab for a column: the displayed caption, the type of the values, the distinct
    ''' values (already formatted, in sort order) and the current filter (<c>Nothing</c> = none).
    ''' </summary>
    Friend Sub New(columnKey As String, columnCaption As String, valueType As KBotValueType,
                   distinctValues As IEnumerable(Of String), currentFilter As KBotColumnFilter)
        InitializeComponent()

        _columnKey = columnKey
        If distinctValues IsNot Nothing Then _values.AddRange(distinctValues)
        _working = If(currentFilter Is Nothing, New KBotColumnFilter(columnKey), currentFilter.Clone())

        ' Ticks start from the existing filter; without one, everything is ticked -- that is the
        ' "unfiltered" state, not an empty one the operator would have to repair with «Selecteaza tot».
        If _working.SelectedValues Is Nothing Then
            _checked = New HashSet(Of String)(_values, StringComparer.CurrentCultureIgnoreCase)
        Else
            _checked = New HashSet(Of String)(_working.SelectedValues, StringComparer.CurrentCultureIgnoreCase)
        End If

        'btnClearFilter.Text = $"Șterge filtrul din «{If(columnCaption, String.Empty)}»"
        btnClearFilter.Enabled = _working.IsActive

        ' The button HIDES and its row collapses to zero -- otherwise an empty band would stay
        ' in the middle of the tab (see KBotFilterEngine.AllowedOperators).
        Dim hasConditions As Boolean = KBotFilterEngine.AllowedOperators(valueType).Length > 0
        btnConditions.Visible = hasConditions
        tlyFilter.SetRowCollapsed(ConditionsRow, Not hasConditions)
        If hasConditions Then btnConditions.Text = KBotFilterEngine.ConditionMenuCaption(valueType) & "  ▸"

        RebuildShown()
    End Sub

    Public ReadOnly Property ViewKey As String Implements IKBotFilterMenuView.ViewKey
        Get
            Return "filtrare"
        End Get
    End Property

    Public ReadOnly Property ShowsCommandBar As Boolean Implements IKBotFilterMenuView.ShowsCommandBar
        Get
            Return True
        End Get
    End Property

    ''' <summary>The first operand of the working condition (for the operand dialog to start from).</summary>
    Friend ReadOnly Property Operand1 As String
        Get
            Return _working.Operand1
        End Get
    End Property

    ''' <summary>The second operand of the working condition (for the operand dialog to start from).</summary>
    Friend ReadOnly Property Operand2 As String
        Get
            Return _working.Operand2
        End Get
    End Property

    Public Sub Activated() Implements IKBotFilterMenuView.Activated
        txtSearch.Focus()
    End Sub

    Public Function RequiredHeight() As Integer Implements IKBotFilterMenuView.RequiredHeight
        ' The fixed rows take the scheme's measure first (its padding and font); measuring
        ' before that would size the window on rows that change right after.
        tlyFilter.RefitToTheme()
        PerformLayout()
        Return KBotFilterMenuLayout.FixedRowsHeight(tlyFilter) +
               KBotFilterMenuLayout.ListHeight(lstValues, _shown.Count)
    End Function

    ''' <summary>
    ''' What a value's row SAYS. The empty value has a label of its own -- a completely blank
    ''' row looks like a broken row, and the operator must be able to tick the blank cells.
    ''' </summary>
    Friend Shared Function ValueLabel(value As String) As String
        If String.IsNullOrEmpty(value) Then Return "(Necompletate)"
        Return value
    End Function

    ''' <summary>
    ''' The filter an OK pressed NOW would hand over. Separate from the popup's accept path so
    ''' the rule below can be checked without a screen -- the menu is a window, its decisions
    ''' are not.
    ''' </summary>
    Friend Function BuildFilter() As KBotColumnFilter
        Dim result As New KBotColumnFilter(_columnKey) With {
            .Condition = _working.Condition,
            .Operand1 = _working.Operand1,
            .Operand2 = _working.Operand2}

        ' ALL values ticked = no list restriction. Without this rule an "all ticked" filter
        ' would stay active forever and the header would show the column as filtered for nothing.
        If _checked.Count < _values.Count Then
            result.SelectedValues = New HashSet(Of String)(_checked, StringComparer.CurrentCultureIgnoreCase)
        End If

        Return result
    End Function

    ''' <summary>Puts a condition (and its operands) on the working filter; the popup hands it over.</summary>
    Friend Sub SetCondition(op As KBotFilterOperator, operand1 As String, operand2 As String)
        _working.Condition = op
        _working.Operand1 = operand1
        _working.Operand2 = operand2
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' THEME
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The semantic colours the generic per-type rules cannot know: the surface, the separator
    ''' (a Panel, because the Label rule would make a 1px line transparent), the list that
    ''' continues the menu surface, and the two menu rows. The red of «Sterge filtrul» comes
    ''' from the palette, not the designer: it is the warning colour of the active scheme, not a
    ''' Firebrick written once. The rest comes from <c>ThemeManager.Apply</c>.
    ''' </summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyFilter.BackColor = p.SurfaceAltColor
            picSearch.BackColor = p.SurfaceAltColor
            sepFilter.BackColor = p.BorderColor
            lstValues.BackColor = p.SurfaceAltColor
            lstValues.ForeColor = p.TextColor
            KBotFilterMenuLayout.ApplyMenuRow(btnConditions, p, p.TextColor)
            KBotFilterMenuLayout.ApplyMenuRow(btnClearFilter, p, p.ErrorColor)
        Catch ex As Exception
            ' Theme boundary: log and swallow -- a throw here would break the scheme switch.
            GlobalErrorLog.Write("KBotFilterPopupFilterView.ApplyTheme", ex)
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' THE LIST
    ' ══════════════════════════════════════════════════════════════════════════

    ' Fills the list with the values that pass the search (all, when empty) and refreshes the ticks.
    Private Sub RebuildShown()
        _shown.Clear()
        Dim needle As String = txtSearch.Text.Trim()
        For i As Integer = 0 To _values.Count - 1
            If needle.Length = 0 OrElse
               ValueLabel(_values(i)).Contains(needle, StringComparison.CurrentCultureIgnoreCase) Then
                _shown.Add(i)
            End If
        Next

        _syncing = True
        Try
            lstValues.BeginUpdate()
            lstValues.Items.Clear()
            For Each i In _shown
                lstValues.Items.Add(ValueLabel(_values(i)), _checked.Contains(_values(i)))
            Next
            lstValues.EndUpdate()
        Finally
            _syncing = False
        End Try

        UpdateSelectAll()
    End Sub

    ' The top box shows the state of the SHOWN values: all / none / some (the third state).
    Private Sub UpdateSelectAll()
        Dim ticked As Integer = 0
        For Each i In _shown
            If _checked.Contains(_values(i)) Then ticked += 1
        Next

        _syncing = True
        Try
            If _shown.Count > 0 AndAlso ticked = _shown.Count Then
                chkSelectAll.CheckState = CheckState.Checked
            ElseIf ticked = 0 Then
                chkSelectAll.CheckState = CheckState.Unchecked
            Else
                chkSelectAll.CheckState = CheckState.Indeterminate
            End If
        Finally
            _syncing = False
        End Try
    End Sub

    ' Ticks / unticks all SHOWN values (the ones that pass the search). Over a searched list, a
    ' «Selecteaza tot» that also touched the unseen values would do more than what is on screen.
    Private Sub ToggleAll()
        Dim allTicked As Boolean = AllShownChecked()
        For Each i In _shown
            If allTicked Then
                _checked.Remove(_values(i))
            Else
                _checked.Add(_values(i))
            End If
        Next
        SyncListChecks()
        UpdateSelectAll()
    End Sub

    Private Function AllShownChecked() As Boolean
        For Each i In _shown
            If Not _checked.Contains(_values(i)) Then Return False
        Next
        Return _shown.Count > 0
    End Function

    ' Puts the list's ticks on the model's state (without going through the operator's ItemCheck).
    Private Sub SyncListChecks()
        _syncing = True
        Try
            For pos As Integer = 0 To _shown.Count - 1
                lstValues.SetItemChecked(pos, _checked.Contains(_values(_shown(pos))))
            Next
        Finally
            _syncing = False
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' CONTROL EVENTS
    ' ══════════════════════════════════════════════════════════════════════════

    Private Sub TxtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        Try
            RebuildShown()
            RaiseEvent ContentChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupFilterView.TxtSearch_TextChanged", ex)
        End Try
    End Sub

    Private Sub ChkSelectAll_Click(sender As Object, e As EventArgs) Handles chkSelectAll.Click
        Try
            If _syncing Then Return
            ToggleAll()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupFilterView.ChkSelectAll_Click", ex)
        End Try
    End Sub

    ' ItemCheck comes BEFORE the list changes its state, so e.NewValue is read, not the tick.
    '
    ' The «Handles» clause below is NOT decorative and must not disappear: without it the ticks
    ' put with the mouse never reach _checked. The menu looks right, but the filter handed over
    ' at OK is the one from before any click -- and "untick all, then tick one" (the usual path)
    ' hands over an EMPTY set, i.e. an empty grid. It was lost once exactly like that, in a rename.
    Private Sub LstValues_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstValues.ItemCheck
        Try
            If _syncing Then Return
            If e.Index < 0 OrElse e.Index >= _shown.Count Then Return
            Dim v As String = _values(_shown(e.Index))
            If e.NewValue = CheckState.Checked Then
                _checked.Add(v)
            Else
                _checked.Remove(v)
            End If
            UpdateSelectAll()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupFilterView.LstValues_ItemCheck", ex)
        End Try
    End Sub

    Private Sub BtnConditions_Click(sender As Object, e As EventArgs) Handles btnConditions.Click
        Try
            ' The anchor goes out in SCREEN coordinates: the button sits three parents down
            ' (table, tab, host), so its Bounds are relative to the table, not to the popup.
            RaiseEvent ConditionMenuRequested(btnConditions.Parent.RectangleToScreen(btnConditions.Bounds))
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupFilterView.BtnConditions_Click", ex)
        End Try
    End Sub

    Private Sub BtnClearFilter_Click(sender As Object, e As EventArgs) Handles btnClearFilter.Click
        Try
            RaiseEvent ClearRequested()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotFilterPopupFilterView.BtnClearFilter_Click", ex)
        End Try
    End Sub

    ' ── Headless check gates (the house Debug* convention) ────────────────────

    ''' <summary>How many distinct values the list shows (after the search).</summary>
    Friend Function DebugShownCount() As Integer
        Return _shown.Count
    End Function

    ''' <summary>How many values are ticked now.</summary>
    Friend Function DebugCheckedCount() As Integer
        Return _checked.Count
    End Function

    ''' <summary>Toggles a value's tick by its TEXT -- the path a click would take.</summary>
    Friend Sub DebugToggleValue(displayText As String)
        Dim i As Integer = _values.IndexOf(displayText)
        If i < 0 Then Throw New ArgumentException($"Valoare inexistentă în listă: «{displayText}».", NameOf(displayText))
        Dim pos As Integer = _shown.IndexOf(i)
        If pos < 0 Then Throw New ArgumentException($"Valoarea «{displayText}» nu e în lista arătată acum.", NameOf(displayText))
        Dim v As String = _values(i)
        If Not _checked.Remove(v) Then
            _checked.Add(v)
        End If
        SyncListChecks()
        UpdateSelectAll()
    End Sub

    ''' <summary>Types into the search box, as the operator would.</summary>
    Friend Sub DebugSearch(text As String)
        txtSearch.Text = text
    End Sub

End Class
