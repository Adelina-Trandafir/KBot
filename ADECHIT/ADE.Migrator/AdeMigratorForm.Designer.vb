<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdeMigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel

    ' --- source ---
    Friend WithEvents grpSursa As System.Windows.Forms.GroupBox
    Friend WithEvents tlpSursa As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblFisier As System.Windows.Forms.Label
    Friend WithEvents txtFisier As KBot.Controls.KBotTextField
    Friend WithEvents btnRasfoire As System.Windows.Forms.Button
    Friend WithEvents lblDc As System.Windows.Forms.Label
    Friend WithEvents btnCiteste As System.Windows.Forms.Button

    ' --- server ---
    Friend WithEvents grpServer As System.Windows.Forms.GroupBox
    Friend WithEvents tlpServer As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblGazda As System.Windows.Forms.Label
    Friend WithEvents txtGazda As KBot.Controls.KBotTextField
    Friend WithEvents lblPort As System.Windows.Forms.Label
    Friend WithEvents txtPort As KBot.Controls.KBotTextField
    Friend WithEvents lblUtilizator As System.Windows.Forms.Label
    Friend WithEvents txtUtilizator As KBot.Controls.KBotTextField
    Friend WithEvents lblParola As System.Windows.Forms.Label
    Friend WithEvents txtParola As KBot.Controls.KBotTextField
    Friend WithEvents btnTesteaza As System.Windows.Forms.Button
    Friend WithEvents lblStareServer As System.Windows.Forms.Label

    ' --- action row ---
    Friend WithEvents tlpActiune As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblRezumat As System.Windows.Forms.Label
    Friend WithEvents btnMigreaza As System.Windows.Forms.Button

    ' --- bottom: tables (left), educators + conversions (right) ---
    Friend WithEvents tlpJos As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents grpTabele As System.Windows.Forms.GroupBox
    Friend WithEvents dgvTabele As KBot.Controls.KBotDataView
    Friend WithEvents tlpDreapta As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents grpEducatori As System.Windows.Forms.GroupBox
    Friend WithEvents dgvEducatori As KBot.Controls.KBotDataView
    Friend WithEvents grpConversii As System.Windows.Forms.GroupBox
    Friend WithEvents rtbConversii As System.Windows.Forms.RichTextBox

    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents tipAde As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Dim colTabel As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colRanduriAccess As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colTinta As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colDeScris As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colInBaza As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colGrupa As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colBrut As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim colNume As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        grpSursa = New System.Windows.Forms.GroupBox()
        tlpSursa = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblFisier = New System.Windows.Forms.Label()
        txtFisier = New KBot.Controls.KBotTextField()
        btnRasfoire = New System.Windows.Forms.Button()
        lblDc = New System.Windows.Forms.Label()
        btnCiteste = New System.Windows.Forms.Button()
        grpServer = New System.Windows.Forms.GroupBox()
        tlpServer = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblGazda = New System.Windows.Forms.Label()
        txtGazda = New KBot.Controls.KBotTextField()
        lblPort = New System.Windows.Forms.Label()
        txtPort = New KBot.Controls.KBotTextField()
        lblUtilizator = New System.Windows.Forms.Label()
        txtUtilizator = New KBot.Controls.KBotTextField()
        lblParola = New System.Windows.Forms.Label()
        txtParola = New KBot.Controls.KBotTextField()
        btnTesteaza = New System.Windows.Forms.Button()
        lblStareServer = New System.Windows.Forms.Label()
        tlpActiune = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblRezumat = New System.Windows.Forms.Label()
        btnMigreaza = New System.Windows.Forms.Button()
        tlpJos = New Global.KBot.Controls.KBotTableLayoutPanel()
        grpTabele = New System.Windows.Forms.GroupBox()
        dgvTabele = New KBot.Controls.KBotDataView()
        tlpDreapta = New Global.KBot.Controls.KBotTableLayoutPanel()
        grpEducatori = New System.Windows.Forms.GroupBox()
        dgvEducatori = New KBot.Controls.KBotDataView()
        grpConversii = New System.Windows.Forms.GroupBox()
        rtbConversii = New System.Windows.Forms.RichTextBox()
        lblStare = New System.Windows.Forms.Label()
        tipAde = New KBot.Controls.KBotToolTip(components)
        CType(dgvTabele, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvEducatori, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpRoot.SuspendLayout()
        grpSursa.SuspendLayout()
        tlpSursa.SuspendLayout()
        grpServer.SuspendLayout()
        tlpServer.SuspendLayout()
        tlpActiune.SuspendLayout()
        tlpJos.SuspendLayout()
        grpTabele.SuspendLayout()
        tlpDreapta.SuspendLayout()
        grpEducatori.SuspendLayout()
        grpConversii.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(grpSursa, 0, 0)
        tlpRoot.Controls.Add(grpServer, 0, 1)
        tlpRoot.Controls.Add(tlpActiune, 0, 2)
        tlpRoot.Controls.Add(tlpJos, 0, 3)
        tlpRoot.Controls.Add(lblStare, 0, 4)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(8)
        tlpRoot.RowCount = 5
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 70.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 70.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tlpRoot.Size = New Size(1280, 820)
        tlpRoot.TabIndex = 0
        '
        ' grpSursa
        '
        grpSursa.Controls.Add(tlpSursa)
        grpSursa.Dock = DockStyle.Fill
        grpSursa.Name = "grpSursa"
        grpSursa.TabIndex = 0
        grpSursa.TabStop = False
        grpSursa.Text = "Baza Access ADECHIT"
        '
        ' tlpSursa
        '
        tlpSursa.ColumnCount = 5
        tlpSursa.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70.0F))
        tlpSursa.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpSursa.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40.0F))
        tlpSursa.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260.0F))
        tlpSursa.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120.0F))
        tlpSursa.Controls.Add(lblFisier, 0, 0)
        tlpSursa.Controls.Add(txtFisier, 1, 0)
        tlpSursa.Controls.Add(btnRasfoire, 2, 0)
        tlpSursa.Controls.Add(lblDc, 3, 0)
        tlpSursa.Controls.Add(btnCiteste, 4, 0)
        tlpSursa.Dock = DockStyle.Fill
        tlpSursa.Name = "tlpSursa"
        tlpSursa.RowCount = 1
        tlpSursa.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpSursa.TabIndex = 0
        '
        ' lblFisier
        '
        lblFisier.Dock = DockStyle.Fill
        lblFisier.Name = "lblFisier"
        lblFisier.TabIndex = 0
        lblFisier.Text = "Fișier .mdb"
        lblFisier.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtFisier
        '
        txtFisier.BackColor = Color.Transparent
        txtFisier.Dock = DockStyle.Fill
        txtFisier.Name = "txtFisier"
        txtFisier.TabIndex = 1
        '
        ' btnRasfoire
        '
        btnRasfoire.Dock = DockStyle.Fill
        btnRasfoire.Name = "btnRasfoire"
        btnRasfoire.TabIndex = 2
        btnRasfoire.Text = "..."
        btnRasfoire.UseVisualStyleBackColor = True
        '
        ' lblDc
        '
        lblDc.Dock = DockStyle.Fill
        lblDc.Name = "lblDc"
        lblDc.TabIndex = 3
        lblDc.Text = "DC: (necitit)"
        lblDc.TextAlign = ContentAlignment.MiddleCenter
        '
        ' btnCiteste
        '
        btnCiteste.Dock = DockStyle.Fill
        btnCiteste.Name = "btnCiteste"
        btnCiteste.TabIndex = 4
        btnCiteste.Text = "Citește"
        btnCiteste.UseVisualStyleBackColor = True
        '
        ' grpServer
        '
        grpServer.Controls.Add(tlpServer)
        grpServer.Dock = DockStyle.Fill
        grpServer.Name = "grpServer"
        grpServer.TabIndex = 1
        grpServer.TabStop = False
        grpServer.Text = "Server MariaDB (baza = DC-ul din Access)"
        '
        ' tlpServer
        '
        tlpServer.ColumnCount = 10
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 45.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120.0F))
        tlpServer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpServer.Controls.Add(lblGazda, 0, 0)
        tlpServer.Controls.Add(txtGazda, 1, 0)
        tlpServer.Controls.Add(lblPort, 2, 0)
        tlpServer.Controls.Add(txtPort, 3, 0)
        tlpServer.Controls.Add(lblUtilizator, 4, 0)
        tlpServer.Controls.Add(txtUtilizator, 5, 0)
        tlpServer.Controls.Add(lblParola, 6, 0)
        tlpServer.Controls.Add(txtParola, 7, 0)
        tlpServer.Controls.Add(btnTesteaza, 8, 0)
        tlpServer.Controls.Add(lblStareServer, 9, 0)
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
        lblPort.TextAlign = ContentAlignment.MiddleRight
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
        lblUtilizator.TextAlign = ContentAlignment.MiddleRight
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
        lblParola.TextAlign = ContentAlignment.MiddleRight
        '
        ' txtParola
        '
        txtParola.BackColor = Color.Transparent
        txtParola.Dock = DockStyle.Fill
        txtParola.Name = "txtParola"
        txtParola.TabIndex = 7
        txtParola.UseSystemPasswordChar = True
        '
        ' btnTesteaza
        '
        btnTesteaza.Dock = DockStyle.Fill
        btnTesteaza.Name = "btnTesteaza"
        btnTesteaza.TabIndex = 8
        btnTesteaza.Text = "Testează"
        btnTesteaza.UseVisualStyleBackColor = True
        '
        ' lblStareServer
        '
        lblStareServer.Dock = DockStyle.Fill
        lblStareServer.Name = "lblStareServer"
        lblStareServer.TabIndex = 9
        lblStareServer.Text = "Netestat"
        lblStareServer.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlpActiune
        '
        tlpActiune.ColumnCount = 2
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpActiune.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180.0F))
        tlpActiune.Controls.Add(lblRezumat, 0, 0)
        tlpActiune.Controls.Add(btnMigreaza, 1, 0)
        tlpActiune.Dock = DockStyle.Fill
        tlpActiune.Name = "tlpActiune"
        tlpActiune.RowCount = 1
        tlpActiune.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpActiune.TabIndex = 2
        '
        ' lblRezumat
        '
        lblRezumat.Dock = DockStyle.Fill
        lblRezumat.Name = "lblRezumat"
        lblRezumat.TabIndex = 0
        lblRezumat.Text = "Alegeți fișierul Access și apăsați «Citește»."
        lblRezumat.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnMigreaza
        '
        btnMigreaza.Dock = DockStyle.Fill
        btnMigreaza.Enabled = False
        btnMigreaza.Name = "btnMigreaza"
        btnMigreaza.TabIndex = 1
        btnMigreaza.Text = "Migrează"
        btnMigreaza.UseVisualStyleBackColor = True
        '
        ' tlpJos
        '
        tlpJos.ColumnCount = 2
        tlpJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 46.0F))
        tlpJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 54.0F))
        tlpJos.Controls.Add(grpTabele, 0, 0)
        tlpJos.Controls.Add(tlpDreapta, 1, 0)
        tlpJos.Dock = DockStyle.Fill
        tlpJos.Name = "tlpJos"
        tlpJos.RowCount = 1
        tlpJos.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpJos.TabIndex = 3
        '
        ' grpTabele
        '
        grpTabele.Controls.Add(dgvTabele)
        grpTabele.Dock = DockStyle.Fill
        grpTabele.Name = "grpTabele"
        grpTabele.TabIndex = 0
        grpTabele.TabStop = False
        grpTabele.Text = "Tabele care se migrează"
        '
        ' dgvTabele
        '
        dgvTabele.BackColor = SystemColors.Window
        colTabel.HeaderText = "Tabel Access"
        colTabel.Key = "tabel"
        colTabel.ReadOnly = True
        colTabel.Width = 130
        colRanduriAccess.HeaderText = "Rânduri Access"
        colRanduriAccess.Key = "randuriAccess"
        colRanduriAccess.ReadOnly = True
        colRanduriAccess.TextAlign = ContentAlignment.MiddleRight
        colRanduriAccess.Width = 110
        colTinta.HeaderText = "Tabel MariaDB"
        colTinta.Key = "tinta"
        colTinta.ReadOnly = True
        colTinta.Width = 160
        colDeScris.HeaderText = "De scris"
        colDeScris.Key = "deScris"
        colDeScris.ReadOnly = True
        colDeScris.TextAlign = ContentAlignment.MiddleRight
        colDeScris.Width = 80
        colInBaza.HeaderText = "Acum în MariaDB"
        colInBaza.Key = "inBaza"
        colInBaza.ReadOnly = True
        colInBaza.TextAlign = ContentAlignment.MiddleRight
        colInBaza.Width = 120
        dgvTabele.Columns.Add(colTabel)
        dgvTabele.Columns.Add(colRanduriAccess)
        dgvTabele.Columns.Add(colTinta)
        dgvTabele.Columns.Add(colDeScris)
        dgvTabele.Columns.Add(colInBaza)
        dgvTabele.Dock = DockStyle.Fill
        dgvTabele.HeaderHeight = 26
        dgvTabele.Name = "dgvTabele"
        dgvTabele.ReadOnlyGrid = True
        dgvTabele.RowHeight = 24
        dgvTabele.TabIndex = 0
        '
        ' tlpDreapta
        '
        tlpDreapta.ColumnCount = 1
        tlpDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpDreapta.Controls.Add(grpEducatori, 0, 0)
        tlpDreapta.Controls.Add(grpConversii, 0, 1)
        tlpDreapta.Dock = DockStyle.Fill
        tlpDreapta.Name = "tlpDreapta"
        tlpDreapta.RowCount = 2
        tlpDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        tlpDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        tlpDreapta.TabIndex = 1
        '
        ' grpEducatori
        '
        grpEducatori.Controls.Add(dgvEducatori)
        grpEducatori.Dock = DockStyle.Fill
        grpEducatori.Name = "grpEducatori"
        grpEducatori.TabIndex = 0
        grpEducatori.TabStop = False
        grpEducatori.Text = "Educatori găsiți pe fiecare grupă — corectați ultima coloană înainte de migrare"
        '
        ' dgvEducatori
        '
        dgvEducatori.BackColor = SystemColors.Window
        dgvEducatori.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        colGrupa.HeaderText = "Grupă"
        colGrupa.Key = "grupa"
        colGrupa.ReadOnly = True
        colGrupa.Width = 150
        colBrut.HeaderText = "Educator în Access"
        colBrut.Key = "brut"
        colBrut.ReadOnly = True
        colBrut.Width = 260
        colNume.HeaderText = "Educatori de scris (unul pe rând în Grupe_Educator)"
        colNume.Key = "nume"
        colNume.Width = 300
        dgvEducatori.Columns.Add(colGrupa)
        dgvEducatori.Columns.Add(colBrut)
        dgvEducatori.Columns.Add(colNume)
        dgvEducatori.Dock = DockStyle.Fill
        dgvEducatori.FillColumnKey = "nume"
        dgvEducatori.HeaderHeight = 26
        dgvEducatori.Name = "dgvEducatori"
        dgvEducatori.RowHeight = 24
        dgvEducatori.TabIndex = 0
        '
        ' grpConversii
        '
        grpConversii.Controls.Add(rtbConversii)
        grpConversii.Dock = DockStyle.Fill
        grpConversii.Name = "grpConversii"
        grpConversii.TabIndex = 1
        grpConversii.TabStop = False
        grpConversii.Text = "Conversii și constatări"
        '
        ' rtbConversii
        '
        rtbConversii.Dock = DockStyle.Fill
        rtbConversii.Name = "rtbConversii"
        rtbConversii.ReadOnly = True
        rtbConversii.TabIndex = 0
        rtbConversii.Text = ""
        '
        ' lblStare
        '
        lblStare.Dock = DockStyle.Fill
        lblStare.Name = "lblStare"
        lblStare.TabIndex = 4
        lblStare.Text = ""
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tipAde
        '
        tipAde.SetToolTipHeader(btnRasfoire, "Alegeți fișierul")
        tipAde.SetToolTipText(btnRasfoire, "Caută fișiere .mdb, de obicei în C:\adechit.")
        tipAde.SetToolTipHeader(btnCiteste, "Citește fișierul")
        tipAde.SetToolTipText(btnCiteste, "Citește Access-ul (doar citire) și arată ce se va scrie. Nu atinge serverul.")
        tipAde.SetToolTipHeader(btnTesteaza, "Testează serverul")
        tipAde.SetToolTipText(btnTesteaza, "Verifică parola, baza cu numele DC-ului și dacă tabelele AD_ sunt create și goale.")
        tipAde.SetToolTipHeader(btnMigreaza, "Migrează")
        tipAde.SetToolTipText(btnMigreaza, "Scrie totul într-o singură tranzacție: dacă ceva eșuează, baza rămâne neschimbată.")
        '
        ' AdeMigratorForm
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1280, 820)
        Controls.Add(tlpRoot)
        MinimumSize = New Size(1100, 700)
        Name = "AdeMigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "ADE.Migrator — Access ADECHIT ▸ MariaDB"
        CType(dgvTabele, System.ComponentModel.ISupportInitialize).EndInit()
        CType(dgvEducatori, System.ComponentModel.ISupportInitialize).EndInit()
        tlpRoot.ResumeLayout(False)
        grpSursa.ResumeLayout(False)
        tlpSursa.ResumeLayout(False)
        grpServer.ResumeLayout(False)
        tlpServer.ResumeLayout(False)
        tlpActiune.ResumeLayout(False)
        tlpJos.ResumeLayout(False)
        grpTabele.ResumeLayout(False)
        tlpDreapta.ResumeLayout(False)
        grpEducatori.ResumeLayout(False)
        grpConversii.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
