<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ResendForm
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
        splMain = New System.Windows.Forms.SplitContainer()
        lbxFiles = New System.Windows.Forms.ListBox()
        lblFilesHeader = New System.Windows.Forms.Label()
        btnChangeFolder = New System.Windows.Forms.Button()
        tcDetails = New System.Windows.Forms.TabControl()
        tpTree = New System.Windows.Forms.TabPage()
        tpJson = New System.Windows.Forms.TabPage()
        pnlBottom = New System.Windows.Forms.Panel()
        lblStatus = New System.Windows.Forms.Label()
        btnResend = New System.Windows.Forms.Button()
        btnClose = New System.Windows.Forms.Button()
        CType(splMain, ComponentModel.ISupportInitialize).BeginInit()
        splMain.Panel1.SuspendLayout()
        splMain.Panel2.SuspendLayout()
        splMain.SuspendLayout()
        tcDetails.SuspendLayout()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        ' 
        ' splMain
        ' 
        splMain.Dock = System.Windows.Forms.DockStyle.Fill
        splMain.Location = New System.Drawing.Point(0, 0)
        splMain.Margin = New System.Windows.Forms.Padding(4)
        splMain.Name = "splMain"
        ' 
        ' splMain.Panel1
        ' 
        splMain.Panel1.Controls.Add(lbxFiles)
        splMain.Panel1.Controls.Add(lblFilesHeader)
        splMain.Panel1.Controls.Add(btnChangeFolder)
        ' 
        ' splMain.Panel2
        ' 
        splMain.Panel2.Controls.Add(tcDetails)
        splMain.Size = New System.Drawing.Size(1200, 676)
        splMain.SplitterDistance = 305
        splMain.SplitterWidth = 6
        splMain.TabIndex = 0
        ' 
        ' lbxFiles
        ' 
        lbxFiles.BorderStyle = System.Windows.Forms.BorderStyle.None
        lbxFiles.Dock = System.Windows.Forms.DockStyle.Fill
        lbxFiles.Font = New System.Drawing.Font("Consolas", 9F)
        lbxFiles.IntegralHeight = False
        lbxFiles.ItemHeight = 22
        lbxFiles.Location = New System.Drawing.Point(0, 35)
        lbxFiles.Margin = New System.Windows.Forms.Padding(4)
        lbxFiles.Name = "lbxFiles"
        lbxFiles.Size = New System.Drawing.Size(305, 599)
        lbxFiles.TabIndex = 1
        ' 
        ' lblFilesHeader
        ' 
        lblFilesHeader.Dock = System.Windows.Forms.DockStyle.Top
        lblFilesHeader.Font = New System.Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        lblFilesHeader.Location = New System.Drawing.Point(0, 0)
        lblFilesHeader.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblFilesHeader.Name = "lblFilesHeader"
        lblFilesHeader.Padding = New System.Windows.Forms.Padding(8, 8, 0, 0)
        lblFilesHeader.Size = New System.Drawing.Size(305, 35)
        lblFilesHeader.TabIndex = 0
        lblFilesHeader.Text = "📁 Fișiere Resend"
        ' 
        ' btnChangeFolder
        ' 
        btnChangeFolder.Dock = System.Windows.Forms.DockStyle.Bottom
        btnChangeFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnChangeFolder.Font = New System.Drawing.Font("Segoe UI", 9F)
        btnChangeFolder.Location = New System.Drawing.Point(0, 634)
        btnChangeFolder.Margin = New System.Windows.Forms.Padding(4)
        btnChangeFolder.Name = "btnChangeFolder"
        btnChangeFolder.Size = New System.Drawing.Size(305, 42)
        btnChangeFolder.TabIndex = 2
        btnChangeFolder.Text = "📂  Schimbă folder..."
        btnChangeFolder.UseVisualStyleBackColor = False
        ' 
        ' tcDetails
        ' 
        tcDetails.Controls.Add(tpTree)
        tcDetails.Controls.Add(tpJson)
        tcDetails.Dock = System.Windows.Forms.DockStyle.Fill
        tcDetails.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        tcDetails.Location = New System.Drawing.Point(0, 0)
        tcDetails.Margin = New System.Windows.Forms.Padding(4)
        tcDetails.Name = "tcDetails"
        tcDetails.SelectedIndex = 0
        tcDetails.Size = New System.Drawing.Size(889, 676)
        tcDetails.TabIndex = 0
        ' 
        ' tpTree
        ' 
        tpTree.Location = New System.Drawing.Point(4, 34)
        tpTree.Margin = New System.Windows.Forms.Padding(4)
        tpTree.Name = "tpTree"
        tpTree.Size = New System.Drawing.Size(881, 638)
        tpTree.TabIndex = 0
        tpTree.Text = "🌲  Structură"
        ' 
        ' tpJson
        ' 
        tpJson.Location = New System.Drawing.Point(4, 34)
        tpJson.Margin = New System.Windows.Forms.Padding(4)
        tpJson.Name = "tpJson"
        tpJson.Size = New System.Drawing.Size(1035, 638)
        tpJson.TabIndex = 1
        tpJson.Text = "📄  JSON raw"
        ' 
        ' pnlBottom
        ' 
        pnlBottom.Controls.Add(lblStatus)
        pnlBottom.Controls.Add(btnResend)
        pnlBottom.Controls.Add(btnClose)
        pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlBottom.Location = New System.Drawing.Point(0, 676)
        pnlBottom.Margin = New System.Windows.Forms.Padding(4)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Padding = New System.Windows.Forms.Padding(8, 6, 8, 6)
        pnlBottom.Size = New System.Drawing.Size(1200, 52)
        pnlBottom.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill
        lblStatus.Font = New System.Drawing.Font("Segoe UI", 9F)
        lblStatus.Location = New System.Drawing.Point(8, 6)
        lblStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New System.Drawing.Size(802, 40)
        lblStatus.TabIndex = 0
        lblStatus.Text = "Selectează un fișier din listă."
        lblStatus.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' btnResend
        ' 
        btnResend.BackColor = Drawing.Color.FromArgb(CByte(46), CByte(125), CByte(50))
        btnResend.Dock = System.Windows.Forms.DockStyle.Right
        btnResend.Enabled = False
        btnResend.FlatAppearance.BorderSize = 0
        btnResend.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnResend.Font = New System.Drawing.Font("Segoe UI", 9F, Drawing.FontStyle.Bold)
        btnResend.ForeColor = Drawing.Color.White
        btnResend.Location = New System.Drawing.Point(810, 6)
        btnResend.Margin = New System.Windows.Forms.Padding(4)
        btnResend.Name = "btnResend"
        btnResend.Size = New System.Drawing.Size(262, 40)
        btnResend.TabIndex = 2
        btnResend.Text = "🔁  Retrimite mesajul"
        btnResend.UseVisualStyleBackColor = False
        ' 
        ' btnClose
        ' 
        btnClose.Dock = System.Windows.Forms.DockStyle.Right
        btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnClose.Font = New System.Drawing.Font("Segoe UI", 9F)
        btnClose.Location = New System.Drawing.Point(1072, 6)
        btnClose.Margin = New System.Windows.Forms.Padding(4)
        btnClose.Name = "btnClose"
        btnClose.Size = New System.Drawing.Size(120, 40)
        btnClose.TabIndex = 3
        btnClose.Text = "✕  Închide"
        btnClose.UseVisualStyleBackColor = False
        ' 
        ' ResendForm
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(10F, 25F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(1200, 728)
        Controls.Add(splMain)
        Controls.Add(pnlBottom)
        Margin = New System.Windows.Forms.Padding(4)
        MinimumSize = New System.Drawing.Size(770, 511)
        Name = "ResendForm"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "📤  Resend Manager"
        splMain.Panel1.ResumeLayout(False)
        splMain.Panel2.ResumeLayout(False)
        CType(splMain, ComponentModel.ISupportInitialize).EndInit()
        splMain.ResumeLayout(False)
        tcDetails.ResumeLayout(False)
        pnlBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents splMain As System.Windows.Forms.SplitContainer
    Friend WithEvents lblFilesHeader As System.Windows.Forms.Label
    Friend WithEvents lbxFiles As System.Windows.Forms.ListBox
    Friend WithEvents btnChangeFolder As System.Windows.Forms.Button
    Friend WithEvents tcDetails As System.Windows.Forms.TabControl
    Friend WithEvents tpTree As System.Windows.Forms.TabPage
    Friend WithEvents tpJson As System.Windows.Forms.TabPage
    Friend WithEvents pnlBottom As System.Windows.Forms.Panel
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnResend As System.Windows.Forms.Button
    Friend WithEvents btnClose As System.Windows.Forms.Button

End Class
