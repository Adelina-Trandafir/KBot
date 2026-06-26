Namespace WorkflowModels

    ''' <summary>
    ''' SetInternalVar action - Setează o variabilă internă care poate fi folosită în acțiunile copil sau în alte părți ale workflow-ului. Nu se expune în LogValue și nu se transmite la Access. Util pentru stocarea de valori temporare sau de control al fluxului.
    ''' </summary>
    Public Class SetInternalVarAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "SetInternalVar"
            End Get
        End Property

        Public Property Timeout As Integer = 0 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Name As String = String.Empty
        <WflRequired> Public Property Value As String = String.Empty
    End Class

End Namespace
