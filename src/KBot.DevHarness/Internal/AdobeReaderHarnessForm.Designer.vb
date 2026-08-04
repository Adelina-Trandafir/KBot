<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdobeReaderHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' Adobe embed test bench. Layout (slice 0023, config+layout pass): a vertical SplitContainer
    ' the operator can drag at runtime — Panel1 holds the options as one GroupBox + TableLayoutPanel
    ' per section stacked in a SCROLLING FlowLayoutPanel, Panel2 holds the Adobe host (100%) over
    ' the status line (AutoSize). The Adobe area can be traded against the options area.
    ' House rule: ALL WinForms controls are declared here, in .Designer.vb — including the
    ' SplitContainer and every TableLayoutPanel, so the layout stays editable in the designer.

    Private components As System.ComponentModel.IContainer

    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    ' The options stack is a FlowLayoutPanel, NOT a TableLayoutPanel: a TLP with AutoScroll and a
    ' Percent filler row reports that its content always fits, so it never shows a scrollbar and
    ' silently clips every section past the fold (that defect made both registry sections
    ' unreachable). A TopDown FlowLayoutPanel with AutoScroll scrolls this content reliably — it is
    ' the same container that worked here before the layout rework.
    Friend WithEvents flowOptions As System.Windows.Forms.FlowLayoutPanel
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

    ' Diagnostic — the child window probe + the machine-state read.
    Friend WithEvents grpProbe As System.Windows.Forms.GroupBox
    Friend WithEvents tlpProbe As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents btnProbe As System.Windows.Forms.Button
    Friend WithEvents btnMachineState As System.Windows.Forms.Button

    ' Scenariu — load / run / save a scenario file (JSON, AppDir\Config).
    Friend WithEvents grpScenario As System.Windows.Forms.GroupBox
    Friend WithEvents tlpScenario As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblScenario As System.Windows.Forms.Label
    Friend WithEvents btnLoadScenario As System.Windows.Forms.Button
    Friend WithEvents btnRunScenario As System.Windows.Forms.Button
    Friend WithEvents btnSaveScenario As System.Windows.Forms.Button

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

    ' Mută ferestre copil — move/resize a child window instead of hiding it or clipping the host.
    Friend WithEvents grpMove As System.Windows.Forms.GroupBox
    Friend WithEvents tlpMove As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblMoveTarget As System.Windows.Forms.Label
    Friend WithEvents txtMoveTarget As System.Windows.Forms.TextBox
    Friend WithEvents lblDx As System.Windows.Forms.Label
    Friend WithEvents numDx As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDy As System.Windows.Forms.Label
    Friend WithEvents numDy As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDw As System.Windows.Forms.Label
    Friend WithEvents numDw As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDh As System.Windows.Forms.Label
    Friend WithEvents numDh As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnApplyMove As System.Windows.Forms.Button
    Friend WithEvents btnResetMoves As System.Windows.Forms.Button
    Friend WithEvents chkReapplyMoves As System.Windows.Forms.CheckBox
    Friend WithEvents lblReapplyMs As System.Windows.Forms.Label
    Friend WithEvents numReapplyMs As System.Windows.Forms.NumericUpDown
    ' Re-imposes the recorded moves; Adobe puts its windows back on resize/zoom/document change.
    Friend WithEvents tmrReapplyMoves As System.Windows.Forms.Timer

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
    ' One ROW per preference — «nu atinge» / «șterge» / a literal value. NOT checkboxes: a
    ' checkbox has two states and the schema has four, so «bEnableAv2 = 0» could never express 1.
    Friend WithEvents tlpPrefRows As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblExpandRhp As System.Windows.Forms.Label
    Friend WithEvents cboExpandRhp As System.Windows.Forms.ComboBox
    Friend WithEvents lblRhpSticky As System.Windows.Forms.Label
    Friend WithEvents cboRhpSticky As System.Windows.Forms.ComboBox
    Friend WithEvents lblRhpViewMode As System.Windows.Forms.Label
    Friend WithEvents cboRhpViewMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblEnableAv2 As System.Windows.Forms.Label
    Friend WithEvents cboEnableAv2 As System.Windows.Forms.ComboBox
    Friend WithEvents lblPrefHint As System.Windows.Forms.Label
    ' Read-only view of what the loaded scenario ASKED FOR vs what the machine currently holds —
    ' the display that makes a clamped or refused value obvious instead of invisible.
    Friend WithEvents gridPrefs As System.Windows.Forms.DataGridView
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
    Friend WithEvents chkRevertPolicyOnClose As System.Windows.Forms.CheckBox

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
        flowOptions = New System.Windows.Forms.FlowLayoutPanel()
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
        btnMachineState = New System.Windows.Forms.Button()
        grpScenario = New System.Windows.Forms.GroupBox()
        tlpScenario = New System.Windows.Forms.TableLayoutPanel()
        lblScenario = New System.Windows.Forms.Label()
        btnLoadScenario = New System.Windows.Forms.Button()
        btnRunScenario = New System.Windows.Forms.Button()
        btnSaveScenario = New System.Windows.Forms.Button()
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
        grpMove = New System.Windows.Forms.GroupBox()
        tlpMove = New System.Windows.Forms.TableLayoutPanel()
        lblMoveTarget = New System.Windows.Forms.Label()
        txtMoveTarget = New System.Windows.Forms.TextBox()
        lblDx = New System.Windows.Forms.Label()
        numDx = New System.Windows.Forms.NumericUpDown()
        lblDy = New System.Windows.Forms.Label()
        numDy = New System.Windows.Forms.NumericUpDown()
        lblDw = New System.Windows.Forms.Label()
        numDw = New System.Windows.Forms.NumericUpDown()
        lblDh = New System.Windows.Forms.Label()
        numDh = New System.Windows.Forms.NumericUpDown()
        btnApplyMove = New System.Windows.Forms.Button()
        btnResetMoves = New System.Windows.Forms.Button()
        chkReapplyMoves = New System.Windows.Forms.CheckBox()
        lblReapplyMs = New System.Windows.Forms.Label()
        numReapplyMs = New System.Windows.Forms.NumericUpDown()
        tmrReapplyMoves = New System.Windows.Forms.Timer(components)
        grpKeys = New System.Windows.Forms.GroupBox()
        tlpKeys = New System.Windows.Forms.TableLayoutPanel()
        btnSendShiftF4 = New System.Windows.Forms.Button()
        btnSendF4 = New System.Windows.Forms.Button()
        grpUser = New System.Windows.Forms.GroupBox()
        tlpUser = New System.Windows.Forms.TableLayoutPanel()
        lblHive = New System.Windows.Forms.Label()
        cboHive = New System.Windows.Forms.ComboBox()
        tlpPrefRows = New System.Windows.Forms.TableLayoutPanel()
        lblExpandRhp = New System.Windows.Forms.Label()
        cboExpandRhp = New System.Windows.Forms.ComboBox()
        lblRhpSticky = New System.Windows.Forms.Label()
        cboRhpSticky = New System.Windows.Forms.ComboBox()
        lblRhpViewMode = New System.Windows.Forms.Label()
        cboRhpViewMode = New System.Windows.Forms.ComboBox()
        lblEnableAv2 = New System.Windows.Forms.Label()
        cboEnableAv2 = New System.Windows.Forms.ComboBox()
        lblPrefHint = New System.Windows.Forms.Label()
        gridPrefs = New System.Windows.Forms.DataGridView()
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
        chkRevertPolicyOnClose = New System.Windows.Forms.CheckBox()
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
        flowOptions.SuspendLayout()
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
        grpMove.SuspendLayout()
        tlpMove.SuspendLayout()
        CType(numDx, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(numDy, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(numDw, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(numDh, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(numReapplyMs, System.ComponentModel.ISupportInitialize).BeginInit()
        grpKeys.SuspendLayout()
        tlpKeys.SuspendLayout()
        grpUser.SuspendLayout()
        tlpUser.SuspendLayout()
        tlpPrefRows.SuspendLayout()
        CType(gridPrefs, System.ComponentModel.ISupportInitialize).BeginInit()
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
        splitMain.Panel1.Controls.Add(flowOptions)
        splitMain.Panel1MinSize = 300
        splitMain.Panel2.Controls.Add(tlpRight)
        splitMain.Panel2MinSize = 200
        splitMain.Size = New System.Drawing.Size(1240, 727)
        ' 470, not 320: the operator's own DPI needs ~430px for the option captions
        ' (chkNewInstance measured 427 and chkDisableServices 412 in the designer-regenerated file).
        splitMain.SplitterDistance = 470
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 0
        '
        ' flowOptions — the scrolling stack of sections (top-down, no wrapping). Section widths are
        ' tracked to the panel in AdobeReaderHarnessForm.SizeSections; the GroupBoxes deliberately
        ' do NOT dock, because Dock inside a FlowLayoutPanel fights AutoSize.
        '
        flowOptions.AutoScroll = True
        flowOptions.Controls.Add(grpLaunch)
        flowOptions.Controls.Add(grpChrome)
        flowOptions.Controls.Add(grpFile)
        flowOptions.Controls.Add(grpProbe)
        flowOptions.Controls.Add(grpScenario)
        flowOptions.Controls.Add(grpClip)
        flowOptions.Controls.Add(grpChildren)
        flowOptions.Controls.Add(grpMove)
        flowOptions.Controls.Add(grpKeys)
        flowOptions.Controls.Add(grpUser)
        flowOptions.Controls.Add(grpMachine)
        flowOptions.Controls.Add(grpCmd)
        flowOptions.Dock = System.Windows.Forms.DockStyle.Fill
        flowOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        flowOptions.Location = New System.Drawing.Point(0, 0)
        flowOptions.Name = "flowOptions"
        flowOptions.Padding = New System.Windows.Forms.Padding(6)
        flowOptions.Size = New System.Drawing.Size(470, 727)
        flowOptions.TabIndex = 0
        flowOptions.WrapContents = False
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
        tlpProbe.Controls.Add(btnMachineState, 0, 1)
        tlpProbe.Dock = System.Windows.Forms.DockStyle.Fill
        tlpProbe.Name = "tlpProbe"
        tlpProbe.RowCount = 2
        tlpProbe.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
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
        btnMachineState.AutoSize = True
        btnMachineState.Dock = System.Windows.Forms.DockStyle.Fill
        btnMachineState.Name = "btnMachineState"
        btnMachineState.TabIndex = 1
        btnMachineState.Text = "Starea mașinii (Adobe + registry)"
        btnMachineState.UseVisualStyleBackColor = True
        '
        ' grpScenario
        '
        grpScenario.AutoSize = True
        grpScenario.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpScenario.Controls.Add(tlpScenario)
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
        tlpScenario.Dock = System.Windows.Forms.DockStyle.Fill
        tlpScenario.Name = "tlpScenario"
        tlpScenario.RowCount = 4
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
        '
        ' grpClip — label/input pairs: column 0 AutoSize, column 1 100%
        '
        grpClip.AutoSize = True
        grpClip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpClip.Controls.Add(tlpClip)
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
        ' grpMove — move/resize a child instead of hiding it or inflating the host
        '
        grpMove.AutoSize = True
        grpMove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpMove.Controls.Add(tlpMove)
        grpMove.Name = "grpMove"
        grpMove.TabIndex = 7
        grpMove.TabStop = False
        grpMove.Text = "Mută ferestre copil"
        '
        tlpMove.AutoSize = True
        tlpMove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpMove.ColumnCount = 2
        tlpMove.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpMove.Controls.Add(lblMoveTarget, 0, 0)
        tlpMove.Controls.Add(txtMoveTarget, 1, 0)
        tlpMove.Controls.Add(lblDx, 0, 1)
        tlpMove.Controls.Add(numDx, 1, 1)
        tlpMove.Controls.Add(lblDy, 0, 2)
        tlpMove.Controls.Add(numDy, 1, 2)
        tlpMove.Controls.Add(lblDw, 0, 3)
        tlpMove.Controls.Add(numDw, 1, 3)
        tlpMove.Controls.Add(lblDh, 0, 4)
        tlpMove.Controls.Add(numDh, 1, 4)
        tlpMove.Controls.Add(btnApplyMove, 0, 5)
        tlpMove.SetColumnSpan(btnApplyMove, 2)
        tlpMove.Controls.Add(btnResetMoves, 0, 6)
        tlpMove.SetColumnSpan(btnResetMoves, 2)
        tlpMove.Controls.Add(chkReapplyMoves, 0, 7)
        tlpMove.SetColumnSpan(chkReapplyMoves, 2)
        tlpMove.Controls.Add(lblReapplyMs, 0, 8)
        tlpMove.Controls.Add(numReapplyMs, 1, 8)
        tlpMove.Dock = System.Windows.Forms.DockStyle.Fill
        tlpMove.Name = "tlpMove"
        tlpMove.RowCount = 9
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpMove.TabIndex = 0
        '
        lblMoveTarget.AutoSize = True
        lblMoveTarget.Dock = System.Windows.Forms.DockStyle.Fill
        lblMoveTarget.Name = "lblMoveTarget"
        lblMoveTarget.TabIndex = 0
        lblMoveTarget.Text = "Text fereastră"
        lblMoveTarget.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' Filled by selecting an entry in lstChildren; still editable, because a scenario may target
        ' a window that is not in the current probe.
        txtMoveTarget.Dock = System.Windows.Forms.DockStyle.Fill
        txtMoveTarget.Name = "txtMoveTarget"
        txtMoveTarget.TabIndex = 1
        '
        lblDx.AutoSize = True
        lblDx.Dock = System.Windows.Forms.DockStyle.Fill
        lblDx.Name = "lblDx"
        lblDx.TabIndex = 2
        lblDx.Text = "dx (stânga −)"
        lblDx.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        numDx.Dock = System.Windows.Forms.DockStyle.Fill
        numDx.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDx.Minimum = New Decimal(New Integer() {2000, 0, 0, -2147483648})
        numDx.Name = "numDx"
        numDx.TabIndex = 3
        '
        lblDy.AutoSize = True
        lblDy.Dock = System.Windows.Forms.DockStyle.Fill
        lblDy.Name = "lblDy"
        lblDy.TabIndex = 4
        lblDy.Text = "dy (sus −)"
        lblDy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        numDy.Dock = System.Windows.Forms.DockStyle.Fill
        numDy.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDy.Minimum = New Decimal(New Integer() {2000, 0, 0, -2147483648})
        numDy.Name = "numDy"
        numDy.TabIndex = 5
        '
        lblDw.AutoSize = True
        lblDw.Dock = System.Windows.Forms.DockStyle.Fill
        lblDw.Name = "lblDw"
        lblDw.TabIndex = 6
        lblDw.Text = "dw (lățime)"
        lblDw.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        numDw.Dock = System.Windows.Forms.DockStyle.Fill
        numDw.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDw.Minimum = New Decimal(New Integer() {2000, 0, 0, -2147483648})
        numDw.Name = "numDw"
        numDw.TabIndex = 7
        '
        lblDh.AutoSize = True
        lblDh.Dock = System.Windows.Forms.DockStyle.Fill
        lblDh.Name = "lblDh"
        lblDh.TabIndex = 8
        lblDh.Text = "dh (înălțime)"
        lblDh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        numDh.Dock = System.Windows.Forms.DockStyle.Fill
        numDh.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDh.Minimum = New Decimal(New Integer() {2000, 0, 0, -2147483648})
        numDh.Name = "numDh"
        numDh.TabIndex = 9
        '
        btnApplyMove.AutoSize = True
        btnApplyMove.Dock = System.Windows.Forms.DockStyle.Fill
        btnApplyMove.Name = "btnApplyMove"
        btnApplyMove.TabIndex = 10
        btnApplyMove.Text = "Aplică mutarea"
        btnApplyMove.UseVisualStyleBackColor = True
        '
        btnResetMoves.AutoSize = True
        btnResetMoves.Dock = System.Windows.Forms.DockStyle.Fill
        btnResetMoves.Name = "btnResetMoves"
        btnResetMoves.TabIndex = 11
        btnResetMoves.Text = "Readu la poziția inițială"
        btnResetMoves.UseVisualStyleBackColor = True
        '
        chkReapplyMoves.AutoSize = True
        chkReapplyMoves.Dock = System.Windows.Forms.DockStyle.Fill
        chkReapplyMoves.Name = "chkReapplyMoves"
        chkReapplyMoves.TabIndex = 12
        chkReapplyMoves.Text = "Reaplică periodic (Adobe reface aranjarea)"
        '
        lblReapplyMs.AutoSize = True
        lblReapplyMs.Dock = System.Windows.Forms.DockStyle.Fill
        lblReapplyMs.Name = "lblReapplyMs"
        lblReapplyMs.TabIndex = 13
        lblReapplyMs.Text = "Interval (ms)"
        lblReapplyMs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        numReapplyMs.Dock = System.Windows.Forms.DockStyle.Fill
        numReapplyMs.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        numReapplyMs.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        numReapplyMs.Minimum = New Decimal(New Integer() {50, 0, 0, 0})
        numReapplyMs.Name = "numReapplyMs"
        numReapplyMs.TabIndex = 14
        numReapplyMs.Value = New Decimal(New Integer() {500, 0, 0, 0})
        '
        ' tmrReapplyMoves — interval driven by numReapplyMs; started only by chkReapplyMoves
        '
        tmrReapplyMoves.Interval = 500
        '
        ' grpKeys
        '
        grpKeys.AutoSize = True
        grpKeys.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpKeys.Controls.Add(tlpKeys)
        grpKeys.Name = "grpKeys"
        grpKeys.TabIndex = 8
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
        grpUser.Name = "grpUser"
        grpUser.TabIndex = 9
        grpUser.TabStop = False
        grpUser.Text = "Preferințe Adobe (utilizator, HKCU)"
        '
        tlpUser.AutoSize = True
        tlpUser.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpUser.ColumnCount = 1
        tlpUser.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpUser.Controls.Add(lblHive, 0, 0)
        tlpUser.Controls.Add(cboHive, 0, 1)
        tlpUser.Controls.Add(tlpPrefRows, 0, 2)
        tlpUser.Controls.Add(lblPrefHint, 0, 3)
        tlpUser.Controls.Add(gridPrefs, 0, 4)
        tlpUser.Controls.Add(btnApplyUser, 0, 5)
        tlpUser.Controls.Add(btnRestoreUser, 0, 6)
        tlpUser.Controls.Add(chkRestoreOnClose, 0, 7)
        tlpUser.Dock = System.Windows.Forms.DockStyle.Fill
        tlpUser.Name = "tlpUser"
        tlpUser.RowCount = 8
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
        ' tlpPrefRows — one «nume | valoare» row per HKCU preference. Column 0 AutoSize (the value
        ' name), column 1 Percent 100 (the editable combo), so the combos line up under each other.
        '
        tlpPrefRows.AutoSize = True
        tlpPrefRows.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        tlpPrefRows.ColumnCount = 2
        tlpPrefRows.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpPrefRows.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlpPrefRows.Controls.Add(lblExpandRhp, 0, 0)
        tlpPrefRows.Controls.Add(cboExpandRhp, 1, 0)
        tlpPrefRows.Controls.Add(lblRhpSticky, 0, 1)
        tlpPrefRows.Controls.Add(cboRhpSticky, 1, 1)
        tlpPrefRows.Controls.Add(lblRhpViewMode, 0, 2)
        tlpPrefRows.Controls.Add(cboRhpViewMode, 1, 2)
        tlpPrefRows.Controls.Add(lblEnableAv2, 0, 3)
        tlpPrefRows.Controls.Add(cboEnableAv2, 1, 3)
        tlpPrefRows.Dock = System.Windows.Forms.DockStyle.Fill
        tlpPrefRows.Name = "tlpPrefRows"
        tlpPrefRows.RowCount = 4
        tlpPrefRows.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpPrefRows.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpPrefRows.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpPrefRows.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize))
        tlpPrefRows.TabIndex = 2
        '
        ' gridPrefs — read-only: Valoare · Cerut · Curent · Tip
        '
        gridPrefs.AllowUserToAddRows = False
        gridPrefs.AllowUserToDeleteRows = False
        gridPrefs.AllowUserToResizeRows = False
        gridPrefs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        gridPrefs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        gridPrefs.Dock = System.Windows.Forms.DockStyle.Fill
        gridPrefs.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically
        gridPrefs.MinimumSize = New System.Drawing.Size(0, 110)
        gridPrefs.MultiSelect = False
        gridPrefs.Name = "gridPrefs"
        gridPrefs.ReadOnly = True
        gridPrefs.RowHeadersVisible = False
        gridPrefs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        gridPrefs.TabIndex = 6
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
        lblExpandRhp.AutoSize = True
        lblExpandRhp.Dock = System.Windows.Forms.DockStyle.Fill
        lblExpandRhp.Name = "lblExpandRhp"
        lblExpandRhp.TabIndex = 0
        lblExpandRhp.Text = "bExpandRHPInViewer"
        lblExpandRhp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' DropDown (editabil), nu DropDownList: scenariile pot cere orice întreg, nu doar 0/1.
        cboExpandRhp.Dock = System.Windows.Forms.DockStyle.Fill
        cboExpandRhp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        cboExpandRhp.Name = "cboExpandRhp"
        cboExpandRhp.TabIndex = 1
        '
        lblRhpSticky.AutoSize = True
        lblRhpSticky.Dock = System.Windows.Forms.DockStyle.Fill
        lblRhpSticky.Name = "lblRhpSticky"
        lblRhpSticky.TabIndex = 2
        lblRhpSticky.Text = "bRHPSticky"
        lblRhpSticky.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        cboRhpSticky.Dock = System.Windows.Forms.DockStyle.Fill
        cboRhpSticky.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        cboRhpSticky.Name = "cboRhpSticky"
        cboRhpSticky.TabIndex = 3
        '
        lblRhpViewMode.AutoSize = True
        lblRhpViewMode.Dock = System.Windows.Forms.DockStyle.Fill
        lblRhpViewMode.Name = "lblRhpViewMode"
        lblRhpViewMode.TabIndex = 4
        lblRhpViewMode.Text = "aDefaultRHPViewMode_L"
        lblRhpViewMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' REG_SZ: text liber, cu «Collapsed»/«Expanded» doar ca sugestii în listă.
        cboRhpViewMode.Dock = System.Windows.Forms.DockStyle.Fill
        cboRhpViewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        cboRhpViewMode.Name = "cboRhpViewMode"
        cboRhpViewMode.TabIndex = 5
        '
        lblEnableAv2.AutoSize = True
        lblEnableAv2.Dock = System.Windows.Forms.DockStyle.Fill
        lblEnableAv2.Name = "lblEnableAv2"
        lblEnableAv2.TabIndex = 6
        lblEnableAv2.Text = "bEnableAv2 (0 = clasic, 1 = modern)"
        lblEnableAv2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        cboEnableAv2.Dock = System.Windows.Forms.DockStyle.Fill
        cboEnableAv2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        cboEnableAv2.Name = "cboEnableAv2"
        cboEnableAv2.TabIndex = 7
        '
        lblPrefHint.AutoSize = True
        lblPrefHint.Dock = System.Windows.Forms.DockStyle.Fill
        lblPrefHint.Name = "lblPrefHint"
        lblPrefHint.TabIndex = 3
        lblPrefHint.Text = "«nu atinge» lasă valoarea exact cum e (NU o scrie pe 0); «șterge» o elimină din registry; " &
                           "orice altceva se scrie literal."
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
        ' DEBIFAT implicit: bifat, el anulează experimentul la fiecare închidere a bancului — exact
        ' ce s-a întâmplat cu bEnableAv2 = 1, readus la 0 fără ca nimeni să ceară asta.
        chkRestoreOnClose.AutoSize = True
        chkRestoreOnClose.Dock = System.Windows.Forms.DockStyle.Fill
        chkRestoreOnClose.Name = "chkRestoreOnClose"
        chkRestoreOnClose.TabIndex = 8
        chkRestoreOnClose.Text = "Restaurează valorile HKCU la închiderea bancului"
        '
        ' grpMachine
        '
        grpMachine.AutoSize = True
        grpMachine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpMachine.Controls.Add(tlpMachine)
        grpMachine.Name = "grpMachine"
        grpMachine.TabIndex = 10
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
        tlpMachine.Controls.Add(chkRevertPolicyOnClose, 0, 5)
        tlpMachine.Dock = System.Windows.Forms.DockStyle.Fill
        tlpMachine.Name = "tlpMachine"
        tlpMachine.RowCount = 6
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
        chkRevertPolicyOnClose.AutoSize = True
        chkRevertPolicyOnClose.Checked = True
        chkRevertPolicyOnClose.CheckState = System.Windows.Forms.CheckState.Checked
        chkRevertPolicyOnClose.Dock = System.Windows.Forms.DockStyle.Fill
        chkRevertPolicyOnClose.Name = "chkRevertPolicyOnClose"
        chkRevertPolicyOnClose.TabIndex = 5
        chkRevertPolicyOnClose.Text = "Revocă politica HKLM la închiderea bancului"
        '
        ' grpCmd
        '
        grpCmd.AutoSize = True
        grpCmd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        grpCmd.Controls.Add(tlpCmd)
        grpCmd.Name = "grpCmd"
        grpCmd.TabIndex = 11
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
        flowOptions.ResumeLayout(False)
        flowOptions.PerformLayout()
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
        CType(numDx, System.ComponentModel.ISupportInitialize).EndInit()
        CType(numDy, System.ComponentModel.ISupportInitialize).EndInit()
        CType(numDw, System.ComponentModel.ISupportInitialize).EndInit()
        CType(numDh, System.ComponentModel.ISupportInitialize).EndInit()
        CType(numReapplyMs, System.ComponentModel.ISupportInitialize).EndInit()
        grpMove.ResumeLayout(False)
        grpMove.PerformLayout()
        tlpMove.ResumeLayout(False)
        tlpMove.PerformLayout()
        grpKeys.ResumeLayout(False)
        grpKeys.PerformLayout()
        tlpKeys.ResumeLayout(False)
        tlpKeys.PerformLayout()
        grpUser.ResumeLayout(False)
        grpUser.PerformLayout()
        CType(gridPrefs, System.ComponentModel.ISupportInitialize).EndInit()
        tlpUser.ResumeLayout(False)
        tlpUser.PerformLayout()
        tlpPrefRows.ResumeLayout(False)
        tlpPrefRows.PerformLayout()
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
