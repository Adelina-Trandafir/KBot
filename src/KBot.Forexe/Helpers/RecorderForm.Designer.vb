<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RecorderForm
    Inherits System.Windows.Forms.Form

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    ' ── Control declarations ──────────────────────────────────────────────────
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents pnlBrowser As System.Windows.Forms.Panel

    Friend WithEvents pnlToolbarDock As System.Windows.Forms.Panel
    Friend WithEvents btnAndocheaza As System.Windows.Forms.Button
    Friend WithEvents btnDetaseaza As System.Windows.Forms.Button
    Friend WithEvents btnResincronizeaza As System.Windows.Forms.Button

    Friend WithEvents pnlToolbarRec As System.Windows.Forms.Panel
    Friend WithEvents btnPornesteInreg As System.Windows.Forms.Button
    Friend WithEvents btnOpresteInreg As System.Windows.Forms.Button
    Friend WithEvents btnCurata As System.Windows.Forms.Button
    Friend WithEvents btnMonitor As System.Windows.Forms.Button

    Friend WithEvents pnlOptiuni As System.Windows.Forms.Panel
    Friend WithEvents chkWaitForAutomat As System.Windows.Forms.CheckBox
    Friend WithEvents chkBlocReset As System.Windows.Forms.CheckBox

    Friend WithEvents splitLista As System.Windows.Forms.SplitContainer
    Friend WithEvents lvPasi As System.Windows.Forms.ListView
    Friend WithEvents colIndex As System.Windows.Forms.ColumnHeader
    Friend WithEvents colTip As System.Windows.Forms.ColumnHeader
    Friend WithEvents colSelector As System.Windows.Forms.ColumnHeader
    Friend WithEvents colValoare As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAjax As System.Windows.Forms.ColumnHeader

    Friend WithEvents pnlDetaliu As System.Windows.Forms.Panel
    Friend WithEvents lblCandidati As System.Windows.Forms.Label
    Friend WithEvents cmbCandidati As System.Windows.Forms.ComboBox
    Friend WithEvents chkWaitForPas As System.Windows.Forms.CheckBox
    Friend WithEvents lblLogValue As System.Windows.Forms.Label
    Friend WithEvents txtLogValue As System.Windows.Forms.TextBox
    Friend WithEvents btnStergePas As System.Windows.Forms.Button
    Friend WithEvents btnSus As System.Windows.Forms.Button
    Friend WithEvents btnJos As System.Windows.Forms.Button

    Friend WithEvents pnlToolbarGen As System.Windows.Forms.Panel
    Friend WithEvents btnGenereaza As System.Windows.Forms.Button
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents rtbPreview As System.Windows.Forms.RichTextBox

    Friend WithEvents tmrResync As System.Windows.Forms.Timer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        splitMain = New System.Windows.Forms.SplitContainer()
        pnlBrowser = New System.Windows.Forms.Panel()
        pnlToolbarDock = New System.Windows.Forms.Panel()
        btnAndocheaza = New System.Windows.Forms.Button()
        btnDetaseaza = New System.Windows.Forms.Button()
        btnResincronizeaza = New System.Windows.Forms.Button()
        pnlToolbarRec = New System.Windows.Forms.Panel()
        btnPornesteInreg = New System.Windows.Forms.Button()
        btnOpresteInreg = New System.Windows.Forms.Button()
        btnCurata = New System.Windows.Forms.Button()
        btnMonitor = New System.Windows.Forms.Button()
        pnlOptiuni = New System.Windows.Forms.Panel()
        chkWaitForAutomat = New System.Windows.Forms.CheckBox()
        chkBlocReset = New System.Windows.Forms.CheckBox()
        splitLista = New System.Windows.Forms.SplitContainer()
        lvPasi = New System.Windows.Forms.ListView()
        colIndex = New System.Windows.Forms.ColumnHeader()
        colTip = New System.Windows.Forms.ColumnHeader()
        colSelector = New System.Windows.Forms.ColumnHeader()
        colValoare = New System.Windows.Forms.ColumnHeader()
        colAjax = New System.Windows.Forms.ColumnHeader()
        pnlDetaliu = New System.Windows.Forms.Panel()
        lblCandidati = New System.Windows.Forms.Label()
        cmbCandidati = New System.Windows.Forms.ComboBox()
        chkWaitForPas = New System.Windows.Forms.CheckBox()
        lblLogValue = New System.Windows.Forms.Label()
        txtLogValue = New System.Windows.Forms.TextBox()
        btnStergePas = New System.Windows.Forms.Button()
        btnSus = New System.Windows.Forms.Button()
        btnJos = New System.Windows.Forms.Button()
        pnlToolbarGen = New System.Windows.Forms.Panel()
        btnGenereaza = New System.Windows.Forms.Button()
        btnSalveaza = New System.Windows.Forms.Button()
        rtbPreview = New System.Windows.Forms.RichTextBox()
        tmrResync = New System.Windows.Forms.Timer(components)
        CType(splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        pnlToolbarDock.SuspendLayout()
        pnlToolbarRec.SuspendLayout()
        pnlOptiuni.SuspendLayout()
        CType(splitLista, System.ComponentModel.ISupportInitialize).BeginInit()
        splitLista.Panel1.SuspendLayout()
        splitLista.Panel2.SuspendLayout()
        splitLista.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        pnlToolbarGen.SuspendLayout()
        SuspendLayout()
        '
        ' splitMain
        '
        splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        splitMain.Location = New System.Drawing.Point(0, 0)
        splitMain.Name = "splitMain"
        splitMain.Panel1.Controls.Add(pnlBrowser)
        splitMain.Panel2.Controls.Add(splitLista)
        splitMain.Panel2.Controls.Add(pnlOptiuni)
        splitMain.Panel2.Controls.Add(pnlToolbarRec)
        splitMain.Panel2.Controls.Add(pnlToolbarDock)
        splitMain.Size = New System.Drawing.Size(1400, 900)
        splitMain.SplitterDistance = 940
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 0
        '
        ' pnlBrowser
        '
        pnlBrowser.Dock = System.Windows.Forms.DockStyle.Fill
        pnlBrowser.Location = New System.Drawing.Point(0, 0)
        pnlBrowser.Name = "pnlBrowser"
        pnlBrowser.Size = New System.Drawing.Size(940, 900)
        pnlBrowser.TabIndex = 0
        '
        ' pnlToolbarDock
        '
        pnlToolbarDock.Controls.Add(btnResincronizeaza)
        pnlToolbarDock.Controls.Add(btnDetaseaza)
        pnlToolbarDock.Controls.Add(btnAndocheaza)
        pnlToolbarDock.Dock = System.Windows.Forms.DockStyle.Top
        pnlToolbarDock.Location = New System.Drawing.Point(0, 0)
        pnlToolbarDock.Name = "pnlToolbarDock"
        pnlToolbarDock.Padding = New System.Windows.Forms.Padding(6, 6, 6, 0)
        pnlToolbarDock.Size = New System.Drawing.Size(454, 40)
        pnlToolbarDock.TabIndex = 0
        '
        ' btnAndocheaza
        '
        btnAndocheaza.Location = New System.Drawing.Point(6, 6)
        btnAndocheaza.Name = "btnAndocheaza"
        btnAndocheaza.Size = New System.Drawing.Size(110, 28)
        btnAndocheaza.TabIndex = 0
        btnAndocheaza.Text = "Andochează"
        btnAndocheaza.UseVisualStyleBackColor = True
        '
        ' btnDetaseaza
        '
        btnDetaseaza.Location = New System.Drawing.Point(122, 6)
        btnDetaseaza.Name = "btnDetaseaza"
        btnDetaseaza.Size = New System.Drawing.Size(110, 28)
        btnDetaseaza.TabIndex = 1
        btnDetaseaza.Text = "Detașează"
        btnDetaseaza.UseVisualStyleBackColor = True
        '
        ' btnResincronizeaza
        '
        btnResincronizeaza.Location = New System.Drawing.Point(238, 6)
        btnResincronizeaza.Name = "btnResincronizeaza"
        btnResincronizeaza.Size = New System.Drawing.Size(130, 28)
        btnResincronizeaza.TabIndex = 2
        btnResincronizeaza.Text = "Resincronizează"
        btnResincronizeaza.UseVisualStyleBackColor = True
        '
        ' pnlToolbarRec
        '
        pnlToolbarRec.Controls.Add(btnMonitor)
        pnlToolbarRec.Controls.Add(btnCurata)
        pnlToolbarRec.Controls.Add(btnOpresteInreg)
        pnlToolbarRec.Controls.Add(btnPornesteInreg)
        pnlToolbarRec.Dock = System.Windows.Forms.DockStyle.Top
        pnlToolbarRec.Location = New System.Drawing.Point(0, 40)
        pnlToolbarRec.Name = "pnlToolbarRec"
        pnlToolbarRec.Padding = New System.Windows.Forms.Padding(6, 6, 6, 0)
        pnlToolbarRec.Size = New System.Drawing.Size(454, 40)
        pnlToolbarRec.TabIndex = 1
        '
        ' btnPornesteInreg
        '
        btnPornesteInreg.Location = New System.Drawing.Point(6, 6)
        btnPornesteInreg.Name = "btnPornesteInreg"
        btnPornesteInreg.Size = New System.Drawing.Size(110, 28)
        btnPornesteInreg.TabIndex = 0
        btnPornesteInreg.Text = "Înregistrează"
        btnPornesteInreg.UseVisualStyleBackColor = True
        '
        ' btnOpresteInreg
        '
        btnOpresteInreg.Location = New System.Drawing.Point(122, 6)
        btnOpresteInreg.Name = "btnOpresteInreg"
        btnOpresteInreg.Size = New System.Drawing.Size(110, 28)
        btnOpresteInreg.TabIndex = 1
        btnOpresteInreg.Text = "Oprește"
        btnOpresteInreg.UseVisualStyleBackColor = True
        '
        ' btnCurata
        '
        btnCurata.Location = New System.Drawing.Point(238, 6)
        btnCurata.Name = "btnCurata"
        btnCurata.Size = New System.Drawing.Size(130, 28)
        btnCurata.TabIndex = 2
        btnCurata.Text = "Golește lista"
        btnCurata.UseVisualStyleBackColor = True
        '
        ' btnMonitor
        '
        btnMonitor.Location = New System.Drawing.Point(374, 6)
        btnMonitor.Name = "btnMonitor"
        btnMonitor.Size = New System.Drawing.Size(74, 28)
        btnMonitor.TabIndex = 3
        btnMonitor.Text = "Monitor"
        btnMonitor.UseVisualStyleBackColor = True
        '
        ' pnlOptiuni
        '
        pnlOptiuni.Controls.Add(chkBlocReset)
        pnlOptiuni.Controls.Add(chkWaitForAutomat)
        pnlOptiuni.Dock = System.Windows.Forms.DockStyle.Top
        pnlOptiuni.Location = New System.Drawing.Point(0, 80)
        pnlOptiuni.Name = "pnlOptiuni"
        pnlOptiuni.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        pnlOptiuni.Size = New System.Drawing.Size(454, 56)
        pnlOptiuni.TabIndex = 2
        '
        ' chkWaitForAutomat
        '
        chkWaitForAutomat.AutoSize = True
        chkWaitForAutomat.Location = New System.Drawing.Point(9, 6)
        chkWaitForAutomat.Name = "chkWaitForAutomat"
        chkWaitForAutomat.Size = New System.Drawing.Size(280, 19)
        chkWaitForAutomat.TabIndex = 0
        chkWaitForAutomat.Text = "Inserează WaitFor după acțiunile cu AJAX"
        chkWaitForAutomat.UseVisualStyleBackColor = True
        '
        ' chkBlocReset
        '
        chkBlocReset.AutoSize = True
        chkBlocReset.Location = New System.Drawing.Point(9, 29)
        chkBlocReset.Name = "chkBlocReset"
        chkBlocReset.Size = New System.Drawing.Size(280, 19)
        chkBlocReset.TabIndex = 1
        chkBlocReset.Text = "Adaugă blocul de reset la început"
        chkBlocReset.UseVisualStyleBackColor = True
        '
        ' splitLista
        '
        splitLista.Dock = System.Windows.Forms.DockStyle.Fill
        splitLista.Location = New System.Drawing.Point(0, 136)
        splitLista.Name = "splitLista"
        splitLista.Orientation = System.Windows.Forms.Orientation.Horizontal
        splitLista.Panel1.Controls.Add(lvPasi)
        splitLista.Panel1.Controls.Add(pnlDetaliu)
        splitLista.Panel2.Controls.Add(rtbPreview)
        splitLista.Panel2.Controls.Add(pnlToolbarGen)
        splitLista.Size = New System.Drawing.Size(454, 764)
        splitLista.SplitterDistance = 430
        splitLista.SplitterWidth = 6
        splitLista.TabIndex = 3
        '
        ' lvPasi
        '
        lvPasi.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {colIndex, colTip, colSelector, colValoare, colAjax})
        lvPasi.Dock = System.Windows.Forms.DockStyle.Fill
        lvPasi.FullRowSelect = True
        lvPasi.HideSelection = False
        lvPasi.Location = New System.Drawing.Point(0, 0)
        lvPasi.MultiSelect = False
        lvPasi.OwnerDraw = True
        lvPasi.Name = "lvPasi"
        lvPasi.Size = New System.Drawing.Size(454, 254)
        lvPasi.TabIndex = 0
        lvPasi.UseCompatibleStateImageBehavior = False
        lvPasi.View = System.Windows.Forms.View.Details
        '
        ' colIndex
        '
        colIndex.Text = "#"
        colIndex.Width = 36
        '
        ' colTip
        '
        colTip.Text = "Tip"
        colTip.Width = 76
        '
        ' colSelector
        '
        colSelector.Text = "Selector"
        colSelector.Width = 170
        '
        ' colValoare
        '
        colValoare.Text = "Valoare"
        colValoare.Width = 92
        '
        ' colAjax
        '
        colAjax.Text = "AJAX"
        colAjax.Width = 46
        '
        ' pnlDetaliu
        '
        pnlDetaliu.Controls.Add(btnJos)
        pnlDetaliu.Controls.Add(btnSus)
        pnlDetaliu.Controls.Add(btnStergePas)
        pnlDetaliu.Controls.Add(txtLogValue)
        pnlDetaliu.Controls.Add(lblLogValue)
        pnlDetaliu.Controls.Add(chkWaitForPas)
        pnlDetaliu.Controls.Add(cmbCandidati)
        pnlDetaliu.Controls.Add(lblCandidati)
        pnlDetaliu.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlDetaliu.Location = New System.Drawing.Point(0, 254)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Padding = New System.Windows.Forms.Padding(6)
        pnlDetaliu.Size = New System.Drawing.Size(454, 176)
        pnlDetaliu.TabIndex = 1
        pnlDetaliu.Visible = False
        '
        ' lblCandidati
        '
        lblCandidati.AutoSize = True
        lblCandidati.Location = New System.Drawing.Point(9, 8)
        lblCandidati.Name = "lblCandidati"
        lblCandidati.Size = New System.Drawing.Size(120, 15)
        lblCandidati.TabIndex = 0
        lblCandidati.Text = "Candidați de selector"
        '
        ' cmbCandidati
        '
        cmbCandidati.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cmbCandidati.Location = New System.Drawing.Point(9, 27)
        cmbCandidati.Name = "cmbCandidati"
        cmbCandidati.Size = New System.Drawing.Size(432, 23)
        cmbCandidati.TabIndex = 1
        '
        ' chkWaitForPas
        '
        chkWaitForPas.AutoSize = True
        chkWaitForPas.Location = New System.Drawing.Point(9, 58)
        chkWaitForPas.Name = "chkWaitForPas"
        chkWaitForPas.Size = New System.Drawing.Size(260, 19)
        chkWaitForPas.TabIndex = 2
        chkWaitForPas.Text = "Inserează WaitFor după acest pas"
        chkWaitForPas.UseVisualStyleBackColor = True
        '
        ' lblLogValue
        '
        lblLogValue.AutoSize = True
        lblLogValue.Location = New System.Drawing.Point(9, 86)
        lblLogValue.Name = "lblLogValue"
        lblLogValue.Size = New System.Drawing.Size(120, 15)
        lblLogValue.TabIndex = 3
        lblLogValue.Text = "Mesaj în jurnal (LogValue)"
        '
        ' txtLogValue
        '
        txtLogValue.Location = New System.Drawing.Point(9, 105)
        txtLogValue.Name = "txtLogValue"
        txtLogValue.Size = New System.Drawing.Size(432, 23)
        txtLogValue.TabIndex = 4
        '
        ' btnStergePas
        '
        btnStergePas.Location = New System.Drawing.Point(9, 138)
        btnStergePas.Name = "btnStergePas"
        btnStergePas.Size = New System.Drawing.Size(110, 28)
        btnStergePas.TabIndex = 5
        btnStergePas.Text = "Șterge pasul"
        btnStergePas.UseVisualStyleBackColor = True
        '
        ' btnSus
        '
        btnSus.Location = New System.Drawing.Point(125, 138)
        btnSus.Name = "btnSus"
        btnSus.Size = New System.Drawing.Size(70, 28)
        btnSus.TabIndex = 6
        btnSus.Text = "Sus"
        btnSus.UseVisualStyleBackColor = True
        '
        ' btnJos
        '
        btnJos.Location = New System.Drawing.Point(201, 138)
        btnJos.Name = "btnJos"
        btnJos.Size = New System.Drawing.Size(70, 28)
        btnJos.TabIndex = 7
        btnJos.Text = "Jos"
        btnJos.UseVisualStyleBackColor = True
        '
        ' pnlToolbarGen
        '
        pnlToolbarGen.Controls.Add(btnSalveaza)
        pnlToolbarGen.Controls.Add(btnGenereaza)
        pnlToolbarGen.Dock = System.Windows.Forms.DockStyle.Top
        pnlToolbarGen.Location = New System.Drawing.Point(0, 0)
        pnlToolbarGen.Name = "pnlToolbarGen"
        pnlToolbarGen.Padding = New System.Windows.Forms.Padding(6, 6, 6, 0)
        pnlToolbarGen.Size = New System.Drawing.Size(454, 40)
        pnlToolbarGen.TabIndex = 0
        '
        ' btnGenereaza
        '
        btnGenereaza.Location = New System.Drawing.Point(6, 6)
        btnGenereaza.Name = "btnGenereaza"
        btnGenereaza.Size = New System.Drawing.Size(110, 28)
        btnGenereaza.TabIndex = 0
        btnGenereaza.Text = "Generează"
        btnGenereaza.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Location = New System.Drawing.Point(122, 6)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New System.Drawing.Size(110, 28)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează .wfl"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' rtbPreview
        '
        rtbPreview.BorderStyle = System.Windows.Forms.BorderStyle.None
        rtbPreview.Dock = System.Windows.Forms.DockStyle.Fill
        rtbPreview.Font = New System.Drawing.Font("Consolas", 9.5F)
        rtbPreview.Location = New System.Drawing.Point(0, 40)
        rtbPreview.Name = "rtbPreview"
        rtbPreview.ReadOnly = True
        rtbPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both
        rtbPreview.Size = New System.Drawing.Size(454, 288)
        rtbPreview.TabIndex = 1
        rtbPreview.Text = ""
        rtbPreview.WordWrap = False
        '
        ' tmrResync
        '
        tmrResync.Interval = 150
        '
        ' RecorderForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(1400, 900)
        Controls.Add(splitMain)
        MinimumSize = New System.Drawing.Size(1000, 640)
        Name = "RecorderForm"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "K-BOT Recorder"
        TopMost = False
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        pnlToolbarDock.ResumeLayout(False)
        pnlToolbarRec.ResumeLayout(False)
        pnlOptiuni.ResumeLayout(False)
        pnlOptiuni.PerformLayout()
        splitLista.Panel1.ResumeLayout(False)
        splitLista.Panel2.ResumeLayout(False)
        CType(splitLista, System.ComponentModel.ISupportInitialize).EndInit()
        splitLista.ResumeLayout(False)
        pnlDetaliu.ResumeLayout(False)
        pnlDetaliu.PerformLayout()
        pnlToolbarGen.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
