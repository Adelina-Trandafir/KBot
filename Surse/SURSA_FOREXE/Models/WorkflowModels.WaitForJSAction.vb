Namespace WorkflowModels

    ''' <summary>
    ''' WaitForJS action - Evaluează o expresie JavaScript în browser și
    ''' așteaptă (prin polling) până când rezultatul îndeplinește condiția.
    ''' </summary>
    Public Class WaitForJSAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "WaitForJS"
            End Get
        End Property

        Public Property Timeout As Integer = 10 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        ''' <summary>Expresia JS de evaluat. Suportă [[VAR]] și {{VAR}}.</summary>
        <WflRequired> Public Property Expression As String = String.Empty

        ''' <summary>Valoarea așteptată ca string. Dacă lipsește, se aplică waitMode.</summary>
        Public Property ExpectedValue As String = Nothing

        ''' <summary>Operator de comparație: eq, neq, contains, regex (cu flags opțional).</summary>
        Public Property Compare As String = "eq"

        ''' <summary>
        ''' Comportament când ExpectedValue lipsește:
        '''   truthy  (default) — orice valoare JS truthy = succes
        '''   nonNull            — orice valoare non-null/undefined = succes
        '''   noWait             — execută o singură dată și salvează, fără polling
        ''' </summary>
        Public Property WaitMode As String = "truthy"

        ''' <summary>Interval de polling în milisecunde.</summary>
        Public Property PollingMs As Integer = 100

        ''' <summary>Variabilă internă unde se salvează rezultatul expresiei.</summary>
        Public Property SaveTo As String = String.Empty

    End Class

End Namespace
