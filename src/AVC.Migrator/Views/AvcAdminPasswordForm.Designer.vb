<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AvcAdminPasswordForm
    Inherits KBot.Theming.KBotThemedForm

    ' Every control is declared HERE so the form renders in the VS designer (docs/kbot-forms-ui-convention.md).
    ' Pixel values are logical, saved at 96 dpi.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents tlpRoot As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblIntro As System.Windows.Forms.Label
    Friend WithEvents txtParola As KBot.Controls.KBotTextField
    Friend WithEvents tlpButoane As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents tipPar As KBot.Controls.KBotToolTip

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tlpRoot = New Global.KBot.Controls.KBotTableLayoutPanel()
        lblIntro = New System.Windows.Forms.Label()
        txtParola = New KBot.Controls.KBotTextField()
        tlpButoane = New Global.KBot.Controls.KBotTableLayoutPanel()
        btnRenunta = New System.Windows.Forms.Button()
        btnOk = New System.Windows.Forms.Button()
        tipPar = New KBot.Controls.KBotToolTip(components)
        tlpRoot.SuspendLayout()
        tlpButoane.SuspendLayout()
        SuspendLayout()
        '
        ' tlpRoot
        '
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpRoot.Controls.Add(lblIntro, 0, 0)
        tlpRoot.Controls.Add(txtParola, 0, 1)
        tlpRoot.Controls.Add(tlpButoane, 0, 2)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(12)
        tlpRoot.RowCount = 3
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 36.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Absolute, 44.0F))
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpRoot.TabIndex = 0
        '
        ' lblIntro
        '
        lblIntro.Dock = DockStyle.Fill
        lblIntro.Name = "lblIntro"
        lblIntro.TabIndex = 0
        lblIntro.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtParola
        '
        txtParola.BackColor = Color.Transparent
        txtParola.Dock = DockStyle.Fill
        txtParola.Margin = New Padding(0)
        txtParola.Name = "txtParola"
        txtParola.PlaceholderText = "parola de Admin a serverului"
        txtParola.TabIndex = 1
        txtParola.TextPadding = New Padding(12, 0, 12, 0)
        txtParola.UseSystemPasswordChar = True
        '
        ' tlpButoane
        '
        tlpButoane.ColumnCount = 3
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpButoane.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110.0F))
        tlpButoane.Controls.Add(btnRenunta, 0, 0)
        tlpButoane.Controls.Add(btnOk, 2, 0)
        tlpButoane.Dock = DockStyle.Fill
        tlpButoane.Name = "tlpButoane"
        tlpButoane.Padding = New Padding(0, 10, 0, 0)
        tlpButoane.RowCount = 1
        tlpButoane.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpButoane.TabIndex = 2
        '
        ' btnRenunta
        '
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Fill
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.TabIndex = 0
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnOk
        '
        btnOk.Dock = DockStyle.Fill
        btnOk.Margin = New Padding(0)
        btnOk.Name = "btnOk"
        btnOk.TabIndex = 1
        btnOk.Text = "Continuă"
        btnOk.UseVisualStyleBackColor = True
        '
        ' tipPar
        '
        tipPar.SetToolTipHeader(txtParola, "Parola Admin")
        tipPar.SetToolTipText(txtParola, "Parola contului de administrare al serverului MariaDB. Se folosește doar în această rulare; nu se salvează nicăieri.")
        '
        ' AvcAdminPasswordForm
        '
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(96.0F, 96.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(440, 150)
        Controls.Add(tlpRoot)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "AvcAdminPasswordForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Parola Admin"
        tlpRoot.ResumeLayout(False)
        tlpButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
