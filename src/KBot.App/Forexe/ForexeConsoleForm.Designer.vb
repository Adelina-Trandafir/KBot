Imports KBot.Controls

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
        tips = New KBotToolTip(components)
        btnAnulare = New Button()
        btnAfiseazaBrowser = New Button()
        btnAfiseazaLog = New Button()
        pnlCard = New Panel()
        rtbLog = New RichTextBox()
        pnlFoot = New Panel()
        pnlStare = New Panel()
        lblStatus = New Label()
        lblCert = New Label()
        pbProgress = New KBotProgressBar()
        capBar = New KBotCaptionBar()
        tlpFoot = New TableLayoutPanel()
        btnRecorder = New Button()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        pnlStare.SuspendLayout()
        tlpFoot.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnAnulare
        ' 
        btnAnulare.Dock = DockStyle.Fill
        btnAnulare.Enabled = False
        btnAnulare.FlatStyle = FlatStyle.Flat
        btnAnulare.Font = New Font("Calibri", 10.0F)
        btnAnulare.Image = My.Resources.Resources.Sekkyumu_Developpers_Stop_32
        btnAnulare.Location = New Point(4, 5)
        btnAnulare.Margin = New Padding(4, 5, 4, 5)
        btnAnulare.Name = "btnAnulare"
        btnAnulare.RightToLeft = RightToLeft.No
        btnAnulare.Size = New Size(52, 49)
        btnAnulare.TabIndex = 0
        tips.SetToolTipHeader(btnAnulare, "Anulează")
        tips.SetToolTipText(btnAnulare, "Oprește lucrarea FOREXE în curs." & vbLf & "Ce s-a descărcat deja rămâne pe disc.")
        btnAnulare.UseVisualStyleBackColor = True
        ' 
        ' btnAfiseazaBrowser
        ' 
        btnAfiseazaBrowser.Dock = DockStyle.Fill
        btnAfiseazaBrowser.FlatStyle = FlatStyle.Flat
        btnAfiseazaBrowser.Font = New Font("Calibri", 10.0F)
        btnAfiseazaBrowser.Image = My.Resources.Resources.Sekkyumu_Developpers_Web_Browser_32
        btnAfiseazaBrowser.Location = New Point(762, 5)
        btnAfiseazaBrowser.Margin = New Padding(4, 5, 4, 5)
        btnAfiseazaBrowser.Name = "btnAfiseazaBrowser"
        btnAfiseazaBrowser.Size = New Size(52, 49)
        btnAfiseazaBrowser.TabIndex = 1
        tips.SetToolTipHeader(btnAfiseazaBrowser, "Arată browserul")
        tips.SetToolTipText(btnAfiseazaBrowser, "Aduce în față fereastra de browser prin care lucrează robotul FOREXE." & vbLf & "Folosește-o când portalul cere o confirmare.")
        btnAfiseazaBrowser.UseVisualStyleBackColor = True
        ' 
        ' btnAfiseazaLog
        ' 
        btnAfiseazaLog.Dock = DockStyle.Fill
        btnAfiseazaLog.FlatStyle = FlatStyle.Flat
        btnAfiseazaLog.Font = New Font("Calibri", 10.0F)
        btnAfiseazaLog.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Livejournal_32
        btnAfiseazaLog.Location = New Point(702, 5)
        btnAfiseazaLog.Margin = New Padding(4, 5, 4, 5)
        btnAfiseazaLog.Name = "btnAfiseazaLog"
        btnAfiseazaLog.Size = New Size(52, 49)
        btnAfiseazaLog.TabIndex = 2
        tips.SetToolTipHeader(btnAfiseazaLog, "Jurnal")
        tips.SetToolTipText(btnAfiseazaLog, "Deschide jurnalul lucrării: pașii executați și erorile întâlnite.")
        btnAfiseazaLog.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlCard.Controls.Add(rtbLog)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(pnlStare)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(912, 661)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' rtbLog
        ' 
        rtbLog.BorderStyle = BorderStyle.None
        rtbLog.Dock = DockStyle.Fill
        rtbLog.Font = New Font("Consolas", 9.0F)
        rtbLog.Location = New Point(0, 57)
        rtbLog.Margin = New Padding(0)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbLog.Size = New Size(912, 418)
        rtbLog.TabIndex = 0
        rtbLog.Text = ""
        rtbLog.WordWrap = False
        ' 
        ' pnlFoot
        ' 
        pnlFoot.Controls.Add(tlpFoot)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 475)
        pnlFoot.Margin = New Padding(0)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Padding = New Padding(17, 0, 17, 0)
        pnlFoot.Size = New Size(912, 59)
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
        ' tlpFoot
        ' 
        tlpFoot.ColumnCount = 5
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60.0F))
        tlpFoot.Controls.Add(btnRecorder, 4, 0)
        tlpFoot.Controls.Add(btnAnulare, 0, 0)
        tlpFoot.Controls.Add(btnAfiseazaLog, 2, 0)
        tlpFoot.Controls.Add(btnAfiseazaBrowser, 3, 0)
        tlpFoot.Dock = DockStyle.Fill
        tlpFoot.Location = New Point(17, 0)
        tlpFoot.Margin = New Padding(0)
        tlpFoot.Name = "tlpFoot"
        tlpFoot.RowCount = 1
        tlpFoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpFoot.Size = New Size(878, 59)
        tlpFoot.TabIndex = 3
        ' 
        ' btnRecorder
        ' 
        btnRecorder.Dock = DockStyle.Fill
        btnRecorder.FlatStyle = FlatStyle.Flat
        btnRecorder.Font = New Font("Calibri", 10.0F)
        btnRecorder.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Record_slide_show_32
        btnRecorder.Location = New Point(822, 5)
        btnRecorder.Margin = New Padding(4, 5, 4, 5)
        btnRecorder.Name = "btnRecorder"
        btnRecorder.Size = New Size(52, 49)
        btnRecorder.TabIndex = 3
        tips.SetToolTipFooter(btnRecorder, "FOREXE RECORDER")
        tips.SetToolTipHeader(btnRecorder, "Înregistrează acțiunile din FOREXE")
        btnRecorder.UseVisualStyleBackColor = True
        ' 
        ' ForexeConsoleForm
        ' 
        AutoScaleDimensions = New SizeF(9.0F, 22.0F)
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
        tlpFoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents rtbLog As RichTextBox
    Friend WithEvents pnlStare As Panel
    Friend WithEvents pbProgress As KBotProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnAnulare As Button
    Friend WithEvents btnAfiseazaBrowser As Button
    Friend WithEvents btnAfiseazaLog As Button
    Friend WithEvents tlpFoot As TableLayoutPanel
    Friend WithEvents btnRecorder As Button
End Class
