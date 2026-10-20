' The account dialog (slice ME01): link an e-mail to a database with a K-BOT role, or change that role.
' All controls are declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AccountForm
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
        lblUn = New Label()
        txtUn = New KBotTextField()
        lblDb = New Label()
        cmbDb = New KBotComboBox()
        lblRol = New Label()
        cmbRol = New KBotComboBox()
        lblInfo = New Label()
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
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblUn, 0, 0)
        tlyMain.Controls.Add(txtUn, 1, 0)
        tlyMain.Controls.Add(lblDb, 0, 1)
        tlyMain.Controls.Add(cmbDb, 1, 1)
        tlyMain.Controls.Add(lblRol, 0, 2)
        tlyMain.Controls.Add(cmbRol, 1, 2)
        tlyMain.Controls.Add(lblInfo, 0, 3)
        tlyMain.Controls.Add(tlyButtons, 0, 4)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 16, 16, 12)
        tlyMain.RowCount = 5
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.Size = New Size(480, 280)
        tlyMain.TabIndex = 0
        '
        ' lblUn
        '
        tips.SetToolTipHeader(lblUn, "Cont")
        tips.SetToolTipText(lblUn, "Adresa de e-mail a contului (identificatorul din K-BOT).")
        lblUn.Dock = DockStyle.Fill
        lblUn.Location = New Point(16, 16)
        lblUn.Margin = New Padding(0)
        lblUn.Name = "lblUn"
        lblUn.Size = New Size(110, 40)
        lblUn.TabIndex = 0
        lblUn.Text = "Cont (e-mail):"
        lblUn.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtUn
        '
        txtUn.BackColor = Color.Transparent
        txtUn.Dock = DockStyle.Fill
        txtUn.Location = New Point(126, 16)
        txtUn.Margin = New Padding(0, 3, 0, 3)
        txtUn.Name = "txtUn"
        txtUn.Size = New Size(338, 34)
        txtUn.TabIndex = 1
        txtUn.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtUn, "Cont")
        tips.SetToolTipText(txtUn, "Adresa de e-mail a contului." & vbLf & "Contul MariaDB al utilizatorului trebuie să existe deja; aici se face doar legătura cu baza.")
        '
        ' lblDb
        '
        tips.SetToolTipHeader(lblDb, "Baza")
        tips.SetToolTipText(lblDb, "Baza de date de unitate la care se leagă contul.")
        lblDb.Dock = DockStyle.Fill
        lblDb.Location = New Point(16, 56)
        lblDb.Margin = New Padding(0)
        lblDb.Name = "lblDb"
        lblDb.Size = New Size(110, 40)
        lblDb.TabIndex = 2
        lblDb.Text = "Baza:"
        lblDb.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbDb
        '
        tips.SetToolTipHeader(cmbDb, "Baza")
        tips.SetToolTipText(cmbDb, "Alege baza la care se leagă contul. Doar bazele de unitate apar aici; AVACONT_SURSA și AVACONT_COMUN nu.")
        cmbDb.Dock = DockStyle.Fill
        cmbDb.Location = New Point(126, 56)
        cmbDb.Margin = New Padding(0, 4, 0, 4)
        cmbDb.Name = "cmbDb"
        cmbDb.Size = New Size(338, 32)
        cmbDb.TabIndex = 3
        '
        ' lblRol
        '
        tips.SetToolTipHeader(lblRol, "Rol K-BOT")
        tips.SetToolTipText(lblRol, "Rolul contului în aplicația K-BOT, în baza aleasă.")
        lblRol.Dock = DockStyle.Fill
        lblRol.Location = New Point(16, 96)
        lblRol.Margin = New Padding(0)
        lblRol.Name = "lblRol"
        lblRol.Size = New Size(110, 40)
        lblRol.TabIndex = 4
        lblRol.Text = "Rol K-BOT:"
        lblRol.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbRol
        '
        cmbRol.Dock = DockStyle.Fill
        cmbRol.Location = New Point(126, 96)
        cmbRol.Margin = New Padding(0, 4, 0, 4)
        cmbRol.Name = "cmbRol"
        cmbRol.Size = New Size(338, 32)
        cmbRol.TabIndex = 5
        tips.SetToolTipHeader(cmbRol, "Rol K-BOT")
        tips.SetToolTipText(cmbRol, "Rolul din aplicația K-BOT (contabil, director, administrator, secretar)." & vbLf & "Drepturile ADECHIT se dau separat, în pagina «Drepturi pe conturi».")
        '
        ' lblInfo
        '
        tips.SetToolTipHeader(lblInfo, "Ce se întâmplă")
        tips.SetToolTipText(lblInfo, "Se salvează legătura cont–bază și se sincronizează privilegiile MariaDB pentru toate bazele contului. Operația intră în jurnalul de activitate.")
        tlyMain.SetColumnSpan(lblInfo, 2)
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(16, 136)
        lblInfo.Margin = New Padding(0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(448, 78)
        lblInfo.TabIndex = 6
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyMain.SetColumnSpan(tlyButtons, 2)
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.Controls.Add(btnCancel, 0, 0)
        tlyButtons.Controls.Add(btnOk, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 224)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(448, 44)
        tlyButtons.TabIndex = 7
        '
        ' btnCancel
        '
        tips.SetToolTipHeader(btnCancel, "Renunță")
        tips.SetToolTipText(btnCancel, "Închide fereastra fără să schimbe nimic.")
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Fill
        btnCancel.Location = New Point(0, 0)
        btnCancel.Margin = New Padding(0, 2, 6, 2)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(114, 40)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Renunță"
        '
        ' btnOk
        '
        tips.SetToolTipHeader(btnOk, "Salvează")
        tips.SetToolTipText(btnOk, "Scrie legătura pe server și o trece în jurnalul de activitate.")
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(328, 0)
        btnOk.Margin = New Padding(6, 2, 0, 2)
        btnOk.Name = "btnOk"
        btnOk.Primary = True
        btnOk.Size = New Size(114, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Salvează"
        '
        ' AccountForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(480, 280)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "AccountForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Cont"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblUn As Label
    Friend WithEvents txtUn As KBotTextField
    Friend WithEvents lblDb As Label
    Friend WithEvents cmbDb As KBotComboBox
    Friend WithEvents lblRol As Label
    Friend WithEvents cmbRol As KBotComboBox
    Friend WithEvents lblInfo As Label
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCancel As KBotButton
    Friend WithEvents btnOk As KBotButton
End Class
