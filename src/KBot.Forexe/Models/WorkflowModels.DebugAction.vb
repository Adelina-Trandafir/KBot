Namespace WorkflowModels

    ''' <summary>
    ''' LogDebug action - O acțiune specială care nu face nimic în execuție,
    ''' dar poate fi folosită pentru a marca puncte de interes în workflow și a afișa mesaje în log pentru debugging.
    ''' Poate fi checkpoint pentru a permite restart de la acest punct.
    ''' Nu se transmite la Access și nu afectează rezultatul final, fiind utilă doar pentru dezvoltare și depanare.
    ''' </summary>
    Public Class DebugAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Debug"
            End Get
        End Property

        Public Property Timeout As Integer = 5 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = True Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        Public Property HaltWhenDone As Boolean = False
        Public Property IncludeChildren As Boolean = False
    End Class

End Namespace
