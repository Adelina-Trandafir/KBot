Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WorkflowEditorForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        SplitContainer1 = New SplitContainer()
        _acListBox = New ListBox()
        TableLayoutPanel1 = New TableLayoutPanel()
        lblPath = New Label()
        rtbEditor = New RichTextBox()
        pnlBottom = New Panel()
        lblStatus = New Label()
        btnCancel = New Button()
        btnSaveAs = New Button()
        DocMapTree = New NoHScrollTreeView()
        ErrTooltip = New ToolTip(components)
        chkWordWrap = New CheckBox()
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        TableLayoutPanel1.SuspendLayout()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.Location = New System.Drawing.Point(0, 0)
        SplitContainer1.Name = "SplitContainer1"
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(_acListBox)
        SplitContainer1.Panel1.Controls.Add(TableLayoutPanel1)
        SplitContainer1.Panel1.Controls.Add(pnlBottom)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(DocMapTree)
        SplitContainer1.Size = New System.Drawing.Size(1207, 644)
        SplitContainer1.SplitterDistance = 792
        SplitContainer1.SplitterWidth = 10
        SplitContainer1.TabIndex = 0
        ' 
        ' _acListBox
        ' 
        _acListBox.BackColor = Drawing.Color.FromArgb(CByte(45), CByte(45), CByte(48))
        _acListBox.BorderStyle = BorderStyle.FixedSingle
        _acListBox.Font = New System.Drawing.Font("Consolas", 10F)
        _acListBox.ForeColor = Drawing.Color.FromArgb(CByte(220), CByte(220), CByte(220))
        _acListBox.ItemHeight = 23
        _acListBox.Location = New System.Drawing.Point(0, 0)
        _acListBox.Name = "_acListBox"
        _acListBox.Size = New System.Drawing.Size(120, 94)
        _acListBox.TabIndex = 5
        _acListBox.TabStop = False
        _acListBox.Visible = False
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 1
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 20F))
        TableLayoutPanel1.Controls.Add(lblPath, 0, 0)
        TableLayoutPanel1.Controls.Add(rtbEditor, 0, 1)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.Size = New System.Drawing.Size(792, 600)
        TableLayoutPanel1.TabIndex = 8
        ' 
        ' lblPath
        ' 
        lblPath.Dock = DockStyle.Top
        lblPath.Font = New System.Drawing.Font("Segoe UI", 8F)
        lblPath.ForeColor = Drawing.Color.FromArgb(CByte(180), CByte(180), CByte(180))
        lblPath.Location = New System.Drawing.Point(3, 0)
        lblPath.Name = "lblPath"
        lblPath.Size = New System.Drawing.Size(786, 30)
        lblPath.TabIndex = 13
        lblPath.Text = "Path"
        lblPath.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' rtbEditor
        ' 
        rtbEditor.AcceptsTab = True
        rtbEditor.BackColor = Drawing.SystemColors.ControlDark
        rtbEditor.BorderStyle = BorderStyle.None
        rtbEditor.DetectUrls = False
        rtbEditor.Dock = DockStyle.Fill
        rtbEditor.Font = New System.Drawing.Font("Consolas", 10.5F)
        rtbEditor.Location = New System.Drawing.Point(0, 30)
        rtbEditor.Margin = New Padding(0)
        rtbEditor.Name = "rtbEditor"
        rtbEditor.Size = New System.Drawing.Size(792, 570)
        rtbEditor.TabIndex = 12
        rtbEditor.Text = ""
        rtbEditor.WordWrap = False
        ' 
        ' pnlBottom
        ' 
        pnlBottom.BackColor = Drawing.Color.FromArgb(CByte(50), CByte(50), CByte(50))
        pnlBottom.Controls.Add(chkWordWrap)
        pnlBottom.Controls.Add(lblStatus)
        pnlBottom.Controls.Add(btnCancel)
        pnlBottom.Controls.Add(btnSaveAs)
        pnlBottom.Dock = DockStyle.Bottom
        pnlBottom.Location = New System.Drawing.Point(0, 600)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Padding = New Padding(8, 6, 8, 6)
        pnlBottom.Size = New System.Drawing.Size(792, 44)
        pnlBottom.TabIndex = 7
        ' 
        ' lblStatus
        ' 
        lblStatus.Dock = DockStyle.Right
        lblStatus.Font = New System.Drawing.Font("Segoe UI", 8.5F)
        lblStatus.ForeColor = Drawing.Color.FromArgb(CByte(180), CByte(180), CByte(180))
        lblStatus.Location = New System.Drawing.Point(118, 6)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New System.Drawing.Size(446, 32)
        lblStatus.TabIndex = 0
        lblStatus.Text = "Gata."
        lblStatus.TextAlign = Drawing.ContentAlignment.MiddleLeft
        ' 
        ' btnCancel
        ' 
        btnCancel.BackColor = Drawing.Color.FromArgb(CByte(70), CByte(70), CByte(70))
        btnCancel.Dock = DockStyle.Right
        btnCancel.FlatAppearance.BorderColor = Drawing.Color.FromArgb(CByte(90), CByte(90), CByte(90))
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.ForeColor = Drawing.Color.FromArgb(CByte(210), CByte(210), CByte(210))
        btnCancel.Location = New System.Drawing.Point(564, 6)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New System.Drawing.Size(90, 32)
        btnCancel.TabIndex = 1
        btnCancel.Text = "Renunță"
        btnCancel.UseVisualStyleBackColor = False
        ' 
        ' btnSaveAs
        ' 
        btnSaveAs.BackColor = Drawing.Color.FromArgb(CByte(25), CByte(118), CByte(210))
        btnSaveAs.Dock = DockStyle.Right
        btnSaveAs.FlatAppearance.BorderSize = 0
        btnSaveAs.FlatStyle = FlatStyle.Flat
        btnSaveAs.ForeColor = Drawing.Color.White
        btnSaveAs.Location = New System.Drawing.Point(654, 6)
        btnSaveAs.Name = "btnSaveAs"
        btnSaveAs.Size = New System.Drawing.Size(130, 32)
        btnSaveAs.TabIndex = 2
        btnSaveAs.Text = "Salvează ca...  💾"
        btnSaveAs.UseVisualStyleBackColor = False
        ' 
        ' DocMapTree
        ' 
        DocMapTree.BackColor = Drawing.Color.FromArgb(CByte(37), CByte(37), CByte(38))
        DocMapTree.BorderStyle = BorderStyle.None
        DocMapTree.Dock = DockStyle.Fill
        DocMapTree.Font = New System.Drawing.Font("Consolas", 9F)
        DocMapTree.ForeColor = Drawing.Color.FromArgb(CByte(212), CByte(212), CByte(212))
        DocMapTree.FullRowSelect = True
        DocMapTree.HideSelection = False
        DocMapTree.Indent = 16
        DocMapTree.Location = New System.Drawing.Point(0, 0)
        DocMapTree.Name = "DocMapTree"
        DocMapTree.ShowNodeToolTips = True
        DocMapTree.Size = New System.Drawing.Size(405, 644)
        DocMapTree.TabIndex = 0
        ' 
        ' chkWordWrap
        ' 
        chkWordWrap.AutoSize = True
        chkWordWrap.Dock = DockStyle.Left
        chkWordWrap.ForeColor = Drawing.SystemColors.ButtonHighlight
        chkWordWrap.Location = New System.Drawing.Point(8, 6)
        chkWordWrap.Name = "chkWordWrap"
        chkWordWrap.Size = New System.Drawing.Size(81, 32)
        chkWordWrap.TabIndex = 3
        chkWordWrap.Text = "Wrap"
        chkWordWrap.UseVisualStyleBackColor = True
        ' 
        ' WorkflowEditorForm
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(10F, 25F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Drawing.Color.FromArgb(CByte(80), CByte(80), CByte(80))
        ClientSize = New System.Drawing.Size(1207, 644)
        Controls.Add(SplitContainer1)
        DoubleBuffered = True
        Font = New System.Drawing.Font("Segoe UI", 9F)
        MinimumSize = New System.Drawing.Size(600, 400)
        Name = "WorkflowEditorForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Editor Workflow"
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, System.ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        TableLayoutPanel1.ResumeLayout(False)
        pnlBottom.ResumeLayout(False)
        pnlBottom.PerformLayout()
        ResumeLayout(False)

    End Sub

    Friend WithEvents SplitContainer1 As SplitContainer
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents lblStatus As Label
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnSaveAs As Button
    Friend WithEvents _acListBox As ListBox
    Friend WithEvents DocMapTree As NoHScrollTreeView
    Friend WithEvents ErrTooltip As ToolTip
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents lblPath As Label
    Friend WithEvents rtbEditor As RichTextBox
    Friend WithEvents chkWordWrap As CheckBox

End Class