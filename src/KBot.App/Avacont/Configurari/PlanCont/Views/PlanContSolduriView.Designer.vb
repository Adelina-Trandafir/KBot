Imports KBot.Controls

' «Solduri initiale» view of the «Plan de conturi» window (slice AV01-04): one grid, a row per account of the subunit,
' where the opening balances and the cumulated turnovers are typed. No empty row to add: the accounts come from the chart.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PlanContSolduriView
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
        tips = New KBotToolTip(components)
        tlyRoot = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        gridSolduri = New KBotDataView()
        CType(gridSolduri, ComponentModel.ISupportInitialize).BeginInit()
        tlyRoot.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlyRoot
        ' 
        tlyRoot.ColumnCount = 1
        tlyRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRoot.Controls.Add(lblTitlu, 0, 0)
        tlyRoot.Controls.Add(gridSolduri, 0, 1)
        tlyRoot.Dock = DockStyle.Fill
        tlyRoot.Location = New Point(0, 0)
        tlyRoot.Margin = New Padding(0)
        tlyRoot.Name = "tlyRoot"
        tlyRoot.RowCount = 2
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRoot.Size = New Size(1412, 760)
        tlyRoot.TabIndex = 0
        ' 
        ' lblTitlu
        ' 
        lblTitlu.AutoEllipsis = True
        lblTitlu.Dock = DockStyle.Fill
        lblTitlu.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblTitlu.Margin = New Padding(0)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.TabIndex = 0
        lblTitlu.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' gridSolduri
        ' 
        gridSolduri.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridSolduri.BackColor = SystemColors.Window
        gridSolduri.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Simbol Cont"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "simbol"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 360
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.DecimalPlaces = 2
        KBotDataColumn2.Format = KBotFormat.Standard
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "S.I. Debitor"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "sid"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.ValueType = KBotValueType.Number
        KBotDataColumn2.Width = 220
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "S.I. Creditor"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "sic"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 220
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "R.P. Debitor"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "rpd"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 220
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "R.P. Creditor"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "rpc"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 220
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Id"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "idcont"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Visible = KBotColumnVisibility.Hidden
        gridSolduri.Columns.Add(KBotDataColumn1)
        gridSolduri.Columns.Add(KBotDataColumn2)
        gridSolduri.Columns.Add(KBotDataColumn3)
        gridSolduri.Columns.Add(KBotDataColumn4)
        gridSolduri.Columns.Add(KBotDataColumn5)
        gridSolduri.Columns.Add(KBotDataColumn6)
        gridSolduri.Dock = DockStyle.Fill
        gridSolduri.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridSolduri.FooterVisible = False
        gridSolduri.HeaderHeight = 28
        gridSolduri.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridSolduri.Margin = New Padding(0)
        gridSolduri.Name = "gridSolduri"
        gridSolduri.RowHeight = 28
        gridSolduri.Size = New Size(1412, 716)
        gridSolduri.TabIndex = 1
        tips.SetToolTipHeader(gridSolduri, "Solduri inițiale și rulaje")
        tips.SetToolTipText(gridSolduri, "Tastați direct în tabel. Un rând modificat se salvează singur când treceți pe alt rând.")
        ' 
        ' PlanContSolduriView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyRoot)
        Margin = New Padding(0)
        Name = "PlanContSolduriView"
        Size = New Size(1412, 760)
        CType(gridSolduri, ComponentModel.ISupportInitialize).EndInit()
        tlyRoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyRoot As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents gridSolduri As KBotDataView
End Class
