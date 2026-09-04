<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfFisierePage
    Inherits KBot.Theming.KBotThemedUserControl

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
        browser = New DdfFileBrowser()
        lblFisiereGol = New Label()
        SuspendLayout()
        '
        ' browser — lista PDF-urilor angajamentului. Adăugat PRIMUL, deci stă în fața
        ' etichetei goale de dedesubt (ordinea Z, nu andocarea).
        '
        browser.Dock = DockStyle.Fill
        browser.Location = New Point(0, 0)
        browser.Name = "browser"
        browser.Size = New Size(849, 488)
        browser.TabIndex = 0
        '
        ' lblFisiereGol — plasa de dedesubt, când browserul lipsește
        '
        lblFisiereGol.Dock = DockStyle.Fill
        lblFisiereGol.Font = New Font("Segoe UI", 10F)
        lblFisiereGol.Location = New Point(0, 0)
        lblFisiereGol.Name = "lblFisiereGol"
        lblFisiereGol.Size = New Size(849, 488)
        lblFisiereGol.TabIndex = 1
        lblFisiereGol.Text = "Selectați un angajament din arbore."
        lblFisiereGol.TextAlign = ContentAlignment.MiddleCenter
        '
        ' DdfFisierePage
        '
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(browser)
        Controls.Add(lblFisiereGol)
        Margin = New Padding(4, 5, 4, 5)
        Name = "DdfFisierePage"
        Size = New Size(849, 488)
        ResumeLayout(False)
    End Sub

    Friend WithEvents browser As DdfFileBrowser
    Friend WithEvents lblFisiereGol As Label
End Class
