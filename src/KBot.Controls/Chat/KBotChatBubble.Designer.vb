Imports KBot.Theming

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotChatBubble
    Inherits Global.KBot.Theming.KBotThemedUserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                _fontBold?.Dispose()
                _fontSmall?.Dispose()
                _fontGap?.Dispose()
                _fontLink?.Dispose()
                If components IsNot Nothing Then components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        pnlInner = New Panel()
        pnlLog = New Panel()
        edLog = New KBotRichTextEditor()
        lblStatus = New Label()
        lblOlder = New Label()
        pnlComposer = New Panel()
        pnlSend = New Panel()
        btnSend = New KBotButton()
        lblHint = New Label()
        txtMessage = New KBotTextBox()
        txtName = New KBotTextBox()
        pnlIdentity = New Panel()
        lblPseudonym = New Label()
        chkAnonymous = New KBotCheckBox()
        pnlFilter = New Panel()
        lblFilter = New Label()
        chkVercon = New KBotCheckBox()
        chkAdechit = New KBotCheckBox()
        chkKbot = New KBotCheckBox()
        pnlReply = New Panel()
        lblReply = New Label()
        btnReplyCancel = New KBotButton()
        pnlHeader = New Panel()
        lblTitle = New Label()
        btnClose = New KBotButton()
        pnlInner.SuspendLayout()
        pnlLog.SuspendLayout()
        pnlComposer.SuspendLayout()
        pnlSend.SuspendLayout()
        pnlIdentity.SuspendLayout()
        pnlFilter.SuspendLayout()
        pnlReply.SuspendLayout()
        pnlHeader.SuspendLayout()
        SuspendLayout()
        '
        ' pnlInner
        '
        pnlInner.Controls.Add(pnlLog)
        pnlInner.Controls.Add(pnlComposer)
        pnlInner.Controls.Add(pnlFilter)
        pnlInner.Controls.Add(pnlHeader)
        pnlInner.Location = New Point(10, 10)
        pnlInner.Margin = New Padding(0)
        pnlInner.Name = "pnlInner"
        pnlInner.Size = New Size(380, 470)
        pnlInner.TabIndex = 0
        '
        ' pnlLog
        '
        pnlLog.Controls.Add(edLog)
        pnlLog.Controls.Add(lblStatus)
        pnlLog.Controls.Add(lblOlder)
        pnlLog.Dock = DockStyle.Fill
        pnlLog.Location = New Point(0, 34)
        pnlLog.Margin = New Padding(0)
        pnlLog.Name = "pnlLog"
        pnlLog.Size = New Size(380, 264)
        pnlLog.TabIndex = 1
        '
        ' edLog
        '
        edLog.Dock = DockStyle.Fill
        edLog.FooterVisible = False
        edLog.HeaderVisible = False
        edLog.Location = New Point(0, 22)
        edLog.Margin = New Padding(0)
        edLog.Name = "edLog"
        edLog.Size = New Size(380, 222)
        edLog.TabIndex = 0
        '
        ' lblStatus
        '
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(0, 244)
        lblStatus.Margin = New Padding(0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(380, 20)
        lblStatus.TabIndex = 1
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        lblStatus.Visible = False
        '
        ' lblOlder
        '
        lblOlder.Cursor = Cursors.Hand
        lblOlder.Dock = DockStyle.Top
        lblOlder.Location = New Point(0, 0)
        lblOlder.Margin = New Padding(0)
        lblOlder.Name = "lblOlder"
        lblOlder.Size = New Size(380, 22)
        lblOlder.TabIndex = 2
        lblOlder.Text = "Arată mesajele mai vechi"
        lblOlder.TextAlign = ContentAlignment.MiddleCenter
        lblOlder.Visible = False
        '
        ' pnlComposer
        '
        pnlComposer.Controls.Add(pnlSend)
        pnlComposer.Controls.Add(txtMessage)
        pnlComposer.Controls.Add(txtName)
        pnlComposer.Controls.Add(pnlIdentity)
        pnlComposer.Controls.Add(pnlReply)
        pnlComposer.Dock = DockStyle.Bottom
        pnlComposer.Location = New Point(0, 298)
        pnlComposer.Margin = New Padding(0)
        pnlComposer.Name = "pnlComposer"
        pnlComposer.Size = New Size(380, 172)
        pnlComposer.TabIndex = 2
        '
        ' pnlSend
        '
        pnlSend.Controls.Add(lblHint)
        pnlSend.Controls.Add(btnSend)
        pnlSend.Dock = DockStyle.Top
        pnlSend.Location = New Point(0, 136)
        pnlSend.Margin = New Padding(0)
        pnlSend.Name = "pnlSend"
        pnlSend.Size = New Size(380, 36)
        pnlSend.TabIndex = 3
        '
        ' btnSend
        '
        btnSend.Dock = DockStyle.Right
        btnSend.Location = New Point(290, 0)
        btnSend.Margin = New Padding(0)
        btnSend.Name = "btnSend"
        btnSend.Primary = True
        btnSend.Size = New Size(90, 36)
        btnSend.TabIndex = 0
        btnSend.Text = "Trimite"
        '
        ' lblHint
        '
        lblHint.Dock = DockStyle.Fill
        lblHint.Location = New Point(0, 0)
        lblHint.Margin = New Padding(0)
        lblHint.Name = "lblHint"
        lblHint.Size = New Size(290, 36)
        lblHint.TabIndex = 1
        lblHint.Text = "Enter trimite · Ctrl+Enter rând nou"
        lblHint.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtMessage
        '
        txtMessage.AcceptsReturn = True
        txtMessage.Dock = DockStyle.Top
        txtMessage.Location = New Point(0, 62)
        txtMessage.Margin = New Padding(0)
        txtMessage.MaxLength = 4000
        txtMessage.Multiline = True
        txtMessage.Name = "txtMessage"
        txtMessage.PlaceholderText = "Scrieți un mesaj…"
        txtMessage.Size = New Size(380, 74)
        txtMessage.TabIndex = 2
        '
        ' txtName
        '
        txtName.Dock = DockStyle.Top
        txtName.Location = New Point(0, 30)
        txtName.Margin = New Padding(0)
        txtName.MaxLength = 80
        txtName.Name = "txtName"
        txtName.PlaceholderText = "Numele afișat la mesajele cu nume"
        txtName.Size = New Size(380, 32)
        txtName.TabIndex = 1
        txtName.Visible = False
        '
        ' pnlIdentity
        '
        pnlIdentity.Controls.Add(lblPseudonym)
        pnlIdentity.Controls.Add(chkAnonymous)
        pnlIdentity.Dock = DockStyle.Top
        pnlIdentity.Location = New Point(0, 0)
        pnlIdentity.Margin = New Padding(0)
        pnlIdentity.Name = "pnlIdentity"
        pnlIdentity.Size = New Size(380, 30)
        pnlIdentity.TabIndex = 0
        '
        ' lblPseudonym
        '
        lblPseudonym.Dock = DockStyle.Fill
        lblPseudonym.Location = New Point(130, 0)
        lblPseudonym.Margin = New Padding(0)
        lblPseudonym.Name = "lblPseudonym"
        lblPseudonym.Size = New Size(250, 30)
        lblPseudonym.TabIndex = 1
        lblPseudonym.TextAlign = ContentAlignment.MiddleLeft
        '
        ' chkAnonymous
        '
        chkAnonymous.Checked = True
        chkAnonymous.CheckState = CheckState.Checked
        chkAnonymous.Dock = DockStyle.Left
        chkAnonymous.Location = New Point(0, 0)
        chkAnonymous.Margin = New Padding(0)
        chkAnonymous.Name = "chkAnonymous"
        chkAnonymous.Size = New Size(130, 30)
        chkAnonymous.TabIndex = 0
        chkAnonymous.Text = "Cu pseudonim"
        '
        ' pnlFilter
        '
        pnlFilter.Controls.Add(chkKbot)
        pnlFilter.Controls.Add(chkAdechit)
        pnlFilter.Controls.Add(chkVercon)
        pnlFilter.Controls.Add(lblFilter)
        pnlFilter.Dock = DockStyle.Top
        pnlFilter.Location = New Point(0, 34)
        pnlFilter.Margin = New Padding(0)
        pnlFilter.Name = "pnlFilter"
        pnlFilter.Size = New Size(380, 30)
        pnlFilter.TabIndex = 3
        '
        ' lblFilter
        '
        lblFilter.Dock = DockStyle.Left
        lblFilter.Location = New Point(0, 0)
        lblFilter.Margin = New Padding(0)
        lblFilter.Name = "lblFilter"
        lblFilter.Size = New Size(66, 30)
        lblFilter.TabIndex = 0
        lblFilter.Text = "Afișează:"
        lblFilter.TextAlign = ContentAlignment.MiddleLeft
        '
        ' chkVercon
        '
        chkVercon.Checked = True
        chkVercon.CheckState = CheckState.Checked
        chkVercon.Dock = DockStyle.Left
        chkVercon.Location = New Point(66, 0)
        chkVercon.Margin = New Padding(0)
        chkVercon.Name = "chkVercon"
        chkVercon.Size = New Size(96, 30)
        chkVercon.TabIndex = 1
        chkVercon.Tag = "V"
        chkVercon.Text = "VERCON"
        '
        ' chkAdechit
        '
        chkAdechit.Checked = True
        chkAdechit.CheckState = CheckState.Checked
        chkAdechit.Dock = DockStyle.Left
        chkAdechit.Location = New Point(162, 0)
        chkAdechit.Margin = New Padding(0)
        chkAdechit.Name = "chkAdechit"
        chkAdechit.Size = New Size(100, 30)
        chkAdechit.TabIndex = 2
        chkAdechit.Tag = "A"
        chkAdechit.Text = "ADECHIT"
        '
        ' chkKbot
        '
        chkKbot.Checked = True
        chkKbot.CheckState = CheckState.Checked
        chkKbot.Dock = DockStyle.Left
        chkKbot.Location = New Point(262, 0)
        chkKbot.Margin = New Padding(0)
        chkKbot.Name = "chkKbot"
        chkKbot.Size = New Size(84, 30)
        chkKbot.TabIndex = 3
        chkKbot.Tag = "K"
        chkKbot.Text = "K-BOT"
        '
        ' pnlReply
        '
        pnlReply.Controls.Add(lblReply)
        pnlReply.Controls.Add(btnReplyCancel)
        pnlReply.Dock = DockStyle.Top
        pnlReply.Location = New Point(0, 0)
        pnlReply.Margin = New Padding(0)
        pnlReply.Name = "pnlReply"
        pnlReply.Size = New Size(380, 28)
        pnlReply.TabIndex = 4
        pnlReply.Visible = False
        '
        ' lblReply
        '
        lblReply.AutoEllipsis = True
        lblReply.Dock = DockStyle.Fill
        lblReply.Location = New Point(0, 0)
        lblReply.Margin = New Padding(0)
        lblReply.Name = "lblReply"
        lblReply.Size = New Size(352, 28)
        lblReply.TabIndex = 0
        lblReply.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnReplyCancel
        '
        btnReplyCancel.Dock = DockStyle.Right
        btnReplyCancel.Location = New Point(352, 0)
        btnReplyCancel.Margin = New Padding(0)
        btnReplyCancel.Name = "btnReplyCancel"
        btnReplyCancel.Size = New Size(28, 28)
        btnReplyCancel.TabIndex = 1
        btnReplyCancel.Text = "×"
        tips.SetToolTipHeader(btnReplyCancel, "Renunță la răspuns")
        tips.SetToolTipText(btnReplyCancel, "Mesajul se trimite ca mesaj obișnuit, nu ca răspuns.")
        '
        ' pnlHeader
        '
        pnlHeader.Controls.Add(lblTitle)
        pnlHeader.Controls.Add(btnClose)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Margin = New Padding(0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Size = New Size(380, 34)
        pnlHeader.TabIndex = 0
        '
        ' lblTitle
        '
        lblTitle.Dock = DockStyle.Fill
        lblTitle.Font = New Font("Segoe UI Semibold", 11.0F)
        lblTitle.Location = New Point(0, 0)
        lblTitle.Margin = New Padding(0)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(346, 34)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Discuții"
        lblTitle.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnClose
        '
        btnClose.Dock = DockStyle.Right
        btnClose.Location = New Point(346, 0)
        btnClose.Margin = New Padding(0)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(34, 34)
        btnClose.TabIndex = 1
        btnClose.Text = "×"
        tips.SetToolTipHeader(btnClose, "Închide")
        tips.SetToolTipText(btnClose, "Închide fereastra cu mesaje. Ciorna rămâne.")
        '
        ' KBotChatBubble
        '
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(pnlInner)
        Name = "KBotChatBubble"
        Size = New Size(400, 506)
        pnlInner.ResumeLayout(False)
        pnlLog.ResumeLayout(False)
        pnlComposer.ResumeLayout(False)
        pnlSend.ResumeLayout(False)
        pnlIdentity.ResumeLayout(False)
        pnlFilter.ResumeLayout(False)
        pnlReply.ResumeLayout(False)
        pnlHeader.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlInner As Panel
    Friend WithEvents pnlLog As Panel
    Friend WithEvents edLog As KBotRichTextEditor
    Friend WithEvents lblStatus As Label
    Friend WithEvents lblOlder As Label
    Friend WithEvents pnlComposer As Panel
    Friend WithEvents pnlSend As Panel
    Friend WithEvents btnSend As KBotButton
    Friend WithEvents lblHint As Label
    Friend WithEvents txtMessage As KBotTextBox
    Friend WithEvents txtName As KBotTextBox
    Friend WithEvents pnlIdentity As Panel
    Friend WithEvents lblPseudonym As Label
    Friend WithEvents chkAnonymous As KBotCheckBox
    Friend WithEvents pnlFilter As Panel
    Friend WithEvents lblFilter As Label
    Friend WithEvents chkVercon As KBotCheckBox
    Friend WithEvents chkAdechit As KBotCheckBox
    Friend WithEvents chkKbot As KBotCheckBox
    Friend WithEvents pnlReply As Panel
    Friend WithEvents lblReply As Label
    Friend WithEvents btnReplyCancel As KBotButton
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents btnClose As KBotButton
End Class
