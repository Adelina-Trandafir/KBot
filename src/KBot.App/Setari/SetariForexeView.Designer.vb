Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariForexeView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        lblTitluStare = New Label()
        tlyStare = New KBotTableLayoutPanel()
        lblConexiuneCaption = New Label()
        lblConexiune = New Label()
        lblCertificatCaption = New Label()
        lblCertificat = New Label()
        lblTitluCertificat = New Label()
        tlyCertificat = New KBotTableLayoutPanel()
        chkUitaCertificatLaUnitate = New CheckBox()
        lblCertMemoratCaption = New Label()
        lblCertMemorat = New Label()
        btnUitaCertificat = New Button()
        lblTitluBrowser = New Label()
        tlyBrowser = New KBotTableLayoutPanel()
        chkHideChrome = New CheckBox()
        chkDevTools = New CheckBox()
        chkMeniuPagina = New CheckBox()
        tlyCaptura = New KBotTableLayoutPanel()
        lblCapturaCaption = New Label()
        cmbCaptura = New KBotComboBox()
        lblTitluFoldere = New Label()
        tlyFoldere = New KBotTableLayoutPanel()
        lblWorkflowsCaption = New Label()
        lblWorkflows = New Label()
        lblRezultateCaption = New Label()
        lblRezultate = New Label()
        lblExtraseCaption = New Label()
        lblExtrase = New Label()
        lblFoldereHint = New Label()
        lblTitluViteza = New Label()
        tlyViteza = New KBotTableLayoutPanel()
        lblVitezaCaption = New Label()
        lblViteza = New Label()
        btnTesteazaViteza = New Button()
        chkValidareDubla = New CheckBox()
        lblMultiplicatorCaption = New Label()
        cmbMultiplicator = New KBotComboBox()
        lblVitezaHint = New Label()
        lblTitleTests = New Label()
        tlyTests = New KBotTableLayoutPanel()
        chkDryRun = New CheckBox()
        chkReplay = New CheckBox()
        lblTestsHint = New Label()
        lblAnswersCaption = New Label()
        lblAnswers = New Label()
        tlyBody.SuspendLayout()
        tlyStare.SuspendLayout()
        tlyCertificat.SuspendLayout()
        tlyBrowser.SuspendLayout()
        tlyViteza.SuspendLayout()
        tlyFoldere.SuspendLayout()
        tlyTests.SuspendLayout()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.AutoScroll = True
        tlyBody.ColumnCount = 1
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.Controls.Add(lblTitluStare, 0, 0)
        tlyBody.Controls.Add(tlyStare, 0, 1)
        tlyBody.Controls.Add(lblTitluCertificat, 0, 2)
        tlyBody.Controls.Add(tlyCertificat, 0, 3)
        tlyBody.Controls.Add(lblTitluBrowser, 0, 4)
        tlyBody.Controls.Add(tlyBrowser, 0, 5)
        tlyBody.Controls.Add(lblTitluViteza, 0, 6)
        tlyBody.Controls.Add(tlyViteza, 0, 7)
        tlyBody.Controls.Add(lblTitleTests, 0, 8)
        tlyBody.Controls.Add(tlyTests, 0, 9)
        tlyBody.Controls.Add(lblTitluFoldere, 0, 10)
        tlyBody.Controls.Add(tlyFoldere, 0, 11)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(0, 0)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.Padding = New Padding(24, 18, 24, 18)
        tlyBody.RowCount = 13
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle())
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(960, 840)
        tlyBody.TabIndex = 0
        '
        ' lblTitluStare
        '
        lblTitluStare.AutoSize = True
        lblTitluStare.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluStare.Location = New Point(28, 18)
        lblTitluStare.Margin = New Padding(4, 0, 4, 8)
        lblTitluStare.Name = "lblTitluStare"
        lblTitluStare.Size = New Size(160, 32)
        lblTitluStare.TabIndex = 0
        lblTitluStare.Text = "Starea robotului"
        '
        ' tlyStare
        '
        tlyStare.AutoFitToTheme = False
        tlyStare.AutoSize = True
        tlyStare.ColumnCount = 2
        tlyStare.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyStare.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStare.Controls.Add(lblConexiuneCaption, 0, 0)
        tlyStare.Controls.Add(lblConexiune, 1, 0)
        tlyStare.Controls.Add(lblCertificatCaption, 0, 1)
        tlyStare.Controls.Add(lblCertificat, 1, 1)
        tlyStare.Dock = DockStyle.Top
        tlyStare.Location = New Point(28, 58)
        tlyStare.Margin = New Padding(4, 0, 4, 24)
        tlyStare.Name = "tlyStare"
        tlyStare.RowCount = 2
        tlyStare.RowStyles.Add(New RowStyle())
        tlyStare.RowStyles.Add(New RowStyle())
        tlyStare.Size = New Size(904, 64)
        tlyStare.TabIndex = 1
        '
        ' lblConexiuneCaption
        '
        lblConexiuneCaption.AutoSize = True
        lblConexiuneCaption.Dock = DockStyle.Fill
        lblConexiuneCaption.Location = New Point(4, 0)
        lblConexiuneCaption.Margin = New Padding(4, 0, 4, 6)
        lblConexiuneCaption.Name = "lblConexiuneCaption"
        lblConexiuneCaption.Size = New Size(252, 26)
        lblConexiuneCaption.TabIndex = 0
        lblConexiuneCaption.Text = "Sesiune FOREXE"
        '
        ' lblConexiune
        '
        lblConexiune.AutoSize = True
        lblConexiune.Dock = DockStyle.Fill
        lblConexiune.Font = New Font("Segoe UI Semibold", 9F)
        lblConexiune.Location = New Point(264, 0)
        lblConexiune.Margin = New Padding(4, 0, 4, 6)
        lblConexiune.Name = "lblConexiune"
        lblConexiune.Size = New Size(636, 26)
        lblConexiune.TabIndex = 1
        lblConexiune.Text = "—"
        '
        ' lblCertificatCaption
        '
        lblCertificatCaption.AutoSize = True
        lblCertificatCaption.Dock = DockStyle.Fill
        lblCertificatCaption.Location = New Point(4, 32)
        lblCertificatCaption.Margin = New Padding(4, 0, 4, 6)
        lblCertificatCaption.Name = "lblCertificatCaption"
        lblCertificatCaption.Size = New Size(252, 26)
        lblCertificatCaption.TabIndex = 2
        lblCertificatCaption.Text = "Certificatul sesiunii"
        '
        ' lblCertificat
        '
        lblCertificat.AutoSize = True
        lblCertificat.Dock = DockStyle.Fill
        lblCertificat.Font = New Font("Segoe UI Semibold", 9F)
        lblCertificat.Location = New Point(264, 32)
        lblCertificat.Margin = New Padding(4, 0, 4, 6)
        lblCertificat.Name = "lblCertificat"
        lblCertificat.Size = New Size(636, 26)
        lblCertificat.TabIndex = 3
        lblCertificat.Text = "—"
        '
        ' lblTitluCertificat
        '
        lblTitluCertificat.AutoSize = True
        lblTitluCertificat.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluCertificat.Location = New Point(28, 146)
        lblTitluCertificat.Margin = New Padding(4, 0, 4, 8)
        lblTitluCertificat.Name = "lblTitluCertificat"
        lblTitluCertificat.Size = New Size(220, 32)
        lblTitluCertificat.TabIndex = 2
        lblTitluCertificat.Text = "Certificatul memorat"
        '
        ' tlyCertificat
        '
        tlyCertificat.AutoFitToTheme = False
        tlyCertificat.AutoSize = True
        tlyCertificat.ColumnCount = 3
        tlyCertificat.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyCertificat.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCertificat.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyCertificat.Controls.Add(lblCertMemoratCaption, 0, 0)
        tlyCertificat.Controls.Add(lblCertMemorat, 1, 0)
        tlyCertificat.Controls.Add(btnUitaCertificat, 2, 0)
        tlyCertificat.Controls.Add(chkUitaCertificatLaUnitate, 0, 1)
        tlyCertificat.Dock = DockStyle.Top
        tlyCertificat.Location = New Point(28, 186)
        tlyCertificat.Margin = New Padding(4, 0, 4, 24)
        tlyCertificat.Name = "tlyCertificat"
        tlyCertificat.RowCount = 2
        tlyCertificat.RowStyles.Add(New RowStyle())
        tlyCertificat.RowStyles.Add(New RowStyle())
        tlyCertificat.Size = New Size(904, 50)
        tlyCertificat.TabIndex = 3
        '
        ' chkUitaCertificatLaUnitate
        '
        chkUitaCertificatLaUnitate.AutoSize = True
        tlyCertificat.SetColumnSpan(chkUitaCertificatLaUnitate, 3)
        chkUitaCertificatLaUnitate.Location = New Point(4, 60)
        chkUitaCertificatLaUnitate.Margin = New Padding(4, 10, 4, 0)
        chkUitaCertificatLaUnitate.Name = "chkUitaCertificatLaUnitate"
        chkUitaCertificatLaUnitate.Size = New Size(420, 29)
        chkUitaCertificatLaUnitate.TabIndex = 3
        chkUitaCertificatLaUnitate.Text = "Uită certificatul memorat când schimb unitatea din bara de titlu"
        tips.SetToolTipHeader(chkUitaCertificatLaUnitate, "Certificatul la schimbarea unității")
        tips.SetToolTipText(chkUitaCertificatLaUnitate, "La schimbarea unității, conexiunea FOREXE se închide oricum." & vbLf & "Bifată: se uită și certificatul memorat, iar «Conectare» ți-l cere din nou" & vbLf & "(folositoare când unitățile au certificate diferite).")
        chkUitaCertificatLaUnitate.UseVisualStyleBackColor = True
        '
        ' lblCertMemoratCaption
        '
        lblCertMemoratCaption.AutoSize = True
        lblCertMemoratCaption.Dock = DockStyle.Fill
        lblCertMemoratCaption.Location = New Point(4, 0)
        lblCertMemoratCaption.Margin = New Padding(4, 0, 4, 0)
        lblCertMemoratCaption.Name = "lblCertMemoratCaption"
        lblCertMemoratCaption.Size = New Size(252, 50)
        lblCertMemoratCaption.TabIndex = 0
        lblCertMemoratCaption.Text = "Propus la următoarea conectare"
        lblCertMemoratCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblCertMemorat
        '
        lblCertMemorat.AutoSize = True
        lblCertMemorat.Dock = DockStyle.Fill
        lblCertMemorat.Font = New Font("Segoe UI Semibold", 9F)
        lblCertMemorat.Location = New Point(264, 0)
        lblCertMemorat.Margin = New Padding(4, 0, 4, 0)
        lblCertMemorat.Name = "lblCertMemorat"
        lblCertMemorat.Size = New Size(388, 50)
        lblCertMemorat.TabIndex = 1
        lblCertMemorat.Text = "—"
        lblCertMemorat.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnUitaCertificat
        '
        btnUitaCertificat.Dock = DockStyle.Fill
        btnUitaCertificat.Enabled = False
        btnUitaCertificat.FlatStyle = FlatStyle.Flat
        btnUitaCertificat.Font = New Font("Segoe UI Semibold", 9F)
        btnUitaCertificat.Location = New Point(664, 0)
        btnUitaCertificat.Margin = New Padding(4, 0, 4, 0)
        btnUitaCertificat.Name = "btnUitaCertificat"
        btnUitaCertificat.Size = New Size(236, 50)
        btnUitaCertificat.TabIndex = 2
        btnUitaCertificat.Text = "Uită certificatul"
        tips.SetToolTipHeader(btnUitaCertificat, "Uită certificatul")
        tips.SetToolTipText(btnUitaCertificat, "Șterge certificatul memorat (doar partea publică, din AppData)." & vbLf & "La următoarea conectare se alege din nou.")
        btnUitaCertificat.UseVisualStyleBackColor = True
        '
        ' lblTitluBrowser
        '
        lblTitluBrowser.AutoSize = True
        lblTitluBrowser.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluBrowser.Location = New Point(28, 260)
        lblTitluBrowser.Margin = New Padding(4, 0, 4, 8)
        lblTitluBrowser.Name = "lblTitluBrowser"
        lblTitluBrowser.Size = New Size(98, 32)
        lblTitluBrowser.TabIndex = 4
        lblTitluBrowser.Text = "Browserul"
        '
        ' tlyBrowser
        '
        tlyBrowser.AutoFitToTheme = False
        tlyBrowser.AutoSize = True
        tlyBrowser.ColumnCount = 1
        tlyBrowser.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBrowser.Controls.Add(chkHideChrome, 0, 0)
        tlyBrowser.Controls.Add(chkDevTools, 0, 1)
        tlyBrowser.Controls.Add(chkMeniuPagina, 0, 2)
        tlyBrowser.Controls.Add(tlyCaptura, 0, 3)
        tlyBrowser.Dock = DockStyle.Top
        tlyBrowser.Location = New Point(28, 300)
        tlyBrowser.Margin = New Padding(4, 0, 4, 24)
        tlyBrowser.Name = "tlyBrowser"
        tlyBrowser.RowCount = 4
        tlyBrowser.RowStyles.Add(New RowStyle())
        tlyBrowser.RowStyles.Add(New RowStyle())
        tlyBrowser.RowStyles.Add(New RowStyle())
        tlyBrowser.RowStyles.Add(New RowStyle())
        tlyBrowser.Size = New Size(904, 163)
        tlyBrowser.TabIndex = 5
        '
        ' tlyCaptura
        '
        tlyCaptura.AutoFitToTheme = False
        tlyCaptura.AutoSize = True
        tlyCaptura.ColumnCount = 2
        tlyCaptura.ColumnStyles.Add(New ColumnStyle())
        tlyCaptura.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCaptura.Controls.Add(lblCapturaCaption, 0, 0)
        tlyCaptura.Controls.Add(cmbCaptura, 1, 0)
        tlyCaptura.Location = New Point(4, 117)
        tlyCaptura.Margin = New Padding(0, 0, 0, 0)
        tlyCaptura.Name = "tlyCaptura"
        tlyCaptura.RowCount = 1
        tlyCaptura.RowStyles.Add(New RowStyle())
        tlyCaptura.Size = New Size(900, 46)
        tlyCaptura.TabIndex = 3
        '
        ' lblCapturaCaption
        '
        lblCapturaCaption.Anchor = AnchorStyles.Left
        lblCapturaCaption.AutoSize = True
        lblCapturaCaption.Location = New Point(4, 8)
        lblCapturaCaption.Margin = New Padding(4, 0, 12, 0)
        lblCapturaCaption.Name = "lblCapturaCaption"
        lblCapturaCaption.Size = New Size(300, 29)
        lblCapturaCaption.TabIndex = 0
        lblCapturaCaption.Text = "Capturile pentru documente arată:"
        '
        ' cmbCaptura
        '
        cmbCaptura.Anchor = AnchorStyles.Left
        cmbCaptura.Location = New Point(320, 4)
        cmbCaptura.Margin = New Padding(4, 4, 4, 4)
        cmbCaptura.Name = "cmbCaptura"
        cmbCaptura.Size = New Size(420, 34)
        cmbCaptura.TabIndex = 1
        tips.SetToolTipHeader(cmbCaptura, "Capturile din FOREXE")
        tips.SetToolTipText(cmbCaptura, "Pagina originală: regulile dumneavoastră de stil sunt ridicate pentru poză," & vbLf &
                            "deci documentul arată pagina așa cum o trimite FOREXE (ca exemplele din ghid)." & vbLf &
                            "Așa cum se vede: poza păstrează regulile." & vbLf &
                            "În ambele cazuri poza nu conține meniul K-BOT și nu e întunecată.")
        '
        ' chkHideChrome
        '
        chkHideChrome.AutoSize = True
        chkHideChrome.Checked = True
        chkHideChrome.CheckState = CheckState.Checked
        chkHideChrome.Location = New Point(4, 0)
        chkHideChrome.Margin = New Padding(4, 0, 4, 10)
        chkHideChrome.Name = "chkHideChrome"
        chkHideChrome.Size = New Size(420, 29)
        chkHideChrome.TabIndex = 0
        chkHideChrome.Text = "În vizualizator, bara browserului (file, adresă) rămâne în afara panoului"
        tips.SetToolTipHeader(chkHideChrome, "Bara browserului")
        tips.SetToolTipText(chkHideChrome, "Bifat, operatorul vede doar pagina FOREXE." & vbLf & "Debifat, apare și bara Chromium. Se aplică de la următoarea lucrare.")
        chkHideChrome.UseVisualStyleBackColor = True
        '
        ' chkDevTools
        '
        chkDevTools.AutoSize = True
        chkDevTools.Location = New Point(4, 39)
        chkDevTools.Margin = New Padding(4, 0, 4, 10)
        chkDevTools.Name = "chkDevTools"
        chkDevTools.Size = New Size(420, 29)
        chkDevTools.TabIndex = 1
        chkDevTools.Text = "Permite instrumentele pentru dezvoltatori în pagina FOREXE (F12, Ctrl+Shift+I, clic dreapta)"
        tips.SetToolTipHeader(chkDevTools, "Instrumente pentru dezvoltatori")
        tips.SetToolTipText(chkDevTools, "Debifat, pagina înghite F12, Ctrl+Shift+I / J / C, Ctrl+U și meniul de clic dreapta." & vbLf & "Bifat, toate rămân la îndemână. Se aplică imediat în pagina deschisă.")
        chkDevTools.UseVisualStyleBackColor = True
        '
        ' chkMeniuPagina
        '
        chkMeniuPagina.AutoSize = True
        chkMeniuPagina.Checked = True
        chkMeniuPagina.CheckState = CheckState.Checked
        chkMeniuPagina.Location = New Point(4, 78)
        chkMeniuPagina.Margin = New Padding(4, 0, 4, 10)
        chkMeniuPagina.Name = "chkMeniuPagina"
        chkMeniuPagina.Size = New Size(420, 29)
        chkMeniuPagina.TabIndex = 2
        chkMeniuPagina.Text = "Arată mini-meniul K-BOT în pagina FOREXE (mărire / micșorare, starea urmăririi)"
        tips.SetToolTipHeader(chkMeniuPagina, "Mini-meniul K-BOT din pagină")
        tips.SetToolTipText(chkMeniuPagina, "Bifat: în colțul paginii FOREXE stă micul meniu K-BOT (− / + / 100% și starea urmăririi)." & vbLf & "Debifat: meniul nu se mai desenează; K-BOT urmărește în continuare ce salvați." & vbLf & "Se aplică imediat în pagina deschisă.")
        chkMeniuPagina.UseVisualStyleBackColor = True
        '
        ' lblTitluViteza
        '
        lblTitluViteza.AutoSize = True
        lblTitluViteza.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluViteza.Location = New Point(28, 448)
        lblTitluViteza.Margin = New Padding(4, 0, 4, 8)
        lblTitluViteza.Name = "lblTitluViteza"
        lblTitluViteza.Size = New Size(380, 32)
        lblTitluViteza.TabIndex = 6
        lblTitluViteza.Text = "Viteza internetului și așteptările"
        '
        ' tlyViteza
        '
        tlyViteza.AutoFitToTheme = False
        tlyViteza.AutoSize = True
        tlyViteza.ColumnCount = 3
        tlyViteza.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyViteza.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyViteza.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 240F))
        tlyViteza.Controls.Add(lblVitezaCaption, 0, 0)
        tlyViteza.Controls.Add(lblViteza, 1, 0)
        tlyViteza.Controls.Add(btnTesteazaViteza, 2, 0)
        tlyViteza.Controls.Add(chkValidareDubla, 0, 1)
        tlyViteza.Controls.Add(lblMultiplicatorCaption, 0, 2)
        tlyViteza.Controls.Add(cmbMultiplicator, 1, 2)
        tlyViteza.Controls.Add(lblVitezaHint, 0, 3)
        tlyViteza.Dock = DockStyle.Top
        tlyViteza.Location = New Point(28, 488)
        tlyViteza.Margin = New Padding(4, 0, 4, 24)
        tlyViteza.Name = "tlyViteza"
        tlyViteza.RowCount = 4
        tlyViteza.RowStyles.Add(New RowStyle())
        tlyViteza.RowStyles.Add(New RowStyle())
        tlyViteza.RowStyles.Add(New RowStyle())
        tlyViteza.RowStyles.Add(New RowStyle())
        tlyViteza.Size = New Size(904, 180)
        tlyViteza.TabIndex = 7
        '
        ' lblVitezaCaption
        '
        lblVitezaCaption.AutoSize = True
        lblVitezaCaption.Dock = DockStyle.Fill
        lblVitezaCaption.Location = New Point(4, 0)
        lblVitezaCaption.Margin = New Padding(4, 0, 4, 10)
        lblVitezaCaption.Name = "lblVitezaCaption"
        lblVitezaCaption.Size = New Size(252, 50)
        lblVitezaCaption.TabIndex = 0
        lblVitezaCaption.Text = "Viteza măsurată (fast.com)"
        lblVitezaCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblViteza
        '
        lblViteza.AutoSize = True
        lblViteza.Dock = DockStyle.Fill
        lblViteza.Font = New Font("Segoe UI Semibold", 9F)
        lblViteza.Location = New Point(264, 0)
        lblViteza.Margin = New Padding(4, 0, 4, 10)
        lblViteza.Name = "lblViteza"
        lblViteza.Size = New Size(388, 50)
        lblViteza.TabIndex = 1
        lblViteza.Text = "—"
        lblViteza.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnTesteazaViteza
        '
        btnTesteazaViteza.Dock = DockStyle.Fill
        btnTesteazaViteza.FlatStyle = FlatStyle.Flat
        btnTesteazaViteza.Font = New Font("Segoe UI Semibold", 9F)
        btnTesteazaViteza.Location = New Point(664, 0)
        btnTesteazaViteza.Margin = New Padding(4, 0, 4, 10)
        btnTesteazaViteza.Name = "btnTesteazaViteza"
        btnTesteazaViteza.Size = New Size(236, 50)
        btnTesteazaViteza.TabIndex = 2
        btnTesteazaViteza.Text = "Testează viteza"
        tips.SetToolTipHeader(btnTesteazaViteza, "Testează viteza")
        tips.SetToolTipText(btnTesteazaViteza, "Deschide fast.com într-un browser ascuns și citește viteza de descărcare." & vbLf & "Durează de obicei 10–30 de secunde.")
        btnTesteazaViteza.UseVisualStyleBackColor = True
        '
        ' chkValidareDubla
        '
        chkValidareDubla.AutoSize = True
        tlyViteza.SetColumnSpan(chkValidareDubla, 3)
        chkValidareDubla.Enabled = False
        chkValidareDubla.Location = New Point(4, 60)
        chkValidareDubla.Margin = New Padding(4, 0, 4, 10)
        chkValidareDubla.Name = "chkValidareDubla"
        chkValidareDubla.Size = New Size(420, 29)
        chkValidareDubla.TabIndex = 3
        chkValidareDubla.Text = "Validează de 2× datele descărcate (fiecare tabel se citește de două ori și se compară)"
        tips.SetToolTipHeader(chkValidareDubla, "Validează de 2×")
        tips.SetToolTipText(chkValidareDubla, "Robotul citește fiecare tabel din FOREXE de două ori și îl mai citește" & vbLf & "până când două citiri ies la fel. Mai lent, dar prinde paginile citite" & vbLf & "înainte să se fi încărcat de tot. Se poate bifa doar pe o conexiune lentă.")
        chkValidareDubla.UseVisualStyleBackColor = True
        '
        ' lblMultiplicatorCaption
        '
        lblMultiplicatorCaption.Anchor = AnchorStyles.Left
        lblMultiplicatorCaption.AutoSize = True
        lblMultiplicatorCaption.Location = New Point(4, 107)
        lblMultiplicatorCaption.Margin = New Padding(4, 0, 4, 0)
        lblMultiplicatorCaption.Name = "lblMultiplicatorCaption"
        lblMultiplicatorCaption.Size = New Size(252, 29)
        lblMultiplicatorCaption.TabIndex = 4
        lblMultiplicatorCaption.Text = "Timpii de așteptare din WFL"
        '
        ' cmbMultiplicator
        '
        cmbMultiplicator.Anchor = AnchorStyles.Left
        tlyViteza.SetColumnSpan(cmbMultiplicator, 2)
        cmbMultiplicator.Location = New Point(264, 103)
        cmbMultiplicator.Margin = New Padding(4, 4, 4, 4)
        cmbMultiplicator.Name = "cmbMultiplicator"
        cmbMultiplicator.Size = New Size(420, 34)
        cmbMultiplicator.TabIndex = 5
        tips.SetToolTipHeader(cmbMultiplicator, "Timpii de așteptare")
        tips.SetToolTipText(cmbMultiplicator, "Înmulțește toți timpii de așteptare scriși în fișierele WFL" & vbLf & "(timeout, pauzele Wait) și așteptarea robotului după Ajax." & vbLf & "Fișierele nu se schimbă. Se aplică de la următoarea lucrare.")
        '
        ' lblVitezaHint
        '
        lblVitezaHint.AutoSize = True
        tlyViteza.SetColumnSpan(lblVitezaHint, 3)
        lblVitezaHint.Dock = DockStyle.Fill
        lblVitezaHint.Location = New Point(4, 147)
        lblVitezaHint.Margin = New Padding(4, 6, 4, 0)
        lblVitezaHint.Name = "lblVitezaHint"
        lblVitezaHint.Size = New Size(896, 26)
        lblVitezaHint.TabIndex = 6
        lblVitezaHint.Text = "—"
        '
        ' lblTitleTests
        '
        lblTitleTests.AutoSize = True
        lblTitleTests.Font = New Font("Segoe UI Semibold", 12F)
        lblTitleTests.Location = New Point(28, 402)
        lblTitleTests.Margin = New Padding(4, 0, 4, 8)
        lblTitleTests.Name = "lblTitleTests"
        lblTitleTests.Size = New Size(250, 32)
        lblTitleTests.TabIndex = 6
        lblTitleTests.Text = "Probă și reîncărcare"
        '
        ' tlyTests
        '
        tlyTests.AutoFitToTheme = False
        tlyTests.AutoSize = True
        tlyTests.ColumnCount = 1
        tlyTests.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyTests.Controls.Add(chkDryRun, 0, 0)
        tlyTests.Controls.Add(chkReplay, 0, 1)
        tlyTests.Controls.Add(lblTestsHint, 0, 2)
        tlyTests.Dock = DockStyle.Top
        tlyTests.Location = New Point(28, 442)
        tlyTests.Margin = New Padding(4, 0, 4, 24)
        tlyTests.Name = "tlyTests"
        tlyTests.RowCount = 3
        tlyTests.RowStyles.Add(New RowStyle())
        tlyTests.RowStyles.Add(New RowStyle())
        tlyTests.RowStyles.Add(New RowStyle())
        tlyTests.Size = New Size(904, 110)
        tlyTests.TabIndex = 7
        '
        ' chkDryRun
        '
        chkDryRun.AutoSize = True
        chkDryRun.Location = New Point(4, 0)
        chkDryRun.Margin = New Padding(4, 0, 4, 10)
        chkDryRun.Name = "chkDryRun"
        chkDryRun.Size = New Size(420, 29)
        chkDryRun.TabIndex = 0
        chkDryRun.Text = "Mod probă: robotul se oprește înainte de orice pas care salvează în FOREXE"
        tips.SetToolTipHeader(chkDryRun, "Mod probă")
        tips.SetToolTipText(chkDryRun, "Robotul parcurge paginile FOREXE reale (formulare, liste, sume, capturi)" & vbLf & "și se oprește chiar înainte de primul clic care ar salva ceva." & vbLf & "Revizia DDF nu își schimbă starea. Se oprește la închiderea K-BOT.")
        chkDryRun.UseVisualStyleBackColor = True
        '
        ' chkReplay
        '
        chkReplay.AutoSize = True
        chkReplay.Location = New Point(4, 39)
        chkReplay.Margin = New Padding(4, 0, 4, 10)
        chkReplay.Name = "chkReplay"
        chkReplay.Size = New Size(420, 29)
        chkReplay.TabIndex = 1
        chkReplay.Text = "Mod reîncărcare: răspunsurile FOREXE se aleg din «Rezultate_Forexe», fără a intra în FOREXE"
        tips.SetToolTipHeader(chkReplay, "Mod reîncărcare")
        tips.SetToolTipText(chkReplay, "Fiecare răspuns FOREXE se păstrează în «Rezultate_Forexe»." & vbLf & "Cu bifa pusă, K-BOT nu mai pornește robotul: pentru fiecare pas cere fișierul" & vbLf & "răspunsului și continuă ca și cum FOREXE ar fi răspuns acum. Se oprește la închiderea K-BOT.")
        chkReplay.UseVisualStyleBackColor = True
        '
        ' lblTestsHint
        '
        lblTestsHint.AutoSize = True
        lblTestsHint.Dock = DockStyle.Fill
        lblTestsHint.Location = New Point(4, 78)
        lblTestsHint.Margin = New Padding(4, 0, 4, 0)
        lblTestsHint.Name = "lblTestsHint"
        lblTestsHint.Size = New Size(896, 26)
        lblTestsHint.TabIndex = 2
        lblTestsHint.Text = "Cele două moduri nu merg împreună și se opresc singure la închiderea K-BOT."
        '
        ' lblTitluFoldere
        '
        lblTitluFoldere.AutoSize = True
        lblTitluFoldere.Font = New Font("Segoe UI Semibold", 12F)
        lblTitluFoldere.Location = New Point(28, 576)
        lblTitluFoldere.Margin = New Padding(4, 0, 4, 8)
        lblTitluFoldere.Name = "lblTitluFoldere"
        lblTitluFoldere.Size = New Size(93, 32)
        lblTitluFoldere.TabIndex = 8
        lblTitluFoldere.Text = "Foldere"
        '
        ' tlyFoldere
        '
        tlyFoldere.AutoFitToTheme = False
        tlyFoldere.AutoSize = True
        tlyFoldere.ColumnCount = 2
        tlyFoldere.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyFoldere.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoldere.Controls.Add(lblWorkflowsCaption, 0, 0)
        tlyFoldere.Controls.Add(lblWorkflows, 1, 0)
        tlyFoldere.Controls.Add(lblRezultateCaption, 0, 1)
        tlyFoldere.Controls.Add(lblRezultate, 1, 1)
        tlyFoldere.Controls.Add(lblExtraseCaption, 0, 2)
        tlyFoldere.Controls.Add(lblExtrase, 1, 2)
        tlyFoldere.Controls.Add(lblAnswersCaption, 0, 3)
        tlyFoldere.Controls.Add(lblAnswers, 1, 3)
        tlyFoldere.Controls.Add(lblFoldereHint, 0, 4)
        tlyFoldere.Dock = DockStyle.Top
        tlyFoldere.Location = New Point(28, 616)
        tlyFoldere.Margin = New Padding(4, 0, 4, 0)
        tlyFoldere.Name = "tlyFoldere"
        tlyFoldere.RowCount = 5
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.RowStyles.Add(New RowStyle())
        tlyFoldere.Size = New Size(904, 162)
        tlyFoldere.TabIndex = 9
        '
        ' lblWorkflowsCaption
        '
        lblWorkflowsCaption.AutoSize = True
        lblWorkflowsCaption.Dock = DockStyle.Fill
        lblWorkflowsCaption.Location = New Point(4, 0)
        lblWorkflowsCaption.Margin = New Padding(4, 0, 4, 6)
        lblWorkflowsCaption.Name = "lblWorkflowsCaption"
        lblWorkflowsCaption.Size = New Size(252, 26)
        lblWorkflowsCaption.TabIndex = 0
        lblWorkflowsCaption.Text = "Definițiile de workflow (.wfl)"
        '
        ' lblWorkflows
        '
        lblWorkflows.AutoEllipsis = True
        lblWorkflows.AutoSize = False
        lblWorkflows.Dock = DockStyle.Fill
        lblWorkflows.Font = New Font("Segoe UI Semibold", 9F)
        lblWorkflows.Location = New Point(264, 0)
        lblWorkflows.Margin = New Padding(4, 0, 4, 6)
        lblWorkflows.Name = "lblWorkflows"
        lblWorkflows.Size = New Size(636, 26)
        lblWorkflows.TabIndex = 1
        lblWorkflows.Text = "—"
        '
        ' lblRezultateCaption
        '
        lblRezultateCaption.AutoSize = True
        lblRezultateCaption.Dock = DockStyle.Fill
        lblRezultateCaption.Location = New Point(4, 32)
        lblRezultateCaption.Margin = New Padding(4, 0, 4, 6)
        lblRezultateCaption.Name = "lblRezultateCaption"
        lblRezultateCaption.Size = New Size(252, 26)
        lblRezultateCaption.TabIndex = 2
        lblRezultateCaption.Text = "Rezultatele brute (JSON)"
        '
        ' lblRezultate
        '
        lblRezultate.AutoEllipsis = True
        lblRezultate.AutoSize = False
        lblRezultate.Dock = DockStyle.Fill
        lblRezultate.Font = New Font("Segoe UI Semibold", 9F)
        lblRezultate.Location = New Point(264, 32)
        lblRezultate.Margin = New Padding(4, 0, 4, 6)
        lblRezultate.Name = "lblRezultate"
        lblRezultate.Size = New Size(636, 26)
        lblRezultate.TabIndex = 3
        lblRezultate.Text = "—"
        '
        ' lblExtraseCaption
        '
        lblExtraseCaption.AutoSize = True
        lblExtraseCaption.Dock = DockStyle.Fill
        lblExtraseCaption.Location = New Point(4, 64)
        lblExtraseCaption.Margin = New Padding(4, 0, 4, 6)
        lblExtraseCaption.Name = "lblExtraseCaption"
        lblExtraseCaption.Size = New Size(252, 26)
        lblExtraseCaption.TabIndex = 4
        lblExtraseCaption.Text = "Extrasele de cont (PDF)"
        '
        ' lblExtrase
        '
        lblExtrase.AutoEllipsis = True
        lblExtrase.AutoSize = False
        lblExtrase.Dock = DockStyle.Fill
        lblExtrase.Font = New Font("Segoe UI Semibold", 9F)
        lblExtrase.Location = New Point(264, 64)
        lblExtrase.Margin = New Padding(4, 0, 4, 6)
        lblExtrase.Name = "lblExtrase"
        lblExtrase.Size = New Size(636, 26)
        lblExtrase.TabIndex = 5
        lblExtrase.Text = "—"
        '
        ' lblAnswersCaption
        '
        lblAnswersCaption.AutoSize = True
        lblAnswersCaption.Dock = DockStyle.Fill
        lblAnswersCaption.Location = New Point(4, 96)
        lblAnswersCaption.Margin = New Padding(4, 0, 4, 6)
        lblAnswersCaption.Name = "lblAnswersCaption"
        lblAnswersCaption.Size = New Size(252, 26)
        lblAnswersCaption.TabIndex = 6
        lblAnswersCaption.Text = "Răspunsurile FOREXE (reîncărcabile)"
        '
        ' lblAnswers
        '
        lblAnswers.AutoEllipsis = True
        lblAnswers.AutoSize = False
        lblAnswers.Dock = DockStyle.Fill
        lblAnswers.Font = New Font("Segoe UI Semibold", 9F)
        lblAnswers.Location = New Point(264, 96)
        lblAnswers.Margin = New Padding(4, 0, 4, 6)
        lblAnswers.Name = "lblAnswers"
        lblAnswers.Size = New Size(636, 26)
        lblAnswers.TabIndex = 7
        lblAnswers.Text = "—"
        '
        ' lblFoldereHint
        '
        lblFoldereHint.AutoSize = True
        tlyFoldere.SetColumnSpan(lblFoldereHint, 2)
        lblFoldereHint.Dock = DockStyle.Fill
        lblFoldereHint.Location = New Point(4, 128)
        lblFoldereHint.Margin = New Padding(4, 6, 4, 0)
        lblFoldereHint.Name = "lblFoldereHint"
        lblFoldereHint.Size = New Size(896, 26)
        lblFoldereHint.TabIndex = 8
        lblFoldereHint.Text = "Căile se schimbă din pagina «Aplicație», secțiunea «Foldere»."
        '
        ' SetariForexeView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "SetariForexeView"
        Size = New Size(960, 840)
        tlyBody.ResumeLayout(False)
        tlyBody.PerformLayout()
        tlyStare.ResumeLayout(False)
        tlyStare.PerformLayout()
        tlyCertificat.ResumeLayout(False)
        tlyCertificat.PerformLayout()
        tlyBrowser.ResumeLayout(False)
        tlyBrowser.PerformLayout()
        tlyViteza.ResumeLayout(False)
        tlyViteza.PerformLayout()
        tlyFoldere.ResumeLayout(False)
        tlyFoldere.PerformLayout()
        tlyTests.ResumeLayout(False)
        tlyTests.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents lblTitluStare As Label
    Friend WithEvents tlyStare As KBotTableLayoutPanel
    Friend WithEvents lblConexiuneCaption As Label
    Friend WithEvents lblConexiune As Label
    Friend WithEvents lblCertificatCaption As Label
    Friend WithEvents lblCertificat As Label
    Friend WithEvents lblTitluCertificat As Label
    Friend WithEvents tlyCertificat As KBotTableLayoutPanel
    Friend WithEvents chkUitaCertificatLaUnitate As CheckBox
    Friend WithEvents lblCertMemoratCaption As Label
    Friend WithEvents lblCertMemorat As Label
    Friend WithEvents btnUitaCertificat As Button
    Friend WithEvents lblTitluBrowser As Label
    Friend WithEvents tlyBrowser As KBotTableLayoutPanel
    Friend WithEvents chkHideChrome As CheckBox
    Friend WithEvents chkDevTools As CheckBox
    Friend WithEvents chkMeniuPagina As CheckBox
    Friend WithEvents tlyCaptura As KBotTableLayoutPanel
    Friend WithEvents lblCapturaCaption As Label
    Friend WithEvents cmbCaptura As KBotComboBox
    Friend WithEvents lblTitluFoldere As Label
    Friend WithEvents tlyFoldere As KBotTableLayoutPanel
    Friend WithEvents lblWorkflowsCaption As Label
    Friend WithEvents lblWorkflows As Label
    Friend WithEvents lblRezultateCaption As Label
    Friend WithEvents lblRezultate As Label
    Friend WithEvents lblExtraseCaption As Label
    Friend WithEvents lblExtrase As Label
    Friend WithEvents lblFoldereHint As Label
    Friend WithEvents lblTitluViteza As Label
    Friend WithEvents tlyViteza As KBotTableLayoutPanel
    Friend WithEvents lblVitezaCaption As Label
    Friend WithEvents lblViteza As Label
    Friend WithEvents btnTesteazaViteza As Button
    Friend WithEvents chkValidareDubla As CheckBox
    Friend WithEvents lblMultiplicatorCaption As Label
    Friend WithEvents cmbMultiplicator As KBotComboBox
    Friend WithEvents lblVitezaHint As Label
    Friend WithEvents lblTitleTests As Label
    Friend WithEvents tlyTests As KBotTableLayoutPanel
    Friend WithEvents chkDryRun As CheckBox
    Friend WithEvents chkReplay As CheckBox
    Friend WithEvents lblTestsHint As Label
    Friend WithEvents lblAnswersCaption As Label
    Friend WithEvents lblAnswers As Label
End Class
