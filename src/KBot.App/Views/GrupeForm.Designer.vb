Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class GrupeForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        tlyBody = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        tlyDetalii = New KBotTableLayoutPanel()
        lblDenumire = New Label()
        txtDenumire = New KBotTextField()
        lblCuloare = New Label()
        btnCuloare = New Button()
        lblAngajamente = New Label()
        gridAng = New KBotDataView()
        tlySubsol = New KBotTableLayoutPanel()
        btnIesire = New Button()
        lblStare = New Label()
        btnSalveaza = New Button()
        CType(gridAng, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyDetalii.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52.0F))
        tlyMain.Size = New Size(958, 598)
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
        capBar.Size = New Size(958, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Grupe de angajamente"
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(8, 6, 8, 6)
        pnlCard.Size = New Size(958, 502)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' tlyBody
        '
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32.0F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 68.0F))
        tlyBody.Controls.Add(tree, 0, 0)
        tlyBody.Controls.Add(tlyDetalii, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(8, 6)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyBody.Size = New Size(942, 490)
        tlyBody.TabIndex = 0
        '
        ' tree
        '
        tree.BorderColor = SystemColors.ActiveBorder
        tree.DragEnabled = True
        tree.Dock = DockStyle.Fill
        tree.FooterBackColor = SystemColors.Control
        tree.FooterCaption = "Adăugare grupă"
        tree.FooterHeight = 28
        tree.FooterLeftIcon = My.Resources.Resources.plus_green
        tree.FooterLeftIconTooltip = "Adaugă o grupă nouă." & vbLf & "Toate angajamentele apar nebifate; bifați-le pe cele din grupă."
        tree.FooterSeparatorColor = SystemColors.ActiveBorder
        tree.FooterSeparatorWidth = 2
        tree.FooterVisible = True
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " GRUPE"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0, 0, 10, 0)
        tree.Name = "tree"
        tree.SearchShow = False
        tree.Size = New Size(291, 490)
        tree.TabIndex = 0
        '
        ' tlyDetalii
        '
        tlyDetalii.ColumnCount = 2
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyDetalii.Controls.Add(lblDenumire, 0, 0)
        tlyDetalii.Controls.Add(txtDenumire, 1, 0)
        tlyDetalii.Controls.Add(lblCuloare, 0, 1)
        tlyDetalii.Controls.Add(btnCuloare, 1, 1)
        tlyDetalii.Controls.Add(lblAngajamente, 0, 2)
        tlyDetalii.Controls.Add(gridAng, 0, 3)
        tlyDetalii.Dock = DockStyle.Fill
        tlyDetalii.Location = New Point(301, 0)
        tlyDetalii.Margin = New Padding(0)
        tlyDetalii.Name = "tlyDetalii"
        tlyDetalii.RowCount = 4
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40.0F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyDetalii.Size = New Size(641, 490)
        tlyDetalii.TabIndex = 1
        '
        ' lblDenumire
        '
        lblDenumire.Dock = DockStyle.Fill
        lblDenumire.Location = New Point(0, 0)
        lblDenumire.Margin = New Padding(0)
        lblDenumire.Name = "lblDenumire"
        lblDenumire.Size = New Size(110, 40)
        lblDenumire.TabIndex = 0
        lblDenumire.Text = "Denumire grupă *"
        lblDenumire.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDenumire
        '
        txtDenumire.BackColor = Color.Transparent
        txtDenumire.Dock = DockStyle.Fill
        txtDenumire.Location = New Point(110, 3)
        txtDenumire.Margin = New Padding(0, 3, 0, 3)
        txtDenumire.MaxLength = 100
        txtDenumire.Name = "txtDenumire"
        txtDenumire.Size = New Size(531, 34)
        txtDenumire.TabIndex = 1
        txtDenumire.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipText(txtDenumire, "Numele grupei. Două grupe nu pot avea același nume.")
        '
        ' lblCuloare
        '
        lblCuloare.Dock = DockStyle.Fill
        lblCuloare.Location = New Point(0, 40)
        lblCuloare.Margin = New Padding(0)
        lblCuloare.Name = "lblCuloare"
        lblCuloare.Size = New Size(110, 40)
        lblCuloare.TabIndex = 2
        lblCuloare.Text = "Culoare grupă"
        lblCuloare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnCuloare
        '
        btnCuloare.Dock = DockStyle.Left
        btnCuloare.FlatStyle = FlatStyle.Flat
        btnCuloare.Location = New Point(110, 43)
        btnCuloare.Margin = New Padding(0, 3, 0, 3)
        btnCuloare.Name = "btnCuloare"
        btnCuloare.Size = New Size(150, 34)
        btnCuloare.TabIndex = 3
        btnCuloare.Text = "#000000"
        tips.SetToolTipText(btnCuloare, "Alegeți culoarea grupei. Cu ea apar grupa în meniu și angajamentele ei în arborele principal.")
        btnCuloare.UseVisualStyleBackColor = False
        '
        ' lblAngajamente
        '
        tlyDetalii.SetColumnSpan(lblAngajamente, 2)
        lblAngajamente.Dock = DockStyle.Fill
        lblAngajamente.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        lblAngajamente.Location = New Point(0, 80)
        lblAngajamente.Margin = New Padding(0)
        lblAngajamente.Name = "lblAngajamente"
        lblAngajamente.Size = New Size(641, 28)
        lblAngajamente.TabIndex = 4
        lblAngajamente.Text = "Angajamente"
        lblAngajamente.TextAlign = ContentAlignment.BottomLeft
        '
        ' gridAng
        '
        gridAng.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAng.BackColor = SystemColors.Window
        gridAng.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = ""
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "bifa"
        KBotDataColumn1.MinWidth = 32
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Resizable = False
        KBotDataColumn1.Width = 32
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Cod angajament"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "cod"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 120
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Denumire"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "denumire"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 200
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Indicatori"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "indicatori"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 80
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Alias"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "alias"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 170
        gridAng.Columns.Add(KBotDataColumn1)
        gridAng.Columns.Add(KBotDataColumn2)
        gridAng.Columns.Add(KBotDataColumn3)
        gridAng.Columns.Add(KBotDataColumn4)
        gridAng.Columns.Add(KBotDataColumn5)
        tlyDetalii.SetColumnSpan(gridAng, 2)
        gridAng.Dock = DockStyle.Fill
        gridAng.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridAng.FillColumnKey = "denumire"
        gridAng.HeaderHeight = 24
        gridAng.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridAng.Location = New Point(0, 108)
        gridAng.Margin = New Padding(0)
        gridAng.Name = "gridAng"
        gridAng.RowHeight = 24
        gridAng.Size = New Size(641, 382)
        gridAng.TabIndex = 5
        '
        ' tlySubsol
        '
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnIesire, 0, 0)
        tlySubsol.Controls.Add(lblStare, 1, 0)
        tlySubsol.Controls.Add(btnSalveaza, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 546)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlySubsol.Size = New Size(958, 52)
        tlySubsol.TabIndex = 2
        '
        ' btnIesire
        '
        btnIesire.AutoSize = True
        btnIesire.FlatStyle = FlatStyle.Flat
        btnIesire.Location = New Point(8, 6)
        btnIesire.Margin = New Padding(0)
        btnIesire.Name = "btnIesire"
        btnIesire.Padding = New Padding(18, 3, 18, 3)
        btnIesire.Size = New Size(100, 40)
        btnIesire.TabIndex = 0
        btnIesire.Text = "Ieșire"
        btnIesire.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(120, 6)
        lblStare.Margin = New Padding(12, 0, 12, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(700, 40)
        lblStare.TabIndex = 1
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnSalveaza
        '
        btnSalveaza.AutoSize = True
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(832, 6)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(18, 3, 18, 3)
        btnSalveaza.Size = New Size(118, 40)
        btnSalveaza.TabIndex = 2
        btnSalveaza.Text = "Salvează"
        tips.SetToolTipText(btnSalveaza, "Salvează grupa: are nevoie de denumire, culoare și cel puțin un angajament bifat.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' GrupeForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(960, 600)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(860, 520)
        Name = "GrupeForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Grupe de angajamente"
        CType(gridAng, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyDetalii.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents tlyDetalii As KBotTableLayoutPanel
    Friend WithEvents lblDenumire As Label
    Friend WithEvents txtDenumire As KBotTextField
    Friend WithEvents lblCuloare As Label
    Friend WithEvents btnCuloare As Button
    Friend WithEvents lblAngajamente As Label
    Friend WithEvents gridAng As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnIesire As Button
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
End Class
