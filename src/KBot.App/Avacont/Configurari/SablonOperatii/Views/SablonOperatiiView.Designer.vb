Imports KBot.Controls

' «Operatii» page of the «Sabloane» window (slice AV01-14, redone in AV01-16): the operation templates, laid out like the Access form «Configurare operatii
' contabile automate» -- the tree of operations on the left (40 % of the width; type M: under the classification; type A: one flat list), the header of the
' selected operation on the right and, under it, ONE table with the lines of the operation (type M: one row per form of the classification; type A: the table
' takes the whole width under the tree and the fields, with an empty row at the end). The only button of the page is «Salveaza».
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 96 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SablonOperatiiView
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
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn13 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyRight = New KBotTableLayoutPanel()
        lblExplicatie = New Label()
        txtExplicatie = New KBotTextField()
        lblJurnal = New Label()
        cmbJurnal = New KBotComboBox()
        lblCompartiment = New Label()
        cmbCompartiment = New KBotComboBox()
        lblFel = New Label()
        cmbFel = New KBotComboBox()
        lblFelOp = New Label()
        cmbFelOp = New KBotComboBox()
        gridLinii = New KBotDataView()
        tlySubsol = New KBotTableLayoutPanel()
        lblStare = New Label()
        btnSalveaza = New Button()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyRight.SuspendLayout()
        CType(gridLinii, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyMain
        ' 
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(pnlCard, 0, 0)
        tlyMain.Controls.Add(tlySubsol, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 2
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 93F))
        tlyMain.Size = New Size(1500, 900)
        tlyMain.TabIndex = 0
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 0)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 9, 12, 9)
        pnlCard.Size = New Size(1500, 807)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.AutoFitToTheme = False
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 35F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 65F))
        tlyBody.Controls.Add(tree, 0, 0)
        tlyBody.Controls.Add(tlyRight, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1476, 789)
        tlyBody.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BackColor = SystemColors.Window
        tree.BorderColor = SystemColors.ActiveBorder
        tree.Dock = DockStyle.Fill
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " OPERAȚII CONTABILE"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderRightIcon = My.Resources.Resources.settings__1_
        tree.HeaderRightIconTooltip = "Opțiuni"
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0, 0, 12, 0)
        tree.Name = "tree"
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… căutare explicație …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.Size = New Size(504, 789)
        tree.TabIndex = 0
        ' 
        ' tlyRight
        ' 
        tlyRight.AutoFitToTheme = False
        tlyRight.ColumnCount = 1
        tlyRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRight.Controls.Add(lblExplicatie, 0, 0)
        tlyRight.Controls.Add(txtExplicatie, 0, 1)
        tlyRight.Controls.Add(lblJurnal, 0, 2)
        tlyRight.Controls.Add(cmbJurnal, 0, 3)
        tlyRight.Controls.Add(lblCompartiment, 0, 4)
        tlyRight.Controls.Add(cmbCompartiment, 0, 5)
        tlyRight.Controls.Add(lblFel, 0, 6)
        tlyRight.Controls.Add(cmbFel, 0, 7)
        tlyRight.Controls.Add(lblFelOp, 0, 8)
        tlyRight.Controls.Add(cmbFelOp, 0, 9)
        tlyRight.Controls.Add(gridLinii, 0, 10)
        tlyRight.Dock = DockStyle.Fill
        tlyRight.Location = New Point(516, 0)
        tlyRight.Margin = New Padding(0)
        tlyRight.Name = "tlyRight"
        tlyRight.RowCount = 11
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 33F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 63F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 33F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 63F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 33F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 33F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 63F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 33F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 63F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRight.ScaleAbsoluteStyles = False
        tlyRight.Size = New Size(960, 789)
        tlyRight.TabIndex = 1
        ' 
        ' lblExplicatie
        ' 
        lblExplicatie.Dock = DockStyle.Fill
        lblExplicatie.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblExplicatie.Location = New Point(0, 0)
        lblExplicatie.Margin = New Padding(0)
        lblExplicatie.Name = "lblExplicatie"
        lblExplicatie.Size = New Size(960, 33)
        lblExplicatie.TabIndex = 0
        lblExplicatie.Text = "Explicație document"
        lblExplicatie.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' txtExplicatie
        ' 
        txtExplicatie.Dock = DockStyle.Fill
        txtExplicatie.Location = New Point(0, 33)
        txtExplicatie.Margin = New Padding(0, 0, 0, 3)
        txtExplicatie.MaxLength = 255
        txtExplicatie.Name = "txtExplicatie"
        txtExplicatie.Size = New Size(960, 60)
        txtExplicatie.TabIndex = 1
        ' 
        ' lblJurnal
        ' 
        lblJurnal.Dock = DockStyle.Fill
        lblJurnal.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblJurnal.Location = New Point(0, 96)
        lblJurnal.Margin = New Padding(0)
        lblJurnal.Name = "lblJurnal"
        lblJurnal.Size = New Size(960, 33)
        lblJurnal.TabIndex = 2
        lblJurnal.Text = "Cod jurnal"
        lblJurnal.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' cmbJurnal
        ' 
        cmbJurnal.Dock = DockStyle.Fill
        cmbJurnal.Editable = True
        cmbJurnal.FindAsYouType = True
        cmbJurnal.Location = New Point(0, 129)
        cmbJurnal.Margin = New Padding(0, 0, 0, 3)
        cmbJurnal.Name = "cmbJurnal"
        cmbJurnal.Size = New Size(960, 37)
        cmbJurnal.TabIndex = 3
        ' 
        ' lblCompartiment
        ' 
        lblCompartiment.Dock = DockStyle.Fill
        lblCompartiment.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCompartiment.Location = New Point(0, 192)
        lblCompartiment.Margin = New Padding(0)
        lblCompartiment.Name = "lblCompartiment"
        lblCompartiment.Size = New Size(960, 33)
        lblCompartiment.TabIndex = 4
        lblCompartiment.Text = "Compartiment"
        lblCompartiment.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' cmbCompartiment
        ' 
        cmbCompartiment.Dock = DockStyle.Fill
        cmbCompartiment.Editable = True
        cmbCompartiment.LimitToList = False
        cmbCompartiment.Location = New Point(0, 225)
        cmbCompartiment.Margin = New Padding(0, 0, 0, 3)
        cmbCompartiment.Name = "cmbCompartiment"
        cmbCompartiment.Size = New Size(960, 37)
        cmbCompartiment.TabIndex = 5
        ' 
        ' lblFel
        ' 
        lblFel.Dock = DockStyle.Fill
        lblFel.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblFel.Location = New Point(0, 273)
        lblFel.Margin = New Padding(0)
        lblFel.Name = "lblFel"
        lblFel.Size = New Size(960, 33)
        lblFel.TabIndex = 6
        lblFel.Text = "Tip operație"
        lblFel.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' cmbFel
        ' 
        cmbFel.Dock = DockStyle.Fill
        cmbFel.Location = New Point(0, 306)
        cmbFel.Margin = New Padding(0, 0, 0, 3)
        cmbFel.Name = "cmbFel"
        cmbFel.Size = New Size(960, 37)
        cmbFel.TabIndex = 7
        ' 
        ' lblFelOp
        ' 
        lblFelOp.Dock = DockStyle.Fill
        lblFelOp.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblFelOp.Location = New Point(0, 369)
        lblFelOp.Margin = New Padding(0)
        lblFelOp.Name = "lblFelOp"
        lblFelOp.Size = New Size(960, 33)
        lblFelOp.TabIndex = 8
        lblFelOp.Text = "Fel factură (OP)"
        lblFelOp.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' cmbFelOp
        ' 
        cmbFelOp.Dock = DockStyle.Fill
        cmbFelOp.Editable = True
        cmbFelOp.LimitToList = False
        cmbFelOp.Location = New Point(0, 402)
        cmbFelOp.Margin = New Padding(0, 0, 0, 3)
        cmbFelOp.Name = "cmbFelOp"
        cmbFelOp.Size = New Size(960, 37)
        cmbFelOp.TabIndex = 9
        ' 
        ' gridLinii
        ' 
        gridLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLinii.BackColor = SystemColors.Window
        gridLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Ix"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "ix"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn1.Width = 40
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.ColumnType = KBotColumnType.Combo
        KBotDataColumn2.ComboDropDownWidth = 480
        KBotDataColumn2.HeaderText = "Clasificația"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "clsf"
        KBotDataColumn2.MinWidth = 150
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 180
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnType = KBotColumnType.Combo
        KBotDataColumn3.ComboDropDownWidth = 460
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Cont debit"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "contd"
        KBotDataColumn3.MinWidth = 160
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 200
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "P. Debit"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.HeaderTooltip = "Partenerul variabil" & vbLf & "Bifat: se folosește partenerul operației, înlocuit la întocmirea operației."
        KBotDataColumn4.Key = "pd"
        KBotDataColumn4.MinWidth = 60
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Resizable = False
        KBotDataColumn4.Width = 60
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Combo
        KBotDataColumn5.ComboDropDownWidth = 460
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Cont credit"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "contc"
        KBotDataColumn5.MinWidth = 160
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 200
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "P. Credit"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.HeaderTooltip = "Partenerul variabil" & vbLf & "Bifat: se folosește partenerul operației, înlocuit la întocmirea operației."
        KBotDataColumn6.Key = "pc"
        KBotDataColumn6.MinWidth = 60
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Resizable = False
        KBotDataColumn6.Width = 60
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.ColumnType = KBotColumnType.Combo
        KBotDataColumn7.ComboDropDownWidth = 400
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Partener debit"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "partd"
        KBotDataColumn7.MinWidth = 140
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.Width = 200
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ColumnType = KBotColumnType.Combo
        KBotDataColumn8.ComboDropDownWidth = 400
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Partener credit"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "partc"
        KBotDataColumn8.MinWidth = 140
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.Width = 200
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "O"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.HeaderTooltip = "Ordonanțare"
        KBotDataColumn9.Key = "o"
        KBotDataColumn9.MinWidth = 30
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.Resizable = False
        KBotDataColumn9.Width = 30
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "P/A"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.HeaderTooltip = "Propunere / Angajament"
        KBotDataColumn10.Key = "pa"
        KBotDataColumn10.MinWidth = 30
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.Resizable = False
        KBotDataColumn10.Width = 30
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "C"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.HeaderTooltip = "Cheltuială"
        KBotDataColumn11.Key = "c"
        KBotDataColumn11.MinWidth = 30
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.Resizable = False
        KBotDataColumn11.Width = 30
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.ButtonBorders = KBotBorderSides.None
        KBotDataColumn12.ButtonMargin = New Padding(0)
        KBotDataColumn12.ColumnType = KBotColumnType.Button
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = ""
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "sterge"
        KBotDataColumn12.MinWidth = 34
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.Resizable = False
        KBotDataColumn12.Width = 34
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Forma"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "forma"
        KBotDataColumn13.MinWidth = 80
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Width = 120
        gridLinii.Columns.Add(KBotDataColumn1)
        gridLinii.Columns.Add(KBotDataColumn13)
        gridLinii.Columns.Add(KBotDataColumn2)
        gridLinii.Columns.Add(KBotDataColumn3)
        gridLinii.Columns.Add(KBotDataColumn4)
        gridLinii.Columns.Add(KBotDataColumn5)
        gridLinii.Columns.Add(KBotDataColumn6)
        gridLinii.Columns.Add(KBotDataColumn7)
        gridLinii.Columns.Add(KBotDataColumn8)
        gridLinii.Columns.Add(KBotDataColumn9)
        gridLinii.Columns.Add(KBotDataColumn10)
        gridLinii.Columns.Add(KBotDataColumn11)
        gridLinii.Columns.Add(KBotDataColumn12)
        gridLinii.Dock = DockStyle.Fill
        gridLinii.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridLinii.FillColumnKey = "contd"
        gridLinii.FooterBackColor = SystemColors.Control
        gridLinii.FooterHeight = 24
        gridLinii.FooterSeparatorColor = SystemColors.ActiveBorder
        gridLinii.FooterVisible = False
        gridLinii.HeaderHeight = 24
        gridLinii.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridLinii.Location = New Point(0, 477)
        gridLinii.Margin = New Padding(0, 12, 0, 0)
        gridLinii.Name = "gridLinii"
        gridLinii.RowHeight = 24
        gridLinii.Size = New Size(960, 312)
        gridLinii.TabIndex = 10
        ' 
        ' tlySubsol
        ' 
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 2
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(lblStare, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 1, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 807)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1500, 93)
        tlySubsol.TabIndex = 1
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(8, 6)
        lblStare.Margin = New Padding(0, 0, 18, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1276, 81)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Anchor = AnchorStyles.Right
        btnSalveaza.AutoSize = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1349, 9)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(3, 9, 3, 9)
        btnSalveaza.Size = New Size(143, 74)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' SablonOperatiiView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(0)
        Name = "SablonOperatiiView"
        Size = New Size(1500, 900)
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyRight.ResumeLayout(False)
        CType(gridLinii, ComponentModel.ISupportInitialize).EndInit()
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyRight As KBotTableLayoutPanel
    Friend WithEvents lblExplicatie As Label
    Friend WithEvents txtExplicatie As KBotTextField
    Friend WithEvents lblJurnal As Label
    Friend WithEvents cmbJurnal As KBotComboBox
    Friend WithEvents lblCompartiment As Label
    Friend WithEvents cmbCompartiment As KBotComboBox
    Friend WithEvents lblFel As Label
    Friend WithEvents cmbFel As KBotComboBox
    Friend WithEvents lblFelOp As Label
    Friend WithEvents cmbFelOp As KBotComboBox
    Friend WithEvents gridLinii As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
End Class
