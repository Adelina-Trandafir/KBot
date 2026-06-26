Namespace WorkflowModels

    ''' <summary>
    ''' IfUnique action - Similar cu IfExists, dar verifică că există exact un singur element care corespunde selectorului.
    ''' </summary>
    Public Class IfUniqueAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "IfUnique"
            End Get
        End Property

        Public Property Timeout As Integer = 5 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequired> Public Property Children As New List(Of IWorkflowAction)
        Public Property ElseChildren As New List(Of IWorkflowAction)
        Public Property OnlyIfVisible As Boolean = False
    End Class

End Namespace
