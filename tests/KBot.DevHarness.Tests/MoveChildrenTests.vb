Option Strict On
Imports System.Drawing
Imports System.Linq
Imports KBot.DevHarness
Imports Xunit

' Slice 0023 pass 6 — moving child windows instead of hiding them or inflating the host.
'
' The bench can hide a child or clip the whole host; it could not MOVE one. Clipping enlarges the
' hosted window, so a fit-width zoom rescales the page to the inflated width and the clip starts
' eating document content; moving a child leaves the host size alone. These tests pin the pure
' half: delta semantics, outcome classification (a move that changed nothing must not read as a
' success) and the origin store that makes «readu la poziția inițială» exact.
Public Class MoveChildrenTests

    ' ══ deltas ═════════════════════════════════════════════════════════════════
    <Fact>
    Public Sub AbsentDeltas_AreZero_NotDefaultedToSomethingUseful()
        Dim e As New MoveChildEntry() With {.ByText = "AVSplitterView"}
        Assert.Equal(0, e.EffectiveDx())
        Assert.Equal(0, e.EffectiveDy())
        Assert.Equal(0, e.EffectiveDw())
        Assert.Equal(0, e.EffectiveDh())
        Assert.True(e.IsNoOp())
    End Sub

    <Fact>
    Public Sub TheSplitterHypothesis_IsNotANoOp()
        ' dx=-120 dy=-90 dw=+120 dh=+90 — one operation meant to replace both the left and the
        ' top clip. Whether it works on screen is a question for the operator, not for a test.
        Dim e As New MoveChildEntry() With {.ByText = "AVSplitterView", .Dx = -120, .Dy = -90,
                                            .Dw = 120, .Dh = 90}
        Assert.False(e.IsNoOp())
        Assert.Equal(-120, e.EffectiveDx())
        Assert.Equal(90, e.EffectiveDh())
    End Sub

    <Fact>
    Public Sub AnEntryWithOnlyOneDelta_IsStillAMove()
        Assert.False(New MoveChildEntry() With {.ByText = "x", .Dw = 1}.IsNoOp())
    End Sub

    ' ══ reapply options ════════════════════════════════════════════════════════
    <Fact>
    Public Sub ReapplyDefaultsToOff()
        ' Adobe fights back, but a periodic writer must never start because a section exists.
        Assert.False(New MoveOptionsConfig().ShouldReapply())
    End Sub

    <Fact>
    Public Sub ReapplyIntervalFallsBackToTheDefault_WhenAbsentOrNonsense()
        Assert.Equal(MoveOptionsConfig.DefaultReapplyIntervalMs, New MoveOptionsConfig().EffectiveIntervalMs())
        Assert.Equal(MoveOptionsConfig.DefaultReapplyIntervalMs,
                     New MoveOptionsConfig() With {.ReapplyIntervalMs = 0}.EffectiveIntervalMs())
        Assert.Equal(250, New MoveOptionsConfig() With {.ReapplyIntervalMs = 250}.EffectiveIntervalMs())
    End Sub

    ' ══ outcome classification ═════════════════════════════════════════════════
    <Fact>
    Public Sub AMoveThatChangedTheRectangle_IsAMove()
        Dim before As New Rectangle(131, 153, 1346, 1188)
        Dim after As New Rectangle(11, 63, 1466, 1278)
        Assert.Equal(MoveOutcome.Moved, MoveOutcomeClassifier.Classify(True, True, before, after))
    End Sub

    <Fact>
    Public Sub AMoveThatLandedOnTheSameRectangle_IsUnchanged()
        ' Adobe accepting the call and immediately putting the window back looks identical to a
        ' success unless the rectangles are compared. This is the hideChildren lesson, re-applied.
        Dim r As New Rectangle(131, 153, 1346, 1188)
        Assert.Equal(MoveOutcome.Unchanged, MoveOutcomeClassifier.Classify(True, True, r, r))
    End Sub

    <Fact>
    Public Sub AWindowThatIsNotThere_IsNotFound()
        Assert.Equal(MoveOutcome.NotFound,
                     MoveOutcomeClassifier.Classify(False, False, Rectangle.Empty, Rectangle.Empty))
    End Sub

    <Fact>
    Public Sub ARefusedApiCall_IsAFailure_NotAnUnchanged()
        Dim r As New Rectangle(1, 2, 3, 4)
        Assert.Equal(MoveOutcome.Failed, MoveOutcomeClassifier.Classify(True, False, r, r))
    End Sub

    <Fact>
    Public Sub LabelsAreTheRomanianOnesTheLogUses()
        Assert.Equal("MUTAT", MoveOutcomeClassifier.Label(MoveOutcome.Moved))
        Assert.Equal("NEGĂSIT", MoveOutcomeClassifier.Label(MoveOutcome.NotFound))
        Assert.Equal("NESCHIMBAT", MoveOutcomeClassifier.Label(MoveOutcome.Unchanged))
    End Sub

    <Fact>
    Public Sub AMoveLine_CarriesBothRectangles()
        Dim a As New MoveAttempt("AVSplitterView", MoveOutcome.Moved,
                                 New Rectangle(131, 153, 1346, 1188), New Rectangle(11, 63, 1466, 1278))
        Assert.Contains("131,153 1346x1188", a.LogLine())
        Assert.Contains("11,63 1466x1278", a.LogLine())
        Assert.Contains("MUTAT", a.LogLine())
    End Sub

    <Fact>
    Public Sub ANotFoundLine_DoesNotPretendToHaveRectangles()
        Dim a As New MoveAttempt("AVFoo", MoveOutcome.NotFound, Rectangle.Empty, Rectangle.Empty)
        Assert.DoesNotContain("->", a.LogLine())
    End Sub

    ' ══ summary ════════════════════════════════════════════════════════════════
    <Fact>
    Public Sub ASummaryWithNoRealMove_CarriesTheWarning()
        Dim s As New MoveAttemptSummary({
            New MoveAttempt("a", MoveOutcome.Unchanged, Rectangle.Empty, Rectangle.Empty),
            New MoveAttempt("b", MoveOutcome.NotFound, Rectangle.Empty, Rectangle.Empty)})
        Assert.True(s.ChangedNothing)
        Assert.Contains("ATENȚIE", s.SummaryLine())
    End Sub

    <Fact>
    Public Sub ASummaryWithOneRealMove_DoesNotWarn()
        Dim s As New MoveAttemptSummary({
            New MoveAttempt("a", MoveOutcome.Moved, New Rectangle(0, 0, 1, 1), New Rectangle(1, 1, 1, 1)),
            New MoveAttempt("b", MoveOutcome.NotFound, Rectangle.Empty, Rectangle.Empty)})
        Assert.False(s.ChangedNothing)
        Assert.Equal(1, s.MovedCount)
        Assert.Equal(1, s.NotFoundCount)
        Assert.DoesNotContain("ATENȚIE", s.SummaryLine())
    End Sub

    <Fact>
    Public Sub AnEmptySummary_SaysThereWasNothingToDo()
        Dim s As New MoveAttemptSummary(Nothing)
        Assert.False(s.ChangedNothing)   ' nothing attempted is not "changed nothing"
        Assert.Contains("nicio intrare", s.SummaryLine())
    End Sub

    ' ══ origin store ═══════════════════════════════════════════════════════════
    <Fact>
    Public Sub TheFirstRectangleWins_SoResetIsExact()
        ' Same rule as the registry snapshot: a second move must not overwrite the true original,
        ' or reset would restore a state the operator produced rather than Adobe's own layout.
        Dim store As New MoveOriginStore()
        store.Capture("AVSplitterView", New Rectangle(131, 153, 1346, 1188))
        store.Capture("AVSplitterView", New Rectangle(11, 63, 1466, 1278))
        Dim got As Rectangle = Rectangle.Empty
        Assert.True(store.TryGet("AVSplitterView", got))
        Assert.Equal(New Rectangle(131, 153, 1346, 1188), got)
    End Sub

    <Fact>
    Public Sub TheStoreIsKeyedByText_CaseInsensitively()
        ' Keyed by text, not HWND: handles change on every launch, «AVSplitterView» does not.
        Dim store As New MoveOriginStore()
        store.Capture("AVSplitterView", New Rectangle(1, 2, 3, 4))
        Dim got As Rectangle = Rectangle.Empty
        Assert.True(store.TryGet("avsplitterview", got))
        Assert.Equal(New Rectangle(1, 2, 3, 4), got)
    End Sub

    <Fact>
    Public Sub BlankTextIsNeverStored()
        Dim store As New MoveOriginStore()
        store.Capture("", New Rectangle(1, 2, 3, 4))
        store.Capture(Nothing, New Rectangle(1, 2, 3, 4))
        Assert.Equal(0, store.Count)
    End Sub

    <Fact>
    Public Sub UnknownTextYieldsNothing()
        Dim got As Rectangle = Rectangle.Empty
        Assert.False(New MoveOriginStore().TryGet("AVNimic", got))
    End Sub

    <Fact>
    Public Sub ClearForgetsEverything()
        Dim store As New MoveOriginStore()
        store.Capture("a", New Rectangle(1, 1, 1, 1))
        store.Clear()
        Assert.Equal(0, store.Count)
        Assert.Empty(store.Texts())
    End Sub

    ' ══ scenario parsing ═══════════════════════════════════════════════════════
    <Fact>
    Public Sub TheStepNameIsRecognised()
        Assert.True(HarnessScenarioSteps.IsKnown("moveChildren"))
        Assert.Contains("moveChildren", HarnessScenarioSteps.AllAsText())
    End Sub

    <Fact>
    Public Sub AScenarioCarriesTheEntriesInFileOrder()
        Const json As String = "{
  ""schema"": 1,
  ""moveChildren"": [
    { ""byText"": ""AVSplitterView"", ""dx"": -120, ""dy"": -90, ""dw"": 120, ""dh"": 90 },
    { ""byText"": ""AVAlt"", ""dx"": 5 }
  ],
  ""moveOptions"": { ""reapply"": true, ""reapplyIntervalMs"": 500 },
  ""scenario"": [ ""moveChildren"" ]
}"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid, String.Join(" / ", r.Errors))
        Assert.Equal(2, r.Scenario.MoveChildren.Count)
        Assert.Equal("AVSplitterView", r.Scenario.MoveChildren(0).ByText)
        Assert.Equal(-120, r.Scenario.MoveChildren(0).EffectiveDx())
        ' Second entry names only dx: the rest are absent, i.e. zero, not inherited from the first.
        Assert.Equal("AVAlt", r.Scenario.MoveChildren(1).ByText)
        Assert.Equal(5, r.Scenario.MoveChildren(1).EffectiveDx())
        Assert.Equal(0, r.Scenario.MoveChildren(1).EffectiveDy())
        Assert.True(r.Scenario.MoveOptions.ShouldReapply())
        Assert.Equal(500, r.Scenario.MoveOptions.EffectiveIntervalMs())
    End Sub

    <Fact>
    Public Sub AnAbsentMoveSection_StaysNothing_NotAnEmptyList()
        ' Absent means "do nothing", and must stay distinguishable from an empty array.
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""scenario"": [ ""probe"" ] }")
        Assert.True(r.IsValid)
        Assert.Null(r.Scenario.MoveChildren)
        Assert.Null(r.Scenario.MoveOptions)
    End Sub

    <Fact>
    Public Sub AnEntryWithoutByText_IsAnError_NotASilentSkip()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""moveChildren"": [ { ""dx"": -120 } ], ""scenario"": [ ""moveChildren"" ] }")
        Assert.False(r.IsValid)
        Assert.Contains(r.Errors, Function(e) e.Contains("byText"))
    End Sub

    <Fact>
    Public Sub AnAllZeroEntry_IsOnlyAWarning()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""moveChildren"": [ { ""byText"": ""AVX"" } ], ""scenario"": [ ""moveChildren"" ] }")
        Assert.True(r.IsValid)
        Assert.Contains(r.Warnings, Function(w) w.Contains("AVX"))
    End Sub

    <Fact>
    Public Sub UnknownPropertiesInsideAMoveEntry_AreReported()
        Dim r = HarnessScenarioReader.Read(
            "{ ""schema"": 1, ""moveChildren"": [ { ""byText"": ""AVX"", ""dz"": 3 } ], ""scenario"": [ ""moveChildren"" ] }")
        Assert.Contains(r.Warnings, Function(w) w.Contains("moveChildren[0].dz"))
    End Sub

    <Fact>
    Public Sub TheShippedSplitterScenario_ParsesAndTargetsTheSplitter()
        ' The gutter lives INSIDE AVSplitationPageView, to the left of AVSplitterView — moving the
        ' page view would take the gutter along with it. Pin the target so that stays deliberate.
        Dim path As String = IO.Path.Combine(AppContext.BaseDirectory, "Config", "rhp_05_muta_splitter.json")
        Assert.True(IO.File.Exists(path), "rhp_05_muta_splitter.json lipsește din Config\")
        Dim r = HarnessScenarioReader.Read(IO.File.ReadAllText(path))
        Assert.True(r.IsValid, String.Join(" / ", r.Errors))
        Dim entry = r.Scenario.MoveChildren.Single()
        Assert.Equal("AVSplitterView", entry.ByText)
        Assert.Equal(-120, entry.EffectiveDx())
        Assert.Equal(-90, entry.EffectiveDy())
        Assert.Equal(120, entry.EffectiveDw())
        Assert.Equal(90, entry.EffectiveDh())
        ' Clipping is explicitly OFF: with both levers active nothing on screen would be attributable.
        Assert.False(r.Scenario.Clip.Enabled.Value)
        Assert.Contains("moveChildren", r.Scenario.Scenario)
    End Sub

End Class
