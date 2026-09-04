Imports KBot.Controls

' Dialogul care cere TEXTUL unui document justificativ (felia 0049, pasul 0049-02).
'
' Pe randul sintetic «< TOTI BENEFICIARII >» grila e doar-citire (cerinta operatorului):
' un rand de acolo sta pe cate o copie la fiecare beneficiar, deci nu se scrie in el direct.
' Textul se cere aici si abia apoi se imparte la toti. Pe un beneficiar anume grila ramane
' editabila, dar randul se naste tot completat — un rand gol adaugat in grila nu se deosebea
' de unul uitat.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md).
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste: Calibri 9 se
' masoara (6, 14) acolo (felia 0052). Cele doua se schimba INTOTDEAUNA impreuna, si numai
' din designer -- o pereche luata de la alt font sau de la alt dpi turteste fereastra la
' deschidere, fara ca nimic din designer s-o arate.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdTextForm
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
        components = New ComponentModel.Container()
        tips = New KBotToolTip(components)
        tlyMain = New TableLayoutPanel()
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        txtDoc = New TextBox()
        tlySubsol = New TableLayoutPanel()
        btnRenunta = New Button()
        btnOk = New Button()
        tlyMain.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(lblIntro, 0, 1)
        tlyMain.Controls.Add(txtDoc, 0, 2)
        tlyMain.Controls.Add(tlySubsol, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 56F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(520, 288)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.ShowThemeEditor = False
        capBar.ShowThemeOptions = False
        capBar.Size = New Size(520, 46)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Document justificativ"
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(3, 46)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(10, 6, 10, 6)
        lblIntro.Size = New Size(514, 56)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Scrieți textul documentului justificativ."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDoc
        '
        txtDoc.Dock = DockStyle.Fill
        txtDoc.Location = New Point(13, 108)
        txtDoc.Margin = New Padding(13, 6, 13, 6)
        txtDoc.Multiline = True
        txtDoc.Name = "txtDoc"
        txtDoc.ScrollBars = ScrollBars.Vertical
        txtDoc.Size = New Size(494, 122)
        txtDoc.TabIndex = 2
        '
        ' tlySubsol
        '
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlySubsol.Controls.Add(btnRenunta, 1, 0)
        tlySubsol.Controls.Add(btnOk, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 236)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(520, 52)
        tlySubsol.TabIndex = 3
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Location = New Point(263, 6)
        btnRenunta.Margin = New Padding(3, 6, 3, 6)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(114, 40)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnOk
        '
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(383, 6)
        btnOk.Margin = New Padding(3, 6, 3, 6)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(134, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Adaugă"
        btnOk.UseVisualStyleBackColor = True
        '
        ' OrdTextForm
        '
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnRenunta
        ClientSize = New Size(522, 290)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "OrdTextForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Document justificativ"
        tlyMain.ResumeLayout(False)
        tlyMain.PerformLayout()
        tlySubsol.ResumeLayout(False)
        tips.SetToolTipHeader(txtDoc, "Textul documentului")
        tips.SetToolTipText(txtDoc, "Ce scrie aici ajunge în coloana «Document justificativ»." & vbLf & "Pe rândul «< TOȚI BENEFICIARII >» textul se dă tuturor beneficiarilor, câte o copie fiecăruia.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents capBar As KBot.Controls.KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents txtDoc As TextBox
    Friend WithEvents tlySubsol As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnOk As Button
End Class
