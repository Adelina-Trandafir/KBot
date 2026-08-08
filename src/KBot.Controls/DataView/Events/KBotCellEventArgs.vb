Option Strict On

''' <summary>
''' Identifică o celulă într-un eveniment de interacțiune (click, dublu-click).
''' Spre deosebire de argumentele de FORMATARE, acestea NU se refolosesc: evenimentele de
''' interacțiune sunt rare (o acțiune a operatorului), deci o alocare per eveniment e
''' irelevantă — și scapă handler-ele de capcana „nu reține instanța”.
''' </summary>
Public Class KBotCellEventArgs
    Inherits EventArgs

    ''' <summary>Cheia coloanei.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>Indexul rândului.</summary>
    Public ReadOnly Property RowIndex As Integer

    Public Sub New(columnKey As String, rowIndex As Integer)
        _ColumnKey = columnKey
        _RowIndex = rowIndex
    End Sub

End Class
