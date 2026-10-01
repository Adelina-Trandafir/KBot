' The «Actualizeaza angajamente» window (slice 0100): the choice of the angajamente that are
' downloaded together, on several FOREXE tabs.
'
' Every control is declared HERE, like any form of K-BOT (docs/kbot-forms-ui-convention.md):
' the designer draws them, the code behind builds none. The children of `pnlCard` are added in
' REVERSE dock order -- the grid (Fill) first, then the bars -- because WinForms puts the
' last-added control nearest to the edge.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ActualizareMultiplaForm
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
        tips = New KBot.Controls.KBotToolTip(components)
        btnActualizeaza = New Button()
        btnRenunta = New Button()
        btnVechi = New Button()
        pnlCard = New Panel()
        grilaAngajamente = New Controls.KBotDataView()
        pnlJos = New Panel()
        lblSpatiu1 = New Label()
        lblSpatiu2 = New Label()
        lblTotal = New Label()
        lblAntet = New Label()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        CType(grilaAngajamente, ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' btnActualizeaza
        '
        btnActualizeaza.Dock = DockStyle.Right
        btnActualizeaza.FlatStyle = FlatStyle.Flat
        btnActualizeaza.Location = New Point(735, 10)
        btnActualizeaza.Margin = New Padding(0)
        btnActualizeaza.Name = "btnActualizeaza"
        btnActualizeaza.Size = New Size(154, 46)
        btnActualizeaza.TabIndex = 4
        btnActualizeaza.Text = "Actualizează"
        tips.SetToolTipHeader(btnActualizeaza, "Actualizează")
        tips.SetToolTipText(btnActualizeaza, "Descarcă din FOREXE angajamentele <b>bifate</b>, mai multe deodată (fiecare pe tabul lui)." & vbLf & "Cele peste numărul de taburi așteaptă la rând.")
        btnActualizeaza.UseVisualStyleBackColor = True
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
        btnRenunta.TabIndex = 3
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să descarce nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnVechi
        '
        btnVechi.AutoSize = True
        btnVechi.Dock = DockStyle.Right
        btnVechi.FlatStyle = FlatStyle.Flat
        btnVechi.Location = New Point(300, 10)
        btnVechi.Margin = New Padding(0)
        btnVechi.Name = "btnVechi"
        btnVechi.Padding = New Padding(10, 0, 10, 0)
        btnVechi.Size = New Size(250, 46)
        btnVechi.TabIndex = 2
        btnVechi.Text = "Bifează cele vechi"
        tips.SetToolTipHeader(btnVechi, "Cele neactualizate")
        tips.SetToolTipText(btnVechi, "Bifează doar angajamentele descărcate deja, dar neactualizate de cel puțin numărul de zile din «Setări → Aplicație».")
        btnVechi.UseVisualStyleBackColor = True
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(grilaAngajamente)
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
        ' grilaAngajamente
        '
        grilaAngajamente.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grilaAngajamente.BackColor = SystemColors.Window
        grilaAngajamente.CellTooltip.Enabled = False
        grilaAngajamente.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
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
        KBotDataColumn2.HeaderText = "Cod"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "cod"
        KBotDataColumn2.MinWidth = 100
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 130
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Descriere"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "descriere"
        KBotDataColumn3.MinWidth = 150
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 330
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Stare"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "stare"
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 110
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Actualizat"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "actualizat"
        KBotDataColumn5.MinWidth = 120
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 150
        grilaAngajamente.Columns.Add(KBotDataColumn1)
        grilaAngajamente.Columns.Add(KBotDataColumn2)
        grilaAngajamente.Columns.Add(KBotDataColumn3)
        grilaAngajamente.Columns.Add(KBotDataColumn4)
        grilaAngajamente.Columns.Add(KBotDataColumn5)
        grilaAngajamente.Dock = DockStyle.Fill
        grilaAngajamente.Location = New Point(0, 153)
        grilaAngajamente.Margin = New Padding(4)
        grilaAngajamente.Name = "grilaAngajamente"
        grilaAngajamente.Size = New Size(899, 517)
        grilaAngajamente.TabIndex = 2
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblTotal)
        pnlJos.Controls.Add(btnVechi)
        pnlJos.Controls.Add(lblSpatiu2)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Controls.Add(lblSpatiu1)
        pnlJos.Controls.Add(btnActualizeaza)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 670)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(0, 10, 10, 20)
        pnlJos.Size = New Size(899, 76)
        pnlJos.TabIndex = 3
        pnlJos.Tag = "Card"
        '
        ' lblSpatiu1
        '
        lblSpatiu1.Dock = DockStyle.Right
        lblSpatiu1.Location = New Point(720, 10)
        lblSpatiu1.Margin = New Padding(4, 0, 4, 0)
        lblSpatiu1.Name = "lblSpatiu1"
        lblSpatiu1.Size = New Size(15, 46)
        lblSpatiu1.TabIndex = 5
        '
        ' lblSpatiu2
        '
        lblSpatiu2.Dock = DockStyle.Right
        lblSpatiu2.Location = New Point(550, 10)
        lblSpatiu2.Margin = New Padding(4, 0, 4, 0)
        lblSpatiu2.Name = "lblSpatiu2"
        lblSpatiu2.Size = New Size(15, 46)
        lblSpatiu2.TabIndex = 6
        '
        ' lblTotal
        '
        lblTotal.AutoEllipsis = True
        lblTotal.Dock = DockStyle.Fill
        lblTotal.Location = New Point(0, 10)
        lblTotal.Margin = New Padding(4, 0, 4, 0)
        lblTotal.Name = "lblTotal"
        lblTotal.Padding = New Padding(20, 0, 0, 0)
        lblTotal.Size = New Size(300, 46)
        lblTotal.TabIndex = 0
        lblTotal.Text = "Nimic bifat."
        lblTotal.TextAlign = ContentAlignment.MiddleLeft
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
        lblAntet.Text = "Bifează angajamentele pe care vrei să le aduci la zi din FOREXE. Se descarcă mai multe deodată, fiecare pe un tab FOREXE al lui; cele peste numărul de taburi așteaptă la rând."
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
        capBar.Text = "K-BOT — Actualizează angajamente"
        '
        ' ActualizareMultiplaForm
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
        Name = "ActualizareMultiplaForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Actualizează angajamente"
        pnlCard.ResumeLayout(False)
        CType(grilaAngajamente, ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As Label
    Friend WithEvents grilaAngajamente As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblTotal As Label
    Friend WithEvents btnActualizeaza As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnVechi As Button
    Friend WithEvents lblSpatiu1 As Label
    Friend WithEvents lblSpatiu2 As Label
End Class
