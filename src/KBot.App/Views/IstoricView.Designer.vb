<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class IstoricView
    Inherits System.Windows.Forms.UserControl

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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        btnFiltruClsf = New Button()
        btnFiltruTipRand = New Button()
        btnFiltruData = New Button()
        btnReset = New Button()
        splitTree = New SplitContainer()
        tree = New Controls.AdvancedTreeControl()
        split = New SplitContainer()
        grid = New Controls.KBotDataView()
        pnlDetaliu = New Panel()
        detailTable = New TableLayoutPanel()
        lblCapDescriere = New Controls.KBotLabel()
        txtDescriere = New Controls.KBotTextBox()
        gridValori = New Controls.KBotDataView()
        pnlFiltre = New Panel()
        TableLayoutPanel1 = New TableLayoutPanel()
        lblFiltruActiv = New Label()
        lblEmpty = New Label()
        menuClsf = New ContextMenuStrip(components)
        menuTipRand = New ContextMenuStrip(components)
        menuData = New ContextMenuStrip(components)
        CType(splitTree, ComponentModel.ISupportInitialize).BeginInit()
        splitTree.Panel1.SuspendLayout()
        splitTree.Panel2.SuspendLayout()
        splitTree.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlDetaliu.SuspendLayout()
        detailTable.SuspendLayout()
        CType(gridValori, ComponentModel.ISupportInitialize).BeginInit()
        pnlFiltre.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnFiltruClsf
        ' 
        btnFiltruClsf.AutoSize = True
        btnFiltruClsf.Dock = DockStyle.Fill
        btnFiltruClsf.Location = New Point(2, 2)
        btnFiltruClsf.Margin = New Padding(2)
        btnFiltruClsf.Name = "btnFiltruClsf"
        btnFiltruClsf.Size = New Size(146, 48)
        btnFiltruClsf.TabIndex = 1
        btnFiltruClsf.Text = "Clasificație ▾"
        tips.SetToolTipHeader(btnFiltruClsf, "Filtru: clasificații")
        tips.SetToolTipText(btnFiltruClsf, "Restrânge lista la clasificațiile bifate." & vbLf & "Fără nicio bifă, se văd toate.")
        btnFiltruClsf.UseVisualStyleBackColor = True
        ' 
        ' btnFiltruTipRand
        ' 
        btnFiltruTipRand.AutoSize = True
        btnFiltruTipRand.Dock = DockStyle.Fill
        btnFiltruTipRand.Location = New Point(152, 2)
        btnFiltruTipRand.Margin = New Padding(2)
        btnFiltruTipRand.Name = "btnFiltruTipRand"
        btnFiltruTipRand.Size = New Size(146, 48)
        btnFiltruTipRand.TabIndex = 1
        btnFiltruTipRand.Text = "Tip rând ▾"
        tips.SetToolTipHeader(btnFiltruTipRand, "Filtru: tip rând")
        tips.SetToolTipText(btnFiltruTipRand, "Arată doar rândurile de tipul ales (angajament, plată, recepție…).")
        btnFiltruTipRand.UseVisualStyleBackColor = True
        ' 
        ' btnFiltruData
        ' 
        btnFiltruData.AutoSize = True
        btnFiltruData.Dock = DockStyle.Fill
        btnFiltruData.Location = New Point(302, 2)
        btnFiltruData.Margin = New Padding(2)
        btnFiltruData.Name = "btnFiltruData"
        btnFiltruData.Size = New Size(146, 48)
        btnFiltruData.TabIndex = 2
        btnFiltruData.Text = "Data ▾"
        tips.SetToolTipHeader(btnFiltruData, "Filtru: dată FX")
        tips.SetToolTipText(btnFiltruData, "Restrânge istoricul la datele FX bifate.")
        btnFiltruData.UseVisualStyleBackColor = True
        ' 
        ' btnReset
        ' 
        btnReset.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnReset.Location = New Point(1688, 5)
        btnReset.Margin = New Padding(2)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(78, 26)
        btnReset.TabIndex = 4
        btnReset.Text = "Reset"
        tips.SetToolTipHeader(btnReset, "Șterge filtrele")
        tips.SetToolTipText(btnReset, "Renunță la toate filtrele și readuce istoricul întreg.")
        btnReset.UseVisualStyleBackColor = True
        ' 
        ' splitTree
        ' 
        splitTree.Dock = DockStyle.Fill
        splitTree.Location = New Point(0, 0)
        splitTree.Margin = New Padding(4, 5, 4, 5)
        splitTree.Name = "splitTree"
        ' 
        ' splitTree.Panel1
        ' 
        splitTree.Panel1.Controls.Add(tree)
        ' 
        ' splitTree.Panel2
        ' 
        splitTree.Panel2.Controls.Add(split)
        splitTree.Size = New Size(986, 568)
        splitTree.SplitterDistance = 259
        splitTree.SplitterWidth = 9
        splitTree.TabIndex = 3
        ' 
        ' tree
        ' 
        tree.BorderColor = SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = DockStyle.Fill
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Perioade"
        tree.FooterCaptionFont = New Font("Consolas", 8F, FontStyle.Bold)
        tree.FooterCollapseButton = True
        tree.FooterCollapseButtonPosition = KBot.Controls.AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 40
        tree.FooterIconSize = New Size(24, 24)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterRightIconTooltip = "Reîncarcă istoricul angajamentului de la server."
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = KBot.Controls.AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " ISTORIC"
        tree.HeaderForeColor = SystemColors.ActiveCaptionText
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderSearchIconTooltip = "Caută în arbore." & vbLf & "ESC golește căutarea și închide banda."
        tree.HeaderSeparatorColor = Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 20
        tree.LeftIconSize = New Size(14, 14)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 10
        tree.PaddingIconGap = 10
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = True
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 60
        tree.Size = New Size(259, 568)
        tree.TabIndex = 0
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        split.Orientation = Orientation.Horizontal
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(grid)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(pnlDetaliu)
        split.Size = New Size(718, 568)
        split.SplitterDistance = 384
        split.SplitterWidth = 9
        split.TabIndex = 1
        ' 
        ' grid
        ' 
        grid.AlternatingRows = False
        grid.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grid.BackColor = SystemColors.Window
        grid.BorderColor = SystemColors.ActiveBorder
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBot.Controls.KBotAutoSizeMode.None
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Clasificația"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 130
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBot.Controls.KBotAutoSizeMode.None
        KBotDataColumn2.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Tipul"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "tip"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.ShowColumnFilter = True
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBot.Controls.KBotAutoSizeMode.None
        KBotDataColumn3.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Data"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "data"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.ValueType = KBot.Controls.KBotValueType.DateTime
        KBotDataColumn3.Width = 85
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBot.Controls.KBotAutoSizeMode.None
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Descriere"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "desc"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 120
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Dock = DockStyle.Fill
        grid.EnableGrouping = True
        grid.FilterIconSize = New Size(14, 14)
        grid.FilterIconTooltip = "Filtrează coloana." & vbLf & "Bifează valorile pe care vrei să le vezi; fără nicio bifă, se văd toate."
        grid.FooterBackColor = SystemColors.Control
        grid.FooterForeColor = SystemColors.ActiveCaptionText
        grid.FrozenColumnCount = 1
        grid.HeaderBackColor = SystemColors.Control
        grid.HeaderColumnSeparatorColor = Color.Gainsboro
        grid.HeaderForeColor = SystemColors.ActiveCaptionText
        grid.HeaderSeparatorColor = Color.Gainsboro
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 22
        grid.ScrollByColumn = True
        grid.ShrinkColumnsToFit = False
        grid.Size = New Size(718, 384)
        grid.TabIndex = 0
        ' 
        ' pnlDetaliu
        ' 
        pnlDetaliu.Controls.Add(detailTable)
        pnlDetaliu.Dock = DockStyle.Fill
        pnlDetaliu.Location = New Point(0, 0)
        pnlDetaliu.Margin = New Padding(0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Size = New Size(718, 175)
        pnlDetaliu.TabIndex = 0
        pnlDetaliu.Tag = "Card"
        ' 
        ' detailTable
        ' 
        detailTable.ColumnCount = 2
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        detailTable.Controls.Add(lblCapDescriere, 0, 0)
        detailTable.Controls.Add(txtDescriere, 0, 1)
        detailTable.Controls.Add(gridValori, 1, 0)
        detailTable.Dock = DockStyle.Fill
        detailTable.Location = New Point(0, 0)
        detailTable.Margin = New Padding(0)
        detailTable.Name = "detailTable"
        detailTable.RowCount = 2
        detailTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 25F))
        detailTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        detailTable.Size = New Size(718, 175)
        detailTable.TabIndex = 0
        ' 
        ' lblCapDescriere
        ' 
        lblCapDescriere.AutoSize = True
        lblCapDescriere.BorderColor = SystemColors.ActiveBorder
        lblCapDescriere.Dock = DockStyle.Fill
        lblCapDescriere.FlatStyle = FlatStyle.Flat
        lblCapDescriere.Font = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblCapDescriere.Location = New Point(0, 0)
        lblCapDescriere.Margin = New Padding(0)
        lblCapDescriere.Name = "lblCapDescriere"
        lblCapDescriere.Size = New Size(359, 25)
        lblCapDescriere.TabIndex = 0
        lblCapDescriere.Text = "Observații"
        lblCapDescriere.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' txtDescriere
        ' 
        txtDescriere.BackColor = SystemColors.Window
        txtDescriere.BorderColor = SystemColors.ActiveBorder
        txtDescriere.CornerRadius = 0
        txtDescriere.Dock = DockStyle.Fill
        txtDescriere.Location = New Point(0, 25)
        txtDescriere.Margin = New Padding(0)
        txtDescriere.Name = "txtDescriere"
        txtDescriere.ReadOnly = True
        txtDescriere.Size = New Size(359, 150)
        txtDescriere.TabIndex = 2
        txtDescriere.TabStop = False
        ' 
        ' gridValori
        ' 
        gridValori.AlternatingRows = False
        gridValori.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridValori.AutoSizeHeaderHeight = False
        gridValori.BackColor = SystemColors.Window
        gridValori.BorderColor = SystemColors.ActiveBorder
        gridValori.CellTooltip.Enabled = False
        gridValori.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBot.Controls.KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "TIP"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "vtip"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 140
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Valoare"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "vval"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Resizable = False
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn6.Width = 90
        gridValori.Columns.Add(KBotDataColumn5)
        gridValori.Columns.Add(KBotDataColumn6)
        gridValori.Dock = DockStyle.Fill
        gridValori.HeaderBackColor = SystemColors.Control
        gridValori.HeaderColumnSeparatorColor = Color.Gainsboro
        gridValori.HeaderForeColor = SystemColors.ActiveCaptionText
        gridValori.HeaderHeight = 25
        gridValori.HeaderSeparatorColor = Color.Gainsboro
        gridValori.Location = New Point(361, 0)
        gridValori.Margin = New Padding(2, 0, 0, 0)
        gridValori.Name = "gridValori"
        gridValori.ReadOnlyGrid = True
        gridValori.RowHeight = 22
        detailTable.SetRowSpan(gridValori, 2)
        gridValori.Size = New Size(357, 175)
        gridValori.TabIndex = 3
        ' 
        ' pnlFiltre
        ' 
        pnlFiltre.Controls.Add(TableLayoutPanel1)
        pnlFiltre.Controls.Add(btnReset)
        pnlFiltre.Location = New Point(0, 0)
        pnlFiltre.Margin = New Padding(2)
        pnlFiltre.Name = "pnlFiltre"
        pnlFiltre.Size = New Size(986, 52)
        pnlFiltre.TabIndex = 0
        pnlFiltre.Tag = "Card"
        pnlFiltre.Visible = False
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 5
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 20F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.Controls.Add(btnFiltruClsf, 0, 0)
        TableLayoutPanel1.Controls.Add(btnFiltruTipRand, 1, 0)
        TableLayoutPanel1.Controls.Add(btnFiltruData, 2, 0)
        TableLayoutPanel1.Controls.Add(lblFiltruActiv, 4, 0)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Margin = New Padding(0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 1
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.Size = New Size(986, 52)
        TableLayoutPanel1.TabIndex = 5
        ' 
        ' lblFiltruActiv
        ' 
        lblFiltruActiv.Dock = DockStyle.Fill
        lblFiltruActiv.Location = New Point(472, 0)
        lblFiltruActiv.Margin = New Padding(2, 0, 2, 0)
        lblFiltruActiv.Name = "lblFiltruActiv"
        lblFiltruActiv.Size = New Size(512, 52)
        lblFiltruActiv.TabIndex = 3
        lblFiltruActiv.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(2, 0, 2, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 568)
        lblEmpty.TabIndex = 2
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' menuClsf
        ' 
        menuClsf.ImageScalingSize = New Size(24, 24)
        menuClsf.Name = "menuClsf"
        menuClsf.Size = New Size(61, 4)
        ' 
        ' menuTipRand
        ' 
        menuTipRand.ImageScalingSize = New Size(24, 24)
        menuTipRand.Name = "menuTipRand"
        menuTipRand.Size = New Size(61, 4)
        ' 
        ' menuData
        ' 
        menuData.ImageScalingSize = New Size(24, 24)
        menuData.Name = "menuData"
        menuData.Size = New Size(61, 4)
        ' 
        ' IstoricView
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(splitTree)
        Controls.Add(pnlFiltre)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "IstoricView"
        Size = New Size(986, 568)
        splitTree.Panel1.ResumeLayout(False)
        splitTree.Panel2.ResumeLayout(False)
        CType(splitTree, ComponentModel.ISupportInitialize).EndInit()
        splitTree.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlDetaliu.ResumeLayout(False)
        detailTable.ResumeLayout(False)
        detailTable.PerformLayout()
        CType(gridValori, ComponentModel.ISupportInitialize).EndInit()
        pnlFiltre.ResumeLayout(False)
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents splitTree As SplitContainer
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents split As SplitContainer
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlDetaliu As Panel
    Friend WithEvents detailTable As TableLayoutPanel
    Friend WithEvents lblCapDescriere As KBot.Controls.KBotLabel
    Friend WithEvents txtDescriere As KBot.Controls.KBotTextBox
    Friend WithEvents gridValori As KBot.Controls.KBotDataView
    Friend WithEvents pnlFiltre As Panel
    Friend WithEvents btnFiltruTipRand As Button
    Friend WithEvents btnFiltruData As Button
    Friend WithEvents lblFiltruActiv As Label
    Friend WithEvents btnReset As Button
    Friend WithEvents lblEmpty As Label
    Friend WithEvents menuClsf As ContextMenuStrip
    Friend WithEvents menuTipRand As ContextMenuStrip
    Friend WithEvents menuData As ContextMenuStrip
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents btnFiltruClsf As Button
End Class
