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
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyBody = New KBotTableLayoutPanel()
        pnlArbore = New System.Windows.Forms.Panel()
        tree = New AdvancedTreeControl()
        chkToate = New System.Windows.Forms.CheckBox()
        txtCauta = New KBotTextField()
        pnlDetaliu = New System.Windows.Forms.Panel()
        pnlPages = New System.Windows.Forms.Panel()
        pgLinii = New PrimiteLiniiPage()
        pgPdf = New VanzarePdfPage()
        pgAtasamente = New PrimiteAtasamentePage()
        pgMesaje = New PrimiteMesajePage()
        navDetaliu = New KBotNavList()
        ntfMesaj = New KBotNotice()
        tlyBody.SuspendLayout()
        pnlArbore.SuspendLayout()
        pnlDetaliu.SuspendLayout()
        pnlPages.SuspendLayout()
        CType(navDetaliu, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyBody
        '
        tlyBody.ColumnCount = 2
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.0!))
        tlyBody.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68.0!))
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
        pnlArbore.Controls.Add(chkToate)
        pnlArbore.Controls.Add(txtCauta)
        pnlArbore.Dock = System.Windows.Forms.DockStyle.Fill
        pnlArbore.Location = New System.Drawing.Point(0, 0)
        pnlArbore.Margin = New System.Windows.Forms.Padding(0)
        pnlArbore.Name = "pnlArbore"
        pnlArbore.Size = New System.Drawing.Size(320, 600)
        pnlArbore.TabIndex = 0
        '
        ' txtCauta
        '
        txtCauta.BackColor = System.Drawing.Color.Transparent
        txtCauta.Dock = System.Windows.Forms.DockStyle.Top
        txtCauta.Location = New System.Drawing.Point(0, 0)
        txtCauta.MaxLength = 100
        txtCauta.Name = "txtCauta"
        txtCauta.PlaceholderText = "Caută după furnizor sau număr…"
        txtCauta.Size = New System.Drawing.Size(320, 48)
        txtCauta.TabIndex = 2
        tips.SetToolTipHeader(txtCauta, "Căutare")
        tips.SetToolTipText(txtCauta, "Arată în arbore doar facturile al căror furnizor, cod fiscal sau număr conține textul scris.")
        txtCauta.Visible = False
        '
        ' chkToate
        '
        chkToate.AutoSize = False
        chkToate.Dock = System.Windows.Forms.DockStyle.Top
        chkToate.Location = New System.Drawing.Point(0, 0)
        chkToate.Margin = New System.Windows.Forms.Padding(0)
        chkToate.Name = "chkToate"
        chkToate.Padding = New System.Windows.Forms.Padding(8, 0, 0, 0)
        chkToate.Size = New System.Drawing.Size(320, 28)
        chkToate.TabIndex = 1
        chkToate.Text = "Arată toate facturile primite"
        tips.SetToolTipText(chkToate, "Lista cuprinde și facturile care nu sunt legate de acest DDF. Clic dreapta pe una o leagă de DDF.")
        chkToate.UseVisualStyleBackColor = True
        chkToate.Visible = False
        '
        ' tree
        '
        tree.BorderColor = Drawing.SystemColors.ActiveBorder
        tree.CollapseButtonTooltip = "Strânge arborele la o bandă îngustă." & vbLf & "Rândurile se citesc atunci prin eticheta care iese la survolare."
        tree.Dock = System.Windows.Forms.DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpandButtonTooltip = "Desfă arborele la loc, pe toată lățimea lui."
        tree.ExpanderSize = 10
        tree.Font = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderBackColor = Drawing.SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " FACTURI PRIMITE, PE LUNI"
        tree.HeaderFont = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
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
        tree.Location = New System.Drawing.Point(0, 28)
        tree.Margin = New System.Windows.Forms.Padding(0)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 8
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = False
        tree.RightClickSelects = False
        tree.RightIconSize = New System.Drawing.Size(14, 14)
        tree.ShowRightIconOnHover = False
        tree.Size = New System.Drawing.Size(320, 572)
        tree.TabIndex = 0
        '
        ' pnlDetaliu
        '
        pnlDetaliu.Controls.Add(pnlPages)
        pnlDetaliu.Controls.Add(navDetaliu)
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
        pnlPages.Controls.Add(pgAtasamente)
        pnlPages.Controls.Add(pgMesaje)
        pnlPages.Dock = System.Windows.Forms.DockStyle.Fill
        pnlPages.Location = New System.Drawing.Point(0, 40)
        pnlPages.Margin = New System.Windows.Forms.Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New System.Drawing.Size(680, 476)
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
        ' pgAtasamente
        '
        pgAtasamente.Dock = System.Windows.Forms.DockStyle.Fill
        pgAtasamente.Location = New System.Drawing.Point(0, 0)
        pgAtasamente.Margin = New System.Windows.Forms.Padding(0)
        pgAtasamente.Name = "pgAtasamente"
        pgAtasamente.Size = New System.Drawing.Size(680, 476)
        pgAtasamente.TabIndex = 2
        pgAtasamente.Visible = False
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
        ' navDetaliu
        '
        navDetaliu.Dock = System.Windows.Forms.DockStyle.Top
        navDetaliu.IconSize = 18
        navDetaliu.ItemCornerRadius = 2
        navDetaliu.ItemPadding = New System.Windows.Forms.Padding(0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.table
        KBotNavItem1.Key = "linii"
        KBotNavItem1.Text = "Linii"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.invoice
        KBotNavItem2.Key = "pdf"
        KBotNavItem2.Text = "Factură PDF"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.attach
        KBotNavItem3.Key = "atasamente"
        KBotNavItem3.Text = "Atașamente"
        KBotNavItem3.Visible = False
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.cells
        KBotNavItem4.Key = "mesaje"
        KBotNavItem4.Text = "Mesaje"
        KBotNavItem4.Visible = False
        navDetaliu.Items.Add(KBotNavItem1)
        navDetaliu.Items.Add(KBotNavItem2)
        navDetaliu.Items.Add(KBotNavItem3)
        navDetaliu.Items.Add(KBotNavItem4)
        navDetaliu.Location = New System.Drawing.Point(0, 0)
        navDetaliu.Name = "navDetaliu"
        navDetaliu.Orientation = KBotNavOrientation.Horizontal
        navDetaliu.SelectedKey = Nothing
        navDetaliu.Size = New System.Drawing.Size(680, 40)
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
        pnlPages.ResumeLayout(False)
        CType(navDetaliu, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyBody As KBotTableLayoutPanel
    Friend WithEvents pnlArbore As System.Windows.Forms.Panel
    Friend WithEvents chkToate As System.Windows.Forms.CheckBox
    Friend WithEvents txtCauta As KBotTextField
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents pnlDetaliu As System.Windows.Forms.Panel
    Friend WithEvents pnlPages As System.Windows.Forms.Panel
    Friend WithEvents pgLinii As PrimiteLiniiPage
    Friend WithEvents pgPdf As VanzarePdfPage
    Friend WithEvents pgAtasamente As PrimiteAtasamentePage
    Friend WithEvents pgMesaje As PrimiteMesajePage
    Friend WithEvents navDetaliu As KBotNavList
    Friend WithEvents ntfMesaj As KBotNotice
End Class
