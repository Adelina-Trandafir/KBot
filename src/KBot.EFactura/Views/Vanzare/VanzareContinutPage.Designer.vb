Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VanzareContinutPage
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
        components = New System.ComponentModel.Container()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyContinut = New KBotTableLayoutPanel()
        tlyLinii = New KBotTableLayoutPanel()
        btnLinieNoua = New System.Windows.Forms.Button()
        gridLinii = New KBotDataView()
        CType(gridLinii, System.ComponentModel.ISupportInitialize).BeginInit()
        tlyContinut.SuspendLayout()
        tlyLinii.SuspendLayout()
        SuspendLayout()
        '
        ' tlyContinut
        '
        tlyContinut.ColumnCount = 1
        tlyContinut.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyContinut.Controls.Add(tlyLinii, 0, 0)
        tlyContinut.Controls.Add(gridLinii, 0, 1)
        tlyContinut.Dock = System.Windows.Forms.DockStyle.Fill
        tlyContinut.Location = New System.Drawing.Point(3, 3)
        tlyContinut.Margin = New System.Windows.Forms.Padding(0)
        tlyContinut.Name = "tlyContinut"
        tlyContinut.Padding = New System.Windows.Forms.Padding(8)
        tlyContinut.RowCount = 2
        tlyContinut.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        tlyContinut.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyContinut.Size = New System.Drawing.Size(743, 556)
        tlyContinut.TabIndex = 0
        '
        ' tlyLinii
        '
        tlyLinii.ColumnCount = 2
        tlyLinii.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlyLinii.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyLinii.Controls.Add(btnLinieNoua, 0, 0)
        tlyLinii.Dock = System.Windows.Forms.DockStyle.Fill
        tlyLinii.Margin = New System.Windows.Forms.Padding(0)
        tlyLinii.Name = "tlyLinii"
        tlyLinii.Size = New System.Drawing.Size(727, 42)
        tlyLinii.RowCount = 1
        tlyLinii.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyLinii.TabIndex = 0
        '
        ' btnLinieNoua
        '
        btnLinieNoua.Anchor = System.Windows.Forms.AnchorStyles.Left
        btnLinieNoua.AutoSize = True
        btnLinieNoua.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnLinieNoua.Margin = New System.Windows.Forms.Padding(0)
        btnLinieNoua.Name = "btnLinieNoua"
        btnLinieNoua.Padding = New System.Windows.Forms.Padding(14, 2, 14, 2)
        btnLinieNoua.TabIndex = 0
        btnLinieNoua.Text = "Linie nouă"
        tips.SetToolTipText(btnLinieNoua, "Adaugă o linie la sfârșitul facturii; se completează direct în tabel (unitatea de măsură implicită este XPP, bucată).")
        btnLinieNoua.UseVisualStyleBackColor = True
        '
        ' gridLinii
        '
        gridLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        gridLinii.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Nr"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "nr"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 44
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Conținut"
        KBotDataColumn7.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "continut"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.Width = 260
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ColumnType = KBotColumnType.Combo
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Um"
        KBotDataColumn8.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "um"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.Width = 120
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.DecimalPlaces = 3
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Cant"
        KBotDataColumn9.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "cant"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 80
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.DecimalPlaces = 4
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "PU"
        KBotDataColumn10.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "pu"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Width = 90
        KBotDataColumn11.Aggregate = KBotAggregate.Sum
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Valoare"
        KBotDataColumn11.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "valoare"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 100
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.ColumnType = KBotColumnType.Button
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = ""
        KBotDataColumn12.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleLeft
        KBotDataColumn12.Key = "sterge"
        KBotDataColumn12.MinWidth = 34
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.Resizable = False
        KBotDataColumn12.Width = 34
        gridLinii.Columns.Add(KBotDataColumn6)
        gridLinii.Columns.Add(KBotDataColumn7)
        gridLinii.Columns.Add(KBotDataColumn8)
        gridLinii.Columns.Add(KBotDataColumn9)
        gridLinii.Columns.Add(KBotDataColumn10)
        gridLinii.Columns.Add(KBotDataColumn11)
        gridLinii.Columns.Add(KBotDataColumn12)
        gridLinii.Dock = System.Windows.Forms.DockStyle.Fill
        gridLinii.FillColumnKey = "continut"
        gridLinii.FooterCaption = "TOTAL FACTURĂ"
        gridLinii.FooterVisible = True
        gridLinii.Location = New System.Drawing.Point(8, 50)
        gridLinii.Margin = New System.Windows.Forms.Padding(0)
        gridLinii.Name = "gridLinii"
        gridLinii.Size = New System.Drawing.Size(727, 498)
        gridLinii.TabIndex = 1
        tips.SetToolTipHeader(gridLinii, "Conținutul facturii")
        tips.SetToolTipText(gridLinii, "Cantitatea și prețul se scriu direct în tabel; valoarea liniei se calculează. «✕» șterge linia. Totul se scrie la «Salvare».")
        '
        ' VanzareContinutPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyContinut)
        Name = "VanzareContinutPage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        CType(gridLinii, System.ComponentModel.ISupportInitialize).EndInit()
        tlyContinut.ResumeLayout(False)
        tlyLinii.ResumeLayout(False)
        tlyLinii.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyContinut As KBotTableLayoutPanel
    Friend WithEvents tlyLinii As KBotTableLayoutPanel
    Friend WithEvents btnLinieNoua As System.Windows.Forms.Button
    Friend WithEvents gridLinii As KBotDataView
End Class
