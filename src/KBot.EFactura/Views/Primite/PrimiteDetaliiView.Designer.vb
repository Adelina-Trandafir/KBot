Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PrimiteDetaliiView
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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem4 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem5 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem6 As KBotNavItem = New KBotNavItem()
        pnlPages = New System.Windows.Forms.Panel()
        pgContare = New PrimiteContarePage()
        pgLinii = New PrimiteLiniiPage()
        pgPdf = New PrimitePdfPage()
        pgMesaje = New PrimiteMesajePage()
        pgXml = New XmlViz()
        pnlNav = New System.Windows.Forms.Panel()
        navDetaliu = New KBotNavList()
        pnlPages.SuspendLayout()
        pnlNav.SuspendLayout()
        CType(navDetaliu, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlPages
        ' 
        pnlPages.Controls.Add(pgContare)
        pnlPages.Controls.Add(pgLinii)
        pnlPages.Controls.Add(pgPdf)
        pnlPages.Controls.Add(pgMesaje)
        pnlPages.Controls.Add(pgXml)
        pnlPages.Dock = System.Windows.Forms.DockStyle.Fill
        pnlPages.Location = New System.Drawing.Point(0, 45)
        pnlPages.Margin = New System.Windows.Forms.Padding(0)
        pnlPages.Name = "pnlPages"
        pnlPages.Size = New System.Drawing.Size(1140, 811)
        pnlPages.TabIndex = 2
        ' 
        ' pgContare
        ' 
        pgContare.Dock = System.Windows.Forms.DockStyle.Fill
        pgContare.Font = New System.Drawing.Font("Calibri", 9F)
        pgContare.Location = New System.Drawing.Point(0, 0)
        pgContare.Margin = New System.Windows.Forms.Padding(0)
        pgContare.Name = "pgContare"
        pgContare.Padding = New System.Windows.Forms.Padding(0)
        pgContare.Size = New System.Drawing.Size(1140, 811)
        pgContare.TabIndex = 6
        pgContare.Visible = False
        ' 
        ' pgLinii
        ' 
        pgLinii.Dock = System.Windows.Forms.DockStyle.Fill
        pgLinii.Font = New System.Drawing.Font("Calibri", 9F)
        pgLinii.Location = New System.Drawing.Point(0, 0)
        pgLinii.Margin = New System.Windows.Forms.Padding(0)
        pgLinii.Name = "pgLinii"
        pgLinii.Padding = New System.Windows.Forms.Padding(0)
        pgLinii.Size = New System.Drawing.Size(1140, 811)
        pgLinii.TabIndex = 0
        ' 
        ' pgPdf
        ' 
        pgPdf.Dock = System.Windows.Forms.DockStyle.Fill
        pgPdf.Font = New System.Drawing.Font("Calibri", 9F)
        pgPdf.Location = New System.Drawing.Point(0, 0)
        pgPdf.Margin = New System.Windows.Forms.Padding(0)
        pgPdf.Name = "pgPdf"
        pgPdf.Size = New System.Drawing.Size(1140, 811)
        pgPdf.TabIndex = 1
        pgPdf.Visible = False
        ' 
        ' pgMesaje
        ' 
        pgMesaje.Dock = System.Windows.Forms.DockStyle.Fill
        pgMesaje.Font = New System.Drawing.Font("Calibri", 9F)
        pgMesaje.Location = New System.Drawing.Point(0, 0)
        pgMesaje.Margin = New System.Windows.Forms.Padding(0)
        pgMesaje.Name = "pgMesaje"
        pgMesaje.Padding = New System.Windows.Forms.Padding(0)
        pgMesaje.Size = New System.Drawing.Size(1140, 811)
        pgMesaje.TabIndex = 3
        pgMesaje.Visible = False
        ' 
        ' pgXml
        ' 
        pgXml.Dock = System.Windows.Forms.DockStyle.Fill
        pgXml.Font = New System.Drawing.Font("Calibri", 9F)
        pgXml.Location = New System.Drawing.Point(0, 0)
        pgXml.Margin = New System.Windows.Forms.Padding(0)
        pgXml.Name = "pgXml"
        pgXml.Padding = New System.Windows.Forms.Padding(0)
        pgXml.Size = New System.Drawing.Size(1140, 811)
        pgXml.TabIndex = 5
        pgXml.Visible = False
        ' 
        ' pnlNav
        ' 
        pnlNav.Controls.Add(navDetaliu)
        pnlNav.Dock = System.Windows.Forms.DockStyle.Top
        pnlNav.Location = New System.Drawing.Point(0, 0)
        pnlNav.Margin = New System.Windows.Forms.Padding(0)
        pnlNav.Name = "pnlNav"
        pnlNav.Padding = New System.Windows.Forms.Padding(0)
        pnlNav.Size = New System.Drawing.Size(1140, 45)
        pnlNav.TabIndex = 0
        ' 
        ' navDetaliu
        ' 
        navDetaliu.Dock = System.Windows.Forms.DockStyle.Fill
        navDetaliu.IconSize = 18
        navDetaliu.ItemCornerRadius = 2
        navDetaliu.ItemPadding = New System.Windows.Forms.Padding(0)
        KBotNavItem1.AutoSize = True
        KBotNavItem1.Image = My.Resources.Resources.cells
        KBotNavItem1.Key = "contare"
        KBotNavItem1.Text = "Contări"
        KBotNavItem2.AutoSize = True
        KBotNavItem2.Image = My.Resources.Resources.table
        KBotNavItem2.Key = "linii"
        KBotNavItem2.Text = "Conținut"
        KBotNavItem3.AutoSize = True
        KBotNavItem3.Image = My.Resources.Resources.invoice
        KBotNavItem3.Key = "pdf"
        KBotNavItem3.Text = "Factură PDF"
        KBotNavItem4.AutoSize = True
        KBotNavItem4.Image = My.Resources.Resources.cells
        KBotNavItem4.Key = "mesaje"
        KBotNavItem4.Text = "Mesaje"
        KBotNavItem4.Visible = False
        KBotNavItem5.AutoSize = True
        KBotNavItem5.Image = My.Resources.Resources.invoice
        KBotNavItem5.Key = "receptii"
        KBotNavItem5.Text = "Recepții"
        KBotNavItem5.Visible = False
        KBotNavItem6.Align = KBotNavAlign.Far
        KBotNavItem6.AutoSize = True
        KBotNavItem6.Image = My.Resources.Resources.table
        KBotNavItem6.Key = "xml"
        KBotNavItem6.Text = "XML Factură"
        KBotNavItem6.Visible = False
        navDetaliu.Items.Add(KBotNavItem1)
        navDetaliu.Items.Add(KBotNavItem2)
        navDetaliu.Items.Add(KBotNavItem3)
        navDetaliu.Items.Add(KBotNavItem4)
        navDetaliu.Items.Add(KBotNavItem5)
        navDetaliu.Items.Add(KBotNavItem6)
        navDetaliu.Location = New System.Drawing.Point(4, 0)
        navDetaliu.Margin = New System.Windows.Forms.Padding(0)
        navDetaliu.Name = "navDetaliu"
        navDetaliu.Orientation = KBotNavOrientation.Horizontal
        navDetaliu.SelectedKey = "linii"
        navDetaliu.Size = New System.Drawing.Size(1132, 45)
        navDetaliu.TabIndex = 0
        '
        ' PrimiteDetaliiView
        '
        AutoScaleDimensions = New System.Drawing.SizeF(144F, 144F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(pnlPages)
        Controls.Add(pnlNav)
        Margin = New System.Windows.Forms.Padding(0)
        Name = "PrimiteDetaliiView"
        Size = New System.Drawing.Size(1140, 856)
        pnlPages.ResumeLayout(False)
        pnlNav.ResumeLayout(False)
        CType(navDetaliu, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlNav As System.Windows.Forms.Panel
    Friend WithEvents pnlPages As System.Windows.Forms.Panel
    Friend WithEvents pgLinii As PrimiteLiniiPage
    Friend WithEvents pgContare As PrimiteContarePage
    Friend WithEvents pgPdf As PrimitePdfPage
    Friend WithEvents pgMesaje As PrimiteMesajePage
    Friend WithEvents pgXml As XmlViz
    Friend WithEvents navDetaliu As KBotNavList
End Class
