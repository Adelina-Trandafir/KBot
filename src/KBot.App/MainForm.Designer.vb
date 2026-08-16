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
        components = New ComponentModel.Container()
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
        tips = New KBot.Controls.KBotToolTip(components)
        btnInfo = New Button()
        btnSort = New Button()
        btnOpt = New Button()
        cboAn = New Controls.KBotComboBox()
        cboSs = New Controls.KBotComboBox()
        pnlRoot = New Panel()
        pnlWork = New Panel()
        split = New SplitContainer()
        pnlTree = New Panel()
        tree = New Controls.AdvancedTreeControl()
        pnlTreeHead = New Panel()
        lblTree = New Label()
        viewHost = New Panel()
        navViews = New Controls.KBotNavList()
        pnlStatus = New Panel()
        forexeFooter = New ForexeFooterView()
        pnlHeader = New Panel()
        tlyHeader = New TableLayoutPanel()
        lblOperator = New Label()
        lblSs = New Label()
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
        ' btnInfo
        ' 
        btnInfo.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnInfo.FlatStyle = FlatStyle.Flat
        btnInfo.Location = New Point(302, 5)
        btnInfo.Margin = New Padding(3, 4, 3, 4)
        btnInfo.Name = "btnInfo"
        btnInfo.Size = New Size(32, 37)
        btnInfo.TabIndex = 1
        btnInfo.Text = "ⓘ"
        tips.SetToolTipHeader(btnInfo, "Informații")
        tips.SetToolTipText(btnInfo, "Deschide fereastra cu datele interne ale sesiunii:" & vbLf & "operator, unitate, an/subperioadă, versiuni de componente.")
        btnInfo.UseVisualStyleBackColor = True
        ' 
        ' btnSort
        ' 
        btnSort.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSort.FlatStyle = FlatStyle.Flat
        btnSort.Location = New Point(341, 5)
        btnSort.Margin = New Padding(3, 4, 3, 4)
        btnSort.Name = "btnSort"
        btnSort.Size = New Size(32, 37)
        btnSort.TabIndex = 1
        btnSort.Text = "↕"
        tips.SetToolTipHeader(btnSort, "Sortare arbore")
        tips.SetToolTipText(btnSort, "Schimbă ordinea în care se așază angajamentele în arbore.")
        btnSort.UseVisualStyleBackColor = True
        ' 
        ' btnOpt
        ' 
        btnOpt.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnOpt.FlatStyle = FlatStyle.Flat
        btnOpt.Location = New Point(379, 5)
        btnOpt.Margin = New Padding(3, 4, 3, 4)
        btnOpt.Name = "btnOpt"
        btnOpt.Size = New Size(32, 37)
        btnOpt.TabIndex = 2
        btnOpt.Text = "…"
        tips.SetToolTipHeader(btnOpt, "Opțiuni")
        tips.SetToolTipText(btnOpt, "Meniul de opțiuni al arborelui:" & vbLf & "arată/ascunde rândurile ascunse, jurnale, temă.")
        btnOpt.UseVisualStyleBackColor = True
        ' 
        ' cboAn
        ' 
        cboAn.Dock = DockStyle.Fill
        cboAn.DrawMode = DrawMode.OwnerDrawFixed
        cboAn.DropDownStyle = ComboBoxStyle.DropDownList
        cboAn.FlatStyle = FlatStyle.Flat
        cboAn.Location = New Point(959, 3)
        cboAn.Margin = New Padding(3, 3, 3, 0)
        cboAn.Name = "cboAn"
        cboAn.Size = New Size(114, 28)
        cboAn.TabIndex = 3
        tips.SetToolTipHeader(cboAn, "An")
        tips.SetToolTipText(cboAn, "Anul de lucru." & vbLf & "Schimbarea lui reîncarcă arborele și toate ecranele.")
        ' 
        ' cboSs
        ' 
        cboSs.Dock = DockStyle.Fill
        cboSs.DrawMode = DrawMode.OwnerDrawFixed
        cboSs.DropDownStyle = ComboBoxStyle.DropDownList
        cboSs.FlatStyle = FlatStyle.Flat
        cboSs.Location = New Point(1239, 3)
        cboSs.Margin = New Padding(3, 3, 3, 0)
        cboSs.Name = "cboSs"
        cboSs.Size = New Size(114, 28)
        cboSs.TabIndex = 5
        tips.SetToolTipHeader(cboSs, "Subperioadă")
        tips.SetToolTipText(cboSs, "Subperioada (SS) din anul ales." & vbLf & "Ultima aleasă se ține minte pentru data viitoare.")
        ' 
        ' pnlRoot
        ' 
        pnlRoot.Controls.Add(pnlWork)
        pnlRoot.Controls.Add(pnlStatus)
        pnlRoot.Controls.Add(pnlHeader)
        pnlRoot.Controls.Add(busyBar)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 1)
        pnlRoot.Margin = New Padding(3, 4, 3, 4)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1358, 854)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        ' 
        ' pnlWork
        ' 
        pnlWork.Controls.Add(split)
        pnlWork.Controls.Add(navViews)
        pnlWork.Dock = DockStyle.Fill
        pnlWork.Location = New Point(0, 98)
        pnlWork.Margin = New Padding(3, 4, 3, 4)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(9, 11, 9, 11)
        pnlWork.Size = New Size(1358, 697)
        pnlWork.TabIndex = 0
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(184, 11)
        split.Margin = New Padding(3, 4, 3, 4)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(pnlTree)
        split.Panel1.Padding = New Padding(9, 0, 0, 0)
        split.Panel1MinSize = 240
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(viewHost)
        split.Panel2.Padding = New Padding(9, 0, 0, 0)
        split.Panel2MinSize = 400
        split.Size = New Size(1165, 675)
        split.SplitterDistance = 343
        split.SplitterWidth = 7
        split.TabIndex = 1
        ' 
        ' pnlTree
        ' 
        pnlTree.Controls.Add(tree)
        pnlTree.Controls.Add(pnlTreeHead)
        pnlTree.Dock = DockStyle.Fill
        pnlTree.Location = New Point(9, 0)
        pnlTree.Margin = New Padding(3, 4, 3, 4)
        pnlTree.Name = "pnlTree"
        pnlTree.Size = New Size(334, 675)
        pnlTree.TabIndex = 0
        pnlTree.Tag = "Card"
        ' 
        ' tree
        ' 
        tree.BorderColor = SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.ColumnHeaderSeparatorColor = Color.Gainsboro
        tree.ColumnHeaderSeparatorWidth = 2
        tree.Dock = DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.ExpanderSize = 16
        tree.FlyoutDelay = 150
        tree.FlyoutSlideDuration = 100
        tree.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Actualizează"
        tree.FooterCaptionBackColor = SystemColors.Control
        tree.FooterCaptionFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.FooterCaptionForeColor = SystemColors.ActiveCaptionText
        tree.FooterHeight = 30
        tree.FooterLeftIcon = My.Resources.Resources.database
        tree.FooterLeftIconTooltip = "Sursa datelor: unitatea și perioada din care s-a încărcat arborele."
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterRightIconTooltip = "Actualizează lista de angajamente din FOREXE." & vbLf & "Se conectează întâi, dacă nu există sesiune."
        tree.FooterSeparatorColor = Color.Gainsboro
        tree.FooterSeparatorWidth = 2
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = KBot.Controls.AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " LISTĂ ANGAJAMENTE"
        tree.HeaderFont = New Font("Calibri", 10F, FontStyle.Bold)
        tree.HeaderForeColor = Color.Black
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderRightIcon = My.Resources.Resources.settings__1_
        tree.HeaderRightIconTooltip = "Setările arborelui: coloane, sortare și rânduri ascunse."
        tree.HeaderSearchIcon = My.Resources.Resources.Everaldo_Crystal_Clear_App_xmag_search_48
        tree.HeaderSearchIconTooltip = "Deschide banda de căutare peste arbore." & vbLf & "ESC golește căutarea și o închide."
        tree.HeaderSeparatorColor = Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.ItemHeight = 24
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(3, 4, 3, 4)
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
        tree.RightIconSize = New Size(16, 16)
        tree.RootExpander = False
        tree.ScrollBarTheme = KBot.Controls.AdvancedTreeControl.En_ScrollBarTheme.Default
        tree.SearchBarFont = New Font("Calibri", 9F)
        tree.SearchBoxBackColor = SystemColors.Control
        tree.SearchClearButton = True
        tree.SearchClearButtonHoverColor = SystemColors.Control
        tree.SearchDefaultText = "... tastează minim 3 caractere ..."
        tree.SearchIn = KBot.Controls.AdvancedTreeControl.En_Tree_SearchIn.SearchIn_Both
        tree.SearchSeparatorColor = Color.Gainsboro
        tree.ShowRightIconOnHover = True
        tree.Size = New Size(334, 675)
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
        pnlTreeHead.Margin = New Padding(3, 4, 3, 4)
        pnlTreeHead.Name = "pnlTreeHead"
        pnlTreeHead.Size = New Size(417, 48)
        pnlTreeHead.TabIndex = 1
        pnlTreeHead.Tag = "Card"
        pnlTreeHead.Visible = False
        ' 
        ' lblTree
        ' 
        lblTree.AutoSize = True
        lblTree.Font = New Font("Segoe UI Semibold", 10F)
        lblTree.Location = New Point(11, 11)
        lblTree.Name = "lblTree"
        lblTree.Size = New Size(112, 23)
        lblTree.TabIndex = 0
        lblTree.Text = "Angajamente"
        ' 
        ' viewHost
        ' 
        viewHost.Dock = DockStyle.Fill
        viewHost.Location = New Point(9, 0)
        viewHost.Margin = New Padding(3, 4, 3, 4)
        viewHost.Name = "viewHost"
        viewHost.Size = New Size(806, 675)
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
        navViews.Location = New Point(9, 11)
        navViews.Margin = New Padding(3, 4, 3, 4)
        navViews.Name = "navViews"
        navViews.SelectedKey = Nothing
        navViews.Size = New Size(175, 675)
        navViews.TabIndex = 0
        ' 
        ' pnlStatus
        ' 
        pnlStatus.Controls.Add(forexeFooter)
        pnlStatus.Dock = DockStyle.Bottom
        pnlStatus.Location = New Point(0, 795)
        pnlStatus.Margin = New Padding(3, 4, 3, 4)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(1358, 59)
        pnlStatus.TabIndex = 1
        pnlStatus.Tag = "Card"
        ' 
        ' forexeFooter
        ' 
        forexeFooter.Dock = DockStyle.Fill
        forexeFooter.Location = New Point(0, 0)
        forexeFooter.Margin = New Padding(0)
        forexeFooter.Name = "forexeFooter"
        forexeFooter.Padding = New Padding(7, 8, 7, 8)
        forexeFooter.Size = New Size(1358, 59)
        forexeFooter.TabIndex = 1
        ' 
        ' pnlHeader
        ' 
        pnlHeader.BorderStyle = BorderStyle.FixedSingle
        pnlHeader.Controls.Add(tlyHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 57)
        pnlHeader.Margin = New Padding(0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(1358, 41)
        pnlHeader.TabIndex = 2
        pnlHeader.Tag = "Card"
        ' 
        ' tlyHeader
        ' 
        tlyHeader.BackColor = Color.Transparent
        tlyHeader.ColumnCount = 8
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyHeader.Controls.Add(lblOperator, 1, 0)
        tlyHeader.Controls.Add(cboSs, 7, 0)
        tlyHeader.Controls.Add(lblSs, 6, 0)
        tlyHeader.Controls.Add(cboAn, 4, 0)
        tlyHeader.Controls.Add(lblAn, 3, 0)
        tlyHeader.Controls.Add(lblUnit, 0, 0)
        tlyHeader.Dock = DockStyle.Fill
        tlyHeader.Location = New Point(0, 0)
        tlyHeader.Margin = New Padding(0)
        tlyHeader.Name = "tlyHeader"
        tlyHeader.RowCount = 1
        tlyHeader.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyHeader.Size = New Size(1356, 39)
        tlyHeader.TabIndex = 6
        ' 
        ' lblOperator
        ' 
        lblOperator.Dock = DockStyle.Left
        lblOperator.Location = New Point(639, 0)
        lblOperator.Name = "lblOperator"
        lblOperator.Padding = New Padding(14, 0, 7, 0)
        lblOperator.Size = New Size(153, 39)
        lblOperator.TabIndex = 6
        lblOperator.Text = "Operator"
        lblOperator.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblSs
        ' 
        lblSs.AutoSize = True
        lblSs.Dock = DockStyle.Fill
        lblSs.Location = New Point(1119, 0)
        lblSs.Name = "lblSs"
        lblSs.Size = New Size(114, 39)
        lblSs.TabIndex = 4
        lblSs.Text = "Sursă/Sector:"
        lblSs.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' lblAn
        ' 
        lblAn.AutoSize = True
        lblAn.Dock = DockStyle.Fill
        lblAn.Location = New Point(839, 0)
        lblAn.Name = "lblAn"
        lblAn.Size = New Size(114, 39)
        lblAn.TabIndex = 2
        lblAn.Text = "An Date:"
        lblAn.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' lblUnit
        ' 
        lblUnit.AutoSize = True
        lblUnit.Dock = DockStyle.Fill
        lblUnit.Font = New Font("Calibri", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblUnit.Location = New Point(3, 0)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(630, 39)
        lblUnit.TabIndex = 1
        lblUnit.Text = "Unitate"
        lblUnit.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' busyBar
        ' 
        busyBar.Dock = DockStyle.Top
        busyBar.Location = New Point(0, 53)
        busyBar.Margin = New Padding(3, 4, 3, 4)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1358, 4)
        busyBar.TabIndex = 3
        busyBar.TabStop = False
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(3, 4, 3, 4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = My.Resources.Resources.settings__1_
        capBar.OptionButtonPadding = 2
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.ShowOptionsButton = True
        capBar.ShowThemeButton = True
        capBar.Size = New Size(1358, 53)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        capBar.TintOptionButtonImage = False
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1360, 856)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(3, 4, 3, 4)
        MinimumSize = New Size(1257, 800)
        Name = "MainForm"
        Padding = New Padding(1)
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
        pnlHeader.ResumeLayout(False)
        tlyHeader.ResumeLayout(False)
        tlyHeader.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Controls.KBotBusyBar
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents forexeFooter As ForexeFooterView
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
    Friend WithEvents lblOperator As Label
End Class
