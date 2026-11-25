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
        Dim KBotDataColumn29 As KBotDataColumn = New KBotDataColumn()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem5 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem6 As KBotNavItem = New KBotNavItem()
        Dim KBotDataColumn22 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn23 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn24 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn25 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn26 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn27 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn28 As KBotDataColumn = New KBotDataColumn()
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn21 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn30 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn31 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn32 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn33 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn34 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        cmbClsf = New KBotComboBox()
        cmbPartener = New KBotComboBox()
        txtCodClient = New KBotTextBox()
        txtIban = New KBotTextBox()
        btnRenunta = New KBotButton()
        btnSalveaza = New KBotButton()
        tlyPagina = New KBotTableLayoutPanel()
        tlyClsf = New KBotTableLayoutPanel()
        lblClsf = New KBotLabel()
        tlyAntet = New KBotTableLayoutPanel()
        lblPartener = New KBotLabel()
        lblCodClient = New KBotLabel()
        lblIban = New KBotLabel()
        lstEvidentiere = New KBotDataView()
        tlyView = New KBotTableLayoutPanel()
        pnlNav = New Panel()
        navFacturi = New KBotNavList()
        pnlView = New Panel()
        gridNoi = New KBotDataView()
        gridCurent = New KBotDataView()
        gridAnterior = New KBotDataView()
        lstPlata = New KBotDataView()
        gridPlata = New KBotDataView()
        tlyJos = New KBotTableLayoutPanel()
        lblStare = New KBotLabel()
        tlyPagina.SuspendLayout()
        tlyClsf.SuspendLayout()
        tlyAntet.SuspendLayout()
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).BeginInit()
        tlyView.SuspendLayout()
        pnlNav.SuspendLayout()
        CType(navFacturi, ComponentModel.ISupportInitialize).BeginInit()
        pnlView.SuspendLayout()
        CType(gridNoi, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridCurent, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridAnterior, ComponentModel.ISupportInitialize).BeginInit()
        CType(lstPlata, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPlata, ComponentModel.ISupportInitialize).BeginInit()
        tlyJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' cmbClsf
        ' 
        cmbClsf.Dock = DockStyle.Fill
        cmbClsf.Enabled = False
        cmbClsf.FindAsYouType = True
        cmbClsf.FindInItemOrder = True
        cmbClsf.FitDropDownToWorkingArea = True
        cmbClsf.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbClsf.Location = New Point(4, 36)
        cmbClsf.Margin = New Padding(4, 0, 4, 0)
        cmbClsf.MaxDropDownItems = 15
        cmbClsf.Name = "cmbClsf"
        cmbClsf.PlaceholderText = "Alegeți clasificația"
        cmbClsf.Size = New Size(392, 40)
        cmbClsf.TabIndex = 3
        tips.SetToolTipHeader(cmbClsf, "Clasificația bugetară")
        tips.SetToolTipText(cmbClsf, "Sunt disponibile clasificațiile cu șabloane de operații în subunitatea în lucru.")
        ' 
        ' cmbPartener
        ' 
        cmbPartener.Dock = DockStyle.Fill
        cmbPartener.Editable = True
        cmbPartener.Enabled = False
        cmbPartener.FindAsYouType = True
        cmbPartener.FindInItemOrder = True
        cmbPartener.FitDropDownToWorkingArea = True
        cmbPartener.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        cmbPartener.Location = New Point(4, 36)
        cmbPartener.Margin = New Padding(4, 0, 4, 0)
        cmbPartener.MaxDropDownItems = 15
        cmbPartener.Name = "cmbPartener"
        cmbPartener.PlaceholderText = "Alegeți partenerul"
        cmbPartener.Size = New Size(517, 40)
        cmbPartener.TabIndex = 8
        tips.SetToolTipHeader(cmbPartener, "Partenerul")
        tips.SetToolTipText(cmbPartener, "Furnizorul căruia îi înregistrați facturi sau îi plătiți. Se pot căuta cuvinte din denumire sau codul.")
        ' 
        ' txtCodClient
        ' 
        txtCodClient.Dock = DockStyle.Fill
        txtCodClient.Enabled = False
        txtCodClient.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtCodClient.HorizontalScrollBarPadding = New Padding(0)
        txtCodClient.Location = New Point(529, 36)
        txtCodClient.Margin = New Padding(4, 0, 4, 0)
        txtCodClient.Multiline = False
        txtCodClient.Name = "txtCodClient"
        txtCodClient.ScrollBars = ScrollBars.None
        txtCodClient.Size = New Size(232, 40)
        txtCodClient.TabIndex = 9
        txtCodClient.TextAlign = HorizontalAlignment.Center
        txtCodClient.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtCodClient, "Cod client")
        tips.SetToolTipText(txtCodClient, "Codul partenerului, din configurarea partenerului.")
        txtCodClient.VerticalScrollBarPadding = New Padding(0)
        ' 
        ' txtIban
        ' 
        txtIban.Dock = DockStyle.Fill
        txtIban.Enabled = False
        txtIban.Font = New Font("Calibri", 11F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtIban.HorizontalScrollBarPadding = New Padding(0)
        txtIban.Location = New Point(769, 36)
        txtIban.Margin = New Padding(4, 0, 4, 0)
        txtIban.Multiline = False
        txtIban.Name = "txtIban"
        txtIban.ScrollBars = ScrollBars.None
        txtIban.Size = New Size(512, 40)
        txtIban.TabIndex = 10
        txtIban.TextAlign = HorizontalAlignment.Center
        txtIban.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtIban, "Cod bancar")
        tips.SetToolTipText(txtIban, "Contul bancar al partenerului pentru clasificația aleasă (din configurarea partenerului).")
        txtIban.VerticalScrollBarPadding = New Padding(0)
        ' 
        ' btnRenunta
        ' 
        btnRenunta.AccessibleRole = AccessibleRole.PushButton
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Enabled = False
        btnRenunta.Location = New Point(1291, 4)
        btnRenunta.Margin = New Padding(6, 4, 0, 4)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(194, 57)
        btnRenunta.TabIndex = 24
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Renunță la ce ați completat și reia datele partenerului.")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AccessibleRole = AccessibleRole.PushButton
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Enabled = False
        btnSalveaza.Location = New Point(1491, 4)
        btnSalveaza.Margin = New Padding(6, 4, 0, 4)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Primary = True
        btnSalveaza.Size = New Size(194, 57)
        btnSalveaza.TabIndex = 25
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Înregistrează facturile noi și/sau plata cu ordinul ei.")
        ' 
        ' tlyPagina
        ' 
        tlyPagina.ColumnCount = 2
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 400F))
        tlyPagina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPagina.Controls.Add(tlyClsf, 0, 0)
        tlyPagina.Controls.Add(tlyAntet, 1, 0)
        tlyPagina.Controls.Add(lstEvidentiere, 0, 1)
        tlyPagina.Controls.Add(tlyView, 1, 1)
        tlyPagina.Controls.Add(lstPlata, 0, 2)
        tlyPagina.Controls.Add(gridPlata, 1, 2)
        tlyPagina.Controls.Add(tlyJos, 0, 3)
        tlyPagina.Dock = DockStyle.Fill
        tlyPagina.Location = New Point(0, 0)
        tlyPagina.Margin = New Padding(0)
        tlyPagina.Name = "tlyPagina"
        tlyPagina.RowCount = 4
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 84F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 62F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Percent, 38F))
        tlyPagina.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyPagina.Size = New Size(1685, 900)
        tlyPagina.TabIndex = 0
        ' 
        ' tlyClsf
        ' 
        tlyClsf.ColumnCount = 1
        tlyClsf.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyClsf.Controls.Add(lblClsf, 0, 0)
        tlyClsf.Controls.Add(cmbClsf, 0, 1)
        tlyClsf.Dock = DockStyle.Fill
        tlyClsf.Location = New Point(0, 0)
        tlyClsf.Margin = New Padding(0)
        tlyClsf.Name = "tlyClsf"
        tlyClsf.RowCount = 2
        tlyClsf.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyClsf.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyClsf.Size = New Size(400, 84)
        tlyClsf.TabIndex = 1
        ' 
        ' lblClsf
        ' 
        lblClsf.BorderWidth = 0
        lblClsf.Dock = DockStyle.Fill
        lblClsf.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblClsf.Location = New Point(0, 0)
        lblClsf.Margin = New Padding(0, 0, 6, 3)
        lblClsf.Name = "lblClsf"
        lblClsf.Size = New Size(394, 33)
        lblClsf.TabIndex = 2
        lblClsf.Text = "Clasificația"
        lblClsf.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' tlyAntet
        ' 
        tlyAntet.AutoFitToTheme = False
        tlyAntet.ColumnCount = 3
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyAntet.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 520F))
        tlyAntet.Controls.Add(lblPartener, 0, 0)
        tlyAntet.Controls.Add(lblCodClient, 1, 0)
        tlyAntet.Controls.Add(lblIban, 2, 0)
        tlyAntet.Controls.Add(cmbPartener, 0, 1)
        tlyAntet.Controls.Add(txtCodClient, 1, 1)
        tlyAntet.Controls.Add(txtIban, 2, 1)
        tlyAntet.Dock = DockStyle.Fill
        tlyAntet.Location = New Point(400, 0)
        tlyAntet.Margin = New Padding(0)
        tlyAntet.Name = "tlyAntet"
        tlyAntet.RowCount = 3
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyAntet.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyAntet.ScaleAbsoluteStyles = False
        tlyAntet.Size = New Size(1285, 84)
        tlyAntet.TabIndex = 4
        ' 
        ' lblPartener
        ' 
        lblPartener.BorderWidth = 0
        lblPartener.Dock = DockStyle.Fill
        lblPartener.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblPartener.Location = New Point(0, 0)
        lblPartener.Margin = New Padding(0, 0, 6, 3)
        lblPartener.Name = "lblPartener"
        lblPartener.Size = New Size(519, 33)
        lblPartener.TabIndex = 5
        lblPartener.Text = "Partenerul"
        lblPartener.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblCodClient
        ' 
        lblCodClient.BorderWidth = 0
        lblCodClient.Dock = DockStyle.Fill
        lblCodClient.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblCodClient.Location = New Point(525, 0)
        lblCodClient.Margin = New Padding(0, 0, 6, 3)
        lblCodClient.Name = "lblCodClient"
        lblCodClient.Size = New Size(234, 33)
        lblCodClient.TabIndex = 6
        lblCodClient.Text = "Cod client"
        lblCodClient.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblIban
        ' 
        lblIban.BorderWidth = 0
        lblIban.Dock = DockStyle.Fill
        lblIban.Font = New Font("Calibri", 9F, FontStyle.Bold)
        lblIban.Location = New Point(765, 0)
        lblIban.Margin = New Padding(0, 0, 0, 3)
        lblIban.Name = "lblIban"
        lblIban.Size = New Size(520, 33)
        lblIban.TabIndex = 7
        lblIban.Text = "Cod bancar"
        lblIban.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lstEvidentiere
        ' 
        lstEvidentiere.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstEvidentiere.BackColor = SystemColors.Window
        lstEvidentiere.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn29.AggregateFormatString = Nothing
        KBotDataColumn29.FormatString = Nothing
        KBotDataColumn29.HeaderText = "Documente evidențiere"
        KBotDataColumn29.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn29.Key = "nume"
        KBotDataColumn29.OptionGroup = Nothing
        KBotDataColumn29.ReadOnly = True
        KBotDataColumn29.Width = 300
        lstEvidentiere.Columns.Add(KBotDataColumn29)
        lstEvidentiere.Dock = DockStyle.Fill
        lstEvidentiere.Enabled = False
        lstEvidentiere.FillColumnKey = "nume"
        lstEvidentiere.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstEvidentiere.Location = New Point(0, 90)
        lstEvidentiere.Margin = New Padding(0, 6, 6, 6)
        lstEvidentiere.Name = "lstEvidentiere"
        lstEvidentiere.ReadOnlyGrid = True
        lstEvidentiere.Size = New Size(394, 454)
        lstEvidentiere.TabIndex = 11
        ' 
        ' tlyView
        ' 
        tlyView.ColumnCount = 1
        tlyView.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyView.Controls.Add(pnlNav, 0, 0)
        tlyView.Controls.Add(pnlView, 0, 1)
        tlyView.Dock = DockStyle.Fill
        tlyView.Location = New Point(400, 84)
        tlyView.Margin = New Padding(0, 0, 0, 6)
        tlyView.Name = "tlyView"
        tlyView.RowCount = 2
        tlyView.RowStyles.Add(New RowStyle(SizeType.Absolute, 51F))
        tlyView.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyView.Size = New Size(1285, 466)
        tlyView.TabIndex = 12
        ' 
        ' pnlNav (the frame of the bar: its colour is set in ApplyTheme; same height as the header of the grids)
        ' 
        pnlNav.Controls.Add(navFacturi)
        pnlNav.Dock = DockStyle.Fill
        pnlNav.Margin = New Padding(0, 6, 0, 0)
        pnlNav.Name = "pnlNav"
        pnlNav.Padding = New Padding(2, 2, 2, 0)
        pnlNav.Size = New Size(1285, 45)
        pnlNav.TabIndex = 13
        ' 
        ' navFacturi
        ' 
        navFacturi.Dock = DockStyle.Fill
        navFacturi.FooterBorderVisible = False
        navFacturi.IconSize = 16
        navFacturi.ItemCornerRadius = 2
        navFacturi.ItemPadding = New Padding(3)
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.add_fact
        KBotNavItem4.Key = "noi"
        KBotNavItem4.Text = "Facturi noi"
        KBotNavItem5.AutoSize = True
        KBotNavItem5.Image = My.Resources.Resources.invoice
        KBotNavItem5.Key = "curent"
        KBotNavItem5.Text = "Facturi an curent"
        KBotNavItem6.AutoSize = True
        KBotNavItem6.Image = My.Resources.Resources.istoric_32
        KBotNavItem6.Key = "anterior"
        KBotNavItem6.Text = "Facturi an anterior"
        navFacturi.Items.Add(KBotNavItem4)
        navFacturi.Items.Add(KBotNavItem5)
        navFacturi.Items.Add(KBotNavItem6)
        navFacturi.Location = New Point(2, 2)
        navFacturi.Margin = New Padding(0)
        navFacturi.Name = "navFacturi"
        navFacturi.Orientation = KBotNavOrientation.Horizontal
        navFacturi.SelectedKey = "noi"
        navFacturi.Size = New Size(1281, 43)
        navFacturi.TabIndex = 0
        ' 
        ' pnlView
        ' 
        pnlView.Controls.Add(gridNoi)
        pnlView.Controls.Add(gridCurent)
        pnlView.Controls.Add(gridAnterior)
        pnlView.Dock = DockStyle.Fill
        pnlView.Location = New Point(0, 44)
        pnlView.Margin = New Padding(0)
        pnlView.Name = "pnlView"
        pnlView.Size = New Size(1285, 422)
        pnlView.TabIndex = 14
        ' 
        ' gridNoi
        ' 
        gridNoi.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridNoi.BackColor = SystemColors.Window
        gridNoi.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn22.AggregateFormatString = Nothing
        KBotDataColumn22.FormatString = Nothing
        KBotDataColumn22.HeaderText = "Număr factură"
        KBotDataColumn22.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Key = "numar"
        KBotDataColumn22.OptionGroup = Nothing
        KBotDataColumn22.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn22.Width = 200
        KBotDataColumn23.AggregateFormatString = Nothing
        KBotDataColumn23.FormatString = "dd.MM.yyyy"
        KBotDataColumn23.HeaderText = "Data"
        KBotDataColumn23.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.Key = "data"
        KBotDataColumn23.OptionGroup = Nothing
        KBotDataColumn23.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn23.ValueType = KBotValueType.DateTime
        KBotDataColumn23.Width = 150
        KBotDataColumn24.AggregateFormatString = Nothing
        KBotDataColumn24.ColumnType = KBotColumnType.Combo
        KBotDataColumn24.CommitOnSelect = True
        KBotDataColumn24.FormatString = Nothing
        KBotDataColumn24.HeaderText = "Fel"
        KBotDataColumn24.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.Key = "fel"
        KBotDataColumn24.OptionGroup = Nothing
        KBotDataColumn24.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn24.Width = 150
        KBotDataColumn25.AggregateFormatString = Nothing
        KBotDataColumn25.ColumnType = KBotColumnType.Combo
        KBotDataColumn25.CommitOnSelect = True
        KBotDataColumn25.FormatString = Nothing
        KBotDataColumn25.HeaderText = "Nota contabilă"
        KBotDataColumn25.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.HeaderTooltip = "Obligatorie: nota contabilă în care se înregistrează factura. Alegeți una existentă sau scrieți un număr nou."
        KBotDataColumn25.Key = "nota"
        KBotDataColumn25.OptionGroup = Nothing
        KBotDataColumn25.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn25.Width = 150
        KBotDataColumn26.Aggregate = KBotAggregate.Sum
        KBotDataColumn26.AggregateFormatString = Nothing
        KBotDataColumn26.DecimalPlaces = 2
        KBotDataColumn26.Format = KBotFormat.Standard
        KBotDataColumn26.FormatString = Nothing
        KBotDataColumn26.HeaderText = "Valoare"
        KBotDataColumn26.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn26.Key = "suma"
        KBotDataColumn26.OptionGroup = Nothing
        KBotDataColumn26.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn26.ValueType = KBotValueType.Number
        KBotDataColumn26.Width = 160
        KBotDataColumn27.Aggregate = KBotAggregate.Sum
        KBotDataColumn27.AggregateFormatString = Nothing
        KBotDataColumn27.DecimalPlaces = 2
        KBotDataColumn27.Format = KBotFormat.Standard
        KBotDataColumn27.FormatString = Nothing
        KBotDataColumn27.HeaderText = "De plată"
        KBotDataColumn27.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn27.HeaderTooltip = "Cât se plătește acum din factură (0 = nimic)."
        KBotDataColumn27.Key = "platit"
        KBotDataColumn27.OptionGroup = Nothing
        KBotDataColumn27.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn27.ValueType = KBotValueType.Number
        KBotDataColumn27.Width = 160
        KBotDataColumn28.AggregateFormatString = Nothing
        KBotDataColumn28.ButtonKind = KBotButtonKind.Delete
        KBotDataColumn28.ColumnType = KBotColumnType.Button
        KBotDataColumn28.FormatString = Nothing
        KBotDataColumn28.HeaderText = ""
        KBotDataColumn28.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn28.Key = "sterge"
        KBotDataColumn28.MinWidth = 34
        KBotDataColumn28.OptionGroup = Nothing
        KBotDataColumn28.Resizable = False
        KBotDataColumn28.Width = 34
        gridNoi.Columns.Add(KBotDataColumn25)
        gridNoi.Columns.Add(KBotDataColumn22)
        gridNoi.Columns.Add(KBotDataColumn23)
        gridNoi.Columns.Add(KBotDataColumn24)
        gridNoi.Columns.Add(KBotDataColumn26)
        gridNoi.Columns.Add(KBotDataColumn27)
        gridNoi.Columns.Add(KBotDataColumn28)
        gridNoi.Dock = DockStyle.Fill
        gridNoi.Enabled = False
        gridNoi.FillColumnKey = "numar"
        gridNoi.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridNoi.FooterVisible = True
        gridNoi.Location = New Point(0, 0)
        gridNoi.Margin = New Padding(0)
        gridNoi.Name = "gridNoi"
        gridNoi.Size = New Size(1285, 422)
        gridNoi.TabIndex = 15
        ' 
        ' gridCurent
        ' 
        gridCurent.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridCurent.BackColor = SystemColors.Window
        gridCurent.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Document"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "numar"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 200
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Data"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "data"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 150
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Fel"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "fel"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Valoare"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "valoare"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 150
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Rest de plată"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "rest"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 160
        KBotDataColumn7.Aggregate = KBotAggregate.Sum
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "De plată"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.HeaderTooltip = "Cât se plătește acum din rest. Nu poate depăși restul de plată."
        KBotDataColumn7.Key = "platit"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 160
        gridCurent.Columns.Add(KBotDataColumn2)
        gridCurent.Columns.Add(KBotDataColumn3)
        gridCurent.Columns.Add(KBotDataColumn4)
        gridCurent.Columns.Add(KBotDataColumn5)
        gridCurent.Columns.Add(KBotDataColumn6)
        gridCurent.Columns.Add(KBotDataColumn7)
        gridCurent.Dock = DockStyle.Fill
        gridCurent.Enabled = False
        gridCurent.FillColumnKey = "numar"
        gridCurent.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridCurent.FooterVisible = True
        gridCurent.Location = New Point(0, 0)
        gridCurent.Margin = New Padding(0)
        gridCurent.Name = "gridCurent"
        gridCurent.Size = New Size(1285, 422)
        gridCurent.TabIndex = 16
        gridCurent.Visible = False
        ' 
        ' gridAnterior
        ' 
        gridAnterior.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAnterior.BackColor = SystemColors.Window
        gridAnterior.ColumnFillMode = KBotFillMode.SpecificColumn
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
        KBotDataColumn12.Aggregate = KBotAggregate.Sum
        KBotDataColumn12.AggregateFormatString = Nothing
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
        KBotDataColumn13.Aggregate = KBotAggregate.Sum
        KBotDataColumn13.AggregateFormatString = Nothing
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
        gridAnterior.Columns.Add(KBotDataColumn8)
        gridAnterior.Columns.Add(KBotDataColumn9)
        gridAnterior.Columns.Add(KBotDataColumn10)
        gridAnterior.Columns.Add(KBotDataColumn11)
        gridAnterior.Columns.Add(KBotDataColumn12)
        gridAnterior.Columns.Add(KBotDataColumn13)
        gridAnterior.Dock = DockStyle.Fill
        gridAnterior.Enabled = False
        gridAnterior.FillColumnKey = "numar"
        gridAnterior.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridAnterior.FooterVisible = True
        gridAnterior.Location = New Point(0, 0)
        gridAnterior.Margin = New Padding(0)
        gridAnterior.Name = "gridAnterior"
        gridAnterior.Size = New Size(1285, 422)
        gridAnterior.TabIndex = 17
        gridAnterior.Visible = False
        ' 
        ' lstPlata
        ' 
        lstPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        lstPlata.BackColor = SystemColors.Window
        lstPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Documente plată"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 300
        lstPlata.Columns.Add(KBotDataColumn1)
        lstPlata.Dock = DockStyle.Fill
        lstPlata.Enabled = False
        lstPlata.FillColumnKey = "nume"
        lstPlata.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstPlata.Location = New Point(0, 550)
        lstPlata.Margin = New Padding(0, 0, 6, 6)
        lstPlata.Name = "lstPlata"
        lstPlata.ReadOnlyGrid = True
        lstPlata.Size = New Size(394, 279)
        lstPlata.TabIndex = 18
        ' 
        ' gridPlata
        ' 
        gridPlata.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPlata.BackColor = SystemColors.Window
        gridPlata.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn21.AggregateFormatString = Nothing
        KBotDataColumn21.FormatString = Nothing
        KBotDataColumn21.HeaderText = "Număr O.P."
        KBotDataColumn21.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.HeaderTooltip = "Numărul care ar urma acum este PROVIZORIU: numărul final se stabilește la salvare și poate fi altul, dacă între timp s-a emis un alt ordin. După salvare vezi numărul real."
        KBotDataColumn21.Key = "nrop"
        KBotDataColumn21.OptionGroup = Nothing
        KBotDataColumn21.ReadOnly = True
        KBotDataColumn21.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn21.Width = 150
        KBotDataColumn30.AggregateFormatString = Nothing
        KBotDataColumn30.FormatString = "dd.MM.yyyy"
        KBotDataColumn30.HeaderText = "Data O.P."
        KBotDataColumn30.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn30.HeaderTooltip = "Data plății și a ordinului de plată (în anul de lucru), zz.ll.aaaa."
        KBotDataColumn30.Key = "data"
        KBotDataColumn30.OptionGroup = Nothing
        KBotDataColumn30.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn30.ValueType = KBotValueType.DateTime
        KBotDataColumn30.Width = 140
        KBotDataColumn31.AggregateFormatString = Nothing
        KBotDataColumn31.ColumnType = KBotColumnType.Combo
        KBotDataColumn31.CommitOnSelect = True
        KBotDataColumn31.FormatString = Nothing
        KBotDataColumn31.HeaderText = "Nota contabilă"
        KBotDataColumn31.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn31.HeaderTooltip = "Alegeți o notă existentă sau scrieți un număr nou. Fără alegere se folosește numărul următor."
        KBotDataColumn31.Key = "nota"
        KBotDataColumn31.OptionGroup = Nothing
        KBotDataColumn31.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn31.Width = 150
        KBotDataColumn32.AggregateFormatString = Nothing
        KBotDataColumn32.FormatString = Nothing
        KBotDataColumn32.HeaderText = "Documente"
        KBotDataColumn32.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn32.Key = "documente"
        KBotDataColumn32.OptionGroup = Nothing
        KBotDataColumn32.ReadOnly = True
        KBotDataColumn32.Width = 300
        KBotDataColumn33.AggregateFormatString = Nothing
        KBotDataColumn33.FormatString = Nothing
        KBotDataColumn33.HeaderText = "Explicație O.P."
        KBotDataColumn33.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn33.HeaderTooltip = "Între 1 și 75 de caractere, fără diacritice și fără virgule (ANAF le refuză). Dacă o lăsați goală, se compune din numerele facturilor plătite și felul din șablon."
        KBotDataColumn33.Key = "explicatie"
        KBotDataColumn33.OptionGroup = Nothing
        KBotDataColumn33.Width = 420
        KBotDataColumn34.AggregateFormatString = Nothing
        KBotDataColumn34.DecimalPlaces = 2
        KBotDataColumn34.Format = KBotFormat.Standard
        KBotDataColumn34.FormatString = Nothing
        KBotDataColumn34.HeaderText = "Valoare"
        KBotDataColumn34.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn34.Key = "valoare"
        KBotDataColumn34.OptionGroup = Nothing
        KBotDataColumn34.ReadOnly = True
        KBotDataColumn34.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn34.ValueType = KBotValueType.Number
        KBotDataColumn34.Width = 160
        gridPlata.Columns.Add(KBotDataColumn31)
        gridPlata.Columns.Add(KBotDataColumn21)
        gridPlata.Columns.Add(KBotDataColumn30)
        gridPlata.Columns.Add(KBotDataColumn32)
        gridPlata.Columns.Add(KBotDataColumn33)
        gridPlata.Columns.Add(KBotDataColumn34)
        gridPlata.Dock = DockStyle.Fill
        gridPlata.Enabled = False
        gridPlata.FillColumnKey = "explicatie"
        gridPlata.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        gridPlata.Location = New Point(400, 550)
        gridPlata.Margin = New Padding(0, 0, 0, 6)
        gridPlata.Name = "gridPlata"
        gridPlata.Size = New Size(1285, 279)
        gridPlata.TabIndex = 19
        ' 
        ' tlyJos
        ' 
        tlyJos.ColumnCount = 3
        tlyPagina.SetColumnSpan(tlyJos, 2)
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyJos.Controls.Add(lblStare, 0, 0)
        tlyJos.Controls.Add(btnRenunta, 1, 0)
        tlyJos.Controls.Add(btnSalveaza, 2, 0)
        tlyJos.Dock = DockStyle.Fill
        tlyJos.Location = New Point(0, 835)
        tlyJos.Margin = New Padding(0)
        tlyJos.Name = "tlyJos"
        tlyJos.RowCount = 1
        tlyJos.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyJos.Size = New Size(1685, 65)
        tlyJos.TabIndex = 20
        ' 
        ' lblStare
        ' 
        lblStare.BorderWidth = 0
        lblStare.Dock = DockStyle.Fill
        lblStare.Font = New Font("Calibri", 9F)
        lblStare.Location = New Point(0, 0)
        lblStare.Margin = New Padding(0, 0, 8, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1277, 65)
        lblStare.TabIndex = 21
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' FacturiPlatiPage
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyPagina)
        Margin = New Padding(4)
        Name = "FacturiPlatiPage"
        Size = New Size(1685, 900)
        tlyPagina.ResumeLayout(False)
        tlyClsf.ResumeLayout(False)
        tlyAntet.ResumeLayout(False)
        CType(lstEvidentiere, ComponentModel.ISupportInitialize).EndInit()
        tlyView.ResumeLayout(False)
        pnlNav.ResumeLayout(False)
        CType(navFacturi, ComponentModel.ISupportInitialize).EndInit()
        pnlView.ResumeLayout(False)
        CType(gridNoi, ComponentModel.ISupportInitialize).EndInit()
        CType(gridCurent, ComponentModel.ISupportInitialize).EndInit()
        CType(gridAnterior, ComponentModel.ISupportInitialize).EndInit()
        CType(lstPlata, ComponentModel.ISupportInitialize).EndInit()
        CType(gridPlata, ComponentModel.ISupportInitialize).EndInit()
        tlyJos.ResumeLayout(False)
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
    Friend WithEvents pnlNav As Panel
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
