Option Strict On
Imports KBot.DevHarness
Imports Microsoft.Win32
Imports Xunit

' Slice 0023 pass 5: one HKCU preference row must be able to say four different things —
' "leave it alone", "write this number", "write this text", "remove it". The checkbox it replaced
' could say two, which is why «bEnableAv2 = 0» ticked on a machine holding 0 logged `0 -> 0` and
' there was no way at all to ask for 1 from the panel.
Public Class PrefRowSelectionTests

    <Fact>
    Public Sub Untouched_ProducesNoIntentAtAll()
        Dim p = PrefRowSelection.ParseDword("bEnableAv2", PrefRowSelection.Untouched)
        Assert.True(p.IsUntouched)
        Assert.Null(p.Intent)
        Assert.False(p.Invalid)
    End Sub

    <Fact>
    Public Sub BlankIsUntouched_NotZero()
        ' The distinction the old panel could not make: "nu atinge" is NOT "scrie 0".
        Assert.True(PrefRowSelection.ParseDword("bEnableAv2", "").IsUntouched)
        Assert.True(PrefRowSelection.ParseDword("bEnableAv2", "   ").IsUntouched)
        Assert.True(PrefRowSelection.ParseDword("bEnableAv2", Nothing).IsUntouched)
    End Sub

    <Fact>
    Public Sub Zero_IsAWriteOfZero()
        Dim p = PrefRowSelection.ParseDword("bEnableAv2", "0")
        Assert.False(p.IsUntouched)
        Assert.Equal(UserPrefAction.WriteDword, p.Intent.Action)
        Assert.Equal(0, CInt(p.Intent.Value))
        Assert.Equal(RegistryValueKind.DWord, p.Intent.Kind)
    End Sub

    <Fact>
    Public Sub One_IsAWriteOfOne()
        Dim p = PrefRowSelection.ParseDword("bEnableAv2", "1")
        Assert.Equal(1, CInt(p.Intent.Value))
        Assert.Equal("1", p.Intent.RequestedText())
    End Sub

    <Fact>
    Public Sub AnyOtherInteger_IsCarriedLiterally()
        ' The combo is editable on purpose: the panel is not limited to the values it lists.
        Assert.Equal(7, CInt(PrefRowSelection.ParseDword("bOraCeva", "7").Intent.Value))
        Assert.Equal(-3, CInt(PrefRowSelection.ParseDword("bOraCeva", "-3").Intent.Value))
    End Sub

    <Fact>
    Public Sub SurroundingSpaceIsTolerated()
        Assert.Equal(1, CInt(PrefRowSelection.ParseDword("bEnableAv2", "  1 ").Intent.Value))
    End Sub

    <Fact>
    Public Sub Delete_IsADeletion()
        Dim p = PrefRowSelection.ParseDword("bRHPSticky", PrefRowSelection.DeleteText)
        Assert.Equal(UserPrefAction.Delete, p.Intent.Action)
        Assert.Equal("(șters)", p.Intent.RequestedText())
    End Sub

    <Fact>
    Public Sub NonNumericDword_IsRejectedNotGuessed()
        Dim p = PrefRowSelection.ParseDword("bEnableAv2", "da")
        Assert.True(p.Invalid)
        Assert.False(p.IsUntouched)   ' invalid must NOT be silently treated as "leave alone"
        Assert.Null(p.Intent)
        Assert.Contains("bEnableAv2", p.Message)
    End Sub

    <Fact>
    Public Sub StringRow_WritesTheTextLiterally()
        Dim p = PrefRowSelection.ParseString("aDefaultRHPViewMode_L", "Collapsed")
        Assert.Equal(UserPrefAction.WriteString, p.Intent.Action)
        Assert.Equal("Collapsed", CStr(p.Intent.Value))
        Assert.Equal(RegistryValueKind.String, p.Intent.Kind)
    End Sub

    <Fact>
    Public Sub StringRow_AcceptsAValueThePanelNeverListed()
        Assert.Equal("Docked", CStr(PrefRowSelection.ParseString("aDefaultRHPViewMode_L", "Docked").Intent.Value))
    End Sub

    <Fact>
    Public Sub StringRow_HonoursBothSentinels()
        Assert.True(PrefRowSelection.ParseString("aDefaultRHPViewMode_L", PrefRowSelection.Untouched).IsUntouched)
        Assert.Equal(UserPrefAction.Delete,
                     PrefRowSelection.ParseString("aDefaultRHPViewMode_L", PrefRowSelection.DeleteText).Intent.Action)
    End Sub

    <Fact>
    Public Sub SentinelsAreMatchedCaseInsensitively()
        Assert.True(PrefRowSelection.ParseDword("x", "NU ATINGE").IsUntouched)
        Assert.Equal(UserPrefAction.Delete, PrefRowSelection.ParseDword("x", "ȘTERGE").Intent.Action)
    End Sub

    <Fact>
    Public Sub TextFor_RoundTripsAnIntentBackIntoTheRow()
        ' Scenario -> panel: the row must state what will be written, not the nearest thing it lists.
        Assert.Equal("1", PrefRowSelection.TextFor(New UserPrefIntent("bEnableAv2", UserPrefAction.WriteDword, 1)))
        Assert.Equal("0", PrefRowSelection.TextFor(New UserPrefIntent("bEnableAv2", UserPrefAction.WriteDword, 0)))
        Assert.Equal("Collapsed", PrefRowSelection.TextFor(
            New UserPrefIntent("aDefaultRHPViewMode_L", UserPrefAction.WriteString, "Collapsed")))
        Assert.Equal(PrefRowSelection.DeleteText, PrefRowSelection.TextFor(
            New UserPrefIntent("bRHPSticky", UserPrefAction.Delete, Nothing)))
    End Sub

    <Fact>
    Public Sub TextFor_NothingIsUntouched()
        ' A scenario silent about a value leaves its row alone rather than zeroing it.
        Assert.Equal(PrefRowSelection.Untouched, PrefRowSelection.TextFor(Nothing))
    End Sub

    <Fact>
    Public Sub EveryTextFor_ParsesBackToTheSameIntent()
        Dim original As New UserPrefIntent("bEnableAv2", UserPrefAction.WriteDword, 1)
        Dim back = PrefRowSelection.ParseDword("bEnableAv2", PrefRowSelection.TextFor(original))
        Assert.Equal(original.Action, back.Intent.Action)
        Assert.Equal(CInt(original.Value), CInt(back.Intent.Value))
    End Sub

End Class
