Namespace WorkflowModels

    ''' <summary>
    ''' ForEach action - Iterează printr-o listă de elemente
    ''' </summary>
    Public Class ForEachAction
        Implements ILoopAction
        Public Property RuntimeIndex As Integer = 0 Implements ILoopAction.RuntimeIndex

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "ForEach"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue
        <WflRequired> Public Property Selector As String
        Public Property ClickElement As String = String.Empty
        <WflRequired> Public Property Children As New List(Of IWorkflowAction)
        Public Property IndexVariable As String = String.Empty
    End Class

End Namespace
