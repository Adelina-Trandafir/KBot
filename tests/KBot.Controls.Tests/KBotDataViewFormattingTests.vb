Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Teste pentru slice 0010-04: rezoluția activării EFECTIVE (coloană × rând × celulă) și
''' evenimentele de formatare. Culoarea pictată nu se poate citi headless, dar CONTRACTUL
''' care o determină — da, și el e partea care contează pentru input/editare (05/06).
''' </summary>
Public Class KBotDataViewFormattingTests

    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(400, 200)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("a", "A", KBotColumnType.Text, 100)
        dv.AddColumn("b", "B", KBotColumnType.Text, 100)
        dv.AddRow()
        dv.AddRow()
        Return dv
    End Function

    ' ── Cele trei niveluri de dezactivare ───────────────────────────────────────

    <Fact>
    Public Sub AllLevelsEnabled_ByDefault()
        Using dv = Grid()
            Assert.True(dv.IsRowEnabled(0))
            Assert.True(dv.IsCellEnabled("a", 0))
        End Using
    End Sub

    <Fact>
    Public Sub ColumnDisable_DisablesEveryCellInThatColumnOnly()
        Using dv = Grid()
            dv.Column("a").Enabled = False
            Assert.False(dv.IsCellEnabled("a", 0))
            Assert.False(dv.IsCellEnabled("a", 1))
            Assert.True(dv.IsCellEnabled("b", 0))     ' cealaltă coloană rămâne activă
            Assert.True(dv.IsRowEnabled(0))           ' rândul în sine nu e afectat
        End Using
    End Sub

    <Fact>
    Public Sub RowDisable_DisablesEveryCellInThatRowOnly()
        Using dv = Grid()
            dv.Rows(0).Enabled = False
            Assert.False(dv.IsRowEnabled(0))
            Assert.False(dv.IsCellEnabled("a", 0))
            Assert.False(dv.IsCellEnabled("b", 0))
            Assert.True(dv.IsCellEnabled("a", 1))     ' celălalt rând rămâne activ
        End Using
    End Sub

    <Fact>
    Public Sub CellDisable_ViaFormattingEvent_HitsOneCellOnly()
        Using dv = Grid()
            AddHandler dv.CellFormatting,
                Sub(s, e)
                    If e.ColumnKey = "a" AndAlso e.RowIndex = 0 Then e.Enabled = False
                End Sub
            Assert.False(dv.IsCellEnabled("a", 0))
            Assert.True(dv.IsCellEnabled("a", 1))
            Assert.True(dv.IsCellEnabled("b", 0))
        End Using
    End Sub

    <Fact>
    Public Sub RowFormattingVeto_DisablesWholeRow()
        Using dv = Grid()
            dv.Rows(1)("a") = "Anulat"
            AddHandler dv.RowFormatting,
                Sub(s, e)
                    ' Regula de acceptanță: Stare = «Anulat» => tot rândul inert.
                    If TypeOf e.Row("a") Is String AndAlso CStr(e.Row("a")) = "Anulat" Then e.Enabled = False
                End Sub
            Assert.True(dv.IsRowEnabled(0))
            Assert.False(dv.IsRowEnabled(1))
            Assert.False(dv.IsCellEnabled("b", 1))
        End Using
    End Sub

    <Fact>
    Public Sub CellHandler_CannotEnableAboveDisabledColumnOrRow()
        Using dv = Grid()
            dv.Column("a").Enabled = False
            dv.Rows(1).Enabled = False
            AddHandler dv.CellFormatting, Sub(s, e) e.Enabled = True   ' încearcă să ridice
            Assert.False(dv.IsCellEnabled("a", 0))   ' coloana dezactivată bate handler-ul
            Assert.False(dv.IsCellEnabled("b", 1))   ' rândul dezactivat bate handler-ul
        End Using
    End Sub

    <Fact>
    Public Sub IsCellEnabled_RejectsUnknownColumn()
        Using dv = Grid()
            Assert.Throws(Of ArgumentException)(Function() dv.IsCellEnabled("inexistent", 0))
        End Using
    End Sub

    ' ── Evenimentele de formatare la pictare ────────────────────────────────────

    <Fact>
    Public Sub FormattingEvents_FireForVisibleCellsOnPaint()
        Using dv = Grid()
            Dim rowRaises As Integer = 0
            Dim cellRaises As Integer = 0
            AddHandler dv.RowFormatting, Sub(s, e) rowRaises += 1
            AddHandler dv.CellFormatting, Sub(s, e) cellRaises += 1

            Using bmp As New Bitmap(400, 200)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, 400, 200))
            End Using

            Assert.Equal(2, rowRaises)               ' două rânduri vizibile
            Assert.Equal(4, cellRaises)              ' × două coloane
        End Using
    End Sub

    <Fact>
    Public Sub CellFormatting_CanOverrideDisplayedTextWithoutTouchingTheValue()
        Using dv = Grid()
            dv("a", 0) = 42
            AddHandler dv.CellFormatting, Sub(s, e) e.Text = "suprascris"
            Using bmp As New Bitmap(400, 200)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, 400, 200))
            End Using
            ' Afișarea s-a schimbat, dar valoarea din model NU.
            Assert.Equal(42, dv("a", 0))
        End Using
    End Sub

    <Fact>
    Public Sub DisabledCells_StillPaintWithoutThrowing()
        Using dv = Grid()
            dv.Column("a").Enabled = False
            dv.Rows(1).Enabled = False
            Using bmp As New Bitmap(400, 200)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, 400, 200))
            End Using
            Assert.Equal(2, dv.DebugLastPaintedDataRows)
        End Using
    End Sub

End Class
