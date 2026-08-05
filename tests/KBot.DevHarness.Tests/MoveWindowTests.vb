Option Strict On
Imports System.Drawing
Imports System.Linq
' Slice 0024 moved HostedWindowGeometry / MoveOutcome into KBot.Controls, shared with the shipping
' DDF preview. The tests are unchanged — the same assertions now pin the shared implementation.
Imports KBot.Controls
Imports KBot.DevHarness
Imports Xunit

' Slice 0023 pass 6 — placing the HOSTED Adobe window with dx/dy/dw/dh.
'
' The deltas drive the same window clip right/top already drive; they are its general form (clip
' right is a dw, clip top is a dy plus a dh). Everything goes through one rectangle, so a delta is
' re-applied by the existing layout paths and nothing has to keep re-imposing it. These tests pin
' the pure half: the arithmetic, and the outcome classification that keeps a placement Adobe
' silently ignored from reading like a success.
Public Class MoveWindowTests

    ' ══ geometry ═══════════════════════════════════════════════════════════════
    Private Shared ReadOnly Panel As New Rectangle(0, 0, 1000, 800)

    <Fact>
    Public Sub ZeroDeltas_LeaveTheRectangleAlone()
        Assert.Equal(Panel, HostedWindowGeometry.Offset(Panel, 0, 0, 0, 0))
        Assert.True(HostedWindowGeometry.IsNeutral(0, 0, 0, 0))
    End Sub

    <Fact>
    Public Sub NegativeDx_PullsTheWindowLeft_SoTheLeftBandLeavesTheVisibleArea()
        Dim r = HostedWindowGeometry.Offset(Panel, -120, 0, 120, 0)
        Assert.Equal(-120, r.X)
        Assert.Equal(1120, r.Width)
        ' The right edge still lands exactly on the panel edge: 120 pulled off the left, 120 given
        ' back in width. That is the whole trick, and it is why dw exists next to dx.
        Assert.Equal(1000, r.Right)
    End Sub

    <Fact>
    Public Sub NegativeDy_PullsTheWindowUp_TheSameWay()
        Dim r = HostedWindowGeometry.Offset(Panel, 0, -90, 0, 90)
        Assert.Equal(-90, r.Y)
        Assert.Equal(890, r.Height)
        Assert.Equal(800, r.Bottom)
    End Sub

    <Fact>
    Public Sub TheShippedHypothesis_MovesBothEdgesAtOnce()
        Dim r = HostedWindowGeometry.Offset(Panel, -120, -90, 120, 90)
        Assert.Equal(New Rectangle(-120, -90, 1120, 890), r)
        Assert.Equal(1000, r.Right)
        Assert.Equal(800, r.Bottom)
    End Sub

    <Fact>
    Public Sub DeltasComposeOnTopOfAClippedRectangle()
        ' Clip right=200 top=60 produces (0,-60,1200,860); the move then shifts THAT.
        Dim clipped As New Rectangle(0, -60, 1200, 860)
        Assert.Equal(New Rectangle(-120, -150, 1320, 950),
                     HostedWindowGeometry.Offset(clipped, -120, -90, 120, 90))
    End Sub

    <Fact>
    Public Sub SizeIsFlooredAtOne_SoTheWindowCannotBeMadeToVanish()
        ' A zero-sized window would disappear with no way back except a relaunch.
        Dim r = HostedWindowGeometry.Offset(Panel, 0, 0, -5000, -5000)
        Assert.Equal(1, r.Width)
        Assert.Equal(1, r.Height)
    End Sub

    <Fact>
    Public Sub PositiveDeltas_AreAllowedToo()
        ' Nothing forces the deltas negative: pushing the window right/down is a legitimate probe.
        Dim r = HostedWindowGeometry.Offset(Panel, 40, 25, -40, -25)
        Assert.Equal(New Rectangle(40, 25, 960, 775), r)
    End Sub

    <Fact>
    Public Sub AnyNonZeroDelta_IsNotNeutral()
        Assert.False(HostedWindowGeometry.IsNeutral(1, 0, 0, 0))
        Assert.False(HostedWindowGeometry.IsNeutral(0, 0, 0, -1))
    End Sub

    ' ══ outcome classification ═════════════════════════════════════════════════
    <Fact>
    Public Sub APlacementThatChangedTheRectangle_IsAMove()
        Assert.Equal(MoveOutcome.Moved,
                     MoveOutcomeClassifier.Classify(True, True,
                                                    New Rectangle(0, 0, 1000, 800),
                                                    New Rectangle(-120, -90, 1120, 890)))
    End Sub

    <Fact>
    Public Sub APlacementThatLandedOnTheSameRectangle_IsUnchanged()
        ' Adobe accepting the call and keeping the old rectangle looks identical to success unless
        ' the rectangles are compared. The hideChildren lesson from pass 4, applied here first.
        Dim r As New Rectangle(0, 0, 1000, 800)
        Assert.Equal(MoveOutcome.Unchanged, MoveOutcomeClassifier.Classify(True, True, r, r))
    End Sub

    <Fact>
    Public Sub NoHostedWindow_IsNotFound()
        Assert.Equal(MoveOutcome.NotFound,
                     MoveOutcomeClassifier.Classify(False, False, Rectangle.Empty, Rectangle.Empty))
    End Sub

    <Fact>
    Public Sub ARefusedCall_IsAFailure_NotAnUnchanged()
        Dim r As New Rectangle(1, 2, 3, 4)
        Assert.Equal(MoveOutcome.Failed, MoveOutcomeClassifier.Classify(True, False, r, r))
    End Sub

    <Fact>
    Public Sub LabelsAreTheRomanianOnesTheLogUses()
        Assert.Equal("MUTAT", MoveOutcomeClassifier.Label(MoveOutcome.Moved))
        Assert.Equal("NEGĂSIT", MoveOutcomeClassifier.Label(MoveOutcome.NotFound))
        Assert.Equal("NESCHIMBAT", MoveOutcomeClassifier.Label(MoveOutcome.Unchanged))
        Assert.Equal("EȘUAT", MoveOutcomeClassifier.Label(MoveOutcome.Failed))
    End Sub

    <Fact>
    Public Sub RectanglesAreDescribedTheWayTheProbeLogsThem()
        Assert.Equal("-120,-90 1120x890",
                     MoveOutcomeClassifier.Describe(New Rectangle(-120, -90, 1120, 890)))
    End Sub

    ' ══ scenario parsing ═══════════════════════════════════════════════════════
    <Fact>
    Public Sub TheStepNameIsRecognised()
        Assert.True(HarnessScenarioSteps.IsKnown("applyMove"))
        Assert.Contains("applyMove", HarnessScenarioSteps.AllAsText())
    End Sub

    <Fact>
    Public Sub TheOldChildStepNameIsGone()
        ' The first attempt at this pass moved individual Adobe child windows. Wrong mechanism —
        ' a file written against it must fail loudly rather than silently do nothing.
        Assert.False(HarnessScenarioSteps.IsKnown("moveChildren"))
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""scenario"": [ ""moveChildren"" ] }")
        Assert.False(r.IsValid)
    End Sub

    <Fact>
    Public Sub AScenarioCarriesTheFourDeltas()
        Const json As String = "{
  ""schema"": 1,
  ""move"": { ""dx"": -120, ""dy"": -90, ""dw"": 120, ""dh"": 90 },
  ""scenario"": [ ""applyMove"" ]
}"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid, String.Join(" / ", r.Errors))
        Assert.Equal(-120, r.Scenario.Move.EffectiveDx())
        Assert.Equal(-90, r.Scenario.Move.EffectiveDy())
        Assert.Equal(120, r.Scenario.Move.EffectiveDw())
        Assert.Equal(90, r.Scenario.Move.EffectiveDh())
        Assert.False(r.Scenario.Move.IsNoOp())
    End Sub

    <Fact>
    Public Sub APartialMoveSection_LeavesTheOthersAtZero()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""move"": { ""dx"": -50 }, ""scenario"": [ ""applyMove"" ] }")
        Assert.Equal(-50, r.Scenario.Move.EffectiveDx())
        Assert.Equal(0, r.Scenario.Move.EffectiveDy())
        Assert.Equal(0, r.Scenario.Move.EffectiveDw())
    End Sub

    <Fact>
    Public Sub AnAbsentMoveSection_StaysNothing()
        ' Absent means "leave the window where the panel puts it", and must stay distinguishable
        ' from a present section full of zeros.
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""scenario"": [ ""probe"" ] }")
        Assert.True(r.IsValid)
        Assert.Null(r.Scenario.Move)
    End Sub

    <Fact>
    Public Sub AnAllZeroMoveSection_IsAWarning()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""move"": { ""dx"": 0 }, ""scenario"": [ ""applyMove"" ] }")
        Assert.True(r.IsValid)
        Assert.NotNull(r.Scenario.Move)
        Assert.Contains(r.Warnings, Function(w) w.Contains("move"))
    End Sub

    <Fact>
    Public Sub UnknownPropertiesInTheMoveSection_AreReported()
        Dim r = HarnessScenarioReader.Read(
            "{ ""schema"": 1, ""move"": { ""dx"": -1, ""dz"": 3 }, ""scenario"": [ ""applyMove"" ] }")
        Assert.Contains(r.Warnings, Function(w) w.Contains("move.dz"))
    End Sub

    <Fact>
    Public Sub TheShippedScenario_ParsesAndCarriesTheHypothesis()
        Dim path As String = IO.Path.Combine(AppContext.BaseDirectory, "Config", "rhp_05_muta_fereastra.json")
        Assert.True(IO.File.Exists(path), "rhp_05_muta_fereastra.json lipsește din Config\")
        Dim r = HarnessScenarioReader.Read(IO.File.ReadAllText(path))
        Assert.True(r.IsValid, String.Join(" / ", r.Errors))
        Assert.Equal(-120, r.Scenario.Move.EffectiveDx())
        Assert.Equal(-90, r.Scenario.Move.EffectiveDy())
        Assert.Equal(120, r.Scenario.Move.EffectiveDw())
        Assert.Equal(90, r.Scenario.Move.EffectiveDh())
        ' Clipping is explicitly OFF: the two compose, so with both active nothing on screen would
        ' be attributable to one mechanism.
        Assert.False(r.Scenario.Clip.Enabled.Value)
        Assert.Contains("applyMove", r.Scenario.Scenario)
    End Sub

End Class
