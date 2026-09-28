Imports KBot.Controls

' «Note corecție» (slice 0088): the CAB correction notes of the selected angajament -- a tree of
' month -> note on the left, and on the right the same horizontal sub-navigation as the ORD view:
' «Vizualizare» (the note's rows), «Document» (its PDF) and «Recipisă» (FOREXE's receipt, slice
' 0088-04). All controls are declared HERE.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class NoteCabView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
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
        split.Size = New Size(983, 528)
        split.SplitterDistance = 318
        split.SplitterWidth = 9
        split.TabIndex = 0
        '
        ' tree
        '
        tree.BorderColor = SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.ExpanderSize = 10
        tree.Font = New Font("Calibri", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Note"
        tree.FooterCaptionFont = New Font("Calibri", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.FooterCollapseButton = True
        tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 30
        tree.FooterSeparatorColor = Color.Gainsboro
        tree.FooterSeparatorWidth = 2
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " Note de corecție CAB"
        tree.HeaderFont = New Font("Calibri", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Color.Black
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderSearchIconTooltip = "Caută în arbore." & vbLf & "ESC golește căutarea și închide banda."
        tree.HeaderSeparatorColor = Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.LeftIconSize = New Size(14, 14)
        tree.LeftTextWidth = 100
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.NodeImages = image_list
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeStart = 8
        tree.RightIconSize = New Size(14, 14)
        tree.SearchIn = AdvancedTreeControl.En_Tree_SearchIn.SearchIn_Both
        tree.Size = New Size(318, 528)
        tree.TabIndex = 0
        '
        ' image_list -- month, then a note by its state: unsigned / signed / sent into CAB.
        '
        image_list.ColorDepth = ColorDepth.Depth32Bit
        image_list.ImageSize = New Size(16, 16)
        image_list.TransparentColor = Color.Transparent
        image_list.Images.Add("month", My.Resources.Resources.calendar)
        image_list.Images.Add("unsigned", My.Resources.Resources.FX_ORANGE_16)
        image_list.Images.Add("signed", My.Resources.Resources.FX_GREEN_16)
        image_list.Images.Add("sent", My.Resources.Resources.FX_BLUE_16)
        '
        ' pnlPages
        '
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 40)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New Size(656, 488)
        pnlPages.TabIndex = 1
        '
        ' navSub
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
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Check_boxes_32
        KBotNavItem3.Key = "recipisa"
        KBotNavItem3.Text = "Recipisă"
        navSub.Items.Add(KBotNavItem1)
        navSub.Items.Add(KBotNavItem2)
        navSub.Items.Add(KBotNavItem3)
        navSub.Location = New Point(0, 0)
        navSub.Name = "navSub"
        navSub.Orientation = KBotNavOrientation.Horizontal
        navSub.SelectedKey = Nothing
        navSub.Size = New Size(656, 40)
        navSub.TabIndex = 0
        '
        ' lblEmpty
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10.0F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(983, 528)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' NoteCabView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "NoteCabView"
        Size = New Size(983, 528)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        CType(navSub, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As Global.KBot.Controls.AdvancedTreeControl
    Friend WithEvents navSub As Global.KBot.Controls.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents lblEmpty As Label
    Friend WithEvents image_list As ImageList
End Class
