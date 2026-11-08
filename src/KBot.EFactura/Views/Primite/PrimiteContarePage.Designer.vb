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
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn13 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyPagina = New KBotTableLayoutPanel()
        tlySus = New KBotTableLayoutPanel()
        lblClsf = New Global.KBot.Controls.KBotLabel()
        lblCodClient = New Global.KBot.Controls.KBotLabel()
        lblIban = New Global.KBot.Controls.KBotLabel()
        lblFelOp = New Global.KBot.Controls.KBotLabel()
        cmbClsf = New KBotComboBox()
        txtCodClient = New KBotTextBox()
        txtIban = New KBotTextBox()
        txtFelOp = New KBotTextBox()
        tlyEvid = New KBotTableLayoutPanel()
        lstEvidentiere = New KBotDataView()
        gridEvidenta = New KBotDataView()
        tlyPlata = New KBotTableLayoutPanel()
        lstPlata = New KBotDataView()
        tlyPlataDr = New KBotTableLayoutPanel()
        gridPlata = New KBotDataView()
        tlyBara = New KBotTableLayoutPanel()
        barExplicatie = New KBotProgressBar()
        lblLungime = New Global.KBot.Controls.KBotLabel()
        pnlJos = New KBotTableLayoutPanel()
        btnRenunta = New KBotButton()
        lblInfo = New KBotHtmlLabel()
        btnSalveaza = New KBotButton()
        CType(lstEvidentiere, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(gridEvidenta, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(lstPlata, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPlata, System.ComponentModel.ISupportInitialize).BeginInit()
        tlyPagina.SuspendLayout()
        tlySus.SuspendLayout()
        tlyEvid.SuspendLayout()
        tlyPlata.SuspendLayout()
        tlyPlataDr.SuspendLayout()
        tlyBara.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' tlyPagina: the headed fields on top, the evidence block, the payment block, the footer (the layout of the Access form, slice 01EF-07)
        '
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPagina.Controls.Add(tlySus, 0, 0)
        tlyPagina.Controls.Add(tlyEvid, 0, 1)
        tlyPagina.Controls.Add(tlyPlata, 0, 2)
        tlyPagina.Controls.Add(pnlJos, 0, 3)
        tlyPagina.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPagina.Location = New System.Drawing.Point(3, 3)
        tlyPagina.Margin = New System.Windows.Forms.Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.RowCount = 4
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.0!))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44.0!))
        tlyPagina.Size = New System.Drawing.Size(751, 520)
        tlyPagina.TabIndex = 0
        '
        ' tlySus: four headers, and under each the field it names
        '
        tlySus.ColumnCount = 4
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.0!))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.0!))
        tlySus.Controls.Add(lblClsf, 0, 0)
        tlySus.Controls.Add(lblCodClient, 1, 0)
        tlySus.Controls.Add(lblIban, 2, 0)
        tlySus.Controls.Add(lblFelOp, 3, 0)
        tlySus.Controls.Add(cmbClsf, 0, 1)
        tlySus.Controls.Add(txtCodClient, 1, 1)
        tlySus.Controls.Add(txtIban, 2, 1)
        tlySus.Controls.Add(txtFelOp, 3, 1)
        tlySus.Dock = System.Windows.Forms.DockStyle.Fill
        tlySus.Location = New System.Drawing.Point(0, 0)
        tlySus.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        tlySus.Name = "tlySus"
        tlySus.RowCount = 2
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlySus.Size = New System.Drawing.Size(751, 66)
        tlySus.TabIndex = 0
        '
        ' lblClsf
        '
        lblClsf.BorderWidth = 0
        lblClsf.Dock = System.Windows.Forms.DockStyle.Fill
        lblClsf.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblClsf.Margin = New System.Windows.Forms.Padding(0, 0, 2, 2)
        lblClsf.Name = "lblClsf"
        lblClsf.TabIndex = 0
        lblClsf.Text = "Clasificație bugetară"
        lblClsf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' lblCodClient
        '
        lblCodClient.BorderWidth = 0
        lblCodClient.Dock = System.Windows.Forms.DockStyle.Fill
        lblCodClient.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblCodClient.Margin = New System.Windows.Forms.Padding(0, 0, 2, 2)
        lblCodClient.Name = "lblCodClient"
        lblCodClient.TabIndex = 1
        lblCodClient.Text = "Cod Client"
        lblCodClient.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' lblIban
        '
        lblIban.BorderWidth = 0
        lblIban.Dock = System.Windows.Forms.DockStyle.Fill
        lblIban.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblIban.Margin = New System.Windows.Forms.Padding(0, 0, 2, 2)
        lblIban.Name = "lblIban"
        lblIban.TabIndex = 2
        lblIban.Text = "Cont IBAN"
        lblIban.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' lblFelOp
        '
        lblFelOp.BorderWidth = 0
        lblFelOp.Dock = System.Windows.Forms.DockStyle.Fill
        lblFelOp.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblFelOp.Margin = New System.Windows.Forms.Padding(0, 0, 0, 2)
        lblFelOp.Name = "lblFelOp"
        lblFelOp.TabIndex = 3
        lblFelOp.Text = "Fel factură (pentru O.P.-uri)"
        lblFelOp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' cmbClsf
        '
        cmbClsf.Dock = System.Windows.Forms.DockStyle.Fill
        cmbClsf.Editable = True
        cmbClsf.FindAfterNChars = 1
        cmbClsf.FindAsYouType = True
        cmbClsf.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        cmbClsf.Margin = New System.Windows.Forms.Padding(0, 0, 2, 0)
        cmbClsf.MaxDropDownItems = 14
        cmbClsf.Name = "cmbClsf"
        cmbClsf.PlaceholderText = "Alegeți clasificația bugetară"
        cmbClsf.TabIndex = 4
        tips.SetToolTipHeader(cmbClsf, "Clasificația bugetară")
        tips.SetToolTipText(cmbClsf, "Se oferă doar clasificațiile pentru care există șabloane de operații. Cele configurate pentru furnizor sunt primele.")
        '
        ' txtCodClient
        '
        txtCodClient.Dock = System.Windows.Forms.DockStyle.Fill
        txtCodClient.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        txtCodClient.Margin = New System.Windows.Forms.Padding(0, 0, 2, 0)
        txtCodClient.MaxLength = 20
        txtCodClient.Name = "txtCodClient"
        txtCodClient.TabIndex = 5
        txtCodClient.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        tips.SetToolTipHeader(txtCodClient, "Cod client")
        tips.SetToolTipText(txtCodClient, "Codul dumneavoastră de client la furnizor. Dacă este scris, intră în explicația ordinului de plată (după «C»).")
        '
        ' txtIban
        '
        txtIban.Dock = System.Windows.Forms.DockStyle.Fill
        txtIban.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        txtIban.Margin = New System.Windows.Forms.Padding(0, 0, 2, 0)
        txtIban.Name = "txtIban"
        txtIban.ReadOnly = True
        txtIban.TabIndex = 6
        txtIban.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        tips.SetToolTipHeader(txtIban, "Cont IBAN")
        tips.SetToolTipText(txtIban, "Contul bancar al furnizorului pentru clasificația aleasă (din configurarea partenerului).")
        '
        ' txtFelOp
        '
        txtFelOp.Dock = System.Windows.Forms.DockStyle.Fill
        txtFelOp.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        txtFelOp.Margin = New System.Windows.Forms.Padding(0)
        txtFelOp.MaxLength = 60
        txtFelOp.Name = "txtFelOp"
        txtFelOp.TabIndex = 7
        txtFelOp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        tips.SetToolTipHeader(txtFelOp, "Fel factură")
        tips.SetToolTipText(txtFelOp, "Se ia din șablonul de plată ales și încheie explicația ordinului de plată. Pe ordinul de plată nu intră diacritice și nici virgule.")
        '
        ' tlyEvid: the evidence templates on the left, the invoices on the right
        '
        tlyEvid.ColumnCount = 2
        tlyEvid.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230.0!))
        tlyEvid.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyEvid.Controls.Add(lstEvidentiere, 0, 0)
        tlyEvid.Controls.Add(gridEvidenta, 1, 0)
        tlyEvid.Dock = System.Windows.Forms.DockStyle.Fill
        tlyEvid.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        tlyEvid.Name = "tlyEvid"
        tlyEvid.RowCount = 1
        tlyEvid.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyEvid.TabIndex = 1
        '
        ' lstEvidentiere: the evidence templates (a click chooses one)
        '
        lstEvidentiere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstEvidentiere.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Documente evidențiere"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 200
        lstEvidentiere.Columns.Add(KBotDataColumn1)
        lstEvidentiere.Dock = System.Windows.Forms.DockStyle.Fill
        lstEvidentiere.FillColumnKey = "nume"
        lstEvidentiere.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lstEvidentiere.Margin = New System.Windows.Forms.Padding(0, 0, 4, 0)
        lstEvidentiere.Name = "lstEvidentiere"
        lstEvidentiere.ReadOnlyGrid = True
        lstEvidentiere.TabIndex = 0
        '
        ' gridEvidenta: one row per ticked invoice (Access EF_F); only the value to pay is edited
        '
        gridEvidenta.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridEvidenta.AutoSizeHeaderHeight = True
        gridEvidenta.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnType = KBotColumnType.Combo
        KBotDataColumn2.CommitOnSelect = True
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "NC"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nc"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 90
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Document"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "doc"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 150
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Fel"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "fel"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 90
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Data"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "data"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 100
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Valoare rămasă de înregistrat"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "suma"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 130
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Valoare care se plătește"
        KBotDataColumn7.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "platit"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 130
        gridEvidenta.Columns.Add(KBotDataColumn2)
        gridEvidenta.Columns.Add(KBotDataColumn3)
        gridEvidenta.Columns.Add(KBotDataColumn4)
        gridEvidenta.Columns.Add(KBotDataColumn5)
        gridEvidenta.Columns.Add(KBotDataColumn6)
        gridEvidenta.Columns.Add(KBotDataColumn7)
        gridEvidenta.Dock = System.Windows.Forms.DockStyle.Fill
        gridEvidenta.FillColumnKey = "doc"
        gridEvidenta.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        gridEvidenta.FooterVisible = True
        gridEvidenta.Margin = New System.Windows.Forms.Padding(0)
        gridEvidenta.Name = "gridEvidenta"
        gridEvidenta.TabIndex = 1
        '
        ' tlyPlata: the payment templates on the left, the order of payment on the right
        '
        tlyPlata.ColumnCount = 2
        tlyPlata.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230.0!))
        tlyPlata.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPlata.Controls.Add(lstPlata, 0, 0)
        tlyPlata.Controls.Add(tlyPlataDr, 1, 0)
        tlyPlata.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPlata.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        tlyPlata.Name = "tlyPlata"
        tlyPlata.RowCount = 1
        tlyPlata.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPlata.TabIndex = 2
        '
        ' lstPlata: the payment templates (a click chooses one, a click on the chosen one takes it off); without one there is no order of payment
        '
        lstPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Documente plată"
        KBotDataColumn8.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "nume"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 200
        lstPlata.Columns.Add(KBotDataColumn8)
        lstPlata.Dock = System.Windows.Forms.DockStyle.Fill
        lstPlata.FillColumnKey = "nume"
        lstPlata.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lstPlata.Margin = New System.Windows.Forms.Padding(0, 0, 4, 0)
        lstPlata.Name = "lstPlata"
        lstPlata.ReadOnlyGrid = True
        lstPlata.TabIndex = 0
        '
        ' tlyPlataDr: the order of payment (one row) and, under it, the length of its explanation
        '
        tlyPlataDr.ColumnCount = 1
        tlyPlataDr.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPlataDr.Controls.Add(gridPlata, 0, 0)
        tlyPlataDr.Controls.Add(tlyBara, 0, 1)
        tlyPlataDr.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPlataDr.Margin = New System.Windows.Forms.Padding(0)
        tlyPlataDr.Name = "tlyPlataDr"
        tlyPlataDr.RowCount = 2
        tlyPlataDr.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyPlataDr.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22.0!))
        tlyPlataDr.TabIndex = 1
        '
        ' gridPlata: the order of payment, one row (Access EF_P); it is awake only with a payment template
        '
        gridPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.ColumnType = KBotColumnType.Combo
        KBotDataColumn9.CommitOnSelect = True
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "NC"
        KBotDataColumn9.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "nc"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Width = 90
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Dată"
        KBotDataColumn10.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "data"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Width = 100
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Număr OP"
        KBotDataColumn11.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "nrop"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Width = 100
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Explicație O.P."
        KBotDataColumn12.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "explicatie"
        KBotDataColumn12.MultiLine = True
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.Width = 300
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.DecimalPlaces = 2
        KBotDataColumn13.Format = KBotFormat.Standard
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Valoare"
        KBotDataColumn13.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "valoare"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn13.ValueType = KBotValueType.Number
        KBotDataColumn13.Width = 130
        gridPlata.Columns.Add(KBotDataColumn9)
        gridPlata.Columns.Add(KBotDataColumn10)
        gridPlata.Columns.Add(KBotDataColumn11)
        gridPlata.Columns.Add(KBotDataColumn12)
        gridPlata.Columns.Add(KBotDataColumn13)
        gridPlata.Dock = System.Windows.Forms.DockStyle.Fill
        gridPlata.Enabled = False
        gridPlata.FillColumnKey = "explicatie"
        gridPlata.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        gridPlata.Margin = New System.Windows.Forms.Padding(0)
        gridPlata.Name = "gridPlata"
        gridPlata.TabIndex = 0
        '
        ' tlyBara: the length of the explanation (bar) and its figures
        '
        tlyBara.ColumnCount = 2
        tlyBara.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyBara.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80.0!))
        tlyBara.Controls.Add(barExplicatie, 0, 0)
        tlyBara.Controls.Add(lblLungime, 1, 0)
        tlyBara.Dock = System.Windows.Forms.DockStyle.Fill
        tlyBara.Margin = New System.Windows.Forms.Padding(0)
        tlyBara.Name = "tlyBara"
        tlyBara.RowCount = 1
        tlyBara.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyBara.TabIndex = 1
        '
        ' barExplicatie: LEN(explanation), from 1 to the official limit of 75
        '
        barExplicatie.Dock = System.Windows.Forms.DockStyle.Fill
        barExplicatie.Margin = New System.Windows.Forms.Padding(0, 6, 4, 4)
        barExplicatie.Maximum = 75
        barExplicatie.Name = "barExplicatie"
        barExplicatie.TabIndex = 0
        tips.SetToolTipHeader(barExplicatie, "Lungimea explicației")
        tips.SetToolTipText(barExplicatie, "Explicația ordinului de plată are între 1 și 75 de caractere (limita oficială). Diacriticele se înlocuiesc cu litere simple, iar virgulele se scot: ANAF refuză ordinul cu diacritice, iar codul de bare citește virgula ca «câmpul următor».")
        '
        ' lblLungime
        '
        lblLungime.BorderWidth = 0
        lblLungime.Dock = System.Windows.Forms.DockStyle.Fill
        lblLungime.Font = New System.Drawing.Font("Calibri", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblLungime.Margin = New System.Windows.Forms.Padding(0)
        lblLungime.Name = "lblLungime"
        lblLungime.TabIndex = 1
        lblLungime.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        ' pnlJos: «Renunță» on the left, the warnings in the middle, «Salvează» on the right (button-side rule)
        '
        pnlJos.ColumnCount = 3
        pnlJos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        pnlJos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        pnlJos.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        pnlJos.Controls.Add(btnRenunta, 0, 0)
        pnlJos.Controls.Add(lblInfo, 1, 0)
        pnlJos.Controls.Add(btnSalveaza, 2, 0)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Fill
        pnlJos.Margin = New System.Windows.Forms.Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.RowCount = 1
        pnlJos.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        pnlJos.TabIndex = 3
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
        ' lblInfo: the supplier and the most serious warning; the others and the bookings are in its tooltip
        '
        lblInfo.Dock = System.Windows.Forms.DockStyle.Fill
        lblInfo.Font = New System.Drawing.Font("Calibri", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblInfo.Margin = New System.Windows.Forms.Padding(8, 2, 8, 0)
        lblInfo.Name = "lblInfo"
        lblInfo.TabIndex = 1
        '
        ' btnSalveaza: books the evidence (01EF-04); the payment follows in 01EF-05
        '
        btnSalveaza.Dock = System.Windows.Forms.DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Margin = New System.Windows.Forms.Padding(0, 4, 0, 0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 2
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
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
        CType(lstEvidentiere, System.ComponentModel.ISupportInitialize).EndInit()
        CType(gridEvidenta, System.ComponentModel.ISupportInitialize).EndInit()
        CType(lstPlata, System.ComponentModel.ISupportInitialize).EndInit()
        CType(gridPlata, System.ComponentModel.ISupportInitialize).EndInit()
        tlyPagina.ResumeLayout(False)
        tlySus.ResumeLayout(False)
        tlyEvid.ResumeLayout(False)
        tlyPlata.ResumeLayout(False)
        tlyPlataDr.ResumeLayout(False)
        tlyBara.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlySus As KBotTableLayoutPanel
    Friend WithEvents lblClsf As Global.KBot.Controls.KBotLabel
    Friend WithEvents lblCodClient As Global.KBot.Controls.KBotLabel
    Friend WithEvents lblIban As Global.KBot.Controls.KBotLabel
    Friend WithEvents lblFelOp As Global.KBot.Controls.KBotLabel
    Friend WithEvents cmbClsf As KBotComboBox
    Friend WithEvents txtCodClient As KBotTextBox
    Friend WithEvents txtIban As KBotTextBox
    Friend WithEvents txtFelOp As KBotTextBox
    Friend WithEvents tlyEvid As KBotTableLayoutPanel
    Friend WithEvents lstEvidentiere As KBotDataView
    Friend WithEvents gridEvidenta As KBotDataView
    Friend WithEvents tlyPlata As KBotTableLayoutPanel
    Friend WithEvents lstPlata As KBotDataView
    Friend WithEvents tlyPlataDr As KBotTableLayoutPanel
    Friend WithEvents gridPlata As KBotDataView
    Friend WithEvents tlyBara As KBotTableLayoutPanel
    Friend WithEvents barExplicatie As KBotProgressBar
    Friend WithEvents lblLungime As Global.KBot.Controls.KBotLabel
    Friend WithEvents pnlJos As KBotTableLayoutPanel
    Friend WithEvents btnRenunta As KBotButton
    Friend WithEvents lblInfo As KBotHtmlLabel
    Friend WithEvents btnSalveaza As KBotButton
End Class
