Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteScreenshotAsync(action As ScreenshotAction) As Task
        LogStep(action, "Salvez captura de ecran...")

        If Not String.IsNullOrEmpty(action.SaveTo) Then
            ' Captură în memorie -> Base64 -> variabilă (fără disc).
            ' JPEG since 28.09.2026 (operator): these captures end up inside the signed PDF
            ' (Table4), where a full-page PNG costs several times as much for figures that
            ' read just as well. The cell says what the bytes are (DdfXmlBuilder), and the
            ' server takes either (routes/forexe/ddf_trimitere.py).
            Dim bytes = Await _page.ScreenshotAsync(New PageScreenshotOptions With {
                .FullPage = True,
                .Type = ScreenshotType.Jpeg,
                .Quality = ShotJpegQuality})
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
