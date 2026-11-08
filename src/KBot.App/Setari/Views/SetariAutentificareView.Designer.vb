Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariAutentificareView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        tlyBody = New KBotTableLayoutPanel()
        lblTitluMemorie = New Label()
        tlyMemorie = New KBotTableLayoutPanel()
        chkRememberLogin = New CheckBox()
        chkRememberUnit = New CheckBox()
        lblUtilizatorCaption = New Label()
        lblUtilizator = New Label()
        lblUnitateCaption = New Label()
        lblUnitate = New Label()
        btnUita = New Button()
        lblTitluLucru = New Label()
        tlyLucru = New KBotTableLayoutPanel()
        chkRelogin = New CheckBox()
        lblIntervalCaption = New Label()
        cmbInterval = New KBotComboBox()
        lblReloginHint = New Label()
        chkRememberPasswordOption = New CheckBox()
        btnUitaParola = New Button()
        lblTitluServer = New Label()
        tlyServer = New KBotTableLayoutPanel()
        lblServerCaption = New Label()
        lblServer = New Label()
        lblTimeoutCaption = New Label()
        lblTimeout = New Label()
        lblServerHint = New Label()
        tlyBody.SuspendLayout()
        tlyMemorie.SuspendLayout()
        tlyLucru.SuspendLayout()
        tlyServer.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.AutoScroll = True
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitluMemorie, 0, 0)
        tlyBody.Controls.Add(tlyMemorie, 0, 1)
        tlyBody.Controls.Add(lblTitluLucru, 0, 2)
        tlyBody.Controls.Add(tlyLucru, 0, 3)
        tlyBody.Controls.Add(lblTitluServer, 0, 4)
        tlyBody.Controls.Add(tlyServer, 0, 5)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 7
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(960, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitluMemorie
        '
        lblTitluMemorie.AutoSize = True
        lblTitluMemorie.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluMemorie.Location = New Point(28, 18)
        lblTitluMemorie.Margin = New Padding(4, 0, 4, 8)
        lblTitluMemorie.Name = "lblTitluMemorie"
        lblTitluMemorie.Size = New Size(280, 32)
        lblTitluMemorie.TabIndex = 0
        lblTitluMemorie.Text = "Fereastra de autentificare"
        '
        ' tlyMemorie
        '
        tlyMemorie.AutoFitToTheme = False
        tlyMemorie.AutoSize = True
        tlyMemorie.ColumnCount = 3
        tlyMemorie.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyMemorie.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMemorie.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyMemorie.Controls.Add(chkRememberLogin, 0, 0)
        tlyMemorie.Controls.Add(chkRememberUnit, 0, 1)
        tlyMemorie.Controls.Add(lblUtilizatorCaption, 0, 2)
        tlyMemorie.Controls.Add(lblUtilizator, 1, 2)
        tlyMemorie.Controls.Add(lblUnitateCaption, 0, 3)
        tlyMemorie.Controls.Add(lblUnitate, 1, 3)
        tlyMemorie.Controls.Add(btnUita, 2, 3)
        tlyMemorie.Dock = DockStyle.Top
        tlyMemorie.Location = New Point(28, 58)
        tlyMemorie.Margin = New Padding(4, 0, 4, 24)
        tlyMemorie.Name = "tlyMemorie"
        tlyMemorie.RowCount = 4
        tlyMemorie.RowStyles.Add(New RowStyle())
        tlyMemorie.RowStyles.Add(New RowStyle())
        tlyMemorie.RowStyles.Add(New RowStyle())
        tlyMemorie.RowStyles.Add(New RowStyle())
        tlyMemorie.Size = New Size(904, 180)
        tlyMemorie.TabIndex = 1
        '
        ' chkRememberLogin
        '
        chkRememberLogin.AutoSize = True
        chkRememberLogin.Checked = True
        chkRememberLogin.CheckState = CheckState.Checked
        tlyMemorie.SetColumnSpan(chkRememberLogin, 3)
        chkRememberLogin.Location = New Point(4, 0)
        chkRememberLogin.Margin = New Padding(4, 0, 4, 10)
        chkRememberLogin.Name = "chkRememberLogin"
        chkRememberLogin.Size = New Size(420, 29)
        chkRememberLogin.TabIndex = 0
        chkRememberLogin.Text = "Ține minte ultimul utilizator care s-a autentificat (e-mailul se completează singur)"
        tips.SetToolTipHeader(chkRememberLogin, "Utilizatorul memorat")
        tips.SetToolTipText(chkRememberLogin, "Doar e-mailul și unitatea, într-un fișier din AppData." & vbLf & "Parola nu se păstrează niciodată pe acest calculator.")
        chkRememberLogin.UseVisualStyleBackColor = True
        '
        ' chkRememberUnit
        '
        chkRememberUnit.AutoSize = True
        chkRememberUnit.Checked = True
        chkRememberUnit.CheckState = CheckState.Checked
        tlyMemorie.SetColumnSpan(chkRememberUnit, 3)
        chkRememberUnit.Location = New Point(4, 39)
        chkRememberUnit.Margin = New Padding(4, 0, 4, 16)
        chkRememberUnit.Name = "chkRememberUnit"
        chkRememberUnit.Size = New Size(420, 29)
        chkRememberUnit.TabIndex = 1
        chkRememberUnit.Text = "Ține minte și unitatea aleasă ultima dată (se preselectează la pasul al doilea)"
        tips.SetToolTipHeader(chkRememberUnit, "Unitatea memorată")
        tips.SetToolTipText(chkRememberUnit, "Are efect doar când și utilizatorul e memorat." & vbLf & "Alt utilizator sau o unitate dispărută din listă -> prima din listă, ca înainte.")
        chkRememberUnit.UseVisualStyleBackColor = True
        '
        ' lblUtilizatorCaption
        '
        lblUtilizatorCaption.AutoSize = True
        lblUtilizatorCaption.Dock = DockStyle.Fill
        lblUtilizatorCaption.Location = New Point(4, 84)
        lblUtilizatorCaption.Margin = New Padding(4, 0, 4, 6)
        lblUtilizatorCaption.Name = "lblUtilizatorCaption"
        lblUtilizatorCaption.Size = New Size(252, 26)
        lblUtilizatorCaption.TabIndex = 2
        lblUtilizatorCaption.Text = "Utilizator memorat"
        '
        ' lblUtilizator
        '
        lblUtilizator.AutoSize = True
        lblUtilizator.Dock = DockStyle.Fill
        lblUtilizator.Font = New Font("Segoe UI Semibold", 9F)
        lblUtilizator.Location = New Point(264, 84)
        lblUtilizator.Margin = New Padding(4, 0, 4, 6)
        lblUtilizator.Name = "lblUtilizator"
        lblUtilizator.Size = New Size(388, 26)
        lblUtilizator.TabIndex = 3
        lblUtilizator.Text = "—"
        '
        ' lblUnitateCaption
        '
        lblUnitateCaption.AutoSize = True
        lblUnitateCaption.Dock = DockStyle.Fill
        lblUnitateCaption.Location = New Point(4, 116)
        lblUnitateCaption.Margin = New Padding(4, 0, 4, 0)
        lblUnitateCaption.Name = "lblUnitateCaption"
        lblUnitateCaption.Size = New Size(252, 50)
        lblUnitateCaption.TabIndex = 4
        lblUnitateCaption.Text = "Unitate memorată (DC)"
        lblUnitateCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblUnitate
        '
        lblUnitate.AutoSize = True
        lblUnitate.Dock = DockStyle.Fill
        lblUnitate.Font = New Font("Segoe UI Semibold", 9F)
        lblUnitate.Location = New Point(264, 116)
        lblUnitate.Margin = New Padding(4, 0, 4, 0)
        lblUnitate.Name = "lblUnitate"
        lblUnitate.Size = New Size(388, 50)
        lblUnitate.TabIndex = 5
        lblUnitate.Text = "—"
        lblUnitate.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnUita
        '
        btnUita.Dock = DockStyle.Fill
        btnUita.Enabled = False
        btnUita.FlatStyle = FlatStyle.Flat
        btnUita.Font = New Font("Segoe UI Semibold", 9F)
        btnUita.Location = New Point(664, 116)
        btnUita.Margin = New Padding(4, 0, 4, 0)
        btnUita.Name = "btnUita"
        btnUita.Size = New Size(236, 50)
        btnUita.TabIndex = 6
        btnUita.Text = "Uită datele memorate"
        tips.SetToolTipHeader(btnUita, "Uită datele memorate")
        tips.SetToolTipText(btnUita, "Șterge fișierul last_login.json din AppData." & vbLf & "La următoarea pornire, e-mailul se tastează din nou.")
        btnUita.UseVisualStyleBackColor = True
        '
        ' lblTitluLucru
        '
        lblTitluLucru.AutoSize = True
        lblTitluLucru.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluLucru.Location = New Point(28, 262)
        lblTitluLucru.Margin = New Padding(4, 0, 4, 8)
        lblTitluLucru.Name = "lblTitluLucru"
        lblTitluLucru.Size = New Size(380, 32)
        lblTitluLucru.TabIndex = 2
        lblTitluLucru.Text = "În timpul lucrului (sesiunea expirată)"
        '
        ' tlyLucru
        '
        tlyLucru.AutoFitToTheme = False
        tlyLucru.AutoSize = True
        tlyLucru.ColumnCount = 3
        tlyLucru.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyLucru.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyLucru.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyLucru.Controls.Add(chkRelogin, 0, 0)
        tlyLucru.Controls.Add(lblIntervalCaption, 0, 1)
        tlyLucru.Controls.Add(cmbInterval, 1, 1)
        tlyLucru.Controls.Add(lblReloginHint, 0, 2)
        tlyLucru.Controls.Add(chkRememberPasswordOption, 0, 3)
        tlyLucru.Controls.Add(btnUitaParola, 2, 4)
        tlyLucru.Dock = DockStyle.Top
        tlyLucru.Location = New Point(28, 302)
        tlyLucru.Margin = New Padding(4, 0, 4, 24)
        tlyLucru.Name = "tlyLucru"
        tlyLucru.RowCount = 5
        tlyLucru.RowStyles.Add(New RowStyle())
        tlyLucru.RowStyles.Add(New RowStyle())
        tlyLucru.RowStyles.Add(New RowStyle())
        tlyLucru.RowStyles.Add(New RowStyle())
        tlyLucru.RowStyles.Add(New RowStyle())
        tlyLucru.Size = New Size(904, 230)
        tlyLucru.TabIndex = 3
        '
        ' chkRelogin
        '
        chkRelogin.AutoSize = True
        chkRelogin.Checked = True
        chkRelogin.CheckState = CheckState.Checked
        tlyLucru.SetColumnSpan(chkRelogin, 3)
        chkRelogin.Location = New Point(4, 0)
        chkRelogin.Margin = New Padding(4, 0, 4, 10)
        chkRelogin.Name = "chkRelogin"
        chkRelogin.Size = New Size(420, 29)
        chkRelogin.TabIndex = 0
        chkRelogin.Text = "Reafișează fereastra de autentificare când expiră sesiunea"
        tips.SetToolTipHeader(chkRelogin, "Fereastra de autentificare")
        tips.SetToolTipText(chkRelogin, "Debifată, K-BOT se reautentifică singur, cu parola introdusă la intrare." & vbLf & "Se poate schimba doar cu opțiunile avansate activate.")
        chkRelogin.UseVisualStyleBackColor = True
        '
        ' lblIntervalCaption
        '
        lblIntervalCaption.AutoSize = True
        lblIntervalCaption.Dock = DockStyle.Fill
        lblIntervalCaption.Location = New Point(4, 39)
        lblIntervalCaption.Margin = New Padding(4, 0, 4, 6)
        lblIntervalCaption.Name = "lblIntervalCaption"
        lblIntervalCaption.Size = New Size(252, 42)
        lblIntervalCaption.TabIndex = 1
        lblIntervalCaption.Text = "Reafișează cel mult o dată la"
        lblIntervalCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbInterval
        '
        cmbInterval.Anchor = AnchorStyles.Left
        cmbInterval.Location = New Point(264, 43)
        cmbInterval.Margin = New Padding(4, 4, 4, 4)
        cmbInterval.Name = "cmbInterval"
        cmbInterval.Size = New Size(300, 34)
        cmbInterval.TabIndex = 2
        tips.SetToolTipHeader(cmbInterval, "Cât de des")
        tips.SetToolTipText(cmbInterval, "Dacă sesiunea expiră mai devreme de atât după ultima autentificare," & vbLf & "K-BOT se reautentifică singur, fără fereastră." & vbLf & "Între o dată la 10 minute și o dată pe oră.")
        '
        ' lblReloginHint
        '
        lblReloginHint.AutoSize = True
        tlyLucru.SetColumnSpan(lblReloginHint, 3)
        lblReloginHint.Dock = DockStyle.Fill
        lblReloginHint.Location = New Point(4, 91)
        lblReloginHint.Margin = New Padding(4, 6, 4, 16)
        lblReloginHint.Name = "lblReloginHint"
        lblReloginHint.Size = New Size(896, 26)
        lblReloginHint.TabIndex = 3
        lblReloginHint.Text = "La pornirea aplicației fereastra de autentificare apare întotdeauna."
        '
        ' chkRememberPasswordOption
        '
        chkRememberPasswordOption.AutoSize = True
        chkRememberPasswordOption.Checked = True
        chkRememberPasswordOption.CheckState = CheckState.Checked
        tlyLucru.SetColumnSpan(chkRememberPasswordOption, 3)
        chkRememberPasswordOption.Location = New Point(4, 133)
        chkRememberPasswordOption.Margin = New Padding(4, 0, 4, 10)
        chkRememberPasswordOption.Name = "chkRememberPasswordOption"
        chkRememberPasswordOption.Size = New Size(420, 29)
        chkRememberPasswordOption.TabIndex = 4
        chkRememberPasswordOption.Text = "Arată în fereastra de autentificare bifa «Ține minte parola până la repornirea calculatorului»"
        tips.SetToolTipHeader(chkRememberPasswordOption, "Ține minte parola")
        tips.SetToolTipText(chkRememberPasswordOption, "Parola bifată acolo se păstrează criptată pentru contul Windows curent," & vbLf & "până la repornirea calculatorului sau ieșirea din Windows.")
        chkRememberPasswordOption.UseVisualStyleBackColor = True
        '
        ' btnUitaParola
        '
        btnUitaParola.Dock = DockStyle.Fill
        btnUitaParola.Enabled = False
        btnUitaParola.FlatStyle = FlatStyle.Flat
        btnUitaParola.Font = New Font("Segoe UI Semibold", 9F)
        btnUitaParola.Location = New Point(664, 172)
        btnUitaParola.Margin = New Padding(4, 0, 4, 0)
        btnUitaParola.Name = "btnUitaParola"
        btnUitaParola.Size = New Size(236, 50)
        btnUitaParola.TabIndex = 5
        btnUitaParola.Text = "Uită parola memorată"
        tips.SetToolTipHeader(btnUitaParola, "Uită parola memorată")
        tips.SetToolTipText(btnUitaParola, "Șterge parola ținută minte pentru această sesiune Windows." & vbLf & "La următoarea pornire, parola se tastează din nou.")
        btnUitaParola.UseVisualStyleBackColor = True
        '
        ' lblTitluServer
        '
        lblTitluServer.AutoSize = True
        lblTitluServer.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluServer.Location = New Point(28, 556)
        lblTitluServer.Margin = New Padding(4, 0, 4, 8)
        lblTitluServer.Name = "lblTitluServer"
        lblTitluServer.Size = New Size(84, 32)
        lblTitluServer.TabIndex = 4
        lblTitluServer.Text = "Serverul"
        '
        ' tlyServer
        '
        tlyServer.AutoFitToTheme = False
        tlyServer.AutoSize = True
        tlyServer.ColumnCount = 2
        tlyServer.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyServer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyServer.Controls.Add(lblServerCaption, 0, 0)
        tlyServer.Controls.Add(lblServer, 1, 0)
        tlyServer.Controls.Add(lblTimeoutCaption, 0, 1)
        tlyServer.Controls.Add(lblTimeout, 1, 1)
        tlyServer.Controls.Add(lblServerHint, 0, 2)
        tlyServer.Dock = DockStyle.Top
        tlyServer.Location = New Point(28, 596)
        tlyServer.Margin = New Padding(4, 0, 4, 0)
        tlyServer.Name = "tlyServer"
        tlyServer.RowCount = 3
        tlyServer.RowStyles.Add(New RowStyle())
        tlyServer.RowStyles.Add(New RowStyle())
        tlyServer.RowStyles.Add(New RowStyle())
        tlyServer.Size = New Size(904, 100)
        tlyServer.TabIndex = 5
        '
        ' lblServerCaption
        '
        lblServerCaption.AutoSize = True
        lblServerCaption.Dock = DockStyle.Fill
        lblServerCaption.Location = New Point(4, 0)
        lblServerCaption.Margin = New Padding(4, 0, 4, 6)
        lblServerCaption.Name = "lblServerCaption"
        lblServerCaption.Size = New Size(252, 26)
        lblServerCaption.TabIndex = 0
        lblServerCaption.Text = "Adresa serverului K-BOT"
        '
        ' lblServer
        '
        lblServer.AutoSize = True
        lblServer.Dock = DockStyle.Fill
        lblServer.Font = New Font("Segoe UI Semibold", 9F)
        lblServer.Location = New Point(264, 0)
        lblServer.Margin = New Padding(4, 0, 4, 6)
        lblServer.Name = "lblServer"
        lblServer.Size = New Size(636, 26)
        lblServer.TabIndex = 1
        lblServer.Text = "—"
        '
        ' lblTimeoutCaption
        '
        lblTimeoutCaption.AutoSize = True
        lblTimeoutCaption.Dock = DockStyle.Fill
        lblTimeoutCaption.Location = New Point(4, 32)
        lblTimeoutCaption.Margin = New Padding(4, 0, 4, 6)
        lblTimeoutCaption.Name = "lblTimeoutCaption"
        lblTimeoutCaption.Size = New Size(252, 26)
        lblTimeoutCaption.TabIndex = 2
        lblTimeoutCaption.Text = "Timp maxim de așteptare"
        '
        ' lblTimeout
        '
        lblTimeout.AutoSize = True
        lblTimeout.Dock = DockStyle.Fill
        lblTimeout.Font = New Font("Segoe UI Semibold", 9F)
        lblTimeout.Location = New Point(264, 32)
        lblTimeout.Margin = New Padding(4, 0, 4, 6)
        lblTimeout.Name = "lblTimeout"
        lblTimeout.Size = New Size(636, 26)
        lblTimeout.TabIndex = 3
        lblTimeout.Text = "—"
        '
        ' lblServerHint
        '
        lblServerHint.AutoSize = True
        tlyServer.SetColumnSpan(lblServerHint, 2)
        lblServerHint.Dock = DockStyle.Fill
        lblServerHint.Location = New Point(4, 70)
        lblServerHint.Margin = New Padding(4, 6, 4, 0)
        lblServerHint.Name = "lblServerHint"
        lblServerHint.Size = New Size(896, 26)
        lblServerHint.TabIndex = 4
        lblServerHint.Text = "Adresa este fixată în program (doar https) și nu se poate schimba de aici. Sesiunea expiră după 20 de minute fără activitate; ce urmează spune secțiunea de mai sus."
        '
        ' SetariAutentificareView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariAutentificareView"
        Size = New Size(960, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyMemorie.ResumeLayout(False)
        tlyMemorie.PerformLayout()
        tlyLucru.ResumeLayout(False)
        tlyLucru.PerformLayout()
        tlyServer.ResumeLayout(False)
        tlyServer.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitluMemorie As Label
    Friend WithEvents tlyMemorie As KBotTableLayoutPanel
    Friend WithEvents chkRememberLogin As CheckBox
    Friend WithEvents chkRememberUnit As CheckBox
    Friend WithEvents lblUtilizatorCaption As Label
    Friend WithEvents lblUtilizator As Label
    Friend WithEvents lblUnitateCaption As Label
    Friend WithEvents lblUnitate As Label
    Friend WithEvents btnUita As Button
    Friend WithEvents lblTitluLucru As Label
    Friend WithEvents tlyLucru As KBotTableLayoutPanel
    Friend WithEvents chkRelogin As CheckBox
    Friend WithEvents lblIntervalCaption As Label
    Friend WithEvents cmbInterval As KBotComboBox
    Friend WithEvents lblReloginHint As Label
    Friend WithEvents chkRememberPasswordOption As CheckBox
    Friend WithEvents btnUitaParola As Button
    Friend WithEvents lblTitluServer As Label
    Friend WithEvents tlyServer As KBotTableLayoutPanel
    Friend WithEvents lblServerCaption As Label
    Friend WithEvents lblServer As Label
    Friend WithEvents lblTimeoutCaption As Label
    Friend WithEvents lblTimeout As Label
    Friend WithEvents lblServerHint As Label
End Class
