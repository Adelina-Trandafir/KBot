Namespace WorkflowModels

    ''' <summary>
    ''' Screenshot action - Captură de ecran a paginii curente și salvare locală sau într-o variabilă.
    ''' </summary>
    Public Class ScreenshotAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Screenshot"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequiredOneOf("wheretosave")> Public Property ScreenshotPath As String = Nothing
        <WflRequiredOneOf("wheretosave")> Public Property SaveTo As String = Nothing
    End Class

End Namespace
