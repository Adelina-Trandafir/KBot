Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CertificateSelectionForm
    Inherits KBot.Theming.KBotThemedForm

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
        lstCertificates = New ListBox()
        pnlBottom = New Panel()
        btnResendOnly = New Button()
        btnSelect = New Button()
        lblTitle = New Label()
        pnlTop = New Panel()
        btnClose = New Button()
        btnRefresh = New Button()
        pnlBody = New Panel()
        pnlBottom.SuspendLayout()
        pnlTop.SuspendLayout()
        pnlBody.SuspendLayout()
        SuspendLayout()
        ' 
        ' lstCertificates
        ' 
        lstCertificates.BorderStyle = BorderStyle.None
        lstCertificates.Dock = DockStyle.Fill
        lstCertificates.DrawMode = DrawMode.OwnerDrawVariable
        lstCertificates.Font = New System.Drawing.Font("Calibri", 10.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        lstCertificates.FormattingEnabled = True
        lstCertificates.IntegralHeight = False
        lstCertificates.ItemHeight = 70
        lstCertificates.Location = New System.Drawing.Point(10, 10)
        lstCertificates.Name = "lstCertificates"
        lstCertificates.Size = New System.Drawing.Size(564, 360)
        lstCertificates.TabIndex = 1
        ' 
        ' pnlBottom
        ' 
        pnlBottom.BackColor = Drawing.Color.WhiteSmoke
        pnlBottom.Controls.Add(btnResendOnly)
        pnlBottom.Controls.Add(btnSelect)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New System.Drawing.Point(0, 425)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Size = New System.Drawing.Size(584, 40)
        pnlBottom.TabIndex = 2
        ' 
        ' btnResendOnly
        ' 
        btnResendOnly.BackColor = Drawing.Color.FromArgb(CByte(180), CByte(110), CByte(20))
        btnResendOnly.Dock = DockStyle.Left
        btnResendOnly.FlatAppearance.BorderSize = 0
        btnResendOnly.FlatStyle = FlatStyle.Flat
        btnResendOnly.Font = New System.Drawing.Font("Calibri", 9.0F)
        btnResendOnly.ForeColor = Drawing.Color.White
        btnResendOnly.Location = New System.Drawing.Point(0, 0)
        btnResendOnly.Name = "btnResendOnly"
        btnResendOnly.Size = New System.Drawing.Size(122, 40)
        btnResendOnly.TabIndex = 2
        btnResendOnly.Text = "Fără Token"
        btnResendOnly.UseVisualStyleBackColor = False
        ' 
        ' btnSelect
        ' 
        btnSelect.BackColor = Drawing.Color.FromArgb(CByte(0), CByte(120), CByte(215))
        btnSelect.Dock = DockStyle.Right
        btnSelect.FlatAppearance.BorderSize = 0
        btnSelect.FlatStyle = FlatStyle.Flat
        btnSelect.Font = New System.Drawing.Font("Calibri", 9.0F)
        btnSelect.ForeColor = Drawing.Color.White
        btnSelect.Location = New System.Drawing.Point(484, 0)
        btnSelect.Name = "btnSelect"
        btnSelect.Size = New System.Drawing.Size(100, 40)
        btnSelect.TabIndex = 0
        btnSelect.Text = "Confirmă"
        btnSelect.UseVisualStyleBackColor = False
        ' 
        ' lblTitle
        ' 
        lblTitle.BackColor = Drawing.Color.WhiteSmoke
        lblTitle.Dock = DockStyle.Fill
        lblTitle.Font = New System.Drawing.Font("Calibri", 12.0F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        lblTitle.ForeColor = Drawing.SystemColors.ActiveCaptionText
        lblTitle.Location = New System.Drawing.Point(10, 0)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New System.Drawing.Size(564, 45)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Selectează un Certificat"
        lblTitle.TextAlign = Drawing.ContentAlignment.MiddleCenter
        ' 
        ' pnlTop
        ' 
        pnlTop.Controls.Add(btnClose)
        pnlTop.Controls.Add(btnRefresh)
        pnlTop.Controls.Add(lblTitle)
        pnlTop.Dock = DockStyle.Top
        pnlTop.Location = New System.Drawing.Point(0, 0)
        pnlTop.Margin = New Padding(0)
        pnlTop.Name = "pnlTop"
        pnlTop.Padding = New Padding(10, 0, 10, 0)
        pnlTop.Size = New System.Drawing.Size(584, 45)
        pnlTop.TabIndex = 3
        ' 
        ' btnClose
        ' 
        btnClose.BackColor = Drawing.Color.WhiteSmoke
        btnClose.BackgroundImageLayout = ImageLayout.Center
        btnClose.Dock = DockStyle.Right
        btnClose.FlatAppearance.BorderSize = 0
        btnClose.FlatAppearance.MouseDownBackColor = Drawing.Color.FromArgb(CByte(192), CByte(255), CByte(192))
        btnClose.FlatAppearance.MouseOverBackColor = Drawing.Color.FromArgb(CByte(192), CByte(255), CByte(192))
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.Image = My.Resources.Resources.Icojam_Blueberry_Basic_Close_delete_2_32
        btnClose.Location = New System.Drawing.Point(540, 0)
        btnClose.Name = "btnClose"
        btnClose.Size = New System.Drawing.Size(34, 45)
        btnClose.TabIndex = 2
        btnClose.UseVisualStyleBackColor = False
        ' 
        ' btnRefresh
        ' 
        btnRefresh.BackColor = Drawing.Color.WhiteSmoke
        btnRefresh.Dock = DockStyle.Left
        btnRefresh.FlatAppearance.BorderSize = 0
        btnRefresh.FlatAppearance.MouseDownBackColor = Drawing.Color.FromArgb(CByte(192), CByte(255), CByte(192))
        btnRefresh.FlatAppearance.MouseOverBackColor = Drawing.Color.FromArgb(CByte(192), CByte(255), CByte(192))
        btnRefresh.FlatStyle = FlatStyle.Flat
        btnRefresh.Image = CType(resources.GetObject("btnRefresh.Image"), Drawing.Image)
        btnRefresh.Location = New System.Drawing.Point(10, 0)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New System.Drawing.Size(34, 45)
        btnRefresh.TabIndex = 1
        btnRefresh.UseVisualStyleBackColor = False
        ' 
        ' pnlBody
        ' 
        pnlBody.Controls.Add(lstCertificates)
        pnlBody.Dock = DockStyle.Fill
        pnlBody.Location = New System.Drawing.Point(0, 45)
        pnlBody.Margin = New Padding(0)
        pnlBody.Name = "pnlBody"
        pnlBody.Padding = New Padding(10)
        pnlBody.Size = New System.Drawing.Size(584, 380)
        pnlBody.TabIndex = 4
        ' 
        ' CertificateSelectionForm
        ' 
        AcceptButton = btnSelect
        AutoScaleMode = AutoScaleMode.None
        BackColor = Drawing.SystemColors.Window
        ClientSize = New System.Drawing.Size(584, 465)
        Controls.Add(pnlBody)
        Controls.Add(pnlTop)
        Controls.Add(pnlBottom)
        Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
        Margin = New Padding(3, 4, 3, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "CertificateSelectionForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Securitate"
        TopMost = True
        pnlBottom.ResumeLayout(False)
        pnlTop.ResumeLayout(False)
        pnlBody.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
    Friend WithEvents lstCertificates As ListBox
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents btnSelect As Button
    Friend WithEvents btnResendOnly As Button
    Friend WithEvents lblTitle As Label
    Friend WithEvents pnlTop As Panel
    Friend WithEvents btnRefresh As Button
    Friend WithEvents pnlBody As Panel
    Friend WithEvents btnClose As Button
End Class