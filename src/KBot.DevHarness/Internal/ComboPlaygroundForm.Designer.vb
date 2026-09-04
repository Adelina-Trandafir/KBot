<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ComboPlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    ' The KBotComboBox bench: the combo under test (Top, on the right) with the MEASUREMENT line
    ' under it, and on the left a PropertyGrid bound to that combo -- which IS the complete list
    ' of editable properties, categories and descriptions included, with no parallel list to fall
    ' behind every property somebody adds. Above the grid sit only the switches a property grid
    ' cannot give: reloading the sample list, clearing the selection, and the application scale.
    ' House rule: every WinForms control is declared here, in .Designer.vb.

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblScaling As System.Windows.Forms.Label
    Friend WithEvents cboScaling As System.Windows.Forms.ComboBox
    Friend WithEvents lblManual As System.Windows.Forms.Label
    Friend WithEvents numManual As System.Windows.Forms.NumericUpDown

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnPass As System.Windows.Forms.Button
    Friend WithEvents btnFail As System.Windows.Forms.Button

    Friend WithEvents pnlLeft As System.Windows.Forms.Panel
    Friend WithEvents splLeft As System.Windows.Forms.SplitContainer
    Friend WithEvents prop As System.Windows.Forms.PropertyGrid
    Friend WithEvents flowQuick As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblSecQuick As System.Windows.Forms.Label
    Friend WithEvents chkEditable As System.Windows.Forms.CheckBox
    Friend WithEvents chkLimitToList As System.Windows.Forms.CheckBox
    Friend WithEvents lblTextOffsetY As System.Windows.Forms.Label
    Friend WithEvents numTextOffsetY As System.Windows.Forms.NumericUpDown
    Friend WithEvents btnReloadItems As System.Windows.Forms.Button
    Friend WithEvents btnClearSelection As System.Windows.Forms.Button

    Friend WithEvents pnlRight As System.Windows.Forms.Panel
    Friend WithEvents cbo As KBot.Controls.KBotComboBox
    Friend WithEvents lblReadout As System.Windows.Forms.Label

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlTop = New FlowLayoutPanel()
        btnClassic = New Button()
        btnDark = New Button()
        btnModern = New Button()
        lblScaling = New Label()
        cboScaling = New ComboBox()
        lblManual = New Label()
        numManual = New NumericUpDown()
        pnlButtons = New FlowLayoutPanel()
        btnPass = New Button()
        btnFail = New Button()
        pnlLeft = New Panel()
        splLeft = New SplitContainer()
        prop = New PropertyGrid()
        flowQuick = New FlowLayoutPanel()
        lblSecQuick = New Label()
        chkEditable = New CheckBox()
        chkLimitToList = New CheckBox()
        lblTextOffsetY = New Label()
        numTextOffsetY = New NumericUpDown()
        btnReloadItems = New Button()
        btnClearSelection = New Button()
        pnlRight = New Panel()
        cbo = New Controls.KBotComboBox()
        lblReadout = New Label()
        pnlTop.SuspendLayout()
        CType(numManual, ComponentModel.ISupportInitialize).BeginInit()
        pnlButtons.SuspendLayout()
        pnlLeft.SuspendLayout()
        CType(splLeft, ComponentModel.ISupportInitialize).BeginInit()
        splLeft.Panel1.SuspendLayout()
        splLeft.Panel2.SuspendLayout()
        splLeft.SuspendLayout()
        flowQuick.SuspendLayout()
        CType(numTextOffsetY, ComponentModel.ISupportInitialize).BeginInit()
        pnlRight.SuspendLayout()
        SuspendLayout()
        '
        ' pnlTop
        '
        pnlTop.AutoSize = True
        pnlTop.Controls.Add(btnClassic)
        pnlTop.Controls.Add(btnDark)
        pnlTop.Controls.Add(btnModern)
        pnlTop.Controls.Add(lblScaling)
        pnlTop.Controls.Add(cboScaling)
        pnlTop.Controls.Add(lblManual)
        pnlTop.Controls.Add(numManual)
        pnlTop.Dock = DockStyle.Top
        pnlTop.Location = New Point(0, 0)
        pnlTop.Name = "pnlTop"
        pnlTop.Padding = New Padding(6)
        pnlTop.Size = New Size(1024, 53)
        pnlTop.TabIndex = 0
        '
        ' btnClassic
        '
        btnClassic.AutoSize = True
        btnClassic.Location = New Point(9, 9)
        btnClassic.Name = "btnClassic"
        btnClassic.Size = New Size(75, 35)
        btnClassic.TabIndex = 0
        btnClassic.Text = "Classic"
        btnClassic.UseVisualStyleBackColor = True
        '
        ' btnDark
        '
        btnDark.AutoSize = True
        btnDark.Location = New Point(90, 9)
        btnDark.Name = "btnDark"
        btnDark.Size = New Size(75, 35)
        btnDark.TabIndex = 1
        btnDark.Text = "Dark"
        btnDark.UseVisualStyleBackColor = True
        '
        ' btnModern
        '
        btnModern.AutoSize = True
        btnModern.Location = New Point(171, 9)
        btnModern.Name = "btnModern"
        btnModern.Size = New Size(85, 35)
        btnModern.TabIndex = 2
        btnModern.Text = "Modern"
        btnModern.UseVisualStyleBackColor = True
        '
        ' lblScaling
        '
        lblScaling.AutoSize = True
        lblScaling.Location = New Point(262, 6)
        lblScaling.Name = "lblScaling"
        lblScaling.Padding = New Padding(18, 8, 4, 0)
        lblScaling.Size = New Size(78, 33)
        lblScaling.TabIndex = 3
        lblScaling.Text = "Scară"
        '
        ' cboScaling
        '
        cboScaling.DropDownStyle = ComboBoxStyle.DropDownList
        cboScaling.Location = New Point(346, 9)
        cboScaling.Name = "cboScaling"
        cboScaling.Size = New Size(260, 33)
        cboScaling.TabIndex = 4
        '
        ' lblManual
        '
        lblManual.AutoSize = True
        lblManual.Location = New Point(612, 6)
        lblManual.Name = "lblManual"
        lblManual.Padding = New Padding(12, 8, 4, 0)
        lblManual.Size = New Size(74, 33)
        lblManual.TabIndex = 5
        lblManual.Text = "Factor"
        '
        ' numManual
        '
        numManual.DecimalPlaces = 2
        numManual.Increment = 0.05D
        numManual.Location = New Point(692, 9)
        numManual.Maximum = 4D
        numManual.Minimum = 0.5D
        numManual.Name = "numManual"
        numManual.Size = New Size(100, 31)
        numManual.TabIndex = 6
        numManual.Value = 1D
        '
        ' pnlButtons
        '
        pnlButtons.AutoSize = True
        pnlButtons.Controls.Add(btnPass)
        pnlButtons.Controls.Add(btnFail)
        pnlButtons.Dock = DockStyle.Bottom
        pnlButtons.FlowDirection = FlowDirection.RightToLeft
        pnlButtons.Location = New Point(0, 587)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Padding = New Padding(6)
        pnlButtons.Size = New Size(1024, 53)
        pnlButtons.TabIndex = 3
        '
        ' btnPass
        '
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.OK
        btnPass.Location = New Point(934, 9)
        btnPass.Name = "btnPass"
        btnPass.Size = New Size(75, 35)
        btnPass.TabIndex = 0
        btnPass.Text = "Pass"
        btnPass.UseVisualStyleBackColor = True
        '
        ' btnFail
        '
        btnFail.AutoSize = True
        btnFail.DialogResult = DialogResult.Cancel
        btnFail.Location = New Point(853, 9)
        btnFail.Name = "btnFail"
        btnFail.Size = New Size(75, 35)
        btnFail.TabIndex = 1
        btnFail.Text = "Fail"
        btnFail.UseVisualStyleBackColor = True
        '
        ' pnlLeft
        '
        pnlLeft.Controls.Add(splLeft)
        pnlLeft.Dock = DockStyle.Left
        pnlLeft.Location = New Point(0, 53)
        pnlLeft.Name = "pnlLeft"
        pnlLeft.Size = New Size(420, 534)
        pnlLeft.TabIndex = 1
        '
        ' splLeft
        '
        ' Quick switches on top, property grid below, with a bar between them.
        splLeft.Dock = DockStyle.Fill
        splLeft.Location = New Point(0, 0)
        splLeft.Name = "splLeft"
        splLeft.Orientation = Orientation.Horizontal
        splLeft.Panel1.Controls.Add(flowQuick)
        splLeft.Panel1MinSize = 120
        splLeft.Panel2.Controls.Add(prop)
        splLeft.Panel2MinSize = 180
        splLeft.Size = New Size(420, 534)
        splLeft.SplitterDistance = 240
        splLeft.SplitterWidth = 6
        splLeft.TabIndex = 0
        '
        ' prop
        '
        ' The grid IS the complete list: the «K-BOT Combo / K-BOT Combo Colors» categories come
        ' off the control's own attributes, so they cannot fall behind.
        prop.Dock = DockStyle.Fill
        prop.HelpVisible = True
        prop.Location = New Point(0, 0)
        prop.Name = "prop"
        prop.PropertySort = PropertySort.CategorizedAlphabetical
        prop.Size = New Size(420, 288)
        prop.TabIndex = 0
        prop.ToolbarVisible = True
        '
        ' flowQuick
        '
        flowQuick.AutoScroll = True
        flowQuick.Controls.Add(lblSecQuick)
        flowQuick.Controls.Add(chkEditable)
        flowQuick.Controls.Add(chkLimitToList)
        flowQuick.Controls.Add(lblTextOffsetY)
        flowQuick.Controls.Add(numTextOffsetY)
        flowQuick.Controls.Add(btnReloadItems)
        flowQuick.Controls.Add(btnClearSelection)
        flowQuick.Dock = DockStyle.Fill
        flowQuick.FlowDirection = FlowDirection.TopDown
        flowQuick.Location = New Point(0, 0)
        flowQuick.Name = "flowQuick"
        flowQuick.Padding = New Padding(8)
        flowQuick.Size = New Size(420, 240)
        flowQuick.TabIndex = 0
        flowQuick.WrapContents = False
        '
        ' lblSecQuick
        '
        lblSecQuick.AutoSize = True
        lblSecQuick.Location = New Point(11, 8)
        lblSecQuick.Name = "lblSecQuick"
        lblSecQuick.Size = New Size(300, 25)
        lblSecQuick.TabIndex = 0
        lblSecQuick.Text = "—— Ce nu încape într-o proprietate ——"
        '
        ' chkEditable
        '
        chkEditable.AutoSize = True
        chkEditable.Location = New Point(11, 36)
        chkEditable.Name = "chkEditable"
        chkEditable.Size = New Size(180, 29)
        chkEditable.TabIndex = 1
        chkEditable.Text = "Editable"
        chkEditable.UseVisualStyleBackColor = True
        '
        ' chkLimitToList
        '
        chkLimitToList.AutoSize = True
        chkLimitToList.Checked = True
        chkLimitToList.CheckState = CheckState.Checked
        chkLimitToList.Location = New Point(11, 71)
        chkLimitToList.Name = "chkLimitToList"
        chkLimitToList.Size = New Size(180, 29)
        chkLimitToList.TabIndex = 2
        chkLimitToList.Text = "LimitToList"
        chkLimitToList.UseVisualStyleBackColor = True
        '
        ' lblTextOffsetY
        '
        lblTextOffsetY.AutoSize = True
        lblTextOffsetY.Location = New Point(11, 106)
        lblTextOffsetY.Margin = New Padding(3, 3, 3, 0)
        lblTextOffsetY.Name = "lblTextOffsetY"
        lblTextOffsetY.Size = New Size(240, 25)
        lblTextOffsetY.TabIndex = 3
        lblTextOffsetY.Text = "TextOffsetY (0 = centrat automat)"
        '
        ' numTextOffsetY
        '
        numTextOffsetY.Location = New Point(11, 134)
        numTextOffsetY.Maximum = 10D
        numTextOffsetY.Minimum = -10D
        numTextOffsetY.Name = "numTextOffsetY"
        numTextOffsetY.Size = New Size(390, 31)
        numTextOffsetY.TabIndex = 4
        numTextOffsetY.Value = 0D
        '
        ' btnReloadItems
        '
        btnReloadItems.AutoSize = True
        btnReloadItems.Location = New Point(11, 171)
        btnReloadItems.Name = "btnReloadItems"
        btnReloadItems.Size = New Size(240, 35)
        btnReloadItems.TabIndex = 5
        btnReloadItems.Text = "Reîncarcă lista de probă"
        btnReloadItems.UseVisualStyleBackColor = True
        '
        ' btnClearSelection
        '
        btnClearSelection.AutoSize = True
        btnClearSelection.Location = New Point(11, 212)
        btnClearSelection.Name = "btnClearSelection"
        btnClearSelection.Size = New Size(180, 35)
        btnClearSelection.TabIndex = 6
        btnClearSelection.Text = "Fără selecție"
        btnClearSelection.UseVisualStyleBackColor = True
        '
        ' pnlRight
        '
        pnlRight.Controls.Add(lblReadout)
        pnlRight.Controls.Add(cbo)
        pnlRight.Dock = DockStyle.Fill
        pnlRight.Location = New Point(420, 53)
        pnlRight.Name = "pnlRight"
        pnlRight.Padding = New Padding(12)
        pnlRight.Size = New Size(604, 534)
        pnlRight.TabIndex = 2
        '
        ' cbo
        '
        ' The control under test. Docked Top with a fixed height, like every KBotComboBox on a
        ' real form -- it is a one-line input, not something that grows to fill a panel.
        cbo.Dock = DockStyle.Top
        cbo.DrawMode = DrawMode.OwnerDrawFixed
        cbo.DropDownStyle = ComboBoxStyle.DropDownList
        cbo.FlatStyle = FlatStyle.Flat
        cbo.Font = New Font("Segoe UI", 10F)
        cbo.Location = New Point(12, 12)
        cbo.Name = "cbo"
        cbo.Size = New Size(580, 42)
        cbo.TabIndex = 0
        '
        ' lblReadout
        '
        ' What ACTUALLY happened on screen: the scale, the combo's own size, the selection, and --
        ' when Editable is on -- the native EDIT's real rectangle next to the logical TextOffsetY typed.
        lblReadout.Dock = DockStyle.Fill
        lblReadout.Font = New Font("Consolas", 9F)
        lblReadout.Location = New Point(12, 54)
        lblReadout.Name = "lblReadout"
        lblReadout.Padding = New Padding(4)
        lblReadout.Size = New Size(580, 468)
        lblReadout.TabIndex = 1
        '
        ' ComboPlaygroundForm
        '
        AcceptButton = btnPass
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnFail
        ClientSize = New Size(1024, 640)
        Controls.Add(pnlRight)
        Controls.Add(pnlLeft)
        Controls.Add(pnlButtons)
        Controls.Add(pnlTop)
        Name = "ComboPlaygroundForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "KBotComboBox — playground proprietăți runtime"
        pnlTop.ResumeLayout(False)
        pnlTop.PerformLayout()
        CType(numManual, ComponentModel.ISupportInitialize).EndInit()
        pnlButtons.ResumeLayout(False)
        pnlButtons.PerformLayout()
        pnlLeft.ResumeLayout(False)
        splLeft.Panel1.ResumeLayout(False)
        splLeft.Panel2.ResumeLayout(False)
        CType(splLeft, ComponentModel.ISupportInitialize).EndInit()
        splLeft.ResumeLayout(False)
        flowQuick.ResumeLayout(False)
        flowQuick.PerformLayout()
        CType(numTextOffsetY, ComponentModel.ISupportInitialize).EndInit()
        pnlRight.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub
End Class
