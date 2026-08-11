Option Strict On
Imports System.ComponentModel
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
'''  English (slice 0028-04): knob 1 is no longer grid-wide only. Every column carries its own
'''  <see cref="KBotDataColumn.AutoSizeMode"/> and IT TAKES PRECEDENCE over the grid's — the grid
'''  setting is what a column falls back on, not what it obeys. The default column value is
'''  <see cref="KBotAutoSizeMode.Inherit"/> ("no opinion"), so a grid nobody has touched behaves
'''  exactly as it did before. Two consequences worth naming, both deliberate:
'''
'''  • the pass now runs even when the GRID says <c>None</c>, as long as one column asks for
'''    <c>ToContent</c> — otherwise the per-column knob would be a no-op in the one arrangement
'''    (measure just this column, leave the others alone) it exists for;
'''  • precedence covers MEASURING only. Knob 2 (fill / shrink) still spreads across every visible
'''    column, including a column pinned at <c>None</c> — same rule as a column the operator has
'''    drag-resized (<c>UserSized</c>), which ToContent skips but fill still moves. A column that
'''    must not move at all is pinned with <c>MinWidth = MaxWidth</c>, which the clamp enforces.
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

    ''' <summary>
    ''' English (slice 0013): how columns are measured. Default <c>ToContent</c>.
    '''
    ''' English (slice 0028-04): this is the FALLBACK now — it decides only for the columns whose
    ''' own <see cref="KBotDataColumn.AutoSizeMode"/> is <c>Inherit</c>.
    ''' <see cref="KBotAutoSizeMode.Inherit"/> itself is rejected here: there is nothing above the
    ''' grid to inherit from, so accepting it would silently have to mean something else.
    ''' </summary>
    <Category("K-BOT")>
    <Description("Cum se măsoară coloanele: None (lățimi fixe) sau ToContent (după conținut).")>
    <DefaultValue(KBotAutoSizeMode.ToContent)>
    Public Property AutoSizeColumnsMode As KBotAutoSizeMode
        Get
            Return _autoSizeMode
        End Get
        Set(value As KBotAutoSizeMode)
            If value = KBotAutoSizeMode.Inherit Then
                Throw New ArgumentException(
                    "«Inherit» e doar pentru coloane (nu există nimic deasupra grilei de moștenit).", NameOf(value))
            End If
            If Not [Enum].IsDefined(GetType(KBotAutoSizeMode), value) Then
                Throw New ArgumentException($"Mod de auto-dimensionare necunoscut: «{value}».", NameOf(value))
            End If
            _autoSizeMode = value
            LayoutChanged()
        End Set
    End Property

    ''' <summary>English (slice 0013): how leftover/overflow space is spent. Default <c>None</c>.</summary>
    <Category("K-BOT")>
    <Description("Cum se cheltuie spațiul rămas: None, FirstColumn, LastColumn sau Proportional.")>
    <DefaultValue(KBotFillMode.None)>
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
    <Category("K-BOT")>
    <Description("Câte rânduri (de sus) se măsoară la dimensionarea după conținut. 0 = toate.")>
    <DefaultValue(200)>
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

        ' English (slice 0028-05): the pass NEVER runs inside the Visual Studio designer. It
        ' writes `Width`, and the designer serializes whatever it finds afterwards — so a measured
        ' or stretched width lands in `.Designer.vb` as if the operator had typed it, and then
        ' outlives the layout that produced it. That is exactly how `IstoricView` ended up with
        ' `KBotDataColumn1.Width = 747`: the four widths summed to the design surface's own width,
        ' which no human types. It is the same trap the house rule describes for
        ' `ShouldSerialize*`, one level down — here the value is not a property default but the
        ' output of a layout pass, and a layout pass has no business authoring the form.
        ' The designer therefore shows the widths as AUTHORED; fill and measuring happen live.
        If KBotDesignTime.IsDesignTime(Me) Then Return

        Dim anyAutoHide As Boolean = AnyColumnCanAutoHide()

        ' Manual-only (Case 3) AND nothing to auto-hide: keep the caller's widths/visibility.
        ' English (slice 0028-04): «manual-only» is now decided per column — a grid set to None
        ' still runs the pass when a single column asks for ToContent on its own.
        If _fillMode = KBotFillMode.None AndAlso Not anyAutoHide AndAlso
           Not AnyColumnSizesToContent() Then Return

        _inAutoLayout = True
        Try
            ' Step 0 — reset the auto-hidden state so a widened grid re-shows columns that now fit,
            ' and put every width back to what the CALLER asked for. English (slice 0028-05): without
            ' that second reset the pass compounds its own output — a grid that was briefly narrow
            ' shrank a column to its floor, and widening the window afterwards could only ever grow
            ' the fill target, so the caller's 200px column stayed at 65px for the rest of the
            ' session. A pass must be a function of (authored widths, available space), nothing else.
            ClearAutoHiddenState()
            RestoreAuthoredWidths()

            Dim vis As List(Of KBotDataColumn) = VisibleColumns()
            If vis.Count = 0 Then Return

            ' Step 1 — size to content (skipping columns the operator has dragged). The mode is
            ' asked PER COLUMN: the column's own knob wins, the grid's answers for Inherit.
            For Each c In vis
                If c.UserSized Then Continue For
                If EffectiveAutoSizeMode(c) <> KBotAutoSizeMode.ToContent Then Continue For
                c.SetLayoutWidth(MeasureColumnToContent(c))   ' clamps to [Min, Max]
            Next

            ' Step 2 — auto-hide overflowing columns to avoid the horizontal scrollbar. Once
            ' auto-hide is engaged, unresolved overflow shows the scrollbar (it does NOT shrink):
            ' hiding a column is the caller's chosen response, not squeezing the survivors.
            Dim autoHideEngaged As Boolean = False
            If anyAutoHide Then
                PerformAutoHide(vis)                      ' may set AutoHidden on some columns
                vis = VisibleColumns()                    ' recompute after hiding
                If vis.Count = 0 Then Return
                autoHideEngaged = True
            End If

            ' Step 3 — spend the leftover (fill the gap a hidden column left), or absorb overflow.
            If _fillMode <> KBotFillMode.None Then DistributeOrShrink(vis, suppressShrink:=autoHideEngaged)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotDataView.PerformAutoSize", ex)
        Finally
            _inAutoLayout = False
        End Try
    End Sub

    ' ── Per-column precedence (slice 0028-04) ────────────────────────────────────

    ''' <summary>
    ''' English (slice 0028-04): the mode that actually applies to one column — its own, unless it
    ''' says <c>Inherit</c>, in which case the grid-wide setting answers. This is THE place the
    ''' precedence lives; nothing else may read <c>_autoSizeMode</c> to decide a column's fate.
    ''' </summary>
    Friend Function EffectiveAutoSizeMode(col As KBotDataColumn) As KBotAutoSizeMode
        If col Is Nothing Then Return _autoSizeMode
        If col.AutoSizeMode = KBotAutoSizeMode.Inherit Then Return _autoSizeMode
        Return col.AutoSizeMode
    End Function

    ' Does anything at all want measuring? Gates the pass together with fill / auto-hide. Hidden
    ' columns do not count: they take no space, so measuring them would change nothing on screen.
    Private Function AnyColumnSizesToContent() As Boolean
        For Each c In _columns
            If Not c.Visible Then Continue For
            If c.UserSized Then Continue For              ' a dragged column is skipped anyway
            If EffectiveAutoSizeMode(c) = KBotAutoSizeMode.ToContent Then Return True
        Next
        Return False
    End Function

    ' ── Auto-hide (slice 0016) ────────────────────────────────────────────────────

    ' Any auto-hideable column the caller currently shows? Gates the whole pass.
    Private Function AnyColumnCanAutoHide() As Boolean
        For Each c In _columns
            If c.AutoHide AndAlso c.Visible Then Return True
        Next
        Return False
    End Function

    ' Baseline of the pass: every column back to the width the caller authored. A column the
    ' operator has drag-resized is skipped — that drag IS the caller's width now, and it stands
    ' until ResetColumnSizing().
    Private Sub RestoreAuthoredWidths()
        For Each c In _columns
            If c.UserSized Then Continue For
            c.RestoreAuthoredWidth()
        Next
    End Sub

    ' Clear the pass-owned auto-hidden state (the caller's Visible flag is never touched).
    Private Sub ClearAutoHiddenState()
        For Each c In _columns
            c.AutoHidden = False
        Next
    End Sub

    ' Hide auto-hideable columns until the rest fit, or none are left (then the scrollbar shows).
    ' Order: rightmost hideable first (collapse from the right). The fill target is protected —
    ' if a column is BOTH auto-hideable and the fill target, stretching wins and it stays.
    Private Sub PerformAutoHide(vis As List(Of KBotDataColumn))
        Dim available As Integer = AutoSizeAvailableWidth()
        Dim total As Integer = SumWidths(vis)
        If total <= available Then Return                ' already fits — hide nothing

        Dim expander As KBotDataColumn = FillTargetColumn(vis)

        For i As Integer = vis.Count - 1 To 0 Step -1
            If total <= available Then Exit For
            Dim c As KBotDataColumn = vis(i)
            If Not c.AutoHide Then Continue For
            If ReferenceEquals(c, expander) Then Continue For   ' expanding takes precedence
            c.AutoHidden = True
            total -= c.Width
        Next
    End Sub

    ' The single column a First/Last fill grows — protected from auto-hide. Proportional / None
    ' has no single protected expander (Nothing), so its auto-hideable columns can all disappear.
    Private Function FillTargetColumn(vis As List(Of KBotDataColumn)) As KBotDataColumn
        If vis.Count = 0 Then Return Nothing
        Select Case _fillMode
            Case KBotFillMode.FirstColumn
                Return vis(0)
            Case KBotFillMode.LastColumn
                Return vis(vis.Count - 1)
            Case Else
                Return Nothing
        End Select
    End Function

    ' ── Measuring (ToContent) ────────────────────────────────────────────────────

    ' Width = max(header need, content need), then clamped to [MinWidth, MaxWidth]. Header and
    ' cells are measured with the same fonts the painter uses, so the result does not ellipsize.
    Private Function MeasureColumnToContent(col As KBotDataColumn) As Integer
        Dim cellPadX As Integer = ScaleDpi(6)             ' matches DrawTextCell
        Dim headerPadX As Integer = ScaleDpi(8)           ' matches DrawHeaderCell

        ' Header text always participates (semibold header font), plus whatever the column's
        ' header icons take (slice 0028-02) — measuring only the caption would size the column so
        ' the icons eat the text back, which is a defect, not a limitation.
        Dim need As Integer = MeasureText(col.HeaderText, ResolvedHeaderFont()) + 2 * headerPadX +
                              HeaderIconsExtent(col)

        ' English (slice 0017-01): the footer cell participates in measuring too — a wide total
        ' that was never measured would ellipsize, which is a defect not a limitation. It is
        ' painted with the footer band's own font and padding, so measure it the same way.
        If _showFooter AndAlso col.Aggregate <> KBotAggregate.None Then
            need = Math.Max(need, MeasureText(FooterTextFor(col), ResolvedFooterFont()) + 2 * headerPadX)
        End If

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

        ' EffectiveMinWidth, nu MinWidth: podeaua ține cont și de pictogramele de antet, și ea
        ' bate plafonul (vezi KBotDataColumn.ClampWidth).
        Return Math.Max(col.EffectiveMinWidth, Math.Min(need, col.MaxWidth))
    End Function

    ' Widest sampled cell for a column, measured formatted (never raising CellFormatting).
    Private Function MeasureSampledCells(col As KBotDataColumn) As Integer
        ' English (slice 0028-03): sample the VISIBLE rows. Measuring rows a filter has removed
        ' would size the column to content nobody can see — the column would stay wide and the
        ' filtered grid would look like it had lost its data off to the right.
        Dim total As Integer = ViewCount()
        Dim limit As Integer = If(_autoSizeSampleRows <= 0, total,
                                  Math.Min(_autoSizeSampleRows, total))
        Dim maxW As Integer = 0
        For i As Integer = 0 To limit - 1
            Dim row As KBotDataRow = ViewRowAt(i)
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
    ' English (slice 0017-01): the pinned totals band eats body height too, so subtract it here
    ' as well — otherwise the auto-size vscroll prediction and UpdateScrollBars would disagree.
    Private Function WillVScrollBeVisible() As Boolean
        ' Aceleași rânduri pe care le socotește UpdateScrollBars (cele care trec de filtre),
        ' altfel predicția și bara adevărată s-ar contrazice.
        Dim contentH As Integer = ViewCount() * _rowHeight
        Dim availH As Integer = Math.Max(0, ClientSize.Height - HeaderBandHeight() - FooterBandHeight())
        Return contentH > availH
    End Function

    ' suppressShrink (auto-hide engaged): on overflow, do NOT shrink the survivors — let the
    ' horizontal scrollbar appear. The leftover branch (fill the gap a hidden column left) still runs.
    Private Sub DistributeOrShrink(vis As List(Of KBotDataColumn), Optional suppressShrink As Boolean = False)
        Dim available As Integer = AutoSizeAvailableWidth()
        Dim total As Integer = SumWidths(vis)

        If total = available Then Return
        If total < available Then
            DistributeLeftover(vis, available - total)
        ElseIf Not suppressShrink Then
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
        col.SetLayoutWidth(col.Width + extra)
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
            c.SetLayoutWidth(want)                       ' clamps to MaxWidth
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
            cols(i).SetLayoutWidth(cols(i).Width + shares(i))   ' clamps; any residue is dropped
        Next
    End Sub

    ' total > available and a fill mode is active: shrink so the scrollbar does not appear.
    Private Sub ShrinkToFit(vis As List(Of KBotDataColumn), available As Integer)
        ' English (slice 0028-02): the floor is EffectiveMinWidth — a column carrying header icons
        ' cannot shrink below what they need, so the shrink pass must count that, not MinWidth.
        Dim minTotal As Integer = 0
        For Each c In vis
            minTotal += c.EffectiveMinWidth
        Next

        ' Honest fallback: even at MinWidth the columns overflow. Pin everything to MinWidth
        ' and let UpdateScrollBars show the horizontal scrollbar — text vanishing entirely is
        ' worse than a scrollbar the caller did not ask for.
        If minTotal >= available Then
            For Each c In vis
                c.SetLayoutWidth(c.EffectiveMinWidth)
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
                If c.Width > c.EffectiveMinWidth Then
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
                c.SetLayoutWidth(Math.Max(c.EffectiveMinWidth, c.Width - shares(i)))   ' floor at the real min
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
