<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class XmlViz
    Inherits Global.KBot.Theming.KBotThemedUserControl

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
        components = New System.ComponentModel.Container()
        tlyMain = New KBotTableLayoutPanel()
        pnlHarta = New System.Windows.Forms.Panel()
        tree = New AdvancedTreeControl()
        pnlText = New System.Windows.Forms.Panel()
        rtb = New System.Windows.Forms.RichTextBox()
        tlyMain.SuspendLayout()
        pnlHarta.SuspendLayout()
        pnlText.SuspendLayout()
        SuspendLayout()
        '
        ' tlyMain: the map on the left, the text on the right
        '
        tlyMain.ColumnCount = 2
        tlyMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.0!))
        tlyMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68.0!))
        tlyMain.Controls.Add(pnlHarta, 0, 0)
        tlyMain.Controls.Add(pnlText, 1, 0)
        tlyMain.Dock = System.Windows.Forms.DockStyle.Fill
        tlyMain.Location = New System.Drawing.Point(0, 0)
        tlyMain.Margin = New System.Windows.Forms.Padding(0)
        tlyMain.Name = "tlyMain"
        tlyMain.RowCount = 1
        tlyMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        tlyMain.Size = New System.Drawing.Size(700, 400)
        tlyMain.TabIndex = 0
        '
        ' pnlHarta
        '
        pnlHarta.Controls.Add(tree)
        pnlHarta.Dock = System.Windows.Forms.DockStyle.Fill
        pnlHarta.Location = New System.Drawing.Point(0, 0)
        pnlHarta.Margin = New System.Windows.Forms.Padding(0)
        pnlHarta.Name = "pnlHarta"
        pnlHarta.Padding = New System.Windows.Forms.Padding(0, 0, 3, 0)
        pnlHarta.Size = New System.Drawing.Size(224, 400)
        pnlHarta.TabIndex = 0
        '
        ' tree
        '
        tree.BorderColor = Drawing.SystemColors.ActiveBorder
        tree.Dock = System.Windows.Forms.DockStyle.Fill
        tree.DynamicColumns = False
        tree.ExpanderSize = 10
        tree.Font = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderBackColor = Drawing.SystemColors.Control
        tree.HeaderBackStyle = AdvancedTreeControl.En_HeaderBackStyle.GradientHorizontal
        tree.HeaderCaption = " STRUCTURA XML"
        tree.HeaderFont = New System.Drawing.Font("Calibri", 9.0!, Drawing.FontStyle.Bold, Drawing.GraphicsUnit.Point, CByte(0))
        tree.HeaderForeColor = Drawing.Color.Black
        tree.HeaderHeight = 30
        tree.HeaderSeparatorColor = Drawing.Color.Gainsboro
        tree.HeaderSeparatorWidth = 2
        tree.HeaderVisible = True
        tree.Indent = 8
        tree.ItemHeight = 22
        tree.LeftIconSize = New System.Drawing.Size(14, 14)
        tree.Location = New System.Drawing.Point(0, 0)
        tree.Margin = New System.Windows.Forms.Padding(0)
        tree.MinimumCollapsedWidth = 120
        tree.Name = "tree"
        tree.PaddingExpanderGap = 8
        tree.PaddingIconGap = 4
        tree.PaddingTreeEnd = 8
        tree.PaddingTreeStart = 8
        tree.ReserveRightIconSpace = False
        tree.RightClickSelects = False
        tree.RightIconSize = New System.Drawing.Size(14, 14)
        tree.RightTextWidth = 110
        tree.ShowRightIconOnHover = False
        tree.Size = New System.Drawing.Size(221, 400)
        tree.TabIndex = 0
        '
        ' pnlText
        '
        pnlText.Controls.Add(rtb)
        pnlText.Dock = System.Windows.Forms.DockStyle.Fill
        pnlText.Location = New System.Drawing.Point(224, 0)
        pnlText.Margin = New System.Windows.Forms.Padding(0)
        pnlText.Name = "pnlText"
        pnlText.Size = New System.Drawing.Size(476, 400)
        pnlText.TabIndex = 1
        pnlText.Tag = "Card"
        '
        ' rtb
        '
        rtb.BorderStyle = System.Windows.Forms.BorderStyle.None
        rtb.DetectUrls = False
        rtb.Dock = System.Windows.Forms.DockStyle.Fill
        rtb.Font = New System.Drawing.Font("Consolas", 9.75!, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        rtb.HideSelection = False
        rtb.Location = New System.Drawing.Point(0, 0)
        rtb.Margin = New System.Windows.Forms.Padding(0)
        rtb.Name = "rtb"
        rtb.ReadOnly = True
        rtb.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both
        rtb.Size = New System.Drawing.Size(476, 400)
        rtb.TabIndex = 0
        rtb.Text = ""
        rtb.WordWrap = False
        '
        ' XmlViz
        '
        AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Controls.Add(tlyMain)
        Name = "XmlViz"
        Size = New System.Drawing.Size(700, 400)
        tlyMain.ResumeLayout(False)
        pnlHarta.ResumeLayout(False)
        pnlText.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyMain As KBotTableLayoutPanel
    Friend WithEvents pnlHarta As System.Windows.Forms.Panel
    Friend WithEvents tree As AdvancedTreeControl
    Friend WithEvents pnlText As System.Windows.Forms.Panel
    Friend WithEvents rtb As System.Windows.Forms.RichTextBox
End Class
