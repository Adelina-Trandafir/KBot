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
        Dim KBotNavItem9 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem10 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem12 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem13 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem14 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem15 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem16 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        btnToken = New System.Windows.Forms.Button()
        mnuUnitate = New KBotDropDownMenu(components)
        btnSterge = New System.Windows.Forms.Button()
        btnRenunta = New System.Windows.Forms.Button()
        btnModifica = New System.Windows.Forms.Button()
        btnAdauga = New System.Windows.Forms.Button()
        btnSalveaza = New System.Windows.Forms.Button()
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        barBusy = New KBotBusyBar()
        pnlCard = New System.Windows.Forms.Panel()
        tlyBody = New KBotTableLayoutPanel()
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
        tlyMain.SuspendLayout()
        pnlCard.SuspendLayout()
        tlyBody.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        pnlPages.SuspendLayout()
        CType(navDetaliu, ComponentModel.ISupportInitialize).BeginInit()
        tlySubsol.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnToken
        ' 
        btnToken.AutoSize = True
        btnToken.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnToken.Location = New System.Drawing.Point(60, 6)
        btnToken.Margin = New System.Windows.Forms.Padding(0, 0, 36, 0)
        btnToken.Name = "btnToken"
        btnToken.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnToken.Size = New System.Drawing.Size(242, 63)
        btnToken.TabIndex = 2
        btnToken.Text = "Token ANAF"
        tips.SetToolTipHeader(btnToken, "Token ANAF")
        tips.SetToolTipText(btnToken, "Arată până când este valabil tokenul ANAF al unității și îl reînnoiește cu certificatul calificat.")
        btnToken.UseVisualStyleBackColor = True
        ' 
        ' mnuUnitate
        ' 
        KBotMenuItem1.Image = My.Resources.Resources.credit_card
        KBotMenuItem1.Key = "conturi"
        KBotMenuItem1.Text = "Conturi Unitate"
        KBotMenuItem2.Image = My.Resources.Resources.kbot_64
        KBotMenuItem2.Key = "date"
        KBotMenuItem2.Text = "Date Unitate"
        mnuUnitate.Items.Add(KBotMenuItem1)
        mnuUnitate.Items.Add(KBotMenuItem2)
        ' 
        ' btnSterge
        ' 
        btnSterge.AutoSize = True
        btnSterge.Enabled = False
        btnSterge.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSterge.Location = New System.Drawing.Point(338, 6)
        btnSterge.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        btnSterge.Name = "btnSterge"
        btnSterge.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnSterge.Size = New System.Drawing.Size(194, 63)
        btnSterge.TabIndex = 3
        btnSterge.Text = "Ștergere"
        tips.SetToolTipText(btnSterge, "Șterge factura aleasă. Se poate șterge doar o ciornă care are ultimul număr al seriei; o factură trimisă la ANAF nu se șterge.")
        btnSterge.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.AutoSize = True
        btnRenunta.Enabled = False
        btnRenunta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnRenunta.Location = New System.Drawing.Point(544, 6)
        btnRenunta.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnRenunta.Size = New System.Drawing.Size(192, 63)
        btnRenunta.TabIndex = 4
        btnRenunta.Text = "Renunță"
        tips.SetToolTipText(btnRenunta, "Renunță la modificările nesalvate ale facturii.")
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' btnModifica
        ' 
        btnModifica.AutoSize = True
        btnModifica.Enabled = False
        btnModifica.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnModifica.Location = New System.Drawing.Point(748, 6)
        btnModifica.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        btnModifica.Name = "btnModifica"
        btnModifica.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnModifica.Size = New System.Drawing.Size(222, 63)
        btnModifica.TabIndex = 5
        btnModifica.Text = "Modificare"
        tips.SetToolTipText(btnModifica, "Deblochează factura aleasă pentru modificare. Se modifică doar o ciornă (netrimisă la ANAF).")
        btnModifica.UseVisualStyleBackColor = True
        ' 
        ' btnAdauga
        ' 
        btnAdauga.AutoSize = True
        btnAdauga.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnAdauga.Location = New System.Drawing.Point(982, 6)
        btnAdauga.Margin = New System.Windows.Forms.Padding(0, 0, 12, 0)
        btnAdauga.Name = "btnAdauga"
        btnAdauga.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnAdauga.Size = New System.Drawing.Size(212, 63)
        btnAdauga.TabIndex = 6
        btnAdauga.Text = "Adăugare"
        tips.SetToolTipText(btnAdauga, "Începe o factură nouă. Seria și numărul se dau la salvare.")
        btnAdauga.UseVisualStyleBackColor = True
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.AutoSize = True
        btnSalveaza.Dock = System.Windows.Forms.DockStyle.Right
        btnSalveaza.Enabled = False
        btnSalveaza.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnSalveaza.Location = New System.Drawing.Point(1206, 6)
        btnSalveaza.Margin = New System.Windows.Forms.Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnSalveaza.Size = New System.Drawing.Size(180, 69)
        btnSalveaza.TabIndex = 7
        btnSalveaza.Text = "Salvare"
        tips.SetToolTipText(btnSalveaza, "Scrie factura și liniile ei în baza de date (nu o trimite la ANAF).")
        btnSalveaza.UseVisualStyleBackColor = True
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
        tlyMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 81F))
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
        capBar.OptionButtonImage = My.Resources.Resources.kbot_64
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowOptionsButton = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New System.Drawing.Size(1394, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.TintOptionButtonImage = False
        capBar.Text = "E-Factura"
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
        pnlCard.Size = New System.Drawing.Size(1394, 741)
        pnlCard.TabIndex = 2
        pnlCard.Tag = "Card"
        ' 
        ' tlyBody
        ' 
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.608696F))
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.391304F))
        tlyBody.Controls.Add(tree, 0, 0)
        tlyBody.Controls.Add(pnlDetaliu, 1, 0)
        tlyBody.Dock = System.Windows.Forms.DockStyle.Fill
        tlyBody.Location = New System.Drawing.Point(12, 9)
        tlyBody.Margin = New System.Windows.Forms.Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlyBody.Size = New System.Drawing.Size(1370, 723)
        tlyBody.TabIndex = 0
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
        tree.Margin = New System.Windows.Forms.Padding(0, 0, 15, 0)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = True
        tree.RightClickSelects = False
        tree.RightIconSize = New System.Drawing.Size(14, 14)
        tree.ShowRightIconOnHover = True
        tree.Size = New System.Drawing.Size(431, 723)
        tree.TabIndex = 0
        ' 
        ' pnlDetaliu
        ' 
        pnlDetaliu.Controls.Add(pnlPages)
        pnlDetaliu.Controls.Add(navDetaliu)
        pnlDetaliu.Controls.Add(ntfMesaj)
        pnlDetaliu.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDetaliu.Location = New System.Drawing.Point(446, 0)
        pnlDetaliu.Margin = New System.Windows.Forms.Padding(0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Size = New System.Drawing.Size(924, 723)
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
        pnlPages.Size = New System.Drawing.Size(924, 599)
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
        pgGenerale.Size = New System.Drawing.Size(924, 599)
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
        pgCumparator.Size = New System.Drawing.Size(924, 599)
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
        pgAtasamente.Size = New System.Drawing.Size(924, 599)
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
        pgContinut.Size = New System.Drawing.Size(924, 599)
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
        pgPdf.Size = New System.Drawing.Size(924, 599)
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
        pgAnaf.Size = New System.Drawing.Size(924, 599)
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
        pgEroare.Size = New System.Drawing.Size(924, 599)
        pgEroare.TabIndex = 7
        pgEroare.Visible = False
        ' 
        ' navDetaliu
        ' 
        navDetaliu.Dock = System.Windows.Forms.DockStyle.Top
        navDetaliu.IconSize = 16
        navDetaliu.ItemCornerRadius = 2
        navDetaliu.ItemPadding = New System.Windows.Forms.Padding(3)
        KBotNavItem9.AutoSize = True
        KBotNavItem9.Image = My.Resources.Resources.cells
        KBotNavItem9.Key = "generale"
        KBotNavItem9.Text = "Generale"
        KBotNavItem10.AutoSize = True
        KBotNavItem10.Image = My.Resources.Resources.Fatcow_Farm_Fresh_Reseller_account_template_32
        KBotNavItem10.Key = "cumparator"
        KBotNavItem10.Text = "Cumpărător"
        KBotNavItem12.AutoSize = True
        KBotNavItem12.Image = My.Resources.Resources.attach
        KBotNavItem12.Key = "atasamente"
        KBotNavItem12.Text = "Atașamente"
        KBotNavItem13.AutoSize = True
        KBotNavItem13.Image = My.Resources.Resources.table
        KBotNavItem13.Key = "continut"
        KBotNavItem13.Text = "Conținut"
        KBotNavItem14.AutoSize = True
        KBotNavItem14.Image = My.Resources.Resources.invoice
        KBotNavItem14.Key = "pdf"
        KBotNavItem14.Text = "Factură PDF"
        KBotNavItem14.Visible = False
        KBotNavItem15.AutoSize = True
        KBotNavItem15.Image = My.Resources.Resources.anaf
        KBotNavItem15.Key = "anaf"
        KBotNavItem15.Text = "Factură ANAF"
        KBotNavItem15.Visible = False
        KBotNavItem16.AutoSize = True
        KBotNavItem16.Image = My.Resources.Resources.invoice_error
        KBotNavItem16.Key = "eroare"
        KBotNavItem16.Text = "Eroare ANAF"
        KBotNavItem16.Visible = False
        navDetaliu.Items.Add(KBotNavItem9)
        navDetaliu.Items.Add(KBotNavItem10)
        navDetaliu.Items.Add(KBotNavItem12)
        navDetaliu.Items.Add(KBotNavItem13)
        navDetaliu.Items.Add(KBotNavItem14)
        navDetaliu.Items.Add(KBotNavItem15)
        navDetaliu.Items.Add(KBotNavItem16)
        navDetaliu.Location = New System.Drawing.Point(0, 0)
        navDetaliu.Name = "navDetaliu"
        navDetaliu.Orientation = KBotNavOrientation.Horizontal
        navDetaliu.SelectedKey = Nothing
        navDetaliu.Size = New System.Drawing.Size(924, 40)
        navDetaliu.TabIndex = 0
        ' 
        ' ntfMesaj
        ' 
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 639)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(924, 84)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        ' 
        ' tlySubsol
        ' 
        tlySubsol.AutoFitToTheme = False
        tlySubsol.ColumnCount = 8
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        tlySubsol.Controls.Add(btnIesire, 0, 0)
        tlySubsol.Controls.Add(lblStare, 1, 0)
        tlySubsol.Controls.Add(btnToken, 2, 0)
        tlySubsol.Controls.Add(btnSterge, 3, 0)
        tlySubsol.Controls.Add(btnRenunta, 4, 0)
        tlySubsol.Controls.Add(btnModifica, 5, 0)
        tlySubsol.Controls.Add(btnAdauga, 6, 0)
        tlySubsol.Controls.Add(btnSalveaza, 7, 0)
        tlySubsol.Dock = System.Windows.Forms.DockStyle.Fill
        tlySubsol.Location = New System.Drawing.Point(0, 813)
        tlySubsol.Margin = New System.Windows.Forms.Padding(0)
        tlySubsol.Name = "tlySubsol"
        tlySubsol.Padding = New System.Windows.Forms.Padding(8, 6, 8, 6)
        tlySubsol.RowCount = 1
        tlySubsol.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F))
        tlySubsol.Size = New System.Drawing.Size(1394, 81)
        tlySubsol.TabIndex = 3
        ' 
        ' btnIesire
        ' 
        btnIesire.AutoSize = True
        btnIesire.Dock = System.Windows.Forms.DockStyle.Left
        btnIesire.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        btnIesire.Location = New System.Drawing.Point(8, 6)
        btnIesire.Margin = New System.Windows.Forms.Padding(0)
        btnIesire.Name = "btnIesire"
        btnIesire.Padding = New System.Windows.Forms.Padding(30, 4, 30, 4)
        btnIesire.Size = New System.Drawing.Size(158, 69)
        btnIesire.TabIndex = 0
        btnIesire.Text = "Ieșire"
        btnIesire.UseVisualStyleBackColor = True
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = System.Windows.Forms.DockStyle.Fill
        lblStare.Location = New System.Drawing.Point(184, 6)
        lblStare.Margin = New System.Windows.Forms.Padding(18, 0, 18, 0)
        lblStare.Name = "lblStare"
        lblStare.Size = New System.Drawing.Size(1, 69)
        lblStare.TabIndex = 1
        lblStare.TextAlign = Drawing.ContentAlignment.MiddleLeft
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
        tlyBody.ResumeLayout(False)
        pnlDetaliu.ResumeLayout(False)
        pnlPages.ResumeLayout(False)
        CType(navDetaliu, ComponentModel.ISupportInitialize).EndInit()
        tlySubsol.ResumeLayout(False)
        tlySubsol.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents barBusy As KBotBusyBar
    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents tlyBody As KBotTableLayoutPanel
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
    Friend WithEvents btnToken As System.Windows.Forms.Button
    Friend WithEvents mnuUnitate As KBotDropDownMenu
    Friend WithEvents btnSterge As System.Windows.Forms.Button
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
    Friend WithEvents btnModifica As System.Windows.Forms.Button
    Friend WithEvents btnAdauga As System.Windows.Forms.Button
    Friend WithEvents btnSalveaza As System.Windows.Forms.Button
End Class
