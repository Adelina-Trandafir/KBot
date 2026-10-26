Imports KBot.Controls

' «Jurnale» (slice AV01-09): the modal window with the journals of the unit. Small on purpose: a code, a name, a grid.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 96 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class JurnaleForm
    Inherits Global.KBot.Theming.KBotModalForm

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
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        ntfMesaj = New KBotNotice()
        gridJurnale = New KBotDataView()
        tlySubsol = New KBotTableLayoutPanel()
        btnRenunta = New Button()
        btnSalveaza = New Button()
        CType(gridJurnale, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(558, 438)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.folder_open
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = False
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(558, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Jurnale"
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(8, 6, 8, 6)
        pnlCard.Size = New Size(558, 342)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.AutoFitToTheme = False
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(ntfMesaj, 0, 0)
        tlyCorp.Controls.Add(gridJurnale, 0, 1)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(8, 6)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.RowCount = 2
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.Size = New Size(542, 330)
        tlyCorp.TabIndex = 0
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = Color.Transparent
        ntfMesaj.Dock = DockStyle.Fill
        ntfMesaj.Location = New Point(0, 3)
        ntfMesaj.Margin = New Padding(0, 3, 0, 6)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New Size(542, 40)
        ntfMesaj.TabIndex = 0
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' gridJurnale
        '
        gridJurnale.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridJurnale.BackColor = SystemColors.Window
        gridJurnale.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Cod"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "cod"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 90
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Denumire"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "denumire"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 300
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.ColumnType = KBotColumnType.Button
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = ""
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn3.Key = "sterge"
        KBotDataColumn3.MinWidth = 34
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Resizable = False
        KBotDataColumn3.Width = 34
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Id"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn4.Key = "id"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Visible = KBotColumnVisibility.Hidden
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Combo
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Sursa"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "ss"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 80
        gridJurnale.Columns.Add(KBotDataColumn1)
        gridJurnale.Columns.Add(KBotDataColumn5)
        gridJurnale.Columns.Add(KBotDataColumn2)
        gridJurnale.Columns.Add(KBotDataColumn3)
        gridJurnale.Columns.Add(KBotDataColumn4)
        gridJurnale.Dock = DockStyle.Fill
        gridJurnale.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridJurnale.FillColumnKey = "denumire"
        gridJurnale.FooterBackColor = SystemColors.Control
        gridJurnale.FooterHeight = 24
        gridJurnale.FooterRightIcon = My.Resources.Resources.plus_green
        gridJurnale.FooterRightIconTooltip = "Adaugă un jurnal" & vbLf & "Rândul nou se completează direct în tabel."
        gridJurnale.FooterSeparatorColor = SystemColors.ActiveBorder
        gridJurnale.FooterVisible = True
        gridJurnale.HeaderHeight = 24
        gridJurnale.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridJurnale.Location = New Point(0, 49)
        gridJurnale.Margin = New Padding(0)
        gridJurnale.Name = "gridJurnale"
        gridJurnale.RowHeight = 24
        gridJurnale.Size = New Size(542, 281)
        gridJurnale.TabIndex = 1
        '
        ' tlySubsol
        '
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnRenunta, 0, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 386)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(558, 52)
        tlySubsol.TabIndex = 2
        '
        ' btnRenunta
        '
        btnRenunta.AutoSize = True
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(8, 6)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(18, 3, 18, 3)
        btnRenunta.Size = New Size(100, 40)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(440, 6)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(18, 3, 18, 3)
        btnSalveaza.Size = New Size(110, 40)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvare"
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' JurnaleForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(560, 440)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(460, 360)
        Name = "JurnaleForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Jurnale"
        CType(gridJurnale, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents ntfMesaj As KBotNotice
    Friend WithEvents gridJurnale As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnSalveaza As Button
End Class
