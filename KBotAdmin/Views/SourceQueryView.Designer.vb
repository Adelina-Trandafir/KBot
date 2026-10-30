' The «Interogare pe SURSA» page (sliceless): a .sql file read, run on AVACONT_SURSA alone, checked against its real structure, then carried to the units by the schema sync.
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SourceQueryView
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
        Dim gridObjCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridObjCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridObjCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridObjCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridObjCol5 As KBotDataColumn = New KBotDataColumn()
        Dim gridObjCol6 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridHistCol5 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyPath = New KBotTableLayoutPanel()
        txtPath = New KBotTextField()
        btnBrowse = New KBotButton()
        txtName = New KBotTextField()
        btnRead = New KBotButton()
        tlyBar = New KBotTableLayoutPanel()
        btnView = New KBotButton()
        btnRun = New KBotButton()
        btnCheck = New KBotButton()
        btnSync = New KBotButton()
        btnReload = New KBotButton()
        tlyMid = New KBotTableLayoutPanel()
        gridObj = New KBotDataView()
        gridHist = New KBotDataView()
        logBox = New KBotLogBox()
        lblState = New Label()
        CType(gridObj, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridHist, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyPath.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyMid.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyPath, 0, 0)
        tlyMain.Controls.Add(tlyBar, 0, 1)
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 45F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 55F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyPath
        '
        tlyPath.AutoFitToTheme = False
        tlyPath.ColumnCount = 4
        tlyPath.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyPath.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyPath.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyPath.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyPath.Controls.Add(txtPath, 0, 0)
        tlyPath.Controls.Add(btnBrowse, 1, 0)
        tlyPath.Controls.Add(txtName, 2, 0)
        tlyPath.Controls.Add(btnRead, 3, 0)
        tlyPath.Dock = DockStyle.Fill
        tlyPath.Location = New Point(0, 0)
        tlyPath.Margin = New Padding(0)
        tlyPath.Name = "tlyPath"
        tlyPath.RowCount = 1
        tlyPath.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyPath.Size = New Size(884, 44)
        tlyPath.TabIndex = 1
        '
        ' txtPath
        '
        txtPath.BackColor = Color.Transparent
        txtPath.Dock = DockStyle.Fill
        txtPath.Location = New Point(0, 0)
        txtPath.Margin = New Padding(0, 4, 8, 6)
        txtPath.Name = "txtPath"
        txtPath.PlaceholderText = "Calea fișierului .sql"
        txtPath.Size = New Size(500, 34)
        txtPath.TextPadding = New Padding(8, 0, 8, 0)
        txtPath.TabIndex = 2
        tips.SetToolTipHeader(txtPath, "Fișierul SQL")
        tips.SetToolTipText(txtPath, "Calea fișierului cu interogarea (UTF-8). Se poate scrie sau alege cu «Alege...»." & vbLf & "Numele de tabele NU se calificează cu baza: textul rulează pe AVACONT_SURSA.")
        '
        ' btnBrowse
        '
        btnBrowse.Dock = DockStyle.Fill
        btnBrowse.Location = New Point(0, 0)
        btnBrowse.Margin = New Padding(0, 2, 6, 6)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(104, 36)
        btnBrowse.TabIndex = 3
        btnBrowse.Text = "Alege…"
        tips.SetToolTipHeader(btnBrowse, "Alege fișierul")
        tips.SetToolTipText(btnBrowse, "Deschide un fișier .sql. Dacă numele e gol, ia numele fișierului.")
        '
        ' txtName
        '
        txtName.BackColor = Color.Transparent
        txtName.Dock = DockStyle.Fill
        txtName.Location = New Point(0, 0)
        txtName.Margin = New Padding(0, 4, 8, 6)
        txtName.Name = "txtName"
        txtName.PlaceholderText = "Nume interogare"
        txtName.Size = New Size(222, 34)
        txtName.TextPadding = New Padding(8, 0, 8, 0)
        txtName.TabIndex = 4
        tips.SetToolTipHeader(txtName, "Numele interogării")
        tips.SetToolTipText(txtName, "3-100 de caractere: litere, cifre, _ . -" & vbLf & "Numele și textul alcătuiesc identitatea interogării. Același text e sărit; același nume cu alt text e refuzat.")
        '
        ' btnRead
        '
        btnRead.Dock = DockStyle.Fill
        btnRead.Location = New Point(0, 0)
        btnRead.Margin = New Padding(0, 2, 0, 6)
        btnRead.Name = "btnRead"
        btnRead.Size = New Size(120, 36)
        btnRead.TabIndex = 5
        btnRead.Text = "Citește"
        tips.SetToolTipHeader(btnRead, "Citește")
        tips.SetToolTipText(btnRead, "Citește fișierul și arată, pe serverul de pe care rulează, ce atinge fiecare instrucțiune: tabele, coloane, indexuri, chei. Nu rulează nimic.")
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 6
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnView, 0, 0)
        tlyBar.Controls.Add(btnRun, 1, 0)
        tlyBar.Controls.Add(btnCheck, 2, 0)
        tlyBar.Controls.Add(btnSync, 3, 0)
        tlyBar.Controls.Add(btnReload, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(0, 0)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 6
        '
        ' btnView
        '
        btnView.Dock = DockStyle.Fill
        btnView.Location = New Point(0, 0)
        btnView.Margin = New Padding(0, 2, 6, 6)
        btnView.Name = "btnView"
        btnView.Size = New Size(154, 36)
        btnView.TabIndex = 7
        btnView.Text = "Vezi (nu execută)"
        tips.SetToolTipHeader(btnView, "Vezi")
        tips.SetToolTipText(btnView, "Arată dacă interogarea e de rulat pe AVACONT_SURSA, deja rulată sau în conflict de nume. Nu scrie nimic, nici măcar registrul.")
        '
        ' btnRun
        '
        btnRun.Dock = DockStyle.Fill
        btnRun.Location = New Point(0, 0)
        btnRun.Margin = New Padding(0, 2, 6, 6)
        btnRun.Name = "btnRun"
        btnRun.Size = New Size(164, 36)
        btnRun.TabIndex = 8
        btnRun.Text = "Rulează pe SURSA"
        tips.SetToolTipHeader(btnRun, "Rulează pe SURSA")
        tips.SetToolTipText(btnRun, "Rulează textul pe AVACONT_SURSA, doar acolo. Unitățile nu sunt atinse. Textul rămâne în registrul Interogari_Unice al serverului, iar apoi se verifică automat în structura SURSEI.")
        '
        ' btnCheck
        '
        btnCheck.Dock = DockStyle.Fill
        btnCheck.Location = New Point(0, 0)
        btnCheck.Margin = New Padding(0, 2, 6, 6)
        btnCheck.Name = "btnCheck"
        btnCheck.Size = New Size(164, 36)
        btnCheck.TabIndex = 9
        btnCheck.Text = "Verifică în SURSA"
        tips.SetToolTipHeader(btnCheck, "Verifică în SURSA")
        tips.SetToolTipText(btnCheck, "Compară ce spune interogarea aleasă din istoric cu structura reală a AVACONT_SURSA (information_schema): fiecare tabel, coloană, index sau cheie există sau lipsește. Nu scrie nimic.")
        '
        ' btnSync
        '
        btnSync.Dock = DockStyle.Fill
        btnSync.Location = New Point(0, 0)
        btnSync.Margin = New Padding(0, 2, 6, 6)
        btnSync.Name = "btnSync"
        btnSync.Primary = True
        btnSync.Size = New Size(194, 36)
        btnSync.TabIndex = 10
        btnSync.Text = "Sincronizează unitățile"
        tips.SetToolTipHeader(btnSync, "Sincronizează unitățile")
        tips.SetToolTipText(btnSync, "Pentru interogarea aleasă din istoric: arată planul schema_sync pe toate bazele de unitate, cere confirmarea (și încă una dacă planul e distructiv), execută, apoi marchează interogarea ca rulată în registrul fiecărei unități, fără s-o execute.")
        '
        ' btnReload
        '
        btnReload.Dock = DockStyle.Fill
        btnReload.Location = New Point(0, 0)
        btnReload.Margin = New Padding(0, 2, 6, 6)
        btnReload.Name = "btnReload"
        btnReload.Size = New Size(164, 36)
        btnReload.TabIndex = 11
        btnReload.Text = "Reîncarcă istoricul"
        tips.SetToolTipHeader(btnReload, "Istoric")
        tips.SetToolTipText(btnReload, "Citește din nou de pe server interogările rulate pe AVACONT_SURSA, cu unitățile care le au deja.")
        '
        ' tlyMid
        '
        tlyMid.AutoFitToTheme = False
        tlyMid.ColumnCount = 2
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58F))
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42F))
        tlyMid.Controls.Add(gridObj, 0, 0)
        tlyMid.Controls.Add(gridHist, 1, 0)
        tlyMid.Dock = DockStyle.Fill
        tlyMid.Location = New Point(0, 0)
        tlyMid.Margin = New Padding(0)
        tlyMid.Name = "tlyMid"
        tlyMid.RowCount = 1
        tlyMid.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMid.Size = New Size(884, 200)
        tlyMid.TabIndex = 12
        '
        ' gridObj
        '
        gridObj.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridObj.ColumnFillMode = KBotFillMode.SpecificColumn
        gridObjCol1.AggregateFormatString = Nothing
        gridObjCol1.FormatString = Nothing
        gridObjCol1.HeaderText = "Instr."
        gridObjCol1.HeaderTooltip = "Numărul instrucțiunii din fișier."
        gridObjCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridObjCol1.Key = "stmt"
        gridObjCol1.MinWidth = 40
        gridObjCol1.OptionGroup = Nothing
        gridObjCol1.ReadOnly = True
        gridObjCol1.TextAlign = ContentAlignment.MiddleRight
        gridObjCol1.Width = 50
        gridObjCol2.AggregateFormatString = Nothing
        gridObjCol2.FormatString = Nothing
        gridObjCol2.HeaderText = "Acțiune"
        gridObjCol2.HeaderTooltip = "Ce face instrucțiunea cu obiectul; la verificare: ce trebuie să fie adevărat la sfârșit."
        gridObjCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridObjCol2.Key = "action"
        gridObjCol2.MinWidth = 60
        gridObjCol2.OptionGroup = Nothing
        gridObjCol2.ReadOnly = True
        gridObjCol2.Width = 130
        gridObjCol3.AggregateFormatString = Nothing
        gridObjCol3.FormatString = Nothing
        gridObjCol3.HeaderText = "Tip"
        gridObjCol3.HeaderTooltip = "Tabel, coloană, index, cheie primară, cheie străină sau date (INSERT, UPDATE...)."
        gridObjCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridObjCol3.Key = "kind"
        gridObjCol3.MinWidth = 50
        gridObjCol3.OptionGroup = Nothing
        gridObjCol3.ReadOnly = True
        gridObjCol3.Width = 100
        gridObjCol4.AggregateFormatString = Nothing
        gridObjCol4.FormatString = Nothing
        gridObjCol4.HeaderText = "Tabel"
        gridObjCol4.HeaderTooltip = "Tabelul atins."
        gridObjCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridObjCol4.Key = "table"
        gridObjCol4.MinWidth = 70
        gridObjCol4.OptionGroup = Nothing
        gridObjCol4.ReadOnly = True
        gridObjCol4.Width = 170
        gridObjCol5.AggregateFormatString = Nothing
        gridObjCol5.FormatString = Nothing
        gridObjCol5.HeaderText = "Obiect"
        gridObjCol5.HeaderTooltip = "Coloana, indexul sau cheia atinsă."
        gridObjCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridObjCol5.Key = "name"
        gridObjCol5.MinWidth = 70
        gridObjCol5.OptionGroup = Nothing
        gridObjCol5.ReadOnly = True
        gridObjCol5.Width = 170
        gridObjCol6.AggregateFormatString = Nothing
        gridObjCol6.FormatString = Nothing
        gridObjCol6.HeaderText = "Stare în SURSA"
        gridObjCol6.HeaderTooltip = "După «Verifică»: există, lipsește, mai există (trebuia șters) sau nu se poate verifica (index sau cheie fără nume)."
        gridObjCol6.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridObjCol6.Key = "state"
        gridObjCol6.MinWidth = 90
        gridObjCol6.OptionGroup = Nothing
        gridObjCol6.ReadOnly = True
        gridObjCol6.Width = 150
        gridObj.Columns.Add(gridObjCol1)
        gridObj.Columns.Add(gridObjCol2)
        gridObj.Columns.Add(gridObjCol3)
        gridObj.Columns.Add(gridObjCol4)
        gridObj.Columns.Add(gridObjCol5)
        gridObj.Columns.Add(gridObjCol6)
        gridObj.Dock = DockStyle.Fill
        gridObj.FillColumnKey = "name"
        gridObj.HeaderHeight = 24
        gridObj.Location = New Point(0, 0)
        gridObj.Margin = New Padding(0, 0, 8, 6)
        gridObj.Name = "gridObj"
        gridObj.ReadOnlyGrid = True
        gridObj.Size = New Size(500, 190)
        gridObj.TabIndex = 13
        tips.SetToolTipHeader(gridObj, "Ce atinge interogarea")
        tips.SetToolTipText(gridObj, "După «Citește»: un rând pe fiecare obiect pe care îl creează, îl schimbă sau îl șterge o instrucțiune. După «Verifică»: starea finală a fiecărui obiect în AVACONT_SURSA.")
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
        gridHistCol2.HeaderTooltip = "Când a rulat pe AVACONT_SURSA."
        gridHistCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol2.Key = "ranat"
        gridHistCol2.MinWidth = 90
        gridHistCol2.OptionGroup = Nothing
        gridHistCol2.ReadOnly = True
        gridHistCol2.Width = 130
        gridHistCol3.AggregateFormatString = Nothing
        gridHistCol3.FormatString = Nothing
        gridHistCol3.HeaderText = "Obiecte"
        gridHistCol3.HeaderTooltip = "Câte obiecte de structură atinge (tabele, coloane, indexuri, chei) și câte instrucțiuni de date are, ca structură/date."
        gridHistCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol3.Key = "objects"
        gridHistCol3.MinWidth = 60
        gridHistCol3.OptionGroup = Nothing
        gridHistCol3.ReadOnly = True
        gridHistCol3.TextAlign = ContentAlignment.MiddleRight
        gridHistCol3.Width = 80
        gridHistCol4.AggregateFormatString = Nothing
        gridHistCol4.FormatString = Nothing
        gridHistCol4.HeaderText = "Unități"
        gridHistCol4.HeaderTooltip = "Câte baze de unitate au interogarea marcată ca rulată, din câte există."
        gridHistCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol4.Key = "done"
        gridHistCol4.MinWidth = 50
        gridHistCol4.OptionGroup = Nothing
        gridHistCol4.ReadOnly = True
        gridHistCol4.TextAlign = ContentAlignment.MiddleRight
        gridHistCol4.Width = 70
        gridHistCol5.AggregateFormatString = Nothing
        gridHistCol5.FormatString = Nothing
        gridHistCol5.HeaderText = "Încă de sincronizat pe"
        gridHistCol5.HeaderTooltip = "Bazele de unitate care nu au interogarea marcată ca rulată."
        gridHistCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridHistCol5.Key = "missing"
        gridHistCol5.MinWidth = 80
        gridHistCol5.OptionGroup = Nothing
        gridHistCol5.ReadOnly = True
        gridHistCol5.Width = 220
        gridHist.Columns.Add(gridHistCol1)
        gridHist.Columns.Add(gridHistCol2)
        gridHist.Columns.Add(gridHistCol3)
        gridHist.Columns.Add(gridHistCol4)
        gridHist.Columns.Add(gridHistCol5)
        gridHist.Dock = DockStyle.Fill
        gridHist.FillColumnKey = "name"
        gridHist.HeaderHeight = 24
        gridHist.Location = New Point(0, 0)
        gridHist.Margin = New Padding(0, 0, 0, 6)
        gridHist.Name = "gridHist"
        gridHist.ReadOnlyGrid = True
        gridHist.Size = New Size(370, 190)
        gridHist.TabIndex = 14
        tips.SetToolTipHeader(gridHist, "Interogările rulate pe SURSA")
        tips.SetToolTipText(gridHist, "Din registrul Interogari_Unice al AVACONT_SURSA. Rândul ales este cel pe care lucrează «Verifică» și «Sincronizează». Dublu clic arată textul.")
        '
        ' logBox
        '
        logBox.Dock = DockStyle.Fill
        logBox.Location = New Point(0, 0)
        logBox.Margin = New Padding(0, 0, 0, 6)
        logBox.Name = "logBox"
        logBox.Size = New Size(884, 150)
        logBox.TabIndex = 15
        logBox.Text = ""
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Rezultatul ultimei acțiuni, în cuvinte.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 24)
        lblState.TabIndex = 16
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' SourceQueryView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "SourceQueryView"
        Size = New Size(900, 560)
        CType(gridObj, ComponentModel.ISupportInitialize).EndInit()
        CType(gridHist, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyPath.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyMid.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyPath As KBotTableLayoutPanel
    Friend WithEvents txtPath As KBotTextField
    Friend WithEvents btnBrowse As KBotButton
    Friend WithEvents txtName As KBotTextField
    Friend WithEvents btnRead As KBotButton
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnView As KBotButton
    Friend WithEvents btnRun As KBotButton
    Friend WithEvents btnCheck As KBotButton
    Friend WithEvents btnSync As KBotButton
    Friend WithEvents btnReload As KBotButton
    Friend WithEvents tlyMid As KBotTableLayoutPanel
    Friend WithEvents gridObj As KBotDataView
    Friend WithEvents gridHist As KBotDataView
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents lblState As Label
End Class
