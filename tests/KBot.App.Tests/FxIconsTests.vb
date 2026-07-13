Option Strict On
Imports Xunit
Imports KBot.App

' Tests the pure Stare -> icon-name mapping (FxIcons.StatusIconName), mirroring
' mdl_FX_PopulareTree.Angajament_Iconita_Dreapta. Case- AND diacritic-insensitive.
Public Class FxIconsTests

    <Theory>
    <InlineData("În derulare", "FX_GREEN")>
    <InlineData("derulare", "FX_GREEN")>
    <InlineData("Anulat", "FX_RED")>
    <InlineData("ANULAT", "FX_RED")>
    <InlineData("Reziliat", "FX_RED")>
    <InlineData("Suspendat", "FX_ORANGE")>
    <InlineData("Inițial", "FX_GRAY")>
    <InlineData("Arhivat", "FX_BLUE")>
    <InlineData("Manual", "FX_WHITE")>
    <InlineData("În definitivare", "FX_BLUE")>
    Public Sub StatusIconName_MapsKnownStates(stare As String, expected As String)
        Assert.Equal(expected, FxIcons.StatusIconName(stare))
    End Sub

    <Theory>
    <InlineData("")>
    <InlineData("   ")>
    <InlineData(Nothing)>
    <InlineData("stare necunoscuta")>
    Public Sub StatusIconName_FallsBackToGray(stare As String)
        Assert.Equal("FX_GRAY", FxIcons.StatusIconName(stare))
    End Sub

    <Fact>
    Public Sub StatusIconName_DefinitivareIsBlue_NotGray()
        ' "definitivare" must not be swallowed by the "initial" rule that precedes it.
        Assert.Equal("FX_BLUE", FxIcons.StatusIconName("În definitivare"))
    End Sub

End Class
