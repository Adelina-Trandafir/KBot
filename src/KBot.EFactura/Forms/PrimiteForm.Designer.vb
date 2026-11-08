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
        components = New ComponentModel.Container()
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
        capBar.Location = New System.Drawing.Point(4, 4)
        capBar.Margin = New System.Windows.Forms.Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New System.Drawing.Size(1642, 55)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Facturi primite"
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(primite)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(4, 59)
        pnlCard.Margin = New System.Windows.Forms.Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New System.Windows.Forms.Padding(4)
        pnlCard.Size = New System.Drawing.Size(1642, 987)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        ' 
        ' primite
        ' 
        primite.AngajamentCode = ""
        primite.Dock = System.Windows.Forms.DockStyle.Fill
        primite.Font = New System.Drawing.Font("Calibri", 9F)
        primite.Location = New System.Drawing.Point(4, 4)
        primite.Margin = New System.Windows.Forms.Padding(0)
        primite.Name = "primite"
        primite.Size = New System.Drawing.Size(1634, 979)
        primite.TabIndex = 0
        ' 
        ' PrimiteForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(1650, 1050)
        Controls.Add(pnlCard)
        Controls.Add(capBar)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1650, 1050)
        Name = "PrimiteForm"
        Padding = New System.Windows.Forms.Padding(4, 4, 4, 4)
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
