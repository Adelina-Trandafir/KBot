Namespace WorkflowModels

    ''' <summary>
    ''' Exit action - Similar cu Stop, dar aruncă o excepție specială care
    ''' poate fi tratată în KBOT_STANDALONE pentru a face curățenie sau a returna un mesaj specific în Access.
    ''' </summary>
    Public Class ExitAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Exit"
            End Get
        End Property

        Public Property Timeout As Integer = 0 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = True Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        Public Property Message As String = "Execuție anulată manual"
    End Class

End Namespace
