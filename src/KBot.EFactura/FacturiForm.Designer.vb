Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FacturiForm
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
        Dim KBotMenuItem1 As KBotMenuItem = New KBotMenuItem()
        Dim KBotMenuItem2 As KBotMenuItem = New KBotMenuItem()
        Dim KBotMenuItem3 As KBotMenuItem = New KBotMenuItem()
        Dim KBotMenuItem4 As KBotMenuItem = New KBotMenuItem()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FacturiForm))
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem5 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem6 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem7 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        btnRenunta = New System.Windows.Forms.Button()
        btnModifica = New System.Windows.Forms.Button()
        btnAdauga = New System.Windows.Forms.Button()
        btnSalveaza = New System.Windows.Forms.Button()
        mnuUnitate = New KBotDropDownMenu(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        barBusy = New KBotBusyBar()
        pnlCard = New System.Windows.Forms.Panel()
        tlyBody = New KBotTableLayoutPanel()
        pnlArbore = New System.Windows.Forms.Panel()
        cmbLuna = New KBotComboBox()
        tree = New AdvancedTreeControl()
        pnlDetaliu = New System.Windows.Forms.Panel()
        pnlPages = New System.Windows.Forms.Panel()
        pgGenerale = New VanzareGeneralePage()
        pgCumparator = New VanzareCumparatorPage()
        pgAtasamente = New VanzareAtasamentePage()
        pgContinut = New VanzareContinutPage()
        pgPdf = New VanzareFacturaPdfPage()
        pgAnaf = New VanzareFacturaAnafPage()
        pgEroare = New VanzareEroareAnafPage()
        navDetaliu = New KBotNavList()
        ntfMesaj = New KBotNotice()
        tlySubsol = New KBotTableLayoutPanel()
        btnIesire = New System.Windows.Forms.Button()
        lblStare = New System.Windows.Forms.Label()
        tlyButoaneStanga = New System.Windows.Forms.TableLayoutPanel()
        tlyButoaneDreapta = New System.Windows.Forms.TableLayoutPanel()
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        pnlArbore.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        pnlPages.SuspendLayout()
        CType(navDetaliu, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        tlyButoaneStanga.SuspendLayout()
        tlyButoaneDreapta.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnRenunta
        ' 
        btnRenunta.Dock = System.Windows.Forms.DockStyle.Fill
        btnRenunta.Enabled = False
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Image = My.Resources.Resources.undo
        btnRenunta.Location = New System.Drawing.Point(0, 0)
        btnRenunta.Margin = New System.Windows.Forms.Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnRenunta.Size = New System.Drawing.Size(50, 50)
        btnRenunta.TabIndex = 4
        tips.SetToolTipText(btnRenunta, "Renunță la modificările nesalvate ale facturii.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnModifica
        ' 
        btnModifica.Enabled = False
        btnModifica.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnModifica.Image = My.Resources.Resources.edit_fact
        btnModifica.Location = New System.Drawing.Point(802, 0)
        btnModifica.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        btnModifica.Name = "btnModifica"
        btnModifica.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnModifica.Size = New System.Drawing.Size(50, 50)
        btnModifica.TabIndex = 5
        tips.SetToolTipText(btnModifica, "Deblochează factura aleasă pentru modificare. Se modifică doar o ciornă (netrimisă la ANAF).")
        btnModifica.UseVisualStyleBackColor = True
        ' 
        ' btnAdauga
        ' 
        btnAdauga.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAdauga.Image = My.Resources.Resources.add_fact
        btnAdauga.Location = New System.Drawing.Point(860, 0)
        btnAdauga.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnAdauga.Size = New System.Drawing.Size(50, 50)
        btnAdauga.TabIndex = 6
        tips.SetToolTipText(btnAdauga, "Începe o factură nouă. Seria și numărul se dau la salvare.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSalveaza.Image = My.Resources.Resources.save_32
        btnSalveaza.Location = New System.Drawing.Point(918, 0)
        btnSalveaza.Margin = New System.Windows.Forms.Padding(0, 0, 8, 0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnSalveaza.Size = New System.Drawing.Size(50, 50)
        btnSalveaza.TabIndex = 7
        tips.SetToolTipText(btnSalveaza, "Scrie factura și liniile ei în baza de date (nu o trimite la ANAF).")
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' mnuUnitate
        ' 
        KBotMenuItem1.Image = My.Resources.Resources.credit_card
        KBotMenuItem1.Key = "conturi"
        KBotMenuItem1.Text = "Conturi Unitate"
        KBotMenuItem2.Image = My.Resources.Resources.kbot_64
        KBotMenuItem2.Key = "date"
        KBotMenuItem2.Text = "Date Unitate"
        KBotMenuItem3.Image = My.Resources.Resources.anaf
        KBotMenuItem3.Key = "primite"
        KBotMenuItem3.Text = "Sincronizează facturi primite"
        mnuUnitate.Items.Add(KBotMenuItem1)
        mnuUnitate.Items.Add(KBotMenuItem2)
        KBotMenuItem4.Image = My.Resources.Resources.invoice
        KBotMenuItem4.Key = "listaprimite"
        KBotMenuItem4.Text = "Facturi primite"
        mnuUnitate.Items.Add(KBotMenuItem3)
        mnuUnitate.Items.Add(KBotMenuItem4)
        ' 
        ' tlyMain
        ' 
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(barBusy, 0, 1)
        tlyMain.Controls.Add(pnlCard, 0, 2)
        tlyMain.Controls.Add(tlySubsol, 0, 3)
        tlyMain.Dock = System.Windows.Forms.DockStyle.Fill
        tlyMain.Location = New System.Drawing.Point(3, 3)
        tlyMain.Margin = New System.Windows.Forms.Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 4
        tlyMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tlyMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F))
        tlyMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyMain.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tlyMain.Size = New System.Drawing.Size(1394, 894)
        tlyMain.TabIndex = 0
        ' 
        ' capBar
        ' 
        capBar.Dock = System.Windows.Forms.DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.kbot_e_64
        capBar.Location = New System.Drawing.Point(0, 0)
        capBar.Margin = New System.Windows.Forms.Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = CType(resources.GetObject("capBar.OptionButtonImage"), Drawing.Image)
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowOptionsButton = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New System.Drawing.Size(1394, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Factura"
        capBar.TintOptionButtonImage = False
        ' 
        ' barBusy
        ' 
        barBusy.Dock = System.Windows.Forms.DockStyle.Fill
        barBusy.Location = New System.Drawing.Point(0, 66)
        barBusy.Margin = New System.Windows.Forms.Padding(0)
        barBusy.Name = "barBusy"
        barBusy.Size = New System.Drawing.Size(1394, 6)
        barBusy.TabIndex = 1
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyBody)
        pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        pnlCard.Location = New System.Drawing.Point(0, 72)
        pnlCard.Margin = New System.Windows.Forms.Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Padding = New System.Windows.Forms.Padding(12, 9, 12, 9)
        pnlCard.Size = New System.Drawing.Size(1394, 772)
        pnlCard.TabIndex = 2
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F))
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F))
        tlyBody.Controls.Add(pnlArbore, 0, 0)
        tlyBody.Controls.Add(pnlDetaliu, 1, 0)
        tlyBody.Dock = System.Windows.Forms.DockStyle.Fill
        tlyBody.Location = New System.Drawing.Point(12, 9)
        tlyBody.Margin = New System.Windows.Forms.Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyBody.Size = New System.Drawing.Size(1370, 754)
        tlyBody.TabIndex = 0
        ' 
        ' pnlArbore
        ' 
        pnlArbore.Controls.Add(tree)
        pnlArbore.Controls.Add(cmbLuna)
        pnlArbore.Dock = System.Windows.Forms.DockStyle.Fill
        pnlArbore.Location = New System.Drawing.Point(0, 0)
        pnlArbore.Margin = New System.Windows.Forms.Padding(0)
        pnlArbore.Name = "pnlArbore"
        pnlArbore.Size = New System.Drawing.Size(411, 754)
        pnlArbore.TabIndex = 0
        ' 
        ' cmbLuna
        ' 
        cmbLuna.Dock = System.Windows.Forms.DockStyle.Top
        cmbLuna.Location = New System.Drawing.Point(0, 0)
        cmbLuna.Margin = New System.Windows.Forms.Padding(0)
        cmbLuna.Name = "cmbLuna"
        cmbLuna.Size = New System.Drawing.Size(411, 37)
        cmbLuna.TabIndex = 1
        tips.SetToolTipHeader(cmbLuna, "Luna facturilor")
        tips.SetToolTipText(cmbLuna, "Arată în arbore doar facturile din luna aleasă. Anul este cel ales în K-BOT.")
        ' 
        ' tree
        ' 
        tree.BorderColor = Drawing.SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = System.Windows.Forms.DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.ExpanderSize = 10
        tree.Font = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderBackColor = Drawing.SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " FACTURI EMISE, PE CLIENȚI"
        tree.HeaderFont = New System.Drawing.Font("Calibri", 9F, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Drawing.Color.Black
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New System.Drawing.Size(18, 18)
        tree.HeaderSeparatorColor = Drawing.Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 24
        tree.LeftIconSize = New System.Drawing.Size(14, 14)
        tree.LeftTextWidth = 100
        tree.Location = New System.Drawing.Point(0, 0)
        tree.Margin = New System.Windows.Forms.Padding(0)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = True
        tree.RightClickSelects = False
        tree.RightIconSize = New System.Drawing.Size(14, 14)
        tree.ShowRightIconOnHover = True
        tree.Size = New System.Drawing.Size(411, 754)
        tree.TabIndex = 0
        ' 
        ' pnlDetaliu
        ' 
        pnlDetaliu.Controls.Add(pnlPages)
        pnlDetaliu.Controls.Add(navDetaliu)
        pnlDetaliu.Controls.Add(ntfMesaj)
        pnlDetaliu.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDetaliu.Location = New System.Drawing.Point(411, 0)
        pnlDetaliu.Margin = New System.Windows.Forms.Padding(0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Size = New System.Drawing.Size(959, 754)
        pnlDetaliu.TabIndex = 1
        pnlDetaliu.Tag = "Card"
        ' 
        ' pnlPages
        ' 
        pnlPages.Controls.Add(pgGenerale)
        pnlPages.Controls.Add(pgCumparator)
        pnlPages.Controls.Add(pgAtasamente)
        pnlPages.Controls.Add(pgContinut)
        pnlPages.Controls.Add(pgPdf)
        pnlPages.Controls.Add(pgAnaf)
        pnlPages.Controls.Add(pgEroare)
        pnlPages.Dock = System.Windows.Forms.DockStyle.Fill
        pnlPages.Location = New System.Drawing.Point(0, 40)
        pnlPages.Margin = New System.Windows.Forms.Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New System.Drawing.Size(959, 630)
        pnlPages.TabIndex = 1
        ' 
        ' pgGenerale
        ' 
        pgGenerale.Dock = System.Windows.Forms.DockStyle.Fill
        pgGenerale.Font = New System.Drawing.Font("Calibri", 9F)
        pgGenerale.Location = New System.Drawing.Point(0, 0)
        pgGenerale.Margin = New System.Windows.Forms.Padding(0)
        pgGenerale.Name = "pgGenerale"
        pgGenerale.Padding = New System.Windows.Forms.Padding(6)
        pgGenerale.Size = New System.Drawing.Size(959, 630)
        pgGenerale.TabIndex = 0
        ' 
        ' pgCumparator
        ' 
        pgCumparator.Dock = System.Windows.Forms.DockStyle.Fill
        pgCumparator.Font = New System.Drawing.Font("Calibri", 9F)
        pgCumparator.Location = New System.Drawing.Point(0, 0)
        pgCumparator.Margin = New System.Windows.Forms.Padding(0)
        pgCumparator.Name = "pgCumparator"
        pgCumparator.Padding = New System.Windows.Forms.Padding(6)
        pgCumparator.Size = New System.Drawing.Size(959, 630)
        pgCumparator.TabIndex = 1
        pgCumparator.Visible = False
        ' 
        ' pgAtasamente
        ' 
        pgAtasamente.Dock = System.Windows.Forms.DockStyle.Fill
        pgAtasamente.Font = New System.Drawing.Font("Calibri", 9F)
        pgAtasamente.Location = New System.Drawing.Point(0, 0)
        pgAtasamente.Margin = New System.Windows.Forms.Padding(0)
        pgAtasamente.Name = "pgAtasamente"
        pgAtasamente.Padding = New System.Windows.Forms.Padding(6)
        pgAtasamente.Size = New System.Drawing.Size(959, 630)
        pgAtasamente.TabIndex = 3
        pgAtasamente.Visible = False
        ' 
        ' pgContinut
        ' 
        pgContinut.Dock = System.Windows.Forms.DockStyle.Fill
        pgContinut.Font = New System.Drawing.Font("Calibri", 9F)
        pgContinut.Location = New System.Drawing.Point(0, 0)
        pgContinut.Margin = New System.Windows.Forms.Padding(0)
        pgContinut.Name = "pgContinut"
        pgContinut.Padding = New System.Windows.Forms.Padding(6)
        pgContinut.Size = New System.Drawing.Size(959, 630)
        pgContinut.TabIndex = 4
        pgContinut.Visible = False
        ' 
        ' pgPdf
        ' 
        pgPdf.Dock = System.Windows.Forms.DockStyle.Fill
        pgPdf.Font = New System.Drawing.Font("Calibri", 9F)
        pgPdf.Location = New System.Drawing.Point(0, 0)
        pgPdf.Margin = New System.Windows.Forms.Padding(0)
        pgPdf.Name = "pgPdf"
        pgPdf.Size = New System.Drawing.Size(959, 630)
        pgPdf.TabIndex = 5
        pgPdf.Visible = False
        ' 
        ' pgAnaf
        ' 
        pgAnaf.Dock = System.Windows.Forms.DockStyle.Fill
        pgAnaf.Font = New System.Drawing.Font("Calibri", 9F)
        pgAnaf.Location = New System.Drawing.Point(0, 0)
        pgAnaf.Margin = New System.Windows.Forms.Padding(0)
        pgAnaf.Name = "pgAnaf"
        pgAnaf.Size = New System.Drawing.Size(959, 630)
        pgAnaf.TabIndex = 6
        pgAnaf.Visible = False
        ' 
        ' pgEroare
        ' 
        pgEroare.Dock = System.Windows.Forms.DockStyle.Fill
        pgEroare.Font = New System.Drawing.Font("Calibri", 9F)
        pgEroare.Location = New System.Drawing.Point(0, 0)
        pgEroare.Margin = New System.Windows.Forms.Padding(0)
        pgEroare.Name = "pgEroare"
        pgEroare.Size = New System.Drawing.Size(959, 630)
        pgEroare.TabIndex = 7
        pgEroare.Visible = False
        ' 
        ' navDetaliu
        ' 
        navDetaliu.Dock = System.Windows.Forms.DockStyle.Top
        navDetaliu.IconSize = 18
        navDetaliu.ItemCornerRadius = 2
        navDetaliu.ItemPadding = New System.Windows.Forms.Padding(0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.cells
        KBotNavItem1.Key = "generale"
        KBotNavItem1.Text = "Generale"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Reseller_account_template_32
        KBotNavItem2.Key = "cumparator"
        KBotNavItem2.Text = "Cumpărător"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.table
        KBotNavItem3.Key = "continut"
        KBotNavItem3.Text = "Conținut"
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.attach
        KBotNavItem4.Key = "atasamente"
        KBotNavItem4.Text = "Atașamente"
        KBotNavItem5.AutoSize = True
        KBotNavItem5.Image = My.Resources.Resources.invoice
        KBotNavItem5.Key = "pdf"
        KBotNavItem5.Text = "Factură PDF"
        KBotNavItem5.Visible = False
        KBotNavItem6.AutoSize = True
        KBotNavItem6.Image = My.Resources.Resources.anaf
        KBotNavItem6.Key = "anaf"
        KBotNavItem6.Text = "Factură ANAF"
        KBotNavItem6.Visible = False
        KBotNavItem7.AutoSize = True
        KBotNavItem7.Image = My.Resources.Resources.invoice_error
        KBotNavItem7.Key = "eroare"
        KBotNavItem7.Text = "Eroare ANAF"
        KBotNavItem7.Visible = False
        navDetaliu.Items.Add(KBotNavItem1)
        navDetaliu.Items.Add(KBotNavItem2)
        navDetaliu.Items.Add(KBotNavItem3)
        navDetaliu.Items.Add(KBotNavItem4)
        navDetaliu.Items.Add(KBotNavItem5)
        navDetaliu.Items.Add(KBotNavItem6)
        navDetaliu.Items.Add(KBotNavItem7)
        navDetaliu.Location = New System.Drawing.Point(0, 0)
        navDetaliu.Name = "navDetaliu"
        navDetaliu.Orientation = KBotNavOrientation.Horizontal
        navDetaliu.SelectedKey = Nothing
        navDetaliu.Size = New System.Drawing.Size(959, 40)
        navDetaliu.TabIndex = 0
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 670)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(959, 84)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' tlySubsol
        ' 
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 2
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F))
        tlySubsol.Controls.Add(tlyButoaneStanga, 0, 0)
        tlySubsol.Controls.Add(tlyButoaneDreapta, 1, 0)
        tlySubsol.Dock = System.Windows.Forms.DockStyle.Fill
        tlySubsol.Location = New System.Drawing.Point(0, 844)
        tlySubsol.Margin = New System.Windows.Forms.Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New System.Windows.Forms.RowStyle())
        tlySubsol.Size = New System.Drawing.Size(1394, 50)
        tlySubsol.TabIndex = 3
        ' 
        ' btnIesire
        ' 
        btnIesire.Dock = System.Windows.Forms.DockStyle.Fill
        btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnIesire.Image = My.Resources.Resources.exit_door
        btnIesire.Location = New System.Drawing.Point(8, 0)
        btnIesire.Margin = New System.Windows.Forms.Padding(8, 0, 0, 0)
        btnIesire.Name = "btnIesire"
        btnIesire.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnIesire.Size = New System.Drawing.Size(50, 50)
        btnIesire.TabIndex = 0
        btnIesire.Text = "Ieșire"
        btnIesire.UseVisualStyleBackColor = True
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = System.Windows.Forms.DockStyle.Fill
        lblStare.Location = New System.Drawing.Point(76, 0)
        lblStare.Margin = New System.Windows.Forms.Padding(18, 0, 18, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New System.Drawing.Size(324, 50)
        lblStare.TabIndex = 1
        lblStare.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' tlyButoaneStanga
        ' 
        tlyButoaneStanga.ColumnCount = 2
        tlyButoaneStanga.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlyButoaneStanga.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyButoaneStanga.Controls.Add(btnIesire, 0, 0)
        tlyButoaneStanga.Controls.Add(lblStare, 1, 0)
        tlyButoaneStanga.Dock = System.Windows.Forms.DockStyle.Fill
        tlyButoaneStanga.Location = New System.Drawing.Point(0, 0)
        tlyButoaneStanga.Margin = New System.Windows.Forms.Padding(0)
        tlyButoaneStanga.Name = "tlyButoaneStanga"
        tlyButoaneStanga.RowCount = 1
        tlyButoaneStanga.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyButoaneStanga.Size = New System.Drawing.Size(418, 50)
        tlyButoaneStanga.TabIndex = 8
        ' 
        ' tlyButoaneDreapta
        ' 
        tlyButoaneDreapta.ColumnCount = 5
        tlyButoaneDreapta.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlyButoaneDreapta.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyButoaneDreapta.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlyButoaneDreapta.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlyButoaneDreapta.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlyButoaneDreapta.Controls.Add(btnSalveaza, 4, 0)
        tlyButoaneDreapta.Controls.Add(btnAdauga, 3, 0)
        tlyButoaneDreapta.Controls.Add(btnModifica, 2, 0)
        tlyButoaneDreapta.Controls.Add(btnRenunta, 0, 0)
        tlyButoaneDreapta.Dock = System.Windows.Forms.DockStyle.Fill
        tlyButoaneDreapta.Location = New System.Drawing.Point(418, 0)
        tlyButoaneDreapta.Margin = New System.Windows.Forms.Padding(0)
        tlyButoaneDreapta.Name = "tlyButoaneDreapta"
        tlyButoaneDreapta.RowCount = 1
        tlyButoaneDreapta.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyButoaneDreapta.Size = New System.Drawing.Size(976, 50)
        tlyButoaneDreapta.TabIndex = 9
        ' 
        ' FacturiForm
        ' 
        AutoFitToTheme = False
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        ClientSize = New System.Drawing.Size(1400, 900)
        Controls.Add(tlyMain)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Margin = New System.Windows.Forms.Padding(6)
        MinimizeBox = False
        MinimumSize = New System.Drawing.Size(1400, 900)
        Name = "FacturiForm"
        Padding = New System.Windows.Forms.Padding(3)
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "E-Factura — facturi emise"
        tlyMain.ResumeLayout(False)
        pnlCard.ResumeLayout(False)
        pnlArbore.ResumeLayout(False)
        tlyBody.ResumeLayout(False)
        pnlDetaliu.ResumeLayout(False)
        pnlPages.ResumeLayout(False)
        CType(navDetaliu, ComponentModel.ISupportInitialize).EndInit()
        tlySubsol.ResumeLayout(False)
        tlyButoaneStanga.ResumeLayout(False)
        tlyButoaneDreapta.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents barBusy As KBotBusyBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents pnlArbore As System.Windows.Forms.Panel
    Friend WithEvents cmbLuna As KBotComboBox
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents pnlDetaliu As System.Windows.Forms.Panel
    Friend WithEvents navDetaliu As KBotNavList
    Friend WithEvents pnlPages As System.Windows.Forms.Panel
    Friend WithEvents pgGenerale As VanzareGeneralePage
    Friend WithEvents pgCumparator As VanzareCumparatorPage
    Friend WithEvents pgAtasamente As VanzareAtasamentePage
    Friend WithEvents pgContinut As VanzareContinutPage
    Friend WithEvents pgPdf As VanzareFacturaPdfPage
    Friend WithEvents pgAnaf As VanzareFacturaAnafPage
    Friend WithEvents pgEroare As VanzareEroareAnafPage
    Friend WithEvents ntfMesaj As KBotNotice
    Friend WithEvents tlySubsol As KBotTableLayoutPanel
    Friend WithEvents btnIesire As System.Windows.Forms.Button
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents mnuUnitate As KBotDropDownMenu
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
    Friend WithEvents btnModifica As System.Windows.Forms.Button
    Friend WithEvents btnAdauga As System.Windows.Forms.Button
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
    Friend WithEvents tlyButoaneStanga As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents tlyButoaneDreapta As System.Windows.Forms.TableLayoutPanel
End Class
