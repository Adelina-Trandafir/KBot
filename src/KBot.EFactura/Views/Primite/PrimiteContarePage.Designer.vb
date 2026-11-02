Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteContarePage
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
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyPagina = New KBotTableLayoutPanel()
        tlySus = New KBotTableLayoutPanel()
        lblClsf = New System.Windows.Forms.Label()
        cmbClsf = New KBotComboBox()
        lblEvidentiere = New System.Windows.Forms.Label()
        cmbSablonE = New KBotComboBox()
        lblPlata = New System.Windows.Forms.Label()
        cmbSablonP = New KBotComboBox()
        lblNota = New System.Windows.Forms.Label()
        cmbNota = New KBotComboBox()
        lblInfo = New KBotHtmlLabel()
        gridEvidenta = New KBotDataView()
        gridPlata = New KBotDataView()
        lblOperatii = New KBotHtmlLabel()
        pnlJos = New KBotTableLayoutPanel()
        btnRenunta = New KBotButton()
        btnSalveaza = New KBotButton()
        CType(gridEvidenta, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPlata, System.ComponentModel.ISupportInitialize).BeginInit()
        tlyPagina.SuspendLayout()
        tlySus.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' tlyPagina: the choices on top, the supplier's data, the evidence grid, the payment grid, what would be booked, the footer
        '
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPagina.Controls.Add(tlySus, 0, 0)
        tlyPagina.Controls.Add(lblInfo, 0, 1)
        tlyPagina.Controls.Add(gridEvidenta, 0, 2)
        tlyPagina.Controls.Add(gridPlata, 0, 3)
        tlyPagina.Controls.Add(lblOperatii, 0, 4)
        tlyPagina.Controls.Add(pnlJos, 0, 5)
        tlyPagina.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPagina.Location = New System.Drawing.Point(3, 3)
        tlyPagina.Margin = New System.Windows.Forms.Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.RowCount = 6
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 114.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0!))
        tlyPagina.Size = New System.Drawing.Size(751, 520)
        tlyPagina.TabIndex = 0
        '
        ' tlySus: classification (whole row), then the evidence and the payment template
        '
        tlySus.ColumnCount = 4
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        tlySus.Controls.Add(lblClsf, 0, 0)
        tlySus.Controls.Add(cmbClsf, 1, 0)
        tlySus.Controls.Add(lblEvidentiere, 0, 1)
        tlySus.Controls.Add(cmbSablonE, 1, 1)
        tlySus.Controls.Add(lblPlata, 2, 1)
        tlySus.Controls.Add(cmbSablonP, 3, 1)
        tlySus.Controls.Add(lblNota, 0, 2)
        tlySus.Controls.Add(cmbNota, 1, 2)
        tlySus.SetColumnSpan(cmbClsf, 3)
        tlySus.SetColumnSpan(cmbNota, 3)
        tlySus.Dock = System.Windows.Forms.DockStyle.Fill
        tlySus.Location = New System.Drawing.Point(0, 0)
        tlySus.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        tlySus.Name = "tlySus"
        tlySus.RowCount = 3
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33!))
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33!))
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34!))
        tlySus.Size = New System.Drawing.Size(751, 110)
        tlySus.TabIndex = 0
        '
        ' lblClsf
        '
        lblClsf.Dock = System.Windows.Forms.DockStyle.Fill
        lblClsf.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblClsf.Margin = New System.Windows.Forms.Padding(0)
        lblClsf.Name = "lblClsf"
        lblClsf.TabIndex = 0
        lblClsf.Text = "Clasificație"
        lblClsf.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbClsf
        '
        cmbClsf.Dock = System.Windows.Forms.DockStyle.Fill
        cmbClsf.Editable = True
        cmbClsf.FindAfterNChars = 1
        cmbClsf.FindAsYouType = True
        cmbClsf.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cmbClsf.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        cmbClsf.MaxDropDownItems = 14
        cmbClsf.Name = "cmbClsf"
        cmbClsf.PlaceholderText = "Alegeți clasificația bugetară"
        cmbClsf.TabIndex = 1
        tips.SetToolTipHeader(cmbClsf, "Clasificația bugetară")
        tips.SetToolTipText(cmbClsf, "Se oferă doar clasificațiile pentru care există șabloane de operații. Cele configurate pentru furnizor sunt primele.")
        '
        ' lblEvidentiere
        '
        lblEvidentiere.Dock = System.Windows.Forms.DockStyle.Fill
        lblEvidentiere.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblEvidentiere.Margin = New System.Windows.Forms.Padding(0)
        lblEvidentiere.Name = "lblEvidentiere"
        lblEvidentiere.TabIndex = 2
        lblEvidentiere.Text = "Evidențiere"
        lblEvidentiere.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbSablonE
        '
        cmbSablonE.Dock = System.Windows.Forms.DockStyle.Fill
        cmbSablonE.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cmbSablonE.Margin = New System.Windows.Forms.Padding(0, 2, 6, 2)
        cmbSablonE.MaxDropDownItems = 10
        cmbSablonE.Name = "cmbSablonE"
        cmbSablonE.PlaceholderText = "Șablon de evidențiere"
        cmbSablonE.TabIndex = 3
        tips.SetToolTipHeader(cmbSablonE, "Șablon de evidențiere")
        tips.SetToolTipText(cmbSablonE, "Operația contabilă de înregistrare a facturii (șabloane de tip «cu clasificație», fel «evidențiere»).")
        '
        ' lblPlata
        '
        lblPlata.Dock = System.Windows.Forms.DockStyle.Fill
        lblPlata.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblPlata.Margin = New System.Windows.Forms.Padding(0)
        lblPlata.Name = "lblPlata"
        lblPlata.TabIndex = 4
        lblPlata.Text = "Plată"
        lblPlata.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbSablonP
        '
        cmbSablonP.Dock = System.Windows.Forms.DockStyle.Fill
        cmbSablonP.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cmbSablonP.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        cmbSablonP.MaxDropDownItems = 10
        cmbSablonP.Name = "cmbSablonP"
        cmbSablonP.PlaceholderText = "Șablon de plată"
        cmbSablonP.TabIndex = 5
        tips.SetToolTipHeader(cmbSablonP, "Șablon de plată")
        tips.SetToolTipText(cmbSablonP, "Operația contabilă de plată (șabloane de tip «cu clasificație», fel «plată»). Fără șablon de plată nu se întocmește ordin de plată.")
        '
        ' lblNota
        '
        lblNota.Dock = System.Windows.Forms.DockStyle.Fill
        lblNota.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblNota.Margin = New System.Windows.Forms.Padding(0)
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 6
        lblNota.Text = "Nota contabilă"
        lblNota.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' cmbNota: an existing note of the subunit and year, or a NEW number typed (the notes are chosen like this everywhere)
        '
        cmbNota.Dock = System.Windows.Forms.DockStyle.Fill
        cmbNota.Editable = True
        cmbNota.FindAsYouType = True
        cmbNota.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cmbNota.LimitToList = False
        cmbNota.Margin = New System.Windows.Forms.Padding(0, 2, 0, 2)
        cmbNota.MaxDropDownItems = 10
        cmbNota.Name = "cmbNota"
        cmbNota.PlaceholderText = "Nouă"
        cmbNota.TabIndex = 7
        tips.SetToolTipHeader(cmbNota, "Nota contabilă")
        tips.SetToolTipText(cmbNota, "Alegeți o notă existentă (documentele noi se adaugă la ea) sau tastați un număr nou (se face o notă cu acel număr). Lăsată goală: se face o notă nouă, cu numărul următor.")
        '
        ' lblInfo: the supplier, its account and codes, the warnings
        '
        lblInfo.Dock = System.Windows.Forms.DockStyle.Fill
        lblInfo.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblInfo.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        lblInfo.Name = "lblInfo"
        lblInfo.TabIndex = 1
        '
        ' gridEvidenta: one row per ticked invoice (Access EF_F); only «De plătit» is edited
        '
        gridEvidenta.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridEvidenta.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Număr"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "doc"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Dată"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "data"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 100
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Fel"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "fel"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 100
        KBotDataColumn4.Aggregate = KBotAggregate.Sum
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Total factură"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "total"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 120
        KBotDataColumn5.Aggregate = KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Deja înregistrat"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "salvat"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 120
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Rămas de înregistrat"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "suma"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 150
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "De plătit"
        KBotDataColumn7.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "platit"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 120
        gridEvidenta.Columns.Add(KBotDataColumn1)
        gridEvidenta.Columns.Add(KBotDataColumn2)
        gridEvidenta.Columns.Add(KBotDataColumn3)
        gridEvidenta.Columns.Add(KBotDataColumn4)
        gridEvidenta.Columns.Add(KBotDataColumn5)
        gridEvidenta.Columns.Add(KBotDataColumn6)
        gridEvidenta.Columns.Add(KBotDataColumn7)
        gridEvidenta.Dock = System.Windows.Forms.DockStyle.Fill
        gridEvidenta.FillColumnKey = "doc"
        gridEvidenta.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        gridEvidenta.FooterCaption = "TOTAL"
        gridEvidenta.FooterVisible = True
        gridEvidenta.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        gridEvidenta.Name = "gridEvidenta"
        gridEvidenta.TabIndex = 2
        '
        ' gridPlata: the payment, one row (Access EF_P); read only until the booking is saved (01EF-05)
        '
        gridPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Explicație ordin de plată"
        KBotDataColumn8.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "explicatie"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 400
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Valoare"
        KBotDataColumn9.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "valoare"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBotValueType.Number
        KBotDataColumn9.Width = 130
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Data cea mai nouă a facturilor"
        KBotDataColumn10.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "dataMin"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.Width = 200
        gridPlata.Columns.Add(KBotDataColumn8)
        gridPlata.Columns.Add(KBotDataColumn9)
        gridPlata.Columns.Add(KBotDataColumn10)
        gridPlata.Dock = System.Windows.Forms.DockStyle.Fill
        gridPlata.FillColumnKey = "explicatie"
        gridPlata.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        gridPlata.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        gridPlata.Name = "gridPlata"
        gridPlata.ReadOnlyGrid = True
        gridPlata.TabIndex = 3
        '
        ' lblOperatii: the bookings the chosen templates would produce
        '
        lblOperatii.Dock = System.Windows.Forms.DockStyle.Fill
        lblOperatii.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblOperatii.Margin = New System.Windows.Forms.Padding(0)
        lblOperatii.Name = "lblOperatii"
        lblOperatii.TabIndex = 4
        '
        ' pnlJos
        '
        pnlJos.ColumnCount = 3
        pnlJos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        pnlJos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        pnlJos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190.0!))
        pnlJos.Controls.Add(btnRenunta, 0, 0)
        pnlJos.Controls.Add(btnSalveaza, 2, 0)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Fill
        pnlJos.Margin = New System.Windows.Forms.Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.RowCount = 1
        pnlJos.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        pnlJos.TabIndex = 5
        '
        ' btnRenunta
        '
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Fill
        btnRenunta.Margin = New System.Windows.Forms.Padding(0, 4, 0, 0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Scoate toate bifele din arbore și golește pagina.")
        '
        ' btnSalveaza: books the evidence (01EF-04); the payment follows in 01EF-05
        '
        btnSalveaza.Dock = System.Windows.Forms.DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Margin = New System.Windows.Forms.Padding(0, 4, 0, 0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează evidențierea"
        tips.SetToolTipHeader(btnSalveaza, "Salvează evidențierea")
        tips.SetToolTipText(btnSalveaza, "Înregistrează facturile bifate cu șablonul de evidențiere ales: o notă contabilă, un document și o operație pentru fiecare factură. Plata se înregistrează separat (încă nu este disponibilă).")
        '
        ' PrimiteContarePage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Name = "PrimiteContarePage"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 0)
        Size = New System.Drawing.Size(757, 526)
        CType(gridEvidenta, System.ComponentModel.ISupportInitialize).EndInit()
        CType(gridPlata, System.ComponentModel.ISupportInitialize).EndInit()
        tlyPagina.ResumeLayout(False)
        tlySus.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlySus As KBotTableLayoutPanel
    Friend WithEvents lblClsf As System.Windows.Forms.Label
    Friend WithEvents cmbClsf As KBotComboBox
    Friend WithEvents lblEvidentiere As System.Windows.Forms.Label
    Friend WithEvents cmbSablonE As KBotComboBox
    Friend WithEvents lblPlata As System.Windows.Forms.Label
    Friend WithEvents cmbSablonP As KBotComboBox
    Friend WithEvents lblNota As System.Windows.Forms.Label
    Friend WithEvents cmbNota As KBotComboBox
    Friend WithEvents lblInfo As KBotHtmlLabel
    Friend WithEvents gridEvidenta As KBotDataView
    Friend WithEvents gridPlata As KBotDataView
    Friend WithEvents lblOperatii As KBotHtmlLabel
    Friend WithEvents pnlJos As KBotTableLayoutPanel
    Friend WithEvents btnRenunta As KBotButton
    Friend WithEvents btnSalveaza As KBotButton
End Class
