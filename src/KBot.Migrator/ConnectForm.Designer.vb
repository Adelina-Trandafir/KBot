<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConnectForm
    Inherits KBot.Theming.KBotThemedForm

    ' Toate controalele sunt declarate AICI, ca formularul să se randeze în designerul VS
    ' (docs/kbot-forms-ui-convention.md). Nimic nu se construiește în cod.
    Private components As System.ComponentModel.IContainer

    Friend WithEvents lblTitlu As System.Windows.Forms.Label
    Friend WithEvents lblServer As System.Windows.Forms.Label
    Friend WithEvents txtServer As System.Windows.Forms.TextBox
    Friend WithEvents lblCheie As System.Windows.Forms.Label
    Friend WithEvents txtCheie As System.Windows.Forms.TextBox
    Friend WithEvents btnConecteaza As System.Windows.Forms.Button
    Friend WithEvents lblBaze As System.Windows.Forms.Label
    Friend WithEvents lstBaze As System.Windows.Forms.ListBox
    Friend WithEvents lblStare As System.Windows.Forms.Label
    Friend WithEvents btnContinua As System.Windows.Forms.Button
    Friend WithEvents btnRenunta As System.Windows.Forms.Button
    Friend WithEvents sfat As KBot.Controls.KBotToolTip

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
        Me.lblTitlu = New System.Windows.Forms.Label()
        Me.lblServer = New System.Windows.Forms.Label()
        Me.txtServer = New System.Windows.Forms.TextBox()
        Me.lblCheie = New System.Windows.Forms.Label()
        Me.txtCheie = New System.Windows.Forms.TextBox()
        Me.btnConecteaza = New System.Windows.Forms.Button()
        Me.lblBaze = New System.Windows.Forms.Label()
        Me.lstBaze = New System.Windows.Forms.ListBox()
        Me.lblStare = New System.Windows.Forms.Label()
        Me.btnContinua = New System.Windows.Forms.Button()
        Me.btnRenunta = New System.Windows.Forms.Button()
        Me.sfat = New KBot.Controls.KBotToolTip(Me.components)
        Me.SuspendLayout()
        '
        'lblTitlu
        '
        Me.lblTitlu.AutoSize = True
        Me.lblTitlu.Font = New System.Drawing.Font("Segoe UI Semibold", 12.0!)
        Me.lblTitlu.Location = New System.Drawing.Point(16, 14)
        Me.lblTitlu.Name = "lblTitlu"
        Me.lblTitlu.Size = New System.Drawing.Size(300, 21)
        Me.lblTitlu.TabIndex = 0
        Me.lblTitlu.Text = "Conectare la serverul de migrare"
        '
        'lblServer
        '
        Me.lblServer.AutoSize = True
        Me.lblServer.Location = New System.Drawing.Point(16, 52)
        Me.lblServer.Name = "lblServer"
        Me.lblServer.Size = New System.Drawing.Size(48, 15)
        Me.lblServer.TabIndex = 1
        Me.lblServer.Text = "Server:"
        '
        'txtServer
        '
        Me.txtServer.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtServer.Location = New System.Drawing.Point(130, 49)
        Me.txtServer.Name = "txtServer"
        Me.txtServer.Size = New System.Drawing.Size(340, 23)
        Me.txtServer.TabIndex = 2
        '
        'lblCheie
        '
        Me.lblCheie.AutoSize = True
        Me.lblCheie.Location = New System.Drawing.Point(16, 83)
        Me.lblCheie.Name = "lblCheie"
        Me.lblCheie.Size = New System.Drawing.Size(108, 15)
        Me.lblCheie.TabIndex = 3
        Me.lblCheie.Text = "Cheie API (X-Api-Key):"
        '
        'txtCheie
        '
        Me.txtCheie.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.txtCheie.Location = New System.Drawing.Point(130, 80)
        Me.txtCheie.Name = "txtCheie"
        Me.txtCheie.Size = New System.Drawing.Size(340, 23)
        Me.txtCheie.TabIndex = 4
        Me.txtCheie.UseSystemPasswordChar = True
        '
        'btnConecteaza
        '
        Me.btnConecteaza.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnConecteaza.Location = New System.Drawing.Point(130, 113)
        Me.btnConecteaza.Name = "btnConecteaza"
        Me.btnConecteaza.Size = New System.Drawing.Size(140, 28)
        Me.btnConecteaza.TabIndex = 5
        Me.btnConecteaza.Text = "Conectează"
        Me.btnConecteaza.UseVisualStyleBackColor = True
        '
        'lblBaze
        '
        Me.lblBaze.AutoSize = True
        Me.lblBaze.Location = New System.Drawing.Point(16, 156)
        Me.lblBaze.Name = "lblBaze"
        Me.lblBaze.Size = New System.Drawing.Size(180, 15)
        Me.lblBaze.TabIndex = 6
        Me.lblBaze.Text = "Baze de unitate găsite pe MariaDB:"
        '
        'lstBaze
        '
        Me.lstBaze.Anchor = CType(System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lstBaze.FormattingEnabled = True
        Me.lstBaze.ItemHeight = 15
        Me.lstBaze.Location = New System.Drawing.Point(16, 176)
        Me.lstBaze.Name = "lstBaze"
        Me.lstBaze.Size = New System.Drawing.Size(454, 154)
        Me.lstBaze.TabIndex = 7
        '
        'lblStare
        '
        Me.lblStare.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.lblStare.Location = New System.Drawing.Point(16, 340)
        Me.lblStare.Name = "lblStare"
        Me.lblStare.Size = New System.Drawing.Size(454, 34)
        Me.lblStare.TabIndex = 8
        Me.lblStare.Text = "Completează adresa serverului și cheia API, apoi apasă «Conectează»."
        '
        'btnContinua
        '
        Me.btnContinua.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnContinua.Enabled = False
        Me.btnContinua.Location = New System.Drawing.Point(250, 381)
        Me.btnContinua.Name = "btnContinua"
        Me.btnContinua.Size = New System.Drawing.Size(110, 30)
        Me.btnContinua.TabIndex = 9
        Me.btnContinua.Text = "Continuă"
        Me.btnContinua.UseVisualStyleBackColor = True
        '
        'btnRenunta
        '
        Me.btnRenunta.Anchor = CType(System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right, System.Windows.Forms.AnchorStyles)
        Me.btnRenunta.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.btnRenunta.Location = New System.Drawing.Point(366, 381)
        Me.btnRenunta.Name = "btnRenunta"
        Me.btnRenunta.Size = New System.Drawing.Size(104, 30)
        Me.btnRenunta.TabIndex = 10
        Me.btnRenunta.Text = "Renunță"
        Me.btnRenunta.UseVisualStyleBackColor = True
        '
        'sfat
        '
        Me.sfat.SetToolTipHeader(Me.txtServer, "Adresa serverului")
        Me.sfat.SetToolTipText(Me.txtServer, "Trebuie să înceapă cu «https://». Implicit: adresa din ApiOptions.")
        Me.sfat.SetToolTipHeader(Me.txtCheie, "Cheia API")
        Me.sfat.SetToolTipText(Me.txtCheie, "Rutele de migrare sunt păzite cu X-Api-Key, nu cu token bearer." & Global.Microsoft.VisualBasic.ChrW(10) & "Se poate preîncărca din variabila de mediu KBOT_SEED_API_KEY.")
        Me.sfat.SetToolTipHeader(Me.lstBaze, "Bazele de pe MariaDB")
        Me.sfat.SetToolTipText(Me.lstBaze, "Cele care nu au toate cele 16 tabele FX_ sunt marcate." & Global.Microsoft.VisualBasic.ChrW(10) & "Migrarea NU creează tabele — schema se instalează separat.")
        '
        'ConnectForm
        '
        Me.AcceptButton = Me.btnConecteaza
        Me.CancelButton = Me.btnRenunta
        Me.ClientSize = New System.Drawing.Size(486, 425)
        Me.Controls.Add(Me.btnRenunta)
        Me.Controls.Add(Me.btnContinua)
        Me.Controls.Add(Me.lblStare)
        Me.Controls.Add(Me.lstBaze)
        Me.Controls.Add(Me.lblBaze)
        Me.Controls.Add(Me.btnConecteaza)
        Me.Controls.Add(Me.txtCheie)
        Me.Controls.Add(Me.lblCheie)
        Me.Controls.Add(Me.txtServer)
        Me.Controls.Add(Me.lblServer)
        Me.Controls.Add(Me.lblTitlu)
        Me.MinimumSize = New System.Drawing.Size(502, 464)
        Me.Name = "ConnectForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Migrare FX — conectare"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

End Class
