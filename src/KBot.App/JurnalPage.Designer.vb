<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class JurnalPage
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
        jurnalView = New SetariJurnalView()
        lblStatus = New Label()
        SuspendLayout()
        '
        ' jurnalView
        '
        jurnalView.Dock = DockStyle.Fill
        jurnalView.Location = New Point(0, 0)
        jurnalView.Margin = New Padding(0)
        jurnalView.Name = "jurnalView"
        jurnalView.Size = New Size(1920, 800)
        jurnalView.TabIndex = 0
        '
        ' lblStatus
        '
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(0, 800)
        lblStatus.Margin = New Padding(0)
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New Padding(14, 0, 0, 0)
        lblStatus.Size = New Size(1920, 40)
        lblStatus.TabIndex = 1
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        ' JurnalPage
        '
        AutoScaleDimensions = New SizeF(144.0F, 144.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1920, 840)
        Controls.Add(jurnalView)
        Controls.Add(lblStatus)
        FormBorderStyle = FormBorderStyle.None
        Name = "JurnalPage"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "K-BOT — Jurnal activitate"
        ResumeLayout(False)
    End Sub

    Friend WithEvents jurnalView As SetariJurnalView
    Friend WithEvents lblStatus As Label
End Class
