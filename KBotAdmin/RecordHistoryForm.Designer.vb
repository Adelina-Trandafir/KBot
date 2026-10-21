' The "history of a row" window (slice ME02-07): one record followed through the saved copies of a database. All controls are
' declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RecordHistoryForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        Dim VersionColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim VersionColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim VersionColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim VersionColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim ValueColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim ValueColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim ValueColumn3 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyKey = New KBotTableLayoutPanel()
        lblKey = New Label()
        txtKey = New KBotTextField()
        btnSearch = New KBotButton()
        gridVersions = New KBotDataView()
        gridValues = New KBotDataView()
        lblState = New Label()
        tlyButtons = New KBotTableLayoutPanel()
        btnCopySql = New KBotButton()
        btnClose = New KBotButton()
        CType(gridVersions, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridValues, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyKey.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyKey, 0, 0)
        tlyMain.Controls.Add(gridVersions, 0, 1)
        tlyMain.Controls.Add(gridValues, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Controls.Add(tlyButtons, 0, 4)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(12)
        tlyMain.RowCount = 5
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 38F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 62F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(900, 640)
        tlyMain.TabIndex = 0
        '
        ' tlyKey
        '
        tlyKey.AutoFitToTheme = False
        tlyKey.ColumnCount = 3
        tlyKey.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyKey.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyKey.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyKey.Controls.Add(lblKey, 0, 0)
        tlyKey.Controls.Add(txtKey, 1, 0)
        tlyKey.Controls.Add(btnSearch, 2, 0)
        tlyKey.Dock = DockStyle.Fill
        tlyKey.Location = New Point(12, 12)
        tlyKey.Margin = New Padding(0)
        tlyKey.Name = "tlyKey"
        tlyKey.RowCount = 1
        tlyKey.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyKey.Size = New Size(876, 48)
        tlyKey.TabIndex = 0
        '
        ' lblKey
        '
        tips.SetToolTipHeader(lblKey, "Cheia rândului")
        tips.SetToolTipText(lblKey, "Valorile cheii primare a tabelei, în ordinea arătată; la o cheie din mai multe coloane, separate prin ;")
        lblKey.Dock = DockStyle.Fill
        lblKey.Location = New Point(12, 12)
        lblKey.Margin = New Padding(0)
        lblKey.Name = "lblKey"
        lblKey.Size = New Size(300, 48)
        lblKey.TabIndex = 0
        lblKey.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtKey
        '
        txtKey.BackColor = Color.Transparent
        txtKey.Dock = DockStyle.Fill
        txtKey.Location = New Point(312, 15)
        txtKey.Margin = New Padding(0, 3, 6, 3)
        txtKey.Name = "txtKey"
        txtKey.Size = New Size(458, 34)
        txtKey.TabIndex = 1
        txtKey.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtKey, "Cheia")
        tips.SetToolTipText(txtKey, "De exemplu 1532 sau, pentru o cheie din trei coloane, 2;1;877. Enter caută.")
        '
        ' btnSearch
        '
        btnSearch.Dock = DockStyle.Fill
        btnSearch.Location = New Point(770, 12)
        btnSearch.Margin = New Padding(0, 4, 0, 4)
        btnSearch.Name = "btnSearch"
        btnSearch.Primary = True
        btnSearch.Size = New Size(110, 40)
        btnSearch.TabIndex = 2
        btnSearch.Text = "Caută"
        tips.SetToolTipHeader(btnSearch, "Caută")
        tips.SetToolTipText(btnSearch, "Caută rândul în fiecare copie a bazei și arată cum a arătat de fiecare dată. Citește doar din dosarul copiilor, nu din server.")
        '
        ' gridVersions
        '
        gridVersions.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridVersions.ColumnFillMode = KBotFillMode.SpecificColumn
        VersionColumn1.AggregateFormatString = Nothing
        VersionColumn1.FormatString = Nothing
        VersionColumn1.HeaderText = "Copia din"
        VersionColumn1.HeaderTooltip = "Când s-a terminat copia."
        VersionColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        VersionColumn1.Key = "when"
        VersionColumn1.MinWidth = 100
        VersionColumn1.OptionGroup = Nothing
        VersionColumn1.ReadOnly = True
        VersionColumn1.Width = 150
        VersionColumn2.AggregateFormatString = Nothing
        VersionColumn2.FormatString = Nothing
        VersionColumn2.HeaderText = "Starea rândului"
        VersionColumn2.HeaderTooltip = "Nu există încă, apare, neschimbat, modificat, a dispărut."
        VersionColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        VersionColumn2.Key = "state"
        VersionColumn2.MinWidth = 90
        VersionColumn2.OptionGroup = Nothing
        VersionColumn2.ReadOnly = True
        VersionColumn2.Width = 130
        VersionColumn3.AggregateFormatString = Nothing
        VersionColumn3.FormatString = Nothing
        VersionColumn3.HeaderText = "Fel"
        VersionColumn3.HeaderTooltip = "manual sau periodic."
        VersionColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        VersionColumn3.Key = "kind"
        VersionColumn3.MinWidth = 50
        VersionColumn3.OptionGroup = Nothing
        VersionColumn3.ReadOnly = True
        VersionColumn3.Width = 80
        VersionColumn4.AggregateFormatString = Nothing
        VersionColumn4.FormatString = Nothing
        VersionColumn4.HeaderText = "Coloane schimbate"
        VersionColumn4.HeaderTooltip = "Coloanele care au altă valoare decât în copia precedentă în care rândul exista."
        VersionColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        VersionColumn4.Key = "changed"
        VersionColumn4.MinWidth = 120
        VersionColumn4.OptionGroup = Nothing
        VersionColumn4.ReadOnly = True
        VersionColumn4.Width = 400
        gridVersions.Columns.Add(VersionColumn1)
        gridVersions.Columns.Add(VersionColumn2)
        gridVersions.Columns.Add(VersionColumn3)
        gridVersions.Columns.Add(VersionColumn4)
        gridVersions.Dock = DockStyle.Fill
        gridVersions.FillColumnKey = "changed"
        gridVersions.HeaderHeight = 24
        gridVersions.Location = New Point(12, 60)
        gridVersions.Margin = New Padding(0, 0, 0, 6)
        gridVersions.Name = "gridVersions"
        gridVersions.ReadOnlyGrid = True
        gridVersions.Size = New Size(876, 220)
        gridVersions.TabIndex = 1
        tips.SetToolTipHeader(gridVersions, "Copiile")
        tips.SetToolTipText(gridVersions, "Câte un rând pe copie, de la cea mai veche la cea mai nouă. Alegeți una ca să vedeți mai jos valorile rândului atunci.")
        '
        ' gridValues
        '
        gridValues.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridValues.ColumnFillMode = KBotFillMode.SpecificColumn
        ValueColumn1.AggregateFormatString = Nothing
        ValueColumn1.FormatString = Nothing
        ValueColumn1.HeaderText = "Coloana"
        ValueColumn1.HeaderTooltip = "Coloanele tabelei."
        ValueColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        ValueColumn1.Key = "column"
        ValueColumn1.MinWidth = 100
        ValueColumn1.OptionGroup = Nothing
        ValueColumn1.ReadOnly = True
        ValueColumn1.Width = 200
        ValueColumn2.AggregateFormatString = Nothing
        ValueColumn2.FormatString = Nothing
        ValueColumn2.HeaderText = "Valoarea în copia aleasă"
        ValueColumn2.HeaderTooltip = "Valoarea coloanei la acel moment. [NULL] = fără valoare; [binar] = conținut binar (PDF, imagine)."
        ValueColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        ValueColumn2.Key = "value"
        ValueColumn2.MinWidth = 120
        ValueColumn2.OptionGroup = Nothing
        ValueColumn2.ReadOnly = True
        ValueColumn2.Width = 330
        ValueColumn3.AggregateFormatString = Nothing
        ValueColumn3.FormatString = Nothing
        ValueColumn3.HeaderText = "Valoarea înainte"
        ValueColumn3.HeaderTooltip = "Valoarea din copia precedentă în care rândul exista; se vede doar la coloanele care s-au schimbat."
        ValueColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        ValueColumn3.Key = "before"
        ValueColumn3.MinWidth = 120
        ValueColumn3.OptionGroup = Nothing
        ValueColumn3.ReadOnly = True
        ValueColumn3.Width = 330
        gridValues.Columns.Add(ValueColumn1)
        gridValues.Columns.Add(ValueColumn2)
        gridValues.Columns.Add(ValueColumn3)
        gridValues.Dock = DockStyle.Fill
        gridValues.FillColumnKey = "value"
        gridValues.HeaderHeight = 24
        gridValues.Location = New Point(12, 286)
        gridValues.Margin = New Padding(0)
        gridValues.Name = "gridValues"
        gridValues.ReadOnlyGrid = True
        gridValues.Size = New Size(876, 262)
        gridValues.TabIndex = 2
        tips.SetToolTipHeader(gridValues, "Valorile")
        tips.SetToolTipText(gridValues, "Valorile rândului în copia aleasă sus; la coloanele schimbate se vede și valoarea de dinainte.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Ce face căutarea sau câte copii s-au parcurs.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(12, 548)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(876, 28)
        lblState.TabIndex = 3
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 280F))
        tlyButtons.Controls.Add(btnClose, 0, 0)
        tlyButtons.Controls.Add(btnCopySql, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(12, 576)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(876, 52)
        tlyButtons.TabIndex = 4
        '
        ' btnClose
        '
        btnClose.DialogResult = DialogResult.Cancel
        btnClose.Dock = DockStyle.Fill
        btnClose.Location = New Point(0, 0)
        btnClose.Margin = New Padding(0, 6, 6, 6)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(114, 40)
        btnClose.TabIndex = 0
        btnClose.Text = "Închide"
        tips.SetToolTipHeader(btnClose, "Închide")
        tips.SetToolTipText(btnClose, "Închide fereastra.")
        '
        ' btnCopySql
        '
        btnCopySql.Dock = DockStyle.Fill
        btnCopySql.Location = New Point(596, 0)
        btnCopySql.Margin = New Padding(6, 6, 0, 6)
        btnCopySql.Name = "btnCopySql"
        btnCopySql.Size = New Size(274, 40)
        btnCopySql.TabIndex = 1
        btnCopySql.Text = "Copiază SQL pentru această versiune"
        tips.SetToolTipHeader(btnCopySql, "SQL pentru versiunea aleasă")
        tips.SetToolTipText(btnCopySql, "Pune în clipboard o comandă care readuce rândul la valorile din copia aleasă (INSERT ... ON DUPLICATE KEY UPDATE). Nu o rulează: o citiți și o rulați dumneavoastră.")
        '
        ' RecordHistoryForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(900, 640)
        Controls.Add(tlyMain)
        MinimumSize = New Size(760, 520)
        Name = "RecordHistoryForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Istoricul unui rând"
        CType(gridVersions, ComponentModel.ISupportInitialize).EndInit()
        CType(gridValues, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyKey.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyKey As KBotTableLayoutPanel
    Friend WithEvents lblKey As Label
    Friend WithEvents txtKey As KBotTextField
    Friend WithEvents btnSearch As KBotButton
    Friend WithEvents gridVersions As KBotDataView
    Friend WithEvents gridValues As KBotDataView
    Friend WithEvents lblState As Label
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCopySql As KBotButton
    Friend WithEvents btnClose As KBotButton
End Class
