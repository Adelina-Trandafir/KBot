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

    ' Arborele NU e docat Fill: butonul de strângere din subsol își face treaba scriind Width, iar
    ' un control docat Fill n-are lățime proprie. Docat Left într-o gazdă care umple restul, exact
    ' ca o bară laterală reală, strângerea chiar se vede.
    Friend WithEvents pnlTreeHost As System.Windows.Forms.Panel
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
    Friend WithEvents lblHeaderIconSize As System.Windows.Forms.Label
    Friend WithEvents numHeaderIconSize As System.Windows.Forms.NumericUpDown

    ' —— Subsol ——
    Friend WithEvents lblSecFooter As System.Windows.Forms.Label
    Friend WithEvents chkFooterVisible As System.Windows.Forms.CheckBox
    Friend WithEvents lblFooterCaption As System.Windows.Forms.Label
    Friend WithEvents txtFooterCaption As System.Windows.Forms.TextBox
    Friend WithEvents lblFooterHeight As System.Windows.Forms.Label
    Friend WithEvents numFooterHeight As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnFooterFont As System.Windows.Forms.Button
    Friend WithEvents lblFooterAlign As System.Windows.Forms.Label
    Friend WithEvents cboFooterAlign As System.Windows.Forms.ComboBox
    Friend WithEvents lblFooterStyle As System.Windows.Forms.Label
    Friend WithEvents cboFooterStyle As System.Windows.Forms.ComboBox
    Friend WithEvents btnFooterBack As System.Windows.Forms.Button
    Friend WithEvents btnFooterFore As System.Windows.Forms.Button
    Friend WithEvents btnFooterGradEnd As System.Windows.Forms.Button
    Friend WithEvents btnFooterCapBack As System.Windows.Forms.Button
    Friend WithEvents btnFooterCapFore As System.Windows.Forms.Button
    Friend WithEvents chkFooterLeftIcon As System.Windows.Forms.CheckBox
    Friend WithEvents lblFooterIconSize As System.Windows.Forms.Label
    Friend WithEvents numFooterIconSize As System.Windows.Forms.NumericUpDown

    ' —— Subsol: strângere ——
    Friend WithEvents lblSecCollapse As System.Windows.Forms.Label
    Friend WithEvents chkCollapseButton As System.Windows.Forms.CheckBox
    Friend WithEvents lblCollapseSize As System.Windows.Forms.Label
    Friend WithEvents numCollapseSize As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblCollapsePos As System.Windows.Forms.Label
    Friend WithEvents cboCollapsePos As System.Windows.Forms.ComboBox
    Friend WithEvents chkCollapseImages As System.Windows.Forms.CheckBox
    Friend WithEvents lblMinCollapsed As System.Windows.Forms.Label
    Friend WithEvents numMinCollapsed As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkCollapsedFlyout As System.Windows.Forms.CheckBox
    Friend WithEvents lblFlyoutDelay As System.Windows.Forms.Label
    Friend WithEvents numFlyoutDelay As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblFlyoutSlide As System.Windows.Forms.Label
    Friend WithEvents numFlyoutSlide As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnToggleCollapse As System.Windows.Forms.Button

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
    Friend WithEvents btnLabelFore As System.Windows.Forms.Button
    Friend WithEvents lblSearchMode As System.Windows.Forms.Label
    Friend WithEvents cboSearchMode As System.Windows.Forms.ComboBox

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
    Friend WithEvents lblLeftIconSize As System.Windows.Forms.Label
    Friend WithEvents numLeftIconSize As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblRightIconSize As System.Windows.Forms.Label
    Friend WithEvents numRightIconSize As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnFont As System.Windows.Forms.Button
    Friend WithEvents lblScrollTheme As System.Windows.Forms.Label
    Friend WithEvents cboScrollTheme As System.Windows.Forms.ComboBox

    ' —— Culori ——
    Friend WithEvents lblSecColors As System.Windows.Forms.Label
    Friend WithEvents btnBackColor As System.Windows.Forms.Button
    Friend WithEvents btnForeColor As System.Windows.Forms.Button
    Friend WithEvents btnHoverBack As System.Windows.Forms.Button
    Friend WithEvents btnSelectedBack As System.Windows.Forms.Button
    Friend WithEvents btnSelectedBorder As System.Windows.Forms.Button
    Friend WithEvents btnLineColor As System.Windows.Forms.Button
    Friend WithEvents btnBorderColor As System.Windows.Forms.Button

    ' —— Tooltip ——
    Friend WithEvents lblSecTooltip As System.Windows.Forms.Label
    Friend WithEvents chkTooltip As System.Windows.Forms.CheckBox
    Friend WithEvents chkTooltipIconOnly As System.Windows.Forms.CheckBox
    Friend WithEvents lblTooltipDelay As System.Windows.Forms.Label
    Friend WithEvents numTooltipDelay As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnTooltipBack As System.Windows.Forms.Button
    Friend WithEvents btnTooltipFore As System.Windows.Forms.Button

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
        Me.pnlTreeHost = New System.Windows.Forms.Panel()
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
        Me.lblHeaderIconSize = New System.Windows.Forms.Label()
        Me.numHeaderIconSize = New System.Windows.Forms.NumericUpDown()
        Me.lblSecFooter = New System.Windows.Forms.Label()
        Me.chkFooterVisible = New System.Windows.Forms.CheckBox()
        Me.lblFooterCaption = New System.Windows.Forms.Label()
        Me.txtFooterCaption = New System.Windows.Forms.TextBox()
        Me.lblFooterHeight = New System.Windows.Forms.Label()
        Me.numFooterHeight = New System.Windows.Forms.NumericUpDown()
        Me.btnFooterFont = New System.Windows.Forms.Button()
        Me.lblFooterAlign = New System.Windows.Forms.Label()
        Me.cboFooterAlign = New System.Windows.Forms.ComboBox()
        Me.lblFooterStyle = New System.Windows.Forms.Label()
        Me.cboFooterStyle = New System.Windows.Forms.ComboBox()
        Me.btnFooterBack = New System.Windows.Forms.Button()
        Me.btnFooterFore = New System.Windows.Forms.Button()
        Me.btnFooterGradEnd = New System.Windows.Forms.Button()
        Me.btnFooterCapBack = New System.Windows.Forms.Button()
        Me.btnFooterCapFore = New System.Windows.Forms.Button()
        Me.chkFooterLeftIcon = New System.Windows.Forms.CheckBox()
        Me.lblFooterIconSize = New System.Windows.Forms.Label()
        Me.numFooterIconSize = New System.Windows.Forms.NumericUpDown()
        Me.lblSecCollapse = New System.Windows.Forms.Label()
        Me.chkCollapseButton = New System.Windows.Forms.CheckBox()
        Me.lblCollapseSize = New System.Windows.Forms.Label()
        Me.numCollapseSize = New System.Windows.Forms.NumericUpDown()
        Me.lblCollapsePos = New System.Windows.Forms.Label()
        Me.cboCollapsePos = New System.Windows.Forms.ComboBox()
        Me.chkCollapseImages = New System.Windows.Forms.CheckBox()
        Me.lblMinCollapsed = New System.Windows.Forms.Label()
        Me.numMinCollapsed = New System.Windows.Forms.NumericUpDown()
        Me.chkCollapsedFlyout = New System.Windows.Forms.CheckBox()
        Me.lblFlyoutDelay = New System.Windows.Forms.Label()
        Me.numFlyoutDelay = New System.Windows.Forms.NumericUpDown()
        Me.lblFlyoutSlide = New System.Windows.Forms.Label()
        Me.numFlyoutSlide = New System.Windows.Forms.NumericUpDown()
        Me.btnToggleCollapse = New System.Windows.Forms.Button()
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
        Me.btnLabelFore = New System.Windows.Forms.Button()
        Me.lblSearchMode = New System.Windows.Forms.Label()
        Me.cboSearchMode = New System.Windows.Forms.ComboBox()
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
        Me.lblLeftIconSize = New System.Windows.Forms.Label()
        Me.numLeftIconSize = New System.Windows.Forms.NumericUpDown()
        Me.lblRightIconSize = New System.Windows.Forms.Label()
        Me.numRightIconSize = New System.Windows.Forms.NumericUpDown()
        Me.btnFont = New System.Windows.Forms.Button()
        Me.lblScrollTheme = New System.Windows.Forms.Label()
        Me.cboScrollTheme = New System.Windows.Forms.ComboBox()
        Me.lblSecColors = New System.Windows.Forms.Label()
        Me.btnBackColor = New System.Windows.Forms.Button()
        Me.btnForeColor = New System.Windows.Forms.Button()
        Me.btnHoverBack = New System.Windows.Forms.Button()
        Me.btnSelectedBack = New System.Windows.Forms.Button()
        Me.btnSelectedBorder = New System.Windows.Forms.Button()
        Me.btnLineColor = New System.Windows.Forms.Button()
        Me.btnBorderColor = New System.Windows.Forms.Button()
        Me.lblSecTooltip = New System.Windows.Forms.Label()
        Me.chkTooltip = New System.Windows.Forms.CheckBox()
        Me.chkTooltipIconOnly = New System.Windows.Forms.CheckBox()
        Me.lblTooltipDelay = New System.Windows.Forms.Label()
        Me.numTooltipDelay = New System.Windows.Forms.NumericUpDown()
        Me.btnTooltipBack = New System.Windows.Forms.Button()
        Me.btnTooltipFore = New System.Windows.Forms.Button()
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
        Me.pnlTreeHost.SuspendLayout()
        Me.flowLeft.SuspendLayout()
        CType(Me.numHeaderHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numHeaderIconSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numFooterHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numFooterIconSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numCollapseSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numMinCollapsed, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numFlyoutDelay, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numFlyoutSlide, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numClearPad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numItemHeight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numIndent, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numExpander, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numCheckSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numRadioLevel, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numRightPad, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numLeftIconSize, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numRightIconSize, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.flowLeft.Controls.Add(Me.lblHeaderIconSize)
        Me.flowLeft.Controls.Add(Me.numHeaderIconSize)
        Me.flowLeft.Controls.Add(Me.lblSecFooter)
        Me.flowLeft.Controls.Add(Me.chkFooterVisible)
        Me.flowLeft.Controls.Add(Me.lblFooterCaption)
        Me.flowLeft.Controls.Add(Me.txtFooterCaption)
        Me.flowLeft.Controls.Add(Me.lblFooterHeight)
        Me.flowLeft.Controls.Add(Me.numFooterHeight)
        Me.flowLeft.Controls.Add(Me.btnFooterFont)
        Me.flowLeft.Controls.Add(Me.lblFooterAlign)
        Me.flowLeft.Controls.Add(Me.cboFooterAlign)
        Me.flowLeft.Controls.Add(Me.lblFooterStyle)
        Me.flowLeft.Controls.Add(Me.cboFooterStyle)
        Me.flowLeft.Controls.Add(Me.btnFooterBack)
        Me.flowLeft.Controls.Add(Me.btnFooterFore)
        Me.flowLeft.Controls.Add(Me.btnFooterGradEnd)
        Me.flowLeft.Controls.Add(Me.btnFooterCapBack)
        Me.flowLeft.Controls.Add(Me.btnFooterCapFore)
        Me.flowLeft.Controls.Add(Me.chkFooterLeftIcon)
        Me.flowLeft.Controls.Add(Me.lblFooterIconSize)
        Me.flowLeft.Controls.Add(Me.numFooterIconSize)
        Me.flowLeft.Controls.Add(Me.lblSecCollapse)
        Me.flowLeft.Controls.Add(Me.chkCollapseButton)
        Me.flowLeft.Controls.Add(Me.lblCollapseSize)
        Me.flowLeft.Controls.Add(Me.numCollapseSize)
        Me.flowLeft.Controls.Add(Me.lblCollapsePos)
        Me.flowLeft.Controls.Add(Me.cboCollapsePos)
        Me.flowLeft.Controls.Add(Me.chkCollapseImages)
        Me.flowLeft.Controls.Add(Me.lblMinCollapsed)
        Me.flowLeft.Controls.Add(Me.numMinCollapsed)
        Me.flowLeft.Controls.Add(Me.chkCollapsedFlyout)
        Me.flowLeft.Controls.Add(Me.lblFlyoutDelay)
        Me.flowLeft.Controls.Add(Me.numFlyoutDelay)
        Me.flowLeft.Controls.Add(Me.lblFlyoutSlide)
        Me.flowLeft.Controls.Add(Me.numFlyoutSlide)
        Me.flowLeft.Controls.Add(Me.btnToggleCollapse)
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
        Me.flowLeft.Controls.Add(Me.btnLabelFore)
        Me.flowLeft.Controls.Add(Me.lblSearchMode)
        Me.flowLeft.Controls.Add(Me.cboSearchMode)
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
        Me.flowLeft.Controls.Add(Me.lblLeftIconSize)
        Me.flowLeft.Controls.Add(Me.numLeftIconSize)
        Me.flowLeft.Controls.Add(Me.lblRightIconSize)
        Me.flowLeft.Controls.Add(Me.numRightIconSize)
        Me.flowLeft.Controls.Add(Me.btnFont)
        Me.flowLeft.Controls.Add(Me.lblScrollTheme)
        Me.flowLeft.Controls.Add(Me.cboScrollTheme)
        Me.flowLeft.Controls.Add(Me.lblSecColors)
        Me.flowLeft.Controls.Add(Me.btnBackColor)
        Me.flowLeft.Controls.Add(Me.btnForeColor)
        Me.flowLeft.Controls.Add(Me.btnHoverBack)
        Me.flowLeft.Controls.Add(Me.btnSelectedBack)
        Me.flowLeft.Controls.Add(Me.btnSelectedBorder)
        Me.flowLeft.Controls.Add(Me.btnLineColor)
        Me.flowLeft.Controls.Add(Me.btnBorderColor)
        Me.flowLeft.Controls.Add(Me.lblSecTooltip)
        Me.flowLeft.Controls.Add(Me.chkTooltip)
        Me.flowLeft.Controls.Add(Me.chkTooltipIconOnly)
        Me.flowLeft.Controls.Add(Me.lblTooltipDelay)
        Me.flowLeft.Controls.Add(Me.numTooltipDelay)
        Me.flowLeft.Controls.Add(Me.btnTooltipBack)
        Me.flowLeft.Controls.Add(Me.btnTooltipFore)
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
        Me.lblHeaderIconSize.AutoSize = True : Me.lblHeaderIconSize.Text = "HeaderIconSize (latură)" : Me.lblHeaderIconSize.Name = "lblHeaderIconSize"
        Me.numHeaderIconSize.Minimum = 8D : Me.numHeaderIconSize.Maximum = 48D : Me.numHeaderIconSize.Value = 16D : Me.numHeaderIconSize.Width = 120 : Me.numHeaderIconSize.Name = "numHeaderIconSize"
        '
        '—— Subsol ——
        '
        Me.lblSecFooter.AutoSize = True : Me.lblSecFooter.Text = "—— Subsol ——" : Me.lblSecFooter.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecFooter.Name = "lblSecFooter"
        Me.chkFooterVisible.AutoSize = True : Me.chkFooterVisible.Text = "FooterVisible" : Me.chkFooterVisible.Name = "chkFooterVisible"
        Me.lblFooterCaption.AutoSize = True : Me.lblFooterCaption.Text = "FooterCaption" : Me.lblFooterCaption.Name = "lblFooterCaption"
        Me.txtFooterCaption.Width = 270 : Me.txtFooterCaption.Name = "txtFooterCaption"
        Me.lblFooterHeight.AutoSize = True : Me.lblFooterHeight.Text = "FooterHeight" : Me.lblFooterHeight.Name = "lblFooterHeight"
        Me.numFooterHeight.Minimum = 16D : Me.numFooterHeight.Maximum = 120D : Me.numFooterHeight.Value = 28D : Me.numFooterHeight.Width = 120 : Me.numFooterHeight.Name = "numFooterHeight"
        Me.btnFooterFont.AutoSize = True : Me.btnFooterFont.Text = "FooterCaptionFont…" : Me.btnFooterFont.Name = "btnFooterFont" : Me.btnFooterFont.UseVisualStyleBackColor = True
        Me.lblFooterAlign.AutoSize = True : Me.lblFooterAlign.Text = "FooterTextAlign" : Me.lblFooterAlign.Name = "lblFooterAlign"
        Me.cboFooterAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboFooterAlign.Width = 270 : Me.cboFooterAlign.Name = "cboFooterAlign"
        Me.lblFooterStyle.AutoSize = True : Me.lblFooterStyle.Text = "FooterBackStyle" : Me.lblFooterStyle.Name = "lblFooterStyle"
        Me.cboFooterStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboFooterStyle.Width = 270 : Me.cboFooterStyle.Name = "cboFooterStyle"
        Me.btnFooterBack.AutoSize = True : Me.btnFooterBack.Text = "FooterBackColor…" : Me.btnFooterBack.Name = "btnFooterBack" : Me.btnFooterBack.UseVisualStyleBackColor = True
        Me.btnFooterFore.AutoSize = True : Me.btnFooterFore.Text = "FooterForeColor…" : Me.btnFooterFore.Name = "btnFooterFore" : Me.btnFooterFore.UseVisualStyleBackColor = True
        Me.btnFooterGradEnd.AutoSize = True : Me.btnFooterGradEnd.Text = "FooterGradientEndColor…" : Me.btnFooterGradEnd.Name = "btnFooterGradEnd" : Me.btnFooterGradEnd.UseVisualStyleBackColor = True
        Me.btnFooterCapBack.AutoSize = True : Me.btnFooterCapBack.Text = "FooterCaptionBackColor…" : Me.btnFooterCapBack.Name = "btnFooterCapBack" : Me.btnFooterCapBack.UseVisualStyleBackColor = True
        Me.btnFooterCapFore.AutoSize = True : Me.btnFooterCapFore.Text = "FooterCaptionForeColor…" : Me.btnFooterCapFore.Name = "btnFooterCapFore" : Me.btnFooterCapFore.UseVisualStyleBackColor = True
        Me.chkFooterLeftIcon.AutoSize = True : Me.chkFooterLeftIcon.Text = "Iconiță stânga în subsol" : Me.chkFooterLeftIcon.Name = "chkFooterLeftIcon"
        Me.lblFooterIconSize.AutoSize = True : Me.lblFooterIconSize.Text = "FooterIconSize (latură)" : Me.lblFooterIconSize.Name = "lblFooterIconSize"
        Me.numFooterIconSize.Minimum = 8D : Me.numFooterIconSize.Maximum = 48D : Me.numFooterIconSize.Value = 16D : Me.numFooterIconSize.Width = 120 : Me.numFooterIconSize.Name = "numFooterIconSize"
        '
        '—— Subsol: strângere ——
        '
        Me.lblSecCollapse.AutoSize = True : Me.lblSecCollapse.Text = "—— Subsol: strângere ——" : Me.lblSecCollapse.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecCollapse.Name = "lblSecCollapse"
        Me.chkCollapseButton.AutoSize = True : Me.chkCollapseButton.Text = "FooterCollapseButton" : Me.chkCollapseButton.Name = "chkCollapseButton"
        Me.lblCollapseSize.AutoSize = True : Me.lblCollapseSize.Text = "FooterCollapseButtonSize" : Me.lblCollapseSize.Name = "lblCollapseSize"
        Me.numCollapseSize.Minimum = 8D : Me.numCollapseSize.Maximum = 48D : Me.numCollapseSize.Value = 16D : Me.numCollapseSize.Width = 120 : Me.numCollapseSize.Name = "numCollapseSize"
        Me.lblCollapsePos.AutoSize = True : Me.lblCollapsePos.Text = "FooterCollapseButtonPosition" : Me.lblCollapsePos.Name = "lblCollapsePos"
        Me.cboCollapsePos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboCollapsePos.Width = 270 : Me.cboCollapsePos.Name = "cboCollapsePos"
        Me.chkCollapseImages.AutoSize = True : Me.chkCollapseImages.Text = "Pictograme pe buton (în loc de unghi)" : Me.chkCollapseImages.Name = "chkCollapseImages"
        Me.lblMinCollapsed.AutoSize = True : Me.lblMinCollapsed.Text = "MinimumCollapsedWidth" : Me.lblMinCollapsed.Name = "lblMinCollapsed"
        Me.numMinCollapsed.Minimum = 16D : Me.numMinCollapsed.Maximum = 400D : Me.numMinCollapsed.Value = 100D : Me.numMinCollapsed.Width = 120 : Me.numMinCollapsed.Name = "numMinCollapsed"
        Me.chkCollapsedFlyout.AutoSize = True : Me.chkCollapsedFlyout.Text = "CollapsedFlyout (nod plutitor)" : Me.chkCollapsedFlyout.Checked = True : Me.chkCollapsedFlyout.Name = "chkCollapsedFlyout"
        Me.lblFlyoutDelay.AutoSize = True : Me.lblFlyoutDelay.Text = "FlyoutDelay (ms)" : Me.lblFlyoutDelay.Name = "lblFlyoutDelay"
        Me.numFlyoutDelay.Minimum = 0D : Me.numFlyoutDelay.Maximum = 3000D : Me.numFlyoutDelay.Value = 250D : Me.numFlyoutDelay.Increment = 50D : Me.numFlyoutDelay.Width = 120 : Me.numFlyoutDelay.Name = "numFlyoutDelay"
        Me.lblFlyoutSlide.AutoSize = True : Me.lblFlyoutSlide.Text = "FlyoutSlideDuration (ms)" : Me.lblFlyoutSlide.Name = "lblFlyoutSlide"
        Me.numFlyoutSlide.Minimum = 0D : Me.numFlyoutSlide.Maximum = 2000D : Me.numFlyoutSlide.Value = 120D : Me.numFlyoutSlide.Increment = 20D : Me.numFlyoutSlide.Width = 120 : Me.numFlyoutSlide.Name = "numFlyoutSlide"
        Me.btnToggleCollapse.AutoSize = True : Me.btnToggleCollapse.Text = "ToggleCollapse()" : Me.btnToggleCollapse.Name = "btnToggleCollapse" : Me.btnToggleCollapse.UseVisualStyleBackColor = True
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
        Me.btnLabelFore.AutoSize = True : Me.btnLabelFore.Text = "SearchBarLabelForeColor…" : Me.btnLabelFore.Name = "btnLabelFore" : Me.btnLabelFore.UseVisualStyleBackColor = True
        Me.lblSearchMode.AutoSize = True : Me.lblSearchMode.Text = "SearchMode" : Me.lblSearchMode.Name = "lblSearchMode"
        Me.cboSearchMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboSearchMode.Width = 270 : Me.cboSearchMode.Name = "cboSearchMode"
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
        Me.chkReserveRight.AutoSize = True : Me.chkReserveRight.Text = "ReserveRightIconSpace (loc fix, text nemișcat)" : Me.chkReserveRight.Name = "chkReserveRight"
        Me.lblRightPad.AutoSize = True : Me.lblRightPad.Text = "RightIconRightPadding" : Me.lblRightPad.Name = "lblRightPad"
        Me.numRightPad.Minimum = 0D : Me.numRightPad.Maximum = 40D : Me.numRightPad.Value = 6D : Me.numRightPad.Width = 120 : Me.numRightPad.Name = "numRightPad"
        Me.lblLeftIconSize.AutoSize = True : Me.lblLeftIconSize.Text = "LeftIconSize (latură)" : Me.lblLeftIconSize.Name = "lblLeftIconSize"
        Me.numLeftIconSize.Minimum = 8D : Me.numLeftIconSize.Maximum = 48D : Me.numLeftIconSize.Value = 16D : Me.numLeftIconSize.Width = 120 : Me.numLeftIconSize.Name = "numLeftIconSize"
        Me.lblRightIconSize.AutoSize = True : Me.lblRightIconSize.Text = "RightIconSize (latură)" : Me.lblRightIconSize.Name = "lblRightIconSize"
        Me.numRightIconSize.Minimum = 8D : Me.numRightIconSize.Maximum = 48D : Me.numRightIconSize.Value = 16D : Me.numRightIconSize.Width = 120 : Me.numRightIconSize.Name = "numRightIconSize"
        ' O SINGURĂ intrare de font pentru arbore: Font. «TreeFont» a dispărut din control.
        Me.btnFont.AutoSize = True : Me.btnFont.Text = "Font (nodurile)…" : Me.btnFont.Name = "btnFont" : Me.btnFont.UseVisualStyleBackColor = True
        Me.lblScrollTheme.AutoSize = True : Me.lblScrollTheme.Text = "ScrollBarTheme" : Me.lblScrollTheme.Name = "lblScrollTheme"
        Me.cboScrollTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboScrollTheme.Width = 270 : Me.cboScrollTheme.Name = "cboScrollTheme"
        '
        '—— Culori ——
        '
        Me.lblSecColors.AutoSize = True : Me.lblSecColors.Text = "—— Culori ——" : Me.lblSecColors.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecColors.Name = "lblSecColors"
        Me.btnBackColor.AutoSize = True : Me.btnBackColor.Text = "BackColor…" : Me.btnBackColor.Name = "btnBackColor" : Me.btnBackColor.UseVisualStyleBackColor = True
        Me.btnForeColor.AutoSize = True : Me.btnForeColor.Text = "ForeColor…" : Me.btnForeColor.Name = "btnForeColor" : Me.btnForeColor.UseVisualStyleBackColor = True
        Me.btnHoverBack.AutoSize = True : Me.btnHoverBack.Text = "HoverBackColor…" : Me.btnHoverBack.Name = "btnHoverBack" : Me.btnHoverBack.UseVisualStyleBackColor = True
        Me.btnSelectedBack.AutoSize = True : Me.btnSelectedBack.Text = "SelectedBackColor…" : Me.btnSelectedBack.Name = "btnSelectedBack" : Me.btnSelectedBack.UseVisualStyleBackColor = True
        Me.btnSelectedBorder.AutoSize = True : Me.btnSelectedBorder.Text = "SelectedBorderColor…" : Me.btnSelectedBorder.Name = "btnSelectedBorder" : Me.btnSelectedBorder.UseVisualStyleBackColor = True
        Me.btnLineColor.AutoSize = True : Me.btnLineColor.Text = "LineColor…" : Me.btnLineColor.Name = "btnLineColor" : Me.btnLineColor.UseVisualStyleBackColor = True
        Me.btnBorderColor.AutoSize = True : Me.btnBorderColor.Text = "BorderColor…" : Me.btnBorderColor.Name = "btnBorderColor" : Me.btnBorderColor.UseVisualStyleBackColor = True
        '
        '—— Tooltip ——
        '
        Me.lblSecTooltip.AutoSize = True : Me.lblSecTooltip.Text = "—— Tooltip ——" : Me.lblSecTooltip.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecTooltip.Name = "lblSecTooltip"
        Me.chkTooltip.AutoSize = True : Me.chkTooltip.Text = "TooltipShow" : Me.chkTooltip.Checked = True : Me.chkTooltip.Name = "chkTooltip"
        Me.chkTooltipIconOnly.AutoSize = True : Me.chkTooltipIconOnly.Text = "TooltipShowOnlyOnLeftIcon" : Me.chkTooltipIconOnly.Name = "chkTooltipIconOnly"
        Me.lblTooltipDelay.AutoSize = True : Me.lblTooltipDelay.Text = "TooltipDelayMs" : Me.lblTooltipDelay.Name = "lblTooltipDelay"
        Me.numTooltipDelay.Minimum = 0D : Me.numTooltipDelay.Maximum = 5000D : Me.numTooltipDelay.Value = 600D : Me.numTooltipDelay.Increment = 50D : Me.numTooltipDelay.Width = 120 : Me.numTooltipDelay.Name = "numTooltipDelay"
        Me.btnTooltipBack.AutoSize = True : Me.btnTooltipBack.Text = "TooltipBackColor…" : Me.btnTooltipBack.Name = "btnTooltipBack" : Me.btnTooltipBack.UseVisualStyleBackColor = True
        Me.btnTooltipFore.AutoSize = True : Me.btnTooltipFore.Text = "TooltipForeColor…" : Me.btnTooltipFore.Name = "btnTooltipFore" : Me.btnTooltipFore.UseVisualStyleBackColor = True
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
        'pnlTreeHost — gazda arborelui. Arborele e docat LEFT în ea, nu Fill pe formular:
        'butonul de strângere scrie Width, iar un control docat Fill n-are lățime proprie.
        '
        Me.pnlTreeHost.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlTreeHost.Name = "pnlTreeHost"
        Me.pnlTreeHost.Controls.Add(Me.tree)
        '
        'tree — controlul testat
        '
        Me.tree.Dock = System.Windows.Forms.DockStyle.Left
        Me.tree.Width = 420
        Me.tree.Name = "tree"
        Me.tree.HeaderVisible = True
        Me.tree.HeaderCaption = "Arbore de probă"
        Me.tree.FooterVisible = True
        Me.tree.FooterCaption = "Subsol de probă"
        Me.tree.FooterCollapseButton = True
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
        Me.Controls.Add(Me.pnlTreeHost)
        Me.Controls.Add(Me.flowLeft)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlTop)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "AdvancedTreeControl — playground proprietăți runtime"
        Me.Name = "TreePlaygroundForm"
        Me.pnlTop.ResumeLayout(False) : Me.pnlTop.PerformLayout()
        Me.pnlButtons.ResumeLayout(False) : Me.pnlButtons.PerformLayout()
        Me.pnlTreeHost.ResumeLayout(False)
        Me.flowLeft.ResumeLayout(False) : Me.flowLeft.PerformLayout()
        CType(Me.numHeaderHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numHeaderIconSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numFooterHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numFooterIconSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numCollapseSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numMinCollapsed, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numFlyoutDelay, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numFlyoutSlide, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numClearPad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numItemHeight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numIndent, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numExpander, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numCheckSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numRadioLevel, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numRightPad, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numLeftIconSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numRightIconSize, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numTooltipDelay, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False) : Me.PerformLayout()
    End Sub
End Class
