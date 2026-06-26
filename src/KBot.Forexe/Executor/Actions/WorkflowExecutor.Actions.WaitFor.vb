Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteWaitForAsync(action As WaitForAction) As Task
        Dim state As WaitForSelectorState
        Dim parsedSelector As String = action.Selector

        Select Case action.State.ToLower()
            Case "visible" : state = WaitForSelectorState.Visible
            Case "hidden" : state = WaitForSelectorState.Hidden
            Case "attached" : state = WaitForSelectorState.Attached
            Case "detached" : state = WaitForSelectorState.Detached
            Case Else : state = WaitForSelectorState.Visible
        End Select

        Dim attempt As Integer = 0
        Dim maxAttempts As Integer = If(action.RefreshOnFail, action.MaxRetries, 1)

        While attempt < maxAttempts
            attempt += 1
            parsedSelector = ReplaceInternalVariables(action.Selector)
            LogStep(action, $"Aștept element: {parsedSelector} ({action.State})")

            ' Folosim TryWaitForElementAsync care nu aruncă excepții pe care să le prindem greșit
            Dim found = Await TryWaitForElementAsync(parsedSelector, state, action.Timeout, action.Strict)

            If found Then
                Return
            End If

            If action.RefreshOnFail AndAlso attempt < maxAttempts Then
                _logger.LogWarning($"Element negăsit (încercare {attempt}/{maxAttempts}). Dau refresh...")
                Await _page.ReloadAsync(New PageReloadOptions With {.WaitUntil = WaitUntilState.NetworkIdle})
            End If
        End While

        Throw New TimeoutException($"Elementul '{parsedSelector}' nu a fost găsit.")
    End Function

    Private Async Function TryWaitForElementAsync(selector As String, state As WaitForSelectorState, timeoutSeconds As Integer, Optional strict As Boolean = False) As Task(Of Boolean)
        Try
            Dim parsedSelector = ReplaceInternalVariables(selector)
            Dim locator = If(strict, _page.Locator(parsedSelector), _page.Locator(parsedSelector).First)
            Await locator.WaitForAsync(New LocatorWaitForOptions With {
                    .State = state,
                    .Timeout = timeoutSeconds * 1000
                })
            Return True
        Catch ex As TimeoutException
            Return False
        Catch ex As PlaywrightException
            Return False
        End Try
    End Function

End Class
