Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteScreenshotAsync(action As ScreenshotAction) As Task
        LogStep(action, "Salvez captura de ecran...")

        If Not String.IsNullOrEmpty(action.SaveTo) Then
            ' Captură în memorie -> Base64 -> variabilă (fără disc)
            Dim bytes = Await _page.ScreenshotAsync(New PageScreenshotOptions With {.FullPage = True})
            Dim resolvedVar = ReplaceInternalVariables(action.SaveTo)
            SetVariable(resolvedVar, Convert.ToBase64String(bytes))
            _logger.LogSuccess($"[Screenshot] Base64 salvat în [[{resolvedVar}]].")
        Else
            ' Comportament original
            Dim path = If(String.IsNullOrEmpty(action.ScreenshotPath),
                      IO.Path.Combine(IO.Path.GetTempPath(), $"scr_{DateTime.Now:yyyyMMdd_HHmmss}.png"),
                      action.ScreenshotPath)
            Await _page.ScreenshotAsync(New PageScreenshotOptions With {.Path = path, .FullPage = True})
            _logger.LogSuccess($"[Screenshot] Captură salvată la: {path}")
        End If
    End Function

End Class
