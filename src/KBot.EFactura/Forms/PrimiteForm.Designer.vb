Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteForm
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
        components = New System.ComponentModel.Container()
        tips = New KBotToolTip(components)
        capBar = New KBotCaptionBar()
        pnlCard = New System.Windows.Forms.Panel()
        primite = New PrimiteView()
        pnlCard.SuspendLayout()
        SuspendLayout()
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_e_64
        capBar.Location = New System.Drawing.Point(3, 3)
        capBar.Margin = New System.Windows.Forms.Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowOptionsButton = False
        capBar.ShowTextScaleSlider = False
        capBar.Size = New System.Drawing.Size(1194, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Facturi primite"
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(primite)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(3, 69)
        pnlCard.Margin = New System.Windows.Forms.Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New System.Windows.Forms.Padding(12, 9, 12, 9)
        pnlCard.Size = New System.Drawing.Size(1194, 778)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' primite
        '
        primite.Dock = System.Windows.Forms.DockStyle.Fill
        primite.Location = New System.Drawing.Point(12, 9)
        primite.Margin = New System.Windows.Forms.Padding(0)
        primite.Name = "primite"
        primite.Size = New System.Drawing.Size(1170, 760)
        primite.TabIndex = 0
        '
        ' PrimiteForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(1200, 908)
        Controls.Add(pnlCard)
        Controls.Add(capBar)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1100, 700)
        Name = "PrimiteForm"
        Padding = New System.Windows.Forms.Padding(3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "E-Factura — facturi primite"
        pnlCard.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents primite As PrimiteView
End Class
