Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteLiniiPage
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        gridLinii = New KBotDataView()
        tlyPagina = New KBotTableLayoutPanel()
        tlySus = New KBotTableLayoutPanel()
        rtbNote = New KBotRichTextEditor()
        lblFurnizor = New KBotHtmlLabel()
        CType(gridLinii, System.ComponentModel.ISupportInitialize).BeginInit()
        tlyPagina.SuspendLayout()
        tlySus.SuspendLayout()
        SuspendLayout()
        '
        ' gridLinii
        '
        gridLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Nr"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nr"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 44
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Denumire"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "denumire"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 348
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "CPV"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cpv"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 132
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Um"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "um"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 56
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 3
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Cant"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "cant"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 4
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Preț"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "pret"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 90
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Valoare"
        KBotDataColumn7.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "valoare"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 100
        KBotDataColumn8.Aggregate = KBotAggregate.Sum
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Valoare cu TVA"
        KBotDataColumn8.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "cuTva"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBotValueType.Number
        KBotDataColumn8.Width = 110
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Clasificație"
        KBotDataColumn9.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "clsf"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.Width = 150
        gridLinii.Columns.Add(KBotDataColumn1)
        gridLinii.Columns.Add(KBotDataColumn2)
        gridLinii.Columns.Add(KBotDataColumn3)
        gridLinii.Columns.Add(KBotDataColumn4)
        gridLinii.Columns.Add(KBotDataColumn5)
        gridLinii.Columns.Add(KBotDataColumn6)
        gridLinii.Columns.Add(KBotDataColumn7)
        gridLinii.Columns.Add(KBotDataColumn8)
        gridLinii.Columns.Add(KBotDataColumn9)
        gridLinii.Dock = System.Windows.Forms.DockStyle.Fill
        gridLinii.FillColumnKey = "denumire"
        gridLinii.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        gridLinii.FooterCaption = "TOTAL LINII"
        gridLinii.FooterVisible = True
        gridLinii.Location = New System.Drawing.Point(0, 208)
        gridLinii.Margin = New System.Windows.Forms.Padding(0)
        gridLinii.Name = "gridLinii"
        gridLinii.ReadOnlyGrid = True
        gridLinii.Size = New System.Drawing.Size(751, 312)
        gridLinii.TabIndex = 0
        '
        ' tlyPagina: the supplier and the notes on top (40%), the lines under them (60%)
        '
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPagina.Controls.Add(tlySus, 0, 0)
        tlyPagina.Controls.Add(gridLinii, 0, 1)
        tlyPagina.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPagina.Location = New System.Drawing.Point(3, 3)
        tlyPagina.Margin = New System.Windows.Forms.Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.RowCount = 2
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60.0!))
        tlyPagina.Size = New System.Drawing.Size(751, 520)
        tlyPagina.TabIndex = 0
        '
        ' tlySus: the supplier on the left, a small gap, the notes on the right
        '
        tlySus.ColumnCount = 3
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.0!))
        tlySus.Controls.Add(lblFurnizor, 0, 0)
        tlySus.Controls.Add(rtbNote, 2, 0)
        tlySus.Dock = System.Windows.Forms.DockStyle.Fill
        tlySus.Location = New System.Drawing.Point(0, 0)
        tlySus.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        tlySus.Name = "tlySus"
        tlySus.RowCount = 1
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlySus.Size = New System.Drawing.Size(751, 204)
        tlySus.TabIndex = 0
        '
        ' lblFurnizor
        '
        lblFurnizor.Dock = System.Windows.Forms.DockStyle.Fill
        lblFurnizor.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblFurnizor.Location = New System.Drawing.Point(0, 0)
        lblFurnizor.Margin = New System.Windows.Forms.Padding(0)
        lblFurnizor.Name = "lblFurnizor"
        lblFurnizor.Size = New System.Drawing.Size(311, 204)
        lblFurnizor.TabIndex = 0
        '
        ' rtbNote
        '
        rtbNote.Dock = System.Windows.Forms.DockStyle.Fill
        rtbNote.EditorFont = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        rtbNote.FooterVisible = False
        rtbNote.HeaderVisible = False
        rtbNote.Location = New System.Drawing.Point(317, 0)
        rtbNote.Margin = New System.Windows.Forms.Padding(0)
        rtbNote.Name = "rtbNote"
        rtbNote.Size = New System.Drawing.Size(434, 204)
        rtbNote.TabIndex = 1
        '
        ' PrimiteLiniiPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Name = "PrimiteLiniiPage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        CType(gridLinii, System.ComponentModel.ISupportInitialize).EndInit()
        tlyPagina.ResumeLayout(False)
        tlySus.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents gridLinii As KBotDataView
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlySus As KBotTableLayoutPanel
    Friend WithEvents lblFurnizor As KBotHtmlLabel
    Friend WithEvents rtbNote As KBotRichTextEditor
End Class
