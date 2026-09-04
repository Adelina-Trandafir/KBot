Option Strict On

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ThemeEditorForm
    Inherits KBotThemedForm

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
        pnlTop = New Panel()
        lblScope = New Label()
        cboScope = New KBotComboBox()
        btnRefresh = New Button()
        splitMain = New SplitContainer()
        treeControls = New TreeView()
        grid = New PropertyGrid()
        pnlBottom = New Panel()
        lblStatus = New Label()
        btnResetControl = New Button()
        btnResetAll = New Button()
        btnLoad = New Button()
        btnSave = New Button()
        pnlTop.SuspendLayout()
        CType(splitMain, ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        '
        ' lblScope
        '
        lblScope.AutoSize = True
        lblScope.Location = New Point(12, 17)
        lblScope.Name = "lblScope"
        lblScope.Size = New Size(60, 15)
        lblScope.TabIndex = 0
        lblScope.Text = "Suprafață:"
        '
        ' cboScope
        '
        cboScope.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        cboScope.Location = New Point(90, 12)
        cboScope.Name = "cboScope"
        cboScope.Size = New Size(360, 26)
        cboScope.TabIndex = 1
        '
        ' btnRefresh
        '
        btnRefresh.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRefresh.Location = New Point(460, 11)
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(110, 28)
        btnRefresh.TabIndex = 2
        btnRefresh.Text = "Reîmprospătează"
        btnRefresh.UseVisualStyleBackColor = True
        '
        ' pnlTop
        '
        pnlTop.Controls.Add(btnRefresh)
        pnlTop.Controls.Add(cboScope)
        pnlTop.Controls.Add(lblScope)
        pnlTop.Dock = DockStyle.Top
        pnlTop.Location = New Point(0, 0)
        pnlTop.Name = "pnlTop"
        pnlTop.Size = New Size(584, 50)
        pnlTop.TabIndex = 0
        '
        ' treeControls
        '
        treeControls.Dock = DockStyle.Fill
        treeControls.HideSelection = False
        treeControls.Location = New Point(0, 0)
        treeControls.Name = "treeControls"
        treeControls.Size = New Size(240, 340)
        treeControls.TabIndex = 0
        '
        ' grid
        '
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 0)
        grid.Name = "grid"
        grid.PropertySort = PropertySort.Categorized
        grid.Size = New Size(340, 340)
        grid.TabIndex = 0
        grid.ToolbarVisible = False
        '
        ' splitMain
        '
        splitMain.Dock = DockStyle.Fill
        splitMain.Location = New Point(0, 50)
        splitMain.Name = "splitMain"
        splitMain.Panel1.Controls.Add(treeControls)
        splitMain.Panel1MinSize = 160
        splitMain.Panel2.Controls.Add(grid)
        splitMain.Panel2MinSize = 240
        splitMain.Size = New Size(584, 340)
        splitMain.SplitterDistance = 240
        splitMain.TabIndex = 1
        '
        ' lblStatus
        '
        lblStatus.AutoEllipsis = True
        lblStatus.Location = New Point(12, 18)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(200, 20)
        lblStatus.TabIndex = 0
        lblStatus.Text = ""
        '
        ' btnResetControl
        '
        btnResetControl.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnResetControl.Location = New Point(218, 13)
        btnResetControl.Name = "btnResetControl"
        btnResetControl.Size = New Size(110, 28)
        btnResetControl.TabIndex = 1
        btnResetControl.Text = "Reset control"
        btnResetControl.UseVisualStyleBackColor = True
        '
        ' btnResetAll
        '
        btnResetAll.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnResetAll.Location = New Point(334, 13)
        btnResetAll.Name = "btnResetAll"
        btnResetAll.Size = New Size(80, 28)
        btnResetAll.TabIndex = 2
        btnResetAll.Text = "Reset tot"
        btnResetAll.UseVisualStyleBackColor = True
        '
        ' btnLoad
        '
        btnLoad.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnLoad.Location = New Point(420, 13)
        btnLoad.Name = "btnLoad"
        btnLoad.Size = New Size(74, 28)
        btnLoad.TabIndex = 3
        btnLoad.Text = "Încarcă…"
        btnLoad.UseVisualStyleBackColor = True
        '
        ' btnSave
        '
        btnSave.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSave.Location = New Point(500, 13)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(74, 28)
        btnSave.TabIndex = 4
        btnSave.Text = "Salvează…"
        btnSave.UseVisualStyleBackColor = True
        '
        ' pnlBottom
        '
        pnlBottom.Controls.Add(btnSave)
        pnlBottom.Controls.Add(btnLoad)
        pnlBottom.Controls.Add(btnResetAll)
        pnlBottom.Controls.Add(btnResetControl)
        pnlBottom.Controls.Add(lblStatus)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New Point(0, 390)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Size = New Size(584, 54)
        pnlBottom.TabIndex = 2
        '
        ' ThemeEditorForm
        '
        AutoScaleDimensions = New SizeF(6F, 14F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(584, 444)
        ' Ordine INVERSĂ de andocare (regula casei): Fill primul, apoi Bottom/Top.
        Controls.Add(splitMain)
        Controls.Add(pnlBottom)
        Controls.Add(pnlTop)
        FormBorderStyle = FormBorderStyle.SizableToolWindow
        MinimizeBox = False
        MinimumSize = New Size(520, 380)
        Name = "ThemeEditorForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Editor de stiluri"
        pnlTop.ResumeLayout(False)
        pnlTop.PerformLayout()
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        pnlBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlTop As Panel
    Friend WithEvents lblScope As Label
    Friend WithEvents cboScope As KBotComboBox
    Friend WithEvents btnRefresh As Button
    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents treeControls As TreeView
    Friend WithEvents grid As PropertyGrid
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnResetControl As Button
    Friend WithEvents btnResetAll As Button
    Friend WithEvents btnLoad As Button
    Friend WithEvents btnSave As Button
End Class
