Imports System.Windows.Forms
Imports System.Drawing

' RootNamespace = AvacontPush, so no Namespace block here.
Partial Class Form1
    Inherits Form

    Private components As System.ComponentModel.IContainer

    ' Layout is entirely TableLayoutPanel-based and docked - no absolute positioning.
    Friend WithEvents tlpRoot As TableLayoutPanel
    Friend WithEvents tlpInputs As TableLayoutPanel
    Friend WithEvents tlpLocal As TableLayoutPanel
    Friend WithEvents tlpRemote As TableLayoutPanel
    Friend WithEvents tlpConn As TableLayoutPanel
    Friend WithEvents tlpActions As TableLayoutPanel
    Friend WithEvents tlpStatus As TableLayoutPanel
    Friend WithEvents splitMain As SplitContainer

    Friend WithEvents lblLocalRoot As Label
    Friend WithEvents txtLocalRoot As TextBox
    Friend WithEvents btnBrowseLocal As Button
    Friend WithEvents lblRemoteRoot As Label
    Friend WithEvents txtRemoteRoot As TextBox

    Friend WithEvents lblHost As Label
    Friend WithEvents txtHost As TextBox
    Friend WithEvents lblPort As Label
    Friend WithEvents txtPort As TextBox
    Friend WithEvents lblUser As Label
    Friend WithEvents txtUser As TextBox
    Friend WithEvents lblPassword As Label
    Friend WithEvents txtPassword As TextBox

    Friend WithEvents btnScan As Button
    Friend WithEvents chkRestart As CheckBox
    Friend WithEvents btnPush As Button

    Friend WithEvents tvFiles As TreeView
    Friend WithEvents rtbOutput As RichTextBox

    Friend WithEvents pbProgress As ProgressBar
    Friend WithEvents lblStatus As Label

    Friend WithEvents dlgFolder As FolderBrowserDialog

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        tlpRoot = New TableLayoutPanel()
        tlpInputs = New TableLayoutPanel()
        tlpLocal = New TableLayoutPanel()
        lblLocalRoot = New Label()
        txtLocalRoot = New TextBox()
        btnBrowseLocal = New Button()
        tlpRemote = New TableLayoutPanel()
        lblRemoteRoot = New Label()
        txtRemoteRoot = New TextBox()
        tlpConn = New TableLayoutPanel()
        lblHost = New Label()
        txtHost = New TextBox()
        lblPort = New Label()
        txtPort = New TextBox()
        lblUser = New Label()
        txtUser = New TextBox()
        lblPassword = New Label()
        txtPassword = New TextBox()
        tlpActions = New TableLayoutPanel()
        btnScan = New Button()
        chkRestart = New CheckBox()
        btnPush = New Button()
        splitMain = New SplitContainer()
        tvFiles = New TreeView()
        rtbOutput = New RichTextBox()
        tlpStatus = New TableLayoutPanel()
        pbProgress = New ProgressBar()
        lblStatus = New Label()
        dlgFolder = New FolderBrowserDialog()
        tlpRoot.SuspendLayout()
        tlpInputs.SuspendLayout()
        tlpLocal.SuspendLayout()
        tlpRemote.SuspendLayout()
        tlpConn.SuspendLayout()
        tlpActions.SuspendLayout()
        CType(splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        tlpStatus.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlpRoot
        ' 
        tlpRoot.ColumnCount = 1
        tlpRoot.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpRoot.Controls.Add(tlpInputs, 0, 0)
        tlpRoot.Controls.Add(splitMain, 0, 1)
        tlpRoot.Controls.Add(tlpStatus, 0, 2)
        tlpRoot.Dock = DockStyle.Fill
        tlpRoot.Location = New Point(0, 0)
        tlpRoot.Name = "tlpRoot"
        tlpRoot.Padding = New Padding(6)
        tlpRoot.RowCount = 3
        tlpRoot.RowStyles.Add(New RowStyle())
        tlpRoot.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpRoot.RowStyles.Add(New RowStyle())
        tlpRoot.Size = New Size(900, 680)
        tlpRoot.TabIndex = 0
        ' 
        ' tlpInputs
        ' 
        tlpInputs.AutoSize = True
        tlpInputs.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpInputs.ColumnCount = 1
        tlpInputs.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpInputs.Controls.Add(tlpLocal, 0, 0)
        tlpInputs.Controls.Add(tlpRemote, 0, 1)
        tlpInputs.Controls.Add(tlpConn, 0, 2)
        tlpInputs.Controls.Add(tlpActions, 0, 3)
        tlpInputs.Dock = DockStyle.Fill
        tlpInputs.Location = New Point(9, 9)
        tlpInputs.Name = "tlpInputs"
        tlpInputs.RowCount = 4
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.Size = New Size(882, 146)
        tlpInputs.TabIndex = 0
        ' 
        ' tlpLocal
        ' 
        tlpLocal.AutoSize = True
        tlpLocal.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpLocal.ColumnCount = 3
        tlpLocal.ColumnStyles.Add(New ColumnStyle())
        tlpLocal.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpLocal.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlpLocal.Controls.Add(lblLocalRoot, 0, 0)
        tlpLocal.Controls.Add(txtLocalRoot, 1, 0)
        tlpLocal.Controls.Add(btnBrowseLocal, 2, 0)
        tlpLocal.Dock = DockStyle.Fill
        tlpLocal.Location = New Point(0, 0)
        tlpLocal.Margin = New Padding(0)
        tlpLocal.Name = "tlpLocal"
        tlpLocal.RowCount = 1
        tlpLocal.RowStyles.Add(New RowStyle())
        tlpLocal.Size = New Size(882, 37)
        tlpLocal.TabIndex = 0
        ' 
        ' lblLocalRoot
        ' 
        lblLocalRoot.Anchor = AnchorStyles.Left
        lblLocalRoot.AutoSize = True
        lblLocalRoot.Location = New Point(3, 6)
        lblLocalRoot.Name = "lblLocalRoot"
        lblLocalRoot.Size = New Size(137, 25)
        lblLocalRoot.TabIndex = 0
        lblLocalRoot.Text = "Rădăcină locală:"
        ' 
        ' txtLocalRoot
        ' 
        txtLocalRoot.Dock = DockStyle.Fill
        txtLocalRoot.Location = New Point(146, 3)
        txtLocalRoot.Name = "txtLocalRoot"
        txtLocalRoot.Size = New Size(613, 31)
        txtLocalRoot.TabIndex = 1
        ' 
        ' btnBrowseLocal
        ' 
        btnBrowseLocal.Dock = DockStyle.Fill
        btnBrowseLocal.Location = New Point(762, 0)
        btnBrowseLocal.Margin = New Padding(0)
        btnBrowseLocal.Name = "btnBrowseLocal"
        btnBrowseLocal.Size = New Size(120, 37)
        btnBrowseLocal.TabIndex = 2
        btnBrowseLocal.Text = "Răsfoire..."
        ' 
        ' tlpRemote
        ' 
        tlpRemote.AutoSize = True
        tlpRemote.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpRemote.ColumnCount = 2
        tlpRemote.ColumnStyles.Add(New ColumnStyle())
        tlpRemote.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpRemote.Controls.Add(lblRemoteRoot, 0, 0)
        tlpRemote.Controls.Add(txtRemoteRoot, 1, 0)
        tlpRemote.Dock = DockStyle.Fill
        tlpRemote.Location = New Point(0, 37)
        tlpRemote.Margin = New Padding(0)
        tlpRemote.Name = "tlpRemote"
        tlpRemote.RowCount = 1
        tlpRemote.RowStyles.Add(New RowStyle())
        tlpRemote.Size = New Size(882, 37)
        tlpRemote.TabIndex = 1
        ' 
        ' lblRemoteRoot
        ' 
        lblRemoteRoot.Anchor = AnchorStyles.Left
        lblRemoteRoot.AutoSize = True
        lblRemoteRoot.Location = New Point(3, 6)
        lblRemoteRoot.Name = "lblRemoteRoot"
        lblRemoteRoot.Size = New Size(139, 25)
        lblRemoteRoot.TabIndex = 0
        lblRemoteRoot.Text = "Rădăcină server:"
        ' 
        ' txtRemoteRoot
        ' 
        txtRemoteRoot.Dock = DockStyle.Fill
        txtRemoteRoot.Location = New Point(148, 3)
        txtRemoteRoot.Name = "txtRemoteRoot"
        txtRemoteRoot.Size = New Size(731, 31)
        txtRemoteRoot.TabIndex = 1
        ' 
        ' tlpConn
        ' 
        tlpConn.AutoSize = True
        tlpConn.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpConn.ColumnCount = 8
        tlpConn.ColumnStyles.Add(New ColumnStyle())
        tlpConn.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 34F))
        tlpConn.ColumnStyles.Add(New ColumnStyle())
        tlpConn.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70F))
        tlpConn.ColumnStyles.Add(New ColumnStyle())
        tlpConn.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33F))
        tlpConn.ColumnStyles.Add(New ColumnStyle())
        tlpConn.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33F))
        tlpConn.Controls.Add(lblHost, 0, 0)
        tlpConn.Controls.Add(txtHost, 1, 0)
        tlpConn.Controls.Add(lblPort, 2, 0)
        tlpConn.Controls.Add(txtPort, 3, 0)
        tlpConn.Controls.Add(lblUser, 4, 0)
        tlpConn.Controls.Add(txtUser, 5, 0)
        tlpConn.Controls.Add(lblPassword, 6, 0)
        tlpConn.Controls.Add(txtPassword, 7, 0)
        tlpConn.Dock = DockStyle.Fill
        tlpConn.Location = New Point(0, 74)
        tlpConn.Margin = New Padding(0)
        tlpConn.Name = "tlpConn"
        tlpConn.RowCount = 1
        tlpConn.RowStyles.Add(New RowStyle())
        tlpConn.Size = New Size(882, 37)
        tlpConn.TabIndex = 2
        ' 
        ' lblHost
        ' 
        lblHost.Anchor = AnchorStyles.Left
        lblHost.AutoSize = True
        lblHost.Location = New Point(3, 6)
        lblHost.Name = "lblHost"
        lblHost.Size = New Size(54, 25)
        lblHost.TabIndex = 0
        lblHost.Text = "Host:"
        ' 
        ' txtHost
        ' 
        txtHost.Dock = DockStyle.Fill
        txtHost.Location = New Point(63, 3)
        txtHost.Name = "txtHost"
        txtHost.Size = New Size(176, 31)
        txtHost.TabIndex = 1
        ' 
        ' lblPort
        ' 
        lblPort.Anchor = AnchorStyles.Left
        lblPort.AutoSize = True
        lblPort.Location = New Point(245, 6)
        lblPort.Name = "lblPort"
        lblPort.Size = New Size(48, 25)
        lblPort.TabIndex = 2
        lblPort.Text = "Port:"
        ' 
        ' txtPort
        ' 
        txtPort.Dock = DockStyle.Fill
        txtPort.Location = New Point(299, 3)
        txtPort.Name = "txtPort"
        txtPort.Size = New Size(64, 31)
        txtPort.TabIndex = 3
        ' 
        ' lblUser
        ' 
        lblUser.Anchor = AnchorStyles.Left
        lblUser.AutoSize = True
        lblUser.Location = New Point(369, 6)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(86, 25)
        lblUser.TabIndex = 4
        lblUser.Text = "Utilizator:"
        ' 
        ' txtUser
        ' 
        txtUser.Dock = DockStyle.Fill
        txtUser.Location = New Point(461, 3)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(170, 31)
        txtUser.TabIndex = 5
        ' 
        ' lblPassword
        ' 
        lblPassword.Anchor = AnchorStyles.Left
        lblPassword.AutoSize = True
        lblPassword.Location = New Point(637, 6)
        lblPassword.Name = "lblPassword"
        lblPassword.Size = New Size(64, 25)
        lblPassword.TabIndex = 6
        lblPassword.Text = "Parolă:"
        ' 
        ' txtPassword
        ' 
        txtPassword.Dock = DockStyle.Fill
        txtPassword.Location = New Point(707, 3)
        txtPassword.Name = "txtPassword"
        txtPassword.Size = New Size(172, 31)
        txtPassword.TabIndex = 7
        txtPassword.UseSystemPasswordChar = True
        ' 
        ' tlpActions
        ' 
        tlpActions.AutoSize = True
        tlpActions.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpActions.ColumnCount = 3
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlpActions.Controls.Add(btnScan, 0, 0)
        tlpActions.Controls.Add(chkRestart, 1, 0)
        tlpActions.Controls.Add(btnPush, 2, 0)
        tlpActions.Dock = DockStyle.Fill
        tlpActions.Location = New Point(0, 111)
        tlpActions.Margin = New Padding(0)
        tlpActions.Name = "tlpActions"
        tlpActions.RowCount = 1
        tlpActions.RowStyles.Add(New RowStyle())
        tlpActions.Size = New Size(882, 35)
        tlpActions.TabIndex = 3
        ' 
        ' btnScan
        ' 
        btnScan.Dock = DockStyle.Fill
        btnScan.Location = New Point(0, 0)
        btnScan.Margin = New Padding(0)
        btnScan.Name = "btnScan"
        btnScan.Size = New Size(140, 35)
        btnScan.TabIndex = 0
        btnScan.Text = "Scanează"
        ' 
        ' chkRestart
        ' 
        chkRestart.Anchor = AnchorStyles.Left
        chkRestart.AutoSize = True
        chkRestart.Location = New Point(143, 3)
        chkRestart.Name = "chkRestart"
        chkRestart.Size = New Size(284, 29)
        chkRestart.TabIndex = 1
        chkRestart.Text = "Repornește serviciul după push"
        ' 
        ' btnPush
        ' 
        btnPush.Dock = DockStyle.Fill
        btnPush.Location = New Point(762, 0)
        btnPush.Margin = New Padding(0)
        btnPush.Name = "btnPush"
        btnPush.Size = New Size(120, 35)
        btnPush.TabIndex = 2
        btnPush.Text = "Trimite"
        ' 
        ' splitMain
        ' 
        splitMain.Dock = DockStyle.Fill
        splitMain.Location = New Point(9, 161)
        splitMain.Name = "splitMain"
        splitMain.Orientation = Orientation.Horizontal
        ' 
        ' splitMain.Panel1
        ' 
        splitMain.Panel1.Controls.Add(tvFiles)
        splitMain.Panel1MinSize = 80
        ' 
        ' splitMain.Panel2
        ' 
        splitMain.Panel2.Controls.Add(rtbOutput)
        splitMain.Panel2MinSize = 80
        splitMain.Size = New Size(882, 453)
        splitMain.SplitterDistance = 261
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 1
        ' 
        ' tvFiles
        ' 
        tvFiles.CheckBoxes = True
        tvFiles.Dock = DockStyle.Fill
        tvFiles.Font = New Font("Segoe UI", 9F)
        tvFiles.HideSelection = False
        tvFiles.Location = New Point(0, 0)
        tvFiles.Name = "tvFiles"
        tvFiles.ShowNodeToolTips = True
        tvFiles.Size = New Size(882, 261)
        tvFiles.TabIndex = 0
        ' 
        ' rtbOutput
        ' 
        rtbOutput.DetectUrls = False
        rtbOutput.Dock = DockStyle.Fill
        rtbOutput.Font = New Font("Consolas", 9F)
        rtbOutput.Location = New Point(0, 0)
        rtbOutput.Name = "rtbOutput"
        rtbOutput.ReadOnly = True
        rtbOutput.Size = New Size(882, 186)
        rtbOutput.TabIndex = 0
        rtbOutput.Text = ""
        rtbOutput.WordWrap = False
        ' 
        ' tlpStatus
        ' 
        tlpStatus.AutoSize = True
        tlpStatus.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpStatus.ColumnCount = 1
        tlpStatus.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpStatus.Controls.Add(pbProgress, 0, 0)
        tlpStatus.Controls.Add(lblStatus, 0, 1)
        tlpStatus.Dock = DockStyle.Fill
        tlpStatus.Location = New Point(9, 620)
        tlpStatus.Name = "tlpStatus"
        tlpStatus.RowCount = 2
        tlpStatus.RowStyles.Add(New RowStyle(SizeType.Absolute, 26F))
        tlpStatus.RowStyles.Add(New RowStyle())
        tlpStatus.Size = New Size(882, 51)
        tlpStatus.TabIndex = 2
        ' 
        ' pbProgress
        ' 
        pbProgress.Dock = DockStyle.Fill
        pbProgress.Location = New Point(3, 3)
        pbProgress.Margin = New Padding(3, 3, 3, 4)
        pbProgress.Name = "pbProgress"
        pbProgress.Size = New Size(876, 19)
        pbProgress.TabIndex = 0
        ' 
        ' lblStatus
        ' 
        lblStatus.Anchor = AnchorStyles.Left
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(3, 26)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(77, 25)
        lblStatus.TabIndex = 1
        lblStatus.Text = "Pregătit."
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(900, 680)
        Controls.Add(tlpRoot)
        MinimumSize = New Size(780, 560)
        Name = "Form1"
        StartPosition = FormStartPosition.CenterScreen
        Text = "AVACONT Push"
        tlpRoot.ResumeLayout(False)
        tlpRoot.PerformLayout()
        tlpInputs.ResumeLayout(False)
        tlpInputs.PerformLayout()
        tlpLocal.ResumeLayout(False)
        tlpLocal.PerformLayout()
        tlpRemote.ResumeLayout(False)
        tlpRemote.PerformLayout()
        tlpConn.ResumeLayout(False)
        tlpConn.PerformLayout()
        tlpActions.ResumeLayout(False)
        tlpActions.PerformLayout()
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        tlpStatus.ResumeLayout(False)
        tlpStatus.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
