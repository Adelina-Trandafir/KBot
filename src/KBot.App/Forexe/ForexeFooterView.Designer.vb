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
        pbProgress = New KBot.Controls.KBotProgressBar()
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
        btnConectare.Margin = New Padding(4, 5, 4, 5)
        btnConectare.Name = "btnConectare"
        btnConectare.Size = New Size(200, 52)
        btnConectare.TabIndex = 0
        btnConectare.Text = "Conectare"
        btnConectare.UseVisualStyleBackColor = True
        ' 
        ' lblConexiune
        ' 
        lblConexiune.Dock = DockStyle.Left
        lblConexiune.Location = New Point(200, 0)
        lblConexiune.Margin = New Padding(4, 0, 4, 0)
        lblConexiune.Name = "lblConexiune"
        lblConexiune.Size = New Size(151, 52)
        lblConexiune.TabIndex = 1
        lblConexiune.Text = "● Neconectat"
        lblConexiune.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = DockStyle.Left
        pbProgress.Location = New Point(351, 0)
        pbProgress.Margin = New Padding(6, 7, 6, 7)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(257, 52)
        pbProgress.TabIndex = 2
        ' 
        ' lblCert
        ' 
        lblCert.AutoEllipsis = True
        lblCert.Dock = DockStyle.Left
        lblCert.Location = New Point(608, 0)
        lblCert.Margin = New Padding(4, 0, 4, 0)
        lblCert.Name = "lblCert"
        lblCert.Size = New Size(227, 52)
        lblCert.TabIndex = 3
        lblCert.Text = "Certificat: —"
        lblCert.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(835, 0)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(250, 52)
        lblStatus.TabIndex = 4
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnExtinde
        ' 
        btnExtinde.Dock = DockStyle.Right
        btnExtinde.FlatStyle = FlatStyle.Flat
        btnExtinde.Location = New Point(1085, 0)
        btnExtinde.Margin = New Padding(4, 5, 4, 5)
        btnExtinde.Name = "btnExtinde"
        btnExtinde.Size = New Size(86, 52)
        btnExtinde.TabIndex = 5
        btnExtinde.Text = "▲"
        btnExtinde.UseVisualStyleBackColor = True
        ' 
        ' ForexeFooterView
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(lblStatus)
        Controls.Add(btnExtinde)
        Controls.Add(lblCert)
        Controls.Add(pbProgress)
        Controls.Add(lblConexiune)
        Controls.Add(btnConectare)
        Margin = New Padding(4, 5, 4, 5)
        Name = "ForexeFooterView"
        Size = New Size(1171, 52)
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnConectare As Button
    Friend WithEvents lblConexiune As Label
    Friend WithEvents pbProgress As KBot.Controls.KBotProgressBar
    Friend WithEvents lblCert As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnExtinde As Button
End Class
