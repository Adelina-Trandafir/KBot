Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

' Slice 02EF-02 -- a number typed in the HEADER band of a group (KBotDataColumn.GroupHeaderEdit): the aggregate shown there can be typed over. A click on it opens one
' floating TextBox (groupText, declared in the Designer file) over the aggregate; Enter or leaving the box hands the typed text to the host through GroupHeaderValueEdited,
' which spreads it on the rows of the group. Nothing is written here. Escape closes the box without handing anything over.
Partial Class KBotDataView

    ''' <summary>The operator typed a value over the aggregate of a group header; <c>Value</c> of the arguments is the typed text, the host reads it and writes the rows it names.</summary>
    Public Event GroupHeaderValueEdited As EventHandler(Of KBotGroupHeaderValueEventArgs)

    Private _groupTextGroup As Integer = -1
    Private _groupTextColumn As String = String.Empty
    Private _suppressGroupTextEvents As Boolean

    Private Shared Function HasGroupHeaderEdit(k_col As KBotDataColumn) As Boolean
        Return k_col.GroupHeaderEdit AndAlso k_col.Aggregate <> KBotAggregate.None AndAlso Not k_col.ReadOnly
    End Function

    ' A press on a header band: true when it was on a cell whose aggregate can be typed over (the box opens; the group is NOT folded).
    Private Function TryOpenGroupHeaderEdit(k_bandIndex As Integer, k_x As Integer) As Boolean
        Dim k_col As KBotDataColumn = ColumnAtX(k_x)
        If k_col Is Nothing OrElse Not HasGroupHeaderEdit(k_col) Then Return False
        If _readOnlyGrid OrElse KBotDesignTime.IsDesignTime(Me) Then Return True
        OpenGroupTextEditor(k_bandIndex, k_col)
        Return True
    End Function

    ' UI boundary (called from the mouse handler): logs and swallows.
    Private Sub OpenGroupTextEditor(k_bandIndex As Integer, k_col As KBotDataColumn)
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
            ' The same rectangle the painter gives the aggregate (DrawGroupAggregateCell): 8 logical pixels of air on each side.
            Dim k_padX As Integer = ScaleDpi(8)
            Dim k_h As Integer = Math.Min(k_lineH, k_cell.Height)
            Dim k_top As Integer = Math.Max(k_cell.Top, Math.Min(k_cell.Top + (k_cell.Height - k_h + 1) \ 2, k_cell.Bottom - k_h))

            _groupTextGroup = k_band.GroupIndex
            _groupTextColumn = k_col.Key
            _suppressGroupTextEvents = True
            Try
                groupText.Font = k_font
                groupText.TextAlign = HorizontalFrom(k_col.TextAlign)
                groupText.Text = GroupAggregateText(k_node, k_col)
                groupText.Bounds = New Rectangle(k_cell.Left + k_padX, k_top, Math.Max(1, k_cell.Width - 2 * k_padX), k_h)
                groupText.Visible = True
                groupText.BringToFront()
                groupText.Focus()
                groupText.SelectAll()
            Finally
                _suppressGroupTextEvents = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.OpenGroupTextEditor", ex)
        End Try
    End Sub

    Private Sub CloseGroupTextEditor()
        _suppressGroupTextEvents = True
        Try
            groupText.Visible = False
        Finally
            _suppressGroupTextEvents = False
        End Try
        _groupTextGroup = -1
        _groupTextColumn = String.Empty
    End Sub

    ' The text typed goes to the host; nothing is written here.
    Private Sub CommitGroupTextEditor()
        If _groupTextGroup < 0 Then Return
        Dim k_text As String = groupText.Text
        Dim k_colKey As String = _groupTextColumn
        Dim k_rows As List(Of Integer) = GroupModelRows(_groupTextGroup)
        CloseGroupTextEditor()
        Focus()
        RaiseEvent GroupHeaderValueEdited(Me, New KBotGroupHeaderValueEventArgs(k_colKey, k_text, k_rows))
        Invalidate()
    End Sub

    ' UI boundary: logs and swallows.
    Private Sub GroupText_KeyDown(sender As Object, e As KeyEventArgs) Handles groupText.KeyDown
        Try
            Select Case e.KeyCode
                Case Keys.Enter
                    e.Handled = True
                    e.SuppressKeyPress = True
                    CommitGroupTextEditor()
                Case Keys.Escape
                    e.Handled = True
                    e.SuppressKeyPress = True
                    CloseGroupTextEditor()
                    Focus()
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.GroupText_KeyDown", ex)
        End Try
    End Sub

    ' UI boundary: leaving the box hands the typed value over, as leaving a cell does.
    Private Sub GroupText_Leave(sender As Object, e As EventArgs) Handles groupText.Leave
        Try
            If _suppressGroupTextEvents OrElse _groupTextGroup < 0 Then Return
            CommitGroupTextEditor()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.GroupText_Leave", ex)
        End Try
    End Sub

End Class
