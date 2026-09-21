<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class IstoricIntervalForm
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
        pnlCard = New Panel()
        pnlContinut = New Panel()
        pnlFoot = New Panel()
        lblInterval = New Label()
        btnTotIstoricul = New Button()
        btnInchide = New Button()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlFoot.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard - in the card the children are added in REVERSE dock order:
        ' Fill first, then Bottom, Top last (the last one added sits highest).
        '
        pnlCard.Controls.Add(pnlContinut)
        pnlCard.Controls.Add(pnlFoot)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(1, 3)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1010, 655)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' pnlContinut - hosts the IstoricView. The view needs the API client and the shell's
        ' re-login net at construction, so it is created in code and dropped in here (the same
        ' arrangement MainForm uses for its views).
        '
        pnlContinut.Dock = DockStyle.Fill
        pnlContinut.Location = New Point(0, 48)
        pnlContinut.Name = "pnlContinut"
        pnlContinut.Size = New Size(1010, 543)
        pnlContinut.TabIndex = 0
        '
        ' pnlFoot
        '
        pnlFoot.Controls.Add(lblInterval)
        pnlFoot.Controls.Add(btnTotIstoricul)
        pnlFoot.Controls.Add(btnInchide)
        pnlFoot.Dock = DockStyle.Bottom
        pnlFoot.Location = New Point(0, 591)
        pnlFoot.Name = "pnlFoot"
        pnlFoot.Padding = New Padding(14, 10, 14, 10)
        pnlFoot.Size = New Size(1010, 64)
        pnlFoot.TabIndex = 1
        '
        ' lblInterval - the window of time the rows are cut to, spelled out for the operator.
        '
        lblInterval.Dock = DockStyle.Fill
        lblInterval.Location = New Point(14, 10)
        lblInterval.Name = "lblInterval"
        lblInterval.Size = New Size(632, 44)
        lblInterval.TabIndex = 0
        lblInterval.Text = ""
        lblInterval.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnTotIstoricul
        '
        btnTotIstoricul.Dock = DockStyle.Right
        btnTotIstoricul.FlatStyle = FlatStyle.Flat
        btnTotIstoricul.Location = New Point(646, 10)
        btnTotIstoricul.Name = "btnTotIstoricul"
        btnTotIstoricul.Size = New Size(200, 44)
        btnTotIstoricul.TabIndex = 1
        btnTotIstoricul.Text = "Tot istoricul"
        btnTotIstoricul.UseVisualStyleBackColor = True
        '
        ' btnInchide
        '
        btnInchide.Dock = DockStyle.Right
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(846, 10)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(150, 44)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Închide"
        btnInchide.UseVisualStyleBackColor = True
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1010, 48)
        capBar.TabIndex = 2
        capBar.TabStop = False
        capBar.Text = "Istoric angajament"
        '
        ' IstoricIntervalForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1012, 661)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        MinimumSize = New Size(760, 460)
        Name = "IstoricIntervalForm"
        Padding = New Padding(1, 3, 1, 3)
        StartPosition = FormStartPosition.CenterParent
        Text = "Istoric angajament"
        pnlCard.ResumeLayout(False)
        pnlFoot.ResumeLayout(False)
        '
        ' tips - hover labels, all in Romanian.
        '
        tips.SetToolTipHeader(btnTotIstoricul, "Tot istoricul")
        tips.SetToolTipText(btnTotIstoricul, "Scoate limita de timp: arată toate rândurile de istoric ale angajamentului.")
        tips.SetToolTipHeader(btnInchide, "Închide")
        tips.SetToolTipText(btnInchide, "Închide fereastra.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents pnlContinut As Panel
    Friend WithEvents pnlFoot As Panel
    Friend WithEvents lblInterval As Label
    Friend WithEvents btnTotIstoricul As Button
    Friend WithEvents btnInchide As Button
End Class
