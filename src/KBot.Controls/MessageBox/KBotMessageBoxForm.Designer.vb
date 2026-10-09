Option Strict On

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotMessageBoxForm
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

    ' Authored at 144 dpi. The sizes here are STARTING values and MINIMUMS: LayoutContent grows the window to
    ' its message and reads these back -- pnlBody.Padding (the margin round the message), picIcon.Margin.Right
    ' MinimumSize.Width / MaximumSize.Width (the window's width range: a longer message wraps),
    ' picIcon.Margin.Right (the gap after the glyph), lblHeader.Margin.Bottom (the gap under the heading), capBar.Height,
    ' pnlButtons.Height and the size of btn3 (the smallest a button may be).
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        capBar = New KBotCaptionBar()
        pnlBody = New Panel()
        picIcon = New KBotMessageIcon()
        lblHeader = New KBotHtmlLabel()
        lblText = New KBotHtmlLabel()
        pnlButtons = New Panel()
        btnExtra = New KBotButton()
        btn3 = New KBotButton()
        btnOK = New KBotButton()
        btnNOK = New KBotButton()
        pnlBody.SuspendLayout()
        pnlButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowHelpButton = False
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(634, 50)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Mesaj"
        ' 
        ' pnlBody
        ' 
        pnlBody.AutoScroll = True
        pnlBody.Controls.Add(picIcon)
        pnlBody.Controls.Add(lblHeader)
        pnlBody.Controls.Add(lblText)
        pnlBody.Dock = DockStyle.Fill
        pnlBody.Location = New Point(0, 50)
        pnlBody.Margin = New Padding(0)
        pnlBody.Name = "pnlBody"
        pnlBody.Padding = New Padding(10)
        pnlBody.Size = New Size(634, 271)
        pnlBody.TabIndex = 1
        ' 
        ' picIcon
        ' 
        picIcon.BackColor = Color.Transparent
        picIcon.Dock = DockStyle.Left
        picIcon.Location = New Point(10, 10)
        picIcon.Margin = New Padding(0, 0, 18, 0)
        picIcon.Name = "picIcon"
        picIcon.Size = New Size(82, 251)
        picIcon.TabIndex = 0
        picIcon.TabStop = False
        ' 
        ' lblHeader
        ' 
        lblHeader.BackColor = Color.Transparent
        lblHeader.BorderWidth = 0
        lblHeader.Font = New Font("Calibri", 12F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lblHeader.Location = New Point(102, 24)
        lblHeader.Margin = New Padding(0, 0, 0, 9)
        lblHeader.Name = "lblHeader"
        lblHeader.Size = New Size(508, 36)
        lblHeader.TabIndex = 1
        lblHeader.UseMnemonic = False
        lblHeader.Visible = False
        ' 
        ' lblText
        ' 
        lblText.BackColor = Color.Transparent
        lblText.BorderWidth = 0
        lblText.Font = New Font("Calibri", 10F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lblText.Location = New Point(102, 58)
        lblText.Margin = New Padding(0)
        lblText.Name = "lblText"
        lblText.Size = New Size(508, 56)
        lblText.TabIndex = 2
        lblText.UseMnemonic = False
        ' 
        ' pnlButtons
        ' 
        pnlButtons.Controls.Add(btnExtra)
        pnlButtons.Controls.Add(btn3)
        pnlButtons.Controls.Add(btnOK)
        pnlButtons.Controls.Add(btnNOK)
        pnlButtons.Dock = DockStyle.Bottom
        pnlButtons.Location = New Point(0, 321)
        pnlButtons.Margin = New Padding(0)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Size = New Size(634, 50)
        pnlButtons.TabIndex = 2
        ' 
        ' btnExtra
        ' 
        btnExtra.AccessibleRole = AccessibleRole.PushButton
        btnExtra.Location = New Point(324, 4)
        btnExtra.Name = "btnExtra"
        btnExtra.Size = New Size(150, 48)
        btnExtra.TabIndex = 0
        btnExtra.Text = "Extra"
        btnExtra.Visible = False
        ' 
        ' btn3
        ' 
        btn3.AccessibleRole = AccessibleRole.PushButton
        btn3.Location = New Point(156, 4)
        btn3.Name = "btn3"
        btn3.Size = New Size(150, 48)
        btn3.TabIndex = 3
        btn3.Text = "btn3"
        btn3.Visible = False
        ' 
        ' btnOK
        ' 
        btnOK.AccessibleRole = AccessibleRole.PushButton
        btnOK.DialogResult = DialogResult.OK
        btnOK.Dock = DockStyle.Right
        btnOK.Location = New Point(484, 0)
        btnOK.Name = "btnOK"
        btnOK.Primary = True
        btnOK.Size = New Size(150, 50)
        btnOK.TabIndex = 1
        btnOK.Text = "OK"
        ' 
        ' btnNOK
        ' 
        btnNOK.AccessibleRole = AccessibleRole.PushButton
        btnNOK.DialogResult = DialogResult.Cancel
        btnNOK.Dock = DockStyle.Left
        btnNOK.Location = New Point(0, 0)
        btnNOK.Name = "btnNOK"
        btnNOK.Size = New Size(150, 50)
        btnNOK.TabIndex = 2
        btnNOK.Text = "Anulare"
        ' 
        ' KBotMessageBoxForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(634, 371)
        Controls.Add(pnlBody)
        Controls.Add(pnlButtons)
        Controls.Add(capBar)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MaximumSize = New Size(720, 0)
        MinimizeBox = False
        MinimumSize = New Size(480, 0)
        Name = "KBotMessageBoxForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        pnlBody.ResumeLayout(False)
        pnlButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlBody As Panel
    Friend WithEvents picIcon As KBotMessageIcon
    Friend WithEvents lblHeader As KBotHtmlLabel
    Friend WithEvents lblText As KBotHtmlLabel
    Friend WithEvents pnlButtons As Panel
    Friend WithEvents btnExtra As KBotButton
    Friend WithEvents btnOK As KBotButton
    Friend WithEvents btnNOK As KBotButton
    Friend WithEvents btn3 As KBotButton

End Class
