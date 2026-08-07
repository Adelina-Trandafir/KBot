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
        split = New SplitContainer()
        grid = New Controls.KBotDataView()
        pnlDetaliu = New Panel()
        detailTable = New TableLayoutPanel()
        lblCapDescriere = New Label()
        lblCapValori = New Label()
        txtDescriere = New TextBox()
        gridValori = New Controls.KBotDataView()
        pnlFiltre = New Panel()
        TableLayoutPanel1 = New TableLayoutPanel()
        btnFiltruClsf = New Button()
        lblFiltruActiv = New Label()
        btnReset = New Button()
        btnFiltruData = New Button()
        btnFiltruTipRand = New Button()
        lblEmpty = New Label()
        menuClsf = New ContextMenuStrip(components)
        menuTipRand = New ContextMenuStrip(components)
        menuData = New ContextMenuStrip(components)
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
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 52)
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
        split.Size = New Size(986, 515)
        split.SplitterDistance = 349
        split.SplitterWidth = 9
        split.TabIndex = 1
        ' 
        ' grid
        ' 
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.ScrollByColumn = True
        grid.Size = New Size(986, 349)
        grid.TabIndex = 0
        ' 
        ' pnlDetaliu
        ' 
        pnlDetaliu.Controls.Add(detailTable)
        pnlDetaliu.Dock = DockStyle.Fill
        pnlDetaliu.Location = New Point(0, 0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Padding = New Padding(6)
        pnlDetaliu.Size = New Size(986, 157)
        pnlDetaliu.TabIndex = 0
        pnlDetaliu.Tag = "Card"
        ' 
        ' detailTable
        ' 
        detailTable.ColumnCount = 2
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        detailTable.Controls.Add(lblCapDescriere, 0, 0)
        detailTable.Controls.Add(lblCapValori, 1, 0)
        detailTable.Controls.Add(txtDescriere, 0, 1)
        detailTable.Controls.Add(gridValori, 1, 1)
        detailTable.Dock = DockStyle.Fill
        detailTable.Location = New Point(6, 6)
        detailTable.Name = "detailTable"
        detailTable.RowCount = 2
        detailTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 22F))
        detailTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        detailTable.Size = New Size(974, 145)
        detailTable.TabIndex = 0
        ' 
        ' lblCapDescriere
        ' 
        lblCapDescriere.AutoSize = True
        lblCapDescriere.Dock = DockStyle.Fill
        lblCapDescriere.Location = New Point(3, 0)
        lblCapDescriere.Name = "lblCapDescriere"
        lblCapDescriere.Size = New Size(481, 22)
        lblCapDescriere.TabIndex = 0
        lblCapDescriere.Text = "Descriere"
        lblCapDescriere.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCapValori
        ' 
        lblCapValori.AutoSize = True
        lblCapValori.Dock = DockStyle.Fill
        lblCapValori.Location = New Point(490, 0)
        lblCapValori.Name = "lblCapValori"
        lblCapValori.Size = New Size(481, 22)
        lblCapValori.TabIndex = 1
        lblCapValori.Text = "Valori"
        lblCapValori.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtDescriere
        ' 
        txtDescriere.Dock = DockStyle.Fill
        txtDescriere.Location = New Point(0, 22)
        txtDescriere.Margin = New Padding(0, 0, 3, 0)
        txtDescriere.Multiline = True
        txtDescriere.Name = "txtDescriere"
        txtDescriere.ReadOnly = True
        txtDescriere.ScrollBars = ScrollBars.Vertical
        txtDescriere.Size = New Size(484, 123)
        txtDescriere.TabIndex = 2
        ' 
        ' gridValori
        ' 
        gridValori.BackColor = SystemColors.Window
        gridValori.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        gridValori.Dock = DockStyle.Fill
        gridValori.HeaderHeight = 26
        gridValori.Location = New Point(490, 22)
        gridValori.Margin = New Padding(3, 0, 0, 0)
        gridValori.Name = "gridValori"
        gridValori.ReadOnlyGrid = True
        gridValori.RowHeight = 26
        gridValori.Size = New Size(484, 123)
        gridValori.TabIndex = 3
        ' 
        ' pnlFiltre
        ' 
        pnlFiltre.Controls.Add(TableLayoutPanel1)
        pnlFiltre.Controls.Add(btnReset)
        pnlFiltre.Dock = DockStyle.Top
        pnlFiltre.Location = New Point(0, 0)
        pnlFiltre.Name = "pnlFiltre"
        pnlFiltre.Size = New Size(986, 52)
        pnlFiltre.TabIndex = 0
        pnlFiltre.Tag = "Card"
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
        ' btnFiltruClsf
        ' 
        btnFiltruClsf.AutoSize = True
        btnFiltruClsf.Dock = DockStyle.Fill
        btnFiltruClsf.Location = New Point(3, 3)
        btnFiltruClsf.Name = "btnFiltruClsf"
        btnFiltruClsf.Size = New Size(144, 46)
        btnFiltruClsf.TabIndex = 1
        btnFiltruClsf.Text = "Clasificație ▾"
        btnFiltruClsf.UseVisualStyleBackColor = True
        ' 
        ' lblFiltruActiv
        ' 
        lblFiltruActiv.Dock = DockStyle.Fill
        lblFiltruActiv.Location = New Point(473, 0)
        lblFiltruActiv.Name = "lblFiltruActiv"
        lblFiltruActiv.Size = New Size(510, 52)
        lblFiltruActiv.TabIndex = 3
        lblFiltruActiv.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnReset
        ' 
        btnReset.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnReset.Location = New Point(1688, 5)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(78, 26)
        btnReset.TabIndex = 4
        btnReset.Text = "Reset"
        btnReset.UseVisualStyleBackColor = True
        ' 
        ' btnFiltruData
        ' 
        btnFiltruData.AutoSize = True
        btnFiltruData.Dock = DockStyle.Fill
        btnFiltruData.Location = New Point(303, 3)
        btnFiltruData.Name = "btnFiltruData"
        btnFiltruData.Size = New Size(144, 46)
        btnFiltruData.TabIndex = 2
        btnFiltruData.Text = "Data ▾"
        btnFiltruData.UseVisualStyleBackColor = True
        ' 
        ' btnFiltruTipRand
        ' 
        btnFiltruTipRand.AutoSize = True
        btnFiltruTipRand.Dock = DockStyle.Fill
        btnFiltruTipRand.Location = New Point(153, 3)
        btnFiltruTipRand.Name = "btnFiltruTipRand"
        btnFiltruTipRand.Size = New Size(144, 46)
        btnFiltruTipRand.TabIndex = 1
        btnFiltruTipRand.Text = "Tip rând ▾"
        btnFiltruTipRand.UseVisualStyleBackColor = True
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 567)
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
        Controls.Add(split)
        Controls.Add(pnlFiltre)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "IstoricView"
        Size = New Size(986, 567)
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

    Friend WithEvents split As SplitContainer
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlDetaliu As Panel
    Friend WithEvents detailTable As TableLayoutPanel
    Friend WithEvents lblCapDescriere As Label
    Friend WithEvents lblCapValori As Label
    Friend WithEvents txtDescriere As TextBox
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
