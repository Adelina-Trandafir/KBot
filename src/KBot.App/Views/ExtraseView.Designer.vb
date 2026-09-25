Imports KBot.Controls

' The Extrase view (slice 0080-02): the statements of the angajament selected in the main tree.
' The body is ExtrasePanel (tree + grids + detail, shared with the «Extrase de cont» window);
' this control adds only the empty-state label. All controls are declared here
' (docs/kbot-forms-ui-convention.md); coordinates are in 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ExtraseView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        panel = New ExtrasePanel()
        lblEmpty = New Label()
        SuspendLayout()
        '
        ' panel
        '
        panel.Dock = DockStyle.Fill
        panel.Location = New Point(0, 0)
        panel.Margin = New Padding(0)
        panel.Name = "panel"
        panel.Size = New Size(986, 568)
        panel.TabIndex = 0
        '
        ' lblEmpty
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 568)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' ExtraseView
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(panel)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "ExtraseView"
        Size = New Size(986, 568)
        ResumeLayout(False)
    End Sub

    Friend WithEvents panel As ExtrasePanel
    Friend WithEvents lblEmpty As Label
End Class
