<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DataViewHarnessForm
    Inherits KBot.Theming.KBotThemedForm

    ' Proba vizuală a KBotDataView: o grilă încărcată cu date sintetice (mii de rânduri ×
    ' zeci de coloane), butoanele de comutare a temei și verdictul uman Pass/Fail.
    ' Controalele sunt declarate aici (regula: controalele WinForms în .Designer.vb).

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblInfo As System.Windows.Forms.Label

    Friend WithEvents grid As KBot.Controls.KBotDataView

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.grid = New KBot.Controls.KBotDataView()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.pnlTop.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop — comutatoarele de temă
        '
        Me.pnlTop.AutoSize = True
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 40
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlTop.Controls.Add(Me.btnClassic)
        Me.pnlTop.Controls.Add(Me.btnDark)
        Me.pnlTop.Controls.Add(Me.btnModern)
        Me.pnlTop.Controls.Add(Me.lblInfo)
        Me.pnlTop.Name = "pnlTop"
        '
        'btnClassic
        '
        Me.btnClassic.AutoSize = True
        Me.btnClassic.Text = "Classic"
        Me.btnClassic.UseVisualStyleBackColor = True
        Me.btnClassic.Name = "btnClassic"
        '
        'btnDark
        '
        Me.btnDark.AutoSize = True
        Me.btnDark.Text = "Dark"
        Me.btnDark.UseVisualStyleBackColor = True
        Me.btnDark.Name = "btnDark"
        '
        'btnModern
        '
        Me.btnModern.AutoSize = True
        Me.btnModern.Text = "Modern"
        Me.btnModern.UseVisualStyleBackColor = True
        Me.btnModern.Name = "btnModern"
        '
        'lblInfo
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Padding = New System.Windows.Forms.Padding(12, 6, 0, 0)
        Me.lblInfo.Name = "lblInfo"
        '
        'grid — controlul testat
        '
        Me.grid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grid.Name = "grid"
        '
        'pnlButtons — verdictul uman
        '
        Me.pnlButtons.AutoSize = True
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Name = "pnlButtons"
        '
        'btnFail
        '
        Me.btnFail.AutoSize = True
        Me.btnFail.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnFail.Text = "Fail"
        Me.btnFail.UseVisualStyleBackColor = True
        Me.btnFail.Name = "btnFail"
        '
        'btnPass
        '
        Me.btnPass.AutoSize = True
        Me.btnPass.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnPass.Text = "Pass"
        Me.btnPass.UseVisualStyleBackColor = True
        Me.btnPass.Name = "btnPass"
        '
        'DataViewHarnessForm
        '
        Me.AcceptButton = Me.btnPass
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1000, 620)
        ' Ordine INVERSĂ de dock (regula casei): Fill întâi, apoi Bottom/Top.
        Me.Controls.Add(Me.grid)
        Me.Controls.Add(Me.pnlButtons)
        Me.Controls.Add(Me.pnlTop)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "KBotDataView — probă vizuală (virtualizare + temă)"
        Me.Name = "DataViewHarnessForm"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

End Class
