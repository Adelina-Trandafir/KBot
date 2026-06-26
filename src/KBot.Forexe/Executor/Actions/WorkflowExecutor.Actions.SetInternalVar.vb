Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Sub ExecuteSetInternalVar(action As SetInternalVarAction)
        Dim resolvedName = ReplaceInternalVariables(action.Name)
        Dim resolvedValue = ReplaceInternalVariables(action.Value)

        ' Dacă valoarea conține încă {{...}} = vine din exterior (KBOT_STANDALONE/VBA)
        ' și e deja în _variables. Nu suprascriem.
        If resolvedValue.Contains("{{") Then
            _logger.LogDebug($"[SetInternalVar] '{resolvedName}' vine din exterior. Sar.")
            Return
        End If

        SetVariable(resolvedName, resolvedValue)
        _logger.LogSuccess($"[SetInternalVar] [[{resolvedName}]] = '{resolvedValue.Substring(0, Math.Min(resolvedValue.Length, 50))}'")
    End Sub

End Class
