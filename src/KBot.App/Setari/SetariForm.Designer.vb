Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SetariForm
    Inherits Global.KBot.Theming.KBotShellForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
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
        Dim KBotNavItem7 As KBotNavItem = New KBotNavItem()
        tips = New KBotToolTip(components)
        btnClose = New Button()
        pnlRoot = New Panel()
        pnlWork = New Panel()
        viewHost = New Panel()
        navViews = New KBotNavList()
        pnlStatus = New Panel()
        tlyStatus = New KBotTableLayoutPanel()
        lblStatus = New Label()
        busyBar = New KBotBusyBar()
        capBar = New KBotCaptionBar()
        pnlRoot.SuspendLayout()
        pnlWork.SuspendLayout()
        CType(navViews, ComponentModel.ISupportInitialize).BeginInit()
        pnlStatus.SuspendLayout()
        tlyStatus.SuspendLayout()
        SuspendLayout()
        ' 
        ' btnClose
        ' 
        btnClose.Dock = DockStyle.Fill
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.Font = New Font("Segoe UI", 9F)
        btnClose.Location = New Point(1007, 12)
        btnClose.Margin = New Padding(4, 0, 4, 0)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(172, 49)
        btnClose.TabIndex = 1
        btnClose.Text = "Închide"
        tips.SetToolTipHeader(btnClose, "Închide")
        tips.SetToolTipText(btnClose, "Închide fereastra de setări. Setările se salvează pe măsură ce le schimbi.")
        btnClose.UseVisualStyleBackColor = True
        ' 
        ' pnlRoot
        ' 
        pnlRoot.Controls.Add(pnlWork)
        pnlRoot.Controls.Add(pnlStatus)
        pnlRoot.Controls.Add(busyBar)
        pnlRoot.Controls.Add(capBar)
        pnlRoot.Dock = DockStyle.Fill
        pnlRoot.Location = New Point(1, 2)
        pnlRoot.Margin = New Padding(4, 5, 4, 5)
        pnlRoot.Name = "pnlRoot"
        pnlRoot.Size = New Size(1198, 896)
        pnlRoot.TabIndex = 0
        pnlRoot.Tag = "Card"
        ' 
        ' pnlWork
        ' 
        pnlWork.Controls.Add(viewHost)
        pnlWork.Controls.Add(navViews)
        pnlWork.Dock = DockStyle.Fill
        pnlWork.Location = New Point(0, 62)
        pnlWork.Margin = New Padding(4, 5, 4, 5)
        pnlWork.Name = "pnlWork"
        pnlWork.Padding = New Padding(11, 13, 11, 13)
        pnlWork.Size = New Size(1198, 761)
        pnlWork.TabIndex = 0
        ' 
        ' viewHost
        ' 
        viewHost.Dock = DockStyle.Fill
        viewHost.Location = New Point(241, 13)
        viewHost.Margin = New Padding(4, 5, 4, 5)
        viewHost.Name = "viewHost"
        viewHost.Padding = New Padding(11, 0, 0, 0)
        viewHost.Size = New Size(946, 735)
        viewHost.TabIndex = 1
        ' 
        ' navViews
        ' 
        navViews.CollapseButtonSize = 14
        navViews.CollapseCollapsedImage = My.Resources.Resources.expand_24
        navViews.CollapseCorner = KBotNavCorner.BottomLeft
        navViews.CollapseExpandedImage = My.Resources.Resources.collapse_24
        navViews.Collapsible = True
        navViews.Dock = DockStyle.Left
        navViews.FlyoutDelay = 150
        navViews.FlyoutSlideDuration = 100
        navViews.ItemCornerRadius = 8
        navViews.ItemPadding = New Padding(0)
        KBotNavItem1.Image = My.Resources.Resources.Wefunction_Woofunction_Window_app_list_info_32
        KBotNavItem1.Key = "info"
        KBotNavItem1.Text = "Informații"
        KBotNavItem2.Image = My.Resources.Resources.settings__1_
        KBotNavItem2.Key = "aplicatie"
        KBotNavItem2.Text = "Aplicație"
        KBotNavItem3.Image = My.Resources.Resources.butonforexebug2022
        KBotNavItem3.Key = "forexe"
        KBotNavItem3.Text = "FOREXE"
        KBotNavItem4.Image = My.Resources.Resources.Papirus_Team_Papirus_Apps_Preferences_desktop_theme_24
        KBotNavItem4.Key = "tema"
        KBotNavItem4.Text = "Temă"
        KBotNavItem5.Image = My.Resources.Resources.Umut_Pulat_Tulliana_2_File_locked_32
        KBotNavItem5.Key = "autentificare"
        KBotNavItem5.Text = "Autentificare"
        KBotNavItem6.Align = KBotNavAlign.Far
        KBotNavItem6.Image = My.Resources.Resources.Papirus_Team_Papirus_Apps_Accessories_text_editor_512_resized
        KBotNavItem6.Key = "jurnal"
        KBotNavItem6.Text = "Jurnal"
        KBotNavItem7.Align = KBotNavAlign.Far
        KBotNavItem7.IsSeparator = True
        KBotNavItem7.Key = "jur2"
        KBotNavItem7.Text = Nothing
        navViews.Items.Add(KBotNavItem1)
        navViews.Items.Add(KBotNavItem2)
        navViews.Items.Add(KBotNavItem3)
        navViews.Items.Add(KBotNavItem4)
        navViews.Items.Add(KBotNavItem5)
        navViews.Items.Add(KBotNavItem6)
        navViews.Items.Add(KBotNavItem7)
        navViews.Location = New Point(11, 13)
        navViews.Margin = New Padding(4, 5, 4, 5)
        navViews.Name = "navViews"
        navViews.SelectedKey = Nothing
        navViews.Size = New Size(230, 735)
        navViews.TabIndex = 0
        ' 
        ' pnlStatus
        ' 
        pnlStatus.Controls.Add(tlyStatus)
        pnlStatus.Dock = DockStyle.Bottom
        pnlStatus.Location = New Point(0, 823)
        pnlStatus.Margin = New Padding(4, 5, 4, 5)
        pnlStatus.Name = "pnlStatus"
        pnlStatus.Size = New Size(1198, 73)
        pnlStatus.TabIndex = 1
        pnlStatus.Tag = "Card"
        ' 
        ' tlyStatus
        ' 
        tlyStatus.ColumnCount = 2
        tlyStatus.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyStatus.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 180F))
        tlyStatus.Controls.Add(lblStatus, 0, 0)
        tlyStatus.Controls.Add(btnClose, 1, 0)
        tlyStatus.Dock = DockStyle.Fill
        tlyStatus.Location = New Point(0, 0)
        tlyStatus.Margin = New Padding(0)
        tlyStatus.Name = "tlyStatus"
        tlyStatus.Padding = New Padding(15, 12, 15, 12)
        tlyStatus.RowCount = 1
        tlyStatus.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyStatus.Size = New Size(1198, 73)
        tlyStatus.TabIndex = 0
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoEllipsis = True
        lblStatus.Dock = DockStyle.Fill
        lblStatus.Location = New Point(19, 12)
        lblStatus.Margin = New Padding(4, 0, 4, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(980, 49)
        lblStatus.TabIndex = 0
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' busyBar
        ' 
        busyBar.Dock = DockStyle.Top
        busyBar.Location = New Point(0, 57)
        busyBar.Margin = New Padding(4, 5, 4, 5)
        busyBar.Name = "busyBar"
        busyBar.Size = New Size(1198, 5)
        busyBar.TabIndex = 2
        busyBar.TabStop = False
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.settings__1_
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(4, 5, 4, 5)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowMaximize = True
        capBar.ShowThemeButton = True
        capBar.Size = New Size(1198, 57)
        capBar.TabIndex = 3
        capBar.TabStop = False
        capBar.Text = "K-BOT — Setări"
        capBar.TintOptionButtonImage = False
        ' 
        ' SetariForm
        ' 
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1200, 900)
        Controls.Add(pnlRoot)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 5, 4, 5)
        MinimumSize = New Size(1000, 760)
        Name = "SetariForm"
        Padding = New Padding(1, 2, 1, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterScreen
        Text = "K-BOT — Setări"
        pnlRoot.ResumeLayout(False)
        pnlWork.ResumeLayout(False)
        CType(navViews, ComponentModel.ISupportInitialize).EndInit()
        pnlStatus.ResumeLayout(False)
        tlyStatus.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlRoot As Panel
    Friend WithEvents capBar As KBotCaptionBar
    Friend WithEvents busyBar As KBotBusyBar
    Friend WithEvents pnlWork As Panel
    Friend WithEvents navViews As KBotNavList
    Friend WithEvents viewHost As Panel
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents tlyStatus As KBotTableLayoutPanel
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnClose As Button
End Class
