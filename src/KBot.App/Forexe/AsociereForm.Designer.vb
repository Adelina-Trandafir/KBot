Imports KBot.Controls

' Editorul de asociere R <-> H (felia 0048-04) — echivalentul gazdei Access `frmFX_ASOC`.
'
' GAZDA ACCESS NU E IN EXPORT. `frmFX_DUBII_LISTA_HA.Form_Open` si `..._RH.Form_Open` se
' ramifica pe `isLoaded("frmFX_ASOC")`, deci Access avea DOUA gazde peste aceleasi patru
' subformulare — `frmFX_DUBII` in timpul ingestiei si `frmFX_ASOC` oricand — dar numai
' subformularele sunt in `FX_System_Export/FORMS`. Regulile vin de acolo si sunt portate;
' ASPECTUL de mai jos este PROIECTAT, nu portat. Consemnat in worklog ca neverificat.
'
' Cele patru panouri Access devin patru zone, fiindca aici mutarea se face TRAGAND:
'   stanga sus    = recepțiile cu lanturile lor      (Access: `_LISTA` + `_LISTA_HA`)
'   stanga jos    = instantaneele inca neasezate     (Access: `_LISTA_HN`)
'   dreapta jos   = liniile rândului selectat        (Access: `_LISTA_RH`)
'   dreapta sus   = graficul evolutiei (`grafic`), care NU are corespondent in Access —
'                   e locul ramas liber dupa ce cele doua liste au trecut in stanga
' Combo-ul de asezare din `_LISTA_HN` si butonul de desprindere din `_LISTA_HA` se
' contopesc intr-o singura miscare: tragi la stanga ca sa asezi, tragi la dreapta ca sa
' desprinzi.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md): formularul trebuie
' sa se randeze in designerul Visual Studio, nu sa se construiasca la rulare.
'
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste (7, 15).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AsociereForm
    Inherits KBot.Theming.KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AsociereForm))
        Dim TreeNodeDefinition1 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim TreeNodeDefinition2 As TreeNodeDefinition = New TreeNodeDefinition()
        Dim KBotChartTab1 As KBotChartTab = New KBotChartTab()
        Dim KBotChartTab2 As KBotChartTab = New KBotChartTab()
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        pnlCard = New Panel()
        split = New SplitContainer()
        SplitContainer1 = New SplitContainer()
        treeLant = New AdvancedTreeControl()
        Il_Receptii = New ImageList(components)
        treeLibere = New AdvancedTreeControl()
        splitDreapta = New SplitContainer()
        benzi = New KBotLaneView()
        grafic = New KBotChartView()
        navGrafice = New KBotNavList()
        grid = New KBotDataView()
        ntfMesaj = New KBotNotice()
        lblIntro = New Label()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        capBar = New KBotCaptionBar()
        tlyAsociere = New TableLayoutPanel()
        pnlCard.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        CType(splitDreapta, ComponentModel.ISupportInitialize).BeginInit()
        splitDreapta.Panel1.SuspendLayout()
        splitDreapta.Panel2.SuspendLayout()
        splitDreapta.SuspendLayout()
        CType(benzi, ComponentModel.ISupportInitialize).BeginInit()
        CType(grafic, ComponentModel.ISupportInitialize).BeginInit()
        CType(navGrafice, ComponentModel.ISupportInitialize).BeginInit()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyAsociere.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlCard
        ' 
        tlyAsociere.SetColumnSpan(pnlCard, 2)
        pnlCard.Controls.Add(split)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(lblIntro)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 67)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(17, 0, 17, 20)
        pnlCard.Size = New Size(1084, 741)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(17, 71)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(SplitContainer1)
        split.Panel1MinSize = 160
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(splitDreapta)
        split.Panel2MinSize = 160
        split.Size = New Size(1050, 557)
        split.SplitterDistance = 407
        split.SplitterWidth = 9
        split.TabIndex = 2
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.Location = New Point(0, 0)
        SplitContainer1.Margin = New Padding(0)
        SplitContainer1.Name = "SplitContainer1"
        SplitContainer1.Orientation = Orientation.Horizontal
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(treeLant)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(treeLibere)
        SplitContainer1.Size = New Size(407, 557)
        SplitContainer1.SplitterDistance = 314
        SplitContainer1.TabIndex = 1
        ' 
        ' treeLant
        ' 
        treeLant.BorderColor = SystemColors.ActiveBorder
        treeLant.Dock = DockStyle.Fill
        treeLant.DragEnabled = True
        treeLant.ExpanderSize = 10
        treeLant.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        treeLant.HeaderBackColor = SystemColors.Control
        treeLant.HeaderCaption = " RECEPȚII ȘI LANȚURILE LOR"
        treeLant.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        treeLant.HeaderHeight = 30
        treeLant.HeaderIconSize = New Size(18, 18)
        treeLant.HeaderLeftIcon = CType(resources.GetObject("treeLant.HeaderLeftIcon"), Image)
        treeLant.HeaderLeftIconKey = "Receptii"
        treeLant.HeaderSeparatorColor = SystemColors.ActiveBorder
        treeLant.HeaderSeparatorWidth = 2
        treeLant.HeaderVisible = True
        treeLant.Indent = 12
        treeLant.ItemHeight = 24
        treeLant.Location = New Point(0, 0)
        treeLant.Margin = New Padding(4, 5, 4, 5)
        treeLant.MinimumCollapsedWidth = 120
        treeLant.Name = "treeLant"
        treeLant.NodeImages = Il_Receptii
        TreeNodeDefinition1.Caption = "01/01/2026~~~1.234.567,89 (1)"
        TreeNodeDefinition1.Expanded = True
        TreeNodeDefinition1.ImageKey = "Receptii"
        TreeNodeDefinition1.Key = "1"
        TreeNodeDefinition1.OpenImageKey = Nothing
        TreeNodeDefinition1.ParentKey = Nothing
        TreeNodeDefinition1.RightImageKey = Nothing
        TreeNodeDefinition1.Tag = Nothing
        TreeNodeDefinition1.Tooltip = Nothing
        TreeNodeDefinition2.Caption = "01/01/2026 12:34:56~~~1.234.567,89 (X)"
        TreeNodeDefinition2.ImageKey = "Receptii_Link"
        TreeNodeDefinition2.Key = "2"
        TreeNodeDefinition2.OpenImageKey = Nothing
        TreeNodeDefinition2.ParentKey = "1"
        TreeNodeDefinition2.RightImageKey = Nothing
        TreeNodeDefinition2.Tag = Nothing
        TreeNodeDefinition2.Tooltip = Nothing
        treeLant.Nodes.Add(TreeNodeDefinition1)
        treeLant.Nodes.Add(TreeNodeDefinition2)
        treeLant.PaddingExpanderGap = 2
        treeLant.PaddingIconGap = 8
        treeLant.ReserveRightIconSpace = True
        treeLant.RightIconSize = New Size(12, 12)
        treeLant.RootExpander = False
        treeLant.Size = New Size(407, 314)
        treeLant.TabIndex = 1
        treeLant.TooltipShowOnlyOnRightIcon = True
        ' 
        ' Il_Receptii
        ' 
        Il_Receptii.ColorDepth = ColorDepth.Depth32Bit
        Il_Receptii.ImageStream = CType(resources.GetObject("Il_Receptii.ImageStream"), ImageListStreamer)
        Il_Receptii.TransparentColor = Color.Transparent
        Il_Receptii.Images.SetKeyName(0, "Receptii")
        Il_Receptii.Images.SetKeyName(1, "Receptii_Add")
        Il_Receptii.Images.SetKeyName(2, "Receptii_Del")
        Il_Receptii.Images.SetKeyName(3, "Receptii_Edit")
        Il_Receptii.Images.SetKeyName(4, "Receptii_Error")
        Il_Receptii.Images.SetKeyName(5, "Receptii_Link")
        Il_Receptii.Images.SetKeyName(6, "Receptii_Move")
        Il_Receptii.Images.SetKeyName(7, "Lock")
        ' 
        ' treeLibere
        ' 
        treeLibere.BorderColor = SystemColors.ActiveBorder
        treeLibere.Dock = DockStyle.Fill
        treeLibere.DragEnabled = True
        treeLibere.ExpanderSize = 10
        treeLibere.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        treeLibere.HeaderBackColor = SystemColors.Control
        treeLibere.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        treeLibere.HeaderCaption = " INSTANTANEE NEAȘEZATE"
        treeLibere.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        treeLibere.HeaderHeight = 30
        treeLibere.HeaderIconSize = New Size(18, 18)
        treeLibere.HeaderLeftIcon = CType(resources.GetObject("treeLibere.HeaderLeftIcon"), Image)
        treeLibere.HeaderLeftIconKey = "Receptii_Link"
        treeLibere.HeaderSeparatorColor = SystemColors.ActiveBorder
        treeLibere.HeaderSeparatorWidth = 2
        treeLibere.HeaderVisible = True
        treeLibere.Indent = 12
        treeLibere.Location = New Point(0, 0)
        treeLibere.Margin = New Padding(4, 5, 4, 5)
        treeLibere.Name = "treeLibere"
        treeLibere.NodeImages = Il_Receptii
        treeLibere.PaddingExpanderGap = 8
        treeLibere.PaddingIconGap = 8
        treeLibere.Size = New Size(407, 239)
        treeLibere.TabIndex = 1
        ' 
        ' splitDreapta
        ' 
        splitDreapta.Dock = DockStyle.Fill
        splitDreapta.Location = New Point(0, 0)
        splitDreapta.Margin = New Padding(4, 5, 4, 5)
        splitDreapta.Name = "splitDreapta"
        splitDreapta.Orientation = Orientation.Horizontal
        ' 
        ' splitDreapta.Panel1
        ' 
        splitDreapta.Panel1.Controls.Add(benzi)
        splitDreapta.Panel1.Controls.Add(grafic)
        splitDreapta.Panel1.Controls.Add(navGrafice)
        splitDreapta.Panel1MinSize = 80
        ' 
        ' splitDreapta.Panel2
        ' 
        splitDreapta.Panel2.Controls.Add(grid)
        splitDreapta.Panel2MinSize = 80
        splitDreapta.Size = New Size(634, 557)
        splitDreapta.SplitterDistance = 308
        splitDreapta.SplitterWidth = 10
        splitDreapta.TabIndex = 1
        ' 
        ' benzi
        ' 
        benzi.AxisVisible = True
        benzi.Dock = DockStyle.Fill
        benzi.EmptyText = "Trage un instantaneu dintr-o bandă în alta ca să-l muți."
        benzi.EnlargeButtonTooltip = "Deschide benzile mari" & vbCrLf & "Aceleași benzi, cu denumirile întregi și datele pe axă — pentru când tragerea cere loc."
        benzi.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        benzi.HeaderBackColor = SystemColors.Control
        benzi.HeaderCaption = " AȘEZAREA INSTANTANEELOR"
        benzi.HeaderGradient = 5
        benzi.HeaderHeight = 30
        benzi.HeaderSeparatorColor = SystemColors.ActiveBorder
        benzi.HeaderSeparatorWidth = 2
        benzi.LaneCaptionsVisible = True
        benzi.LaneCaptionWidth = 150
        benzi.LaneHeight = 18
        benzi.LaneSpacing = 3
        benzi.Location = New Point(0, 34)
        benzi.Margin = New Padding(4, 5, 4, 5)
        benzi.MarkerSize = 9
        benzi.Name = "benzi"
        benzi.SegmentWidth = 4
        benzi.Size = New Size(634, 274)
        benzi.TabIndex = 1
        benzi.TrailingSpace = 50
        benzi.Visible = False
        ' 
        ' grafic
        ' 
        grafic.Dock = DockStyle.Fill
        grafic.EmptyText = "Alege o recepție în stânga ca să-i vezi evoluția."
        grafic.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        grafic.HeaderBackColor = SystemColors.Control
        grafic.HeaderCaption = " EVOLUȚIA VALORII"
        grafic.HeaderGradient = 5
        grafic.HeaderHeight = 30
        grafic.HeaderSeparatorWidth = 2
        grafic.LegendVisible = False
        grafic.Location = New Point(0, 34)
        grafic.Margin = New Padding(4, 5, 4, 5)
        grafic.Name = "grafic"
        grafic.PlotMargin = 2
        grafic.SelectedTabKey = "receptie"
        grafic.Size = New Size(634, 274)
        grafic.TabHeight = 26
        grafic.TabIndex = 0
        grafic.TabPadding = 4
        KBotChartTab1.Key = "receptie"
        KBotChartTab1.Text = "Recepția"
        KBotChartTab1.Tooltip = "Evoluția recepției alese" & vbCrLf & "Fiecare punct este un instantaneu al lanțului ei, la ora la care a fost salvat."
        KBotChartTab2.Key = "angajament"
        KBotChartTab2.Text = "Tot angajamentul"
        KBotChartTab2.Tooltip = "Evoluția întregului angajament" & vbCrLf & "Câte o linie pentru fiecare recepție, plus linia îngroșată a totalului."
        grafic.Tabs.Add(KBotChartTab1)
        grafic.Tabs.Add(KBotChartTab2)
        ' 
        ' navGrafice
        ' 
        navGrafice.Dock = DockStyle.Top
        navGrafice.ItemPadding = New Padding(0)
        KBotNavItem1.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Chart_curve_32
        KBotNavItem1.Key = "grafic"
        KBotNavItem1.Text = "Grafic"
        KBotNavItem2.Align = KBotNavAlign.Far
        KBotNavItem2.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Barchart_32
        KBotNavItem2.Key = "benzi"
        KBotNavItem2.Text = "Distribuție"
        navGrafice.Items.Add(KBotNavItem1)
        navGrafice.Items.Add(KBotNavItem2)
        navGrafice.Location = New Point(0, 0)
        navGrafice.Name = "navGrafice"
        navGrafice.Orientation = KBotNavOrientation.Horizontal
        navGrafice.SelectedKey = "grafic"
        navGrafice.Size = New Size(634, 34)
        navGrafice.TabIndex = 1
        navGrafice.Text = "KBotNavList1"
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.BackColor = SystemColors.Window
        grid.CellTooltip.Enabled = False
        grid.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Indicator"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "indicator"
        KBotDataColumn1.MinWidth = 60
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 90
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Cod SSI"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "ssi"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 150
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Credit bugetar"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.Key = "credit"
        KBotDataColumn3.MinWidth = 80
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.Width = 120
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Valoare"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.Key = "valoare"
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.Width = 120
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Dock = DockStyle.Fill
        grid.HeaderBackColor = SystemColors.Control
        grid.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grid.HeaderSeparatorColor = SystemColors.ActiveBorder
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.RowHeight = 22
        grid.Size = New Size(634, 239)
        grid.TabIndex = 0
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Bottom
        ntfMesaj.Location = New Point(17, 628)
        ntfMesaj.Margin = New Padding(0, 10, 0, 10)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(1050, 93)
        ntfMesaj.TabIndex = 3
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' lblIntro
        ' 
        lblIntro.Dock = DockStyle.Top
        lblIntro.Location = New Point(17, 0)
        lblIntro.Margin = New Padding(4, 0, 4, 0)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(0, 0, 0, 13)
        lblIntro.Size = New Size(1050, 71)
        lblIntro.TabIndex = 1
        lblIntro.Text = resources.GetString("lblIntro.Text")
        lblIntro.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' btnRenunta
        ' 
        btnRenunta.AutoSize = True
        btnRenunta.Dock = DockStyle.Left
        btnRenunta.Location = New Point(4, 813)
        btnRenunta.Margin = New Padding(4, 5, 4, 5)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(17, 10, 17, 10)
        btnRenunta.Size = New Size(404, 60)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AutoSize = True
        btnSalveaza.Dock = DockStyle.Right
        btnSalveaza.Enabled = False
        btnSalveaza.Location = New Point(675, 813)
        btnSalveaza.Margin = New Padding(4, 5, 4, 5)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(17, 10, 17, 10)
        btnSalveaza.Size = New Size(405, 60)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează legăturile"
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' capBar
        ' 
        tlyAsociere.SetColumnSpan(capBar, 2)
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(1084, 67)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT — Legăturile recepțiilor"
        ' 
        ' tlyAsociere
        ' 
        tlyAsociere.ColumnCount = 2
        tlyAsociere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyAsociere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyAsociere.Controls.Add(btnSalveaza, 1, 2)
        tlyAsociere.Controls.Add(btnRenunta, 0, 2)
        tlyAsociere.Controls.Add(capBar, 0, 0)
        tlyAsociere.Controls.Add(pnlCard, 0, 1)
        tlyAsociere.Dock = DockStyle.Fill
        tlyAsociere.Location = New Point(1, 5)
        tlyAsociere.Margin = New Padding(0)
        tlyAsociere.Name = "tlyAsociere"
        tlyAsociere.RowCount = 3
        tlyAsociere.RowStyles.Add(New RowStyle())
        tlyAsociere.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyAsociere.RowStyles.Add(New RowStyle(SizeType.Absolute, 70F))
        tlyAsociere.Size = New Size(1084, 878)
        tlyAsociere.TabIndex = 2
        ' 
        ' AsociereForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1086, 888)
        Controls.Add(tlyAsociere)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(1086, 867)
        Name = "AsociereForm"
        Padding = New Padding(1, 5, 1, 5)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Legăturile recepțiilor"
        pnlCard.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        splitDreapta.Panel1.ResumeLayout(False)
        splitDreapta.Panel2.ResumeLayout(False)
        CType(splitDreapta, ComponentModel.ISupportInitialize).EndInit()
        splitDreapta.ResumeLayout(False)
        CType(benzi, ComponentModel.ISupportInitialize).EndInit()
        CType(grafic, ComponentModel.ISupportInitialize).EndInit()
        CType(navGrafice, ComponentModel.ISupportInitialize).EndInit()
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyAsociere.ResumeLayout(False)
        tlyAsociere.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents lblIntro As Label
    Friend WithEvents split As SplitContainer
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents grafic As KBot.Controls.KBotChartView
    Friend WithEvents benzi As KBot.Controls.KBotLaneView
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As KBot.Controls.KBotNotice
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyAsociere As TableLayoutPanel
    Friend WithEvents Il_Receptii As ImageList
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents treeLant As AdvancedTreeControl
    Friend WithEvents treeLibere As AdvancedTreeControl
    Friend WithEvents navGrafice As KBotNavList
End Class
