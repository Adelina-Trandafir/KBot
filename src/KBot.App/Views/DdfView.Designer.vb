<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfView
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
        Dim KBotNavItem1 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem2 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem3 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        Dim KBotNavItem4 As KBot.Theming.KBotNavItem = New Theming.KBotNavItem()
        split = New SplitContainer()
        tree = New Controls.AdvancedTreeControl()
        pnlTreeHead = New Panel()
        lblTreeTitle = New Label()
        pnlPages = New Panel()
        pnlValori = New Panel()
        grid = New Controls.KBotDataView()
        pnlFilter = New Panel()
        cboClsf = New ComboBox()
        lblClsf = New Label()
        pnlPreview = New Panel()
        lblPreviewGol = New Label()
        pnlPdf = New Panel()
        pnlAdobe = New Panel()
        cboAdobeInst = New ComboBox()
        lblAdobeInst = New Label()
        cboAdobeMod = New ComboBox()
        lblAdobeMod = New Label()
        cboAdobeMotor = New ComboBox()
        lblAdobeMotor = New Label()
        pnlFisiere = New Panel()
        lblFisiereGol = New Label()
        navSub = New Theming.KBotNavList()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlTreeHead.SuspendLayout()
        pnlPages.SuspendLayout()
        pnlValori.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlFilter.SuspendLayout()
        pnlPreview.SuspendLayout()
        pnlPdf.SuspendLayout()
        pnlAdobe.SuspendLayout()
        pnlFisiere.SuspendLayout()
        CType(navSub, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tree)
        split.Panel1.Controls.Add(pnlTreeHead)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(pnlPages)
        split.Panel2.Controls.Add(navSub)
        split.Size = New Size(986, 567)
        split.SplitterDistance = 336
        split.SplitterWidth = 9
        split.TabIndex = 0
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
        tree.ItemHeight = 24
        tree.LeftIconSize = New Size(16, 16)
        tree.LineColor = Color.FromArgb(CByte(160), CByte(160), CByte(160))
        tree.Location = New Point(0, 28)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.Name = "tree"
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 110
        tree.SearchBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.SearchBarFontSize = 10F
        tree.SearchBarLabelForeColor = Color.Empty
        tree.SearchBoxBackColor = Color.Empty
        tree.SelectedBackColor = Color.FromArgb(CByte(200), CByte(220), CByte(255))
        tree.SelectedBorderColor = Color.FromArgb(CByte(150), CByte(180), CByte(255))
        tree.Size = New Size(336, 539)
        tree.TabIndex = 1
        tree.TooltipBackColor = Color.FromArgb(CByte(255), CByte(255), CByte(232))
        tree.TooltipForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.TreeFont = New Font("Consolas", 9F)
        ' 
        ' pnlTreeHead
        ' 
        pnlTreeHead.Controls.Add(lblTreeTitle)
        pnlTreeHead.Dock = DockStyle.Top
        pnlTreeHead.Location = New Point(0, 0)
        pnlTreeHead.Name = "pnlTreeHead"
        pnlTreeHead.Size = New Size(336, 28)
        pnlTreeHead.TabIndex = 0
        ' 
        ' lblTreeTitle
        ' 
        lblTreeTitle.Dock = DockStyle.Fill
        lblTreeTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblTreeTitle.Location = New Point(0, 0)
        lblTreeTitle.Name = "lblTreeTitle"
        lblTreeTitle.Padding = New Padding(6, 0, 0, 0)
        lblTreeTitle.Size = New Size(336, 28)
        lblTreeTitle.TabIndex = 0
        lblTreeTitle.Text = "Revizii"
        lblTreeTitle.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlPages
        ' 
        pnlPages.Controls.Add(pnlValori)
        pnlPages.Controls.Add(pnlPreview)
        pnlPages.Controls.Add(pnlPdf)
        pnlPages.Controls.Add(pnlFisiere)
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 49)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(641, 518)
        pnlPages.TabIndex = 1
        ' 
        ' pnlValori
        ' 
        pnlValori.Controls.Add(grid)
        pnlValori.Controls.Add(pnlFilter)
        pnlValori.Dock = DockStyle.Fill
        pnlValori.Location = New Point(0, 0)
        pnlValori.Name = "pnlValori"
        pnlValori.Size = New Size(641, 518)
        pnlValori.TabIndex = 0
        ' 
        ' grid
        ' 
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 37)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.ScrollByColumn = True
        grid.ShowTotalsRow = True
        grid.Size = New Size(641, 481)
        grid.TabIndex = 1
        grid.TotalsRowHeight = 30
        ' 
        ' pnlFilter
        ' 
        pnlFilter.Controls.Add(cboClsf)
        pnlFilter.Controls.Add(lblClsf)
        pnlFilter.Dock = DockStyle.Top
        pnlFilter.Location = New Point(0, 0)
        pnlFilter.Name = "pnlFilter"
        pnlFilter.Padding = New Padding(6, 4, 6, 4)
        pnlFilter.Size = New Size(641, 37)
        pnlFilter.TabIndex = 0
        ' 
        ' cboClsf
        ' 
        cboClsf.Dock = DockStyle.Left
        cboClsf.DropDownStyle = ComboBoxStyle.DropDownList
        cboClsf.FlatStyle = FlatStyle.Flat
        cboClsf.Location = New Point(112, 4)
        cboClsf.Name = "cboClsf"
        cboClsf.Size = New Size(280, 33)
        cboClsf.TabIndex = 1
        ' 
        ' lblClsf
        ' 
        lblClsf.AutoSize = True
        lblClsf.Dock = DockStyle.Left
        lblClsf.Location = New Point(6, 4)
        lblClsf.Name = "lblClsf"
        lblClsf.Padding = New Padding(0, 5, 8, 0)
        lblClsf.Size = New Size(106, 30)
        lblClsf.TabIndex = 0
        lblClsf.Text = "Clasificație:"
        lblClsf.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlPreview
        ' 
        pnlPreview.Controls.Add(lblPreviewGol)
        pnlPreview.Dock = DockStyle.Fill
        pnlPreview.Location = New Point(0, 0)
        pnlPreview.Name = "pnlPreview"
        pnlPreview.Size = New Size(641, 518)
        pnlPreview.TabIndex = 1
        pnlPreview.Visible = False
        ' 
        ' lblPreviewGol
        ' 
        lblPreviewGol.Dock = DockStyle.Fill
        lblPreviewGol.Font = New Font("Segoe UI", 10F)
        lblPreviewGol.Location = New Point(0, 0)
        lblPreviewGol.Name = "lblPreviewGol"
        lblPreviewGol.Size = New Size(641, 518)
        lblPreviewGol.TabIndex = 0
        lblPreviewGol.Text = "Selectați o revizie din arbore."
        lblPreviewGol.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' pnlPdf
        ' 
        pnlPdf.Controls.Add(pnlAdobe)
        pnlPdf.Dock = DockStyle.Fill
        pnlPdf.Location = New Point(0, 0)
        pnlPdf.Name = "pnlPdf"
        pnlPdf.Size = New Size(641, 518)
        pnlPdf.TabIndex = 3
        pnlPdf.Visible = False
        ' 
        ' pnlAdobe
        ' 
        pnlAdobe.Controls.Add(cboAdobeInst)
        pnlAdobe.Controls.Add(lblAdobeInst)
        pnlAdobe.Controls.Add(cboAdobeMod)
        pnlAdobe.Controls.Add(lblAdobeMod)
        pnlAdobe.Controls.Add(cboAdobeMotor)
        pnlAdobe.Controls.Add(lblAdobeMotor)
        pnlAdobe.Dock = DockStyle.Top
        pnlAdobe.Location = New Point(0, 0)
        pnlAdobe.Name = "pnlAdobe"
        pnlAdobe.Padding = New Padding(6, 4, 6, 4)
        pnlAdobe.Size = New Size(641, 32)
        pnlAdobe.TabIndex = 0
        ' 
        ' cboAdobeInst
        ' 
        cboAdobeInst.Dock = DockStyle.Left
        cboAdobeInst.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeInst.FlatStyle = FlatStyle.Flat
        cboAdobeInst.Location = New Point(912, 4)
        cboAdobeInst.Name = "cboAdobeInst"
        cboAdobeInst.Size = New Size(120, 33)
        cboAdobeInst.TabIndex = 3
        ' 
        ' lblAdobeInst
        ' 
        lblAdobeInst.AutoSize = True
        lblAdobeInst.Dock = DockStyle.Left
        lblAdobeInst.Location = New Point(705, 4)
        lblAdobeInst.Name = "lblAdobeInst"
        lblAdobeInst.Padding = New Padding(16, 5, 8, 0)
        lblAdobeInst.Size = New Size(207, 30)
        lblAdobeInst.TabIndex = 2
        lblAdobeInst.Text = "Instanță nouă Adobe:"
        lblAdobeInst.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboAdobeMod
        ' 
        cboAdobeMod.Dock = DockStyle.Left
        cboAdobeMod.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMod.FlatStyle = FlatStyle.Flat
        cboAdobeMod.Location = New Point(565, 4)
        cboAdobeMod.Name = "cboAdobeMod"
        cboAdobeMod.Size = New Size(140, 33)
        cboAdobeMod.TabIndex = 1
        ' 
        ' lblAdobeMod
        ' 
        lblAdobeMod.AutoSize = True
        lblAdobeMod.Dock = DockStyle.Left
        lblAdobeMod.Location = New Point(351, 4)
        lblAdobeMod.Name = "lblAdobeMod"
        lblAdobeMod.Padding = New Padding(0, 5, 8, 0)
        lblAdobeMod.Size = New Size(214, 30)
        lblAdobeMod.TabIndex = 0
        lblAdobeMod.Text = "Mod vizualizator Adobe:"
        lblAdobeMod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboAdobeMotor
        ' 
        cboAdobeMotor.Dock = DockStyle.Left
        cboAdobeMotor.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMotor.FlatStyle = FlatStyle.Flat
        cboAdobeMotor.Location = New Point(191, 4)
        cboAdobeMotor.Name = "cboAdobeMotor"
        cboAdobeMotor.Size = New Size(160, 33)
        cboAdobeMotor.TabIndex = 5
        ' 
        ' lblAdobeMotor
        ' 
        lblAdobeMotor.AutoSize = True
        lblAdobeMotor.Dock = DockStyle.Left
        lblAdobeMotor.Location = New Point(6, 4)
        lblAdobeMotor.Name = "lblAdobeMotor"
        lblAdobeMotor.Padding = New Padding(0, 5, 8, 0)
        lblAdobeMotor.Size = New Size(185, 30)
        lblAdobeMotor.TabIndex = 4
        lblAdobeMotor.Text = "Motor previzualizare:"
        lblAdobeMotor.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlFisiere
        ' 
        pnlFisiere.Controls.Add(lblFisiereGol)
        pnlFisiere.Dock = DockStyle.Fill
        pnlFisiere.Location = New Point(0, 0)
        pnlFisiere.Name = "pnlFisiere"
        pnlFisiere.Size = New Size(641, 518)
        pnlFisiere.TabIndex = 2
        pnlFisiere.Visible = False
        ' 
        ' lblFisiereGol
        ' 
        lblFisiereGol.Dock = DockStyle.Fill
        lblFisiereGol.Font = New Font("Segoe UI", 10F)
        lblFisiereGol.Location = New Point(0, 0)
        lblFisiereGol.Name = "lblFisiereGol"
        lblFisiereGol.Size = New Size(641, 518)
        lblFisiereGol.TabIndex = 0
        lblFisiereGol.Text = "Selectați un angajament din arbore."
        lblFisiereGol.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' navSub
        ' 
        navSub.Dock = DockStyle.Top
        KBotNavItem1.Image = Nothing
        KBotNavItem1.Key = "valori"
        KBotNavItem1.Text = "Valori"
        KBotNavItem2.Image = Nothing
        KBotNavItem2.Key = "previzualizare"
        KBotNavItem2.Text = "Vizualizare"
        KBotNavItem3.Image = Nothing
        KBotNavItem3.Key = "document"
        KBotNavItem3.Text = "Document"
        KBotNavItem4.Image = Nothing
        KBotNavItem4.Key = "fisiere"
        KBotNavItem4.Text = "Fișiere"
        navSub.Items.Add(KBotNavItem1)
        navSub.Items.Add(KBotNavItem2)
        navSub.Items.Add(KBotNavItem3)
        navSub.Items.Add(KBotNavItem4)
        navSub.Location = New Point(0, 0)
        navSub.Name = "navSub"
        navSub.Orientation = Theming.KBotNavOrientation.Horizontal
        navSub.SelectedKey = Nothing
        navSub.Size = New Size(641, 49)
        navSub.TabIndex = 0
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 567)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' DdfView
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfView"
        Size = New Size(986, 567)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        pnlTreeHead.ResumeLayout(False)
        pnlPages.ResumeLayout(False)
        pnlValori.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlFilter.ResumeLayout(False)
        pnlFilter.PerformLayout()
        pnlPreview.ResumeLayout(False)
        pnlPdf.ResumeLayout(False)
        pnlAdobe.ResumeLayout(False)
        pnlAdobe.PerformLayout()
        pnlFisiere.ResumeLayout(False)
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents pnlTreeHead As Panel
    Friend WithEvents lblTreeTitle As Label
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents navSub As KBot.Theming.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents pnlValori As Panel
    Friend WithEvents pnlFilter As Panel
    Friend WithEvents lblClsf As Label
    Friend WithEvents cboClsf As ComboBox
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlPreview As Panel
    Friend WithEvents lblPreviewGol As Label
    Friend WithEvents pnlPdf As Panel
    Friend WithEvents pnlAdobe As Panel
    Friend WithEvents lblAdobeMod As Label
    Friend WithEvents cboAdobeMod As ComboBox
    Friend WithEvents lblAdobeInst As Label
    Friend WithEvents cboAdobeInst As ComboBox
    Friend WithEvents lblAdobeMotor As Label
    Friend WithEvents cboAdobeMotor As ComboBox
    Friend WithEvents pnlFisiere As Panel
    Friend WithEvents lblFisiereGol As Label
    Friend WithEvents lblEmpty As Label
End Class
