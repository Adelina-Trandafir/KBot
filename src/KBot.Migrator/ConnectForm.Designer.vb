<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConnectForm
    Inherits KBot.Theming.KBotThemedForm

    ' Toate controalele sunt declarate AICI, ca formularul sa se randeze in designerul VS
    ' (docs/kbot-forms-ui-convention.md). Nimic nu se construieste in cod.
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
        components = New ComponentModel.Container()
        lblTitlu = New Label()
        lblServer = New Label()
        txtServer = New TextBox()
        lblCheie = New Label()
        txtCheie = New TextBox()
        btnConecteaza = New Button()
        lblBaze = New Label()
        lstBaze = New ListBox()
        lblStare = New Label()
        btnContinua = New Button()
        btnRenunta = New Button()
        sfat = New KBot.Controls.KBotToolTip(components)
        TableLayoutPanel1 = New TableLayoutPanel()
        TableLayoutPanel1.SuspendLayout()
        SuspendLayout()
        ' 
        ' lblTitlu
        ' 
        lblTitlu.AutoSize = True
        TableLayoutPanel1.SetColumnSpan(lblTitlu, 2)
        lblTitlu.Dock = DockStyle.Fill
        lblTitlu.Font = New Font("Segoe UI Semibold", 12F)
        lblTitlu.Location = New Point(3, 0)
        lblTitlu.Name = "lblTitlu"
        lblTitlu.Size = New Size(715, 42)
        lblTitlu.TabIndex = 0
        lblTitlu.Text = "Conectare la serverul de migrare"
        lblTitlu.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblServer
        ' 
        lblServer.AutoSize = True
        lblServer.Dock = DockStyle.Fill
        lblServer.Location = New Point(10, 50)
        lblServer.Margin = New Padding(10, 0, 0, 0)
        lblServer.Name = "lblServer"
        lblServer.Size = New Size(186, 40)
        lblServer.TabIndex = 1
        lblServer.Text = "Server:"
        lblServer.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtServer
        ' 
        txtServer.Dock = DockStyle.Fill
        txtServer.Location = New Point(199, 53)
        txtServer.Name = "txtServer"
        txtServer.Size = New Size(519, 31)
        txtServer.TabIndex = 2
        sfat.SetToolTipHeader(txtServer, "Adresa serverului")
        sfat.SetToolTipText(txtServer, "Trebuie să înceapă cu «https://». Implicit: adresa din ApiOptions.")
        ' 
        ' lblCheie
        ' 
        lblCheie.AutoSize = True
        lblCheie.Dock = DockStyle.Fill
        lblCheie.Location = New Point(10, 90)
        lblCheie.Margin = New Padding(10, 0, 0, 0)
        lblCheie.Name = "lblCheie"
        lblCheie.Size = New Size(186, 40)
        lblCheie.TabIndex = 3
        lblCheie.Text = "Cheie API (X-Api-Key):"
        lblCheie.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCheie
        ' 
        txtCheie.Dock = DockStyle.Fill
        txtCheie.Location = New Point(199, 93)
        txtCheie.Name = "txtCheie"
        txtCheie.Size = New Size(519, 31)
        txtCheie.TabIndex = 4
        sfat.SetToolTipHeader(txtCheie, "Cheia API")
        sfat.SetToolTipText(txtCheie, "Rutele de migrare sunt păzite cu X-Api-Key, nu cu token bearer." & vbLf & "Se poate preîncărca din variabila de mediu KBOT_SEED_API_KEY.")
        txtCheie.UseSystemPasswordChar = True
        ' 
        ' btnConecteaza
        ' 
        btnConecteaza.Dock = DockStyle.Right
        btnConecteaza.Location = New Point(578, 133)
        btnConecteaza.Name = "btnConecteaza"
        btnConecteaza.Size = New Size(140, 34)
        btnConecteaza.TabIndex = 5
        btnConecteaza.Text = "Conectează"
        btnConecteaza.UseVisualStyleBackColor = True
        ' 
        ' lblBaze
        ' 
        lblBaze.AutoSize = True
        TableLayoutPanel1.SetColumnSpan(lblBaze, 2)
        lblBaze.Dock = DockStyle.Fill
        lblBaze.Location = New Point(3, 178)
        lblBaze.Name = "lblBaze"
        lblBaze.Size = New Size(715, 40)
        lblBaze.TabIndex = 6
        lblBaze.Text = "Baze de unitate găsite pe MariaDB:"
        ' 
        ' lstBaze
        ' 
        TableLayoutPanel1.SetColumnSpan(lstBaze, 2)
        lstBaze.Dock = DockStyle.Fill
        lstBaze.FormattingEnabled = True
        lstBaze.ItemHeight = 25
        lstBaze.Location = New Point(3, 221)
        lstBaze.Name = "lstBaze"
        lstBaze.Size = New Size(715, 248)
        lstBaze.TabIndex = 7
        sfat.SetToolTipHeader(lstBaze, "Bazele de pe MariaDB")
        sfat.SetToolTipText(lstBaze, "Cele care nu au toate tabelele FX_ migrate sunt marcate." & vbLf & "Migrarea NU creează tabele — schema se instalează separat.")
        ' 
        ' lblStare
        ' 
        TableLayoutPanel1.SetColumnSpan(lblStare, 2)
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(0, 472)
        lblStare.Margin = New Padding(0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(721, 20)
        lblStare.TabIndex = 8
        lblStare.Text = "Completează adresa serverului și cheia API, apoi apasă «Conectează»."
        ' 
        ' btnContinua
        ' 
        btnContinua.Dock = DockStyle.Right
        btnContinua.Enabled = False
        btnContinua.Location = New Point(588, 503)
        btnContinua.Name = "btnContinua"
        btnContinua.Size = New Size(130, 38)
        btnContinua.TabIndex = 9
        btnContinua.Text = "Continuă"
        btnContinua.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Left
        btnRenunta.Location = New Point(3, 503)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(130, 38)
        btnRenunta.TabIndex = 10
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 2
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 27.1844654F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 72.81554F))
        TableLayoutPanel1.Controls.Add(lblTitlu, 0, 0)
        TableLayoutPanel1.Controls.Add(btnRenunta, 0, 10)
        TableLayoutPanel1.Controls.Add(lblServer, 0, 2)
        TableLayoutPanel1.Controls.Add(btnContinua, 1, 10)
        TableLayoutPanel1.Controls.Add(lblCheie, 0, 3)
        TableLayoutPanel1.Controls.Add(lblStare, 0, 8)
        TableLayoutPanel1.Controls.Add(txtServer, 1, 2)
        TableLayoutPanel1.Controls.Add(lstBaze, 0, 7)
        TableLayoutPanel1.Controls.Add(txtCheie, 1, 3)
        TableLayoutPanel1.Controls.Add(lblBaze, 0, 6)
        TableLayoutPanel1.Controls.Add(btnConecteaza, 1, 4)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 11
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 8F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 8F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 40F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 8F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 44F))
        TableLayoutPanel1.Size = New Size(721, 544)
        TableLayoutPanel1.TabIndex = 11
        ' 
        ' ConnectForm
        ' 
        AcceptButton = btnConecteaza
        CancelButton = btnRenunta
        ClientSize = New Size(721, 544)
        Controls.Add(TableLayoutPanel1)
        MinimumSize = New Size(502, 464)
        Name = "ConnectForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Migrare FX — conectare"
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel

End Class
