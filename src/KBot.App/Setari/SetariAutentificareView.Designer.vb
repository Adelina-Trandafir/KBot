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
        lblTitluServer = New Label()
        tlyServer = New KBotTableLayoutPanel()
        lblServerCaption = New Label()
        lblServer = New Label()
        lblTimeoutCaption = New Label()
        lblTimeout = New Label()
        lblServerHint = New Label()
        tlyBody.SuspendLayout()
        tlyMemorie.SuspendLayout()
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
        tlyBody.Controls.Add(lblTitluServer, 0, 2)
        tlyBody.Controls.Add(tlyServer, 0, 3)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 5
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
        ' lblTitluServer
        '
        lblTitluServer.AutoSize = True
        lblTitluServer.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluServer.Location = New Point(28, 262)
        lblTitluServer.Margin = New Padding(4, 0, 4, 8)
        lblTitluServer.Name = "lblTitluServer"
        lblTitluServer.Size = New Size(84, 32)
        lblTitluServer.TabIndex = 2
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
        tlyServer.Location = New Point(28, 302)
        tlyServer.Margin = New Padding(4, 0, 4, 0)
        tlyServer.Name = "tlyServer"
        tlyServer.RowCount = 3
        tlyServer.RowStyles.Add(New RowStyle())
        tlyServer.RowStyles.Add(New RowStyle())
        tlyServer.RowStyles.Add(New RowStyle())
        tlyServer.Size = New Size(904, 100)
        tlyServer.TabIndex = 3
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
        lblServerHint.Text = "Adresa este fixată în program (doar https) și nu se poate schimba de aici. Sesiunea expiră după 20 de minute fără activitate; la expirare se cere din nou autentificarea."
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
    Friend WithEvents lblTitluServer As Label
    Friend WithEvents tlyServer As KBotTableLayoutPanel
    Friend WithEvents lblServerCaption As Label
    Friend WithEvents lblServer As Label
    Friend WithEvents lblTimeoutCaption As Label
    Friend WithEvents lblTimeout As Label
    Friend WithEvents lblServerHint As Label
End Class
