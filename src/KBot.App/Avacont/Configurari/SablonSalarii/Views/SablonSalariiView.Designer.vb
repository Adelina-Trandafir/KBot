Imports KBot.Controls

' «Salarii» page of the «Sabloane» window (slice AV01-15, redone in AV01-16): the salary templates, laid out like the Access form OCS -- on the left the tree of
' chapters (the subunits) with the articles under them (40 % of the width); on the right a bar with the four tables of the article (the commitment, the retentions,
' the viraments, the cash payments), each one a view of its own that takes the whole height. The classification of the article is the first column of the
' commitment table. The only button of the page is «Salveaza».
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 96 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SablonSalariiView
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
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn13 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn14 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn15 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn16 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn17 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn18 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn19 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn20 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn21 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn22 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn23 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn24 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn25 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn26 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn27 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn28 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn29 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn30 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn31 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn32 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn33 As KBotDataColumn = New KBotDataColumn()
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyRight = New KBotTableLayoutPanel()
        navTabele = New KBotNavList()
        pnlTabele = New Panel()
        gridAng = New KBotDataView()
        gridRet = New KBotDataView()
        gridVir = New KBotDataView()
        gridNum = New KBotDataView()
        tlySubsol = New KBotTableLayoutPanel()
        lblStare = New Label()
        btnSalveaza = New Button()
        CType(navTabele, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridAng, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridRet, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridVir, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridNum, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyRight.SuspendLayout()
        pnlTabele.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(pnlCard, 0, 0)
        tlyMain.Controls.Add(tlySubsol, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 2
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 62F))
        tlyMain.TabIndex = 0
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(8, 6, 8, 6)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyBody
        '
        tlyBody.AutoFitToTheme = False
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60F))
        tlyBody.Controls.Add(tree, 0, 0)
        tlyBody.Controls.Add(tlyRight, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.TabIndex = 0
        '
        ' tree
        '
        tree.BackColor = SystemColors.Window
        tree.BorderColor = SystemColors.ActiveBorder
        tree.Dock = DockStyle.Fill
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " ARTICOLE DE SALARII"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.Margin = New Padding(0, 0, 8, 0)
        tree.Name = "tree"
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… căutare articol …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.TabIndex = 0
        '
        ' tlyRight
        '
        tlyRight.AutoFitToTheme = False
        tlyRight.ColumnCount = 1
        tlyRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRight.Controls.Add(navTabele, 0, 0)
        tlyRight.Controls.Add(pnlTabele, 0, 1)
        tlyRight.Dock = DockStyle.Fill
        tlyRight.Margin = New Padding(0)
        tlyRight.Name = "tlyRight"
        tlyRight.RowCount = 2
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRight.TabIndex = 1
        '
        ' navTabele
        '
        navTabele.Dock = DockStyle.Fill
        navTabele.IconSize = 16
        navTabele.ItemCornerRadius = 2
        navTabele.ItemPadding = New Padding(3)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.cells
        KBotNavItem1.Key = "ang"
        KBotNavItem1.Text = "Angajamente"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.credit_card
        KBotNavItem2.Key = "ret"
        KBotNavItem2.Text = "Rețineri"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.binvoice
        KBotNavItem3.Key = "vir"
        KBotNavItem3.Text = "Viramente"
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.folder_open
        KBotNavItem4.Key = "num"
        KBotNavItem4.Text = "Numerar"
        navTabele.Items.Add(KBotNavItem1)
        navTabele.Items.Add(KBotNavItem2)
        navTabele.Items.Add(KBotNavItem3)
        navTabele.Items.Add(KBotNavItem4)
        navTabele.Location = New Point(0, 0)
        navTabele.Margin = New Padding(0)
        navTabele.Name = "navTabele"
        navTabele.Orientation = KBotNavOrientation.Horizontal
        navTabele.SelectedKey = "ang"
        navTabele.Size = New Size(590, 40)
        navTabele.TabIndex = 0
        '
        ' pnlTabele
        '
        pnlTabele.Controls.Add(gridNum)
        pnlTabele.Controls.Add(gridVir)
        pnlTabele.Controls.Add(gridRet)
        pnlTabele.Controls.Add(gridAng)
        pnlTabele.Dock = DockStyle.Fill
        pnlTabele.Margin = New Padding(0, 4, 0, 0)
        pnlTabele.Name = "pnlTabele"
        pnlTabele.TabIndex = 1
        '
        ' gridAng
        '
        gridAng.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAng.BackColor = SystemColors.Window
        gridAng.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Clasificația"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 90
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 120
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Explicație"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "expl"
        KBotDataColumn2.MinWidth = 120
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 260
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnType = KBotColumnType.Combo
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "C. Debit"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "contd"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 96
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnType = KBotColumnType.Combo
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "P. Debit"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "partd"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 80
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Combo
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "C. Credit"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "contc"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 96
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBotColumnType.Combo
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "P. Credit"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "partc"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Width = 80
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Ref"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "ref"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn7.Width = 40
        gridAng.Columns.Add(KBotDataColumn1)
        gridAng.Columns.Add(KBotDataColumn2)
        gridAng.Columns.Add(KBotDataColumn3)
        gridAng.Columns.Add(KBotDataColumn4)
        gridAng.Columns.Add(KBotDataColumn5)
        gridAng.Columns.Add(KBotDataColumn6)
        gridAng.Columns.Add(KBotDataColumn7)
        gridAng.Dock = DockStyle.Fill
        gridAng.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridAng.FillColumnKey = "expl"
        gridAng.HeaderHeight = 31
        gridAng.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridAng.Margin = New Padding(0)
        gridAng.Name = "gridAng"
        gridAng.RowHeight = 31
        gridAng.TabIndex = 0
        '
        ' gridRet
        '
        gridRet.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridRet.BackColor = SystemColors.Window
        gridRet.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "CM"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "cm"
        KBotDataColumn8.MinWidth = 44
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.Resizable = False
        KBotDataColumn8.Width = 44
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Explicație"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "expl"
        KBotDataColumn9.MinWidth = 120
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.Width = 260
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.ColumnType = KBotColumnType.Combo
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "C. Debit"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "contd"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Width = 96
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.ColumnType = KBotColumnType.Combo
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "P. Debit"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "partd"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Width = 80
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.ColumnType = KBotColumnType.Combo
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "C. Credit"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "contc"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Width = 96
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.ColumnType = KBotColumnType.Combo
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "P. Credit"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "partc"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Width = 80
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.ButtonBorders = KBotBorderSides.None
        KBotDataColumn14.ButtonMargin = New Padding(0)
        KBotDataColumn14.ColumnType = KBotColumnType.Button
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = ""
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Key = "sterge"
        KBotDataColumn14.MinWidth = 34
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.Resizable = False
        KBotDataColumn14.Width = 34
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Ref"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "ref"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.ReadOnly = True
        KBotDataColumn15.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn15.Width = 40
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Par"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn16.Key = "par"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn16.Width = 40
        gridRet.Columns.Add(KBotDataColumn8)
        gridRet.Columns.Add(KBotDataColumn9)
        gridRet.Columns.Add(KBotDataColumn10)
        gridRet.Columns.Add(KBotDataColumn11)
        gridRet.Columns.Add(KBotDataColumn12)
        gridRet.Columns.Add(KBotDataColumn13)
        gridRet.Columns.Add(KBotDataColumn14)
        gridRet.Columns.Add(KBotDataColumn15)
        gridRet.Columns.Add(KBotDataColumn16)
        gridRet.Dock = DockStyle.Fill
        gridRet.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridRet.FillColumnKey = "expl"
        gridRet.HeaderHeight = 24
        gridRet.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridRet.Margin = New Padding(0)
        gridRet.Name = "gridRet"
        gridRet.RowHeight = 24
        gridRet.TabIndex = 1
        gridRet.Visible = False
        '
        ' gridVir
        '
        gridVir.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridVir.BackColor = SystemColors.Window
        gridVir.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "CM"
        KBotDataColumn17.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn17.Key = "cm"
        KBotDataColumn17.MinWidth = 44
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.Resizable = False
        KBotDataColumn17.Width = 44
        KBotDataColumn18.AggregateFormatString = Nothing
        KBotDataColumn18.FormatString = Nothing
        KBotDataColumn18.HeaderText = "Explicație"
        KBotDataColumn18.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn18.Key = "expl"
        KBotDataColumn18.MinWidth = 120
        KBotDataColumn18.OptionGroup = Nothing
        KBotDataColumn18.Width = 260
        KBotDataColumn19.AggregateFormatString = Nothing
        KBotDataColumn19.ColumnType = KBotColumnType.Combo
        KBotDataColumn19.FormatString = Nothing
        KBotDataColumn19.HeaderText = "C. Debit"
        KBotDataColumn19.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn19.Key = "contd"
        KBotDataColumn19.OptionGroup = Nothing
        KBotDataColumn19.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn19.Width = 96
        KBotDataColumn20.AggregateFormatString = Nothing
        KBotDataColumn20.ColumnType = KBotColumnType.Combo
        KBotDataColumn20.FormatString = Nothing
        KBotDataColumn20.HeaderText = "P. Debit"
        KBotDataColumn20.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Key = "partd"
        KBotDataColumn20.OptionGroup = Nothing
        KBotDataColumn20.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Width = 80
        KBotDataColumn21.AggregateFormatString = Nothing
        KBotDataColumn21.ColumnType = KBotColumnType.Combo
        KBotDataColumn21.FormatString = Nothing
        KBotDataColumn21.HeaderText = "C. Credit"
        KBotDataColumn21.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Key = "contc"
        KBotDataColumn21.OptionGroup = Nothing
        KBotDataColumn21.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Width = 96
        KBotDataColumn22.AggregateFormatString = Nothing
        KBotDataColumn22.ColumnType = KBotColumnType.Combo
        KBotDataColumn22.FormatString = Nothing
        KBotDataColumn22.HeaderText = "P. Credit"
        KBotDataColumn22.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Key = "partc"
        KBotDataColumn22.OptionGroup = Nothing
        KBotDataColumn22.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Width = 80
        KBotDataColumn23.AggregateFormatString = Nothing
        KBotDataColumn23.ButtonBorders = KBotBorderSides.None
        KBotDataColumn23.ButtonMargin = New Padding(0)
        KBotDataColumn23.ColumnType = KBotColumnType.Button
        KBotDataColumn23.FormatString = Nothing
        KBotDataColumn23.HeaderText = ""
        KBotDataColumn23.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.Key = "sterge"
        KBotDataColumn23.MinWidth = 34
        KBotDataColumn23.OptionGroup = Nothing
        KBotDataColumn23.Resizable = False
        KBotDataColumn23.Width = 34
        KBotDataColumn24.AggregateFormatString = Nothing
        KBotDataColumn24.FormatString = Nothing
        KBotDataColumn24.HeaderText = "Ref"
        KBotDataColumn24.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.Key = "ref"
        KBotDataColumn24.OptionGroup = Nothing
        KBotDataColumn24.ReadOnly = True
        KBotDataColumn24.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn24.Width = 40
        KBotDataColumn25.AggregateFormatString = Nothing
        KBotDataColumn25.FormatString = Nothing
        KBotDataColumn25.HeaderText = "Par"
        KBotDataColumn25.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Key = "par"
        KBotDataColumn25.OptionGroup = Nothing
        KBotDataColumn25.ReadOnly = True
        KBotDataColumn25.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn25.Width = 40
        gridVir.Columns.Add(KBotDataColumn17)
        gridVir.Columns.Add(KBotDataColumn18)
        gridVir.Columns.Add(KBotDataColumn19)
        gridVir.Columns.Add(KBotDataColumn20)
        gridVir.Columns.Add(KBotDataColumn21)
        gridVir.Columns.Add(KBotDataColumn22)
        gridVir.Columns.Add(KBotDataColumn23)
        gridVir.Columns.Add(KBotDataColumn24)
        gridVir.Columns.Add(KBotDataColumn25)
        gridVir.Dock = DockStyle.Fill
        gridVir.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridVir.FillColumnKey = "expl"
        gridVir.HeaderHeight = 24
        gridVir.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridVir.Margin = New Padding(0)
        gridVir.Name = "gridVir"
        gridVir.RowHeight = 24
        gridVir.TabIndex = 2
        gridVir.Visible = False
        '
        ' gridNum
        '
        gridNum.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridNum.BackColor = SystemColors.Window
        gridNum.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn26.AggregateFormatString = Nothing
        KBotDataColumn26.FormatString = Nothing
        KBotDataColumn26.HeaderText = "Explicație"
        KBotDataColumn26.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn26.Key = "expl"
        KBotDataColumn26.MinWidth = 120
        KBotDataColumn26.OptionGroup = Nothing
        KBotDataColumn26.Width = 260
        KBotDataColumn27.AggregateFormatString = Nothing
        KBotDataColumn27.ColumnType = KBotColumnType.Combo
        KBotDataColumn27.FormatString = Nothing
        KBotDataColumn27.HeaderText = "C. Debit"
        KBotDataColumn27.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn27.Key = "contd"
        KBotDataColumn27.OptionGroup = Nothing
        KBotDataColumn27.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn27.Width = 96
        KBotDataColumn28.AggregateFormatString = Nothing
        KBotDataColumn28.ColumnType = KBotColumnType.Combo
        KBotDataColumn28.FormatString = Nothing
        KBotDataColumn28.HeaderText = "P. Debit"
        KBotDataColumn28.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn28.Key = "partd"
        KBotDataColumn28.OptionGroup = Nothing
        KBotDataColumn28.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn28.Width = 80
        KBotDataColumn29.AggregateFormatString = Nothing
        KBotDataColumn29.ColumnType = KBotColumnType.Combo
        KBotDataColumn29.FormatString = Nothing
        KBotDataColumn29.HeaderText = "C. Credit"
        KBotDataColumn29.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn29.Key = "contc"
        KBotDataColumn29.OptionGroup = Nothing
        KBotDataColumn29.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn29.Width = 96
        KBotDataColumn30.AggregateFormatString = Nothing
        KBotDataColumn30.ColumnType = KBotColumnType.Combo
        KBotDataColumn30.FormatString = Nothing
        KBotDataColumn30.HeaderText = "P. Credit"
        KBotDataColumn30.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn30.Key = "partc"
        KBotDataColumn30.OptionGroup = Nothing
        KBotDataColumn30.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn30.Width = 80
        KBotDataColumn31.AggregateFormatString = Nothing
        KBotDataColumn31.ButtonBorders = KBotBorderSides.None
        KBotDataColumn31.ButtonMargin = New Padding(0)
        KBotDataColumn31.ColumnType = KBotColumnType.Button
        KBotDataColumn31.FormatString = Nothing
        KBotDataColumn31.HeaderText = ""
        KBotDataColumn31.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn31.Key = "sterge"
        KBotDataColumn31.MinWidth = 34
        KBotDataColumn31.OptionGroup = Nothing
        KBotDataColumn31.Resizable = False
        KBotDataColumn31.Width = 34
        KBotDataColumn32.AggregateFormatString = Nothing
        KBotDataColumn32.FormatString = Nothing
        KBotDataColumn32.HeaderText = "Ref"
        KBotDataColumn32.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn32.Key = "ref"
        KBotDataColumn32.OptionGroup = Nothing
        KBotDataColumn32.ReadOnly = True
        KBotDataColumn32.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn32.Width = 40
        KBotDataColumn33.AggregateFormatString = Nothing
        KBotDataColumn33.FormatString = Nothing
        KBotDataColumn33.HeaderText = "Par"
        KBotDataColumn33.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn33.Key = "par"
        KBotDataColumn33.OptionGroup = Nothing
        KBotDataColumn33.ReadOnly = True
        KBotDataColumn33.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn33.Width = 40
        gridNum.Columns.Add(KBotDataColumn26)
        gridNum.Columns.Add(KBotDataColumn27)
        gridNum.Columns.Add(KBotDataColumn28)
        gridNum.Columns.Add(KBotDataColumn29)
        gridNum.Columns.Add(KBotDataColumn30)
        gridNum.Columns.Add(KBotDataColumn31)
        gridNum.Columns.Add(KBotDataColumn32)
        gridNum.Columns.Add(KBotDataColumn33)
        gridNum.Dock = DockStyle.Fill
        gridNum.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridNum.FillColumnKey = "expl"
        gridNum.HeaderHeight = 24
        gridNum.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridNum.Margin = New Padding(0)
        gridNum.Name = "gridNum"
        gridNum.RowHeight = 24
        gridNum.TabIndex = 3
        gridNum.Visible = False
        '
        ' tlySubsol
        '
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 2
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(lblStare, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 1, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.TabIndex = 1
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Margin = New Padding(0, 0, 12, 0)
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnSalveaza
        '
        btnSalveaza.Anchor = AnchorStyles.Right
        btnSalveaza.AutoSize = True
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(18, 6, 18, 6)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' SablonSalariiView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(0)
        Name = "SablonSalariiView"
        Size = New Size(1000, 600)
        CType(navTabele, ComponentModel.ISupportInitialize).EndInit()
        CType(gridAng, ComponentModel.ISupportInitialize).EndInit()
        CType(gridRet, ComponentModel.ISupportInitialize).EndInit()
        CType(gridVir, ComponentModel.ISupportInitialize).EndInit()
        CType(gridNum, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyRight.ResumeLayout(False)
        pnlTabele.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyRight As KBotTableLayoutPanel
    Friend WithEvents navTabele As KBotNavList
    Friend WithEvents pnlTabele As Panel
    Friend WithEvents gridAng As KBotDataView
    Friend WithEvents gridRet As KBotDataView
    Friend WithEvents gridVir As KBotDataView
    Friend WithEvents gridNum As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
End Class
