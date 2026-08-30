Imports KBot.Controls

' Editorul de asociere R <-> H (felia 0048-04) — echivalentul gazdei Access `frmFX_ASOC`.
'
' GAZDA ACCESS NU E IN EXPORT. `frmFX_DUBII_LISTA_HA.Form_Open` si `..._RH.Form_Open` se
' ramifica pe `isLoaded("frmFX_ASOC")`, deci Access avea DOUA gazde peste aceleasi patru
' subformulare — `frmFX_DUBII` in timpul ingestiei si `frmFX_ASOC` oricand — dar numai
' subformularele sunt in `FX_System_Export/FORMS`. Regulile vin de acolo si sunt portate;
' ASPECTUL de mai jos este PROIECTAT, nu portat. Consemnat in worklog ca neverificat.
'
' Cele patru panouri Access devin trei zone, fiindca aici mutarea se face TRAGAND:
'   stanga        = recepțiile cu lanturile lor      (Access: `_LISTA` + `_LISTA_HA`)
'   dreapta sus   = instantaneele inca neasezate     (Access: `_LISTA_HN`)
'   dreapta jos   = liniile rândului selectat        (Access: `_LISTA_RH`)
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AsociereForm))
        tips = New KBotToolTip(components)
        pnlCard = New Panel()
        split = New SplitContainer()
        treeLant = New AdvancedTreeControl()
        splitDreapta = New SplitContainer()
        treeLibere = New AdvancedTreeControl()
        grid = New KBotDataView()
        ntfMesaj = New KBotNotice()
        lblIntro = New Label()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        capBar = New KBotCaptionBar()
        tlyAsociere = New TableLayoutPanel()
        il = New ImageList(components)
        pnlCard.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(splitDreapta, ComponentModel.ISupportInitialize).BeginInit()
        splitDreapta.Panel1.SuspendLayout()
        splitDreapta.Panel2.SuspendLayout()
        splitDreapta.SuspendLayout()
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
        split.Panel1.Controls.Add(treeLant)
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
        ' treeLant
        ' 
        treeLant.Dock = DockStyle.Fill
        treeLant.DragEnabled = True
        treeLant.ExpanderSize = 10
        treeLant.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        treeLant.HeaderCaption = " RECEPȚII ȘI LANȚURILE LOR"
        treeLant.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        treeLant.HeaderHeight = 30
        treeLant.HeaderVisible = True
        treeLant.Indent = 12
        treeLant.Location = New Point(0, 0)
        treeLant.Margin = New Padding(4, 5, 4, 5)
        treeLant.MinimumCollapsedWidth = 120
        treeLant.Name = "treeLant"
        treeLant.NodeImages = il
        treeLant.PaddingExpanderGap = 8
        treeLant.PaddingIconGap = 8
        treeLant.Size = New Size(407, 557)
        treeLant.TabIndex = 0
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
        splitDreapta.Panel1.Controls.Add(treeLibere)
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
        ' treeLibere
        ' 
        treeLibere.Dock = DockStyle.Fill
        treeLibere.DragEnabled = True
        treeLibere.ExpanderSize = 10
        treeLibere.Font = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        treeLibere.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        treeLibere.HeaderCaption = " INSTANTANEE NEAȘEZATE"
        treeLibere.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        treeLibere.HeaderHeight = 30
        treeLibere.HeaderVisible = True
        treeLibere.Indent = 12
        treeLibere.Location = New Point(0, 0)
        treeLibere.Margin = New Padding(4, 5, 4, 5)
        treeLibere.Name = "treeLibere"
        treeLibere.PaddingExpanderGap = 8
        treeLibere.PaddingIconGap = 8
        treeLibere.Size = New Size(634, 308)
        treeLibere.TabIndex = 0
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
        ' il
        ' 
        il.ColorDepth = ColorDepth.Depth32Bit
        il.ImageStream = CType(resources.GetObject("il.ImageStream"), ImageListStreamer)
        il.TransparentColor = Color.Transparent
        il.Images.SetKeyName(0, "link_del")
        il.Images.SetKeyName(1, "link_break")
        il.Images.SetKeyName(2, "link_add")
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
        splitDreapta.Panel1.ResumeLayout(False)
        splitDreapta.Panel2.ResumeLayout(False)
        CType(splitDreapta, ComponentModel.ISupportInitialize).EndInit()
        splitDreapta.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyAsociere.ResumeLayout(False)
        tlyAsociere.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents lblIntro As Label
    Friend WithEvents split As SplitContainer
    Friend WithEvents treeLant As KBot.Controls.AdvancedTreeControl
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents treeLibere As KBot.Controls.AdvancedTreeControl
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As KBot.Controls.KBotNotice
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyAsociere As TableLayoutPanel
    Friend WithEvents il As ImageList
End Class
