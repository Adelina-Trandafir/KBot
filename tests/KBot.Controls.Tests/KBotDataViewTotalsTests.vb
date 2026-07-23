Imports System.Drawing
Imports System.Globalization
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Tests for the pinned totals row (slice 0017-01). They cover the computation (each aggregate;
''' non-numeric / Nothing skipped; Average over nothing is empty; Count via HasValue), the
''' recompute triggers (AddRow / ClearRows / EndUpdate / committed edit), the exclusion of the
''' band from RowCount + navigation, and the layout math (the body and the vertical scrollbar
''' both shrink by the band height). Frozen-column ALIGNMENT is a painted concern verified in the
''' DevHarness; here it is only smoke-tested (a frozen column's total computes and paint runs).
'''
''' Numeric assertions parse the formatted text back with CurrentCulture, so they do not depend
''' on the test host's decimal separator; Count asserts the plain integer string directly.
''' </summary>
Public Class KBotDataViewTotalsTests

    Private Shared Function ParseNum(text As String) As Double
        Return Double.Parse(text, NumberStyles.Any, CultureInfo.CurrentCulture)
    End Function

    ' A grid with a text/count column and two numeric/sum columns, totals ON.
    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 400)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("cod", "Cod", KBotColumnType.Text, 120).Aggregate = KBotAggregate.Count
        Dim s = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120)
        s.FormatString = "N2"
        s.Aggregate = KBotAggregate.Sum
        Dim a = dv.AddColumn("medie", "Medie", KBotColumnType.Text, 120)
        a.Aggregate = KBotAggregate.Average
        dv.ShowTotalsRow = True
        Return dv
    End Function

    ' ── Computation ──────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Sum_Count_Average_ComputeCorrectly()
        Using dv = Grid()
            For Each v In New Double() {10.0, 20.0, 30.0}
                dv.AddRow()
                Dim i = dv.RowCount - 1
                dv("cod", i) = "X"
                dv("suma", i) = v
                dv("medie", i) = v
            Next

            Assert.Equal("3", dv.DebugTotalsText("cod"))              ' Count of rows with a value
            Assert.Equal(60.0, ParseNum(dv.DebugTotalsText("suma")), 2)
            Assert.Equal(20.0, ParseNum(dv.DebugTotalsText("medie")), 2)
        End Using
    End Sub

    <Fact>
    Public Sub Sum_And_Average_SkipNonNumericAndNothing()
        Using dv = Grid()
            dv.AddRow() : dv("suma", 0) = 10.0 : dv("medie", 0) = 10.0
            dv.AddRow() : dv("suma", 1) = "abc" : dv("medie", 1) = "abc"      ' non-numeric -> skipped
            dv.AddRow() : dv("suma", 2) = Nothing : dv("medie", 2) = Nothing  ' Nothing -> skipped
            dv.AddRow() : dv("suma", 3) = 30.0 : dv("medie", 3) = 30.0

            ' Sum = 10 + 30 = 40; Average = 40 / 2 countable = 20 (the two skipped do not count).
            Assert.Equal(40.0, ParseNum(dv.DebugTotalsText("suma")), 2)
            Assert.Equal(20.0, ParseNum(dv.DebugTotalsText("medie")), 2)
        End Using
    End Sub

    <Fact>
    Public Sub Average_WithNoCountableCells_IsEmptyNotZeroNorNaN()
        Using dv = Grid()
            dv.AddRow() : dv("medie", 0) = "x"
            dv.AddRow() : dv("medie", 1) = Nothing
            Assert.Equal(String.Empty, dv.DebugTotalsText("medie"))
        End Using
    End Sub

    <Fact>
    Public Sub Count_CountsRowsThatHaveAStoredValue_NotNonEmptyCells()
        Using dv = Grid()
            dv.AddRow() : dv("cod", 0) = "A"                ' has value
            dv.AddRow() : dv("cod", 1) = Nothing            ' stored Nothing -> still HAS a value
            dv.AddRow()                                     ' never set "cod" -> no stored value
            ' Two rows have a stored "cod" value; the third never set it.
            Assert.Equal("2", dv.DebugTotalsText("cod"))
        End Using
    End Sub

    <Fact>
    Public Sub AggregateFormatString_OverridesColumnFormat_AndCountIgnoresBoth()
        Using dv As New KBotDataView()
            dv.AddColumn("cod", "Cod", KBotColumnType.Text, 120).Aggregate = KBotAggregate.Count
            Dim s = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120)
            s.FormatString = "N2"
            s.AggregateFormatString = "N0"                  ' aggregate-specific format wins
            s.Aggregate = KBotAggregate.Sum
            dv.ShowTotalsRow = True

            dv.AddRow() : dv("cod", 0) = "x" : dv("suma", 0) = 1234.56
            ' Count ignores every format string -> plain integer.
            Assert.Equal("1", dv.DebugTotalsText("cod"))
            ' Sum uses AggregateFormatString "N0" -> no decimals (parses back to 1235).
            Assert.Equal(1235.0, ParseNum(dv.DebugTotalsText("suma")), 0)
        End Using
    End Sub

    <Fact>
    Public Sub NoAggregate_Column_RendersEmptyTotalsCell()
        Using dv As New KBotDataView()
            dv.AddColumn("plain", "Plain", KBotColumnType.Text, 120)   ' Aggregate = None (default)
            dv.ShowTotalsRow = True
            dv.AddRow() : dv("plain", 0) = "value"
            Assert.Equal(String.Empty, dv.DebugTotalsText("plain"))
        End Using
    End Sub

    <Fact>
    Public Sub TotalsOff_ComputesNothing()
        Using dv As New KBotDataView()
            dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120).Aggregate = KBotAggregate.Sum
            dv.AddRow() : dv("suma", 0) = 5.0
            ' ShowTotalsRow left False -> no cached text.
            Assert.Equal(String.Empty, dv.DebugTotalsText("suma"))
        End Using
    End Sub

    ' ── Recompute triggers ───────────────────────────────────────────────────────

    <Fact>
    Public Sub Totals_RecomputeAfterAddRowAndClearRows()
        Using dv = Grid()
            dv.AddRow() : dv("suma", 0) = 100.0
            Assert.Equal(100.0, ParseNum(dv.DebugTotalsText("suma")), 2)

            dv.AddRow() : dv("suma", 1) = 50.0
            Assert.Equal(150.0, ParseNum(dv.DebugTotalsText("suma")), 2)

            dv.ClearRows()
            Assert.Equal(0.0, ParseNum(dv.DebugTotalsText("suma")), 2)   ' Sum of nothing = 0
            Assert.Equal("0", dv.DebugTotalsText("cod"))                 ' Count of nothing = 0
        End Using
    End Sub

    <Fact>
    Public Sub Totals_RecomputeOnceAfterEndUpdate_NotPerAddRow()
        Using dv = Grid()
            dv.BeginUpdate()
            For i As Integer = 1 To 5
                dv.AddRow()("suma") = 10.0
            Next
            dv.EndUpdate()
            Assert.Equal(50.0, ParseNum(dv.DebugTotalsText("suma")), 2)
        End Using
    End Sub

    <Fact>
    Public Sub Totals_RecomputeAfterCommittedEdit()
        Using dv As New KBotDataView()
            dv.Size = New Size(500, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            Dim s = dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120)
            s.Aggregate = KBotAggregate.Sum
            dv.ShowTotalsRow = True
            dv.AddRow() : dv("suma", 0) = 10.0
            Assert.Equal(10.0, ParseNum(dv.DebugTotalsText("suma")), 2)

            Assert.True(dv.BeginEdit("suma", 0))
            dv.editText.Text = "99"
            Assert.True(dv.CommitEdit())
            ' The committed edit recomputed the total.
            Assert.Equal(99.0, ParseNum(dv.DebugTotalsText("suma")), 2)
        End Using
    End Sub

    ' ── Exclusion from the row model ─────────────────────────────────────────────

    <Fact>
    Public Sub TotalsRow_ExcludedFromRowCount()
        Using dv = Grid()
            dv.AddRow()
            dv.AddRow()
            Assert.Equal(2, dv.RowCount)          ' the band is not a row
        End Using
    End Sub

    <Fact>
    Public Sub TotalsRow_NotReachableByNavigation()
        Using dv = Grid()
            dv.AddRow()
            dv.AddRow()
            dv.CurrentColumnKey = "suma"
            dv.CurrentRowIndex = 1                 ' last real row
            ' Moving down cannot land on the totals band (it clamps to the last real row).
            dv.CurrentRowIndex = 99
            Assert.Equal(-1, dv.CurrentRowIndex)   ' out of range -> deselect, never the band
            dv.CurrentRowIndex = 1
            Assert.Equal(1, dv.CurrentRowIndex)
        End Using
    End Sub

    ' ── Layout math ──────────────────────────────────────────────────────────────

    ' The body must shrink by the band height: fewer virtualized rows paint, and the vertical
    ' scrollbar's viewport (LargeChange) drops by the same amount UpdateScrollBars removed.
    <Fact>
    Public Sub ShowingTotals_ShrinksBody_AndScrollbarViewport()
        Using dv As New KBotDataView()
            dv.Size = New Size(400, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.AddColumn("suma", "Suma", KBotColumnType.Text, 120).Aggregate = KBotAggregate.Sum
            For i As Integer = 1 To 200                       ' enough to force the vertical bar
                dv.AddRow()("suma") = CDbl(i)
            Next

            PaintOnce(dv)
            Dim paintedOff As Integer = dv.DebugLastPaintedDataRows
            Dim largeOff As Integer = dv.vScroll.LargeChange
            Assert.True(dv.vScroll.Visible)

            dv.TotalsRowHeight = 56                           ' ~2 rows at RowHeight 28
            dv.ShowTotalsRow = True
            PaintOnce(dv)
            Dim paintedOn As Integer = dv.DebugLastPaintedDataRows
            Dim largeOn As Integer = dv.vScroll.LargeChange

            Assert.True(paintedOn < paintedOff,
                        $"body did not shrink: {paintedOn} vs {paintedOff}")
            Assert.Equal(largeOff - 56, largeOn)              ' scrollbar viewport agrees exactly
        End Using
    End Sub

    ' Frozen-column smoke test: a frozen column carrying a Sum still computes its total, and a
    ' full paint with a horizontal scroll engaged does not throw. Pixel alignment is eyeballed
    ' in the DevHarness (the visual harness is still unrun — see the worklog).
    <Fact>
    Public Sub FrozenColumn_TotalsComputeAndPaintDoesNotThrow()
        Using dv As New KBotDataView()
            dv.Size = New Size(300, 300)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.AddColumn("cod", "Cod", KBotColumnType.Text, 120).Aggregate = KBotAggregate.Count
            For i As Integer = 1 To 6
                dv.AddColumn("c" & i.ToString(), "C" & i.ToString(), KBotColumnType.Text, 120).
                    Aggregate = KBotAggregate.Sum
            Next
            dv.FrozenColumnCount = 1
            dv.ScrollByColumn = True
            dv.ShowTotalsRow = True
            ' Row-indexer load inside a batch: EndUpdate recomputes once with every cell set.
            dv.BeginUpdate()
            For r As Integer = 1 To 4
                Dim row = dv.AddRow()
                row("cod") = "R" & r.ToString()
                For i As Integer = 1 To 6
                    row("c" & i.ToString()) = CDbl(r * i)
                Next
            Next
            dv.EndUpdate()

            Assert.Equal("4", dv.DebugTotalsText("cod"))      ' frozen column total present
            PaintOnce(dv)                                     ' must not throw with H-scroll + frozen
        End Using
    End Sub

    Private Shared Sub PaintOnce(dv As KBotDataView)
        Using bmp As New Bitmap(dv.Width, dv.Height)
            dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
        End Using
    End Sub

End Class
