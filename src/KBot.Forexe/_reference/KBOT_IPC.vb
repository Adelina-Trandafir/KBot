Imports System.IO
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports GeneralClasses
Imports System.Drawing

Partial Public Class KBOT_IPC
    ' =========================
    ' CONSTRUCTOR
    ' =========================
    Public Sub New(jobPath As String)
        InitializeComponent()
        _jobFilePath = jobPath

        ' Inițializare model UI minimal
        Try
            If File.Exists(_jobFilePath) Then
                Dim jsonContent As String = File.ReadAllText(_jobFilePath)
                Dim tempJob = JsonConvert.DeserializeObject(Of RobotJob)(jsonContent)
                _currentJobConfig = tempJob

                If tempJob IsNot Nothing AndAlso Not tempJob.ShowLogs Then
                    lblTitleLog.Visible = False
                    rtbLog.Visible = False
                    tlpLayout.RowStyles(3).SizeType = SizeType.Absolute
                    tlpLayout.RowStyles(3).Height = 0
                    tlpLayout.RowStyles(4).SizeType = SizeType.Absolute
                    tlpLayout.RowStyles(4).Height = 0
                    Me.Height = 400
                End If
            End If
        Catch
        End Try
    End Sub

    Public Sub New(jobPath As String, ByVal pHwnd As IntPtr)
        InitializeComponent()
        Me.TopMost = True
        Me.Activate()

#If DEBUG Then
        FindWindowByTitleInsideAccess(pHwnd, "K-BOT")
        _deleteJobFileAfterRead = False
#Else
        _deleteJobFileAfterRead = True
#End If

        _jobFilePath = jobPath
        _isServer = pHwnd <> IntPtr.Zero
        _hwndWatcher = pHwnd

        Try
            If File.Exists(_jobFilePath) Then
                Dim jsonContent As String = File.ReadAllText(_jobFilePath)
                Dim tempJob = JsonConvert.DeserializeObject(Of RobotJob)(jsonContent)
                _currentJobConfig = tempJob
                _jobFolderPath = If(String.IsNullOrEmpty(tempJob.JobFolder), Path.GetDirectoryName(_jobFilePath), tempJob.JobFolder)

            End If
        Catch
        End Try


        lblTitleLog.Visible = False
        rtbLog.Visible = False

        tlpLayout.RowStyles(1).SizeType = SizeType.Absolute
        tlpLayout.RowStyles(1).Height = 0
        tlpLayout.RowStyles(2).SizeType = SizeType.Absolute
        tlpLayout.RowStyles(2).Height = 0
        Me.ClientSize = New Size(Me.ClientSize.Width, 90)

        Me.WindowState = FormWindowState.Normal
        Me.ShowInTaskbar = True
        Me.BringToFront()
        Me.Activate()

        _originalConsoleHeight = Me.Height
    End Sub

    ' =========================
    ' FORM
    ' =========================
    Private Sub btnAfiseazaLog_Click(sender As Object, e As EventArgs) Handles btnAfiseazaLog.Click
        If Not _isConsoleVisible Then
            Me.lblTitleLog.Visible = True
            Me.rtbLog.Visible = True
            Me.tlpLayout.RowStyles(1).SizeType = SizeType.AutoSize
            Me.tlpLayout.RowStyles(2).SizeType = SizeType.Percent
            Me.tlpLayout.RowStyles(2).Height = 70.0F
            Me.Height = 600

            _isConsoleVisible = True
            btnAfiseazaLog.Image = My.Resources.Hide_32
        Else
            lblTitleLog.Visible = False
            rtbLog.Visible = False
            Me.tlpLayout.RowStyles(1).SizeType = SizeType.Absolute
            Me.tlpLayout.RowStyles(1).Height = 0
            Me.tlpLayout.RowStyles(2).SizeType = SizeType.Absolute
            Me.tlpLayout.RowStyles(2).Height = 0
            Me.Height = _originalConsoleHeight

            _isConsoleVisible = False
            btnAfiseazaLog.Image = My.Resources.Show_32
        End If

    End Sub

    Private Sub KBOT_IPC_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        KBotTheme.ApplyTheme(Me)
        _logger = New RichTextBoxLogger(rtbLog)
        _ipcListener = New IPCListener()

        AddHandler _ipcListener.OnMessageReceived, AddressOf OnIpcMessageReceived

        If _isServer Then
#If DEBUG Then
            Return
#End If
            If Not ConnectToPipe() Then
                MessageBox.Show("Nu am putut stabili conexiunea IPC cu Access-ul părinte! Mă închid.", "Eroare IPC", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Application.Exit()
            End If
        End If

        If _isConnected Then
            SendMessageToPipe(PipeCmd.VBNET_HWND, 0, _ipcListener.HandleValue.ToString(), Nothing)
        End If
    End Sub

    Private Async Sub KBOT_IPC_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        lblStatus.Text = "Inițializare..."

        ' 3. Dacă e prima rulare (nu standby), executăm
        Dim result = Await ExecuteConnectionJob(_jobFilePath)

        ' === MODIFICAREA AICI ===
        If Not result Then
            If _isLightMode Then
                ' Utilizatorul a ales Resend Only — nu închidem, intrăm în standby fără executor
                _logger?.LogInfo("[LIGHT] Mod Resend Only activ. Intru în standby fără executor.")
                _wasLogonCompleted = True  ' Prevenim EmergencyDump la hide/close
                Me.TopMost = False
                EnterStandbyMode()
                ' NU trimitem CONNECTED — VBA a primit deja JOB_ERROR din SetCertificateAndPin
                Return
            End If

            _logger?.LogError("Eroare critică la executarea job-ului principal.")
            _forceExit = True
            Application.Exit()
            Return
        End If

        ' 4. După finalizare, intrăm în mod standby sau închidem
        If _isServer AndAlso _hwndWatcher <> IntPtr.Zero Then
            _cacheExpirationTime = DateTime.Now.AddMinutes(CERTIFICATE_CACHE_MINUTES)
            If _deleteJobFileAfterRead Then File.Delete(_jobFilePath) ' Ștergem fișierul job după rulare)

            _wasLogonCompleted = True
            Me.TopMost = False

            EnterStandbyMode()
            SendMessageToPipe(PipeCmd.CONNECTED, 0, "CONECTARE", "Job finalizat. Aștept comenzi noi.")
            'SendMessageToPipe(PipeCmd.JOB_SUCCESS, _executor.CurrentURL)
            'SendMessageToPipe(PipeCmd.WAITING, "Robotul a intrat în mod standby.")
        Else
            Application.Exit()

        End If
    End Sub

    ' CERINȚA A: La apăsarea X (Close), dacă e Server, se ascunde.
    Private Sub KBOT_IPC_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If Not _wasLogonCompleted AndAlso Not _isLightMode Then
            ' Dacă logon-ul nu s-a completat și nu suntem în mod light, dump + ieșire forțată
            _forceExit = True
            _logger.EmergencyDump()
        End If

        ' 1. Logica de Server: X-ul doar ascunde, nu închide (decât dacă _forceExit e True)
        If _isServer AndAlso Not _forceExit AndAlso _executor IsNot Nothing Then
            If e.CloseReason = CloseReason.UserClosing Then
                e.Cancel = True
                Me.Hide()

                ' Asigurăm că tray icon e vizibil
                If _notifyIcon Is Nothing Then InitializeTrayComponents()
                _notifyIcon.Visible = True
                _notifyIcon.ShowBalloonTip(1000, "ForexeBot", "Robotul rulează în fundal.", ToolTipIcon.Info)

                ' Pornim timerul de tooltip dacă nu merge
                If _trayTimer IsNot Nothing AndAlso Not _trayTimer.Enabled Then _trayTimer.Start()
                Return
            End If
        End If

        ' 2. Cleanup (executat doar la Exit real)
        _trayTimer?.Stop()
        _cancellationTokenSource?.Cancel()

        If _notifyIcon IsNot Nothing Then
            _notifyIcon.Visible = False
            _notifyIcon.Dispose()
        End If

        If _pipeClient IsNot Nothing Then
            If _pipeClient.IsConnected Then _pipeClient.Close()
            _pipeClient.Dispose()
        End If

        _ipcListener?.Dispose()
    End Sub

    ' CERINȚA B: La minimizare, se ascunde în Tray.
    Private Sub KBOT_IPC_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If _isServer AndAlso Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()

            If _notifyIcon Is Nothing Then InitializeTrayComponents()
            _notifyIcon.Visible = True

            If _trayTimer IsNot Nothing AndAlso Not _trayTimer.Enabled Then _trayTimer.Start()
        End If
    End Sub

    Private Sub BtnAnulare_Click(sender As Object, e As EventArgs) Handles btnAnulare.Click
        ' Verificăm dacă robotul lucrează și dacă avem un token de anulare valid
        If _isBusy AndAlso _cancellationTokenSource IsNot Nothing Then
            If MessageBox.Show("Sigur doriți să opriți execuția curentă?", "Confirmare Anulare", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                Try
                    _cancellationTokenSource.Cancel
                Catch ex As Exception
                    _logger?.LogError($"Eroare la trimiterea semnalului de cancel: {ex.Message}")
                End Try

                ' Opțional: Resetăm starea UI
                lblStatus.Text = "Operație anulată"
            End If
        Else
            MessageBox.Show("Nu există niciun proces activ în acest moment.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Me.TopMost = True ' Runtime: Start normal, nu deasupra
    End Sub

    Private Sub OnIpcMessageReceived(m As Message)
        If m.Msg = CMD_EXIT Then
            _logger?.LogInfo("[IPC] Primit comanda EXIT de la Access.")
            Application.Exit()

        ElseIf m.Msg = CMD_RECONNECT Then
            _logger?.LogInfo("[IPC] Primit comanda RECONNECT. Încerc reconectare Pipe...")
            SendMessageToPipe(PipeCmd.RECONNECTED, 0, "Robotul s-a reconectat la cerere.")

        ElseIf m.Msg = CMD_START_JOB Then
            _logger?.LogInfo("[IPC] Primit comanda START JOB.")

            Me.Invoke(Sub()
                          Me.TopMost = False
                          Me.ShowInTaskbar = True
                          Me.Show()
                          Me.WindowState = FormWindowState.Normal
                          Me.Activate()
                          Me.BringToFront()

                          SetForegroundWindow(Me.Handle)
                      End Sub)

            If Not _isBusy Then
#Disable Warning BC42358
                RunJobFromFolder()
#Enable Warning BC42358
            Else
                SendMessageToPipe(PipeCmd.BUSY, _currentTaskId, _currentJobConfig.Tasks.GetEnumerator().Current?.Path)
            End If

        ElseIf m.Msg = CMD_STOP_JOB Then
            _logger?.LogInfo("[IPC] Primit comanda STOP JOB.")
            If _isBusy AndAlso _cancellationTokenSource IsNot Nothing Then
                Try
                    _cancellationTokenSource.Cancel()
                    _logger?.LogInfo("[IPC] Anulare job în curs...")
                Catch ex As Exception
                    _logger?.LogError($"[IPC] Eroare la anularea job-ului: {ex.Message}")
                    SendMessageToPipe(PipeCmd.JOB_ERROR, _currentTaskId, _currentJobName, $"Eroare la anularea job-ului: {ex.Message}")
                End Try
            Else
                _logger?.LogInfo("[IPC] Nu există job în curs de anulare.")
            End If

        ElseIf m.Msg = CMD_HELLO Then
            _logger?.LogInfo("[IPC] Primit comanda HELLO de la Access.")
            MsgBox("Salut din ACCESS!", MsgBoxStyle.Information, "IPC Test")

        ElseIf m.Msg = CMD_VBA_READY Then
            Dim finishedTaskId As Integer = m.WParam.ToInt32()
            If finishedTaskId > 0 Then
                _logger?.LogInfo($"[IPC] VBA a confirmat procesarea Task-ului ID: {finishedTaskId}")
            End If
            _waitingForVbaAck = False
            TrySendNextMessage()

        ElseIf m.Msg = CMD_SAVE_LOG Then
            _logger?.LogInfo("[IPC] Primit comanda SAVE_LOG de la VBA.")
            If Me.InvokeRequired Then
                Me.Invoke(Sub() SaveLogAutomatic())
            Else
                SaveLogAutomatic()
            End If

        ElseIf m.Msg = CMD_SHOW_BROWSER Then
            _logger?.LogInfo("[IPC] Primit comanda SHOW_BROWSER de la Access.")
            If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then
                SendMessageToPipe(PipeCmd.INFO, _currentTaskId, "Browser indisponibil. Executorul nu este activ.")
            Else
#Disable Warning BC42358
                If _executor.IsBrowserVisible Then
                    _executor.HideBrowserWindowAsync()
                Else
                    _executor.ShowBrowserWindowAsync()
                End If
#Enable Warning BC42358
            End If

        ElseIf m.Msg = CMD_SHOW_HISTORY Then
            _logger?.LogInfo("[IPC] Primit comanda SHOW_HISTORY de la Access.")
            If _isBusy Then
                _logger?.LogInfo("[IPC] SHOW_HISTORY ignorat: robotul este ocupat.")
            Else
                Dim hForm As New HistoryForm()
                hForm.Show()
            End If

        ElseIf m.Msg = CMD_GET_EXTRASE Then
            _logger?.LogInfo("[IPC] Primit comanda GET_EXTRASE de la Access.")

            Me.Invoke(Sub()
                          Me.TopMost = False
                          Me.ShowInTaskbar = True
                          Me.Show()
                          Me.WindowState = FormWindowState.Normal
                          Me.Activate()
                          Me.BringToFront()

                          SetForegroundWindow(Me.Handle)
                      End Sub)

            If Not _isBusy Then
                If _executor IsNot Nothing Then
#Disable Warning BC42358
                    RunSNMJobFromFolder()
#Enable Warning BC42358
                End If
            Else
                SendMessageToPipe(PipeCmd.BUSY, _currentTaskId, _currentJobConfig.Tasks.GetEnumerator().Current?.Path)
            End If

        End If
    End Sub

    Private Sub Executor_OnBrowserClosed(message As String) Handles _executor.OnBrowserClosed
        'salveaza fluxul curent la fel ca in monitoring, dar cu un flag care sa indice ca inchiderea a fost neasteptata
        If Me.InvokeRequired Then
            Me.Invoke(Sub() SaveLogAutomatic())
            Me.Invoke(Sub() ManualSaveResendPackage(True))
        Else
            SaveLogAutomatic()
            ManualSaveResendPackage(True)
        End If

        Application.Exit()
    End Sub

    Private Async Sub btnAfiseazaBrowser_Click(sender As Object, e As EventArgs) Handles btnAfiseazaBrowser.Click
        If _executor.IsBrowserVisible Then
            Await _executor.HideBrowserWindowAsync()
        Else
            Await _executor.ShowBrowserWindowAsync()
        End If
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property
End Class