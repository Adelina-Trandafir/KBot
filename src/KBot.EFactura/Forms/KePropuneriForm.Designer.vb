<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KePropuneriForm
    Inherits Global.KBot.Theming.KBotThemedForm

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
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn7 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn8 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn9 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn10 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn11 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn12 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        tips = New KBot.Controls.KBotToolTip(components)
        gridFact = New Controls.KBotDataView()
        gridCand = New Controls.KBotDataView()
        btnLeaga = New System.Windows.Forms.Button()
        btnRenunta = New System.Windows.Forms.Button()
        pnlCard = New System.Windows.Forms.Panel()
        lblCand = New System.Windows.Forms.Label()
        ntfMesaj = New Controls.KBotNotice()
        pnlJos = New System.Windows.Forms.Panel()
        lblSep1 = New System.Windows.Forms.Label()
        lblAntet = New System.Windows.Forms.Label()
        capBar = New Controls.KBotCaptionBar()
        CType(gridFact, ComponentModel.ISupportInitialize).BeginInit()
        CType(gridCand, ComponentModel.ISupportInitialize).BeginInit()
        pnlCard.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' gridFact
        ' 
        gridFact.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridFact.BackColor = Drawing.SystemColors.Window
        gridFact.CheckBoxes = KBot.Controls.KBotCheckBoxMode.RowsOnly
        gridFact.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Factura"
        KBotDataColumn1.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "factura"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 150
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Data"
        KBotDataColumn2.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "data"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 86
        KBotDataColumn3.Aggregate = KBot.Controls.KBotAggregate.Sum
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
        KBotDataColumn3.Width = 110
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Recepția propusă"
        KBotDataColumn4.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "receptie"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 300
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Data recepției"
        KBotDataColumn5.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "dataR"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Observații"
        KBotDataColumn6.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "obs"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 320
        gridFact.Columns.Add(KBotDataColumn1)
        gridFact.Columns.Add(KBotDataColumn2)
        gridFact.Columns.Add(KBotDataColumn3)
        gridFact.Columns.Add(KBotDataColumn4)
        gridFact.Columns.Add(KBotDataColumn5)
        gridFact.Columns.Add(KBotDataColumn6)
        gridFact.Dock = System.Windows.Forms.DockStyle.Fill
        gridFact.FillColumnKey = "obs"
        gridFact.Location = New System.Drawing.Point(0, 141)
        gridFact.Margin = New System.Windows.Forms.Padding(0)
        gridFact.Name = "gridFact"
        gridFact.Size = New System.Drawing.Size(1494, 375)
        gridFact.TabIndex = 2
        tips.SetToolTipHeader(gridFact, "Facturile neasociate")
        tips.SetToolTipText(gridFact, "Bifează facturile pe care vrei să le legi de recepția propusă. O factură fără recepție propusă se bifează după ce alegi recepția în tabelul de jos.")
        ' 
        ' gridCand
        ' 
        gridCand.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        gridCand.BackColor = Drawing.SystemColors.Window
        gridCand.ColumnFillMode = KBot.Controls.KBotFillMode.SpecificColumn
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = "Data"
        KBotDataColumn7.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "dataR"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.Width = 86
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderText = "Descriere"
        KBotDataColumn8.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "descriere"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 320
        KBotDataColumn9.AggregateFormatString = Nothing
        KBotDataColumn9.FormatString = Nothing
        KBotDataColumn9.HeaderText = "Angajament"
        KBotDataColumn9.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn9.Key = "cod"
        KBotDataColumn9.OptionGroup = Nothing
        KBotDataColumn9.ReadOnly = True
        KBotDataColumn9.Width = 130
        KBotDataColumn10.AggregateFormatString = Nothing
        KBotDataColumn10.FormatString = Nothing
        KBotDataColumn10.HeaderText = "Clasificație"
        KBotDataColumn10.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn10.Key = "clsf"
        KBotDataColumn10.OptionGroup = Nothing
        KBotDataColumn10.ReadOnly = True
        KBotDataColumn10.Width = 160
        KBotDataColumn11.AggregateFormatString = Nothing
        KBotDataColumn11.FormatString = Nothing
        KBotDataColumn11.HeaderText = "Zile de la factură"
        KBotDataColumn11.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Key = "zile"
        KBotDataColumn11.OptionGroup = Nothing
        KBotDataColumn11.ReadOnly = True
        KBotDataColumn11.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn11.Width = 110
        KBotDataColumn12.AggregateFormatString = Nothing
        KBotDataColumn12.FormatString = Nothing
        KBotDataColumn12.HeaderText = "Aleasă"
        KBotDataColumn12.HeaderTextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn12.Key = "aleasa"
        KBotDataColumn12.OptionGroup = Nothing
        KBotDataColumn12.ReadOnly = True
        KBotDataColumn12.TextAlign = Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn12.Width = 70
        gridCand.Columns.Add(KBotDataColumn7)
        gridCand.Columns.Add(KBotDataColumn8)
        gridCand.Columns.Add(KBotDataColumn9)
        gridCand.Columns.Add(KBotDataColumn10)
        gridCand.Columns.Add(KBotDataColumn11)
        gridCand.Columns.Add(KBotDataColumn12)
        gridCand.Dock = System.Windows.Forms.DockStyle.Bottom
        gridCand.FillColumnKey = "descriere"
        gridCand.Location = New System.Drawing.Point(0, 564)
        gridCand.Margin = New System.Windows.Forms.Padding(0)
        gridCand.Name = "gridCand"
        gridCand.ReadOnlyGrid = True
        gridCand.Size = New System.Drawing.Size(1494, 225)
        gridCand.TabIndex = 4
        tips.SetToolTipHeader(gridCand, "Recepțiile posibile")
        tips.SetToolTipText(gridCand, "Recepțiile care au, întregi, valoarea facturii alese sus. Dublu clic pe una o folosește pentru factură.")
        ' 
        ' btnLeaga
        ' 
        btnLeaga.Dock = System.Windows.Forms.DockStyle.Right
        btnLeaga.Enabled = False
        btnLeaga.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnLeaga.Location = New System.Drawing.Point(1164, 12)
        btnLeaga.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        btnLeaga.Name = "btnLeaga"
        btnLeaga.Size = New System.Drawing.Size(300, 63)
        btnLeaga.TabIndex = 0
        btnLeaga.Text = "Leagă"
        tips.SetToolTipHeader(btnLeaga, "Leagă")
        tips.SetToolTipText(btnLeaga, "Leagă fiecare factură bifată de recepția ei, întreagă. Până aici nu s-a salvat nimic.")
        btnLeaga.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Left
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(12, 12)
        btnRenunta.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New System.Drawing.Size(176, 63)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        tips.SetToolTipHeader(btnRenunta, "Renunță")
        tips.SetToolTipText(btnRenunta, "Închide fereastra fără să lege nimic.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(gridFact)
        pnlCard.Controls.Add(lblCand)
        pnlCard.Controls.Add(gridCand)
        pnlCard.Controls.Add(ntfMesaj)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(lblAntet)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(3, 3)
        pnlCard.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New System.Drawing.Size(1494, 954)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' lblCand
        ' 
        lblCand.Dock = System.Windows.Forms.DockStyle.Bottom
        lblCand.Location = New System.Drawing.Point(0, 516)
        lblCand.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblCand.Name = "lblCand"
        lblCand.Padding = New System.Windows.Forms.Padding(21, 12, 21, 6)
        lblCand.Size = New System.Drawing.Size(1494, 48)
        lblCand.TabIndex = 3
        lblCand.Text = "Recepții cu aceeași valoare ca factura aleasă sus (dublu clic pe una o alege)"
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 789)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(1494, 78)
        ntfMesaj.TabIndex = 7
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(btnLeaga)
        pnlJos.Controls.Add(lblSep1)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(0, 867)
        pnlJos.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(12, 12, 12, 12)
        pnlJos.Size = New System.Drawing.Size(1494, 87)
        pnlJos.TabIndex = 6
        pnlJos.Tag = "Card"
        ' 
        ' lblSep1
        ' 
        lblSep1.Dock = System.Windows.Forms.DockStyle.Right
        lblSep1.Location = New System.Drawing.Point(1464, 12)
        lblSep1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblSep1.Name = "lblSep1"
        lblSep1.Size = New System.Drawing.Size(18, 63)
        lblSep1.TabIndex = 1
        ' 
        ' lblAntet
        ' 
        lblAntet.Dock = System.Windows.Forms.DockStyle.Top
        lblAntet.Location = New System.Drawing.Point(0, 60)
        lblAntet.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        lblAntet.Name = "lblAntet"
        lblAntet.Padding = New System.Windows.Forms.Padding(21, 15, 21, 9)
        lblAntet.Size = New System.Drawing.Size(1494, 81)
        lblAntet.TabIndex = 1
        ' 
        ' capBar
        ' 
        capBar.Dock = System.Windows.Forms.DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New System.Drawing.Size(1494, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Recepții — propuneri pentru facturile neasociate"
        ' 
        ' KePropuneriForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        CancelButton = btnRenunta
        CenterOnScreen = False
        ClientSize = New System.Drawing.Size(1500, 960)
        Controls.Add(pnlCard)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1200, 780)
        Name = "KePropuneriForm"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "Recepții — propuneri pentru facturile neasociate"
        CType(gridFact, ComponentModel.ISupportInitialize).EndInit()
        CType(gridCand, ComponentModel.ISupportInitialize).EndInit()
        pnlCard.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As Global.KBot.Controls.KBotToolTip
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents capBar As Global.KBot.Controls.KBotCaptionBar
    Friend WithEvents lblAntet As System.Windows.Forms.Label
    Friend WithEvents gridFact As Global.KBot.Controls.KBotDataView
    Friend WithEvents lblCand As System.Windows.Forms.Label
    Friend WithEvents gridCand As Global.KBot.Controls.KBotDataView
    Friend WithEvents ntfMesaj As Global.KBot.Controls.KBotNotice
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents btnLeaga As System.Windows.Forms.Button
    Friend WithEvents lblSep1 As System.Windows.Forms.Label
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
End Class
