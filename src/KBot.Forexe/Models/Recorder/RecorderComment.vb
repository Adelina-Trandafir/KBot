Imports WorkflowModels

''' <summary>
''' A note the compactor wants to leave in the generated file (loop candidates,
''' section headers). WflWriter turns it into an XML comment; WorkflowParser never
''' sees it, because XDocument comments are not elements.
''' </summary>
''' <remarks>
''' It rides in the action list rather than in a parallel structure so that the
''' order of notes and actions cannot drift apart.
''' </remarks>
Public Class RecorderComment
    Implements IWorkflowAction

    Public Sub New(text As String)
        Me.Text = If(text, String.Empty)
    End Sub

    ''' <summary>Comment body, written verbatim between the XML comment markers.</summary>
    Public Property Text As String

    Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
        Get
            Return "RecorderComment"
        End Get
    End Property

    Public Property Timeout As Integer = 0 Implements IWorkflowAction.Timeout
    Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
    Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

End Class
