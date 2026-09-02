Imports KBot.Controls

' Pagina «Beneficiari» a editorului de ordonantare (felia 0049) — portul lui
' `frmFX_ORD_PART` + subformularul continuu `frmFX_ORD_TBL`.
'
' Cele doua formulare Access devin trei zone:
'   stanga    = lista beneficiarilor (Access: `lstDenBene`), cu randul sintetic
'               «< TOTI BENEFICIARII >» si cu bifa `chkClsf` deasupra;
'   dreapta sus = campurile beneficiarului selectat (DenBene / CodFiscal / ContIBAN / Banca)
'               plus alegatorul de partener (Access: combo-ul `CodPartener`);
'   dreapta jos = grila liniilor de plata (Access: `frmFX_ORD_TBL`), cu rand de totaluri.
'
' `btnClsf` din `frmFX_ORD_TBL` NU se porteaza: in tot exportul Access nu are niciun
' `btnClsf_Click`, iar singura lui aparitie e in `PositionElements`, o functie care incepe cu
' `Exit Function`. Un buton fara comportament ar fi un no-op tacut (interzis de regulile casei).
'
' Toate controalele se declara AICI (docs/kbot-forms-ui-convention.md): pagina trebuie sa se
' randeze in designerul Visual Studio, nu sa se construiasca la rulare.
' Coordonatele sunt scrise la 96 dpi si AutoScaleDimensions le insoteste.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OrdBeneficiariPage
    Inherits System.Windows.Forms.UserControl

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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        tips = New KBotToolTip(components)
        chkClsf = New CheckBox()
        cboCodPartener = New KBotComboBox()
        grdLinii = New KBotDataView()
        split = New SplitContainer()
        tlyStanga = New TableLayoutPanel()
        grdStanga = New KBotDataView()
        tlyDreapta = New TableLayoutPanel()
        tlyAntetBene = New TableLayoutPanel()
        lblDenBene = New Label()
        txtDenBene = New KBotTextField()
        lblCodPartener = New Label()
        lblCodFiscal = New Label()
        txtCodFiscal = New KBotTextField()
        lblContIban = New Label()
        txtContIban = New KBotTextField()
        lblBanca = New Label()
        txtBanca = New KBotTextField()
        CType(grdLinii, ComponentModel.ISupportInitialize).BeginInit()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        tlyStanga.SuspendLayout()
        CType(grdStanga, ComponentModel.ISupportInitialize).BeginInit()
        tlyDreapta.SuspendLayout()
        tlyAntetBene.SuspendLayout()
        SuspendLayout()
        ' 
        ' chkClsf
        ' 
        chkClsf.AutoSize = True
        chkClsf.Dock = DockStyle.Fill
        chkClsf.Location = New Point(9, 7)
        chkClsf.Margin = New Padding(9, 7, 9, 3)
        chkClsf.Name = "chkClsf"
        chkClsf.Size = New Size(313, 40)
        chkClsf.TabIndex = 0
        chkClsf.Text = "Grupează pe clasificații"
        tips.SetToolTipHeader(chkClsf, "Grupează pe clasificații")
        tips.SetToolTipText(chkClsf, "Bifat: lista din stânga arată clasificațiile, iar coloana a doua a grilei arată contul IBAN." & vbLf & "Nebifat: lista arată beneficiarii, iar coloana arată codul SSI.")
        chkClsf.UseVisualStyleBackColor = True
        ' 
        ' cboCodPartener
        ' 
        cboCodPartener.Dock = DockStyle.Fill
        cboCodPartener.DrawMode = DrawMode.OwnerDrawFixed
        cboCodPartener.DropDownStyle = ComboBoxStyle.DropDownList
        cboCodPartener.FlatStyle = FlatStyle.Flat
        cboCodPartener.Location = New Point(624, 5)
        cboCodPartener.Margin = New Padding(4, 5, 4, 5)
        cboCodPartener.Name = "cboCodPartener"
        cboCodPartener.Size = New Size(290, 32)
        cboCodPartener.TabIndex = 3
        tips.SetToolTipHeader(cboCodPartener, "Partener")
        tips.SetToolTipText(cboCodPartener, "Partenerul din nomenclator care corespunde beneficiarului." & vbLf & "Completează codul fiscal și contul, dacă sunt goale.")
        ' 
        ' grdLinii
        ' 
        grdLinii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdLinii.AutoSizeHeaderHeight = False
        grdLinii.BackColor = SystemColors.Window
        grdLinii.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn1.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn1.ColumnFilterIcon = My.Resources.Resources.Everaldo_Crystal_Clear_App_xmag_search_48
        KBotDataColumn1.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clsf"
        KBotDataColumn1.MinWidth = 60
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.Width = 160
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn2.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn2.HeaderText = "Cod SSI"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "ssi_iban"
        KBotDataColumn2.MinWidth = 80
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 180
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn3.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn3.HeaderText = "Explicație"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "explicatie"
        KBotDataColumn3.MinWidth = 100
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn4.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.DecimalPlaces = 2
        KBotDataColumn4.Format = KBotFormat.Standard
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn4.HeaderText = "Recepții"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "total_receptii"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn4.ValueType = KBotValueType.Number
        KBotDataColumn4.Width = 110
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn5.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn5.DecimalPlaces = 2
        KBotDataColumn5.Format = KBotFormat.Standard
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn5.HeaderText = "Plăți ant."
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "plati_ant"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn5.ValueType = KBotValueType.Number
        KBotDataColumn5.Width = 110
        KBotDataColumn6.Aggregate = KBotAggregate.Sum
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn6.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn6.DecimalPlaces = 2
        KBotDataColumn6.Format = KBotFormat.Standard
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn6.HeaderText = "Valoare"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "valoare"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn6.ValueType = KBotValueType.Number
        KBotDataColumn6.Width = 110
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn7.CellPadding = New Padding(2, 0, 2, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn7.HeaderText = "Rămas"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "ramas"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn7.Width = 110
        grdLinii.Columns.Add(KBotDataColumn1)
        grdLinii.Columns.Add(KBotDataColumn2)
        grdLinii.Columns.Add(KBotDataColumn3)
        grdLinii.Columns.Add(KBotDataColumn4)
        grdLinii.Columns.Add(KBotDataColumn5)
        grdLinii.Columns.Add(KBotDataColumn6)
        grdLinii.Columns.Add(KBotDataColumn7)
        grdLinii.Dock = DockStyle.Fill
        grdLinii.EnableGrouping = True
        grdLinii.FillColumnKey = "explicatie"
        grdLinii.FooterBackColor = SystemColors.Control
        grdLinii.FooterCaption = "TOTAL"
        grdLinii.FooterFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdLinii.FooterHeight = 30
        grdLinii.FooterSeparatorColor = SystemColors.ActiveBorder
        grdLinii.FooterVisible = True
        grdLinii.FrozenColumnCount = 1
        grdLinii.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdLinii.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdLinii.Location = New Point(4, 145)
        grdLinii.Margin = New Padding(4, 5, 4, 5)
        grdLinii.Name = "grdLinii"
        grdLinii.RowHeight = 22
        grdLinii.Size = New Size(918, 538)
        grdLinii.TabIndex = 1
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(0)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tlyStanga)
        split.Panel1.Padding = New Padding(10, 0, 0, 10)
        split.Panel1MinSize = 180
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(tlyDreapta)
        split.Panel2.Padding = New Padding(0, 0, 10, 10)
        split.Panel2MinSize = 380
        split.Size = New Size(1286, 698)
        split.SplitterDistance = 341
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' tlyStanga
        ' 
        tlyStanga.ColumnCount = 1
        tlyStanga.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStanga.Controls.Add(grdStanga, 0, 1)
        tlyStanga.Controls.Add(chkClsf, 0, 0)
        tlyStanga.Dock = DockStyle.Fill
        tlyStanga.Location = New Point(10, 0)
        tlyStanga.Margin = New Padding(0)
        tlyStanga.Name = "tlyStanga"
        tlyStanga.RowCount = 2
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyStanga.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyStanga.Size = New Size(331, 688)
        tlyStanga.TabIndex = 0
        ' 
        ' grdStanga
        ' 
        grdStanga.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdStanga.BackColor = SystemColors.Window
        grdStanga.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.AutoSizeMode = KBotAutoSizeMode.None
        KBotDataColumn8.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        KBotDataColumn8.HeaderText = "Beneficiari"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn8.Key = "eticheta"
        KBotDataColumn8.MinWidth = 80
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 240
        grdStanga.Columns.Add(KBotDataColumn8)
        grdStanga.Dock = DockStyle.Fill
        grdStanga.EnableGrouping = True
        grdStanga.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        grdStanga.HeaderSeparatorColor = SystemColors.ActiveBorder
        grdStanga.Location = New Point(0, 50)
        grdStanga.Margin = New Padding(0)
        grdStanga.Name = "grdStanga"
        grdStanga.ReadOnlyGrid = True
        grdStanga.RowHeight = 22
        grdStanga.Size = New Size(331, 638)
        grdStanga.TabIndex = 1
        ' 
        ' tlyDreapta
        ' 
        tlyDreapta.ColumnCount = 1
        tlyDreapta.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDreapta.Controls.Add(grdLinii, 0, 1)
        tlyDreapta.Controls.Add(tlyAntetBene, 0, 0)
        tlyDreapta.Dock = DockStyle.Fill
        tlyDreapta.Location = New Point(0, 0)
        tlyDreapta.Margin = New Padding(0)
        tlyDreapta.Name = "tlyDreapta"
        tlyDreapta.RowCount = 2
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Absolute, 140F))
        tlyDreapta.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDreapta.Size = New Size(926, 688)
        tlyDreapta.TabIndex = 0
        ' 
        ' tlyAntetBene
        ' 
        tlyAntetBene.ColumnCount = 4
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 143F))
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55F))
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 114F))
        tlyAntetBene.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45F))
        tlyAntetBene.Controls.Add(lblDenBene, 0, 0)
        tlyAntetBene.Controls.Add(txtDenBene, 1, 0)
        tlyAntetBene.Controls.Add(lblCodPartener, 2, 0)
        tlyAntetBene.Controls.Add(cboCodPartener, 3, 0)
        tlyAntetBene.Controls.Add(lblCodFiscal, 0, 1)
        tlyAntetBene.Controls.Add(txtCodFiscal, 1, 1)
        tlyAntetBene.Controls.Add(lblContIban, 2, 1)
        tlyAntetBene.Controls.Add(txtContIban, 3, 1)
        tlyAntetBene.Controls.Add(lblBanca, 0, 2)
        tlyAntetBene.Controls.Add(txtBanca, 1, 2)
        tlyAntetBene.Dock = DockStyle.Fill
        tlyAntetBene.Location = New Point(4, 5)
        tlyAntetBene.Margin = New Padding(4, 5, 4, 5)
        tlyAntetBene.Name = "tlyAntetBene"
        tlyAntetBene.RowCount = 3
        tlyAntetBene.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyAntetBene.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyAntetBene.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyAntetBene.Size = New Size(918, 130)
        tlyAntetBene.TabIndex = 0
        ' 
        ' lblDenBene
        ' 
        lblDenBene.AutoSize = True
        lblDenBene.Dock = DockStyle.Fill
        lblDenBene.Location = New Point(4, 0)
        lblDenBene.Margin = New Padding(4, 0, 4, 0)
        lblDenBene.Name = "lblDenBene"
        lblDenBene.Size = New Size(135, 42)
        lblDenBene.TabIndex = 0
        lblDenBene.Text = "Beneficiar"
        lblDenBene.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtDenBene
        ' 
        txtDenBene.BackColor = Color.Transparent
        txtDenBene.Dock = DockStyle.Fill
        txtDenBene.Location = New Point(147, 5)
        txtDenBene.Margin = New Padding(4, 5, 4, 5)
        txtDenBene.MaxLength = 32767
        txtDenBene.Name = "txtDenBene"
        txtDenBene.PlaceholderText = ""
        txtDenBene.Size = New Size(355, 32)
        txtDenBene.TabIndex = 1
        txtDenBene.TabStop = False
        txtDenBene.UseSystemPasswordChar = False
        ' 
        ' lblCodPartener
        ' 
        lblCodPartener.AutoSize = True
        lblCodPartener.Dock = DockStyle.Fill
        lblCodPartener.Location = New Point(510, 0)
        lblCodPartener.Margin = New Padding(4, 0, 4, 0)
        lblCodPartener.Name = "lblCodPartener"
        lblCodPartener.Size = New Size(106, 42)
        lblCodPartener.TabIndex = 2
        lblCodPartener.Text = "Partener"
        lblCodPartener.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCodFiscal
        ' 
        lblCodFiscal.AutoSize = True
        lblCodFiscal.Dock = DockStyle.Fill
        lblCodFiscal.Location = New Point(4, 42)
        lblCodFiscal.Margin = New Padding(4, 0, 4, 0)
        lblCodFiscal.Name = "lblCodFiscal"
        lblCodFiscal.Size = New Size(135, 42)
        lblCodFiscal.TabIndex = 4
        lblCodFiscal.Text = "Cod fiscal"
        lblCodFiscal.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCodFiscal
        ' 
        txtCodFiscal.BackColor = Color.Transparent
        txtCodFiscal.Dock = DockStyle.Fill
        txtCodFiscal.Location = New Point(147, 47)
        txtCodFiscal.Margin = New Padding(4, 5, 4, 5)
        txtCodFiscal.MaxLength = 32767
        txtCodFiscal.Name = "txtCodFiscal"
        txtCodFiscal.PlaceholderText = ""
        txtCodFiscal.Size = New Size(355, 32)
        txtCodFiscal.TabIndex = 5
        txtCodFiscal.TabStop = False
        txtCodFiscal.UseSystemPasswordChar = False
        ' 
        ' lblContIban
        ' 
        lblContIban.AutoSize = True
        lblContIban.Dock = DockStyle.Fill
        lblContIban.Location = New Point(510, 42)
        lblContIban.Margin = New Padding(4, 0, 4, 0)
        lblContIban.Name = "lblContIban"
        lblContIban.Size = New Size(106, 42)
        lblContIban.TabIndex = 6
        lblContIban.Text = "Cont IBAN"
        lblContIban.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtContIban
        ' 
        txtContIban.BackColor = Color.Transparent
        txtContIban.Dock = DockStyle.Fill
        txtContIban.Location = New Point(624, 47)
        txtContIban.Margin = New Padding(4, 5, 4, 5)
        txtContIban.MaxLength = 32767
        txtContIban.Name = "txtContIban"
        txtContIban.PlaceholderText = ""
        txtContIban.Size = New Size(290, 32)
        txtContIban.TabIndex = 7
        txtContIban.TabStop = False
        txtContIban.UseSystemPasswordChar = False
        ' 
        ' lblBanca
        ' 
        lblBanca.AutoSize = True
        lblBanca.Dock = DockStyle.Fill
        lblBanca.Location = New Point(4, 84)
        lblBanca.Margin = New Padding(4, 0, 4, 0)
        lblBanca.Name = "lblBanca"
        lblBanca.Size = New Size(135, 46)
        lblBanca.TabIndex = 8
        lblBanca.Text = "Banca"
        lblBanca.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtBanca
        ' 
        txtBanca.BackColor = Color.Transparent
        tlyAntetBene.SetColumnSpan(txtBanca, 3)
        txtBanca.Dock = DockStyle.Fill
        txtBanca.Location = New Point(147, 89)
        txtBanca.Margin = New Padding(4, 5, 4, 5)
        txtBanca.MaxLength = 32767
        txtBanca.Name = "txtBanca"
        txtBanca.PlaceholderText = ""
        txtBanca.Size = New Size(767, 36)
        txtBanca.TabIndex = 9
        txtBanca.TabStop = False
        txtBanca.UseSystemPasswordChar = False
        ' 
        ' OrdBeneficiariPage
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Margin = New Padding(4, 5, 4, 5)
        Name = "OrdBeneficiariPage"
        Size = New Size(1286, 698)
        CType(grdLinii, ComponentModel.ISupportInitialize).EndInit()
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        tlyStanga.ResumeLayout(False)
        tlyStanga.PerformLayout()
        CType(grdStanga, ComponentModel.ISupportInitialize).EndInit()
        tlyDreapta.ResumeLayout(False)
        tlyAntetBene.ResumeLayout(False)
        tlyAntetBene.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents split As SplitContainer
    Friend WithEvents tlyStanga As TableLayoutPanel
    Friend WithEvents chkClsf As CheckBox
    Friend WithEvents grdStanga As KBot.Controls.KBotDataView
    Friend WithEvents tlyDreapta As TableLayoutPanel
    Friend WithEvents tlyAntetBene As TableLayoutPanel
    Friend WithEvents lblDenBene As Label
    Friend WithEvents txtDenBene As KBot.Controls.KBotTextField
    Friend WithEvents lblCodPartener As Label
    Friend WithEvents cboCodPartener As KBot.Controls.KBotComboBox
    Friend WithEvents lblCodFiscal As Label
    Friend WithEvents txtCodFiscal As KBot.Controls.KBotTextField
    Friend WithEvents lblContIban As Label
    Friend WithEvents txtContIban As KBot.Controls.KBotTextField
    Friend WithEvents lblBanca As Label
    Friend WithEvents txtBanca As KBot.Controls.KBotTextField
    Friend WithEvents grdLinii As KBot.Controls.KBotDataView
End Class
