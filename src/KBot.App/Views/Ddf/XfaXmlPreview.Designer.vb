<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class XfaXmlPreview
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
        pnlContent = New Panel()
        grid = New Controls.KBotDataView()
        pnlHeader = New Panel()
        lblNota = New Label()
        tblHeader = New TableLayoutPanel()
        pnlMissing = New Panel()
        tblMissing = New TableLayoutPanel()
        lblMissing = New Label()
        btnGenereaza = New Button()
        lblMessage = New Label()
        pnlContent.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        pnlHeader.SuspendLayout()
        pnlMissing.SuspendLayout()
        tblMissing.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlContent
        ' 
        pnlContent.Controls.Add(grid)
        pnlContent.Controls.Add(pnlHeader)
        pnlContent.Dock = DockStyle.Fill
        pnlContent.Location = New Point(0, 0)
        pnlContent.Name = "pnlContent"
        pnlContent.Size = New Size(840, 460)
        pnlContent.TabIndex = 0
        pnlContent.Visible = False
        ' 
        ' grid
        ' 
        grid.BackColor = SystemColors.Window
        grid.ColumnFillMode = KBot.Controls.KBotFillMode.LastColumn
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 120)
        grid.Name = "grid"
        grid.ReadOnlyGrid = True
        grid.Size = New Size(840, 340)
        grid.TabIndex = 1
        ' 
        ' pnlHeader
        ' 
        pnlHeader.Controls.Add(lblNota)
        pnlHeader.Controls.Add(tblHeader)
        pnlHeader.Dock = DockStyle.Top
        pnlHeader.Location = New Point(0, 0)
        pnlHeader.Name = "pnlHeader"
        pnlHeader.Padding = New Padding(8, 6, 8, 6)
        pnlHeader.Size = New Size(840, 120)
        pnlHeader.TabIndex = 0
        ' 
        ' lblNota
        ' 
        lblNota.Dock = DockStyle.Fill
        lblNota.Location = New Point(8, 6)
        lblNota.Name = "lblNota"
        lblNota.Size = New Size(824, 108)
        lblNota.TabIndex = 1
        ' 
        ' tblHeader
        ' 
        tblHeader.AutoSize = True
        tblHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tblHeader.ColumnCount = 2
        tblHeader.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170F))
        tblHeader.ColumnStyles.Add(New ColumnStyle())
        tblHeader.Dock = DockStyle.Top
        tblHeader.Location = New Point(8, 6)
        tblHeader.Name = "tblHeader"
        tblHeader.Size = New Size(824, 0)
        tblHeader.TabIndex = 0
        ' 
        ' pnlMissing
        ' 
        pnlMissing.Controls.Add(tblMissing)
        pnlMissing.Dock = DockStyle.Fill
        pnlMissing.Location = New Point(0, 0)
        pnlMissing.Name = "pnlMissing"
        pnlMissing.Size = New Size(840, 460)
        pnlMissing.TabIndex = 1
        pnlMissing.Visible = False
        ' 
        ' tblMissing
        ' 
        tblMissing.ColumnCount = 1
        tblMissing.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tblMissing.Controls.Add(lblMissing, 0, 0)
        tblMissing.Controls.Add(btnGenereaza, 0, 1)
        tblMissing.Dock = DockStyle.Fill
        tblMissing.Location = New Point(0, 0)
        tblMissing.Name = "tblMissing"
        tblMissing.RowCount = 2
        tblMissing.RowStyles.Add(New RowStyle(SizeType.Percent, 55F))
        tblMissing.RowStyles.Add(New RowStyle(SizeType.Percent, 45F))
        tblMissing.Size = New Size(840, 460)
        tblMissing.TabIndex = 0
        ' 
        ' lblMissing
        ' 
        lblMissing.Dock = DockStyle.Fill
        lblMissing.Font = New Font("Segoe UI", 10F)
        lblMissing.Location = New Point(3, 0)
        lblMissing.Name = "lblMissing"
        lblMissing.Size = New Size(834, 253)
        lblMissing.TabIndex = 0
        lblMissing.Text = "Documentul nu a fost încă generat."
        lblMissing.TextAlign = ContentAlignment.BottomCenter
        ' 
        ' btnGenereaza
        ' 
        btnGenereaza.Anchor = AnchorStyles.Top
        btnGenereaza.AutoSize = True
        btnGenereaza.FlatStyle = FlatStyle.Flat
        btnGenereaza.Location = New Point(303, 256)
        btnGenereaza.Name = "btnGenereaza"
        btnGenereaza.Padding = New Padding(14, 6, 14, 6)
        btnGenereaza.Size = New Size(233, 49)
        btnGenereaza.TabIndex = 1
        btnGenereaza.Text = "Generează documentul"
        btnGenereaza.UseVisualStyleBackColor = True
        ' 
        ' lblMessage
        ' 
        lblMessage.Dock = DockStyle.Fill
        lblMessage.Font = New Font("Segoe UI", 10F)
        lblMessage.Location = New Point(0, 0)
        lblMessage.Name = "lblMessage"
        lblMessage.Size = New Size(840, 460)
        lblMessage.TabIndex = 2
        lblMessage.Text = "Selectați o revizie din arbore."
        lblMessage.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' XfaXmlPreview
        ' 
        AutoScaleDimensions = New SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(pnlContent)
        Controls.Add(pnlMissing)
        Controls.Add(lblMessage)
        Name = "XfaXmlPreview"
        Size = New Size(840, 460)
        pnlContent.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        pnlHeader.ResumeLayout(False)
        pnlHeader.PerformLayout()
        pnlMissing.ResumeLayout(False)
        tblMissing.ResumeLayout(False)
        tblMissing.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents pnlContent As Panel
    Friend WithEvents grid As KBot.Controls.KBotDataView
    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblNota As Label
    Friend WithEvents tblHeader As TableLayoutPanel
    Friend WithEvents pnlMissing As Panel
    Friend WithEvents tblMissing As TableLayoutPanel
    Friend WithEvents lblMissing As Label
    Friend WithEvents btnGenereaza As Button
    Friend WithEvents lblMessage As Label
End Class
