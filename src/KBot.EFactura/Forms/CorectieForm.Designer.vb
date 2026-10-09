<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CorectieForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        components = New System.ComponentModel.Container()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        pnlCard = New System.Windows.Forms.Panel()
        pnlJos = New System.Windows.Forms.Panel()
        btnTrimite = New System.Windows.Forms.Button()
        lblSep1 = New System.Windows.Forms.Label()
        btnRenunta = New System.Windows.Forms.Button()
        txtRef = New Global.KBot.Controls.KBotTextField()
        lblRefT = New System.Windows.Forms.Label()
        txtComentarii = New Global.KBot.Controls.KBotTextBox()
        lblComentariiT = New System.Windows.Forms.Label()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Global.KBot.Controls.KBotCaptionBar()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(txtRef)
        pnlCard.Controls.Add(lblRefT)
        pnlCard.Controls.Add(txtComentarii)
        pnlCard.Controls.Add(lblComentariiT)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(2, 2)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(596, 396)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(btnTrimite)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 338)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(8)
        pnlJos.Size = New System.Drawing.Size(596, 58)
        pnlJos.TabIndex = 6
        pnlJos.Tag = "Card"
        '
        ' btnTrimite
        '
        btnTrimite.DialogResult = System.Windows.Forms.DialogResult.OK
        btnTrimite.Dock = System.Windows.Forms.DockStyle.Right
        btnTrimite.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnTrimite.Location = New System.Drawing.Point(428, 8)
        btnTrimite.Name = "btnTrimite"
        btnTrimite.Size = New System.Drawing.Size(160, 42)
        btnTrimite.TabIndex = 0
        btnTrimite.Text = "Trimite corecția"
        tips.SetToolTipHeader(btnTrimite, "Trimite corecția")
        tips.SetToolTipText(btnTrimite, "Trimite la ANAF factura corectată (tip 384) cu textele de mai sus.")
        btnTrimite.UseVisualStyleBackColor = True
        '
        ' lblSep1
        '
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(416, 8)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(12, 42)
        lblSep1.TabIndex = 1
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Left
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(8, 8)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New System.Drawing.Size(117, 42)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să trimită nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' txtRef
        '
        txtRef.BackColor = System.Drawing.Color.Transparent
        txtRef.Dock = System.Windows.Forms.DockStyle.Top
        txtRef.Location = New System.Drawing.Point(0, 268)
        txtRef.MaxLength = 30
        txtRef.Name = "txtRef"
        txtRef.Size = New System.Drawing.Size(596, 48)
        txtRef.TabIndex = 5
        tips.SetToolTipText(txtRef, "Numărul comenzii la care se referă factura (cel mult 30 de caractere). Poate rămâne gol.")
        '
        ' lblRefT
        '
        lblRefT.Dock = System.Windows.Forms.DockStyle.Top
        lblRefT.Location = New System.Drawing.Point(0, 244)
        lblRefT.Name = "lblRefT"
        lblRefT.Padding = New System.Windows.Forms.Padding(14, 4, 14, 0)
        lblRefT.Size = New System.Drawing.Size(596, 24)
        lblRefT.TabIndex = 4
        lblRefT.Text = "Referința comenzii"
        '
        ' txtComentarii
        '
        txtComentarii.Dock = System.Windows.Forms.DockStyle.Top
        txtComentarii.Location = New System.Drawing.Point(0, 124)
        txtComentarii.MaxLength = 255
        txtComentarii.Name = "txtComentarii"
        txtComentarii.Size = New System.Drawing.Size(596, 120)
        txtComentarii.TabIndex = 3
        tips.SetToolTipHeader(txtComentarii, "Comentarii factură")
        tips.SetToolTipText(txtComentarii, "Text liber, cel mult 255 de caractere; se trimite la ANAF împreună cu factura corectată.")
        '
        ' lblComentariiT
        '
        lblComentariiT.Dock = System.Windows.Forms.DockStyle.Top
        lblComentariiT.Location = New System.Drawing.Point(0, 100)
        lblComentariiT.Name = "lblComentariiT"
        lblComentariiT.Padding = New System.Windows.Forms.Padding(14, 4, 14, 0)
        lblComentariiT.Size = New System.Drawing.Size(596, 24)
        lblComentariiT.TabIndex = 2
        lblComentariiT.Text = "Comentarii factură"
        '
        ' lblAntet
        '
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 40)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(14, 10, 14, 10)
        lblAntet.Size = New System.Drawing.Size(596, 60)
        lblAntet.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(596, 40)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "E-Factura — corectarea facturii"
        '
        ' CorectieForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New System.Drawing.Size(600, 400)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "CorectieForm"
        Padding = New System.Windows.Forms.Padding(2)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "E-Factura — corectarea facturii"
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents lblComentariiT As System.Windows.Forms.Label
    Friend WithEvents txtComentarii As Global.KBot.Controls.KBotTextBox
    Friend WithEvents lblRefT As System.Windows.Forms.Label
    Friend WithEvents txtRef As Global.KBot.Controls.KBotTextField
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnTrimite As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
