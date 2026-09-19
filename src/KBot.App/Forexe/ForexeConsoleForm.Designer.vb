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
        btnRecorder = New Button()
        pnlCard = New Panel()
        pnlBody = New Panel()
        rtbLog = New RichTextBox()
        pnlCaption = New Panel()
        capBar = New KBotCaptionBar()
        pnlFoot = New Panel()
        tlpFoot = New KBotTableLayoutPanel()
        pnlStare = New Panel()
        lblStatus = New Label()
        lblCert = New Label()
        pbProgress = New KBotProgressBar()
        pnlCard.SuspendLayout()
        pnlBody.SuspendLayout()
        pnlCaption.SuspendLayout()
        pnlFoot.SuspendLayout()
        tlpFoot.SuspendLayout()
        pnlStare.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnAnulare
        ' 
        btnAnulare.Dock = DockStyle.Fill
        btnAnulare.Enabled = False
        btnAnulare.FlatStyle = FlatStyle.Flat
        btnAnulare.Font = New Font("Calibri", 10F)
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
        btnAfiseazaBrowser.Font = New Font("Calibri", 10F)
        btnAfiseazaBrowser.Image = My.Resources.Resources.Sekkyumu_Developpers_Web_Browser_32
        btnAfiseazaBrowser.Location = New Point(774, 5)
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
        btnAfiseazaLog.Font = New Font("Calibri", 10F)
        btnAfiseazaLog.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Livejournal_32
        btnAfiseazaLog.Location = New Point(714, 5)
        btnAfiseazaLog.Margin = New Padding(4, 5, 4, 5)
        btnAfiseazaLog.Name = "btnAfiseazaLog"
        btnAfiseazaLog.Size = New Size(52, 49)
        btnAfiseazaLog.TabIndex = 2
        tips.SetToolTipHeader(btnAfiseazaLog, "Jurnal")
        tips.SetToolTipText(btnAfiseazaLog, "Deschide jurnalul lucrării: pașii executați și erorile întâlnite.")
        btnAfiseazaLog.UseVisualStyleBackColor = True
        ' 
        ' btnRecorder
        ' 
        btnRecorder.Dock = DockStyle.Fill
        btnRecorder.FlatStyle = FlatStyle.Flat
        btnRecorder.Font = New Font("Calibri", 10F)
        btnRecorder.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Record_slide_show_32
        btnRecorder.Location = New Point(834, 5)
        btnRecorder.Margin = New Padding(4, 5, 4, 5)
        btnRecorder.Name = "btnRecorder"
        btnRecorder.Size = New Size(52, 49)
        btnRecorder.TabIndex = 3
        tips.SetToolTipFooter(btnRecorder, "FOREXE RECORDER")
        tips.SetToolTipHeader(btnRecorder, "Înregistrează acțiunile din FOREXE")
        btnRecorder.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlCard.Controls.Add(pnlBody)
        pnlCard.Controls.Add(pnlCaption)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(pnlStare)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 4)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(910, 659)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' pnlBody
        ' 
        pnlBody.Controls.Add(rtbLog)
        pnlBody.Dock = DockStyle.Fill
        pnlBody.Location = New Point(0, 49)
        pnlBody.Margin = New Padding(0)
        pnlBody.Name = "pnlBody"
        pnlBody.Padding = New Padding(10)
        pnlBody.Size = New Size(910, 424)
        pnlBody.TabIndex = 6
        ' 
        ' rtbLog
        ' 
        rtbLog.BackColor = SystemColors.Window
        rtbLog.BorderStyle = BorderStyle.None
        rtbLog.Dock = DockStyle.Fill
        rtbLog.Font = New Font("Consolas", 9F)
        rtbLog.Location = New Point(10, 10)
        rtbLog.Margin = New Padding(0)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbLog.Size = New Size(890, 404)
        rtbLog.TabIndex = 0
        rtbLog.Text = ""
        rtbLog.WordWrap = False
        ' 
        ' pnlCaption
        ' 
        pnlCaption.Controls.Add(capBar)
        pnlCaption.Dock = DockStyle.Top
        pnlCaption.Location = New Point(0, 0)
        pnlCaption.Margin = New Padding(0)
        pnlCaption.Name = "pnlCaption"
        pnlCaption.Size = New Size(910, 49)
        pnlCaption.TabIndex = 5
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(910, 49)
        capBar.TabIndex = 4
        capBar.TabStop = False
        capBar.Text = "Consolă FOREXE"
        ' 
        ' pnlFoot
        ' 
        pnlFoot.Controls.Add(tlpFoot)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 473)
        pnlFoot.Margin = New Padding(0)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Padding = New Padding(10, 0, 10, 0)
        pnlFoot.Size = New Size(910, 59)
        pnlFoot.TabIndex = 2
        ' 
        ' tlpFoot
        ' 
        tlpFoot.ColumnCount = 5
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpFoot.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 60F))
        tlpFoot.Controls.Add(btnRecorder, 4, 0)
        tlpFoot.Controls.Add(btnAnulare, 0, 0)
        tlpFoot.Controls.Add(btnAfiseazaLog, 2, 0)
        tlpFoot.Controls.Add(btnAfiseazaBrowser, 3, 0)
        tlpFoot.Dock = DockStyle.Fill
        tlpFoot.Location = New Point(10, 0)
        tlpFoot.Margin = New Padding(0)
        tlpFoot.Name = "tlpFoot"
        tlpFoot.RowCount = 1
        tlpFoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpFoot.Size = New Size(890, 59)
        tlpFoot.TabIndex = 3
        ' 
        ' pnlStare
        ' 
        pnlStare.Controls.Add(lblStatus)
        pnlStare.Controls.Add(lblCert)
        pnlStare.Controls.Add(pbProgress)
        pnlStare.Dock = DockStyle.Bottom
        pnlStare.Location = New Point(0, 532)
        pnlStare.Margin = New Padding(4, 5, 4, 5)
        pnlStare.Name = "pnlStare"
        pnlStare.Padding = New Padding(10)
        pnlStare.Size = New Size(910, 127)
        pnlStare.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(10, 40)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(890, 40)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Neconectat."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCert
        ' 
        lblCert.Dock = DockStyle.Bottom
        lblCert.Location = New Point(10, 80)
        lblCert.Margin = New Padding(4, 0, 4, 0)
        lblCert.Name = "lblCert"
        lblCert.Size = New Size(890, 37)
        lblCert.TabIndex = 1
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = DockStyle.Top
        pbProgress.Location = New Point(10, 10)
        pbProgress.Margin = New Padding(4, 5, 4, 5)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(890, 30)
        pbProgress.TabIndex = 0
        ' 
        ' ForexeConsoleForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(914, 667)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(914, 667)
        Name = "ForexeConsoleForm"
        Padding = New Padding(2, 4, 2, 4)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Consolă FOREXE"
        pnlCard.ResumeLayout(False)
        pnlBody.ResumeLayout(False)
        pnlCaption.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        tlpFoot.ResumeLayout(False)
        pnlStare.ResumeLayout(False)
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
    Friend WithEvents tlpFoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnRecorder As Button
    Friend WithEvents pnlBody As Panel
    Friend WithEvents pnlCaption As Panel
End Class
