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
        Dim gridExtCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridExtCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridPatCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridPatCol2 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnScan = New KBotButton()
        btnSend = New KBotButton()
        chkRestart = New KBotCheckBox()
        chkIdentical = New KBotCheckBox()
        btnAll = New KBotButton()
        btnNone = New KBotButton()
        lblRoot = New Label()
        tlyMid = New KBotTableLayoutPanel()
        gridFiles = New KBotDataView()
        tlySide = New KBotTableLayoutPanel()
        tlyExtBar = New KBotTableLayoutPanel()
        btnExtAdd = New KBotButton()
        btnExtDel = New KBotButton()
        gridExt = New KBotDataView()
        tlyPatBar = New KBotTableLayoutPanel()
        btnPatAdd = New KBotButton()
        btnPatDel = New KBotButton()
        gridPat = New KBotDataView()
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
        tlyMain.Controls.Add(lblRoot, 0, 1)
        tlyMain.Controls.Add(tlyMid, 0, 2)
        tlyMain.Controls.Add(logBox, 0, 3)
        tlyMain.Controls.Add(lblState, 0, 4)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 5
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
        tips.SetToolTipText(btnScan, "Se conectează prin SFTP, compară fiecare fișier permis cu cel de pe server și arată ce e nou sau modificat." & vbLf & "Citește și regulile din .pushignore.")
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
        tips.SetToolTipText(btnSend, "Încarcă pe server fișierele bifate, creând dosarele care lipsesc." & vbLf & "Ora serverului se aliniază cu a fișierului local, ca o nouă scanare să le vadă identice.")
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
        tips.SetToolTipText(chkIdentical, "Bifat: lista arată și fișierele identice cu cele de pe server (debifate). Implicit se văd doar cele de trimis.")
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
        tips.SetToolTipText(btnAll, "Bifează toate fișierele din listă.")
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
        tips.SetToolTipText(btnNone, "Debifează toate fișierele din listă.")
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
        gridFilesCol1.HeaderTooltip = "Bifat = fișierul se trimite la «Trimite bifatele». Fișierele noi și cele modificate vin bifate."
        gridFilesCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol1.Key = "send"
        gridFilesCol1.MinWidth = 50
        gridFilesCol1.OptionGroup = Nothing
        gridFilesCol1.Width = 64
        gridFilesCol2.AggregateFormatString = Nothing
        gridFilesCol2.FormatString = Nothing
        gridFilesCol2.HeaderText = "Fișier"
        gridFilesCol2.HeaderTooltip = "Calea față de rădăcina locală; pe server ajunge sub rădăcina de pe server."
        gridFilesCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol2.Key = "path"
        gridFilesCol2.MinWidth = 120
        gridFilesCol2.OptionGroup = Nothing
        gridFilesCol2.ReadOnly = True
        gridFilesCol2.Width = 320
        gridFilesCol3.AggregateFormatString = Nothing
        gridFilesCol3.FormatString = Nothing
        gridFilesCol3.HeaderText = "Stare"
        gridFilesCol3.HeaderTooltip = "NOU = nu există pe server; MODIFICAT = local e mai nou cu peste 2 secunde; IDENTIC = nimic de trimis."
        gridFilesCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol3.Key = "state"
        gridFilesCol3.MinWidth = 60
        gridFilesCol3.OptionGroup = Nothing
        gridFilesCol3.ReadOnly = True
        gridFilesCol3.Width = 90
        gridFilesCol4.AggregateFormatString = Nothing
        gridFilesCol4.FormatString = Nothing
        gridFilesCol4.HeaderText = "Ext."
        gridFilesCol4.HeaderTooltip = "Extensia fișierului."
        gridFilesCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol4.Key = "ext"
        gridFilesCol4.MinWidth = 40
        gridFilesCol4.OptionGroup = Nothing
        gridFilesCol4.ReadOnly = True
        gridFilesCol4.Width = 54
        gridFilesCol5.AggregateFormatString = Nothing
        gridFilesCol5.FormatString = Nothing
        gridFilesCol5.HeaderText = "Mărime locală"
        gridFilesCol5.HeaderTooltip = "Mărimea fișierului de pe acest calculator."
        gridFilesCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol5.Key = "size"
        gridFilesCol5.MinWidth = 60
        gridFilesCol5.OptionGroup = Nothing
        gridFilesCol5.ReadOnly = True
        gridFilesCol5.TextAlign = ContentAlignment.MiddleRight
        gridFilesCol5.Width = 96
        gridFilesCol6.AggregateFormatString = Nothing
        gridFilesCol6.FormatString = Nothing
        gridFilesCol6.HeaderText = "Mărime server"
        gridFilesCol6.HeaderTooltip = "Mărimea fișierului de pe server (gol dacă nu există)."
        gridFilesCol6.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol6.Key = "srvsize"
        gridFilesCol6.MinWidth = 60
        gridFilesCol6.OptionGroup = Nothing
        gridFilesCol6.ReadOnly = True
        gridFilesCol6.TextAlign = ContentAlignment.MiddleRight
        gridFilesCol6.Width = 96
        gridFilesCol7.AggregateFormatString = Nothing
        gridFilesCol7.FormatString = Nothing
        gridFilesCol7.HeaderText = "Local (UTC)"
        gridFilesCol7.HeaderTooltip = "Ora ultimei modificări a fișierului local."
        gridFilesCol7.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol7.Key = "local"
        gridFilesCol7.MinWidth = 90
        gridFilesCol7.OptionGroup = Nothing
        gridFilesCol7.ReadOnly = True
        gridFilesCol7.Width = 140
        gridFilesCol8.AggregateFormatString = Nothing
        gridFilesCol8.FormatString = Nothing
        gridFilesCol8.HeaderText = "Server (UTC)"
        gridFilesCol8.HeaderTooltip = "Ora ultimei modificări a fișierului de pe server."
        gridFilesCol8.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridFilesCol8.Key = "server"
        gridFilesCol8.MinWidth = 90
        gridFilesCol8.OptionGroup = Nothing
        gridFilesCol8.ReadOnly = True
        gridFilesCol8.Width = 140
        gridFiles.Columns.Add(gridFilesCol1)
        gridFiles.Columns.Add(gridFilesCol2)
        gridFiles.Columns.Add(gridFilesCol3)
        gridFiles.Columns.Add(gridFilesCol4)
        gridFiles.Columns.Add(gridFilesCol5)
        gridFiles.Columns.Add(gridFilesCol6)
        gridFiles.Columns.Add(gridFilesCol7)
        gridFiles.Columns.Add(gridFilesCol8)
        gridFiles.Dock = DockStyle.Fill
        gridFiles.FillColumnKey = "path"
        gridFiles.HeaderHeight = 24
        gridFiles.Location = New Point(0, 0)
        gridFiles.Margin = New Padding(0)
        gridFiles.Name = "gridFiles"
        gridFiles.Size = New Size(600, 300)
        gridFiles.TabIndex = 10
        tips.SetToolTipHeader(gridFiles, "Fișierele")
        tips.SetToolTipText(gridFiles, "Un rând = un fișier permis de reguli. Bifa din prima coloană spune dacă se trimite." & vbLf & "Coloanele arată mărimea și ora locală față de server, ca să vedeți ce s-a schimbat.")
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
        gridExtCol1.HeaderTooltip = "Extensia fără punct. Se poate edita direct; o extensie nouă se adaugă cu butonul de sub tabel."
        gridExtCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol1.Key = "ext"
        gridExtCol1.MinWidth = 50
        gridExtCol1.OptionGroup = Nothing
        gridExtCol1.Width = 90
        gridExtCol2.AggregateFormatString = Nothing
        gridExtCol2.ColumnType = KBotColumnType.CheckBox
        gridExtCol2.FormatString = Nothing
        gridExtCol2.HeaderText = "Acceptată"
        gridExtCol2.HeaderTooltip = "Bifat = fișierele cu această extensie se compară și se trimit. Debifat = sunt ignorate. Modificarea se salvează pe loc; se vede la următoarea scanare."
        gridExtCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol2.Key = "accepted"
        gridExtCol2.MinWidth = 60
        gridExtCol2.OptionGroup = Nothing
        gridExtCol2.Width = 80
        gridExtCol3.AggregateFormatString = Nothing
        gridExtCol3.FormatString = Nothing
        gridExtCol3.HeaderText = "Găsite"
        gridExtCol3.HeaderTooltip = "Câte fișiere cu această extensie are folderul local (fără cele oprite de reguli de ignorare)."
        gridExtCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol3.Key = "found"
        gridExtCol3.MinWidth = 50
        gridExtCol3.OptionGroup = Nothing
        gridExtCol3.ReadOnly = True
        gridExtCol3.TextAlign = ContentAlignment.MiddleRight
        gridExtCol3.Width = 64
        gridExtCol4.AggregateFormatString = Nothing
        gridExtCol4.FormatString = Nothing
        gridExtCol4.HeaderText = "De trimis"
        gridExtCol4.HeaderTooltip = "Câte dintre ele sunt noi sau modificate (se numără doar pentru extensiile acceptate)."
        gridExtCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridExtCol4.Key = "tosend"
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
        gridExt.HeaderHeight = 24
        gridExt.Location = New Point(0, 0)
        gridExt.Margin = New Padding(0)
        gridExt.Name = "gridExt"
        gridExt.Size = New Size(600, 300)
        gridExt.TabIndex = 15
        tips.SetToolTipHeader(gridExt, "Extensii")
        tips.SetToolTipText(gridExt, "Ce extensii se trimit (bifate) și care sunt ignorate (debifate), cu câte fișiere s-au găsit." & vbLf & "Se poate edita direct; setările se salvează în admin_settings.json.")
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
        gridPatCol1.HeaderTooltip = "Un nume de dosar sau fișier (oriunde în arbore) ori *.extensie. Se poate edita direct; salvarea e automată."
        gridPatCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridPatCol1.Key = "pattern"
        gridPatCol1.MinWidth = 80
        gridPatCol1.OptionGroup = Nothing
        gridPatCol1.Width = 170
        gridPatCol2.AggregateFormatString = Nothing
        gridPatCol2.FormatString = Nothing
        gridPatCol2.HeaderText = "Sărite"
        gridPatCol2.HeaderTooltip = "Câte fișiere a oprit regula la ultima scanare."
        gridPatCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridPatCol2.Key = "skipped"
        gridPatCol2.MinWidth = 50
        gridPatCol2.OptionGroup = Nothing
        gridPatCol2.ReadOnly = True
        gridPatCol2.TextAlign = ContentAlignment.MiddleRight
        gridPatCol2.Width = 64
        gridPat.Columns.Add(gridPatCol1)
        gridPat.Columns.Add(gridPatCol2)
        gridPat.Dock = DockStyle.Fill
        gridPat.FillColumnKey = "pattern"
        gridPat.HeaderHeight = 24
        gridPat.Location = New Point(0, 0)
        gridPat.Margin = New Padding(0)
        gridPat.Name = "gridPat"
        gridPat.Size = New Size(600, 300)
        gridPat.TabIndex = 19
        tips.SetToolTipHeader(gridPat, "Reguli de ignorare")
        tips.SetToolTipText(gridPat, "Dosare, nume de fișiere și *.extensii care nu se scanează niciodată." & vbLf & "Pentru reguli în stil gitignore (cu excepții) folosiți fișierul .pushignore din folderul local.")
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
    Friend WithEvents btnScan As KBotButton
    Friend WithEvents btnSend As KBotButton
    Friend WithEvents chkRestart As KBotCheckBox
    Friend WithEvents chkIdentical As KBotCheckBox
    Friend WithEvents btnAll As KBotButton
    Friend WithEvents btnNone As KBotButton
    Friend WithEvents lblRoot As Label
    Friend WithEvents tlyMid As KBotTableLayoutPanel
    Friend WithEvents gridFiles As KBotDataView
    Friend WithEvents tlySide As KBotTableLayoutPanel
    Friend WithEvents tlyExtBar As KBotTableLayoutPanel
    Friend WithEvents btnExtAdd As KBotButton
    Friend WithEvents btnExtDel As KBotButton
    Friend WithEvents gridExt As KBotDataView
    Friend WithEvents tlyPatBar As KBotTableLayoutPanel
    Friend WithEvents btnPatAdd As KBotButton
    Friend WithEvents btnPatDel As KBotButton
    Friend WithEvents gridPat As KBotDataView
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents lblState As Label
End Class
