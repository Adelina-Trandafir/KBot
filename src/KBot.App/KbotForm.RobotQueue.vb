Imports KBot.Common

''' <summary>
''' Slice 0098: the robot queue in the shell -- the footer button and the queue window. The queue
''' itself is <see cref="RobotQueue"/>; every robot entry point of the shell puts its whole
''' operation there (KbotForm.Download / Ingest / Extrase / Ddf / CabNotes / ForexeWatch).
''' </summary>
Partial Public Class KbotForm

    ' The operator closed the window while tasks were still waiting: it is not pushed at them
    ' again until the queue has emptied once.
    Private _queueFormDismissed As Boolean

    ''' <summary>Called once from Load, after the footer band is bound to the coordinator.</summary>
    Private Sub BindRobotQueue()
        forexeFooter.BindQueue(_robotQueue)
        AddHandler _robotQueue.Changed, AddressOf RobotQueue_Changed
        AddHandler _controller.ParallelBoard.Started, AddressOf ParallelBoard_Started
    End Sub

    ''' <summary>The queue lives as long as the shell, but the handler is removed on close anyway.</summary>
    Private Sub UnbindRobotQueue()
        RemoveHandler _robotQueue.Changed, AddressOf RobotQueue_Changed
        RemoveHandler _controller.ParallelBoard.Started, AddressOf ParallelBoard_Started
    End Sub

    ' Slice 0100-03: a download of TWO OR MORE angajamente began -- the window with a row per running
    ' download opens by itself, even though it is one single queued task (operator, 03.10.2026). One
    ' angajament alone never opens it. Raised from the robot side: posted to the UI thread.
    Private Sub ParallelBoard_Started(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            BeginInvoke(New Action(
                Sub()
                    Try
                        If IsDisposed OrElse Disposing Then Return
                        _queueFormDismissed = False
                        ShowRobotQueue(activate:=False)
                    Catch ex As Exception
                        GlobalErrorLog.Write("MainForm.ParallelBoard_Started", ex)
                    End Try
                End Sub))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ParallelBoard_Started", ex)
        End Try
    End Sub

    ' The window opens by itself only when MORE THAN ONE FOREXE action is under way (operator,
    ' 30.09.2026): that is when there is an order to see and something to take out. One action
    ' alone never opens it; the footer button still does.
    Private Sub RobotQueue_Changed(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing Then Return
            Dim actions As Integer = _robotQueue.ActionCount(_controller.IsBusy)
            If _robotQueue.Current Is Nothing AndAlso _robotQueue.Waiting.Count = 0 Then
                _queueFormDismissed = False
                ' Done with the queue: the window closes by itself (operator, 01.10.2026). Closed
                ' later, in a message of its own, so the queue can start its next task meanwhile.
                If IsHandleCreated Then BeginInvoke(New Action(AddressOf CloseRobotQueueIfIdle))
                Return
            End If
            If actions > 1 AndAlso Not _queueFormDismissed Then ShowRobotQueue(activate:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RobotQueue_Changed", ex)
        End Try
    End Sub

    Private Sub CloseRobotQueueIfIdle()
        Try
            If IsDisposed OrElse Disposing Then Return
            If _robotQueue.Current IsNot Nothing OrElse _robotQueue.Waiting.Count > 0 Then Return
            If _queueForm IsNot Nothing AndAlso Not _queueForm.IsDisposed Then _queueForm.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.CloseRobotQueueIfIdle", ex)
        End Try
    End Sub

    Private Sub ForexeFooter_QueueRequested(sender As Object, e As EventArgs) Handles forexeFooter.QueueRequested
        Try
            _queueFormDismissed = False
            ShowRobotQueue(activate:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.forexeFooter_QueueRequested", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens the queue window (modeless, owned by the shell), or brings the open one forward.
    ''' Placed at the bottom-right of the shell, above the footer band, the first time.
    ''' </summary>
    Private Sub ShowRobotQueue(activate As Boolean)
        If _queueForm Is Nothing OrElse _queueForm.IsDisposed Then
            _queueForm = New RobotQueueForm(_robotQueue, _controller)
            AddHandler _queueForm.FormClosed,
                Sub()
                    If _robotQueue.Waiting.Count > 0 Then _queueFormDismissed = True
                    _queueForm = Nothing
                End Sub
            _queueForm.StartPosition = FormStartPosition.Manual
            _queueForm.Show(Me)
            ' Placed AFTER Show: the window is scaled to the screen's dpi only when its handle is made, so
            ' its width before Show is the designer's and the right edge landed past the shell's (off the
            ' screen when the shell is maximized). Its real size is known now.
            PlaceRobotQueue()
        ElseIf activate Then
            If _queueForm.WindowState = FormWindowState.Minimized Then _queueForm.WindowState = FormWindowState.Normal
            _queueForm.BringToFront()
        End If
        If activate Then _queueForm.Activate()
    End Sub

    ''' <summary>
    ''' Puts the queue window flush with the shell's right edge, its bottom edge just above the footer
    ''' band, by the window's REAL size. Never lets it leave the screen's working area.
    ''' </summary>
    Private Sub PlaceRobotQueue()
        Try
            If _queueForm Is Nothing OrElse _queueForm.IsDisposed Then Return
            Dim k_area As Rectangle = RectangleToScreen(ClientRectangle)
            Dim k_footerTop As Integer = forexeFooter.PointToScreen(Point.Empty).Y
            Dim k_work As Rectangle = Screen.FromControl(Me).WorkingArea
            Dim k_left As Integer = Math.Min(k_area.Right, k_work.Right) - _queueForm.Width
            Dim k_top As Integer = k_footerTop - _queueForm.Height - 8
            _queueForm.Location = New Point(Math.Max(Math.Max(k_area.Left, k_work.Left), k_left),
                                            Math.Max(Math.Max(k_area.Top, k_work.Top), k_top))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.PlaceRobotQueue", ex)
        End Try
    End Sub
End Class
