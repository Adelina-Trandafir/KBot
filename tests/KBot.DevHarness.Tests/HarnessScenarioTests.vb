Option Strict On
Imports System.Text.Json
Imports KBot.DevHarness
Imports Xunit

' Slice 0023 (config+layout pass), plan §5. Pure: no registry, no windows, no Adobe.
' The distinction these tests exist to protect: an ABSENT section means "leave that alone" and
' must stay distinguishable from a present section that switches something off.
Public Class HarnessScenarioTests

    ' The full schema-1 example, used as the round-trip fixture. NOTE: no `document` section —
    ' a scenario carries settings only; the PDF always comes from «Deschide PDF…».
    Private Const FullJson As String = "{
  ""schema"": 1,
  ""name"": ""RHP: collapse then hide"",
  ""note"": ""Free text, shown in the status line when loaded."",
  ""launch"": { ""newInstance"": true, ""noSplash"": true },
  ""openParameters"": {
    ""toolbar"": 0, ""navpanes"": 0, ""statusbar"": 0,
    ""messages"": 0, ""scrollbar"": 0, ""pagemode"": ""none""
  },
  ""clip"": { ""enabled"": false, ""right"": 0, ""top"": 0 },
  ""hideChildren"": {
    ""byText"": [""AVTaskPaneHostView""],
    ""reapplyOnRelaunch"": true,
    ""reapplyAttempts"": 10,
    ""reapplyIntervalMs"": 400
  },
  ""keys"": [ { ""send"": ""Shift+F4"", ""delayMsBefore"": 600 } ],
  ""userPrefs"": {
    ""hive"": ""auto"",
    ""values"": {
      ""bExpandRHPInViewer"": 0,
      ""bRHPSticky"": 1,
      ""aDefaultRHPViewMode_L"": ""Collapsed"",
      ""bEnableAv2"": 0
    },
    ""restoreOnClose"": true
  },
  ""machinePolicy"": {
    ""product"": ""auto"",
    ""apply"": false,
    ""values"": {
      ""bAcroSuppressUpsell"": 1,
      ""cServices\\bToggleAdobeDocumentServices"": 1
    }
  },
  ""scenario"": [
    ""applyUserPrefs"", ""launch"", ""waitForEmbed"", ""sendKeys"",
    ""probe"", ""hideChildren"", ""applyClip"", ""probe""
  ]
}"

    <Fact>
    Public Sub FullFile_ParsesEveryField()
        Dim r = HarnessScenarioReader.Read(FullJson)
        Assert.True(r.IsValid)
        Dim s = r.Scenario
        Assert.Equal(1, s.Schema)
        Assert.Equal("RHP: collapse then hide", s.Name)
        Assert.True(s.Launch.NewInstance.Value)
        Assert.True(s.Launch.NoSplash.Value)
        Assert.Equal(0, s.OpenParameters.Toolbar.Value)
        Assert.Equal("none", s.OpenParameters.Pagemode)
        Assert.False(s.Clip.Enabled.Value)
        Assert.Equal("AVTaskPaneHostView", s.HideChildren.ByText(0))
        Assert.True(s.HideChildren.ReapplyOnRelaunch.Value)
        Assert.Equal(10, s.HideChildren.EffectiveAttempts())
        Assert.Equal(400, s.HideChildren.EffectiveIntervalMs())
        Assert.Equal("Shift+F4", s.Keys(0).Send)
        Assert.Equal(600, s.Keys(0).DelayMsBefore.Value)
        Assert.Equal("auto", s.UserPrefs.Hive)
        Assert.Equal(4, s.UserPrefs.Values.Count)
        Assert.Equal("Collapsed", s.UserPrefs.Values("aDefaultRHPViewMode_L").GetString())
        Assert.False(s.MachinePolicy.Apply)
        Assert.True(s.MachinePolicy.Values.ContainsKey("cServices\bToggleAdobeDocumentServices"))
        Assert.Equal(8, s.Scenario.Count)
    End Sub

    <Fact>
    Public Sub RoundTrip_SerializeDeserializeSerialize_IsUnchanged()
        Dim first = HarnessScenarioReader.Read(FullJson)
        Assert.True(first.IsValid)

        Dim written As String = HarnessScenarioWriter.Write(first.Scenario)
        Dim second = HarnessScenarioReader.Read(written)
        Assert.True(second.IsValid)
        Dim writtenAgain As String = HarnessScenarioWriter.Write(second.Scenario)

        Assert.Equal(written, writtenAgain)
    End Sub

    <Fact>
    Public Sub Schema_HasNoDocumentSection_ScenariosCarrySettingsOnly()
        ' The PDF is always chosen with «Deschide PDF…»; a path baked into a scenario would be
        ' meaningless on anyone else's machine. Pinned so it cannot creep back in.
        Assert.Null(GetType(HarnessScenario).GetProperty("Document"))
        Dim s As New HarnessScenario() With {
            .Schema = 1,
            .Scenario = New List(Of String) From {HarnessScenarioSteps.Launch}}
        Assert.DoesNotContain("document", HarnessScenarioWriter.Write(s), StringComparison.OrdinalIgnoreCase)
    End Sub

    <Fact>
    Public Sub OlderFileWithADocumentSection_StillLoads_WithAWarning()
        ' Migration: files written against the earlier schema must not become unusable.
        Dim json As String = "{ ""schema"": 1, ""document"": { ""path"": ""C:\\x.pdf"" }, ""scenario"": [ ""launch"" ] }"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid)
        Assert.Contains(r.Warnings, Function(w) w.Contains("document"))
    End Sub

    <Fact>
    Public Sub AbsentSection_IsNothing_NotADisabledSection()
        ' No `clip` object at all: the clip must be left untouched.
        Dim json As String = "{ ""schema"": 1, ""scenario"": [ ""probe"" ] }"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid)
        Assert.Null(r.Scenario.Clip)
        Assert.Null(r.Scenario.Launch)
        Assert.Null(r.Scenario.HideChildren)
        Assert.Null(r.Scenario.UserPrefs)
        Assert.Null(r.Scenario.MachinePolicy)
    End Sub

    <Fact>
    Public Sub PresentButDisabledSection_IsDistinguishableFromAbsent()
        Dim json As String = "{ ""schema"": 1, ""clip"": { ""enabled"": false }, ""scenario"": [ ""applyClip"" ] }"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid)
        Assert.NotNull(r.Scenario.Clip)          ' the section exists…
        Assert.True(r.Scenario.Clip.Enabled.HasValue)
        Assert.False(r.Scenario.Clip.Enabled.Value)  ' …and explicitly turns clipping OFF
        ' Scalars not mentioned stay unset, so they too mean "leave alone".
        Assert.False(r.Scenario.Clip.Right.HasValue)
        Assert.False(r.Scenario.Clip.Top.HasValue)
    End Sub

    <Fact>
    Public Sub UnknownProperty_IsAWarning_NotAnException()
        Dim json As String = "{ ""schema"": 1, ""wat"": 5, ""clip"": { ""enabled"": true, ""nope"": 1 }, ""scenario"": [ ""probe"" ] }"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid)
        Assert.Contains(r.Warnings, Function(w) w.Contains("wat"))
        Assert.Contains(r.Warnings, Function(w) w.Contains("clip.nope"))
    End Sub

    <Fact>
    Public Sub UnknownStep_IsInvalid_AndNamesTheOffender()
        Dim json As String = "{ ""schema"": 1, ""scenario"": [ ""probe"", ""hideChldren"" ] }"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.False(r.IsValid)
        Assert.Contains(r.Errors, Function(e) e.Contains("hideChldren"))
        ' The error also lists the valid names, so a typo is fixable without the docs.
        Assert.Contains(r.Errors, Function(e) e.Contains("hideChildren"))
    End Sub

    <Fact>
    Public Sub MachinePolicyApply_AbsentMeansFalse()
        Dim json As String = "{ ""schema"": 1, ""machinePolicy"": { ""product"": ""auto"" }, ""scenario"": [ ""applyMachinePolicy"" ] }"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid)
        Assert.False(r.Scenario.MachinePolicy.Apply)
    End Sub

    <Fact>
    Public Sub MalformedJson_IsInvalid_WithRomanianError_NoException()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""scenario"": [ ")
        Assert.False(r.IsValid)
        Assert.NotEmpty(r.Errors)
        Assert.Contains("JSON invalid", r.Errors(0))
    End Sub

    <Fact>
    Public Sub EmptyText_IsInvalid_NotAnException()
        Dim r = HarnessScenarioReader.Read("   ")
        Assert.False(r.IsValid)
        Assert.NotEmpty(r.Errors)
    End Sub

    <Fact>
    Public Sub CommentsAndTrailingCommas_AreAccepted()
        ' These files are written by hand, with comments in them.
        Dim json As String = "{
  // scenariul meu
  ""schema"": 1,
  ""scenario"": [ ""probe"", ],
}"
        Dim r = HarnessScenarioReader.Read(json)
        Assert.True(r.IsValid)
        Assert.Single(r.Scenario.Scenario)
    End Sub

    <Fact>
    Public Sub UnsupportedSchemaVersion_IsInvalid()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 99, ""scenario"": [ ""probe"" ] }")
        Assert.False(r.IsValid)
        Assert.Contains(r.Errors, Function(e) e.Contains("99"))
    End Sub

    <Fact>
    Public Sub MissingSchema_WarnsAndAssumesVersion1()
        Dim r = HarnessScenarioReader.Read("{ ""scenario"": [ ""probe"" ] }")
        Assert.True(r.IsValid)
        Assert.Equal(HarnessScenarioReader.SupportedSchema, r.Scenario.Schema)
        Assert.Contains(r.Warnings, Function(w) w.Contains("schema"))
    End Sub

    <Fact>
    Public Sub EmptyStepList_WarnsButStaysValid()
        Dim r = HarnessScenarioReader.Read("{ ""schema"": 1, ""scenario"": [] }")
        Assert.True(r.IsValid)
        Assert.Contains(r.Warnings, Function(w) w.Contains("scenario"))
    End Sub

    <Fact>
    Public Sub AllStepNames_AreRecognised()
        For Each s As String In HarnessScenarioSteps.All
            Assert.True(HarnessScenarioSteps.IsKnown(s), s & " ar trebui recunoscut")
        Next
        Assert.False(HarnessScenarioSteps.IsKnown("nope"))
        Assert.False(HarnessScenarioSteps.IsKnown(""))
        Assert.False(HarnessScenarioSteps.IsKnown(Nothing))
    End Sub

    <Fact>
    Public Sub HideChildrenDefaults_AppliedOnlyWhenAbsentOrNonPositive()
        Dim cfg As New HideChildrenConfig()
        Assert.Equal(HideChildrenConfig.DefaultReapplyAttempts, cfg.EffectiveAttempts())
        Assert.Equal(HideChildrenConfig.DefaultReapplyIntervalMs, cfg.EffectiveIntervalMs())

        cfg.ReapplyAttempts = 0
        cfg.ReapplyIntervalMs = -5
        Assert.Equal(HideChildrenConfig.DefaultReapplyAttempts, cfg.EffectiveAttempts())
        Assert.Equal(HideChildrenConfig.DefaultReapplyIntervalMs, cfg.EffectiveIntervalMs())

        cfg.ReapplyAttempts = 3
        cfg.ReapplyIntervalMs = 250
        Assert.Equal(3, cfg.EffectiveAttempts())
        Assert.Equal(250, cfg.EffectiveIntervalMs())
    End Sub

    <Fact>
    Public Sub Writer_OmitsAbsentSections_SoASavedFileKeepsMeaning()
        Dim s As New HarnessScenario() With {
            .Schema = 1,
            .Scenario = New List(Of String) From {HarnessScenarioSteps.Probe}}
        Dim json As String = HarnessScenarioWriter.Write(s)
        Assert.DoesNotContain("""clip""", json)
        Assert.DoesNotContain("""userPrefs""", json)
        Assert.Contains("""schema"": 1", json)
    End Sub

    <Fact>
    Public Sub Writer_IsIndented_SoSavedFilesAreDiffable()
        Dim s As New HarnessScenario() With {
            .Schema = 1,
            .Scenario = New List(Of String) From {HarnessScenarioSteps.Probe}}
        Assert.Contains(Environment.NewLine, HarnessScenarioWriter.Write(s))
    End Sub

End Class
