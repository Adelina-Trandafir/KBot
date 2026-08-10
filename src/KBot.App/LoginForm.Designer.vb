<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LoginForm
    Inherits KBot.Theming.KBotThemedForm

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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(LoginForm))
        pnlCard = New Panel()
        tlpBody = New TableLayoutPanel()
        picLogo = New PictureBox()
        lblTitle = New Label()
        lblSubtitle = New Label()
        lblUser = New Label()
        txtUser = New Controls.KBotTextField()
        lblPass = New Label()
        txtPass = New Controls.KBotTextField()
        btnContinue = New Button()
        pnlUnit = New TableLayoutPanel()
        lblUnit = New Label()
        cboUnit = New ComboBox()
        btnBack = New Button()
        btnLogin = New Button()
        ntfError = New Controls.KBotNotice()
        busyBar = New Controls.KBotBusyBar()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlpBody.SuspendLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).BeginInit()
        pnlUnit.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(busyBar)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 2)
        pnlCard.Margin = New Padding(4, 5, 4, 5)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(598, 863)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlpBody
        ' 
        tlpBody.ColumnCount = 1
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpBody.Controls.Add(picLogo, 0, 0)
        tlpBody.Controls.Add(lblTitle, 0, 1)
        tlpBody.Controls.Add(lblSubtitle, 0, 2)
        tlpBody.Controls.Add(lblUser, 0, 3)
        tlpBody.Controls.Add(txtUser, 0, 4)
        tlpBody.Controls.Add(lblPass, 0, 5)
        tlpBody.Controls.Add(txtPass, 0, 6)
        tlpBody.Controls.Add(btnContinue, 0, 7)
        tlpBody.Controls.Add(pnlUnit, 0, 8)
        tlpBody.Controls.Add(ntfError, 0, 9)
        tlpBody.Dock = DockStyle.Fill
        tlpBody.Location = New Point(0, 72)
        tlpBody.Margin = New Padding(4, 5, 4, 5)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(40, 13, 40, 17)
        tlpBody.RowCount = 11
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
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpBody.Size = New Size(598, 791)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        ' 
        ' picLogo
        ' 
        picLogo.Anchor = AnchorStyles.None
        picLogo.Location = New Point(253, 23)
        picLogo.Margin = New Padding(4, 10, 4, 10)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(91, 107)
        picLogo.SizeMode = PictureBoxSizeMode.Zoom
        picLogo.TabIndex = 0
        picLogo.TabStop = False
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Dock = DockStyle.Top
        lblTitle.Font = New Font("Segoe UI", 18F, FontStyle.Bold)
        lblTitle.Location = New Point(44, 140)
        lblTitle.Margin = New Padding(4, 0, 4, 3)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(510, 48)
        lblTitle.TabIndex = 1
        lblTitle.Text = "K-BOT"
        lblTitle.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblSubtitle
        ' 
        lblSubtitle.AutoSize = True
        lblSubtitle.Dock = DockStyle.Top
        lblSubtitle.Font = New Font("Segoe UI", 10F)
        lblSubtitle.Location = New Point(44, 191)
        lblSubtitle.Margin = New Padding(4, 0, 4, 20)
        lblSubtitle.Name = "lblSubtitle"
        lblSubtitle.Size = New Size(510, 28)
        lblSubtitle.TabIndex = 2
        lblSubtitle.Text = "Autentificare operator"
        lblSubtitle.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblUser
        ' 
        lblUser.AutoSize = True
        lblUser.Dock = DockStyle.Top
        lblUser.Location = New Point(44, 239)
        lblUser.Margin = New Padding(4, 0, 4, 5)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(510, 25)
        lblUser.TabIndex = 3
        lblUser.Text = "Utilizator"
        ' 
        ' txtUser
        ' 
        txtUser.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtUser.BackColor = Color.Transparent
        txtUser.Location = New Point(44, 269)
        txtUser.Margin = New Padding(4, 0, 4, 17)
        txtUser.MaxLength = 32767
        txtUser.Name = "txtUser"
        txtUser.PlaceholderText = ""
        txtUser.Size = New Size(510, 60)
        txtUser.TabIndex = 4
        txtUser.TabStop = False
        txtUser.UseSystemPasswordChar = False
        ' 
        ' lblPass
        ' 
        lblPass.AutoSize = True
        lblPass.Dock = DockStyle.Top
        lblPass.Location = New Point(44, 346)
        lblPass.Margin = New Padding(4, 0, 4, 5)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(510, 25)
        lblPass.TabIndex = 5
        lblPass.Text = "Parolă"
        ' 
        ' txtPass
        ' 
        txtPass.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtPass.BackColor = Color.Transparent
        txtPass.Location = New Point(44, 376)
        txtPass.Margin = New Padding(4, 0, 4, 23)
        txtPass.MaxLength = 32767
        txtPass.Name = "txtPass"
        txtPass.PlaceholderText = ""
        txtPass.Size = New Size(510, 60)
        txtPass.TabIndex = 6
        txtPass.TabStop = False
        txtPass.UseSystemPasswordChar = True
        ' 
        ' btnContinue
        ' 
        btnContinue.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnContinue.FlatStyle = FlatStyle.Flat
        btnContinue.Font = New Font("Segoe UI Semibold", 10F)
        btnContinue.Location = New Point(44, 459)
        btnContinue.Margin = New Padding(4, 0, 4, 10)
        btnContinue.Name = "btnContinue"
        btnContinue.Size = New Size(510, 67)
        btnContinue.TabIndex = 7
        btnContinue.Text = "Continuă"
        btnContinue.UseVisualStyleBackColor = True
        ' 
        ' pnlUnit
        ' 
        pnlUnit.AutoSize = True
        pnlUnit.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlUnit.ColumnCount = 2
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        pnlUnit.Controls.Add(lblUnit, 0, 0)
        pnlUnit.Controls.Add(cboUnit, 0, 1)
        pnlUnit.Controls.Add(btnBack, 0, 2)
        pnlUnit.Controls.Add(btnLogin, 1, 2)
        pnlUnit.Dock = DockStyle.Top
        pnlUnit.Location = New Point(40, 546)
        pnlUnit.Margin = New Padding(0, 10, 0, 0)
        pnlUnit.Name = "pnlUnit"
        pnlUnit.RowCount = 3
        pnlUnit.RowStyles.Add(New RowStyle())
        pnlUnit.RowStyles.Add(New RowStyle())
        pnlUnit.RowStyles.Add(New RowStyle())
        pnlUnit.Size = New Size(518, 154)
        pnlUnit.TabIndex = 8
        pnlUnit.Tag = "Card"
        pnlUnit.Visible = False
        ' 
        ' lblUnit
        ' 
        lblUnit.AutoSize = True
        pnlUnit.SetColumnSpan(lblUnit, 2)
        lblUnit.Dock = DockStyle.Top
        lblUnit.Location = New Point(4, 0)
        lblUnit.Margin = New Padding(4, 0, 4, 5)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(510, 25)
        lblUnit.TabIndex = 0
        lblUnit.Text = "Selectați unitatea"
        ' 
        ' cboUnit
        ' 
        cboUnit.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pnlUnit.SetColumnSpan(cboUnit, 2)
        cboUnit.DropDownStyle = ComboBoxStyle.DropDownList
        cboUnit.FlatStyle = FlatStyle.Flat
        cboUnit.Font = New Font("Segoe UI", 10F)
        cboUnit.Location = New Point(4, 35)
        cboUnit.Margin = New Padding(4, 5, 4, 20)
        cboUnit.Name = "cboUnit"
        cboUnit.Size = New Size(510, 36)
        cboUnit.TabIndex = 1
        ' 
        ' btnBack
        ' 
        btnBack.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnBack.FlatStyle = FlatStyle.Flat
        btnBack.Font = New Font("Segoe UI", 9F)
        btnBack.Location = New Point(4, 91)
        btnBack.Margin = New Padding(4, 0, 9, 0)
        btnBack.Name = "btnBack"
        btnBack.Size = New Size(246, 63)
        btnBack.TabIndex = 3
        btnBack.Text = "Înapoi"
        btnBack.UseVisualStyleBackColor = True
        ' 
        ' btnLogin
        ' 
        btnLogin.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.Font = New Font("Segoe UI Semibold", 10F)
        btnLogin.Location = New Point(268, 91)
        btnLogin.Margin = New Padding(9, 0, 4, 0)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(246, 63)
        btnLogin.TabIndex = 2
        btnLogin.Text = "Autentificare"
        btnLogin.UseVisualStyleBackColor = True
        ' 
        ' ntfError
        ' 
        ntfError.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        ntfError.BackColor = Color.Transparent
        ntfError.Location = New Point(44, 710)
        ntfError.Margin = New Padding(4, 10, 4, 5)
        ntfError.Name = "ntfError"
        ntfError.Size = New Size(510, 67)
        ntfError.TabIndex = 9
        ntfError.TabStop = False
        ntfError.Visible = False
        ' 
        ' busyBar
        ' 
        busyBar.Dock = DockStyle.Top
        busyBar.Location = New Point(0, 67)
        busyBar.Margin = New Padding(4, 5, 4, 5)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(598, 5)
        busyBar.TabIndex = 2
        busyBar.TabStop = False
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.Size = New Size(598, 67)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        ' 
        ' LoginForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(600, 867)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "LoginForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Autentificare"
        pnlCard.ResumeLayout(False)
        tlpBody.ResumeLayout(False)
        tlpBody.PerformLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).EndInit()
        pnlUnit.ResumeLayout(False)
        pnlUnit.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Controls.KBotBusyBar
    Friend WithEvents tlpBody As TableLayoutPanel
    Friend WithEvents picLogo As PictureBox
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As KBot.Controls.KBotTextField
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As KBot.Controls.KBotTextField
    Friend WithEvents btnContinue As Button
    Friend WithEvents pnlUnit As TableLayoutPanel
    Friend WithEvents lblUnit As Label
    Friend WithEvents cboUnit As ComboBox
    Friend WithEvents btnBack As Button
    Friend WithEvents btnLogin As Button
    Friend WithEvents ntfError As KBot.Controls.KBotNotice
End Class
