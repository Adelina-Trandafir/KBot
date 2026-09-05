Imports KBot.Controls

' Dialogul care cere ZIUA pentru care se genereaza o ordonantare (felia 0049).
'
' In Access, data venea din randul de plata pe care statea cursorul in `frmFX_MAIN`, iar
' `FX_Adaugare_ORD_Din_Plati` o primea ca parametru. Aici punctul de intrare e arborele
' vederii ORD, care nu poarta plati, deci ziua se cere explicit — un camp in plus, dar in
' locul unei date ghicite.
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md).
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste: Calibri 9 se
' masoara (6, 14) acolo (felia 0052). Cele doua se schimba INTOTDEAUNA impreuna, si numai
' din designer -- o pereche luata de la alt font sau de la alt dpi turteste fereastra la
' deschidere, fara ca nimic din designer s-o arate.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdZiuaForm
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
        tlyMain = New TableLayoutPanel()
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        dtpZiua = New DateTimePicker()
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
        tlyMain.Controls.Add(dtpZiua, 0, 2)
        tlyMain.Controls.Add(tlySubsol, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(430, 218)
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
        capBar.Size = New Size(430, 46)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Ordonanțare nouă"
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(3, 46)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(10, 8, 10, 8)
        lblIntro.Size = New Size(424, 80)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Alegeți ziua pentru care se generează ordonanțarea. Se iau toate plățile " &
                        "neordonanțate din acea zi."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' dtpZiua
        '
        dtpZiua.Dock = DockStyle.Fill
        dtpZiua.Format = DateTimePickerFormat.Custom
        dtpZiua.CustomFormat = "dd.MM.yyyy"
        dtpZiua.Location = New Point(13, 132)
        dtpZiua.Margin = New Padding(13, 6, 13, 6)
        dtpZiua.Name = "dtpZiua"
        dtpZiua.Size = New Size(404, 23)
        dtpZiua.TabIndex = 2
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
        tlySubsol.Location = New Point(0, 166)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(430, 52)
        tlySubsol.TabIndex = 3
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Location = New Point(173, 6)
        btnRenunta.Margin = New Padding(3, 6, 3, 6)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(114, 40)
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnOk
        '
        btnOk.DialogResult = DialogResult.OK
        btnOk.Dock = DockStyle.Fill
        btnOk.Location = New Point(293, 6)
        btnOk.Margin = New Padding(3, 6, 3, 6)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(134, 40)
        btnOk.TabIndex = 1
        btnOk.Text = "Generează"
        btnOk.UseVisualStyleBackColor = True
        '
        ' OrdZiuaForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnRenunta
        ClientSize = New Size(432, 220)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "OrdZiuaForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Ordonanțare nouă"
        tlyMain.ResumeLayout(False)
        tlySubsol.ResumeLayout(False)
        tips.SetToolTipHeader(dtpZiua, "Ziua plăților")
        tips.SetToolTipText(dtpZiua, "Ordonanțarea acoperă plățile neordonanțate din ziua asta." & vbLf & "Dacă ziua are peste 25 de parteneri, veți fi avertizat și va fi nevoie de mai multe ordonanțări.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents dtpZiua As DateTimePicker
    Friend WithEvents tlySubsol As TableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnOk As Button
End Class
