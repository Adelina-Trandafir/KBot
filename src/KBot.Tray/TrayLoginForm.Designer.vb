' The tray's own sign-in window (slice TRAY-01): e-mail and password for the director who does not tick «Ține minte parola»
' in K-BOT. All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates at 96 dpi, AutoScaleMode.Dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TrayLoginForm
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

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        lblInfo = New Label()
        lblUser = New Label()
        txtUser = New TextBox()
        lblPass = New Label()
        txtPass = New TextBox()
        lblError = New Label()
        btnCancel = New Button()
        btnLogin = New Button()
        SuspendLayout()
        '
        ' lblInfo
        '
        lblInfo.Location = New Point(16, 14)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(348, 40)
        lblInfo.TabIndex = 0
        lblInfo.Text = "Autentificați-vă ca să pot verifica documentele de semnat. Parola rămâne doar cât rulează această iconiță."
        '
        ' lblUser
        '
        lblUser.Location = New Point(16, 62)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(348, 18)
        lblUser.TabIndex = 1
        lblUser.Text = "E-mail"
        '
        ' txtUser
        '
        txtUser.Location = New Point(16, 82)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(348, 23)
        txtUser.TabIndex = 2
        '
        ' lblPass
        '
        lblPass.Location = New Point(16, 112)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(348, 18)
        lblPass.TabIndex = 3
        lblPass.Text = "Parola"
        '
        ' txtPass
        '
        txtPass.Location = New Point(16, 132)
        txtPass.Name = "txtPass"
        txtPass.Size = New Size(348, 23)
        txtPass.TabIndex = 4
        txtPass.UseSystemPasswordChar = True
        '
        ' lblError
        '
        lblError.Location = New Point(16, 160)
        lblError.Name = "lblError"
        lblError.Size = New Size(348, 34)
        lblError.TabIndex = 5
        '
        ' btnCancel
        '
        btnCancel.Location = New Point(16, 200)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(110, 30)
        btnCancel.TabIndex = 7
        btnCancel.Text = "Anulează"
        btnCancel.UseVisualStyleBackColor = True
        '
        ' btnLogin
        '
        btnLogin.Location = New Point(254, 200)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(110, 30)
        btnLogin.TabIndex = 6
        btnLogin.Text = "Conectare"
        btnLogin.UseVisualStyleBackColor = True
        '
        ' TrayLoginForm
        '
        AcceptButton = btnLogin
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(380, 246)
        Controls.Add(lblInfo)
        Controls.Add(lblUser)
        Controls.Add(txtUser)
        Controls.Add(lblPass)
        Controls.Add(txtPass)
        Controls.Add(lblError)
        Controls.Add(btnCancel)
        Controls.Add(btnLogin)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "TrayLoginForm"
        ShowInTaskbar = True
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — documente de semnat"
        TopMost = True
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblInfo As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As TextBox
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As TextBox
    Friend WithEvents lblError As Label
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnLogin As Button
End Class
