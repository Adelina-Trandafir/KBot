Imports Xunit

''' <summary>
''' Tests for the pure sort/filter engine (slice 0028-03): typed comparison, the ordering of blanks
''' and of values that do not read in the column's type, condition matching, and the per-type
''' offer of operators.
'''
''' Everything here is <c>Shared</c> and control-free on purpose — this is the layer that decides
''' what "smaller" and "contains" mean, and it has to be assertable without a window.
''' </summary>
Public Class KBotFilterEngineTests

    ' ── Comparație ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Compare_Numbers_UsesNumericOrder_NotAlphabetic()
        ' Alfabetic, «9» ar fi după «10» — capcana pe care o rezolvă tipul.
        Assert.True(KBotFilterEngine.Compare(9, 10, KBotValueType.Number) < 0)
        Assert.True(KBotFilterEngine.Compare("9", "10", KBotValueType.Number) < 0)
        ' Pe o coloană de TEXT, aceleași două valori se compară chiar alfabetic.
        Assert.True(KBotFilterEngine.Compare("9", "10", KBotValueType.Text) > 0)
    End Sub

    <Fact>
    Public Sub Compare_Blanks_SortFirst_AndAreAllTheSame()
        Assert.True(KBotFilterEngine.Compare(Nothing, "orice", KBotValueType.Text) < 0)
        Assert.True(KBotFilterEngine.Compare("orice", Nothing, KBotValueType.Text) > 0)
        ' Nothing, șirul vid și spațiile sunt aceeași stare: o celulă necompletată.
        Assert.Equal(0, KBotFilterEngine.Compare(Nothing, "   ", KBotValueType.Text))
        Assert.Equal(0, KBotFilterEngine.Compare("", Nothing, KBotValueType.Number))
    End Sub

    <Fact>
    Public Sub Compare_UnreadableValue_SortsAfterReadableOne()
        ' O notă text pe o coloană numerică: se citește ca număr ce se poate, restul vine după.
        Assert.True(KBotFilterEngine.Compare(5, "n/a", KBotValueType.Number) < 0)
        Assert.True(KBotFilterEngine.Compare("n/a", 5, KBotValueType.Number) > 0)
        ' Două necitibile se compară între ele ca text (deci ordinea e stabilă, nu arbitrară).
        Assert.True(KBotFilterEngine.Compare("a", "b", KBotValueType.Number) < 0)
    End Sub

    <Fact>
    Public Sub Compare_Dates_UsesChronology()
        Dim vechi As New Date(2020, 1, 31)
        Dim nou As New Date(2026, 8, 10)
        Assert.True(KBotFilterEngine.Compare(vechi, nou, KBotValueType.DateTime) < 0)
        Assert.True(KBotFilterEngine.Compare(nou, vechi, KBotValueType.DateTime) > 0)
        Assert.Equal(0, KBotFilterEngine.Compare(nou, nou, KBotValueType.DateTime))
    End Sub

    <Fact>
    Public Sub Compare_Booleans_UncheckedBeforeChecked()
        Assert.True(KBotFilterEngine.Compare(False, True, KBotValueType.Boolean) < 0)
        Assert.Equal(0, KBotFilterEngine.Compare(True, True, KBotValueType.Boolean))
    End Sub

    ' ── Oferta de condiții ───────────────────────────────────────────────────────

    <Fact>
    Public Sub AllowedOperators_AreGatedByValueType()
        ' «Conține» pe numere și «Între» pe text sunt întrebări fără răspuns.
        Assert.False(KBotFilterEngine.IsAllowed(KBotValueType.Number, KBotFilterOperator.Contains))
        Assert.False(KBotFilterEngine.IsAllowed(KBotValueType.Text, KBotFilterOperator.Between))
        Assert.True(KBotFilterEngine.IsAllowed(KBotValueType.Text, KBotFilterOperator.Contains))
        Assert.True(KBotFilterEngine.IsAllowed(KBotValueType.Number, KBotFilterOperator.Between))
        Assert.True(KBotFilterEngine.IsAllowed(KBotValueType.DateTime, KBotFilterOperator.Between))
    End Sub

    <Fact>
    Public Sub AllowedOperators_Boolean_OffersNone()
        ' Cele două căsuțe din listă spun deja tot ce se poate spune despre o bifă.
        Assert.Empty(KBotFilterEngine.AllowedOperators(KBotValueType.Boolean))
    End Sub

    <Fact>
    Public Sub OperandCount_MatchesTheOperator()
        Assert.Equal(0, KBotFilterEngine.OperandCount(KBotFilterOperator.IsEmpty))
        Assert.Equal(0, KBotFilterEngine.OperandCount(KBotFilterOperator.None))
        Assert.Equal(1, KBotFilterEngine.OperandCount(KBotFilterOperator.Contains))
        Assert.Equal(2, KBotFilterEngine.OperandCount(KBotFilterOperator.Between))
    End Sub

    <Fact>
    Public Sub OperatorCaption_ReadsDifferentlyOnDates()
        ' Despre o dată nimeni nu spune că e «mai mică».
        Assert.Equal("Înainte de…", KBotFilterEngine.OperatorCaption(KBotFilterOperator.LessThan, KBotValueType.DateTime))
        Assert.Equal("Mai mic decât…", KBotFilterEngine.OperatorCaption(KBotFilterOperator.LessThan, KBotValueType.Number))
    End Sub

    ' ── Potrivire ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub TextConditions_MatchOnTheDisplayedText_CaseInsensitively()
        Assert.True(Match("x", "Angajament 12", KBotValueType.Text, KBotFilterOperator.Contains, "ajament"))
        Assert.True(Match("x", "Angajament 12", KBotValueType.Text, KBotFilterOperator.Contains, "ANGAJ"))
        Assert.False(Match("x", "Angajament 12", KBotValueType.Text, KBotFilterOperator.Contains, "zz"))
        Assert.True(Match("x", "Angajament 12", KBotValueType.Text, KBotFilterOperator.BeginsWith, "ang"))
        Assert.True(Match("x", "Angajament 12", KBotValueType.Text, KBotFilterOperator.EndsWith, "12"))
        ' Negațiile sunt exact negațiile, nu o a doua regulă.
        Assert.False(Match("x", "Angajament 12", KBotValueType.Text, KBotFilterOperator.NotContains, "ajament"))
    End Sub

    <Fact>
    Public Sub SizeConditions_CompareTheRawValue_Numerically()
        Assert.True(Match(9, "9", KBotValueType.Number, KBotFilterOperator.LessThan, "10"))
        Assert.False(Match(90, "90", KBotValueType.Number, KBotFilterOperator.LessThan, "10"))
        Assert.True(Match(90, "90", KBotValueType.Number, KBotFilterOperator.GreaterThan, "10"))
    End Sub

    <Fact>
    Public Sub Between_IncludesBothEnds_AndToleratesSwappedOperands()
        Assert.True(Match(10, "10", KBotValueType.Number, KBotFilterOperator.Between, "10", "20"))
        Assert.True(Match(20, "20", KBotValueType.Number, KBotFilterOperator.Between, "10", "20"))
        Assert.False(Match(21, "21", KBotValueType.Number, KBotFilterOperator.Between, "10", "20"))
        ' Capetele date invers descriu tot un interval.
        Assert.True(Match(15, "15", KBotValueType.Number, KBotFilterOperator.Between, "20", "10"))
    End Sub

    <Fact>
    Public Sub EmptyConditions_LookAtTheRawValue()
        Assert.True(Match(Nothing, "0,00", KBotValueType.Number, KBotFilterOperator.IsEmpty, Nothing))
        Assert.False(Match(0, "0,00", KBotValueType.Number, KBotFilterOperator.IsEmpty, Nothing))
        Assert.True(Match(0, "0,00", KBotValueType.Number, KBotFilterOperator.IsNotEmpty, Nothing))
    End Sub

    <Fact>
    Public Sub UnreadableOperand_MakesTheConditionInert_NotEmptying()
        ' «mai mic decât <ceva ce nu e număr>» nu are voie să golească grila: arătăm tot.
        Assert.True(Match(5, "5", KBotValueType.Number, KBotFilterOperator.LessThan, "nu-i număr"))
        Assert.True(Match(5, "5", KBotValueType.Number, KBotFilterOperator.Between, "x", "y"))
    End Sub

    <Fact>
    Public Sub BlankValue_IsNeitherAboveNorBelowAnOperand()
        Assert.False(Match(Nothing, "", KBotValueType.Number, KBotFilterOperator.LessThan, "10"))
        Assert.False(Match(Nothing, "", KBotValueType.Number, KBotFilterOperator.GreaterThan, "10"))
        Assert.False(Match(Nothing, "", KBotValueType.Number, KBotFilterOperator.Between, "1", "10"))
    End Sub

    Private Shared Function Match(raw As Object, display As String, tip As KBotValueType,
                                  op As KBotFilterOperator, o1 As String,
                                  Optional o2 As String = Nothing) As Boolean
        Return KBotFilterEngine.MatchesCondition(raw, display, tip, op, o1, o2)
    End Function

End Class
