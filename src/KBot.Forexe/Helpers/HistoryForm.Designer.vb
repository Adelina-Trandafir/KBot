<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HistoryForm
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        SplitContainer1 = New System.Windows.Forms.SplitContainer()
        tvHistory = New System.Windows.Forms.TreeView()
        pnlHistoryActions = New System.Windows.Forms.Panel()
        btnSaveAllHistory = New System.Windows.Forms.Button()
        tcDetails = New System.Windows.Forms.TabControl()
        tpLog = New System.Windows.Forms.TabPage()
        rtbLog = New System.Windows.Forms.RichTextBox()
        tpInput = New System.Windows.Forms.TabPage()
        rtbInput = New System.Windows.Forms.RichTextBox()
        tpOutput = New System.Windows.Forms.TabPage()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        pnlHistoryActions.SuspendLayout()
        tcDetails.SuspendLayout()
        tpLog.SuspendLayout()
        tpInput.SuspendLayout()
        SuspendLayout()
        '
        ' SplitContainer1
        '
        SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill
        SplitContainer1.Location = New System.Drawing.Point(0, 0)
        SplitContainer1.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        SplitContainer1.Name = "SplitContainer1"
        '
        ' SplitContainer1.Panel1
        '
        SplitContainer1.Panel1.Controls.Add(tvHistory)
        SplitContainer1.Panel1.Controls.Add(pnlHistoryActions)
        '
        ' SplitContainer1.Panel2
        '
        SplitContainer1.Panel2.Controls.Add(tcDetails)
        SplitContainer1.Size = New System.Drawing.Size(1014, 500)
        SplitContainer1.SplitterDistance = 337
        SplitContainer1.SplitterWidth = 5
        SplitContainer1.TabIndex = 0
        '
        ' tvHistory
        '
        tvHistory.Dock = System.Windows.Forms.DockStyle.Fill
        tvHistory.Font = New System.Drawing.Font("Consolas", 10.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        tvHistory.FullRowSelect = True
        tvHistory.HideSelection = False
        tvHistory.Location = New System.Drawing.Point(0, 0)
        tvHistory.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tvHistory.Name = "tvHistory"
        tvHistory.Size = New System.Drawing.Size(337, 460)
        tvHistory.TabIndex = 0
        '
        ' pnlHistoryActions
        '
        pnlHistoryActions.Controls.Add(btnSaveAllHistory)
        pnlHistoryActions.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlHistoryActions.Height = 40
        pnlHistoryActions.Name = "pnlHistoryActions"
        pnlHistoryActions.Padding = New System.Windows.Forms.Padding(4)
        '
        ' btnSaveAllHistory
        '
        btnSaveAllHistory.Dock = System.Windows.Forms.DockStyle.Fill
        btnSaveAllHistory.Font = New System.Drawing.Font("Segoe UI", 9.0F, Drawing.FontStyle.Regular)
        btnSaveAllHistory.Text = "💾 Salvează Istoricul..."
        btnSaveAllHistory.UseVisualStyleBackColor = True
        btnSaveAllHistory.Name = "btnSaveAllHistory"
        btnSaveAllHistory.TabIndex = 0
        '
        ' tcDetails
        '
        tcDetails.Controls.Add(tpLog)
        tcDetails.Controls.Add(tpInput)
        tcDetails.Controls.Add(tpOutput)
        tcDetails.Dock = System.Windows.Forms.DockStyle.Fill
        tcDetails.Location = New System.Drawing.Point(0, 0)
        tcDetails.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tcDetails.Name = "tcDetails"
        tcDetails.SelectedIndex = 0
        tcDetails.Size = New System.Drawing.Size(672, 500)
        tcDetails.TabIndex = 0
        '
        ' tpLog
        '
        tpLog.Controls.Add(rtbLog)
        tpLog.Location = New System.Drawing.Point(4, 34)
        tpLog.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tpLog.Name = "tpLog"
        tpLog.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tpLog.Size = New System.Drawing.Size(664, 462)
        tpLog.TabIndex = 0
        tpLog.Text = "Log Execuție"
        tpLog.UseVisualStyleBackColor = True
        '
        ' rtbLog
        '
        rtbLog.BackColor = Drawing.Color.White
        rtbLog.Dock = System.Windows.Forms.DockStyle.Fill
        rtbLog.Font = New System.Drawing.Font("Consolas", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        rtbLog.Location = New System.Drawing.Point(4, 5)
        rtbLog.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        rtbLog.Name = "rtbLog"
        rtbLog.ReadOnly = True
        rtbLog.Size = New System.Drawing.Size(656, 452)
        rtbLog.TabIndex = 0
        rtbLog.Text = ""
        '
        ' tpInput
        '
        tpInput.Controls.Add(rtbInput)
        tpInput.Location = New System.Drawing.Point(4, 34)
        tpInput.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tpInput.Name = "tpInput"
        tpInput.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tpInput.Size = New System.Drawing.Size(664, 462)
        tpInput.TabIndex = 1
        tpInput.Text = "Input (JSON)"
        tpInput.UseVisualStyleBackColor = True
        '
        ' rtbInput
        '
        rtbInput.BackColor = Drawing.Color.WhiteSmoke
        rtbInput.Dock = System.Windows.Forms.DockStyle.Fill
        rtbInput.Font = New System.Drawing.Font("Consolas", 9.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        rtbInput.Location = New System.Drawing.Point(4, 5)
        rtbInput.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        rtbInput.Name = "rtbInput"
        rtbInput.ReadOnly = True
        rtbInput.Size = New System.Drawing.Size(656, 452)
        rtbInput.TabIndex = 0
        rtbInput.Text = ""
        '
        ' tpOutput
        '
        tpOutput.Location = New System.Drawing.Point(4, 34)
        tpOutput.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tpOutput.Name = "tpOutput"
        tpOutput.Padding = New System.Windows.Forms.Padding(4, 5, 4, 5)
        tpOutput.Size = New System.Drawing.Size(664, 462)
        tpOutput.TabIndex = 2
        tpOutput.Text = "Output (Rezultat)"
        tpOutput.UseVisualStyleBackColor = True
        '
        ' HistoryForm
        '
        AutoScaleDimensions = New System.Drawing.SizeF(10.0F, 25.0F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(1014, 500)
        Controls.Add(SplitContainer1)
        Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Name = "HistoryForm"
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Text = "Istoric K-BOT - Detaliat"
        TopMost = True
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        pnlHistoryActions.ResumeLayout(False)
        tcDetails.ResumeLayout(False)
        tpLog.ResumeLayout(False)
        tpInput.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents SplitContainer1 As System.Windows.Forms.SplitContainer
    Friend WithEvents tvHistory As System.Windows.Forms.TreeView
    Friend WithEvents pnlHistoryActions As System.Windows.Forms.Panel
    Friend WithEvents btnSaveAllHistory As System.Windows.Forms.Button
    Friend WithEvents tcDetails As System.Windows.Forms.TabControl
    Friend WithEvents tpLog As System.Windows.Forms.TabPage
    Friend WithEvents rtbLog As System.Windows.Forms.RichTextBox
    Friend WithEvents tpInput As System.Windows.Forms.TabPage
    Friend WithEvents rtbInput As System.Windows.Forms.RichTextBox
    Friend WithEvents tpOutput As System.Windows.Forms.TabPage
    Friend WithEvents rtbOutput As System.Windows.Forms.RichTextBox
End Class