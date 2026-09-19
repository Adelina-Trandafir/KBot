<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UpdaterForm
    Inherits System.Windows.Forms.Form

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
        lblTitlu = New System.Windows.Forms.Label()
        lblStare = New System.Windows.Forms.Label()
        bara = New System.Windows.Forms.ProgressBar()
        lblDetaliu = New System.Windows.Forms.Label()
        SuspendLayout()
        '
        ' lblTitlu
        '
        lblTitlu.AutoSize = True
        lblTitlu.Font = New System.Drawing.Font("Segoe UI", 12.0F, System.Drawing.FontStyle.Bold)
        lblTitlu.Location = New System.Drawing.Point(20, 18)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New System.Drawing.Size(180, 21)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Actualizare K-BOT"
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Location = New System.Drawing.Point(20, 52)
        lblStare.Name = "lblStare"
        lblStare.Size = New System.Drawing.Size(440, 22)
        lblStare.TabIndex = 1
        lblStare.Text = "Se pregătește…"
        '
        ' bara
        '
        bara.Location = New System.Drawing.Point(20, 80)
        bara.Name = "bara"
        bara.Size = New System.Drawing.Size(440, 20)
        bara.Style = System.Windows.Forms.ProgressBarStyle.Marquee
        bara.TabIndex = 2
        '
        ' lblDetaliu
        '
        lblDetaliu.AutoEllipsis = True
        lblDetaliu.ForeColor = System.Drawing.SystemColors.GrayText
        lblDetaliu.Location = New System.Drawing.Point(20, 108)
        lblDetaliu.Name = "lblDetaliu"
        lblDetaliu.Size = New System.Drawing.Size(440, 20)
        lblDetaliu.TabIndex = 3
        lblDetaliu.Text = ""
        '
        ' UpdaterForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0F, 96.0F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(480, 144)
        ControlBox = False
        Controls.Add(lblDetaliu)
        Controls.Add(bara)
        Controls.Add(lblStare)
        Controls.Add(lblTitlu)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "UpdaterForm"
        ShowInTaskbar = True
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "Actualizare K-BOT"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblTitlu As System.Windows.Forms.Label
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents bara As System.Windows.Forms.ProgressBar
    Friend WithEvents lblDetaliu As System.Windows.Forms.Label
End Class
