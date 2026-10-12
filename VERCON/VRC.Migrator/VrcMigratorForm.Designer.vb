<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VrcMigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents grpSurse As System.Windows.Forms.GroupBox
    Friend WithEvents tlpSurse As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblFolder As System.Windows.Forms.Label
    Friend WithEvents dgvFisiere As KBot.Controls.KBotDataView
    Friend WithEvents btnFolder As System.Windows.Forms.Button
    Friend WithEvents btnToate As System.Windows.Forms.Button
    Friend WithEvents btnNiciunul As System.Windows.Forms.Button
    Friend WithEvents btnCiteste As System.Windows.Forms.Button
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents grpRaport As System.Windows.Forms.GroupBox
    Friend WithEvents rtbRaport As System.Windows.Forms.RichTextBox
    Friend WithEvents grpPlan As System.Windows.Forms.GroupBox
    Friend WithEvents tlpPlan As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents tlpServer As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblGazda As System.Windows.Forms.Label
    Friend WithEvents txtGazda As KBot.Controls.KBotTextField
    Friend WithEvents lblPort As System.Windows.Forms.Label
    Friend WithEvents txtPort As KBot.Controls.KBotTextField
    Friend WithEvents lblUtilizator As System.Windows.Forms.Label
    Friend WithEvents txtUtilizator As KBot.Controls.KBotTextField
    Friend WithEvents lblParola As System.Windows.Forms.Label
    Friend WithEvents txtParola As KBot.Controls.KBotTextField
    Friend WithEvents lblDc As System.Windows.Forms.Label
    Friend WithEvents txtDc As KBot.Controls.KBotTextField
    Friend WithEvents lblSubunitate As System.Windows.Forms.Label
    Friend WithEvents txtSubunitate As KBot.Controls.KBotTextField
    Friend WithEvents dgvPlan As KBot.Controls.KBotDataView
    Friend WithEvents btnVerifica As System.Windows.Forms.Button
    Friend WithEvents btnExportaPlan As System.Windows.Forms.Button
    Friend WithEvents btnTesteaza As System.Windows.Forms.Button
    Friend WithEvents btnParteneri As System.Windows.Forms.Button
    Friend WithEvents btnMigreaza As System.Windows.Forms.Button
    Friend WithEvents btnOpreste As System.Windows.Forms.Button
    Friend WithEvents btnDiagnostic As System.Windows.Forms.Button
    Friend WithEvents sfdPlan As System.Windows.Forms.SaveFileDialog
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents fbdFolder As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents sfdRaport As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tipVrc As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Dim colSel As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colFisier As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colFormat As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colRol As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colAn As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colDimensiune As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colNota As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colPFisier As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colPAn As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colPGestiune As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colPTip As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colPNota As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        grpSurse = New System.Windows.Forms.GroupBox()
        tlpSurse = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblFolder = New System.Windows.Forms.Label()
        dgvFisiere = New KBot.Controls.KBotDataView()
        btnFolder = New System.Windows.Forms.Button()
        btnToate = New System.Windows.Forms.Button()
        btnNiciunul = New System.Windows.Forms.Button()
        btnCiteste = New System.Windows.Forms.Button()
        btnSalveaza = New System.Windows.Forms.Button()
        grpRaport = New System.Windows.Forms.GroupBox()
        rtbRaport = New System.Windows.Forms.RichTextBox()
        grpPlan = New System.Windows.Forms.GroupBox()
        tlpPlan = New Global.KBot.Controls.KBotTableLayoutPanel()
        tlpServer = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblGazda = New System.Windows.Forms.Label()
        txtGazda = New KBot.Controls.KBotTextField()
        lblPort = New System.Windows.Forms.Label()
        txtPort = New KBot.Controls.KBotTextField()
        lblUtilizator = New System.Windows.Forms.Label()
        txtUtilizator = New KBot.Controls.KBotTextField()
        lblParola = New System.Windows.Forms.Label()
        txtParola = New KBot.Controls.KBotTextField()
        lblDc = New System.Windows.Forms.Label()
        txtDc = New KBot.Controls.KBotTextField()
        lblSubunitate = New System.Windows.Forms.Label()
        txtSubunitate = New KBot.Controls.KBotTextField()
        dgvPlan = New KBot.Controls.KBotDataView()
        btnVerifica = New System.Windows.Forms.Button()
        btnExportaPlan = New System.Windows.Forms.Button()
        btnTesteaza = New System.Windows.Forms.Button()
        btnParteneri = New System.Windows.Forms.Button()
        btnMigreaza = New System.Windows.Forms.Button()
        btnOpreste = New System.Windows.Forms.Button()
        btnDiagnostic = New System.Windows.Forms.Button()
        sfdPlan = New System.Windows.Forms.SaveFileDialog()
        lblStare = New System.Windows.Forms.Label()
        fbdFolder = New System.Windows.Forms.FolderBrowserDialog()
        sfdRaport = New System.Windows.Forms.SaveFileDialog()
        tipVrc = New KBot.Controls.KBotToolTip(components)
        CType(dgvFisiere, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvPlan, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpRoot.SuspendLayout()
        grpPlan.SuspendLayout()
        tlpPlan.SuspendLayout()
        tlpServer.SuspendLayout()
        grpSurse.SuspendLayout()
        tlpSurse.SuspendLayout()
        grpRaport.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(grpSurse, 0, 0)
        tlpRoot.Controls.Add(grpPlan, 0, 1)
        tlpRoot.Controls.Add(grpRaport, 0, 2)
        tlpRoot.Controls.Add(lblStare, 0, 3)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 4
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 230.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 300.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tlpRoot.Size = New Size(1100, 900)
        tlpRoot.TabIndex = 0
        '
        ' grpSurse
        '
        grpSurse.Controls.Add(tlpSurse)
        grpSurse.Dock = DockStyle.Fill
        grpSurse.Name = "grpSurse"
        grpSurse.TabIndex = 0
        grpSurse.TabStop = False
        grpSurse.Text = "Folderul cu bazele subunității (tz_aaaa și tz_2100, .accdb și .mdb)"
        '
        ' tlpSurse
        '
        tlpSurse.ColumnCount = 2
        tlpSurse.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpSurse.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0F))
        tlpSurse.Controls.Add(lblFolder, 0, 0)
        tlpSurse.SetColumnSpan(lblFolder, 2)
        tlpSurse.Controls.Add(dgvFisiere, 0, 1)
        tlpSurse.SetRowSpan(dgvFisiere, 5)
        tlpSurse.Controls.Add(btnFolder, 1, 1)
        tlpSurse.Controls.Add(btnCiteste, 1, 2)
        tlpSurse.Controls.Add(btnToate, 1, 3)
        tlpSurse.Controls.Add(btnNiciunul, 1, 4)
        tlpSurse.Controls.Add(btnSalveaza, 1, 5)
        tlpSurse.Dock = DockStyle.Fill
        tlpSurse.Name = "tlpSurse"
        tlpSurse.RowCount = 6
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 20.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 20.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 20.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 20.0F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 20.0F))
        tlpSurse.TabIndex = 0
        '
        ' lblFolder
        '
        lblFolder.Dock = DockStyle.Fill
        lblFolder.Name = "lblFolder"
        lblFolder.TabIndex = 0
        lblFolder.Text = "Niciun folder ales."
        lblFolder.TextAlign = ContentAlignment.MiddleLeft
        '
        ' dgvFisiere
        '
        dgvFisiere.BackColor = SystemColors.Window
        colSel.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        colSel.HeaderText = "Citește"
        colSel.Key = "sel"
        colSel.Width = 70
        colFisier.HeaderText = "Fișier"
        colFisier.Key = "fisier"
        colFisier.ReadOnly = True
        colFisier.Width = 190
        colFormat.HeaderText = "Format"
        colFormat.Key = "format"
        colFormat.ReadOnly = True
        colFormat.Width = 70
        colRol.HeaderText = "Rol (după nume)"
        colRol.Key = "rol"
        colRol.ReadOnly = True
        colRol.Width = 220
        colAn.HeaderText = "An"
        colAn.Key = "an"
        colAn.ReadOnly = True
        colAn.TextAlign = ContentAlignment.MiddleRight
        colAn.Width = 60
        colDimensiune.HeaderText = "Dimensiune"
        colDimensiune.Key = "dimensiune"
        colDimensiune.ReadOnly = True
        colDimensiune.TextAlign = ContentAlignment.MiddleRight
        colDimensiune.Width = 100
        colNota.HeaderText = "Observații"
        colNota.Key = "nota"
        colNota.ReadOnly = True
        colNota.Width = 260
        dgvFisiere.Columns.Add(colSel)
        dgvFisiere.Columns.Add(colFisier)
        dgvFisiere.Columns.Add(colFormat)
        dgvFisiere.Columns.Add(colRol)
        dgvFisiere.Columns.Add(colAn)
        dgvFisiere.Columns.Add(colDimensiune)
        dgvFisiere.Columns.Add(colNota)
        dgvFisiere.Dock = DockStyle.Fill
        dgvFisiere.HeaderHeight = 26
        dgvFisiere.Name = "dgvFisiere"
        dgvFisiere.RowHeight = 24
        dgvFisiere.TabIndex = 1
        '
        ' btnFolder
        '
        btnFolder.Dock = DockStyle.Fill
        btnFolder.Name = "btnFolder"
        btnFolder.TabIndex = 2
        btnFolder.Text = "Alege folder…"
        btnFolder.UseVisualStyleBackColor = True
        '
        ' btnToate
        '
        btnToate.Dock = DockStyle.Fill
        btnToate.Name = "btnToate"
        btnToate.TabIndex = 4
        btnToate.Text = "Bifează toate"
        btnToate.UseVisualStyleBackColor = True
        '
        ' btnNiciunul
        '
        btnNiciunul.Dock = DockStyle.Fill
        btnNiciunul.Name = "btnNiciunul"
        btnNiciunul.TabIndex = 5
        btnNiciunul.Text = "Debifează toate"
        btnNiciunul.UseVisualStyleBackColor = True
        '
        ' btnCiteste
        '
        btnCiteste.Dock = DockStyle.Fill
        btnCiteste.Name = "btnCiteste"
        btnCiteste.TabIndex = 3
        btnCiteste.Text = "Citește bifatele"
        btnCiteste.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.TabIndex = 6
        btnSalveaza.Text = "Salvează raportul…"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' grpPlan
        '
        grpPlan.Controls.Add(tlpPlan)
        grpPlan.Dock = DockStyle.Fill
        grpPlan.Name = "grpPlan"
        grpPlan.TabIndex = 1
        grpPlan.TabStop = False
        grpPlan.Text = "Plan de asociere și transfer (o singură dată pe subunitate)"
        '
        ' tlpPlan
        '
        tlpPlan.ColumnCount = 2
        tlpPlan.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpPlan.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0F))
        tlpPlan.Controls.Add(tlpServer, 0, 0)
        tlpPlan.SetColumnSpan(tlpServer, 2)
        tlpPlan.Controls.Add(dgvPlan, 0, 1)
        tlpPlan.SetRowSpan(dgvPlan, 7)
        tlpPlan.Controls.Add(btnVerifica, 1, 1)
        tlpPlan.Controls.Add(btnExportaPlan, 1, 2)
        tlpPlan.Controls.Add(btnTesteaza, 1, 3)
        tlpPlan.Controls.Add(btnParteneri, 1, 4)
        tlpPlan.Controls.Add(btnMigreaza, 1, 5)
        tlpPlan.Controls.Add(btnOpreste, 1, 6)
        tlpPlan.Controls.Add(btnDiagnostic, 1, 7)
        tlpPlan.Dock = DockStyle.Fill
        tlpPlan.Name = "tlpPlan"
        tlpPlan.RowCount = 8
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.RowStyles.Add(New RowStyle(SizeType.Percent, 14.28F))
        tlpPlan.TabIndex = 0
        '
        ' tlpServer
        '
        tlpServer.ColumnCount = 12
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 50.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 50.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 95.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpServer.Controls.Add(lblGazda, 0, 0)
        tlpServer.Controls.Add(txtGazda, 1, 0)
        tlpServer.Controls.Add(lblPort, 2, 0)
        tlpServer.Controls.Add(txtPort, 3, 0)
        tlpServer.Controls.Add(lblUtilizator, 4, 0)
        tlpServer.Controls.Add(txtUtilizator, 5, 0)
        tlpServer.Controls.Add(lblParola, 6, 0)
        tlpServer.Controls.Add(txtParola, 7, 0)
        tlpServer.Controls.Add(lblDc, 8, 0)
        tlpServer.Controls.Add(txtDc, 9, 0)
        tlpServer.Controls.Add(lblSubunitate, 10, 0)
        tlpServer.Controls.Add(txtSubunitate, 11, 0)
        tlpServer.Dock = DockStyle.Fill
        tlpServer.Name = "tlpServer"
        tlpServer.RowCount = 1
        tlpServer.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpServer.TabIndex = 0
        '
        ' lblGazda
        '
        lblGazda.Dock = DockStyle.Fill
        lblGazda.Name = "lblGazda"
        lblGazda.TabIndex = 0
        lblGazda.Text = "Gazdă"
        lblGazda.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtGazda
        '
        txtGazda.BackColor = Color.Transparent
        txtGazda.Dock = DockStyle.Fill
        txtGazda.Name = "txtGazda"
        txtGazda.TabIndex = 1
        '
        ' lblPort
        '
        lblPort.Dock = DockStyle.Fill
        lblPort.Name = "lblPort"
        lblPort.TabIndex = 2
        lblPort.Text = "Port"
        lblPort.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtPort
        '
        txtPort.BackColor = Color.Transparent
        txtPort.Dock = DockStyle.Fill
        txtPort.Name = "txtPort"
        txtPort.TabIndex = 3
        '
        ' lblUtilizator
        '
        lblUtilizator.Dock = DockStyle.Fill
        lblUtilizator.Name = "lblUtilizator"
        lblUtilizator.TabIndex = 4
        lblUtilizator.Text = "Utilizator"
        lblUtilizator.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtUtilizator
        '
        txtUtilizator.BackColor = Color.Transparent
        txtUtilizator.Dock = DockStyle.Fill
        txtUtilizator.Name = "txtUtilizator"
        txtUtilizator.TabIndex = 5
        '
        ' lblParola
        '
        lblParola.Dock = DockStyle.Fill
        lblParola.Name = "lblParola"
        lblParola.TabIndex = 6
        lblParola.Text = "Parolă"
        lblParola.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParola
        '
        txtParola.BackColor = Color.Transparent
        txtParola.Dock = DockStyle.Fill
        txtParola.Name = "txtParola"
        txtParola.TabIndex = 7
        txtParola.UseSystemPasswordChar = True
        '
        ' lblDc
        '
        lblDc.Dock = DockStyle.Fill
        lblDc.Name = "lblDc"
        lblDc.TabIndex = 8
        lblDc.Text = "DC destinație"
        lblDc.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDc
        '
        txtDc.BackColor = Color.Transparent
        txtDc.Dock = DockStyle.Fill
        txtDc.Name = "txtDc"
        txtDc.TabIndex = 9
        '
        ' lblSubunitate
        '
        lblSubunitate.Dock = DockStyle.Fill
        lblSubunitate.Name = "lblSubunitate"
        lblSubunitate.TabIndex = 10
        lblSubunitate.Text = "Subunitate"
        lblSubunitate.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtSubunitate
        '
        txtSubunitate.BackColor = Color.Transparent
        txtSubunitate.Dock = DockStyle.Fill
        txtSubunitate.Name = "txtSubunitate"
        txtSubunitate.TabIndex = 11
        '
        ' dgvPlan
        '
        dgvPlan.BackColor = SystemColors.Window
        colPFisier.HeaderText = "Fișier"
        colPFisier.Key = "fisier"
        colPFisier.ReadOnly = True
        colPFisier.Width = 170
        colPAn.HeaderText = "An exercițiu"
        colPAn.Key = "an"
        colPAn.TextAlign = ContentAlignment.MiddleRight
        colPAn.Width = 90
        colPGestiune.HeaderText = "Gestiune (denumire)"
        colPGestiune.Key = "gestiune"
        colPGestiune.Width = 200
        colPTip.ColumnType = KBot.Controls.KBotColumnType.Combo
        colPTip.HeaderText = "Tip"
        colPTip.Key = "tip"
        colPTip.Width = 120
        colPNota.HeaderText = "Propunere / observații"
        colPNota.Key = "nota"
        colPNota.ReadOnly = True
        colPNota.Width = 320
        dgvPlan.Columns.Add(colPFisier)
        dgvPlan.Columns.Add(colPAn)
        dgvPlan.Columns.Add(colPGestiune)
        dgvPlan.Columns.Add(colPTip)
        dgvPlan.Columns.Add(colPNota)
        dgvPlan.Dock = DockStyle.Fill
        dgvPlan.HeaderHeight = 26
        dgvPlan.Name = "dgvPlan"
        dgvPlan.RowHeight = 24
        dgvPlan.TabIndex = 1
        '
        ' btnVerifica
        '
        btnVerifica.Dock = DockStyle.Fill
        btnVerifica.Name = "btnVerifica"
        btnVerifica.TabIndex = 2
        btnVerifica.Text = "Verifică planul"
        btnVerifica.UseVisualStyleBackColor = True
        '
        ' btnExportaPlan
        '
        btnExportaPlan.Dock = DockStyle.Fill
        btnExportaPlan.Name = "btnExportaPlan"
        btnExportaPlan.TabIndex = 3
        btnExportaPlan.Text = "Exportă planul…"
        btnExportaPlan.UseVisualStyleBackColor = True
        '
        ' btnTesteaza
        '
        btnTesteaza.Dock = DockStyle.Fill
        btnTesteaza.Name = "btnTesteaza"
        btnTesteaza.TabIndex = 4
        btnTesteaza.Text = "Testează"
        btnTesteaza.UseVisualStyleBackColor = True
        '
        ' btnParteneri
        '
        btnParteneri.Dock = DockStyle.Fill
        btnParteneri.Name = "btnParteneri"
        btnParteneri.TabIndex = 8
        btnParteneri.Text = "Parteneri ANAF…"
        btnParteneri.UseVisualStyleBackColor = True
        '
        ' btnMigreaza
        '
        btnMigreaza.Dock = DockStyle.Fill
        btnMigreaza.Name = "btnMigreaza"
        btnMigreaza.TabIndex = 5
        btnMigreaza.Text = "Migrează"
        btnMigreaza.UseVisualStyleBackColor = True
        '
        ' btnOpreste
        '
        btnOpreste.Dock = DockStyle.Fill
        btnOpreste.Name = "btnOpreste"
        btnOpreste.TabIndex = 6
        btnOpreste.Text = "Oprește"
        btnOpreste.UseVisualStyleBackColor = True
        '
        ' btnDiagnostic
        '
        btnDiagnostic.Dock = DockStyle.Fill
        btnDiagnostic.Name = "btnDiagnostic"
        btnDiagnostic.TabIndex = 7
        btnDiagnostic.Text = "Diagnostic"
        btnDiagnostic.UseVisualStyleBackColor = True
        '
        ' grpRaport
        '
        grpRaport.Controls.Add(rtbRaport)
        grpRaport.Dock = DockStyle.Fill
        grpRaport.Name = "grpRaport"
        grpRaport.TabIndex = 2
        grpRaport.TabStop = False
        grpRaport.Text = "Raport / jurnal"
        '
        ' rtbRaport
        '
        rtbRaport.Dock = DockStyle.Fill
        rtbRaport.Font = New Font("Consolas", 9.0F)
        rtbRaport.Name = "rtbRaport"
        rtbRaport.ReadOnly = True
        rtbRaport.TabIndex = 0
        rtbRaport.Text = ""
        rtbRaport.WordWrap = False
        '
        ' lblStare
        '
        lblStare.Dock = DockStyle.Fill
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 3
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' fbdFolder
        '
        fbdFolder.Description = "Alegeți folderul cu bazele Access ale subunității"
        fbdFolder.UseDescriptionForTitle = True
        '
        ' sfdRaport
        '
        sfdRaport.DefaultExt = "md"
        sfdRaport.Filter = "Markdown (*.md)|*.md"
        sfdRaport.Title = "Salvați raportul de profilare"
        '
        ' sfdPlan
        '
        sfdPlan.DefaultExt = "json"
        sfdPlan.Filter = "Plan (*.json)|*.json"
        sfdPlan.Title = "Exportați planul de asociere"
        '
        ' tipVrc
        '
        tipVrc.SetToolTipHeader(txtDc, "Baza destinație (DC)")
        tipVrc.SetToolTipText(txtDc, "Se propune UNIT.DC din fișiere. Se poate schimba pentru o primă rulare pe o bază de test (de exemplu 000_DEMO); transferul se scrie în baza din acest câmp.")
        tipVrc.SetToolTipHeader(btnVerifica, "Verifică planul")
        tipVrc.SetToolTipText(btnVerifica, "Verifică asocierea fișier → gestiune / tip / an și arată motivele concrete pentru care transferul nu poate începe.")
        tipVrc.SetToolTipHeader(btnTesteaza, "Testează")
        tipVrc.SetToolTipText(btnTesteaza, "Verifică serverul: baza DC, tabelele VC_, subunitatea nouă și furnizorii fără partener. Nu scrie nimic.")
        tipVrc.SetToolTipHeader(btnParteneri, "Parteneri din ANAF")
        tipVrc.SetToolTipText(btnParteneri, "Pentru furnizorii fără partener cu același cod fiscal: interoghează ANAF și vă lasă să bifați sursa-sector a fiecăruia. Partenerii pregătiți se scriu în Parteneri abia la «Migrează», în aceeași tranzacție cu transferul. Aici nu se scrie nimic.")
        tipVrc.SetToolTipHeader(btnMigreaza, "Migrează")
        tipVrc.SetToolTipText(btnMigreaza, "Transferul unic al subunității, într-o singură tranzacție. După COMMIT subunitatea nu mai poate fi importată.")
        tipVrc.SetToolTipHeader(btnOpreste, "Oprește")
        tipVrc.SetToolTipText(btnOpreste, "Cere oprirea înainte de COMMIT. După trimiterea COMMIT-ului oprirea nu mai este posibilă.")
        tipVrc.SetToolTipHeader(btnDiagnostic, "Diagnostic")
        tipVrc.SetToolTipText(btnDiagnostic, "Citește de pe server dacă transferul subunității există (marker, rânduri). Nu inserează nimic; folosiți-l după un răspuns pierdut.")
        tipVrc.SetToolTipHeader(btnFolder, "Alege folderul")
        tipVrc.SetToolTipText(btnFolder, "Se citește tot folderul: fișierele tz_aaaa (câte unul pe an) și tz_2100 (bază specială OI sau MF), atât .accdb cât și .mdb. Fișierul tz.mdb nu se ia niciodată.")
        tipVrc.SetToolTipHeader(btnCiteste, "Citește bifatele")
        tipVrc.SetToolTipText(btnCiteste, "Citește bazele bifate în mod doar-citire și face raportul. Nu scrie nimic în Access și nu atinge serverul.")
        tipVrc.SetToolTipHeader(btnSalveaza, "Salvează raportul")
        tipVrc.SetToolTipText(btnSalveaza, "Salvează raportul ca fișier Markdown. Conține numere și intervale, nu rânduri de date.")
        '
        ' VrcMigratorForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1100, 900)
        Controls.Add(tlpRoot)
        MinimumSize = New Size(1000, 700)
        Name = "VrcMigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "VRC.Migrator — Access VERCON ▸ MariaDB"
        CType(dgvFisiere, System.ComponentModel.ISupportInitialize).EndInit()
        CType(dgvPlan, System.ComponentModel.ISupportInitialize).EndInit()
        tlpRoot.ResumeLayout(False)
        tlpServer.ResumeLayout(False)
        tlpPlan.ResumeLayout(False)
        grpPlan.ResumeLayout(False)
        grpSurse.ResumeLayout(False)
        tlpSurse.ResumeLayout(False)
        grpRaport.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
