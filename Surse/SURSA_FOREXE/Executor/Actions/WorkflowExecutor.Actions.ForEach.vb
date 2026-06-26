Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteForEachAsync(action As ForEachAction) As Task
        Dim parsedSelector = ReplaceInternalVariables(action.Selector)

        _logger.LogInfo($"[ForEach] Încep bucla pentru selector: {parsedSelector}")

        ' 1. Găsim elementele părinte (ex: rândurile tabelului)
        Dim elements = _page.Locator(parsedSelector)
        Dim count = Await elements.CountAsync()

        If count = 0 Then
            _logger.LogWarning("[ForEach] 0 elemente găsite. Sar peste.")
            Return
        End If

        ' 2. CREĂM CONTEXTUL ȘI ÎL PUNEM PE STIVĂ
        Dim loopCtx As New LoopContext With {
        .ActionType = "ForEach",
        .RuntimeIndex = 1,
        .IndexVariableName = action.IndexVariable
    }
        _executionStack.Push(loopCtx)

        Try
            ' 3. Iterăm prin elemente
            For i As Integer = 0 To count - 1
                ' ACTUALIZĂM INDEXUL ÎN CONTEXTUL EXISTENT PE STIVĂ
                loopCtx.RuntimeIndex = i + 1

                _logger.LogInfo($"--- Procesare rândul {loopCtx.RuntimeIndex} din {count} ---")

                ' === LOGICA DE CLICK (REPARATĂ) ===
                Dim parsedClickElement = ReplaceInternalVariables(action.ClickElement)
                If Not String.IsNullOrEmpty(parsedClickElement) Then
                    Try
                        ' A. Luăm locatorul pentru rândul curent (Specific Row)
                        Dim currentRow = elements.Nth(i)

                        ' B. Construim locatorul butonului RELATIV la rândul curent
                        ' Folosim ReplaceInternalVariables în caz că selectorul de click are variabile
                        Dim clickSelector As String = ReplaceInternalVariables(action.ClickElement)
                        Dim clickTarget = currentRow.Locator(clickSelector)

                        _logger.LogInfo($"[ForEach] Click pe element: {clickSelector}")

                        ' C. Executăm click-ul
                        Await clickTarget.ClickAsync()
                        Await WaitForWicketIdleAsync()

                        ' Opțional: Dacă click-ul provoacă animații, o mică pauză poate ajuta
                        ' Await Task.Delay(100)

                    Catch ex As Exception
                        _logger.LogError($"[ForEach] Eroare la click în iterația {i + 1}: {ex.Message}")
                        ' Poți decide dacă dai Throw sau Continue For
                        Throw
                    End Try
                End If
                ' ===================================

                ' 4. Executăm acțiunile copil
                ' (Ele vor rula ÎN CONTEXTUL creat de click-ul de mai sus, dacă acel click a deschis ceva)
                If action.Children.Count > 0 Then
                    Await ExecuteActionsAsync(action.Children, True)
                End If
            Next
        Finally
            ' 5. CURĂȚĂM STIVA OBLIGATORIU
            _executionStack.Pop()
        End Try
    End Function

End Class
