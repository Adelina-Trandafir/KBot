Option Strict On
Imports Xunit
Imports KBot.Domain

' Pure-model tests for the DDF POCOs (slice 0020-01). They pin the two derived properties
' the view and the file browser depend on, both of which encode a rule read out of the
' Access source rather than guessed:
'   * DdfAntet.FolderPdf   — the partener/GENERAL folder convention from mdl_FX_DDF_PDF;
'   * RevizieRow.EtichetaRevizie — Format(NumarRev, "@@@") is a TEXT format (spaces), not D3;
'   * DdfInfo.AntetDeLucru — the explicit header pick, because FX_DDF's PK is composite
'     (IDDF, CUAL) and nothing enforces one header per CodAngajament.
Public Class DdfInfoTests

    ' --- DdfAntet.FolderPdf / NormalizeazaNume -------------------------------------------

    <Fact>
    Public Sub FolderPdf_IsGeneral_WhenNotPartenerBound()
        Dim a As New DdfAntet() With {.PartAng = False, .NumePartener = "TERMO PLOIESTI"}
        ' PartAng False -> GENERAL, chiar dacă numele partenerului este completat.
        Assert.Equal("GENERAL", a.FolderPdf)
    End Sub

    <Fact>
    Public Sub FolderPdf_IsGeneral_WhenPartenerNameIsBlank()
        Dim a As New DdfAntet() With {.PartAng = True, .NumePartener = "   "}
        Assert.Equal("GENERAL", a.FolderPdf)
    End Sub

    <Fact>
    Public Sub FolderPdf_UsesNormalisedPartenerName()
        Dim a As New DdfAntet() With {.PartAng = True, .NumePartener = "TERMO PLOIESTI"}
        Assert.Equal("TERMO_PLOIESTI", a.FolderPdf)
    End Sub

    <Theory>
    <InlineData("AVATAR SOFT SRL", "AVATAR_SOFT_SRL")>
    <InlineData("S.C. Bin Go Solutions S.R.L.", "S_C_Bin_Go_Solutions_S_R_L")>
    <InlineData("  Dedeman  ", "Dedeman")>
    <InlineData("A---B", "A_B")>
    <InlineData("A / B \ C", "A_B_C")>
    Public Sub NormalizeazaNume_CollapsesRunsOfNonWordChars(intrare As String, asteptat As String)
        ' VBA înlocuiește \W+ (un GRUP întreg), nu \W, deci «---» devine UN «_», iar
        ' separatoarele de la capete se taie.
        Assert.Equal(asteptat, DdfAntet.NormalizeazaNume(intrare))
    End Sub

    <Fact>
    Public Sub NormalizeazaNume_EmptyFallsBackToGeneral()
        Assert.Equal("GENERAL", DdfAntet.NormalizeazaNume(""))
        Assert.Equal("GENERAL", DdfAntet.NormalizeazaNume(Nothing))
    End Sub

    ' --- RevizieRow.EtichetaRevizie -------------------------------------------------------

    <Fact>
    Public Sub EtichetaRevizie_PadsWithSpaces_NotZeroes()
        ' §2.6: Format(NumarRev, "@@@") este un format TEXT — aliniere la dreapta în trei
        ' caractere, umplut cu SPAȚII. «000»/D3 ar produce «000 - …», ceea ce e greșit.
        Dim r As New RevizieRow() With {.NumarRev = 0, .DataRev = New Date(2026, 1, 18)}
        Assert.Equal("  0 - 18.01.2026", r.EtichetaRevizie)
    End Sub

    <Fact>
    Public Sub EtichetaRevizie_HandlesTwoAndThreeDigits()
        Assert.Equal(" 12 - 11.02.2026",
                     New RevizieRow() With {.NumarRev = 12, .DataRev = New Date(2026, 2, 11)}.EtichetaRevizie)
        Assert.Equal("123 - 11.02.2026",
                     New RevizieRow() With {.NumarRev = 123, .DataRev = New Date(2026, 2, 11)}.EtichetaRevizie)
    End Sub

    <Fact>
    Public Sub EtichetaRevizie_MissingDate_LeavesDatePartEmpty()
        Dim r As New RevizieRow() With {.NumarRev = 1, .DataRev = Nothing}
        Assert.Equal("  1 - ", r.EtichetaRevizie)
    End Sub

    ' --- DdfInfo.AntetDeLucru -------------------------------------------------------------

    Private Shared Function CuAntete(ParamArray idduri As Integer()) As DdfInfo
        Dim info As New DdfInfo()
        For Each id In idduri
            info.Antet.Add(New DdfAntet() With {.Iddf = id})
        Next
        Return info
    End Function

    <Fact>
    Public Sub AntetDeLucru_NoHeaders_ReturnsNothing()
        Assert.Null(New DdfInfo().AntetDeLucru(0))
        Assert.Null(New DdfInfo().AntetDeLucru(9900201))
    End Sub

    <Fact>
    Public Sub AntetDeLucru_PrefersMatchingIddf()
        Dim info = CuAntete(11, 22, 33)
        Assert.Equal(22, info.AntetDeLucru(22).Iddf)
    End Sub

    <Fact>
    Public Sub AntetDeLucru_FallsBackToFirst_WhenNoPreferenceGiven()
        Dim info = CuAntete(11, 22)
        Assert.Equal(11, info.AntetDeLucru(0).Iddf)
    End Sub

    <Fact>
    Public Sub AntetDeLucru_FallsBackToFirst_WhenPreferenceDoesNotMatch()
        ' Un IDDF care nu există nu are voie să întoarcă Nothing: vederea trebuie să arate
        ' ceva, iar apelantul loghează separat că sunt mai multe antete.
        Dim info = CuAntete(11, 22)
        Assert.Equal(11, info.AntetDeLucru(999).Iddf)
    End Sub
End Class
