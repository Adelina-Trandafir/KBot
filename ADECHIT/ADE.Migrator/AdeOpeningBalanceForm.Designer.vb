<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdeOpeningBalanceForm
    Inherits KBot.Theming.KBotThemedForm

    Private components As System.ComponentModel.IContainer
    Friend WithEvents tlpBalances As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents actions As KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblContext As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnPreview As KBot.Controls.KBotButton
    Friend WithEvents btnApply As KBot.Controls.KBotButton
    Friend WithEvents chkUnresolved As KBot.Controls.KBotCheckBox
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents busyBar As KBot.Controls.KBotBusyBar

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tlpBalances = New KBot.Controls.KBotTableLayoutPanel()
        actions = New KBot.Controls.KBotTableLayoutPanel()
        lblContext = New Label()
        lblStatus = New Label()
        btnPreview = New KBot.Controls.KBotButton()
        btnApply = New KBot.Controls.KBotButton()
        chkUnresolved = New KBot.Controls.KBotCheckBox()
        grid = New KBot.Controls.KBotDataView()
        busyBar = New KBot.Controls.KBotBusyBar()
        Dim colName As New KBot.Controls.KBotDataColumn()
        Dim colInitial As New KBot.Controls.KBotDataColumn()
        Dim colCombined As New KBot.Controls.KBotDataColumn()
        Dim colOld As New KBot.Controls.KBotDataColumn()
        Dim colNew As New KBot.Controls.KBotDataColumn()
        Dim colDifference As New KBot.Controls.KBotDataColumn()
        Dim colOrigin As New KBot.Controls.KBotDataColumn()
        Dim colState As New KBot.Controls.KBotDataColumn()
        CType(grid, System.ComponentModel.ISupportInitialize).BeginInit()
        tlpBalances.SuspendLayout()
        actions.SuspendLayout()
        SuspendLayout()
        tlpBalances.ColumnCount = 1
        tlpBalances.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        tlpBalances.RowCount = 5
        tlpBalances.RowStyles.Add(New RowStyle(SizeType.Absolute, 66.0!))
        tlpBalances.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        tlpBalances.RowStyles.Add(New RowStyle(SizeType.Absolute, 46.0!))
        tlpBalances.RowStyles.Add(New RowStyle(SizeType.Absolute, 54.0!))
        tlpBalances.RowStyles.Add(New RowStyle(SizeType.Absolute, 5.0!))
        tlpBalances.Controls.Add(lblContext, 0, 0)
        tlpBalances.Controls.Add(grid, 0, 1)
        tlpBalances.Controls.Add(actions, 0, 2)
        tlpBalances.Controls.Add(lblStatus, 0, 3)
        tlpBalances.Controls.Add(busyBar, 0, 4)
        tlpBalances.Dock = DockStyle.Fill
        tlpBalances.Padding = New Padding(10)
        tlpBalances.Name = "tlpBalances"
        lblContext.Dock = DockStyle.Fill
        lblContext.Name = "lblContext"
        lblContext.Text = "Solduri reportate separat pentru copiii din 2020 care continuă în 2021."
        lblContext.TextAlign = ContentAlignment.MiddleLeft
        actions.ColumnCount = 3
        actions.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        actions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180.0!))
        actions.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220.0!))
        actions.RowCount = 1
        actions.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        actions.Controls.Add(chkUnresolved, 0, 0)
        actions.Controls.Add(btnPreview, 1, 0)
        actions.Controls.Add(btnApply, 2, 0)
        actions.Dock = DockStyle.Fill
        actions.Name = "actions"
        chkUnresolved.AutoSize = True
        chkUnresolved.Checked = True
        chkUnresolved.CheckState = CheckState.Checked
        chkUnresolved.Dock = DockStyle.Fill
        chkUnresolved.Name = "chkUnresolved"
        chkUnresolved.TabIndex = 0
        chkUnresolved.Text = "Doar copiii nerezolvați"
        btnPreview.TabIndex = 1
        btnPreview.Dock = DockStyle.Fill
        btnPreview.Name = "btnPreview"
        btnPreview.Text = "Previzualizează"
        btnApply.Dock = DockStyle.Fill
        btnApply.Enabled = False
        btnApply.TabIndex = 2
        btnApply.Name = "btnApply"
        btnApply.Text = "Salvează reportările"
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Name = "lblStatus"
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        busyBar.Dock = DockStyle.Fill
        busyBar.Name = "busyBar"
        colName.Key = "name"
        colName.HeaderText = "Copil"
        colName.Width = 230
        colInitial.Key = "initial"
        colInitial.HeaderText = "SI existent (păstrat)"
        colInitial.ValueType = KBot.Controls.KBotValueType.Number
        colInitial.TextAlign = ContentAlignment.MiddleRight
        colInitial.Width = 150
        colCombined.Key = "combined"
        colCombined.HeaderText = "SI + reportat"
        colCombined.ValueType = KBot.Controls.KBotValueType.Number
        colCombined.TextAlign = ContentAlignment.MiddleRight
        colCombined.Width = 120
        colOld.Key = "old"
        colOld.HeaderText = "Reportat existent"
        colOld.ValueType = KBot.Controls.KBotValueType.Number
        colOld.TextAlign = ContentAlignment.MiddleRight
        colOld.Width = 140
        colNew.Key = "new"
        colNew.HeaderText = "Reportat propus"
        colNew.ValueType = KBot.Controls.KBotValueType.Number
        colNew.TextAlign = ContentAlignment.MiddleRight
        colNew.Width = 140
        colDifference.Key = "difference"
        colDifference.HeaderText = "Diferență"
        colDifference.ValueType = KBot.Controls.KBotValueType.Number
        colDifference.TextAlign = ContentAlignment.MiddleRight
        colDifference.Width = 100
        colOrigin.Key = "origin"
        colOrigin.HeaderText = "Originea soldului"
        colOrigin.Width = 420
        colState.Key = "state"
        colState.HeaderText = "Stare / motiv"
        colState.Width = 350
        grid.Columns.Add(colName)
        grid.Columns.Add(colInitial)
        grid.Columns.Add(colOld)
        grid.Columns.Add(colNew)
        grid.Columns.Add(colDifference)
        grid.Columns.Add(colCombined)
        grid.Columns.Add(colOrigin)
        grid.Columns.Add(colState)
        grid.ReadOnlyGrid = True
        grid.Dock = DockStyle.Fill
        grid.Name = "grid"
        grid.RowHeight = 28
        AutoScaleDimensions = New SizeF(96.0!, 96.0!)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1180, 650)
        MinimumSize = New Size(950, 500)
        Controls.Add(tlpBalances)
        Name = "AdeOpeningBalanceForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "ADE.Migrator — solduri reportate din anii anteriori"
        CType(grid, System.ComponentModel.ISupportInitialize).EndInit()
        actions.ResumeLayout(False)
        tlpBalances.ResumeLayout(False)
        ResumeLayout(False)
    End Sub
End Class
