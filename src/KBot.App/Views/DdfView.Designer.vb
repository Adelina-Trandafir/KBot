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
        split = New SplitContainer()
        pnlTreeHead = New Panel()
        lblTreeTitle = New Label()
        tree = New Controls.AdvancedTreeControl()
        navSub = New Theming.KBotNavList()
        pnlPages = New Panel()
        pnlValori = New Panel()
        pnlFilter = New Panel()
        lblClsf = New Label()
        cboClsf = New ComboBox()
        grid = New Controls.KBotDataView()
        pnlPreview = New Panel()
        lblPreviewGol = New Label()
        pnlFisiere = New Panel()
        lblFisiereGol = New Label()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        pnlTreeHead.SuspendLayout()
        pnlPages.SuspendLayout()
        pnlValori.SuspendLayout()
        pnlFilter.SuspendLayout()
        pnlPreview.SuspendLayout()
        pnlFisiere.SuspendLayout()
        SuspendLayout()
        '
        ' split — arborele de revizii (stânga) | paginile sub-navigării (dreapta)
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' Ordine INVERSĂ de andocare: Fill întâi, apoi Top (ultimul Top rămâne cel mai sus).
        split.Panel1.Controls.Add(tree)
        split.Panel1.Controls.Add(pnlTreeHead)
        split.Panel2.Controls.Add(pnlPages)
        split.Panel2.Controls.Add(navSub)
        split.Size = New Size(986, 567)
        split.SplitterDistance = 336
        split.SplitterWidth = 9
        split.TabIndex = 0
        '
        ' pnlTreeHead — banda de titlu a arborelui
        '
        pnlTreeHead.Controls.Add(lblTreeTitle)
        pnlTreeHead.Dock = DockStyle.Top
        pnlTreeHead.Height = 28
        pnlTreeHead.Location = New Point(0, 0)
        pnlTreeHead.Name = "pnlTreeHead"
        pnlTreeHead.TabIndex = 0
        '
        ' lblTreeTitle
        '
        lblTreeTitle.Dock = DockStyle.Fill
        lblTreeTitle.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblTreeTitle.Name = "lblTreeTitle"
        lblTreeTitle.Padding = New Padding(6, 0, 0, 0)
        lblTreeTitle.TabIndex = 0
        lblTreeTitle.Text = "Revizii"
        lblTreeTitle.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tree — două niveluri: lună (rădăcină) -> revizie (frunză)
        '
        tree.AutoScrollMinSize = New Size(0, 0)
        tree.BackColor = Color.White
        tree.BorderColor = Color.Transparent
        tree.Dock = DockStyle.Fill
        tree.Font = New Font("Segoe UI", 9F)
        tree.HeaderBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.HeaderForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.HeaderIconSize = New Size(16, 16)
        tree.HoverBackColor = Color.FromArgb(CByte(230), CByte(240), CByte(255))
        tree.ItemHeight = 24
        tree.LeftIconSize = New Size(16, 16)
        tree.LineColor = Color.FromArgb(CByte(160), CByte(160), CByte(160))
        tree.Location = New Point(0, 28)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.Name = "tree"
        tree.ReserveRightIconSpace = False
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 110
        tree.SearchBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.SearchBarFontSize = 10F
        tree.SearchBarLabelForeColor = Color.Empty
        tree.SearchBoxBackColor = Color.Empty
        tree.SelectedBackColor = Color.FromArgb(CByte(200), CByte(220), CByte(255))
        tree.SelectedBorderColor = Color.FromArgb(CByte(150), CByte(180), CByte(255))
        tree.Size = New Size(336, 539)
        tree.TabIndex = 1
        tree.TooltipBackColor = Color.FromArgb(CByte(255), CByte(255), CByte(232))
        tree.TooltipForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.TreeFont = New Font("Consolas", 9F)
        '
        ' navSub — sub-navigarea ORIZONTALĂ (decizia 8 a operatorului)
        '
        navSub.Dock = DockStyle.Top
        navSub.Height = 34
        navSub.Location = New Point(0, 0)
        navSub.Name = "navSub"
        navSub.Orientation = Theming.KBotNavOrientation.Horizontal
        navSub.TabIndex = 0
        '
        ' pnlPages — gazda celor trei pagini (doar una Visible odată; NU un TabControl)
        '
        pnlPages.Controls.Add(pnlValori)
        pnlPages.Controls.Add(pnlPreview)
        pnlPages.Controls.Add(pnlFisiere)
        pnlPages.Dock = DockStyle.Fill
        pnlPages.Location = New Point(0, 34)
        pnlPages.Name = "pnlPages"
        pnlPages.TabIndex = 1
        '
        ' pnlValori — pagina «Valori»: filtrul de clasificație peste grilă
        '
        ' Ordine INVERSĂ de andocare: grila (Fill) întâi, apoi banda de filtru (Top).
        pnlValori.Controls.Add(grid)
        pnlValori.Controls.Add(pnlFilter)
        pnlValori.Dock = DockStyle.Fill
        pnlValori.Location = New Point(0, 0)
        pnlValori.Name = "pnlValori"
        pnlValori.TabIndex = 0
        '
        ' pnlFilter — banda de filtrare pe clasificație
        '
        pnlFilter.Controls.Add(cboClsf)
        pnlFilter.Controls.Add(lblClsf)
        pnlFilter.Dock = DockStyle.Top
        pnlFilter.Height = 32
        pnlFilter.Location = New Point(0, 0)
        pnlFilter.Name = "pnlFilter"
        pnlFilter.Padding = New Padding(6, 4, 6, 4)
        pnlFilter.TabIndex = 0
        '
        ' lblClsf
        '
        lblClsf.AutoSize = True
        lblClsf.Dock = DockStyle.Left
        lblClsf.Name = "lblClsf"
        lblClsf.Padding = New Padding(0, 5, 8, 0)
        lblClsf.TabIndex = 0
        lblClsf.Text = "Clasificație:"
        lblClsf.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboClsf — ComboBox simplu (DropDownList + Flat), temat de ThemeManager ca
        ' cboUnit / cboAn din MainForm.
        '
        cboClsf.Dock = DockStyle.Left
        cboClsf.DropDownStyle = ComboBoxStyle.DropDownList
        cboClsf.FlatStyle = FlatStyle.Flat
        cboClsf.Name = "cboClsf"
        cboClsf.Size = New Size(280, 24)
        cboClsf.TabIndex = 1
        '
        ' grid — liniile de secțiune A (read-only, cu rând de totaluri)
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
        grid.Location = New Point(0, 32)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 28
        grid.ScrollByColumn = True
        grid.ShowHeader = True
        grid.ShowTotalsRow = True
        grid.Size = New Size(641, 501)
        grid.TabIndex = 1
        '
        ' pnlPreview — pagina «Vizualizare». Suprafața de previzualizare se montează aici
        ' în felia 03; până atunci arată doar starea goală.
        '
        pnlPreview.Controls.Add(lblPreviewGol)
        pnlPreview.Dock = DockStyle.Fill
        pnlPreview.Location = New Point(0, 0)
        pnlPreview.Name = "pnlPreview"
        pnlPreview.TabIndex = 1
        pnlPreview.Visible = False
        '
        ' lblPreviewGol
        '
        lblPreviewGol.Dock = DockStyle.Fill
        lblPreviewGol.Font = New Font("Segoe UI", 10F)
        lblPreviewGol.Name = "lblPreviewGol"
        lblPreviewGol.TabIndex = 0
        lblPreviewGol.Text = "Selectați o revizie din arbore."
        lblPreviewGol.TextAlign = ContentAlignment.MiddleCenter
        '
        ' pnlFisiere — pagina «Fișiere». Lista de PDF-uri se montează aici în felia 04.
        '
        pnlFisiere.Controls.Add(lblFisiereGol)
        pnlFisiere.Dock = DockStyle.Fill
        pnlFisiere.Location = New Point(0, 0)
        pnlFisiere.Name = "pnlFisiere"
        pnlFisiere.TabIndex = 2
        pnlFisiere.Visible = False
        '
        ' lblFisiereGol
        '
        lblFisiereGol.Dock = DockStyle.Fill
        lblFisiereGol.Font = New Font("Segoe UI", 10F)
        lblFisiereGol.Name = "lblFisiereGol"
        lblFisiereGol.TabIndex = 0
        lblFisiereGol.Text = "Selectați un angajament din arbore."
        lblFisiereGol.TextAlign = ContentAlignment.MiddleCenter
        '
        ' lblEmpty — starea goală / de încărcare a vederii
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
        ' DdfView
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfView"
        Size = New Size(986, 567)
        pnlFisiere.ResumeLayout(False)
        pnlPreview.ResumeLayout(False)
        pnlFilter.ResumeLayout(False)
        pnlFilter.PerformLayout()
        pnlValori.ResumeLayout(False)
        pnlPages.ResumeLayout(False)
        pnlTreeHead.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents pnlTreeHead As Panel
    Friend WithEvents lblTreeTitle As Label
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents navSub As KBot.Theming.KBotNavList
    Friend WithEvents pnlPages As Panel
    Friend WithEvents pnlValori As Panel
    Friend WithEvents pnlFilter As Panel
    Friend WithEvents lblClsf As Label
    Friend WithEvents cboClsf As ComboBox
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlPreview As Panel
    Friend WithEvents lblPreviewGol As Label
    Friend WithEvents pnlFisiere As Panel
    Friend WithEvents lblFisiereGol As Label
    Friend WithEvents lblEmpty As Label
End Class
