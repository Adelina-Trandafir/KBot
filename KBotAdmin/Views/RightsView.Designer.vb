' The «Drepturi pe conturi» page (slice ME01): one row per account and database, one check box per ADECHIT
' role. A tick gives the right on the server, a cleared tick takes it away; each change goes through at once.
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
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn9 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn10 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn11 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn12 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn13 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnRefresh = New KBotButton()
        btnAllAd = New KBotButton()
        btnNoAd = New KBotButton()
        btnAdd = New KBotButton()
        btnRole = New KBotButton()
        btnRemove = New KBotButton()
        grid = New KBotDataView()
        tlyFoot = New KBotTableLayoutPanel()
        lblState = New Label()
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
        tlyBar.ColumnCount = 7
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.Controls.Add(btnRefresh, 0, 0)
        tlyBar.Controls.Add(btnAllAd, 1, 0)
        tlyBar.Controls.Add(btnNoAd, 2, 0)
        tlyBar.Controls.Add(btnAdd, 3, 0)
        tlyBar.Controls.Add(btnRole, 4, 0)
        tlyBar.Controls.Add(btnRemove, 5, 0)
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
        ' btnAllAd
        '
        btnAllAd.Dock = DockStyle.Fill
        btnAllAd.Location = New Point(140, 0)
        btnAllAd.Margin = New Padding(0, 2, 6, 6)
        btnAllAd.Name = "btnAllAd"
        btnAllAd.Size = New Size(144, 36)
        btnAllAd.TabIndex = 1
        btnAllAd.Text = "Toate ADECHIT"
        tips.SetToolTipHeader(btnAllAd, "Toate drepturile ADECHIT")
        tips.SetToolTipText(btnAllAd, "Dă contului rândului ales toate drepturile ADECHIT din lista de roluri a serverului.")
        '
        ' btnNoAd
        '
        btnNoAd.Dock = DockStyle.Fill
        btnNoAd.Location = New Point(290, 0)
        btnNoAd.Margin = New Padding(0, 2, 6, 6)
        btnNoAd.Name = "btnNoAd"
        btnNoAd.Size = New Size(124, 36)
        btnNoAd.TabIndex = 2
        btnNoAd.Text = "Fără ADECHIT"
        tips.SetToolTipHeader(btnNoAd, "Retrage drepturile ADECHIT")
        tips.SetToolTipText(btnNoAd, "Ia contului rândului ales toate drepturile ADECHIT. Contul rămâne legat de bază.")
        '
        ' btnAdd
        '
        btnAdd.Dock = DockStyle.Fill
        btnAdd.Location = New Point(420, 0)
        btnAdd.Margin = New Padding(0, 2, 6, 6)
        btnAdd.Name = "btnAdd"
        btnAdd.Size = New Size(124, 36)
        btnAdd.TabIndex = 3
        btnAdd.Text = "Adaugă cont…"
        tips.SetToolTipHeader(btnAdd, "Adaugă cont la bază")
        tips.SetToolTipText(btnAdd, "Leagă un cont de o bază, cu un rol K-BOT. Fără drepturi ADECHIT până nu le bifați.")
        '
        ' btnRole
        '
        btnRole.Dock = DockStyle.Fill
        btnRole.Location = New Point(550, 0)
        btnRole.Margin = New Padding(0, 2, 6, 6)
        btnRole.Name = "btnRole"
        btnRole.Size = New Size(124, 36)
        btnRole.TabIndex = 4
        btnRole.Text = "Rol K-BOT…"
        tips.SetToolTipHeader(btnRole, "Rolul K-BOT")
        tips.SetToolTipText(btnRole, "Schimbă rolul K-BOT al contului rândului ales (contabil, director, administrator, secretar).")
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
        tips.SetToolTipText(btnRemove, "Desface legătura contului cu baza și îi șterge drepturile ADECHIT din ea." & vbLf & "Utilizatorul MariaDB nu se șterge.")
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
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Citire"
        KBotDataColumn4.HeaderTooltip = "Poate deschide ADECHIT și vedea datele."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "AD_CITIRE"
        KBotDataColumn4.MinWidth = 50
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 70
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Prezență"
        KBotDataColumn5.HeaderTooltip = "Poate completa prezența (zilele lucrate)."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "AD_PREZENTA"
        KBotDataColumn5.MinWidth = 50
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Plăți"
        KBotDataColumn6.HeaderTooltip = "Poate lucra cu plățile."
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "AD_PLATI"
        KBotDataColumn6.MinWidth = 50
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Width = 70
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Anulare documente"
        KBotDataColumn7.HeaderTooltip = "Poate anula documente. Se dă doar per cont."
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "AD_ANULARE"
        KBotDataColumn7.MinWidth = 50
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Width = 100
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Închidere lună"
        KBotDataColumn8.HeaderTooltip = "Poate închide o lună."
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "AD_INCHIDERE"
        KBotDataColumn8.MinWidth = 50
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Width = 90
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Redeschidere lună"
        KBotDataColumn9.HeaderTooltip = "Poate redeschide o lună închisă."
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "AD_REDESCHIDERE"
        KBotDataColumn9.MinWidth = 50
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Width = 100
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Cataloage"
        KBotDataColumn10.HeaderTooltip = "Poate modifica cataloagele (nomenclatoarele)."
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "AD_CATALOAGE"
        KBotDataColumn10.MinWidth = 50
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Width = 80
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Transfer"
        KBotDataColumn11.HeaderTooltip = "Poate face transferuri."
        KBotDataColumn11.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "AD_TRANSFER"
        KBotDataColumn11.MinWidth = 50
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn11.Width = 70
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Import"
        KBotDataColumn12.HeaderTooltip = "Poate importa date."
        KBotDataColumn12.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "AD_IMPORT"
        KBotDataColumn12.MinWidth = 50
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn12.Width = 70
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Editare lună închisă"
        KBotDataColumn13.HeaderTooltip = "Poate modifica prezența în ULTIMA lună închisă. Cere și «Prezență»."
        KBotDataColumn13.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "AD_EDIT_INCHISA"
        KBotDataColumn13.MinWidth = 50
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn13.Width = 110
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Columns.Add(KBotDataColumn7)
        grid.Columns.Add(KBotDataColumn8)
        grid.Columns.Add(KBotDataColumn9)
        grid.Columns.Add(KBotDataColumn10)
        grid.Columns.Add(KBotDataColumn11)
        grid.Columns.Add(KBotDataColumn12)
        grid.Columns.Add(KBotDataColumn13)
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
        tips.SetToolTipText(grid, "Un rând = un cont într-o bază. Bifa dă dreptul pe loc, fără bifă dreptul se retrage." & vbLf & "«Anulare documente» = poate anula documente; «Editare lună închisă» = poate modifica prezența în ultima lună închisă.")
        '
        ' tlyFoot
        '
        tlyFoot.AutoFitToTheme = False
        tlyFoot.ColumnCount = 2
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 230F))
        tlyFoot.Controls.Add(lblState, 0, 0)
        tlyFoot.Controls.Add(btnEnsure, 1, 0)
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
        tips.SetToolTipText(lblState, "Rezultatul ultimei modificări de drepturi. O bifă respinsă de server se întoarce singură.")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(654, 40)
        lblState.TabIndex = 0
        lblState.TextAlign = ContentAlignment.MiddleLeft
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
    Friend WithEvents btnAllAd As KBotButton
    Friend WithEvents btnNoAd As KBotButton
    Friend WithEvents btnAdd As KBotButton
    Friend WithEvents btnRole As KBotButton
    Friend WithEvents btnRemove As KBotButton
    Friend WithEvents grid As KBotDataView
    Friend WithEvents tlyFoot As KBotTableLayoutPanel
    Friend WithEvents lblState As Label
    Friend WithEvents btnEnsure As KBotButton
End Class
