Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteIfUniqueAsync(action As IfUniqueAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"Verific unicitate: {parsedSelector}")
        Dim isUnique As Boolean

        Try
            ' 1. Așteptăm cu timeout să apară în DOM
            Dim appeared = Await TryWaitForElementAsync(parsedSelector, WaitForSelectorState.Attached, action.Timeout)

            If Not appeared Then
                _logger.LogError($"Nu s-au găsit elemente pentru selector: {parsedSelector}")
                Return
            End If

            Dim locator = _page.Locator(parsedSelector)
            Dim count As Integer = Await locator.CountAsync()

            If count = 0 Then
                _logger.LogError($"Nu s-au găsit elemente pentru selector: {parsedSelector}")
                Return
            End If

            If action.OnlyIfVisible Then
                Dim visibleCount As Integer = 0
                For i As Integer = 0 To count - 1
                    If Await locator.Nth(i).IsVisibleAsync() Then
                        visibleCount += 1
                    End If
                Next
                isUnique = (visibleCount = 1)
            Else
                isUnique = (count = 1)
            End If

        Catch ex As Exception
            _logger.LogError($"Eroare la verificare: {ex.Message}")
            isUnique = False
        End Try

        If isUnique Then
            _logger.LogSuccess($"Element unic detectat. Intru pe ramura TRUE.")
            Await ExecuteActionsAsync(action.Children, True)
        Else
            If action.ElseChildren.Count > 0 Then
                _logger.LogInfo($"Elementul nu e unic. Intru pe ramura FALSE.")
                Await ExecuteActionsAsync(action.ElseChildren, True)
            End If
        End If
    End Function

End Class
