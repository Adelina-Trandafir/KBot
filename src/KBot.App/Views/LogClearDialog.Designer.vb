<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LogClearDialog
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
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn9 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn10 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        btnSterge = New Button()
        btnRenunta = New Button()
        pnlCard = New Panel()
        grilaFisiere = New Controls.KBotDataView()
        pnlJos = New Panel()
        lblTotal = New Label()
        lblAntet = New Label()
        busy = New Controls.KBotBusyBar()
        capBar = New Controls.KBotCaptionBar()
        lblSep1 = New Label()
        pnlCard.SuspendLayout()
        CType(grilaFisiere, ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnSterge
        ' 
        btnSterge.Dock = DockStyle.Right
        btnSterge.Enabled = False
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Location = New Point(729, 8)
        btnSterge.Margin = New Padding(4)
        btnSterge.Name = "btnSterge"
        btnSterge.Size = New Size(147, 42)
        btnSterge.TabIndex = 1
        btnSterge.Text = "Șterge"
        tips.SetToolTipHeader(btnSterge, "Șterge")
        tips.SetToolTipText(btnSterge, "<b>Șterge definitiv</b> fișierele bifate." & vbLf & "Operația nu se poate desface.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(891, 8)
        btnRenunta.Margin = New Padding(4)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(147, 42)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să șteargă nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(grilaFisiere)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1046, 675)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' grilaFisiere
        ' 
        grilaFisiere.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grilaFisiere.BackColor = SystemColors.Window
        grilaFisiere.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderRightIcon = My.Resources.Resources.Fatcow_Farm_Fresh_Check_boxes_32
        KBotDataColumn6.HeaderText = ""
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "sel"
        KBotDataColumn6.MinWidth = 20
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Width = 32
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Fișier"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "fisier"
        KBotDataColumn7.MinWidth = 120
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 240
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Mărime"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "marime"
        KBotDataColumn8.MinWidth = 70
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Intrări"
        KBotDataColumn9.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "intrari"
        KBotDataColumn9.MinWidth = 60
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn9.Width = 80
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Stare"
        KBotDataColumn10.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "stare"
        KBotDataColumn10.MinWidth = 100
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.Visible = KBot.Controls.KBotColumnVisibility.Hidden
        KBotDataColumn10.Width = 180
        grilaFisiere.Columns.Add(KBotDataColumn6)
        grilaFisiere.Columns.Add(KBotDataColumn7)
        grilaFisiere.Columns.Add(KBotDataColumn8)
        grilaFisiere.Columns.Add(KBotDataColumn9)
        grilaFisiere.Columns.Add(KBotDataColumn10)
        grilaFisiere.Dock = DockStyle.Fill
        grilaFisiere.FillColumnKey = "fisier"
        grilaFisiere.HeaderHeight = 24
        grilaFisiere.HeaderSeparatorColor = SystemColors.ActiveBorder
        grilaFisiere.Location = New Point(0, 141)
        grilaFisiere.Margin = New Padding(4)
        grilaFisiere.Name = "grilaFisiere"
        grilaFisiere.Size = New Size(1046, 476)
        grilaFisiere.TabIndex = 3
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(btnSterge)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(lblTotal)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 617)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(8)
        pnlJos.Size = New Size(1046, 58)
        pnlJos.TabIndex = 4
        pnlJos.Tag = "Card"
        ' 
        ' lblTotal
        ' 
        lblTotal.AutoSize = True
        lblTotal.Location = New Point(12, 18)
        lblTotal.Margin = New Padding(4, 0, 4, 0)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New Size(95, 22)
        lblTotal.TabIndex = 0
        lblTotal.Text = "Nimic bifat."
        ' 
        ' lblAntet
        ' 
        lblAntet.Dock = DockStyle.Top
        lblAntet.Location = New Point(0, 58)
        lblAntet.Margin = New Padding(4, 0, 4, 0)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New Padding(18, 15, 18, 15)
        lblAntet.Size = New Size(1046, 83)
        lblAntet.TabIndex = 2
        lblAntet.Text = "Bifează fișierele de șters. Ștergerea NU se poate anula. Jurnalele de server nu se ating de aici — rutele sunt doar de citire."
        ' 
        ' busy
        ' 
        busy.Dock = DockStyle.Top
        busy.Location = New Point(0, 52)
        busy.Margin = New Padding(4)
        busy.Name = "busy"
        busy.Size = New Size(1046, 6)
        busy.TabIndex = 1
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
        capBar.Size = New Size(1046, 52)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Golește jurnale"
        ' 
        ' lblSep1
        ' 
        lblSep1.Dock = DockStyle.Right
        lblSep1.Location = New Point(876, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New Size(15, 42)
        lblSep1.TabIndex = 3
        lblSep1.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' LogClearDialog
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1050, 679)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "LogClearDialog"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Golește jurnale"
        pnlCard.ResumeLayout(False)
        CType(grilaFisiere, ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busy As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents lblAntet As Label
    Friend WithEvents grilaFisiere As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblTotal As Label
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents lblSep1 As Label
End Class
