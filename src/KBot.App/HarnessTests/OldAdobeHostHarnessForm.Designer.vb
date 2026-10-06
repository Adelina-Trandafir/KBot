#If DEBUG Then
' Bench for slice 0078-12: the hosted Adobe window with an OLDER Adobe (before 2024: Acrobat DC /
' 2020), with the script-error monitor in view.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OldAdobeHostHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBar As FlowLayoutPanel
    Friend WithEvents btnOpen As Button
    Friend WithEvents btnReopen As Button
    Friend WithEvents btnRelease As Button
    Friend WithEvents chkReadMode As CheckBox
    Friend WithEvents chkSaveTrap As CheckBox
    Friend WithEvents chkTrace As CheckBox
    Friend WithEvents btnAlertList As Button
    Friend WithEvents btnTree As Button
    Friend WithEvents btnCopy As Button
    Friend WithEvents btnClearLog As Button

    Friend WithEvents pnlState As FlowLayoutPanel
    Friend WithEvents lblAdobe As Label
    Friend WithEvents lblFile As Label
    Friend WithEvents lblStatus As Label

    Friend WithEvents splRoot As SplitContainer
    Friend WithEvents pnlHost As Panel
    Friend WithEvents splRight As SplitContainer
    Friend WithEvents pnlAlerts As Panel
    Friend WithEvents lvAlerts As ListView
    Friend WithEvents colTime As ColumnHeader
    Friend WithEvents colKind As ColumnHeader
    Friend WithEvents colAction As ColumnHeader
    Friend WithEvents colText As ColumnHeader
    Friend WithEvents lblCounters As Label
    Friend WithEvents txtLog As TextBox

    Friend WithEvents pnlVerdict As FlowLayoutPanel
    Friend WithEvents btnPass As Button
    Friend WithEvents btnFail As Button

    Friend WithEvents dlgOpen As OpenFileDialog

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' The host (and the Adobe it started) and the log listener go before the controls do.
            If disposing Then ShutDownBench()
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlBar = New FlowLayoutPanel()
        btnOpen = New Button()
        btnReopen = New Button()
        btnRelease = New Button()
        chkReadMode = New CheckBox()
        chkSaveTrap = New CheckBox()
        chkTrace = New CheckBox()
        btnAlertList = New Button()
        btnTree = New Button()
        btnCopy = New Button()
        btnClearLog = New Button()
        pnlState = New FlowLayoutPanel()
        lblAdobe = New Label()
        lblFile = New Label()
        lblStatus = New Label()
        splRoot = New SplitContainer()
        pnlHost = New Panel()
        splRight = New SplitContainer()
        pnlAlerts = New Panel()
        lvAlerts = New ListView()
        colTime = New ColumnHeader()
        colKind = New ColumnHeader()
        colAction = New ColumnHeader()
        colText = New ColumnHeader()
        lblCounters = New Label()
        txtLog = New TextBox()
        pnlVerdict = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        dlgOpen = New OpenFileDialog()
        pnlBar.SuspendLayout()
        pnlState.SuspendLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).BeginInit()
        splRoot.Panel1.SuspendLayout()
        splRoot.Panel2.SuspendLayout()
        splRoot.SuspendLayout()
        CType(splRight, ComponentModel.ISupportInitialize).BeginInit()
        splRight.Panel1.SuspendLayout()
        splRight.Panel2.SuspendLayout()
        splRight.SuspendLayout()
        pnlAlerts.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBar
        '
        pnlBar.Controls.Add(btnOpen)
        pnlBar.Controls.Add(btnReopen)
        pnlBar.Controls.Add(btnRelease)
        pnlBar.Controls.Add(chkReadMode)
        pnlBar.Controls.Add(chkSaveTrap)
        pnlBar.Controls.Add(chkTrace)
        pnlBar.Controls.Add(btnAlertList)
        pnlBar.Controls.Add(btnTree)
        pnlBar.Controls.Add(btnCopy)
        pnlBar.Controls.Add(btnClearLog)
        pnlBar.Dock = DockStyle.Top
        pnlBar.Height = 76
        pnlBar.Name = "pnlBar"
        pnlBar.Padding = New Padding(8, 6, 8, 4)
        pnlBar.TabIndex = 0
        '
        ' btnOpen -- copies the PDF into Temp\PDF\Banc and hosts the copy (the trap may overwrite it)
        '
        btnOpen.AutoSize = True
        btnOpen.Name = "btnOpen"
        btnOpen.Padding = New Padding(8, 2, 8, 2)
        btnOpen.TabIndex = 0
        btnOpen.Text = "Deschide un PDF…"
        btnOpen.UseVisualStyleBackColor = True
        '
        ' btnReopen -- the same copy again, with the switches as they are now
        '
        btnReopen.AutoSize = True
        btnReopen.Enabled = False
        btnReopen.Name = "btnReopen"
        btnReopen.Padding = New Padding(8, 2, 8, 2)
        btnReopen.TabIndex = 1
        btnReopen.Text = "Redeschide"
        btnReopen.UseVisualStyleBackColor = True
        '
        ' btnRelease
        '
        btnRelease.AutoSize = True
        btnRelease.Name = "btnRelease"
        btnRelease.Padding = New Padding(8, 2, 8, 2)
        btnRelease.TabIndex = 2
        btnRelease.Text = "Închide documentul"
        btnRelease.UseVisualStyleBackColor = True
        '
        ' chkReadMode -- Ctrl+H (Read Mode) and Ctrl+2 sent once the page is laid out; from the next document
        '
        chkReadMode.AutoSize = True
        chkReadMode.Checked = True
        chkReadMode.CheckState = CheckState.Checked
        chkReadMode.Margin = New Padding(12, 8, 3, 0)
        chkReadMode.Name = "chkReadMode"
        chkReadMode.TabIndex = 3
        chkReadMode.Text = "Ctrl+H / F8 / Ctrl+2 (mod citire)"
        chkReadMode.UseVisualStyleBackColor = True
        '
        ' chkSaveTrap -- the «Salvare ca» trap AND the script-alert clicker are one object: off = neither runs
        '
        chkSaveTrap.AutoSize = True
        chkSaveTrap.Checked = True
        chkSaveTrap.CheckState = CheckState.Checked
        chkSaveTrap.Margin = New Padding(12, 8, 3, 0)
        chkSaveTrap.Name = "chkSaveTrap"
        chkSaveTrap.TabIndex = 4
        chkSaveTrap.Text = "Capcana (Salvare ca + mesaje de script)"
        chkSaveTrap.UseVisualStyleBackColor = True
        '
        ' chkTrace -- AcroPdfTraceLog.SwitchedOn: in memory only, restored when the bench closes
        '
        chkTrace.AutoSize = True
        chkTrace.Margin = New Padding(12, 8, 3, 0)
        chkTrace.Name = "chkTrace"
        chkTrace.TabIndex = 5
        chkTrace.Text = "Jurnal detaliat (acropdf_trace.log)"
        chkTrace.UseVisualStyleBackColor = True
        '
        ' btnAlertList
        '
        btnAlertList.AutoSize = True
        btnAlertList.Margin = New Padding(12, 3, 3, 3)
        btnAlertList.Name = "btnAlertList"
        btnAlertList.Padding = New Padding(8, 2, 8, 2)
        btnAlertList.TabIndex = 6
        btnAlertList.Text = "Lista mesajelor închise automat…"
        btnAlertList.UseVisualStyleBackColor = True
        '
        ' btnTree
        '
        btnTree.AutoSize = True
        btnTree.Name = "btnTree"
        btnTree.Padding = New Padding(8, 2, 8, 2)
        btnTree.TabIndex = 7
        btnTree.Text = "Arborele ferestrelor Adobe"
        btnTree.UseVisualStyleBackColor = True
        '
        ' btnCopy
        '
        btnCopy.AutoSize = True
        btnCopy.Name = "btnCopy"
        btnCopy.Padding = New Padding(8, 2, 8, 2)
        btnCopy.TabIndex = 8
        btnCopy.Text = "Copiază raportul"
        btnCopy.UseVisualStyleBackColor = True
        '
        ' btnClearLog
        '
        btnClearLog.AutoSize = True
        btnClearLog.Name = "btnClearLog"
        btnClearLog.Padding = New Padding(8, 2, 8, 2)
        btnClearLog.TabIndex = 9
        btnClearLog.Text = "Golește jurnalul"
        btnClearLog.UseVisualStyleBackColor = True
        '
        ' pnlState
        '
        pnlState.Controls.Add(lblAdobe)
        pnlState.Controls.Add(lblFile)
        pnlState.Controls.Add(lblStatus)
        pnlState.Dock = DockStyle.Top
        pnlState.Height = 26
        pnlState.Name = "pnlState"
        pnlState.Padding = New Padding(8, 2, 8, 2)
        pnlState.TabIndex = 1
        '
        ' lblAdobe -- the Adobe found on this machine and which side of 2024 it is
        '
        lblAdobe.AutoSize = True
        lblAdobe.Margin = New Padding(3, 3, 16, 0)
        lblAdobe.Name = "lblAdobe"
        lblAdobe.TabIndex = 0
        lblAdobe.Text = "Adobe: —"
        '
        ' lblFile
        '
        lblFile.AutoSize = True
        lblFile.Margin = New Padding(3, 3, 16, 0)
        lblFile.Name = "lblFile"
        lblFile.TabIndex = 1
        lblFile.Text = "Niciun document"
        '
        ' lblStatus -- hosted after N ms / document ready after N ms
        '
        lblStatus.AutoSize = True
        lblStatus.Margin = New Padding(3, 3, 3, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.TabIndex = 2
        lblStatus.Text = ""
        '
        ' splRoot -- the hosted Adobe on the left, the monitor and the live log on the right
        '
        splRoot.Dock = DockStyle.Fill
        splRoot.Name = "splRoot"
        splRoot.Panel1.Controls.Add(pnlHost)
        splRoot.Panel2.Controls.Add(splRight)
        splRoot.SplitterDistance = 760
        splRoot.TabIndex = 2
        '
        ' pnlHost -- the panel AdobeReaderHost reparents the Adobe window into
        '
        pnlHost.BorderStyle = BorderStyle.FixedSingle
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Name = "pnlHost"
        pnlHost.TabIndex = 0
        '
        ' splRight
        '
        splRight.Dock = DockStyle.Fill
        splRight.Name = "splRight"
        splRight.Orientation = Orientation.Horizontal
        splRight.Panel1.Controls.Add(pnlAlerts)
        splRight.Panel2.Controls.Add(txtLog)
        splRight.SplitterDistance = 260
        splRight.TabIndex = 0
        '
        ' pnlAlerts -- children in REVERSE dock order: Fill first, then the top edge
        '
        pnlAlerts.Controls.Add(lvAlerts)
        pnlAlerts.Controls.Add(lblCounters)
        pnlAlerts.Dock = DockStyle.Fill
        pnlAlerts.Name = "pnlAlerts"
        pnlAlerts.TabIndex = 0
        '
        ' lvAlerts -- one row per script window of the document on screen (cleared at each open)
        '
        lvAlerts.Columns.AddRange(New ColumnHeader() {colTime, colKind, colAction, colText})
        lvAlerts.Dock = DockStyle.Fill
        lvAlerts.FullRowSelect = True
        lvAlerts.GridLines = True
        lvAlerts.HideSelection = False
        lvAlerts.MultiSelect = False
        lvAlerts.Name = "lvAlerts"
        lvAlerts.TabIndex = 0
        lvAlerts.UseCompatibleStateImageBehavior = False
        lvAlerts.View = View.Details
        '
        ' colTime
        '
        colTime.Text = "Ora"
        colTime.Width = 90
        '
        ' colKind
        '
        colKind.Text = "Ce e"
        colKind.Width = 70
        '
        ' colAction
        '
        colAction.Text = "Ce a făcut K-BOT"
        colAction.Width = 190
        '
        ' colText
        '
        colText.Text = "Textul ferestrei"
        colText.Width = 520
        '
        ' lblCounters
        '
        lblCounters.Dock = DockStyle.Top
        lblCounters.Height = 22
        lblCounters.Name = "lblCounters"
        lblCounters.Padding = New Padding(6, 4, 6, 0)
        lblCounters.TabIndex = 1
        lblCounters.Text = "Mesaje de script Adobe: —"
        '
        ' txtLog
        '
        txtLog.Dock = DockStyle.Fill
        txtLog.Multiline = True
        txtLog.Name = "txtLog"
        txtLog.ReadOnly = True
        txtLog.ScrollBars = ScrollBars.Both
        txtLog.TabIndex = 0
        txtLog.WordWrap = False
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
        pnlVerdict.TabIndex = 3
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
        dlgOpen.Title = "Alege PDF-ul (se lucrează pe o copie)"
        '
        ' OldAdobeHostHarnessForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1400, 820)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(splRoot)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlState)
        Controls.Add(pnlBar)
        Name = "OldAdobeHostHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc fereastră găzduită — Adobe vechi (< 2024), cu monitorul de erori JS"
        pnlBar.ResumeLayout(False)
        pnlBar.PerformLayout()
        pnlState.ResumeLayout(False)
        pnlState.PerformLayout()
        splRoot.Panel1.ResumeLayout(False)
        splRoot.Panel2.ResumeLayout(False)
        CType(splRoot, ComponentModel.ISupportInitialize).EndInit()
        splRoot.ResumeLayout(False)
        splRight.Panel1.ResumeLayout(False)
        splRight.Panel2.ResumeLayout(False)
        splRight.Panel2.PerformLayout()
        CType(splRight, ComponentModel.ISupportInitialize).EndInit()
        splRight.ResumeLayout(False)
        pnlAlerts.ResumeLayout(False)
        pnlVerdict.ResumeLayout(False)
        pnlVerdict.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
#End If
