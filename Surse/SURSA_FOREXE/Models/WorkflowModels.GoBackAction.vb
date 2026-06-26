Namespace WorkflowModels

    ''' <summary>
    ''' Simulează apăsarea butonului "Back" din browser.
    ''' </summary>
    Public Class GoBackAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "GoBack"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue
    End Class

End Namespace
