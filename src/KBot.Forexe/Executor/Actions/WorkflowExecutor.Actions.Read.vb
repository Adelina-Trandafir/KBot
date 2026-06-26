Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteReadAsync(action As ReadAction) As Task
        Dim finalSelector = ReplaceInternalVariables(action.Selector)

        Try

            LogStep(action, $"[Read] Inițializez citirea: {finalSelector}")

            Dim locator = _page.Locator(finalSelector)

            ' 1. Așteptăm ca elementul să fie efectiv vizibil (nu doar în DOM)
            ' Folosim timeout-ul specificat în acțiune
            Await locator.WaitForAsync(New LocatorWaitForOptions With {
                .State = WaitForSelectorState.Visible,
                .Timeout = action.Timeout * 1000
            })

            ' 2. Determinăm dinamic tipul elementului (Tag Name)
            ' Asta face funcția "Smart": știe singură cum să citească
            Dim tagName As String = Await locator.EvaluateAsync(Of String)("el => el.tagName")
            Dim extractedValue As String = String.Empty

            ' 3. Alegem metoda corectă de extracție
            Select Case tagName.ToUpper()
                Case "INPUT", "TEXTAREA"
                    ' Pentru câmpuri editabile, vrem valoarea, nu textul
                    extractedValue = Await locator.InputValueAsync()

                Case "SELECT"
                    ' InputValueAsync returneaza "value" (GUID-ul).
                    ' Noi vrem "text"-ul opțiunii selectate (ex: Final).
                    ' Folosim EvaluateAsync pentru a accesa proprietatea .text a indexului selectat via JavaScript.
                    extractedValue = Await locator.EvaluateAsync(Of String)("el => el.selectedIndex >= 0 ? el.options[el.selectedIndex].text : ''")
                    ' -----------------------
                Case Else
                    ' Pentru div, span, strong, td, p -> Folosim InnerText (ce vede omul)
                    ' InnerText ignoră elementele hidden, spre deosebire de TextContent
                    extractedValue = Await locator.InnerTextAsync()
            End Select

            ' 4. Curățare (Sanitization)
            extractedValue = If(extractedValue, "").Trim()

            Dim saveToFinal = ReplaceInternalVariables(action.SaveTo)
            ' 5. Salvare și Logare
            If Not String.IsNullOrEmpty(saveToFinal) Then
                SetVariable(saveToFinal, extractedValue)
                _logger.LogSuccess($"[Read] '{extractedValue}' -> Salvat în {{{{{saveToFinal}}}}}")
            Else
                _logger.LogInfo($"[Read] Am citit '{extractedValue}', dar nu am salvat (SaveTo lipsă).")
            End If

        Catch ex As TimeoutException
            _logger.LogError($"[Read] Timeout! Elementul '{finalSelector}' nu a apărut în {action.Timeout} secunde.")
            Throw ' Aruncăm eroarea mai departe pentru a opri fluxul sau a fi gestionată de părinte
        Catch ex As Exception
            _logger.LogError($"[Read] Eroare critică la citire: {ex.Message}")
            Throw
        End Try
    End Function

End Class
