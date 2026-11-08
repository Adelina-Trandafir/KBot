<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VerconReportsView
    Inherits Global.KBot.Theming.KBotThemedUserControl

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        lblUnitName = New Label()
        lblUnitFiscal = New Label()
        lblUnitAddress = New Label()
        lblUnitManager = New Label()
        lblUnitHeader = New Label()
        lblUnitFooter = New Label()
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        btnList = New KBotButton()
        cmbDefinition = New KBotComboBox()
        btnRead = New KBotButton()
        btnSave = New KBotButton()
        btnDesigner = New KBotButton()
        lblHelp = New Label()
        pnlEditor = New Panel()
        txtSql = New KBotSqlBox()
        pnlUnit = New KBotTableLayoutPanel()
        txtName = New KBotTextBox()
        txtFiscal = New KBotTextBox()
        txtAddress = New KBotTextBox()
        txtManager = New KBotTextBox()
        txtHeader = New KBotTextBox()
        txtFooter = New KBotTextBox()
        lblState = New Label()
        tlyMain.SuspendLayout()
        tlyBar.SuspendLayout()
        pnlEditor.SuspendLayout()
        pnlUnit.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblUnitName
        ' 
        lblUnitName.Dock = DockStyle.Fill
        lblUnitName.Location = New Point(4, 0)
        lblUnitName.Margin = New Padding(4, 0, 4, 0)
        lblUnitName.Name = "lblUnitName"
        lblUnitName.Size = New Size(217, 63)
        lblUnitName.TabIndex = 0
        lblUnitName.Text = "Denumire unitate"
        lblUnitName.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblUnitFiscal
        ' 
        lblUnitFiscal.Dock = DockStyle.Fill
        lblUnitFiscal.Location = New Point(4, 63)
        lblUnitFiscal.Margin = New Padding(4, 0, 4, 0)
        lblUnitFiscal.Name = "lblUnitFiscal"
        lblUnitFiscal.Size = New Size(217, 63)
        lblUnitFiscal.TabIndex = 2
        lblUnitFiscal.Text = "CUI"
        lblUnitFiscal.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblUnitAddress
        ' 
        lblUnitAddress.Dock = DockStyle.Fill
        lblUnitAddress.Location = New Point(4, 126)
        lblUnitAddress.Margin = New Padding(4, 0, 4, 0)
        lblUnitAddress.Name = "lblUnitAddress"
        lblUnitAddress.Size = New Size(217, 63)
        lblUnitAddress.TabIndex = 4
        lblUnitAddress.Text = "Adresă"
        lblUnitAddress.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblUnitManager
        ' 
        lblUnitManager.Dock = DockStyle.Fill
        lblUnitManager.Location = New Point(4, 189)
        lblUnitManager.Margin = New Padding(4, 0, 4, 0)
        lblUnitManager.Name = "lblUnitManager"
        lblUnitManager.Size = New Size(217, 63)
        lblUnitManager.TabIndex = 6
        lblUnitManager.Text = "Gestionar"
        lblUnitManager.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblUnitHeader
        ' 
        lblUnitHeader.Dock = DockStyle.Fill
        lblUnitHeader.Location = New Point(4, 252)
        lblUnitHeader.Margin = New Padding(4, 0, 4, 0)
        lblUnitHeader.Name = "lblUnitHeader"
        lblUnitHeader.Size = New Size(217, 201)
        lblUnitHeader.TabIndex = 8
        lblUnitHeader.Text = "Antet"
        lblUnitHeader.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblUnitFooter
        ' 
        lblUnitFooter.Dock = DockStyle.Fill
        lblUnitFooter.Location = New Point(4, 453)
        lblUnitFooter.Margin = New Padding(4, 0, 4, 0)
        lblUnitFooter.Name = "lblUnitFooter"
        lblUnitFooter.Size = New Size(217, 201)
        lblUnitFooter.TabIndex = 10
        lblUnitFooter.Text = "Subsol"
        lblUnitFooter.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tlyMain
        ' 
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(lblHelp, 0, 1)
        tlyMain.Controls.Add(pnlEditor, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(4)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(8)
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 66.0F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 93.0F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 63.0F))
        tlyMain.Size = New Size(1500, 900)
        tlyMain.TabIndex = 0
        ' 
        ' tlyBar
        ' 
        tlyBar.AutoFitToTheme = False
        tlyBar.ColumnCount = 5
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 202.0F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150.0F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180.0F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 210.0F))
        tlyBar.Controls.Add(btnList, 0, 0)
        tlyBar.Controls.Add(cmbDefinition, 1, 0)
        tlyBar.Controls.Add(btnRead, 2, 0)
        tlyBar.Controls.Add(btnSave, 3, 0)
        tlyBar.Controls.Add(btnDesigner, 4, 0)
        tlyBar.Dock = DockStyle.Fill
        tlyBar.Location = New Point(12, 12)
        tlyBar.Margin = New Padding(4)
        tlyBar.Name = "tlyBar"
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyBar.Size = New Size(1476, 58)
        tlyBar.TabIndex = 0
        ' 
        ' btnList
        ' 
        btnList.AccessibleRole = AccessibleRole.PushButton
        btnList.Dock = DockStyle.Fill
        btnList.Location = New Point(4, 4)
        btnList.Margin = New Padding(4)
        btnList.Name = "btnList"
        btnList.Size = New Size(194, 50)
        btnList.TabIndex = 0
        btnList.Text = "Citește lista"
        ' 
        ' cmbDefinition
        ' 
        cmbDefinition.Dock = DockStyle.Fill
        cmbDefinition.Location = New Point(206, 4)
        cmbDefinition.Margin = New Padding(4)
        cmbDefinition.Name = "cmbDefinition"
        cmbDefinition.Size = New Size(726, 37)
        cmbDefinition.TabIndex = 1
        ' 
        ' btnRead
        ' 
        btnRead.AccessibleRole = AccessibleRole.PushButton
        btnRead.Dock = DockStyle.Fill
        btnRead.Location = New Point(940, 4)
        btnRead.Margin = New Padding(4)
        btnRead.Name = "btnRead"
        btnRead.Size = New Size(142, 50)
        btnRead.TabIndex = 2
        btnRead.Text = "Deschide"
        ' 
        ' btnSave
        ' 
        btnSave.AccessibleRole = AccessibleRole.PushButton
        btnSave.Dock = DockStyle.Fill
        btnSave.Enabled = False
        btnSave.Location = New Point(1090, 4)
        btnSave.Margin = New Padding(4)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(172, 50)
        btnSave.TabIndex = 3
        btnSave.Text = "Salvează"
        ' 
        ' btnDesigner
        ' 
        btnDesigner.AccessibleRole = AccessibleRole.PushButton
        btnDesigner.Dock = DockStyle.Fill
        btnDesigner.Enabled = False
        btnDesigner.Location = New Point(1270, 4)
        btnDesigner.Margin = New Padding(4)
        btnDesigner.Name = "btnDesigner"
        btnDesigner.Size = New Size(202, 50)
        btnDesigner.TabIndex = 4
        btnDesigner.Text = "Editor vizual"
        ' 
        ' lblHelp
        ' 
        lblHelp.Dock = DockStyle.Fill
        lblHelp.Location = New Point(12, 74)
        lblHelp.Margin = New Padding(4, 0, 4, 0)
        lblHelp.Name = "lblHelp"
        lblHelp.Size = New Size(1476, 93)
        lblHelp.TabIndex = 1
        lblHelp.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlEditor
        ' 
        pnlEditor.Controls.Add(txtSql)
        pnlEditor.Controls.Add(pnlUnit)
        pnlEditor.Dock = DockStyle.Fill
        pnlEditor.Location = New Point(12, 171)
        pnlEditor.Margin = New Padding(4)
        pnlEditor.Name = "pnlEditor"
        pnlEditor.Size = New Size(1476, 654)
        pnlEditor.TabIndex = 2
        ' 
        ' txtSql
        ' 
        txtSql.AcceptsTab = True
        txtSql.BorderStyle = BorderStyle.None
        txtSql.DetectUrls = False
        txtSql.Dock = DockStyle.Fill
        txtSql.HideSelection = False
        txtSql.Location = New Point(0, 0)
        txtSql.Margin = New Padding(4)
        txtSql.MaxLength = 100000
        txtSql.Name = "txtSql"
        txtSql.ScrollBars = RichTextBoxScrollBars.ForcedBoth
        txtSql.Size = New Size(1476, 654)
        txtSql.TabIndex = 0
        txtSql.Text = ""
        txtSql.WordWrap = False
        ' 
        ' pnlUnit
        ' 
        pnlUnit.AutoFitToTheme = False
        pnlUnit.ColumnCount = 2
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 225.0F))
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        pnlUnit.Controls.Add(lblUnitName, 0, 0)
        pnlUnit.Controls.Add(txtName, 1, 0)
        pnlUnit.Controls.Add(lblUnitFiscal, 0, 1)
        pnlUnit.Controls.Add(txtFiscal, 1, 1)
        pnlUnit.Controls.Add(lblUnitAddress, 0, 2)
        pnlUnit.Controls.Add(txtAddress, 1, 2)
        pnlUnit.Controls.Add(lblUnitManager, 0, 3)
        pnlUnit.Controls.Add(txtManager, 1, 3)
        pnlUnit.Controls.Add(lblUnitHeader, 0, 4)
        pnlUnit.Controls.Add(txtHeader, 1, 4)
        pnlUnit.Controls.Add(lblUnitFooter, 0, 5)
        pnlUnit.Controls.Add(txtFooter, 1, 5)
        pnlUnit.Dock = DockStyle.Fill
        pnlUnit.Location = New Point(0, 0)
        pnlUnit.Margin = New Padding(4)
        pnlUnit.Name = "pnlUnit"
        pnlUnit.RowCount = 6
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 63.0F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 63.0F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 63.0F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 63.0F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        pnlUnit.Size = New Size(1476, 654)
        pnlUnit.TabIndex = 1
        pnlUnit.Visible = False
        ' 
        ' txtName
        ' 
        txtName.Dock = DockStyle.Fill
        txtName.Location = New Point(229, 4)
        txtName.Margin = New Padding(4)
        txtName.Name = "txtName"
        txtName.Size = New Size(1243, 55)
        txtName.TabIndex = 1
        ' 
        ' txtFiscal
        ' 
        txtFiscal.Dock = DockStyle.Fill
        txtFiscal.Location = New Point(229, 67)
        txtFiscal.Margin = New Padding(4)
        txtFiscal.Name = "txtFiscal"
        txtFiscal.Size = New Size(1243, 55)
        txtFiscal.TabIndex = 3
        ' 
        ' txtAddress
        ' 
        txtAddress.Dock = DockStyle.Fill
        txtAddress.Location = New Point(229, 130)
        txtAddress.Margin = New Padding(4)
        txtAddress.Name = "txtAddress"
        txtAddress.Size = New Size(1243, 55)
        txtAddress.TabIndex = 5
        ' 
        ' txtManager
        ' 
        txtManager.Dock = DockStyle.Fill
        txtManager.Location = New Point(229, 193)
        txtManager.Margin = New Padding(4)
        txtManager.Name = "txtManager"
        txtManager.Size = New Size(1243, 55)
        txtManager.TabIndex = 7
        ' 
        ' txtHeader
        ' 
        txtHeader.Dock = DockStyle.Fill
        txtHeader.Location = New Point(229, 256)
        txtHeader.Margin = New Padding(4)
        txtHeader.Name = "txtHeader"
        txtHeader.Size = New Size(1243, 193)
        txtHeader.TabIndex = 9
        ' 
        ' txtFooter
        ' 
        txtFooter.Dock = DockStyle.Fill
        txtFooter.Location = New Point(229, 457)
        txtFooter.Margin = New Padding(4)
        txtFooter.Name = "txtFooter"
        txtFooter.Size = New Size(1243, 193)
        txtFooter.TabIndex = 11
        ' 
        ' lblState
        ' 
        lblState.Dock = DockStyle.Fill
        lblState.Location = New Point(12, 829)
        lblState.Margin = New Padding(4, 0, 4, 0)
        lblState.Name = "lblState"
        lblState.Size = New Size(1476, 63)
        lblState.TabIndex = 3
        lblState.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' VerconReportsView
        ' 
        AutoScaleDimensions = New SizeF(144.0F, 144.0F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Margin = New Padding(4)
        Name = "VerconReportsView"
        Size = New Size(1500, 900)
        tlyMain.ResumeLayout(False)
        tlyBar.ResumeLayout(False)
        pnlEditor.ResumeLayout(False)
        pnlUnit.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBar As KBotTableLayoutPanel
    Friend WithEvents cmbDefinition As KBotComboBox
    Friend WithEvents btnList As KBotButton
    Friend WithEvents btnRead As KBotButton
    Friend WithEvents btnSave As KBotButton
    Friend WithEvents btnDesigner As KBotButton
    Friend WithEvents lblHelp As Label
    Friend WithEvents lblState As Label
    Friend WithEvents pnlEditor As Panel
    Friend WithEvents pnlUnit As KBotTableLayoutPanel
    Friend WithEvents txtSql As KBotSqlBox
    Friend WithEvents txtName As KBotTextBox
    Friend WithEvents txtFiscal As KBotTextBox
    Friend WithEvents txtAddress As KBotTextBox
    Friend WithEvents txtManager As KBotTextBox
    Friend WithEvents txtHeader As KBotTextBox
    Friend WithEvents txtFooter As KBotTextBox
    Friend WithEvents lblUnitName As Label
    Friend WithEvents lblUnitFiscal As Label
    Friend WithEvents lblUnitAddress As Label
    Friend WithEvents lblUnitManager As Label
    Friend WithEvents lblUnitHeader As Label
    Friend WithEvents lblUnitFooter As Label
End Class
