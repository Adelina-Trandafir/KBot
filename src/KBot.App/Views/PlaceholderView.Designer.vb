<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PlaceholderView
    Inherits System.Windows.Forms.UserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblMessage = New Label()
        SuspendLayout()
        '
        ' lblMessage
        '
        lblMessage.Dock = DockStyle.Fill
        lblMessage.Font = New Font("Segoe UI", 11.0F)
        lblMessage.Location = New Point(0, 0)
        lblMessage.Name = "lblMessage"
        lblMessage.Size = New Size(400, 300)
        lblMessage.TabIndex = 0
        lblMessage.Text = "— în lucru —"
        lblMessage.TextAlign = ContentAlignment.MiddleCenter
        '
        ' PlaceholderView
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblMessage)
        Name = "PlaceholderView"
        Size = New Size(400, 300)
        ResumeLayout(False)
    End Sub

    Friend WithEvents lblMessage As Label
End Class
