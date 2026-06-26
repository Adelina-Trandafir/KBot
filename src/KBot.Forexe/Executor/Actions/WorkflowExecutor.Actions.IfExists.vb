Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor
    Private Async Function ExecuteIfExistsAsync(action As IfExistsAction) As Task
        Dim isVisible As Boolean = False

        If Not String.IsNullOrEmpty(action.JsCondition) Then
            ' Ramura JS
            LogStep(action, $"IfExists [JS]: {action.JsCondition.Substring(0, Math.Min(60, action.JsCondition.Length))}...")
            isVisible = Await EvaluateJsConditionAsync(action.JsCondition)
        Else
            ' Ramura selector clasică
            Dim parsedSelector = ReplaceInternalVariables(action.Selector)
            LogStep(action, $"Verific existența: {parsedSelector}")

            Try
                Dim locator = _page.Locator(parsedSelector)

                ' 1. Așteptăm cu timeout să apară în DOM
                Dim appeared = Await TryWaitForElementAsync(parsedSelector, WaitForSelectorState.Attached, action.Timeout, action.Strict)

                If Not appeared Then
                    isVisible = False
                Else
                    ' 2. Verificăm vizibilitatea (cu logica multi-element pentru strict mode)
                    Dim count As Integer = Await locator.CountAsync()

                    If count = 0 Then
                        isVisible = False
                    ElseIf count = 1 Then
                        isVisible = Await locator.IsVisibleAsync()
                    Else
                        For i As Integer = 0 To count - 1
                            If Await locator.Nth(i).IsVisibleAsync() Then
                                isVisible = True
                                Exit For
                            End If
                        Next
                    End If
                End If

            Catch ex As Exception
                _logger.LogDebug($"Eroare la verificare: {ex.Message}")
                isVisible = False
            End Try
        End If


        If isVisible Then
            If action.Children.Count > 0 Then
                _logger.LogSuccess($"Element vizibil detectat. Intru pe ramura DA.")
                Await ExecuteActionsAsync(action.Children, True)
            End If
        Else
            If action.ElseChildren.Count > 0 Then
                _logger.LogInfo($"Elementul nu e vizibil. Intru pe ramura ELSE.")
                Await ExecuteActionsAsync(action.ElseChildren, True)
            End If
        End If
    End Function
End Class
