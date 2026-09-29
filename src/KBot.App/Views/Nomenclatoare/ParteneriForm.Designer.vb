Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ParteneriForm
    Inherits Global.KBot.Theming.KBotShellForm

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
        tips = New KBotToolTip(components)
        chkFaraActivitate = New CheckBox()
        txtCod = New KBotTextField()
        txtCodFiscal = New KBotTextField()
        txtAdresa = New KBotTextField()
        gridCoduri = New KBotDataView()
        btnSterge = New Button()
        btnRenunta = New Button()
        btnAdauga = New Button()
        btnSalveaza = New Button()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tlyLeft = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        flowFiltre = New FlowLayoutPanel()
        tlyDetalii = New KBotTableLayoutPanel()
        lblCod = New Label()
        lblCodFiscal = New Label()
        lblDenumire = New Label()
        txtDenumire = New KBotTextField()
        lblIban = New Label()
        txtIban = New KBotTextField()
        lblBanca = New Label()
        txtBanca = New KBotTextField()
        lblAdresa = New Label()
        chkAscuns = New CheckBox()
        lblCoduri = New Label()
        tlySubsol = New KBotTableLayoutPanel()
        btnIesire = New Button()
        lblStare = New Label()
        CType(gridCoduri, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyLeft.SuspendLayout()
        flowFiltre.SuspendLayout()
        tlyDetalii.SuspendLayout()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' chkFaraActivitate
        ' 
        chkFaraActivitate.AutoSize = True
        chkFaraActivitate.FlatStyle = FlatStyle.Flat
        chkFaraActivitate.Location = New Point(0, 12)
        chkFaraActivitate.Margin = New Padding(0, 12, 0, 0)
        chkFaraActivitate.Name = "chkFaraActivitate"
        chkFaraActivitate.Size = New Size(274, 26)
        chkFaraActivitate.TabIndex = 0
        chkFaraActivitate.Text = "Ascunde partenerii fără activitate"
        tips.SetToolTipText(chkFaraActivitate, "Ascunde partenerii care nu apar pe niciun document (DDF sau ORD).")
        ' 
        ' txtCod
        ' 
        txtCod.BackColor = Color.Transparent
        txtCod.Dock = DockStyle.Left
        txtCod.Location = New Point(255, 4)
        txtCod.Margin = New Padding(0, 4, 0, 4)
        txtCod.MaxLength = 50
        txtCod.Name = "txtCod"
        txtCod.Size = New Size(300, 41)
        txtCod.TabIndex = 1
        txtCod.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipText(txtCod, "Unic în unitate. La «Adăugare» se propune următorul cod numeric liber.")
        ' 
        ' txtCodFiscal
        ' 
        txtCodFiscal.BackColor = Color.Transparent
        txtCodFiscal.Dock = DockStyle.Fill
        txtCodFiscal.Location = New Point(255, 53)
        txtCodFiscal.Margin = New Padding(0, 4, 0, 4)
        txtCodFiscal.Name = "txtCodFiscal"
        txtCodFiscal.Size = New Size(564, 41)
        txtCodFiscal.TabIndex = 5
        txtCodFiscal.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtCodFiscal, "Cod fiscal")
        tips.SetToolTipText(txtCodFiscal, "La ieșirea din câmp (sau Enter) se caută codul la ANAF și se completează denumirea și adresa." & vbLf & "Un cod fiscal poate avea un singur partener.")
        ' 
        ' txtAdresa
        ' 
        txtAdresa.BackColor = Color.Transparent
        txtAdresa.Dock = DockStyle.Fill
        txtAdresa.Location = New Point(255, 249)
        txtAdresa.Margin = New Padding(0, 4, 0, 4)
        txtAdresa.Name = "txtAdresa"
        txtAdresa.Size = New Size(564, 41)
        txtAdresa.TabIndex = 13
        txtAdresa.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipText(txtAdresa, "Adresa partenerului; se completează de la ANAF după codul fiscal.")
        ' 
        ' gridCoduri
        ' 
        gridCoduri.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridCoduri.BackColor = SystemColors.Window
        gridCoduri.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.Combo
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clasificatie"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 170
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Cont bancar asociat"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "cont_bancar"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 160
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Cod ang."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cod_ang"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 90
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Cod ind."
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "cod_ind"
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.Width = 90
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.ColumnType = KBotColumnType.Button
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = ""
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn5.Key = "sterge"
        KBotDataColumn5.MinWidth = 34
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 34
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Id"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleLeft
        KBotDataColumn6.Key = "id"
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Visible = KBotColumnVisibility.Hidden
        gridCoduri.Columns.Add(KBotDataColumn1)
        gridCoduri.Columns.Add(KBotDataColumn2)
        gridCoduri.Columns.Add(KBotDataColumn3)
        gridCoduri.Columns.Add(KBotDataColumn4)
        gridCoduri.Columns.Add(KBotDataColumn5)
        gridCoduri.Columns.Add(KBotDataColumn6)
        tlyDetalii.SetColumnSpan(gridCoduri, 2)
        gridCoduri.Dock = DockStyle.Fill
        gridCoduri.EnterKeyMode = KBotEnterKeyMode.NextEditableCell
        gridCoduri.FooterBackColor = SystemColors.Control
        gridCoduri.FooterHeight = 24
        gridCoduri.FooterRightIcon = My.Resources.Resources.plus_green
        gridCoduri.FooterRightIconTooltip = "Adaugă un cod de angajament" & vbLf & "Rândul nou se completează direct în tabel."
        gridCoduri.FooterSeparatorColor = SystemColors.ActiveBorder
        gridCoduri.FooterVisible = True
        gridCoduri.HeaderHeight = 24
        gridCoduri.HeaderSeparatorColor = SystemColors.ActiveBorder
        gridCoduri.Location = New Point(0, 360)
        gridCoduri.Margin = New Padding(0, 6, 0, 0)
        gridCoduri.Name = "gridCoduri"
        gridCoduri.RowHeight = 24
        gridCoduri.Size = New Size(819, 314)
        gridCoduri.TabIndex = 16
        tips.SetToolTipHeader(gridCoduri, "Coduri angajament")
        tips.SetToolTipText(gridCoduri, "O clasificație o singură dată pe partener." & vbLf & "«+» din subsol adaugă un rând; «✕» îl șterge. Totul se scrie la «Salvare».")
        ' 
        ' btnSterge
        ' 
        btnSterge.AutoSize = True
        btnSterge.Enabled = False
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Location = New Point(610, 6)
        btnSterge.Margin = New Padding(0, 0, 12, 0)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New Padding(26, 4, 26, 4)
        btnSterge.Size = New Size(184, 60)
        btnSterge.TabIndex = 2
        btnSterge.Text = "Ștergere"
        tips.SetToolTipText(btnSterge, "Șterge partenerul ales. Un partener folosit pe documente nu se poate șterge, doar ascunde.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.AutoSize = True
        btnRenunta.Enabled = False
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(806, 6)
        btnRenunta.Margin = New Padding(0, 0, 12, 0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(26, 4, 26, 4)
        btnRenunta.Size = New Size(183, 60)
        btnRenunta.TabIndex = 3
        btnRenunta.Text = "Renunță"
        tips.SetToolTipText(btnRenunta, "Renunță la modificările nesalvate ale partenerului.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnAdauga
        ' 
        btnAdauga.AutoSize = True
        btnAdauga.FlatStyle = FlatStyle.Flat
        btnAdauga.Location = New Point(1001, 6)
        btnAdauga.Margin = New Padding(0, 0, 12, 0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New Padding(26, 4, 26, 4)
        btnAdauga.Size = New Size(202, 60)
        btnAdauga.TabIndex = 4
        btnAdauga.Text = "Adăugare"
        tips.SetToolTipText(btnAdauga, "Golește câmpurile pentru un partener nou.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AutoSize = True
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(1215, 6)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(26, 4, 26, 4)
        btnSalveaza.Size = New Size(213, 60)
        btnSalveaza.TabIndex = 5
        btnSalveaza.Text = "Salvare"
        tips.SetToolTipText(btnSalveaza, "Scrie partenerul și codurile lui de angajament în baza de date.")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(2, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 78F))
        tlyMain.Size = New Size(1436, 836)
        tlyMain.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.binvoice
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1436, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Adăugare / editare parteneri"
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 66)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(12, 9, 12, 9)
        pnlCard.Size = New Size(1436, 692)
        pnlCard.TabIndex = 1
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42F))
        tlyBody.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 58F))
        tlyBody.Controls.Add(tlyLeft, 0, 0)
        tlyBody.Controls.Add(tlyDetalii, 1, 0)
        tlyBody.Dock = DockStyle.Fill
        tlyBody.Location = New Point(12, 9)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1412, 674)
        tlyBody.TabIndex = 0
        ' 
        ' tlyLeft
        ' 
        tlyLeft.ColumnCount = 1
        tlyLeft.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyLeft.Controls.Add(tree, 0, 0)
        tlyLeft.Controls.Add(flowFiltre, 0, 1)
        tlyLeft.Dock = DockStyle.Fill
        tlyLeft.Location = New Point(0, 0)
        tlyLeft.Margin = New Padding(0, 0, 15, 0)
        tlyLeft.Name = "tlyLeft"
        tlyLeft.RowCount = 2
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Absolute, 51F))
        tlyLeft.Size = New Size(578, 674)
        tlyLeft.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.BorderColor = SystemColors.ActiveBorder
        tree.Dock = DockStyle.Fill
        tree.HeaderBackColor = SystemColors.Control
        tree.HeaderCaption = " PARTENERI"
        tree.HeaderHeight = 24
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderVisible = True
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0)
        tree.Name = "tree"
        tree.RightTextColumn = 110
        tree.SearchBackColor = SystemColors.Control
        tree.SearchDefaultText = "… tastați o parte din cod sau din denumire …"
        tree.SearchSeparatorColor = SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.SearchShow = True
        tree.Size = New Size(578, 623)
        tree.TabIndex = 0
        ' 
        ' flowFiltre
        ' 
        flowFiltre.Controls.Add(chkFaraActivitate)
        flowFiltre.Dock = DockStyle.Fill
        flowFiltre.Location = New Point(0, 623)
        flowFiltre.Margin = New Padding(0)
        flowFiltre.Name = "flowFiltre"
        flowFiltre.Size = New Size(578, 51)
        flowFiltre.TabIndex = 1
        flowFiltre.WrapContents = False
        ' 
        ' tlyDetalii
        ' 
        tlyDetalii.ColumnCount = 2
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 255F))
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyDetalii.Controls.Add(lblCod, 0, 0)
        tlyDetalii.Controls.Add(txtCod, 1, 0)
        tlyDetalii.Controls.Add(lblCodFiscal, 0, 1)
        tlyDetalii.Controls.Add(txtCodFiscal, 1, 1)
        tlyDetalii.Controls.Add(lblDenumire, 0, 2)
        tlyDetalii.Controls.Add(txtDenumire, 1, 2)
        tlyDetalii.Controls.Add(lblIban, 0, 3)
        tlyDetalii.Controls.Add(txtIban, 1, 3)
        tlyDetalii.Controls.Add(lblBanca, 0, 4)
        tlyDetalii.Controls.Add(txtBanca, 1, 4)
        tlyDetalii.Controls.Add(lblAdresa, 0, 5)
        tlyDetalii.Controls.Add(txtAdresa, 1, 5)
        tlyDetalii.Controls.Add(chkAscuns, 1, 6)
        tlyDetalii.Controls.Add(lblCoduri, 0, 7)
        tlyDetalii.Controls.Add(gridCoduri, 0, 8)
        tlyDetalii.Dock = DockStyle.Fill
        tlyDetalii.Location = New Point(593, 0)
        tlyDetalii.Margin = New Padding(0)
        tlyDetalii.Name = "tlyDetalii"
        tlyDetalii.RowCount = 9
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 8F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 8F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 8F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 8F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 8F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 8F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 52F))
        tlyDetalii.Size = New Size(819, 674)
        tlyDetalii.TabIndex = 1
        ' 
        ' lblCod
        ' 
        lblCod.Dock = DockStyle.Fill
        lblCod.Location = New Point(0, 0)
        lblCod.Margin = New Padding(0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(255, 49)
        lblCod.TabIndex = 0
        lblCod.Text = "Cod partener *"
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblCodFiscal
        ' 
        lblCodFiscal.Dock = DockStyle.Fill
        lblCodFiscal.Location = New Point(0, 49)
        lblCodFiscal.Margin = New Padding(0)
        lblCodFiscal.Name = "lblCodFiscal"
        lblCodFiscal.Size = New Size(255, 49)
        lblCodFiscal.TabIndex = 4
        lblCodFiscal.Text = "Cod fiscal *"
        lblCodFiscal.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDenumire
        ' 
        lblDenumire.Dock = DockStyle.Fill
        lblDenumire.Location = New Point(0, 98)
        lblDenumire.Margin = New Padding(0)
        lblDenumire.Name = "lblDenumire"
        lblDenumire.Size = New Size(255, 49)
        lblDenumire.TabIndex = 6
        lblDenumire.Text = "Denumire partener *"
        lblDenumire.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtDenumire
        ' 
        txtDenumire.BackColor = Color.Transparent
        txtDenumire.Dock = DockStyle.Fill
        txtDenumire.Location = New Point(255, 102)
        txtDenumire.Margin = New Padding(0, 4, 0, 4)
        txtDenumire.Name = "txtDenumire"
        txtDenumire.Size = New Size(564, 41)
        txtDenumire.TabIndex = 7
        txtDenumire.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' lblIban
        ' 
        lblIban.Dock = DockStyle.Fill
        lblIban.Location = New Point(0, 147)
        lblIban.Margin = New Padding(0)
        lblIban.Name = "lblIban"
        lblIban.Size = New Size(255, 49)
        lblIban.TabIndex = 8
        lblIban.Text = "Cont IBAN principal"
        lblIban.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtIban
        ' 
        txtIban.BackColor = Color.Transparent
        txtIban.Dock = DockStyle.Fill
        txtIban.Location = New Point(255, 151)
        txtIban.Margin = New Padding(0, 4, 0, 4)
        txtIban.Name = "txtIban"
        txtIban.Size = New Size(564, 41)
        txtIban.TabIndex = 9
        txtIban.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' lblBanca
        ' 
        lblBanca.Dock = DockStyle.Fill
        lblBanca.Location = New Point(0, 196)
        lblBanca.Margin = New Padding(0)
        lblBanca.Name = "lblBanca"
        lblBanca.Size = New Size(255, 49)
        lblBanca.TabIndex = 10
        lblBanca.Text = "Banca"
        lblBanca.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtBanca
        ' 
        txtBanca.BackColor = Color.Transparent
        txtBanca.Dock = DockStyle.Fill
        txtBanca.Location = New Point(255, 200)
        txtBanca.Margin = New Padding(0, 4, 0, 4)
        txtBanca.Name = "txtBanca"
        txtBanca.Size = New Size(564, 41)
        txtBanca.TabIndex = 11
        txtBanca.TextPadding = New Padding(8, 0, 8, 0)
        ' 
        ' lblAdresa
        ' 
        lblAdresa.Dock = DockStyle.Fill
        lblAdresa.Location = New Point(0, 245)
        lblAdresa.Margin = New Padding(0)
        lblAdresa.Name = "lblAdresa"
        lblAdresa.Size = New Size(255, 49)
        lblAdresa.TabIndex = 12
        lblAdresa.Text = "Adresa"
        lblAdresa.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' chkAscuns
        ' 
        chkAscuns.AutoSize = True
        chkAscuns.FlatStyle = FlatStyle.Flat
        chkAscuns.Location = New Point(255, 294)
        chkAscuns.Margin = New Padding(0)
        chkAscuns.Name = "chkAscuns"
        chkAscuns.Size = New Size(307, 26)
        chkAscuns.TabIndex = 14
        chkAscuns.Text = "Partener ascuns (nu mai apare în liste)"
        ' 
        ' lblCoduri
        ' 
        tlyDetalii.SetColumnSpan(lblCoduri, 2)
        lblCoduri.Dock = DockStyle.Fill
        lblCoduri.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblCoduri.Location = New Point(0, 324)
        lblCoduri.Margin = New Padding(0)
        lblCoduri.Name = "lblCoduri"
        lblCoduri.Size = New Size(819, 30)
        lblCoduri.TabIndex = 15
        lblCoduri.Text = "Coduri angajament"
        lblCoduri.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' tlySubsol
        ' 
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 6
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.ColumnStyles.Add(New ColumnStyle())
        tlySubsol.Controls.Add(btnIesire, 0, 0)
        tlySubsol.Controls.Add(lblStare, 1, 0)
        tlySubsol.Controls.Add(btnSterge, 2, 0)
        tlySubsol.Controls.Add(btnRenunta, 3, 0)
        tlySubsol.Controls.Add(btnAdauga, 4, 0)
        tlySubsol.Controls.Add(btnSalveaza, 5, 0)
        tlySubsol.Dock = DockStyle.Fill
        tlySubsol.Location = New Point(0, 758)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1436, 78)
        tlySubsol.TabIndex = 2
        ' 
        ' btnIesire
        ' 
        btnIesire.AutoSize = True
        btnIesire.FlatStyle = FlatStyle.Flat
        btnIesire.Location = New Point(8, 6)
        btnIesire.Margin = New Padding(0)
        btnIesire.Name = "btnIesire"
        btnIesire.Padding = New Padding(26, 4, 26, 4)
        btnIesire.Size = New Size(165, 60)
        btnIesire.TabIndex = 0
        btnIesire.Text = "Ieșire"
        btnIesire.UseVisualStyleBackColor = True
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(185, 6)
        lblStare.Margin = New Padding(12, 0, 12, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(413, 66)
        lblStare.TabIndex = 1
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' ParteneriForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1440, 840)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1440, 840)
        Name = "ParteneriForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Adăugare / editare parteneri"
        CType(gridCoduri, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyLeft.ResumeLayout(False)
        flowFiltre.ResumeLayout(False)
        flowFiltre.PerformLayout()
        tlyDetalii.ResumeLayout(False)
        tlyDetalii.PerformLayout()
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents tlyLeft As KBotTableLayoutPanel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents flowFiltre As FlowLayoutPanel
    Friend WithEvents chkFaraActivitate As CheckBox
    Friend WithEvents tlyDetalii As KBotTableLayoutPanel
    Friend WithEvents lblCod As Label
    Friend WithEvents txtCod As KBotTextField
    Friend WithEvents lblCodFiscal As Label
    Friend WithEvents txtCodFiscal As KBotTextField
    Friend WithEvents lblDenumire As Label
    Friend WithEvents txtDenumire As KBotTextField
    Friend WithEvents lblIban As Label
    Friend WithEvents txtIban As KBotTextField
    Friend WithEvents lblBanca As Label
    Friend WithEvents txtBanca As KBotTextField
    Friend WithEvents lblAdresa As Label
    Friend WithEvents txtAdresa As KBotTextField
    Friend WithEvents chkAscuns As CheckBox
    Friend WithEvents lblCoduri As Label
    Friend WithEvents gridCoduri As KBotDataView
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnIesire As Button
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSterge As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents btnAdauga As Button
    Friend WithEvents btnSalveaza As Button
End Class
