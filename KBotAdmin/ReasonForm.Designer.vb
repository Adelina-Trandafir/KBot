' The "reason" dialog (slice ME01): a few lines of text the applicant will receive. All controls are
' declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ReasonForm
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
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        lblPrompt = New Label()
        txtReason = New KBotTextBox()
        tlyButtons = New KBotTableLayoutPanel()
        btnCancel = New KBotButton()
        btnOk = New KBotButton()
        tlyMain.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblPrompt, 0, 0)
        tlyMain.Controls.Add(txtReason, 0, 1)
        tlyMain.Controls.Add(tlyButtons, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 16, 16, 12)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(520, 320)
        tlyMain.TabIndex = 0
        '
        ' lblPrompt
        '
        tips.SetToolTipHeader(lblPrompt, "Motivul respingerii")
        tips.SetToolTipText(lblPrompt, "Scrieți pe scurt de ce se respinge cererea; solicitantul primește textul pe e-mail.")
        lblPrompt.Dock = DockStyle.Fill
        lblPrompt.Location = New Point(16, 16)
        lblPrompt.Margin = New Padding(0)
        lblPrompt.Name = "lblPrompt"
        lblPrompt.Size = New Size(488, 52)
        lblPrompt.TabIndex = 0
        lblPrompt.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtReason
        '
        txtReason.Dock = DockStyle.Fill
        txtReason.Location = New Point(16, 68)
        txtReason.Margin = New Padding(0, 0, 0, 8)
        txtReason.MaxLength = 2000
        txtReason.Multiline = True
        txtReason.Name = "txtReason"
        txtReason.ScrollBars = ScrollBars.Vertical
        txtReason.Size = New Size(488, 172)
        txtReason.TabIndex = 1
        tips.SetToolTipHeader(txtReason, "Motivul")
        tips.SetToolTipText(txtReason, "Între 5 și 2000 de caractere. Solicitantul îl primește pe e-mail.")
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyButtons.Controls.Add(btnCancel, 0, 0)
        tlyButtons.Controls.Add(btnOk, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 248)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(488, 52)
        tlyButtons.TabIndex = 2
        '
        ' btnCancel
        '
        tips.SetToolTipHeader(btnCancel, "Renunță")
        tips.SetToolTipText(btnCancel, "Închide fereastra. Cererea rămâne în așteptare.")
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Fill
        btnCancel.Location = New Point(0, 0)
        btnCancel.Margin = New Padding(0, 6, 6, 6)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(114, 40)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Renunță"
        '
        ' btnOk
        '
        tips.SetToolTipHeader(btnOk, "Respinge cererea")
        tips.SetToolTipText(btnOk, "Respinge cererea și trimite motivul solicitantului pe e-mail. Nu se poate anula.")
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(348, 0)
        btnOk.Margin = New Padding(6, 6, 0, 6)
        btnOk.Name = "btnOk"
        btnOk.Primary = True
        btnOk.Size = New Size(140, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Respinge cererea"
        '
        ' ReasonForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(520, 320)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "ReasonForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Motivul respingerii"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblPrompt As Label
    Friend WithEvents txtReason As KBotTextBox
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnCancel As KBotButton
    Friend WithEvents btnOk As KBotButton
End Class
