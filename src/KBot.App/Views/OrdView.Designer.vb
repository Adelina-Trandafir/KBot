Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdView
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(OrdView))
        Dim TreeNodeDefinition1 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim TreeNodeDefinition2 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim TreeNodeDefinition3 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        split = New SplitContainer()
        tree = New AdvancedTreeControl()
        image_list = New ImageList(components)
        pnlPages = New Panel()
        navSub = New KBotNavList()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
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
        ' tree — arborele ordonanțărilor (lună -> ordonanțare). Toate proprietățile de aspect
        ' sunt AUTORITE AICI (felia 0033, decizia 6), copiate după «tree»-ul lui RezervariView:
        ' fontul, ItemHeight, dimensiunile iconițelor, benzile de antet/subsol. Din cod se
        ' populează DOAR nodurile.
        '
        tree.Dock = DockStyle.Fill
        tree.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaptionFont = New Font("Consolas", 8F, FontStyle.Bold)
        tree.FooterCollapseButton = True
        tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 40
        tree.FooterIconSize = New Size(24, 24)
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " ORDONANȚĂRI"
        tree.HeaderFont = New Font("Tahoma", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Color.Black
        tree.HeaderGradientEndColor = Color.CornflowerBlue
        tree.HeaderHeight = 40
        tree.HeaderIconSize = New Size(24, 24)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 30
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.NodeImages = image_list
        TreeNodeDefinition1.Caption = "Aprilie~~~11.234.567,78"
        TreeNodeDefinition1.Expanded = True
        TreeNodeDefinition1.ImageKey = "month"
        TreeNodeDefinition1.Key = "apr"
        TreeNodeDefinition1.OpenImageKey = Nothing
        TreeNodeDefinition1.ParentKey = Nothing
        TreeNodeDefinition1.RightImageKey = Nothing
        TreeNodeDefinition1.Tag = Nothing
        TreeNodeDefinition1.Tooltip = Nothing
        TreeNodeDefinition2.Caption = "14 - 07.04.2026~~~11.234.567,89"
        TreeNodeDefinition2.ImageKey = "up"
        TreeNodeDefinition2.Key = "1"
        TreeNodeDefinition2.OpenImageKey = Nothing
        TreeNodeDefinition2.ParentKey = "apr"
        TreeNodeDefinition2.RightImageKey = Nothing
        TreeNodeDefinition2.Tag = Nothing
        TreeNodeDefinition2.Tooltip = Nothing
        TreeNodeDefinition3.Caption = "15 - 07.04.2026~~~123.456,78"
        TreeNodeDefinition3.ImageKey = "down"
        TreeNodeDefinition3.Key = "2"
        TreeNodeDefinition3.OpenImageKey = Nothing
        TreeNodeDefinition3.ParentKey = "apr"
        TreeNodeDefinition3.RightImageKey = Nothing
        TreeNodeDefinition3.Tag = Nothing
        TreeNodeDefinition3.Tooltip = Nothing
        tree.Nodes.Add(TreeNodeDefinition1)
        tree.Nodes.Add(TreeNodeDefinition2)
        tree.Nodes.Add(TreeNodeDefinition3)
        tree.PaddingExpanderGap = 10
        tree.PaddingIconGap = 10
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = True
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 110
        tree.Size = New Size(305, 528)
        tree.TabIndex = 0
        '
        ' image_list — iconițele nodurilor, legate de arbore prin tree.NodeImages. Cheile sunt
        ' cele citite de OrdView (month / up / down / neutral). Pozele pornesc de la setul lui
        ' RezervariView; operatorul le poate schimba din designer, cheile rămân.
        '
        image_list.ColorDepth = ColorDepth.Depth32Bit
        image_list.ImageStream = CType(resources.GetObject("image_list.ImageStream"), ImageListStreamer)
        image_list.TransparentColor = Color.Transparent
        image_list.Images.SetKeyName(0, "up")
        image_list.Images.SetKeyName(1, "down")
        image_list.Images.SetKeyName(2, "month")
        image_list.Images.SetKeyName(3, "neutral")
        image_list.Images.SetKeyName(4, "plus")
        '
        ' pnlPages — GAZDA sub-paginilor, echivalentul lui `pnlPages` din DdfView. Rămâne GOL
        ' în designer: paginile (OrdVizualizarePage / OrdDocumentPage) sunt UserControl-uri
        ' separate, create LENEȘ la prima activare din navSub și adăugate aici de
        ' OrdView.ActivatePage.
        '
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 40)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(849, 488)
        pnlPages.TabIndex = 1
        '
        ' navSub — sub-navigarea orizontală. Cheile sunt LITERALE aici și trebuie să rămână în
        ' acord cu constantele PAGE_* din OrdView (o desincronizare aruncă zgomotos la
        ' selecția inițială, nu tăcut).
        '
        navSub.Dock = DockStyle.Top
        navSub.IconSize = 16
        navSub.ItemCornerRadius = 2
        navSub.ItemPadding = New Padding(3)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.vertical
        KBotNavItem1.Key = "vizualizare"
        KBotNavItem1.Text = "Vizualizare"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Pdf_exports_24
        KBotNavItem2.Key = "document"
        KBotNavItem2.Text = "Document"
        navSub.Items.Add(KBotNavItem1)
        navSub.Items.Add(KBotNavItem2)
        navSub.Location = New Point(0, 0)
        navSub.Name = "navSub"
        navSub.Orientation = KBotNavOrientation.Horizontal
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
        ' OrdView
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "OrdView"
        Size = New Size(1163, 528)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents navSub As KBot.Controls.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents lblEmpty As Label
    Friend WithEvents image_list As ImageList
End Class
