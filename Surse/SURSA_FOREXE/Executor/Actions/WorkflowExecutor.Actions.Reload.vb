Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteReloadAsync(action As ReloadAction) As Task
        LogStep(action, "Reîncarc pagina...")
        Await _page.ReloadAsync()
        If action.WaitNavigation Then Await _page.WaitForLoadStateAsync(LoadState.NetworkIdle)
    End Function

End Class
