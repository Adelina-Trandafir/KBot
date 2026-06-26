Namespace WorkflowModels

    ''' <summary>
    ''' GetAttribute action - Obține valoarea unui atribut al unui element și o salvează într-o variabilă
    ''' </summary>
    Public Class GetAttributeAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "GetAttribute"
            End Get
        End Property

        Public Property Timeout As Integer = 5 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = True Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequired> Public Property AttributeName As String = String.Empty
        Public Property Result As String = String.Empty
        Public Property ShowErrorMessage As Boolean = False
        Public Property ShowNormalMessage As Boolean = False
        Public Property SaveTo As String = String.Empty

    End Class

End Namespace
