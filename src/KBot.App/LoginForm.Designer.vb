<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LoginForm
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlCard As System.Windows.Forms.Panel

    Friend WithEvents lblError As System.Windows.Forms.Label

    Private Sub InitializeComponent()
        pnlCard = New Panel()
        pnlCreds = New TableLayoutPanel()
        lblUser = New Label()
        txtUser = New TextBox()
        lblPass = New Label()
        txtPass = New TextBox()
        btnContinue = New Button()
        pnlUnit = New TableLayoutPanel()
        lblUnit = New Label()
        cboUnit = New ComboBox()
        btnLogin = New Button()
        btnBack = New Button()
        pnlCaption = New TableLayoutPanel()
        lblSubtitle = New Label()
        lblTitle = New Label()
        lblError = New Label()
        pbBusy = New ProgressBar()
        pnlCard.SuspendLayout()
        pnlCreds.SuspendLayout()
        pnlUnit.SuspendLayout()
        pnlCaption.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(pnlCreds)
        pnlCard.Controls.Add(pnlCaption)
        pnlCard.Controls.Add(lblError)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 0)
        pnlCard.Margin = New Padding(4, 5, 4, 5)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(10)
        pnlCard.Size = New Size(561, 432)
        pnlCard.TabIndex = 0
        ' 
        ' pnlCreds
        ' 
        pnlCreds.ColumnCount = 2
        pnlCreds.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 46.21072F))
        pnlCreds.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 53.78928F))
        pnlCreds.Controls.Add(lblUser, 0, 0)
        pnlCreds.Controls.Add(txtUser, 1, 0)
        pnlCreds.Controls.Add(lblPass, 0, 1)
        pnlCreds.Controls.Add(txtPass, 1, 1)
        pnlCreds.Controls.Add(btnContinue, 0, 3)
        pnlCreds.Controls.Add(pnlUnit, 0, 5)
        pnlCreds.Dock = DockStyle.Fill
        pnlCreds.Location = New Point(10, 105)
        pnlCreds.Name = "pnlCreds"
        pnlCreds.RowCount = 6
        pnlCreds.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        pnlCreds.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        pnlCreds.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        pnlCreds.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        pnlCreds.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        pnlCreds.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        pnlCreds.Size = New Size(541, 317)
        pnlCreds.TabIndex = 7
        ' 
        ' lblUser
        ' 
        lblUser.Dock = DockStyle.Fill
        lblUser.Font = New Font("Segoe UI", 9F)
        lblUser.Location = New Point(4, 0)
        lblUser.Margin = New Padding(4, 0, 4, 0)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(242, 48)
        lblUser.TabIndex = 5
        lblUser.Text = "Utilizator"
        lblUser.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' txtUser
        ' 
        txtUser.BorderStyle = BorderStyle.FixedSingle
        txtUser.Font = New Font("Segoe UI", 11F)
        txtUser.Location = New Point(254, 5)
        txtUser.Margin = New Padding(4, 5, 4, 5)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(281, 37)
        txtUser.TabIndex = 6
        ' 
        ' lblPass
        ' 
        lblPass.Dock = DockStyle.Fill
        lblPass.Font = New Font("Segoe UI", 9F)
        lblPass.Location = New Point(4, 48)
        lblPass.Margin = New Padding(4, 0, 4, 0)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(242, 48)
        lblPass.TabIndex = 7
        lblPass.Text = "Parolă"
        lblPass.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' txtPass
        ' 
        txtPass.BorderStyle = BorderStyle.FixedSingle
        txtPass.Font = New Font("Segoe UI", 11F)
        txtPass.Location = New Point(254, 53)
        txtPass.Margin = New Padding(4, 5, 4, 5)
        txtPass.Name = "txtPass"
        txtPass.Size = New Size(281, 37)
        txtPass.TabIndex = 8
        txtPass.UseSystemPasswordChar = True
        ' 
        ' btnContinue
        ' 
        pnlCreds.SetColumnSpan(btnContinue, 2)
        btnContinue.FlatStyle = FlatStyle.Flat
        btnContinue.Font = New Font("Segoe UI Semibold", 10F)
        btnContinue.Location = New Point(4, 121)
        btnContinue.Margin = New Padding(4, 5, 4, 5)
        btnContinue.Name = "btnContinue"
        btnContinue.Size = New Size(533, 38)
        btnContinue.TabIndex = 9
        btnContinue.Text = "Continuă"
        ' 
        ' pnlUnit
        ' 
        pnlUnit.ColumnCount = 2
        pnlCreds.SetColumnSpan(pnlUnit, 2)
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 46.16822F))
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 53.8317757F))
        pnlUnit.Controls.Add(lblUnit, 0, 0)
        pnlUnit.Controls.Add(cboUnit, 1, 0)
        pnlUnit.Controls.Add(btnLogin, 1, 2)
        pnlUnit.Controls.Add(btnBack, 0, 2)
        pnlUnit.Dock = DockStyle.Fill
        pnlUnit.Location = New Point(3, 187)
        pnlUnit.Name = "pnlUnit"
        pnlUnit.RowCount = 4
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 193F))
        pnlUnit.Size = New Size(535, 127)
        pnlUnit.TabIndex = 10
        ' 
        ' lblUnit
        ' 
        lblUnit.Dock = DockStyle.Fill
        lblUnit.Font = New Font("Segoe UI", 9F)
        lblUnit.Location = New Point(4, 0)
        lblUnit.Margin = New Padding(4, 0, 4, 0)
        lblUnit.Name = "lblUnit"
        lblUnit.Size = New Size(238, 48)
        lblUnit.TabIndex = 4
        lblUnit.Text = "Selectați unitatea"
        lblUnit.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' cboUnit
        ' 
        cboUnit.Dock = DockStyle.Fill
        cboUnit.DropDownStyle = ComboBoxStyle.DropDownList
        cboUnit.FlatStyle = FlatStyle.Flat
        cboUnit.Font = New Font("Segoe UI", 11F)
        cboUnit.Location = New Point(250, 5)
        cboUnit.Margin = New Padding(4, 5, 4, 5)
        cboUnit.Name = "cboUnit"
        cboUnit.Size = New Size(281, 38)
        cboUnit.TabIndex = 5
        ' 
        ' btnLogin
        ' 
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.Font = New Font("Segoe UI Semibold", 10F)
        btnLogin.Location = New Point(250, 73)
        btnLogin.Margin = New Padding(4, 5, 4, 5)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(281, 38)
        btnLogin.TabIndex = 6
        btnLogin.Text = "Autentificare"
        ' 
        ' btnBack
        ' 
        btnBack.FlatStyle = FlatStyle.Flat
        btnBack.Font = New Font("Segoe UI", 9F)
        btnBack.Location = New Point(4, 73)
        btnBack.Margin = New Padding(4, 5, 4, 5)
        btnBack.Name = "btnBack"
        btnBack.Size = New Size(238, 38)
        btnBack.TabIndex = 7
        btnBack.Text = "Înapoi"
        ' 
        ' pnlCaption
        ' 
        pnlCaption.ColumnCount = 1
        pnlCaption.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        pnlCaption.Controls.Add(lblSubtitle, 0, 1)
        pnlCaption.Controls.Add(lblTitle, 0, 0)
        pnlCaption.Dock = DockStyle.Top
        pnlCaption.Location = New Point(10, 10)
        pnlCaption.Name = "pnlCaption"
        pnlCaption.RowCount = 2
        pnlCaption.RowStyles.Add(New RowStyle(SizeType.Percent, 56.8421059F))
        pnlCaption.RowStyles.Add(New RowStyle(SizeType.Percent, 43.1578941F))
        pnlCaption.Size = New Size(541, 95)
        pnlCaption.TabIndex = 6
        ' 
        ' lblSubtitle
        ' 
        lblSubtitle.Dock = DockStyle.Fill
        lblSubtitle.Font = New Font("Segoe UI", 10F)
        lblSubtitle.Location = New Point(4, 54)
        lblSubtitle.Margin = New Padding(4, 0, 4, 0)
        lblSubtitle.Name = "lblSubtitle"
        lblSubtitle.Size = New Size(533, 41)
        lblSubtitle.TabIndex = 2
        lblSubtitle.Text = "Autentificare operator"
        lblSubtitle.TextAlign = ContentAlignment.TopCenter
        ' 
        ' lblTitle
        ' 
        lblTitle.Dock = DockStyle.Fill
        lblTitle.FlatStyle = FlatStyle.Popup
        lblTitle.Font = New Font("Calibri", 20F, FontStyle.Bold)
        lblTitle.Location = New Point(4, 0)
        lblTitle.Margin = New Padding(4, 0, 4, 0)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(533, 54)
        lblTitle.TabIndex = 1
        lblTitle.Text = "K-BOT - UN ROBOT PRIETENOS"
        lblTitle.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblError
        ' 
        lblError.Font = New Font("Segoe UI", 9F)
        lblError.Location = New Point(43, 583)
        lblError.Margin = New Padding(4, 0, 4, 0)
        lblError.Name = "lblError"
        lblError.Size = New Size(514, 57)
        lblError.TabIndex = 4
        lblError.TextAlign = ContentAlignment.MiddleLeft
        lblError.Visible = False
        ' 
        ' pbBusy
        ' 
        pbBusy.Dock = DockStyle.Bottom
        pbBusy.Location = New Point(0, 422)
        pbBusy.Margin = New Padding(4, 5, 4, 5)
        pbBusy.MarqueeAnimationSpeed = 30
        pbBusy.Name = "pbBusy"
        pbBusy.Size = New Size(561, 10)
        pbBusy.Style = ProgressBarStyle.Marquee
        pbBusy.TabIndex = 9
        pbBusy.Visible = False
        ' 
        ' LoginForm
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(561, 432)
        Controls.Add(pbBusy)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.FixedDialog
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "LoginForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Autentificare"
        pnlCard.ResumeLayout(False)
        pnlCreds.ResumeLayout(False)
        pnlCreds.PerformLayout()
        pnlUnit.ResumeLayout(False)
        pnlCaption.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCaption As TableLayoutPanel
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents lblTitle As Label
    Friend WithEvents pnlCreds As TableLayoutPanel
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As TextBox
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As TextBox
    Friend WithEvents btnContinue As Button
    Friend WithEvents pnlUnit As TableLayoutPanel
    Friend WithEvents lblUnit As Label
    Friend WithEvents cboUnit As ComboBox
    Friend WithEvents btnLogin As Button
    Friend WithEvents btnBack As Button
    Friend WithEvents pbBusy As ProgressBar
End Class
