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

    ' --- region 1: files -------------------------------------------------------
    Friend WithEvents grpFisiere As System.Windows.Forms.GroupBox
    Friend WithEvents tlpFisiere As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblRegistru As System.Windows.Forms.Label
    Friend WithEvents txtRegistru As KBot.Controls.KBotTextField
    Friend WithEvents btnRasfoireRegistru As System.Windows.Forms.Button
    Friend WithEvents lblParolaUnitati As System.Windows.Forms.Label
    Friend WithEvents txtParolaUnitati As KBot.Controls.KBotTextField
    Friend WithEvents lblParolaForexe As System.Windows.Forms.Label
    Friend WithEvents txtParolaForexe As KBot.Controls.KBotTextField
    Friend WithEvents lblJurnal As System.Windows.Forms.Label
    Friend WithEvents txtJurnal As KBot.Controls.KBotTextField
    Friend WithEvents btnRasfoireJurnal As System.Windows.Forms.Button

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
        Me.components = New System.ComponentModel.Container()

        Me.tlpRoot = New System.Windows.Forms.TableLayoutPanel()
        Me.grpFisiere = New System.Windows.Forms.GroupBox()
        Me.tlpFisiere = New System.Windows.Forms.TableLayoutPanel()
        Me.lblRegistru = New System.Windows.Forms.Label()
        Me.txtRegistru = New KBot.Controls.KBotTextField()
        Me.btnRasfoireRegistru = New System.Windows.Forms.Button()
        Me.lblParolaUnitati = New System.Windows.Forms.Label()
        Me.txtParolaUnitati = New KBot.Controls.KBotTextField()
        Me.lblParolaForexe = New System.Windows.Forms.Label()
        Me.txtParolaForexe = New KBot.Controls.KBotTextField()
        Me.lblJurnal = New System.Windows.Forms.Label()
        Me.txtJurnal = New KBot.Controls.KBotTextField()
        Me.btnRasfoireJurnal = New System.Windows.Forms.Button()
        Me.grpServer = New System.Windows.Forms.GroupBox()
        Me.tlpServer = New System.Windows.Forms.TableLayoutPanel()
        Me.lblGazda = New System.Windows.Forms.Label()
        Me.txtGazda = New KBot.Controls.KBotTextField()
        Me.lblPort = New System.Windows.Forms.Label()
        Me.txtPort = New KBot.Controls.KBotTextField()
        Me.lblUtilizator = New System.Windows.Forms.Label()
        Me.txtUtilizator = New KBot.Controls.KBotTextField()
        Me.lblParolaServer = New System.Windows.Forms.Label()
        Me.txtParolaServer = New KBot.Controls.KBotTextField()
        Me.btnTesteaza = New System.Windows.Forms.Button()
        Me.lblStareServer = New System.Windows.Forms.Label()
        Me.grpUnitate = New System.Windows.Forms.GroupBox()
        Me.pnlUnitateSus = New System.Windows.Forms.Panel()
        Me.lblDc = New System.Windows.Forms.Label()
        Me.cboDc = New System.Windows.Forms.ComboBox()
        Me.btnCitesteRegistru = New System.Windows.Forms.Button()
        Me.lblBazaTinta = New System.Windows.Forms.Label()
        Me.dgvUnitati = New KBot.Controls.KBotDataView()
        Me.grpTransfer = New System.Windows.Forms.GroupBox()
        Me.tlpTransfer = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlButoane = New System.Windows.Forms.Panel()
        Me.btnVerifica = New System.Windows.Forms.Button()
        Me.btnTransfera = New System.Windows.Forms.Button()
        Me.btnOpreste = New System.Windows.Forms.Button()
        Me.prgTransfer = New System.Windows.Forms.ProgressBar()
        Me.tlpGrile = New System.Windows.Forms.TableLayoutPanel()
        Me.dgvTabele = New KBot.Controls.KBotDataView()
        Me.tlpDreapta = New System.Windows.Forms.TableLayoutPanel()
        Me.dgvConstatari = New KBot.Controls.KBotDataView()
        Me.rtbJurnal = New System.Windows.Forms.RichTextBox()
        Me.tipMigrator = New KBot.Controls.KBotToolTip(Me.components)

        CType(Me.dgvUnitati, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvTabele, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgvConstatari, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tlpRoot.SuspendLayout()
        Me.grpFisiere.SuspendLayout()
        Me.tlpFisiere.SuspendLayout()
        Me.grpServer.SuspendLayout()
        Me.tlpServer.SuspendLayout()
        Me.grpUnitate.SuspendLayout()
        Me.pnlUnitateSus.SuspendLayout()
        Me.grpTransfer.SuspendLayout()
        Me.tlpTransfer.SuspendLayout()
        Me.pnlButoane.SuspendLayout()
        Me.tlpGrile.SuspendLayout()
        Me.tlpDreapta.SuspendLayout()
        Me.SuspendLayout()

        '
        ' tlpRoot
        '
        Me.tlpRoot.ColumnCount = 1
        Me.tlpRoot.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpRoot.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpRoot.Name = "tlpRoot"
        Me.tlpRoot.Padding = New System.Windows.Forms.Padding(12, 8, 12, 12)
        Me.tlpRoot.RowCount = 4
        Me.tlpRoot.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        Me.tlpRoot.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        Me.tlpRoot.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.0!))
        Me.tlpRoot.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.0!))
        Me.tlpRoot.TabIndex = 0
        ' Reverse dock order inside the root: Fill first, then the rows above it.
        Me.tlpRoot.Controls.Add(Me.grpFisiere, 0, 0)
        Me.tlpRoot.Controls.Add(Me.grpServer, 0, 1)
        Me.tlpRoot.Controls.Add(Me.grpUnitate, 0, 2)
        Me.tlpRoot.Controls.Add(Me.grpTransfer, 0, 3)

        '
        ' grpFisiere
        '
        Me.grpFisiere.AutoSize = True
        Me.grpFisiere.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.grpFisiere.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpFisiere.Name = "grpFisiere"
        Me.grpFisiere.Padding = New System.Windows.Forms.Padding(10, 6, 10, 10)
        Me.grpFisiere.TabIndex = 0
        Me.grpFisiere.TabStop = False
        Me.grpFisiere.Text = "Fișiere"
        Me.grpFisiere.Controls.Add(Me.tlpFisiere)

        '
        ' tlpFisiere
        '
        Me.tlpFisiere.AutoSize = True
        Me.tlpFisiere.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.tlpFisiere.ColumnCount = 3
        Me.tlpFisiere.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190.0!))
        Me.tlpFisiere.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpFisiere.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0!))
        Me.tlpFisiere.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpFisiere.Name = "tlpFisiere"
        Me.tlpFisiere.RowCount = 4
        Me.tlpFisiere.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpFisiere.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpFisiere.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpFisiere.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpFisiere.TabIndex = 0
        Me.tlpFisiere.Controls.Add(Me.lblRegistru, 0, 0)
        Me.tlpFisiere.Controls.Add(Me.txtRegistru, 1, 0)
        Me.tlpFisiere.Controls.Add(Me.btnRasfoireRegistru, 2, 0)
        Me.tlpFisiere.Controls.Add(Me.lblParolaUnitati, 0, 1)
        Me.tlpFisiere.Controls.Add(Me.txtParolaUnitati, 1, 1)
        Me.tlpFisiere.Controls.Add(Me.lblParolaForexe, 0, 2)
        Me.tlpFisiere.Controls.Add(Me.txtParolaForexe, 1, 2)
        Me.tlpFisiere.Controls.Add(Me.lblJurnal, 0, 3)
        Me.tlpFisiere.Controls.Add(Me.txtJurnal, 1, 3)
        Me.tlpFisiere.Controls.Add(Me.btnRasfoireJurnal, 2, 3)

        Me.lblRegistru.AutoSize = True
        Me.lblRegistru.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblRegistru.Name = "lblRegistru"
        Me.lblRegistru.TabIndex = 0
        Me.lblRegistru.Text = "Registrul AVACONT (cale.accdb)"
        Me.lblRegistru.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        Me.txtRegistru.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtRegistru.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtRegistru.Name = "txtRegistru"
        Me.txtRegistru.PlaceholderText = "C:\AVACONT\cale.accdb"
        Me.txtRegistru.TabIndex = 1

        Me.btnRasfoireRegistru.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnRasfoireRegistru.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.btnRasfoireRegistru.Name = "btnRasfoireRegistru"
        Me.btnRasfoireRegistru.TabIndex = 2
        Me.btnRasfoireRegistru.Text = "Răsfoiește…"
        Me.btnRasfoireRegistru.UseVisualStyleBackColor = True

        Me.lblParolaUnitati.AutoSize = True
        Me.lblParolaUnitati.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblParolaUnitati.Name = "lblParolaUnitati"
        Me.lblParolaUnitati.TabIndex = 3
        Me.lblParolaUnitati.Text = "Parolă fișiere de unitate"
        Me.lblParolaUnitati.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        Me.txtParolaUnitati.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtParolaUnitati.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtParolaUnitati.Name = "txtParolaUnitati"
        Me.txtParolaUnitati.PlaceholderText = "lăsați gol dacă nu sunt protejate"
        Me.txtParolaUnitati.TabIndex = 4
        Me.txtParolaUnitati.UseSystemPasswordChar = True

        Me.lblParolaForexe.AutoSize = True
        Me.lblParolaForexe.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblParolaForexe.Name = "lblParolaForexe"
        Me.lblParolaForexe.TabIndex = 5
        Me.lblParolaForexe.Text = "Parolă fișiere FOREXE"
        Me.lblParolaForexe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        Me.txtParolaForexe.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtParolaForexe.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtParolaForexe.Name = "txtParolaForexe"
        Me.txtParolaForexe.PlaceholderText = "lăsați gol dacă nu sunt protejate"
        Me.txtParolaForexe.TabIndex = 6
        Me.txtParolaForexe.UseSystemPasswordChar = True

        Me.lblJurnal.AutoSize = True
        Me.lblJurnal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblJurnal.Name = "lblJurnal"
        Me.lblJurnal.TabIndex = 7
        Me.lblJurnal.Text = "Dosarul jurnalului SQL"
        Me.lblJurnal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        Me.txtJurnal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtJurnal.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtJurnal.Name = "txtJurnal"
        Me.txtJurnal.TabIndex = 8

        Me.btnRasfoireJurnal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnRasfoireJurnal.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.btnRasfoireJurnal.Name = "btnRasfoireJurnal"
        Me.btnRasfoireJurnal.TabIndex = 9
        Me.btnRasfoireJurnal.Text = "Răsfoiește…"
        Me.btnRasfoireJurnal.UseVisualStyleBackColor = True

        '
        ' grpServer
        '
        Me.grpServer.AutoSize = True
        Me.grpServer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.grpServer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpServer.Name = "grpServer"
        Me.grpServer.Padding = New System.Windows.Forms.Padding(10, 6, 10, 10)
        Me.grpServer.TabIndex = 1
        Me.grpServer.TabStop = False
        Me.grpServer.Text = "Server MariaDB"
        Me.grpServer.Controls.Add(Me.tlpServer)

        Me.tlpServer.AutoSize = True
        Me.tlpServer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.tlpServer.ColumnCount = 4
        Me.tlpServer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190.0!))
        Me.tlpServer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tlpServer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150.0!))
        Me.tlpServer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tlpServer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpServer.Name = "tlpServer"
        Me.tlpServer.RowCount = 3
        Me.tlpServer.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpServer.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpServer.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42.0!))
        Me.tlpServer.TabIndex = 0
        Me.tlpServer.Controls.Add(Me.lblGazda, 0, 0)
        Me.tlpServer.Controls.Add(Me.txtGazda, 1, 0)
        Me.tlpServer.Controls.Add(Me.lblPort, 2, 0)
        Me.tlpServer.Controls.Add(Me.txtPort, 3, 0)
        Me.tlpServer.Controls.Add(Me.lblUtilizator, 0, 1)
        Me.tlpServer.Controls.Add(Me.txtUtilizator, 1, 1)
        Me.tlpServer.Controls.Add(Me.lblParolaServer, 2, 1)
        Me.tlpServer.Controls.Add(Me.txtParolaServer, 3, 1)
        Me.tlpServer.Controls.Add(Me.btnTesteaza, 0, 2)
        Me.tlpServer.Controls.Add(Me.lblStareServer, 1, 2)
        Me.tlpServer.SetColumnSpan(Me.lblStareServer, 3)

        Me.lblGazda.AutoSize = True
        Me.lblGazda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblGazda.Name = "lblGazda"
        Me.lblGazda.TabIndex = 0
        Me.lblGazda.Text = "Gazdă"
        Me.lblGazda.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        Me.txtGazda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtGazda.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtGazda.Name = "txtGazda"
        Me.txtGazda.TabIndex = 1

        Me.lblPort.AutoSize = True
        Me.lblPort.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblPort.Name = "lblPort"
        Me.lblPort.TabIndex = 2
        Me.lblPort.Text = "Port"
        Me.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight

        Me.txtPort.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtPort.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtPort.Name = "txtPort"
        Me.txtPort.TabIndex = 3

        Me.lblUtilizator.AutoSize = True
        Me.lblUtilizator.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblUtilizator.Name = "lblUtilizator"
        Me.lblUtilizator.TabIndex = 4
        Me.lblUtilizator.Text = "Utilizator administrator"
        Me.lblUtilizator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        Me.txtUtilizator.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtUtilizator.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtUtilizator.Name = "txtUtilizator"
        Me.txtUtilizator.TabIndex = 5

        Me.lblParolaServer.AutoSize = True
        Me.lblParolaServer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblParolaServer.Name = "lblParolaServer"
        Me.lblParolaServer.TabIndex = 6
        Me.lblParolaServer.Text = "Parolă"
        Me.lblParolaServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight

        Me.txtParolaServer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtParolaServer.Margin = New System.Windows.Forms.Padding(3, 3, 3, 3)
        Me.txtParolaServer.Name = "txtParolaServer"
        Me.txtParolaServer.TabIndex = 7
        Me.txtParolaServer.UseSystemPasswordChar = True

        Me.btnTesteaza.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnTesteaza.Margin = New System.Windows.Forms.Padding(3, 5, 3, 5)
        Me.btnTesteaza.Name = "btnTesteaza"
        Me.btnTesteaza.TabIndex = 8
        Me.btnTesteaza.Text = "Testează conexiunea"
        Me.btnTesteaza.UseVisualStyleBackColor = True

        Me.lblStareServer.AutoSize = True
        Me.lblStareServer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblStareServer.Name = "lblStareServer"
        Me.lblStareServer.TabIndex = 9
        Me.lblStareServer.Text = "Neconectat."
        Me.lblStareServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft

        '
        ' grpUnitate
        '
        Me.grpUnitate.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpUnitate.Name = "grpUnitate"
        Me.grpUnitate.Padding = New System.Windows.Forms.Padding(10, 6, 10, 10)
        Me.grpUnitate.TabIndex = 2
        Me.grpUnitate.TabStop = False
        Me.grpUnitate.Text = "Unitate"
        ' Reverse dock order: Fill first, then Top.
        Me.grpUnitate.Controls.Add(Me.dgvUnitati)
        Me.grpUnitate.Controls.Add(Me.pnlUnitateSus)

        Me.pnlUnitateSus.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlUnitateSus.Height = 44
        Me.pnlUnitateSus.Name = "pnlUnitateSus"
        Me.pnlUnitateSus.TabIndex = 0
        Me.pnlUnitateSus.Controls.Add(Me.lblDc)
        Me.pnlUnitateSus.Controls.Add(Me.cboDc)
        Me.pnlUnitateSus.Controls.Add(Me.btnCitesteRegistru)
        Me.pnlUnitateSus.Controls.Add(Me.lblBazaTinta)

        Me.lblDc.AutoSize = True
        Me.lblDc.Location = New System.Drawing.Point(3, 12)
        Me.lblDc.Name = "lblDc"
        Me.lblDc.Size = New System.Drawing.Size(30, 20)
        Me.lblDc.TabIndex = 0
        Me.lblDc.Text = "DC"

        Me.cboDc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDc.Location = New System.Drawing.Point(45, 8)
        Me.cboDc.Name = "cboDc"
        Me.cboDc.Size = New System.Drawing.Size(220, 28)
        Me.cboDc.TabIndex = 1

        Me.btnCitesteRegistru.Location = New System.Drawing.Point(280, 6)
        Me.btnCitesteRegistru.Name = "btnCitesteRegistru"
        Me.btnCitesteRegistru.Size = New System.Drawing.Size(150, 32)
        Me.btnCitesteRegistru.TabIndex = 2
        Me.btnCitesteRegistru.Text = "Citește registrul"
        Me.btnCitesteRegistru.UseVisualStyleBackColor = True

        Me.lblBazaTinta.AutoSize = True
        Me.lblBazaTinta.Location = New System.Drawing.Point(450, 12)
        Me.lblBazaTinta.Name = "lblBazaTinta"
        Me.lblBazaTinta.Size = New System.Drawing.Size(200, 20)
        Me.lblBazaTinta.TabIndex = 3
        Me.lblBazaTinta.Text = "Baza-țintă: (necunoscută)"

        Me.dgvUnitati.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvUnitati.Name = "dgvUnitati"
        Me.dgvUnitati.TabIndex = 1
        Me.dgvUnitati.AddColumn("bifa", "Transferă", KBot.Controls.KBotColumnType.CheckBox, 90)
        Me.dgvUnitati.AddColumn("id", "IdUnitate", KBot.Controls.KBotColumnType.Text, 90)
        Me.dgvUnitati.AddColumn("nume", "Unitate", KBot.Controls.KBotColumnType.Text, 240)
        Me.dgvUnitati.AddColumn("sursa", "Sursă", KBot.Controls.KBotColumnType.Text, 70)
        Me.dgvUnitati.AddColumn("nomenclator", "Fișier nomenclatoare", KBot.Controls.KBotColumnType.Text, 300)
        Me.dgvUnitati.AddColumn("forexe", "Fișier FOREXE", KBot.Controls.KBotColumnType.Text, 300)

        '
        ' grpTransfer
        '
        Me.grpTransfer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpTransfer.Name = "grpTransfer"
        Me.grpTransfer.Padding = New System.Windows.Forms.Padding(10, 6, 10, 10)
        Me.grpTransfer.TabIndex = 3
        Me.grpTransfer.TabStop = False
        Me.grpTransfer.Text = "Transfer"
        Me.grpTransfer.Controls.Add(Me.tlpTransfer)

        Me.tlpTransfer.ColumnCount = 1
        Me.tlpTransfer.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpTransfer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpTransfer.Name = "tlpTransfer"
        Me.tlpTransfer.RowCount = 2
        Me.tlpTransfer.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        Me.tlpTransfer.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpTransfer.TabIndex = 0
        Me.tlpTransfer.Controls.Add(Me.pnlButoane, 0, 0)
        Me.tlpTransfer.Controls.Add(Me.tlpGrile, 0, 1)

        Me.pnlButoane.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlButoane.Name = "pnlButoane"
        Me.pnlButoane.TabIndex = 0
        Me.pnlButoane.Controls.Add(Me.btnVerifica)
        Me.pnlButoane.Controls.Add(Me.btnTransfera)
        Me.pnlButoane.Controls.Add(Me.btnOpreste)
        Me.pnlButoane.Controls.Add(Me.prgTransfer)

        Me.btnVerifica.Location = New System.Drawing.Point(3, 6)
        Me.btnVerifica.Name = "btnVerifica"
        Me.btnVerifica.Size = New System.Drawing.Size(150, 34)
        Me.btnVerifica.TabIndex = 0
        Me.btnVerifica.Text = "Verifică"
        Me.btnVerifica.UseVisualStyleBackColor = True

        Me.btnTransfera.Enabled = False
        Me.btnTransfera.Location = New System.Drawing.Point(162, 6)
        Me.btnTransfera.Name = "btnTransfera"
        Me.btnTransfera.Size = New System.Drawing.Size(150, 34)
        Me.btnTransfera.TabIndex = 1
        Me.btnTransfera.Text = "Transferă"
        Me.btnTransfera.UseVisualStyleBackColor = True

        Me.btnOpreste.Enabled = False
        Me.btnOpreste.Location = New System.Drawing.Point(321, 6)
        Me.btnOpreste.Name = "btnOpreste"
        Me.btnOpreste.Size = New System.Drawing.Size(120, 34)
        Me.btnOpreste.TabIndex = 2
        Me.btnOpreste.Text = "Oprește"
        Me.btnOpreste.UseVisualStyleBackColor = True

        Me.prgTransfer.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.prgTransfer.Location = New System.Drawing.Point(456, 12)
        Me.prgTransfer.Name = "prgTransfer"
        Me.prgTransfer.Size = New System.Drawing.Size(600, 22)
        Me.prgTransfer.TabIndex = 3

        Me.tlpGrile.ColumnCount = 2
        Me.tlpGrile.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.0!))
        Me.tlpGrile.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.0!))
        Me.tlpGrile.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpGrile.Name = "tlpGrile"
        Me.tlpGrile.RowCount = 1
        Me.tlpGrile.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpGrile.TabIndex = 1
        Me.tlpGrile.Controls.Add(Me.dgvTabele, 0, 0)
        Me.tlpGrile.Controls.Add(Me.tlpDreapta, 1, 0)

        Me.dgvTabele.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvTabele.Name = "dgvTabele"
        Me.dgvTabele.TabIndex = 0
        Me.dgvTabele.AddColumn("bifa", "Scrie", KBot.Controls.KBotColumnType.CheckBox, 70)
        Me.dgvTabele.AddColumn("tabel", "Tabel", KBot.Controls.KBotColumnType.Text, 200)
        Me.dgvTabele.AddColumn("sursa", "Sursă", KBot.Controls.KBotColumnType.Text, 130)
        Me.dgvTabele.AddColumn("randuri", "Rânduri Access", KBot.Controls.KBotColumnType.Text, 130)

        Me.tlpDreapta.ColumnCount = 1
        Me.tlpDreapta.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpDreapta.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpDreapta.Name = "tlpDreapta"
        Me.tlpDreapta.RowCount = 2
        Me.tlpDreapta.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45.0!))
        Me.tlpDreapta.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.0!))
        Me.tlpDreapta.TabIndex = 1
        Me.tlpDreapta.Controls.Add(Me.dgvConstatari, 0, 0)
        Me.tlpDreapta.Controls.Add(Me.rtbJurnal, 0, 1)

        Me.dgvConstatari.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvConstatari.Name = "dgvConstatari"
        Me.dgvConstatari.TabIndex = 0
        Me.dgvConstatari.AddColumn("clasa", "Clasă", KBot.Controls.KBotColumnType.Text, 100)
        Me.dgvConstatari.AddColumn("fel", "Fel", KBot.Controls.KBotColumnType.Text, 200)
        Me.dgvConstatari.AddColumn("tabel", "Tabel", KBot.Controls.KBotColumnType.Text, 150)
        Me.dgvConstatari.AddColumn("coloana", "Coloană", KBot.Controls.KBotColumnType.Text, 140)
        Me.dgvConstatari.AddColumn("mesaj", "Mesaj", KBot.Controls.KBotColumnType.Text, 620)

        Me.rtbJurnal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rtbJurnal.Name = "rtbJurnal"
        Me.rtbJurnal.ReadOnly = True
        Me.rtbJurnal.TabIndex = 1
        Me.rtbJurnal.Text = ""
        Me.rtbJurnal.WordWrap = False

        '
        ' tooltips - Romanian, authored here, never a System.Windows.Forms.ToolTip
        '
        Me.tipMigrator.SetToolTipHeader(Me.txtRegistru, "Registrul AVACONT")
        Me.tipMigrator.SetToolTipText(Me.txtRegistru,
            "Fișierul «cale.accdb». Din el se citesc DC-urile, unitățile și căile" & vbLf &
            "către fișierele fiecărei unități — nu trebuie tastate una câte una.")
        Me.tipMigrator.SetToolTipHeader(Me.txtJurnal, "Dosarul jurnalului")
        Me.tipMigrator.SetToolTipText(Me.txtJurnal,
            "Fiecare rulare lasă aici un subdosar cu marcaj de timp, un «.sql» pe tabel" & vbLf &
            "și «_99_final.txt» cu COMMIT sau ROLLBACK. Fără el, transferul nu pornește.")
        Me.tipMigrator.SetToolTipHeader(Me.btnVerifica, "Verifică")
        Me.tipMigrator.SetToolTipText(Me.btnVerifica,
            "Rulează toate porțile fără să scrie nimic: fișiere, bază, AVACONT_COMUN," & vbLf &
            "Unitati, coloane obligatorii, lățimi, rezoluții, ordinea de scriere.")
        Me.tipMigrator.SetToolTipHeader(Me.btnTransfera, "Transferă")
        Me.tipMigrator.SetToolTipText(Me.btnTransfera,
            "Se activează doar după o verificare fără constatări blocante." & vbLf &
            "Scrie totul într-o singură tranzacție; orice eșec derulează tot înapoi.")
        Me.tipMigrator.SetToolTipHeader(Me.btnOpreste, "Oprește")
        Me.tipMigrator.SetToolTipText(Me.btnOpreste,
            "Oprirea derulează tranzacția înapoi — baza rămâne exact cum era.")

        '
        ' MigratorForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1240, 860)
        Me.MinimumSize = New System.Drawing.Size(1060, 720)
        Me.Name = "MigratorForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "K-BOT — migrare Access ▸ MariaDB"
        Me.Controls.Add(Me.tlpRoot)

        CType(Me.dgvUnitati, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvTabele, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgvConstatari, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tlpDreapta.ResumeLayout(False)
        Me.tlpGrile.ResumeLayout(False)
        Me.pnlButoane.ResumeLayout(False)
        Me.tlpTransfer.ResumeLayout(False)
        Me.grpTransfer.ResumeLayout(False)
        Me.pnlUnitateSus.ResumeLayout(False)
        Me.pnlUnitateSus.PerformLayout()
        Me.grpUnitate.ResumeLayout(False)
        Me.tlpServer.ResumeLayout(False)
        Me.tlpServer.PerformLayout()
        Me.grpServer.ResumeLayout(False)
        Me.grpServer.PerformLayout()
        Me.tlpFisiere.ResumeLayout(False)
        Me.tlpFisiere.PerformLayout()
        Me.grpFisiere.ResumeLayout(False)
        Me.grpFisiere.PerformLayout()
        Me.tlpRoot.ResumeLayout(False)
        Me.tlpRoot.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
