Imports KBot.Controls

' The standalone log window (slice 0031-04, hollowed out in 0072-01): a caption bar and the
' «Jurnal» page of the settings window docked underneath. Everything the viewer does lives
' in SetariJurnalView; this form only gives it a window for the harness and the «Jurnale»
' launcher. All controls are declared HERE (docs/kbot-forms-ui-convention.md); coordinates
' are in the 144 dpi the form was authored at and AutoScaleDimensions carries the same stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LogViewerForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        pnlRoot = New Panel()
        jurnal = New SetariJurnalView()
        capBar = New KBotCaptionBar()
        pnlRoot.SuspendLayout()
        SuspendLayout()
        '
        ' pnlRoot
        '
        pnlRoot.Controls.Add(jurnal)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 2)
        pnlRoot.Margin = New Padding(0)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1173, 869)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        '
        ' jurnal
        '
        jurnal.Dock = DockStyle.Fill
        jurnal.Location = New Point(0, 70)
        jurnal.Margin = New Padding(0)
        jurnal.Name = "jurnal"
        jurnal.Padding = New Padding(11, 13, 11, 13)
        jurnal.Size = New Size(1173, 799)
        jurnal.TabIndex = 1
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1173, 70)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Jurnale"
        '
        ' LogViewerForm
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1175, 873)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(1086, 767)
        Name = "LogViewerForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Jurnale"
        pnlRoot.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents jurnal As SetariJurnalView
End Class
