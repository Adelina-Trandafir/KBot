Imports KBot.Controls

' «OperatiiSabloane» window (slices AV02-03 / AV02-04, a MODAL window like «Sabloane»: 1900 x 1000 px at 150 % = 1267 x 667 at 96 dpi): the shell. A vertical bar chooses
' the page, one at a time: «Operatii diverse» (the Access form OP9) and «Facturi si plati» (OPuri2017). «Salarii» is added later as the third page.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OperatiiSabloaneForm
    Inherits Global.KBot.Theming.KBotModalForm

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
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlWork = New Panel()
        pnlHost = New Panel()
        pageFacturi = New FacturiPlatiPage()
        pageDiverse = New OperatiiDiversePage()
        navOperatii = New KBotNavList()
        tlyMain.SuspendLayout()
        pnlWork.SuspendLayout()
        pnlHost.SuspendLayout()
        CType(navOperatii, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlWork, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(3, 3)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 2
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1894, 994)
        tlyMain.TabIndex = 0
        '
        ' capBar
        '
        capBar.Dock = DockStyle.Fill
        capBar.IconImage = My.Resources.Resources.cells
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(1894, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Operații contabile"
        '
        ' pnlWork
        '
        pnlWork.Controls.Add(pnlHost)
        pnlWork.Controls.Add(navOperatii)
        pnlWork.Dock = DockStyle.Fill
        pnlWork.Location = New Point(0, 66)
        pnlWork.Margin = New Padding(0)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(10, 14, 10, 14)
        pnlWork.Size = New Size(1894, 928)
        pnlWork.TabIndex = 1
        pnlWork.Tag = "Card"
        '
        ' pnlHost
        '
        pnlHost.Controls.Add(pageFacturi)
        pnlHost.Controls.Add(pageDiverse)
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Location = New Point(199, 14)
        pnlHost.Margin = New Padding(0)
        pnlHost.Name = "pnlHost"
        pnlHost.Padding = New Padding(10, 0, 0, 0)
        pnlHost.Size = New Size(1685, 900)
        pnlHost.TabIndex = 1
        '
        ' pageFacturi
        '
        pageFacturi.Dock = DockStyle.Fill
        pageFacturi.Font = New Font("Calibri", 9F)
        pageFacturi.Location = New Point(10, 0)
        pageFacturi.Margin = New Padding(0)
        pageFacturi.Name = "pageFacturi"
        pageFacturi.Size = New Size(1675, 900)
        pageFacturi.TabIndex = 1
        pageFacturi.Visible = False
        '
        ' pageDiverse
        '
        pageDiverse.Dock = DockStyle.Fill
        pageDiverse.Font = New Font("Calibri", 9F)
        pageDiverse.Location = New Point(10, 0)
        pageDiverse.Margin = New Padding(0)
        pageDiverse.Name = "pageDiverse"
        pageDiverse.Size = New Size(1675, 900)
        pageDiverse.TabIndex = 0
        '
        ' navOperatii
        '
        navOperatii.CollapseButtonSize = 14
        navOperatii.CollapseCollapsedImage = My.Resources.Resources.expand_24
        navOperatii.CollapseCorner = KBotNavCorner.BottomLeft
        navOperatii.CollapseExpandedImage = My.Resources.Resources.collapse_24
        navOperatii.Collapsible = True
        navOperatii.Dock = DockStyle.Left
        navOperatii.FlyoutDelay = 150
        navOperatii.FlyoutSlideDuration = 100
        navOperatii.ItemCornerRadius = 8
        navOperatii.ItemPadding = New Padding(0)
        KBotNavItem1.Image = My.Resources.Resources.folder_open
        KBotNavItem1.Key = "diverse"
        KBotNavItem1.Text = "Diverse"
        KBotNavItem2.Image = My.Resources.Resources.credit_card
        KBotNavItem2.Key = "facturi"
        KBotNavItem2.Text = "Facturi și plăți"
        navOperatii.Items.Add(KBotNavItem1)
        navOperatii.Items.Add(KBotNavItem2)
        navOperatii.Location = New Point(10, 14)
        navOperatii.Margin = New Padding(4, 4, 4, 4)
        navOperatii.Name = "navOperatii"
        navOperatii.SelectedKey = "diverse"
        navOperatii.Size = New Size(189, 900)
        navOperatii.TabIndex = 0
        '
        ' OperatiiSabloaneForm
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1900, 1000)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MinimizeBox = False
        MinimumSize = New Size(1650, 960)
        Name = "OperatiiSabloaneForm"
        Padding = New Padding(3, 3, 3, 3)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Operații contabile"
        tlyMain.ResumeLayout(False)
        pnlWork.ResumeLayout(False)
        pnlHost.ResumeLayout(False)
        CType(navOperatii, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlWork As Panel
    Friend WithEvents pnlHost As Panel
    Friend WithEvents pageDiverse As OperatiiDiversePage
    Friend WithEvents pageFacturi As FacturiPlatiPage
    Friend WithEvents navOperatii As KBotNavList
End Class
