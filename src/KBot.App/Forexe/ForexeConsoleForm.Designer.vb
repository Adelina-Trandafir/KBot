<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForexeConsoleForm
    Inherits KBot.Theming.KBotShellForm

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
        pnlCard = New Panel()
        rtbLog = New RichTextBox()
        pnlFoot = New Panel()
        btnAnulare = New Button()
        btnAfiseazaBrowser = New Button()
        btnAfiseazaLog = New Button()
        pnlStare = New Panel()
        lblStatus = New Label()
        lblCert = New Label()
        pbProgress = New ProgressBar()
        capBar = New KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        pnlStare.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard — cardul rădăcină. Copiii se adaugă în ordine INVERSĂ de dock:
        ' rtbLog (Fill) primul, apoi pnlFoot (Bottom), pnlStare (Bottom), capBar (Top).
        '
        pnlCard.Controls.Add(rtbLog)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(pnlStare)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(898, 596)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(898, 40)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "Consolă FOREXE"
        '
        ' rtbLog — jurnalul complet al robotului (ținta RichTextBoxLogger).
        '
        rtbLog.BorderStyle = BorderStyle.None
        rtbLog.Dock = DockStyle.Fill
        rtbLog.Font = New Font("Consolas", 9F)
        rtbLog.Location = New Point(0, 40)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbLog.Size = New Size(898, 428)
        rtbLog.TabIndex = 0
        rtbLog.Text = ""
        rtbLog.WordWrap = False
        '
        ' pnlStare — progresul, certificatul și ultima linie de stare.
        '
        pnlStare.Controls.Add(lblStatus)
        pnlStare.Controls.Add(lblCert)
        pnlStare.Controls.Add(pbProgress)
        pnlStare.Dock = DockStyle.Bottom
        pnlStare.Location = New Point(0, 468)
        pnlStare.Name = "pnlStare"
        pnlStare.Padding = New Padding(12, 6, 12, 6)
        pnlStare.Size = New Size(898, 76)
        pnlStare.TabIndex = 1
        '
        ' pbProgress
        '
        pbProgress.Dock = DockStyle.Top
        pbProgress.Location = New Point(12, 6)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(874, 18)
        pbProgress.TabIndex = 0
        '
        ' lblCert
        '
        lblCert.Dock = DockStyle.Bottom
        lblCert.Location = New Point(12, 24)
        lblCert.Name = "lblCert"
        lblCert.Size = New Size(874, 22)
        lblCert.TabIndex = 1
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lblStatus
        '
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(12, 46)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(874, 24)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Neconectat."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        ' pnlFoot — butoanele.
        '
        pnlFoot.Controls.Add(btnAfiseazaLog)
        pnlFoot.Controls.Add(btnAfiseazaBrowser)
        pnlFoot.Controls.Add(btnAnulare)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 544)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Padding = New Padding(12, 8, 12, 8)
        pnlFoot.Size = New Size(898, 52)
        pnlFoot.TabIndex = 2
        '
        ' btnAnulare
        '
        btnAnulare.Dock = DockStyle.Left
        btnAnulare.Enabled = False
        btnAnulare.FlatStyle = FlatStyle.Flat
        btnAnulare.Location = New Point(12, 8)
        btnAnulare.Name = "btnAnulare"
        btnAnulare.Size = New Size(140, 36)
        btnAnulare.TabIndex = 0
        btnAnulare.Text = "Anulează"
        btnAnulare.UseVisualStyleBackColor = True
        '
        ' btnAfiseazaBrowser
        '
        btnAfiseazaBrowser.Dock = DockStyle.Right
        btnAfiseazaBrowser.FlatStyle = FlatStyle.Flat
        btnAfiseazaBrowser.Location = New Point(586, 8)
        btnAfiseazaBrowser.Name = "btnAfiseazaBrowser"
        btnAfiseazaBrowser.Size = New Size(160, 36)
        btnAfiseazaBrowser.TabIndex = 1
        btnAfiseazaBrowser.Text = "Arată browserul"
        btnAfiseazaBrowser.UseVisualStyleBackColor = True
        '
        ' btnAfiseazaLog
        '
        btnAfiseazaLog.Dock = DockStyle.Right
        btnAfiseazaLog.FlatStyle = FlatStyle.Flat
        btnAfiseazaLog.Location = New Point(746, 8)
        btnAfiseazaLog.Name = "btnAfiseazaLog"
        btnAfiseazaLog.Size = New Size(140, 36)
        btnAfiseazaLog.TabIndex = 2
        btnAfiseazaLog.Text = "Deschide jurnalul"
        btnAfiseazaLog.UseVisualStyleBackColor = True
        '
        ' ForexeConsoleForm
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(900, 600)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(640, 400)
        Name = "ForexeConsoleForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Consolă FOREXE"
        pnlCard.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        pnlStare.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents rtbLog As RichTextBox
    Friend WithEvents pnlStare As Panel
    Friend WithEvents pbProgress As ProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnAnulare As Button
    Friend WithEvents btnAfiseazaBrowser As Button
    Friend WithEvents btnAfiseazaLog As Button
End Class
