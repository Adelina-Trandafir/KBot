Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteWhileAsync(action As WhileAction) As Task
        _logger.LogInfo($"[While] Start. MaxIterations={action.MaxIterations}")

        Dim loopCtx As New LoopContext With {
        .ActionType = "While",
        .RuntimeIndex = 0,
        .IndexVariableName = action.IndexVariable
    }
        _executionStack.Push(loopCtx)

        Try
            Dim iterations As Integer = 0

            Do
                iterations += 1
                loopCtx.RuntimeIndex = iterations

                If iterations > action.MaxIterations Then
                    _logger.LogWarning($"[While] MaxIterations ({action.MaxIterations}) atins. Opresc.")
                    Exit Do
                End If

                ' ── Evaluare condiție ──────────────────────────────────────────
                Dim shouldContinue As Boolean = False

                If Not String.IsNullOrEmpty(action.JsCondition) Then
                    ' Ramura JS
                    shouldContinue = Await EvaluateJsConditionAsync(action.JsCondition)
                    _logger.LogDebug($"[While] JsCondition → {shouldContinue}")
                Else
                    ' Ramura selector clasică
                    Dim parsedSelector = ReplaceInternalVariables(action.Selector)
                    Dim appeared = Await TryWaitForElementAsync(
                    parsedSelector,
                    WaitForSelectorState.Visible,
                    action.Timeout)

                    Select Case action.Condition.Trim().ToLower()
                        Case "visible"
                            shouldContinue = appeared
                        Case "hidden"
                            shouldContinue = Not appeared
                        Case "present"
                            Dim present = Await TryWaitForElementAsync(
                            parsedSelector,
                            WaitForSelectorState.Attached,
                            action.Timeout)
                            shouldContinue = present
                        Case Else
                            shouldContinue = appeared
                    End Select
                End If

                If Not shouldContinue Then
                    _logger.LogInfo($"[While] Condiție false la iterația {iterations}. Opresc.")
                    Exit Do
                End If

                _logger.LogInfo($"--- [While] Iterația {iterations} ---")

                If action.Children.Count > 0 Then
                    Await ExecuteActionsAsync(action.Children, True)
                End If

            Loop

        Finally
            _executionStack.Pop()
        End Try

        _logger.LogSuccess("[While] Buclă finalizată.")
    End Function

End Class
