Imports System.Drawing
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Tests for the grid's sort/filter surface (slice 0028-03) — above all THE RULE that keeps the
''' whole feature honest: the public API keeps speaking in MODEL row indices, while only the
''' on-screen geometry moves to view positions.
'''
''' A test that asserts «row 2 still holds what row 2 held» after a filter is not pedantry: every
''' existing view (Sumar, Istoric, Plăți…) stores row indices in its own state, and a filter that
''' renumbered them under those views would corrupt them silently.
''' </summary>
Public Class KBotDataViewFilteringTests

    ' Grilă cu o coloană de text și una numerică, umplută cu patru rânduri cunoscute.
    Private Shared Function Grid() As KBotDataView
        Dim dv As New KBotDataView()
        dv.Size = New Size(600, 300)
        dv.AutoSizeColumnsMode = KBotAutoSizeMode.None
        dv.ApplyTheme(BuiltInSchemes.Classic())
        dv.AddColumn("nume", "Nume", KBotColumnType.Text, 200)
        Dim suma = dv.AddColumn("suma", "Sumă", KBotColumnType.Text, 100)
        suma.ValueType = KBotValueType.Number

        Umple(dv, "Ana", 30)
        Umple(dv, "Barbu", 10)
        Umple(dv, "Cezar", 20)
        Umple(dv, "Ana", 40)
        Return dv
    End Function

    Private Shared Sub Umple(dv As KBotDataView, nume As String, suma As Double)
        Dim r = dv.AddRow()
        r("nume") = nume
        r("suma") = suma
    End Sub

    Private Shared Function FiltruPeValori(colKey As String, ParamArray valori As String()) As KBotColumnFilter
        Return New KBotColumnFilter(colKey) With {
            .SelectedValues = New HashSet(Of String)(valori, StringComparer.CurrentCultureIgnoreCase)}
    End Function

    ' ── Filtrare ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub NoFilter_ViewIsTheModel()
        Using dv = Grid()
            Assert.Equal(4, dv.FilteredRowCount)
            Assert.False(dv.IsFiltered)
        End Using
    End Sub

    <Fact>
    Public Sub ValueFilter_KeepsOnlyCheckedValues()
        Using dv = Grid()
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            Assert.True(dv.IsFiltered)
            Assert.Equal(2, dv.FilteredRowCount)
            Assert.True(dv.HasColumnFilter("nume"))
        End Using
    End Sub

    <Fact>
    Public Sub PublicRowIndices_StayMODELIndices_WhenFiltered()
        Using dv = Grid()
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            ' Ana e pe rândurile de MODEL 0 și 3; ele își păstrează valorile și indicii.
            Assert.Equal("Ana", CStr(dv("nume", 0)))
            Assert.Equal("Ana", CStr(dv("nume", 3)))
            ' Rândul filtrat AFARĂ e în continuare citibil pe indexul lui — e ascuns, nu șters.
            Assert.Equal("Barbu", CStr(dv("nume", 1)))
            Assert.Equal(4, dv.RowCount)
        End Using
    End Sub

    <Fact>
    Public Sub ConditionFilter_AppliesWithoutAValueList()
        Using dv = Grid()
            dv.SetColumnFilter(New KBotColumnFilter("suma") With {
                               .Condition = KBotFilterOperator.GreaterThan, .Operand1 = "15"})
            ' 30, 20 și 40 trec; 10 nu.
            Assert.Equal(3, dv.FilteredRowCount)
        End Using
    End Sub

    <Fact>
    Public Sub TwoFilters_AreCombinedWithAnd()
        Using dv = Grid()
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            dv.SetColumnFilter(New KBotColumnFilter("suma") With {
                               .Condition = KBotFilterOperator.GreaterThan, .Operand1 = "35"})
            ' Doar Ana / 40.
            Assert.Equal(1, dv.FilteredRowCount)
        End Using
    End Sub

    <Fact>
    Public Sub InactiveFilter_ClearsTheColumnInsteadOfMarkingIt()
        Using dv = Grid()
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            ' Un filtru care nu restrânge nimic nu are voie să lase coloana „aprinsă”.
            dv.SetColumnFilter(New KBotColumnFilter("nume"))
            Assert.False(dv.HasColumnFilter("nume"))
            Assert.False(dv.IsFiltered)
            Assert.Equal(4, dv.FilteredRowCount)
        End Using
    End Sub

    <Fact>
    Public Sub ClearAllFilters_BringsEveryRowBack()
        Using dv = Grid()
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            dv.ClearAllFilters()
            Assert.Equal(4, dv.FilteredRowCount)
            Assert.False(dv.IsFiltered)
        End Using
    End Sub

    <Fact>
    Public Sub UnknownColumnKey_Throws_NeverSilentlyIgnored()
        Using dv = Grid()
            Assert.Throws(Of ArgumentException)(Sub() dv.SetColumnFilter(FiltruPeValori("nuexista", "x")))
            Assert.Throws(Of ArgumentException)(Sub() dv.ApplySort("nuexista", KBotSortDirection.Ascending))
            Assert.Throws(Of ArgumentException)(Sub() dv.DistinctDisplayValues("nuexista"))
        End Using
    End Sub

    <Fact>
    Public Sub FilterChanged_IsRaisedOnce_PerChange()
        Using dv = Grid()
            Dim n As Integer = 0
            AddHandler dv.FilterChanged, Sub() n += 1
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            Assert.Equal(1, n)
            dv.ClearAllFilters()
            Assert.Equal(2, n)
        End Using
    End Sub

    ' ── Valorile distincte oferite de meniu ──────────────────────────────────────

    <Fact>
    Public Sub DistinctValues_AreDeduplicatedAndSorted()
        Using dv = Grid()
            Dim v = dv.DistinctDisplayValues("nume")
            Assert.Equal(New String() {"Ana", "Barbu", "Cezar"}, v.ToArray())
        End Using
    End Sub

    <Fact>
    Public Sub DistinctValues_CountBlanksOnce_UnderTheBlankKey()
        Using dv = Grid()
            Umple(dv, Nothing, 1)
            Umple(dv, "   ", 2)
            Dim v = dv.DistinctDisplayValues("nume")
            ' Cele două goale sunt aceeași intrare, iar ea stă prima (goalele se sortează întâi).
            Assert.Equal(KBotFilterEngine.CheieGol, v(0))
            Assert.Equal(4, v.Count)
        End Using
    End Sub

    <Fact>
    Public Sub DistinctValues_IgnoreTheColumnsOwnFilter_ButHonourTheOthers()
        Using dv = Grid()
            ' Filtru pe SUMĂ: rămân 30, 20, 40 => numele Ana și Cezar.
            dv.SetColumnFilter(New KBotColumnFilter("suma") With {
                               .Condition = KBotFilterOperator.GreaterThan, .Operand1 = "15"})
            Assert.Equal(New String() {"Ana", "Cezar"}, dv.DistinctDisplayValues("nume").ToArray())

            ' Filtrul PROPRIU al coloanei nu-i restrânge lista — altfel, odată bifată o valoare,
            ' celelalte ar dispărea și nimeni nu s-ar mai putea răzgândi.
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            Assert.Equal(New String() {"Ana", "Cezar"}, dv.DistinctDisplayValues("nume").ToArray())
        End Using
    End Sub

    ' ── Sortare ──────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Sort_ReordersTheView_NotTheModel()
        Using dv = Grid()
            dv.ApplySort("suma", KBotSortDirection.Ascending)
            Assert.Equal(KBotSortDirection.Ascending, dv.SortDirection)
            Assert.Equal("suma", dv.SortColumnKey)
            ' Modelul e neatins: rândul 0 e tot Ana/30.
            Assert.Equal(30.0R, CDbl(dv("suma", 0)))
            Assert.Equal(4, dv.RowCount)
        End Using
    End Sub

    <Fact>
    Public Sub Sort_None_RestoresTheLoadOrder()
        Using dv = Grid()
            dv.ApplySort("suma", KBotSortDirection.Descending)
            dv.ApplySort("suma", KBotSortDirection.None)
            Assert.Equal(KBotSortDirection.None, dv.SortDirection)
            Assert.Null(dv.SortColumnKey)
        End Using
    End Sub

    <Fact>
    Public Sub SortChanged_IsRaised()
        Using dv = Grid()
            Dim n As Integer = 0
            AddHandler dv.SortChanged, Sub() n += 1
            dv.ApplySort("nume", KBotSortDirection.Ascending)
            Assert.Equal(1, n)
        End Using
    End Sub

    ' ── Subsolul urmează filtrul ─────────────────────────────────────────────────

    <Fact>
    Public Sub Footer_AggregatesOnlyTheRowsThatPassTheFilter()
        Using dv = Grid()
            dv.FooterVisible = True
            dv.Column("suma").Aggregate = KBotAggregate.Sum
            ' Fără filtru: 30 + 10 + 20 + 40 = 100.
            Assert.Equal("100", dv.DebugFooterText("suma"))

            ' Cu filtru pe Ana: 30 + 40 = 70. Un total care ar aduna și rândurile ascunse ar fi,
            ' pentru cine citește pagina, o greșeală de calcul.
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            Assert.Equal("70", dv.DebugFooterText("suma"))
        End Using
    End Sub

    ' ── Selecția ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Selection_IsDropped_WhenItsRowIsFilteredOut()
        Using dv = Grid()
            dv.CurrentRowIndex = 1                       ' Barbu
            Assert.Equal(1, dv.CurrentRowIndex)
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            ' Barbu nu se mai vede: o selecție pe un rând invizibil ar muta săgețile în gol.
            Assert.Equal(-1, dv.CurrentRowIndex)
        End Using
    End Sub

    <Fact>
    Public Sub Selection_Survives_WhenItsRowStillPasses()
        Using dv = Grid()
            dv.CurrentRowIndex = 3                       ' Ana / 40
            dv.SetColumnFilter(FiltruPeValori("nume", "Ana"))
            Assert.Equal(3, dv.CurrentRowIndex)
        End Using
    End Sub

End Class
