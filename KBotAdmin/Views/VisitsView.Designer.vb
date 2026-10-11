' The «Vizite» page (slice ME01): who looked at the public site (table Vizite_Site). All controls are
' declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VisitsView
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
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        lblDays = New Label()
        cmbDays = New KBotComboBox()
        chkBots = New CheckBox()
        btnRefresh = New KBotButton()
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
        tlyBar.ColumnCount = 5
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(lblDays, 0, 0)
        tlyBar.Controls.Add(cmbDays, 1, 0)
        tlyBar.Controls.Add(chkBots, 2, 0)
        tlyBar.Controls.Add(btnRefresh, 3, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' lblDays
        '
        tips.SetToolTipHeader(lblDays, "Zile")
        tips.SetToolTipText(lblDays, "Câte zile înapoi se citesc vizitele.")
        lblDays.Dock = DockStyle.Fill
        lblDays.Location = New Point(0, 0)
        lblDays.Margin = New Padding(0)
        lblDays.Name = "lblDays"
        lblDays.Size = New Size(60, 44)
        lblDays.TabIndex = 0
        lblDays.Text = "Zile:"
        lblDays.TextAlign = ContentAlignment.MiddleLeft
        '
        ' cmbDays
        '
        cmbDays.Dock = DockStyle.Fill
        cmbDays.Location = New Point(60, 0)
        cmbDays.Margin = New Padding(0, 4, 8, 8)
        cmbDays.Name = "cmbDays"
        cmbDays.Size = New Size(102, 32)
        cmbDays.TabIndex = 1
        tips.SetToolTipHeader(cmbDays, "Zile")
        tips.SetToolTipText(cmbDays, "Câte zile înapoi se arată vizitele.")
        '
        ' chkBots
        '
        chkBots.Dock = DockStyle.Fill
        chkBots.Location = New Point(170, 0)
        chkBots.Margin = New Padding(0)
        chkBots.Name = "chkBots"
        chkBots.Size = New Size(150, 44)
        chkBots.TabIndex = 2
        chkBots.Text = "Include roboții"
        tips.SetToolTipHeader(chkBots, "Roboți")
        tips.SetToolTipText(chkBots, "Bifat: se arată și vizitele recunoscute ca roboți (motoare de căutare, scanere).")
        chkBots.UseVisualStyleBackColor = True
        '
        ' btnRefresh
        '
        tips.SetToolTipHeader(btnRefresh, "Reîmprospătează")
        tips.SetToolTipText(btnRefresh, "Citește din nou vizitele.")
        btnRefresh.Dock = DockStyle.Fill
        btnRefresh.Location = New Point(320, 0)
        btnRefresh.Margin = New Padding(0, 2, 6, 6)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(144, 36)
        btnRefresh.TabIndex = 3
        btnRefresh.Text = "Reîmprospătează"
        '
        ' grid
        '
        tips.SetToolTipHeader(grid, "Vizite")
        tips.SetToolTipText(grid, "Un rând = o vizită pe pagina de prezentare. Coloana «Secțiuni» arată ce a citit vizitatorul.")
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Prima"
        KBotDataColumn1.HeaderTooltip = "Momentul primei deschideri a paginii."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "first"
        KBotDataColumn1.MinWidth = 90
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 140
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Durată (s)"
        KBotDataColumn2.HeaderTooltip = "Cât timp a stat vizitatorul pe pagină."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "seconds"
        KBotDataColumn2.MinWidth = 50
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.Width = 80
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "IP"
        KBotDataColumn3.HeaderTooltip = "Adresa vizitatorului."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "ip"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 120
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Țară"
        KBotDataColumn4.HeaderTooltip = "Țara, după adresa IP."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "country"
        KBotDataColumn4.MinWidth = 40
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 50
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Fel"
        KBotDataColumn5.HeaderTooltip = "Om sau robot (indexare)."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "kind"
        KBotDataColumn5.MinWidth = 50
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Pagina"
        KBotDataColumn6.HeaderTooltip = "Pagina deschisă."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "page"
        KBotDataColumn6.MinWidth = 60
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 110
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Provine din"
        KBotDataColumn7.HeaderTooltip = "De unde a venit vizitatorul."
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "referrer"
        KBotDataColumn7.MinWidth = 60
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 160
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Secțiuni văzute"
        KBotDataColumn8.HeaderTooltip = "Secțiunile citite și timpul pe fiecare."
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "sections"
        KBotDataColumn8.MinWidth = 80
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 200
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Columns.Add(KBotDataColumn7)
        grid.Columns.Add(KBotDataColumn8)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "sections"
        grid.HeaderHeight = 24
        grid.Location = New Point(8, 52)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.Size = New Size(884, 472)
        grid.TabIndex = 1
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Câte vizite s-au citit în perioada aleasă.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 2
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' VisitsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "VisitsView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents lblDays As Label
    Friend WithEvents cmbDays As KBotComboBox
    Friend WithEvents chkBots As CheckBox
    Friend WithEvents btnRefresh As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents lblState As Label
End Class
