Namespace WorkflowModels

    ''' <summary>
    ''' ForEachVar action - Iterează printr-un array JSON stocat într-o variabilă și expune câmpurile fiecărui element ca variabile temporare pentru acțiunile copil.
    ''' </summary>
    Public Class ForEachVarAction
        Implements ILoopAction
        Public Property RuntimeIndex As Integer = 0 Implements ILoopAction.RuntimeIndex

        Public ReadOnly Property ActionType As String Implements IWorkflowAction.ActionType
            Get
                Return "ForEachVar"
            End Get
        End Property

        Public Property Timeout As Integer = 30 Implements IWorkflowAction.Timeout
        Public Property IsCheckpoint As Boolean = False Implements IWorkflowAction.IsCheckpoint
        Public Property LogValue As String = String.Empty Implements IWorkflowAction.LogValue

        <WflRequired> Public Property Source As String = String.Empty        ' numele variabilei JSON array: [[MY_VAR]]
        <WflRequired> Public Property ItemPrefix As String = String.Empty    ' prefix pentru câmpurile expuse: ex "RAND"
        Public Property IndexVariable As String = String.Empty ' opțional, ca la ForEach

        Public Property CollectKey As String = String.Empty 'Dacă setat, după fiecare iterație colectează variabilele al căror nume conține valoarea acestui câmp ca sufix.
        Public Property CollectFields As String = String.Empty 'Daca setat, specifică o listă de câmpuri separate prin virgulă care trebuie colectate la fiecare iterație (ex: "RAND_id,RAND_name"). Dacă CollectKey e setat, se colectează toate variabilele care conțin sufixul specificat în CollectKey.
        Public Property UseMap As Boolean = False 'Daca e activat mai produce o variabilă de tip Map care conține perechi cheie-valoare pentru fiecare câmp colectat, unde cheia e valoarea din câmpul specificat ca și CollectKey. Ex: [[RAND_Map]]
        Public Property Children As New List(Of IWorkflowAction)
    End Class

End Namespace
