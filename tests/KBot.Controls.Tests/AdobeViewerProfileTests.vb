Option Strict On
Imports System.Drawing
Imports KBot.Controls
Imports Xunit

' Slice 0024 — the two Adobe host profiles.
'
' THESE NUMBERS ARE MEASUREMENTS. They were read off two bench states the operator saved on
' 04.08.2026 (20:06 modern, 20:10 classic) against Acrobat 26.1.21771.0. The point of this file is
' that an accidental edit to AdobeViewerProfiles FAILS A TEST instead of quietly breaking the DDF
' preview on the operator's machine — nobody would notice a «230» turning into a «203» by reading.
Public Class AdobeViewerProfileTests

    ' ══ the measured values ════════════════════════════════════════════════════
    <Fact>
    Public Sub ModernProfile_CarriesExactlyTheMeasuredValues()
        Dim p = AdobeViewerProfiles.Modern
        ' launch: newInstance = false, noSplash = false
        Assert.False(p.NewInstance)
        Assert.False(p.NoSplash)
        ' openParameters: NONE. The /A switches have no effect on this UI, and the classic ones are
        ' deliberately NOT "helpfully" added.
        Assert.Empty(p.OpenParameters)
        Assert.Equal("", p.OpenParametersText())
        ' clip: enabled, right = 230, top = 152
        Assert.True(p.ClipEnabled)
        Assert.Equal(230, p.ClipRight)
        Assert.Equal(152, p.ClipTop)
        ' move: dx = -130, dy = 0, dw = 0, dh = 0
        Assert.Equal(-130, p.Dx)
        Assert.Equal(0, p.Dy)
        Assert.Equal(0, p.Dw)
        Assert.Equal(0, p.Dh)
        ' the popup badge is hidden by the watcher on BOTH profiles
        Assert.True(p.HidePopups)
    End Sub

    <Fact>
    Public Sub ClassicProfile_CarriesExactlyTheMeasuredValues()
        Dim p = AdobeViewerProfiles.Classic
        ' launch: newInstance = true, noSplash = true
        Assert.True(p.NewInstance)
        Assert.True(p.NoSplash)
        ' openParameters: toolbar = 0, navpanes = 0 — in that order
        Assert.Equal(New String() {"toolbar=0", "navpanes=0"}, p.OpenParameters)
        Assert.Equal("toolbar=0&navpanes=0", p.OpenParametersText())
        ' clip: disabled — and the numbers stay zero, so a later "enable clip" cannot inherit a
        ' stale right/top from somewhere else
        Assert.False(p.ClipEnabled)
        Assert.Equal(0, p.ClipRight)
        Assert.Equal(0, p.ClipTop)
        ' move: none
        Assert.Equal(0, p.Dx)
        Assert.Equal(0, p.Dy)
        Assert.Equal(0, p.Dw)
        Assert.Equal(0, p.Dh)
        Assert.True(p.HidePopups)
    End Sub

    ' ══ the command line each profile produces ═════════════════════════════════
    <Fact>
    Public Sub ModernProfile_LaunchesWithoutSwitchesAndWithoutOpenParameters()
        Assert.Equal("""C:\x\d.pdf""",
                     AdobeWindowHosting.BuildArguments(AdobeViewerProfiles.Modern, "C:\x\d.pdf"))
    End Sub

    <Fact>
    Public Sub ClassicProfile_LaunchesWithNewInstanceNoSplashAndItsOpenParameters()
        ' /A must come BEFORE the file name — Adobe ignores the parameters otherwise.
        Assert.Equal("/n /s /A ""toolbar=0&navpanes=0"" ""C:\x\d.pdf""",
                     AdobeWindowHosting.BuildArguments(AdobeViewerProfiles.Classic, "C:\x\d.pdf"))
    End Sub

    ' ══ the «Instanță nouă Adobe» setting ══════════════════════════════════════
    <Fact>
    Public Sub NewInstanceAuto_LeavesTheProfileExactlyAsItIs()
        ' Reference equality on purpose: «Auto» must cost nothing, so the log line about which
        ' profile is in force stays about the profile, not about a copy of it.
        Assert.Same(AdobeViewerProfiles.Modern,
                    AdobeViewerProfiles.Modern.WithNewInstance(AdobeNewInstanceMode.Auto))
    End Sub

    <Fact>
    Public Sub ForcingNewInstance_OverridesTheModernProfilesFalse()
        Dim forced = AdobeViewerProfiles.Modern.WithNewInstance(AdobeNewInstanceMode.Da)
        Assert.True(forced.NewInstance)
        ' …and changes NOTHING else: this is the one lever, not a second profile.
        Assert.Equal(AdobeViewerProfiles.Modern.ClipRight, forced.ClipRight)
        Assert.Equal(AdobeViewerProfiles.Modern.Dx, forced.Dx)
        Assert.Equal(AdobeViewerProfiles.Modern.NoSplash, forced.NoSplash)
        Assert.Equal(AdobeViewerProfiles.Modern.OpenParametersText(), forced.OpenParametersText())
        Assert.Equal("/n ""C:\x\d.pdf""", AdobeWindowHosting.BuildArguments(forced, "C:\x\d.pdf"))
    End Sub

    <Fact>
    Public Sub ForcingNoNewInstance_OverridesTheClassicProfilesTrue()
        Dim forced = AdobeViewerProfiles.Classic.WithNewInstance(AdobeNewInstanceMode.Nu)
        Assert.False(forced.NewInstance)
        Assert.Equal("/s /A ""toolbar=0&navpanes=0"" ""C:\x\d.pdf""",
                     AdobeWindowHosting.BuildArguments(forced, "C:\x\d.pdf"))
    End Sub

    <Fact>
    Public Sub ForcingWhatTheProfileAlreadySays_IsANoOp()
        Assert.Same(AdobeViewerProfiles.Classic,
                    AdobeViewerProfiles.Classic.WithNewInstance(AdobeNewInstanceMode.Da))
        Assert.Same(AdobeViewerProfiles.Modern,
                    AdobeViewerProfiles.Modern.WithNewInstance(AdobeNewInstanceMode.Nu))
    End Sub

    ' ══ the geometry each profile produces ═════════════════════════════════════
    <Fact>
    Public Sub ModernProfile_PlacesTheWindowOversizedAndPulledLeft()
        ' 1000x800 host: clip makes it (0,-152) 1230x952, then dx = -130 pulls it left.
        Assert.Equal(New Rectangle(-130, -152, 1230, 952),
                     AdobeHostGeometry.Compute(New Size(1000, 800), AdobeViewerProfiles.Modern))
    End Sub

    <Fact>
    Public Sub ClassicProfile_FillsTheHostExactly()
        Assert.Equal(New Rectangle(0, 0, 1000, 800),
                     AdobeHostGeometry.Compute(New Size(1000, 800), AdobeViewerProfiles.Classic))
    End Sub

End Class
