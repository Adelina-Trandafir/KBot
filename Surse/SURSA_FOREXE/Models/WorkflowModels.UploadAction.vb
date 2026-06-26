Namespace WorkflowModels

    ''' <summary>
    ''' Upload action - Încarcă un fișier într-un input de tip file. Poate accepta fie o cale locală,
    ''' fie o variabilă care conține calea sau conținutul fișierului, în funcție de implementare.
    ''' </summary>
    Public Class UploadAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Upload"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequired> Public Property Path As String = String.Empty
    End Class

End Namespace
