Imports Microsoft.Playwright
Imports System.Windows.Forms
Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteGetAttribute(action As GetAttributeAction) As Task
        Dim resolvedSelector = ReplaceInternalVariables(action.Selector)
        Dim resolvedAttribute = ReplaceInternalVariables(action.AttributeName)
        LogStep(action, $"Obtin atribut '{resolvedAttribute}' din {resolvedSelector}")

        Dim element As ILocator
        Dim attributeValue As String

        Try
            element = _page.Locator(resolvedSelector)

            ' Așteptăm ca elementul să fie prezent în DOM cu timeout
            Await element.WaitForAsync(New LocatorWaitForOptions With {
            .State = WaitForSelectorState.Attached,
            .Timeout = action.Timeout * 1000
        })
        Catch ex As Exception
            _logger.LogError($"Eroare la găsirea elementului '{resolvedSelector}': {ex.Message}")
            Return
        End Try

        Try
            attributeValue = Await element.GetAttributeAsync(resolvedAttribute)
        Catch ex As Exception
            _logger.LogError($"Eroare la obtinerea atributului '{resolvedAttribute}' pentru '{resolvedSelector}': {ex.Message}")
            Return
        End Try

        action.Result = attributeValue

        If Not String.IsNullOrEmpty(action.SaveTo) Then
            Dim resolvedSaveTo = ReplaceInternalVariables(action.SaveTo)
            SetVariable(resolvedSaveTo, attributeValue)
            _logger.LogSuccess($"[GetAttribute] '{attributeValue}' -> salvat in [[{resolvedSaveTo}]]")
        End If

        If action.ShowErrorMessage Then
            MessageBox.Show($"{attributeValue}", "EROARE", MessageBoxButtons.OK, MessageBoxIcon.Error)
            _logger.LogError($"Atribut '{resolvedAttribute}' pentru selector '{resolvedSelector}': {attributeValue}")
        ElseIf action.ShowNormalMessage Then
            MessageBox.Show($"{attributeValue}", "INFORMATIE", MessageBoxButtons.OK, MessageBoxIcon.Information)
            _logger.LogInfo($"Atribut '{resolvedAttribute}' pentru selector '{resolvedSelector}': {attributeValue}")
        End If
    End Function

End Class
