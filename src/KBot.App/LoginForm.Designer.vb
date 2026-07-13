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
        pnlCard = New Panel()
        capBar = New KBot.Theming.KBotCaptionBar()
        busyBar = New KBot.Theming.KBotBusyBar()
        tlpBody = New TableLayoutPanel()
        picLogo = New PictureBox()
        lblTitle = New Label()
        lblSubtitle = New Label()
        lblUser = New Label()
        txtUser = New KBot.Theming.KBotTextField()
        lblPass = New Label()
        txtPass = New KBot.Theming.KBotTextField()
        btnContinue = New Button()
        pnlUnit = New TableLayoutPanel()
        lblUnit = New Label()
        cboUnit = New ComboBox()
        btnBack = New Button()
        btnLogin = New Button()
        ntfError = New KBot.Theming.KBotNotice()
        pnlCard.SuspendLayout()
        tlpBody.SuspendLayout()
        pnlUnit.SuspendLayout()
        CType(picLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlpBody)
        pnlCard.Controls.Add(busyBar)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(418, 518)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.Size = New Size(418, 40)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "K-BOT"
        '
        ' busyBar
        '
        busyBar.Dock = DockStyle.Top
        busyBar.Location = New Point(0, 40)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(418, 3)
        busyBar.TabIndex = 2
        busyBar.TabStop = False
        '
        ' tlpBody
        '
        tlpBody.ColumnCount = 1
        tlpBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
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
        tlpBody.Location = New Point(0, 43)
        tlpBody.Name = "tlpBody"
        tlpBody.Padding = New Padding(28, 8, 28, 10)
        tlpBody.RowCount = 11
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        tlpBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpBody.Size = New Size(418, 475)
        tlpBody.TabIndex = 0
        tlpBody.Tag = "Card"
        '
        ' picLogo
        '
        picLogo.Anchor = AnchorStyles.None
        picLogo.Location = New Point(177, 6)
        picLogo.Margin = New Padding(3, 6, 3, 6)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(64, 64)
        picLogo.SizeMode = PictureBoxSizeMode.Zoom
        picLogo.TabIndex = 0
        picLogo.TabStop = False
        '
        ' lblTitle
        '
        lblTitle.AutoSize = True
        lblTitle.Dock = DockStyle.Top
        lblTitle.Font = New Font("Segoe UI", 18.0F, FontStyle.Bold)
        lblTitle.Location = New Point(31, 76)
        lblTitle.Margin = New Padding(3, 0, 3, 2)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(356, 32)
        lblTitle.TabIndex = 1
        lblTitle.Text = "K-BOT"
        lblTitle.TextAlign = ContentAlignment.MiddleCenter
        '
        ' lblSubtitle
        '
        lblSubtitle.AutoSize = True
        lblSubtitle.Dock = DockStyle.Top
        lblSubtitle.Font = New Font("Segoe UI", 10.0F)
        lblSubtitle.Location = New Point(31, 110)
        lblSubtitle.Margin = New Padding(3, 0, 3, 12)
        lblSubtitle.Name = "lblSubtitle"
        lblSubtitle.Size = New Size(356, 19)
        lblSubtitle.TabIndex = 2
        lblSubtitle.Text = "Autentificare operator"
        lblSubtitle.TextAlign = ContentAlignment.MiddleCenter
        '
        ' lblUser
        '
        lblUser.AutoSize = True
        lblUser.Dock = DockStyle.Top
        lblUser.Location = New Point(31, 141)
        lblUser.Margin = New Padding(3, 0, 3, 3)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(356, 15)
        lblUser.TabIndex = 3
        lblUser.Text = "Utilizator"
        '
        ' txtUser
        '
        txtUser.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtUser.Location = New Point(31, 159)
        txtUser.Margin = New Padding(3, 0, 3, 10)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(356, 36)
        txtUser.TabIndex = 4
        '
        ' lblPass
        '
        lblPass.AutoSize = True
        lblPass.Dock = DockStyle.Top
        lblPass.Location = New Point(31, 205)
        lblPass.Margin = New Padding(3, 0, 3, 3)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(356, 15)
        lblPass.TabIndex = 5
        lblPass.Text = "Parolă"
        '
        ' txtPass
        '
        txtPass.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtPass.Location = New Point(31, 223)
        txtPass.Margin = New Padding(3, 0, 3, 14)
        txtPass.Name = "txtPass"
        txtPass.Size = New Size(356, 36)
        txtPass.TabIndex = 6
        txtPass.UseSystemPasswordChar = True
        '
        ' btnContinue
        '
        btnContinue.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnContinue.FlatStyle = FlatStyle.Flat
        btnContinue.Font = New Font("Segoe UI Semibold", 10.0F)
        btnContinue.Location = New Point(31, 273)
        btnContinue.Margin = New Padding(3, 0, 3, 6)
        btnContinue.Name = "btnContinue"
        btnContinue.Size = New Size(356, 40)
        btnContinue.TabIndex = 7
        btnContinue.Text = "Continuă"
        btnContinue.UseVisualStyleBackColor = True
        '
        ' pnlUnit
        '
        pnlUnit.AutoSize = True
        pnlUnit.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlUnit.ColumnCount = 2
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        pnlUnit.Controls.Add(lblUnit, 0, 0)
        pnlUnit.Controls.Add(cboUnit, 0, 1)
        pnlUnit.Controls.Add(btnBack, 0, 2)
        pnlUnit.Controls.Add(btnLogin, 1, 2)
        pnlUnit.SetColumnSpan(lblUnit, 2)
        pnlUnit.SetColumnSpan(cboUnit, 2)
        pnlUnit.Dock = DockStyle.Top
        pnlUnit.Location = New Point(28, 319)
        pnlUnit.Margin = New Padding(0, 6, 0, 0)
        pnlUnit.Name = "pnlUnit"
        pnlUnit.RowCount = 3
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        pnlUnit.Size = New Size(362, 100)
        pnlUnit.TabIndex = 8
        pnlUnit.Tag = "Card"
        pnlUnit.Visible = False
        '
        ' lblUnit
        '
        lblUnit.AutoSize = True
        lblUnit.Dock = DockStyle.Top
        lblUnit.Location = New Point(3, 0)
        lblUnit.Margin = New Padding(3, 0, 3, 3)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(356, 15)
        lblUnit.TabIndex = 0
        lblUnit.Text = "Selectați unitatea"
        '
        ' cboUnit
        '
        cboUnit.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cboUnit.DropDownStyle = ComboBoxStyle.DropDownList
        cboUnit.FlatStyle = FlatStyle.Flat
        cboUnit.Font = New Font("Segoe UI", 10.0F)
        cboUnit.Location = New Point(3, 21)
        cboUnit.Margin = New Padding(3, 3, 3, 12)
        cboUnit.Name = "cboUnit"
        cboUnit.Size = New Size(356, 25)
        cboUnit.TabIndex = 1
        '
        ' btnBack
        '
        btnBack.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnBack.FlatStyle = FlatStyle.Flat
        btnBack.Font = New Font("Segoe UI", 9.0F)
        btnBack.Location = New Point(3, 61)
        btnBack.Margin = New Padding(3, 0, 6, 0)
        btnBack.Name = "btnBack"
        btnBack.Size = New Size(172, 38)
        btnBack.TabIndex = 3
        btnBack.Text = "Înapoi"
        btnBack.UseVisualStyleBackColor = True
        '
        ' btnLogin
        '
        btnLogin.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.Font = New Font("Segoe UI Semibold", 10.0F)
        btnLogin.Location = New Point(187, 61)
        btnLogin.Margin = New Padding(6, 0, 3, 0)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(172, 38)
        btnLogin.TabIndex = 2
        btnLogin.Text = "Autentificare"
        btnLogin.UseVisualStyleBackColor = True
        '
        ' ntfError
        '
        ntfError.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        ntfError.Location = New Point(31, 425)
        ntfError.Margin = New Padding(3, 6, 3, 3)
        ntfError.Name = "ntfError"
        ntfError.Size = New Size(356, 40)
        ntfError.TabIndex = 9
        ntfError.TabStop = False
        ntfError.Visible = False
        '
        ' LoginForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(420, 520)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "LoginForm"
        Padding = New Padding(1)
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Autentificare"
        pnlCard.ResumeLayout(False)
        tlpBody.ResumeLayout(False)
        tlpBody.PerformLayout()
        pnlUnit.ResumeLayout(False)
        pnlUnit.PerformLayout()
        CType(picLogo, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Theming.KBotCaptionBar
    Friend WithEvents busyBar As KBot.Theming.KBotBusyBar
    Friend WithEvents tlpBody As TableLayoutPanel
    Friend WithEvents picLogo As PictureBox
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As KBot.Theming.KBotTextField
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As KBot.Theming.KBotTextField
    Friend WithEvents btnContinue As Button
    Friend WithEvents pnlUnit As TableLayoutPanel
    Friend WithEvents lblUnit As Label
    Friend WithEvents cboUnit As ComboBox
    Friend WithEvents btnBack As Button
    Friend WithEvents btnLogin As Button
    Friend WithEvents ntfError As KBot.Theming.KBotNotice
End Class
