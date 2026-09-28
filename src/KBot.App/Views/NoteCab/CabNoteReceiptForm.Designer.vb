Imports KBot.Controls

' «Nota trimisă în CAB» (slice 0088-04): what FOREXE answered to the upload of a correction note
' (or why it failed), the registration index of the upload, and «Verifică recipisa», which looks
' for FOREXE's receipt in the SNM inbox, downloads it and stores it on the server.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in 144 dpi.
' Card: children in REVERSE dock order (Fill first, then Bottom, then Top).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CabNoteReceiptForm
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
        tlyCorp = New KBotTableLayoutPanel()
        lblTitlu = New Label()
        txtRaspuns = New TextBox()
        tlyIndex = New KBotTableLayoutPanel()
        lblIndexCaption = New Label()
        txtIndex = New KBotTextField()
        pnlJos = New Panel()
        lblStare = New Label()
        btnDeschide = New Button()
        btnVerifica = New Button()
        btnInchide = New Button()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        tlyIndex.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1146, 716)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' tlyCorp -- headline, FOREXE's answer, the registration index.
        '
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblTitlu, 0, 0)
        tlyCorp.Controls.Add(txtRaspuns, 0, 1)
        tlyCorp.Controls.Add(tlyIndex, 0, 2)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 60)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(18, 14, 18, 6)
        tlyCorp.RowCount = 3
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 70F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 76F))
        tlyCorp.Size = New Size(1146, 580)
        tlyCorp.TabIndex = 1
        '
        ' lblTitlu -- success / failure of the upload; its colour is set from the theme in code.
        '
        lblTitlu.AutoEllipsis = True
        lblTitlu.Dock = DockStyle.Fill
        lblTitlu.Font = New Font("Segoe UI", 11F, FontStyle.Bold)
        lblTitlu.Location = New Point(22, 14)
        lblTitlu.Margin = New Padding(4, 0, 4, 0)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(1102, 70)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Nota a fost trimisă în FOREXE."
        lblTitlu.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtRaspuns -- FOREXE's answer as the page showed it (read only).
        '
        txtRaspuns.BorderStyle = BorderStyle.FixedSingle
        txtRaspuns.Dock = DockStyle.Fill
        txtRaspuns.Font = New Font("Segoe UI", 9.5F)
        txtRaspuns.Location = New Point(22, 87)
        txtRaspuns.Margin = New Padding(4, 3, 4, 3)
        txtRaspuns.Multiline = True
        txtRaspuns.Name = "txtRaspuns"
        txtRaspuns.ReadOnly = True
        txtRaspuns.ScrollBars = ScrollBars.Vertical
        txtRaspuns.Size = New Size(1102, 408)
        txtRaspuns.TabIndex = 1
        txtRaspuns.TabStop = False
        tips.SetToolTipHeader(txtRaspuns, "Răspunsul FOREXE")
        tips.SetToolTipText(txtRaspuns, "Textul afișat de FOREXE după «Trimite» (sau motivul pentru care încărcarea nu a reușit).")
        '
        ' tlyIndex
        '
        tlyIndex.ColumnCount = 2
        tlyIndex.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 330F))
        tlyIndex.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyIndex.Controls.Add(lblIndexCaption, 0, 0)
        tlyIndex.Controls.Add(txtIndex, 1, 0)
        tlyIndex.Dock = DockStyle.Fill
        tlyIndex.Location = New Point(18, 498)
        tlyIndex.Margin = New Padding(0)
        tlyIndex.Name = "tlyIndex"
        tlyIndex.Padding = New Padding(0, 12, 0, 0)
        tlyIndex.RowCount = 1
        tlyIndex.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyIndex.Size = New Size(1110, 76)
        tlyIndex.TabIndex = 2
        '
        ' lblIndexCaption
        '
        lblIndexCaption.Dock = DockStyle.Fill
        lblIndexCaption.Location = New Point(4, 12)
        lblIndexCaption.Margin = New Padding(4, 0, 4, 0)
        lblIndexCaption.Name = "lblIndexCaption"
        lblIndexCaption.Size = New Size(322, 64)
        lblIndexCaption.TabIndex = 0
        lblIndexCaption.Text = "Index de înregistrare FOREXE *"
        lblIndexCaption.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtIndex
        '
        txtIndex.BackColor = Color.Transparent
        txtIndex.Dock = DockStyle.Fill
        txtIndex.Location = New Point(334, 15)
        txtIndex.Margin = New Padding(4, 3, 4, 3)
        txtIndex.MaxLength = 20
        txtIndex.Name = "txtIndex"
        txtIndex.Size = New Size(772, 58)
        txtIndex.TabIndex = 1
        txtIndex.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtIndex, "Indexul încărcării")
        tips.SetToolTipText(txtIndex, "Cifrele din numărul de înregistrare dat de FOREXE: «INTERNT-1230450081-2026/28-09-2026» → 1230450081." & vbLf & "Se completează singur din răspunsul FOREXE când îl conține; altfel scrieți-l.")
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnDeschide)
        pnlJos.Controls.Add(btnVerifica)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 640)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(18, 10, 18, 20)
        pnlJos.Size = New Size(1146, 76)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(18, 10)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(482, 46)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnDeschide -- opens the downloaded receipt; enabled once there is one.
        '
        btnDeschide.Dock = DockStyle.Right
        btnDeschide.Enabled = False
        btnDeschide.FlatStyle = FlatStyle.Flat
        btnDeschide.Location = New Point(500, 10)
        btnDeschide.Margin = New Padding(0)
        btnDeschide.Name = "btnDeschide"
        btnDeschide.Size = New Size(210, 46)
        btnDeschide.TabIndex = 3
        btnDeschide.Text = "Deschide recipisa"
        btnDeschide.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(btnDeschide, "Recipisa")
        tips.SetToolTipText(btnDeschide, "Deschide fișierul recipisei descărcat pe acest calculator.")
        '
        ' btnVerifica
        '
        btnVerifica.Dock = DockStyle.Right
        btnVerifica.FlatStyle = FlatStyle.Flat
        btnVerifica.Location = New Point(710, 10)
        btnVerifica.Margin = New Padding(0)
        btnVerifica.Name = "btnVerifica"
        btnVerifica.Size = New Size(270, 46)
        btnVerifica.TabIndex = 1
        btnVerifica.Text = "Verifică recipisa"
        btnVerifica.UseVisualStyleBackColor = True
        tips.SetToolTipHeader(btnVerifica, "Verifică recipisa")
        tips.SetToolTipText(btnVerifica, "Caută în FOREXE (mesaje SNM, recipise) mesajul cu indexul de mai sus," & vbLf & "descarcă recipisa și o păstrează pe server, lângă PDF-ul notei." & vbLf & "FOREXE pune recipisa în cutie la câteva minute după încărcare.")
        '
        ' btnInchide
        '
        btnInchide.DialogResult = DialogResult.Cancel
        btnInchide.Dock = DockStyle.Right
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(980, 10)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(148, 46)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
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
        capBar.Size = New Size(1146, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Nota de corecție în CAB"
        '
        ' CabNoteReceiptForm
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New Size(1150, 720)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "CabNoteReceiptForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Nota de corecție în CAB"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlyCorp.PerformLayout()
        tlyIndex.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblTitlu As Label
    Friend WithEvents txtRaspuns As TextBox
    Friend WithEvents tlyIndex As KBotTableLayoutPanel
    Friend WithEvents lblIndexCaption As Label
    Friend WithEvents txtIndex As KBotTextField
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnDeschide As Button
    Friend WithEvents btnVerifica As Button
    Friend WithEvents btnInchide As Button
End Class
