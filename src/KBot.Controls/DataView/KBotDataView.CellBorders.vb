Option Strict On
Imports System.Collections.Generic
Imports System.Drawing

''' <summary>
''' Cell borders (slice 0085-03). Every data cell draws the sides its column asks for
''' (<see cref="KBotDataColumn.CellBorders"/>, default right + bottom) in the colour its column
''' asks for (<see cref="KBotDataColumn.CellBorderColor"/>, default the theme's grid-line colour);
''' a <c>CellFormatting</c> handler can change both for one cell.
'''
''' <para>The default look is the grid lines of before: a vertical line at the right edge of
''' each cell and ONE horizontal line under the whole row. Any other side is an extra line on the
''' outermost pixels of the cell; a cell whose bottom side is not the plain grid line (none, or
''' another colour) is left out of the row line and draws its own.</para>
'''
''' <para><b>Cost.</b> The default path costs what it did before: one line per cell plus one
''' per row, nothing allocated. Only cells that deviate draw more (up to four lines). The theme
''' colour reuses the cached pen, and a custom colour reuses ONE pen kept for the last colour
''' used (cells of one column almost always share a colour, so the pen is rebuilt only when the
''' colour changes between neighbours).</para>
''' </summary>
Partial Class KBotDataView

    Private _pCellBorderCustom As Pen
    Private _cCellBorderCustom As Color

    ''' <summary>
    ''' The pen for a cell border colour: the cached theme pen for <c>Color.Empty</c>, nothing for
    ''' a fully transparent colour, else the single custom pen (rebuilt only on a colour change).
    ''' </summary>
    Private Function CellBorderPen(k_color As Color) As Pen
        If k_color.IsEmpty Then Return _pGridLine
        If k_color.A = 0 Then Return Nothing
        If _pCellBorderCustom Is Nothing OrElse _cCellBorderCustom <> k_color Then
            _pCellBorderCustom?.Dispose()
            _pCellBorderCustom = New Pen(k_color)
            _cCellBorderCustom = k_color
        End If
        Return _pCellBorderCustom
    End Function

    ' Horizontal spans [left, right) of the cells of the row being painted whose bottom side is
    ' NOT the plain grid line; the row line is drawn around them. Reused for every row.
    Private ReadOnly _rowGaps As New List(Of Point)

    Private Shared ReadOnly GapOrder As Comparison(Of Point) = Function(k_a As Point, k_b As Point) k_a.X.CompareTo(k_b.X)

    ' One horizontal segment [k_from, k_to) on the row's bottom pixel line.
    Private Sub DrawGridSegment(g As Graphics, k_y As Integer, k_from As Integer, k_to As Integer)
        If k_to > k_from Then g.DrawLine(_pGridLine, k_from, k_y, k_to - 1, k_y)
    End Sub

    ''' <summary>
    ''' The grid line under a row: ONE line from the left edge to <paramref name="k_end"/>
    ''' (exclusive), except across the spans in <c>_rowGaps</c>. With no gaps (the normal case)
    ''' it is a single <c>DrawLine</c> exactly as before the cell borders existed.
    ''' </summary>
    Private Sub DrawRowBottomLine(g As Graphics, k_y As Integer, k_end As Integer)
        If _rowGaps.Count = 0 Then
            DrawGridSegment(g, k_y, 0, k_end)
            Return
        End If
        If _rowGaps.Count > 1 Then _rowGaps.Sort(GapOrder)
        Dim k_x As Integer = 0
        For Each k_gap As Point In _rowGaps
            If k_gap.X >= k_end Then Exit For
            DrawGridSegment(g, k_y, k_x, k_gap.X)
            k_x = Math.Max(k_x, k_gap.Y)
            If k_x >= k_end Then Return
        Next
        DrawGridSegment(g, k_y, k_x, k_end)
    End Sub

    ' Draws the chosen sides of one cell. The plain bottom side (theme colour) is NOT drawn here:
    ' the row line does it in one stroke. A bottom side that is not plain (no bottom, or another
    ' colour) reports the cell's span in _rowGaps so the row line leaves it alone, and the cell
    ' draws its own bottom when it has one. Covered by the Try in OnPaint.
    Private Sub DrawCellBorders(g As Graphics, k_cellRect As Rectangle, k_sides As KBotBorderSides, k_color As Color)
        ' Fast path: the default (right + bottom, theme colour) is nearly every cell. One line,
        ' the cached grid-line pen, no side tests, no pen lookup.
        If k_sides = KBotDataColumn.DefaultCellBorders AndAlso k_color.IsEmpty Then
            g.DrawLine(_pGridLine, k_cellRect.Right - 1, k_cellRect.Top, k_cellRect.Right - 1, k_cellRect.Bottom - 1)
            Return
        End If

        Dim k_plainBottom As Boolean = (k_sides And KBotBorderSides.Bottom) <> 0 AndAlso k_color.IsEmpty
        If Not k_plainBottom Then _rowGaps.Add(New Point(k_cellRect.Left, k_cellRect.Right))
        If k_plainBottom Then k_sides = k_sides And Not KBotBorderSides.Bottom

        If k_sides = KBotBorderSides.None Then Return
        Dim k_pen As Pen = CellBorderPen(k_color)
        If k_pen Is Nothing Then Return

        Dim k_right As Integer = k_cellRect.Right - 1
        Dim k_bottom As Integer = k_cellRect.Bottom - 1
        If (k_sides And KBotBorderSides.Left) <> 0 Then g.DrawLine(k_pen, k_cellRect.Left, k_cellRect.Top, k_cellRect.Left, k_bottom)
        If (k_sides And KBotBorderSides.Top) <> 0 Then g.DrawLine(k_pen, k_cellRect.Left, k_cellRect.Top, k_right, k_cellRect.Top)
        If (k_sides And KBotBorderSides.Right) <> 0 Then g.DrawLine(k_pen, k_right, k_cellRect.Top, k_right, k_bottom)
        If (k_sides And KBotBorderSides.Bottom) <> 0 Then g.DrawLine(k_pen, k_cellRect.Left, k_bottom, k_right, k_bottom)
    End Sub

    ' Released with the other pens (theme change / dispose); the next paint rebuilds it.
    Private Sub DisposeCellBorderPen()
        _pCellBorderCustom?.Dispose()
        _pCellBorderCustom = Nothing
    End Sub

End Class
