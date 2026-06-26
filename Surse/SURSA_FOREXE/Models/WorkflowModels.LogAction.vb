Namespace WorkflowModels

    ''' <summary>
    ''' Log action - Loghează un mesaj personalizat în logul de execuție.
    ''' Poate fi folosit pentru a marca puncte importante în workflow sau pentru a transmite informații relevante
    ''' despre starea curentă. Nu afectează execuția și nu este checkpoint implicit,
    ''' dar poate fi setat ca atare dacă vrem să permitem restart de la acest punct.
    ''' LogValue poate conține variabile care vor fi înlocuite la runtime,
    ''' permițând mesaje dinamice bazate pe datele curente.
    ''' </summary>
    Public Class LogAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Log"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Message As String = String.Empty
        Public Property Level As String = "info"
    End Class

End Namespace
