Option Strict On

''' <summary>
''' <see cref="KBotComboBox.NewItemRequested"/>: the operator clicked the «new item» row.
''' <see cref="Text"/> is what was typed in the box at that moment (empty when the row was offered
''' on an empty list, or when the box is not editable).
''' </summary>
Public NotInheritable Class KBotComboNewItemEventArgs
    Inherits EventArgs

    Public ReadOnly Property Text As String

    Public Sub New(text As String)
        Me.Text = If(text, String.Empty)
    End Sub
End Class
