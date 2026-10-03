#If DEBUG Then
' Playground for slice 0100-03: the REAL «Coada robotului» window over a simulated robot, and a property
' grid on every grid and every column of it; what was changed is saved as designer lines.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class QueuePlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        tips = New KBot.Controls.KBotToolTip(components)
        pnlSus = New Panel()
        lblAng = New Label()
        numAng = New NumericUpDown()
        lblTaburi = New Label()
        numTaburi = New NumericUpDown()
        lblDurata = New Label()
        numDurata = New NumericUpDown()
        flpComenzi = New FlowLayoutPanel()
        btnPorneste = New Button()
        btnSarcina = New Button()
        btnOpreste = New Button()
        btnFereastra = New Button()
        pnlAlegere = New Panel()
        lblTinta = New Label()
        cboTinta = New ComboBox()
        lblImagine = New Label()
        cboImagine = New ComboBox()
        pg = New PropertyGrid()
        txtExport = New TextBox()
        pnlJos = New Panel()
        btnSalveaza = New Button()
        btnInchide = New Button()
        lblSalvat = New Label()
        pnlSus.SuspendLayout()
        CType(numAng, ComponentModel.ISupportInitialize).BeginInit()
        CType(numTaburi, ComponentModel.ISupportInitialize).BeginInit()
        CType(numDurata, ComponentModel.ISupportInitialize).BeginInit()
        flpComenzi.SuspendLayout()
        pnlAlegere.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlSus -- the simulated run: how many angajamente, how many tabs, how long each one takes.
        '
        pnlSus.Controls.Add(flpComenzi)
        pnlSus.Controls.Add(numDurata)
        pnlSus.Controls.Add(lblDurata)
        pnlSus.Controls.Add(numTaburi)
        pnlSus.Controls.Add(lblTaburi)
        pnlSus.Controls.Add(numAng)
        pnlSus.Controls.Add(lblAng)
        pnlSus.Dock = DockStyle.Top
        pnlSus.Name = "pnlSus"
        pnlSus.Size = New Size(760, 96)
        pnlSus.TabIndex = 4
        pnlSus.Tag = "Card"
        '
        ' lblAng
        '
        lblAng.AutoSize = True
        lblAng.Location = New Point(12, 14)
        lblAng.Name = "lblAng"
        lblAng.TabIndex = 0
        lblAng.Text = "Angajamente:"
        '
        ' numAng
        '
        numAng.Location = New Point(110, 10)
        numAng.Maximum = New Decimal(New Integer() {40, 0, 0, 0})
        numAng.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numAng.Name = "numAng"
        numAng.Size = New Size(56, 23)
        numAng.TabIndex = 1
        numAng.Value = New Decimal(New Integer() {7, 0, 0, 0})
        '
        ' lblTaburi
        '
        lblTaburi.AutoSize = True
        lblTaburi.Location = New Point(190, 14)
        lblTaburi.Name = "lblTaburi"
        lblTaburi.TabIndex = 2
        lblTaburi.Text = "Taburi deodata:"
        '
        ' numTaburi
        '
        numTaburi.Location = New Point(292, 10)
        numTaburi.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        numTaburi.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numTaburi.Name = "numTaburi"
        numTaburi.Size = New Size(56, 23)
        numTaburi.TabIndex = 3
        numTaburi.Value = New Decimal(New Integer() {3, 0, 0, 0})
        '
        ' lblDurata
        '
        lblDurata.AutoSize = True
        lblDurata.Location = New Point(372, 14)
        lblDurata.Name = "lblDurata"
        lblDurata.TabIndex = 4
        lblDurata.Text = "Secunde / descarcare:"
        '
        ' numDurata
        '
        numDurata.Location = New Point(512, 10)
        numDurata.Maximum = New Decimal(New Integer() {900, 0, 0, 0})
        numDurata.Minimum = New Decimal(New Integer() {5, 0, 0, 0})
        numDurata.Name = "numDurata"
        numDurata.Size = New Size(64, 23)
        numDurata.TabIndex = 5
        numDurata.Value = New Decimal(New Integer() {90, 0, 0, 0})
        '
        ' flpComenzi
        '
        flpComenzi.Controls.Add(btnPorneste)
        flpComenzi.Controls.Add(btnSarcina)
        flpComenzi.Controls.Add(btnOpreste)
        flpComenzi.Controls.Add(btnFereastra)
        flpComenzi.Dock = DockStyle.Bottom
        flpComenzi.Name = "flpComenzi"
        flpComenzi.Padding = New Padding(8, 0, 8, 6)
        flpComenzi.Size = New Size(760, 48)
        flpComenzi.TabIndex = 6
        '
        ' btnPorneste
        '
        btnPorneste.AutoSize = True
        btnPorneste.FlatStyle = FlatStyle.Flat
        btnPorneste.Name = "btnPorneste"
        btnPorneste.Padding = New Padding(8, 2, 8, 2)
        btnPorneste.TabIndex = 0
        btnPorneste.Text = "Porneste descarcarea multipla"
        tips.SetToolTipHeader(btnPorneste, "Descarcare multipla simulata")
        tips.SetToolTipText(btnPorneste, "Pune in coada O sarcina «Actualizare multipla (N)»: N angajamente cu coduri inventate, cel mult T taburi deodata." & vbLf & "Randurile din lista de sus sunt cele in lucru; cele din lista de jos asteapta un tab.")
        btnPorneste.UseVisualStyleBackColor = True
        '
        ' btnSarcina
        '
        btnSarcina.AutoSize = True
        btnSarcina.FlatStyle = FlatStyle.Flat
        btnSarcina.Name = "btnSarcina"
        btnSarcina.Padding = New Padding(8, 2, 8, 2)
        btnSarcina.TabIndex = 1
        btnSarcina.Text = "+ sarcina simpla in coada"
        tips.SetToolTipHeader(btnSarcina, "Sarcina simpla")
        tips.SetToolTipText(btnSarcina, "O descarcare obisnuita, pusa la rand dupa cea curenta: apare in lista de jos, langa angajamentele care asteapta.")
        btnSarcina.UseVisualStyleBackColor = True
        '
        ' btnOpreste
        '
        btnOpreste.AutoSize = True
        btnOpreste.FlatStyle = FlatStyle.Flat
        btnOpreste.Name = "btnOpreste"
        btnOpreste.Padding = New Padding(8, 2, 8, 2)
        btnOpreste.TabIndex = 2
        btnOpreste.Text = "Opreste tot"
        btnOpreste.UseVisualStyleBackColor = True
        '
        ' btnFereastra
        '
        btnFereastra.AutoSize = True
        btnFereastra.FlatStyle = FlatStyle.Flat
        btnFereastra.Name = "btnFereastra"
        btnFereastra.Padding = New Padding(8, 2, 8, 2)
        btnFereastra.TabIndex = 3
        btnFereastra.Text = "(Re)deschide fereastra cozii"
        btnFereastra.UseVisualStyleBackColor = True
        '
        ' pnlAlegere -- which grid / column the property grid shows, and the picture of its button.
        '
        pnlAlegere.Controls.Add(cboImagine)
        pnlAlegere.Controls.Add(lblImagine)
        pnlAlegere.Controls.Add(cboTinta)
        pnlAlegere.Controls.Add(lblTinta)
        pnlAlegere.Dock = DockStyle.Top
        pnlAlegere.Name = "pnlAlegere"
        pnlAlegere.Size = New Size(760, 68)
        pnlAlegere.TabIndex = 3
        pnlAlegere.Tag = "Card"
        '
        ' lblTinta
        '
        lblTinta.AutoSize = True
        lblTinta.Location = New Point(12, 10)
        lblTinta.Name = "lblTinta"
        lblTinta.TabIndex = 0
        lblTinta.Text = "Grila / coloana:"
        '
        ' cboTinta
        '
        cboTinta.DropDownStyle = ComboBoxStyle.DropDownList
        cboTinta.Location = New Point(130, 6)
        cboTinta.Name = "cboTinta"
        cboTinta.Size = New Size(400, 23)
        cboTinta.TabIndex = 1
        '
        ' lblImagine
        '
        lblImagine.AutoSize = True
        lblImagine.Location = New Point(12, 40)
        lblImagine.Name = "lblImagine"
        lblImagine.TabIndex = 2
        lblImagine.Text = "Poza butonului:"
        '
        ' cboImagine
        '
        cboImagine.DropDownStyle = ComboBoxStyle.DropDownList
        cboImagine.Location = New Point(130, 36)
        cboImagine.Name = "cboImagine"
        cboImagine.Size = New Size(400, 23)
        cboImagine.TabIndex = 3
        tips.SetToolTipHeader(cboImagine, "Poza butonului")
        tips.SetToolTipText(cboImagine, "Resursele proiectului. Alegerea se aplica pe coloana aleasa (ButtonImage) si numele ei ajunge in fisierul salvat." & vbLf & "Merge doar pe o coloana, nu pe grila.")
        '
        ' pg -- the selected grid / column; every change shows at once in the real window.
        '
        pg.Dock = DockStyle.Fill
        pg.Name = "pg"
        pg.TabIndex = 2
        '
        ' txtExport -- what «Salveaza» wrote.
        '
        txtExport.Dock = DockStyle.Bottom
        txtExport.Multiline = True
        txtExport.Name = "txtExport"
        txtExport.ReadOnly = True
        txtExport.ScrollBars = ScrollBars.Both
        txtExport.Size = New Size(760, 150)
        txtExport.TabIndex = 1
        txtExport.WordWrap = False
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblSalvat)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(8, 6, 8, 6)
        pnlJos.Size = New Size(760, 48)
        pnlJos.TabIndex = 0
        pnlJos.Tag = "Card"
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.Dock = DockStyle.Left
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(12, 2, 12, 2)
        btnSalveaza.TabIndex = 0
        btnSalveaza.Text = "Salveaza ce am modificat"
        tips.SetToolTipHeader(btnSalveaza, "Salveaza pentru designer")
        tips.SetToolTipText(btnSalveaza, "Scrie liniile de designer (ce ar serializa Visual Studio) pentru ambele grile si toate coloanele lor." & vbLf & "Le pune in caseta de mai sus, in clipboard si intr-un fisier; i le dai lui Claude ca sa le scrie in RobotQueueForm.Designer.vb.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' btnInchide
        '
        btnInchide.AutoSize = True
        btnInchide.DialogResult = DialogResult.OK
        btnInchide.Dock = DockStyle.Right
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(12, 2, 12, 2)
        btnInchide.TabIndex = 1
        btnInchide.Text = "Gata"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' lblSalvat
        '
        lblSalvat.AutoEllipsis = True
        lblSalvat.Dock = DockStyle.Fill
        lblSalvat.Name = "lblSalvat"
        lblSalvat.Padding = New Padding(12, 0, 0, 0)
        lblSalvat.TabIndex = 2
        lblSalvat.TextAlign = ContentAlignment.MiddleLeft
        '
        ' QueuePlaygroundForm -- reverse dock order: pg (Fill) first.
        '
        AcceptButton = btnInchide
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(760, 800)
        Controls.Add(pg)
        Controls.Add(pnlAlegere)
        Controls.Add(txtExport)
        Controls.Add(pnlJos)
        Controls.Add(pnlSus)
        MinimumSize = New Size(640, 600)
        Name = "QueuePlaygroundForm"
        StartPosition = FormStartPosition.Manual
        Text = "Coada robotului — playground coloane (descarcare multipla simulata)"
        pnlSus.ResumeLayout(False)
        pnlSus.PerformLayout()
        CType(numAng, ComponentModel.ISupportInitialize).EndInit()
        CType(numTaburi, ComponentModel.ISupportInitialize).EndInit()
        CType(numDurata, ComponentModel.ISupportInitialize).EndInit()
        flpComenzi.ResumeLayout(False)
        flpComenzi.PerformLayout()
        pnlAlegere.ResumeLayout(False)
        pnlAlegere.PerformLayout()
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlSus As Panel
    Friend WithEvents lblAng As Label
    Friend WithEvents numAng As NumericUpDown
    Friend WithEvents lblTaburi As Label
    Friend WithEvents numTaburi As NumericUpDown
    Friend WithEvents lblDurata As Label
    Friend WithEvents numDurata As NumericUpDown
    Friend WithEvents flpComenzi As FlowLayoutPanel
    Friend WithEvents btnPorneste As Button
    Friend WithEvents btnSarcina As Button
    Friend WithEvents btnOpreste As Button
    Friend WithEvents btnFereastra As Button
    Friend WithEvents pnlAlegere As Panel
    Friend WithEvents lblTinta As Label
    Friend WithEvents cboTinta As ComboBox
    Friend WithEvents lblImagine As Label
    Friend WithEvents cboImagine As ComboBox
    Friend WithEvents pg As PropertyGrid
    Friend WithEvents txtExport As TextBox
    Friend WithEvents pnlJos As Panel
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnInchide As Button
    Friend WithEvents lblSalvat As Label
End Class
#End If
