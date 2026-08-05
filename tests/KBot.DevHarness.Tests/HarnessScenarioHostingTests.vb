Option Strict On
Imports Xunit
Imports KBot.DevHarness

''' <summary>
''' The `hosting` section added in slice 0024-03: how the window is caught and how it is let go.
'''
''' §9 test 9 has two halves that must both hold — a new field must round-trip, AND a scenario file
''' written before the field existed must still load. The bench's scenario files are written by hand
''' and passed between machines; a reader that rejects an older file would strand every state saved
''' in slice 0023.
''' </summary>
Public Class HarnessScenarioHostingTests

    <Fact>
    Public Sub HostingSection_RoundTripsThroughTheReader()
        Dim json As String = "
{
  ""schema"": 1,
  ""name"": ""A vs B"",
  ""hosting"": {
    ""detachMode"": ""close"",
    ""useCreationHook"": true,
    ""captureDelayMs"": 250,
    ""closeGraceMs"": 800
  }
}"
        Dim result = HarnessScenarioReader.Read(json)

        Assert.NotNull(result.Scenario)
        Dim h = result.Scenario.Hosting
        Assert.NotNull(h)
        Assert.True(h.WantsCloseWindow())
        Assert.True(h.UseCreationHook.Value)
        Assert.Equal(250, h.CaptureDelayMs.Value)
        Assert.Equal(800, h.CloseGraceMs.Value)
        Assert.Null(h.DetachModeWarning())
    End Sub

    <Fact>
    Public Sub AnOlderFileWithoutTheHostingSection_StillLoads()
        ' The exact shape slice 0023 saved: no `hosting` key at all.
        Dim json As String = "
{
  ""schema"": 1,
  ""name"": ""stare veche"",
  ""launch"": { ""newInstance"": true, ""noSplash"": true },
  ""clip"": { ""enabled"": false }
}"
        Dim result = HarnessScenarioReader.Read(json)

        Assert.NotNull(result.Scenario)
        ' Absent means «leave it alone», never «reset to a default».
        Assert.Null(result.Scenario.Hosting)
        Assert.True(result.Scenario.Launch.NewInstance.Value)
    End Sub

    <Fact>
    Public Sub AnUnrecognisedDetachMode_FallsBackToKill_AndSaysSo()
        ' A typo in a file sent from outside must be loud, not silently rounded.
        Dim json As String = "{ ""schema"": 1, ""hosting"": { ""detachMode"": ""terminate"" } }"
        Dim result = HarnessScenarioReader.Read(json)

        Dim h = result.Scenario.Hosting
        Assert.False(h.WantsCloseWindow())          ' mode A, the safe default
        Dim warning As String = h.DetachModeWarning()
        Assert.NotNull(warning)
        Assert.Contains("terminate", warning)
        Assert.Contains("kill", warning)
    End Sub

    <Fact>
    Public Sub MissingHostingScalars_FallBackToTheDocumentedDefaults()
        Dim json As String = "{ ""schema"": 1, ""hosting"": { ""detachMode"": ""kill"" } }"
        Dim result = HarnessScenarioReader.Read(json)

        Dim h = result.Scenario.Hosting
        Assert.Equal(0, h.EffectiveCaptureDelayMs())
        Assert.Equal(HostingConfig.DefaultCloseGraceMs, h.EffectiveCloseGraceMs())
        Assert.False(h.UseCreationHook.HasValue)    ' absent stays absent
    End Sub

    <Fact>
    Public Sub DetachModeIsText_SoASavedFileSaysWhichModeRan()
        ' Deliberately not an integer: a 0 in a file tells nobody which mode was used.
        Assert.Equal("kill", HostingConfig.DetachKill)
        Assert.Equal("close", HostingConfig.DetachClose)

        Dim kill As New HostingConfig() With {.DetachMode = HostingConfig.DetachKill}
        Dim close As New HostingConfig() With {.DetachMode = HostingConfig.DetachClose}
        Assert.False(kill.WantsCloseWindow())
        Assert.True(close.WantsCloseWindow())
    End Sub

End Class
