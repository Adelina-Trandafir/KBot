' The «Tabele» page of «Baze de date»: the tables of the chosen database on the left; on the right the rows of the chosen table, and under
' them the cell box (half of the width) and the facts about the table and the chosen column (a quarter each). Every border between panels is
' a splitter the operator drags. Columns of the rows grid are made at run time (they depend on the table).
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DbBrowserView
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        splitMain = New SplitContainer()
        treeTables = New AdvancedTreeControl()
        splitRight = New SplitContainer()
        tlyTop = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        lblInfo = New Label()
        btnReload = New Button()
        btnMore = New KBotButton()
        gridData = New AdminDataView()
        tlyFoot = New KBotTableLayoutPanel()
        btnBack = New Button()
        btnForward = New Button()
        splitBottom = New SplitContainer()
        edCell = New KBotRichTextEditor()
        splitInfo = New SplitContainer()
        gridTable = New AdminDataView()
        gridColumn = New AdminDataView()
        gridQueryColumns = New AdminDataView()
        CType(splitMain, ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        CType(splitRight, ComponentModel.ISupportInitialize).BeginInit()
        splitRight.Panel1.SuspendLayout()
        splitRight.Panel2.SuspendLayout()
        splitRight.SuspendLayout()
        tlyTop.SuspendLayout()
        tlyBar.SuspendLayout()
        CType(gridData, ComponentModel.ISupportInitialize).BeginInit()
        tlyFoot.SuspendLayout()
        CType(splitBottom, ComponentModel.ISupportInitialize).BeginInit()
        splitBottom.Panel1.SuspendLayout()
        splitBottom.Panel2.SuspendLayout()
        splitBottom.SuspendLayout()
        CType(splitInfo, ComponentModel.ISupportInitialize).BeginInit()
        splitInfo.Panel1.SuspendLayout()
        splitInfo.Panel2.SuspendLayout()
        splitInfo.SuspendLayout()
        CType(gridTable, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridColumn, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridQueryColumns, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' splitMain: tables | everything else
        '
        splitMain.Dock = DockStyle.Fill
        splitMain.FixedPanel = FixedPanel.Panel1
        splitMain.Location = New Point(0, 0)
        splitMain.Margin = New Padding(0)
        splitMain.Name = "splitMain"
        splitMain.Panel1.Controls.Add(treeTables)
        splitMain.Panel1MinSize = 120
        splitMain.Panel2.Controls.Add(splitRight)
        splitMain.Panel2MinSize = 300
        splitMain.Size = New Size(1100, 700)
        splitMain.SplitterDistance = 270
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 0
        '
        ' treeTables
        '
        treeTables.Dock = DockStyle.Fill
        treeTables.FooterVisible = False
        treeTables.HeaderCaption = "Tabele"
        treeTables.HeaderVisible = True
        treeTables.ItemHeight = 22
        treeTables.Location = New Point(0, 0)
        treeTables.Margin = New Padding(0)
        treeTables.Name = "treeTables"
        treeTables.Size = New Size(270, 700)
        treeTables.TabIndex = 0
        '
        ' splitRight: rows on top, the strip of three panels under them
        '
        splitRight.Dock = DockStyle.Fill
        splitRight.Location = New Point(0, 0)
        splitRight.Margin = New Padding(0)
        splitRight.Name = "splitRight"
        splitRight.Orientation = Orientation.Horizontal
        splitRight.Panel1.Controls.Add(tlyTop)
        splitRight.Panel1MinSize = 120
        splitRight.Panel2.Controls.Add(splitBottom)
        splitRight.Panel2MinSize = 80
        splitRight.Size = New Size(824, 700)
        splitRight.SplitterDistance = 560
        splitRight.SplitterWidth = 6
        splitRight.TabIndex = 0
        '
        ' tlyTop: the bar over the rows grid, the grid, the back / forward strip under it
        '
        tlyTop.AutoFitToTheme = False
        tlyTop.ColumnCount = 1
        tlyTop.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyTop.Controls.Add(tlyBar, 0, 0)
        tlyTop.Controls.Add(gridData, 0, 1)
        tlyTop.Controls.Add(tlyFoot, 0, 2)
        tlyTop.Dock = DockStyle.Fill
        tlyTop.Location = New Point(0, 0)
        tlyTop.Margin = New Padding(0)
        tlyTop.Name = "tlyTop"
        tlyTop.RowCount = 3
        tlyTop.RowStyles.Add(New RowStyle(SizeType.Absolute, 38F))
        tlyTop.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyTop.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tlyTop.Size = New Size(824, 560)
        tlyTop.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 3
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.Controls.Add(lblInfo, 0, 0)
        tlyBar.Controls.Add(btnReload, 1, 0)
        tlyBar.Controls.Add(btnMore, 2, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(0, 0)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(824, 38)
        tlyBar.TabIndex = 0
        '
        ' lblInfo
        '
        lblInfo.AutoEllipsis = True
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(0, 0)
        lblInfo.Margin = New Padding(6, 0, 0, 0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(554, 38)
        lblInfo.TabIndex = 0
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblInfo, "Starea tabelului")
        tips.SetToolTipText(lblInfo, "Câte rânduri s-au încărcat din tabelul deschis. Numărul din tabel este o estimare a serverului.")
        '
        ' btnReload: a plain flat button (no inner padding, square corners); ButtonStyles.ApplyTrans in ApplyTheme makes it transparent
        '
        btnReload.Dock = DockStyle.Fill
        btnReload.FlatStyle = FlatStyle.Flat
        btnReload.Location = New Point(554, 0)
        btnReload.Margin = New Padding(4, 4, 4, 4)
        btnReload.Name = "btnReload"
        btnReload.Padding = New Padding(0)
        btnReload.Size = New Size(112, 30)
        btnReload.TabIndex = 1
        btnReload.Text = "Reîncarcă"
        tips.SetToolTipHeader(btnReload, "Reîncarcă")
        tips.SetToolTipText(btnReload, "Șterge datele păstrate în memorie pentru baza aleasă și citește din nou din server lista tabelelor și vederea deschisă." & vbLf & "Fără acest buton, ce s-a citit o dată nu se mai cere încă o dată.")
        btnReload.UseVisualStyleBackColor = False
        '
        ' btnMore
        '
        btnMore.Dock = DockStyle.Fill
        btnMore.Enabled = False
        btnMore.Location = New Point(674, 0)
        btnMore.Margin = New Padding(4, 4, 4, 4)
        btnMore.Name = "btnMore"
        btnMore.Size = New Size(142, 30)
        btnMore.TabIndex = 2
        btnMore.Text = "Încarcă încă 1.000"
        tips.SetToolTipHeader(btnMore, "Încarcă încă")
        tips.SetToolTipText(btnMore, "Aduce următoarele 1.000 de rânduri ale tabelului și le adaugă sub cele de acum. Activ doar când mai sunt rânduri.")
        '
        ' gridData: the rows of the table; its columns are made when a table is opened
        '
        gridData.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridData.BackColor = SystemColors.Window
        gridData.Dock = DockStyle.Fill
        gridData.EnableGrouping = True
        gridData.Location = New Point(0, 38)
        gridData.Margin = New Padding(0)
        gridData.Name = "gridData"
        gridData.ReadOnlyGrid = True
        gridData.ShrinkColumnsToFit = False
        gridData.Size = New Size(824, 492)
        gridData.TabIndex = 1
        '
        ' tlyFoot: the two navigation buttons under the rows grid
        '
        tlyFoot.AutoFitToTheme = False
        tlyFoot.ColumnCount = 3
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoot.Controls.Add(btnBack, 0, 0)
        tlyFoot.Controls.Add(btnForward, 1, 0)
        tlyFoot.Dock = DockStyle.Fill
        tlyFoot.Location = New Point(0, 530)
        tlyFoot.Margin = New Padding(0)
        tlyFoot.Name = "tlyFoot"
        tlyFoot.RowCount = 1
        tlyFoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFoot.Size = New Size(824, 30)
        tlyFoot.TabIndex = 2
        '
        ' btnBack: back to the view the rows grid showed before this one (a list of the steps when there are several)
        '
        btnBack.Dock = DockStyle.Fill
        btnBack.Enabled = False
        btnBack.FlatStyle = FlatStyle.Flat
        btnBack.Location = New Point(0, 0)
        btnBack.Margin = New Padding(2, 2, 2, 2)
        btnBack.Name = "btnBack"
        btnBack.Padding = New Padding(0)
        btnBack.Size = New Size(40, 30)
        btnBack.TabIndex = 0
        btnBack.Text = "◀"
        tips.SetToolTipHeader(btnBack, "Înapoi")
        tips.SetToolTipText(btnBack, "Înapoi la vederea de dinaintea celei de acum (tabelul și filtrul ei)." & vbLf & "Dacă s-a ajuns aici prin mai multe tabele, se deschide lista lor, ca să alegi unde te întorci." & vbLf & "Nu e activ pentru un tabel deschis din arbore.")
        btnBack.UseVisualStyleBackColor = False
        '
        ' btnForward: on to a view reached from this one (a list when there are several)
        '
        btnForward.Dock = DockStyle.Fill
        btnForward.Enabled = False
        btnForward.FlatStyle = FlatStyle.Flat
        btnForward.Location = New Point(40, 0)
        btnForward.Margin = New Padding(2, 2, 2, 2)
        btnForward.Name = "btnForward"
        btnForward.Padding = New Padding(0)
        btnForward.Size = New Size(40, 30)
        btnForward.TabIndex = 1
        btnForward.Text = "▶"
        tips.SetToolTipHeader(btnForward, "Înainte")
        tips.SetToolTipText(btnForward, "Merge la un tabel legat deschis de aici (un clic pe o celulă legată, scrisă albastru și subliniat)." & vbLf & "Dacă din vederea de acum s-au deschis mai multe, se deschide lista lor." & vbLf & "Activ doar după ce s-a umblat prin tabelele legate.")
        btnForward.UseVisualStyleBackColor = False
        '
        ' splitBottom: the cell box | the two fact grids
        '
        splitBottom.Dock = DockStyle.Fill
        splitBottom.Location = New Point(0, 0)
        splitBottom.Margin = New Padding(0)
        splitBottom.Name = "splitBottom"
        splitBottom.Panel1.Controls.Add(edCell)
        splitBottom.Panel1MinSize = 100
        splitBottom.Panel2.Controls.Add(splitInfo)
        splitBottom.Panel2MinSize = 160
        splitBottom.Size = New Size(824, 134)
        splitBottom.SplitterDistance = 412
        splitBottom.SplitterWidth = 6
        splitBottom.TabIndex = 0
        '
        ' edCell: what the chosen cell holds, whole
        '
        edCell.Dock = DockStyle.Fill
        edCell.Editabil = False
        edCell.FooterVisible = False
        edCell.HeaderVisible = False
        edCell.Location = New Point(0, 0)
        edCell.Margin = New Padding(0)
        edCell.Name = "edCell"
        edCell.Size = New Size(412, 134)
        edCell.TabIndex = 0
        tips.SetToolTipHeader(edCell, "Conținutul celulei")
        tips.SetToolTipText(edCell, "Ce conține celula aleasă în grilă, întreg: util la textele care nu încap în celulă." & vbLf & "Un clic pe un rând din tabelele de informații din dreapta îi arată aici valoarea.")
        '
        ' splitInfo: facts about the table | facts about the column
        '
        splitInfo.Dock = DockStyle.Fill
        splitInfo.Location = New Point(0, 0)
        splitInfo.Margin = New Padding(0)
        splitInfo.Name = "splitInfo"
        splitInfo.Panel1.Controls.Add(gridTable)
        splitInfo.Panel1MinSize = 80
        splitInfo.Panel2.Controls.Add(gridColumn)
        splitInfo.Panel2.Controls.Add(gridQueryColumns)
        splitInfo.Panel2MinSize = 80
        splitInfo.Size = New Size(406, 134)
        splitInfo.SplitterDistance = 200
        splitInfo.SplitterWidth = 6
        splitInfo.TabIndex = 0
        '
        ' gridTable: facts about the table (property / value)
        '
        gridTable.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridTable.BackColor = SystemColors.Window
        gridTable.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Tabel"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "k"
        KBotDataColumn1.MinWidth = 60
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 120
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Valoare"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "v"
        KBotDataColumn2.MinWidth = 60
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 200
        gridTable.Columns.Add(KBotDataColumn1)
        gridTable.Columns.Add(KBotDataColumn2)
        gridTable.Dock = DockStyle.Fill
        gridTable.FillColumnKey = "v"
        gridTable.Location = New Point(0, 0)
        gridTable.Margin = New Padding(0)
        gridTable.Name = "gridTable"
        gridTable.ReadOnlyGrid = True
        gridTable.Size = New Size(200, 134)
        gridTable.TabIndex = 0
        '
        ' gridColumn: facts about the chosen column (property / value)
        '
        gridColumn.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridColumn.BackColor = SystemColors.Window
        gridColumn.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Coloană"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "k"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 130
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Valoare"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "v"
        KBotDataColumn4.MinWidth = 60
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 200
        gridColumn.Columns.Add(KBotDataColumn3)
        gridColumn.Columns.Add(KBotDataColumn4)
        gridColumn.Dock = DockStyle.Fill
        gridColumn.FillColumnKey = "v"
        gridColumn.Location = New Point(0, 0)
        gridColumn.Margin = New Padding(0)
        gridColumn.Name = "gridColumn"
        gridColumn.ReadOnlyGrid = True
        gridColumn.Size = New Size(200, 134)
        gridColumn.TabIndex = 0
        '
        ' gridQueryColumns: the columns of the query on screen, each with a tick «Fix» (kept on the left when the rows scroll right; any number of them
        ' together). It takes the place of gridColumn while the rows grid shows a query (the code swaps their Visible).
        '
        gridQueryColumns.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridQueryColumns.BackColor = SystemColors.Window
        gridQueryColumns.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Fix"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "on"
        KBotDataColumn5.ValueType = KBotValueType.Boolean
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 48
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Coloană"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "name"
        KBotDataColumn6.MinWidth = 60
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 130
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Tip"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "type"
        KBotDataColumn7.MinWidth = 50
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 90
        gridQueryColumns.Columns.Add(KBotDataColumn5)
        gridQueryColumns.Columns.Add(KBotDataColumn6)
        gridQueryColumns.Columns.Add(KBotDataColumn7)
        gridQueryColumns.Dock = DockStyle.Fill
        gridQueryColumns.FillColumnKey = "name"
        gridQueryColumns.Location = New Point(0, 0)
        gridQueryColumns.Margin = New Padding(0)
        gridQueryColumns.Name = "gridQueryColumns"
        gridQueryColumns.Size = New Size(200, 134)
        gridQueryColumns.TabIndex = 1
        gridQueryColumns.Visible = False
        '
        ' DbBrowserView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(splitMain)
        Margin = New Padding(4, 4, 4, 4)
        Name = "DbBrowserView"
        Size = New Size(1100, 700)
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        splitRight.Panel1.ResumeLayout(False)
        splitRight.Panel2.ResumeLayout(False)
        CType(splitRight, ComponentModel.ISupportInitialize).EndInit()
        splitRight.ResumeLayout(False)
        tlyTop.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        CType(gridData, ComponentModel.ISupportInitialize).EndInit()
        tlyFoot.ResumeLayout(False)
        splitBottom.Panel1.ResumeLayout(False)
        splitBottom.Panel2.ResumeLayout(False)
        CType(splitBottom, ComponentModel.ISupportInitialize).EndInit()
        splitBottom.ResumeLayout(False)
        splitInfo.Panel1.ResumeLayout(False)
        splitInfo.Panel2.ResumeLayout(False)
        CType(splitInfo, ComponentModel.ISupportInitialize).EndInit()
        splitInfo.ResumeLayout(False)
        CType(gridTable, ComponentModel.ISupportInitialize).EndInit()
        CType(gridColumn, ComponentModel.ISupportInitialize).EndInit()
        CType(gridQueryColumns, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents treeTables As AdvancedTreeControl
    Friend WithEvents splitRight As SplitContainer
    Friend WithEvents tlyTop As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents lblInfo As Label
    Friend WithEvents btnReload As Button
    Friend WithEvents btnMore As KBotButton
    Friend WithEvents gridData As AdminDataView
    Friend WithEvents tlyFoot As KBotTableLayoutPanel
    Friend WithEvents btnBack As Button
    Friend WithEvents btnForward As Button
    Friend WithEvents splitBottom As SplitContainer
    Friend WithEvents edCell As KBotRichTextEditor
    Friend WithEvents splitInfo As SplitContainer
    Friend WithEvents gridTable As AdminDataView
    Friend WithEvents gridColumn As AdminDataView
    Friend WithEvents gridQueryColumns As AdminDataView
End Class
