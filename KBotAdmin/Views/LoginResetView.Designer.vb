' The «Login» page of «Publicare» (slice ME01): the operators with their failed-login counters, and the reset of those counters through the admin API.
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LoginResetView
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
        Dim gridCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol5 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol6 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol7 As KBotDataColumn = New KBotDataColumn()
        Dim gridCol8 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnLoad = New KBotButton()
        btnReset = New KBotButton()
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
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 250F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnLoad, 0, 0)
        tlyBar.Controls.Add(btnReset, 1, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(0, 0)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyBar.Size = New Size(900, 560)
        tlyBar.TabIndex = 1
        '
        ' btnLoad
        '
        btnLoad.Dock = DockStyle.Fill
        btnLoad.Location = New Point(0, 0)
        btnLoad.Margin = New Padding(0, 2, 6, 6)
        btnLoad.Name = "btnLoad"
        btnLoad.Primary = True
        btnLoad.Size = New Size(124, 36)
        btnLoad.TabIndex = 2
        btnLoad.Text = "Citește"
        tips.SetToolTipHeader(btnLoad, "Citește")
        tips.SetToolTipText(btnLoad, "Citește utilizatorii și starea încercărilor de login de la serverul care rulează." & vbLf & "Cheia API se ia o singură dată, prin SSH, din config.py de pe server; nu se salvează.")
        '
        ' btnReset
        '
        btnReset.Dock = DockStyle.Fill
        btnReset.Location = New Point(0, 0)
        btnReset.Margin = New Padding(0, 2, 6, 6)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(244, 36)
        btnReset.TabIndex = 3
        btnReset.Text = "Resetează încercările"
        tips.SetToolTipHeader(btnReset, "Resetează")
        tips.SetToolTipText(btnReset, "Șterge contoarele de încercări eșuate (și blocarea) pentru utilizatorii bifați." & vbLf & "Un utilizator pe mai multe unități apare pe mai multe rânduri, dar se resetează o singură dată.")
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        gridCol1.AggregateFormatString = Nothing
        gridCol1.ColumnType = KBotColumnType.CheckBox
        gridCol1.FormatString = Nothing
        gridCol1.HeaderText = "Alege"
        gridCol1.HeaderTooltip = "Bifați utilizatorii ale căror încercări se resetează."
        gridCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol1.Key = "sel"
        gridCol1.MinWidth = 50
        gridCol1.OptionGroup = Nothing
        gridCol1.Width = 64
        gridCol2.AggregateFormatString = Nothing
        gridCol2.FormatString = Nothing
        gridCol2.HeaderText = "Cont"
        gridCol2.HeaderTooltip = "Adresa de e-mail a utilizatorului."
        gridCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol2.Key = "un"
        gridCol2.MinWidth = 100
        gridCol2.OptionGroup = Nothing
        gridCol2.ReadOnly = True
        gridCol2.Width = 230
        gridCol3.AggregateFormatString = Nothing
        gridCol3.FormatString = Nothing
        gridCol3.HeaderText = "Baza"
        gridCol3.HeaderTooltip = "Baza de unitate la care e legat contul."
        gridCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol3.Key = "dc"
        gridCol3.MinWidth = 60
        gridCol3.OptionGroup = Nothing
        gridCol3.ReadOnly = True
        gridCol3.Width = 100
        gridCol4.AggregateFormatString = Nothing
        gridCol4.FormatString = Nothing
        gridCol4.HeaderText = "Unitatea"
        gridCol4.HeaderTooltip = "Denumirea unității."
        gridCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol4.Key = "unit"
        gridCol4.MinWidth = 80
        gridCol4.OptionGroup = Nothing
        gridCol4.ReadOnly = True
        gridCol4.Width = 220
        gridCol5.AggregateFormatString = Nothing
        gridCol5.FormatString = Nothing
        gridCol5.HeaderText = "Rol K-BOT"
        gridCol5.HeaderTooltip = "Rolul contului în K-BOT."
        gridCol5.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol5.Key = "rol"
        gridCol5.MinWidth = 60
        gridCol5.OptionGroup = Nothing
        gridCol5.ReadOnly = True
        gridCol5.Width = 100
        gridCol6.AggregateFormatString = Nothing
        gridCol6.FormatString = Nothing
        gridCol6.HeaderText = "Ultima perioadă"
        gridCol6.HeaderTooltip = "Ultima perioadă (SS) deschisă de utilizator."
        gridCol6.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol6.Key = "last"
        gridCol6.MinWidth = 60
        gridCol6.OptionGroup = Nothing
        gridCol6.ReadOnly = True
        gridCol6.Width = 110
        gridCol7.AggregateFormatString = Nothing
        gridCol7.FormatString = Nothing
        gridCol7.HeaderText = "Eșecuri"
        gridCol7.HeaderTooltip = "Încercări de login eșuate în contorul serverului."
        gridCol7.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol7.Key = "fails"
        gridCol7.MinWidth = 50
        gridCol7.OptionGroup = Nothing
        gridCol7.ReadOnly = True
        gridCol7.TextAlign = ContentAlignment.MiddleRight
        gridCol7.Width = 80
        gridCol8.AggregateFormatString = Nothing
        gridCol8.FormatString = Nothing
        gridCol8.HeaderText = "Blocat"
        gridCol8.HeaderTooltip = "Da (minute rămase) dacă serverul îl blochează acum."
        gridCol8.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridCol8.Key = "blocked"
        gridCol8.MinWidth = 60
        gridCol8.OptionGroup = Nothing
        gridCol8.ReadOnly = True
        gridCol8.Width = 120
        grid.Columns.Add(gridCol1)
        grid.Columns.Add(gridCol2)
        grid.Columns.Add(gridCol3)
        grid.Columns.Add(gridCol4)
        grid.Columns.Add(gridCol5)
        grid.Columns.Add(gridCol6)
        grid.Columns.Add(gridCol7)
        grid.Columns.Add(gridCol8)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "unit"
        grid.HeaderHeight = 24
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.Size = New Size(600, 300)
        grid.TabIndex = 4
        tips.SetToolTipHeader(grid, "Utilizatorii")
        tips.SetToolTipText(grid, "Un rând = un cont pe o unitate, cu starea încercărilor de login de pe serverul care rulează." & vbLf & "Rândurile blocate apar în roșu.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Câți utilizatori s-au citit și câți sunt blocați acum.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 24)
        lblState.TabIndex = 5
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' LoginResetView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "LoginResetView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnLoad As KBotButton
    Friend WithEvents btnReset As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents lblState As Label
End Class
