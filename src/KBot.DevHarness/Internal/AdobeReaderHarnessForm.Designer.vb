<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdobeReaderHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' Adobe embed test bench (slice 0023 extends the original switches bench): a host panel
    ' (Fill) that reparents the Adobe Reader/Acrobat DC window, and a left strip exposing every
    ' lever the operator can judge: /A open parameters (document chrome), the child-window probe,
    ' geometry clipping, direct child hiding, keyboard shortcuts, HKCU preferences and HKLM
    ' policies (the four candidate levers against the right-hand Tools pane). House rule: ALL
    ' WinForms controls are declared here, in .Designer.vb.

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

    ' §3.A Diagnostic — the child window probe.
    Friend WithEvents lblSecProbe As System.Windows.Forms.Label
    Friend WithEvents btnProbe As System.Windows.Forms.Button

    ' §3.B Decupare — geometry clipping (live, no relaunch).
    Friend WithEvents lblSecClip As System.Windows.Forms.Label
    Friend WithEvents chkClip As System.Windows.Forms.CheckBox
    Friend WithEvents lblClipRight As System.Windows.Forms.Label
    Friend WithEvents numClipRight As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblClipTop As System.Windows.Forms.Label
    Friend WithEvents numClipTop As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnClipAuto As System.Windows.Forms.Button

    ' §3.C Ferestre copil — hide a child window directly.
    Friend WithEvents lblSecChildren As System.Windows.Forms.Label
    Friend WithEvents lstChildren As System.Windows.Forms.ListBox
    Friend WithEvents btnHideChild As System.Windows.Forms.Button
    Friend WithEvents btnShowChild As System.Windows.Forms.Button
    Friend WithEvents btnShowAllChildren As System.Windows.Forms.Button

    ' §3.D Scurtături — keyboard toggles (experimental).
    Friend WithEvents lblSecKeys As System.Windows.Forms.Label
    Friend WithEvents btnSendShiftF4 As System.Windows.Forms.Button
    Friend WithEvents btnSendF4 As System.Windows.Forms.Button

    ' §3.E Preferințe Adobe (utilizator) — HKCU, no elevation.
    Friend WithEvents lblSecUser As System.Windows.Forms.Label
    Friend WithEvents lblHive As System.Windows.Forms.Label
    Friend WithEvents cboHive As System.Windows.Forms.ComboBox
    Friend WithEvents chkExpandRhp As System.Windows.Forms.CheckBox
    Friend WithEvents chkRhpSticky As System.Windows.Forms.CheckBox
    Friend WithEvents chkRhpCollapsed As System.Windows.Forms.CheckBox
    Friend WithEvents chkClassicViewer As System.Windows.Forms.CheckBox
    Friend WithEvents btnApplyUser As System.Windows.Forms.Button
    Friend WithEvents btnRestoreUser As System.Windows.Forms.Button
    Friend WithEvents chkRestoreOnClose As System.Windows.Forms.CheckBox

    ' §3.F Politici Adobe (mașină) — HKLM via elevated reg.exe import.
    Friend WithEvents lblSecMachine As System.Windows.Forms.Label
    Friend WithEvents cboProduct As System.Windows.Forms.ComboBox
    Friend WithEvents chkSuppressUpsell As System.Windows.Forms.CheckBox
    Friend WithEvents chkDisableServices As System.Windows.Forms.CheckBox
    Friend WithEvents btnApplyMachine As System.Windows.Forms.Button
    Friend WithEvents btnRevertMachine As System.Windows.Forms.Button

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
        Me.lblSecProbe = New System.Windows.Forms.Label()
        Me.btnProbe = New System.Windows.Forms.Button()
        Me.lblSecClip = New System.Windows.Forms.Label()
        Me.chkClip = New System.Windows.Forms.CheckBox()
        Me.lblClipRight = New System.Windows.Forms.Label()
        Me.numClipRight = New System.Windows.Forms.NumericUpDown()
        Me.lblClipTop = New System.Windows.Forms.Label()
        Me.numClipTop = New System.Windows.Forms.NumericUpDown()
        Me.btnClipAuto = New System.Windows.Forms.Button()
        Me.lblSecChildren = New System.Windows.Forms.Label()
        Me.lstChildren = New System.Windows.Forms.ListBox()
        Me.btnHideChild = New System.Windows.Forms.Button()
        Me.btnShowChild = New System.Windows.Forms.Button()
        Me.btnShowAllChildren = New System.Windows.Forms.Button()
        Me.lblSecKeys = New System.Windows.Forms.Label()
        Me.btnSendShiftF4 = New System.Windows.Forms.Button()
        Me.btnSendF4 = New System.Windows.Forms.Button()
        Me.lblSecUser = New System.Windows.Forms.Label()
        Me.lblHive = New System.Windows.Forms.Label()
        Me.cboHive = New System.Windows.Forms.ComboBox()
        Me.chkExpandRhp = New System.Windows.Forms.CheckBox()
        Me.chkRhpSticky = New System.Windows.Forms.CheckBox()
        Me.chkRhpCollapsed = New System.Windows.Forms.CheckBox()
        Me.chkClassicViewer = New System.Windows.Forms.CheckBox()
        Me.btnApplyUser = New System.Windows.Forms.Button()
        Me.btnRestoreUser = New System.Windows.Forms.Button()
        Me.chkRestoreOnClose = New System.Windows.Forms.CheckBox()
        Me.lblSecMachine = New System.Windows.Forms.Label()
        Me.cboProduct = New System.Windows.Forms.ComboBox()
        Me.chkSuppressUpsell = New System.Windows.Forms.CheckBox()
        Me.chkDisableServices = New System.Windows.Forms.CheckBox()
        Me.btnApplyMachine = New System.Windows.Forms.Button()
        Me.btnRevertMachine = New System.Windows.Forms.Button()
        Me.lblSecCmd = New System.Windows.Forms.Label()
        Me.lblCmd = New System.Windows.Forms.Label()
        Me.lblSecStatus = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.flowLeft.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        CType(Me.numClipRight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numClipTop, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.flowLeft.Width = 360
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
        Me.flowLeft.Controls.Add(Me.lblSecProbe)
        Me.flowLeft.Controls.Add(Me.btnProbe)
        Me.flowLeft.Controls.Add(Me.lblSecClip)
        Me.flowLeft.Controls.Add(Me.chkClip)
        Me.flowLeft.Controls.Add(Me.lblClipRight)
        Me.flowLeft.Controls.Add(Me.numClipRight)
        Me.flowLeft.Controls.Add(Me.lblClipTop)
        Me.flowLeft.Controls.Add(Me.numClipTop)
        Me.flowLeft.Controls.Add(Me.btnClipAuto)
        Me.flowLeft.Controls.Add(Me.lblSecChildren)
        Me.flowLeft.Controls.Add(Me.lstChildren)
        Me.flowLeft.Controls.Add(Me.btnHideChild)
        Me.flowLeft.Controls.Add(Me.btnShowChild)
        Me.flowLeft.Controls.Add(Me.btnShowAllChildren)
        Me.flowLeft.Controls.Add(Me.lblSecKeys)
        Me.flowLeft.Controls.Add(Me.btnSendShiftF4)
        Me.flowLeft.Controls.Add(Me.btnSendF4)
        Me.flowLeft.Controls.Add(Me.lblSecUser)
        Me.flowLeft.Controls.Add(Me.lblHive)
        Me.flowLeft.Controls.Add(Me.cboHive)
        Me.flowLeft.Controls.Add(Me.chkExpandRhp)
        Me.flowLeft.Controls.Add(Me.chkRhpSticky)
        Me.flowLeft.Controls.Add(Me.chkRhpCollapsed)
        Me.flowLeft.Controls.Add(Me.chkClassicViewer)
        Me.flowLeft.Controls.Add(Me.btnApplyUser)
        Me.flowLeft.Controls.Add(Me.btnRestoreUser)
        Me.flowLeft.Controls.Add(Me.chkRestoreOnClose)
        Me.flowLeft.Controls.Add(Me.lblSecMachine)
        Me.flowLeft.Controls.Add(Me.cboProduct)
        Me.flowLeft.Controls.Add(Me.chkSuppressUpsell)
        Me.flowLeft.Controls.Add(Me.chkDisableServices)
        Me.flowLeft.Controls.Add(Me.btnApplyMachine)
        Me.flowLeft.Controls.Add(Me.btnRevertMachine)
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
        Me.lblFile.AutoSize = True : Me.lblFile.MaximumSize = New System.Drawing.Size(330, 0) : Me.lblFile.Text = "<niciun PDF>" : Me.lblFile.Name = "lblFile"
        Me.btnRelaunch.AutoSize = True : Me.btnRelaunch.Text = "Reîncorporează / redesenează" : Me.btnRelaunch.Margin = New System.Windows.Forms.Padding(3, 6, 3, 3) : Me.btnRelaunch.Name = "btnRelaunch" : Me.btnRelaunch.UseVisualStyleBackColor = True
        '
        ' §3.A Diagnostic
        Me.lblSecProbe.AutoSize = True : Me.lblSecProbe.Text = "—— Diagnostic ——" : Me.lblSecProbe.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecProbe.Name = "lblSecProbe"
        Me.btnProbe.AutoSize = True : Me.btnProbe.Text = "Arborele de ferestre copil" : Me.btnProbe.Name = "btnProbe" : Me.btnProbe.UseVisualStyleBackColor = True
        '
        ' §3.B Decupare
        Me.lblSecClip.AutoSize = True : Me.lblSecClip.Text = "—— Decupare ——" : Me.lblSecClip.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecClip.Name = "lblSecClip"
        Me.chkClip.AutoSize = True : Me.chkClip.Text = "Decupare activă" : Me.chkClip.Name = "chkClip"
        Me.lblClipRight.AutoSize = True : Me.lblClipRight.Text = "Decupare dreapta (px)" : Me.lblClipRight.Name = "lblClipRight"
        Me.numClipRight.Minimum = 0D : Me.numClipRight.Maximum = 800D : Me.numClipRight.Increment = 10D : Me.numClipRight.Value = 0D : Me.numClipRight.Width = 120 : Me.numClipRight.Name = "numClipRight"
        Me.lblClipTop.AutoSize = True : Me.lblClipTop.Text = "Decupare sus (px)" : Me.lblClipTop.Name = "lblClipTop"
        Me.numClipTop.Minimum = 0D : Me.numClipTop.Maximum = 400D : Me.numClipTop.Increment = 10D : Me.numClipTop.Value = 0D : Me.numClipTop.Width = 120 : Me.numClipTop.Name = "numClipTop"
        Me.btnClipAuto.AutoSize = True : Me.btnClipAuto.Text = "Măsoară din probă" : Me.btnClipAuto.Enabled = False : Me.btnClipAuto.Name = "btnClipAuto" : Me.btnClipAuto.UseVisualStyleBackColor = True
        '
        ' §3.C Ferestre copil
        Me.lblSecChildren.AutoSize = True : Me.lblSecChildren.Text = "—— Ferestre copil ——" : Me.lblSecChildren.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecChildren.Name = "lblSecChildren"
        Me.lstChildren.Width = 336 : Me.lstChildren.Height = 140 : Me.lstChildren.IntegralHeight = False : Me.lstChildren.Name = "lstChildren"
        Me.btnHideChild.AutoSize = True : Me.btnHideChild.Text = "Ascunde fereastra selectată" : Me.btnHideChild.Name = "btnHideChild" : Me.btnHideChild.UseVisualStyleBackColor = True
        Me.btnShowChild.AutoSize = True : Me.btnShowChild.Text = "Arată fereastra selectată" : Me.btnShowChild.Name = "btnShowChild" : Me.btnShowChild.UseVisualStyleBackColor = True
        Me.btnShowAllChildren.AutoSize = True : Me.btnShowAllChildren.Text = "Restaurează toate" : Me.btnShowAllChildren.Name = "btnShowAllChildren" : Me.btnShowAllChildren.UseVisualStyleBackColor = True
        '
        ' §3.D Scurtături
        Me.lblSecKeys.AutoSize = True : Me.lblSecKeys.Text = "—— Scurtături (experimental) ——" : Me.lblSecKeys.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecKeys.Name = "lblSecKeys"
        Me.btnSendShiftF4.AutoSize = True : Me.btnSendShiftF4.Text = "Trimite Shift+F4 (comută panoul de instrumente)" : Me.btnSendShiftF4.Name = "btnSendShiftF4" : Me.btnSendShiftF4.UseVisualStyleBackColor = True
        Me.btnSendF4.AutoSize = True : Me.btnSendF4.Text = "Trimite F4 (comută panoul de navigare)" : Me.btnSendF4.Name = "btnSendF4" : Me.btnSendF4.UseVisualStyleBackColor = True
        '
        ' §3.E Preferințe Adobe (utilizator)
        Me.lblSecUser.AutoSize = True : Me.lblSecUser.Text = "—— Preferințe Adobe (utilizator, HKCU) ——" : Me.lblSecUser.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecUser.Name = "lblSecUser"
        Me.lblHive.AutoSize = True : Me.lblHive.MaximumSize = New System.Drawing.Size(330, 0) : Me.lblHive.Text = "" : Me.lblHive.Name = "lblHive"
        Me.cboHive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboHive.Width = 336 : Me.cboHive.DropDownWidth = 420 : Me.cboHive.Name = "cboHive"
        Me.chkExpandRhp.AutoSize = True : Me.chkExpandRhp.Text = "bExpandRHPInViewer = 0" : Me.chkExpandRhp.Name = "chkExpandRhp"
        Me.chkRhpSticky.AutoSize = True : Me.chkRhpSticky.Text = "bRHPSticky = 1" : Me.chkRhpSticky.Name = "chkRhpSticky"
        Me.chkRhpCollapsed.AutoSize = True : Me.chkRhpCollapsed.Text = "aDefaultRHPViewMode_L = Collapsed" : Me.chkRhpCollapsed.Name = "chkRhpCollapsed"
        Me.chkClassicViewer.AutoSize = True : Me.chkClassicViewer.Text = "bEnableAv2 = 0 (interfața clasică)" : Me.chkClassicViewer.Name = "chkClassicViewer"
        Me.btnApplyUser.AutoSize = True : Me.btnApplyUser.Text = "Aplică și repornește Adobe" : Me.btnApplyUser.Name = "btnApplyUser" : Me.btnApplyUser.UseVisualStyleBackColor = True
        Me.btnRestoreUser.AutoSize = True : Me.btnRestoreUser.Text = "Restaurează valorile originale" : Me.btnRestoreUser.Name = "btnRestoreUser" : Me.btnRestoreUser.UseVisualStyleBackColor = True
        Me.chkRestoreOnClose.AutoSize = True : Me.chkRestoreOnClose.Checked = True : Me.chkRestoreOnClose.CheckState = System.Windows.Forms.CheckState.Checked : Me.chkRestoreOnClose.Text = "Restaurează la închiderea bancului" : Me.chkRestoreOnClose.Name = "chkRestoreOnClose"
        '
        ' §3.F Politici Adobe (mașină)
        Me.lblSecMachine.AutoSize = True : Me.lblSecMachine.Text = "—— Politici Adobe (mașină, HKLM) ——" : Me.lblSecMachine.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecMachine.Name = "lblSecMachine"
        Me.cboProduct.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboProduct.Width = 336 : Me.cboProduct.Name = "cboProduct"
        Me.chkSuppressUpsell.AutoSize = True : Me.chkSuppressUpsell.Text = "bAcroSuppressUpsell = 1" : Me.chkSuppressUpsell.Name = "chkSuppressUpsell"
        Me.chkDisableServices.AutoSize = True : Me.chkDisableServices.Text = "cServices\bToggleAdobeDocumentServices = 1" : Me.chkDisableServices.Name = "chkDisableServices"
        Me.btnApplyMachine.AutoSize = True : Me.btnApplyMachine.Text = "Aplică (cere elevare)" : Me.btnApplyMachine.Name = "btnApplyMachine" : Me.btnApplyMachine.UseVisualStyleBackColor = True
        Me.btnRevertMachine.AutoSize = True : Me.btnRevertMachine.Text = "Revocă (cere elevare)" : Me.btnRevertMachine.Name = "btnRevertMachine" : Me.btnRevertMachine.UseVisualStyleBackColor = True
        '
        Me.lblSecCmd.AutoSize = True : Me.lblSecCmd.Text = "—— Linie de comandă ——" : Me.lblSecCmd.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecCmd.Name = "lblSecCmd"
        Me.lblCmd.AutoSize = True : Me.lblCmd.MaximumSize = New System.Drawing.Size(330, 0) : Me.lblCmd.Font = New System.Drawing.Font("Consolas", 8.25!) : Me.lblCmd.Text = "" : Me.lblCmd.Name = "lblCmd"
        '
        Me.lblSecStatus.AutoSize = True : Me.lblSecStatus.Text = "—— Stare ——" : Me.lblSecStatus.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecStatus.Name = "lblSecStatus"
        Me.lblStatus.AutoSize = True : Me.lblStatus.MaximumSize = New System.Drawing.Size(330, 0) : Me.lblStatus.Text = "" : Me.lblStatus.Name = "lblStatus"
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
        Me.ClientSize = New System.Drawing.Size(1240, 780)
        ' Dock order (house rule): Fill first, then Left, then Bottom (so the bottom band spans
        ' the full width under the left strip).
        Me.Controls.Add(Me.pnlHost)
        Me.Controls.Add(Me.flowLeft)
        Me.Controls.Add(Me.pnlButtons)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Adobe Reader DC — încorporare + switch-uri (bare ascunse)"
        Me.Name = "AdobeReaderHarnessForm"
        Me.flowLeft.ResumeLayout(False) : Me.flowLeft.PerformLayout()
        Me.pnlButtons.ResumeLayout(False) : Me.pnlButtons.PerformLayout()
        CType(Me.numClipRight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numClipTop, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False) : Me.PerformLayout()
    End Sub
End Class
