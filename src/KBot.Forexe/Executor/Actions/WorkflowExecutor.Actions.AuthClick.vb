Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    ' The sentence the operator reads when a failure sentinel fires and the workflow gave
    ' none of its own. {0} is the URL the page ended up on.
    Private Const AuthRefusedMessageFormat As String =
        "FOREXE a respins autentificarea: certificatul nu este disponibil sau nu a fost acceptat (pagina a ajuns la {0})."

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

        ' 2. Așteptăm REZULTATUL: navigarea la URL-ul așteptat SAU una dintre santinelele de
        '    eșec (slice 0071). The three waits run side by side and the first one to speak
        '    decides. Before this, a refused certificate -- FOREXE answers it by redirecting
        '    to its logout page, not by hanging -- kept the step waiting the whole AuthTimeout
        '    on a page that had already said no.
        Dim successTask As Task
        If Not String.IsNullOrEmpty(action.ExpectedUrlAfterAuth) Then
            _logger.LogInfo($"[Auth] Aștept navigarea către: {action.ExpectedUrlAfterAuth} ...")

            ' WaitForURLAsync este funcția magică:
            ' - Așteaptă până când URL-ul se potrivește
            ' - Are timeout integrat
            ' - Suportă wildcards (ex: "**/dashboard/**")
            successTask = _page.WaitForURLAsync(action.ExpectedUrlAfterAuth, New PageWaitForURLOptions With {
                .Timeout = timeoutMs,
                .WaitUntil = WaitUntilState.Load ' Sau NetworkIdle pentru siguranță maximă
            })
        Else
            ' Fallback: Dacă nu ai definit un URL, așteptăm doar să se încarce pagina (NetworkIdle)
            _logger.LogInfo("[Auth] Nu s-a specificat 'ExpectedUrlAfterAuth'. Aștept doar NetworkIdle.")
            successTask = _page.WaitForLoadStateAsync(LoadState.NetworkIdle, New PageWaitForLoadStateOptions With {.Timeout = timeoutMs})
        End If

        Dim failTasks As New List(Of Task)
        If Not String.IsNullOrWhiteSpace(action.FailUrl) Then
            _logger.LogInfo($"[Auth] Santinelă de eșec (URL): {action.FailUrl}")
            failTasks.Add(_page.WaitForURLAsync(action.FailUrl, New PageWaitForURLOptions With {
                .Timeout = timeoutMs,
                .WaitUntil = WaitUntilState.Commit
            }))
        End If
        If Not String.IsNullOrWhiteSpace(action.FailSelector) Then
            _logger.LogInfo($"[Auth] Santinelă de eșec (element): {action.FailSelector}")
            ' Attached, not Visible: the logout table being in the document at all is the
            ' verdict. Locator waits survive navigations, so this keeps watching through the
            ' redirects of the authentication round-trip.
            failTasks.Add(_page.Locator(action.FailSelector).First.WaitForAsync(New LocatorWaitForOptions With {
                .State = WaitForSelectorState.Attached,
                .Timeout = timeoutMs
            }))
        End If

        Try
            Await RaceAuthWaitsAsync(action, successTask, failTasks)

            If Not String.IsNullOrEmpty(action.ExpectedUrlAfterAuth) Then
                _logger.LogSuccess($"[Auth] Succes! Am ajuns la: {_page.Url}")
            End If

        Catch ex As TimeoutException
            ' Not logged here: the message travels in the exception and ForexeRunner logs it
            ' once, with the phase (slice 0071). Logging it here too printed it twice.
            Dim msg As String = $"Timeout la autentificare: nu am ajuns la pagina așteptată în {action.AuthTimeout} secunde."
            Throw New Exception(msg)
        Finally
            ' The waits that lost the race keep running until their own timeout; their
            ' faults must be observed or they surface as unobserved task exceptions.
            ObserveLater(successTask)
            For Each t In failTasks
                ObserveLater(t)
            Next
        End Try

        ' Asigurăm finalizarea task-ului de click (deși probabil e gata de mult)
        Await clickTask
    End Function

    ''' <summary>
    ''' Waits until the success wait finishes (or times out) or a failure sentinel fires,
    ''' whichever comes first. A sentinel that faults (its own timeout, a destroyed context)
    ''' is simply dropped from the race; only a sentinel that COMPLETES is a verdict, and it
    ''' throws with the operator message. The success wait's own timeout propagates as-is.
    ''' </summary>
    Private Async Function RaceAuthWaitsAsync(action As AuthClickAction, successTask As Task, failTasks As List(Of Task)) As Task
        Dim pending As New List(Of Task) From {successTask}
        pending.AddRange(failTasks)

        Do
            Dim done As Task = Await Task.WhenAny(pending)

            If done Is successTask Then
                ' Rethrows the Playwright TimeoutException when the URL never came.
                Await successTask
                Return
            End If

            If done.Status = TaskStatus.RanToCompletion Then
                Dim landedOn As String = _page.Url
                Dim msg As String = If(String.IsNullOrWhiteSpace(action.FailMessage),
                                       String.Format(AuthRefusedMessageFormat, landedOn),
                                       ReplaceInternalVariables(action.FailMessage))
                Throw New Exception(msg)
            End If

            ' A sentinel that gave up is not a verdict either way -- the others keep watching.
            _logger.LogDebug($"[AuthClick] Santinelă încheiată fără verdict: {done.Exception?.GetBaseException().Message}")
            pending.Remove(done)
        Loop
    End Function

    ''' <summary>Marks a task's eventual fault as observed without waiting for it.</summary>
    Private Shared Sub ObserveLater(t As Task)
        If t Is Nothing Then Return
        If t.IsCompleted Then
            Dim ignored = t.Exception
            Return
        End If
        t.ContinueWith(Sub(x)
                           Dim ignored = x.Exception
                       End Sub, TaskContinuationOptions.OnlyOnFaulted)
    End Sub

End Class
