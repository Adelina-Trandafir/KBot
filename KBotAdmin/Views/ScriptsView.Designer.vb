' The «Scripturi» page of «Publicare»: on the left the push-*.ps1 / publish-*.ps1 scripts (grouped), on the right the switches of the chosen script.
' All controls are declared HERE; coordinates are in the 96 dpi it was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ScriptsView
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
        Dim gridScriptsCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridScriptsCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridScriptsCol3 As KBotDataColumn = New KBotDataColumn()
        Dim gridScriptsCol4 As KBotDataColumn = New KBotDataColumn()
        Dim gridScriptsGroup1 As KBotGroupLevel = New KBotGroupLevel()
        Dim gridSwitchesCol1 As KBotDataColumn = New KBotDataColumn()
        Dim gridSwitchesCol2 As KBotDataColumn = New KBotDataColumn()
        Dim gridSwitchesCol3 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        tlyMid = New KBotTableLayoutPanel()
        gridScripts = New KBotDataView()
        tlySide = New KBotTableLayoutPanel()
        gridSwitches = New KBotDataView()
        txtNotes = New KBotTextBox()
        lblCommand = New Label()
        logBox = New KBotLogBox()
        lblState = New Label()
        CType(gridScripts, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridSwitches, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyMid.SuspendLayout()
        tlySide.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(tlyMid, 0, 0)
        tlyMain.Controls.Add(lblCommand, 0, 1)
        tlyMain.Controls.Add(logBox, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 130F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 28F))
        tlyMain.Size = New Size(900, 560)
        tlyMain.TabIndex = 0
        '
        ' tlyMid
        '
        tlyMid.AutoFitToTheme = False
        tlyMid.ColumnCount = 3
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8F))
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyMid.Controls.Add(gridScripts, 0, 0)
        tlyMid.Controls.Add(tlySide, 2, 0)
        tlyMid.Dock = DockStyle.Fill
        tlyMid.Location = New Point(0, 0)
        tlyMid.Margin = New Padding(0, 0, 0, 6)
        tlyMid.Name = "tlyMid"
        tlyMid.RowCount = 1
        tlyMid.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMid.Size = New Size(884, 300)
        tlyMid.TabIndex = 1
        '
        ' gridScripts
        '
        gridScripts.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridScripts.ColumnFillMode = KBotFillMode.SpecificColumn
        gridScriptsCol1.AggregateFormatString = Nothing
        gridScriptsCol1.FormatString = Nothing
        gridScriptsCol1.HeaderText = "Grup"
        gridScriptsCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridScriptsCol1.Key = "grup"
        gridScriptsCol1.MinWidth = 60
        gridScriptsCol1.OptionGroup = Nothing
        gridScriptsCol1.ReadOnly = True
        gridScriptsCol1.Visible = KBotColumnVisibility.Hidden
        gridScriptsCol1.Width = 120
        gridScriptsCol2.AggregateFormatString = Nothing
        gridScriptsCol2.FormatString = Nothing
        gridScriptsCol2.HeaderText = "Script"
        gridScriptsCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridScriptsCol2.Key = "script"
        gridScriptsCol2.MinWidth = 120
        gridScriptsCol2.OptionGroup = Nothing
        gridScriptsCol2.ReadOnly = True
        gridScriptsCol2.Width = 210
        gridScriptsCol3.AggregateFormatString = Nothing
        gridScriptsCol3.FormatString = Nothing
        gridScriptsCol3.HeaderText = "Ce face"
        gridScriptsCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridScriptsCol3.Key = "desc"
        gridScriptsCol3.MinWidth = 120
        gridScriptsCol3.OptionGroup = Nothing
        gridScriptsCol3.ReadOnly = True
        gridScriptsCol3.Width = 320
        gridScriptsCol4.AggregateFormatString = Nothing
        gridScriptsCol4.ButtonMargin = New Padding(0)
        gridScriptsCol4.ButtonPadding = New Padding(0)
        gridScriptsCol4.ButtonText = "Rulează"
        gridScriptsCol4.CellPadding = New Padding(0)
        gridScriptsCol4.ColumnType = KBotColumnType.Button
        gridScriptsCol4.FormatString = Nothing
        gridScriptsCol4.HeaderText = ""
        gridScriptsCol4.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridScriptsCol4.Key = "run"
        gridScriptsCol4.MinWidth = 80
        gridScriptsCol4.OptionGroup = Nothing
        gridScriptsCol4.Resizable = False
        gridScriptsCol4.Width = 110
        gridScripts.Columns.Add(gridScriptsCol1)
        gridScripts.Columns.Add(gridScriptsCol2)
        gridScripts.Columns.Add(gridScriptsCol3)
        gridScripts.Columns.Add(gridScriptsCol4)
        gridScripts.Dock = DockStyle.Fill
        gridScripts.EnableGrouping = True
        gridScripts.FillColumnKey = "desc"
        gridScriptsGroup1.ColumnKey = "grup"
        gridScriptsGroup1.FooterCaptionFormat = "{1}"
        gridScriptsGroup1.HeaderCaptionFormat = "{1}"
        gridScriptsGroup1.ShowFooter = False
        gridScriptsGroup1.ShowFooterAggregates = False
        gridScriptsGroup1.SortDirection = KBotSortDirection.Descending
        gridScripts.Groups.Add(gridScriptsGroup1)
        gridScripts.HeaderHeight = 24
        gridScripts.Location = New Point(0, 0)
        gridScripts.Margin = New Padding(0)
        gridScripts.Name = "gridScripts"
        gridScripts.ReadOnlyGrid = True
        gridScripts.Size = New Size(438, 300)
        gridScripts.TabIndex = 0
        '
        ' tlySide
        '
        tlySide.AutoFitToTheme = False
        tlySide.ColumnCount = 1
        tlySide.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySide.Controls.Add(gridSwitches, 0, 0)
        tlySide.Controls.Add(txtNotes, 0, 1)
        tlySide.Dock = DockStyle.Fill
        tlySide.Location = New Point(0, 0)
        tlySide.Margin = New Padding(0)
        tlySide.Name = "tlySide"
        tlySide.RowCount = 2
        tlySide.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySide.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlySide.Size = New Size(438, 300)
        tlySide.TabIndex = 1
        '
        ' gridSwitches
        '
        gridSwitches.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridSwitches.ColumnFillMode = KBotFillMode.SpecificColumn
        gridSwitchesCol1.AggregateFormatString = Nothing
        gridSwitchesCol1.ColumnType = KBotColumnType.CheckBox
        gridSwitchesCol1.FormatString = Nothing
        gridSwitchesCol1.HeaderText = ""
        gridSwitchesCol1.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridSwitchesCol1.Key = "on"
        gridSwitchesCol1.MinWidth = 40
        gridSwitchesCol1.OptionGroup = Nothing
        gridSwitchesCol1.Resizable = False
        gridSwitchesCol1.Width = 48
        gridSwitchesCol2.AggregateFormatString = Nothing
        gridSwitchesCol2.FormatString = Nothing
        gridSwitchesCol2.HeaderText = "Switch"
        gridSwitchesCol2.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridSwitchesCol2.Key = "switch"
        gridSwitchesCol2.MinWidth = 90
        gridSwitchesCol2.OptionGroup = Nothing
        gridSwitchesCol2.ReadOnly = True
        gridSwitchesCol2.Width = 170
        gridSwitchesCol3.AggregateFormatString = Nothing
        gridSwitchesCol3.FormatString = Nothing
        gridSwitchesCol3.HeaderText = "Ce face"
        gridSwitchesCol3.HeaderTextAlign = ContentAlignment.MiddleCenter
        gridSwitchesCol3.Key = "desc"
        gridSwitchesCol3.MinWidth = 120
        gridSwitchesCol3.OptionGroup = Nothing
        gridSwitchesCol3.ReadOnly = True
        gridSwitchesCol3.Width = 320
        gridSwitches.Columns.Add(gridSwitchesCol1)
        gridSwitches.Columns.Add(gridSwitchesCol2)
        gridSwitches.Columns.Add(gridSwitchesCol3)
        gridSwitches.Dock = DockStyle.Fill
        gridSwitches.FillColumnKey = "desc"
        gridSwitches.HeaderHeight = 24
        gridSwitches.Location = New Point(0, 0)
        gridSwitches.Margin = New Padding(0)
        gridSwitches.Name = "gridSwitches"
        gridSwitches.Size = New Size(438, 260)
        gridSwitches.TabIndex = 0
        '
        ' txtNotes
        '
        txtNotes.Dock = DockStyle.Fill
        txtNotes.Location = New Point(0, 0)
        txtNotes.Margin = New Padding(0, 6, 0, 0)
        txtNotes.Multiline = False
        txtNotes.Name = "txtNotes"
        txtNotes.PlaceholderText = "Note pentru fereastra de actualizare (-Notes), fără diacritice"
        txtNotes.Size = New Size(438, 34)
        txtNotes.TabIndex = 1
        tips.SetToolTipHeader(txtNotes, "Note (-Notes)")
        tips.SetToolTipText(txtNotes, "Textul din fereastra de actualizare, în loc de cel din NOUTATI.md. Scrieți-l fără diacritice (fereastra le arată ca «?»). Doar scripturile care au -Notes îl folosesc.")
        '
        ' lblCommand
        '
        tips.SetToolTipHeader(lblCommand, "Apelul")
        tips.SetToolTipText(lblCommand, "Ce face scriptul ales și apelul exact, cu switch-urile bifate, așa cum ar fi scris în PowerShell.")
        lblCommand.AutoEllipsis = True
        lblCommand.Dock = DockStyle.Fill
        lblCommand.Location = New Point(0, 0)
        lblCommand.Margin = New Padding(0)
        lblCommand.Name = "lblCommand"
        lblCommand.Size = New Size(884, 56)
        lblCommand.TabIndex = 2
        lblCommand.TextAlign = ContentAlignment.MiddleLeft
        '
        ' logBox
        '
        logBox.Dock = DockStyle.Fill
        logBox.Location = New Point(0, 0)
        logBox.Margin = New Padding(0, 0, 0, 6)
        logBox.Name = "logBox"
        logBox.Size = New Size(884, 130)
        logBox.TabIndex = 3
        logBox.Text = ""
        '
        ' lblState
        '
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Folderul în care sunt scripturile. Jurnalul de deasupra arată ce s-a pornit și cum s-a terminat (codul scriptului).")
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(0, 0)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(884, 28)
        lblState.TabIndex = 4
        lblState.TextAlign = ContentAlignment.MiddleLeft
        '
        ' ScriptsView
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "ScriptsView"
        Size = New Size(900, 560)
        CType(gridScripts, ComponentModel.ISupportInitialize).EndInit()
        CType(gridSwitches, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyMid.ResumeLayout(False)
        tlySide.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyMid As KBotTableLayoutPanel
    Friend WithEvents gridScripts As KBotDataView
    Friend WithEvents tlySide As KBotTableLayoutPanel
    Friend WithEvents gridSwitches As KBotDataView
    Friend WithEvents txtNotes As KBotTextBox
    Friend WithEvents lblCommand As Label
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents lblState As Label
End Class
