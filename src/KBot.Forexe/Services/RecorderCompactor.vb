Imports WorkflowModels

''' <summary>
''' Options for <see cref="RecorderCompactor.Compact"/>.
''' </summary>
Public Class CompactorOptions

    ''' <summary>Insert a WaitFor after every step that triggered a Wicket round trip.</summary>
    Public Property InsertWaitForAutomatically As Boolean = False

    ''' <summary>Prepend the interface reset block (mask dismissal + Home).</summary>
    Public Property AddResetBlock As Boolean = False

    ''' <summary>Timeout written on the generated WaitFor actions.</summary>
    Public Property DefaultTimeout As Integer = 10

End Class

''' <summary>
''' Turns the raw recorded trail into a compact list of workflow actions.
''' Pure: no UI, no browser, no global state - the same input always produces the
''' same output.
''' </summary>
''' <remarks>
''' The point of this class is pattern recognition. A select2 selection reaches the
''' recorder as five or more raw events; it must leave as the exact five action block
''' the hand written workflows use, otherwise the generated file will not survive a
''' Wicket rerender.
''' </remarks>
Public NotInheritable Class RecorderCompactor

    Private Const Select2SearchSelector As String = "#select2-drop input.select2-input"
    Private Const Select2ResultSelector As String = "#select2-drop li.select2-result-selectable"
    Private Const Select2LookaheadLimit As Integer = 40

    Private Sub New()
        ' Static only.
    End Sub

    ''' <summary>One generated action plus the step it came from, used by the WaitFor pass.</summary>
    Private NotInheritable Class Emission
        Public Property Action As IWorkflowAction
        Public Property Source As RecordedStep
        Public Property InGeneratedBlock As Boolean
        Public Property LastOfStep As Boolean
    End Class

    ' =========================================================================
    '  Compact
    ' =========================================================================
    Public Shared Function Compact(steps As IEnumerable(Of RecordedStep),
                                   options As CompactorOptions) As List(Of IWorkflowAction)
        If steps Is Nothing Then Throw New ArgumentNullException(NameOf(steps))
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))

        Dim clean As List(Of RecordedStep) = Denoise(steps)
        Dim emissions As New List(Of Emission)

        Dim i As Integer = 0
        While i < clean.Count
            Dim current As RecordedStep = clean(i)

            Select Case True
                Case String.Equals(current.Widget, "select2-open", StringComparison.Ordinal)
                    i = EmitSelect2(clean, i, options, emissions)

                Case String.Equals(current.Widget, "wicket-select", StringComparison.Ordinal) AndAlso
                     String.Equals(current.Kind, "change", StringComparison.Ordinal)
                    EmitWicketSelect(clean, i, emissions)
                    i += 1

                Case IsValueKind(current.Kind)
                    i = EmitFill(clean, i, emissions)

                Case String.Equals(current.Kind, "click", StringComparison.Ordinal)
                    EmitClick(current, emissions)
                    i += 1

                Case String.Equals(current.Kind, "key", StringComparison.Ordinal)
                    EmitKey(current, emissions)
                    i += 1

                Case Else
                    i += 1
            End Select
        End While

        Dim withWaits As List(Of Emission) = InsertAutomaticWaits(emissions, options)

        Dim result As New List(Of IWorkflowAction)
        If options.AddResetBlock Then result.AddRange(BuildResetBlock())
        For Each e As Emission In withWaits
            result.Add(e.Action)
        Next
        Return result
    End Function

    ' =========================================================================
    '  Rule 1 - noise removal
    ' =========================================================================
    Private Shared Function Denoise(steps As IEnumerable(Of RecordedStep)) As List(Of RecordedStep)
        Dim kept As New List(Of RecordedStep)

        For Each s As RecordedStep In steps
            If s Is Nothing OrElse s.Deleted Then Continue For

            ' The select2 overlay is never a step of its own.
            If String.Equals(s.Widget, "select2-mask", StringComparison.Ordinal) Then Continue For

            If String.Equals(s.Kind, "click", StringComparison.Ordinal) Then
                ' Clicks that landed on the page background carry no intent.
                If String.Equals(s.Tag, "body", StringComparison.Ordinal) OrElse
                   String.Equals(s.Tag, "html", StringComparison.Ordinal) Then Continue For
                ' mousedown on a <select> only opens it; the following change carries the value.
                If String.Equals(s.Tag, "select", StringComparison.Ordinal) Then Continue For
            End If

            If IsValueKind(s.Kind) Then
                ' Hidden inputs are written by select2 itself, not by the operator.
                If String.Equals(s.TypeAttr, "hidden", StringComparison.OrdinalIgnoreCase) Then Continue For
                If Not IsFillable(s) Then Continue For

                ' A blur that did not change anything adds nothing over the last input.
                If String.Equals(s.Kind, "blur", StringComparison.Ordinal) AndAlso
                   Not ChangesValue(kept, s) Then Continue For
            End If

            kept.Add(s)
        Next

        Return kept
    End Function

    Private Shared Function IsValueKind(kind As String) As Boolean
        Return String.Equals(kind, "input", StringComparison.Ordinal) OrElse
               String.Equals(kind, "change", StringComparison.Ordinal) OrElse
               String.Equals(kind, "blur", StringComparison.Ordinal)
    End Function

    Private Shared Function IsFillable(s As RecordedStep) As Boolean
        Return String.Equals(s.Tag, "input", StringComparison.Ordinal) OrElse
               String.Equals(s.Tag, "textarea", StringComparison.Ordinal) OrElse
               String.Equals(s.Tag, "select", StringComparison.Ordinal)
    End Function

    Private Shared Function ChangesValue(kept As List(Of RecordedStep), blurStep As RecordedStep) As Boolean
        Dim selector As String = blurStep.ChosenSelector
        For k As Integer = kept.Count - 1 To 0 Step -1
            Dim prev As RecordedStep = kept(k)
            If Not IsValueKind(prev.Kind) Then Continue For
            If Not String.Equals(prev.ChosenSelector, selector, StringComparison.Ordinal) Then Continue For
            Return Not String.Equals(prev.Value, blurStep.Value, StringComparison.Ordinal)
        Next
        Return True
    End Function

    ' =========================================================================
    '  Rules 2 and 3 - select2, with and without a search box
    ' =========================================================================
    Private Shared Function EmitSelect2(list As List(Of RecordedStep),
                                        start As Integer,
                                        options As CompactorOptions,
                                        emissions As List(Of Emission)) As Integer
        Dim openStep As RecordedStep = list(start)
        Dim searchText As String = String.Empty
        Dim pickStep As RecordedStep = Nothing
        Dim pickIndex As Integer = -1

        Dim j As Integer = start + 1
        Dim scanned As Integer = 0
        While j < list.Count AndAlso scanned < Select2LookaheadLimit
            Dim s As RecordedStep = list(j)
            If String.Equals(s.Widget, "select2-open", StringComparison.Ordinal) Then Exit While
            If String.Equals(s.Widget, "select2-search", StringComparison.Ordinal) Then
                If Not String.IsNullOrEmpty(s.Value) Then searchText = s.Value
            ElseIf String.Equals(s.Widget, "select2-pick", StringComparison.Ordinal) Then
                pickStep = s
                pickIndex = j
                Exit While
            End If
            j += 1
            scanned += 1
        End While

        Dim openSelector As String = openStep.ChosenSelector
        Dim label As String = LabelFor(openStep)

        ' No selection followed: keep the click alone rather than invent a block.
        If pickStep Is Nothing Then
            EmitClick(openStep, emissions)
            Return start + 1
        End If

        Dim chosenText As String = If(String.IsNullOrEmpty(pickStep.Text), pickStep.Value, pickStep.Text)

        Add(emissions, openStep, True, New ClickAction With {
            .Selector = openSelector,
            .WaitNavigation = False,
            .LogValue = $"Deschid dropdown-ul {label}"
        })

        If Not String.IsNullOrEmpty(searchText) Then
            ' Rule 2 - select2 WITH a search box. The dropdown must exist before the
            ' search input is filled, otherwise select2 repositions it off screen.
            Add(emissions, openStep, True, New WaitForAction With {
                .Selector = Select2SearchSelector,
                .Timeout = options.DefaultTimeout
            })
            Add(emissions, openStep, True, New FillAction With {
                .Selector = Select2SearchSelector,
                .Value = searchText
            })
            Add(emissions, openStep, True, New WaitForAction With {
                .Selector = Select2ResultSelector,
                .Timeout = options.DefaultTimeout
            })
            Add(emissions, pickStep, True, New ClickAction With {
                .Selector = Select2ResultSelector & ":nth-child(1)",
                .WaitNavigation = False,
                .LogValue = $"Aleg {chosenText}"
            })
        Else
            ' Rule 3 - select2 WITHOUT a search box.
            Add(emissions, openStep, True, New WaitForAction With {
                .Selector = $".select2-results li:has-text('{chosenText}')",
                .Timeout = options.DefaultTimeout
            })
            Add(emissions, pickStep, True, New ClickAction With {
                .Selector = $"#select2-drop ul.select2-results li:has-text('{chosenText}')",
                .WaitNavigation = False,
                .LogValue = $"Aleg {chosenText}"
            })
        End If

        MarkLastOfStep(emissions)
        Return pickIndex + 1
    End Function

    ' =========================================================================
    '  Rule 4 - Wicket <select>, plus the source to indicator wait
    ' =========================================================================
    Private Shared Sub EmitWicketSelect(list As List(Of RecordedStep),
                                        index As Integer,
                                        emissions As List(Of Emission))
        Dim s As RecordedStep = list(index)
        Dim selector As String = PreferEnabledSelector(s)
        If String.IsNullOrEmpty(selector) Then Return

        Add(emissions, s, False, New SelectAction With {
            .Selector = selector,
            .Value = s.Value,
            .LogValue = $"Selectez {s.Value}"
        })

        ' Source to indicator rule: the Wicket container reloads over AJAX after the
        ' source changes. Without this pause the next click lands on the old DOM.
        Dim [next] As RecordedStep = If(index + 1 < list.Count, list(index + 1), Nothing)
        If [next] IsNot Nothing AndAlso
           [next].Widget IsNot Nothing AndAlso
           [next].Widget.StartsWith("select2", StringComparison.Ordinal) Then
            Add(emissions, s, False, New WaitAction With {
                .Seconds = 1,
                .LogValue = "Aștept reîncărcarea containerului după schimbarea sursei"
            })
        End If

        MarkLastOfStep(emissions)
    End Sub

    Private Shared Function PreferEnabledSelector(s As RecordedStep) As String
        If s.Candidates IsNot Nothing Then
            For Each c As SelectorCandidate In s.Candidates
                If c.Selector IsNot Nothing AndAlso
                   c.Selector.Contains(":not([disabled])") Then Return c.Selector
            Next
        End If
        Return s.ChosenSelector
    End Function

    ' =========================================================================
    '  Rule 5 - ordinary Fill, collapsing every keystroke on one element
    ' =========================================================================
    Private Shared Function EmitFill(list As List(Of RecordedStep),
                                     start As Integer,
                                     emissions As List(Of Emission)) As Integer
        Dim selector As String = list(start).ChosenSelector
        If String.IsNullOrEmpty(selector) Then Return start + 1

        Dim lastInput As String = Nothing
        Dim lastSettled As String = Nothing
        Dim owner As RecordedStep = list(start)

        Dim j As Integer = start
        While j < list.Count
            Dim s As RecordedStep = list(j)
            If Not IsValueKind(s.Kind) Then Exit While
            If Not String.Equals(s.ChosenSelector, selector, StringComparison.Ordinal) Then Exit While

            If String.Equals(s.Kind, "input", StringComparison.Ordinal) Then
                lastInput = s.Value
            Else
                lastSettled = s.Value
            End If
            owner = s
            j += 1
        End While

        Dim finalValue As String = If(lastSettled, If(lastInput, String.Empty))

        Add(emissions, owner, False, New FillAction With {
            .Selector = selector,
            .Value = finalValue,
            .Clear = True,
            .LogValue = $"Completez {LabelFor(list(start))}"
        })
        MarkLastOfStep(emissions)

        Return j
    End Function

    ' =========================================================================
    '  Rules 6 and 8 - ordinary click, with the pagination note
    ' =========================================================================
    Private Shared Sub EmitClick(s As RecordedStep, emissions As List(Of Emission))
        Dim selector As String = s.ChosenSelector
        If String.IsNullOrEmpty(selector) Then Return

        If String.Equals(s.Widget, "paginator", StringComparison.Ordinal) Then
            ' Loops stay a human decision - the note says what to look at, nothing more.
            Add(emissions, Nothing, True, New RecorderComment(
                " CANDIDAT BUCLĂ: acest click pare paginare. Ia în calcul " &
                "<While selector=""a[rel='next']"" condition=""Visible""> "))
        End If

        Add(emissions, s, False, New ClickAction With {
            .Selector = selector,
            .WaitNavigation = Not (Not s.TriggeredAjax AndAlso LooksLocal(s)),
            .LogValue = If(String.IsNullOrEmpty(s.LogValue), DescribeClick(s), s.LogValue)
        })
        MarkLastOfStep(emissions)
    End Sub

    Private Shared Function LooksLocal(s As RecordedStep) As Boolean
        If Not String.Equals(s.Widget, "none", StringComparison.Ordinal) Then Return True
        Return Not (String.Equals(s.Tag, "a", StringComparison.Ordinal) OrElse
                    String.Equals(s.Tag, "button", StringComparison.Ordinal) OrElse
                    String.Equals(s.TypeAttr, "submit", StringComparison.OrdinalIgnoreCase))
    End Function

    Private Shared Function DescribeClick(s As RecordedStep) As String
        If Not String.IsNullOrEmpty(s.Text) Then Return $"Apăs {s.Text}"
        If Not String.IsNullOrEmpty(s.NameAttr) Then Return $"Apăs {s.NameAttr}"
        Return $"Click pe {s.Tag}"
    End Function

    ' =========================================================================
    '  Enter and Tab - noted, never guessed into an action
    ' =========================================================================
    Private Shared Sub EmitKey(s As RecordedStep, emissions As List(Of Emission))
        If Not String.Equals(s.KeyName, "Enter", StringComparison.Ordinal) Then Return

        Add(emissions, Nothing, True, New RecorderComment(
            $" TASTA ENTER pe {LabelFor(s)} — dacă declanșa o căutare sau o salvare, " &
            "adaugă manual acțiunea corespunzătoare aici. "))
    End Sub

    ' =========================================================================
    '  Rule 7 - automatic WaitFor after an AJAX step
    ' =========================================================================
    Private Shared Function InsertAutomaticWaits(emissions As List(Of Emission),
                                                 options As CompactorOptions) As List(Of Emission)
        Dim result As New List(Of Emission)

        For k As Integer = 0 To emissions.Count - 1
            Dim current As Emission = emissions(k)
            result.Add(current)

            If current.Source Is Nothing Then Continue For
            If current.InGeneratedBlock Then Continue For       ' rules 2 and 3 wait already
            If Not current.LastOfStep Then Continue For
            If Not current.Source.TriggeredAjax Then Continue For
            If Not (options.InsertWaitForAutomatically OrElse current.Source.InsertWaitFor) Then Continue For

            Dim [next] As Emission = NextRealAction(emissions, k)
            If [next] Is Nothing Then Continue For
            If TypeOf [next].Action Is WaitForAction Then Continue For

            Dim selector As String = SelectorOf([next].Action)
            If String.IsNullOrEmpty(selector) Then Continue For

            result.Add(New Emission With {
                .Action = New WaitForAction With {
                    .Selector = selector,
                    .Timeout = options.DefaultTimeout
                },
                .Source = Nothing,
                .InGeneratedBlock = True,
                .LastOfStep = False
            })
        Next

        Return result
    End Function

    Private Shared Function NextRealAction(emissions As List(Of Emission), after As Integer) As Emission
        For k As Integer = after + 1 To emissions.Count - 1
            If TypeOf emissions(k).Action Is RecorderComment Then Continue For
            Return emissions(k)
        Next
        Return Nothing
    End Function

    ' =========================================================================
    '  Rule 9 - interface reset block
    ' =========================================================================
    Private Shared Function BuildResetBlock() As List(Of IWorkflowAction)
        Dim guard As New IfExistsAction With {
            .Selector = "#select2-drop-mask",
            .Timeout = 1
        }
        guard.Children.Add(New ClickAction With {
            .Selector = "#select2-drop-mask",
            .Force = True,
            .WaitNavigation = False,
            .LogValue = "Resetare interfață — închid dropdown-uri deschise"
        })

        Return New List(Of IWorkflowAction) From {
            guard,
            New ClickAction With {
                .Selector = "#statlogo",
                .WaitNavigation = True,
                .Timeout = 20,
                .LogValue = "Navigare Home via logo"
            }
        }
    End Function

    ' =========================================================================
    '  Small helpers
    ' =========================================================================
    Private Shared Sub Add(emissions As List(Of Emission),
                           source As RecordedStep,
                           inBlock As Boolean,
                           action As IWorkflowAction)
        emissions.Add(New Emission With {
            .Action = action,
            .Source = source,
            .InGeneratedBlock = inBlock,
            .LastOfStep = False
        })
    End Sub

    ''' <summary>Marks the emission just added as the one the WaitFor pass may follow.</summary>
    Private Shared Sub MarkLastOfStep(emissions As List(Of Emission))
        If emissions.Count = 0 Then Return
        emissions(emissions.Count - 1).LastOfStep = True
    End Sub

    Private Shared Function LabelFor(s As RecordedStep) As String
        If Not String.IsNullOrEmpty(s.NameAttr) Then Return s.NameAttr
        If Not String.IsNullOrEmpty(s.Text) Then Return s.Text
        Return s.Tag
    End Function

    Friend Shared Function SelectorOf(action As IWorkflowAction) As String
        Dim clickAction = TryCast(action, ClickAction)
        If clickAction IsNot Nothing Then Return clickAction.Selector

        Dim fillAction = TryCast(action, FillAction)
        If fillAction IsNot Nothing Then Return fillAction.Selector

        Dim selectAction = TryCast(action, SelectAction)
        If selectAction IsNot Nothing Then Return selectAction.Selector

        Dim waitForAction = TryCast(action, WaitForAction)
        If waitForAction IsNot Nothing Then Return waitForAction.Selector

        Dim ifExistsAction = TryCast(action, IfExistsAction)
        If ifExistsAction IsNot Nothing Then Return ifExistsAction.Selector

        Return String.Empty
    End Function

End Class
