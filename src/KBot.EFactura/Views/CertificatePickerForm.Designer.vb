<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CertificatePickerForm
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
        Dim KBotDataColumn1 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn2 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn3 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn4 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        grilaCertificate = New Global.KBot.Controls.KBotDataView()
        pnlJos = New System.Windows.Forms.Panel()
        btnAlege = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnRenunta = New System.Windows.Forms.Button()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        CType(grilaCertificate, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(grilaCertificate)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(796, 396)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' grilaCertificate
        '
        grilaCertificate.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        grilaCertificate.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.HeaderText = "Certificat"
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.MinWidth = 120
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 220
        KBotDataColumn2.HeaderText = "Emis de"
        KBotDataColumn2.Key = "emis"
        KBotDataColumn2.MinWidth = 100
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 170
        KBotDataColumn3.HeaderText = "Valabil până la"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "expira"
        KBotDataColumn3.MinWidth = 90
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 110
        KBotDataColumn4.HeaderText = "Dispozitiv"
        KBotDataColumn4.Key = "furnizor"
        KBotDataColumn4.MinWidth = 100
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 200
        grilaCertificate.Columns.Add(KBotDataColumn1)
        grilaCertificate.Columns.Add(KBotDataColumn2)
        grilaCertificate.Columns.Add(KBotDataColumn3)
        grilaCertificate.Columns.Add(KBotDataColumn4)
        grilaCertificate.Dock = System.Windows.Forms.DockStyle.Fill
        grilaCertificate.FillColumnKey = "nume"
        grilaCertificate.HeaderHeight = 24
        grilaCertificate.Location = New System.Drawing.Point(0, 106)
        grilaCertificate.Name = "grilaCertificate"
        grilaCertificate.ReadOnlyGrid = True
        grilaCertificate.Size = New System.Drawing.Size(796, 232)
        grilaCertificate.TabIndex = 3
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnAlege)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 338)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(796, 58)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' btnAlege
        '
        btnAlege.DialogResult = System.Windows.Forms.DialogResult.OK
        btnAlege.Dock = System.Windows.Forms.DockStyle.Right
        btnAlege.Enabled = False
        btnAlege.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAlege.Location = New System.Drawing.Point(519, 8)
        btnAlege.Name = "btnAlege"
        btnAlege.Size = New System.Drawing.Size(140, 42)
        btnAlege.TabIndex = 0
        btnAlege.Text = "Folosește"
        tips.SetToolTipHeader(btnAlege, "Folosește certificatul")
        tips.SetToolTipText(btnAlege, "Autorizează E-Factura cu certificatul ales. Tokenul sau cardul trebuie să fie conectat; PIN-ul îl cere programul lui.")
        btnAlege.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(659, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Right
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(671, 8)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New System.Drawing.Size(117, 42)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să autorizeze nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 10)
        lblAntet.Size = New System.Drawing.Size(796, 66)
        lblAntet.TabIndex = 1
        lblAntet.Text = "Alegeți certificatul calificat al unității (cel de pe token sau card). Cheia lui rămâne pe dispozitiv; K-BOT nu o citește și nu o copiază."
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(796, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — alegerea certificatului"
        '
        ' CertificatePickerForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        AcceptButton = btnAlege
        CancelButton = btnRenunta
        ClientSize = New System.Drawing.Size(800, 400)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "CertificatePickerForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — alegerea certificatului"
        pnlCard.ResumeLayout(False)
        CType(grilaCertificate, System.ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents grilaCertificate As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnAlege As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
