<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetupPromptForm
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    Friend WithEvents lblIntro As System.Windows.Forms.Label
    Friend WithEvents lblBaseUrl As System.Windows.Forms.Label
    Friend WithEvents txtBaseUrl As System.Windows.Forms.TextBox
    Friend WithEvents lblApiKey As System.Windows.Forms.Label
    Friend WithEvents txtApiKey As System.Windows.Forms.TextBox
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.lblIntro = New System.Windows.Forms.Label()
        Me.lblBaseUrl = New System.Windows.Forms.Label()
        Me.txtBaseUrl = New System.Windows.Forms.TextBox()
        Me.lblApiKey = New System.Windows.Forms.Label()
        Me.txtApiKey = New System.Windows.Forms.TextBox()
        Me.btnOk = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblIntro
        '
        Me.lblIntro.Location = New System.Drawing.Point(12, 12)
        Me.lblIntro.Name = "lblIntro"
        Me.lblIntro.Size = New System.Drawing.Size(436, 44)
        Me.lblIntro.Text = "Configurare API K-BOT (o singură dată pe acest calculator). Valorile se salvează în variabilele de mediu Windows — nu într-un fișier."
        '
        'lblBaseUrl
        '
        Me.lblBaseUrl.AutoSize = True
        Me.lblBaseUrl.Location = New System.Drawing.Point(12, 64)
        Me.lblBaseUrl.Name = "lblBaseUrl"
        Me.lblBaseUrl.Text = "URL API (ex. http://server:5008/):"
        '
        'txtBaseUrl
        '
        Me.txtBaseUrl.Location = New System.Drawing.Point(12, 84)
        Me.txtBaseUrl.Name = "txtBaseUrl"
        Me.txtBaseUrl.Size = New System.Drawing.Size(436, 23)
        '
        'lblApiKey
        '
        Me.lblApiKey.AutoSize = True
        Me.lblApiKey.Location = New System.Drawing.Point(12, 118)
        Me.lblApiKey.Name = "lblApiKey"
        Me.lblApiKey.Text = "Cheie API (X-Api-Key):"
        '
        'txtApiKey
        '
        Me.txtApiKey.Location = New System.Drawing.Point(12, 138)
        Me.txtApiKey.Name = "txtApiKey"
        Me.txtApiKey.Size = New System.Drawing.Size(436, 23)
        Me.txtApiKey.UseSystemPasswordChar = True
        '
        'btnOk
        '
        Me.btnOk.Location = New System.Drawing.Point(272, 180)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(84, 28)
        Me.btnOk.Text = "Salvează"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnCancel.Location = New System.Drawing.Point(364, 180)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(84, 28)
        Me.btnCancel.Text = "Renunță"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'SetupPromptForm
        '
        Me.AcceptButton = Me.btnOk
        Me.CancelButton = Me.btnCancel
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(460, 224)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.txtApiKey)
        Me.Controls.Add(Me.lblApiKey)
        Me.Controls.Add(Me.txtBaseUrl)
        Me.Controls.Add(Me.lblBaseUrl)
        Me.Controls.Add(Me.lblIntro)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "SetupPromptForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "K-BOT Setup — Configurare API"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

End Class
