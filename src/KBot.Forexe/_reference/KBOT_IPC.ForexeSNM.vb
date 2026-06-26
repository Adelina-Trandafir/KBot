Imports System.IO
Imports System.Windows.Forms
Imports GeneralClasses
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Partial Public Class KBOT_IPC

    Private Async Function RunSNMJobFromFolder() As Task(Of Boolean)
        Dim currentJobPath As String = ""
        Dim jsonContent As String = ""
        Dim task As RobotTask = Nothing

        Try
            ' ============================================================
            ' 1. VALIDARE INPUT
            ' ============================================================
            Dim jsonFiles = Directory.GetFiles(_jobFolderPath, "*.json")

            If jsonFiles.Length <> 1 Then
                Dim msg = $"[BATCH] Aștept exact un fișier job în folderul '{_jobFolderPath}'. Găsite: {jsonFiles.Length}"
                _logger.LogError(msg)
                SendMessageToPipe(PipeCmd.JOB_ERROR, 0, "", msg)
                Return False
            End If

            currentJobPath = jsonFiles(0)

            If Not File.Exists(currentJobPath) Then
                Throw New Exception("Fișier job inexistent!")
            End If

            ' ============================================================
            ' 2. DESERIALIZARE
            ' ============================================================
            jsonContent = File.ReadAllText(currentJobPath)

            Dim partialJob As RobotJob = Nothing
            Try
                partialJob = JsonConvert.DeserializeObject(Of RobotJob)(jsonContent)
            Catch ex As Exception
                Throw New Exception("Eroare deserializare JSON job.", ex)
            End Try

            If partialJob Is Nothing Then
                Throw New Exception("Job NULL după deserializare.")
            End If

            _currentJobName = partialJob.JobName

            ' ============================================================
            ' 3. START HISTORY
            ' ============================================================
            JobHistoryManager.StartJob(_currentJobName, jsonContent)

            ' ============================================================
            ' 4. EXTRAGERE TASK + VARS
            ' ============================================================
            task = partialJob.Tasks?.FirstOrDefault()

            If task Is Nothing Then
                Throw New Exception("Task lipsă în job.")
            End If

            Dim dataStart As DateTime = DateTime.Today
            Dim folderSave As String = ""

            If task.Vars IsNot Nothing Then

                Dim value As String = Nothing
                If task.Vars.TryGetValue("DataInceput", value) Then
                    If Not DateTime.TryParseExact(value,
                                             New String() {"dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd"},
                                             Globalization.CultureInfo.InvariantCulture,
                                             Globalization.DateTimeStyles.None,
                                             dataStart) Then
                        Throw New Exception($"DataInceput invalid: '{value}'")
                    End If
                End If

                Dim value2 As String = Nothing
                If task.Vars.TryGetValue("FolderExtrase", value2) Then
                    folderSave = value2
                End If
            End If

            _logger.LogInfo($"[SNM] Start extrase | DataInceput={dataStart.ToString("dd.MM.yyyy", Globalization.CultureInfo.InvariantCulture)} | Folder={folderSave}")

            ' ============================================================
            ' 5. EXECUȚIE SNM
            ' ============================================================
            Dim resultArray As JArray = Await IPC_DescarcaExtraseForexe(dataStart, _currentJobName, folderSave)

            If resultArray Is Nothing Then
                Dim msg = "[SNM] Nu s-au obținut rezultate (resultArray = NULL)."
                _logger.LogError(msg)

                SendMessageToPipe(PipeCmd.JOB_ERROR, task.TaskId, "", msg)
                JobHistoryManager.FailJob(msg)

                Me.Invoke(Sub()
                              pbProgress.Value = 0
                              lblStatus.Text = "SNM: Eroare — fără rezultate."
                          End Sub)

                Return False
            End If

            ' ============================================================
            ' 6. CONSTRUIRE PAYLOAD
            ' ============================================================
            Dim currentProcessed As New Dictionary(Of String, Object)

            currentProcessed("Extrase") = resultArray
            currentProcessed("_wfl_name") = _currentJobName
            currentProcessed("_wfl_path") = "SNM_INTERNAL"

            Dim varsMeta As New Dictionary(Of String, String)
            varsMeta("DataInceput") = dataStart.ToString("dd.MM.yyyy", Globalization.CultureInfo.InvariantCulture)
            varsMeta("FolderExtrase") = folderSave

            currentProcessed("_wfl_vars") = varsMeta

            _logger.LogDebug($"[SNM] Rezultate: {resultArray.Count} extrase.")

            ' ============================================================
            ' 7. SALVARE HISTORY
            ' ============================================================
            JobHistoryManager.FinishWorkflow(_currentJobName, currentProcessed)

            ' ============================================================
            ' 8. TRIMITERE PIPE
            ' ============================================================
            SendMessageToPipe(PipeCmd.WORKFLOW_SUCCESS,
                              task.TaskId,
                              "",
                              currentProcessed)

            ' ============================================================
            ' 9. FINALIZARE JOB
            ' ============================================================
            JobHistoryManager.FinishJob("Succes")

            _logger.LogSuccess("[SNM] Job finalizat cu succes.")

            Me.Invoke(Sub()
                          lblStatus.Text = $"SNM: {resultArray.Count} extrase procesate."
                      End Sub)

            ' Ștergere job file (best effort)
            Try
                File.Delete(currentJobPath)
            Catch ex As Exception
                _logger.LogWarning($"Nu am putut șterge fișierul job: {ex.Message}")
            End Try

            Return True

        Catch ex As Exception
            ' ============================================================
            ' ERROR HANDLING CENTRALIZAT
            ' ============================================================
            Dim msg = $"[SNM] Eroare: {ex.Message}"

            _logger.LogException(ex, "RunSNMJobFromFolder")

            If task IsNot Nothing Then
                SendMessageToPipe(PipeCmd.JOB_ERROR, task.TaskId, _currentJobName, ex.Message)
            Else
                SendMessageToPipe(PipeCmd.JOB_ERROR, 0, _currentJobName, ex.Message)
            End If

            JobHistoryManager.FailJob(ex.Message)

            Me.Invoke(Sub()
                          lblStatus.Text = "SNM: Eroare la execuție."
                      End Sub)

            Return False
        End Try
    End Function

    ''' <summary>
    ''' Apelează DescarcaExtraseForexeAPI cu progressCallback care actualizează lblStatus.
    ''' pbProgress este setat în Marquee pe durata descărcării și resetat la Continuous după.
    ''' Finally garantează resetul barei indiferent de rezultat (succes, eroare, excepție).
    ''' </summary>
    Private Async Function IPC_DescarcaExtraseForexe(dataStart As DateTime,
                                                      jobName As String,
                                                      dwnFolder As String) As Task(Of JArray)
        Dim resultArray As JArray = Nothing

        Try
            ' Pornire Marquee înainte de download
            Me.Invoke(Sub()
                          pbProgress.Style = ProgressBarStyle.Marquee
                          lblStatus.Text = "Descărcare extrase în curs..."
                      End Sub)

            Dim progressCb As Action(Of Integer, Integer, String) =
                Sub(current As Integer, total As Integer, msg As String)
                    Me.Invoke(Sub()
                                  lblStatus.Text = msg
                                  lblStatus.Refresh()
                              End Sub)
                End Sub

            resultArray = Await ForexeSNM.DescarcaExtraseForexeAPI(
                                            page:=_executor.CurrentPage,
                                            logger:=_logger,
                                            downloadFolder:=dwnFolder,
                                            dataDeLa:=dataStart,
                                            progressCallback:=progressCb)

            resultArray = New JArray(resultArray.Reverse())

            _logger.LogSuccess($"Descarcare finalizata. {resultArray.Count} extrase descărcate.")

        Catch ex As Exception
            _logger.LogException(ex, "IPC_DescarcaExtraseForexe")

        Finally
            ' Reset Marquee → Continuous indiferent de rezultat
            Me.Invoke(Sub()
                          pbProgress.Style = ProgressBarStyle.Continuous
                          pbProgress.Value = 0
                      End Sub)
        End Try

        Return resultArray
    End Function

End Class