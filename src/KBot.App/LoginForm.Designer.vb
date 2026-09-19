<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LoginForm
    Inherits Global.KBot.Theming.KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(LoginForm))
        tips = New KBot.Controls.KBotToolTip(components)
        tlpBody = New Controls.KBotTableLayoutPanel()
        busyBar = New Controls.KBotBusyBar()
        picLogo = New PictureBox()
        lblTitle = New Label()
        lblSubtitle = New Label()
        lblUser = New Label()
        txtUser = New Controls.KBotTextField()
        lblPass = New Label()
        txtPass = New Controls.KBotTextField()
        btnContinue = New Button()
        lblUnit = New Label()
        cboUnit = New Controls.KBotComboBox()
        btnBack = New Button()
        btnLogin = New Button()
        ntfError = New Controls.KBotNotice()
        capBar = New Controls.KBotCaptionBar()
        tlpBody.SuspendLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlpBody
        '
        tlpBody.ColumnCount = 2
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        tlpBody.Controls.Add(busyBar, 0, 0)
        tlpBody.Controls.Add(picLogo, 0, 1)
        tlpBody.Controls.Add(lblTitle, 0, 2)
        tlpBody.Controls.Add(lblSubtitle, 0, 3)
        tlpBody.Controls.Add(lblUser, 0, 4)
        tlpBody.Controls.Add(txtUser, 0, 5)
        tlpBody.Controls.Add(lblPass, 0, 6)
        tlpBody.Controls.Add(txtPass, 0, 7)
        tlpBody.Controls.Add(btnContinue, 0, 8)
        tlpBody.Controls.Add(lblUnit, 0, 9)
        tlpBody.Controls.Add(cboUnit, 0, 10)
        tlpBody.Controls.Add(btnBack, 0, 11)
        tlpBody.Controls.Add(btnLogin, 1, 11)
        tlpBody.Controls.Add(ntfError, 0, 12)
        tlpBody.Dock = DockStyle.Fill
        tlpBody.GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        tlpBody.Location = New Point(1, 69)
        tlpBody.Margin = New Padding(4, 5, 4, 5)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(40, 13, 40, 17)
        tlpBody.RowCount = 14
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle())
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpBody.Size = New Size(516, 829)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        '
        ' busyBar
        '
        tlpBody.SetColumnSpan(busyBar, 2)
        busyBar.Dock = DockStyle.Fill
        busyBar.Location = New Point(40, 13)
        busyBar.Margin = New Padding(0, 0, 0, 10)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(436, 5)
        busyBar.TabIndex = 0
        busyBar.TabStop = False
        '
        ' picLogo
        '
        tlpBody.SetColumnSpan(picLogo, 2)
        picLogo.Dock = DockStyle.Fill
        picLogo.Location = New Point(44, 38)
        picLogo.Margin = New Padding(4, 10, 4, 10)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(428, 107)
        picLogo.SizeMode = PictureBoxSizeMode.Zoom
        picLogo.TabIndex = 1
        picLogo.TabStop = False
        '
        ' lblTitle
        '
        lblTitle.AutoSize = True
        tlpBody.SetColumnSpan(lblTitle, 2)
        lblTitle.Dock = DockStyle.Top
        lblTitle.Font = New Font("Segoe UI", 18.0F, FontStyle.Bold)
        lblTitle.Location = New Point(44, 155)
        lblTitle.Margin = New Padding(4, 0, 4, 3)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(428, 48)
        lblTitle.TabIndex = 2
        lblTitle.Text = "K-BOT"
        lblTitle.TextAlign = ContentAlignment.MiddleCenter
        '
        ' lblSubtitle
        '
        lblSubtitle.AutoSize = True
        tlpBody.SetColumnSpan(lblSubtitle, 2)
        lblSubtitle.Dock = DockStyle.Top
        lblSubtitle.Font = New Font("Segoe UI", 10.0F)
        lblSubtitle.Location = New Point(44, 206)
        lblSubtitle.Margin = New Padding(4, 0, 4, 20)
        lblSubtitle.Name = "lblSubtitle"
        lblSubtitle.Size = New Size(428, 28)
        lblSubtitle.TabIndex = 3
        lblSubtitle.Text = "Autentificare operator"
        lblSubtitle.TextAlign = ContentAlignment.MiddleCenter
        '
        ' lblUser
        '
        lblUser.AutoSize = True
        tlpBody.SetColumnSpan(lblUser, 2)
        lblUser.Dock = DockStyle.Top
        lblUser.Location = New Point(44, 254)
        lblUser.Margin = New Padding(4, 0, 4, 5)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(428, 22)
        lblUser.TabIndex = 4
        lblUser.Text = "Utilizator"
        '
        ' txtUser
        '
        txtUser.BackColor = Color.Transparent
        tlpBody.SetColumnSpan(txtUser, 2)
        txtUser.Dock = DockStyle.Fill
        txtUser.Location = New Point(44, 281)
        txtUser.Margin = New Padding(4, 0, 4, 17)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(428, 60)
        txtUser.TabIndex = 5
        tips.SetToolTipHeader(txtUser, "Utilizator")
        tips.SetToolTipText(txtUser, "Adresa de e-mail cu care ești înregistrat în K-BOT." & vbLf & "Ea ține locul vechiului nume de utilizator.")
        '
        ' lblPass
        '
        lblPass.AutoSize = True
        tlpBody.SetColumnSpan(lblPass, 2)
        lblPass.Dock = DockStyle.Top
        lblPass.Location = New Point(44, 358)
        lblPass.Margin = New Padding(4, 0, 4, 5)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(428, 22)
        lblPass.TabIndex = 6
        lblPass.Text = "Parolă"
        '
        ' txtPass
        '
        txtPass.BackColor = Color.Transparent
        tlpBody.SetColumnSpan(txtPass, 2)
        txtPass.Dock = DockStyle.Fill
        txtPass.Location = New Point(44, 385)
        txtPass.Margin = New Padding(4, 0, 4, 23)
        txtPass.Name = "txtPass"
        txtPass.Size = New Size(428, 60)
        txtPass.TabIndex = 7
        tips.SetToolTipHeader(txtPass, "Parolă")
        tips.SetToolTipText(txtPass, "Parola contului." & vbLf & "Se trimite criptat; nu se păstrează pe acest calculator.")
        txtPass.UseSystemPasswordChar = True
        '
        ' btnContinue
        '
        tlpBody.SetColumnSpan(btnContinue, 2)
        btnContinue.Dock = DockStyle.Fill
        btnContinue.FlatStyle = FlatStyle.Flat
        btnContinue.Font = New Font("Segoe UI Semibold", 10.0F)
        btnContinue.Location = New Point(44, 468)
        btnContinue.Margin = New Padding(4, 0, 4, 10)
        btnContinue.Name = "btnContinue"
        btnContinue.Size = New Size(428, 63)
        btnContinue.TabIndex = 8
        btnContinue.Text = "Continuă"
        tips.SetToolTipHeader(btnContinue, "Continuă")
        tips.SetToolTipText(btnContinue, "Verifică utilizatorul și parola, apoi trece la alegerea unității.")
        btnContinue.UseVisualStyleBackColor = True
        '
        ' lblUnit
        '
        lblUnit.AutoSize = True
        tlpBody.SetColumnSpan(lblUnit, 2)
        lblUnit.Dock = DockStyle.Top
        lblUnit.Location = New Point(44, 551)
        lblUnit.Margin = New Padding(4, 10, 4, 5)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(428, 22)
        lblUnit.TabIndex = 9
        lblUnit.Text = "Selectați unitatea"
        '
        ' cboUnit
        '
        cboUnit.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        tlpBody.SetColumnSpan(cboUnit, 2)
        cboUnit.CornerRadius = 4
        cboUnit.DrawMode = DrawMode.OwnerDrawFixed
        cboUnit.DropDownStyle = ComboBoxStyle.DropDownList
        cboUnit.Enabled = False
        cboUnit.FlatStyle = FlatStyle.Flat
        cboUnit.Font = New Font("Segoe UI", 10.0F)
        cboUnit.ItemHeight = 36
        cboUnit.Location = New Point(44, 583)
        cboUnit.Margin = New Padding(4, 5, 4, 20)
        cboUnit.Name = "cboUnit"
        cboUnit.Size = New Size(428, 42)
        cboUnit.TabIndex = 10
        tips.SetToolTipHeader(cboUnit, "Unitate")
        tips.SetToolTipText(cboUnit, "Unitatea (baza de date) în care vei lucra." & vbLf & "Se poate schimba doar reluând autentificarea.")
        '
        ' btnBack
        '
        btnBack.Dock = DockStyle.Fill
        btnBack.Enabled = False
        btnBack.FlatStyle = FlatStyle.Flat
        btnBack.Font = New Font("Segoe UI", 9.0F)
        btnBack.Location = New Point(44, 645)
        btnBack.Margin = New Padding(4, 0, 9, 0)
        btnBack.Name = "btnBack"
        btnBack.Size = New Size(205, 63)
        btnBack.TabIndex = 11
        btnBack.Text = "Înapoi"
        tips.SetToolTipHeader(btnBack, "Înapoi")
        tips.SetToolTipText(btnBack, "Revino la utilizator și parolă, fără să te autentifici.")
        btnBack.UseVisualStyleBackColor = True
        '
        ' btnLogin
        '
        btnLogin.Dock = DockStyle.Fill
        btnLogin.Enabled = False
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.Font = New Font("Segoe UI Semibold", 10.0F)
        btnLogin.Location = New Point(267, 645)
        btnLogin.Margin = New Padding(9, 0, 4, 0)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(205, 63)
        btnLogin.TabIndex = 12
        btnLogin.Text = "Autentificare"
        tips.SetToolTipHeader(btnLogin, "Autentificare")
        tips.SetToolTipText(btnLogin, "Intră în aplicație cu unitatea aleasă.")
        btnLogin.UseVisualStyleBackColor = True
        '
        ' ntfError
        '
        ntfError.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        ntfError.BackColor = Color.Transparent
        tlpBody.SetColumnSpan(ntfError, 2)
        ntfError.Location = New Point(44, 718)
        ntfError.Margin = New Padding(4, 10, 4, 5)
        ntfError.Name = "ntfError"
        ntfError.Size = New Size(428, 67)
        ntfError.TabIndex = 13
        ntfError.TabStop = False
        ntfError.Visible = False
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(1, 2)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowThemeButton = True
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(516, 67)
        capBar.TabIndex = 1
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        '
        ' LoginForm
        '
        AutoScaleDimensions = New SizeF(144.0F, 144.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(518, 900)
        Controls.Add(tlpBody)
        Controls.Add(capBar)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "LoginForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Autentificare"
        tlpBody.ResumeLayout(False)
        tlpBody.PerformLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents tlpBody As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents busyBar As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents picLogo As PictureBox
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As Global.KBot.Controls.KBotTextField
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As Global.KBot.Controls.KBotTextField
    Friend WithEvents btnContinue As Button
    Friend WithEvents lblUnit As Label
    Friend WithEvents cboUnit As Global.KBot.Controls.KBotComboBox
    Friend WithEvents btnBack As Button
    Friend WithEvents btnLogin As Button
    Friend WithEvents ntfError As Global.KBot.Controls.KBotNotice
End Class
