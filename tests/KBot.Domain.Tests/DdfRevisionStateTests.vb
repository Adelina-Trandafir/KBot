Option Strict On
Imports Xunit
Imports KBot.Domain

' Slice 0081-01: the revision state of the sending flow, derived from the send stage
' (FX_DDF_REV.StareTrimitere), the signer roles (Semnatura) and "already in forexecab".
Public Class DdfRevisionStateTests

    <Theory>
    <InlineData("", DdfRevisionState.Draft)>
    <InlineData(Nothing, DdfRevisionState.Draft)>
    <InlineData("A", DdfRevisionState.SignedA)>
    <InlineData("A,B", DdfRevisionState.SignedA)>
    <InlineData("B", DdfRevisionState.Draft)>
    Public Sub NotSent_NotInForexe_ReadsTheInterimSignature(semnatura As String, expected As DdfRevisionState)
        Assert.Equal(expected, DdfRevisionStates.Derive(DdfSendStage.NotSent, False, semnatura))
    End Sub

    <Theory>
    <InlineData("", DdfRevisionState.FinalToSign)>
    <InlineData("A", DdfRevisionState.FinalToSign)>
    <InlineData("A,B", DdfRevisionState.SignedAB)>
    <InlineData("A,B,Ordonator", DdfRevisionState.Approved)>
    Public Sub NotSent_AlreadyInForexe_IsAFinalDocument(semnatura As String, expected As DdfRevisionState)
        ' A revision from before slice 0081 that forexecab already has: its PDF has section B.
        Assert.Equal(expected, DdfRevisionStates.Derive(DdfSendStage.NotSent, True, semnatura))
    End Sub

    <Fact>
    Public Sub Interrupted_IsSendInterrupted_WhateverTheSignature()
        Assert.Equal(DdfRevisionState.SendInterrupted, DdfRevisionStates.Derive(DdfSendStage.Interrupted, False, "A"))
        Assert.Equal(DdfRevisionState.SendInterrupted, DdfRevisionStates.Derive(DdfSendStage.Interrupted, True, ""))
    End Sub

    <Fact>
    Public Sub SentInProgress_KeepsTheInterimA_AndStaysInProgress()
        ' The import sets Incarcat = 1 by itself; the stage is what keeps S2a apart from S2b.
        Assert.Equal(DdfRevisionState.SentInProgress, DdfRevisionStates.Derive(DdfSendStage.SentInProgress, True, "A"))
    End Sub

    <Theory>
    <InlineData("", DdfRevisionState.FinalToSign)>
    <InlineData("A", DdfRevisionState.FinalToSign)>
    <InlineData("B", DdfRevisionState.FinalToSign)>
    <InlineData("A,B", DdfRevisionState.SignedAB)>
    <InlineData("A,B,Ordonator", DdfRevisionState.Approved)>
    Public Sub FinalPdf_IsDecidedBySignatures(semnatura As String, expected As DdfRevisionState)
        ' «A only on the final PDF» is S2b, never S2a -- the case the new column exists for.
        Assert.Equal(expected, DdfRevisionStates.Derive(DdfSendStage.FinalPdf, True, semnatura))
    End Sub

    <Fact>
    Public Sub UnknownStage_Throws()
        Assert.Throws(Of ArgumentOutOfRangeException)(Sub() DdfRevisionStates.Derive(9, False, ""))
    End Sub

    <Fact>
    Public Sub HasRole_IsExactPerItem()
        Assert.True(DdfRevisionStates.HasRole("A,B,Ordonator", "Ordonator"))
        Assert.True(DdfRevisionStates.HasRole(" A , B ", "B"))
        Assert.False(DdfRevisionStates.HasRole("AB,CD", "A"))
        Assert.False(DdfRevisionStates.HasRole("", "A"))
    End Sub

    <Fact>
    Public Sub OpenEditSend_FollowThePlan()
        Assert.True(DdfRevisionStates.IsOpen(DdfRevisionState.Draft))
        Assert.True(DdfRevisionStates.IsOpen(DdfRevisionState.SentInProgress))
        Assert.False(DdfRevisionStates.IsOpen(DdfRevisionState.FinalToSign))

        Assert.True(DdfRevisionStates.CanEdit(DdfRevisionState.SignedA))
        Assert.False(DdfRevisionStates.CanEdit(DdfRevisionState.SendInterrupted))

        Assert.True(DdfRevisionStates.CanSend(DdfRevisionState.SignedA))
        Assert.True(DdfRevisionStates.CanSend(DdfRevisionState.SendInterrupted))
        Assert.False(DdfRevisionStates.CanSend(DdfRevisionState.Draft))

        Assert.False(DdfRevisionStates.IsSent(DdfRevisionState.SignedA))
        Assert.True(DdfRevisionStates.IsSent(DdfRevisionState.SentInProgress))
    End Sub

    <Fact>
    Public Sub EveryState_HasAnOperatorLabel()
        For Each s As DdfRevisionState In [Enum].GetValues(GetType(DdfRevisionState))
            Assert.False(String.IsNullOrWhiteSpace(DdfRevisionStates.Label(s)))
        Next
    End Sub

    <Fact>
    Public Sub RevizieRow_DerivesItsState()
        Dim r As New RevizieRow() With {.StareTrimitere = 0, .Preluat = True, .Semnatura = "A,B"}
        Assert.Equal(DdfRevisionState.SignedAB, r.Stare)
        r = New RevizieRow() With {.StareTrimitere = 0, .Semnatura = "A"}
        Assert.Equal(DdfRevisionState.SignedA, r.Stare)
        r = New RevizieRow() With {.AreRezervari = True}
        Assert.Equal(DdfRevisionState.FinalToSign, r.Stare)
    End Sub

End Class
