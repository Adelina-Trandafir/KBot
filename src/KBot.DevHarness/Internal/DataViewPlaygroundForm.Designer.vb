<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DataViewPlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    ' Playground KBotDataView: grila testată (Fill), un panou stânga cu TOATE comutatoarele
    ' de proprietăți runtime, butoanele de temă (sus) și verdictul uman (jos). Regula casei:
    ' toate controalele WinForms se declară aici, în .Designer.vb.

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label

    Friend WithEvents grid As KBot.Controls.KBotDataView

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    Friend WithEvents flowLeft As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblSecGrid As System.Windows.Forms.Label
    Friend WithEvents lblAutoSize As System.Windows.Forms.Label
    Friend WithEvents cboAutoSize As System.Windows.Forms.ComboBox
    Friend WithEvents lblFill As System.Windows.Forms.Label
    Friend WithEvents cboFill As System.Windows.Forms.ComboBox
    Friend WithEvents lblSample As System.Windows.Forms.Label
    Friend WithEvents numSample As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblFrozen As System.Windows.Forms.Label
    Friend WithEvents numFrozen As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblRowH As System.Windows.Forms.Label
    Friend WithEvents numRowH As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblHeaderH As System.Windows.Forms.Label
    Friend WithEvents numHeaderH As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkHeader As System.Windows.Forms.CheckBox
    Friend WithEvents chkAlt As System.Windows.Forms.CheckBox
    Friend WithEvents chkReadOnly As System.Windows.Forms.CheckBox
    Friend WithEvents btnAutoSize As System.Windows.Forms.Button
    Friend WithEvents btnReset As System.Windows.Forms.Button
    Friend WithEvents lblSecCol As System.Windows.Forms.Label
    Friend WithEvents cboColumn As System.Windows.Forms.ComboBox
    Friend WithEvents chkColVisible As System.Windows.Forms.CheckBox
    Friend WithEvents chkColEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents chkColReadOnly As System.Windows.Forms.CheckBox
    Friend WithEvents chkColAutoHide As System.Windows.Forms.CheckBox
    Friend WithEvents lblColWidth As System.Windows.Forms.Label
    Friend WithEvents numColWidth As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblColMin As System.Windows.Forms.Label
    Friend WithEvents numColMin As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblColMax As System.Windows.Forms.Label
    Friend WithEvents numColMax As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblSecData As System.Windows.Forms.Label
    Friend WithEvents lblRowCount As System.Windows.Forms.Label
    Friend WithEvents cboRowCount As System.Windows.Forms.ComboBox

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.grid = New KBot.Controls.KBotDataView()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.flowLeft = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblSecGrid = New System.Windows.Forms.Label()
        Me.lblAutoSize = New System.Windows.Forms.Label()
        Me.cboAutoSize = New System.Windows.Forms.ComboBox()
        Me.lblFill = New System.Windows.Forms.Label()
        Me.cboFill = New System.Windows.Forms.ComboBox()
        Me.lblSample = New System.Windows.Forms.Label()
        Me.numSample = New System.Windows.Forms.NumericUpDown()
        Me.lblFrozen = New System.Windows.Forms.Label()
        Me.numFrozen = New System.Windows.Forms.NumericUpDown()
        Me.lblRowH = New System.Windows.Forms.Label()
        Me.numRowH = New System.Windows.Forms.NumericUpDown()
        Me.lblHeaderH = New System.Windows.Forms.Label()
        Me.numHeaderH = New System.Windows.Forms.NumericUpDown()
        Me.chkHeader = New System.Windows.Forms.CheckBox()
        Me.chkAlt = New System.Windows.Forms.CheckBox()
        Me.chkReadOnly = New System.Windows.Forms.CheckBox()
        Me.btnAutoSize = New System.Windows.Forms.Button()
        Me.btnReset = New System.Windows.Forms.Button()
        Me.lblSecCol = New System.Windows.Forms.Label()
        Me.cboColumn = New System.Windows.Forms.ComboBox()
        Me.chkColVisible = New System.Windows.Forms.CheckBox()
        Me.chkColEnabled = New System.Windows.Forms.CheckBox()
        Me.chkColReadOnly = New System.Windows.Forms.CheckBox()
        Me.chkColAutoHide = New System.Windows.Forms.CheckBox()
        Me.lblColWidth = New System.Windows.Forms.Label()
        Me.numColWidth = New System.Windows.Forms.NumericUpDown()
        Me.lblColMin = New System.Windows.Forms.Label()
        Me.numColMin = New System.Windows.Forms.NumericUpDown()
        Me.lblColMax = New System.Windows.Forms.Label()
        Me.numColMax = New System.Windows.Forms.NumericUpDown()
        Me.lblSecData = New System.Windows.Forms.Label()
        Me.lblRowCount = New System.Windows.Forms.Label()
        Me.cboRowCount = New System.Windows.Forms.ComboBox()
        Me.pnlTop.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.flowLeft.SuspendLayout()
        CType(Me.numSample, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numFrozen, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numRowH, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numHeaderH, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numColWidth, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numColMin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numColMax, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlTop — comutatoarele de temă + info
        '
        Me.pnlTop.AutoSize = True
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 40
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.Controls.Add(Me.btnClassic)
        Me.pnlTop.Controls.Add(Me.btnDark)
        Me.pnlTop.Controls.Add(Me.btnModern)
        Me.pnlTop.Controls.Add(Me.lblInfo)
        Me.pnlTop.Name = "pnlTop"
        '
        Me.btnClassic.AutoSize = True : Me.btnClassic.Text = "Classic" : Me.btnClassic.Name = "btnClassic" : Me.btnClassic.UseVisualStyleBackColor = True
        Me.btnDark.AutoSize = True : Me.btnDark.Text = "Dark" : Me.btnDark.Name = "btnDark" : Me.btnDark.UseVisualStyleBackColor = True
        Me.btnModern.AutoSize = True : Me.btnModern.Text = "Modern" : Me.btnModern.Name = "btnModern" : Me.btnModern.UseVisualStyleBackColor = True
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Padding = New System.Windows.Forms.Padding(12, 8, 0, 0)
        Me.lblInfo.Name = "lblInfo"
        '
        'flowLeft — panoul de proprietăți (derulabil)
        '
        Me.flowLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.flowLeft.Width = 280
        Me.flowLeft.AutoScroll = True
        Me.flowLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.flowLeft.WrapContents = False
        Me.flowLeft.Padding = New System.Windows.Forms.Padding(8)
        Me.flowLeft.Name = "flowLeft"
        Me.flowLeft.Controls.Add(Me.lblSecGrid)
        Me.flowLeft.Controls.Add(Me.lblAutoSize)
        Me.flowLeft.Controls.Add(Me.cboAutoSize)
        Me.flowLeft.Controls.Add(Me.lblFill)
        Me.flowLeft.Controls.Add(Me.cboFill)
        Me.flowLeft.Controls.Add(Me.lblSample)
        Me.flowLeft.Controls.Add(Me.numSample)
        Me.flowLeft.Controls.Add(Me.lblFrozen)
        Me.flowLeft.Controls.Add(Me.numFrozen)
        Me.flowLeft.Controls.Add(Me.lblRowH)
        Me.flowLeft.Controls.Add(Me.numRowH)
        Me.flowLeft.Controls.Add(Me.lblHeaderH)
        Me.flowLeft.Controls.Add(Me.numHeaderH)
        Me.flowLeft.Controls.Add(Me.chkHeader)
        Me.flowLeft.Controls.Add(Me.chkAlt)
        Me.flowLeft.Controls.Add(Me.chkReadOnly)
        Me.flowLeft.Controls.Add(Me.btnAutoSize)
        Me.flowLeft.Controls.Add(Me.btnReset)
        Me.flowLeft.Controls.Add(Me.lblSecCol)
        Me.flowLeft.Controls.Add(Me.cboColumn)
        Me.flowLeft.Controls.Add(Me.chkColVisible)
        Me.flowLeft.Controls.Add(Me.chkColEnabled)
        Me.flowLeft.Controls.Add(Me.chkColReadOnly)
        Me.flowLeft.Controls.Add(Me.chkColAutoHide)
        Me.flowLeft.Controls.Add(Me.lblColWidth)
        Me.flowLeft.Controls.Add(Me.numColWidth)
        Me.flowLeft.Controls.Add(Me.lblColMin)
        Me.flowLeft.Controls.Add(Me.numColMin)
        Me.flowLeft.Controls.Add(Me.lblColMax)
        Me.flowLeft.Controls.Add(Me.numColMax)
        Me.flowLeft.Controls.Add(Me.lblSecData)
        Me.flowLeft.Controls.Add(Me.lblRowCount)
        Me.flowLeft.Controls.Add(Me.cboRowCount)
        '
        Me.lblSecGrid.AutoSize = True : Me.lblSecGrid.Text = "—— Grilă ——" : Me.lblSecGrid.Name = "lblSecGrid"
        Me.lblAutoSize.AutoSize = True : Me.lblAutoSize.Text = "AutoSizeColumnsMode" : Me.lblAutoSize.Name = "lblAutoSize"
        Me.cboAutoSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboAutoSize.Width = 250 : Me.cboAutoSize.Name = "cboAutoSize"
        Me.lblFill.AutoSize = True : Me.lblFill.Text = "ColumnFillMode" : Me.lblFill.Name = "lblFill"
        Me.cboFill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboFill.Width = 250 : Me.cboFill.Name = "cboFill"
        Me.lblSample.AutoSize = True : Me.lblSample.Text = "AutoSizeSampleRows (0 = toate)" : Me.lblSample.Name = "lblSample"
        Me.numSample.Minimum = 0D : Me.numSample.Maximum = 100000D : Me.numSample.Value = 200D : Me.numSample.Width = 120 : Me.numSample.Name = "numSample"
        Me.lblFrozen.AutoSize = True : Me.lblFrozen.Text = "FrozenColumnCount" : Me.lblFrozen.Name = "lblFrozen"
        Me.numFrozen.Minimum = 0D : Me.numFrozen.Maximum = 8D : Me.numFrozen.Value = 1D : Me.numFrozen.Width = 120 : Me.numFrozen.Name = "numFrozen"
        Me.lblRowH.AutoSize = True : Me.lblRowH.Text = "RowHeight" : Me.lblRowH.Name = "lblRowH"
        Me.numRowH.Minimum = 12D : Me.numRowH.Maximum = 80D : Me.numRowH.Value = 28D : Me.numRowH.Width = 120 : Me.numRowH.Name = "numRowH"
        Me.lblHeaderH.AutoSize = True : Me.lblHeaderH.Text = "HeaderHeight" : Me.lblHeaderH.Name = "lblHeaderH"
        Me.numHeaderH.Minimum = 0D : Me.numHeaderH.Maximum = 80D : Me.numHeaderH.Value = 30D : Me.numHeaderH.Width = 120 : Me.numHeaderH.Name = "numHeaderH"
        Me.chkHeader.AutoSize = True : Me.chkHeader.Text = "ShowHeader" : Me.chkHeader.Checked = True : Me.chkHeader.Name = "chkHeader"
        Me.chkAlt.AutoSize = True : Me.chkAlt.Text = "AlternatingRows" : Me.chkAlt.Checked = True : Me.chkAlt.Name = "chkAlt"
        Me.chkReadOnly.AutoSize = True : Me.chkReadOnly.Text = "ReadOnlyGrid" : Me.chkReadOnly.Name = "chkReadOnly"
        Me.btnAutoSize.AutoSize = True : Me.btnAutoSize.Text = "AutoSizeColumns()" : Me.btnAutoSize.Name = "btnAutoSize" : Me.btnAutoSize.UseVisualStyleBackColor = True
        Me.btnReset.AutoSize = True : Me.btnReset.Text = "ResetColumnSizing()" : Me.btnReset.Name = "btnReset" : Me.btnReset.UseVisualStyleBackColor = True
        '
        Me.lblSecCol.AutoSize = True : Me.lblSecCol.Text = "—— Coloană (inspector) ——" : Me.lblSecCol.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecCol.Name = "lblSecCol"
        Me.cboColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboColumn.Width = 250 : Me.cboColumn.Name = "cboColumn"
        Me.chkColVisible.AutoSize = True : Me.chkColVisible.Text = "Visible" : Me.chkColVisible.Name = "chkColVisible"
        Me.chkColEnabled.AutoSize = True : Me.chkColEnabled.Text = "Enabled" : Me.chkColEnabled.Name = "chkColEnabled"
        Me.chkColReadOnly.AutoSize = True : Me.chkColReadOnly.Text = "ReadOnly" : Me.chkColReadOnly.Name = "chkColReadOnly"
        Me.chkColAutoHide.AutoSize = True : Me.chkColAutoHide.Text = "AutoHide (dispare când nu încape)" : Me.chkColAutoHide.Name = "chkColAutoHide"
        Me.lblColWidth.AutoSize = True : Me.lblColWidth.Text = "Width" : Me.lblColWidth.Name = "lblColWidth"
        Me.numColWidth.Minimum = 0D : Me.numColWidth.Maximum = 4000D : Me.numColWidth.Value = 100D : Me.numColWidth.Width = 120 : Me.numColWidth.Name = "numColWidth"
        Me.lblColMin.AutoSize = True : Me.lblColMin.Text = "MinWidth" : Me.lblColMin.Name = "lblColMin"
        Me.numColMin.Minimum = 0D : Me.numColMin.Maximum = 4000D : Me.numColMin.Value = 40D : Me.numColMin.Width = 120 : Me.numColMin.Name = "numColMin"
        Me.lblColMax.AutoSize = True : Me.lblColMax.Text = "MaxWidth (0 = neplafonat)" : Me.lblColMax.Name = "lblColMax"
        Me.numColMax.Minimum = 0D : Me.numColMax.Maximum = 4000D : Me.numColMax.Value = 0D : Me.numColMax.Width = 120 : Me.numColMax.Name = "numColMax"
        '
        Me.lblSecData.AutoSize = True : Me.lblSecData.Text = "—— Date ——" : Me.lblSecData.Margin = New System.Windows.Forms.Padding(3, 12, 3, 0) : Me.lblSecData.Name = "lblSecData"
        Me.lblRowCount.AutoSize = True : Me.lblRowCount.Text = "Rânduri" : Me.lblRowCount.Name = "lblRowCount"
        Me.cboRowCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList : Me.cboRowCount.Width = 250 : Me.cboRowCount.Name = "cboRowCount"
        '
        'grid — controlul testat
        '
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.Name = "grid"
        '
        'pnlButtons — verdictul uman
        '
        Me.pnlButtons.AutoSize = True
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Name = "pnlButtons"
        '
        Me.btnFail.AutoSize = True : Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel : Me.btnFail.Text = "Fail" : Me.btnFail.Name = "btnFail" : Me.btnFail.UseVisualStyleBackColor = True
        Me.btnPass.AutoSize = True : Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK : Me.btnPass.Text = "Pass" : Me.btnPass.Name = "btnPass" : Me.btnPass.UseVisualStyleBackColor = True
        '
        'DataViewPlaygroundForm
        '
        Me.AcceptButton = Me.btnPass
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1180, 700)
        ' Ordine de dock (regula casei): Fill întâi, apoi Left, apoi Bottom/Top (ca marginile
        ' orizontale să treacă peste banda din stânga).
        Me.Controls.Add(Me.grid)
        Me.Controls.Add(Me.flowLeft)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlTop)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "KBotDataView — playground proprietăți runtime"
        Me.Name = "DataViewPlaygroundForm"
        Me.pnlTop.ResumeLayout(False) : Me.pnlTop.PerformLayout()
        Me.pnlButtons.ResumeLayout(False) : Me.pnlButtons.PerformLayout()
        Me.flowLeft.ResumeLayout(False) : Me.flowLeft.PerformLayout()
        CType(Me.numSample, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numFrozen, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numRowH, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numHeaderH, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numColWidth, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numColMin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numColMax, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False) : Me.PerformLayout()
    End Sub
End Class
