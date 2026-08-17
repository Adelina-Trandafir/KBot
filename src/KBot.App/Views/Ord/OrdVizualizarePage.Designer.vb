Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdVizualizarePage
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        grid = New KBotDataView()
        tlyMain = New TableLayoutPanel()
        tblFooter = New TableLayoutPanel()
        lblContIban = New Label()
        lblContIbanCaption = New Label()
        lblCodFiscal = New Label()
        lblCodFiscalCaption = New Label()
        lblBeneficiarCaption = New Label()
        lblBeneficiar = New Label()
        lblDocJustCaption = New Label()
        lblDocJust = New Label()
        lblInfoPlataCaption = New Label()
        lblInfoPlata = New Label()
        tlyHeader = New TableLayoutPanel()
        lblSelecteazaBeneficiar = New Label()
        cboBeneficiar = New KBotComboBox()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tblFooter.SuspendLayout()
        tlyHeader.SuspendLayout()
        SuspendLayout()
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.AutoSizeHeaderHeight = False
        grid.BackColor = SystemColors.Window
        grid.BorderColor = SystemColors.ActiveBorder
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.ColumnFilterIconSize = New Size(14, 14)
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 110
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Descriere"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "descriere"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Visible = False
        KBotDataColumn2.Width = 150
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Recepții"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "total_receptii"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 80
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Plăți"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "plati_ant"
        KBotDataColumn4.MultiLine = True
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 80
        KBotDataColumn5.Aggregate = KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Valoare"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "valoare"
        KBotDataColumn5.MultiLine = True
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn6.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Rămas"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "ramas"
        KBotDataColumn6.MultiLine = True
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 80
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Dock = DockStyle.Fill
        grid.EnableGrouping = True
        grid.FillColumnKey = "clsf"
        grid.FooterBackColor = SystemColors.Control
        grid.FooterCaption = "TOTAL"
        grid.FooterColumnSeparatorColor = Color.Gainsboro
        grid.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grid.FooterForeColor = SystemColors.ActiveCaptionText
        grid.FooterHeight = 30
        grid.FooterSeparatorColor = Color.Gainsboro
        grid.FooterVisible = True
        grid.FrozenColumnCount = 1
        grid.HeaderBackColor = SystemColors.Control
        grid.HeaderColumnSeparatorColor = Color.Gainsboro
        grid.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grid.HeaderForeColor = SystemColors.ActiveCaptionText
        grid.HeaderSeparatorColor = Color.Gainsboro
        grid.Location = New Point(4, 55)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 22
        grid.ScrollByColumn = True
        grid.ShrinkColumnsToFit = False
        grid.Size = New Size(655, 288)
        grid.TabIndex = 0
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tblFooter, 0, 2)
        tlyMain.Controls.Add(grid, 0, 1)
        tlyMain.Controls.Add(tlyHeader, 0, 0)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 140F))
        tlyMain.Size = New Size(663, 488)
        tlyMain.TabIndex = 2
        ' 
        ' tblFooter
        ' 
        tblFooter.ColumnCount = 4
        tblFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tblFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tblFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tblFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblFooter.Controls.Add(lblContIban, 3, 2)
        tblFooter.Controls.Add(lblContIbanCaption, 2, 2)
        tblFooter.Controls.Add(lblCodFiscal, 1, 2)
        tblFooter.Controls.Add(lblCodFiscalCaption, 0, 2)
        tblFooter.Controls.Add(lblBeneficiarCaption, 0, 0)
        tblFooter.Controls.Add(lblBeneficiar, 1, 0)
        tblFooter.Controls.Add(lblDocJustCaption, 0, 1)
        tblFooter.Controls.Add(lblDocJust, 1, 1)
        tblFooter.Controls.Add(lblInfoPlataCaption, 0, 3)
        tblFooter.Controls.Add(lblInfoPlata, 1, 3)
        tblFooter.Dock = DockStyle.Fill
        tblFooter.Location = New Point(4, 353)
        tblFooter.Margin = New Padding(4, 5, 4, 5)
        tblFooter.Name = "tblFooter"
        tblFooter.RowCount = 4
        tblFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tblFooter.Size = New Size(655, 130)
        tblFooter.TabIndex = 5
        ' 
        ' lblContIban
        ' 
        lblContIban.Dock = DockStyle.Fill
        lblContIban.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblContIban.Location = New Point(434, 60)
        lblContIban.Margin = New Padding(4, 0, 4, 0)
        lblContIban.Name = "lblContIban"
        lblContIban.Size = New Size(217, 30)
        lblContIban.TabIndex = 17
        lblContIban.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblContIbanCaption
        ' 
        lblContIbanCaption.Dock = DockStyle.Fill
        lblContIbanCaption.Location = New Point(314, 60)
        lblContIbanCaption.Margin = New Padding(4, 0, 4, 0)
        lblContIbanCaption.Name = "lblContIbanCaption"
        lblContIbanCaption.Size = New Size(112, 30)
        lblContIbanCaption.TabIndex = 16
        lblContIbanCaption.Text = "Cont IBAN:"
        lblContIbanCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCodFiscal
        ' 
        lblCodFiscal.Dock = DockStyle.Fill
        lblCodFiscal.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblCodFiscal.Location = New Point(164, 60)
        lblCodFiscal.Margin = New Padding(4, 0, 4, 0)
        lblCodFiscal.Name = "lblCodFiscal"
        lblCodFiscal.Size = New Size(142, 30)
        lblCodFiscal.TabIndex = 15
        lblCodFiscal.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCodFiscalCaption
        ' 
        lblCodFiscalCaption.Dock = DockStyle.Fill
        lblCodFiscalCaption.Location = New Point(4, 60)
        lblCodFiscalCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodFiscalCaption.Name = "lblCodFiscalCaption"
        lblCodFiscalCaption.Size = New Size(152, 30)
        lblCodFiscalCaption.TabIndex = 14
        lblCodFiscalCaption.Text = "Cod Fiscal:"
        lblCodFiscalCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblBeneficiarCaption
        ' 
        lblBeneficiarCaption.Dock = DockStyle.Fill
        lblBeneficiarCaption.Location = New Point(4, 0)
        lblBeneficiarCaption.Margin = New Padding(4, 0, 4, 0)
        lblBeneficiarCaption.Name = "lblBeneficiarCaption"
        lblBeneficiarCaption.Size = New Size(152, 30)
        lblBeneficiarCaption.TabIndex = 0
        lblBeneficiarCaption.Text = "Beneficiar:"
        lblBeneficiarCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblBeneficiar
        ' 
        tblFooter.SetColumnSpan(lblBeneficiar, 3)
        lblBeneficiar.Dock = DockStyle.Fill
        lblBeneficiar.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblBeneficiar.Location = New Point(164, 0)
        lblBeneficiar.Margin = New Padding(4, 0, 4, 0)
        lblBeneficiar.Name = "lblBeneficiar"
        lblBeneficiar.Size = New Size(487, 30)
        lblBeneficiar.TabIndex = 1
        lblBeneficiar.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDocJustCaption
        ' 
        lblDocJustCaption.Dock = DockStyle.Fill
        lblDocJustCaption.Location = New Point(4, 30)
        lblDocJustCaption.Margin = New Padding(4, 0, 4, 0)
        lblDocJustCaption.Name = "lblDocJustCaption"
        lblDocJustCaption.Size = New Size(152, 30)
        lblDocJustCaption.TabIndex = 4
        lblDocJustCaption.Text = "Doc. Justificative:"
        lblDocJustCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDocJust
        ' 
        tblFooter.SetColumnSpan(lblDocJust, 3)
        lblDocJust.Dock = DockStyle.Fill
        lblDocJust.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblDocJust.Location = New Point(164, 30)
        lblDocJust.Margin = New Padding(4, 0, 4, 0)
        lblDocJust.Name = "lblDocJust"
        lblDocJust.Size = New Size(487, 30)
        lblDocJust.TabIndex = 5
        lblDocJust.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblInfoPlataCaption
        ' 
        lblInfoPlataCaption.Dock = DockStyle.Fill
        lblInfoPlataCaption.Location = New Point(4, 90)
        lblInfoPlataCaption.Margin = New Padding(4, 0, 4, 0)
        lblInfoPlataCaption.Name = "lblInfoPlataCaption"
        lblInfoPlataCaption.Size = New Size(152, 40)
        lblInfoPlataCaption.TabIndex = 12
        lblInfoPlataCaption.Text = "Obiect DDF"
        lblInfoPlataCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblInfoPlata
        ' 
        tblFooter.SetColumnSpan(lblInfoPlata, 3)
        lblInfoPlata.Dock = DockStyle.Fill
        lblInfoPlata.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblInfoPlata.Location = New Point(164, 90)
        lblInfoPlata.Margin = New Padding(4, 0, 4, 0)
        lblInfoPlata.Name = "lblInfoPlata"
        lblInfoPlata.Size = New Size(487, 40)
        lblInfoPlata.TabIndex = 13
        lblInfoPlata.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyHeader
        ' 
        tlyHeader.ColumnCount = 2
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 26.0935135F))
        tlyHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 73.90649F))
        tlyHeader.Controls.Add(lblSelecteazaBeneficiar, 0, 0)
        tlyHeader.Controls.Add(cboBeneficiar, 1, 0)
        tlyHeader.Dock = DockStyle.Fill
        tlyHeader.Location = New Point(0, 0)
        tlyHeader.Margin = New Padding(0)
        tlyHeader.Name = "tlyHeader"
        tlyHeader.RowCount = 1
        tlyHeader.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlyHeader.Size = New Size(663, 50)
        tlyHeader.TabIndex = 4
        ' 
        ' lblSelecteazaBeneficiar
        ' 
        lblSelecteazaBeneficiar.Dock = DockStyle.Fill
        lblSelecteazaBeneficiar.FlatStyle = FlatStyle.Flat
        lblSelecteazaBeneficiar.Location = New Point(8, 8)
        lblSelecteazaBeneficiar.Margin = New Padding(8, 8, 0, 8)
        lblSelecteazaBeneficiar.Name = "lblSelecteazaBeneficiar"
        lblSelecteazaBeneficiar.Size = New Size(165, 34)
        lblSelecteazaBeneficiar.TabIndex = 3
        lblSelecteazaBeneficiar.Text = "Caută Beneficiar"
        lblSelecteazaBeneficiar.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboBeneficiar
        ' 
        cboBeneficiar.Dock = DockStyle.Fill
        cboBeneficiar.DrawMode = DrawMode.OwnerDrawFixed
        cboBeneficiar.DropDownStyle = ComboBoxStyle.DropDownList
        cboBeneficiar.FlatStyle = FlatStyle.Flat
        cboBeneficiar.FormattingEnabled = True
        cboBeneficiar.IntegralHeight = False
        cboBeneficiar.Location = New Point(173, 8)
        cboBeneficiar.Margin = New Padding(0, 8, 8, 8)
        cboBeneficiar.Name = "cboBeneficiar"
        cboBeneficiar.Size = New Size(482, 32)
        cboBeneficiar.TabIndex = 3
        ' 
        ' OrdVizualizarePage
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(tlyMain)
        Margin = New Padding(4, 5, 4, 5)
        Name = "OrdVizualizarePage"
        Size = New Size(663, 488)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tblFooter.ResumeLayout(False)
        tlyHeader.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents tlyHeader As TableLayoutPanel
    Friend WithEvents lblSelecteazaBeneficiar As Label
    Friend WithEvents cboBeneficiar As KBotComboBox
    Friend WithEvents tblFooter As TableLayoutPanel
    Friend WithEvents lblContIban As Label
    Friend WithEvents lblContIbanCaption As Label
    Friend WithEvents lblCodFiscal As Label
    Friend WithEvents lblCodFiscalCaption As Label
    Friend WithEvents lblBeneficiarCaption As Label
    Friend WithEvents lblBeneficiar As Label
    Friend WithEvents lblDocJustCaption As Label
    Friend WithEvents lblDocJust As Label
    Friend WithEvents lblInfoPlataCaption As Label
    Friend WithEvents lblInfoPlata As Label
End Class
