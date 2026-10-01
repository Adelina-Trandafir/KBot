<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HelpCaptureViewForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        picImagine = New PictureBox()
        CType(picImagine, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' picImagine
        '
        picImagine.Dock = DockStyle.Fill
        picImagine.Location = New Point(0, 0)
        picImagine.Margin = New Padding(0)
        picImagine.Name = "picImagine"
        picImagine.Size = New Size(900, 600)
        picImagine.SizeMode = PictureBoxSizeMode.Zoom
        picImagine.TabIndex = 0
        picImagine.TabStop = False
        '
        ' HelpCaptureViewForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(900, 600)
        Controls.Add(picImagine)
        MinimumSize = New Size(320, 240)
        Name = "HelpCaptureViewForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Imagine salvată"
        CType(picImagine, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents picImagine As PictureBox
End Class
