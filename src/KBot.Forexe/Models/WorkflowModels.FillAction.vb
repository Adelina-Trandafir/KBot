Namespace WorkflowModels

    ''' <summary>
    ''' Fill action - completează un input cu o valoare specificată sau dinamică (variabilă)
    ''' </summary>
    Public Class FillAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Fill"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequired> Public Property Value As String = String.Empty
        Public Property Clear As Boolean = True
        Public Property PickFromList As Boolean = False
        ''' <summary>Dacă true, folosește PressSequentially în loc de Fill — compatibil cu select2 și alte input-uri JS-heavy.</summary>
        Public Property Sequential As Boolean = False
    End Class

End Namespace
