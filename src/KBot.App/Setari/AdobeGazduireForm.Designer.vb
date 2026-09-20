Imports KBot.Controls

' The small dialog with the extra settings of the HOSTED Adobe window (slice 0072-01): the
' viewer profile, the /n switch, how the window is released and the floating-badge watcher.
' It opens from the «Aplicație» page when the PDF engine is set to «Fereastră găzduită» (and
' from its «Opțiuni…» button), so those four rows are no longer on the page itself.
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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        tlyCampuri = New KBotTableLayoutPanel()
        lblAdobeMod = New Label()
        cboAdobeMod = New KBotComboBox()
        lblAdobeInst = New Label()
        cboAdobeInst = New KBotComboBox()
        lblAdobeDetach = New Label()
        cboAdobeDetach = New KBotComboBox()
        chkAdobePopup = New CheckBox()
        tlySubsol = New KBotTableLayoutPanel()
        btnRenunta = New Button()
        btnSalveaza = New Button()
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 80F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.Size = New Size(858, 418)
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
        capBar.Size = New Size(858, 57)
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
        lblIntro.Size = New Size(850, 80)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Setările de mai jos privesc DOAR motorul «Fereastră găzduită» — fereastra Adobe mutată în panoul K-BOT. Se aplică documentului următor."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyCampuri
        '
        tlyCampuri.AutoFitToTheme = False
        tlyCampuri.ColumnCount = 2
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 330F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.Controls.Add(lblAdobeMod, 0, 0)
        tlyCampuri.Controls.Add(cboAdobeMod, 1, 0)
        tlyCampuri.Controls.Add(lblAdobeInst, 0, 1)
        tlyCampuri.Controls.Add(cboAdobeInst, 1, 1)
        tlyCampuri.Controls.Add(lblAdobeDetach, 0, 2)
        tlyCampuri.Controls.Add(cboAdobeDetach, 1, 2)
        tlyCampuri.Controls.Add(chkAdobePopup, 0, 3)
        tlyCampuri.Dock = DockStyle.Fill
        tlyCampuri.Location = New Point(0, 137)
        tlyCampuri.Margin = New Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.Padding = New Padding(24, 8, 24, 8)
        tlyCampuri.RowCount = 5
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCampuri.Size = New Size(858, 203)
        tlyCampuri.TabIndex = 2
        '
        ' lblAdobeMod
        '
        lblAdobeMod.AutoSize = True
        lblAdobeMod.Dock = DockStyle.Fill
        lblAdobeMod.Location = New Point(28, 8)
        lblAdobeMod.Margin = New Padding(4, 0, 4, 10)
        lblAdobeMod.Name = "lblAdobeMod"
        lblAdobeMod.Size = New Size(322, 37)
        lblAdobeMod.TabIndex = 0
        lblAdobeMod.Text = "Mod vizualizator Adobe"
        lblAdobeMod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeMod
        '
        cboAdobeMod.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeMod.CornerRadius = 4
        cboAdobeMod.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeMod.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeMod.FlatStyle = FlatStyle.Flat
        cboAdobeMod.ItemHeight = 31
        cboAdobeMod.Location = New Point(358, 8)
        cboAdobeMod.Margin = New Padding(4, 0, 4, 10)
        cboAdobeMod.Name = "cboAdobeMod"
        cboAdobeMod.Size = New Size(472, 37)
        cboAdobeMod.TabIndex = 1
        tips.SetToolTipHeader(cboAdobeMod, "Profilul de găzduire Adobe")
        tips.SetToolTipText(cboAdobeMod, "Automat: K-BOT recunoaște singur interfața Adobe (modernă / clasică)." & vbLf & "Modern / Clasic forțează rețeta, iar jurnalul spune dacă arborele o contrazice.")
        '
        ' lblAdobeInst
        '
        lblAdobeInst.AutoSize = True
        lblAdobeInst.Dock = DockStyle.Fill
        lblAdobeInst.Location = New Point(28, 55)
        lblAdobeInst.Margin = New Padding(4, 0, 4, 10)
        lblAdobeInst.Name = "lblAdobeInst"
        lblAdobeInst.Size = New Size(322, 37)
        lblAdobeInst.TabIndex = 2
        lblAdobeInst.Text = "Instanță nouă Adobe (/n)"
        lblAdobeInst.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeInst
        '
        cboAdobeInst.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeInst.CornerRadius = 4
        cboAdobeInst.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeInst.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeInst.FlatStyle = FlatStyle.Flat
        cboAdobeInst.ItemHeight = 31
        cboAdobeInst.Location = New Point(358, 55)
        cboAdobeInst.Margin = New Padding(4, 0, 4, 10)
        cboAdobeInst.Name = "cboAdobeInst"
        cboAdobeInst.Size = New Size(472, 37)
        cboAdobeInst.TabIndex = 3
        tips.SetToolTipHeader(cboAdobeInst, "Comutatorul /n")
        tips.SetToolTipText(cboAdobeInst, "Da: Adobe pornește un proces nou, al K-BOT." & vbLf & "Nu: Adobe poate preda documentul unei instanțe deja deschise de tine." & vbLf & "Automat: decide profilul.")
        '
        ' lblAdobeDetach
        '
        lblAdobeDetach.AutoSize = True
        lblAdobeDetach.Dock = DockStyle.Fill
        lblAdobeDetach.Location = New Point(28, 102)
        lblAdobeDetach.Margin = New Padding(4, 0, 4, 10)
        lblAdobeDetach.Name = "lblAdobeDetach"
        lblAdobeDetach.Size = New Size(322, 37)
        lblAdobeDetach.TabIndex = 4
        lblAdobeDetach.Text = "La schimbarea documentului"
        lblAdobeDetach.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cboAdobeDetach
        '
        cboAdobeDetach.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cboAdobeDetach.CornerRadius = 4
        cboAdobeDetach.DrawMode = DrawMode.OwnerDrawFixed
        cboAdobeDetach.DropDownStyle = ComboBoxStyle.DropDownList
        cboAdobeDetach.FlatStyle = FlatStyle.Flat
        cboAdobeDetach.ItemHeight = 31
        cboAdobeDetach.Location = New Point(358, 102)
        cboAdobeDetach.Margin = New Padding(4, 0, 4, 10)
        cboAdobeDetach.Name = "cboAdobeDetach"
        cboAdobeDetach.Size = New Size(472, 37)
        cboAdobeDetach.TabIndex = 5
        tips.SetToolTipHeader(cboAdobeDetach, "Cum se eliberează fereastra Adobe")
        tips.SetToolTipText(cboAdobeDetach, "A: oprește procesul pornit de K-BOT — determinist." & vbLf & "B: închide doar fereastra și lasă procesul cald, deci următorul document pornește mai repede.")
        '
        ' chkAdobePopup
        '
        chkAdobePopup.AutoSize = True
        tlyCampuri.SetColumnSpan(chkAdobePopup, 2)
        chkAdobePopup.Location = New Point(28, 149)
        chkAdobePopup.Margin = New Padding(4, 0, 4, 10)
        chkAdobePopup.Name = "chkAdobePopup"
        chkAdobePopup.Size = New Size(520, 29)
        chkAdobePopup.TabIndex = 6
        chkAdobePopup.Text = "Ascunde fereastra plutitoare a Adobe (insigna de peste document)"
        tips.SetToolTipHeader(chkAdobePopup, "Fereastra plutitoare")
        tips.SetToolTipText(chkAdobePopup, "Un supraveghetor mătură ecranul la 500 ms și ascunde ferestrele AVL_AVPopup ale Adobe cât timp documentul e afișat.")
        chkAdobePopup.UseVisualStyleBackColor = True
        '
        ' tlySubsol
        '
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlySubsol.Controls.Add(btnRenunta, 1, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 340)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(20, 0, 20, 0)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(858, 78)
        tlySubsol.TabIndex = 3
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(452, 12)
        btnRenunta.Margin = New Padding(4, 12, 4, 12)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(172, 54)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fără să schimbe nimic.")
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Fill
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Font = New Font("Segoe UI Semibold", 9F)
        btnSalveaza.Location = New Point(632, 12)
        btnSalveaza.Margin = New Padding(4, 12, 4, 12)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(202, 54)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează"
        btnSalveaza.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(btnSalveaza, "Salvează")
        tips.SetToolTipText(btnSalveaza, "Scrie cele patru setări (kbot_paths.json și app_settings.json) și închide.")
        '
        ' AdobeGazduireForm
        '
        AcceptButton = btnSalveaza
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(860, 420)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "AdobeGazduireForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
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
    Friend WithEvents lblAdobeMod As Label
    Friend WithEvents cboAdobeMod As KBotComboBox
    Friend WithEvents lblAdobeInst As Label
    Friend WithEvents cboAdobeInst As KBotComboBox
    Friend WithEvents lblAdobeDetach As Label
    Friend WithEvents cboAdobeDetach As KBotComboBox
    Friend WithEvents chkAdobePopup As CheckBox
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
