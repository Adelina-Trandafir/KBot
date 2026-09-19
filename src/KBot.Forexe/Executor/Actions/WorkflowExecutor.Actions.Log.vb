Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Sub ExecuteLog(action As LogAction)
        Dim finalMessage As String = ReplaceInternalVariables(action.Message)
        ' Text the workflow author wrote for the operator: on the console whatever the
        ' verbosity (slice 0071), hence operatorFacing.
        Dim level As LogLevel
        Select Case action.Level.ToLower()
            Case "success" : level = LogLevel.Success
            Case "warning" : level = LogLevel.Warning
            Case "error" : level = LogLevel.Error
            Case "info" : level = LogLevel.Info
            Case Else : level = LogLevel.Normal
        End Select
        _logger.LogOperator(finalMessage, level)
        RaiseEvent OnLogMessage(finalMessage)
    End Sub

End Class