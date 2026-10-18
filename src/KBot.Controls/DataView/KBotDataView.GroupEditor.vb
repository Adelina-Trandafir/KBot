Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

' Slice KE11 -- a combo in the HEADER band of a group (KBotDataColumn.GroupHeaderCombo): it shows the value the rows of the group have in that column
' (empty when they differ) and, picked, hands the choice to the host through GroupHeaderValuePicked so it can put it on every row of the group.
' One floating combo (groupCombo, declared in the Designer file) serves all the headers; it is open only while a header is being edited.
Partial Class KBotDataView

    ''' <summary>The operator picked a value in the combo of a group header; the host writes it in the rows named by the arguments.</summary>
    Public Event GroupHeaderValuePicked As EventHandler(Of KBotGroupHeaderValueEventArgs)

    Private _groupEditGroup As Integer = -1
    Private _groupEditColumn As String = String.Empty
    Private _groupEditCell As Rectangle
    Private _suppressGroupEvents As Boolean

    ' Wires the floating combo (from the constructor).
    Private Sub WireGroupEditor()
        groupCombo.CellEditorMode = True
        groupCombo.Editable = True
        groupCombo.FindAsYouType = True
        groupCombo.LimitToList = False
        groupCombo.DropDownAnchorProvider = AddressOf GroupEditCellScreenRect
        AddHandler groupCombo.ItemPicked, AddressOf OnGroupComboPicked
        AddHandler groupCombo.KeyDown, AddressOf OnGroupComboKeyDown
        AddHandler groupCombo.Leave, AddressOf OnGroupComboLeave
    End Sub

    Private Function GroupEditCellScreenRect() As Rectangle
        Return RectangleToScreen(_groupEditCell)
    End Function

    ' ── Reading ─────────────────────────────────────────────────────────────────

    ''' <summary>The model indexes of every row of a group, in drawn order.</summary>
    Private Function GroupModelRows(k_groupIndex As Integer) As List(Of Integer)
        Dim k_list As New List(Of Integer)()
        Dim k_node As KBotGroupNode = GroupAt(k_groupIndex)
        If k_node Is Nothing Then Return k_list
        For k_pos As Integer = k_node.FirstViewPos To k_node.LastViewPos
            k_list.Add(ModelIndexAt(k_pos))
        Next
        Return k_list
    End Function

    ' The text the rows of the group share in the column, else empty.
    Private Function GroupHeaderComboText(k_node As KBotGroupNode, k_col As KBotDataColumn) As String
        Dim k_text As String = Nothing
        For k_pos As Integer = k_node.FirstViewPos To k_node.LastViewPos
            Dim k_one As String = FormatValue(_rows(ModelIndexAt(k_pos))(k_col.Key), k_col)
            If k_text Is Nothing Then
                k_text = k_one
            ElseIf Not String.Equals(k_text, k_one, StringComparison.Ordinal) Then
                Return String.Empty
            End If
        Next
        Return If(k_text, String.Empty)
    End Function

    Private Shared Function HasGroupHeaderCombo(k_col As KBotDataColumn) As Boolean
        Return k_col.GroupHeaderCombo AndAlso k_col.ColumnType = KBotColumnType.Combo
    End Function

    ' The cell of a column inside a header band, in client coordinates.
    Private Function GroupHeaderCellRect(k_bandIndex As Integer, k_col As KBotDataColumn) As Rectangle
        Dim k_band As KBotBand = BandAt(k_bandIndex)
        Dim k_y As Integer = HeaderBandHeight() + k_band.Top - VScrollOffset()
        For Each k_cl In _frozenLayout
            If ReferenceEquals(k_cl.Column, k_col) Then Return New Rectangle(k_cl.X, k_y, k_col.WidthPx, k_band.Height)
        Next
        Dim k_hOffset As Integer = HScrollOffset()
        For Each k_cl In _scrollLayout
            If ReferenceEquals(k_cl.Column, k_col) Then Return New Rectangle(_frozenBandWidth + k_cl.X - k_hOffset, k_y, k_col.WidthPx, k_band.Height)
        Next
        Return Rectangle.Empty
    End Function

    ' ── Painting ────────────────────────────────────────────────────────────────

    ' The combo cells of a header band: the shared value and the chevron, over the band already filled.
    Private Sub DrawGroupHeaderCombos(g As Graphics, k_node As KBotGroupNode, k_bandRect As Rectangle, k_font As Font, k_fore As Color)
        Dim k_hOffset As Integer = HScrollOffset()
        For Each k_cl In _scrollLayout
            If Not HasGroupHeaderCombo(k_cl.Column) Then Continue For
            DrawGroupHeaderComboCell(g, k_node, k_cl.Column, _frozenBandWidth + k_cl.X - k_hOffset, k_bandRect, k_font, k_fore)
        Next
        For Each k_cl In _frozenLayout
            If Not HasGroupHeaderCombo(k_cl.Column) Then Continue For
            DrawGroupHeaderComboCell(g, k_node, k_cl.Column, k_cl.X, k_bandRect, k_font, k_fore)
        Next
    End Sub

    Private Sub DrawGroupHeaderComboCell(g As Graphics, k_node As KBotGroupNode, k_col As KBotDataColumn, k_x As Integer,
                                         k_bandRect As Rectangle, k_font As Font, k_fore As Color)
        Dim k_cell As New Rectangle(k_x, k_bandRect.Top, k_col.WidthPx, k_bandRect.Height)
        If k_cell.Right < 0 OrElse k_cell.Left > ClientSize.Width Then Return
        DrawComboCell(g, CellContentRect(k_col, k_cell), GroupHeaderComboText(k_node, k_col), k_font, k_fore, k_col.TextAlign, True)
    End Sub

    ' ── Editing ─────────────────────────────────────────────────────────────────

    ' A press on a header band: true when it was on a combo cell (the editor opens; the group is NOT folded).
    Private Function TryOpenGroupHeaderCombo(k_bandIndex As Integer, k_x As Integer) As Boolean
        Dim k_col As KBotDataColumn = ColumnAtX(k_x)
        If k_col Is Nothing OrElse Not HasGroupHeaderCombo(k_col) Then Return False
        If _readOnlyGrid OrElse KBotDesignTime.IsDesignTime(Me) Then Return True
        OpenGroupEditor(k_bandIndex, k_col)
        Return True
    End Function

    Private Sub OpenGroupEditor(k_bandIndex As Integer, k_col As KBotDataColumn)
        Try
            If Not CommitEdit() Then Return
            Dim k_band As KBotBand = BandAt(k_bandIndex)
            Dim k_node As KBotGroupNode = GroupAt(k_band.GroupIndex)
            If k_node Is Nothing OrElse k_node.Level < 0 OrElse k_node.Level >= _activeLevels.Count Then Return
            Dim k_level As KBotGroupLevel = _activeLevels(k_node.Level)
            Dim k_cell As Rectangle = GroupHeaderCellRect(k_bandIndex, k_col)
            If k_cell.Width <= 0 OrElse k_cell.Height <= 0 Then Return

            Dim k_font As Font = GroupBandFontFor(k_level, True)
            Dim k_lineH As Integer
            Dim k_padLeft As Integer
            Dim k_padRight As Integer
            MeasureTextPadding(k_font, k_lineH, k_padLeft, k_padRight)
            Dim k_content As Rectangle = CellContentRect(k_col, k_cell)
            Dim k_h As Integer = Math.Min(k_lineH, k_cell.Height)
            Dim k_top As Integer = Math.Max(k_cell.Top, Math.Min(k_content.Top + (k_content.Height - k_h + 1) \ 2, k_cell.Bottom - k_h))

            _groupEditGroup = k_band.GroupIndex
            _groupEditColumn = k_col.Key
            _groupEditCell = k_cell
            _suppressGroupEvents = True
            Try
                groupCombo.Items.Clear()
                If k_col.ComboItems IsNot Nothing Then groupCombo.Items.AddRange(k_col.ComboItems)
                groupCombo.Font = k_font
                groupCombo.BackColor = GroupBandBackFor(k_level, k_node.Level, True)
                groupCombo.ForeColor = GroupBandForeFor(k_level, True)
                groupCombo.TextAlign = HorizontalFrom(k_col.TextAlign)
                groupCombo.Text = GroupHeaderComboText(k_node, k_col)
                groupCombo.Bounds = New Rectangle(k_content.Left, k_top, Math.Max(1, k_content.Width - ScaleDpi(ComboChevronZone)), k_h)
                groupCombo.Visible = True
                groupCombo.BringToFront()
                groupCombo.Focus()
                groupCombo.SelectAll()
            Finally
                _suppressGroupEvents = False
            End Try
            groupCombo.DroppedDown = True
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OpenGroupEditor", ex)
            Throw
        End Try
    End Sub

    Private Sub CloseGroupEditor()
        _suppressGroupEvents = True
        Try
            groupCombo.Visible = False
        Finally
            _suppressGroupEvents = False
        End Try
        _groupEditGroup = -1
        _groupEditColumn = String.Empty
    End Sub

    ' The item chosen goes to the host; nothing is written here.
    Private Sub CommitGroupEditor()
        Try
            If _groupEditGroup < 0 Then Return
            groupCombo.CommitText()
            If groupCombo.SelectedIndex < 0 Then Return
            Dim k_picked As Object = groupCombo.SelectedItem
            Dim k_colKey As String = _groupEditColumn
            Dim k_rows As List(Of Integer) = GroupModelRows(_groupEditGroup)
            CloseGroupEditor()
            Focus()
            RaiseEvent GroupHeaderValuePicked(Me, New KBotGroupHeaderValueEventArgs(k_colKey, k_picked, k_rows))
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.CommitGroupEditor", ex)
            Throw
        End Try
    End Sub

    Private Sub OnGroupComboPicked(sender As Object, e As EventArgs)
        Try
            If _suppressGroupEvents Then Return
            CommitGroupEditor()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnGroupComboPicked", ex)
        End Try
    End Sub

    Private Sub OnGroupComboKeyDown(sender As Object, e As KeyEventArgs)
        Try
            Select Case e.KeyCode
                Case Keys.Enter
                    CommitGroupEditor()
                    e.Handled = True
                    e.SuppressKeyPress = True
                Case Keys.Escape
                    CloseGroupEditor()
                    Focus()
                    e.Handled = True
                    e.SuppressKeyPress = True
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnGroupComboKeyDown", ex)
        End Try
    End Sub

    Private Sub OnGroupComboLeave(sender As Object, e As EventArgs)
        Try
            If _suppressGroupEvents Then Return
            CloseGroupEditor()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OnGroupComboLeave", ex)
        End Try
    End Sub

End Class
