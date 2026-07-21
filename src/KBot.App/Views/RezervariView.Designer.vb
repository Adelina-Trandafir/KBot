<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RezervariView
    Inherits System.Windows.Forms.UserControl

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
        split = New SplitContainer()
        tree = New Controls.AdvancedTreeControl()
        grid = New Controls.KBotDataView()
        lblEmpty = New Label()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        SuspendLayout()
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Margin = New Padding(4, 5, 4, 5)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(tree)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(grid)
        split.Size = New Size(986, 567)
        split.SplitterDistance = 336
        split.SplitterWidth = 9
        split.TabIndex = 0
        ' 
        ' tree
        ' 
        tree.AutoScrollMinSize = New Size(0, 0)
        tree.BackColor = Color.White
        tree.BorderColor = Color.Transparent
        tree.Dock = DockStyle.Fill
        tree.Font = New Font("Segoe UI", 9F)
        tree.HeaderBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.HeaderForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.HeaderIconSize = New Size(16, 16)
        tree.HoverBackColor = Color.FromArgb(CByte(230), CByte(240), CByte(255))
        tree.ItemHeight = 24
        tree.LeftIconSize = New Size(16, 16)
        tree.LineColor = Color.FromArgb(CByte(160), CByte(160), CByte(160))
        tree.Location = New Point(0, 0)
        tree.Margin = New Padding(4, 5, 4, 5)
        tree.Name = "tree"
        tree.ReserveRightIconSpace = True
        tree.RightIconSize = New Size(14, 14)
        tree.RightTextWidth = 110
        tree.SearchBackColor = Color.FromArgb(CByte(222), CByte(222), CByte(222))
        tree.SearchBarFontSize = 10F
        tree.SearchBarLabelForeColor = Color.Empty
        tree.SearchBoxBackColor = Color.Empty
        tree.SelectedBackColor = Color.FromArgb(CByte(200), CByte(220), CByte(255))
        tree.SelectedBorderColor = Color.FromArgb(CByte(150), CByte(180), CByte(255))
        tree.Size = New Size(336, 567)
        tree.TabIndex = 0
        tree.TooltipBackColor = Color.FromArgb(CByte(255), CByte(255), CByte(232))
        tree.TooltipForeColor = Color.FromArgb(CByte(50), CByte(50), CByte(60))
        tree.TreeFont = New Font("Consolas", 9F)
        ' 
        ' grid
        ' 
        grid.AlternatingRows = True
        grid.AutoSizeColumnsMode = KBot.Controls.KBotAutoSizeMode.ToContent
        grid.AutoSizeSampleRows = 200
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.None
        grid.CurrentColumnKey = Nothing
        grid.CurrentRowIndex = -1
        grid.Dock = DockStyle.Fill
        grid.FrozenColumnCount = 0
        grid.HeaderHeight = 30
        grid.Location = New Point(0, 0)
        grid.Margin = New Padding(4, 5, 4, 5)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.RowHeight = 28
        grid.ScrollByColumn = True
        grid.ShowHeader = True
        grid.Size = New Size(641, 567)
        grid.TabIndex = 0
        ' 
        ' lblEmpty
        ' 
        lblEmpty.Dock = DockStyle.Fill
        lblEmpty.Font = New Font("Segoe UI", 10F)
        lblEmpty.Location = New Point(0, 0)
        lblEmpty.Margin = New Padding(4, 0, 4, 0)
        lblEmpty.Name = "lblEmpty"
        lblEmpty.Size = New Size(986, 567)
        lblEmpty.TabIndex = 1
        lblEmpty.Text = "Selectați un angajament din arbore."
        lblEmpty.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' RezervariView
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(split)
        Controls.Add(lblEmpty)
        Margin = New Padding(4, 5, 4, 5)
        Name = "RezervariView"
        Size = New Size(986, 567)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As SplitContainer
    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents lblEmpty As Label
End Class
