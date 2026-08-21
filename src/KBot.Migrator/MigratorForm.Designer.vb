<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Toate controalele sunt declarate AICI, ca formularul să se randeze în designerul VS
    ' (docs/kbot-forms-ui-convention.md). Nimic nu se construiește în cod.
    ' Fiecare panou mare își ține câmpurile într-un TableLayoutPanel, ca aranjarea să se
    ' facă din celule (rânduri/coloane), nu din coordonate scrise de mână. Singura
    ' excepție este grila de constatări, andocată direct pe formular.
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
    Friend WithEvents dgvTabele As System.Windows.Forms.DataGridView
    Friend WithEvents colBifa As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents colTabel As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colRanduri As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colAleUnitatii As System.Windows.Forms.DataGridViewTextBoxColumn

    ' --- regiunea 3: actiuni ---------------------------------------------------
    Friend WithEvents pnlActiuni As System.Windows.Forms.Panel
    Friend WithEvents tlpActiuni As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnInventar As System.Windows.Forms.Button
    Friend WithEvents btnAnalizeaza As System.Windows.Forms.Button
    Friend WithEvents btnRuleaza As System.Windows.Forms.Button
    Friend WithEvents btnForteaza As System.Windows.Forms.Button
    Friend WithEvents lblStare As System.Windows.Forms.Label

    ' --- regiunea 4: constatari + jurnal --------------------------------------
    Friend WithEvents dgvConstatari As System.Windows.Forms.DataGridView
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
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
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
        dgvTabele = New DataGridView()
        colBifa = New DataGridViewCheckBoxColumn()
        colTabel = New DataGridViewTextBoxColumn()
        colRanduri = New DataGridViewTextBoxColumn()
        colAleUnitatii = New DataGridViewTextBoxColumn()
        lblTabele = New Label()
        pnlActiuni = New Panel()
        tlpActiuni = New TableLayoutPanel()
        btnInventar = New Button()
        btnAnalizeaza = New Button()
        lblStare = New Label()
        btnRuleaza = New Button()
        btnForteaza = New Button()
        dgvConstatari = New DataGridView()
        txtJurnal = New TextBox()
        dlgFisier = New OpenFileDialog()
        sfat = New KBot.Controls.KBotToolTip(components)
        pnlSurse.SuspendLayout()
        tlpSurse.SuspendLayout()
        pnlTabele.SuspendLayout()
        CType(dgvTabele, ComponentModel.ISupportInitialize).BeginInit()
        pnlActiuni.SuspendLayout()
        tlpActiuni.SuspendLayout()
        CType(dgvConstatari, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlSurse
        ' 
        pnlSurse.Controls.Add(tlpSurse)
        pnlSurse.Dock = DockStyle.Top
        pnlSurse.Location = New Point(0, 0)
        pnlSurse.Name = "pnlSurse"
        pnlSurse.Padding = New Padding(10, 8, 10, 8)
        pnlSurse.Size = New Size(954, 219)
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
        tlpSurse.Size = New Size(934, 203)
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
        lblUnitate.Size = New Size(416, 40)
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
        cboBaza.Size = New Size(210, 33)
        cboBaza.TabIndex = 6
        sfat.SetToolTipHeader(cboBaza, "Baza de pe MariaDB")
        sfat.SetToolTipText(cboBaza, "Rândurile se rutează prin [Cai]; se scriu doar cele care ajung aici." & vbLf & "Migrarea NU creează tabele.")
        ' 
        ' btnReciteste
        ' 
        tlpSurse.SetColumnSpan(btnReciteste, 2)
        btnReciteste.Dock = DockStyle.Fill
        btnReciteste.Font = New Font("Calibri", 9F, FontStyle.Bold)
        btnReciteste.Location = New Point(725, 42)
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
        txtFx.Size = New Size(520, 31)
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
        prgPush.Size = New Size(390, 26)
        prgPush.TabIndex = 15
        ' 
        ' lblFisiere
        ' 
        lblFisiere.AutoEllipsis = True
        tlpSurse.SetColumnSpan(lblFisiere, 2)
        lblFisiere.Dock = DockStyle.Fill
        lblFisiere.Location = New Point(731, 143)
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
        btnRasfoireFx.Location = New Point(725, 86)
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
        pnlTabele.Controls.Add(lblTabele)
        pnlTabele.Dock = DockStyle.Left
        pnlTabele.Location = New Point(0, 219)
        pnlTabele.Name = "pnlTabele"
        pnlTabele.Padding = New Padding(8, 6, 4, 6)
        pnlTabele.Size = New Size(442, 279)
        pnlTabele.TabIndex = 1
        ' 
        ' dgvTabele
        ' 
        dgvTabele.AllowUserToAddRows = False
        dgvTabele.AllowUserToDeleteRows = False
        dgvTabele.AllowUserToResizeRows = False
        dgvTabele.ColumnHeadersHeight = 34
        dgvTabele.Columns.AddRange(New DataGridViewColumn() {colBifa, colTabel, colRanduri, colAleUnitatii})
        dgvTabele.Dock = DockStyle.Fill
        dgvTabele.Location = New Point(8, 36)
        dgvTabele.MultiSelect = False
        dgvTabele.Name = "dgvTabele"
        dgvTabele.RowHeadersVisible = False
        dgvTabele.RowHeadersWidth = 62
        dgvTabele.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvTabele.Size = New Size(430, 237)
        dgvTabele.TabIndex = 1
        sfat.SetToolTipHeader(dgvTabele, "Ce se actualizează")
        sfat.SetToolTipText(dgvTabele, "Un tabel fără rânduri în fișierul Access se oferă NEBIFAT." & vbLf & "«Ale unității» se completează abia după analiză: acolo se află" & vbLf & "câte dintre rânduri sunt chiar ale bazei alese.")
        ' 
        ' colBifa
        ' 
        colBifa.HeaderText = ""
        colBifa.MinimumWidth = 8
        colBifa.Name = "colBifa"
        colBifa.Resizable = DataGridViewTriState.False
        colBifa.Width = 40
        ' 
        ' colTabel
        ' 
        colTabel.HeaderText = "Tabel"
        colTabel.MinimumWidth = 8
        colTabel.Name = "colTabel"
        colTabel.ReadOnly = True
        colTabel.Width = 150
        ' 
        ' colRanduri
        ' 
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleRight
        colRanduri.DefaultCellStyle = DataGridViewCellStyle1
        colRanduri.HeaderText = "Rânduri"
        colRanduri.MinimumWidth = 8
        colRanduri.Name = "colRanduri"
        colRanduri.ReadOnly = True
        colRanduri.Width = 70
        ' 
        ' colAleUnitatii
        ' 
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight
        colAleUnitatii.DefaultCellStyle = DataGridViewCellStyle2
        colAleUnitatii.HeaderText = "Ale unității"
        colAleUnitatii.MinimumWidth = 8
        colAleUnitatii.Name = "colAleUnitatii"
        colAleUnitatii.ReadOnly = True
        colAleUnitatii.Width = 90
        ' 
        ' lblTabele
        ' 
        lblTabele.Dock = DockStyle.Top
        lblTabele.Location = New Point(8, 6)
        lblTabele.Name = "lblTabele"
        lblTabele.Size = New Size(430, 30)
        lblTabele.TabIndex = 0
        lblTabele.Text = "Tabele de actualizat:"
        lblTabele.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlActiuni
        ' 
        pnlActiuni.Controls.Add(tlpActiuni)
        pnlActiuni.Dock = DockStyle.Bottom
        pnlActiuni.Location = New Point(0, 652)
        pnlActiuni.Margin = New Padding(0)
        pnlActiuni.Name = "pnlActiuni"
        pnlActiuni.Size = New Size(954, 60)
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
        tlpActiuni.Dock = DockStyle.Fill
        tlpActiuni.Location = New Point(0, 0)
        tlpActiuni.Margin = New Padding(0)
        tlpActiuni.Name = "tlpActiuni"
        tlpActiuni.RowCount = 1
        tlpActiuni.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpActiuni.Size = New Size(954, 60)
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
        lblStare.Size = New Size(210, 54)
        lblStare.TabIndex = 2
        lblStare.Text = "Alege unitatea, anul și baza țintă."
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnRuleaza
        ' 
        btnRuleaza.Dock = DockStyle.Fill
        btnRuleaza.Enabled = False
        btnRuleaza.Location = New Point(600, 3)
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
        btnForteaza.Location = New Point(769, 3)
        btnForteaza.Margin = New Padding(6, 3, 3, 3)
        btnForteaza.MinimumSize = New Size(182, 32)
        btnForteaza.Name = "btnForteaza"
        btnForteaza.Size = New Size(182, 54)
        btnForteaza.TabIndex = 4
        btnForteaza.Text = "Forțează rularea"
        sfat.SetToolTipHeader(btnForteaza, "Forțează rularea")
        sfat.SetToolTipText(btnForteaza, "Pornește când singurele probleme sunt de integritate (chei străine," & vbLf & "id-uri DDF, chei duble, rânduri nerutabile). Acele rânduri se SAR." & vbLf & "Problemele de tip sau de dimensiune opresc și acest buton.")
        btnForteaza.UseVisualStyleBackColor = True
        ' 
        ' dgvConstatari
        ' 
        dgvConstatari.AllowUserToAddRows = False
        dgvConstatari.AllowUserToDeleteRows = False
        dgvConstatari.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvConstatari.ColumnHeadersHeight = 34
        dgvConstatari.Dock = DockStyle.Fill
        dgvConstatari.Location = New Point(442, 219)
        dgvConstatari.MultiSelect = False
        dgvConstatari.Name = "dgvConstatari"
        dgvConstatari.ReadOnly = True
        dgvConstatari.RowHeadersVisible = False
        dgvConstatari.RowHeadersWidth = 62
        dgvConstatari.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvConstatari.Size = New Size(512, 279)
        dgvConstatari.TabIndex = 2
        ' 
        ' txtJurnal
        ' 
        txtJurnal.Dock = DockStyle.Bottom
        txtJurnal.Location = New Point(0, 498)
        txtJurnal.Multiline = True
        txtJurnal.Name = "txtJurnal"
        txtJurnal.ReadOnly = True
        txtJurnal.ScrollBars = ScrollBars.Vertical
        txtJurnal.Size = New Size(954, 154)
        txtJurnal.TabIndex = 3
        ' 
        ' dlgFisier
        ' 
        dlgFisier.Filter = "Baze Access (*.accdb)|*.accdb|Toate fișierele (*.*)|*.*"
        dlgFisier.Title = "Alege fișierul Access"
        ' 
        ' MigratorForm
        ' 
        ClientSize = New Size(954, 712)
        Controls.Add(dgvConstatari)
        Controls.Add(pnlTabele)
        Controls.Add(txtJurnal)
        Controls.Add(pnlActiuni)
        Controls.Add(pnlSurse)
        MinimumSize = New Size(900, 600)
        Name = "MigratorForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Migrare FX — Access ▸ MariaDB"
        pnlSurse.ResumeLayout(False)
        tlpSurse.ResumeLayout(False)
        tlpSurse.PerformLayout()
        pnlTabele.ResumeLayout(False)
        CType(dgvTabele, ComponentModel.ISupportInitialize).EndInit()
        pnlActiuni.ResumeLayout(False)
        tlpActiuni.ResumeLayout(False)
        CType(dgvConstatari, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

End Class
