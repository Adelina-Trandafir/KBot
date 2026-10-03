Option Strict On
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Slice 0098: the small modeless window of the robot queue -- the running task, the waiting
''' ones in the order they will run, and four commands: pause / resume, take the selected task
''' out, empty the queue, stop the running task. The shell opens it (footer button, or by itself
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
    ' The ids of the rows in lstCoada, in the same order.
    Private ReadOnly _ids As New List(Of Integer)()

    ' Slice 0100-03: the grid of the running downloads of a multi-thread run (two or more angajamente).
    Private Const COL_COD As String = "cod"
    Private Const COL_PROG As String = "prog"
    Private Const COL_STOP As String = "opreste"
    Private Const COL_REMOVE As String = "scoate"
    Private Const STOP_CAPTION As String = "X"
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
        pnlDescarcari.Visible = False
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

    ' The grids of a multi-thread run (two or more angajamente; one alone gets none -- operator,
    ' 03.10.2026). Top grid: one row per tab at work -- code, progress bar, X. A row that ends leaves, the
    ' next one that starts takes a free place. Under it, the angajamente still WAITING for a tab, each with
    ' an X that takes it out of the run. The queue's own list (other tasks) shows only when it has any.
    Private Sub RefreshGrid()
        Try
            If _board Is Nothing Then Return
            Dim k_snap As ParallelDownloadBoard.Snapshot = _board.GetSnapshot()
            If k_snap.IsMulti Then
                FillRunning(k_snap)
                FillWaiting(k_snap)
            ElseIf gridDescarcari.RowCount > 0 OrElse gridAsteapta.RowCount > 0 Then
                gridDescarcari.ClearRows()
                gridAsteapta.ClearRows()
            End If
            FitWindow(k_snap)
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
                k_row(COL_STOP) = STOP_CAPTION
            Next
        Finally
            gridDescarcari.EndUpdate()
        End Try
    End Sub

    Private Sub FillWaiting(k_snap As ParallelDownloadBoard.Snapshot)
        Dim k_same As Boolean = gridAsteapta.RowCount = k_snap.WaitingCodes.Count
        If k_same Then
            For k_i As Integer = 0 To k_snap.WaitingCodes.Count - 1
                If Not String.Equals(TryCast(gridAsteapta.Rows(k_i).Tag, String), k_snap.WaitingCodes(k_i),
                                     StringComparison.OrdinalIgnoreCase) Then
                    k_same = False
                    Exit For
                End If
            Next
        End If
        If k_same Then Return

        gridAsteapta.BeginUpdate()
        Try
            gridAsteapta.ClearRows()
            For Each k_cod As String In k_snap.WaitingCodes
                Dim k_row As KBotDataRow = gridAsteapta.AddRow()
                k_row.Tag = k_cod
                k_row(COL_COD) = k_cod
                k_row(COL_REMOVE) = STOP_CAPTION
            Next
        Finally
            gridAsteapta.EndUpdate()
        End Try
    End Sub

    ' Sizes the panel and the window to what is on show. Entering a multi-thread run the window remembers its
    ' height and gives up its minimum height; it then grows or shrinks to fit -- the running grid, the waiting
    ' grid when something waits (a short scrolling list above MAX_WAITING_ROWS), and the queue's own list only
    ' when other tasks wait -- always keeping its BOTTOM edge where it is (it sits above the footer band).
    Private Sub FitWindow(k_snap As ParallelDownloadBoard.Snapshot)
        If Not k_snap.IsMulti Then
            If _inMulti Then LeaveMulti()
            Return
        End If
        If Not _inMulti Then EnterMulti()

        Dim k_scale As Double = DeviceDpi / 96.0
        Dim k_row As Integer = CInt(Math.Ceiling(gridDescarcari.RowHeight * k_scale))
        Dim k_border As Integer = CInt(Math.Ceiling(4 * k_scale))
        gridDescarcari.Height = Math.Max(1, Math.Min(k_snap.MaxRows, 10)) * k_row + k_border

        Dim k_hasWaiting As Boolean = k_snap.Waiting > 0
        lblInCoada.Visible = k_hasWaiting
        gridAsteapta.Visible = k_hasWaiting
        Dim k_panel As Integer = gridDescarcari.Height
        If k_hasWaiting Then
            lblInCoada.Text = $"Încă {k_snap.Waiting} în coadă:"
            k_panel += lblInCoada.Height + Math.Min(k_snap.Waiting, MAX_WAITING_ROWS) * k_row + k_border
        End If
        pnlDescarcari.Height = k_panel

        Dim k_listVisible As Boolean = _ids.Count > 0
        lstCoada.Visible = k_listVisible
        Dim k_listHeight As Integer = If(k_listVisible, lstCoada.ItemHeight * Math.Min(Math.Max(_ids.Count, 2), 4) + 4, 0)

        ApplyHeight(Padding.Vertical + capBar.Height + lblCurent.Height + k_panel + pnlFoot.Height + k_listHeight)
    End Sub

    Private Sub EnterMulti()
        _inMulti = True
        _heightBeforeMulti = Height
        _minSizeBeforeMulti = MinimumSize
        MinimumSize = New Size(MinimumSize.Width, 0)
        pnlDescarcari.Visible = True
    End Sub

    Private Sub LeaveMulti()
        _inMulti = False
        pnlDescarcari.Visible = False
        lstCoada.Visible = True
        If lstCoada.Items.Count = 0 Then lstCoada.Items.Add("(nicio sarcină în așteptare)")
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

    ' The X of a WAITING angajament: it leaves the run and never gets a tab.
    Private Sub GridAsteapta_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridAsteapta.ButtonClick
        Try
            If e.ColumnKey <> COL_REMOVE OrElse _board Is Nothing Then Return
            If e.RowIndex < 0 OrElse e.RowIndex >= gridAsteapta.RowCount Then Return
            Dim k_cod As String = TryCast(gridAsteapta.Rows(e.RowIndex).Tag, String)
            If Not String.IsNullOrEmpty(k_cod) Then _board.RemoveWaiting(k_cod)
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

        Dim selectedId As Integer = If(lstCoada.SelectedIndex >= 0 AndAlso lstCoada.SelectedIndex < _ids.Count,
                                     _ids(lstCoada.SelectedIndex), -1)
        lstCoada.BeginUpdate()
        Try
            lstCoada.Items.Clear()
            _ids.Clear()
            Dim position As Integer = 0
            For Each t As RobotQueue.RobotTask In _queue.Waiting
                position += 1
                lstCoada.Items.Add($"{position}. {t.Label}   ({t.QueuedAt:HH:mm:ss})")
                _ids.Add(t.Id)
            Next
            If lstCoada.Items.Count = 0 Then
                ' A multi-thread run hides the list when it is empty; the placeholder is for the plain window.
                If Not _inMulti Then lstCoada.Items.Add("(nicio sarcină în așteptare)")
            Else
                Dim restored As Integer = _ids.IndexOf(selectedId)
                If restored >= 0 Then lstCoada.SelectedIndex = restored
            End If
        Finally
            lstCoada.EndUpdate()
        End Try
        RefreshGrid()
        RefreshButtons()
    End Sub

    Private Sub RefreshButtons()
        If _queue Is Nothing OrElse _controller Is Nothing Then Return
        btnPauza.Text = If(_queue.IsPaused, "Continuă", "Pauză")
        btnScoate.Enabled = lstCoada.SelectedIndex >= 0 AndAlso lstCoada.SelectedIndex < _ids.Count
        btnGoleste.Enabled = _ids.Count > 0
        btnOpreste.Enabled = _queue.Current IsNot Nothing AndAlso _controller.IsBusy
    End Sub

    Private Sub LstCoada_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstCoada.SelectedIndexChanged
        Try
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.lstCoada_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub BtnPauza_Click(sender As Object, e As EventArgs) Handles btnPauza.Click
        Try
            _queue.SetPaused(Not _queue.IsPaused)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.btnPauza_Click", ex)
        End Try
    End Sub

    Private Sub BtnScoate_Click(sender As Object, e As EventArgs) Handles btnScoate.Click
        Try
            Dim i As Integer = lstCoada.SelectedIndex
            If i < 0 OrElse i >= _ids.Count Then Return
            ' False = it started meanwhile; the list is redrawn by Changed either way.
            _queue.Cancel(_ids(i))
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.btnScoate_Click", ex)
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
            lstCoada.BackColor = p.SurfaceAltColor
            lstCoada.ForeColor = p.TextColor

            ButtonStyles.ApplySecondary(btnPauza, scheme)
            ButtonStyles.ApplySecondary(btnScoate, scheme)
            ButtonStyles.ApplySecondary(btnGoleste, scheme)
            ButtonStyles.ApplySecondary(btnOpreste, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("RobotQueueForm.OnThemeChanged", ex)
        End Try
    End Sub
End Class
