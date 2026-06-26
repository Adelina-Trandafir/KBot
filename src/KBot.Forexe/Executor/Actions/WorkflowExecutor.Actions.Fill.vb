Imports Microsoft.Playwright
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteFillAsync(action As FillAction) As Task
        ' Preluăm valoarea brută (ex: "10.01~10.02~10.03")
        ' Aceasta vine deja rezolvată din KBOT_STANDALONE pentru variabilele {{...}}, deci e un string lung.
        Dim valueToFill As String = action.Value

        ' --- LOGICA PICK FROM LIST ---
        If action.PickFromList Then
            ' 1. Aflăm la ce iterație suntem folosind variabila internă
            Dim idxString As String = GetVariable("RepeatIndex")
            Dim currentIndex As Integer = 1

            ' Dacă cumva nu suntem într-un loop, fallback la 1
            If Not Integer.TryParse(idxString, currentIndex) Then
                currentIndex = 1
            End If

            ' 2. Spargem valoarea după caracterul tildă (~)
            Dim parts As String() = valueToFill.Split("~"c)

            ' 3. Calculăm indexul din array (RepeatIndex e 1-based, Array e 0-based)
            Dim arrayIndex As Integer = currentIndex - 1

            ' 4. Extragem valoarea corectă
            If arrayIndex >= 0 AndAlso arrayIndex < parts.Length Then
                valueToFill = parts(arrayIndex).Trim()
            Else
                _logger.LogWarning($"[Fill] Indexul {currentIndex} e în afara listei (Lungime: {parts.Length}). Folosesc ultima valoare.")
                valueToFill = parts(parts.Length - 1).Trim()
            End If

            _logger.LogInfo($"[PickFromList] Am extras valoarea '{valueToFill}' pentru iterația {currentIndex}.")
        Else
            ' Dacă nu e PickFromList, dar conține variabile interne [[...]], le rezolvăm aici
            ' (De exemplu dacă vrei să scrii "Fisier_[[RepeatIndex]]")
            valueToFill = ReplaceInternalVariables(valueToFill)
        End If

        ' --- EXECUȚIA STANDARD ---
        Dim resolvedSelector = ReplaceInternalVariables(action.Selector)
        LogStep(action, $"Completez: {resolvedSelector} cu '{valueToFill}'")

        Dim locator = _page.Locator(resolvedSelector)
        Await locator.WaitForAsync(New LocatorWaitForOptions With {.State = WaitForSelectorState.Visible, .Timeout = action.Timeout * 1000})

        If action.Clear Then Await locator.ClearAsync()

        If action.Sequential Then
            Await locator.PressSequentiallyAsync(valueToFill, New LocatorPressSequentiallyOptions With {.Delay = 50})
        Else
            Await locator.FillAsync(valueToFill)
        End If

    End Function

End Class
