Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports GeneralClasses
Imports Microsoft.Win32
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports WorkflowModels

Partial Public Class KBOT_STANDALONE
    Inherits Form

    Private WithEvents _executor As WorkflowExecutor = Nothing

    Private ReadOnly _startupConfig As StartupConfig
    Private _logger As RichTextBoxLogger
    Private _certificates As List(Of X509Certificate2)
    Private _cancellationTokenSource As CancellationTokenSource
    Private _isRunning As Boolean = False
    Private _contextMenuWorkflow As ContextMenuStrip
    Private _contextMenuIndex As Integer = -1
    Private _currentWflFolder As String = ""

    Private _chkVisible As CheckBox
    Private _chkStepByStep As CheckBox
    Private _chkOnlyCheckpoints As CheckBox

    ' Handler OnStatusUpdate — stocat ca să poată fi RemoveHandler-it la rewire
    Private _onStatusUpdateHandler As WorkflowExecutor.OnStatusUpdateEventHandler = Nothing
    Private _onLogMessageHandler As WorkflowExecutor.OnLogMessageEventHandler = Nothing

    ' Step-by-step control
    ' Folosim AutoResetEvent pentru a nu bloca thread-urile aiurea
    Private _stepWaitEvent As New AutoResetEvent(False)
    Private _stepResult As StepResult

    ' For resume functionality  
    Private _lastExecutor As WorkflowExecutor = Nothing

    ' Culoarea originală a panelului
    Private _defaultPanelColor As Color

    Private CodAngajamentCautat As String = ""

    Private Const LOGIN_WORKFLOW_PATH As String = "C:\AVACONT\FB_WORKFLOWS\adlop - Conectare.wfl"

    Private _currentThrottleSettings As ThrottleSettings = ThrottleSettings.None

    ' Timer care polling _executor.IsBrowserOpen → sincronizează chkVisible
    ' Se oprește singur după ce browserul e detectat și pornit
    Private _browserReadyTimer As System.Windows.Forms.Timer

    Private _jobFolderPath As String

    Private Const ResendSubfolderName As String = "Resend"
    Private Const ResendExtension As String = ".resend.json"

    Private dumpFolder As String = "C:\AVACONT\FOREXE\Dumps"

    Private _wicketMonitorForm As WicketMonitorForm = Nothing

    ' Enum-ul trebuie să fie identic cu cel din Executor (inclusiv Previous)
    Private Enum StepResult
        [Continue]
        Skip
        [Stop]
        Previous
    End Enum

    Public Sub New() '(startupConfig As StartupConfig)
        InitializeComponent()
        '_startupConfig = StartupConfig
        _defaultPanelColor = pnlStepControl.BackColor
    End Sub

    Private Sub btnDarkMode_Click(sender As Object, e As EventArgs) Handles btnDarkMode.Click
        SetTheme(Not IsDark)
        ' Re-aplică starea activă a tab-urilor și tree-ului după schimbarea temei
        Dim activeTab = If(pnlTabPrincipal.Visible, btnTabPrincipal, btnTabSetari)
        UpdateTabButtons(activeTab)
        UpdateToggleButtons()
        ApplyStepTreeTheme()
    End Sub

    ' ── Tab switching ──────────────────────────────────────────────────────────
    Private Sub btnTabPrincipal_Click(sender As Object, e As EventArgs) Handles btnTabPrincipal.Click
        pnlTabPrincipal.Visible = True
        pnlTabSetari.Visible = False
        UpdateTabButtons(btnTabPrincipal)
    End Sub

    Private Sub btnTabSetari_Click(sender As Object, e As EventArgs) Handles btnTabSetari.Click
        pnlTabSetari.Visible = True
        pnlTabPrincipal.Visible = False
        UpdateTabButtons(btnTabSetari)
    End Sub

    Private Sub UpdateTabButtons(active As Button)
        Dim isDark = KBotTheme.IsDark
        For Each btn As Button In {btnTabPrincipal, btnTabSetari}
            If btn Is active Then
                btn.FlatAppearance.BorderColor = KBotTheme.CLR_TAB_ACCENT
                btn.FlatAppearance.BorderSize = 2
                btn.BackColor = If(isDark, KBotTheme.CLR_BG, SystemColors.ControlLight)
                btn.UseVisualStyleBackColor = False
            Else
                btn.FlatAppearance.BorderColor = If(isDark, KBotTheme.CLR_BTN_BORDER, SystemColors.ControlDark)
                btn.FlatAppearance.BorderSize = 1
                btn.BackColor = If(isDark, KBotTheme.CLR_BTN, SystemColors.Control)
                btn.UseVisualStyleBackColor = False
            End If
        Next
    End Sub

    Private Sub KBOT_STANDALONE_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        KBotTheme.ApplyTheme(Me)
        UpdateTabButtons(btnTabPrincipal)
        ' Initialize logger
        _logger = New RichTextBoxLogger(rtbLog)
        _logger.LogInfo("ADLOPER pornit")
        '_logger.LogInfo($"Mod lansare: {_startupConfig.LaunchMode}")
        '_logger.LogInfo($"Cale workflow: {_startupConfig.WorkflowPath}")

        _chkVisible = New CheckBox With {.Checked = False, .Visible = False}
        _chkStepByStep = New CheckBox With {.Checked = False, .Visible = False}
        _chkOnlyCheckpoints = New CheckBox With {.Checked = False, .Visible = False}

        ' Set checkbox based on DEBUG mode
#If DEBUG Then
        _chkVisible.Checked = True
        _logger.LogDebug("Mod DEBUG activ")
#End If

        ' Aplică starea inițială a butoanelor toggle conform temei curente
        UpdateToggleButtons()

        ' Load certificates
        LoadCertificates()

        ' Wire up events
        AddHandler cmbCertificates.SelectedIndexChanged, AddressOf CmbCertificates_SelectedIndexChanged
        'AddHandler txtPin.TextChanged, AddressOf TxtPin_TextChanged
        AddHandler btnRefreshCerts.Click, AddressOf BtnRefreshCerts_Click
        AddHandler btnStart.Click, AddressOf BtnStart_Click

        ' Step control buttons
        AddHandler btnStepNext.Click, AddressOf BtnStepNext_Click
        AddHandler btnStepSkip.Click, AddressOf BtnStepSkip_Click
        AddHandler btnStepStop.Click, AddressOf BtnStepStop_Click
        AddHandler btnStepPrev.Click, AddressOf BtnStepPrev_Click

        ' Update button state
        UpdateStartButtonState()

        UpdateUI(False)

        LoadWorkflows()
        InitializeWorkflowContextMenu()
        SA_UpdateButtonState()

        _browserReadyTimer = New System.Windows.Forms.Timer With {
            .Interval = 500
        }
        AddHandler _browserReadyTimer.Tick, AddressOf OnBrowserReadyTimerTick

        spltMain.Panel2Collapsed = True

    End Sub

    Private Sub LoadCertificates()
        Try
            _logger.LogDebug("Încarc certificate...")
            cmbCertificates.Items.Clear()

            _certificates = CertificateService.GetSmartcardCertificates()

            If _certificates.Count = 0 Then
                _logger.LogWarning("Nu s-au găsit certificate valide pe smartcard/token")
                'MessageBox.Show(
                '    "Nu s-au găsit certificate valide pe smartcard/token-uri conectate." & vbCrLf &
                '    "Verifică conexiunea token-ului și driverii PKCS#11.",
                '    "Informare",
                '    MessageBoxButtons.OK,
                '    MessageBoxIcon.Information)
                Return
            End If

            For Each cert In _certificates
                cmbCertificates.Items.Add(CertificateService.GetDisplayName(cert))
            Next

            If cmbCertificates.Items.Count > 0 Then
                cmbCertificates.SelectedIndex = 0
                cmbCertificates.Enabled = True
            End If

            _logger.LogSuccess($"Găsite {_certificates.Count} certificate")

        Catch ex As Exception
            _logger.LogException(ex, "Eroare la încărcare certificate")
        End Try
    End Sub

    Private Sub CmbCertificates_SelectedIndexChanged(sender As Object, e As EventArgs)
        If cmbCertificates.SelectedIndex >= 0 AndAlso _certificates IsNot Nothing AndAlso cmbCertificates.SelectedIndex < _certificates.Count Then
            Dim cert = _certificates(cmbCertificates.SelectedIndex)
            _logger.LogDebug($"Certificat selectat: {CertificateService.GetCommonName(cert)}")
        End If
        UpdateStartButtonState()
    End Sub

    Private Sub TxtPin_TextChanged(sender As Object, e As EventArgs)
        UpdateStartButtonState()
    End Sub

    Private Sub BtnRefreshCerts_Click(sender As Object, e As EventArgs)
        _logger.LogInfo(String.Join(vbCrLf, GetType(WorkflowExecutor).Assembly.GetManifestResourceNames()))
        LoadCertificates()
    End Sub

    Private Sub UpdateStartButtonState()
        ' Activăm butonul CONNECT doar dacă avem un certificat selectat
        btnConnect.Enabled = cmbCertificates.SelectedIndex >= 0
        'btnStart.Enabled = True
        'lstWorkflows.Enabled = False
    End Sub

    Private Async Sub btnConnect_Click(sender As Object, e As EventArgs) Handles btnConnect.Click
        If _isRunning Then Return

        pb1.Value = 0
        pb1.Maximum = 100
        btnConnect.Enabled = False

        Dim progressAction As Action(Of Integer, Integer) =
        Sub(currentStep, totalSteps)
            If InvokeRequired Then
                Invoke(Sub()
                           pb1.Maximum = totalSteps
                           If currentStep <= totalSteps Then pb1.Value = currentStep
                       End Sub)
            Else
                pb1.Maximum = totalSteps
                If currentStep <= totalSteps Then pb1.Value = currentStep
            End If
        End Sub

        Try
            _browserReadyTimer.Start()
            _isRunning = True
            UpdateUI(False)
            _logger.Clear()

            Dim certificate = _certificates(cmbCertificates.SelectedIndex)
            _cancellationTokenSource = New CancellationTokenSource

            Dim stealthMode = True
            Dim stepByStep = _chkStepByStep.Checked
            Dim onlyCheckpoints = _chkOnlyCheckpoints.Checked

            ' =====================================================================
            ' FAZA 1: INIȚIALIZARE EXECUTOR
            ' Connect forțează întotdeauna un executor nou (sesiune nouă)
            ' =====================================================================
            _executor = Nothing

            Dim stepCallback As Func(Of String, WorkflowExecutor.StepResult) = Nothing
            If stepByStep Then
                spltMain.Panel2Collapsed = False
                lblStepDescription.Text = "Inițializare..."
                WindowSnapper.SnapLeft(Handle)
                stepCallback = BuildStepCallback()
            Else
                spltMain.Panel2Collapsed = True
            End If

            _executor = New WorkflowExecutor(
                                            _logger, certificate, stealthMode,
                                            stepByStep, stepCallback, onlyCheckpoints,
                                            progressAction, stepByStep,
                                            _cancellationTokenSource.Token)

            WireStatusUpdate()
            If stepByStep Then WireActionStart()

            lblStatus.Text = "Lansare browser..."
            Await Task.Run(Function() _executor.LaunchAndPositionBrowserAsync)

            ' =====================================================================
            ' FAZA 2: AUTENTIFICARE
            ' =====================================================================
            _logger.LogInfo("Execut procedura de Autentificare...")
            lblStatus.Text = "Autentificare..."

            If Not File.Exists(LOGIN_WORKFLOW_PATH) Then
                Throw New Exception($"Fișierul de login lipsește: {LOGIN_WORKFLOW_PATH}")
            End If

            Dim loginXml = File.ReadAllText(LOGIN_WORKFLOW_PATH)

            WorkflowParser.Logger = _logger  ' Injectăm loggerul în parser pentru debug variabile

            Dim loginWorkflow = WorkflowParser.Parse(loginXml, LOGIN_WORKFLOW_PATH)
            _executor.SetWorkflowPath(LOGIN_WORKFLOW_PATH)

            Await Task.Run(Function() _executor.ExecuteAsync(loginWorkflow))

            _logger.LogSuccess("Autentificare reușită!")
            _logger.Separator()
            _isRunning = False

            UpdateUI(True)
            cmbCertificates.Enabled = False

        Catch ex As Exception
            'MessageBox.Show($"Eroare: {ex.Message}{vbCrLf}Browserul a rămas deschis pentru investigație.",
            '            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
            _isRunning = False
            btnConnect.Enabled = True
        Finally
            btnHistory.Enabled = True
            spltMain.Panel2Collapsed = True
            pb1.Value = 0
        End Try
    End Sub

#Region "Step Control Buttons"
    Private Sub BtnStepNext_Click(sender As Object, e As EventArgs)
        _stepResult = StepResult.Continue
        _stepWaitEvent.Set()
    End Sub

    Private Sub BtnStepSkip_Click(sender As Object, e As EventArgs)
        _stepResult = StepResult.Skip
        _stepWaitEvent.Set()
    End Sub

    Private Sub BtnStepStop_Click(sender As Object, e As EventArgs)
        _stepResult = StepResult.Stop
        _stepWaitEvent.Set()
    End Sub

    Private Sub BtnStepPrev_Click(sender As Object, e As EventArgs)
        _stepResult = StepResult.Previous
        _stepWaitEvent.Set()
    End Sub

    ''' <summary>
    ''' Callback for step-by-step execution - shows panel and waits for user input
    ''' </summary>
    Private Function ConfirmNextStep(actionDescription As String) As StepResult
        ' Update UI on UI thread
        ' Folosim Invoke pentru a fi siguri că suntem pe thread-ul corect
        Me.Invoke(Sub()
                      pnlStepControl.Visible = True
                      pnlStepControl.BringToFront()
                      ' VISUAL CUE: Galben când așteaptă confirmare
                      pnlStepControl.BackColor = Color.LightGoldenrodYellow

                      ' Textul e deja actualizat prin OnStatusUpdate, dar îl forțăm și aici
                      lblStepDescription.Text = $"► {actionDescription}"
                      lblStepDescription.Refresh()

                      btnStepNext.Enabled = True
                      btnStepSkip.Enabled = True
                      btnStepPrev.Enabled = True
                      btnStepStop.Enabled = True

                      btnStepNext.Focus()
                  End Sub)

        ' Wait for user input (Blochează thread-ul background, NU UI-ul)
        _stepWaitEvent.WaitOne()

        ' Reset UI after click
        Me.Invoke(Sub()
                      pnlStepControl.BackColor = _defaultPanelColor
                      btnStepNext.Enabled = False
                      btnStepSkip.Enabled = False
                      btnStepPrev.Enabled = False
                      btnStepStop.Enabled = False
                  End Sub)

        Return _stepResult
    End Function

    ''' <summary>
    ''' Construiește callback-ul step-by-step, traducând StepResult-ul local în cel al executorului.
    ''' </summary>
    Private Function BuildStepCallback() As Func(Of String, WorkflowExecutor.StepResult)
        Return Function(desc As String) As WorkflowExecutor.StepResult
                   Dim result = ConfirmNextStep(desc)
                   Select Case result
                       Case StepResult.Continue : Return WorkflowExecutor.StepResult.Continue
                       Case StepResult.Skip : Return WorkflowExecutor.StepResult.Skip
                       Case StepResult.Stop : Return WorkflowExecutor.StepResult.Stop
                       Case StepResult.Previous : Return WorkflowExecutor.StepResult.Previous
                       Case Else : Return WorkflowExecutor.StepResult.Continue
                   End Select
               End Function
    End Function

    ''' <summary>
    ''' (Re)conectează handlerul OnStatusUpdate la executorul curent.
    ''' RemoveHandler înainte de AddHandler previne duplicatele la re-rulări succesive.
    ''' </summary>
    Private Sub WireStatusUpdate()
        If _onStatusUpdateHandler IsNot Nothing Then
            RemoveHandler _executor.OnStatusUpdate, _onStatusUpdateHandler
        End If
        _onStatusUpdateHandler = Sub(msg)
                                     Me.Invoke(Sub()
                                                   lblStepDescription.Text = msg
                                                   lblStepDescription.Refresh()
                                               End Sub)
                                 End Sub
        AddHandler _executor.OnStatusUpdate, _onStatusUpdateHandler

        ' ← NOU: Log action → lblStatus
        If _onLogMessageHandler IsNot Nothing Then
            RemoveHandler _executor.OnLogMessage, _onLogMessageHandler
        End If
        _onLogMessageHandler = Sub(msg)
                                   Me.Invoke(Sub()
                                                 lblStatus.Text = msg
                                                 lblStatus.Refresh()
                                             End Sub)
                               End Sub
        AddHandler _executor.OnLogMessage, _onLogMessageHandler
    End Sub
#End Region

    Private Async Sub BtnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        If _isRunning Then Return

        pb1.Value = 0
        pb1.Maximum = 100

        Dim progressAction As Action(Of Integer, Integer) =
        Sub(currentStep, totalSteps)
            If InvokeRequired Then
                Invoke(Sub()
                           pb1.Maximum = totalSteps
                           If currentStep <= totalSteps Then pb1.Value = currentStep
                       End Sub)
            Else
                pb1.Maximum = totalSteps
                If currentStep <= totalSteps Then pb1.Value = currentStep
            End If
        End Sub

        Try
            _isRunning = True
            UpdateUI(False)
            _logger.Clear()

            Dim certificate = _certificates(cmbCertificates.SelectedIndex)
            _cancellationTokenSource = New CancellationTokenSource

            Dim stealthMode = Not _chkVisible.Checked
            Dim stepByStep = _chkStepByStep.Checked
            Dim onlyCheckpoints = _chkOnlyCheckpoints.Checked

            ' =====================================================================
            ' FAZA 1A: UI STEP-BY-STEP (întotdeauna, indiferent de starea browserului)
            ' =====================================================================
            Dim stepCallback As Func(Of String, WorkflowExecutor.StepResult) = Nothing
            spltMain.Panel2Collapsed = stepByStep
            pnlStepControl.Visible = stepByStep

            If stepByStep Then
                lblStepDescription.Text = "Inițializare..."
                'WindowSnapper.SnapLeft(Handle)
                stepCallback = BuildStepCallback()
                spltMain.Height = 67
            Else
                pnlStepControl.Visible = False
            End If

            ' =====================================================================
            ' FAZA 1B: EXECUTOR (nou sau refolosit)
            ' =====================================================================
            If _executor Is Nothing OrElse Not _executor.IsBrowserOpen Then
                _logger.LogInfo("Sesiune inexistentă. Se va inițializa browserul...")
                _executor = New WorkflowExecutor(
                                                _logger, certificate, stealthMode,
                                                stepByStep, stepCallback, onlyCheckpoints,
                                                progressAction, stepByStep,
                                                _cancellationTokenSource.Token)
                _executor.UpdateThrottleSettings(_currentThrottleSettings)

                lblStatus.Text = "Lansare browser..."
                Await Task.Run(Function() _executor.LaunchAndPositionBrowserAsync)
            Else
                _logger.LogInfo("Sesiune activă detectată. Refolosesc browserul.")
                _executor.IsReloaded = True
                _executor.UpdateContext(_cancellationTokenSource.Token)
                _executor.UpdateStepSettings(stepByStep, stepCallback, onlyCheckpoints)
                _executor.UpdateThrottleSettings(_currentThrottleSettings)
            End If

            ' =====================================================================
            ' FAZA 1C: WIRE HANDLERS (întotdeauna)
            ' =====================================================================
            WireStatusUpdate()
            If stepByStep Then WireActionStart()

            ' =====================================================================
            ' FAZA 2A: PRE-PARSARE TOATE WORKFLOW-URILE (pentru tree + execuție)
            ' =====================================================================
            Dim items = lstWorkflows.CheckedItems.Cast(Of CustomCheckedItem).OrderBy(Function(x) x.Index).ToList

            Dim allInputs = items.ToDictionary(
                                            Function(i) i.Text,
                                            Function(i) i.Variables.ToDictionary(Function(k) k.Key, Function(v) v.Value.Value))

            JobHistoryManager.StartJob($"Batch [{items.Count} module]", JsonConvert.SerializeObject(allInputs, Formatting.Indented))

            ' Parsăm toate workflow-urile o dată — le folosim și pentru tree și pentru execuție
            Dim parsedItems As New List(Of (Item As CustomCheckedItem, Wfl As Workflow, JsonVars As Dictionary(Of String, String)))

            For Each item In items
                Dim filePath = item.Tag.ToString
                If Not File.Exists(filePath) Then
                    _logger.LogError($"Fișier lipsă: {filePath}")
                    Continue For
                End If

                Dim xmlContent = File.ReadAllText(filePath)

                ' Variabile normale → în XML
                Dim flatVars = item.Variables.
                Where(Function(kvp) Not kvp.Value.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase)).
                ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value.Value)
                If flatVars.Count > 0 Then
                    xmlContent = WorkflowParser.ApplyVariables(xmlContent, flatVars)
                End If

                ' Variabile JSON → le salvăm separat, le injectăm înainte de execuție
                Dim jsonVars = item.Variables.
                Where(Function(kvp) kvp.Value.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase)).
                ToDictionary(Function(kvp) kvp.Key, Function(kvp) kvp.Value.Value)

                WorkflowParser.Logger = _logger  ' Injectăm loggerul în parser pentru debug variabile
                Dim wfl = WorkflowParser.Parse(xmlContent, filePath)
                parsedItems.Add((item, wfl, jsonVars))
            Next

            ' =====================================================================
            ' FAZA 2B: CONSTRUIRE TREE (doar în modul step-by-step)
            ' =====================================================================
            'If stepByStep Then
            Dim treeData = parsedItems.Select(Function(p) (Name:=p.Item.Text, p.Wfl)).ToList
            BuildAndShowStepTree(treeData)
            'End If

            ' =====================================================================
            ' FAZA 3: EXECUȚIE BATCH (din lista pre-parsată)
            ' =====================================================================
            For Each entry In parsedItems
                _logger.LogInfo($"► LANSARE MODUL: {entry.Item.Text}")
                lblStatus.Text = $"Execut: {entry.Item.Text}..."
                _executor.ResetVariables()
                _logger.LogDebug($"Resetat variabile. Rămase: {_executor.GetAllVariables.Count}")

                ' Injectăm variabilele JSON în executor
                For Each kvp In entry.JsonVars
                    _logger.LogDebug($"[JSON Var] Injectez '{kvp.Key}' ({kvp.Value.Length} chars).")
                    _executor.SetVariable(kvp.Key, kvp.Value)
                Next

                _executor.SetWorkflowPath(entry.Item.Tag.ToString)
                Await Task.Run(Function() _executor.ExecuteAsync(entry.Wfl))

                ' Output variables
                Dim outputVariables = _executor.GetAllVariables
                If outputVariables.Count > 0 Then
                    _logger.LogInfo("--- Variabile colectate din Workflow ---")
                    For Each kvp In outputVariables
                        _logger.LogInfo($"[{kvp.Key}] = {kvp.Value.Substring(0, Math.Min(kvp.Value.Length, 50))}...")
                        If kvp.Key.Contains("XML") OrElse kvp.Value.StartsWith("<Table>") Then
                            Dim fileNameOutput = $"Export_{entry.Item.Text}_{Date.Now:HHmmss}.xml"
                            Dim fullPath = Path.Combine("C:\AVACONT\Logs", fileNameOutput)
                            File.WriteAllText(fullPath, kvp.Value)
                            _logger.LogSuccess($"Tabel salvat în: {fileNameOutput}")
                        End If
                    Next
                    _logger.LogInfo("--------------------------------------")
                End If

                _logger.LogSuccess($"✓ Finalizat modulul: {entry.Item.Text}")
                Await Task.Delay(1000)
            Next

            ' Finalizare
            If _executor IsNot Nothing Then
                JobHistoryManager.SaveOutputVariables(_executor.GetAllVariables)
            End If

            JobHistoryManager.FinishJob("Succes")

            lblStatus.Text = "Toate operațiunile finalizate!"
            lblStatus.ForeColor = Color.Green
            'MessageBox.Show("Flux complet executat cu succes!", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information)

            _logger.Separator()
            _isRunning = False
            btnStart.Enabled = True
            lstWorkflows.Enabled = True
            cmbCertificates.Enabled = False

        Catch ex As Exception
            _logger.LogException(ex, "Eroare critică")
            lblStatus.Text = "Eroare (Verifică Log)"
            lblStatus.ForeColor = Color.Red
            If _executor IsNot Nothing Then
                JobHistoryManager.SaveOutputVariables(_executor.GetAllVariables)
            End If
            JobHistoryManager.FailJob(ex.Message)

            _isRunning = False
        Finally
            btnHistory.Enabled = True
            pnlStepControl.Visible = False
            btnStart.Enabled = True
            pb1.Value = 0
            HideStepTree()  ' ← restaurează lstWorkflows întotdeauna
        End Try
    End Sub

    Private Function LoadWorkflow() As Workflow
        ' (Codul tău de încărcare a rămas neschimbat, e corect)
        Try
            Dim workflowPath = "C:\AVACONT\FB_WORKFLOWS\" '_startupConfig.WorkflowPath
            If Not File.Exists(workflowPath) Then
                MessageBox.Show("Fișier lipsă: " & workflowPath)
                Return Nothing
            End If

            _logger.LogInfo($"Încarc workflow: {workflowPath}")
            Dim content = File.ReadAllText(workflowPath)
            WorkflowParser.Logger = _logger ' Injectăm loggerul în parser pentru debug variabile
            Dim workflow = WorkflowParser.Parse(content, workflowPath)

            Return workflow
        Catch ex As Exception
            _logger.LogException(ex, "Eroare load workflow")
            Return Nothing
        End Try
    End Function

    Private Sub UpdateUI(enabled As Boolean)
        cmbCertificates.Enabled = Not enabled
        btnSaveSendAction.Enabled = enabled
        btnHistory.Enabled = enabled
        btnStart.Enabled = enabled
    End Sub

    Private Async Sub KBOT_STANDALONE_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _executor IsNot Nothing Then
            Await _executor.CloseAsync() ' Asta rulează pe UI thread, e ok
        End If
        _cancellationTokenSource?.Cancel()
        _stepWaitEvent.Set() ' Deblocăm orice așteptare ca să se închidă aplicația
    End Sub

    Private Sub BtnOnlyCheckpoints_Click(sender As Object, e As EventArgs) Handles btnOnlyCheckpoints.Click
        _chkOnlyCheckpoints.Checked = Not _chkOnlyCheckpoints.Checked
        UpdateToggleButtons()
    End Sub

    Private Sub BtnStepByStep_Click(sender As Object, e As EventArgs) Handles btnStepByStep.Click
        _chkStepByStep.Checked = Not _chkStepByStep.Checked
        ' Dezactivăm OnlyCheckpoints când ieșim din modul Pas cu Pas
        If Not _chkStepByStep.Checked Then _chkOnlyCheckpoints.Checked = False
        UpdateToggleButtons()
    End Sub

    ''' <summary>
    ''' Re-aplică culorile butoanelor toggle (👣 și 🚩) în funcție de starea lor
    ''' curentă și de tema activă. Apelat la click și la schimbarea temei.
    ''' </summary>
    Private Sub UpdateToggleButtons()
        Dim offColor = If(KBotTheme.IsDark, KBotTheme.CLR_BTN, SystemColors.Control)
        Dim onColor = Color.FromArgb(180, 50, 50)   ' roșu vizibil în ambele teme

        btnStepByStep.BackColor = If(_chkStepByStep.Checked, onColor, offColor)
        btnStepByStep.UseVisualStyleBackColor = False

        btnOnlyCheckpoints.Enabled = _chkStepByStep.Checked
        btnOnlyCheckpoints.BackColor = If(_chkOnlyCheckpoints.Checked, onColor, offColor)
        btnOnlyCheckpoints.UseVisualStyleBackColor = False
    End Sub

    Private Sub LoadWorkflows(Optional folderPath As String = "C:\AVACONT\FB_WORKFLOWS")
        Try
            lstWorkflows.Items.Clear()

            If _currentWflFolder <> folderPath Then _currentWflFolder = folderPath

            If Not Directory.Exists(_currentWflFolder) Then Directory.CreateDirectory(_currentWflFolder)

            Dim files = Directory.GetFiles(_currentWflFolder, "*.wfl")

            Dim indexCounter As Integer = 1

            For Each filePath In files
                Dim fileName = Path.GetFileNameWithoutExtension(filePath)

                ' Sărim peste scriptul de Login dacă e gestionat separat (opțional)
                If fileName.ToLower().Equals(Path.GetFileNameWithoutExtension(LOGIN_WORKFLOW_PATH).ToLower()) Then Continue For

                ' 1. Citim conținutul pentru a găsi variabilele
                Dim content As String = File.ReadAllText(filePath)

                Dim detectedVars As Dictionary(Of String, WorkflowVariable) = WorkflowParser.ExtractVariablesDetailed(content)

                ' 2. Creăm obiectul
                Dim item As New CustomCheckedItem With {
                    .Index = indexCounter,
                    .Text = fileName.Replace("adlop - ", ""),
                    .Tag = filePath,
                    .Variables = detectedVars
                }

                ' 4. Adăugăm în listă
                lstWorkflows.Items.Add(item)
                indexCounter += 1
            Next

        Catch ex As Exception
            _logger.LogError("Eroare la încărcare workflow-uri: " & ex.Message)
        End Try
    End Sub

    Private Sub LstWorkflows_MouseDown(sender As Object, e As MouseEventArgs) Handles lstWorkflows.MouseDown
        If e.Button = MouseButtons.Right Then
            Dim index = lstWorkflows.IndexFromPoint(e.Location)
            If index = ListBox.NoMatches Then Return
            lstWorkflows.SelectedIndex = index
            _contextMenuIndex = index
            _contextMenuWorkflow.Show(lstWorkflows, e.Location)
        End If
    End Sub

    Private Sub LstWorkflows_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstWorkflows.ItemCheck
        Try
            If e.NewValue = CheckState.Checked Then
                Dim item = DirectCast(lstWorkflows.Items(e.Index), CustomCheckedItem)

                If item.Variables.Count > 0 Then
                    Dim varForm As New VariablesInputForm(item.Variables)
                    Dim result = varForm.ShowDialog(Me)
                    If result <> DialogResult.OK Then
                        e.NewValue = CheckState.Unchecked
                        Return
                    End If
                    ' item.Variables a fost actualizat in-place de VariablesInputForm
                End If

            ElseIf e.NewValue = CheckState.Unchecked Then
                ' Resetăm valorile
                Dim item = DirectCast(lstWorkflows.Items(e.Index), CustomCheckedItem)
                For Each v In item.Variables.Values
                    v.Value = ""
                Next
            End If
        Catch ex As Exception
            _logger.LogError("Eroare la gestionare variabile: " & ex.Message)
        End Try
    End Sub

    ' Apelat din progress callback sau dintr-un timer existent
    Private Sub SyncBrowserCheckbox()
        If _executor Is Nothing Then Return
        _chkVisible.Checked = _executor.IsBrowserVisible

        btnAfiseazaBrowser.Enabled = _executor.IsBrowserOpen

        If _chkVisible.Checked Then
            btnAfiseazaBrowser.Text = "🚫"
            btnAfiseazaBrowser.BackColor = Color.LightCoral
        Else
            btnAfiseazaBrowser.Text = "🌐"
            btnAfiseazaBrowser.BackColor = SystemColors.Control
        End If
    End Sub

    Private Sub BtnHistory_Click(sender As Object, e As EventArgs) Handles btnHistory.Click
        Dim hForm As New HistoryForm
        hForm.Show()
    End Sub

    Private Sub InitializeWorkflowContextMenu()
        _contextMenuWorkflow = New ContextMenuStrip()

        Dim menuEditeaza = New ToolStripMenuItem("✏️ Editează workflow")
        Dim menuDuplica = New ToolStripMenuItem("📋 Duplică workflow")
        Dim menuDeschideLocatie = New ToolStripMenuItem("📂 Deschide locație fișier")
        Dim menuReincarca = New ToolStripMenuItem("🔄 Reîncarcă lista")
        Dim menuSterge = New ToolStripMenuItem("🗑️ Șterge workflow") With {
            .ForeColor = Color.Crimson
        }

        AddHandler menuEditeaza.Click, AddressOf MenuEditeaza_Click
        AddHandler menuDuplica.Click, AddressOf MenuDuplica_Click
        AddHandler menuDeschideLocatie.Click, AddressOf MenuDeschideLocatie_Click
        AddHandler menuReincarca.Click, AddressOf MenuReincarca_Click
        AddHandler menuSterge.Click, AddressOf MenuSterge_Click

        _contextMenuWorkflow.Items.Add(menuEditeaza)
        _contextMenuWorkflow.Items.Add(menuDuplica)
        _contextMenuWorkflow.Items.Add(New ToolStripSeparator())
        _contextMenuWorkflow.Items.Add(menuDeschideLocatie)
        _contextMenuWorkflow.Items.Add(menuReincarca)
        _contextMenuWorkflow.Items.Add(New ToolStripSeparator())
        _contextMenuWorkflow.Items.Add(menuSterge)
    End Sub

    Private Sub MenuEditeaza_Click(sender As Object, e As EventArgs)
        If _contextMenuIndex < 0 Then Return
        Dim item = DirectCast(lstWorkflows.Items(_contextMenuIndex), CustomCheckedItem)
        Dim filePath = item.Tag?.ToString()
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then
            MessageBox.Show($"Fișierul workflow nu a fost găsit:{Environment.NewLine}{filePath}",
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using editor As New WorkflowEditorForm(filePath)
            editor.ShowDialog(Me)
        End Using
        LoadWorkflows(_currentWflFolder)
    End Sub

    Private Sub MenuDuplica_Click(sender As Object, e As EventArgs)
        If _contextMenuIndex < 0 Then Return
        Dim item = DirectCast(lstWorkflows.Items(_contextMenuIndex), CustomCheckedItem)
        Dim filePath = item.Tag?.ToString()
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then
            MessageBox.Show("Fișierul sursă nu a fost găsit.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim folder = Path.GetDirectoryName(filePath)
        Dim ext = Path.GetExtension(filePath)
        Dim defaultName = Path.GetFileNameWithoutExtension(filePath) & "_copy"

        Dim newName = InputBox("Introduceți numele noului workflow:", "Duplicare workflow", defaultName)
        If String.IsNullOrWhiteSpace(newName) Then Return

        ' Adaugă extensia dacă utilizatorul nu a pus-o
        If Not newName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) Then
            newName &= ext
        End If

        Dim destPath = Path.Combine(folder, newName)
        If File.Exists(destPath) Then
            Dim overwrite = MessageBox.Show("Există deja un fișier cu acest nume. Suprascrii?",
                                            "Confirmare", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If overwrite <> DialogResult.Yes Then Return
        End If

        File.Copy(filePath, destPath, overwrite:=True)
        _logger.LogInfo($"Workflow duplicat: {destPath}")
        LoadWorkflows()
    End Sub

    Private Sub MenuDeschideLocatie_Click(sender As Object, e As EventArgs)
        If _contextMenuIndex < 0 Then Return
        Dim item = DirectCast(lstWorkflows.Items(_contextMenuIndex), CustomCheckedItem)
        Dim filePath = item.Tag?.ToString()
        If String.IsNullOrEmpty(filePath) OrElse Not File.Exists(filePath) Then Return

        Process.Start("explorer.exe", $"/select,""{filePath}""")
    End Sub

    Private Sub MenuReincarca_Click(sender As Object, e As EventArgs)
        LoadWorkflows()
        _logger.LogInfo("Lista de workflows reîncărcată.")
    End Sub

    Private Sub MenuSterge_Click(sender As Object, e As EventArgs)
        If _contextMenuIndex < 0 Then Return
        Dim item = DirectCast(lstWorkflows.Items(_contextMenuIndex), CustomCheckedItem)
        Dim filePath = item.Tag?.ToString()

        Dim confirm = MessageBox.Show($"Ești sigur că vrei să ștergi:{Environment.NewLine}{Path.GetFileName(filePath)}?",
                                      "Confirmare ștergere", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If confirm <> DialogResult.Yes Then Return

        Try
            File.Delete(filePath)
            _logger.LogInfo($"Workflow șters: {filePath}")
            LoadWorkflows()
        Catch ex As Exception
            MessageBox.Show($"Eroare la ștergere: {ex.Message}", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnChangeFolder_Click(sender As Object, e As EventArgs) Handles btnChangeFolder.Click
        Using dlg As New FolderBrowserDialog With {
        .Description = "Selectează folderul cu fișiere .wfl",
        .SelectedPath = "C:\AVACONT\FB_WORKFLOWS"
    }
            If dlg.ShowDialog = DialogResult.OK Then
                LoadWorkflows(dlg.SelectedPath)
                _logger.LogInfo($"Folder schimbat: {dlg.SelectedPath}")
            End If
        End Using
    End Sub

    Private Sub CmbThrottle_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbThrottle.SelectedIndexChanged
        ' Dacă e Custom, deschidem dialogul
        If cmbThrottle.SelectedIndex = 4 Then  ' "Custom..."
            Dim dlg As New ThrottleCustomDialog
            If dlg.ShowDialog(Me) = DialogResult.OK Then
                _currentThrottleSettings = dlg.Result
                ' Înlocuim "Custom..." cu label-ul complet în combobox
                ' (fără să adăugăm un item permanent - păstrăm indexul 4)
            Else
                ' Userul a anulat - revenim la "Niciun throttle"
                cmbThrottle.SelectedIndex = 0
            End If
        Else
            _currentThrottleSettings = GetThrottleFromIndex(cmbThrottle.SelectedIndex)
        End If

        ' Dacă executorul e activ, aplicăm imediat setările pentru URMĂTOAREA rulare
        _executor?.UpdateThrottleSettings(_currentThrottleSettings)
    End Sub

    Private Function GetThrottleFromIndex(index As Integer) As ThrottleSettings
        Select Case index
            Case 0 : Return ThrottleSettings.None
            Case 1 : Return ThrottleSettings.Fast3G
            Case 2 : Return ThrottleSettings.Slow3G
            Case 3 : Return ThrottleSettings.TwoG
            Case Else : Return ThrottleSettings.None
        End Select
    End Function

    Private Async Sub btnAfiseazaBrowser_Click(sender As Object, e As EventArgs) Handles btnAfiseazaBrowser.Click
        ' Dacă executorul nu e pornit, nu facem nimic și resetăm bifa vizual
        If _executor Is Nothing Then Return

        _chkVisible.Checked = Not _chkVisible.Checked

        If _chkVisible.Checked Then
            Await _executor.ShowBrowserWindowAsync
            btnAfiseazaBrowser.Text = "🚫"
            btnAfiseazaBrowser.BackColor = Color.LightCoral
        Else
            Await _executor.HideBrowserWindowAsync
            btnAfiseazaBrowser.Text = "🌐"
            btnAfiseazaBrowser.BackColor = SystemColors.Control
        End If
    End Sub

    Private Sub OnStepPanelVisibilityChanged(sender As Object, e As EventArgs)
        Dim delta As Integer = pnlStepControl.Height
        If pnlStepControl.Visible Then
            Me.ClientSize = New System.Drawing.Size(
                Me.ClientSize.Width,
                Me.ClientSize.Height + delta)
        Else
            Me.ClientSize = New System.Drawing.Size(
                Me.ClientSize.Width,
                Me.ClientSize.Height - delta)
        End If
    End Sub

    Private Sub OnBrowserReadyTimerTick(sender As Object, e As EventArgs)
        If _executor Is Nothing Then Return

        If _executor.IsBrowserOpen Then
            ' Browserul e gata — sincronizăm checkbox-ul și oprim timer-ul
            SyncBrowserCheckbox()

            ' Oprim polling-ul: nu mai e nevoie să verificăm permanent
            ' (SyncBrowserCheckbox e apelat și din alte locuri când e nevoie)
            If Not _isRunning Then
                _browserReadyTimer.Stop()
            End If
        End If
    End Sub

    Private Sub KBOT_STANDALONE_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        _browserReadyTimer?.Stop()
        _browserReadyTimer?.Dispose()
    End Sub

    Private Sub _executor_OnBrowserClosed(message As String) Handles _executor.OnBrowserClosed
        If Me.InvokeRequired Then
            Me.Invoke(Sub() SaveLogAutomatic())
            Me.Invoke(Sub() ManualSaveResendPackage())
        Else
            SaveLogAutomatic()
            ManualSaveResendPackage()
        End If

        Application.Exit()
    End Sub

    ''' <summary>
    ''' Salvează automat log-ul curent în _jobFolderPath, fără dialog.
    ''' Apelată la comanda CMD_SAVE_LOG din VBA.
    ''' </summary>
    Private Sub SaveLogAutomatic()
        Try
            ' Folder destinație: același cu job folder-ul, sau lângă exe ca fallback
            Dim folder As String = ""
            folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

            Dim fileName As String = $"ForexeBot_Log_{DateTime.Now:yyyyMMdd_HHmmss}.rtf"
            Dim filePath As String = Path.Combine(folder, fileName)

            ' Salvăm RTF direct din control (cu culori)
            rtbLog.SaveFile(filePath, RichTextBoxStreamType.RichText)

            _logger?.LogSuccess($"[LOG] Log salvat automat: {filePath}")

        Catch ex As Exception
            _logger?.LogError($"[LOG] Eroare la salvarea automată: {ex.Message}")
        End Try
    End Sub

    Friend Sub ManualSaveResendPackage()
        Dim lastJob As JobHistoryItem = JobHistoryManager.History.
            AsEnumerable().Reverse().
            FirstOrDefault(Function(j) j.SentPipeMessages IsNot Nothing AndAlso
                                       j.SentPipeMessages.Count > 0)

        If lastJob Is Nothing Then
            MessageBox.Show("Nu există mesaje pipe salvate în sesiunea curentă.",
                            "Resend — Nimic de salvat",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If String.IsNullOrEmpty(_jobFolderPath) Then
            MessageBox.Show("Nu este configurat un folder pentru job. Nu pot salva.",
                                "Resend — Folder lipsă",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If


        Try
            Dim resendDir As String = Path.Combine(_jobFolderPath, ResendSubfolderName)
            If Not Directory.Exists(resendDir) Then Directory.CreateDirectory(resendDir)

            Dim safeName As String = SanitizeFileName(lastJob.JobName)
            Dim fileName As String = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}{ResendExtension}"
            Dim filePath As String = Path.Combine(resendDir, fileName)

            Dim pkg As New ResendPackage With {
                .JobName = lastJob.JobName,
                .SavedAt = DateTime.Now,
                .Messages = lastJob.SentPipeMessages
            }

            File.WriteAllText(filePath,
                              JsonConvert.SerializeObject(pkg, Formatting.Indented),
                              System.Text.Encoding.UTF8)

            _logger?.LogSuccess($"[RESEND] Pachet salvat: {fileName}")

            MessageBox.Show($"Salvat în:{Environment.NewLine}{filePath}",
                        "Resend — Salvat cu succes",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            _logger?.LogWarning($"[RESEND] Eroare la salvare: {ex.Message}")
            MessageBox.Show($"Eroare la salvare:{Environment.NewLine}{ex.Message}",
                            "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function SanitizeFileName(name As String) As String
        If String.IsNullOrWhiteSpace(name) Then Return "job"
        Dim invalid As Char() = Path.GetInvalidFileNameChars()
        Return New String(name.Select(
            Function(c) If(Array.IndexOf(invalid, c) >= 0, "_"c, c)).ToArray())
    End Function

    Private Sub btnForexeSNM_Click(sender As Object, e As EventArgs) Handles btnForexeSNM.Click
        Try
            Dim cms As New ContextMenuStrip

            Dim itemDescarca As New ToolStripMenuItem("Descarcă extrase")

            AddHandler itemDescarca.Click, AddressOf DescarcaExtrase

            cms.Items.Add(itemDescarca)

            cms.Items.Add(New ToolStripSeparator)

            Dim itemReplayFromDump As New ToolStripMenuItem("Ruleaza din dump")

            AddHandler itemReplayFromDump.Click, AddressOf ReplayFromDump

            cms.Items.Add(itemReplayFromDump)
            ' afișare la poziția butonului
            cms.Show(btnForexeSNM, New Point(0, btnForexeSNM.Height))

        Catch ex As Exception
            _logger.LogException(ex, "btnForexeSNM_Click")
        End Try
    End Sub

    Async Sub DescarcaExtrase()
        Dim resultArray As Newtonsoft.Json.Linq.JArray = Nothing

        Try
            ' 1. selectare dată
            Dim dtDialog As New DateTimePickerDialog()

            If dtDialog.ShowDialog() <> DialogResult.OK Then Exit Sub

            Dim dataStart As DateTime = dtDialog.SelectedDate

            ' 2. apel funcție principală
            resultArray = Await ForexeSNM.DescarcaExtraseForexeAPI(
                                            page:=_executor.CurrentPage,
                                            logger:=_logger,
                                            downloadFolder:=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads\ExtraseForexe"),
                                            dataDeLa:=dataStart,
                                            dumpFolder:=dumpFolder)

            _logger.LogSuccess($"Descarcare finalizata. {resultArray.Count} extrase descărcate.")

        Catch ex As Exception
            _logger.LogException(ex, "DescarcaExtrase")

        Finally
            Dim hItem As JObject = Nothing

            If resultArray IsNot Nothing Then
                Try
                    hItem = ForexeHistoryBuilder.BuildHistoryObject("Extrase", resultArray, _logger)

                Catch ex As Exception
                    _logger.LogException(ex, "Procesare rezultate Forexe")
                End Try

                Try
                    If hItem IsNot Nothing Then
                        ForexeHistoryBuilder.ToJobHistoryItem(hItem, _logger)
                    End If

                Catch ex As Exception
                    _logger.LogException(ex, "Salvare istoric Forexe")
                End Try

            End If

        End Try
    End Sub

    Sub ReplayFromDump()
        Try
            Dim dlg As New OpenFolderDialog With {
                .Title = "Selectează folderul dump",
                .DefaultDirectory = dumpFolder
            }

            If dlg.ShowDialog() = DialogResult.OK Then
                dumpFolder = dlg.FolderName
            End If

            If dumpFolder Is Nothing Then
                _logger.LogWarning("Replay din dump anulat: nu a fost selectat niciun folder.")
                Return
            End If

            btnHistory.Enabled = True
            Dim job As JobHistoryItem = Nothing
            Dim jResults As JObject = Nothing

            Try
                Dim results = ForexeSNM.ReplayExtraseFromDump(dumpFolder, _logger)
                If results IsNot Nothing Then
                    jResults = ForexeHistoryBuilder.BuildHistoryObject("EXTRASE_DUMP", results, _logger)
                Else
                    _logger.LogError("Nu am gasit rezultate de adaugat in Manager!")
                    Return
                End If

                job = ForexeHistoryBuilder.ToJobHistoryItem(jResults, _logger)

            Catch ex As Exception
                _logger.LogException(ex, "ReplayDump")
            End Try

            Try
                If job IsNot Nothing Then
                    JobHistoryManager.History.Add(job)
                End If
            Catch ex As Exception
                _logger.LogException(ex, "Adaugare in Manager")
            End Try
        Catch ex As Exception
            _logger.LogException(ex, "ReplayFromDump")
        End Try
    End Sub

    Private Async Sub btnWicketMonitor_Click(sender As Object, e As EventArgs) _
       Handles btnWicketMonitor.Click

        Try
            If _executor Is Nothing Then
                MessageBox.Show(
                    "Browserul nu este activ.",
                    "Wicket Monitor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return
            End If

            ' Activăm monitorizarea dacă nu e deja activă
            If Not _executor.WicketMonitoringActive Then
                Await _executor.StartWicketMonitoringAsync()
            End If

            ' Creare sau refolosire fereastră singleton
            If _wicketMonitorForm Is Nothing OrElse _wicketMonitorForm.IsDisposed Then
                _wicketMonitorForm = New WicketMonitorForm()
            End If

            ' Reataș executorul curent
            _wicketMonitorForm.AttachExecutor(_executor)

            _wicketMonitorForm.Show()
            _wicketMonitorForm.BringToFront()

        Catch ex As Exception
            ' silently ignore — monitorizarea e opțională
        End Try
    End Sub
End Class