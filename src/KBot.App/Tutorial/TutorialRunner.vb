Option Strict On
Imports System.Drawing
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Runs one interactive tutorial (slice 000T). Unlike a guided tour it WAITS for what the user does:
''' the step's target gets a ring, everything else is dimmed (<see cref="TutorialDim"/>), the bubble
''' says what to do, and the tutorial moves on by itself when the user did it (the runner never
''' clicks for them). Anything else on the dimmed window, or a key typed outside the allowed places,
''' asks «Vrei sa iesi din tutorial?». The ring / dim / bubble are built per host window AFTER it is
''' shown, so a modal window (the DDF editor) does not disable them; each window that implements
''' <see cref="IKBotTutorialHost"/> is told when the tutorial walks into it and leaves it.
''' One tutorial at a time. Rules: <c>HelpContent\README.md</c>, «Tutoriale».
''' </summary>
Friend NotInheritable Class TutorialRunner

    Private Shared _active As TutorialRunner

    Private ReadOnly _service As HelpService
    Private ReadOnly _flow As TutorialFlow
    Private ReadOnly _onFinished As Action
    Private ReadOnly _timer As New System.Windows.Forms.Timer With {.Interval = 150}
    Private ReadOnly _detach As New List(Of Action)()
    Private ReadOnly _armed As New HashSet(Of Integer)()
    Private ReadOnly _signals As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _hosts As New List(Of IKBotTutorialHost)()
    Private ReadOnly _filter As KeyFilter

    Private _index As Integer = -1
    Private _finished As Boolean
    Private _asking As Boolean
    Private _closeWatch As Form
    Private ReadOnly _typed As New HashSet(Of Integer)()   ' «changed» steps whose field got text since they were armed
    Private _revisit As Boolean                             ' the step on screen was reached with «Inapoi»
    Private _canBack As Boolean

    Private _dim As TutorialDim
    Private _frame As HelpTourFrame
    Private _bubble As HelpTourBubble
    Private _presenterHost As Form
    Private _layoutKey As String = String.Empty
    Private _shownIndex As Integer = -1
    Private _dimOn As Boolean
    Private _frameOn As Boolean
    Private _holes As New List(Of Rectangle)()

    Private Sub New(k_service As HelpService, k_flow As TutorialFlow, k_onFinished As Action)
        _service = k_service
        _flow = k_flow
        _onFinished = k_onFinished
        _filter = New KeyFilter(Me)
    End Sub

    ''' <summary>
    ''' Starts <paramref name="k_flow"/>; <paramref name="k_onFinished"/> runs when it ends, however it
    ''' ends. Another tutorial running is ended first. A tutorial that starts in a window that is not
    ''' open says so and does not start.
    ''' </summary>
    Public Shared Sub Start(k_service As HelpService, k_flow As TutorialFlow, k_onFinished As Action,
                            Optional k_startIndex As Integer = 0)
        Try
            _active?.Finish()
            If k_flow.Starts.Length > 0 AndAlso HelpTourRunner.FindTarget(k_flow.Starts) Is Nothing Then
                KBotMessage.ShowOnTop("Tutorialul «" & k_flow.Title & "» pornește dintr-o altă fereastră. Deschide fereastra potrivită și încearcă din nou.",
                                      "Tutorial", MessageBoxButtons.OK, MessageBoxIcon.Information)
                k_onFinished?.Invoke()
                Return
            End If
            If k_startIndex < 0 OrElse k_startIndex >= k_flow.Steps.Count Then Throw New ArgumentOutOfRangeException(NameOf(k_startIndex))
            _active = New TutorialRunner(k_service, k_flow, k_onFinished)
            _active.Begin(k_startIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.Start", ex)
            Throw
        End Try
    End Sub

    Private Sub Begin(k_startIndex As Integer)
        AddHandler _timer.Tick, AddressOf OnTick
        Application.AddMessageFilter(_filter)
        _timer.Start()
        If _flow.Starts.Length > 0 Then BeginHostOf(HelpTourRunner.FindTarget(_flow.Starts)?.FindForm())
        If Not _finished Then EnterStep(k_startIndex)
    End Sub

    ' ── steps ────────────────────────────────────────────────────────────────────

    ' Goes to step <k_index>, or to the first one after it that applies (a step whose when: is not met
    ' is skipped), arms what it waits for and puts it on screen.
    Private Sub EnterStep(k_index As Integer, Optional k_revisit As Boolean = False)
        Dim i As Integer = k_index
        DetachAll()
        While i < _flow.Steps.Count AndAlso Not WhenHolds(_flow.Steps(i))
            i += 1
        End While
        If i >= _flow.Steps.Count Then
            Finish()
            Return
        End If
        _index = i
        _revisit = k_revisit
        _canBack = PreviousStep() >= 0
        _shownIndex = -1
        _layoutKey = String.Empty
        _closeWatch = Nothing
        ArmMissing()
        Present()
    End Sub

    ' The action of step <k_j> (the current one, or one the look-ahead accepts) was done.
    Private Sub Done(k_j As Integer)
        If _finished OrElse _asking Then Return
        Dim st As TutorialStep = _flow.Steps(k_j)
        If st.WaitKind = TutorialWaitKind.Opens Then BeginHostOf(HelpTourRunner.FindTarget(st.WaitArg)?.FindForm())
        If _finished Then Return
        EnterStep(k_j + 1)
    End Sub

    ' Event boundary: a control the step watches did something.
    Private Sub OnAction(k_j As Integer)
        Try
            If _finished OrElse _asking OrElse _index < 0 Then Return
            If Not _flow.AcceptedFrom(_index).Contains(k_j) Then Return
            Done(k_j)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.OnAction", ex)
        End Try
    End Sub

    Private Sub OnSignal(k_name As String)
        _signals.Add(k_name)
    End Sub

    ' The bubble's «Sari peste» (optional step) / «Inainte» (manual step).
    Private Sub OnNext()
        Try
            If _finished OrElse _asking OrElse _index < 0 Then Return
            Dim st As TutorialStep = _flow.Steps(_index)
            If st.IsOptional OrElse _revisit OrElse st.WaitKind = TutorialWaitKind.Manual Then Done(_index)
            FocusHost()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.OnNext", ex)
        End Try
    End Sub

    ' The bubble's «Sari la pasul obligatoriu» (shown on an optional step): this step and every optional step
    ' after it are skipped, up to the next mandatory step that applies; none left = the tutorial ends (the
    ' bubble closes, no question).
    Private Sub OnSkipOptional()
        Try
            If _finished OrElse _asking OrElse _index < 0 Then Return
            For j As Integer = _index + 1 To _flow.Steps.Count - 1
                If Not _flow.Steps(j).IsOptional AndAlso WhenHolds(_flow.Steps(j)) Then
                    EnterStep(j)
                    FocusHost()
                    Return
                End If
            Next
            Finish()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.OnSkipOptional", ex)
        End Try
    End Sub

    ' The bubble's «Inapoi»: the step before this one that applies, shown again; what the operator did
    ' since is not undone.
    Private Sub OnBack()
        Try
            If _finished OrElse _asking OrElse _index < 0 Then Return
            Dim previous As Integer = PreviousStep()
            If previous >= 0 Then EnterStep(previous, True)
            FocusHost()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.OnBack", ex)
        End Try
    End Sub

    ' The step «Inapoi» goes to, or -1 when there is none: the nearest earlier step that applies, as long
    ' as it is in the window already shown (one left behind -- the list under the document that opened --
    ' cannot be worked in) and neither it nor this step is only a wait for a window to open.
    Private Function PreviousStep() As Integer
        If _index <= 0 OrElse _flow.Steps(_index).WaitKind = TutorialWaitKind.Opens Then Return -1
        Dim i As Integer = _index - 1
        While i >= 0 AndAlso Not WhenHolds(_flow.Steps(i))
            i -= 1
        End While
        If i < 0 OrElse _flow.Steps(i).WaitKind = TutorialWaitKind.Opens Then Return -1
        Dim here As Form = Nothing
        ResolveHoles(_flow.Steps(_index), here)
        If here Is Nothing Then here = _presenterHost
        Dim there As Form = Nothing
        ResolveHoles(_flow.Steps(i), there)
        If there IsNot Nothing AndAlso here IsNot Nothing AndAlso Not ReferenceEquals(there, here) Then Return -1
        Return i
    End Function

    ' The keyboard belongs to the window the operator works in, never to the bubble: after a click on a
    ' bubble button the focus goes back to that window.
    Private Sub FocusHost()
        If _presenterHost IsNot Nothing AndAlso Not _presenterHost.IsDisposed AndAlso _presenterHost.Visible Then _presenterHost.Activate()
    End Sub

    ' Whether a keyboard message is addressed to a control of the bubble.
    Private Function KeyIsForBubble(k_hwnd As IntPtr) As Boolean
        If _bubble Is Nothing OrElse _bubble.IsDisposed Then Return False
        Dim c As Control = Control.FromChildHandle(k_hwnd)
        Return c IsNot Nothing AndAlso ReferenceEquals(c.FindForm(), _bubble)
    End Function

    ' ── conditions ───────────────────────────────────────────────────────────────

    Private Shared Function ResolveControl(k_step As TutorialStep) As Control
        If k_step.Target.Length = 0 Then Return Nothing
        Return HelpTourRunner.FindTarget(k_step.Target)
    End Function

    Private Shared Function WhenHolds(k_step As TutorialStep) As Boolean
        Select Case k_step.WhenKind
            Case TutorialWhenKind.Always
                Return True
            Case TutorialWhenKind.Visible
                Return ResolveControl(k_step) IsNot Nothing
            Case TutorialWhenKind.Enabled
                Dim c As Control = ResolveControl(k_step)
                Return c IsNot Nothing AndAlso c.Enabled
            Case TutorialWhenKind.Editable
                Dim c As Control = ResolveControl(k_step)
                Return c IsNot Nothing AndAlso c.Enabled AndAlso Not IsReadOnly(c)
            Case Else
                Dim box As CheckBox = TryCast(HelpTourRunner.FindTarget(k_step.WhenArg), CheckBox)
                If box Is Nothing Then Return False
                Return box.Checked = (k_step.WhenKind = TutorialWhenKind.Checked)
        End Select
    End Function

    Private Shared Function IsReadOnly(k_control As Control) As Boolean
        Dim grid As KBotDataView = TryCast(k_control, KBotDataView)
        If grid IsNot Nothing Then Return grid.ReadOnlyGrid
        Dim text As TextBoxBase = TryCast(k_control, TextBoxBase)
        Return text IsNot Nothing AndAlso text.ReadOnly
    End Function

    ' ── what each step waits for ─────────────────────────────────────────────────

    ' Arms, for every step the look-ahead accepts that is not armed yet, whatever it can already
    ' watch; a step whose control is not on screen yet (a page built on its first opening) is tried
    ' again on the next tick.
    Private Sub ArmMissing()
        For Each j As Integer In _flow.AcceptedFrom(_index)
            If _armed.Contains(j) Then Continue For
            If TryArm(j) Then _armed.Add(j)
        Next
    End Sub

    Private Function TryArm(k_j As Integer) As Boolean
        Dim st As TutorialStep = _flow.Steps(k_j)
        Select Case st.WaitKind
            Case TutorialWaitKind.Select
                Dim tree As AdvancedTreeControl = TryCast(ResolveControl(st), AdvancedTreeControl)
                If tree Is Nothing Then Return False
                Dim h As AdvancedTreeControl.NodeMouseUpEventHandler = Sub(n As AdvancedTreeControl.TreeItem, e As MouseEventArgs) OnAction(k_j)
                AddHandler tree.NodeMouseUp, h
                _detach.Add(Sub() RemoveHandler tree.NodeMouseUp, h)
            Case TutorialWaitKind.Click
                Dim c As Control = ResolveControl(st)
                If c Is Nothing Then Return False
                Dim tree As AdvancedTreeControl = TryCast(c, AdvancedTreeControl)
                If tree IsNot Nothing Then
                    ' A tree is clicked all the time: only its row button («+») counts.
                    Dim h As AdvancedTreeControl.RightIconClickedEventHandler = Sub(n As AdvancedTreeControl.TreeItem, e As MouseEventArgs) OnAction(k_j)
                    AddHandler tree.RightIconClicked, h
                    _detach.Add(Sub() RemoveHandler tree.RightIconClicked, h)
                Else
                    Dim h As EventHandler = Sub(s As Object, e As EventArgs) OnAction(k_j)
                    AddHandler c.Click, h
                    _detach.Add(Sub() RemoveHandler c.Click, h)
                End If
            Case TutorialWaitKind.Changed
                Dim c As Control = ResolveControl(st)
                If c Is Nothing Then Return False
                ' A tick box and a drop-down list are done by one gesture: the tick / the chosen row. Text
                ' being typed is NOT: typing only marks the step, and it is done when the operator leaves the
                ' field (PollDone) -- the first letter must not move the tutorial on (slice 000T-05).
                Dim h As EventHandler = Sub(s As Object, e As EventArgs) OnAction(k_j)
                Dim box As CheckBox = TryCast(c, CheckBox)
                Dim combo As KBotComboBox = TryCast(c, KBotComboBox)
                If box IsNot Nothing Then
                    AddHandler box.CheckedChanged, h
                    _detach.Add(Sub() RemoveHandler box.CheckedChanged, h)
                ElseIf combo IsNot Nothing Then
                    AddHandler combo.SelectedIndexChanged, h
                    _detach.Add(Sub() RemoveHandler combo.SelectedIndexChanged, h)
                Else
                    Dim typed As EventHandler = Sub(s As Object, e As EventArgs) _typed.Add(k_j)
                    AddHandler c.TextChanged, typed
                    _detach.Add(Sub() RemoveHandler c.TextChanged, typed)
                End If
            Case Else
                ' Manual, Tab, Checked, Opens, Closes, Signal: watched by state in PollDone.
        End Select
        Return True
    End Function

    Private Sub DetachAll()
        For Each undo As Action In _detach
            Try
                undo()
            Catch ex As Exception
                GlobalErrorLog.Write("TutorialRunner.DetachAll", ex)
            End Try
        Next
        _detach.Clear()
        _armed.Clear()
        _typed.Clear()
    End Sub

    ' Whether the action of step <k_j> is done, judged by the state of the app.
    Private Function PollDone(k_j As Integer) As Boolean
        Dim k_step As TutorialStep = _flow.Steps(k_j)
        ' A step shown again by «Inapoi» is not completed by the state it is already in (the tab is open,
        ' the box is ticked): only a new action or «Inainte» moves on.
        If _revisit AndAlso k_step.WaitKind <> TutorialWaitKind.Changed Then Return False
        Select Case k_step.WaitKind
            Case TutorialWaitKind.Changed
                ' Text typed into the field and the focus has left it (a tick box / a list is done by its event).
                Dim field As Control = ResolveControl(k_step)
                Return _typed.Contains(k_j) AndAlso field IsNot Nothing AndAlso Not field.ContainsFocus
            Case TutorialWaitKind.Tab
                Dim nav As KBotNavList = TryCast(ResolveControl(k_step), KBotNavList)
                Return nav IsNot Nothing AndAlso String.Equals(nav.SelectedKey, k_step.WaitArg, StringComparison.OrdinalIgnoreCase)
            Case TutorialWaitKind.Checked
                Dim box As CheckBox = TryCast(ResolveControl(k_step), CheckBox)
                Return box IsNot Nothing AndAlso box.Checked
            Case TutorialWaitKind.Opens
                Return HelpTourRunner.FindTarget(k_step.WaitArg) IsNot Nothing
            Case TutorialWaitKind.Closes
                If _closeWatch Is Nothing Then _closeWatch = ResolveControl(k_step)?.FindForm()
                Return _closeWatch IsNot Nothing AndAlso (_closeWatch.IsDisposed OrElse Not _closeWatch.Visible)
            Case TutorialWaitKind.Signal
                Return _signals.Remove(k_step.WaitArg)
            Case Else
                Return False
        End Select
    End Function

    Private Sub OnTick(k_sender As Object, k_e As EventArgs)
        Try
            If _finished OrElse _asking OrElse _index < 0 Then Return
            ArmMissing()
            Dim accepted As IReadOnlyList(Of Integer) = _flow.AcceptedFrom(_index)
            For n As Integer = accepted.Count - 1 To 0 Step -1
                If PollDone(accepted(n)) Then
                    Done(accepted(n))
                    Return
                End If
            Next
            If _presenterHost IsNot Nothing AndAlso (_presenterHost.IsDisposed OrElse Not _presenterHost.Visible) Then
                HostClosed()
                Return
            End If
            Present()
            ApplyVisibility()
        Catch ex As Exception
            ' UI boundary (timer): a tutorial that cannot continue ends instead of repeating the failure.
            GlobalErrorLog.Write("TutorialRunner.OnTick", ex)
            Finish()
        End Try
    End Sub

    ' ── what is on screen ────────────────────────────────────────────────────────

    ' Screen rectangle of a control, or of one painted part of it (the whole control when the part
    ' is not on screen or the control has no such part).
    Private Shared Function ControlRect(k_control As Control, k_part As String) As Rectangle
        If k_part.Length > 0 Then
            Dim owner As IKBotHelpParts = TryCast(k_control, IKBotHelpParts)
            If owner IsNot Nothing Then
                Try
                    Dim r As Rectangle = owner.HelpPartBounds(k_part)
                    If Not r.IsEmpty Then Return k_control.RectangleToScreen(r)
                Catch ex As ArgumentException
                    GlobalErrorLog.Write("TutorialRunner.ControlRect", ex)
                End Try
            End If
        End If
        Return k_control.RectangleToScreen(k_control.ClientRectangle)
    End Function

    ' The places of a step on screen (an anchor the window knows, else the control / its part) and
    ' the window they are in.
    Private Shared Function ResolveHoles(k_step As TutorialStep, ByRef k_host As Form) As List(Of Rectangle)
        Dim control As Control = ResolveControl(k_step)
        Dim rects As New List(Of Rectangle)()
        If k_step.Anchor.Length > 0 Then
            Dim hosts As New List(Of IKBotTutorialHost)()
            Dim own As IKBotTutorialHost = TryCast(control?.FindForm(), IKBotTutorialHost)
            If own IsNot Nothing Then hosts.Add(own)
            For Each other As IKBotTutorialHost In Application.OpenForms.OfType(Of IKBotTutorialHost)().ToList()
                If Not hosts.Contains(other) Then hosts.Add(other)
            Next
            For Each h As IKBotTutorialHost In hosts
                Dim found As IReadOnlyList(Of Rectangle) = h.TutorialAnchor(k_step.Anchor)
                If found IsNot Nothing AndAlso found.Count > 0 Then
                    rects.AddRange(found.Where(Function(r) Not r.IsEmpty))
                    k_host = TryCast(h, Form)
                    Exit For
                End If
            Next
        End If
        If rects.Count = 0 AndAlso control IsNot Nothing Then rects.Add(ControlRect(control, k_step.Part))
        If k_host Is Nothing AndAlso control IsNot Nothing Then k_host = control.FindForm()
        Return rects
    End Function

    Private Shared Function CaptionRect(k_host As Form) As Rectangle
        Dim bar As KBotCaptionBar = k_host.Controls.OfType(Of KBotCaptionBar)().FirstOrDefault(Function(b) b.Visible)
        Return If(bar Is Nothing, Rectangle.Empty, k_host.RectangleToScreen(bar.Bounds))
    End Function

    ' Puts the current step on screen: ring, veil, bubble; redone only when the step or the layout changed.
    Private Sub Present()
        Dim st As TutorialStep = _flow.Steps(_index)
        Dim host As Form = Nothing
        Dim primary As List(Of Rectangle) = ResolveHoles(st, host)
        If host Is Nothing Then
            host = If(_presenterHost IsNot Nothing AndAlso Not _presenterHost.IsDisposed AndAlso _presenterHost.Visible, _presenterHost, TryCast(_service.MainWindow(), Form))
        End If
        If host Is Nothing OrElse host.IsDisposed OrElse host.WindowState = FormWindowState.Minimized Then Return
        EnsurePresenter(host)
        If _finished Then Return

        Dim everything As New List(Of Rectangle)(primary)
        For Each j As Integer In _flow.AcceptedFrom(_index)
            If j = _index Then Continue For
            Dim other As Form = Nothing
            Dim more As List(Of Rectangle) = ResolveHoles(_flow.Steps(j), other)
            If other Is Nothing OrElse ReferenceEquals(other, host) Then everything.AddRange(more)
        Next
        For Each a As String In st.Allow
            Dim c As Control = HelpTourRunner.FindTarget(a)
            If c IsNot Nothing Then everything.Add(ControlRect(c, String.Empty))
        Next
        _holes = everything

        Dim key As String = host.Bounds.ToString() & "|" & String.Join(";", primary.Select(Function(r) r.ToString())) & "|" &
                            String.Join(";", everything.Select(Function(r) r.ToString()))
        If key = _layoutKey AndAlso _shownIndex = _index Then Return

        If _shownIndex <> _index Then
            Dim note As String = Nothing
            If st.IsOptional Then note = "Pas opțional: " & st.Why
            If primary.Count = 0 Then
                Dim lost As String = "Ce trebuie să faci nu e pe ecran acum; deschide vederea sau fereastra potrivită ca să îl vezi."
                note = If(note Is Nothing, lost, note & " " & lost)
            End If
            ' A manual step has nothing to watch, so its button is the only way on («Inainte»); any other
            ' optional step moves on by itself when the user does it, and «Sari peste» is the way past it.
            Dim nextText As String
            If st.WaitKind = TutorialWaitKind.Manual OrElse _revisit Then
                nextText = If(_index = _flow.Steps.Count - 1, "Gata", "Înainte ►")
            ElseIf st.IsOptional Then
                nextText = "Sari peste"
            Else
                nextText = String.Empty
            End If
            _bubble.ShowTutorial(_flow.Title, st.Title, st.Text, note, _index, _flow.Steps.Count, nextText, _canBack, st.IsOptional)
        End If

        _dimOn = st.DimRest AndAlso primary.Count > 0
        If _dimOn Then _dim.Cover(host.RectangleToScreen(host.ClientRectangle), everything, CaptionRect(host))
        Dim pointAt As Rectangle = Rectangle.Empty
        If primary.Count > 0 Then
            Dim union As Rectangle = primary(0)
            For Each r As Rectangle In primary
                union = Rectangle.Union(union, r)
            Next
            If primary.Count = 1 Then
                _frame.Surround(union)
                _frameOn = True
                pointAt = _frame.Bounds
            Else
                _frameOn = False
                pointAt = Rectangle.Inflate(union, 4, 4)
            End If
        Else
            _frameOn = False
        End If
        _bubble.PlaceNear(pointAt)
        _layoutKey = key
        _shownIndex = _index
        ApplyVisibility()
        If Not _bubble.Visible Then
            _bubble.Show()
            ' The bubble must not keep the focus the user needs in the window.
            If Not host.IsDisposed Then host.Activate()
        End If
    End Sub

    ' Veil, ring and bubble follow the user: nothing is drawn while another program or a dialog that
    ' is not ours (file chooser) is in front; the veil also steps back while a drop-down list is open.
    Private Sub ApplyVisibility()
        If _bubble Is Nothing OrElse _bubble.IsDisposed Then Return
        Dim active As Form = Form.ActiveForm
        Dim anyOurs As Boolean = active IsNot Nothing
        Dim mine As Boolean = active IsNot Nothing AndAlso (ReferenceEquals(active, _presenterHost) OrElse ReferenceEquals(active, _bubble) OrElse
                                                             ReferenceEquals(active, _dim) OrElse ReferenceEquals(active, _frame))
        SetVisible(_dim, mine AndAlso _dimOn)
        SetVisible(_frame, anyOurs AndAlso _frameOn)
        If _shownIndex >= 0 Then SetVisible(_bubble, anyOurs)
    End Sub

    Private Shared Sub SetVisible(k_form As Form, k_visible As Boolean)
        If k_form IsNot Nothing AndAlso Not k_form.IsDisposed AndAlso k_form.Visible <> k_visible Then k_form.Visible = k_visible
    End Sub

    Private Sub EnsurePresenter(k_host As Form)
        If ReferenceEquals(_presenterHost, k_host) AndAlso _bubble IsNot Nothing AndAlso Not _bubble.IsDisposed Then Return
        ClosePresenter()
        _presenterHost = k_host
        BeginHostOf(k_host)
        If _finished Then Return
        _dim = New TutorialDim()
        _dim.Owner = k_host
        AddHandler _dim.Clicked, AddressOf OnDimClicked
        _frame = New HelpTourFrame()
        _bubble = New HelpTourBubble() With {.NeverActivates = True}
        AddHandler _bubble.NextRequested, AddressOf OnNext
        AddHandler _bubble.BackRequested, AddressOf OnBack
        AddHandler _bubble.SkipOptionalRequested, AddressOf OnSkipOptional
        AddHandler _bubble.CloseRequested, AddressOf OnStopRequested
        _shownIndex = -1
        _layoutKey = String.Empty
    End Sub

    Private Sub ClosePresenter()
        For Each f As Form In New Form() {_dim, _frame}
            If f IsNot Nothing AndAlso Not f.IsDisposed Then f.Close()
        Next
        If _bubble IsNot Nothing AndAlso Not _bubble.IsDisposed Then _bubble.CloseByRunner()
        _dim = Nothing
        _frame = Nothing
        _bubble = Nothing
        _presenterHost = Nothing
        _dimOn = False
        _frameOn = False
    End Sub

    ' ── windows that know the tutorial state ─────────────────────────────────────

    Private Sub BeginHostOf(k_form As Form)
        Dim host As IKBotTutorialHost = TryCast(k_form, IKBotTutorialHost)
        If host Is Nothing OrElse _hosts.Contains(host) Then Return
        If Not host.TutorialSupports(_flow.HostKey) Then
            KBotMessage.ShowOnTop(k_form, "Această fereastră nu poate fi folosită în tutorialul «" & _flow.Title & "».", "Tutorial",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information)
            Finish()
            Return
        End If
        _hosts.Add(host)
        AddHandler host.TutorialSignal, AddressOf OnSignal
        host.TutorialBegin(New KBotTutorialRequest(_flow.Id, _flow.HostKey))
    End Sub

    ' ── leaving ──────────────────────────────────────────────────────────────────

    Private Sub OnDimClicked()
        Ask("Ai făcut altceva decât pasul cerut. Vrei să ieși din tutorial?")
    End Sub

    ' The bubble's «Mă opresc» is a deliberate choice: the tutorial ends at once, no question (slice 000T-05).
    ' Esc / Alt+F4 on the bubble reach here too.
    Private Sub OnStopRequested()
        Finish()
    End Sub

    Private Sub HostClosed()
        ClosePresenter()
        Ask("Ai închis fereastra în care lucrai. Vrei să ieși din tutorial?")
    End Sub

    ' «Da» ends the tutorial; «Nu» goes on at the same step (what the user did is not undone).
    Private Sub Ask(k_message As String)
        If _asking OrElse _finished Then Return
        _asking = True
        Try
            ' Top-most: the step card and the ring are top-most too and would hide an ordinary box.
            Dim answer As DialogResult = KBotMessage.ShowOnTop(_presenterHost, k_message, "Tutorial", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If answer = DialogResult.Yes Then Finish()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.Ask", ex)
            Finish()
        Finally
            _asking = False
        End Try
    End Sub

    Private Sub Finish()
        If _finished Then Return
        _finished = True
        Try
            _timer.Stop()
            RemoveHandler _timer.Tick, AddressOf OnTick
            _timer.Dispose()
            Application.RemoveMessageFilter(_filter)
            DetachAll()
            For Each h As IKBotTutorialHost In _hosts
                Try
                    RemoveHandler h.TutorialSignal, AddressOf OnSignal
                    h.TutorialEnd()
                Catch ex As Exception
                    GlobalErrorLog.Write("TutorialRunner.Finish.TutorialEnd", ex)
                End Try
            Next
            _hosts.Clear()
            ClosePresenter()
            If ReferenceEquals(_active, Me) Then _active = Nothing
            _onFinished?.Invoke()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRunner.Finish", ex)
        End Try
    End Sub

    ' ── keys ─────────────────────────────────────────────────────────────────────

    ' Enter in a single-line text box finishes its «changed» step at once (slice 000T-05), as leaving the field
    ' does. A multi-line box (Enter is a new line there), a tick box and a drop-down list are not concerned.
    Private Sub OnEnterKey(k_hwnd As IntPtr)
        If _finished OrElse _asking OrElse _index < 0 OrElse _presenterHost Is Nothing Then Return
        Dim box As TextBoxBase = TryCast(Control.FromChildHandle(k_hwnd), TextBoxBase)
        If box Is Nothing OrElse box.Multiline OrElse box.ReadOnly Then Return
        For Each j As Integer In _flow.AcceptedFrom(_index)
            If _flow.Steps(j).WaitKind <> TutorialWaitKind.Changed Then Continue For
            Dim target As Control = ResolveControl(_flow.Steps(j))
            If target Is Nothing OrElse TypeOf target Is CheckBox OrElse TypeOf target Is KBotComboBox Then Continue For
            If Not ReferenceEquals(target, box) AndAlso Not target.Contains(box) Then Continue For
            Dim k_j As Integer = j
            ' After the box has handled the key.
            _presenterHost.BeginInvoke(New Action(Sub() OnAction(k_j)))
            Return
        Next
    End Sub

    ' A key typed into a control of the dimmed window that is not where the user may act.
    Private Sub OnKey(k_hwnd As IntPtr, k_key As Keys)
        If _finished OrElse _asking OrElse Not _dimOn OrElse _presenterHost Is Nothing Then Return
        Select Case k_key
            Case Keys.ShiftKey, Keys.ControlKey, Keys.Menu, Keys.Tab, Keys.Escape, Keys.CapsLock, Keys.NumLock,
                 Keys.Scroll, Keys.PrintScreen, Keys.LWin, Keys.RWin, Keys.F1 To Keys.F12
                Return
        End Select
        Dim c As Control = Control.FromChildHandle(k_hwnd)
        If c Is Nothing OrElse Not ReferenceEquals(c.FindForm(), _presenterHost) Then Return   ' popups and dialogs are not policed
        Dim x As Control = c
        Dim steps As Integer = 0
        While x IsNot Nothing AndAlso steps < 2 AndAlso Not ReferenceEquals(x, _presenterHost)
            Dim r As Rectangle = x.RectangleToScreen(x.ClientRectangle)
            If _holes.Any(Function(h) h.IntersectsWith(r)) Then Return
            ' Only the control and, when it is of a similar size, its parent (a combo around its text box).
            If x.Parent Is Nothing OrElse CLng(x.Parent.Width) * x.Parent.Height > 4L * Math.Max(1, x.Width) * Math.Max(1, x.Height) Then Exit While
            x = x.Parent
            steps += 1
        End While
        _presenterHost.BeginInvoke(New Action(Sub() Ask("Ai făcut altceva decât pasul cerut. Vrei să ieși din tutorial?")))
    End Sub

    Private NotInheritable Class KeyFilter
        Implements IMessageFilter

        Private Const WM_KEYFIRST As Integer = &H100   ' WM_KEYDOWN
        Private Const WM_KEYLAST As Integer = &H109    ' WM_UNICHAR
        Private ReadOnly _owner As TutorialRunner

        Public Sub New(k_owner As TutorialRunner)
            _owner = k_owner
        End Sub

        Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
            Try
                If m.Msg >= WM_KEYFIRST AndAlso m.Msg <= WM_KEYLAST Then
                    ' Whatever happened to put the focus on the bubble, a key never reaches it (a Space on
                    ' «Inainte» moved the tutorial on while the operator typed): swallow it and hand the focus back.
                    If _owner.KeyIsForBubble(m.HWnd) Then
                        If m.Msg = WM_KEYFIRST Then _owner._presenterHost?.BeginInvoke(New Action(AddressOf _owner.FocusHost))
                        Return True
                    End If
                    If m.Msg = WM_KEYFIRST Then
                        Dim k_key As Keys = CType(m.WParam.ToInt32(), Keys)
                        If k_key = Keys.Return Then _owner.OnEnterKey(m.HWnd)
                        _owner.OnKey(m.HWnd, k_key)
                    End If
                End If
            Catch ex As Exception
                GlobalErrorLog.Write("TutorialRunner.KeyFilter", ex)
            End Try
            Return False
        End Function

    End Class

End Class
