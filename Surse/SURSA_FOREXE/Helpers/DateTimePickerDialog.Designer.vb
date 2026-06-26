Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DateTimePickerDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlMain = New Panel()
        lblTitle = New Label()
        btnOK = New Button()
        btnCancel = New Button()
        dtp = New DateTimePicker()
        pnlMain.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlMain
        ' 
        pnlMain.Controls.Add(dtp)
        pnlMain.Controls.Add(lblTitle)
        pnlMain.Controls.Add(btnOK)
        pnlMain.Controls.Add(btnCancel)
        pnlMain.Dock = DockStyle.Fill
        pnlMain.Location = New System.Drawing.Point(0, 0)
        pnlMain.Margin = New Padding(4, 5, 4, 5)
        pnlMain.Name = "pnlMain"
        pnlMain.Padding = New Padding(17, 20, 17, 20)
        pnlMain.Size = New System.Drawing.Size(365, 177)
        pnlMain.TabIndex = 0
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Location = New System.Drawing.Point(21, 25)
        lblTitle.Margin = New Padding(4, 0, 4, 0)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New System.Drawing.Size(269, 25)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Selectează data și ora de început"
        ' 
        ' btnOK
        ' 
        btnOK.Location = New System.Drawing.Point(239, 116)
        btnOK.Margin = New Padding(4, 5, 4, 5)
        btnOK.Name = "btnOK"
        btnOK.Size = New System.Drawing.Size(107, 45)
        btnOK.TabIndex = 2
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' btnCancel
        ' 
        btnCancel.Location = New System.Drawing.Point(21, 116)
        btnCancel.Margin = New Padding(4, 5, 4, 5)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New System.Drawing.Size(107, 45)
        btnCancel.TabIndex = 3
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' dtp
        ' 
        dtp.Location = New System.Drawing.Point(21, 63)
        dtp.Name = "dtp"
        dtp.Size = New System.Drawing.Size(325, 31)
        dtp.TabIndex = 4
        ' 
        ' DateTimePickerDialog
        ' 
        AcceptButton = btnOK
        AutoScaleDimensions = New System.Drawing.SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancel
        ClientSize = New System.Drawing.Size(365, 177)
        Controls.Add(pnlMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        Margin = New Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimizeBox = False
        Name = "DateTimePickerDialog"
        StartPosition = FormStartPosition.CenterParent
        Text = "ForexeSNM - Selectare dată"
        pnlMain.ResumeLayout(False)
        pnlMain.PerformLayout()
        ResumeLayout(False)

    End Sub

    Friend WithEvents pnlMain As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents dtp As DateTimePicker

End Class