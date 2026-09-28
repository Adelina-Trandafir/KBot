Imports KBot.Controls

' «Note de corecție CAB» (slice 0088): the ERR operations FOREXE showed after the login, in a grid
' at the top; under it the correction of the selected one -- angajament, indicator, the two rows'
' account symbols and programs, the note number, the explanation -- and «Salvează nota».
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the 144 dpi
' the form was authored at. Card: children in REVERSE dock order (Fill first, then Bottom, then Top).
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CabNoteForm
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn6 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn7 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn8 As KBotDataColumn = New KBotDataColumn()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(CabNoteForm))
        tips = New KBotToolTip(components)
        grdOperatii = New KBotDataView()
        cmbAngajament = New KBotComboBox()
        cmbIndicator = New KBotComboBox()
        txtSimbolCont = New KBotTextField()
        txtSimbolContCorectie = New KBotTextField()
        txtCodProgramCorectie = New KBotTextField()
        txtNrNota = New KBotTextField()
        txtDataOper = New KBotTextField()
        txtExplicatii = New KBotTextField()
        txtDenumire = New KBotTextField()
        btnSalveaza = New Button()
        pnlCard = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        tlyDetalii = New KBotTableLayoutPanel()
        lblAngajamentCaption = New Label()
        lblIndicatorCaption = New Label()
        lblSimbolContCaption = New Label()
        lblSimbolContCorectieCaption = New Label()
        lblCodProgramCaption = New Label()
        txtCodProgram = New KBotTextField()
        lblCodProgramCorectieCaption = New Label()
        lblNrNotaCaption = New Label()
        lblDataOperCaption = New Label()
        lblExplicatiiCaption = New Label()
        lblDenumireCaption = New Label()
        lblCifCaption = New Label()
        txtCif = New KBotTextField()
        pnlJos = New Panel()
        lblStare = New Label()
        btnInchide = New Button()
        capBar = New KBotCaptionBar()
        CType(grdOperatii, ComponentModel.ISupportInitialize).BeginInit()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        tlyDetalii.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' grdOperatii
        ' 
        grdOperatii.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grdOperatii.BackColor = SystemColors.Window
        grdOperatii.ColumnFillMode = KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.CheckBox
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn1.HeaderText = "✓"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "gata"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.ValueType = KBotValueType.Boolean
        KBotDataColumn1.Width = 40
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn2.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn2.HeaderText = "Referință TREZOR"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "referinta"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 150
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn3.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn3.HeaderText = "Nr. document"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "nr_doc"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.Width = 120
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn4.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn4.HeaderText = "Dată plată"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "data"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.TextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Width = 90
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn5.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn5.HeaderText = "Tip"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "tip"
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 80
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn6.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn6.HeaderText = "Sector - Sursă - Indicator"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "ssi"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 170
        KBotDataColumn7.AggregateFormatString = Nothing
        KBotDataColumn7.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn7.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn7.DecimalPlaces = 2
        KBotDataColumn7.Format = KBotFormat.Standard
        KBotDataColumn7.FormatString = Nothing
        KBotDataColumn7.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn7.HeaderText = "Suma"
        KBotDataColumn7.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn7.Key = "suma"
        KBotDataColumn7.OptionGroup = Nothing
        KBotDataColumn7.ReadOnly = True
        KBotDataColumn7.TextAlign = ContentAlignment.MiddleRight
        KBotDataColumn7.ValueType = KBotValueType.Number
        KBotDataColumn8.AggregateFormatString = Nothing
        KBotDataColumn8.CellPadding = New Padding(4, 0, 4, 0)
        KBotDataColumn8.ColumnFont = New Font("Calibri", 9F)
        KBotDataColumn8.FormatString = Nothing
        KBotDataColumn8.HeaderFont = New Font("Calibri", 9F, FontStyle.Bold)
        KBotDataColumn8.HeaderText = "Stare"
        KBotDataColumn8.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn8.Key = "stare"
        KBotDataColumn8.OptionGroup = Nothing
        KBotDataColumn8.ReadOnly = True
        KBotDataColumn8.Width = 200
        grdOperatii.Columns.Add(KBotDataColumn1)
        grdOperatii.Columns.Add(KBotDataColumn2)
        grdOperatii.Columns.Add(KBotDataColumn3)
        grdOperatii.Columns.Add(KBotDataColumn4)
        grdOperatii.Columns.Add(KBotDataColumn5)
        grdOperatii.Columns.Add(KBotDataColumn6)
        grdOperatii.Columns.Add(KBotDataColumn7)
        grdOperatii.Columns.Add(KBotDataColumn8)
        grdOperatii.Dock = DockStyle.Fill
        grdOperatii.Location = New Point(14, 16)
        grdOperatii.Margin = New Padding(4, 6, 4, 6)
        grdOperatii.Name = "grdOperatii"
        grdOperatii.ReadOnlyGrid = True
        grdOperatii.ShrinkColumnsToFit = False
        grdOperatii.Size = New Size(1383, 428)
        grdOperatii.TabIndex = 0
        tips.SetToolTipHeader(grdOperatii, "Operațiunile «ERRRRRRRRRR»")
        tips.SetToolTipText(grdOperatii, resources.GetString("grdOperatii.ToolTipText"))
        ' 
        ' cmbAngajament
        ' 
        cmbAngajament.Dock = DockStyle.Fill
        cmbAngajament.DrawMode = DrawMode.OwnerDrawFixed
        cmbAngajament.Editable = True
        cmbAngajament.FindAsYouType = True
        cmbAngajament.FlatStyle = FlatStyle.Flat
        cmbAngajament.ItemHeight = 31
        cmbAngajament.Location = New Point(274, 15)
        cmbAngajament.Margin = New Padding(4, 3, 4, 3)
        cmbAngajament.MaxDropDownItems = 14
        cmbAngajament.Name = "cmbAngajament"
        cmbAngajament.Size = New Size(417, 37)
        cmbAngajament.TabIndex = 1
        tips.SetToolTipHeader(cmbAngajament, "Angajamentul corect")
        tips.SetToolTipText(cmbAngajament, "Angajamentele din K-BOT. Primele sunt cele care au un indicator" & vbLf & "pe aceeași sursă și clasificație ca operațiunea.")
        ' 
        ' cmbIndicator
        ' 
        cmbIndicator.Dock = DockStyle.Fill
        cmbIndicator.DrawMode = DrawMode.OwnerDrawFixed
        cmbIndicator.DropDownStyle = ComboBoxStyle.DropDownList
        cmbIndicator.FlatStyle = FlatStyle.Flat
        cmbIndicator.ItemHeight = 31
        cmbIndicator.Location = New Point(969, 15)
        cmbIndicator.Margin = New Padding(4, 3, 4, 3)
        cmbIndicator.Name = "cmbIndicator"
        cmbIndicator.Size = New Size(418, 37)
        cmbIndicator.TabIndex = 3
        tips.SetToolTipHeader(cmbIndicator, "Indicatorul angajamentului")
        tips.SetToolTipText(cmbIndicator, "Indicatorii angajamentului ales. Când are unul singur, se alege singur.")
        ' 
        ' txtSimbolCont
        ' 
        txtSimbolCont.BackColor = Color.Transparent
        txtSimbolCont.CharacterCasing = CharacterCasing.Upper
        txtSimbolCont.Dock = DockStyle.Fill
        txtSimbolCont.Location = New Point(274, 79)
        txtSimbolCont.Margin = New Padding(4, 3, 4, 3)
        txtSimbolCont.MaxLength = 15
        txtSimbolCont.Name = "txtSimbolCont"
        txtSimbolCont.ReadOnly = True
        txtSimbolCont.Size = New Size(417, 58)
        txtSimbolCont.TabIndex = 5
        txtSimbolCont.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtSimbolCont, "Simbolul contului -- rândul 1")
        tips.SetToolTipText(txtSimbolCont, "Din sursa și clasificația operațiunii (ex. 24A650401200103)." & vbLf & "Se poate corecta.")
        ' 
        ' txtSimbolContCorectie
        ' 
        txtSimbolContCorectie.BackColor = Color.Transparent
        txtSimbolContCorectie.CharacterCasing = CharacterCasing.Upper
        txtSimbolContCorectie.Dock = DockStyle.Fill
        txtSimbolContCorectie.Location = New Point(969, 79)
        txtSimbolContCorectie.Margin = New Padding(4, 3, 4, 3)
        txtSimbolContCorectie.MaxLength = 15
        txtSimbolContCorectie.Name = "txtSimbolContCorectie"
        txtSimbolContCorectie.ReadOnly = True
        txtSimbolContCorectie.Size = New Size(418, 58)
        txtSimbolContCorectie.TabIndex = 7
        txtSimbolContCorectie.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtSimbolContCorectie, "Simbolul contului -- rândul 2")
        tips.SetToolTipText(txtSimbolContCorectie, "Din sursa și clasificația indicatorului ales. Se poate corecta.")
        ' 
        ' txtCodProgramCorectie
        ' 
        txtCodProgramCorectie.BackColor = Color.Transparent
        txtCodProgramCorectie.Dock = DockStyle.Fill
        txtCodProgramCorectie.Location = New Point(969, 143)
        txtCodProgramCorectie.Margin = New Padding(4, 3, 4, 3)
        txtCodProgramCorectie.MaxLength = 10
        txtCodProgramCorectie.Name = "txtCodProgramCorectie"
        txtCodProgramCorectie.ReadOnly = True
        txtCodProgramCorectie.Size = New Size(418, 58)
        txtCodProgramCorectie.TabIndex = 11
        txtCodProgramCorectie.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtCodProgramCorectie, "Programul rândului 2")
        tips.SetToolTipText(txtCodProgramCorectie, "Programul sursei indicatorului ales (nomenclatorul de programe).")
        ' 
        ' txtNrNota
        ' 
        txtNrNota.BackColor = Color.Transparent
        txtNrNota.Dock = DockStyle.Fill
        txtNrNota.Location = New Point(274, 207)
        txtNrNota.Margin = New Padding(4, 3, 4, 3)
        txtNrNota.MaxLength = 10
        txtNrNota.Name = "txtNrNota"
        txtNrNota.Size = New Size(417, 58)
        txtNrNota.TabIndex = 13
        txtNrNota.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtNrNota, "Numărul notei de corecție")
        tips.SetToolTipText(txtNrNota, resources.GetString("txtNrNota.ToolTipText"))
        ' 
        ' txtDataOper
        ' 
        txtDataOper.BackColor = Color.Transparent
        txtDataOper.Dock = DockStyle.Fill
        txtDataOper.Location = New Point(969, 207)
        txtDataOper.Margin = New Padding(4, 3, 4, 3)
        txtDataOper.MaxLength = 10
        txtDataOper.Name = "txtDataOper"
        txtDataOper.ReadOnly = True
        txtDataOper.Size = New Size(418, 58)
        txtDataOper.TabIndex = 15
        txtDataOper.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtDataOper, "Data operațiunii inițiale")
        tips.SetToolTipText(txtDataOper, "zz.ll.aaaa -- «Dată plată» din FOREXE. Se poate corecta.")
        ' 
        ' txtExplicatii
        ' 
        txtExplicatii.BackColor = Color.Transparent
        txtExplicatii.CharacterCasing = CharacterCasing.Upper
        tlyDetalii.SetColumnSpan(txtExplicatii, 3)
        txtExplicatii.Dock = DockStyle.Fill
        txtExplicatii.Location = New Point(274, 271)
        txtExplicatii.Margin = New Padding(4, 3, 4, 3)
        txtExplicatii.MaxLength = 70
        txtExplicatii.Name = "txtExplicatii"
        txtExplicatii.ReadOnly = True
        txtExplicatii.Size = New Size(1113, 58)
        txtExplicatii.TabIndex = 17
        txtExplicatii.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtExplicatii, "Explicațiile notei")
        tips.SetToolTipText(txtExplicatii, "Cel mult 70 de caractere, fără diacritice: litere, cifre, spațiu și . , - /" & vbLf & "Aceleași pe ambele rânduri.")
        ' 
        ' txtDenumire
        ' 
        txtDenumire.BackColor = Color.Transparent
        txtDenumire.CharacterCasing = CharacterCasing.Upper
        txtDenumire.Dock = DockStyle.Fill
        txtDenumire.Location = New Point(274, 335)
        txtDenumire.Margin = New Padding(4, 3, 4, 3)
        txtDenumire.MaxLength = 30
        txtDenumire.Name = "txtDenumire"
        txtDenumire.ReadOnly = True
        txtDenumire.Size = New Size(417, 58)
        txtDenumire.TabIndex = 19
        txtDenumire.TextPadding = New Padding(12, 0, 12, 0)
        tips.SetToolTipHeader(txtDenumire, "Denumirea entității publice")
        tips.SetToolTipText(txtDenumire, "Cel mult 30 de caractere, fără diacritice: litere, cifre și spațiu.")
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Right
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1212, 10)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(189, 56)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvează tot"
        tips.SetToolTipHeader(btnSalveaza, "Nota de corecție")
        tips.SetToolTipText(btnSalveaza, resources.GetString("btnSalveaza.ToolTipText"))
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(4)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(1411, 1036)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlyCorp
        ' 
        tlyCorp.ColumnCount = 1
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(grdOperatii, 0, 0)
        tlyCorp.Controls.Add(tlyDetalii, 0, 1)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 60)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(10)
        tlyCorp.RowCount = 2
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 440F))
        tlyCorp.Size = New Size(1411, 900)
        tlyCorp.TabIndex = 1
        ' 
        ' tlyDetalii
        ' 
        tlyDetalii.ColumnCount = 4
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 270F))
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 270F))
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyDetalii.Controls.Add(lblAngajamentCaption, 0, 0)
        tlyDetalii.Controls.Add(cmbAngajament, 1, 0)
        tlyDetalii.Controls.Add(lblIndicatorCaption, 2, 0)
        tlyDetalii.Controls.Add(cmbIndicator, 3, 0)
        tlyDetalii.Controls.Add(lblSimbolContCaption, 0, 1)
        tlyDetalii.Controls.Add(txtSimbolCont, 1, 1)
        tlyDetalii.Controls.Add(lblSimbolContCorectieCaption, 2, 1)
        tlyDetalii.Controls.Add(txtSimbolContCorectie, 3, 1)
        tlyDetalii.Controls.Add(lblCodProgramCaption, 0, 2)
        tlyDetalii.Controls.Add(txtCodProgram, 1, 2)
        tlyDetalii.Controls.Add(lblCodProgramCorectieCaption, 2, 2)
        tlyDetalii.Controls.Add(txtCodProgramCorectie, 3, 2)
        tlyDetalii.Controls.Add(lblNrNotaCaption, 0, 3)
        tlyDetalii.Controls.Add(txtNrNota, 1, 3)
        tlyDetalii.Controls.Add(lblDataOperCaption, 2, 3)
        tlyDetalii.Controls.Add(txtDataOper, 3, 3)
        tlyDetalii.Controls.Add(lblExplicatiiCaption, 0, 4)
        tlyDetalii.Controls.Add(txtExplicatii, 1, 4)
        tlyDetalii.Controls.Add(lblDenumireCaption, 0, 5)
        tlyDetalii.Controls.Add(txtDenumire, 1, 5)
        tlyDetalii.Controls.Add(lblCifCaption, 2, 5)
        tlyDetalii.Controls.Add(txtCif, 3, 5)
        tlyDetalii.Dock = DockStyle.Fill
        tlyDetalii.Location = New Point(10, 450)
        tlyDetalii.Margin = New Padding(0)
        tlyDetalii.Name = "tlyDetalii"
        tlyDetalii.Padding = New Padding(0, 12, 0, 0)
        tlyDetalii.RowCount = 7
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 64F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDetalii.Size = New Size(1391, 440)
        tlyDetalii.TabIndex = 1
        ' 
        ' lblAngajamentCaption
        ' 
        lblAngajamentCaption.Dock = DockStyle.Fill
        lblAngajamentCaption.Location = New Point(4, 12)
        lblAngajamentCaption.Margin = New Padding(4, 0, 4, 0)
        lblAngajamentCaption.Name = "lblAngajamentCaption"
        lblAngajamentCaption.Size = New Size(262, 64)
        lblAngajamentCaption.TabIndex = 0
        lblAngajamentCaption.Text = "Angajament *"
        lblAngajamentCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblIndicatorCaption
        ' 
        lblIndicatorCaption.Dock = DockStyle.Fill
        lblIndicatorCaption.Location = New Point(699, 12)
        lblIndicatorCaption.Margin = New Padding(4, 0, 4, 0)
        lblIndicatorCaption.Name = "lblIndicatorCaption"
        lblIndicatorCaption.Size = New Size(262, 64)
        lblIndicatorCaption.TabIndex = 2
        lblIndicatorCaption.Text = "Indicator angajament *"
        lblIndicatorCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblSimbolContCaption
        ' 
        lblSimbolContCaption.Dock = DockStyle.Fill
        lblSimbolContCaption.Location = New Point(4, 76)
        lblSimbolContCaption.Margin = New Padding(4, 0, 4, 0)
        lblSimbolContCaption.Name = "lblSimbolContCaption"
        lblSimbolContCaption.Size = New Size(262, 64)
        lblSimbolContCaption.TabIndex = 4
        lblSimbolContCaption.Text = "Simbol cont (stornare) *"
        lblSimbolContCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblSimbolContCorectieCaption
        ' 
        lblSimbolContCorectieCaption.Dock = DockStyle.Fill
        lblSimbolContCorectieCaption.Location = New Point(699, 76)
        lblSimbolContCorectieCaption.Margin = New Padding(4, 0, 4, 0)
        lblSimbolContCorectieCaption.Name = "lblSimbolContCorectieCaption"
        lblSimbolContCorectieCaption.Size = New Size(262, 64)
        lblSimbolContCorectieCaption.TabIndex = 6
        lblSimbolContCorectieCaption.Text = "Simbol cont (corecție) *"
        lblSimbolContCorectieCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCodProgramCaption
        ' 
        lblCodProgramCaption.Dock = DockStyle.Fill
        lblCodProgramCaption.Location = New Point(4, 140)
        lblCodProgramCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodProgramCaption.Name = "lblCodProgramCaption"
        lblCodProgramCaption.Size = New Size(262, 64)
        lblCodProgramCaption.TabIndex = 8
        lblCodProgramCaption.Text = "Cod program (stornare) *"
        lblCodProgramCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCodProgram
        ' 
        txtCodProgram.BackColor = Color.Transparent
        txtCodProgram.Dock = DockStyle.Fill
        txtCodProgram.Location = New Point(274, 143)
        txtCodProgram.Margin = New Padding(4, 3, 4, 3)
        txtCodProgram.MaxLength = 10
        txtCodProgram.Name = "txtCodProgram"
        txtCodProgram.ReadOnly = True
        txtCodProgram.Size = New Size(417, 58)
        txtCodProgram.TabIndex = 9
        txtCodProgram.TextPadding = New Padding(12, 0, 12, 0)
        ' 
        ' lblCodProgramCorectieCaption
        ' 
        lblCodProgramCorectieCaption.Dock = DockStyle.Fill
        lblCodProgramCorectieCaption.Location = New Point(699, 140)
        lblCodProgramCorectieCaption.Margin = New Padding(4, 0, 4, 0)
        lblCodProgramCorectieCaption.Name = "lblCodProgramCorectieCaption"
        lblCodProgramCorectieCaption.Size = New Size(262, 64)
        lblCodProgramCorectieCaption.TabIndex = 10
        lblCodProgramCorectieCaption.Text = "Cod program (corecție) *"
        lblCodProgramCorectieCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblNrNotaCaption
        ' 
        lblNrNotaCaption.Dock = DockStyle.Fill
        lblNrNotaCaption.Location = New Point(4, 204)
        lblNrNotaCaption.Margin = New Padding(4, 0, 4, 0)
        lblNrNotaCaption.Name = "lblNrNotaCaption"
        lblNrNotaCaption.Size = New Size(262, 64)
        lblNrNotaCaption.TabIndex = 12
        lblNrNotaCaption.Text = "Număr notă *"
        lblNrNotaCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDataOperCaption
        ' 
        lblDataOperCaption.Dock = DockStyle.Fill
        lblDataOperCaption.Location = New Point(699, 204)
        lblDataOperCaption.Margin = New Padding(4, 0, 4, 0)
        lblDataOperCaption.Name = "lblDataOperCaption"
        lblDataOperCaption.Size = New Size(262, 64)
        lblDataOperCaption.TabIndex = 14
        lblDataOperCaption.Text = "Data operațiunii inițiale *"
        lblDataOperCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblExplicatiiCaption
        ' 
        lblExplicatiiCaption.Dock = DockStyle.Fill
        lblExplicatiiCaption.Location = New Point(4, 268)
        lblExplicatiiCaption.Margin = New Padding(4, 0, 4, 0)
        lblExplicatiiCaption.Name = "lblExplicatiiCaption"
        lblExplicatiiCaption.Size = New Size(262, 64)
        lblExplicatiiCaption.TabIndex = 16
        lblExplicatiiCaption.Text = "Explicații *"
        lblExplicatiiCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDenumireCaption
        ' 
        lblDenumireCaption.Dock = DockStyle.Fill
        lblDenumireCaption.Location = New Point(4, 332)
        lblDenumireCaption.Margin = New Padding(4, 0, 4, 0)
        lblDenumireCaption.Name = "lblDenumireCaption"
        lblDenumireCaption.Size = New Size(262, 64)
        lblDenumireCaption.TabIndex = 18
        lblDenumireCaption.Text = "Denumire entitate *"
        lblDenumireCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCifCaption
        ' 
        lblCifCaption.Dock = DockStyle.Fill
        lblCifCaption.Location = New Point(699, 332)
        lblCifCaption.Margin = New Padding(4, 0, 4, 0)
        lblCifCaption.Name = "lblCifCaption"
        lblCifCaption.Size = New Size(262, 64)
        lblCifCaption.TabIndex = 20
        lblCifCaption.Text = "Cod fiscal entitate *"
        lblCifCaption.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCif
        ' 
        txtCif.BackColor = Color.Transparent
        txtCif.Dock = DockStyle.Fill
        txtCif.Location = New Point(969, 335)
        txtCif.Margin = New Padding(4, 3, 4, 3)
        txtCif.MaxLength = 10
        txtCif.Name = "txtCif"
        txtCif.ReadOnly = True
        txtCif.Size = New Size(418, 58)
        txtCif.TabIndex = 21
        txtCif.TextPadding = New Padding(12, 0, 12, 0)
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Controls.Add(btnInchide)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 960)
        pnlJos.Margin = New Padding(4)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(10)
        pnlJos.Size = New Size(1411, 76)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(157, 10)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(1055, 56)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnInchide
        ' 
        btnInchide.DialogResult = DialogResult.Cancel
        btnInchide.Dock = DockStyle.Left
        btnInchide.FlatStyle = FlatStyle.Flat
        btnInchide.Location = New Point(10, 10)
        btnInchide.Margin = New Padding(0)
        btnInchide.Name = "btnInchide"
        btnInchide.Size = New Size(147, 56)
        btnInchide.TabIndex = 2
        btnInchide.Text = "Ieșire"
        btnInchide.UseVisualStyleBackColor = True
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = Nothing
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.Size = New Size(1411, 60)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Note de corecție CAB"
        ' 
        ' CabNoteForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnInchide
        ClientSize = New Size(1415, 1040)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "CabNoteForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Note de corecție CAB"
        CType(grdOperatii, ComponentModel.ISupportInitialize).EndInit()
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlyDetalii.ResumeLayout(False)
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents grdOperatii As KBotDataView
    Friend WithEvents tlyDetalii As KBotTableLayoutPanel
    Friend WithEvents lblAngajamentCaption As Label
    Friend WithEvents cmbAngajament As KBotComboBox
    Friend WithEvents lblIndicatorCaption As Label
    Friend WithEvents cmbIndicator As KBotComboBox
    Friend WithEvents lblSimbolContCaption As Label
    Friend WithEvents txtSimbolCont As KBotTextField
    Friend WithEvents lblSimbolContCorectieCaption As Label
    Friend WithEvents txtSimbolContCorectie As KBotTextField
    Friend WithEvents lblCodProgramCaption As Label
    Friend WithEvents txtCodProgram As KBotTextField
    Friend WithEvents lblCodProgramCorectieCaption As Label
    Friend WithEvents txtCodProgramCorectie As KBotTextField
    Friend WithEvents lblNrNotaCaption As Label
    Friend WithEvents txtNrNota As KBotTextField
    Friend WithEvents lblDataOperCaption As Label
    Friend WithEvents txtDataOper As KBotTextField
    Friend WithEvents lblExplicatiiCaption As Label
    Friend WithEvents txtExplicatii As KBotTextField
    Friend WithEvents lblDenumireCaption As Label
    Friend WithEvents txtDenumire As KBotTextField
    Friend WithEvents lblCifCaption As Label
    Friend WithEvents txtCif As KBotTextField
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnInchide As Button
End Class
