<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WicketMonitorForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    '<System.Diagnostics.DebuggerNonUserCode()>
    'Protected Overrides Sub Dispose(ByVal disposing As Boolean)
    '    Try
    '        If disposing AndAlso components IsNot Nothing Then
    '            components.Dispose()
    '        End If
    '    Finally
    '        MyBase.Dispose(disposing)
    '    End Try
    'End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    ' ── Declarații controale ──────────────────────────────────────────────────
    Friend WithEvents rtbMonitor As System.Windows.Forms.RichTextBox
    Friend WithEvents btnClear As System.Windows.Forms.Button
    Friend WithEvents pnlBottom As System.Windows.Forms.Panel

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        pnlBottom = New System.Windows.Forms.Panel()
        btnCopyText = New System.Windows.Forms.Button()
        btnClear = New System.Windows.Forms.Button()
        rtbMonitor = New System.Windows.Forms.RichTextBox()
        pnlBottom.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlBottom
        ' 
        pnlBottom.Controls.Add(btnCopyText)
        pnlBottom.Controls.Add(btnClear)
        pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom
        pnlBottom.Location = New System.Drawing.Point(0, 351)
        pnlBottom.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        pnlBottom.Name = "pnlBottom"
        pnlBottom.Size = New System.Drawing.Size(378, 53)
        pnlBottom.TabIndex = 2
        ' 
        ' btnCopyText
        ' 
        btnCopyText.Dock = System.Windows.Forms.DockStyle.Left
        btnCopyText.Font = New System.Drawing.Font("Segoe UI", 14F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, CByte(0))
        btnCopyText.ForeColor = Drawing.Color.Blue
        btnCopyText.Location = New System.Drawing.Point(0, 0)
        btnCopyText.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        btnCopyText.Name = "btnCopyText"
        btnCopyText.Size = New System.Drawing.Size(54, 53)
        btnCopyText.TabIndex = 1
        btnCopyText.Text = "🗐"
        btnCopyText.UseVisualStyleBackColor = True
        ' 
        ' btnClear
        ' 
        btnClear.Dock = System.Windows.Forms.DockStyle.Right
        btnClear.Location = New System.Drawing.Point(235, 0)
        btnClear.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        btnClear.Name = "btnClear"
        btnClear.Size = New System.Drawing.Size(143, 53)
        btnClear.TabIndex = 0
        btnClear.Text = "  Șterge"
        btnClear.UseVisualStyleBackColor = True
        ' 
        ' rtbMonitor
        ' 
        rtbMonitor.BorderStyle = System.Windows.Forms.BorderStyle.None
        rtbMonitor.Dock = System.Windows.Forms.DockStyle.Fill
        rtbMonitor.Font = New System.Drawing.Font("Consolas", 9.5F)
        rtbMonitor.Location = New System.Drawing.Point(0, 0)
        rtbMonitor.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        rtbMonitor.Name = "rtbMonitor"
        rtbMonitor.ReadOnly = True
        rtbMonitor.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
        rtbMonitor.Size = New System.Drawing.Size(378, 351)
        rtbMonitor.TabIndex = 1
        rtbMonitor.Text = ""
        rtbMonitor.WordWrap = False
        ' 
        ' WicketMonitorForm
        ' 
        AutoScaleDimensions = New System.Drawing.SizeF(10F, 25F)
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        ClientSize = New System.Drawing.Size(378, 404)
        Controls.Add(rtbMonitor)
        Controls.Add(pnlBottom)
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        MaximizeBox = False
        MinimumSize = New System.Drawing.Size(400, 460)
        Name = "WicketMonitorForm"
        ShowInTaskbar = False
        StartPosition = System.Windows.Forms.FormStartPosition.Manual
        Text = "Wicket Monitor"
        TopMost = True
        pnlBottom.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnCopyText As System.Windows.Forms.Button
End Class
