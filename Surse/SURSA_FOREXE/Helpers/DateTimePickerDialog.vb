Public Class DateTimePickerDialog

    Public Property SelectedDate As DateTime

    Private Sub DateTimePickerDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        KBotTheme.ApplyTheme(Me)
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        SelectedDate = dtp.Value
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

End Class