Imports KBot.Controls

' «Facturi si plati» page (slice AV02-04, redesigned 10.11.2026; the Access form OPuri2017; machete type M = classification + partner on ONE side). Layout (operator's sketch, 10.11.2026):
'   top      : Clasificatia (over the left column) | Partenerul | Cod client | Cod bancar
'   middle   : left = «Documente evidentiere» (templates) | right = a nav list (Facturi noi / an curent / an anterior) over ONE view = one stand-alone grid per option
'              (the grid of the new invoices always keeps a blank row at the end for adding)
'   bottom   : left = «Documente plata» (templates) | right = the payment grid (first row = the payment being made: O.P. no., date, note, explanation; below it the orders of the year)
'   very low : status line + the buttons
' Nothing here is shared with the E-Factura pages. All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
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
        Dim KBotDataColumn26 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn27 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn28 As KBotDataColumn = New KBotDataColumn()
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyPagina = New KBotTableLayoutPanel()
        tlyClsf = New KBotTableLayoutPanel()
        lblClsf = New KBotLabel()
        cmbClsf = New KBotComboBox()
        tlyAntet = New KBotTableLayoutPanel()
        lblPartener = New KBotLabel()
        lblCodClient = New KBotLabel()
        lblIban = New KBotLabel()
        cmbPartener = New KBotComboBox()
        txtCodClient = New KBotTextBox()
        txtIban = New KBotTextBox()
        lstEvidentiere = New KBotDataView()
        tlyView = New KBotTableLayoutPanel()
        navFacturi = New KBotNavList()
        pnlView = New Panel()
        gridNoi = New KBotDataView()
        gridCurent = New KBotDataView()
        gridAnterior = New KBotDataView()
        lstPlata = New KBotDataView()
        gridPlata = New KBotDataView()
        tlyJos = New KBotTableLayoutPanel()
        lblStare = New KBotLabel()
        btnRenunta = New KBotButton()
        btnSalveaza = New KBotButton()
        tlyPagina.SuspendLayout()
        tlyClsf.SuspendLayout()
        tlyAntet.SuspendLayout()
        tlyView.SuspendLayout()
        pnlView.SuspendLayout()
        tlyJos.SuspendLayout()
        CType(navFacturi, ComponentModel.ISupportInitialize).BeginInit()
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).BeginInit()
        CType(lstPlata, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridNoi, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridCurent, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridAnterior, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPlata, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyPagina
        '
        tlyPagina.ColumnCount = 2
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Margin = New Padding(0)
        tlyPagina.RowCount = 4
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 104F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 62F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 38F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyPagina.Size = New Size(100, 100)
        tlyPagina.Controls.Add(tlyClsf, 0, 0)
        tlyPagina.Controls.Add(tlyAntet, 1, 0)
        tlyPagina.Controls.Add(lstEvidentiere, 0, 1)
        tlyPagina.Controls.Add(tlyView, 1, 1)
        tlyPagina.Controls.Add(lstPlata, 0, 2)
        tlyPagina.Controls.Add(gridPlata, 1, 2)
        tlyPagina.Controls.Add(tlyJos, 0, 3)
        tlyPagina.SetColumnSpan(tlyJos, 2)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.TabIndex = 0
        '
        ' tlyClsf
        '
        tlyClsf.ColumnCount = 1
        tlyClsf.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyClsf.Dock = DockStyle.Fill
        tlyClsf.Margin = New Padding(0)
        tlyClsf.RowCount = 2
        tlyClsf.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyClsf.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyClsf.Size = New Size(100, 100)
        tlyClsf.Controls.Add(lblClsf, 0, 0)
        tlyClsf.Controls.Add(cmbClsf, 0, 1)
        tlyClsf.Name = "tlyClsf"
        tlyClsf.TabIndex = 1
        '
        ' lblClsf
        '
        lblClsf.BorderWidth = 0
        lblClsf.Dock = DockStyle.Fill
        lblClsf.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblClsf.Margin = New Padding(0, 0, 6, 3)
        lblClsf.Text = "Clasificația"
        lblClsf.TextAlign = ContentAlignment.MiddleCenter
        lblClsf.Name = "lblClsf"
        lblClsf.TabIndex = 2
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
        cmbClsf.PlaceholderText = "Alegeți clasificația"
        cmbClsf.Size = New Size(100, 40)
        cmbClsf.Enabled = False
        cmbClsf.Name = "cmbClsf"
        cmbClsf.TabIndex = 3
        tips.SetToolTipHeader(cmbClsf, "Clasificația bugetară")
        tips.SetToolTipText(cmbClsf, "Sunt disponibile clasificațiile cu șabloane de operații în subunitatea în lucru.")
        '
        ' tlyAntet
        '
        tlyAntet.ColumnCount = 3
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 520F))
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Margin = New Padding(0)
        tlyAntet.RowCount = 2
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyAntet.Size = New Size(100, 100)
        tlyAntet.Controls.Add(lblPartener, 0, 0)
        tlyAntet.Controls.Add(lblCodClient, 1, 0)
        tlyAntet.Controls.Add(lblIban, 2, 0)
        tlyAntet.Controls.Add(cmbPartener, 0, 1)
        tlyAntet.Controls.Add(txtCodClient, 1, 1)
        tlyAntet.Controls.Add(txtIban, 2, 1)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.TabIndex = 4
        '
        ' lblPartener
        '
        lblPartener.BorderWidth = 0
        lblPartener.Dock = DockStyle.Fill
        lblPartener.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblPartener.Margin = New Padding(0, 0, 6, 3)
        lblPartener.Text = "Partenerul"
        lblPartener.TextAlign = ContentAlignment.MiddleCenter
        lblPartener.Name = "lblPartener"
        lblPartener.TabIndex = 5
        '
        ' lblCodClient
        '
        lblCodClient.BorderWidth = 0
        lblCodClient.Dock = DockStyle.Fill
        lblCodClient.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblCodClient.Margin = New Padding(0, 0, 6, 3)
        lblCodClient.Text = "Cod client"
        lblCodClient.TextAlign = ContentAlignment.MiddleCenter
        lblCodClient.Name = "lblCodClient"
        lblCodClient.TabIndex = 6
        '
        ' lblIban
        '
        lblIban.BorderWidth = 0
        lblIban.Dock = DockStyle.Fill
        lblIban.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblIban.Margin = New Padding(0, 0, 0, 3)
        lblIban.Text = "Cod bancar"
        lblIban.TextAlign = ContentAlignment.MiddleCenter
        lblIban.Name = "lblIban"
        lblIban.TabIndex = 7
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
        cmbPartener.TabIndex = 8
        tips.SetToolTipHeader(cmbPartener, "Partenerul")
        tips.SetToolTipText(cmbPartener, "Furnizorul căruia îi înregistrați facturi sau îi plătiți. Se pot căuta cuvinte din denumire sau codul.")
        '
        ' txtCodClient
        '
        txtCodClient.Dock = DockStyle.Fill
        txtCodClient.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtCodClient.Margin = New Padding(0, 4, 6, 4)
        txtCodClient.Multiline = False
        txtCodClient.ScrollBars = ScrollBars.None
        txtCodClient.Size = New Size(100, 40)
        txtCodClient.TextAlign = HorizontalAlignment.Center
        txtCodClient.Enabled = False
        txtCodClient.Name = "txtCodClient"
        txtCodClient.TabIndex = 9
        tips.SetToolTipHeader(txtCodClient, "Cod client")
        tips.SetToolTipText(txtCodClient, "Codul partenerului, din configurarea partenerului.")
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
        txtIban.TabIndex = 10
        tips.SetToolTipHeader(txtIban, "Cod bancar")
        tips.SetToolTipText(txtIban, "Contul bancar al partenerului pentru clasificația aleasă (din configurarea partenerului).")
        '
        ' lstEvidentiere
        '
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Documente evidențiere"
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
        lstEvidentiere.Margin = New Padding(0, 6, 6, 6)
        lstEvidentiere.ReadOnlyGrid = True
        lstEvidentiere.Size = New Size(100, 100)
        lstEvidentiere.Name = "lstEvidentiere"
        lstEvidentiere.TabIndex = 11
        '
        ' tlyView
        '
        tlyView.ColumnCount = 1
        tlyView.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyView.Dock = DockStyle.Fill
        tlyView.Margin = New Padding(0)
        tlyView.RowCount = 2
        tlyView.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyView.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyView.Size = New Size(100, 100)
        tlyView.Controls.Add(navFacturi, 0, 0)
        tlyView.Controls.Add(pnlView, 0, 1)
        tlyView.Name = "tlyView"
        tlyView.TabIndex = 12
        '
        ' navFacturi
        '
        navFacturi.Dock = DockStyle.Fill
        navFacturi.ItemCornerRadius = 2
        navFacturi.ItemPadding = New Padding(3)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Key = "noi"
        KBotNavItem1.Text = "Facturi noi"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Key = "curent"
        KBotNavItem2.Text = "Facturi an curent"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Key = "anterior"
        KBotNavItem3.Text = "Facturi an anterior"
        navFacturi.Items.Add(KBotNavItem1)
        navFacturi.Items.Add(KBotNavItem2)
        navFacturi.Items.Add(KBotNavItem3)
        navFacturi.Margin = New Padding(0, 6, 0, 0)
        navFacturi.Name = "navFacturi"
        navFacturi.Orientation = KBotNavOrientation.Horizontal
        navFacturi.SelectedKey = "noi"
        navFacturi.Size = New Size(100, 44)
        navFacturi.TabIndex = 13
        '
        ' pnlView
        '
        pnlView.Controls.Add(gridNoi)
        pnlView.Controls.Add(gridCurent)
        pnlView.Controls.Add(gridAnterior)
        pnlView.Dock = DockStyle.Fill
        pnlView.Margin = New Padding(0)
        pnlView.Name = "pnlView"
        pnlView.Size = New Size(100, 100)
        pnlView.TabIndex = 14
        '
        ' gridNoi
        '
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Număr factură"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "numar"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 200
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Data"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "data"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 150
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnType = KBotColumnType.Combo
        KBotDataColumn4.CommitOnSelect = True
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Fel"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "fel"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.Aggregate = KBotAggregate.Sum
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Valoare"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "suma"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 160
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "De plată"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.HeaderTooltip = "Cât se plătește acum din factură (0 = nimic)."
        KBotDataColumn6.Key = "platit"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 160
        KBotDataColumn27.AggregateFormatString = Nothing
        KBotDataColumn27.ColumnType = KBotColumnType.Button
        KBotDataColumn27.ButtonKind = KBotButtonKind.Delete
        KBotDataColumn27.FormatString = Nothing
        KBotDataColumn27.HeaderText = ""
        KBotDataColumn27.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn27.Key = "sterge"
        KBotDataColumn27.MinWidth = 34
        KBotDataColumn27.OptionGroup = Nothing
        KBotDataColumn27.Resizable = False
        KBotDataColumn27.Width = 34
        gridNoi.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridNoi.BackColor = SystemColors.Window
        gridNoi.ColumnFillMode = KBotFillMode.SpecificColumn
        gridNoi.Columns.Add(KBotDataColumn2)
        gridNoi.Columns.Add(KBotDataColumn3)
        gridNoi.Columns.Add(KBotDataColumn4)
        gridNoi.Columns.Add(KBotDataColumn26)
        gridNoi.Columns.Add(KBotDataColumn5)
        gridNoi.Columns.Add(KBotDataColumn6)
        gridNoi.Columns.Add(KBotDataColumn27)
        gridNoi.Dock = DockStyle.Fill
        gridNoi.Enabled = False
        gridNoi.FillColumnKey = "numar"
        gridNoi.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridNoi.FooterVisible = True
        gridNoi.Margin = New Padding(0)
        gridNoi.Size = New Size(100, 100)
        gridNoi.Name = "gridNoi"
        gridNoi.TabIndex = 15
        '
        ' gridCurent
        '
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Document"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "numar"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Width = 200
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Data"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "data"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Width = 150
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Fel"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "fel"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Width = 150
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Valoare"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "valoare"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBotValueType.Number
        KBotDataColumn10.Width = 150
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.Aggregate = KBotAggregate.Sum
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Rest de plată"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "rest"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBotValueType.Number
        KBotDataColumn11.Width = 160
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.Aggregate = KBotAggregate.Sum
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "De plată"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.HeaderTooltip = "Cât se plătește acum din rest. Nu poate depăși restul de plată."
        KBotDataColumn12.Key = "platit"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBotValueType.Number
        KBotDataColumn12.Width = 160
        gridCurent.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridCurent.BackColor = SystemColors.Window
        gridCurent.ColumnFillMode = KBotFillMode.SpecificColumn
        gridCurent.Columns.Add(KBotDataColumn7)
        gridCurent.Columns.Add(KBotDataColumn8)
        gridCurent.Columns.Add(KBotDataColumn9)
        gridCurent.Columns.Add(KBotDataColumn10)
        gridCurent.Columns.Add(KBotDataColumn11)
        gridCurent.Columns.Add(KBotDataColumn12)
        gridCurent.Dock = DockStyle.Fill
        gridCurent.Enabled = False
        gridCurent.FillColumnKey = "numar"
        gridCurent.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridCurent.FooterVisible = True
        gridCurent.Margin = New Padding(0)
        gridCurent.Size = New Size(100, 100)
        gridCurent.Name = "gridCurent"
        gridCurent.TabIndex = 16
        gridCurent.Visible = False
        '
        ' gridAnterior
        '
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Document"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "numar"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Width = 200
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Data"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Key = "data"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Width = 150
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Fel"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "fel"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.ReadOnly = True
        KBotDataColumn15.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Width = 150
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.DecimalPlaces = 2
        KBotDataColumn16.Format = KBotFormat.Standard
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Valoare"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn16.Key = "valoare"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn16.ValueType = KBotValueType.Number
        KBotDataColumn16.Width = 150
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.Aggregate = KBotAggregate.Sum
        KBotDataColumn17.DecimalPlaces = 2
        KBotDataColumn17.Format = KBotFormat.Standard
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Rest de plată"
        KBotDataColumn17.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn17.Key = "rest"
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.ReadOnly = True
        KBotDataColumn17.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn17.ValueType = KBotValueType.Number
        KBotDataColumn17.Width = 160
        KBotDataColumn18.AggregateFormatString = Nothing
        KBotDataColumn18.Aggregate = KBotAggregate.Sum
        KBotDataColumn18.DecimalPlaces = 2
        KBotDataColumn18.Format = KBotFormat.Standard
        KBotDataColumn18.FormatString = Nothing
        KBotDataColumn18.HeaderText = "De plată"
        KBotDataColumn18.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn18.HeaderTooltip = "Cât se plătește acum din rest. Nu poate depăși restul de plată."
        KBotDataColumn18.Key = "platit"
        KBotDataColumn18.OptionGroup = Nothing
        KBotDataColumn18.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn18.ValueType = KBotValueType.Number
        KBotDataColumn18.Width = 160
        gridAnterior.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAnterior.BackColor = SystemColors.Window
        gridAnterior.ColumnFillMode = KBotFillMode.SpecificColumn
        gridAnterior.Columns.Add(KBotDataColumn13)
        gridAnterior.Columns.Add(KBotDataColumn14)
        gridAnterior.Columns.Add(KBotDataColumn15)
        gridAnterior.Columns.Add(KBotDataColumn16)
        gridAnterior.Columns.Add(KBotDataColumn17)
        gridAnterior.Columns.Add(KBotDataColumn18)
        gridAnterior.Dock = DockStyle.Fill
        gridAnterior.Enabled = False
        gridAnterior.FillColumnKey = "numar"
        gridAnterior.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridAnterior.FooterVisible = True
        gridAnterior.Margin = New Padding(0)
        gridAnterior.Size = New Size(100, 100)
        gridAnterior.Name = "gridAnterior"
        gridAnterior.TabIndex = 17
        gridAnterior.Visible = False
        '
        ' lstPlata
        '
        KBotDataColumn19.AggregateFormatString = Nothing
        KBotDataColumn19.FormatString = Nothing
        KBotDataColumn19.HeaderText = "Documente plată"
        KBotDataColumn19.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn19.Key = "nume"
        KBotDataColumn19.OptionGroup = Nothing
        KBotDataColumn19.ReadOnly = True
        KBotDataColumn19.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn19.Width = 300
        lstPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstPlata.BackColor = SystemColors.Window
        lstPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        lstPlata.Columns.Add(KBotDataColumn19)
        lstPlata.Dock = DockStyle.Fill
        lstPlata.Enabled = False
        lstPlata.FillColumnKey = "nume"
        lstPlata.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstPlata.Margin = New Padding(0, 0, 6, 6)
        lstPlata.ReadOnlyGrid = True
        lstPlata.Size = New Size(100, 100)
        lstPlata.Name = "lstPlata"
        lstPlata.TabIndex = 18
        '
        ' gridPlata
        '
        KBotDataColumn20.AggregateFormatString = Nothing
        KBotDataColumn20.FormatString = Nothing
        KBotDataColumn20.HeaderText = "Număr O.P."
        KBotDataColumn20.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.HeaderTooltip = "Numărul care ar urma acum este PROVIZORIU: numărul final se stabilește la salvare și poate fi altul, dacă între timp s-a emis un alt ordin. După salvare vezi numărul real."
        KBotDataColumn20.Key = "nrop"
        KBotDataColumn20.OptionGroup = Nothing
        KBotDataColumn20.ReadOnly = True
        KBotDataColumn20.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn20.Width = 150
        KBotDataColumn21.AggregateFormatString = Nothing
        KBotDataColumn21.FormatString = Nothing
        KBotDataColumn21.HeaderText = "Data O.P."
        KBotDataColumn21.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.HeaderTooltip = "Data plății și a ordinului de plată (în anul de lucru), zz.ll.aaaa."
        KBotDataColumn21.Key = "data"
        KBotDataColumn21.OptionGroup = Nothing
        KBotDataColumn21.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Width = 140
        KBotDataColumn22.AggregateFormatString = Nothing
        KBotDataColumn22.ColumnType = KBotColumnType.Combo
        KBotDataColumn22.CommitOnSelect = True
        KBotDataColumn22.FormatString = Nothing
        KBotDataColumn22.HeaderText = "Nota contabilă"
        KBotDataColumn22.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.HeaderTooltip = "Alegeți o notă existentă sau scrieți un număr nou. Fără alegere se folosește numărul următor."
        KBotDataColumn22.Key = "nota"
        KBotDataColumn22.OptionGroup = Nothing
        KBotDataColumn22.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Width = 150
        KBotDataColumn23.AggregateFormatString = Nothing
        KBotDataColumn23.FormatString = Nothing
        KBotDataColumn23.HeaderText = "Documente"
        KBotDataColumn23.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.Key = "documente"
        KBotDataColumn23.OptionGroup = Nothing
        KBotDataColumn23.ReadOnly = True
        KBotDataColumn23.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn23.Width = 300
        KBotDataColumn24.AggregateFormatString = Nothing
        KBotDataColumn24.FormatString = Nothing
        KBotDataColumn24.HeaderText = "Explicație O.P."
        KBotDataColumn24.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.HeaderTooltip = "Între 1 și 75 de caractere, fără diacritice și fără virgule (ANAF le refuză). Dacă o lăsați goală, se compune din numerele facturilor plătite și felul din șablon."
        KBotDataColumn24.Key = "explicatie"
        KBotDataColumn24.OptionGroup = Nothing
        KBotDataColumn24.TextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn24.Width = 420
        KBotDataColumn25.AggregateFormatString = Nothing
        KBotDataColumn25.DecimalPlaces = 2
        KBotDataColumn25.Format = KBotFormat.Standard
        KBotDataColumn25.FormatString = Nothing
        KBotDataColumn25.HeaderText = "Valoare"
        KBotDataColumn25.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Key = "valoare"
        KBotDataColumn25.OptionGroup = Nothing
        KBotDataColumn25.ReadOnly = True
        KBotDataColumn25.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn25.ValueType = KBotValueType.Number
        KBotDataColumn25.Width = 160
        KBotDataColumn26.AggregateFormatString = Nothing
        KBotDataColumn26.ColumnType = KBotColumnType.Combo
        KBotDataColumn26.CommitOnSelect = True
        KBotDataColumn26.FormatString = Nothing
        KBotDataColumn26.HeaderText = "Nota contabilă"
        KBotDataColumn26.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn26.HeaderTooltip = "Obligatorie: nota contabilă în care se înregistrează factura. Alegeți una existentă sau scrieți un număr nou."
        KBotDataColumn26.Key = "nota"
        KBotDataColumn26.OptionGroup = Nothing
        KBotDataColumn26.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn26.Width = 150
        KBotDataColumn28.AggregateFormatString = Nothing
        KBotDataColumn28.ColumnType = KBotColumnType.Button
        KBotDataColumn28.ButtonKind = KBotButtonKind.Delete
        KBotDataColumn28.FormatString = Nothing
        KBotDataColumn28.HeaderText = ""
        KBotDataColumn28.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn28.Key = "sterge"
        KBotDataColumn28.MinWidth = 34
        KBotDataColumn28.OptionGroup = Nothing
        KBotDataColumn28.Resizable = False
        KBotDataColumn28.Width = 34
        gridPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPlata.BackColor = SystemColors.Window
        gridPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        gridPlata.Columns.Add(KBotDataColumn20)
        gridPlata.Columns.Add(KBotDataColumn21)
        gridPlata.Columns.Add(KBotDataColumn22)
        gridPlata.Columns.Add(KBotDataColumn23)
        gridPlata.Columns.Add(KBotDataColumn24)
        gridPlata.Columns.Add(KBotDataColumn25)
        gridPlata.Columns.Add(KBotDataColumn28)
        gridPlata.Dock = DockStyle.Fill
        gridPlata.Enabled = False
        gridPlata.FillColumnKey = "explicatie"
        gridPlata.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridPlata.Margin = New Padding(0, 0, 0, 6)
        gridPlata.Size = New Size(100, 100)
        gridPlata.Name = "gridPlata"
        gridPlata.TabIndex = 19
        '
        ' tlyJos
        '
        tlyJos.ColumnCount = 3
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyJos.Dock = DockStyle.Fill
        tlyJos.Margin = New Padding(0)
        tlyJos.RowCount = 1
        tlyJos.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyJos.Size = New Size(100, 100)
        tlyJos.Controls.Add(lblStare, 0, 0)
        tlyJos.Controls.Add(btnRenunta, 1, 0)
        tlyJos.Controls.Add(btnSalveaza, 2, 0)
        tlyJos.Name = "tlyJos"
        tlyJos.TabIndex = 20
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
        lblStare.TabIndex = 21
        '
        ' btnRenunta
        '
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Enabled = False
        btnRenunta.Margin = New Padding(6, 4, 0, 4)
        btnRenunta.Size = New Size(100, 40)
        btnRenunta.Text = "Renunță"
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 24
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
        btnSalveaza.TabIndex = 25
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
        tlyClsf.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        tlyView.ResumeLayout(False)
        pnlView.ResumeLayout(False)
        tlyJos.ResumeLayout(False)
        CType(navFacturi, ComponentModel.ISupportInitialize).EndInit()
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).EndInit()
        CType(lstPlata, ComponentModel.ISupportInitialize).EndInit()
        CType(gridNoi, ComponentModel.ISupportInitialize).EndInit()
        CType(gridCurent, ComponentModel.ISupportInitialize).EndInit()
        CType(gridAnterior, ComponentModel.ISupportInitialize).EndInit()
        CType(gridPlata, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyPagina As KBotTableLayoutPanel
    Friend WithEvents tlyClsf As KBotTableLayoutPanel
    Friend WithEvents lblClsf As KBotLabel
    Friend WithEvents cmbClsf As KBotComboBox
    Friend WithEvents tlyAntet As KBotTableLayoutPanel
    Friend WithEvents lblPartener As KBotLabel
    Friend WithEvents lblCodClient As KBotLabel
    Friend WithEvents lblIban As KBotLabel
    Friend WithEvents cmbPartener As KBotComboBox
    Friend WithEvents txtCodClient As KBotTextBox
    Friend WithEvents txtIban As KBotTextBox
    Friend WithEvents lstEvidentiere As KBotDataView
    Friend WithEvents tlyView As KBotTableLayoutPanel
    Friend WithEvents navFacturi As KBotNavList
    Friend WithEvents pnlView As Panel
    Friend WithEvents gridNoi As KBotDataView
    Friend WithEvents gridCurent As KBotDataView
    Friend WithEvents gridAnterior As KBotDataView
    Friend WithEvents lstPlata As KBotDataView
    Friend WithEvents gridPlata As KBotDataView
    Friend WithEvents tlyJos As KBotTableLayoutPanel
    Friend WithEvents lblStare As KBotLabel
    Friend WithEvents btnRenunta As KBotButton
    Friend WithEvents btnSalveaza As KBotButton
End Class
