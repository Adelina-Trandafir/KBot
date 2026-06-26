Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteScrollToViewAsync(action As ScrollToViewAction) As Task
        Try
            Dim parsedSelector = ReplaceInternalVariables(action.Selector)
            LogStep(action, $"Scrollez la elementul: {parsedSelector}")

            Dim locator = _page.Locator(parsedSelector)

            ' 1. Așteptăm ca elementul să existe în DOM (chiar dacă nu e vizibil încă)
            ' Nu folosim WaitForSelectorState.Visible aici, pentru că tocmai asta vrem să rezolvăm prin scroll.
            ' Așteptăm doar 'Attached' (să existe în HTML).
            Await locator.WaitForAsync(New LocatorWaitForOptions With {
                .State = WaitForSelectorState.Attached,
                .Timeout = action.Timeout * 1000
            })

            ' 2. Executăm scroll-ul efectiv
            ' Această funcție Playwright face scroll doar dacă elementul nu e deja în viewport.
            Await locator.ScrollIntoViewIfNeededAsync(New LocatorScrollIntoViewIfNeededOptions With {
                .Timeout = action.Timeout * 1000
            })

            ' 3. Opțional: Așteptăm puțin să se stabilizeze randarea (dacă e lazy loading)
            Await Task.Delay(200)

            _logger.LogSuccess($"[ScrollToView] Elementul '{parsedSelector}' este acum vizibil în viewport.")

        Catch ex As TimeoutException
            _logger.LogError($"[ScrollToView] Timeout! Elementul '{action.Selector}' nu a fost găsit în DOM pentru a putea face scroll.")
            Throw
        Catch ex As Exception
            _logger.LogError($"[ScrollToView] Eroare: {ex.Message}")
            Throw
        End Try
    End Function

End Class
