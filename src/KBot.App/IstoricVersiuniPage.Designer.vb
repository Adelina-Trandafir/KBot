<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class IstoricVersiuniPage
    Inherits Global.KBot.Theming.KBotShellForm

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
        istoricView = New SetariIstoricView()
        SuspendLayout()
        '
        ' istoricView
        '
        istoricView.Dock = DockStyle.Fill
        istoricView.Location = New Point(0, 0)
        istoricView.Margin = New Padding(0)
        istoricView.Name = "istoricView"
        istoricView.Size = New Size(1920, 840)
        istoricView.TabIndex = 0
        '
        ' IstoricVersiuniPage
        '
        AutoScaleDimensions = New SizeF(144.0F, 144.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1920, 840)
        Controls.Add(istoricView)
        FormBorderStyle = FormBorderStyle.None
        Name = "IstoricVersiuniPage"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "K-BOT — Istoric versiuni"
        ResumeLayout(False)
    End Sub

    Friend WithEvents istoricView As SetariIstoricView
End Class
