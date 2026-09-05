Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PlaceholderView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        busy = New KBotBusyBar()
        SuspendLayout()
        '
        ' lblMessage — adăugat PRIMUL (regula cardului: Fill întâi, apoi benzile)
        '
        lblMessage.Dock = DockStyle.Fill
        lblMessage.Font = New Font("Segoe UI", 11.0F)
        lblMessage.Location = New Point(0, 3)
        lblMessage.Name = "lblMessage"
        lblMessage.Size = New Size(400, 297)
        lblMessage.TabIndex = 1
        lblMessage.Text = "— în lucru —"
        lblMessage.TextAlign = ContentAlignment.MiddleCenter
        '
        ' busy — banda de ocupare; pornirea/oprirea o face codul, la VisibleChanged
        '
        busy.Dock = DockStyle.Top
        busy.Location = New Point(0, 0)
        busy.Name = "busy"
        busy.Size = New Size(400, 3)
        busy.TabIndex = 0
        '
        ' PlaceholderView
        '
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblMessage)
        Controls.Add(busy)
        Name = "PlaceholderView"
        Size = New Size(400, 300)
        ResumeLayout(False)
    End Sub

    Friend WithEvents lblMessage As Label
    Friend WithEvents busy As Global.KBot.Controls.KBotBusyBar
End Class
