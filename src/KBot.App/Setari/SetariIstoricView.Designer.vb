<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariIstoricView
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
        components = New ComponentModel.Container()
        edIstoric = New Global.KBot.Controls.KBotRichTextEditor()
        SuspendLayout()
        '
        ' edIstoric -- read only, no toolbar, no footer; the text wraps, only the vertical bar scrolls
        '
        edIstoric.CollapseButton = False
        edIstoric.Dock = DockStyle.Fill
        edIstoric.FooterVisible = False
        edIstoric.HeaderVisible = False
        edIstoric.Location = New Point(0, 0)
        edIstoric.Name = "edIstoric"
        edIstoric.Size = New Size(600, 400)
        edIstoric.TabIndex = 0
        '
        ' SetariIstoricView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(edIstoric)
        Name = "SetariIstoricView"
        Size = New Size(600, 400)
        ResumeLayout(False)
    End Sub

    Friend WithEvents edIstoric As Global.KBot.Controls.KBotRichTextEditor
End Class
