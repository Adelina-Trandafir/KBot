Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports KBot.Common

' Slice KE12-03 -- tick boxes built into the grid (CheckBoxes): a small first column with no header text, a tick box on every data row and, in mode All, a tri-state
' tick box on the header band of every group. The column is the grid's own (key CheckColumnKey): the grid adds it when the mode is set and takes it away with None.
' The state of a row is the Boolean in that cell. The operator's tick raises RowChecked (and CellValueChanged, like any CheckBox column).
Partial Class KBotDataView

    ''' <summary>The key of the column the grid adds for <see cref="CheckBoxes"/>; the tick state of a row is <c>row(CheckColumnKey)</c>.</summary>
    Public Const CheckColumnKey As String = "__check"

    ''' <summary>The operator ticked or cleared the tick box of a row (or of the header of its group).</summary>
    Public Event RowChecked As EventHandler(Of KBotRowCheckedEventArgs)

    Private _checkBoxes As KBotCheckBoxMode = KBotCheckBoxMode.None
    Private Const CheckColumnWidth As Integer = 30

    ''' <summary>
    ''' Tick boxes: None (default), All (rows and group headers) or RowsOnly. A mode other than None adds a small first column without header text, at run time only
    ''' (the designer does not show it, so it is never written in the form's file).
    ''' </summary>
    <Category("K-BOT")>
    <Description("Casete de bifat: None = fără; All = pe fiecare rând și pe antetul fiecărui grup (bifează tot grupul); RowsOnly = doar pe rânduri. Coloana apare la rulare.")>
    <DefaultValue(KBotCheckBoxMode.None)>
    Public Property CheckBoxes As KBotCheckBoxMode
        Get
            Return _checkBoxes
        End Get
        Set(value As KBotCheckBoxMode)
            If value = _checkBoxes Then Return
            _checkBoxes = value
            ApplyCheckBoxes()
            Invalidate()
        End Set
    End Property

    Private Sub ApplyCheckBoxes()
        Try
            If KBotDesignTime.IsDesignTime(Me) Then Return
            Dim k_col As KBotDataColumn = Nothing
            Dim k_has As Boolean = _columnIndex.TryGetValue(CheckColumnKey, k_col)
            If _checkBoxes = KBotCheckBoxMode.None Then
                If k_has Then _columns.Remove(k_col)
            ElseIf Not k_has Then
                k_col = New KBotDataColumn(CheckColumnKey, String.Empty, KBotColumnType.CheckBox, CheckColumnWidth)
                k_col.MinWidth = 20
                k_col.Width = CheckColumnWidth
                k_col.Resizable = False
                k_col.AllowGrouping = False
                k_col.HeaderTextAlign = ContentAlignment.MiddleCenter
                _columns.Insert(0, k_col)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ApplyCheckBoxes", ex)
            Throw
        End Try
    End Sub

    ''' <summary>True when the tick box of the row is ticked.</summary>
    Public Function GetRowChecked(k_rowIndex As Integer) As Boolean
        If k_rowIndex < 0 OrElse k_rowIndex >= _rows.Count Then Throw New ArgumentOutOfRangeException(NameOf(k_rowIndex), k_rowIndex, "No row at this index.")
        Return ToBool(_rows(k_rowIndex)(CheckColumnKey))
    End Function

    ''' <summary>Ticks or clears the tick box of a row from code. Raises nothing (this is loading data, not an operator gesture) and does not mark the row as edited.</summary>
    Public Sub SetRowChecked(k_rowIndex As Integer, k_checked As Boolean)
        If k_rowIndex < 0 OrElse k_rowIndex >= _rows.Count Then Throw New ArgumentOutOfRangeException(NameOf(k_rowIndex), k_rowIndex, "No row at this index.")
        _rows(k_rowIndex)(CheckColumnKey) = k_checked
        InvalidateRow(k_rowIndex)
    End Sub

    ''' <summary>The model indexes of the rows whose tick box is ticked, in row order.</summary>
    Public Function GetCheckedRows() As List(Of Integer)
        Dim k_list As New List(Of Integer)()
        For k_i As Integer = 0 To _rows.Count - 1
            If ToBool(_rows(k_i)(CheckColumnKey)) Then k_list.Add(k_i)
        Next
        Return k_list
    End Function

    ' ── Group headers ───────────────────────────────────────────────────────────

    Private Function CheckColumn() As KBotDataColumn
        Dim k_col As KBotDataColumn = Nothing
        If _checkBoxes = KBotCheckBoxMode.None OrElse Not _columnIndex.TryGetValue(CheckColumnKey, k_col) OrElse Not k_col.IsEffectivelyVisible Then Return Nothing
        Return k_col
    End Function

    ' The room a header band leaves on its left for its tick box (0 = no tick box on headers).
    Friend Function GroupCheckOffset() As Integer
        If _checkBoxes <> KBotCheckBoxMode.All Then Return 0
        Dim k_col As KBotDataColumn = CheckColumn()
        Return If(k_col Is Nothing, 0, k_col.WidthPx)
    End Function

    ' 0 = none of the rows ticked, 1 = all, 2 = some.
    Private Function GroupCheckState(k_node As KBotGroupNode) As Integer
        Dim k_any As Boolean = False
        Dim k_all As Boolean = True
        For k_pos As Integer = k_node.FirstViewPos To k_node.LastViewPos
            If ToBool(_rows(ModelIndexAt(k_pos))(CheckColumnKey)) Then
                k_any = True
            Else
                k_all = False
            End If
        Next
        If Not k_any Then Return 0
        Return If(k_all, 1, 2)
    End Function

    ' The check column's cell in a header band (client coordinates), or Empty.
    Private Function GroupCheckCell(k_bandRect As Rectangle) As Rectangle
        Dim k_col As KBotDataColumn = CheckColumn()
        If k_col Is Nothing Then Return Rectangle.Empty
        For Each k_cl In _frozenLayout
            If ReferenceEquals(k_cl.Column, k_col) Then Return New Rectangle(k_cl.X, k_bandRect.Top, k_col.WidthPx, k_bandRect.Height)
        Next
        Dim k_hOffset As Integer = HScrollOffset()
        For Each k_cl In _scrollLayout
            If ReferenceEquals(k_cl.Column, k_col) Then Return New Rectangle(_frozenBandWidth + k_cl.X - k_hOffset, k_bandRect.Top, k_col.WidthPx, k_bandRect.Height)
        Next
        Return Rectangle.Empty
    End Function

    Private Sub DrawGroupCheck(g As Graphics, k_node As KBotGroupNode, k_bandRect As Rectangle)
        If _checkBoxes <> KBotCheckBoxMode.All Then Return
        Dim k_cell As Rectangle = GroupCheckCell(k_bandRect)
        If k_cell.IsEmpty OrElse k_cell.Right < 0 OrElse k_cell.Left > ClientSize.Width Then Return
        Select Case GroupCheckState(k_node)
            Case 1 : DrawCheckCell(g, k_cell, True, True)
            Case 0 : DrawCheckCell(g, k_cell, False, True)
            Case Else : DrawMixedCheck(g, k_cell)
        End Select
    End Sub

    ' «Some»: the box with a filled square inside.
    Private Sub DrawMixedCheck(g As Graphics, k_cell As Rectangle)
        Dim k_size As Integer = ScaleDpi(14)
        Dim k_box As New Rectangle(k_cell.Left + (k_cell.Width - k_size) \ 2, k_cell.Top + (k_cell.Height - k_size) \ 2, k_size, k_size)
        Dim k_old As SmoothingMode = g.SmoothingMode
        g.SmoothingMode = SmoothingMode.AntiAlias
        Using k_path As GraphicsPath = RoundedRect(k_box, ScaleDpi(3))
            g.DrawPath(_pCheckBorder, k_path)
        End Using
        Dim k_in As Rectangle = Rectangle.Inflate(k_box, -ScaleDpi(4), -ScaleDpi(4))
        If k_in.Width > 0 AndAlso k_in.Height > 0 Then g.FillRectangle(_bCheckFill, k_in)
        g.SmoothingMode = k_old
    End Sub

    ' A press on a header band: true when it was on the tick box of the header (the group is NOT folded).
    Private Function TryToggleGroupCheck(k_bandIndex As Integer, k_location As Point) As Boolean
        If _checkBoxes <> KBotCheckBoxMode.All Then Return False
        Dim k_band As KBotBand = BandAt(k_bandIndex)
        Dim k_y As Integer = HeaderBandHeight() + k_band.Top - VScrollOffset()
        Dim k_cell As Rectangle = GroupCheckCell(New Rectangle(0, k_y, ClientSize.Width, k_band.Height))
        If k_cell.IsEmpty OrElse Not k_cell.Contains(k_location) Then Return False
        If _readOnlyGrid OrElse KBotDesignTime.IsDesignTime(Me) Then Return True
        Dim k_node As KBotGroupNode = GroupAt(k_band.GroupIndex)
        If k_node Is Nothing Then Return True
        If Not CommitEdit() Then Return True
        Dim k_target As Boolean = GroupCheckState(k_node) <> 1
        Dim k_changed As New List(Of Integer)()
        For Each k_row As Integer In GroupModelRows(k_band.GroupIndex)
            If ToBool(_rows(k_row)(CheckColumnKey)) = k_target OrElse Not IsCellEnabled(CheckColumnKey, k_row) Then Continue For
            _rows(k_row).CaptureBaseline()
            _rows(k_row)(CheckColumnKey) = k_target
            _rows(k_row).IsDirty = True
            k_changed.Add(k_row)
        Next
        Invalidate()
        For k_i As Integer = 0 To k_changed.Count - 1
            RaiseEvent CellValueChanged(Me, New KBotCellValueEventArgs(CheckColumnKey, k_changed(k_i), Not k_target, k_target))
            RaiseEvent RowChecked(Me, New KBotRowCheckedEventArgs(k_changed(k_i), k_target, k_changed.Count - 1 - k_i))
        Next
        Return True
    End Function

End Class
