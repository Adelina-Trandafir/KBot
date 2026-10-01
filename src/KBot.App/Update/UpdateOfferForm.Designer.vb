<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UpdateOfferForm
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
        pnlCard = New Panel()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlText = New Panel()
        edNoutati = New Global.KBot.Controls.KBotRichTextEditor()
        lblIntrebare = New Label()
        pnlJos = New Panel()
        btnDa = New Button()
        btnNu = New Button()

        pnlCard.SuspendLayout()
        pnlText.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard -- docked children in REVERSE dock order (Fill first, then the bottom ones,
        ' the caption last).
        '
        pnlCard.Controls.Add(pnlText)
        pnlCard.Controls.Add(lblIntrebare)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(638, 518)
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
        capBar.Size = New Size(638, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Actualizare K-BOT"
        '
        ' pnlText -- the 16 px side gutter around the notes
        '
        pnlText.Controls.Add(edNoutati)
        pnlText.Dock = DockStyle.Fill
        pnlText.Location = New Point(0, 40)
        pnlText.Name = "pnlText"
        pnlText.Padding = New Padding(16, 8, 16, 8)
        pnlText.Size = New Size(638, 342)
        pnlText.TabIndex = 1
        pnlText.Tag = "Card"
        '
        ' edNoutati -- read only, no toolbar, no footer; the text wraps, only the vertical bar scrolls
        '
        edNoutati.CollapseButton = False
        edNoutati.Dock = DockStyle.Fill
        edNoutati.FooterVisible = False
        edNoutati.HeaderVisible = False
        edNoutati.Location = New Point(16, 8)
        edNoutati.Name = "edNoutati"
        edNoutati.Size = New Size(606, 326)
        edNoutati.TabIndex = 0
        '
        ' lblIntrebare
        '
        lblIntrebare.Dock = DockStyle.Bottom
        lblIntrebare.Location = New Point(0, 382)
        lblIntrebare.Name = "lblIntrebare"
        lblIntrebare.Padding = New Padding(16, 4, 16, 4)
        lblIntrebare.Size = New Size(638, 64)
        lblIntrebare.TabIndex = 2
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnDa)
        pnlJos.Controls.Add(btnNu)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 446)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(12, 8, 12, 8)
        pnlJos.Size = New Size(638, 72)
        pnlJos.TabIndex = 3
        pnlJos.Tag = "Card"
        '
        ' btnDa
        '
        btnDa.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnDa.DialogResult = DialogResult.OK
        btnDa.FlatStyle = FlatStyle.Flat
        btnDa.Location = New Point(396, 20)
        btnDa.Name = "btnDa"
        btnDa.Size = New Size(110, 32)
        btnDa.TabIndex = 0
        btnDa.Text = "Da"
        btnDa.UseVisualStyleBackColor = True
        '
        ' btnNu
        '
        btnNu.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnNu.DialogResult = DialogResult.Cancel
        btnNu.FlatStyle = FlatStyle.Flat
        btnNu.Location = New Point(514, 20)
        btnNu.Name = "btnNu"
        btnNu.Size = New Size(110, 32)
        btnNu.TabIndex = 1
        btnNu.Text = "Nu"
        btnNu.UseVisualStyleBackColor = True
        '
        ' UpdateOfferForm
        '
        AcceptButton = btnDa
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnNu
        ClientSize = New Size(640, 520)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "UpdateOfferForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Actualizare K-BOT"

        pnlCard.ResumeLayout(False)
        pnlText.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents pnlText As Panel
    Friend WithEvents edNoutati As Global.KBot.Controls.KBotRichTextEditor
    Friend WithEvents lblIntrebare As Label
    Friend WithEvents pnlJos As Panel
    Friend WithEvents btnDa As Button
    Friend WithEvents btnNu As Button
End Class
