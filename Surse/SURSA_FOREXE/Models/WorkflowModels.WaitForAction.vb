Namespace WorkflowModels

    ''' <summary>
    ''' WaitFor action - Așteaptă până când un element devine vizibil sau dispare
    ''' </summary>
    Public Class WaitForAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "WaitFor"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        Public Property State As String = "visible"
        Public Property RefreshOnFail As Boolean = False
        Public Property MaxRetries As Integer = 3
        Public Property Strict As Boolean = False  ' False = .First, True = comportament default Playwright (pică la multiple)
    End Class

End Namespace
