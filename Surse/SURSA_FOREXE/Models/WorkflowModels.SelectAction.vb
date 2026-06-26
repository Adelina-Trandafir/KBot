Namespace WorkflowModels

    ''' <summary>
    ''' Select action - selectează o opțiune dintr-un dropdown bazat pe valoare sau text vizibil
    ''' </summary>
    Public Class SelectAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Select"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequiredOneOf("selection")> Public Property Value As String = Nothing
        <WflRequiredOneOf("selection")> Public Property Text As String = Nothing
        <WflRequiredOneOf("selection")> Public Property Index As Integer? = Nothing
    End Class

End Namespace
