Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FacturiCumparareContarePage
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
        tips = New KBotToolTip(components)
        cmbClsf = New KBotComboBox()
        txtCodClient = New KBotTextBox()
        txtIban = New KBotTextBox()
        txtFelOp = New KBotTextBox()
        tlyPagina = New KBotTableLayoutPanel()
        tlySus = New KBotTableLayoutPanel()
        lblClsf = New KBotLabel()
        lblCodClient = New KBotLabel()
        lblIban = New KBotLabel()
        lblFelOp = New KBotLabel()
        tlyEvid = New KBotTableLayoutPanel()
        lstEvidentiere = New KBotDataView()
        gridEvidenta = New KBotDataView()
        tlyPlata = New KBotTableLayoutPanel()
        lstPlata = New KBotDataView()
        gridPlata = New KBotDataView()
        tlyPagina.SuspendLayout()
        tlySus.SuspendLayout()
        tlyEvid.SuspendLayout()
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridEvidenta, ComponentModel.ISupportInitialize).BeginInit()
        tlyPlata.SuspendLayout()
        CType(lstPlata, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPlata, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' cmbClsf
        ' 
        cmbClsf.Dock = System.Windows.Forms.DockStyle.Fill
        cmbClsf.Editable = True
        cmbClsf.FindAsYouType = True
        cmbClsf.FindInItemOrder = True
        cmbClsf.FitDropDownToWorkingArea = True
        cmbClsf.Font = New System.Drawing.Font("Calibri", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        cmbClsf.Location = New System.Drawing.Point(0, 53)
        cmbClsf.Margin = New System.Windows.Forms.Padding(0, 4, 6, 4)
        cmbClsf.MaxDropDownItems = 15
        cmbClsf.Name = "cmbClsf"
        cmbClsf.PlaceholderText = "Alegeți clasificația bugetară"
        cmbClsf.Size = New System.Drawing.Size(339, 40)
        cmbClsf.TabIndex = 4
        tips.SetToolTipHeader(cmbClsf, "Clasificația bugetară")
        tips.SetToolTipText(cmbClsf, "Deschideți un articol și alegeți o clasificație de sub el. Sunt disponibile clasificațiile cu șabloane de operații. Cele configurate pentru furnizor apar primele.")
        ' 
        ' txtCodClient
        ' 
        txtCodClient.Dock = System.Windows.Forms.DockStyle.Fill
        txtCodClient.Font = New System.Drawing.Font("Calibri", 11F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        txtCodClient.HorizontalScrollBarPadding = New System.Windows.Forms.Padding(0)
        txtCodClient.Location = New System.Drawing.Point(354, 53)
        txtCodClient.Margin = New System.Windows.Forms.Padding(4)
        txtCodClient.MaxLength = 20
        txtCodClient.Multiline = False
        txtCodClient.Name = "txtCodClient"
        txtCodClient.ScrollBars = System.Windows.Forms.ScrollBars.None
        txtCodClient.Size = New System.Drawing.Size(242, 42)
        txtCodClient.TabIndex = 5
        txtCodClient.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        txtCodClient.TextPadding = New System.Windows.Forms.Padding(0)
        tips.SetToolTipHeader(txtCodClient, "Cod client")
        tips.SetToolTipText(txtCodClient, "Codul dumneavoastră de client la furnizor. Dacă este scris, intră în explicația ordinului de plată (după «C»).")
        txtCodClient.VerticalScrollBarPadding = New System.Windows.Forms.Padding(0)
        ' 
        ' txtIban
        ' 
        txtIban.Dock = System.Windows.Forms.DockStyle.Fill
        txtIban.Font = New System.Drawing.Font("Calibri", 11F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        txtIban.HorizontalScrollBarPadding = New System.Windows.Forms.Padding(0)
        txtIban.Location = New System.Drawing.Point(604, 53)
        txtIban.Margin = New System.Windows.Forms.Padding(4)
        txtIban.Multiline = False
        txtIban.Name = "txtIban"
        txtIban.ScrollBars = System.Windows.Forms.ScrollBars.None
        txtIban.Size = New System.Drawing.Size(342, 42)
        txtIban.TabIndex = 6
        txtIban.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        txtIban.TextPadding = New System.Windows.Forms.Padding(0)
        tips.SetToolTipHeader(txtIban, "Cont IBAN")
        tips.SetToolTipText(txtIban, "Contul bancar al furnizorului pentru clasificația aleasă (din configurarea partenerului).")
        txtIban.VerticalScrollBarPadding = New System.Windows.Forms.Padding(0)
        ' 
        ' txtFelOp
        ' 
        txtFelOp.Dock = System.Windows.Forms.DockStyle.Fill
        txtFelOp.Font = New System.Drawing.Font("Calibri", 11F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        txtFelOp.HorizontalScrollBarPadding = New System.Windows.Forms.Padding(0)
        txtFelOp.Location = New System.Drawing.Point(954, 53)
        txtFelOp.Margin = New System.Windows.Forms.Padding(4)
        txtFelOp.MaxLength = 60
        txtFelOp.Multiline = False
        txtFelOp.Name = "txtFelOp"
        txtFelOp.ScrollBars = System.Windows.Forms.ScrollBars.None
        txtFelOp.Size = New System.Drawing.Size(292, 42)
        txtFelOp.TabIndex = 7
        txtFelOp.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        txtFelOp.TextPadding = New System.Windows.Forms.Padding(0)
        tips.SetToolTipHeader(txtFelOp, "Fel factură")
        tips.SetToolTipText(txtFelOp, "Se ia din șablonul de plată ales și încheie explicația ordinului de plată. Pe ordinul de plată nu intră diacritice și nici virgule.")
        txtFelOp.VerticalScrollBarPadding = New System.Windows.Forms.Padding(0)
        ' 
        ' tlyPagina
        ' 
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyPagina.Controls.Add(tlySus, 0, 0)
        tlyPagina.Controls.Add(tlyEvid, 0, 1)
        tlyPagina.Controls.Add(tlyPlata, 0, 2)
        tlyPagina.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPagina.Location = New System.Drawing.Point(4, 4)
        tlyPagina.Margin = New System.Windows.Forms.Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.RowCount = 3
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 105F))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F))
        tlyPagina.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F))
        tlyPagina.Size = New System.Drawing.Size(1434, 785)
        tlyPagina.TabIndex = 0
        ' 
        ' tlySus
        ' 
        tlySus.ColumnCount = 5
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 345F))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350F))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F))
        tlySus.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlySus.Controls.Add(lblClsf, 0, 1)
        tlySus.Controls.Add(lblCodClient, 1, 1)
        tlySus.Controls.Add(lblIban, 2, 1)
        tlySus.Controls.Add(lblFelOp, 3, 1)
        tlySus.Controls.Add(cmbClsf, 0, 2)
        tlySus.Controls.Add(txtCodClient, 1, 2)
        tlySus.Controls.Add(txtIban, 2, 2)
        tlySus.Controls.Add(txtFelOp, 3, 2)
        tlySus.Dock = System.Windows.Forms.DockStyle.Fill
        tlySus.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize
        tlySus.Location = New System.Drawing.Point(0, 0)
        tlySus.Margin = New System.Windows.Forms.Padding(0, 0, 0, 6)
        tlySus.Name = "tlySus"
        tlySus.RowCount = 3
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F))
        tlySus.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F))
        tlySus.Size = New System.Drawing.Size(1434, 99)
        tlySus.TabIndex = 0
        ' 
        ' lblClsf
        ' 
        lblClsf.BorderWidth = 0
        lblClsf.Dock = System.Windows.Forms.DockStyle.Fill
        lblClsf.Font = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Bold)
        lblClsf.Location = New System.Drawing.Point(0, 17)
        lblClsf.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        lblClsf.Name = "lblClsf"
        lblClsf.Size = New System.Drawing.Size(347, 29)
        lblClsf.TabIndex = 0
        lblClsf.Text = "Clasificație bugetară"
        lblClsf.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' lblCodClient
        ' 
        lblCodClient.BorderWidth = 0
        lblCodClient.Dock = System.Windows.Forms.DockStyle.Fill
        lblCodClient.Font = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Bold)
        lblCodClient.Location = New System.Drawing.Point(350, 17)
        lblCodClient.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        lblCodClient.Name = "lblCodClient"
        lblCodClient.Size = New System.Drawing.Size(247, 29)
        lblCodClient.TabIndex = 1
        lblCodClient.Text = "Cod Client"
        lblCodClient.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' lblIban
        ' 
        lblIban.BorderWidth = 0
        lblIban.Dock = System.Windows.Forms.DockStyle.Fill
        lblIban.Font = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Bold)
        lblIban.Location = New System.Drawing.Point(600, 17)
        lblIban.Margin = New System.Windows.Forms.Padding(0, 0, 3, 3)
        lblIban.Name = "lblIban"
        lblIban.Size = New System.Drawing.Size(347, 29)
        lblIban.TabIndex = 2
        lblIban.Text = "Cont IBAN"
        lblIban.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' lblFelOp
        ' 
        lblFelOp.BorderWidth = 0
        lblFelOp.Dock = System.Windows.Forms.DockStyle.Fill
        lblFelOp.Font = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Bold)
        lblFelOp.Location = New System.Drawing.Point(950, 17)
        lblFelOp.Margin = New System.Windows.Forms.Padding(0, 0, 0, 3)
        lblFelOp.Name = "lblFelOp"
        lblFelOp.Size = New System.Drawing.Size(300, 29)
        lblFelOp.TabIndex = 3
        lblFelOp.Text = "Fel factură (pentru O.P.-uri)"
        lblFelOp.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' tlyEvid
        ' 
        tlyEvid.ColumnCount = 2
        tlyEvid.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 345F))
        tlyEvid.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyEvid.Controls.Add(lstEvidentiere, 0, 0)
        tlyEvid.Controls.Add(gridEvidenta, 1, 0)
        tlyEvid.Dock = System.Windows.Forms.DockStyle.Fill
        tlyEvid.Location = New System.Drawing.Point(0, 105)
        tlyEvid.Margin = New System.Windows.Forms.Padding(0, 0, 0, 6)
        tlyEvid.Name = "tlyEvid"
        tlyEvid.RowCount = 1
        tlyEvid.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyEvid.Size = New System.Drawing.Size(1434, 350)
        tlyEvid.TabIndex = 1
        ' 
        ' lstEvidentiere
        ' 
        lstEvidentiere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstEvidentiere.BackColor = Drawing.SystemColors.Window
        lstEvidentiere.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AllowGrouping = False
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Documente evidențiere"
        KBotDataColumn1.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 200
        lstEvidentiere.Columns.Add(KBotDataColumn1)
        lstEvidentiere.Dock = System.Windows.Forms.DockStyle.Fill
        lstEvidentiere.Font = New System.Drawing.Font("Calibri", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lstEvidentiere.Location = New System.Drawing.Point(0, 0)
        lstEvidentiere.Margin = New System.Windows.Forms.Padding(0, 0, 6, 0)
        lstEvidentiere.Name = "lstEvidentiere"
        lstEvidentiere.ReadOnlyGrid = True
        lstEvidentiere.Size = New System.Drawing.Size(339, 350)
        lstEvidentiere.TabIndex = 0
        ' 
        ' gridEvidenta
        ' 
        gridEvidenta.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridEvidenta.BackColor = Drawing.SystemColors.Window
        gridEvidenta.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnType = KBotColumnType.Combo
        KBotDataColumn2.CommitOnSelect = True
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "NC"
        KBotDataColumn2.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nc"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 90
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Document"
        KBotDataColumn3.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "doc"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 150
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Fel"
        KBotDataColumn4.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "fel"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 90
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Data"
        KBotDataColumn5.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "data"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Rămasă"
        KBotDataColumn6.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.HeaderTooltip = "Valoare rămasă de înregistrat"
        KBotDataColumn6.Key = "suma"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 130
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "De Plată"
        KBotDataColumn7.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.HeaderTooltip = "Valoare care se plătește"
        KBotDataColumn7.Key = "platit"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = Drawing.ContentAlignment.MiddleRight
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
        gridEvidenta.Font = New System.Drawing.Font("Calibri", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        gridEvidenta.FooterVisible = True
        gridEvidenta.Location = New System.Drawing.Point(345, 0)
        gridEvidenta.Margin = New System.Windows.Forms.Padding(0)
        gridEvidenta.Name = "gridEvidenta"
        gridEvidenta.Size = New System.Drawing.Size(1089, 350)
        gridEvidenta.TabIndex = 1
        ' 
        ' tlyPlata
        ' 
        tlyPlata.ColumnCount = 2
        tlyPlata.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 345F))
        tlyPlata.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyPlata.Controls.Add(lstPlata, 0, 0)
        tlyPlata.Controls.Add(gridPlata, 1, 0)
        tlyPlata.Dock = System.Windows.Forms.DockStyle.Fill
        tlyPlata.Location = New System.Drawing.Point(0, 461)
        tlyPlata.Margin = New System.Windows.Forms.Padding(0)
        tlyPlata.Name = "tlyPlata"
        tlyPlata.RowCount = 1
        tlyPlata.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyPlata.Size = New System.Drawing.Size(1434, 251)
        tlyPlata.TabIndex = 2
        ' 
        ' lstPlata
        ' 
        lstPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstPlata.BackColor = Drawing.SystemColors.Window
        lstPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Documente plată"
        KBotDataColumn8.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "nume"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 200
        lstPlata.Columns.Add(KBotDataColumn8)
        lstPlata.Dock = System.Windows.Forms.DockStyle.Fill
        lstPlata.FillColumnKey = "nume"
        lstPlata.Font = New System.Drawing.Font("Calibri", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lstPlata.Location = New System.Drawing.Point(0, 0)
        lstPlata.Margin = New System.Windows.Forms.Padding(0, 0, 6, 0)
        lstPlata.Name = "lstPlata"
        lstPlata.ReadOnlyGrid = True
        lstPlata.Size = New System.Drawing.Size(339, 251)
        lstPlata.TabIndex = 0
        ' 
        ' gridPlata
        ' 
        gridPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPlata.BackColor = Drawing.SystemColors.Window
        gridPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.ColumnType = KBotColumnType.Combo
        KBotDataColumn9.CommitOnSelect = True
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "NC"
        KBotDataColumn9.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "nc"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Width = 90
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Dată"
        KBotDataColumn10.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "data"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Număr OP"
        KBotDataColumn11.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "nrop"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Explicație O.P."
        KBotDataColumn12.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "explicatie"
        KBotDataColumn12.MultiLine = True
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.Width = 300
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.ColumnType = KBotColumnType.ProgressBar
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Lungime"
        KBotDataColumn14.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn14.HeaderTooltip = "Explicația ordinului de plată are între 1 și 75 de caractere (limita oficială). Diacriticele se înlocuiesc cu litere simple, iar virgulele se scot: ANAF refuză ordinul cu diacritice, iar codul de bare citește virgula ca «câmpul următor»."
        KBotDataColumn14.Key = "lungime"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ProgressMax = 75R
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.Width = 90
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.DecimalPlaces = 2
        KBotDataColumn13.Format = KBotFormat.Standard
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Valoare"
        KBotDataColumn13.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "valoare"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn13.ValueType = KBotValueType.Number
        KBotDataColumn13.Width = 130
        gridPlata.Columns.Add(KBotDataColumn9)
        gridPlata.Columns.Add(KBotDataColumn10)
        gridPlata.Columns.Add(KBotDataColumn11)
        gridPlata.Columns.Add(KBotDataColumn12)
        gridPlata.Columns.Add(KBotDataColumn14)
        gridPlata.Columns.Add(KBotDataColumn13)
        gridPlata.Dock = System.Windows.Forms.DockStyle.Fill
        gridPlata.Enabled = False
        gridPlata.FillColumnKey = "explicatie"
        gridPlata.Font = New System.Drawing.Font("Calibri", 10F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        gridPlata.Location = New System.Drawing.Point(345, 0)
        gridPlata.Margin = New System.Windows.Forms.Padding(0)
        gridPlata.Name = "gridPlata"
        gridPlata.Size = New System.Drawing.Size(1089, 251)
        gridPlata.TabIndex = 0
        ' 
        ' FacturiCumparareContarePage
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Margin = New System.Windows.Forms.Padding(4)
        Name = "FacturiCumparareContarePage"
        Padding = New System.Windows.Forms.Padding(4, 4, 4, 0)
        Size = New System.Drawing.Size(1442, 789)
        tlyPagina.ResumeLayout(False)
        tlySus.ResumeLayout(False)
        tlyEvid.ResumeLayout(False)
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).EndInit()
        CType(gridEvidenta, ComponentModel.ISupportInitialize).EndInit()
        tlyPlata.ResumeLayout(False)
        CType(lstPlata, ComponentModel.ISupportInitialize).EndInit()
        CType(gridPlata, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents gridPlata As KBotDataView
End Class
