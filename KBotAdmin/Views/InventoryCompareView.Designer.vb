' SLICE-VC12-01. Inventory comparison, no import action.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class InventoryCompareView
    Inherits Global.KBot.Theming.KBotThemedUserControl

    Private Sub InitializeComponent()
        txtFile = New KBotTextBox() With {.Dock = DockStyle.Fill}
        btnBrowse = New KBotButton() With {.Text = "Alege Excel…", .Dock = DockStyle.Fill}
        cmbSheet = New KBotComboBox() With {.Dock = DockStyle.Fill}
        btnRead = New KBotButton() With {.Text = "Citește foaia", .Dock = DockStyle.Fill}
        btnTargets = New KBotButton() With {.Text = "Citește destinațiile", .Dock = DockStyle.Fill}
        cmbSubunit = New KBotComboBox() With {.Dock = DockStyle.Fill}
        cmbGestiune = New KBotComboBox() With {.Dock = DockStyle.Fill}
        cmbAccount = New KBotComboBox() With {.Dock = DockStyle.Fill}
        btnCompare = New KBotButton() With {.Text = "Compară", .Dock = DockStyle.Fill}
        btnExport = New KBotButton() With {.Text = "Salvează CSV…", .Dock = DockStyle.Fill, .Enabled = False}
        lblState = New Label() With {.Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft,
            .Text = "Alegeți Excelul și foaia, apoi baza de date și Gestiune / Subgestiune / Cont. Comparația doar citește."}
        lblMethod = New Label() With {.Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft,
            .Text = "Candidați în nomenclator, inclusiv inactivi. Preț VERCON cu TVA. Scor orientativ, nu probabilitate. Selectați un rând pentru primii 5 candidați."}
        grid = New InventoryCompareGrid() With {.Dock = DockStyle.Fill}
        candidates = New InventoryCompareGrid() With {.Dock = DockStyle.Fill}
        tlyMain = New KBotTableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 6,
            .Padding = New Padding(8), .AutoFitToTheme = False}
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 70))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 50))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 65))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 35))
        tlyFile = New KBotTableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 4, .RowCount = 1, .AutoFitToTheme = False}
        For Each k_width As Single In {45.0F, 15.0F, 25.0F, 15.0F}
            tlyFile.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, k_width))
        Next
        tlyFile.Controls.Add(txtFile, 0, 0)
        tlyFile.Controls.Add(btnBrowse, 1, 0)
        tlyFile.Controls.Add(cmbSheet, 2, 0)
        tlyFile.Controls.Add(btnRead, 3, 0)
        tlyTarget = New KBotTableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 6, .RowCount = 2, .AutoFitToTheme = False}
        For Each k_width As Single In {16.0F, 22.0F, 22.0F, 20.0F, 10.0F, 10.0F}
            tlyTarget.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, k_width))
        Next
        tlyTarget.RowStyles.Add(New RowStyle(SizeType.Absolute, 22))
        tlyTarget.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        tlyTarget.Controls.Add(New Label() With {.Text = "Gestiune", .Dock = DockStyle.Fill}, 1, 0)
        tlyTarget.Controls.Add(New Label() With {.Text = "Subgestiune (OI / MF)", .Dock = DockStyle.Fill}, 2, 0)
        tlyTarget.Controls.Add(New Label() With {.Text = "Cont", .Dock = DockStyle.Fill}, 3, 0)
        tlyTarget.Controls.Add(btnTargets, 0, 1)
        tlyTarget.Controls.Add(cmbSubunit, 1, 1)
        tlyTarget.Controls.Add(cmbGestiune, 2, 1)
        tlyTarget.Controls.Add(cmbAccount, 3, 1)
        tlyTarget.Controls.Add(btnCompare, 4, 1)
        tlyTarget.Controls.Add(btnExport, 5, 1)
        tlyMain.Controls.Add(tlyFile, 0, 0)
        tlyMain.Controls.Add(tlyTarget, 0, 1)
        tlyMain.Controls.Add(lblState, 0, 2)
        tlyMain.Controls.Add(grid, 0, 3)
        tlyMain.Controls.Add(lblMethod, 0, 4)
        tlyMain.Controls.Add(candidates, 0, 5)
        AutoScaleDimensions = New SizeF(96, 96)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "InventoryCompareView"
        Size = New Size(1200, 650)
    End Sub

    Friend WithEvents txtFile As KBotTextBox
    Friend WithEvents btnBrowse As KBotButton
    Friend WithEvents cmbSheet As KBotComboBox
    Friend WithEvents btnRead As KBotButton
    Friend WithEvents btnTargets As KBotButton
    Friend WithEvents cmbSubunit As KBotComboBox
    Friend WithEvents cmbGestiune As KBotComboBox
    Friend WithEvents cmbAccount As KBotComboBox
    Friend WithEvents btnCompare As KBotButton
    Friend WithEvents btnExport As KBotButton
    Friend WithEvents grid As InventoryCompareGrid
    Friend WithEvents candidates As InventoryCompareGrid
    Friend lblState As Label
    Friend lblMethod As Label
    Friend tlyMain As KBotTableLayoutPanel
    Friend tlyFile As KBotTableLayoutPanel
    Friend tlyTarget As KBotTableLayoutPanel
End Class
