Namespace WorkflowModels

    ''' <summary>
    ''' Click action - clicks an element found by a CSS selector.
    ''' </summary>
    Public Class ClickAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Click"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        Public Property WaitNavigation As Boolean = True
        Public Property Force As Boolean = False
        Public Property JsClick As Boolean = False
        Public Property ExpectNewTab As Boolean = False
        ''' <summary>
        ''' Slice 0081-07: this click makes FOREXE SAVE something (a new angajament, a
        ''' reservation row, a confirmation). Attribute <c>commits="true"</c>. In the dry run
        ''' (<see cref="WorkflowExecutor.StopBeforeCommit"/>) the run stops right before it.
        ''' </summary>
        Public Property Commits As Boolean = False
    End Class

End Namespace
