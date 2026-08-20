<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Toate controalele sunt declarate AICI, ca formularul să se randeze în designerul VS
    ' (docs/kbot-forms-ui-convention.md). Nimic nu se construiește în cod.
    Private components As System.ComponentModel.IContainer

    ' --- regiunea 1: sursa -----------------------------------------------------
    Friend WithEvents pnlSurse As System.Windows.Forms.Panel
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
    Friend WithEvents lblCai As System.Windows.Forms.Label
    Friend WithEvents txtCai As System.Windows.Forms.TextBox
    Friend WithEvents btnRasfoireCai As System.Windows.Forms.Button
    Friend WithEvents btnImpinge As System.Windows.Forms.Button
    Friend WithEvents prgPush As System.Windows.Forms.ProgressBar
    Friend WithEvents lblFisiere As System.Windows.Forms.Label

    ' --- regiunea 2: actiuni ---------------------------------------------------
    Friend WithEvents pnlActiuni As System.Windows.Forms.Panel
    Friend WithEvents btnAnalizeaza As System.Windows.Forms.Button
    Friend WithEvents btnRuleaza As System.Windows.Forms.Button
    Friend WithEvents btnForteaza As System.Windows.Forms.Button
    Friend WithEvents lblStare As System.Windows.Forms.Label

    ' --- regiunea 3: constatari + jurnal --------------------------------------
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
        Me.components = New System.ComponentModel.Container()
        Me.pnlSurse = New System.Windows.Forms.Panel()
        Me.lblDc = New System.Windows.Forms.Label()
        Me.cboDc = New System.Windows.Forms.ComboBox()
        Me.lblUnitate = New System.Windows.Forms.Label()
        Me.lblAn = New System.Windows.Forms.Label()
        Me.cboAn = New System.Windows.Forms.ComboBox()
        Me.lblBaza = New System.Windows.Forms.Label()
        Me.cboBaza = New System.Windows.Forms.ComboBox()
        Me.btnReciteste = New System.Windows.Forms.Button()
        Me.lblFx = New System.Windows.Forms.Label()
        Me.txtFx = New System.Windows.Forms.TextBox()
        Me.btnRasfoireFx = New System.Windows.Forms.Button()
        Me.lblCai = New System.Windows.Forms.Label()
        Me.txtCai = New System.Windows.Forms.TextBox()
        Me.btnRasfoireCai = New System.Windows.Forms.Button()
        Me.btnImpinge = New System.Windows.Forms.Button()
        Me.prgPush = New System.Windows.Forms.ProgressBar()
        Me.lblFisiere = New System.Windows.Forms.Label()
        Me.pnlActiuni = New System.Windows.Forms.Panel()
        Me.btnAnalizeaza = New System.Windows.Forms.Button()
        Me.btnRuleaza = New System.Windows.Forms.Button()
        Me.btnForteaza = New System.Windows.Forms.Button()
        Me.lblStare = New System.Windows.Forms.Label()
        Me.dgvConstatari = New System.Windows.Forms.DataGridView()
        Me.txtJurnal = New System.Windows.Forms.TextBox()
        Me.dlgFisier = New System.Windows.Forms.OpenFileDialog()
        Me.sfat = New KBot.Controls.KBotToolTip(Me.components)
        Me.pnlSurse.SuspendLayout()
        Me.pnlActiuni.SuspendLayout()
        CType(Me.dgvConstatari, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblDc
        '
        Me.lblDc.AutoSize = True
        Me.lblDc.Location = New System.Drawing.Point(14, 16)
        Me.lblDc.Name = "lblDc"
        Me.lblDc.Size = New System.Drawing.Size(120, 15)
        Me.lblDc.TabIndex = 0
        Me.lblDc.Text = "Unitatea (din registru):"
        '
        'cboDc
        '
        Me.cboDc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboDc.Location = New System.Drawing.Point(150, 13)
        Me.cboDc.Name = "cboDc"
        Me.cboDc.Size = New System.Drawing.Size(300, 23)
        Me.cboDc.TabIndex = 1
        '
        'lblUnitate
        '
        Me.lblUnitate.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblUnitate.Location = New System.Drawing.Point(460, 16)
        Me.lblUnitate.Name = "lblUnitate"
        Me.lblUnitate.Size = New System.Drawing.Size(480, 18)
        Me.lblUnitate.TabIndex = 2
        Me.lblUnitate.Text = "—"
        '
        'lblAn
        '
        Me.lblAn.AutoSize = True
        Me.lblAn.Location = New System.Drawing.Point(14, 47)
        Me.lblAn.Name = "lblAn"
        Me.lblAn.Size = New System.Drawing.Size(24, 15)
        Me.lblAn.TabIndex = 3
        Me.lblAn.Text = "An:"
        '
        'cboAn
        '
        Me.cboAn.Location = New System.Drawing.Point(150, 44)
        Me.cboAn.Name = "cboAn"
        Me.cboAn.Size = New System.Drawing.Size(100, 23)
        Me.cboAn.TabIndex = 4
        '
        'lblBaza
        '
        Me.lblBaza.AutoSize = True
        Me.lblBaza.Location = New System.Drawing.Point(276, 47)
        Me.lblBaza.Name = "lblBaza"
        Me.lblBaza.Size = New System.Drawing.Size(110, 15)
        Me.lblBaza.TabIndex = 5
        Me.lblBaza.Text = "Baza țintă (MariaDB):"
        '
        'cboBaza
        '
        Me.cboBaza.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboBaza.Location = New System.Drawing.Point(400, 44)
        Me.cboBaza.Name = "cboBaza"
        Me.cboBaza.Size = New System.Drawing.Size(260, 23)
        Me.cboBaza.TabIndex = 6
        '
        'btnReciteste
        '
        Me.btnReciteste.Location = New System.Drawing.Point(670, 43)
        Me.btnReciteste.Name = "btnReciteste"
        Me.btnReciteste.Size = New System.Drawing.Size(130, 25)
        Me.btnReciteste.TabIndex = 7
        Me.btnReciteste.Text = "Recitește serverul"
        Me.btnReciteste.UseVisualStyleBackColor = True
        '
        'lblFx
        '
        Me.lblFx.AutoSize = True
        Me.lblFx.Location = New System.Drawing.Point(14, 79)
        Me.lblFx.Name = "lblFx"
        Me.lblFx.Size = New System.Drawing.Size(100, 15)
        Me.lblFx.TabIndex = 8
        Me.lblFx.Text = "Fișier FOREXE:"
        '
        'txtFx
        '
        Me.txtFx.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtFx.Location = New System.Drawing.Point(150, 76)
        Me.txtFx.Name = "txtFx"
        Me.txtFx.Size = New System.Drawing.Size(680, 23)
        Me.txtFx.TabIndex = 9
        '
        'btnRasfoireFx
        '
        Me.btnRasfoireFx.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnRasfoireFx.Location = New System.Drawing.Point(838, 75)
        Me.btnRasfoireFx.Name = "btnRasfoireFx"
        Me.btnRasfoireFx.Size = New System.Drawing.Size(102, 25)
        Me.btnRasfoireFx.TabIndex = 10
        Me.btnRasfoireFx.Text = "Răsfoiește…"
        Me.btnRasfoireFx.UseVisualStyleBackColor = True
        '
        'lblCai
        '
        Me.lblCai.AutoSize = True
        Me.lblCai.Location = New System.Drawing.Point(14, 110)
        Me.lblCai.Name = "lblCai"
        Me.lblCai.Size = New System.Drawing.Size(100, 15)
        Me.lblCai.TabIndex = 11
        Me.lblCai.Text = "Fișier cale.accdb:"
        '
        'txtCai
        '
        Me.txtCai.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtCai.Location = New System.Drawing.Point(150, 107)
        Me.txtCai.Name = "txtCai"
        Me.txtCai.Size = New System.Drawing.Size(680, 23)
        Me.txtCai.TabIndex = 12
        '
        'btnRasfoireCai
        '
        Me.btnRasfoireCai.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnRasfoireCai.Location = New System.Drawing.Point(838, 106)
        Me.btnRasfoireCai.Name = "btnRasfoireCai"
        Me.btnRasfoireCai.Size = New System.Drawing.Size(102, 25)
        Me.btnRasfoireCai.TabIndex = 13
        Me.btnRasfoireCai.Text = "Răsfoiește…"
        Me.btnRasfoireCai.UseVisualStyleBackColor = True
        '
        'btnImpinge
        '
        Me.btnImpinge.Location = New System.Drawing.Point(150, 139)
        Me.btnImpinge.Name = "btnImpinge"
        Me.btnImpinge.Size = New System.Drawing.Size(180, 28)
        Me.btnImpinge.TabIndex = 14
        Me.btnImpinge.Text = "Împinge pe server"
        Me.btnImpinge.UseVisualStyleBackColor = True
        '
        'prgPush
        '
        Me.prgPush.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.prgPush.Location = New System.Drawing.Point(340, 143)
        Me.prgPush.Name = "prgPush"
        Me.prgPush.Size = New System.Drawing.Size(300, 20)
        Me.prgPush.TabIndex = 15
        '
        'lblFisiere
        '
        Me.lblFisiere.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblFisiere.Location = New System.Drawing.Point(650, 145)
        Me.lblFisiere.Name = "lblFisiere"
        Me.lblFisiere.Size = New System.Drawing.Size(290, 18)
        Me.lblFisiere.TabIndex = 16
        Me.lblFisiere.Text = "Pe server: —"
        '
        'pnlSurse
        '
        Me.pnlSurse.Controls.Add(Me.lblFisiere)
        Me.pnlSurse.Controls.Add(Me.prgPush)
        Me.pnlSurse.Controls.Add(Me.btnImpinge)
        Me.pnlSurse.Controls.Add(Me.btnRasfoireCai)
        Me.pnlSurse.Controls.Add(Me.txtCai)
        Me.pnlSurse.Controls.Add(Me.lblCai)
        Me.pnlSurse.Controls.Add(Me.btnRasfoireFx)
        Me.pnlSurse.Controls.Add(Me.txtFx)
        Me.pnlSurse.Controls.Add(Me.lblFx)
        Me.pnlSurse.Controls.Add(Me.btnReciteste)
        Me.pnlSurse.Controls.Add(Me.cboBaza)
        Me.pnlSurse.Controls.Add(Me.lblBaza)
        Me.pnlSurse.Controls.Add(Me.cboAn)
        Me.pnlSurse.Controls.Add(Me.lblAn)
        Me.pnlSurse.Controls.Add(Me.lblUnitate)
        Me.pnlSurse.Controls.Add(Me.cboDc)
        Me.pnlSurse.Controls.Add(Me.lblDc)
        Me.pnlSurse.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSurse.Location = New System.Drawing.Point(0, 0)
        Me.pnlSurse.Name = "pnlSurse"
        Me.pnlSurse.Size = New System.Drawing.Size(954, 178)
        Me.pnlSurse.TabIndex = 0
        '
        'btnAnalizeaza
        '
        Me.btnAnalizeaza.Location = New System.Drawing.Point(14, 14)
        Me.btnAnalizeaza.Name = "btnAnalizeaza"
        Me.btnAnalizeaza.Size = New System.Drawing.Size(180, 32)
        Me.btnAnalizeaza.TabIndex = 0
        Me.btnAnalizeaza.Text = "Analizează"
        Me.btnAnalizeaza.UseVisualStyleBackColor = True
        '
        'btnRuleaza
        '
        Me.btnRuleaza.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnRuleaza.Enabled = False
        Me.btnRuleaza.Location = New System.Drawing.Point(590, 14)
        Me.btnRuleaza.Name = "btnRuleaza"
        Me.btnRuleaza.Size = New System.Drawing.Size(160, 32)
        Me.btnRuleaza.TabIndex = 1
        Me.btnRuleaza.Text = "Rulează"
        Me.btnRuleaza.UseVisualStyleBackColor = True
        '
        'btnForteaza
        '
        Me.btnForteaza.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnForteaza.Enabled = False
        Me.btnForteaza.Location = New System.Drawing.Point(758, 14)
        Me.btnForteaza.Name = "btnForteaza"
        Me.btnForteaza.Size = New System.Drawing.Size(182, 32)
        Me.btnForteaza.TabIndex = 2
        Me.btnForteaza.Text = "Forțează rularea"
        Me.btnForteaza.UseVisualStyleBackColor = True
        '
        'lblStare
        '
        Me.lblStare.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblStare.Location = New System.Drawing.Point(204, 20)
        Me.lblStare.Name = "lblStare"
        Me.lblStare.Size = New System.Drawing.Size(376, 34)
        Me.lblStare.TabIndex = 3
        Me.lblStare.Text = "Alege unitatea, anul și baza țintă."
        '
        'pnlActiuni
        '
        Me.pnlActiuni.Controls.Add(Me.lblStare)
        Me.pnlActiuni.Controls.Add(Me.btnForteaza)
        Me.pnlActiuni.Controls.Add(Me.btnRuleaza)
        Me.pnlActiuni.Controls.Add(Me.btnAnalizeaza)
        Me.pnlActiuni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlActiuni.Location = New System.Drawing.Point(0, 561)
        Me.pnlActiuni.Name = "pnlActiuni"
        Me.pnlActiuni.Size = New System.Drawing.Size(954, 60)
        Me.pnlActiuni.TabIndex = 3
        '
        'dgvConstatari
        '
        Me.dgvConstatari.AllowUserToAddRows = False
        Me.dgvConstatari.AllowUserToDeleteRows = False
        Me.dgvConstatari.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvConstatari.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvConstatari.Location = New System.Drawing.Point(0, 178)
        Me.dgvConstatari.MultiSelect = False
        Me.dgvConstatari.Name = "dgvConstatari"
        Me.dgvConstatari.ReadOnly = True
        Me.dgvConstatari.RowHeadersVisible = False
        Me.dgvConstatari.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvConstatari.Size = New System.Drawing.Size(954, 203)
        Me.dgvConstatari.TabIndex = 1
        '
        'txtJurnal
        '
        Me.txtJurnal.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.txtJurnal.Location = New System.Drawing.Point(0, 381)
        Me.txtJurnal.Multiline = True
        Me.txtJurnal.Name = "txtJurnal"
        Me.txtJurnal.ReadOnly = True
        Me.txtJurnal.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtJurnal.Size = New System.Drawing.Size(954, 180)
        Me.txtJurnal.TabIndex = 2
        '
        'dlgFisier
        '
        Me.dlgFisier.Filter = "Baze Access (*.accdb)|*.accdb|Toate fișierele (*.*)|*.*"
        Me.dlgFisier.Title = "Alege fișierul Access"
        '
        'sfat
        '
        Me.sfat.SetToolTipHeader(Me.cboDc, "Unitățile instalate")
        Me.sfat.SetToolTipText(Me.cboDc, "Citite din HKCU\Software\VB and VBA Program Settings\AVACONT." & Global.Microsoft.VisualBasic.ChrW(10) & "Migratorul doar citește registrul; nu scrie nimic în el.")
        Me.sfat.SetToolTipHeader(Me.cboBaza, "Baza de pe MariaDB")
        Me.sfat.SetToolTipText(Me.cboBaza, "Rândurile se rutează prin [Cai]; se scriu doar cele care ajung aici." & Global.Microsoft.VisualBasic.ChrW(10) & "Migrarea NU creează tabele.")
        Me.sfat.SetToolTipHeader(Me.txtFx, "Fișierul FOREXE al anului")
        Me.sfat.SetToolTipText(Me.txtFx, "Trebuie să fie FĂRĂ parolă de bază de date." & Global.Microsoft.VisualBasic.ChrW(10) & "Serverul citește cu mdbtools, care nu poate decripta.")
        Me.sfat.SetToolTipFooter(Me.txtFx, "În Access: Fișier ▸ Informații ▸ Decriptare bază de date.")
        Me.sfat.SetToolTipHeader(Me.txtCai, "cale.accdb")
        Me.sfat.SetToolTipText(Me.txtCai, "Poartă tabelul [Cai] — legătura IdUnitate → bază de date." & Global.Microsoft.VisualBasic.ChrW(10) & "Fără el nu se poate ruta niciun rând. Tot fără parolă.")
        Me.sfat.SetToolTipHeader(Me.btnRuleaza, "Rulează")
        Me.sfat.SetToolTipText(Me.btnRuleaza, "Pornește doar dacă analiza n-a găsit absolut nimic.")
        Me.sfat.SetToolTipHeader(Me.btnForteaza, "Forțează rularea")
        Me.sfat.SetToolTipText(Me.btnForteaza, "Pornește când singurele probleme sunt de integritate (chei străine," & Global.Microsoft.VisualBasic.ChrW(10) & "id-uri DDF, chei duble, rânduri nerutabile). Acele rânduri se SAR." & Global.Microsoft.VisualBasic.ChrW(10) & "Problemele de tip sau de dimensiune opresc și acest buton.")
        '
        'MigratorForm
        '
        Me.ClientSize = New System.Drawing.Size(954, 621)
        Me.Controls.Add(Me.dgvConstatari)
        Me.Controls.Add(Me.txtJurnal)
        Me.Controls.Add(Me.pnlActiuni)
        Me.Controls.Add(Me.pnlSurse)
        Me.MinimumSize = New System.Drawing.Size(900, 600)
        Me.Name = "MigratorForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Migrare FX — Access ▸ MariaDB"
        Me.pnlSurse.ResumeLayout(False)
        Me.pnlSurse.PerformLayout()
        Me.pnlActiuni.ResumeLayout(False)
        CType(Me.dgvConstatari, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

End Class
