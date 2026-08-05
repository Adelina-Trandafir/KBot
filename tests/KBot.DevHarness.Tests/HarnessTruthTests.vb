Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.Json
Imports Microsoft.Win32
' HideOutcome / HideAttemptSummary moved to KBot.Controls in slice 0024 (shared with the DDF
' preview's popup watcher); the assertions below are untouched.
Imports KBot.Controls
Imports KBot.DevHarness
Imports Xunit

' Slice 0023, "make the bench tell the truth" pass (§6). Pure: no registry, no Adobe, no windows.
'
' Each group here pins one of the defects that let four scenario runs on 04.08 report success while
' testing nothing.
Public Class HarnessTruthTests

    ' ══ §1.1 userPrefs values are carried LITERALLY ════════════════════════════
    Private Shared Function Values(json As String) As Dictionary(Of String, JsonElement)
        Return JsonSerializer.Deserialize(Of Dictionary(Of String, JsonElement))(json)
    End Function

    <Fact>
    Public Sub Integer_BecomesADwordIntent_WithTheValueFromTheFile()
        ' The exact case that was clamped: the file said 1, the checkbox said 0.
        Dim intents = UserPrefIntentFactory.FromValues(Values("{ ""bEnableAv2"": 1 }"))
        Dim i = Assert.Single(intents)
        Assert.Equal("bEnableAv2", i.Name)
        Assert.Equal(UserPrefAction.WriteDword, i.Action)
        Assert.Equal(RegistryValueKind.DWord, i.Kind)
        Assert.Equal(1, CInt(i.Value))
    End Sub

    <Fact>
    Public Sub String_BecomesAnSzIntent()
        Dim i = Assert.Single(UserPrefIntentFactory.FromValues(Values("{ ""aDefaultRHPViewMode_L"": ""Collapsed"" }")))
        Assert.Equal(UserPrefAction.WriteString, i.Action)
        Assert.Equal(RegistryValueKind.String, i.Kind)
        Assert.Equal("Collapsed", CStr(i.Value))
    End Sub

    <Fact>
    Public Sub Null_BecomesADeleteIntent_NotAZero()
        ' "remove it" must stay distinct from "set it to 0".
        Dim i = Assert.Single(UserPrefIntentFactory.FromValues(Values("{ ""bRHPSticky"": null }")))
        Assert.Equal(UserPrefAction.Delete, i.Action)
        Assert.Null(i.Value)
        Assert.Equal("(șters)", i.RequestedText())
    End Sub

    <Fact>
    Public Sub AbsentSection_ProducesNoIntentAtAll()
        ' …and "leave alone" stays distinct from both of the above.
        Assert.Empty(UserPrefIntentFactory.FromValues(Nothing))
        Assert.Empty(UserPrefIntentFactory.FromValues(Values("{ }")))
    End Sub

    <Fact>
    Public Sub UnsupportedJsonKind_IsReported_NotGuessed()
        Dim rejected As List(Of String) = Nothing
        Dim intents = UserPrefIntentFactory.FromValues(Values("{ ""x"": true, ""y"": [1,2] }"), rejected)
        Assert.Empty(intents)
        Assert.Equal(2, rejected.Count)
    End Sub

    <Fact>
    Public Sub ArbitraryValueNames_AreAllowed_NoFixedList()
        ' A preference we have never seen before must be drivable from a file without a code change.
        Dim i = Assert.Single(UserPrefIntentFactory.FromValues(Values("{ ""bSomeBrandNewPref"": 7 }")))
        Assert.Equal("bSomeBrandNewPref", i.Name)
        Assert.Equal(7, CInt(i.Value))
    End Sub

    <Fact>
    Public Sub Merge_ScenarioWins_CheckboxesFillTheGaps()
        Dim scenario = UserPrefIntentFactory.FromValues(Values("{ ""bEnableAv2"": 1 }"))
        Dim checkboxes = New List(Of UserPrefIntent) From {
            New UserPrefIntent("bEnableAv2", UserPrefAction.WriteDword, 0),
            New UserPrefIntent("bRHPSticky", UserPrefAction.WriteDword, 1)}

        Dim merged = UserPrefIntentFactory.Merge(scenario, checkboxes)

        Assert.Equal(2, merged.Count)
        Assert.Equal(1, CInt(merged.Single(Function(m) m.Name = "bEnableAv2").Value))  ' file, not checkbox
        Assert.Equal(1, CInt(merged.Single(Function(m) m.Name = "bRHPSticky").Value))  ' checkbox filled the gap
    End Sub

    ' ══ §1.3 write-back verification ═══════════════════════════════════════════
    Private Const Hive As String = "HKEY_CURRENT_USER\Software\Adobe\Adobe Acrobat\DC\AVGeneral"

    <Fact>
    Public Sub Verify_DwordMatch_IsOk()
        Dim intent As New UserPrefIntent("bEnableAv2", UserPrefAction.WriteDword, 1)
        Dim actual = RegistryValueSnapshot.PresentSnap(Hive, "bEnableAv2", RegistryValueKind.DWord, 1)
        Assert.True(RegistryWriteVerifier.Verify(Hive, intent, actual).Matches)
    End Sub

    <Fact>
    Public Sub Verify_DwordMismatch_IsDetected_TheCaseThatWasMissed()
        ' Asked for 1, machine holds 0 — logged as success before this pass existed.
        Dim intent As New UserPrefIntent("bEnableAv2", UserPrefAction.WriteDword, 1)
        Dim actual = RegistryValueSnapshot.PresentSnap(Hive, "bEnableAv2", RegistryValueKind.DWord, 0)
        Dim v = RegistryWriteVerifier.Verify(Hive, intent, actual)
        Assert.False(v.Matches)
        Assert.Contains("EȘEC", v.Message)
        Assert.Contains("cerut 1", v.Message)
        Assert.Contains("citit 0", v.Message)
    End Sub

    <Fact>
    Public Sub Verify_WrongKind_IsAMismatch()
        Dim intent As New UserPrefIntent("aDefaultRHPViewMode_L", UserPrefAction.WriteString, "Collapsed")
        Dim actual = RegistryValueSnapshot.PresentSnap(Hive, "aDefaultRHPViewMode_L", RegistryValueKind.DWord, 0)
        Assert.False(RegistryWriteVerifier.Verify(Hive, intent, actual).Matches)
    End Sub

    <Fact>
    Public Sub Verify_IntendedWriteButAbsent_IsAMismatch()
        Dim intent As New UserPrefIntent("bRHPSticky", UserPrefAction.WriteDword, 1)
        Dim v = RegistryWriteVerifier.Verify(Hive, intent, RegistryValueSnapshot.AbsentSnap(Hive, "bRHPSticky"))
        Assert.False(v.Matches)
        Assert.Contains("(absent)", v.Message)
    End Sub

    <Fact>
    Public Sub Verify_IntendedDelete_StillPresent_IsAMismatch()
        Dim intent As New UserPrefIntent("bRHPSticky", UserPrefAction.Delete, Nothing)
        Dim actual = RegistryValueSnapshot.PresentSnap(Hive, "bRHPSticky", RegistryValueKind.DWord, 1)
        Dim v = RegistryWriteVerifier.Verify(Hive, intent, actual)
        Assert.False(v.Matches)
        Assert.Contains("(șters)", v.Message)
    End Sub

    <Fact>
    Public Sub Verify_IntendedDelete_AndAbsent_IsOk()
        Dim intent As New UserPrefIntent("bRHPSticky", UserPrefAction.Delete, Nothing)
        Assert.True(RegistryWriteVerifier.Verify(Hive, intent,
            RegistryValueSnapshot.AbsentSnap(Hive, "bRHPSticky")).Matches)
    End Sub

    ' ══ §2 hide outcome classification ═════════════════════════════════════════
    <Fact>
    Public Sub Classify_VisibleWithSize_IsARealHide()
        Assert.Equal(HideOutcome.Hidden, HideOutcomeClassifier.Classify(True, True, 346, 1440))
    End Sub

    <Fact>
    Public Sub Classify_ZeroSize_IsNotAHide_EvenWhenVisible()
        ' The 04.08 case: 0x0. Hiding it changes nothing and proves nothing.
        Assert.Equal(HideOutcome.ZeroSize, HideOutcomeClassifier.Classify(True, True, 0, 0))
        Assert.Equal(HideOutcome.ZeroSize, HideOutcomeClassifier.Classify(True, False, 0, 0))
    End Sub

    <Fact>
    Public Sub Classify_InvisibleWithSize_IsAlreadyHidden()
        Assert.Equal(HideOutcome.AlreadyHidden, HideOutcomeClassifier.Classify(True, False, 346, 1440))
    End Sub

    <Fact>
    Public Sub Classify_NotFound()
        Assert.Equal(HideOutcome.NotFound, HideOutcomeClassifier.Classify(False, False, 0, 0))
    End Sub

    <Fact>
    Public Sub Summary_WhenNothingChanged_SaysSoLoudly()
        ' Reproduces the 04.08 log line that claimed success.
        Dim s As New HideAttemptSummary(1, New HideOutcome() {HideOutcome.ZeroSize})
        Assert.True(s.ChangedNothing)
        Assert.Equal(0, s.HiddenCount)
        Assert.Equal(1, s.FoundCount)
        Dim line As String = s.SummaryLine(1, 10)
        Assert.Contains("0 ascunse", line)
        Assert.Contains("ATENȚIE: nicio schimbare reală", line)
    End Sub

    <Fact>
    Public Sub Summary_WhenSomethingWasHidden_IsNotAWarning()
        Dim s As New HideAttemptSummary(1, New HideOutcome() {HideOutcome.Hidden})
        Assert.False(s.ChangedNothing)
        Assert.DoesNotContain("ATENȚIE", s.SummaryLine(1, 10))
    End Sub

    ' ══ §3 baseline evaluation ═════════════════════════════════════════════════
    Private Shared Function Policy(present As Boolean) As PolicyReading
        Return New PolicyReading("HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Adobe\Adobe Acrobat\DC\FeatureLockDown",
                                 "bAcroSuppressUpsell", present, If(present, CObj(1), Nothing))
    End Function

    <Fact>
    Public Sub Baseline_NoPolicyPresent_IsClean()
        Dim a = BaselineEvaluator.Evaluate({Policy(False), Policy(False)}, requireCleanBaseline:=False)
        Assert.Equal(BaselineVerdict.Clean, a.Verdict)
        Assert.True(a.IsClean)
        Assert.Empty(a.Active)
    End Sub

    <Fact>
    Public Sub Baseline_PolicyPresent_WarnsByDefault()
        Dim a = BaselineEvaluator.Evaluate({Policy(True)}, requireCleanBaseline:=False)
        Assert.Equal(BaselineVerdict.Warn, a.Verdict)
        Assert.Single(a.Active)
        Assert.Contains("bAcroSuppressUpsell", a.WarningText())
    End Sub

    <Fact>
    Public Sub Baseline_PolicyPresent_BlocksWhenScenarioDemandsClean()
        Dim a = BaselineEvaluator.Evaluate({Policy(True)}, requireCleanBaseline:=True)
        Assert.Equal(BaselineVerdict.Block, a.Verdict)
        Assert.Contains("requireCleanBaseline", a.BlockedText())
    End Sub

    <Fact>
    Public Sub Baseline_RequireCleanOnACleanMachine_StillRuns()
        Dim a = BaselineEvaluator.Evaluate({Policy(False)}, requireCleanBaseline:=True)
        Assert.Equal(BaselineVerdict.Clean, a.Verdict)
    End Sub

    <Fact>
    Public Sub Baseline_NullReadings_AreTreatedAsClean()
        Assert.Equal(BaselineVerdict.Clean, BaselineEvaluator.Evaluate(Nothing, False).Verdict)
    End Sub

    ' ── pass 5: HKCU pane preferences contaminate the baseline just as a policy does ──
    ' A baseline read on 04.08 said "curată" while bRHPSticky = 1 and
    ' aDefaultRHPViewMode_L = "Collapsed" were still set — Adobe already remembering a collapsed
    ' pane. Cleaning only the policies would still leave a machine that is not neutral.
    Private Shared Function Pref(name As String, present As Boolean, value As Object) As PolicyReading
        Return New PolicyReading("HKEY_CURRENT_USER\Software\Adobe\Adobe Acrobat\DC\AVGeneral",
                                 name, present, value, BaselineOrigin.UserPreference)
    End Function

    <Fact>
    Public Sub Baseline_LeftoverHkcuPreference_IsNotClean()
        Dim a = BaselineEvaluator.Evaluate({Policy(False), Pref("bRHPSticky", True, 1)},
                                           requireCleanBaseline:=False)
        Assert.Equal(BaselineVerdict.Warn, a.Verdict)
        Assert.Contains("bRHPSticky", a.WarningText())
    End Sub

    <Fact>
    Public Sub Baseline_LeftoverHkcuPreference_BlocksAScenarioDemandingClean()
        Dim a = BaselineEvaluator.Evaluate({Pref("aDefaultRHPViewMode_L", True, "Collapsed")},
                                           requireCleanBaseline:=True)
        Assert.Equal(BaselineVerdict.Block, a.Verdict)
        ' The cleanup route for HKCU is «șterge» + apply, not the elevated revert.
        Assert.Contains("șterge", a.BlockedText())
    End Sub

    <Fact>
    Public Sub Baseline_SeparatesPoliciesFromPreferences()
        Dim a = BaselineEvaluator.Evaluate(
            {Policy(True), Pref("bRHPSticky", True, 1), Pref("bExpandRHPInViewer", False, Nothing)},
            requireCleanBaseline:=False)
        Assert.Single(a.Policies)
        Assert.Single(a.Preferences)
        Assert.Equal(2, a.Active.Count)
    End Sub

    <Fact>
    Public Sub Baseline_AbsentPreferences_AreTheNeutralMachine()
        Dim a = BaselineEvaluator.Evaluate(
            {Policy(False), Pref("bRHPSticky", False, Nothing),
             Pref("bExpandRHPInViewer", False, Nothing), Pref("aDefaultRHPViewMode_L", False, Nothing)},
            requireCleanBaseline:=True)
        Assert.Equal(BaselineVerdict.Clean, a.Verdict)
    End Sub

    <Fact>
    Public Sub Baseline_EachActiveValueIsTaggedWithWhereItLives()
        Dim a = BaselineEvaluator.Evaluate({Policy(True), Pref("bRHPSticky", True, 1)},
                                           requireCleanBaseline:=False)
        Assert.Contains("[HKLM politică]", a.Describe())
        Assert.Contains("[HKCU preferință]", a.Describe())
    End Sub

    <Fact>
    Public Sub Baseline_PolicyOnly_DoesNotTalkAboutHkcuCleanup()
        ' Each consequence and each cleanup route is stated only when it actually applies.
        Dim a = BaselineEvaluator.Evaluate({Policy(True)}, requireCleanBaseline:=True)
        Assert.DoesNotContain("șterge", a.BlockedText())
        Assert.Contains("elevare", a.BlockedText())
    End Sub

    ' ══ §3.3 marker file round trip ════════════════════════════════════════════
    <Fact>
    Public Sub Marker_RoundTrips()
        Dim m As New MachineStateMarker() With {
            .PolicyApplied = True,
            .Product = "Adobe Acrobat",
            .AppliedAt = "2026-08-03 12:09:52",
            .RevertRegFile = "C:\x\adobe_policy_revert.reg",
            .PreApply = New List(Of String) From {"HKLM\...\bAcroSuppressUpsell = (absent)"}}

        Dim r = MachineStateMarkerStore.Parse(MachineStateMarkerStore.Serialize(m))

        Assert.Equal(MarkerReadStatus.Present, r.Status)
        Assert.True(r.Marker.PolicyApplied)
        Assert.Equal("Adobe Acrobat", r.Marker.Product)
        Assert.Equal("C:\x\adobe_policy_revert.reg", r.Marker.RevertRegFile)
        Assert.Single(r.Marker.PreApply)
        Assert.True(r.NeedsWarning)
    End Sub

    <Fact>
    Public Sub Marker_CorruptFile_IsUnknownAndWarns_NotAnException()
        ' A file we cannot read may be HIDING an applied policy — it must never read as "clean".
        Dim r = MachineStateMarkerStore.Parse("{ this is not json ")
        Assert.Equal(MarkerReadStatus.Corrupt, r.Status)
        Assert.Null(r.Marker)
        Assert.True(r.NeedsWarning)
        Assert.NotNull(r.[Error])
    End Sub

    <Fact>
    Public Sub Marker_EmptyText_IsNone_AndDoesNotWarn()
        Dim r = MachineStateMarkerStore.Parse("   ")
        Assert.Equal(MarkerReadStatus.None, r.Status)
        Assert.False(r.NeedsWarning)
    End Sub

    <Fact>
    Public Sub Marker_RevertedPolicy_DoesNotWarn()
        Dim m As New MachineStateMarker() With {.PolicyApplied = False}
        Dim r = MachineStateMarkerStore.Parse(MachineStateMarkerStore.Serialize(m))
        Assert.False(r.NeedsWarning)
    End Sub

    ' ══ §5 schema additions ════════════════════════════════════════════════════
    <Fact>
    Public Sub RequireCleanBaseline_DefaultsToFalse_AndParses()
        Assert.False(HarnessScenarioReader.Read("{ ""schema"": 1, ""scenario"": [ ""probe"" ] }").
                     Scenario.RequireCleanBaseline)
        Assert.True(HarnessScenarioReader.Read(
            "{ ""schema"": 1, ""requireCleanBaseline"": true, ""scenario"": [ ""probe"" ] }").
            Scenario.RequireCleanBaseline)
    End Sub

    <Fact>
    Public Sub MachinePolicyRevertOnClose_DefaultsToTrue()
        Dim r = HarnessScenarioReader.Read(
            "{ ""schema"": 1, ""machinePolicy"": { ""product"": ""auto"" }, ""scenario"": [ ""probe"" ] }")
        Assert.True(r.Scenario.MachinePolicy.ShouldRevertOnClose())

        Dim off = HarnessScenarioReader.Read(
            "{ ""schema"": 1, ""machinePolicy"": { ""revertOnClose"": false }, ""scenario"": [ ""probe"" ] }")
        Assert.False(off.Scenario.MachinePolicy.ShouldRevertOnClose())
    End Sub

    <Fact>
    Public Sub NullUserPrefValue_SurvivesTheScenarioReader()
        ' End to end through the real reader, since JSON null is easy to lose in deserialization.
        Dim r = HarnessScenarioReader.Read(
            "{ ""schema"": 1, ""userPrefs"": { ""values"": { ""bRHPSticky"": null } }, ""scenario"": [ ""applyUserPrefs"" ] }")
        Assert.True(r.IsValid)
        Dim intents = UserPrefIntentFactory.FromValues(r.Scenario.UserPrefs.Values)
        Assert.Equal(UserPrefAction.Delete, Assert.Single(intents).Action)
    End Sub

End Class
