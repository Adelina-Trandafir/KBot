Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports Xunit
Imports KBot.Domain

' Slice 0081-04: the pure half of the send (workflow inputs, reading forexecab's grid back,
' the check against section A, captures, the resume filter). Written, not run (house rule).
Public Class DdfSendInputsTests

    Private Shared Function Line(id As Integer, clsf As String, ss As String, cod As String,
                                 cur As Double, tot As Double) As DdfSendLine
        Return New DdfSendLine() With {.IdSecA = id, .Clsf = clsf, .Ss = ss, .CodIndicator = cod,
                                       .ValPrec = 0, .ValCur = cur, .ValTot = tot}
    End Function

    Private Shared Function Row(ssi As String, cod As String, initial As String, definitiv As String) As RandTabel
        Return New RandTabel From {
            {"Sector_Sursa_Indicator", ssi},
            {"Indicator_ang", cod},
            {"Credit_bugetar_rezervat_initial", initial},
            {"Credit_bugetar_rezervat_definitiv_an_curent", definitiv}}
    End Function

    ' ── Inputs ────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Motiv_AppendsTheRevisionTag()
        Assert.Equal("Achizitie hartie (REV:2)", DdfSendInputs.Motiv("Achizitie hartie", 2))
    End Sub

    <Fact>
    Public Sub Motiv_EmptyDescription_IsTheTagAlone()
        Assert.Equal("(REV:0)", DdfSendInputs.Motiv("  ", 0))
    End Sub

    <Fact>
    Public Sub Motiv_AlreadyTagged_IsNotTaggedTwice()
        Assert.Equal("X (REV:1)", DdfSendInputs.Motiv("X (REV:1)", 1))
    End Sub

    <Fact>
    Public Sub FormatSuma_IsRomanian()
        Assert.Equal("1.380,00", DdfSendInputs.FormatSuma(1380))
    End Sub

    <Fact>
    Public Sub ParseSuma_ReadsRomanian_AndRefusesText()
        Assert.Equal(1380.5R, DdfSendInputs.ParseSuma("1.380,50").Value)
        Assert.False(DdfSendInputs.ParseSuma("abc").HasValue)
        Assert.False(DdfSendInputs.ParseSuma("").HasValue)
    End Sub

    <Fact>
    Public Sub CreareRows_CarryTheKeyAndTheInitialValue()
        Dim rows = DdfSendInputs.CreareRows({Line(7, "65.04.02.20.01.01", "02A", "!abc", 100, 100)}, "P1")
        Assert.Single(rows)
        Assert.Equal("IdSecA-7", rows(0)("Cheie"))
        Assert.Equal("65.04.02.20.01.01", rows(0)("Clasificatia"))
        Assert.Equal("P1", rows(0)("CodProgram"))
        Assert.Equal("02A", rows(0)("Sursa"))
        Assert.Equal("100,00", rows(0)("CB_INITIAL"))
    End Sub

    <Fact>
    Public Sub IncarcaRows_NewRowGetsTheMarker_ExistingKeepsItsCode()
        Dim rows = DdfSendInputs.IncarcaRows({Line(1, "65.04", "02A", "!x", 0, 50), Line(2, "65.05", "02A", "AAB", 0, 70)},
                                             "P1", "M (REV:1)")
        Assert.Equal(DdfSendInputs.NewRowMarker, rows(0)("Indicator_ang"))
        Assert.Equal("AAB", rows(1)("Indicator_ang"))
        Assert.Equal("6505", rows(1)("ClsfSal"))
        Assert.Equal("70,00", rows(1)("ValoareNoua"))
        Assert.Equal("M (REV:1)", rows(1)("Motiv"))
    End Sub

    ' ── The grid ──────────────────────────────────────────────────────────────

    <Fact>
    Public Sub GridRowFor_MatchesOnSsAndClassification()
        Dim grid As New List(Of RandTabel) From {
            Row("02B - 65.04.02.20.01.01", "AAA", "0", "0"),
            Row("02A - 65.04.02.20.01.01", "AAB", "0", "0")}
        Assert.Equal("AAB", DdfSendInputs.RowCode(grid, Line(1, "65.04.02.20.01.01", "02A", "!x", 0, 0)))
    End Sub

    <Fact>
    Public Sub RowCode_NoMatch_IsEmpty()
        Dim grid As New List(Of RandTabel) From {Row("02A - 65.04.02.20.01.01", "AAB", "0", "0")}
        Assert.Equal(String.Empty, DdfSendInputs.RowCode(grid, Line(1, "70.01", "02A", "!x", 0, 0)))
    End Sub

    <Fact>
    Public Sub Differences_Initial_ComparesCbInitialWithCurrentValue()
        Dim grid As New List(Of RandTabel) From {Row("02A - 65.04", "AAB", "100,00", "0,00")}
        Assert.Empty(DdfSendInputs.Differences(grid, {Line(1, "65.04", "02A", "!x", 100, 100)}, initial:=True))
        Assert.Single(DdfSendInputs.Differences(grid, {Line(1, "65.04", "02A", "!x", 90, 90)}, initial:=True))
    End Sub

    <Fact>
    Public Sub Differences_NotInitial_ComparesDefinitiveWithTotal_AndReportsMissingRows()
        Dim grid As New List(Of RandTabel) From {Row("02A - 65.04", "AAB", "0,00", "150,00")}
        Dim diffs = DdfSendInputs.Differences(grid, {Line(1, "65.04", "02A", "AAB", 50, 150),
                                                     Line(2, "70.01", "02A", "!y", 10, 10)}, initial:=False)
        Assert.Single(diffs)
        Assert.Contains("70.01", diffs(0))
    End Sub

    <Fact>
    Public Sub LinesToResume_SkipsDoneLines_AndSetsTheCodeOfExistingOnes()
        Dim grid As New List(Of RandTabel) From {
            Row("02A - 65.04", "AAB", "0", "150,00"),
            Row("02A - 65.05", "AAC", "0", "10,00")}
        Dim rest = DdfSendInputs.LinesToResume(grid, {
            Line(1, "65.04", "02A", "AAB", 0, 150),
            Line(2, "65.05", "02A", "!z", 0, 70),
            Line(3, "70.01", "02A", "!y", 0, 5)})
        Assert.Equal(2, rest.Count)
        Assert.Equal("AAC", rest(0).CodIndicator)
        Assert.False(rest(0).EsteRandNou)
        Assert.True(rest(1).EsteRandNou)
    End Sub

    ' ── What a run leaves behind ──────────────────────────────────────────────

    <Fact>
    Public Sub Captures_KeepOnlyPozaBase64_InOrder()
        Dim png As String = Convert.ToBase64String(New Byte() {137, 80, 78, 71})
        Dim data As New Dictionary(Of String, String) From {
            {"Poza_IdSecA-1", png},
            {"CodAng_Final", "AAB123"},
            {"Poza_Lista", "[""a"",""b""]"},
            {"Poza_Stricata", "not base64 !"},
            {"Poza_Final", png}}
        Dim c = DdfSendInputs.Captures(data)
        Assert.Equal({"Poza_IdSecA-1", "Poza_Final"}, c.Select(Function(x) x.Key).ToArray())
    End Sub

    <Fact>
    Public Sub CaptureFileName_IsAscii()
        Assert.Equal("Poza_IdSecA-1.png", DdfSendInputs.CaptureFileName("Poza_IdSecA-1"))
        Assert.Equal("Poza__.png", DdfSendInputs.CaptureFileName("Poza_" & ChrW(&H103)))
    End Sub

    <Theory>
    <InlineData("ANG2026000123", "ANG2026000123")>
    <InlineData("Cod: ANG2026000123 Stare: Initial", "ANG2026000123")>
    <InlineData("", "")>
    <InlineData("fara cod aici", "")>
    Public Sub CodeFromText_ReadsTheCode(text As String, expected As String)
        Assert.Equal(expected, DdfSendInputs.CodeFromText(text))
    End Sub

    ' Slice 0081-10: Access's «.02» after the chapter never reaches forexecab.
    <Theory>
    <InlineData("65.02.04.02.20.01.01", "65.04.02.20.01.01")>
    <InlineData(" 65.02.04.02.20.01.01 ", "65.04.02.20.01.01")>
    <InlineData("65.04.02.20.01.01", "65.04.02.20.01.01")>
    <InlineData("", "")>
    <InlineData(Nothing, "")>
    Public Sub ForexeClsf_DropsTheAccessHalfOfTheChapter(clsf As String, expected As String)
        Assert.Equal(expected, DdfSendInputs.ForexeClsf(clsf))
    End Sub

    <Fact>
    Public Sub FromDraft_SendsTheForexeClassification()
        Dim l As DdfSendLine = DdfSendLine.FromDraft(New DdfDraftLinieA() With {.Clsf = "65.02.04.02.20.01.01", .Ss = "02A"})
        Assert.Equal("65.04.02.20.01.01", l.Clsf)
        Assert.Equal("650402200101", l.ClsfSal)
    End Sub
End Class
