' The main window (slice ME01): databases tree and accounts tree on the left, the pages on the right.
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
        Dim KBotNavItem14 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem5 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem6 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem7 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem8 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem9 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem10 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem11 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem12 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem13 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        notify = New NotifyIcon(components)
        mnuTray = New ContextMenuStrip(components)
        mnuShow = New ToolStripMenuItem()
        mnuExit = New ToolStripMenuItem()
        mnuTray.SuspendLayout()
        pnlRoot = New Panel()
        capBar = New KBotCaptionBar()
        pnlStatus = New Panel()
        busy = New KBotBusyBar()
        lblStatus = New Label()
        tlyMain = New KBotTableLayoutPanel()
        tlyLeft = New KBotTableLayoutPanel()
        treeDb = New AdvancedTreeControl()
        treeAcc = New AdvancedTreeControl()
        tlyRight = New KBotTableLayoutPanel()
        navViews = New KBotNavList()
        pnlViews = New Panel()
        viewCommands = New CommandsView()
        viewDiff = New DiffView()
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
        viewLogin = New LoginResetView()
        viewVerconReports = New VerconReportsView()
        CType(navViews, ComponentModel.ISupportInitialize).BeginInit()
        pnlRoot.SuspendLayout()
        pnlStatus.SuspendLayout()
        tlyMain.SuspendLayout()
        tlyLeft.SuspendLayout()
        tlyRight.SuspendLayout()
        pnlViews.SuspendLayout()
        SuspendLayout()
        '
        ' pnlRoot
        '
        pnlRoot.Controls.Add(tlyMain)
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
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.ShowThemeButton = False
        capBar.Size = New Size(1278, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "KBotAdmin"
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
        tlyMain.ColumnCount = 2
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyLeft, 0, 0)
        tlyMain.Controls.Add(tlyRight, 1, 0)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 44)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8, 8, 8, 4)
        tlyMain.RowCount = 1
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1278, 644)
        tlyMain.TabIndex = 1
        '
        ' tlyLeft
        '
        tlyLeft.AutoFitToTheme = False
        tlyLeft.ColumnCount = 1
        tlyLeft.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyLeft.Controls.Add(treeDb, 0, 0)
        tlyLeft.Controls.Add(treeAcc, 0, 1)
        tlyLeft.Dock = DockStyle.Fill
        tlyLeft.Location = New Point(8, 8)
        tlyLeft.Margin = New Padding(0, 0, 8, 0)
        tlyLeft.Name = "tlyLeft"
        tlyLeft.RowCount = 2
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Percent, 58F))
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Percent, 42F))
        tlyLeft.Size = New Size(292, 632)
        tlyLeft.TabIndex = 0
        '
        ' treeDb
        '
        treeDb.Dock = DockStyle.Fill
        treeDb.FooterVisible = False
        treeDb.HeaderCaption = " BAZE DE DATE"
        treeDb.HeaderHeight = 28
        treeDb.HeaderVisible = True
        treeDb.ItemHeight = 22
        treeDb.Location = New Point(0, 0)
        treeDb.Margin = New Padding(0, 0, 0, 6)
        treeDb.Name = "treeDb"
        treeDb.RootExpander = False
        treeDb.Size = New Size(292, 360)
        treeDb.TabIndex = 0
        tips.SetToolTipHeader(treeDb, "Bazele de date")
        tips.SetToolTipText(treeDb, "Alege baza pe care lucrează paginile din dreapta." & vbLf & "<TOATE BAZELE> = toate bazele de unitate (fără AVACONT_SURSA și AVACONT_COMUN).")
        '
        ' treeAcc
        '
        treeAcc.Dock = DockStyle.Fill
        treeAcc.FooterVisible = False
        treeAcc.HeaderCaption = " CONTURI"
        treeAcc.HeaderHeight = 28
        treeAcc.HeaderVisible = True
        treeAcc.ItemHeight = 22
        treeAcc.Location = New Point(0, 366)
        treeAcc.Margin = New Padding(0)
        treeAcc.Name = "treeAcc"
        treeAcc.RootExpander = False
        treeAcc.Size = New Size(292, 266)
        treeAcc.TabIndex = 1
        tips.SetToolTipHeader(treeAcc, "Conturile bazei")
        tips.SetToolTipText(treeAcc, "Conturile legate de baza aleasă, cu rolul lor K-BOT." & vbLf & "Un clic pe un cont îl alege în pagina «Drepturi».")
        '
        ' tlyRight
        '
        tlyRight.AutoFitToTheme = False
        tlyRight.ColumnCount = 1
        tlyRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRight.Controls.Add(navViews, 0, 0)
        tlyRight.Controls.Add(pnlViews, 0, 1)
        tlyRight.Dock = DockStyle.Fill
        tlyRight.Location = New Point(308, 8)
        tlyRight.Margin = New Padding(0)
        tlyRight.Name = "tlyRight"
        tlyRight.RowCount = 2
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyRight.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRight.Size = New Size(962, 632)
        tlyRight.TabIndex = 1
        '
        ' navViews
        '
        tips.SetToolTipHeader(navViews, "Paginile programului")
        tips.SetToolTipText(navViews, "Comenzi: interogări sigure pe baza aleasă sau pe toate." & vbLf & "Diferență schemă: ce diferă de AVACONT_SURSA (doar citire)." & vbLf & "Sincronizare: aduce schema bazelor la schema sursei." & vbLf & "Drepturi: ce poate face fiecare cont în ADECHIT." & vbLf & "Loguri: jurnalele serverului, colorate." & vbLf & "Jurnal: ce s-a făcut în conturi." & vbLf & "Vizite: cine a deschis pagina de prezentare." & vbLf & "Server: disc, memorie, certificat, servicii." & vbLf & "Copii: copii de siguranță ale bazelor." & vbLf & "Cereri: cereri de conturi noi, de aprobat sau respins." & vbLf & "Interogări unice: rulează o dată un text SQL pe toate bazele." & vbLf & "Resetare Login: șterge încercările de login eșuate ale utilizatorilor." & vbLf & "Publicare: trimite fișierele din PYTHON pe server și repornește serviciul.")
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Key = "comenzi"
        KBotNavItem1.Text = "Comenzi"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Key = "diferenta"
        KBotNavItem2.Text = "Diferență"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Key = "sincronizare"
        KBotNavItem3.Text = "Sincronizare"
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Key = "drepturi"
        KBotNavItem4.Text = "Drepturi"
        KBotNavItem5.AutoSize = True
        KBotNavItem5.Key = "loguri"
        KBotNavItem5.Text = "Loguri"
        KBotNavItem6.AutoSize = True
        KBotNavItem6.Key = "audit"
        KBotNavItem6.Text = "Jurnal"
        KBotNavItem7.AutoSize = True
        KBotNavItem7.Key = "vizite"
        KBotNavItem7.Text = "Vizite"
        KBotNavItem8.AutoSize = True
        KBotNavItem8.Key = "server"
        KBotNavItem8.Text = "Server"
        KBotNavItem9.AutoSize = True
        KBotNavItem9.Key = "copii"
        KBotNavItem9.Text = "Copii"
        KBotNavItem10.AutoSize = True
        KBotNavItem10.Key = "cereri"
        KBotNavItem10.Text = "Cereri"
        KBotNavItem11.AutoSize = True
        KBotNavItem11.Key = "unice"
        KBotNavItem11.Text = "Interogări unice"
        KBotNavItem12.AutoSize = True
        KBotNavItem12.Key = "login"
        KBotNavItem12.Text = "Resetare Login"
        KBotNavItem13.Align = KBotNavAlign.Far
        KBotNavItem13.AutoSize = True
        KBotNavItem13.Key = "publicare"
        KBotNavItem13.Text = "Publicare"
        navViews.Dock = DockStyle.Fill
        navViews.IconSize = 0
        navViews.Items.Add(KBotNavItem1)
        navViews.Items.Add(KBotNavItem2)
        navViews.Items.Add(KBotNavItem3)
        navViews.Items.Add(KBotNavItem4)
        navViews.Items.Add(KBotNavItem5)
        navViews.Items.Add(KBotNavItem6)
        navViews.Items.Add(KBotNavItem7)
        navViews.Items.Add(KBotNavItem8)
        navViews.Items.Add(KBotNavItem9)
        navViews.Items.Add(KBotNavItem10)
        navViews.Items.Add(KBotNavItem11)
        navViews.Items.Add(KBotNavItem12)
        navViews.Items.Add(KBotNavItem13)
        KBotNavItem14.Key = "vercon-reports"
        KBotNavItem14.Text = "Rapoarte VERCON"
        KBotNavItem14.AutoSize = True
        navViews.Items.Add(KBotNavItem14)
        navViews.Location = New Point(0, 0)
        navViews.Margin = New Padding(0)
        navViews.Name = "navViews"
        navViews.Orientation = KBotNavOrientation.Horizontal
        navViews.SelectedKey = Nothing
        navViews.Size = New Size(962, 40)
        navViews.TabIndex = 0
        '
        ' pnlViews
        '
        pnlViews.Controls.Add(viewCommands)
        pnlViews.Controls.Add(viewDiff)
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
        pnlViews.Controls.Add(viewLogin)
        pnlViews.Controls.Add(viewVerconReports)
        viewVerconReports.Dock = DockStyle.Fill
        viewVerconReports.Visible = False
        pnlViews.Dock = DockStyle.Fill
        pnlViews.Location = New Point(0, 40)
        pnlViews.Margin = New Padding(0)
        pnlViews.Name = "pnlViews"
        pnlViews.Size = New Size(962, 592)
        pnlViews.TabIndex = 1
        pnlViews.Tag = "Card"
        '
        ' viewCommands
        '
        viewCommands.Dock = DockStyle.Fill
        viewCommands.Location = New Point(0, 0)
        viewCommands.Margin = New Padding(0)
        viewCommands.Name = "viewCommands"
        viewCommands.Size = New Size(962, 592)
        viewCommands.TabIndex = 0
        viewCommands.Visible = False
        '
        ' viewDiff
        '
        viewDiff.Dock = DockStyle.Fill
        viewDiff.Location = New Point(0, 0)
        viewDiff.Margin = New Padding(0)
        viewDiff.Name = "viewDiff"
        viewDiff.Size = New Size(962, 592)
        viewDiff.TabIndex = 1
        viewDiff.Visible = False
        '
        ' viewSync
        '
        viewSync.Dock = DockStyle.Fill
        viewSync.Location = New Point(0, 0)
        viewSync.Margin = New Padding(0)
        viewSync.Name = "viewSync"
        viewSync.Size = New Size(962, 592)
        viewSync.TabIndex = 2
        viewSync.Visible = False
        '
        ' viewRights
        '
        viewRights.Dock = DockStyle.Fill
        viewRights.Location = New Point(0, 0)
        viewRights.Margin = New Padding(0)
        viewRights.Name = "viewRights"
        viewRights.Size = New Size(962, 592)
        viewRights.TabIndex = 3
        viewRights.Visible = False
        '
        ' viewLogs
        '
        viewLogs.Dock = DockStyle.Fill
        viewLogs.Location = New Point(0, 0)
        viewLogs.Margin = New Padding(0)
        viewLogs.Name = "viewLogs"
        viewLogs.Size = New Size(962, 592)
        viewLogs.TabIndex = 4
        viewLogs.Visible = False
        '
        ' viewAudit
        '
        viewAudit.Dock = DockStyle.Fill
        viewAudit.Location = New Point(0, 0)
        viewAudit.Margin = New Padding(0)
        viewAudit.Name = "viewAudit"
        viewAudit.Size = New Size(962, 592)
        viewAudit.TabIndex = 5
        viewAudit.Visible = False
        '
        ' viewVisits
        '
        viewVisits.Dock = DockStyle.Fill
        viewVisits.Location = New Point(0, 0)
        viewVisits.Margin = New Padding(0)
        viewVisits.Name = "viewVisits"
        viewVisits.Size = New Size(962, 592)
        viewVisits.TabIndex = 6
        viewVisits.Visible = False
        '
        ' viewServer
        '
        viewServer.Dock = DockStyle.Fill
        viewServer.Location = New Point(0, 0)
        viewServer.Margin = New Padding(0)
        viewServer.Name = "viewServer"
        viewServer.Size = New Size(962, 592)
        viewServer.TabIndex = 7
        viewServer.Visible = False
        '
        ' viewBackups
        '
        viewBackups.Dock = DockStyle.Fill
        viewBackups.Location = New Point(0, 0)
        viewBackups.Margin = New Padding(0)
        viewBackups.Name = "viewBackups"
        viewBackups.Size = New Size(962, 592)
        viewBackups.TabIndex = 8
        viewBackups.Visible = False
        '
        ' viewRequests
        '
        viewRequests.Dock = DockStyle.Fill
        viewRequests.Location = New Point(0, 0)
        viewRequests.Margin = New Padding(0)
        viewRequests.Name = "viewRequests"
        viewRequests.Size = New Size(962, 592)
        viewRequests.TabIndex = 9
        viewRequests.Visible = False
        '
        ' viewPublish
        '
        viewPublish.Dock = DockStyle.Fill
        viewPublish.Location = New Point(0, 0)
        viewPublish.Margin = New Padding(0)
        viewPublish.Name = "viewPublish"
        viewPublish.Size = New Size(962, 592)
        viewPublish.TabIndex = 10
        viewPublish.Visible = False
        '
        ' viewOnce
        '
        viewOnce.Dock = DockStyle.Fill
        viewOnce.Location = New Point(0, 0)
        viewOnce.Margin = New Padding(0)
        viewOnce.Name = "viewOnce"
        viewOnce.Size = New Size(962, 592)
        viewOnce.TabIndex = 11
        viewOnce.Visible = False
        '
        ' viewLogin
        '
        viewLogin.Dock = DockStyle.Fill
        viewLogin.Location = New Point(0, 0)
        viewLogin.Margin = New Padding(0)
        viewLogin.Name = "viewLogin"
        viewLogin.Size = New Size(962, 592)
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
        CType(navViews, ComponentModel.ISupportInitialize).EndInit()
        pnlRoot.ResumeLayout(False)
        pnlStatus.ResumeLayout(False)
        tlyMain.ResumeLayout(False)
        tlyLeft.ResumeLayout(False)
        tlyRight.ResumeLayout(False)
        pnlViews.ResumeLayout(False)
        mnuTray.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents busy As KBotBusyBar
    Friend WithEvents lblStatus As Label
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyLeft As KBotTableLayoutPanel
    Friend WithEvents treeDb As AdvancedTreeControl
    Friend WithEvents treeAcc As AdvancedTreeControl
    Friend WithEvents tlyRight As KBotTableLayoutPanel
    Friend WithEvents navViews As KBotNavList
    Friend WithEvents pnlViews As Panel
    Friend WithEvents viewCommands As CommandsView
    Friend WithEvents viewDiff As DiffView
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
    Friend WithEvents viewLogin As LoginResetView
    Friend WithEvents viewVerconReports As VerconReportsView
End Class
