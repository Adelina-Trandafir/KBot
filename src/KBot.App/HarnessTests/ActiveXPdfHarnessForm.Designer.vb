#If DEBUG Then
' Bench for slice 0078-15: the AcroPDF ActiveX control alone, loading a PDF the operator picks.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ActiveXPdfHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBar As Panel
    Friend WithEvents pnlOptions As FlowLayoutPanel
    Friend WithEvents btnOpen As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents btnSeen As Button
    Friend WithEvents btnLogViewer As Button
    Friend WithEvents btnLanes As Button
    Friend WithEvents chkSrc As CheckBox
    Friend WithEvents chkDetailed As CheckBox
    Friend WithEvents chkSaveTrap As CheckBox
    Friend WithEvents btnCopyFolder As Button
    Friend WithEvents btnSaveNow As Button
    Friend WithEvents chkAutoSave As CheckBox
    Friend WithEvents lblFile As Label
    Friend WithEvents pnlHost As Panel
    Friend WithEvents pnlVerdict As FlowLayoutPanel
    Friend WithEvents btnPass As Button
    Friend WithEvents btnFail As Button
    Friend WithEvents dlgOpen As OpenFileDialog
    Friend WithEvents tmrSaved As Timer

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' The viewer (and the control it hosts) goes before the controls do.
            If disposing Then ShutDownBench()
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlBar = New Panel()
        pnlOptions = New FlowLayoutPanel()
        btnOpen = New Button()
        btnClose = New Button()
        btnSeen = New Button()
        btnLogViewer = New Button()
        btnLanes = New Button()
        chkSrc = New CheckBox()
        chkDetailed = New CheckBox()
        chkSaveTrap = New CheckBox()
        btnCopyFolder = New Button()
        btnSaveNow = New Button()
        chkAutoSave = New CheckBox()
        lblFile = New Label()
        pnlHost = New Panel()
        pnlVerdict = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        dlgOpen = New OpenFileDialog()
        tmrSaved = New Timer(components)
        pnlBar.SuspendLayout()
        pnlOptions.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBar -- one strip: the label fills the left, every button is docked to the RIGHT, one after the other.
        ' Children in REVERSE dock order: the label (Fill) first, then the buttons, the LAST added being the rightmost.
        '
        pnlBar.Controls.Add(lblFile)
        pnlBar.Controls.Add(btnOpen)
        pnlBar.Controls.Add(btnClose)
        pnlBar.Controls.Add(btnSeen)
        pnlBar.Controls.Add(btnLogViewer)
        pnlBar.Controls.Add(btnLanes)
        pnlBar.Controls.Add(btnCopyFolder)
        pnlBar.Controls.Add(btnSaveNow)
        pnlBar.Dock = DockStyle.Top
        pnlBar.Height = 40
        pnlBar.Name = "pnlBar"
        pnlBar.Padding = New Padding(8, 4, 8, 4)
        pnlBar.TabIndex = 0
        '
        ' pnlOptions -- the check boxes, in a row of their own under the buttons
        '
        pnlOptions.Controls.Add(chkSrc)
        pnlOptions.Controls.Add(chkDetailed)
        pnlOptions.Controls.Add(chkSaveTrap)
        pnlOptions.Controls.Add(chkAutoSave)
        pnlOptions.AutoSize = True
        pnlOptions.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlOptions.Dock = DockStyle.Top
        pnlOptions.Name = "pnlOptions"
        pnlOptions.Padding = New Padding(8, 0, 8, 4)
        pnlOptions.TabIndex = 3
        '
        ' btnOpen -- the PDF itself, no copy
        '
        btnOpen.AutoSize = True
        btnOpen.Dock = DockStyle.Right
        btnOpen.Margin = New Padding(3)
        btnOpen.Name = "btnOpen"
        btnOpen.Padding = New Padding(8, 2, 8, 2)
        btnOpen.TabIndex = 0
        btnOpen.Text = "Deschide un PDF…"
        btnOpen.UseVisualStyleBackColor = True
        '
        ' btnClose -- releases the control (the next PDF gets a new one)
        '
        btnClose.AutoSize = True
        btnClose.Dock = DockStyle.Right
        btnClose.Margin = New Padding(3)
        btnClose.Name = "btnClose"
        btnClose.Padding = New Padding(8, 2, 8, 2)
        btnClose.TabIndex = 1
        btnClose.Text = "Închide documentul"
        btnClose.UseVisualStyleBackColor = True
        '
        ' btnSeen -- the moment the document LOOKS loaded, into activex_check.log
        '
        btnSeen.AutoSize = True
        btnSeen.Dock = DockStyle.Right
        btnSeen.Margin = New Padding(3)
        btnSeen.Name = "btnSeen"
        btnSeen.Padding = New Padding(8, 2, 8, 2)
        btnSeen.TabIndex = 2
        btnSeen.Text = "Îl văd încărcat"
        btnSeen.UseVisualStyleBackColor = True
        '
        ' btnLogViewer
        '
        btnLogViewer.AutoSize = True
        btnLogViewer.Dock = DockStyle.Right
        btnLogViewer.Margin = New Padding(3)
        btnLogViewer.Name = "btnLogViewer"
        btnLogViewer.Padding = New Padding(8, 2, 8, 2)
        btnLogViewer.TabIndex = 3
        btnLogViewer.Text = "Jurnalul ActiveX…"
        btnLogViewer.UseVisualStyleBackColor = True
        '
        ' btnLanes -- the newest load of activex_check.log as lanes (ActivexLaneForm): a root of the tree is a lane, a leaf a change
        '
        btnLanes.AutoSize = True
        btnLanes.Dock = DockStyle.Right
        btnLanes.Margin = New Padding(3)
        btnLanes.Name = "btnLanes"
        btnLanes.Padding = New Padding(8, 2, 8, 2)
        btnLanes.TabIndex = 11
        btnLanes.Text = "Jurnalul pe culoare…"
        btnLanes.UseVisualStyleBackColor = True
        '
        ' chkSrc -- load through the control's src property (file:///) instead of LoadFile; from the next PDF. Checked by
        ' default (07.10.2026: src showed the page without a click); unchecked = LoadFile, kept in case src fails somewhere
        '
        chkSrc.AutoSize = True
        chkSrc.Checked = True
        chkSrc.CheckState = CheckState.Checked
        chkSrc.Margin = New Padding(16, 8, 3, 0)
        chkSrc.Name = "chkSrc"
        chkSrc.TabIndex = 4
        chkSrc.Text = "Încarcă prin src (file:///) în loc de LoadFile"
        chkSrc.UseVisualStyleBackColor = True
        '
        ' chkDetailed -- the big watch (every window event into Logs\activex_check.log) instead of the small one; from
        ' the next PDF. Unchecked by default (operator, 07.10.2026: the tracking stays in the code, unused).
        '
        chkDetailed.AutoSize = True
        chkDetailed.Margin = New Padding(16, 8, 3, 0)
        chkDetailed.Name = "chkDetailed"
        chkDetailed.TabIndex = 6
        chkDetailed.Text = "Urmărire detaliată (activex_check.log)"
        chkDetailed.UseVisualStyleBackColor = True
        '
        ' chkSaveTrap -- signing and saving as in the app: the PDF is COPIED to TempPdf and the copy is loaded, with the
        ' Save As trap on (the trap overwrites the file it loaded, so never the operator's original). From the next PDF.
        ' Checked by default (operator, 07.10.2026): unchecked, a signature is saved wherever Adobe proposes.
        '
        chkSaveTrap.AutoSize = True
        chkSaveTrap.Checked = True
        chkSaveTrap.CheckState = CheckState.Checked
        chkSaveTrap.Margin = New Padding(16, 8, 3, 0)
        chkSaveTrap.Name = "chkSaveTrap"
        chkSaveTrap.TabIndex = 7
        chkSaveTrap.Text = "Semnare și salvare ca în aplicație (pe o copie)"
        chkSaveTrap.UseVisualStyleBackColor = True
        '
        ' btnCopyFolder -- the copy in Explorer (the signed file is checked there)
        '
        btnCopyFolder.AutoSize = True
        btnCopyFolder.Dock = DockStyle.Right
        btnCopyFolder.Enabled = False
        btnCopyFolder.Margin = New Padding(3)
        btnCopyFolder.Name = "btnCopyFolder"
        btnCopyFolder.Padding = New Padding(8, 2, 8, 2)
        btnCopyFolder.TabIndex = 8
        btnCopyFolder.Text = "Arată copia"
        btnCopyFolder.UseVisualStyleBackColor = True
        '
        ' btnSaveNow -- the viewer's RequestSave by hand (what the app's signing session asks after a signature)
        '
        btnSaveNow.AutoSize = True
        btnSaveNow.Dock = DockStyle.Right
        btnSaveNow.Margin = New Padding(3)
        btnSaveNow.Name = "btnSaveNow"
        btnSaveNow.Padding = New Padding(8, 2, 8, 2)
        btnSaveNow.TabIndex = 9
        btnSaveNow.Text = "Ctrl+S acum"
        btnSaveNow.UseVisualStyleBackColor = True
        '
        ' chkAutoSave -- what the app's signing session does: once the signed copy is written, Ctrl+S on its own
        '
        chkAutoSave.AutoSize = True
        chkAutoSave.Checked = True
        chkAutoSave.CheckState = CheckState.Checked
        chkAutoSave.Margin = New Padding(8, 8, 3, 0)
        chkAutoSave.Name = "chkAutoSave"
        chkAutoSave.TabIndex = 10
        chkAutoSave.Text = "Ctrl+S automat după semnătură"
        chkAutoSave.UseVisualStyleBackColor = True
        '
        ' lblFile
        '
        lblFile.AutoEllipsis = True
        lblFile.AutoSize = False
        lblFile.Dock = DockStyle.Fill
        lblFile.Name = "lblFile"
        lblFile.TextAlign = ContentAlignment.MiddleLeft
        lblFile.TabIndex = 5
        lblFile.Text = "Niciun document"
        '
        ' pnlHost -- the panel the AcroPDF control fills
        '
        pnlHost.BorderStyle = BorderStyle.FixedSingle
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Name = "pnlHost"
        pnlHost.TabIndex = 1
        '
        ' pnlVerdict
        '
        pnlVerdict.Controls.Add(btnPass)
        pnlVerdict.Controls.Add(btnFail)
        pnlVerdict.Dock = DockStyle.Bottom
        pnlVerdict.FlowDirection = FlowDirection.RightToLeft
        pnlVerdict.Height = 44
        pnlVerdict.Name = "pnlVerdict"
        pnlVerdict.Padding = New Padding(8, 6, 8, 6)
        pnlVerdict.TabIndex = 2
        '
        ' btnPass -- Yes/No, so that closing with X stays «no verdict» (Cancel)
        '
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.Yes
        btnPass.Name = "btnPass"
        btnPass.Padding = New Padding(14, 2, 14, 2)
        btnPass.TabIndex = 0
        btnPass.Text = "Merge"
        btnPass.UseVisualStyleBackColor = True
        '
        ' btnFail
        '
        btnFail.AutoSize = True
        btnFail.DialogResult = DialogResult.No
        btnFail.Name = "btnFail"
        btnFail.Padding = New Padding(14, 2, 14, 2)
        btnFail.TabIndex = 1
        btnFail.Text = "Nu merge"
        btnFail.UseVisualStyleBackColor = True
        '
        ' dlgOpen
        '
        dlgOpen.Filter = "Documente PDF|*.pdf|Toate fișierele|*.*"
        dlgOpen.Title = "Alege PDF-ul de deschis în ActiveX"
        '
        ' tmrSaved -- after a trapped save, waits for Adobe to finish writing the file before it is read
        '
        tmrSaved.Interval = 500
        '
        ' ActiveXPdfHarnessForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1400, 900)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(pnlHost)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlOptions)
        Controls.Add(pnlBar)
        Name = "ActiveXPdfHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc ActiveX — doar deschiderea unui PDF"
        pnlBar.ResumeLayout(False)
        pnlBar.PerformLayout()
        pnlOptions.ResumeLayout(False)
        pnlOptions.PerformLayout()
        pnlVerdict.ResumeLayout(False)
        pnlVerdict.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
#End If
