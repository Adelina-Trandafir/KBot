<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForexeConsoleForm
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
        tips = New Global.KBot.Controls.KBotToolTip(components)
        btnAnulare = New Button()
        btnAfiseazaBrowser = New Button()
        btnAfiseazaLog = New Button()
        pnlCard = New Panel()
        rtbLog = New RichTextBox()
        pnlFoot = New Panel()
        pnlStare = New Panel()
        lblStatus = New Label()
        lblCert = New Label()
        pbProgress = New Controls.KBotProgressBar()
        capBar = New Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        pnlStare.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnAnulare
        ' 
        btnAnulare.Dock = DockStyle.Left
        btnAnulare.Enabled = False
        btnAnulare.FlatStyle = FlatStyle.Flat
        btnAnulare.Location = New Point(17, 13)
        btnAnulare.Margin = New Padding(4, 5, 4, 5)
        btnAnulare.Name = "btnAnulare"
        btnAnulare.Size = New Size(200, 51)
        btnAnulare.TabIndex = 0
        btnAnulare.Text = "Anulează"
        tips.SetToolTipHeader(btnAnulare, "Anulează")
        tips.SetToolTipText(btnAnulare, "Oprește lucrarea FOREXE în curs." & vbLf & "Ce s-a descărcat deja rămâne pe disc.")
        btnAnulare.UseVisualStyleBackColor = True
        ' 
        ' btnAfiseazaBrowser
        ' 
        btnAfiseazaBrowser.Dock = DockStyle.Right
        btnAfiseazaBrowser.FlatStyle = FlatStyle.Flat
        btnAfiseazaBrowser.Location = New Point(666, 13)
        btnAfiseazaBrowser.Margin = New Padding(4, 5, 4, 5)
        btnAfiseazaBrowser.Name = "btnAfiseazaBrowser"
        btnAfiseazaBrowser.Size = New Size(229, 51)
        btnAfiseazaBrowser.TabIndex = 1
        btnAfiseazaBrowser.Text = "Arată browserul"
        tips.SetToolTipHeader(btnAfiseazaBrowser, "Arată browserul")
        tips.SetToolTipText(btnAfiseazaBrowser, "Aduce în față fereastra de browser prin care lucrează robotul FOREXE." & vbLf & "Folosește-o când portalul cere o confirmare.")
        btnAfiseazaBrowser.UseVisualStyleBackColor = True
        ' 
        ' btnAfiseazaLog
        ' 
        btnAfiseazaLog.Dock = DockStyle.Right
        btnAfiseazaLog.FlatStyle = FlatStyle.Flat
        btnAfiseazaLog.Location = New Point(466, 13)
        btnAfiseazaLog.Margin = New Padding(4, 5, 4, 5)
        btnAfiseazaLog.Name = "btnAfiseazaLog"
        btnAfiseazaLog.Size = New Size(200, 51)
        btnAfiseazaLog.TabIndex = 2
        btnAfiseazaLog.Text = "Deschide jurnalul"
        tips.SetToolTipHeader(btnAfiseazaLog, "Jurnal")
        tips.SetToolTipText(btnAfiseazaLog, "Deschide jurnalul lucrării: pașii executați și erorile întâlnite.")
        btnAfiseazaLog.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(rtbLog)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(pnlStare)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Margin = New Padding(4, 5, 4, 5)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(912, 661)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' rtbLog
        ' 
        rtbLog.BorderStyle = BorderStyle.None
        rtbLog.Dock = DockStyle.Fill
        rtbLog.Font = New Font("Consolas", 9F)
        rtbLog.Location = New Point(0, 57)
        rtbLog.Margin = New Padding(4, 5, 4, 5)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbLog.Size = New Size(912, 400)
        rtbLog.TabIndex = 0
        rtbLog.Text = ""
        rtbLog.WordWrap = False
        ' 
        ' pnlFoot
        ' 
        pnlFoot.Controls.Add(btnAfiseazaLog)
        pnlFoot.Controls.Add(btnAfiseazaBrowser)
        pnlFoot.Controls.Add(btnAnulare)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 457)
        pnlFoot.Margin = New Padding(4, 5, 4, 5)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Padding = New Padding(17, 13, 17, 13)
        pnlFoot.Size = New Size(912, 77)
        pnlFoot.TabIndex = 2
        ' 
        ' pnlStare
        ' 
        pnlStare.Controls.Add(lblStatus)
        pnlStare.Controls.Add(lblCert)
        pnlStare.Controls.Add(pbProgress)
        pnlStare.Dock = DockStyle.Bottom
        pnlStare.Location = New Point(0, 534)
        pnlStare.Margin = New Padding(4, 5, 4, 5)
        pnlStare.Name = "pnlStare"
        pnlStare.Padding = New Padding(17, 10, 17, 10)
        pnlStare.Size = New Size(912, 127)
        pnlStare.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(17, 40)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(878, 40)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Neconectat."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCert
        ' 
        lblCert.Dock = DockStyle.Bottom
        lblCert.Location = New Point(17, 80)
        lblCert.Margin = New Padding(4, 0, 4, 0)
        lblCert.Name = "lblCert"
        lblCert.Size = New Size(878, 37)
        lblCert.TabIndex = 1
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = DockStyle.Top
        pbProgress.Location = New Point(17, 10)
        pbProgress.Margin = New Padding(4, 5, 4, 5)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(878, 30)
        pbProgress.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(912, 57)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "Consolă FOREXE"
        ' 
        ' ForexeConsoleForm
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(914, 667)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(914, 667)
        Name = "ForexeConsoleForm"
        Padding = New Padding(1, 3, 1, 3)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Consolă FOREXE"
        pnlCard.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        pnlStare.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents rtbLog As RichTextBox
    Friend WithEvents pnlStare As Panel
    Friend WithEvents pbProgress As Global.KBot.Controls.KBotProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnAnulare As Button
    Friend WithEvents btnAfiseazaBrowser As Button
    Friend WithEvents btnAfiseazaLog As Button
End Class
