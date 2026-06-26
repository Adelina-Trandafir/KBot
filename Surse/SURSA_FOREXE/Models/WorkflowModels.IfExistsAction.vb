Namespace WorkflowModels

    ''' <summary>
    ''' IfExists action - Execută acțiunile copil dacă există cel puțin un element care corespunde selectorului.
    ''' </summary>
    Public Class IfExistsAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "IfExists"
            End Get
        End Property

        Public Property Timeout As Integer = 5 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequired> Public Property Children As New List(Of IWorkflowAction)
        Public Property ElseChildren As New List(Of IWorkflowAction)
        Public Property Strict As Boolean = False  ' False = .First, True = comportament default Playwright (pică la multiple)
        Public Property JsCondition As String = String.Empty
    End Class

End Namespace
