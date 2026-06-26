Namespace WorkflowModels

    ''' <summary>
    ''' Reload action - Reîncarcă pagina curentă.
    ''' Poate fi util după acțiuni care pot lăsa pagina într-o stare instabilă sau
    ''' când vrem să ne asigurăm că avem cea mai recentă versiune a conținutului.
    ''' Opțional, poate aștepta navigația și poate avea un timeout mai mare
    ''' pentru a permite încărcarea completă a paginii.
    ''' Nu este checkpoint implicit, dar poate fi setat ca atare
    ''' dacă vrem să permitem restart de la acest punct în caz de eșec ulterior.
    ''' </summary>
    Public Class ReloadAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "Reload"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        Public Property WaitNavigation As Boolean = True
    End Class

End Namespace
