#If DEBUG Then
' Bench for slice 0078-04: the interim DDF (real section A, placeholder section B), generated from a real revision, signed on A
' and saved locally; then section B written INTO it (incremental update) and signed again.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfSectiuneaBHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBara As FlowLayoutPanel
    Friend WithEvents btnAutentificare As Button
    Friend WithEvents lblCod As Label
    Friend WithEvents txtCod As TextBox
    Friend WithEvents lblIdrev As Label
    Friend WithEvents txtIdrev As TextBox
    Friend WithEvents btnGenereaza As Button
    Friend WithEvents btnDeschide As Button
    Friend WithEvents btnDinServer As Button
    Friend WithEvents btnJurnal As Button
    Friend WithEvents btnInchideAdobe As Button

    Friend WithEvents pnlSectB As FlowLayoutPanel
    Friend WithEvents lblCodB As Label
    Friend WithEvents txtCodB As TextBox
    Friend WithEvents lblIndicator As Label
    Friend WithEvents txtIndicator As TextBox
    Friend WithEvents btnSectiuneaB As Button
    Friend WithEvents btnVerifica As Button
    Friend WithEvents lblDupa As Label
    Friend WithEvents cmbDupa As ComboBox
    Friend WithEvents chkServerBanc As CheckBox

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

    Friend WithEvents dlgDeschide As OpenFileDialog

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            ' The log listener goes before the controls do.
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
        btnAutentificare = New Button()
        lblCod = New Label()
        txtCod = New TextBox()
        lblIdrev = New Label()
        txtIdrev = New TextBox()
        btnGenereaza = New Button()
        btnDeschide = New Button()
        btnDinServer = New Button()
        btnJurnal = New Button()
        btnInchideAdobe = New Button()
        pnlSectB = New FlowLayoutPanel()
        lblCodB = New Label()
        txtCodB = New TextBox()
        lblIndicator = New Label()
        txtIndicator = New TextBox()
        btnSectiuneaB = New Button()
        btnVerifica = New Button()
        lblDupa = New Label()
        cmbDupa = New ComboBox()
        chkServerBanc = New CheckBox()
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
        dlgDeschide = New OpenFileDialog()
        pnlBara.SuspendLayout()
        pnlSectB.SuspendLayout()
        pnlStare.SuspendLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).BeginInit()
        splRoot.Panel1.SuspendLayout()
        splRoot.Panel2.SuspendLayout()
        splRoot.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBara -- step 1: the interim document (real A, placeholder B), from a real revision
        '
        pnlBara.Controls.Add(btnAutentificare)
        pnlBara.Controls.Add(lblCod)
        pnlBara.Controls.Add(txtCod)
        pnlBara.Controls.Add(lblIdrev)
        pnlBara.Controls.Add(txtIdrev)
        pnlBara.Controls.Add(btnGenereaza)
        pnlBara.Controls.Add(btnDeschide)
        pnlBara.Controls.Add(btnDinServer)
        pnlBara.Controls.Add(btnJurnal)
        pnlBara.Controls.Add(btnInchideAdobe)
        pnlBara.Dock = DockStyle.Top
        pnlBara.Height = 44
        pnlBara.Name = "pnlBara"
        pnlBara.Padding = New Padding(8, 6, 8, 4)
        pnlBara.TabIndex = 0
        '
        ' btnAutentificare
        '
        btnAutentificare.AutoSize = True
        btnAutentificare.Name = "btnAutentificare"
        btnAutentificare.Padding = New Padding(8, 2, 8, 2)
        btnAutentificare.TabIndex = 0
        btnAutentificare.Text = "Autentificare…"
        btnAutentificare.UseVisualStyleBackColor = True
        '
        ' lblCod
        '
        lblCod.AutoSize = True
        lblCod.Margin = New Padding(12, 9, 3, 0)
        lblCod.Name = "lblCod"
        lblCod.TabIndex = 1
        lblCod.Text = "Cod angajament (FX_DDF):"
        '
        ' txtCod -- the server reads a DDF by angajament code, not by IDREV
        '
        txtCod.Margin = New Padding(3, 5, 3, 3)
        txtCod.Name = "txtCod"
        txtCod.Size = New Size(130, 23)
        txtCod.TabIndex = 2
        '
        ' lblIdrev
        '
        lblIdrev.AutoSize = True
        lblIdrev.Margin = New Padding(8, 9, 3, 0)
        lblIdrev.Name = "lblIdrev"
        lblIdrev.TabIndex = 3
        lblIdrev.Text = "IDREV:"
        '
        ' txtIdrev
        '
        txtIdrev.Margin = New Padding(3, 5, 3, 3)
        txtIdrev.Name = "txtIdrev"
        txtIdrev.Size = New Size(70, 23)
        txtIdrev.TabIndex = 4
        '
        ' btnGenereaza
        '
        btnGenereaza.AutoSize = True
        btnGenereaza.Margin = New Padding(12, 3, 3, 3)
        btnGenereaza.Name = "btnGenereaza"
        btnGenereaza.Padding = New Padding(8, 2, 8, 2)
        btnGenereaza.TabIndex = 5
        btnGenereaza.Text = "1. Generează DDF intermediar"
        btnGenereaza.UseVisualStyleBackColor = True
        '
        ' btnDeschide
        '
        btnDeschide.AutoSize = True
        btnDeschide.Name = "btnDeschide"
        btnDeschide.Padding = New Padding(8, 2, 8, 2)
        btnDeschide.TabIndex = 6
        btnDeschide.Text = "2. Deschide un PDF salvat…"
        btnDeschide.UseVisualStyleBackColor = True
        '
        ' btnDinServer -- slice 0078-05: a PDF from the bench table on the server (KBOT_BANC_PDF)
        '
        btnDinServer.AutoSize = True
        btnDinServer.Name = "btnDinServer"
        btnDinServer.Padding = New Padding(8, 2, 8, 2)
        btnDinServer.TabIndex = 9
        btnDinServer.Text = "Încarcă PDF de pe server…"
        btnDinServer.UseVisualStyleBackColor = True
        '
        ' btnJurnal
        '
        btnJurnal.AutoSize = True
        btnJurnal.Margin = New Padding(12, 3, 3, 3)
        btnJurnal.Name = "btnJurnal"
        btnJurnal.Padding = New Padding(8, 2, 8, 2)
        btnJurnal.TabIndex = 7
        btnJurnal.Text = "Golește jurnalul"
        btnJurnal.UseVisualStyleBackColor = True
        '
        ' btnInchideAdobe -- slice 0078-05: empties the viewer and closes every Adobe process (the
        ' log is kept)
        '
        btnInchideAdobe.AutoSize = True
        btnInchideAdobe.Margin = New Padding(12, 3, 3, 3)
        btnInchideAdobe.Name = "btnInchideAdobe"
        btnInchideAdobe.Padding = New Padding(8, 2, 8, 2)
        btnInchideAdobe.TabIndex = 8
        btnInchideAdobe.Text = "Golește + închide Adobe"
        btnInchideAdobe.UseVisualStyleBackColor = True
        '
        ' pnlSectB -- step 3: section B written INTO the A-signed document
        '
        pnlSectB.Controls.Add(lblCodB)
        pnlSectB.Controls.Add(txtCodB)
        pnlSectB.Controls.Add(lblIndicator)
        pnlSectB.Controls.Add(txtIndicator)
        pnlSectB.Controls.Add(btnSectiuneaB)
        pnlSectB.Controls.Add(btnVerifica)
        pnlSectB.Controls.Add(lblDupa)
        pnlSectB.Controls.Add(cmbDupa)
        pnlSectB.Controls.Add(chkServerBanc)
        pnlSectB.Dock = DockStyle.Top
        pnlSectB.Height = 72
        pnlSectB.Name = "pnlSectB"
        pnlSectB.Padding = New Padding(8, 2, 8, 4)
        pnlSectB.TabIndex = 1
        '
        ' lblCodB
        '
        lblCodB.AutoSize = True
        lblCodB.Margin = New Padding(3, 9, 3, 0)
        lblCodB.Name = "lblCodB"
        lblCodB.TabIndex = 0
        lblCodB.Text = "Secțiunea B (doar dacă serverul n-o are) — cod angajament:"
        '
        ' txtCodB
        '
        txtCodB.Margin = New Padding(3, 5, 3, 3)
        txtCodB.MaxLength = 11
        txtCodB.Name = "txtCodB"
        txtCodB.Size = New Size(110, 23)
        txtCodB.TabIndex = 1
        '
        ' lblIndicator
        '
        lblIndicator.AutoSize = True
        lblIndicator.Margin = New Padding(8, 9, 3, 0)
        lblIndicator.Name = "lblIndicator"
        lblIndicator.TabIndex = 2
        lblIndicator.Text = "Indicator:"
        '
        ' txtIndicator
        '
        txtIndicator.Margin = New Padding(3, 5, 3, 3)
        txtIndicator.MaxLength = 3
        txtIndicator.Name = "txtIndicator"
        txtIndicator.Size = New Size(50, 23)
        txtIndicator.TabIndex = 3
        '
        ' btnSectiuneaB
        '
        btnSectiuneaB.AutoSize = True
        btnSectiuneaB.Margin = New Padding(12, 3, 3, 3)
        btnSectiuneaB.Name = "btnSectiuneaB"
        btnSectiuneaB.Padding = New Padding(8, 2, 8, 2)
        btnSectiuneaB.TabIndex = 4
        btnSectiuneaB.Text = "3. Inserează Secțiunea B"
        btnSectiuneaB.UseVisualStyleBackColor = True
        '
        ' btnVerifica
        '
        btnVerifica.AutoSize = True
        btnVerifica.Name = "btnVerifica"
        btnVerifica.Padding = New Padding(8, 2, 8, 2)
        btnVerifica.TabIndex = 5
        btnVerifica.Text = "Verifică semnăturile"
        btnVerifica.UseVisualStyleBackColor = True
        '
        ' lblDupa -- slice 0078-05: what happens to the change the form makes AFTER a signature
        '
        lblDupa.AutoSize = True
        lblDupa.Margin = New Padding(16, 9, 3, 0)
        lblDupa.Name = "lblDupa"
        lblDupa.TabIndex = 6
        lblDupa.Text = "După semnătură (încărcare doar simulată):"
        '
        ' cmbDupa -- item 1 («Nimic») switches the save after signing off
        '
        cmbDupa.DropDownStyle = ComboBoxStyle.DropDownList
        cmbDupa.Items.AddRange(New Object() {
            "K-BOT salvează singur (Ctrl+S), apoi încarcă",
            "Nimic (ca înainte de 0078-05)"})
        cmbDupa.Margin = New Padding(3, 5, 3, 3)
        cmbDupa.Name = "cmbDupa"
        cmbDupa.Size = New Size(320, 23)
        cmbDupa.TabIndex = 7
        '
        ' chkServerBanc -- slice 0078-05: every simulated upload is ALSO sent to the bench table on
        ' the server (KBOT_BANC_PDF, only in 000_DEMO)
        '
        chkServerBanc.AutoSize = True
        chkServerBanc.Margin = New Padding(12, 8, 3, 0)
        chkServerBanc.Name = "chkServerBanc"
        chkServerBanc.TabIndex = 8
        chkServerBanc.Text = "Trimite și în tabela de probă (000_DEMO)"
        chkServerBanc.UseVisualStyleBackColor = True
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
        pnlStare.TabIndex = 2
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
        splRoot.TabIndex = 3
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
        pnlVerdict.TabIndex = 4
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
        ' dlgDeschide
        '
        dlgDeschide.Filter = "Documente PDF|*.pdf|Toate fișierele|*.*"
        dlgDeschide.Title = "Alege DDF-ul salvat (se deschide pe loc, din Temp\PDF)"
        '
        ' DdfSectiuneaBHarnessForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1280, 800)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(splRoot)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlStare)
        Controls.Add(pnlSectB)
        Controls.Add(pnlBara)
        Name = "DdfSectiuneaBHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc DDF — Secțiunea A semnată, apoi Secțiunea B (doar local)"
        pnlBara.ResumeLayout(False)
        pnlBara.PerformLayout()
        pnlSectB.ResumeLayout(False)
        pnlSectB.PerformLayout()
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
