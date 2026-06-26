Imports System.Windows.Forms
Imports System.Drawing

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBOT_STANDALONE
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(KBOT_STANDALONE))
        TT = New ToolTip(components)
        btnChangeFolder = New Button()
        btnRefreshCerts = New Button()
        btnSendMessage = New Button()
        btnAfiseazaBrowser = New Button()
        btnConectAccess = New Button()
        btnSaveSendAction = New Button()
        btnHistory = New Button()
        btnConnect = New Button()
        btnStart = New Button()
        btnStepPrev = New Button()
        btnStepNext = New Button()
        btnStepSkip = New Button()
        btnStepStop = New Button()
        lstWorkflows = New CheckedListBox()
        cmbCertificates = New ComboBox()
        cmbThrottle = New ComboBox()
        btnTabPrincipal = New Button()
        btnOnlyCheckpoints = New Button()
        btnStepByStep = New Button()
        btnForexeSNM = New Button()
        btnTabSetari = New Button()
        btnDarkMode = New Button()
        spltMain = New SplitContainer()
        pnlTop = New Panel()
        SplitContainer1 = New SplitContainer()
        pnlLeftContent = New Panel()
        pnlLeftHeader = New Panel()
        Label1 = New Label()
        tlyMain = New TableLayoutPanel()
        TableLayoutPanel1 = New TableLayoutPanel()
        pnlTabContainer = New Panel()
        pnlTabSetari = New Panel()
        tlySetari = New TableLayoutPanel()
        btnWicketMonitor = New Button()
        Label2 = New Label()
        pnlTabPrincipal = New Panel()
        tlyPrincipal = New TableLayoutPanel()
        pnlTabHeader = New Panel()
        lblLog = New Label()
        rtbLog = New RichTextBox()
        lblStatus = New Label()
        pb1 = New ProgressBar()
        pnlStepControl = New Panel()
        TableLayoutPanel3 = New TableLayoutPanel()
        lblStepDescription = New Label()
        CType(spltMain, System.ComponentModel.ISupportInitialize).BeginInit()
        spltMain.Panel1.SuspendLayout()
        spltMain.Panel2.SuspendLayout()
        spltMain.SuspendLayout()
        pnlTop.SuspendLayout()
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        pnlLeftContent.SuspendLayout()
        pnlLeftHeader.SuspendLayout()
        tlyMain.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        pnlTabContainer.SuspendLayout()
        pnlTabSetari.SuspendLayout()
        tlySetari.SuspendLayout()
        pnlTabPrincipal.SuspendLayout()
        tlyPrincipal.SuspendLayout()
        pnlTabHeader.SuspendLayout()
        pnlStepControl.SuspendLayout()
        TableLayoutPanel3.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnChangeFolder
        ' 
        btnChangeFolder.Dock = DockStyle.Right
        btnChangeFolder.Location = New Point(209, 0)
        btnChangeFolder.Margin = New Padding(0)
        btnChangeFolder.Name = "btnChangeFolder"
        btnChangeFolder.Size = New Size(36, 38)
        btnChangeFolder.TabIndex = 22
        btnChangeFolder.Text = "..."
        TT.SetToolTip(btnChangeFolder, "Schimbă folderul cu fluxuri ADLOP")
        btnChangeFolder.UseVisualStyleBackColor = True
        ' 
        ' btnRefreshCerts
        ' 
        btnRefreshCerts.Dock = DockStyle.Fill
        btnRefreshCerts.Location = New Point(647, 2)
        btnRefreshCerts.Margin = New Padding(2)
        btnRefreshCerts.Name = "btnRefreshCerts"
        btnRefreshCerts.Size = New Size(44, 40)
        btnRefreshCerts.TabIndex = 5
        btnRefreshCerts.Text = "🔄"
        TT.SetToolTip(btnRefreshCerts, "Reîmprospătează lista cu semnături digitale")
        btnRefreshCerts.UseVisualStyleBackColor = True
        ' 
        ' btnSendMessage
        ' 
        btnSendMessage.Dock = DockStyle.Fill
        btnSendMessage.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnSendMessage.Location = New Point(212, 4)
        btnSendMessage.Margin = New Padding(4)
        btnSendMessage.Name = "btnSendMessage"
        btnSendMessage.Size = New Size(44, 54)
        btnSendMessage.TabIndex = 25
        btnSendMessage.Text = "🚀"
        TT.SetToolTip(btnSendMessage, "Trimite mesaj IPC la serverul VBA" & vbCrLf & "Transmite un mesaj prin canal Named Pipe." & vbCrLf & "Folosit pentru comunicarea în timp real cu Excel/Access.")
        btnSendMessage.UseVisualStyleBackColor = True
        ' 
        ' btnAfiseazaBrowser
        ' 
        btnAfiseazaBrowser.Dock = DockStyle.Fill
        btnAfiseazaBrowser.Enabled = False
        btnAfiseazaBrowser.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnAfiseazaBrowser.Location = New Point(4, 4)
        btnAfiseazaBrowser.Margin = New Padding(4)
        btnAfiseazaBrowser.Name = "btnAfiseazaBrowser"
        btnAfiseazaBrowser.Size = New Size(44, 54)
        btnAfiseazaBrowser.TabIndex = 24
        btnAfiseazaBrowser.Text = "🌐"
        TT.SetToolTip(btnAfiseazaBrowser, "Afișează / Ascunde browserul Chrome" & vbCrLf & "Comută vizibilitatea ferestrei de browser în timp real." & vbCrLf & "Nu afectează execuția fluxului în curs.")
        btnAfiseazaBrowser.UseVisualStyleBackColor = True
        ' 
        ' btnConectAccess
        ' 
        btnConectAccess.Dock = DockStyle.Fill
        btnConectAccess.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnConectAccess.Location = New Point(160, 4)
        btnConectAccess.Margin = New Padding(4)
        btnConectAccess.Name = "btnConectAccess"
        btnConectAccess.Size = New Size(44, 54)
        btnConectAccess.TabIndex = 23
        btnConectAccess.Text = "🔌"
        TT.SetToolTip(btnConectAccess, "Conectare la baza de date Access (OLE)" & vbCrLf & "Inițiază conexiunea cu fișierul .accdb configurat." & vbCrLf & "Necesar pentru acțiunile de tip Access din flux.")
        btnConectAccess.UseVisualStyleBackColor = True
        ' 
        ' btnSaveSendAction
        ' 
        btnSaveSendAction.Dock = DockStyle.Fill
        btnSaveSendAction.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnSaveSendAction.Location = New Point(108, 4)
        btnSaveSendAction.Margin = New Padding(4)
        btnSaveSendAction.Name = "btnSaveSendAction"
        btnSaveSendAction.Size = New Size(44, 54)
        btnSaveSendAction.TabIndex = 22
        btnSaveSendAction.Text = "💾"
        TT.SetToolTip(btnSaveSendAction, "Salvează acțiunea curentă ca flux" & vbCrLf & "Stochează ultimul flux primit pentru rulare ulterioară." & vbCrLf & "Util pentru replay fără a rechema serverul VBA.")
        btnSaveSendAction.UseVisualStyleBackColor = True
        ' 
        ' btnHistory
        ' 
        btnHistory.Dock = DockStyle.Fill
        btnHistory.Enabled = False
        btnHistory.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        btnHistory.Location = New Point(56, 4)
        btnHistory.Margin = New Padding(4)
        btnHistory.Name = "btnHistory"
        btnHistory.Size = New Size(44, 54)
        btnHistory.TabIndex = 18
        btnHistory.Text = "🕑"
        TT.SetToolTip(btnHistory, "Afișează istoricul sesiunii curente" & vbCrLf & "Log detaliat cu toate acțiunile executate." & vbCrLf & "Disponibil după cel puțin o execuție de flux.")
        btnHistory.UseVisualStyleBackColor = True
        ' 
        ' btnConnect
        ' 
        btnConnect.Dock = DockStyle.Fill
        btnConnect.Enabled = False
        btnConnect.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnConnect.Location = New Point(345, 4)
        btnConnect.Margin = New Padding(4)
        btnConnect.Name = "btnConnect"
        btnConnect.Size = New Size(168, 54)
        btnConnect.TabIndex = 21
        btnConnect.Text = "Conectare"
        TT.SetToolTip(btnConnect, "Conectare la serverul FOREXE" & vbCrLf & "ATENȚIE! Dacă nu există niciun token cu " & vbCrLf & "semnătură digitlă conectat, acest buton " & vbCrLf & "este DEZACTIVAT!")
        btnConnect.UseVisualStyleBackColor = True
        ' 
        ' btnStart
        ' 
        btnStart.Dock = DockStyle.Fill
        btnStart.Enabled = False
        btnStart.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnStart.Location = New Point(521, 4)
        btnStart.Margin = New Padding(4)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(168, 54)
        btnStart.TabIndex = 17
        btnStart.Text = "START"
        TT.SetToolTip(btnStart, "Rulează fluxul / fluxurile bifate in lista din stânga")
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' btnStepPrev
        ' 
        btnStepPrev.BackColor = Color.FromArgb(CByte(0), CByte(150), CByte(0))
        btnStepPrev.Dock = DockStyle.Fill
        btnStepPrev.FlatStyle = FlatStyle.Flat
        btnStepPrev.Font = New Font("Consolas", 11F, FontStyle.Bold)
        btnStepPrev.ForeColor = Color.White
        btnStepPrev.Location = New Point(756, 14)
        btnStepPrev.Margin = New Padding(4, 14, 4, 14)
        btnStepPrev.Name = "btnStepPrev"
        btnStepPrev.Size = New Size(40, 47)
        btnStepPrev.TabIndex = 5
        btnStepPrev.Text = "◀️"
        TT.SetToolTip(btnStepPrev, resources.GetString("btnStepPrev.ToolTip"))
        btnStepPrev.UseVisualStyleBackColor = False
        ' 
        ' btnStepNext
        ' 
        btnStepNext.BackColor = Color.FromArgb(CByte(0), CByte(150), CByte(0))
        btnStepNext.Dock = DockStyle.Fill
        btnStepNext.FlatStyle = FlatStyle.Flat
        btnStepNext.Font = New Font("Consolas", 11F, FontStyle.Bold)
        btnStepNext.ForeColor = Color.White
        btnStepNext.Location = New Point(804, 14)
        btnStepNext.Margin = New Padding(4, 14, 4, 14)
        btnStepNext.Name = "btnStepNext"
        btnStepNext.Size = New Size(40, 47)
        btnStepNext.TabIndex = 6
        btnStepNext.Text = "▶"
        TT.SetToolTip(btnStepNext, "Rulează codul până la următorul check-point")
        btnStepNext.UseVisualStyleBackColor = False
        ' 
        ' btnStepSkip
        ' 
        btnStepSkip.BackColor = Color.FromArgb(CByte(200), CByte(150), CByte(0))
        btnStepSkip.Dock = DockStyle.Fill
        btnStepSkip.FlatStyle = FlatStyle.Flat
        btnStepSkip.Font = New Font("Consolas", 11F, FontStyle.Bold)
        btnStepSkip.ForeColor = Color.White
        btnStepSkip.Location = New Point(852, 14)
        btnStepSkip.Margin = New Padding(4, 14, 4, 14)
        btnStepSkip.Name = "btnStepSkip"
        btnStepSkip.Size = New Size(40, 47)
        btnStepSkip.TabIndex = 7
        btnStepSkip.Text = "⏭"
        TT.SetToolTip(btnStepSkip, "Sari peste calupul de acțiuni până la următorul check-point")
        btnStepSkip.UseVisualStyleBackColor = False
        ' 
        ' btnStepStop
        ' 
        btnStepStop.BackColor = Color.FromArgb(CByte(200), CByte(50), CByte(50))
        btnStepStop.Dock = DockStyle.Fill
        btnStepStop.FlatStyle = FlatStyle.Flat
        btnStepStop.Font = New Font("Consolas", 11F, FontStyle.Bold)
        btnStepStop.ForeColor = Color.White
        btnStepStop.Location = New Point(900, 14)
        btnStepStop.Margin = New Padding(4, 14, 4, 14)
        btnStepStop.Name = "btnStepStop"
        btnStepStop.Size = New Size(40, 47)
        btnStepStop.TabIndex = 8
        btnStepStop.Text = "■"
        TT.SetToolTip(btnStepStop, "Oprește execuția fluxului")
        btnStepStop.UseVisualStyleBackColor = False
        ' 
        ' lstWorkflows
        ' 
        lstWorkflows.Dock = DockStyle.Fill
        lstWorkflows.FormattingEnabled = True
        lstWorkflows.Location = New Point(2, 10)
        lstWorkflows.Margin = New Padding(0)
        lstWorkflows.Name = "lstWorkflows"
        lstWorkflows.Size = New Size(241, 393)
        lstWorkflows.TabIndex = 21
        TT.SetToolTip(lstWorkflows, "Lista fluxurilor ADLOP disponibile" & vbCrLf & "Bifați unul sau mai multe fluxuri pentru a le rula în secvență." & vbCrLf & "Clic dreapta pentru opțiuni suplimentare." & vbCrLf & "Ordinea de execuție respectă ordinea din listă.")
        ' 
        ' cmbCertificates
        ' 
        cmbCertificates.Dock = DockStyle.Fill
        cmbCertificates.DropDownStyle = ComboBoxStyle.DropDownList
        cmbCertificates.FormattingEnabled = True
        cmbCertificates.IntegralHeight = False
        cmbCertificates.ItemHeight = 25
        cmbCertificates.Location = New Point(10, 4)
        cmbCertificates.Margin = New Padding(10, 4, 0, 0)
        cmbCertificates.Name = "cmbCertificates"
        cmbCertificates.Size = New Size(635, 33)
        cmbCertificates.TabIndex = 3
        TT.SetToolTip(cmbCertificates, resources.GetString("cmbCertificates.ToolTip"))
        ' 
        ' cmbThrottle
        ' 
        cmbThrottle.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cmbThrottle.DropDownStyle = ComboBoxStyle.DropDownList
        cmbThrottle.FormattingEnabled = True
        cmbThrottle.Items.AddRange(New Object() {"Niciun throttle", "Fast 3G (~1.5 Mbps)", "Slow 3G (~400 Kbps)", "2G (~200 Kbps)", "Custom..."})
        cmbThrottle.Location = New Point(401, 14)
        cmbThrottle.Margin = New Padding(8, 0, 8, 0)
        cmbThrottle.Name = "cmbThrottle"
        cmbThrottle.Size = New Size(284, 33)
        cmbThrottle.TabIndex = 26
        TT.SetToolTip(cmbThrottle, "Throttle rețea — simulare conexiune lentă" & vbCrLf & "Simulează o conexiune la internet mai lentă." & vbCrLf & "Util pentru testarea comportamentului pe 3G / 2G." & vbCrLf & "Niciun throttle = viteză maximă disponibilă.")
        ' 
        ' btnTabPrincipal
        ' 
        btnTabPrincipal.Dock = DockStyle.Left
        btnTabPrincipal.FlatStyle = FlatStyle.Flat
        btnTabPrincipal.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnTabPrincipal.Location = New Point(4, 0)
        btnTabPrincipal.Margin = New Padding(0)
        btnTabPrincipal.Name = "btnTabPrincipal"
        btnTabPrincipal.Size = New Size(110, 48)
        btnTabPrincipal.TabIndex = 0
        btnTabPrincipal.Tag = "TabBtn"
        btnTabPrincipal.Text = "Principal"
        TT.SetToolTip(btnTabPrincipal, "Tab Principal — acțiuni de execuție" & vbCrLf & "Butoane de conectare, start și instrumente auxiliare.")
        btnTabPrincipal.UseVisualStyleBackColor = True
        ' 
        ' btnOnlyCheckpoints
        ' 
        btnOnlyCheckpoints.Dock = DockStyle.Fill
        btnOnlyCheckpoints.Font = New Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnOnlyCheckpoints.ForeColor = Color.MidnightBlue
        btnOnlyCheckpoints.Location = New Point(58, 4)
        btnOnlyCheckpoints.Margin = New Padding(4)
        btnOnlyCheckpoints.Name = "btnOnlyCheckpoints"
        btnOnlyCheckpoints.Size = New Size(46, 54)
        btnOnlyCheckpoints.TabIndex = 29
        btnOnlyCheckpoints.Tag = ""
        btnOnlyCheckpoints.Text = "🚩"
        TT.SetToolTip(btnOnlyCheckpoints, "Activează / Dezactivează oprirea doar la checkpoint-uri." & vbCrLf & "Funcționează numai când modul Pas cu Pas (👣) este activ.")
        btnOnlyCheckpoints.UseVisualStyleBackColor = True
        ' 
        ' btnStepByStep
        ' 
        btnStepByStep.Dock = DockStyle.Fill
        btnStepByStep.Font = New Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnStepByStep.ForeColor = Color.DarkGreen
        btnStepByStep.Location = New Point(4, 4)
        btnStepByStep.Margin = New Padding(4)
        btnStepByStep.Name = "btnStepByStep"
        btnStepByStep.Size = New Size(46, 54)
        btnStepByStep.TabIndex = 28
        btnStepByStep.Tag = ""
        btnStepByStep.Text = "👣"
        TT.SetToolTip(btnStepByStep, "Activează / Dezactivează modul Pas cu Pas." & vbCrLf & "Execuția se va opri la fiecare acțiune, așteptând confirmare manuală.")
        btnStepByStep.UseVisualStyleBackColor = True
        ' 
        ' btnForexeSNM
        ' 
        btnForexeSNM.Dock = DockStyle.Right
        btnForexeSNM.FlatStyle = FlatStyle.Flat
        btnForexeSNM.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnForexeSNM.Location = New Point(547, 0)
        btnForexeSNM.Margin = New Padding(0)
        btnForexeSNM.Name = "btnForexeSNM"
        btnForexeSNM.Size = New Size(142, 48)
        btnForexeSNM.TabIndex = 2
        btnForexeSNM.Tag = "TabBtn"
        btnForexeSNM.Text = "Rapoarte FX"
        TT.SetToolTip(btnForexeSNM, "Tab Principal — acțiuni de execuție" & vbCrLf & "Butoane de conectare, start și instrumente auxiliare.")
        btnForexeSNM.UseVisualStyleBackColor = True
        ' 
        ' btnTabSetari
        ' 
        btnTabSetari.Dock = DockStyle.Left
        btnTabSetari.FlatStyle = FlatStyle.Flat
        btnTabSetari.Font = New Font("Segoe UI", 9F)
        btnTabSetari.Location = New Point(114, 0)
        btnTabSetari.Margin = New Padding(0)
        btnTabSetari.Name = "btnTabSetari"
        btnTabSetari.Size = New Size(151, 48)
        btnTabSetari.TabIndex = 3
        btnTabSetari.Tag = "TabBtn"
        btnTabSetari.Text = "Setări"
        TT.SetToolTip(btnTabSetari, "Tab Setări — opțiuni de execuție" & vbCrLf & "Vizibilitate browser, debugging pas-cu-pas și throttle rețea.")
        btnTabSetari.UseVisualStyleBackColor = True
        ' 
        ' btnDarkMode
        ' 
        btnDarkMode.Dock = DockStyle.Fill
        btnDarkMode.Font = New Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnDarkMode.Location = New Point(112, 4)
        btnDarkMode.Margin = New Padding(4)
        btnDarkMode.Name = "btnDarkMode"
        btnDarkMode.Size = New Size(46, 54)
        btnDarkMode.TabIndex = 27
        btnDarkMode.Tag = "ThemeToggle"
        btnDarkMode.Text = "🌙 "
        btnDarkMode.UseVisualStyleBackColor = True
        ' 
        ' spltMain
        ' 
        spltMain.Dock = DockStyle.Fill
        spltMain.Location = New Point(0, 0)
        spltMain.Name = "spltMain"
        spltMain.Orientation = Orientation.Horizontal
        ' 
        ' spltMain.Panel1
        ' 
        spltMain.Panel1.Controls.Add(pnlTop)
        spltMain.Panel1.Padding = New Padding(0, 0, 0, 5)
        ' 
        ' spltMain.Panel2
        ' 
        spltMain.Panel2.Controls.Add(pnlStepControl)
        spltMain.Size = New Size(944, 554)
        spltMain.SplitterDistance = 475
        spltMain.TabIndex = 2
        ' 
        ' pnlTop
        ' 
        pnlTop.Controls.Add(SplitContainer1)
        pnlTop.Controls.Add(pb1)
        pnlTop.Dock = DockStyle.Fill
        pnlTop.Location = New Point(0, 0)
        pnlTop.Margin = New Padding(0)
        pnlTop.Name = "pnlTop"
        pnlTop.Size = New Size(944, 470)
        pnlTop.TabIndex = 1
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.Location = New Point(0, 0)
        SplitContainer1.Name = "SplitContainer1"
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(pnlLeftContent)
        SplitContainer1.Panel1.Controls.Add(pnlLeftHeader)
        SplitContainer1.Panel1.Padding = New Padding(0, 10, 0, 0)
        SplitContainer1.Panel1MinSize = 150
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(tlyMain)
        SplitContainer1.Panel2MinSize = 420
        SplitContainer1.Size = New Size(944, 453)
        SplitContainer1.SplitterDistance = 245
        SplitContainer1.SplitterWidth = 6
        SplitContainer1.TabIndex = 30
        ' 
        ' pnlLeftContent
        ' 
        pnlLeftContent.Controls.Add(lstWorkflows)
        pnlLeftContent.Dock = DockStyle.Fill
        pnlLeftContent.Location = New Point(0, 48)
        pnlLeftContent.Name = "pnlLeftContent"
        pnlLeftContent.Padding = New Padding(2, 10, 2, 2)
        pnlLeftContent.Size = New Size(245, 405)
        pnlLeftContent.TabIndex = 1
        ' 
        ' pnlLeftHeader
        ' 
        pnlLeftHeader.Controls.Add(btnChangeFolder)
        pnlLeftHeader.Controls.Add(Label1)
        pnlLeftHeader.Dock = DockStyle.Top
        pnlLeftHeader.Location = New Point(0, 10)
        pnlLeftHeader.Name = "pnlLeftHeader"
        pnlLeftHeader.Size = New Size(245, 38)
        pnlLeftHeader.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.Dock = DockStyle.Fill
        Label1.Font = New Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(0, 0)
        Label1.Margin = New Padding(4, 0, 4, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(245, 38)
        Label1.TabIndex = 20
        Label1.Text = "Fluxuri disponibile"
        Label1.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 2
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 159F))
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(TableLayoutPanel1, 0, 1)
        tlyMain.Controls.Add(pnlTabContainer, 0, 3)
        tlyMain.Controls.Add(lblLog, 0, 4)
        tlyMain.Controls.Add(rtbLog, 0, 5)
        tlyMain.Controls.Add(lblStatus, 0, 6)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 7
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 4F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 10F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 110F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(693, 453)
        tlyMain.TabIndex = 12
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 2
        tlyMain.SetColumnSpan(TableLayoutPanel1, 2)
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48F))
        TableLayoutPanel1.Controls.Add(cmbCertificates, 0, 0)
        TableLayoutPanel1.Controls.Add(btnRefreshCerts, 1, 0)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 4)
        TableLayoutPanel1.Margin = New Padding(0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 1
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.Size = New Size(693, 44)
        TableLayoutPanel1.TabIndex = 15
        ' 
        ' pnlTabContainer
        ' 
        tlyMain.SetColumnSpan(pnlTabContainer, 2)
        pnlTabContainer.Controls.Add(pnlTabSetari)
        pnlTabContainer.Controls.Add(pnlTabPrincipal)
        pnlTabContainer.Controls.Add(pnlTabHeader)
        pnlTabContainer.Dock = DockStyle.Fill
        pnlTabContainer.Location = New Point(0, 58)
        pnlTabContainer.Margin = New Padding(0)
        pnlTabContainer.Name = "pnlTabContainer"
        pnlTabContainer.Size = New Size(693, 110)
        pnlTabContainer.TabIndex = 26
        ' 
        ' pnlTabSetari
        ' 
        pnlTabSetari.Controls.Add(tlySetari)
        pnlTabSetari.Dock = DockStyle.Fill
        pnlTabSetari.Location = New Point(0, 48)
        pnlTabSetari.Margin = New Padding(0)
        pnlTabSetari.Name = "pnlTabSetari"
        pnlTabSetari.Size = New Size(693, 62)
        pnlTabSetari.TabIndex = 2
        pnlTabSetari.Visible = False
        ' 
        ' tlySetari
        ' 
        tlySetari.ColumnCount = 7
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 54F))
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlySetari.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 300F))
        tlySetari.Controls.Add(btnWicketMonitor, 3, 0)
        tlySetari.Controls.Add(btnOnlyCheckpoints, 1, 0)
        tlySetari.Controls.Add(btnStepByStep, 0, 0)
        tlySetari.Controls.Add(btnDarkMode, 2, 0)
        tlySetari.Controls.Add(cmbThrottle, 6, 0)
        tlySetari.Controls.Add(Label2, 5, 0)
        tlySetari.Dock = DockStyle.Fill
        tlySetari.Location = New Point(0, 0)
        tlySetari.Margin = New Padding(0)
        tlySetari.Name = "tlySetari"
        tlySetari.RowCount = 1
        tlySetari.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySetari.Size = New Size(693, 62)
        tlySetari.TabIndex = 0
        ' 
        ' btnWicketMonitor
        ' 
        btnWicketMonitor.Dock = DockStyle.Fill
        btnWicketMonitor.Font = New Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnWicketMonitor.ForeColor = Color.DarkRed
        btnWicketMonitor.Location = New Point(166, 4)
        btnWicketMonitor.Margin = New Padding(4)
        btnWicketMonitor.Name = "btnWicketMonitor"
        btnWicketMonitor.Size = New Size(46, 54)
        btnWicketMonitor.TabIndex = 31
        btnWicketMonitor.Tag = ""
        btnWicketMonitor.Text = "🚨"
        btnWicketMonitor.UseVisualStyleBackColor = True
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(293, 14)
        Label2.Margin = New Padding(0, 14, 0, 0)
        Label2.Name = "Label2"
        Label2.Size = New Size(73, 25)
        Label2.TabIndex = 30
        Label2.Text = "Throttle"
        ' 
        ' pnlTabPrincipal
        ' 
        pnlTabPrincipal.Controls.Add(tlyPrincipal)
        pnlTabPrincipal.Dock = DockStyle.Fill
        pnlTabPrincipal.Location = New Point(0, 48)
        pnlTabPrincipal.Margin = New Padding(0)
        pnlTabPrincipal.Name = "pnlTabPrincipal"
        pnlTabPrincipal.Size = New Size(693, 62)
        pnlTabPrincipal.TabIndex = 1
        ' 
        ' tlyPrincipal
        ' 
        tlyPrincipal.ColumnCount = 8
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 52F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 52F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 52F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 52F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 52F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 176F))
        tlyPrincipal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 176F))
        tlyPrincipal.Controls.Add(btnSendMessage, 4, 0)
        tlyPrincipal.Controls.Add(btnAfiseazaBrowser, 0, 0)
        tlyPrincipal.Controls.Add(btnConectAccess, 3, 0)
        tlyPrincipal.Controls.Add(btnSaveSendAction, 2, 0)
        tlyPrincipal.Controls.Add(btnHistory, 1, 0)
        tlyPrincipal.Controls.Add(btnConnect, 6, 0)
        tlyPrincipal.Controls.Add(btnStart, 7, 0)
        tlyPrincipal.Dock = DockStyle.Fill
        tlyPrincipal.Location = New Point(0, 0)
        tlyPrincipal.Margin = New Padding(0)
        tlyPrincipal.Name = "tlyPrincipal"
        tlyPrincipal.RowCount = 1
        tlyPrincipal.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyPrincipal.Size = New Size(693, 62)
        tlyPrincipal.TabIndex = 0
        ' 
        ' pnlTabHeader
        ' 
        pnlTabHeader.Controls.Add(btnTabSetari)
        pnlTabHeader.Controls.Add(btnForexeSNM)
        pnlTabHeader.Controls.Add(btnTabPrincipal)
        pnlTabHeader.Dock = DockStyle.Top
        pnlTabHeader.Location = New Point(0, 0)
        pnlTabHeader.Margin = New Padding(0)
        pnlTabHeader.Name = "pnlTabHeader"
        pnlTabHeader.Padding = New Padding(4, 0, 4, 0)
        pnlTabHeader.Size = New Size(693, 48)
        pnlTabHeader.TabIndex = 0
        ' 
        ' lblLog
        ' 
        lblLog.AutoSize = True
        tlyMain.SetColumnSpan(lblLog, 2)
        lblLog.Dock = DockStyle.Left
        lblLog.Location = New Point(4, 168)
        lblLog.Margin = New Padding(4, 0, 4, 0)
        lblLog.Name = "lblLog"
        lblLog.Size = New Size(46, 42)
        lblLog.TabIndex = 13
        lblLog.Text = "Log:"
        lblLog.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' rtbLog
        ' 
        rtbLog.BackColor = Color.White
        tlyMain.SetColumnSpan(rtbLog, 2)
        rtbLog.Dock = DockStyle.Fill
        rtbLog.Font = New Font("Consolas", 9F)
        rtbLog.Location = New Point(0, 210)
        rtbLog.Margin = New Padding(0, 0, 4, 0)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.Size = New Size(689, 215)
        rtbLog.TabIndex = 14
        rtbLog.Text = ""
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        tlyMain.SetColumnSpan(lblStatus, 2)
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Font = New Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblStatus.ForeColor = Color.Gray
        lblStatus.Location = New Point(4, 425)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(685, 28)
        lblStatus.TabIndex = 10
        lblStatus.Text = "În așteptare..."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pb1
        ' 
        pb1.Dock = DockStyle.Bottom
        pb1.Location = New Point(0, 453)
        pb1.Margin = New Padding(0)
        pb1.Name = "pb1"
        pb1.Size = New Size(944, 17)
        pb1.Style = ProgressBarStyle.Continuous
        pb1.TabIndex = 18
        ' 
        ' pnlStepControl
        ' 
        pnlStepControl.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlStepControl.BackColor = SystemColors.Control
        pnlStepControl.Controls.Add(TableLayoutPanel3)
        pnlStepControl.Dock = DockStyle.Fill
        pnlStepControl.Location = New Point(0, 0)
        pnlStepControl.Margin = New Padding(0)
        pnlStepControl.Name = "pnlStepControl"
        pnlStepControl.Size = New Size(944, 75)
        pnlStepControl.TabIndex = 2
        pnlStepControl.Visible = False
        ' 
        ' TableLayoutPanel3
        ' 
        TableLayoutPanel3.ColumnCount = 5
        TableLayoutPanel3.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel3.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48F))
        TableLayoutPanel3.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48F))
        TableLayoutPanel3.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48F))
        TableLayoutPanel3.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48F))
        TableLayoutPanel3.Controls.Add(lblStepDescription, 0, 0)
        TableLayoutPanel3.Controls.Add(btnStepPrev, 1, 0)
        TableLayoutPanel3.Controls.Add(btnStepNext, 2, 0)
        TableLayoutPanel3.Controls.Add(btnStepSkip, 3, 0)
        TableLayoutPanel3.Controls.Add(btnStepStop, 4, 0)
        TableLayoutPanel3.Dock = DockStyle.Fill
        TableLayoutPanel3.Location = New Point(0, 0)
        TableLayoutPanel3.Margin = New Padding(0)
        TableLayoutPanel3.Name = "TableLayoutPanel3"
        TableLayoutPanel3.RowCount = 1
        TableLayoutPanel3.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel3.Size = New Size(944, 75)
        TableLayoutPanel3.TabIndex = 5
        ' 
        ' lblStepDescription
        ' 
        lblStepDescription.Dock = DockStyle.Fill
        lblStepDescription.Font = New Font("Consolas", 9F, FontStyle.Bold)
        lblStepDescription.ForeColor = Color.FromArgb(CByte(64), CByte(64), CByte(64))
        lblStepDescription.Location = New Point(4, 4)
        lblStepDescription.Margin = New Padding(4)
        lblStepDescription.Name = "lblStepDescription"
        lblStepDescription.Size = New Size(744, 67)
        lblStepDescription.TabIndex = 1
        lblStepDescription.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' KBOT_STANDALONE
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(944, 554)
        Controls.Add(spltMain)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(720, 610)
        Name = "KBOT_STANDALONE"
        StartPosition = FormStartPosition.CenterScreen
        Text = "ForexeBot - Automatizare FOREXEBUG"
        spltMain.Panel1.ResumeLayout(False)
        spltMain.Panel2.ResumeLayout(False)
        CType(spltMain, System.ComponentModel.ISupportInitialize).EndInit()
        spltMain.ResumeLayout(False)
        pnlTop.ResumeLayout(False)
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        pnlLeftContent.ResumeLayout(False)
        pnlLeftHeader.ResumeLayout(False)
        tlyMain.ResumeLayout(False)
        tlyMain.PerformLayout()
        TableLayoutPanel1.ResumeLayout(False)
        pnlTabContainer.ResumeLayout(False)
        pnlTabSetari.ResumeLayout(False)
        tlySetari.ResumeLayout(False)
        tlySetari.PerformLayout()
        pnlTabPrincipal.ResumeLayout(False)
        tlyPrincipal.ResumeLayout(False)
        pnlTabHeader.ResumeLayout(False)
        pnlStepControl.ResumeLayout(False)
        TableLayoutPanel3.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents TT As ToolTip
    Friend WithEvents spltMain As SplitContainer
    Friend WithEvents pnlTop As Panel
    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents pnlLeftContent As Panel
    Friend WithEvents lstWorkflows As CheckedListBox
    Friend WithEvents pnlLeftHeader As Panel
    Friend WithEvents btnChangeFolder As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents cmbCertificates As ComboBox
    Friend WithEvents btnRefreshCerts As Button
    Friend WithEvents pnlTabContainer As Panel
    Friend WithEvents pnlTabHeader As Panel
    Friend WithEvents btnTabPrincipal As Button
    Friend WithEvents pnlTabPrincipal As Panel
    Friend WithEvents tlyPrincipal As TableLayoutPanel
    Friend WithEvents btnSendMessage As Button
    Friend WithEvents btnAfiseazaBrowser As Button
    Friend WithEvents btnConectAccess As Button
    Friend WithEvents btnSaveSendAction As Button
    Friend WithEvents btnHistory As Button
    Friend WithEvents btnConnect As Button
    Friend WithEvents btnStart As Button
    Friend WithEvents pnlTabSetari As Panel
    Friend WithEvents tlySetari As TableLayoutPanel
    Friend WithEvents cmbThrottle As ComboBox
    Friend WithEvents lblLog As Label
    Friend WithEvents rtbLog As RichTextBox
    Friend WithEvents lblStatus As Label
    Friend WithEvents pb1 As ProgressBar
    Friend WithEvents pnlStepControl As Panel
    Friend WithEvents TableLayoutPanel3 As TableLayoutPanel
    Friend WithEvents lblStepDescription As Label
    Friend WithEvents btnStepPrev As Button
    Friend WithEvents btnStepNext As Button
    Friend WithEvents btnStepSkip As Button
    Friend WithEvents btnStepStop As Button
    Friend WithEvents btnDarkMode As Button
    Friend WithEvents btnStepByStep As Button
    Friend WithEvents btnOnlyCheckpoints As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents btnWicketMonitor As Button
    Friend WithEvents btnTabSetari As Button
    Friend WithEvents btnForexeSNM As Button
End Class