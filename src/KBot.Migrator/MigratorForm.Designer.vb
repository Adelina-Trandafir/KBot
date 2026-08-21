<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Toate controalele sunt declarate AICI, ca formularul sa se randeze in designerul VS
    ' (docs/kbot-forms-ui-convention.md). Nimic nu se construieste in cod.
    ' Fiecare panou mare isi tine campurile intr-un TableLayoutPanel, ca aranjarea sa se
    ' faca din celule (randuri/coloane), nu din coordonate scrise de mana. Singura
    ' exceptie este grila de constatari, andocata direct pe formular.
    Private components As System.ComponentModel.IContainer

    ' --- regiunea 1: sursa -----------------------------------------------------
    Friend WithEvents pnlSurse As System.Windows.Forms.Panel
    Friend WithEvents tlpSurse As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblDc As System.Windows.Forms.Label
    Friend WithEvents cboDc As System.Windows.Forms.ComboBox
    Friend WithEvents lblUnitate As System.Windows.Forms.Label
    Friend WithEvents lblAn As System.Windows.Forms.Label
    Friend WithEvents cboAn As System.Windows.Forms.ComboBox
    Friend WithEvents lblBaza As System.Windows.Forms.Label
    Friend WithEvents cboBaza As System.Windows.Forms.ComboBox
    Friend WithEvents btnReciteste As System.Windows.Forms.Button
    Friend WithEvents lblFx As System.Windows.Forms.Label
    Friend WithEvents txtFx As System.Windows.Forms.TextBox
    Friend WithEvents btnRasfoireFx As System.Windows.Forms.Button
    Friend WithEvents btnImpinge As System.Windows.Forms.Button
    Friend WithEvents prgPush As System.Windows.Forms.ProgressBar
    Friend WithEvents lblFisiere As System.Windows.Forms.Label

    ' --- regiunea 2: tabelele de actualizat ------------------------------------
    Friend WithEvents pnlTabele As System.Windows.Forms.Panel
    Friend WithEvents lblTabele As System.Windows.Forms.Label
    Friend WithEvents dgvTabele As KBot.Controls.KBotDataView
    Friend WithEvents pnlOrdine As System.Windows.Forms.Panel
    Friend WithEvents btnSus As System.Windows.Forms.Button
    Friend WithEvents btnJos As System.Windows.Forms.Button

    ' --- regiunea 2b: coloanele tabelului ales ---------------------------------
    Friend WithEvents pnlColoane As System.Windows.Forms.Panel
    Friend WithEvents lblColoane As System.Windows.Forms.Label
    Friend WithEvents dgvColoane As KBot.Controls.KBotDataView

    ' --- regiunea 3: actiuni ---------------------------------------------------
    Friend WithEvents pnlActiuni As System.Windows.Forms.Panel
    Friend WithEvents tlpActiuni As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnInventar As System.Windows.Forms.Button
    Friend WithEvents btnAnalizeaza As System.Windows.Forms.Button
    Friend WithEvents btnRuleaza As System.Windows.Forms.Button
    Friend WithEvents btnForteaza As System.Windows.Forms.Button
    Friend WithEvents chkInlocuieste As System.Windows.Forms.CheckBox
    Friend WithEvents lblStare As System.Windows.Forms.Label

    ' --- regiunea 4: constatari / corelatii (file) + jurnal --------------------
    ' Cele doua file impart acelasi loc din dreapta: «Constatari» e ce a gasit
    ' analiza, «Corelatii coloane» e harta Access - MariaDB a tabelului ales.
    Friend WithEvents tabRezultate As System.Windows.Forms.TabControl
    Friend WithEvents tabPagConstatari As System.Windows.Forms.TabPage
    Friend WithEvents dgvConstatari As KBot.Controls.KBotDataView
    Friend WithEvents tabPagCorelatii As System.Windows.Forms.TabPage
    Friend WithEvents lblCorelatii As System.Windows.Forms.Label
    Friend WithEvents dgvCorelatii As KBot.Controls.KBotDataView
    Friend WithEvents txtJurnal As System.Windows.Forms.TextBox
    Friend WithEvents dlgFisier As System.Windows.Forms.OpenFileDialog
    Friend WithEvents sfat As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MigratorForm))
        ' Coloanele celor patru grile, in ordinea in care se adauga:
        ' 1-4 dgvTabele, 5-7 dgvColoane, 8-15 dgvConstatari, 16-19 dgvCorelatii.
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
        Dim KBotDataColumn16 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn17 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn18 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn19 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        pnlSurse = New Panel()
        tlpSurse = New TableLayoutPanel()
        lblDc = New Label()
        cboDc = New ComboBox()
        lblUnitate = New Label()
        lblAn = New Label()
        cboAn = New ComboBox()
        lblBaza = New Label()
        cboBaza = New ComboBox()
        btnReciteste = New Button()
        lblFx = New Label()
        txtFx = New TextBox()
        btnImpinge = New Button()
        prgPush = New ProgressBar()
        lblFisiere = New Label()
        btnRasfoireFx = New Button()
        pnlTabele = New Panel()
        dgvTabele = New Controls.KBotDataView()
        pnlOrdine = New Panel()
        btnSus = New Button()
        btnJos = New Button()
        lblTabele = New Label()
        pnlColoane = New Panel()
        dgvColoane = New Controls.KBotDataView()
        lblColoane = New Label()
        pnlActiuni = New Panel()
        tlpActiuni = New TableLayoutPanel()
        btnInventar = New Button()
        btnAnalizeaza = New Button()
        lblStare = New Label()
        btnRuleaza = New Button()
        btnForteaza = New Button()
        chkInlocuieste = New CheckBox()
        tabRezultate = New TabControl()
        tabPagConstatari = New TabPage()
        dgvConstatari = New Controls.KBotDataView()
        tabPagCorelatii = New TabPage()
        dgvCorelatii = New Controls.KBotDataView()
        lblCorelatii = New Label()
        txtJurnal = New TextBox()
        dlgFisier = New OpenFileDialog()
        sfat = New KBot.Controls.KBotToolTip(components)
        pnlSurse.SuspendLayout()
        tlpSurse.SuspendLayout()
        pnlTabele.SuspendLayout()
        CType(dgvTabele, ComponentModel.ISupportInitialize).BeginInit()
        pnlOrdine.SuspendLayout()
        pnlColoane.SuspendLayout()
        CType(dgvColoane, ComponentModel.ISupportInitialize).BeginInit()
        pnlActiuni.SuspendLayout()
        tlpActiuni.SuspendLayout()
        CType(dgvConstatari, ComponentModel.ISupportInitialize).BeginInit()
        CType(dgvCorelatii, ComponentModel.ISupportInitialize).BeginInit()
        tabRezultate.SuspendLayout()
        tabPagConstatari.SuspendLayout()
        tabPagCorelatii.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlSurse
        ' 
        pnlSurse.Controls.Add(tlpSurse)
        pnlSurse.Dock = DockStyle.Top
        pnlSurse.Location = New Point(0, 0)
        pnlSurse.Name = "pnlSurse"
        pnlSurse.Padding = New Padding(10, 8, 10, 8)
        pnlSurse.Size = New Size(1240, 219)
        pnlSurse.TabIndex = 0
        ' 
        ' tlpSurse
        ' 
        tlpSurse.ColumnCount = 6
        tlpSurse.ColumnStyles.Add(New ColumnStyle())
        tlpSurse.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlpSurse.ColumnStyles.Add(New ColumnStyle())
        tlpSurse.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpSurse.ColumnStyles.Add(New ColumnStyle())
        tlpSurse.ColumnStyles.Add(New ColumnStyle())
        tlpSurse.Controls.Add(lblDc, 0, 0)
        tlpSurse.Controls.Add(cboDc, 1, 0)
        tlpSurse.Controls.Add(lblUnitate, 3, 0)
        tlpSurse.Controls.Add(lblAn, 0, 1)
        tlpSurse.Controls.Add(cboAn, 1, 1)
        tlpSurse.Controls.Add(lblBaza, 2, 1)
        tlpSurse.Controls.Add(cboBaza, 3, 1)
        tlpSurse.Controls.Add(btnReciteste, 4, 1)
        tlpSurse.Controls.Add(lblFx, 0, 2)
        tlpSurse.Controls.Add(txtFx, 1, 2)
        tlpSurse.Controls.Add(btnImpinge, 0, 4)
        tlpSurse.Controls.Add(prgPush, 2, 4)
        tlpSurse.Controls.Add(lblFisiere, 4, 4)
        tlpSurse.Controls.Add(btnRasfoireFx, 4, 2)
        tlpSurse.Dock = DockStyle.Fill
        tlpSurse.Location = New Point(10, 8)
        tlpSurse.Name = "tlpSurse"
        tlpSurse.RowCount = 6
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Absolute, 15F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlpSurse.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpSurse.Size = New Size(1220, 203)
        tlpSurse.TabIndex = 0
        ' 
        ' lblDc
        ' 
        lblDc.Anchor = AnchorStyles.Left
        lblDc.AutoSize = True
        lblDc.Location = New Point(3, 7)
        lblDc.Margin = New Padding(3, 0, 8, 0)
        lblDc.Name = "lblDc"
        lblDc.Size = New Size(186, 25)
        lblDc.TabIndex = 0
        lblDc.Text = "Unitatea (din registru):"
        ' 
        ' cboDc
        ' 
        tlpSurse.SetColumnSpan(cboDc, 2)
        cboDc.Dock = DockStyle.Fill
        cboDc.DropDownStyle = ComboBoxStyle.DropDownList
        cboDc.Location = New Point(200, 4)
        cboDc.Margin = New Padding(3, 4, 3, 4)
        cboDc.Name = "cboDc"
        cboDc.Size = New Size(304, 33)
        cboDc.TabIndex = 1
        sfat.SetToolTipHeader(cboDc, "Unitățile instalate")
        sfat.SetToolTipText(cboDc, "Citite din HKCU\Software\VB and VBA Program Settings\AVACONT." & vbLf & "Migratorul doar citește registrul; nu scrie nimic în el.")
        ' 
        ' lblUnitate
        ' 
        lblUnitate.AutoEllipsis = True
        tlpSurse.SetColumnSpan(lblUnitate, 3)
        lblUnitate.Dock = DockStyle.Fill
        lblUnitate.Location = New Point(515, 0)
        lblUnitate.Margin = New Padding(8, 0, 3, 0)
        lblUnitate.Name = "lblUnitate"
        lblUnitate.Size = New Size(702, 40)
        lblUnitate.TabIndex = 2
        lblUnitate.Text = "—"
        lblUnitate.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblAn
        ' 
        lblAn.Anchor = AnchorStyles.Left
        lblAn.AutoSize = True
        lblAn.Location = New Point(3, 49)
        lblAn.Margin = New Padding(3, 0, 8, 0)
        lblAn.Name = "lblAn"
        lblAn.Size = New Size(38, 25)
        lblAn.TabIndex = 3
        lblAn.Text = "An:"
        ' 
        ' cboAn
        ' 
        cboAn.Dock = DockStyle.Fill
        cboAn.Location = New Point(200, 44)
        cboAn.Margin = New Padding(3, 4, 3, 4)
        cboAn.Name = "cboAn"
        cboAn.Size = New Size(114, 33)
        cboAn.TabIndex = 4
        ' 
        ' lblBaza
        ' 
        lblBaza.Anchor = AnchorStyles.Left
        lblBaza.AutoSize = True
        lblBaza.Location = New Point(325, 49)
        lblBaza.Margin = New Padding(8, 0, 8, 0)
        lblBaza.Name = "lblBaza"
        lblBaza.Size = New Size(174, 25)
        lblBaza.TabIndex = 5
        lblBaza.Text = "Baza țintă (MariaDB):"
        ' 
        ' cboBaza
        ' 
        cboBaza.Dock = DockStyle.Fill
        cboBaza.DropDownStyle = ComboBoxStyle.DropDownList
        cboBaza.Location = New Point(510, 44)
        cboBaza.Margin = New Padding(3, 4, 3, 4)
        cboBaza.Name = "cboBaza"
        cboBaza.Size = New Size(496, 33)
        cboBaza.TabIndex = 6
        sfat.SetToolTipHeader(cboBaza, "Baza de pe MariaDB")
        sfat.SetToolTipText(cboBaza, "Rândurile se rutează prin [Cai]; se scriu doar cele care ajung aici." & vbLf & "Migrarea NU creează tabele.")
        ' 
        ' btnReciteste
        ' 
        tlpSurse.SetColumnSpan(btnReciteste, 2)
        btnReciteste.Dock = DockStyle.Fill
        btnReciteste.Font = New Font("Calibri", 9F, FontStyle.Bold)
        btnReciteste.Location = New Point(1011, 42)
        btnReciteste.Margin = New Padding(2)
        btnReciteste.MinimumSize = New Size(130, 25)
        btnReciteste.Name = "btnReciteste"
        btnReciteste.Size = New Size(207, 40)
        btnReciteste.TabIndex = 7
        btnReciteste.Text = "Recitește serverul"
        btnReciteste.UseVisualStyleBackColor = True
        ' 
        ' lblFx
        ' 
        lblFx.Anchor = AnchorStyles.Left
        lblFx.AutoSize = True
        lblFx.Location = New Point(3, 93)
        lblFx.Margin = New Padding(3, 0, 8, 0)
        lblFx.Name = "lblFx"
        lblFx.Size = New Size(124, 25)
        lblFx.TabIndex = 8
        lblFx.Text = "Fișier FOREXE:"
        ' 
        ' txtFx
        ' 
        tlpSurse.SetColumnSpan(txtFx, 3)
        txtFx.Dock = DockStyle.Fill
        txtFx.Location = New Point(200, 88)
        txtFx.Margin = New Padding(3, 4, 3, 4)
        txtFx.Name = "txtFx"
        txtFx.Size = New Size(806, 31)
        txtFx.TabIndex = 9
        sfat.SetToolTipFooter(txtFx, "În Access: Fișier ▸ Informații ▸ Decriptare bază de date.")
        sfat.SetToolTipHeader(txtFx, "Fișierul FOREXE al anului")
        sfat.SetToolTipText(txtFx, "Trebuie să fie FĂRĂ parolă de bază de date." & vbLf & "Serverul citește cu mdbtools, care nu poate decripta.")
        ' 
        ' btnImpinge
        ' 
        tlpSurse.SetColumnSpan(btnImpinge, 2)
        btnImpinge.Dock = DockStyle.Fill
        btnImpinge.Font = New Font("Calibri", 9F, FontStyle.Bold)
        btnImpinge.Location = New Point(2, 145)
        btnImpinge.Margin = New Padding(2)
        btnImpinge.MinimumSize = New Size(180, 28)
        btnImpinge.Name = "btnImpinge"
        btnImpinge.Size = New Size(313, 40)
        btnImpinge.TabIndex = 14
        btnImpinge.Text = "Încarcă pe server"
        btnImpinge.UseVisualStyleBackColor = True
        ' 
        ' prgPush
        ' 
        tlpSurse.SetColumnSpan(prgPush, 2)
        prgPush.Dock = DockStyle.Fill
        prgPush.Location = New Point(325, 152)
        prgPush.Margin = New Padding(8, 9, 8, 9)
        prgPush.Name = "prgPush"
        prgPush.Size = New Size(676, 26)
        prgPush.TabIndex = 15
        ' 
        ' lblFisiere
        ' 
        lblFisiere.AutoEllipsis = True
        tlpSurse.SetColumnSpan(lblFisiere, 2)
        lblFisiere.Dock = DockStyle.Fill
        lblFisiere.Location = New Point(1017, 143)
        lblFisiere.Margin = New Padding(8, 0, 3, 0)
        lblFisiere.Name = "lblFisiere"
        lblFisiere.Size = New Size(200, 44)
        lblFisiere.TabIndex = 16
        lblFisiere.Text = "Pe server: —"
        lblFisiere.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnRasfoireFx
        ' 
        tlpSurse.SetColumnSpan(btnRasfoireFx, 2)
        btnRasfoireFx.Dock = DockStyle.Fill
        btnRasfoireFx.Font = New Font("Calibri", 9F, FontStyle.Bold)
        btnRasfoireFx.Location = New Point(1011, 86)
        btnRasfoireFx.Margin = New Padding(2)
        btnRasfoireFx.MinimumSize = New Size(102, 25)
        btnRasfoireFx.Name = "btnRasfoireFx"
        btnRasfoireFx.Size = New Size(207, 40)
        btnRasfoireFx.TabIndex = 10
        btnRasfoireFx.Text = "Răsfoiește…"
        btnRasfoireFx.UseVisualStyleBackColor = True
        ' 
        ' pnlTabele
        ' 
        pnlTabele.Controls.Add(dgvTabele)
        pnlTabele.Controls.Add(pnlOrdine)
        pnlTabele.Controls.Add(lblTabele)
        pnlTabele.Dock = DockStyle.Left
        pnlTabele.Location = New Point(0, 219)
        pnlTabele.Margin = New Padding(0)
        pnlTabele.Name = "pnlTabele"
        pnlTabele.Size = New Size(442, 243)
        pnlTabele.TabIndex = 1
        ' 
        ' dgvTabele
        ' 
        KBotDataColumn1.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn1.HeaderText = ""
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "bifa"
        KBotDataColumn1.MinWidth = 40
        KBotDataColumn1.Resizable = False
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 40
        KBotDataColumn2.HeaderText = "Tabel"
        KBotDataColumn2.Key = "tabel"
        KBotDataColumn2.MinWidth = 110
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 170
        KBotDataColumn3.HeaderText = "Rânduri"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.Key = "randuri"
        KBotDataColumn3.MinWidth = 70
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.Width = 80
        KBotDataColumn4.HeaderText = "Ale unității"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.Key = "ale_unitatii"
        KBotDataColumn4.MinWidth = 90
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.Width = 100
        dgvTabele.AllowDrop = True
        dgvTabele.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvTabele.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        dgvTabele.Columns.Add(KBotDataColumn1)
        dgvTabele.Columns.Add(KBotDataColumn2)
        dgvTabele.Columns.Add(KBotDataColumn3)
        dgvTabele.Columns.Add(KBotDataColumn4)
        dgvTabele.Dock = DockStyle.Fill
        dgvTabele.FillColumnKey = "tabel"
        dgvTabele.HeaderHeight = 30
        dgvTabele.Location = New Point(0, 30)
        dgvTabele.Name = "dgvTabele"
        dgvTabele.RowHeight = 26
        dgvTabele.Size = New Size(442, 163)
        dgvTabele.TabIndex = 1
        sfat.SetToolTipHeader(dgvTabele, "Ce se actualizează, și în ce ordine")
        sfat.SetToolTipText(dgvTabele, resources.GetString("dgvTabele.ToolTipText"))
        ' 
        ' pnlOrdine
        ' 
        pnlOrdine.Controls.Add(btnSus)
        pnlOrdine.Controls.Add(btnJos)
        pnlOrdine.Dock = DockStyle.Bottom
        pnlOrdine.Location = New Point(0, 193)
        pnlOrdine.Name = "pnlOrdine"
        pnlOrdine.Size = New Size(442, 50)
        pnlOrdine.TabIndex = 2
        ' 
        ' btnSus
        ' 
        btnSus.Dock = DockStyle.Right
        btnSus.Location = New Point(342, 0)
        btnSus.MinimumSize = New Size(60, 25)
        btnSus.Name = "btnSus"
        btnSus.Size = New Size(100, 50)
        btnSus.TabIndex = 0
        btnSus.Text = "▲ Sus"
        sfat.SetToolTipHeader(btnSus, "Mută mai devreme")
        sfat.SetToolTipText(btnSus, "Urcă tabelul ales cu un loc: se va scrie mai devreme." & vbLf & "Părinții trebuie scriși înaintea copiilor.")
        btnSus.UseVisualStyleBackColor = True
        ' 
        ' btnJos
        ' 
        btnJos.Dock = DockStyle.Left
        btnJos.Location = New Point(0, 0)
        btnJos.MinimumSize = New Size(60, 25)
        btnJos.Name = "btnJos"
        btnJos.Size = New Size(100, 50)
        btnJos.TabIndex = 1
        btnJos.Text = "▼ Jos"
        sfat.SetToolTipHeader(btnJos, "Mută mai târziu")
        sfat.SetToolTipText(btnJos, "Coboară tabelul ales cu un loc: se va scrie mai târziu.")
        btnJos.UseVisualStyleBackColor = True
        ' 
        ' lblTabele
        ' 
        lblTabele.Dock = DockStyle.Top
        lblTabele.Location = New Point(0, 0)
        lblTabele.Name = "lblTabele"
        lblTabele.Size = New Size(442, 30)
        lblTabele.TabIndex = 0
        lblTabele.Text = "Tabele de actualizat:"
        lblTabele.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlColoane
        ' 
        pnlColoane.Controls.Add(dgvColoane)
        pnlColoane.Controls.Add(lblColoane)
        pnlColoane.Dock = DockStyle.Left
        pnlColoane.Location = New Point(442, 219)
        pnlColoane.Margin = New Padding(0)
        pnlColoane.Name = "pnlColoane"
        pnlColoane.Padding = New Padding(4, 0, 4, 0)
        pnlColoane.Size = New Size(300, 243)
        pnlColoane.TabIndex = 5
        ' 
        ' dgvColoane
        ' 
        KBotDataColumn5.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn5.HeaderText = ""
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "bifa"
        KBotDataColumn5.MinWidth = 40
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 40
        KBotDataColumn6.HeaderText = "Coloană"
        KBotDataColumn6.Key = "nume"
        KBotDataColumn6.MinWidth = 110
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 150
        KBotDataColumn7.HeaderText = "Pe MariaDB"
        KBotDataColumn7.Key = "stare"
        KBotDataColumn7.MinWidth = 95
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 95
        dgvColoane.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvColoane.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        dgvColoane.Columns.Add(KBotDataColumn5)
        dgvColoane.Columns.Add(KBotDataColumn6)
        dgvColoane.Columns.Add(KBotDataColumn7)
        dgvColoane.Dock = DockStyle.Fill
        dgvColoane.FillColumnKey = "nume"
        dgvColoane.HeaderHeight = 30
        dgvColoane.Location = New Point(4, 30)
        dgvColoane.Margin = New Padding(0)
        dgvColoane.Name = "dgvColoane"
        dgvColoane.RowHeight = 26
        dgvColoane.Size = New Size(292, 213)
        dgvColoane.TabIndex = 1
        sfat.SetToolTipHeader(dgvColoane, "Ce coloane călătoresc")
        sfat.SetToolTipText(dgvColoane, resources.GetString("dgvColoane.ToolTipText"))
        ' 
        ' lblColoane
        ' 
        lblColoane.Dock = DockStyle.Top
        lblColoane.Location = New Point(4, 0)
        lblColoane.Name = "lblColoane"
        lblColoane.Size = New Size(292, 30)
        lblColoane.TabIndex = 0
        lblColoane.Text = "Coloane:"
        lblColoane.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlActiuni
        ' 
        pnlActiuni.Controls.Add(tlpActiuni)
        pnlActiuni.Dock = DockStyle.Bottom
        pnlActiuni.Location = New Point(0, 616)
        pnlActiuni.Margin = New Padding(0)
        pnlActiuni.Name = "pnlActiuni"
        pnlActiuni.Size = New Size(1240, 96)
        pnlActiuni.TabIndex = 4
        ' 
        ' tlpActiuni
        ' 
        tlpActiuni.ColumnCount = 5
        tlpActiuni.ColumnStyles.Add(New ColumnStyle())
        tlpActiuni.ColumnStyles.Add(New ColumnStyle())
        tlpActiuni.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpActiuni.ColumnStyles.Add(New ColumnStyle())
        tlpActiuni.ColumnStyles.Add(New ColumnStyle())
        tlpActiuni.Controls.Add(btnInventar, 0, 0)
        tlpActiuni.Controls.Add(btnAnalizeaza, 1, 0)
        tlpActiuni.Controls.Add(lblStare, 2, 0)
        tlpActiuni.Controls.Add(btnRuleaza, 3, 0)
        tlpActiuni.Controls.Add(btnForteaza, 4, 0)
        tlpActiuni.Controls.Add(chkInlocuieste, 0, 1)
        tlpActiuni.Dock = DockStyle.Fill
        tlpActiuni.Location = New Point(0, 0)
        tlpActiuni.Margin = New Padding(0)
        tlpActiuni.Name = "tlpActiuni"
        tlpActiuni.RowCount = 2
        tlpActiuni.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlpActiuni.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpActiuni.Size = New Size(1240, 96)
        tlpActiuni.TabIndex = 0
        ' 
        ' btnInventar
        ' 
        btnInventar.Dock = DockStyle.Fill
        btnInventar.Location = New Point(3, 3)
        btnInventar.Margin = New Padding(3, 3, 6, 3)
        btnInventar.MinimumSize = New Size(170, 32)
        btnInventar.Name = "btnInventar"
        btnInventar.Size = New Size(170, 54)
        btnInventar.TabIndex = 0
        btnInventar.Text = "Citește tabelele"
        sfat.SetToolTipHeader(btnInventar, "Citește tabelele")
        sfat.SetToolTipText(btnInventar, "Numără rândurile fiecărui tabel din fișierul deja împins." & vbLf & "Tabelele fără rânduri rămân nebifate.")
        btnInventar.UseVisualStyleBackColor = True
        ' 
        ' btnAnalizeaza
        ' 
        btnAnalizeaza.Dock = DockStyle.Fill
        btnAnalizeaza.Location = New Point(182, 3)
        btnAnalizeaza.Margin = New Padding(3, 3, 6, 3)
        btnAnalizeaza.MinimumSize = New Size(180, 32)
        btnAnalizeaza.Name = "btnAnalizeaza"
        btnAnalizeaza.Size = New Size(180, 54)
        btnAnalizeaza.TabIndex = 1
        btnAnalizeaza.Text = "Analizează"
        btnAnalizeaza.UseVisualStyleBackColor = True
        ' 
        ' lblStare
        ' 
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(376, 3)
        lblStare.Margin = New Padding(8, 3, 8, 3)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(496, 54)
        lblStare.TabIndex = 2
        lblStare.Text = "Alege unitatea, anul și baza țintă."
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnRuleaza
        ' 
        btnRuleaza.Dock = DockStyle.Fill
        btnRuleaza.Enabled = False
        btnRuleaza.Location = New Point(886, 3)
        btnRuleaza.Margin = New Padding(6, 3, 3, 3)
        btnRuleaza.MinimumSize = New Size(160, 32)
        btnRuleaza.Name = "btnRuleaza"
        btnRuleaza.Size = New Size(160, 54)
        btnRuleaza.TabIndex = 3
        btnRuleaza.Text = "Rulează"
        sfat.SetToolTipHeader(btnRuleaza, "Rulează")
        sfat.SetToolTipText(btnRuleaza, "Pornește doar dacă analiza n-a găsit absolut nimic.")
        btnRuleaza.UseVisualStyleBackColor = True
        ' 
        ' btnForteaza
        ' 
        btnForteaza.Dock = DockStyle.Fill
        btnForteaza.Enabled = False
        btnForteaza.Location = New Point(1055, 3)
        btnForteaza.Margin = New Padding(6, 3, 3, 3)
        btnForteaza.MinimumSize = New Size(182, 32)
        btnForteaza.Name = "btnForteaza"
        btnForteaza.Size = New Size(182, 54)
        btnForteaza.TabIndex = 4
        btnForteaza.Text = "Forțează rularea"
        sfat.SetToolTipHeader(btnForteaza, "Forțează rularea")
        sfat.SetToolTipText(btnForteaza, "Pornește când singurele probleme sunt de integritate (chei străine," & vbLf & "chei duble, rânduri nerutabile). Acele rânduri se SAR." & vbLf & "Problemele de tip sau de dimensiune opresc și acest buton.")
        btnForteaza.UseVisualStyleBackColor = True
        ' 
        ' chkInlocuieste
        ' 
        chkInlocuieste.Anchor = AnchorStyles.Left
        chkInlocuieste.AutoSize = True
        tlpActiuni.SetColumnSpan(chkInlocuieste, 5)
        chkInlocuieste.Location = New Point(6, 63)
        chkInlocuieste.Margin = New Padding(6, 3, 3, 3)
        chkInlocuieste.Name = "chkInlocuieste"
        chkInlocuieste.Size = New Size(641, 29)
        chkInlocuieste.TabIndex = 5
        chkInlocuieste.Text = "Înlocuiește tot pe server — golește întâi tabelele bifate, apoi le scrie din fișier"
        sfat.SetToolTipHeader(chkInlocuieste, "Înlocuiește tot pe server")
        sfat.SetToolTipText(chkInlocuieste, "Datele existente din tabelele BIFATE se șterg întâi de pe server," & vbLf & "apoi se scriu cele din fișierul Access. Totul într-o SINGURĂ" & vbLf & "tranzacție: la orice eroare, baza rămâne exact cum era.")
        chkInlocuieste.UseVisualStyleBackColor = True
        ' 
        ' tabRezultate
        ' 
        tabRezultate.Controls.Add(tabPagConstatari)
        tabRezultate.Controls.Add(tabPagCorelatii)
        tabRezultate.Dock = DockStyle.Fill
        tabRezultate.Location = New Point(742, 219)
        tabRezultate.Margin = New Padding(0)
        tabRezultate.Name = "tabRezultate"
        tabRezultate.SelectedIndex = 0
        tabRezultate.Size = New Size(498, 243)
        tabRezultate.TabIndex = 2
        ' 
        ' tabPagConstatari
        ' 
        tabPagConstatari.Controls.Add(dgvConstatari)
        tabPagConstatari.Location = New Point(4, 34)
        tabPagConstatari.Name = "tabPagConstatari"
        tabPagConstatari.Padding = New Padding(3)
        tabPagConstatari.Size = New Size(490, 205)
        tabPagConstatari.TabIndex = 0
        tabPagConstatari.Text = "Constatări"
        tabPagConstatari.UseVisualStyleBackColor = True
        ' 
        ' dgvConstatari
        ' 
        KBotDataColumn8.HeaderText = "Clasă"
        KBotDataColumn8.Key = "clasa"
        KBotDataColumn8.MinWidth = 80
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 90
        KBotDataColumn9.HeaderText = "Tabel"
        KBotDataColumn9.Key = "tabel"
        KBotDataColumn9.MinWidth = 110
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.Width = 140
        KBotDataColumn10.HeaderText = "Coloană"
        KBotDataColumn10.Key = "coloana"
        KBotDataColumn10.MinWidth = 90
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.Width = 120
        KBotDataColumn11.HeaderText = "Fel"
        KBotDataColumn11.Key = "fel"
        KBotDataColumn11.MinWidth = 100
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.Width = 140
        KBotDataColumn12.HeaderText = "Rânduri"
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.Key = "randuri"
        KBotDataColumn12.MinWidth = 70
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn12.Width = 80
        KBotDataColumn13.HeaderText = "Exemplu — cheie"
        KBotDataColumn13.Key = "cheie"
        KBotDataColumn13.MinWidth = 100
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.Width = 130
        KBotDataColumn14.HeaderText = "Exemplu — ce nu e în regulă"
        KBotDataColumn14.Key = "mesaj"
        KBotDataColumn14.MinWidth = 160
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.Width = 280
        KBotDataColumn15.HeaderText = "Exemplu — valoare"
        KBotDataColumn15.Key = "valoare"
        KBotDataColumn15.MinWidth = 110
        KBotDataColumn15.ReadOnly = True
        KBotDataColumn15.Width = 150
        dgvConstatari.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvConstatari.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        dgvConstatari.Columns.Add(KBotDataColumn8)
        dgvConstatari.Columns.Add(KBotDataColumn9)
        dgvConstatari.Columns.Add(KBotDataColumn10)
        dgvConstatari.Columns.Add(KBotDataColumn11)
        dgvConstatari.Columns.Add(KBotDataColumn12)
        dgvConstatari.Columns.Add(KBotDataColumn13)
        dgvConstatari.Columns.Add(KBotDataColumn14)
        dgvConstatari.Columns.Add(KBotDataColumn15)
        dgvConstatari.Dock = DockStyle.Fill
        dgvConstatari.FillColumnKey = "mesaj"
        dgvConstatari.HeaderHeight = 30
        dgvConstatari.Location = New Point(3, 3)
        dgvConstatari.Margin = New Padding(0)
        dgvConstatari.Name = "dgvConstatari"
        dgvConstatari.ReadOnlyGrid = True
        dgvConstatari.RowHeight = 26
        dgvConstatari.Size = New Size(484, 199)
        dgvConstatari.TabIndex = 0
        sfat.SetToolTipHeader(dgvConstatari, "Ce a găsit analiza")
        sfat.SetToolTipText(dgvConstatari, "Un rând = un FEL de problemă, cu numărul de rânduri lovite și un exemplu." & vbLf & "Dublu-click pe rând scrie TOATE exemplele lui în jurnal, de unde se pot copia.")
        ' 
        ' tabPagCorelatii
        ' 
        tabPagCorelatii.Controls.Add(dgvCorelatii)
        tabPagCorelatii.Controls.Add(lblCorelatii)
        tabPagCorelatii.Location = New Point(4, 34)
        tabPagCorelatii.Name = "tabPagCorelatii"
        tabPagCorelatii.Padding = New Padding(3)
        tabPagCorelatii.Size = New Size(490, 205)
        tabPagCorelatii.TabIndex = 1
        tabPagCorelatii.Text = "Corelații coloane"
        tabPagCorelatii.UseVisualStyleBackColor = True
        ' 
        ' dgvCorelatii
        ' 
        KBotDataColumn16.HeaderText = "Coloană în Access"
        KBotDataColumn16.Key = "access"
        KBotDataColumn16.MinWidth = 140
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.Width = 190
        KBotDataColumn17.ColumnType = KBot.Controls.KBotColumnType.Combo
        KBotDataColumn17.HeaderText = "Se scrie în (MariaDB)"
        KBotDataColumn17.Key = "tinta"
        KBotDataColumn17.MinWidth = 140
        KBotDataColumn17.Width = 200
        KBotDataColumn18.HeaderText = "Propus de server"
        KBotDataColumn18.Key = "implicit"
        KBotDataColumn18.MinWidth = 130
        KBotDataColumn18.ReadOnly = True
        KBotDataColumn18.Width = 180
        KBotDataColumn19.HeaderText = "Stare"
        KBotDataColumn19.Key = "stare"
        KBotDataColumn19.MinWidth = 120
        KBotDataColumn19.ReadOnly = True
        KBotDataColumn19.Width = 150
        dgvCorelatii.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        dgvCorelatii.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        dgvCorelatii.Columns.Add(KBotDataColumn16)
        dgvCorelatii.Columns.Add(KBotDataColumn17)
        dgvCorelatii.Columns.Add(KBotDataColumn18)
        dgvCorelatii.Columns.Add(KBotDataColumn19)
        dgvCorelatii.Dock = DockStyle.Fill
        dgvCorelatii.FillColumnKey = "stare"
        dgvCorelatii.HeaderHeight = 30
        dgvCorelatii.Location = New Point(3, 33)
        dgvCorelatii.Margin = New Padding(0)
        dgvCorelatii.Name = "dgvCorelatii"
        dgvCorelatii.RowHeight = 26
        dgvCorelatii.Size = New Size(484, 169)
        dgvCorelatii.TabIndex = 1
        sfat.SetToolTipFooter(dgvCorelatii, "Dublu-click sau F2 pe «Se scrie în» deschide lista.")
        sfat.SetToolTipHeader(dgvCorelatii, "În ce coloană de pe MariaDB ajunge fiecare coloană din Access")
        sfat.SetToolTipText(dgvCorelatii, resources.GetString("dgvCorelatii.ToolTipText"))
        ' 
        ' lblCorelatii
        ' 
        lblCorelatii.Dock = DockStyle.Top
        lblCorelatii.Location = New Point(3, 3)
        lblCorelatii.Name = "lblCorelatii"
        lblCorelatii.Size = New Size(484, 30)
        lblCorelatii.TabIndex = 0
        lblCorelatii.Text = "Corelații:"
        lblCorelatii.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtJurnal
        ' 
        txtJurnal.Dock = DockStyle.Bottom
        txtJurnal.Location = New Point(0, 462)
        txtJurnal.Multiline = True
        txtJurnal.Name = "txtJurnal"
        txtJurnal.ReadOnly = True
        txtJurnal.ScrollBars = ScrollBars.Vertical
        txtJurnal.Size = New Size(1240, 154)
        txtJurnal.TabIndex = 3
        ' 
        ' dlgFisier
        ' 
        dlgFisier.Filter = "Baze Access (*.accdb)|*.accdb|Toate fișierele (*.*)|*.*"
        dlgFisier.Title = "Alege fișierul Access"
        ' 
        ' MigratorForm
        ' 
        ClientSize = New Size(1240, 712)
        Controls.Add(tabRezultate)
        Controls.Add(pnlColoane)
        Controls.Add(pnlTabele)
        Controls.Add(txtJurnal)
        Controls.Add(pnlActiuni)
        Controls.Add(pnlSurse)
        MinimumSize = New Size(1140, 640)
        Name = "MigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Migrare FX — Access ▸ MariaDB"
        pnlSurse.ResumeLayout(False)
        tlpSurse.ResumeLayout(False)
        tlpSurse.PerformLayout()
        pnlTabele.ResumeLayout(False)
        CType(dgvTabele, ComponentModel.ISupportInitialize).EndInit()
        pnlOrdine.ResumeLayout(False)
        pnlColoane.ResumeLayout(False)
        CType(dgvColoane, ComponentModel.ISupportInitialize).EndInit()
        pnlActiuni.ResumeLayout(False)
        tlpActiuni.ResumeLayout(False)
        tlpActiuni.PerformLayout()
        CType(dgvConstatari, ComponentModel.ISupportInitialize).EndInit()
        CType(dgvCorelatii, ComponentModel.ISupportInitialize).EndInit()
        tabPagConstatari.ResumeLayout(False)
        tabPagCorelatii.ResumeLayout(False)
        tabRezultate.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

End Class
