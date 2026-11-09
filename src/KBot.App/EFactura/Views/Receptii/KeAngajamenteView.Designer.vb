<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KeAngajamenteView
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        gridAng = New Controls.KBotDataView()
        CType(gridAng, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' gridAng
        ' 
        gridAng.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridAng.BackColor = Drawing.SystemColors.Window
        gridAng.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBot.Controls.KBotColumnType.Combo
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Angajament"
        KBotDataColumn1.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "ang"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 220
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.ButtonKind = KBot.Controls.KBotButtonKind.Delete
        KBotDataColumn2.ColumnType = KBot.Controls.KBotColumnType.Button
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = ""
        KBotDataColumn2.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "opt"
        KBotDataColumn2.MinWidth = 34
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Resizable = False
        KBotDataColumn2.Width = 34
        gridAng.Columns.Add(KBotDataColumn1)
        gridAng.Columns.Add(KBotDataColumn2)
        gridAng.Dock = System.Windows.Forms.DockStyle.Fill
        gridAng.FillColumnKey = "ang"
        gridAng.Location = New System.Drawing.Point(0, 0)
        gridAng.Margin = New System.Windows.Forms.Padding(0)
        gridAng.Name = "gridAng"
        gridAng.Size = New System.Drawing.Size(480, 483)
        gridAng.TabIndex = 0
        '
        ' KeAngajamenteView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridAng)
        Margin = New System.Windows.Forms.Padding(0)
        Name = "KeAngajamenteView"
        Size = New System.Drawing.Size(480, 483)
        CType(gridAng, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents gridAng As Global.KBot.Controls.KBotDataView
End Class
