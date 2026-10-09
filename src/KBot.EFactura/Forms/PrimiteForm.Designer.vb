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
        pnlJos = New System.Windows.Forms.Panel()
        lblStare = New System.Windows.Forms.Label()
        btnReimprospateaza = New System.Windows.Forms.Button()
        btnSincronizeaza = New System.Windows.Forms.Button()
        btnIesire = New System.Windows.Forms.Button()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
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
        ' pnlJos
        '
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnReimprospateaza)
        pnlJos.Controls.Add(btnSincronizeaza)
        pnlJos.Controls.Add(btnIesire)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(3, 847)
        pnlJos.Margin = New System.Windows.Forms.Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(1194, 58)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = System.Windows.Forms.DockStyle.Fill
        lblStare.Location = New System.Drawing.Point(133, 8)
        lblStare.Name = "lblStare"
        lblStare.Padding = New System.Windows.Forms.Padding(12, 0, 12, 0)
        lblStare.Size = New System.Drawing.Size(634, 42)
        lblStare.TabIndex = 3
        lblStare.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' btnReimprospateaza
        '
        btnReimprospateaza.Dock = System.Windows.Forms.DockStyle.Right
        btnReimprospateaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnReimprospateaza.Location = New System.Drawing.Point(767, 8)
        btnReimprospateaza.Name = "btnReimprospateaza"
        btnReimprospateaza.Size = New System.Drawing.Size(200, 42)
        btnReimprospateaza.TabIndex = 2
        btnReimprospateaza.Text = "Reîmprospătează"
        tips.SetToolTipHeader(btnReimprospateaza, "Reîmprospătează")
        tips.SetToolTipText(btnReimprospateaza, "Citește din nou lista facturilor primite din baza de date (nu cheamă ANAF).")
        btnReimprospateaza.UseVisualStyleBackColor = True
        '
        ' btnSincronizeaza
        '
        btnSincronizeaza.Dock = System.Windows.Forms.DockStyle.Right
        btnSincronizeaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSincronizeaza.Location = New System.Drawing.Point(967, 8)
        btnSincronizeaza.Name = "btnSincronizeaza"
        btnSincronizeaza.Size = New System.Drawing.Size(219, 42)
        btnSincronizeaza.TabIndex = 1
        btnSincronizeaza.Text = "Sincronizează cu ANAF"
        tips.SetToolTipHeader(btnSincronizeaza, "Sincronizează cu ANAF")
        tips.SetToolTipText(btnSincronizeaza, "Aduce de la ANAF facturile primite în ultimele zile (cel mult 60) și le scrie în baza de date.")
        btnSincronizeaza.UseVisualStyleBackColor = True
        '
        ' btnIesire
        '
        btnIesire.Dock = System.Windows.Forms.DockStyle.Left
        btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnIesire.Location = New System.Drawing.Point(8, 8)
        btnIesire.Name = "btnIesire"
        btnIesire.Size = New System.Drawing.Size(125, 42)
        btnIesire.TabIndex = 0
        btnIesire.Text = "Ieșire"
        tips.SetToolTipHeader(btnIesire, "Ieșire")
        tips.SetToolTipText(btnIesire, "Închide fereastra facturilor primite.")
        btnIesire.UseVisualStyleBackColor = True
        '
        ' PrimiteForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnIesire
        ClientSize = New System.Drawing.Size(1200, 908)
        Controls.Add(pnlCard)
        Controls.Add(pnlJos)
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
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents primite As PrimiteView
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents btnReimprospateaza As System.Windows.Forms.Button
    Friend WithEvents btnSincronizeaza As System.Windows.Forms.Button
    Friend WithEvents btnIesire As System.Windows.Forms.Button
End Class
