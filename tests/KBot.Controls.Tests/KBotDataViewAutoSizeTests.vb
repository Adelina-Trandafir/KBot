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

    ' ── Coloana elastică cedează chiar cu ShrinkColumnsToFit = False ─────────────
    '
    ' Configurația e cea din DdfValoriPage: umplere pe prima coloană, strâmtare generală stinsă,
    ' lățimi autorate care încap pe lat DOAR cât timp nu e bară verticală. Când bara apare, cei
    ' 17px ai ei fac diferența, iar înainte de felia asta apărea și bara orizontală — deși coloana
    ' elastică avea zeci de pixeli deasupra minimului ei.

    ' Un grid ca al paginii Valori: 5 coloane, 410px autorați, umplere pe prima, fără strâmtare.
    Private Shared Function NewValoriLikeGrid(w As Integer, rows As Integer) As KBotDataView
        Dim dv = NewGrid(w, 300)
        dv.BeginUpdate()
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ColumnFillMode = KBotFillMode.FirstColumn
        dv.ShrinkColumnsToFit = False
        dv.AddColumn("clsf", "Clasificație", KBotColumnType.Text, 120)
        dv.Column("clsf").MinWidth = 50
        dv.AddColumn("element", "Element", KBotColumnType.Text, 50)
        For Each k In {"valprec", "valcur", "valtot"}
            dv.AddColumn(k, "Valoare", KBotColumnType.Text, 80)
        Next
        For i As Integer = 1 To rows
            Dim r = dv.AddRow()
            r("clsf") = "20.01.02"
        Next
        dv.EndUpdate()
        Return dv
    End Function

    Private Shared Function VScrollVisible(dv As KBotDataView) As Boolean
        For Each c As Control In dv.Controls
            If TypeOf c Is VScrollBar Then Return c.Visible
        Next
        Return False
    End Function

    ''' <summary>
    ''' Cazul raportat: bara verticală apare, iar totalul trebuie să încapă în ce a rămas —
    ''' fără bară orizontală. Coloana elastică cedează exact cei 17px.
    ''' </summary>
    <Fact>
    Public Sub FillTarget_CedeazaLatimeaBareiVerticale_ChiarFaraShrink()
        ' 420 clienți, 410 autorați: încape fără bară verticală, NU încape cu ea.
        Using dv = NewValoriLikeGrid(420, 200)
            Assert.True(VScrollVisible(dv), "cazul cere bara verticală vizibilă")

            Dim disponibil As Integer = dv.ClientSize.Width - SystemInformation.VerticalScrollBarWidth
            Assert.Equal(disponibil, SumVisibleWidths(dv))
            Assert.True(dv.Column("clsf").Width < 120, "coloana elastică trebuie să fi cedat")
            Assert.True(dv.Column("clsf").Width >= dv.Column("clsf").MinWidth)
        End Using
    End Sub

    ''' <summary>Celelalte coloane rămân neatinse — «nu le strâmta» e respectat pentru ele.</summary>
    <Fact>
    Public Sub FillTarget_CedeazaSingura_CelelalteRamanLaLatimeaLor()
        Using dv = NewValoriLikeGrid(420, 200)
            Assert.Equal(50, dv.Column("element").Width)
            For Each k In {"valprec", "valcur", "valtot"}
                Assert.Equal(80, dv.Column(k).Width)
            Next
        End Using
    End Sub

    ''' <summary>
    ''' Fără bară verticală (puține rânduri) totul încape, iar coloana elastică CREȘTE ca înainte:
    ''' cedarea e o cale nouă pe depășire, nu o schimbare a umplerii.
    ''' </summary>
    <Fact>
    Public Sub FaraBaraVerticala_ColoanaElasticaCreste_CaInainte()
        Using dv = NewValoriLikeGrid(420, 2)
            Assert.False(VScrollVisible(dv))
            Assert.Equal(dv.ClientSize.Width, SumVisibleWidths(dv))
            Assert.True(dv.Column("clsf").Width > 120, "spațiul rămas se cheltuie pe coloana aleasă")
        End Using
    End Sub

    ''' <summary>
    ''' Podeaua ține: dacă nici coloana elastică golită până la minim nu ajunge, lățimile se
    ''' opresc acolo și bara orizontală apare — răspunsul onest, nu text tăiat.
    ''' </summary>
    <Fact>
    Public Sub FillTarget_NuCoboaraSubMinim_SiAtunciBaraOrizontalaEsteCorecta()
        Using dv = NewValoriLikeGrid(200, 200)
            Assert.Equal(dv.Column("clsf").MinWidth, dv.Column("clsf").Width)
            Assert.True(SumVisibleWidths(dv) > dv.ClientSize.Width,
                        "depășirea rămasă e reală, deci bara orizontală e corectă")
        End Using
    End Sub

    ''' <summary>
    ''' <c>Proportional</c> nu are o singură coloană elastică — acolo «nu strâmta» rămâne întreg,
    ''' altfel comutatorul n-ar mai însemna nimic.
    ''' </summary>
    <Fact>
    Public Sub Proportional_NuCedeazaNimic_CandShrinkEsteStins()
        Using dv = NewGrid(200, 300)
            dv.BeginUpdate()
            dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
            dv.ColumnFillMode = KBotFillMode.Proportional
            dv.ShrinkColumnsToFit = False
            dv.AddColumn("a", "A", KBotColumnType.Text, 150)
            dv.AddColumn("b", "B", KBotColumnType.Text, 150)
            dv.EndUpdate()

            Assert.Equal(150, dv.Column("a").Width)
            Assert.Equal(150, dv.Column("b").Width)
        End Using
    End Sub

    ' ── Rotunjirea logic ↔ scalat nu are voie să depășească ──────────────────────
    '
    ' Lățimile se țin LOGIC (px la 96 dpi) și se re-derivă la folosire, deci fiecare scriere a
    ' trecerii face dus-întorsul round(v / s) * s. La scara 1 e exact, deci toate testele de mai
    ' sus n-au putut vedea nimic; la 125% dus-întorsul poate ieși cu UN pixel peste cât s-a cerut,
    ' iar un pixel peste lățimea disponibilă e o bară orizontală. Așa se vedea în DdfValoriPage
    ' după ce coloana elastică a început să cedeze: 639 disponibili, 640 desenați.
    '
    ' Scara e stare GLOBALĂ (AppScaling), deci se pune și se pune la loc în Finally. Se poate
    ' face fără curse: paralelizarea e stinsă pe tot assembly-ul (vezi TestAssemblyInfo.vb).
    ' Configure persistă setarea, de aceea restaurarea nu e opțională.
    Private Shared Sub RunAtScale(factor As Single, body As Action)
        Dim modVechi As ScalingMode = AppScaling.Mode
        Dim facVechi As Single = AppScaling.ManualFactor
        Try
            AppScaling.Configure(ScalingMode.Manual, factor)
            body()
        Finally
            AppScaling.Configure(modVechi, facVechi)
        End Try
    End Sub

    ' Lățimea PICTATĂ a unei coloane. WidthPx e Friend în KBot.Controls, deci se reface aici cu
    ' aceeași formulă (ScalePx): valoarea logică înmulțită cu scara grilei.
    Private Shared Function WidthPxOf(dv As KBotDataView, key As String) As Integer
        Return CInt(Math.Round(dv.Column(key).Width * dv.DpiScaleX))
    End Function

    Private Shared Function SumWidthsPx(dv As KBotDataView) As Integer
        Dim total As Integer = 0
        For Each c In dv.Columns
            If c.Visible Then total += CInt(Math.Round(c.Width * dv.DpiScaleX))
        Next
        Return total
    End Function

    ''' <summary>
    ''' La 125%, o grilă ca DdfValoriPage cu bară verticală trebuie să încapă pe lat. Nu se cere
    ''' potrivire EXACTĂ: un pixel de joc rămas nefolosit nu se vede, unul în plus costă o bară.
    ''' </summary>
    <Fact>
    Public Sub La125_TotalulNuDepasesteSpatiulDisponibil()
        RunAtScale(1.25F,
            Sub()
                For Each w In {500, 656, 700}
                    Using dv = NewValoriLikeGrid(w, 200)
                        dv.RefreshDpiMetrics()
                        Assert.True(VScrollVisible(dv), "cazul cere bara verticală")

                        Dim disponibil As Integer = dv.ClientSize.Width -
                                                    SystemInformation.VerticalScrollBarWidth
                        Dim total As Integer = SumWidthsPx(dv)
                        Assert.True(total <= disponibil,
                                    $"la client={w}: {total}px desenați în {disponibil}px disponibili")
                        Assert.True(disponibil - total <= 1,
                                    $"la client={w}: {disponibil - total}px nefolosiți — prea mult joc")
                    End Using
                Next
            End Sub)
    End Sub

    ''' <summary>
    ''' Creșterea coloanei de umplere nu depășește nici ea: e cealaltă jumătate a aceleiași
    ''' rotunjiri, pe cazul în care există spațiu rămas de cheltuit.
    ''' </summary>
    <Fact>
    Public Sub La125_CrestereaColoaneiElasticeNuDepaseste()
        RunAtScale(1.25F,
            Sub()
                Using dv = NewValoriLikeGrid(900, 2)
                    dv.RefreshDpiMetrics()
                    Assert.False(VScrollVisible(dv), "puține rânduri — fără bară verticală")

                    Assert.True(SumWidthsPx(dv) <= dv.ClientSize.Width,
                                $"{SumWidthsPx(dv)}px desenați în {dv.ClientSize.Width}px")
                    Assert.True(WidthPxOf(dv, "clsf") > CInt(Math.Round(120 * dv.DpiScaleX)),
                                "coloana elastică trebuie să fi crescut")
                End Using
            End Sub)
    End Sub

End Class
