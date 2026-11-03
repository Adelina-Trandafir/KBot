' The connection dialog (slice ME01). All controls are declared HERE (docs/kbot-forms-ui-convention.md);
' coordinates are in the 96 dpi the form was authored at and AutoScaleDimensions carries the same stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConnectionForm
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
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        lblHost = New Label()
        txtHost = New KBotTextField()
        lblPort = New Label()
        txtPort = New KBotTextField()
        lblUser = New Label()
        txtUser = New KBotTextField()
        lblKey = New Label()
        txtKey = New KBotTextField()
        lblPass = New Label()
        txtPass = New KBotTextField()
        lblInfo = New Label()
        busy = New KBotBusyBar()
        tlyButtons = New KBotTableLayoutPanel()
        btnCancel = New KBotButton()
        btnConnect = New KBotButton()
        tlyMain.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 2
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblHost, 0, 0)
        tlyMain.Controls.Add(txtHost, 1, 0)
        tlyMain.Controls.Add(lblPort, 0, 1)
        tlyMain.Controls.Add(txtPort, 1, 1)
        tlyMain.Controls.Add(lblUser, 0, 2)
        tlyMain.Controls.Add(txtUser, 1, 2)
        tlyMain.Controls.Add(lblKey, 0, 3)
        tlyMain.Controls.Add(txtKey, 1, 3)
        tlyMain.Controls.Add(lblPass, 0, 4)
        tlyMain.Controls.Add(txtPass, 1, 4)
        tlyMain.Controls.Add(lblInfo, 0, 5)
        tlyMain.Controls.Add(busy, 0, 6)
        tlyMain.Controls.Add(tlyButtons, 0, 7)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 16, 16, 12)
        tlyMain.RowCount = 8
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 8F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.Size = New Size(520, 360)
        tlyMain.TabIndex = 0
        '
        ' lblHost
        '
        tips.SetToolTipHeader(lblHost, "Server")
        tips.SetToolTipText(lblHost, "Adresa serverului unde rulează K-BOT (SSH).")
        lblHost.Dock = DockStyle.Fill
        lblHost.Location = New Point(16, 16)
        lblHost.Margin = New Padding(0)
        lblHost.Name = "lblHost"
        lblHost.Size = New Size(130, 40)
        lblHost.TabIndex = 0
        lblHost.Text = "Server:"
        lblHost.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtHost
        '
        txtHost.BackColor = Color.Transparent
        txtHost.Dock = DockStyle.Fill
        txtHost.Location = New Point(146, 16)
        txtHost.Margin = New Padding(0, 3, 0, 3)
        txtHost.Name = "txtHost"
        txtHost.Size = New Size(358, 34)
        txtHost.TabIndex = 1
        txtHost.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtHost, "Server")
        tips.SetToolTipText(txtHost, "Adresa serverului unde rulează K-BOT (SSH).")
        '
        ' lblPort
        '
        tips.SetToolTipHeader(lblPort, "Port SSH")
        tips.SetToolTipText(lblPort, "Portul SSH al serverului. Implicit 22.")
        lblPort.Dock = DockStyle.Fill
        lblPort.Location = New Point(16, 56)
        lblPort.Margin = New Padding(0)
        lblPort.Name = "lblPort"
        lblPort.Size = New Size(130, 40)
        lblPort.TabIndex = 2
        lblPort.Text = "Port SSH:"
        lblPort.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtPort
        '
        tips.SetToolTipHeader(txtPort, "Port SSH")
        tips.SetToolTipText(txtPort, "Portul SSH al serverului. Implicit 22.")
        txtPort.BackColor = Color.Transparent
        txtPort.Dock = DockStyle.Fill
        txtPort.Location = New Point(146, 56)
        txtPort.Margin = New Padding(0, 3, 0, 3)
        txtPort.Name = "txtPort"
        txtPort.Size = New Size(358, 34)
        txtPort.TabIndex = 3
        txtPort.TextPadding = New Padding(8, 0, 8, 0)
        '
        ' lblUser
        '
        tips.SetToolTipHeader(lblUser, "Utilizator")
        tips.SetToolTipText(lblUser, "Contul de pe server cu care se face conexiunea SSH.")
        lblUser.Dock = DockStyle.Fill
        lblUser.Location = New Point(16, 96)
        lblUser.Margin = New Padding(0)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(130, 40)
        lblUser.TabIndex = 4
        lblUser.Text = "Utilizator:"
        lblUser.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtUser
        '
        tips.SetToolTipHeader(txtUser, "Utilizator")
        tips.SetToolTipText(txtUser, "Contul de pe server cu care se face conexiunea SSH (de obicei root).")
        txtUser.BackColor = Color.Transparent
        txtUser.Dock = DockStyle.Fill
        txtUser.Location = New Point(146, 96)
        txtUser.Margin = New Padding(0, 3, 0, 3)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(358, 34)
        txtUser.TabIndex = 5
        txtUser.TextPadding = New Padding(8, 0, 8, 0)
        '
        ' lblKey
        '
        tips.SetToolTipHeader(lblKey, "Cheie SSH")
        tips.SetToolTipText(lblKey, "Calea fișierului cu cheia privată. Gol = se caută automat în ~/.ssh.")
        lblKey.Dock = DockStyle.Fill
        lblKey.Location = New Point(16, 136)
        lblKey.Margin = New Padding(0)
        lblKey.Name = "lblKey"
        lblKey.Size = New Size(130, 40)
        lblKey.TabIndex = 6
        lblKey.Text = "Cheie SSH:"
        lblKey.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtKey
        '
        txtKey.BackColor = Color.Transparent
        txtKey.Dock = DockStyle.Fill
        txtKey.Location = New Point(146, 136)
        txtKey.Margin = New Padding(0, 3, 0, 3)
        txtKey.Name = "txtKey"
        txtKey.PlaceholderText = "gol = cheia din ~/.ssh"
        txtKey.Size = New Size(358, 34)
        txtKey.TabIndex = 7
        txtKey.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtKey, "Cheie SSH")
        tips.SetToolTipText(txtKey, "Calea fișierului cheii private." & vbLf & "Gol: se caută în ~/.ssh/config, apoi id_ed25519, id_ecdsa, id_rsa.")
        '
        ' lblPass
        '
        tips.SetToolTipHeader(lblPass, "Parolă")
        tips.SetToolTipText(lblPass, "Parola cheii (dacă are) sau parola contului. Nu se salvează niciodată.")
        lblPass.Dock = DockStyle.Fill
        lblPass.Location = New Point(16, 176)
        lblPass.Margin = New Padding(0)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(130, 40)
        lblPass.TabIndex = 8
        lblPass.Text = "Parolă:"
        lblPass.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtPass
        '
        txtPass.BackColor = Color.Transparent
        txtPass.Dock = DockStyle.Fill
        txtPass.Location = New Point(146, 176)
        txtPass.Margin = New Padding(0, 3, 0, 3)
        txtPass.Name = "txtPass"
        txtPass.PlaceholderText = "parola cheii sau a contului (nu se salvează)"
        txtPass.Size = New Size(358, 34)
        txtPass.TabIndex = 9
        txtPass.TextPadding = New Padding(8, 0, 8, 0)
        txtPass.UseSystemPasswordChar = True
        tips.SetToolTipHeader(txtPass, "Parolă")
        tips.SetToolTipText(txtPass, "Parola cheii SSH (dacă are) sau parola contului." & vbLf & "Nu se salvează nicăieri; se tastează la fiecare pornire.")
        '
        ' lblInfo
        '
        tips.SetToolTipHeader(lblInfo, "Conexiune")
        tips.SetToolTipText(lblInfo, "Singura legătură cu serverul este SSH. Nu se deschide niciun port de baze de date.")
        tlyMain.SetColumnSpan(lblInfo, 2)
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(16, 216)
        lblInfo.Margin = New Padding(0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(488, 60)
        lblInfo.TabIndex = 10
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' busy
        '
        tips.SetToolTipHeader(busy, "Se conectează")
        tips.SetToolTipText(busy, "Bara se mișcă cât timp se stabilește conexiunea.")
        tlyMain.SetColumnSpan(busy, 2)
        busy.Dock = DockStyle.Fill
        busy.Location = New Point(16, 276)
        busy.Margin = New Padding(0, 2, 0, 2)
        busy.Name = "busy"
        busy.Size = New Size(488, 4)
        busy.TabIndex = 11
        busy.TabStop = False
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyMain.SetColumnSpan(tlyButtons, 2)
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyButtons.Controls.Add(btnCancel, 0, 0)
        tlyButtons.Controls.Add(btnConnect, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 284)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(488, 44)
        tlyButtons.TabIndex = 12
        '
        ' btnCancel
        '
        tips.SetToolTipHeader(btnCancel, "Renunță")
        tips.SetToolTipText(btnCancel, "Închide programul fără să se conecteze.")
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Fill
        btnCancel.Location = New Point(0, 0)
        btnCancel.Margin = New Padding(0, 2, 6, 2)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(114, 40)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Renunță"
        '
        ' btnConnect
        '
        btnConnect.Dock = DockStyle.Fill
        btnConnect.Location = New Point(348, 0)
        btnConnect.Margin = New Padding(6, 2, 0, 2)
        btnConnect.Name = "btnConnect"
        btnConnect.Primary = True
        btnConnect.Size = New Size(134, 40)
        btnConnect.TabIndex = 1
        btnConnect.Text = "Conectează"
        tips.SetToolTipHeader(btnConnect, "Conectează")
        tips.SetToolTipText(btnConnect, "Se conectează la server prin SSH. Nicio bază de date nu se deschide direct.")
        '
        ' ConnectionForm
        '
        AcceptButton = btnConnect
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(520, 360)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "ConnectionForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "KBotAdmin - conectare"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblHost As Label
    Friend WithEvents txtHost As KBotTextField
    Friend WithEvents lblPort As Label
    Friend WithEvents txtPort As KBotTextField
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As KBotTextField
    Friend WithEvents lblKey As Label
    Friend WithEvents txtKey As KBotTextField
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As KBotTextField
    Friend WithEvents lblInfo As Label
    Friend WithEvents busy As KBotBusyBar
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCancel As KBotButton
    Friend WithEvents btnConnect As KBotButton
End Class
