Imports System.Threading
Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Public Async Function ExecuteAsync(workflow As Workflow) As Task
        Dim errorOccurred As Boolean = False
        Dim errorException As Exception = Nothing

        Try
            _logger.LogInfo($"Pornesc execuția pașilor pentru: {workflow.Name}")
            _currentWorkflow = workflow
            _currentActionIndex = 0

            ' VERIFICARE CRITICĂ: Dacă browserul nu e pornit, îl pornim acum (fallback)
            If _page Is Nothing Then
                _logger.LogWarning("Browserul nu era pornit. Îl lansez acum...")
                Await LaunchAndPositionBrowserAsync()
            End If

            Await ApplyThrottleAsync()

            ' === MODIFICARE PENTRU STARTURL = CURRENT ===
            If workflow.StartUrl.Trim().Equals("CURRENT", StringComparison.CurrentCultureIgnoreCase) Then
                'verific daca expected url e diferit de cel curent
                If workflow.ExpectedUrl IsNot Nothing AndAlso
                   Not String.IsNullOrEmpty(workflow.ExpectedUrl.Trim()) AndAlso
                   Not _page.Url.Contains(workflow.ExpectedUrl.Trim()) Then
                    _logger.LogAction($"Url gresit{_page.Url} Navighez la: {workflow.ExpectedUrl}")
                    Await _page.GotoAsync(workflow.ExpectedUrl.Trim())
                End If
                '_logger.LogInfo("Păstrez pagina curentă (Continuare flux...)")

                ' Logica veche de navigare:
            Else 'If _page.Url <> workflow.StartUrl AndAlso Not _page.Url.Contains(workflow.StartUrl) Then
                _logger.LogAction($"Navighez la: {workflow.StartUrl}")
                Await _page.GotoAsync(workflow.StartUrl)
            End If

            If _progressCallback IsNot Nothing Then _progressCallback(0, workflow.Actions.Count)

            ' Validare variabile JSON înainte de execuție
            ValidateJsonVariables(workflow)

            ' Executăm acțiunile
            Await ExecuteActionsAsync(workflow.Actions)

            '_logger.LogSuccess("Workflow terminat cu succes!")
            If _progressCallback IsNot Nothing Then _progressCallback(workflow.Actions.Count, workflow.Actions.Count)
        Catch ex As WorkflowExitException
            _progressCallback?(workflow.Actions.Count, workflow.Actions.Count)
            _logger.LogWarning($"Execuție oprită: {ex.Message}")
        Catch ex As Exception
            errorOccurred = True
            errorException = ex
            _logger.LogException(ex, "Eroare la executare workflow")
        Finally
            StopAuthMonitoring()
        End Try

        If errorOccurred Then
            Try
                If _page IsNot Nothing Then
                    Dim path = IO.Path.Combine("C:\AVACONT\LOGS\Errors", $"Eroare_{DateTime.Now:yyyyMMdd_HHmmss}.png")
                    Dim dir = IO.Path.GetDirectoryName(path)
                    If Not IO.Directory.Exists(dir) Then IO.Directory.CreateDirectory(dir)
                    Await _page.ScreenshotAsync(New PageScreenshotOptions With {.Path = path, .FullPage = True, .Timeout = 1000})
                End If
            Catch
            End Try
            Throw errorException
        End If
    End Function

    Private Async Function ExecuteActionsAsync(actions As List(Of IWorkflowAction), Optional isSubWorkflow As Boolean = False) As Task
        ' Resetăm indexul local: 
        ' - 0 dacă e sub-workflow (începem un IF)
        ' - _currentActionIndex dacă e workflow-ul principal (continuăm de unde am rămas)
        Dim i As Integer = If(isSubWorkflow, 0, _currentActionIndex)

        While i < actions.Count
            Dim action = actions(i)

            Dim selectorInfo As String = ""
            Dim prop = action.GetType().GetProperty("Selector")
            If prop IsNot Nothing Then
                Dim val = prop.GetValue(action)
                If val IsNot Nothing Then selectorInfo = $" [{val}]"
            End If

            ' --- LOGICA DE AFIȘARE A CONTORULUI SEPARAT ---
            Dim msg As String
            If isSubWorkflow Then
                ' Suntem într-un IF/Sub-flux: Afișăm contorul local indentat
                msg = $"   ╚► [IF] Pasul {i + 1}/{actions.Count}: {action.ActionType}{selectorInfo}"
            Else
                ' Suntem în fluxul principal
                msg = $"[MAIN] Pasul {i + 1}/{actions.Count}: {action.ActionType}{selectorInfo}"
            End If

            RaiseEvent OnStatusUpdate(msg)
            RaiseEvent OnActionStart(action)

            ' Actualizăm bara de progres DOAR pentru fluxul principal 
            ' (ca să nu sară înainte și înapoi când intrăm în sub-fluxuri)
            If _progressCallback IsNot Nothing AndAlso Not isSubWorkflow Then
                _progressCallback(i, actions.Count)
            End If

            ' --- Logica de Pauză (Step-by-Step) ---
            Dim shouldPause As Boolean = False
            If TypeOf action Is StopAction Then
                shouldPause = True
                _logger.LogWarning($"[STOP] {DirectCast(action, StopAction).Message}")
            ElseIf _stepByStep Then
                If _stepOnlyCheckpoints Then
                    shouldPause = action.IsCheckpoint   ' isCheckpoint bate tipul acțiunii
                ElseIf TypeOf action Is LogAction Then
                    shouldPause = False                 ' fără checkpoint: Log e sărit
                Else
                    shouldPause = True
                End If
            End If

            If shouldPause Then
                Dim result = WaitForConfirmation(msg)
                Select Case result
                    Case StepResult.Stop
                        Throw New OperationCanceledException("Oprit de utilizator")

                    Case StepResult.Skip
                        _logger.LogWarning($"SKIP: {action.ActionType}")
                        i += 1
                        Continue While

                    Case StepResult.Previous
                        ' --- LOGICA PREVIOUS ---
                        If i > 0 Then
                            ' Mergem la pasul anterior din lista CURENTĂ
                            Dim targetIndex As Integer = i - 1

                            ' Logică specifică pentru Checkpoints
                            If _stepOnlyCheckpoints Then
                                Dim found As Boolean = False
                                For backIndex As Integer = (i - 1) To 0 Step -1
                                    If actions(backIndex).IsCheckpoint Then
                                        targetIndex = backIndex
                                        found = True
                                        Exit For
                                    End If
                                Next
                                If Not found Then targetIndex = 0
                            End If

                            _logger.LogInfo($"<< Înapoi la pasul {targetIndex + 1} (din fluxul curent)")
                            i = targetIndex
                            Continue While
                        Else
                            ' Suntem la pasul 1 din lista curentă
                            _logger.LogWarning("Ești la începutul acestui flux. Nu poți merge mai în spate.")
                            ' Rămânem pe loc (reîncărcăm pasul curent)
                            Continue While
                        End If
                End Select
            End If

            _cancellationToken.ThrowIfCancellationRequested()

            ' Executăm acțiunea
            Await ExecuteActionAsync(action)

            ' Actualizăm starea globală DOAR dacă suntem pe main thread
            If Not isSubWorkflow Then
                If action.IsCheckpoint Then
                    _lastCheckpointIndex = i
                End If
                _currentActionIndex = i + 1
            End If

            i += 1
        End While
    End Function

    Private Function WaitForConfirmation(msg As String) As StepResult
        If _confirmStep Is Nothing Then Return StepResult.Continue
        Return _confirmStep(msg)
    End Function

    Public Async Function ResumeFromCheckpointAsync() As Task
        If CanResumeFromCheckpoint Then
            If Not String.IsNullOrEmpty(_workflowPath) AndAlso IO.File.Exists(_workflowPath) Then
                Try
                    Dim reloaded = WorkflowParser.ParseFromFile(_workflowPath, _logger)
                    _currentWorkflow = reloaded
                Catch
                End Try
            End If

            _logger.LogInfo($"Reiau de la index {_lastCheckpointIndex + 1}")
            _currentActionIndex = _lastCheckpointIndex + 1
            Await ExecuteActionsAsync(_currentWorkflow.Actions)
        End If
    End Function

    ''' <summary>
    ''' Expune ReplaceInternalVariables public — folosit de KBOT_STANDALONE pentru tooltip-uri live.
    ''' </summary>
    Public Function ResolveText(input As String) As String
        Return ReplaceInternalVariables(input)
    End Function

    ''' <summary>
    ''' Actualizează contextul de execuție pentru un nou Job, păstrând Browserul deschis.
    ''' </summary>
    Public Sub UpdateContext(newToken As CancellationToken)
        _cancellationToken = newToken
        _logger.LogInfo("[EXECUTOR] Context actualizat (Token nou preluat).")
    End Sub


    ''' <summary>
    ''' Actualizează setările step-by-step fără a reiniția browserul.
    ''' Folosit când executorul e refolosit între rulări.
    ''' </summary>
    Public Sub UpdateStepSettings(stepByStep As Boolean,
                                   confirmStep As Func(Of String, StepResult),
                                   stepOnlyCheckpoints As Boolean)
        _stepByStep = stepByStep
        _confirmStep = confirmStep
        _stepOnlyCheckpoints = stepOnlyCheckpoints
        _useSnapAssist = stepByStep
        _logger.LogInfo($"[EXECUTOR] Setări step-by-step actualizate (activ: {stepByStep}).")
    End Sub
End Class