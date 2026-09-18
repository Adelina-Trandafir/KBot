<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormFitProbeDialog
    Inherits KBot.Theming.KBotShellForm

    ' The window the bench opens to prove WHERE a themed form lands and HOW BIG it comes up:
    ' one card, a few readout labels, a close button. Controls declared here (house rule).

    Friend WithEvents pnlCard As System.Windows.Forms.Panel
    Friend WithEvents tlyCard As Global.KBot.Controls.KBotTableLayoutPanel
    Friend WithEvents lblTitlu As System.Windows.Forms.Label
    Friend WithEvents lblEcran As System.Windows.Forms.Label
    Friend WithEvents lblMarime As System.Windows.Forms.Label
    Friend WithEvents lblPozitie As System.Windows.Forms.Label
    Friend WithEvents btnInchide As System.Windows.Forms.Button

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlCard = New System.Windows.Forms.Panel()
        Me.tlyCard = New Global.KBot.Controls.KBotTableLayoutPanel()
        Me.lblTitlu = New System.Windows.Forms.Label()
        Me.lblEcran = New System.Windows.Forms.Label()
        Me.lblMarime = New System.Windows.Forms.Label()
        Me.lblPozitie = New System.Windows.Forms.Label()
        Me.btnInchide = New System.Windows.Forms.Button()
        Me.pnlCard.SuspendLayout()
        Me.tlyCard.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlCard
        '
        Me.pnlCard.Controls.Add(Me.tlyCard)
        Me.pnlCard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCard.Padding = New System.Windows.Forms.Padding(12)
        Me.pnlCard.Tag = "Card"
        Me.pnlCard.Name = "pnlCard"
        '
        'tlyCard
        '
        Me.tlyCard.ColumnCount = 1
        Me.tlyCard.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0F))
        Me.tlyCard.Controls.Add(Me.lblTitlu, 0, 0)
        Me.tlyCard.Controls.Add(Me.lblEcran, 0, 1)
        Me.tlyCard.Controls.Add(Me.lblMarime, 0, 2)
        Me.tlyCard.Controls.Add(Me.lblPozitie, 0, 3)
        Me.tlyCard.Controls.Add(Me.btnInchide, 0, 4)
        Me.tlyCard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlyCard.RowCount = 5
        ' Rows authored SHORT on purpose (28px for a 9pt label): the point of the probe is to see
        ' the table grow them under Modern / a larger text size and shrink them back.
        Me.tlyCard.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0F))
        Me.tlyCard.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0F))
        Me.tlyCard.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0F))
        Me.tlyCard.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0F))
        Me.tlyCard.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40.0F))
        Me.tlyCard.Name = "tlyCard"
        '
        'lblTitlu
        '
        Me.lblTitlu.AutoSize = True
        Me.lblTitlu.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblTitlu.Text = "Fereastră de probă"
        Me.lblTitlu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblTitlu.Name = "lblTitlu"
        '
        'lblEcran
        '
        Me.lblEcran.AutoSize = True
        Me.lblEcran.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblEcran.Text = "ecran: —"
        Me.lblEcran.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblEcran.Name = "lblEcran"
        '
        'lblMarime
        '
        Me.lblMarime.AutoSize = True
        Me.lblMarime.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblMarime.Text = "mărime: —"
        Me.lblMarime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblMarime.Name = "lblMarime"
        '
        'lblPozitie
        '
        Me.lblPozitie.AutoSize = True
        Me.lblPozitie.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblPozitie.Text = "poziție: —"
        Me.lblPozitie.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.lblPozitie.Name = "lblPozitie"
        '
        'btnInchide
        '
        Me.btnInchide.AutoSize = True
        Me.btnInchide.Anchor = System.Windows.Forms.AnchorStyles.Right
        Me.btnInchide.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnInchide.Text = "Închide"
        Me.btnInchide.UseVisualStyleBackColor = True
        Me.btnInchide.Name = "btnInchide"
        '
        'FormFitProbeDialog
        '
        Me.AcceptButton = Me.btnInchide
        Me.CancelButton = Me.btnInchide
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96F, 96F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(420, 200)
        Me.Controls.Add(Me.pnlCard)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Probă — unde apar și cât de mare sunt"
        Me.Name = "FormFitProbeDialog"
        Me.tlyCard.ResumeLayout(False)
        Me.tlyCard.PerformLayout()
        Me.pnlCard.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

End Class
