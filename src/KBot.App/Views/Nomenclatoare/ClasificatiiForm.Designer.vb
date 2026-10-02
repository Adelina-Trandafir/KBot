Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ClasificatiiForm
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
        tips = New KBotToolTip(components)
        gridBuget = New KBotDataView()
        gridRectificari = New KBotDataView()
        btnSalveaza = New Button()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyRight = New KBotTableLayoutPanel()
        lblBuget = New Label()
        lblRectificari = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        lblStare = New Label()
        btnInchide = New Button()
        CType(gridBuget, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridRectificari, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyRight.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' gridBuget
        ' 
        gridBuget.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridBuget.BackColor = SystemColors.Window
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.Format = KBotFormat.ShortDate
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Început"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "inceput"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.ValueType = KBotValueType.DateTime
        KBotDataColumn15.Width = 110
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.DecimalPlaces = 2
        KBotDataColumn1.Format = KBotFormat.Standard
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Trim. 1"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "trim1"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn1.ValueType = KBotValueType.Number
        KBotDataColumn1.Width = 110
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.DecimalPlaces = 2
        KBotDataColumn2.Format = KBotFormat.Standard
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Trim. 2"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "trim2"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.ValueType = KBotValueType.Number
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Trim. 3"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "trim3"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 110
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Trim. 4"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "trim4"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 110
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Button
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = ""
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "sterge"
        KBotDataColumn5.MinWidth = 34
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 34
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Id"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn16.Key = "id"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.Visible = KBotColumnVisibility.Hidden
        gridBuget.Columns.Add(KBotDataColumn15)
        gridBuget.Columns.Add(KBotDataColumn1)
        gridBuget.Columns.Add(KBotDataColumn2)
        gridBuget.Columns.Add(KBotDataColumn3)
        gridBuget.Columns.Add(KBotDataColumn4)
        gridBuget.Columns.Add(KBotDataColumn5)
        gridBuget.Columns.Add(KBotDataColumn16)
        gridBuget.Dock = DockStyle.Fill
        gridBuget.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridBuget.FooterBackColor = SystemColors.Control
        gridBuget.FooterCaption = "Versiuni de buget"
        gridBuget.FooterRightIcon = My.Resources.Resources.plus_green
        gridBuget.FooterRightIconTooltip = "Adaugă o versiune de buget" & vbLf & "Rândul nou se completează direct în tabel."
        gridBuget.FooterSeparatorColor = SystemColors.ActiveBorder
        gridBuget.FooterVisible = True
        gridBuget.HeaderBackColor = SystemColors.Control
        gridBuget.HeaderHeight = 24
        gridBuget.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridBuget.Location = New Point(0, 45)
        gridBuget.Margin = New Padding(0)
        gridBuget.Name = "gridBuget"
        gridBuget.RowHeight = 24
        gridBuget.Size = New Size(955, 200)
        gridBuget.TabIndex = 1
        tips.SetToolTipHeader(gridBuget, "Bugetul clasificației, pe versiuni")
        tips.SetToolTipText(gridBuget, "Fiecare rând este bugetul de la data din «Început» încolo (trimestrele 1 - 4); documentul de fundamentare citește bugetul de la data revizuirii." & vbLf & "«+» din subsol adaugă o versiune; «✕» o șterge. Totul se scrie la «Salvează».")
        ' 
        ' gridRectificari
        ' 
        gridRectificari.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridRectificari.BackColor = SystemColors.Window
        gridRectificari.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Nr. doc."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "document"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Width = 110
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.Format = KBotFormat.ShortDate
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Data"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "data"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.ValueType = KBotValueType.DateTime
        KBotDataColumn7.Width = 96
        KBotDataColumn8.Aggregate = KBotAggregate.Sum
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Trim. 1"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "trim1"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn8.Width = 96
        KBotDataColumn9.Aggregate = KBotAggregate.Sum
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Trim. 2"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "trim2"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 96
        KBotDataColumn10.Aggregate = KBotAggregate.Sum
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Trim. 3"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "trim3"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Width = 96
        KBotDataColumn11.Aggregate = KBotAggregate.Sum
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Trim. 4"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "trim4"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 96
        KBotDataColumn12.Aggregate = KBotAggregate.Sum
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Total"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "total"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBotValueType.Number
        KBotDataColumn12.Width = 110
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.ColumnType = KBotColumnType.Button
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = ""
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn13.Key = "sterge"
        KBotDataColumn13.MinWidth = 34
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.Resizable = False
        KBotDataColumn13.Width = 34
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Id"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn14.Key = "id"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.Visible = KBotColumnVisibility.Hidden
        gridRectificari.Columns.Add(KBotDataColumn6)
        gridRectificari.Columns.Add(KBotDataColumn7)
        gridRectificari.Columns.Add(KBotDataColumn8)
        gridRectificari.Columns.Add(KBotDataColumn9)
        gridRectificari.Columns.Add(KBotDataColumn10)
        gridRectificari.Columns.Add(KBotDataColumn11)
        gridRectificari.Columns.Add(KBotDataColumn12)
        gridRectificari.Columns.Add(KBotDataColumn13)
        gridRectificari.Columns.Add(KBotDataColumn14)
        gridRectificari.Dock = DockStyle.Fill
        gridRectificari.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridRectificari.FooterBackColor = SystemColors.Control
        gridRectificari.FooterCaption = "Total"
        gridRectificari.FooterRightIcon = My.Resources.Resources.plus_green
        gridRectificari.FooterRightIconTooltip = "Adaugă o rectificare" & vbLf & "Rândul nou se completează direct în tabel."
        gridRectificari.FooterSeparatorColor = SystemColors.ActiveBorder
        gridRectificari.FooterVisible = True
        gridRectificari.HeaderHeight = 24
        gridRectificari.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridRectificari.Location = New Point(0, 192)
        gridRectificari.Margin = New Padding(0)
        gridRectificari.Name = "gridRectificari"
        gridRectificari.Size = New Size(955, 422)
        gridRectificari.TabIndex = 3
        tips.SetToolTipHeader(gridRectificari, "Rectificările bugetare ale anului")
        tips.SetToolTipText(gridRectificari, "Nr. doc. și data sunt obligatorii; data trebuie să fie în anul de lucru." & vbLf & "«+» din subsol adaugă un rând; «✕» îl șterge. Totul se scrie la «Salvează».")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AutoSize = True
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1186, 6)
        btnSalveaza.Margin = New Padding(0, 0, 12, 0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(26, 4, 26, 4)
        btnSalveaza.Size = New Size(198, 60)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie în baza de date versiunile de buget și rectificările clasificației alese.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(2, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.Size = New Size(1599, 776)
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
        capBar.Size = New Size(1599, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Clasificații bugetare"
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 66)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 9, 12, 9)
        pnlCard.Size = New Size(1599, 632)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 39.42857F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60.57143F))
        tlyBody.Controls.Add(tree, 0, 0)
        tlyBody.Controls.Add(tlyRight, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1575, 614)
        tlyBody.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BackColor = SystemColors.Window
        tree.BorderColor = SystemColors.ActiveBorder
        tree.Dock = DockStyle.Fill
        tree.FooterCaption = "Adaugă clasificații"
        tree.FooterHeight = 30
        tree.FooterIconSize = New Size(18, 18)
        tree.FooterRightIcon = My.Resources.Resources.plus_green
        tree.FooterRightIconTooltip = "Adaugă clasificații" & vbLf & "Alegeți sursa, clasificațiile funcționale și pe cele economice," & vbLf & "ca la înregistrarea unității."
        tree.FooterTextAlign = ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " CLASIFICAȚII"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Indent = 14
        tree.LeftIconSize = New Size(16, 16)
        tree.LeftTextWidth = 80
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0, 0, 12, 0)
        tree.Name = "tree"
        tree.RightTextColumn = 150
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… tastați o parte din cod sau din denumire …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.Size = New Size(608, 614)
        tree.TabIndex = 0
        ' 
        ' tlyRight
        ' 
        tlyRight.ColumnCount = 1
        tlyRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRight.Controls.Add(lblBuget, 0, 0)
        tlyRight.Controls.Add(gridBuget, 0, 1)
        tlyRight.Controls.Add(lblRectificari, 0, 2)
        tlyRight.Controls.Add(gridRectificari, 0, 3)
        tlyRight.Dock = DockStyle.Fill
        tlyRight.Location = New Point(620, 0)
        tlyRight.Margin = New Padding(0)
        tlyRight.Name = "tlyRight"
        tlyRight.RowCount = 4
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 45F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 40F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 60F))
        tlyRight.Size = New Size(955, 614)
        tlyRight.TabIndex = 1
        ' 
        ' lblBuget
        ' 
        lblBuget.AutoEllipsis = True
        lblBuget.Dock = DockStyle.Fill
        lblBuget.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblBuget.Location = New Point(0, 0)
        lblBuget.Margin = New Padding(0)
        lblBuget.Name = "lblBuget"
        lblBuget.Size = New Size(955, 45)
        lblBuget.TabIndex = 0
        lblBuget.Text = "Buget"
        lblBuget.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblRectificari
        ' 
        lblRectificari.AutoEllipsis = True
        lblRectificari.Dock = DockStyle.Fill
        lblRectificari.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblRectificari.Location = New Point(0, 147)
        lblRectificari.Margin = New Padding(0, 12, 0, 0)
        lblRectificari.Name = "lblRectificari"
        lblRectificari.Size = New Size(955, 45)
        lblRectificari.TabIndex = 2
        lblRectificari.Text = "Rectificări bugetare"
        lblRectificari.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' tlySubsol
        ' 
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(lblStare, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 1, 0)
        tlySubsol.Controls.Add(btnInchide, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 698)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1599, 78)
        tlySubsol.TabIndex = 2
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(8, 6)
        lblStare.Margin = New Padding(0, 0, 12, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1166, 66)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnInchide
        ' 
        btnInchide.AutoSize = True
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(1396, 6)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(26, 4, 26, 4)
        btnInchide.Size = New Size(195, 60)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' ClasificatiiForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1603, 780)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1350, 780)
        Name = "ClasificatiiForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Clasificații bugetare"
        CType(gridBuget, ComponentModel.ISupportInitialize).EndInit()
        CType(gridRectificari, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyRight.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyRight As KBotTableLayoutPanel
    Friend WithEvents lblBuget As Label
    Friend WithEvents gridBuget As KBotDataView
    Friend WithEvents lblRectificari As Label
    Friend WithEvents gridRectificari As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnInchide As Button
End Class
