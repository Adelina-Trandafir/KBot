Imports KBot.Controls

' «Operatii diverse» page (slice AV02-03; the Access form OP9): the templates of type A on the left (a tree by journal, collapsed, with its own search), the header and the lines on the right.
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
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        cmbNota = New KBotComboBox()
        txtNrDoc = New KBotTextBox()
        cmbFelDoc = New KBotComboBox()
        dtpData = New KBotDatePicker()
        cmbPartener = New KBotComboBox()
        txtSuma = New KBotTextBox()
        btnRenunta = New KBotButton()
        btnSalveaza = New KBotButton()
        tlyPagina = New KBotTableLayoutPanel()
        tlyStanga = New KBotTableLayoutPanel()
        treeSabloane = New AdvancedTreeControl()
        tlyDreapta = New KBotTableLayoutPanel()
        tlyAntet = New KBotTableLayoutPanel()
        lblNota = New KBotLabel()
        lblNrDoc = New KBotLabel()
        lblFelDoc = New KBotLabel()
        lblData = New KBotLabel()
        lblPartener = New KBotLabel()
        lblSuma = New KBotLabel()
        gridLinii = New KBotDataView()
        tlyJos = New KBotTableLayoutPanel()
        lblStare = New KBotLabel()
        tlyPagina.SuspendLayout()
        tlyStanga.SuspendLayout()
        tlyDreapta.SuspendLayout()
        tlyAntet.SuspendLayout()
        CType(gridLinii, ComponentModel.ISupportInitialize).BeginInit()
        tlyJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' cmbNota
        ' 
        cmbNota.Dock = DockStyle.Fill
        cmbNota.Editable = True
        cmbNota.Enabled = False
        cmbNota.FindAsYouType = True
        cmbNota.FindInItemOrder = True
        cmbNota.FitDropDownToWorkingArea = True
        cmbNota.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbNota.Location = New Point(4, 36)
        cmbNota.Margin = New Padding(4, 0, 4, 0)
        cmbNota.MaxDropDownItems = 15
        cmbNota.Name = "cmbNota"
        cmbNota.PlaceholderText = "Nota"
        cmbNota.Size = New Size(162, 40)
        cmbNota.TabIndex = 12
        tips.SetToolTipHeader(cmbNota, "Nota contabilă")
        tips.SetToolTipText(cmbNota, "Alegeți o notă existentă sau scrieți un număr nou. Fără alegere se folosește numărul următor.")
        ' 
        ' txtNrDoc
        ' 
        txtNrDoc.Dock = DockStyle.Fill
        txtNrDoc.Enabled = False
        txtNrDoc.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtNrDoc.HorizontalScrollBarPadding = New Padding(0)
        txtNrDoc.Location = New Point(174, 36)
        txtNrDoc.Margin = New Padding(4, 0, 4, 0)
        txtNrDoc.MaxLength = 50
        txtNrDoc.Multiline = False
        txtNrDoc.Name = "txtNrDoc"
        txtNrDoc.ScrollBars = ScrollBars.None
        txtNrDoc.Size = New Size(222, 40)
        txtNrDoc.TabIndex = 13
        txtNrDoc.TextAlign = HorizontalAlignment.Center
        txtNrDoc.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtNrDoc, "Număr document")
        tips.SetToolTipText(txtNrDoc, "Numărul scris pe document. Se pune pe fiecare document al operațiilor înregistrate.")
        txtNrDoc.VerticalScrollBarPadding = New Padding(0)
        ' 
        ' cmbFelDoc
        ' 
        cmbFelDoc.Dock = DockStyle.Fill
        cmbFelDoc.Editable = True
        cmbFelDoc.Enabled = False
        cmbFelDoc.FindAsYouType = True
        cmbFelDoc.FindInItemOrder = True
        cmbFelDoc.FitDropDownToWorkingArea = True
        cmbFelDoc.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbFelDoc.Location = New Point(404, 36)
        cmbFelDoc.Margin = New Padding(4, 0, 4, 0)
        cmbFelDoc.MaxDropDownItems = 15
        cmbFelDoc.Name = "cmbFelDoc"
        cmbFelDoc.PlaceholderText = "Felul documentului"
        cmbFelDoc.Size = New Size(242, 40)
        cmbFelDoc.TabIndex = 14
        tips.SetToolTipHeader(cmbFelDoc, "Fel document")
        tips.SetToolTipText(cmbFelDoc, "Felul documentului (notă contabilă, stat de plată, extras ...). Puteți scrie și altul.")
        ' 
        ' dtpData
        ' 
        dtpData.Dock = DockStyle.Fill
        dtpData.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        dtpData.Location = New Point(654, 36)
        dtpData.Margin = New Padding(4, 0, 4, 0)
        dtpData.Name = "dtpData"
        dtpData.Size = New Size(202, 40)
        dtpData.TabIndex = 15
        tips.SetToolTipHeader(dtpData, "Data documentului")
        tips.SetToolTipText(dtpData, "Trebuie să fie în anul de lucru și după ultima lună închisă.")
        ' 
        ' cmbPartener
        ' 
        tlyAntet.SetColumnSpan(cmbPartener, 2)
        cmbPartener.Dock = DockStyle.Fill
        cmbPartener.Enabled = False
        cmbPartener.FindAsYouType = True
        cmbPartener.FindInItemOrder = True
        cmbPartener.FitDropDownToWorkingArea = True
        cmbPartener.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbPartener.Location = New Point(4, 122)
        cmbPartener.Margin = New Padding(4, 0, 4, 0)
        cmbPartener.MaxDropDownItems = 15
        cmbPartener.Name = "cmbPartener"
        cmbPartener.PlaceholderText = "Partener"
        cmbPartener.Size = New Size(392, 40)
        cmbPartener.TabIndex = 16
        tips.SetToolTipHeader(cmbPartener, "Partener")
        tips.SetToolTipText(cmbPartener, "Se cere doar când operația are linii cu «partenerul operației».")
        ' 
        ' txtSuma
        ' 
        txtSuma.Dock = DockStyle.Fill
        txtSuma.Enabled = False
        txtSuma.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtSuma.HorizontalScrollBarPadding = New Padding(0)
        txtSuma.Location = New Point(864, 36)
        txtSuma.Margin = New Padding(4, 0, 4, 0)
        txtSuma.Multiline = False
        txtSuma.Name = "txtSuma"
        txtSuma.ScrollBars = ScrollBars.None
        txtSuma.Size = New Size(202, 40)
        txtSuma.TabIndex = 17
        txtSuma.TextAlign = HorizontalAlignment.Right
        txtSuma.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtSuma, "Suma")
        tips.SetToolTipText(txtSuma, "Se copiază pe fiecare linie a operației; o puteți schimba apoi pe rând. Liniile cu suma zero nu se înregistrează.")
        txtSuma.VerticalScrollBarPadding = New Padding(0)
        ' 
        ' btnRenunta
        ' 
        btnRenunta.AccessibleRole = AccessibleRole.PushButton
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Enabled = False
        btnRenunta.Location = New Point(771, 4)
        btnRenunta.Margin = New Padding(6, 4, 0, 4)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(214, 56)
        btnRenunta.TabIndex = 21
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Renunță la operația în lucru și golește formularul.")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AccessibleRole = AccessibleRole.PushButton
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Location = New Point(991, 4)
        btnSalveaza.Margin = New Padding(6, 4, 0, 4)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Primary = True
        btnSalveaza.Size = New Size(214, 56)
        btnSalveaza.TabIndex = 22
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Înregistrează operațiile cu sumă: câte un document și o operație pentru fiecare linie.")
        ' 
        ' tlyPagina
        ' 
        tlyPagina.ColumnCount = 2
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 480F))
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Controls.Add(tlyStanga, 0, 0)
        tlyPagina.Controls.Add(tlyDreapta, 1, 0)
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Location = New Point(0, 0)
        tlyPagina.Margin = New Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.RowCount = 1
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPagina.Size = New Size(1685, 900)
        tlyPagina.TabIndex = 0
        ' 
        ' tlyStanga
        ' 
        tlyStanga.ColumnCount = 1
        tlyStanga.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStanga.Controls.Add(treeSabloane, 0, 0)
        tlyStanga.Dock = DockStyle.Fill
        tlyStanga.Location = New Point(0, 0)
        tlyStanga.Margin = New Padding(0)
        tlyStanga.Name = "tlyStanga"
        tlyStanga.RowCount = 1
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyStanga.Size = New Size(480, 900)
        tlyStanga.TabIndex = 1
        ' 
        ' treeSabloane
        ' 
        treeSabloane.Dock = DockStyle.Fill
        treeSabloane.HeaderCaption = " OPERAȚII DIVERSE"
        treeSabloane.HeaderVisible = True
        treeSabloane.Indent = 14
        treeSabloane.LeftIconSize = New Size(16, 16)
        treeSabloane.Location = New Point(0, 0)
        treeSabloane.Margin = New Padding(0, 0, 6, 0)
        treeSabloane.Name = "treeSabloane"
        treeSabloane.SearchBackColor = SystemColors.Control
        treeSabloane.SearchDefaultText = "… căutare operație …"
        treeSabloane.SearchSeparatorColor = SystemColors.ActiveBorder
        treeSabloane.SearchSeparatorWidth = 2
        treeSabloane.SearchShow = True
        treeSabloane.Size = New Size(474, 900)
        treeSabloane.TabIndex = 3
        ' 
        ' tlyDreapta
        ' 
        tlyDreapta.ColumnCount = 1
        tlyDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDreapta.Controls.Add(tlyAntet, 0, 0)
        tlyDreapta.Controls.Add(gridLinii, 0, 1)
        tlyDreapta.Controls.Add(tlyJos, 0, 2)
        tlyDreapta.Dock = DockStyle.Fill
        tlyDreapta.Location = New Point(480, 0)
        tlyDreapta.Margin = New Padding(0)
        tlyDreapta.Name = "tlyDreapta"
        tlyDreapta.RowCount = 3
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Absolute, 209F))
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDreapta.Size = New Size(1205, 900)
        tlyDreapta.TabIndex = 4
        ' 
        ' tlyAntet
        ' 
        tlyAntet.AutoFitToTheme = False
        tlyAntet.ColumnCount = 6
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyAntet.Controls.Add(lblNota, 0, 0)
        tlyAntet.Controls.Add(lblNrDoc, 1, 0)
        tlyAntet.Controls.Add(lblFelDoc, 2, 0)
        tlyAntet.Controls.Add(lblData, 3, 0)
        tlyAntet.Controls.Add(lblSuma, 4, 0)
        tlyAntet.Controls.Add(cmbNota, 0, 1)
        tlyAntet.Controls.Add(txtNrDoc, 1, 1)
        tlyAntet.Controls.Add(cmbFelDoc, 2, 1)
        tlyAntet.Controls.Add(dtpData, 3, 1)
        tlyAntet.Controls.Add(txtSuma, 4, 1)
        tlyAntet.Controls.Add(lblPartener, 0, 3)
        tlyAntet.Controls.Add(cmbPartener, 0, 4)
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Location = New Point(0, 0)
        tlyAntet.Margin = New Padding(0)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.RowCount = 6
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 10F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 10F))
        tlyAntet.ScaleAbsoluteStyles = False
        tlyAntet.Size = New Size(1205, 209)
        tlyAntet.TabIndex = 5
        ' 
        ' lblNota
        ' 
        lblNota.BorderWidth = 0
        lblNota.Dock = DockStyle.Fill
        lblNota.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNota.Location = New Point(0, 0)
        lblNota.Margin = New Padding(0, 0, 3, 3)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(167, 33)
        lblNota.TabIndex = 6
        lblNota.Text = "Nota contabilă"
        lblNota.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblNrDoc
        ' 
        lblNrDoc.BorderWidth = 0
        lblNrDoc.Dock = DockStyle.Fill
        lblNrDoc.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNrDoc.Location = New Point(170, 0)
        lblNrDoc.Margin = New Padding(0, 0, 3, 3)
        lblNrDoc.Name = "lblNrDoc"
        lblNrDoc.Size = New Size(227, 33)
        lblNrDoc.TabIndex = 7
        lblNrDoc.Text = "Număr document"
        lblNrDoc.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblFelDoc
        ' 
        lblFelDoc.BorderWidth = 0
        lblFelDoc.Dock = DockStyle.Fill
        lblFelDoc.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblFelDoc.Location = New Point(400, 0)
        lblFelDoc.Margin = New Padding(0, 0, 3, 3)
        lblFelDoc.Name = "lblFelDoc"
        lblFelDoc.Size = New Size(247, 33)
        lblFelDoc.TabIndex = 8
        lblFelDoc.Text = "Fel document"
        lblFelDoc.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblData
        ' 
        lblData.BorderWidth = 0
        lblData.Dock = DockStyle.Fill
        lblData.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblData.Location = New Point(650, 0)
        lblData.Margin = New Padding(0, 0, 3, 3)
        lblData.Name = "lblData"
        lblData.Size = New Size(207, 33)
        lblData.TabIndex = 9
        lblData.Text = "Data document"
        lblData.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblPartener
        ' 
        lblPartener.BorderWidth = 0
        tlyAntet.SetColumnSpan(lblPartener, 2)
        lblPartener.Dock = DockStyle.Fill
        lblPartener.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblPartener.Location = New Point(0, 86)
        lblPartener.Margin = New Padding(0, 0, 3, 3)
        lblPartener.Name = "lblPartener"
        lblPartener.Size = New Size(397, 33)
        lblPartener.TabIndex = 10
        lblPartener.Text = "Partener"
        lblPartener.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblSuma
        ' 
        lblSuma.BorderWidth = 0
        lblSuma.Dock = DockStyle.Fill
        lblSuma.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblSuma.Location = New Point(860, 0)
        lblSuma.Margin = New Padding(0, 0, 3, 3)
        lblSuma.Name = "lblSuma"
        lblSuma.Size = New Size(207, 33)
        lblSuma.TabIndex = 11
        lblSuma.Text = "Suma"
        lblSuma.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' gridLinii
        ' 
        gridLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLinii.BackColor = SystemColors.Window
        gridLinii.ColumnFillMode = KBotFillMode.SpecificColumn
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
        KBotDataColumn8.Width = 240
        KBotDataColumn9.Aggregate = KBotAggregate.Sum
        KBotDataColumn9.AggregateFormatString = Nothing
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
        gridLinii.Location = New Point(0, 215)
        gridLinii.Margin = New Padding(0, 6, 0, 6)
        gridLinii.Name = "gridLinii"
        gridLinii.Size = New Size(1205, 615)
        gridLinii.TabIndex = 18
        ' 
        ' tlyJos
        ' 
        tlyJos.ColumnCount = 3
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyJos.Controls.Add(lblStare, 0, 0)
        tlyJos.Controls.Add(btnRenunta, 1, 0)
        tlyJos.Controls.Add(btnSalveaza, 2, 0)
        tlyJos.Dock = DockStyle.Fill
        tlyJos.Location = New Point(0, 836)
        tlyJos.Margin = New Padding(0)
        tlyJos.Name = "tlyJos"
        tlyJos.RowCount = 1
        tlyJos.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyJos.Size = New Size(1205, 64)
        tlyJos.TabIndex = 19
        ' 
        ' lblStare
        ' 
        lblStare.BorderWidth = 0
        lblStare.Dock = DockStyle.Fill
        lblStare.Font = New Font("Calibri", 9F)
        lblStare.Location = New Point(0, 0)
        lblStare.Margin = New Padding(0, 0, 8, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(757, 64)
        lblStare.TabIndex = 20
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' OperatiiDiversePage
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Margin = New Padding(4)
        Name = "OperatiiDiversePage"
        Size = New Size(1685, 900)
        tlyPagina.ResumeLayout(False)
        tlyStanga.ResumeLayout(False)
        tlyDreapta.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        CType(gridLinii, ComponentModel.ISupportInitialize).EndInit()
        tlyJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlyStanga As KBotTableLayoutPanel
    Friend WithEvents treeSabloane As AdvancedTreeControl
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
