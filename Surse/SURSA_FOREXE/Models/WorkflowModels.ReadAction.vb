Namespace WorkflowModels

    ''' <summary>
    ''' Read action - citește textul unui element și îl salvează într-o variabilă
    ''' sau îl transmite la Access prin LogValue
    ''' </summary>
    Public Class ReadAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Read"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        <WflRequiredOneOf("wheretosave")> Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequiredOneOf("wheretosave")> Public Property SaveTo As String = Nothing
    End Class

End Namespace
