Namespace WorkflowModels

    ''' <summary>
    ''' Wait action - Așteaptă un număr specificat de secunde înainte de a continua cu următoarea acțiune.
    ''' </summary>
    Public Class WaitAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Wait"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        Public Property Seconds As Double = 1
    End Class

End Namespace
