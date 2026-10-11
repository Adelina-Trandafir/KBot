Imports KBot.Controls

' The mandatory password-change dialog shown when the account still has its initial password
' (security plan S1). All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are in the 144 dpi the dialog was authored at; AutoScaleDimensions carries the stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForcedPasswordChangeForm
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
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        tlyCampuri = New KBotTableLayoutPanel()
        lblStare = New Label()
        lblCod = New Label()
        txtCod = New KBotTextField()
        lblNoua = New Label()
        txtNoua = New KBotTextField()
        lblConfirmare = New Label()
        txtConfirmare = New KBotTextField()
        lblCerinte = New Label()
        ntfEroare = New KBotNotice()
        tlySubsol = New KBotTableLayoutPanel()
        btnIesire = New Button()
        btnRetrimite = New Button()
        btnSchimba = New Button()
        tlyMain.SuspendLayout()
        tlyCampuri.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(lblIntro, 0, 1)
        tlyMain.Controls.Add(tlyCampuri, 0, 2)
        tlyMain.Controls.Add(tlySubsol, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 96F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.Size = New Size(758, 638)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.settings__1_
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(758, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Schimbarea parolei"
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(4, 57)
        lblIntro.Margin = New Padding(4, 0, 4, 0)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(20, 8, 20, 8)
        lblIntro.Size = New Size(750, 96)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Parola contului este cea inițială. Din motive de securitate trebuie să o schimbați înainte de a folosi aplicația."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyCampuri
        '
        tlyCampuri.AutoFitToTheme = False
        tlyCampuri.ColumnCount = 2
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.Controls.Add(lblStare, 0, 0)
        tlyCampuri.Controls.Add(lblCod, 0, 1)
        tlyCampuri.Controls.Add(txtCod, 1, 1)
        tlyCampuri.Controls.Add(lblNoua, 0, 2)
        tlyCampuri.Controls.Add(txtNoua, 1, 2)
        tlyCampuri.Controls.Add(lblConfirmare, 0, 3)
        tlyCampuri.Controls.Add(txtConfirmare, 1, 3)
        tlyCampuri.Controls.Add(lblCerinte, 0, 4)
        tlyCampuri.Controls.Add(ntfEroare, 0, 5)
        tlyCampuri.Dock = DockStyle.Fill
        tlyCampuri.Location = New Point(0, 153)
        tlyCampuri.Margin = New Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.Padding = New Padding(24, 8, 24, 8)
        tlyCampuri.RowCount = 6
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Absolute, 70F))
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.Size = New Size(758, 437)
        tlyCampuri.TabIndex = 2
        '
        ' lblStare
        '
        tlyCampuri.SetColumnSpan(lblStare, 2)
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(28, 8)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(702, 70)
        lblStare.TabIndex = 0
        lblStare.Text = "Se trimite codul de confirmare pe e-mail..."
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCod
        '
        lblCod.Dock = DockStyle.Fill
        lblCod.Location = New Point(28, 78)
        lblCod.Margin = New Padding(4, 0, 4, 10)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(192, 50)
        lblCod.TabIndex = 1
        lblCod.Text = "Cod primit pe e-mail"
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCod
        '
        txtCod.BackColor = Color.Transparent
        txtCod.Dock = DockStyle.Fill
        txtCod.Location = New Point(228, 78)
        txtCod.Margin = New Padding(4, 0, 4, 10)
        txtCod.MaxLength = 12
        txtCod.Name = "txtCod"
        txtCod.Size = New Size(502, 50)
        txtCod.TabIndex = 2
        tips.SetToolTipHeader(txtCod, "Cod de confirmare")
        tips.SetToolTipText(txtCod, "Codul de 6 cifre trimis pe adresa de e-mail a contului.")
        '
        ' lblNoua
        '
        lblNoua.Dock = DockStyle.Fill
        lblNoua.Location = New Point(28, 138)
        lblNoua.Margin = New Padding(4, 0, 4, 10)
        lblNoua.Name = "lblNoua"
        lblNoua.Size = New Size(192, 50)
        lblNoua.TabIndex = 3
        lblNoua.Text = "Parola nouă"
        lblNoua.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtNoua
        '
        txtNoua.BackColor = Color.Transparent
        txtNoua.Dock = DockStyle.Fill
        txtNoua.Location = New Point(228, 138)
        txtNoua.Margin = New Padding(4, 0, 4, 10)
        txtNoua.Name = "txtNoua"
        txtNoua.Size = New Size(502, 50)
        txtNoua.TabIndex = 4
        tips.SetToolTipHeader(txtNoua, "Parola nouă")
        tips.SetToolTipText(txtNoua, "Cerințele serverului sunt afișate mai jos și se actualizează pe măsură ce scrieți.")
        txtNoua.UseSystemPasswordChar = True
        '
        ' lblConfirmare
        '
        lblConfirmare.Dock = DockStyle.Fill
        lblConfirmare.Location = New Point(28, 198)
        lblConfirmare.Margin = New Padding(4, 0, 4, 10)
        lblConfirmare.Name = "lblConfirmare"
        lblConfirmare.Size = New Size(192, 50)
        lblConfirmare.TabIndex = 5
        lblConfirmare.Text = "Confirmare parolă"
        lblConfirmare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtConfirmare
        '
        txtConfirmare.BackColor = Color.Transparent
        txtConfirmare.Dock = DockStyle.Fill
        txtConfirmare.Location = New Point(228, 198)
        txtConfirmare.Margin = New Padding(4, 0, 4, 10)
        txtConfirmare.Name = "txtConfirmare"
        txtConfirmare.Size = New Size(502, 50)
        txtConfirmare.TabIndex = 6
        tips.SetToolTipHeader(txtConfirmare, "Confirmare parolă")
        tips.SetToolTipText(txtConfirmare, "Scrieți din nou parola nouă.")
        txtConfirmare.UseSystemPasswordChar = True
        '
        ' lblCerinte
        '
        tlyCampuri.SetColumnSpan(lblCerinte, 2)
        lblCerinte.Dock = DockStyle.Fill
        lblCerinte.Location = New Point(28, 258)
        lblCerinte.Margin = New Padding(4, 0, 4, 0)
        lblCerinte.Name = "lblCerinte"
        lblCerinte.Size = New Size(702, 100)
        lblCerinte.TabIndex = 7
        lblCerinte.Text = "Se citesc cerințele de parolă de pe server..."
        '
        ' ntfEroare
        '
        ntfEroare.BackColor = Color.Transparent
        tlyCampuri.SetColumnSpan(ntfEroare, 2)
        ntfEroare.Dock = DockStyle.Fill
        ntfEroare.Location = New Point(28, 362)
        ntfEroare.Margin = New Padding(4, 4, 4, 0)
        ntfEroare.Name = "ntfEroare"
        ntfEroare.Size = New Size(702, 67)
        ntfEroare.TabIndex = 8
        ntfEroare.TabStop = False
        ntfEroare.Visible = False
        '
        ' tlySubsol
        '
        tlySubsol.ColumnCount = 4
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnIesire, 0, 0)
        tlySubsol.Controls.Add(btnRetrimite, 1, 0)
        tlySubsol.Controls.Add(btnSchimba, 3, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 590)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle())
        tlySubsol.Size = New Size(758, 48)
        tlySubsol.TabIndex = 3
        '
        ' btnIesire
        '
        btnIesire.DialogResult = DialogResult.Cancel
        btnIesire.Dock = DockStyle.Fill
        btnIesire.FlatStyle = FlatStyle.Flat
        btnIesire.Image = My.Resources.Resources.left_32
        btnIesire.Location = New Point(0, 0)
        btnIesire.Margin = New Padding(0)
        btnIesire.Name = "btnIesire"
        btnIesire.Size = New Size(48, 48)
        btnIesire.TabIndex = 2
        tips.SetToolTipHeader(btnIesire, "Ieșire")
        tips.SetToolTipText(btnIesire, "Închide fereastra fără a schimba parola. Aplicația nu se deschide până la schimbare.")
        btnIesire.UseVisualStyleBackColor = True
        '
        ' btnRetrimite
        '
        btnRetrimite.AutoSize = True
        btnRetrimite.Dock = DockStyle.Fill
        btnRetrimite.FlatStyle = FlatStyle.Flat
        btnRetrimite.Location = New Point(58, 0)
        btnRetrimite.Margin = New Padding(10, 0, 0, 0)
        btnRetrimite.Name = "btnRetrimite"
        btnRetrimite.Padding = New Padding(10, 0, 10, 0)
        btnRetrimite.Size = New Size(200, 48)
        btnRetrimite.TabIndex = 1
        btnRetrimite.Text = "Trimite din nou codul"
        btnRetrimite.UseVisualStyleBackColor = True
        '
        ' btnSchimba
        '
        btnSchimba.Dock = DockStyle.Fill
        btnSchimba.FlatStyle = FlatStyle.Flat
        btnSchimba.Font = New Font("Segoe UI Semibold", 9F)
        btnSchimba.Image = My.Resources.Resources.right_32
        btnSchimba.Location = New Point(710, 0)
        btnSchimba.Margin = New Padding(0)
        btnSchimba.Name = "btnSchimba"
        btnSchimba.Size = New Size(48, 48)
        btnSchimba.TabIndex = 0
        tips.SetToolTipHeader(btnSchimba, "Schimbă parola")
        tips.SetToolTipText(btnSchimba, "Schimbă parola contului și continuă către autentificare.")
        btnSchimba.UseVisualStyleBackColor = True
        '
        ' ForcedPasswordChangeForm
        '
        AcceptButton = btnSchimba
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnIesire
        ClientSize = New Size(760, 640)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "ForcedPasswordChangeForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Schimbarea parolei"
        tlyMain.ResumeLayout(False)
        tlyCampuri.ResumeLayout(False)
        tlyCampuri.PerformLayout()
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents tlyCampuri As KBotTableLayoutPanel
    Friend WithEvents lblStare As Label
    Friend WithEvents lblCod As Label
    Friend WithEvents txtCod As KBotTextField
    Friend WithEvents lblNoua As Label
    Friend WithEvents txtNoua As KBotTextField
    Friend WithEvents lblConfirmare As Label
    Friend WithEvents txtConfirmare As KBotTextField
    Friend WithEvents lblCerinte As Label
    Friend WithEvents ntfEroare As KBotNotice
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnIesire As Button
    Friend WithEvents btnRetrimite As Button
    Friend WithEvents btnSchimba As Button
End Class
