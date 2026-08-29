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
        tips = New KBotToolTip(components)
        pnlCard = New Panel()
        split = New SplitContainer()
        treeLant = New AdvancedTreeControl()
        splitDreapta = New SplitContainer()
        treeLibere = New AdvancedTreeControl()
        grid = New KBotDataView()
        ntfMesaj = New KBotNotice()
        tlpButtons = New TableLayoutPanel()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        lblIntro = New Label()
        capBar = New KBotCaptionBar()
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
        tlpButtons.SuspendLayout()
        SuspendLayout()
        '
        ' treeLant
        '
        treeLant.Dock = DockStyle.Fill
        treeLant.DragEnabled = True
        treeLant.ExpanderSize = 10
        treeLant.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        treeLant.HeaderCaption = " RECEPȚII ȘI LANȚURILE LOR"
        treeLant.HeaderHeight = 26
        treeLant.HeaderIconSize = New Size(16, 16)
        treeLant.HeaderVisible = True
        treeLant.Indent = 12
        treeLant.Location = New Point(0, 0)
        treeLant.MinimumCollapsedWidth = 120
        treeLant.Name = "treeLant"
        treeLant.PaddingExpanderGap = 8
        treeLant.PaddingIconGap = 8
        treeLant.Size = New Size(470, 452)
        treeLant.TabIndex = 0
        '
        ' treeLibere
        '
        treeLibere.Dock = DockStyle.Fill
        treeLibere.DragEnabled = True
        treeLibere.ExpanderSize = 10
        treeLibere.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        treeLibere.HeaderCaption = " INSTANTANEE NEAȘEZATE"
        treeLibere.HeaderHeight = 26
        treeLibere.HeaderIconSize = New Size(16, 16)
        treeLibere.HeaderVisible = True
        treeLibere.Indent = 12
        treeLibere.Location = New Point(0, 0)
        treeLibere.Name = "treeLibere"
        treeLibere.PaddingExpanderGap = 8
        treeLibere.PaddingIconGap = 8
        treeLibere.Size = New Size(414, 250)
        treeLibere.TabIndex = 0
        '
        ' grid
        '
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.CellTooltip.Enabled = False
        grid.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Indicator"
        KBotDataColumn1.Key = "indicator"
        KBotDataColumn1.MinWidth = 60
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 90
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Cod SSI"
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
        grid.Name = "grid"
        grid.RowHeight = 22
        grid.Size = New Size(414, 198)
        grid.TabIndex = 0
        '
        ' splitDreapta.Panel1
        '
        splitDreapta.Panel1.Controls.Add(treeLibere)
        '
        ' splitDreapta.Panel2
        '
        splitDreapta.Panel2.Controls.Add(grid)
        '
        ' splitDreapta
        '
        splitDreapta.Dock = DockStyle.Fill
        splitDreapta.Location = New Point(0, 0)
        splitDreapta.Name = "splitDreapta"
        splitDreapta.Orientation = Orientation.Horizontal
        splitDreapta.Panel1MinSize = 80
        splitDreapta.Panel2MinSize = 80
        splitDreapta.Size = New Size(414, 452)
        splitDreapta.SplitterDistance = 250
        splitDreapta.SplitterWidth = 6
        splitDreapta.TabIndex = 1
        '
        ' split.Panel1
        '
        split.Panel1.Controls.Add(treeLant)
        '
        ' split.Panel2
        '
        split.Panel2.Controls.Add(splitDreapta)
        '
        ' split
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(12, 78)
        split.Name = "split"
        split.Panel1MinSize = 160
        split.Panel2MinSize = 160
        split.Size = New Size(890, 452)
        split.SplitterDistance = 470
        split.SplitterWidth = 6
        split.TabIndex = 2
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Bottom
        ntfMesaj.Location = New Point(12, 530)
        ntfMesaj.Margin = New Padding(0, 6, 0, 6)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(890, 56)
        ntfMesaj.TabIndex = 3
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' btnRenunta
        '
        btnRenunta.AutoSize = True
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Location = New Point(3, 3)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(12, 6, 12, 6)
        btnRenunta.Size = New Size(439, 33)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Location = New Point(448, 3)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(12, 6, 12, 6)
        btnSalveaza.Size = New Size(439, 33)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează legăturile"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' tlpButtons
        '
        tlpButtons.AutoSize = True
        tlpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpButtons.ColumnCount = 2
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlpButtons.Controls.Add(btnRenunta, 0, 0)
        tlpButtons.Controls.Add(btnSalveaza, 1, 0)
        tlpButtons.Dock = DockStyle.Bottom
        tlpButtons.Location = New Point(12, 586)
        tlpButtons.Margin = New Padding(0)
        tlpButtons.Name = "tlpButtons"
        tlpButtons.RowCount = 1
        tlpButtons.RowStyles.Add(New RowStyle())
        tlpButtons.Size = New Size(890, 39)
        tlpButtons.TabIndex = 4
        tlpButtons.Tag = "Card"
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Top
        lblIntro.Location = New Point(12, 44)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(0, 0, 0, 8)
        lblIntro.Size = New Size(890, 34)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Trage un instantaneu peste recepția căreia îi aparține. Trage-l înapoi la dreapta ca să-l desprinzi. Legăturile pe care s-a construit deja o ordonanțare sau pe care s-au calculat plăți rămân vizibile, dar nu se mai pot muta."
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(12, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(890, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Legăturile recepțiilor"
        '
        ' pnlCard
        '
        ' ÎN ORDINE INVERSĂ DE ANDOCARE (regula casei): Fill întâi, apoi Bottom, apoi Top.
        ' Ultimul Top adăugat ajunge cel mai sus, ultimul Bottom cel mai jos.
        pnlCard.Controls.Add(split)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(tlpButtons)
        pnlCard.Controls.Add(lblIntro)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 0, 12, 12)
        pnlCard.Size = New Size(914, 637)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' AsociereForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(916, 643)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(760, 520)
        Name = "AsociereForm"
        Padding = New Padding(1, 3, 1, 3)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Legăturile recepțiilor"
        pnlCard.ResumeLayout(False)
        pnlCard.PerformLayout()
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        splitDreapta.Panel1.ResumeLayout(False)
        splitDreapta.Panel2.ResumeLayout(False)
        CType(splitDreapta, ComponentModel.ISupportInitialize).EndInit()
        splitDreapta.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlpButtons.ResumeLayout(False)
        tlpButtons.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents split As SplitContainer
    Friend WithEvents treeLant As KBot.Controls.AdvancedTreeControl
    Friend WithEvents splitDreapta As SplitContainer
    Friend WithEvents treeLibere As KBot.Controls.AdvancedTreeControl
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As KBot.Controls.KBotNotice
    Friend WithEvents tlpButtons As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
