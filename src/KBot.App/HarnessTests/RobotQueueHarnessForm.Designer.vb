#If DEBUG Then
' Bench for slice 0098: the robot queue + the server gate, over a simulated FOREXE robot and a
' simulated server (FakeForexeRunner / FakeServerHandler). Nothing leaves the PC.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RobotQueueHarnessForm
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
        lblDurata = New Label()
        numDurata = New NumericUpDown()
        lblServer = New Label()
        numServer = New NumericUpDown()
        chkAsociere = New CheckBox()
        chkEsec = New CheckBox()
        lblPoarta = New Label()
        lblRobot = New Label()
        pnlStanga = New Panel()
        flpComenzi = New FlowLayoutPanel()
        btnReimprospatare = New Button()
        btnRafala = New Button()
        btnDublura = New Button()
        btnOperatiune = New Button()
        btnDirect = New Button()
        btnCitire = New Button()
        btnScriere = New Button()
        btnCoada = New Button()
        btnGolesteJurnal = New Button()
        lstCoduri = New ListBox()
        lblCoduri = New Label()
        pnlJos = New Panel()
        btnReusit = New Button()
        btnEsuat = New Button()
        txtJurnal = New TextBox()
        pnlSus.SuspendLayout()
        CType(numDurata, ComponentModel.ISupportInitialize).BeginInit()
        CType(numServer, ComponentModel.ISupportInitialize).BeginInit()
        pnlStanga.SuspendLayout()
        flpComenzi.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlSus -- the simulation settings and the live state of the gate / robot.
        '
        pnlSus.Controls.Add(lblDurata)
        pnlSus.Controls.Add(numDurata)
        pnlSus.Controls.Add(lblServer)
        pnlSus.Controls.Add(numServer)
        pnlSus.Controls.Add(chkAsociere)
        pnlSus.Controls.Add(chkEsec)
        pnlSus.Controls.Add(lblPoarta)
        pnlSus.Controls.Add(lblRobot)
        pnlSus.Dock = DockStyle.Top
        pnlSus.Location = New Point(0, 0)
        pnlSus.Name = "pnlSus"
        pnlSus.Size = New Size(980, 76)
        pnlSus.TabIndex = 0
        '
        ' lblDurata
        '
        lblDurata.AutoSize = True
        lblDurata.Location = New Point(12, 12)
        lblDurata.Name = "lblDurata"
        lblDurata.Size = New Size(120, 15)
        lblDurata.TabIndex = 0
        lblDurata.Text = "Durata robotului (s):"
        '
        ' numDurata
        '
        numDurata.DecimalPlaces = 1
        numDurata.Increment = New Decimal(New Integer() {5, 0, 0, 65536})
        numDurata.Location = New Point(150, 9)
        numDurata.Maximum = New Decimal(New Integer() {120, 0, 0, 0})
        numDurata.Minimum = New Decimal(New Integer() {5, 0, 0, 65536})
        numDurata.Name = "numDurata"
        numDurata.Size = New Size(70, 23)
        numDurata.TabIndex = 1
        numDurata.Value = New Decimal(New Integer() {4, 0, 0, 0})
        '
        ' lblServer
        '
        lblServer.AutoSize = True
        lblServer.Location = New Point(240, 12)
        lblServer.Name = "lblServer"
        lblServer.Size = New Size(130, 15)
        lblServer.TabIndex = 2
        lblServer.Text = "Răspunsul serverului (ms):"
        '
        ' numServer
        '
        numServer.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        numServer.Location = New Point(400, 9)
        numServer.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        numServer.Name = "numServer"
        numServer.Size = New Size(70, 23)
        numServer.TabIndex = 3
        numServer.Value = New Decimal(New Integer() {400, 0, 0, 0})
        '
        ' chkAsociere
        '
        chkAsociere.AutoSize = True
        chkAsociere.Checked = True
        chkAsociere.CheckState = CheckState.Checked
        chkAsociere.Location = New Point(492, 10)
        chkAsociere.Name = "chkAsociere"
        chkAsociere.Size = New Size(230, 19)
        chkAsociere.TabIndex = 4
        chkAsociere.Text = "Deschide «Asociere» simulată după descărcare"
        chkAsociere.UseVisualStyleBackColor = True
        '
        ' chkEsec
        '
        chkEsec.AutoSize = True
        chkEsec.Location = New Point(492, 40)
        chkEsec.Name = "chkEsec"
        chkEsec.Size = New Size(160, 19)
        chkEsec.TabIndex = 5
        chkEsec.Text = "Robotul eșuează (workflow oprit)"
        chkEsec.UseVisualStyleBackColor = True
        '
        ' lblPoarta
        '
        lblPoarta.AutoSize = True
        lblPoarta.Font = New Font("Segoe UI Semibold", 9.75F)
        lblPoarta.Location = New Point(12, 44)
        lblPoarta.Name = "lblPoarta"
        lblPoarta.Size = New Size(190, 17)
        lblPoarta.TabIndex = 6
        lblPoarta.Text = "Poarta: deschisă"
        '
        ' lblRobot
        '
        lblRobot.AutoSize = True
        lblRobot.Font = New Font("Segoe UI Semibold", 9.75F)
        lblRobot.Location = New Point(240, 44)
        lblRobot.Name = "lblRobot"
        lblRobot.Size = New Size(120, 17)
        lblRobot.TabIndex = 7
        lblRobot.Text = "Robot: liber"
        '
        ' pnlStanga -- the codes and the scenarios. Reverse dock order: flpComenzi (Fill) first.
        '
        pnlStanga.Controls.Add(flpComenzi)
        pnlStanga.Controls.Add(lstCoduri)
        pnlStanga.Controls.Add(lblCoduri)
        pnlStanga.Dock = DockStyle.Left
        pnlStanga.Location = New Point(0, 76)
        pnlStanga.Name = "pnlStanga"
        pnlStanga.Padding = New Padding(8, 0, 8, 8)
        pnlStanga.Size = New Size(290, 516)
        pnlStanga.TabIndex = 1
        '
        ' flpComenzi
        '
        flpComenzi.Controls.Add(btnReimprospatare)
        flpComenzi.Controls.Add(btnRafala)
        flpComenzi.Controls.Add(btnDublura)
        flpComenzi.Controls.Add(btnOperatiune)
        flpComenzi.Controls.Add(btnDirect)
        flpComenzi.Controls.Add(btnCitire)
        flpComenzi.Controls.Add(btnScriere)
        flpComenzi.Controls.Add(btnCoada)
        flpComenzi.Controls.Add(btnGolesteJurnal)
        flpComenzi.Dock = DockStyle.Fill
        flpComenzi.FlowDirection = FlowDirection.TopDown
        flpComenzi.Location = New Point(8, 186)
        flpComenzi.Name = "flpComenzi"
        flpComenzi.Padding = New Padding(0, 8, 0, 0)
        flpComenzi.Size = New Size(274, 322)
        flpComenzi.TabIndex = 2
        flpComenzi.WrapContents = False
        '
        ' btnReimprospatare
        '
        btnReimprospatare.FlatStyle = FlatStyle.Flat
        btnReimprospatare.Name = "btnReimprospatare"
        btnReimprospatare.Size = New Size(268, 30)
        btnReimprospatare.TabIndex = 0
        btnReimprospatare.Text = "Reîmprospătează nodurile selectate"
        btnReimprospatare.UseVisualStyleBackColor = True
        '
        ' btnRafala
        '
        btnRafala.FlatStyle = FlatStyle.Flat
        btnRafala.Name = "btnRafala"
        btnRafala.Size = New Size(268, 30)
        btnRafala.TabIndex = 1
        btnRafala.Text = "Toate nodurile, la rând"
        btnRafala.UseVisualStyleBackColor = True
        '
        ' btnDublura
        '
        btnDublura.FlatStyle = FlatStyle.Flat
        btnDublura.Name = "btnDublura"
        btnDublura.Size = New Size(268, 30)
        btnDublura.TabIndex = 2
        btnDublura.Text = "Același nod de două ori"
        btnDublura.UseVisualStyleBackColor = True
        '
        ' btnOperatiune
        '
        btnOperatiune.FlatStyle = FlatStyle.Flat
        btnOperatiune.Name = "btnOperatiune"
        btnOperatiune.Size = New Size(268, 30)
        btnOperatiune.TabIndex = 3
        btnOperatiune.Text = "Operațiune FOREXE (în fața cozii)"
        btnOperatiune.UseVisualStyleBackColor = True
        '
        ' btnDirect
        '
        btnDirect.FlatStyle = FlatStyle.Flat
        btnDirect.Name = "btnDirect"
        btnDirect.Size = New Size(268, 30)
        btnDirect.TabIndex = 4
        btnDirect.Text = "Robot pornit în afara cozii"
        btnDirect.UseVisualStyleBackColor = True
        '
        ' btnCitire
        '
        btnCitire.FlatStyle = FlatStyle.Flat
        btnCitire.Name = "btnCitire"
        btnCitire.Size = New Size(268, 30)
        btnCitire.TabIndex = 5
        btnCitire.Text = "Citire de pe server (GET)"
        btnCitire.UseVisualStyleBackColor = True
        '
        ' btnScriere
        '
        btnScriere.FlatStyle = FlatStyle.Flat
        btnScriere.Name = "btnScriere"
        btnScriere.Size = New Size(268, 30)
        btnScriere.TabIndex = 6
        btnScriere.Text = "Scriere pe server (POST)"
        btnScriere.UseVisualStyleBackColor = True
        '
        ' btnCoada
        '
        btnCoada.FlatStyle = FlatStyle.Flat
        btnCoada.Name = "btnCoada"
        btnCoada.Size = New Size(268, 30)
        btnCoada.TabIndex = 7
        btnCoada.Text = "Fereastra cozii"
        btnCoada.UseVisualStyleBackColor = True
        '
        ' btnGolesteJurnal
        '
        btnGolesteJurnal.FlatStyle = FlatStyle.Flat
        btnGolesteJurnal.Name = "btnGolesteJurnal"
        btnGolesteJurnal.Size = New Size(268, 30)
        btnGolesteJurnal.TabIndex = 8
        btnGolesteJurnal.Text = "Golește jurnalul"
        btnGolesteJurnal.UseVisualStyleBackColor = True
        '
        ' lstCoduri
        '
        lstCoduri.Dock = DockStyle.Top
        lstCoduri.IntegralHeight = False
        lstCoduri.Items.AddRange(New Object() {"PROBA-0001", "PROBA-0002", "PROBA-0003", "PROBA-0004", "PROBA-0005", "PROBA-0006"})
        lstCoduri.Location = New Point(8, 22)
        lstCoduri.Name = "lstCoduri"
        lstCoduri.SelectionMode = SelectionMode.MultiExtended
        lstCoduri.Size = New Size(274, 164)
        lstCoduri.TabIndex = 1
        '
        ' lblCoduri
        '
        lblCoduri.Dock = DockStyle.Top
        lblCoduri.Location = New Point(8, 0)
        lblCoduri.Name = "lblCoduri"
        lblCoduri.Size = New Size(274, 22)
        lblCoduri.TabIndex = 0
        lblCoduri.Text = "Angajamente de probă:"
        lblCoduri.TextAlign = ContentAlignment.MiddleLeft
        '
        ' pnlJos -- the verdict.
        '
        pnlJos.Controls.Add(btnReusit)
        pnlJos.Controls.Add(btnEsuat)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 592)
        pnlJos.Name = "pnlJos"
        pnlJos.Size = New Size(980, 48)
        pnlJos.TabIndex = 3
        '
        ' btnReusit
        '
        btnReusit.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnReusit.FlatStyle = FlatStyle.Flat
        btnReusit.Location = New Point(706, 8)
        btnReusit.Name = "btnReusit"
        btnReusit.Size = New Size(130, 32)
        btnReusit.TabIndex = 0
        btnReusit.Text = "Merge corect"
        btnReusit.UseVisualStyleBackColor = True
        '
        ' btnEsuat
        '
        btnEsuat.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnEsuat.FlatStyle = FlatStyle.Flat
        btnEsuat.Location = New Point(842, 8)
        btnEsuat.Name = "btnEsuat"
        btnEsuat.Size = New Size(130, 32)
        btnEsuat.TabIndex = 1
        btnEsuat.Text = "Nu merge"
        btnEsuat.UseVisualStyleBackColor = True
        '
        ' txtJurnal -- everything that happened, with the time, client and server side.
        '
        txtJurnal.BorderStyle = BorderStyle.None
        txtJurnal.Dock = DockStyle.Fill
        txtJurnal.Font = New Font("Consolas", 9F)
        txtJurnal.Location = New Point(290, 76)
        txtJurnal.Multiline = True
        txtJurnal.Name = "txtJurnal"
        txtJurnal.ReadOnly = True
        txtJurnal.ScrollBars = ScrollBars.Vertical
        txtJurnal.Size = New Size(690, 516)
        txtJurnal.TabIndex = 2
        '
        ' RobotQueueHarnessForm -- reverse dock order: txtJurnal (Fill) first.
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(980, 640)
        Controls.Add(txtJurnal)
        Controls.Add(pnlStanga)
        Controls.Add(pnlJos)
        Controls.Add(pnlSus)
        MinimumSize = New Size(900, 560)
        Name = "RobotQueueHarnessForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Coada robotului — banc de probă (FOREXE și server simulate)"
        pnlSus.ResumeLayout(False)
        pnlSus.PerformLayout()
        CType(numDurata, ComponentModel.ISupportInitialize).EndInit()
        CType(numServer, ComponentModel.ISupportInitialize).EndInit()
        pnlStanga.ResumeLayout(False)
        flpComenzi.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        '
        ' tips
        '
        tips.SetToolTipHeader(btnReimprospatare, "Reîmprospătare")
        tips.SetToolTipText(btnReimprospatare, "Pune în coadă câte o sarcină pentru fiecare nod selectat, în ordinea din listă:" & vbLf & "citire (GET) → robot simulat → propunere (POST) → «Asociere» → salvare (POST).")
        tips.SetToolTipHeader(btnRafala, "La rând")
        tips.SetToolTipText(btnRafala, "Pune în coadă toate cele șase noduri, unul după altul, ca o serie de clicuri rapide.")
        tips.SetToolTipHeader(btnDublura, "Dublură")
        tips.SetToolTipText(btnDublura, "Cere același nod de două ori: a doua cerere trebuie refuzată.")
        tips.SetToolTipHeader(btnOperatiune, "Operațiune FOREXE")
        tips.SetToolTipText(btnOperatiune, "Simulează o operațiune salvată în pagina FOREXE: intră ÎN FAȚA sarcinilor care așteaptă.")
        tips.SetToolTipHeader(btnDirect, "În afara cozii")
        tips.SetToolTipText(btnDirect, "Pornește robotul direct (ca «Conectare» sau vederea «Browser FOREXE»)." & vbLf & "Coada trebuie să aștepte să se termine înainte de sarcina următoare.")
        tips.SetToolTipHeader(btnCitire, "Citire")
        tips.SetToolTipText(btnCitire, "Un GET spre serverul simulat: trece imediat, chiar dacă robotul lucrează.")
        tips.SetToolTipHeader(btnScriere, "Scriere")
        tips.SetToolTipText(btnScriere, "Un POST spre serverul simulat: așteaptă la poartă cât lucrează robotul.")
        tips.SetToolTipHeader(btnCoada, "Fereastra cozii")
        tips.SetToolTipText(btnCoada, "Deschide fereastra reală «Coada robotului», legată de coada bancului.")
        tips.SetToolTipHeader(btnReusit, "Verdict")
        tips.SetToolTipText(btnReusit, "Închide bancul: comportamentul a fost cel așteptat.")
        tips.SetToolTipHeader(btnEsuat, "Verdict")
        tips.SetToolTipText(btnEsuat, "Închide bancul: ceva nu a mers (descrieți în jurnalul harness-ului).")
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlSus As Panel
    Friend WithEvents lblDurata As Label
    Friend WithEvents numDurata As NumericUpDown
    Friend WithEvents lblServer As Label
    Friend WithEvents numServer As NumericUpDown
    Friend WithEvents chkAsociere As CheckBox
    Friend WithEvents chkEsec As CheckBox
    Friend WithEvents lblPoarta As Label
    Friend WithEvents lblRobot As Label
    Friend WithEvents pnlStanga As Panel
    Friend WithEvents flpComenzi As FlowLayoutPanel
    Friend WithEvents btnReimprospatare As Button
    Friend WithEvents btnRafala As Button
    Friend WithEvents btnDublura As Button
    Friend WithEvents btnOperatiune As Button
    Friend WithEvents btnDirect As Button
    Friend WithEvents btnCitire As Button
    Friend WithEvents btnScriere As Button
    Friend WithEvents btnCoada As Button
    Friend WithEvents btnGolesteJurnal As Button
    Friend WithEvents lstCoduri As ListBox
    Friend WithEvents lblCoduri As Label
    Friend WithEvents pnlJos As Panel
    Friend WithEvents btnReusit As Button
    Friend WithEvents btnEsuat As Button
    Friend WithEvents txtJurnal As TextBox
End Class
#End If
