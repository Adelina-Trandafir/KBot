Imports KBot.Controls

' The small password prompt shown when the operator ticks the advanced options switch on the
' application page of Setari. All controls are declared HERE (docs/kbot-forms-ui-convention.md).
' Coordinates are in the 144 dpi the dialog was authored at; AutoScaleDimensions carries the stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ParolaAvansataForm
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
        capBar = New KBotCaptionBar()
        lblIntro = New Label()
        tlyCampuri = New KBotTableLayoutPanel()
        lblParola = New Label()
        txtParola = New KBotTextField()
        lblEroare = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        btnRenunta = New Button()
        btnConfirma = New Button()
        tlyMain.SuspendLayout()
        tlyCampuri.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(lblIntro, 0, 1)
        tlyMain.Controls.Add(tlyCampuri, 0, 2)
        tlyMain.Controls.Add(tlySubsol, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 57F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 70F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.Size = New Size(658, 338)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.settings__1_
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(658, 57)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Opțiuni avansate"
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Location = New Point(4, 57)
        lblIntro.Margin = New Padding(4, 0, 4, 0)
        lblIntro.Name = "lblIntro"
        lblIntro.Padding = New Padding(20, 8, 20, 8)
        lblIntro.Size = New Size(650, 70)
        lblIntro.TabIndex = 1
        lblIntro.Text = "Opțiunile avansate se activează doar cu parolă."
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' tlyCampuri
        '
        tlyCampuri.AutoFitToTheme = False
        tlyCampuri.ColumnCount = 2
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.Controls.Add(lblParola, 0, 0)
        tlyCampuri.Controls.Add(txtParola, 1, 0)
        tlyCampuri.Controls.Add(lblEroare, 1, 1)
        tlyCampuri.Dock = DockStyle.Fill
        tlyCampuri.Location = New Point(0, 127)
        tlyCampuri.Margin = New Padding(0)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.Padding = New Padding(24, 8, 24, 8)
        tlyCampuri.RowCount = 3
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCampuri.Size = New Size(658, 133)
        tlyCampuri.TabIndex = 2
        '
        ' lblParola
        '
        lblParola.AutoSize = True
        lblParola.Dock = DockStyle.Fill
        lblParola.Location = New Point(28, 8)
        lblParola.Margin = New Padding(4, 0, 4, 10)
        lblParola.Name = "lblParola"
        lblParola.Size = New Size(152, 50)
        lblParola.TabIndex = 0
        lblParola.Text = "Parola"
        lblParola.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParola
        '
        txtParola.BackColor = Color.Transparent
        txtParola.Dock = DockStyle.Fill
        txtParola.Location = New Point(188, 8)
        txtParola.Margin = New Padding(4, 0, 4, 10)
        txtParola.Name = "txtParola"
        txtParola.Size = New Size(442, 50)
        txtParola.TabIndex = 1
        tips.SetToolTipHeader(txtParola, "Parola opțiunilor avansate")
        tips.SetToolTipText(txtParola, "Literele mari și mici contează.")
        txtParola.UseSystemPasswordChar = True
        '
        ' lblEroare
        '
        lblEroare.AutoSize = True
        lblEroare.Dock = DockStyle.Fill
        lblEroare.Location = New Point(188, 68)
        lblEroare.Margin = New Padding(4, 0, 4, 0)
        lblEroare.Name = "lblEroare"
        lblEroare.Size = New Size(442, 25)
        lblEroare.TabIndex = 2
        '
        ' tlySubsol
        '
        tlySubsol.ColumnCount = 3
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210F))
        tlySubsol.Controls.Add(btnRenunta, 1, 0)
        tlySubsol.Controls.Add(btnConfirma, 2, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 260)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(20, 0, 20, 0)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(658, 78)
        tlySubsol.TabIndex = 3
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(252, 12)
        btnRenunta.Margin = New Padding(4, 12, 4, 12)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(172, 54)
        btnRenunta.TabIndex = 1
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnConfirma
        '
        btnConfirma.Dock = DockStyle.Fill
        btnConfirma.FlatStyle = FlatStyle.Flat
        btnConfirma.Font = New Font("Segoe UI Semibold", 9F)
        btnConfirma.Location = New Point(432, 12)
        btnConfirma.Margin = New Padding(4, 12, 4, 12)
        btnConfirma.Name = "btnConfirma"
        btnConfirma.Size = New Size(202, 54)
        btnConfirma.TabIndex = 0
        btnConfirma.Text = "Activează"
        btnConfirma.UseVisualStyleBackColor = True
        '
        ' ParolaAvansataForm
        '
        AcceptButton = btnConfirma
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(660, 340)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MaximizeBox = False
        MinimizeBox = False
        Name = "ParolaAvansataForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Opțiuni avansate"
        tlyMain.ResumeLayout(False)
        tlyCampuri.ResumeLayout(False)
        tlyCampuri.PerformLayout()
        tlySubsol.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents lblIntro As Label
    Friend WithEvents tlyCampuri As KBotTableLayoutPanel
    Friend WithEvents lblParola As Label
    Friend WithEvents txtParola As KBotTextField
    Friend WithEvents lblEroare As Label
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnConfirma As Button
End Class
