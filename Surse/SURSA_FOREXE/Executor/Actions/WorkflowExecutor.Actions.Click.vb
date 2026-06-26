Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteClickAsync(action As ClickAction) As Task
        Dim finalSelector As String = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"Click pe: {finalSelector} {(If(action.ExpectNewTab, "[TAB NOU]", ""))}")

        Dim locator = _page.Locator(finalSelector)
        Dim timeoutMs As Integer = action.Timeout * 1000

        ' A. PREGĂTIRE MONITORIZARE TAB NOU (Dacă e cerut explicit)
        Dim newPageTask As Task(Of IPage) = Nothing
        If action.ExpectNewTab Then
            ' Pornim ascultătorul ÎNAINTE de click.
            ' Dacă click-ul se întâmplă și noi nu ascultăm, pierdem evenimentul.
            newPageTask = _context.WaitForPageAsync(New BrowserContextWaitForPageOptions With {
                .Timeout = timeoutMs
            })
        End If

        ' B. EXECUȚIA CLICK-ULUI (JS sau Standard)
        If action.JsClick Then
            ' --- RAMURA JS ---
            Try
                Dim count As Integer = Await locator.CountAsync()
                If count = 0 Then Throw New Exception("[JS Click] Element negăsit (Count=0).")

                Await locator.EvaluateAsync("e => e.click()")
            Catch ex As Exception
                _logger.LogError($"[JS Click Error] {ex.Message}")
                Throw
            End Try
        Else
            ' --- RAMURA STANDARD ---
            If Not action.Force Then
                ' Verificare vizibilitate (fără Await în Catch - clean)
                Dim pEx As Microsoft.Playwright.PlaywrightException = Nothing
                Try
                    Await locator.WaitForAsync(New LocatorWaitForOptions With {
                        .State = WaitForSelectorState.Visible,
                        .Timeout = timeoutMs
                    })
                Catch ex As Microsoft.Playwright.PlaywrightException
                    pEx = ex
                End Try

                If pEx IsNot Nothing Then
                    _logger.LogError($"[Vizibilitate] Elementul nu a apărut: {pEx.Message}")
                    Throw pEx
                End If
            End If

            ' Click efectiv
            Dim clickOptions As New LocatorClickOptions With {.Timeout = timeoutMs, .Force = action.Force}
            Await locator.ClickAsync(clickOptions)
        End If

        ' C. GESTIONAREA NAVIGĂRII (Momentul Adevărului)
        If action.ExpectNewTab Then
            ' CAZ 1: EXPLICIT TAB NOU
            Try
                _logger.LogInfo("Aștept deschiderea noului tab (ExpectNewTab=True)...")

                ' Aici așteptăm task-ul pornit la pasul A
                Dim newPage = Await newPageTask

                ' Așteptăm să fie gata
                Await newPage.WaitForLoadStateAsync(LoadState.Load)

                ' CRITIC: ACTUALIZĂM REFERINȚA GLOBALĂ
                _page = newPage

                ' Îl aducem în față (focus) ca să fim siguri vizual
                Try
                    Await _page.BringToFrontAsync()
                Catch
                End Try

                _logger.LogSuccess($"Context mutat pe noul tab: {_page.Url}")

            Catch ex As TimeoutException
                _logger.LogError($"[ExpectNewTab] Timeout! Nu s-a deschis niciun tab nou în {action.Timeout} secunde.")
                Throw
            Catch ex As Exception
                _logger.LogError($"[ExpectNewTab] Eroare la preluarea noului tab: {ex.Message}")
                Throw
            End Try

        ElseIf action.WaitNavigation Then
            ' CAZ 2: NAVIGARE ÎN PAGINA CURENTĂ (Default curat)
            Try
                ' Așteptăm LoadState pe pagina CURENTĂ (care e neschimbată)
                Await _page.WaitForLoadStateAsync(LoadState.Load, New PageWaitForLoadStateOptions With {
                    .Timeout = timeoutMs
                })
            Catch ex As TimeoutException
                _logger.LogWarning($"[Navigare] Pagina curentă nu a raportat 'Load' complet în {action.Timeout}s (dar continuăm).")
            End Try
        End If

    End Function

End Class
