<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class VerconReportDesignerForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        tlyBody = New KBotTableLayoutPanel()
        tlyProperties = New KBotTableLayoutPanel()
        tlyButtons = New KBotTableLayoutPanel()
        pnlCanvas = New Panel()
        lblHelp = New Label()
        cmbSection = New KBotComboBox()
        cmbItem = New KBotComboBox()
        txtCaption = New KBotTextBox()
        cmbBinding = New KBotComboBox()
        cmbType = New KBotComboBox()
        cmbBand = New KBotComboBox()
        numFont = New NumericUpDown()
        numWidth = New NumericUpDown()
        chkLandscape = New KBotCheckBox()
        chkBold = New KBotCheckBox()
        btnAddField = New KBotButton()
        btnAddText = New KBotButton()
        btnDelete = New KBotButton()
        btnLeft = New KBotButton()
        btnRight = New KBotButton()
        btnOk = New KBotButton()
        btnCancel = New KBotButton()
        tlyMain.Dock = DockStyle.Fill
        tlyMain.AutoFitToTheme = False
        tlyMain.Padding = New Padding(12)
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 62F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 46F))
        lblHelp.Dock = DockStyle.Fill
        lblHelp.Text = "Selectați un element din pagină. Trageți textele pentru poziționare și marginea dreaptă pentru lățime. Coloanele se mută cu ← / →. Previzualizarea arată macheta; datele reale apar în PDF. Lățimile Excel se citesc separat din RAPOARTE_EXCEL_COLOANE."
        tlyBody.Dock = DockStyle.Fill
        tlyBody.AutoFitToTheme = False
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 330F))
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        pnlCanvas.Dock = DockStyle.Fill
        tlyProperties.Dock = DockStyle.Fill
        tlyProperties.AutoFitToTheme = False
        tlyProperties.Padding = New Padding(8)
        tlyProperties.ColumnCount = 2
        tlyProperties.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 98F))
        tlyProperties.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyProperties.RowCount = 15
        For k_index As Integer = 0 To 13
            tlyProperties.RowStyles.Add(New RowStyle(SizeType.Absolute, If(k_index = 2, 70F, 34F)))
        Next
        tlyProperties.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        Dim k_labels = New String() {"Secțiune", "Element", "Text / etichetă", "Câmp", "Format", "Bandă", "Font", "Lățime"}
        Dim k_inputs = New Control() {cmbSection, cmbItem, txtCaption, cmbBinding, cmbType, cmbBand, numFont, numWidth}
        For k_index As Integer = 0 To k_inputs.Length - 1
            Dim k_label As New Label() With {.Text = k_labels(k_index), .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft}
            k_inputs(k_index).Dock = DockStyle.Fill
            tlyProperties.Controls.Add(k_label, 0, k_index)
            tlyProperties.Controls.Add(k_inputs(k_index), 1, k_index)
        Next
        txtCaption.Multiline = True
        cmbBinding.Editable = True
        cmbBinding.LimitToList = False
        cmbBinding.FindAsYouType = True
        cmbType.Items.AddRange(New Object() {"text", "decimal", "int", "date"})
        cmbBand.Items.AddRange(New Object() {"header", "footer", "before", "after"})
        numFont.Minimum = 5D
        numFont.Maximum = 36D
        numFont.DecimalPlaces = 1
        numFont.Increment = 0.5D
        numWidth.Minimum = 0.1D
        numWidth.Maximum = 20D
        numWidth.DecimalPlaces = 2
        numWidth.Increment = 0.05D
        chkLandscape.Text = "Pagină landscape"
        chkBold.Text = "Text îngroșat"
        btnAddField.Text = "Adaugă câmp"
        btnAddText.Text = "Adaugă text"
        btnDelete.Text = "Șterge element"
        btnLeft.Text = "←"
        btnRight.Text = "→"
        For Each k_control As Control In New Control() {chkLandscape, chkBold, btnAddField, btnAddText, btnDelete, btnLeft, btnRight}
            k_control.Dock = DockStyle.Fill
        Next
        tlyProperties.Controls.Add(chkLandscape, 0, 8)
        tlyProperties.SetColumnSpan(chkLandscape, 2)
        tlyProperties.Controls.Add(chkBold, 0, 9)
        tlyProperties.SetColumnSpan(chkBold, 2)
        tlyProperties.Controls.Add(btnAddField, 0, 10)
        tlyProperties.SetColumnSpan(btnAddField, 2)
        tlyProperties.Controls.Add(btnAddText, 0, 11)
        tlyProperties.SetColumnSpan(btnAddText, 2)
        tlyProperties.Controls.Add(btnDelete, 0, 12)
        tlyProperties.SetColumnSpan(btnDelete, 2)
        tlyProperties.Controls.Add(btnLeft, 0, 13)
        tlyProperties.Controls.Add(btnRight, 1, 13)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 3
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        btnCancel.Text = "Renunță"
        btnCancel.DialogResult = DialogResult.Cancel
        btnOk.Text = "Aplică în ciornă"
        btnCancel.Dock = DockStyle.Fill
        btnOk.Dock = DockStyle.Fill
        tlyButtons.Controls.Add(btnCancel, 1, 0)
        tlyButtons.Controls.Add(btnOk, 2, 0)
        tlyBody.Controls.Add(pnlCanvas, 0, 0)
        tlyBody.Controls.Add(tlyProperties, 1, 0)
        tlyMain.Controls.Add(lblHelp, 0, 0)
        tlyMain.Controls.Add(tlyBody, 0, 1)
        tlyMain.Controls.Add(tlyButtons, 0, 2)
        Controls.Add(tlyMain)
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1100, 760)
        MinimumSize = New Size(950, 730)
        StartPosition = FormStartPosition.CenterParent
        Text = "Editor vizual rapoarte VERCON"
        CancelButton = btnCancel
    End Sub

    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tlyProperties As KBotTableLayoutPanel
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents pnlCanvas As Panel
    Friend WithEvents lblHelp As Label
    Friend WithEvents cmbSection As KBotComboBox
    Friend WithEvents cmbItem As KBotComboBox
    Friend WithEvents txtCaption As KBotTextBox
    Friend WithEvents cmbBinding As KBotComboBox
    Friend WithEvents cmbType As KBotComboBox
    Friend WithEvents cmbBand As KBotComboBox
    Friend WithEvents numFont As NumericUpDown
    Friend WithEvents numWidth As NumericUpDown
    Friend WithEvents chkLandscape As KBotCheckBox
    Friend WithEvents chkBold As KBotCheckBox
    Friend WithEvents btnAddField As KBotButton
    Friend WithEvents btnAddText As KBotButton
    Friend WithEvents btnDelete As KBotButton
    Friend WithEvents btnLeft As KBotButton
    Friend WithEvents btnRight As KBotButton
    Friend WithEvents btnOk As KBotButton
    Friend WithEvents btnCancel As KBotButton
End Class
