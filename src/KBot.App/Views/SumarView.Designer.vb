<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SumarView
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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        pnlHeader = New Panel()
        tblHeader = New TableLayoutPanel()
        lblCodCaption = New Label()
        lblCod = New Label()
        lblDataFxCaption = New Label()
        lblDataFx = New Label()
        lblDataCreareCaption = New Label()
        lblDataCreare = New Label()
        lblDataDefCaption = New Label()
        lblDataDef = New Label()
        lblStareCaption = New Label()
        lblStare = New Label()
        lblStatusCaption = New Label()
        lblStatus = New Label()
        lblDescriereCaption = New Label()
        lblDescriere = New Label()
        grid = New Controls.KBotDataView()
        lblEmpty = New Label()
        pnlHeader.SuspendLayout()
        tblHeader.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlHeader
        ' 
        pnlHeader.BackColor = SystemColors.Window
        pnlHeader.Controls.Add(tblHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Margin = New Padding(3, 4, 3, 4)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Padding = New Padding(14)
        pnlHeader.Size = New Size(914, 166)
        pnlHeader.TabIndex = 0
        ' 
        ' tblHeader
        ' 
        tblHeader.ColumnCount = 4
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 149F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 149F))
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tblHeader.Controls.Add(lblCodCaption, 0, 0)
        tblHeader.Controls.Add(lblCod, 1, 0)
        tblHeader.Controls.Add(lblDataFxCaption, 2, 0)
        tblHeader.Controls.Add(lblDataFx, 3, 0)
        tblHeader.Controls.Add(lblDataCreareCaption, 0, 1)
        tblHeader.Controls.Add(lblDataCreare, 1, 1)
        tblHeader.Controls.Add(lblDataDefCaption, 2, 1)
        tblHeader.Controls.Add(lblDataDef, 3, 1)
        tblHeader.Controls.Add(lblStareCaption, 0, 2)
        tblHeader.Controls.Add(lblStare, 1, 2)
        tblHeader.Controls.Add(lblStatusCaption, 2, 2)
        tblHeader.Controls.Add(lblStatus, 3, 2)
        tblHeader.Controls.Add(lblDescriereCaption, 0, 3)
        tblHeader.Controls.Add(lblDescriere, 1, 3)
        tblHeader.Dock = DockStyle.Fill
        tblHeader.Location = New Point(14, 14)
        tblHeader.Margin = New Padding(3, 4, 3, 4)
        tblHeader.Name = "tblHeader"
        tblHeader.RowCount = 4
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tblHeader.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tblHeader.Size = New Size(886, 138)
        tblHeader.TabIndex = 0
        ' 
        ' lblCodCaption
        ' 
        lblCodCaption.Dock = DockStyle.Fill
        lblCodCaption.Location = New Point(3, 0)
        lblCodCaption.Name = "lblCodCaption"
        lblCodCaption.Size = New Size(143, 34)
        lblCodCaption.TabIndex = 0
        lblCodCaption.Text = "Cod angajament:"
        lblCodCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCod
        ' 
        lblCod.Dock = DockStyle.Fill
        lblCod.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        lblCod.Location = New Point(152, 0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(288, 34)
        lblCod.TabIndex = 1
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataFxCaption
        ' 
        lblDataFxCaption.Dock = DockStyle.Fill
        lblDataFxCaption.Location = New Point(446, 0)
        lblDataFxCaption.Name = "lblDataFxCaption"
        lblDataFxCaption.Size = New Size(143, 34)
        lblDataFxCaption.TabIndex = 2
        lblDataFxCaption.Text = "Data FX:"
        lblDataFxCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataFx
        ' 
        lblDataFx.Dock = DockStyle.Fill
        lblDataFx.Location = New Point(595, 0)
        lblDataFx.Name = "lblDataFx"
        lblDataFx.Size = New Size(288, 34)
        lblDataFx.TabIndex = 3
        lblDataFx.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataCreareCaption
        ' 
        lblDataCreareCaption.Dock = DockStyle.Fill
        lblDataCreareCaption.Location = New Point(3, 34)
        lblDataCreareCaption.Name = "lblDataCreareCaption"
        lblDataCreareCaption.Size = New Size(143, 34)
        lblDataCreareCaption.TabIndex = 4
        lblDataCreareCaption.Text = "Data creare:"
        lblDataCreareCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataCreare
        ' 
        lblDataCreare.Dock = DockStyle.Fill
        lblDataCreare.Location = New Point(152, 34)
        lblDataCreare.Name = "lblDataCreare"
        lblDataCreare.Size = New Size(288, 34)
        lblDataCreare.TabIndex = 5
        lblDataCreare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataDefCaption
        ' 
        lblDataDefCaption.Dock = DockStyle.Fill
        lblDataDefCaption.Location = New Point(446, 34)
        lblDataDefCaption.Name = "lblDataDefCaption"
        lblDataDefCaption.Size = New Size(143, 34)
        lblDataDefCaption.TabIndex = 6
        lblDataDefCaption.Text = "Data definitivare:"
        lblDataDefCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataDef
        ' 
        lblDataDef.Dock = DockStyle.Fill
        lblDataDef.Location = New Point(595, 34)
        lblDataDef.Name = "lblDataDef"
        lblDataDef.Size = New Size(288, 34)
        lblDataDef.TabIndex = 7
        lblDataDef.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStareCaption
        ' 
        lblStareCaption.Dock = DockStyle.Fill
        lblStareCaption.Location = New Point(3, 68)
        lblStareCaption.Name = "lblStareCaption"
        lblStareCaption.Size = New Size(143, 34)
        lblStareCaption.TabIndex = 8
        lblStareCaption.Text = "Stare:"
        lblStareCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStare
        ' 
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(152, 68)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(288, 34)
        lblStare.TabIndex = 9
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStatusCaption
        ' 
        lblStatusCaption.Dock = DockStyle.Fill
        lblStatusCaption.Location = New Point(446, 68)
        lblStatusCaption.Name = "lblStatusCaption"
        lblStatusCaption.Size = New Size(143, 34)
        lblStatusCaption.TabIndex = 10
        lblStatusCaption.Text = "Încărcat / Preluat:"
        lblStatusCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStatus
        ' 
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(595, 68)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(288, 34)
        lblStatus.TabIndex = 11
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDescriereCaption
        ' 
        lblDescriereCaption.Dock = DockStyle.Fill
        lblDescriereCaption.Location = New Point(3, 102)
        lblDescriereCaption.Name = "lblDescriereCaption"
        lblDescriereCaption.Size = New Size(143, 36)
        lblDescriereCaption.TabIndex = 12
        lblDescriereCaption.Text = "Descriere:"
        lblDescriereCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDescriere
        ' 
        tblHeader.SetColumnSpan(lblDescriere, 3)
        lblDescriere.Dock = DockStyle.Fill
        lblDescriere.Location = New Point(152, 102)
        lblDescriere.Name = "lblDescriere"
        lblDescriere.Size = New Size(731, 36)
        lblDescriere.TabIndex = 13
        lblDescriere.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' grid
        ' 
        grid.AutoSizeHeaderHeight = False
        grid.BackColor = SystemColors.Window
        grid.BorderColor = SystemColors.ActiveBorder
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.ColumnFilterIconSize = New Size(14, 14)
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.Frozen = True
        KBotDataColumn1.HeaderLeftIconSize = New Size(14, 14)
        KBotDataColumn1.HeaderText = "Clasificația"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 140
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 250
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Indicator"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "cod_indicator"
        KBotDataColumn2.MaxWidth = 1313131
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 80
        KBotDataColumn3.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Rezervări"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "total_rezervari"
        KBotDataColumn3.MaxWidth = 13000
        KBotDataColumn3.MinWidth = 80
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn3.Width = 110
        KBotDataColumn4.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Recepții"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "total_receptii"
        KBotDataColumn4.MaxWidth = 130000
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn4.Width = 110
        KBotDataColumn5.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Plăți"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "total_plati"
        KBotDataColumn5.MaxWidth = 130000
        KBotDataColumn5.MinWidth = 80
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn5.Width = 110
        KBotDataColumn6.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Revizii"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "total_revizii"
        KBotDataColumn6.MaxWidth = 13000
        KBotDataColumn6.MinWidth = 80
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn6.Width = 110
        KBotDataColumn7.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Ordonanțări"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "total_ordonantari"
        KBotDataColumn7.MaxWidth = 130000
        KBotDataColumn7.MinWidth = 80
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn7.Width = 110
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Columns.Add(KBotDataColumn7)
        grid.Dock = DockStyle.Fill
        grid.FooterBackColor = SystemColors.Control
        grid.FooterCaption = "TOTALURI"
        grid.FooterColumnSeparatorColor = Color.Gainsboro
        grid.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grid.FooterForeColor = SystemColors.ActiveCaptionText
        grid.FooterHeight = 30
        grid.FooterLeftIcon = My.Resources.Resources.settings__1_
        grid.FooterLeftIconHoverColor = SystemColors.Highlight
        grid.FooterSeparatorColor = Color.Gainsboro
        grid.FooterVisible = True
        grid.FrozenColumnCount = 1
        grid.HeaderBackColor = SystemColors.Control
        grid.HeaderColumnSeparatorColor = Color.Gainsboro
        grid.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grid.HeaderForeColor = SystemColors.ActiveCaptionText
        grid.HeaderSeparatorColor = Color.Gainsboro
        grid.Location = New Point(0, 166)
        grid.Margin = New Padding(3, 4, 3, 4)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 20
        grid.Size = New Size(914, 500)
        grid.TabIndex = 1
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 166)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(914, 500)
        lblEmpty.TabIndex = 2
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' SumarView
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(grid)
        Controls.Add(lblEmpty)
        Controls.Add(pnlHeader)
        Margin = New Padding(3, 4, 3, 4)
        Name = "SumarView"
        Size = New Size(914, 666)
        pnlHeader.ResumeLayout(False)
        tblHeader.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents tblHeader As TableLayoutPanel
    Friend WithEvents lblCodCaption As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents lblDataFxCaption As Label
    Friend WithEvents lblDataFx As Label
    Friend WithEvents lblDataCreareCaption As Label
    Friend WithEvents lblDataCreare As Label
    Friend WithEvents lblDataDefCaption As Label
    Friend WithEvents lblDataDef As Label
    Friend WithEvents lblStareCaption As Label
    Friend WithEvents lblStare As Label
    Friend WithEvents lblStatusCaption As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblDescriereCaption As Label
    Friend WithEvents lblDescriere As Label
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents lblEmpty As Label
End Class
