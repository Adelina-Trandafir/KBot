' The cell viewer of the page «Tabele»: the whole text of a long cell, or the bytes of a blob as a hex dump, read-only, in a modal window.
' All controls are declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CellContentForm
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
        txtContent = New KBotTextBox()
        tlyButtons = New KBotTableLayoutPanel()
        btnClose = New KBotButton()
        btnSave = New KBotButton()
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
        tlyMain.Controls.Add(txtContent, 0, 1)
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
        tlyMain.Size = New Size(900, 620)
        tlyMain.TabIndex = 0
        '
        ' lblInfo
        '
        tips.SetToolTipHeader(lblInfo, "Celula")
        tips.SetToolTipText(lblInfo, "Tabelul, coloana și rândul celulei, mărimea ei și cât din ea se vede aici.")
        lblInfo.AutoEllipsis = True
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(16, 12)
        lblInfo.Margin = New Padding(0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(868, 48)
        lblInfo.TabIndex = 0
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtContent
        '
        txtContent.CornerRadius = 0
        txtContent.Dock = DockStyle.Fill
        txtContent.Font = New Font("Consolas", 9.75F)
        txtContent.Location = New Point(16, 60)
        txtContent.Margin = New Padding(0, 0, 0, 8)
        txtContent.MaxLength = 8000000
        txtContent.Multiline = True
        txtContent.Name = "txtContent"
        txtContent.ReadOnly = True
        txtContent.ScrollBars = ScrollBars.Both
        txtContent.Size = New Size(868, 490)
        txtContent.TabIndex = 1
        txtContent.WordWrap = False
        tips.SetToolTipHeader(txtContent, "Conținutul celulei")
        tips.SetToolTipText(txtContent, "Doar pentru citire. Se poate selecta și copia. Un blob apare ca hexazecimal, cu textul lui în dreapta.")
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 4
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        tlyButtons.Controls.Add(btnClose, 0, 0)
        tlyButtons.Controls.Add(btnSave, 2, 0)
        tlyButtons.Controls.Add(btnCopy, 3, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(16, 558)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(868, 52)
        tlyButtons.TabIndex = 2
        '
        ' btnClose
        '
        tips.SetToolTipHeader(btnClose, "Închide")
        tips.SetToolTipText(btnClose, "Închide fereastra. Nu se schimbă nimic în bază.")
        btnClose.DialogResult = DialogResult.Cancel
        btnClose.Dock = DockStyle.Fill
        btnClose.Location = New Point(0, 0)
        btnClose.Margin = New Padding(0, 6, 6, 6)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(114, 40)
        btnClose.TabIndex = 0
        btnClose.Text = "Închide"
        '
        ' btnSave
        '
        tips.SetToolTipHeader(btnSave, "Salvează ca fișier")
        tips.SetToolTipText(btnSave, "Scrie conținutul pe disc: octeții blobului așa cum sunt în bază, sau textul ca UTF-8.")
        btnSave.Dock = DockStyle.Fill
        btnSave.Location = New Point(528, 0)
        btnSave.Margin = New Padding(6, 6, 6, 6)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(178, 40)
        btnSave.TabIndex = 1
        btnSave.Text = "Salvează ca fișier..."
        '
        ' btnCopy
        '
        tips.SetToolTipHeader(btnCopy, "Copiază")
        tips.SetToolTipText(btnCopy, "Copiază în clipboard tot ce e afișat aici.")
        btnCopy.Dock = DockStyle.Fill
        btnCopy.Location = New Point(718, 0)
        btnCopy.Margin = New Padding(6, 6, 0, 6)
        btnCopy.Name = "btnCopy"
        btnCopy.Primary = True
        btnCopy.Size = New Size(150, 40)
        btnCopy.TabIndex = 2
        btnCopy.Text = "Copiază"
        '
        ' CellContentForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(900, 620)
        Controls.Add(tlyMain)
        MinimumSize = New Size(520, 340)
        Name = "CellContentForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Conținut celulă"
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblInfo As Label
    Friend WithEvents txtContent As KBotTextBox
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnClose As KBotButton
    Friend WithEvents btnSave As KBotButton
    Friend WithEvents btnCopy As KBotButton
End Class
