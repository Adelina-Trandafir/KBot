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

    ' Schema sync: the same SSH connection, running routes.schema_sync on the server.
    Friend WithEvents tlpSchema As TableLayoutPanel
    Friend WithEvents tlpSchemaActions As TableLayoutPanel
    Friend WithEvents btnSchemaTargets As Button
    Friend WithEvents lblSchemaMode As Label
    Friend WithEvents cmbSchemaMode As ComboBox
    Friend WithEvents lblRemotePython As Label
    Friend WithEvents txtRemotePython As TextBox
    Friend WithEvents btnSchemaView As Button
    Friend WithEvents btnSchemaRun As Button
    Friend WithEvents clbTargets As CheckedListBox
    Friend WithEvents lblSchemaHint As Label

    Friend WithEvents tabsMain As TabControl
    Friend WithEvents tabFiles As TabPage
    Friend WithEvents tabSchema As TabPage
    Friend WithEvents tabOnce As TabPage
    Friend WithEvents tlpOnce As TableLayoutPanel
    Friend WithEvents tlpOnceActions As TableLayoutPanel
    Friend WithEvents lblOnceName As Label
    Friend WithEvents txtOnceName As TextBox
    Friend WithEvents btnOnceTargets As Button
    Friend WithEvents clbOnceTargets As CheckedListBox
    Friend WithEvents btnOnceLoad As Button
    Friend WithEvents btnOnceStatus As Button
    Friend WithEvents btnOnceView As Button
    Friend WithEvents btnOnceRun As Button
    Friend WithEvents txtOnceSql As TextBox
    Friend WithEvents lblOnceHint As Label
    Friend WithEvents dlgOnceFile As OpenFileDialog
    Friend WithEvents tabUsers As TabPage
    Friend WithEvents tlpUsers As TableLayoutPanel
    Friend WithEvents tlpUsersActions As TableLayoutPanel
    Friend WithEvents lblApiUrl As Label
    Friend WithEvents txtApiUrl As TextBox
    Friend WithEvents lblApiKey As Label
    Friend WithEvents txtApiKey As TextBox
    Friend WithEvents btnUsersLoad As Button
    Friend WithEvents btnLoginReset As Button
    Friend WithEvents dgvUsers As DataGridView
    Friend WithEvents colUn As DataGridViewTextBoxColumn
    Friend WithEvents colDc As DataGridViewTextBoxColumn
    Friend WithEvents colUnitate As DataGridViewTextBoxColumn
    Friend WithEvents colRol As DataGridViewTextBoxColumn
    Friend WithEvents colLastSs As DataGridViewTextBoxColumn
    Friend WithEvents colFails As DataGridViewTextBoxColumn
    Friend WithEvents colBlocked As DataGridViewTextBoxColumn
    Friend WithEvents tlpFiles As TableLayoutPanel
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
        tlpSchema = New TableLayoutPanel()
        tlpSchemaActions = New TableLayoutPanel()
        btnSchemaTargets = New Button()
        lblSchemaMode = New Label()
        cmbSchemaMode = New ComboBox()
        lblRemotePython = New Label()
        txtRemotePython = New TextBox()
        btnSchemaView = New Button()
        btnSchemaRun = New Button()
        clbTargets = New CheckedListBox()
        lblSchemaHint = New Label()
        splitMain = New SplitContainer()
        tabsMain = New TabControl()
        tabFiles = New TabPage()
        tabSchema = New TabPage()
        tabOnce = New TabPage()
        tlpOnce = New TableLayoutPanel()
        tlpOnceActions = New TableLayoutPanel()
        lblOnceName = New Label()
        txtOnceName = New TextBox()
        btnOnceTargets = New Button()
        clbOnceTargets = New CheckedListBox()
        btnOnceLoad = New Button()
        btnOnceStatus = New Button()
        btnOnceView = New Button()
        btnOnceRun = New Button()
        txtOnceSql = New TextBox()
        lblOnceHint = New Label()
        dlgOnceFile = New OpenFileDialog()
        tabUsers = New TabPage()
        tlpUsers = New TableLayoutPanel()
        tlpUsersActions = New TableLayoutPanel()
        lblApiUrl = New Label()
        txtApiUrl = New TextBox()
        lblApiKey = New Label()
        txtApiKey = New TextBox()
        btnUsersLoad = New Button()
        btnLoginReset = New Button()
        dgvUsers = New DataGridView()
        colUn = New DataGridViewTextBoxColumn()
        colDc = New DataGridViewTextBoxColumn()
        colUnitate = New DataGridViewTextBoxColumn()
        colRol = New DataGridViewTextBoxColumn()
        colLastSs = New DataGridViewTextBoxColumn()
        colFails = New DataGridViewTextBoxColumn()
        colBlocked = New DataGridViewTextBoxColumn()
        tlpFiles = New TableLayoutPanel()
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
        tlpSchema.SuspendLayout()
        tlpSchemaActions.SuspendLayout()
        tabsMain.SuspendLayout()
        tabFiles.SuspendLayout()
        tabSchema.SuspendLayout()
        tabOnce.SuspendLayout()
        tlpOnce.SuspendLayout()
        tlpOnceActions.SuspendLayout()
        tabUsers.SuspendLayout()
        tlpUsers.SuspendLayout()
        tlpUsersActions.SuspendLayout()
        CType(dgvUsers, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpFiles.SuspendLayout()
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
        tlpInputs.Dock = DockStyle.Fill
        tlpInputs.Location = New Point(9, 9)
        tlpInputs.Name = "tlpInputs"
        tlpInputs.RowCount = 3
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.RowStyles.Add(New RowStyle())
        tlpInputs.Size = New Size(882, 111)
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
        tlpActions.Location = New Point(0, 0)
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
        splitMain.Panel1.Controls.Add(tabsMain)
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
        ' tabsMain
        ' 
        tabsMain.Controls.Add(tabFiles)
        tabsMain.Controls.Add(tabSchema)
        tabsMain.Controls.Add(tabOnce)
        tabsMain.Controls.Add(tabUsers)
        tabsMain.Dock = DockStyle.Fill
        tabsMain.Location = New Point(0, 0)
        tabsMain.Name = "tabsMain"
        tabsMain.SelectedIndex = 0
        tabsMain.Size = New Size(882, 261)
        tabsMain.TabIndex = 0
        ' 
        ' tabFiles
        ' 
        tabFiles.Controls.Add(tlpFiles)
        tabFiles.Location = New Point(4, 34)
        tabFiles.Name = "tabFiles"
        tabFiles.Padding = New Padding(6)
        tabFiles.Size = New Size(874, 223)
        tabFiles.TabIndex = 0
        tabFiles.Text = "Fișiere"
        tabFiles.UseVisualStyleBackColor = True
        ' 
        ' tlpFiles
        ' 
        tlpFiles.ColumnCount = 1
        tlpFiles.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpFiles.Controls.Add(tlpActions, 0, 0)
        tlpFiles.Controls.Add(tvFiles, 0, 1)
        tlpFiles.Dock = DockStyle.Fill
        tlpFiles.Location = New Point(6, 6)
        tlpFiles.Margin = New Padding(0)
        tlpFiles.Name = "tlpFiles"
        tlpFiles.RowCount = 2
        tlpFiles.RowStyles.Add(New RowStyle())
        tlpFiles.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpFiles.Size = New Size(862, 211)
        tlpFiles.TabIndex = 0
        ' 
        ' tabSchema
        ' 
        tabSchema.Controls.Add(tlpSchema)
        tabSchema.Location = New Point(4, 34)
        tabSchema.Name = "tabSchema"
        tabSchema.Padding = New Padding(6)
        tabSchema.Size = New Size(874, 223)
        tabSchema.TabIndex = 1
        tabSchema.Text = "Sincronizare schemă"
        tabSchema.UseVisualStyleBackColor = True
        ' 
        ' tlpSchema
        ' 
        tlpSchema.ColumnCount = 1
        tlpSchema.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpSchema.Controls.Add(tlpSchemaActions, 0, 0)
        tlpSchema.Controls.Add(clbTargets, 0, 1)
        tlpSchema.Controls.Add(lblSchemaHint, 0, 2)
        tlpSchema.Dock = DockStyle.Fill
        tlpSchema.Location = New Point(6, 6)
        tlpSchema.Margin = New Padding(0)
        tlpSchema.Name = "tlpSchema"
        tlpSchema.RowCount = 3
        tlpSchema.RowStyles.Add(New RowStyle())
        tlpSchema.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpSchema.RowStyles.Add(New RowStyle())
        tlpSchema.Size = New Size(862, 211)
        tlpSchema.TabIndex = 0
        ' 
        ' tlpSchemaActions
        ' 
        tlpSchemaActions.AutoSize = True
        tlpSchemaActions.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpSchemaActions.ColumnCount = 7
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle())
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle())
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlpSchemaActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlpSchemaActions.Controls.Add(btnSchemaTargets, 0, 0)
        tlpSchemaActions.Controls.Add(lblSchemaMode, 1, 0)
        tlpSchemaActions.Controls.Add(cmbSchemaMode, 2, 0)
        tlpSchemaActions.Controls.Add(lblRemotePython, 3, 0)
        tlpSchemaActions.Controls.Add(txtRemotePython, 4, 0)
        tlpSchemaActions.Controls.Add(btnSchemaView, 5, 0)
        tlpSchemaActions.Controls.Add(btnSchemaRun, 6, 0)
        tlpSchemaActions.Dock = DockStyle.Fill
        tlpSchemaActions.Location = New Point(0, 0)
        tlpSchemaActions.Margin = New Padding(0)
        tlpSchemaActions.Name = "tlpSchemaActions"
        tlpSchemaActions.RowCount = 1
        tlpSchemaActions.RowStyles.Add(New RowStyle())
        tlpSchemaActions.Size = New Size(862, 35)
        tlpSchemaActions.TabIndex = 0
        ' 
        ' btnSchemaTargets
        ' 
        btnSchemaTargets.Dock = DockStyle.Fill
        btnSchemaTargets.Location = New Point(0, 0)
        btnSchemaTargets.Margin = New Padding(0)
        btnSchemaTargets.Name = "btnSchemaTargets"
        btnSchemaTargets.Size = New Size(170, 35)
        btnSchemaTargets.TabIndex = 0
        btnSchemaTargets.Text = "Citește bazele"
        ' 
        ' lblSchemaMode
        ' 
        lblSchemaMode.Anchor = AnchorStyles.Left
        lblSchemaMode.AutoSize = True
        lblSchemaMode.Location = New Point(176, 5)
        lblSchemaMode.Name = "lblSchemaMode"
        lblSchemaMode.Size = New Size(46, 25)
        lblSchemaMode.TabIndex = 1
        lblSchemaMode.Text = "Mod:"
        ' 
        ' cmbSchemaMode
        ' 
        cmbSchemaMode.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        cmbSchemaMode.DropDownStyle = ComboBoxStyle.DropDownList
        cmbSchemaMode.Items.AddRange(New Object() {"SAFE", "FORCE"})
        cmbSchemaMode.Location = New Point(228, 2)
        cmbSchemaMode.Name = "cmbSchemaMode"
        cmbSchemaMode.Size = New Size(104, 33)
        cmbSchemaMode.TabIndex = 2
        ' 
        ' lblRemotePython
        ' 
        lblRemotePython.Anchor = AnchorStyles.Left
        lblRemotePython.AutoSize = True
        lblRemotePython.Location = New Point(338, 5)
        lblRemotePython.Name = "lblRemotePython"
        lblRemotePython.Size = New Size(74, 25)
        lblRemotePython.TabIndex = 3
        lblRemotePython.Text = "Python:"
        ' 
        ' txtRemotePython
        ' 
        txtRemotePython.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtRemotePython.Location = New Point(418, 2)
        txtRemotePython.Name = "txtRemotePython"
        txtRemotePython.Size = New Size(128, 31)
        txtRemotePython.TabIndex = 4
        ' 
        ' btnSchemaView
        ' 
        btnSchemaView.Dock = DockStyle.Fill
        btnSchemaView.Location = New Point(552, 0)
        btnSchemaView.Margin = New Padding(0)
        btnSchemaView.Name = "btnSchemaView"
        btnSchemaView.Size = New Size(180, 35)
        btnSchemaView.TabIndex = 5
        btnSchemaView.Text = "Vezi (nu execută)"
        ' 
        ' btnSchemaRun
        ' 
        btnSchemaRun.Dock = DockStyle.Fill
        btnSchemaRun.Location = New Point(732, 0)
        btnSchemaRun.Margin = New Padding(0)
        btnSchemaRun.Name = "btnSchemaRun"
        btnSchemaRun.Size = New Size(130, 35)
        btnSchemaRun.TabIndex = 6
        btnSchemaRun.Text = "Execută"
        ' 
        ' clbTargets
        ' 
        clbTargets.CheckOnClick = True
        clbTargets.Dock = DockStyle.Fill
        clbTargets.Font = New Font("Consolas", 9F)
        clbTargets.FormattingEnabled = True
        clbTargets.IntegralHeight = False
        clbTargets.Location = New Point(3, 38)
        clbTargets.Name = "clbTargets"
        clbTargets.Size = New Size(856, 140)
        clbTargets.TabIndex = 1
        ' 
        ' lblSchemaHint
        ' 
        lblSchemaHint.AutoSize = True
        lblSchemaHint.Dock = DockStyle.Fill
        lblSchemaHint.Location = New Point(3, 181)
        lblSchemaHint.Name = "lblSchemaHint"
        lblSchemaHint.Size = New Size(856, 25)
        lblSchemaHint.TabIndex = 2
        lblSchemaHint.Text = "Lista vine de pe server. «lipsă din CAI» = baza există, dar registrul nu o listează; «nu există pe server» = numai în CAI, deci nu poate fi bifată."
        '
        ' tabOnce
        '
        tabOnce.Controls.Add(tlpOnce)
        tabOnce.Location = New Point(4, 34)
        tabOnce.Name = "tabOnce"
        tabOnce.Padding = New Padding(6)
        tabOnce.Size = New Size(874, 223)
        tabOnce.TabIndex = 3
        tabOnce.Text = "Interogări unice"
        tabOnce.UseVisualStyleBackColor = True
        '
        ' tlpOnce
        '
        tlpOnce.ColumnCount = 1
        tlpOnce.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpOnce.Controls.Add(tlpOnceActions, 0, 0)
        tlpOnce.Controls.Add(clbOnceTargets, 0, 1)
        tlpOnce.Controls.Add(txtOnceSql, 0, 2)
        tlpOnce.Controls.Add(lblOnceHint, 0, 3)
        tlpOnce.Dock = DockStyle.Fill
        tlpOnce.Location = New Point(6, 6)
        tlpOnce.Margin = New Padding(0)
        tlpOnce.Name = "tlpOnce"
        tlpOnce.RowCount = 4
        tlpOnce.RowStyles.Add(New RowStyle())
        tlpOnce.RowStyles.Add(New RowStyle(SizeType.Percent, 40F))
        tlpOnce.RowStyles.Add(New RowStyle(SizeType.Percent, 60F))
        tlpOnce.RowStyles.Add(New RowStyle())
        tlpOnce.Size = New Size(862, 211)
        tlpOnce.TabIndex = 0
        '
        ' tlpOnceActions
        '
        tlpOnceActions.AutoSize = True
        tlpOnceActions.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpOnceActions.ColumnCount = 7
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle())
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110F))
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlpOnceActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlpOnceActions.Controls.Add(btnOnceTargets, 0, 0)
        tlpOnceActions.Controls.Add(lblOnceName, 1, 0)
        tlpOnceActions.Controls.Add(txtOnceName, 2, 0)
        tlpOnceActions.Controls.Add(btnOnceLoad, 3, 0)
        tlpOnceActions.Controls.Add(btnOnceStatus, 4, 0)
        tlpOnceActions.Controls.Add(btnOnceView, 5, 0)
        tlpOnceActions.Controls.Add(btnOnceRun, 6, 0)
        tlpOnceActions.Dock = DockStyle.Fill
        tlpOnceActions.Location = New Point(0, 0)
        tlpOnceActions.Margin = New Padding(0)
        tlpOnceActions.Name = "tlpOnceActions"
        tlpOnceActions.RowCount = 1
        tlpOnceActions.RowStyles.Add(New RowStyle())
        tlpOnceActions.Size = New Size(862, 35)
        tlpOnceActions.TabIndex = 0
        '
        ' btnOnceTargets
        '
        btnOnceTargets.Dock = DockStyle.Fill
        btnOnceTargets.Location = New Point(0, 0)
        btnOnceTargets.Margin = New Padding(0)
        btnOnceTargets.Name = "btnOnceTargets"
        btnOnceTargets.Size = New Size(130, 35)
        btnOnceTargets.TabIndex = 0
        btnOnceTargets.Text = "Citește bazele"
        '
        ' clbOnceTargets
        '
        clbOnceTargets.CheckOnClick = True
        clbOnceTargets.Dock = DockStyle.Fill
        clbOnceTargets.Font = New Font("Consolas", 9F)
        clbOnceTargets.FormattingEnabled = True
        clbOnceTargets.IntegralHeight = False
        clbOnceTargets.Location = New Point(3, 38)
        clbOnceTargets.Name = "clbOnceTargets"
        clbOnceTargets.Size = New Size(856, 60)
        clbOnceTargets.TabIndex = 1
        '
        ' lblOnceName
        '
        lblOnceName.Anchor = AnchorStyles.Left
        lblOnceName.AutoSize = True
        lblOnceName.Location = New Point(3, 5)
        lblOnceName.Name = "lblOnceName"
        lblOnceName.Size = New Size(62, 25)
        lblOnceName.TabIndex = 1
        lblOnceName.Text = "Nume:"
        '
        ' txtOnceName
        '
        txtOnceName.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtOnceName.Location = New Point(71, 2)
        txtOnceName.MaxLength = 100
        txtOnceName.Name = "txtOnceName"
        txtOnceName.Size = New Size(236, 31)
        txtOnceName.TabIndex = 2
        '
        ' btnOnceLoad
        '
        btnOnceLoad.Dock = DockStyle.Fill
        btnOnceLoad.Location = New Point(310, 0)
        btnOnceLoad.Margin = New Padding(0)
        btnOnceLoad.Name = "btnOnceLoad"
        btnOnceLoad.Size = New Size(150, 35)
        btnOnceLoad.TabIndex = 3
        btnOnceLoad.Text = "Din fișier…"
        '
        ' btnOnceStatus
        '
        btnOnceStatus.Dock = DockStyle.Fill
        btnOnceStatus.Location = New Point(460, 0)
        btnOnceStatus.Margin = New Padding(0)
        btnOnceStatus.Name = "btnOnceStatus"
        btnOnceStatus.Size = New Size(150, 35)
        btnOnceStatus.TabIndex = 4
        btnOnceStatus.Text = "Ce s-a rulat"
        '
        ' btnOnceView
        '
        btnOnceView.Dock = DockStyle.Fill
        btnOnceView.Location = New Point(610, 0)
        btnOnceView.Margin = New Padding(0)
        btnOnceView.Name = "btnOnceView"
        btnOnceView.Size = New Size(180, 35)
        btnOnceView.TabIndex = 5
        btnOnceView.Text = "Vezi (nu execută)"
        '
        ' btnOnceRun
        '
        btnOnceRun.Dock = DockStyle.Fill
        btnOnceRun.Location = New Point(790, 0)
        btnOnceRun.Margin = New Padding(0)
        btnOnceRun.Name = "btnOnceRun"
        btnOnceRun.Size = New Size(72, 35)
        btnOnceRun.TabIndex = 6
        btnOnceRun.Text = "Execută"
        '
        ' txtOnceSql
        '
        txtOnceSql.AcceptsReturn = True
        txtOnceSql.AcceptsTab = True
        txtOnceSql.Dock = DockStyle.Fill
        txtOnceSql.Font = New Font("Consolas", 9F)
        txtOnceSql.Location = New Point(3, 38)
        txtOnceSql.Multiline = True
        txtOnceSql.Name = "txtOnceSql"
        txtOnceSql.ScrollBars = ScrollBars.Both
        txtOnceSql.Size = New Size(856, 140)
        txtOnceSql.TabIndex = 2
        txtOnceSql.WordWrap = False
        '
        ' lblOnceHint
        '
        lblOnceHint.AutoSize = True
        lblOnceHint.Dock = DockStyle.Fill
        lblOnceHint.Location = New Point(3, 181)
        lblOnceHint.Name = "lblOnceHint"
        lblOnceHint.Size = New Size(856, 25)
        lblOnceHint.TabIndex = 3
        lblOnceHint.Text = "Citiți bazele (se bifează toate cele care există) și rulați: AVACONT_SURSA se rulează mereu, prima, apoi bazele bifate, fiecare în baza ei (nume de tabele fără prefix). «Vezi» nu scrie nimic; o bază care a rulat deja textul e sărită."
        '
        ' dlgOnceFile
        '
        dlgOnceFile.Filter = "Interogări SQL (*.sql)|*.sql|Toate fișierele (*.*)|*.*"
        dlgOnceFile.Title = "Alegeți fișierul cu interogarea"
        '
        ' tabUsers
        '
        tabUsers.Controls.Add(tlpUsers)
        tabUsers.Location = New Point(4, 34)
        tabUsers.Name = "tabUsers"
        tabUsers.Padding = New Padding(6)
        tabUsers.Size = New Size(874, 223)
        tabUsers.TabIndex = 2
        tabUsers.Text = "Utilizatori"
        tabUsers.UseVisualStyleBackColor = True
        '
        ' tlpUsers
        '
        tlpUsers.ColumnCount = 1
        tlpUsers.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpUsers.Controls.Add(tlpUsersActions, 0, 0)
        tlpUsers.Controls.Add(dgvUsers, 0, 1)
        tlpUsers.Dock = DockStyle.Fill
        tlpUsers.Location = New Point(6, 6)
        tlpUsers.Margin = New Padding(0)
        tlpUsers.Name = "tlpUsers"
        tlpUsers.RowCount = 2
        tlpUsers.RowStyles.Add(New RowStyle())
        tlpUsers.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpUsers.Size = New Size(862, 211)
        tlpUsers.TabIndex = 0
        '
        ' tlpUsersActions
        '
        tlpUsersActions.AutoSize = True
        tlpUsersActions.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpUsersActions.ColumnCount = 6
        tlpUsersActions.ColumnStyles.Add(New ColumnStyle())
        tlpUsersActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 60F))
        tlpUsersActions.ColumnStyles.Add(New ColumnStyle())
        tlpUsersActions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 40F))
        tlpUsersActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 190F))
        tlpUsersActions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 280F))
        tlpUsersActions.Controls.Add(lblApiUrl, 0, 0)
        tlpUsersActions.Controls.Add(txtApiUrl, 1, 0)
        tlpUsersActions.Controls.Add(lblApiKey, 2, 0)
        tlpUsersActions.Controls.Add(txtApiKey, 3, 0)
        tlpUsersActions.Controls.Add(btnUsersLoad, 4, 0)
        tlpUsersActions.Controls.Add(btnLoginReset, 5, 0)
        tlpUsersActions.Dock = DockStyle.Fill
        tlpUsersActions.Location = New Point(0, 0)
        tlpUsersActions.Margin = New Padding(0)
        tlpUsersActions.Name = "tlpUsersActions"
        tlpUsersActions.RowCount = 1
        tlpUsersActions.RowStyles.Add(New RowStyle())
        tlpUsersActions.Size = New Size(862, 37)
        tlpUsersActions.TabIndex = 0
        '
        ' lblApiUrl
        '
        lblApiUrl.Anchor = AnchorStyles.Left
        lblApiUrl.AutoSize = True
        lblApiUrl.Location = New Point(3, 6)
        lblApiUrl.Name = "lblApiUrl"
        lblApiUrl.Size = New Size(44, 25)
        lblApiUrl.TabIndex = 0
        lblApiUrl.Text = "API:"
        '
        ' txtApiUrl
        '
        txtApiUrl.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtApiUrl.Location = New Point(53, 3)
        txtApiUrl.Name = "txtApiUrl"
        txtApiUrl.Size = New Size(160, 31)
        txtApiUrl.TabIndex = 1
        '
        ' lblApiKey
        '
        lblApiKey.Anchor = AnchorStyles.Left
        lblApiKey.AutoSize = True
        lblApiKey.Location = New Point(219, 6)
        lblApiKey.Name = "lblApiKey"
        lblApiKey.Size = New Size(58, 25)
        lblApiKey.TabIndex = 2
        lblApiKey.Text = "Cheie:"
        '
        ' txtApiKey
        '
        txtApiKey.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtApiKey.Location = New Point(283, 3)
        txtApiKey.Name = "txtApiKey"
        txtApiKey.Size = New Size(106, 31)
        txtApiKey.TabIndex = 3
        txtApiKey.UseSystemPasswordChar = True
        '
        ' btnUsersLoad
        '
        btnUsersLoad.Dock = DockStyle.Fill
        btnUsersLoad.Location = New Point(392, 0)
        btnUsersLoad.Margin = New Padding(0)
        btnUsersLoad.Name = "btnUsersLoad"
        btnUsersLoad.Size = New Size(190, 37)
        btnUsersLoad.TabIndex = 4
        btnUsersLoad.Text = "Citește utilizatorii"
        '
        ' btnLoginReset
        '
        btnLoginReset.Dock = DockStyle.Fill
        btnLoginReset.Location = New Point(582, 0)
        btnLoginReset.Margin = New Padding(0)
        btnLoginReset.Name = "btnLoginReset"
        btnLoginReset.Size = New Size(280, 37)
        btnLoginReset.TabIndex = 5
        btnLoginReset.Text = "Resetează încercările de login"
        '
        ' dgvUsers
        '
        dgvUsers.AllowUserToAddRows = False
        dgvUsers.AllowUserToDeleteRows = False
        dgvUsers.AllowUserToResizeRows = False
        dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvUsers.Columns.AddRange(New DataGridViewColumn() {colUn, colDc, colUnitate, colRol, colLastSs, colFails, colBlocked})
        dgvUsers.Dock = DockStyle.Fill
        dgvUsers.Location = New Point(0, 43)
        dgvUsers.Margin = New Padding(0, 6, 0, 0)
        dgvUsers.MultiSelect = True
        dgvUsers.Name = "dgvUsers"
        dgvUsers.ReadOnly = True
        dgvUsers.RowHeadersVisible = False
        dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvUsers.Size = New Size(862, 168)
        dgvUsers.TabIndex = 1
        ' 
        ' colUn
        ' 
        colUn.FillWeight = 28F
        colUn.HeaderText = "Utilizator"
        colUn.Name = "colUn"
        colUn.ReadOnly = True
        ' 
        ' colDc
        ' 
        colDc.FillWeight = 11F
        colDc.HeaderText = "DC"
        colDc.Name = "colDc"
        colDc.ReadOnly = True
        ' 
        ' colUnitate
        ' 
        colUnitate.FillWeight = 33F
        colUnitate.HeaderText = "Unitate"
        colUnitate.Name = "colUnitate"
        colUnitate.ReadOnly = True
        ' 
        ' colRol
        ' 
        colRol.FillWeight = 10F
        colRol.HeaderText = "Rol"
        colRol.Name = "colRol"
        colRol.ReadOnly = True
        ' 
        ' colLastSs
        ' 
        colLastSs.FillWeight = 6F
        colLastSs.HeaderText = "SS"
        colLastSs.Name = "colLastSs"
        colLastSs.ReadOnly = True
        ' 
        ' colFails
        ' 
        colFails.FillWeight = 6F
        colFails.HeaderText = "Eșecuri"
        colFails.Name = "colFails"
        colFails.ReadOnly = True
        ' 
        ' colBlocked
        ' 
        colBlocked.FillWeight = 10F
        colBlocked.HeaderText = "Blocat"
        colBlocked.Name = "colBlocked"
        colBlocked.ReadOnly = True
        '
        ' tvFiles
        ' 
        tvFiles.CheckBoxes = True
        tvFiles.Dock = DockStyle.Fill
        tvFiles.Font = New Font("Segoe UI", 9F)
        tvFiles.HideSelection = False
        tvFiles.Location = New Point(0, 0)
        tvFiles.Name = "tvFiles"
        tvFiles.Margin = New Padding(0, 6, 0, 0)
        tvFiles.ShowNodeToolTips = True
        tvFiles.Size = New Size(862, 170)
        tvFiles.TabIndex = 1
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
        tlpFiles.ResumeLayout(False)
        tlpFiles.PerformLayout()
        tabFiles.ResumeLayout(False)
        tlpSchemaActions.ResumeLayout(False)
        tlpSchemaActions.PerformLayout()
        tlpSchema.ResumeLayout(False)
        tlpSchema.PerformLayout()
        tabSchema.ResumeLayout(False)
        tlpOnceActions.ResumeLayout(False)
        tlpOnceActions.PerformLayout()
        tlpOnce.ResumeLayout(False)
        tlpOnce.PerformLayout()
        tabOnce.ResumeLayout(False)
        tlpUsersActions.ResumeLayout(False)
        tlpUsersActions.PerformLayout()
        CType(dgvUsers, System.ComponentModel.ISupportInitialize).EndInit()
        tlpUsers.ResumeLayout(False)
        tlpUsers.PerformLayout()
        tabUsers.ResumeLayout(False)
        tabsMain.ResumeLayout(False)
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        tlpStatus.ResumeLayout(False)
        tlpStatus.PerformLayout()
        ResumeLayout(False)
    End Sub

End Class
