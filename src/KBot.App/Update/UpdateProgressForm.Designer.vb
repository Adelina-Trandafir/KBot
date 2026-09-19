<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UpdateProgressForm
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
        pnlCard = New Panel()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        lblAntet = New Label()
        bara = New Global.KBot.Controls.KBotProgressBar()
        lblDetaliu = New Label()
        pnlJos = New Panel()
        btnRenunta = New Button()

        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard -- docked children in REVERSE dock order (bottom bar first, caption last);
        ' the anchored ones in between.
        '
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblDetaliu)
        pnlCard.Controls.Add(bara)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(518, 198)
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
        capBar.Size = New Size(518, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Actualizare K-BOT"
        '
        ' lblAntet -- the three middle controls are anchored, not docked: the bar keeps a
        ' 16 px side gutter, which Dock=Top cannot give it.
        '
        lblAntet.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblAntet.AutoEllipsis = True
        lblAntet.Location = New Point(16, 52)
        lblAntet.Name = "lblAntet"
        lblAntet.Size = New Size(486, 24)
        lblAntet.TabIndex = 1
        lblAntet.Text = "Se descarcă actualizarea…"
        '
        ' bara
        '
        bara.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        bara.Location = New Point(16, 84)
        bara.Name = "bara"
        bara.ShowPercentText = True
        bara.Size = New Size(486, 22)
        bara.TabIndex = 2
        '
        ' lblDetaliu
        '
        lblDetaliu.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblDetaliu.AutoEllipsis = True
        lblDetaliu.Location = New Point(16, 112)
        lblDetaliu.Name = "lblDetaliu"
        lblDetaliu.Size = New Size(486, 22)
        lblDetaliu.TabIndex = 3
        lblDetaliu.Text = ""
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 142)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(12, 8, 12, 8)
        pnlJos.Size = New Size(518, 56)
        pnlJos.TabIndex = 4
        pnlJos.Tag = "Card"
        '
        ' btnRenunta
        '
        btnRenunta.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(396, 12)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(110, 32)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' UpdateProgressForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(520, 200)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "UpdateProgressForm"
        Padding = New Padding(1)
        ShowInTaskbar = True
        StartPosition = FormStartPosition.CenterScreen
        Text = "Actualizare K-BOT"

        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        '
        ' tips
        '
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Oprește descărcarea. Actualizarea se poate relua oricând." & vbLf & "La o actualizare obligatorie, aplicația se închide.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As Label
    Friend WithEvents bara As Global.KBot.Controls.KBotProgressBar
    Friend WithEvents lblDetaliu As Label
    Friend WithEvents pnlJos As Panel
    Friend WithEvents btnRenunta As Button
End Class
