<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KeClasificariForm
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
        gridLinii = New Global.KBot.Controls.KBotDataView()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        pnlJos = New System.Windows.Forms.Panel()
        btnRenunta = New System.Windows.Forms.Button()
        btnToate = New System.Windows.Forms.Button()
        lblSep2 = New System.Windows.Forms.Label()
        btnSalveaza = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        pnlAng = New System.Windows.Forms.Panel()
        cmbAngajament = New Global.KBot.Controls.KBotComboBox()
        btnAltul = New System.Windows.Forms.Button()
        lblAng = New System.Windows.Forms.Label()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        pnlAng.SuspendLayout()
        CType(gridLinii, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridLinii)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(pnlAng)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(896, 566)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' gridLinii
        '
        gridLinii.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        gridLinii.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Nr"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nr"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 44
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Denumire"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "denumire"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 330
        KBotDataColumn3.Aggregate = Global.KBot.Controls.KBotAggregate.Sum
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Valoare cu TVA"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cuTva"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn3.Width = 120
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnType = Global.KBot.Controls.KBotColumnType.Combo
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Clasificație"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "clsf"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 330
        gridLinii.Columns.Add(KBotDataColumn1)
        gridLinii.Columns.Add(KBotDataColumn2)
        gridLinii.Columns.Add(KBotDataColumn3)
        gridLinii.Columns.Add(KBotDataColumn4)
        gridLinii.Dock = System.Windows.Forms.DockStyle.Fill
        gridLinii.FillColumnKey = "denumire"
        gridLinii.FooterCaption = "TOTAL"
        gridLinii.FooterVisible = True
        gridLinii.Location = New System.Drawing.Point(0, 128)
        gridLinii.Margin = New System.Windows.Forms.Padding(0)
        gridLinii.Name = "gridLinii"
        gridLinii.Size = New System.Drawing.Size(896, 320)
        gridLinii.TabIndex = 3
        tips.SetToolTipHeader(gridLinii, "Liniile facturii")
        tips.SetToolTipText(gridLinii, "Alege pentru fiecare linie clasificația din angajament. O recepție se face pe clasificații, nu pe linii.")
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 448)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(896, 60)
        ntfMesaj.TabIndex = 7
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(btnToate)
        pnlJos.Controls.Add(lblSep2)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 508)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(896, 58)
        pnlJos.TabIndex = 6
        pnlJos.Tag = "Card"
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Left
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(8, 8)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New System.Drawing.Size(117, 42)
        btnRenunta.TabIndex = 4
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să salveze clasificațiile.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnToate
        '
        btnToate.Dock = System.Windows.Forms.DockStyle.Right
        btnToate.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnToate.Location = New System.Drawing.Point(480, 8)
        btnToate.Name = "btnToate"
        btnToate.Size = New System.Drawing.Size(230, 42)
        btnToate.TabIndex = 3
        btnToate.Text = "Ca la prima linie"
        tips.SetToolTipHeader(btnToate, "Ca la prima linie")
        tips.SetToolTipText(btnToate, "Pune pe toate liniile clasificația aleasă la prima linie.")
        btnToate.UseVisualStyleBackColor = True
        '
        ' lblSep2
        '
        lblSep2.Dock = System.Windows.Forms.DockStyle.Right
        lblSep2.Location = New System.Drawing.Point(710, 8)
        lblSep2.Name = "lblSep2"
        lblSep2.Size = New System.Drawing.Size(12, 42)
        lblSep2.TabIndex = 2
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = System.Windows.Forms.DockStyle.Right
        btnSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSalveaza.Location = New System.Drawing.Point(722, 8)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New System.Drawing.Size(154, 42)
        btnSalveaza.TabIndex = 0
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Salvează clasificațiile liniilor. Numai rolul Contabil poate salva.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(876, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' pnlAng
        '
        pnlAng.Controls.Add(cmbAngajament)
        pnlAng.Controls.Add(btnAltul)
        pnlAng.Controls.Add(lblAng)
        pnlAng.Dock = System.Windows.Forms.DockStyle.Top
        pnlAng.Location = New System.Drawing.Point(0, 80)
        pnlAng.Name = "pnlAng"
        pnlAng.Padding = New System.Windows.Forms.Padding(14, 6, 14, 6)
        pnlAng.Size = New System.Drawing.Size(896, 48)
        pnlAng.TabIndex = 2
        '
        ' cmbAngajament
        '
        cmbAngajament.Dock = System.Windows.Forms.DockStyle.Fill
        cmbAngajament.Location = New System.Drawing.Point(114, 6)
        cmbAngajament.Name = "cmbAngajament"
        cmbAngajament.Size = New System.Drawing.Size(512, 36)
        cmbAngajament.TabIndex = 0
        tips.SetToolTipHeader(cmbAngajament, "Angajamentul")
        tips.SetToolTipText(cmbAngajament, "Angajamentele la care poate merge factura (după DDF-ul furnizorului). Indicatorii lui sunt clasificațiile din care se alege.")
        '
        ' btnAltul
        '
        btnAltul.Dock = System.Windows.Forms.DockStyle.Right
        btnAltul.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAltul.Location = New System.Drawing.Point(626, 6)
        btnAltul.Name = "btnAltul"
        btnAltul.Size = New System.Drawing.Size(256, 36)
        btnAltul.TabIndex = 1
        btnAltul.Text = "Alt angajament…"
        tips.SetToolTipHeader(btnAltul, "Alt angajament")
        tips.SetToolTipText(btnAltul, "Alege un angajament din lista tuturor, când furnizorul nu are DDF sau factura merge la altul.")
        btnAltul.UseVisualStyleBackColor = True
        '
        ' lblAng
        '
        lblAng.Dock = System.Windows.Forms.DockStyle.Left
        lblAng.Location = New System.Drawing.Point(14, 6)
        lblAng.Name = "lblAng"
        lblAng.Size = New System.Drawing.Size(100, 36)
        lblAng.TabIndex = 2
        lblAng.Text = "Angajament:"
        lblAng.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 6)
        lblAntet.Size = New System.Drawing.Size(896, 40)
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
        capBar.Size = New System.Drawing.Size(896, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — clasificațiile liniilor"
        '
        ' KeClasificariForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New System.Drawing.Size(900, 570)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(820, 460)
        Name = "KeClasificariForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — clasificațiile liniilor"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlAng.ResumeLayout(False)
        CType(gridLinii, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents pnlAng As System.Windows.Forms.Panel
    Friend WithEvents lblAng As System.Windows.Forms.Label
    Friend WithEvents cmbAngajament As Global.KBot.Controls.KBotComboBox
    Friend WithEvents btnAltul As System.Windows.Forms.Button
    Friend WithEvents gridLinii As Global.KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents lblSep2 As System.Windows.Forms.Label
    Friend WithEvents btnToate As System.Windows.Forms.Button
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
