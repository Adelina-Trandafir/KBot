#If DEBUG Then
' Bench for slice 0078-15: the AcroPDF ActiveX control alone, loading a PDF the operator picks.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ActiveXPdfHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBar As FlowLayoutPanel
    Friend WithEvents btnOpen As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents btnSeen As Button
    Friend WithEvents btnLogViewer As Button
    Friend WithEvents chkSrc As CheckBox
    Friend WithEvents lblFile As Label
    Friend WithEvents pnlHost As Panel
    Friend WithEvents pnlVerdict As FlowLayoutPanel
    Friend WithEvents btnPass As Button
    Friend WithEvents btnFail As Button
    Friend WithEvents dlgOpen As OpenFileDialog

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
        pnlBar = New FlowLayoutPanel()
        btnOpen = New Button()
        btnClose = New Button()
        btnSeen = New Button()
        btnLogViewer = New Button()
        chkSrc = New CheckBox()
        lblFile = New Label()
        pnlHost = New Panel()
        pnlVerdict = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        dlgOpen = New OpenFileDialog()
        pnlBar.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBar
        '
        pnlBar.Controls.Add(btnOpen)
        pnlBar.Controls.Add(btnClose)
        pnlBar.Controls.Add(btnSeen)
        pnlBar.Controls.Add(btnLogViewer)
        pnlBar.Controls.Add(chkSrc)
        pnlBar.Controls.Add(lblFile)
        pnlBar.Dock = DockStyle.Top
        pnlBar.Height = 42
        pnlBar.Name = "pnlBar"
        pnlBar.Padding = New Padding(8, 6, 8, 4)
        pnlBar.TabIndex = 0
        '
        ' btnOpen -- the PDF itself, no copy
        '
        btnOpen.AutoSize = True
        btnOpen.Name = "btnOpen"
        btnOpen.Padding = New Padding(8, 2, 8, 2)
        btnOpen.TabIndex = 0
        btnOpen.Text = "Deschide un PDF…"
        btnOpen.UseVisualStyleBackColor = True
        '
        ' btnClose -- releases the control (the next PDF gets a new one)
        '
        btnClose.AutoSize = True
        btnClose.Name = "btnClose"
        btnClose.Padding = New Padding(8, 2, 8, 2)
        btnClose.TabIndex = 1
        btnClose.Text = "Închide documentul"
        btnClose.UseVisualStyleBackColor = True
        '
        ' btnSeen -- the moment the document LOOKS loaded, into activex_check.log
        '
        btnSeen.AutoSize = True
        btnSeen.Margin = New Padding(12, 3, 3, 3)
        btnSeen.Name = "btnSeen"
        btnSeen.Padding = New Padding(8, 2, 8, 2)
        btnSeen.TabIndex = 2
        btnSeen.Text = "Îl văd încărcat"
        btnSeen.UseVisualStyleBackColor = True
        '
        ' btnLogViewer
        '
        btnLogViewer.AutoSize = True
        btnLogViewer.Margin = New Padding(12, 3, 3, 3)
        btnLogViewer.Name = "btnLogViewer"
        btnLogViewer.Padding = New Padding(8, 2, 8, 2)
        btnLogViewer.TabIndex = 3
        btnLogViewer.Text = "Jurnalul ActiveX…"
        btnLogViewer.UseVisualStyleBackColor = True
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
        ' lblFile
        '
        lblFile.AutoSize = True
        lblFile.Margin = New Padding(16, 9, 3, 0)
        lblFile.Name = "lblFile"
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
        ' ActiveXPdfHarnessForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1400, 900)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(pnlHost)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlBar)
        Name = "ActiveXPdfHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc ActiveX — doar deschiderea unui PDF"
        pnlBar.ResumeLayout(False)
        pnlBar.PerformLayout()
        pnlVerdict.ResumeLayout(False)
        pnlVerdict.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
#End If
