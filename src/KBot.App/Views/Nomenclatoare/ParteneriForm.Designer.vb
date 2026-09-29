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
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlCard = New Panel()
        tlyBody = New KBotTableLayoutPanel()
        tlyLeft = New KBotTableLayoutPanel()
        tree = New AdvancedTreeControl()
        flowFiltre = New FlowLayoutPanel()
        chkFaraActivitate = New CheckBox()
        tlyDetalii = New KBotTableLayoutPanel()
        lblCod = New Label()
        txtCod = New KBotTextField()
        lblCodFiscal = New Label()
        txtCodFiscal = New KBotTextField()
        lblDenumire = New Label()
        txtDenumire = New KBotTextField()
        lblIban = New Label()
        txtIban = New KBotTextField()
        lblBanca = New Label()
        txtBanca = New KBotTextField()
        lblAdresa = New Label()
        txtAdresa = New KBotTextField()
        chkAscuns = New CheckBox()
        lblCoduri = New Label()
        gridCoduri = New KBotDataView()
        tlySubsol = New KBotTableLayoutPanel()
        btnIesire = New Button()
        lblStare = New Label()
        btnSterge = New Button()
        btnRenunta = New Button()
        btnAdauga = New Button()
        btnSalveaza = New Button()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        tlyLeft.SuspendLayout()
        flowFiltre.SuspendLayout()
        tlyDetalii.SuspendLayout()
        CType(gridCoduri, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlCard, 0, 1)
        tlyMain.Controls.Add(tlySubsol, 0, 2)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 1)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 3
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 52F))
        tlyMain.Size = New Size(1238, 718)
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
        capBar.Size = New Size(1238, 44)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Adăugare / editare parteneri"
        '
        ' pnlCard
        '
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(0, 44)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New Padding(8, 6, 8, 6)
        pnlCard.Size = New Size(1238, 622)
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
        tlyBody.Location = New Point(8, 6)
        tlyBody.Margin = New Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyBody.Size = New Size(1222, 610)
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
        tlyLeft.Margin = New Padding(0, 0, 10, 0)
        tlyLeft.Name = "tlyLeft"
        tlyLeft.RowCount = 2
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyLeft.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyLeft.Size = New Size(503, 610)
        tlyLeft.TabIndex = 0
        '
        ' tree
        '
        tree.Dock = DockStyle.Fill
        tree.HeaderCaption = " PARTENERI"
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New Size(18, 18)
        tree.HeaderLeftIcon = My.Resources.Resources.folder_open
        tree.HeaderTextAlign = ContentAlignment.MiddleLeft
        tree.HeaderVisible = True
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(0)
        tree.Name = "tree"
        tree.RightTextColumn = 110
        tree.SearchDefaultText = "… tastați o parte din cod sau din denumire …"
        tree.SearchShow = True
        tree.Size = New Size(503, 576)
        tree.TabIndex = 0
        '
        ' flowFiltre
        '
        flowFiltre.Controls.Add(chkFaraActivitate)
        flowFiltre.Dock = DockStyle.Fill
        flowFiltre.Location = New Point(0, 576)
        flowFiltre.Margin = New Padding(0)
        flowFiltre.Name = "flowFiltre"
        flowFiltre.Size = New Size(503, 34)
        flowFiltre.TabIndex = 1
        flowFiltre.WrapContents = False
        '
        ' chkFaraActivitate
        '
        chkFaraActivitate.AutoSize = True
        chkFaraActivitate.FlatStyle = FlatStyle.Flat
        chkFaraActivitate.Location = New Point(0, 8)
        chkFaraActivitate.Margin = New Padding(0, 8, 0, 0)
        chkFaraActivitate.Name = "chkFaraActivitate"
        chkFaraActivitate.Size = New Size(250, 24)
        chkFaraActivitate.TabIndex = 0
        chkFaraActivitate.Text = "Ascunde partenerii fără activitate"
        tips.SetToolTipText(chkFaraActivitate, "Ascunde partenerii care nu apar pe niciun document (DDF sau ORD).")
        '
        ' tlyDetalii
        '
        tlyDetalii.ColumnCount = 2
        tlyDetalii.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
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
        tlyDetalii.Location = New Point(513, 0)
        tlyDetalii.Margin = New Padding(0)
        tlyDetalii.Name = "tlyDetalii"
        tlyDetalii.RowCount = 9
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyDetalii.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyDetalii.Size = New Size(709, 610)
        tlyDetalii.TabIndex = 1
        '
        ' lblCod
        '
        lblCod.Dock = DockStyle.Fill
        lblCod.Location = New Point(0, 0)
        lblCod.Margin = New Padding(0)
        lblCod.Name = "lblCod"
        lblCod.Size = New Size(170, 40)
        lblCod.TabIndex = 0
        lblCod.Text = "Cod partener *"
        lblCod.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCod
        '
        txtCod.BackColor = Color.Transparent
        txtCod.Dock = DockStyle.Left
        txtCod.Location = New Point(170, 3)
        txtCod.Margin = New Padding(0, 3, 0, 3)
        txtCod.MaxLength = 50
        txtCod.Name = "txtCod"
        txtCod.Size = New Size(200, 34)
        txtCod.TabIndex = 1
        txtCod.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipText(txtCod, "Unic în unitate. La «Adăugare» se propune următorul cod numeric liber.")
        '
        ' lblCodFiscal
        '
        lblCodFiscal.Dock = DockStyle.Fill
        lblCodFiscal.Location = New Point(0, 40)
        lblCodFiscal.Margin = New Padding(0)
        lblCodFiscal.Name = "lblCodFiscal"
        lblCodFiscal.Size = New Size(170, 40)
        lblCodFiscal.TabIndex = 4
        lblCodFiscal.Text = "Cod fiscal *"
        lblCodFiscal.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtCodFiscal
        '
        txtCodFiscal.BackColor = Color.Transparent
        txtCodFiscal.Dock = DockStyle.Fill
        txtCodFiscal.Location = New Point(170, 43)
        txtCodFiscal.Margin = New Padding(0, 3, 0, 3)
        txtCodFiscal.Name = "txtCodFiscal"
        txtCodFiscal.Size = New Size(539, 34)
        txtCodFiscal.TabIndex = 5
        txtCodFiscal.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtCodFiscal, "Cod fiscal")
        tips.SetToolTipText(txtCodFiscal, "La ieșirea din câmp (sau Enter) se caută codul la ANAF și se completează denumirea și adresa." & vbLf & "Un cod fiscal poate avea un singur partener.")
        '
        ' lblDenumire
        '
        lblDenumire.Dock = DockStyle.Fill
        lblDenumire.Location = New Point(0, 80)
        lblDenumire.Margin = New Padding(0)
        lblDenumire.Name = "lblDenumire"
        lblDenumire.Size = New Size(170, 40)
        lblDenumire.TabIndex = 6
        lblDenumire.Text = "Denumire partener *"
        lblDenumire.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtDenumire
        '
        txtDenumire.BackColor = Color.Transparent
        txtDenumire.Dock = DockStyle.Fill
        txtDenumire.Location = New Point(170, 83)
        txtDenumire.Margin = New Padding(0, 3, 0, 3)
        txtDenumire.Name = "txtDenumire"
        txtDenumire.Size = New Size(539, 34)
        txtDenumire.TabIndex = 7
        txtDenumire.TextPadding = New Padding(8, 0, 8, 0)
        '
        ' lblIban
        '
        lblIban.Dock = DockStyle.Fill
        lblIban.Location = New Point(0, 120)
        lblIban.Margin = New Padding(0)
        lblIban.Name = "lblIban"
        lblIban.Size = New Size(170, 40)
        lblIban.TabIndex = 8
        lblIban.Text = "Cont IBAN principal"
        lblIban.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtIban
        '
        txtIban.BackColor = Color.Transparent
        txtIban.Dock = DockStyle.Fill
        txtIban.Location = New Point(170, 123)
        txtIban.Margin = New Padding(0, 3, 0, 3)
        txtIban.Name = "txtIban"
        txtIban.Size = New Size(539, 34)
        txtIban.TabIndex = 9
        txtIban.TextPadding = New Padding(8, 0, 8, 0)
        '
        ' lblBanca
        '
        lblBanca.Dock = DockStyle.Fill
        lblBanca.Location = New Point(0, 160)
        lblBanca.Margin = New Padding(0)
        lblBanca.Name = "lblBanca"
        lblBanca.Size = New Size(170, 40)
        lblBanca.TabIndex = 10
        lblBanca.Text = "Banca"
        lblBanca.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtBanca
        '
        txtBanca.BackColor = Color.Transparent
        txtBanca.Dock = DockStyle.Fill
        txtBanca.Location = New Point(170, 163)
        txtBanca.Margin = New Padding(0, 3, 0, 3)
        txtBanca.Name = "txtBanca"
        txtBanca.Size = New Size(539, 34)
        txtBanca.TabIndex = 11
        txtBanca.TextPadding = New Padding(8, 0, 8, 0)
        '
        ' lblAdresa
        '
        lblAdresa.Dock = DockStyle.Fill
        lblAdresa.Location = New Point(0, 200)
        lblAdresa.Margin = New Padding(0)
        lblAdresa.Name = "lblAdresa"
        lblAdresa.Size = New Size(170, 40)
        lblAdresa.TabIndex = 12
        lblAdresa.Text = "Adresa"
        lblAdresa.TextAlign = ContentAlignment.MiddleLeft
        '
        ' txtAdresa
        '
        txtAdresa.BackColor = Color.Transparent
        txtAdresa.Dock = DockStyle.Fill
        txtAdresa.Location = New Point(170, 203)
        txtAdresa.Margin = New Padding(0, 3, 0, 3)
        txtAdresa.Name = "txtAdresa"
        txtAdresa.Size = New Size(539, 34)
        txtAdresa.TabIndex = 13
        txtAdresa.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipText(txtAdresa, "Adresa partenerului; se completează de la ANAF după codul fiscal.")
        '
        ' chkAscuns
        '
        chkAscuns.AutoSize = True
        chkAscuns.FlatStyle = FlatStyle.Flat
        chkAscuns.Location = New Point(170, 248)
        chkAscuns.Margin = New Padding(0, 8, 0, 0)
        chkAscuns.Name = "chkAscuns"
        chkAscuns.Size = New Size(260, 24)
        chkAscuns.TabIndex = 14
        chkAscuns.Text = "Partener ascuns (nu mai apare în liste)"
        '
        ' lblCoduri
        '
        tlyDetalii.SetColumnSpan(lblCoduri, 2)
        lblCoduri.Dock = DockStyle.Fill
        lblCoduri.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lblCoduri.Location = New Point(0, 274)
        lblCoduri.Margin = New Padding(0)
        lblCoduri.Name = "lblCoduri"
        lblCoduri.Size = New Size(709, 34)
        lblCoduri.TabIndex = 15
        lblCoduri.Text = "Coduri angajament"
        lblCoduri.TextAlign = ContentAlignment.BottomLeft
        '
        ' gridCoduri
        '
        gridCoduri.AutoSizeColumnsMode = KBotAutoSizeMode.None
        gridCoduri.ColumnFillMode = KBotFillMode.FirstColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.ColumnType = KBotColumnType.Combo
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Clasificație"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "clasificatie"
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.Width = 230
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Cont bancar asociat"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "cont_bancar"
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.Width = 200
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Cod ang."
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "cod_ang"
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.Width = 100
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
        KBotDataColumn5.Key = "sterge"
        KBotDataColumn5.MinWidth = 34
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.Resizable = False
        KBotDataColumn5.Width = 34
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Id"
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
        gridCoduri.FooterRightIcon = My.Resources.Resources.plus_green
        gridCoduri.FooterRightIconTooltip = "Adaugă un cod de angajament" & vbLf & "Rândul nou se completează direct în tabel."
        gridCoduri.FooterVisible = True
        gridCoduri.Location = New Point(0, 308)
        gridCoduri.Margin = New Padding(0, 4, 0, 0)
        gridCoduri.Name = "gridCoduri"
        gridCoduri.Size = New Size(709, 262)
        gridCoduri.TabIndex = 16
        tips.SetToolTipHeader(gridCoduri, "Coduri angajament")
        tips.SetToolTipText(gridCoduri, "O clasificație o singură dată pe partener." & vbLf & "«+» din subsol adaugă un rând; «✕» îl șterge. Totul se scrie la «Salvare».")
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
        tlySubsol.Location = New Point(0, 666)
        tlySubsol.Margin = New Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySubsol.Size = New Size(1238, 52)
        tlySubsol.TabIndex = 2
        '
        ' btnIesire
        '
        btnIesire.AutoSize = True
        btnIesire.FlatStyle = FlatStyle.Flat
        btnIesire.Location = New Point(8, 6)
        btnIesire.Margin = New Padding(0)
        btnIesire.Name = "btnIesire"
        btnIesire.Padding = New Padding(17, 3, 17, 3)
        btnIesire.Size = New Size(110, 40)
        btnIesire.TabIndex = 0
        btnIesire.Text = "Ieșire"
        btnIesire.UseVisualStyleBackColor = True
        '
        ' lblStare
        '
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(126, 6)
        lblStare.Margin = New Padding(8, 0, 8, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(560, 40)
        lblStare.TabIndex = 1
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        '
        ' btnSterge
        '
        btnSterge.AutoSize = True
        btnSterge.Enabled = False
        btnSterge.FlatStyle = FlatStyle.Flat
        btnSterge.Location = New Point(694, 6)
        btnSterge.Margin = New Padding(0, 0, 8, 0)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New Padding(17, 3, 17, 3)
        btnSterge.Size = New Size(120, 40)
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
        btnRenunta.Location = New Point(822, 6)
        btnRenunta.Margin = New Padding(0, 0, 8, 0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New Padding(17, 3, 17, 3)
        btnRenunta.Size = New Size(120, 40)
        btnRenunta.TabIndex = 3
        btnRenunta.Text = "Renunță"
        tips.SetToolTipText(btnRenunta, "Renunță la modificările nesalvate ale partenerului.")
        btnRenunta.UseVisualStyleBackColor = True
        '
        ' btnAdauga
        '
        btnAdauga.AutoSize = True
        btnAdauga.FlatStyle = FlatStyle.Flat
        btnAdauga.Location = New Point(950, 6)
        btnAdauga.Margin = New Padding(0, 0, 8, 0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New Padding(17, 3, 17, 3)
        btnAdauga.Size = New Size(130, 40)
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
        btnSalveaza.Location = New Point(1088, 6)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New Padding(17, 3, 17, 3)
        btnSalveaza.Size = New Size(142, 40)
        btnSalveaza.TabIndex = 5
        btnSalveaza.Text = "Salvare"
        tips.SetToolTipText(btnSalveaza, "Scrie partenerul și codurile lui de angajament în baza de date.")
        btnSalveaza.UseVisualStyleBackColor = True
        '
        ' ParteneriForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1240, 720)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        MinimizeBox = False
        MinimumSize = New Size(960, 560)
        Name = "ParteneriForm"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Adăugare / editare parteneri"
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        tlyLeft.ResumeLayout(False)
        flowFiltre.ResumeLayout(False)
        flowFiltre.PerformLayout()
        tlyDetalii.ResumeLayout(False)
        tlyDetalii.PerformLayout()
        CType(gridCoduri, ComponentModel.ISupportInitialize).EndInit()
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
