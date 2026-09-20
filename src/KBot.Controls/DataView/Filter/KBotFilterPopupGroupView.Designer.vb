Option Strict On
Imports System.Windows.Forms

''' <summary>
''' Designer half of <see cref="KBotFilterPopupGroupView"/>: one <c>KBotTableLayoutPanel</c> with
''' the «Grupeaza dupa…» box, the direction pair, a separator, the five band/collapse options,
''' the caption of the hierarchy and the elastic <c>ListBox</c> of levels (the only Percent
''' row). Only the CONTENT comes at runtime -- the grid's levels do not exist at design time.
'''
''' <para>The rows are authored on the Classic scheme; how much they grow under a padded scheme
''' (Modern) is the table's own business. <c>AutoScaleMode.Inherit</c> on purpose: the host popup
''' is <c>AutoScaleMode.None</c> and measures its own height from these rows, so a private
''' scaling pass here would put the tab on a different ruler from the window around it.</para>
''' </summary>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class KBotFilterPopupGroupView
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
        tlyGroup = New Global.KBot.Controls.KBotTableLayoutPanel()
        chkGroupBy = New CheckBox()
        pnlGroupDirection = New Panel()
        rbGroupDesc = New RadioButton()
        rbGroupAsc = New RadioButton()
        sepGroup = New Panel()
        chkGroupHeader = New CheckBox()
        chkGroupFooter = New CheckBox()
        chkGroupHeaderAggregates = New CheckBox()
        chkGroupCollapsible = New CheckBox()
        chkGroupStartCollapsed = New CheckBox()
        lblLevels = New Label()
        lstLevels = New ListBox()
        tlyGroup.SuspendLayout()
        pnlGroupDirection.SuspendLayout()
        SuspendLayout()
        '
        ' tlyGroup
        '
        tlyGroup.ColumnCount = 1
        tlyGroup.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlyGroup.Controls.Add(chkGroupBy, 0, 0)
        tlyGroup.Controls.Add(pnlGroupDirection, 0, 1)
        tlyGroup.Controls.Add(sepGroup, 0, 2)
        tlyGroup.Controls.Add(chkGroupHeader, 0, 3)
        tlyGroup.Controls.Add(chkGroupFooter, 0, 4)
        tlyGroup.Controls.Add(chkGroupHeaderAggregates, 0, 5)
        tlyGroup.Controls.Add(chkGroupCollapsible, 0, 6)
        tlyGroup.Controls.Add(chkGroupStartCollapsed, 0, 7)
        tlyGroup.Controls.Add(lblLevels, 0, 8)
        tlyGroup.Controls.Add(lstLevels, 0, 9)
        tlyGroup.Dock = DockStyle.Fill
        tlyGroup.Location = New Point(0, 0)
        tlyGroup.Margin = New Padding(0)
        tlyGroup.Name = "tlyGroup"
        tlyGroup.RowCount = 10
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 13.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 28.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Absolute, 26.0F))
        tlyGroup.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlyGroup.Size = New Size(334, 452)
        tlyGroup.TabIndex = 0
        '
        ' chkGroupBy
        '
        chkGroupBy.AutoSize = True
        chkGroupBy.Dock = DockStyle.Top
        chkGroupBy.Location = New Point(8, 8)
        chkGroupBy.Margin = New Padding(8, 8, 8, 0)
        chkGroupBy.Name = "chkGroupBy"
        chkGroupBy.Size = New Size(318, 19)
        chkGroupBy.TabIndex = 0
        chkGroupBy.Text = "Grupează după coloana aceasta"
        chkGroupBy.UseVisualStyleBackColor = True
        '
        ' pnlGroupDirection
        '
        ' AutoSize, not Dock=Fill: under a scheme with another font the radio buttons grow, and a
        ' panel that takes its height from the cell has no way to say so. This way the
        ' KBotTableLayoutPanel asks it how much it needs and grows the row by exactly that.
        pnlGroupDirection.AutoSize = True
        pnlGroupDirection.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlGroupDirection.Controls.Add(rbGroupDesc)
        pnlGroupDirection.Controls.Add(rbGroupAsc)
        pnlGroupDirection.Dock = DockStyle.Top
        pnlGroupDirection.Location = New Point(26, 34)
        pnlGroupDirection.Margin = New Padding(26, 0, 8, 0)
        pnlGroupDirection.Name = "pnlGroupDirection"
        pnlGroupDirection.Size = New Size(300, 25)
        pnlGroupDirection.TabIndex = 1
        '
        ' rbGroupAsc
        '
        rbGroupAsc.AutoSize = True
        rbGroupAsc.Location = New Point(0, 3)
        rbGroupAsc.Name = "rbGroupAsc"
        rbGroupAsc.Size = New Size(84, 19)
        rbGroupAsc.TabIndex = 0
        rbGroupAsc.TabStop = True
        rbGroupAsc.Text = "Crescător"
        rbGroupAsc.UseVisualStyleBackColor = True
        '
        ' rbGroupDesc
        '
        rbGroupDesc.AutoSize = True
        rbGroupDesc.Location = New Point(110, 3)
        rbGroupDesc.Name = "rbGroupDesc"
        rbGroupDesc.Size = New Size(102, 19)
        rbGroupDesc.TabIndex = 1
        rbGroupDesc.Text = "Descrescător"
        rbGroupDesc.UseVisualStyleBackColor = True
        '
        ' sepGroup
        '
        sepGroup.Dock = DockStyle.Top
        sepGroup.Location = New Point(6, 70)
        sepGroup.Margin = New Padding(6, 6, 6, 6)
        sepGroup.Name = "sepGroup"
        sepGroup.Size = New Size(322, 1)
        sepGroup.TabIndex = 2
        '
        ' chkGroupHeader
        '
        chkGroupHeader.AutoSize = True
        chkGroupHeader.Dock = DockStyle.Top
        chkGroupHeader.Location = New Point(26, 77)
        chkGroupHeader.Margin = New Padding(26, 0, 8, 0)
        chkGroupHeader.Name = "chkGroupHeader"
        chkGroupHeader.Size = New Size(300, 19)
        chkGroupHeader.TabIndex = 3
        chkGroupHeader.Text = "Bandă de antet (titlul grupului)"
        chkGroupHeader.UseVisualStyleBackColor = True
        '
        ' chkGroupFooter
        '
        chkGroupFooter.AutoSize = True
        chkGroupFooter.Dock = DockStyle.Top
        chkGroupFooter.Location = New Point(26, 105)
        chkGroupFooter.Margin = New Padding(26, 0, 8, 0)
        chkGroupFooter.Name = "chkGroupFooter"
        chkGroupFooter.Size = New Size(300, 19)
        chkGroupFooter.TabIndex = 4
        chkGroupFooter.Text = "Bandă de subsol (totalurile grupului)"
        chkGroupFooter.UseVisualStyleBackColor = True
        '
        ' chkGroupHeaderAggregates
        '
        chkGroupHeaderAggregates.AutoSize = True
        chkGroupHeaderAggregates.Dock = DockStyle.Top
        chkGroupHeaderAggregates.Location = New Point(26, 133)
        chkGroupHeaderAggregates.Margin = New Padding(26, 0, 8, 0)
        chkGroupHeaderAggregates.Name = "chkGroupHeaderAggregates"
        chkGroupHeaderAggregates.Size = New Size(300, 19)
        chkGroupHeaderAggregates.TabIndex = 5
        chkGroupHeaderAggregates.Text = "Totalurile și în antet (se văd și strâns)"
        chkGroupHeaderAggregates.UseVisualStyleBackColor = True
        '
        ' chkGroupCollapsible
        '
        chkGroupCollapsible.AutoSize = True
        chkGroupCollapsible.Dock = DockStyle.Top
        chkGroupCollapsible.Location = New Point(26, 161)
        chkGroupCollapsible.Margin = New Padding(26, 0, 8, 0)
        chkGroupCollapsible.Name = "chkGroupCollapsible"
        chkGroupCollapsible.Size = New Size(300, 19)
        chkGroupCollapsible.TabIndex = 6
        chkGroupCollapsible.Text = "Grupurile se pot strânge"
        chkGroupCollapsible.UseVisualStyleBackColor = True
        '
        ' chkGroupStartCollapsed
        '
        chkGroupStartCollapsed.AutoSize = True
        chkGroupStartCollapsed.Dock = DockStyle.Top
        chkGroupStartCollapsed.Location = New Point(26, 189)
        chkGroupStartCollapsed.Margin = New Padding(26, 0, 8, 0)
        chkGroupStartCollapsed.Name = "chkGroupStartCollapsed"
        chkGroupStartCollapsed.Size = New Size(300, 19)
        chkGroupStartCollapsed.TabIndex = 7
        chkGroupStartCollapsed.Text = "Pornesc strânse"
        chkGroupStartCollapsed.UseVisualStyleBackColor = True
        '
        ' lblLevels
        '
        lblLevels.AutoSize = True
        lblLevels.Dock = DockStyle.Top
        lblLevels.Location = New Point(8, 217)
        lblLevels.Margin = New Padding(8, 6, 8, 0)
        lblLevels.Name = "lblLevels"
        lblLevels.Size = New Size(318, 15)
        lblLevels.TabIndex = 8
        lblLevels.Text = "Niveluri de grupare, de la cel dinafară:"
        lblLevels.TextAlign = ContentAlignment.MiddleLeft
        '
        ' lstLevels
        '
        lstLevels.BorderStyle = BorderStyle.None
        lstLevels.Dock = DockStyle.Fill
        lstLevels.IntegralHeight = False
        lstLevels.Location = New Point(8, 243)
        lstLevels.Margin = New Padding(8, 0, 8, 6)
        lstLevels.Name = "lstLevels"
        lstLevels.SelectionMode = SelectionMode.None
        lstLevels.Size = New Size(318, 203)
        lstLevels.TabIndex = 9
        '
        ' KBotFilterPopupGroupView
        '
        AutoScaleMode = AutoScaleMode.Inherit
        Controls.Add(tlyGroup)
        Margin = New Padding(0)
        Name = "KBotFilterPopupGroupView"
        Size = New Size(334, 452)
        tlyGroup.ResumeLayout(False)
        tlyGroup.PerformLayout()
        pnlGroupDirection.ResumeLayout(False)
        pnlGroupDirection.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyGroup As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents chkGroupBy As CheckBox
    Friend WithEvents pnlGroupDirection As Panel
    Friend WithEvents rbGroupAsc As RadioButton
    Friend WithEvents rbGroupDesc As RadioButton
    Friend WithEvents sepGroup As Panel
    Friend WithEvents chkGroupHeader As CheckBox
    Friend WithEvents chkGroupFooter As CheckBox
    Friend WithEvents chkGroupHeaderAggregates As CheckBox
    Friend WithEvents chkGroupCollapsible As CheckBox
    Friend WithEvents chkGroupStartCollapsed As CheckBox
    Friend WithEvents lblLevels As Label
    Friend WithEvents lstLevels As ListBox

End Class
