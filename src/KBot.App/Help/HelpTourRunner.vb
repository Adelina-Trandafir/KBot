Option Strict On
Imports System.Drawing
Imports KBot.Common

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
            _active.ShowStep(0)
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
            ShowStep(_index + 1)
        End If
    End Sub

    Private Sub OnBack()
        If _busy OrElse _index = 0 Then Return
        ShowStep(_index - 1)
    End Sub

    ' UI boundary (async void, from bubble buttons): log and swallow; the tour ends on a failure.
    Private Async Sub ShowStep(index As Integer)
        Try
            _busy = True
            _index = index
            Dim [step] As HelpTourStep = _tour.Steps(index)
            Dim note As String = _service.Navigate([step].GoToTarget, _tour.TopicId)
            ' A view is built lazily on its first opening: give the layout a moment before measuring.
            Await Task.Delay(If([step].GoToTarget.Length > 0, 400, 60)).ConfigureAwait(True)
            If _finished Then Return

            Dim target As Control = Nothing
            If [step].Target.Length > 0 Then
                target = FindTarget([step].Target)
                If target Is Nothing AndAlso note Is Nothing Then
                    note = "Partea despre care e vorba nu e pe ecran acum; deschide fereastra sau vederea potrivită ca s-o vezi."
                End If
            End If

            _bubble.ShowStep(_tour.Title, [step].Title, [step].Text, note, index, _tour.Steps.Count)
            Dim rect As Rectangle = Rectangle.Empty
            If target IsNot Nothing Then
                rect = target.RectangleToScreen(target.ClientRectangle)
                _frame.Surround(rect)
            ElseIf _frame.Visible Then
                _frame.Hide()
            End If
            _bubble.PlaceNear(rect)
            If Not _bubble.Visible Then _bubble.Show()
            _bubble.Activate()
        Catch ex As Exception
            GlobalErrorLog.Write("HelpTourRunner.ShowStep", ex)
            Finish()
        Finally
            _busy = False
        End Try
    End Sub

    Private Sub Finish()
        Try
            If _finished Then Return
            _finished = True
            If ReferenceEquals(_active, Me) Then _active = Nothing
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
        Dim dot As Integer = target.IndexOf("."c)
        Dim typeName As String = If(dot < 0, target, target.Substring(0, dot)).Trim()
        Dim controlName As String = If(dot < 0, String.Empty, target.Substring(dot + 1).Trim())
        For Each f As Form In Application.OpenForms.Cast(Of Form)().ToList()
            If Not f.Visible OrElse TypeOf f Is HelpForm OrElse TypeOf f Is HelpTourBubble OrElse TypeOf f Is HelpTourFrame Then Continue For
            For Each host As Control In OfType(f, typeName)
                If controlName.Length = 0 Then Return host
                For Each hit As Control In host.Controls.Find(controlName, True)
                    If hit.Visible AndAlso hit.Width > 0 AndAlso hit.Height > 0 Then Return hit
                Next
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
