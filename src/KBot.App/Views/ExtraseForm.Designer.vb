Imports KBot.Controls

' The «Extrase de cont» window (slice 0080-03): every bank statement of the database, opened
' modal from the left icon of the main tree's footer. The body is ExtrasePanel in «Toate» mode
' (headers with or without an angajament, operations with CodContract, only CodContract, or
' neither); the footer carries the real download button.
'
' Resizable and maximizable: inherits KBotShellForm, which gives back the 8px resize band lost to
' FormBorderStyle.None; the caption bar shows the maximize button.
'
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at
' 96 dpi and AutoScaleDimensions goes with them: (96, 96) in AutoScaleMode.Dpi (slice 0066-02).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ExtraseForm
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
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        panel = New ExtrasePanel()
        lblEmpty = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        btnDescarca = New Button()
        lblStare = New Label()
        btnInchide = New Button()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(1198, 718)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.binvoice
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1198, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Extrase de cont"
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(panel)
        pnlCard.Controls.Add(lblEmpty)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(8, 6, 8, 6)
        pnlCard.Size = New Size(1198, 622)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        '
        ' panel
        '
        panel.Dock = DockStyle.Fill
        panel.Location = New Point(8, 6)
        panel.Margin = New Padding(0)
        panel.Mode = ExtrasePanelMode.Toate
        panel.Name = "panel"
        panel.ShowDownloadIcon = False
        panel.Size = New Size(1182, 610)
        panel.TabIndex = 0
        '
        ' lblEmpty
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(8, 6)
        lblEmpty.Margin = New Padding(0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(1182, 610)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Se încarcă extrasele…"
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' tlySubsol
        '
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnDescarca, 0, 0)
        tlySubsol.Controls.Add(lblStare, 1, 0)
        tlySubsol.Controls.Add(btnInchide, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 666)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1198, 52)
        tlySubsol.TabIndex = 2
        '
        ' btnDescarca
        '
        btnDescarca.AutoSize = True
        btnDescarca.FlatStyle = FlatStyle.Flat
        btnDescarca.Location = New Point(8, 6)
        btnDescarca.Margin = New Padding(0)
        btnDescarca.Name = "btnDescarca"
        btnDescarca.Padding = New Padding(8, 3, 8, 3)
        btnDescarca.Size = New Size(210, 40)
        btnDescarca.TabIndex = 0
        btnDescarca.Text = "Descarcă extrasele din FOREXE"
        tips.SetToolTipHeader(btnDescarca, "Descarcă extrasele")
        tips.SetToolTipText(btnDescarca, "Descarcă extrasele de cont (SNM) noi din FOREXE și le importă." & vbLf & "Se conectează întâi, dacă nu există sesiune; lista se reîncarcă după import.")
        btnDescarca.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(226, 6)
        lblStare.Margin = New Padding(8, 0, 8, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(826, 40)
        lblStare.TabIndex = 1
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnInchide
        '
        btnInchide.AutoSize = True
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(1060, 6)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Padding = New Padding(17, 3, 17, 3)
        btnInchide.Size = New Size(130, 40)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' ExtraseForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1200, 720)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New Size(820, 480)
        Name = "ExtraseForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Extrase de cont"
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents panel As ExtrasePanel
    Friend WithEvents lblEmpty As Label
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnDescarca As Button
    Friend WithEvents lblStare As Label
    Friend WithEvents btnInchide As Button
End Class
