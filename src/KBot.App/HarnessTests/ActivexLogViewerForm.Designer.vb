#If DEBUG Then
' Slice 0078-15: the viewer of Logs\activex_check.log -- two loads side by side, each as a tree (elements as nodes, lines
' as leaves), the selected node in full under each tree.
'
' House rule: every WinForms control is declared here, in .Designer.vb.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ActivexLogViewerForm
    Inherits KBot.Theming.KBotThemedForm

    Friend WithEvents pnlBar As FlowLayoutPanel
    Friend WithEvents btnReload As Button
    Friend WithEvents chkHideTree As CheckBox
    Friend WithEvents chkHideSame As CheckBox
    Friend WithEvents btnPrevDiff As Button
    Friend WithEvents btnNextDiff As Button
    Friend WithEvents btnExport As Button
    Friend WithEvents btnLanes As Button
    Friend WithEvents lblStatus As Label

    Friend WithEvents splMain As SplitContainer

    Friend WithEvents pnlLeftBar As FlowLayoutPanel
    Friend WithEvents btnPickLeft As Button
    Friend WithEvents cboRunLeft As ComboBox
    Friend WithEvents lblFileLeft As Label
    Friend WithEvents splLeft As SplitContainer
    Friend WithEvents tvLeft As TreeView
    Friend WithEvents rtbLeft As RichTextBox

    Friend WithEvents pnlRightBar As FlowLayoutPanel
    Friend WithEvents btnPickRight As Button
    Friend WithEvents cboRunRight As ComboBox
    Friend WithEvents lblFileRight As Label
    Friend WithEvents splRight As SplitContainer
    Friend WithEvents tvRight As TreeView
    Friend WithEvents rtbRight As RichTextBox

    Friend WithEvents lblLegend As Label
    Friend WithEvents dlgOpen As OpenFileDialog
    Friend WithEvents dlgSave As SaveFileDialog

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
        components = New ComponentModel.Container()
        pnlBar = New FlowLayoutPanel()
        btnReload = New Button()
        chkHideTree = New CheckBox()
        chkHideSame = New CheckBox()
        btnPrevDiff = New Button()
        btnNextDiff = New Button()
        btnExport = New Button()
        btnLanes = New Button()
        lblStatus = New Label()
        splMain = New SplitContainer()
        pnlLeftBar = New FlowLayoutPanel()
        btnPickLeft = New Button()
        cboRunLeft = New ComboBox()
        lblFileLeft = New Label()
        splLeft = New SplitContainer()
        tvLeft = New TreeView()
        rtbLeft = New RichTextBox()
        pnlRightBar = New FlowLayoutPanel()
        btnPickRight = New Button()
        cboRunRight = New ComboBox()
        lblFileRight = New Label()
        splRight = New SplitContainer()
        tvRight = New TreeView()
        rtbRight = New RichTextBox()
        lblLegend = New Label()
        dlgOpen = New OpenFileDialog()
        dlgSave = New SaveFileDialog()
        pnlBar.SuspendLayout()
        CType(splMain, ComponentModel.ISupportInitialize).BeginInit()
        splMain.Panel1.SuspendLayout()
        splMain.Panel2.SuspendLayout()
        splMain.SuspendLayout()
        pnlLeftBar.SuspendLayout()
        CType(splLeft, ComponentModel.ISupportInitialize).BeginInit()
        splLeft.Panel1.SuspendLayout()
        splLeft.Panel2.SuspendLayout()
        splLeft.SuspendLayout()
        pnlRightBar.SuspendLayout()
        CType(splRight, ComponentModel.ISupportInitialize).BeginInit()
        splRight.Panel1.SuspendLayout()
        splRight.Panel2.SuspendLayout()
        splRight.SuspendLayout()
        SuspendLayout()
        '
        ' pnlBar
        '
        pnlBar.Controls.Add(btnReload)
        pnlBar.Controls.Add(chkHideTree)
        pnlBar.Controls.Add(chkHideSame)
        pnlBar.Controls.Add(btnPrevDiff)
        pnlBar.Controls.Add(btnNextDiff)
        pnlBar.Controls.Add(btnExport)
        pnlBar.Controls.Add(btnLanes)
        pnlBar.Controls.Add(lblStatus)
        pnlBar.Dock = DockStyle.Top
        pnlBar.Height = 42
        pnlBar.Name = "pnlBar"
        pnlBar.Padding = New Padding(8, 6, 8, 4)
        pnlBar.TabIndex = 0
        '
        ' btnReload -- both files again (the bench keeps writing to them)
        '
        btnReload.AutoSize = True
        btnReload.Name = "btnReload"
        btnReload.Padding = New Padding(8, 2, 8, 2)
        btnReload.TabIndex = 0
        btnReload.Text = "Reîncarcă"
        btnReload.UseVisualStyleBackColor = True
        '
        ' chkHideTree -- the window-tree dumps are hundreds of lines
        '
        chkHideTree.AutoSize = True
        chkHideTree.Checked = True
        chkHideTree.CheckState = CheckState.Checked
        chkHideTree.Margin = New Padding(12, 8, 3, 0)
        chkHideTree.Name = "chkHideTree"
        chkHideTree.TabIndex = 1
        chkHideTree.Text = "Fără rândurile arborilor de ferestre"
        chkHideTree.UseVisualStyleBackColor = True
        '
        ' chkHideSame -- only what differs or is missing; the trees are rebuilt, the pairing stays
        '
        chkHideSame.AutoSize = True
        chkHideSame.Margin = New Padding(12, 8, 3, 0)
        chkHideSame.Name = "chkHideSame"
        chkHideSame.TabIndex = 5
        chkHideSame.Text = "Ascunde ce e identic"
        chkHideSame.UseVisualStyleBackColor = True
        '
        ' btnPrevDiff
        '
        btnPrevDiff.AutoSize = True
        btnPrevDiff.Margin = New Padding(16, 3, 3, 3)
        btnPrevDiff.Name = "btnPrevDiff"
        btnPrevDiff.Padding = New Padding(8, 2, 8, 2)
        btnPrevDiff.TabIndex = 2
        btnPrevDiff.Text = "◀ Diferența anterioară"
        btnPrevDiff.UseVisualStyleBackColor = True
        '
        ' btnNextDiff -- the lines that differ or are missing, in log order (left first, then the right-only ones)
        '
        btnNextDiff.AutoSize = True
        btnNextDiff.Name = "btnNextDiff"
        btnNextDiff.Padding = New Padding(8, 2, 8, 2)
        btnNextDiff.TabIndex = 3
        btnNextDiff.Text = "Diferența următoare ▶"
        btnNextDiff.UseVisualStyleBackColor = True
        '
        ' btnExport -- both trees side by side into an .xlsx
        '
        btnExport.AutoSize = True
        btnExport.Margin = New Padding(16, 3, 3, 3)
        btnExport.Name = "btnExport"
        btnExport.Padding = New Padding(8, 2, 8, 2)
        btnExport.TabIndex = 6
        btnExport.Text = "Export Excel…"
        btnExport.UseVisualStyleBackColor = True
        '
        ' btnLanes -- the left tree as lanes: a root is a lane, a leaf a change in it (ActivexLaneForm)
        '
        btnLanes.AutoSize = True
        btnLanes.Name = "btnLanes"
        btnLanes.Padding = New Padding(8, 2, 8, 2)
        btnLanes.TabIndex = 7
        btnLanes.Text = "Pe culoare…"
        btnLanes.UseVisualStyleBackColor = True
        '
        ' lblStatus
        '
        lblStatus.AutoSize = True
        lblStatus.Margin = New Padding(16, 9, 3, 0)
        lblStatus.Name = "lblStatus"
        lblStatus.TabIndex = 4
        lblStatus.Text = ""
        '
        ' splMain -- the left load and the right load
        '
        splMain.Dock = DockStyle.Fill
        splMain.Name = "splMain"
        splMain.Panel1.Controls.Add(splLeft)
        splMain.Panel1.Controls.Add(pnlLeftBar)
        splMain.Panel2.Controls.Add(splRight)
        splMain.Panel2.Controls.Add(pnlRightBar)
        splMain.Size = New Size(1500, 800)
        splMain.SplitterDistance = 748
        splMain.TabIndex = 1
        '
        ' pnlLeftBar
        '
        pnlLeftBar.Controls.Add(btnPickLeft)
        pnlLeftBar.Controls.Add(cboRunLeft)
        pnlLeftBar.Controls.Add(lblFileLeft)
        pnlLeftBar.Dock = DockStyle.Top
        pnlLeftBar.Height = 36
        pnlLeftBar.Name = "pnlLeftBar"
        pnlLeftBar.Padding = New Padding(2, 2, 2, 2)
        pnlLeftBar.TabIndex = 0
        pnlLeftBar.WrapContents = False
        '
        ' btnPickLeft
        '
        btnPickLeft.AutoSize = True
        btnPickLeft.Name = "btnPickLeft"
        btnPickLeft.Padding = New Padding(6, 0, 6, 0)
        btnPickLeft.TabIndex = 0
        btnPickLeft.Text = "Jurnalul…"
        btnPickLeft.UseVisualStyleBackColor = True
        '
        ' cboRunLeft -- one entry per «===== LOAD», newest first
        '
        cboRunLeft.DropDownStyle = ComboBoxStyle.DropDownList
        cboRunLeft.Margin = New Padding(3, 4, 3, 3)
        cboRunLeft.Name = "cboRunLeft"
        cboRunLeft.Size = New Size(440, 23)
        cboRunLeft.TabIndex = 1
        '
        ' lblFileLeft
        '
        lblFileLeft.AutoSize = True
        lblFileLeft.Margin = New Padding(8, 8, 3, 0)
        lblFileLeft.Name = "lblFileLeft"
        lblFileLeft.TabIndex = 2
        lblFileLeft.Text = ""
        '
        ' splLeft -- the tree above, the selected node in full below
        '
        splLeft.Dock = DockStyle.Fill
        splLeft.Name = "splLeft"
        splLeft.Orientation = Orientation.Horizontal
        splLeft.Panel1.Controls.Add(tvLeft)
        splLeft.Panel2.Controls.Add(rtbLeft)
        splLeft.Size = New Size(748, 760)
        splLeft.SplitterDistance = 520
        splLeft.TabIndex = 1
        '
        ' tvLeft
        '
        tvLeft.Dock = DockStyle.Fill
        tvLeft.Font = New Font("Segoe UI", 11.5F)
        tvLeft.HideSelection = False
        tvLeft.Name = "tvLeft"
        tvLeft.TabIndex = 0
        '
        ' rtbLeft -- written as RTF by the form
        '
        rtbLeft.BorderStyle = BorderStyle.None
        rtbLeft.DetectUrls = False
        rtbLeft.Dock = DockStyle.Fill
        rtbLeft.Name = "rtbLeft"
        rtbLeft.ReadOnly = True
        rtbLeft.ScrollBars = RichTextBoxScrollBars.Both
        rtbLeft.TabIndex = 0
        rtbLeft.Text = ""
        rtbLeft.WordWrap = False
        '
        ' pnlRightBar
        '
        pnlRightBar.Controls.Add(btnPickRight)
        pnlRightBar.Controls.Add(cboRunRight)
        pnlRightBar.Controls.Add(lblFileRight)
        pnlRightBar.Dock = DockStyle.Top
        pnlRightBar.Height = 36
        pnlRightBar.Name = "pnlRightBar"
        pnlRightBar.Padding = New Padding(2, 2, 2, 2)
        pnlRightBar.TabIndex = 0
        pnlRightBar.WrapContents = False
        '
        ' btnPickRight
        '
        btnPickRight.AutoSize = True
        btnPickRight.Name = "btnPickRight"
        btnPickRight.Padding = New Padding(6, 0, 6, 0)
        btnPickRight.TabIndex = 0
        btnPickRight.Text = "Jurnalul…"
        btnPickRight.UseVisualStyleBackColor = True
        '
        ' cboRunRight
        '
        cboRunRight.DropDownStyle = ComboBoxStyle.DropDownList
        cboRunRight.Margin = New Padding(3, 4, 3, 3)
        cboRunRight.Name = "cboRunRight"
        cboRunRight.Size = New Size(440, 23)
        cboRunRight.TabIndex = 1
        '
        ' lblFileRight
        '
        lblFileRight.AutoSize = True
        lblFileRight.Margin = New Padding(8, 8, 3, 0)
        lblFileRight.Name = "lblFileRight"
        lblFileRight.TabIndex = 2
        lblFileRight.Text = ""
        '
        ' splRight
        '
        splRight.Dock = DockStyle.Fill
        splRight.Name = "splRight"
        splRight.Orientation = Orientation.Horizontal
        splRight.Panel1.Controls.Add(tvRight)
        splRight.Panel2.Controls.Add(rtbRight)
        splRight.Size = New Size(748, 760)
        splRight.SplitterDistance = 520
        splRight.TabIndex = 1
        '
        ' tvRight
        '
        tvRight.Dock = DockStyle.Fill
        tvRight.Font = New Font("Segoe UI", 11.5F)
        tvRight.HideSelection = False
        tvRight.Name = "tvRight"
        tvRight.TabIndex = 0
        '
        ' rtbRight
        '
        rtbRight.BorderStyle = BorderStyle.None
        rtbRight.DetectUrls = False
        rtbRight.Dock = DockStyle.Fill
        rtbRight.Name = "rtbRight"
        rtbRight.ReadOnly = True
        rtbRight.ScrollBars = RichTextBoxScrollBars.Both
        rtbRight.TabIndex = 0
        rtbRight.Text = ""
        rtbRight.WordWrap = False
        '
        ' lblLegend
        '
        lblLegend.AutoSize = False
        lblLegend.Dock = DockStyle.Bottom
        lblLegend.Height = 26
        lblLegend.Name = "lblLegend"
        lblLegend.Padding = New Padding(8, 5, 8, 0)
        lblLegend.TabIndex = 2
        lblLegend.Text = "✓ identic în celălalt jurnal (se selectează singur)   ≠ diferă: se selectează cel asemănător, cuvintele " &
                         "diferite sunt evidențiate   ✗ lipsește din celălalt   gri: ferestre și pid-uri, diferă la fiecare pornire"
        '
        ' dlgOpen
        '
        dlgOpen.Filter = "Jurnale|*.log|Toate fișierele|*.*"
        dlgOpen.Title = "Alege jurnalul ActiveX"
        '
        ' dlgSave
        '
        dlgSave.DefaultExt = "xlsx"
        dlgSave.Filter = "Registre Excel|*.xlsx"
        dlgSave.Title = "Salvează comparația în Excel"
        '
        ' ActivexLogViewerForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(1500, 900)
        ' Children in REVERSE dock order: Fill first, then the docked edges.
        Controls.Add(splMain)
        Controls.Add(lblLegend)
        Controls.Add(pnlBar)
        Name = "ActivexLogViewerForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Jurnalul ActiveX — două încărcări comparate"
        pnlBar.ResumeLayout(False)
        pnlBar.PerformLayout()
        pnlLeftBar.ResumeLayout(False)
        pnlLeftBar.PerformLayout()
        splLeft.Panel1.ResumeLayout(False)
        splLeft.Panel2.ResumeLayout(False)
        CType(splLeft, ComponentModel.ISupportInitialize).EndInit()
        splLeft.ResumeLayout(False)
        pnlRightBar.ResumeLayout(False)
        pnlRightBar.PerformLayout()
        splRight.Panel1.ResumeLayout(False)
        splRight.Panel2.ResumeLayout(False)
        CType(splRight, ComponentModel.ISupportInitialize).EndInit()
        splRight.ResumeLayout(False)
        splMain.Panel1.ResumeLayout(False)
        splMain.Panel2.ResumeLayout(False)
        CType(splMain, ComponentModel.ISupportInitialize).EndInit()
        splMain.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

End Class
#End If
