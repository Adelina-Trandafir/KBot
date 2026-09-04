<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RichTextPlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    ' The KBotRichTextEditor bench: the editor under test (Fill, on the right) with the
    ' MEASUREMENT line under it, and on the left a PropertyGrid bound to that editor -- which IS
    ' the complete list of editable properties, categories and descriptions included, with no
    ' parallel list to fall behind every property somebody adds. Above the grid sit only the
    ' switches a property grid cannot give: which icon set is bound, in which ORDER it is bound,
    ' and the application scale.
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
    Friend WithEvents lblIconSet As System.Windows.Forms.Label
    Friend WithEvents cboIconSet As System.Windows.Forms.ComboBox
    Friend WithEvents lblIconOrder As System.Windows.Forms.Label
    Friend WithEvents cboIconOrder As System.Windows.Forms.ComboBox
    Friend WithEvents lblLayout As System.Windows.Forms.Label
    Friend WithEvents cboLayout As System.Windows.Forms.ComboBox
    Friend WithEvents chkEditabil As System.Windows.Forms.CheckBox
    Friend WithEvents chkCollapsed As System.Windows.Forms.CheckBox
    Friend WithEvents btnSample As System.Windows.Forms.Button

    Friend WithEvents pnlRight As System.Windows.Forms.Panel
    Friend WithEvents edt As KBot.Controls.KBotRichTextEditor
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
        lblIconSet = New Label()
        cboIconSet = New ComboBox()
        lblIconOrder = New Label()
        cboIconOrder = New ComboBox()
        lblLayout = New Label()
        cboLayout = New ComboBox()
        chkEditabil = New CheckBox()
        chkCollapsed = New CheckBox()
        btnSample = New Button()
        pnlRight = New Panel()
        edt = New Controls.KBotRichTextEditor()
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
        pnlTop.Size = New Size(1280, 53)
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
        pnlButtons.Location = New Point(0, 747)
        pnlButtons.Name = "pnlButtons"
        pnlButtons.Padding = New Padding(6)
        pnlButtons.Size = New Size(1280, 53)
        pnlButtons.TabIndex = 3
        '
        ' btnPass
        '
        btnPass.AutoSize = True
        btnPass.DialogResult = DialogResult.OK
        btnPass.Location = New Point(1190, 9)
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
        btnFail.Location = New Point(1109, 9)
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
        pnlLeft.Size = New Size(470, 694)
        pnlLeft.TabIndex = 1
        '
        ' splLeft
        '
        ' Quick switches on top, property grid below, with a bar between them: the property list
        ' is long, so the operator has to be able to give it more room.
        splLeft.Dock = DockStyle.Fill
        splLeft.Location = New Point(0, 0)
        splLeft.Name = "splLeft"
        splLeft.Orientation = Orientation.Horizontal
        splLeft.Panel1.Controls.Add(flowQuick)
        splLeft.Panel1MinSize = 120
        splLeft.Panel2.Controls.Add(prop)
        splLeft.Panel2MinSize = 180
        splLeft.Size = New Size(470, 694)
        splLeft.SplitterDistance = 380
        splLeft.SplitterWidth = 6
        splLeft.TabIndex = 0
        '
        ' prop
        '
        ' The grid IS the complete list: the «K-BOT Header / Editor / Footer / Icons / Collapse /
        ' Colors» categories come off the control's own attributes, so they cannot fall behind.
        prop.Dock = DockStyle.Fill
        prop.HelpVisible = True
        prop.Location = New Point(0, 0)
        prop.Name = "prop"
        prop.PropertySort = PropertySort.CategorizedAlphabetical
        prop.Size = New Size(470, 358)
        prop.TabIndex = 0
        prop.ToolbarVisible = True
        '
        ' flowQuick
        '
        flowQuick.AutoScroll = True
        flowQuick.Controls.Add(lblSecQuick)
        flowQuick.Controls.Add(lblIconSet)
        flowQuick.Controls.Add(cboIconSet)
        flowQuick.Controls.Add(lblIconOrder)
        flowQuick.Controls.Add(cboIconOrder)
        flowQuick.Controls.Add(lblLayout)
        flowQuick.Controls.Add(cboLayout)
        flowQuick.Controls.Add(chkEditabil)
        flowQuick.Controls.Add(chkCollapsed)
        flowQuick.Controls.Add(btnSample)
        flowQuick.Dock = DockStyle.Fill
        flowQuick.FlowDirection = FlowDirection.TopDown
        flowQuick.Location = New Point(0, 0)
        flowQuick.Name = "flowQuick"
        flowQuick.Padding = New Padding(8)
        flowQuick.Size = New Size(470, 330)
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
        ' lblIconSet
        '
        lblIconSet.AutoSize = True
        lblIconSet.Location = New Point(11, 36)
        lblIconSet.Margin = New Padding(3, 3, 3, 0)
        lblIconSet.Name = "lblIconSet"
        lblIconSet.Size = New Size(240, 25)
        lblIconSet.TabIndex = 1
        lblIconSet.Text = "Set de pictograme (Images)"
        '
        ' cboIconSet
        '
        cboIconSet.DropDownStyle = ComboBoxStyle.DropDownList
        cboIconSet.Location = New Point(11, 64)
        cboIconSet.Name = "cboIconSet"
        cboIconSet.Size = New Size(390, 33)
        cboIconSet.TabIndex = 2
        '
        ' lblIconOrder
        '
        lblIconOrder.AutoSize = True
        lblIconOrder.Location = New Point(11, 100)
        lblIconOrder.Margin = New Padding(3, 3, 3, 0)
        lblIconOrder.Name = "lblIconOrder"
        lblIconOrder.Size = New Size(240, 25)
        lblIconOrder.TabIndex = 3
        lblIconOrder.Text = "Ordinea legării"
        '
        ' cboIconOrder
        '
        cboIconOrder.DropDownStyle = ComboBoxStyle.DropDownList
        cboIconOrder.Location = New Point(11, 128)
        cboIconOrder.Name = "cboIconOrder"
        cboIconOrder.Size = New Size(390, 33)
        cboIconOrder.TabIndex = 4
        '
        ' lblLayout
        '
        lblLayout.AutoSize = True
        lblLayout.Location = New Point(11, 164)
        lblLayout.Margin = New Padding(3, 3, 3, 0)
        lblLayout.Name = "lblLayout"
        lblLayout.Size = New Size(240, 25)
        lblLayout.TabIndex = 5
        lblLayout.Text = "ButtonImageLayout"
        '
        ' cboLayout
        '
        cboLayout.DropDownStyle = ComboBoxStyle.DropDownList
        cboLayout.Location = New Point(11, 192)
        cboLayout.Name = "cboLayout"
        cboLayout.Size = New Size(390, 33)
        cboLayout.TabIndex = 6
        '
        ' chkEditabil
        '
        chkEditabil.AutoSize = True
        chkEditabil.Checked = True
        chkEditabil.CheckState = CheckState.Checked
        chkEditabil.Location = New Point(11, 228)
        chkEditabil.Name = "chkEditabil"
        chkEditabil.Size = New Size(180, 29)
        chkEditabil.TabIndex = 7
        chkEditabil.Text = "Editabil"
        chkEditabil.UseVisualStyleBackColor = True
        '
        ' chkCollapsed
        '
        chkCollapsed.AutoSize = True
        chkCollapsed.Location = New Point(11, 263)
        chkCollapsed.Name = "chkCollapsed"
        chkCollapsed.Size = New Size(180, 29)
        chkCollapsed.TabIndex = 8
        chkCollapsed.Text = "Strâns (Collapsed)"
        chkCollapsed.UseVisualStyleBackColor = True
        '
        ' btnSample
        '
        btnSample.AutoSize = True
        btnSample.Location = New Point(11, 298)
        btnSample.Name = "btnSample"
        btnSample.Size = New Size(190, 35)
        btnSample.TabIndex = 9
        btnSample.Text = "Text de probă"
        btnSample.UseVisualStyleBackColor = True
        '
        ' pnlRight
        '
        pnlRight.Controls.Add(edt)
        pnlRight.Controls.Add(lblReadout)
        pnlRight.Dock = DockStyle.Fill
        pnlRight.Location = New Point(470, 53)
        pnlRight.Name = "pnlRight"
        pnlRight.Padding = New Padding(8)
        pnlRight.Size = New Size(850, 694)
        pnlRight.TabIndex = 2
        '
        ' edt
        '
        edt.CollapseButton = True
        edt.Dock = DockStyle.Fill
        edt.Location = New Point(8, 8)
        edt.Name = "edt"
        edt.Size = New Size(834, 534)
        edt.TabIndex = 0
        '
        ' lblReadout
        '
        ' What ACTUALLY happened on screen: the scale, the header height in real pixels, the
        ' button size and -- for each button -- whether a picture landed on it or the fallback
        ' letter did.
        lblReadout.Dock = DockStyle.Bottom
        lblReadout.Font = New Font("Consolas", 9F)
        lblReadout.Location = New Point(8, 542)
        lblReadout.Name = "lblReadout"
        lblReadout.Padding = New Padding(4)
        lblReadout.Size = New Size(834, 144)
        lblReadout.TabIndex = 1
        '
        ' RichTextPlaygroundForm
        '
        AcceptButton = btnPass
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnFail
        ClientSize = New Size(1280, 800)
        Controls.Add(pnlRight)
        Controls.Add(pnlLeft)
        Controls.Add(pnlButtons)
        Controls.Add(pnlTop)
        Name = "RichTextPlaygroundForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "KBotRichTextEditor — playground proprietăți runtime"
        pnlTop.ResumeLayout(False)
        pnlTop.PerformLayout()
        CType(numManual, ComponentModel.ISupportInitialize).EndInit()
        pnlButtons.ResumeLayout(False)
        pnlButtons.PerformLayout()
        flowQuick.ResumeLayout(False)
        flowQuick.PerformLayout()
        splLeft.Panel1.ResumeLayout(False)
        splLeft.Panel1.PerformLayout()
        splLeft.Panel2.ResumeLayout(False)
        CType(splLeft, ComponentModel.ISupportInitialize).EndInit()
        splLeft.ResumeLayout(False)
        pnlLeft.ResumeLayout(False)
        pnlRight.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub
End Class
