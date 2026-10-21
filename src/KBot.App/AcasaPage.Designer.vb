Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AcasaPage
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AcasaPage))
        tips = New KBotToolTip(components)
        btnNoutati = New Button()
        pnlCentru = New Panel()
        pnlNoutati = New Panel()
        edNoutati = New KBotRichTextEditor()
        pnlNoutatiHead = New Panel()
        lblNoutati = New Label()
        picLogo = New KBotChatRobot()
        bubble = New KBotChatBubble()
        pnlCentru.SuspendLayout()
        pnlNoutati.SuspendLayout()
        pnlNoutatiHead.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnNoutati
        ' 
        btnNoutati.Dock = DockStyle.Right
        btnNoutati.FlatStyle = FlatStyle.Flat
        btnNoutati.Location = New Point(811, 0)
        btnNoutati.Margin = New Padding(0)
        btnNoutati.Name = "btnNoutati"
        btnNoutati.Size = New Size(44, 44)
        btnNoutati.TabIndex = 1
        btnNoutati.TabStop = False
        tips.SetToolTipHeader(btnNoutati, "Ce este nou")
        tips.SetToolTipText(btnNoutati, "Strânge sau desface lista cu schimbările ultimelor versiuni.")
        btnNoutati.UseVisualStyleBackColor = True
        ' 
        ' pnlCentru
        ' 
        pnlCentru.Controls.Add(bubble)
        pnlCentru.Controls.Add(pnlNoutati)
        pnlCentru.Controls.Add(picLogo)
        pnlCentru.Dock = DockStyle.Fill
        pnlCentru.Location = New Point(0, 0)
        pnlCentru.Margin = New Padding(0)
        pnlCentru.Name = "pnlCentru"
        pnlCentru.Size = New Size(1386, 771)
        pnlCentru.TabIndex = 0
        ' 
        ' pnlNoutati
        ' 
        pnlNoutati.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        pnlNoutati.Controls.Add(edNoutati)
        pnlNoutati.Controls.Add(pnlNoutatiHead)
        pnlNoutati.Location = New Point(527, 12)
        pnlNoutati.Margin = New Padding(0)
        pnlNoutati.Name = "pnlNoutati"
        pnlNoutati.Size = New Size(855, 560)
        pnlNoutati.TabIndex = 2
        ' 
        ' edNoutati
        ' 
        edNoutati.Dock = DockStyle.Fill
        edNoutati.Font = New Font("Calibri", 9F)
        edNoutati.FooterVisible = False
        edNoutati.HeaderVisible = False
        edNoutati.Location = New Point(0, 44)
        edNoutati.Margin = New Padding(0)
        edNoutati.Name = "edNoutati"
        edNoutati.Size = New Size(855, 516)
        edNoutati.TabIndex = 1
        ' 
        ' pnlNoutatiHead
        ' 
        pnlNoutatiHead.Controls.Add(lblNoutati)
        pnlNoutatiHead.Controls.Add(btnNoutati)
        pnlNoutatiHead.Dock = DockStyle.Top
        pnlNoutatiHead.Location = New Point(0, 0)
        pnlNoutatiHead.Margin = New Padding(0)
        pnlNoutatiHead.Name = "pnlNoutatiHead"
        pnlNoutatiHead.Size = New Size(855, 44)
        pnlNoutatiHead.TabIndex = 0
        ' 
        ' lblNoutati
        ' 
        lblNoutati.Dock = DockStyle.Fill
        lblNoutati.Font = New Font("Segoe UI Semibold", 11F)
        lblNoutati.Location = New Point(0, 0)
        lblNoutati.Margin = New Padding(0)
        lblNoutati.Name = "lblNoutati"
        lblNoutati.Padding = New Padding(0, 0, 10, 0)
        lblNoutati.Size = New Size(811, 44)
        lblNoutati.TabIndex = 0
        lblNoutati.Text = "Ce este nou"
        lblNoutati.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' picLogo
        ' 
        picLogo.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        picLogo.Image = CType(resources.GetObject("picLogo.Image"), Image)
        picLogo.Location = New Point(1202, 587)
        picLogo.Margin = New Padding(0)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(160, 160)
        picLogo.TabIndex = 1
        picLogo.TabStop = False
        tips.SetToolTipHeader(picLogo, "Discuții")
        tips.SetToolTipText(picLogo, "Deschide mesajele schimbate cu ceilalți utilizatori ai K-BOT, VERCON și ADECHIT.")
        ' 
        ' bubble
        ' 
        bubble.Anchor = AnchorStyles.None
        bubble.Location = New Point(700, 20)
        bubble.Margin = New Padding(0)
        bubble.Name = "bubble"
        bubble.Size = New Size(570, 700)
        bubble.TabIndex = 3
        bubble.Visible = False
        ' 
        ' AcasaPage
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1386, 771)
        Controls.Add(pnlCentru)
        FormBorderStyle = FormBorderStyle.None
        Name = "AcasaPage"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "K-BOT — Acasă"
        pnlCentru.ResumeLayout(False)
        pnlNoutati.ResumeLayout(False)
        pnlNoutatiHead.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCentru As Panel
    Friend WithEvents pnlNoutati As Panel
    Friend WithEvents edNoutati As KBotRichTextEditor
    Friend WithEvents pnlNoutatiHead As Panel
    Friend WithEvents lblNoutati As Label
    Friend WithEvents btnNoutati As Button
    Friend WithEvents picLogo As KBotChatRobot
    Friend WithEvents bubble As KBotChatBubble
End Class
