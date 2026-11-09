<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SincronizareNoiForm
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
        components = New System.ComponentModel.Container()
        Dim KBotDataColumn1 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn2 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn3 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn4 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn5 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        pnlCard = New System.Windows.Forms.Panel()
        gridNoi = New Global.KBot.Controls.KBotDataView()
        pnlJos = New System.Windows.Forms.Panel()
        btnOk = New System.Windows.Forms.Button()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        CType(gridNoi, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridNoi)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(796, 446)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' gridNoi
        '
        gridNoi.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        gridNoi.ColumnFillMode = Global.KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Data"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "data"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 100
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Număr"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "numar"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 170
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Furnizor"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "furnizor"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 300
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Cod fiscal"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "cui"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 110
        KBotDataColumn5.Aggregate = Global.KBot.Controls.KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = Global.KBot.Controls.KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Total"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "total"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = Global.KBot.Controls.KBotValueType.Number
        KBotDataColumn5.Width = 110
        gridNoi.Columns.Add(KBotDataColumn1)
        gridNoi.Columns.Add(KBotDataColumn2)
        gridNoi.Columns.Add(KBotDataColumn3)
        gridNoi.Columns.Add(KBotDataColumn4)
        gridNoi.Columns.Add(KBotDataColumn5)
        gridNoi.Dock = System.Windows.Forms.DockStyle.Fill
        gridNoi.FillColumnKey = "furnizor"
        gridNoi.FooterCaption = "TOTAL"
        gridNoi.FooterVisible = True
        gridNoi.Location = New System.Drawing.Point(0, 80)
        gridNoi.Margin = New System.Windows.Forms.Padding(0)
        gridNoi.Name = "gridNoi"
        gridNoi.ReadOnlyGrid = True
        gridNoi.Size = New System.Drawing.Size(796, 308)
        gridNoi.TabIndex = 2
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnOk)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 388)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(796, 58)
        pnlJos.TabIndex = 3
        pnlJos.Tag = "Card"
        '
        ' btnOk
        '
        btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
        btnOk.Dock = System.Windows.Forms.DockStyle.Right
        btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnOk.Location = New System.Drawing.Point(628, 8)
        btnOk.Name = "btnOk"
        btnOk.Size = New System.Drawing.Size(160, 42)
        btnOk.TabIndex = 0
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 6)
        lblAntet.Size = New System.Drawing.Size(796, 40)
        lblAntet.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(796, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Sincronizează facturi primite"
        '
        ' SincronizareNoiForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnOk
        ClientSize = New System.Drawing.Size(800, 450)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(640, 360)
        Name = "SincronizareNoiForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Sincronizează facturi primite"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        CType(gridNoi, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents gridNoi As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnOk As System.Windows.Forms.Button
End Class
