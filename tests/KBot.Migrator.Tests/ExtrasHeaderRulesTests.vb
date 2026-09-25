Imports System.Linq
Imports Xunit

''' <summary>
''' FX_Extrase_H's IdUnitate / IdClsf / IdClsfV are computed from Cont / CodIBAN with the
''' extrase download's rules (operator, 25.09.2026). The cases are the operator's own rows.
''' </summary>
Public Class ExtrasHeaderRulesTests

    <Theory>
    <InlineData("23A65030210010129164800", "23A")>
    <InlineData("01A65000059000029164800", "01A")>
    <InlineData("500529164800", "500")>
    <InlineData("2A", "2A")>
    <InlineData("", "")>
    Public Sub The_source_is_the_first_three_characters_of_the_account(cont As String, expected As String)
        Assert.Equal(expected, ExtrasHeaderRules.SourcePrefix(cont))
    End Sub

    <Fact>
    Public Sub A_missing_account_has_an_empty_source()
        Assert.Equal(String.Empty, ExtrasHeaderRules.SourcePrefix(Nothing))
    End Sub

    <Theory>
    <InlineData("23A65030210010129164800", "RO14TREZ23A650302100101X", "650302100101")>
    <InlineData("24A65040120013029164800", "RO41TREZ24A650401200130X", "650401200130")>
    <InlineData("21E30053029164800", "RO38TREZ52121E300530XXXX", "300530")>
    Public Sub With_an_IBAN_the_classification_is_cut_from_the_account(cont As String, iban As String, expected As String)
        Assert.Equal(expected, ExtrasHeaderRules.ClsfSalFrom(cont, iban))
    End Sub

    <Theory>
    <InlineData("21E99999629164800", "RO91TREZ52121E999996XXXX")>
    <InlineData("24E99999629164800", "RO58TREZ24E999996XXXXXXX")>
    <InlineData("500529164800", "RO62TREZ5215005XXX018003")>
    Public Sub Only_a_6_or_a_3_after_the_source_gives_a_classification(cont As String, iban As String)
        Assert.Equal(String.Empty, ExtrasHeaderRules.ClsfSalFrom(cont, iban))
    End Sub

    <Theory>
    <InlineData(Nothing)>
    <InlineData("")>
    Public Sub Without_an_IBAN_there_is_no_classification(iban As String)
        Assert.Equal(String.Empty, ExtrasHeaderRules.ClsfSalFrom("01A65000059000029164800", iban))
    End Sub

    <Fact>
    Public Sub A_short_account_is_cut_as_far_as_it_goes()
        Assert.Equal("6504", ExtrasHeaderRules.ClsfSalFrom("24A6504", "RO"))
        Assert.Equal(String.Empty, ExtrasHeaderRules.ClsfSalFrom("24A", "RO"))
    End Sub

    <Fact>
    Public Sub A_500_prefix_is_read_on_four_characters()
        ' Operator, 25.09.2026: 5005 = 02A, 5006 = 01A. A unit, never a classification.
        Assert.Equal("02A", ExtrasHeaderRules.Source500X("5005"))
        Assert.Equal("01A", ExtrasHeaderRules.Source500X("5006"))
        Assert.Equal(2, ExtrasHeaderRules.Source500X.Count)
        Assert.Equal(String.Empty, ExtrasHeaderRules.ClsfSalFrom("500529164800", "RO62TREZ5215005XXX018003"))
    End Sub

    <Fact>
    Public Sub The_catalogue_computes_the_three_columns_and_keeps_the_marker_check()
        Dim map = TableMaps.All().Single(Function(m) m.TargetTable = "FX_Extrase_H")
        Dim ruled = map.Derived.Where(Function(d) d.Kind = ColumnSourceKind.ExtrasHeaderRule).
            Select(Function(d) d.TargetColumn).OrderBy(Function(n) n).ToList()
        Assert.Equal({"IdClsf", "IdClsfV", "IdUnitate"}.ToList(), ruled)
        Assert.All(map.Derived.Where(Function(d) d.Kind = ColumnSourceKind.ExtrasHeaderRule),
                   Sub(d) Assert.False(d.BlockingOnMiss))
        Assert.DoesNotContain(map.Derived, Function(d) d.Kind = ColumnSourceKind.ClasificatieByRowUnit)
        Assert.True(map.HasClsfPair)
    End Sub

End Class
