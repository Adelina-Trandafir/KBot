<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForexeFooterView
    Inherits System.Windows.Forms.UserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing Then
                Dezleaga()
                If components IsNot Nothing Then components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        btnConectare = New Button()
        lblConexiune = New Label()
        pbProgress = New ProgressBar()
        lblCert = New Label()
        lblStatus = New Label()
        btnExtinde = New Button()
        SuspendLayout()
        '
        ' btnConectare
        '
        btnConectare.Dock = DockStyle.Left
        btnConectare.FlatStyle = FlatStyle.Flat
        btnConectare.Location = New Point(0, 0)
        btnConectare.Name = "btnConectare"
        btnConectare.Size = New Size(140, 60)
        btnConectare.TabIndex = 0
        btnConectare.Text = "Conectare"
        btnConectare.UseVisualStyleBackColor = True
        '
        ' lblConexiune — pastila de stare a conexiunii (înlocuiește vechiul lblForexe).
        '
        lblConexiune.Dock = DockStyle.Left
        lblConexiune.Location = New Point(140, 0)
        lblConexiune.Name = "lblConexiune"
        lblConexiune.Size = New Size(180, 60)
        lblConexiune.TabIndex = 1
        lblConexiune.Text = "● Forexe: neconectat"
        lblConexiune.TextAlign = ContentAlignment.MiddleCenter
        '
        ' pbProgress
        '
        pbProgress.Dock = DockStyle.Left
        pbProgress.Location = New Point(320, 0)
        pbProgress.Margin = New Padding(4)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(180, 60)
        pbProgress.TabIndex = 2
        '
        ' lblCert
        '
        lblCert.AutoEllipsis = True
        lblCert.Dock = DockStyle.Left
        lblCert.Location = New Point(500, 0)
        lblCert.Name = "lblCert"
        lblCert.Size = New Size(220, 60)
        lblCert.TabIndex = 3
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnExtinde — deschide consola FOREXE.
        '
        btnExtinde.Dock = DockStyle.Right
        btnExtinde.FlatStyle = FlatStyle.Flat
        btnExtinde.Location = New Point(760, 0)
        btnExtinde.Name = "btnExtinde"
        btnExtinde.Size = New Size(60, 60)
        btnExtinde.TabIndex = 5
        btnExtinde.Text = "▲"
        btnExtinde.UseVisualStyleBackColor = True
        '
        ' lblStatus — ULTIMA linie de stare, nu jurnalul (acela e în consolă).
        '
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(720, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(40, 60)
        lblStatus.TabIndex = 4
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        ' ForexeFooterView — copiii se adaugă în ordine INVERSĂ de dock:
        ' lblStatus (Fill) primul, apoi btnExtinde (Right), apoi cei ancorați la stânga.
        '
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblStatus)
        Controls.Add(btnExtinde)
        Controls.Add(lblCert)
        Controls.Add(pbProgress)
        Controls.Add(lblConexiune)
        Controls.Add(btnConectare)
        Name = "ForexeFooterView"
        Size = New Size(820, 60)
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnConectare As Button
    Friend WithEvents lblConexiune As Label
    Friend WithEvents pbProgress As ProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnExtinde As Button
End Class
