Imports KBot.Controls

' The small dialog with the extra settings of the HOSTED Adobe window (slice 0072-01): the /n
' switch and how the window is released (slice 0078-05: plus the screen size at K-BOT's close). (The viewer profile and the floating-badge watcher went
' with slice 0078-05: the hosted window is no longer positioned or trimmed.) It opens from the
' «Aplicație» page when the PDF engine is set to «Fereastră găzduită» (and from its «Opțiuni…»
' button).
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the
' 144 dpi the dialog was authored at; AutoScaleDimensions carries the same stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdobeGazduireForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AdobeGazduireForm))
        tips = New KBotToolTip(components)
        cboAdobeInst = New KBotComboBox()
        cboAdobeDetach = New KBotComboBox()
        chkAdobeEcran = New CheckBox()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        tlyCampuri = New KBotTableLayoutPanel()
        lblAdobeInst = New Label()
        lblAdobeDetach = New Label()
        lblAdobeEcran = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        tlyMain.SuspendLayout()
        tlyCampuri.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' cboAdobeInst
        ' 
        cboAdobeInst.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeInst.CornerRadius = 4
        cboAdobeInst.Location = New Point(267, 8)
        cboAdobeInst.Margin = New Padding(4, 0, 4, 10)
        cboAdobeInst.Name = "cboAdobeInst"
        cboAdobeInst.Size = New Size(430, 37)
        cboAdobeInst.TabIndex = 1
        tips.SetToolTipHeader(cboAdobeInst, "Comutatorul /n")
        tips.SetToolTipText(cboAdobeInst, "Da: Adobe pornește un proces nou, al K-BOT." & vbLf & "Nu: Adobe poate preda documentul unei instanțe deja deschise de tine." & vbLf & "Automat: Da.")
        ' 
        ' cboAdobeDetach
        ' 
        cboAdobeDetach.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeDetach.CornerRadius = 4
        cboAdobeDetach.Location = New Point(267, 55)
        cboAdobeDetach.Margin = New Padding(4, 0, 4, 10)
        cboAdobeDetach.Name = "cboAdobeDetach"
        cboAdobeDetach.Size = New Size(430, 37)
        cboAdobeDetach.TabIndex = 3
        tips.SetToolTipHeader(cboAdobeDetach, "Cum se eliberează fereastra Adobe")
        tips.SetToolTipText(cboAdobeDetach, "A: oprește procesul pornit de K-BOT — determinist." & vbLf & "B: închide doar fereastra și lasă procesul cald, deci următorul document pornește mai repede.")
        ' 
        ' chkAdobeEcran
        ' 
        chkAdobeEcran.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        chkAdobeEcran.AutoSize = True
        chkAdobeEcran.Location = New Point(267, 102)
        chkAdobeEcran.Margin = New Padding(4, 0, 4, 10)
        chkAdobeEcran.Name = "chkAdobeEcran"
        chkAdobeEcran.Size = New Size(430, 26)
        chkAdobeEcran.TabIndex = 5
        chkAdobeEcran.Text = "Readu fereastra Adobe la dimensiunea ecranului"
        tips.SetToolTipHeader(chkAdobeEcran, "Adobe pe tot ecranul la închidere")
        tips.SetToolTipText(chkAdobeEcran, resources.GetString("chkAdobeEcran.ToolTipText"))
        chkAdobeEcran.UseVisualStyleBackColor = True
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
        btnSalveaza.Location = New Point(677, 0)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(48, 48)
        btnSalveaza.TabIndex = 1
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie setările (kbot_paths.json și app_settings.json) și închide.")
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 80F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.Size = New Size(725, 360)
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
        capBar.Size = New Size(725, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Fereastră găzduită Adobe"
        ' 
        ' lblIntro
        ' 
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(4, 57)
        lblIntro.Margin = New Padding(4, 0, 4, 0)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(20, 8, 20, 8)
        lblIntro.Size = New Size(717, 80)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Setările de mai jos privesc DOAR motorul «Fereastră găzduită» — fereastra Adobe mutată în panoul K-BOT. Se aplică documentului următor."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyCampuri
        ' 
        tlyCampuri.AutoFitToTheme = False
        tlyCampuri.ColumnCount = 2
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 239F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.Controls.Add(lblAdobeInst, 0, 0)
        tlyCampuri.Controls.Add(cboAdobeInst, 1, 0)
        tlyCampuri.Controls.Add(lblAdobeDetach, 0, 1)
        tlyCampuri.Controls.Add(cboAdobeDetach, 1, 1)
        tlyCampuri.Controls.Add(lblAdobeEcran, 0, 2)
        tlyCampuri.Controls.Add(chkAdobeEcran, 1, 2)
        tlyCampuri.Dock = DockStyle.Fill
        tlyCampuri.Location = New Point(0, 137)
        tlyCampuri.Margin = New Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.Padding = New Padding(24, 8, 24, 8)
        tlyCampuri.RowCount = 4
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCampuri.Size = New Size(725, 145)
        tlyCampuri.TabIndex = 2
        ' 
        ' lblAdobeInst
        ' 
        lblAdobeInst.AutoSize = True
        lblAdobeInst.Dock = DockStyle.Fill
        lblAdobeInst.Location = New Point(28, 8)
        lblAdobeInst.Margin = New Padding(4, 0, 4, 10)
        lblAdobeInst.Name = "lblAdobeInst"
        lblAdobeInst.Size = New Size(231, 37)
        lblAdobeInst.TabIndex = 0
        lblAdobeInst.Text = "Instanță nouă Adobe (/n)"
        lblAdobeInst.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblAdobeDetach
        ' 
        lblAdobeDetach.AutoSize = True
        lblAdobeDetach.Dock = DockStyle.Fill
        lblAdobeDetach.Location = New Point(28, 55)
        lblAdobeDetach.Margin = New Padding(4, 0, 4, 10)
        lblAdobeDetach.Name = "lblAdobeDetach"
        lblAdobeDetach.Size = New Size(231, 37)
        lblAdobeDetach.TabIndex = 2
        lblAdobeDetach.Text = "La schimbarea documentului"
        lblAdobeDetach.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblAdobeEcran
        ' 
        lblAdobeEcran.AutoSize = True
        lblAdobeEcran.Dock = DockStyle.Fill
        lblAdobeEcran.Location = New Point(28, 102)
        lblAdobeEcran.Margin = New Padding(4, 0, 4, 10)
        lblAdobeEcran.Name = "lblAdobeEcran"
        lblAdobeEcran.Size = New Size(231, 26)
        lblAdobeEcran.TabIndex = 4
        lblAdobeEcran.Text = "La închiderea K-BOT"
        lblAdobeEcran.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlySubsol
        ' 
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnRenunta, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Bottom
        tlySubsol.Location = New Point(0, 312)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlySubsol.Size = New Size(725, 48)
        tlySubsol.TabIndex = 3
        ' 
        ' AdobeGazduireForm
        ' 
        AcceptButton = btnSalveaza
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(727, 362)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "AdobeGazduireForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Fereastră găzduită Adobe"
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
    Friend WithEvents lblAdobeInst As Label
    Friend WithEvents cboAdobeInst As KBotComboBox
    Friend WithEvents lblAdobeDetach As Label
    Friend WithEvents cboAdobeDetach As KBotComboBox
    Friend WithEvents lblAdobeEcran As Label
    Friend WithEvents chkAdobeEcran As CheckBox
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
