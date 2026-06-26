Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteAuthClickAsync(action As AuthClickAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"AuthClick (Smart) pe: {parsedSelector}")

        Dim locator = _page.Locator(parsedSelector)
        Dim timeoutMs As Integer = action.AuthTimeout * 1000

        ' 1. Executăm CLICK-ul
        ' Folosim un Task separat pentru click doar ca măsură de siguranță (în caz că totuși apare ceva scurt)
        ' Dar, dacă ești sigur pe Chromium, ai putea face și 'Await locator.ClickAsync' direct.
        Dim clickTask = Task.Run(Async Function()
                                     Try
                                         ' Folosim Timeout-ul din acțiune, nu hardcoded
                                         Dim clickOptions As New LocatorClickOptions With {
                                             .Force = True,
                                             .Timeout = If(action.Timeout > 0, action.Timeout * 1000, 10000)
                                         }
                                         Await locator.ClickAsync(clickOptions)
                                     Catch ex As Exception
                                         _logger.LogDebug($"[AuthClick] Click terminat (sau eroare ignorată): {ex.Message}")
                                     End Try
                                 End Function)

        ' 2. Așteptăm REZULTATUL (Navigarea la URL-ul așteptat)
        If Not String.IsNullOrEmpty(action.ExpectedUrlAfterAuth) Then
            Try
                _logger.LogInfo($"[Auth] Aștept navigarea către: {action.ExpectedUrlAfterAuth} ...")

                ' WaitForURLAsync este funcția magică:
                ' - Așteaptă până când URL-ul se potrivește
                ' - Are timeout integrat
                ' - Suportă wildcards (ex: "**/dashboard/**")
                Await _page.WaitForURLAsync(action.ExpectedUrlAfterAuth, New PageWaitForURLOptions With {
                    .Timeout = timeoutMs,
                    .WaitUntil = WaitUntilState.Load ' Sau NetworkIdle pentru siguranță maximă
                })

                _logger.LogSuccess($"[Auth] Succes! Am ajuns la: {_page.Url}")

            Catch ex As TimeoutException
                Dim msg As String = $"[Auth] EROARE CRITICĂ: Timeout la autentificare! Nu am ajuns la URL-ul așteptat în {action.AuthTimeout} secunde."
                _logger.LogError(msg)

                ' ARUNCĂM EXCEPȚIA - Asta va opri ExecuteCoreWorkflowAsync și va returna False
                Throw New Exception(msg)
            End Try
        Else
            ' Fallback: Dacă nu ai definit un URL, așteptăm doar să se încarce pagina (NetworkIdle)
            _logger.LogInfo("[Auth] Nu s-a specificat 'ExpectedUrlAfterAuth'. Aștept doar NetworkIdle.")
            Await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, New PageWaitForLoadStateOptions With {.Timeout = timeoutMs})
        End If

        ' Asigurăm finalizarea task-ului de click (deși probabil e gata de mult)
        Await clickTask
    End Function

End Class
