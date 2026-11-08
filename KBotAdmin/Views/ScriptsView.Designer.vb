' The «Scripturi» page of «Publicare»: on the left the push-*.ps1 / publish-*.ps1 scripts (grouped), on the right the switches of the chosen script
' and, under them (a bar to drag between the two), the NOUTATI text of the version that comes out, or the -Notes box.
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotGroupLevel1 As KBotGroupLevel = New KBotGroupLevel()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        txtNotes = New KBotTextBox()
        lblCommand = New Label()
        lblState = New Label()
        tlyMain = New KBotTableLayoutPanel()
        tlyMid = New KBotTableLayoutPanel()
        gridScripts = New AdminDataView()
        splSide = New SplitContainer()
        lblNotesHead = New Label()
        rtbNotes = New KBotNotesBox()
        gridSwitches = New AdminDataView()
        logBox = New KBotLogBox()
        tlyMain.SuspendLayout()
        tlyMid.SuspendLayout()
        CType(gridScripts, ComponentModel.ISupportInitialize).BeginInit()
        CType(splSide, ComponentModel.ISupportInitialize).BeginInit()
        splSide.Panel1.SuspendLayout()
        splSide.Panel2.SuspendLayout()
        splSide.SuspendLayout()
        CType(gridSwitches, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' txtNotes
        '
        txtNotes.Dock = DockStyle.Fill
        txtNotes.Location = New Point(0, 36)
        txtNotes.Margin = New Padding(0)
        txtNotes.Name = "txtNotes"
        txtNotes.PlaceholderText = "Note pentru fereastra de actualizare (-Notes), fără diacritice"
        txtNotes.Size = New Size(661, 120)
        txtNotes.TabIndex = 1
        txtNotes.TextPadding = New Padding(0)
        tips.SetToolTipHeader(txtNotes, "Note (-Notes)")
        tips.SetToolTipText(txtNotes, "Textul din fereastra de actualizare, în loc de cel din NOUTATI.md. Scrieți-l fără diacritice (fereastra le arată ca «?»). Doar scripturile care au -Notes îl folosesc.")
        '
        ' lblNotesHead: which version the notes are for (and whether the file has them)
        '
        lblNotesHead.AutoEllipsis = True
        lblNotesHead.Dock = DockStyle.Top
        lblNotesHead.Location = New Point(0, 0)
        lblNotesHead.Margin = New Padding(0)
        lblNotesHead.Name = "lblNotesHead"
        lblNotesHead.Size = New Size(661, 36)
        lblNotesHead.TabIndex = 2
        lblNotesHead.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblNotesHead, "Versiunea și NOUTATI")
        tips.SetToolTipText(lblNotesHead, "Versiunea care iese din construire (cea de acum, sau cea crescută cu partea bifată) și dacă NOUTATI.md are text scris pentru ea.")
        '
        ' rtbNotes: the NOUTATI.md section of that version
        '
        rtbNotes.Dock = DockStyle.Fill
        rtbNotes.Location = New Point(0, 36)
        rtbNotes.Margin = New Padding(0)
        rtbNotes.Name = "rtbNotes"
        rtbNotes.Size = New Size(661, 120)
        rtbNotes.TabIndex = 3
        tips.SetToolTipHeader(rtbNotes, "NOUTATI")
        tips.SetToolTipText(rtbNotes, "Ce scrie în docs\release-notes\NOUTATI.md pentru versiunea care iese. Doar se citește; textul se scrie în fișier, de asistent.")
        '
        ' lblCommand
        ' 
        lblCommand.AutoEllipsis = True
        lblCommand.Dock = DockStyle.Fill
        lblCommand.Location = New Point(8, 511)
        lblCommand.Margin = New Padding(0)
        lblCommand.Name = "lblCommand"
        lblCommand.Size = New Size(1334, 84)
        lblCommand.TabIndex = 2
        lblCommand.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblCommand, "Apelul")
        tips.SetToolTipText(lblCommand, "Ce face scriptul ales și apelul exact, cu switch-urile bifate, așa cum ar fi scris în PowerShell.")
        '
        ' lblState
        '
        lblState.AutoEllipsis = True
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(8, 790)
        lblState.Margin = New Padding(0)
        lblState.Name = "lblState"
        lblState.Size = New Size(1334, 42)
        lblState.TabIndex = 4
        lblState.TextAlign = ContentAlignment.MiddleLeft
        tips.SetToolTipHeader(lblState, "Starea")
        tips.SetToolTipText(lblState, "Folderul în care sunt scripturile. Jurnalul de deasupra arată ce s-a pornit și cum s-a terminat (codul scriptului).")
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
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 84F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 195F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyMain.Size = New Size(1350, 840)
        tlyMain.TabIndex = 0
        '
        ' tlyMid
        '
        tlyMid.AutoFitToTheme = False
        tlyMid.ColumnCount = 3
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 12F))
        tlyMid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyMid.Controls.Add(gridScripts, 0, 0)
        tlyMid.Controls.Add(splSide, 2, 0)
        tlyMid.Dock = DockStyle.Fill
        tlyMid.Location = New Point(8, 8)
        tlyMid.Margin = New Padding(0, 0, 0, 9)
        tlyMid.Name = "tlyMid"
        tlyMid.RowCount = 1
        tlyMid.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMid.Size = New Size(1334, 494)
        tlyMid.TabIndex = 1
        '
        ' gridScripts
        '
        gridScripts.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridScripts.BackColor = SystemColors.Window
        gridScripts.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Grup"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "grup"
        KBotDataColumn1.MinWidth = 60
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn1.Width = 120
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Script"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "script"
        KBotDataColumn2.MinWidth = 120
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 210
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Ce face"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "desc"
        KBotDataColumn3.MinWidth = 120
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 320
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.ButtonMargin = New Padding(0)
        KBotDataColumn4.ButtonText = "Rulează"
        KBotDataColumn4.CellPadding = New Padding(0)
        KBotDataColumn4.ColumnType = KBotColumnType.Button
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = ""
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "run"
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Resizable = False
        KBotDataColumn4.Width = 110
        gridScripts.Columns.Add(KBotDataColumn1)
        gridScripts.Columns.Add(KBotDataColumn2)
        gridScripts.Columns.Add(KBotDataColumn3)
        gridScripts.Columns.Add(KBotDataColumn4)
        gridScripts.Dock = DockStyle.Fill
        gridScripts.EnableGrouping = True
        gridScripts.FillColumnKey = "desc"
        KBotGroupLevel1.ColumnKey = "grup"
        KBotGroupLevel1.FooterCaptionFormat = "{1}"
        KBotGroupLevel1.HeaderCaptionFormat = "{1}"
        KBotGroupLevel1.KeyPattern = Nothing
        KBotGroupLevel1.ShowFooter = False
        KBotGroupLevel1.ShowFooterAggregates = False
        KBotGroupLevel1.SortDirection = KBotSortDirection.Descending
        gridScripts.Groups.Add(KBotGroupLevel1)
        gridScripts.Location = New Point(0, 0)
        gridScripts.Margin = New Padding(0)
        gridScripts.Name = "gridScripts"
        gridScripts.ReadOnlyGrid = True
        gridScripts.Size = New Size(661, 494)
        gridScripts.TabIndex = 0
        '
        ' splSide: the operator drags the bar between the switch list and the notes
        '
        splSide.Dock = DockStyle.Fill
        splSide.Location = New Point(673, 0)
        splSide.Margin = New Padding(0)
        splSide.Name = "splSide"
        splSide.Orientation = Orientation.Horizontal
        splSide.Panel1.Controls.Add(gridSwitches)
        splSide.Panel1MinSize = 120
        splSide.Panel2.Controls.Add(rtbNotes)
        splSide.Panel2.Controls.Add(txtNotes)
        splSide.Panel2.Controls.Add(lblNotesHead)
        splSide.Panel2MinSize = 90
        splSide.Size = New Size(661, 494)
        splSide.SplitterDistance = 330
        splSide.SplitterWidth = 8
        splSide.TabIndex = 1
        tips.SetToolTipHeader(splSide, "Înălțimea listelor")
        tips.SetToolTipText(splSide, "Trage de bara dintre lista de switch-uri și text ca să mărești sau să micșorezi fiecare parte.")
        '
        ' gridSwitches
        '
        gridSwitches.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridSwitches.BackColor = SystemColors.Window
        gridSwitches.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = ""
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "on"
        KBotDataColumn5.ValueType = KBotValueType.Boolean
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 48
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Switch"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "switch"
        KBotDataColumn6.MinWidth = 90
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 170
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Ce face"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "desc"
        KBotDataColumn7.MinWidth = 120
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 320
        gridSwitches.Columns.Add(KBotDataColumn5)
        gridSwitches.Columns.Add(KBotDataColumn6)
        gridSwitches.Columns.Add(KBotDataColumn7)
        gridSwitches.Dock = DockStyle.Fill
        gridSwitches.FillColumnKey = "desc"
        gridSwitches.Location = New Point(0, 0)
        gridSwitches.Margin = New Padding(0)
        gridSwitches.Name = "gridSwitches"
        gridSwitches.Size = New Size(661, 330)
        gridSwitches.TabIndex = 0
        '
        ' logBox
        '
        logBox.BorderStyle = BorderStyle.None
        logBox.DetectUrls = False
        logBox.Dock = DockStyle.Fill
        logBox.HideSelection = False
        logBox.Location = New Point(8, 595)
        logBox.Margin = New Padding(0, 0, 0, 9)
        logBox.Name = "logBox"
        logBox.ReadOnly = True
        logBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth
        logBox.Size = New Size(1334, 186)
        logBox.TabIndex = 3
        logBox.Text = ""
        logBox.WordWrap = False
        '
        ' ScriptsView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(4, 4, 4, 4)
        Name = "ScriptsView"
        Size = New Size(1350, 840)
        tlyMain.ResumeLayout(False)
        tlyMid.ResumeLayout(False)
        CType(gridScripts, ComponentModel.ISupportInitialize).EndInit()
        splSide.Panel1.ResumeLayout(False)
        splSide.Panel2.ResumeLayout(False)
        CType(splSide, ComponentModel.ISupportInitialize).EndInit()
        splSide.ResumeLayout(False)
        CType(gridSwitches, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyMid As KBotTableLayoutPanel
    Friend WithEvents gridScripts As AdminDataView
    Friend WithEvents splSide As SplitContainer
    Friend WithEvents lblNotesHead As Label
    Friend WithEvents rtbNotes As KBotNotesBox
    Friend WithEvents gridSwitches As AdminDataView
    Friend WithEvents txtNotes As KBotTextBox
    Friend WithEvents lblCommand As Label
    Friend WithEvents logBox As KBotLogBox
    Friend WithEvents lblState As Label
End Class
