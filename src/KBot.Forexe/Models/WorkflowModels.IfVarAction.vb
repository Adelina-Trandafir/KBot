Namespace WorkflowModels

    ''' <summary>
    ''' IfVar action - Executes children only if Value is not empty
    ''' </summary>
    Public Class IfVarAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "IfVar"
            End Get
        End Property

        Public Property Timeout As Integer = 0 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        ' Valoarea de verificat (va fi populată automat de KBOT_STANDALONE prin Replace)
        <WflRequired> Public Property Value As String = String.Empty
        Public Property Compare As String = ""  ' ex: "eq:2", "gt:1", "neq:0"

        <WflRequired> Public Property Children As New List(Of IWorkflowAction)
        Public Property ElseChildren As New List(Of IWorkflowAction)
    End Class

End Namespace
