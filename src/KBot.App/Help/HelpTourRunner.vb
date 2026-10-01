Option Strict On
Imports System.Drawing
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' Runs one guided tour (slice 0000-04): for each step it opens the step's screen
''' (<see cref="HelpService.Navigate"/>), finds the control the step names among the open windows,
''' rings it (<see cref="HelpTourFrame"/>) and puts the bubble next to it. One tour at a time:
''' starting another ends the running one.
''' </summary>
Friend NotInheritable Class HelpTourRunner

    Private Shared _active As HelpTourRunner

    Private ReadOnly _service As HelpService
    Private ReadOnly _tour As HelpTour
    Private ReadOnly _onFinished As Action
    Private ReadOnly _bubble As New HelpTourBubble()
    Private ReadOnly _frame As New HelpTourFrame()
    Private _index As Integer
    Private _finished As Boolean
    Private _busy As Boolean
    ' Slice 0000-23: the part the current step made visible, put back when the step changes.
    Private _demoOwner As IKBotHelpParts
    Private _demoPart As String

    Private Sub New(service As HelpService, tour As HelpTour, onFinished As Action)
        _service = service
        _tour = tour
        _onFinished = onFinished
        AddHandler _bubble.NextRequested, AddressOf OnNext
        AddHandler _bubble.BackRequested, AddressOf OnBack
        AddHandler _bubble.CloseRequested, AddressOf Finish
    End Sub

    ''' <summary>Starts <paramref name="tour"/>; <paramref name="onFinished"/> runs when it ends, however it ends.</summary>
    Public Shared Sub Start(service As HelpService, tour As HelpTour, onFinished As Action)
        Try
            _active?.Finish()
            _active = New HelpTourRunner(service, tour, onFinished)
            _active.ShowStep(0, 1)
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourRunner.Start", ex)
            Throw
        End Try
    End Sub

    Private Sub OnNext()
        If _busy Then Return
        If _index >= _tour.Steps.Count - 1 Then
            Finish()
        Else
            ShowStep(_index + 1, 1)
        End If
    End Sub

    Private Sub OnBack()
        If _busy OrElse _index = 0 Then Return
        ShowStep(_index - 1, -1)
    End Sub

    ' UI boundary (async void, from bubble buttons): log and swallow; the tour ends on a failure.
    ' Slice 0000-23: a step about one button of a control (part:) that is not on screen now is
    ' skipped in the direction of travel (the button does not exist in this view, or the list is
    ' empty); the bubble is a callout whose point touches the ring.
    Private Async Sub ShowStep(index As Integer, direction As Integer)
        Try
            _busy = True
            Do
                EndDemo()
                _index = index
                Dim [step] As HelpTourStep = _tour.Steps(index)
                Dim note As String = _service.Navigate([step].GoToTarget, _tour.TopicId)
                ' A view is built lazily on its first opening: give the layout a moment before measuring.
                Await Task.Delay(If([step].GoToTarget.Length > 0, 400, 60)).ConfigureAwait(True)
                If _finished Then Return

                Dim rect As Rectangle = Rectangle.Empty
                If [step].Target.Length > 0 Then
                    Dim target As Control = FindTarget([step].Target)
                    If target Is Nothing Then
                        If note Is Nothing Then
                            note = "Partea despre care e vorba nu e pe ecran acum; deschide fereastra sau vederea potrivită ca s-o vezi."
                        End If
                    Else
                        If [step].Part.Length > 0 Then
                            Dim partMissing As Boolean
                            Dim piece As Rectangle = PartBounds(target, [step].Part, note, partMissing)
                            If partMissing Then
                                Dim nextIndex As Integer = index + direction
                                If nextIndex >= _tour.Steps.Count Then
                                    Finish()
                                    Return
                                End If
                                If nextIndex < 0 Then
                                    direction = 1
                                    nextIndex = index + 1
                                End If
                                index = nextIndex
                                Continue Do
                            End If
                            If Not piece.IsEmpty Then rect = target.RectangleToScreen(piece)
                        End If
                        If rect.IsEmpty Then rect = target.RectangleToScreen(target.ClientRectangle)
                    End If
                End If

                _bubble.ShowStep(_tour.Title, [step].Title, [step].Text, note, index, _tour.Steps.Count)
                Dim pointAt As Rectangle = Rectangle.Empty
                If Not rect.IsEmpty Then
                    _frame.Surround(rect)
                    pointAt = _frame.Bounds
                ElseIf _frame.Visible Then
                    _frame.Hide()
                End If
                _bubble.PlaceNear(pointAt)
                If Not _bubble.Visible Then _bubble.Show()
                _bubble.Activate()
                Exit Do
            Loop
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourRunner.ShowStep", ex)
            Finish()
        Finally
            _busy = False
        End Try
    End Sub

    ''' <summary>
    ''' Slice 0000-23: where <paramref name="part"/> of <paramref name="target"/> is (client
    ''' coordinates), after asking the control to show it if it normally waits for the mouse.
    ''' <paramref name="missing"/> = the part exists but is not on screen now (the step is skipped).
    ''' A control that has no such part is a mistake in the tour file: logged, the whole control
    ''' is shown with a note.
    ''' </summary>
    Private Function PartBounds(target As Control, part As String, ByRef note As String, ByRef missing As Boolean) As Rectangle
        missing = False
        Dim owner As IKBotHelpParts = TryCast(target, IKBotHelpParts)
        Try
            If owner Is Nothing Then
                Throw New ArgumentException("'" & target.GetType().Name & "' has no help parts (tour part '" & part & "').")
            End If
            owner.SetHelpPartDemo(part, True)
            _demoOwner = owner
            _demoPart = part
            Dim r As Rectangle = owner.HelpPartBounds(part)
            missing = r.IsEmpty
            Return r
        Catch ex As ArgumentException
            GlobalErrorLog.Write("HelpTourRunner.PartBounds", ex)
            If note Is Nothing Then note = "Turul nu găsește butonul despre care vorbește; îți arată tot controlul."
            Return Rectangle.Empty
        End Try
    End Function

    ' Puts back a part the tour made visible (the tree row's button that waits for the mouse).
    Private Sub EndDemo()
        If _demoOwner Is Nothing Then Return
        Dim owner As IKBotHelpParts = _demoOwner
        Dim part As String = _demoPart
        _demoOwner = Nothing
        _demoPart = Nothing
        If TypeOf owner Is Control AndAlso DirectCast(owner, Control).IsDisposed Then Return
        owner.SetHelpPartDemo(part, False)
    End Sub

    Private Sub Finish()
        Try
            If _finished Then Return
            _finished = True
            If ReferenceEquals(_active, Me) Then _active = Nothing
            EndDemo()
            If Not _frame.IsDisposed Then _frame.Close()
            If Not _bubble.IsDisposed Then _bubble.CloseByRunner()
            _onFinished?.Invoke()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourRunner.Finish", ex)
        End Try
    End Sub

    ''' <summary>
    ''' <c>TypeName</c> or <c>TypeName.controlName</c>: the first VISIBLE match among the open
    ''' windows (the help and tour windows excluded). Nothing when it is not on screen.
    ''' </summary>
    Friend Shared Function FindTarget(target As String) As Control
        For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
            If Not f.Visible OrElse TypeOf f Is HelpForm OrElse TypeOf f Is HelpTourBubble OrElse TypeOf f Is HelpTourFrame Then Continue For
            Dim hit As Control = FindInWindow(f, target)
            If hit IsNot Nothing Then Return hit
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Slice 0000-20: <c>TypeName</c> or <c>TypeName.controlName</c> inside one window, VISIBLE
    ''' matches only (a view that is not selected is hidden, so it does not count). Nothing = not there.
    ''' </summary>
    Friend Shared Function FindInWindow(window As Form, target As String) As Control
        Dim dot As Integer = target.IndexOf("."c)
        Dim typeName As String = If(dot < 0, target, target.Substring(0, dot)).Trim()
        Dim controlName As String = If(dot < 0, String.Empty, target.Substring(dot + 1).Trim())
        For Each host As Control In OfType(window, typeName)
            If controlName.Length = 0 Then Return host
            For Each hit As Control In host.Controls.Find(controlName, True)
                If hit.Visible AndAlso hit.Width > 0 AndAlso hit.Height > 0 Then Return hit
            Next
        Next
        Return Nothing
    End Function

    ' The control itself and every descendant whose type is typeName, visible ones only.
    Private Shared Iterator Function OfType(root As Control, typeName As String) As IEnumerable(Of Control)
        If Not root.Visible Then Return
        If String.Equals(root.GetType().Name, typeName, StringComparison.OrdinalIgnoreCase) Then Yield root
        For Each child As Control In root.Controls
            For Each hit As Control In OfType(child, typeName)
                Yield hit
            Next
        Next
    End Function

End Class
