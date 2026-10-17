Imports KBot.Controls

' Slice 00EF-41 -- the list of all the received invoices (shown when the root «Toate facturile» of the tree is chosen).
' All the controls are declared HERE (docs/kbot-forms-ui-convention.md). The children of `pnlJos` are added in reverse dock order:
' the last one added sits closest to the edge.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteListaPage
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
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        grila = New KBotDataView()
        pnlJos = New System.Windows.Forms.Panel()
        lblStatus = New System.Windows.Forms.Label()
        btnListeaza = New KBotButton()
        pnlGap = New System.Windows.Forms.Panel()
        btnXml = New KBotButton()
        CType(grila, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlJos.SuspendLayout()
        SuspendLayout()
        '
        ' grila: one row per invoice; the first column is the tick, the last one the options button of the row
        '
        grila.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grila.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderRightIcon = My.Resources.Resources.check_boxes
        KBotDataColumn1.HeaderRightIconTooltip = "Alege ce facturi se bifează."
        KBotDataColumn1.HeaderText = ""
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "sel"
        KBotDataColumn1.MinWidth = 10
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Width = 40
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Data"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "data"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Width = 100
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Număr"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "nr"
        KBotDataColumn3.MinWidth = 80
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 170
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Recepție asociată"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.HeaderTooltip = "Recepția legată de factură: data, descrierea și valoarea ei, într-o singură celulă. Gol = factura nu e legată de nicio recepție."
        KBotDataColumn4.Key = "receptie"
        KBotDataColumn4.MinWidth = 140
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 380
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Scadență"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "scad"
        KBotDataColumn5.MinWidth = 80
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Width = 100
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Total"
        KBotDataColumn6.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "total"
        KBotDataColumn6.MinWidth = 90
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 120
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.ButtonText = "…"
        KBotDataColumn7.ColumnType = KBotColumnType.Button
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderText = ""
        KBotDataColumn7.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn7.HeaderTooltip = "Opțiunile rândului: listează factura sau o salvează ca XML."
        KBotDataColumn7.Key = "opt"
        KBotDataColumn7.MinWidth = 36
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.Width = 48
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        grila.Columns.Add(KBotDataColumn3)
        grila.Columns.Add(KBotDataColumn4)
        grila.Columns.Add(KBotDataColumn5)
        grila.Columns.Add(KBotDataColumn6)
        grila.Columns.Add(KBotDataColumn7)
        grila.Dock = System.Windows.Forms.DockStyle.Fill
        grila.FillColumnKey = "receptie"
        grila.FooterCaption = "TOTAL"
        grila.FooterVisible = True
        grila.Location = New System.Drawing.Point(3, 3)
        grila.Margin = New System.Windows.Forms.Padding(0)
        grila.Name = "grila"
        grila.Size = New System.Drawing.Size(751, 457)
        grila.TabIndex = 0
        tips.SetToolTipHeader(grila, "Toate facturile")
        tips.SetToolTipText(grila, "Bifează facturile la care se aplică butoanele de jos. Butonul «…» de pe un rând se aplică numai acelui rând.")
        '
        ' pnlJos
        '
        pnlJos.Controls.Add(lblStatus)
        pnlJos.Controls.Add(btnListeaza)
        pnlJos.Controls.Add(pnlGap)
        pnlJos.Controls.Add(btnXml)
        pnlJos.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlJos.Location = New System.Drawing.Point(3, 460)
        pnlJos.Margin = New System.Windows.Forms.Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New System.Windows.Forms.Padding(0, 8, 0, 4)
        pnlJos.Size = New System.Drawing.Size(751, 62)
        pnlJos.TabIndex = 1
        '
        ' lblStatus
        '
        lblStatus.Dock = System.Windows.Forms.DockStyle.Fill
        lblStatus.Location = New System.Drawing.Point(0, 8)
        lblStatus.Margin = New System.Windows.Forms.Padding(0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New System.Drawing.Size(351, 50)
        lblStatus.TabIndex = 0
        lblStatus.Text = "Nimic bifat."
        lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' btnListeaza
        '
        btnListeaza.Dock = System.Windows.Forms.DockStyle.Right
        btnListeaza.Location = New System.Drawing.Point(351, 8)
        btnListeaza.Margin = New System.Windows.Forms.Padding(0)
        btnListeaza.Name = "btnListeaza"
        btnListeaza.Size = New System.Drawing.Size(200, 50)
        btnListeaza.TabIndex = 1
        btnListeaza.Text = "Listează"
        tips.SetToolTipHeader(btnListeaza, "Listează")
        tips.SetToolTipText(btnListeaza, "Pregătește facturile <b>bifate</b> (factura clasică, făcută de K-BOT) și le trimite la imprimantă, fără să le deschidă.")
        '
        ' pnlGap
        '
        pnlGap.Dock = System.Windows.Forms.DockStyle.Right
        pnlGap.Location = New System.Drawing.Point(551, 8)
        pnlGap.Margin = New System.Windows.Forms.Padding(0)
        pnlGap.Name = "pnlGap"
        pnlGap.Size = New System.Drawing.Size(10, 50)
        pnlGap.TabIndex = 2
        '
        ' btnXml
        '
        btnXml.Dock = System.Windows.Forms.DockStyle.Right
        btnXml.Location = New System.Drawing.Point(561, 8)
        btnXml.Margin = New System.Windows.Forms.Padding(0)
        btnXml.Name = "btnXml"
        btnXml.Size = New System.Drawing.Size(190, 50)
        btnXml.TabIndex = 3
        btnXml.Text = "Salvează XML"
        tips.SetToolTipHeader(btnXml, "Salvează XML")
        tips.SetToolTipText(btnXml, "Salvează XML-urile facturilor <b>bifate</b> într-un dosar ales de tine.")
        '
        ' PrimiteListaPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(grila)
        Controls.Add(pnlJos)
        Name = "PrimiteListaPage"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 0)
        Size = New System.Drawing.Size(757, 525)
        CType(grila, System.ComponentModel.ISupportInitialize).EndInit()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents grila As KBotDataView
    Friend WithEvents pnlJos As System.Windows.Forms.Panel
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents btnListeaza As KBotButton
    Friend WithEvents pnlGap As System.Windows.Forms.Panel
    Friend WithEvents btnXml As KBotButton
End Class
