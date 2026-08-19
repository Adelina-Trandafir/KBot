<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MigratorForm
    Inherits KBot.Theming.KBotThemedForm

    ' Toate controalele sunt declarate AICI, ca formularul să se randeze în designerul VS
    ' (docs/kbot-forms-ui-convention.md). Nimic nu se construiește în cod.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents pnlSurse As System.Windows.Forms.Panel
    Friend WithEvents lblFolder As System.Windows.Forms.Label
    Friend WithEvents txtFolder As System.Windows.Forms.TextBox
    Friend WithEvents btnRasfoire As System.Windows.Forms.Button
    Friend WithEvents btnIncarca As System.Windows.Forms.Button
    Friend WithEvents lblCheie As System.Windows.Forms.Label
    Friend WithEvents txtCheie As System.Windows.Forms.TextBox
    Friend WithEvents lblSurse As System.Windows.Forms.Label
    Friend WithEvents clbDc As System.Windows.Forms.CheckedListBox

    Friend WithEvents pnlActiuni As System.Windows.Forms.Panel
    Friend WithEvents btnVerifica As System.Windows.Forms.Button
    Friend WithEvents btnTransfera As System.Windows.Forms.Button
    Friend WithEvents chkForteaza As System.Windows.Forms.CheckBox
    Friend WithEvents lblStare As System.Windows.Forms.Label

    Friend WithEvents dgvRezultate As System.Windows.Forms.DataGridView
    Friend WithEvents txtJurnal As System.Windows.Forms.TextBox
    Friend WithEvents dlgFolder As System.Windows.Forms.FolderBrowserDialog

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

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.pnlSurse = New System.Windows.Forms.Panel()
        Me.lblFolder = New System.Windows.Forms.Label()
        Me.txtFolder = New System.Windows.Forms.TextBox()
        Me.btnRasfoire = New System.Windows.Forms.Button()
        Me.btnIncarca = New System.Windows.Forms.Button()
        Me.lblCheie = New System.Windows.Forms.Label()
        Me.txtCheie = New System.Windows.Forms.TextBox()
        Me.lblSurse = New System.Windows.Forms.Label()
        Me.clbDc = New System.Windows.Forms.CheckedListBox()
        Me.pnlActiuni = New System.Windows.Forms.Panel()
        Me.btnVerifica = New System.Windows.Forms.Button()
        Me.btnTransfera = New System.Windows.Forms.Button()
        Me.chkForteaza = New System.Windows.Forms.CheckBox()
        Me.lblStare = New System.Windows.Forms.Label()
        Me.dgvRezultate = New System.Windows.Forms.DataGridView()
        Me.txtJurnal = New System.Windows.Forms.TextBox()
        Me.dlgFolder = New System.Windows.Forms.FolderBrowserDialog()
        Me.pnlSurse.SuspendLayout()
        Me.pnlActiuni.SuspendLayout()
        CType(Me.dgvRezultate, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblFolder
        '
        Me.lblFolder.AutoSize = True
        Me.lblFolder.Location = New System.Drawing.Point(12, 15)
        Me.lblFolder.Name = "lblFolder"
        Me.lblFolder.Size = New System.Drawing.Size(96, 15)
        Me.lblFolder.TabIndex = 0
        Me.lblFolder.Text = "Folder artefacte:"
        '
        'txtFolder
        '
        Me.txtFolder.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtFolder.Location = New System.Drawing.Point(140, 12)
        Me.txtFolder.Name = "txtFolder"
        Me.txtFolder.Size = New System.Drawing.Size(520, 23)
        Me.txtFolder.TabIndex = 1
        '
        'btnRasfoire
        '
        Me.btnRasfoire.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnRasfoire.Location = New System.Drawing.Point(666, 11)
        Me.btnRasfoire.Name = "btnRasfoire"
        Me.btnRasfoire.Size = New System.Drawing.Size(90, 25)
        Me.btnRasfoire.TabIndex = 2
        Me.btnRasfoire.Text = "Răsfoiește…"
        Me.btnRasfoire.UseVisualStyleBackColor = True
        '
        'btnIncarca
        '
        Me.btnIncarca.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnIncarca.Location = New System.Drawing.Point(762, 11)
        Me.btnIncarca.Name = "btnIncarca"
        Me.btnIncarca.Size = New System.Drawing.Size(110, 25)
        Me.btnIncarca.TabIndex = 3
        Me.btnIncarca.Text = "Încarcă DC-urile"
        Me.btnIncarca.UseVisualStyleBackColor = True
        '
        'lblCheie
        '
        Me.lblCheie.AutoSize = True
        Me.lblCheie.Location = New System.Drawing.Point(12, 46)
        Me.lblCheie.Name = "lblCheie"
        Me.lblCheie.Size = New System.Drawing.Size(107, 15)
        Me.lblCheie.TabIndex = 4
        Me.lblCheie.Text = "Cheie API (X-Api-Key):"
        '
        'txtCheie
        '
        Me.txtCheie.Location = New System.Drawing.Point(140, 43)
        Me.txtCheie.Name = "txtCheie"
        Me.txtCheie.Size = New System.Drawing.Size(300, 23)
        Me.txtCheie.TabIndex = 5
        Me.txtCheie.UseSystemPasswordChar = True
        '
        'lblSurse
        '
        Me.lblSurse.AutoSize = True
        Me.lblSurse.Location = New System.Drawing.Point(12, 77)
        Me.lblSurse.Name = "lblSurse"
        Me.lblSurse.Size = New System.Drawing.Size(122, 15)
        Me.lblSurse.TabIndex = 6
        Me.lblSurse.Text = "Baze de date (DC):"
        '
        'clbDc
        '
        Me.clbDc.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.clbDc.CheckOnClick = True
        Me.clbDc.IntegralHeight = False
        Me.clbDc.Location = New System.Drawing.Point(140, 74)
        Me.clbDc.MultiColumn = True
        Me.clbDc.Name = "clbDc"
        Me.clbDc.Size = New System.Drawing.Size(732, 84)
        Me.clbDc.TabIndex = 7
        '
        'pnlSurse
        '
        Me.pnlSurse.Controls.Add(Me.clbDc)
        Me.pnlSurse.Controls.Add(Me.lblSurse)
        Me.pnlSurse.Controls.Add(Me.txtCheie)
        Me.pnlSurse.Controls.Add(Me.lblCheie)
        Me.pnlSurse.Controls.Add(Me.btnIncarca)
        Me.pnlSurse.Controls.Add(Me.btnRasfoire)
        Me.pnlSurse.Controls.Add(Me.txtFolder)
        Me.pnlSurse.Controls.Add(Me.lblFolder)
        Me.pnlSurse.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSurse.Location = New System.Drawing.Point(0, 0)
        Me.pnlSurse.Name = "pnlSurse"
        Me.pnlSurse.Size = New System.Drawing.Size(884, 168)
        Me.pnlSurse.TabIndex = 0
        '
        'btnVerifica
        '
        Me.btnVerifica.Location = New System.Drawing.Point(12, 8)
        Me.btnVerifica.Name = "btnVerifica"
        Me.btnVerifica.Size = New System.Drawing.Size(140, 28)
        Me.btnVerifica.TabIndex = 0
        Me.btnVerifica.Text = "Verificare"
        Me.btnVerifica.UseVisualStyleBackColor = True
        '
        'btnTransfera
        '
        Me.btnTransfera.Enabled = False
        Me.btnTransfera.Location = New System.Drawing.Point(158, 8)
        Me.btnTransfera.Name = "btnTransfera"
        Me.btnTransfera.Size = New System.Drawing.Size(140, 28)
        Me.btnTransfera.TabIndex = 1
        Me.btnTransfera.Text = "Transfer"
        Me.btnTransfera.UseVisualStyleBackColor = True
        '
        'chkForteaza
        '
        Me.chkForteaza.AutoSize = True
        Me.chkForteaza.Location = New System.Drawing.Point(310, 14)
        Me.chkForteaza.Name = "chkForteaza"
        Me.chkForteaza.Size = New System.Drawing.Size(232, 19)
        Me.chkForteaza.TabIndex = 2
        Me.chkForteaza.Text = "Permite transferul deși verificarea nu e curată"
        Me.chkForteaza.UseVisualStyleBackColor = True
        '
        'lblStare
        '
        Me.lblStare.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblStare.Location = New System.Drawing.Point(560, 14)
        Me.lblStare.Name = "lblStare"
        Me.lblStare.Size = New System.Drawing.Size(312, 19)
        Me.lblStare.TabIndex = 3
        Me.lblStare.Text = "Alege folderul VBA_ARTEFACTE și încarcă DC-urile."
        Me.lblStare.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'pnlActiuni
        '
        Me.pnlActiuni.Controls.Add(Me.lblStare)
        Me.pnlActiuni.Controls.Add(Me.chkForteaza)
        Me.pnlActiuni.Controls.Add(Me.btnTransfera)
        Me.pnlActiuni.Controls.Add(Me.btnVerifica)
        Me.pnlActiuni.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlActiuni.Location = New System.Drawing.Point(0, 168)
        Me.pnlActiuni.Name = "pnlActiuni"
        Me.pnlActiuni.Size = New System.Drawing.Size(884, 46)
        Me.pnlActiuni.TabIndex = 1
        '
        'dgvRezultate
        '
        Me.dgvRezultate.AllowUserToAddRows = False
        Me.dgvRezultate.AllowUserToDeleteRows = False
        Me.dgvRezultate.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvRezultate.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvRezultate.Dock = System.Windows.Forms.DockStyle.Top
        Me.dgvRezultate.Location = New System.Drawing.Point(0, 214)
        Me.dgvRezultate.Name = "dgvRezultate"
        Me.dgvRezultate.ReadOnly = True
        Me.dgvRezultate.RowHeadersVisible = False
        Me.dgvRezultate.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvRezultate.Size = New System.Drawing.Size(884, 220)
        Me.dgvRezultate.TabIndex = 2
        '
        'txtJurnal
        '
        Me.txtJurnal.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtJurnal.Location = New System.Drawing.Point(0, 434)
        Me.txtJurnal.Multiline = True
        Me.txtJurnal.Name = "txtJurnal"
        Me.txtJurnal.ReadOnly = True
        Me.txtJurnal.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtJurnal.Size = New System.Drawing.Size(884, 227)
        Me.txtJurnal.TabIndex = 3
        Me.txtJurnal.WordWrap = False
        '
        'dlgFolder
        '
        Me.dlgFolder.Description = "Alege folderul VBA_ARTEFACTE scris de exportul din Access."
        Me.dlgFolder.UseDescriptionForTitle = True
        '
        'MigratorForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(884, 661)
        ' Ordine INVERSĂ de andocare: Fill întâi, apoi Top-urile — ultimul Top adăugat
        ' andochează cel mai sus (regula casei pentru panourile-card).
        Me.Controls.Add(Me.txtJurnal)
        Me.Controls.Add(Me.dgvRezultate)
        Me.Controls.Add(Me.pnlActiuni)
        Me.Controls.Add(Me.pnlSurse)
        Me.MinimumSize = New System.Drawing.Size(760, 520)
        Me.Name = "MigratorForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Migrare FX — Access → MariaDB"
        Me.pnlSurse.ResumeLayout(False)
        Me.pnlSurse.PerformLayout()
        Me.pnlActiuni.ResumeLayout(False)
        Me.pnlActiuni.PerformLayout()
        CType(Me.dgvRezultate, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

End Class
