Imports KBot.Controls

' «Sabloane» window (slice AV01-14, the salary page AV01-15, a MODAL window since AV01-16: 1900 x 1000 px at 150 % = 1267 x 667 at 96 dpi): the shell. A vertical bar (like the one of the main window) chooses the page, one at a time:
' the operation templates with a classification (type M), the operation templates without one (type A) and the salary templates.
' All controls are declared HERE (docs/kbot-forms-ui-convention.md). Coordinates are written at 96 dpi.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SabloaneForm
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
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        tlyMain = New KBotTableLayoutPanel()
        capBar = New KBotCaptionBar()
        pnlWork = New Panel()
        pnlHost = New Panel()
        viewSalarii = New SablonSalariiView()
        viewOperatiiA = New SablonOperatiiView()
        viewOperatiiM = New SablonOperatiiView()
        navSabloane = New KBotNavList()
        tlyMain.SuspendLayout()
        pnlWork.SuspendLayout()
        pnlHost.SuspendLayout()
        CType(navSabloane, ComponentModel.ISupportInitialize).BeginInit()
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
        capBar.Text = "K-BOT — Șabloane"
        ' 
        ' pnlWork
        ' 
        pnlWork.Controls.Add(pnlHost)
        pnlWork.Controls.Add(navSabloane)
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
        pnlHost.Controls.Add(viewSalarii)
        pnlHost.Controls.Add(viewOperatiiA)
        pnlHost.Controls.Add(viewOperatiiM)
        pnlHost.Dock = DockStyle.Fill
        pnlHost.Location = New Point(199, 14)
        pnlHost.Margin = New Padding(0)
        pnlHost.Name = "pnlHost"
        pnlHost.Padding = New Padding(10, 0, 0, 0)
        pnlHost.Size = New Size(1685, 900)
        pnlHost.TabIndex = 1
        ' 
        ' viewSalarii
        ' 
        viewSalarii.Dock = DockStyle.Fill
        viewSalarii.Font = New Font("Calibri", 9F)
        viewSalarii.Location = New Point(10, 0)
        viewSalarii.Margin = New Padding(0)
        viewSalarii.Name = "viewSalarii"
        viewSalarii.Size = New Size(1675, 900)
        viewSalarii.TabIndex = 2
        viewSalarii.Visible = False
        ' 
        ' viewOperatiiA
        ' 
        viewOperatiiA.Dock = DockStyle.Fill
        viewOperatiiA.Font = New Font("Calibri", 9F)
        viewOperatiiA.Location = New Point(10, 0)
        viewOperatiiA.Margin = New Padding(0)
        viewOperatiiA.Name = "viewOperatiiA"
        viewOperatiiA.Size = New Size(1675, 900)
        viewOperatiiA.TabIndex = 1
        viewOperatiiA.Visible = False
        ' 
        ' viewOperatiiM
        ' 
        viewOperatiiM.Dock = DockStyle.Fill
        viewOperatiiM.Font = New Font("Calibri", 9F)
        viewOperatiiM.Location = New Point(10, 0)
        viewOperatiiM.Margin = New Padding(0)
        viewOperatiiM.Name = "viewOperatiiM"
        viewOperatiiM.Size = New Size(1675, 900)
        viewOperatiiM.TabIndex = 0
        ' 
        ' navSabloane
        ' 
        navSabloane.CollapseButtonSize = 14
        navSabloane.CollapseCollapsedImage = My.Resources.Resources.expand_24
        navSabloane.CollapseCorner = KBotNavCorner.BottomLeft
        navSabloane.CollapseExpandedImage = My.Resources.Resources.collapse_24
        navSabloane.Collapsible = True
        navSabloane.Dock = DockStyle.Left
        navSabloane.FlyoutDelay = 150
        navSabloane.FlyoutSlideDuration = 100
        navSabloane.ItemCornerRadius = 8
        navSabloane.ItemPadding = New Padding(0)
        KBotNavItem1.Image = My.Resources.Resources.cells
        KBotNavItem1.Key = "op_m"
        KBotNavItem1.Text = "Clasificații"
        KBotNavItem2.Image = My.Resources.Resources.folder_open
        KBotNavItem2.Key = "op_a"
        KBotNavItem2.Text = "Diverse"
        KBotNavItem3.Image = My.Resources.Resources.credit_card
        KBotNavItem3.Key = "salarii"
        KBotNavItem3.Text = "Salarii"
        navSabloane.Items.Add(KBotNavItem1)
        navSabloane.Items.Add(KBotNavItem2)
        navSabloane.Items.Add(KBotNavItem3)
        navSabloane.Location = New Point(10, 14)
        navSabloane.Margin = New Padding(4, 4, 4, 4)
        navSabloane.Name = "navSabloane"
        navSabloane.SelectedKey = "op_m"
        navSabloane.Size = New Size(189, 900)
        navSabloane.TabIndex = 0
        ' 
        ' SabloaneForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1900, 1000)
        Controls.Add(tlyMain)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MinimizeBox = False
        MinimumSize = New Size(1650, 960)
        Name = "SabloaneForm"
        Padding = New Padding(3, 3, 3, 3)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "K-BOT — Șabloane"
        tlyMain.ResumeLayout(False)
        pnlWork.ResumeLayout(False)
        pnlHost.ResumeLayout(False)
        CType(navSabloane, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents pnlWork As Panel
    Friend WithEvents pnlHost As Panel
    Friend WithEvents viewOperatiiM As SablonOperatiiView
    Friend WithEvents viewOperatiiA As SablonOperatiiView
    Friend WithEvents viewSalarii As SablonSalariiView
    Friend WithEvents navSabloane As KBotNavList
End Class
