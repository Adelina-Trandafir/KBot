Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteView
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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem6 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem7 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        pnlArbore = New System.Windows.Forms.Panel()
        tree = New AdvancedTreeControl()
        pnlDetaliu = New System.Windows.Forms.Panel()
        pnlNav = New System.Windows.Forms.Panel()
        pnlPages = New System.Windows.Forms.Panel()
        pgLinii = New PrimiteLiniiPage()
        pgPdf = New PrimitePdfPage()
        pgMesaje = New PrimiteMesajePage()
        pgReceptii = New PrimiteReceptiiPage()
        pgXml = New XmlViz()
        navDetaliu = New KBotNavList()
        ntfMesaj = New KBotNotice()
        tlyBody.SuspendLayout()
        pnlArbore.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        pnlNav.SuspendLayout()
        pnlPages.SuspendLayout()
        CType(navDetaliu, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.0!))
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76.0!))
        tlyBody.Controls.Add(pnlArbore, 0, 0)
        tlyBody.Controls.Add(pnlDetaliu, 1, 0)
        tlyBody.Dock = System.Windows.Forms.DockStyle.Fill
        tlyBody.Location = New System.Drawing.Point(0, 0)
        tlyBody.Margin = New System.Windows.Forms.Padding(0)
        tlyBody.Name = "tlyBody"
        tlyBody.RowCount = 1
        tlyBody.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyBody.Size = New System.Drawing.Size(1000, 600)
        tlyBody.TabIndex = 0
        '
        ' pnlArbore
        '
        pnlArbore.Controls.Add(tree)
        pnlArbore.Dock = System.Windows.Forms.DockStyle.Fill
        pnlArbore.Location = New System.Drawing.Point(0, 0)
        pnlArbore.Margin = New System.Windows.Forms.Padding(0)
        pnlArbore.Name = "pnlArbore"
        pnlArbore.Size = New System.Drawing.Size(320, 600)
        pnlArbore.TabIndex = 0
        '
        ' tree
        '
        tree.BorderColor = Drawing.SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = System.Windows.Forms.DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.FooterBackColor = Drawing.SystemColors.Control
        tree.FooterCaption = "Actualizează"
        tree.FooterCaptionFont = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        tree.FooterHeight = 30
        tree.FooterIconSize = New System.Drawing.Size(18, 18)
        tree.FooterRightIcon = My.Resources.Resources.Jonas_Rask_Danish_Royalty_Free_Refresh_32
        tree.FooterRightIconTooltip = "Aduce de la ANAF facturile noi." & vbLf & "Alegeți perioada sau scrieți numărul de zile (1-60)."
        tree.FooterSeparatorColor = Drawing.Color.Gainsboro
        tree.FooterSeparatorWidth = 2
        tree.FooterTextAlign = Drawing.ContentAlignment.MiddleRight
        tree.FooterVisible = True
        tree.ExpanderSize = 10
        tree.Font = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderBackColor = Drawing.SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " FACTURI PRIMITE, PE LUNI"
        tree.HeaderFont = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Drawing.Color.Black
        tree.HeaderHeight = 30
        tree.HeaderIconSize = New System.Drawing.Size(18, 18)
        tree.HeaderRightIcon = My.Resources.Resources.filter
        tree.HeaderRightIconTooltip = "Opțiunile arborelui: filtrele și gruparea facturilor."
        tree.HeaderSearchIcon = My.Resources.Resources.search
        tree.HeaderSearchIconTooltip = "Deschide banda de căutare peste arbore." & vbLf & "ESC golește căutarea și o închide."
        tree.HeaderSeparatorColor = Drawing.Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 24
        tree.LeftIconSize = New System.Drawing.Size(14, 14)
        tree.Location = New System.Drawing.Point(0, 0)
        tree.Margin = New System.Windows.Forms.Padding(0)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeEnd = 8
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = False
        tree.RightTextWidth = 96
        tree.RightClickSelects = False
        tree.RightIconSize = New System.Drawing.Size(14, 14)
        tree.SearchBarFont = New System.Drawing.Font("Calibri", 9.0!)
        tree.SearchBoxBackColor = Drawing.SystemColors.Control
        tree.SearchClearButton = True
        tree.SearchClearButtonHoverColor = Drawing.SystemColors.Control
        tree.SearchDefaultText = "... tastează minim 3 caractere ..."
        tree.SearchSeparatorColor = Drawing.SystemColors.ActiveBorder
        tree.SearchSeparatorWidth = 2
        tree.ShowRightIconOnHover = False
        tree.Size = New System.Drawing.Size(320, 600)
        tree.TabIndex = 0
        '
        ' pnlDetaliu
        '
        pnlDetaliu.Controls.Add(pnlPages)
        pnlDetaliu.Controls.Add(pnlNav)
        pnlDetaliu.Controls.Add(ntfMesaj)
        pnlDetaliu.Dock = System.Windows.Forms.DockStyle.Fill
        pnlDetaliu.Location = New System.Drawing.Point(320, 0)
        pnlDetaliu.Margin = New System.Windows.Forms.Padding(0)
        pnlDetaliu.Name = "pnlDetaliu"
        pnlDetaliu.Size = New System.Drawing.Size(680, 600)
        pnlDetaliu.TabIndex = 1
        pnlDetaliu.Tag = "Card"
        '
        ' pnlPages
        '
        pnlPages.Controls.Add(pgLinii)
        pnlPages.Controls.Add(pgPdf)
        pnlPages.Controls.Add(pgMesaje)
        pnlPages.Controls.Add(pgReceptii)
        pnlPages.Controls.Add(pgXml)
        pnlPages.Dock = System.Windows.Forms.DockStyle.Fill
        pnlPages.Location = New System.Drawing.Point(0, 30)
        pnlPages.Margin = New System.Windows.Forms.Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New System.Drawing.Size(680, 486)
        pnlPages.TabIndex = 2
        '
        ' pgLinii
        '
        pgLinii.Dock = System.Windows.Forms.DockStyle.Fill
        pgLinii.Location = New System.Drawing.Point(0, 0)
        pgLinii.Margin = New System.Windows.Forms.Padding(0)
        pgLinii.Name = "pgLinii"
        pgLinii.Size = New System.Drawing.Size(680, 476)
        pgLinii.TabIndex = 0
        '
        ' pgPdf
        '
        pgPdf.Dock = System.Windows.Forms.DockStyle.Fill
        pgPdf.Location = New System.Drawing.Point(0, 0)
        pgPdf.Margin = New System.Windows.Forms.Padding(0)
        pgPdf.Name = "pgPdf"
        pgPdf.Size = New System.Drawing.Size(680, 476)
        pgPdf.TabIndex = 1
        pgPdf.Visible = False
        '
        ' pgMesaje
        '
        pgMesaje.Dock = System.Windows.Forms.DockStyle.Fill
        pgMesaje.Location = New System.Drawing.Point(0, 0)
        pgMesaje.Margin = New System.Windows.Forms.Padding(0)
        pgMesaje.Name = "pgMesaje"
        pgMesaje.Size = New System.Drawing.Size(680, 476)
        pgMesaje.TabIndex = 3
        pgMesaje.Visible = False
        '
        ' pgReceptii
        '
        pgReceptii.Dock = System.Windows.Forms.DockStyle.Fill
        pgReceptii.Location = New System.Drawing.Point(0, 0)
        pgReceptii.Margin = New System.Windows.Forms.Padding(0)
        pgReceptii.Name = "pgReceptii"
        pgReceptii.Size = New System.Drawing.Size(680, 476)
        pgReceptii.TabIndex = 4
        pgReceptii.Visible = False
        '
        ' pgXml
        '
        pgXml.Dock = System.Windows.Forms.DockStyle.Fill
        pgXml.Location = New System.Drawing.Point(0, 0)
        pgXml.Margin = New System.Windows.Forms.Padding(0)
        pgXml.Name = "pgXml"
        pgXml.Padding = New System.Windows.Forms.Padding(3, 3, 3, 0)
        pgXml.Size = New System.Drawing.Size(680, 486)
        pgXml.TabIndex = 5
        pgXml.Visible = False
        '
        ' pnlNav: the bar of views is as high as the header of the tree and starts where the grids do (3 px from the tree)
        '
        pnlNav.Controls.Add(navDetaliu)
        pnlNav.Dock = System.Windows.Forms.DockStyle.Top
        pnlNav.Location = New System.Drawing.Point(0, 0)
        pnlNav.Margin = New System.Windows.Forms.Padding(0)
        pnlNav.Name = "pnlNav"
        pnlNav.Padding = New System.Windows.Forms.Padding(3, 0, 3, 0)
        pnlNav.Size = New System.Drawing.Size(680, 30)
        pnlNav.TabIndex = 0
        '
        ' navDetaliu
        '
        navDetaliu.Dock = System.Windows.Forms.DockStyle.Fill
        navDetaliu.IconSize = 18
        navDetaliu.ItemCornerRadius = 2
        navDetaliu.ItemPadding = New System.Windows.Forms.Padding(0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.table
        KBotNavItem1.Key = "linii"
        KBotNavItem1.Text = "Conținut"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.invoice
        KBotNavItem2.Key = "pdf"
        KBotNavItem2.Text = "Factură PDF"
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.cells
        KBotNavItem4.Key = "mesaje"
        KBotNavItem4.Text = "Mesaje"
        KBotNavItem4.Visible = False
        navDetaliu.Items.Add(KBotNavItem1)
        navDetaliu.Items.Add(KBotNavItem2)
        navDetaliu.Items.Add(KBotNavItem4)
        KBotNavItem7.AutoSize = True
        KBotNavItem7.Image = My.Resources.Resources.invoice
        KBotNavItem7.Key = "receptii"
        KBotNavItem7.Text = "Recepții"
        KBotNavItem7.Visible = False
        navDetaliu.Items.Add(KBotNavItem7)
        KBotNavItem6.Align = KBotNavAlign.Far
        KBotNavItem6.AutoSize = True
        KBotNavItem6.Image = My.Resources.Resources.table
        KBotNavItem6.Key = "xml"
        KBotNavItem6.Text = "XML Factură"
        KBotNavItem6.Visible = False
        navDetaliu.Items.Add(KBotNavItem6)
        navDetaliu.Location = New System.Drawing.Point(0, 0)
        navDetaliu.Name = "navDetaliu"
        navDetaliu.Orientation = KBotNavOrientation.Horizontal
        navDetaliu.SelectedKey = Nothing
        navDetaliu.Size = New System.Drawing.Size(674, 30)
        navDetaliu.TabIndex = 0
        '
        ' ntfMesaj
        '
        ntfMesaj.BackColor = Drawing.Color.Transparent
        ntfMesaj.Dock = System.Windows.Forms.DockStyle.Bottom
        ntfMesaj.Location = New System.Drawing.Point(0, 516)
        ntfMesaj.Margin = New System.Windows.Forms.Padding(4)
        ntfMesaj.Name = "ntfMesaj"
        ntfMesaj.Size = New System.Drawing.Size(680, 84)
        ntfMesaj.TabIndex = 1
        ntfMesaj.TabStop = False
        ntfMesaj.Visible = False
        '
        ' PrimiteView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyBody)
        Name = "PrimiteView"
        Size = New System.Drawing.Size(1000, 600)
        tlyBody.ResumeLayout(False)
        pnlArbore.ResumeLayout(False)
        pnlDetaliu.ResumeLayout(False)
        pnlNav.ResumeLayout(False)
        pnlPages.ResumeLayout(False)
        CType(navDetaliu, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents pnlArbore As System.Windows.Forms.Panel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents pnlDetaliu As System.Windows.Forms.Panel
    Friend WithEvents pnlNav As System.Windows.Forms.Panel
    Friend WithEvents pnlPages As System.Windows.Forms.Panel
    Friend WithEvents pgLinii As PrimiteLiniiPage
    Friend WithEvents pgPdf As PrimitePdfPage
    Friend WithEvents pgMesaje As PrimiteMesajePage
    Friend WithEvents pgReceptii As PrimiteReceptiiPage
    Friend WithEvents pgXml As XmlViz
    Friend WithEvents navDetaliu As KBotNavList
    Friend WithEvents ntfMesaj As KBotNotice
End Class
