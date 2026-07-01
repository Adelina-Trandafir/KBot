<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TreeVisualForm
    Inherits System.Windows.Forms.Form

    ' NOTĂ: Dispose (pentru _rightIcon) rămâne în TreeVisualForm.vb, deci nu e generat aici.
    ' Nodurile arborelui (date hardcodate) se populează în cod (PopulateTree), nu la design-time.

    Friend WithEvents tree As KBot.Controls.AdvancedTreeControl
    Friend WithEvents pnl As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.tree = New KBot.Controls.AdvancedTreeControl()
        Me.pnl = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.pnl.SuspendLayout()
        Me.SuspendLayout()
        '
        'tree
        '
        Me.tree.CheckBoxes = True
        Me.tree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tree.Name = "tree"
        '
        'pnl
        '
        Me.pnl.Controls.Add(Me.btnFail)
        Me.pnl.Controls.Add(Me.btnPass)
        Me.pnl.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnl.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnl.Height = 40
        Me.pnl.Name = "pnl"
        Me.pnl.Padding = New System.Windows.Forms.Padding(6)
        '
        'btnFail
        '
        Me.btnFail.AutoSize = True
        Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnFail.Name = "btnFail"
        Me.btnFail.Text = "Fail"
        Me.btnFail.UseVisualStyleBackColor = True
        '
        'btnPass
        '
        Me.btnPass.AutoSize = True
        Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnPass.Name = "btnPass"
        Me.btnPass.Text = "Pass"
        Me.btnPass.UseVisualStyleBackColor = True
        '
        'TreeVisualForm
        '
        Me.AcceptButton = Me.btnPass
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.btnFail
        Me.ClientSize = New System.Drawing.Size(504, 521)
        Me.Controls.Add(Me.tree)
        Me.Controls.Add(Me.pnl)
        Me.Name = "TreeVisualForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "AdvancedTreeControl — proba vizuală"
        Me.pnl.ResumeLayout(False)
        Me.pnl.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
