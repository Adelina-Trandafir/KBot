Imports KBot.Controls

' The dialog with the list of Adobe script alerts K-BOT closes by itself (slice 0078-03, pass 06):
' one regular expression per line, a test box that says what a pasted message would get, and
' the defaults button to go back to the shipped list. Opens from the application page of Setari.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the
' 144 dpi the dialog was authored at; AutoScaleDimensions carries the same stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdobeMesajeForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AdobeMesajeForm))
        tips = New KBotToolTip(components)
        txtReguli = New KBotTextBox()
        txtProba = New KBotTextBox()
        btnImplicite = New Button()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        tlyCampuri = New KBotTableLayoutPanel()
        lblReguli = New Label()
        lblProba = New Label()
        lblRezultat = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyCampuri.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' txtReguli
        ' 
        txtReguli.AcceptsReturn = True
        txtReguli.Dock = DockStyle.Fill
        txtReguli.Font = New Font("Consolas", 10.5F)
        txtReguli.Location = New Point(28, 36)
        txtReguli.Margin = New Padding(4, 0, 4, 12)
        txtReguli.Name = "txtReguli"
        txtReguli.PlaceholderText = "GeneralError" & vbLf & "Operation failed" & vbLf & "TypeError"
        txtReguli.Size = New Size(882, 264)
        txtReguli.TabIndex = 1
        tips.SetToolTipHeader(txtReguli, "Lista mesajelor închise automat")
        tips.SetToolTipText(txtReguli, "Câte o expresie regulată pe rând; rândurile goale se ignoră." & vbLf & "Exemple: «GeneralError», «^Operation failed», «is not a (function|object)»." & vbLf & "Un mesaj care nu se potrivește cu niciun rând rămâne pe ecran.")
        txtReguli.WordWrap = False
        ' 
        ' txtProba
        ' 
        txtProba.Dock = DockStyle.Fill
        txtProba.Location = New Point(28, 340)
        txtProba.Margin = New Padding(4, 0, 4, 6)
        txtProba.Name = "txtProba"
        txtProba.PlaceholderText = "Validarea s-a terminat cu succes!"
        txtProba.Size = New Size(882, 42)
        txtProba.TabIndex = 3
        tips.SetToolTipHeader(txtProba, "Probă")
        tips.SetToolTipText(txtProba, "Textul se verifică pe loc cu lista de mai sus (și cea nesalvată)." & vbLf & "Textul exact al unui mesaj îl găsești în jurnalul Adobe (adobe_preview.log).")
        ' 
        ' btnImplicite
        ' 
        btnImplicite.Dock = DockStyle.Fill
        btnImplicite.FlatStyle = FlatStyle.Flat
        btnImplicite.Image = My.Resources.Resources.default_32
        btnImplicite.Location = New Point(842, 0)
        btnImplicite.Margin = New Padding(0)
        btnImplicite.Name = "btnImplicite"
        btnImplicite.Size = New Size(48, 48)
        btnImplicite.TabIndex = 2
        tips.SetToolTipHeader(btnImplicite, "Lista implicită")
        tips.SetToolTipText(btnImplicite, "Pune în casetă lista livrată cu K-BOT (erorile de script văzute până acum)." & vbLf & "Nu salvează: apasă «Salvează» ca să rămână.")
        btnImplicite.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Image = My.Resources.Resources.left_32
        btnRenunta.Location = New Point(0, 0)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(48, 48)
        btnRenunta.TabIndex = 0
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fără să schimbe nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Font = New Font("Segoe UI Semibold", 9F)
        btnSalveaza.Image = My.Resources.Resources.save_32
        btnSalveaza.Location = New Point(890, 0)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(48, 48)
        btnSalveaza.TabIndex = 1
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Verifică fiecare rând, scrie lista în app_settings.json și închide." & vbLf & "Se aplică de la următorul mesaj Adobe.")
        btnSalveaza.UseVisualStyleBackColor = True
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 120F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.Size = New Size(938, 643)
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
        capBar.Size = New Size(938, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Mesaje de script Adobe"
        ' 
        ' lblIntro
        ' 
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(4, 57)
        lblIntro.Margin = New Padding(4, 0, 4, 0)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(20, 8, 20, 8)
        lblIntro.Size = New Size(930, 120)
        lblIntro.TabIndex = 1
        lblIntro.Text = resources.GetString("lblIntro.Text")
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyCampuri
        ' 
        tlyCampuri.AutoFitToTheme = False
        tlyCampuri.ColumnCount = 1
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.Controls.Add(lblReguli, 0, 0)
        tlyCampuri.Controls.Add(txtReguli, 0, 1)
        tlyCampuri.Controls.Add(lblProba, 0, 2)
        tlyCampuri.Controls.Add(txtProba, 0, 3)
        tlyCampuri.Controls.Add(lblRezultat, 0, 4)
        tlyCampuri.Dock = DockStyle.Fill
        tlyCampuri.Location = New Point(0, 177)
        tlyCampuri.Margin = New Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.Padding = New Padding(24, 8, 24, 8)
        tlyCampuri.RowCount = 5
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.Size = New Size(938, 418)
        tlyCampuri.TabIndex = 2
        ' 
        ' lblReguli
        ' 
        lblReguli.AutoSize = True
        lblReguli.Dock = DockStyle.Fill
        lblReguli.Location = New Point(28, 8)
        lblReguli.Margin = New Padding(4, 0, 4, 6)
        lblReguli.Name = "lblReguli"
        lblReguli.Size = New Size(882, 22)
        lblReguli.TabIndex = 0
        lblReguli.Text = "Mesaje închise automat (câte o expresie pe rând)"
        ' 
        ' lblProba
        ' 
        lblProba.AutoSize = True
        lblProba.Dock = DockStyle.Fill
        lblProba.Location = New Point(28, 312)
        lblProba.Margin = New Padding(4, 0, 4, 6)
        lblProba.Name = "lblProba"
        lblProba.Size = New Size(882, 22)
        lblProba.TabIndex = 2
        lblProba.Text = "Probă — lipește aici textul unui mesaj Adobe"
        ' 
        ' lblRezultat
        ' 
        lblRezultat.AutoSize = True
        lblRezultat.Dock = DockStyle.Fill
        lblRezultat.Location = New Point(28, 388)
        lblRezultat.Margin = New Padding(4, 0, 4, 0)
        lblRezultat.Name = "lblRezultat"
        lblRezultat.Size = New Size(882, 22)
        lblRezultat.TabIndex = 4
        ' 
        ' tlySubsol
        ' 
        tlySubsol.ColumnCount = 4
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnImplicite, 2, 0)
        tlySubsol.Controls.Add(btnRenunta, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 3, 0)
        tlySubsol.Dock = DockStyle.Bottom
        tlySubsol.Location = New Point(0, 595)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle())
        tlySubsol.Size = New Size(938, 48)
        tlySubsol.TabIndex = 3
        ' 
        ' AdobeMesajeForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(940, 645)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "AdobeMesajeForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Mesaje de script Adobe"
        tlyMain.ResumeLayout(False)
        tlyCampuri.ResumeLayout(False)
        tlyCampuri.PerformLayout()
        tlySubsol.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents tlyCampuri As KBotTableLayoutPanel
    Friend WithEvents lblReguli As Label
    Friend WithEvents txtReguli As KBotTextBox
    Friend WithEvents lblProba As Label
    Friend WithEvents txtProba As KBotTextBox
    Friend WithEvents lblRezultat As Label
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnImplicite As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
