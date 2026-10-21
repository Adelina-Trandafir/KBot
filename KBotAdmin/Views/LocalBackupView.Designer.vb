' The "Copii locale" page (slice ME02): the encrypted copies on this PC - folder, new copy, check, SQL export, history of a row,
' clean-up and the daily task. All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LocalBackupView
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
        Dim SnapColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim SnapColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim TableColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim TableColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim TableColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim TableColumn4 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnFolder = New KBotButton()
        btnOpen = New KBotButton()
        btnRun = New KBotButton()
        btnVerify = New KBotButton()
        btnExport = New KBotButton()
        btnHistory = New KBotButton()
        btnPurge = New KBotButton()
        btnDelete = New KBotButton()
        tlySplit = New KBotTableLayoutPanel()
        gridSnaps = New KBotDataView()
        gridTables = New KBotDataView()
        lblState = New Label()
        CType(gridSnaps, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridTables, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlySplit.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(tlySplit, 0, 1)
        tlyMain.Controls.Add(lblState, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 9
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 84F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 92F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 112F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 92F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 104F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 108F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 84F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 84F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnFolder, 0, 0)
        tlyBar.Controls.Add(btnOpen, 1, 0)
        tlyBar.Controls.Add(btnRun, 2, 0)
        tlyBar.Controls.Add(btnVerify, 3, 0)
        tlyBar.Controls.Add(btnExport, 4, 0)
        tlyBar.Controls.Add(btnHistory, 5, 0)
        tlyBar.Controls.Add(btnPurge, 6, 0)
        tlyBar.Controls.Add(btnDelete, 7, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' btnFolder
        '
        btnFolder.Dock = DockStyle.Fill
        btnFolder.Location = New Point(0, 0)
        btnFolder.Margin = New Padding(0, 2, 6, 6)
        btnFolder.Name = "btnFolder"
        btnFolder.Size = New Size(78, 36)
        btnFolder.TabIndex = 0
        btnFolder.Text = "Dosar..."
        tips.SetToolTipHeader(btnFolder, "Dosarul copiilor")
        tips.SetToolTipText(btnFolder, "Alege dosarul în care se păstrează copiile locale (pe un disc cu loc destul). Dacă nu e creat încă, copia locală se creează în el la prima copie.")
        '
        ' btnOpen
        '
        btnOpen.Dock = DockStyle.Fill
        btnOpen.Location = New Point(84, 0)
        btnOpen.Margin = New Padding(0, 2, 6, 6)
        btnOpen.Name = "btnOpen"
        btnOpen.Size = New Size(86, 36)
        btnOpen.TabIndex = 1
        btnOpen.Text = "Deschide"
        tips.SetToolTipHeader(btnOpen, "Deschide copiile")
        tips.SetToolTipText(btnOpen, "Citește lista copiilor din dosar. Cere parola dacă cheia nu e păstrată pe acest calculator.")
        '
        ' btnRun
        '
        btnRun.Dock = DockStyle.Fill
        btnRun.Location = New Point(176, 0)
        btnRun.Margin = New Padding(0, 2, 6, 6)
        btnRun.Name = "btnRun"
        btnRun.Primary = True
        btnRun.Size = New Size(106, 36)
        btnRun.TabIndex = 2
        btnRun.Text = "Copie acum"
        tips.SetToolTipHeader(btnRun, "Copie locală acum")
        tips.SetToolTipText(btnRun, "Citește de pe server baza aleasă în stânga (sau toate bazele de unitate, fără 000_DEMO) și scrie în dosar doar ce s-a schimbat de la copia precedentă. Cere un motiv.")
        '
        ' btnVerify
        '
        btnVerify.Dock = DockStyle.Fill
        btnVerify.Location = New Point(288, 0)
        btnVerify.Margin = New Padding(0, 2, 6, 6)
        btnVerify.Name = "btnVerify"
        btnVerify.Size = New Size(86, 36)
        btnVerify.TabIndex = 3
        btnVerify.Text = "Verifică"
        tips.SetToolTipHeader(btnVerify, "Verifică copiile")
        tips.SetToolTipText(btnVerify, "Deschide fiecare fișier din dosar și controlează că nu e stricat și că fiecare copie își găsește toate fișierele. Nu atinge serverul.")
        '
        ' btnExport
        '
        btnExport.Dock = DockStyle.Fill
        btnExport.Location = New Point(380, 0)
        btnExport.Margin = New Padding(0, 2, 6, 6)
        btnExport.Name = "btnExport"
        btnExport.Size = New Size(98, 36)
        btnExport.TabIndex = 4
        btnExport.Text = "Export SQL"
        tips.SetToolTipHeader(btnExport, "Exportă copia aleasă ca SQL")
        tips.SetToolTipText(btnExport, "Scrie copia aleasă în listă ca fișier SQL pentru o bază de probă zz_restore_... Nu atinge nicio bază: fișierul îl rulați dumneavoastră.")
        '
        ' btnHistory
        '
        btnHistory.Dock = DockStyle.Fill
        btnHistory.Location = New Point(484, 0)
        btnHistory.Margin = New Padding(0, 2, 6, 6)
        btnHistory.Name = "btnHistory"
        btnHistory.Size = New Size(102, 36)
        btnHistory.TabIndex = 5
        btnHistory.Text = "Istoric rând"
        tips.SetToolTipHeader(btnHistory, "Istoricul unui rând")
        tips.SetToolTipText(btnHistory, "Arată cum a arătat un rând (după cheia lui primară) în toate copiile bazei alese, cu ce s-a schimbat de la una la alta.")
        '
        ' btnPurge
        '
        btnPurge.Dock = DockStyle.Fill
        btnPurge.Location = New Point(592, 0)
        btnPurge.Margin = New Padding(0, 2, 6, 6)
        btnPurge.Name = "btnPurge"
        btnPurge.Size = New Size(78, 36)
        btnPurge.TabIndex = 6
        btnPurge.Text = "Curăță"
        tips.SetToolTipHeader(btnPurge, "Curăță copiile vechi")
        tips.SetToolTipText(btnPurge, "Păstrează cele mai noi copii pe zile, săptămâni și luni (numerele din admin_settings.json) și șterge restul, apoi fișierele la care nu mai trimite nimic. Arată întâi ce se șterge.")
        '
        ' btnDelete
        '
        btnDelete.Dock = DockStyle.Fill
        btnDelete.Location = New Point(676, 0)
        btnDelete.Margin = New Padding(0, 2, 6, 6)
        btnDelete.Name = "btnDelete"
        btnDelete.Size = New Size(78, 36)
        btnDelete.TabIndex = 7
        btnDelete.Text = "Șterge"
        tips.SetToolTipHeader(btnDelete, "Șterge copia aleasă")
        tips.SetToolTipText(btnDelete, "Șterge definitiv copia aleasă în listă (și fișierele pe care nu le mai folosește nicio altă copie). Copiile făcute de mână se șterg doar așa, nu prin Curăță.")
        '
        ' tlySplit
        '
        tlySplit.AutoFitToTheme = False
        tlySplit.ColumnCount = 2
        tlySplit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 64F))
        tlySplit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 36F))
        tlySplit.Controls.Add(gridSnaps, 0, 0)
        tlySplit.Controls.Add(gridTables, 1, 0)
        tlySplit.Dock = DockStyle.Fill
        tlySplit.Location = New Point(8, 52)
        tlySplit.Margin = New Padding(0)
        tlySplit.Name = "tlySplit"
        tlySplit.RowCount = 1
        tlySplit.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySplit.Size = New Size(884, 460)
        tlySplit.TabIndex = 1
        '
        ' gridSnaps
        '
        gridSnaps.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridSnaps.ColumnFillMode = KBotFillMode.SpecificColumn
        SnapColumn1.AggregateFormatString = Nothing
        SnapColumn1.FormatString = Nothing
        SnapColumn1.HeaderText = "Baza"
        SnapColumn1.HeaderTooltip = "Baza de date copiată."
        SnapColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn1.Key = "db"
        SnapColumn1.MinWidth = 70
        SnapColumn1.OptionGroup = Nothing
        SnapColumn1.ReadOnly = True
        SnapColumn1.Width = 130
        SnapColumn2.AggregateFormatString = Nothing
        SnapColumn2.FormatString = Nothing
        SnapColumn2.HeaderText = "Data"
        SnapColumn2.HeaderTooltip = "Când s-a terminat copia."
        SnapColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn2.Key = "when"
        SnapColumn2.MinWidth = 90
        SnapColumn2.OptionGroup = Nothing
        SnapColumn2.ReadOnly = True
        SnapColumn2.Width = 128
        SnapColumn3.AggregateFormatString = Nothing
        SnapColumn3.FormatString = Nothing
        SnapColumn3.HeaderText = "Fel"
        SnapColumn3.HeaderTooltip = "manual = pornită de dumneavoastră; periodic = sarcina zilnică."
        SnapColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn3.Key = "kind"
        SnapColumn3.MinWidth = 50
        SnapColumn3.OptionGroup = Nothing
        SnapColumn3.ReadOnly = True
        SnapColumn3.Width = 70
        SnapColumn4.AggregateFormatString = Nothing
        SnapColumn4.FormatString = Nothing
        SnapColumn4.HeaderText = "Mod"
        SnapColumn4.HeaderTooltip = "fast = compară cu copia precedentă; full = verifică și conținutul PDF-urilor."
        SnapColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn4.Key = "mode"
        SnapColumn4.MinWidth = 40
        SnapColumn4.OptionGroup = Nothing
        SnapColumn4.ReadOnly = True
        SnapColumn4.Width = 56
        SnapColumn5.AggregateFormatString = Nothing
        SnapColumn5.FormatString = Nothing
        SnapColumn5.HeaderText = "Motiv"
        SnapColumn5.HeaderTooltip = "Motivul scris la copiile manuale."
        SnapColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn5.Key = "reason"
        SnapColumn5.MinWidth = 80
        SnapColumn5.OptionGroup = Nothing
        SnapColumn5.ReadOnly = True
        SnapColumn5.Width = 200
        SnapColumn6.AggregateFormatString = Nothing
        SnapColumn6.FormatString = Nothing
        SnapColumn6.HeaderText = "Tabele"
        SnapColumn6.HeaderTooltip = "Câte tabele are copia."
        SnapColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn6.Key = "tables"
        SnapColumn6.MinWidth = 40
        SnapColumn6.OptionGroup = Nothing
        SnapColumn6.ReadOnly = True
        SnapColumn6.TextAlign = ContentAlignment.MiddleRight
        SnapColumn6.Width = 60
        SnapColumn7.AggregateFormatString = Nothing
        SnapColumn7.FormatString = Nothing
        SnapColumn7.HeaderText = "Pagini noi"
        SnapColumn7.HeaderTooltip = "Câte pagini s-au scris acum (rândurile schimbate sau noi)."
        SnapColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn7.Key = "pagesNew"
        SnapColumn7.MinWidth = 40
        SnapColumn7.OptionGroup = Nothing
        SnapColumn7.ReadOnly = True
        SnapColumn7.TextAlign = ContentAlignment.MiddleRight
        SnapColumn7.Width = 70
        SnapColumn8.AggregateFormatString = Nothing
        SnapColumn8.FormatString = Nothing
        SnapColumn8.HeaderText = "La fel"
        SnapColumn8.HeaderTooltip = "Câte pagini erau neschimbate și nu au ocupat loc nou."
        SnapColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn8.Key = "pagesSame"
        SnapColumn8.MinWidth = 40
        SnapColumn8.OptionGroup = Nothing
        SnapColumn8.ReadOnly = True
        SnapColumn8.TextAlign = ContentAlignment.MiddleRight
        SnapColumn8.Width = 60
        SnapColumn9.AggregateFormatString = Nothing
        SnapColumn9.FormatString = Nothing
        SnapColumn9.HeaderText = "Rânduri"
        SnapColumn9.HeaderTooltip = "Rândurile din toate tabelele la acel moment."
        SnapColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn9.Key = "rows"
        SnapColumn9.MinWidth = 50
        SnapColumn9.OptionGroup = Nothing
        SnapColumn9.ReadOnly = True
        SnapColumn9.TextAlign = ContentAlignment.MiddleRight
        SnapColumn9.Width = 80
        SnapColumn10.AggregateFormatString = Nothing
        SnapColumn10.FormatString = Nothing
        SnapColumn10.HeaderText = "Durata"
        SnapColumn10.HeaderTooltip = "Cât a durat copia, în secunde."
        SnapColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        SnapColumn10.Key = "secs"
        SnapColumn10.MinWidth = 40
        SnapColumn10.OptionGroup = Nothing
        SnapColumn10.ReadOnly = True
        SnapColumn10.TextAlign = ContentAlignment.MiddleRight
        SnapColumn10.Width = 56
        gridSnaps.Columns.Add(SnapColumn1)
        gridSnaps.Columns.Add(SnapColumn2)
        gridSnaps.Columns.Add(SnapColumn3)
        gridSnaps.Columns.Add(SnapColumn4)
        gridSnaps.Columns.Add(SnapColumn5)
        gridSnaps.Columns.Add(SnapColumn6)
        gridSnaps.Columns.Add(SnapColumn7)
        gridSnaps.Columns.Add(SnapColumn8)
        gridSnaps.Columns.Add(SnapColumn9)
        gridSnaps.Columns.Add(SnapColumn10)
        gridSnaps.Dock = DockStyle.Fill
        gridSnaps.FillColumnKey = "reason"
        gridSnaps.HeaderHeight = 24
        gridSnaps.Location = New Point(0, 0)
        gridSnaps.Margin = New Padding(0, 0, 4, 0)
        gridSnaps.Name = "gridSnaps"
        gridSnaps.ReadOnlyGrid = True
        gridSnaps.Size = New Size(560, 460)
        gridSnaps.TabIndex = 0
        tips.SetToolTipHeader(gridSnaps, "Copiile locale")
        tips.SetToolTipText(gridSnaps, "Fiecare rând e o copie a unei baze la un moment dat, cele mai noi sus. Alegeți un rând ca să vedeți în dreapta tabelele lui și ce s-a schimbat față de copia precedentă.")
        '
        ' gridTables
        '
        gridTables.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridTables.ColumnFillMode = KBotFillMode.SpecificColumn
        TableColumn1.AggregateFormatString = Nothing
        TableColumn1.FormatString = Nothing
        TableColumn1.HeaderText = "Tabelă"
        TableColumn1.HeaderTooltip = "Tabela din copia aleasă."
        TableColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        TableColumn1.Key = "table"
        TableColumn1.MinWidth = 80
        TableColumn1.OptionGroup = Nothing
        TableColumn1.ReadOnly = True
        TableColumn1.Width = 170
        TableColumn2.AggregateFormatString = Nothing
        TableColumn2.FormatString = Nothing
        TableColumn2.HeaderText = "Rânduri"
        TableColumn2.HeaderTooltip = "Rândurile tabelei la acel moment."
        TableColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        TableColumn2.Key = "rows"
        TableColumn2.MinWidth = 40
        TableColumn2.OptionGroup = Nothing
        TableColumn2.ReadOnly = True
        TableColumn2.TextAlign = ContentAlignment.MiddleRight
        TableColumn2.Width = 70
        TableColumn3.AggregateFormatString = Nothing
        TableColumn3.FormatString = Nothing
        TableColumn3.HeaderText = "Pagini"
        TableColumn3.HeaderTooltip = "Din câte pagini e alcătuită tabela în copie."
        TableColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        TableColumn3.Key = "pages"
        TableColumn3.MinWidth = 36
        TableColumn3.OptionGroup = Nothing
        TableColumn3.ReadOnly = True
        TableColumn3.TextAlign = ContentAlignment.MiddleRight
        TableColumn3.Width = 56
        TableColumn4.AggregateFormatString = Nothing
        TableColumn4.FormatString = Nothing
        TableColumn4.HeaderText = "Schimbate"
        TableColumn4.HeaderTooltip = "Pagini diferite de copia precedentă a aceleiași baze (tabelele fără schimbare au 0)."
        TableColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        TableColumn4.Key = "changed"
        TableColumn4.MinWidth = 40
        TableColumn4.OptionGroup = Nothing
        TableColumn4.ReadOnly = True
        TableColumn4.TextAlign = ContentAlignment.MiddleRight
        TableColumn4.Width = 76
        gridTables.Columns.Add(TableColumn1)
        gridTables.Columns.Add(TableColumn2)
        gridTables.Columns.Add(TableColumn3)
        gridTables.Columns.Add(TableColumn4)
        gridTables.Dock = DockStyle.Fill
        gridTables.FillColumnKey = "table"
        gridTables.HeaderHeight = 24
        gridTables.Location = New Point(565, 0)
        gridTables.Margin = New Padding(4, 0, 0, 0)
        gridTables.Name = "gridTables"
        gridTables.ReadOnlyGrid = True
        gridTables.Size = New Size(320, 460)
        gridTables.TabIndex = 1
        tips.SetToolTipHeader(gridTables, "Tabelele copiei")
        tips.SetToolTipText(gridTables, "Tabelele copiei alese. Coloana Schimbate arată câte pagini diferă de copia precedentă a aceleiași baze. Alegeți o tabelă pentru «Istoric rând».")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Dosarul copiilor, câte copii are, cât ocupă pe disc și ce face operația în curs.")
        lblState.AutoEllipsis = False
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 512)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 44)
        lblState.TabIndex = 2
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' LocalBackupView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "LocalBackupView"
        Size = New Size(900, 560)
        CType(gridSnaps, ComponentModel.ISupportInitialize).EndInit()
        CType(gridTables, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlySplit.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnFolder As KBotButton
    Friend WithEvents btnOpen As KBotButton
    Friend WithEvents btnRun As KBotButton
    Friend WithEvents btnVerify As KBotButton
    Friend WithEvents btnExport As KBotButton
    Friend WithEvents btnHistory As KBotButton
    Friend WithEvents btnPurge As KBotButton
    Friend WithEvents btnDelete As KBotButton
    Friend WithEvents tlySplit As KBotTableLayoutPanel
    Friend WithEvents gridSnaps As KBotDataView
    Friend WithEvents gridTables As KBotDataView
    Friend WithEvents lblState As Label
End Class
