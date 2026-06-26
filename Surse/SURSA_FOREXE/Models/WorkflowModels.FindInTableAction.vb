Namespace WorkflowModels

    ''' <summary>
    ''' Caută un rând într-un tabel bazat pe o coloană și o valoare.
    ''' </summary>
    Public Class FindInTableAction
        Implements IWorkflowAction

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "FindInTable"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue
        <WflRequired> Public Property Selector As String = String.Empty
        <WflRequired> Public Property FieldName As String = String.Empty
        <WflRequired> Public Property Value As String = String.Empty
        Public Property SaveRowTo As String = String.Empty
        Public Property ClickSelector As String = String.Empty
        Public Property Children As New List(Of IWorkflowAction)

        ''' <summary>
        ''' Transformări aplicate IDENTIC pe valoarea din celulă și pe value="" înainte de comparație.
        ''' Se înlănțuie cu | (pipe). Exemple:
        '''
        ''' fieldTransform="trim"
        '''   → elimină spații: "  AAB  " → "AAB"
        '''
        ''' fieldTransform="lower"
        '''   → litere mici: "AAB" → "aab"
        '''
        ''' fieldTransform="upper"
        '''   → litere mari: "aab" → "AAB"
        '''
        ''' fieldTransform="stripNonAlphaNum"
        '''   → elimină separatori: "02E-65.04.02" → "02E650402"
        '''
        ''' fieldTransform="left:3"
        '''   → primele 3 caractere: "AAB123" → "AAB"
        '''
        ''' fieldTransform="right:4"
        '''   → ultimele 4 caractere: "AAB123" → "B123"
        '''
        ''' fieldTransform="regex:/\s+/g:"
        '''   → elimină toate spațiile: "A A B" → "AAB"
        '''
        ''' fieldTransform="regex:/[^a-zA-Z0-9\-\.]/g:"
        '''   → păstrează doar alfanumerice, - și .: "02E-65.04 extra" → "02E-65.04"
        '''
        ''' fieldTransform="trim|stripNonAlphaNum|lower"
        '''   → combinat: "  02E-65.04.02  " → "02e650402"
        '''
        ''' fieldTransform="trim|left:15|stripNonAlphaNum"
        '''   → trim, primele 15 caractere, apoi elimină separatori
        ''' </summary>
        Public Property FieldTransform As String = String.Empty
    End Class

End Namespace
