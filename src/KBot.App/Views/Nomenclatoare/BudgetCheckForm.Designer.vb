Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BudgetCheckForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        tips = New KBotToolTip(components)
        gridVerificare = New KBotDataView()
        pnlCard = New Panel()
        pnlJos = New Panel()
        lblStare = New Label()
        btnInchide = New Button()
        capBar = New KBotCaptionBar()
        CType(gridVerificare, ComponentModel.ISupportInitialize).BeginInit()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' gridVerificare
        '
        gridVerificare.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridVerificare.BackColor = SystemColors.Window
        gridVerificare.ColumnFillMode = KBotFillMode.SpecificColumn
        gridVerificare.FillColumnKey = "denumire"
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 190
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Denumire"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "denumire"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 300
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "SS"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "ss"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 60
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Buget K-BOT"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "kbot"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Credit FOREXE"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "fx"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 150
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Diferență"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "diferenta"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 150
        gridVerificare.Columns.Add(KBotDataColumn1)
        gridVerificare.Columns.Add(KBotDataColumn2)
        gridVerificare.Columns.Add(KBotDataColumn3)
        gridVerificare.Columns.Add(KBotDataColumn4)
        gridVerificare.Columns.Add(KBotDataColumn5)
        gridVerificare.Columns.Add(KBotDataColumn6)
        gridVerificare.Dock = DockStyle.Fill
        gridVerificare.FooterBackColor = SystemColors.Control
        gridVerificare.FooterCaption = "Clasificații"
        gridVerificare.FooterSeparatorColor = SystemColors.ActiveBorder
        gridVerificare.FooterVisible = True
        gridVerificare.HeaderBackColor = SystemColors.Control
        gridVerificare.HeaderHeight = 24
        gridVerificare.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridVerificare.Location = New Point(0, 66)
        gridVerificare.Margin = New Padding(0)
        gridVerificare.Name = "gridVerificare"
        gridVerificare.ReadOnlyGrid = True
        gridVerificare.RowHeight = 24
        gridVerificare.Size = New Size(1196, 600)
        gridVerificare.TabIndex = 1
        tips.SetToolTipHeader(gridVerificare, "Bugetul din FOREXE față de cel din K-BOT")
        tips.SetToolTipText(gridVerificare, "«Buget K-BOT» = totalul versiunii în vigoare azi + totalul rectificărilor ei." & vbLf & "«Credit FOREXE» = creditul bugetar al clasificației, de la ultima descărcare (același pentru toate angajamentele ei)." & vbLf & "Diferența = FOREXE − K-BOT." & vbLf & "Din fereastra «Clasificații bugetare»: dublu clic pe un rând închide fereastra și alege clasificația în arbore.")
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(gridVerificare)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1196, 796)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 712)
        pnlJos.Margin = New Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(21, 12, 21, 12)
        pnlJos.Size = New Size(1196, 84)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(21, 12)
        lblStare.Margin = New Padding(0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(745, 60)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnInchide
        '
        btnInchide.DialogResult = DialogResult.Cancel
        btnInchide.Dock = DockStyle.Right
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(995, 12)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(180, 60)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.cells
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1196, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Verificare buget FOREXE"
        '
        ' BudgetCheckForm
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New Size(1200, 800)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "BudgetCheckForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "Verificare buget FOREXE"
        CType(gridVerificare, ComponentModel.ISupportInitialize).EndInit()
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents gridVerificare As KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnInchide As Button
End Class
