' Lista de documente de tipărit a unei rădăcini de lună / «Toate» din ORD și DDF (felia 0099).
'
' Toate controalele sunt declarate AICI (docs/kbot-forms-ui-convention.md). Copiii lui `pnlJos` sunt
' puși în ordine inversă de dock: ultimul adăugat stă cel mai aproape de margine.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrintListPage
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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        btnImprima = New Button()
        btnSalveaza = New Button()
        grila = New Controls.KBotDataView()
        pnlJos = New Panel()
        pnlGap = New Panel()
        lblStatus = New Label()
        CType(grila, ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' btnImprima
        '
        btnImprima.Dock = DockStyle.Right
        btnImprima.FlatStyle = FlatStyle.Flat
        btnImprima.Location = New Point(392, 10)
        btnImprima.Margin = New Padding(0)
        btnImprima.Name = "btnImprima"
        btnImprima.Size = New Size(230, 46)
        btnImprima.TabIndex = 1
        btnImprima.Text = "Generează și imprimă"
        tips.SetToolTipHeader(btnImprima, "Generează și imprimă")
        tips.SetToolTipText(btnImprima, "Pregătește documentele <b>bifate</b> și le trimite la imprimantă, fără să le deschidă în program." & vbLf & "Fiecare document trimis se numără ca tipărit.")
        btnImprima.UseVisualStyleBackColor = True
        '
        ' btnSalveaza
        '
        btnSalveaza.Dock = DockStyle.Right
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(632, 10)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(170, 46)
        btnSalveaza.TabIndex = 3
        btnSalveaza.Text = "Salvează local"
        tips.SetToolTipHeader(btnSalveaza, "Salvează local")
        tips.SetToolTipText(btnSalveaza, "Salvează documentele <b>bifate</b> într-un dosar ales de tine." & vbLf & "Nu le numără ca tipărite.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' grila
        '
        grila.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grila.BackColor = SystemColors.Window
        grila.CellTooltip.Enabled = False
        grila.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderRightIcon = My.Resources.Resources.Fatcow_Farm_Fresh_Check_boxes_32
        KBotDataColumn1.HeaderRightIconTooltip = "Alege ce documente se bifează."
        KBotDataColumn1.HeaderText = ""
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "sel"
        KBotDataColumn1.MinWidth = 10
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 40
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Document"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "doc"
        KBotDataColumn2.MinWidth = 120
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 200
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Semnături"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "sem"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 180
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Semnat la"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "data"
        KBotDataColumn4.MinWidth = 120
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 150
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Listat"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "listat"
        KBotDataColumn5.MinWidth = 60
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Nr. tipăriri"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "nr"
        KBotDataColumn6.MinWidth = 80
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.Width = 100
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        grila.Columns.Add(KBotDataColumn3)
        grila.Columns.Add(KBotDataColumn4)
        grila.Columns.Add(KBotDataColumn5)
        grila.Columns.Add(KBotDataColumn6)
        grila.Dock = DockStyle.Fill
        grila.FillColumnKey = "sem"
        grila.Location = New Point(0, 0)
        grila.Margin = New Padding(4)
        grila.Name = "grila"
        grila.Size = New Size(812, 422)
        grila.TabIndex = 0
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblStatus)
        pnlJos.Controls.Add(btnImprima)
        pnlJos.Controls.Add(pnlGap)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 422)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(10, 10, 10, 10)
        pnlJos.Size = New Size(812, 66)
        pnlJos.TabIndex = 1
        '
        ' pnlGap
        '
        pnlGap.Dock = DockStyle.Right
        pnlGap.Location = New Point(622, 10)
        pnlGap.Margin = New Padding(0)
        pnlGap.Name = "pnlGap"
        pnlGap.Size = New Size(10, 46)
        pnlGap.TabIndex = 2
        '
        ' lblStatus
        '
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(10, 10)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(382, 46)
        lblStatus.TabIndex = 0
        lblStatus.Text = "Nimic bifat."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        ' PrintListPage
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(grila)
        Controls.Add(pnlJos)
        Margin = New Padding(4, 5, 4, 5)
        Name = "PrintListPage"
        Size = New Size(812, 488)
        CType(grila, ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents grila As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents pnlGap As Panel
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnImprima As Button
    Friend WithEvents btnSalveaza As Button
End Class
