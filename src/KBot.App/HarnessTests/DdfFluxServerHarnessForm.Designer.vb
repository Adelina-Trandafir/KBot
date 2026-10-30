' Bench for the DDF signing chain through the server: A signed -> uploaded -> closed -> deleted; then B from the server's A;
' then the director from the server's A+B.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfFluxServerHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBara As Panel
    Friend WithEvents btnAutentificare As Button
    Friend WithEvents lblCod As Label
    Friend WithEvents txtCod As TextBox
    Friend WithEvents lblIdrev As Label
    Friend WithEvents txtIdrev As TextBox
    Friend WithEvents lblCodB As Label
    Friend WithEvents txtCodB As TextBox
    Friend WithEvents lblIndicator As Label
    Friend WithEvents txtIndicator As TextBox
    Friend WithEvents btnCapturi As Button
    Friend WithEvents dlgCapturi As OpenFileDialog

    Friend WithEvents pnlPasi As Panel
    Friend WithEvents btnPas1 As Button
    Friend WithEvents btnPas2 As Button
    Friend WithEvents btnPas3 As Button
    Friend WithEvents btnInchideAdobe As Button
    Friend WithEvents btnJurnal As Button

    Friend WithEvents pnlStare As Panel
    Friend WithEvents lblSesiune As Label
    Friend WithEvents lblEtapa As Label
    Friend WithEvents lblFisier As Label

    Friend WithEvents splRoot As SplitContainer
    Friend WithEvents preview As ReaderHostPreview
    Friend WithEvents txtJurnal As TextBox

    Friend WithEvents pnlVerdict As Panel
    Friend WithEvents btnPass As Button
    Friend WithEvents btnFail As Button

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
        pnlBara = New Panel()
        btnAutentificare = New Button()
        lblCod = New Label()
        txtCod = New TextBox()
        lblIdrev = New Label()
        txtIdrev = New TextBox()
        lblCodB = New Label()
        txtCodB = New TextBox()
        lblIndicator = New Label()
        txtIndicator = New TextBox()
        btnCapturi = New Button()
        dlgCapturi = New OpenFileDialog()
        pnlPasi = New Panel()
        btnPas1 = New Button()
        btnPas2 = New Button()
        btnPas3 = New Button()
        btnInchideAdobe = New Button()
        btnJurnal = New Button()
        pnlStare = New Panel()
        lblSesiune = New Label()
        lblEtapa = New Label()
        lblFisier = New Label()
        splRoot = New SplitContainer()
        preview = New ReaderHostPreview()
        txtJurnal = New TextBox()
        pnlVerdict = New Panel()
        btnPass = New Button()
        btnFail = New Button()
        pnlBara.SuspendLayout()
        pnlPasi.SuspendLayout()
        pnlStare.SuspendLayout()
        CType(splRoot, ComponentModel.ISupportInitialize).BeginInit()
        splRoot.Panel1.SuspendLayout()
        splRoot.Panel2.SuspendLayout()
        splRoot.SuspendLayout()
        pnlVerdict.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBara -- sign-in, the revision, and the data section B needs when the server has no rows for it
        '
        pnlBara.Controls.Add(btnAutentificare)
        pnlBara.Controls.Add(lblCod)
        pnlBara.Controls.Add(txtCod)
        pnlBara.Controls.Add(lblIdrev)
        pnlBara.Controls.Add(txtIdrev)
        pnlBara.Controls.Add(lblCodB)
        pnlBara.Controls.Add(txtCodB)
        pnlBara.Controls.Add(lblIndicator)
        pnlBara.Controls.Add(txtIndicator)
        pnlBara.Controls.Add(btnCapturi)
        pnlBara.Dock = DockStyle.Top
        pnlBara.Height = 44
        pnlBara.Name = "pnlBara"
        pnlBara.TabIndex = 0
        '
        ' btnAutentificare
        '
        btnAutentificare.Location = New Point(8, 7)
        btnAutentificare.Name = "btnAutentificare"
        btnAutentificare.Size = New Size(120, 30)
        btnAutentificare.TabIndex = 0
        btnAutentificare.Text = "Autentificare…"
        btnAutentificare.UseVisualStyleBackColor = True
        '
        ' lblCod
        '
        lblCod.AutoSize = True
        lblCod.Location = New Point(140, 14)
        lblCod.Name = "lblCod"
        lblCod.TabIndex = 1
        lblCod.Text = "Cod angajament (FX_DDF):"
        '
        ' txtCod -- the server reads a DDF by angajament code, not by IDREV
        '
        txtCod.Location = New Point(300, 10)
        txtCod.Name = "txtCod"
        txtCod.Size = New Size(130, 23)
        txtCod.TabIndex = 2
        '
        ' lblIdrev
        '
        lblIdrev.AutoSize = True
        lblIdrev.Location = New Point(442, 14)
        lblIdrev.Name = "lblIdrev"
        lblIdrev.TabIndex = 3
        lblIdrev.Text = "IDREV:"
        '
        ' txtIdrev
        '
        txtIdrev.Location = New Point(494, 10)
        txtIdrev.Name = "txtIdrev"
        txtIdrev.Size = New Size(70, 23)
        txtIdrev.TabIndex = 4
        '
        ' lblCodB
        '
        lblCodB.AutoSize = True
        lblCodB.Location = New Point(584, 14)
        lblCodB.Name = "lblCodB"
        lblCodB.TabIndex = 5
        lblCodB.Text = "Secțiunea B, dacă serverul n-o are — cod:"
        '
        ' txtCodB
        '
        txtCodB.Location = New Point(830, 10)
        txtCodB.MaxLength = 11
        txtCodB.Name = "txtCodB"
        txtCodB.Size = New Size(110, 23)
        txtCodB.TabIndex = 6
        '
        ' lblIndicator
        '
        lblIndicator.AutoSize = True
        lblIndicator.Location = New Point(952, 14)
        lblIndicator.Name = "lblIndicator"
        lblIndicator.TabIndex = 7
        lblIndicator.Text = "Indicator:"
        '
        ' txtIndicator
        '
        txtIndicator.Location = New Point(1016, 10)
        txtIndicator.MaxLength = 3
        txtIndicator.Name = "txtIndicator"
        txtIndicator.Size = New Size(50, 23)
        txtIndicator.TabIndex = 8
        '
        ' btnCapturi -- sample captures for step 2 (instead of the revision's own PrtScr attachments)
        '
        btnCapturi.Location = New Point(1076, 7)
        btnCapturi.Name = "btnCapturi"
        btnCapturi.Size = New Size(194, 30)
        btnCapturi.TabIndex = 9
        btnCapturi.Text = "Capturi de probă (server)"
        btnCapturi.UseVisualStyleBackColor = True
        '
        ' dlgCapturi
        '
        dlgCapturi.Filter = "Imagini|*.png;*.jpg;*.jpeg|Toate fișierele|*.*"
        dlgCapturi.Multiselect = True
        dlgCapturi.Title = "Alege capturile de probă pentru Secțiunea B"
        '
        ' pnlPasi -- the three steps, in order; each one ends by itself (upload, close Adobe, delete the local copies)
        '
        pnlPasi.Controls.Add(btnPas1)
        pnlPasi.Controls.Add(btnPas2)
        pnlPasi.Controls.Add(btnPas3)
        pnlPasi.Controls.Add(btnInchideAdobe)
        pnlPasi.Controls.Add(btnJurnal)
        pnlPasi.Dock = DockStyle.Top
        pnlPasi.Height = 48
        pnlPasi.Name = "pnlPasi"
        pnlPasi.TabIndex = 1
        '
        ' btnPas1
        '
        btnPas1.Location = New Point(8, 7)
        btnPas1.Name = "btnPas1"
        btnPas1.Size = New Size(240, 34)
        btnPas1.TabIndex = 0
        btnPas1.Text = "1. Generează A + semnare → server"
        btnPas1.UseVisualStyleBackColor = True
        '
        ' btnPas2
        '
        btnPas2.Location = New Point(256, 7)
        btnPas2.Name = "btnPas2"
        btnPas2.Size = New Size(360, 34)
        btnPas2.TabIndex = 1
        btnPas2.Text = "2. A de pe server + Secțiunea B + semnare → server"
        btnPas2.UseVisualStyleBackColor = True
        '
        ' btnPas3
        '
        btnPas3.Location = New Point(624, 7)
        btnPas3.Name = "btnPas3"
        btnPas3.Size = New Size(290, 34)
        btnPas3.TabIndex = 2
        btnPas3.Text = "3. A+B de pe server + director → server"
        btnPas3.UseVisualStyleBackColor = True
        '
        ' btnInchideAdobe -- empties the viewer and closes every Adobe process (the log is kept)
        '
        btnInchideAdobe.Location = New Point(922, 7)
        btnInchideAdobe.Name = "btnInchideAdobe"
        btnInchideAdobe.Size = New Size(170, 34)
        btnInchideAdobe.TabIndex = 3
        btnInchideAdobe.Text = "Golește + închide Adobe"
        btnInchideAdobe.UseVisualStyleBackColor = True
        '
        ' btnJurnal
        '
        btnJurnal.Location = New Point(1100, 7)
        btnJurnal.Name = "btnJurnal"
        btnJurnal.Size = New Size(150, 34)
        btnJurnal.TabIndex = 4
        btnJurnal.Text = "Golește jurnalul"
        btnJurnal.UseVisualStyleBackColor = True
        '
        ' pnlStare
        '
        pnlStare.Controls.Add(lblSesiune)
        pnlStare.Controls.Add(lblEtapa)
        pnlStare.Controls.Add(lblFisier)
        pnlStare.Dock = DockStyle.Top
        pnlStare.Height = 26
        pnlStare.Name = "pnlStare"
        pnlStare.TabIndex = 2
        '
        ' lblSesiune
        '
        lblSesiune.AutoSize = True
        lblSesiune.Location = New Point(11, 5)
        lblSesiune.Name = "lblSesiune"
        lblSesiune.TabIndex = 0
        lblSesiune.Text = "Neautentificat"
        '
        ' lblEtapa
        '
        lblEtapa.AutoSize = True
        lblEtapa.Location = New Point(330, 5)
        lblEtapa.Name = "lblEtapa"
        lblEtapa.TabIndex = 1
        lblEtapa.Text = "Etapa: —"
        '
        ' lblFisier
        '
        lblFisier.AutoSize = True
        lblFisier.Location = New Point(640, 5)
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
        pnlVerdict.Height = 44
        pnlVerdict.Name = "pnlVerdict"
        pnlVerdict.TabIndex = 4
        '
        ' btnPass -- Yes/No, so that closing with X stays «no verdict» (Cancel)
        '
        btnPass.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnPass.DialogResult = DialogResult.Yes
        btnPass.Location = New Point(1162, 7)
        btnPass.Name = "btnPass"
        btnPass.Size = New Size(110, 30)
        btnPass.TabIndex = 0
        btnPass.Text = "Merge"
        btnPass.UseVisualStyleBackColor = True
        '
        ' btnFail
        '
        btnFail.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnFail.DialogResult = DialogResult.No
        btnFail.Location = New Point(1042, 7)
        btnFail.Name = "btnFail"
        btnFail.Size = New Size(110, 30)
        btnFail.TabIndex = 1
        btnFail.Text = "Nu merge"
        btnFail.UseVisualStyleBackColor = True
        '
        ' DdfFluxServerHarnessForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1280, 800)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(splRoot)
        Controls.Add(pnlVerdict)
        Controls.Add(pnlStare)
        Controls.Add(pnlPasi)
        Controls.Add(pnlBara)
        Name = "DdfFluxServerHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Banc DDF — A, apoi B, apoi directorul, cu serverul între pași"
        pnlBara.ResumeLayout(False)
        pnlBara.PerformLayout()
        pnlPasi.ResumeLayout(False)
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
