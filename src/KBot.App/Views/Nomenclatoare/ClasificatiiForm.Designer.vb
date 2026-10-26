Imports KBot.Controls

' «Clasificatii» window (slice AV01-11): the shell. A vertical bar (like the one of the main window) chooses the page, one at a time:
' «Buget» (ClasificatiiBugetView, the former content of this window) and «Venituri» (ClasificatiiVenituriView).
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 144 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ClasificatiiForm
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
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlWork = New Panel()
        pnlHost = New Panel()
        viewVenituri = New ClasificatiiVenituriView()
        viewBuget = New ClasificatiiBugetView()
        navClasificatii = New KBotNavList()
        CType(navClasificatii, ComponentModel.ISupportInitialize).BeginInit()
        tlyMain.SuspendLayout()
        pnlWork.SuspendLayout()
        pnlHost.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain
        '
        tlyMain.ColumnCount = 1
        tlyMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyMain.Controls.Add(capBar, 0, 0)
        tlyMain.Controls.Add(pnlWork, 0, 1)
        tlyMain.Dock = DockStyle.Fill
        tlyMain.Location = New Point(2, 2)
        tlyMain.Margin = New Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 2
        tlyMain.RowStyles.Add(New RowStyle())
        tlyMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyMain.Size = New Size(1818, 776)
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
        capBar.Size = New Size(1818, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "K-BOT — Clasificații cheltuieli"
        '
        ' pnlWork
        '
        pnlWork.Controls.Add(pnlHost)
        pnlWork.Controls.Add(navClasificatii)
        pnlWork.Dock = DockStyle.Fill
        pnlWork.Location = New Point(0, 66)
        pnlWork.Margin = New Padding(0)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(11, 13, 11, 13)
        pnlWork.Size = New Size(1818, 710)
        pnlWork.TabIndex = 1
        pnlWork.Tag = "Card"
        '
        ' pnlHost
        '
        pnlHost.Controls.Add(viewVenituri)
        pnlHost.Controls.Add(viewBuget)
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Location = New Point(230, 13)
        pnlHost.Margin = New Padding(0)
        pnlHost.Name = "pnlHost"
        pnlHost.Padding = New Padding(11, 0, 0, 0)
        pnlHost.Size = New Size(1577, 684)
        pnlHost.TabIndex = 1
        '
        ' viewVenituri
        '
        viewVenituri.Dock = DockStyle.Fill
        viewVenituri.Location = New Point(11, 0)
        viewVenituri.Margin = New Padding(0)
        viewVenituri.Name = "viewVenituri"
        viewVenituri.Size = New Size(1566, 684)
        viewVenituri.TabIndex = 1
        viewVenituri.Visible = False
        '
        ' viewBuget
        '
        viewBuget.Dock = DockStyle.Fill
        viewBuget.Location = New Point(11, 0)
        viewBuget.Margin = New Padding(0)
        viewBuget.Name = "viewBuget"
        viewBuget.Size = New Size(1566, 684)
        viewBuget.TabIndex = 0
        '
        ' navClasificatii
        '
        navClasificatii.CollapseButtonSize = 14
        navClasificatii.CollapseCollapsedImage = My.Resources.Resources.expand_24
        navClasificatii.CollapseCorner = KBotNavCorner.BottomLeft
        navClasificatii.CollapseExpandedImage = My.Resources.Resources.collapse_24
        navClasificatii.Collapsible = True
        navClasificatii.Dock = DockStyle.Left
        navClasificatii.FlyoutDelay = 150
        navClasificatii.FlyoutSlideDuration = 100
        navClasificatii.ItemCornerRadius = 8
        navClasificatii.ItemPadding = New Padding(0)
        KBotNavItem1.Image = My.Resources.Resources.ef_iesiri
        KBotNavItem1.Key = "buget"
        KBotNavItem1.Text = "Cheltuieli"
        KBotNavItem2.Image = My.Resources.Resources.ef_intrari
        KBotNavItem2.Key = "venituri"
        KBotNavItem2.Text = "Venituri"
        navClasificatii.Items.Add(KBotNavItem1)
        navClasificatii.Items.Add(KBotNavItem2)
        navClasificatii.Location = New Point(11, 13)
        navClasificatii.Margin = New Padding(4, 5, 4, 5)
        navClasificatii.Name = "navClasificatii"
        navClasificatii.SelectedKey = "buget"
        navClasificatii.Size = New Size(219, 684)
        navClasificatii.TabIndex = 0
        '
        ' ClasificatiiForm
        '
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1822, 780)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4)
        MinimizeBox = False
        MinimumSize = New Size(1570, 780)
        Name = "ClasificatiiForm"
        Padding = New Padding(2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Clasificații cheltuieli"
        CType(navClasificatii, ComponentModel.ISupportInitialize).EndInit()
        tlyMain.ResumeLayout(False)
        pnlWork.ResumeLayout(False)
        pnlHost.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlWork As Panel
    Friend WithEvents pnlHost As Panel
    Friend WithEvents viewBuget As ClasificatiiBugetView
    Friend WithEvents viewVenituri As ClasificatiiVenituriView
    Friend WithEvents navClasificatii As KBotNavList
End Class
