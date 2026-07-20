Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Common

''' <summary>
''' English (slice 0013): column auto-sizing for <see cref="KBotDataView"/>. Two grid-wide
''' knobs run as one pass inside <c>UpdateLayout</c>, before the offsets and scrollbars are
''' recomputed:
'''
'''  1. <see cref="AutoSizeColumnsMode"/> — measure each visible column to its content
'''     (header vs a bounded sample of cells) and clamp to [MinWidth, MaxWidth].
'''  2. <see cref="ColumnFillMode"/> — then spend the leftover space (or absorb the overflow)
'''     so the fill modes never leave an empty strip nor a scrollbar (except the honest
'''     sum(MinWidth) &gt; available fallback).
'''
'''  The vertical scrollbar's visibility depends only on row count and body height, never on
'''  column widths, so it is decided first and there is no circular dependency. A re-entrancy
'''  guard (<c>_inAutoLayout</c>) makes doubly sure a pass can never trigger itself.
'''
'''  Measuring never raises <c>CellFormatting</c> (it would be expensive and re-entrant): a
'''  handler that widens the displayed text past the measured width will ellipsize. Cells are
'''  sampled (<see cref="AutoSizeSampleRows"/>, default 200) to keep the pass O(sample), not
'''  O(rows) — a wider value further down the grid ellipsizes; set 0 on small grids for exact.
''' </summary>
Partial Class KBotDataView

    ' ── State (grid-wide) ────────────────────────────────────────────────────────
    Private _autoSizeMode As KBotAutoSizeMode = KBotAutoSizeMode.ToContent
    Private _fillMode As KBotFillMode = KBotFillMode.None
    Private _autoSizeSampleRows As Integer = 200

    ' Re-entrancy guard: the pass mutates column widths, so it must never re-enter itself.
    Private _inAutoLayout As Boolean = False

    ' ── Public properties ────────────────────────────────────────────────────────

    ''' <summary>English (slice 0013): how columns are measured. Default <c>ToContent</c>.</summary>
    Public Property AutoSizeColumnsMode As KBotAutoSizeMode
        Get
            Return _autoSizeMode
        End Get
        Set(value As KBotAutoSizeMode)
            _autoSizeMode = value
            LayoutChanged()
        End Set
    End Property

    ''' <summary>English (slice 0013): how leftover/overflow space is spent. Default <c>None</c>.</summary>
    Public Property ColumnFillMode As KBotFillMode
        Get
            Return _fillMode
        End Get
        Set(value As KBotFillMode)
            _fillMode = value
            LayoutChanged()
        End Set
    End Property

    ''' <summary>
    ''' English (slice 0013): how many rows (from the top) are measured when sizing to content.
    ''' Default 200; 0 measures every row. Clamped to be non-negative.
    ''' </summary>
    Public Property AutoSizeSampleRows As Integer
        Get
            Return _autoSizeSampleRows
        End Get
        Set(value As Integer)
            _autoSizeSampleRows = Math.Max(0, value)
            LayoutChanged()
        End Set
    End Property

    ' ── Public methods ───────────────────────────────────────────────────────────

    ''' <summary>English (slice 0013): force a full auto-size pass on demand.</summary>
    Public Sub AutoSizeColumns()
        Try
            LayoutChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.AutoSizeColumns", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' English (slice 0013): clear every column's <c>UserSized</c> flag (undo the operator's
    ''' manual drags) and force a fresh auto-size pass.
    ''' </summary>
    Public Sub ResetColumnSizing()
        Try
            For Each c In _columns
                c.UserSized = False
            Next
            LayoutChanged()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.ResetColumnSizing", ex)
            Throw
        End Try
    End Sub

    ' ── The pass ─────────────────────────────────────────────────────────────────

    ''' <summary>
    ''' English (slice 0013): measure (ToContent) then fill/shrink. Called from
    ''' <c>UpdateLayout</c>, ahead of <c>RecalcColumnLayout</c>/<c>UpdateScrollBars</c>.
    ''' Layout/measure boundary: log and swallow — a measurement glitch must not blow up
    ''' column setup; the widths simply stay as they were.
    ''' </summary>
    Private Sub PerformAutoSize()
        If _inAutoLayout Then Return
        If _updateDepth > 0 Then Return                 ' deferred until EndUpdate
        ' Manual-only (Case 3): keep the caller's widths exactly, touch nothing.
        If _autoSizeMode = KBotAutoSizeMode.None AndAlso _fillMode = KBotFillMode.None Then Return

        _inAutoLayout = True
        Try
            Dim vis As List(Of KBotDataColumn) = VisibleColumns()
            If vis.Count = 0 Then Return

            ' Step 1 — size to content (skipping columns the operator has dragged).
            If _autoSizeMode = KBotAutoSizeMode.ToContent Then
                For Each c In vis
                    If c.UserSized Then Continue For
                    c.Width = MeasureColumnToContent(c)   ' setter clamps to [Min, Max]
                Next
            End If

            ' Step 2 — spend the leftover, or absorb the overflow.
            If _fillMode <> KBotFillMode.None Then DistributeOrShrink(vis)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.PerformAutoSize", ex)
        Finally
            _inAutoLayout = False
        End Try
    End Sub

    ' ── Measuring (ToContent) ────────────────────────────────────────────────────

    ' Width = max(header need, content need), then clamped to [MinWidth, MaxWidth]. Header and
    ' cells are measured with the same fonts the painter uses, so the result does not ellipsize.
    Private Function MeasureColumnToContent(col As KBotDataColumn) As Integer
        Dim cellPadX As Integer = ScaleDpi(6)             ' matches DrawTextCell
        Dim headerPadX As Integer = ScaleDpi(8)           ' matches DrawHeaderCell

        ' Header text always participates (semibold header font).
        Dim need As Integer = MeasureText(col.HeaderText, HeaderFont()) + 2 * headerPadX

        Select Case col.ColumnType
            Case KBotColumnType.CheckBox, KBotColumnType.OptionButton
                ' No text content: the centered glyph box plus padding (see DrawCheckCell).
                need = Math.Max(need, ScaleDpi(14) + 2 * cellPadX)

            Case KBotColumnType.ProgressBar
                ' No intrinsic content: keep the caller's width; header still participates.
                need = Math.Max(need, col.Width)

            Case KBotColumnType.Combo
                ' Widest formatted cell plus padding plus the chevron zone (see DrawComboCell).
                need = Math.Max(need, MeasureSampledCells(col) + 2 * cellPadX + ScaleDpi(16))

            Case Else
                ' Text and Button: widest formatted / caption cell plus padding.
                need = Math.Max(need, MeasureSampledCells(col) + 2 * cellPadX)
        End Select

        Return Math.Max(col.MinWidth, Math.Min(need, col.MaxWidth))
    End Function

    ' Widest sampled cell for a column, measured formatted (never raising CellFormatting).
    Private Function MeasureSampledCells(col As KBotDataColumn) As Integer
        Dim limit As Integer = If(_autoSizeSampleRows <= 0, _rows.Count,
                                  Math.Min(_autoSizeSampleRows, _rows.Count))
        Dim maxW As Integer = 0
        For i As Integer = 0 To limit - 1
            Dim row As KBotDataRow = _rows(i)
            Dim text As String = FormatValue(row(col.Key), col)
            ' A Button paints its caption, falling back to the header when the cell is empty.
            If col.ColumnType = KBotColumnType.Button AndAlso String.IsNullOrEmpty(text) Then
                text = col.HeaderText
            End If
            Dim w As Integer = MeasureText(text, Font)
            If w > maxW Then maxW = w
        Next
        Return maxW
    End Function

    ' Horizontal extent of a string. TextRenderer.MeasureText works headless (screen DC) and
    ' includes the same internal padding the matching DrawText uses, so we do not under-measure.
    Private Shared Function MeasureText(text As String, font As Font) As Integer
        If String.IsNullOrEmpty(text) Then Return 0
        Return TextRenderer.MeasureText(text, font).Width
    End Function

    ' ── Fill / shrink ────────────────────────────────────────────────────────────

    ' The available width mirrors what UpdateScrollBars uses, so a fill mode makes the totals
    ' match the viewport exactly and no horizontal scrollbar appears.
    Private Function AutoSizeAvailableWidth() As Integer
        Dim vw As Integer = If(WillVScrollBeVisible(), SystemInformation.VerticalScrollBarWidth, 0)
        Return Math.Max(0, ClientSize.Width - vw)
    End Function

    ' The vertical scrollbar depends only on row count and body height (never on column widths).
    Private Function WillVScrollBeVisible() As Boolean
        Dim contentH As Integer = _rows.Count * _rowHeight
        Dim availH As Integer = Math.Max(0, ClientSize.Height - HeaderBandHeight())
        Return contentH > availH
    End Function

    Private Sub DistributeOrShrink(vis As List(Of KBotDataColumn))
        Dim available As Integer = AutoSizeAvailableWidth()
        Dim total As Integer = SumWidths(vis)

        If total = available Then Return
        If total < available Then
            DistributeLeftover(vis, available - total)
        Else
            ShrinkToFit(vis, available)
        End If
    End Sub

    ' available > total: hand the leftover to first / last / all columns.
    Private Sub DistributeLeftover(vis As List(Of KBotDataColumn), leftover As Integer)
        Select Case _fillMode
            Case KBotFillMode.FirstColumn
                GrowColumn(vis(0), leftover)             ' MaxWidth may cap; surplus unused
            Case KBotFillMode.LastColumn
                GrowColumn(vis(vis.Count - 1), leftover)
            Case KBotFillMode.Proportional
                DistributeProportional(vis, leftover)
        End Select
    End Sub

    ' Add extra to a single column. The Width setter clamps at MaxWidth, so an over-cap
    ' remainder is silently dropped (it must not spill into a neighbour).
    Private Shared Sub GrowColumn(col As KBotDataColumn, extra As Integer)
        col.Width = col.Width + extra
    End Sub

    ' Split the leftover in proportion to each column's current width. Integer division leaves
    ' a few pixels over; the whole remainder goes to the last column so the totals add up
    ' exactly (no 1–2 px gap at the right edge). MaxWidth-capped columns pass their surplus to
    ' the uncapped ones in ONE extra pass (not a loop to convergence).
    Private Sub DistributeProportional(vis As List(Of KBotDataColumn), leftover As Integer)
        Dim totalWidth As Long = SumWidths(vis)
        If totalWidth <= 0 Then Return

        Dim shares(vis.Count - 1) As Integer
        Dim assigned As Integer = 0
        For i As Integer = 0 To vis.Count - 1
            shares(i) = CInt(CLng(leftover) * vis(i).Width \ totalWidth)
            assigned += shares(i)
        Next
        shares(vis.Count - 1) += (leftover - assigned)   ' exact remainder to the last column

        Dim surplus As Integer = 0
        Dim uncapped As New List(Of KBotDataColumn)()
        For i As Integer = 0 To vis.Count - 1
            Dim c As KBotDataColumn = vis(i)
            Dim want As Integer = c.Width + shares(i)
            c.Width = want                               ' setter clamps to MaxWidth
            If c.Width < want Then
                surplus += (want - c.Width)              ' capped: could not take its full share
            ElseIf c.MaxWidth > c.Width Then
                uncapped.Add(c)                          ' still has headroom
            End If
        Next

        If surplus > 0 AndAlso uncapped.Count > 0 Then RedistributeSurplus(uncapped, surplus)
    End Sub

    ' One extra proportional pass to place a capped column's surplus among the uncapped ones.
    Private Shared Sub RedistributeSurplus(cols As List(Of KBotDataColumn), surplus As Integer)
        Dim totalWidth As Long = 0
        For Each c In cols
            totalWidth += c.Width
        Next
        If totalWidth <= 0 Then Return

        Dim shares(cols.Count - 1) As Integer
        Dim assigned As Integer = 0
        For i As Integer = 0 To cols.Count - 1
            shares(i) = CInt(CLng(surplus) * cols(i).Width \ totalWidth)
            assigned += shares(i)
        Next
        shares(cols.Count - 1) += (surplus - assigned)
        For i As Integer = 0 To cols.Count - 1
            cols(i).Width = cols(i).Width + shares(i)    ' setter clamps; any residue is dropped
        Next
    End Sub

    ' total > available and a fill mode is active: shrink so the scrollbar does not appear.
    Private Sub ShrinkToFit(vis As List(Of KBotDataColumn), available As Integer)
        Dim minTotal As Integer = 0
        For Each c In vis
            minTotal += c.MinWidth
        Next

        ' Honest fallback: even at MinWidth the columns overflow. Pin everything to MinWidth
        ' and let UpdateScrollBars show the horizontal scrollbar — text vanishing entirely is
        ' worse than a scrollbar the caller did not ask for.
        If minTotal >= available Then
            For Each c In vis
                c.Width = c.MinWidth
            Next
            Return
        End If

        ' Remove the deficit from columns still above MinWidth, proportional to their current
        ' width, flooring at MinWidth. Bounded: each round either converges or pins at least one
        ' more column at its floor (so at most vis.Count rounds).
        Dim guard As Integer = 0
        Do
            Dim deficit As Integer = SumWidths(vis) - available
            If deficit <= 0 Then Exit Do

            Dim flex As New List(Of KBotDataColumn)()
            Dim flexWidth As Long = 0
            For Each c In vis
                If c.Width > c.MinWidth Then
                    flex.Add(c)
                    flexWidth += c.Width
                End If
            Next
            If flex.Count = 0 OrElse flexWidth <= 0 Then Exit Do

            Dim shares(flex.Count - 1) As Integer
            Dim assigned As Integer = 0
            For i As Integer = 0 To flex.Count - 1
                shares(i) = CInt(CLng(deficit) * flex(i).Width \ flexWidth)
                assigned += shares(i)
            Next
            shares(flex.Count - 1) += (deficit - assigned)   ' rounding remainder to last flex
            For i As Integer = 0 To flex.Count - 1
                Dim c As KBotDataColumn = flex(i)
                c.Width = Math.Max(c.MinWidth, c.Width - shares(i))   ' floor at MinWidth
            Next

            guard += 1
        Loop While guard <= vis.Count + 1
    End Sub

    Private Shared Function SumWidths(cols As List(Of KBotDataColumn)) As Integer
        Dim total As Integer = 0
        For Each c In cols
            total += c.Width
        Next
        Return total
    End Function

End Class
