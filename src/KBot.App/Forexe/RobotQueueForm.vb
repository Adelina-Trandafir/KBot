Option Strict On
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0098: the small modeless window of the robot queue -- the running task, the waiting
''' ones in the order they will run (each with an X that takes it out), and three commands: pause /
''' resume, empty the queue, stop the running task. The shell opens it (footer button, or by itself
''' when a task has to wait) and owns it; closing it disposes it.
''' </summary>
''' <remarks>
''' While an «Asociere» window (or any other modal box of the running task) is open, this window
''' is disabled like every other window of the application: the queue is waiting for the
''' operator then, and the top line says so.
''' </remarks>
Public Class RobotQueueForm

    Private ReadOnly _queue As RobotQueue
    Private ReadOnly _controller As ForexeController
    ' The ids of the robot tasks waiting, in queue order.
    Private ReadOnly _ids As New List(Of Integer)()

    ' Slice 0100-03: the grid of the running downloads of a multi-thread run (two or more angajamente).
    Private Const COL_COD As String = "cod"
    Private Const COL_PROG As String = "prog"
    Private Const COL_STOP As String = "opreste"
    Private Const COL_REMOVE As String = "scoate"
    ' Waiting angajamente shown at once; more of them scroll.
    Private Const MAX_WAITING_ROWS As Integer = 5
    Private ReadOnly _board As ParallelDownloadBoard
    ' True while the window shows a multi-thread run; the height and minimum size it had before, to go back to.
    Private _inMulti As Boolean
    Private _heightBeforeMulti As Integer
    Private _minSizeBeforeMulti As Size

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(queue As RobotQueue, controller As ForexeController)
        InitializeComponent()
        ArgumentNullException.ThrowIfNull(queue)
        ArgumentNullException.ThrowIfNull(controller)
        _queue = queue
        _controller = controller
        Try
            capBar.IconImage = My.Resources.kbot_64
        Catch ex As Exception
            ' The icon is cosmetic; its absence must not stop the window from opening.
            GlobalErrorLog.Write("RobotQueueForm.New", ex)
        End Try
        _board = controller.ParallelBoard
        AddHandler _queue.Changed, AddressOf Queue_Changed
        AddHandler _controller.StateChanged, AddressOf Controller_StateChanged
        AddHandler _board.Changed, AddressOf Board_Changed
        RefreshAll()
    End Sub

    ' Opened by the shell while the operator works elsewhere: it must not take the focus.
    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Try
            If _queue IsNot Nothing Then RemoveHandler _queue.Changed, AddressOf Queue_Changed
            If _controller IsNot Nothing Then RemoveHandler _controller.StateChanged, AddressOf Controller_StateChanged
            If _board IsNot Nothing Then RemoveHandler _board.Changed, AddressOf Board_Changed
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.OnFormClosed", ex)
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    ' Raised on the UI thread (the queue lives there).
    Private Sub Queue_Changed(sender As Object, e As EventArgs)
        Try
            OnUiThread(AddressOf RefreshAll)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.Queue_Changed", ex)
        End Try
    End Sub

    ' May come from the robot's thread.
    Private Sub Controller_StateChanged(sender As Object, e As EventArgs)
        Try
            OnUiThread(AddressOf RefreshButtons)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.Controller_StateChanged", ex)
        End Try
    End Sub

    ' Slice 0100-03: from the robot's threads (a row moved, came, went) -- posted to the UI thread.
    Private Sub Board_Changed(sender As Object, e As EventArgs)
        Try
            OnUiThread(AddressOf RefreshGrid)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.Board_Changed", ex)
        End Try
    End Sub

    ' The grids of the window. Top grid (a multi-thread run of two or more angajamente only; one alone gets
    ' none -- operator, 03.10.2026): one row per tab at work -- code, progress bar, X. A row that ends leaves,
    ' the next one that starts takes a free place. The WAITING grid replaces the old list: the angajamente
    ' waiting for a tab (multi-thread run) and the robot tasks waiting in the queue, each with an X that
    ' takes it out. Nothing waiting: in a multi-thread run the grid goes and the window shrinks.
    Private Sub RefreshGrid()
        Try
            If _board Is Nothing Then Return
            Dim k_snap As ParallelDownloadBoard.Snapshot = _board.GetSnapshot()
            gridDescarcari.Visible = k_snap.IsMulti
            If k_snap.IsMulti Then
                FillRunning(k_snap)
            ElseIf gridDescarcari.RowCount > 0 Then
                gridDescarcari.ClearRows()
            End If

            Dim k_items As List(Of KeyValuePair(Of String, Object)) = BuildWaitingItems(k_snap)
            FillWaiting(k_items)
            gridAsteapta.Visible = k_items.Count > 0
            lblInCoada.Visible = Not k_snap.IsMulti OrElse k_items.Count > 0
            lblInCoada.Text = If(k_items.Count = 0, "(nicio sarcină în așteptare)", $"În așteptare ({k_items.Count}):")

            FitWindow(k_snap, k_items.Count)
        Catch ex As Exception
            ' UI boundary (posted from a robot thread): log and swallow.
            GlobalErrorLog.Write("RobotQueueForm.RefreshGrid", ex)
        End Try
    End Sub

    Private Sub FillRunning(k_snap As ParallelDownloadBoard.Snapshot)
        ' Same angajamente in the same places: only the bars move (no flicker from a rebuild).
        Dim k_same As Boolean = gridDescarcari.RowCount = k_snap.Rows.Count
        If k_same Then
            For k_i As Integer = 0 To k_snap.Rows.Count - 1
                If Not String.Equals(TryCast(gridDescarcari.Rows(k_i).Tag, String), k_snap.Rows(k_i).Cod,
                                     StringComparison.OrdinalIgnoreCase) Then
                    k_same = False
                    Exit For
                End If
            Next
        End If

        gridDescarcari.BeginUpdate()
        Try
            If Not k_same Then gridDescarcari.ClearRows()
            For k_i As Integer = 0 To k_snap.Rows.Count - 1
                Dim k_entry As ParallelDownloadBoard.Entry = k_snap.Rows(k_i)
                Dim k_row As KBotDataRow = If(k_same, gridDescarcari.Rows(k_i), gridDescarcari.AddRow())
                k_row.Tag = k_entry.Cod
                k_row(COL_COD) = If(k_entry.Stopping, k_entry.Cod & "  (se oprește...)", k_entry.Cod)
                k_row(COL_PROG) = k_entry.Percent
            Next
        Finally
            gridDescarcari.EndUpdate()
        End Try
    End Sub

    ' What waits, in the order it will go: first the angajamente of a multi-thread run still without a tab
    ' (the Tag is their code, a String), then the robot tasks of the queue (the Tag is their id, an Integer).
    Private Function BuildWaitingItems(k_snap As ParallelDownloadBoard.Snapshot) As List(Of KeyValuePair(Of String, Object))
        Dim k_items As New List(Of KeyValuePair(Of String, Object))()
        If k_snap.IsMulti Then
            For Each k_cod As String In k_snap.WaitingCodes
                k_items.Add(New KeyValuePair(Of String, Object)(k_cod, k_cod))
            Next
        End If
        Dim k_position As Integer = 0
        For Each k_task As RobotQueue.RobotTask In _queue.Waiting
            k_position += 1
            k_items.Add(New KeyValuePair(Of String, Object)(
                $"{k_position}. {k_task.Label}   ({k_task.QueuedAt:HH:mm:ss})", k_task.Id))
        Next
        Return k_items
    End Function

    Private Sub FillWaiting(k_items As List(Of KeyValuePair(Of String, Object)))
        Dim k_same As Boolean = gridAsteapta.RowCount = k_items.Count
        If k_same Then
            For k_i As Integer = 0 To k_items.Count - 1
                If Not Object.Equals(gridAsteapta.Rows(k_i).Tag, k_items(k_i).Value) OrElse
                   Not String.Equals(TryCast(gridAsteapta(COL_COD, k_i), String), k_items(k_i).Key, StringComparison.Ordinal) Then
                    k_same = False
                    Exit For
                End If
            Next
        End If
        If k_same Then Return

        gridAsteapta.BeginUpdate()
        Try
            gridAsteapta.ClearRows()
            For Each k_item As KeyValuePair(Of String, Object) In k_items
                Dim k_row As KBotDataRow = gridAsteapta.AddRow()
                k_row.Tag = k_item.Value
                k_row(COL_COD) = k_item.Key
            Next
        Finally
            gridAsteapta.EndUpdate()
        End Try
    End Sub

    ' Sizes the window to what is on show, in a multi-thread run: the running grid, and under it the label and
    ' the waiting grid when something waits (a short scrolling grid above MAX_WAITING_ROWS) -- always keeping
    ' the BOTTOM edge where it is (the window sits above the footer band). Entering a run the window remembers
    ' its height and gives up its minimum height; leaving it, both come back. Outside a run the window keeps
    ' its own size and the waiting grid simply fills what is left.
    Private Sub FitWindow(k_snap As ParallelDownloadBoard.Snapshot, k_waiting As Integer)
        If Not k_snap.IsMulti Then
            If _inMulti Then LeaveMulti()
            Return
        End If
        If Not _inMulti Then EnterMulti()

        ' The grid's OWN scale (it follows the operator's text size / zoom, not only the screen's dpi): a row is
        ' round(RowHeight * scale) px and the frame is the border on both sides. A few pixels short and a
        ' scroll bar appears for a row that should have fitted.
        Dim k_scale As Double = gridDescarcari.DpiScaleY
        Dim k_row As Integer = CInt(Math.Round(gridDescarcari.RowHeight * k_scale))
        Dim k_borderTop As Integer = 2 * CInt(Math.Round(gridDescarcari.BorderWidth * k_scale)) + 2
        Dim k_borderBottom As Integer = 2 * CInt(Math.Round(gridAsteapta.BorderWidth * k_scale)) + 2
        gridDescarcari.Height = Math.Max(1, Math.Min(k_snap.MaxRows, 10)) * k_row + k_borderTop

        Dim k_panel As Integer = gridDescarcari.Height
        If k_waiting > 0 Then k_panel += lblInCoada.Height + Math.Min(k_waiting, MAX_WAITING_ROWS) * k_row + k_borderBottom
        ApplyHeight(Padding.Vertical + capBar.Height + lblCurent.Height + k_panel + pnlFoot.Height)
    End Sub

    Private Sub EnterMulti()
        _inMulti = True
        _heightBeforeMulti = Height
        _minSizeBeforeMulti = MinimumSize
        MinimumSize = New Size(MinimumSize.Width, 0)
    End Sub

    Private Sub LeaveMulti()
        _inMulti = False
        MinimumSize = _minSizeBeforeMulti
        ApplyHeight(_heightBeforeMulti)
    End Sub

    ' New height, bottom edge kept (the window sits above the footer band). Not on screen yet: the shell
    ' places it by its final height, so only the height is set.
    Private Sub ApplyHeight(k_height As Integer)
        If Math.Abs(Height - k_height) < 2 Then Return
        If Not Visible Then
            Height = k_height
            Return
        End If
        Dim k_bottom As Integer = Top + Height
        Dim k_top As Integer = Math.Max(Screen.FromControl(Me).WorkingArea.Top, k_bottom - k_height)
        SetBounds(Left, k_top, Width, k_height)
    End Sub

    ' Once on screen the window has its real (scaled) sizes: fit again.
    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        Try
            RefreshGrid()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.OnShown", ex)
        End Try
    End Sub

    ' The X of a row: stops THAT download only (its tab closes; the others go on).
    Private Sub GridDescarcari_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridDescarcari.ButtonClick
        Try
            If e.ColumnKey <> COL_STOP OrElse _board Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridDescarcari.RowCount Then Return
            Dim k_cod As String = TryCast(gridDescarcari.Rows(e.RowIndex).Tag, String)
            If Not String.IsNullOrEmpty(k_cod) Then _board.StopRow(k_cod)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.GridDescarcari_ButtonClick", ex)
        End Try
    End Sub

    ' The X of a WAITING row: an angajament of the run (it never gets a tab) or a task of the queue.
    Private Sub GridAsteapta_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridAsteapta.ButtonClick
        Try
            If e.ColumnKey <> COL_REMOVE OrElse _board Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAsteapta.RowCount Then Return
            Dim k_tag As Object = gridAsteapta.Rows(e.RowIndex).Tag
            If TypeOf k_tag Is String Then
                _board.RemoveWaiting(DirectCast(k_tag, String))
            ElseIf TypeOf k_tag Is Integer Then
                ' False = it started meanwhile; the grid is redrawn by Changed either way.
                _queue.Cancel(DirectCast(k_tag, Integer))
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.GridAsteapta_ButtonClick", ex)
        End Try
    End Sub

    ' Everything the window shows, from the queue as it stands.
    Private Sub RefreshAll()
        If _queue Is Nothing Then Return
        Dim running As RobotQueue.RobotTask = _queue.Current
        If running Is Nothing Then
            lblCurent.Text = If(_queue.IsPaused, "Coada e în pauză.", "Nicio sarcină în lucru.")
        Else
            Dim text As String = "În lucru: " & running.Label
            If _queue.Note.Length > 0 Then text &= Environment.NewLine & _queue.Note
            If _queue.IsPaused Then text &= Environment.NewLine & "Pauză după ea."
            lblCurent.Text = text
        End If

        _ids.Clear()
        For Each t As RobotQueue.RobotTask In _queue.Waiting
            _ids.Add(t.Id)
        Next
        RefreshGrid()
        RefreshButtons()
    End Sub

    Private Sub RefreshButtons()
        If _queue Is Nothing OrElse _controller Is Nothing Then Return
        btnPauza.Text = If(_queue.IsPaused, "Continuă", "Pauză")
        btnGoleste.Enabled = _ids.Count > 0
        btnOpreste.Enabled = _queue.Current IsNot Nothing AndAlso _controller.IsBusy
    End Sub

    Private Sub BtnPauza_Click(sender As Object, e As EventArgs) Handles btnPauza.Click
        Try
            _queue.SetPaused(Not _queue.IsPaused)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.btnPauza_Click", ex)
        End Try
    End Sub

    Private Sub BtnGoleste_Click(sender As Object, e As EventArgs) Handles btnGoleste.Click
        Try
            If _ids.Count = 0 Then Return
            If KBotMessage.Show(Me, $"Scot din coadă toate cele {_ids.Count} sarcini care așteaptă?" &
                                Environment.NewLine & "Sarcina în lucru se termină normal.",
                                "Coada robotului", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
            _queue.CancelAll()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.btnGoleste_Click", ex)
        End Try
    End Sub

    Private Sub BtnOpreste_Click(sender As Object, e As EventArgs) Handles btnOpreste.Click
        Try
            If Not _controller.IsBusy Then Return
            _controller.Cancel()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.btnOpreste_Click", ex)
        End Try
    End Sub

    Private Sub OnUiThread(action As Action)
        If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(action)
        Else
            action()
        End If
    End Sub

    ' The semantic colours (after ThemeManager.Apply and on a live switch).
    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme = ThemeManager.Current
            Dim p = scheme.Palette

            ' The form's background IS the 1px outline (as InternalInfoForm).
            BackColor = p.BorderColor

            lblCurent.ForeColor = p.TextColor
            lblCurent.BackColor = p.SurfaceAltColor
            lblInCoada.ForeColor = p.TextColor
            lblInCoada.BackColor = p.SurfaceAltColor

            ButtonStyles.ApplySecondary(btnPauza, scheme)
            ButtonStyles.ApplySecondary(btnGoleste, scheme)
            ButtonStyles.ApplySecondary(btnOpreste, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.OnThemeChanged", ex)
        End Try
    End Sub
End Class
