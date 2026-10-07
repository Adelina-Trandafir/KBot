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
    Friend WithEvents chkSimuleaza As CheckBox
    Friend WithEvents chkGazduita As CheckBox
    Friend WithEvents btnCompara As Button
    Friend WithEvents btnJurnal As Button
    Friend WithEvents btnVad As Button

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
        pnlBara = New FlowLayoutPanel()
        cmbTip = New ComboBox()
        lblId = New Label()
        txtId = New TextBox()
        btnAutentificare = New Button()
        btnDinServer = New Button()
        btnAlege = New Button()
        chkIncarca = New CheckBox()
        chkSimuleaza = New CheckBox()
        chkGazduita = New CheckBox()
        btnCompara = New Button()
        btnJurnal = New Button()
        btnVad = New Button()
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
        pnlBara.Controls.Add(chkGazduita)
        pnlBara.Controls.Add(chkSimuleaza)
        pnlBara.Controls.Add(chkIncarca)
        pnlBara.Controls.Add(btnCompara)
        pnlBara.Controls.Add(btnJurnal)
        pnlBara.Controls.Add(btnVad)
        pnlBara.Dock = DockStyle.Top
        pnlBara.Location = New Point(0, 0)
        pnlBara.Margin = New Padding(4, 4, 4, 4)
        pnlBara.Name = "pnlBara"
        pnlBara.Padding = New Padding(12, 9, 12, 6)
        pnlBara.Size = New Size(1920, 153)
        pnlBara.TabIndex = 0
        ' 
        ' cmbTip
        ' 
        cmbTip.DropDownStyle = ComboBoxStyle.DropDownList
        cmbTip.Items.AddRange(New Object() {"DDF", "ORD"})
        cmbTip.Location = New Point(16, 17)
        cmbTip.Margin = New Padding(4, 8, 4, 4)
        cmbTip.Name = "cmbTip"
        cmbTip.Size = New Size(103, 30)
        cmbTip.TabIndex = 0
        ' 
        ' lblId
        ' 
        lblId.AutoSize = True
        lblId.Location = New Point(135, 23)
        lblId.Margin = New Padding(12, 14, 4, 0)
        lblId.Name = "lblId"
        lblId.Size = New Size(161, 22)
        lblId.TabIndex = 1
        lblId.Text = "Id (IDREV / IDORDP):"
        ' 
        ' txtId
        ' 
        txtId.Location = New Point(304, 17)
        txtId.Margin = New Padding(4, 8, 4, 4)
        txtId.Name = "txtId"
        txtId.Size = New Size(118, 29)
        txtId.TabIndex = 2
        ' 
        ' btnAutentificare
        ' 
        btnAutentificare.AutoSize = True
        btnAutentificare.Location = New Point(444, 13)
        btnAutentificare.Margin = New Padding(18, 4, 4, 4)
        btnAutentificare.Name = "btnAutentificare"
        btnAutentificare.Padding = New Padding(12, 3, 12, 3)
        btnAutentificare.Size = New Size(225, 58)
        btnAutentificare.TabIndex = 3
        btnAutentificare.Text = "Autentificare…"
        btnAutentificare.UseVisualStyleBackColor = True
        ' 
        ' btnDinServer
        ' 
        btnDinServer.AutoSize = True
        btnDinServer.Location = New Point(677, 13)
        btnDinServer.Margin = New Padding(4, 4, 4, 4)
        btnDinServer.Name = "btnDinServer"
        btnDinServer.Padding = New Padding(12, 3, 12, 3)
        btnDinServer.Size = New Size(390, 58)
        btnDinServer.TabIndex = 4
        btnDinServer.Text = "Deschide copia de pe server"
        btnDinServer.UseVisualStyleBackColor = True
        ' 
        ' btnAlege
        ' 
        btnAlege.AutoSize = True
        btnAlege.Location = New Point(1075, 13)
        btnAlege.Margin = New Padding(4, 4, 4, 4)
        btnAlege.Name = "btnAlege"
        btnAlege.Padding = New Padding(12, 3, 12, 3)
        btnAlege.Size = New Size(339, 58)
        btnAlege.TabIndex = 5
        btnAlege.Text = "Deschide un PDF local…"
        btnAlege.UseVisualStyleBackColor = True
        ' 
        ' chkIncarca
        ' 
        chkIncarca.AutoSize = True
        chkIncarca.Checked = True
        chkIncarca.CheckState = CheckState.Checked
        chkIncarca.Location = New Point(281, 87)
        chkIncarca.Margin = New Padding(18, 12, 4, 0)
        chkIncarca.Name = "chkIncarca"
        chkIncarca.Size = New Size(268, 26)
        chkIncarca.TabIndex = 6
        chkIncarca.Text = "Încarcă pe server după semnare"
        chkIncarca.UseVisualStyleBackColor = True
        ' 
        ' chkSimuleaza
        ' 
        chkSimuleaza.AutoSize = True
        chkSimuleaza.Location = New Point(30, 87)
        chkSimuleaza.Margin = New Padding(18, 12, 4, 0)
        chkSimuleaza.Name = "chkSimuleaza"
        chkSimuleaza.Size = New Size(229, 26)
        chkSimuleaza.TabIndex = 7
        chkSimuleaza.Text = "Doar simulează încărcarea"
        chkSimuleaza.UseVisualStyleBackColor = True
        ' 
        ' chkGazduita
        ' 
        chkGazduita.AutoSize = True
        chkGazduita.Checked = True
        chkGazduita.CheckState = CheckState.Checked
        chkGazduita.Location = New Point(1436, 21)
        chkGazduita.Margin = New Padding(18, 12, 4, 0)
        chkGazduita.Name = "chkGazduita"
        chkGazduita.Size = New Size(236, 26)
        chkGazduita.TabIndex = 8
        chkGazduita.Text = "Forțează fereastra găzduită"
        chkGazduita.UseVisualStyleBackColor = True
        ' 
        ' btnCompara
        ' 
        btnCompara.AutoSize = True
        btnCompara.Location = New Point(571, 79)
        btnCompara.Margin = New Padding(18, 4, 4, 4)
        btnCompara.Name = "btnCompara"
        btnCompara.Padding = New Padding(12, 3, 12, 3)
        btnCompara.Size = New Size(300, 58)
        btnCompara.TabIndex = 9
        btnCompara.Text = "Compară cu serverul"
        btnCompara.UseVisualStyleBackColor = True
        ' 
        ' btnJurnal
        ' 
        btnJurnal.AutoSize = True
        btnJurnal.Dock = DockStyle.Right
        btnJurnal.Location = New Point(879, 79)
        btnJurnal.Margin = New Padding(4, 4, 4, 4)
        btnJurnal.Name = "btnJurnal"
        btnJurnal.Padding = New Padding(12, 3, 12, 3)
        btnJurnal.Size = New Size(158, 58)
        btnJurnal.TabIndex = 10
        btnJurnal.Text = "Golește jurnalul"
        btnJurnal.UseVisualStyleBackColor = True
        ' 
        ' btnVad
        ' 
        btnVad.AutoSize = True
        btnVad.Dock = DockStyle.Right
        btnVad.Location = New Point(1059, 79)
        btnVad.Margin = New Padding(18, 4, 4, 4)
        btnVad.Name = "btnVad"
        btnVad.Padding = New Padding(12, 3, 12, 3)
        btnVad.Size = New Size(146, 58)
        btnVad.TabIndex = 11
        btnVad.Text = "Îl văd încărcat"
        btnVad.UseVisualStyleBackColor = True
        ' 
        ' pnlStare
        ' 
        pnlStare.Controls.Add(lblSesiune)
        pnlStare.Controls.Add(lblMotor)
        pnlStare.Controls.Add(lblFisier)
        pnlStare.Dock = DockStyle.Top
        pnlStare.Location = New Point(0, 153)
        pnlStare.Margin = New Padding(4, 4, 4, 4)
        pnlStare.Name = "pnlStare"
        pnlStare.Padding = New Padding(12, 3, 12, 3)
        pnlStare.Size = New Size(1920, 39)
        pnlStare.TabIndex = 1
        ' 
        ' lblSesiune
        ' 
        lblSesiune.AutoSize = True
        lblSesiune.Location = New Point(16, 7)
        lblSesiune.Margin = New Padding(4, 4, 24, 0)
        lblSesiune.Name = "lblSesiune"
        lblSesiune.Size = New Size(116, 22)
        lblSesiune.TabIndex = 0
        lblSesiune.Text = "Neautentificat"
        ' 
        ' lblMotor
        ' 
        lblMotor.AutoSize = True
        lblMotor.Location = New Point(160, 7)
        lblMotor.Margin = New Padding(4, 4, 24, 0)
        lblMotor.Name = "lblMotor"
        lblMotor.Size = New Size(82, 22)
        lblMotor.TabIndex = 1
        lblMotor.Text = "Motor: —"
        ' 
        ' lblFisier
        ' 
        lblFisier.AutoSize = True
        lblFisier.Location = New Point(270, 7)
        lblFisier.Margin = New Padding(4, 4, 4, 0)
        lblFisier.Name = "lblFisier"
        lblFisier.Size = New Size(134, 22)
        lblFisier.TabIndex = 2
        lblFisier.Text = "Niciun document"
        ' 
        ' splRoot
        ' 
        splRoot.Dock = DockStyle.Fill
        splRoot.Location = New Point(0, 192)
        splRoot.Margin = New Padding(4, 4, 4, 4)
        splRoot.Name = "splRoot"
        ' 
        ' splRoot.Panel1
        ' 
        splRoot.Panel1.Controls.Add(preview)
        ' 
        ' splRoot.Panel2
        ' 
        splRoot.Panel2.Controls.Add(txtJurnal)
        splRoot.Size = New Size(1920, 942)
        splRoot.SplitterDistance = 1548
        splRoot.SplitterWidth = 6
        splRoot.TabIndex = 2
        ' 
        ' preview
        ' 
        preview.Dock = DockStyle.Fill
        preview.Font = New Font("Calibri", 9F)
        preview.Location = New Point(0, 0)
        preview.Margin = New Padding(4, 4, 4, 4)
        preview.Name = "preview"
        preview.Size = New Size(1548, 942)
        preview.TabIndex = 0
        ' 
        ' txtJurnal
        ' 
        txtJurnal.Dock = DockStyle.Fill
        txtJurnal.Location = New Point(0, 0)
        txtJurnal.Margin = New Padding(4, 4, 4, 4)
        txtJurnal.Multiline = True
        txtJurnal.Name = "txtJurnal"
        txtJurnal.ReadOnly = True
        txtJurnal.ScrollBars = ScrollBars.Both
        txtJurnal.Size = New Size(366, 942)
        txtJurnal.TabIndex = 0
        txtJurnal.WordWrap = False
        ' 
        ' pnlVerdict
        ' 
        pnlVerdict.Controls.Add(btnPass)
        pnlVerdict.Controls.Add(btnFail)
        pnlVerdict.Dock = DockStyle.Bottom
        pnlVerdict.FlowDirection = FlowDirection.RightToLeft
        pnlVerdict.Location = New Point(0, 1134)
        pnlVerdict.Margin = New Padding(4, 4, 4, 4)
        pnlVerdict.Name = "pnlVerdict"
        pnlVerdict.Padding = New Padding(12, 9, 12, 9)
        pnlVerdict.Size = New Size(1920, 66)
        pnlVerdict.TabIndex = 3
        ' 
        ' btnPass
        ' 
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.Yes
        btnPass.Location = New Point(1740, 13)
        btnPass.Margin = New Padding(4, 4, 4, 4)
        btnPass.Name = "btnPass"
        btnPass.Padding = New Padding(21, 3, 21, 3)
        btnPass.Size = New Size(152, 58)
        btnPass.TabIndex = 0
        btnPass.Text = "Merge"
        btnPass.UseVisualStyleBackColor = True
        ' 
        ' btnFail
        ' 
        btnFail.AutoSize = True
        btnFail.DialogResult = DialogResult.No
        btnFail.Location = New Point(1538, 13)
        btnFail.Margin = New Padding(4, 4, 4, 4)
        btnFail.Name = "btnFail"
        btnFail.Padding = New Padding(21, 3, 21, 3)
        btnFail.Size = New Size(194, 58)
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
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1920, 1200)
        Controls.Add(splRoot)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlStare)
        Controls.Add(pnlBara)
        Margin = New Padding(4, 4, 4, 4)
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
