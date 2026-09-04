<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DataViewPlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    ' Playground KBotDataView: grila testată (Fill), un panou stânga cu TOATE comutatoarele
    ' de proprietăți runtime, butoanele de temă (sus) și verdictul uman (jos). Regula casei:
    ' toate controalele WinForms se declară aici, în .Designer.vb.

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label

    Friend WithEvents grid As KBot.Controls.KBotDataView

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    Friend WithEvents flowLeft As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblSecGrid As System.Windows.Forms.Label
    Friend WithEvents lblAutoSize As System.Windows.Forms.Label
    Friend WithEvents cboAutoSize As System.Windows.Forms.ComboBox
    Friend WithEvents lblFill As System.Windows.Forms.Label
    Friend WithEvents cboFill As System.Windows.Forms.ComboBox
    Friend WithEvents lblSample As System.Windows.Forms.Label
    Friend WithEvents numSample As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblFrozen As System.Windows.Forms.Label
    Friend WithEvents numFrozen As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblRowH As System.Windows.Forms.Label
    Friend WithEvents numRowH As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblHeaderH As System.Windows.Forms.Label
    Friend WithEvents numHeaderH As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkHeader As System.Windows.Forms.CheckBox
    Friend WithEvents chkAlt As System.Windows.Forms.CheckBox
    Friend WithEvents chkReadOnly As System.Windows.Forms.CheckBox
    Friend WithEvents btnClearFilters As System.Windows.Forms.Button
    Friend WithEvents chkColFilterable As System.Windows.Forms.CheckBox
    Friend WithEvents btnAutoSize As System.Windows.Forms.Button
    Friend WithEvents btnReset As System.Windows.Forms.Button
    Friend WithEvents lblSecCol As System.Windows.Forms.Label
    Friend WithEvents cboColumn As System.Windows.Forms.ComboBox
    Friend WithEvents chkColVisible As System.Windows.Forms.CheckBox
    Friend WithEvents chkColEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents chkColReadOnly As System.Windows.Forms.CheckBox
    Friend WithEvents chkColAutoHide As System.Windows.Forms.CheckBox
    Friend WithEvents lblColAutoSize As System.Windows.Forms.Label
    Friend WithEvents cboColAutoSize As System.Windows.Forms.ComboBox
    Friend WithEvents lblColWidth As System.Windows.Forms.Label
    Friend WithEvents numColWidth As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblColMin As System.Windows.Forms.Label
    Friend WithEvents numColMin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblColMax As System.Windows.Forms.Label
    Friend WithEvents numColMax As System.Windows.Forms.NumericUpDown
    ' —— Grupare (slice 0029) ——
    Friend WithEvents lblSecGroup As System.Windows.Forms.Label
    Friend WithEvents lblGroup1 As System.Windows.Forms.Label
    Friend WithEvents cboGroup1 As System.Windows.Forms.ComboBox
    Friend WithEvents lblGroup2 As System.Windows.Forms.Label
    Friend WithEvents cboGroup2 As System.Windows.Forms.ComboBox
    Friend WithEvents chkFooter As System.Windows.Forms.CheckBox
    Friend WithEvents chkGroupHeaderAgg As System.Windows.Forms.CheckBox
    Friend WithEvents chkGroupFooterAgg As System.Windows.Forms.CheckBox
    Friend WithEvents chkGroupCollapsed As System.Windows.Forms.CheckBox
    Friend WithEvents btnCollapseAll As System.Windows.Forms.Button
    Friend WithEvents btnExpandAll As System.Windows.Forms.Button

    Friend WithEvents lblSecData As System.Windows.Forms.Label
    Friend WithEvents lblRowCount As System.Windows.Forms.Label
    Friend WithEvents cboRowCount As System.Windows.Forms.ComboBox

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlTop = New FlowLayoutPanel()
        btnClassic = New Button()
        btnDark = New Button()
        btnModern = New Button()
        lblInfo = New Label()
        grid = New Controls.KBotDataView()
        pnlButtons = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        flowLeft = New FlowLayoutPanel()
        lblSecGrid = New Label()
        lblAutoSize = New Label()
        cboAutoSize = New ComboBox()
        lblFill = New Label()
        cboFill = New ComboBox()
        lblSample = New Label()
        numSample = New NumericUpDown()
        lblFrozen = New Label()
        numFrozen = New NumericUpDown()
        lblRowH = New Label()
        numRowH = New NumericUpDown()
        lblHeaderH = New Label()
        numHeaderH = New NumericUpDown()
        chkHeader = New CheckBox()
        chkAlt = New CheckBox()
        chkReadOnly = New CheckBox()
        btnClearFilters = New Button()
        btnAutoSize = New Button()
        btnReset = New Button()
        lblSecCol = New Label()
        cboColumn = New ComboBox()
        chkColVisible = New CheckBox()
        chkColEnabled = New CheckBox()
        chkColReadOnly = New CheckBox()
        chkColAutoHide = New CheckBox()
        chkColFilterable = New CheckBox()
        lblColAutoSize = New Label()
        cboColAutoSize = New ComboBox()
        lblColWidth = New Label()
        numColWidth = New NumericUpDown()
        lblColMin = New Label()
        numColMin = New NumericUpDown()
        lblColMax = New Label()
        numColMax = New NumericUpDown()
        lblSecGroup = New Label()
        lblGroup1 = New Label()
        cboGroup1 = New ComboBox()
        lblGroup2 = New Label()
        cboGroup2 = New ComboBox()
        chkFooter = New CheckBox()
        chkGroupHeaderAgg = New CheckBox()
        chkGroupFooterAgg = New CheckBox()
        chkGroupCollapsed = New CheckBox()
        btnCollapseAll = New Button()
        btnExpandAll = New Button()
        lblSecData = New Label()
        lblRowCount = New Label()
        cboRowCount = New ComboBox()
        pnlTop.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlButtons.SuspendLayout()
        flowLeft.SuspendLayout()
        CType(numSample, ComponentModel.ISupportInitialize).BeginInit()
        CType(numFrozen, ComponentModel.ISupportInitialize).BeginInit()
        CType(numRowH, ComponentModel.ISupportInitialize).BeginInit()
        CType(numHeaderH, ComponentModel.ISupportInitialize).BeginInit()
        CType(numColWidth, ComponentModel.ISupportInitialize).BeginInit()
        CType(numColMin, ComponentModel.ISupportInitialize).BeginInit()
        CType(numColMax, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlTop
        ' 
        pnlTop.AutoSize = True
        pnlTop.Controls.Add(btnClassic)
        pnlTop.Controls.Add(btnDark)
        pnlTop.Controls.Add(btnModern)
        pnlTop.Controls.Add(lblInfo)
        pnlTop.Dock = DockStyle.Top
        pnlTop.Location = New Point(0, 0)
        pnlTop.Name = "pnlTop"
        pnlTop.Padding = New Padding(6)
        pnlTop.Size = New Size(1180, 53)
        pnlTop.TabIndex = 3
        ' 
        ' btnClassic
        ' 
        btnClassic.AutoSize = True
        btnClassic.Location = New Point(9, 9)
        btnClassic.Name = "btnClassic"
        btnClassic.Size = New Size(75, 35)
        btnClassic.TabIndex = 0
        btnClassic.Text = "Classic"
        btnClassic.UseVisualStyleBackColor = True
        ' 
        ' btnDark
        ' 
        btnDark.AutoSize = True
        btnDark.Location = New Point(90, 9)
        btnDark.Name = "btnDark"
        btnDark.Size = New Size(75, 35)
        btnDark.TabIndex = 1
        btnDark.Text = "Dark"
        btnDark.UseVisualStyleBackColor = True
        ' 
        ' btnModern
        ' 
        btnModern.AutoSize = True
        btnModern.Location = New Point(171, 9)
        btnModern.Name = "btnModern"
        btnModern.Size = New Size(85, 35)
        btnModern.TabIndex = 2
        btnModern.Text = "Modern"
        btnModern.UseVisualStyleBackColor = True
        ' 
        ' lblInfo
        ' 
        lblInfo.AutoSize = True
        lblInfo.Location = New Point(262, 6)
        lblInfo.Name = "lblInfo"
        lblInfo.Padding = New Padding(12, 8, 0, 0)
        lblInfo.Size = New Size(12, 33)
        lblInfo.TabIndex = 3
        ' 
        ' grid
        ' 
        grid.BackColor = SystemColors.Window
        grid.Dock = DockStyle.Fill
        grid.EnableGrouping = True
        grid.Location = New Point(343, 53)
        grid.Name = "grid"
        grid.Size = New Size(837, 594)
        grid.TabIndex = 0
        ' 
        ' pnlButtons
        ' 
        pnlButtons.AutoSize = True
        pnlButtons.Controls.Add(btnPass)
        pnlButtons.Controls.Add(btnFail)
        pnlButtons.Dock = DockStyle.Bottom
        pnlButtons.FlowDirection = FlowDirection.RightToLeft
        pnlButtons.Location = New Point(0, 647)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Padding = New Padding(6)
        pnlButtons.Size = New Size(1180, 53)
        pnlButtons.TabIndex = 2
        ' 
        ' btnPass
        ' 
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.OK
        btnPass.Location = New Point(1090, 9)
        btnPass.Name = "btnPass"
        btnPass.Size = New Size(75, 35)
        btnPass.TabIndex = 0
        btnPass.Text = "Pass"
        btnPass.UseVisualStyleBackColor = True
        ' 
        ' btnFail
        ' 
        btnFail.AutoSize = True
        btnFail.DialogResult = DialogResult.Cancel
        btnFail.Location = New Point(1009, 9)
        btnFail.Name = "btnFail"
        btnFail.Size = New Size(75, 35)
        btnFail.TabIndex = 1
        btnFail.Text = "Fail"
        btnFail.UseVisualStyleBackColor = True
        ' 
        ' flowLeft
        ' 
        flowLeft.AutoScroll = True
        flowLeft.Controls.Add(lblSecGrid)
        flowLeft.Controls.Add(lblAutoSize)
        flowLeft.Controls.Add(cboAutoSize)
        flowLeft.Controls.Add(lblFill)
        flowLeft.Controls.Add(cboFill)
        flowLeft.Controls.Add(lblSample)
        flowLeft.Controls.Add(numSample)
        flowLeft.Controls.Add(lblFrozen)
        flowLeft.Controls.Add(numFrozen)
        flowLeft.Controls.Add(lblRowH)
        flowLeft.Controls.Add(numRowH)
        flowLeft.Controls.Add(lblHeaderH)
        flowLeft.Controls.Add(numHeaderH)
        flowLeft.Controls.Add(chkHeader)
        flowLeft.Controls.Add(chkAlt)
        flowLeft.Controls.Add(chkReadOnly)
        flowLeft.Controls.Add(btnClearFilters)
        flowLeft.Controls.Add(btnAutoSize)
        flowLeft.Controls.Add(btnReset)
        flowLeft.Controls.Add(lblSecCol)
        flowLeft.Controls.Add(cboColumn)
        flowLeft.Controls.Add(chkColVisible)
        flowLeft.Controls.Add(chkColEnabled)
        flowLeft.Controls.Add(chkColReadOnly)
        flowLeft.Controls.Add(chkColAutoHide)
        flowLeft.Controls.Add(chkColFilterable)
        flowLeft.Controls.Add(lblColAutoSize)
        flowLeft.Controls.Add(cboColAutoSize)
        flowLeft.Controls.Add(lblColWidth)
        flowLeft.Controls.Add(numColWidth)
        flowLeft.Controls.Add(lblColMin)
        flowLeft.Controls.Add(numColMin)
        flowLeft.Controls.Add(lblColMax)
        flowLeft.Controls.Add(numColMax)
        flowLeft.Controls.Add(lblSecGroup)
        flowLeft.Controls.Add(lblGroup1)
        flowLeft.Controls.Add(cboGroup1)
        flowLeft.Controls.Add(lblGroup2)
        flowLeft.Controls.Add(cboGroup2)
        flowLeft.Controls.Add(chkFooter)
        flowLeft.Controls.Add(chkGroupHeaderAgg)
        flowLeft.Controls.Add(chkGroupFooterAgg)
        flowLeft.Controls.Add(chkGroupCollapsed)
        flowLeft.Controls.Add(btnCollapseAll)
        flowLeft.Controls.Add(btnExpandAll)
        flowLeft.Controls.Add(lblSecData)
        flowLeft.Controls.Add(lblRowCount)
        flowLeft.Controls.Add(cboRowCount)
        flowLeft.Dock = DockStyle.Left
        flowLeft.FlowDirection = FlowDirection.TopDown
        flowLeft.Location = New Point(0, 53)
        flowLeft.Name = "flowLeft"
        flowLeft.Padding = New Padding(8)
        flowLeft.Size = New Size(343, 594)
        flowLeft.TabIndex = 1
        flowLeft.WrapContents = False
        ' 
        ' lblSecGrid
        ' 
        lblSecGrid.AutoSize = True
        lblSecGrid.Location = New Point(11, 8)
        lblSecGrid.Name = "lblSecGrid"
        lblSecGrid.Size = New Size(129, 25)
        lblSecGrid.TabIndex = 0
        lblSecGrid.Text = "—— Grilă ——"
        ' 
        ' lblAutoSize
        ' 
        lblAutoSize.AutoSize = True
        lblAutoSize.Location = New Point(11, 33)
        lblAutoSize.Name = "lblAutoSize"
        lblAutoSize.Size = New Size(199, 25)
        lblAutoSize.TabIndex = 1
        lblAutoSize.Text = "AutoSizeColumnsMode"
        ' 
        ' cboAutoSize
        ' 
        cboAutoSize.DropDownStyle = ComboBoxStyle.DropDownList
        cboAutoSize.Location = New Point(11, 61)
        cboAutoSize.Name = "cboAutoSize"
        cboAutoSize.Size = New Size(250, 33)
        cboAutoSize.TabIndex = 2
        ' 
        ' lblFill
        ' 
        lblFill.AutoSize = True
        lblFill.Location = New Point(11, 97)
        lblFill.Name = "lblFill"
        lblFill.Size = New Size(142, 25)
        lblFill.TabIndex = 3
        lblFill.Text = "ColumnFillMode"
        ' 
        ' cboFill
        ' 
        cboFill.DropDownStyle = ComboBoxStyle.DropDownList
        cboFill.Location = New Point(11, 125)
        cboFill.Name = "cboFill"
        cboFill.Size = New Size(250, 33)
        cboFill.TabIndex = 4
        ' 
        ' lblSample
        ' 
        lblSample.AutoSize = True
        lblSample.Location = New Point(11, 161)
        lblSample.Name = "lblSample"
        lblSample.Size = New Size(271, 25)
        lblSample.TabIndex = 5
        lblSample.Text = "AutoSizeSampleRows (0 = toate)"
        ' 
        ' numSample
        ' 
        numSample.Location = New Point(11, 189)
        numSample.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        numSample.Name = "numSample"
        numSample.Size = New Size(120, 31)
        numSample.TabIndex = 6
        numSample.Value = New Decimal(New Integer() {200, 0, 0, 0})
        ' 
        ' lblFrozen
        ' 
        lblFrozen.AutoSize = True
        lblFrozen.Location = New Point(11, 223)
        lblFrozen.Name = "lblFrozen"
        lblFrozen.Size = New Size(175, 25)
        lblFrozen.TabIndex = 7
        lblFrozen.Text = "FrozenColumnCount"
        ' 
        ' numFrozen
        ' 
        numFrozen.Location = New Point(11, 251)
        numFrozen.Maximum = New Decimal(New Integer() {8, 0, 0, 0})
        numFrozen.Name = "numFrozen"
        numFrozen.Size = New Size(120, 31)
        numFrozen.TabIndex = 8
        numFrozen.Value = New Decimal(New Integer() {1, 0, 0, 0})
        ' 
        ' lblRowH
        ' 
        lblRowH.AutoSize = True
        lblRowH.Location = New Point(11, 285)
        lblRowH.Name = "lblRowH"
        lblRowH.Size = New Size(99, 25)
        lblRowH.TabIndex = 9
        lblRowH.Text = "RowHeight"
        ' 
        ' numRowH
        ' 
        numRowH.Location = New Point(11, 313)
        numRowH.Maximum = New Decimal(New Integer() {80, 0, 0, 0})
        numRowH.Minimum = New Decimal(New Integer() {12, 0, 0, 0})
        numRowH.Name = "numRowH"
        numRowH.Size = New Size(120, 31)
        numRowH.TabIndex = 10
        numRowH.Value = New Decimal(New Integer() {28, 0, 0, 0})
        ' 
        ' lblHeaderH
        ' 
        lblHeaderH.AutoSize = True
        lblHeaderH.Location = New Point(11, 347)
        lblHeaderH.Name = "lblHeaderH"
        lblHeaderH.Size = New Size(122, 25)
        lblHeaderH.TabIndex = 11
        lblHeaderH.Text = "HeaderHeight"
        ' 
        ' numHeaderH
        ' 
        numHeaderH.Location = New Point(11, 375)
        numHeaderH.Maximum = New Decimal(New Integer() {80, 0, 0, 0})
        numHeaderH.Name = "numHeaderH"
        numHeaderH.Size = New Size(120, 31)
        numHeaderH.TabIndex = 12
        numHeaderH.Value = New Decimal(New Integer() {30, 0, 0, 0})
        ' 
        ' chkHeader
        ' 
        chkHeader.AutoSize = True
        chkHeader.Checked = True
        chkHeader.CheckState = CheckState.Checked
        chkHeader.Location = New Point(11, 412)
        chkHeader.Name = "chkHeader"
        chkHeader.Size = New Size(139, 29)
        chkHeader.TabIndex = 13
        chkHeader.Text = "ShowHeader"
        ' 
        ' chkAlt
        ' 
        chkAlt.AutoSize = True
        chkAlt.Checked = True
        chkAlt.CheckState = CheckState.Checked
        chkAlt.Location = New Point(11, 447)
        chkAlt.Name = "chkAlt"
        chkAlt.Size = New Size(167, 29)
        chkAlt.TabIndex = 14
        chkAlt.Text = "AlternatingRows"
        ' 
        ' chkReadOnly
        ' 
        chkReadOnly.AutoSize = True
        chkReadOnly.Location = New Point(11, 482)
        chkReadOnly.Name = "chkReadOnly"
        chkReadOnly.Size = New Size(147, 29)
        chkReadOnly.TabIndex = 15
        chkReadOnly.Text = "ReadOnlyGrid"
        ' 
        ' btnClearFilters
        ' 
        btnClearFilters.AutoSize = True
        btnClearFilters.Location = New Point(11, 517)
        btnClearFilters.Name = "btnClearFilters"
        btnClearFilters.Size = New Size(137, 35)
        btnClearFilters.TabIndex = 16
        btnClearFilters.Text = "ClearAllFilters()"
        btnClearFilters.UseVisualStyleBackColor = True
        ' 
        ' btnAutoSize
        ' 
        btnAutoSize.AutoSize = True
        btnAutoSize.Location = New Point(11, 558)
        btnAutoSize.Name = "btnAutoSize"
        btnAutoSize.Size = New Size(172, 35)
        btnAutoSize.TabIndex = 17
        btnAutoSize.Text = "AutoSizeColumns()"
        btnAutoSize.UseVisualStyleBackColor = True
        ' 
        ' btnReset
        ' 
        btnReset.AutoSize = True
        btnReset.Location = New Point(11, 599)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(183, 35)
        btnReset.TabIndex = 18
        btnReset.Text = "ResetColumnSizing()"
        btnReset.UseVisualStyleBackColor = True
        ' 
        ' lblSecCol
        ' 
        lblSecCol.AutoSize = True
        lblSecCol.Location = New Point(11, 649)
        lblSecCol.Margin = New Padding(3, 12, 3, 0)
        lblSecCol.Name = "lblSecCol"
        lblSecCol.Size = New Size(247, 25)
        lblSecCol.TabIndex = 19
        lblSecCol.Text = "—— Coloană (inspector) ——"
        ' 
        ' cboColumn
        ' 
        cboColumn.DropDownStyle = ComboBoxStyle.DropDownList
        cboColumn.Location = New Point(11, 677)
        cboColumn.Name = "cboColumn"
        cboColumn.Size = New Size(250, 33)
        cboColumn.TabIndex = 20
        ' 
        ' chkColVisible
        ' 
        chkColVisible.AutoSize = True
        chkColVisible.Location = New Point(11, 716)
        chkColVisible.Name = "chkColVisible"
        chkColVisible.Size = New Size(89, 29)
        chkColVisible.TabIndex = 21
        chkColVisible.Text = "Visible"
        ' 
        ' chkColEnabled
        ' 
        chkColEnabled.AutoSize = True
        chkColEnabled.Location = New Point(11, 751)
        chkColEnabled.Name = "chkColEnabled"
        chkColEnabled.Size = New Size(101, 29)
        chkColEnabled.TabIndex = 22
        chkColEnabled.Text = "Enabled"
        ' 
        ' chkColReadOnly
        ' 
        chkColReadOnly.AutoSize = True
        chkColReadOnly.Location = New Point(11, 786)
        chkColReadOnly.Name = "chkColReadOnly"
        chkColReadOnly.Size = New Size(114, 29)
        chkColReadOnly.TabIndex = 23
        chkColReadOnly.Text = "ReadOnly"
        ' 
        ' chkColAutoHide
        ' 
        chkColAutoHide.AutoSize = True
        chkColAutoHide.Location = New Point(11, 821)
        chkColAutoHide.Name = "chkColAutoHide"
        chkColAutoHide.Size = New Size(311, 29)
        chkColAutoHide.TabIndex = 24
        chkColAutoHide.Text = "AutoHide (dispare când nu încape)"
        ' 
        ' chkColFilterable
        ' 
        chkColFilterable.AutoSize = True
        chkColFilterable.Location = New Point(11, 856)
        chkColFilterable.Name = "chkColFilterable"
        chkColFilterable.Size = New Size(345, 29)
        chkColFilterable.TabIndex = 25
        chkColFilterable.Text = "ShowColumnFilter (meniu ca în Access)"
        ' 
        ' lblColAutoSize
        ' 
        lblColAutoSize.AutoSize = True
        lblColAutoSize.Location = New Point(11, 888)
        lblColAutoSize.Name = "lblColAutoSize"
        lblColAutoSize.Size = New Size(279, 25)
        lblColAutoSize.TabIndex = 26
        lblColAutoSize.Text = "AutoSizeMode (bate modul grilei)"
        ' 
        ' cboColAutoSize
        ' 
        cboColAutoSize.DropDownStyle = ComboBoxStyle.DropDownList
        cboColAutoSize.Location = New Point(11, 916)
        cboColAutoSize.Name = "cboColAutoSize"
        cboColAutoSize.Size = New Size(250, 33)
        cboColAutoSize.TabIndex = 27
        ' 
        ' lblColWidth
        ' 
        lblColWidth.AutoSize = True
        lblColWidth.Location = New Point(11, 952)
        lblColWidth.Name = "lblColWidth"
        lblColWidth.Size = New Size(60, 25)
        lblColWidth.TabIndex = 28
        lblColWidth.Text = "Width"
        ' 
        ' numColWidth
        ' 
        numColWidth.Location = New Point(11, 980)
        numColWidth.Maximum = New Decimal(New Integer() {4000, 0, 0, 0})
        numColWidth.Name = "numColWidth"
        numColWidth.Size = New Size(120, 31)
        numColWidth.TabIndex = 29
        numColWidth.Value = New Decimal(New Integer() {100, 0, 0, 0})
        ' 
        ' lblColMin
        ' 
        lblColMin.AutoSize = True
        lblColMin.Location = New Point(11, 1014)
        lblColMin.Name = "lblColMin"
        lblColMin.Size = New Size(90, 25)
        lblColMin.TabIndex = 30
        lblColMin.Text = "MinWidth"
        ' 
        ' numColMin
        ' 
        numColMin.Location = New Point(11, 1042)
        numColMin.Maximum = New Decimal(New Integer() {4000, 0, 0, 0})
        numColMin.Name = "numColMin"
        numColMin.Size = New Size(120, 31)
        numColMin.TabIndex = 31
        numColMin.Value = New Decimal(New Integer() {40, 0, 0, 0})
        ' 
        ' lblColMax
        ' 
        lblColMax.AutoSize = True
        lblColMax.Location = New Point(11, 1076)
        lblColMax.Name = "lblColMax"
        lblColMax.Size = New Size(225, 25)
        lblColMax.TabIndex = 32
        lblColMax.Text = "MaxWidth (0 = neplafonat)"
        ' 
        ' numColMax
        ' 
        numColMax.Location = New Point(11, 1104)
        numColMax.Maximum = New Decimal(New Integer() {4000, 0, 0, 0})
        numColMax.Name = "numColMax"
        numColMax.Size = New Size(120, 31)
        numColMax.TabIndex = 33
        ' 
        ' lblSecGroup
        ' 
        lblSecGroup.AutoSize = True
        lblSecGroup.Location = New Point(11, 1150)
        lblSecGroup.Margin = New Padding(3, 12, 3, 0)
        lblSecGroup.Name = "lblSecGroup"
        lblSecGroup.Size = New Size(333, 25)
        lblSecGroup.TabIndex = 34
        lblSecGroup.Text = "—— Grupare (ca la raportul Access) ——"
        ' 
        ' lblGroup1
        ' 
        lblGroup1.AutoSize = True
        lblGroup1.Location = New Point(11, 1175)
        lblGroup1.Name = "lblGroup1"
        lblGroup1.Size = New Size(159, 25)
        lblGroup1.TabIndex = 35
        lblGroup1.Text = "Nivelul 1 (dinafară)"
        ' 
        ' cboGroup1
        ' 
        cboGroup1.DropDownStyle = ComboBoxStyle.DropDownList
        cboGroup1.Location = New Point(11, 1203)
        cboGroup1.Name = "cboGroup1"
        cboGroup1.Size = New Size(250, 33)
        cboGroup1.TabIndex = 36
        ' 
        ' lblGroup2
        ' 
        lblGroup2.AutoSize = True
        lblGroup2.Location = New Point(11, 1239)
        lblGroup2.Name = "lblGroup2"
        lblGroup2.Size = New Size(171, 25)
        lblGroup2.TabIndex = 37
        lblGroup2.Text = "Nivelul 2 (dinăuntru)"
        ' 
        ' cboGroup2
        ' 
        cboGroup2.DropDownStyle = ComboBoxStyle.DropDownList
        cboGroup2.Location = New Point(11, 1267)
        cboGroup2.Name = "cboGroup2"
        cboGroup2.Size = New Size(250, 33)
        cboGroup2.TabIndex = 38
        ' 
        ' chkFooter
        ' 
        chkFooter.AutoSize = True
        chkFooter.Checked = True
        chkFooter.CheckState = CheckState.Checked
        chkFooter.Location = New Point(11, 1306)
        chkFooter.Name = "chkFooter"
        chkFooter.Size = New Size(269, 29)
        chkFooter.TabIndex = 39
        chkFooter.Text = "FooterVisible (totalul general)"
        ' 
        ' chkGroupHeaderAgg
        ' 
        chkGroupHeaderAgg.AutoSize = True
        chkGroupHeaderAgg.Location = New Point(11, 1341)
        chkGroupHeaderAgg.Name = "chkGroupHeaderAgg"
        chkGroupHeaderAgg.Size = New Size(295, 29)
        chkGroupHeaderAgg.TabIndex = 40
        chkGroupHeaderAgg.Text = "Agregate și în ANTETUL grupului"
        ' 
        ' chkGroupFooterAgg
        ' 
        chkGroupFooterAgg.AutoSize = True
        chkGroupFooterAgg.Checked = True
        chkGroupFooterAgg.CheckState = CheckState.Checked
        chkGroupFooterAgg.Location = New Point(11, 1376)
        chkGroupFooterAgg.Name = "chkGroupFooterAgg"
        chkGroupFooterAgg.Size = New Size(290, 29)
        chkGroupFooterAgg.TabIndex = 41
        chkGroupFooterAgg.Text = "Agregate în SUBSOLUL grupului"
        ' 
        ' chkGroupCollapsed
        ' 
        chkGroupCollapsed.AutoSize = True
        chkGroupCollapsed.Location = New Point(11, 1411)
        chkGroupCollapsed.Name = "chkGroupCollapsed"
        chkGroupCollapsed.Size = New Size(192, 29)
        chkGroupCollapsed.TabIndex = 42
        chkGroupCollapsed.Text = "CollapsedByDefault"
        ' 
        ' btnCollapseAll
        ' 
        btnCollapseAll.AutoSize = True
        btnCollapseAll.Location = New Point(11, 1446)
        btnCollapseAll.Name = "btnCollapseAll"
        btnCollapseAll.Size = New Size(177, 35)
        btnCollapseAll.TabIndex = 43
        btnCollapseAll.Text = "CollapseAllGroups()"
        btnCollapseAll.UseVisualStyleBackColor = True
        ' 
        ' btnExpandAll
        ' 
        btnExpandAll.AutoSize = True
        btnExpandAll.Location = New Point(11, 1487)
        btnExpandAll.Name = "btnExpandAll"
        btnExpandAll.Size = New Size(168, 35)
        btnExpandAll.TabIndex = 44
        btnExpandAll.Text = "ExpandAllGroups()"
        btnExpandAll.UseVisualStyleBackColor = True
        ' 
        ' lblSecData
        ' 
        lblSecData.AutoSize = True
        lblSecData.Location = New Point(11, 1537)
        lblSecData.Margin = New Padding(3, 12, 3, 0)
        lblSecData.Name = "lblSecData"
        lblSecData.Size = New Size(131, 25)
        lblSecData.TabIndex = 45
        lblSecData.Text = "—— Date ——"
        ' 
        ' lblRowCount
        ' 
        lblRowCount.AutoSize = True
        lblRowCount.Location = New Point(11, 1562)
        lblRowCount.Name = "lblRowCount"
        lblRowCount.Size = New Size(73, 25)
        lblRowCount.TabIndex = 46
        lblRowCount.Text = "Rânduri"
        ' 
        ' cboRowCount
        ' 
        cboRowCount.DropDownStyle = ComboBoxStyle.DropDownList
        cboRowCount.Location = New Point(11, 1590)
        cboRowCount.Name = "cboRowCount"
        cboRowCount.Size = New Size(250, 33)
        cboRowCount.TabIndex = 47
        ' 
        ' DataViewPlaygroundForm
        ' 
        AcceptButton = btnPass
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnFail
        ClientSize = New Size(1180, 700)
        Controls.Add(grid)
        Controls.Add(flowLeft)
        Controls.Add(pnlButtons)
        Controls.Add(pnlTop)
        Name = "DataViewPlaygroundForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "KBotDataView — playground proprietăți runtime"
        pnlTop.ResumeLayout(False)
        pnlTop.PerformLayout()
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlButtons.ResumeLayout(False)
        pnlButtons.PerformLayout()
        flowLeft.ResumeLayout(False)
        flowLeft.PerformLayout()
        CType(numSample, ComponentModel.ISupportInitialize).EndInit()
        CType(numFrozen, ComponentModel.ISupportInitialize).EndInit()
        CType(numRowH, ComponentModel.ISupportInitialize).EndInit()
        CType(numHeaderH, ComponentModel.ISupportInitialize).EndInit()
        CType(numColWidth, ComponentModel.ISupportInitialize).EndInit()
        CType(numColMin, ComponentModel.ISupportInitialize).EndInit()
        CType(numColMax, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
End Class
