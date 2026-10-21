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
        components = New ComponentModel.Container()
        tlyMain = New KBotTableLayoutPanel()
        tlyBar = New KBotTableLayoutPanel()
        cmbDefinition = New KBotComboBox()
        btnList = New KBotButton()
        btnRead = New KBotButton()
        btnSave = New KBotButton()
        btnDesigner = New KBotButton()
        lblHelp = New Label()
        lblState = New Label()
        pnlEditor = New Panel()
        pnlUnit = New KBotTableLayoutPanel()
        txtSql = New KBotSqlBox()
        txtName = New KBotTextBox()
        txtFiscal = New KBotTextBox()
        txtAddress = New KBotTextBox()
        txtManager = New KBotTextBox()
        txtHeader = New KBotTextBox()
        txtFooter = New KBotTextBox()
        tlyMain.Dock = DockStyle.Fill
        tlyMain.AutoFitToTheme = False
        tlyMain.Padding = New Padding(8)
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 62F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyBar.AutoFitToTheme = False
        tlyBar.Dock = DockStyle.Fill
        tlyBar.ColumnCount = 5
        tlyBar.RowCount = 1
        tlyBar.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 135F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyBar.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        btnList.Text = "Citește lista"
        btnRead.Text = "Deschide"
        btnSave.Text = "Salvează"
        btnSave.Enabled = False
        btnDesigner.Text = "Editor vizual"
        btnDesigner.Enabled = False
        For Each k_control As Control In New Control() {btnList, cmbDefinition, btnRead, btnSave, btnDesigner}
            k_control.Dock = DockStyle.Fill
            k_control.Margin = New Padding(3)
        Next
        tlyBar.Controls.Add(btnList, 0, 0)
        tlyBar.Controls.Add(cmbDefinition, 1, 0)
        tlyBar.Controls.Add(btnRead, 2, 0)
        tlyBar.Controls.Add(btnSave, 3, 0)
        tlyBar.Controls.Add(btnDesigner, 4, 0)
        lblHelp.Dock = DockStyle.Fill
        lblState.Dock = DockStyle.Fill
        lblHelp.TextAlign = ContentAlignment.MiddleLeft
        lblState.TextAlign = ContentAlignment.MiddleLeft
        pnlEditor.Dock = DockStyle.Fill
        txtSql.Dock = DockStyle.Fill
        txtSql.MaxLength = 100000
        pnlUnit.AutoFitToTheme = False
        pnlUnit.Dock = DockStyle.Fill
        pnlUnit.ColumnCount = 2
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150F))
        pnlUnit.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        pnlUnit.RowCount = 6
        For k_index As Integer = 0 To 3
            pnlUnit.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        Next
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        pnlUnit.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        Dim k_labels = New String() {"Denumire unitate", "CUI", "Adresă", "Gestionar", "Antet", "Subsol"}
        Dim k_inputs = New KBotTextBox() {txtName, txtFiscal, txtAddress, txtManager, txtHeader, txtFooter}
        For k_index As Integer = 0 To 5
            Dim k_label As New Label() With {.Text = k_labels(k_index), .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft}
            k_inputs(k_index).Dock = DockStyle.Fill
            pnlUnit.Controls.Add(k_label, 0, k_index)
            pnlUnit.Controls.Add(k_inputs(k_index), 1, k_index)
        Next
        txtHeader.Multiline = True
        txtFooter.Multiline = True
        pnlUnit.Visible = False
        pnlEditor.Controls.Add(txtSql)
        pnlEditor.Controls.Add(pnlUnit)
        tlyMain.Controls.Add(tlyBar, 0, 0)
        tlyMain.Controls.Add(lblHelp, 0, 1)
        tlyMain.Controls.Add(pnlEditor, 0, 2)
        tlyMain.Controls.Add(lblState, 0, 3)
        Controls.Add(tlyMain)
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        Name = "VerconReportsView"
        Size = New Size(1000, 600)
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
End Class
