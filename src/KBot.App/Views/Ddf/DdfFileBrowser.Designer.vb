<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DdfFileBrowser
    Inherits KBot.Theming.KBotThemedUserControl

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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New KBot.Controls.KBotDataColumn()
        grid = New Controls.KBotDataView()
        lblEmpty = New Label()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' grid — lista PDF-urilor (read-only)
        '
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.ToContent
        grid.BackColor = SystemColors.Window
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Folder"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "folder"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Nume fișier"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "name"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 320
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "CUAL"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cual"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 70
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Rev."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "rev"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 60
        ' The size arrives already formatted ("12,3 KB"), so the column stays text -- only the
        ' right alignment is needed to make the figures read down the column.
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Dimensiune"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "size"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.Width = 100
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Modificat"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "mod"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.Width = 140
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Columns.Add(KBotDataColumn6)
        grid.Dock = DockStyle.Fill
        grid.HeaderHeight = 30
        grid.Location = New Point(0, 0)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 28
        grid.ShowHeader = True
        grid.FooterVisible = False
        grid.Size = New Size(641, 460)
        grid.TabIndex = 0
        '
        ' lblEmpty — starea goală (rădăcină lipsă / niciun fișier)
        '
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        '
        ' DdfFileBrowser
        '
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(grid)
        Controls.Add(lblEmpty)
        Name = "DdfFileBrowser"
        Size = New Size(641, 460)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents lblEmpty As Label
End Class
