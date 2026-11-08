<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KeReceptiiView
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
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotGroupLevel1 As KBot.Controls.KBotGroupLevel = New Controls.KBotGroupLevel()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn9 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn10 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn11 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn12 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn13 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn14 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn15 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn16 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn17 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        backgroundRoles = New Global.KBot.Theming.KBotBackgroundRoles(components)
        btnOk = New Global.KBot.Controls.KBotButton()
        tlyRec = New Controls.KBotTableLayoutPanel()
        pnlTree = New System.Windows.Forms.Panel()
        pnlNav = New System.Windows.Forms.Panel()
        lblDetalii = New Global.KBot.Controls.KBotLabel()
        pnlLinii = New System.Windows.Forms.Panel()
        gridLinii = New Controls.KBotDataView()
        pnlEditor = New System.Windows.Forms.Panel()
        editorReceptii = New PrimiteReceptiiEditorView()
        ntfMesaj = New Controls.KBotNotice()
        pnlJoker = New System.Windows.Forms.Panel()
        gridAloc = New Controls.KBotDataView()
        tlyRec.SuspendLayout()
        pnlLinii.SuspendLayout()
        pnlEditor.SuspendLayout()
        CType(gridLinii, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridAloc, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' btnOk
        ' 
        btnOk.Visible = False
        btnOk.Enabled = False
        btnOk.Location = New System.Drawing.Point(1632, 12)
        btnOk.Margin = New System.Windows.Forms.Padding(4)
        btnOk.Name = "btnOk"
        btnOk.Size = New System.Drawing.Size(330, 63)
        btnOk.TabIndex = 0
        btnOk.Text = "Leagă"
        tips.SetToolTipHeader(btnOk, "Leagă")
        ' 
        ' tlyRec
        ' 
        tlyRec.ColumnCount = 3
        tlyRec.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F))
        tlyRec.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47F))
        tlyRec.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F))
        tlyRec.Controls.Add(pnlTree, 0, 0)
        tlyRec.Controls.Add(pnlNav, 1, 0)
        tlyRec.Controls.Add(lblDetalii, 1, 1)
        tlyRec.Controls.Add(pnlLinii, 1, 2)
        tlyRec.Controls.Add(pnlEditor, 2, 1)
        tlyRec.Controls.Add(pnlJoker, 0, 3)
        tlyRec.Controls.Add(gridAloc, 1, 3)
        tlyRec.Dock = System.Windows.Forms.DockStyle.Fill
        tlyRec.Location = New System.Drawing.Point(0, 0)
        tlyRec.Margin = New System.Windows.Forms.Padding(0)
        tlyRec.Name = "tlyRec"
        tlyRec.RowCount = 4
        tlyRec.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F))
        tlyRec.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F))
        tlyRec.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52F))
        tlyRec.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 48F))
        tlyRec.SetRowSpan(pnlTree, 3)
        tlyRec.SetColumnSpan(pnlNav, 2)
        tlyRec.SetRowSpan(pnlEditor, 2)
        tlyRec.SetColumnSpan(gridAloc, 2)
        tlyRec.Size = New System.Drawing.Size(1980, 1017)
        tlyRec.TabIndex = 0
        ' 
        ' pnlTree
        ' 
        pnlTree.Dock = System.Windows.Forms.DockStyle.Fill
        pnlTree.Location = New System.Drawing.Point(0, 0)
        pnlTree.Margin = New System.Windows.Forms.Padding(0, 0, 9, 6)
        pnlTree.Name = "pnlTree"
        pnlTree.Size = New System.Drawing.Size(495, 480)
        pnlTree.TabIndex = 1
        ' 
        ' pnlNav
        ' 
        pnlNav.Dock = System.Windows.Forms.DockStyle.Fill
        pnlNav.Location = New System.Drawing.Point(495, 0)
        pnlNav.Margin = New System.Windows.Forms.Padding(0)
        pnlNav.Name = "pnlNav"
        pnlNav.Size = New System.Drawing.Size(1485, 45)
        pnlNav.TabIndex = 2
        ' 
        ' lblDetalii
        ' 
        lblDetalii.BorderWidth = 0
        lblDetalii.Dock = System.Windows.Forms.DockStyle.Fill
        lblDetalii.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblDetalii.Location = New System.Drawing.Point(0, 0)
        lblDetalii.Margin = New System.Windows.Forms.Padding(0)
        lblDetalii.Name = "lblDetalii"
        lblDetalii.Padding = New System.Windows.Forms.Padding(3, 9, 3, 9)
        lblDetalii.Size = New System.Drawing.Size(920, 48)
        lblDetalii.TabIndex = 2
        ' 
        ' pnlLinii
        ' 
        pnlLinii.Controls.Add(gridLinii)
        pnlLinii.Dock = System.Windows.Forms.DockStyle.Fill
        pnlLinii.Location = New System.Drawing.Point(495, 93)
        pnlLinii.Margin = New System.Windows.Forms.Padding(0, 0, 0, 6)
        pnlLinii.Name = "pnlLinii"
        pnlLinii.Size = New System.Drawing.Size(930, 387)
        pnlLinii.TabIndex = 4
        ' 
        ' gridLinii
        ' 
        gridLinii.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridLinii.BackColor = Drawing.SystemColors.Window
        gridLinii.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Număr / Dată factură"
        KBotDataColumn1.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "factura"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Visible = KBot.Controls.KBotColumnVisibility.Hidden
        KBotDataColumn1.Width = 170
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Detalii"
        KBotDataColumn2.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "denumire"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 260
        KBotDataColumn3.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Valoare cu TVA"
        KBotDataColumn3.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cuTva"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn3.Visible = KBot.Controls.KBotColumnVisibility.Hidden
        KBotDataColumn3.Width = 110
        KBotDataColumn4.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Recepționată"
        KBotDataColumn4.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "rec"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn4.Width = 110
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBot.Controls.KBotColumnType.Combo
        KBotDataColumn5.CommitOnSelect = True
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.GroupHeaderCombo = True
        KBotDataColumn5.GroupHeaderComboFirstRow = True
        KBotDataColumn5.HeaderText = "Clasificație"
        KBotDataColumn5.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "clsf"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Width = 300
        gridLinii.Columns.Add(KBotDataColumn1)
        gridLinii.Columns.Add(KBotDataColumn2)
        gridLinii.Columns.Add(KBotDataColumn3)
        gridLinii.Columns.Add(KBotDataColumn4)
        gridLinii.Columns.Add(KBotDataColumn5)
        gridLinii.Dock = System.Windows.Forms.DockStyle.Fill
        gridLinii.FillColumnKey = "denumire"
        gridLinii.FooterCaption = "TOTAL"
        gridLinii.FooterVisible = True
        KBotGroupLevel1.CollapsedByDefault = True
        KBotGroupLevel1.ColumnKey = "factura"
        KBotGroupLevel1.FooterCaptionFormat = "{1}"
        KBotGroupLevel1.HeaderBackColor = Drawing.Color.White
        KBotGroupLevel1.HeaderBorder = True
        KBotGroupLevel1.HeaderCaptionFormat = "{1}"
        KBotGroupLevel1.KeyPattern = Nothing
        KBotGroupLevel1.ShowFooter = False
        KBotGroupLevel1.ShowFooterAggregates = False
        KBotGroupLevel1.ShowHeaderAggregates = True
        gridLinii.Groups.Add(KBotGroupLevel1)
        gridLinii.Location = New System.Drawing.Point(0, 39)
        gridLinii.Margin = New System.Windows.Forms.Padding(0)
        gridLinii.Name = "gridLinii"
        gridLinii.Size = New System.Drawing.Size(920, 477)
        gridLinii.TabIndex = 1
        ' 
        ' pnlEditor
        ' 
        pnlEditor.Controls.Add(editorReceptii)
        pnlEditor.Controls.Add(ntfMesaj)
        pnlEditor.Dock = System.Windows.Forms.DockStyle.Fill
        pnlEditor.Location = New System.Drawing.Point(1434, 45)
        pnlEditor.Margin = New System.Windows.Forms.Padding(9, 0, 0, 6)
        pnlEditor.Name = "pnlEditor"
        pnlEditor.Size = New System.Drawing.Size(546, 435)
        pnlEditor.TabIndex = 5
        ' 
        ' editorReceptii
        ' 
        editorReceptii.Dock = System.Windows.Forms.DockStyle.Fill
        editorReceptii.Location = New System.Drawing.Point(0, 0)
        editorReceptii.Margin = New System.Windows.Forms.Padding(0)
        editorReceptii.Name = "editorReceptii"
        editorReceptii.Size = New System.Drawing.Size(546, 435)
        editorReceptii.TabIndex = 0
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 381)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(0)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(546, 54)
        ntfMesaj.TabIndex = 2
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' pnlJoker
        ' 
        pnlJoker.Dock = System.Windows.Forms.DockStyle.Fill
        pnlJoker.Location = New System.Drawing.Point(0, 486)
        pnlJoker.Margin = New System.Windows.Forms.Padding(0, 0, 9, 0)
        pnlJoker.Name = "pnlJoker"
        pnlJoker.Size = New System.Drawing.Size(486, 531)
        pnlJoker.TabIndex = 6
        ' 
        ' gridAloc
        ' 
        gridAloc.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridAloc.BackColor = Drawing.SystemColors.Window
        gridAloc.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Clasificație"
        KBotDataColumn8.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "clsf"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 260
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.DecimalPlaces = 2
        KBotDataColumn9.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Buget"
        KBotDataColumn9.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "buget"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn9.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn9.Visible = KBot.Controls.KBotColumnVisibility.WhenRoom
        KBotDataColumn9.Width = 110
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.DecimalPlaces = 2
        KBotDataColumn10.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Total recepții"
        KBotDataColumn10.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "receptii"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn10.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn10.Visible = KBot.Controls.KBotColumnVisibility.WhenRoom
        KBotDataColumn10.Width = 110
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.DecimalPlaces = 2
        KBotDataColumn11.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Disponibil"
        KBotDataColumn11.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "disponibil"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn11.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn11.Visible = KBot.Controls.KBotColumnVisibility.WhenRoom
        KBotDataColumn11.Width = 110
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.DecimalPlaces = 2
        KBotDataColumn12.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Rezervări"
        KBotDataColumn12.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "angRezervari"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn12.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn12.Visible = KBot.Controls.KBotColumnVisibility.WhenRoom
        KBotDataColumn12.Width = 110
        KBotDataColumn13.AggregateFormatString = Nothing
        KBotDataColumn13.DecimalPlaces = 2
        KBotDataColumn13.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn13.FormatString = Nothing
        KBotDataColumn13.HeaderText = "Recepții"
        KBotDataColumn13.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn13.Key = "angReceptii"
        KBotDataColumn13.OptionGroup = Nothing
        KBotDataColumn13.ReadOnly = True
        KBotDataColumn13.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn13.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn13.Visible = KBot.Controls.KBotColumnVisibility.WhenRoom
        KBotDataColumn13.Width = 110
        KBotDataColumn14.AggregateFormatString = Nothing
        KBotDataColumn14.DecimalPlaces = 2
        KBotDataColumn14.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn14.FormatString = Nothing
        KBotDataColumn14.HeaderText = "Disponibil"
        KBotDataColumn14.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn14.Key = "angDisponibil"
        KBotDataColumn14.OptionGroup = Nothing
        KBotDataColumn14.ReadOnly = True
        KBotDataColumn14.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn14.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn14.Visible = KBot.Controls.KBotColumnVisibility.WhenRoom
        KBotDataColumn14.Width = 110
        KBotDataColumn15.Aggregate = KBot.Controls.KBotAggregate.Sum
        KBotDataColumn15.AggregateFormatString = Nothing
        KBotDataColumn15.DecimalPlaces = 2
        KBotDataColumn15.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn15.FormatString = Nothing
        KBotDataColumn15.HeaderText = "Valoare"
        KBotDataColumn15.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn15.Key = "valRecep"
        KBotDataColumn15.OptionGroup = Nothing
        KBotDataColumn15.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn15.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn15.Width = 110
        KBotDataColumn16.AggregateFormatString = Nothing
        KBotDataColumn16.DecimalPlaces = 2
        KBotDataColumn16.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn16.FormatString = Nothing
        KBotDataColumn16.HeaderText = "Rămas"
        KBotDataColumn16.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn16.Key = "ramas"
        KBotDataColumn16.OptionGroup = Nothing
        KBotDataColumn16.ReadOnly = True
        KBotDataColumn16.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn16.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn16.Width = 110
        KBotDataColumn17.AggregateFormatString = Nothing
        KBotDataColumn17.DecimalPlaces = 2
        KBotDataColumn17.Format = KBot.Controls.KBotFormat.Standard
        KBotDataColumn17.FormatString = Nothing
        KBotDataColumn17.HeaderText = "Rest Recepție"
        KBotDataColumn17.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn17.Key = "restRecep"
        KBotDataColumn17.OptionGroup = Nothing
        KBotDataColumn17.ReadOnly = True
        KBotDataColumn17.TextAlign = Drawing.ContentAlignment.MiddleRight
        KBotDataColumn17.ValueType = KBot.Controls.KBotValueType.Number
        KBotDataColumn17.Visible = KBot.Controls.KBotColumnVisibility.Hidden
        KBotDataColumn17.Width = 140
        gridAloc.Columns.Add(KBotDataColumn8)
        gridAloc.Columns.Add(KBotDataColumn9)
        gridAloc.Columns.Add(KBotDataColumn10)
        gridAloc.Columns.Add(KBotDataColumn11)
        gridAloc.Columns.Add(KBotDataColumn12)
        gridAloc.Columns.Add(KBotDataColumn13)
        gridAloc.Columns.Add(KBotDataColumn14)
        gridAloc.Columns.Add(KBotDataColumn15)
        gridAloc.Columns.Add(KBotDataColumn16)
        gridAloc.Columns.Add(KBotDataColumn17)
        gridAloc.Dock = System.Windows.Forms.DockStyle.Fill
        gridAloc.FillColumnKey = "clsf"
        gridAloc.FooterCaption = "TOTAL"
        gridAloc.FooterVisible = True
        gridAloc.Location = New System.Drawing.Point(0, 0)
        gridAloc.Margin = New System.Windows.Forms.Padding(0)
        gridAloc.Name = "gridAloc"
        gridAloc.Size = New System.Drawing.Size(1469, 483)
        gridAloc.TabIndex = 1
        '
        ' KeReceptiiView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        backgroundRoles.SetBackgroundRole(Me, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(tlyRec, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(pnlTree, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(pnlNav, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(pnlLinii, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(pnlEditor, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(editorReceptii, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        backgroundRoles.SetBackgroundRole(pnlJoker, Global.KBot.Theming.ThemeBackgroundRole.Inherit)
        Controls.Add(tlyRec)
        Controls.Add(btnOk)
        Margin = New System.Windows.Forms.Padding(0)
        Name = "KeReceptiiView"
        Size = New System.Drawing.Size(1980, 1017)
        tlyRec.ResumeLayout(False)
        pnlLinii.ResumeLayout(False)
        pnlEditor.ResumeLayout(False)
        CType(gridLinii, ComponentModel.ISupportInitialize).EndInit()
        CType(gridAloc, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents backgroundRoles As Global.KBot.Theming.KBotBackgroundRoles
    Friend WithEvents tlyRec As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents pnlTree As System.Windows.Forms.Panel
    Friend WithEvents pnlNav As System.Windows.Forms.Panel
    Friend WithEvents lblDetalii As Global.KBot.Controls.KBotLabel
    Friend WithEvents pnlLinii As System.Windows.Forms.Panel
    Friend WithEvents gridLinii As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlEditor As System.Windows.Forms.Panel
    Friend WithEvents editorReceptii As PrimiteReceptiiEditorView
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJoker As System.Windows.Forms.Panel
    Friend WithEvents gridAloc As Global.KBot.Controls.KBotDataView
    Friend WithEvents btnOk As Global.KBot.Controls.KBotButton
End Class
