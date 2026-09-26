Imports KBot.Common
Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteClickAsync(action As ClickAction) As Task
        Dim finalSelector As String = ReplaceInternalVariables(action.Selector)
        If action.Commits AndAlso StopBeforeCommit Then
            Await StopBeforeCommitAsync(action, finalSelector)
        End If
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
            ' The operator's page rules (ForexeWatch.js, section 4) may hide the very thing
            ' this step clicks - the FOREXE menu while an angajament is open. Such a target
            ' gets the rules lifted for this one click; the next document has them back.
            Dim lifted As Boolean = Await LiftPageStylesIfHiddenAsync(locator)
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
            ' VB cannot Await in a Finally: the click's exception is caught, the rules are
            ' put back, then it is rethrown with its stack.
            Dim clickEx As Exception = Nothing
            Try
                Await locator.ClickAsync(clickOptions)
            Catch ex As Exception
                clickEx = ex
            End Try
            If lifted Then Await RestorePageStylesAsync()
            If clickEx IsNot Nothing Then
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(clickEx).Throw()
            End If
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

    ''' <summary>
    ''' True when the first element the locator finds is hidden ONLY by the operator's page
    ''' rules - in which case the rules are switched off in the page until
    ''' <see cref="RestorePageStylesAsync"/>. False for a target that is visible, missing, or
    ''' hidden by FOREXE itself; never throws (the click that follows reports the real problem).
    ''' </summary>
    Private Async Function LiftPageStylesIfHiddenAsync(locator As ILocator) As Task(Of Boolean)
        Try
            If Await locator.CountAsync() = 0 Then Return False
            Dim hidden As Boolean = Await locator.First.EvaluateAsync(Of Boolean)(
                "el => !!(window._kbotWatch && window._kbotWatch.hiddenByStyles && window._kbotWatch.hiddenByStyles(el))")
            If Not hidden Then Return False
            Await _page.EvaluateAsync(Of Object)("() => { if (window._kbotWatch) { window._kbotWatch.liftStyles(true); } }")
            _logger.LogDebug("[Click] Ținta e ascunsă de regulile paginii: le ridic pentru acest clic.")
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.LiftPageStylesIfHiddenAsync", ex)
            Return False
        End Try
    End Function

    ''' <summary>Puts the rules back. Quiet when the click navigated away: the new document has them on.</summary>
    Private Async Function RestorePageStylesAsync() As Task
        Try
            Await _page.EvaluateAsync(Of Object)("() => { if (window._kbotWatch) { window._kbotWatch.liftStyles(false); } }")
        Catch ex As Exception
            _logger.LogDebug("[Click] Regulile paginii nu au putut fi repuse (pagina s-a schimbat): " & ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Variable holding the capture of the page at the dry-run stop. NOT «Poza_*»: those are
    ''' the send's captures and would be stored on the DDF revision.
    ''' </summary>
    Public Const DryRunCaptureVariable As String = "Proba_Captura"

    ''' <summary>
    ''' Slice 0081-07, the dry run: the page is captured as it stands, and the run stops before a
    ''' click that would make FOREXE save. Always throws <see cref="WorkflowExitException"/>.
    ''' </summary>
    Private Async Function StopBeforeCommitAsync(action As ClickAction, finalSelector As String) As Task
        Try
            Dim bytes As Byte() = Await _page.ScreenshotAsync(New PageScreenshotOptions With {.FullPage = True})
            SetVariable(DryRunCaptureVariable, Convert.ToBase64String(bytes))
        Catch ex As Exception
            ' The capture only helps the reading; the stop itself must happen regardless.
            GlobalErrorLog.Write("WorkflowExecutor.StopBeforeCommitAsync", ex)
        End Try
        _stoppedBeforeCommit = True
        Dim stepName As String = If(String.IsNullOrWhiteSpace(action.LogValue), finalSelector, action.LogValue)
        _logger.LogWarning($"[Mod probă] Oprit înainte de pasul care salvează: {stepName}")
        Throw New WorkflowExitException(
            $"Mod probă: robotul s-a oprit înainte de pasul care salvează («{stepName}»). FOREXE nu a salvat nimic.")
    End Function

End Class
