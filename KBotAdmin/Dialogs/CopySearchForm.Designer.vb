' The "search in the copies" window (slice ME04-01): a text looked for in every saved copy of one database. All controls are
' declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CopySearchForm
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
        Dim HitColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim HitColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim HitColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim HitColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim HitColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim HitColumn6 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        lblText = New Label()
        txtText = New KBotTextField()
        lblTable = New Label()
        txtTable = New KBotTextField()
        btnSearch = New KBotButton()
        gridHits = New AdminDataView()
        lblState = New Label()
        tlyButtons = New KBotTableLayoutPanel()
        btnClose = New KBotButton()
        btnHistory = New KBotButton()
        CType(gridHits, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(gridHits, 0, 1)
        tlyMain.Controls.Add(lblState, 0, 2)
        tlyMain.Controls.Add(tlyButtons, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(12)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(1000, 600)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 5
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyBar.Controls.Add(lblText, 0, 0)
        tlyBar.Controls.Add(txtText, 1, 0)
        tlyBar.Controls.Add(lblTable, 2, 0)
        tlyBar.Controls.Add(txtTable, 3, 0)
        tlyBar.Controls.Add(btnSearch, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(12, 12)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(976, 48)
        tlyBar.TabIndex = 0
        '
        ' lblText
        '
        lblText.Dock = DockStyle.Fill
        lblText.Location = New Point(12, 12)
        lblText.Margin = New Padding(0)
        lblText.Name = "lblText"
        lblText.Size = New Size(90, 48)
        lblText.TabIndex = 0
        lblText.Text = "Text căutat:"
        lblText.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtText
        '
        txtText.BackColor = Color.Transparent
        txtText.Dock = DockStyle.Fill
        txtText.Location = New Point(102, 15)
        txtText.Margin = New Padding(0, 3, 6, 3)
        txtText.Name = "txtText"
        txtText.Size = New Size(400, 34)
        txtText.TabIndex = 1
        txtText.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtText, "Textul căutat")
        tips.SetToolTipText(txtText, "Un nume, un număr, un cod: se caută în orice coloană text a oricărui rând din toate copiile bazei, fără diferență între litere mari și mici. Enter caută.")
        '
        ' lblTable
        '
        lblTable.Dock = DockStyle.Fill
        lblTable.Location = New Point(500, 12)
        lblTable.Margin = New Padding(0)
        lblTable.Name = "lblTable"
        lblTable.Size = New Size(70, 48)
        lblTable.TabIndex = 2
        lblTable.Text = "Tabela:"
        lblTable.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtTable
        '
        txtTable.BackColor = Color.Transparent
        txtTable.Dock = DockStyle.Fill
        txtTable.Location = New Point(570, 15)
        txtTable.Margin = New Padding(0, 3, 6, 3)
        txtTable.Name = "txtTable"
        txtTable.Size = New Size(214, 34)
        txtTable.TabIndex = 3
        txtTable.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtTable, "Tabela")
        tips.SetToolTipText(txtTable, "Numele exact al unei tabele, ca să se caute doar în ea. Gol = în toate tabelele (mai încet).")
        '
        ' btnSearch
        '
        btnSearch.Dock = DockStyle.Fill
        btnSearch.Location = New Point(790, 12)
        btnSearch.Margin = New Padding(0, 4, 0, 4)
        btnSearch.Name = "btnSearch"
        btnSearch.Primary = True
        btnSearch.Size = New Size(110, 40)
        btnSearch.TabIndex = 4
        btnSearch.Text = "Caută"
        tips.SetToolTipHeader(btnSearch, "Caută în copii")
        tips.SetToolTipText(btnSearch, "Parcurge copiile bazei și arată fiecare versiune a rândurilor care conțin textul, cu prima și ultima copie în care apare. Citește doar din dosarul copiilor, nu din server.")
        '
        ' gridHits
        '
        gridHits.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridHits.ColumnFillMode = KBotFillMode.SpecificColumn
        HitColumn1.AggregateFormatString = Nothing
        HitColumn1.FormatString = "dd.MM.yyyy HH:mm"
        HitColumn1.HeaderText = "Prima copie"
        HitColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        HitColumn1.Key = "first"
        HitColumn1.ValueType = KBotValueType.DateTime
        HitColumn1.MinWidth = 100
        HitColumn1.OptionGroup = Nothing
        HitColumn1.ReadOnly = True
        HitColumn1.Width = 130
        HitColumn2.AggregateFormatString = Nothing
        HitColumn2.FormatString = "dd.MM.yyyy HH:mm"
        HitColumn2.HeaderText = "Ultima copie"
        HitColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        HitColumn2.Key = "last"
        HitColumn2.ValueType = KBotValueType.DateTime
        HitColumn2.MinWidth = 100
        HitColumn2.OptionGroup = Nothing
        HitColumn2.ReadOnly = True
        HitColumn2.Width = 130
        HitColumn3.AggregateFormatString = Nothing
        HitColumn3.FormatString = Nothing
        HitColumn3.HeaderText = "În copii"
        HitColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        HitColumn3.Key = "copies"
        HitColumn3.ValueType = KBotValueType.Number
        HitColumn3.MinWidth = 50
        HitColumn3.OptionGroup = Nothing
        HitColumn3.ReadOnly = True
        HitColumn3.Width = 70
        HitColumn4.AggregateFormatString = Nothing
        HitColumn4.FormatString = Nothing
        HitColumn4.HeaderText = "Acum"
        HitColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        HitColumn4.Key = "state"
        HitColumn4.MinWidth = 70
        HitColumn4.OptionGroup = Nothing
        HitColumn4.ReadOnly = True
        HitColumn4.Width = 130
        HitColumn5.AggregateFormatString = Nothing
        HitColumn5.FormatString = Nothing
        HitColumn5.HeaderText = "Tabela"
        HitColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        HitColumn5.Key = "table"
        HitColumn5.MinWidth = 80
        HitColumn5.OptionGroup = Nothing
        HitColumn5.ReadOnly = True
        HitColumn5.Width = 170
        HitColumn6.AggregateFormatString = Nothing
        HitColumn6.FormatString = Nothing
        HitColumn6.HeaderText = "Rândul (cheia, apoi valorile)"
        HitColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        HitColumn6.Key = "row"
        HitColumn6.MinWidth = 150
        HitColumn6.OptionGroup = Nothing
        HitColumn6.ReadOnly = True
        HitColumn6.Width = 400
        gridHits.Columns.Add(HitColumn1)
        gridHits.Columns.Add(HitColumn2)
        gridHits.Columns.Add(HitColumn3)
        gridHits.Columns.Add(HitColumn4)
        gridHits.Columns.Add(HitColumn5)
        gridHits.Columns.Add(HitColumn6)
        gridHits.Dock = DockStyle.Fill
        gridHits.FillColumnKey = "row"
        gridHits.Location = New Point(12, 60)
        gridHits.Margin = New Padding(0, 0, 0, 6)
        gridHits.Name = "gridHits"
        gridHits.ReadOnlyGrid = True
        gridHits.Size = New Size(976, 454)
        gridHits.TabIndex = 1
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Ce face căutarea sau câte rezultate a găsit.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(12, 514)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(976, 28)
        lblState.TabIndex = 2
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyButtons.Controls.Add(btnClose, 0, 0)
        tlyButtons.Controls.Add(btnHistory, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(12, 542)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(976, 52)
        tlyButtons.TabIndex = 3
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
        ' btnHistory
        '
        btnHistory.Dock = DockStyle.Fill
        btnHistory.Location = New Point(756, 0)
        btnHistory.Margin = New Padding(6, 6, 0, 6)
        btnHistory.Name = "btnHistory"
        btnHistory.Size = New Size(214, 40)
        btnHistory.TabIndex = 1
        btnHistory.Text = "Istoricul rândului ales"
        tips.SetToolTipHeader(btnHistory, "Istoricul rândului ales")
        tips.SetToolTipText(btnHistory, "Deschide istoricul rândului ales (după cheia lui primară) prin toate copiile: ce valori a avut și când s-a schimbat sau a dispărut.")
        '
        ' CopySearchForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(1000, 600)
        Controls.Add(tlyMain)
        MinimumSize = New Size(820, 460)
        Name = "CopySearchForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Căutare în copiile locale"
        CType(gridHits, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents lblText As Label
    Friend WithEvents txtText As KBotTextField
    Friend WithEvents lblTable As Label
    Friend WithEvents txtTable As KBotTextField
    Friend WithEvents btnSearch As KBotButton
    Friend WithEvents gridHits As AdminDataView
    Friend WithEvents lblState As Label
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnClose As KBotButton
    Friend WithEvents btnHistory As KBotButton
End Class
