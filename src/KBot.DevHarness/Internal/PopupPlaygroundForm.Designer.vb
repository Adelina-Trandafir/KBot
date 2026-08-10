<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PopupPlaygroundForm
    Inherits KBot.Theming.KBotThemedForm

    ' Bancul de probă al lui CustomPopup: butoanele care-l deschid în cele trei feluri, comutatoarele
    ' care-i schimbă conținutul (pictograme / separatori / rând dezactivat / selecție inițială),
    ' butoanele de temă (comutare LIVE) și jurnalul alegerilor. Controalele sunt declarate aici
    ' (regula casei: controalele WinForms în .Designer.vb).

    Friend WithEvents pnlTop As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnClassic As System.Windows.Forms.Button
    Friend WithEvents btnDark As System.Windows.Forms.Button
    Friend WithEvents btnModern As System.Windows.Forms.Button
    Friend WithEvents lblActive As System.Windows.Forms.Label

    Friend WithEvents pnlOptions As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents chkImagini As System.Windows.Forms.CheckBox
    Friend WithEvents chkSeparatori As System.Windows.Forms.CheckBox
    Friend WithEvents chkDezactivat As System.Windows.Forms.CheckBox
    Friend WithEvents chkSelectie As System.Windows.Forms.CheckBox
    Friend WithEvents chkMulte As System.Windows.Forms.CheckBox

    Friend WithEvents pnlActions As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents btnSubButon As System.Windows.Forms.Button
    Friend WithEvents btnLaCursor As System.Windows.Forms.Button
    Friend WithEvents lblHint As System.Windows.Forms.Label

    Friend WithEvents lstLog As System.Windows.Forms.ListBox

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
        Me.pnlOptions = New System.Windows.Forms.FlowLayoutPanel()
        Me.chkImagini = New System.Windows.Forms.CheckBox()
        Me.chkSeparatori = New System.Windows.Forms.CheckBox()
        Me.chkDezactivat = New System.Windows.Forms.CheckBox()
        Me.chkSelectie = New System.Windows.Forms.CheckBox()
        Me.chkMulte = New System.Windows.Forms.CheckBox()
        Me.pnlActions = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnSubButon = New System.Windows.Forms.Button()
        Me.btnLaCursor = New System.Windows.Forms.Button()
        Me.lblHint = New System.Windows.Forms.Label()
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.pnlButtons = New System.Windows.Forms.FlowLayoutPanel()
        Me.btnFail = New System.Windows.Forms.Button()
        Me.btnPass = New System.Windows.Forms.Button()
        Me.pnlTop.SuspendLayout()
        Me.pnlOptions.SuspendLayout()
        Me.pnlActions.SuspendLayout()
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
        'pnlOptions
        '
        Me.pnlOptions.Controls.Add(Me.chkImagini)
        Me.pnlOptions.Controls.Add(Me.chkSeparatori)
        Me.pnlOptions.Controls.Add(Me.chkDezactivat)
        Me.pnlOptions.Controls.Add(Me.chkSelectie)
        Me.pnlOptions.Controls.Add(Me.chkMulte)
        Me.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlOptions.Height = 40
        Me.pnlOptions.Padding = New System.Windows.Forms.Padding(6, 4, 6, 4)
        Me.pnlOptions.Name = "pnlOptions"
        '
        'chkImagini
        '
        Me.chkImagini.AutoSize = True
        Me.chkImagini.Checked = True
        Me.chkImagini.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkImagini.Margin = New System.Windows.Forms.Padding(3, 5, 12, 3)
        Me.chkImagini.Text = "pictograme"
        Me.chkImagini.Name = "chkImagini"
        '
        'chkSeparatori
        '
        Me.chkSeparatori.AutoSize = True
        Me.chkSeparatori.Checked = True
        Me.chkSeparatori.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSeparatori.Margin = New System.Windows.Forms.Padding(3, 5, 12, 3)
        Me.chkSeparatori.Text = "separatori"
        Me.chkSeparatori.Name = "chkSeparatori"
        '
        'chkDezactivat
        '
        Me.chkDezactivat.AutoSize = True
        Me.chkDezactivat.Checked = True
        Me.chkDezactivat.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkDezactivat.Margin = New System.Windows.Forms.Padding(3, 5, 12, 3)
        Me.chkDezactivat.Text = "un rând dezactivat"
        Me.chkDezactivat.Name = "chkDezactivat"
        '
        'chkSelectie
        '
        Me.chkSelectie.AutoSize = True
        Me.chkSelectie.Checked = True
        Me.chkSelectie.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSelectie.Margin = New System.Windows.Forms.Padding(3, 5, 12, 3)
        Me.chkSelectie.Text = "selecție din constructor"
        Me.chkSelectie.Name = "chkSelectie"
        '
        'chkMulte
        '
        Me.chkMulte.AutoSize = True
        Me.chkMulte.Margin = New System.Windows.Forms.Padding(3, 5, 12, 3)
        Me.chkMulte.Text = "40 de rânduri (derulare)"
        Me.chkMulte.Name = "chkMulte"
        '
        'pnlActions
        '
        Me.pnlActions.Controls.Add(Me.btnSubButon)
        Me.pnlActions.Controls.Add(Me.btnLaCursor)
        Me.pnlActions.Controls.Add(Me.lblHint)
        Me.pnlActions.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlActions.Height = 44
        Me.pnlActions.Padding = New System.Windows.Forms.Padding(6)
        Me.pnlActions.Name = "pnlActions"
        '
        'btnSubButon
        '
        Me.btnSubButon.AutoSize = True
        Me.btnSubButon.Text = "Deschide sub buton ▾"
        Me.btnSubButon.UseVisualStyleBackColor = True
        Me.btnSubButon.Name = "btnSubButon"
        '
        'btnLaCursor
        '
        Me.btnLaCursor.AutoSize = True
        Me.btnLaCursor.Text = "Deschide la cursor"
        Me.btnLaCursor.UseVisualStyleBackColor = True
        Me.btnLaCursor.Name = "btnLaCursor"
        '
        'lblHint
        '
        Me.lblHint.AutoSize = True
        Me.lblHint.Margin = New System.Windows.Forms.Padding(12, 9, 3, 0)
        Me.lblHint.Text = "…sau clic dreapta oriunde pe fereastră. Taste: ↑ ↓ Home End Enter Esc, literă de acces."
        Me.lblHint.Name = "lblHint"
        '
        'lstLog
        '
        Me.lstLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstLog.IntegralHeight = False
        Me.lstLog.Name = "lstLog"
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
        'PopupPlaygroundForm
        '
        ' AcceptButton lipsește INTENȚIONAT: Enter trebuie să ajungă la meniu, nu la «Pass».
        Me.CancelButton = Me.btnFail
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(760, 480)
        ' Ordine INVERSĂ de andocare (regula casei): Fill întâi, apoi benzile Top/Bottom —
        ' ultimul Top adăugat ajunge cel mai sus.
        Me.Controls.Add(Me.lstLog)
        Me.Controls.Add(Me.pnlActions)
        Me.Controls.Add(Me.pnlOptions)
        Me.Controls.Add(Me.pnlTop)
        Me.Controls.Add(Me.pnlButtons)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "CustomPopup — meniu tematizat (playground)"
        Me.Name = "PopupPlaygroundForm"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        Me.pnlOptions.ResumeLayout(False)
        Me.pnlOptions.PerformLayout()
        Me.pnlActions.ResumeLayout(False)
        Me.pnlActions.PerformLayout()
        Me.pnlButtons.ResumeLayout(False)
        Me.pnlButtons.PerformLayout()
        Me.ResumeLayout(False)
    End Sub

End Class
