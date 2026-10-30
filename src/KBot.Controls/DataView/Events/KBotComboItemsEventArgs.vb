Option Strict On
Imports System.Collections.Generic

''' <summary>
''' Raised when a combo cell is about to open its editor, so the host can give THAT cell its own list (the accounts a row may use depend on
''' the row's classification). <see cref="Items"/> starts as the column's <see cref="KBotDataColumn.ComboItems"/>; the handler may replace it.
''' </summary>
Public NotInheritable Class KBotComboItemsEventArgs
    Inherits EventArgs

    ''' <summary>The key of the column being edited.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>The index of the row being edited.</summary>
    Public ReadOnly Property RowIndex As Integer

    ''' <summary>The items the editor shows. Nothing = an empty list.</summary>
    Public Property Items As IList(Of Object)

    Public Sub New(k_columnKey As String, k_rowIndex As Integer, k_items As IList(Of Object))
        _ColumnKey = k_columnKey
        _RowIndex = k_rowIndex
        Me.Items = k_items
    End Sub

End Class
