<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DevHarnessForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnRunChecked As System.Windows.Forms.Button
    Friend WithEvents btnRunSelected As System.Windows.Forms.Button
    Friend WithEvents btnRunAll As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnClear As System.Windows.Forms.Button
    Friend WithEvents btnOpenMainForm As System.Windows.Forms.Button
    Friend WithEvents filterBar As System.Windows.Forms.Panel
    Friend WithEvents txtFilter As System.Windows.Forms.TextBox
    Friend WithEvents lblFilter As System.Windows.Forms.Label
    Friend WithEvents split As System.Windows.Forms.SplitContainer
    Friend WithEvents clbTests As System.Windows.Forms.CheckedListBox
    Friend WithEvents rtbResults As System.Windows.Forms.RichTextBox
    Friend WithEvents pnlBottom As System.Windows.Forms.Panel
    Friend WithEvents pbProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents lblStatus As System.Windows.Forms.Label

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnRunChecked = New System.Windows.Forms.Button()
        Me.btnRunSelected = New System.Windows.Forms.Button()
        Me.btnRunAll = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnOpenMainForm = New System.Windows.Forms.Button()
        Me.filterBar = New System.Windows.Forms.Panel()
        Me.txtFilter = New System.Windows.Forms.TextBox()
        Me.lblFilter = New System.Windows.Forms.Label()
        Me.split = New System.Windows.Forms.SplitContainer()
        Me.clbTests = New System.Windows.Forms.CheckedListBox()
        Me.rtbResults = New System.Windows.Forms.RichTextBox()
        Me.pnlBottom = New System.Windows.Forms.Panel()
        Me.pbProgress = New System.Windows.Forms.ProgressBar()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.pnlTop.SuspendLayout()
        Me.filterBar.SuspendLayout()
        CType(Me.split, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.split.Panel1.SuspendLayout()
        Me.split.Panel2.SuspendLayout()
        Me.split.SuspendLayout()
        Me.pnlBottom.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Controls.Add(Me.btnRunChecked)
        Me.pnlTop.Controls.Add(Me.btnRunSelected)
        Me.pnlTop.Controls.Add(Me.btnRunAll)
        Me.pnlTop.Controls.Add(Me.btnCancel)
        Me.pnlTop.Controls.Add(Me.btnClear)
        Me.pnlTop.Controls.Add(Me.btnOpenMainForm)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 38
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.WrapContents = False
        '
        'btnRunChecked
        '
        Me.btnRunChecked.AutoSize = True
        Me.btnRunChecked.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnRunChecked.Name = "btnRunChecked"
        Me.btnRunChecked.Text = "Run Checked"
        Me.btnRunChecked.UseVisualStyleBackColor = True
        '
        'btnRunSelected
        '
        Me.btnRunSelected.AutoSize = True
        Me.btnRunSelected.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnRunSelected.Name = "btnRunSelected"
        Me.btnRunSelected.Text = "Run Selected"
        Me.btnRunSelected.UseVisualStyleBackColor = True
        '
        'btnRunAll
        '
        Me.btnRunAll.AutoSize = True
        Me.btnRunAll.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnRunAll.Name = "btnRunAll"
        Me.btnRunAll.Text = "Run All"
        Me.btnRunAll.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.AutoSize = True
        Me.btnCancel.Enabled = False
        Me.btnCancel.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnClear
        '
        Me.btnClear.AutoSize = True
        Me.btnClear.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Text = "Clear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'btnOpenMainForm
        '
        Me.btnOpenMainForm.AutoSize = True
        Me.btnOpenMainForm.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnOpenMainForm.Name = "btnOpenMainForm"
        Me.btnOpenMainForm.Text = "Deschide MainForm"
        Me.btnOpenMainForm.UseVisualStyleBackColor = True
        '
        'filterBar
        '
        Me.filterBar.Controls.Add(Me.txtFilter)
        Me.filterBar.Controls.Add(Me.lblFilter)
        Me.filterBar.Dock = System.Windows.Forms.DockStyle.Top
        Me.filterBar.Height = 28
        Me.filterBar.Name = "filterBar"
        Me.filterBar.Padding = New System.Windows.Forms.Padding(6, 2, 6, 2)
        '
        'txtFilter
        '
        Me.txtFilter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtFilter.Name = "txtFilter"
        '
        'lblFilter
        '
        Me.lblFilter.AutoSize = True
        Me.lblFilter.Dock = System.Windows.Forms.DockStyle.Left
        Me.lblFilter.Name = "lblFilter"
        Me.lblFilter.Padding = New System.Windows.Forms.Padding(0, 6, 4, 0)
        Me.lblFilter.Text = "Filtru:"
        '
        'split
        '
        Me.split.Dock = System.Windows.Forms.DockStyle.Fill
        Me.split.Name = "split"
        '
        'split.Panel1
        '
        Me.split.Panel1.Controls.Add(Me.clbTests)
        '
        'split.Panel2
        '
        Me.split.Panel2.Controls.Add(Me.rtbResults)
        Me.split.SplitterDistance = 360
        '
        'clbTests
        '
        Me.clbTests.CheckOnClick = True
        Me.clbTests.Dock = System.Windows.Forms.DockStyle.Fill
        Me.clbTests.IntegralHeight = False
        Me.clbTests.Name = "clbTests"
        '
        'rtbResults
        '
        Me.rtbResults.BackColor = System.Drawing.Color.White
        Me.rtbResults.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rtbResults.Font = New System.Drawing.Font("Consolas", 9.0F)
        Me.rtbResults.Name = "rtbResults"
        '
        'pnlBottom
        '
        Me.pnlBottom.Controls.Add(Me.pbProgress)
        Me.pnlBottom.Controls.Add(Me.lblStatus)
        Me.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBottom.Height = 42
        Me.pnlBottom.Name = "pnlBottom"
        Me.pnlBottom.Padding = New System.Windows.Forms.Padding(6)
        '
        'pbProgress
        '
        Me.pbProgress.Dock = System.Windows.Forms.DockStyle.Top
        Me.pbProgress.Height = 16
        Me.pbProgress.Name = "pbProgress"
        '
        'lblStatus
        '
        Me.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.lblStatus.Height = 18
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Text = "Gata."
        '
        'DevHarnessForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(984, 621)
        Me.Controls.Add(Me.split)
        Me.Controls.Add(Me.pnlBottom)
        Me.Controls.Add(Me.filterBar)
        Me.Controls.Add(Me.pnlTop)
        Me.Name = "DevHarnessForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "K-BOT — Dev Harness (DEBUG)"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.filterBar.ResumeLayout(False)
        Me.filterBar.PerformLayout()
        Me.split.Panel1.ResumeLayout(False)
        Me.split.Panel2.ResumeLayout(False)
        CType(Me.split, System.ComponentModel.ISupportInitialize).EndInit()
        Me.split.ResumeLayout(False)
        Me.pnlBottom.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

End Class
