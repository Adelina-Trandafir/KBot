<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ThemeGalleryForm
    Inherits KBot.Theming.KBotThemedForm

    ' Formular-galerie: câte un exemplar din fiecare tip de control tematizat, plus
    ' butoanele Classic/Dark/Modern (comutare live) și Pass/Fail (verdict uman).
    ' Controalele sunt declarate aici (regula: controalele WinForms în .Designer.vb).

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblActive As System.Windows.Forms.Label

    Friend WithEvents pnlBody As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblSample As System.Windows.Forms.Label
    Friend WithEvents txtSample As System.Windows.Forms.TextBox
    Friend WithEvents mtxtSample As System.Windows.Forms.MaskedTextBox
    Friend WithEvents cboSample As System.Windows.Forms.ComboBox
    Friend WithEvents numSample As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkSample As System.Windows.Forms.CheckBox
    Friend WithEvents optSample As System.Windows.Forms.RadioButton
    Friend WithEvents btnSample As System.Windows.Forms.Button
    Friend WithEvents grpSample As System.Windows.Forms.GroupBox
    Friend WithEvents lstSample As System.Windows.Forms.ListBox
    Friend WithEvents clbSample As System.Windows.Forms.CheckedListBox
    Friend WithEvents tvSample As System.Windows.Forms.TreeView
    Friend WithEvents tabSample As System.Windows.Forms.TabControl
    Friend WithEvents tabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents tabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents pbSample As System.Windows.Forms.ProgressBar
    Friend WithEvents rtbSample As System.Windows.Forms.RichTextBox

    Friend WithEvents pnlButtons As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnFail As System.Windows.Forms.Button
    Friend WithEvents btnPass As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlTop = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnClassic = New System.Windows.Forms.Button()
        Me.btnDark = New System.Windows.Forms.Button()
        Me.btnModern = New System.Windows.Forms.Button()
        Me.lblActive = New System.Windows.Forms.Label()
        Me.pnlBody = New System.Windows.Forms.FlowLayoutPanel()
        Me.lblSample = New System.Windows.Forms.Label()
        Me.txtSample = New System.Windows.Forms.TextBox()
        Me.mtxtSample = New System.Windows.Forms.MaskedTextBox()
        Me.cboSample = New System.Windows.Forms.ComboBox()
        Me.numSample = New System.Windows.Forms.NumericUpDown()
        Me.chkSample = New System.Windows.Forms.CheckBox()
        Me.optSample = New System.Windows.Forms.RadioButton()
        Me.btnSample = New System.Windows.Forms.Button()
        Me.grpSample = New System.Windows.Forms.GroupBox()
        Me.lstSample = New System.Windows.Forms.ListBox()
        Me.clbSample = New System.Windows.Forms.CheckedListBox()
        Me.tvSample = New System.Windows.Forms.TreeView()
        Me.tabSample = New System.Windows.Forms.TabControl()
        Me.tabPage1 = New System.Windows.Forms.TabPage()
        Me.tabPage2 = New System.Windows.Forms.TabPage()
        Me.pbSample = New System.Windows.Forms.ProgressBar()
        Me.rtbSample = New System.Windows.Forms.RichTextBox()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        CType(Me.numSample, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlTop.SuspendLayout()
        Me.pnlBody.SuspendLayout()
        Me.tabSample.SuspendLayout()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Controls.Add(Me.btnClassic)
        Me.pnlTop.Controls.Add(Me.btnDark)
        Me.pnlTop.Controls.Add(Me.btnModern)
        Me.pnlTop.Controls.Add(Me.lblActive)
        Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTop.Height = 44
        Me.pnlTop.Padding = New System.Windows.Forms.Padding(6)
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
        'lblActive
        '
        Me.lblActive.AutoSize = True
        Me.lblActive.Margin = New System.Windows.Forms.Padding(12, 9, 3, 0)
        Me.lblActive.Text = "activ: —"
        Me.lblActive.Name = "lblActive"
        '
        'pnlBody
        '
        Me.pnlBody.AutoScroll = True
        Me.pnlBody.Controls.Add(Me.lblSample)
        Me.pnlBody.Controls.Add(Me.txtSample)
        Me.pnlBody.Controls.Add(Me.mtxtSample)
        Me.pnlBody.Controls.Add(Me.cboSample)
        Me.pnlBody.Controls.Add(Me.numSample)
        Me.pnlBody.Controls.Add(Me.chkSample)
        Me.pnlBody.Controls.Add(Me.optSample)
        Me.pnlBody.Controls.Add(Me.btnSample)
        Me.pnlBody.Controls.Add(Me.grpSample)
        Me.pnlBody.Controls.Add(Me.lstSample)
        Me.pnlBody.Controls.Add(Me.clbSample)
        Me.pnlBody.Controls.Add(Me.tvSample)
        Me.pnlBody.Controls.Add(Me.tabSample)
        Me.pnlBody.Controls.Add(Me.pbSample)
        Me.pnlBody.Controls.Add(Me.rtbSample)
        Me.pnlBody.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlBody.Padding = New System.Windows.Forms.Padding(8)
        Me.pnlBody.Name = "pnlBody"
        '
        'lblSample
        '
        Me.lblSample.AutoSize = True
        Me.lblSample.Margin = New System.Windows.Forms.Padding(6, 9, 6, 3)
        Me.lblSample.Text = "Etichetă (Label)"
        Me.lblSample.Name = "lblSample"
        '
        'txtSample
        '
        Me.txtSample.Size = New System.Drawing.Size(160, 23)
        Me.txtSample.Margin = New System.Windows.Forms.Padding(6)
        Me.txtSample.Text = "TextBox"
        Me.txtSample.Name = "txtSample"
        '
        'mtxtSample
        '
        Me.mtxtSample.Size = New System.Drawing.Size(120, 23)
        Me.mtxtSample.Margin = New System.Windows.Forms.Padding(6)
        Me.mtxtSample.Mask = "00/00/0000"
        Me.mtxtSample.Name = "mtxtSample"
        '
        'cboSample
        '
        Me.cboSample.Size = New System.Drawing.Size(140, 23)
        Me.cboSample.Margin = New System.Windows.Forms.Padding(6)
        Me.cboSample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboSample.Name = "cboSample"
        '
        'numSample
        '
        Me.numSample.Size = New System.Drawing.Size(90, 23)
        Me.numSample.Margin = New System.Windows.Forms.Padding(6)
        Me.numSample.Name = "numSample"
        '
        'chkSample
        '
        Me.chkSample.AutoSize = True
        Me.chkSample.Margin = New System.Windows.Forms.Padding(6, 9, 6, 3)
        Me.chkSample.Text = "CheckBox"
        Me.chkSample.Name = "chkSample"
        '
        'optSample
        '
        Me.optSample.AutoSize = True
        Me.optSample.Margin = New System.Windows.Forms.Padding(6, 9, 6, 3)
        Me.optSample.Text = "RadioButton"
        Me.optSample.Name = "optSample"
        '
        'btnSample
        '
        Me.btnSample.AutoSize = True
        Me.btnSample.Margin = New System.Windows.Forms.Padding(6)
        Me.btnSample.Text = "Buton"
        Me.btnSample.UseVisualStyleBackColor = True
        Me.btnSample.Name = "btnSample"
        '
        'grpSample
        '
        Me.grpSample.Size = New System.Drawing.Size(150, 60)
        Me.grpSample.Margin = New System.Windows.Forms.Padding(6)
        Me.grpSample.Text = "GroupBox"
        Me.grpSample.Name = "grpSample"
        '
        'lstSample
        '
        Me.lstSample.Size = New System.Drawing.Size(150, 80)
        Me.lstSample.Margin = New System.Windows.Forms.Padding(6)
        Me.lstSample.Name = "lstSample"
        '
        'clbSample
        '
        Me.clbSample.Size = New System.Drawing.Size(150, 80)
        Me.clbSample.Margin = New System.Windows.Forms.Padding(6)
        Me.clbSample.Name = "clbSample"
        '
        'tvSample
        '
        Me.tvSample.Size = New System.Drawing.Size(150, 100)
        Me.tvSample.Margin = New System.Windows.Forms.Padding(6)
        Me.tvSample.Name = "tvSample"
        '
        'tabSample
        '
        Me.tabSample.Controls.Add(Me.tabPage1)
        Me.tabSample.Controls.Add(Me.tabPage2)
        Me.tabSample.Size = New System.Drawing.Size(200, 100)
        Me.tabSample.Margin = New System.Windows.Forms.Padding(6)
        Me.tabSample.Name = "tabSample"
        '
        'tabPage1
        '
        Me.tabPage1.Text = "Tab 1"
        Me.tabPage1.Name = "tabPage1"
        '
        'tabPage2
        '
        Me.tabPage2.Text = "Tab 2"
        Me.tabPage2.Name = "tabPage2"
        '
        'pbSample
        '
        Me.pbSample.Size = New System.Drawing.Size(160, 22)
        Me.pbSample.Margin = New System.Windows.Forms.Padding(6)
        Me.pbSample.Value = 60
        Me.pbSample.Name = "pbSample"
        '
        'rtbSample
        '
        Me.rtbSample.Size = New System.Drawing.Size(200, 80)
        Me.rtbSample.Margin = New System.Windows.Forms.Padding(6)
        Me.rtbSample.Text = "RichTextBox"
        Me.rtbSample.Name = "rtbSample"
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnFail)
        Me.pnlButtons.Controls.Add(Me.btnPass)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.pnlButtons.Height = 44
        Me.pnlButtons.Padding = New System.Windows.Forms.Padding(6)
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
        'ThemeGalleryForm
        '
        Me.AcceptButton = Me.btnPass
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(640, 560)
        Me.Controls.Add(Me.pnlBody)
        Me.Controls.Add(Me.pnlTop)
        Me.Controls.Add(Me.pnlButtons)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Galerie temă — Classic / Dark / Modern"
        Me.Name = "ThemeGalleryForm"
        CType(Me.numSample, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlBody.ResumeLayout(False)
        Me.pnlBody.PerformLayout()
        Me.tabSample.ResumeLayout(False)
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
