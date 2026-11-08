Imports KBot.Controls

' «Operatii diverse» page (slice AV02-03; the Access form OP9): the templates of type A on the left (searchable), the header and the lines on the right.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OperatiiDiversePage
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyPagina = New KBotTableLayoutPanel()
        tlyStanga = New KBotTableLayoutPanel()
        txtCauta = New KBotTextBox()
        lstSabloane = New KBotDataView()
        tlyDreapta = New KBotTableLayoutPanel()
        tlyAntet = New KBotTableLayoutPanel()
        lblNota = New KBotLabel()
        lblNrDoc = New KBotLabel()
        lblFelDoc = New KBotLabel()
        lblData = New KBotLabel()
        lblPartener = New KBotLabel()
        lblSuma = New KBotLabel()
        cmbNota = New KBotComboBox()
        txtNrDoc = New KBotTextBox()
        cmbFelDoc = New KBotComboBox()
        dtpData = New KBotDatePicker()
        cmbPartener = New KBotComboBox()
        txtSuma = New KBotTextBox()
        gridLinii = New KBotDataView()
        tlyJos = New KBotTableLayoutPanel()
        lblStare = New KBotLabel()
        btnRenunta = New KBotButton()
        btnSalveaza = New KBotButton()
        tlyPagina.SuspendLayout()
        tlyStanga.SuspendLayout()
        tlyDreapta.SuspendLayout()
        tlyAntet.SuspendLayout()
        tlyJos.SuspendLayout()
        CType(lstSabloane, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridLinii, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' tlyPagina
        ' 
        tlyPagina.ColumnCount = 2
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 480F))
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Margin = New Padding(0)
        tlyPagina.RowCount = 1
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPagina.Size = New Size(100, 100)
        tlyPagina.Controls.Add(tlyStanga, 0, 0)
        tlyPagina.Controls.Add(tlyDreapta, 1, 0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.TabIndex = 0
        ' 
        ' tlyStanga
        ' 
        tlyStanga.ColumnCount = 1
        tlyStanga.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStanga.Dock = DockStyle.Fill
        tlyStanga.Margin = New Padding(0)
        tlyStanga.RowCount = 2
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyStanga.Size = New Size(100, 100)
        tlyStanga.Controls.Add(txtCauta, 0, 0)
        tlyStanga.Controls.Add(lstSabloane, 0, 1)
        tlyStanga.Name = "tlyStanga"
        tlyStanga.TabIndex = 1
        ' 
        ' txtCauta
        ' 
        txtCauta.Dock = DockStyle.Fill
        txtCauta.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtCauta.Margin = New Padding(0, 0, 6, 6)
        txtCauta.Multiline = False
        txtCauta.ScrollBars = ScrollBars.None
        txtCauta.Size = New Size(100, 40)
        txtCauta.TextAlign = HorizontalAlignment.Left
        txtCauta.PlaceholderText = "Caută o operație..."
        txtCauta.Name = "txtCauta"
        txtCauta.TabIndex = 2
        tips.SetToolTipHeader(txtCauta, "Căutare")
        tips.SetToolTipText(txtCauta, "Scrieți o parte din explicația operației. Lista se restrânge pe măsură ce scrieți.")
        ' 
        ' lstSabloane
        ' 
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Operații diverse"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Width = 300
        lstSabloane.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstSabloane.BackColor = SystemColors.Window
        lstSabloane.ColumnFillMode = KBotFillMode.SpecificColumn
        lstSabloane.Columns.Add(KBotDataColumn1)
        lstSabloane.Dock = DockStyle.Fill
        lstSabloane.FillColumnKey = "nume"
        lstSabloane.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstSabloane.Margin = New Padding(0, 0, 6, 0)
        lstSabloane.ReadOnlyGrid = True
        lstSabloane.Size = New Size(100, 100)
        lstSabloane.Name = "lstSabloane"
        lstSabloane.TabIndex = 3
        ' 
        ' tlyDreapta
        ' 
        tlyDreapta.ColumnCount = 1
        tlyDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDreapta.Dock = DockStyle.Fill
        tlyDreapta.Margin = New Padding(0)
        tlyDreapta.RowCount = 3
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Absolute, 104F))
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDreapta.Size = New Size(100, 100)
        tlyDreapta.Controls.Add(tlyAntet, 0, 0)
        tlyDreapta.Controls.Add(gridLinii, 0, 1)
        tlyDreapta.Controls.Add(tlyJos, 0, 2)
        tlyDreapta.Name = "tlyDreapta"
        tlyDreapta.TabIndex = 4
        ' 
        ' tlyAntet
        ' 
        tlyAntet.ColumnCount = 6
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Margin = New Padding(0)
        tlyAntet.RowCount = 2
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyAntet.Size = New Size(100, 100)
        tlyAntet.Controls.Add(lblNota, 0, 0)
        tlyAntet.Controls.Add(lblNrDoc, 1, 0)
        tlyAntet.Controls.Add(lblFelDoc, 2, 0)
        tlyAntet.Controls.Add(lblData, 3, 0)
        tlyAntet.Controls.Add(lblPartener, 4, 0)
        tlyAntet.Controls.Add(lblSuma, 5, 0)
        tlyAntet.Controls.Add(cmbNota, 0, 1)
        tlyAntet.Controls.Add(txtNrDoc, 1, 1)
        tlyAntet.Controls.Add(cmbFelDoc, 2, 1)
        tlyAntet.Controls.Add(dtpData, 3, 1)
        tlyAntet.Controls.Add(cmbPartener, 4, 1)
        tlyAntet.Controls.Add(txtSuma, 5, 1)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.TabIndex = 5
        ' 
        ' lblNota
        ' 
        lblNota.BorderWidth = 0
        lblNota.Dock = DockStyle.Fill
        lblNota.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNota.Margin = New Padding(0, 0, 3, 3)
        lblNota.Text = "Nota contabilă"
        lblNota.TextAlign = ContentAlignment.MiddleCenter
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 6
        ' 
        ' lblNrDoc
        ' 
        lblNrDoc.BorderWidth = 0
        lblNrDoc.Dock = DockStyle.Fill
        lblNrDoc.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNrDoc.Margin = New Padding(0, 0, 3, 3)
        lblNrDoc.Text = "Număr document"
        lblNrDoc.TextAlign = ContentAlignment.MiddleCenter
        lblNrDoc.Name = "lblNrDoc"
        lblNrDoc.TabIndex = 7
        ' 
        ' lblFelDoc
        ' 
        lblFelDoc.BorderWidth = 0
        lblFelDoc.Dock = DockStyle.Fill
        lblFelDoc.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblFelDoc.Margin = New Padding(0, 0, 3, 3)
        lblFelDoc.Text = "Fel document"
        lblFelDoc.TextAlign = ContentAlignment.MiddleCenter
        lblFelDoc.Name = "lblFelDoc"
        lblFelDoc.TabIndex = 8
        ' 
        ' lblData
        ' 
        lblData.BorderWidth = 0
        lblData.Dock = DockStyle.Fill
        lblData.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblData.Margin = New Padding(0, 0, 3, 3)
        lblData.Text = "Data document"
        lblData.TextAlign = ContentAlignment.MiddleCenter
        lblData.Name = "lblData"
        lblData.TabIndex = 9
        ' 
        ' lblPartener
        ' 
        lblPartener.BorderWidth = 0
        lblPartener.Dock = DockStyle.Fill
        lblPartener.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblPartener.Margin = New Padding(0, 0, 3, 3)
        lblPartener.Text = "Partener"
        lblPartener.TextAlign = ContentAlignment.MiddleCenter
        lblPartener.Name = "lblPartener"
        lblPartener.TabIndex = 10
        ' 
        ' lblSuma
        ' 
        lblSuma.BorderWidth = 0
        lblSuma.Dock = DockStyle.Fill
        lblSuma.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblSuma.Margin = New Padding(0, 0, 3, 3)
        lblSuma.Text = "Suma"
        lblSuma.TextAlign = ContentAlignment.MiddleCenter
        lblSuma.Name = "lblSuma"
        lblSuma.TabIndex = 11
        ' 
        ' cmbNota
        ' 
        cmbNota.Dock = DockStyle.Fill
        cmbNota.Editable = True
        cmbNota.FindAsYouType = True
        cmbNota.FindInItemOrder = True
        cmbNota.FitDropDownToWorkingArea = True
        cmbNota.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbNota.Margin = New Padding(0, 4, 6, 4)
        cmbNota.MaxDropDownItems = 15
        cmbNota.PlaceholderText = "Nota"
        cmbNota.Size = New Size(100, 40)
        cmbNota.Enabled = False
        cmbNota.Name = "cmbNota"
        cmbNota.TabIndex = 12
        tips.SetToolTipHeader(cmbNota, "Nota contabilă")
        tips.SetToolTipText(cmbNota, "Alegeți o notă existentă sau scrieți un număr nou. Fără alegere se folosește numărul următor.")
        ' 
        ' txtNrDoc
        ' 
        txtNrDoc.Dock = DockStyle.Fill
        txtNrDoc.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtNrDoc.Margin = New Padding(0, 4, 6, 4)
        txtNrDoc.MaxLength = 50
        txtNrDoc.Multiline = False
        txtNrDoc.ScrollBars = ScrollBars.None
        txtNrDoc.Size = New Size(100, 40)
        txtNrDoc.TextAlign = HorizontalAlignment.Center
        txtNrDoc.Enabled = False
        txtNrDoc.Name = "txtNrDoc"
        txtNrDoc.TabIndex = 13
        tips.SetToolTipHeader(txtNrDoc, "Număr document")
        tips.SetToolTipText(txtNrDoc, "Numărul scris pe document. Se pune pe fiecare document al operațiilor înregistrate.")
        ' 
        ' cmbFelDoc
        ' 
        cmbFelDoc.Dock = DockStyle.Fill
        cmbFelDoc.Editable = True
        cmbFelDoc.FindAsYouType = True
        cmbFelDoc.FindInItemOrder = True
        cmbFelDoc.FitDropDownToWorkingArea = True
        cmbFelDoc.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbFelDoc.Margin = New Padding(0, 4, 6, 4)
        cmbFelDoc.MaxDropDownItems = 15
        cmbFelDoc.PlaceholderText = "Felul documentului"
        cmbFelDoc.Size = New Size(100, 40)
        cmbFelDoc.Enabled = False
        cmbFelDoc.Name = "cmbFelDoc"
        cmbFelDoc.TabIndex = 14
        tips.SetToolTipHeader(cmbFelDoc, "Fel document")
        tips.SetToolTipText(cmbFelDoc, "Felul documentului (notă contabilă, stat de plată, extras ...). Puteți scrie și altul.")
        ' 
        ' dtpData
        ' 
        dtpData.Dock = DockStyle.Fill
        dtpData.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        dtpData.Format = "dd.MM.yyyy"
        dtpData.Margin = New Padding(0, 4, 6, 4)
        dtpData.Size = New Size(100, 40)
        dtpData.Name = "dtpData"
        dtpData.TabIndex = 15
        tips.SetToolTipHeader(dtpData, "Data documentului")
        tips.SetToolTipText(dtpData, "Trebuie să fie în anul de lucru și după ultima lună închisă.")
        ' 
        ' cmbPartener
        ' 
        cmbPartener.Dock = DockStyle.Fill
        cmbPartener.Editable = False
        cmbPartener.FindAsYouType = True
        cmbPartener.FindInItemOrder = True
        cmbPartener.FitDropDownToWorkingArea = True
        cmbPartener.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbPartener.Margin = New Padding(0, 4, 6, 4)
        cmbPartener.MaxDropDownItems = 15
        cmbPartener.PlaceholderText = "Partener"
        cmbPartener.Size = New Size(100, 40)
        cmbPartener.Enabled = False
        cmbPartener.Name = "cmbPartener"
        cmbPartener.TabIndex = 16
        tips.SetToolTipHeader(cmbPartener, "Partener")
        tips.SetToolTipText(cmbPartener, "Se cere doar când operația are linii cu «partenerul operației».")
        ' 
        ' txtSuma
        ' 
        txtSuma.Dock = DockStyle.Fill
        txtSuma.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtSuma.Margin = New Padding(0, 4, 0, 4)
        txtSuma.Multiline = False
        txtSuma.ScrollBars = ScrollBars.None
        txtSuma.Size = New Size(100, 40)
        txtSuma.TextAlign = HorizontalAlignment.Right
        txtSuma.Enabled = False
        txtSuma.Name = "txtSuma"
        txtSuma.TabIndex = 17
        tips.SetToolTipHeader(txtSuma, "Suma")
        tips.SetToolTipText(txtSuma, "Se copiază pe fiecare linie a operației; o puteți schimba apoi pe rând. Liniile cu suma zero nu se înregistrează.")
        ' 
        ' gridLinii
        ' 
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Jurnal"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "jurnal"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 90
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Clasificație"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "clsf"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 260
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Explicație"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "explicatie"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Width = 300
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Cont debit"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "contd"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Partener debit"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "partd"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Width = 240
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Cont credit"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "contc"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Width = 150
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Partener credit"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "partc"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Width = 240
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.Aggregate = KBotAggregate.Sum
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Suma"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "suma"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 150
        gridLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLinii.BackColor = SystemColors.Window
        gridLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        gridLinii.Columns.Add(KBotDataColumn2)
        gridLinii.Columns.Add(KBotDataColumn3)
        gridLinii.Columns.Add(KBotDataColumn4)
        gridLinii.Columns.Add(KBotDataColumn5)
        gridLinii.Columns.Add(KBotDataColumn6)
        gridLinii.Columns.Add(KBotDataColumn7)
        gridLinii.Columns.Add(KBotDataColumn8)
        gridLinii.Columns.Add(KBotDataColumn9)
        gridLinii.Dock = DockStyle.Fill
        gridLinii.FillColumnKey = "explicatie"
        gridLinii.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridLinii.FooterVisible = True
        gridLinii.Margin = New Padding(0, 6, 0, 6)
        gridLinii.Size = New Size(100, 100)
        gridLinii.Name = "gridLinii"
        gridLinii.TabIndex = 18
        ' 
        ' tlyJos
        ' 
        tlyJos.ColumnCount = 3
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyJos.Dock = DockStyle.Fill
        tlyJos.Margin = New Padding(0)
        tlyJos.RowCount = 1
        tlyJos.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyJos.Size = New Size(100, 100)
        tlyJos.Controls.Add(lblStare, 0, 0)
        tlyJos.Controls.Add(btnRenunta, 1, 0)
        tlyJos.Controls.Add(btnSalveaza, 2, 0)
        tlyJos.Name = "tlyJos"
        tlyJos.TabIndex = 19
        ' 
        ' lblStare
        ' 
        lblStare.BorderWidth = 0
        lblStare.Dock = DockStyle.Fill
        lblStare.Font = New Font("Calibri", 9F)
        lblStare.Margin = New Padding(0, 0, 8, 0)
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 20
        ' 
        ' btnRenunta
        ' 
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Enabled = False
        btnRenunta.Margin = New Padding(6, 4, 0, 4)
        btnRenunta.Size = New Size(100, 40)
        btnRenunta.Text = "Renunță"
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 21
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Renunță la operația în lucru și golește formularul.")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Primary = True
        btnSalveaza.Margin = New Padding(6, 4, 0, 4)
        btnSalveaza.Size = New Size(100, 40)
        btnSalveaza.Text = "Salvează"
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 22
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Înregistrează operațiile cu sumă: câte un document și o operație pentru fiecare linie.")
        ' 
        ' OperatiiDiversePage
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Margin = New Padding(4, 4, 4, 4)
        Name = "OperatiiDiversePage"
        Size = New Size(1685, 900)
        tlyPagina.ResumeLayout(False)
        tlyStanga.ResumeLayout(False)
        tlyDreapta.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        tlyJos.ResumeLayout(False)
        CType(lstSabloane, ComponentModel.ISupportInitialize).EndInit()
        CType(gridLinii, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlyStanga As KBotTableLayoutPanel
    Friend WithEvents txtCauta As KBotTextBox
    Friend WithEvents lstSabloane As KBotDataView
    Friend WithEvents tlyDreapta As KBotTableLayoutPanel
    Friend WithEvents tlyAntet As KBotTableLayoutPanel
    Friend WithEvents lblNota As KBotLabel
    Friend WithEvents lblNrDoc As KBotLabel
    Friend WithEvents lblFelDoc As KBotLabel
    Friend WithEvents lblData As KBotLabel
    Friend WithEvents lblPartener As KBotLabel
    Friend WithEvents lblSuma As KBotLabel
    Friend WithEvents cmbNota As KBotComboBox
    Friend WithEvents txtNrDoc As KBotTextBox
    Friend WithEvents cmbFelDoc As KBotComboBox
    Friend WithEvents dtpData As KBotDatePicker
    Friend WithEvents cmbPartener As KBotComboBox
    Friend WithEvents txtSuma As KBotTextBox
    Friend WithEvents gridLinii As KBotDataView
    Friend WithEvents tlyJos As KBotTableLayoutPanel
    Friend WithEvents lblStare As KBotLabel
    Friend WithEvents btnRenunta As KBotButton
    Friend WithEvents btnSalveaza As KBotButton
End Class
