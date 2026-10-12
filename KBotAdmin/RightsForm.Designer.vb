' The rights dialog (ME01): the roles of ONE application (ADECHIT or VERCON) for ONE account in ONE database, one under
' the other: «Explicație» and a check box «Bifă». Click on the title of «Bifă» ticks / clears them all.
' All controls are declared HERE; coordinates are in the 96 dpi the form was authored at.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RightsForm
    Inherits Global.KBot.Theming.KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        lblInfo = New Label()
        grid = New KBotDataView()
        tlyButtons = New KBotTableLayoutPanel()
        btnClose = New KBotButton()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyButtons.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.AutoFitToTheme = False
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(lblInfo, 0, 0)
        tlyMain.Controls.Add(grid, 0, 1)
        tlyMain.Controls.Add(tlyButtons, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.Padding = New Padding(14, 12, 14, 12)
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 48F))
        tlyMain.Size = New Size(640, 520)
        tlyMain.TabIndex = 0
        '
        ' lblInfo
        '
        tips.SetToolTipHeader(lblInfo, "Contul și baza")
        tips.SetToolTipText(lblInfo, "Drepturile se dau sau se retrag pentru acest cont, în această bază.")
        lblInfo.AutoEllipsis = True
        lblInfo.Dock = DockStyle.Fill
        lblInfo.Location = New Point(14, 12)
        lblInfo.Margin = New Padding(0)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(612, 52)
        lblInfo.TabIndex = 0
        lblInfo.TextAlign = ContentAlignment.MiddleLeft
        '
        ' grid
        '
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.AutoSizeHeaderHeight = True
        grid.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Explicație"
        KBotDataColumn1.HeaderTooltip = "Ce îi permite dreptul."
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn1.Key = "expl"
        KBotDataColumn1.MinWidth = 200
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 480
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Bifă"
        KBotDataColumn2.HeaderTooltip = "Bifa dă dreptul pe loc, fără bifă dreptul se retrage. Click pe titlu: bifează sau debifează tot."
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "bifa"
        KBotDataColumn2.MinWidth = 60
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 90
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Dock = DockStyle.Fill
        grid.FillColumnKey = "expl"
        grid.HeaderHeight = 32
        grid.Location = New Point(14, 64)
        grid.Margin = New Padding(0)
        grid.MaxHeaderHeight = 48
        grid.Name = "grid"
        grid.Size = New Size(612, 396)
        grid.TabIndex = 1
        tips.SetToolTipHeader(grid, "Drepturile")
        tips.SetToolTipText(grid, "Un rând = un drept. Bifa dă dreptul pe loc, fără bifă dreptul se retrage." & vbLf & "Click pe titlul coloanei «Bifă» bifează sau debifează toate drepturile.")
        '
        ' tlyButtons
        '
        tlyButtons.AutoFitToTheme = False
        tlyButtons.ColumnCount = 2
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 140F))
        tlyButtons.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyButtons.Controls.Add(btnClose, 0, 0)
        tlyButtons.Dock = DockStyle.Fill
        tlyButtons.Location = New Point(14, 460)
        tlyButtons.Margin = New Padding(0)
        tlyButtons.Name = "tlyButtons"
        tlyButtons.RowCount = 1
        tlyButtons.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyButtons.Size = New Size(612, 48)
        tlyButtons.TabIndex = 2
        '
        ' btnClose
        '
        tips.SetToolTipHeader(btnClose, "Închide")
        tips.SetToolTipText(btnClose, "Închide fereastra. Drepturile bifate au fost deja scrise pe server.")
        btnClose.DialogResult = DialogResult.Cancel
        btnClose.Dock = DockStyle.Fill
        btnClose.Location = New Point(0, 0)
        btnClose.Margin = New Padding(0, 6, 6, 2)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(134, 40)
        btnClose.TabIndex = 0
        btnClose.Text = "Închide"
        '
        ' RightsForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnClose
        ClientSize = New Size(640, 520)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "RightsForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Drepturi"
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyButtons.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents lblInfo As Label
    Friend WithEvents grid As KBotDataView
    Friend WithEvents tlyButtons As KBotTableLayoutPanel
    Friend WithEvents btnClose As KBotButton
End Class
