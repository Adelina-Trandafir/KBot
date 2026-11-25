Option Strict On
Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class NotaManualaView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        tlyMain = New KBotTableLayoutPanel()
        pnlAntet = New KBotTableLayoutPanel()
        lblNota = New Label()
        cmbNota = New KBotComboBox()
        lblDataDocument = New Label()
        dtpDocument = New KBotDatePicker()
        dtpOperatie = New KBotDatePicker()
        lblTipOperatie = New Label()
        lblExplicatie = New Label()
        cmbExplicatie = New KBotComboBox()
        chkCfpp = New CheckBox()
        chkChelt = New CheckBox()
        PnlDataOperatieLabel = New KBotTableLayoutPanel()
        lblDataOperatie = New Label()
        chkDocument = New CheckBox()
        lblDocument = New Label()
        cmbDocument = New KBotComboBox()
        lblFelDocument = New Label()
        cmbFelDocument = New KBotComboBox()
        lblJurnal = New Label()
        cmbJurnal = New KBotComboBox()
        lblCompartiment = New Label()
        cmbTipOperatie = New KBotComboBox()
        cmbCompartiment = New KBotComboBox()
        lblClsf = New Label()
        cmbClsf = New KBotComboBox()
        KBotTableLayoutPanel1 = New KBotTableLayoutPanel()
        BtnIesire = New KBotButton()
        BtnOperatieNoua = New KBotButton()
        BtnDocumentNou = New KBotButton()
        BtnSalveaza = New KBotButton()
        pnlLinii = New KBotTableLayoutPanel()
        gridDebit = New KBotDataView()
        gridCredit = New KBotDataView()
        KBotToolTip1 = New KBotToolTip(components)
        tlyMain.SuspendLayout()
        pnlAntet.SuspendLayout()
        PnlDataOperatieLabel.SuspendLayout()
        KBotTableLayoutPanel1.SuspendLayout()
        pnlLinii.SuspendLayout()
        CType(gridDebit, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridCredit, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(pnlAntet, 0, 0)
        tlyMain.Controls.Add(pnlLinii, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 2
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1446, 563)
        tlyMain.TabIndex = 0
        ' 
        ' pnlAntet
        ' 
        pnlAntet.ColumnCount = 6
        pnlAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20F))
        pnlAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20F))
        pnlAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20F))
        pnlAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20F))
        pnlAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        pnlAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        pnlAntet.Controls.Add(lblNota, 0, 0)
        pnlAntet.Controls.Add(cmbNota, 0, 1)
        pnlAntet.Controls.Add(lblDataDocument, 2, 0)
        pnlAntet.Controls.Add(dtpDocument, 2, 1)
        pnlAntet.Controls.Add(dtpOperatie, 3, 1)
        pnlAntet.Controls.Add(lblTipOperatie, 1, 2)
        pnlAntet.Controls.Add(lblExplicatie, 2, 4)
        pnlAntet.Controls.Add(cmbExplicatie, 2, 5)
        pnlAntet.Controls.Add(chkCfpp, 0, 6)
        pnlAntet.Controls.Add(chkChelt, 3, 6)
        pnlAntet.Controls.Add(PnlDataOperatieLabel, 3, 0)
        pnlAntet.Controls.Add(lblDocument, 0, 2)
        pnlAntet.Controls.Add(cmbDocument, 0, 3)
        pnlAntet.Controls.Add(lblFelDocument, 0, 4)
        pnlAntet.Controls.Add(cmbFelDocument, 0, 5)
        pnlAntet.Controls.Add(lblJurnal, 1, 0)
        pnlAntet.Controls.Add(cmbJurnal, 1, 1)
        pnlAntet.Controls.Add(lblCompartiment, 1, 4)
        pnlAntet.Controls.Add(cmbTipOperatie, 1, 3)
        pnlAntet.Controls.Add(cmbCompartiment, 1, 5)
        pnlAntet.Controls.Add(lblClsf, 2, 2)
        pnlAntet.Controls.Add(cmbClsf, 2, 3)
        pnlAntet.Controls.Add(KBotTableLayoutPanel1, 4, 0)
        pnlAntet.Dock = DockStyle.Fill
        pnlAntet.Enabled = False
        pnlAntet.Location = New Point(4, 4)
        pnlAntet.Margin = New Padding(4)
        pnlAntet.Name = "pnlAntet"
        pnlAntet.Padding = New Padding(10, 6, 10, 0)
        pnlAntet.RowCount = 7
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 24F))
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 24F))
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 24F))
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        pnlAntet.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        pnlAntet.Size = New Size(1438, 257)
        pnlAntet.TabIndex = 0
        ' 
        ' lblNota
        ' 
        lblNota.Dock = DockStyle.Fill
        lblNota.Location = New Point(10, 6)
        lblNota.Margin = New Padding(0)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(283, 24)
        lblNota.TabIndex = 0
        lblNota.Text = "Notă contabilă"
        lblNota.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbNota
        ' 
        cmbNota.AccessibleName = "Notă contabilă"
        cmbNota.Dock = DockStyle.Fill
        cmbNota.Editable = True
        cmbNota.FindAsYouType = True
        cmbNota.FindInItemOrder = True
        cmbNota.LimitToList = False
        cmbNota.Location = New Point(14, 34)
        cmbNota.Margin = New Padding(4)
        cmbNota.Name = "cmbNota"
        cmbNota.RequireExplicitSelection = True
        cmbNota.Size = New Size(275, 37)
        cmbNota.TabIndex = 1
        ' 
        ' lblDataDocument
        ' 
        lblDataDocument.Dock = DockStyle.Fill
        lblDataDocument.Location = New Point(576, 6)
        lblDataDocument.Margin = New Padding(0)
        lblDataDocument.Name = "lblDataDocument"
        lblDataDocument.Size = New Size(283, 24)
        lblDataDocument.TabIndex = 4
        lblDataDocument.Text = "Dată document"
        lblDataDocument.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' dtpDocument
        ' 
        dtpDocument.AccessibleName = "Dată document"
        dtpDocument.Dock = DockStyle.Fill
        dtpDocument.Location = New Point(580, 34)
        dtpDocument.Margin = New Padding(4)
        dtpDocument.Name = "dtpDocument"
        dtpDocument.Size = New Size(275, 36)
        dtpDocument.TabIndex = 5
        ' 
        ' dtpOperatie
        ' 
        dtpOperatie.AccessibleName = "Dată operație"
        dtpOperatie.Dock = DockStyle.Fill
        dtpOperatie.Location = New Point(863, 34)
        dtpOperatie.Margin = New Padding(4)
        dtpOperatie.Name = "dtpOperatie"
        dtpOperatie.Size = New Size(275, 36)
        dtpOperatie.TabIndex = 7
        ' 
        ' lblTipOperatie
        ' 
        lblTipOperatie.Dock = DockStyle.Fill
        lblTipOperatie.Location = New Point(293, 74)
        lblTipOperatie.Margin = New Padding(0)
        lblTipOperatie.Name = "lblTipOperatie"
        lblTipOperatie.Size = New Size(283, 24)
        lblTipOperatie.TabIndex = 10
        lblTipOperatie.Text = "Tip operație"
        lblTipOperatie.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' lblExplicatie
        ' 
        pnlAntet.SetColumnSpan(lblExplicatie, 2)
        lblExplicatie.Dock = DockStyle.Fill
        lblExplicatie.Location = New Point(576, 142)
        lblExplicatie.Margin = New Padding(0)
        lblExplicatie.Name = "lblExplicatie"
        lblExplicatie.Size = New Size(566, 24)
        lblExplicatie.TabIndex = 18
        lblExplicatie.Text = "Explicație"
        lblExplicatie.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbExplicatie
        ' 
        cmbExplicatie.AccessibleName = "Explicație"
        pnlAntet.SetColumnSpan(cmbExplicatie, 2)
        cmbExplicatie.Dock = DockStyle.Fill
        cmbExplicatie.DropDownWidth = 520
        cmbExplicatie.Editable = True
        cmbExplicatie.FindAsYouType = True
        cmbExplicatie.FindInItemOrder = True
        cmbExplicatie.LimitToList = False
        cmbExplicatie.Location = New Point(580, 170)
        cmbExplicatie.Margin = New Padding(4)
        cmbExplicatie.Name = "cmbExplicatie"
        cmbExplicatie.RequireExplicitSelection = True
        cmbExplicatie.Size = New Size(558, 37)
        cmbExplicatie.TabIndex = 19
        ' 
        ' chkCfpp
        ' 
        chkCfpp.Checked = True
        chkCfpp.CheckState = CheckState.Checked
        chkCfpp.Dock = DockStyle.Fill
        chkCfpp.Location = New Point(14, 214)
        chkCfpp.Margin = New Padding(4)
        chkCfpp.Name = "chkCfpp"
        chkCfpp.Size = New Size(275, 39)
        chkCfpp.TabIndex = 20
        chkCfpp.Text = "C.F.P.P."
        ' 
        ' chkChelt
        ' 
        chkChelt.Dock = DockStyle.Fill
        chkChelt.Location = New Point(863, 214)
        chkChelt.Margin = New Padding(4)
        chkChelt.Name = "chkChelt"
        chkChelt.RightToLeft = RightToLeft.Yes
        chkChelt.Size = New Size(275, 39)
        chkChelt.TabIndex = 21
        chkChelt.Text = "Cheltuială"
        ' 
        ' PnlDataOperatieLabel
        ' 
        PnlDataOperatieLabel.ColumnCount = 2
        PnlDataOperatieLabel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        PnlDataOperatieLabel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 32F))
        PnlDataOperatieLabel.Controls.Add(lblDataOperatie, 0, 0)
        PnlDataOperatieLabel.Controls.Add(chkDocument, 1, 0)
        PnlDataOperatieLabel.Dock = DockStyle.Fill
        PnlDataOperatieLabel.Location = New Point(859, 6)
        PnlDataOperatieLabel.Margin = New Padding(0)
        PnlDataOperatieLabel.Name = "PnlDataOperatieLabel"
        PnlDataOperatieLabel.RowCount = 1
        PnlDataOperatieLabel.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        PnlDataOperatieLabel.Size = New Size(283, 24)
        PnlDataOperatieLabel.TabIndex = 23
        ' 
        ' lblDataOperatie
        ' 
        lblDataOperatie.Dock = DockStyle.Fill
        lblDataOperatie.Location = New Point(0, 0)
        lblDataOperatie.Margin = New Padding(0)
        lblDataOperatie.Name = "lblDataOperatie"
        lblDataOperatie.Size = New Size(251, 24)
        lblDataOperatie.TabIndex = 7
        lblDataOperatie.Text = "Dată operație"
        lblDataOperatie.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' chkDocument
        ' 
        chkDocument.Dock = DockStyle.Fill
        chkDocument.Location = New Point(251, 0)
        chkDocument.Margin = New Padding(0)
        chkDocument.Name = "chkDocument"
        chkDocument.Size = New Size(32, 24)
        chkDocument.TabIndex = 22
        KBotToolTip1.SetToolTipText(chkDocument, "Modifică data documentului")
        chkDocument.Visible = False
        ' 
        ' lblDocument
        ' 
        lblDocument.Dock = DockStyle.Fill
        lblDocument.Location = New Point(10, 74)
        lblDocument.Margin = New Padding(0)
        lblDocument.Name = "lblDocument"
        lblDocument.Size = New Size(283, 24)
        lblDocument.TabIndex = 2
        lblDocument.Text = "Număr document"
        lblDocument.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbDocument
        ' 
        cmbDocument.AccessibleName = "Număr document"
        cmbDocument.Dock = DockStyle.Fill
        cmbDocument.DropDownWidth = 420
        cmbDocument.Editable = True
        cmbDocument.FindAsYouType = True
        cmbDocument.FindInItemOrder = True
        cmbDocument.LimitToList = False
        cmbDocument.Location = New Point(14, 102)
        cmbDocument.Margin = New Padding(4)
        cmbDocument.Name = "cmbDocument"
        cmbDocument.RequireExplicitSelection = True
        cmbDocument.Size = New Size(275, 37)
        cmbDocument.TabIndex = 3
        ' 
        ' lblFelDocument
        ' 
        lblFelDocument.Dock = DockStyle.Fill
        lblFelDocument.Location = New Point(10, 142)
        lblFelDocument.Margin = New Padding(0)
        lblFelDocument.Name = "lblFelDocument"
        lblFelDocument.Size = New Size(283, 24)
        lblFelDocument.TabIndex = 8
        lblFelDocument.Text = "Fel document"
        lblFelDocument.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbFelDocument
        ' 
        cmbFelDocument.AccessibleName = "Fel document"
        cmbFelDocument.Dock = DockStyle.Fill
        cmbFelDocument.Editable = True
        cmbFelDocument.FindAsYouType = True
        cmbFelDocument.FindInItemOrder = True
        cmbFelDocument.LimitToList = False
        cmbFelDocument.Location = New Point(14, 170)
        cmbFelDocument.Margin = New Padding(4)
        cmbFelDocument.Name = "cmbFelDocument"
        cmbFelDocument.RequireExplicitSelection = True
        cmbFelDocument.Size = New Size(275, 37)
        cmbFelDocument.TabIndex = 9
        ' 
        ' lblJurnal
        ' 
        lblJurnal.Dock = DockStyle.Fill
        lblJurnal.Location = New Point(293, 6)
        lblJurnal.Margin = New Padding(0)
        lblJurnal.Name = "lblJurnal"
        lblJurnal.Size = New Size(283, 24)
        lblJurnal.TabIndex = 12
        lblJurnal.Text = "Jurnal"
        lblJurnal.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbJurnal
        ' 
        cmbJurnal.AccessibleName = "Jurnal"
        cmbJurnal.Dock = DockStyle.Fill
        cmbJurnal.DropDownWidth = 420
        cmbJurnal.Editable = True
        cmbJurnal.FindAsYouType = True
        cmbJurnal.FindInItemOrder = True
        cmbJurnal.Location = New Point(297, 34)
        cmbJurnal.Margin = New Padding(4)
        cmbJurnal.Name = "cmbJurnal"
        cmbJurnal.RequireExplicitSelection = True
        cmbJurnal.Size = New Size(275, 37)
        cmbJurnal.TabIndex = 13
        ' 
        ' lblCompartiment
        ' 
        lblCompartiment.Dock = DockStyle.Fill
        lblCompartiment.Location = New Point(293, 142)
        lblCompartiment.Margin = New Padding(0)
        lblCompartiment.Name = "lblCompartiment"
        lblCompartiment.Size = New Size(283, 24)
        lblCompartiment.TabIndex = 14
        lblCompartiment.Text = "Compartiment"
        lblCompartiment.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbTipOperatie
        ' 
        cmbTipOperatie.AccessibleName = "Tip operație"
        cmbTipOperatie.Dock = DockStyle.Fill
        cmbTipOperatie.Editable = True
        cmbTipOperatie.FindAsYouType = True
        cmbTipOperatie.FindInItemOrder = True
        cmbTipOperatie.Location = New Point(297, 102)
        cmbTipOperatie.Margin = New Padding(4)
        cmbTipOperatie.Name = "cmbTipOperatie"
        cmbTipOperatie.Size = New Size(275, 37)
        cmbTipOperatie.TabIndex = 11
        ' 
        ' cmbCompartiment
        ' 
        cmbCompartiment.AccessibleName = "Compartiment"
        cmbCompartiment.Dock = DockStyle.Fill
        cmbCompartiment.Editable = True
        cmbCompartiment.FindAsYouType = True
        cmbCompartiment.FindInItemOrder = True
        cmbCompartiment.LimitToList = False
        cmbCompartiment.Location = New Point(297, 170)
        cmbCompartiment.Margin = New Padding(4)
        cmbCompartiment.Name = "cmbCompartiment"
        cmbCompartiment.RequireExplicitSelection = True
        cmbCompartiment.Size = New Size(275, 37)
        cmbCompartiment.TabIndex = 15
        ' 
        ' lblClsf
        ' 
        pnlAntet.SetColumnSpan(lblClsf, 2)
        lblClsf.Dock = DockStyle.Fill
        lblClsf.Location = New Point(576, 74)
        lblClsf.Margin = New Padding(0)
        lblClsf.Name = "lblClsf"
        lblClsf.Size = New Size(566, 24)
        lblClsf.TabIndex = 16
        lblClsf.Text = "Clasificație"
        lblClsf.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' cmbClsf
        ' 
        cmbClsf.AccessibleName = "Clasificație"
        pnlAntet.SetColumnSpan(cmbClsf, 2)
        cmbClsf.Dock = DockStyle.Fill
        cmbClsf.DropDownWidth = 520
        cmbClsf.Editable = True
        cmbClsf.FindAsYouType = True
        cmbClsf.FindInItemOrder = True
        cmbClsf.Location = New Point(580, 102)
        cmbClsf.Margin = New Padding(4)
        cmbClsf.Name = "cmbClsf"
        cmbClsf.Size = New Size(558, 37)
        cmbClsf.TabIndex = 17
        ' 
        ' KBotTableLayoutPanel1
        ' 
        KBotTableLayoutPanel1.ColumnCount = 1
        pnlAntet.SetColumnSpan(KBotTableLayoutPanel1, 2)
        KBotTableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        KBotTableLayoutPanel1.Controls.Add(BtnIesire, 0, 3)
        KBotTableLayoutPanel1.Controls.Add(BtnOperatieNoua, 0, 2)
        KBotTableLayoutPanel1.Controls.Add(BtnDocumentNou, 0, 1)
        KBotTableLayoutPanel1.Controls.Add(BtnSalveaza, 0, 0)
        KBotTableLayoutPanel1.Dock = DockStyle.Fill
        KBotTableLayoutPanel1.Location = New Point(1142, 6)
        KBotTableLayoutPanel1.Margin = New Padding(0)
        KBotTableLayoutPanel1.Name = "KBotTableLayoutPanel1"
        KBotTableLayoutPanel1.RowCount = 4
        pnlAntet.SetRowSpan(KBotTableLayoutPanel1, 6)
        KBotTableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        KBotTableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        KBotTableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        KBotTableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 25F))
        KBotTableLayoutPanel1.Size = New Size(286, 204)
        KBotTableLayoutPanel1.TabIndex = 24
        ' 
        ' BtnIesire
        ' 
        BtnIesire.AccessibleRole = AccessibleRole.PushButton
        BtnIesire.Dock = DockStyle.Fill
        BtnIesire.Location = New Point(8, 161)
        BtnIesire.Margin = New Padding(8)
        BtnIesire.Name = "BtnIesire"
        BtnIesire.Size = New Size(270, 35)
        BtnIesire.TabIndex = 3
        BtnIesire.Text = "Ieșire"
        ' 
        ' BtnOperatieNoua
        ' 
        BtnOperatieNoua.AccessibleRole = AccessibleRole.PushButton
        BtnOperatieNoua.Dock = DockStyle.Fill
        BtnOperatieNoua.Location = New Point(8, 110)
        BtnOperatieNoua.Margin = New Padding(8)
        BtnOperatieNoua.Name = "BtnOperatieNoua"
        BtnOperatieNoua.Size = New Size(270, 35)
        BtnOperatieNoua.TabIndex = 2
        BtnOperatieNoua.Text = "Operație nouă"
        ' 
        ' BtnDocumentNou
        ' 
        BtnDocumentNou.AccessibleRole = AccessibleRole.PushButton
        BtnDocumentNou.Dock = DockStyle.Fill
        BtnDocumentNou.Location = New Point(8, 59)
        BtnDocumentNou.Margin = New Padding(8)
        BtnDocumentNou.Name = "BtnDocumentNou"
        BtnDocumentNou.Size = New Size(270, 35)
        BtnDocumentNou.TabIndex = 1
        BtnDocumentNou.Text = "Document nou"
        ' 
        ' BtnSalveaza
        ' 
        BtnSalveaza.AccessibleRole = AccessibleRole.PushButton
        BtnSalveaza.Dock = DockStyle.Fill
        BtnSalveaza.Location = New Point(8, 8)
        BtnSalveaza.Margin = New Padding(8)
        BtnSalveaza.Name = "BtnSalveaza"
        BtnSalveaza.Primary = True
        BtnSalveaza.Size = New Size(270, 35)
        BtnSalveaza.TabIndex = 0
        BtnSalveaza.Text = "Salvează"
        ' 
        ' pnlLinii
        ' 
        pnlLinii.ColumnCount = 2
        pnlLinii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        pnlLinii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        pnlLinii.Controls.Add(gridDebit, 0, 0)
        pnlLinii.Controls.Add(gridCredit, 1, 0)
        pnlLinii.Dock = DockStyle.Fill
        pnlLinii.Location = New Point(4, 269)
        pnlLinii.Margin = New Padding(4)
        pnlLinii.Name = "pnlLinii"
        pnlLinii.Padding = New Padding(10, 0, 10, 0)
        pnlLinii.RowCount = 1
        pnlLinii.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        pnlLinii.Size = New Size(1438, 290)
        pnlLinii.TabIndex = 1
        ' 
        ' gridDebit
        ' 
        gridDebit.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridDebit.BackColor = SystemColors.Window
        gridDebit.ColumnFillMode = KBotFillMode.Proportional
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.Combo
        KBotDataColumn1.ComboDropDownWidth = 520
        KBotDataColumn1.ComboFindInItemOrder = True
        KBotDataColumn1.ComboRequireExplicitSelection = True
        KBotDataColumn1.CommitOnSelect = True
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Cont debit"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "cont"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 190
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnType = KBotColumnType.Combo
        KBotDataColumn2.ComboDropDownWidth = 450
        KBotDataColumn2.ComboFindInItemOrder = True
        KBotDataColumn2.ComboRequireExplicitSelection = True
        KBotDataColumn2.CommitOnSelect = True
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Partener"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "partener"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 150
        KBotDataColumn3.Aggregate = KBotAggregate.Sum
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Sumă debit"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "suma"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 95
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ButtonKind = KBotButtonKind.Delete
        KBotDataColumn4.ColumnType = KBotColumnType.Button
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = ""
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "sterge"
        KBotDataColumn4.MaxWidth = 40
        KBotDataColumn4.MinWidth = 36
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 36
        gridDebit.Columns.Add(KBotDataColumn1)
        gridDebit.Columns.Add(KBotDataColumn2)
        gridDebit.Columns.Add(KBotDataColumn3)
        gridDebit.Columns.Add(KBotDataColumn4)
        gridDebit.Dock = DockStyle.Fill
        gridDebit.FooterCaption = "Total debit"
        gridDebit.FooterVisible = True
        gridDebit.Location = New Point(14, 4)
        gridDebit.Margin = New Padding(4)
        gridDebit.Name = "gridDebit"
        gridDebit.ReadOnlyGrid = True
        gridDebit.Size = New Size(701, 282)
        gridDebit.TabIndex = 0
        ' 
        ' gridCredit
        ' 
        gridCredit.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridCredit.BackColor = SystemColors.Window
        gridCredit.ColumnFillMode = KBotFillMode.Proportional
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Combo
        KBotDataColumn5.ComboDropDownWidth = 520
        KBotDataColumn5.ComboFindInItemOrder = True
        KBotDataColumn5.ComboRequireExplicitSelection = True
        KBotDataColumn5.CommitOnSelect = True
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Cont credit"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "cont"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 190
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBotColumnType.Combo
        KBotDataColumn6.ComboDropDownWidth = 450
        KBotDataColumn6.ComboFindInItemOrder = True
        KBotDataColumn6.ComboRequireExplicitSelection = True
        KBotDataColumn6.CommitOnSelect = True
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Partener"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "partener"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Width = 150
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Sumă credit"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn7.Key = "suma"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 95
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ButtonKind = KBotButtonKind.Delete
        KBotDataColumn8.ColumnType = KBotColumnType.Button
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = ""
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Key = "sterge"
        KBotDataColumn8.MaxWidth = 40
        KBotDataColumn8.MinWidth = 36
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.Width = 36
        gridCredit.Columns.Add(KBotDataColumn5)
        gridCredit.Columns.Add(KBotDataColumn6)
        gridCredit.Columns.Add(KBotDataColumn7)
        gridCredit.Columns.Add(KBotDataColumn8)
        gridCredit.Dock = DockStyle.Fill
        gridCredit.FooterCaption = "Total credit"
        gridCredit.FooterVisible = True
        gridCredit.Location = New Point(723, 4)
        gridCredit.Margin = New Padding(4)
        gridCredit.Name = "gridCredit"
        gridCredit.ReadOnlyGrid = True
        gridCredit.Size = New Size(701, 282)
        gridCredit.TabIndex = 1
        ' 
        ' NotaManualaView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "NotaManualaView"
        Size = New Size(1446, 563)
        tlyMain.ResumeLayout(False)
        pnlAntet.ResumeLayout(False)
        PnlDataOperatieLabel.ResumeLayout(False)
        KBotTableLayoutPanel1.ResumeLayout(False)
        pnlLinii.ResumeLayout(False)
        CType(gridDebit, ComponentModel.ISupportInitialize).EndInit()
        CType(gridCredit, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Private tlyMain As KBotTableLayoutPanel
    Private pnlAntet As KBotTableLayoutPanel
    Private lblNota As Label
    Private WithEvents cmbNota As KBotComboBox
    Private lblDocument As Label
    Private WithEvents cmbDocument As KBotComboBox
    Private lblDataDocument As Label
    Private WithEvents dtpDocument As KBotDatePicker
    Private WithEvents dtpOperatie As KBotDatePicker
    Private lblFelDocument As Label
    Private WithEvents cmbFelDocument As KBotComboBox
    Private lblTipOperatie As Label
    Private WithEvents cmbTipOperatie As KBotComboBox
    Private lblJurnal As Label
    Private WithEvents cmbJurnal As KBotComboBox
    Private lblCompartiment As Label
    Private WithEvents cmbCompartiment As KBotComboBox
    Private lblClsf As Label
    Private WithEvents cmbClsf As KBotComboBox
    Private lblExplicatie As Label
    Private WithEvents cmbExplicatie As KBotComboBox
    Private WithEvents chkCfpp As CheckBox
    Private WithEvents chkChelt As CheckBox
    Private WithEvents chkDocument As CheckBox
    Private pnlLinii As KBotTableLayoutPanel
    Private WithEvents gridDebit As KBotDataView
    Private WithEvents gridCredit As KBotDataView
    Friend WithEvents PnlDataOperatieLabel As KBotTableLayoutPanel
    Private WithEvents lblDataOperatie As Label
    Friend WithEvents KBotToolTip1 As KBotToolTip
    Private components As System.ComponentModel.IContainer
    Friend WithEvents KBotTableLayoutPanel1 As KBotTableLayoutPanel
    Friend WithEvents BtnSalveaza As KBotButton
    Friend WithEvents BtnIesire As KBotButton
    Friend WithEvents BtnOperatieNoua As KBotButton
    Friend WithEvents BtnDocumentNou As KBotButton
End Class
