<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdobeReaderHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' Adobe embed test bench. Layout (slice 0023, config+layout pass): a vertical SplitContainer
    ' the operator can drag at runtime — Panel1 holds the options as one GroupBox + TableLayoutPanel
    ' per section stacked in a scrolling TableLayoutPanel, Panel2 holds the Adobe host (100%) over
    ' the status line (AutoSize). Everything docks, so controls follow the form and the Adobe area
    ' can be traded against the options area. House rule: ALL WinForms controls are declared here,
    ' in .Designer.vb — including the SplitContainer and every TableLayoutPanel, so the layout stays
    ' editable in the designer.

    Private components As System.ComponentModel.IContainer

    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents tlpOptions As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents tlpRight As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents pnlHost As System.Windows.Forms.Panel
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    ' Debounce for resize/splitter storms — Adobe repaints late and badly during them.
    Friend WithEvents tmrLayout As System.Windows.Forms.Timer

    ' Lansare
    Friend WithEvents grpLaunch As System.Windows.Forms.GroupBox
    Friend WithEvents tlpLaunch As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents chkNewInstance As System.Windows.Forms.CheckBox
    Friend WithEvents chkNoSplash As System.Windows.Forms.CheckBox

    ' Chrome ascuns (parametri /A)
    Friend WithEvents grpChrome As System.Windows.Forms.GroupBox
    Friend WithEvents tlpChrome As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents chkToolbar As System.Windows.Forms.CheckBox
    Friend WithEvents chkNavpanes As System.Windows.Forms.CheckBox
    Friend WithEvents chkStatusbar As System.Windows.Forms.CheckBox
    Friend WithEvents chkMessages As System.Windows.Forms.CheckBox
    Friend WithEvents chkScrollbar As System.Windows.Forms.CheckBox
    Friend WithEvents chkPagemodeNone As System.Windows.Forms.CheckBox

    ' Document
    Friend WithEvents grpFile As System.Windows.Forms.GroupBox
    Friend WithEvents tlpFile As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents lblFile As System.Windows.Forms.Label
    Friend WithEvents btnRelaunch As System.Windows.Forms.Button

    ' Diagnostic — the child window probe.
    Friend WithEvents grpProbe As System.Windows.Forms.GroupBox
    Friend WithEvents tlpProbe As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnProbe As System.Windows.Forms.Button

    ' Scenariu — load / run / save a scenario file (JSON, AppDir\Config).
    Friend WithEvents grpScenario As System.Windows.Forms.GroupBox
    Friend WithEvents tlpScenario As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblScenario As System.Windows.Forms.Label
    Friend WithEvents btnLoadScenario As System.Windows.Forms.Button
    Friend WithEvents btnRunScenario As System.Windows.Forms.Button
    Friend WithEvents btnSaveScenario As System.Windows.Forms.Button
    Friend WithEvents chkApplyOnLoad As System.Windows.Forms.CheckBox

    ' Decupare — geometry clipping (live, no relaunch).
    Friend WithEvents grpClip As System.Windows.Forms.GroupBox
    Friend WithEvents tlpClip As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents chkClip As System.Windows.Forms.CheckBox
    Friend WithEvents lblClipRight As System.Windows.Forms.Label
    Friend WithEvents numClipRight As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblClipTop As System.Windows.Forms.Label
    Friend WithEvents numClipTop As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnClipAuto As System.Windows.Forms.Button

    ' Ferestre copil — hide a child window directly.
    Friend WithEvents grpChildren As System.Windows.Forms.GroupBox
    Friend WithEvents tlpChildren As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lstChildren As System.Windows.Forms.ListBox
    Friend WithEvents btnHideChild As System.Windows.Forms.Button
    Friend WithEvents btnShowChild As System.Windows.Forms.Button
    Friend WithEvents btnShowAllChildren As System.Windows.Forms.Button

    ' Scurtături — keyboard toggles (experimental).
    Friend WithEvents grpKeys As System.Windows.Forms.GroupBox
    Friend WithEvents tlpKeys As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnSendShiftF4 As System.Windows.Forms.Button
    Friend WithEvents btnSendF4 As System.Windows.Forms.Button

    ' Preferințe Adobe (utilizator) — HKCU, no elevation.
    Friend WithEvents grpUser As System.Windows.Forms.GroupBox
    Friend WithEvents tlpUser As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblHive As System.Windows.Forms.Label
    Friend WithEvents cboHive As System.Windows.Forms.ComboBox
    Friend WithEvents chkExpandRhp As System.Windows.Forms.CheckBox
    Friend WithEvents chkRhpSticky As System.Windows.Forms.CheckBox
    Friend WithEvents chkRhpCollapsed As System.Windows.Forms.CheckBox
    Friend WithEvents chkClassicViewer As System.Windows.Forms.CheckBox
    Friend WithEvents btnApplyUser As System.Windows.Forms.Button
    Friend WithEvents btnRestoreUser As System.Windows.Forms.Button
    Friend WithEvents chkRestoreOnClose As System.Windows.Forms.CheckBox

    ' Politici Adobe (mașină) — HKLM via elevated reg.exe import.
    Friend WithEvents grpMachine As System.Windows.Forms.GroupBox
    Friend WithEvents tlpMachine As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents cboProduct As System.Windows.Forms.ComboBox
    Friend WithEvents chkSuppressUpsell As System.Windows.Forms.CheckBox
    Friend WithEvents chkDisableServices As System.Windows.Forms.CheckBox
    Friend WithEvents btnApplyMachine As System.Windows.Forms.Button
    Friend WithEvents btnRevertMachine As System.Windows.Forms.Button

    ' Linie de comandă — the one control that must not collapse when the splitter goes narrow.
    Friend WithEvents grpCmd As System.Windows.Forms.GroupBox
    Friend WithEvents tlpCmd As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents txtCmd As System.Windows.Forms.TextBox

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        splitMain = New System.Windows.Forms.SplitContainer()
        tlpOptions = New System.Windows.Forms.TableLayoutPanel()
        tlpRight = New System.Windows.Forms.TableLayoutPanel()
        pnlHost = New System.Windows.Forms.Panel()
        lblStatus = New System.Windows.Forms.Label()
        tmrLayout = New System.Windows.Forms.Timer(components)
        grpLaunch = New System.Windows.Forms.GroupBox()
        tlpLaunch = New System.Windows.Forms.TableLayoutPanel()
        chkNewInstance = New System.Windows.Forms.CheckBox()
        chkNoSplash = New System.Windows.Forms.CheckBox()
        grpChrome = New System.Windows.Forms.GroupBox()
        tlpChrome = New System.Windows.Forms.TableLayoutPanel()
        chkToolbar = New System.Windows.Forms.CheckBox()
        chkNavpanes = New System.Windows.Forms.CheckBox()
        chkStatusbar = New System.Windows.Forms.CheckBox()
        chkMessages = New System.Windows.Forms.CheckBox()
        chkScrollbar = New System.Windows.Forms.CheckBox()
        chkPagemodeNone = New System.Windows.Forms.CheckBox()
        grpFile = New System.Windows.Forms.GroupBox()
        tlpFile = New System.Windows.Forms.TableLayoutPanel()
        btnBrowse = New System.Windows.Forms.Button()
        lblFile = New System.Windows.Forms.Label()
        btnRelaunch = New System.Windows.Forms.Button()
        grpProbe = New System.Windows.Forms.GroupBox()
        tlpProbe = New System.Windows.Forms.TableLayoutPanel()
        btnProbe = New System.Windows.Forms.Button()
        grpScenario = New System.Windows.Forms.GroupBox()
        tlpScenario = New System.Windows.Forms.TableLayoutPanel()
        lblScenario = New System.Windows.Forms.Label()
        btnLoadScenario = New System.Windows.Forms.Button()
        btnRunScenario = New System.Windows.Forms.Button()
        btnSaveScenario = New System.Windows.Forms.Button()
        chkApplyOnLoad = New System.Windows.Forms.CheckBox()
        grpClip = New System.Windows.Forms.GroupBox()
        tlpClip = New System.Windows.Forms.TableLayoutPanel()
        chkClip = New System.Windows.Forms.CheckBox()
        lblClipRight = New System.Windows.Forms.Label()
        numClipRight = New System.Windows.Forms.NumericUpDown()
        lblClipTop = New System.Windows.Forms.Label()
        numClipTop = New System.Windows.Forms.NumericUpDown()
        btnClipAuto = New System.Windows.Forms.Button()
        grpChildren = New System.Windows.Forms.GroupBox()
        tlpChildren = New System.Windows.Forms.TableLayoutPanel()
        lstChildren = New System.Windows.Forms.ListBox()
        btnHideChild = New System.Windows.Forms.Button()
        btnShowChild = New System.Windows.Forms.Button()
        btnShowAllChildren = New System.Windows.Forms.Button()
        grpKeys = New System.Windows.Forms.GroupBox()
        tlpKeys = New System.Windows.Forms.TableLayoutPanel()
        btnSendShiftF4 = New System.Windows.Forms.Button()
        btnSendF4 = New System.Windows.Forms.Button()
        grpUser = New System.Windows.Forms.GroupBox()
        tlpUser = New System.Windows.Forms.TableLayoutPanel()
        lblHive = New System.Windows.Forms.Label()
        cboHive = New System.Windows.Forms.ComboBox()
        chkExpandRhp = New System.Windows.Forms.CheckBox()
        chkRhpSticky = New System.Windows.Forms.CheckBox()
        chkRhpCollapsed = New System.Windows.Forms.CheckBox()
        chkClassicViewer = New System.Windows.Forms.CheckBox()
        btnApplyUser = New System.Windows.Forms.Button()
        btnRestoreUser = New System.Windows.Forms.Button()
        chkRestoreOnClose = New System.Windows.Forms.CheckBox()
        grpMachine = New System.Windows.Forms.GroupBox()
        tlpMachine = New System.Windows.Forms.TableLayoutPanel()
        cboProduct = New System.Windows.Forms.ComboBox()
        chkSuppressUpsell = New System.Windows.Forms.CheckBox()
        chkDisableServices = New System.Windows.Forms.CheckBox()
        btnApplyMachine = New System.Windows.Forms.Button()
        btnRevertMachine = New System.Windows.Forms.Button()
        grpCmd = New System.Windows.Forms.GroupBox()
        tlpCmd = New System.Windows.Forms.TableLayoutPanel()
        txtCmd = New System.Windows.Forms.TextBox()
        pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        btnPass = New System.Windows.Forms.Button()
        btnFail = New System.Windows.Forms.Button()
        CType(splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        tlpOptions.SuspendLayout()
        tlpRight.SuspendLayout()
        grpLaunch.SuspendLayout()
        tlpLaunch.SuspendLayout()
        grpChrome.SuspendLayout()
        tlpChrome.SuspendLayout()
        grpFile.SuspendLayout()
        tlpFile.SuspendLayout()
        grpProbe.SuspendLayout()
        tlpProbe.SuspendLayout()
        grpScenario.SuspendLayout()
        tlpScenario.SuspendLayout()
        grpClip.SuspendLayout()
        tlpClip.SuspendLayout()
        CType(numClipRight, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(numClipTop, System.ComponentModel.ISupportInitialize).BeginInit()
        grpChildren.SuspendLayout()
        tlpChildren.SuspendLayout()
        grpKeys.SuspendLayout()
        tlpKeys.SuspendLayout()
        grpUser.SuspendLayout()
        tlpUser.SuspendLayout()
        grpMachine.SuspendLayout()
        tlpMachine.SuspendLayout()
        grpCmd.SuspendLayout()
        tlpCmd.SuspendLayout()
        pnlButtons.SuspendLayout()
        SuspendLayout()
        '
        ' splitMain — dragged at runtime; both sides share the growth (FixedPanel = None)
        '
        splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        splitMain.FixedPanel = System.Windows.Forms.FixedPanel.None
        splitMain.IsSplitterFixed = False
        splitMain.Location = New System.Drawing.Point(0, 0)
        splitMain.Name = "splitMain"
        splitMain.Orientation = System.Windows.Forms.Orientation.Vertical
        splitMain.Panel1.Controls.Add(tlpOptions)
        splitMain.Panel1MinSize = 260
        splitMain.Panel2.Controls.Add(tlpRight)
        splitMain.Panel2MinSize = 200
        splitMain.Size = New System.Drawing.Size(1240, 727)
        splitMain.SplitterDistance = 320
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 0
        '
        ' tlpOptions — one AutoSize row per section, plus a filler row so sections stay top-aligned
        '
        tlpOptions.AutoScroll = True
        tlpOptions.ColumnCount = 1
        tlpOptions.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpOptions.Controls.Add(grpLaunch, 0, 0)
        tlpOptions.Controls.Add(grpChrome, 0, 1)
        tlpOptions.Controls.Add(grpFile, 0, 2)
        tlpOptions.Controls.Add(grpProbe, 0, 3)
        tlpOptions.Controls.Add(grpScenario, 0, 4)
        tlpOptions.Controls.Add(grpClip, 0, 5)
        tlpOptions.Controls.Add(grpChildren, 0, 6)
        tlpOptions.Controls.Add(grpKeys, 0, 7)
        tlpOptions.Controls.Add(grpUser, 0, 8)
        tlpOptions.Controls.Add(grpMachine, 0, 9)
        tlpOptions.Controls.Add(grpCmd, 0, 10)
        tlpOptions.Dock = System.Windows.Forms.DockStyle.Fill
        tlpOptions.Location = New System.Drawing.Point(0, 0)
        tlpOptions.Name = "tlpOptions"
        tlpOptions.Padding = New System.Windows.Forms.Padding(6)
        tlpOptions.RowCount = 12
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpOptions.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpOptions.Size = New System.Drawing.Size(320, 727)
        tlpOptions.TabIndex = 0
        '
        ' tlpRight — Adobe host (100%) over the status line (AutoSize)
        '
        tlpRight.ColumnCount = 1
        tlpRight.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpRight.Controls.Add(pnlHost, 0, 0)
        tlpRight.Controls.Add(lblStatus, 0, 1)
        tlpRight.Dock = System.Windows.Forms.DockStyle.Fill
        tlpRight.Location = New System.Drawing.Point(0, 0)
        tlpRight.Name = "tlpRight"
        tlpRight.RowCount = 2
        tlpRight.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpRight.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpRight.Size = New System.Drawing.Size(914, 727)
        tlpRight.TabIndex = 0
        '
        ' pnlHost — gazda ferestrei Adobe reparentate
        '
        pnlHost.Dock = System.Windows.Forms.DockStyle.Fill
        pnlHost.Location = New System.Drawing.Point(3, 3)
        pnlHost.Name = "pnlHost"
        pnlHost.Size = New System.Drawing.Size(908, 640)
        pnlHost.TabIndex = 0
        '
        ' lblStatus
        '
        lblStatus.AutoSize = True
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill
        lblStatus.Location = New System.Drawing.Point(3, 646)
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New System.Windows.Forms.Padding(4)
        lblStatus.Size = New System.Drawing.Size(908, 33)
        lblStatus.TabIndex = 1
        '
        ' tmrLayout — 150 ms debounce, restarted on every resize/splitter move
        '
        tmrLayout.Interval = 150
        '
        ' grpLaunch
        '
        grpLaunch.AutoSize = True
        grpLaunch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpLaunch.Controls.Add(tlpLaunch)
        grpLaunch.Dock = System.Windows.Forms.DockStyle.Fill
        grpLaunch.Location = New System.Drawing.Point(9, 9)
        grpLaunch.Name = "grpLaunch"
        grpLaunch.Size = New System.Drawing.Size(296, 90)
        grpLaunch.TabIndex = 0
        grpLaunch.TabStop = False
        grpLaunch.Text = "Lansare"
        '
        tlpLaunch.AutoSize = True
        tlpLaunch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpLaunch.ColumnCount = 1
        tlpLaunch.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpLaunch.Controls.Add(chkNewInstance, 0, 0)
        tlpLaunch.Controls.Add(chkNoSplash, 0, 1)
        tlpLaunch.Dock = System.Windows.Forms.DockStyle.Fill
        tlpLaunch.Name = "tlpLaunch"
        tlpLaunch.RowCount = 2
        tlpLaunch.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpLaunch.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpLaunch.TabIndex = 0
        '
        chkNewInstance.AutoSize = True
        chkNewInstance.Checked = True
        chkNewInstance.CheckState = System.Windows.Forms.CheckState.Checked
        chkNewInstance.Dock = System.Windows.Forms.DockStyle.Fill
        chkNewInstance.Name = "chkNewInstance"
        chkNewInstance.TabIndex = 0
        chkNewInstance.Text = "/n  — instanță nouă (recomandat pt. încorporare)"
        '
        chkNoSplash.AutoSize = True
        chkNoSplash.Checked = True
        chkNoSplash.CheckState = System.Windows.Forms.CheckState.Checked
        chkNoSplash.Dock = System.Windows.Forms.DockStyle.Fill
        chkNoSplash.Name = "chkNoSplash"
        chkNoSplash.TabIndex = 1
        chkNoSplash.Text = "/s  — fără ecran de întâmpinare"
        '
        ' grpChrome
        '
        grpChrome.AutoSize = True
        grpChrome.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpChrome.Controls.Add(tlpChrome)
        grpChrome.Dock = System.Windows.Forms.DockStyle.Fill
        grpChrome.Name = "grpChrome"
        grpChrome.TabIndex = 1
        grpChrome.TabStop = False
        grpChrome.Text = "Chrome ascuns (parametri /A)"
        '
        tlpChrome.AutoSize = True
        tlpChrome.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpChrome.ColumnCount = 1
        tlpChrome.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpChrome.Controls.Add(chkToolbar, 0, 0)
        tlpChrome.Controls.Add(chkNavpanes, 0, 1)
        tlpChrome.Controls.Add(chkStatusbar, 0, 2)
        tlpChrome.Controls.Add(chkMessages, 0, 3)
        tlpChrome.Controls.Add(chkScrollbar, 0, 4)
        tlpChrome.Controls.Add(chkPagemodeNone, 0, 5)
        tlpChrome.Dock = System.Windows.Forms.DockStyle.Fill
        tlpChrome.Name = "tlpChrome"
        tlpChrome.RowCount = 6
        tlpChrome.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChrome.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChrome.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChrome.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChrome.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChrome.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChrome.TabIndex = 0
        '
        chkToolbar.AutoSize = True
        chkToolbar.Checked = True
        chkToolbar.CheckState = System.Windows.Forms.CheckState.Checked
        chkToolbar.Dock = System.Windows.Forms.DockStyle.Fill
        chkToolbar.Name = "chkToolbar"
        chkToolbar.TabIndex = 0
        chkToolbar.Text = "toolbar=0  — ascunde bara de instrumente"
        '
        chkNavpanes.AutoSize = True
        chkNavpanes.Checked = True
        chkNavpanes.CheckState = System.Windows.Forms.CheckState.Checked
        chkNavpanes.Dock = System.Windows.Forms.DockStyle.Fill
        chkNavpanes.Name = "chkNavpanes"
        chkNavpanes.TabIndex = 1
        chkNavpanes.Text = "navpanes=0  — ascunde panourile de navigare"
        '
        chkStatusbar.AutoSize = True
        chkStatusbar.Checked = True
        chkStatusbar.CheckState = System.Windows.Forms.CheckState.Checked
        chkStatusbar.Dock = System.Windows.Forms.DockStyle.Fill
        chkStatusbar.Name = "chkStatusbar"
        chkStatusbar.TabIndex = 2
        chkStatusbar.Text = "statusbar=0  — ascunde bara de stare"
        '
        chkMessages.AutoSize = True
        chkMessages.Checked = True
        chkMessages.CheckState = System.Windows.Forms.CheckState.Checked
        chkMessages.Dock = System.Windows.Forms.DockStyle.Fill
        chkMessages.Name = "chkMessages"
        chkMessages.TabIndex = 3
        chkMessages.Text = "messages=0  — ascunde bara de mesaje"
        '
        chkScrollbar.AutoSize = True
        chkScrollbar.Checked = True
        chkScrollbar.CheckState = System.Windows.Forms.CheckState.Checked
        chkScrollbar.Dock = System.Windows.Forms.DockStyle.Fill
        chkScrollbar.Name = "chkScrollbar"
        chkScrollbar.TabIndex = 4
        chkScrollbar.Text = "scrollbar=0  — ascunde barele de derulare"
        '
        chkPagemodeNone.AutoSize = True
        chkPagemodeNone.Checked = True
        chkPagemodeNone.CheckState = System.Windows.Forms.CheckState.Checked
        chkPagemodeNone.Dock = System.Windows.Forms.DockStyle.Fill
        chkPagemodeNone.Name = "chkPagemodeNone"
        chkPagemodeNone.TabIndex = 5
        chkPagemodeNone.Text = "pagemode=none  — fără panou lateral deschis"
        '
        ' grpFile
        '
        grpFile.AutoSize = True
        grpFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpFile.Controls.Add(tlpFile)
        grpFile.Dock = System.Windows.Forms.DockStyle.Fill
        grpFile.Name = "grpFile"
        grpFile.TabIndex = 2
        grpFile.TabStop = False
        grpFile.Text = "Document"
        '
        tlpFile.AutoSize = True
        tlpFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpFile.ColumnCount = 1
        tlpFile.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpFile.Controls.Add(btnBrowse, 0, 0)
        tlpFile.Controls.Add(lblFile, 0, 1)
        tlpFile.Controls.Add(btnRelaunch, 0, 2)
        tlpFile.Dock = System.Windows.Forms.DockStyle.Fill
        tlpFile.Name = "tlpFile"
        tlpFile.RowCount = 3
        tlpFile.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpFile.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpFile.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpFile.TabIndex = 0
        '
        btnBrowse.AutoSize = True
        btnBrowse.Dock = System.Windows.Forms.DockStyle.Fill
        btnBrowse.Name = "btnBrowse"
        btnBrowse.TabIndex = 0
        btnBrowse.Text = "Deschide PDF…"
        btnBrowse.UseVisualStyleBackColor = True
        '
        lblFile.AutoSize = True
        lblFile.Dock = System.Windows.Forms.DockStyle.Fill
        lblFile.Name = "lblFile"
        lblFile.TabIndex = 1
        lblFile.Text = "<niciun PDF>"
        '
        btnRelaunch.AutoSize = True
        btnRelaunch.Dock = System.Windows.Forms.DockStyle.Fill
        btnRelaunch.Name = "btnRelaunch"
        btnRelaunch.TabIndex = 2
        btnRelaunch.Text = "Reîncorporează / redesenează"
        btnRelaunch.UseVisualStyleBackColor = True
        '
        ' grpProbe
        '
        grpProbe.AutoSize = True
        grpProbe.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpProbe.Controls.Add(tlpProbe)
        grpProbe.Dock = System.Windows.Forms.DockStyle.Fill
        grpProbe.Name = "grpProbe"
        grpProbe.TabIndex = 3
        grpProbe.TabStop = False
        grpProbe.Text = "Diagnostic"
        '
        tlpProbe.AutoSize = True
        tlpProbe.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpProbe.ColumnCount = 1
        tlpProbe.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpProbe.Controls.Add(btnProbe, 0, 0)
        tlpProbe.Dock = System.Windows.Forms.DockStyle.Fill
        tlpProbe.Name = "tlpProbe"
        tlpProbe.RowCount = 1
        tlpProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpProbe.TabIndex = 0
        '
        btnProbe.AutoSize = True
        btnProbe.Dock = System.Windows.Forms.DockStyle.Fill
        btnProbe.Name = "btnProbe"
        btnProbe.TabIndex = 0
        btnProbe.Text = "Arborele de ferestre copil"
        btnProbe.UseVisualStyleBackColor = True
        '
        ' grpScenario
        '
        grpScenario.AutoSize = True
        grpScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpScenario.Controls.Add(tlpScenario)
        grpScenario.Dock = System.Windows.Forms.DockStyle.Fill
        grpScenario.Name = "grpScenario"
        grpScenario.TabIndex = 4
        grpScenario.TabStop = False
        grpScenario.Text = "Scenariu"
        '
        tlpScenario.AutoSize = True
        tlpScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpScenario.ColumnCount = 1
        tlpScenario.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpScenario.Controls.Add(lblScenario, 0, 0)
        tlpScenario.Controls.Add(btnLoadScenario, 0, 1)
        tlpScenario.Controls.Add(btnRunScenario, 0, 2)
        tlpScenario.Controls.Add(btnSaveScenario, 0, 3)
        tlpScenario.Controls.Add(chkApplyOnLoad, 0, 4)
        tlpScenario.Dock = System.Windows.Forms.DockStyle.Fill
        tlpScenario.Name = "tlpScenario"
        tlpScenario.RowCount = 5
        tlpScenario.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpScenario.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpScenario.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpScenario.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpScenario.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpScenario.TabIndex = 0
        '
        lblScenario.AutoSize = True
        lblScenario.Dock = System.Windows.Forms.DockStyle.Fill
        lblScenario.Name = "lblScenario"
        lblScenario.TabIndex = 0
        lblScenario.Text = "(niciun scenariu)"
        '
        btnLoadScenario.AutoSize = True
        btnLoadScenario.Dock = System.Windows.Forms.DockStyle.Fill
        btnLoadScenario.Name = "btnLoadScenario"
        btnLoadScenario.TabIndex = 1
        btnLoadScenario.Text = "Încarcă scenariu…"
        btnLoadScenario.UseVisualStyleBackColor = True
        '
        btnRunScenario.AutoSize = True
        btnRunScenario.Dock = System.Windows.Forms.DockStyle.Fill
        btnRunScenario.Enabled = False
        btnRunScenario.Name = "btnRunScenario"
        btnRunScenario.TabIndex = 2
        btnRunScenario.Text = "Rulează scenariul"
        btnRunScenario.UseVisualStyleBackColor = True
        '
        btnSaveScenario.AutoSize = True
        btnSaveScenario.Dock = System.Windows.Forms.DockStyle.Fill
        btnSaveScenario.Name = "btnSaveScenario"
        btnSaveScenario.TabIndex = 3
        btnSaveScenario.Text = "Salvează starea curentă ca scenariu…"
        btnSaveScenario.UseVisualStyleBackColor = True
        '
        chkApplyOnLoad.AutoSize = True
        chkApplyOnLoad.Checked = True
        chkApplyOnLoad.CheckState = System.Windows.Forms.CheckState.Checked
        chkApplyOnLoad.Dock = System.Windows.Forms.DockStyle.Fill
        chkApplyOnLoad.Name = "chkApplyOnLoad"
        chkApplyOnLoad.TabIndex = 4
        chkApplyOnLoad.Text = "Aplică valorile în controale la încărcare"
        '
        ' grpClip — label/input pairs: column 0 AutoSize, column 1 100%
        '
        grpClip.AutoSize = True
        grpClip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpClip.Controls.Add(tlpClip)
        grpClip.Dock = System.Windows.Forms.DockStyle.Fill
        grpClip.Name = "grpClip"
        grpClip.TabIndex = 5
        grpClip.TabStop = False
        grpClip.Text = "Decupare"
        '
        tlpClip.AutoSize = True
        tlpClip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpClip.ColumnCount = 2
        tlpClip.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpClip.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpClip.Controls.Add(chkClip, 0, 0)
        tlpClip.Controls.Add(lblClipRight, 0, 1)
        tlpClip.Controls.Add(numClipRight, 1, 1)
        tlpClip.Controls.Add(lblClipTop, 0, 2)
        tlpClip.Controls.Add(numClipTop, 1, 2)
        tlpClip.Controls.Add(btnClipAuto, 0, 3)
        tlpClip.Dock = System.Windows.Forms.DockStyle.Fill
        tlpClip.Name = "tlpClip"
        tlpClip.RowCount = 4
        tlpClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpClip.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpClip.SetColumnSpan(chkClip, 2)
        tlpClip.SetColumnSpan(btnClipAuto, 2)
        tlpClip.TabIndex = 0
        '
        chkClip.AutoSize = True
        chkClip.Dock = System.Windows.Forms.DockStyle.Fill
        chkClip.Name = "chkClip"
        chkClip.TabIndex = 0
        chkClip.Text = "Decupare activă"
        '
        lblClipRight.AutoSize = True
        lblClipRight.Dock = System.Windows.Forms.DockStyle.Fill
        lblClipRight.Name = "lblClipRight"
        lblClipRight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        lblClipRight.TabIndex = 1
        lblClipRight.Text = "Decupare dreapta (px)"
        '
        numClipRight.Dock = System.Windows.Forms.DockStyle.Fill
        numClipRight.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numClipRight.Maximum = New Decimal(New Integer() {800, 0, 0, 0})
        numClipRight.Name = "numClipRight"
        numClipRight.TabIndex = 2
        '
        lblClipTop.AutoSize = True
        lblClipTop.Dock = System.Windows.Forms.DockStyle.Fill
        lblClipTop.Name = "lblClipTop"
        lblClipTop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        lblClipTop.TabIndex = 3
        lblClipTop.Text = "Decupare sus (px)"
        '
        numClipTop.Dock = System.Windows.Forms.DockStyle.Fill
        numClipTop.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numClipTop.Maximum = New Decimal(New Integer() {400, 0, 0, 0})
        numClipTop.Name = "numClipTop"
        numClipTop.TabIndex = 4
        '
        btnClipAuto.AutoSize = True
        btnClipAuto.Dock = System.Windows.Forms.DockStyle.Fill
        btnClipAuto.Enabled = False
        btnClipAuto.Name = "btnClipAuto"
        btnClipAuto.TabIndex = 5
        btnClipAuto.Text = "Măsoară din probă"
        btnClipAuto.UseVisualStyleBackColor = True
        '
        ' grpChildren
        '
        grpChildren.AutoSize = True
        grpChildren.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpChildren.Controls.Add(tlpChildren)
        grpChildren.Dock = System.Windows.Forms.DockStyle.Fill
        grpChildren.Name = "grpChildren"
        grpChildren.TabIndex = 6
        grpChildren.TabStop = False
        grpChildren.Text = "Ferestre copil"
        '
        tlpChildren.AutoSize = True
        tlpChildren.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpChildren.ColumnCount = 1
        tlpChildren.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpChildren.Controls.Add(lstChildren, 0, 0)
        tlpChildren.Controls.Add(btnHideChild, 0, 1)
        tlpChildren.Controls.Add(btnShowChild, 0, 2)
        tlpChildren.Controls.Add(btnShowAllChildren, 0, 3)
        tlpChildren.Dock = System.Windows.Forms.DockStyle.Fill
        tlpChildren.Name = "tlpChildren"
        tlpChildren.RowCount = 4
        tlpChildren.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChildren.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChildren.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChildren.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpChildren.TabIndex = 0
        '
        lstChildren.Dock = System.Windows.Forms.DockStyle.Fill
        lstChildren.IntegralHeight = False
        lstChildren.MinimumSize = New System.Drawing.Size(0, 120)
        lstChildren.Name = "lstChildren"
        lstChildren.TabIndex = 0
        '
        btnHideChild.AutoSize = True
        btnHideChild.Dock = System.Windows.Forms.DockStyle.Fill
        btnHideChild.Name = "btnHideChild"
        btnHideChild.TabIndex = 1
        btnHideChild.Text = "Ascunde fereastra selectată"
        btnHideChild.UseVisualStyleBackColor = True
        '
        btnShowChild.AutoSize = True
        btnShowChild.Dock = System.Windows.Forms.DockStyle.Fill
        btnShowChild.Name = "btnShowChild"
        btnShowChild.TabIndex = 2
        btnShowChild.Text = "Arată fereastra selectată"
        btnShowChild.UseVisualStyleBackColor = True
        '
        btnShowAllChildren.AutoSize = True
        btnShowAllChildren.Dock = System.Windows.Forms.DockStyle.Fill
        btnShowAllChildren.Name = "btnShowAllChildren"
        btnShowAllChildren.TabIndex = 3
        btnShowAllChildren.Text = "Restaurează toate"
        btnShowAllChildren.UseVisualStyleBackColor = True
        '
        ' grpKeys
        '
        grpKeys.AutoSize = True
        grpKeys.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpKeys.Controls.Add(tlpKeys)
        grpKeys.Dock = System.Windows.Forms.DockStyle.Fill
        grpKeys.Name = "grpKeys"
        grpKeys.TabIndex = 7
        grpKeys.TabStop = False
        grpKeys.Text = "Scurtături (experimental)"
        '
        tlpKeys.AutoSize = True
        tlpKeys.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpKeys.ColumnCount = 1
        tlpKeys.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpKeys.Controls.Add(btnSendShiftF4, 0, 0)
        tlpKeys.Controls.Add(btnSendF4, 0, 1)
        tlpKeys.Dock = System.Windows.Forms.DockStyle.Fill
        tlpKeys.Name = "tlpKeys"
        tlpKeys.RowCount = 2
        tlpKeys.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpKeys.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpKeys.TabIndex = 0
        '
        btnSendShiftF4.AutoSize = True
        btnSendShiftF4.Dock = System.Windows.Forms.DockStyle.Fill
        btnSendShiftF4.Name = "btnSendShiftF4"
        btnSendShiftF4.TabIndex = 0
        btnSendShiftF4.Text = "Trimite Shift+F4 (comută panoul de instrumente)"
        btnSendShiftF4.UseVisualStyleBackColor = True
        '
        btnSendF4.AutoSize = True
        btnSendF4.Dock = System.Windows.Forms.DockStyle.Fill
        btnSendF4.Name = "btnSendF4"
        btnSendF4.TabIndex = 1
        btnSendF4.Text = "Trimite F4 (comută panoul de navigare)"
        btnSendF4.UseVisualStyleBackColor = True
        '
        ' grpUser
        '
        grpUser.AutoSize = True
        grpUser.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpUser.Controls.Add(tlpUser)
        grpUser.Dock = System.Windows.Forms.DockStyle.Fill
        grpUser.Name = "grpUser"
        grpUser.TabIndex = 8
        grpUser.TabStop = False
        grpUser.Text = "Preferințe Adobe (utilizator, HKCU)"
        '
        tlpUser.AutoSize = True
        tlpUser.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpUser.ColumnCount = 1
        tlpUser.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpUser.Controls.Add(lblHive, 0, 0)
        tlpUser.Controls.Add(cboHive, 0, 1)
        tlpUser.Controls.Add(chkExpandRhp, 0, 2)
        tlpUser.Controls.Add(chkRhpSticky, 0, 3)
        tlpUser.Controls.Add(chkRhpCollapsed, 0, 4)
        tlpUser.Controls.Add(chkClassicViewer, 0, 5)
        tlpUser.Controls.Add(btnApplyUser, 0, 6)
        tlpUser.Controls.Add(btnRestoreUser, 0, 7)
        tlpUser.Controls.Add(chkRestoreOnClose, 0, 8)
        tlpUser.Dock = System.Windows.Forms.DockStyle.Fill
        tlpUser.Name = "tlpUser"
        tlpUser.RowCount = 9
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpUser.TabIndex = 0
        '
        lblHive.AutoSize = True
        lblHive.Dock = System.Windows.Forms.DockStyle.Fill
        lblHive.Name = "lblHive"
        lblHive.TabIndex = 0
        '
        cboHive.Dock = System.Windows.Forms.DockStyle.Fill
        cboHive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboHive.DropDownWidth = 420
        cboHive.Name = "cboHive"
        cboHive.TabIndex = 1
        '
        chkExpandRhp.AutoSize = True
        chkExpandRhp.Dock = System.Windows.Forms.DockStyle.Fill
        chkExpandRhp.Name = "chkExpandRhp"
        chkExpandRhp.TabIndex = 2
        chkExpandRhp.Text = "bExpandRHPInViewer = 0"
        '
        chkRhpSticky.AutoSize = True
        chkRhpSticky.Dock = System.Windows.Forms.DockStyle.Fill
        chkRhpSticky.Name = "chkRhpSticky"
        chkRhpSticky.TabIndex = 3
        chkRhpSticky.Text = "bRHPSticky = 1"
        '
        chkRhpCollapsed.AutoSize = True
        chkRhpCollapsed.Dock = System.Windows.Forms.DockStyle.Fill
        chkRhpCollapsed.Name = "chkRhpCollapsed"
        chkRhpCollapsed.TabIndex = 4
        chkRhpCollapsed.Text = "aDefaultRHPViewMode_L = Collapsed"
        '
        chkClassicViewer.AutoSize = True
        chkClassicViewer.Dock = System.Windows.Forms.DockStyle.Fill
        chkClassicViewer.Name = "chkClassicViewer"
        chkClassicViewer.TabIndex = 5
        chkClassicViewer.Text = "bEnableAv2 = 0 (interfața clasică)"
        '
        btnApplyUser.AutoSize = True
        btnApplyUser.Dock = System.Windows.Forms.DockStyle.Fill
        btnApplyUser.Name = "btnApplyUser"
        btnApplyUser.TabIndex = 6
        btnApplyUser.Text = "Aplică și repornește Adobe"
        btnApplyUser.UseVisualStyleBackColor = True
        '
        btnRestoreUser.AutoSize = True
        btnRestoreUser.Dock = System.Windows.Forms.DockStyle.Fill
        btnRestoreUser.Name = "btnRestoreUser"
        btnRestoreUser.TabIndex = 7
        btnRestoreUser.Text = "Restaurează valorile originale"
        btnRestoreUser.UseVisualStyleBackColor = True
        '
        chkRestoreOnClose.AutoSize = True
        chkRestoreOnClose.Checked = True
        chkRestoreOnClose.CheckState = System.Windows.Forms.CheckState.Checked
        chkRestoreOnClose.Dock = System.Windows.Forms.DockStyle.Fill
        chkRestoreOnClose.Name = "chkRestoreOnClose"
        chkRestoreOnClose.TabIndex = 8
        chkRestoreOnClose.Text = "Restaurează la închiderea bancului"
        '
        ' grpMachine
        '
        grpMachine.AutoSize = True
        grpMachine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpMachine.Controls.Add(tlpMachine)
        grpMachine.Dock = System.Windows.Forms.DockStyle.Fill
        grpMachine.Name = "grpMachine"
        grpMachine.TabIndex = 9
        grpMachine.TabStop = False
        grpMachine.Text = "Politici Adobe (mașină, HKLM)"
        '
        tlpMachine.AutoSize = True
        tlpMachine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpMachine.ColumnCount = 1
        tlpMachine.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpMachine.Controls.Add(cboProduct, 0, 0)
        tlpMachine.Controls.Add(chkSuppressUpsell, 0, 1)
        tlpMachine.Controls.Add(chkDisableServices, 0, 2)
        tlpMachine.Controls.Add(btnApplyMachine, 0, 3)
        tlpMachine.Controls.Add(btnRevertMachine, 0, 4)
        tlpMachine.Dock = System.Windows.Forms.DockStyle.Fill
        tlpMachine.Name = "tlpMachine"
        tlpMachine.RowCount = 5
        tlpMachine.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMachine.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMachine.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMachine.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMachine.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMachine.TabIndex = 0
        '
        cboProduct.Dock = System.Windows.Forms.DockStyle.Fill
        cboProduct.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        cboProduct.Name = "cboProduct"
        cboProduct.TabIndex = 0
        '
        chkSuppressUpsell.AutoSize = True
        chkSuppressUpsell.Dock = System.Windows.Forms.DockStyle.Fill
        chkSuppressUpsell.Name = "chkSuppressUpsell"
        chkSuppressUpsell.TabIndex = 1
        chkSuppressUpsell.Text = "bAcroSuppressUpsell = 1"
        '
        chkDisableServices.AutoSize = True
        chkDisableServices.Dock = System.Windows.Forms.DockStyle.Fill
        chkDisableServices.Name = "chkDisableServices"
        chkDisableServices.TabIndex = 2
        chkDisableServices.Text = "cServices\bToggleAdobeDocumentServices = 1"
        '
        btnApplyMachine.AutoSize = True
        btnApplyMachine.Dock = System.Windows.Forms.DockStyle.Fill
        btnApplyMachine.Name = "btnApplyMachine"
        btnApplyMachine.TabIndex = 3
        btnApplyMachine.Text = "Aplică (cere elevare)"
        btnApplyMachine.UseVisualStyleBackColor = True
        '
        btnRevertMachine.AutoSize = True
        btnRevertMachine.Dock = System.Windows.Forms.DockStyle.Fill
        btnRevertMachine.Name = "btnRevertMachine"
        btnRevertMachine.TabIndex = 4
        btnRevertMachine.Text = "Revocă (cere elevare)"
        btnRevertMachine.UseVisualStyleBackColor = True
        '
        ' grpCmd
        '
        grpCmd.AutoSize = True
        grpCmd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpCmd.Controls.Add(tlpCmd)
        grpCmd.Dock = System.Windows.Forms.DockStyle.Fill
        grpCmd.Name = "grpCmd"
        grpCmd.TabIndex = 10
        grpCmd.TabStop = False
        grpCmd.Text = "Linie de comandă"
        '
        tlpCmd.AutoSize = True
        tlpCmd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpCmd.ColumnCount = 1
        tlpCmd.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpCmd.Controls.Add(txtCmd, 0, 0)
        tlpCmd.Dock = System.Windows.Forms.DockStyle.Fill
        tlpCmd.Name = "tlpCmd"
        tlpCmd.RowCount = 1
        tlpCmd.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpCmd.TabIndex = 0
        '
        txtCmd.Dock = System.Windows.Forms.DockStyle.Fill
        txtCmd.Font = New System.Drawing.Font("Consolas", 8.25F)
        txtCmd.Multiline = True
        txtCmd.MinimumSize = New System.Drawing.Size(0, 72)
        txtCmd.Name = "txtCmd"
        txtCmd.ReadOnly = True
        txtCmd.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        txtCmd.TabIndex = 0
        '
        ' pnlButtons — verdictul uman (stays where the harness framework puts it)
        '
        pnlButtons.AutoSize = True
        pnlButtons.Controls.Add(btnPass)
        pnlButtons.Controls.Add(btnFail)
        pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        pnlButtons.Location = New System.Drawing.Point(0, 727)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        pnlButtons.Size = New System.Drawing.Size(1240, 53)
        pnlButtons.TabIndex = 1
        '
        btnPass.AutoSize = True
        btnPass.DialogResult = System.Windows.Forms.DialogResult.OK
        btnPass.Name = "btnPass"
        btnPass.TabIndex = 0
        btnPass.Text = "Pass"
        btnPass.UseVisualStyleBackColor = True
        '
        btnFail.AutoSize = True
        btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnFail.Name = "btnFail"
        btnFail.TabIndex = 1
        btnFail.Text = "Fail"
        btnFail.UseVisualStyleBackColor = True
        '
        ' AdobeReaderHarnessForm
        '
        AcceptButton = btnPass
        AutoScaleDimensions = New System.Drawing.SizeF(10F, 25F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        CancelButton = btnFail
        ClientSize = New System.Drawing.Size(1240, 780)
        ' Dock order (house rule): Fill first, then Bottom.
        Controls.Add(splitMain)
        Controls.Add(pnlButtons)
        Name = "AdobeReaderHarnessForm"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Adobe Reader DC — încorporare + switch-uri (bare ascunse)"
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        tlpOptions.ResumeLayout(False)
        tlpOptions.PerformLayout()
        tlpRight.ResumeLayout(False)
        tlpRight.PerformLayout()
        grpLaunch.ResumeLayout(False)
        grpLaunch.PerformLayout()
        tlpLaunch.ResumeLayout(False)
        tlpLaunch.PerformLayout()
        grpChrome.ResumeLayout(False)
        grpChrome.PerformLayout()
        tlpChrome.ResumeLayout(False)
        tlpChrome.PerformLayout()
        grpFile.ResumeLayout(False)
        grpFile.PerformLayout()
        tlpFile.ResumeLayout(False)
        tlpFile.PerformLayout()
        grpProbe.ResumeLayout(False)
        grpProbe.PerformLayout()
        tlpProbe.ResumeLayout(False)
        tlpProbe.PerformLayout()
        grpScenario.ResumeLayout(False)
        grpScenario.PerformLayout()
        tlpScenario.ResumeLayout(False)
        tlpScenario.PerformLayout()
        grpClip.ResumeLayout(False)
        grpClip.PerformLayout()
        tlpClip.ResumeLayout(False)
        tlpClip.PerformLayout()
        CType(numClipRight, System.ComponentModel.ISupportInitialize).EndInit()
        CType(numClipTop, System.ComponentModel.ISupportInitialize).EndInit()
        grpChildren.ResumeLayout(False)
        grpChildren.PerformLayout()
        tlpChildren.ResumeLayout(False)
        tlpChildren.PerformLayout()
        grpKeys.ResumeLayout(False)
        grpKeys.PerformLayout()
        tlpKeys.ResumeLayout(False)
        tlpKeys.PerformLayout()
        grpUser.ResumeLayout(False)
        grpUser.PerformLayout()
        tlpUser.ResumeLayout(False)
        tlpUser.PerformLayout()
        grpMachine.ResumeLayout(False)
        grpMachine.PerformLayout()
        tlpMachine.ResumeLayout(False)
        tlpMachine.PerformLayout()
        grpCmd.ResumeLayout(False)
        grpCmd.PerformLayout()
        tlpCmd.ResumeLayout(False)
        tlpCmd.PerformLayout()
        pnlButtons.ResumeLayout(False)
        pnlButtons.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
End Class
