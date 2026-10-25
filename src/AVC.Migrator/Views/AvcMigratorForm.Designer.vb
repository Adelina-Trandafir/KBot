<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AvcMigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel

    ' --- unit (registry, DC, units) ---
    Friend WithEvents grpUnitate As System.Windows.Forms.GroupBox
    Friend WithEvents pnlUnitateSus As System.Windows.Forms.Panel
    Friend WithEvents tlpUnitateSus As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblRegistru As System.Windows.Forms.Label
    Friend WithEvents txtRegistru As KBot.Controls.KBotTextField
    Friend WithEvents btnRasfoireRegistru As System.Windows.Forms.Button
    Friend WithEvents lblDc As System.Windows.Forms.Label
    Friend WithEvents cboDc As KBot.Controls.KBotComboBox
    Friend WithEvents lblCodFiscal As System.Windows.Forms.Label
    Friend WithEvents txtCodFiscal As KBot.Controls.KBotTextField
    Friend WithEvents dgvUnitati As KBot.Controls.KBotDataView

    ' --- action ---
    Friend WithEvents tlpActiune As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents btnCiteste As System.Windows.Forms.Button
    Friend WithEvents btnVerifica As System.Windows.Forms.Button
    Friend WithEvents btnMigreaza As System.Windows.Forms.Button

    ' --- steps and findings ---
    Friend WithEvents tlpJos As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents grpPasi As System.Windows.Forms.GroupBox
    Friend WithEvents dgvPasi As KBot.Controls.KBotDataView
    Friend WithEvents grpConstatari As System.Windows.Forms.GroupBox
    Friend WithEvents rtbConstatari As System.Windows.Forms.RichTextBox

    ' --- log ---
    Friend WithEvents grpLog As System.Windows.Forms.GroupBox
    Friend WithEvents rtbLog As System.Windows.Forms.RichTextBox
    Friend WithEvents lblStare As System.Windows.Forms.Label

    Friend WithEvents tipAvc As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn9 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn10 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AvcMigratorForm))
        Dim KBotDataColumn11 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn12 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn13 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn14 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn15 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn16 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn17 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        tlpRoot = New KBot.Controls.KBotTableLayoutPanel()
        grpUnitate = New GroupBox()
        pnlUnitateSus = New Panel()
        KBotTableLayoutPanel1 = New KBot.Controls.KBotTableLayoutPanel()
        dgvUnitati = New KBot.Controls.KBotDataView()
        tlpUnitateSus = New KBot.Controls.KBotTableLayoutPanel()
        BtnCitesteRegistru = New Button()
        lblRegistru = New Label()
        btnCiteste = New Button()
        txtRegistru = New KBot.Controls.KBotTextField()
        btnRasfoireRegistru = New Button()
        lblDc = New Label()
        cboDc = New KBot.Controls.KBotComboBox()
        lblCodFiscal = New Label()
        txtCodFiscal = New KBot.Controls.KBotTextField()
        tlpActiune = New KBot.Controls.KBotTableLayoutPanel()
        btnVerifica = New Button()
        btnMigreaza = New Button()
        btnStop = New Button()
        tlpJos = New KBot.Controls.KBotTableLayoutPanel()
        grpPasi = New GroupBox()
        dgvPasi = New KBot.Controls.KBotDataView()
        grpConstatari = New GroupBox()
        rtbConstatari = New RichTextBox()
        grpLog = New GroupBox()
        rtbLog = New RichTextBox()
        lblStare = New Label()
        tipAvc = New KBot.Controls.KBotToolTip(components)
        BtnRaportHTML = New Button()
        tlpRoot.SuspendLayout()
        grpUnitate.SuspendLayout()
        pnlUnitateSus.SuspendLayout()
        CType(dgvUnitati, ComponentModel.ISupportInitialize).BeginInit()
        tlpUnitateSus.SuspendLayout()
        tlpActiune.SuspendLayout()
        tlpJos.SuspendLayout()
        grpPasi.SuspendLayout()
        CType(dgvPasi, ComponentModel.ISupportInitialize).BeginInit()
        grpConstatari.SuspendLayout()
        grpLog.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlpRoot
        ' 
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpRoot.Controls.Add(grpUnitate, 0, 0)
        tlpRoot.Controls.Add(tlpActiune, 0, 1)
        tlpRoot.Controls.Add(tlpJos, 0, 2)
        tlpRoot.Controls.Add(grpLog, 0, 3)
        tlpRoot.Controls.Add(lblStare, 0, 4)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Margin = New Padding(4)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 5
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 300F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 250F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tlpRoot.Size = New Size(1617, 966)
        tlpRoot.TabIndex = 0
        ' 
        ' grpUnitate
        ' 
        grpUnitate.Controls.Add(pnlUnitateSus)
        grpUnitate.Dock = DockStyle.Fill
        grpUnitate.Location = New Point(12, 12)
        grpUnitate.Margin = New Padding(4)
        grpUnitate.Name = "grpUnitate"
        grpUnitate.Padding = New Padding(10)
        grpUnitate.Size = New Size(1593, 292)
        grpUnitate.TabIndex = 0
        grpUnitate.TabStop = False
        grpUnitate.Text = "Unitate"
        ' 
        ' pnlUnitateSus
        ' 
        pnlUnitateSus.Controls.Add(KBotTableLayoutPanel1)
        pnlUnitateSus.Controls.Add(dgvUnitati)
        pnlUnitateSus.Controls.Add(tlpUnitateSus)
        pnlUnitateSus.Dock = DockStyle.Fill
        pnlUnitateSus.Location = New Point(10, 32)
        pnlUnitateSus.Margin = New Padding(0)
        pnlUnitateSus.Name = "pnlUnitateSus"
        pnlUnitateSus.Size = New Size(1573, 250)
        pnlUnitateSus.TabIndex = 0
        ' 
        ' KBotTableLayoutPanel1
        ' 
        KBotTableLayoutPanel1.ColumnCount = 1
        KBotTableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        KBotTableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 20F))
        KBotTableLayoutPanel1.Dock = DockStyle.Top
        KBotTableLayoutPanel1.Location = New Point(0, 40)
        KBotTableLayoutPanel1.Name = "KBotTableLayoutPanel1"
        KBotTableLayoutPanel1.RowCount = 1
        KBotTableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        KBotTableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        KBotTableLayoutPanel1.Size = New Size(1573, 10)
        KBotTableLayoutPanel1.TabIndex = 5
        ' 
        ' dgvUnitati
        ' 
        dgvUnitati.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvUnitati.BackColor = SystemColors.Window
        dgvUnitati.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Transferă"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "bifa"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "IdUnitate"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn2.Key = "id"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Unitate"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "nume"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 300
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Denumire (AlteDetalii)"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "denumire"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 330
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Capitol"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "capitol"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 90
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Sursă"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "sursa"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 80
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Zecimale salarii"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn7.Key = "zec"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.Width = 150
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Părinte"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Key = "parinte"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 90
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "AnDate"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn9.Key = "anDate"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Stare"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn10.Key = "stare"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.Width = 220
        dgvUnitati.Columns.Add(KBotDataColumn1)
        dgvUnitati.Columns.Add(KBotDataColumn2)
        dgvUnitati.Columns.Add(KBotDataColumn3)
        dgvUnitati.Columns.Add(KBotDataColumn4)
        dgvUnitati.Columns.Add(KBotDataColumn5)
        dgvUnitati.Columns.Add(KBotDataColumn6)
        dgvUnitati.Columns.Add(KBotDataColumn7)
        dgvUnitati.Columns.Add(KBotDataColumn8)
        dgvUnitati.Columns.Add(KBotDataColumn9)
        dgvUnitati.Columns.Add(KBotDataColumn10)
        dgvUnitati.Dock = DockStyle.Fill
        dgvUnitati.FillColumnKey = "nume"
        dgvUnitati.HeaderHeight = 26
        dgvUnitati.Location = New Point(0, 40)
        dgvUnitati.Margin = New Padding(0)
        dgvUnitati.Name = "dgvUnitati"
        dgvUnitati.RowHeight = 24
        dgvUnitati.Size = New Size(1573, 210)
        dgvUnitati.TabIndex = 1
        ' 
        ' tlpUnitateSus
        ' 
        tlpUnitateSus.ColumnCount = 9
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 123F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlpUnitateSus.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpUnitateSus.Controls.Add(BtnCitesteRegistru, 3, 0)
        tlpUnitateSus.Controls.Add(lblRegistru, 0, 0)
        tlpUnitateSus.Controls.Add(btnCiteste, 8, 0)
        tlpUnitateSus.Controls.Add(txtRegistru, 1, 0)
        tlpUnitateSus.Controls.Add(btnRasfoireRegistru, 2, 0)
        tlpUnitateSus.Controls.Add(lblDc, 4, 0)
        tlpUnitateSus.Controls.Add(cboDc, 5, 0)
        tlpUnitateSus.Controls.Add(lblCodFiscal, 6, 0)
        tlpUnitateSus.Controls.Add(txtCodFiscal, 7, 0)
        tlpUnitateSus.Dock = DockStyle.Top
        tlpUnitateSus.Location = New Point(0, 0)
        tlpUnitateSus.Margin = New Padding(0)
        tlpUnitateSus.Name = "tlpUnitateSus"
        tlpUnitateSus.Padding = New Padding(0, 0, 0, 10)
        tlpUnitateSus.RowCount = 1
        tlpUnitateSus.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlpUnitateSus.Size = New Size(1573, 40)
        tlpUnitateSus.TabIndex = 4
        ' 
        ' BtnCitesteRegistru
        ' 
        BtnCitesteRegistru.Dock = DockStyle.Fill
        BtnCitesteRegistru.Image = CType(resources.GetObject("BtnCitesteRegistru.Image"), Image)
        BtnCitesteRegistru.Location = New Point(438, 0)
        BtnCitesteRegistru.Margin = New Padding(4, 0, 4, 0)
        BtnCitesteRegistru.Name = "BtnCitesteRegistru"
        BtnCitesteRegistru.Size = New Size(46, 40)
        BtnCitesteRegistru.TabIndex = 10
        tipAvc.SetToolTipHeader(BtnCitesteRegistru, "Alegeți registrul")
        tipAvc.SetToolTipText(BtnCitesteRegistru, "Caută fișierul cale.accdb, de obicei în C:\AVACONT.")
        BtnCitesteRegistru.UseVisualStyleBackColor = True
        ' 
        ' lblRegistru
        ' 
        lblRegistru.AutoSize = True
        lblRegistru.Dock = DockStyle.Fill
        lblRegistru.Location = New Point(0, 0)
        lblRegistru.Margin = New Padding(0)
        lblRegistru.Name = "lblRegistru"
        lblRegistru.Size = New Size(130, 40)
        lblRegistru.TabIndex = 0
        lblRegistru.Text = "Cale.accdb"
        lblRegistru.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnCiteste
        ' 
        btnCiteste.Dock = DockStyle.Right
        btnCiteste.Location = New Point(1378, 0)
        btnCiteste.Margin = New Padding(0)
        btnCiteste.Name = "btnCiteste"
        btnCiteste.Size = New Size(195, 40)
        btnCiteste.TabIndex = 2
        btnCiteste.Text = "Citește unitățile bifate"
        tipAvc.SetToolTipHeader(btnCiteste, "Citește unitățile bifate")
        tipAvc.SetToolTipText(btnCiteste, "Citește fișierele Access ale unităților bifate (doar citire) și arată câte rânduri are fiecare pas. Nu atinge serverul.")
        btnCiteste.UseVisualStyleBackColor = True
        ' 
        ' txtRegistru
        ' 
        txtRegistru.BackColor = Color.Transparent
        txtRegistru.Dock = DockStyle.Fill
        txtRegistru.Location = New Point(130, 0)
        txtRegistru.Margin = New Padding(0)
        txtRegistru.Name = "txtRegistru"
        txtRegistru.PlaceholderText = "C:\AVACONT\cale.accdb"
        txtRegistru.Size = New Size(250, 40)
        txtRegistru.TabIndex = 1
        txtRegistru.TextPadding = New Padding(12, 0, 12, 0)
        tipAvc.SetToolTipHeader(txtRegistru, "Registrul AVACONT")
        tipAvc.SetToolTipText(txtRegistru, "Fișierul «cale.accdb». Din el se citesc DC-urile, unitățile și căile către fișierele fiecărei unități — nu trebuie tastate una câte una.")
        ' 
        ' btnRasfoireRegistru
        ' 
        btnRasfoireRegistru.Dock = DockStyle.Fill
        btnRasfoireRegistru.Location = New Point(384, 0)
        btnRasfoireRegistru.Margin = New Padding(4, 0, 4, 0)
        btnRasfoireRegistru.Name = "btnRasfoireRegistru"
        btnRasfoireRegistru.Size = New Size(46, 40)
        btnRasfoireRegistru.TabIndex = 2
        btnRasfoireRegistru.Text = "..."
        tipAvc.SetToolTipHeader(btnRasfoireRegistru, "Alegeți registrul")
        tipAvc.SetToolTipText(btnRasfoireRegistru, "Caută fișierul cale.accdb, de obicei în C:\AVACONT.")
        btnRasfoireRegistru.UseVisualStyleBackColor = True
        ' 
        ' lblDc
        ' 
        lblDc.AutoSize = True
        lblDc.Dock = DockStyle.Fill
        lblDc.Location = New Point(488, 0)
        lblDc.Margin = New Padding(0)
        lblDc.Name = "lblDc"
        lblDc.Size = New Size(100, 40)
        lblDc.TabIndex = 6
        lblDc.Text = "Unitate"
        lblDc.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboDc
        ' 
        cboDc.Dock = DockStyle.Fill
        cboDc.Location = New Point(588, 0)
        cboDc.Margin = New Padding(0)
        cboDc.Name = "cboDc"
        cboDc.Size = New Size(250, 37)
        cboDc.TabIndex = 7
        tipAvc.SetToolTipHeader(cboDc, "DC")
        tipAvc.SetToolTipText(cboDc, "Baza MariaDB a unității: aici se scrie totul. Schimbă lista de unități de mai jos.")
        ' 
        ' lblCodFiscal
        ' 
        lblCodFiscal.AutoSize = True
        lblCodFiscal.Dock = DockStyle.Fill
        lblCodFiscal.Location = New Point(838, 0)
        lblCodFiscal.Margin = New Padding(0)
        lblCodFiscal.Name = "lblCodFiscal"
        lblCodFiscal.Size = New Size(123, 40)
        lblCodFiscal.TabIndex = 8
        lblCodFiscal.Text = "Cod fiscal"
        lblCodFiscal.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCodFiscal
        ' 
        txtCodFiscal.BackColor = Color.Transparent
        txtCodFiscal.Dock = DockStyle.Fill
        txtCodFiscal.Location = New Point(961, 0)
        txtCodFiscal.Margin = New Padding(0)
        txtCodFiscal.Name = "txtCodFiscal"
        txtCodFiscal.PlaceholderText = "se completează la alegerea DC-ului"
        txtCodFiscal.Size = New Size(250, 40)
        txtCodFiscal.TabIndex = 9
        txtCodFiscal.TextPadding = New Padding(12, 0, 12, 0)
        tipAvc.SetToolTipHeader(txtCodFiscal, "Cod fiscal")
        tipAvc.SetToolTipText(txtCodFiscal, "Propus din registrul Windows al aplicației vechi, pentru DC-ul ales; îl puteți corecta. Se scrie în antetul jurnalului SQL. Nu se memorează între porniri.")
        ' 
        ' tlpActiune
        ' 
        tlpActiune.ColumnCount = 6
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 310F))
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpActiune.Controls.Add(BtnRaportHTML, 5, 0)
        tlpActiune.Controls.Add(btnVerifica, 2, 0)
        tlpActiune.Controls.Add(btnMigreaza, 3, 0)
        tlpActiune.Controls.Add(btnStop, 0, 0)
        tlpActiune.Dock = DockStyle.Fill
        tlpActiune.Location = New Point(12, 312)
        tlpActiune.Margin = New Padding(4)
        tlpActiune.Name = "tlpActiune"
        tlpActiune.RowCount = 1
        tlpActiune.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpActiune.Size = New Size(1593, 42)
        tlpActiune.TabIndex = 2
        ' 
        ' btnVerifica
        ' 
        btnVerifica.Dock = DockStyle.Fill
        btnVerifica.Location = New Point(460, 0)
        btnVerifica.Margin = New Padding(0)
        btnVerifica.Name = "btnVerifica"
        btnVerifica.Size = New Size(150, 42)
        btnVerifica.TabIndex = 3
        btnVerifica.Text = "Verifică"
        tipAvc.SetToolTipHeader(btnVerifica, "Verifică")
        tipAvc.SetToolTipText(btnVerifica, "Verifică parola, baza și structura, apoi compară ce s-a citit din Access cu nomenclatoarele. Nu scrie nimic.")
        btnVerifica.UseVisualStyleBackColor = True
        ' 
        ' btnMigreaza
        ' 
        btnMigreaza.Dock = DockStyle.Fill
        btnMigreaza.Enabled = False
        btnMigreaza.Location = New Point(610, 0)
        btnMigreaza.Margin = New Padding(0)
        btnMigreaza.Name = "btnMigreaza"
        btnMigreaza.Size = New Size(150, 42)
        btnMigreaza.TabIndex = 4
        btnMigreaza.Text = "Migrează"
        tipAvc.SetToolTipHeader(btnMigreaza, "Migrează")
        tipAvc.SetToolTipText(btnMigreaza, "Scrie pașii bifați într-o singură tranzacție, după o verificare fără blocaje.")
        btnMigreaza.UseVisualStyleBackColor = True
        ' 
        ' btnStop
        ' 
        btnStop.Dock = DockStyle.Fill
        btnStop.Enabled = False
        btnStop.Location = New Point(0, 0)
        btnStop.Margin = New Padding(0)
        btnStop.Name = "btnStop"
        btnStop.Size = New Size(150, 42)
        btnStop.TabIndex = 1
        btnStop.Text = "Oprește"
        tipAvc.SetToolTipHeader(btnStop, "Oprește")
        tipAvc.SetToolTipText(btnStop, "Cere oprirea înainte de COMMIT; se face rollback.")
        btnStop.UseVisualStyleBackColor = True
        ' 
        ' tlpJos
        ' 
        tlpJos.ColumnCount = 2
        tlpJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 48F))
        tlpJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52F))
        tlpJos.Controls.Add(grpPasi, 0, 0)
        tlpJos.Controls.Add(grpConstatari, 1, 0)
        tlpJos.Dock = DockStyle.Fill
        tlpJos.Location = New Point(12, 362)
        tlpJos.Margin = New Padding(4)
        tlpJos.Name = "tlpJos"
        tlpJos.RowCount = 1
        tlpJos.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpJos.Size = New Size(1593, 312)
        tlpJos.TabIndex = 3
        ' 
        ' grpPasi
        ' 
        grpPasi.Controls.Add(dgvPasi)
        grpPasi.Dock = DockStyle.Fill
        grpPasi.Location = New Point(4, 4)
        grpPasi.Margin = New Padding(4)
        grpPasi.Name = "grpPasi"
        grpPasi.Padding = New Padding(4)
        grpPasi.Size = New Size(756, 304)
        grpPasi.TabIndex = 0
        grpPasi.TabStop = False
        grpPasi.Text = "Ce se migrează (în ordinea rulării)"
        ' 
        ' dgvPasi
        ' 
        dgvPasi.BackColor = SystemColors.Window
        dgvPasi.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "S"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn11.Key = "bifat"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.Width = 40
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Etapa"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn12.Key = "etapa"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.Width = 120
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Pas"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn13.Key = "pas"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.Width = 120
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Tabel"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn14.Key = "tabel"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.Width = 110
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Rânduri"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "randuriAccess"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.ReadOnly = True
        KBotDataColumn15.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn15.Width = 50
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "De scris"
        KBotDataColumn16.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn16.Key = "deScris"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn16.Width = 80
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Acum în MariaDB"
        KBotDataColumn17.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn17.Key = "inBaza"
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.ReadOnly = True
        KBotDataColumn17.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn17.Width = 120
        dgvPasi.Columns.Add(KBotDataColumn11)
        dgvPasi.Columns.Add(KBotDataColumn12)
        dgvPasi.Columns.Add(KBotDataColumn13)
        dgvPasi.Columns.Add(KBotDataColumn14)
        dgvPasi.Columns.Add(KBotDataColumn15)
        dgvPasi.Columns.Add(KBotDataColumn16)
        dgvPasi.Columns.Add(KBotDataColumn17)
        dgvPasi.Dock = DockStyle.Fill
        dgvPasi.FillColumnKey = "pas"
        dgvPasi.HeaderHeight = 26
        dgvPasi.Location = New Point(4, 26)
        dgvPasi.Margin = New Padding(4)
        dgvPasi.Name = "dgvPasi"
        dgvPasi.RowHeight = 24
        dgvPasi.Size = New Size(748, 274)
        dgvPasi.TabIndex = 0
        ' 
        ' grpConstatari
        ' 
        grpConstatari.Controls.Add(rtbConstatari)
        grpConstatari.Dock = DockStyle.Fill
        grpConstatari.Location = New Point(768, 4)
        grpConstatari.Margin = New Padding(4)
        grpConstatari.Name = "grpConstatari"
        grpConstatari.Padding = New Padding(4)
        grpConstatari.Size = New Size(821, 304)
        grpConstatari.TabIndex = 1
        grpConstatari.TabStop = False
        grpConstatari.Text = "Constatări (apar după «Citește» și «Verifică»)"
        ' 
        ' rtbConstatari
        ' 
        rtbConstatari.Dock = DockStyle.Fill
        rtbConstatari.Location = New Point(4, 26)
        rtbConstatari.Margin = New Padding(4)
        rtbConstatari.Name = "rtbConstatari"
        rtbConstatari.ReadOnly = True
        rtbConstatari.Size = New Size(813, 274)
        rtbConstatari.TabIndex = 0
        rtbConstatari.Text = ""
        ' 
        ' grpLog
        ' 
        grpLog.Controls.Add(rtbLog)
        grpLog.Dock = DockStyle.Fill
        grpLog.Location = New Point(12, 682)
        grpLog.Margin = New Padding(4)
        grpLog.Name = "grpLog"
        grpLog.Padding = New Padding(4)
        grpLog.Size = New Size(1593, 242)
        grpLog.TabIndex = 4
        grpLog.TabStop = False
        grpLog.Text = "Jurnal de operații și erori"
        ' 
        ' rtbLog
        ' 
        rtbLog.Dock = DockStyle.Fill
        rtbLog.Location = New Point(4, 26)
        rtbLog.Margin = New Padding(4)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.Size = New Size(1585, 212)
        rtbLog.TabIndex = 0
        rtbLog.Text = ""
        ' 
        ' lblStare
        ' 
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(12, 928)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1593, 30)
        lblStare.TabIndex = 5
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' BtnRaportHTML
        ' 
        BtnRaportHTML.Dock = DockStyle.Fill
        BtnRaportHTML.Enabled = False
        BtnRaportHTML.Location = New Point(1443, 0)
        BtnRaportHTML.Margin = New Padding(0)
        BtnRaportHTML.Name = "BtnRaportHTML"
        BtnRaportHTML.Size = New Size(150, 42)
        BtnRaportHTML.TabIndex = 5
        BtnRaportHTML.Text = "Raport HTML"
        tipAvc.SetToolTipHeader(BtnRaportHTML, "Raport HTML")
        tipAvc.SetToolTipText(BtnRaportHTML, "Arată, lizibil și colorat, blocajele, atenționările și informațiile verificării și, după migrare, ce s-a scris. Se poate salva ca fișier HTML. Folosește doar ce s-a citit deja.")
        BtnRaportHTML.UseVisualStyleBackColor = True
        ' 
        ' AvcMigratorForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1617, 966)
        Controls.Add(tlpRoot)
        Margin = New Padding(4)
        MinimumSize = New Size(1639, 1022)
        Name = "AvcMigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "AVC.Migrator — Access Avacont ▸ MariaDB"
        tlpRoot.ResumeLayout(False)
        grpUnitate.ResumeLayout(False)
        pnlUnitateSus.ResumeLayout(False)
        CType(dgvUnitati, ComponentModel.ISupportInitialize).EndInit()
        tlpUnitateSus.ResumeLayout(False)
        tlpUnitateSus.PerformLayout()
        tlpActiune.ResumeLayout(False)
        tlpJos.ResumeLayout(False)
        grpPasi.ResumeLayout(False)
        CType(dgvPasi, ComponentModel.ISupportInitialize).EndInit()
        grpConstatari.ResumeLayout(False)
        grpLog.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents KBotTableLayoutPanel1 As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents BtnCitesteRegistru As Button
    Friend WithEvents BtnRaportHTML As Button

End Class
