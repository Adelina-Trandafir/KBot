<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer
    ' (docs/kbot-forms-ui-convention.md). Nothing is built in code.
    ' Each region keeps its fields in a TableLayoutPanel, so the layout comes from cells
    ' rather than from hand-written coordinates.
    '
    ' Slice 0045 note on the FILE fields: the plan asked for a "Cale FX" and a "Cale CAI"
    ' box. The discovery pass changed that - the per-unit baza<year>.accdb and FX_<year>.accdb
    ' paths are IN the registry (cai.FullPath, cai.CaleForexe), so the operator picks
    ' cale.accdb only, and the tool reads the rest. What is left to type is the two
    ' passwords and the journal folder.
    Private components As System.ComponentModel.IContainer

    ' --- root ------------------------------------------------------------------
    Friend WithEvents tlpRoot As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblRegistru As System.Windows.Forms.Label
    Friend WithEvents txtRegistru As KBot.Controls.KBotTextField
    Friend WithEvents btnRasfoireRegistru As System.Windows.Forms.Button
    Friend WithEvents lblParolaUnitati As System.Windows.Forms.Label
    Friend WithEvents txtParolaUnitati As KBot.Controls.KBotTextField

    ' --- region 1.5: python -------------------------------------------------------
    Friend WithEvents grpPython As System.Windows.Forms.GroupBox

    ' --- region 2: server ------------------------------------------------------
    Friend WithEvents grpServer As System.Windows.Forms.GroupBox
    Friend WithEvents tlpServer As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblGazda As System.Windows.Forms.Label
    Friend WithEvents txtGazda As KBot.Controls.KBotTextField
    Friend WithEvents lblPort As System.Windows.Forms.Label
    Friend WithEvents txtPort As KBot.Controls.KBotTextField
    Friend WithEvents lblUtilizator As System.Windows.Forms.Label
    Friend WithEvents txtUtilizator As KBot.Controls.KBotTextField
    Friend WithEvents lblParolaServer As System.Windows.Forms.Label
    Friend WithEvents txtParolaServer As KBot.Controls.KBotTextField
    Friend WithEvents btnTesteaza As System.Windows.Forms.Button
    Friend WithEvents lblStareServer As System.Windows.Forms.Label

    ' --- region 3: unit --------------------------------------------------------
    Friend WithEvents grpUnitate As System.Windows.Forms.GroupBox
    Friend WithEvents pnlUnitateSus As System.Windows.Forms.Panel
    Friend WithEvents lblDc As System.Windows.Forms.Label
    Friend WithEvents cboDc As System.Windows.Forms.ComboBox
    Friend WithEvents btnCitesteRegistru As System.Windows.Forms.Button
    Friend WithEvents lblBazaTinta As System.Windows.Forms.Label
    Friend WithEvents dgvUnitati As KBot.Controls.KBotDataView

    ' --- region 4: transfer ----------------------------------------------------
    Friend WithEvents grpTransfer As System.Windows.Forms.GroupBox
    Friend WithEvents tlpTransfer As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents pnlButoane As System.Windows.Forms.Panel
    Friend WithEvents btnVerifica As System.Windows.Forms.Button
    Friend WithEvents btnTransfera As System.Windows.Forms.Button
    Friend WithEvents btnOpreste As System.Windows.Forms.Button
    Friend WithEvents prgTransfer As System.Windows.Forms.ProgressBar
    Friend WithEvents tlpGrile As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents dgvTabele As KBot.Controls.KBotDataView
    Friend WithEvents tlpDreapta As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents dgvConstatari As KBot.Controls.KBotDataView
    Friend WithEvents rtbJurnal As System.Windows.Forms.RichTextBox

    Friend WithEvents tipMigrator As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MigratorForm))
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn9 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn10 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn11 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn12 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn13 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn14 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn15 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tlpRoot = New TableLayoutPanel()
        capBar = New Controls.KBotCaptionBar()
        grpPython = New GroupBox()
        tplPython = New TableLayoutPanel()
        txtCheieApi = New Controls.KBotTextField()
        lblCheieApi = New Label()
        txtServerUrl = New Controls.KBotTextField()
        lblServerUrl = New Label()
        grpServer = New GroupBox()
        tlpServer = New TableLayoutPanel()
        lblGazda = New Label()
        txtGazda = New Controls.KBotTextField()
        lblPort = New Label()
        txtPort = New Controls.KBotTextField()
        lblUtilizator = New Label()
        txtUtilizator = New Controls.KBotTextField()
        lblParolaServer = New Label()
        txtParolaServer = New Controls.KBotTextField()
        btnTesteaza = New Button()
        lblStareServer = New Label()
        grpUnitate = New GroupBox()
        dgvUnitati = New Controls.KBotDataView()
        pnlUnitateSus = New Panel()
        TableLayoutPanel1 = New TableLayoutPanel()
        btnRasfoireRegistru = New Button()
        txtRegistru = New Controls.KBotTextField()
        txtParolaUnitati = New Controls.KBotTextField()
        lblRegistru = New Label()
        lblParolaUnitati = New Label()
        btnCitesteRegistru = New Button()
        cboDc = New ComboBox()
        lblDc = New Label()
        lblBazaTinta = New Label()
        grpTransfer = New GroupBox()
        tlpTransfer = New TableLayoutPanel()
        pnlButoane = New Panel()
        tlpButoane = New TableLayoutPanel()
        btnVerifica = New Button()
        prgTransfer = New ProgressBar()
        btnOpreste = New Button()
        btnTransfera = New Button()
        tlpGrile = New TableLayoutPanel()
        dgvTabele = New Controls.KBotDataView()
        tlpDreapta = New TableLayoutPanel()
        dgvConstatari = New Controls.KBotDataView()
        rtbJurnal = New RichTextBox()
        rtbInfoRowConstatari = New RichTextBox()
        tipMigrator = New KBot.Controls.KBotToolTip(components)
        tlpRoot.SuspendLayout()
        grpPython.SuspendLayout()
        tplPython.SuspendLayout()
        grpServer.SuspendLayout()
        tlpServer.SuspendLayout()
        grpUnitate.SuspendLayout()
        CType(dgvUnitati, ComponentModel.ISupportInitialize).BeginInit()
        pnlUnitateSus.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        grpTransfer.SuspendLayout()
        tlpTransfer.SuspendLayout()
        pnlButoane.SuspendLayout()
        tlpButoane.SuspendLayout()
        tlpGrile.SuspendLayout()
        CType(dgvTabele, ComponentModel.ISupportInitialize).BeginInit()
        tlpDreapta.SuspendLayout()
        CType(dgvConstatari, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' tlpRoot
        ' 
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpRoot.Controls.Add(capBar, 0, 0)
        tlpRoot.Controls.Add(grpPython, 0, 1)
        tlpRoot.Controls.Add(grpServer, 0, 2)
        tlpRoot.Controls.Add(grpUnitate, 0, 3)
        tlpRoot.Controls.Add(grpTransfer, 0, 4)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Margin = New Padding(4, 5, 4, 5)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.RowCount = 5
        tlpRoot.RowStyles.Add(New RowStyle())
        tlpRoot.RowStyles.Add(New RowStyle())
        tlpRoot.RowStyles.Add(New RowStyle())
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 30.77922F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 69.22078F))
        tlpRoot.Size = New Size(1505, 1044)
        tlpRoot.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(4, 5)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(1497, 67)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        ' 
        ' grpPython
        ' 
        grpPython.Controls.Add(tplPython)
        grpPython.Dock = DockStyle.Fill
        grpPython.Location = New Point(4, 82)
        grpPython.Margin = New Padding(4, 5, 4, 5)
        grpPython.Name = "grpPython"
        grpPython.Padding = New Padding(14, 4, 14, 4)
        grpPython.Size = New Size(1497, 84)
        grpPython.TabIndex = 0
        grpPython.TabStop = False
        grpPython.Text = "Server Python"
        ' 
        ' tplPython
        ' 
        tplPython.ColumnCount = 5
        tplPython.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tplPython.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 399F))
        tplPython.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 101F))
        tplPython.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 350F))
        tplPython.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tplPython.Controls.Add(txtCheieApi, 3, 0)
        tplPython.Controls.Add(lblCheieApi, 2, 0)
        tplPython.Controls.Add(txtServerUrl, 1, 0)
        tplPython.Controls.Add(lblServerUrl, 0, 0)
        tplPython.Dock = DockStyle.Fill
        tplPython.Location = New Point(14, 28)
        tplPython.Margin = New Padding(0)
        tplPython.Name = "tplPython"
        tplPython.RowCount = 1
        tplPython.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tplPython.Size = New Size(1469, 52)
        tplPython.TabIndex = 0
        ' 
        ' txtCheieApi
        ' 
        txtCheieApi.BackColor = Color.Transparent
        tplPython.SetColumnSpan(txtCheieApi, 2)
        txtCheieApi.Dock = DockStyle.Fill
        txtCheieApi.Location = New Point(654, 5)
        txtCheieApi.Margin = New Padding(4, 5, 4, 5)
        txtCheieApi.MaxLength = 32767
        txtCheieApi.Name = "txtCheieApi"
        txtCheieApi.PlaceholderText = "doar când se construiește o bază goală"
        txtCheieApi.Size = New Size(811, 42)
        txtCheieApi.TabIndex = 4
        txtCheieApi.TabStop = False
        tipMigrator.SetToolTipHeader(txtCheieApi, "Cheia API")
        tipMigrator.SetToolTipText(txtCheieApi, resources.GetString("txtCheieApi.ToolTipText"))
        txtCheieApi.UseSystemPasswordChar = True
        ' 
        ' lblCheieApi
        ' 
        lblCheieApi.AutoSize = True
        lblCheieApi.Dock = DockStyle.Fill
        lblCheieApi.Location = New Point(552, 0)
        lblCheieApi.Name = "lblCheieApi"
        lblCheieApi.Size = New Size(95, 52)
        lblCheieApi.TabIndex = 3
        lblCheieApi.Text = "Cheie API"
        lblCheieApi.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' txtServerUrl
        ' 
        txtServerUrl.BackColor = Color.Transparent
        txtServerUrl.Dock = DockStyle.Fill
        txtServerUrl.Location = New Point(154, 5)
        txtServerUrl.Margin = New Padding(4, 5, 4, 5)
        txtServerUrl.MaxLength = 32767
        txtServerUrl.Name = "txtServerUrl"
        txtServerUrl.PlaceholderText = "https://server.exemplu.ro"
        txtServerUrl.Size = New Size(391, 42)
        txtServerUrl.TabIndex = 2
        txtServerUrl.TabStop = False
        tipMigrator.SetToolTipHeader(txtServerUrl, "Adresa serverului")
        tipMigrator.SetToolTipText(txtServerUrl, resources.GetString("txtServerUrl.ToolTipText"))
        txtServerUrl.UseSystemPasswordChar = False
        ' 
        ' lblServerUrl
        ' 
        lblServerUrl.AutoSize = True
        lblServerUrl.Dock = DockStyle.Fill
        lblServerUrl.Location = New Point(3, 0)
        lblServerUrl.Name = "lblServerUrl"
        lblServerUrl.Size = New Size(144, 52)
        lblServerUrl.TabIndex = 0
        lblServerUrl.Text = "Adresă server"
        lblServerUrl.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' grpServer
        ' 
        grpServer.AutoSize = True
        grpServer.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpServer.Controls.Add(tlpServer)
        grpServer.Dock = DockStyle.Fill
        grpServer.Location = New Point(4, 176)
        grpServer.Margin = New Padding(4, 5, 4, 5)
        grpServer.Name = "grpServer"
        grpServer.Padding = New Padding(14, 4, 14, 4)
        grpServer.Size = New Size(1497, 84)
        grpServer.TabIndex = 1
        grpServer.TabStop = False
        grpServer.Text = "Server MariaDB"
        ' 
        ' tlpServer
        ' 
        tlpServer.AutoSize = True
        tlpServer.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpServer.ColumnCount = 10
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpServer.Controls.Add(lblGazda, 0, 0)
        tlpServer.Controls.Add(txtGazda, 1, 0)
        tlpServer.Controls.Add(lblPort, 2, 0)
        tlpServer.Controls.Add(txtPort, 3, 0)
        tlpServer.Controls.Add(lblUtilizator, 4, 0)
        tlpServer.Controls.Add(txtUtilizator, 5, 0)
        tlpServer.Controls.Add(lblParolaServer, 6, 0)
        tlpServer.Controls.Add(txtParolaServer, 7, 0)
        tlpServer.Controls.Add(btnTesteaza, 8, 0)
        tlpServer.Controls.Add(lblStareServer, 9, 0)
        tlpServer.Dock = DockStyle.Fill
        tlpServer.Location = New Point(14, 28)
        tlpServer.Margin = New Padding(4, 5, 4, 5)
        tlpServer.Name = "tlpServer"
        tlpServer.RowCount = 1
        tlpServer.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlpServer.Size = New Size(1469, 52)
        tlpServer.TabIndex = 0
        ' 
        ' lblGazda
        ' 
        lblGazda.AutoSize = True
        lblGazda.Dock = DockStyle.Fill
        lblGazda.Location = New Point(4, 0)
        lblGazda.Margin = New Padding(4, 0, 4, 0)
        lblGazda.Name = "lblGazda"
        lblGazda.Size = New Size(92, 52)
        lblGazda.TabIndex = 0
        lblGazda.Text = "Gazdă"
        lblGazda.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtGazda
        ' 
        txtGazda.BackColor = Color.Transparent
        txtGazda.Dock = DockStyle.Fill
        txtGazda.Location = New Point(104, 5)
        txtGazda.Margin = New Padding(4, 5, 4, 5)
        txtGazda.MaxLength = 32767
        txtGazda.Name = "txtGazda"
        txtGazda.PlaceholderText = ""
        txtGazda.Size = New Size(142, 42)
        txtGazda.TabIndex = 1
        txtGazda.TabStop = False
        txtGazda.UseSystemPasswordChar = False
        ' 
        ' lblPort
        ' 
        lblPort.AutoSize = True
        lblPort.Dock = DockStyle.Fill
        lblPort.Location = New Point(254, 0)
        lblPort.Margin = New Padding(4, 0, 4, 0)
        lblPort.Name = "lblPort"
        lblPort.Size = New Size(92, 52)
        lblPort.TabIndex = 2
        lblPort.Text = "Port"
        lblPort.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' txtPort
        ' 
        txtPort.BackColor = Color.Transparent
        txtPort.Dock = DockStyle.Fill
        txtPort.Location = New Point(354, 5)
        txtPort.Margin = New Padding(4, 5, 4, 5)
        txtPort.MaxLength = 32767
        txtPort.Name = "txtPort"
        txtPort.PlaceholderText = ""
        txtPort.Size = New Size(142, 42)
        txtPort.TabIndex = 3
        txtPort.TabStop = False
        txtPort.UseSystemPasswordChar = False
        ' 
        ' lblUtilizator
        ' 
        lblUtilizator.AutoSize = True
        lblUtilizator.Dock = DockStyle.Fill
        lblUtilizator.Location = New Point(504, 0)
        lblUtilizator.Margin = New Padding(4, 0, 4, 0)
        lblUtilizator.Name = "lblUtilizator"
        lblUtilizator.Size = New Size(92, 52)
        lblUtilizator.TabIndex = 4
        lblUtilizator.Text = "Utilizator"
        lblUtilizator.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtUtilizator
        ' 
        txtUtilizator.BackColor = Color.Transparent
        txtUtilizator.Dock = DockStyle.Fill
        txtUtilizator.Location = New Point(604, 5)
        txtUtilizator.Margin = New Padding(4, 5, 4, 5)
        txtUtilizator.MaxLength = 32767
        txtUtilizator.Name = "txtUtilizator"
        txtUtilizator.PlaceholderText = ""
        txtUtilizator.Size = New Size(142, 42)
        txtUtilizator.TabIndex = 5
        txtUtilizator.TabStop = False
        txtUtilizator.UseSystemPasswordChar = False
        ' 
        ' lblParolaServer
        ' 
        lblParolaServer.AutoSize = True
        lblParolaServer.Dock = DockStyle.Fill
        lblParolaServer.Location = New Point(754, 0)
        lblParolaServer.Margin = New Padding(4, 0, 4, 0)
        lblParolaServer.Name = "lblParolaServer"
        lblParolaServer.Size = New Size(92, 52)
        lblParolaServer.TabIndex = 6
        lblParolaServer.Text = "Parolă"
        lblParolaServer.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' txtParolaServer
        ' 
        txtParolaServer.BackColor = Color.Transparent
        txtParolaServer.Dock = DockStyle.Fill
        txtParolaServer.Location = New Point(854, 5)
        txtParolaServer.Margin = New Padding(4, 5, 4, 5)
        txtParolaServer.MaxLength = 32767
        txtParolaServer.Name = "txtParolaServer"
        txtParolaServer.PlaceholderText = ""
        txtParolaServer.Size = New Size(192, 42)
        txtParolaServer.TabIndex = 7
        txtParolaServer.TabStop = False
        txtParolaServer.UseSystemPasswordChar = True
        ' 
        ' btnTesteaza
        ' 
        btnTesteaza.Dock = DockStyle.Fill
        btnTesteaza.Location = New Point(1050, 0)
        btnTesteaza.Margin = New Padding(0)
        btnTesteaza.Name = "btnTesteaza"
        btnTesteaza.Size = New Size(150, 52)
        btnTesteaza.TabIndex = 8
        btnTesteaza.Text = "Testează"
        btnTesteaza.UseVisualStyleBackColor = True
        ' 
        ' lblStareServer
        ' 
        lblStareServer.AutoSize = True
        lblStareServer.Dock = DockStyle.Fill
        lblStareServer.Location = New Point(1204, 0)
        lblStareServer.Margin = New Padding(4, 0, 4, 0)
        lblStareServer.Name = "lblStareServer"
        lblStareServer.Size = New Size(261, 52)
        lblStareServer.TabIndex = 9
        lblStareServer.Text = "Neconectat."
        lblStareServer.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' grpUnitate
        ' 
        grpUnitate.Controls.Add(dgvUnitati)
        grpUnitate.Controls.Add(pnlUnitateSus)
        grpUnitate.Dock = DockStyle.Fill
        grpUnitate.Location = New Point(4, 270)
        grpUnitate.Margin = New Padding(4, 5, 4, 5)
        grpUnitate.Name = "grpUnitate"
        grpUnitate.Padding = New Padding(14, 4, 14, 4)
        grpUnitate.Size = New Size(1497, 229)
        grpUnitate.TabIndex = 2
        grpUnitate.TabStop = False
        grpUnitate.Text = "Unitate"
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
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Unitate"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "nume"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 300
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Sursă"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "sursa"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 70
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Fișier nomenclatoare"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "nomenclator"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 350
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Fișier FOREXE"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "forexe"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Visible = False
        KBotDataColumn6.Width = 350
        dgvUnitati.Columns.Add(KBotDataColumn1)
        dgvUnitati.Columns.Add(KBotDataColumn2)
        dgvUnitati.Columns.Add(KBotDataColumn3)
        dgvUnitati.Columns.Add(KBotDataColumn4)
        dgvUnitati.Columns.Add(KBotDataColumn5)
        dgvUnitati.Columns.Add(KBotDataColumn6)
        dgvUnitati.Dock = DockStyle.Fill
        dgvUnitati.FillColumnKey = "nume"
        dgvUnitati.HeaderHeight = 26
        dgvUnitati.Location = New Point(527, 28)
        dgvUnitati.Margin = New Padding(4, 5, 4, 5)
        dgvUnitati.Name = "dgvUnitati"
        dgvUnitati.RowHeight = 24
        dgvUnitati.Size = New Size(956, 197)
        dgvUnitati.TabIndex = 1
        ' 
        ' pnlUnitateSus
        ' 
        pnlUnitateSus.Controls.Add(TableLayoutPanel1)
        pnlUnitateSus.Controls.Add(lblBazaTinta)
        pnlUnitateSus.Dock = DockStyle.Left
        pnlUnitateSus.Location = New Point(14, 28)
        pnlUnitateSus.Margin = New Padding(0)
        pnlUnitateSus.Name = "pnlUnitateSus"
        pnlUnitateSus.Padding = New Padding(0, 0, 10, 10)
        pnlUnitateSus.Size = New Size(513, 197)
        pnlUnitateSus.TabIndex = 0
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 3
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 29.82107F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70.17893F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 62F))
        TableLayoutPanel1.Controls.Add(btnRasfoireRegistru, 2, 0)
        TableLayoutPanel1.Controls.Add(txtRegistru, 1, 0)
        TableLayoutPanel1.Controls.Add(txtParolaUnitati, 1, 1)
        TableLayoutPanel1.Controls.Add(lblRegistru, 0, 0)
        TableLayoutPanel1.Controls.Add(lblParolaUnitati, 0, 1)
        TableLayoutPanel1.Controls.Add(btnCitesteRegistru, 2, 1)
        TableLayoutPanel1.Controls.Add(cboDc, 1, 2)
        TableLayoutPanel1.Controls.Add(lblDc, 0, 2)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Margin = New Padding(0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 5
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 38F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        TableLayoutPanel1.Size = New Size(503, 187)
        TableLayoutPanel1.TabIndex = 4
        ' 
        ' btnRasfoireRegistru
        ' 
        btnRasfoireRegistru.Dock = DockStyle.Fill
        btnRasfoireRegistru.Location = New Point(444, 4)
        btnRasfoireRegistru.Margin = New Padding(4)
        btnRasfoireRegistru.Name = "btnRasfoireRegistru"
        btnRasfoireRegistru.Size = New Size(55, 42)
        btnRasfoireRegistru.TabIndex = 2
        btnRasfoireRegistru.Text = "..."
        btnRasfoireRegistru.UseVisualStyleBackColor = True
        ' 
        ' txtRegistru
        ' 
        txtRegistru.BackColor = Color.Transparent
        txtRegistru.Dock = DockStyle.Fill
        txtRegistru.Location = New Point(135, 5)
        txtRegistru.Margin = New Padding(4, 5, 4, 5)
        txtRegistru.MaxLength = 32767
        txtRegistru.Name = "txtRegistru"
        txtRegistru.PlaceholderText = "C:\AVACONT\cale.accdb"
        txtRegistru.Size = New Size(301, 40)
        txtRegistru.TabIndex = 1
        txtRegistru.TabStop = False
        tipMigrator.SetToolTipHeader(txtRegistru, "Registrul AVACONT")
        tipMigrator.SetToolTipText(txtRegistru, "Fișierul «cale.accdb». Din el se citesc DC-urile, unitățile și căile" & vbLf & "către fișierele fiecărei unități — nu trebuie tastate una câte una.")
        txtRegistru.UseSystemPasswordChar = False
        ' 
        ' txtParolaUnitati
        ' 
        txtParolaUnitati.BackColor = Color.Transparent
        txtParolaUnitati.Dock = DockStyle.Fill
        txtParolaUnitati.Location = New Point(135, 55)
        txtParolaUnitati.Margin = New Padding(4, 5, 4, 5)
        txtParolaUnitati.MaxLength = 32767
        txtParolaUnitati.Name = "txtParolaUnitati"
        txtParolaUnitati.PlaceholderText = "lăsați gol dacă nu sunt protejate"
        txtParolaUnitati.Size = New Size(301, 42)
        txtParolaUnitati.TabIndex = 4
        txtParolaUnitati.TabStop = False
        txtParolaUnitati.UseSystemPasswordChar = True
        ' 
        ' lblRegistru
        ' 
        lblRegistru.AutoSize = True
        lblRegistru.Dock = DockStyle.Fill
        lblRegistru.Location = New Point(4, 0)
        lblRegistru.Margin = New Padding(4, 0, 4, 0)
        lblRegistru.Name = "lblRegistru"
        lblRegistru.Size = New Size(123, 50)
        lblRegistru.TabIndex = 0
        lblRegistru.Text = "Registrul (cale.accdb)"
        lblRegistru.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblParolaUnitati
        ' 
        lblParolaUnitati.AutoSize = True
        lblParolaUnitati.Dock = DockStyle.Fill
        lblParolaUnitati.Location = New Point(4, 50)
        lblParolaUnitati.Margin = New Padding(4, 0, 4, 0)
        lblParolaUnitati.Name = "lblParolaUnitati"
        lblParolaUnitati.Size = New Size(123, 52)
        lblParolaUnitati.TabIndex = 3
        lblParolaUnitati.Text = "Parolă fișiere"
        lblParolaUnitati.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnCitesteRegistru
        ' 
        btnCitesteRegistru.BackColor = SystemColors.Control
        btnCitesteRegistru.Dock = DockStyle.Fill
        btnCitesteRegistru.FlatAppearance.MouseDownBackColor = Color.FromArgb(CByte(192), CByte(192), CByte(255))
        btnCitesteRegistru.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(128), CByte(128), CByte(255))
        btnCitesteRegistru.FlatStyle = FlatStyle.Flat
        btnCitesteRegistru.Image = My.Resources.Resources.database_play_32
        btnCitesteRegistru.ImageAlign = ContentAlignment.BottomCenter
        btnCitesteRegistru.Location = New Point(444, 54)
        btnCitesteRegistru.Margin = New Padding(4)
        btnCitesteRegistru.Name = "btnCitesteRegistru"
        btnCitesteRegistru.Size = New Size(55, 44)
        btnCitesteRegistru.TabIndex = 2
        tipMigrator.SetToolTipHeader(btnCitesteRegistru, "Citește CALE.ACCDB")
        btnCitesteRegistru.UseVisualStyleBackColor = False
        ' 
        ' cboDc
        ' 
        cboDc.Dock = DockStyle.Fill
        cboDc.DropDownStyle = ComboBoxStyle.DropDownList
        cboDc.Location = New Point(135, 107)
        cboDc.Margin = New Padding(4, 5, 4, 5)
        cboDc.Name = "cboDc"
        cboDc.Size = New Size(301, 33)
        cboDc.TabIndex = 1
        ' 
        ' lblDc
        ' 
        lblDc.AutoSize = True
        lblDc.Dock = DockStyle.Fill
        lblDc.Location = New Point(4, 102)
        lblDc.Margin = New Padding(4, 0, 4, 0)
        lblDc.Name = "lblDc"
        lblDc.Size = New Size(123, 38)
        lblDc.TabIndex = 0
        lblDc.Text = "DC"
        lblDc.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblBazaTinta
        ' 
        lblBazaTinta.AutoSize = True
        lblBazaTinta.Location = New Point(643, 20)
        lblBazaTinta.Margin = New Padding(4, 0, 4, 0)
        lblBazaTinta.Name = "lblBazaTinta"
        lblBazaTinta.Size = New Size(208, 25)
        lblBazaTinta.TabIndex = 3
        lblBazaTinta.Text = "Baza-țintă: (necunoscută)"
        ' 
        ' grpTransfer
        ' 
        grpTransfer.Controls.Add(tlpTransfer)
        grpTransfer.Dock = DockStyle.Fill
        grpTransfer.Location = New Point(4, 509)
        grpTransfer.Margin = New Padding(4, 5, 4, 5)
        grpTransfer.Name = "grpTransfer"
        grpTransfer.Padding = New Padding(14, 4, 14, 4)
        grpTransfer.Size = New Size(1497, 530)
        grpTransfer.TabIndex = 3
        grpTransfer.TabStop = False
        grpTransfer.Text = "Transfer"
        ' 
        ' tlpTransfer
        ' 
        tlpTransfer.ColumnCount = 1
        tlpTransfer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpTransfer.Controls.Add(pnlButoane, 0, 0)
        tlpTransfer.Controls.Add(tlpGrile, 0, 1)
        tlpTransfer.Dock = DockStyle.Fill
        tlpTransfer.Location = New Point(14, 28)
        tlpTransfer.Margin = New Padding(4, 5, 4, 5)
        tlpTransfer.Name = "tlpTransfer"
        tlpTransfer.RowCount = 2
        tlpTransfer.RowStyles.Add(New RowStyle(SizeType.Absolute, 65F))
        tlpTransfer.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpTransfer.Size = New Size(1469, 498)
        tlpTransfer.TabIndex = 0
        ' 
        ' pnlButoane
        ' 
        pnlButoane.Controls.Add(tlpButoane)
        pnlButoane.Dock = DockStyle.Fill
        pnlButoane.Location = New Point(0, 0)
        pnlButoane.Margin = New Padding(0)
        pnlButoane.Name = "pnlButoane"
        pnlButoane.Size = New Size(1469, 65)
        pnlButoane.TabIndex = 0
        ' 
        ' tlpButoane
        ' 
        tlpButoane.ColumnCount = 4
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpButoane.Controls.Add(btnVerifica, 0, 0)
        tlpButoane.Controls.Add(prgTransfer, 3, 0)
        tlpButoane.Controls.Add(btnOpreste, 2, 0)
        tlpButoane.Controls.Add(btnTransfera, 1, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Location = New Point(0, 0)
        tlpButoane.Name = "tlpButoane"
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpButoane.Size = New Size(1469, 65)
        tlpButoane.TabIndex = 4
        ' 
        ' btnVerifica
        ' 
        btnVerifica.Dock = DockStyle.Fill
        btnVerifica.Location = New Point(4, 5)
        btnVerifica.Margin = New Padding(4, 5, 4, 5)
        btnVerifica.Name = "btnVerifica"
        btnVerifica.Size = New Size(142, 55)
        btnVerifica.TabIndex = 0
        btnVerifica.Text = "Verifică"
        tipMigrator.SetToolTipHeader(btnVerifica, "Verifică")
        tipMigrator.SetToolTipText(btnVerifica, "Rulează toate porțile fără să scrie nimic: fișiere, bază, AVACONT_COMUN," & vbLf & "Unitati, coloane obligatorii, lățimi, rezoluții, ordinea de scriere.")
        btnVerifica.UseVisualStyleBackColor = True
        ' 
        ' prgTransfer
        ' 
        prgTransfer.Dock = DockStyle.Fill
        prgTransfer.Location = New Point(454, 5)
        prgTransfer.Margin = New Padding(4, 5, 4, 5)
        prgTransfer.Name = "prgTransfer"
        prgTransfer.Size = New Size(1011, 55)
        prgTransfer.TabIndex = 3
        ' 
        ' btnOpreste
        ' 
        btnOpreste.Dock = DockStyle.Fill
        btnOpreste.Enabled = False
        btnOpreste.Location = New Point(304, 5)
        btnOpreste.Margin = New Padding(4, 5, 4, 5)
        btnOpreste.Name = "btnOpreste"
        btnOpreste.Size = New Size(142, 55)
        btnOpreste.TabIndex = 2
        btnOpreste.Text = "Oprește"
        tipMigrator.SetToolTipHeader(btnOpreste, "Oprește")
        tipMigrator.SetToolTipText(btnOpreste, "Oprirea derulează tranzacția înapoi — baza rămâne exact cum era.")
        btnOpreste.UseVisualStyleBackColor = True
        ' 
        ' btnTransfera
        ' 
        btnTransfera.Dock = DockStyle.Fill
        btnTransfera.Enabled = False
        btnTransfera.Location = New Point(154, 5)
        btnTransfera.Margin = New Padding(4, 5, 4, 5)
        btnTransfera.Name = "btnTransfera"
        btnTransfera.Size = New Size(142, 55)
        btnTransfera.TabIndex = 1
        btnTransfera.Text = "Transferă"
        tipMigrator.SetToolTipHeader(btnTransfera, "Transferă")
        tipMigrator.SetToolTipText(btnTransfera, "Se activează doar după o verificare fără constatări blocante." & vbLf & "Scrie totul într-o singură tranzacție; orice eșec derulează tot înapoi.")
        btnTransfera.UseVisualStyleBackColor = True
        ' 
        ' tlpGrile
        ' 
        tlpGrile.ColumnCount = 2
        tlpGrile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40F))
        tlpGrile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60F))
        tlpGrile.Controls.Add(dgvTabele, 0, 0)
        tlpGrile.Controls.Add(tlpDreapta, 1, 0)
        tlpGrile.Dock = DockStyle.Fill
        tlpGrile.Location = New Point(4, 70)
        tlpGrile.Margin = New Padding(4, 5, 4, 5)
        tlpGrile.Name = "tlpGrile"
        tlpGrile.RowCount = 1
        tlpGrile.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpGrile.Size = New Size(1461, 423)
        tlpGrile.TabIndex = 1
        ' 
        ' dgvTabele
        ' 
        dgvTabele.BackColor = SystemColors.Window
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Scrie"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn7.Key = "bifa"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.Width = 70
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Tabel"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Key = "tabel"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.Width = 200
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Sursă"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn9.Key = "sursa"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.Width = 130
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Rânduri Access"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn10.Key = "randuri"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.Width = 130
        dgvTabele.Columns.Add(KBotDataColumn7)
        dgvTabele.Columns.Add(KBotDataColumn8)
        dgvTabele.Columns.Add(KBotDataColumn9)
        dgvTabele.Columns.Add(KBotDataColumn10)
        dgvTabele.Dock = DockStyle.Fill
        dgvTabele.HeaderHeight = 26
        dgvTabele.Location = New Point(4, 5)
        dgvTabele.Margin = New Padding(4, 5, 4, 5)
        dgvTabele.Name = "dgvTabele"
        dgvTabele.RowHeight = 24
        dgvTabele.Size = New Size(576, 413)
        dgvTabele.TabIndex = 0
        ' 
        ' tlpDreapta
        ' 
        tlpDreapta.ColumnCount = 2
        tlpDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 500F))
        tlpDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpDreapta.Controls.Add(dgvConstatari, 0, 0)
        tlpDreapta.Controls.Add(rtbJurnal, 0, 1)
        tlpDreapta.Controls.Add(rtbInfoRowConstatari, 1, 0)
        tlpDreapta.Dock = DockStyle.Fill
        tlpDreapta.Location = New Point(584, 0)
        tlpDreapta.Margin = New Padding(0)
        tlpDreapta.Name = "tlpDreapta"
        tlpDreapta.RowCount = 2
        tlpDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 61.810154F))
        tlpDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 38.189846F))
        tlpDreapta.Size = New Size(877, 423)
        tlpDreapta.TabIndex = 1
        ' 
        ' dgvConstatari
        ' 
        dgvConstatari.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvConstatari.BackColor = SystemColors.Window
        dgvConstatari.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Clasă"
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn11.Key = "clasa"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Fel"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn12.Key = "fel"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.Width = 200
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Tabel"
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn13.Key = "tabel"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.Width = 150
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Coloană"
        KBotDataColumn14.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn14.Key = "coloana"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.Visible = False
        KBotDataColumn14.Width = 140
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Mesaj"
        KBotDataColumn15.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn15.Key = "mesaj"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.Visible = False
        KBotDataColumn15.Width = 620
        dgvConstatari.Columns.Add(KBotDataColumn11)
        dgvConstatari.Columns.Add(KBotDataColumn12)
        dgvConstatari.Columns.Add(KBotDataColumn13)
        dgvConstatari.Columns.Add(KBotDataColumn14)
        dgvConstatari.Columns.Add(KBotDataColumn15)
        dgvConstatari.Dock = DockStyle.Fill
        dgvConstatari.HeaderHeight = 26
        dgvConstatari.Location = New Point(4, 5)
        dgvConstatari.Margin = New Padding(4, 5, 4, 5)
        dgvConstatari.Name = "dgvConstatari"
        dgvConstatari.RowHeight = 24
        dgvConstatari.Size = New Size(492, 251)
        dgvConstatari.TabIndex = 0
        ' 
        ' rtbJurnal
        ' 
        tlpDreapta.SetColumnSpan(rtbJurnal, 2)
        rtbJurnal.Dock = DockStyle.Fill
        rtbJurnal.Location = New Point(4, 266)
        rtbJurnal.Margin = New Padding(4, 5, 4, 5)
        rtbJurnal.Name = "rtbJurnal"
        rtbJurnal.ReadOnly = True
        rtbJurnal.Size = New Size(869, 152)
        rtbJurnal.TabIndex = 1
        rtbJurnal.Text = ""
        rtbJurnal.WordWrap = False
        ' 
        ' rtbInfoRowConstatari
        ' 
        rtbInfoRowConstatari.BackColor = SystemColors.Control
        rtbInfoRowConstatari.Dock = DockStyle.Fill
        rtbInfoRowConstatari.Location = New Point(503, 3)
        rtbInfoRowConstatari.Name = "rtbInfoRowConstatari"
        rtbInfoRowConstatari.Size = New Size(371, 255)
        rtbInfoRowConstatari.TabIndex = 2
        rtbInfoRowConstatari.Text = ""
        ' 
        ' MigratorForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1505, 1044)
        ControlBox = False
        Controls.Add(tlpRoot)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(1505, 900)
        Name = "MigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — migrare Access ▸ MariaDB"
        tlpRoot.ResumeLayout(False)
        tlpRoot.PerformLayout()
        grpPython.ResumeLayout(False)
        tplPython.ResumeLayout(False)
        tplPython.PerformLayout()
        grpServer.ResumeLayout(False)
        grpServer.PerformLayout()
        tlpServer.ResumeLayout(False)
        tlpServer.PerformLayout()
        grpUnitate.ResumeLayout(False)
        CType(dgvUnitati, ComponentModel.ISupportInitialize).EndInit()
        pnlUnitateSus.ResumeLayout(False)
        pnlUnitateSus.PerformLayout()
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        grpTransfer.ResumeLayout(False)
        tlpTransfer.ResumeLayout(False)
        pnlButoane.ResumeLayout(False)
        tlpButoane.ResumeLayout(False)
        tlpGrile.ResumeLayout(False)
        CType(dgvTabele, ComponentModel.ISupportInitialize).EndInit()
        tlpDreapta.ResumeLayout(False)
        CType(dgvConstatari, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents rtbInfoRowConstatari As RichTextBox
    Friend WithEvents tlpButoane As TableLayoutPanel
    Friend WithEvents tplPython As TableLayoutPanel
    Friend WithEvents txtCheieApi As Controls.KBotTextField
    Friend WithEvents lblCheieApi As Label
    Friend WithEvents txtServerUrl As Controls.KBotTextField
    Friend WithEvents lblServerUrl As Label
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents capBar As Controls.KBotCaptionBar

End Class
