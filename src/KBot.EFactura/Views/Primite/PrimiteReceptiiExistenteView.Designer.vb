<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteReceptiiExistenteView
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
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        cmbMod = New Controls.KBotComboBox()
        pnlMod = New Controls.KBotTableLayoutPanel()
        lblMod = New Global.KBot.Controls.KBotLabel()
        gridReceptii = New Controls.KBotDataView()
        pnlMod.SuspendLayout()
        CType(gridReceptii, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' cmbMod
        ' 
        cmbMod.Dock = System.Windows.Forms.DockStyle.Fill
        cmbMod.Location = New System.Drawing.Point(133, 4)
        cmbMod.Margin = New System.Windows.Forms.Padding(4)
        cmbMod.Name = "cmbMod"
        cmbMod.Size = New System.Drawing.Size(403, 37)
        cmbMod.TabIndex = 1
        tips.SetToolTipHeader(cmbMod, "Ce se face cu recepția aleasă")
        tips.SetToolTipText(cmbMod, "Leagă: doar leagă facturile de recepție, nimic nu se schimbă în FOREXE. Modifică: valorile recepției cresc cu sumele din «Recepționată».")
        ' 
        ' pnlMod
        ' 
        pnlMod.ColumnCount = 2
        pnlMod.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 129F))
        pnlMod.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        pnlMod.Controls.Add(lblMod, 0, 0)
        pnlMod.Controls.Add(cmbMod, 1, 0)
        pnlMod.Dock = System.Windows.Forms.DockStyle.Top
        pnlMod.Location = New System.Drawing.Point(0, 0)
        pnlMod.Margin = New System.Windows.Forms.Padding(0)
        pnlMod.Name = "pnlMod"
        pnlMod.RowCount = 1
        pnlMod.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        pnlMod.Size = New System.Drawing.Size(540, 48)
        pnlMod.TabIndex = 2
        ' 
        ' lblMod
        ' 
        lblMod.BorderWidth = 0
        lblMod.Dock = System.Windows.Forms.DockStyle.Fill
        lblMod.Location = New System.Drawing.Point(4, 0)
        lblMod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblMod.Name = "lblMod"
        lblMod.Size = New System.Drawing.Size(121, 48)
        lblMod.TabIndex = 0
        lblMod.Text = "Acțiune:"
        lblMod.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' gridReceptii
        ' 
        gridReceptii.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridReceptii.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = "dd.MM.yyyy"
        KBotDataColumn1.HeaderText = "Data"
        KBotDataColumn1.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "data"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ValueType = KBot.Controls.KBotValueType.DateTime
        KBotDataColumn1.Width = 84
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Descriere"
        KBotDataColumn2.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "descriere"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 140
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Valoare"
        KBotDataColumn3.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "total"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Rest nealocat"
        KBotDataColumn4.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "rest"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBot.Controls.KBotValueType.Number
        gridReceptii.Columns.Add(KBotDataColumn1)
        gridReceptii.Columns.Add(KBotDataColumn2)
        gridReceptii.Columns.Add(KBotDataColumn3)
        gridReceptii.Columns.Add(KBotDataColumn4)
        gridReceptii.Dock = System.Windows.Forms.DockStyle.Fill
        gridReceptii.FillColumnKey = "descriere"
        gridReceptii.Location = New System.Drawing.Point(0, 48)
        gridReceptii.Margin = New System.Windows.Forms.Padding(0)
        gridReceptii.Name = "gridReceptii"
        gridReceptii.ReadOnlyGrid = True
        gridReceptii.Size = New System.Drawing.Size(540, 176)
        gridReceptii.TabIndex = 0
        ' 
        ' PrimiteReceptiiExistenteView
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridReceptii)
        Controls.Add(pnlMod)
        Margin = New System.Windows.Forms.Padding(0)
        Name = "PrimiteReceptiiExistenteView"
        Size = New System.Drawing.Size(540, 224)
        pnlMod.ResumeLayout(False)
        CType(gridReceptii, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlMod As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblMod As Global.KBot.Controls.KBotLabel
    Friend WithEvents cmbMod As Global.KBot.Controls.KBotComboBox
    Friend WithEvents gridReceptii As Global.KBot.Controls.KBotDataView
End Class
