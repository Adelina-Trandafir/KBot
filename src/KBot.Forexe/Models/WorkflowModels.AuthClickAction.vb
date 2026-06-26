Namespace WorkflowModels

    ''' <summary>
    ''' AuthClick action - similar cu Click, dar cu un timeout mai mare și opțiunea de a aștepta o anumită URL
    ''' după click pentru a confirma că autentificarea a reușit.
    ''' </summary>
    Public Class AuthClickAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "AuthClick"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        Public Property AuthTimeout As Integer = 120
        Public Property ExpectedUrlAfterAuth As String = String.Empty
        Public Property WaitNavigation As Boolean = True
    End Class

End Namespace
