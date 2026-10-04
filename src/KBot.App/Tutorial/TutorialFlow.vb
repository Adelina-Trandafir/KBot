Option Strict On
Imports System.ComponentModel
Imports System.IO
Imports System.Text

''' <summary>What a tutorial step waits for (slice 000T). The <c>wait:</c> key of the step.</summary>
Public Enum TutorialWaitKind
    ''' <summary>Nothing to watch: the bubble's «Inainte» moves on (also the default).</summary>
    Manual
    ''' <summary>A row of the target tree is selected.</summary>
    [Select]
    ''' <summary>A tab / nav item (<c>tab:key</c>) of the target nav list becomes the selected one.</summary>
    Tab
    ''' <summary>The target button, or a tree's row icon, is pressed.</summary>
    Click
    ''' <summary>A window of the named type (<c>opens:Type</c>) appears.</summary>
    Opens
    ''' <summary>The window that holds the target closes.</summary>
    Closes
    ''' <summary>The target's text or selection changes.</summary>
    Changed
    ''' <summary>The target check box is ticked.</summary>
    Checked
    ''' <summary>The window raises <c>TutorialSignal</c> with the given name (<c>signal:name</c>).</summary>
    Signal
End Enum

''' <summary>When a step applies (slice 000T). The <c>when:</c> key; a step that does not apply is skipped.</summary>
Public Enum TutorialWhenKind
    Always
    ''' <summary>The target control is enabled.</summary>
    Enabled
    ''' <summary>The target control accepts input (enabled and not read-only).</summary>
    Editable
    ''' <summary>The target control is on screen.</summary>
    Visible
    ''' <summary><c>checked:name</c>: the named check box in the target's window is ticked.</summary>
    Checked
    ''' <summary><c>unchecked:name</c>: the named check box is not ticked.</summary>
    Unchecked
End Enum

''' <summary>One step of an interactive tutorial (slice 000T): what to point at, what to wait for.</summary>
Public NotInheritable Class TutorialStep

    ''' <summary>Romanian heading of the bubble.</summary>
    Public Property Title As String = String.Empty

    ''' <summary><c>TypeName</c> or <c>TypeName.controlName</c> (same names as a tour's <c>target:</c>).</summary>
    Public Property Target As String = String.Empty

    ''' <summary>A painted piece of the target (same names as a tour's <c>part:</c>).</summary>
    Public Property Part As String = String.Empty

    ''' <summary>A named place only the owning window knows (<c>IKBotTutorialHost.TutorialAnchor</c>).</summary>
    Public Property Anchor As String = String.Empty

    Public Property WaitKind As TutorialWaitKind = TutorialWaitKind.Manual
    Public Property WaitArg As String = String.Empty
    Public Property WhenKind As TutorialWhenKind = TutorialWhenKind.Always
    Public Property WhenArg As String = String.Empty

    ''' <summary>The step may be skipped («Sari peste»); needs a <see cref="Why"/>.</summary>
    Public Property IsOptional As Boolean

    ''' <summary>
    ''' The action of the NEXT step also completes this one (selecting the reservation and pressing its
    ''' «+» are one click).
    ''' </summary>
    Public Property Merge As Boolean

    ''' <summary>Romanian: why an optional step is optional.</summary>
    Public Property Why As String = String.Empty

    ''' <summary>True (default) = everything but the target is dimmed; False = only the ring.</summary>
    Public Property DimRest As Boolean = True

    ''' <summary>Extra places the user may use during the step (<c>TypeName.controlName</c>).</summary>
    Public ReadOnly Property Allow As New List(Of String)()

    ''' <summary>The bubble text (the designer edits it in its own box, not in the property grid).</summary>
    <Browsable(False)>
    Public Property Text As String = String.Empty

End Class

''' <summary>
''' An interactive tutorial (slice 000T): a Markdown file under <c>Help\tutorials\</c>. Header block
''' (<c>id</c>, <c>title</c>, <c>part</c>, <c>keywords</c>, <c>starts</c>, <c>host-key</c>), then one
''' <c>## Step title</c> per step whose first lines are keys (<c>target</c>, <c>part</c>,
''' <c>anchor</c>, <c>wait</c>, <c>when</c>, <c>optional</c>, <c>merge</c>, <c>why</c>, <c>dim</c>,
''' <c>allow</c>), then the bubble text. Format: <c>HelpContent\README.md</c>, «Tutoriale».
''' </summary>
Public NotInheritable Class TutorialFlow

    Public Property Id As String = String.Empty
    Public Property Title As String = String.Empty
    Public Property Part As HelpPart = HelpPart.Contabil

    ''' <summary>Words the typed question is matched with besides the title and the step titles.</summary>
    Public Property Keywords As String = String.Empty

    ''' <summary>The window type that must be open to start; empty = any.</summary>
    Public Property Starts As String = String.Empty

    ''' <summary>Handed to every <c>IKBotTutorialHost</c> the tutorial walks into.</summary>
    Public Property HostKey As String = String.Empty

    <Browsable(False)>
    Public ReadOnly Property Steps As New List(Of TutorialStep)()

    <Browsable(False)>
    Public Property SourcePath As String = String.Empty

    Private Shared ReadOnly StepKeys As String() = {"target", "part", "anchor", "wait", "when", "optional", "merge", "why", "dim", "allow"}

    ''' <summary>Reads a tutorial file. Throws <see cref="ArgumentException"/> on a malformed file.</summary>
    Public Shared Function ParseFile(k_file As String) As TutorialFlow
        Dim k_flow As TutorialFlow = Parse(File.ReadAllText(k_file, Encoding.UTF8))
        k_flow.SourcePath = k_file
        Return k_flow
    End Function

    Public Shared Function Parse(k_text As String) As TutorialFlow
        Dim lines As String() = HelpLibrary.StripSourceTags(k_text.Replace(vbCrLf, vbLf)).Split(ChrW(10))
        If lines.Length = 0 OrElse lines(0).Trim() <> "---" Then Throw New ArgumentException("the tutorial does not start with a '---' header block")

        Dim flow As New TutorialFlow()
        Dim i As Integer = 1
        Dim closed As Boolean = False
        While i < lines.Length
            Dim line As String = lines(i).Trim()
            i += 1
            If line = "---" Then
                closed = True
                Exit While
            End If
            If line.Length = 0 OrElse line.StartsWith("#", StringComparison.Ordinal) Then Continue While
            Dim colon As Integer = line.IndexOf(":"c)
            If colon <= 0 Then Throw New ArgumentException("header line without 'key:' -> " & line)
            Dim key As String = line.Substring(0, colon).Trim().ToLowerInvariant()
            Dim value As String = line.Substring(colon + 1).Trim()
            Select Case key
                Case "id" : flow.Id = value
                Case "title" : flow.Title = value
                Case "part" : flow.Part = HelpLibrary.ParsePart(value)
                Case "keywords" : flow.Keywords = value
                Case "starts" : flow.Starts = value
                Case "host-key" : flow.HostKey = value
                Case Else : Throw New ArgumentException("unknown tutorial header key '" & key & "'")
            End Select
        End While
        If Not closed Then Throw New ArgumentException("the tutorial header block is not closed with '---'")
        If flow.Id.Length = 0 OrElse flow.Title.Length = 0 Then Throw New ArgumentException("tutorial header needs 'id' and 'title'")

        Dim current As TutorialStep = Nothing
        Dim body As New StringBuilder()
        Dim inPreamble As Boolean = False
        While i < lines.Length
            Dim raw As String = lines(i)
            i += 1
            If raw.StartsWith("## ", StringComparison.Ordinal) Then
                If current IsNot Nothing Then Finish(current, body)
                current = New TutorialStep With {.Title = raw.Substring(3).Trim()}
                flow.Steps.Add(current)
                body.Clear()
                inPreamble = True
                Continue While
            End If
            If current Is Nothing Then
                If raw.Trim().Length > 0 Then Throw New ArgumentException("text before the first '## ' step")
                Continue While
            End If
            Dim t As String = raw.Trim()
            If inPreamble Then
                Dim key As String = StepKeyOf(t)
                If key IsNot Nothing Then
                    ApplyKey(current, key, t.Substring(key.Length + 1).Trim())
                    Continue While
                End If
                If t.Length = 0 Then Continue While
                inPreamble = False
            End If
            body.AppendLine(raw)
        End While
        If current IsNot Nothing Then Finish(current, body)
        If flow.Steps.Count = 0 Then Throw New ArgumentException("the tutorial has no '## ' steps")
        Return flow
    End Function

    Private Shared Sub Finish(k_step As TutorialStep, k_body As StringBuilder)
        k_step.Text = HelpTour.CleanText(k_body.ToString())
        If k_step.IsOptional AndAlso k_step.Why.Length = 0 Then
            Throw New ArgumentException("the optional step '" & k_step.Title & "' has no 'why:' (the reason it is optional)")
        End If
    End Sub

    Private Shared Function StepKeyOf(k_line As String) As String
        For Each k As String In StepKeys
            If k_line.StartsWith(k & ":", StringComparison.OrdinalIgnoreCase) Then Return k
        Next
        Return Nothing
    End Function

    Private Shared Sub ApplyKey(k_step As TutorialStep, k_key As String, k_value As String)
        Select Case k_key
            Case "target" : k_step.Target = k_value
            Case "part" : k_step.Part = k_value
            Case "anchor" : k_step.Anchor = k_value
            Case "wait" : ParseWait(k_step, k_value)
            Case "when" : ParseWhen(k_step, k_value)
            Case "optional" : k_step.IsOptional = ParseYesNo(k_key, k_value)
            Case "merge" : k_step.Merge = ParseYesNo(k_key, k_value)
            Case "why" : k_step.Why = k_value
            Case "dim"
                If String.Equals(k_value, "ring", StringComparison.OrdinalIgnoreCase) Then
                    k_step.DimRest = False
                Else
                    k_step.DimRest = ParseYesNo(k_key, k_value)
                End If
            Case "allow" : k_step.Allow.AddRange(k_value.Split(","c).Select(Function(s) s.Trim()).Where(Function(s) s.Length > 0))
        End Select
    End Sub

    Private Shared Function ParseYesNo(k_key As String, k_value As String) As Boolean
        Select Case k_value.ToLowerInvariant()
            Case "yes", "true" : Return True
            Case "no", "false" : Return False
            Case Else : Throw New ArgumentException("'" & k_key & ":' takes yes or no, not '" & k_value & "'")
        End Select
    End Function

    Private Shared Sub ParseWait(k_step As TutorialStep, k_value As String)
        Dim colon As Integer = k_value.IndexOf(":"c)
        Dim name As String = If(colon < 0, k_value, k_value.Substring(0, colon)).Trim().ToLowerInvariant()
        Dim arg As String = If(colon < 0, String.Empty, k_value.Substring(colon + 1).Trim())
        Select Case name
            Case "manual" : k_step.WaitKind = TutorialWaitKind.Manual
            Case "select" : k_step.WaitKind = TutorialWaitKind.Select
            Case "click" : k_step.WaitKind = TutorialWaitKind.Click
            Case "closes" : k_step.WaitKind = TutorialWaitKind.Closes
            Case "changed" : k_step.WaitKind = TutorialWaitKind.Changed
            Case "checked" : k_step.WaitKind = TutorialWaitKind.Checked
            Case "tab" : k_step.WaitKind = TutorialWaitKind.Tab
            Case "opens" : k_step.WaitKind = TutorialWaitKind.Opens
            Case "signal" : k_step.WaitKind = TutorialWaitKind.Signal
            Case Else : Throw New ArgumentException("unknown wait kind '" & name & "'")
        End Select
        Dim needsArg As Boolean = k_step.WaitKind = TutorialWaitKind.Tab OrElse k_step.WaitKind = TutorialWaitKind.Opens OrElse k_step.WaitKind = TutorialWaitKind.Signal
        If needsArg AndAlso arg.Length = 0 Then Throw New ArgumentException("'wait: " & name & "' needs a value (" & name & ":value)")
        k_step.WaitArg = arg
    End Sub

    Private Shared Sub ParseWhen(k_step As TutorialStep, k_value As String)
        Dim colon As Integer = k_value.IndexOf(":"c)
        Dim name As String = If(colon < 0, k_value, k_value.Substring(0, colon)).Trim().ToLowerInvariant()
        Dim arg As String = If(colon < 0, String.Empty, k_value.Substring(colon + 1).Trim())
        Select Case name
            Case "always" : k_step.WhenKind = TutorialWhenKind.Always
            Case "enabled" : k_step.WhenKind = TutorialWhenKind.Enabled
            Case "editable" : k_step.WhenKind = TutorialWhenKind.Editable
            Case "visible" : k_step.WhenKind = TutorialWhenKind.Visible
            Case "checked" : k_step.WhenKind = TutorialWhenKind.Checked
            Case "unchecked" : k_step.WhenKind = TutorialWhenKind.Unchecked
            Case Else : Throw New ArgumentException("unknown when kind '" & name & "'")
        End Select
        If (k_step.WhenKind = TutorialWhenKind.Checked OrElse k_step.WhenKind = TutorialWhenKind.Unchecked) AndAlso arg.Length = 0 Then
            Throw New ArgumentException("'when: " & name & "' needs the check box name (" & name & ":name)")
        End If
        k_step.WhenArg = arg
    End Sub

    ''' <summary>
    ''' The steps whose action the user may do while step <paramref name="k_index"/> is on: the step
    ''' itself; after a step that is optional or merged, the next one too, going on past optional /
    ''' merged steps up to and including the first mandatory one. Doing the action of a later step
    ''' of this list completes (or skips) the ones before it.
    ''' </summary>
    Public Function AcceptedFrom(k_index As Integer) As IReadOnlyList(Of Integer)
        If k_index < 0 OrElse k_index >= Steps.Count Then Throw New ArgumentOutOfRangeException(NameOf(k_index))
        Dim accepted As New List(Of Integer) From {k_index}
        Dim i As Integer = k_index
        While i < Steps.Count - 1 AndAlso (Steps(i).IsOptional OrElse Steps(i).Merge)
            i += 1
            accepted.Add(i)
        End While
        Return accepted
    End Function

    ''' <summary>
    ''' The file text of this tutorial, in the shape <see cref="Parse"/> reads (the designer saves with
    ''' it): header, then per step the source tag, the keys that are not at their default, the text.
    ''' Parsing the result gives the same tutorial back.
    ''' </summary>
    Public Function ToMarkdown() As String
        Dim sb As New StringBuilder()
        Const k_tag As String = "<!-- slice: 000T -->"
        sb.Append("---").Append(vbLf)
        sb.Append("id: ").Append(Id).Append(vbLf)
        sb.Append("title: ").Append(Title).Append(vbLf)
        sb.Append("part: ").Append(Part.ToString().ToLowerInvariant()).Append(vbLf)
        If Keywords.Length > 0 Then sb.Append("keywords: ").Append(Keywords).Append(vbLf)
        If Starts.Length > 0 Then sb.Append("starts: ").Append(Starts).Append(vbLf)
        If HostKey.Length > 0 Then sb.Append("host-key: ").Append(HostKey).Append(vbLf)
        sb.Append("---").Append(vbLf).Append(k_tag).Append(vbLf)
        For Each st As TutorialStep In Steps
            sb.Append(vbLf).Append("## ").Append(st.Title).Append(vbLf).Append(k_tag).Append(vbLf)
            If st.Target.Length > 0 Then sb.Append("target: ").Append(st.Target).Append(vbLf)
            If st.Part.Length > 0 Then sb.Append("part: ").Append(st.Part).Append(vbLf)
            If st.Anchor.Length > 0 Then sb.Append("anchor: ").Append(st.Anchor).Append(vbLf)
            If st.WhenKind <> TutorialWhenKind.Always Then sb.Append("when: ").Append(WhenText(st)).Append(vbLf)
            If st.WaitKind <> TutorialWaitKind.Manual Then sb.Append("wait: ").Append(WaitText(st)).Append(vbLf)
            If st.IsOptional Then sb.Append("optional: yes").Append(vbLf)
            If st.Why.Length > 0 Then sb.Append("why: ").Append(st.Why).Append(vbLf)
            If st.Merge Then sb.Append("merge: yes").Append(vbLf)
            If Not st.DimRest Then sb.Append("dim: ring").Append(vbLf)
            If st.Allow.Count > 0 Then sb.Append("allow: ").Append(String.Join(", ", st.Allow)).Append(vbLf)
            sb.Append(st.Text.Replace("• ", "- ")).Append(vbLf)
        Next
        Return sb.ToString()
    End Function

    Private Shared Function WaitText(k_step As TutorialStep) As String
        Dim name As String = k_step.WaitKind.ToString().ToLowerInvariant()
        Return If(k_step.WaitArg.Length > 0, name & ":" & k_step.WaitArg, name)
    End Function

    Private Shared Function WhenText(k_step As TutorialStep) As String
        Dim name As String = k_step.WhenKind.ToString().ToLowerInvariant()
        Return If(k_step.WhenArg.Length > 0, name & ":" & k_step.WhenArg, name)
    End Function

    ''' <summary>The title as stems (see <see cref="HelpSearch"/>); built by <see cref="BuildSearchIndex"/>.</summary>
    Friend Property TitleTerms As HelpTermSet

    ''' <summary>The keywords and the step titles as stems.</summary>
    Friend Property KeywordTerms As HelpTermSet

    ''' <summary>Reduces title, keywords and step titles to stems, once, when the library loads.</summary>
    Friend Sub BuildSearchIndex()
        TitleTerms = New HelpTermSet(HelpSearch.Fold(Title))
        KeywordTerms = New HelpTermSet(HelpSearch.Fold(Keywords & " " & String.Join(" ", Steps.Select(Function(s) s.Title))))
    End Sub

    ''' <summary>
    ''' Folded words for the typed question: title, keywords and the step titles, so «cum adaug o
    ''' revizie» finds the tutorial whichever of them the person echoes.
    ''' </summary>
    Public Function SearchText() As String
        Return Title & " " & Keywords & " " & String.Join(" ", Steps.Select(Function(s) s.Title))
    End Function

End Class
