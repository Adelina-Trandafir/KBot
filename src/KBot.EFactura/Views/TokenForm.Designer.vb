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
        components = New System.ComponentModel.Container()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        pnlDetalii = New System.Windows.Forms.Panel()
        lblEroare = New System.Windows.Forms.Label()
        lblAutorizat = New System.Windows.Forms.Label()
        lblCertificat = New System.Windows.Forms.Label()
        lblCui = New System.Windows.Forms.Label()
        lblUnitate = New System.Windows.Forms.Label()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        lblStare = New System.Windows.Forms.Label()
        pnlJos = New System.Windows.Forms.Panel()
        btnReimprospateaza = New System.Windows.Forms.Button()
        btnAutorizeaza = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnInchide = New System.Windows.Forms.Button()
        busy = New Global.KBot.Controls.KBotBusyBar()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlDetalii.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(pnlDetalii)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(676, 416)
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
        pnlDetalii.Location = New System.Drawing.Point(0, 46)
        pnlDetalii.Name = "pnlDetalii"
        pnlDetalii.Padding = New System.Windows.Forms.Padding(18, 12, 18, 12)
        pnlDetalii.Size = New System.Drawing.Size(676, 312)
        pnlDetalii.TabIndex = 2
        pnlDetalii.Tag = "Card"
        '
        ' lblEroare
        '
        lblEroare.Dock = System.Windows.Forms.DockStyle.Top
        lblEroare.Location = New System.Drawing.Point(18, 296)
        lblEroare.Name = "lblEroare"
        lblEroare.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        lblEroare.Size = New System.Drawing.Size(640, 44)
        lblEroare.TabIndex = 6
        '
        ' lblAutorizat
        '
        lblAutorizat.Dock = System.Windows.Forms.DockStyle.Top
        lblAutorizat.Location = New System.Drawing.Point(18, 268)
        lblAutorizat.Name = "lblAutorizat"
        lblAutorizat.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        lblAutorizat.Size = New System.Drawing.Size(640, 28)
        lblAutorizat.TabIndex = 5
        '
        ' lblCertificat
        '
        lblCertificat.Dock = System.Windows.Forms.DockStyle.Top
        lblCertificat.Location = New System.Drawing.Point(18, 240)
        lblCertificat.Name = "lblCertificat"
        lblCertificat.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        lblCertificat.Size = New System.Drawing.Size(640, 28)
        lblCertificat.TabIndex = 4
        '
        ' lblCui
        '
        lblCui.Dock = System.Windows.Forms.DockStyle.Top
        lblCui.Location = New System.Drawing.Point(18, 212)
        lblCui.Name = "lblCui"
        lblCui.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        lblCui.Size = New System.Drawing.Size(640, 28)
        lblCui.TabIndex = 3
        '
        ' lblUnitate
        '
        lblUnitate.Dock = System.Windows.Forms.DockStyle.Top
        lblUnitate.Location = New System.Drawing.Point(18, 184)
        lblUnitate.Name = "lblUnitate"
        lblUnitate.Padding = New System.Windows.Forms.Padding(0, 4, 0, 4)
        lblUnitate.Size = New System.Drawing.Size(640, 28)
        lblUnitate.TabIndex = 2
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Top
        ntfMesaj.Location = New System.Drawing.Point(18, 120)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(640, 64)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' lblStare
        '
        lblStare.Dock = System.Windows.Forms.DockStyle.Top
        lblStare.Location = New System.Drawing.Point(18, 12)
        lblStare.Name = "lblStare"
        lblStare.Padding = New System.Windows.Forms.Padding(0, 4, 0, 12)
        lblStare.Size = New System.Drawing.Size(640, 108)
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
        pnlJos.Location = New System.Drawing.Point(0, 358)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(676, 58)
        pnlJos.TabIndex = 3
        pnlJos.Tag = "Card"
        '
        ' btnReimprospateaza
        '
        btnReimprospateaza.Dock = System.Windows.Forms.DockStyle.Left
        btnReimprospateaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnReimprospateaza.Location = New System.Drawing.Point(8, 8)
        btnReimprospateaza.Name = "btnReimprospateaza"
        btnReimprospateaza.Size = New System.Drawing.Size(160, 42)
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
        btnAutorizeaza.Location = New System.Drawing.Point(348, 8)
        btnAutorizeaza.Name = "btnAutorizeaza"
        btnAutorizeaza.Size = New System.Drawing.Size(196, 42)
        btnAutorizeaza.TabIndex = 1
        btnAutorizeaza.Text = "Autorizează"
        tips.SetToolTipHeader(btnAutorizeaza, "Autorizare cu certificatul")
        tips.SetToolTipText(btnAutorizeaza, "Obține (sau reînnoiește) tokenul ANAF al unității. Cere certificatul calificat de pe token sau card; PIN-ul îl cere programul lui. Tokenul rămâne pe server.")
        btnAutorizeaza.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(544, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 2
        '
        ' btnInchide
        '
        btnInchide.Dock = System.Windows.Forms.DockStyle.Right
        btnInchide.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnInchide.Location = New System.Drawing.Point(556, 8)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New System.Drawing.Size(112, 42)
        btnInchide.TabIndex = 3
        btnInchide.Text = "Închide"
        tips.SetToolTipHeader(btnInchide, "Închide")
        tips.SetToolTipText(btnInchide, "Închide fereastra.")
        btnInchide.UseVisualStyleBackColor = True
        '
        ' busy
        '
        busy.Dock = System.Windows.Forms.DockStyle.Top
        busy.Location = New System.Drawing.Point(0, 40)
        busy.Name = "busy"
        busy.Size = New System.Drawing.Size(676, 6)
        busy.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(676, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — tokenul ANAF"
        '
        ' TokenForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New System.Drawing.Size(680, 420)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "TokenForm"
        Padding = New System.Windows.Forms.Padding(2)
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
