Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RezervariView
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RezervariView))
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
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
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tree)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(grid)
        split.Size = New Size(986, 567)
        split.SplitterDistance = 275
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.Dock = DockStyle.Fill
        tree.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Actualizează Rezervări"
        tree.FooterCaptionFont = New Font("Consolas", 8F, FontStyle.Bold)
        tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterHeight = 40
        tree.FooterIconSize = New Size(24, 24)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " REZERVĂRI"
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
        TreeNodeDefinition1.Caption = "Ianuarie~~~11.234.567,78"
        TreeNodeDefinition1.Expanded = True
        TreeNodeDefinition1.ImageKey = "month"
        TreeNodeDefinition1.Key = "ian"
        TreeNodeDefinition1.OpenImageKey = Nothing
        TreeNodeDefinition1.ParentKey = Nothing
        TreeNodeDefinition1.RightImageKey = Nothing
        TreeNodeDefinition1.Tag = Nothing
        TreeNodeDefinition1.Tooltip = Nothing
        TreeNodeDefinition2.Caption = "20.01.2026~~~11.234.567,89"
        TreeNodeDefinition2.ImageKey = "up"
        TreeNodeDefinition2.Key = "1"
        TreeNodeDefinition2.OpenImageKey = Nothing
        TreeNodeDefinition2.ParentKey = "ian"
        TreeNodeDefinition2.RightImageKey = Nothing
        TreeNodeDefinition2.Tag = Nothing
        TreeNodeDefinition2.Tooltip = Nothing
        TreeNodeDefinition3.Caption = "25.01.2026~~~123.456,78"
        TreeNodeDefinition3.ImageKey = "down"
        TreeNodeDefinition3.Key = "2"
        TreeNodeDefinition3.OpenImageKey = Nothing
        TreeNodeDefinition3.ParentKey = "ian"
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
        tree.Size = New Size(275, 567)
        tree.TabIndex = 0
        ' 
        ' image_list
        ' 
        image_list.ColorDepth = ColorDepth.Depth32Bit
        image_list.ImageStream = CType(resources.GetObject("image_list.ImageStream"), ImageListStreamer)
        image_list.TransparentColor = Color.Transparent
        image_list.Images.SetKeyName(0, "up")
        image_list.Images.SetKeyName(1, "down")
        image_list.Images.SetKeyName(2, "month")
        image_list.Images.SetKeyName(3, "equal")
        image_list.Images.SetKeyName(4, "plus")
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.AutoSizeHeaderHeight = False
        grid.BackColor = SystemColors.Window
        grid.CellTooltip.Enabled = False
        grid.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 170
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn2.DecimalPlaces = 2
        KBotDataColumn2.Format = KBotFormat.Standard
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Credit" & vbCrLf & "Bugetar"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "credit_bug"
        KBotDataColumn2.MultiLine = True
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.ValueType = KBotValueType.Number
        KBotDataColumn2.Width = 130
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Rezervare" & vbCrLf & "Inițială"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "r_initiala"
        KBotDataColumn3.MultiLine = True
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 130
        KBotDataColumn4.Aggregate = KBotAggregate.Sum
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Rezervare" & vbCrLf & "Curentă"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "r_valoare"
        KBotDataColumn4.MultiLine = True
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 140
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Rezervare" & vbCrLf & "Definitivă"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "r_definitiva"
        KBotDataColumn5.MultiLine = True
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 130
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Dock = DockStyle.Fill
        grid.EnableGrouping = True
        grid.FooterBackColor = SystemColors.Control
        grid.FooterCaption = "TOTALURI"
        grid.FooterHeight = 40
        grid.FooterVisible = True
        grid.FrozenColumnCount = 1
        grid.HeaderHeight = 60
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.ScrollByColumn = True
        grid.ShrinkColumnsToFit = False
        grid.Size = New Size(702, 567)
        grid.TabIndex = 0
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
        ' RezervariView
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "RezervariView"
        Size = New Size(986, 567)
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
