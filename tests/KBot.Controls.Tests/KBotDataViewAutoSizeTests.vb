Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0013: column auto-sizing and fill modes. Width maths needs no screen, so these run
''' headless — the control is instantiated without a handle (ClientSize follows Size) and the
''' pass runs on EndUpdate / AutoSizeColumns just as it would live. The exact pixel width of a
''' string is font/DPI dependent, so the assertions lean on relative comparisons and on the
''' invariants the plan fixes (sums that match the viewport, caps and floors that hold).
''' </summary>
Public Class KBotDataViewAutoSizeTests

    Private Shared Function NewGrid(w As Integer, h As Integer) As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(w, h)
        dv.ApplyTheme(BuiltInSchemes.Classic())
        Return dv
    End Function

    Private Shared Function SumVisibleWidths(dv As KBotDataView) As Integer
        Dim total As Integer = 0
        For Each c In dv.Columns
            If c.Visible Then total += c.Width
        Next
        Return total
    End Function

    ' ── Defaults ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Defaults_ToContentAndNoneFill()
        Using dv As New KBotDataView()
            Assert.Equal(KBotAutoSizeMode.ToContent, dv.AutoSizeColumnsMode)
            Assert.Equal(KBotFillMode.None, dv.ColumnFillMode)
            Assert.Equal(200, dv.AutoSizeSampleRows)
        End Using
    End Sub

    ' ── ToContent ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub ToContent_WidensColumnToFitWideCell()
        Using dv = NewGrid(600, 400)
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            r("a") = "a fairly long cell value that easily beats the header"
            dv.EndUpdate()

            Dim cellWidth As Integer = TextRenderer.MeasureText(CStr(dv("a", 0)), dv.Font).Width
            Assert.True(dv.Column("a").Width >= cellWidth,
                        $"coloana ({dv.Column("a").Width}) trebuie să cuprindă textul ({cellWidth})")
            Assert.True(dv.Column("a").Width > 40, "coloana trebuie să fi crescut peste MinWidth")
        End Using
    End Sub

    <Fact>
    Public Sub ToContent_UsesHeaderWhenHeaderIsWider()
        Using dv = NewGrid(600, 400)
            dv.BeginUpdate()
            dv.AddColumn("wide", "A very long header caption goes here", KBotColumnType.Text, 40)
            dv.AddColumn("narrow", "B", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            r("wide") = "x"
            r("narrow") = "x"
            dv.EndUpdate()

            ' Same cell text on both: the wider column can only come from the header.
            Assert.True(dv.Column("wide").Width > dv.Column("narrow").Width,
                        "antetul mai lat trebuie să lățească coloana")
        End Using
    End Sub

    <Fact>
    Public Sub ToContent_RespectsMinAndMaxWidth()
        Using dv = NewGrid(1000, 400)
            dv.BeginUpdate()
            dv.AddColumn("mn", "M", KBotColumnType.Text, 40)
            dv.Column("mn").MinWidth = 200
            dv.AddColumn("mx", "M", KBotColumnType.Text, 40)
            dv.Column("mx").MaxWidth = 60
            Dim r = dv.AddRow()
            r("mn") = "x"                                       ' tiny content, MinWidth wins
            r("mx") = New String("W"c, 100)                     ' huge content, MaxWidth caps
            dv.EndUpdate()

            Assert.Equal(200, dv.Column("mn").Width)
            Assert.Equal(60, dv.Column("mx").Width)
        End Using
    End Sub

    <Fact>
    Public Sub ToContent_MeasuresFormattedValueNotRaw()
        Using dv = NewGrid(1000, 400)
            dv.BeginUpdate()
            dv.AddColumn("fmt", "F", KBotColumnType.Text, 40)
            dv.Column("fmt").FormatString = "N2"
            dv.AddColumn("raw", "R", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            r("fmt") = 123456.0                                 ' formats to e.g. 123,456.00
            r("raw") = "123456"
            dv.EndUpdate()

            ' The formatted money value is visibly wider than its 6 raw digits.
            Assert.True(dv.Column("fmt").Width > dv.Column("raw").Width,
                        "valoarea formatată N2 trebuie măsurată mai lată decât cifrele brute")
        End Using
    End Sub

    ' ── Fill: spend the leftover ─────────────────────────────────────────────────

    <Fact>
    Public Sub Fill_LastColumn_AbsorbsLeftoverExactly()
        Using dv = NewGrid(500, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("b", "B", KBotColumnType.Text, 100)
            dv.AddColumn("c", "C", KBotColumnType.Text, 100)
            dv.AddRow()
            dv.EndUpdate()

            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))
            Assert.Equal(100, dv.Column("a").Width)             ' only the last grew
            Assert.Equal(100, dv.Column("b").Width)
            Assert.True(dv.Column("c").Width > 100)
        End Using
    End Sub

    <Fact>
    Public Sub Fill_FirstColumn_AbsorbsLeftoverExactly()
        Using dv = NewGrid(500, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.FirstColumn
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("b", "B", KBotColumnType.Text, 100)
            dv.AddColumn("c", "C", KBotColumnType.Text, 100)
            dv.AddRow()
            dv.EndUpdate()

            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))
            Assert.True(dv.Column("a").Width > 100)             ' only the first grew
            Assert.Equal(100, dv.Column("b").Width)
            Assert.Equal(100, dv.Column("c").Width)
        End Using
    End Sub

    <Fact>
    Public Sub Fill_Proportional_SplitsExactlyAndByWidth()
        Using dv = NewGrid(700, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.Proportional
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("b", "B", KBotColumnType.Text, 200)     ' twice as wide as a
            dv.AddColumn("c", "C", KBotColumnType.Text, 100)
            dv.AddRow()
            dv.EndUpdate()

            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))  ' exact, no right-edge gap
            Dim growA As Integer = dv.Column("a").Width - 100
            Dim growB As Integer = dv.Column("b").Width - 200
            Assert.True(growB > growA, "coloana mai lată trebuie să ia o parte mai mare")
            Assert.True(Math.Abs(growB - 2 * growA) <= 3, "≈ dublul, până la rotunjire")
        End Using
    End Sub

    <Fact>
    Public Sub Fill_Proportional_CappedColumnHoldsAndSurplusGoesToOthers()
        Using dv = NewGrid(1000, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.Proportional
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.Column("a").MaxWidth = 120                        ' will hit its cap
            dv.AddColumn("b", "B", KBotColumnType.Text, 100)
            dv.AddColumn("c", "C", KBotColumnType.Text, 100)
            dv.AddRow()
            dv.EndUpdate()

            Assert.Equal(120, dv.Column("a").Width)              ' cap holds
            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))  ' surplus fully redistributed
            Assert.True(dv.Column("b").Width > 100)
            Assert.True(dv.Column("c").Width > 100)
        End Using
    End Sub

    ' ── Overflow ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Overflow_WithNoneFill_ShowsHorizontalScrollbar()
        Using dv = NewGrid(300, 400)                             ' narrow viewport
            ' Default ToContent + None. Wide cells push the columns past the viewport.
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 40)
            dv.AddColumn("b", "B", KBotColumnType.Text, 40)
            dv.AddColumn("c", "C", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            Dim wide As String = New String("W"c, 40)
            r("a") = wide : r("b") = wide : r("c") = wide
            dv.EndUpdate()

            Assert.True(dv.hScroll.Visible, "None + overflow => bară orizontală")
        End Using
    End Sub

    <Fact>
    Public Sub Overflow_WithFillMode_ShrinksAndHidesScrollbar()
        Using dv = NewGrid(300, 400)
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 40)
            dv.AddColumn("b", "B", KBotColumnType.Text, 40)
            dv.AddColumn("c", "C", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            Dim wide As String = New String("W"c, 40)
            r("a") = wide : r("b") = wide : r("c") = wide
            dv.EndUpdate()

            Assert.False(dv.hScroll.Visible, "un mod de umplere nu lasă bară orizontală")
            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))
            For Each c In dv.Columns
                Assert.True(c.Width >= c.MinWidth, "nicio coloană sub MinWidth")
            Next
        End Using
    End Sub

    <Fact>
    Public Sub Overflow_MinWidthsExceedViewport_FallsBackToMinWidthAndScrollbar()
        Using dv = NewGrid(150, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None       ' keep the explicit widths
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.BeginUpdate()
            For i As Integer = 0 To 4
                dv.AddColumn("c" & i.ToString(), "C" & i.ToString(), KBotColumnType.Text, 100)
            Next
            dv.AddRow()
            dv.EndUpdate()

            ' 5 * MinWidth(40) = 200 > 150 viewport: the honest fallback.
            For Each c In dv.Columns
                Assert.Equal(c.MinWidth, c.Width)
            Next
            Assert.True(dv.hScroll.Visible, "sub sum(MinWidth) > available bara reapare")
        End Using
    End Sub

    ' ── Hidden columns ───────────────────────────────────────────────────────────

    <Fact>
    Public Sub HiddenColumns_TakePartInNothing()
        Using dv = NewGrid(500, 400)
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("h", "H", KBotColumnType.Text, 100)
            dv.Column("h").Visible = False
            dv.AddColumn("c", "C", KBotColumnType.Text, 100)
            dv.AddRow()
            dv.EndUpdate()

            Assert.Equal(100, dv.Column("h").Width)              ' untouched
            Assert.Equal(100, dv.Column("a").Width)
            Assert.Equal(dv.ClientSize.Width, dv.Column("a").Width + dv.Column("c").Width)
            Assert.True(dv.Column("c").Width > 100)              ' last VISIBLE column grew
        End Using
    End Sub

    ' ── Sampling ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Sampling_TwoRowsIgnoresFarWideValue_ZeroPicksItUp()
        Using dv = NewGrid(2000, 2000)                           ' wide + tall: no shrink, no vscroll
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 40)
            For i As Integer = 0 To 59
                Dim r = dv.AddRow()
                r("a") = If(i = 50, New String("W"c, 60), "x")   ' the wide value is far down
            Next
            dv.EndUpdate()

            dv.AutoSizeSampleRows = 2
            Dim sampled As Integer = dv.Column("a").Width

            dv.AutoSizeSampleRows = 0                            ' measure every row
            Dim full As Integer = dv.Column("a").Width

            Assert.True(full > sampled, "măsurarea completă trebuie să prindă valoarea din rândul 50")
        End Using
    End Sub

    ' ── Manual drag interaction ──────────────────────────────────────────────────

    <Fact>
    Public Sub UserSized_NotReMeasured_ButResetRestoresAuto()
        Using dv = NewGrid(2000, 800)
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 40)
            dv.AddColumn("b", "B", KBotColumnType.Text, 40)
            Dim r = dv.AddRow()
            r("a") = "x"
            r("b") = "x"
            dv.EndUpdate()

            ' Simulate a manual drag on column a.
            dv.Column("a").UserSized = True
            dv.Column("a").Width = 250
            dv.AutoSizeColumns()

            Assert.Equal(250, dv.Column("a").Width)              ' ToContent skips a dragged column
            Assert.True(dv.Column("b").Width < 250, "coloana ne-trasă e măsurată la conținut")

            dv.ResetColumnSizing()
            Assert.True(dv.Column("a").Width < 250, "resetarea repune auto-măsurarea")
        End Using
    End Sub

    <Fact>
    Public Sub UserSized_StillParticipatesInShrink()
        Using dv = NewGrid(200, 400)
            dv.ColumnFillMode = KBotFillMode.LastColumn
            dv.BeginUpdate()
            dv.AddColumn("a", "A", KBotColumnType.Text, 100)
            dv.AddColumn("b", "B", KBotColumnType.Text, 100)
            Dim r = dv.AddRow()
            r("a") = "x"
            r("b") = "x"
            dv.EndUpdate()

            ' Drag a wide, then force a pass: a is not re-measured, but shrink still reaches it.
            dv.Column("a").UserSized = True
            dv.Column("a").Width = 300
            dv.AutoSizeColumns()

            Assert.True(dv.Column("a").Width < 300, "micșorarea se aplică și coloanelor trase")
            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))
            Assert.True(dv.Column("a").Width >= dv.Column("a").MinWidth)
            Assert.True(dv.Column("b").Width >= dv.Column("b").MinWidth)
        End Using
    End Sub

End Class
