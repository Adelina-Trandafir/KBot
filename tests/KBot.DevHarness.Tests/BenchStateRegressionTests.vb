Option Strict On
Imports System.Drawing
Imports System.IO
Imports KBot.Controls
Imports KBot.DevHarness
Imports Xunit

' Slice 0024 — the harness regression the brief asks for (§8, last bullet).
'
' The two files under Fixtures\ are BYTE-VERBATIM copies of the states the operator saved from the
' bench on 04.08.2026 (20:06 modern, 20:10 classic) against Acrobat 26.1.21771.0. They are the
' evidence behind AdobeViewerProfiles.
'
' Two things are pinned here, and they are different claims:
'   1. THE REFACTOR DID NOT MOVE ANYTHING. Feeding a saved state through AdobeHostGeometry.Compute
'      — the function the bench now calls instead of its own HostedBounds — produces the same
'      rectangle the bench produced before the extraction.
'   2. THE PROFILES ARE FAITHFUL TRANSCRIPTIONS. The shipped profile and the saved file place the
'      window identically; if someone edits a profile number, this fails and names the file that
'      disagrees with it.
Public Class BenchStateRegressionTests

    ' A fixed host size, so the expected rectangles below are literal numbers rather than a second
    ' implementation of the same arithmetic.
    Private Const HostW As Integer = 1000
    Private Const HostH As Integer = 800

    Private Shared Function ReadFixture(name As String) As HarnessScenario
        Dim p As String = Path.Combine(AppContext.BaseDirectory, "Fixtures", name)
        Assert.True(File.Exists(p), $"Lipsește fixture-ul {name} (copiat din starea salvată a bancului).")
        Dim r = HarnessScenarioReader.Read(File.ReadAllText(p))
        Assert.True(r.IsValid, String.Join(" / ", r.Errors))
        Return r.Scenario
    End Function

    ' The geometry a saved state asks for, computed exactly the way the bench computes it.
    Private Shared Function GeometryOf(s As HarnessScenario) As Rectangle
        Dim clipOn As Boolean = s.Clip IsNot Nothing AndAlso s.Clip.Enabled.GetValueOrDefault()
        Dim right As Integer = If(s.Clip Is Nothing, 0, s.Clip.Right.GetValueOrDefault())
        Dim top As Integer = If(s.Clip Is Nothing, 0, s.Clip.Top.GetValueOrDefault())
        Dim dx As Integer = If(s.Move Is Nothing, 0, s.Move.EffectiveDx())
        Dim dy As Integer = If(s.Move Is Nothing, 0, s.Move.EffectiveDy())
        Dim dw As Integer = If(s.Move Is Nothing, 0, s.Move.EffectiveDw())
        Dim dh As Integer = If(s.Move Is Nothing, 0, s.Move.EffectiveDh())
        Return AdobeHostGeometry.Compute(HostW, HostH, clipOn, right, top, dx, dy, dw, dh)
    End Function

    ' ══ the modern state (04.08.2026 20:06) ════════════════════════════════════
    <Fact>
    Public Sub ModernBenchState_StillLoadsAndCarriesTheMeasuredNumbers()
        Dim s = ReadFixture("bench_state_modern_20260804_2006.json")
        Assert.False(s.Launch.NewInstance.Value)
        Assert.False(s.Launch.NoSplash.Value)
        ' No open parameters at all on this UI.
        Assert.Null(s.OpenParameters.Toolbar)
        Assert.Null(s.OpenParameters.Navpanes)
        Assert.True(s.Clip.Enabled.Value)
        Assert.Equal(230, s.Clip.Right.Value)
        Assert.Equal(152, s.Clip.Top.Value)
        Assert.Equal(-130, s.Move.EffectiveDx())
        Assert.Equal(0, s.Move.EffectiveDy())
        Assert.Equal(0, s.Move.EffectiveDw())
        Assert.Equal(0, s.Move.EffectiveDh())
    End Sub

    <Fact>
    Public Sub ModernBenchState_ProducesTheSameRectangleAsBeforeTheRefactor()
        ' (0,-152) 1230x952 from the clip, then dx = -130.
        Assert.Equal(New Rectangle(-130, -152, 1230, 952),
                     GeometryOf(ReadFixture("bench_state_modern_20260804_2006.json")))
    End Sub

    <Fact>
    Public Sub ModernProfile_PlacesTheWindowExactlyWhereTheSavedStateDoes()
        Assert.Equal(GeometryOf(ReadFixture("bench_state_modern_20260804_2006.json")),
                     AdobeHostGeometry.Compute(New Size(HostW, HostH), AdobeViewerProfiles.Modern))
    End Sub

    <Fact>
    Public Sub ModernProfile_LaunchesTheSameWayTheSavedStateDoes()
        Dim s = ReadFixture("bench_state_modern_20260804_2006.json")
        Assert.Equal(s.Launch.NewInstance.Value, AdobeViewerProfiles.Modern.NewInstance)
        Assert.Equal(s.Launch.NoSplash.Value, AdobeViewerProfiles.Modern.NoSplash)
        Assert.Equal("", AdobeViewerProfiles.Modern.OpenParametersText())
    End Sub

    ' ══ the classic state (04.08.2026 20:10) ═══════════════════════════════════
    <Fact>
    Public Sub ClassicBenchState_StillLoadsAndCarriesTheMeasuredNumbers()
        Dim s = ReadFixture("bench_state_classic_20260804_2010.json")
        Assert.True(s.Launch.NewInstance.Value)
        Assert.True(s.Launch.NoSplash.Value)
        Assert.Equal(0, s.OpenParameters.Toolbar.Value)
        Assert.Equal(0, s.OpenParameters.Navpanes.Value)
        Assert.False(s.Clip.Enabled.Value)
        ' An ABSENT move section is not the same as a present all-zero one.
        Assert.Null(s.Move)
    End Sub

    <Fact>
    Public Sub ClassicBenchState_ProducesTheSameRectangleAsBeforeTheRefactor()
        Assert.Equal(New Rectangle(0, 0, HostW, HostH),
                     GeometryOf(ReadFixture("bench_state_classic_20260804_2010.json")))
    End Sub

    <Fact>
    Public Sub ClassicProfile_PlacesTheWindowExactlyWhereTheSavedStateDoes()
        Assert.Equal(GeometryOf(ReadFixture("bench_state_classic_20260804_2010.json")),
                     AdobeHostGeometry.Compute(New Size(HostW, HostH), AdobeViewerProfiles.Classic))
    End Sub

    <Fact>
    Public Sub ClassicProfile_LaunchesTheSameWayTheSavedStateDoes()
        Dim s = ReadFixture("bench_state_classic_20260804_2010.json")
        Assert.Equal(s.Launch.NewInstance.Value, AdobeViewerProfiles.Classic.NewInstance)
        Assert.Equal(s.Launch.NoSplash.Value, AdobeViewerProfiles.Classic.NoSplash)
        Assert.Equal("toolbar=0&navpanes=0", AdobeViewerProfiles.Classic.OpenParametersText())
    End Sub

    ' ══ what the shipping code deliberately does NOT take from these files ═════
    <Fact>
    Public Sub BothBenchStates_SetBEnableAv2_WhichTheShippingCodeIgnores()
        ' The bench wrote this HKCU value to FORCE each UI generation; that is what a bench is for.
        ' ReaderHostPreview must never write it — it changes the operator's Adobe for every PDF they
        ' open, everywhere. The profiles carry the GEOMETRY these states measured, nothing else, and
        ' the shipping code decides which one to use by looking at the window tree instead.
        Dim modern = ReadFixture("bench_state_modern_20260804_2006.json")
        Dim classic = ReadFixture("bench_state_classic_20260804_2010.json")
        Assert.Equal(1, modern.UserPrefs.Values("bEnableAv2").GetInt32())
        Assert.Equal(0, classic.UserPrefs.Values("bEnableAv2").GetInt32())
    End Sub

End Class
