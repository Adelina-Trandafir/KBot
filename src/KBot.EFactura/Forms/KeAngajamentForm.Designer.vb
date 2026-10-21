<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KeAngajamentForm
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
        Dim KBotDataColumn5 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        gridAng = New Global.KBot.Controls.KBotDataView()
        pnlJos = New System.Windows.Forms.Panel()
        btnAlege = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnRenunta = New System.Windows.Forms.Button()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        txtCauta = New Global.KBot.Controls.KBotTextField()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        CType(gridAng, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridAng)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(txtCauta)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(796, 546)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' gridAng
        '
        gridAng.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        gridAng.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Angajament"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "cod"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Descriere"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "descriere"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 280
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Stare"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "stare"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 110
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Partener"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "partener"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 200
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Creat"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "data"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 90
        gridAng.Columns.Add(KBotDataColumn1)
        gridAng.Columns.Add(KBotDataColumn2)
        gridAng.Columns.Add(KBotDataColumn3)
        gridAng.Columns.Add(KBotDataColumn4)
        gridAng.Columns.Add(KBotDataColumn5)
        gridAng.Dock = System.Windows.Forms.DockStyle.Fill
        gridAng.FillColumnKey = "descriere"
        gridAng.Location = New System.Drawing.Point(0, 128)
        gridAng.Margin = New System.Windows.Forms.Padding(0)
        gridAng.Name = "gridAng"
        gridAng.ReadOnlyGrid = True
        gridAng.Size = New System.Drawing.Size(796, 300)
        gridAng.TabIndex = 3
        ' tips.SetToolTipHeader(gridAng, "Angajamentele")
        ' tips.SetToolTipText(gridAng, "Alege angajamentul pe care se face recepția facturii. Dublu clic îl alege direct.")
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 428)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(796, 60)
        ntfMesaj.TabIndex = 7
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnAlege)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 488)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(796, 58)
        pnlJos.TabIndex = 6
        pnlJos.Tag = "Card"
        '
        ' btnAlege
        '
        btnAlege.Dock = System.Windows.Forms.DockStyle.Right
        btnAlege.Enabled = False
        btnAlege.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAlege.Location = New System.Drawing.Point(628, 8)
        btnAlege.Name = "btnAlege"
        btnAlege.Size = New System.Drawing.Size(160, 42)
        btnAlege.TabIndex = 0
        btnAlege.Text = "Alege"
        tips.SetToolTipHeader(btnAlege, "Alege")
        tips.SetToolTipText(btnAlege, "Folosește angajamentul ales pentru recepția facturii.")
        btnAlege.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(616, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Left
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(8, 8)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New System.Drawing.Size(117, 42)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să aleagă nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' txtCauta
        '
        txtCauta.BackColor = System.Drawing.Color.Transparent
        txtCauta.Dock = System.Windows.Forms.DockStyle.Top
        txtCauta.Location = New System.Drawing.Point(0, 80)
        txtCauta.MaxLength = 100
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Caută după cod, descriere, partener sau cod fiscal…"
        txtCauta.Size = New System.Drawing.Size(796, 48)
        txtCauta.TabIndex = 2
        tips.SetToolTipText(txtCauta, "Arată doar angajamentele care conțin textul scris.")
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 6)
        lblAntet.Size = New System.Drawing.Size(796, 40)
        lblAntet.TabIndex = 1
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
        capBar.Text = "E-Factura — angajamentul recepției"
        '
        ' KeAngajamentForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New System.Drawing.Size(800, 550)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(700, 450)
        Name = "KeAngajamentForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — angajamentul recepției"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        CType(gridAng, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents txtCauta As Global.KBot.Controls.KBotTextField
    Friend WithEvents gridAng As Global.KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnAlege As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
