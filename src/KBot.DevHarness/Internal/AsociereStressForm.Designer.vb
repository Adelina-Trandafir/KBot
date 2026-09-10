<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AsociereStressForm
    Inherits KBot.Theming.KBotThemedForm

    ' Bancul de sarcina al lui KBotChartView si KBotLaneView: aceleasi doua suprafete pe care le
    ' pune AsociereForm, umplute cu un tablou nascocit a carui marime se schimba din casetele de
    ' sus (R = recepții, H = instantanee pe recepție). Controalele sunt declarate aici — regula
    ' casei: controalele WinForms in .Designer.vb, ca sa se vada la design time.

    Friend WithEvents pnlSize As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblReceipts As System.Windows.Forms.Label
    Friend WithEvents numReceipts As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblSnapshots As System.Windows.Forms.Label
    Friend WithEvents numSnapshots As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblLoose As System.Windows.Forms.Label
    Friend WithEvents numLoose As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblPayments As System.Windows.Forms.Label
    Friend WithEvents numPayments As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnGenerate As System.Windows.Forms.Button
    Friend WithEvents lblCounts As System.Windows.Forms.Label

    Friend WithEvents pnlMeasure As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnMeasure As System.Windows.Forms.Button
    Friend WithEvents lblFrames As System.Windows.Forms.Label
    Friend WithEvents numFrames As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkAnimate As System.Windows.Forms.CheckBox
    Friend WithEvents lblFps As System.Windows.Forms.Label
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button

    Friend WithEvents split As System.Windows.Forms.SplitContainer
    Friend WithEvents chart As Global.KBot.Controls.KBotChartView
    Friend WithEvents lanes As Global.KBot.Controls.KBotLaneView

    Friend WithEvents lstLog As System.Windows.Forms.ListBox

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim tabReceipt As New Global.KBot.Controls.KBotChartTab()
        Dim tabCommitment As New Global.KBot.Controls.KBotChartTab()
        Me.pnlSize = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblReceipts = New System.Windows.Forms.Label()
        Me.numReceipts = New System.Windows.Forms.NumericUpDown()
        Me.lblSnapshots = New System.Windows.Forms.Label()
        Me.numSnapshots = New System.Windows.Forms.NumericUpDown()
        Me.lblLoose = New System.Windows.Forms.Label()
        Me.numLoose = New System.Windows.Forms.NumericUpDown()
        Me.lblPayments = New System.Windows.Forms.Label()
        Me.numPayments = New System.Windows.Forms.NumericUpDown()
        Me.btnGenerate = New System.Windows.Forms.Button()
        Me.lblCounts = New System.Windows.Forms.Label()
        Me.pnlMeasure = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnMeasure = New System.Windows.Forms.Button()
        Me.lblFrames = New System.Windows.Forms.Label()
        Me.numFrames = New System.Windows.Forms.NumericUpDown()
        Me.chkAnimate = New System.Windows.Forms.CheckBox()
        Me.lblFps = New System.Windows.Forms.Label()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.split = New System.Windows.Forms.SplitContainer()
        Me.chart = New Global.KBot.Controls.KBotChartView()
        Me.lanes = New Global.KBot.Controls.KBotLaneView()
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        CType(Me.numReceipts, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numSnapshots, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numLoose, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numPayments, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numFrames, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.chart, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.lanes, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.split, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.split.Panel1.SuspendLayout()
        Me.split.Panel2.SuspendLayout()
        Me.split.SuspendLayout()
        Me.pnlSize.SuspendLayout()
        Me.pnlMeasure.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlSize
        '
        Me.pnlSize.Controls.Add(Me.lblReceipts)
        Me.pnlSize.Controls.Add(Me.numReceipts)
        Me.pnlSize.Controls.Add(Me.lblSnapshots)
        Me.pnlSize.Controls.Add(Me.numSnapshots)
        Me.pnlSize.Controls.Add(Me.lblLoose)
        Me.pnlSize.Controls.Add(Me.numLoose)
        Me.pnlSize.Controls.Add(Me.lblPayments)
        Me.pnlSize.Controls.Add(Me.numPayments)
        Me.pnlSize.Controls.Add(Me.btnGenerate)
        Me.pnlSize.Controls.Add(Me.lblCounts)
        Me.pnlSize.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSize.Height = 42
        Me.pnlSize.Padding = New System.Windows.Forms.Padding(6, 5, 6, 5)
        Me.pnlSize.Name = "pnlSize"
        '
        'lblReceipts
        '
        Me.lblReceipts.AutoSize = True
        Me.lblReceipts.Margin = New System.Windows.Forms.Padding(3, 7, 3, 0)
        Me.lblReceipts.Text = "R (recepții):"
        Me.lblReceipts.Name = "lblReceipts"
        '
        'numReceipts
        '
        Me.numReceipts.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        Me.numReceipts.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numReceipts.Value = New Decimal(New Integer() {10, 0, 0, 0})
        Me.numReceipts.Width = 60
        Me.numReceipts.Margin = New System.Windows.Forms.Padding(3, 3, 14, 3)
        Me.numReceipts.Name = "numReceipts"
        '
        'lblSnapshots
        '
        Me.lblSnapshots.AutoSize = True
        Me.lblSnapshots.Margin = New System.Windows.Forms.Padding(3, 7, 3, 0)
        Me.lblSnapshots.Text = "H pe recepție:"
        Me.lblSnapshots.Name = "lblSnapshots"
        '
        'numSnapshots
        '
        Me.numSnapshots.Maximum = New Decimal(New Integer() {500, 0, 0, 0})
        Me.numSnapshots.Minimum = New Decimal(New Integer() {0, 0, 0, 0})
        Me.numSnapshots.Value = New Decimal(New Integer() {20, 0, 0, 0})
        Me.numSnapshots.Width = 60
        Me.numSnapshots.Margin = New System.Windows.Forms.Padding(3, 3, 14, 3)
        Me.numSnapshots.Name = "numSnapshots"
        '
        'lblLoose
        '
        Me.lblLoose.AutoSize = True
        Me.lblLoose.Margin = New System.Windows.Forms.Padding(3, 7, 3, 0)
        Me.lblLoose.Text = "H neașezate:"
        Me.lblLoose.Name = "lblLoose"
        '
        'numLoose
        '
        Me.numLoose.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.numLoose.Minimum = New Decimal(New Integer() {0, 0, 0, 0})
        Me.numLoose.Value = New Decimal(New Integer() {6, 0, 0, 0})
        Me.numLoose.Width = 60
        Me.numLoose.Margin = New System.Windows.Forms.Padding(3, 3, 14, 3)
        Me.numLoose.Name = "numLoose"
        '
        'lblPayments
        '
        Me.lblPayments.AutoSize = True
        Me.lblPayments.Margin = New System.Windows.Forms.Padding(3, 7, 3, 0)
        Me.lblPayments.Text = "plăți (repere):"
        Me.lblPayments.Name = "lblPayments"
        '
        'numPayments
        '
        Me.numPayments.Maximum = New Decimal(New Integer() {200, 0, 0, 0})
        Me.numPayments.Minimum = New Decimal(New Integer() {0, 0, 0, 0})
        Me.numPayments.Value = New Decimal(New Integer() {8, 0, 0, 0})
        Me.numPayments.Width = 60
        Me.numPayments.Margin = New System.Windows.Forms.Padding(3, 3, 14, 3)
        Me.numPayments.Name = "numPayments"
        '
        'btnGenerate
        '
        Me.btnGenerate.AutoSize = True
        Me.btnGenerate.Text = "Regenerează"
        Me.btnGenerate.UseVisualStyleBackColor = True
        Me.btnGenerate.Name = "btnGenerate"
        '
        'lblCounts
        '
        Me.lblCounts.AutoSize = True
        Me.lblCounts.Margin = New System.Windows.Forms.Padding(14, 7, 3, 0)
        Me.lblCounts.Text = "—"
        Me.lblCounts.Name = "lblCounts"
        '
        'pnlMeasure
        '
        Me.pnlMeasure.Controls.Add(Me.btnMeasure)
        Me.pnlMeasure.Controls.Add(Me.lblFrames)
        Me.pnlMeasure.Controls.Add(Me.numFrames)
        Me.pnlMeasure.Controls.Add(Me.chkAnimate)
        Me.pnlMeasure.Controls.Add(Me.lblFps)
        Me.pnlMeasure.Controls.Add(Me.btnClassic)
        Me.pnlMeasure.Controls.Add(Me.btnDark)
        Me.pnlMeasure.Controls.Add(Me.btnModern)
        Me.pnlMeasure.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlMeasure.Height = 42
        Me.pnlMeasure.Padding = New System.Windows.Forms.Padding(6, 5, 6, 5)
        Me.pnlMeasure.Name = "pnlMeasure"
        '
        'btnMeasure
        '
        Me.btnMeasure.AutoSize = True
        Me.btnMeasure.Text = "Măsoară repictarea"
        Me.btnMeasure.UseVisualStyleBackColor = True
        Me.btnMeasure.Name = "btnMeasure"
        '
        'lblFrames
        '
        Me.lblFrames.AutoSize = True
        Me.lblFrames.Margin = New System.Windows.Forms.Padding(12, 7, 3, 0)
        Me.lblFrames.Text = "cadre:"
        Me.lblFrames.Name = "lblFrames"
        '
        'numFrames
        '
        Me.numFrames.Maximum = New Decimal(New Integer() {2000, 0, 0, 0})
        Me.numFrames.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numFrames.Value = New Decimal(New Integer() {60, 0, 0, 0})
        Me.numFrames.Width = 60
        Me.numFrames.Margin = New System.Windows.Forms.Padding(3, 3, 14, 3)
        Me.numFrames.Name = "numFrames"
        '
        'chkAnimate
        '
        Me.chkAnimate.AutoSize = True
        Me.chkAnimate.Margin = New System.Windows.Forms.Padding(3, 7, 12, 3)
        Me.chkAnimate.Text = "repictare continuă"
        Me.chkAnimate.Name = "chkAnimate"
        '
        'lblFps
        '
        Me.lblFps.AutoSize = True
        Me.lblFps.Margin = New System.Windows.Forms.Padding(3, 7, 20, 0)
        Me.lblFps.Text = "—"
        Me.lblFps.Name = "lblFps"
        '
        'btnClassic
        '
        Me.btnClassic.AutoSize = True
        Me.btnClassic.Text = "Classic"
        Me.btnClassic.UseVisualStyleBackColor = True
        Me.btnClassic.Name = "btnClassic"
        '
        'btnDark
        '
        Me.btnDark.AutoSize = True
        Me.btnDark.Text = "Dark"
        Me.btnDark.UseVisualStyleBackColor = True
        Me.btnDark.Name = "btnDark"
        '
        'btnModern
        '
        Me.btnModern.AutoSize = True
        Me.btnModern.Text = "Modern"
        Me.btnModern.UseVisualStyleBackColor = True
        Me.btnModern.Name = "btnModern"
        '
        'split
        '
        Me.split.Dock = System.Windows.Forms.DockStyle.Fill
        Me.split.Orientation = System.Windows.Forms.Orientation.Horizontal
        Me.split.SplitterWidth = 6
        Me.split.Panel1.Controls.Add(Me.chart)
        Me.split.Panel2.Controls.Add(Me.lanes)
        Me.split.Name = "split"
        '
        'chart
        '
        Me.chart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.chart.EmptyText = "Nimic de desenat."
        Me.chart.Font = New System.Drawing.Font("Calibri", 9.0!)
        Me.chart.HeaderCaption = " EVOLUȚIA VALORII"
        Me.chart.HeaderGradient = 5
        Me.chart.HeaderHeight = 30
        Me.chart.HeaderSeparatorWidth = 2
        Me.chart.LegendVisible = False
        Me.chart.PlotMargin = 2
        Me.chart.TabHeight = 26
        Me.chart.TabPadding = 4
        tabReceipt.Key = "receptie"
        tabReceipt.Text = "Recepția"
        tabReceipt.Tooltip = "Evoluția recepției alese"
        tabCommitment.Key = "angajament"
        tabCommitment.Text = "Tot angajamentul"
        tabCommitment.Tooltip = "Câte o linie pentru fiecare recepție, plus linia îngroșată a totalului."
        Me.chart.Tabs.Add(tabReceipt)
        Me.chart.Tabs.Add(tabCommitment)
        Me.chart.SelectedTabKey = "receptie"
        Me.chart.Name = "chart"
        '
        'lanes
        '
        Me.lanes.AxisVisible = True
        Me.lanes.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lanes.EmptyText = "Trage un instantaneu dintr-o bandă în alta ca să-l muți."
        Me.lanes.Font = New System.Drawing.Font("Calibri", 9.0!)
        Me.lanes.HeaderCaption = " AȘEZAREA INSTANTANEELOR"
        Me.lanes.HeaderGradient = 5
        Me.lanes.HeaderHeight = 30
        Me.lanes.HeaderSeparatorWidth = 2
        Me.lanes.LaneCaptionsVisible = True
        Me.lanes.LaneCaptionWidth = 150
        Me.lanes.LaneHeight = 18
        Me.lanes.LaneSpacing = 3
        Me.lanes.MarkerSize = 9
        Me.lanes.SegmentWidth = 4
        Me.lanes.TrailingSpace = 50
        Me.lanes.Name = "lanes"
        '
        'lstLog
        '
        Me.lstLog.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.lstLog.Height = 130
        Me.lstLog.IntegralHeight = False
        Me.lstLog.Name = "lstLog"
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Height = 44
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Name = "pnlButtons"
        '
        'btnFail
        '
        Me.btnFail.AutoSize = True
        Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnFail.Text = "Fail"
        Me.btnFail.UseVisualStyleBackColor = True
        Me.btnFail.Name = "btnFail"
        '
        'btnPass
        '
        Me.btnPass.AutoSize = True
        Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnPass.Text = "Pass"
        Me.btnPass.UseVisualStyleBackColor = True
        Me.btnPass.Name = "btnPass"
        '
        'AsociereStressForm
        '
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1100, 780)
        ' Ordine INVERSĂ de andocare (regula casei): Fill întâi, apoi benzile Bottom/Top —
        ' ultima bandă adăugată pe o latură ajunge cea mai apropiată de margine.
        Me.Controls.Add(Me.split)
        Me.Controls.Add(Me.lstLog)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlMeasure)
        Me.Controls.Add(Me.pnlSize)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Chart + Lane — banc de sarcină (R × H)"
        Me.Name = "AsociereStressForm"
        CType(Me.numReceipts, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numSnapshots, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numLoose, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numPayments, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numFrames, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.chart, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.lanes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.split.Panel1.ResumeLayout(False)
        Me.split.Panel2.ResumeLayout(False)
        CType(Me.split, System.ComponentModel.ISupportInitialize).EndInit()
        Me.split.ResumeLayout(False)
        Me.pnlSize.ResumeLayout(False)
        Me.pnlSize.PerformLayout()
        Me.pnlMeasure.ResumeLayout(False)
        Me.pnlMeasure.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
