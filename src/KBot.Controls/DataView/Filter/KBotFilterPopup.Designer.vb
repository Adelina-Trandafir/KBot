Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Designer half of <see cref="KBotFilterPopup"/> (slice 0028-06, tabs in 0030, then each tab
''' split into a view of its own). By the house rule ALL child controls are declared HERE, not
''' built in code.
'''
''' <para><b>The frame only.</b> The three tabs -- «Sortare», «Filtrare», «Grupare» -- are
''' UserControls of their own (<c>KBotFilterPopupSortView</c>, <c>KBotFilterPopupFilterView</c>,
''' <c>KBotFilterPopupGroupView</c>, each with its own designer file next to this one), created
''' lazily by <c>KBotFilterPopup.vb</c> at first activation and docked <c>Fill</c> in
''' <c>viewHost</c>, one visible at a time -- exactly as <c>KbotForm</c> and <c>SetariForm</c> host
''' their views behind a <see cref="KBotNavList"/>. What stays here is what every tab shares:
''' the horizontal nav bar on top, the line under it, the host panel, and the OK / «Anuleaza»
''' bar at the bottom (shown only under the filter tab).</para>
'''
''' <para><b>Dock order is the REVERSE of the visual order</b> (the house rule for a card
''' panel): the last <c>Dock = Top</c> added ends up highest. So <c>InitializeComponent</c> adds
''' bottom-up: the host (Fill), the command bar (Bottom), the line under the navigation, then
''' the navigation.</para>
'''
''' <para>The window is borderless (it is a menu); the 1px «frame» is the form's <c>Padding</c>
''' plus its background, coloured from the theme in <c>OnThemeChanged</c> -- no colour is
''' written here.</para>
''' </summary>
Partial Class KBotFilterPopup
    Inherits KBot.Theming.KBotThemedForm

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        Dim KBotNavItem1 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem2 As KBotNavItem = New KBotNavItem()
        Dim KBotNavItem3 As KBotNavItem = New KBotNavItem()
        pnlBody = New Panel()
        viewHost = New Panel()
        pnlCommands = New Panel()
        btnOk = New Button()
        btnCancel = New Button()
        sepNav = New Panel()
        navFile = New KBotNavList()
        pnlBody.SuspendLayout()
        pnlCommands.SuspendLayout()
        CType(navFile, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' pnlBody
        ' 
        pnlBody.Controls.Add(viewHost)
        pnlBody.Controls.Add(pnlCommands)
        pnlBody.Controls.Add(sepNav)
        pnlBody.Controls.Add(navFile)
        pnlBody.Dock = DockStyle.Fill
        pnlBody.Location = New Point(1, 1)
        pnlBody.Margin = New Padding(0)
        pnlBody.Name = "pnlBody"
        pnlBody.Padding = New Padding(2)
        pnlBody.Size = New Size(338, 558)
        pnlBody.TabIndex = 0
        ' 
        ' viewHost
        ' 
        viewHost.Dock = DockStyle.Fill
        viewHost.Location = New Point(2, 45)
        viewHost.Margin = New Padding(0)
        viewHost.Name = "viewHost"
        viewHost.Size = New Size(334, 448)
        viewHost.TabIndex = 2
        ' 
        ' pnlCommands
        ' 
        pnlCommands.Controls.Add(btnOk)
        pnlCommands.Controls.Add(btnCancel)
        pnlCommands.Dock = DockStyle.Bottom
        pnlCommands.Location = New Point(2, 493)
        pnlCommands.Margin = New Padding(0)
        pnlCommands.Name = "pnlCommands"
        pnlCommands.Padding = New Padding(8)
        pnlCommands.Size = New Size(334, 63)
        pnlCommands.TabIndex = 3
        ' 
        ' btnOk
        ' 
        btnOk.Dock = DockStyle.Right
        btnOk.Location = New Point(206, 8)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(120, 47)
        btnOk.TabIndex = 0
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        ' 
        ' btnCancel
        ' 
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Dock = DockStyle.Left
        btnCancel.Location = New Point(8, 8)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(120, 47)
        btnCancel.TabIndex = 1
        btnCancel.Text = "Anulează"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' sepNav
        ' 
        sepNav.Dock = DockStyle.Top
        sepNav.Location = New Point(2, 44)
        sepNav.Name = "sepNav"
        sepNav.Size = New Size(334, 1)
        sepNav.TabIndex = 1
        ' 
        ' navFile
        ' 
        navFile.Dock = DockStyle.Top
        navFile.IconSize = 0
        navFile.ItemCornerRadius = 0
        navFile.ItemPadding = New Padding(0)
        KBotNavItem1.Key = "sortare"
        KBotNavItem1.Text = "Sortare"
        KBotNavItem2.Key = "filtrare"
        KBotNavItem2.Text = "Filtrare"
        KBotNavItem3.Key = "grupare"
        KBotNavItem3.Text = "Grupare"
        navFile.Items.Add(KBotNavItem1)
        navFile.Items.Add(KBotNavItem2)
        navFile.Items.Add(KBotNavItem3)
        navFile.Location = New Point(2, 2)
        navFile.Margin = New Padding(0)
        navFile.Name = "navFile"
        navFile.Orientation = KBotNavOrientation.Horizontal
        navFile.SelectedKey = "filtrare"
        navFile.Size = New Size(334, 42)
        navFile.TabIndex = 0
        ' 
        ' KBotFilterPopup
        ' 
        AutoScaleMode = AutoScaleMode.None
        CancelButton = btnCancel
        ClientSize = New Size(340, 560)
        ControlBox = False
        Controls.Add(pnlBody)
        FormBorderStyle = FormBorderStyle.None
        KeyPreview = True
        MaximizeBox = False
        MinimizeBox = False
        Name = "KBotFilterPopup"
        Padding = New Padding(1)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        pnlBody.ResumeLayout(False)
        pnlCommands.ResumeLayout(False)
        CType(navFile, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlBody As Panel
    Friend WithEvents viewHost As Panel
    Friend WithEvents pnlCommands As Panel
    Friend WithEvents btnOk As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents sepNav As Panel
    Friend WithEvents navFile As KBotNavList

End Class
