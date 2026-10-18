Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteReceptiiPage
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
        tips = New KBotToolTip(components)
        pnlForma = New System.Windows.Forms.Panel()
        lblNota = New System.Windows.Forms.Label()
        lblRezumat = New System.Windows.Forms.Label()
        SuspendLayout()
        '
        ' pnlForma: hosts the reception form of the shown invoice (slice KE12-04)
        '
        pnlForma.Dock = System.Windows.Forms.DockStyle.Fill
        pnlForma.Location = New System.Drawing.Point(3, 35)
        pnlForma.Margin = New System.Windows.Forms.Padding(0)
        pnlForma.Name = "pnlForma"
        pnlForma.Size = New System.Drawing.Size(751, 402)
        pnlForma.TabIndex = 1
        '
        ' lblNota
        '
        lblNota.Dock = System.Windows.Forms.DockStyle.Bottom
        lblNota.Location = New System.Drawing.Point(3, 437)
        lblNota.Margin = New System.Windows.Forms.Padding(0)
        lblNota.Name = "lblNota"
        lblNota.Padding = New System.Windows.Forms.Padding(2, 4, 2, 4)
        lblNota.Size = New System.Drawing.Size(751, 36)
        lblNota.TabIndex = 2
        '
        ' lblRezumat
        '
        lblRezumat.Dock = System.Windows.Forms.DockStyle.Top
        lblRezumat.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblRezumat.Location = New System.Drawing.Point(3, 3)
        lblRezumat.Margin = New System.Windows.Forms.Padding(0)
        lblRezumat.Name = "lblRezumat"
        lblRezumat.Padding = New System.Windows.Forms.Padding(2, 6, 2, 6)
        lblRezumat.Size = New System.Drawing.Size(751, 32)
        lblRezumat.TabIndex = 0
        lblRezumat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' PrimiteReceptiiPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(pnlForma)
        Controls.Add(lblNota)
        Controls.Add(lblRezumat)
        Name = "PrimiteReceptiiPage"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 0)
        Size = New System.Drawing.Size(757, 526)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlForma As System.Windows.Forms.Panel
    Friend WithEvents lblNota As System.Windows.Forms.Label
    Friend WithEvents lblRezumat As System.Windows.Forms.Label
End Class
