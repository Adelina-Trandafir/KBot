Option Strict On

''' <summary>
''' Ridicat DUPĂ ce valoarea unei celule s-a schimbat efectiv (commit de editare sau comutare
''' de bifă/opțiune). Rândul e deja marcat „murdar” în acest moment.
''' </summary>
Public NotInheritable Class KBotCellValueEventArgs
    Inherits KBotCellEventArgs

    ''' <summary>Valoarea dinaintea schimbării.</summary>
    Public ReadOnly Property OldValue As Object

    ''' <summary>Valoarea de după schimbare.</summary>
    Public ReadOnly Property NewValue As Object

    Public Sub New(columnKey As String, rowIndex As Integer, oldValue As Object, newValue As Object)
        MyBase.New(columnKey, rowIndex)
        _OldValue = oldValue
        _NewValue = newValue
    End Sub

End Class
