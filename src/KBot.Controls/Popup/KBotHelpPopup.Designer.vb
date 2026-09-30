<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotHelpPopup
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
        pnlCautare = New KBotHelpSearchPanel()
        btnAjutorComplet = New Button()
        SuspendLayout()
        '
        ' pnlCautare
        '
        pnlCautare.Dock = DockStyle.Fill
        pnlCautare.Location = New Point(1, 1)
        pnlCautare.Margin = New Padding(0)
        pnlCautare.Name = "pnlCautare"
        pnlCautare.Size = New Size(418, 452)
        pnlCautare.TabIndex = 0
        '
        ' btnAjutorComplet
        '
        btnAjutorComplet.Dock = DockStyle.Bottom
        btnAjutorComplet.FlatStyle = FlatStyle.Flat
        btnAjutorComplet.Location = New Point(1, 453)
        btnAjutorComplet.Margin = New Padding(0)
        btnAjutorComplet.Name = "btnAjutorComplet"
        btnAjutorComplet.Size = New Size(418, 36)
        btnAjutorComplet.TabIndex = 1
        btnAjutorComplet.TabStop = False
        btnAjutorComplet.Text = "Deschide ajutorul complet (F1)"
        tips.SetToolTipHeader(btnAjutorComplet, "Ajutorul complet")
        tips.SetToolTipText(btnAjutorComplet, "Fereastra de ajutor, cu tot cuprinsul, deschisă la pagina despre ce ai pe ecran. F1 face același lucru din orice fereastră.")
        btnAjutorComplet.UseVisualStyleBackColor = False
        '
        ' KBotHelpPopup
        '
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CenterOnScreen = False
        ClientSize = New Size(420, 490)
        ControlBox = False
        Controls.Add(pnlCautare)
        Controls.Add(btnAjutorComplet)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        MaximizeBox = False
        MinimizeBox = False
        Name = "KBotHelpPopup"
        Padding = New Padding(1)
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Ajutor K-BOT"
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCautare As KBotHelpSearchPanel
    Friend WithEvents btnAjutorComplet As Button
End Class
