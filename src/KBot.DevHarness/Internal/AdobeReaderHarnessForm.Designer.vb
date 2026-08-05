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

    ' Poziția ferestrei Adobe — dx/dy/dw/dh on the HOSTED window, exactly like clip right/top.
    Friend WithEvents grpMove As System.Windows.Forms.GroupBox
    ' ── Felia 0024-03: «Închidere / captură» ────────────────────────────────────
    Friend WithEvents grpHosting As System.Windows.Forms.GroupBox
    Friend WithEvents tlpHosting As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblDetachMode As System.Windows.Forms.Label
    Friend WithEvents flowDetachMode As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents rdoDetachKill As System.Windows.Forms.RadioButton
    Friend WithEvents rdoDetachClose As System.Windows.Forms.RadioButton
    Friend WithEvents lblCaptureDelay As System.Windows.Forms.Label
    Friend WithEvents numCaptureDelay As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblCloseGrace As System.Windows.Forms.Label
    Friend WithEvents numCloseGrace As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkCreationHook As System.Windows.Forms.CheckBox
    Friend WithEvents chkForceClassicUi As System.Windows.Forms.CheckBox
    Friend WithEvents lblEmbedTiming As System.Windows.Forms.Label
    ' ── Felia 0024-03: «ActiveX (AcroPDF)» ──────────────────────────────────────
    Friend WithEvents grpActiveX As System.Windows.Forms.GroupBox
    Friend WithEvents tlpActiveX As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblAcroStatus As System.Windows.Forms.Label
    Friend WithEvents flowAcroButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnAcroLoad As System.Windows.Forms.Button
    Friend WithEvents btnAcroSecond As System.Windows.Forms.Button
    Friend WithEvents btnAcroClear As System.Windows.Forms.Button
    Friend WithEvents chkAcroChrome As System.Windows.Forms.CheckBox
    Friend WithEvents btnAcroProbe As System.Windows.Forms.Button
    Friend WithEvents btnAcroPrefs As System.Windows.Forms.Button
    Friend WithEvents btnAcroHideChrome As System.Windows.Forms.Button
    Friend WithEvents pnlAcroHost As System.Windows.Forms.Panel
    Friend WithEvents tlpMove As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents lblDx As System.Windows.Forms.Label
    Friend WithEvents numDx As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDy As System.Windows.Forms.Label
    Friend WithEvents numDy As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDw As System.Windows.Forms.Label
    Friend WithEvents numDw As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblDh As System.Windows.Forms.Label
    Friend WithEvents numDh As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnResetMove As System.Windows.Forms.Button
    Friend WithEvents lblMoveHint As System.Windows.Forms.Label

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
        components = New ComponentModel.Container()
        splitMain = New SplitContainer()
        flowOptions = New FlowLayoutPanel()
        grpLaunch = New GroupBox()
        tlpLaunch = New TableLayoutPanel()
        chkNewInstance = New CheckBox()
        chkNoSplash = New CheckBox()
        grpChrome = New GroupBox()
        tlpChrome = New TableLayoutPanel()
        chkToolbar = New CheckBox()
        chkNavpanes = New CheckBox()
        chkStatusbar = New CheckBox()
        chkMessages = New CheckBox()
        chkScrollbar = New CheckBox()
        chkPagemodeNone = New CheckBox()
        grpFile = New GroupBox()
        tlpFile = New TableLayoutPanel()
        btnBrowse = New Button()
        lblFile = New Label()
        btnRelaunch = New Button()
        grpProbe = New GroupBox()
        tlpProbe = New TableLayoutPanel()
        btnProbe = New Button()
        btnMachineState = New Button()
        grpScenario = New GroupBox()
        tlpScenario = New TableLayoutPanel()
        lblScenario = New Label()
        btnLoadScenario = New Button()
        btnRunScenario = New Button()
        btnSaveScenario = New Button()
        grpClip = New GroupBox()
        tlpClip = New TableLayoutPanel()
        chkClip = New CheckBox()
        lblClipRight = New Label()
        numClipRight = New NumericUpDown()
        lblClipTop = New Label()
        numClipTop = New NumericUpDown()
        btnClipAuto = New Button()
        grpMove = New GroupBox()
        grpHosting = New GroupBox()
        tlpHosting = New TableLayoutPanel()
        lblDetachMode = New Label()
        flowDetachMode = New FlowLayoutPanel()
        rdoDetachKill = New RadioButton()
        rdoDetachClose = New RadioButton()
        lblCaptureDelay = New Label()
        numCaptureDelay = New NumericUpDown()
        lblCloseGrace = New Label()
        numCloseGrace = New NumericUpDown()
        chkCreationHook = New CheckBox()
        chkForceClassicUi = New CheckBox()
        lblEmbedTiming = New Label()
        grpActiveX = New GroupBox()
        tlpActiveX = New TableLayoutPanel()
        lblAcroStatus = New Label()
        flowAcroButtons = New FlowLayoutPanel()
        btnAcroLoad = New Button()
        btnAcroSecond = New Button()
        btnAcroClear = New Button()
        chkAcroChrome = New CheckBox()
        btnAcroProbe = New Button()
        btnAcroPrefs = New Button()
        btnAcroHideChrome = New Button()
        pnlAcroHost = New Panel()
        tlpMove = New TableLayoutPanel()
        lblDx = New Label()
        numDx = New NumericUpDown()
        lblDy = New Label()
        numDy = New NumericUpDown()
        lblDw = New Label()
        numDw = New NumericUpDown()
        lblDh = New Label()
        numDh = New NumericUpDown()
        btnResetMove = New Button()
        lblMoveHint = New Label()
        grpChildren = New GroupBox()
        tlpChildren = New TableLayoutPanel()
        lstChildren = New ListBox()
        btnHideChild = New Button()
        btnShowChild = New Button()
        btnShowAllChildren = New Button()
        grpKeys = New GroupBox()
        tlpKeys = New TableLayoutPanel()
        btnSendShiftF4 = New Button()
        btnSendF4 = New Button()
        grpUser = New GroupBox()
        tlpUser = New TableLayoutPanel()
        lblHive = New Label()
        cboHive = New ComboBox()
        tlpPrefRows = New TableLayoutPanel()
        lblExpandRhp = New Label()
        cboExpandRhp = New ComboBox()
        lblRhpSticky = New Label()
        cboRhpSticky = New ComboBox()
        lblRhpViewMode = New Label()
        cboRhpViewMode = New ComboBox()
        lblEnableAv2 = New Label()
        cboEnableAv2 = New ComboBox()
        lblPrefHint = New Label()
        gridPrefs = New DataGridView()
        btnApplyUser = New Button()
        btnRestoreUser = New Button()
        chkRestoreOnClose = New CheckBox()
        grpMachine = New GroupBox()
        tlpMachine = New TableLayoutPanel()
        cboProduct = New ComboBox()
        chkSuppressUpsell = New CheckBox()
        chkDisableServices = New CheckBox()
        btnApplyMachine = New Button()
        btnRevertMachine = New Button()
        chkRevertPolicyOnClose = New CheckBox()
        grpCmd = New GroupBox()
        tlpCmd = New TableLayoutPanel()
        txtCmd = New TextBox()
        tlpRight = New TableLayoutPanel()
        pnlHost = New Panel()
        lblStatus = New Label()
        tmrLayout = New Timer(components)
        pnlButtons = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        CType(splitMain, ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        flowOptions.SuspendLayout()
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
        CType(numClipRight, ComponentModel.ISupportInitialize).BeginInit()
        CType(numClipTop, ComponentModel.ISupportInitialize).BeginInit()
        grpMove.SuspendLayout()
        grpHosting.SuspendLayout()
        tlpHosting.SuspendLayout()
        flowDetachMode.SuspendLayout()
        CType(numCaptureDelay, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(numCloseGrace, System.ComponentModel.ISupportInitialize).BeginInit()
        grpActiveX.SuspendLayout()
        tlpActiveX.SuspendLayout()
        flowAcroButtons.SuspendLayout()
        tlpMove.SuspendLayout()
        CType(numDx, ComponentModel.ISupportInitialize).BeginInit()
        CType(numDy, ComponentModel.ISupportInitialize).BeginInit()
        CType(numDw, ComponentModel.ISupportInitialize).BeginInit()
        CType(numDh, ComponentModel.ISupportInitialize).BeginInit()
        grpChildren.SuspendLayout()
        tlpChildren.SuspendLayout()
        grpKeys.SuspendLayout()
        tlpKeys.SuspendLayout()
        grpUser.SuspendLayout()
        tlpUser.SuspendLayout()
        tlpPrefRows.SuspendLayout()
        CType(gridPrefs, ComponentModel.ISupportInitialize).BeginInit()
        grpMachine.SuspendLayout()
        tlpMachine.SuspendLayout()
        grpCmd.SuspendLayout()
        tlpCmd.SuspendLayout()
        tlpRight.SuspendLayout()
        pnlButtons.SuspendLayout()
        SuspendLayout()
        ' 
        ' splitMain
        ' 
        splitMain.Dock = DockStyle.Fill
        splitMain.Location = New Point(0, 0)
        splitMain.Name = "splitMain"
        ' 
        ' splitMain.Panel1
        ' 
        splitMain.Panel1.Controls.Add(flowOptions)
        splitMain.Panel1MinSize = 300
        ' 
        ' splitMain.Panel2
        ' 
        splitMain.Panel2.Controls.Add(tlpRight)
        splitMain.Panel2MinSize = 200
        splitMain.Size = New Size(1240, 727)
        splitMain.SplitterDistance = 470
        splitMain.SplitterWidth = 6
        splitMain.TabIndex = 0
        ' 
        ' flowOptions
        ' 
        flowOptions.AutoScroll = True
        flowOptions.Controls.Add(grpLaunch)
        flowOptions.Controls.Add(grpChrome)
        flowOptions.Controls.Add(grpFile)
        flowOptions.Controls.Add(grpProbe)
        flowOptions.Controls.Add(grpScenario)
        flowOptions.Controls.Add(grpClip)
        flowOptions.Controls.Add(grpMove)
        ' Imediat după «Poziția ferestrei Adobe»: ambele descriu ce se întâmplă cu FEREASTRA
        ' găzduită, iar comparația A/B se face uitându-te la ele împreună.
        flowOptions.Controls.Add(grpHosting)
        flowOptions.Controls.Add(grpChildren)
        flowOptions.Controls.Add(grpKeys)
        flowOptions.Controls.Add(grpUser)
        flowOptions.Controls.Add(grpMachine)
        ' Evaluarea ActiveX stă la sfârșit: e o INVESTIGAȚIE separată (poate înlocui tot mecanismul
        ' de găzduire de ferestre), nu o manetă peste fereastra găzduită acum.
        flowOptions.Controls.Add(grpActiveX)
        flowOptions.Controls.Add(grpCmd)
        flowOptions.Dock = DockStyle.Fill
        flowOptions.FlowDirection = FlowDirection.TopDown
        flowOptions.Location = New Point(0, 0)
        flowOptions.Name = "flowOptions"
        flowOptions.Padding = New Padding(6)
        flowOptions.Size = New Size(470, 727)
        flowOptions.TabIndex = 0
        flowOptions.WrapContents = False
        ' 
        ' grpLaunch
        ' 
        grpLaunch.AutoSize = True
        grpLaunch.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpLaunch.Controls.Add(tlpLaunch)
        grpLaunch.Location = New Point(9, 9)
        grpLaunch.Name = "grpLaunch"
        grpLaunch.Size = New Size(439, 100)
        grpLaunch.TabIndex = 0
        grpLaunch.TabStop = False
        grpLaunch.Text = "Lansare"
        ' 
        ' tlpLaunch
        ' 
        tlpLaunch.AutoSize = True
        tlpLaunch.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpLaunch.ColumnCount = 1
        tlpLaunch.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpLaunch.Controls.Add(chkNewInstance, 0, 0)
        tlpLaunch.Controls.Add(chkNoSplash, 0, 1)
        tlpLaunch.Dock = DockStyle.Fill
        tlpLaunch.Location = New Point(3, 27)
        tlpLaunch.Name = "tlpLaunch"
        tlpLaunch.RowCount = 2
        tlpLaunch.RowStyles.Add(New RowStyle())
        tlpLaunch.RowStyles.Add(New RowStyle())
        tlpLaunch.Size = New Size(433, 70)
        tlpLaunch.TabIndex = 0
        ' 
        ' chkNewInstance
        ' 
        chkNewInstance.AutoSize = True
        chkNewInstance.Checked = True
        chkNewInstance.CheckState = CheckState.Checked
        chkNewInstance.Dock = DockStyle.Fill
        chkNewInstance.Location = New Point(3, 3)
        chkNewInstance.Name = "chkNewInstance"
        chkNewInstance.Size = New Size(427, 29)
        chkNewInstance.TabIndex = 0
        chkNewInstance.Text = "/n  — instanță nouă (recomandat pt. încorporare)"
        ' 
        ' chkNoSplash
        ' 
        chkNoSplash.AutoSize = True
        chkNoSplash.Checked = True
        chkNoSplash.CheckState = CheckState.Checked
        chkNoSplash.Dock = DockStyle.Fill
        chkNoSplash.Location = New Point(3, 38)
        chkNoSplash.Name = "chkNoSplash"
        chkNoSplash.Size = New Size(427, 29)
        chkNoSplash.TabIndex = 1
        chkNoSplash.Text = "/s  — fără ecran de întâmpinare"
        ' 
        ' grpChrome
        ' 
        grpChrome.AutoSize = True
        grpChrome.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpChrome.Controls.Add(tlpChrome)
        grpChrome.Location = New Point(9, 115)
        grpChrome.Name = "grpChrome"
        grpChrome.Size = New Size(423, 240)
        grpChrome.TabIndex = 1
        grpChrome.TabStop = False
        grpChrome.Text = "Chrome ascuns (parametri /A)"
        ' 
        ' tlpChrome
        ' 
        tlpChrome.AutoSize = True
        tlpChrome.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpChrome.ColumnCount = 1
        tlpChrome.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpChrome.Controls.Add(chkToolbar, 0, 0)
        tlpChrome.Controls.Add(chkNavpanes, 0, 1)
        tlpChrome.Controls.Add(chkStatusbar, 0, 2)
        tlpChrome.Controls.Add(chkMessages, 0, 3)
        tlpChrome.Controls.Add(chkScrollbar, 0, 4)
        tlpChrome.Controls.Add(chkPagemodeNone, 0, 5)
        tlpChrome.Dock = DockStyle.Fill
        tlpChrome.Location = New Point(3, 27)
        tlpChrome.Name = "tlpChrome"
        tlpChrome.RowCount = 6
        tlpChrome.RowStyles.Add(New RowStyle())
        tlpChrome.RowStyles.Add(New RowStyle())
        tlpChrome.RowStyles.Add(New RowStyle())
        tlpChrome.RowStyles.Add(New RowStyle())
        tlpChrome.RowStyles.Add(New RowStyle())
        tlpChrome.RowStyles.Add(New RowStyle())
        tlpChrome.Size = New Size(417, 210)
        tlpChrome.TabIndex = 0
        ' 
        ' chkToolbar
        ' 
        chkToolbar.AutoSize = True
        chkToolbar.Checked = True
        chkToolbar.CheckState = CheckState.Checked
        chkToolbar.Dock = DockStyle.Fill
        chkToolbar.Location = New Point(3, 3)
        chkToolbar.Name = "chkToolbar"
        chkToolbar.Size = New Size(411, 29)
        chkToolbar.TabIndex = 0
        chkToolbar.Text = "toolbar=0  — ascunde bara de instrumente"
        ' 
        ' chkNavpanes
        ' 
        chkNavpanes.AutoSize = True
        chkNavpanes.Checked = True
        chkNavpanes.CheckState = CheckState.Checked
        chkNavpanes.Dock = DockStyle.Fill
        chkNavpanes.Location = New Point(3, 38)
        chkNavpanes.Name = "chkNavpanes"
        chkNavpanes.Size = New Size(411, 29)
        chkNavpanes.TabIndex = 1
        chkNavpanes.Text = "navpanes=0  — ascunde panourile de navigare"
        ' 
        ' chkStatusbar
        ' 
        chkStatusbar.AutoSize = True
        chkStatusbar.Checked = True
        chkStatusbar.CheckState = CheckState.Checked
        chkStatusbar.Dock = DockStyle.Fill
        chkStatusbar.Location = New Point(3, 73)
        chkStatusbar.Name = "chkStatusbar"
        chkStatusbar.Size = New Size(411, 29)
        chkStatusbar.TabIndex = 2
        chkStatusbar.Text = "statusbar=0  — ascunde bara de stare"
        ' 
        ' chkMessages
        ' 
        chkMessages.AutoSize = True
        chkMessages.Checked = True
        chkMessages.CheckState = CheckState.Checked
        chkMessages.Dock = DockStyle.Fill
        chkMessages.Location = New Point(3, 108)
        chkMessages.Name = "chkMessages"
        chkMessages.Size = New Size(411, 29)
        chkMessages.TabIndex = 3
        chkMessages.Text = "messages=0  — ascunde bara de mesaje"
        ' 
        ' chkScrollbar
        ' 
        chkScrollbar.AutoSize = True
        chkScrollbar.Checked = True
        chkScrollbar.CheckState = CheckState.Checked
        chkScrollbar.Dock = DockStyle.Fill
        chkScrollbar.Location = New Point(3, 143)
        chkScrollbar.Name = "chkScrollbar"
        chkScrollbar.Size = New Size(411, 29)
        chkScrollbar.TabIndex = 4
        chkScrollbar.Text = "scrollbar=0  — ascunde barele de derulare"
        ' 
        ' chkPagemodeNone
        ' 
        chkPagemodeNone.AutoSize = True
        chkPagemodeNone.Checked = True
        chkPagemodeNone.CheckState = CheckState.Checked
        chkPagemodeNone.Dock = DockStyle.Fill
        chkPagemodeNone.Location = New Point(3, 178)
        chkPagemodeNone.Name = "chkPagemodeNone"
        chkPagemodeNone.Size = New Size(411, 29)
        chkPagemodeNone.TabIndex = 5
        chkPagemodeNone.Text = "pagemode=none  — fără panou lateral deschis"
        ' 
        ' grpFile
        ' 
        grpFile.AutoSize = True
        grpFile.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpFile.Controls.Add(tlpFile)
        grpFile.Location = New Point(9, 361)
        grpFile.Name = "grpFile"
        grpFile.Size = New Size(269, 137)
        grpFile.TabIndex = 2
        grpFile.TabStop = False
        grpFile.Text = "Document"
        ' 
        ' tlpFile
        ' 
        tlpFile.AutoSize = True
        tlpFile.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpFile.ColumnCount = 1
        tlpFile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpFile.Controls.Add(btnBrowse, 0, 0)
        tlpFile.Controls.Add(lblFile, 0, 1)
        tlpFile.Controls.Add(btnRelaunch, 0, 2)
        tlpFile.Dock = DockStyle.Fill
        tlpFile.Location = New Point(3, 27)
        tlpFile.Name = "tlpFile"
        tlpFile.RowCount = 3
        tlpFile.RowStyles.Add(New RowStyle())
        tlpFile.RowStyles.Add(New RowStyle())
        tlpFile.RowStyles.Add(New RowStyle())
        tlpFile.Size = New Size(263, 107)
        tlpFile.TabIndex = 0
        ' 
        ' btnBrowse
        ' 
        btnBrowse.AutoSize = True
        btnBrowse.Dock = DockStyle.Fill
        btnBrowse.Location = New Point(3, 3)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(257, 35)
        btnBrowse.TabIndex = 0
        btnBrowse.Text = "Deschide PDF…"
        btnBrowse.UseVisualStyleBackColor = True
        ' 
        ' lblFile
        ' 
        lblFile.AutoSize = True
        lblFile.Dock = DockStyle.Fill
        lblFile.Location = New Point(3, 41)
        lblFile.Name = "lblFile"
        lblFile.Size = New Size(257, 25)
        lblFile.TabIndex = 1
        lblFile.Text = "<niciun PDF>"
        ' 
        ' btnRelaunch
        ' 
        btnRelaunch.AutoSize = True
        btnRelaunch.Dock = DockStyle.Fill
        btnRelaunch.Location = New Point(3, 69)
        btnRelaunch.Name = "btnRelaunch"
        btnRelaunch.Size = New Size(257, 35)
        btnRelaunch.TabIndex = 2
        btnRelaunch.Text = "Reîncorporează / redesenează"
        btnRelaunch.UseVisualStyleBackColor = True
        ' 
        ' grpProbe
        ' 
        grpProbe.AutoSize = True
        grpProbe.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpProbe.Controls.Add(tlpProbe)
        grpProbe.Location = New Point(9, 504)
        grpProbe.Name = "grpProbe"
        grpProbe.Size = New Size(292, 130)
        grpProbe.TabIndex = 3
        grpProbe.TabStop = False
        grpProbe.Text = "Diagnostic"
        ' 
        ' tlpProbe
        ' 
        tlpProbe.AutoSize = True
        tlpProbe.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpProbe.ColumnCount = 1
        tlpProbe.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpProbe.Controls.Add(btnProbe, 0, 0)
        tlpProbe.Controls.Add(btnMachineState, 0, 1)
        tlpProbe.Dock = DockStyle.Fill
        tlpProbe.Location = New Point(3, 27)
        tlpProbe.Name = "tlpProbe"
        tlpProbe.RowCount = 2
        tlpProbe.RowStyles.Add(New RowStyle())
        tlpProbe.RowStyles.Add(New RowStyle())
        tlpProbe.Size = New Size(286, 100)
        tlpProbe.TabIndex = 0
        ' 
        ' btnProbe
        ' 
        btnProbe.AutoSize = True
        btnProbe.Dock = DockStyle.Fill
        btnProbe.Location = New Point(3, 3)
        btnProbe.Name = "btnProbe"
        btnProbe.Size = New Size(280, 35)
        btnProbe.TabIndex = 0
        btnProbe.Text = "Arborele de ferestre copil"
        btnProbe.UseVisualStyleBackColor = True
        ' 
        ' btnMachineState
        ' 
        btnMachineState.AutoSize = True
        btnMachineState.Dock = DockStyle.Fill
        btnMachineState.Location = New Point(3, 44)
        btnMachineState.Name = "btnMachineState"
        btnMachineState.Size = New Size(280, 53)
        btnMachineState.TabIndex = 1
        btnMachineState.Text = "Starea mașinii (Adobe + registry)"
        btnMachineState.UseVisualStyleBackColor = True
        ' 
        ' grpScenario
        ' 
        grpScenario.AutoSize = True
        grpScenario.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpScenario.Controls.Add(tlpScenario)
        grpScenario.Location = New Point(9, 640)
        grpScenario.Name = "grpScenario"
        grpScenario.Size = New Size(320, 178)
        grpScenario.TabIndex = 4
        grpScenario.TabStop = False
        grpScenario.Text = "Scenariu"
        ' 
        ' tlpScenario
        ' 
        tlpScenario.AutoSize = True
        tlpScenario.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpScenario.ColumnCount = 1
        tlpScenario.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpScenario.Controls.Add(lblScenario, 0, 0)
        tlpScenario.Controls.Add(btnLoadScenario, 0, 1)
        tlpScenario.Controls.Add(btnRunScenario, 0, 2)
        tlpScenario.Controls.Add(btnSaveScenario, 0, 3)
        tlpScenario.Dock = DockStyle.Fill
        tlpScenario.Location = New Point(3, 27)
        tlpScenario.Name = "tlpScenario"
        tlpScenario.RowCount = 4
        tlpScenario.RowStyles.Add(New RowStyle())
        tlpScenario.RowStyles.Add(New RowStyle())
        tlpScenario.RowStyles.Add(New RowStyle())
        tlpScenario.RowStyles.Add(New RowStyle())
        tlpScenario.Size = New Size(314, 148)
        tlpScenario.TabIndex = 0
        ' 
        ' lblScenario
        ' 
        lblScenario.AutoSize = True
        lblScenario.Dock = DockStyle.Fill
        lblScenario.Location = New Point(3, 0)
        lblScenario.Name = "lblScenario"
        lblScenario.Size = New Size(308, 25)
        lblScenario.TabIndex = 0
        lblScenario.Text = "(niciun scenariu)"
        ' 
        ' btnLoadScenario
        ' 
        btnLoadScenario.AutoSize = True
        btnLoadScenario.Dock = DockStyle.Fill
        btnLoadScenario.Location = New Point(3, 28)
        btnLoadScenario.Name = "btnLoadScenario"
        btnLoadScenario.Size = New Size(308, 35)
        btnLoadScenario.TabIndex = 1
        btnLoadScenario.Text = "Încarcă scenariu…"
        btnLoadScenario.UseVisualStyleBackColor = True
        ' 
        ' btnRunScenario
        ' 
        btnRunScenario.AutoSize = True
        btnRunScenario.Dock = DockStyle.Fill
        btnRunScenario.Enabled = False
        btnRunScenario.Location = New Point(3, 69)
        btnRunScenario.Name = "btnRunScenario"
        btnRunScenario.Size = New Size(308, 35)
        btnRunScenario.TabIndex = 2
        btnRunScenario.Text = "Rulează scenariul"
        btnRunScenario.UseVisualStyleBackColor = True
        ' 
        ' btnSaveScenario
        ' 
        btnSaveScenario.AutoSize = True
        btnSaveScenario.Dock = DockStyle.Fill
        btnSaveScenario.Location = New Point(3, 110)
        btnSaveScenario.Name = "btnSaveScenario"
        btnSaveScenario.Size = New Size(308, 35)
        btnSaveScenario.TabIndex = 3
        btnSaveScenario.Text = "Salvează starea curentă ca scenariu…"
        btnSaveScenario.UseVisualStyleBackColor = True
        ' 
        ' grpClip
        ' 
        grpClip.AutoSize = True
        grpClip.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpClip.Controls.Add(tlpClip)
        grpClip.Location = New Point(9, 824)
        grpClip.Name = "grpClip"
        grpClip.Size = New Size(325, 180)
        grpClip.TabIndex = 5
        grpClip.TabStop = False
        grpClip.Text = "Decupare"
        ' 
        ' tlpClip
        ' 
        tlpClip.AutoSize = True
        tlpClip.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpClip.ColumnCount = 2
        tlpClip.ColumnStyles.Add(New ColumnStyle())
        tlpClip.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpClip.Controls.Add(chkClip, 0, 0)
        tlpClip.Controls.Add(lblClipRight, 0, 1)
        tlpClip.Controls.Add(numClipRight, 1, 1)
        tlpClip.Controls.Add(lblClipTop, 0, 2)
        tlpClip.Controls.Add(numClipTop, 1, 2)
        tlpClip.Controls.Add(btnClipAuto, 0, 3)
        tlpClip.Dock = DockStyle.Fill
        tlpClip.Location = New Point(3, 27)
        tlpClip.Name = "tlpClip"
        tlpClip.RowCount = 4
        tlpClip.RowStyles.Add(New RowStyle())
        tlpClip.RowStyles.Add(New RowStyle())
        tlpClip.RowStyles.Add(New RowStyle())
        tlpClip.RowStyles.Add(New RowStyle())
        tlpClip.Size = New Size(319, 150)
        tlpClip.TabIndex = 0
        ' 
        ' chkClip
        ' 
        chkClip.AutoSize = True
        tlpClip.SetColumnSpan(chkClip, 2)
        chkClip.Dock = DockStyle.Fill
        chkClip.Location = New Point(3, 3)
        chkClip.Name = "chkClip"
        chkClip.Size = New Size(313, 29)
        chkClip.TabIndex = 0
        chkClip.Text = "Decupare activă"
        ' 
        ' lblClipRight
        ' 
        lblClipRight.AutoSize = True
        lblClipRight.Dock = DockStyle.Fill
        lblClipRight.Location = New Point(3, 35)
        lblClipRight.Name = "lblClipRight"
        lblClipRight.Size = New Size(187, 37)
        lblClipRight.TabIndex = 1
        lblClipRight.Text = "Decupare dreapta (px)"
        lblClipRight.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' numClipRight
        ' 
        numClipRight.Dock = DockStyle.Fill
        numClipRight.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numClipRight.Location = New Point(196, 38)
        numClipRight.Maximum = New Decimal(New Integer() {800, 0, 0, 0})
        numClipRight.Name = "numClipRight"
        numClipRight.Size = New Size(120, 31)
        numClipRight.TabIndex = 2
        ' 
        ' lblClipTop
        ' 
        lblClipTop.AutoSize = True
        lblClipTop.Dock = DockStyle.Fill
        lblClipTop.Location = New Point(3, 72)
        lblClipTop.Name = "lblClipTop"
        lblClipTop.Size = New Size(187, 37)
        lblClipTop.TabIndex = 3
        lblClipTop.Text = "Decupare sus (px)"
        lblClipTop.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' numClipTop
        ' 
        numClipTop.Dock = DockStyle.Fill
        numClipTop.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numClipTop.Location = New Point(196, 75)
        numClipTop.Maximum = New Decimal(New Integer() {400, 0, 0, 0})
        numClipTop.Name = "numClipTop"
        numClipTop.Size = New Size(120, 31)
        numClipTop.TabIndex = 4
        ' 
        ' btnClipAuto
        ' 
        btnClipAuto.AutoSize = True
        tlpClip.SetColumnSpan(btnClipAuto, 2)
        btnClipAuto.Dock = DockStyle.Fill
        btnClipAuto.Enabled = False
        btnClipAuto.Location = New Point(3, 112)
        btnClipAuto.Name = "btnClipAuto"
        btnClipAuto.Size = New Size(313, 35)
        btnClipAuto.TabIndex = 5
        btnClipAuto.Text = "Măsoară din probă"
        btnClipAuto.UseVisualStyleBackColor = True
        ' 
        ' grpMove
        ' 
        grpMove.AutoSize = True
        grpMove.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpMove.Controls.Add(tlpMove)
        grpMove.Location = New Point(9, 1010)
        grpMove.Name = "grpMove"
        grpMove.Size = New Size(1600, 244)
        grpMove.TabIndex = 6
        grpMove.TabStop = False
        grpMove.Text = "Poziția ferestrei Adobe"
        '
        ' grpHosting — cum se PRINDE fereastra și cum se DĂ DRUMUL la ea (felia 0024-03).
        ' Cele două defecte reparate în felia asta se compară doar cu numere: timpul de la lansare la
        ' încorporare, pentru fiecare mod de închidere.
        '
        grpHosting.AutoSize = True
        grpHosting.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpHosting.Controls.Add(tlpHosting)
        grpHosting.Name = "grpHosting"
        grpHosting.Size = New Size(1600, 260)
        grpHosting.TabIndex = 7
        grpHosting.TabStop = False
        grpHosting.Text = "Închidere / captură"
        '
        ' tlpHosting
        '
        tlpHosting.AutoSize = True
        tlpHosting.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpHosting.ColumnCount = 2
        tlpHosting.ColumnStyles.Add(New ColumnStyle())
        tlpHosting.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpHosting.Controls.Add(lblDetachMode, 0, 0)
        tlpHosting.Controls.Add(flowDetachMode, 1, 0)
        tlpHosting.Controls.Add(lblCaptureDelay, 0, 1)
        tlpHosting.Controls.Add(numCaptureDelay, 1, 1)
        tlpHosting.Controls.Add(lblCloseGrace, 0, 2)
        tlpHosting.Controls.Add(numCloseGrace, 1, 2)
        tlpHosting.Controls.Add(chkCreationHook, 0, 3)
        tlpHosting.SetColumnSpan(chkCreationHook, 2)
        tlpHosting.Controls.Add(chkForceClassicUi, 0, 4)
        tlpHosting.SetColumnSpan(chkForceClassicUi, 2)
        tlpHosting.Controls.Add(lblEmbedTiming, 0, 5)
        tlpHosting.SetColumnSpan(lblEmbedTiming, 2)
        tlpHosting.Dock = DockStyle.Fill
        tlpHosting.Location = New Point(3, 27)
        tlpHosting.Name = "tlpHosting"
        tlpHosting.RowCount = 6
        tlpHosting.RowStyles.Add(New RowStyle())
        tlpHosting.RowStyles.Add(New RowStyle())
        tlpHosting.RowStyles.Add(New RowStyle())
        tlpHosting.RowStyles.Add(New RowStyle())
        tlpHosting.RowStyles.Add(New RowStyle())
        tlpHosting.RowStyles.Add(New RowStyle())
        tlpHosting.Size = New Size(1594, 230)
        tlpHosting.TabIndex = 0
        '
        ' lblDetachMode
        '
        lblDetachMode.AutoSize = True
        lblDetachMode.Dock = DockStyle.Fill
        lblDetachMode.Name = "lblDetachMode"
        lblDetachMode.Size = New Size(190, 37)
        lblDetachMode.TabIndex = 0
        lblDetachMode.Text = "Mod de închidere"
        lblDetachMode.TextAlign = ContentAlignment.MiddleLeft
        '
        ' flowDetachMode
        '
        flowDetachMode.AutoSize = True
        flowDetachMode.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flowDetachMode.Controls.Add(rdoDetachKill)
        flowDetachMode.Controls.Add(rdoDetachClose)
        flowDetachMode.Dock = DockStyle.Fill
        flowDetachMode.Margin = New Padding(0)
        flowDetachMode.Name = "flowDetachMode"
        flowDetachMode.TabIndex = 1
        '
        ' rdoDetachKill
        '
        rdoDetachKill.AutoSize = True
        rdoDetachKill.Checked = True
        rdoDetachKill.Name = "rdoDetachKill"
        rdoDetachKill.TabIndex = 0
        rdoDetachKill.TabStop = True
        rdoDetachKill.Text = "Omoară procesul (A)"
        rdoDetachKill.UseVisualStyleBackColor = True
        '
        ' rdoDetachClose
        '
        rdoDetachClose.AutoSize = True
        rdoDetachClose.Name = "rdoDetachClose"
        rdoDetachClose.TabIndex = 1
        rdoDetachClose.Text = "Închide fereastra (B)"
        rdoDetachClose.UseVisualStyleBackColor = True
        '
        ' lblCaptureDelay
        '
        lblCaptureDelay.AutoSize = True
        lblCaptureDelay.Dock = DockStyle.Fill
        lblCaptureDelay.Name = "lblCaptureDelay"
        lblCaptureDelay.Size = New Size(190, 37)
        lblCaptureDelay.TabIndex = 2
        lblCaptureDelay.Text = "Întârziere captură (ms)"
        lblCaptureDelay.TextAlign = ContentAlignment.MiddleLeft
        '
        ' numCaptureDelay
        '
        numCaptureDelay.Dock = DockStyle.Fill
        numCaptureDelay.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        numCaptureDelay.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numCaptureDelay.Name = "numCaptureDelay"
        numCaptureDelay.Size = New Size(1400, 31)
        numCaptureDelay.TabIndex = 3
        '
        ' lblCloseGrace
        '
        lblCloseGrace.AutoSize = True
        lblCloseGrace.Dock = DockStyle.Fill
        lblCloseGrace.Name = "lblCloseGrace"
        lblCloseGrace.Size = New Size(190, 37)
        lblCloseGrace.TabIndex = 4
        lblCloseGrace.Text = "Răgaz închidere B (ms)"
        lblCloseGrace.TextAlign = ContentAlignment.MiddleLeft
        '
        ' numCloseGrace
        '
        numCloseGrace.Dock = DockStyle.Fill
        numCloseGrace.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        numCloseGrace.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        numCloseGrace.Name = "numCloseGrace"
        numCloseGrace.Size = New Size(1400, 31)
        numCloseGrace.TabIndex = 5
        numCloseGrace.Value = New Decimal(New Integer() {1500, 0, 0, 0})
        '
        ' chkCreationHook
        '
        chkCreationHook.AutoSize = True
        chkCreationHook.Name = "chkCreationHook"
        chkCreationHook.TabIndex = 6
        chkCreationHook.Text = "Folosește cârligul de creare (ascunde fereastra la apariție, nu la următoarea sondare)"
        chkCreationHook.UseVisualStyleBackColor = True
        '
        ' chkForceClassicUi
        '
        chkForceClassicUi.AutoSize = True
        chkForceClassicUi.Name = "chkForceClassicUi"
        chkForceClassicUi.TabIndex = 7
        chkForceClassicUi.Text = "Aplică bEnableAv2 = 0 înainte de fiecare lansare"
        chkForceClassicUi.UseVisualStyleBackColor = True
        '
        ' lblEmbedTiming — NUMĂRUL cu care se compară A și B.
        '
        lblEmbedTiming.AutoSize = True
        lblEmbedTiming.Name = "lblEmbedTiming"
        lblEmbedTiming.TabIndex = 8
        lblEmbedTiming.Text = "Timp lansare → încorporare: —"
        '
        ' grpActiveX — evaluarea controlului AcroPDF (doar banc, felia 0024-03).
        '
        grpActiveX.AutoSize = True
        grpActiveX.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpActiveX.Controls.Add(tlpActiveX)
        grpActiveX.Name = "grpActiveX"
        grpActiveX.Size = New Size(1600, 420)
        grpActiveX.TabIndex = 12
        grpActiveX.TabStop = False
        grpActiveX.Text = "ActiveX (AcroPDF)"
        '
        ' tlpActiveX
        '
        tlpActiveX.AutoSize = True
        tlpActiveX.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpActiveX.ColumnCount = 1
        tlpActiveX.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpActiveX.Controls.Add(lblAcroStatus, 0, 0)
        tlpActiveX.Controls.Add(flowAcroButtons, 0, 1)
        tlpActiveX.Controls.Add(pnlAcroHost, 0, 2)
        tlpActiveX.Dock = DockStyle.Fill
        tlpActiveX.Location = New Point(3, 27)
        tlpActiveX.Name = "tlpActiveX"
        tlpActiveX.RowCount = 3
        tlpActiveX.RowStyles.Add(New RowStyle())
        tlpActiveX.RowStyles.Add(New RowStyle())
        tlpActiveX.RowStyles.Add(New RowStyle())
        tlpActiveX.Size = New Size(1594, 390)
        tlpActiveX.TabIndex = 0
        '
        ' lblAcroStatus
        '
        lblAcroStatus.AutoSize = True
        lblAcroStatus.Dock = DockStyle.Fill
        lblAcroStatus.Name = "lblAcroStatus"
        lblAcroStatus.TabIndex = 0
        lblAcroStatus.Text = "Se verifică dacă AcroPDF e înregistrat…"
        '
        ' flowAcroButtons
        '
        flowAcroButtons.AutoSize = True
        flowAcroButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flowAcroButtons.Controls.Add(btnAcroLoad)
        flowAcroButtons.Controls.Add(btnAcroSecond)
        flowAcroButtons.Controls.Add(btnAcroClear)
        flowAcroButtons.Controls.Add(btnAcroHideChrome)
        flowAcroButtons.Controls.Add(btnAcroProbe)
        flowAcroButtons.Controls.Add(btnAcroPrefs)
        flowAcroButtons.Controls.Add(chkAcroChrome)
        flowAcroButtons.Dock = DockStyle.Fill
        flowAcroButtons.Margin = New Padding(0)
        flowAcroButtons.Name = "flowAcroButtons"
        flowAcroButtons.TabIndex = 1
        '
        ' btnAcroLoad
        '
        btnAcroLoad.AutoSize = True
        btnAcroLoad.Name = "btnAcroLoad"
        btnAcroLoad.TabIndex = 0
        btnAcroLoad.Text = "Încarcă în ActiveX"
        btnAcroLoad.UseVisualStyleBackColor = True
        '
        ' btnAcroSecond — al DOILEA document în ACELAȘI control: exact comparația care contează.
        '
        btnAcroSecond.AutoSize = True
        btnAcroSecond.Name = "btnAcroSecond"
        btnAcroSecond.TabIndex = 1
        btnAcroSecond.Text = "Încarcă alt document…"
        btnAcroSecond.UseVisualStyleBackColor = True
        '
        ' btnAcroClear
        '
        btnAcroClear.AutoSize = True
        btnAcroClear.Name = "btnAcroClear"
        btnAcroClear.TabIndex = 2
        btnAcroClear.Text = "Golește"
        btnAcroClear.UseVisualStyleBackColor = True
        '
        ' btnAcroHideChrome — MĂSURAT pe 05.08.2026, nu propus. Starea „perfectă" a operatorului
        ' (nimic vizibil până la mișcarea mouse-ului, când apare bara plutitoare) NU e un mod al lui
        ' Adobe: e exact situația în care trei ferestre copil sunt invizibile, iar vederea
        ' documentului se întinde atunci pe tot panoul (x=0, lățime completă, în loc de x=67).
        ' Deci se reproduce cu ShowWindow — aceeași pârghie ca «hideChildren» din felia 0023.
        '
        btnAcroHideChrome.AutoSize = True
        btnAcroHideChrome.Name = "btnAcroHideChrome"
        btnAcroHideChrome.TabIndex = 3
        btnAcroHideChrome.Text = "Ascunde chrome-ul (după text)"
        btnAcroHideChrome.UseVisualStyleBackColor = True
        '
        ' btnAcroProbe — arborele de ferestre DIN INTERIORUL controlului ActiveX, la cerere.
        ' Panourile lui Adobe apar/dispar/plutesc în funcție de starea dinainte; asta e STRUCTURĂ DE
        ' FERESTRE, deci se citește cu aceeași sondă ca fereastra găzduită, nu se ghicește.
        '
        btnAcroProbe.AutoSize = True
        btnAcroProbe.Name = "btnAcroProbe"
        btnAcroProbe.TabIndex = 3
        btnAcroProbe.Text = "Sondează ActiveX"
        btnAcroProbe.UseVisualStyleBackColor = True
        '
        ' btnAcroPrefs — dump COMPLET al cheii AVGeneral, fără să numească nimic.
        ' Starea panourilor persistă între documente, deci Adobe o scrie undeva; enumerăm toată
        ' cheia înainte și după o acțiune, iar valoarea care s-a schimbat se numește SINGURĂ.
        '
        btnAcroPrefs.AutoSize = True
        btnAcroPrefs.Name = "btnAcroPrefs"
        btnAcroPrefs.TabIndex = 4
        btnAcroPrefs.Text = "Instantaneu AVGeneral"
        btnAcroPrefs.UseVisualStyleBackColor = True
        '
        ' chkAcroChrome — API-ul DOCUMENTAT al controlului pentru ascunderea barelor. Bifat implicit:
        ' asta e întrebarea deschisă acum, iar ascunderea barelor e exact problema pe care felia 0023
        ' a atacat-o cinci pași cu decupare, ascundere de copii, registry și taste.
        '
        chkAcroChrome.AutoSize = True
        chkAcroChrome.Checked = True
        chkAcroChrome.CheckState = CheckState.Checked
        chkAcroChrome.Margin = New Padding(18, 9, 3, 3)
        chkAcroChrome.Name = "chkAcroChrome"
        chkAcroChrome.TabIndex = 3
        chkAcroChrome.Text = "Ascunde barele (setShowToolbar / setPageMode / setShowScrollbars)"
        chkAcroChrome.UseVisualStyleBackColor = True
        '
        ' pnlAcroHost — aici se creează controlul AxHost la rulare (nu se poate în Designer fără
        ' interop generat, iar felia asta refuză explicit aximp/referințe COM).
        '
        pnlAcroHost.BorderStyle = BorderStyle.FixedSingle
        pnlAcroHost.Dock = DockStyle.Fill
        pnlAcroHost.Name = "pnlAcroHost"
        ' 300 era prea puțin: sondele din 05.08.2026 arată panoul la 1899x298, adică Adobe își
        ' aranjează panourile într-o fâșie în care documentul aproape că nu încape. Nu era cauza
        ' comportamentului raportat, dar făcea imposibil de judecat pe ecran ce s-a întâmplat.
        pnlAcroHost.Size = New Size(1588, 700)
        pnlAcroHost.MinimumSize = New Size(0, 700)
        pnlAcroHost.TabIndex = 2
        '
        ' tlpMove
        '
        tlpMove.AutoSize = True
        tlpMove.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpMove.ColumnCount = 2
        tlpMove.ColumnStyles.Add(New ColumnStyle())
        tlpMove.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpMove.Controls.Add(lblDx, 0, 0)
        tlpMove.Controls.Add(numDx, 1, 0)
        tlpMove.Controls.Add(lblDy, 0, 1)
        tlpMove.Controls.Add(numDy, 1, 1)
        tlpMove.Controls.Add(lblDw, 0, 2)
        tlpMove.Controls.Add(numDw, 1, 2)
        tlpMove.Controls.Add(lblDh, 0, 3)
        tlpMove.Controls.Add(numDh, 1, 3)
        tlpMove.Controls.Add(btnResetMove, 0, 4)
        tlpMove.Controls.Add(lblMoveHint, 0, 5)
        tlpMove.Dock = DockStyle.Fill
        tlpMove.Location = New Point(3, 27)
        tlpMove.Name = "tlpMove"
        tlpMove.RowCount = 6
        tlpMove.RowStyles.Add(New RowStyle())
        tlpMove.RowStyles.Add(New RowStyle())
        tlpMove.RowStyles.Add(New RowStyle())
        tlpMove.RowStyles.Add(New RowStyle())
        tlpMove.RowStyles.Add(New RowStyle())
        tlpMove.RowStyles.Add(New RowStyle())
        tlpMove.Size = New Size(1594, 214)
        tlpMove.TabIndex = 0
        ' 
        ' lblDx
        ' 
        lblDx.AutoSize = True
        lblDx.Dock = DockStyle.Fill
        lblDx.Location = New Point(3, 0)
        lblDx.Name = "lblDx"
        lblDx.Size = New Size(116, 37)
        lblDx.TabIndex = 0
        lblDx.Text = "dx (stânga −)"
        lblDx.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' numDx
        ' 
        numDx.Dock = DockStyle.Fill
        numDx.Location = New Point(125, 3)
        numDx.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDx.Minimum = New Decimal(New Integer() {2000, 0, 0, Integer.MinValue})
        numDx.Name = "numDx"
        numDx.Size = New Size(1466, 31)
        numDx.TabIndex = 1
        ' 
        ' lblDy
        ' 
        lblDy.AutoSize = True
        lblDy.Dock = DockStyle.Fill
        lblDy.Location = New Point(3, 37)
        lblDy.Name = "lblDy"
        lblDy.Size = New Size(116, 37)
        lblDy.TabIndex = 2
        lblDy.Text = "dy (sus −)"
        lblDy.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' numDy
        ' 
        numDy.Dock = DockStyle.Fill
        numDy.Location = New Point(125, 40)
        numDy.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDy.Minimum = New Decimal(New Integer() {2000, 0, 0, Integer.MinValue})
        numDy.Name = "numDy"
        numDy.Size = New Size(1466, 31)
        numDy.TabIndex = 3
        ' 
        ' lblDw
        ' 
        lblDw.AutoSize = True
        lblDw.Dock = DockStyle.Fill
        lblDw.Location = New Point(3, 74)
        lblDw.Name = "lblDw"
        lblDw.Size = New Size(116, 37)
        lblDw.TabIndex = 4
        lblDw.Text = "dw (lățime)"
        lblDw.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' numDw
        ' 
        numDw.Dock = DockStyle.Fill
        numDw.Location = New Point(125, 77)
        numDw.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDw.Minimum = New Decimal(New Integer() {2000, 0, 0, Integer.MinValue})
        numDw.Name = "numDw"
        numDw.Size = New Size(1466, 31)
        numDw.TabIndex = 5
        ' 
        ' lblDh
        ' 
        lblDh.AutoSize = True
        lblDh.Dock = DockStyle.Fill
        lblDh.Location = New Point(3, 111)
        lblDh.Name = "lblDh"
        lblDh.Size = New Size(116, 37)
        lblDh.TabIndex = 6
        lblDh.Text = "dh (înălțime)"
        lblDh.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' numDh
        ' 
        numDh.Dock = DockStyle.Fill
        numDh.Location = New Point(125, 114)
        numDh.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        numDh.Minimum = New Decimal(New Integer() {2000, 0, 0, Integer.MinValue})
        numDh.Name = "numDh"
        numDh.Size = New Size(1466, 31)
        numDh.TabIndex = 7
        ' 
        ' btnResetMove
        ' 
        btnResetMove.AutoSize = True
        tlpMove.SetColumnSpan(btnResetMove, 2)
        btnResetMove.Dock = DockStyle.Fill
        btnResetMove.Location = New Point(3, 151)
        btnResetMove.Name = "btnResetMove"
        btnResetMove.Size = New Size(1588, 35)
        btnResetMove.TabIndex = 8
        btnResetMove.Text = "Readu la zero"
        btnResetMove.UseVisualStyleBackColor = True
        ' 
        ' lblMoveHint
        ' 
        lblMoveHint.AutoSize = True
        tlpMove.SetColumnSpan(lblMoveHint, 2)
        lblMoveHint.Dock = DockStyle.Fill
        lblMoveHint.Location = New Point(3, 189)
        lblMoveHint.Name = "lblMoveHint"
        lblMoveHint.Size = New Size(1588, 25)
        lblMoveHint.TabIndex = 9
        lblMoveHint.Text = "Deplasează ȘI redimensionează FEREASTRA ADOBE în panou, exact ca decuparea dreapta/sus (se compun). dx/dy negative o trag spre stânga/sus, deci banda din marginea aceea iese din zona vizibilă."
        ' 
        ' grpChildren
        ' 
        grpChildren.AutoSize = True
        grpChildren.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpChildren.Controls.Add(tlpChildren)
        grpChildren.Location = New Point(9, 1260)
        grpChildren.Name = "grpChildren"
        grpChildren.Size = New Size(248, 279)
        grpChildren.TabIndex = 7
        grpChildren.TabStop = False
        grpChildren.Text = "Ferestre copil"
        ' 
        ' tlpChildren
        ' 
        tlpChildren.AutoSize = True
        tlpChildren.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpChildren.ColumnCount = 1
        tlpChildren.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpChildren.Controls.Add(lstChildren, 0, 0)
        tlpChildren.Controls.Add(btnHideChild, 0, 1)
        tlpChildren.Controls.Add(btnShowChild, 0, 2)
        tlpChildren.Controls.Add(btnShowAllChildren, 0, 3)
        tlpChildren.Dock = DockStyle.Fill
        tlpChildren.Location = New Point(3, 27)
        tlpChildren.Name = "tlpChildren"
        tlpChildren.RowCount = 4
        tlpChildren.RowStyles.Add(New RowStyle())
        tlpChildren.RowStyles.Add(New RowStyle())
        tlpChildren.RowStyles.Add(New RowStyle())
        tlpChildren.RowStyles.Add(New RowStyle())
        tlpChildren.Size = New Size(242, 249)
        tlpChildren.TabIndex = 0
        ' 
        ' lstChildren
        ' 
        lstChildren.Dock = DockStyle.Fill
        lstChildren.IntegralHeight = False
        lstChildren.ItemHeight = 25
        lstChildren.Location = New Point(3, 3)
        lstChildren.MinimumSize = New Size(0, 120)
        lstChildren.Name = "lstChildren"
        lstChildren.Size = New Size(236, 120)
        lstChildren.TabIndex = 0
        ' 
        ' btnHideChild
        ' 
        btnHideChild.AutoSize = True
        btnHideChild.Dock = DockStyle.Fill
        btnHideChild.Location = New Point(3, 129)
        btnHideChild.Name = "btnHideChild"
        btnHideChild.Size = New Size(236, 35)
        btnHideChild.TabIndex = 1
        btnHideChild.Text = "Ascunde fereastra selectată"
        btnHideChild.UseVisualStyleBackColor = True
        ' 
        ' btnShowChild
        ' 
        btnShowChild.AutoSize = True
        btnShowChild.Dock = DockStyle.Fill
        btnShowChild.Location = New Point(3, 170)
        btnShowChild.Name = "btnShowChild"
        btnShowChild.Size = New Size(236, 35)
        btnShowChild.TabIndex = 2
        btnShowChild.Text = "Arată fereastra selectată"
        btnShowChild.UseVisualStyleBackColor = True
        ' 
        ' btnShowAllChildren
        ' 
        btnShowAllChildren.AutoSize = True
        btnShowAllChildren.Dock = DockStyle.Fill
        btnShowAllChildren.Location = New Point(3, 211)
        btnShowAllChildren.Name = "btnShowAllChildren"
        btnShowAllChildren.Size = New Size(236, 35)
        btnShowAllChildren.TabIndex = 3
        btnShowAllChildren.Text = "Restaurează toate"
        btnShowAllChildren.UseVisualStyleBackColor = True
        ' 
        ' grpKeys
        ' 
        grpKeys.AutoSize = True
        grpKeys.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpKeys.Controls.Add(tlpKeys)
        grpKeys.Location = New Point(9, 1545)
        grpKeys.Name = "grpKeys"
        grpKeys.Size = New Size(417, 130)
        grpKeys.TabIndex = 8
        grpKeys.TabStop = False
        grpKeys.Text = "Scurtături (experimental)"
        ' 
        ' tlpKeys
        ' 
        tlpKeys.AutoSize = True
        tlpKeys.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpKeys.ColumnCount = 1
        tlpKeys.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpKeys.Controls.Add(btnSendShiftF4, 0, 0)
        tlpKeys.Controls.Add(btnSendF4, 0, 1)
        tlpKeys.Dock = DockStyle.Fill
        tlpKeys.Location = New Point(3, 27)
        tlpKeys.Name = "tlpKeys"
        tlpKeys.RowCount = 2
        tlpKeys.RowStyles.Add(New RowStyle())
        tlpKeys.RowStyles.Add(New RowStyle())
        tlpKeys.Size = New Size(411, 100)
        tlpKeys.TabIndex = 0
        ' 
        ' btnSendShiftF4
        ' 
        btnSendShiftF4.AutoSize = True
        btnSendShiftF4.Dock = DockStyle.Fill
        btnSendShiftF4.Location = New Point(3, 3)
        btnSendShiftF4.Name = "btnSendShiftF4"
        btnSendShiftF4.Size = New Size(405, 35)
        btnSendShiftF4.TabIndex = 0
        btnSendShiftF4.Text = "Trimite Shift+F4 (comută panoul de instrumente)"
        btnSendShiftF4.UseVisualStyleBackColor = True
        ' 
        ' btnSendF4
        ' 
        btnSendF4.AutoSize = True
        btnSendF4.Dock = DockStyle.Fill
        btnSendF4.Location = New Point(3, 44)
        btnSendF4.Name = "btnSendF4"
        btnSendF4.Size = New Size(405, 53)
        btnSendF4.TabIndex = 1
        btnSendF4.Text = "Trimite F4 (comută panoul de navigare)"
        btnSendF4.UseVisualStyleBackColor = True
        ' 
        ' grpUser
        ' 
        grpUser.AutoSize = True
        grpUser.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpUser.Controls.Add(tlpUser)
        grpUser.Location = New Point(9, 1681)
        grpUser.Name = "grpUser"
        grpUser.Size = New Size(922, 554)
        grpUser.TabIndex = 9
        grpUser.TabStop = False
        grpUser.Text = "Preferințe Adobe (utilizator, HKCU)"
        ' 
        ' tlpUser
        ' 
        tlpUser.AutoSize = True
        tlpUser.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpUser.ColumnCount = 1
        tlpUser.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpUser.Controls.Add(lblHive, 0, 0)
        tlpUser.Controls.Add(cboHive, 0, 1)
        tlpUser.Controls.Add(tlpPrefRows, 0, 2)
        tlpUser.Controls.Add(lblPrefHint, 0, 3)
        tlpUser.Controls.Add(gridPrefs, 0, 4)
        tlpUser.Controls.Add(btnApplyUser, 0, 5)
        tlpUser.Controls.Add(btnRestoreUser, 0, 6)
        tlpUser.Controls.Add(chkRestoreOnClose, 0, 7)
        tlpUser.Dock = DockStyle.Fill
        tlpUser.Location = New Point(3, 27)
        tlpUser.Name = "tlpUser"
        tlpUser.RowCount = 8
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.RowStyles.Add(New RowStyle())
        tlpUser.Size = New Size(916, 524)
        tlpUser.TabIndex = 0
        ' 
        ' lblHive
        ' 
        lblHive.AutoSize = True
        lblHive.Dock = DockStyle.Fill
        lblHive.Location = New Point(3, 0)
        lblHive.Name = "lblHive"
        lblHive.Size = New Size(910, 25)
        lblHive.TabIndex = 0
        ' 
        ' cboHive
        ' 
        cboHive.Dock = DockStyle.Fill
        cboHive.DropDownStyle = ComboBoxStyle.DropDownList
        cboHive.DropDownWidth = 420
        cboHive.Location = New Point(3, 28)
        cboHive.Name = "cboHive"
        cboHive.Size = New Size(910, 33)
        cboHive.TabIndex = 1
        ' 
        ' tlpPrefRows
        ' 
        tlpPrefRows.AutoSize = True
        tlpPrefRows.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpPrefRows.ColumnCount = 2
        tlpPrefRows.ColumnStyles.Add(New ColumnStyle())
        tlpPrefRows.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpPrefRows.Controls.Add(lblExpandRhp, 0, 0)
        tlpPrefRows.Controls.Add(cboExpandRhp, 1, 0)
        tlpPrefRows.Controls.Add(lblRhpSticky, 0, 1)
        tlpPrefRows.Controls.Add(cboRhpSticky, 1, 1)
        tlpPrefRows.Controls.Add(lblRhpViewMode, 0, 2)
        tlpPrefRows.Controls.Add(cboRhpViewMode, 1, 2)
        tlpPrefRows.Controls.Add(lblEnableAv2, 0, 3)
        tlpPrefRows.Controls.Add(cboEnableAv2, 1, 3)
        tlpPrefRows.Dock = DockStyle.Fill
        tlpPrefRows.Location = New Point(3, 67)
        tlpPrefRows.Name = "tlpPrefRows"
        tlpPrefRows.RowCount = 4
        tlpPrefRows.RowStyles.Add(New RowStyle())
        tlpPrefRows.RowStyles.Add(New RowStyle())
        tlpPrefRows.RowStyles.Add(New RowStyle())
        tlpPrefRows.RowStyles.Add(New RowStyle())
        tlpPrefRows.Size = New Size(910, 156)
        tlpPrefRows.TabIndex = 2
        ' 
        ' lblExpandRhp
        ' 
        lblExpandRhp.AutoSize = True
        lblExpandRhp.Dock = DockStyle.Fill
        lblExpandRhp.Location = New Point(3, 0)
        lblExpandRhp.Name = "lblExpandRhp"
        lblExpandRhp.Size = New Size(298, 39)
        lblExpandRhp.TabIndex = 0
        lblExpandRhp.Text = "bExpandRHPInViewer"
        lblExpandRhp.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboExpandRhp
        ' 
        cboExpandRhp.Dock = DockStyle.Fill
        cboExpandRhp.Location = New Point(307, 3)
        cboExpandRhp.Name = "cboExpandRhp"
        cboExpandRhp.Size = New Size(600, 33)
        cboExpandRhp.TabIndex = 1
        ' 
        ' lblRhpSticky
        ' 
        lblRhpSticky.AutoSize = True
        lblRhpSticky.Dock = DockStyle.Fill
        lblRhpSticky.Location = New Point(3, 39)
        lblRhpSticky.Name = "lblRhpSticky"
        lblRhpSticky.Size = New Size(298, 39)
        lblRhpSticky.TabIndex = 2
        lblRhpSticky.Text = "bRHPSticky"
        lblRhpSticky.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboRhpSticky
        ' 
        cboRhpSticky.Dock = DockStyle.Fill
        cboRhpSticky.Location = New Point(307, 42)
        cboRhpSticky.Name = "cboRhpSticky"
        cboRhpSticky.Size = New Size(600, 33)
        cboRhpSticky.TabIndex = 3
        ' 
        ' lblRhpViewMode
        ' 
        lblRhpViewMode.AutoSize = True
        lblRhpViewMode.Dock = DockStyle.Fill
        lblRhpViewMode.Location = New Point(3, 78)
        lblRhpViewMode.Name = "lblRhpViewMode"
        lblRhpViewMode.Size = New Size(298, 39)
        lblRhpViewMode.TabIndex = 4
        lblRhpViewMode.Text = "aDefaultRHPViewMode_L"
        lblRhpViewMode.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboRhpViewMode
        ' 
        cboRhpViewMode.Dock = DockStyle.Fill
        cboRhpViewMode.Location = New Point(307, 81)
        cboRhpViewMode.Name = "cboRhpViewMode"
        cboRhpViewMode.Size = New Size(600, 33)
        cboRhpViewMode.TabIndex = 5
        ' 
        ' lblEnableAv2
        ' 
        lblEnableAv2.AutoSize = True
        lblEnableAv2.Dock = DockStyle.Fill
        lblEnableAv2.Location = New Point(3, 117)
        lblEnableAv2.Name = "lblEnableAv2"
        lblEnableAv2.Size = New Size(298, 39)
        lblEnableAv2.TabIndex = 6
        lblEnableAv2.Text = "bEnableAv2 (0 = clasic, 1 = modern)"
        lblEnableAv2.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboEnableAv2
        ' 
        cboEnableAv2.Dock = DockStyle.Fill
        cboEnableAv2.Location = New Point(307, 120)
        cboEnableAv2.Name = "cboEnableAv2"
        cboEnableAv2.Size = New Size(600, 33)
        cboEnableAv2.TabIndex = 7
        ' 
        ' lblPrefHint
        ' 
        lblPrefHint.AutoSize = True
        lblPrefHint.Dock = DockStyle.Fill
        lblPrefHint.Location = New Point(3, 226)
        lblPrefHint.Name = "lblPrefHint"
        lblPrefHint.Size = New Size(910, 25)
        lblPrefHint.TabIndex = 3
        lblPrefHint.Text = "«nu atinge» lasă valoarea exact cum e (NU o scrie pe 0); «șterge» o elimină din registry; orice altceva se scrie literal."
        ' 
        ' gridPrefs
        ' 
        gridPrefs.AllowUserToAddRows = False
        gridPrefs.AllowUserToDeleteRows = False
        gridPrefs.AllowUserToResizeRows = False
        gridPrefs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        gridPrefs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        gridPrefs.Dock = DockStyle.Fill
        gridPrefs.EditMode = DataGridViewEditMode.EditProgrammatically
        gridPrefs.Location = New Point(3, 254)
        gridPrefs.MinimumSize = New Size(0, 110)
        gridPrefs.MultiSelect = False
        gridPrefs.Name = "gridPrefs"
        gridPrefs.ReadOnly = True
        gridPrefs.RowHeadersVisible = False
        gridPrefs.RowHeadersWidth = 62
        gridPrefs.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        gridPrefs.Size = New Size(910, 150)
        gridPrefs.TabIndex = 6
        ' 
        ' btnApplyUser
        ' 
        btnApplyUser.AutoSize = True
        btnApplyUser.Dock = DockStyle.Fill
        btnApplyUser.Location = New Point(3, 410)
        btnApplyUser.Name = "btnApplyUser"
        btnApplyUser.Size = New Size(910, 35)
        btnApplyUser.TabIndex = 6
        btnApplyUser.Text = "Aplică și repornește Adobe"
        btnApplyUser.UseVisualStyleBackColor = True
        ' 
        ' btnRestoreUser
        ' 
        btnRestoreUser.AutoSize = True
        btnRestoreUser.Dock = DockStyle.Fill
        btnRestoreUser.Location = New Point(3, 451)
        btnRestoreUser.Name = "btnRestoreUser"
        btnRestoreUser.Size = New Size(910, 35)
        btnRestoreUser.TabIndex = 7
        btnRestoreUser.Text = "Restaurează valorile originale"
        btnRestoreUser.UseVisualStyleBackColor = True
        ' 
        ' chkRestoreOnClose
        ' 
        chkRestoreOnClose.AutoSize = True
        chkRestoreOnClose.Dock = DockStyle.Fill
        chkRestoreOnClose.Location = New Point(3, 492)
        chkRestoreOnClose.Name = "chkRestoreOnClose"
        chkRestoreOnClose.Size = New Size(910, 29)
        chkRestoreOnClose.TabIndex = 8
        chkRestoreOnClose.Text = "Restaurează valorile HKCU la închiderea bancului"
        ' 
        ' grpMachine
        ' 
        grpMachine.AutoSize = True
        grpMachine.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpMachine.Controls.Add(tlpMachine)
        grpMachine.Location = New Point(9, 2241)
        grpMachine.Name = "grpMachine"
        grpMachine.Size = New Size(424, 241)
        grpMachine.TabIndex = 10
        grpMachine.TabStop = False
        grpMachine.Text = "Politici Adobe (mașină, HKLM)"
        ' 
        ' tlpMachine
        ' 
        tlpMachine.AutoSize = True
        tlpMachine.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpMachine.ColumnCount = 1
        tlpMachine.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpMachine.Controls.Add(cboProduct, 0, 0)
        tlpMachine.Controls.Add(chkSuppressUpsell, 0, 1)
        tlpMachine.Controls.Add(chkDisableServices, 0, 2)
        tlpMachine.Controls.Add(btnApplyMachine, 0, 3)
        tlpMachine.Controls.Add(btnRevertMachine, 0, 4)
        tlpMachine.Controls.Add(chkRevertPolicyOnClose, 0, 5)
        tlpMachine.Dock = DockStyle.Fill
        tlpMachine.Location = New Point(3, 27)
        tlpMachine.Name = "tlpMachine"
        tlpMachine.RowCount = 6
        tlpMachine.RowStyles.Add(New RowStyle())
        tlpMachine.RowStyles.Add(New RowStyle())
        tlpMachine.RowStyles.Add(New RowStyle())
        tlpMachine.RowStyles.Add(New RowStyle())
        tlpMachine.RowStyles.Add(New RowStyle())
        tlpMachine.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        tlpMachine.Size = New Size(418, 211)
        tlpMachine.TabIndex = 0
        ' 
        ' cboProduct
        ' 
        cboProduct.Dock = DockStyle.Fill
        cboProduct.DropDownStyle = ComboBoxStyle.DropDownList
        cboProduct.Location = New Point(3, 3)
        cboProduct.Name = "cboProduct"
        cboProduct.Size = New Size(412, 33)
        cboProduct.TabIndex = 0
        ' 
        ' chkSuppressUpsell
        ' 
        chkSuppressUpsell.AutoSize = True
        chkSuppressUpsell.Dock = DockStyle.Fill
        chkSuppressUpsell.Location = New Point(3, 42)
        chkSuppressUpsell.Name = "chkSuppressUpsell"
        chkSuppressUpsell.Size = New Size(412, 29)
        chkSuppressUpsell.TabIndex = 1
        chkSuppressUpsell.Text = "bAcroSuppressUpsell = 1"
        ' 
        ' chkDisableServices
        ' 
        chkDisableServices.AutoSize = True
        chkDisableServices.Dock = DockStyle.Fill
        chkDisableServices.Location = New Point(3, 77)
        chkDisableServices.Name = "chkDisableServices"
        chkDisableServices.Size = New Size(412, 29)
        chkDisableServices.TabIndex = 2
        chkDisableServices.Text = "cServices\bToggleAdobeDocumentServices = 1"
        ' 
        ' btnApplyMachine
        ' 
        btnApplyMachine.AutoSize = True
        btnApplyMachine.Dock = DockStyle.Fill
        btnApplyMachine.Location = New Point(3, 112)
        btnApplyMachine.Name = "btnApplyMachine"
        btnApplyMachine.Size = New Size(412, 35)
        btnApplyMachine.TabIndex = 3
        btnApplyMachine.Text = "Aplică (cere elevare)"
        btnApplyMachine.UseVisualStyleBackColor = True
        ' 
        ' btnRevertMachine
        ' 
        btnRevertMachine.AutoSize = True
        btnRevertMachine.Dock = DockStyle.Fill
        btnRevertMachine.Location = New Point(3, 153)
        btnRevertMachine.Name = "btnRevertMachine"
        btnRevertMachine.Size = New Size(412, 35)
        btnRevertMachine.TabIndex = 4
        btnRevertMachine.Text = "Revocă (cere elevare)"
        btnRevertMachine.UseVisualStyleBackColor = True
        ' 
        ' chkRevertPolicyOnClose
        ' 
        chkRevertPolicyOnClose.AutoSize = True
        chkRevertPolicyOnClose.Checked = True
        chkRevertPolicyOnClose.CheckState = CheckState.Checked
        chkRevertPolicyOnClose.Dock = DockStyle.Fill
        chkRevertPolicyOnClose.Location = New Point(3, 194)
        chkRevertPolicyOnClose.Name = "chkRevertPolicyOnClose"
        chkRevertPolicyOnClose.Size = New Size(412, 14)
        chkRevertPolicyOnClose.TabIndex = 5
        chkRevertPolicyOnClose.Text = "Revocă politica HKLM la închiderea bancului"
        ' 
        ' grpCmd
        ' 
        grpCmd.AutoSize = True
        grpCmd.AutoSizeMode = AutoSizeMode.GrowAndShrink
        grpCmd.Controls.Add(tlpCmd)
        grpCmd.Location = New Point(9, 2488)
        grpCmd.Name = "grpCmd"
        grpCmd.Size = New Size(112, 108)
        grpCmd.TabIndex = 11
        grpCmd.TabStop = False
        grpCmd.Text = "Linie de comandă"
        ' 
        ' tlpCmd
        ' 
        tlpCmd.AutoSize = True
        tlpCmd.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpCmd.ColumnCount = 1
        tlpCmd.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpCmd.Controls.Add(txtCmd, 0, 0)
        tlpCmd.Dock = DockStyle.Fill
        tlpCmd.Location = New Point(3, 27)
        tlpCmd.Name = "tlpCmd"
        tlpCmd.RowCount = 1
        tlpCmd.RowStyles.Add(New RowStyle())
        tlpCmd.Size = New Size(106, 78)
        tlpCmd.TabIndex = 0
        ' 
        ' txtCmd
        ' 
        txtCmd.Dock = DockStyle.Fill
        txtCmd.Font = New Font("Consolas", 8.25F)
        txtCmd.Location = New Point(3, 3)
        txtCmd.MinimumSize = New Size(0, 72)
        txtCmd.Multiline = True
        txtCmd.Name = "txtCmd"
        txtCmd.ReadOnly = True
        txtCmd.ScrollBars = ScrollBars.Vertical
        txtCmd.Size = New Size(100, 72)
        txtCmd.TabIndex = 0
        ' 
        ' tlpRight
        ' 
        tlpRight.ColumnCount = 1
        tlpRight.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlpRight.Controls.Add(pnlHost, 0, 0)
        tlpRight.Controls.Add(lblStatus, 0, 1)
        tlpRight.Dock = DockStyle.Fill
        tlpRight.Location = New Point(0, 0)
        tlpRight.Name = "tlpRight"
        tlpRight.RowCount = 2
        tlpRight.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlpRight.RowStyles.Add(New RowStyle())
        tlpRight.Size = New Size(764, 727)
        tlpRight.TabIndex = 0
        ' 
        ' pnlHost
        ' 
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Location = New Point(3, 3)
        pnlHost.Name = "pnlHost"
        pnlHost.Size = New Size(758, 688)
        pnlHost.TabIndex = 0
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(3, 694)
        lblStatus.Name = "lblStatus"
        lblStatus.Padding = New Padding(4)
        lblStatus.Size = New Size(758, 33)
        lblStatus.TabIndex = 1
        ' 
        ' tmrLayout
        ' 
        tmrLayout.Interval = 150
        ' 
        ' pnlButtons
        ' 
        pnlButtons.AutoSize = True
        pnlButtons.Controls.Add(btnPass)
        pnlButtons.Controls.Add(btnFail)
        pnlButtons.Dock = DockStyle.Bottom
        pnlButtons.FlowDirection = FlowDirection.RightToLeft
        pnlButtons.Location = New Point(0, 727)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Padding = New Padding(6)
        pnlButtons.Size = New Size(1240, 53)
        pnlButtons.TabIndex = 1
        ' 
        ' btnPass
        ' 
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.Yes
        btnPass.Location = New Point(1150, 9)
        btnPass.Name = "btnPass"
        btnPass.Size = New Size(75, 35)
        btnPass.TabIndex = 0
        btnPass.TabStop = False
        btnPass.Text = "Pass"
        btnPass.UseVisualStyleBackColor = True
        ' 
        ' btnFail
        ' 
        btnFail.AutoSize = True
        btnFail.DialogResult = DialogResult.No
        btnFail.Location = New Point(1069, 9)
        btnFail.Name = "btnFail"
        btnFail.Size = New Size(75, 35)
        btnFail.TabIndex = 1
        btnFail.TabStop = False
        btnFail.Text = "Fail"
        btnFail.UseVisualStyleBackColor = True
        ' 
        ' AdobeReaderHarnessForm
        ' 
        ' NU se setează AcceptButton: Enter nu are voie să dea un verdict. Butoanele au și
        ' TabStop = False, deci nu pot fi nici focalizate cu Tab și apoi apăsate din greșeală.
        ' Verdictul se dă DOAR cu mouse-ul, pe butonul respectiv.
        AcceptButton = Nothing
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        ' ATENȚIE: CancelButton apasă efectiv btnFail, deci Esc DĂ verdictul «Fail» (No) — nu doar
        ' închide. Setter-ul nu suprascrie DialogResult fiindcă nu e None. Lăsat aici deliberat
        ' (cererea a fost despre Enter); dacă nici Esc nu trebuie să dea verdict, se șterge linia.
        CancelButton = btnFail
        ClientSize = New Size(1240, 780)
        Controls.Add(splitMain)
        Controls.Add(pnlButtons)
        Name = "AdobeReaderHarnessForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Adobe Reader DC — încorporare + switch-uri (bare ascunse)"
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        flowOptions.ResumeLayout(False)
        flowOptions.PerformLayout()
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
        CType(numClipRight, ComponentModel.ISupportInitialize).EndInit()
        CType(numClipTop, ComponentModel.ISupportInitialize).EndInit()
        grpMove.ResumeLayout(False)
        grpMove.PerformLayout()
        grpHosting.ResumeLayout(False)
        grpHosting.PerformLayout()
        tlpHosting.ResumeLayout(False)
        tlpHosting.PerformLayout()
        flowDetachMode.ResumeLayout(False)
        flowDetachMode.PerformLayout()
        CType(numCaptureDelay, System.ComponentModel.ISupportInitialize).EndInit()
        CType(numCloseGrace, System.ComponentModel.ISupportInitialize).EndInit()
        grpActiveX.ResumeLayout(False)
        grpActiveX.PerformLayout()
        tlpActiveX.ResumeLayout(False)
        tlpActiveX.PerformLayout()
        flowAcroButtons.ResumeLayout(False)
        flowAcroButtons.PerformLayout()
        tlpMove.ResumeLayout(False)
        tlpMove.PerformLayout()
        CType(numDx, ComponentModel.ISupportInitialize).EndInit()
        CType(numDy, ComponentModel.ISupportInitialize).EndInit()
        CType(numDw, ComponentModel.ISupportInitialize).EndInit()
        CType(numDh, ComponentModel.ISupportInitialize).EndInit()
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
        tlpPrefRows.ResumeLayout(False)
        tlpPrefRows.PerformLayout()
        CType(gridPrefs, ComponentModel.ISupportInitialize).EndInit()
        grpMachine.ResumeLayout(False)
        grpMachine.PerformLayout()
        tlpMachine.ResumeLayout(False)
        tlpMachine.PerformLayout()
        grpCmd.ResumeLayout(False)
        grpCmd.PerformLayout()
        tlpCmd.ResumeLayout(False)
        tlpCmd.PerformLayout()
        tlpRight.ResumeLayout(False)
        tlpRight.PerformLayout()
        pnlButtons.ResumeLayout(False)
        pnlButtons.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub
End Class
