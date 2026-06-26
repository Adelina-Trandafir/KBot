Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Sub ExecuteLog(action As LogAction)
        Dim finalMessage As String = ReplaceInternalVariables(action.Message)
        Select Case action.Level.ToLower()
            Case "success" : _logger.LogSuccess(finalMessage)
            Case "warning" : _logger.LogWarning(finalMessage)
            Case "error" : _logger.LogError(finalMessage)
            Case "info" : _logger.LogInfo(finalMessage)
            Case Else : _logger.LogNormal(finalMessage)
        End Select
        RaiseEvent OnLogMessage(finalMessage)
    End Sub

End Class
