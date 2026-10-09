Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteMesajePage
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
        components = New System.ComponentModel.Container()
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        gridMesaje = New KBotDataView()
        CType(gridMesaje, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' gridMesaje
        '
        gridMesaje.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridMesaje.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Fel"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "fel"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 120
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Data"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "data"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 100
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Text"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "text"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 480
        gridMesaje.Columns.Add(KBotDataColumn1)
        gridMesaje.Columns.Add(KBotDataColumn2)
        gridMesaje.Columns.Add(KBotDataColumn3)
        gridMesaje.Dock = System.Windows.Forms.DockStyle.Fill
        gridMesaje.FillColumnKey = "text"
        gridMesaje.Location = New System.Drawing.Point(3, 3)
        gridMesaje.Margin = New System.Windows.Forms.Padding(0)
        gridMesaje.Name = "gridMesaje"
        gridMesaje.ReadOnlyGrid = True
        gridMesaje.Size = New System.Drawing.Size(751, 520)
        gridMesaje.TabIndex = 0
        tips.SetToolTipHeader(gridMesaje, "Mesaje și note")
        tips.SetToolTipText(gridMesaje, "Notele din factura electronică și mesajele schimbate despre ea.")
        '
        ' PrimiteMesajePage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridMesaje)
        Name = "PrimiteMesajePage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        CType(gridMesaje, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents gridMesaje As KBotDataView
End Class
