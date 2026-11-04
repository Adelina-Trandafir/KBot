Option Strict On

''' <summary>
''' Raised by <see cref="KBotDataView.EditorTextChanged"/> on every change of the text the operator is typing in a Text cell editor, before the cell is committed.
''' The host may replace <see cref="Text"/> (to strip characters, to cut the length): the editor then shows the replacement, the caret keeping its distance from the end.
''' </summary>
Public NotInheritable Class KBotEditorTextEventArgs
    Inherits EventArgs

    ''' <summary>The key of the column being edited.</summary>
    Public ReadOnly Property ColumnKey As String

    ''' <summary>The index of the row being edited.</summary>
    Public ReadOnly Property RowIndex As Integer

    ''' <summary>What the editor holds now; assign the text it must hold instead.</summary>
    Public Property Text As String

    Public Sub New(k_columnKey As String, k_rowIndex As Integer, k_text As String)
        _ColumnKey = k_columnKey
        _RowIndex = k_rowIndex
        Me.Text = k_text
    End Sub

End Class
