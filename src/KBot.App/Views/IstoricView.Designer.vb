<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class IstoricView
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
        components = New System.ComponentModel.Container()
        split = New SplitContainer()
        grid = New Controls.KBotDataView()
        pnlDetaliu = New Panel()
        detailTable = New TableLayoutPanel()
        lblCapDescriere = New Label()
        lblCapValori = New Label()
        txtDescriere = New TextBox()
        gridValori = New Controls.KBotDataView()
        pnlFiltre = New Panel()
        btnFiltruClsf = New Button()
        btnFiltruTipRand = New Button()
        btnFiltruData = New Button()
        lblFiltruActiv = New Label()
        btnReset = New Button()
        lblEmpty = New Label()
        menuClsf = New ContextMenuStrip(components)
        menuTipRand = New ContextMenuStrip(components)
        menuData = New ContextMenuStrip(components)
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        detailTable.SuspendLayout()
        pnlFiltre.SuspendLayout()
        SuspendLayout()
        '
        ' split — grila (sus) | panoul de detaliu (jos)
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 36)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        split.Orientation = Orientation.Horizontal
        split.Panel1.Controls.Add(grid)
        split.Panel2.Controls.Add(pnlDetaliu)
        split.Size = New Size(986, 531)
        split.SplitterDistance = 360
        split.SplitterWidth = 9
        split.TabIndex = 1
        '
        ' grid — cele 12 coloane (read-only, cu rând de totaluri pe cele trei coloane Access)
        '
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.ToContent
        grid.AutoSizeSampleRows = 200
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.CurrentColumnKey = Nothing
        grid.CurrentRowIndex = -1
        grid.Dock = DockStyle.Fill
        grid.FrozenColumnCount = 0
        grid.HeaderHeight = 30
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 28
        grid.ScrollByColumn = True
        grid.ShowHeader = True
        grid.ShowTotalsRow = False
        grid.Size = New Size(986, 360)
        grid.TabIndex = 0
        '
        ' pnlDetaliu — panoul de detaliu: Descriere (stânga) + Observații (dreapta)
        '
        pnlDetaliu.Controls.Add(detailTable)
        pnlDetaliu.Dock = DockStyle.Fill
        pnlDetaliu.Location = New Point(0, 0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Padding = New Padding(6)
        pnlDetaliu.TabIndex = 0
        pnlDetaliu.Tag = "Card"
        '
        ' detailTable — 2 coloane egale, 2 rânduri (captiune + text)
        '
        detailTable.ColumnCount = 2
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        detailTable.Controls.Add(lblCapDescriere, 0, 0)
        detailTable.Controls.Add(lblCapValori, 1, 0)
        detailTable.Controls.Add(txtDescriere, 0, 1)
        detailTable.Controls.Add(gridValori, 1, 1)
        detailTable.Dock = DockStyle.Fill
        detailTable.Location = New Point(6, 6)
        detailTable.Name = "detailTable"
        detailTable.RowCount = 2
        detailTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 22.0F))
        detailTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        detailTable.TabIndex = 0
        '
        ' lblCapDescriere
        '
        lblCapDescriere.AutoSize = True
        lblCapDescriere.Dock = DockStyle.Fill
        lblCapDescriere.Name = "lblCapDescriere"
        lblCapDescriere.TabIndex = 0
        lblCapDescriere.Text = "Descriere"
        lblCapDescriere.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCapValori — captiunea grilei de valori non-zero a rândului selectat
        '
        lblCapValori.AutoSize = True
        lblCapValori.Dock = DockStyle.Fill
        lblCapValori.Name = "lblCapValori"
        lblCapValori.TabIndex = 1
        lblCapValori.Text = "Valori"
        lblCapValori.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDescriere — read-only, multiline, cu scroll vertical
        '
        txtDescriere.Dock = DockStyle.Fill
        txtDescriere.Margin = New Padding(0, 0, 3, 0)
        txtDescriere.Multiline = True
        txtDescriere.Name = "txtDescriere"
        txtDescriere.ReadOnly = True
        txtDescriere.ScrollBars = ScrollBars.Vertical
        txtDescriere.TabIndex = 2
        '
        ' gridValori — grila mică Tip | Valoare (valorile <> 0 ale rândului selectat)
        '
        gridValori.AlternatingRows = True
        gridValori.BackColor = SystemColors.Window
        gridValori.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        gridValori.CurrentColumnKey = Nothing
        gridValori.CurrentRowIndex = -1
        gridValori.Dock = DockStyle.Fill
        gridValori.HeaderHeight = 26
        gridValori.Margin = New Padding(3, 0, 0, 0)
        gridValori.Name = "gridValori"
        gridValori.ReadOnlyGrid = True
        gridValori.RowHeight = 26
        gridValori.ShowHeader = True
        gridValori.TabIndex = 3
        '
        ' pnlFiltre — banda de filtre (Access bFilter, înlocuit cu trei butoane de meniu)
        '
        pnlFiltre.Controls.Add(lblFiltruActiv)
        pnlFiltre.Controls.Add(btnReset)
        pnlFiltre.Controls.Add(btnFiltruData)
        pnlFiltre.Controls.Add(btnFiltruTipRand)
        pnlFiltre.Controls.Add(btnFiltruClsf)
        pnlFiltre.Dock = DockStyle.Top
        pnlFiltre.Height = 36
        pnlFiltre.Location = New Point(0, 0)
        pnlFiltre.Name = "pnlFiltre"
        pnlFiltre.TabIndex = 0
        pnlFiltre.Tag = "Card"
        '
        ' btnFiltruClsf
        '
        btnFiltruClsf.Location = New Point(6, 5)
        btnFiltruClsf.Name = "btnFiltruClsf"
        btnFiltruClsf.Size = New Size(120, 26)
        btnFiltruClsf.TabIndex = 0
        btnFiltruClsf.Text = "Clasificație ▾"
        btnFiltruClsf.UseVisualStyleBackColor = True
        '
        ' btnFiltruTipRand
        '
        btnFiltruTipRand.Location = New Point(132, 5)
        btnFiltruTipRand.Name = "btnFiltruTipRand"
        btnFiltruTipRand.Size = New Size(110, 26)
        btnFiltruTipRand.TabIndex = 1
        btnFiltruTipRand.Text = "Tip rând ▾"
        btnFiltruTipRand.UseVisualStyleBackColor = True
        '
        ' btnFiltruData
        '
        btnFiltruData.Location = New Point(248, 5)
        btnFiltruData.Name = "btnFiltruData"
        btnFiltruData.Size = New Size(100, 26)
        btnFiltruData.TabIndex = 2
        btnFiltruData.Text = "Data ▾"
        btnFiltruData.UseVisualStyleBackColor = True
        '
        ' lblFiltruActiv
        '
        lblFiltruActiv.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblFiltruActiv.Location = New Point(360, 5)
        lblFiltruActiv.Name = "lblFiltruActiv"
        lblFiltruActiv.Size = New Size(536, 26)
        lblFiltruActiv.TabIndex = 3
        lblFiltruActiv.Text = ""
        lblFiltruActiv.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnReset
        '
        btnReset.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnReset.Location = New Point(902, 5)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(78, 26)
        btnReset.TabIndex = 4
        btnReset.Text = "Reset"
        btnReset.UseVisualStyleBackColor = True
        '
        ' lblEmpty — starea goală / de încărcare a vederii (topmost; acoperă totul când e vizibil)
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 567)
        lblEmpty.TabIndex = 2
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' menuClsf / menuTipRand / menuData
        '
        menuClsf.Name = "menuClsf"
        menuTipRand.Name = "menuTipRand"
        menuData.Name = "menuData"
        '
        ' IstoricView
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ' Ordine INVERSĂ de andocare: split (Fill) întâi, apoi pnlFiltre (Top); lblEmpty ultimul
        ' (cel mai sus în z-order) ca să acopere totul când starea e goală.
        Controls.Add(split)
        Controls.Add(pnlFiltre)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "IstoricView"
        Size = New Size(986, 567)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        pnlDetaliu.ResumeLayout(False)
        detailTable.ResumeLayout(False)
        detailTable.PerformLayout()
        pnlFiltre.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlDetaliu As Panel
    Friend WithEvents detailTable As TableLayoutPanel
    Friend WithEvents lblCapDescriere As Label
    Friend WithEvents lblCapValori As Label
    Friend WithEvents txtDescriere As TextBox
    Friend WithEvents gridValori As KBot.Controls.KBotDataView
    Friend WithEvents pnlFiltre As Panel
    Friend WithEvents btnFiltruClsf As Button
    Friend WithEvents btnFiltruTipRand As Button
    Friend WithEvents btnFiltruData As Button
    Friend WithEvents lblFiltruActiv As Label
    Friend WithEvents btnReset As Button
    Friend WithEvents lblEmpty As Label
    Friend WithEvents menuClsf As ContextMenuStrip
    Friend WithEvents menuTipRand As ContextMenuStrip
    Friend WithEvents menuData As ContextMenuStrip
End Class
