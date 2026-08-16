<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LogClearDialog
    Inherits KBot.Theming.KBotThemedForm

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
        tips = New KBot.Controls.KBotToolTip(components)
        Dim colSel As New KBot.Controls.KBotDataColumn()
        Dim colFisier As New KBot.Controls.KBotDataColumn()
        Dim colMarime As New KBot.Controls.KBotDataColumn()
        Dim colIntrari As New KBot.Controls.KBotDataColumn()
        Dim colStare As New KBot.Controls.KBotDataColumn()

        pnlCard = New Panel()
        capBar = New KBot.Controls.KBotCaptionBar()
        lblAntet = New Label()
        grilaFisiere = New KBot.Controls.KBotDataView()
        pnlJos = New Panel()
        lblTotal = New Label()
        btnSterge = New Button()
        btnRenunta = New Button()
        busy = New KBot.Controls.KBotBusyBar()

        pnlCard.SuspendLayout()
        CType(grilaFisiere, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard — copiii în ordine INVERSĂ de dock: grila (Fill), apoi barele.
        '
        pnlCard.Controls.Add(grilaFisiere)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(698, 458)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.ShowMaximize = False
        capBar.ShowMinimize = False
        capBar.Size = New Size(698, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Golește jurnale"
        '
        ' busy
        '
        busy.Dock = DockStyle.Top
        busy.Location = New Point(0, 40)
        busy.Name = "busy"
        busy.Size = New Size(698, 4)
        busy.TabIndex = 1
        '
        ' lblAntet
        '
        lblAntet.Dock = DockStyle.Top
        lblAntet.Location = New Point(0, 44)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New Padding(12, 10, 12, 10)
        lblAntet.Size = New Size(698, 58)
        lblAntet.TabIndex = 2
        lblAntet.Text = "Bifează fișierele de șters. Ștergerea NU se poate anula. Jurnalele de server nu se ating de aici — rutele sunt doar de citire."
        '
        ' grilaFisiere
        '
        grilaFisiere.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grilaFisiere.ColumnFillMode = KBot.Controls.KBotFillMode.FirstColumn
        colSel.ColumnType = KBot.Controls.KBotColumnType.CheckBox
        colSel.FormatString = Nothing
        colSel.HeaderText = "Șterge"
        colSel.HeaderTextAlign = ContentAlignment.MiddleCenter
        colSel.Key = "sel"
        colSel.MinWidth = 60
        colSel.OptionGroup = Nothing
        colSel.TextAlign = ContentAlignment.MiddleCenter
        colSel.Width = 70
        colFisier.FormatString = Nothing
        colFisier.HeaderText = "Fișier"
        colFisier.HeaderTextAlign = ContentAlignment.MiddleCenter
        colFisier.Key = "fisier"
        colFisier.MinWidth = 120
        colFisier.OptionGroup = Nothing
        colFisier.ReadOnly = True
        colFisier.Width = 240
        colMarime.FormatString = Nothing
        colMarime.HeaderText = "Mărime"
        colMarime.HeaderTextAlign = ContentAlignment.MiddleCenter
        colMarime.Key = "marime"
        colMarime.MinWidth = 70
        colMarime.OptionGroup = Nothing
        colMarime.ReadOnly = True
        colMarime.TextAlign = ContentAlignment.MiddleRight
        colMarime.Width = 100
        colIntrari.FormatString = Nothing
        colIntrari.HeaderText = "Intrări"
        colIntrari.HeaderTextAlign = ContentAlignment.MiddleCenter
        colIntrari.Key = "intrari"
        colIntrari.MinWidth = 60
        colIntrari.OptionGroup = Nothing
        colIntrari.ReadOnly = True
        colIntrari.TextAlign = ContentAlignment.MiddleRight
        colIntrari.Width = 80
        colStare.FormatString = Nothing
        colStare.HeaderText = "Stare"
        colStare.HeaderTextAlign = ContentAlignment.MiddleCenter
        colStare.Key = "stare"
        colStare.MinWidth = 100
        colStare.OptionGroup = Nothing
        colStare.ReadOnly = True
        colStare.Width = 180
        grilaFisiere.Columns.Add(colSel)
        grilaFisiere.Columns.Add(colFisier)
        grilaFisiere.Columns.Add(colMarime)
        grilaFisiere.Columns.Add(colIntrari)
        grilaFisiere.Columns.Add(colStare)
        grilaFisiere.Dock = DockStyle.Fill
        grilaFisiere.HeaderHeight = 30
        grilaFisiere.Location = New Point(0, 102)
        grilaFisiere.Name = "grilaFisiere"
        grilaFisiere.Size = New Size(698, 300)
        grilaFisiere.TabIndex = 3
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblTotal)
        pnlJos.Controls.Add(btnSterge)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 402)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(12, 8, 12, 8)
        pnlJos.Size = New Size(698, 56)
        pnlJos.TabIndex = 4
        pnlJos.Tag = "Card"
        '
        ' lblTotal
        '
        lblTotal.AutoSize = True
        lblTotal.Location = New Point(12, 20)
        lblTotal.Name = "lblTotal"
        lblTotal.Size = New Size(150, 15)
        lblTotal.TabIndex = 0
        lblTotal.Text = "Nimic bifat."
        '
        ' btnSterge
        '
        btnSterge.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSterge.Enabled = False
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Location = New Point(430, 12)
        btnSterge.Name = "btnSterge"
        btnSterge.Size = New Size(150, 32)
        btnSterge.TabIndex = 1
        btnSterge.Text = "Șterge bifatele"
        btnSterge.UseVisualStyleBackColor = True
        '
        ' btnRenunta
        '
        btnRenunta.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(588, 12)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(98, 32)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' LogClearDialog
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnRenunta
        ClientSize = New Size(700, 460)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "LogClearDialog"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Golește jurnale"

        pnlCard.ResumeLayout(False)
        CType(grilaFisiere, System.ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        '
        ' tips — etichetele de survolare (felia 0035), toate în română.
        '
        tips.SetToolTipHeader(btnSterge, "Șterge")
        tips.SetToolTipText(btnSterge, "<b>Șterge definitiv</b> fișierele bifate." & vbLf & "Operația nu se poate desface.")
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să șteargă nimic.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents busy As KBot.Controls.KBotBusyBar
    Friend WithEvents lblAntet As Label
    Friend WithEvents grilaFisiere As KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblTotal As Label
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnRenunta As Button
End Class
