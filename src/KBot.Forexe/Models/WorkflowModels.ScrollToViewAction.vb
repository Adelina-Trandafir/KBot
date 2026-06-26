Namespace WorkflowModels

    ''' <summary>
    ''' ScrollToView action - Scrollează pagina până când elementul devine vizibil în viewport.
    ''' </summary>
    Public Class ScrollToViewAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "ScrollToView"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        ' Selectorul elementului țintă către care vrem să derulăm
        <WflRequired> Public Property Selector As String = String.Empty
    End Class

End Namespace
