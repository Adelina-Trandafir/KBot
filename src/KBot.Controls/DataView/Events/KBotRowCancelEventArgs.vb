Option Strict On

''' <summary>
''' Raised by <see cref="KBotDataView.RowCancelRequested"/> when the operator pressed Esc a second time in a row (the first one closed the editor of a cell).
''' A host that knows better than the default (a row that exists only as an empty row of the table: remove it) sets <see cref="Handled"/>; otherwise the grid
''' puts the cells of the row back as they were before the operator touched it, raising <see cref="KBotDataView.CellValueChanged"/> for every cell that changes.
''' </summary>
Public NotInheritable Class KBotRowCancelEventArgs
    Inherits KBotCellEventArgs

    ''' <summary>True = the host cancelled the row itself; the grid does nothing more.</summary>
    Public Property Handled As Boolean

    Public Sub New(k_columnKey As String, k_rowIndex As Integer)
        MyBase.New(k_columnKey, k_rowIndex)
    End Sub

End Class
