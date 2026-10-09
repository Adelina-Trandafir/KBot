<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SincronizarePrimiteForm
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
        pnlJos = New System.Windows.Forms.Panel()
        btnPorneste = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnInchide = New System.Windows.Forms.Button()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        lblProgres = New System.Windows.Forms.Label()
        barProgres = New Global.KBot.Controls.KBotBusyBar()
        cmbZile = New Global.KBot.Controls.KBotComboBox()
        lblZileT = New System.Windows.Forms.Label()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblProgres)
        pnlCard.Controls.Add(barProgres)
        pnlCard.Controls.Add(cmbZile)
        pnlCard.Controls.Add(lblZileT)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(556, 366)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 190)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(556, 118)
        ntfMesaj.TabIndex = 7
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnPorneste)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 308)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(556, 58)
        pnlJos.TabIndex = 6
        pnlJos.Tag = "Card"
        '
        ' btnPorneste
        '
        btnPorneste.Dock = System.Windows.Forms.DockStyle.Right
        btnPorneste.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnPorneste.Location = New System.Drawing.Point(388, 8)
        btnPorneste.Name = "btnPorneste"
        btnPorneste.Size = New System.Drawing.Size(160, 42)
        btnPorneste.TabIndex = 0
        btnPorneste.Text = "Sincronizează"
        tips.SetToolTipHeader(btnPorneste, "Sincronizează")
        tips.SetToolTipText(btnPorneste, "Aduce de la ANAF facturile primite în perioada aleasă și le scrie în baza de date. Cele deja aduse nu se dublează.")
        btnPorneste.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(376, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' btnInchide
        '
        btnInchide.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnInchide.Dock = System.Windows.Forms.DockStyle.Left
        btnInchide.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnInchide.Location = New System.Drawing.Point(8, 8)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New System.Drawing.Size(117, 42)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        tips.SetToolTipHeader(btnInchide, "Închide")
        tips.SetToolTipText(btnInchide, "Închide fereastra. Dacă sincronizarea merge, o oprește; ce s-a adus până atunci rămâne.")
        btnInchide.UseVisualStyleBackColor = True
        '
        ' lblProgres
        '
        lblProgres.Dock = System.Windows.Forms.DockStyle.Top
        lblProgres.Location = New System.Drawing.Point(0, 158)
        lblProgres.Name = "lblProgres"
        lblProgres.Padding = New System.Windows.Forms.Padding(14, 6, 14, 0)
        lblProgres.Size = New System.Drawing.Size(556, 32)
        lblProgres.TabIndex = 5
        '
        ' barProgres
        '
        barProgres.Dock = System.Windows.Forms.DockStyle.Top
        barProgres.Location = New System.Drawing.Point(0, 150)
        barProgres.Margin = New System.Windows.Forms.Padding(0)
        barProgres.Name = "barProgres"
        barProgres.Size = New System.Drawing.Size(556, 8)
        barProgres.TabIndex = 4
        '
        ' cmbZile
        '
        cmbZile.Dock = System.Windows.Forms.DockStyle.Top
        cmbZile.Items.AddRange(New Object() {"Ultimele 7 zile", "Ultimele 15 zile", "Ultimele 30 zile", "Ultimele 45 zile", "Ultimele 60 zile"})
        cmbZile.Location = New System.Drawing.Point(0, 113)
        cmbZile.Margin = New System.Windows.Forms.Padding(0)
        cmbZile.Name = "cmbZile"
        cmbZile.Size = New System.Drawing.Size(556, 37)
        cmbZile.TabIndex = 3
        tips.SetToolTipHeader(cmbZile, "Perioada")
        tips.SetToolTipText(cmbZile, "ANAF dă mesajele a cel mult 60 de zile. Se aduc doar facturile care nu sunt deja în baza de date.")
        '
        ' lblZileT
        '
        lblZileT.Dock = System.Windows.Forms.DockStyle.Top
        lblZileT.Location = New System.Drawing.Point(0, 89)
        lblZileT.Name = "lblZileT"
        lblZileT.Padding = New System.Windows.Forms.Padding(14, 4, 14, 0)
        lblZileT.Size = New System.Drawing.Size(556, 24)
        lblZileT.TabIndex = 2
        lblZileT.Text = "Perioada"
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 10)
        lblAntet.Size = New System.Drawing.Size(556, 49)
        lblAntet.TabIndex = 1
        lblAntet.Text = "Aduce de la ANAF facturile primite de unitate în perioada aleasă."
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(556, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — facturi primite"
        '
        ' SincronizarePrimiteForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New System.Drawing.Size(560, 370)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "SincronizarePrimiteForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — facturi primite"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents lblZileT As System.Windows.Forms.Label
    Friend WithEvents cmbZile As Global.KBot.Controls.KBotComboBox
    Friend WithEvents barProgres As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents lblProgres As System.Windows.Forms.Label
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnPorneste As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnInchide As System.Windows.Forms.Button
End Class
