Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteRepeatAsync(action As RepeatAction) As Task
        _logger.LogInfo($"[Repeat] Încep bucla de {action.Iterations} ori.")

        ' 1. CREĂM CONTEXTUL
        Dim loopCtx As New LoopContext With {
        .ActionType = "Repeat",
        .RuntimeIndex = 0,
        .IndexVariableName = action.IndexVariable ' Dacă vrei să poți accesa indexul cu nume custom
    }

        ' 2. PUSH PE STIVĂ
        _executionStack.Push(loopCtx)

        Try
            For i As Integer = 1 To action.Iterations
                ' Actualizăm indexul curent în contextul de pe stivă
                loopCtx.RuntimeIndex = i

                _logger.LogInfo($"--- [Repeat] Iterația {i} din {action.Iterations} ---")

                ' Executăm copiii
                If action.Children.Count > 0 Then
                    Await ExecuteActionsAsync(action.Children, True)
                End If
            Next
        Finally
            ' 3. POP DE PE STIVĂ
            _executionStack.Pop()
        End Try

        _logger.LogSuccess($"[Repeat] Bucla finalizată.")
    End Function

End Class
