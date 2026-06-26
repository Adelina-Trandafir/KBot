<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CustomInputBox
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblPrompt = New System.Windows.Forms.Label()
        txtInput = New System.Windows.Forms.MaskedTextBox()
        btnCancel = New System.Windows.Forms.Button()
        btnOk = New System.Windows.Forms.Button()
        lblError = New System.Windows.Forms.Label()
        pnlContent = New System.Windows.Forms.Panel()
        pnlButtons = New System.Windows.Forms.Panel()
        pnlContent.SuspendLayout()
        pnlButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblPrompt
        ' 
        lblPrompt.AutoSize = True
        lblPrompt.Dock = System.Windows.Forms.DockStyle.Top
        lblPrompt.Font = New System.Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lblPrompt.Location = New System.Drawing.Point(20, 20)
        lblPrompt.MaximumSize = New System.Drawing.Size(460, 0)
        lblPrompt.Name = "lblPrompt"
        lblPrompt.Padding = New System.Windows.Forms.Padding(0, 0, 0, 10)
        lblPrompt.Size = New System.Drawing.Size(167, 33)
        lblPrompt.TabIndex = 0
        lblPrompt.Text = "Introduceți valoarea:"
        ' 
        ' txtInput
        ' 
        txtInput.Dock = System.Windows.Forms.DockStyle.Top
        txtInput.Font = New System.Drawing.Font("Segoe UI", 11.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        txtInput.Location = New System.Drawing.Point(20, 53)
        txtInput.Name = "txtInput"
        txtInput.Size = New System.Drawing.Size(444, 32)
        txtInput.TabIndex = 1
        ' 
        ' btnCancel
        ' 
        btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right
        btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnCancel.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        btnCancel.Location = New System.Drawing.Point(374, 13)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New System.Drawing.Size(90, 32)
        btnCancel.TabIndex = 4
        btnCancel.Text = "Anulează"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnOk
        ' 
        btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right
        btnOk.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        btnOk.Location = New System.Drawing.Point(278, 13)
        btnOk.Name = "btnOk"
        btnOk.Size = New System.Drawing.Size(90, 32)
        btnOk.TabIndex = 3
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        ' 
        ' lblError
        ' 
        lblError.AutoSize = True
        lblError.Dock = System.Windows.Forms.DockStyle.Top
        lblError.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        lblError.ForeColor = Drawing.Color.Crimson
        lblError.Location = New System.Drawing.Point(20, 85)
        lblError.Name = "lblError"
        lblError.Padding = New System.Windows.Forms.Padding(0, 5, 0, 0)
        lblError.Size = New System.Drawing.Size(120, 25)
        lblError.TabIndex = 2
        lblError.Text = "Mesaj de eroare"
        lblError.Visible = False
        ' 
        ' pnlContent
        ' 
        pnlContent.Controls.Add(lblError)
        pnlContent.Controls.Add(txtInput)
        pnlContent.Controls.Add(lblPrompt)
        pnlContent.Dock = System.Windows.Forms.DockStyle.Fill
        pnlContent.Location = New System.Drawing.Point(0, 0)
        pnlContent.Name = "pnlContent"
        pnlContent.Padding = New System.Windows.Forms.Padding(20)
        pnlContent.Size = New System.Drawing.Size(484, 184)
        pnlContent.TabIndex = 5
        ' 
        ' pnlButtons
        ' 
        pnlButtons.BackColor = Drawing.SystemColors.ControlLight
        pnlButtons.Controls.Add(btnOk)
        pnlButtons.Controls.Add(btnCancel)
        pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlButtons.Location = New System.Drawing.Point(0, 184)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Size = New System.Drawing.Size(484, 57)
        pnlButtons.TabIndex = 6
        ' 
        ' CustomInputBox
        ' 
        AcceptButton = btnOk
        AutoScaleDimensions = New System.Drawing.SizeF(8.0F, 20.0F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        BackColor = Drawing.Color.White
        CancelButton = btnCancel
        ClientSize = New System.Drawing.Size(484, 241)
        Controls.Add(pnlContent)
        Controls.Add(pnlButtons)
        Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "CustomInputBox"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Input"
        pnlContent.ResumeLayout(False)
        pnlContent.PerformLayout()
        pnlButtons.ResumeLayout(False)
        ResumeLayout(False)

    End Sub

    Friend WithEvents lblPrompt As System.Windows.Forms.Label
    Friend WithEvents txtInput As System.Windows.Forms.MaskedTextBox
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents lblError As System.Windows.Forms.Label
    Friend WithEvents pnlContent As System.Windows.Forms.Panel
    Friend WithEvents pnlButtons As System.Windows.Forms.Panel
End Class