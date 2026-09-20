Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Designer half of <see cref="KBotFilterPopupSortView"/>: three menu rows and a separator in one
''' <c>KBotTableLayoutPanel</c>. The rows are authored on the Classic scheme; how much they grow
''' under a padded scheme (Modern) is the table's own business (it refits its fixed rows at
''' theme and scale).
'''
''' <para><c>AutoScaleMode.Inherit</c> on purpose: the host popup is <c>AutoScaleMode.None</c> and
''' measures its own height from these rows, so a private scaling pass here would put the tab
''' on a different ruler from the window around it.</para>
''' </summary>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotFilterPopupSortView
    Inherits Global.KBot.Theming.KBotThemedUserControl

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        tlySort = New KBotTableLayoutPanel()
        btnSortAsc = New Button()
        btnSortDesc = New Button()
        sepSort = New Panel()
        btnSortClear = New Button()
        tlySort.SuspendLayout()
        SuspendLayout()
        ' 
        ' tlySort
        ' 
        tlySort.AutoFitToTheme = False
        tlySort.ColumnCount = 1
        tlySort.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlySort.Controls.Add(btnSortAsc, 0, 0)
        tlySort.Controls.Add(btnSortDesc, 0, 1)
        tlySort.Controls.Add(sepSort, 0, 2)
        tlySort.Controls.Add(btnSortClear, 0, 3)
        tlySort.Dock = DockStyle.Fill
        tlySort.Location = New Point(0, 0)
        tlySort.Margin = New Padding(0)
        tlySort.Name = "tlySort"
        tlySort.RowCount = 5
        tlySort.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlySort.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlySort.RowStyles.Add(New RowStyle(SizeType.Absolute, 9F))
        tlySort.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlySort.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlySort.ScaleAbsoluteStyles = False
        tlySort.Size = New Size(334, 133)
        tlySort.TabIndex = 0
        ' 
        ' btnSortAsc
        ' 
        btnSortAsc.Cursor = Cursors.Hand
        btnSortAsc.Dock = DockStyle.Top
        btnSortAsc.FlatAppearance.BorderSize = 0
        btnSortAsc.FlatStyle = FlatStyle.Flat
        btnSortAsc.Image = My.Resources.Resources.sort_asc
        btnSortAsc.ImageAlign = ContentAlignment.MiddleLeft
        btnSortAsc.Location = New Point(0, 0)
        btnSortAsc.Margin = New Padding(0)
        btnSortAsc.Name = "btnSortAsc"
        btnSortAsc.Size = New Size(334, 40)
        btnSortAsc.TabIndex = 0
        btnSortAsc.Text = " Sortează &Crescător"
        btnSortAsc.TextAlign = ContentAlignment.MiddleLeft
        btnSortAsc.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSortAsc.UseVisualStyleBackColor = True
        ' 
        ' btnSortDesc
        ' 
        btnSortDesc.Cursor = Cursors.Hand
        btnSortDesc.Dock = DockStyle.Top
        btnSortDesc.FlatAppearance.BorderSize = 0
        btnSortDesc.FlatStyle = FlatStyle.Flat
        btnSortDesc.Image = My.Resources.Resources.sort_desc
        btnSortDesc.ImageAlign = ContentAlignment.MiddleLeft
        btnSortDesc.Location = New Point(0, 40)
        btnSortDesc.Margin = New Padding(0)
        btnSortDesc.Name = "btnSortDesc"
        btnSortDesc.Size = New Size(334, 40)
        btnSortDesc.TabIndex = 1
        btnSortDesc.Text = " Sortează &Descrescător"
        btnSortDesc.TextAlign = ContentAlignment.MiddleLeft
        btnSortDesc.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSortDesc.UseVisualStyleBackColor = True
        ' 
        ' sepSort
        ' 
        sepSort.Dock = DockStyle.Top
        sepSort.Location = New Point(6, 84)
        sepSort.Margin = New Padding(6, 4, 6, 4)
        sepSort.Name = "sepSort"
        sepSort.Size = New Size(322, 1)
        sepSort.TabIndex = 2
        ' 
        ' btnSortClear
        ' 
        btnSortClear.Cursor = Cursors.Hand
        btnSortClear.Dock = DockStyle.Top
        btnSortClear.FlatAppearance.BorderSize = 0
        btnSortClear.FlatStyle = FlatStyle.Flat
        btnSortClear.Image = My.Resources.Resources.sort_clear
        btnSortClear.ImageAlign = ContentAlignment.MiddleLeft
        btnSortClear.Location = New Point(0, 89)
        btnSortClear.Margin = New Padding(0)
        btnSortClear.Name = "btnSortClear"
        btnSortClear.Size = New Size(334, 40)
        btnSortClear.TabIndex = 3
        btnSortClear.Text = " &Resetează sortarea"
        btnSortClear.TextAlign = ContentAlignment.MiddleLeft
        btnSortClear.TextImageRelation = TextImageRelation.ImageBeforeText
        btnSortClear.UseVisualStyleBackColor = True
        ' 
        ' KBotFilterPopupSortView
        ' 
        AutoScaleMode = AutoScaleMode.Inherit
        Controls.Add(tlySort)
        Margin = New Padding(0)
        Name = "KBotFilterPopupSortView"
        Size = New Size(334, 133)
        tlySort.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlySort As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents btnSortAsc As Button
    Friend WithEvents btnSortDesc As Button
    Friend WithEvents sepSort As Panel
    Friend WithEvents btnSortClear As Button

End Class
