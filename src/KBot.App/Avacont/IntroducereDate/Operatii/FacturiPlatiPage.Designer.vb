Imports KBot.Controls

' «Facturi si plati» page (slice AV02-04; the Access form OPuri2017): the partner, the classification and the note on top; the templates on the left;
' the typed invoices, the unpaid invoices of the current and the previous year and the orders of payment of the partner on the right.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FacturiPlatiPage
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
        Dim KBotDataColumn15 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn16 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn17 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn18 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn19 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn20 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn21 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn22 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn23 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn24 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn25 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyPagina = New KBotTableLayoutPanel()
        tlyAntet = New KBotTableLayoutPanel()
        lblPartener = New KBotLabel()
        lblClsf = New KBotLabel()
        lblNota = New KBotLabel()
        lblDataOp = New KBotLabel()
        lblIban = New KBotLabel()
        cmbPartener = New KBotComboBox()
        cmbClsf = New KBotComboBox()
        cmbNota = New KBotComboBox()
        dtpDataOp = New KBotDatePicker()
        txtIban = New KBotTextBox()
        tlyMijloc = New KBotTableLayoutPanel()
        tlySabloane = New KBotTableLayoutPanel()
        lblEvid = New KBotLabel()
        lstEvidentiere = New KBotDataView()
        lblSabP = New KBotLabel()
        lstPlata = New KBotDataView()
        tlyGrile = New KBotTableLayoutPanel()
        lblNoi = New KBotLabel()
        gridNoi = New KBotDataView()
        lblCurent = New KBotLabel()
        gridCurent = New KBotDataView()
        lblAnterior = New KBotLabel()
        gridAnterior = New KBotDataView()
        lblPlati = New KBotLabel()
        gridPlati = New KBotDataView()
        tlyJos = New KBotTableLayoutPanel()
        lblExplicatie = New KBotLabel()
        txtExplicatie = New KBotTextBox()
        lblNumarOp = New KBotLabel()
        lblStare = New KBotLabel()
        btnAdauga = New KBotButton()
        btnStergeRand = New KBotButton()
        btnStergePlata = New KBotButton()
        btnRenunta = New KBotButton()
        btnSalveaza = New KBotButton()
        tlyPagina.SuspendLayout()
        tlyAntet.SuspendLayout()
        tlyMijloc.SuspendLayout()
        tlySabloane.SuspendLayout()
        tlyGrile.SuspendLayout()
        tlyJos.SuspendLayout()
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).BeginInit()
        CType(lstPlata, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridNoi, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridCurent, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridAnterior, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPlati, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' tlyPagina
        ' 
        tlyPagina.ColumnCount = 1
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Margin = New Padding(0)
        tlyPagina.RowCount = 3
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 104F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 116F))
        tlyPagina.Size = New Size(100, 100)
        tlyPagina.Controls.Add(tlyAntet, 0, 0)
        tlyPagina.Controls.Add(tlyMijloc, 0, 1)
        tlyPagina.Controls.Add(tlyJos, 0, 2)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.TabIndex = 0
        ' 
        ' tlyAntet
        ' 
        tlyAntet.ColumnCount = 5
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 34F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 36F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30F))
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Margin = New Padding(0)
        tlyAntet.RowCount = 2
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyAntet.Size = New Size(100, 100)
        tlyAntet.Controls.Add(lblPartener, 0, 0)
        tlyAntet.Controls.Add(lblClsf, 1, 0)
        tlyAntet.Controls.Add(lblNota, 2, 0)
        tlyAntet.Controls.Add(lblDataOp, 3, 0)
        tlyAntet.Controls.Add(lblIban, 4, 0)
        tlyAntet.Controls.Add(cmbPartener, 0, 1)
        tlyAntet.Controls.Add(cmbClsf, 1, 1)
        tlyAntet.Controls.Add(cmbNota, 2, 1)
        tlyAntet.Controls.Add(dtpDataOp, 3, 1)
        tlyAntet.Controls.Add(txtIban, 4, 1)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.TabIndex = 1
        ' 
        ' lblPartener
        ' 
        lblPartener.BorderWidth = 0
        lblPartener.Dock = DockStyle.Fill
        lblPartener.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblPartener.Margin = New Padding(0, 0, 3, 3)
        lblPartener.Text = "Partener"
        lblPartener.TextAlign = ContentAlignment.MiddleCenter
        lblPartener.Name = "lblPartener"
        lblPartener.TabIndex = 2
        ' 
        ' lblClsf
        ' 
        lblClsf.BorderWidth = 0
        lblClsf.Dock = DockStyle.Fill
        lblClsf.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblClsf.Margin = New Padding(0, 0, 3, 3)
        lblClsf.Text = "Clasificație bugetară"
        lblClsf.TextAlign = ContentAlignment.MiddleCenter
        lblClsf.Name = "lblClsf"
        lblClsf.TabIndex = 3
        ' 
        ' lblNota
        ' 
        lblNota.BorderWidth = 0
        lblNota.Dock = DockStyle.Fill
        lblNota.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNota.Margin = New Padding(0, 0, 3, 3)
        lblNota.Text = "Nota contabilă"
        lblNota.TextAlign = ContentAlignment.MiddleCenter
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 4
        ' 
        ' lblDataOp
        ' 
        lblDataOp.BorderWidth = 0
        lblDataOp.Dock = DockStyle.Fill
        lblDataOp.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblDataOp.Margin = New Padding(0, 0, 3, 3)
        lblDataOp.Text = "Data O.P."
        lblDataOp.TextAlign = ContentAlignment.MiddleCenter
        lblDataOp.Name = "lblDataOp"
        lblDataOp.TabIndex = 5
        ' 
        ' lblIban
        ' 
        lblIban.BorderWidth = 0
        lblIban.Dock = DockStyle.Fill
        lblIban.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblIban.Margin = New Padding(0, 0, 3, 3)
        lblIban.Text = "Cont IBAN"
        lblIban.TextAlign = ContentAlignment.MiddleCenter
        lblIban.Name = "lblIban"
        lblIban.TabIndex = 6
        ' 
        ' cmbPartener
        ' 
        cmbPartener.Dock = DockStyle.Fill
        cmbPartener.Editable = False
        cmbPartener.FindAsYouType = True
        cmbPartener.FindInItemOrder = True
        cmbPartener.FitDropDownToWorkingArea = True
        cmbPartener.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbPartener.Margin = New Padding(0, 4, 6, 4)
        cmbPartener.MaxDropDownItems = 15
        cmbPartener.PlaceholderText = "Alegeți partenerul"
        cmbPartener.Size = New Size(100, 40)
        cmbPartener.Enabled = False
        cmbPartener.Name = "cmbPartener"
        cmbPartener.TabIndex = 7
        tips.SetToolTipHeader(cmbPartener, "Partener")
        tips.SetToolTipText(cmbPartener, "Furnizorul căruia îi înregistrați facturi sau îi plătiți. Se pot căuta cuvinte din denumire sau codul.")
        ' 
        ' cmbClsf
        ' 
        cmbClsf.Dock = DockStyle.Fill
        cmbClsf.Editable = False
        cmbClsf.FindAsYouType = True
        cmbClsf.FindInItemOrder = True
        cmbClsf.FitDropDownToWorkingArea = True
        cmbClsf.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbClsf.Margin = New Padding(0, 4, 6, 4)
        cmbClsf.MaxDropDownItems = 15
        cmbClsf.PlaceholderText = "Alegeți clasificația bugetară"
        cmbClsf.Size = New Size(100, 40)
        cmbClsf.Enabled = False
        cmbClsf.Name = "cmbClsf"
        cmbClsf.TabIndex = 8
        tips.SetToolTipHeader(cmbClsf, "Clasificația bugetară")
        tips.SetToolTipText(cmbClsf, "Sunt disponibile clasificațiile cu șabloane de operații în subunitatea în lucru.")
        ' 
        ' cmbNota
        ' 
        cmbNota.Dock = DockStyle.Fill
        cmbNota.Editable = True
        cmbNota.FindAsYouType = True
        cmbNota.FindInItemOrder = True
        cmbNota.FitDropDownToWorkingArea = True
        cmbNota.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbNota.Margin = New Padding(0, 4, 6, 4)
        cmbNota.MaxDropDownItems = 15
        cmbNota.PlaceholderText = "Nota"
        cmbNota.Size = New Size(100, 40)
        cmbNota.Enabled = False
        cmbNota.Name = "cmbNota"
        cmbNota.TabIndex = 9
        tips.SetToolTipHeader(cmbNota, "Nota contabilă")
        tips.SetToolTipText(cmbNota, "Alegeți o notă existentă sau scrieți un număr nou. Fără alegere se folosește numărul următor.")
        ' 
        ' dtpDataOp
        ' 
        dtpDataOp.Dock = DockStyle.Fill
        dtpDataOp.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        dtpDataOp.Format = "dd.MM.yyyy"
        dtpDataOp.Margin = New Padding(0, 4, 6, 4)
        dtpDataOp.Size = New Size(100, 40)
        dtpDataOp.Name = "dtpDataOp"
        dtpDataOp.TabIndex = 10
        tips.SetToolTipHeader(dtpDataOp, "Data ordinului de plată")
        tips.SetToolTipText(dtpDataOp, "Data plății și a ordinului de plată.")
        ' 
        ' txtIban
        ' 
        txtIban.Dock = DockStyle.Fill
        txtIban.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtIban.Margin = New Padding(0, 4, 0, 4)
        txtIban.Multiline = False
        txtIban.ScrollBars = ScrollBars.None
        txtIban.Size = New Size(100, 40)
        txtIban.TextAlign = HorizontalAlignment.Center
        txtIban.Enabled = False
        txtIban.Name = "txtIban"
        txtIban.TabIndex = 11
        tips.SetToolTipHeader(txtIban, "Cont IBAN")
        tips.SetToolTipText(txtIban, "Contul bancar al partenerului pentru clasificația aleasă (din configurarea partenerului).")
        ' 
        ' tlyMijloc
        ' 
        tlyMijloc.ColumnCount = 2
        tlyMijloc.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyMijloc.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMijloc.Dock = DockStyle.Fill
        tlyMijloc.Margin = New Padding(0)
        tlyMijloc.RowCount = 1
        tlyMijloc.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMijloc.Size = New Size(100, 100)
        tlyMijloc.Controls.Add(tlySabloane, 0, 0)
        tlyMijloc.Controls.Add(tlyGrile, 1, 0)
        tlyMijloc.Name = "tlyMijloc"
        tlyMijloc.TabIndex = 12
        ' 
        ' tlySabloane
        ' 
        tlySabloane.ColumnCount = 1
        tlySabloane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySabloane.Dock = DockStyle.Fill
        tlySabloane.Margin = New Padding(0)
        tlySabloane.RowCount = 4
        tlySabloane.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlySabloane.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlySabloane.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlySabloane.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlySabloane.Size = New Size(100, 100)
        tlySabloane.Controls.Add(lblEvid, 0, 0)
        tlySabloane.Controls.Add(lstEvidentiere, 0, 1)
        tlySabloane.Controls.Add(lblSabP, 0, 2)
        tlySabloane.Controls.Add(lstPlata, 0, 3)
        tlySabloane.Name = "tlySabloane"
        tlySabloane.TabIndex = 13
        ' 
        ' lblEvid
        ' 
        lblEvid.BorderWidth = 0
        lblEvid.Dock = DockStyle.Fill
        lblEvid.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblEvid.Margin = New Padding(0, 0, 3, 0)
        lblEvid.Text = "Șabloane evidențiere (facturi noi)"
        lblEvid.TextAlign = ContentAlignment.MiddleLeft
        lblEvid.Name = "lblEvid"
        lblEvid.TabIndex = 14
        ' 
        ' lstEvidentiere
        ' 
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Evidențiere"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Width = 300
        lstEvidentiere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstEvidentiere.BackColor = SystemColors.Window
        lstEvidentiere.ColumnFillMode = KBotFillMode.SpecificColumn
        lstEvidentiere.Columns.Add(KBotDataColumn1)
        lstEvidentiere.Dock = DockStyle.Fill
        lstEvidentiere.Enabled = False
        lstEvidentiere.FillColumnKey = "nume"
        lstEvidentiere.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstEvidentiere.Margin = New Padding(0, 0, 6, 6)
        lstEvidentiere.ReadOnlyGrid = True
        lstEvidentiere.Size = New Size(100, 100)
        lstEvidentiere.Name = "lstEvidentiere"
        lstEvidentiere.TabIndex = 15
        ' 
        ' lblSabP
        ' 
        lblSabP.BorderWidth = 0
        lblSabP.Dock = DockStyle.Fill
        lblSabP.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblSabP.Margin = New Padding(0, 0, 3, 0)
        lblSabP.Text = "Șabloane plată"
        lblSabP.TextAlign = ContentAlignment.MiddleLeft
        lblSabP.Name = "lblSabP"
        lblSabP.TabIndex = 16
        ' 
        ' lstPlata
        ' 
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Plată"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nume"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Width = 300
        lstPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstPlata.BackColor = SystemColors.Window
        lstPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        lstPlata.Columns.Add(KBotDataColumn2)
        lstPlata.Dock = DockStyle.Fill
        lstPlata.Enabled = False
        lstPlata.FillColumnKey = "nume"
        lstPlata.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstPlata.Margin = New Padding(0, 0, 6, 0)
        lstPlata.ReadOnlyGrid = True
        lstPlata.Size = New Size(100, 100)
        lstPlata.Name = "lstPlata"
        lstPlata.TabIndex = 17
        ' 
        ' tlyGrile
        ' 
        tlyGrile.ColumnCount = 1
        tlyGrile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyGrile.Dock = DockStyle.Fill
        tlyGrile.Margin = New Padding(0)
        tlyGrile.RowCount = 8
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Percent, 22F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Percent, 26F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Percent, 20F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyGrile.RowStyles.Add(New RowStyle(SizeType.Percent, 22F))
        tlyGrile.Size = New Size(100, 100)
        tlyGrile.Controls.Add(lblNoi, 0, 0)
        tlyGrile.Controls.Add(gridNoi, 0, 1)
        tlyGrile.Controls.Add(lblCurent, 0, 2)
        tlyGrile.Controls.Add(gridCurent, 0, 3)
        tlyGrile.Controls.Add(lblAnterior, 0, 4)
        tlyGrile.Controls.Add(gridAnterior, 0, 5)
        tlyGrile.Controls.Add(lblPlati, 0, 6)
        tlyGrile.Controls.Add(gridPlati, 0, 7)
        tlyGrile.Name = "tlyGrile"
        tlyGrile.TabIndex = 18
        ' 
        ' lblNoi
        ' 
        lblNoi.BorderWidth = 0
        lblNoi.Dock = DockStyle.Fill
        lblNoi.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblNoi.Margin = New Padding(0, 0, 3, 0)
        lblNoi.Text = "Facturi noi (introduse de mână)"
        lblNoi.TextAlign = ContentAlignment.MiddleLeft
        lblNoi.Name = "lblNoi"
        lblNoi.TabIndex = 19
        ' 
        ' gridNoi
        ' 
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Număr factură"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "numar"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 200
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Data"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "data"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Combo
        KBotDataColumn5.CommitOnSelect = True
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Fel"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "fel"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Valoare"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "suma"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 160
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "De plată"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.HeaderTooltip = "Cât se plătește acum din factură (0 = nimic)."
        KBotDataColumn7.Key = "platit"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 160
        gridNoi.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridNoi.BackColor = SystemColors.Window
        gridNoi.ColumnFillMode = KBotFillMode.SpecificColumn
        gridNoi.Columns.Add(KBotDataColumn3)
        gridNoi.Columns.Add(KBotDataColumn4)
        gridNoi.Columns.Add(KBotDataColumn5)
        gridNoi.Columns.Add(KBotDataColumn6)
        gridNoi.Columns.Add(KBotDataColumn7)
        gridNoi.Dock = DockStyle.Fill
        gridNoi.Enabled = False
        gridNoi.FillColumnKey = "numar"
        gridNoi.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridNoi.FooterVisible = True
        gridNoi.Margin = New Padding(0)
        gridNoi.Size = New Size(100, 100)
        gridNoi.Name = "gridNoi"
        gridNoi.TabIndex = 20
        ' 
        ' lblCurent
        ' 
        lblCurent.BorderWidth = 0
        lblCurent.Dock = DockStyle.Fill
        lblCurent.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblCurent.Margin = New Padding(0, 0, 3, 0)
        lblCurent.Text = "Facturi din anul curent, neachitate"
        lblCurent.TextAlign = ContentAlignment.MiddleLeft
        lblCurent.Name = "lblCurent"
        lblCurent.TabIndex = 21
        ' 
        ' gridCurent
        ' 
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Document"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "numar"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Width = 200
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Data"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "data"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Width = 150
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Fel"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "fel"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Width = 150
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Valoare"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "valoare"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 150
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.Aggregate = KBotAggregate.Sum
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Rest de plată"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "rest"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBotValueType.Number
        KBotDataColumn12.Width = 160
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.Aggregate = KBotAggregate.Sum
        KBotDataColumn13.DecimalPlaces = 2
        KBotDataColumn13.Format = KBotFormat.Standard
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "De plată"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.HeaderTooltip = "Cât se plătește acum din rest. Nu poate depăși restul de plată."
        KBotDataColumn13.Key = "platit"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn13.ValueType = KBotValueType.Number
        KBotDataColumn13.Width = 160
        gridCurent.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridCurent.BackColor = SystemColors.Window
        gridCurent.ColumnFillMode = KBotFillMode.SpecificColumn
        gridCurent.Columns.Add(KBotDataColumn8)
        gridCurent.Columns.Add(KBotDataColumn9)
        gridCurent.Columns.Add(KBotDataColumn10)
        gridCurent.Columns.Add(KBotDataColumn11)
        gridCurent.Columns.Add(KBotDataColumn12)
        gridCurent.Columns.Add(KBotDataColumn13)
        gridCurent.Dock = DockStyle.Fill
        gridCurent.Enabled = False
        gridCurent.FillColumnKey = "numar"
        gridCurent.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridCurent.FooterVisible = True
        gridCurent.Margin = New Padding(0)
        gridCurent.Size = New Size(100, 100)
        gridCurent.Name = "gridCurent"
        gridCurent.TabIndex = 22
        ' 
        ' lblAnterior
        ' 
        lblAnterior.BorderWidth = 0
        lblAnterior.Dock = DockStyle.Fill
        lblAnterior.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblAnterior.Margin = New Padding(0, 0, 3, 0)
        lblAnterior.Text = "Facturi din anul anterior, neachitate"
        lblAnterior.TextAlign = ContentAlignment.MiddleLeft
        lblAnterior.Name = "lblAnterior"
        lblAnterior.TabIndex = 23
        ' 
        ' gridAnterior
        ' 
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Document"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Key = "numar"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Width = 200
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Data"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "data"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.ReadOnly = True
        KBotDataColumn15.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Width = 150
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Fel"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn16.Key = "fel"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn16.Width = 150
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.DecimalPlaces = 2
        KBotDataColumn17.Format = KBotFormat.Standard
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Valoare"
        KBotDataColumn17.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn17.Key = "valoare"
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.ReadOnly = True
        KBotDataColumn17.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn17.ValueType = KBotValueType.Number
        KBotDataColumn17.Width = 150
        KBotDataColumn18.AggregateFormatString = Nothing
        KBotDataColumn18.Aggregate = KBotAggregate.Sum
        KBotDataColumn18.DecimalPlaces = 2
        KBotDataColumn18.Format = KBotFormat.Standard
        KBotDataColumn18.FormatString = Nothing
        KBotDataColumn18.HeaderText = "Rest de plată"
        KBotDataColumn18.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn18.Key = "rest"
        KBotDataColumn18.OptionGroup = Nothing
        KBotDataColumn18.ReadOnly = True
        KBotDataColumn18.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn18.ValueType = KBotValueType.Number
        KBotDataColumn18.Width = 160
        KBotDataColumn19.AggregateFormatString = Nothing
        KBotDataColumn19.Aggregate = KBotAggregate.Sum
        KBotDataColumn19.DecimalPlaces = 2
        KBotDataColumn19.Format = KBotFormat.Standard
        KBotDataColumn19.FormatString = Nothing
        KBotDataColumn19.HeaderText = "De plată"
        KBotDataColumn19.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn19.HeaderTooltip = "Cât se plătește acum din rest. Nu poate depăși restul de plată."
        KBotDataColumn19.Key = "platit"
        KBotDataColumn19.OptionGroup = Nothing
        KBotDataColumn19.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn19.ValueType = KBotValueType.Number
        KBotDataColumn19.Width = 160
        gridAnterior.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAnterior.BackColor = SystemColors.Window
        gridAnterior.ColumnFillMode = KBotFillMode.SpecificColumn
        gridAnterior.Columns.Add(KBotDataColumn14)
        gridAnterior.Columns.Add(KBotDataColumn15)
        gridAnterior.Columns.Add(KBotDataColumn16)
        gridAnterior.Columns.Add(KBotDataColumn17)
        gridAnterior.Columns.Add(KBotDataColumn18)
        gridAnterior.Columns.Add(KBotDataColumn19)
        gridAnterior.Dock = DockStyle.Fill
        gridAnterior.Enabled = False
        gridAnterior.FillColumnKey = "numar"
        gridAnterior.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridAnterior.FooterVisible = True
        gridAnterior.Margin = New Padding(0)
        gridAnterior.Size = New Size(100, 100)
        gridAnterior.Name = "gridAnterior"
        gridAnterior.TabIndex = 24
        ' 
        ' lblPlati
        ' 
        lblPlati.BorderWidth = 0
        lblPlati.Dock = DockStyle.Fill
        lblPlati.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblPlati.Margin = New Padding(0, 0, 3, 0)
        lblPlati.Text = "Ordine de plată ale partenerului în anul de lucru"
        lblPlati.TextAlign = ContentAlignment.MiddleLeft
        lblPlati.Name = "lblPlati"
        lblPlati.TabIndex = 25
        ' 
        ' gridPlati
        ' 
        KBotDataColumn20.AggregateFormatString = Nothing
        KBotDataColumn20.FormatString = Nothing
        KBotDataColumn20.HeaderText = "Număr OP"
        KBotDataColumn20.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Key = "nrop"
        KBotDataColumn20.OptionGroup = Nothing
        KBotDataColumn20.ReadOnly = True
        KBotDataColumn20.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Width = 120
        KBotDataColumn21.AggregateFormatString = Nothing
        KBotDataColumn21.FormatString = Nothing
        KBotDataColumn21.HeaderText = "Dată"
        KBotDataColumn21.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Key = "data"
        KBotDataColumn21.OptionGroup = Nothing
        KBotDataColumn21.ReadOnly = True
        KBotDataColumn21.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Width = 140
        KBotDataColumn22.AggregateFormatString = Nothing
        KBotDataColumn22.FormatString = Nothing
        KBotDataColumn22.HeaderText = "Documente"
        KBotDataColumn22.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Key = "documente"
        KBotDataColumn22.OptionGroup = Nothing
        KBotDataColumn22.ReadOnly = True
        KBotDataColumn22.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn22.Width = 300
        KBotDataColumn23.AggregateFormatString = Nothing
        KBotDataColumn23.FormatString = Nothing
        KBotDataColumn23.HeaderText = "Explicație O.P."
        KBotDataColumn23.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.Key = "explicatie"
        KBotDataColumn23.OptionGroup = Nothing
        KBotDataColumn23.ReadOnly = True
        KBotDataColumn23.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn23.Width = 420
        KBotDataColumn24.AggregateFormatString = Nothing
        KBotDataColumn24.Aggregate = KBotAggregate.Sum
        KBotDataColumn24.DecimalPlaces = 2
        KBotDataColumn24.Format = KBotFormat.Standard
        KBotDataColumn24.FormatString = Nothing
        KBotDataColumn24.HeaderText = "Valoare"
        KBotDataColumn24.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.Key = "valoare"
        KBotDataColumn24.OptionGroup = Nothing
        KBotDataColumn24.ReadOnly = True
        KBotDataColumn24.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn24.ValueType = KBotValueType.Number
        KBotDataColumn24.Width = 160
        KBotDataColumn25.AggregateFormatString = Nothing
        KBotDataColumn25.FormatString = Nothing
        KBotDataColumn25.HeaderText = "Stare"
        KBotDataColumn25.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Key = "stare"
        KBotDataColumn25.OptionGroup = Nothing
        KBotDataColumn25.ReadOnly = True
        KBotDataColumn25.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Width = 160
        gridPlati.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPlati.BackColor = SystemColors.Window
        gridPlati.ColumnFillMode = KBotFillMode.SpecificColumn
        gridPlati.Columns.Add(KBotDataColumn20)
        gridPlati.Columns.Add(KBotDataColumn21)
        gridPlati.Columns.Add(KBotDataColumn22)
        gridPlati.Columns.Add(KBotDataColumn23)
        gridPlati.Columns.Add(KBotDataColumn24)
        gridPlati.Columns.Add(KBotDataColumn25)
        gridPlati.Dock = DockStyle.Fill
        gridPlati.Enabled = False
        gridPlati.FillColumnKey = "explicatie"
        gridPlati.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridPlati.FooterVisible = True
        gridPlati.Margin = New Padding(0)
        gridPlati.ReadOnlyGrid = True
        gridPlati.Size = New Size(100, 100)
        gridPlati.Name = "gridPlati"
        gridPlati.TabIndex = 26
        ' 
        ' tlyJos
        ' 
        tlyJos.ColumnCount = 8
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 440F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyJos.Dock = DockStyle.Fill
        tlyJos.Margin = New Padding(0)
        tlyJos.RowCount = 2
        tlyJos.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyJos.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyJos.Size = New Size(100, 100)
        tlyJos.Controls.Add(lblExplicatie, 0, 0)
        tlyJos.Controls.Add(txtExplicatie, 1, 0)
        tlyJos.Controls.Add(lblNumarOp, 2, 0)
        tlyJos.Controls.Add(lblStare, 0, 1)
        tlyJos.SetColumnSpan(lblStare, 3)
        tlyJos.Controls.Add(btnAdauga, 3, 1)
        tlyJos.Controls.Add(btnStergeRand, 4, 1)
        tlyJos.Controls.Add(btnStergePlata, 5, 1)
        tlyJos.Controls.Add(btnRenunta, 6, 1)
        tlyJos.Controls.Add(btnSalveaza, 7, 1)
        tlyJos.Name = "tlyJos"
        tlyJos.TabIndex = 27
        ' 
        ' lblExplicatie
        ' 
        lblExplicatie.BorderWidth = 0
        lblExplicatie.Dock = DockStyle.Fill
        lblExplicatie.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblExplicatie.Margin = New Padding(0, 0, 3, 0)
        lblExplicatie.Text = "Explicație O.P."
        lblExplicatie.TextAlign = ContentAlignment.MiddleLeft
        lblExplicatie.Name = "lblExplicatie"
        lblExplicatie.TabIndex = 28
        ' 
        ' txtExplicatie
        ' 
        txtExplicatie.Dock = DockStyle.Fill
        txtExplicatie.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtExplicatie.Margin = New Padding(0, 4, 6, 4)
        txtExplicatie.MaxLength = 75
        txtExplicatie.Multiline = False
        txtExplicatie.ScrollBars = ScrollBars.None
        txtExplicatie.Size = New Size(100, 40)
        txtExplicatie.TextAlign = HorizontalAlignment.Left
        txtExplicatie.Enabled = False
        txtExplicatie.Name = "txtExplicatie"
        txtExplicatie.TabIndex = 29
        tips.SetToolTipHeader(txtExplicatie, "Explicația ordinului de plată")
        tips.SetToolTipText(txtExplicatie, "Între 1 și 75 de caractere, fără diacritice și fără virgule (ANAF le refuză). Dacă o lăsați goală, se compune din numerele facturilor plătite și felul din șablon.")
        ' 
        ' lblNumarOp
        ' 
        lblNumarOp.BorderWidth = 0
        lblNumarOp.Dock = DockStyle.Fill
        lblNumarOp.Font = New Font("Calibri", 10F, FontStyle.Bold)
        lblNumarOp.Margin = New Padding(0)
        lblNumarOp.Text = ""
        lblNumarOp.TextAlign = ContentAlignment.MiddleCenter
        lblNumarOp.Name = "lblNumarOp"
        lblNumarOp.TabIndex = 30
        tips.SetToolTipHeader(lblNumarOp, "Număr ordin de plată")
        tips.SetToolTipText(lblNumarOp, "Numărul care ar urma acum. Este PROVIZORIU: numărul final se stabilește la salvare și poate fi altul, dacă între timp s-a emis un alt ordin. După salvare vezi numărul real.")
        ' 
        ' lblStare
        ' 
        lblStare.BorderWidth = 0
        lblStare.Dock = DockStyle.Fill
        lblStare.Font = New Font("Calibri", 9F)
        lblStare.Margin = New Padding(0, 0, 8, 0)
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 31
        ' 
        ' btnAdauga
        ' 
        btnAdauga.Dock = DockStyle.Fill
        btnAdauga.Enabled = False
        btnAdauga.Margin = New Padding(6, 4, 0, 4)
        btnAdauga.Size = New Size(100, 40)
        btnAdauga.Text = "Adaugă factură"
        btnAdauga.Name = "btnAdauga"
        btnAdauga.TabIndex = 32
        tips.SetToolTipHeader(btnAdauga, "Adaugă factură")
        tips.SetToolTipText(btnAdauga, "Adaugă un rând pentru o factură nouă, introdusă de mână. Se înregistrează cu șablonul de evidențiere ales.")
        ' 
        ' btnStergeRand
        ' 
        btnStergeRand.Dock = DockStyle.Fill
        btnStergeRand.Enabled = False
        btnStergeRand.Margin = New Padding(6, 4, 0, 4)
        btnStergeRand.Size = New Size(100, 40)
        btnStergeRand.Text = "Șterge rândul"
        btnStergeRand.Name = "btnStergeRand"
        btnStergeRand.TabIndex = 33
        tips.SetToolTipHeader(btnStergeRand, "Șterge rândul")
        tips.SetToolTipText(btnStergeRand, "Scoate din listă factura nouă pe care stați (încă nesalvată).")
        ' 
        ' btnStergePlata
        ' 
        btnStergePlata.Dock = DockStyle.Fill
        btnStergePlata.Enabled = False
        btnStergePlata.Margin = New Padding(6, 4, 0, 4)
        btnStergePlata.Size = New Size(100, 40)
        btnStergePlata.Text = "Șterge plata"
        btnStergePlata.Name = "btnStergePlata"
        btnStergePlata.TabIndex = 34
        tips.SetToolTipHeader(btnStergePlata, "Șterge plata")
        tips.SetToolTipText(btnStergePlata, "Șterge plata aleasă în tabelul ordinelor, împreună cu ordinul ei. Nu merge dacă ordinul e într-un OPME sau confirmat prin extras.")
        ' 
        ' btnRenunta
        ' 
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Enabled = False
        btnRenunta.Margin = New Padding(6, 4, 0, 4)
        btnRenunta.Size = New Size(100, 40)
        btnRenunta.Text = "Renunță"
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 35
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Renunță la ce ați completat și reia datele partenerului.")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Primary = True
        btnSalveaza.Margin = New Padding(6, 4, 0, 4)
        btnSalveaza.Size = New Size(100, 40)
        btnSalveaza.Text = "Salvează"
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 36
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Înregistrează facturile noi și/sau plata cu ordinul ei.")
        ' 
        ' FacturiPlatiPage
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Margin = New Padding(4, 4, 4, 4)
        Name = "FacturiPlatiPage"
        Size = New Size(1685, 900)
        tlyPagina.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        tlyMijloc.ResumeLayout(False)
        tlySabloane.ResumeLayout(False)
        tlyGrile.ResumeLayout(False)
        tlyJos.ResumeLayout(False)
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).EndInit()
        CType(lstPlata, ComponentModel.ISupportInitialize).EndInit()
        CType(gridNoi, ComponentModel.ISupportInitialize).EndInit()
        CType(gridCurent, ComponentModel.ISupportInitialize).EndInit()
        CType(gridAnterior, ComponentModel.ISupportInitialize).EndInit()
        CType(gridPlati, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlyAntet As KBotTableLayoutPanel
    Friend WithEvents lblPartener As KBotLabel
    Friend WithEvents lblClsf As KBotLabel
    Friend WithEvents lblNota As KBotLabel
    Friend WithEvents lblDataOp As KBotLabel
    Friend WithEvents lblIban As KBotLabel
    Friend WithEvents cmbPartener As KBotComboBox
    Friend WithEvents cmbClsf As KBotComboBox
    Friend WithEvents cmbNota As KBotComboBox
    Friend WithEvents dtpDataOp As KBotDatePicker
    Friend WithEvents txtIban As KBotTextBox
    Friend WithEvents tlyMijloc As KBotTableLayoutPanel
    Friend WithEvents tlySabloane As KBotTableLayoutPanel
    Friend WithEvents lblEvid As KBotLabel
    Friend WithEvents lstEvidentiere As KBotDataView
    Friend WithEvents lblSabP As KBotLabel
    Friend WithEvents lstPlata As KBotDataView
    Friend WithEvents tlyGrile As KBotTableLayoutPanel
    Friend WithEvents lblNoi As KBotLabel
    Friend WithEvents gridNoi As KBotDataView
    Friend WithEvents lblCurent As KBotLabel
    Friend WithEvents gridCurent As KBotDataView
    Friend WithEvents lblAnterior As KBotLabel
    Friend WithEvents gridAnterior As KBotDataView
    Friend WithEvents lblPlati As KBotLabel
    Friend WithEvents gridPlati As KBotDataView
    Friend WithEvents tlyJos As KBotTableLayoutPanel
    Friend WithEvents lblExplicatie As KBotLabel
    Friend WithEvents txtExplicatie As KBotTextBox
    Friend WithEvents lblNumarOp As KBotLabel
    Friend WithEvents lblStare As KBotLabel
    Friend WithEvents btnAdauga As KBotButton
    Friend WithEvents btnStergeRand As KBotButton
    Friend WithEvents btnStergePlata As KBotButton
    Friend WithEvents btnRenunta As KBotButton
    Friend WithEvents btnSalveaza As KBotButton
End Class
