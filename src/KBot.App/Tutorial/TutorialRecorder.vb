Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Records a tutorial from real use (slice 000T-04): while it runs, every mouse press on a K-BOT
''' control that does something (button, row, tab, tick box...), the first key typed into an input and
''' every window that opens become a step, with the target, the part and the wait that fit. The
''' operator then fills in the texts, optional and when in the designer. Nothing typed is kept (a step
''' says «changed», never the value); K-BOT's own tutorial windows are never recorded.
''' </summary>
Friend NotInheritable Class TutorialRecorder
    Implements IMessageFilter

    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_KEYDOWN As Integer = &H100

    Private ReadOnly _steps As New List(Of TutorialStep)()
    Private ReadOnly _bar As New TutorialRecorderBar()
    Private ReadOnly _timer As New System.Windows.Forms.Timer With {.Interval = 300}
    Private ReadOnly _knownWindows As New HashSet(Of Form)()
    Private ReadOnly _typedInto As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _onDone As Action(Of List(Of TutorialStep))
    Private _running As Boolean

    Private Sub New(k_onDone As Action(Of List(Of TutorialStep)))
        _onDone = k_onDone
    End Sub

    ''' <summary>
    ''' Starts recording; <paramref name="k_onDone"/> gets the recorded steps when the operator presses
    ''' «Opreste» on the bar.
    ''' </summary>
    Public Shared Sub Start(k_onDone As Action(Of List(Of TutorialStep)))
        Try
            Dim recorder As New TutorialRecorder(k_onDone)
            recorder.Begin()
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRecorder.Start", ex)
            Throw
        End Try
    End Sub

    Private Sub Begin()
        For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
            _knownWindows.Add(f)
        Next
        AddHandler _bar.StopRequested, AddressOf OnStop
        AddHandler _timer.Tick, AddressOf OnTick
        Application.AddMessageFilter(Me)
        _bar.Show()
        _timer.Start()
        _running = True
    End Sub

    Private Sub OnStop()
        Finish()
    End Sub

    Private Sub Finish()
        If Not _running Then Return
        _running = False
        Try
            _timer.Stop()
            RemoveHandler _timer.Tick, AddressOf OnTick
            _timer.Dispose()
            Application.RemoveMessageFilter(Me)
            RemoveHandler _bar.StopRequested, AddressOf OnStop
            If Not _bar.IsDisposed Then _bar.Close()
            _onDone?.Invoke(_steps)
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRecorder.Finish", ex)
        End Try
    End Sub

    ' ── what is watched ──────────────────────────────────────────────────────────

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        Try
            If Not _running Then Return False
            If m.Msg = WM_LBUTTONDOWN Then OnPress(m.HWnd)
            If m.Msg = WM_KEYDOWN Then OnKey(m.HWnd, CType(m.WParam.ToInt32(), Keys))
        Catch ex As Exception
            ' UI boundary (message filter): log and swallow; recording goes on.
            GlobalErrorLog.Write("TutorialRecorder.PreFilterMessage", ex)
        End Try
        Return False
    End Function

    Private Function IsOurs(k_control As Control) As Boolean
        Dim f As Form = k_control?.FindForm()
        Return f Is Nothing OrElse TypeOf f Is TutorialRecorderBar OrElse TypeOf f Is TutorialDesignerForm OrElse
               TypeOf f Is TutorialDim OrElse TypeOf f Is HelpTourFrame OrElse TypeOf f Is HelpTourBubble
    End Function

    Private Sub OnPress(k_hwnd As IntPtr)
        Dim hit As Control = Control.FromChildHandle(k_hwnd)
        If hit Is Nothing OrElse IsOurs(hit) Then Return
        Dim skip As New List(Of IntPtr)()
        If _bar.IsHandleCreated Then skip.Add(_bar.Handle)
        Dim pick As TutorialPick = TutorialPicker.ResolveAt(Cursor.Position, skip)
        ' A press that does nothing a tutorial could wait for (a label, a panel) is not a step.
        If pick Is Nothing OrElse Not pick.HasStableName OrElse pick.SuggestedWait = TutorialWaitKind.Manual Then Return
        Add(New TutorialStep With {
            .Title = TitleFor(pick), .Target = pick.Target, .Part = pick.Part,
            .WaitKind = pick.SuggestedWait, .WaitArg = pick.SuggestedWaitArg})
    End Sub

    Private Sub OnKey(k_hwnd As IntPtr, k_key As Keys)
        Select Case k_key
            Case Keys.ShiftKey, Keys.ControlKey, Keys.Menu, Keys.Tab, Keys.Escape, Keys.CapsLock, Keys.NumLock,
                 Keys.Scroll, Keys.PrintScreen, Keys.LWin, Keys.RWin, Keys.F1 To Keys.F12, Keys.Return
                Return
        End Select
        Dim hit As Control = Control.FromChildHandle(k_hwnd)
        If hit Is Nothing OrElse IsOurs(hit) Then Return
        Dim named As Control = TutorialPicker.NearestNamed(hit, hit.FindForm())
        Dim pick As TutorialPick = TutorialPicker.ForControl(named, Cursor.Position, False)
        If Not pick.HasStableName OrElse pick.SuggestedWait <> TutorialWaitKind.Changed Then Return
        ' One step per input, however many keys are typed into it.
        If Not _typedInto.Add(pick.Target) Then Return
        Add(New TutorialStep With {
            .Title = TitleFor(pick), .Target = pick.Target, .WaitKind = TutorialWaitKind.Changed})
    End Sub

    ' New windows: a step «opens:Type». Pop-up lists and menus are not windows the user works in.
    Private Sub OnTick(k_sender As Object, k_e As EventArgs)
        Try
            If Not _running Then Return
            For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
                If Not f.Visible OrElse Not _knownWindows.Add(f) Then Continue For
                If IsOurs(f) OrElse TypeOf f Is TutorialPicker OrElse TypeOf f Is HelpForm Then Continue For
                If Not (f.Modal OrElse f.ShowInTaskbar) Then Continue For
                Add(New TutorialStep With {
                    .Title = "Se deschide " & f.GetType().Name, .WaitKind = TutorialWaitKind.Opens, .WaitArg = f.GetType().Name})
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("TutorialRecorder.OnTick", ex)
        End Try
    End Sub

    Private Sub Add(k_step As TutorialStep)
        ' The same press twice in a row (a double click, a held key) is one step.
        Dim last As TutorialStep = If(_steps.Count > 0, _steps(_steps.Count - 1), Nothing)
        If last IsNot Nothing AndAlso last.Target = k_step.Target AndAlso last.Part = k_step.Part AndAlso
           last.WaitKind = k_step.WaitKind AndAlso last.WaitArg = k_step.WaitArg Then Return
        _steps.Add(k_step)
        _bar.SetCount(_steps.Count)
    End Sub

    ' A working title the operator will rewrite: what was pressed, in the words of the screen.
    Private Shared Function TitleFor(k_pick As TutorialPick) As String
        Dim caption As String = If(k_pick.Control?.Text, String.Empty).Replace("&", String.Empty).Trim()
        If caption.Length > 40 Then caption = caption.Substring(0, 40)
        If k_pick.SuggestedWait = TutorialWaitKind.Tab Then Return "Fila «" & k_pick.SuggestedWaitArg & "»"
        If k_pick.SuggestedWait = TutorialWaitKind.Checked Then Return "Bifează" & If(caption.Length > 0, " «" & caption & "»", String.Empty)
        If k_pick.SuggestedWait = TutorialWaitKind.Changed Then Return "Completează " & k_pick.Target.Substring(k_pick.Target.IndexOf("."c) + 1)
        If k_pick.Control IsNot Nothing AndAlso TypeOf k_pick.Control Is ButtonBase AndAlso caption.Length > 0 Then Return "Apasă «" & caption & "»"
        If k_pick.SuggestedWait = TutorialWaitKind.Select Then Return "Alege un rând din listă"
        If k_pick.SuggestedWait = TutorialWaitKind.Click Then Return "Apasă " & k_pick.Target
        Return k_pick.Target
    End Function

End Class
