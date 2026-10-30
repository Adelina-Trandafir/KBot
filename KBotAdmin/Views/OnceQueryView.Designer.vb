' The «Interogari unice» page of «Publicare» (slice ME01): one query text run once on AVACONT_SURSA and on the databases ticked in the main tree, with a ledger on the server.
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OnceQueryView
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
        Dim gridHistCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol5 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol6 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        txtName = New KBotTextField()
        btnFile = New KBotButton()
        btnView = New KBotButton()
        btnRun = New KBotButton()
        btnStatus = New KBotButton()
        tlyMid = New KBotTableLayoutPanel()
        txtSql = New KBotSqlBox()
        tlyBottom = New KBotTableLayoutPanel()
        logBox = New KBotLogBox()
        gridHist = New KBotDataView()
        lblState = New Label()
        CType(gridHist, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyMid.SuspendLayout()
        tlyBottom.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(tlyMid, 0, 1)
        tlyMain.Controls.Add(tlyBottom, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 60F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 6
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(txtName, 0, 0)
        tlyBar.Controls.Add(btnFile, 1, 0)
        tlyBar.Controls.Add(btnView, 2, 0)
        tlyBar.Controls.Add(btnRun, 3, 0)
        tlyBar.Controls.Add(btnStatus, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(0, 0)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyBar.Size = New Size(900, 560)
        tlyBar.TabIndex = 1
        '
        ' txtName
        '
        txtName.BackColor = Color.Transparent
        txtName.Dock = DockStyle.Fill
        txtName.Location = New Point(0, 0)
        txtName.Margin = New Padding(0, 4, 8, 6)
        txtName.Name = "txtName"
        txtName.PlaceholderText = "Nume interogare"
        txtName.Size = New Size(212, 34)
        txtName.TextPadding = New Padding(8, 0, 8, 0)
        txtName.TabIndex = 2
        tips.SetToolTipHeader(txtName, "Numele interogării")
        tips.SetToolTipText(txtName, "3-100 de caractere: litere, cifre, _ . -" & vbLf & "Numele și textul alcătuiesc identitatea interogării. Același text e sărit; același nume cu alt text e refuzat.")
        '
        ' btnFile
        '
        btnFile.Dock = DockStyle.Fill
        btnFile.Location = New Point(0, 0)
        btnFile.Margin = New Padding(0, 2, 6, 6)
        btnFile.Name = "btnFile"
        btnFile.Size = New Size(124, 36)
        btnFile.TabIndex = 3
        btnFile.Text = "Din fișier…"
        tips.SetToolTipHeader(btnFile, "Din fișier")
        tips.SetToolTipText(btnFile, "Încarcă textul dintr-un fișier .sql. Dacă numele e gol, ia numele fișierului.")
        '
        ' btnView
        '
        btnView.Dock = DockStyle.Fill
        btnView.Location = New Point(0, 0)
        btnView.Margin = New Padding(0, 2, 6, 6)
        btnView.Name = "btnView"
        btnView.Size = New Size(144, 36)
        btnView.TabIndex = 4
        btnView.Text = "Vezi (nu execută)"
        tips.SetToolTipHeader(btnView, "Vezi")
        tips.SetToolTipText(btnView, "Arată, pe fiecare bază, dacă interogarea e de rulat (PENDING), deja rulată (DONE) sau în conflict. Nu scrie nimic, nici măcar registrul.")
        '
        ' btnRun
        '
        btnRun.Dock = DockStyle.Fill
        btnRun.Location = New Point(0, 0)
        btnRun.Margin = New Padding(0, 2, 6, 6)
        btnRun.Name = "btnRun"
        btnRun.Primary = True
        btnRun.Size = New Size(114, 36)
        btnRun.TabIndex = 5
        btnRun.Text = "Execută"
        tips.SetToolTipHeader(btnRun, "Execută")
        tips.SetToolTipText(btnRun, "Rulează interogarea întâi pe AVACONT_SURSA, apoi pe bazele bifate în arborele din stânga. O bază care a rulat deja textul e sărită; modificările nu se pot anula.")
        '
        ' btnStatus
        '
        btnStatus.Dock = DockStyle.Fill
        btnStatus.Location = New Point(0, 0)
        btnStatus.Margin = New Padding(0, 2, 6, 6)
        btnStatus.Name = "btnStatus"
        btnStatus.Size = New Size(164, 36)
        btnStatus.TabIndex = 6
        btnStatus.Text = "Reîncarcă istoricul"
        tips.SetToolTipHeader(btnStatus, "Istoric")
        tips.SetToolTipText(btnStatus, "Citește din nou de pe server lista interogărilor unice rulate (din registrul Interogari_Unice).")
        '
        ' tlyMid
        '
        tlyMid.AutoFitToTheme = False
        tlyMid.ColumnCount = 1
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMid.Controls.Add(txtSql, 0, 0)
        tlyMid.Dock = DockStyle.Fill
        tlyMid.Location = New Point(0, 0)
        tlyMid.Margin = New Padding(0)
        tlyMid.Name = "tlyMid"
        tlyMid.RowCount = 1
        tlyMid.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMid.Size = New Size(900, 560)
        tlyMid.TabIndex = 9
        '
        ' txtSql
        '
        txtSql.AcceptsTab = True
        txtSql.Dock = DockStyle.Fill
        txtSql.Location = New Point(0, 0)
        txtSql.Margin = New Padding(0, 0, 0, 6)
        txtSql.MaxLength = 100000
        txtSql.Name = "txtSql"
        txtSql.Size = New Size(600, 200)
        txtSql.TabIndex = 10
        txtSql.WordWrap = False
        tips.SetToolTipHeader(txtSql, "Textul interogării")
        tips.SetToolTipText(txtSql, "Instrucțiuni separate prin ;. Numele de tabele NU se calificează: fiecare bază rulează pe ea însăși." & vbLf & "DELIMITER, DROP DATABASE și DROP SCHEMA sunt refuzate. Cel mult 30.000 de octeți." & vbLf & "Scrieți-o ca să nu strice dacă rulează de două ori (IF EXISTS, WHERE pe ce rămâne de reparat).")
        '
        ' tlyBottom
        '
        tlyBottom.AutoFitToTheme = False
        tlyBottom.ColumnCount = 2
        tlyBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42F))
        tlyBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58F))
        tlyBottom.Controls.Add(logBox, 0, 0)
        tlyBottom.Controls.Add(gridHist, 1, 0)
        tlyBottom.Dock = DockStyle.Fill
        tlyBottom.Location = New Point(0, 0)
        tlyBottom.Margin = New Padding(0)
        tlyBottom.Name = "tlyBottom"
        tlyBottom.RowCount = 1
        tlyBottom.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBottom.Size = New Size(900, 560)
        tlyBottom.TabIndex = 12
        '
        ' logBox
        '
        logBox.Dock = DockStyle.Fill
        logBox.Location = New Point(0, 0)
        logBox.Margin = New Padding(0, 0, 0, 6)
        logBox.Name = "logBox"
        logBox.Size = New Size(884, 150)
        logBox.TabIndex = 13
        logBox.Text = ""
        '
        ' gridHist
        '
        gridHist.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridHist.ColumnFillMode = KBotFillMode.SpecificColumn
        gridHistCol1.AggregateFormatString = Nothing
        gridHistCol1.FormatString = Nothing
        gridHistCol1.HeaderText = "Interogarea"
        gridHistCol1.HeaderTooltip = "Numele dat interogării."
        gridHistCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol1.Key = "name"
        gridHistCol1.MinWidth = 80
        gridHistCol1.OptionGroup = Nothing
        gridHistCol1.ReadOnly = True
        gridHistCol1.Width = 170
        gridHistCol2.AggregateFormatString = Nothing
        gridHistCol2.FormatString = Nothing
        gridHistCol2.HeaderText = "Rulată la"
        gridHistCol2.HeaderTooltip = "Când a rulat pe AVACONT_SURSA (șablonul, prima bază la fiecare rulare)."
        gridHistCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol2.Key = "ranat"
        gridHistCol2.MinWidth = 90
        gridHistCol2.OptionGroup = Nothing
        gridHistCol2.ReadOnly = True
        gridHistCol2.Width = 130
        gridHistCol3.AggregateFormatString = Nothing
        gridHistCol3.FormatString = Nothing
        gridHistCol3.HeaderText = "De cine"
        gridHistCol3.HeaderTooltip = "Cine a rulat-o (AvacontPush sau KBotAdmin), sau «provizionare» dacă unitatea s-a născut cu ea marcată ca rulată."
        gridHistCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol3.Key = "by"
        gridHistCol3.MinWidth = 60
        gridHistCol3.OptionGroup = Nothing
        gridHistCol3.ReadOnly = True
        gridHistCol3.Width = 90
        gridHistCol4.AggregateFormatString = Nothing
        gridHistCol4.FormatString = Nothing
        gridHistCol4.HeaderText = "Rânduri"
        gridHistCol4.HeaderTooltip = "Rândurile raportate ca modificate de instrucțiuni pe șablon."
        gridHistCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol4.Key = "rows"
        gridHistCol4.MinWidth = 50
        gridHistCol4.OptionGroup = Nothing
        gridHistCol4.ReadOnly = True
        gridHistCol4.TextAlign = ContentAlignment.MiddleRight
        gridHistCol4.Width = 70
        gridHistCol5.AggregateFormatString = Nothing
        gridHistCol5.FormatString = Nothing
        gridHistCol5.HeaderText = "Baze"
        gridHistCol5.HeaderTooltip = "Câte baze de unitate au interogarea marcată ca rulată, din câte există."
        gridHistCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol5.Key = "done"
        gridHistCol5.MinWidth = 50
        gridHistCol5.OptionGroup = Nothing
        gridHistCol5.ReadOnly = True
        gridHistCol5.TextAlign = ContentAlignment.MiddleRight
        gridHistCol5.Width = 80
        gridHistCol6.AggregateFormatString = Nothing
        gridHistCol6.FormatString = Nothing
        gridHistCol6.HeaderText = "Încă de rulat pe"
        gridHistCol6.HeaderTooltip = "Bazele de unitate unde interogarea nu a rulat încă."
        gridHistCol6.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol6.Key = "missing"
        gridHistCol6.MinWidth = 80
        gridHistCol6.OptionGroup = Nothing
        gridHistCol6.ReadOnly = True
        gridHistCol6.Width = 220
        gridHist.Columns.Add(gridHistCol1)
        gridHist.Columns.Add(gridHistCol2)
        gridHist.Columns.Add(gridHistCol3)
        gridHist.Columns.Add(gridHistCol4)
        gridHist.Columns.Add(gridHistCol5)
        gridHist.Columns.Add(gridHistCol6)
        gridHist.Dock = DockStyle.Fill
        gridHist.FillColumnKey = "name"
        gridHist.HeaderHeight = 24
        gridHist.Location = New Point(0, 0)
        gridHist.Margin = New Padding(0)
        gridHist.Name = "gridHist"
        gridHist.ReadOnlyGrid = True
        gridHist.Size = New Size(600, 300)
        gridHist.TabIndex = 14
        tips.SetToolTipHeader(gridHist, "Istoricul interogărilor")
        tips.SetToolTipText(gridHist, "Ce interogări unice au rulat, cu rezultatul pe baze. Dublu clic pe un rând arată textul interogării.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Rezultatul ultimei acțiuni, în cuvinte: refuzat, cu erori sau încheiat. Codurile runner-ului: 0 reușit, 1 erori pe unele baze, 2 refuzat.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 24)
        lblState.TabIndex = 15
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' OnceQueryView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "OnceQueryView"
        Size = New Size(900, 560)
        CType(gridHist, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyMid.ResumeLayout(False)
        tlyBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents txtName As KBotTextField
    Friend WithEvents btnFile As KBotButton
    Friend WithEvents btnView As KBotButton
    Friend WithEvents btnRun As KBotButton
    Friend WithEvents btnStatus As KBotButton
    Friend WithEvents tlyMid As KBotTableLayoutPanel
    Friend WithEvents txtSql As KBotSqlBox
    Friend WithEvents tlyBottom As KBotTableLayoutPanel
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents gridHist As KBotDataView
    Friend WithEvents lblState As Label
End Class
