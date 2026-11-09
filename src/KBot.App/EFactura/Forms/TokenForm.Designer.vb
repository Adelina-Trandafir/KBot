<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TokenForm
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
        tips = New KBot.Controls.KBotToolTip(components)
        btnReimprospateaza = New System.Windows.Forms.Button()
        btnAutorizeaza = New System.Windows.Forms.Button()
        btnInchide = New System.Windows.Forms.Button()
        pnlCard = New System.Windows.Forms.Panel()
        pnlDetalii = New System.Windows.Forms.Panel()
        lblEroare = New System.Windows.Forms.Label()
        lblAutorizat = New System.Windows.Forms.Label()
        lblCertificat = New System.Windows.Forms.Label()
        lblCui = New System.Windows.Forms.Label()
        lblUnitate = New System.Windows.Forms.Label()
        ntfMesaj = New Controls.KBotNotice()
        lblStare = New System.Windows.Forms.Label()
        pnlJos = New System.Windows.Forms.Panel()
        lblSep1 = New System.Windows.Forms.Label()
        busy = New Controls.KBotBusyBar()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlDetalii.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnReimprospateaza
        ' 
        btnReimprospateaza.Dock = System.Windows.Forms.DockStyle.Right
        btnReimprospateaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnReimprospateaza.Location = New System.Drawing.Point(431, 12)
        btnReimprospateaza.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        btnReimprospateaza.Name = "btnReimprospateaza"
        btnReimprospateaza.Size = New System.Drawing.Size(168, 63)
        btnReimprospateaza.TabIndex = 0
        btnReimprospateaza.Text = "Reîmprospătează"
        tips.SetToolTipHeader(btnReimprospateaza, "Reîmprospătează")
        tips.SetToolTipText(btnReimprospateaza, "Citește din nou starea tokenului de la server.")
        btnReimprospateaza.UseVisualStyleBackColor = True
        ' 
        ' btnAutorizeaza
        ' 
        btnAutorizeaza.Dock = System.Windows.Forms.DockStyle.Right
        btnAutorizeaza.Enabled = False
        btnAutorizeaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAutorizeaza.Location = New System.Drawing.Point(599, 12)
        btnAutorizeaza.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        btnAutorizeaza.Name = "btnAutorizeaza"
        btnAutorizeaza.Size = New System.Drawing.Size(168, 63)
        btnAutorizeaza.TabIndex = 1
        btnAutorizeaza.Text = "Autorizează"
        tips.SetToolTipHeader(btnAutorizeaza, "Autorizare cu certificatul")
        tips.SetToolTipText(btnAutorizeaza, "Obține (sau reînnoiește) tokenul ANAF al unității. Cere certificatul calificat de pe token sau card; PIN-ul îl cere programul lui. Tokenul rămâne pe server.")
        btnAutorizeaza.UseVisualStyleBackColor = True
        ' 
        ' btnInchide
        ' 
        btnInchide.Dock = System.Windows.Forms.DockStyle.Left
        btnInchide.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnInchide.Location = New System.Drawing.Point(12, 12)
        btnInchide.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New System.Drawing.Size(168, 63)
        btnInchide.TabIndex = 3
        btnInchide.Text = "Închide"
        tips.SetToolTipHeader(btnInchide, "Închide")
        tips.SetToolTipText(btnInchide, "Închide fereastra.")
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(pnlDetalii)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(3, 3)
        pnlCard.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(779, 474)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' pnlDetalii
        ' 
        pnlDetalii.Controls.Add(lblEroare)
        pnlDetalii.Controls.Add(lblAutorizat)
        pnlDetalii.Controls.Add(lblCertificat)
        pnlDetalii.Controls.Add(lblCui)
        pnlDetalii.Controls.Add(lblUnitate)
        pnlDetalii.Controls.Add(ntfMesaj)
        pnlDetalii.Controls.Add(lblStare)
        pnlDetalii.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDetalii.Location = New System.Drawing.Point(0, 69)
        pnlDetalii.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        pnlDetalii.Name = "pnlDetalii"
        pnlDetalii.Padding = New System.Windows.Forms.Padding(27, 18, 27, 18)
        pnlDetalii.Size = New System.Drawing.Size(779, 318)
        pnlDetalii.TabIndex = 2
        pnlDetalii.Tag = "Card"
        ' 
        ' lblEroare
        ' 
        lblEroare.Dock = System.Windows.Forms.DockStyle.Top
        lblEroare.Location = New System.Drawing.Point(27, 444)
        lblEroare.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblEroare.Name = "lblEroare"
        lblEroare.Padding = New System.Windows.Forms.Padding(0, 6, 0, 6)
        lblEroare.Size = New System.Drawing.Size(725, 66)
        lblEroare.TabIndex = 6
        ' 
        ' lblAutorizat
        ' 
        lblAutorizat.Dock = System.Windows.Forms.DockStyle.Top
        lblAutorizat.Location = New System.Drawing.Point(27, 402)
        lblAutorizat.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblAutorizat.Name = "lblAutorizat"
        lblAutorizat.Padding = New System.Windows.Forms.Padding(0, 6, 0, 6)
        lblAutorizat.Size = New System.Drawing.Size(725, 42)
        lblAutorizat.TabIndex = 5
        ' 
        ' lblCertificat
        ' 
        lblCertificat.Dock = System.Windows.Forms.DockStyle.Top
        lblCertificat.Location = New System.Drawing.Point(27, 360)
        lblCertificat.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblCertificat.Name = "lblCertificat"
        lblCertificat.Padding = New System.Windows.Forms.Padding(0, 6, 0, 6)
        lblCertificat.Size = New System.Drawing.Size(725, 42)
        lblCertificat.TabIndex = 4
        ' 
        ' lblCui
        ' 
        lblCui.Dock = System.Windows.Forms.DockStyle.Top
        lblCui.Location = New System.Drawing.Point(27, 318)
        lblCui.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblCui.Name = "lblCui"
        lblCui.Padding = New System.Windows.Forms.Padding(0, 6, 0, 6)
        lblCui.Size = New System.Drawing.Size(725, 42)
        lblCui.TabIndex = 3
        ' 
        ' lblUnitate
        ' 
        lblUnitate.Dock = System.Windows.Forms.DockStyle.Top
        lblUnitate.Location = New System.Drawing.Point(27, 276)
        lblUnitate.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblUnitate.Name = "lblUnitate"
        lblUnitate.Padding = New System.Windows.Forms.Padding(0, 6, 0, 6)
        lblUnitate.Size = New System.Drawing.Size(725, 42)
        lblUnitate.TabIndex = 2
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Top
        ntfMesaj.Location = New System.Drawing.Point(27, 180)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(725, 96)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' lblStare
        ' 
        lblStare.Dock = System.Windows.Forms.DockStyle.Top
        lblStare.Location = New System.Drawing.Point(27, 18)
        lblStare.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Padding = New System.Windows.Forms.Padding(0, 6, 0, 18)
        lblStare.Size = New System.Drawing.Size(725, 162)
        lblStare.TabIndex = 0
        lblStare.Text = "Se citește starea tokenului…"
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(btnReimprospateaza)
        pnlJos.Controls.Add(btnAutorizeaza)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 387)
        pnlJos.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(12, 12, 12, 12)
        pnlJos.Size = New System.Drawing.Size(779, 87)
        pnlJos.TabIndex = 3
        pnlJos.Tag = "Card"
        ' 
        ' lblSep1
        ' 
        lblSep1.Dock = System.Windows.Forms.DockStyle.Left
        lblSep1.Location = New System.Drawing.Point(180, 12)
        lblSep1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(251, 63)
        lblSep1.TabIndex = 2
        ' 
        ' busy
        ' 
        busy.Dock = System.Windows.Forms.DockStyle.Top
        busy.Location = New System.Drawing.Point(0, 60)
        busy.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        busy.Name = "busy"
        busy.Size = New System.Drawing.Size(779, 9)
        busy.TabIndex = 1
        ' 
        ' capBar
        ' 
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(779, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — tokenul ANAF"
        ' 
        ' TokenForm
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New System.Drawing.Size(785, 480)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "TokenForm"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — tokenul ANAF"
        pnlCard.ResumeLayout(False)
        pnlDetalii.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busy As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents pnlDetalii As System.Windows.Forms.Panel
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents lblUnitate As System.Windows.Forms.Label
    Friend WithEvents lblCui As System.Windows.Forms.Label
    Friend WithEvents lblCertificat As System.Windows.Forms.Label
    Friend WithEvents lblAutorizat As System.Windows.Forms.Label
    Friend WithEvents lblEroare As System.Windows.Forms.Label
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnReimprospateaza As System.Windows.Forms.Button
    Friend WithEvents btnAutorizeaza As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnInchide As System.Windows.Forms.Button
End Class
