Namespace WorkflowModels

    ''' <summary>
    ''' While action - Execută acțiunile copil atâta timp cât o condiție este adevărată
    ''' </summary>
    Public Class WhileAction
        Implements ILoopAction
        Public Property RuntimeIndex As Integer = 0 Implements ILoopAction.RuntimeIndex

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "While"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        ' Selectorul elementului pe care îl verificăm
        <WflRequired> Public Property Selector As String

        ' Condiția: "Visible" (implicit), "Hidden", "Present" (există în DOM)
        Public Property Condition As String = "Visible"

        ' Siguranță pentru a evita bucle infinite
        Public Property MaxIterations As Integer = 50

        Public Property RunFirstTime As Boolean = True
        Public Property Children As New List(Of IWorkflowAction)
        Public Property IndexVariable As String = String.Empty
        Public Property JsCondition As String = String.Empty
    End Class

End Namespace
