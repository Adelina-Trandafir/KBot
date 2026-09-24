#If DEBUG Then
' Bench for slice 0078: sign a DDF / ORD inside the real Adobe preview, watch the Save As trap,
' upload the signed file and read it back from the server.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PdfSigningHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBara As FlowLayoutPanel
    Friend WithEvents cmbTip As ComboBox
    Friend WithEvents lblId As Label
    Friend WithEvents txtId As TextBox
    Friend WithEvents btnAutentificare As Button
    Friend WithEvents btnDinServer As Button
    Friend WithEvents btnAlege As Button
    Friend WithEvents chkIncarca As CheckBox
    Friend WithEvents btnCompara As Button
    Friend WithEvents btnJurnal As Button

    Friend WithEvents pnlStare As FlowLayoutPanel
    Friend WithEvents lblSesiune As Label
    Friend WithEvents lblMotor As Label
    Friend WithEvents lblFisier As Label

    Friend WithEvents splRoot As SplitContainer
    Friend WithEvents preview As ReaderHostPreview
    Friend WithEvents txtJurnal As TextBox

    Friend WithEvents pnlVerdict As FlowLayoutPanel
    Friend WithEvents btnPass As Button
    Friend WithEvents btnFail As Button

    Friend WithEvents dlgAlege As OpenFileDialog

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' The session's watcher and the log listener go before the controls do.
            If disposing Then Inchide()
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        pnlBara = New FlowLayoutPanel()
        cmbTip = New ComboBox()
        lblId = New Label()
        txtId = New TextBox()
        btnAutentificare = New Button()
        btnDinServer = New Button()
        btnAlege = New Button()
        chkIncarca = New CheckBox()
        btnCompara = New Button()
        btnJurnal = New Button()
        pnlStare = New FlowLayoutPanel()
        lblSesiune = New Label()
        lblMotor = New Label()
        lblFisier = New Label()
        splRoot = New SplitContainer()
        preview = New ReaderHostPreview()
        txtJurnal = New TextBox()
        pnlVerdict = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        dlgAlege = New OpenFileDialog()
        pnlBara.SuspendLayout()
        pnlStare.SuspendLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).BeginInit()
        splRoot.Panel1.SuspendLayout()
        splRoot.Panel2.SuspendLayout()
        splRoot.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBara
        '
        pnlBara.Controls.Add(cmbTip)
        pnlBara.Controls.Add(lblId)
        pnlBara.Controls.Add(txtId)
        pnlBara.Controls.Add(btnAutentificare)
        pnlBara.Controls.Add(btnDinServer)
        pnlBara.Controls.Add(btnAlege)
        pnlBara.Controls.Add(chkIncarca)
        pnlBara.Controls.Add(btnCompara)
        pnlBara.Controls.Add(btnJurnal)
        pnlBara.Dock = DockStyle.Top
        pnlBara.Height = 44
        pnlBara.Name = "pnlBara"
        pnlBara.Padding = New Padding(8, 6, 8, 4)
        pnlBara.TabIndex = 0
        '
        ' cmbTip -- DDF (id = IDREV) or ORD (id = IDORDP)
        '
        cmbTip.DropDownStyle = ComboBoxStyle.DropDownList
        cmbTip.Items.AddRange(New Object() {"DDF", "ORD"})
        cmbTip.Margin = New Padding(3, 5, 3, 3)
        cmbTip.Name = "cmbTip"
        cmbTip.Size = New Size(70, 23)
        cmbTip.TabIndex = 0
        '
        ' lblId
        '
        lblId.AutoSize = True
        lblId.Margin = New Padding(8, 9, 3, 0)
        lblId.Name = "lblId"
        lblId.TabIndex = 1
        lblId.Text = "Id (IDREV / IDORDP):"
        '
        ' txtId
        '
        txtId.Margin = New Padding(3, 5, 3, 3)
        txtId.Name = "txtId"
        txtId.Size = New Size(80, 23)
        txtId.TabIndex = 2
        '
        ' btnAutentificare
        '
        btnAutentificare.AutoSize = True
        btnAutentificare.Margin = New Padding(12, 3, 3, 3)
        btnAutentificare.Name = "btnAutentificare"
        btnAutentificare.Padding = New Padding(8, 2, 8, 2)
        btnAutentificare.TabIndex = 3
        btnAutentificare.Text = "Autentificare…"
        btnAutentificare.UseVisualStyleBackColor = True
        '
        ' btnDinServer
        '
        btnDinServer.AutoSize = True
        btnDinServer.Name = "btnDinServer"
        btnDinServer.Padding = New Padding(8, 2, 8, 2)
        btnDinServer.TabIndex = 4
        btnDinServer.Text = "Deschide copia de pe server"
        btnDinServer.UseVisualStyleBackColor = True
        '
        ' btnAlege
        '
        btnAlege.AutoSize = True
        btnAlege.Name = "btnAlege"
        btnAlege.Padding = New Padding(8, 2, 8, 2)
        btnAlege.TabIndex = 5
        btnAlege.Text = "Deschide un PDF local…"
        btnAlege.UseVisualStyleBackColor = True
        '
        ' chkIncarca -- off = only the trap is tested, nothing reaches the server
        '
        chkIncarca.AutoSize = True
        chkIncarca.Checked = True
        chkIncarca.CheckState = CheckState.Checked
        chkIncarca.Margin = New Padding(12, 8, 3, 0)
        chkIncarca.Name = "chkIncarca"
        chkIncarca.TabIndex = 6
        chkIncarca.Text = "Încarcă pe server după semnare"
        chkIncarca.UseVisualStyleBackColor = True
        '
        ' btnCompara
        '
        btnCompara.AutoSize = True
        btnCompara.Margin = New Padding(12, 3, 3, 3)
        btnCompara.Name = "btnCompara"
        btnCompara.Padding = New Padding(8, 2, 8, 2)
        btnCompara.TabIndex = 7
        btnCompara.Text = "Compară cu serverul"
        btnCompara.UseVisualStyleBackColor = True
        '
        ' btnJurnal
        '
        btnJurnal.AutoSize = True
        btnJurnal.Name = "btnJurnal"
        btnJurnal.Padding = New Padding(8, 2, 8, 2)
        btnJurnal.TabIndex = 8
        btnJurnal.Text = "Golește jurnalul"
        btnJurnal.UseVisualStyleBackColor = True
        '
        ' pnlStare
        '
        pnlStare.Controls.Add(lblSesiune)
        pnlStare.Controls.Add(lblMotor)
        pnlStare.Controls.Add(lblFisier)
        pnlStare.Dock = DockStyle.Top
        pnlStare.Height = 26
        pnlStare.Name = "pnlStare"
        pnlStare.Padding = New Padding(8, 2, 8, 2)
        pnlStare.TabIndex = 1
        '
        ' lblSesiune
        '
        lblSesiune.AutoSize = True
        lblSesiune.Margin = New Padding(3, 3, 16, 0)
        lblSesiune.Name = "lblSesiune"
        lblSesiune.TabIndex = 0
        lblSesiune.Text = "Neautentificat"
        '
        ' lblMotor -- the operator's engine setting, shown, never changed by the bench
        '
        lblMotor.AutoSize = True
        lblMotor.Margin = New Padding(3, 3, 16, 0)
        lblMotor.Name = "lblMotor"
        lblMotor.TabIndex = 1
        lblMotor.Text = "Motor: —"
        '
        ' lblFisier
        '
        lblFisier.AutoSize = True
        lblFisier.Margin = New Padding(3, 3, 3, 0)
        lblFisier.Name = "lblFisier"
        lblFisier.TabIndex = 2
        lblFisier.Text = "Niciun document"
        '
        ' splRoot -- the real Adobe preview on the left, the live log on the right
        '
        splRoot.Dock = DockStyle.Fill
        splRoot.Name = "splRoot"
        splRoot.Panel1.Controls.Add(preview)
        splRoot.Panel2.Controls.Add(txtJurnal)
        splRoot.SplitterDistance = 760
        splRoot.TabIndex = 2
        '
        ' preview
        '
        preview.Dock = DockStyle.Fill
        preview.Name = "preview"
        preview.TabIndex = 0
        '
        ' txtJurnal
        '
        txtJurnal.Dock = DockStyle.Fill
        txtJurnal.Multiline = True
        txtJurnal.Name = "txtJurnal"
        txtJurnal.ReadOnly = True
        txtJurnal.ScrollBars = ScrollBars.Both
        txtJurnal.TabIndex = 0
        txtJurnal.WordWrap = False
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
        ' dlgAlege
        '
        dlgAlege.Filter = "Documente PDF|*.pdf|Toate fișierele|*.*"
        dlgAlege.Title = "Alege PDF-ul de semnat (se lucrează pe o copie)"
        '
        ' PdfSigningHarnessForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1280, 800)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(splRoot)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlStare)
        Controls.Add(pnlBara)
        Name = "PdfSigningHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc semnare PDF — capcana «Salvare ca», încărcare, recitire"
        pnlBara.ResumeLayout(False)
        pnlBara.PerformLayout()
        pnlStare.ResumeLayout(False)
        pnlStare.PerformLayout()
        splRoot.Panel1.ResumeLayout(False)
        splRoot.Panel2.ResumeLayout(False)
        splRoot.Panel2.PerformLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).EndInit()
        splRoot.ResumeLayout(False)
        pnlVerdict.ResumeLayout(False)
        pnlVerdict.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
#End If
