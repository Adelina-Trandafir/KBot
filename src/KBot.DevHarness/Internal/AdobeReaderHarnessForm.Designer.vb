<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdobeReaderHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' Banc de probă „Adobe încorporat": un panou-gazdă (Fill) în care se reparentează fereastra
    ' Adobe Reader/Acrobat DC, o bandă stânga cu comutatoare pentru fiecare switch de linie de
    ' comandă / parametru de deschidere care ascunde chrome-ul (bare de instrumente/panouri), și
    ' butoanele Pass/Fail (verdict uman). Regula casei: toate controalele WinForms se declară aici.

    Friend WithEvents pnlHost As System.Windows.Forms.Panel

    Friend WithEvents flowLeft As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblSecLaunch As System.Windows.Forms.Label
    Friend WithEvents chkNewInstance As System.Windows.Forms.CheckBox
    Friend WithEvents chkNoSplash As System.Windows.Forms.CheckBox
    Friend WithEvents lblSecChrome As System.Windows.Forms.Label
    Friend WithEvents chkToolbar As System.Windows.Forms.CheckBox
    Friend WithEvents chkNavpanes As System.Windows.Forms.CheckBox
    Friend WithEvents chkStatusbar As System.Windows.Forms.CheckBox
    Friend WithEvents chkMessages As System.Windows.Forms.CheckBox
    Friend WithEvents chkScrollbar As System.Windows.Forms.CheckBox
    Friend WithEvents chkPagemodeNone As System.Windows.Forms.CheckBox
    Friend WithEvents lblSecFile As System.Windows.Forms.Label
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents lblFile As System.Windows.Forms.Label
    Friend WithEvents btnRelaunch As System.Windows.Forms.Button
    Friend WithEvents lblSecCmd As System.Windows.Forms.Label
    Friend WithEvents lblCmd As System.Windows.Forms.Label
    Friend WithEvents lblSecStatus As System.Windows.Forms.Label
    Friend WithEvents lblStatus As System.Windows.Forms.Label

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlHost = New System.Windows.Forms.Panel()
        Me.flowLeft = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblSecLaunch = New System.Windows.Forms.Label()
        Me.chkNewInstance = New System.Windows.Forms.CheckBox()
        Me.chkNoSplash = New System.Windows.Forms.CheckBox()
        Me.lblSecChrome = New System.Windows.Forms.Label()
        Me.chkToolbar = New System.Windows.Forms.CheckBox()
        Me.chkNavpanes = New System.Windows.Forms.CheckBox()
        Me.chkStatusbar = New System.Windows.Forms.CheckBox()
        Me.chkMessages = New System.Windows.Forms.CheckBox()
        Me.chkScrollbar = New System.Windows.Forms.CheckBox()
        Me.chkPagemodeNone = New System.Windows.Forms.CheckBox()
        Me.lblSecFile = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.lblFile = New System.Windows.Forms.Label()
        Me.btnRelaunch = New System.Windows.Forms.Button()
        Me.lblSecCmd = New System.Windows.Forms.Label()
        Me.lblCmd = New System.Windows.Forms.Label()
        Me.lblSecStatus = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.flowLeft.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHost — gazda ferestrei Adobe reparentate
        '
        Me.pnlHost.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlHost.Name = "pnlHost"
        '
        'flowLeft — banda de comutatoare (derulabilă)
        '
        Me.flowLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.flowLeft.Width = 320
        Me.flowLeft.AutoScroll = True
        Me.flowLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.flowLeft.WrapContents = False
        Me.flowLeft.Padding = New System.Windows.Forms.Padding(8)
        Me.flowLeft.Name = "flowLeft"
        Me.flowLeft.Controls.Add(Me.lblSecLaunch)
        Me.flowLeft.Controls.Add(Me.chkNewInstance)
        Me.flowLeft.Controls.Add(Me.chkNoSplash)
        Me.flowLeft.Controls.Add(Me.lblSecChrome)
        Me.flowLeft.Controls.Add(Me.chkToolbar)
        Me.flowLeft.Controls.Add(Me.chkNavpanes)
        Me.flowLeft.Controls.Add(Me.chkStatusbar)
        Me.flowLeft.Controls.Add(Me.chkMessages)
        Me.flowLeft.Controls.Add(Me.chkScrollbar)
        Me.flowLeft.Controls.Add(Me.chkPagemodeNone)
        Me.flowLeft.Controls.Add(Me.lblSecFile)
        Me.flowLeft.Controls.Add(Me.btnBrowse)
        Me.flowLeft.Controls.Add(Me.lblFile)
        Me.flowLeft.Controls.Add(Me.btnRelaunch)
        Me.flowLeft.Controls.Add(Me.lblSecCmd)
        Me.flowLeft.Controls.Add(Me.lblCmd)
        Me.flowLeft.Controls.Add(Me.lblSecStatus)
        Me.flowLeft.Controls.Add(Me.lblStatus)
        '
        Me.lblSecLaunch.AutoSize = True : Me.lblSecLaunch.Text = "—— Lansare ——" : Me.lblSecLaunch.Name = "lblSecLaunch"
        Me.chkNewInstance.AutoSize = True : Me.chkNewInstance.Checked = True : Me.chkNewInstance.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkNewInstance.Text = "/n  — instanță nouă (recomandat pt. încorporare)" : Me.chkNewInstance.Name = "chkNewInstance"
        Me.chkNoSplash.AutoSize = True : Me.chkNoSplash.Checked = True : Me.chkNoSplash.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkNoSplash.Text = "/s  — fără ecran de întâmpinare" : Me.chkNoSplash.Name = "chkNoSplash"
        '
        Me.lblSecChrome.AutoSize = True : Me.lblSecChrome.Text = "—— Chrome ascuns (parametri /A) ——" : Me.lblSecChrome.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecChrome.Name = "lblSecChrome"
        Me.chkToolbar.AutoSize = True : Me.chkToolbar.Checked = True : Me.chkToolbar.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkToolbar.Text = "toolbar=0  — ascunde bara de instrumente" : Me.chkToolbar.Name = "chkToolbar"
        Me.chkNavpanes.AutoSize = True : Me.chkNavpanes.Checked = True : Me.chkNavpanes.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkNavpanes.Text = "navpanes=0  — ascunde panourile de navigare" : Me.chkNavpanes.Name = "chkNavpanes"
        Me.chkStatusbar.AutoSize = True : Me.chkStatusbar.Checked = True : Me.chkStatusbar.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkStatusbar.Text = "statusbar=0  — ascunde bara de stare" : Me.chkStatusbar.Name = "chkStatusbar"
        Me.chkMessages.AutoSize = True : Me.chkMessages.Checked = True : Me.chkMessages.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkMessages.Text = "messages=0  — ascunde bara de mesaje" : Me.chkMessages.Name = "chkMessages"
        Me.chkScrollbar.AutoSize = True : Me.chkScrollbar.Checked = True : Me.chkScrollbar.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkScrollbar.Text = "scrollbar=0  — ascunde barele de derulare" : Me.chkScrollbar.Name = "chkScrollbar"
        Me.chkPagemodeNone.AutoSize = True : Me.chkPagemodeNone.Checked = True : Me.chkPagemodeNone.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkPagemodeNone.Text = "pagemode=none  — fără panou lateral deschis" : Me.chkPagemodeNone.Name = "chkPagemodeNone"
        '
        Me.lblSecFile.AutoSize = True : Me.lblSecFile.Text = "—— Document ——" : Me.lblSecFile.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecFile.Name = "lblSecFile"
        Me.btnBrowse.AutoSize = True : Me.btnBrowse.Text = "Deschide PDF…" : Me.btnBrowse.Name = "btnBrowse" : Me.btnBrowse.UseVisualStyleBackColor = True
        Me.lblFile.AutoSize = True : Me.lblFile.MaximumSize = New System.Drawing.Size(290, 0) : Me.lblFile.Text = "<niciun PDF>" : Me.lblFile.Name = "lblFile"
        Me.btnRelaunch.AutoSize = True : Me.btnRelaunch.Text = "Reîncorporează / redesenează" : Me.btnRelaunch.Margin = New System.Windows.Forms.Padding(3, 6, 3, 3) : Me.btnRelaunch.Name = "btnRelaunch" : Me.btnRelaunch.UseVisualStyleBackColor = True
        '
        Me.lblSecCmd.AutoSize = True : Me.lblSecCmd.Text = "—— Linie de comandă ——" : Me.lblSecCmd.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecCmd.Name = "lblSecCmd"
        Me.lblCmd.AutoSize = True : Me.lblCmd.MaximumSize = New System.Drawing.Size(290, 0) : Me.lblCmd.Font = New System.Drawing.Font("Consolas", 8.25!) : Me.lblCmd.Text = "" : Me.lblCmd.Name = "lblCmd"
        '
        Me.lblSecStatus.AutoSize = True : Me.lblSecStatus.Text = "—— Stare ——" : Me.lblSecStatus.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecStatus.Name = "lblSecStatus"
        Me.lblStatus.AutoSize = True : Me.lblStatus.MaximumSize = New System.Drawing.Size(290, 0) : Me.lblStatus.Text = "" : Me.lblStatus.Name = "lblStatus"
        '
        'pnlButtons — verdictul uman
        '
        Me.pnlButtons.AutoSize = True
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Name = "pnlButtons"
        '
        Me.btnFail.AutoSize = True : Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel : Me.btnFail.Text = "Fail" : Me.btnFail.Name = "btnFail" : Me.btnFail.UseVisualStyleBackColor = True
        Me.btnPass.AutoSize = True : Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK : Me.btnPass.Text = "Pass" : Me.btnPass.Name = "btnPass" : Me.btnPass.UseVisualStyleBackColor = True
        '
        'AdobeReaderHarnessForm
        '
        Me.AcceptButton = Me.btnPass
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1180, 760)
        ' Ordine de dock (regula casei): Fill întâi, apoi Left, apoi Bottom (ca marginea de jos
        ' să treacă peste banda din stânga).
        Me.Controls.Add(Me.pnlHost)
        Me.Controls.Add(Me.flowLeft)
        Me.Controls.Add(Me.pnlButtons)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Adobe Reader DC — încorporare + switch-uri (bare ascunse)"
        Me.Name = "AdobeReaderHarnessForm"
        Me.flowLeft.ResumeLayout(False) : Me.flowLeft.PerformLayout()
        Me.pnlButtons.ResumeLayout(False) : Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False) : Me.PerformLayout()
    End Sub
End Class
