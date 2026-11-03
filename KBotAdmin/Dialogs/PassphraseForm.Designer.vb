' The passphrase dialog of the local backup (slice ME02). All controls are declared HERE; coordinates are in the 96 dpi
' the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PassphraseForm
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
        lblPrompt = New Label()
        lblPass = New Label()
        txtPass = New KBotTextField()
        lblConfirm = New Label()
        txtConfirm = New KBotTextField()
        chkRemember = New KBotCheckBox()
        lblWarn = New Label()
        tlyButtons = New KBotTableLayoutPanel()
        btnCancel = New KBotButton()
        btnOk = New KBotButton()
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
        tlyMain.Controls.Add(lblPrompt, 0, 0)
        tlyMain.Controls.Add(lblPass, 0, 1)
        tlyMain.Controls.Add(txtPass, 1, 1)
        tlyMain.Controls.Add(lblConfirm, 0, 2)
        tlyMain.Controls.Add(txtConfirm, 1, 2)
        tlyMain.Controls.Add(chkRemember, 0, 3)
        tlyMain.Controls.Add(lblWarn, 0, 4)
        tlyMain.Controls.Add(tlyButtons, 0, 5)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 16, 16, 12)
        tlyMain.RowCount = 6
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 72F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(520, 340)
        tlyMain.TabIndex = 0
        '
        ' lblPrompt
        '
        tips.SetToolTipHeader(lblPrompt, "Parola copiilor locale")
        tips.SetToolTipText(lblPrompt, "Parola din care se fac cheile de criptare ale copiilor de pe acest calculator.")
        tlyMain.SetColumnSpan(lblPrompt, 2)
        lblPrompt.Dock = DockStyle.Fill
        lblPrompt.Location = New Point(16, 16)
        lblPrompt.Margin = New Padding(0)
        lblPrompt.Name = "lblPrompt"
        lblPrompt.Size = New Size(488, 72)
        lblPrompt.TabIndex = 0
        lblPrompt.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblPass
        '
        lblPass.Dock = DockStyle.Fill
        lblPass.Location = New Point(16, 88)
        lblPass.Margin = New Padding(0)
        lblPass.Name = "lblPass"
        lblPass.Size = New Size(130, 40)
        lblPass.TabIndex = 1
        lblPass.Text = "Parolă:"
        lblPass.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtPass
        '
        txtPass.BackColor = Color.Transparent
        txtPass.Dock = DockStyle.Fill
        txtPass.Location = New Point(146, 91)
        txtPass.Margin = New Padding(0, 3, 0, 3)
        txtPass.Name = "txtPass"
        txtPass.Size = New Size(358, 34)
        txtPass.TabIndex = 2
        txtPass.TextPadding = New Padding(8, 0, 8, 0)
        txtPass.UseSystemPasswordChar = True
        tips.SetToolTipHeader(txtPass, "Parola")
        tips.SetToolTipText(txtPass, "Nu se salvează în clar nicăieri. Fără ea copiile locale nu se mai pot citi: păstrați-o în managerul de parole.")
        '
        ' lblConfirm
        '
        lblConfirm.Dock = DockStyle.Fill
        lblConfirm.Location = New Point(16, 128)
        lblConfirm.Margin = New Padding(0)
        lblConfirm.Name = "lblConfirm"
        lblConfirm.Size = New Size(130, 40)
        lblConfirm.TabIndex = 3
        lblConfirm.Text = "Încă o dată:"
        lblConfirm.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtConfirm
        '
        txtConfirm.BackColor = Color.Transparent
        txtConfirm.Dock = DockStyle.Fill
        txtConfirm.Location = New Point(146, 131)
        txtConfirm.Margin = New Padding(0, 3, 0, 3)
        txtConfirm.Name = "txtConfirm"
        txtConfirm.Size = New Size(358, 34)
        txtConfirm.TabIndex = 4
        txtConfirm.TextPadding = New Padding(8, 0, 8, 0)
        txtConfirm.UseSystemPasswordChar = True
        tips.SetToolTipHeader(txtConfirm, "Parola, încă o dată")
        tips.SetToolTipText(txtConfirm, "Scrieți aceeași parolă, ca să nu rămână o greșeală de tastare în cheie.")
        '
        ' chkRemember
        '
        tips.SetToolTipHeader(chkRemember, "Păstrează cheia pe acest calculator")
        tips.SetToolTipText(chkRemember, "Cheia se păstrează într-un fișier protejat de contul dumneavoastră de Windows. Copiile automate pornesc fără să vă ceară parola.")
        tlyMain.SetColumnSpan(chkRemember, 2)
        chkRemember.Dock = DockStyle.Fill
        chkRemember.Location = New Point(16, 168)
        chkRemember.Margin = New Padding(0)
        chkRemember.Name = "chkRemember"
        chkRemember.Size = New Size(488, 34)
        chkRemember.TabIndex = 5
        chkRemember.Text = "Păstrează cheia pe acest calculator (pentru copiile automate)"
        '
        ' lblWarn
        '
        tips.SetToolTipHeader(lblWarn, "Atenție")
        tips.SetToolTipText(lblWarn, "Parola nu se poate recupera. Copiile se citesc doar cu ea.")
        tlyMain.SetColumnSpan(lblWarn, 2)
        lblWarn.Dock = DockStyle.Fill
        lblWarn.Location = New Point(16, 202)
        lblWarn.Margin = New Padding(0)
        lblWarn.Name = "lblWarn"
        lblWarn.Size = New Size(488, 74)
        lblWarn.TabIndex = 6
        lblWarn.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyButtons.Controls.Add(btnCancel, 0, 0)
        tlyButtons.Controls.Add(btnOk, 2, 0)
        tlyMain.SetColumnSpan(tlyButtons, 2)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 276)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(488, 52)
        tlyButtons.TabIndex = 7
        '
        ' btnCancel
        '
        tips.SetToolTipHeader(btnCancel, "Renunță")
        tips.SetToolTipText(btnCancel, "Închide fereastra fără să facă nimic.")
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Fill
        btnCancel.Location = New Point(0, 0)
        btnCancel.Margin = New Padding(0, 6, 6, 6)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(114, 40)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Renunță"
        '
        ' btnOk
        '
        tips.SetToolTipHeader(btnOk, "Continuă")
        tips.SetToolTipText(btnOk, "Folosește parola scrisă.")
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(348, 0)
        btnOk.Margin = New Padding(6, 6, 0, 6)
        btnOk.Name = "btnOk"
        btnOk.Primary = True
        btnOk.Size = New Size(140, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Continuă"
        '
        ' PassphraseForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(520, 340)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "PassphraseForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Parola copiilor locale"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblPrompt As Label
    Friend WithEvents lblPass As Label
    Friend WithEvents txtPass As KBotTextField
    Friend WithEvents lblConfirm As Label
    Friend WithEvents txtConfirm As KBotTextField
    Friend WithEvents chkRemember As KBotCheckBox
    Friend WithEvents lblWarn As Label
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCancel As KBotButton
    Friend WithEvents btnOk As KBotButton
End Class
