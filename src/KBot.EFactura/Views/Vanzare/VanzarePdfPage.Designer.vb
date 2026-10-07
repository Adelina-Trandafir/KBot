<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VanzarePdfPage
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
        components = New System.ComponentModel.Container()
        lblNota = New System.Windows.Forms.Label()
        SuspendLayout()
        '
        ' lblNota
        '
        lblNota.Dock = System.Windows.Forms.DockStyle.Fill
        lblNota.Margin = New System.Windows.Forms.Padding(0)
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 0
        lblNota.Text = "Vizualizatorul de documente nu este disponibil în această fereastră."
        lblNota.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        lblNota.Visible = False
        '
        ' VanzarePdfPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(lblNota)
        Name = "VanzarePdfPage"
        Size = New System.Drawing.Size(757, 526)
        ResumeLayout(False)
    End Sub

    Friend WithEvents lblNota As System.Windows.Forms.Label
End Class
