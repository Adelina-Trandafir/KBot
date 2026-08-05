Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports KBot.Controls
Imports Xunit

' Slice 0024 — detecting the Adobe UI generation from the window tree, and picking the profile.
'
' The detection is DRIVEN BY A FAKE WINDOW TREE, which is the whole reason AdobeWindowNode is a
' plain POCO: Adobe does not have to be installed for these rules to be tested, and a tree recorded
' from a real probe can be replayed here verbatim.
'
' The markers below are the ones the bench actually logged on 04.08.2026: the modern Acrobat tree
' carries AV2DocumentTabView and AV2DockableTabStripView as window TEXT on AVL_AVView children.
Public Class AdobeUiDetectorTests

    Private Shared Function Node(text As String,
                                 Optional cls As String = "AVL_AVView",
                                 Optional w As Integer = 100,
                                 Optional h As Integer = 100,
                                 Optional x As Integer = 0) As AdobeWindowNode
        Return New AdobeWindowNode(New IntPtr(&H1000), cls, text, New Rectangle(x, 0, w, h), True, 1)
    End Function

    Private Shared Function ModernTree() As List(Of AdobeWindowNode)
        Return New List(Of AdobeWindowNode) From {
            Node("AVSplitterView"), Node("AV2MetadataPanel"),
            Node("AV2DocumentTabView"), Node("AV2DockableTabStripView"),
            Node("AVUITopRightCommandCluster")}
    End Function

    Private Shared Function ClassicTree() As List(Of AdobeWindowNode)
        Return New List(Of AdobeWindowNode) From {
            Node("AVSplitterView"), Node("AVTaskPaneHostView"), Node("AVScrolledPageView")}
    End Function

    ' ══ detection ══════════════════════════════════════════════════════════════
    <Fact>
    Public Sub TaskPaneHost_MeansClassic()
        Dim d = AdobeUiDetector.Detect(ClassicTree())
        Assert.Equal(AdobeUiGeneration.Classic, d.Generation)
        Assert.Contains("AVTaskPaneHostView", d.Evidence)
        Assert.False(d.Ambiguous)
    End Sub

    <Fact>
    Public Sub AV2TabStrip_MeansModern()
        Dim d = AdobeUiDetector.Detect(ModernTree())
        Assert.Equal(AdobeUiGeneration.Modern, d.Generation)
        Assert.Contains("AV2DockableTabStripView", d.Evidence)
        ' The log line the brief asked for, verbatim.
        Assert.Equal("Mod detectat: Modern (AV2DockableTabStripView prezent)", d.Describe())
    End Sub

    <Fact>
    Public Sub EitherModernMarkerAlone_IsEnough()
        Assert.Equal(AdobeUiGeneration.Modern,
                     AdobeUiDetector.Detect({Node("AV2DocumentTabView")}).Generation)
        Assert.Equal(AdobeUiGeneration.Modern,
                     AdobeUiDetector.Detect({Node("AV2DockableTabStripView")}).Generation)
    End Sub

    <Fact>
    Public Sub AMarkerArrivingAsACLASSNAME_CountsToo()
        ' Today the markers are window text. A future Adobe could make them class names; matching
        ' both costs nothing and saves an investigation.
        Assert.Equal(AdobeUiGeneration.Modern,
                     AdobeUiDetector.Detect({Node("", cls:="AV2DocumentTabView")}).Generation)
    End Sub

    <Fact>
    Public Sub NoKnownMarker_IsUnknownAndSaysHowManyWindowsItLookedAt()
        Dim d = AdobeUiDetector.Detect({Node("AVSplitterView"), Node("Edit", cls:="Edit")})
        Assert.Equal(AdobeUiGeneration.Unknown, d.Generation)
        Assert.Contains("2", d.Evidence)
    End Sub

    <Fact>
    Public Sub AnEmptyTree_IsUnknown_NotAnException()
        Assert.Equal(AdobeUiGeneration.Unknown, AdobeUiDetector.Detect(Nothing).Generation)
        Assert.Equal(AdobeUiGeneration.Unknown,
                     AdobeUiDetector.Detect(New List(Of AdobeWindowNode)()).Generation)
    End Sub

    <Fact>
    Public Sub MarkersOfBothGenerations_AreReportedAsAmbiguous()
        Dim mixed As New List(Of AdobeWindowNode)(ClassicTree())
        mixed.AddRange(ModernTree())
        Dim d = AdobeUiDetector.Detect(mixed)
        ' First rule wins (classic), but the ambiguity is stated out loud rather than swallowed.
        Assert.Equal(AdobeUiGeneration.Classic, d.Generation)
        Assert.True(d.Ambiguous)
        Assert.Contains("AMBELE", d.Describe())
    End Sub

    ' ══ profile selection ══════════════════════════════════════════════════════
    <Fact>
    Public Sub Auto_FollowsTheDetection()
        Dim modern = AdobeViewerProfiles.Resolve(AdobeViewerMode.Auto, AdobeUiDetector.Detect(ModernTree()))
        Assert.Same(AdobeViewerProfiles.Modern, modern.Profile)
        Assert.False(modern.Mismatch)

        Dim classic = AdobeViewerProfiles.Resolve(AdobeViewerMode.Auto, AdobeUiDetector.Detect(ClassicTree()))
        Assert.Same(AdobeViewerProfiles.Classic, classic.Profile)
        Assert.False(classic.Mismatch)
    End Sub

    <Fact>
    Public Sub Auto_OnAnUnrecognisedTree_FallsBackToClassic()
        ' The conservative choice: the classic profile neither clips nor moves the window, so a
        ' wrong guess shows too much chrome instead of a blank rectangle.
        Dim c = AdobeViewerProfiles.Resolve(AdobeViewerMode.Auto, AdobeUiDetector.Detect({Node("Nimic")}))
        Assert.Same(AdobeViewerProfiles.Classic, c.Profile)
        Assert.Equal(AdobeUiGeneration.Unknown, c.Detected)
        Assert.False(c.Mismatch)
    End Sub

    <Fact>
    Public Sub ForcedModern_WinsOverAClassicTree_AndIsFlaggedAsAMismatch()
        Dim c = AdobeViewerProfiles.Resolve(AdobeViewerMode.Modern, AdobeUiDetector.Detect(ClassicTree()))
        Assert.Same(AdobeViewerProfiles.Modern, c.Profile)
        Assert.Equal(AdobeUiGeneration.Classic, c.Detected)
        Assert.True(c.Mismatch)
    End Sub

    <Fact>
    Public Sub ForcedClassic_WinsOverAModernTree_AndIsFlaggedAsAMismatch()
        Dim c = AdobeViewerProfiles.Resolve(AdobeViewerMode.Classic, AdobeUiDetector.Detect(ModernTree()))
        Assert.Same(AdobeViewerProfiles.Classic, c.Profile)
        Assert.Equal(AdobeUiGeneration.Modern, c.Detected)
        Assert.True(c.Mismatch)
    End Sub

    <Fact>
    Public Sub ForcedMode_AgreeingWithTheTree_IsNotAMismatch()
        Assert.False(AdobeViewerProfiles.Resolve(AdobeViewerMode.Modern,
                                                 AdobeUiDetector.Detect(ModernTree())).Mismatch)
        Assert.False(AdobeViewerProfiles.Resolve(AdobeViewerMode.Classic,
                                                 AdobeUiDetector.Detect(ClassicTree())).Mismatch)
    End Sub

    <Fact>
    Public Sub ForcedMode_OnAnUnrecognisedTree_IsNotAMismatch()
        ' "Unknown" contradicts nothing: an Adobe we cannot classify is not evidence that the
        ' operator's forced choice is wrong, and crying mismatch there would train them to ignore it.
        Assert.False(AdobeViewerProfiles.Resolve(AdobeViewerMode.Modern,
                                                 AdobeUiDetector.Detect({Node("Nimic")})).Mismatch)
        Assert.False(AdobeViewerProfiles.Resolve(AdobeViewerMode.Classic,
                                                 AdobeUiDetector.Detect({Node("Nimic")})).Mismatch)
    End Sub

    <Fact>
    Public Sub ANothingDetection_StillResolvesToAProfile()
        ' The first document of a session resolves its LAUNCH profile before anything was probed.
        Dim c = AdobeViewerProfiles.Resolve(AdobeViewerMode.Auto, Nothing)
        Assert.Same(AdobeViewerProfiles.Classic, c.Profile)
    End Sub

End Class
