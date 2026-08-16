Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PlatiView
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PlatiView))
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        split = New SplitContainer()
        tree = New AdvancedTreeControl()
        image_list = New ImageList(components)
        innerSplit = New SplitContainer()
        grid = New KBotDataView()
        detailPane = New Panel()
        detailTable = New TableLayoutPanel()
        lblDetailMessage = New Label()
        capNrDoc = New Label()
        valNrDoc = New Label()
        capDataBanca = New Label()
        valDataBanca = New Label()
        capDataDoc = New Label()
        valDataDoc = New Label()
        capReferinta = New Label()
        valReferinta = New Label()
        capPlatitor = New Label()
        valPlatitor = New Label()
        capCui = New Label()
        valCui = New Label()
        capIban = New Label()
        valIban = New Label()
        capDebit = New Label()
        valDebit = New Label()
        capCredit = New Label()
        valCredit = New Label()
        capExplicatii = New Label()
        valExplicatii = New Label()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(innerSplit, ComponentModel.ISupportInitialize).BeginInit()
        innerSplit.Panel1.SuspendLayout()
        innerSplit.Panel2.SuspendLayout()
        innerSplit.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        detailPane.SuspendLayout()
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
        split.Panel2.Controls.Add(innerSplit)
        split.Size = New Size(986, 567)
        split.SplitterDistance = 275
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.Dock = DockStyle.Fill
        tree.Font = New Font("Calibri", 9.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Actualizează Plăți"
        tree.FooterCaptionFont = New Font("Consolas", 8.0F, FontStyle.Bold)
        tree.FooterCollapseButtonPosition = AdvancedTreeControl.En_FooterButtonPosition.Left
        tree.FooterCollapseCollapsedImage = My.Resources.Resources.expand_24
        tree.FooterCollapseExpandedImage = My.Resources.Resources.collapse_24
        tree.FooterHeight = 40
        tree.FooterIconSize = New Size(24, 24)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " PLĂȚI"
        tree.HeaderFont = New Font("Tahoma", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Color.Black
        tree.HeaderGradientEndColor = Color.CornflowerBlue
        tree.HeaderHeight = 40
        tree.HeaderIconSize = New Size(24, 24)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.FooterRightIconTooltip = "Reîncarcă plățile de la server."
        tree.HeaderSearchIconTooltip = "Caută în arbore." & vbLf & "ESC golește căutarea și închide banda."
        tree.Indent = 8
        tree.ItemHeight = 30
        tree.LeftIconSize = New Size(16, 16)
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.NodeImages = image_list
        tree.Size = New Size(275, 567)
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
        ' innerSplit
        ' 
        innerSplit.Dock = DockStyle.Fill
        innerSplit.Location = New Point(0, 0)
        innerSplit.Name = "innerSplit"
        innerSplit.Orientation = Orientation.Horizontal
        ' 
        ' innerSplit.Panel1
        ' 
        innerSplit.Panel1.Controls.Add(grid)
        ' 
        ' innerSplit.Panel2
        ' 
        innerSplit.Panel2.Controls.Add(detailPane)
        innerSplit.Size = New Size(702, 567)
        innerSplit.SplitterDistance = 340
        innerSplit.SplitterWidth = 9
        innerSplit.TabIndex = 0
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.AutoSizeHeaderHeight = False
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Consolas", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 190
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Număr Doc."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nrdoc"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Visible = False
        KBotDataColumn2.Width = 150
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Data"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "data"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.ValueType = KBotValueType.DateTime
        KBotDataColumn3.Width = 150
        KBotDataColumn4.Aggregate = KBotAggregate.Sum
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Valoare"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "suma"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Plătitor"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "platitor"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 150
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Dock = DockStyle.Fill
        grid.FooterVisible = True
        grid.HeaderHeight = 40
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.ScrollByColumn = True
        grid.Size = New Size(702, 340)
        grid.TabIndex = 0
        ' 
        ' detailPane
        ' 
        detailPane.Controls.Add(detailTable)
        detailPane.Controls.Add(lblDetailMessage)
        detailPane.Dock = DockStyle.Fill
        detailPane.Location = New Point(0, 0)
        detailPane.Name = "detailPane"
        detailPane.Padding = New Padding(8)
        detailPane.Size = New Size(702, 218)
        detailPane.TabIndex = 0
        ' 
        ' detailTable
        ' 
        detailTable.ColumnCount = 2
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130.0F))
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        detailTable.Dock = DockStyle.Fill
        detailTable.Location = New Point(8, 8)
        detailTable.Name = "detailTable"
        detailTable.RowCount = 10
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle())
        detailTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        detailTable.Size = New Size(686, 202)
        detailTable.TabIndex = 0
        detailTable.Visible = False
        ' 
        ' lblDetailMessage
        ' 
        lblDetailMessage.Dock = DockStyle.Fill
        lblDetailMessage.Font = New Font("Segoe UI", 10.0F)
        lblDetailMessage.Location = New Point(8, 8)
        lblDetailMessage.Name = "lblDetailMessage"
        lblDetailMessage.Size = New Size(686, 202)
        lblDetailMessage.TabIndex = 1
        lblDetailMessage.Text = "Selectați o plată."
        lblDetailMessage.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' capNrDoc
        ' 
        capNrDoc.Location = New Point(0, 0)
        capNrDoc.Name = "capNrDoc"
        capNrDoc.Size = New Size(100, 23)
        capNrDoc.TabIndex = 0
        ' 
        ' valNrDoc
        ' 
        valNrDoc.Location = New Point(0, 0)
        valNrDoc.Name = "valNrDoc"
        valNrDoc.Size = New Size(100, 23)
        valNrDoc.TabIndex = 0
        ' 
        ' capDataBanca
        ' 
        capDataBanca.Location = New Point(0, 0)
        capDataBanca.Name = "capDataBanca"
        capDataBanca.Size = New Size(100, 23)
        capDataBanca.TabIndex = 0
        ' 
        ' valDataBanca
        ' 
        valDataBanca.Location = New Point(0, 0)
        valDataBanca.Name = "valDataBanca"
        valDataBanca.Size = New Size(100, 23)
        valDataBanca.TabIndex = 0
        ' 
        ' capDataDoc
        ' 
        capDataDoc.Location = New Point(0, 0)
        capDataDoc.Name = "capDataDoc"
        capDataDoc.Size = New Size(100, 23)
        capDataDoc.TabIndex = 0
        ' 
        ' valDataDoc
        ' 
        valDataDoc.Location = New Point(0, 0)
        valDataDoc.Name = "valDataDoc"
        valDataDoc.Size = New Size(100, 23)
        valDataDoc.TabIndex = 0
        ' 
        ' capReferinta
        ' 
        capReferinta.Location = New Point(0, 0)
        capReferinta.Name = "capReferinta"
        capReferinta.Size = New Size(100, 23)
        capReferinta.TabIndex = 0
        ' 
        ' valReferinta
        ' 
        valReferinta.Location = New Point(0, 0)
        valReferinta.Name = "valReferinta"
        valReferinta.Size = New Size(100, 23)
        valReferinta.TabIndex = 0
        ' 
        ' capPlatitor
        ' 
        capPlatitor.Location = New Point(0, 0)
        capPlatitor.Name = "capPlatitor"
        capPlatitor.Size = New Size(100, 23)
        capPlatitor.TabIndex = 0
        ' 
        ' valPlatitor
        ' 
        valPlatitor.Location = New Point(0, 0)
        valPlatitor.Name = "valPlatitor"
        valPlatitor.Size = New Size(100, 23)
        valPlatitor.TabIndex = 0
        ' 
        ' capCui
        ' 
        capCui.Location = New Point(0, 0)
        capCui.Name = "capCui"
        capCui.Size = New Size(100, 23)
        capCui.TabIndex = 0
        ' 
        ' valCui
        ' 
        valCui.Location = New Point(0, 0)
        valCui.Name = "valCui"
        valCui.Size = New Size(100, 23)
        valCui.TabIndex = 0
        ' 
        ' capIban
        ' 
        capIban.Location = New Point(0, 0)
        capIban.Name = "capIban"
        capIban.Size = New Size(100, 23)
        capIban.TabIndex = 0
        ' 
        ' valIban
        ' 
        valIban.Location = New Point(0, 0)
        valIban.Name = "valIban"
        valIban.Size = New Size(100, 23)
        valIban.TabIndex = 0
        ' 
        ' capDebit
        ' 
        capDebit.Location = New Point(0, 0)
        capDebit.Name = "capDebit"
        capDebit.Size = New Size(100, 23)
        capDebit.TabIndex = 0
        ' 
        ' valDebit
        ' 
        valDebit.Location = New Point(0, 0)
        valDebit.Name = "valDebit"
        valDebit.Size = New Size(100, 23)
        valDebit.TabIndex = 0
        ' 
        ' capCredit
        ' 
        capCredit.Location = New Point(0, 0)
        capCredit.Name = "capCredit"
        capCredit.Size = New Size(100, 23)
        capCredit.TabIndex = 0
        ' 
        ' valCredit
        ' 
        valCredit.Location = New Point(0, 0)
        valCredit.Name = "valCredit"
        valCredit.Size = New Size(100, 23)
        valCredit.TabIndex = 0
        ' 
        ' capExplicatii
        ' 
        capExplicatii.Location = New Point(0, 0)
        capExplicatii.Name = "capExplicatii"
        capExplicatii.Size = New Size(100, 23)
        capExplicatii.TabIndex = 0
        ' 
        ' valExplicatii
        ' 
        valExplicatii.Dock = DockStyle.Fill
        valExplicatii.Location = New Point(0, 0)
        valExplicatii.Name = "valExplicatii"
        valExplicatii.Size = New Size(100, 23)
        valExplicatii.TabIndex = 0
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10.0F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 567)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' PlatiView
        ' 
        AutoScaleDimensions = New SizeF(10.0F, 25.0F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "PlatiView"
        Size = New Size(986, 567)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        innerSplit.Panel1.ResumeLayout(False)
        innerSplit.Panel2.ResumeLayout(False)
        CType(innerSplit, ComponentModel.ISupportInitialize).EndInit()
        innerSplit.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        detailPane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    ' Configurează o pereche etichetă/valoare pe rândul `rowIndex` al tabelului de detaliu.
    ' Nu construiește controale (ele trăiesc ca fields, regula casei) — doar le pune în tabel.
    Private Sub InitDetailPair(caption As Label, text As String, value As Label, rowIndex As Integer)
        caption.AutoSize = True
        caption.Margin = New Padding(3, 4, 8, 4)
        caption.Name = "cap" & rowIndex.ToString()
        caption.Text = text
        value.AutoSize = True
        value.Margin = New Padding(3, 4, 3, 4)
        value.Name = "val" & rowIndex.ToString()
        value.Text = String.Empty
        detailTable.Controls.Add(caption, 0, rowIndex)
        detailTable.Controls.Add(value, 1, rowIndex)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents innerSplit As SplitContainer
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents detailPane As Panel
    Friend WithEvents detailTable As TableLayoutPanel
    Friend WithEvents capNrDoc As Label
    Friend WithEvents valNrDoc As Label
    Friend WithEvents capDataBanca As Label
    Friend WithEvents valDataBanca As Label
    Friend WithEvents capDataDoc As Label
    Friend WithEvents valDataDoc As Label
    Friend WithEvents capReferinta As Label
    Friend WithEvents valReferinta As Label
    Friend WithEvents capPlatitor As Label
    Friend WithEvents valPlatitor As Label
    Friend WithEvents capCui As Label
    Friend WithEvents valCui As Label
    Friend WithEvents capIban As Label
    Friend WithEvents valIban As Label
    Friend WithEvents capDebit As Label
    Friend WithEvents valDebit As Label
    Friend WithEvents capCredit As Label
    Friend WithEvents valCredit As Label
    Friend WithEvents capExplicatii As Label
    Friend WithEvents valExplicatii As Label
    Friend WithEvents lblDetailMessage As Label
    Friend WithEvents lblEmpty As Label
    Friend WithEvents image_list As ImageList
End Class
