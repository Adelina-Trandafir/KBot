<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KeLegaForm
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
        Dim KBotDataColumn6 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn7 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn8 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn9 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn10 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn11 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        gridAloc = New Global.KBot.Controls.KBotDataView()
        ntfMesaj = New Global.KBot.Controls.KBotNotice()
        pnlJos = New System.Windows.Forms.Panel()
        btnRenunta = New System.Windows.Forms.Button()
        btnOk = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        pnlRec = New System.Windows.Forms.Panel()
        gridReceptii = New Global.KBot.Controls.KBotDataView()
        pnlNoua = New System.Windows.Forms.Panel()
        tlyNoua = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblTip = New System.Windows.Forms.Label()
        cmbTip = New Global.KBot.Controls.KBotComboBox()
        lblData = New System.Windows.Forms.Label()
        dtpData = New Global.KBot.Controls.KBotDatePicker()
        lblDescriere = New System.Windows.Forms.Label()
        txtDescriere = New Global.KBot.Controls.KBotTextField()
        pnlAng = New System.Windows.Forms.Panel()
        cmbAngajament = New Global.KBot.Controls.KBotComboBox()
        btnAltul = New System.Windows.Forms.Button()
        lblAng = New System.Windows.Forms.Label()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        pnlRec.SuspendLayout()
        pnlNoua.SuspendLayout()
        tlyNoua.SuspendLayout()
        pnlAng.SuspendLayout()
        CType(gridAloc, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(gridReceptii, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridAloc)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(pnlRec)
        pnlCard.Controls.Add(pnlAng)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(946, 676)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' gridAloc: one row per classification of the invoice with a rest; «De alocat» is typed
        '
        gridAloc.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        gridAloc.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Clasificație"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "clsf"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 260
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Indicator"
        KBotDataColumn7.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "indicator"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 80
        KBotDataColumn8.Aggregate = Global.KBot.Controls.KBotAggregate.Sum
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Rest factură"
        KBotDataColumn8.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "restFactura"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn8.Width = 120
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Valoare recepție"
        KBotDataColumn9.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "valRecep"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn9.Width = 130
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Rest recepție"
        KBotDataColumn10.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "restRecep"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn10.Width = 120
        KBotDataColumn11.Aggregate = Global.KBot.Controls.KBotAggregate.Sum
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "De alocat"
        KBotDataColumn11.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "suma"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn11.Width = 130
        gridAloc.Columns.Add(KBotDataColumn6)
        gridAloc.Columns.Add(KBotDataColumn7)
        gridAloc.Columns.Add(KBotDataColumn8)
        gridAloc.Columns.Add(KBotDataColumn9)
        gridAloc.Columns.Add(KBotDataColumn10)
        gridAloc.Columns.Add(KBotDataColumn11)
        gridAloc.Dock = System.Windows.Forms.DockStyle.Fill
        gridAloc.FillColumnKey = "clsf"
        gridAloc.FooterCaption = "TOTAL"
        gridAloc.FooterVisible = True
        gridAloc.Location = New System.Drawing.Point(0, 344)
        gridAloc.Margin = New System.Windows.Forms.Padding(0)
        gridAloc.Name = "gridAloc"
        gridAloc.Size = New System.Drawing.Size(946, 214)
        gridAloc.TabIndex = 4
        tips.SetToolTipHeader(gridAloc, "Sumele alocate")
        tips.SetToolTipText(gridAloc, "Pentru fiecare clasificație a facturii: cât din rest acoperă recepția. Sumele pot fi scăzute pentru o plată parțială. Coloana «De alocat» se scrie.")
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = System.Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 558)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(946, 60)
        ntfMesaj.TabIndex = 7
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(btnOk)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 618)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(946, 58)
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
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să facă nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnOk
        '
        btnOk.Dock = System.Windows.Forms.DockStyle.Right
        btnOk.Enabled = False
        btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnOk.Location = New System.Drawing.Point(706, 8)
        btnOk.Name = "btnOk"
        btnOk.Size = New System.Drawing.Size(220, 42)
        btnOk.TabIndex = 0
        btnOk.Text = "Leagă"
        tips.SetToolTipHeader(btnOk, "Leagă")
        btnOk.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(926, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' pnlRec: the receptions of the angajament, or the form of a new one
        '
        pnlRec.Controls.Add(gridReceptii)
        pnlRec.Controls.Add(pnlNoua)
        pnlRec.Dock = System.Windows.Forms.DockStyle.Top
        pnlRec.Location = New System.Drawing.Point(0, 174)
        pnlRec.Name = "pnlRec"
        pnlRec.Size = New System.Drawing.Size(946, 170)
        pnlRec.TabIndex = 3
        '
        ' gridReceptii
        '
        gridReceptii.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        gridReceptii.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Data"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "data"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 90
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Tip"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "tip"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 80
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Descriere"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "descriere"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 420
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Valoare"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "total"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn4.Width = 120
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Rest nealocat"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "rest"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn5.Width = 120
        gridReceptii.Columns.Add(KBotDataColumn1)
        gridReceptii.Columns.Add(KBotDataColumn2)
        gridReceptii.Columns.Add(KBotDataColumn3)
        gridReceptii.Columns.Add(KBotDataColumn4)
        gridReceptii.Columns.Add(KBotDataColumn5)
        gridReceptii.Dock = System.Windows.Forms.DockStyle.Fill
        gridReceptii.FillColumnKey = "descriere"
        gridReceptii.Location = New System.Drawing.Point(0, 0)
        gridReceptii.Margin = New System.Windows.Forms.Padding(0)
        gridReceptii.Name = "gridReceptii"
        gridReceptii.ReadOnlyGrid = True
        gridReceptii.Size = New System.Drawing.Size(946, 170)
        gridReceptii.TabIndex = 0
        tips.SetToolTipHeader(gridReceptii, "Recepțiile angajamentului")
        tips.SetToolTipText(gridReceptii, "Alege recepția. «Rest nealocat» este cât din valoarea ei nu e încă acoperit de nicio factură.")
        '
        ' pnlNoua: the fields of a new reception
        '
        pnlNoua.Controls.Add(tlyNoua)
        pnlNoua.Dock = System.Windows.Forms.DockStyle.Fill
        pnlNoua.Location = New System.Drawing.Point(0, 0)
        pnlNoua.Name = "pnlNoua"
        pnlNoua.Padding = New System.Windows.Forms.Padding(14, 6, 14, 6)
        pnlNoua.Size = New System.Drawing.Size(946, 170)
        pnlNoua.TabIndex = 1
        pnlNoua.Visible = False
        '
        ' tlyNoua
        '
        tlyNoua.ColumnCount = 2
        tlyNoua.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110.0!))
        tlyNoua.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyNoua.Controls.Add(lblTip, 0, 0)
        tlyNoua.Controls.Add(cmbTip, 1, 0)
        tlyNoua.Controls.Add(lblData, 0, 1)
        tlyNoua.Controls.Add(dtpData, 1, 1)
        tlyNoua.Controls.Add(lblDescriere, 0, 2)
        tlyNoua.Controls.Add(txtDescriere, 1, 2)
        tlyNoua.Dock = System.Windows.Forms.DockStyle.Fill
        tlyNoua.Location = New System.Drawing.Point(14, 6)
        tlyNoua.Margin = New System.Windows.Forms.Padding(0)
        tlyNoua.Name = "tlyNoua"
        tlyNoua.RowCount = 3
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyNoua.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        tlyNoua.Size = New System.Drawing.Size(918, 158)
        tlyNoua.TabIndex = 0
        '
        ' lblTip
        '
        lblTip.Dock = System.Windows.Forms.DockStyle.Fill
        lblTip.Location = New System.Drawing.Point(0, 0)
        lblTip.Margin = New System.Windows.Forms.Padding(0)
        lblTip.Name = "lblTip"
        lblTip.Size = New System.Drawing.Size(110, 40)
        lblTip.TabIndex = 0
        lblTip.Text = "Tip:"
        lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbTip
        '
        cmbTip.Dock = System.Windows.Forms.DockStyle.Top
        cmbTip.Location = New System.Drawing.Point(110, 0)
        cmbTip.Margin = New System.Windows.Forms.Padding(0, 4, 0, 0)
        cmbTip.Name = "cmbTip"
        cmbTip.Size = New System.Drawing.Size(240, 32)
        cmbTip.TabIndex = 1
        tips.SetToolTipHeader(cmbTip, "Tipul recepției")
        tips.SetToolTipText(cmbTip, "Avans, Partial sau Final, ca în FOREXE. Implicit «Final», ca în FOREXE.")
        '
        ' lblData
        '
        lblData.Dock = System.Windows.Forms.DockStyle.Fill
        lblData.Location = New System.Drawing.Point(0, 40)
        lblData.Margin = New System.Windows.Forms.Padding(0)
        lblData.Name = "lblData"
        lblData.Size = New System.Drawing.Size(110, 40)
        lblData.TabIndex = 2
        lblData.Text = "Data:"
        lblData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' dtpData
        '
        dtpData.Dock = System.Windows.Forms.DockStyle.Top
        dtpData.Location = New System.Drawing.Point(110, 40)
        dtpData.Margin = New System.Windows.Forms.Padding(0, 4, 0, 0)
        dtpData.Name = "dtpData"
        dtpData.Size = New System.Drawing.Size(240, 32)
        dtpData.TabIndex = 3
        tips.SetToolTipHeader(dtpData, "Data recepției")
        tips.SetToolTipText(dtpData, "Data scrisă în recepția din FOREXE. Implicit, data facturii.")
        '
        ' lblDescriere
        '
        lblDescriere.Dock = System.Windows.Forms.DockStyle.Fill
        lblDescriere.Location = New System.Drawing.Point(0, 80)
        lblDescriere.Margin = New System.Windows.Forms.Padding(0)
        lblDescriere.Name = "lblDescriere"
        lblDescriere.Size = New System.Drawing.Size(110, 48)
        lblDescriere.TabIndex = 4
        lblDescriere.Text = "Descriere:"
        lblDescriere.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' txtDescriere
        '
        txtDescriere.BackColor = System.Drawing.Color.Transparent
        txtDescriere.Dock = System.Windows.Forms.DockStyle.Top
        txtDescriere.Location = New System.Drawing.Point(110, 80)
        txtDescriere.Margin = New System.Windows.Forms.Padding(0, 4, 0, 0)
        txtDescriere.MaxLength = 200
        txtDescriere.Name = "txtDescriere"
        txtDescriere.Size = New System.Drawing.Size(808, 40)
        txtDescriere.TabIndex = 5
        tips.SetToolTipHeader(txtDescriere, "Descrierea recepției")
        tips.SetToolTipText(txtDescriere, "Textul scris în recepția din FOREXE, de obicei «plata factura număr/data». K-BOT adaugă la sfârșit marcajul lui.")
        '
        ' pnlAng
        '
        pnlAng.Controls.Add(cmbAngajament)
        pnlAng.Controls.Add(btnAltul)
        pnlAng.Controls.Add(lblAng)
        pnlAng.Dock = System.Windows.Forms.DockStyle.Top
        pnlAng.Location = New System.Drawing.Point(0, 126)
        pnlAng.Name = "pnlAng"
        pnlAng.Padding = New System.Windows.Forms.Padding(14, 6, 14, 6)
        pnlAng.Size = New System.Drawing.Size(946, 48)
        pnlAng.TabIndex = 2
        '
        ' cmbAngajament
        '
        cmbAngajament.Dock = System.Windows.Forms.DockStyle.Fill
        cmbAngajament.Location = New System.Drawing.Point(114, 6)
        cmbAngajament.Name = "cmbAngajament"
        cmbAngajament.Size = New System.Drawing.Size(562, 36)
        cmbAngajament.TabIndex = 0
        tips.SetToolTipHeader(cmbAngajament, "Angajamentul")
        tips.SetToolTipText(cmbAngajament, "Angajamentele la care poate merge factura (după DDF-ul furnizorului). Pentru alt angajament, «Alt angajament…».")
        '
        ' btnAltul
        '
        btnAltul.Dock = System.Windows.Forms.DockStyle.Right
        btnAltul.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAltul.Location = New System.Drawing.Point(676, 6)
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
        lblAntet.Size = New System.Drawing.Size(946, 86)
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
        capBar.Size = New System.Drawing.Size(946, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — recepție"
        '
        ' KeLegaForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New System.Drawing.Size(950, 680)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(860, 560)
        Name = "KeLegaForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — recepție"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlRec.ResumeLayout(False)
        pnlNoua.ResumeLayout(False)
        tlyNoua.ResumeLayout(False)
        pnlAng.ResumeLayout(False)
        CType(gridAloc, System.ComponentModel.ISupportInitialize).EndInit()
        CType(gridReceptii, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents pnlRec As System.Windows.Forms.Panel
    Friend WithEvents gridReceptii As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlNoua As System.Windows.Forms.Panel
    Friend WithEvents tlyNoua As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblTip As System.Windows.Forms.Label
    Friend WithEvents cmbTip As Global.KBot.Controls.KBotComboBox
    Friend WithEvents lblData As System.Windows.Forms.Label
    Friend WithEvents dtpData As Global.KBot.Controls.KBotDatePicker
    Friend WithEvents lblDescriere As System.Windows.Forms.Label
    Friend WithEvents txtDescriere As Global.KBot.Controls.KBotTextField
    Friend WithEvents gridAloc As Global.KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
