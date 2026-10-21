' The «Drepturi pe conturi» page (slice ME01): one row per account and database. The rights of ADECHIT and of VERCON
' are given in a dialog opened from the two buttons at the top (RightsForm), not as columns here.
' All controls are declared HERE; coordinates are in the 96 dpi the page was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RightsView
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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnRefresh = New KBotButton()
        btnRemove = New KBotButton()
        grid = New KBotDataView()
        tlyFoot = New KBotTableLayoutPanel()
        lblState = New Label()
        btnSqlSync = New KBotButton()
        btnEnsure = New KBotButton()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        tlyFoot.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(grid, 0, 1)
        tlyMain.Controls.Add(tlyFoot, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyBar
        '
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 3
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnRefresh, 0, 0)
        tlyBar.Controls.Add(btnRemove, 1, 0)
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
        btnRefresh.Size = New Size(134, 36)
        btnRefresh.TabIndex = 0
        btnRefresh.Text = "Reîmprospătează"
        tips.SetToolTipHeader(btnRefresh, "Reîmprospătează")
        tips.SetToolTipText(btnRefresh, "Recitește conturile și drepturile de pe server.")
        '
        ' btnRemove
        '
        btnRemove.Dock = DockStyle.Fill
        btnRemove.Location = New Point(680, 0)
        btnRemove.Margin = New Padding(0, 2, 6, 6)
        btnRemove.Name = "btnRemove"
        btnRemove.Size = New Size(144, 36)
        btnRemove.TabIndex = 5
        btnRemove.Text = "Scoate din bază"
        tips.SetToolTipHeader(btnRemove, "Scoate din bază")
        tips.SetToolTipText(btnRemove, "Desface legătura contului cu baza și îi șterge drepturile ADECHIT / VERCON din ea." & vbLf & "Retrage și accesul SQL la bază; utilizatorul MariaDB nu se șterge.")
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.AutoSizeHeaderHeight = True
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Cont"
        KBotDataColumn1.HeaderTooltip = "Contul (e-mail) al utilizatorului."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "un"
        KBotDataColumn1.MinWidth = 120
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 200
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Baza"
        KBotDataColumn2.HeaderTooltip = "Baza de date a unității."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "dc"
        KBotDataColumn2.MinWidth = 70
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 100
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Rol K-BOT"
        KBotDataColumn3.HeaderTooltip = "Rolul contului în K-BOT."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "rol"
        KBotDataColumn3.MinWidth = 60
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 90
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "un"
        grid.FrozenColumnCount = 1
        grid.HeaderHeight = 40
        grid.Location = New Point(8, 52)
        grid.Margin = New Padding(0)
        grid.MaxHeaderHeight = 60
        grid.Name = "grid"
        grid.Size = New Size(884, 468)
        grid.TabIndex = 1
        tips.SetToolTipHeader(grid, "Drepturile pe conturi")
        tips.SetToolTipText(grid, "Un rând = un cont într-o bază." & vbLf & "Drepturile ADECHIT și VERCON se dau din butoanele de sus, pe contul din rândul ales.")
        '
        ' tlyFoot
        '
        tlyFoot.AutoFitToTheme = False
        tlyFoot.ColumnCount = 3
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyFoot.Controls.Add(lblState, 0, 0)
        tlyFoot.Controls.Add(btnSqlSync, 1, 0)
        tlyFoot.Controls.Add(btnEnsure, 2, 0)
        tlyFoot.Dock = DockStyle.Fill
        tlyFoot.Location = New Point(8, 520)
        tlyFoot.Margin = New Padding(0)
        tlyFoot.Name = "tlyFoot"
        tlyFoot.RowCount = 1
        tlyFoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFoot.Size = New Size(884, 40)
        tlyFoot.TabIndex = 2
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Rezultatul ultimei citiri a conturilor.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(654, 40)
        lblState.TabIndex = 0
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnSqlSync
        '
        btnSqlSync.Dock = DockStyle.Fill
        btnSqlSync.Margin = New Padding(0, 4, 6, 4)
        btnSqlSync.Name = "btnSqlSync"
        btnSqlSync.Size = New Size(214, 32)
        btnSqlSync.TabIndex = 1
        btnSqlSync.Text = "Aplică drepturile MariaDB"
        tips.SetToolTipHeader(btnSqlSync, "Drepturile reale MariaDB")
        tips.SetToolTipText(btnSqlSync, "Recalculează privilegiile SQL ale contului ales pentru toate bazele lui, după rolurile existente." & vbLf & "Retrage privilegiile vechi și închide conexiunile MariaDB ale contului. Contabilul primește acces la toate datele bazelor lui, fără administrare de server sau structură.")
        '
        ' btnEnsure
        '
        btnEnsure.Dock = DockStyle.Fill
        btnEnsure.Location = New Point(654, 0)
        btnEnsure.Margin = New Padding(0, 4, 0, 4)
        btnEnsure.Name = "btnEnsure"
        btnEnsure.Size = New Size(230, 32)
        btnEnsure.TabIndex = 1
        btnEnsure.Text = "Creează rolul «Editare lună închisă»"
        tips.SetToolTipHeader(btnEnsure, "Rol lipsă")
        tips.SetToolTipText(btnEnsure, "Rolul AD_EDIT_INCHISA nu există încă pe server. Îl creează (cu operația edit_closed).")
        btnEnsure.Visible = False
        '
        ' RightsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "RightsView"
        Size = New Size(900, 560)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        tlyFoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents btnRefresh As KBotButton
    Friend WithEvents btnRemove As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents tlyFoot As KBotTableLayoutPanel
    Friend WithEvents lblState As Label
    Friend WithEvents btnSqlSync As KBotButton
    Friend WithEvents btnEnsure As KBotButton
End Class
