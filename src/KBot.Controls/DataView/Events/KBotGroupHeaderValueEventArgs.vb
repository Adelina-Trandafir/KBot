Option Strict On
Imports System.Collections.Generic

''' <summary>
''' Slice KE11 -- raised by <c>KBotDataView.GroupHeaderValuePicked</c> when the operator picks a value in the combo of a group header
''' (<see cref="KBotDataColumn.GroupHeaderCombo"/>). The grid writes nothing: the host puts the value in the rows it names.
''' </summary>
Public NotInheritable Class KBotGroupHeaderValueEventArgs
    Inherits EventArgs

    ''' <summary>The column whose combo was used.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>The item picked from the column's <c>ComboItems</c>.</summary>
    Public ReadOnly Property Value As Object

    ''' <summary>The model indexes (<c>Rows(i)</c>) of every row of the group, sub-groups included.</summary>
    Public ReadOnly Property RowIndexes As IReadOnlyList(Of Integer)

    Public Sub New(k_columnKey As String, k_value As Object, k_rowIndexes As IReadOnlyList(Of Integer))
        ColumnKey = k_columnKey
        Value = k_value
        RowIndexes = k_rowIndexes
    End Sub

End Class
