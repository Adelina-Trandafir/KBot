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
        Dim KBotNavItem1 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Dim KBotNavItem2 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem3 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem4 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem5 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem6 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem7 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem8 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
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
        navViews = New Theming.KBotNavList()
        pnlStatus = New Panel()
        lblOperator = New Label()
        lblProgram = New Label()
        btnIstoric = New Button()
        btnSinc = New Button()
        pnlHeader = New Panel()
        lblUnit = New Label()
        lblAn = New Label()
        cboAn = New ComboBox()
        lblSs = New Label()
        cboSs = New ComboBox()
        lblForexe = New Label()
        busyBar = New Theming.KBotBusyBar()
        capBar = New Theming.KBotCaptionBar()
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
        pnlWork.Location = New Point(0, 139)
        pnlWork.Margin = New Padding(4, 5, 4, 5)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(11, 13, 11, 13)
        pnlWork.Size = New Size(1827, 1051)
        pnlWork.TabIndex = 0
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(280, 13)
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
        split.Size = New Size(1536, 1025)
        split.SplitterDistance = 533
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
        pnlTree.Size = New Size(522, 1025)
        pnlTree.TabIndex = 0
        pnlTree.Tag = "Card"
        ' 
        ' tree
        ' 
        tree.AutoScrollMinSize = New Size(0, 0)
        tree.BackColor = Color.White
        tree.BorderColor = Color.Transparent
        tree.Dock = DockStyle.Fill
        tree.Font = New Font("Segoe UI", 9F)
        tree.HeaderBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.HeaderForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.HeaderIconSize = New Size(16, 16)
        tree.HoverBackColor = Color.FromArgb(CByte(230), CByte(240), CByte(255))
        tree.LeftIconSize = New Size(18, 18)
        tree.LineColor = Color.FromArgb(CByte(160), CByte(160), CByte(160))
        tree.Location = New Point(0, 60)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.Name = "tree"
        tree.RightIconSize = New Size(18, 18)
        tree.SearchBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.SearchBarFontSize = 10F
        tree.SearchBarLabelForeColor = Color.Empty
        tree.SearchBoxBackColor = Color.Empty
        tree.SelectedBackColor = Color.FromArgb(CByte(200), CByte(220), CByte(255))
        tree.SelectedBorderColor = Color.FromArgb(CByte(150), CByte(180), CByte(255))
        tree.Size = New Size(522, 965)
        tree.TabIndex = 0
        tree.TooltipBackColor = Color.FromArgb(CByte(255), CByte(255), CByte(232))
        tree.TooltipForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.TreeFont = New Font("Consolas", 9F)
        ' 
        ' pnlTreeHead
        ' 
        pnlTreeHead.Controls.Add(lblTree)
        pnlTreeHead.Controls.Add(btnInfo)
        pnlTreeHead.Controls.Add(btnSort)
        pnlTreeHead.Controls.Add(btnOpt)
        pnlTreeHead.Dock = DockStyle.Top
        pnlTreeHead.Location = New Point(0, 0)
        pnlTreeHead.Margin = New Padding(4, 5, 4, 5)
        pnlTreeHead.Name = "pnlTreeHead"
        pnlTreeHead.Size = New Size(522, 60)
        pnlTreeHead.TabIndex = 1
        pnlTreeHead.Tag = "Card"
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
        viewHost.Size = New Size(983, 1025)
        viewHost.TabIndex = 0
        viewHost.Tag = "Card"
        ' 
        ' navViews
        ' 
        navViews.Dock = DockStyle.Left
        KBotNavItem1.Image = CType(resources.GetObject("KBotNavItem1.Image"), Image)
        KBotNavItem1.Key = "sumar"
        KBotNavItem1.Text = "Sumar"
        KBotNavItem2.Image = KBot.App.Resources.calendar
        KBotNavItem2.Key = "istoric"
        KBotNavItem2.Text = "Istoric"
        KBotNavItem3.Image = KBot.App.Resources.database
        KBotNavItem3.Key = "rezervari"
        KBotNavItem3.Text = "Rezervări"
        KBotNavItem4.Image = KBot.App.Resources.binvoice
        KBotNavItem4.Key = "receptii"
        KBotNavItem4.Text = "Recepții"
        KBotNavItem5.Image = KBot.App.Resources.credit_card
        KBotNavItem5.Key = "plati"
        KBotNavItem5.Text = "Plăți"
        KBotNavItem6.Align = Theming.KBotNavAlign.Far
        KBotNavItem6.IsSeparator = True
        KBotNavItem6.Key = "__sep_1"
        KBotNavItem6.Text = Nothing
        KBotNavItem7.Align = Theming.KBotNavAlign.Far
        KBotNavItem7.Image = KBot.App.Resources.Umut_Pulat_Tulliana_2_File_temporary_32
        KBotNavItem7.Key = "ddf"
        KBotNavItem7.Text = "Doc. Fundamentare"
        KBotNavItem8.Align = Theming.KBotNavAlign.Far
        KBotNavItem8.Image = KBot.App.Resources.Umut_Pulat_Tulliana_2_File_locked_32
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
        navViews.Size = New Size(269, 1025)
        navViews.TabIndex = 0
        ' 
        ' pnlStatus
        ' 
        pnlStatus.Controls.Add(lblOperator)
        pnlStatus.Controls.Add(lblProgram)
        pnlStatus.Controls.Add(btnIstoric)
        pnlStatus.Controls.Add(btnSinc)
        pnlStatus.Dock = DockStyle.Bottom
        pnlStatus.Location = New Point(0, 1190)
        pnlStatus.Margin = New Padding(4, 5, 4, 5)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(1827, 73)
        pnlStatus.TabIndex = 1
        pnlStatus.Tag = "Card"
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
        lblProgram.Location = New Point(543, 23)
        lblProgram.Margin = New Padding(4, 0, 4, 0)
        lblProgram.Name = "lblProgram"
        lblProgram.Size = New Size(81, 25)
        lblProgram.TabIndex = 1
        lblProgram.Text = "Program"
        ' 
        ' btnIstoric
        ' 
        btnIstoric.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnIstoric.FlatStyle = FlatStyle.Flat
        btnIstoric.Location = New Point(1424, 10)
        btnIstoric.Margin = New Padding(4, 5, 4, 5)
        btnIstoric.Name = "btnIstoric"
        btnIstoric.Size = New Size(157, 53)
        btnIstoric.TabIndex = 2
        btnIstoric.Text = "Istoric"
        btnIstoric.UseVisualStyleBackColor = True
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
        pnlHeader.Controls.Add(lblUnit)
        pnlHeader.Controls.Add(lblAn)
        pnlHeader.Controls.Add(cboAn)
        pnlHeader.Controls.Add(lblSs)
        pnlHeader.Controls.Add(cboSs)
        pnlHeader.Controls.Add(lblForexe)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 72)
        pnlHeader.Margin = New Padding(4, 5, 4, 5)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(1827, 67)
        pnlHeader.TabIndex = 2
        pnlHeader.Tag = "Card"
        ' 
        ' lblUnit
        ' 
        lblUnit.AutoSize = True
        lblUnit.Font = New Font("Segoe UI Semibold", 10F)
        lblUnit.Location = New Point(17, 17)
        lblUnit.Margin = New Padding(4, 0, 4, 0)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(78, 28)
        lblUnit.TabIndex = 0
        lblUnit.Text = "Unitate"
        ' 
        ' lblAn
        ' 
        lblAn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblAn.AutoSize = True
        lblAn.Location = New Point(1210, 20)
        lblAn.Margin = New Padding(4, 0, 4, 0)
        lblAn.Name = "lblAn"
        lblAn.Size = New Size(38, 25)
        lblAn.TabIndex = 1
        lblAn.Text = "An:"
        ' 
        ' cboAn
        ' 
        cboAn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboAn.DropDownStyle = ComboBoxStyle.DropDownList
        cboAn.FlatStyle = FlatStyle.Flat
        cboAn.Location = New Point(1258, 13)
        cboAn.Margin = New Padding(4, 5, 4, 5)
        cboAn.Name = "cboAn"
        cboAn.Size = New Size(101, 33)
        cboAn.TabIndex = 2
        ' 
        ' lblSs
        ' 
        lblSs.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblSs.AutoSize = True
        lblSs.Location = New Point(1381, 20)
        lblSs.Margin = New Padding(4, 0, 4, 0)
        lblSs.Name = "lblSs"
        lblSs.Size = New Size(36, 25)
        lblSs.TabIndex = 3
        lblSs.Text = "SS:"
        ' 
        ' cboSs
        ' 
        cboSs.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        cboSs.DropDownStyle = ComboBoxStyle.DropDownList
        cboSs.FlatStyle = FlatStyle.Flat
        cboSs.Location = New Point(1430, 13)
        cboSs.Margin = New Padding(4, 5, 4, 5)
        cboSs.Name = "cboSs"
        cboSs.Size = New Size(127, 33)
        cboSs.TabIndex = 4
        ' 
        ' lblForexe
        ' 
        lblForexe.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblForexe.Location = New Point(1581, 20)
        lblForexe.Margin = New Padding(4, 0, 4, 0)
        lblForexe.Name = "lblForexe"
        lblForexe.Size = New Size(229, 25)
        lblForexe.TabIndex = 5
        lblForexe.Text = "● Forexe: neconectat"
        lblForexe.TextAlign = ContentAlignment.MiddleRight
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
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1827, 67)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        ' 
        ' MainForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1829, 1267)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
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
        pnlHeader.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBot.Theming.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Theming.KBotBusyBar
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblUnit As Label
    Friend WithEvents lblAn As Label
    Friend WithEvents cboAn As ComboBox
    Friend WithEvents lblSs As Label
    Friend WithEvents cboSs As ComboBox
    Friend WithEvents lblForexe As Label
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents lblOperator As Label
    Friend WithEvents lblProgram As Label
    Friend WithEvents btnIstoric As Button
    Friend WithEvents btnSinc As Button
    Friend WithEvents pnlWork As Panel
    Friend WithEvents navViews As KBot.Theming.KBotNavList
    Friend WithEvents split As SplitContainer
    Friend WithEvents pnlTree As Panel
    Friend WithEvents pnlTreeHead As Panel
    Friend WithEvents lblTree As Label
    Friend WithEvents btnInfo As Button
    Friend WithEvents btnSort As Button
    Friend WithEvents btnOpt As Button
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents viewHost As Panel
End Class
