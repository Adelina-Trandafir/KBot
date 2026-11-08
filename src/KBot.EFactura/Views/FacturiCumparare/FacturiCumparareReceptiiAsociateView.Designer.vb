<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FacturiCumparareReceptiiAsociateView
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
        Dim KBotDataColumn6 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn7 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn8 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        Dim KBotDataColumn9 As Global.KBot.Controls.KBotDataColumn = New Global.KBot.Controls.KBotDataColumn()
        gridReceptii = New Global.KBot.Controls.KBotDataView()
        CType(gridReceptii, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' gridReceptii
        ' 
        gridReceptii.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridReceptii.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = "dd.MM.yyyy"
        KBotDataColumn6.ValueType = Global.KBot.Controls.KBotValueType.DateTime
        KBotDataColumn6.HeaderText = "Data"
        KBotDataColumn6.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "data"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 84
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Descriere"
        KBotDataColumn7.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "descriere"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.Width = 200
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.DecimalPlaces = 2
        KBotDataColumn8.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Valoare"
        KBotDataColumn8.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "total"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn8.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn9.ButtonKind = KBot.Controls.KBotButtonKind.Delete
        KBotDataColumn9.ColumnType = KBot.Controls.KBotColumnType.Button
        KBotDataColumn9.HeaderText = ""
        KBotDataColumn9.Key = "elimina"
        KBotDataColumn9.MinWidth = 34
        KBotDataColumn9.Resizable = False
        KBotDataColumn9.Width = 34
        gridReceptii.Columns.Add(KBotDataColumn6)
        gridReceptii.Columns.Add(KBotDataColumn7)
        gridReceptii.Columns.Add(KBotDataColumn8)
        gridReceptii.Columns.Add(KBotDataColumn9)
        gridReceptii.Dock = System.Windows.Forms.DockStyle.Fill
        gridReceptii.FillColumnKey = "descriere"
        gridReceptii.Location = New System.Drawing.Point(0, 0)
        gridReceptii.Margin = New System.Windows.Forms.Padding(0)
        gridReceptii.Name = "gridReceptii"
        gridReceptii.ReadOnlyGrid = True
        gridReceptii.Size = New System.Drawing.Size(540, 471)
        gridReceptii.TabIndex = 0
        Controls.Add(gridReceptii)
        '
        ' FacturiCumparareReceptiiAsociateView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Margin = New System.Windows.Forms.Padding(0)
        Name = "FacturiCumparareReceptiiAsociateView"
        Padding = New System.Windows.Forms.Padding(0)
        Size = New System.Drawing.Size(540, 471)
        CType(gridReceptii, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents gridReceptii As Global.KBot.Controls.KBotDataView
End Class
