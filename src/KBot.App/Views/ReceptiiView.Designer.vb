Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ReceptiiView
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
        Dim TreeNodeDefinition1 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim TreeNodeDefinition2 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim TreeNodeDefinition3 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ReceptiiView))
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        split = New SplitContainer()
        tree = New AdvancedTreeControl()
        image_list = New ImageList(components)
        grid = New KBotDataView()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(3, 4, 3, 4)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tree)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(grid)
        split.Size = New Size(789, 454)
        split.SplitterDistance = 255
        split.SplitterWidth = 7
        split.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BorderColor = SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Actualizează"
        tree.FooterCaptionFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 30
        tree.FooterIconSize = New Size(18, 18)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterRightIconTooltip = "Reîncarcă recepțiile de la server."
        tree.FooterSeparatorColor = Color.Gainsboro
        tree.FooterSeparatorWidth = 2
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " RECEPȚII"
        tree.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Color.Black
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderSearchIconTooltip = "Caută în arbore." & vbLf & "ESC golește căutarea și închide banda."
        tree.HeaderSeparatorColor = Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderTextAlign = ContentAlignment.MiddleCenter
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 20
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(3, 4, 3, 4)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.NodeImages = image_list
        TreeNodeDefinition1.Caption = "Ianuarie~~~12.345.678,99"
        TreeNodeDefinition1.Expanded = True
        TreeNodeDefinition1.ImageKey = "month"
        TreeNodeDefinition1.Key = "1"
        TreeNodeDefinition1.OpenImageKey = Nothing
        TreeNodeDefinition1.ParentKey = Nothing
        TreeNodeDefinition1.RightImageKey = Nothing
        TreeNodeDefinition1.Tag = Nothing
        TreeNodeDefinition1.Tooltip = Nothing
        TreeNodeDefinition2.Caption = "21.01.2026~~~12.345.789.69"
        TreeNodeDefinition2.ImageKey = "up"
        TreeNodeDefinition2.Key = "2"
        TreeNodeDefinition2.OpenImageKey = Nothing
        TreeNodeDefinition2.ParentKey = "1"
        TreeNodeDefinition2.RightImageKey = Nothing
        TreeNodeDefinition2.Tag = Nothing
        TreeNodeDefinition2.Tooltip = Nothing
        TreeNodeDefinition3.Caption = "22.01.2026~~~-123.33"
        TreeNodeDefinition3.ImageKey = "down"
        TreeNodeDefinition3.Key = Nothing
        TreeNodeDefinition3.OpenImageKey = Nothing
        TreeNodeDefinition3.ParentKey = "1"
        TreeNodeDefinition3.RightImageKey = Nothing
        TreeNodeDefinition3.Tag = Nothing
        TreeNodeDefinition3.Tooltip = Nothing
        tree.Nodes.Add(TreeNodeDefinition1)
        tree.Nodes.Add(TreeNodeDefinition2)
        tree.Nodes.Add(TreeNodeDefinition3)
        tree.PaddingExpanderGap = 10
        tree.PaddingIconGap = 10
        tree.PaddingTreeStart = 8
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 90
        tree.SearchIn = AdvancedTreeControl.En_Tree_SearchIn.SearchIn_Both
        tree.Size = New Size(255, 454)
        tree.TabIndex = 0
        ' 
        ' image_list
        ' 
        image_list.ColorDepth = ColorDepth.Depth32Bit
        image_list.ImageStream = CType(resources.GetObject("image_list.ImageStream"), ImageListStreamer)
        image_list.TransparentColor = Color.Transparent
        image_list.Images.SetKeyName(0, "month")
        image_list.Images.SetKeyName(1, "up")
        image_list.Images.SetKeyName(2, "down")
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.AutoSizeHeaderHeight = False
        grid.BackColor = SystemColors.Window
        grid.BorderColor = SystemColors.ActiveBorder
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 110
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.HeaderText = "Descriere"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "descriere"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 200
        KBotDataColumn3.Aggregate = KBotAggregate.Sum
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Valoare"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "valoare"
        KBotDataColumn3.MultiLine = True
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 80
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Dock = DockStyle.Fill
        grid.EnableGrouping = True
        grid.FillColumnKey = "descriere"
        grid.FooterBackColor = SystemColors.Control
        grid.FooterCaption = "TOTALURI"
        grid.FooterColumnSeparatorColor = Color.Gainsboro
        grid.FooterHeight = 30
        grid.FooterSeparatorColor = Color.Gainsboro
        grid.FooterVisible = True
        grid.HeaderBackColor = SystemColors.Control
        grid.HeaderColumnSeparatorColor = Color.Gainsboro
        grid.HeaderSeparatorColor = Color.Gainsboro
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(3, 4, 3, 4)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 20
        grid.ShrinkColumnsToFit = False
        grid.Size = New Size(527, 454)
        grid.TabIndex = 0
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(789, 454)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' ReceptiiView
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(3, 4, 3, 4)
        Name = "ReceptiiView"
        Size = New Size(789, 454)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents lblEmpty As Label
    Friend WithEvents image_list As ImageList
End Class
