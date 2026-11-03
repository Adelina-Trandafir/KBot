' The SQL viewer (slice ME01): the text of one backup or schema plan file, read-only, in a modal window.
' All controls are declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SqlTextForm
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
        lblInfo = New Label()
        txtSql = New KBotTextBox()
        tlyButtons = New KBotTableLayoutPanel()
        btnClose = New KBotButton()
        btnCopy = New KBotButton()
        tlyMain.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblInfo, 0, 0)
        tlyMain.Controls.Add(txtSql, 0, 1)
        tlyMain.Controls.Add(tlyButtons, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(16, 12, 16, 12)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(960, 660)
        tlyMain.TabIndex = 0
        '
        ' lblInfo
        '
        tips.SetToolTipHeader(lblInfo, "Fișierul")
        tips.SetToolTipText(lblInfo, "Numele fișierului de pe server, mărimea lui și cât din el se vede aici." & vbLf & "Fișierele foarte mari se arată doar de la început.")
        lblInfo.AutoEllipsis = True
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(16, 12)
        lblInfo.Margin = New Padding(0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(928, 48)
        lblInfo.TabIndex = 0
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtSql
        '
        txtSql.CornerRadius = 0
        txtSql.Dock = DockStyle.Fill
        txtSql.Font = New Font("Consolas", 9.75F)
        txtSql.Location = New Point(16, 60)
        txtSql.Margin = New Padding(0, 0, 0, 8)
        txtSql.MaxLength = 4000000
        txtSql.Multiline = True
        txtSql.Name = "txtSql"
        txtSql.ReadOnly = True
        txtSql.ScrollBars = ScrollBars.Both
        txtSql.Size = New Size(928, 530)
        txtSql.TabIndex = 1
        txtSql.WordWrap = False
        tips.SetToolTipHeader(txtSql, "Textul SQL")
        tips.SetToolTipText(txtSql, "Conținutul fișierului, doar pentru citire. Se poate selecta și copia.")
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyButtons.Controls.Add(btnClose, 0, 0)
        tlyButtons.Controls.Add(btnCopy, 2, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 598)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(928, 52)
        tlyButtons.TabIndex = 2
        '
        ' btnClose
        '
        tips.SetToolTipHeader(btnClose, "Închide")
        tips.SetToolTipText(btnClose, "Închide fereastra. Nu se schimbă nimic pe server.")
        btnClose.DialogResult = DialogResult.Cancel
        btnClose.Dock = DockStyle.Fill
        btnClose.Location = New Point(0, 0)
        btnClose.Margin = New Padding(0, 6, 6, 6)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(114, 40)
        btnClose.TabIndex = 0
        btnClose.Text = "Închide"
        '
        ' btnCopy
        '
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Copiază tot textul afișat în clipboard.")
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(778, 0)
        btnCopy.Margin = New Padding(6, 6, 0, 6)
        btnCopy.Name = "btnCopy"
        btnCopy.Primary = True
        btnCopy.Size = New Size(150, 40)
        btnCopy.TabIndex = 1
        btnCopy.Text = "Copiază textul"
        '
        ' SqlTextForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(960, 660)
        Controls.Add(tlyMain)
        MinimumSize = New Size(480, 320)
        Name = "SqlTextForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Text SQL"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblInfo As Label
    Friend WithEvents txtSql As KBotTextBox
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnClose As KBotButton
    Friend WithEvents btnCopy As KBotButton
End Class
