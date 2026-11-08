' The window the Sumar button «Asociaza parteneri» opens (slice 0084-02): the partners associated
' with the angajament's DDF, and the picker for more. The list is the shared DdfPartnersView
' (the same one the DDF editor's «Parteneri» page hosts); this window only loads it, keeps the
' partners the operator picked until «Salveaza», and sends them.
'
' A modal dialog over the shell, like LogClearDialog: borderless, its own caption bar, one card.
' Reverse dock order inside the card (Fill first, then Bottom, then the Tops).
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are written at 144 dpi; AutoScaleDimensions goes with them (slice 0066-02).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SumarPartnersForm
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
        tips = New KBot.Controls.KBotToolTip(components)
        btnSave = New Button()
        btnClose = New Button()
        pnlCard = New Panel()
        vwPartners = New DdfPartnersView()
        pnlBottom = New Panel()
        lblSep = New Label()
        lblInfo = New Label()
        busy = New Controls.KBotBusyBar()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        '
        ' btnSave
        '
        btnSave.Dock = DockStyle.Right
        btnSave.Enabled = False
        btnSave.FlatStyle = FlatStyle.Flat
        btnSave.Location = New Point(729, 8)
        btnSave.Margin = New Padding(4)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(160, 42)
        btnSave.TabIndex = 1
        btnSave.Text = "Salvează"
        tips.SetToolTipHeader(btnSave, "Salvează asocierea")
        tips.SetToolTipText(btnSave, "Asociază partenerii marcați «De adăugat» cu documentul de fundamentare al angajamentului." & vbLf & "Un partener deja asociat nu se adaugă a doua oară.")
        btnSave.UseVisualStyleBackColor = True
        '
        ' btnClose
        '
        btnClose.DialogResult = DialogResult.Cancel
        btnClose.Dock = DockStyle.Right
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.Location = New Point(906, 8)
        btnClose.Margin = New Padding(4)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(160, 42)
        btnClose.TabIndex = 2
        btnClose.Text = "Renunță"
        tips.SetToolTipHeader(btnClose, "Renunță")
        tips.SetToolTipText(btnClose, "Închide fereastra fără să asocieze partenerii noi.")
        btnClose.UseVisualStyleBackColor = True
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(vwPartners)
        pnlCard.Controls.Add(pnlBottom)
        pnlCard.Controls.Add(lblInfo)
        pnlCard.Controls.Add(busy)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1096, 716)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' vwPartners
        '
        vwPartners.Dock = DockStyle.Fill
        vwPartners.Location = New Point(0, 141)
        vwPartners.Margin = New Padding(0)
        vwPartners.Name = "vwPartners"
        vwPartners.Size = New Size(1096, 517)
        vwPartners.TabIndex = 3
        '
        ' pnlBottom
        '
        pnlBottom.Controls.Add(btnSave)
        pnlBottom.Controls.Add(lblSep)
        pnlBottom.Controls.Add(btnClose)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New Point(0, 658)
        pnlBottom.Margin = New Padding(4)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Padding = New Padding(8)
        pnlBottom.Size = New Size(1096, 58)
        pnlBottom.TabIndex = 4
        pnlBottom.Tag = "Card"
        '
        ' lblSep
        '
        lblSep.Dock = DockStyle.Right
        lblSep.Location = New Point(889, 8)
        lblSep.Name = "lblSep"
        lblSep.Size = New Size(17, 42)
        lblSep.TabIndex = 3
        lblSep.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblInfo
        '
        lblInfo.Dock = DockStyle.Top
        lblInfo.Location = New Point(0, 58)
        lblInfo.Margin = New Padding(4, 0, 4, 0)
        lblInfo.Name = "lblInfo"
        lblInfo.Padding = New Padding(18, 15, 18, 15)
        lblInfo.Size = New Size(1096, 83)
        lblInfo.TabIndex = 2
        lblInfo.Text = "Asociază angajamentul cu cel puțin un partener. Partenerii deja asociați rămân; aici se pot doar adăuga alții."
        '
        ' busy
        '
        busy.Dock = DockStyle.Top
        busy.Location = New Point(0, 52)
        busy.Margin = New Padding(4)
        busy.Name = "busy"
        busy.Size = New Size(1096, 6)
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
        capBar.Size = New Size(1096, 52)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Asociază parteneri"
        '
        ' SumarPartnersForm
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(1100, 720)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "SumarPartnersForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Asociază parteneri"
        pnlCard.ResumeLayout(False)
        pnlBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents busy As Global.KBot.Controls.KBotBusyBar
    Friend WithEvents lblInfo As Label
    Friend WithEvents vwPartners As DdfPartnersView
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents lblSep As Label
End Class
