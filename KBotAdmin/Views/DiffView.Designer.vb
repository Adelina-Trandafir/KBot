' The «Diferenta schema» page (slice ME01): what differs between AVACONT_SURSA and ONE chosen database.
' Nothing is stored or executed here. All controls are declared HERE; coordinates are in the 96 dpi the
' page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DiffView
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        cmbMode = New KBotComboBox()
        btnCompute = New KBotButton()
        btnCopy = New KBotButton()
        lblSummary = New Label()
        grid = New KBotDataView()
        txtSql = New KBotTextBox()
        lblState = New Label()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(lblSummary, 0, 1)
        tlyMain.Controls.Add(grid, 0, 2)
        tlyMain.Controls.Add(txtSql, 0, 3)
        tlyMain.Controls.Add(lblState, 0, 4)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 5
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 170F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 4
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(cmbMode, 0, 0)
        tlyBar.Controls.Add(btnCompute, 1, 0)
        tlyBar.Controls.Add(btnCopy, 2, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' cmbMode
        '
        cmbMode.Dock = DockStyle.Fill
        cmbMode.Location = New Point(0, 0)
        cmbMode.Margin = New Padding(0, 4, 8, 8)
        cmbMode.Name = "cmbMode"
        cmbMode.Size = New Size(252, 32)
        cmbMode.TabIndex = 0
        tips.SetToolTipHeader(cmbMode, "Ce se compară")
        tips.SetToolTipText(cmbMode, "<b>Complet</b>: tot ce diferă, inclusiv modificări și ștergeri." & vbLf & "<b>Doar adăugări</b>: ce lipsește în bază (tabele, coloane, indexuri).")
        '
        ' btnCompute
        '
        btnCompute.Dock = DockStyle.Fill
        btnCompute.Location = New Point(260, 0)
        btnCompute.Margin = New Padding(0, 2, 6, 6)
        btnCompute.Name = "btnCompute"
        btnCompute.Primary = True
        btnCompute.Size = New Size(184, 36)
        btnCompute.TabIndex = 1
        btnCompute.Text = "Calculează diferența"
        tips.SetToolTipHeader(btnCompute, "Calculează diferența")
        tips.SetToolTipText(btnCompute, "Compară baza aleasă cu AVACONT_SURSA." & vbLf & "Doar citire: nu se scrie nimic în nicio bază și nu se păstrează nimic pe server.")
        '
        ' btnCopy
        '
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(450, 0)
        btnCopy.Margin = New Padding(0, 2, 6, 6)
        btnCopy.Name = "btnCopy"
        btnCopy.Size = New Size(124, 36)
        btnCopy.TabIndex = 2
        btnCopy.Text = "Copiază SQL"
        tips.SetToolTipHeader(btnCopy, "Copiază SQL")
        tips.SetToolTipText(btnCopy, "Pune în clipboard toate instrucțiunile, în ordinea în care s-ar executa.")
        '
        ' lblSummary
        '
        tips.SetToolTipHeader(lblSummary, "Rezumat")
        tips.SetToolTipText(lblSummary, "Câte diferențe sunt și câte dintre ele sunt distructive (șterg sau schimbă date).")
        lblSummary.AutoEllipsis = True
        lblSummary.Dock = DockStyle.Fill
        lblSummary.Location = New Point(8, 52)
        lblSummary.Margin = New Padding(0)
        lblSummary.Name = "lblSummary"
        lblSummary.Size = New Size(884, 28)
        lblSummary.TabIndex = 1
        lblSummary.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Tabelă"
        KBotDataColumn1.HeaderTooltip = "Tabela la care se referă diferența."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "table"
        KBotDataColumn1.MinWidth = 90
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 190
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Obiect"
        KBotDataColumn2.HeaderTooltip = "Coloana, cheia sau obiectul care diferă."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "object"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 190
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Fel"
        KBotDataColumn3.HeaderTooltip = "Felul obiectului: coloană, index, cheie străină, vedere etc."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "kind"
        KBotDataColumn3.MinWidth = 50
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 90
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Acțiune"
        KBotDataColumn4.HeaderTooltip = "Ce ar face sincronizarea: adaugă, modifică sau șterge."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "action"
        KBotDataColumn4.MinWidth = 50
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 90
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Distructiv"
        KBotDataColumn5.HeaderTooltip = "Da = operația poate pierde date; cere confirmare dublă la sincronizare."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "destructive"
        KBotDataColumn5.MinWidth = 50
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Problemă"
        KBotDataColumn6.HeaderTooltip = "Motivul pentru care diferența nu se poate aplica automat, dacă e cazul."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "error"
        KBotDataColumn6.MinWidth = 80
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 240
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "error"
        grid.HeaderHeight = 24
        grid.Location = New Point(8, 80)
        grid.Margin = New Padding(0, 0, 0, 6)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.Size = New Size(884, 290)
        grid.TabIndex = 2
        tips.SetToolTipHeader(grid, "Diferențele")
        tips.SetToolTipText(grid, "În ordinea de execuție. Galben = distructiv, roșu = instrucțiune care nu se poate construi." & vbLf & "SQL-ul rândului ales se vede dedesubt.")
        '
        ' txtSql
        '
        txtSql.CornerRadius = 0
        txtSql.Dock = DockStyle.Fill
        txtSql.Font = New Font("Consolas", 9.75F)
        txtSql.Location = New Point(8, 376)
        txtSql.Margin = New Padding(0)
        txtSql.MaxLength = 327670
        txtSql.Name = "txtSql"
        txtSql.ReadOnly = True
        txtSql.ScrollBars = ScrollBars.Both
        txtSql.Size = New Size(884, 148)
        txtSql.TabIndex = 3
        tips.SetToolTipHeader(txtSql, "SQL")
        tips.SetToolTipText(txtSql, "Instrucțiunea rândului ales din tabel.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Baza comparată și cât a durat calculul.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 4
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' DiffView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "DiffView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents cmbMode As KBotComboBox
    Friend WithEvents btnCompute As KBotButton
    Friend WithEvents btnCopy As KBotButton
    Friend WithEvents lblSummary As Label
    Friend WithEvents grid As KBotDataView
    Friend WithEvents txtSql As KBotTextBox
    Friend WithEvents lblState As Label
End Class
