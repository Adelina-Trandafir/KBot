Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteReceptiiPage
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
        tips = New KBotToolTip(components)
        gridLegaturi = New KBotDataView()
        lblNota = New System.Windows.Forms.Label()
        pnlButoane = New System.Windows.Forms.Panel()
        tlyButoane = New KBotTableLayoutPanel()
        btnExistenta = New KBotButton()
        btnNoua = New KBotButton()
        btnModifica = New KBotButton()
        btnScoate = New KBotButton()
        lblRezumat = New System.Windows.Forms.Label()
        CType(gridLegaturi, System.ComponentModel.ISupportInitialize).BeginInit()
        pnlButoane.SuspendLayout()
        tlyButoane.SuspendLayout()
        SuspendLayout()
        '
        ' gridLegaturi
        '
        gridLegaturi.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridLegaturi.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Angajament"
        KBotDataColumn1.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "angajament"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.Width = 120
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Clasificație"
        KBotDataColumn2.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "clsf"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 190
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Recepție"
        KBotDataColumn3.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "receptie"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 330
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Legătură"
        KBotDataColumn4.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "tip"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 140
        KBotDataColumn5.Aggregate = KBotAggregate.Sum
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Sumă (cu TVA)"
        KBotDataColumn5.HeaderTextAlign = System.Drawing.ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "suma"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 120
        gridLegaturi.Columns.Add(KBotDataColumn1)
        gridLegaturi.Columns.Add(KBotDataColumn2)
        gridLegaturi.Columns.Add(KBotDataColumn3)
        gridLegaturi.Columns.Add(KBotDataColumn4)
        gridLegaturi.Columns.Add(KBotDataColumn5)
        gridLegaturi.Dock = System.Windows.Forms.DockStyle.Fill
        gridLegaturi.FillColumnKey = "receptie"
        gridLegaturi.FooterCaption = "TOTAL ALOCAT"
        gridLegaturi.FooterVisible = True
        gridLegaturi.Location = New System.Drawing.Point(3, 35)
        gridLegaturi.Margin = New System.Windows.Forms.Padding(0)
        gridLegaturi.Name = "gridLegaturi"
        gridLegaturi.ReadOnlyGrid = True
        gridLegaturi.Size = New System.Drawing.Size(751, 402)
        gridLegaturi.TabIndex = 1
        tips.SetToolTipHeader(gridLegaturi, "Legăturile cu recepțiile")
        tips.SetToolTipText(gridLegaturi, "Fiecare rând arată cât dintr-o clasificație a facturii acoperă o recepție. O recepție nouă apare cu «așteaptă descărcarea» până când K-BOT o citește din FOREXE.")
        '
        ' lblNota
        '
        lblNota.Dock = System.Windows.Forms.DockStyle.Bottom
        lblNota.Location = New System.Drawing.Point(3, 437)
        lblNota.Margin = New System.Windows.Forms.Padding(0)
        lblNota.Name = "lblNota"
        lblNota.Padding = New System.Windows.Forms.Padding(2, 4, 2, 4)
        lblNota.Size = New System.Drawing.Size(751, 36)
        lblNota.TabIndex = 2
        '
        ' pnlButoane
        '
        pnlButoane.Controls.Add(tlyButoane)
        pnlButoane.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlButoane.Location = New System.Drawing.Point(3, 473)
        pnlButoane.Margin = New System.Windows.Forms.Padding(0)
        pnlButoane.Name = "pnlButoane"
        pnlButoane.Size = New System.Drawing.Size(751, 50)
        pnlButoane.TabIndex = 3
        '
        ' tlyButoane
        '
        tlyButoane.ColumnCount = 4
        tlyButoane.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        tlyButoane.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        tlyButoane.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        tlyButoane.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        tlyButoane.Controls.Add(btnExistenta, 0, 0)
        tlyButoane.Controls.Add(btnNoua, 1, 0)
        tlyButoane.Controls.Add(btnModifica, 2, 0)
        tlyButoane.Controls.Add(btnScoate, 3, 0)
        tlyButoane.Dock = System.Windows.Forms.DockStyle.Fill
        tlyButoane.Location = New System.Drawing.Point(0, 0)
        tlyButoane.Margin = New System.Windows.Forms.Padding(0)
        tlyButoane.Name = "tlyButoane"
        tlyButoane.RowCount = 1
        tlyButoane.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyButoane.Size = New System.Drawing.Size(751, 50)
        tlyButoane.TabIndex = 0
        '
        ' btnExistenta
        '
        btnExistenta.Dock = System.Windows.Forms.DockStyle.Fill
        btnExistenta.Location = New System.Drawing.Point(0, 0)
        btnExistenta.Margin = New System.Windows.Forms.Padding(0, 4, 4, 4)
        btnExistenta.Name = "btnExistenta"
        btnExistenta.Size = New System.Drawing.Size(184, 42)
        btnExistenta.TabIndex = 0
        btnExistenta.Text = "Leagă de recepție…"
        tips.SetToolTipHeader(btnExistenta, "Leagă de o recepție existentă")
        tips.SetToolTipText(btnExistenta, "Leagă factura de o recepție care are deja valoarea. Nu schimbă nimic în FOREXE.")
        '
        ' btnNoua
        '
        btnNoua.Dock = System.Windows.Forms.DockStyle.Fill
        btnNoua.Location = New System.Drawing.Point(188, 0)
        btnNoua.Margin = New System.Windows.Forms.Padding(0, 4, 4, 4)
        btnNoua.Name = "btnNoua"
        btnNoua.Size = New System.Drawing.Size(184, 42)
        btnNoua.TabIndex = 1
        btnNoua.Text = "Recepție nouă…"
        tips.SetToolTipHeader(btnNoua, "Recepție nouă")
        tips.SetToolTipText(btnNoua, "Creează în FOREXE o recepție cu valoarea facturii (sau o parte din ea) și o leagă de factură.")
        '
        ' btnModifica
        '
        btnModifica.Dock = System.Windows.Forms.DockStyle.Fill
        btnModifica.Location = New System.Drawing.Point(376, 0)
        btnModifica.Margin = New System.Windows.Forms.Padding(0, 4, 4, 4)
        btnModifica.Name = "btnModifica"
        btnModifica.Size = New System.Drawing.Size(184, 42)
        btnModifica.TabIndex = 2
        btnModifica.Text = "Modifică recepția…"
        tips.SetToolTipHeader(btnModifica, "Modifică recepția")
        tips.SetToolTipText(btnModifica, "Mărește în FOREXE o recepție care nu ajunge pentru factură și o leagă de factură.")
        '
        ' btnScoate
        '
        btnScoate.Dock = System.Windows.Forms.DockStyle.Fill
        btnScoate.Enabled = False
        btnScoate.Location = New System.Drawing.Point(564, 0)
        btnScoate.Margin = New System.Windows.Forms.Padding(0, 4, 0, 4)
        btnScoate.Name = "btnScoate"
        btnScoate.Size = New System.Drawing.Size(187, 42)
        btnScoate.TabIndex = 3
        btnScoate.Text = "Scoate legătura"
        tips.SetToolTipHeader(btnScoate, "Scoate legătura")
        tips.SetToolTipText(btnScoate, "Șterge legătura aleasă din tabel. Recepția din FOREXE nu se atinge.")
        '
        ' lblRezumat
        '
        lblRezumat.Dock = System.Windows.Forms.DockStyle.Top
        lblRezumat.Font = New System.Drawing.Font("Calibri", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CByte(0))
        lblRezumat.Location = New System.Drawing.Point(3, 3)
        lblRezumat.Margin = New System.Windows.Forms.Padding(0)
        lblRezumat.Name = "lblRezumat"
        lblRezumat.Padding = New System.Windows.Forms.Padding(2, 6, 2, 6)
        lblRezumat.Size = New System.Drawing.Size(751, 32)
        lblRezumat.TabIndex = 0
        lblRezumat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        ' PrimiteReceptiiPage
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(gridLegaturi)
        Controls.Add(lblNota)
        Controls.Add(pnlButoane)
        Controls.Add(lblRezumat)
        Name = "PrimiteReceptiiPage"
        Padding = New System.Windows.Forms.Padding(3, 3, 3, 0)
        Size = New System.Drawing.Size(757, 526)
        CType(gridLegaturi, System.ComponentModel.ISupportInitialize).EndInit()
        pnlButoane.ResumeLayout(False)
        tlyButoane.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents gridLegaturi As KBotDataView
    Friend WithEvents lblNota As System.Windows.Forms.Label
    Friend WithEvents pnlButoane As System.Windows.Forms.Panel
    Friend WithEvents tlyButoane As KBotTableLayoutPanel
    Friend WithEvents btnExistenta As KBotButton
    Friend WithEvents btnNoua As KBotButton
    Friend WithEvents btnModifica As KBotButton
    Friend WithEvents btnScoate As KBotButton
    Friend WithEvents lblRezumat As System.Windows.Forms.Label
End Class
