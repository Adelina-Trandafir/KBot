Option Strict On
Imports System.Collections.Generic
Imports KBot.Domain
Imports Xunit

' Slice 0081-09 -- the SS and header rules behind adding a line to section A. The SSs come from
' AVACONT_COMUN.DefaProgram: 0000000000 -> 02A / 02E, 0000002510 -> 01A.
Public Class DdfSectiuneaAReguliTests

    Private Shared Function DefaProgram() As List(Of DdfSursaProgram)
        Return New List(Of DdfSursaProgram) From {
            New DdfSursaProgram() With {.Program = "0000000000", .Ss = "02A"},
            New DdfSursaProgram() With {.Program = "0000000000", .Ss = "02E"},
            New DdfSursaProgram() With {.Program = "0000002510", .Ss = "01A"}}
    End Function

    Private Shared Function Coduri(surse As List(Of DdfSursaProgram)) As List(Of String)
        Dim r As New List(Of String)()
        For Each s As DdfSursaProgram In surse
            r.Add(s.Ss)
        Next
        Return r
    End Function

    <Fact>
    Public Sub SurseAleProgramului_KeepsOnlyTheProgramsSs()
        Assert.Equal(New List(Of String) From {"02A", "02E"},
                     Coduri(DdfSectiuneaAReguli.SurseAleProgramului(DefaProgram(), "0000000000")))
        Assert.Equal(New List(Of String) From {"01A"},
                     Coduri(DdfSectiuneaAReguli.SurseAleProgramului(DefaProgram(), "0000002510")))
    End Sub

    <Fact>
    Public Sub SurseAleProgramului_MatchesAProgramWrittenWithoutLeadingZeros()
        Assert.Equal(New List(Of String) From {"01A"},
                     Coduri(DdfSectiuneaAReguli.SurseAleProgramului(DefaProgram(), "2510")))
    End Sub

    <Fact>
    Public Sub SurseAleProgramului_WithNoProgram_IsEmpty()
        Assert.Empty(DdfSectiuneaAReguli.SurseAleProgramului(DefaProgram(), ""))
        Assert.Empty(DdfSectiuneaAReguli.SurseAleProgramului(Nothing, "0000000000"))
    End Sub

    <Fact>
    Public Sub AcelasiProgram_DoesNotMatchTwoDifferentNumbers()
        Assert.False(DdfSectiuneaAReguli.AcelasiProgram("0000000000", "0000002510"))
        Assert.True(DdfSectiuneaAReguli.AcelasiProgram("0000000000", "0"))
    End Sub

    <Fact>
    Public Sub ClasificatiileSursei_FiltersBySsAndDropsTheSeparator()
        Dim lista As New List(Of DdfClasificatie) From {
            New DdfClasificatie() With {.IdClsf = 1, .Ss = "02A"},
            New DdfClasificatie() With {.IdClsf = -1, .Ss = ""},
            New DdfClasificatie() With {.IdClsf = 2, .Ss = "02e"}}
        Dim rez As List(Of DdfClasificatie) = DdfSectiuneaAReguli.ClasificatiileSursei(lista, "02E")
        Assert.Single(rez)
        Assert.Equal(2, rez(0).IdClsf)
    End Sub

    <Fact>
    Public Sub ClasificatiileProgramului_KeepsEverySsOfTheProgram()
        Dim lista As New List(Of DdfClasificatie) From {
            New DdfClasificatie() With {.IdClsf = 1, .Ss = "02A"},
            New DdfClasificatie() With {.IdClsf = 2, .Ss = "02E"},
            New DdfClasificatie() With {.IdClsf = 3, .Ss = "01A"},
            New DdfClasificatie() With {.IdClsf = -1}}
        Dim surse As List(Of DdfSursaProgram) = DdfSectiuneaAReguli.SurseAleProgramului(DefaProgram(), "0000000000")
        Dim rez As List(Of DdfClasificatie) = DdfSectiuneaAReguli.ClasificatiileProgramului(lista, surse)
        Assert.Equal(2, rez.Count)
        Assert.DoesNotContain(rez, Function(c) c.IdClsf = 3)
    End Sub

    <Fact>
    Public Sub LipsuriAntet_NamesEveryMissingField()
        Assert.Equal(New List(Of String) From {"programul", "compartimentul", "obiectul", "partenerul"},
                     DdfSectiuneaAReguli.LipsuriAntet("", " ", Nothing, True, ""))
    End Sub

    <Fact>
    Public Sub LipsuriAntet_WithoutPartner_DoesNotAskForOne()
        Assert.Empty(DdfSectiuneaAReguli.LipsuriAntet("0000000000", "Achizitii", "Hartie", False, ""))
    End Sub

    <Fact>
    Public Sub LiniiCuAltaSursa_FindsTheLinesOutsideTheProgram()
        Dim linii As New List(Of DdfDraftLinieA) From {
            New DdfDraftLinieA() With {.Ss = "02A"},
            New DdfDraftLinieA() With {.Ss = "01A"}}
        Assert.Equal(New List(Of Integer) From {2},
                     DdfSectiuneaAReguli.LiniiCuAltaSursa(linii, DefaProgram(), "0000000000"))
    End Sub

    <Fact>
    Public Sub LiniiCuAltaSursa_WithAnUnknownMap_ChecksNothing()
        Dim linii As New List(Of DdfDraftLinieA) From {New DdfDraftLinieA() With {.Ss = "99Z"}}
        Assert.Empty(DdfSectiuneaAReguli.LiniiCuAltaSursa(linii, New List(Of DdfSursaProgram)(), "0000000000"))
    End Sub

    <Fact>
    Public Sub OnlyANewAngajamentIsMarkedAsOne()
        Dim nou As DdfDraft = DdfDraftFactory.ForNewAngajament("!ABCDEFGHIJ", "db", "0000000000", Date.Today)
        Dim existent As DdfDraft = DdfDraftFactory.ForFirstRevisionOfExisting(
            "ABC123", "db", "0000000000", "Obiect", Date.Today, "Initial", Date.Today)
        Assert.True(nou.AngajamentNou)
        Assert.False(existent.AngajamentNou)
        Assert.False(DdfDraftFactory.ForAddedReservation(nou, Date.Today).AngajamentNou)
    End Sub
End Class
