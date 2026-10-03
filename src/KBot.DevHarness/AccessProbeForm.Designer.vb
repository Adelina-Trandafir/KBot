Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AccessProbeForm
    Inherits Global.KBot.Theming.KBotThemedForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        tlyRadacina = New KBotTableLayoutPanel()
        tlyCampuri = New KBotTableLayoutPanel()
        lblCale = New Label()
        txtCale = New TextBox()
        lblAn = New Label()
        txtAn = New TextBox()
        lblSursa = New Label()
        txtSursa = New TextBox()
        btnCiteste = New Button()
        txtRezultat = New TextBox()
        tlyJos = New KBotTableLayoutPanel()
        lblStare = New Label()
        btnInchideDirect = New Button()
        tlyRadacina.SuspendLayout()
        tlyCampuri.SuspendLayout()
        tlyJos.SuspendLayout()
        SuspendLayout()
        '
        ' tlyRadacina
        '
        tlyRadacina.AutoFitToTheme = False
        tlyRadacina.ColumnCount = 1
        tlyRadacina.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyRadacina.Controls.Add(tlyCampuri, 0, 0)
        tlyRadacina.Controls.Add(txtRezultat, 0, 1)
        tlyRadacina.Controls.Add(tlyJos, 0, 2)
        tlyRadacina.Dock = DockStyle.Fill
        tlyRadacina.Location = New Point(0, 0)
        tlyRadacina.Margin = New Padding(0)
        tlyRadacina.Name = "tlyRadacina"
        tlyRadacina.Padding = New Padding(12)
        tlyRadacina.RowCount = 3
        tlyRadacina.RowStyles.Add(New RowStyle())
        tlyRadacina.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyRadacina.RowStyles.Add(New RowStyle())
        tlyRadacina.Size = New Size(860, 480)
        tlyRadacina.TabIndex = 0
        '
        ' tlyCampuri
        '
        tlyCampuri.AutoFitToTheme = False
        tlyCampuri.AutoSize = True
        tlyCampuri.ColumnCount = 7
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70F))
        tlyCampuri.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tlyCampuri.Controls.Add(lblCale, 0, 0)
        tlyCampuri.Controls.Add(txtCale, 1, 0)
        tlyCampuri.Controls.Add(lblAn, 2, 0)
        tlyCampuri.Controls.Add(txtAn, 3, 0)
        tlyCampuri.Controls.Add(lblSursa, 4, 0)
        tlyCampuri.Controls.Add(txtSursa, 5, 0)
        tlyCampuri.Controls.Add(btnCiteste, 6, 0)
        tlyCampuri.Dock = DockStyle.Top
        tlyCampuri.Location = New Point(12, 12)
        tlyCampuri.Margin = New Padding(0, 0, 0, 10)
        tlyCampuri.Name = "tlyCampuri"
        tlyCampuri.RowCount = 1
        tlyCampuri.RowStyles.Add(New RowStyle())
        tlyCampuri.Size = New Size(836, 30)
        tlyCampuri.TabIndex = 0
        '
        ' lblCale
        '
        lblCale.Anchor = AnchorStyles.Left
        lblCale.AutoSize = True
        lblCale.Location = New Point(3, 7)
        lblCale.Name = "lblCale"
        lblCale.Size = New Size(62, 15)
        lblCale.TabIndex = 0
        lblCale.Text = "cale.accdb"
        '
        ' txtCale
        '
        txtCale.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtCale.Location = New Point(71, 3)
        txtCale.Name = "txtCale"
        txtCale.Size = New Size(400, 23)
        txtCale.TabIndex = 1
        '
        ' lblAn
        '
        lblAn.Anchor = AnchorStyles.Left
        lblAn.AutoSize = True
        lblAn.Location = New Point(477, 7)
        lblAn.Name = "lblAn"
        lblAn.Size = New Size(21, 15)
        lblAn.TabIndex = 2
        lblAn.Text = "An"
        '
        ' txtAn
        '
        txtAn.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtAn.Location = New Point(504, 3)
        txtAn.MaxLength = 4
        txtAn.Name = "txtAn"
        txtAn.PlaceholderText = "toți"
        txtAn.Size = New Size(64, 23)
        txtAn.TabIndex = 2
        '
        ' lblSursa
        '
        lblSursa.Anchor = AnchorStyles.Left
        lblSursa.AutoSize = True
        lblSursa.Location = New Point(574, 7)
        lblSursa.Name = "lblSursa"
        lblSursa.Size = New Size(37, 15)
        lblSursa.TabIndex = 4
        lblSursa.Text = "Sursa"
        '
        ' txtSursa
        '
        txtSursa.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtSursa.Location = New Point(617, 3)
        txtSursa.MaxLength = 3
        txtSursa.Name = "txtSursa"
        txtSursa.PlaceholderText = "toate"
        txtSursa.Size = New Size(64, 23)
        txtSursa.TabIndex = 3
        '
        ' btnCiteste
        '
        btnCiteste.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        btnCiteste.Location = New Point(687, 3)
        btnCiteste.Name = "btnCiteste"
        btnCiteste.Size = New Size(146, 24)
        btnCiteste.TabIndex = 4
        btnCiteste.Text = "Încarcă și citește"
        btnCiteste.UseVisualStyleBackColor = True
        '
        ' txtRezultat
        '
        txtRezultat.Dock = DockStyle.Fill
        txtRezultat.Font = New Font("Consolas", 9.5F)
        txtRezultat.Location = New Point(15, 55)
        txtRezultat.Margin = New Padding(0, 0, 0, 10)
        txtRezultat.Multiline = True
        txtRezultat.Name = "txtRezultat"
        txtRezultat.ReadOnly = True
        txtRezultat.ScrollBars = ScrollBars.Both
        txtRezultat.Size = New Size(830, 360)
        txtRezultat.TabIndex = 1
        txtRezultat.WordWrap = False
        '
        ' tlyJos
        '
        tlyJos.AutoFitToTheme = False
        tlyJos.AutoSize = True
        tlyJos.ColumnCount = 2
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyJos.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        tlyJos.Controls.Add(lblStare, 0, 0)
        tlyJos.Controls.Add(btnInchideDirect, 1, 0)
        tlyJos.Dock = DockStyle.Top
        tlyJos.Location = New Point(12, 425)
        tlyJos.Margin = New Padding(0)
        tlyJos.Name = "tlyJos"
        tlyJos.RowCount = 1
        tlyJos.RowStyles.Add(New RowStyle())
        tlyJos.Size = New Size(836, 40)
        tlyJos.TabIndex = 2
        '
        ' lblStare
        '
        lblStare.Anchor = AnchorStyles.Left
        lblStare.AutoSize = True
        lblStare.Location = New Point(3, 12)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(10, 15)
        lblStare.TabIndex = 0
        lblStare.Text = " "
        '
        ' btnInchideDirect
        '
        btnInchideDirect.AutoSize = True
        btnInchideDirect.Location = New Point(683, 3)
        btnInchideDirect.MinimumSize = New Size(150, 34)
        btnInchideDirect.Name = "btnInchideDirect"
        btnInchideDirect.Size = New Size(150, 34)
        btnInchideDirect.TabIndex = 5
        btnInchideDirect.Text = "Închide direct"
        btnInchideDirect.UseVisualStyleBackColor = True
        '
        ' AccessProbeForm
        '
        AutoScaleDimensions = New SizeF(96F, 96F)
        AutoScaleMode = AutoScaleMode.Dpi
        ClientSize = New Size(860, 480)
        Controls.Add(tlyRadacina)
        MinimumSize = New Size(640, 360)
        Name = "AccessProbeForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Probă Access — citește cale.accdb"
        tlyRadacina.ResumeLayout(False)
        tlyRadacina.PerformLayout()
        tlyCampuri.ResumeLayout(False)
        tlyCampuri.PerformLayout()
        tlyJos.ResumeLayout(False)
        tlyJos.PerformLayout()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tlyRadacina As KBotTableLayoutPanel
    Friend WithEvents tlyCampuri As KBotTableLayoutPanel
    Friend WithEvents lblCale As Label
    Friend WithEvents txtCale As TextBox
    Friend WithEvents lblAn As Label
    Friend WithEvents txtAn As TextBox
    Friend WithEvents lblSursa As Label
    Friend WithEvents txtSursa As TextBox
    Friend WithEvents btnCiteste As Button
    Friend WithEvents txtRezultat As TextBox
    Friend WithEvents tlyJos As KBotTableLayoutPanel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnInchideDirect As Button
End Class
