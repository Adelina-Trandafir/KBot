<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LogViewerForm
    Inherits KBot.Theming.KBotShellForm

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
        tips = New KBot.Controls.KBotToolTip(components)
        Dim KBotDataColumn1 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn2 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn3 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn4 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn5 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        Dim KBotDataColumn6 As KBot.Controls.KBotDataColumn = New Controls.KBotDataColumn()
        pnlFisiere = New Panel()
        navFisiere = New Controls.KBotNavList()
        noticeServer = New Controls.KBotNotice()
        pnlGrila = New Panel()
        grila = New Controls.KBotDataView()
        noticeGol = New Controls.KBotNotice()
        txtDetaliu = New TextBox()
        lblStare = New Label()
        busy = New Controls.KBotBusyBar()
        btnCopiaza = New Button()
        btnExporta = New Button()
        btnDeschideDosar = New Button()
        btnGoleste = New Button()
        chipNiveluri = New Controls.KBotChipBar()
        lblCauta = New Label()
        txtCauta = New Controls.KBotTextField()
        lblDeLa = New Label()
        txtDeLa = New Controls.KBotTextField()
        lblPanaLa = New Label()
        txtPanaLa = New Controls.KBotTextField()
        btnReimprospateaza = New Button()
        tmrCautare = New Timer(components)
        tlyMain = New TableLayoutPanel()
        capBar = New Controls.KBotCaptionBar()
        tlyFilter = New TableLayoutPanel()
        tlyFilterActual = New TableLayoutPanel()
        tlyFooter = New TableLayoutPanel()
        pnlFisiere.SuspendLayout()
        CType(navFisiere, ComponentModel.ISupportInitialize).BeginInit()
        pnlGrila.SuspendLayout()
        CType(grila, ComponentModel.ISupportInitialize).BeginInit()
        CType(chipNiveluri, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        tlyFilter.SuspendLayout()
        tlyFilterActual.SuspendLayout()
        tlyFooter.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlFisiere
        ' 
        pnlFisiere.Controls.Add(navFisiere)
        pnlFisiere.Controls.Add(noticeServer)
        pnlFisiere.Dock = DockStyle.Fill
        pnlFisiere.Location = New Point(4, 85)
        pnlFisiere.Margin = New Padding(4, 5, 4, 5)
        pnlFisiere.Name = "pnlFisiere"
        tlyMain.SetRowSpan(pnlFisiere, 3)
        pnlFisiere.Size = New Size(242, 619)
        pnlFisiere.TabIndex = 0
        pnlFisiere.Tag = "Card"
        ' 
        ' navFisiere
        ' 
        navFisiere.Dock = DockStyle.Fill
        navFisiere.Location = New Point(0, 0)
        navFisiere.Margin = New Padding(4, 5, 4, 5)
        navFisiere.Name = "navFisiere"
        navFisiere.SelectedKey = Nothing
        navFisiere.Size = New Size(242, 519)
        navFisiere.TabIndex = 0
        ' 
        ' noticeServer
        ' 
        noticeServer.BackColor = Color.Transparent
        noticeServer.Dock = DockStyle.Bottom
        noticeServer.Location = New Point(0, 519)
        noticeServer.Margin = New Padding(4, 5, 4, 5)
        noticeServer.Name = "noticeServer"
        noticeServer.Size = New Size(242, 100)
        noticeServer.TabIndex = 1
        noticeServer.Visible = False
        ' 
        ' pnlGrila
        ' 
        pnlGrila.Controls.Add(grila)
        pnlGrila.Controls.Add(noticeGol)
        pnlGrila.Dock = DockStyle.Fill
        pnlGrila.Location = New Point(254, 209)
        pnlGrila.Margin = New Padding(4, 5, 4, 5)
        pnlGrila.Name = "pnlGrila"
        pnlGrila.Size = New Size(915, 315)
        pnlGrila.TabIndex = 0
        pnlGrila.Tag = "Card"
        ' 
        ' grila
        ' 
        grila.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.None
        grila.BackColor = SystemColors.Window
        grila.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        KBotDataColumn1.AggregateFormatString = Nothing
        KBotDataColumn1.FormatString = Nothing
        KBotDataColumn1.HeaderText = "Ora"
        KBotDataColumn1.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn1.Key = "ora"
        KBotDataColumn1.MinWidth = 90
        KBotDataColumn1.OptionGroup = Nothing
        KBotDataColumn1.ReadOnly = True
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
        KBotDataColumn3.Width = 70
        KBotDataColumn4.AggregateFormatString = Nothing
        KBotDataColumn4.FormatString = Nothing
        KBotDataColumn4.HeaderText = "Fișier"
        KBotDataColumn4.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn4.Key = "fisier"
        KBotDataColumn4.MinWidth = 80
        KBotDataColumn4.OptionGroup = Nothing
        KBotDataColumn4.ReadOnly = True
        KBotDataColumn4.Width = 170
        KBotDataColumn5.AggregateFormatString = Nothing
        KBotDataColumn5.FormatString = Nothing
        KBotDataColumn5.HeaderText = "Detaliu"
        KBotDataColumn5.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn5.Key = "detaliu"
        KBotDataColumn5.MinWidth = 80
        KBotDataColumn5.OptionGroup = Nothing
        KBotDataColumn5.ReadOnly = True
        KBotDataColumn5.Width = 200
        KBotDataColumn6.AggregateFormatString = Nothing
        KBotDataColumn6.FormatString = Nothing
        KBotDataColumn6.HeaderText = "Mesaj"
        KBotDataColumn6.HeaderTextAlign = ContentAlignment.MiddleCenter
        KBotDataColumn6.Key = "mesaj"
        KBotDataColumn6.MinWidth = 120
        KBotDataColumn6.OptionGroup = Nothing
        KBotDataColumn6.ReadOnly = True
        KBotDataColumn6.Width = 320
        grila.Columns.Add(KBotDataColumn1)
        grila.Columns.Add(KBotDataColumn2)
        grila.Columns.Add(KBotDataColumn3)
        grila.Columns.Add(KBotDataColumn4)
        grila.Columns.Add(KBotDataColumn5)
        grila.Columns.Add(KBotDataColumn6)
        grila.Dock = DockStyle.Fill
        grila.FrozenColumnCount = 1
        grila.Location = New Point(0, 64)
        grila.Margin = New Padding(4, 5, 4, 5)
        grila.Name = "grila"
        grila.ReadOnlyGrid = True
        grila.Size = New Size(915, 251)
        grila.TabIndex = 0
        ' 
        ' noticeGol
        ' 
        noticeGol.BackColor = Color.Transparent
        noticeGol.Dock = DockStyle.Top
        noticeGol.Location = New Point(0, 0)
        noticeGol.Margin = New Padding(4, 5, 4, 5)
        noticeGol.Name = "noticeGol"
        noticeGol.Size = New Size(915, 64)
        noticeGol.TabIndex = 1
        noticeGol.Visible = False
        ' 
        ' txtDetaliu
        ' 
        txtDetaliu.BorderStyle = BorderStyle.None
        txtDetaliu.Dock = DockStyle.Fill
        txtDetaliu.Font = New Font("Consolas", 9.75F)
        txtDetaliu.Location = New Point(254, 534)
        txtDetaliu.Margin = New Padding(4, 5, 4, 5)
        txtDetaliu.Multiline = True
        txtDetaliu.Name = "txtDetaliu"
        txtDetaliu.ReadOnly = True
        txtDetaliu.ScrollBars = ScrollBars.Both
        txtDetaliu.Size = New Size(915, 170)
        txtDetaliu.TabIndex = 0
        txtDetaliu.WordWrap = False
        ' 
        ' lblStare
        ' 
        tlyFooter.SetColumnSpan(lblStare, 5)
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(4, 10)
        lblStare.Margin = New Padding(4, 0, 4, 0)
        lblStare.Name = "lblStare"
        lblStare.Padding = New Padding(0, 13, 0, 0)
        lblStare.Size = New Size(1165, 75)
        lblStare.TabIndex = 1
        lblStare.Text = "Niciun fișier încărcat."
        ' 
        ' busy
        ' 
        tlyFooter.SetColumnSpan(busy, 5)
        busy.Dock = DockStyle.Fill
        busy.Location = New Point(4, 5)
        busy.Margin = New Padding(4, 5, 4, 5)
        busy.Name = "busy"
        busy.Size = New Size(1165, 1)
        busy.TabIndex = 0
        ' 
        ' btnCopiaza
        ' 
        btnCopiaza.Dock = DockStyle.Fill
        btnCopiaza.FlatStyle = FlatStyle.Flat
        btnCopiaza.Location = New Point(853, 85)
        btnCopiaza.Margin = New Padding(0)
        btnCopiaza.Name = "btnCopiaza"
        btnCopiaza.Size = New Size(160, 75)
        btnCopiaza.TabIndex = 0
        btnCopiaza.Text = "Copiază"
        btnCopiaza.UseVisualStyleBackColor = True
        ' 
        ' btnExporta
        ' 
        btnExporta.Dock = DockStyle.Fill
        btnExporta.FlatStyle = FlatStyle.Flat
        btnExporta.Location = New Point(1013, 85)
        btnExporta.Margin = New Padding(0)
        btnExporta.Name = "btnExporta"
        btnExporta.Size = New Size(160, 75)
        btnExporta.TabIndex = 1
        btnExporta.Text = "Exportă"
        btnExporta.UseVisualStyleBackColor = True
        ' 
        ' btnDeschideDosar
        ' 
        btnDeschideDosar.Dock = DockStyle.Fill
        btnDeschideDosar.FlatStyle = FlatStyle.Flat
        btnDeschideDosar.Location = New Point(0, 85)
        btnDeschideDosar.Margin = New Padding(0)
        btnDeschideDosar.Name = "btnDeschideDosar"
        btnDeschideDosar.Size = New Size(160, 75)
        btnDeschideDosar.TabIndex = 2
        btnDeschideDosar.Text = "Deschide dosarul"
        btnDeschideDosar.UseVisualStyleBackColor = True
        ' 
        ' btnGoleste
        ' 
        btnGoleste.Dock = DockStyle.Fill
        btnGoleste.FlatStyle = FlatStyle.Flat
        btnGoleste.Location = New Point(160, 85)
        btnGoleste.Margin = New Padding(0)
        btnGoleste.Name = "btnGoleste"
        btnGoleste.Size = New Size(160, 75)
        btnGoleste.TabIndex = 3
        btnGoleste.Text = "Golește jurnale…"
        btnGoleste.UseVisualStyleBackColor = True
        ' 
        ' chipNiveluri
        ' 
        chipNiveluri.ChipCornerRadius = 12
        tlyFilter.SetColumnSpan(chipNiveluri, 2)
        chipNiveluri.Dock = DockStyle.Fill
        chipNiveluri.Location = New Point(4, 80)
        chipNiveluri.Margin = New Padding(4, 20, 4, 4)
        chipNiveluri.MinimumRequiredChecked = 1
        chipNiveluri.Name = "chipNiveluri"
        chipNiveluri.Size = New Size(915, 40)
        chipNiveluri.TabIndex = 2
        ' 
        ' lblCauta
        ' 
        lblCauta.AutoSize = True
        lblCauta.Dock = DockStyle.Fill
        lblCauta.Location = New Point(4, 0)
        lblCauta.Margin = New Padding(4, 0, 4, 0)
        lblCauta.Name = "lblCauta"
        lblCauta.Size = New Size(72, 60)
        lblCauta.TabIndex = 0
        lblCauta.Text = "Caută:"
        lblCauta.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCauta
        ' 
        txtCauta.BackColor = Color.Transparent
        txtCauta.Dock = DockStyle.Fill
        txtCauta.Location = New Point(80, 0)
        txtCauta.Margin = New Padding(0)
        txtCauta.MaxLength = 32767
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "text din linie sau din urma de stivă"
        txtCauta.Size = New Size(200, 60)
        txtCauta.TabIndex = 1
        txtCauta.TabStop = False
        txtCauta.UseSystemPasswordChar = False
        ' 
        ' lblDeLa
        ' 
        lblDeLa.AutoSize = True
        lblDeLa.Dock = DockStyle.Fill
        lblDeLa.Location = New Point(292, 0)
        lblDeLa.Margin = New Padding(4, 0, 4, 0)
        lblDeLa.Name = "lblDeLa"
        lblDeLa.Size = New Size(72, 60)
        lblDeLa.TabIndex = 2
        lblDeLa.Text = "De la:"
        lblDeLa.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtDeLa
        ' 
        txtDeLa.BackColor = Color.Transparent
        txtDeLa.Dock = DockStyle.Fill
        txtDeLa.Location = New Point(368, 0)
        txtDeLa.Margin = New Padding(0)
        txtDeLa.MaxLength = 32767
        txtDeLa.Name = "txtDeLa"
        txtDeLa.PlaceholderText = "zz.ll.aaaa"
        txtDeLa.Size = New Size(120, 60)
        txtDeLa.TabIndex = 3
        txtDeLa.TabStop = False
        txtDeLa.UseSystemPasswordChar = False
        ' 
        ' lblPanaLa
        ' 
        lblPanaLa.AutoSize = True
        lblPanaLa.Dock = DockStyle.Fill
        lblPanaLa.Location = New Point(500, 0)
        lblPanaLa.Margin = New Padding(4, 0, 4, 0)
        lblPanaLa.Name = "lblPanaLa"
        lblPanaLa.Size = New Size(72, 60)
        lblPanaLa.TabIndex = 4
        lblPanaLa.Text = "Până la:"
        lblPanaLa.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtPanaLa
        ' 
        txtPanaLa.BackColor = Color.Transparent
        txtPanaLa.Dock = DockStyle.Fill
        txtPanaLa.Location = New Point(576, 0)
        txtPanaLa.Margin = New Padding(0)
        txtPanaLa.MaxLength = 32767
        txtPanaLa.Name = "txtPanaLa"
        txtPanaLa.PlaceholderText = "zz.ll.aaaa"
        txtPanaLa.Size = New Size(120, 60)
        txtPanaLa.TabIndex = 5
        txtPanaLa.TabStop = False
        txtPanaLa.UseSystemPasswordChar = False
        ' 
        ' btnReimprospateaza
        ' 
        btnReimprospateaza.Dock = DockStyle.Fill
        btnReimprospateaza.FlatStyle = FlatStyle.Flat
        btnReimprospateaza.Location = New Point(803, 0)
        btnReimprospateaza.Margin = New Padding(0)
        btnReimprospateaza.Name = "btnReimprospateaza"
        btnReimprospateaza.Size = New Size(120, 60)
        btnReimprospateaza.TabIndex = 6
        btnReimprospateaza.Text = "Reîmprospătează"
        btnReimprospateaza.UseVisualStyleBackColor = True
        ' 
        ' tmrCautare
        ' 
        tmrCautare.Interval = 250
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 2
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 21.3128738F))
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 78.6871262F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlFisiere, 0, 1)
        tlyMain.Controls.Add(tlyFilter, 1, 1)
        tlyMain.Controls.Add(pnlGrila, 1, 2)
        tlyMain.Controls.Add(txtDetaliu, 1, 3)
        tlyMain.Controls.Add(tlyFooter, 0, 4)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(1, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 5
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 80F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 124F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 180F))
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Absolute, 160F))
        tlyMain.Size = New Size(1173, 869)
        tlyMain.TabIndex = 1
        ' 
        ' capBar
        ' 
        tlyMain.SetColumnSpan(capBar, 2)
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_64
        capBar.Location = New Point(4, 5)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowMinimize = True
        capBar.Size = New Size(1165, 70)
        capBar.TabIndex = 2
        capBar.TabStop = False
        capBar.Text = "Jurnale"
        ' 
        ' tlyFilter
        ' 
        tlyFilter.ColumnCount = 2
        tlyFilter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyFilter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        tlyFilter.Controls.Add(chipNiveluri, 0, 1)
        tlyFilter.Controls.Add(tlyFilterActual, 0, 0)
        tlyFilter.Dock = DockStyle.Fill
        tlyFilter.Location = New Point(250, 80)
        tlyFilter.Margin = New Padding(0)
        tlyFilter.Name = "tlyFilter"
        tlyFilter.RowCount = 2
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Absolute, 60F))
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFilter.Size = New Size(923, 124)
        tlyFilter.TabIndex = 0
        ' 
        ' tlyFilterActual
        ' 
        tlyFilterActual.ColumnCount = 10
        tlyFilter.SetColumnSpan(tlyFilterActual, 2)
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 200F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 80F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFilterActual.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120F))
        tlyFilterActual.Controls.Add(btnReimprospateaza, 9, 0)
        tlyFilterActual.Controls.Add(txtPanaLa, 7, 0)
        tlyFilterActual.Controls.Add(lblPanaLa, 6, 0)
        tlyFilterActual.Controls.Add(txtDeLa, 4, 0)
        tlyFilterActual.Controls.Add(lblDeLa, 3, 0)
        tlyFilterActual.Controls.Add(txtCauta, 1, 0)
        tlyFilterActual.Controls.Add(lblCauta, 0, 0)
        tlyFilterActual.Dock = DockStyle.Fill
        tlyFilterActual.Location = New Point(0, 0)
        tlyFilterActual.Margin = New Padding(0)
        tlyFilterActual.Name = "tlyFilterActual"
        tlyFilterActual.RowCount = 1
        tlyFilterActual.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFilterActual.Size = New Size(923, 60)
        tlyFilterActual.TabIndex = 3
        ' 
        ' tlyFooter
        ' 
        tlyFooter.ColumnCount = 5
        tlyMain.SetColumnSpan(tlyFooter, 2)
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyFooter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160F))
        tlyFooter.Controls.Add(btnExporta, 4, 2)
        tlyFooter.Controls.Add(btnCopiaza, 3, 2)
        tlyFooter.Controls.Add(lblStare, 0, 1)
        tlyFooter.Controls.Add(busy, 0, 0)
        tlyFooter.Controls.Add(btnDeschideDosar, 0, 2)
        tlyFooter.Controls.Add(btnGoleste, 1, 2)
        tlyFooter.Dock = DockStyle.Fill
        tlyFooter.Location = New Point(0, 709)
        tlyFooter.Margin = New Padding(0)
        tlyFooter.Name = "tlyFooter"
        tlyFooter.RowCount = 3
        tlyFooter.RowStyles.Add(New RowStyle(SizeType.Absolute, 10F))
        tlyFooter.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlyFooter.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        tlyFooter.Size = New Size(1173, 160)
        tlyFooter.TabIndex = 3
        ' 
        ' LogViewerForm
        ' 
        AutoScaleDimensions = New SizeF(9F, 22F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1175, 873)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(1086, 767)
        Name = "LogViewerForm"
        Padding = New Padding(1, 2, 1, 2)
        StartPosition = FormStartPosition.CenterScreen
        Text = "Jurnale"
        pnlFisiere.ResumeLayout(False)
        CType(navFisiere, ComponentModel.ISupportInitialize).EndInit()
        pnlGrila.ResumeLayout(False)
        CType(grila, ComponentModel.ISupportInitialize).EndInit()
        CType(chipNiveluri, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        tlyMain.PerformLayout()
        tlyFilter.ResumeLayout(False)
        tlyFilterActual.ResumeLayout(False)
        tlyFilterActual.PerformLayout()
        tlyFooter.ResumeLayout(False)
        '
        ' tips — etichetele de survolare (felia 0035), toate în română.
        '
        tips.SetToolTipHeader(txtCauta, "Caută")
        tips.SetToolTipText(txtCauta, "Text căutat în mesajele din jurnal." & vbLf & "Se caută pe măsură ce scrii.")
        tips.SetToolTipHeader(txtDeLa, "De la")
        tips.SetToolTipText(txtDeLa, "Data de început a intervalului afișat (zz.ll.aaaa).")
        tips.SetToolTipHeader(txtPanaLa, "Până la")
        tips.SetToolTipText(txtPanaLa, "Data de sfârșit a intervalului afișat (zz.ll.aaaa).")
        tips.SetToolTipHeader(btnReimprospateaza, "Reîmprospătează")
        tips.SetToolTipText(btnReimprospateaza, "Recitește fișierul de jurnal de pe disc.")
        tips.SetToolTipHeader(chipNiveluri, "Niveluri")
        tips.SetToolTipText(chipNiveluri, "Arată doar nivelurile bifate (eroare, avertisment, informație).")
        tips.SetToolTipHeader(navFisiere, "Fișiere de jurnal")
        tips.SetToolTipText(navFisiere, "Alege jurnalul afișat: cele locale și grupul de pe server.")
        tips.SetToolTipHeader(btnCopiaza, "Copiază")
        tips.SetToolTipText(btnCopiaza, "Pune în clipboard rândurile afișate acum, cu filtrele aplicate.")
        tips.SetToolTipHeader(btnExporta, "Exportă")
        tips.SetToolTipText(btnExporta, "Salvează într-un fișier rândurile afișate acum.")
        tips.SetToolTipHeader(btnDeschideDosar, "Deschide dosarul")
        tips.SetToolTipText(btnDeschideDosar, "Deschide în Explorer dosarul în care se scriu jurnalele.")
        tips.SetToolTipHeader(btnGoleste, "Golește")
        tips.SetToolTipText(btnGoleste, "<b>Șterge</b> fișiere de jurnal de pe disc." & vbLf & "Se cere confirmare, cu lista fișierelor și mărimea lor.")
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBot.Controls.KBotToolTip
    Friend WithEvents lblCauta As Label
    Friend WithEvents txtCauta As KBot.Controls.KBotTextField
    Friend WithEvents lblDeLa As Label
    Friend WithEvents txtDeLa As KBot.Controls.KBotTextField
    Friend WithEvents lblPanaLa As Label
    Friend WithEvents txtPanaLa As KBot.Controls.KBotTextField
    Friend WithEvents btnReimprospateaza As Button
    Friend WithEvents chipNiveluri As KBot.Controls.KBotChipBar
    Friend WithEvents navFisiere As KBot.Controls.KBotNavList
    Friend WithEvents pnlGrila As Panel
    Friend WithEvents grila As KBot.Controls.KBotDataView
    Friend WithEvents noticeGol As KBot.Controls.KBotNotice
    Friend WithEvents txtDetaliu As TextBox
    Friend WithEvents btnCopiaza As Button
    Friend WithEvents btnExporta As Button
    Friend WithEvents btnDeschideDosar As Button
    Friend WithEvents btnGoleste As Button
    Friend WithEvents busy As KBot.Controls.KBotBusyBar
    Friend WithEvents lblStare As Label
    Friend WithEvents tmrCautare As Timer
    Friend WithEvents tlyFilter As TableLayoutPanel
    Friend WithEvents tlyMain As TableLayoutPanel
    Friend WithEvents capBar As Controls.KBotCaptionBar
    Friend WithEvents tlyFilterActual As TableLayoutPanel
    Friend WithEvents pnlFisiere As Panel
    Friend WithEvents noticeServer As Controls.KBotNotice
    Friend WithEvents tlyFooter As TableLayoutPanel
End Class
