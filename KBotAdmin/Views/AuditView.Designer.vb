' The «Jurnal activitate» page (slice ME01): the Jurnal table of AVACONT_COMUN (sign-ins, refusals, the
' administration actions of this program). All controls are declared HERE; coordinates are in the 96 dpi
' the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AuditView
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
        cmbAction = New KBotComboBox()
        cmbHours = New KBotComboBox()
        lblUser = New Label()
        txtUser = New KBotTextField()
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
        tlyBar.ColumnCount = 6
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(cmbAction, 0, 0)
        tlyBar.Controls.Add(cmbHours, 1, 0)
        tlyBar.Controls.Add(lblUser, 2, 0)
        tlyBar.Controls.Add(txtUser, 3, 0)
        tlyBar.Controls.Add(btnRefresh, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(8, 8)
        tlyBar.Margin = New Padding(0)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.Size = New Size(884, 44)
        tlyBar.TabIndex = 0
        '
        ' cmbAction
        '
        cmbAction.Dock = DockStyle.Fill
        cmbAction.Location = New Point(0, 0)
        cmbAction.Margin = New Padding(0, 4, 8, 8)
        cmbAction.Name = "cmbAction"
        cmbAction.Size = New Size(222, 32)
        cmbAction.TabIndex = 0
        tips.SetToolTipHeader(cmbAction, "Ce acțiuni")
        tips.SetToolTipText(cmbAction, "Toate, doar autentificările, doar eșecurile și refuzurile, sau doar ce a făcut KBotAdmin.")
        '
        ' cmbHours
        '
        cmbHours.Dock = DockStyle.Fill
        cmbHours.Location = New Point(230, 0)
        cmbHours.Margin = New Padding(0, 4, 8, 8)
        cmbHours.Name = "cmbHours"
        cmbHours.Size = New Size(122, 32)
        cmbHours.TabIndex = 1
        tips.SetToolTipHeader(cmbHours, "Perioada")
        tips.SetToolTipText(cmbHours, "Cât de departe în urmă se citește jurnalul.")
        '
        ' lblUser
        '
        tips.SetToolTipHeader(lblUser, "Cont")
        tips.SetToolTipText(lblUser, "Filtrează după contul care a făcut acțiunea.")
        lblUser.Dock = DockStyle.Fill
        lblUser.Location = New Point(360, 0)
        lblUser.Margin = New Padding(0)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(60, 44)
        lblUser.TabIndex = 2
        lblUser.Text = "Cont:"
        lblUser.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtUser
        '
        txtUser.BackColor = Color.Transparent
        txtUser.Dock = DockStyle.Fill
        txtUser.Location = New Point(420, 0)
        txtUser.Margin = New Padding(0, 4, 8, 8)
        txtUser.Name = "txtUser"
        txtUser.PlaceholderText = "parte din e-mail"
        txtUser.Size = New Size(192, 32)
        txtUser.TabIndex = 3
        txtUser.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtUser, "Cont")
        tips.SetToolTipText(txtUser, "Doar acțiunile conturilor care conțin acest text. Gol = toate.")
        '
        ' btnRefresh
        '
        tips.SetToolTipHeader(btnRefresh, "Citește")
        tips.SetToolTipText(btnRefresh, "Citește din nou jurnalul cu filtrele alese.")
        btnRefresh.Dock = DockStyle.Fill
        btnRefresh.Location = New Point(620, 0)
        btnRefresh.Margin = New Padding(0, 2, 6, 6)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Primary = True
        btnRefresh.Size = New Size(144, 36)
        btnRefresh.TabIndex = 4
        btnRefresh.Text = "Citește"
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Moment"
        KBotDataColumn1.HeaderTooltip = "Data și ora acțiunii."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "moment"
        KBotDataColumn1.MinWidth = 90
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 140
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Cont"
        KBotDataColumn2.HeaderTooltip = "Contul care a făcut acțiunea (sau a încercat-o)."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "un"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 200
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Baza"
        KBotDataColumn3.HeaderTooltip = "Baza de date vizată."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "dc"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 110
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Acțiune"
        KBotDataColumn4.HeaderTooltip = "Ce s-a făcut: autentificare, drept dat sau retras, cont adăugat etc."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "action"
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Țintă"
        KBotDataColumn5.HeaderTooltip = "Obiectul acțiunii (contul sau dreptul vizat)."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "target"
        KBotDataColumn5.MinWidth = 60
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Rezultat"
        KBotDataColumn6.HeaderTooltip = "OK sau motivul eșecului / refuzului."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "result"
        KBotDataColumn6.MinWidth = 50
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 80
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "IP"
        KBotDataColumn7.HeaderTooltip = "Adresa de la care a venit acțiunea."
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "ip"
        KBotDataColumn7.MinWidth = 60
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 110
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Detalii"
        KBotDataColumn8.HeaderTooltip = "Detalii ale acțiunii, așa cum le-a scris serverul."
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "details"
        KBotDataColumn8.MinWidth = 80
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 260
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Columns.Add(KBotDataColumn7)
        grid.Columns.Add(KBotDataColumn8)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "details"
        grid.HeaderHeight = 24
        grid.Location = New Point(8, 52)
        grid.Margin = New Padding(0)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.Size = New Size(884, 472)
        grid.TabIndex = 1
        tips.SetToolTipHeader(grid, "Jurnalul")
        tips.SetToolTipText(grid, "Cele mai noi sus. Roșu = eșec de autentificare sau acces refuzat.")
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Câte rânduri s-au citit și ce filtre sunt active.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 524)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 2
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' AuditView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "AuditView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents cmbAction As KBotComboBox
    Friend WithEvents cmbHours As KBotComboBox
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As KBotTextField
    Friend WithEvents btnRefresh As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents lblState As Label
End Class
