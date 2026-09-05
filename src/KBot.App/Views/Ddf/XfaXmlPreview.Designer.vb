Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class XfaXmlPreview
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
        tips = New Global.KBot.Controls.KBotToolTip(components)
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        pnlContent = New Panel()
        grid = New KBotDataView()
        pnlHeader = New Panel()
        lblNota = New Label()
        tblHeader = New TableLayoutPanel()
        pnlMissing = New Panel()
        tblMissing = New TableLayoutPanel()
        lblMissing = New Label()
        btnGenereaza = New Button()
        lblMessage = New Label()
        pnlContent.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlHeader.SuspendLayout()
        pnlMissing.SuspendLayout()
        tblMissing.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlContent
        ' 
        pnlContent.Controls.Add(grid)
        pnlContent.Controls.Add(pnlHeader)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(0, 0)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(840, 460)
        pnlContent.TabIndex = 0
        pnlContent.Visible = False
        ' 
        ' grid
        ' 
        grid.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.filter
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 50
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 170
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Element Fundamentare"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "element"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 250
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.DecimalPlaces = 2
        KBotDataColumn3.Format = KBotFormat.Standard
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Valoare" & vbCrLf & "Precedentă"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "valprec"
        KBotDataColumn3.MultiLine = True
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn3.ValueType = KBotValueType.Number
        KBotDataColumn3.Width = 130
        KBotDataColumn4.Aggregate = KBotAggregate.Sum
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Valoare" & vbCrLf & "Curentă"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "valcur"
        KBotDataColumn4.MultiLine = True
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 140
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Valoare" & vbCrLf & "Totală"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "valtot"
        KBotDataColumn5.MultiLine = True
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 130
        grid.Columns.Add(KBotDataColumn1)
        grid.Columns.Add(KBotDataColumn2)
        grid.Columns.Add(KBotDataColumn3)
        grid.Columns.Add(KBotDataColumn4)
        grid.Columns.Add(KBotDataColumn5)
        grid.Dock = DockStyle.Fill
        grid.EnableGrouping = True
        grid.FooterBackColor = SystemColors.Control
        grid.FooterCaption = "TOTAL"
        grid.FooterHeight = 40
        grid.FooterVisible = True
        grid.FrozenColumnCount = 1
        grid.Location = New Point(0, 120)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.ScrollByColumn = True
        grid.Size = New Size(840, 340)
        grid.TabIndex = 0
        ' 
        ' pnlHeader
        ' 
        pnlHeader.Controls.Add(lblNota)
        pnlHeader.Controls.Add(tblHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Padding = New Padding(8, 6, 8, 6)
        pnlHeader.Size = New Size(840, 120)
        pnlHeader.TabIndex = 0
        ' 
        ' lblNota
        ' 
        lblNota.Dock = DockStyle.Fill
        lblNota.Location = New Point(8, 6)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(824, 108)
        lblNota.TabIndex = 1
        ' 
        ' tblHeader
        ' 
        tblHeader.AutoSize = True
        tblHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tblHeader.ColumnCount = 2
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tblHeader.ColumnStyles.Add(New ColumnStyle())
        tblHeader.Dock = DockStyle.Top
        tblHeader.Location = New Point(8, 6)
        tblHeader.Name = "tblHeader"
        tblHeader.Size = New Size(824, 0)
        tblHeader.TabIndex = 0
        ' 
        ' pnlMissing
        ' 
        pnlMissing.Controls.Add(tblMissing)
        pnlMissing.Dock = DockStyle.Fill
        pnlMissing.Location = New Point(0, 0)
        pnlMissing.Name = "pnlMissing"
        pnlMissing.Size = New Size(840, 460)
        pnlMissing.TabIndex = 1
        pnlMissing.Visible = False
        ' 
        ' tblMissing
        ' 
        tblMissing.ColumnCount = 1
        tblMissing.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblMissing.Controls.Add(lblMissing, 0, 0)
        tblMissing.Controls.Add(btnGenereaza, 0, 1)
        tblMissing.Dock = DockStyle.Fill
        tblMissing.Location = New Point(0, 0)
        tblMissing.Name = "tblMissing"
        tblMissing.RowCount = 2
        tblMissing.RowStyles.Add(New RowStyle(SizeType.Percent, 55F))
        tblMissing.RowStyles.Add(New RowStyle(SizeType.Percent, 45F))
        tblMissing.Size = New Size(840, 460)
        tblMissing.TabIndex = 0
        ' 
        ' lblMissing
        ' 
        lblMissing.Dock = DockStyle.Fill
        lblMissing.Font = New Font("Segoe UI", 10F)
        lblMissing.Location = New Point(3, 0)
        lblMissing.Name = "lblMissing"
        lblMissing.Size = New Size(834, 253)
        lblMissing.TabIndex = 0
        lblMissing.Text = "Documentul nu a fost încă generat."
        lblMissing.TextAlign = ContentAlignment.BottomCenter
        ' 
        ' btnGenereaza
        ' 
        btnGenereaza.Anchor = AnchorStyles.Top
        btnGenereaza.AutoSize = True
        btnGenereaza.FlatStyle = FlatStyle.Flat
        btnGenereaza.Location = New Point(303, 256)
        btnGenereaza.Name = "btnGenereaza"
        btnGenereaza.Padding = New Padding(14, 6, 14, 6)
        btnGenereaza.Size = New Size(233, 49)
        btnGenereaza.TabIndex = 1
        btnGenereaza.Text = "Generează documentul"
        btnGenereaza.UseVisualStyleBackColor = True
        ' 
        ' lblMessage
        ' 
        lblMessage.Dock = DockStyle.Fill
        lblMessage.Font = New Font("Segoe UI", 10F)
        lblMessage.Location = New Point(0, 0)
        lblMessage.Name = "lblMessage"
        lblMessage.Size = New Size(840, 460)
        lblMessage.TabIndex = 2
        lblMessage.Text = "Selectați o revizie din arbore."
        lblMessage.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' XfaXmlPreview
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pnlContent)
        Controls.Add(pnlMissing)
        Controls.Add(lblMessage)
        Name = "XfaXmlPreview"
        Size = New Size(840, 460)
        pnlContent.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlHeader.ResumeLayout(False)
        pnlHeader.PerformLayout()
        pnlMissing.ResumeLayout(False)
        tblMissing.ResumeLayout(False)
        tblMissing.PerformLayout()
        '
        ' tips — etichetele de survolare (felia 0035), toate în română.
        '
        tips.SetToolTipHeader(btnGenereaza, "Generează")
        tips.SetToolTipText(btnGenereaza, "Construiește PDF-ul din valorile curente." & vbLf & "Generarea rulează în fundal; fereastra rămâne folosibilă.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlContent As Panel
    Friend WithEvents grid As Global.KBot.Controls.KBotDataView
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblNota As Label
    Friend WithEvents tblHeader As TableLayoutPanel
    Friend WithEvents pnlMissing As Panel
    Friend WithEvents tblMissing As TableLayoutPanel
    Friend WithEvents lblMissing As Label
    Friend WithEvents btnGenereaza As Button
    Friend WithEvents lblMessage As Label
End Class
