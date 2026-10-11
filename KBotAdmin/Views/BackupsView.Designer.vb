' The «Copii de siguranta» page (slice ME01): the dump files on the server, and a button for a new copy of
' the chosen database. All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BackupsView
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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnRefresh = New KBotButton()
        btnMake = New KBotButton()
        grid = New KBotDataView()
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
        tlyMain.Controls.Add(grid, 0, 1)
        tlyMain.Controls.Add(lblState, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 3
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 260F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnRefresh, 0, 0)
        tlyBar.Controls.Add(btnMake, 1, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' btnRefresh
        '
        btnRefresh.Dock = DockStyle.Fill
        btnRefresh.Location = New Point(0, 0)
        btnRefresh.Margin = New Padding(0, 2, 6, 6)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(144, 36)
        btnRefresh.TabIndex = 0
        btnRefresh.Text = "Reîmprospătează"
        tips.SetToolTipHeader(btnRefresh, "Reîmprospătează")
        tips.SetToolTipText(btnRefresh, "Recitește lista fișierelor de copie și de plan de schemă de pe server.")
        '
        ' btnMake
        '
        btnMake.Dock = DockStyle.Fill
        btnMake.Location = New Point(150, 0)
        btnMake.Margin = New Padding(0, 2, 6, 6)
        btnMake.Name = "btnMake"
        btnMake.Primary = True
        btnMake.Size = New Size(254, 36)
        btnMake.TabIndex = 1
        btnMake.Text = "Copie nouă pentru baza aleasă"
        tips.SetToolTipHeader(btnMake, "Copie nouă")
        tips.SetToolTipText(btnMake, "Face acum o copie (dump) a bazei alese, în dosarul backup de pe server." & vbLf & "Se cere o singură bază, nu <TOATE BAZELE>.")
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Fișier"
        KBotDataColumn1.HeaderTooltip = "Numele fișierului de copie."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "name"
        KBotDataColumn1.MinWidth = 120
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 320
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Fel"
        KBotDataColumn2.HeaderTooltip = "Felul copiei (bază de unitate sau altceva)."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "kind"
        KBotDataColumn2.MinWidth = 60
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 110
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Mărime"
        KBotDataColumn3.HeaderTooltip = "Mărimea fișierului pe disc."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "size"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.Width = 100
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Data"
        KBotDataColumn4.HeaderTooltip = "Când a fost făcută copia."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "modified"
        KBotDataColumn4.MinWidth = 90
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Calea pe server"
        KBotDataColumn5.HeaderTooltip = "Unde se află fișierul pe server."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "path"
        KBotDataColumn5.MinWidth = 120
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 300
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "path"
        grid.HeaderHeight = 24
        grid.Location = New Point(8, 52)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.Size = New Size(884, 472)
        grid.TabIndex = 1
        tips.SetToolTipHeader(grid, "Fișierele")
        tips.SetToolTipText(grid, "Copiile bazelor și planurile de schemă scrise de sincronizare, cele mai noi sus." & vbLf & "Fișierele nu se șterg și nu se descarcă de aici.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Câte copii există și cât ocupă. Rezultatul ultimei copii noi.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 2
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' BackupsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "BackupsView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnRefresh As KBotButton
    Friend WithEvents btnMake As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents lblState As Label
End Class
