Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports GeneralClasses
Imports Newtonsoft.Json
Imports WorkflowModels

Partial Public Class KBOT_IPC
    ' =========================
    ' FLUX PRINCIPAL & EXECUȚIE
    ' =========================
    Private Async Function ExecuteConnectionJob(targetJobFile As String) As Task(Of Boolean)
        If _isBusy Then Return False
        _isBusy = True
        _isConnecting = True

        Try
            Dim v = Await ProccessConnectionJob(targetJobFile)
            Return v

        Catch ex As Exception
            _logger.LogException(ex, "Eroare Generală")
            SendMessageToPipe(PipeCmd.JOB_ERROR, 0, "CONECTARE", $"Eroare critică: {ex.Message}")
            Return False

        Finally
            _isBusy = False
            _isConnecting = False
        End Try
    End Function

    ' =========================
    ' LOGICA BATCH (DIN FOLDER/PIPE)
    ' =========================
    Private Async Function RunJobFromFolder() As Task(Of Boolean)
        ' 1. Verificare folder
        Dim jsonFiles = Directory.GetFiles(_jobFolderPath, "*.json")
        If jsonFiles.Length <> 1 Then
            Dim msg = $"[BATCH] Aștept exact un fișier job în folderul '{_jobFolderPath}'. Găsite: {jsonFiles.Length}"
            _logger.LogError(msg)
            SendMessageToPipe(PipeCmd.JOB_ERROR, 0, "", msg)
            Return False
        End If

        Dim currentJobPath As String = jsonFiles(0)

        If Not File.Exists(currentJobPath) Then Throw New Exception("Fișier job inexistent!")

        Dim jsonContent As String = File.ReadAllText(currentJobPath)
        Dim partialJob = JsonConvert.DeserializeObject(Of RobotJob)(jsonContent)
        _currentJobName = partialJob.JobName

        Try
            btnAnulare.Visible = True

            _logger.LogInfo($"[BATCH] Procesez: {currentJobPath}")

            ' 3. CONSTRUIM JOBUL EFECTIV
            ' MODIFICARE: CERTIFICATUL SI PIN-UL NU MAI VIN DIN CONFIGURATIE, CI VOR FI SELECTATE MANUAL
            Dim jobToRun As New RobotJob With {
                .ShowBrowser = _currentJobConfig.ShowBrowser, ' Atentie: Daca primul job a fost Hidden si asta e Visible, nu se va schimba starea ferestrei la cald!
                .ShowLogs = _currentJobConfig.ShowLogs,
                .LogToFolder = _currentJobConfig.LogToFolder,
                .JobFolder = _currentJobConfig.JobFolder,
                .OutputFile = _currentJobConfig.OutputFile,
                .Tasks = partialJob.Tasks,
                .JobName = partialJob.JobName
            }
            JobHistoryManager.StartJob(currentJobPath, jsonContent)

            ' 4. APELAM LOGICA COMUNA
            Dim result = Await ExecuteCoreWorkflowAsync(jobToRun)

            If Not result Then Return False

            ' 5. POST-EXECUTIE
            _logger.LogSuccess("Job din Pipe finalizat.")

            SendMessageToPipe(PipeCmd.JOB_SUCCESS, 0, _currentJobName, "Execuție completă.")

            JobHistoryManager.FinishJob("Succes")

            Try
                File.Delete(currentJobPath)
            Catch ex As Exception
                _logger.LogWarning($"Nu am putut șterge fișierul job procesat: {ex.Message}")
            End Try

            ' === 2. CAPTURĂM ERORILE REALE (Crash-uri) ===
        Catch ex As Exception
            ' AICI intră doar dacă crapă ceva neașteptat
            _logger.LogException(ex, "Eroare Execuție Job din Folder")

            ' Trimitem la ACCESS mesajul de eroare
            SendMessageToPipe(PipeCmd.JOB_ERROR, _currentTaskId, _currentJobName, ex.Message)
            JobHistoryManager.FailJob(ex.Message)
            WriteErrorResult(_currentJobConfig?.OutputFile, ex.Message)

            Return False

        Finally
            _isBusy = False
            _currentTaskId = 0
            btnAnulare.Visible = False

            If Me.InvokeRequired Then
                Me.Invoke(Sub()
                              pbProgress.Value = 0
                          End Sub)
            Else
                pbProgress.Value = 0
            End If

            _executor.ClearAllVariables()
        End Try

        Return True
    End Function

    ' =========================
    ' LOGICA SINGLE (DIRECT / CLI)
    ' =========================
    Private Async Function ProccessConnectionJob(currentJobPath As String) As Task(Of Boolean)
        Dim job As RobotJob = Nothing

        Try
            _logger.LogInfo($"[BATCH] Procesez: {currentJobPath}")

            If Not File.Exists(currentJobPath) Then
                System.Windows.Forms.MessageBox.Show("Fișier job inexistent: " & currentJobPath, "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Throw New Exception("Fișier job inexistent!")
            End If

            Dim jsonContent As String = File.ReadAllText(currentJobPath)
            job = JsonConvert.DeserializeObject(Of RobotJob)(jsonContent)
            _currentJobConfig = job ' Actualizăm configurația globală

            ' Setup Logger UI
            If job.ShowLogs Then
                lblTitleLog.Visible = True
                rtbLog.Visible = True
            End If

            ' Logging în fișier
            If Not String.IsNullOrEmpty(job.LogToFolder) Then
                If Not Directory.Exists(job.LogToFolder) Then Directory.CreateDirectory(job.LogToFolder)
                Dim safeCertName As String = Regex.Replace(job.Certificat, "[^a-zA-Z0-9]", "")
                _logger.LogFilePath = Path.Combine(job.LogToFolder, $"Log_{safeCertName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt")
            End If

            _logger.LogSuccess("Job incărcat cu succes.")

            Return Await ExecuteCoreWorkflowAsync(job)

        Catch ex As Exception
            _logger.LogException(ex, "Eroare Execuție Job")
            lblStatus.Text = "Eroare!"

            ' Scriere eroare in output file (folosind helper)
            WriteErrorResult(job?.OutputFile, ex.Message)
            Return False

        Finally
            ' Cleanup specific Single: Se inchide doar daca nu e Server
            If Not _isServer Then
                If _executor IsNot Nothing Then
                    Dim unused = _executor.CloseAsync()
                    _executor = Nothing
                End If
            End If
        End Try
    End Function

    ' ==================================================================================
    ' CORE WORKFLOW - LOGICA COMUNA
    ' ==================================================================================
    Private Async Function ExecuteCoreWorkflowAsync(jobToRun As RobotJob) As Task(Of Boolean)
        Dim lstWorkflows As New ListBox
        _isBusy = True
        ' UI List Update
        lstWorkflows.Items.Clear()
        If jobToRun.Tasks IsNot Nothing Then
            For Each t In jobToRun.Tasks
                lstWorkflows.Items.Add(Path.GetFileName(t.Path))
            Next
        End If


        If SetCertificateAndPin(jobToRun) Is Nothing Then
            Throw New Exception("Eroare setare certificat și PIN.")
        End If

        Try
            If _cancellationTokenSource IsNot Nothing Then _cancellationTokenSource.Dispose()
            _cancellationTokenSource = New CancellationTokenSource()

        Catch ex As Exception
            _logger.LogError($"Eroare inițializare token anulare: {ex.Message}")
            SendMessageToPipe(PipeCmd.JOB_ERROR, 0, _currentJobName, $"Eroare internă: {ex.Message}")
            Return False
        End Try

        Dim progressAction As Action(Of Integer, Integer) = Sub(curr, tot)
                                                                If InvokeRequired Then
                                                                    Invoke(Sub()
                                                                               pbProgress.Maximum = tot
                                                                               pbProgress.Value = Math.Min(curr, tot)
                                                                           End Sub)
                                                                Else
                                                                    pbProgress.Maximum = tot
                                                                    pbProgress.Value = Math.Min(curr, tot)
                                                                End If
                                                            End Sub

        Dim isStealth As Boolean

        Try
            ' Initializare Executor
            If _executor Is Nothing Then
                _logger.LogInfo("[EXECUTOR] Pornesc o instanță nouă de Browser...")
                isStealth = Not jobToRun.ShowBrowser
                _executor = New WorkflowExecutor(_logger, _cachedCert, isStealth, False, Nothing, False, progressAction, False, _cancellationTokenSource.Token)
                lblStatus.Text = "Lansare browser..."
                Await _executor.LaunchAndPositionBrowserAsync()
            Else
                _logger.LogInfo("[EXECUTOR] Refolosesc instanța de Browser existentă.")
                _executor.UpdateContext(_cancellationTokenSource.Token)
            End If

            WireLogMessage()

        Catch ex As Exception
            _logger.LogError($"Eroare inițializare executor: {ex.Message}")
            SendMessageToPipe(PipeCmd.JOB_ERROR, 0, _currentJobName, $"Eroare inițializare executor: {ex.Message}")
            Return False
        End Try

        ' Pornim gardianul de UI (monitorizare PIN) pe un fir separat
        StartUiGuardian(_cancellationTokenSource.Token)

        ' Iterare Task-uri
        Dim taskIndex As Integer = 0
        Dim taskId As Integer = 0
        Dim wrkFlow As Workflow

        For Each taskInfo In jobToRun.Tasks
            _currentTaskId = taskInfo.TaskId
            Try
                lstWorkflows.SelectedIndex = taskIndex
                lblStatus.Text = $"Execut: {Path.GetFileName(taskInfo.Path)}..."

                If Not File.Exists(taskInfo.Path) Then
                    _logger.LogError($"Fișier lipsă: {taskInfo.Path}")
                    taskIndex += 1
                    Continue For
                End If

                Dim xmlContent As String = File.ReadAllText(taskInfo.Path)

                ' varMeta calculat mereu — necesar atât pentru substituție cât și pentru validare
                Dim varMeta = WorkflowParser.ExtractVariablesDetailed(xmlContent)

                If taskInfo.Vars IsNot Nothing Then
                    ' ==============================================================
                    ' ÎNLOCUIRE VARIABILE — SEPARAT PE TIP (ca în KBOT_STANDALONE)
                    ' Variabilele Text/Numeric  → înlocuite direct în XML
                    ' Variabilele JSON          → injectate în executor (SetVariable)
                    '                             ca să nu spargă structura XML
                    ' ==============================================================
                    Dim flatVars As New Dictionary(Of String, String)
                    For Each kvp In taskInfo.Vars
                        Dim meta As WorkflowVariable = Nothing
                        Dim isJson = varMeta.TryGetValue(kvp.Key, meta) AndAlso
                        meta.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase)

                        If isJson Then
                            _logger.LogDebug($"[JSON Var] Injectez '{kvp.Key}' direct în executor.")
                            _executor.SetVariable(kvp.Key, kvp.Value)
                        Else
                            flatVars(kvp.Key) = kvp.Value
                        End If
                    Next

                    If flatVars.Count > 0 Then
                        xmlContent = WorkflowParser.ApplyVariables(xmlContent, flatVars)
                    End If
                End If

                ' ==============================================================
                ' VALIDARE: nicio variabilă {{}} nerezolvată nu trece mai departe
                ' JSON vars sunt excluse — ele rămân intenționat în XML și sunt
                ' injectate separat în executor via SetVariable de mai sus
                ' ==============================================================
                Dim unresolvedMatches = Regex.Matches(xmlContent, "\{\{!?\s*([^|{}]+?)(?:\|[^}]*)?\}\}")
                Dim unresolvedNames = unresolvedMatches.Cast(Of Match)().
                                                            Select(Function(m) m.Groups(1).Value.Trim()).
                                                            Distinct().
                                                            Where(Function(name)
                                                                      Dim meta As WorkflowVariable = Nothing
                                                                      Return Not (varMeta.TryGetValue(name, meta) AndAlso
                                                                                    meta.VarType.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                                                                  End Function).
                                                            ToList()

                If unresolvedNames.Count > 0 Then
                    Dim msg = $"Variabile nerezolvate în '{Path.GetFileName(taskInfo.Path)}': {String.Join(", ", unresolvedNames)}"
                    _logger.LogError($"[VARS] {msg}")
                    SendMessageToPipe(PipeCmd.JOB_ERROR, _currentTaskId, _currentJobName, msg)
                    Return False
                End If

                WorkflowParser.Logger = _logger

                wrkFlow = WorkflowParser.Parse(xmlContent, taskInfo.Path)

                taskId = taskInfo.TaskId

                _executor.SetWorkflowPath(taskInfo.Path)
                Await _executor.ExecuteAsync(wrkFlow)

                If taskInfo.Receive Then
                    _logger.LogInfo($"[PIPE] Workflow '{wrkFlow.Name}' are Receive=True. Procesez și trimit datele...")

                    ' 1. Extragem variabilele curente (snapshot la acest moment)
                    Dim currentRaw = _executor.GetAllVariables()
                    Dim currentProcessed As New Dictionary(Of String, Object)()

                    ' 2. Procesăm JSON-urile (Logica mutată aici)
                    For Each kvp In currentRaw
                        Try
                            If Not String.IsNullOrEmpty(kvp.Value) AndAlso (kvp.Value.Trim().StartsWith("["c) OrElse kvp.Value.Trim().StartsWith("{"c)) Then
                                currentProcessed.Add(kvp.Key, Newtonsoft.Json.Linq.JToken.Parse(kvp.Value))
                            Else
                                currentProcessed.Add(kvp.Key, kvp.Value)
                            End If
                        Catch
                            currentProcessed.Add(kvp.Key, kvp.Value)
                        End Try
                    Next

                    ' 3. Trimitem imediat la Access
                    ' Injectăm metadate pentru reconstrucție resend (prefixate cu _ pentru a fi ignorate ușor în VBA)
                    currentProcessed("_wfl_name") = If(String.IsNullOrEmpty(taskInfo.Name),
                                   Path.GetFileNameWithoutExtension(taskInfo.Path),
                                   taskInfo.Name)
                    currentProcessed("_wfl_path") = taskInfo.Path

                    ' Injectam si vars pentru reconstructie resend
                    If taskInfo.Vars IsNot Nothing AndAlso taskInfo.Vars.Count > 0 Then
                        currentProcessed("_wfl_vars") = taskInfo.Vars
                    End If

                    If Not _isConnecting Then SendMessageToPipe(PipeCmd.WORKFLOW_SUCCESS, _currentTaskId, "", currentProcessed)
                Else
                    If Not _isConnecting Then SendMessageToPipe(PipeCmd.WORKFLOW_SUCCESS, _currentTaskId, "", Nothing)
                End If
                ' =========================================================================
                JobHistoryManager.FinishWorkflow(wrkFlow.Name, _vbaVariables)

                _logger.LogInfo("[EXECUTOR] Resetez variabilele interne pentru următorul flux.")
                _executor.ResetVariables()

                _logger.LogSuccess($"Terminat: {Path.GetFileName(taskInfo.Path)}")
                taskIndex += 1

            Catch ex As OperationCanceledException
                ' AICI intră când dai .Cancel() din buton sau VBA
                _logger.LogWarning("[WORKFLOW] Procesul a fost oprit voluntar (Cancelled).")

                ' Trimitem la ACCESS mesajul corect: STOPPED, nu ERROR
                SendMessageToPipe(PipeCmd.JOB_STOPPED, _currentTaskId, _currentJobName, "")

                ' Scriem în fișierul de output că a fost oprit (opțional, dar util)
                WriteErrorResult(_currentJobConfig?.OutputFile, "USER_CANCELLED")

                Return False

            Catch ex As Exception
                _logger.LogError($"Eroare la workflow '{Path.GetFileName(taskInfo.Path)}': {ex.Message}")
                If Not _isConnecting Then SendMessageToPipe(PipeCmd.WORKFLOW_ERROR, _currentTaskId, ex.Message)
                Return False
            End Try
        Next

        Try
            ' =========================================================================
            ' FINAL JOB - SALVARE FISIER CUMULATIV
            ' (Pastram asta la final ca sa ai un fisier Output complet cu toate datele)
            ' =========================================================================
            If Not String.IsNullOrEmpty(jobToRun.OutputFile) Then
                lblStatus.Text = "Salvare raport final..."

                ' Repetăm procesarea pentru starea finală (poate s-au mai adăugat variabile între timp)
                Dim finalRaw = _executor.GetAllVariables()
                Dim finalResults As New Dictionary(Of String, Object)()

                For Each kvp In finalRaw
                    Try
                        If Not String.IsNullOrEmpty(kvp.Value) AndAlso (kvp.Value.Trim().StartsWith("["c) OrElse kvp.Value.Trim().StartsWith("{"c)) Then
                            finalResults.Add(kvp.Key, Newtonsoft.Json.Linq.JToken.Parse(kvp.Value))
                        Else
                            finalResults.Add(kvp.Key, kvp.Value)
                        End If
                    Catch
                        finalResults.Add(kvp.Key, kvp.Value)
                    End Try
                Next

                Dim jsonResult As String = JsonConvert.SerializeObject(finalResults, Formatting.Indented)
                File.WriteAllText(jobToRun.OutputFile, jsonResult)
            End If
        Catch ex As Exception
            _logger.LogError($"Eroare salvare fișier output:  {ex.Message}")
            If Not _isConnecting Then SendMessageToPipe(PipeCmd.JOB_ERROR, _currentTaskId, _currentJobName, $"Eroare salvare fișier output: {ex.Message}")
            Return False
        End Try

        If Me.InvokeRequired Then
            Me.Invoke(Sub() Me.Hide())
        Else
            Me.Hide()
        End If
        btnAnulare.Visible = False
        lblStatus.Text = "În așteptare..."
        _isBusy = False
        Return True

    End Function

    ' =========================
    ' HELPER FUNCTIONS
    ' =========================
    Private Sub WireLogMessage()
        If _onLogMessageHandler IsNot Nothing Then
            RemoveHandler _executor.OnLogMessage, _onLogMessageHandler
        End If
        _onLogMessageHandler = Sub(msg)
                                   If InvokeRequired Then
                                       Invoke(Sub()
                                                  lblStatus.Text = msg
                                                  lblStatus.Refresh()
                                              End Sub)
                                   Else
                                       lblStatus.Text = msg
                                       lblStatus.Refresh()
                                   End If
                               End Sub
        AddHandler _executor.OnLogMessage, _onLogMessageHandler
    End Sub

    ' Helper pentru cautarea certificatului
    Private Function GetCertificateFromStore(certName As String) As X509Certificate2
        Dim certFound As X509Certificate2 = Nothing
        Using store As New X509Store(StoreName.My, StoreLocation.CurrentUser)
            store.Open(OpenFlags.ReadOnly)
            For Each c In store.Certificates
                If c.GetNameInfo(X509NameType.SimpleName, False).Contains(certName, StringComparison.CurrentCultureIgnoreCase) Then
                    certFound = c
                    Exit For
                End If
            Next
        End Using

        If certFound Is Nothing Then Throw New Exception($"Certificatul '{certName}' lipsă!")
        Return certFound
    End Function

    ' Helper pentru scrierea erorilor in fisierul JSON
    Private Sub WriteErrorResult(outputFilePath As String, errorMessage As String)
        Try
            If Not String.IsNullOrEmpty(outputFilePath) Then
                Dim err = New Dictionary(Of String, String) From {{"ERROR", errorMessage}}
                File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(err, Formatting.Indented))
            End If
        Catch
            ' Ignoram erorile la scrierea erorii (safe fail)
        End Try
    End Sub

    Private Function SetCertificateAndPin(jobToRun As RobotJob) As X509Certificate2
        Dim pinManual As Boolean = jobToRun.ManualPin
        Dim cert As X509Certificate2 = Nothing

        If _cachedCert IsNot Nothing AndAlso DateTime.Now < _cacheExpirationTime Then
            _logger.LogInfo($"[CACHE] Folosesc certificatul din memorie (expira la {_cacheExpirationTime:HH:mm:ss}).")
            jobToRun.Certificat = _cachedCert.GetNameInfo(X509NameType.SimpleName, False)
            _isLightMode = False
            Return _cachedCert

        ElseIf _cachedCert IsNot Nothing AndAlso DateTime.Now >= _cacheExpirationTime Then
            _logger.LogInfo($"[CACHE] Certificatul din memorie a expirat la {_cacheExpirationTime:HH:mm:ss}.")
            pinManual = False
        End If

        Using frm As New CertificateSelectionForm(pinManual)
            Dim dlgResult As DialogResult = frm.ShowDialog()

            If dlgResult = DialogResult.OK Then
                cert = frm.SelectedCertificate
                _isLightMode = False

            ElseIf dlgResult = DialogResult.No AndAlso frm.IsResendOnlyMode Then
                _isLightMode = True
                _logger.LogWarning("[LIGHT] Mod Resend Only activat — executorul nu va porni.")
                Throw New Exception("Mod Resend Only — executor indisponibil.")
            End If
        End Using

        If String.IsNullOrEmpty(cert?.GetNameInfo(X509NameType.SimpleName, False)) Then
            _logger.LogError("Nu a fost selectat niciun certificat!")
            Throw New Exception("Nu a fost selectat niciun certificat!")
        End If

        jobToRun.Certificat = cert.GetNameInfo(X509NameType.SimpleName, False)
        _cacheExpirationTime = DateTime.Now.AddMinutes(CERTIFICATE_CACHE_MINUTES)
        _cachedCert = cert
        Return cert
    End Function
End Class
