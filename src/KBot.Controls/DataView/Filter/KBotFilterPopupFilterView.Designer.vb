Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Designer half of <see cref="KBotFilterPopupFilterView"/>: one <c>KBotTableLayoutPanel</c> with
''' the search row (icon + <c>KBotTextField</c>), «(Selecteaza tot)», the elastic
''' <c>CheckedListBox</c> (the only Percent row), a separator, the conditions row and «Sterge
''' filtrul». Only the CONTENT of the list comes at runtime -- the distinct values of a column
''' do not exist at design time -- the control, its font and its row height are the designer's.
'''
''' <para>The rows are authored on the Classic scheme; how much they grow under a padded scheme
''' (Modern) is the table's own business. <c>AutoScaleMode.Inherit</c> on purpose: the host popup
''' is <c>AutoScaleMode.None</c> and measures its own height from these rows, so a private
''' scaling pass here would put the tab on a different ruler from the window around it.</para>
''' </summary>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotFilterPopupFilterView
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
        tlyFilter = New KBotTableLayoutPanel()
        picSearch = New PictureBox()
        txtSearch = New KBotTextField()
        chkSelectAll = New CheckBox()
        lstValues = New CheckedListBox()
        sepFilter = New Panel()
        btnClearFilter = New Button()
        btnConditions = New Button()
        tlyFilter.SuspendLayout()
        CType(picSearch, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' tlyFilter
        ' 
        tlyFilter.AutoFitToTheme = False
        tlyFilter.ColumnCount = 3
        tlyFilter.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 36F))
        tlyFilter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 42.35669F))
        tlyFilter.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 57.64331F))
        tlyFilter.Controls.Add(picSearch, 0, 1)
        tlyFilter.Controls.Add(txtSearch, 1, 1)
        tlyFilter.Controls.Add(chkSelectAll, 0, 2)
        tlyFilter.Controls.Add(lstValues, 0, 3)
        tlyFilter.Controls.Add(sepFilter, 0, 4)
        tlyFilter.Controls.Add(btnClearFilter, 2, 0)
        tlyFilter.Controls.Add(btnConditions, 0, 0)
        tlyFilter.Dock = DockStyle.Fill
        tlyFilter.Location = New Point(0, 0)
        tlyFilter.Margin = New Padding(0)
        tlyFilter.Name = "tlyFilter"
        tlyFilter.RowCount = 5
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Absolute, 50F))
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Absolute, 34F))
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyFilter.RowStyles.Add(New RowStyle(SizeType.Absolute, 9F))
        tlyFilter.ScaleAbsoluteStyles = False
        tlyFilter.Size = New Size(350, 452)
        tlyFilter.TabIndex = 0
        ' 
        ' picSearch
        ' 
        picSearch.BackColor = Color.Transparent
        picSearch.Dock = DockStyle.Fill
        picSearch.Image = My.Resources.Resources.filter_search
        picSearch.Location = New Point(0, 40)
        picSearch.Margin = New Padding(0)
        picSearch.Name = "picSearch"
        picSearch.Size = New Size(36, 50)
        picSearch.SizeMode = PictureBoxSizeMode.CenterImage
        picSearch.TabIndex = 0
        picSearch.TabStop = False
        ' 
        ' txtSearch
        ' 
        txtSearch.BackColor = Color.Transparent
        tlyFilter.SetColumnSpan(txtSearch, 2)
        txtSearch.Dock = DockStyle.Fill
        txtSearch.Location = New Point(42, 46)
        txtSearch.Margin = New Padding(6)
        txtSearch.Name = "txtSearch"
        txtSearch.PlaceholderText = "Caută…"
        txtSearch.Size = New Size(302, 38)
        txtSearch.TabIndex = 0
        txtSearch.TextPadding = New Padding(8, 4, 8, 4)
        ' 
        ' chkSelectAll
        ' 
        chkSelectAll.AutoSize = True
        tlyFilter.SetColumnSpan(chkSelectAll, 3)
        chkSelectAll.Dock = DockStyle.Top
        chkSelectAll.Font = New Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        chkSelectAll.Location = New Point(8, 96)
        chkSelectAll.Margin = New Padding(8, 6, 0, 0)
        chkSelectAll.Name = "chkSelectAll"
        chkSelectAll.Size = New Size(342, 26)
        chkSelectAll.TabIndex = 1
        chkSelectAll.Text = "(Selectează tot)"
        chkSelectAll.ThreeState = True
        chkSelectAll.UseVisualStyleBackColor = True
        ' 
        ' lstValues
        ' 
        lstValues.BorderStyle = BorderStyle.None
        lstValues.CheckOnClick = True
        tlyFilter.SetColumnSpan(lstValues, 3)
        lstValues.Dock = DockStyle.Fill
        lstValues.Font = New Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lstValues.IntegralHeight = False
        lstValues.Location = New Point(6, 124)
        lstValues.Margin = New Padding(6, 0, 0, 0)
        lstValues.Name = "lstValues"
        lstValues.Size = New Size(344, 319)
        lstValues.TabIndex = 2
        ' 
        ' sepFilter
        ' 
        tlyFilter.SetColumnSpan(sepFilter, 3)
        sepFilter.Dock = DockStyle.Top
        sepFilter.Location = New Point(6, 447)
        sepFilter.Margin = New Padding(6, 4, 6, 4)
        sepFilter.Name = "sepFilter"
        sepFilter.Size = New Size(338, 1)
        sepFilter.TabIndex = 3
        ' 
        ' btnClearFilter
        ' 
        btnClearFilter.Cursor = Cursors.Hand
        btnClearFilter.Dock = DockStyle.Fill
        btnClearFilter.FlatAppearance.BorderSize = 0
        btnClearFilter.FlatStyle = FlatStyle.Flat
        btnClearFilter.Image = My.Resources.Resources.filter_delete
        btnClearFilter.ImageAlign = ContentAlignment.MiddleLeft
        btnClearFilter.Location = New Point(169, 0)
        btnClearFilter.Margin = New Padding(0)
        btnClearFilter.Name = "btnClearFilter"
        btnClearFilter.RightToLeft = RightToLeft.Yes
        btnClearFilter.Size = New Size(181, 40)
        btnClearFilter.TabIndex = 5
        btnClearFilter.Text = " Șterge &Filtrul"
        btnClearFilter.TextAlign = ContentAlignment.MiddleLeft
        btnClearFilter.TextImageRelation = TextImageRelation.ImageBeforeText
        btnClearFilter.UseVisualStyleBackColor = True
        ' 
        ' btnConditions
        ' 
        tlyFilter.SetColumnSpan(btnConditions, 2)
        btnConditions.Cursor = Cursors.Hand
        btnConditions.Dock = DockStyle.Fill
        btnConditions.FlatAppearance.BorderSize = 0
        btnConditions.FlatStyle = FlatStyle.Flat
        btnConditions.Image = My.Resources.Resources.filter_edit
        btnConditions.ImageAlign = ContentAlignment.MiddleLeft
        btnConditions.Location = New Point(0, 0)
        btnConditions.Margin = New Padding(0)
        btnConditions.Name = "btnConditions"
        btnConditions.Size = New Size(169, 40)
        btnConditions.TabIndex = 4
        btnConditions.Text = " Operatori filtru"
        btnConditions.TextAlign = ContentAlignment.MiddleLeft
        btnConditions.TextImageRelation = TextImageRelation.ImageBeforeText
        btnConditions.UseVisualStyleBackColor = True
        ' 
        ' KBotFilterPopupFilterView
        ' 
        AutoScaleMode = AutoScaleMode.Inherit
        Controls.Add(tlyFilter)
        Margin = New Padding(0)
        Name = "KBotFilterPopupFilterView"
        Size = New Size(350, 452)
        tlyFilter.ResumeLayout(False)
        tlyFilter.PerformLayout()
        CType(picSearch, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyFilter As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents picSearch As PictureBox
    Friend WithEvents txtSearch As KBotTextField
    Friend WithEvents chkSelectAll As CheckBox
    Friend WithEvents lstValues As CheckedListBox
    Friend WithEvents sepFilter As Panel
    Friend WithEvents btnConditions As Button
    Friend WithEvents btnClearFilter As Button

End Class
