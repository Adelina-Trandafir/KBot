Imports KBot.Controls

' Slice 000T-04 -- the small bar shown while the tutorial recorder watches (operator tool).
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md). Coordonate la 96 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TutorialRecorderBar
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
        tips = New KBotToolTip(components)
        pnlCard = New Panel()
        tlpBar = New KBotTableLayoutPanel()
        lblInfo = New Label()
        btnOpreste = New Button()
        pnlCard.SuspendLayout()
        tlpBar.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlpBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Name = "pnlCard"
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlpBar
        '
        tlpBar.ColumnCount = 2
        tlpBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlpBar.Controls.Add(lblInfo, 0, 0)
        tlpBar.Controls.Add(btnOpreste, 1, 0)
        tlpBar.Dock = DockStyle.Fill
        tlpBar.Name = "tlpBar"
        tlpBar.Padding = New Padding(10, 6, 10, 6)
        tlpBar.RowCount = 1
        tlpBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpBar.TabIndex = 0
        tlpBar.Tag = "Card"
        '
        ' lblInfo
        '
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Name = "lblInfo"
        lblInfo.TabIndex = 0
        lblInfo.Text = "Înregistrez: 0 pași. Lucrează în K-BOT ca de obicei."
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnOpreste
        '
        btnOpreste.Dock = DockStyle.Fill
        btnOpreste.FlatStyle = FlatStyle.Flat
        btnOpreste.Font = New Font("Segoe UI Semibold", 9.0F)
        btnOpreste.Margin = New Padding(6, 0, 0, 0)
        btnOpreste.Name = "btnOpreste"
        btnOpreste.TabIndex = 1
        btnOpreste.Text = "Oprește"
        tips.SetToolTipHeader(btnOpreste, "Oprește înregistrarea")
        tips.SetToolTipText(btnOpreste, "Pașii înregistrați se adaugă în designer, după pasul ales.")
        btnOpreste.UseVisualStyleBackColor = True
        '
        ' TutorialRecorderBar
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(440, 46)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "TutorialRecorderBar"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Înregistrare tutorial"
        TopMost = True
        pnlCard.ResumeLayout(False)
        tlpBar.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlpBar As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblInfo As Label
    Friend WithEvents btnOpreste As Button
End Class
