Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Frozen (static) column painting: a horizontally-scrolled scroll cell must never bleed into
''' the frozen band — the static column is always drawn on top, on an opaque background. Headless
''' via DrawToBitmap: a scroll cell is given a bright, unmistakable background, then we scroll it
''' under the frozen band and assert that colour is absent from the frozen band's pixels.
''' </summary>
Public Class KBotDataViewFrozenColumnTests

    Private Const ViewW As Integer = 200
    Private Const ViewH As Integer = 200

    <Fact>
    Public Sub FrozenColumn_NoScrollBleed_WhenHorizontallyScrolled()
        Using dv As New KBotDataView()
            dv.Size = New Size(ViewW, ViewH)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None   ' keep explicit widths => force overflow

            dv.BeginUpdate()
            dv.AddColumn("frozen", "F", KBotColumnType.Text, 80)
            For i As Integer = 0 To 4
                dv.AddColumn("s" & i.ToString(), "S" & i.ToString(), KBotColumnType.Text, 120)
            Next
            dv.FrozenColumnCount = 1
            Dim r = dv.AddRow()
            r("frozen") = ""                                  ' frozen band shows only row background
            For i As Integer = 0 To 4
                r("s" & i.ToString()) = ""
            Next
            dv.EndUpdate()

            ' Paint every SCROLL cell background a bright, unique red via formatting.
            Dim red As Color = Color.FromArgb(255, 0, 0)
            AddHandler dv.CellFormatting,
                Sub(s, e)
                    If e.ColumnKey <> "frozen" Then e.BackColor = red
                End Sub

            ' Overflow => horizontal scrollbar; scroll so a red scroll cell slides under the band.
            Assert.True(dv.hScroll.Visible, "coloanele trebuie să depășească viewport-ul")
            dv.hScroll.Value = 60

            Using bmp As New Bitmap(ViewW, ViewH)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, ViewW, ViewH))

                ' Sample several points well inside the frozen band, on the data row: none may be red.
                Dim rowY As Integer = dv.HeaderHeight + dv.RowHeight \ 2
                For Each x As Integer In New Integer() {6, 20, 40, 60, 74}
                    Dim px As Color = bmp.GetPixel(x, rowY)
                    Assert.False(px.R = red.R AndAlso px.G = red.G AndAlso px.B = red.B,
                                 $"banda înghețată nu are voie să arate roșul benzii derulate (x={x})")
                Next
            End Using
        End Using
    End Sub

    ' The frozen band must still paint itself even with no rows (header-only), without throwing.
    <Fact>
    Public Sub FrozenHeader_PaintsWithoutRows()
        Using dv As New KBotDataView()
            dv.Size = New Size(ViewW, ViewH)
            dv.ApplyTheme(BuiltInSchemes.Classic())
            dv.AddColumn("frozen", "F", KBotColumnType.Text, 80)
            dv.AddColumn("s0", "S0", KBotColumnType.Text, 120)
            dv.FrozenColumnCount = 1
            Using bmp As New Bitmap(ViewW, ViewH)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, ViewW, ViewH))
            End Using
            Assert.Equal(0, dv.DebugLastPaintedDataRows)
        End Using
    End Sub

End Class
