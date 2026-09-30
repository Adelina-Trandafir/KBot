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
    End Sub

    ''' <summary>The queue lives as long as the shell, but the handler is removed on close anyway.</summary>
    Private Sub UnbindRobotQueue()
        RemoveHandler _robotQueue.Changed, AddressOf RobotQueue_Changed
    End Sub

    ' The window opens by itself the moment a task has to WAIT behind another one: that is when
    ' the operator needs to see the order (and may want to take something out).
    Private Sub RobotQueue_Changed(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing Then Return
            If _robotQueue.Current Is Nothing AndAlso _robotQueue.Waiting.Count = 0 Then
                _queueFormDismissed = False
                Return
            End If
            If _robotQueue.Waiting.Count > 0 AndAlso Not _queueFormDismissed Then ShowRobotQueue(activate:=False)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.RobotQueue_Changed", ex)
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
            Dim area As Rectangle = RectangleToScreen(ClientRectangle)
            Dim footerTop As Integer = forexeFooter.PointToScreen(Point.Empty).Y
            _queueForm.Location = New Point(Math.Max(area.Left, area.Right - _queueForm.Width - 16),
                                            Math.Max(area.Top, footerTop - _queueForm.Height - 8))
            _queueForm.Show(Me)
        ElseIf activate Then
            If _queueForm.WindowState = FormWindowState.Minimized Then _queueForm.WindowState = FormWindowState.Normal
            _queueForm.BringToFront()
        End If
        If activate Then _queueForm.Activate()
    End Sub
End Class
