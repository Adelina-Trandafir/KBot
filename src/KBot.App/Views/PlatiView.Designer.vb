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
        split = New SplitContainer()
        tree = New Controls.AdvancedTreeControl()
        innerSplit = New SplitContainer()
        grid = New Controls.KBotDataView()
        detailPane = New Panel()
        detailTable = New TableLayoutPanel()
        capNrDoc = New Label() : valNrDoc = New Label()
        capDataBanca = New Label() : valDataBanca = New Label()
        capDataDoc = New Label() : valDataDoc = New Label()
        capReferinta = New Label() : valReferinta = New Label()
        capPlatitor = New Label() : valPlatitor = New Label()
        capCui = New Label() : valCui = New Label()
        capIban = New Label() : valIban = New Label()
        capDebit = New Label() : valDebit = New Label()
        capCredit = New Label() : valCredit = New Label()
        capExplicatii = New Label() : valExplicatii = New Label()
        lblDetailMessage = New Label()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(innerSplit, ComponentModel.ISupportInitialize).BeginInit()
        innerSplit.Panel1.SuspendLayout()
        innerSplit.Panel2.SuspendLayout()
        innerSplit.SuspendLayout()
        detailPane.SuspendLayout()
        detailTable.SuspendLayout()
        SuspendLayout()
        '
        ' split — arbore (stânga) | dreapta (grilă peste detaliu)
        '
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        split.Panel1.Controls.Add(tree)
        split.Panel2.Controls.Add(innerSplit)
        split.Size = New Size(986, 567)
        split.SplitterDistance = 336
        split.SplitterWidth = 9
        split.TabIndex = 0
        '
        ' tree
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
        tree.Location = New Point(0, 0)
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
        tree.Size = New Size(336, 567)
        tree.TabIndex = 0
        tree.TooltipBackColor = Color.FromArgb(CByte(255), CByte(255), CByte(232))
        tree.TooltipForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.TreeFont = New Font("Consolas", 9F)
        '
        ' innerSplit — grilă (sus) / detaliu extras (jos)
        '
        innerSplit.Dock = DockStyle.Fill
        innerSplit.Location = New Point(0, 0)
        innerSplit.Name = "innerSplit"
        innerSplit.Orientation = Orientation.Horizontal
        innerSplit.Panel1.Controls.Add(grid)
        innerSplit.Panel2.Controls.Add(detailPane)
        innerSplit.Size = New Size(641, 567)
        innerSplit.SplitterDistance = 340
        innerSplit.SplitterWidth = 9
        innerSplit.TabIndex = 0
        '
        ' grid — LISTA (read-only, cu rând de totaluri)
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
        grid.ShowTotalsRow = True
        grid.Size = New Size(641, 340)
        grid.TabIndex = 0
        '
        ' detailPane — panoul de detaliu (extrasul bancar)
        '
        detailPane.Controls.Add(detailTable)
        detailPane.Controls.Add(lblDetailMessage)
        detailPane.Dock = DockStyle.Fill
        detailPane.Location = New Point(0, 0)
        detailPane.Name = "detailPane"
        detailPane.Padding = New Padding(8)
        detailPane.Size = New Size(641, 218)
        detailPane.TabIndex = 0
        '
        ' detailTable — perechi etichetă/valoare pentru extras
        '
        detailTable.ColumnCount = 2
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130.0F))
        detailTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        detailTable.Dock = DockStyle.Fill
        detailTable.Name = "detailTable"
        detailTable.RowCount = 10
        For i As Integer = 0 To 8
            detailTable.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next
        detailTable.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))   ' Explicații absoarbe
        detailTable.TabIndex = 0
        detailTable.Visible = False
        '
        ' Perechile etichetă/valoare
        '
        InitDetailPair(capNrDoc, "Nr. document", valNrDoc, 0)
        InitDetailPair(capDataBanca, "Data bancă", valDataBanca, 1)
        InitDetailPair(capDataDoc, "Data document", valDataDoc, 2)
        InitDetailPair(capReferinta, "Referință", valReferinta, 3)
        InitDetailPair(capPlatitor, "Plătitor", valPlatitor, 4)
        InitDetailPair(capCui, "CUI", valCui, 5)
        InitDetailPair(capIban, "IBAN", valIban, 6)
        InitDetailPair(capDebit, "Sumă debit", valDebit, 7)
        InitDetailPair(capCredit, "Sumă credit", valCredit, 8)
        InitDetailPair(capExplicatii, "Explicații", valExplicatii, 9)
        valExplicatii.AutoSize = False
        valExplicatii.Dock = DockStyle.Fill
        '
        ' lblDetailMessage — starea goală a panoului de detaliu
        '
        lblDetailMessage.Dock = DockStyle.Fill
        lblDetailMessage.Font = New Font("Segoe UI", 10F)
        lblDetailMessage.Name = "lblDetailMessage"
        lblDetailMessage.TabIndex = 1
        lblDetailMessage.Text = "Selectați o plată."
        lblDetailMessage.TextAlign = ContentAlignment.MiddleCenter
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
        ' PlatiView
        '
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "PlatiView"
        Size = New Size(986, 567)
        detailTable.ResumeLayout(False)
        detailTable.PerformLayout()
        detailPane.ResumeLayout(False)
        innerSplit.Panel1.ResumeLayout(False)
        innerSplit.Panel2.ResumeLayout(False)
        CType(innerSplit, ComponentModel.ISupportInitialize).EndInit()
        innerSplit.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
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
End Class
