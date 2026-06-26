Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteGoBackAsync(action As GoBackAction) As Task
        LogStep(action, "Naviguez înapoi (Browser Back)...")

        Try
            ' GoBackAsync navighează înapoi și așteaptă implicit 'load' event
            ' Folosim NetworkIdle pentru a fi siguri că tabelul anterior s-a reîncărcat complet
            Await _page.GoBackAsync(New PageGoBackOptions With {
                .Timeout = action.Timeout * 1000,
                .WaitUntil = WaitUntilState.Load
            })

            _logger.LogSuccess("[GoBack] Navigare realizată.")
        Catch ex As Exception
            _logger.LogError($"[GoBack] Eroare: {ex.Message}")
            Throw
        End Try
    End Function

End Class
