Namespace WorkflowModels

    ''' <summary>
    ''' Stop action - O acțiune specială care aruncă o excepție pentru a opri execuția workflow-ului.
    ''' Poate fi folosită pentru a întrerupe execuția în anumite condiții sau pentru a marca un punct de oprire în workflow.
    ''' Mesajul poate fi personalizat și va fi transmis la Access pentru a informa utilizatorul despre motivul opririi.
    ''' </summary>
    Public Class StopAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Stop"
            End Get
        End Property

        Public Property Timeout As Integer = 0 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = True Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        Public Property Message As String = "Execuție oprită manual"
    End Class

End Namespace
