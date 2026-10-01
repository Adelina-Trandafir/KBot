Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HelpCapturePromptForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        pnlRoot = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblImagine = New Label()
        lblPregatire = New Label()
        lblNota = New Label()
        tlyButoane = New KBotTableLayoutPanel()
        btnCaptureaza = New Button()
        btnRenunta = New Button()
        capBar = New KBotCaptionBar()
        pnlRoot.SuspendLayout()
        tlyCorp.SuspendLayout()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        '
        ' pnlRoot
        '
        pnlRoot.Controls.Add(tlyCorp)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 1)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(478, 238)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        '
        ' tlyCorp
        '
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblImagine, 0, 0)
        tlyCorp.Controls.Add(lblPregatire, 0, 1)
        tlyCorp.Controls.Add(lblNota, 0, 2)
        tlyCorp.Controls.Add(tlyButoane, 0, 3)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 34)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(12, 8, 12, 10)
        tlyCorp.RowCount = 4
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle())
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyCorp.Size = New Size(478, 204)
        tlyCorp.TabIndex = 1
        '
        ' lblImagine
        '
        lblImagine.AutoSize = True
        lblImagine.Dock = DockStyle.Fill
        lblImagine.Font = New Font("Segoe UI Semibold", 10.5F)
        lblImagine.Margin = New Padding(0, 0, 0, 6)
        lblImagine.Name = "lblImagine"
        lblImagine.TabIndex = 0
        '
        ' lblPregatire
        '
        lblPregatire.Dock = DockStyle.Fill
        lblPregatire.Margin = New Padding(0)
        lblPregatire.Name = "lblPregatire"
        lblPregatire.TabIndex = 1
        '
        ' lblNota
        '
        lblNota.AutoSize = True
        lblNota.Dock = DockStyle.Fill
        lblNota.Margin = New Padding(0, 6, 0, 0)
        lblNota.Name = "lblNota"
        lblNota.TabIndex = 2
        '
        ' tlyButoane
        '
        tlyButoane.ColumnCount = 3
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyButoane.Controls.Add(btnCaptureaza, 1, 0)
        tlyButoane.Controls.Add(btnRenunta, 2, 0)
        tlyButoane.Dock = DockStyle.Fill
        tlyButoane.Margin = New Padding(0, 8, 0, 0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButoane.TabIndex = 3
        '
        ' btnCaptureaza
        '
        btnCaptureaza.Dock = DockStyle.Fill
        btnCaptureaza.FlatStyle = FlatStyle.Flat
        btnCaptureaza.Margin = New Padding(0, 0, 6, 0)
        btnCaptureaza.Name = "btnCaptureaza"
        btnCaptureaza.TabIndex = 0
        btnCaptureaza.Text = "Capturează"
        tips.SetToolTipHeader(btnCaptureaza, "Capturează")
        tips.SetToolTipText(btnCaptureaza, "Scurtătură: Ctrl + ` (nu închide meniurile deschise)." & vbLf & "Îngheață ecranul. Apoi trage un dreptunghi, fă clic pe o fereastră" & vbLf & "sau Ctrl + clic pe un control. Esc renunță.")
        btnCaptureaza.UseVisualStyleBackColor = True
        '
        ' btnRenunta
        '
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowHelpButton = False
        capBar.Size = New Size(478, 34)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Pregătește captura"
        '
        ' HelpCapturePromptForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(480, 240)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        Name = "HelpCapturePromptForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Pregătește captura"
        TopMost = True
        pnlRoot.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlyCorp.PerformLayout()
        tlyButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblImagine As Label
    Friend WithEvents lblPregatire As Label
    Friend WithEvents lblNota As Label
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnCaptureaza As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents capBar As KBotCaptionBar
End Class
