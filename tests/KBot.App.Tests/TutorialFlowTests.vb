Imports Xunit

''' <summary>Slice 000T: the tutorial file format and the look-ahead rule. Pure: no windows are created.</summary>
Public Class TutorialFlowTests

    Private Shared Function Source(ParamArray k_lines As String()) As String
        Return String.Join(vbLf, k_lines)
    End Function

    Private Shared ReadOnly Head As String() = {"---", "id: demo", "title: Demo", "part: contabil", "host-key: demo-key", "---"}

    Private Shared Function Flow(ParamArray k_steps As String()) As TutorialFlow
        Return TutorialFlow.Parse(Source(Head) & vbLf & Source(k_steps))
    End Function

    <Fact>
    Public Sub Parse_ReadsHeaderAndStepKeys()
        Dim k_flow As TutorialFlow = Flow(
            "<!-- slice: 000T -->",
            "## Pick one",
            "<!-- slice: 000T -->",
            "target: KbotForm.tree",
            "anchor: tree.rows",
            "wait: select",
            "dim: ring",
            "allow: KbotForm.navViews, KbotForm.btnMeniu",
            "Select a row.",
            "## Open the tab",
            "target: KbotForm.navViews",
            "wait: tab:rezervari",
            "merge: yes",
            "Press the tab.")
        Assert.Equal("demo", k_flow.Id)
        Assert.Equal("demo-key", k_flow.HostKey)
        Assert.Equal(2, k_flow.Steps.Count)
        Dim k_first As TutorialStep = k_flow.Steps(0)
        Assert.Equal("Pick one", k_first.Title)
        Assert.Equal("KbotForm.tree", k_first.Target)
        Assert.Equal("tree.rows", k_first.Anchor)
        Assert.Equal(TutorialWaitKind.Select, k_first.WaitKind)
        Assert.False(k_first.DimRest)
        Assert.Equal(2, k_first.Allow.Count)
        Assert.Equal("Select a row.", k_first.Text)
        Dim k_second As TutorialStep = k_flow.Steps(1)
        Assert.Equal(TutorialWaitKind.Tab, k_second.WaitKind)
        Assert.Equal("rezervari", k_second.WaitArg)
        Assert.True(k_second.Merge)
        Assert.True(k_second.DimRest)
    End Sub

    <Fact>
    Public Sub Parse_WhenCheckedKeepsTheTargetOfTheCheckBox()
        Dim k_flow As TutorialFlow = Flow("## Pick", "target: DdfEditForm.tabs", "when: checked:DdfEditForm.chkPartAng", "wait: manual", "Text.")
        Assert.Equal(TutorialWhenKind.Checked, k_flow.Steps(0).WhenKind)
        Assert.Equal("DdfEditForm.chkPartAng", k_flow.Steps(0).WhenArg)
    End Sub

    <Fact>
    Public Sub Parse_OptionalStepWithoutWhyIsRefused()
        Dim k_ex As ArgumentException = Assert.Throws(Of ArgumentException)(Function() Flow("## Maybe", "optional: yes", "Text."))
        Assert.Contains("why", k_ex.Message)
    End Sub

    <Fact>
    Public Sub Parse_OptionalStepWithWhyIsAccepted()
        Dim k_flow As TutorialFlow = Flow("## Maybe", "optional: yes", "why: Only with a partner.", "Text.")
        Assert.True(k_flow.Steps(0).IsOptional)
        Assert.Equal("Only with a partner.", k_flow.Steps(0).Why)
    End Sub

    <Theory>
    <InlineData("wait: tab")>
    <InlineData("wait: opens:")>
    <InlineData("wait: flying")>
    <InlineData("when: checked")>
    <InlineData("when: sometimes")>
    <InlineData("optional: maybe")>
    Public Sub Parse_RefusesBadStepKeys(k_line As String)
        Assert.Throws(Of ArgumentException)(Function() Flow("## Step", k_line, "Text."))
    End Sub

    <Fact>
    Public Sub Parse_RefusesUnknownHeaderKey()
        Assert.Throws(Of ArgumentException)(Function() TutorialFlow.Parse("---" & vbLf & "id: a" & vbLf & "title: A" & vbLf & "colour: red" & vbLf & "---" & vbLf & "## S" & vbLf & "Text."))
    End Sub

    <Fact>
    Public Sub Parse_RefusesAFileWithoutSteps()
        Assert.Throws(Of ArgumentException)(Function() TutorialFlow.Parse(Source(Head)))
    End Sub

    <Fact>
    Public Sub AcceptedFrom_MandatoryStepAcceptsOnlyItself()
        Dim k_flow As TutorialFlow = Flow("## A", "Text.", "## B", "Text.")
        Assert.Equal({0}, k_flow.AcceptedFrom(0).ToArray())
    End Sub

    <Fact>
    Public Sub AcceptedFrom_OptionalStepAcceptsOnlyTheNextOne()
        Dim k_flow As TutorialFlow = Flow(
            "## A", "Text.",
            "## O1", "optional: yes", "why: Reason.", "Text.",
            "## O2", "optional: yes", "why: Reason.", "Text.",
            "## M", "Text.",
            "## Z", "Text.")
        Assert.Equal({0}, k_flow.AcceptedFrom(0).ToArray())
        Assert.Equal({1, 2}, k_flow.AcceptedFrom(1).ToArray())
        Assert.Equal({2, 3}, k_flow.AcceptedFrom(2).ToArray())
        Assert.Equal({3}, k_flow.AcceptedFrom(3).ToArray())
    End Sub

    <Fact>
    Public Sub AcceptedFrom_MergedStepAlsoAcceptsTheNextOne()
        Dim k_flow As TutorialFlow = Flow("## Select", "merge: yes", "Text.", "## Plus", "Text.", "## Open", "Text.")
        Assert.Equal({0, 1}, k_flow.AcceptedFrom(0).ToArray())
        Assert.Equal({1}, k_flow.AcceptedFrom(1).ToArray())
    End Sub

    <Fact>
    Public Sub AcceptedFrom_LastStepNeverRunsPastTheEnd()
        Dim k_flow As TutorialFlow = Flow("## Last", "optional: yes", "why: Reason.", "Text.")
        Assert.Equal({0}, k_flow.AcceptedFrom(0).ToArray())
        Assert.Throws(Of ArgumentOutOfRangeException)(Function() k_flow.AcceptedFrom(1))
    End Sub

    <Fact>
    Public Sub ToMarkdown_ParsesBackToTheSameTutorial()
        Dim k_original As TutorialFlow = TutorialFlow.Parse(
            "---" & vbLf & "id: round" & vbLf & "title: Round trip" & vbLf & "part: avansat" & vbLf & "keywords: a b" & vbLf &
            "starts: KbotForm" & vbLf & "host-key: k" & vbLf & "---" & vbLf & "<!-- slice: 000T -->" & vbLf &
            "## One" & vbLf & "<!-- slice: 000T -->" & vbLf & "target: KbotForm.tree" & vbLf & "part: footer.left" & vbLf &
            "anchor: tree.x" & vbLf & "when: checked:KbotForm.chk" & vbLf & "wait: tab:abc" & vbLf & "optional: yes" & vbLf &
            "why: Because." & vbLf & "merge: yes" & vbLf & "dim: ring" & vbLf & "allow: A.b, C.d" & vbLf &
            "First paragraph." & vbLf & vbLf & "- bullet one" & vbLf & "- bullet two" & vbLf &
            "## Two" & vbLf & "<!-- slice: 000T -->" & vbLf & "wait: opens:DdfEditForm" & vbLf & "Open it.")
        Dim k_text As String = k_original.ToMarkdown()
        Dim k_again As TutorialFlow = TutorialFlow.Parse(k_text)
        Assert.Equal(k_text, k_again.ToMarkdown())
        Assert.Equal(HelpPart.Avansat, k_again.Part)
        Assert.Equal(2, k_again.Steps.Count)
        Dim k_one As TutorialStep = k_again.Steps(0)
        Assert.Equal("footer.left", k_one.Part)
        Assert.Equal(TutorialWhenKind.Checked, k_one.WhenKind)
        Assert.Equal("KbotForm.chk", k_one.WhenArg)
        Assert.Equal(TutorialWaitKind.Tab, k_one.WaitKind)
        Assert.True(k_one.IsOptional AndAlso k_one.Merge AndAlso Not k_one.DimRest)
        Assert.Equal(2, k_one.Allow.Count)
        Assert.Equal(k_original.Steps(0).Text, k_one.Text)
        Assert.Equal(TutorialWaitKind.Opens, k_again.Steps(1).WaitKind)
    End Sub

    <Fact>
    Public Sub SearchText_CarriesTitleKeywordsAndStepTitles()
        Dim k_flow As TutorialFlow = TutorialFlow.Parse("---" & vbLf & "id: a" & vbLf & "title: Add a revision" & vbLf & "keywords: reservation plus" & vbLf & "---" & vbLf & "## Pick a row" & vbLf & "Text.")
        Dim k_text As String = k_flow.SearchText()
        Assert.Contains("Add a revision", k_text)
        Assert.Contains("reservation", k_text)
        Assert.Contains("Pick a row", k_text)
    End Sub

End Class
