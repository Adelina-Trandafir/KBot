Namespace WorkflowModels

    ''' <summary>
    ''' Minimize action - DEPRECATED: minimizează fereastra browser-ului pentru a nu deranja utilizatorul în timpul execuției.
    ''' </summary>
    Public Class MinimizeAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Minimize"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue
    End Class

End Namespace
