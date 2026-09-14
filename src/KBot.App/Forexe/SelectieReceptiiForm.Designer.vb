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
        tips = New Global.KBot.Controls.KBotToolTip(components)
        Dim colSel As New Global.KBot.Controls.KBotDataColumn()
        Dim colNrCrt As New Global.KBot.Controls.KBotDataColumn()
        Dim colData As New Global.KBot.Controls.KBotDataColumn()
        Dim colSuma As New Global.KBot.Controls.KBotDataColumn()
        Dim colAntete As New Global.KBot.Controls.KBotDataColumn()
        Dim colStare As New Global.KBot.Controls.KBotDataColumn()

        pnlCard = New Panel()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        lblAntet = New Label()
        grilaReceptii = New Global.KBot.Controls.KBotDataView()
        pnlJos = New Panel()
        lblTotal = New Label()
        btnTot = New Button()
        btnNimic = New Button()
        btnDescarca = New Button()
        btnRenunta = New Button()

        pnlCard.SuspendLayout()
        CType(grilaReceptii, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard — copiii în ordine INVERSĂ de dock: grila (Fill), apoi barele.
        '
        pnlCard.Controls.Add(grilaReceptii)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(738, 498)
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
        capBar.Size = New Size(738, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ce recepții reîmprospătez?"
        '
        ' lblAntet
        '
        lblAntet.Dock = DockStyle.Top
        lblAntet.Location = New Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New Padding(12, 10, 12, 10)
        lblAntet.Size = New Size(738, 62)
        lblAntet.TabIndex = 1
        lblAntet.Text = "Bifează recepțiile pe care vrei să le citească din nou din FOREXE. O recepție nebifată" &
                        " nu se atinge: rămâne în bază exact cum e acum. Recepțiile care există în FOREXE" &
                        " dar nu și aici se descarcă întotdeauna — nu au cum să fie în listă."
        '
        ' grilaReceptii
        '
        grilaReceptii.AutoSizeColumnsMode = Global.KBot.Controls.KBotAutoSizeMode.None
        grilaReceptii.ColumnFillMode = Global.KBot.Controls.KBotFillMode.LastColumn
        colSel.ColumnType = Global.KBot.Controls.KBotColumnType.CheckBox
        colSel.FormatString = Nothing
        colSel.HeaderText = "Reîmprospătez"
        colSel.HeaderTextAlign = ContentAlignment.MiddleCenter
        colSel.Key = "sel"
        colSel.MinWidth = 90
        colSel.OptionGroup = Nothing
        colSel.TextAlign = ContentAlignment.MiddleCenter
        colSel.Width = 110
        colNrCrt.FormatString = Nothing
        colNrCrt.HeaderText = "Nr."
        colNrCrt.HeaderTextAlign = ContentAlignment.MiddleCenter
        colNrCrt.Key = "nrcrt"
        colNrCrt.MinWidth = 50
        colNrCrt.OptionGroup = Nothing
        colNrCrt.ReadOnly = True
        colNrCrt.TextAlign = ContentAlignment.MiddleRight
        colNrCrt.Width = 60
        colData.FormatString = Nothing
        colData.HeaderText = "Data"
        colData.HeaderTextAlign = ContentAlignment.MiddleCenter
        colData.Key = "data"
        colData.MinWidth = 90
        colData.OptionGroup = Nothing
        colData.ReadOnly = True
        colData.TextAlign = ContentAlignment.MiddleCenter
        colData.Width = 110
        colSuma.FormatString = Nothing
        colSuma.HeaderText = "Valoare"
        colSuma.HeaderTextAlign = ContentAlignment.MiddleCenter
        colSuma.Key = "suma"
        colSuma.MinWidth = 100
        colSuma.OptionGroup = Nothing
        colSuma.ReadOnly = True
        colSuma.TextAlign = ContentAlignment.MiddleRight
        colSuma.Width = 140
        colAntete.FormatString = Nothing
        colAntete.HeaderText = "Instantanee"
        colAntete.HeaderTextAlign = ContentAlignment.MiddleCenter
        colAntete.Key = "antete"
        colAntete.MinWidth = 80
        colAntete.OptionGroup = Nothing
        colAntete.ReadOnly = True
        colAntete.TextAlign = ContentAlignment.MiddleRight
        colAntete.Width = 90
        colStare.FormatString = Nothing
        colStare.HeaderText = "Stare"
        colStare.HeaderTextAlign = ContentAlignment.MiddleCenter
        colStare.Key = "stare"
        colStare.MinWidth = 120
        colStare.OptionGroup = Nothing
        colStare.ReadOnly = True
        colStare.Width = 200
        grilaReceptii.Columns.Add(colSel)
        grilaReceptii.Columns.Add(colNrCrt)
        grilaReceptii.Columns.Add(colData)
        grilaReceptii.Columns.Add(colSuma)
        grilaReceptii.Columns.Add(colAntete)
        grilaReceptii.Columns.Add(colStare)
        grilaReceptii.Dock = DockStyle.Fill
        grilaReceptii.HeaderHeight = 30
        grilaReceptii.Location = New Point(0, 102)
        grilaReceptii.Name = "grilaReceptii"
        grilaReceptii.Size = New Size(738, 340)
        grilaReceptii.TabIndex = 2
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblTotal)
        pnlJos.Controls.Add(btnTot)
        pnlJos.Controls.Add(btnNimic)
        pnlJos.Controls.Add(btnDescarca)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 442)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(12, 8, 12, 8)
        pnlJos.Size = New Size(738, 56)
        pnlJos.TabIndex = 3
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
        ' btnTot
        '
        btnTot.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnTot.FlatStyle = FlatStyle.Flat
        btnTot.Location = New Point(288, 12)
        btnTot.Name = "btnTot"
        btnTot.Size = New Size(96, 32)
        btnTot.TabIndex = 1
        btnTot.Text = "Bifează tot"
        btnTot.UseVisualStyleBackColor = True
        '
        ' btnNimic
        '
        btnNimic.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnNimic.FlatStyle = FlatStyle.Flat
        btnNimic.Location = New Point(392, 12)
        btnNimic.Name = "btnNimic"
        btnNimic.Size = New Size(96, 32)
        btnNimic.TabIndex = 2
        btnNimic.Text = "Debifează tot"
        btnNimic.UseVisualStyleBackColor = True
        '
        ' btnDescarca
        '
        btnDescarca.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnDescarca.FlatStyle = FlatStyle.Flat
        btnDescarca.Location = New Point(496, 12)
        btnDescarca.Name = "btnDescarca"
        btnDescarca.Size = New Size(118, 32)
        btnDescarca.TabIndex = 3
        btnDescarca.Text = "Descarcă"
        btnDescarca.UseVisualStyleBackColor = True
        '
        ' btnRenunta
        '
        btnRenunta.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(622, 12)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(98, 32)
        btnRenunta.TabIndex = 4
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' SelectieReceptiiForm
        '
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnRenunta
        ClientSize = New Size(740, 500)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "SelectieReceptiiForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Ce recepții reîmprospătez?"

        pnlCard.ResumeLayout(False)
        CType(grilaReceptii, System.ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        pnlJos.PerformLayout()
        '
        ' tips — etichetele de survolare, toate în română.
        '
        tips.SetToolTipHeader(btnDescarca, "Descarcă")
        tips.SetToolTipText(btnDescarca, "Pornește descărcarea din FOREXE." & vbLf &
                                         "Recepțiile <b>nebifate</b> nu se deschid și nu se rescriu.")
        tips.SetToolTipHeader(btnTot, "Bifează tot")
        tips.SetToolTipText(btnTot, "Bifează toate recepțiile din listă.")
        tips.SetToolTipHeader(btnNimic, "Debifează tot")
        tips.SetToolTipText(btnNimic, "Lasă lista goală: se aduc doar recepțiile pe care" & vbLf &
                                      "K-BOT nu le are încă.")
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să descarce nimic.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As Label
    Friend WithEvents grilaReceptii As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblTotal As Label
    Friend WithEvents btnTot As Button
    Friend WithEvents btnNimic As Button
    Friend WithEvents btnDescarca As Button
    Friend WithEvents btnRenunta As Button
End Class
