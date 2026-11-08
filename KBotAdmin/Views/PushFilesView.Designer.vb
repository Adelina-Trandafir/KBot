' The «Fisiere» page of «Publicare» (slice ME01): the file push of AvacontPush - scan the local PYTHON folder against the server, tick, send, restart the service.
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PushFilesView
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
        Dim gridFilesCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol5 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol6 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol7 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesCol8 As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesSlices As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesReasons As KBotDataColumn = New KBotDataColumn()
        Dim gridFilesDebug As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridPatCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridPatCol2 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        tlyDebug = New KBotTableLayoutPanel()
        chkDebugging = New KBotCheckBox()
        btnFinalizeDebug = New KBotButton()
        lblDebug = New Label()
        btnScan = New KBotButton()
        btnSend = New KBotButton()
        chkRestart = New KBotCheckBox()
        chkIdentical = New KBotCheckBox()
        btnAll = New KBotButton()
        btnNone = New KBotButton()
        lblRoot = New Label()
        tlyMid = New KBotTableLayoutPanel()
        gridFiles = New AdminDataView()
        tlySide = New KBotTableLayoutPanel()
        tlyExtBar = New KBotTableLayoutPanel()
        btnExtAdd = New KBotButton()
        btnExtDel = New KBotButton()
        gridExt = New AdminDataView()
        tlyPatBar = New KBotTableLayoutPanel()
        btnPatAdd = New KBotButton()
        btnPatDel = New KBotButton()
        gridPat = New AdminDataView()
        logBox = New KBotLogBox()
        lblState = New Label()
        CType(gridFiles, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridExt, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridPat, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyMid.SuspendLayout()
        tlySide.SuspendLayout()
        tlyExtBar.SuspendLayout()
        tlyPatBar.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(tlyDebug, 0, 1)
        tlyMain.Controls.Add(lblRoot, 0, 2)
        tlyMain.Controls.Add(tlyMid, 0, 3)
        tlyMain.Controls.Add(logBox, 0, 4)
        tlyMain.Controls.Add(lblState, 0, 5)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 6
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 24F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 150F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 7
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnScan, 0, 0)
        tlyBar.Controls.Add(btnSend, 1, 0)
        tlyBar.Controls.Add(chkRestart, 2, 0)
        tlyBar.Controls.Add(chkIdentical, 3, 0)
        tlyBar.Controls.Add(btnAll, 4, 0)
        tlyBar.Controls.Add(btnNone, 5, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(0, 0)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyBar.Size = New Size(900, 560)
        tlyBar.TabIndex = 1
        '
        ' Debugging publication and explicit Git-only finalization (ME01-09).
        '
        tlyDebug.AutoFitToTheme = False
        tlyDebug.Dock = DockStyle.Fill
        tlyDebug.Margin = New Padding(0)
        tlyDebug.ColumnCount = 3
        tlyDebug.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyDebug.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyDebug.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDebug.RowCount = 1
        tlyDebug.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDebug.Controls.Add(chkDebugging, 0, 0)
        tlyDebug.Controls.Add(btnFinalizeDebug, 1, 0)
        tlyDebug.Controls.Add(lblDebug, 2, 0)
        chkDebugging.Dock = DockStyle.Fill
        chkDebugging.Margin = New Padding(0)
        chkDebugging.Text = "Publicare debugging (fără Git)"
        btnFinalizeDebug.Dock = DockStyle.Fill
        btnFinalizeDebug.Margin = New Padding(0, 2, 6, 6)
        btnFinalizeDebug.Text = "Finalizează debugging"
        lblDebug.Dock = DockStyle.Fill
        lblDebug.AutoEllipsis = True
        lblDebug.TextAlign = ContentAlignment.MiddleLeft
        lblDebug.Text = "După test: folosiți bifele Debugging pentru finalizare Git; Trimite pentru încărcare."
        tips.SetToolTipHeader(chkDebugging, "Publicare debugging")
        tips.SetToolTipText(chkDebugging, "Trimite toate fișierele bifate fără Git, chiar dacă au slice și motiv. Datele Git existente sunt păstrate. Fără această bifă, fișierele fără informații Git sunt tot publicate fără Git.")
        tips.SetToolTipHeader(btnFinalizeDebug, "Finalizare debugging — numai Git")
        tips.SetToolTipText(btnFinalizeDebug, "Finalizează fișierele bifate în coloana Debugging, independent de coloana Trimite. La scanare, fișierele din evidența debugging sunt bifate în această coloană; debifați-le pe cele pe care nu doriți să le finalizați. Folosește direct slice-urile și motivele deja înregistrate și face numai Git, fără dialog suplimentar, reîncărcare sau repornire.")
        '
        ' btnScan
        '
        btnScan.Dock = DockStyle.Fill
        btnScan.Location = New Point(0, 0)
        btnScan.Margin = New Padding(0, 2, 6, 6)
        btnScan.Name = "btnScan"
        btnScan.Primary = True
        btnScan.Size = New Size(124, 36)
        btnScan.TabIndex = 2
        btnScan.Text = "Scanează"
        tips.SetToolTipHeader(btnScan, "Scanează")
        tips.SetToolTipText(btnScan, "Compară fișierele permise cu serverul și bifează automat în coloana Trimite numai fișierele noi sau modificate. Pentru cele deja publicate debugging compară conținutul; cele neschimbate rămân vizibile, nebifate la Trimite. Coloana Debugging este bifată pentru fișierele din evidența serverului și permite selectarea separată pentru finalizarea Git." & vbLf & "Citește și regulile din .pushignore.")
        '
        ' btnSend
        '
        btnSend.Dock = DockStyle.Fill
        btnSend.Location = New Point(0, 0)
        btnSend.Margin = New Padding(0, 2, 6, 6)
        btnSend.Name = "btnSend"
        btnSend.Size = New Size(164, 36)
        btnSend.TabIndex = 3
        btnSend.Text = "Trimite bifatele"
        tips.SetToolTipHeader(btnSend, "Trimite bifatele")
        tips.SetToolTipText(btnSend, "Încarcă pe server fișierele bifate în coloana Trimite. În modul debugging sau fără informații Git nu face Git și reține versiunile pentru finalizare. Altfel salvează în Git versiunile existente înainte de suprascriere.")
        '
        ' chkRestart
        '
        chkRestart.Dock = DockStyle.Fill
        chkRestart.Location = New Point(0, 0)
        chkRestart.Margin = New Padding(0)
        chkRestart.Name = "chkRestart"
        chkRestart.Size = New Size(190, 44)
        chkRestart.TabIndex = 4
        chkRestart.Text = "Repornește serviciul"
        tips.SetToolTipHeader(chkRestart, "Repornire")
        tips.SetToolTipText(chkRestart, "După trimitere: systemctl daemon-reload, systemctl restart avacont, apoi ultimele 30 de linii din journalctl." & vbLf & "Repornirea nu se face dacă nu trimiteți nimic.")
        chkRestart.UseVisualStyleBackColor = True
        '
        ' chkIdentical
        '
        chkIdentical.Dock = DockStyle.Fill
        chkIdentical.Location = New Point(0, 0)
        chkIdentical.Margin = New Padding(0)
        chkIdentical.Name = "chkIdentical"
        chkIdentical.Size = New Size(170, 44)
        chkIdentical.TabIndex = 5
        chkIdentical.Text = "Arată și identicele"
        tips.SetToolTipHeader(chkIdentical, "Fișiere identice")
        tips.SetToolTipText(chkIdentical, "Bifat: lista arată și fișierele identice cu cele de pe server (nebifate la Trimite). Fișierele debugging de finalizat rămân vizibile și cu această opțiune debifată; dacă sunt neschimbate, nu sunt bifate la Trimite. Selecția pentru finalizare folosește coloana Debugging.")
        chkIdentical.UseVisualStyleBackColor = True
        '
        ' btnAll
        '
        btnAll.Dock = DockStyle.Fill
        btnAll.Location = New Point(0, 0)
        btnAll.Margin = New Padding(0, 2, 6, 6)
        btnAll.Name = "btnAll"
        btnAll.Size = New Size(114, 36)
        btnAll.TabIndex = 6
        btnAll.Text = "Bifează tot"
        tips.SetToolTipHeader(btnAll, "Bifează tot")
        tips.SetToolTipText(btnAll, "Bifează coloana Trimite pentru toate fișierele din listă. Bifele Debugging rămân neschimbate.")
        '
        ' btnNone
        '
        btnNone.Dock = DockStyle.Fill
        btnNone.Location = New Point(0, 0)
        btnNone.Margin = New Padding(0, 2, 6, 6)
        btnNone.Name = "btnNone"
        btnNone.Size = New Size(124, 36)
        btnNone.TabIndex = 7
        btnNone.Text = "Debifează tot"
        tips.SetToolTipHeader(btnNone, "Debifează tot")
        tips.SetToolTipText(btnNone, "Debifează coloana Trimite pentru toate fișierele din listă. Bifele Debugging rămân neschimbate.")
        '
        ' lblRoot
        '
        tips.SetToolTipHeader(lblRoot, "Foldere")
        tips.SetToolTipText(lblRoot, "Folderul local scanat, rădăcina de pe server și câte reguli a citit din .pushignore." & vbLf & "Folderul local se schimbă din LocalRoot, în admin_settings.json.")
        lblRoot.AutoEllipsis = True
        lblRoot.Dock = DockStyle.Fill
        lblRoot.Location = New Point(0, 0)
        lblRoot.Margin = New Padding(0)
        lblRoot.Name = "lblRoot"
        lblRoot.Size = New Size(884, 24)
        lblRoot.TabIndex = 8
        lblRoot.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyMid
        '
        tlyMid.AutoFitToTheme = False
        tlyMid.ColumnCount = 2
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 68F))
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32F))
        tlyMid.Controls.Add(gridFiles, 0, 0)
        tlyMid.Controls.Add(tlySide, 1, 0)
        tlyMid.Dock = DockStyle.Fill
        tlyMid.Location = New Point(0, 0)
        tlyMid.Margin = New Padding(0)
        tlyMid.Name = "tlyMid"
        tlyMid.RowCount = 1
        tlyMid.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMid.Size = New Size(900, 560)
        tlyMid.TabIndex = 9
        '
        ' gridFiles
        '
        gridFiles.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridFiles.ColumnFillMode = KBotFillMode.SpecificColumn
        gridFilesCol1.AggregateFormatString = Nothing
        gridFilesCol1.ColumnType = KBotColumnType.CheckBox
        gridFilesCol1.FormatString = Nothing
        gridFilesCol1.HeaderText = "Trimite"
        gridFilesCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol1.Key = "send"
        gridFilesCol1.ValueType = KBotValueType.Boolean
        gridFilesCol1.MinWidth = 50
        gridFilesCol1.OptionGroup = Nothing
        gridFilesCol1.Width = 64
        gridFilesCol2.AggregateFormatString = Nothing
        gridFilesCol2.FormatString = Nothing
        gridFilesCol2.HeaderText = "Fișier"
        gridFilesCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol2.Key = "path"
        gridFilesCol2.MinWidth = 120
        gridFilesCol2.OptionGroup = Nothing
        gridFilesCol2.ReadOnly = True
        gridFilesCol2.Width = 320
        gridFilesCol3.AggregateFormatString = Nothing
        gridFilesCol3.FormatString = Nothing
        gridFilesCol3.HeaderText = "Stare"
        gridFilesCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol3.Key = "state"
        gridFilesCol3.MinWidth = 60
        gridFilesCol3.OptionGroup = Nothing
        gridFilesCol3.ReadOnly = True
        gridFilesCol3.Width = 90
        gridFilesCol4.AggregateFormatString = Nothing
        gridFilesCol4.FormatString = Nothing
        gridFilesCol4.HeaderText = "Ext."
        gridFilesCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol4.Key = "ext"
        gridFilesCol4.MinWidth = 40
        gridFilesCol4.OptionGroup = Nothing
        gridFilesCol4.ReadOnly = True
        gridFilesCol4.Width = 54
        gridFilesCol5.AggregateFormatString = Nothing
        gridFilesCol5.FormatString = Nothing
        gridFilesCol5.HeaderText = "Mărime locală (B)"
        gridFilesCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol5.Key = "size"
        gridFilesCol5.ValueType = KBotValueType.Number
        gridFilesCol5.FormatString = "N0"
        gridFilesCol5.MinWidth = 60
        gridFilesCol5.OptionGroup = Nothing
        gridFilesCol5.ReadOnly = True
        gridFilesCol5.TextAlign = ContentAlignment.MiddleRight
        gridFilesCol5.Width = 96
        gridFilesCol6.AggregateFormatString = Nothing
        gridFilesCol6.FormatString = Nothing
        gridFilesCol6.HeaderText = "Mărime server (B)"
        gridFilesCol6.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol6.Key = "srvsize"
        gridFilesCol6.ValueType = KBotValueType.Number
        gridFilesCol6.FormatString = "N0"
        gridFilesCol6.MinWidth = 60
        gridFilesCol6.OptionGroup = Nothing
        gridFilesCol6.ReadOnly = True
        gridFilesCol6.TextAlign = ContentAlignment.MiddleRight
        gridFilesCol6.Width = 96
        gridFilesCol7.AggregateFormatString = Nothing
        gridFilesCol7.FormatString = Nothing
        gridFilesCol7.HeaderText = "Local (UTC)"
        gridFilesCol7.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol7.Key = "local"
        gridFilesCol7.ValueType = KBotValueType.DateTime
        gridFilesCol7.FormatString = "yyyy-MM-dd HH:mm:ss"
        gridFilesCol7.MinWidth = 90
        gridFilesCol7.OptionGroup = Nothing
        gridFilesCol7.ReadOnly = True
        gridFilesCol7.Width = 140
        gridFilesCol8.AggregateFormatString = Nothing
        gridFilesCol8.FormatString = Nothing
        gridFilesCol8.HeaderText = "Server (UTC)"
        gridFilesCol8.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol8.Key = "server"
        gridFilesCol8.ValueType = KBotValueType.DateTime
        gridFilesCol8.FormatString = "yyyy-MM-dd HH:mm:ss"
        gridFilesCol8.MinWidth = 90
        gridFilesCol8.OptionGroup = Nothing
        gridFilesCol8.ReadOnly = True
        gridFilesCol8.Width = 140
        gridFiles.Columns.Add(gridFilesCol1)
        gridFiles.Columns.Add(gridFilesCol2)
        gridFiles.Columns.Add(gridFilesCol3)
        gridFilesDebug.Key = "debug"
        gridFilesDebug.ValueType = KBotValueType.Boolean
        gridFilesDebug.HeaderText = "Debugging"
        gridFilesDebug.ColumnType = KBotColumnType.CheckBox
        gridFilesDebug.ReadOnly = False
        gridFilesDebug.Width = 90
        gridFiles.Columns.Add(gridFilesDebug)
        gridFilesSlices.Key = "slices"
        gridFilesSlices.HeaderText = "Slice / nomenclator"
        gridFilesSlices.ReadOnly = True
        gridFilesSlices.Width = 190
        gridFilesReasons.Key = "reasons"
        gridFilesReasons.HeaderText = "Motiv"
        gridFilesReasons.ReadOnly = True
        gridFilesReasons.Width = 300
        gridFiles.Columns.Add(gridFilesSlices)
        gridFiles.Columns.Add(gridFilesReasons)
        gridFiles.Columns.Add(gridFilesCol4)
        gridFiles.Columns.Add(gridFilesCol5)
        gridFiles.Columns.Add(gridFilesCol6)
        gridFiles.Columns.Add(gridFilesCol7)
        gridFiles.Columns.Add(gridFilesCol8)
        gridFiles.Dock = DockStyle.Fill
        gridFiles.FillColumnKey = "path"
        gridFiles.Location = New Point(0, 0)
        gridFiles.Margin = New Padding(0)
        gridFiles.Name = "gridFiles"
        gridFiles.Size = New Size(600, 300)
        gridFiles.TabIndex = 10
        '
        ' tlySide
        '
        tlySide.AutoFitToTheme = False
        tlySide.ColumnCount = 1
        tlySide.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySide.Controls.Add(tlyExtBar, 0, 0)
        tlySide.Controls.Add(gridExt, 0, 1)
        tlySide.Controls.Add(tlyPatBar, 0, 2)
        tlySide.Controls.Add(gridPat, 0, 3)
        tlySide.Dock = DockStyle.Fill
        tlySide.Location = New Point(0, 0)
        tlySide.Margin = New Padding(0)
        tlySide.Name = "tlySide"
        tlySide.RowCount = 4
        tlySide.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlySide.RowStyles.Add(New RowStyle(SizeType.Percent, 55F))
        tlySide.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlySide.RowStyles.Add(New RowStyle(SizeType.Percent, 45F))
        tlySide.Size = New Size(900, 560)
        tlySide.TabIndex = 11
        '
        ' tlyExtBar
        '
        tlyExtBar.AutoFitToTheme = False
        tlyExtBar.ColumnCount = 2
        tlyExtBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyExtBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyExtBar.Controls.Add(btnExtAdd, 0, 0)
        tlyExtBar.Controls.Add(btnExtDel, 1, 0)
        tlyExtBar.Dock = DockStyle.Fill
        tlyExtBar.Location = New Point(0, 0)
        tlyExtBar.Margin = New Padding(0)
        tlyExtBar.Name = "tlyExtBar"
        tlyExtBar.RowCount = 1
        tlyExtBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyExtBar.Size = New Size(900, 560)
        tlyExtBar.TabIndex = 12
        '
        ' btnExtAdd
        '
        btnExtAdd.Dock = DockStyle.Fill
        btnExtAdd.Location = New Point(0, 0)
        btnExtAdd.Margin = New Padding(0, 2, 6, 6)
        btnExtAdd.Name = "btnExtAdd"
        btnExtAdd.Size = New Size(110, 36)
        btnExtAdd.TabIndex = 13
        btnExtAdd.Text = "Extensie nouă"
        tips.SetToolTipHeader(btnExtAdd, "Extensie nouă")
        tips.SetToolTipText(btnExtAdd, "Adaugă un rând gol în tabel: scrieți extensia (fără punct) și bifați «Acceptată».")
        '
        ' btnExtDel
        '
        btnExtDel.Dock = DockStyle.Fill
        btnExtDel.Location = New Point(0, 0)
        btnExtDel.Margin = New Padding(0, 2, 6, 6)
        btnExtDel.Name = "btnExtDel"
        btnExtDel.Size = New Size(110, 36)
        btnExtDel.TabIndex = 14
        btnExtDel.Text = "Șterge extensia"
        tips.SetToolTipHeader(btnExtDel, "Șterge extensia")
        tips.SetToolTipText(btnExtDel, "Scoate din setări extensia rândului ales. Fișierele cu ea nu se mai trimit.")
        '
        ' gridExt
        '
        gridExt.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridExt.ColumnFillMode = KBotFillMode.SpecificColumn
        gridExtCol1.AggregateFormatString = Nothing
        gridExtCol1.FormatString = Nothing
        gridExtCol1.HeaderText = "Extensie"
        gridExtCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol1.Key = "ext"
        gridExtCol1.MinWidth = 50
        gridExtCol1.OptionGroup = Nothing
        gridExtCol1.Width = 90
        gridExtCol2.AggregateFormatString = Nothing
        gridExtCol2.ColumnType = KBotColumnType.CheckBox
        gridExtCol2.FormatString = Nothing
        gridExtCol2.HeaderText = "Acceptată"
        gridExtCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol2.Key = "accepted"
        gridExtCol2.ValueType = KBotValueType.Boolean
        gridExtCol2.MinWidth = 60
        gridExtCol2.OptionGroup = Nothing
        gridExtCol2.Width = 80
        gridExtCol3.AggregateFormatString = Nothing
        gridExtCol3.FormatString = Nothing
        gridExtCol3.HeaderText = "Găsite"
        gridExtCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol3.Key = "found"
        gridExtCol3.ValueType = KBotValueType.Number
        gridExtCol3.MinWidth = 50
        gridExtCol3.OptionGroup = Nothing
        gridExtCol3.ReadOnly = True
        gridExtCol3.TextAlign = ContentAlignment.MiddleRight
        gridExtCol3.Width = 64
        gridExtCol4.AggregateFormatString = Nothing
        gridExtCol4.FormatString = Nothing
        gridExtCol4.HeaderText = "De trimis"
        gridExtCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol4.Key = "tosend"
        gridExtCol4.ValueType = KBotValueType.Number
        gridExtCol4.MinWidth = 50
        gridExtCol4.OptionGroup = Nothing
        gridExtCol4.ReadOnly = True
        gridExtCol4.TextAlign = ContentAlignment.MiddleRight
        gridExtCol4.Width = 70
        gridExt.Columns.Add(gridExtCol1)
        gridExt.Columns.Add(gridExtCol2)
        gridExt.Columns.Add(gridExtCol3)
        gridExt.Columns.Add(gridExtCol4)
        gridExt.Dock = DockStyle.Fill
        gridExt.FillColumnKey = "ext"
        gridExt.Location = New Point(0, 0)
        gridExt.Margin = New Padding(0)
        gridExt.Name = "gridExt"
        gridExt.Size = New Size(600, 300)
        gridExt.TabIndex = 15
        '
        ' tlyPatBar
        '
        tlyPatBar.AutoFitToTheme = False
        tlyPatBar.ColumnCount = 2
        tlyPatBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyPatBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyPatBar.Controls.Add(btnPatAdd, 0, 0)
        tlyPatBar.Controls.Add(btnPatDel, 1, 0)
        tlyPatBar.Dock = DockStyle.Fill
        tlyPatBar.Location = New Point(0, 0)
        tlyPatBar.Margin = New Padding(0)
        tlyPatBar.Name = "tlyPatBar"
        tlyPatBar.RowCount = 1
        tlyPatBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 36F))
        tlyPatBar.Size = New Size(900, 560)
        tlyPatBar.TabIndex = 16
        '
        ' btnPatAdd
        '
        btnPatAdd.Dock = DockStyle.Fill
        btnPatAdd.Location = New Point(0, 0)
        btnPatAdd.Margin = New Padding(0, 2, 6, 6)
        btnPatAdd.Name = "btnPatAdd"
        btnPatAdd.Size = New Size(110, 36)
        btnPatAdd.TabIndex = 17
        btnPatAdd.Text = "Regulă nouă"
        tips.SetToolTipHeader(btnPatAdd, "Regulă nouă")
        tips.SetToolTipText(btnPatAdd, "Adaugă un rând gol: un nume de dosar sau fișier (ex. node_modules) sau *.extensie (ex. *.bak).")
        '
        ' btnPatDel
        '
        btnPatDel.Dock = DockStyle.Fill
        btnPatDel.Location = New Point(0, 0)
        btnPatDel.Margin = New Padding(0, 2, 6, 6)
        btnPatDel.Name = "btnPatDel"
        btnPatDel.Size = New Size(110, 36)
        btnPatDel.TabIndex = 18
        btnPatDel.Text = "Șterge regula"
        tips.SetToolTipHeader(btnPatDel, "Șterge regula")
        tips.SetToolTipText(btnPatDel, "Scoate regula rândului ales din setări.")
        '
        ' gridPat
        '
        gridPat.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridPat.ColumnFillMode = KBotFillMode.SpecificColumn
        gridPatCol1.AggregateFormatString = Nothing
        gridPatCol1.FormatString = Nothing
        gridPatCol1.HeaderText = "Regulă de ignorare"
        gridPatCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridPatCol1.Key = "pattern"
        gridPatCol1.MinWidth = 80
        gridPatCol1.OptionGroup = Nothing
        gridPatCol1.Width = 170
        gridPatCol2.AggregateFormatString = Nothing
        gridPatCol2.FormatString = Nothing
        gridPatCol2.HeaderText = "Sărite"
        gridPatCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridPatCol2.Key = "skipped"
        gridPatCol2.ValueType = KBotValueType.Number
        gridPatCol2.MinWidth = 50
        gridPatCol2.OptionGroup = Nothing
        gridPatCol2.ReadOnly = True
        gridPatCol2.TextAlign = ContentAlignment.MiddleRight
        gridPatCol2.Width = 64
        gridPat.Columns.Add(gridPatCol1)
        gridPat.Columns.Add(gridPatCol2)
        gridPat.Dock = DockStyle.Fill
        gridPat.FillColumnKey = "pattern"
        gridPat.Location = New Point(0, 0)
        gridPat.Margin = New Padding(0)
        gridPat.Name = "gridPat"
        gridPat.Size = New Size(600, 300)
        gridPat.TabIndex = 19
        '
        ' logBox
        '
        logBox.Dock = DockStyle.Fill
        logBox.Location = New Point(0, 0)
        logBox.Margin = New Padding(0, 0, 0, 6)
        logBox.Name = "logBox"
        logBox.Size = New Size(884, 150)
        logBox.TabIndex = 20
        logBox.Text = ""
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Rezultatul ultimei scanări sau trimiteri: câte fișiere, câte de trimis, câte au dat eroare.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 24)
        lblState.TabIndex = 21
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' PushFilesView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "PushFilesView"
        Size = New Size(900, 560)
        CType(gridFiles, ComponentModel.ISupportInitialize).EndInit()
        CType(gridExt, ComponentModel.ISupportInitialize).EndInit()
        CType(gridPat, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyMid.ResumeLayout(False)
        tlySide.ResumeLayout(False)
        tlyExtBar.ResumeLayout(False)
        tlyPatBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents tlyDebug As KBotTableLayoutPanel
    Friend WithEvents chkDebugging As KBotCheckBox
    Friend WithEvents btnFinalizeDebug As KBotButton
    Friend WithEvents lblDebug As Label
    Friend WithEvents btnScan As KBotButton
    Friend WithEvents btnSend As KBotButton
    Friend WithEvents chkRestart As KBotCheckBox
    Friend WithEvents chkIdentical As KBotCheckBox
    Friend WithEvents btnAll As KBotButton
    Friend WithEvents btnNone As KBotButton
    Friend WithEvents lblRoot As Label
    Friend WithEvents tlyMid As KBotTableLayoutPanel
    Friend WithEvents gridFiles As AdminDataView
    Friend WithEvents tlySide As KBotTableLayoutPanel
    Friend WithEvents tlyExtBar As KBotTableLayoutPanel
    Friend WithEvents btnExtAdd As KBotButton
    Friend WithEvents btnExtDel As KBotButton
    Friend WithEvents gridExt As AdminDataView
    Friend WithEvents tlyPatBar As KBotTableLayoutPanel
    Friend WithEvents btnPatAdd As KBotButton
    Friend WithEvents btnPatDel As KBotButton
    Friend WithEvents gridPat As AdminDataView
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents lblState As Label
End Class
