Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteAtasamentePage
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
        gridAtasamente = New KBotDataView()
        CType(gridAtasamente, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' gridAtasamente
        '
        gridAtasamente.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridAtasamente.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Fișier"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "nume"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 360
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Tip"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "mime"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 180
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 0
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Mărime (octeți)"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "octeti"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 130
        gridAtasamente.Columns.Add(KBotDataColumn1)
        gridAtasamente.Columns.Add(KBotDataColumn2)
        gridAtasamente.Columns.Add(KBotDataColumn3)
        gridAtasamente.Dock = System.Windows.Forms.DockStyle.Fill
        gridAtasamente.FillColumnKey = "nume"
        gridAtasamente.Location = New System.Drawing.Point(3, 3)
        gridAtasamente.Margin = New System.Windows.Forms.Padding(0)
        gridAtasamente.Name = "gridAtasamente"
        gridAtasamente.ReadOnlyGrid = True
        gridAtasamente.Size = New System.Drawing.Size(751, 520)
        gridAtasamente.TabIndex = 0
        tips.SetToolTipHeader(gridAtasamente, "Fișiere atașate")
        tips.SetToolTipText(gridAtasamente, "Fișierele pe care furnizorul le-a pus în factura electronică. Dublu clic pe un fișier îl salvează.")
        '
        ' PrimiteAtasamentePage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridAtasamente)
        Name = "PrimiteAtasamentePage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        CType(gridAtasamente, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents gridAtasamente As KBotDataView
End Class
