Imports WorkflowModels

Partial Public Class WorkflowExecutor

    Private Async Function ExecuteWaitAsync(action As WaitAction) As Task
        LogStep(action, $"Așteptare fixă: {action.Seconds} secunde")
        Await Task.Delay(action.Seconds * 1000, _cancellationToken)
    End Function

End Class
