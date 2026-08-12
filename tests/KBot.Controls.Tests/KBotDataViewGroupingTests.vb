Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Testele GRUPĂRII (slice 0029) — secțiunile de grup ale unui raport Access, aduse în grilă.
'''
''' Trei lucruri sunt verificate aici mai insistent decât restul, fiindcă ele sunt exact ce se
''' strică într-o grilă grupată și nu se vede decât cu ochiul, târziu:
''' <list type="number">
''' <item><description><b>sortarea nu poate rupe gruparea</b> — un click pe un antet ordonează
''' ÎNĂUNTRUL grupurilor, nu peste ele, altfel aceeași cheie s-ar împrăștia în listă și ar primi
''' mai multe antete și mai multe totaluri;</description></item>
''' <item><description><b>filtrarea schimbă totalurile, strângerea nu</b> — un filtru scoate
''' rânduri din pagină, o strângere doar le ascunde; dacă cele două s-ar purta la fel, un operator
''' care închide o lună ar vedea totalul general schimbându-se sub ochii lui;</description></item>
''' <item><description><b>indicii publici rămân indici de MODEL</b> — aceeași regulă ca la
''' filtrare (slice 0028-03), acum cu benzi între rânduri.</description></item>
''' </list>
''' </summary>
Public Class KBotDataViewGroupingTests

    ' Grilă cu lună (text), zi (text) și sumă (număr, cu total). Patru luni × câteva rânduri.
    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(800, 400)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("luna", "Luna", KBotColumnType.Text, 120)
        dv.AddColumn("zi", "Zi", KBotColumnType.Text, 80)
        Dim suma = dv.AddColumn("suma", "Sumă", KBotColumnType.Text, 100)
        suma.ValueType = KBotValueType.Number
        suma.Aggregate = KBotAggregate.Sum
        Return dv
    End Function

    Private Shared Sub Umple(dv As KBotDataView, luna As String, zi As String, suma As Double)
        Dim r = dv.AddRow()
        r("luna") = luna
        r("zi") = zi
        r("suma") = suma
    End Sub

    ' Trei rânduri în «ian» (10+20+30) și două în «feb» (40+50). Total general 150.
    Private Shared Function GridCuDouaLuni() As KBotDataView
        Dim dv = Grid()
        Umple(dv, "ian", "01", 10)
        Umple(dv, "ian", "02", 20)
        Umple(dv, "feb", "03", 40)
        Umple(dv, "ian", "03", 30)
        Umple(dv, "feb", "04", 50)
        Return dv
    End Function

    Private Shared Function GrupatPeLuna(dv As KBotDataView) As KBotGroupLevel
        Return dv.GroupBy("luna", KBotSortDirection.Ascending)
    End Function

    ' ── Negrupat: nimic nu se schimbă ────────────────────────────────────────────

    <Fact>
    Public Sub NotGrouped_BandsAreExactlyTheRows()
        Using dv = GridCuDouaLuni()
            Assert.False(dv.IsGrouped)
            Assert.Equal(5, dv.BandCount())
            Assert.Equal("D D D D D", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub NotGrouped_ContentHeightIsRowsTimesRowHeight()
        Using dv = GridCuDouaLuni()
            dv.RowHeight = 20
            Assert.Equal(5 * 20, dv.ContentHeight())
        End Using
    End Sub

    ' ── Benzile ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub OneLevel_EmitsHeaderRowsFooter_PerGroup()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Assert.True(dv.IsGrouped)
            ' «feb» înaintea lui «ian» (crescător): 2 rânduri, apoi 3.
            Assert.Equal("H0 D D F0 H0 D D D F0", dv.DebugBandSummary())
            Assert.Equal(2, dv.GroupCount())
        End Using
    End Sub

    <Fact>
    Public Sub GroupBandsHaveTheirOwnHeights_AndContentHeightCountsThem()
        Using dv = GridCuDouaLuni()
            Dim nivel = GrupatPeLuna(dv)
            dv.RowHeight = 20
            nivel.HeaderHeight = 30
            nivel.FooterHeight = 26
            ' 5 rânduri × 20 + 2 antete × 30 + 2 subsoluri × 26.
            Assert.Equal(5 * 20 + 2 * 30 + 2 * 26, dv.ContentHeight())
        End Using
    End Sub

    <Fact>
    Public Sub LevelWithoutFooter_EmitsNoFooterBand()
        Using dv = GridCuDouaLuni()
            Dim nivel = GrupatPeLuna(dv)
            nivel.ShowFooter = False
            Assert.Equal("H0 D D H0 D D D", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub TwoLevels_SameInnerKeyUnderDifferentOuterKeys_AreTwoGroups()
        Using dv = Grid()
            Umple(dv, "2024", "ian", 1)
            Umple(dv, "2025", "ian", 2)
            dv.Groups.Add(New KBotGroupLevel("luna", KBotSortDirection.Ascending))
            dv.Groups.Add(New KBotGroupLevel("zi", KBotSortDirection.Ascending))
            ' «ian» apare sub amândoi anii, dar sunt DOUĂ grupuri de nivel 1, nu unul.
            Assert.Equal("H0 H1 D F1 F0 H0 H1 D F1 F0", dv.DebugBandSummary())
            Assert.Equal(2, dv.GroupCount(0))
            Assert.Equal(2, dv.GroupCount(1))
        End Using
    End Sub

    ' ── Agregatele ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub GroupAggregate_SumsOnlyItsOwnRows()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            ' Grupul 0 e «feb» (crescător): 40 + 50. Grupul 1 e «ian»: 10 + 20 + 30.
            Assert.Equal("90", dv.DebugGroupAggregate(0, "suma"))
            Assert.Equal("60", dv.DebugGroupAggregate(1, "suma"))
        End Using
    End Sub

    <Fact>
    Public Sub GroupTotals_AddUpToTheGridTotal()
        Using dv = GridCuDouaLuni()
            dv.FooterVisible = True
            GrupatPeLuna(dv)
            Dim suma As Double = 0
            For gi As Integer = 0 To dv.GroupCount() - 1
                suma += Double.Parse(dv.DebugGroupAggregate(gi, "suma"), Globalization.CultureInfo.CurrentCulture)
            Next
            Assert.Equal(dv.DebugFooterText("suma"), suma.ToString(Globalization.CultureInfo.CurrentCulture))
        End Using
    End Sub

    <Fact>
    Public Sub GroupCaption_UsesTheLevelTemplate_WithColumnValueAndCount()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            ' Șablonul implicit: «{0}: {1} ({2})» = titlul coloanei, valoarea, numărul de rânduri.
            Assert.Equal("Luna: feb (2)", dv.DebugGroupCaption(0, antet:=True))
            Assert.Equal("Total ian", dv.DebugGroupCaption(1, antet:=False))
        End Using
    End Sub

    <Fact>
    Public Sub BlankGroupValue_UsesTheEmptyCaption()
        Using dv = Grid()
            Umple(dv, Nothing, "01", 5)
            Dim nivel = GrupatPeLuna(dv)
            nivel.HeaderCaptionFormat = "{1}"
            Assert.Equal("(goale)", dv.DebugGroupCaption(0, antet:=True))
        End Using
    End Sub

    <Fact>
    Public Sub BrokenCaptionTemplate_FallsBackToTheValue_NeverThrows()
        Using dv = GridCuDouaLuni()
            Dim nivel = GrupatPeLuna(dv)
            nivel.HeaderCaptionFormat = "{0} {99"          ' acoladă neînchisă
            Assert.Equal("feb", dv.DebugGroupCaption(0, antet:=True))
        End Using
    End Sub

    ' ── SORTAREA nu poate rupe gruparea ──────────────────────────────────────────

    <Fact>
    Public Sub SortingANonGroupColumn_OrdersInsideGroups_NeverAcrossThem()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            ' Descrescător pe sumă: dacă sortarea ar bate gruparea, rândul de 50 (feb) ar urca
            ' peste tot și grupurile s-ar sparge. Structura benzilor trebuie să rămână intactă.
            dv.ApplySort("suma", KBotSortDirection.Descending)
            Assert.Equal("H0 D D F0 H0 D D D F0", dv.DebugBandSummary())
            Assert.Equal(2, dv.GroupCount())
            ' …iar ÎNĂUNTRU chiar s-a sortat: primul rând al lui «ian» e cel de 30.
            Assert.Equal(30.0, CDbl(dv.Rows(dv.ModelIndexAt(2))("suma")))
            Assert.Equal(10.0, CDbl(dv.Rows(dv.ModelIndexAt(4))("suma")))
        End Using
    End Sub

    <Fact>
    Public Sub SortingTheGroupColumn_FlipsThatLevel_AndLeavesNoRowSort()
        Using dv = GridCuDouaLuni()
            Dim nivel = GrupatPeLuna(dv)
            dv.ApplySort("luna", KBotSortDirection.Descending)

            Assert.Equal(KBotSortDirection.Descending, nivel.SortDirection)
            ' Sortarea de RÂND se ridică: altfel antetul ar purta două semne de sortare deodată.
            Assert.Equal(KBotSortDirection.None, dv.SortDirection)
            ' Și grupurile chiar s-au întors: «ian» primul acum.
            Assert.Equal("Luna: ian (3)", dv.DebugGroupCaption(0, antet:=True))
        End Using
    End Sub

    <Fact>
    Public Sub GroupLevelFor_FindsTheLevelBehindAColumn()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Assert.NotNull(dv.GroupLevelFor("luna"))
            Assert.Null(dv.GroupLevelFor("suma"))
        End Using
    End Sub

    ' ── FILTRAREA: scoate rânduri, deci schimbă și totalurile ────────────────────

    <Fact>
    Public Sub FilteringOutAWholeGroup_RemovesItsBandsToo()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            dv.SetColumnFilter(New KBotColumnFilter("luna") With {
                .SelectedValues = New HashSet(Of String)({"ian"}, StringComparer.CurrentCultureIgnoreCase)})
            ' «feb» a dispărut cu antet, rânduri și subsol cu tot — nu a rămas un antet gol.
            Assert.Equal("H0 D D D F0", dv.DebugBandSummary())
            Assert.Equal(1, dv.GroupCount())
        End Using
    End Sub

    <Fact>
    Public Sub FilteringInsideAGroup_ShrinksItsTotal()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Assert.Equal("60", dv.DebugGroupAggregate(1, "suma"))
            dv.SetColumnFilter(New KBotColumnFilter("zi") With {
                .SelectedValues = New HashSet(Of String)({"01", "03"}, StringComparer.CurrentCultureIgnoreCase)})
            ' «ian» a rămas cu 10 + 30; totalul lui trebuie să urmeze filtrul.
            Dim ian As Integer = If(dv.DebugGroupCaption(0, True).Contains("ian"), 0, 1)
            Assert.Equal("40", dv.DebugGroupAggregate(ian, "suma"))
        End Using
    End Sub

    ' ── STRÂNGEREA: ascunde rânduri, NU schimbă totalurile ───────────────────────

    <Fact>
    Public Sub Collapsing_HidesTheRows_KeepsTheHeader()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Assert.True(dv.DebugToggleGroup(0))                  ' strânge «feb»
            ' Antetul lui rămâne (e singurul loc de unde se redeschide); rândurile și subsolul, nu.
            Assert.Equal("H0 H0 D D D F0", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub Collapsing_DoesNotChangeAnyTotal()
        Using dv = GridCuDouaLuni()
            dv.FooterVisible = True
            GrupatPeLuna(dv)
            Dim totalInainte As String = dv.DebugFooterText("suma")
            Dim grupInainte As String = dv.DebugGroupAggregate(0, "suma")

            dv.DebugToggleGroup(0)

            ' O strângere e o schimbare de AFIȘARE, nu un filtru: pagina a rămas aceeași, deci
            ' și sumele. Dacă ar scădea, operatorul ar vedea totalul mișcându-se singur.
            Assert.Equal(totalInainte, dv.DebugFooterText("suma"))
            Assert.Equal(grupInainte, dv.DebugGroupAggregate(0, "suma"))
        End Using
    End Sub

    <Fact>
    Public Sub Collapsing_ShortensTheScrollableContent()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Dim inainte As Integer = dv.ContentHeight()
            dv.DebugToggleGroup(0)
            Assert.True(dv.ContentHeight() < inainte)
        End Using
    End Sub

    <Fact>
    Public Sub CollapseSurvivesAResort_BecauseItIsKeptByPath()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            dv.DebugToggleGroup(0)                                ' strânge «feb»
            dv.ApplySort("suma", KBotSortDirection.Descending)     ' reconstruiește harta + benzile
            Assert.Equal("H0 H0 D D D F0", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub CollapseAll_ThenExpandAll_RoundTrips()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Dim intreg As String = dv.DebugBandSummary()
            dv.CollapseAllGroups()
            Assert.Equal("H0 H0", dv.DebugBandSummary())
            dv.ExpandAllGroups()
            Assert.Equal(intreg, dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub CollapsedByDefault_StartsClosed_ButAnOperatorsOpeningSticks()
        Using dv = GridCuDouaLuni()
            Dim nivel = GrupatPeLuna(dv)
            nivel.CollapsedByDefault = True
            Assert.Equal("H0 H0", dv.DebugBandSummary())
            dv.ExpandAllGroups()
            ' O reconstrucție de după nu are voie să «corecteze» înapoi ce a desfăcut operatorul.
            dv.ApplySort("suma", KBotSortDirection.Ascending)
            Assert.Equal("H0 D D F0 H0 D D D F0", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub ALevelWithoutAHeader_CannotCollapse()
        Using dv = GridCuDouaLuni()
            Dim nivel = GrupatPeLuna(dv)
            nivel.ShowHeader = False
            Assert.True(nivel.Collapsible)
            Assert.False(nivel.EffectiveCollapsible)
            ' Fără antet nu ar mai exista pe ce apăsa pentru redeschidere, deci nu se strânge deloc.
            Assert.False(dv.DebugToggleGroup(0))
        End Using
    End Sub

    ' ── Selecția și navigarea ────────────────────────────────────────────────────

    <Fact>
    Public Sub PublicRowIndices_StayModelIndices_WhenGrouped()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            ' Gruparea rearanjează ECRANUL, nu modelul: rândul 2 e tot cel încărcat al treilea.
            Assert.Equal("feb", CStr(dv("luna", 2)))
            Assert.Equal(40.0, CDbl(dv("suma", 2)))
        End Using
    End Sub

    <Fact>
    Public Sub ACollapsedRow_KeepsItsSelection_SoItCanBeReopened()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            dv.CurrentRowIndex = 2                        ' un rând din «feb»
            Assert.Equal(2, dv.CurrentRowIndex)

            dv.DebugToggleGroup(0)                        ' se strânge grupul lui
            ' Rândul nu se mai desenează, DAR selecția rămâne — altfel n-ar mai exista de la ce
            ' porni redeschiderea (spre deosebire de un filtru, care chiar scoate rândul).
            Assert.Equal(2, dv.CurrentRowIndex)

            dv.DebugToggleGroup(0)
            Assert.Equal(2, dv.CurrentRowIndex)
        End Using
    End Sub

    <Fact>
    Public Sub AFilteredOutRow_LosesItsSelection_UnlikeACollapsedOne()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            dv.CurrentRowIndex = 2
            dv.SetColumnFilter(New KBotColumnFilter("luna") With {
                .SelectedValues = New HashSet(Of String)({"ian"}, StringComparer.CurrentCultureIgnoreCase)})
            Assert.Equal(-1, dv.CurrentRowIndex)
        End Using
    End Sub

    ' ── Model ────────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub GroupLevel_RefusesSortDirectionNone()
        Dim nivel As New KBotGroupLevel()
        Assert.Throws(Of ArgumentException)(Sub() nivel.SortDirection = KBotSortDirection.None)
    End Sub

    <Fact>
    Public Sub GroupBy_UnknownColumn_Throws()
        Using dv = GridCuDouaLuni()
            Assert.Throws(Of ArgumentException)(Sub() dv.GroupBy("nuexista"))
        End Using
    End Sub

    <Fact>
    Public Sub ALevelWithAnEmptyKey_IsSkippedSilently()
        Using dv = GridCuDouaLuni()
            dv.Groups.Add(New KBotGroupLevel())            ' exact ce inserează designerul la «Add»
            Assert.False(dv.IsGrouped)
            Assert.Equal("D D D D D", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub EndInit_ALevelPointingAtAMissingColumn_Throws()
        Using dv = GridCuDouaLuni()
            dv.BeginInit()
            dv.Groups.Add(New KBotGroupLevel("nuexista", KBotSortDirection.Ascending))
            Assert.Throws(Of ArgumentException)(Sub() dv.EndInit())
        End Using
    End Sub

    <Fact>
    Public Sub RemovingTheGroupColumn_TurnsGroupingOff_WithoutThrowing()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            Assert.True(dv.IsGrouped)
            dv.Columns.Remove(dv.Column("luna"))
            Assert.False(dv.IsGrouped)
            Assert.Equal("D D D D D", dv.DebugBandSummary())
        End Using
    End Sub

    <Fact>
    Public Sub ClearGrouping_ReturnsTheGridToAFlatList()
        Using dv = GridCuDouaLuni()
            GrupatPeLuna(dv)
            dv.ClearGrouping()
            Assert.False(dv.IsGrouped)
            Assert.Equal(5, dv.BandCount())
        End Using
    End Sub

    ' ── Virtualizarea ────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Grouped_PaintingStillCostsOnlyTheVisibleRows()
        Using dv = Grid()
            dv.Size = New Size(800, 300)
            dv.RowHeight = 20
            For i As Integer = 0 To 4999
                Umple(dv, "L" & (i \ 100).ToString(Globalization.CultureInfo.InvariantCulture),
                      i.ToString(Globalization.CultureInfo.InvariantCulture), i)
            Next
            GrupatPeLuna(dv)

            Using bmp As New Bitmap(dv.Width, dv.Height)
                dv.DrawToBitmap(bmp, New Rectangle(0, 0, dv.Width, dv.Height))
            End Using

            ' 5.000 de rânduri, 50 de grupuri — dar o fereastră de 300px la 20px pe rând nu poate
            ' arăta decât vreo 15. Costul pictării rămâne al ferestrei, nu al modelului.
            Assert.True(dv.DebugLastPaintedDataRows > 0)
            Assert.True(dv.DebugLastPaintedDataRows < 40,
                        $"S-au pictat {dv.DebugLastPaintedDataRows} rânduri de date — virtualizarea s-a pierdut.")
        End Using
    End Sub

End Class
