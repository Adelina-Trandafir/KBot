Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteMinimizeAsync(action As MinimizeAction) As Task
        LogStep(action, "Minimizez fereastra...")
        Try
            Dim cdp = Await _page.Context.NewCDPSessionAsync(_page)
            Dim targetInfo = Await cdp.SendAsync("Browser.getWindowForTarget")
            Dim windowId = targetInfo.Value.GetProperty("windowId").GetInt32()
            Dim bounds = New Dictionary(Of String, Object) From {{"windowId", windowId}, {"bounds", New Dictionary(Of String, Object) From {{"left", -2000}, {"top", -2000}}}}
            Await cdp.SendAsync("Browser.setWindowBounds", bounds)
        Catch ex As Exception
            _logger.LogWarning($"Eroare minimize: {ex.Message}")
        End Try
    End Function

End Class
