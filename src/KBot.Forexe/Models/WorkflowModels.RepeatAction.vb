Namespace WorkflowModels

    ''' <summary>
    ''' Execută o serie de acțiuni copil de un număr specificat de ori.
    ''' </summary>
    Public Class RepeatAction
        Implements ILoopAction
        Public Property RuntimeIndex As Integer = 0 Implements ILoopAction.RuntimeIndex

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Repeat"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        ' Numărul de repetări
        Public Property Iterations As Integer = 1

        ' Lista de acțiuni care se vor repeta
        Public Property Children As New List(Of IWorkflowAction)

        ' Numele variabilei de index (opțional)
        Public Property IndexVariable As String = String.Empty
    End Class

End Namespace
