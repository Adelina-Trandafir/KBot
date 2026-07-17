<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class InternalInfoForm
    Inherits KBot.Theming.KBotThemedForm

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
        capBar = New KBot.Theming.KBotCaptionBar()
        lblHeader = New Label()
        txtInfo = New TextBox()
        pnlFoot = New Panel()
        btnRefresh = New Button()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard — cardul rădăcină; copiii se adaugă în ordine INVERSĂ de dock:
        ' txtInfo (Fill) primul, apoi pnlFoot (Bottom), lblHeader (Top), capBar (Top, topmost).
        '
        pnlCard.Controls.Add(txtInfo)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(lblHeader)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 1)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(378, 518)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.ShowMaximize = False
        capBar.ShowMinimize = False
        capBar.Size = New Size(378, 40)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "Informații interne"
        '
        ' lblHeader — Cod + Descriere ale angajamentului selectat.
        '
        lblHeader.Dock = DockStyle.Top
        lblHeader.Font = New Font("Segoe UI Semibold", 10.0F)
        lblHeader.Location = New Point(0, 40)
        lblHeader.Name = "lblHeader"
        lblHeader.Padding = New Padding(12, 8, 12, 6)
        lblHeader.Size = New Size(378, 46)
        lblHeader.TabIndex = 0
        lblHeader.Text = "Niciun angajament selectat"
        '
        ' txtInfo — câmp/valoare, monospațiat, doar-citire (fără chenar).
        '
        txtInfo.BorderStyle = BorderStyle.None
        txtInfo.Dock = DockStyle.Fill
        txtInfo.Font = New Font("Consolas", 9.75F)
        txtInfo.Location = New Point(0, 86)
        txtInfo.Multiline = True
        txtInfo.Name = "txtInfo"
        txtInfo.ReadOnly = True
        txtInfo.ScrollBars = ScrollBars.Vertical
        txtInfo.Size = New Size(378, 384)
        txtInfo.TabIndex = 1
        txtInfo.TabStop = False
        txtInfo.WordWrap = True
        '
        ' pnlFoot — bara de jos: butonul Reîmprospătează.
        '
        pnlFoot.Controls.Add(btnRefresh)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 470)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Size = New Size(378, 48)
        pnlFoot.TabIndex = 2
        pnlFoot.Tag = "Card"
        '
        ' btnRefresh
        '
        btnRefresh.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRefresh.FlatStyle = FlatStyle.Flat
        btnRefresh.Location = New Point(240, 8)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(126, 32)
        btnRefresh.TabIndex = 0
        btnRefresh.Text = "Reîmprospătează"
        btnRefresh.UseVisualStyleBackColor = True
        '
        ' InternalInfoForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(380, 520)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(320, 360)
        Name = "InternalInfoForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Informații interne"
        pnlCard.ResumeLayout(False)
        pnlCard.PerformLayout()
        pnlFoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBot.Theming.KBotCaptionBar
    Friend WithEvents lblHeader As Label
    Friend WithEvents txtInfo As TextBox
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents btnRefresh As Button
End Class
