Option Strict On
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports KBot.Common
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
        AddHandler _queue.Changed, AddressOf Queue_Changed
        AddHandler _controller.StateChanged, AddressOf Controller_StateChanged
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
                lstCoada.Items.Add("(nicio sarcină în așteptare)")
            Else
                Dim restored As Integer = _ids.IndexOf(selectedId)
                If restored >= 0 Then lstCoada.SelectedIndex = restored
            End If
        Finally
            lstCoada.EndUpdate()
        End Try
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
