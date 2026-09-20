Imports KBot.Controls

' The «Jurnal» page of the settings window (slice 0072-01): the body of the former
' LogViewerForm as a UserControl -- file list on the left, filters above the grid, the raw
' block of the selected entry below in a KBotTextBox, the action buttons in the footer.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are in the
' 144 dpi the page was authored at; AutoScaleDimensions carries the same stamp.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariJurnalView
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
        Dim KBotDataColumn1 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn2 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn3 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn4 As KBotDataColumn = New KBotDataColumn()
        Dim KBotDataColumn5 As KBotDataColumn = New KBotDataColumn()
        Dim KBotGroupLevel1 As KBotGroupLevel = New KBotGroupLevel()
        tips = New KBotToolTip(components)
        navFisiere = New KBotNavList()
        chipNiveluri = New KBotChipBar()
        txtCauta = New KBotTextField()
        txtDeLa = New KBotTextField()
        txtPanaLa = New KBotTextField()
        btnReimprospateaza = New Button()
        grila = New KBotDataView()
        txtDetaliu = New KBotTextBox()
        btnDeschideDosar = New Button()
        btnGoleste = New Button()
        btnCopiaza = New Button()
        btnExporta = New Button()
        tlyMain = New KBotTableLayoutPanel()
        pnlFisiere = New Panel()
        noticeServer = New KBotNotice()
        tlyFilter = New KBotTableLayoutPanel()
        tlyFilterActual = New KBotTableLayoutPanel()
        lblCauta = New Label()
        lblDeLa = New Label()
        lblPanaLa = New Label()
        pnlGrila = New Panel()
        noticeGol = New KBotNotice()
        tlyFooter = New KBotTableLayoutPanel()
        busy = New KBotBusyBar()
        lblStare = New Label()
        tmrCautare = New Timer(components)
        CType(navFisiere, ComponentModel.ISupportInitialize).BeginInit()
        CType(chipNiveluri, ComponentModel.ISupportInitialize).BeginInit()
        CType(grila, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlFisiere.SuspendLayout()
        tlyFilter.SuspendLayout()
        tlyFilterActual.SuspendLayout()
        pnlGrila.SuspendLayout()
        tlyFooter.SuspendLayout()
        SuspendLayout()
        ' 
        ' navFisiere
        ' 
        navFisiere.Dock = DockStyle.Fill
        navFisiere.Location = New Point(0, 0)
        navFisiere.Margin = New Padding(4, 5, 4, 5)
        navFisiere.Name = "navFisiere"
        navFisiere.SelectedKey = Nothing
        navFisiere.Size = New Size(1, 520)
        navFisiere.TabIndex = 0
        tips.SetToolTipHeader(navFisiere, "Fișiere de jurnal")
        tips.SetToolTipText(navFisiere, "Alege jurnalul afișat: cele locale și grupul de pe server.")
        ' 
        ' chipNiveluri
        ' 
        chipNiveluri.ChipCornerRadius = 12
        chipNiveluri.Dock = DockStyle.Fill
        chipNiveluri.Location = New Point(4, 60)
        chipNiveluri.Margin = New Padding(4, 20, 4, 4)
        chipNiveluri.MinimumRequiredChecked = 1
        chipNiveluri.Name = "chipNiveluri"
        chipNiveluri.Size = New Size(944, 36)
        chipNiveluri.TabIndex = 1
        tips.SetToolTipHeader(chipNiveluri, "Niveluri")
        tips.SetToolTipText(chipNiveluri, "Arată doar nivelurile bifate (eroare, avertisment, informație).")
        ' 
        ' txtCauta
        ' 
        txtCauta.BackColor = Color.Transparent
        txtCauta.Dock = DockStyle.Fill
        txtCauta.Location = New Point(80, 0)
        txtCauta.Margin = New Padding(0)
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "text din linie sau din urma de stivă"
        txtCauta.Size = New Size(200, 40)
        txtCauta.TabIndex = 1
        txtCauta.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtCauta, "Caută")
        tips.SetToolTipText(txtCauta, "Text căutat în mesajele din jurnal." & vbLf & "Se caută pe măsură ce scrii.")
        ' 
        ' txtDeLa
        ' 
        txtDeLa.BackColor = Color.Transparent
        txtDeLa.Dock = DockStyle.Fill
        txtDeLa.Location = New Point(368, 0)
        txtDeLa.Margin = New Padding(0)
        txtDeLa.Name = "txtDeLa"
        txtDeLa.PlaceholderText = "zz.ll.aaaa"
        txtDeLa.Size = New Size(120, 40)
        txtDeLa.TabIndex = 3
        txtDeLa.TextAlign = HorizontalAlignment.Center
        txtDeLa.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtDeLa, "De la")
        tips.SetToolTipText(txtDeLa, "Data de început a intervalului afișat (zz.ll.aaaa).")
        ' 
        ' txtPanaLa
        ' 
        txtPanaLa.BackColor = Color.Transparent
        txtPanaLa.Dock = DockStyle.Fill
        txtPanaLa.Location = New Point(576, 0)
        txtPanaLa.Margin = New Padding(0)
        txtPanaLa.Name = "txtPanaLa"
        txtPanaLa.PlaceholderText = "zz.ll.aaaa"
        txtPanaLa.Size = New Size(120, 40)
        txtPanaLa.TabIndex = 5
        txtPanaLa.TextAlign = HorizontalAlignment.Center
        txtPanaLa.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtPanaLa, "Până la")
        tips.SetToolTipText(txtPanaLa, "Data de sfârșit a intervalului afișat (zz.ll.aaaa).")
        ' 
        ' btnReimprospateaza
        ' 
        btnReimprospateaza.BackgroundImage = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        btnReimprospateaza.BackgroundImageLayout = ImageLayout.Center
        btnReimprospateaza.Dock = DockStyle.Top
        btnReimprospateaza.FlatStyle = FlatStyle.Flat
        btnReimprospateaza.Location = New Point(903, 0)
        btnReimprospateaza.Margin = New Padding(0)
        btnReimprospateaza.Name = "btnReimprospateaza"
        btnReimprospateaza.Size = New Size(49, 40)
        btnReimprospateaza.TabIndex = 6
        tips.SetToolTipHeader(btnReimprospateaza, "Reîmprospătează")
        tips.SetToolTipText(btnReimprospateaza, "Recitește fișierul de jurnal de pe disc.")
        btnReimprospateaza.UseVisualStyleBackColor = True
        ' 
        ' grila
        ' 
        grila.AutoSizeColumnsMode = KBotAutoSizeMode.None
        grila.AutoSizeHeaderHeight = False
        grila.BackColor = SystemColors.Window
        grila.ColumnFillMode = KBotFillMode.SpecificColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.Format = KBotFormat.GeneralDateMs
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Ora"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "ora"
        KBotDataColumn1.MinWidth = 90
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
        KBotDataColumn1.ShowColumnFilter = True
        KBotDataColumn1.ValueType = KBotValueType.DateTime
        KBotDataColumn1.Width = 140
        KBotDataColumn2.AggregateFormatString = Nothing
        KBotDataColumn2.FormatString = Nothing
        KBotDataColumn2.HeaderText = "Nivel"
        KBotDataColumn2.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn2.Key = "nivel"
        KBotDataColumn2.MinWidth = 50
        KBotDataColumn2.OptionGroup = Nothing
        KBotDataColumn2.ReadOnly = True
        KBotDataColumn2.Width = 80
        KBotDataColumn3.AggregateFormatString = Nothing
        KBotDataColumn3.FormatString = Nothing
        KBotDataColumn3.HeaderText = "Sursă"
        KBotDataColumn3.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn3.Key = "sursa"
        KBotDataColumn3.MinWidth = 50
        KBotDataColumn3.OptionGroup = Nothing
        KBotDataColumn3.ReadOnly = True
        KBotDataColumn3.ShowColumnFilter = True
        KBotDataColumn3.Width = 70
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Fișier"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "fisier"
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.ShowColumnFilter = True
        KBotDataColumn4.Visible = KBotColumnVisibility.WhenRoom
        KBotDataColumn4.Width = 170
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Detaliu"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "detaliu"
        KBotDataColumn5.MinWidth = 80
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Visible = KBotColumnVisibility.WhenRoom
        KBotDataColumn5.Width = 200
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        grila.Columns.Add(KBotDataColumn3)
        grila.Columns.Add(KBotDataColumn4)
        grila.Columns.Add(KBotDataColumn5)
        grila.Dock = DockStyle.Fill
        grila.EnableGrouping = True
        grila.FillColumnKey = "sursa"
        grila.FrozenColumnCount = 1
        KBotGroupLevel1.ColumnKey = "ora"
        KBotGroupLevel1.KeyPattern = "^\S+"
        KBotGroupLevel1.ShowFooter = False
        KBotGroupLevel1.ShowFooterAggregates = False
        KBotGroupLevel1.SortDirection = KBotSortDirection.Descending
        grila.Groups.Add(KBotGroupLevel1)
        grila.HeaderHeight = 24
        grila.HeaderSeparatorColor = SystemColors.ActiveBorder
        grila.Location = New Point(0, 54)
        grila.Margin = New Padding(4, 5, 4, 5)
        grila.Name = "grila"
        grila.ReadOnlyGrid = True
        grila.ShrinkColumnsToFit = False
        grila.Size = New Size(944, 261)
        grila.TabIndex = 0
        tips.SetToolTipHeader(grila, "Intrările jurnalului")
        tips.SetToolTipText(grila, "Cele mai noi sus. Mesajul întreg al rândului selectat se citește în panoul de dedesubt.")
        ' 
        ' txtDetaliu
        ' 
        txtDetaliu.CornerRadius = 0
        txtDetaliu.Dock = DockStyle.Fill
        txtDetaliu.Font = New Font("Consolas", 9.75F)
        txtDetaliu.Location = New Point(12, 430)
        txtDetaliu.Margin = New Padding(4, 5, 4, 5)
        txtDetaliu.MaxLength = 327670
        txtDetaliu.Name = "txtDetaliu"
        txtDetaliu.ReadOnly = True
        txtDetaliu.ScrollBars = ScrollBars.Both
        txtDetaliu.Size = New Size(944, 195)
        txtDetaliu.TabIndex = 3
        tips.SetToolTipHeader(txtDetaliu, "Mesajul intrării")
        tips.SetToolTipText(txtDetaliu, "Blocul BRUT al rândului selectat, exact cum s-a scris în fișier — cu tot cu urma de stivă.")
        ' 
        ' btnDeschideDosar
        ' 
        btnDeschideDosar.Dock = DockStyle.Fill
        btnDeschideDosar.FlatStyle = FlatStyle.Flat
        btnDeschideDosar.Location = New Point(4, 52)
        btnDeschideDosar.Margin = New Padding(4, 5, 4, 5)
        btnDeschideDosar.Name = "btnDeschideDosar"
        btnDeschideDosar.Size = New Size(162, 48)
        btnDeschideDosar.TabIndex = 2
        btnDeschideDosar.Text = "Deschide dosarul"
        tips.SetToolTipHeader(btnDeschideDosar, "Deschide dosarul")
        tips.SetToolTipText(btnDeschideDosar, "Deschide în Explorer dosarul în care se scriu jurnalele.")
        btnDeschideDosar.UseVisualStyleBackColor = True
        ' 
        ' btnGoleste
        ' 
        btnGoleste.Dock = DockStyle.Fill
        btnGoleste.FlatStyle = FlatStyle.Flat
        btnGoleste.Location = New Point(174, 52)
        btnGoleste.Margin = New Padding(4, 5, 4, 5)
        btnGoleste.Name = "btnGoleste"
        btnGoleste.Size = New Size(162, 48)
        btnGoleste.TabIndex = 3
        btnGoleste.Text = "Golește jurnale…"
        tips.SetToolTipHeader(btnGoleste, "Golește")
        tips.SetToolTipText(btnGoleste, "<b>Șterge</b> fișiere de jurnal de pe disc." & vbLf & "Se cere confirmare, cu lista fișierelor și mărimea lor.")
        btnGoleste.UseVisualStyleBackColor = True
        ' 
        ' btnCopiaza
        ' 
        btnCopiaza.Dock = DockStyle.Fill
        btnCopiaza.FlatStyle = FlatStyle.Flat
        btnCopiaza.Location = New Point(624, 52)
        btnCopiaza.Margin = New Padding(4, 5, 4, 5)
        btnCopiaza.Name = "btnCopiaza"
        btnCopiaza.Size = New Size(162, 48)
        btnCopiaza.TabIndex = 4
        btnCopiaza.Text = "Copiază"
        tips.SetToolTipHeader(btnCopiaza, "Copiază")
        tips.SetToolTipText(btnCopiaza, "Pune în clipboard rândul selectat sau, fără selecție, rândurile afișate acum.")
        btnCopiaza.UseVisualStyleBackColor = True
        ' 
        ' btnExporta
        ' 
        btnExporta.Dock = DockStyle.Fill
        btnExporta.FlatStyle = FlatStyle.Flat
        btnExporta.Location = New Point(794, 52)
        btnExporta.Margin = New Padding(4, 5, 4, 5)
        btnExporta.Name = "btnExporta"
        btnExporta.Size = New Size(162, 48)
        btnExporta.TabIndex = 5
        btnExporta.Text = "Exportă"
        tips.SetToolTipHeader(btnExporta, "Exportă")
        tips.SetToolTipText(btnExporta, "Salvează într-un fișier rândurile afișate acum.")
        btnExporta.UseVisualStyleBackColor = True
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 2
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8F))
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(pnlFisiere, 0, 0)
        tlyMain.Controls.Add(tlyFilter, 1, 0)
        tlyMain.Controls.Add(pnlGrila, 1, 1)
        tlyMain.Controls.Add(txtDetaliu, 1, 2)
        tlyMain.Controls.Add(tlyFooter, 0, 3)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(0, 0)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 205F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 105F))
        tlyMain.Size = New Size(960, 735)
        tlyMain.TabIndex = 0
        ' 
        ' pnlFisiere
        ' 
        pnlFisiere.Controls.Add(navFisiere)
        pnlFisiere.Controls.Add(noticeServer)
        pnlFisiere.Dock = DockStyle.Fill
        pnlFisiere.Location = New Point(4, 5)
        pnlFisiere.Margin = New Padding(4, 5, 4, 5)
        pnlFisiere.Name = "pnlFisiere"
        tlyMain.SetRowSpan(pnlFisiere, 3)
        pnlFisiere.Size = New Size(1, 620)
        pnlFisiere.TabIndex = 0
        pnlFisiere.Tag = "Card"
        pnlFisiere.Visible = False
        ' 
        ' noticeServer
        ' 
        noticeServer.BackColor = Color.Transparent
        noticeServer.Dock = DockStyle.Bottom
        noticeServer.Location = New Point(0, 520)
        noticeServer.Margin = New Padding(4, 5, 4, 5)
        noticeServer.Name = "noticeServer"
        noticeServer.Size = New Size(1, 100)
        noticeServer.TabIndex = 1
        noticeServer.Visible = False
        ' 
        ' tlyFilter
        ' 
        tlyFilter.ColumnCount = 1
        tlyFilter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFilter.Controls.Add(tlyFilterActual, 0, 0)
        tlyFilter.Controls.Add(chipNiveluri, 0, 1)
        tlyFilter.Dock = DockStyle.Fill
        tlyFilter.Location = New Point(8, 0)
        tlyFilter.Margin = New Padding(0)
        tlyFilter.Name = "tlyFilter"
        tlyFilter.RowCount = 2
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFilter.Size = New Size(952, 100)
        tlyFilter.TabIndex = 1
        ' 
        ' tlyFilterActual
        ' 
        tlyFilterActual.ColumnCount = 10
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 49F))
        tlyFilterActual.Controls.Add(lblCauta, 0, 0)
        tlyFilterActual.Controls.Add(txtCauta, 1, 0)
        tlyFilterActual.Controls.Add(lblDeLa, 3, 0)
        tlyFilterActual.Controls.Add(txtDeLa, 4, 0)
        tlyFilterActual.Controls.Add(lblPanaLa, 6, 0)
        tlyFilterActual.Controls.Add(txtPanaLa, 7, 0)
        tlyFilterActual.Controls.Add(btnReimprospateaza, 9, 0)
        tlyFilterActual.Dock = DockStyle.Fill
        tlyFilterActual.Location = New Point(0, 0)
        tlyFilterActual.Margin = New Padding(0)
        tlyFilterActual.Name = "tlyFilterActual"
        tlyFilterActual.RowCount = 1
        tlyFilterActual.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFilterActual.Size = New Size(952, 40)
        tlyFilterActual.TabIndex = 0
        ' 
        ' lblCauta
        ' 
        lblCauta.AutoSize = True
        lblCauta.Dock = DockStyle.Fill
        lblCauta.Location = New Point(4, 0)
        lblCauta.Margin = New Padding(4, 0, 4, 0)
        lblCauta.Name = "lblCauta"
        lblCauta.Size = New Size(72, 40)
        lblCauta.TabIndex = 0
        lblCauta.Text = "Caută:"
        lblCauta.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblDeLa
        ' 
        lblDeLa.AutoSize = True
        lblDeLa.Dock = DockStyle.Fill
        lblDeLa.Location = New Point(292, 0)
        lblDeLa.Margin = New Padding(4, 0, 4, 0)
        lblDeLa.Name = "lblDeLa"
        lblDeLa.Size = New Size(72, 40)
        lblDeLa.TabIndex = 2
        lblDeLa.Text = "De la:"
        lblDeLa.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' lblPanaLa
        ' 
        lblPanaLa.AutoSize = True
        lblPanaLa.Dock = DockStyle.Fill
        lblPanaLa.Location = New Point(500, 0)
        lblPanaLa.Margin = New Padding(4, 0, 4, 0)
        lblPanaLa.Name = "lblPanaLa"
        lblPanaLa.Size = New Size(72, 40)
        lblPanaLa.TabIndex = 4
        lblPanaLa.Text = "Până la:"
        lblPanaLa.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' pnlGrila
        ' 
        pnlGrila.Controls.Add(grila)
        pnlGrila.Controls.Add(noticeGol)
        pnlGrila.Dock = DockStyle.Fill
        pnlGrila.Location = New Point(12, 105)
        pnlGrila.Margin = New Padding(4, 5, 4, 5)
        pnlGrila.Name = "pnlGrila"
        pnlGrila.Size = New Size(944, 315)
        pnlGrila.TabIndex = 2
        pnlGrila.Tag = "Card"
        ' 
        ' noticeGol
        ' 
        noticeGol.BackColor = Color.Transparent
        noticeGol.Dock = DockStyle.Top
        noticeGol.Location = New Point(0, 0)
        noticeGol.Margin = New Padding(4, 5, 4, 5)
        noticeGol.Name = "noticeGol"
        noticeGol.Size = New Size(944, 54)
        noticeGol.TabIndex = 1
        noticeGol.Visible = False
        ' 
        ' tlyFooter
        ' 
        tlyFooter.ColumnCount = 5
        tlyMain.SetColumnSpan(tlyFooter, 2)
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tlyFooter.Controls.Add(busy, 0, 0)
        tlyFooter.Controls.Add(lblStare, 0, 1)
        tlyFooter.Controls.Add(btnDeschideDosar, 0, 2)
        tlyFooter.Controls.Add(btnGoleste, 1, 2)
        tlyFooter.Controls.Add(btnCopiaza, 3, 2)
        tlyFooter.Controls.Add(btnExporta, 4, 2)
        tlyFooter.Dock = DockStyle.Fill
        tlyFooter.Location = New Point(0, 630)
        tlyFooter.Margin = New Padding(0)
        tlyFooter.Name = "tlyFooter"
        tlyFooter.RowCount = 3
        tlyFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 10F))
        tlyFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 37F))
        tlyFooter.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFooter.Size = New Size(960, 105)
        tlyFooter.TabIndex = 4
        ' 
        ' busy
        ' 
        tlyFooter.SetColumnSpan(busy, 5)
        busy.Dock = DockStyle.Fill
        busy.Location = New Point(4, 5)
        busy.Margin = New Padding(4, 5, 4, 0)
        busy.Name = "busy"
        busy.Size = New Size(952, 5)
        busy.TabIndex = 0
        busy.TabStop = False
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        tlyFooter.SetColumnSpan(lblStare, 5)
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(4, 10)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(952, 37)
        lblStare.TabIndex = 1
        lblStare.Text = "Niciun fișier încărcat."
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' tmrCautare
        ' 
        tmrCautare.Interval = 250
        ' 
        ' SetariJurnalView
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "SetariJurnalView"
        Size = New Size(960, 735)
        CType(navFisiere, ComponentModel.ISupportInitialize).EndInit()
        CType(chipNiveluri, ComponentModel.ISupportInitialize).EndInit()
        CType(grila, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlFisiere.ResumeLayout(False)
        tlyFilter.ResumeLayout(False)
        tlyFilterActual.ResumeLayout(False)
        tlyFilterActual.PerformLayout()
        pnlGrila.ResumeLayout(False)
        tlyFooter.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents pnlFisiere As Panel
    Friend WithEvents navFisiere As KBotNavList
    Friend WithEvents noticeServer As KBotNotice
    Friend WithEvents tlyFilter As KBotTableLayoutPanel
    Friend WithEvents chipNiveluri As KBotChipBar
    Friend WithEvents tlyFilterActual As KBotTableLayoutPanel
    Friend WithEvents lblCauta As Label
    Friend WithEvents txtCauta As KBotTextField
    Friend WithEvents lblDeLa As Label
    Friend WithEvents txtDeLa As KBotTextField
    Friend WithEvents lblPanaLa As Label
    Friend WithEvents txtPanaLa As KBotTextField
    Friend WithEvents btnReimprospateaza As Button
    Friend WithEvents pnlGrila As Panel
    Friend WithEvents grila As KBotDataView
    Friend WithEvents noticeGol As KBotNotice
    Friend WithEvents txtDetaliu As KBotTextBox
    Friend WithEvents tlyFooter As KBotTableLayoutPanel
    Friend WithEvents busy As KBotBusyBar
    Friend WithEvents lblStare As Label
    Friend WithEvents btnDeschideDosar As Button
    Friend WithEvents btnGoleste As Button
    Friend WithEvents btnCopiaza As Button
    Friend WithEvents btnExporta As Button
    Friend WithEvents tmrCautare As Timer
End Class
