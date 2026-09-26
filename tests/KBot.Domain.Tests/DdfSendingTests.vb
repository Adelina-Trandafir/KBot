Option Strict On
Imports System.Collections.Generic
Imports Xunit
Imports KBot.Domain

' Slice 0081-02: the drafts of the two new entry points and the Rezervari footer menu.
Public Class DdfSendingTests

    ' ── DdfDraftFactory ──────────────────────────────────────────────────────

    <Fact>
    Public Sub NewManualCode_IsBangPlusTen()
        Dim cod = DdfDraftFactory.NewManualCode()
        Assert.StartsWith("!", cod)
        Assert.Equal(11, cod.Length)
    End Sub

    <Fact>
    Public Sub ForNewAngajament_IsAManualRev0WithEmptySectionA()
        Dim d = DdfDraftFactory.ForNewAngajament("!ABCDEFGHIJ", "000_DEMO", "0000000000", New Date(2026, 9, 25))
        Assert.True(d.Nou)
        Assert.True(d.RevizieNoua)
        Assert.True(d.Manual)
        Assert.False(d.Incarcat)
        Assert.False(d.Preluat)
        Assert.Equal("MANUAL", d.Stare)
        Assert.Equal("000_DEMO", d.Dc)
        Assert.Equal(0, d.Revizie.NumarRev)
        Assert.Equal(DdfDraftFactory.ManualRevisionType, d.Revizie.Tip)
        Assert.Empty(d.LiniiA)
        Assert.False(d.DinRezervari)
    End Sub

    <Fact>
    Public Sub ForNewAngajament_RefusesACodeWithoutBang()
        Assert.Throws(Of ArgumentException)(Sub() DdfDraftFactory.ForNewAngajament("AAB123", "db", "p", Date.Today))
    End Sub

    <Fact>
    Public Sub ForAddedReservation_KeepsTheHeader_StartsAnEmptyNextRevision()
        Dim ultima As New DdfDraft() With {.Iddf = 7, .Cual = 3, .CodAngajament = "AAB2EF2MCP4", .Comp = "Achizitii",
                                           .Dc = "000_DEMO", .ObiectDdf = "Obiect", .PartAng = True, .CodFiscal = "123"}
        ultima.Revizie.Idrev = 90
        ultima.Revizie.NumarRev = 4
        ultima.LiniiA.Add(New DdfDraftLinieA() With {.IdSecA = 5, .ValCur = 10})

        Dim d = DdfDraftFactory.ForAddedReservation(ultima, New Date(2026, 9, 25))
        Assert.False(d.Nou)
        Assert.True(d.RevizieNoua)
        Assert.Equal(7, d.Iddf)
        Assert.Equal(3, d.Cual)
        Assert.Equal("Achizitii", d.Comp)
        Assert.True(d.PartAng)
        Assert.Equal(0, d.Revizie.Idrev)
        Assert.Equal(5, d.Revizie.NumarRev)
        Assert.Empty(d.LiniiA)
        Assert.False(d.Revizie.Incarcat)
    End Sub

    <Fact>
    Public Sub ForFirstRevisionOfExisting_IsANewDocumentOnAForexeAngajament()
        Dim d = DdfDraftFactory.ForFirstRevisionOfExisting("AAB2EF2MCP4", "000_DEMO", "0000000000",
                                                           "Burse", New Date(2026, 1, 5), "În derulare", Date.Today)
        Assert.True(d.Nou)
        Assert.False(d.Manual)
        Assert.Equal("Burse", d.ObiectDdf)
        Assert.Equal(0, d.Revizie.NumarRev)
    End Sub

    ' ── RezervariMenu.ParseState ─────────────────────────────────────────────

    <Theory>
    <InlineData("În derulare", ForexeAngajamentState.InDerulare)>
    <InlineData("In derulare", ForexeAngajamentState.InDerulare)>
    <InlineData("În definitivare", ForexeAngajamentState.InDefinitivare)>
    <InlineData("Inițial", ForexeAngajamentState.Initial)>
    <InlineData("Anulat", ForexeAngajamentState.Closed)>
    <InlineData("Reziliat", ForexeAngajamentState.Closed)>
    <InlineData("MANUAL", ForexeAngajamentState.NotInForexe)>
    <InlineData("", ForexeAngajamentState.Unknown)>
    Public Sub ParseState_ReadsTheForexeLabel(stare As String, expected As ForexeAngajamentState)
        Assert.Equal(expected, RezervariMenu.ParseState(stare, "AAB2EF2MCP4"))
    End Sub

    <Fact>
    Public Sub ParseState_BangCode_IsNotInForexe()
        Assert.Equal(ForexeAngajamentState.NotInForexe, RezervariMenu.ParseState("În derulare", "!ABC"))
    End Sub

    ' ── RezervariMenu.Decide: the menu table of plan 0081-04 ─────────────────

    Private Shared Function Rev(numar As Integer, stage As DdfSendStage, semnatura As String,
                                Optional dejaInForexe As Boolean = False) As RevizieRow
        Return New RevizieRow() With {.NumarRev = numar, .StareTrimitere = stage, .Semnatura = semnatura,
                                      .Preluat = dejaInForexe}
    End Function

    <Theory>
    <InlineData(ForexeAngajamentState.Initial, RezervariMenuOption.Definitiveaza)>
    <InlineData(ForexeAngajamentState.InDefinitivare, RezervariMenuOption.Deruleaza)>
    <InlineData(ForexeAngajamentState.InDerulare, RezervariMenuOption.GenereazaPdfFinal)>
    Public Sub NewAngajament_Rev0SentInProgress_FollowsTheForexeState(state As ForexeAngajamentState,
                                                                      expected As RezervariMenuOption)
        Dim revizii = New List(Of RevizieRow) From {Rev(0, DdfSendStage.SentInProgress, "A")}
        Assert.Equal(expected, RezervariMenu.Decide(state, documentManual:=True, revizii:=revizii))
    End Sub

    <Fact>
    Public Sub Running_WithEveryRevisionClosed_OffersAddReservation()
        Dim revizii = New List(Of RevizieRow) From {
            Rev(0, DdfSendStage.FinalPdf, "A,B,Ordonator"), Rev(1, DdfSendStage.FinalPdf, "")}
        Assert.Equal(RezervariMenuOption.AdaugaRezervare,
                     RezervariMenu.Decide(ForexeAngajamentState.InDerulare, True, revizii))
    End Sub

    <Fact>
    Public Sub Running_WithOldForexeRevisions_OffersAddReservation()
        Dim revizii = New List(Of RevizieRow) From {Rev(0, DdfSendStage.NotSent, "", dejaInForexe:=True)}
        Assert.Equal(RezervariMenuOption.AdaugaRezervare,
                     RezervariMenu.Decide(ForexeAngajamentState.InDerulare, False, revizii))
    End Sub

    <Fact>
    Public Sub Running_WithNoDocument_OffersAddReservation()
        Assert.Equal(RezervariMenuOption.AdaugaRezervare,
                     RezervariMenu.Decide(ForexeAngajamentState.InDerulare, False, Nothing))
    End Sub

    <Theory>
    <InlineData(DdfSendStage.NotSent, "")>
    <InlineData(DdfSendStage.NotSent, "A")>
    <InlineData(DdfSendStage.Interrupted, "A")>
    Public Sub AnOpenLaterRevision_OffersNothing(stage As DdfSendStage, semnatura As String)
        Dim revizii = New List(Of RevizieRow) From {
            Rev(0, DdfSendStage.FinalPdf, "A,B"), Rev(1, stage, semnatura)}
        Assert.Equal(RezervariMenuOption.None, RezervariMenu.Decide(ForexeAngajamentState.InDerulare, True, revizii))
    End Sub

    <Fact>
    Public Sub NotManual_Rev0SentInProgress_GetsNoDefinitivare()
        ' K-BOT does not finish angajamente it did not create.
        Dim revizii = New List(Of RevizieRow) From {Rev(0, DdfSendStage.SentInProgress, "A")}
        Assert.Equal(RezervariMenuOption.None, RezervariMenu.Decide(ForexeAngajamentState.Initial, False, revizii))
    End Sub

    <Fact>
    Public Sub StuckLaterRevision_OnARunningAngajament_OffersTheFinalPdf()
        Dim revizii = New List(Of RevizieRow) From {
            Rev(0, DdfSendStage.FinalPdf, "A,B,Ordonator"), Rev(1, DdfSendStage.SentInProgress, "A")}
        Assert.Equal(RezervariMenuOption.GenereazaPdfFinal,
                     RezervariMenu.Decide(ForexeAngajamentState.InDerulare, True, revizii))
    End Sub

    <Theory>
    <InlineData(ForexeAngajamentState.Closed)>
    <InlineData(ForexeAngajamentState.NotInForexe)>
    <InlineData(ForexeAngajamentState.Unknown)>
    Public Sub ClosedOrUnknown_OffersNothing(state As ForexeAngajamentState)
        Assert.Equal(RezervariMenuOption.None, RezervariMenu.Decide(state, True, Nothing))
    End Sub

    <Fact>
    Public Sub InitialWithoutAnOpenRev0_OffersNothing()
        Dim revizii = New List(Of RevizieRow) From {Rev(0, DdfSendStage.FinalPdf, "A,B")}
        Assert.Equal(RezervariMenuOption.None, RezervariMenu.Decide(ForexeAngajamentState.Initial, True, revizii))
    End Sub

End Class
