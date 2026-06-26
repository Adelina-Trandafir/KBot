<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VariablesInputForm
    Inherits System.Windows.Forms.Form

    ''' <summary>
    ''' Eliberare resurse.
    ''' </summary>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso (components IsNot Nothing) Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlLeft = New System.Windows.Forms.Panel()
        lstVars = New System.Windows.Forms.ListBox()
        lblListTitle = New System.Windows.Forms.Label()
        splitter = New System.Windows.Forms.Panel()
        pnlBottom = New System.Windows.Forms.Panel()
        lblAllStatus = New System.Windows.Forms.Label()
        btnCancel = New System.Windows.Forms.Button()
        btnContinue = New System.Windows.Forms.Button()
        pnlRight = New System.Windows.Forms.Panel()
        TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        tblVarEditor = New System.Windows.Forms.TableLayoutPanel()
        lblVarName = New System.Windows.Forms.Label()
        lblConstraints = New System.Windows.Forms.Label()
        lblTypeInfo = New System.Windows.Forms.Label()
        lblDescription = New System.Windows.Forms.Label()
        pnlInputWrapper = New System.Windows.Forms.Panel()
        txtValue = New System.Windows.Forms.TextBox()
        mskValue = New System.Windows.Forms.MaskedTextBox()
        lblError = New System.Windows.Forms.Label()
        FlowLayoutPanel2 = New System.Windows.Forms.FlowLayoutPanel()
        btnLoadVariablesFromJSON = New System.Windows.Forms.Button()
        btnSaveVariablesToJSON = New System.Windows.Forms.Button()
        btnSave = New System.Windows.Forms.Button()
        pnlLeft.SuspendLayout()
        pnlBottom.SuspendLayout()
        pnlRight.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        tblVarEditor.SuspendLayout()
        pnlInputWrapper.SuspendLayout()
        FlowLayoutPanel2.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlLeft
        ' 
        pnlLeft.Controls.Add(lstVars)
        pnlLeft.Controls.Add(lblListTitle)
        pnlLeft.Dock = System.Windows.Forms.DockStyle.Left
        pnlLeft.Location = New System.Drawing.Point(0, 0)
        pnlLeft.Name = "pnlLeft"
        pnlLeft.Padding = New System.Windows.Forms.Padding(8)
        pnlLeft.Size = New System.Drawing.Size(240, 706)
        pnlLeft.TabIndex = 2
        ' 
        ' lstVars
        ' 
        lstVars.BackColor = Drawing.Color.White
        lstVars.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        lstVars.Dock = System.Windows.Forms.DockStyle.Fill
        lstVars.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        lstVars.Font = New System.Drawing.Font("Segoe UI", 9.5F)
        lstVars.ItemHeight = 28
        lstVars.Location = New System.Drawing.Point(8, 34)
        lstVars.Name = "lstVars"
        lstVars.Size = New System.Drawing.Size(224, 664)
        lstVars.TabIndex = 0
        ' 
        ' lblListTitle
        ' 
        lblListTitle.Dock = System.Windows.Forms.DockStyle.Top
        lblListTitle.Font = New System.Drawing.Font("Segoe UI", 9.5F, Drawing.FontStyle.Bold)
        lblListTitle.ForeColor = Drawing.Color.FromArgb(CByte(55), CByte(71), CByte(79))
        lblListTitle.Location = New System.Drawing.Point(8, 8)
        lblListTitle.Name = "lblListTitle"
        lblListTitle.Size = New System.Drawing.Size(224, 26)
        lblListTitle.TabIndex = 1
        lblListTitle.Text = "Variabile"
        ' 
        ' splitter
        ' 
        splitter.BackColor = Drawing.Color.FromArgb(CByte(200), CByte(200), CByte(200))
        splitter.Dock = System.Windows.Forms.DockStyle.Left
        splitter.Location = New System.Drawing.Point(240, 0)
        splitter.Name = "splitter"
        splitter.Size = New System.Drawing.Size(1, 706)
        splitter.TabIndex = 1
        ' 
        ' pnlBottom
        ' 
        pnlBottom.Controls.Add(lblAllStatus)
        pnlBottom.Controls.Add(btnCancel)
        pnlBottom.Controls.Add(btnContinue)
        pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlBottom.Location = New System.Drawing.Point(0, 706)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Padding = New System.Windows.Forms.Padding(8)
        pnlBottom.Size = New System.Drawing.Size(917, 48)
        pnlBottom.TabIndex = 3
        ' 
        ' lblAllStatus
        ' 
        lblAllStatus.Dock = System.Windows.Forms.DockStyle.Left
        lblAllStatus.Font = New System.Drawing.Font("Segoe UI", 8.5F)
        lblAllStatus.ForeColor = Drawing.Color.FromArgb(CByte(100), CByte(100), CByte(100))
        lblAllStatus.Location = New System.Drawing.Point(8, 8)
        lblAllStatus.Name = "lblAllStatus"
        lblAllStatus.Size = New System.Drawing.Size(200, 32)
        lblAllStatus.TabIndex = 0
        lblAllStatus.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' btnCancel
        ' 
        btnCancel.BackColor = Drawing.Color.FromArgb(CByte(240), CByte(240), CByte(240))
        btnCancel.Dock = System.Windows.Forms.DockStyle.Right
        btnCancel.FlatAppearance.BorderColor = Drawing.Color.FromArgb(CByte(180), CByte(180), CByte(180))
        btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnCancel.ForeColor = Drawing.Color.FromArgb(CByte(60), CByte(60), CByte(60))
        btnCancel.Location = New System.Drawing.Point(719, 8)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New System.Drawing.Size(90, 32)
        btnCancel.TabIndex = 1
        btnCancel.Text = "Renunță"
        btnCancel.UseVisualStyleBackColor = False
        ' 
        ' btnContinue
        ' 
        btnContinue.BackColor = Drawing.Color.FromArgb(CByte(46), CByte(125), CByte(50))
        btnContinue.Dock = System.Windows.Forms.DockStyle.Right
        btnContinue.Enabled = False
        btnContinue.FlatAppearance.BorderSize = 0
        btnContinue.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnContinue.ForeColor = Drawing.Color.White
        btnContinue.Location = New System.Drawing.Point(809, 8)
        btnContinue.Name = "btnContinue"
        btnContinue.Size = New System.Drawing.Size(100, 32)
        btnContinue.TabIndex = 2
        btnContinue.Text = "Continuă  ▶"
        btnContinue.UseVisualStyleBackColor = False
        ' 
        ' pnlRight
        ' 
        pnlRight.Controls.Add(TableLayoutPanel1)
        pnlRight.Dock = System.Windows.Forms.DockStyle.Fill
        pnlRight.Location = New System.Drawing.Point(241, 0)
        pnlRight.Name = "pnlRight"
        pnlRight.Padding = New System.Windows.Forms.Padding(16, 12, 16, 12)
        pnlRight.Size = New System.Drawing.Size(676, 706)
        pnlRight.TabIndex = 0
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 1
        TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        TableLayoutPanel1.Controls.Add(tblVarEditor, 0, 0)
        TableLayoutPanel1.Controls.Add(FlowLayoutPanel2, 0, 1)
        TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        TableLayoutPanel1.Location = New System.Drawing.Point(16, 12)
        TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 121.0F))
        TableLayoutPanel1.Size = New System.Drawing.Size(644, 682)
        TableLayoutPanel1.TabIndex = 0
        ' 
        ' tblVarEditor
        ' 
        tblVarEditor.ColumnCount = 1
        tblVarEditor.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        tblVarEditor.Controls.Add(lblVarName, 0, 0)
        tblVarEditor.Controls.Add(lblConstraints, 0, 1)
        tblVarEditor.Controls.Add(lblTypeInfo, 0, 2)
        tblVarEditor.Controls.Add(lblDescription, 0, 3)
        tblVarEditor.Controls.Add(pnlInputWrapper, 0, 4)
        tblVarEditor.Controls.Add(lblError, 0, 5)
        tblVarEditor.Dock = System.Windows.Forms.DockStyle.Fill
        tblVarEditor.Location = New System.Drawing.Point(0, 0)
        tblVarEditor.Margin = New System.Windows.Forms.Padding(0)
        tblVarEditor.Name = "tblVarEditor"
        tblVarEditor.RowCount = 6
        tblVarEditor.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tblVarEditor.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tblVarEditor.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tblVarEditor.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tblVarEditor.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        tblVarEditor.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24.0F))
        tblVarEditor.Size = New System.Drawing.Size(644, 561)
        tblVarEditor.TabIndex = 0
        ' 
        ' lblVarName
        ' 
        lblVarName.Dock = System.Windows.Forms.DockStyle.Fill
        lblVarName.Font = New System.Drawing.Font("Segoe UI", 12.0F, Drawing.FontStyle.Bold)
        lblVarName.ForeColor = Drawing.Color.FromArgb(CByte(33), CByte(33), CByte(33))
        lblVarName.Location = New System.Drawing.Point(0, 0)
        lblVarName.Margin = New System.Windows.Forms.Padding(0, 0, 0, 4)
        lblVarName.Name = "lblVarName"
        lblVarName.Size = New System.Drawing.Size(644, 30)
        lblVarName.TabIndex = 19
        lblVarName.Text = "Selectați o variabilă"
        ' 
        ' lblConstraints
        ' 
        lblConstraints.Dock = System.Windows.Forms.DockStyle.Fill
        lblConstraints.Font = New System.Drawing.Font("Segoe UI", 8.5F)
        lblConstraints.ForeColor = Drawing.Color.FromArgb(CByte(80), CByte(80), CByte(80))
        lblConstraints.Location = New System.Drawing.Point(0, 34)
        lblConstraints.Margin = New System.Windows.Forms.Padding(0, 0, 0, 2)
        lblConstraints.Name = "lblConstraints"
        lblConstraints.Size = New System.Drawing.Size(644, 20)
        lblConstraints.TabIndex = 16
        ' 
        ' lblTypeInfo
        ' 
        lblTypeInfo.Dock = System.Windows.Forms.DockStyle.Fill
        lblTypeInfo.Font = New System.Drawing.Font("Segoe UI", 8.5F)
        lblTypeInfo.ForeColor = Drawing.Color.FromArgb(CByte(80), CByte(80), CByte(120))
        lblTypeInfo.Location = New System.Drawing.Point(0, 56)
        lblTypeInfo.Margin = New System.Windows.Forms.Padding(0, 0, 0, 2)
        lblTypeInfo.Name = "lblTypeInfo"
        lblTypeInfo.Size = New System.Drawing.Size(644, 20)
        lblTypeInfo.TabIndex = 17
        ' 
        ' lblDescription
        ' 
        lblDescription.Dock = System.Windows.Forms.DockStyle.Fill
        lblDescription.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Italic)
        lblDescription.ForeColor = Drawing.Color.FromArgb(CByte(100), CByte(100), CByte(100))
        lblDescription.Location = New System.Drawing.Point(0, 78)
        lblDescription.Margin = New System.Windows.Forms.Padding(0, 0, 0, 8)
        lblDescription.Name = "lblDescription"
        lblDescription.Size = New System.Drawing.Size(644, 60)
        lblDescription.TabIndex = 18
        ' 
        ' pnlInputWrapper
        ' 
        pnlInputWrapper.Controls.Add(txtValue)
        pnlInputWrapper.Controls.Add(mskValue)
        pnlInputWrapper.Dock = System.Windows.Forms.DockStyle.Fill
        pnlInputWrapper.Location = New System.Drawing.Point(0, 146)
        pnlInputWrapper.Margin = New System.Windows.Forms.Padding(0)
        pnlInputWrapper.Name = "pnlInputWrapper"
        pnlInputWrapper.Padding = New System.Windows.Forms.Padding(0, 4, 0, 0)
        pnlInputWrapper.Size = New System.Drawing.Size(644, 391)
        pnlInputWrapper.TabIndex = 15
        ' 
        ' txtValue
        ' 
        txtValue.Dock = System.Windows.Forms.DockStyle.Fill
        txtValue.Font = New System.Drawing.Font("Segoe UI", 10.0F)
        txtValue.Location = New System.Drawing.Point(0, 4)
        txtValue.Name = "txtValue"
        txtValue.Size = New System.Drawing.Size(644, 34)
        txtValue.TabIndex = 0
        txtValue.Visible = False
        ' 
        ' mskValue
        ' 
        mskValue.Dock = System.Windows.Forms.DockStyle.Fill
        mskValue.Font = New System.Drawing.Font("Segoe UI", 10.0F)
        mskValue.Location = New System.Drawing.Point(0, 4)
        mskValue.Name = "mskValue"
        mskValue.Size = New System.Drawing.Size(644, 34)
        mskValue.TabIndex = 1
        mskValue.Visible = False
        ' 
        ' lblError
        ' 
        lblError.Dock = System.Windows.Forms.DockStyle.Fill
        lblError.Font = New System.Drawing.Font("Segoe UI", 8.5F)
        lblError.ForeColor = Drawing.Color.FromArgb(CByte(198), CByte(40), CByte(40))
        lblError.Location = New System.Drawing.Point(0, 537)
        lblError.Margin = New System.Windows.Forms.Padding(0)
        lblError.Name = "lblError"
        lblError.Size = New System.Drawing.Size(644, 24)
        lblError.TabIndex = 14
        lblError.Visible = False
        ' 
        ' FlowLayoutPanel2
        ' 
        FlowLayoutPanel2.Controls.Add(btnLoadVariablesFromJSON)
        FlowLayoutPanel2.Controls.Add(btnSaveVariablesToJSON)
        FlowLayoutPanel2.Controls.Add(btnSave)
        FlowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        FlowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        FlowLayoutPanel2.Location = New System.Drawing.Point(0, 561)
        FlowLayoutPanel2.Margin = New System.Windows.Forms.Padding(0)
        FlowLayoutPanel2.Name = "FlowLayoutPanel2"
        FlowLayoutPanel2.Size = New System.Drawing.Size(644, 121)
        FlowLayoutPanel2.TabIndex = 1
        FlowLayoutPanel2.WrapContents = False
        ' 
        ' btnLoadVariablesFromJSON
        ' 
        btnLoadVariablesFromJSON.BackColor = Drawing.Color.FromArgb(CByte(100), CByte(181), CByte(246))
        btnLoadVariablesFromJSON.FlatAppearance.BorderSize = 0
        btnLoadVariablesFromJSON.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnLoadVariablesFromJSON.ForeColor = Drawing.Color.White
        btnLoadVariablesFromJSON.Location = New System.Drawing.Point(0, 0)
        btnLoadVariablesFromJSON.Margin = New System.Windows.Forms.Padding(0, 0, 0, 8)
        btnLoadVariablesFromJSON.Name = "btnLoadVariablesFromJSON"
        btnLoadVariablesFromJSON.Size = New System.Drawing.Size(644, 32)
        btnLoadVariablesFromJSON.TabIndex = 14
        btnLoadVariablesFromJSON.Text = "Încarcă variabilele dintr-un fișier JSON"
        btnLoadVariablesFromJSON.UseVisualStyleBackColor = False
        ' 
        ' btnSaveVariablesToJSON
        ' 
        btnSaveVariablesToJSON.BackColor = Drawing.Color.FromArgb(CByte(100), CByte(181), CByte(246))
        btnSaveVariablesToJSON.FlatAppearance.BorderSize = 0
        btnSaveVariablesToJSON.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSaveVariablesToJSON.ForeColor = Drawing.Color.White
        btnSaveVariablesToJSON.Location = New System.Drawing.Point(0, 40)
        btnSaveVariablesToJSON.Margin = New System.Windows.Forms.Padding(0, 0, 0, 8)
        btnSaveVariablesToJSON.Name = "btnSaveVariablesToJSON"
        btnSaveVariablesToJSON.Size = New System.Drawing.Size(644, 32)
        btnSaveVariablesToJSON.TabIndex = 15
        btnSaveVariablesToJSON.Text = "Salvează toate variabilele într-un fișier JSON (pentru testare)"
        btnSaveVariablesToJSON.UseVisualStyleBackColor = False
        ' 
        ' btnSave
        ' 
        btnSave.BackColor = Drawing.Color.FromArgb(CByte(25), CByte(118), CByte(210))
        btnSave.Enabled = False
        btnSave.FlatAppearance.BorderSize = 0
        btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSave.ForeColor = Drawing.Color.White
        btnSave.Location = New System.Drawing.Point(0, 80)
        btnSave.Margin = New System.Windows.Forms.Padding(0)
        btnSave.Name = "btnSave"
        btnSave.Size = New System.Drawing.Size(644, 32)
        btnSave.TabIndex = 16
        btnSave.Text = "Salvează valoarea  ↩"
        btnSave.UseVisualStyleBackColor = False
        ' 
        ' VariablesInputForm
        ' 
        BackColor = Drawing.Color.FromArgb(CByte(245), CByte(245), CByte(245))
        CancelButton = btnCancel
        ClientSize = New System.Drawing.Size(917, 754)
        Controls.Add(pnlRight)
        Controls.Add(splitter)
        Controls.Add(pnlLeft)
        Controls.Add(pnlBottom)
        Font = New System.Drawing.Font("Segoe UI", 9.5F)
        MinimumSize = New System.Drawing.Size(560, 380)
        Name = "VariablesInputForm"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Text = "Parametri workflow"
        pnlLeft.ResumeLayout(False)
        pnlBottom.ResumeLayout(False)
        pnlRight.ResumeLayout(False)
        TableLayoutPanel1.ResumeLayout(False)
        tblVarEditor.ResumeLayout(False)
        pnlInputWrapper.ResumeLayout(False)
        pnlInputWrapper.PerformLayout()
        FlowLayoutPanel2.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    ' =========================================================================
    ' Declarații controale
    ' =========================================================================
    Private pnlLeft As System.Windows.Forms.Panel
    Private lblListTitle As System.Windows.Forms.Label
    Private splitter As System.Windows.Forms.Panel
    Private pnlBottom As System.Windows.Forms.Panel

    Private WithEvents lstVars As System.Windows.Forms.ListBox
    Private WithEvents btnContinue As System.Windows.Forms.Button
    Private WithEvents btnCancel As System.Windows.Forms.Button
    Private lblAllStatus As System.Windows.Forms.Label
    Private WithEvents pnlRight As System.Windows.Forms.Panel
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents tblVarEditor As System.Windows.Forms.TableLayoutPanel
    Private WithEvents lblError As System.Windows.Forms.Label
    Private WithEvents pnlInputWrapper As System.Windows.Forms.Panel
    Private WithEvents lblConstraints As System.Windows.Forms.Label
    Private WithEvents lblTypeInfo As System.Windows.Forms.Label
    Private WithEvents lblDescription As System.Windows.Forms.Label
    Private WithEvents lblVarName As System.Windows.Forms.Label
    Friend WithEvents FlowLayoutPanel2 As System.Windows.Forms.FlowLayoutPanel
    Private WithEvents btnLoadVariablesFromJSON As System.Windows.Forms.Button
    Private WithEvents btnSaveVariablesToJSON As System.Windows.Forms.Button
    Private WithEvents btnSave As System.Windows.Forms.Button
    Private WithEvents txtValue As System.Windows.Forms.TextBox
    Private WithEvents mskValue As System.Windows.Forms.MaskedTextBox

End Class