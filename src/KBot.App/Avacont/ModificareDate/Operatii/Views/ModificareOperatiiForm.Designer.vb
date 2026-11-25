Imports KBot.Controls

' «Modificare operatii» (slice AV02-05, the Access form ModificareNC1280): the list of the posted operations (the info panel under it was removed in DGV0-02).
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ModificareOperatiiForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        Dim colTip As KBotDataColumn = New KBotDataColumn()
        Dim colNc As KBotDataColumn = New KBotDataColumn()
        Dim colDoc As KBotDataColumn = New KBotDataColumn()
        Dim colData As KBotDataColumn = New KBotDataColumn()
        Dim colClsf As KBotDataColumn = New KBotDataColumn()
        Dim colFel As KBotDataColumn = New KBotDataColumn()
        Dim colJurnal As KBotDataColumn = New KBotDataColumn()
        Dim colExplicatie As KBotDataColumn = New KBotDataColumn()
        Dim colDebit As KBotDataColumn = New KBotDataColumn()
        Dim colCredit As KBotDataColumn = New KBotDataColumn()
        Dim colSuma As KBotDataColumn = New KBotDataColumn()
        Dim colChelt As KBotDataColumn = New KBotDataColumn()
        Dim colCfpp As KBotDataColumn = New KBotDataColumn()
        Dim colOrd As KBotDataColumn = New KBotDataColumn()
        Dim colRec As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        ntfMesaj = New KBotNotice()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        gridOperatii = New KBotDataView()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(ntfMesaj, 0, 1)
        tlyMain.Controls.Add(pnlCard, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(2, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1916, 1076)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.cells
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1916, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Modificare operații"
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Fill
        ntfMesaj.Location = New Point(12, 75)
        ntfMesaj.Margin = New Padding(12, 9, 12, 0)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(1892, 60)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 135)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 9, 12, 9)
        pnlCard.Size = New Size(1916, 941)
        pnlCard.TabIndex = 2
        pnlCard.Tag = "Card"
        '
        ' tlyBody
        '
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(gridOperatii, 0, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1892, 923)
        tlyBody.TabIndex = 0
        '
        ' gridOperatii
        '
        colTip.AggregateFormatString = Nothing
        colTip.AllowGrouping = False
        colTip.ColumnFilterIcon = My.Resources.Resources.filter
        colTip.FormatString = Nothing
        colTip.HeaderText = "Tip"
        colTip.HeaderTextAlign = ContentAlignment.MiddleCenter
        colTip.Key = "tip"
        colTip.OptionGroup = Nothing
        colTip.ReadOnly = True
        colTip.ShowColumnFilter = True
        colTip.TextAlign = ContentAlignment.MiddleCenter
        colTip.Width = 60
        colNc.AggregateFormatString = Nothing
        colNc.ColumnFilterIcon = My.Resources.Resources.filter
        colNc.FormatString = Nothing
        colNc.HeaderText = "NC"
        colNc.HeaderTextAlign = ContentAlignment.MiddleCenter
        colNc.Key = "nc"
        colNc.OptionGroup = Nothing
        colNc.ReadOnly = True
        colNc.ShowColumnFilter = True
        colNc.TextAlign = ContentAlignment.MiddleRight
        colNc.ValueType = KBotValueType.Number
        colNc.Width = 80
        colDoc.AggregateFormatString = Nothing
        colDoc.AllowGrouping = False
        colDoc.ColumnFilterIcon = My.Resources.Resources.filter
        colDoc.FormatString = Nothing
        colDoc.HeaderText = "Doc."
        colDoc.HeaderTextAlign = ContentAlignment.MiddleCenter
        colDoc.Key = "doc"
        colDoc.OptionGroup = Nothing
        colDoc.ReadOnly = True
        colDoc.ShowColumnFilter = True
        colDoc.TextAlign = ContentAlignment.MiddleLeft
        colDoc.Width = 140
        colData.AggregateFormatString = Nothing
        colData.ColumnFilterIcon = My.Resources.Resources.filter
        colData.Format = KBotFormat.ShortDate
        colData.FormatString = Nothing
        colData.HeaderText = "Data"
        colData.HeaderTextAlign = ContentAlignment.MiddleCenter
        colData.Key = "data"
        colData.OptionGroup = Nothing
        colData.ReadOnly = True
        colData.ShowColumnFilter = True
        colData.TextAlign = ContentAlignment.MiddleCenter
        colData.ValueType = KBotValueType.DateTime
        colData.Width = 120
        colClsf.AggregateFormatString = Nothing
        colClsf.ColumnFilterIcon = My.Resources.Resources.filter
        colClsf.FormatString = Nothing
        colClsf.HeaderText = "Clasificație"
        colClsf.HeaderTextAlign = ContentAlignment.MiddleCenter
        colClsf.Key = "clsf"
        colClsf.OptionGroup = Nothing
        colClsf.ReadOnly = True
        colClsf.ShowColumnFilter = True
        colClsf.TextAlign = ContentAlignment.MiddleCenter
        colClsf.Width = 150
        colFel.AggregateFormatString = Nothing
        colFel.ColumnFilterIcon = My.Resources.Resources.filter
        colFel.FormatString = Nothing
        colFel.HeaderText = "Fel"
        colFel.HeaderTextAlign = ContentAlignment.MiddleCenter
        colFel.Key = "fel"
        colFel.OptionGroup = Nothing
        colFel.ReadOnly = True
        colFel.ShowColumnFilter = True
        colFel.TextAlign = ContentAlignment.MiddleLeft
        colFel.Width = 120
        colJurnal.AggregateFormatString = Nothing
        colJurnal.ColumnFilterIcon = My.Resources.Resources.filter
        colJurnal.FormatString = Nothing
        colJurnal.HeaderText = "Jurn."
        colJurnal.HeaderTextAlign = ContentAlignment.MiddleCenter
        colJurnal.Key = "jurnal"
        colJurnal.OptionGroup = Nothing
        colJurnal.ReadOnly = True
        colJurnal.ShowColumnFilter = True
        colJurnal.TextAlign = ContentAlignment.MiddleCenter
        colJurnal.Width = 80
        colExplicatie.AggregateFormatString = Nothing
        colExplicatie.AllowGrouping = False
        colExplicatie.ColumnFilterIcon = My.Resources.Resources.filter
        colExplicatie.FormatString = Nothing
        colExplicatie.HeaderText = "Explicație"
        colExplicatie.HeaderTextAlign = ContentAlignment.MiddleCenter
        colExplicatie.Key = "explicatie"
        colExplicatie.OptionGroup = Nothing
        colExplicatie.ReadOnly = True
        colExplicatie.ShowColumnFilter = True
        colExplicatie.TextAlign = ContentAlignment.MiddleLeft
        colExplicatie.Width = 300
        colDebit.AggregateFormatString = Nothing
        colDebit.AllowGrouping = False
        colDebit.ColumnFilterIcon = My.Resources.Resources.filter
        colDebit.FormatString = Nothing
        colDebit.HeaderText = "Cont debit"
        colDebit.HeaderTextAlign = ContentAlignment.MiddleCenter
        colDebit.Key = "debit"
        colDebit.OptionGroup = Nothing
        colDebit.ReadOnly = True
        colDebit.ShowColumnFilter = True
        colDebit.TextAlign = ContentAlignment.MiddleLeft
        colDebit.Width = 210
        colCredit.AggregateFormatString = Nothing
        colCredit.AllowGrouping = False
        colCredit.ColumnFilterIcon = My.Resources.Resources.filter
        colCredit.FormatString = Nothing
        colCredit.HeaderText = "Cont credit"
        colCredit.HeaderTextAlign = ContentAlignment.MiddleCenter
        colCredit.Key = "credit"
        colCredit.OptionGroup = Nothing
        colCredit.ReadOnly = True
        colCredit.ShowColumnFilter = True
        colCredit.TextAlign = ContentAlignment.MiddleLeft
        colCredit.Width = 210
        colSuma.Aggregate = KBotAggregate.Sum
        colSuma.AggregateFormatString = Nothing
        colSuma.AllowGrouping = False
        colSuma.ColumnFilterIcon = My.Resources.Resources.filter
        colSuma.DecimalPlaces = 2
        colSuma.Format = KBotFormat.Standard
        colSuma.FormatString = Nothing
        colSuma.HeaderText = "Sumă"
        colSuma.HeaderTextAlign = ContentAlignment.MiddleCenter
        colSuma.Key = "suma"
        colSuma.OptionGroup = Nothing
        colSuma.ReadOnly = True
        colSuma.ShowColumnFilter = True
        colSuma.TextAlign = ContentAlignment.MiddleRight
        colSuma.ValueType = KBotValueType.Number
        colSuma.Width = 150
        colChelt.AggregateFormatString = Nothing
        colChelt.ColumnType = KBotColumnType.CheckBox
        colChelt.FormatString = Nothing
        colChelt.HeaderText = "Ch"
        colChelt.HeaderTextAlign = ContentAlignment.MiddleCenter
        colChelt.Key = "chelt"
        colChelt.MinWidth = 28
        colChelt.OptionGroup = Nothing
        colChelt.ReadOnly = True
        colChelt.TextAlign = ContentAlignment.MiddleCenter
        colChelt.Width = 28
        colCfpp.AggregateFormatString = Nothing
        colCfpp.ColumnType = KBotColumnType.CheckBox
        colCfpp.FormatString = Nothing
        colCfpp.HeaderText = "Cf"
        colCfpp.HeaderTextAlign = ContentAlignment.MiddleCenter
        colCfpp.Key = "cfpp"
        colCfpp.MinWidth = 28
        colCfpp.OptionGroup = Nothing
        colCfpp.ReadOnly = True
        colCfpp.TextAlign = ContentAlignment.MiddleCenter
        colCfpp.Width = 28
        colOrd.AggregateFormatString = Nothing
        colOrd.FormatString = Nothing
        colOrd.HeaderText = "O"
        colOrd.HeaderTextAlign = ContentAlignment.MiddleCenter
        colOrd.Key = "ord"
        colOrd.MinWidth = 28
        colOrd.OptionGroup = Nothing
        colOrd.ReadOnly = True
        colOrd.TextAlign = ContentAlignment.MiddleCenter
        colOrd.Width = 28
        colRec.AggregateFormatString = Nothing
        colRec.ColumnType = KBotColumnType.CheckBox
        colRec.FormatString = Nothing
        colRec.HeaderText = "R"
        colRec.HeaderTextAlign = ContentAlignment.MiddleCenter
        colRec.Key = "rec"
        colRec.NullShowsMixedCheck = True
        colRec.MinWidth = 28
        colRec.OptionGroup = Nothing
        colRec.ReadOnly = True
        colRec.TextAlign = ContentAlignment.MiddleCenter
        colRec.Width = 28
        gridOperatii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridOperatii.BackColor = SystemColors.Window
        gridOperatii.ColumnFillMode = KBotFillMode.SpecificColumn
        gridOperatii.Columns.Add(colTip)
        gridOperatii.Columns.Add(colNc)
        gridOperatii.Columns.Add(colDoc)
        gridOperatii.Columns.Add(colData)
        gridOperatii.Columns.Add(colClsf)
        gridOperatii.Columns.Add(colFel)
        gridOperatii.Columns.Add(colJurnal)
        gridOperatii.Columns.Add(colExplicatie)
        gridOperatii.Columns.Add(colDebit)
        gridOperatii.Columns.Add(colCredit)
        gridOperatii.Columns.Add(colSuma)
        gridOperatii.Columns.Add(colChelt)
        gridOperatii.Columns.Add(colCfpp)
        gridOperatii.Columns.Add(colOrd)
        gridOperatii.Columns.Add(colRec)
        gridOperatii.Dock = DockStyle.Fill
        gridOperatii.EnableGrouping = True
        gridOperatii.FillColumnKey = "explicatie"
        gridOperatii.FilterIconVisibility = KBotFilterIconVisibility.OnHoverOrFiltered
        gridOperatii.Font = New Font("Calibri", 10F)
        gridOperatii.FooterVisible = True
        gridOperatii.Location = New Point(0, 0)
        gridOperatii.Margin = New Padding(0)
        gridOperatii.Name = "gridOperatii"
        gridOperatii.Size = New Size(1892, 653)
        gridOperatii.TabIndex = 0
        '
        ' ModificareOperatiiForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1920, 1080)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1440, 840)
        Name = "ModificareOperatiiForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Modificare operații"
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents ntfMesaj As KBotNotice
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents gridOperatii As KBotDataView
End Class
