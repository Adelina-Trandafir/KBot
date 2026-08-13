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
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(DdfView))
        Dim KBotNavItem1 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem2 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        Dim KBotNavItem3 As KBot.Controls.KBotNavItem = New Controls.KBotNavItem()
        split = New SplitContainer()
        tree = New Controls.AdvancedTreeControl()
        tree_image_list = New ImageList(components)
        pnlPages = New Panel()
        pnlPdf = New Panel()
        tlyPDF = New TableLayoutPanel()
        previewPdf = New ReaderHostPreview()
        pnlBottomButtons = New Panel()
        tlyBottomButtons = New TableLayoutPanel()
        btnOpenInAdobe = New Button()
        btnSaveLocalCopy = New Button()
        pnlAdobe = New Panel()
        cboAdobeInst = New ComboBox()
        lblAdobeInst = New Label()
        cboAdobeMod = New ComboBox()
        lblAdobeMod = New Label()
        cboAdobeMotor = New ComboBox()
        lblAdobeMotor = New Label()
        pnlValori = New Panel()
        grid = New Controls.KBotDataView()
        pnlFilter = New Panel()
        cboClsf = New ComboBox()
        lblClsf = New Label()
        pnlPreview = New Panel()
        previewXfa = New XfaXmlPreview()
        lblPreviewGol = New Label()
        pnlFisiere = New Panel()
        browser = New DdfFileBrowser()
        lblFisiereGol = New Label()
        navSub = New Controls.KBotNavList()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlPages.SuspendLayout()
        pnlPdf.SuspendLayout()
        tlyPDF.SuspendLayout()
        pnlBottomButtons.SuspendLayout()
        tlyBottomButtons.SuspendLayout()
        pnlAdobe.SuspendLayout()
        pnlValori.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlFilter.SuspendLayout()
        pnlPreview.SuspendLayout()
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
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(pnlPages)
        split.Panel2.Controls.Add(navSub)
        split.Size = New Size(1163, 528)
        split.SplitterDistance = 305
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.Dock = DockStyle.Fill
        tree.DynamicColumns = False
        tree.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Actualizează DDF"
        tree.FooterCaptionFont = New Font("Consolas", 8F, FontStyle.Bold)
        tree.FooterCollapseButton = True
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
        tree.HeaderCaption = " REVIZII DDF"
        tree.HeaderFont = New Font("Tahoma", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Color.Black
        tree.HeaderGradientEndColor = Color.CornflowerBlue
        tree.HeaderHeight = 40
        tree.HeaderIconSize = New Size(24, 24)
        tree.HeaderLeftIcon = My.Resources.Resources.Umut_Pulat_Tulliana_2_File_temporary_32
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 24
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.NodeImages = tree_image_list
        tree.PaddingExpanderGap = 10
        tree.PaddingIconGap = 10
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = True
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 110
        tree.ScrollBarTheme = KBot.Controls.AdvancedTreeControl.En_ScrollBarTheme.Default
        tree.Size = New Size(305, 528)
        tree.TabIndex = 1
        ' 
        ' tree_image_list
        ' 
        tree_image_list.ColorDepth = ColorDepth.Depth32Bit
        tree_image_list.ImageStream = CType(resources.GetObject("tree_image_list.ImageStream"), ImageListStreamer)
        tree_image_list.TransparentColor = Color.Transparent
        tree_image_list.Images.SetKeyName(0, "Up")
        tree_image_list.Images.SetKeyName(1, "down")
        tree_image_list.Images.SetKeyName(2, "folder_open")
        tree_image_list.Images.SetKeyName(3, "folder_closed")
        ' 
        ' pnlPages
        ' 
        pnlPages.Controls.Add(pnlPdf)
        pnlPages.Controls.Add(pnlValori)
        pnlPages.Controls.Add(pnlPreview)
        pnlPages.Controls.Add(pnlFisiere)
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 40)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(849, 488)
        pnlPages.TabIndex = 1
        ' 
        ' pnlPdf
        ' 
        pnlPdf.Controls.Add(tlyPDF)
        pnlPdf.Controls.Add(pnlAdobe)
        pnlPdf.Dock = DockStyle.Fill
        pnlPdf.Location = New Point(0, 0)
        pnlPdf.Name = "pnlPdf"
        pnlPdf.Size = New Size(849, 488)
        pnlPdf.TabIndex = 3
        pnlPdf.Visible = False
        ' 
        ' tlyPDF
        ' 
        tlyPDF.ColumnCount = 1
        tlyPDF.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPDF.Controls.Add(previewPdf, 0, 0)
        tlyPDF.Controls.Add(pnlBottomButtons, 0, 1)
        tlyPDF.Dock = DockStyle.Fill
        tlyPDF.Location = New Point(0, 0)
        tlyPDF.Margin = New Padding(0)
        tlyPDF.Name = "tlyPDF"
        tlyPDF.RowCount = 2
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPDF.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyPDF.Size = New Size(849, 488)
        tlyPDF.TabIndex = 3
        ' 
        ' previewPdf
        ' 
        previewPdf.BackColor = SystemColors.Window
        previewPdf.BorderStyle = BorderStyle.FixedSingle
        previewPdf.Dock = DockStyle.Fill
        previewPdf.Location = New Point(3, 3)
        previewPdf.Margin = New Padding(3, 3, 3, 0)
        previewPdf.Name = "previewPdf"
        previewPdf.Size = New Size(843, 445)
        previewPdf.TabIndex = 1
        ' 
        ' pnlBottomButtons
        ' 
        pnlBottomButtons.Controls.Add(tlyBottomButtons)
        pnlBottomButtons.Dock = DockStyle.Fill
        pnlBottomButtons.Location = New Point(0, 448)
        pnlBottomButtons.Margin = New Padding(0)
        pnlBottomButtons.Name = "pnlBottomButtons"
        pnlBottomButtons.Size = New Size(849, 40)
        pnlBottomButtons.TabIndex = 2
        ' 
        ' tlyBottomButtons
        ' 
        tlyBottomButtons.BackColor = Color.Transparent
        tlyBottomButtons.ColumnCount = 3
        tlyBottomButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBottomButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyBottomButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyBottomButtons.Controls.Add(btnOpenInAdobe, 1, 0)
        tlyBottomButtons.Controls.Add(btnSaveLocalCopy, 2, 0)
        tlyBottomButtons.Dock = DockStyle.Fill
        tlyBottomButtons.Location = New Point(0, 0)
        tlyBottomButtons.Margin = New Padding(0)
        tlyBottomButtons.Name = "tlyBottomButtons"
        tlyBottomButtons.RowCount = 1
        tlyBottomButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBottomButtons.Size = New Size(849, 40)
        tlyBottomButtons.TabIndex = 0
        ' 
        ' btnOpenInAdobe
        ' 
        btnOpenInAdobe.Dock = DockStyle.Fill
        btnOpenInAdobe.FlatAppearance.BorderSize = 0
        btnOpenInAdobe.FlatStyle = FlatStyle.Flat
        btnOpenInAdobe.Image = CType(resources.GetObject("btnOpenInAdobe.Image"), Image)
        btnOpenInAdobe.Location = New Point(689, 0)
        btnOpenInAdobe.Margin = New Padding(0)
        btnOpenInAdobe.Name = "btnOpenInAdobe"
        btnOpenInAdobe.Size = New Size(80, 40)
        btnOpenInAdobe.TabIndex = 0
        btnOpenInAdobe.UseVisualStyleBackColor = True
        ' 
        ' btnSaveLocalCopy
        ' 
        btnSaveLocalCopy.Dock = DockStyle.Fill
        btnSaveLocalCopy.FlatAppearance.BorderSize = 0
        btnSaveLocalCopy.FlatStyle = FlatStyle.Flat
        btnSaveLocalCopy.Image = CType(resources.GetObject("btnSaveLocalCopy.Image"), Image)
        btnSaveLocalCopy.Location = New Point(769, 0)
        btnSaveLocalCopy.Margin = New Padding(0)
        btnSaveLocalCopy.Name = "btnSaveLocalCopy"
        btnSaveLocalCopy.Size = New Size(80, 40)
        btnSaveLocalCopy.TabIndex = 1
        btnSaveLocalCopy.UseVisualStyleBackColor = True
        ' 
        ' pnlAdobe
        ' 
        pnlAdobe.Controls.Add(cboAdobeInst)
        pnlAdobe.Controls.Add(lblAdobeInst)
        pnlAdobe.Controls.Add(cboAdobeMod)
        pnlAdobe.Controls.Add(lblAdobeMod)
        pnlAdobe.Controls.Add(cboAdobeMotor)
        pnlAdobe.Controls.Add(lblAdobeMotor)
        pnlAdobe.Location = New Point(0, 0)
        pnlAdobe.Name = "pnlAdobe"
        pnlAdobe.Padding = New Padding(6, 4, 6, 4)
        pnlAdobe.Size = New Size(849, 37)
        pnlAdobe.TabIndex = 0
        pnlAdobe.Visible = False
        ' 
        ' cboAdobeInst
        ' 
        cboAdobeInst.Dock = DockStyle.Left
        cboAdobeInst.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeInst.FlatStyle = FlatStyle.Flat
        cboAdobeInst.Location = New Point(691, 4)
        cboAdobeInst.Name = "cboAdobeInst"
        cboAdobeInst.Size = New Size(158, 33)
        cboAdobeInst.TabIndex = 3
        ' 
        ' lblAdobeInst
        ' 
        lblAdobeInst.AutoSize = True
        lblAdobeInst.Dock = DockStyle.Left
        lblAdobeInst.Location = New Point(543, 4)
        lblAdobeInst.Name = "lblAdobeInst"
        lblAdobeInst.Padding = New Padding(16, 5, 8, 0)
        lblAdobeInst.Size = New Size(148, 30)
        lblAdobeInst.TabIndex = 2
        lblAdobeInst.Text = "Instanță nouă:"
        lblAdobeInst.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboAdobeMod
        ' 
        cboAdobeMod.Dock = DockStyle.Left
        cboAdobeMod.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMod.FlatStyle = FlatStyle.Flat
        cboAdobeMod.Location = New Point(385, 4)
        cboAdobeMod.Name = "cboAdobeMod"
        cboAdobeMod.Size = New Size(158, 33)
        cboAdobeMod.TabIndex = 1
        ' 
        ' lblAdobeMod
        ' 
        lblAdobeMod.AutoSize = True
        lblAdobeMod.Dock = DockStyle.Left
        lblAdobeMod.Location = New Point(238, 4)
        lblAdobeMod.Name = "lblAdobeMod"
        lblAdobeMod.Padding = New Padding(0, 5, 8, 0)
        lblAdobeMod.Size = New Size(147, 30)
        lblAdobeMod.TabIndex = 0
        lblAdobeMod.Text = "Mod vizualizare:"
        lblAdobeMod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboAdobeMotor
        ' 
        cboAdobeMotor.Dock = DockStyle.Left
        cboAdobeMotor.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMotor.FlatStyle = FlatStyle.Flat
        cboAdobeMotor.Location = New Point(80, 4)
        cboAdobeMotor.Name = "cboAdobeMotor"
        cboAdobeMotor.Size = New Size(158, 33)
        cboAdobeMotor.TabIndex = 5
        ' 
        ' lblAdobeMotor
        ' 
        lblAdobeMotor.AutoSize = True
        lblAdobeMotor.Dock = DockStyle.Left
        lblAdobeMotor.Location = New Point(6, 4)
        lblAdobeMotor.Name = "lblAdobeMotor"
        lblAdobeMotor.Padding = New Padding(0, 5, 8, 0)
        lblAdobeMotor.Size = New Size(74, 30)
        lblAdobeMotor.TabIndex = 4
        lblAdobeMotor.Text = "Motor:"
        lblAdobeMotor.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlValori
        ' 
        pnlValori.Controls.Add(grid)
        pnlValori.Controls.Add(pnlFilter)
        pnlValori.Dock = DockStyle.Fill
        pnlValori.Location = New Point(0, 0)
        pnlValori.Name = "pnlValori"
        pnlValori.Size = New Size(849, 488)
        pnlValori.TabIndex = 0
        pnlValori.Visible = False
        ' 
        ' grid
        ' 
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        grid.Dock = DockStyle.Fill
        grid.FrozenColumnCount = 1
        grid.Location = New Point(0, 37)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.ScrollByColumn = True
        grid.FooterVisible = True
        grid.Size = New Size(849, 451)
        grid.TabIndex = 1
        grid.FooterHeight = 40
        ' 
        ' pnlFilter
        ' 
        pnlFilter.Controls.Add(cboClsf)
        pnlFilter.Controls.Add(lblClsf)
        pnlFilter.Dock = DockStyle.Top
        pnlFilter.Location = New Point(0, 0)
        pnlFilter.Name = "pnlFilter"
        pnlFilter.Padding = New Padding(6, 4, 6, 4)
        pnlFilter.Size = New Size(849, 37)
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
        pnlPreview.Controls.Add(previewXfa)
        pnlPreview.Controls.Add(lblPreviewGol)
        pnlPreview.Dock = DockStyle.Fill
        pnlPreview.Location = New Point(0, 0)
        pnlPreview.Name = "pnlPreview"
        pnlPreview.Size = New Size(849, 488)
        pnlPreview.TabIndex = 1
        pnlPreview.Visible = False
        ' 
        ' previewXfa
        ' 
        previewXfa.Dock = DockStyle.Fill
        previewXfa.Location = New Point(0, 0)
        previewXfa.Name = "previewXfa"
        previewXfa.Size = New Size(849, 488)
        previewXfa.TabIndex = 0
        ' 
        ' lblPreviewGol
        ' 
        lblPreviewGol.Dock = DockStyle.Fill
        lblPreviewGol.Font = New Font("Segoe UI", 10F)
        lblPreviewGol.Location = New Point(0, 0)
        lblPreviewGol.Name = "lblPreviewGol"
        lblPreviewGol.Size = New Size(849, 488)
        lblPreviewGol.TabIndex = 0
        lblPreviewGol.Text = "Selectați o revizie din arbore."
        lblPreviewGol.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' pnlFisiere
        ' 
        pnlFisiere.Controls.Add(browser)
        pnlFisiere.Controls.Add(lblFisiereGol)
        pnlFisiere.Dock = DockStyle.Fill
        pnlFisiere.Location = New Point(0, 0)
        pnlFisiere.Name = "pnlFisiere"
        pnlFisiere.Size = New Size(849, 488)
        pnlFisiere.TabIndex = 2
        pnlFisiere.Visible = False
        ' 
        ' browser
        ' 
        browser.Dock = DockStyle.Fill
        browser.Location = New Point(0, 0)
        browser.Name = "browser"
        browser.Size = New Size(849, 488)
        browser.TabIndex = 0
        ' 
        ' lblFisiereGol
        ' 
        lblFisiereGol.Dock = DockStyle.Fill
        lblFisiereGol.Font = New Font("Segoe UI", 10F)
        lblFisiereGol.Location = New Point(0, 0)
        lblFisiereGol.Name = "lblFisiereGol"
        lblFisiereGol.Size = New Size(849, 488)
        lblFisiereGol.TabIndex = 0
        lblFisiereGol.Text = "Selectați un angajament din arbore."
        lblFisiereGol.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' navSub
        ' 
        navSub.Dock = DockStyle.Top
        navSub.IconSize = 16
        navSub.ItemCornerRadius = 2
        navSub.ItemPadding = New Padding(3)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.vertical
        KBotNavItem1.Key = "previzualizare"
        KBotNavItem1.Text = "Vizualizare"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Pdf_exports_24
        KBotNavItem2.Key = "document"
        KBotNavItem2.Text = "Document PDF"
        KBotNavItem3.Key = "fisiere"
        KBotNavItem3.Text = "Fișiere"
        KBotNavItem3.Visible = False
        navSub.Items.Add(KBotNavItem1)
        navSub.Items.Add(KBotNavItem2)
        navSub.Items.Add(KBotNavItem3)
        navSub.Location = New Point(0, 0)
        navSub.Name = "navSub"
        navSub.Orientation = KBot.Controls.KBotNavOrientation.Horizontal
        navSub.SelectedKey = Nothing
        navSub.Size = New Size(849, 40)
        navSub.TabIndex = 0
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(1163, 528)
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
        Size = New Size(1163, 528)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        pnlPages.ResumeLayout(False)
        pnlPdf.ResumeLayout(False)
        tlyPDF.ResumeLayout(False)
        pnlBottomButtons.ResumeLayout(False)
        tlyBottomButtons.ResumeLayout(False)
        pnlAdobe.ResumeLayout(False)
        pnlAdobe.PerformLayout()
        pnlValori.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlFilter.ResumeLayout(False)
        pnlFilter.PerformLayout()
        pnlPreview.ResumeLayout(False)
        pnlFisiere.ResumeLayout(False)
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents navSub As KBot.Controls.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents pnlValori As Panel
    Friend WithEvents pnlFilter As Panel
    Friend WithEvents lblClsf As Label
    Friend WithEvents cboClsf As ComboBox
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlPreview As Panel
    Friend WithEvents previewXfa As XfaXmlPreview
    Friend WithEvents lblPreviewGol As Label
    Friend WithEvents pnlPdf As Panel
    Friend WithEvents previewPdf As ReaderHostPreview
    Friend WithEvents pnlAdobe As Panel
    Friend WithEvents lblAdobeMod As Label
    Friend WithEvents cboAdobeMod As ComboBox
    Friend WithEvents lblAdobeInst As Label
    Friend WithEvents cboAdobeInst As ComboBox
    Friend WithEvents lblAdobeMotor As Label
    Friend WithEvents cboAdobeMotor As ComboBox
    Friend WithEvents pnlFisiere As Panel
    Friend WithEvents browser As DdfFileBrowser
    Friend WithEvents lblFisiereGol As Label
    Friend WithEvents lblEmpty As Label
    Friend WithEvents pnlBottomButtons As Panel
    Friend WithEvents tlyPDF As TableLayoutPanel
    Friend WithEvents tree_image_list As ImageList
    Friend WithEvents tlyBottomButtons As TableLayoutPanel
    Friend WithEvents btnOpenInAdobe As Button
    Friend WithEvents btnSaveLocalCopy As Button
End Class
