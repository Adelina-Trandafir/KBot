<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForexeFooterView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        components = New ComponentModel.Container()
        tips = New Global.KBot.Controls.KBotToolTip(components)
        btnExtinde = New Button()
        btnIstoric = New Button()
        lblConexiune = New Label()
        pbProgress = New Controls.KBotProgressBar()
        lblCert = New Label()
        lblStatus = New Label()
        SuspendLayout()
        ' 
        ' btnExtinde
        ' 
        btnExtinde.Dock = DockStyle.Right
        btnExtinde.FlatStyle = FlatStyle.Flat
        btnExtinde.Location = New Point(783, 0)
        btnExtinde.Name = "btnExtinde"
        btnExtinde.Size = New Size(37, 31)
        btnExtinde.TabIndex = 6
        btnExtinde.Text = "▲"
        tips.SetToolTipHeader(btnExtinde, "Consolă")
        tips.SetToolTipText(btnExtinde, "Deschide consola FOREXE: progres detaliat, jurnal și descărcări.")
        btnExtinde.UseVisualStyleBackColor = True
        ' 
        ' btnIstoric
        ' 
        btnIstoric.Dock = DockStyle.Right
        btnIstoric.FlatStyle = FlatStyle.Flat
        btnIstoric.Location = New Point(745, 0)
        btnIstoric.Name = "btnIstoric"
        btnIstoric.Size = New Size(38, 31)
        btnIstoric.TabIndex = 5
        btnIstoric.Text = "⟲"
        tips.SetToolTipHeader(btnIstoric, "Istoric")
        tips.SetToolTipText(btnIstoric, "Istoricul acțiunilor duse prin FOREXE în această sesiune," & vbLf & "cu rezultatul și jurnalul fiecăreia.")
        btnIstoric.UseVisualStyleBackColor = True
        ' 
        ' lblConexiune
        ' 
        lblConexiune.Dock = DockStyle.Left
        lblConexiune.Location = New Point(0, 0)
        lblConexiune.Name = "lblConexiune"
        lblConexiune.Size = New Size(106, 31)
        lblConexiune.TabIndex = 1
        lblConexiune.Text = "● Neconectat"
        lblConexiune.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = DockStyle.Left
        pbProgress.Location = New Point(106, 0)
        pbProgress.Margin = New Padding(4, 4, 4, 4)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(142, 31)
        pbProgress.TabIndex = 2
        ' 
        ' lblCert
        ' 
        lblCert.AutoEllipsis = True
        lblCert.Dock = DockStyle.Left
        lblCert.Location = New Point(248, 0)
        lblCert.Name = "lblCert"
        lblCert.Size = New Size(164, 31)
        lblCert.TabIndex = 3
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(412, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(333, 31)
        lblStatus.TabIndex = 4
        lblStatus.Text = "În așteptare..."
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ForexeFooterView
        ' 
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblStatus)
        Controls.Add(btnIstoric)
        Controls.Add(btnExtinde)
        Controls.Add(lblCert)
        Controls.Add(pbProgress)
        Controls.Add(lblConexiune)
        Name = "ForexeFooterView"
        Size = New Size(820, 31)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents lblConexiune As Label
    Friend WithEvents pbProgress As Global.KBot.Controls.KBotProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnIstoric As Button
    Friend WithEvents btnExtinde As Button
End Class
