Imports System.Windows.Forms
Imports System.Drawing

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ThrottleCustomDialog
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
        pnlInfo = New Panel()
        lblInfo = New Label()
        tlyFields = New TableLayoutPanel()
        lblDownload = New Label()
        nudDownload = New NumericUpDown()
        lblDownloadUnit = New Label()
        lblUpload = New Label()
        nudUpload = New NumericUpDown()
        lblUploadUnit = New Label()
        lblLatency = New Label()
        nudLatency = New NumericUpDown()
        lblLatencyUnit = New Label()
        pnlButtons = New Panel()
        btnCancel = New Button()
        btnOk = New Button()
        pnlInfo.SuspendLayout()
        tlyFields.SuspendLayout()
        CType(nudDownload, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(nudUpload, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(nudLatency, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlInfo
        ' 
        pnlInfo.BackColor = Color.FromArgb(CByte(230), CByte(244), CByte(255))
        pnlInfo.Controls.Add(lblInfo)
        pnlInfo.Dock = DockStyle.Top
        pnlInfo.Location = New Point(0, 0)
        pnlInfo.Margin = New Padding(4)
        pnlInfo.Name = "pnlInfo"
        pnlInfo.Padding = New Padding(18, 12, 18, 12)
        pnlInfo.Size = New Size(412, 96)
        pnlInfo.TabIndex = 0
        ' 
        ' lblInfo
        ' 
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Font = New Font("Segoe UI", 9F)
        lblInfo.ForeColor = Color.FromArgb(CByte(30), CByte(80), CByte(140))
        lblInfo.Location = New Point(18, 12)
        lblInfo.Margin = New Padding(4, 0, 4, 0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(376, 72)
        lblInfo.TabIndex = 0
        lblInfo.Text = "Completează vitezele în KB/s și latența în ms." & vbCrLf & "Lasă 0 pentru a nu limita acea valoare."
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyFields
        ' 
        tlyFields.ColumnCount = 3
        tlyFields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyFields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 195F))
        tlyFields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFields.Controls.Add(lblDownload, 0, 0)
        tlyFields.Controls.Add(nudDownload, 1, 0)
        tlyFields.Controls.Add(lblDownloadUnit, 2, 0)
        tlyFields.Controls.Add(lblUpload, 0, 1)
        tlyFields.Controls.Add(nudUpload, 1, 1)
        tlyFields.Controls.Add(lblUploadUnit, 2, 1)
        tlyFields.Controls.Add(lblLatency, 0, 2)
        tlyFields.Controls.Add(nudLatency, 1, 2)
        tlyFields.Controls.Add(lblLatencyUnit, 2, 2)
        tlyFields.Dock = DockStyle.Fill
        tlyFields.Location = New Point(0, 96)
        tlyFields.Margin = New Padding(0)
        tlyFields.Name = "tlyFields"
        tlyFields.RowCount = 4
        tlyFields.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyFields.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyFields.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyFields.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFields.Size = New Size(412, 144)
        tlyFields.TabIndex = 1
        ' 
        ' lblDownload
        ' 
        lblDownload.Dock = DockStyle.Fill
        lblDownload.Font = New Font("Segoe UI", 9.5F)
        lblDownload.Location = New Point(0, 0)
        lblDownload.Margin = New Padding(0)
        lblDownload.Name = "lblDownload"
        lblDownload.Size = New Size(130, 44)
        lblDownload.TabIndex = 0
        lblDownload.Text = "Download:"
        lblDownload.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' nudDownload
        ' 
        nudDownload.Dock = DockStyle.Fill
        nudDownload.Font = New Font("Segoe UI", 10F)
        nudDownload.Location = New Point(136, 6)
        nudDownload.Margin = New Padding(6)
        nudDownload.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        nudDownload.Name = "nudDownload"
        nudDownload.Size = New Size(183, 34)
        nudDownload.TabIndex = 1
        nudDownload.Value = New Decimal(New Integer() {1500, 0, 0, 0})
        ' 
        ' lblDownloadUnit
        ' 
        lblDownloadUnit.Dock = DockStyle.Fill
        lblDownloadUnit.Font = New Font("Segoe UI", 9F)
        lblDownloadUnit.ForeColor = Color.Gray
        lblDownloadUnit.Location = New Point(331, 0)
        lblDownloadUnit.Margin = New Padding(6, 0, 0, 0)
        lblDownloadUnit.Name = "lblDownloadUnit"
        lblDownloadUnit.Size = New Size(81, 44)
        lblDownloadUnit.TabIndex = 2
        lblDownloadUnit.Text = "KB/s"
        lblDownloadUnit.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblUpload
        ' 
        lblUpload.Dock = DockStyle.Fill
        lblUpload.Font = New Font("Segoe UI", 9.5F)
        lblUpload.Location = New Point(0, 44)
        lblUpload.Margin = New Padding(0)
        lblUpload.Name = "lblUpload"
        lblUpload.Size = New Size(130, 44)
        lblUpload.TabIndex = 3
        lblUpload.Text = "Upload:"
        lblUpload.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' nudUpload
        ' 
        nudUpload.Dock = DockStyle.Fill
        nudUpload.Font = New Font("Segoe UI", 10F)
        nudUpload.Location = New Point(136, 50)
        nudUpload.Margin = New Padding(6)
        nudUpload.Maximum = New Decimal(New Integer() {100000, 0, 0, 0})
        nudUpload.Name = "nudUpload"
        nudUpload.Size = New Size(183, 34)
        nudUpload.TabIndex = 4
        nudUpload.Value = New Decimal(New Integer() {750, 0, 0, 0})
        ' 
        ' lblUploadUnit
        ' 
        lblUploadUnit.Dock = DockStyle.Fill
        lblUploadUnit.Font = New Font("Segoe UI", 9F)
        lblUploadUnit.ForeColor = Color.Gray
        lblUploadUnit.Location = New Point(331, 44)
        lblUploadUnit.Margin = New Padding(6, 0, 0, 0)
        lblUploadUnit.Name = "lblUploadUnit"
        lblUploadUnit.Size = New Size(81, 44)
        lblUploadUnit.TabIndex = 5
        lblUploadUnit.Text = "KB/s"
        lblUploadUnit.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblLatency
        ' 
        lblLatency.Dock = DockStyle.Fill
        lblLatency.Font = New Font("Segoe UI", 9.5F)
        lblLatency.Location = New Point(0, 88)
        lblLatency.Margin = New Padding(0)
        lblLatency.Name = "lblLatency"
        lblLatency.Size = New Size(130, 44)
        lblLatency.TabIndex = 6
        lblLatency.Text = "Latență:"
        lblLatency.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' nudLatency
        ' 
        nudLatency.Dock = DockStyle.Fill
        nudLatency.Font = New Font("Segoe UI", 10F)
        nudLatency.Location = New Point(136, 94)
        nudLatency.Margin = New Padding(6)
        nudLatency.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        nudLatency.Name = "nudLatency"
        nudLatency.Size = New Size(183, 34)
        nudLatency.TabIndex = 7
        nudLatency.Value = New Decimal(New Integer() {100, 0, 0, 0})
        ' 
        ' lblLatencyUnit
        ' 
        lblLatencyUnit.Dock = DockStyle.Fill
        lblLatencyUnit.Font = New Font("Segoe UI", 9F)
        lblLatencyUnit.ForeColor = Color.Gray
        lblLatencyUnit.Location = New Point(331, 88)
        lblLatencyUnit.Margin = New Padding(6, 0, 0, 0)
        lblLatencyUnit.Name = "lblLatencyUnit"
        lblLatencyUnit.Size = New Size(81, 44)
        lblLatencyUnit.TabIndex = 8
        lblLatencyUnit.Text = "ms"
        lblLatencyUnit.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlButtons
        ' 
        pnlButtons.Controls.Add(btnCancel)
        pnlButtons.Controls.Add(btnOk)
        pnlButtons.Dock = DockStyle.Bottom
        pnlButtons.Location = New Point(0, 240)
        pnlButtons.Margin = New Padding(4)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Padding = New Padding(12)
        pnlButtons.Size = New Size(412, 78)
        pnlButtons.TabIndex = 2
        ' 
        ' btnCancel
        ' 
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Right
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.Font = New Font("Segoe UI", 9F)
        btnCancel.Location = New Point(100, 12)
        btnCancel.Margin = New Padding(0)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(150, 54)
        btnCancel.TabIndex = 1
        btnCancel.Text = "Renunță"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnOk
        ' 
        btnOk.BackColor = Color.FromArgb(CByte(46), CByte(125), CByte(50))
        btnOk.Dock = DockStyle.Right
        btnOk.FlatAppearance.BorderSize = 0
        btnOk.FlatStyle = FlatStyle.Flat
        btnOk.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
        btnOk.ForeColor = Color.White
        btnOk.Location = New Point(250, 12)
        btnOk.Margin = New Padding(0)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(150, 54)
        btnOk.TabIndex = 0
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = False
        ' 
        ' ThrottleCustomDialog
        ' 
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnCancel
        ClientSize = New Size(412, 318)
        Controls.Add(tlyFields)
        Controls.Add(pnlInfo)
        Controls.Add(pnlButtons)
        FormBorderStyle = FormBorderStyle.FixedDialog
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "ThrottleCustomDialog"
        StartPosition = FormStartPosition.CenterParent
        Text = "Throttle rețea — configurare custom"
        pnlInfo.ResumeLayout(False)
        tlyFields.ResumeLayout(False)
        CType(nudDownload, System.ComponentModel.ISupportInitialize).EndInit()
        CType(nudUpload, System.ComponentModel.ISupportInitialize).EndInit()
        CType(nudLatency, System.ComponentModel.ISupportInitialize).EndInit()
        pnlButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlInfo As Panel
    Friend WithEvents lblInfo As Label
    Friend WithEvents tlyFields As TableLayoutPanel
    Friend WithEvents lblDownload As Label
    Friend WithEvents nudDownload As NumericUpDown
    Friend WithEvents lblDownloadUnit As Label
    Friend WithEvents lblUpload As Label
    Friend WithEvents nudUpload As NumericUpDown
    Friend WithEvents lblUploadUnit As Label
    Friend WithEvents lblLatency As Label
    Friend WithEvents nudLatency As NumericUpDown
    Friend WithEvents lblLatencyUnit As Label
    Friend WithEvents pnlButtons As Panel
    Friend WithEvents btnOk As Button
    Friend WithEvents btnCancel As Button

End Class