Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteSelectAsync(action As SelectAction) As Task
        Dim resolvedSelector = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"Selectez in: {resolvedSelector}")
        Dim locator = _page.Locator(resolvedSelector)

        ' Așteptăm ca elementul să fie vizibil înainte de select
        Await locator.WaitForAsync(New LocatorWaitForOptions With {
            .State = WaitForSelectorState.Visible,
            .Timeout = action.Timeout * 1000
        })

        If action.Value IsNot Nothing Then
            Dim resolvedValue = ReplaceInternalVariables(action.Value)
            Await locator.SelectOptionAsync(New SelectOptionValue With {.Value = resolvedValue})
        ElseIf action.Text IsNot Nothing Then
            Dim resolvedText = ReplaceInternalVariables(action.Text)
            Await locator.SelectOptionAsync(New SelectOptionValue With {.Label = resolvedText})
        ElseIf action.Index.HasValue Then
            Await locator.SelectOptionAsync(New SelectOptionValue With {.Index = action.Index.Value})
        End If
    End Function

End Class
