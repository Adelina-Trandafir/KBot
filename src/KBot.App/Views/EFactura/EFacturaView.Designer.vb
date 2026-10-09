<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class EFacturaView
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
        primite = New Global.KBot.EFactura.PrimiteView()
        SuspendLayout()
        '
        ' primite
        '
        primite.Dock = System.Windows.Forms.DockStyle.Fill
        primite.Location = New System.Drawing.Point(0, 0)
        primite.Margin = New System.Windows.Forms.Padding(0)
        primite.Name = "primite"
        primite.Size = New System.Drawing.Size(1000, 600)
        primite.TabIndex = 0
        '
        ' EFacturaView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(primite)
        Name = "EFacturaView"
        Size = New System.Drawing.Size(1000, 600)
        ResumeLayout(False)
    End Sub

    Friend WithEvents primite As Global.KBot.EFactura.PrimiteView
End Class
