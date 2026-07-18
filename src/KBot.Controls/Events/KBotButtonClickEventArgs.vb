Option Strict On

''' <summary>
''' Ridicat când o celulă de tip <see cref="KBotColumnType.Button"/> e acționată (click sau
''' Space). Butonul nu ține valoare și nu murdărește rândul — e pur acțiune.
''' </summary>
Public NotInheritable Class KBotButtonClickEventArgs
    Inherits KBotCellEventArgs

    Public Sub New(columnKey As String, rowIndex As Integer)
        MyBase.New(columnKey, rowIndex)
    End Sub

End Class
