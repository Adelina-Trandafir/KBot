<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TreePlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    ' Playground AdvancedTreeControl: arborele testat (Fill), un panou stânga cu TOATE
    ' comutatoarele de proprietăți runtime (antet / căutare / arbore / tooltip / date),
    ' butoanele de temă (sus) și verdictul uman (jos). Regula casei: toate controalele
    ' WinForms se declară aici, în .Designer.vb.

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label

    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    Friend WithEvents flowLeft As System.Windows.Forms.FlowLayoutPanel

    ' —— Antet ——
    Friend WithEvents lblSecHeader As System.Windows.Forms.Label
    Friend WithEvents chkHeaderVisible As System.Windows.Forms.CheckBox
    Friend WithEvents lblHeaderCaption As System.Windows.Forms.Label
    Friend WithEvents txtHeaderCaption As System.Windows.Forms.TextBox
    Friend WithEvents lblHeaderHeight As System.Windows.Forms.Label
    Friend WithEvents numHeaderHeight As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnHeaderFont As System.Windows.Forms.Button
    Friend WithEvents lblHeaderAlign As System.Windows.Forms.Label
    Friend WithEvents cboHeaderAlign As System.Windows.Forms.ComboBox
    Friend WithEvents lblHeaderStyle As System.Windows.Forms.Label
    Friend WithEvents cboHeaderStyle As System.Windows.Forms.ComboBox
    Friend WithEvents btnHeaderBack As System.Windows.Forms.Button
    Friend WithEvents btnHeaderFore As System.Windows.Forms.Button
    Friend WithEvents btnHeaderGradEnd As System.Windows.Forms.Button
    Friend WithEvents chkHeaderLeftIcon As System.Windows.Forms.CheckBox
    Friend WithEvents chkHeaderSearchIcon As System.Windows.Forms.CheckBox
    Friend WithEvents chkHeaderRightIcon As System.Windows.Forms.CheckBox

    ' —— Căutare ——
    Friend WithEvents lblSecSearch As System.Windows.Forms.Label
    Friend WithEvents chkSearchShow As System.Windows.Forms.CheckBox
    Friend WithEvents chkSearchClear As System.Windows.Forms.CheckBox
    Friend WithEvents lblSearchLabel As System.Windows.Forms.Label
    Friend WithEvents txtSearchLabel As System.Windows.Forms.TextBox
    Friend WithEvents btnLabelFont As System.Windows.Forms.Button
    Friend WithEvents btnSearchFont As System.Windows.Forms.Button
    Friend WithEvents lblPlaceholder As System.Windows.Forms.Label
    Friend WithEvents txtPlaceholder As System.Windows.Forms.TextBox
    Friend WithEvents lblClearPad As System.Windows.Forms.Label
    Friend WithEvents numClearPad As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkClearImage As System.Windows.Forms.CheckBox
    Friend WithEvents lblSearchIn As System.Windows.Forms.Label
    Friend WithEvents cboSearchIn As System.Windows.Forms.ComboBox
    Friend WithEvents lblSearchType As System.Windows.Forms.Label
    Friend WithEvents cboSearchType As System.Windows.Forms.ComboBox
    Friend WithEvents btnSearchBack As System.Windows.Forms.Button
    Friend WithEvents btnSearchBox As System.Windows.Forms.Button

    ' —— Arbore ——
    Friend WithEvents lblSecTree As System.Windows.Forms.Label
    Friend WithEvents lblItemHeight As System.Windows.Forms.Label
    Friend WithEvents numItemHeight As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblIndent As System.Windows.Forms.Label
    Friend WithEvents numIndent As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblExpander As System.Windows.Forms.Label
    Friend WithEvents numExpander As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblCheckSize As System.Windows.Forms.Label
    Friend WithEvents numCheckSize As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkCheckBoxes As System.Windows.Forms.CheckBox
    Friend WithEvents chkRootExpander As System.Windows.Forms.CheckBox
    Friend WithEvents chkNodeIcons As System.Windows.Forms.CheckBox
    Friend WithEvents lblRadioLevel As System.Windows.Forms.Label
    Friend WithEvents numRadioLevel As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkRightIconHover As System.Windows.Forms.CheckBox
    Friend WithEvents chkReserveRight As System.Windows.Forms.CheckBox
    Friend WithEvents lblRightPad As System.Windows.Forms.Label
    Friend WithEvents numRightPad As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblTreeFont As System.Windows.Forms.Label
    Friend WithEvents numTreeFont As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblScrollTheme As System.Windows.Forms.Label
    Friend WithEvents cboScrollTheme As System.Windows.Forms.ComboBox

    ' —— Tooltip ——
    Friend WithEvents lblSecTooltip As System.Windows.Forms.Label
    Friend WithEvents chkTooltip As System.Windows.Forms.CheckBox
    Friend WithEvents chkTooltipIconOnly As System.Windows.Forms.CheckBox
    Friend WithEvents lblTooltipDelay As System.Windows.Forms.Label
    Friend WithEvents numTooltipDelay As System.Windows.Forms.NumericUpDown

    ' —— Date ——
    Friend WithEvents lblSecData As System.Windows.Forms.Label
    Friend WithEvents lblNodeCount As System.Windows.Forms.Label
    Friend WithEvents cboNodeCount As System.Windows.Forms.ComboBox
    Friend WithEvents btnCollapseAll As System.Windows.Forms.Button
    Friend WithEvents btnExpandAll As System.Windows.Forms.Button
    Friend WithEvents btnFromDefinitions As System.Windows.Forms.Button
    Friend WithEvents imgNoduri As System.Windows.Forms.ImageList

    ' —— Export ——
    Friend WithEvents lblSecExport As System.Windows.Forms.Label
    Friend WithEvents btnExport As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.tree = New KBot.Controls.AdvancedTreeControl()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.flowLeft = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblSecHeader = New System.Windows.Forms.Label()
        Me.chkHeaderVisible = New System.Windows.Forms.CheckBox()
        Me.lblHeaderCaption = New System.Windows.Forms.Label()
        Me.txtHeaderCaption = New System.Windows.Forms.TextBox()
        Me.lblHeaderHeight = New System.Windows.Forms.Label()
        Me.numHeaderHeight = New System.Windows.Forms.NumericUpDown()
        Me.btnHeaderFont = New System.Windows.Forms.Button()
        Me.lblHeaderAlign = New System.Windows.Forms.Label()
        Me.cboHeaderAlign = New System.Windows.Forms.ComboBox()
        Me.lblHeaderStyle = New System.Windows.Forms.Label()
        Me.cboHeaderStyle = New System.Windows.Forms.ComboBox()
        Me.btnHeaderBack = New System.Windows.Forms.Button()
        Me.btnHeaderFore = New System.Windows.Forms.Button()
        Me.btnHeaderGradEnd = New System.Windows.Forms.Button()
        Me.chkHeaderLeftIcon = New System.Windows.Forms.CheckBox()
        Me.chkHeaderSearchIcon = New System.Windows.Forms.CheckBox()
        Me.chkHeaderRightIcon = New System.Windows.Forms.CheckBox()
        Me.lblSecSearch = New System.Windows.Forms.Label()
        Me.chkSearchShow = New System.Windows.Forms.CheckBox()
        Me.chkSearchClear = New System.Windows.Forms.CheckBox()
        Me.lblSearchLabel = New System.Windows.Forms.Label()
        Me.txtSearchLabel = New System.Windows.Forms.TextBox()
        Me.btnLabelFont = New System.Windows.Forms.Button()
        Me.btnSearchFont = New System.Windows.Forms.Button()
        Me.lblPlaceholder = New System.Windows.Forms.Label()
        Me.txtPlaceholder = New System.Windows.Forms.TextBox()
        Me.lblClearPad = New System.Windows.Forms.Label()
        Me.numClearPad = New System.Windows.Forms.NumericUpDown()
        Me.chkClearImage = New System.Windows.Forms.CheckBox()
        Me.lblSearchIn = New System.Windows.Forms.Label()
        Me.cboSearchIn = New System.Windows.Forms.ComboBox()
        Me.lblSearchType = New System.Windows.Forms.Label()
        Me.cboSearchType = New System.Windows.Forms.ComboBox()
        Me.btnSearchBack = New System.Windows.Forms.Button()
        Me.btnSearchBox = New System.Windows.Forms.Button()
        Me.lblSecTree = New System.Windows.Forms.Label()
        Me.lblItemHeight = New System.Windows.Forms.Label()
        Me.numItemHeight = New System.Windows.Forms.NumericUpDown()
        Me.lblIndent = New System.Windows.Forms.Label()
        Me.numIndent = New System.Windows.Forms.NumericUpDown()
        Me.lblExpander = New System.Windows.Forms.Label()
        Me.numExpander = New System.Windows.Forms.NumericUpDown()
        Me.lblCheckSize = New System.Windows.Forms.Label()
        Me.numCheckSize = New System.Windows.Forms.NumericUpDown()
        Me.chkCheckBoxes = New System.Windows.Forms.CheckBox()
        Me.chkRootExpander = New System.Windows.Forms.CheckBox()
        Me.chkNodeIcons = New System.Windows.Forms.CheckBox()
        Me.lblRadioLevel = New System.Windows.Forms.Label()
        Me.numRadioLevel = New System.Windows.Forms.NumericUpDown()
        Me.chkRightIconHover = New System.Windows.Forms.CheckBox()
        Me.chkReserveRight = New System.Windows.Forms.CheckBox()
        Me.lblRightPad = New System.Windows.Forms.Label()
        Me.numRightPad = New System.Windows.Forms.NumericUpDown()
        Me.lblTreeFont = New System.Windows.Forms.Label()
        Me.numTreeFont = New System.Windows.Forms.NumericUpDown()
        Me.lblScrollTheme = New System.Windows.Forms.Label()
        Me.cboScrollTheme = New System.Windows.Forms.ComboBox()
        Me.lblSecTooltip = New System.Windows.Forms.Label()
        Me.chkTooltip = New System.Windows.Forms.CheckBox()
        Me.chkTooltipIconOnly = New System.Windows.Forms.CheckBox()
        Me.lblTooltipDelay = New System.Windows.Forms.Label()
        Me.numTooltipDelay = New System.Windows.Forms.NumericUpDown()
        Me.lblSecData = New System.Windows.Forms.Label()
        Me.lblNodeCount = New System.Windows.Forms.Label()
        Me.cboNodeCount = New System.Windows.Forms.ComboBox()
        Me.btnCollapseAll = New System.Windows.Forms.Button()
        Me.btnExpandAll = New System.Windows.Forms.Button()
        Me.btnFromDefinitions = New System.Windows.Forms.Button()
        Me.imgNoduri = New System.Windows.Forms.ImageList()
        Me.lblSecExport = New System.Windows.Forms.Label()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.pnlTop.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.flowLeft.SuspendLayout()
        CType(Me.numHeaderHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numClearPad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numItemHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numIndent, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numExpander, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numCheckSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numRadioLevel, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numRightPad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numTreeFont, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numTooltipDelay, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlTop — comutatoarele de temă + info
        '
        Me.pnlTop.AutoSize = True
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 40
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.Controls.Add(Me.btnClassic)
        Me.pnlTop.Controls.Add(Me.btnDark)
        Me.pnlTop.Controls.Add(Me.btnModern)
        Me.pnlTop.Controls.Add(Me.lblInfo)
        Me.pnlTop.Name = "pnlTop"
        '
        Me.btnClassic.AutoSize = True : Me.btnClassic.Text = "Classic" : Me.btnClassic.Name = "btnClassic" : Me.btnClassic.UseVisualStyleBackColor = True
        Me.btnDark.AutoSize = True : Me.btnDark.Text = "Dark" : Me.btnDark.Name = "btnDark" : Me.btnDark.UseVisualStyleBackColor = True
        Me.btnModern.AutoSize = True : Me.btnModern.Text = "Modern" : Me.btnModern.Name = "btnModern" : Me.btnModern.UseVisualStyleBackColor = True
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Padding = New System.Windows.Forms.Padding(12, 8, 0, 0)
        Me.lblInfo.Name = "lblInfo"
        '
        'flowLeft — panoul de proprietăți (derulabil)
        '
        Me.flowLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.flowLeft.Width = 300
        Me.flowLeft.AutoScroll = True
        Me.flowLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.flowLeft.WrapContents = False
        Me.flowLeft.Padding = New System.Windows.Forms.Padding(8)
        Me.flowLeft.Name = "flowLeft"
        Me.flowLeft.Controls.Add(Me.lblSecHeader)
        Me.flowLeft.Controls.Add(Me.chkHeaderVisible)
        Me.flowLeft.Controls.Add(Me.lblHeaderCaption)
        Me.flowLeft.Controls.Add(Me.txtHeaderCaption)
        Me.flowLeft.Controls.Add(Me.lblHeaderHeight)
        Me.flowLeft.Controls.Add(Me.numHeaderHeight)
        Me.flowLeft.Controls.Add(Me.btnHeaderFont)
        Me.flowLeft.Controls.Add(Me.lblHeaderAlign)
        Me.flowLeft.Controls.Add(Me.cboHeaderAlign)
        Me.flowLeft.Controls.Add(Me.lblHeaderStyle)
        Me.flowLeft.Controls.Add(Me.cboHeaderStyle)
        Me.flowLeft.Controls.Add(Me.btnHeaderBack)
        Me.flowLeft.Controls.Add(Me.btnHeaderFore)
        Me.flowLeft.Controls.Add(Me.btnHeaderGradEnd)
        Me.flowLeft.Controls.Add(Me.chkHeaderLeftIcon)
        Me.flowLeft.Controls.Add(Me.chkHeaderSearchIcon)
        Me.flowLeft.Controls.Add(Me.chkHeaderRightIcon)
        Me.flowLeft.Controls.Add(Me.lblSecSearch)
        Me.flowLeft.Controls.Add(Me.chkSearchShow)
        Me.flowLeft.Controls.Add(Me.chkSearchClear)
        Me.flowLeft.Controls.Add(Me.lblSearchLabel)
        Me.flowLeft.Controls.Add(Me.txtSearchLabel)
        Me.flowLeft.Controls.Add(Me.btnLabelFont)
        Me.flowLeft.Controls.Add(Me.btnSearchFont)
        Me.flowLeft.Controls.Add(Me.lblPlaceholder)
        Me.flowLeft.Controls.Add(Me.txtPlaceholder)
        Me.flowLeft.Controls.Add(Me.lblClearPad)
        Me.flowLeft.Controls.Add(Me.numClearPad)
        Me.flowLeft.Controls.Add(Me.chkClearImage)
        Me.flowLeft.Controls.Add(Me.lblSearchIn)
        Me.flowLeft.Controls.Add(Me.cboSearchIn)
        Me.flowLeft.Controls.Add(Me.lblSearchType)
        Me.flowLeft.Controls.Add(Me.cboSearchType)
        Me.flowLeft.Controls.Add(Me.btnSearchBack)
        Me.flowLeft.Controls.Add(Me.btnSearchBox)
        Me.flowLeft.Controls.Add(Me.lblSecTree)
        Me.flowLeft.Controls.Add(Me.lblItemHeight)
        Me.flowLeft.Controls.Add(Me.numItemHeight)
        Me.flowLeft.Controls.Add(Me.lblIndent)
        Me.flowLeft.Controls.Add(Me.numIndent)
        Me.flowLeft.Controls.Add(Me.lblExpander)
        Me.flowLeft.Controls.Add(Me.numExpander)
        Me.flowLeft.Controls.Add(Me.lblCheckSize)
        Me.flowLeft.Controls.Add(Me.numCheckSize)
        Me.flowLeft.Controls.Add(Me.chkCheckBoxes)
        Me.flowLeft.Controls.Add(Me.chkRootExpander)
        Me.flowLeft.Controls.Add(Me.chkNodeIcons)
        Me.flowLeft.Controls.Add(Me.lblRadioLevel)
        Me.flowLeft.Controls.Add(Me.numRadioLevel)
        Me.flowLeft.Controls.Add(Me.chkRightIconHover)
        Me.flowLeft.Controls.Add(Me.chkReserveRight)
        Me.flowLeft.Controls.Add(Me.lblRightPad)
        Me.flowLeft.Controls.Add(Me.numRightPad)
        Me.flowLeft.Controls.Add(Me.lblTreeFont)
        Me.flowLeft.Controls.Add(Me.numTreeFont)
        Me.flowLeft.Controls.Add(Me.lblScrollTheme)
        Me.flowLeft.Controls.Add(Me.cboScrollTheme)
        Me.flowLeft.Controls.Add(Me.lblSecTooltip)
        Me.flowLeft.Controls.Add(Me.chkTooltip)
        Me.flowLeft.Controls.Add(Me.chkTooltipIconOnly)
        Me.flowLeft.Controls.Add(Me.lblTooltipDelay)
        Me.flowLeft.Controls.Add(Me.numTooltipDelay)
        Me.flowLeft.Controls.Add(Me.lblSecData)
        Me.flowLeft.Controls.Add(Me.lblNodeCount)
        Me.flowLeft.Controls.Add(Me.cboNodeCount)
        Me.flowLeft.Controls.Add(Me.btnExpandAll)
        Me.flowLeft.Controls.Add(Me.btnCollapseAll)
        Me.flowLeft.Controls.Add(Me.btnFromDefinitions)
        Me.flowLeft.Controls.Add(Me.lblSecExport)
        Me.flowLeft.Controls.Add(Me.btnExport)
        '
        '—— Antet ——
        '
        Me.lblSecHeader.AutoSize = True : Me.lblSecHeader.Text = "—— Antet ——" : Me.lblSecHeader.Name = "lblSecHeader"
        Me.chkHeaderVisible.AutoSize = True : Me.chkHeaderVisible.Text = "HeaderVisible" : Me.chkHeaderVisible.Name = "chkHeaderVisible"
        Me.lblHeaderCaption.AutoSize = True : Me.lblHeaderCaption.Text = "HeaderCaption" : Me.lblHeaderCaption.Name = "lblHeaderCaption"
        Me.txtHeaderCaption.Width = 270 : Me.txtHeaderCaption.Name = "txtHeaderCaption"
        Me.lblHeaderHeight.AutoSize = True : Me.lblHeaderHeight.Text = "HeaderHeight" : Me.lblHeaderHeight.Name = "lblHeaderHeight"
        Me.numHeaderHeight.Minimum = 16D : Me.numHeaderHeight.Maximum = 120D : Me.numHeaderHeight.Value = 32D : Me.numHeaderHeight.Width = 120 : Me.numHeaderHeight.Name = "numHeaderHeight"
        Me.btnHeaderFont.AutoSize = True : Me.btnHeaderFont.Text = "HeaderFont…" : Me.btnHeaderFont.Name = "btnHeaderFont" : Me.btnHeaderFont.UseVisualStyleBackColor = True
        Me.lblHeaderAlign.AutoSize = True : Me.lblHeaderAlign.Text = "HeaderTextAlign" : Me.lblHeaderAlign.Name = "lblHeaderAlign"
        Me.cboHeaderAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboHeaderAlign.Width = 270 : Me.cboHeaderAlign.Name = "cboHeaderAlign"
        Me.lblHeaderStyle.AutoSize = True : Me.lblHeaderStyle.Text = "HeaderBackStyle" : Me.lblHeaderStyle.Name = "lblHeaderStyle"
        Me.cboHeaderStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboHeaderStyle.Width = 270 : Me.cboHeaderStyle.Name = "cboHeaderStyle"
        Me.btnHeaderBack.AutoSize = True : Me.btnHeaderBack.Text = "HeaderBackColor…" : Me.btnHeaderBack.Name = "btnHeaderBack" : Me.btnHeaderBack.UseVisualStyleBackColor = True
        Me.btnHeaderFore.AutoSize = True : Me.btnHeaderFore.Text = "HeaderForeColor…" : Me.btnHeaderFore.Name = "btnHeaderFore" : Me.btnHeaderFore.UseVisualStyleBackColor = True
        Me.btnHeaderGradEnd.AutoSize = True : Me.btnHeaderGradEnd.Text = "HeaderGradientEndColor…" : Me.btnHeaderGradEnd.Name = "btnHeaderGradEnd" : Me.btnHeaderGradEnd.UseVisualStyleBackColor = True
        Me.chkHeaderLeftIcon.AutoSize = True : Me.chkHeaderLeftIcon.Text = "Iconiță stânga în antet" : Me.chkHeaderLeftIcon.Name = "chkHeaderLeftIcon"
        Me.chkHeaderSearchIcon.AutoSize = True : Me.chkHeaderSearchIcon.Text = "Iconiță căutare în antet (toggle)" : Me.chkHeaderSearchIcon.Name = "chkHeaderSearchIcon"
        Me.chkHeaderRightIcon.AutoSize = True : Me.chkHeaderRightIcon.Text = "Iconiță dreapta în antet" : Me.chkHeaderRightIcon.Name = "chkHeaderRightIcon"
        '
        '—— Căutare ——
        '
        Me.lblSecSearch.AutoSize = True : Me.lblSecSearch.Text = "—— Căutare ——" : Me.lblSecSearch.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecSearch.Name = "lblSecSearch"
        Me.chkSearchShow.AutoSize = True : Me.chkSearchShow.Text = "SearchShow" : Me.chkSearchShow.Name = "chkSearchShow"
        Me.chkSearchClear.AutoSize = True : Me.chkSearchClear.Text = "SearchClearButton (✕)" : Me.chkSearchClear.Name = "chkSearchClear"
        Me.lblSearchLabel.AutoSize = True : Me.lblSearchLabel.Text = "SearchBarLabelText" : Me.lblSearchLabel.Name = "lblSearchLabel"
        Me.txtSearchLabel.Width = 270 : Me.txtSearchLabel.Name = "txtSearchLabel"
        Me.btnLabelFont.AutoSize = True : Me.btnLabelFont.Text = "SearchBarLabelFont…" : Me.btnLabelFont.Name = "btnLabelFont" : Me.btnLabelFont.UseVisualStyleBackColor = True
        Me.btnSearchFont.AutoSize = True : Me.btnSearchFont.Text = "SearchBarFont…" : Me.btnSearchFont.Name = "btnSearchFont" : Me.btnSearchFont.UseVisualStyleBackColor = True
        Me.lblPlaceholder.AutoSize = True : Me.lblPlaceholder.Text = "SearchDefaultText (placeholder)" : Me.lblPlaceholder.Name = "lblPlaceholder"
        Me.txtPlaceholder.Width = 270 : Me.txtPlaceholder.Name = "txtPlaceholder"
        Me.lblClearPad.AutoSize = True : Me.lblClearPad.Text = "SearchClearButtonPadding (uniform)" : Me.lblClearPad.Name = "lblClearPad"
        Me.numClearPad.Minimum = 0D : Me.numClearPad.Maximum = 24D : Me.numClearPad.Value = 2D : Me.numClearPad.Width = 120 : Me.numClearPad.Name = "numClearPad"
        Me.chkClearImage.AutoSize = True : Me.chkClearImage.Text = "Imagine pe butonul de golire" : Me.chkClearImage.Name = "chkClearImage"
        Me.lblSearchIn.AutoSize = True : Me.lblSearchIn.Text = "SearchIn" : Me.lblSearchIn.Name = "lblSearchIn"
        Me.cboSearchIn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboSearchIn.Width = 270 : Me.cboSearchIn.Name = "cboSearchIn"
        Me.lblSearchType.AutoSize = True : Me.lblSearchType.Text = "SearchType" : Me.lblSearchType.Name = "lblSearchType"
        Me.cboSearchType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboSearchType.Width = 270 : Me.cboSearchType.Name = "cboSearchType"
        Me.btnSearchBack.AutoSize = True : Me.btnSearchBack.Text = "SearchBackColor…" : Me.btnSearchBack.Name = "btnSearchBack" : Me.btnSearchBack.UseVisualStyleBackColor = True
        Me.btnSearchBox.AutoSize = True : Me.btnSearchBox.Text = "SearchBoxBackColor…" : Me.btnSearchBox.Name = "btnSearchBox" : Me.btnSearchBox.UseVisualStyleBackColor = True
        '
        '—— Arbore ——
        '
        Me.lblSecTree.AutoSize = True : Me.lblSecTree.Text = "—— Arbore ——" : Me.lblSecTree.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecTree.Name = "lblSecTree"
        Me.lblItemHeight.AutoSize = True : Me.lblItemHeight.Text = "ItemHeight" : Me.lblItemHeight.Name = "lblItemHeight"
        Me.numItemHeight.Minimum = 12D : Me.numItemHeight.Maximum = 80D : Me.numItemHeight.Value = 22D : Me.numItemHeight.Width = 120 : Me.numItemHeight.Name = "numItemHeight"
        Me.lblIndent.AutoSize = True : Me.lblIndent.Text = "Indent" : Me.lblIndent.Name = "lblIndent"
        Me.numIndent.Minimum = 0D : Me.numIndent.Maximum = 60D : Me.numIndent.Value = 10D : Me.numIndent.Width = 120 : Me.numIndent.Name = "numIndent"
        Me.lblExpander.AutoSize = True : Me.lblExpander.Text = "ExpanderSize" : Me.lblExpander.Name = "lblExpander"
        Me.numExpander.Minimum = 6D : Me.numExpander.Maximum = 32D : Me.numExpander.Value = 12D : Me.numExpander.Width = 120 : Me.numExpander.Name = "numExpander"
        Me.lblCheckSize.AutoSize = True : Me.lblCheckSize.Text = "CheckBoxSize" : Me.lblCheckSize.Name = "lblCheckSize"
        Me.numCheckSize.Minimum = 8D : Me.numCheckSize.Maximum = 32D : Me.numCheckSize.Value = 16D : Me.numCheckSize.Width = 120 : Me.numCheckSize.Name = "numCheckSize"
        Me.chkCheckBoxes.AutoSize = True : Me.chkCheckBoxes.Text = "CheckBoxes" : Me.chkCheckBoxes.Name = "chkCheckBoxes"
        Me.chkRootExpander.AutoSize = True : Me.chkRootExpander.Text = "RootExpander" : Me.chkRootExpander.Name = "chkRootExpander"
        Me.chkNodeIcons.AutoSize = True : Me.chkNodeIcons.Text = "HasNodeIcons" : Me.chkNodeIcons.Name = "chkNodeIcons"
        Me.lblRadioLevel.AutoSize = True : Me.lblRadioLevel.Text = "RadioButtonLevel (-1 = dezactivat)" : Me.lblRadioLevel.Name = "lblRadioLevel"
        Me.numRadioLevel.Minimum = -1D : Me.numRadioLevel.Maximum = 5D : Me.numRadioLevel.Value = -1D : Me.numRadioLevel.Width = 120 : Me.numRadioLevel.Name = "numRadioLevel"
        Me.chkRightIconHover.AutoSize = True : Me.chkRightIconHover.Text = "ShowRightIconOnHover" : Me.chkRightIconHover.Name = "chkRightIconHover"
        Me.chkReserveRight.AutoSize = True : Me.chkReserveRight.Text = "ReserveRightIconSpace" : Me.chkReserveRight.Name = "chkReserveRight"
        Me.lblRightPad.AutoSize = True : Me.lblRightPad.Text = "RightIconRightPadding" : Me.lblRightPad.Name = "lblRightPad"
        Me.numRightPad.Minimum = 0D : Me.numRightPad.Maximum = 40D : Me.numRightPad.Value = 6D : Me.numRightPad.Width = 120 : Me.numRightPad.Name = "numRightPad"
        Me.lblTreeFont.AutoSize = True : Me.lblTreeFont.Text = "TreeFont — dimensiune" : Me.lblTreeFont.Name = "lblTreeFont"
        Me.numTreeFont.Minimum = 6D : Me.numTreeFont.Maximum = 24D : Me.numTreeFont.Value = 9D : Me.numTreeFont.Width = 120 : Me.numTreeFont.Name = "numTreeFont"
        Me.lblScrollTheme.AutoSize = True : Me.lblScrollTheme.Text = "ScrollBarTheme" : Me.lblScrollTheme.Name = "lblScrollTheme"
        Me.cboScrollTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboScrollTheme.Width = 270 : Me.cboScrollTheme.Name = "cboScrollTheme"
        '
        '—— Tooltip ——
        '
        Me.lblSecTooltip.AutoSize = True : Me.lblSecTooltip.Text = "—— Tooltip ——" : Me.lblSecTooltip.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecTooltip.Name = "lblSecTooltip"
        Me.chkTooltip.AutoSize = True : Me.chkTooltip.Text = "TooltipShow" : Me.chkTooltip.Checked = True : Me.chkTooltip.Name = "chkTooltip"
        Me.chkTooltipIconOnly.AutoSize = True : Me.chkTooltipIconOnly.Text = "TooltipShowOnlyOnLeftIcon" : Me.chkTooltipIconOnly.Name = "chkTooltipIconOnly"
        Me.lblTooltipDelay.AutoSize = True : Me.lblTooltipDelay.Text = "TooltipDelayMs" : Me.lblTooltipDelay.Name = "lblTooltipDelay"
        Me.numTooltipDelay.Minimum = 0D : Me.numTooltipDelay.Maximum = 5000D : Me.numTooltipDelay.Value = 600D : Me.numTooltipDelay.Increment = 50D : Me.numTooltipDelay.Width = 120 : Me.numTooltipDelay.Name = "numTooltipDelay"
        '
        '—— Date ——
        '
        Me.lblSecData.AutoSize = True : Me.lblSecData.Text = "—— Date ——" : Me.lblSecData.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecData.Name = "lblSecData"
        Me.lblNodeCount.AutoSize = True : Me.lblNodeCount.Text = "Noduri (grupuri × frunze)" : Me.lblNodeCount.Name = "lblNodeCount"
        Me.cboNodeCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboNodeCount.Width = 270 : Me.cboNodeCount.Name = "cboNodeCount"
        Me.btnExpandAll.AutoSize = True : Me.btnExpandAll.Text = "Expandează tot" : Me.btnExpandAll.Name = "btnExpandAll" : Me.btnExpandAll.UseVisualStyleBackColor = True
        Me.btnCollapseAll.AutoSize = True : Me.btnCollapseAll.Text = "Restrânge tot" : Me.btnCollapseAll.Name = "btnCollapseAll" : Me.btnCollapseAll.UseVisualStyleBackColor = True
        Me.btnFromDefinitions.AutoSize = True : Me.btnFromDefinitions.Text = "Reconstruiește din Nodes (designer)" : Me.btnFromDefinitions.Name = "btnFromDefinitions" : Me.btnFromDefinitions.UseVisualStyleBackColor = True
        '
        '—— Export ——
        '
        Me.lblSecExport.AutoSize = True : Me.lblSecExport.Text = "—— Export ——" : Me.lblSecExport.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecExport.Name = "lblSecExport"
        Me.btnExport.AutoSize = True : Me.btnExport.Text = "Exportă setările (linii de designer)" : Me.btnExport.Name = "btnExport" : Me.btnExport.UseVisualStyleBackColor = True
        '
        ' imgNoduri — sursa de imagini pentru cheile de iconițe (NodeImages). În designerul VS
        ' pozele se încarcă prin editorul listei; aici le generăm în cod (fără resurse).
        '
        Me.imgNoduri.ImageSize = New System.Drawing.Size(16, 16)
        Me.imgNoduri.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
        Me.imgNoduri.TransparentColor = System.Drawing.Color.Transparent
        '
        'tree — controlul testat
        '
        Me.tree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tree.Name = "tree"
        Me.tree.HeaderVisible = True
        Me.tree.HeaderCaption = "Arbore de probă"
        Me.tree.SearchShow = True
        Me.tree.SearchClearButton = True
        Me.tree.SearchDefaultText = "minim 3 caractere…"
        Me.tree.SearchIn = KBot.Controls.AdvancedTreeControl.En_Tree_SearchIn.SearchIn_Both
        '
        'pnlButtons — verdictul uman
        '
        Me.pnlButtons.AutoSize = True
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Name = "pnlButtons"
        '
        Me.btnFail.AutoSize = True : Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel : Me.btnFail.Text = "Fail" : Me.btnFail.Name = "btnFail" : Me.btnFail.UseVisualStyleBackColor = True
        Me.btnPass.AutoSize = True : Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK : Me.btnPass.Text = "Pass" : Me.btnPass.Name = "btnPass" : Me.btnPass.UseVisualStyleBackColor = True
        '
        'TreePlaygroundForm
        '
        Me.AcceptButton = Me.btnPass
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1120, 760)
        ' Ordine de dock (regula casei): Fill întâi, apoi Left, apoi Bottom/Top.
        Me.Controls.Add(Me.tree)
        Me.Controls.Add(Me.flowLeft)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlTop)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "AdvancedTreeControl — playground proprietăți runtime"
        Me.Name = "TreePlaygroundForm"
        Me.pnlTop.ResumeLayout(False) : Me.pnlTop.PerformLayout()
        Me.pnlButtons.ResumeLayout(False) : Me.pnlButtons.PerformLayout()
        Me.flowLeft.ResumeLayout(False) : Me.flowLeft.PerformLayout()
        CType(Me.numHeaderHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numClearPad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numItemHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numIndent, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numExpander, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numCheckSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numRadioLevel, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numRightPad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numTreeFont, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numTooltipDelay, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False) : Me.PerformLayout()
    End Sub
End Class
