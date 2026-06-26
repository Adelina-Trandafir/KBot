Namespace WorkflowModels

    ''' <summary>
    ''' ScrapeTable action
    ''' </summary>
    Public Class ScrapeTableAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "ScrapeTable"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String
        <WflRequiredOneOf("wheretosave")> Public Property SaveTo As String
        <WflRequiredOneOf("wheretosave")> Public Property SaveToFile As String
        Public Property NextPageSelector As String = String.Empty
        Public Property PrevPageSelector As String = String.Empty
        Public Property FirstPageSelector As String = String.Empty
        Public Property LastPageSelector As String = String.Empty
        Public Property WaitSelector As String = String.Empty
        ' result filtering properties
        Public Property SkipFirstNRows As Integer = 0
        Public Property SkipFirstNColumns As Integer = 0
        Public Property SkipLastNRows As Integer = 0
        Public Property SkipLastNColumns As Integer = 0
        Public Property StartFromLast As Boolean = False
        Public Property ExitIfCellEquals As String = String.Empty   ' "Coloana:Valoare" sau "Coloana:~:Regex"
        Public Property ExitIfCellDate As String = String.Empty     ' "Coloana:op:Valoare"  (op: eq lt gt lte gte neq)
        Public Property FingerprintSelector As String = String.Empty
        Public Property Strict As Boolean = False
        Public Property Page As String = String.Empty   ' "first" / "last" / numar
        Public Property Row As String = String.Empty    ' "first" / "last" / numar
    End Class

End Namespace
