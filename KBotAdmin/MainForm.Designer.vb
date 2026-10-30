' The main window (slice ME01, ribbon since ME01-05): a ribbon on top, ONE tree on the left (the databases and, under each,
' the accounts linked to it; or the users and, under each, their databases: the bar above the tree chooses), a «select all»
' tick under it, the pages on the right.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md); coordinates are in the 96 dpi the
' form was authored at and AutoScaleDimensions carries the same stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim rbComenzi As KBotRibbonButton = New KBotRibbonButton()
        Dim rbUnice As KBotRibbonButton = New KBotRibbonButton()
        Dim rbSursa As KBotRibbonButton = New KBotRibbonButton()
        Dim rbSincronizare As KBotRibbonButton = New KBotRibbonButton()
        Dim rbDemo As KBotRibbonButton = New KBotRibbonButton()
        Dim rbDrepturi As KBotRibbonButton = New KBotRibbonButton()
        Dim rbRightsAd As KBotRibbonButton = New KBotRibbonButton()
        Dim rbRightsVr As KBotRibbonButton = New KBotRibbonButton()
        Dim rbAcctAdd As KBotRibbonButton = New KBotRibbonButton()
        Dim rbAcctGrant As KBotRibbonButton = New KBotRibbonButton()
        Dim rbAcctRole As KBotRibbonButton = New KBotRibbonButton()
        Dim rbCereri As KBotRibbonButton = New KBotRibbonButton()
        Dim rbLogin As KBotRibbonButton = New KBotRibbonButton()
        Dim rbLoguri As KBotRibbonButton = New KBotRibbonButton()
        Dim rbAudit As KBotRibbonButton = New KBotRibbonButton()
        Dim rbVizite As KBotRibbonButton = New KBotRibbonButton()
        Dim rbServer As KBotRibbonButton = New KBotRibbonButton()
        Dim rbIstoricMd As KBotRibbonButton = New KBotRibbonButton()
        Dim rbSchemaGraf As KBotRibbonButton = New KBotRibbonButton()
        Dim rbSchemaIstoric As KBotRibbonButton = New KBotRibbonButton()
        Dim rbCopii As KBotRibbonButton = New KBotRibbonButton()
        Dim rbCopiiLocale As KBotRibbonButton = New KBotRibbonButton()
        Dim rbPublicare As KBotRibbonButton = New KBotRibbonButton()
        Dim rbRapoarte As KBotRibbonButton = New KBotRibbonButton()
        Dim rbImportMf As KBotRibbonButton = New KBotRibbonButton()
        Dim rgRulare As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgSchema As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgPagina As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgAplicatii As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgConturi As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgCereri As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgJurnale As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgServer As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgProiect As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgPeServer As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgPeCalculator As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgFisiere As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rgVercon As KBotRibbonGroup = New KBotRibbonGroup()
        Dim rtBaze As KBotRibbonTab = New KBotRibbonTab()
        Dim rtDrepturi As KBotRibbonTab = New KBotRibbonTab()
        Dim rtMonitorizare As KBotRibbonTab = New KBotRibbonTab()
        Dim rtCopii As KBotRibbonTab = New KBotRibbonTab()
        Dim rtPublicare As KBotRibbonTab = New KBotRibbonTab()
        tips = New KBotToolTip(components)
        notify = New NotifyIcon(components)
        mnuTray = New ContextMenuStrip(components)
        mnuShow = New ToolStripMenuItem()
        mnuExit = New ToolStripMenuItem()
        mnuTray.SuspendLayout()
        pnlRoot = New Panel()
        capBar = New KBotCaptionBar()
        ribbon = New KBotRibbon()
        pnlStatus = New Panel()
        busy = New KBotBusyBar()
        lblStatus = New Label()
        tlyMain = New KBotTableLayoutPanel()
        splMain = New SplitContainer()
        tlyLeft = New KBotTableLayoutPanel()
        navMode = New KBotNavList()
        treeDb = New AdvancedTreeControl()
        chkAll = New KBotCheckBox()
        pnlViews = New Panel()
        viewCommands = New CommandsView()
        viewSync = New SyncView()
        viewRights = New RightsView()
        viewLogs = New LogsView()
        viewAudit = New AuditView()
        viewVisits = New VisitsView()
        viewServer = New ServerView()
        viewBackups = New BackupsView()
        viewRequests = New RequestsView()
        viewPublish = New PushFilesView()
        viewOnce = New OnceQueryView()
        viewSource = New SourceQueryView()
        viewLogin = New LoginResetView()
        viewVerconReports = New VerconReportsView()
        viewMfImport = New MfImportView()
        viewDemo = New DemoCloneView()
        viewLocalBackup = New LocalBackupView()
        viewMdHistory = New MdHistoryView()
        viewSchemaGraph = New SchemaGraphView()
        viewSchemaHistory = New SchemaHistoryView()
        CType(navMode, ComponentModel.ISupportInitialize).BeginInit()
        pnlRoot.SuspendLayout()
        pnlStatus.SuspendLayout()
        tlyMain.SuspendLayout()
        CType(splMain, ComponentModel.ISupportInitialize).BeginInit()
        splMain.Panel1.SuspendLayout()
        splMain.Panel2.SuspendLayout()
        splMain.SuspendLayout()
        tlyLeft.SuspendLayout()
        pnlViews.SuspendLayout()
        SuspendLayout()
        '
        ' pnlRoot (children in reverse dock order: Fill first, then the Top / Bottom ones, the caption bar last = outermost)
        '
        pnlRoot.Controls.Add(tlyMain)
        pnlRoot.Controls.Add(ribbon)
        pnlRoot.Controls.Add(pnlStatus)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 2)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1278, 716)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        '
        ' capBar
        '
        tips.SetToolTipHeader(capBar, "KBotAdmin")
        tips.SetToolTipText(capBar, "Program privat de administrare. Nu face parte din K-BOT și nu ajunge la clienți.")
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.OnCloseMinimizeToTray = True
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.ShowThemeButton = False
        capBar.Size = New Size(1278, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "KBotAdmin"
        '
        ' ribbon: the pages and the actions, grouped by what they are for
        '
        rbComenzi.Image = AdminIcons.Get("database")
        rbComenzi.Key = "comenzi"
        rbComenzi.Text = "Comenzi"
        rbComenzi.ToolTipHeader = "Comenzi"
        rbComenzi.ToolTipText = "Interogări sigure, hotărâte pe server, pe baza aleasă sau pe toate."
        rbUnice.Image = AdminIcons.Get("once")
        rbUnice.Key = "unice"
        rbUnice.Text = "Interogări unice"
        rbUnice.ToolTipHeader = "Interogări unice"
        rbUnice.ToolTipText = "Rulează o dată un text SQL pe AVACONT_SURSA și pe bazele bifate, cu istoricul a ce a rulat."
        rbSursa.Image = AdminIcons.Get("once")
        rbSursa.Key = "sursa"
        rbSursa.Text = "Interogare pe SURSA"
        rbSursa.ToolTipHeader = "Interogare pe SURSA"
        rbSursa.ToolTipText = "Dai calea unui fișier .sql: se citește, rulează pe AVACONT_SURSA, se verifică în structura ei, apare în graf și bare, iar la confirmare se sincronizează unitățile."
        rgRulare.Buttons.Add(rbComenzi)
        rgRulare.Buttons.Add(rbUnice)
        rgRulare.Buttons.Add(rbSursa)
        rgRulare.Text = "Rulare"
        rbSincronizare.Image = AdminIcons.Get("sync")
        rbSincronizare.Key = "sincronizare"
        rbSincronizare.Text = "Sincronizare"
        rbSincronizare.ToolTipHeader = "Sincronizare schemă"
        rbSincronizare.ToolTipText = "Aduce schema bazelor la schema din AVACONT_SURSA. Previzualizarea e implicită; execuția cere confirmare."
        rbDemo.Image = AdminIcons.Get("clone")
        rbDemo.Key = "demo"
        rbDemo.Text = "Clonare DEMO"
        rbDemo.ToolTipHeader = "Clonare DEMO"
        rbDemo.ToolTipText = "Golește 000_DEMO și copiază în el o altă bază, păstrând codul fiscal și DC-ul lui 000_DEMO."
        rgSchema.Buttons.Add(rbSincronizare)
        rgSchema.Buttons.Add(rbDemo)
        rgSchema.Text = "Schemă și date"
        rtBaze.Groups.Add(rgRulare)
        rtBaze.Groups.Add(rgSchema)
        rtBaze.Text = "Baze de date"
        rbDrepturi.Image = AdminIcons.Get("rights")
        rbDrepturi.Key = "drepturi"
        rbDrepturi.Text = "Conturi și drepturi"
        rbDrepturi.ToolTipHeader = "Conturi și drepturi"
        rbDrepturi.ToolTipText = "Lista conturilor-în-baze și rolul K-BOT al fiecăruia."
        rgPagina.Buttons.Add(rbDrepturi)
        rgPagina.Text = "Pagina"
        rbRightsAd.Image = AdminIcons.Get("rights_ad")
        rbRightsAd.Key = "rights_ad"
        rbRightsAd.Text = "Drepturi ADECHIT"
        rbRightsAd.ToolTipHeader = "Drepturi ADECHIT"
        rbRightsAd.ToolTipText = "Deschide fereastra cu drepturile ADECHIT ale contului din rândul ales."
        rbRightsVr.Image = AdminIcons.Get("rights_vr")
        rbRightsVr.Key = "rights_vr"
        rbRightsVr.Text = "Drepturi VERCON"
        rbRightsVr.ToolTipHeader = "Drepturi VERCON"
        rbRightsVr.ToolTipText = "Deschide fereastra cu drepturile VERCON ale contului din rândul ales."
        rgAplicatii.Buttons.Add(rbRightsAd)
        rgAplicatii.Buttons.Add(rbRightsVr)
        rgAplicatii.Text = "Drepturi pe aplicații"
        rbAcctAdd.Image = AdminIcons.Get("account_add")
        rbAcctAdd.Key = "acct_add"
        rbAcctAdd.Text = "Adaugă cont"
        rbAcctAdd.ToolTipHeader = "Adaugă cont"
        rbAcctAdd.ToolTipText = "Leagă un cont de o bază, cu un rol K-BOT."
        rbAcctGrant.Image = AdminIcons.Get("account_grant")
        rbAcctGrant.Key = "acct_grant"
        rbAcctGrant.Text = "Acces la o bază"
        rbAcctGrant.ToolTipHeader = "Dă acces la o bază"
        rbAcctGrant.ToolTipText = "Dă unui cont existent acces la încă o bază; se oferă doar bazele la care nu are deja acces."
        rbAcctRole.Image = AdminIcons.Get("account_role")
        rbAcctRole.Key = "acct_role"
        rbAcctRole.Text = "Rol K-BOT"
        rbAcctRole.ToolTipHeader = "Rol K-BOT"
        rbAcctRole.ToolTipText = "Schimbă rolul K-BOT al contului din rândul ales, în baza lui."
        rgConturi.Buttons.Add(rbAcctAdd)
        rgConturi.Buttons.Add(rbAcctGrant)
        rgConturi.Buttons.Add(rbAcctRole)
        rgConturi.Text = "Conturi"
        rbCereri.Image = AdminIcons.Get("requests")
        rbCereri.Key = "cereri"
        rbCereri.Text = "Cereri"
        rbCereri.ToolTipHeader = "Cereri de conturi"
        rbCereri.ToolTipText = "Cereri de conturi noi, de aprobat sau respins."
        rbLogin.Image = AdminIcons.Get("login")
        rbLogin.Key = "login"
        rbLogin.Text = "Resetare Login"
        rbLogin.ToolTipHeader = "Resetare Login"
        rbLogin.ToolTipText = "Șterge încercările de login eșuate ale utilizatorilor și cere schimbarea parolei."
        rgCereri.Buttons.Add(rbCereri)
        rgCereri.Buttons.Add(rbLogin)
        rgCereri.Text = "Conturi noi și login"
        rtDrepturi.Groups.Add(rgPagina)
        rtDrepturi.Groups.Add(rgAplicatii)
        rtDrepturi.Groups.Add(rgConturi)
        rtDrepturi.Groups.Add(rgCereri)
        rtDrepturi.Text = "Drepturi"
        rbLoguri.Image = AdminIcons.Get("logs")
        rbLoguri.Key = "loguri"
        rbLoguri.Text = "Loguri"
        rbLoguri.ToolTipHeader = "Loguri"
        rbLoguri.ToolTipText = "Jurnalele serverului, colorate: api, vba, forexe, asociere, schema_sync, gunicorn, ajutor."
        rbAudit.Image = AdminIcons.Get("journal")
        rbAudit.Key = "audit"
        rbAudit.Text = "Jurnal"
        rbAudit.ToolTipHeader = "Jurnal"
        rbAudit.ToolTipText = "Ce s-a făcut în conturi: autentificări, eșecuri, acțiunile KBotAdmin."
        rbVizite.Image = AdminIcons.Get("visits")
        rbVizite.Key = "vizite"
        rbVizite.Text = "Vizite"
        rbVizite.ToolTipHeader = "Vizite"
        rbVizite.ToolTipText = "Cine a deschis pagina de prezentare."
        rgJurnale.Buttons.Add(rbLoguri)
        rgJurnale.Buttons.Add(rbAudit)
        rgJurnale.Buttons.Add(rbVizite)
        rgJurnale.Text = "Jurnale"
        rbServer.Image = AdminIcons.Get("server")
        rbServer.Key = "server"
        rbServer.Text = "Server"
        rbServer.ToolTipHeader = "Server"
        rbServer.ToolTipText = "Disc, memorie, certificat, servicii."
        rgServer.Buttons.Add(rbServer)
        rgServer.Text = "Server"
        rbIstoricMd.Image = AdminIcons.Get("md_history")
        rbIstoricMd.Key = "istoric-md"
        rbIstoricMd.Text = "Istoric md"
        rbIstoricMd.ToolTipHeader = "Istoric md"
        rbIstoricMd.ToolTipText = "Cum a evoluat proiectul, doar din fișierele md: cronologie pe felii și graf al legăturilor, cu un cursor de timp."
        rgProiect.Buttons.Add(rbIstoricMd)
        rbSchemaGraf.Image = AdminIcons.Get("database")
        rbSchemaGraf.Key = "schema-graf"
        rbSchemaGraf.Text = "Schema bazelor"
        rbSchemaGraf.ToolTipHeader = "Schema bazelor"
        rbSchemaGraf.ToolTipText = "Tabelele schemelor SURSA și COMUN ca graf, cu cheile străine drept linii (din dump-urile din MariaDB_Schema)."
        rgProiect.Buttons.Add(rbSchemaGraf)
        rbSchemaIstoric.Image = AdminIcons.Get("sync")
        rbSchemaIstoric.Key = "schema-istoric"
        rbSchemaIstoric.Text = "Evoluția schemei"
        rbSchemaIstoric.ToolTipHeader = "Evoluția schemei"
        rbSchemaIstoric.ToolTipText = "Ce a schimbat sincronizarea schemei într-o bază, în timp: un rând pe tabel, un punct pe instrucțiune DDL (jurnalul schema_diff_log din dump-ul COMUN)."
        rgProiect.Buttons.Add(rbSchemaIstoric)
        rgProiect.Text = "Proiect"
        rtMonitorizare.Groups.Add(rgJurnale)
        rtMonitorizare.Groups.Add(rgServer)
        rtMonitorizare.Groups.Add(rgProiect)
        rtMonitorizare.Text = "Monitorizare"
        rbCopii.Image = AdminIcons.Get("backup_server")
        rbCopii.Key = "copii"
        rbCopii.Text = "Copii server"
        rbCopii.ToolTipHeader = "Copii server"
        rbCopii.ToolTipText = "Copiile de siguranță ale bazelor, făcute și păstrate pe server."
        rgPeServer.Buttons.Add(rbCopii)
        rgPeServer.Text = "Pe server"
        rbCopiiLocale.Image = AdminIcons.Get("backup_local")
        rbCopiiLocale.Key = "copii-locale"
        rbCopiiLocale.Text = "Copii locale"
        rbCopiiLocale.ToolTipHeader = "Copii locale"
        rbCopiiLocale.ToolTipText = "Copii criptate ale bazelor de unitate, păstrate pe acest calculator, cu istoric."
        rgPeCalculator.Buttons.Add(rbCopiiLocale)
        rgPeCalculator.Text = "Pe acest calculator"
        rtCopii.Groups.Add(rgPeServer)
        rtCopii.Groups.Add(rgPeCalculator)
        rtCopii.Text = "Copii de siguranță"
        rbPublicare.Image = AdminIcons.Get("publish")
        rbPublicare.Key = "publicare"
        rbPublicare.Text = "Publicare"
        rbPublicare.ToolTipHeader = "Publicare"
        rbPublicare.ToolTipText = "Trimite fișierele din PYTHON pe server și repornește serviciul."
        rgFisiere.Buttons.Add(rbPublicare)
        rgFisiere.Text = "Fișiere"
        rbRapoarte.Image = AdminIcons.Get("reports")
        rbRapoarte.Key = "vercon-reports"
        rbRapoarte.Text = "Rapoarte VERCON"
        rbRapoarte.ToolTipHeader = "Rapoarte VERCON"
        rbRapoarte.ToolTipText = "SQL-ul, antetul și machetele rapoartelor VERCON ale unei unități."
        rgVercon.Buttons.Add(rbRapoarte)
        rbImportMf.Image = AdminIcons.Get("database")
        rbImportMf.Key = "mf-import"
        rbImportMf.Text = "Import MF din Access"
        rbImportMf.ToolTipHeader = "Import MF din Access"
        rbImportMf.ToolTipText = "Aduce mijloacele fixe active dintr-un mf.mdb vechi într-o gestiune MF a unei unități, în contul de magazie ales, fără primitor."
        rgVercon.Buttons.Add(rbImportMf)
        rgVercon.Text = "VERCON"
        rtPublicare.Groups.Add(rgFisiere)
        rtPublicare.Groups.Add(rgVercon)
        rtPublicare.Text = "Publicare"
        ribbon.Dock = DockStyle.Top
        ribbon.Location = New Point(0, 44)
        ribbon.Margin = New Padding(0)
        ribbon.Name = "ribbon"
        ribbon.SelectedTabIndex = 0
        ribbon.Size = New Size(1278, 120)
        ribbon.TabIndex = 3
        ribbon.Tabs.Add(rtBaze)
        ribbon.Tabs.Add(rtDrepturi)
        ribbon.Tabs.Add(rtMonitorizare)
        ribbon.Tabs.Add(rtCopii)
        ribbon.Tabs.Add(rtPublicare)
        ribbon.Tooltip = tips
        '
        ' pnlStatus
        '
        pnlStatus.Controls.Add(lblStatus)
        pnlStatus.Controls.Add(busy)
        pnlStatus.Dock = DockStyle.Bottom
        pnlStatus.Location = New Point(0, 688)
        pnlStatus.Margin = New Padding(0)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(1278, 28)
        pnlStatus.TabIndex = 2
        '
        ' busy
        '
        tips.SetToolTipHeader(busy, "Lucrează")
        tips.SetToolTipText(busy, "Bara se mișcă cât timp serverul execută o comandă.")
        busy.Dock = DockStyle.Top
        busy.Location = New Point(0, 0)
        busy.Margin = New Padding(0)
        busy.Name = "busy"
        busy.Size = New Size(1278, 3)
        busy.TabIndex = 0
        busy.TabStop = False
        '
        ' lblStatus
        '
        tips.SetToolTipHeader(lblStatus, "Starea")
        tips.SetToolTipText(lblStatus, "Ultimul lucru făcut sau ultima eroare. Detaliile erorilor sunt în Logs\harness_errors.log.")
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(0, 3)
        lblStatus.Margin = New Padding(0)
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New Padding(8, 0, 8, 0)
        lblStatus.Size = New Size(1278, 25)
        lblStatus.TabIndex = 1
        lblStatus.Text = "Gata."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(splMain, 0, 0)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 164)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8, 8, 8, 4)
        tlyMain.RowCount = 1
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1278, 524)
        tlyMain.TabIndex = 1
        '
        ' splMain: the operator drags the bar between the tree and the pages to resize them
        '
        splMain.Dock = DockStyle.Fill
        splMain.FixedPanel = FixedPanel.Panel1
        splMain.Location = New Point(8, 8)
        splMain.Margin = New Padding(0)
        splMain.Name = "splMain"
        splMain.Panel1.Controls.Add(tlyLeft)
        splMain.Panel1MinSize = 180
        splMain.Panel2.Controls.Add(pnlViews)
        splMain.Panel2MinSize = 400
        splMain.Size = New Size(1262, 512)
        splMain.SplitterDistance = 300
        splMain.SplitterWidth = 8
        splMain.TabIndex = 0
        tips.SetToolTipHeader(splMain, "Lățimea arborelui")
        tips.SetToolTipText(splMain, "Trage de bara dintre arbore și pagini ca să le lărgești sau să le îngustezi.")
        '
        ' tlyLeft: the mode bar glued on top of the tree, the «select all» tick under it
        '
        tlyLeft.AutoFitToTheme = False
        tlyLeft.ColumnCount = 1
        tlyLeft.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyLeft.Controls.Add(navMode, 0, 0)
        tlyLeft.Controls.Add(treeDb, 0, 1)
        tlyLeft.Controls.Add(chkAll, 0, 2)
        tlyLeft.Dock = DockStyle.Fill
        tlyLeft.Location = New Point(0, 0)
        tlyLeft.Margin = New Padding(0)
        tlyLeft.Name = "tlyLeft"
        tlyLeft.RowCount = 3
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tlyLeft.Size = New Size(292, 512)
        tlyLeft.TabIndex = 0
        '
        ' navMode: what the tree shows first: the databases (left) or the users (right)
        '
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Key = "baze"
        KBotNavItem1.Text = "Baze de date"
        KBotNavItem2.Align = KBotNavAlign.Far
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Key = "utilizatori"
        KBotNavItem2.Text = "Utilizatori"
        navMode.Dock = DockStyle.Fill
        navMode.IconSize = 0
        navMode.Items.Add(KBotNavItem1)
        navMode.Items.Add(KBotNavItem2)
        navMode.Location = New Point(0, 0)
        navMode.Margin = New Padding(0)
        navMode.Name = "navMode"
        navMode.Orientation = KBotNavOrientation.Horizontal
        navMode.SelectedKey = "baze"
        navMode.Size = New Size(292, 36)
        navMode.TabIndex = 0
        tips.SetToolTipHeader(navMode, "Ce arată arborele")
        tips.SetToolTipText(navMode, "Baze date: bazele primele, conturile lor dedesubt." & vbLf & "Utilizatori: conturile primele, bazele lor dedesubt.")
        '
        ' treeDb: databases and accounts, with a tick on every row (no title above it)
        '
        treeDb.CheckBoxes = True
        treeDb.Dock = DockStyle.Fill
        treeDb.FooterVisible = False
        treeDb.HeaderVisible = False
        treeDb.ItemHeight = 22
        treeDb.Location = New Point(0, 36)
        treeDb.Margin = New Padding(0)
        treeDb.Name = "treeDb"
        treeDb.RootExpander = False
        treeDb.Size = New Size(292, 446)
        treeDb.TabIndex = 1
        tips.SetToolTipHeader(treeDb, "Bazele de date și conturile lor")
        tips.SetToolTipText(treeDb, "Alege baza pe care lucrează paginile din dreapta." & vbLf & "<TOATE BAZELE> = toate bazele de unitate (fără AVACONT_SURSA și AVACONT_COMUN)." & vbLf & "Un cont legat de mai multe baze apare sub fiecare." & vbLf & "Un clic pe un cont alege baza lui și contul, pentru pagina «Conturi și drepturi»." & vbLf & "Căsuțele bifează rândurile; bifa unui rând cu copii bifează și copiii lui.")
        '
        ' chkAll: ticks (or unticks) every row of the tree
        '
        chkAll.Dock = DockStyle.Fill
        chkAll.Location = New Point(0, 482)
        chkAll.Margin = New Padding(0)
        chkAll.Name = "chkAll"
        chkAll.Size = New Size(292, 30)
        chkAll.TabIndex = 2
        chkAll.Text = "Selectează tot"
        tips.SetToolTipHeader(chkAll, "Selectează tot")
        tips.SetToolTipText(chkAll, "Bifează toate rândurile arborelui; bifat din nou, le debifează.")
        '
        ' pnlViews
        '
        pnlViews.Controls.Add(viewCommands)
        pnlViews.Controls.Add(viewSync)
        pnlViews.Controls.Add(viewRights)
        pnlViews.Controls.Add(viewLogs)
        pnlViews.Controls.Add(viewAudit)
        pnlViews.Controls.Add(viewVisits)
        pnlViews.Controls.Add(viewServer)
        pnlViews.Controls.Add(viewBackups)
        pnlViews.Controls.Add(viewRequests)
        pnlViews.Controls.Add(viewPublish)
        pnlViews.Controls.Add(viewOnce)
        pnlViews.Controls.Add(viewSource)
        pnlViews.Controls.Add(viewLogin)
        pnlViews.Controls.Add(viewVerconReports)
        pnlViews.Controls.Add(viewMfImport)
        pnlViews.Controls.Add(viewDemo)
        pnlViews.Controls.Add(viewLocalBackup)
        pnlViews.Controls.Add(viewMdHistory)
        pnlViews.Controls.Add(viewSchemaGraph)
        pnlViews.Controls.Add(viewSchemaHistory)
        viewSchemaHistory.Dock = DockStyle.Fill
        viewSchemaHistory.Visible = False
        viewSchemaGraph.Dock = DockStyle.Fill
        viewSchemaGraph.Visible = False
        viewMdHistory.Dock = DockStyle.Fill
        viewMdHistory.Visible = False
        viewLocalBackup.Dock = DockStyle.Fill
        viewLocalBackup.Visible = False
        viewDemo.Dock = DockStyle.Fill
        viewDemo.Visible = False
        viewVerconReports.Dock = DockStyle.Fill
        viewVerconReports.Visible = False
        viewMfImport.Dock = DockStyle.Fill
        viewMfImport.Visible = False
        pnlViews.Dock = DockStyle.Fill
        pnlViews.Location = New Point(0, 0)
        pnlViews.Margin = New Padding(0)
        pnlViews.Name = "pnlViews"
        pnlViews.Size = New Size(962, 512)
        pnlViews.TabIndex = 1
        pnlViews.Tag = "Card"
        '
        ' viewCommands
        '
        viewCommands.Dock = DockStyle.Fill
        viewCommands.Location = New Point(0, 0)
        viewCommands.Margin = New Padding(0)
        viewCommands.Name = "viewCommands"
        viewCommands.Size = New Size(962, 512)
        viewCommands.TabIndex = 0
        viewCommands.Visible = False
        '
        ' viewSync
        '
        viewSync.Dock = DockStyle.Fill
        viewSync.Location = New Point(0, 0)
        viewSync.Margin = New Padding(0)
        viewSync.Name = "viewSync"
        viewSync.Size = New Size(962, 512)
        viewSync.TabIndex = 2
        viewSync.Visible = False
        '
        ' viewRights
        '
        viewRights.Dock = DockStyle.Fill
        viewRights.Location = New Point(0, 0)
        viewRights.Margin = New Padding(0)
        viewRights.Name = "viewRights"
        viewRights.Size = New Size(962, 512)
        viewRights.TabIndex = 3
        viewRights.Visible = False
        '
        ' viewLogs
        '
        viewLogs.Dock = DockStyle.Fill
        viewLogs.Location = New Point(0, 0)
        viewLogs.Margin = New Padding(0)
        viewLogs.Name = "viewLogs"
        viewLogs.Size = New Size(962, 512)
        viewLogs.TabIndex = 4
        viewLogs.Visible = False
        '
        ' viewAudit
        '
        viewAudit.Dock = DockStyle.Fill
        viewAudit.Location = New Point(0, 0)
        viewAudit.Margin = New Padding(0)
        viewAudit.Name = "viewAudit"
        viewAudit.Size = New Size(962, 512)
        viewAudit.TabIndex = 5
        viewAudit.Visible = False
        '
        ' viewVisits
        '
        viewVisits.Dock = DockStyle.Fill
        viewVisits.Location = New Point(0, 0)
        viewVisits.Margin = New Padding(0)
        viewVisits.Name = "viewVisits"
        viewVisits.Size = New Size(962, 512)
        viewVisits.TabIndex = 6
        viewVisits.Visible = False
        '
        ' viewServer
        '
        viewServer.Dock = DockStyle.Fill
        viewServer.Location = New Point(0, 0)
        viewServer.Margin = New Padding(0)
        viewServer.Name = "viewServer"
        viewServer.Size = New Size(962, 512)
        viewServer.TabIndex = 7
        viewServer.Visible = False
        '
        ' viewBackups
        '
        viewBackups.Dock = DockStyle.Fill
        viewBackups.Location = New Point(0, 0)
        viewBackups.Margin = New Padding(0)
        viewBackups.Name = "viewBackups"
        viewBackups.Size = New Size(962, 512)
        viewBackups.TabIndex = 8
        viewBackups.Visible = False
        '
        ' viewRequests
        '
        viewRequests.Dock = DockStyle.Fill
        viewRequests.Location = New Point(0, 0)
        viewRequests.Margin = New Padding(0)
        viewRequests.Name = "viewRequests"
        viewRequests.Size = New Size(962, 512)
        viewRequests.TabIndex = 9
        viewRequests.Visible = False
        '
        ' viewPublish
        '
        viewPublish.Dock = DockStyle.Fill
        viewPublish.Location = New Point(0, 0)
        viewPublish.Margin = New Padding(0)
        viewPublish.Name = "viewPublish"
        viewPublish.Size = New Size(962, 512)
        viewPublish.TabIndex = 10
        viewPublish.Visible = False
        '
        ' viewOnce
        '
        viewOnce.Dock = DockStyle.Fill
        viewOnce.Location = New Point(0, 0)
        viewOnce.Margin = New Padding(0)
        viewOnce.Name = "viewOnce"
        viewOnce.Size = New Size(962, 512)
        viewOnce.TabIndex = 11
        viewOnce.Visible = False
        '
        ' viewSource
        '
        viewSource.Dock = DockStyle.Fill
        viewSource.Location = New Point(0, 0)
        viewSource.Margin = New Padding(0)
        viewSource.Name = "viewSource"
        viewSource.Size = New Size(962, 512)
        viewSource.TabIndex = 40
        viewSource.Visible = False
        '
        ' viewLogin
        '
        viewLogin.Dock = DockStyle.Fill
        viewLogin.Location = New Point(0, 0)
        viewLogin.Margin = New Padding(0)
        viewLogin.Name = "viewLogin"
        viewLogin.Size = New Size(962, 512)
        viewLogin.TabIndex = 12
        viewLogin.Visible = False
        '
        ' notify
        '
        notify.ContextMenuStrip = mnuTray
        notify.Text = "KBotAdmin"
        '
        ' mnuTray
        '
        mnuTray.Items.AddRange(New ToolStripItem() {mnuShow, mnuExit})
        mnuTray.Name = "mnuTray"
        mnuTray.Size = New Size(180, 52)
        '
        ' mnuShow
        '
        mnuShow.Name = "mnuShow"
        mnuShow.Size = New Size(179, 24)
        mnuShow.Text = "Afișează fereastra"
        '
        ' mnuExit
        '
        mnuExit.Name = "mnuExit"
        mnuExit.Size = New Size(179, 24)
        mnuExit.Text = "Închide"
        '
        ' MainForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1500, 760)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(1000, 640)
        Name = "MainForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "KBotAdmin"
        WindowState = FormWindowState.Maximized
        CType(navMode, ComponentModel.ISupportInitialize).EndInit()
        pnlRoot.ResumeLayout(False)
        pnlStatus.ResumeLayout(False)
        tlyMain.ResumeLayout(False)
        splMain.Panel1.ResumeLayout(False)
        splMain.Panel2.ResumeLayout(False)
        CType(splMain, ComponentModel.ISupportInitialize).EndInit()
        splMain.ResumeLayout(False)
        tlyLeft.ResumeLayout(False)
        pnlViews.ResumeLayout(False)
        mnuTray.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents ribbon As KBotRibbon
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents busy As KBotBusyBar
    Friend WithEvents lblStatus As Label
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents splMain As SplitContainer
    Friend WithEvents tlyLeft As KBotTableLayoutPanel
    Friend WithEvents navMode As KBotNavList
    Friend WithEvents treeDb As AdvancedTreeControl
    Friend WithEvents chkAll As KBotCheckBox
    Friend WithEvents pnlViews As Panel
    Friend WithEvents viewCommands As CommandsView
    Friend WithEvents viewSync As SyncView
    Friend WithEvents viewRights As RightsView
    Friend WithEvents viewLogs As LogsView
    Friend WithEvents viewAudit As AuditView
    Friend WithEvents viewVisits As VisitsView
    Friend WithEvents viewServer As ServerView
    Friend WithEvents viewBackups As BackupsView
    Friend WithEvents viewRequests As RequestsView
    Friend WithEvents notify As NotifyIcon
    Friend WithEvents mnuTray As ContextMenuStrip
    Friend WithEvents mnuShow As ToolStripMenuItem
    Friend WithEvents mnuExit As ToolStripMenuItem
    Friend WithEvents viewPublish As PushFilesView
    Friend WithEvents viewOnce As OnceQueryView
    Friend WithEvents viewSource As SourceQueryView
    Friend WithEvents viewLogin As LoginResetView
    Friend WithEvents viewVerconReports As VerconReportsView
    Friend WithEvents viewMfImport As MfImportView
    Friend WithEvents viewDemo As DemoCloneView
    Friend WithEvents viewLocalBackup As LocalBackupView
    Friend WithEvents viewMdHistory As MdHistoryView
    Friend WithEvents viewSchemaGraph As SchemaGraphView
    Friend WithEvents viewSchemaHistory As SchemaHistoryView
End Class
