Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CertificateSelectionForm
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CertificateSelectionForm))
        lblTitle = New Label()
        lstCertificates = New ListBox()
        pnlBottom = New Panel()
        btnCancel = New Button()
        btnSelect = New Button()
        btnResendOnly = New Button()
        grpPin = New GroupBox()
        lblPinInfo = New Label()
        txtPin = New TextBox()
        lblPinTitle = New Label()
        pnlBottom.SuspendLayout()
        grpPin.SuspendLayout()
        SuspendLayout()
        '
        ' lblTitle
        '
        lblTitle.AutoSize = True
        lblTitle.Font = New System.Drawing.Font("Segoe UI Semilight", 14.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lblTitle.ForeColor = Drawing.Color.FromArgb(CByte(64), CByte(64), CByte(64))
        lblTitle.Location = New System.Drawing.Point(12, 15)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New System.Drawing.Size(253, 32)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Selectează un Certificat"
        '
        ' lstCertificates
        '
        lstCertificates.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lstCertificates.BorderStyle = BorderStyle.None
        lstCertificates.DrawMode = DrawMode.OwnerDrawVariable
        lstCertificates.Font = New System.Drawing.Font("Segoe UI", 10.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lstCertificates.FormattingEnabled = True
        lstCertificates.IntegralHeight = False
        lstCertificates.ItemHeight = 70
        lstCertificates.Location = New System.Drawing.Point(18, 60)
        lstCertificates.Name = "lstCertificates"
        lstCertificates.Size = New System.Drawing.Size(546, 215)
        lstCertificates.TabIndex = 1
        '
        ' pnlBottom
        '
        pnlBottom.BackColor = Drawing.Color.WhiteSmoke
        pnlBottom.Controls.Add(btnResendOnly)
        pnlBottom.Controls.Add(btnCancel)
        pnlBottom.Controls.Add(btnSelect)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New System.Drawing.Point(0, 385)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Size = New System.Drawing.Size(584, 80)
        pnlBottom.TabIndex = 2
        '
        ' btnResendOnly
        '
        btnResendOnly.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnResendOnly.BackColor = Drawing.Color.FromArgb(CByte(180), CByte(110), CByte(20))
        btnResendOnly.FlatAppearance.BorderSize = 0
        btnResendOnly.FlatStyle = FlatStyle.Flat
        btnResendOnly.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        btnResendOnly.ForeColor = Drawing.Color.White
        btnResendOnly.Location = New System.Drawing.Point(18, 22)
        btnResendOnly.Name = "btnResendOnly"
        btnResendOnly.Size = New System.Drawing.Size(180, 35)
        btnResendOnly.TabIndex = 2
        btnResendOnly.Text = "Fără Token (Resend Only)"
        btnResendOnly.UseVisualStyleBackColor = False
        '
        ' btnCancel
        '
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.FlatAppearance.BorderColor = Drawing.Color.Silver
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        btnCancel.Location = New System.Drawing.Point(354, 22)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New System.Drawing.Size(100, 35)
        btnCancel.TabIndex = 1
        btnCancel.Text = "Anulează"
        btnCancel.UseVisualStyleBackColor = True
        '
        ' btnSelect
        '
        btnSelect.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnSelect.BackColor = Drawing.Color.FromArgb(CByte(0), CByte(120), CByte(215))
        btnSelect.FlatAppearance.BorderSize = 0
        btnSelect.FlatStyle = FlatStyle.Flat
        btnSelect.Font = New System.Drawing.Font("Segoe UI Semibold", 9.0F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        btnSelect.ForeColor = Drawing.Color.White
        btnSelect.Location = New System.Drawing.Point(464, 22)
        btnSelect.Name = "btnSelect"
        btnSelect.Size = New System.Drawing.Size(100, 35)
        btnSelect.TabIndex = 0
        btnSelect.Text = "Confirmă"
        btnSelect.UseVisualStyleBackColor = False
        '
        ' grpPin
        '
        grpPin.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        grpPin.Controls.Add(lblPinInfo)
        grpPin.Controls.Add(txtPin)
        grpPin.Controls.Add(lblPinTitle)
        grpPin.Location = New System.Drawing.Point(18, 291)
        grpPin.Name = "grpPin"
        grpPin.Size = New System.Drawing.Size(546, 80)
        grpPin.TabIndex = 3
        grpPin.TabStop = False
        '
        ' lblPinInfo
        '
        lblPinInfo.AutoSize = True
        lblPinInfo.Font = New System.Drawing.Font("Segoe UI", 8.25F, Drawing.FontStyle.Italic, Drawing.GraphicsUnit.Point, CByte(0))
        lblPinInfo.ForeColor = Drawing.Color.Gray
        lblPinInfo.Location = New System.Drawing.Point(340, 35)
        lblPinInfo.Name = "lblPinInfo"
        lblPinInfo.Size = New System.Drawing.Size(162, 19)
        lblPinInfo.TabIndex = 2
        lblPinInfo.Text = "(Doar dacă este necesar)"
        '
        ' txtPin
        '
        txtPin.Font = New System.Drawing.Font("Segoe UI", 10.0F)
        txtPin.Location = New System.Drawing.Point(100, 30)
        txtPin.Name = "txtPin"
        txtPin.PasswordChar = "*"c
        txtPin.Size = New System.Drawing.Size(230, 30)
        txtPin.TabIndex = 1
        '
        ' lblPinTitle
        '
        lblPinTitle.AutoSize = True
        lblPinTitle.Font = New System.Drawing.Font("Segoe UI Semibold", 10.0F, Drawing.FontStyle.Bold)
        lblPinTitle.Location = New System.Drawing.Point(15, 33)
        lblPinTitle.Name = "lblPinTitle"
        lblPinTitle.Size = New System.Drawing.Size(92, 23)
        lblPinTitle.TabIndex = 0
        lblPinTitle.Text = "PIN Token:"
        '
        ' CertificateSelectionForm
        '
        AcceptButton = btnSelect
        AutoScaleDimensions = New System.Drawing.SizeF(8.0F, 20.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.White
        CancelButton = btnCancel
        ClientSize = New System.Drawing.Size(584, 465)
        Controls.Add(grpPin)
        Controls.Add(pnlBottom)
        Controls.Add(lstCertificates)
        Controls.Add(lblTitle)
        Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        FormBorderStyle = FormBorderStyle.FixedDialog
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New Padding(3, 4, 3, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "CertificateSelectionForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Securitate"
        TopMost = True
        pnlBottom.ResumeLayout(False)
        grpPin.ResumeLayout(False)
        grpPin.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblTitle As Label
    Friend WithEvents lstCertificates As ListBox
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnSelect As Button
    Friend WithEvents btnResendOnly As Button
    Friend WithEvents grpPin As GroupBox
    Friend WithEvents txtPin As TextBox
    Friend WithEvents lblPinTitle As Label
    Friend WithEvents lblPinInfo As Label
End Class