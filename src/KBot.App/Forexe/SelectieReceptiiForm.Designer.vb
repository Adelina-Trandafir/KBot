' Macheta de ALEGERE A RECEPȚIILOR de reîmprospătat (felia 0060).
'
' Toate controalele sunt declarate AICI, ca orice formular din K-BOT
' (docs/kbot-forms-ui-convention.md): designerul le desenează, codul din spate nu
' construiește niciunul. Copiii lui `pnlCard` sunt puși în ordine INVERSĂ de dock —
' grila (Fill) prima, apoi barele — fiindcă WinForms așază ultimul-adăugat cel mai
' aproape de margine.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SelectieReceptiiForm
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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SelectieReceptiiForm))
        tips = New KBot.Controls.KBotToolTip(components)
        btnDescarca = New Button()
        btnRenunta = New Button()
        pnlCard = New Panel()
        grilaReceptii = New Controls.KBotDataView()
        pnlJos = New Panel()
        lblTotal = New Label()
        lblAntet = New Label()
        capBar = New Controls.KBotCaptionBar()
        Label1 = New Label()
        pnlCard.SuspendLayout()
        CType(grilaReceptii, ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnDescarca
        ' 
        btnDescarca.Dock = DockStyle.Right
        btnDescarca.FlatStyle = FlatStyle.Flat
        btnDescarca.Location = New Point(735, 10)
        btnDescarca.Margin = New Padding(0)
        btnDescarca.Name = "btnDescarca"
        btnDescarca.Size = New Size(154, 46)
        btnDescarca.TabIndex = 3
        btnDescarca.Text = "Descarcă"
        tips.SetToolTipHeader(btnDescarca, "Descarcă")
        tips.SetToolTipText(btnDescarca, "Pornește descărcarea din FOREXE." & vbLf & "Recepțiile <b>nebifate</b> nu se deschid și nu se rescriu.")
        btnDescarca.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(573, 10)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(147, 46)
        btnRenunta.TabIndex = 4
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să descarce nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(grilaReceptii)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(899, 746)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' grilaReceptii
        ' 
        grilaReceptii.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grilaReceptii.BackColor = SystemColors.Window
        grilaReceptii.CellTooltip.Enabled = False
        grilaReceptii.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderRightIcon = My.Resources.Resources.Fatcow_Farm_Fresh_Check_boxes_32
        KBotDataColumn1.HeaderText = "S"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "sel"
        KBotDataColumn1.MinWidth = 10
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 32
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Nr."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nrcrt"
        KBotDataColumn2.MinWidth = 50
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn2.Width = 60
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Data"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "data"
        KBotDataColumn3.MinWidth = 90
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Width = 110
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Valoare"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "suma"
        KBotDataColumn4.MinWidth = 100
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.Width = 140
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Instantanee"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "antete"
        KBotDataColumn5.MinWidth = 80
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.Width = 90
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Stare"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "stare"
        KBotDataColumn6.MinWidth = 120
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 120
        grilaReceptii.Columns.Add(KBotDataColumn1)
        grilaReceptii.Columns.Add(KBotDataColumn2)
        grilaReceptii.Columns.Add(KBotDataColumn3)
        grilaReceptii.Columns.Add(KBotDataColumn4)
        grilaReceptii.Columns.Add(KBotDataColumn5)
        grilaReceptii.Columns.Add(KBotDataColumn6)
        grilaReceptii.Dock = DockStyle.Fill
        grilaReceptii.Location = New Point(0, 153)
        grilaReceptii.Margin = New Padding(4)
        grilaReceptii.Name = "grilaReceptii"
        grilaReceptii.Size = New Size(899, 517)
        grilaReceptii.TabIndex = 2
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(Label1)
        pnlJos.Controls.Add(lblTotal)
        pnlJos.Controls.Add(btnDescarca)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 670)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(0, 10, 10, 20)
        pnlJos.Size = New Size(899, 76)
        pnlJos.TabIndex = 3
        pnlJos.Tag = "Card"
        ' 
        ' lblTotal
        ' 
        lblTotal.AutoSize = True
        lblTotal.Location = New Point(20, 22)
        lblTotal.Margin = New Padding(4, 0, 4, 0)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New Size(95, 22)
        lblTotal.TabIndex = 0
        lblTotal.Text = "Nimic bifat."
        ' 
        ' lblAntet
        ' 
        lblAntet.Dock = DockStyle.Top
        lblAntet.Location = New Point(0, 60)
        lblAntet.Margin = New Padding(4, 0, 4, 0)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New Padding(18, 15, 18, 15)
        lblAntet.Size = New Size(899, 93)
        lblAntet.TabIndex = 1
        lblAntet.Text = resources.GetString("lblAntet.Text")
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(899, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ce recepții reîmprospătez?"
        ' 
        ' Label1
        ' 
        Label1.Dock = DockStyle.Right
        Label1.Location = New Point(720, 10)
        Label1.Margin = New Padding(4, 0, 4, 0)
        Label1.Name = "Label1"
        Label1.Size = New Size(15, 46)
        Label1.TabIndex = 5
        ' 
        ' SelectieReceptiiForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(903, 750)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "SelectieReceptiiForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Ce recepții reîmprospătez?"
        pnlCard.ResumeLayout(False)
        CType(grilaReceptii, ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As Label
    Friend WithEvents grilaReceptii As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblTotal As Label
    Friend WithEvents btnDescarca As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents Label1 As Label
End Class
