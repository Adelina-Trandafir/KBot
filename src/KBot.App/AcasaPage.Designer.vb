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
        tips = New KBotToolTip(components)
        pnlCentru = New Panel()
        pnlNoutati = New Panel()
        edNoutati = New KBotRichTextEditor()
        pnlNoutatiHead = New Panel()
        lblNoutati = New Label()
        btnNoutati = New Button()
        picLogo = New PictureBox()
        pnlCentru.SuspendLayout()
        pnlNoutati.SuspendLayout()
        pnlNoutatiHead.SuspendLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' pnlCentru
        '
        pnlCentru.Controls.Add(pnlNoutati)
        pnlCentru.Controls.Add(picLogo)
        pnlCentru.Dock = DockStyle.Fill
        pnlCentru.Location = New Point(0, 0)
        pnlCentru.Margin = New Padding(0)
        pnlCentru.Name = "pnlCentru"
        pnlCentru.Size = New Size(1920, 840)
        pnlCentru.TabIndex = 0
        '
        ' pnlNoutati
        '
        pnlNoutati.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        pnlNoutati.Controls.Add(edNoutati)
        pnlNoutati.Controls.Add(pnlNoutatiHead)
        pnlNoutati.Location = New Point(1061, 12)
        pnlNoutati.Margin = New Padding(0)
        pnlNoutati.Name = "pnlNoutati"
        pnlNoutati.Size = New Size(855, 560)
        pnlNoutati.TabIndex = 2
        '
        ' edNoutati
        '
        edNoutati.CollapseButton = False
        edNoutati.Dock = DockStyle.Fill
        edNoutati.FooterVisible = False
        edNoutati.HeaderVisible = False
        edNoutati.Location = New Point(4, 44)
        edNoutati.Margin = New Padding(0)
        edNoutati.Name = "edNoutati"
        edNoutati.Size = New Size(847, 512)
        edNoutati.TabIndex = 1
        '
        ' pnlNoutatiHead
        '
        pnlNoutatiHead.Controls.Add(lblNoutati)
        pnlNoutatiHead.Controls.Add(btnNoutati)
        pnlNoutatiHead.Dock = DockStyle.Top
        pnlNoutatiHead.Location = New Point(4, 0)
        pnlNoutatiHead.Margin = New Padding(0)
        pnlNoutatiHead.Name = "pnlNoutatiHead"
        pnlNoutatiHead.Size = New Size(847, 44)
        pnlNoutatiHead.TabIndex = 0
        '
        ' lblNoutati
        '
        lblNoutati.Dock = DockStyle.Fill
        lblNoutati.Font = New Font("Segoe UI Semibold", 11.0F)
        lblNoutati.Location = New Point(0, 0)
        lblNoutati.Margin = New Padding(0)
        lblNoutati.Name = "lblNoutati"
        lblNoutati.Padding = New Padding(0, 0, 10, 0)
        lblNoutati.Size = New Size(803, 44)
        lblNoutati.TabIndex = 0
        lblNoutati.Text = "Ce este nou"
        lblNoutati.TextAlign = ContentAlignment.MiddleRight
        '
        ' btnNoutati
        '
        btnNoutati.Dock = DockStyle.Right
        btnNoutati.FlatStyle = FlatStyle.Flat
        btnNoutati.Location = New Point(803, 0)
        btnNoutati.Margin = New Padding(0)
        btnNoutati.Name = "btnNoutati"
        btnNoutati.Size = New Size(44, 44)
        btnNoutati.TabIndex = 1
        btnNoutati.TabStop = False
        btnNoutati.ImageAlign = ContentAlignment.MiddleCenter
        tips.SetToolTipHeader(btnNoutati, "Ce este nou")
        tips.SetToolTipText(btnNoutati, "Strânge sau desface lista cu schimbările ultimelor versiuni.")
        btnNoutati.UseVisualStyleBackColor = True
        '
        ' picLogo
        '
        picLogo.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        picLogo.Image = My.Resources.Resources.kbot_logo
        picLogo.Location = New Point(1736, 656)
        picLogo.Margin = New Padding(0)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(160, 160)
        picLogo.SizeMode = PictureBoxSizeMode.Zoom
        picLogo.TabIndex = 1
        picLogo.TabStop = False
        '
        ' AcasaPage
        '
        AutoScaleDimensions = New SizeF(144.0F, 144.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1920, 840)
        Controls.Add(pnlCentru)
        FormBorderStyle = FormBorderStyle.None
        Name = "AcasaPage"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "K-BOT — Acasă"
        pnlCentru.ResumeLayout(False)
        pnlNoutati.ResumeLayout(False)
        pnlNoutatiHead.ResumeLayout(False)
        CType(picLogo, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCentru As Panel
    Friend WithEvents pnlNoutati As Panel
    Friend WithEvents edNoutati As KBotRichTextEditor
    Friend WithEvents pnlNoutatiHead As Panel
    Friend WithEvents lblNoutati As Label
    Friend WithEvents btnNoutati As Button
    Friend WithEvents picLogo As PictureBox
End Class
