Namespace WorkflowModels

    ''' <summary>
    ''' Include action
    ''' </summary>
    Public Class IncludeAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Include"
            End Get
        End Property

        Public Property Timeout As Integer = 0 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property WorkflowPath As String = String.Empty
    End Class

End Namespace
