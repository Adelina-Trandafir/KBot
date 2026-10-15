Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteObservatiiPage
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
        tips = New KBotToolTip(components)
        gridObservatii = New KBotDataView()
        rtbObservatie = New KBotRichTextEditor()
        CType(gridObservatii, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' gridObservatii
        '
        gridObservatii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridObservatii.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Observație"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "text"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 690
        gridObservatii.Columns.Add(KBotDataColumn1)
        gridObservatii.Dock = System.Windows.Forms.DockStyle.Fill
        gridObservatii.FillColumnKey = "text"
        gridObservatii.Location = New System.Drawing.Point(3, 3)
        gridObservatii.Margin = New System.Windows.Forms.Padding(0)
        gridObservatii.Name = "gridObservatii"
        gridObservatii.ReadOnlyGrid = True
        gridObservatii.Size = New System.Drawing.Size(751, 350)
        gridObservatii.TabIndex = 0
        tips.SetToolTipHeader(gridObservatii, "Observații")
        tips.SetToolTipText(gridObservatii, "Observațiile scrise de furnizor în factura electronică. Textul întreg al celei alese se vede dedesubt.")
        '
        ' rtbObservatie
        '
        rtbObservatie.Dock = System.Windows.Forms.DockStyle.Bottom
        rtbObservatie.FooterVisible = False
        rtbObservatie.HeaderVisible = False
        rtbObservatie.Location = New System.Drawing.Point(3, 353)
        rtbObservatie.Margin = New System.Windows.Forms.Padding(0)
        rtbObservatie.Name = "rtbObservatie"
        rtbObservatie.Size = New System.Drawing.Size(751, 170)
        rtbObservatie.TabIndex = 1
        '
        ' PrimiteObservatiiPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridObservatii)
        Controls.Add(rtbObservatie)
        Name = "PrimiteObservatiiPage"
        Padding = New System.Windows.Forms.Padding(3)
        Size = New System.Drawing.Size(757, 526)
        CType(gridObservatii, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents gridObservatii As KBotDataView
    Friend WithEvents rtbObservatie As KBotRichTextEditor
End Class
