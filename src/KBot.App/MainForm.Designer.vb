<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits KBot.Theming.KBotShellForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim TreeNodeDefinition1 As KBot.Controls.TreeNodeDefinition = New Controls.TreeNodeDefinition()
        Dim KBotNavItem1 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Dim KBotNavItem2 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem3 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem4 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem5 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem6 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem7 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem8 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        pnlRoot = New Panel()
        pnlWork = New Panel()
        split = New SplitContainer()
        pnlTree = New Panel()
        tree = New Controls.AdvancedTreeControl()
        pnlTreeHead = New Panel()
        lblTree = New Label()
        btnInfo = New Button()
        btnSort = New Button()
        btnOpt = New Button()
        viewHost = New Panel()
        navViews = New Controls.KBotNavList()
        pnlStatus = New Panel()
        lblForexe = New Label()
        lblOperator = New Label()
        lblProgram = New Label()
        btnSinc = New Button()
        pnlHeader = New Panel()
        tlyHeader = New TableLayoutPanel()
        cboSs = New Controls.KBotComboBox()
        lblSs = New Label()
        cboAn = New Controls.KBotComboBox()
        lblAn = New Label()
        lblUnit = New Label()
        busyBar = New Controls.KBotBusyBar()
        capBar = New Controls.KBotCaptionBar()
        pnlRoot.SuspendLayout()
        pnlWork.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlTree.SuspendLayout()
        pnlTreeHead.SuspendLayout()
        CType(navViews, ComponentModel.ISupportInitialize).BeginInit()
        pnlStatus.SuspendLayout()
        pnlHeader.SuspendLayout()
        tlyHeader.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlRoot
        ' 
        pnlRoot.Controls.Add(pnlWork)
        pnlRoot.Controls.Add(pnlStatus)
        pnlRoot.Controls.Add(pnlHeader)
        pnlRoot.Controls.Add(busyBar)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 2)
        pnlRoot.Margin = New Padding(4, 5, 4, 5)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1827, 1263)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        ' 
        ' pnlWork
        ' 
        pnlWork.Controls.Add(split)
        pnlWork.Controls.Add(navViews)
        pnlWork.Dock = DockStyle.Fill
        pnlWork.Location = New Point(0, 122)
        pnlWork.Margin = New Padding(4, 5, 4, 5)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(11, 13, 11, 13)
        pnlWork.Size = New Size(1827, 1068)
        pnlWork.TabIndex = 0
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(229, 13)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(pnlTree)
        split.Panel1.Padding = New Padding(11, 0, 0, 0)
        split.Panel1MinSize = 240
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(viewHost)
        split.Panel2.Padding = New Padding(11, 0, 0, 0)
        split.Panel2MinSize = 400
        split.Size = New Size(1587, 1042)
        split.SplitterDistance = 510
        split.SplitterWidth = 9
        split.TabIndex = 1
        ' 
        ' pnlTree
        ' 
        pnlTree.Controls.Add(tree)
        pnlTree.Controls.Add(pnlTreeHead)
        pnlTree.Dock = DockStyle.Fill
        pnlTree.Location = New Point(11, 0)
        pnlTree.Margin = New Padding(4, 5, 4, 5)
        pnlTree.Name = "pnlTree"
        pnlTree.Size = New Size(499, 1042)
        pnlTree.TabIndex = 0
        pnlTree.Tag = "Card"
        ' 
        ' tree
        ' 
        tree.Dock = DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpanderSize = 16
        tree.FlyoutDelay = 150
        tree.FlyoutSlideDuration = 100
        tree.Font = New Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Actualizează angajamente"
        tree.FooterCaptionFont = New Font("Consolas", 8F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.FooterCollapseButtonPosition = KBot.Controls.AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 40
        tree.FooterIconSize = New Size(24, 24)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = KBot.Controls.AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " LISTĂ ANGAJAMENTE"
        tree.HeaderFont = New Font("Tahoma", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Color.Black
        tree.HeaderGradientEndColor = Color.CornflowerBlue
        tree.HeaderHeight = 40
        tree.HeaderIconSize = New Size(24, 24)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderRightIcon = My.Resources.Resources.settings__1_
        tree.HeaderSearchIcon = My.Resources.Resources.Everaldo_Crystal_Clear_App_xmag_search_48
        tree.HeaderVisible = True
        tree.ItemHeight = 34
        tree.LeftIconSize = New Size(20, 20)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.MinimumCollapsedWidth = 50
        tree.Name = "tree"
        TreeNodeDefinition1.Caption = "elasalqw qwlrlqwl qlrqwlr lqwr "
        TreeNodeDefinition1.ImageKey = Nothing
        TreeNodeDefinition1.Key = "1"
        TreeNodeDefinition1.OpenImageKey = Nothing
        TreeNodeDefinition1.ParentKey = Nothing
        TreeNodeDefinition1.RightImageKey = Nothing
        TreeNodeDefinition1.Tag = Nothing
        TreeNodeDefinition1.Tooltip = Nothing
        tree.Nodes.Add(TreeNodeDefinition1)
        tree.PaddingSelectionLeft = 2
        tree.ReserveRightIconSpace = True
        tree.RightIconSize = New Size(20, 20)
        tree.RootExpander = False
        tree.ScrollBarTheme = KBot.Controls.AdvancedTreeControl.En_ScrollBarTheme.Default
        tree.SearchBoxBackColor = Color.FromArgb(CByte(192), CByte(255), CByte(192))
        tree.SearchClearButton = True
        tree.SearchDefaultText = "... tastează minim 3 caractere ..."
        tree.SearchIn = KBot.Controls.AdvancedTreeControl.En_Tree_SearchIn.SearchIn_Both
        tree.ShowRightIconOnHover = True
        tree.Size = New Size(499, 1042)
        tree.TabIndex = 0
        tree.TooltipShowOnlyOnLeftIcon = True
        tree.TreeListView = True
        ' 
        ' pnlTreeHead
        ' 
        pnlTreeHead.Controls.Add(lblTree)
        pnlTreeHead.Controls.Add(btnInfo)
        pnlTreeHead.Controls.Add(btnSort)
        pnlTreeHead.Controls.Add(btnOpt)
        pnlTreeHead.Location = New Point(0, 0)
        pnlTreeHead.Margin = New Padding(4, 5, 4, 5)
        pnlTreeHead.Name = "pnlTreeHead"
        pnlTreeHead.Size = New Size(522, 60)
        pnlTreeHead.TabIndex = 1
        pnlTreeHead.Tag = "Card"
        pnlTreeHead.Visible = False
        ' 
        ' lblTree
        ' 
        lblTree.AutoSize = True
        lblTree.Font = New Font("Segoe UI Semibold", 10F)
        lblTree.Location = New Point(14, 13)
        lblTree.Margin = New Padding(4, 0, 4, 0)
        lblTree.Name = "lblTree"
        lblTree.Size = New Size(133, 28)
        lblTree.TabIndex = 0
        lblTree.Text = "Angajamente"
        ' 
        ' btnInfo
        ' 
        btnInfo.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnInfo.FlatStyle = FlatStyle.Flat
        btnInfo.Location = New Point(377, 7)
        btnInfo.Margin = New Padding(4, 5, 4, 5)
        btnInfo.Name = "btnInfo"
        btnInfo.Size = New Size(40, 47)
        btnInfo.TabIndex = 1
        btnInfo.Text = "ⓘ"
        btnInfo.UseVisualStyleBackColor = True
        ' 
        ' btnSort
        ' 
        btnSort.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSort.FlatStyle = FlatStyle.Flat
        btnSort.Location = New Point(425, 7)
        btnSort.Margin = New Padding(4, 5, 4, 5)
        btnSort.Name = "btnSort"
        btnSort.Size = New Size(40, 47)
        btnSort.TabIndex = 1
        btnSort.Text = "↕"
        btnSort.UseVisualStyleBackColor = True
        ' 
        ' btnOpt
        ' 
        btnOpt.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnOpt.FlatStyle = FlatStyle.Flat
        btnOpt.Location = New Point(474, 7)
        btnOpt.Margin = New Padding(4, 5, 4, 5)
        btnOpt.Name = "btnOpt"
        btnOpt.Size = New Size(40, 47)
        btnOpt.TabIndex = 2
        btnOpt.Text = "…"
        btnOpt.UseVisualStyleBackColor = True
        ' 
        ' viewHost
        ' 
        viewHost.Dock = DockStyle.Fill
        viewHost.Location = New Point(11, 0)
        viewHost.Margin = New Padding(4, 5, 4, 5)
        viewHost.Name = "viewHost"
        viewHost.Size = New Size(1057, 1042)
        viewHost.TabIndex = 0
        viewHost.Tag = "Card"
        ' 
        ' navViews
        ' 
        navViews.CollapseButtonSize = 14
        navViews.CollapseCollapsedImage = My.Resources.Resources.expand_24
        navViews.CollapseCorner = KBot.Controls.KBotNavCorner.BottomLeft
        navViews.CollapseExpandedImage = My.Resources.Resources.collapse_24
        navViews.Collapsible = True
        navViews.Dock = DockStyle.Left
        navViews.FlyoutDelay = 150
        navViews.FlyoutSlideDuration = 100
        navViews.ItemCornerRadius = 8
        navViews.ItemPadding = New Padding(0)
        KBotNavItem1.Image = CType(resources.GetObject("KBotNavItem1.Image"), Image)
        KBotNavItem1.Key = "sumar"
        KBotNavItem1.Text = "Sumar"
        KBotNavItem2.Image = My.Resources.Resources.calendar
        KBotNavItem2.Key = "istoric"
        KBotNavItem2.Text = "Istoric"
        KBotNavItem3.Image = My.Resources.Resources.database
        KBotNavItem3.Key = "rezervari"
        KBotNavItem3.Text = "Rezervări"
        KBotNavItem4.Image = My.Resources.Resources.binvoice
        KBotNavItem4.Key = "receptii"
        KBotNavItem4.Text = "Recepții"
        KBotNavItem5.Image = My.Resources.Resources.credit_card
        KBotNavItem5.Key = "plati"
        KBotNavItem5.Text = "Plăți"
        KBotNavItem6.Align = KBot.Controls.KBotNavAlign.Far
        KBotNavItem6.IsSeparator = True
        KBotNavItem6.Key = "__sep_1"
        KBotNavItem6.Text = Nothing
        KBotNavItem7.Align = KBot.Controls.KBotNavAlign.Far
        KBotNavItem7.Image = My.Resources.Resources.Umut_Pulat_Tulliana_2_File_temporary_32
        KBotNavItem7.Key = "ddf"
        KBotNavItem7.Text = "Fundamentare"
        KBotNavItem8.Align = KBot.Controls.KBotNavAlign.Far
        KBotNavItem8.Image = My.Resources.Resources.Umut_Pulat_Tulliana_2_File_locked_32
        KBotNavItem8.Key = "ord"
        KBotNavItem8.Text = "Ordonanțare"
        navViews.Items.Add(KBotNavItem1)
        navViews.Items.Add(KBotNavItem2)
        navViews.Items.Add(KBotNavItem3)
        navViews.Items.Add(KBotNavItem4)
        navViews.Items.Add(KBotNavItem5)
        navViews.Items.Add(KBotNavItem6)
        navViews.Items.Add(KBotNavItem7)
        navViews.Items.Add(KBotNavItem8)
        navViews.Location = New Point(11, 13)
        navViews.Margin = New Padding(4, 5, 4, 5)
        navViews.Name = "navViews"
        navViews.SelectedKey = Nothing
        navViews.Size = New Size(218, 1042)
        navViews.TabIndex = 0
        ' 
        ' pnlStatus
        ' 
        pnlStatus.Controls.Add(lblForexe)
        pnlStatus.Controls.Add(lblOperator)
        pnlStatus.Controls.Add(lblProgram)
        pnlStatus.Controls.Add(btnSinc)
        pnlStatus.Dock = DockStyle.Bottom
        pnlStatus.Location = New Point(0, 1190)
        pnlStatus.Margin = New Padding(4, 5, 4, 5)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(1827, 73)
        pnlStatus.TabIndex = 1
        pnlStatus.Tag = "Card"
        ' 
        ' lblForexe
        ' 
        lblForexe.AutoSize = True
        lblForexe.Location = New Point(799, 25)
        lblForexe.Margin = New Padding(4, 0, 4, 0)
        lblForexe.Name = "lblForexe"
        lblForexe.Size = New Size(175, 25)
        lblForexe.TabIndex = 7
        lblForexe.Text = "● Forexe: neconectat"
        lblForexe.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblOperator
        ' 
        lblOperator.AutoSize = True
        lblOperator.Location = New Point(17, 23)
        lblOperator.Margin = New Padding(4, 0, 4, 0)
        lblOperator.Name = "lblOperator"
        lblOperator.Size = New Size(84, 25)
        lblOperator.TabIndex = 0
        lblOperator.Text = "Operator"
        ' 
        ' lblProgram
        ' 
        lblProgram.AutoSize = True
        lblProgram.Location = New Point(617, 23)
        lblProgram.Margin = New Padding(4, 0, 4, 0)
        lblProgram.Name = "lblProgram"
        lblProgram.Size = New Size(81, 25)
        lblProgram.TabIndex = 1
        lblProgram.Text = "Program"
        ' 
        ' btnSinc
        ' 
        btnSinc.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSinc.FlatStyle = FlatStyle.Flat
        btnSinc.Font = New Font("Segoe UI Semibold", 10F)
        btnSinc.Location = New Point(1595, 10)
        btnSinc.Margin = New Padding(4, 5, 4, 5)
        btnSinc.Name = "btnSinc"
        btnSinc.Size = New Size(214, 53)
        btnSinc.TabIndex = 3
        btnSinc.Text = "Sincronizare"
        btnSinc.UseVisualStyleBackColor = True
        ' 
        ' pnlHeader
        ' 
        pnlHeader.BorderStyle = BorderStyle.FixedSingle
        pnlHeader.Controls.Add(tlyHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 72)
        pnlHeader.Margin = New Padding(0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(1827, 50)
        pnlHeader.TabIndex = 2
        pnlHeader.Tag = "Card"
        ' 
        ' tlyHeader
        ' 
        tlyHeader.BackColor = Color.Transparent
        tlyHeader.ColumnCount = 7
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 50F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 50F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyHeader.Controls.Add(cboSs, 6, 0)
        tlyHeader.Controls.Add(lblSs, 5, 0)
        tlyHeader.Controls.Add(cboAn, 3, 0)
        tlyHeader.Controls.Add(lblAn, 2, 0)
        tlyHeader.Controls.Add(lblUnit, 0, 0)
        tlyHeader.Dock = DockStyle.Fill
        tlyHeader.Location = New Point(0, 0)
        tlyHeader.Margin = New Padding(0)
        tlyHeader.Name = "tlyHeader"
        tlyHeader.RowCount = 1
        tlyHeader.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyHeader.Size = New Size(1825, 48)
        tlyHeader.TabIndex = 6
        ' 
        ' cboSs
        ' 
        cboSs.Dock = DockStyle.Fill
        cboSs.DrawMode = DrawMode.OwnerDrawFixed
        cboSs.DropDownStyle = ComboBoxStyle.DropDownList
        cboSs.FlatStyle = FlatStyle.Flat
        cboSs.Location = New Point(1679, 4)
        cboSs.Margin = New Padding(4, 4, 4, 0)
        cboSs.Name = "cboSs"
        cboSs.Size = New Size(142, 32)
        cboSs.TabIndex = 5
        ' 
        ' lblSs
        ' 
        lblSs.AutoSize = True
        lblSs.Dock = DockStyle.Fill
        lblSs.Location = New Point(1529, 0)
        lblSs.Margin = New Padding(4, 0, 4, 0)
        lblSs.Name = "lblSs"
        lblSs.Size = New Size(142, 48)
        lblSs.TabIndex = 4
        lblSs.Text = "Sursă/Sector:"
        lblSs.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' cboAn
        ' 
        cboAn.Dock = DockStyle.Fill
        cboAn.DrawMode = DrawMode.OwnerDrawFixed
        cboAn.DropDownStyle = ComboBoxStyle.DropDownList
        cboAn.FlatStyle = FlatStyle.Flat
        cboAn.Location = New Point(1329, 4)
        cboAn.Margin = New Padding(4, 4, 4, 0)
        cboAn.Name = "cboAn"
        cboAn.Size = New Size(142, 32)
        cboAn.TabIndex = 3
        ' 
        ' lblAn
        ' 
        lblAn.AutoSize = True
        lblAn.Dock = DockStyle.Fill
        lblAn.Location = New Point(1179, 0)
        lblAn.Margin = New Padding(4, 0, 4, 0)
        lblAn.Name = "lblAn"
        lblAn.Size = New Size(142, 48)
        lblAn.TabIndex = 2
        lblAn.Text = "An Date:"
        lblAn.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' lblUnit
        ' 
        lblUnit.AutoSize = True
        lblUnit.Dock = DockStyle.Fill
        lblUnit.Font = New Font("Segoe UI Semibold", 10F)
        lblUnit.Location = New Point(4, 0)
        lblUnit.Margin = New Padding(4, 0, 4, 0)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(1117, 48)
        lblUnit.TabIndex = 1
        lblUnit.Text = "Unitate"
        lblUnit.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' busyBar
        ' 
        busyBar.Dock = DockStyle.Top
        busyBar.Location = New Point(0, 67)
        busyBar.Margin = New Padding(4, 5, 4, 5)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1827, 5)
        busyBar.TabIndex = 3
        busyBar.TabStop = False
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = My.Resources.Resources.settings__1_
        capBar.OptionButtonPadding = 2
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.ShowOptionsButton = True
        capBar.ShowThemeButton = True
        capBar.Size = New Size(1827, 67)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        capBar.TintOptionButtonImage = False
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1829, 1267)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(1571, 1067)
        Name = "MainForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT"
        pnlRoot.ResumeLayout(False)
        pnlWork.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        pnlTree.ResumeLayout(False)
        pnlTreeHead.ResumeLayout(False)
        pnlTreeHead.PerformLayout()
        CType(navViews, ComponentModel.ISupportInitialize).EndInit()
        pnlStatus.ResumeLayout(False)
        pnlStatus.PerformLayout()
        pnlHeader.ResumeLayout(False)
        tlyHeader.ResumeLayout(False)
        tlyHeader.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Controls.KBotBusyBar
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents lblOperator As Label
    Friend WithEvents lblProgram As Label
    Friend WithEvents btnSinc As Button
    Friend WithEvents pnlWork As Panel
    Friend WithEvents navViews As KBot.Controls.KBotNavList
    Friend WithEvents split As SplitContainer
    Friend WithEvents pnlTree As Panel
    Friend WithEvents pnlTreeHead As Panel
    Friend WithEvents lblTree As Label
    Friend WithEvents btnInfo As Button
    Friend WithEvents btnSort As Button
    Friend WithEvents btnOpt As Button
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents viewHost As Panel
    Friend WithEvents tlyHeader As TableLayoutPanel
    Friend WithEvents cboSs As Controls.KBotComboBox
    Friend WithEvents lblSs As Label
    Friend WithEvents cboAn As Controls.KBotComboBox
    Friend WithEvents lblAn As Label
    Friend WithEvents lblUnit As Label
    Friend WithEvents lblForexe As Label
End Class
