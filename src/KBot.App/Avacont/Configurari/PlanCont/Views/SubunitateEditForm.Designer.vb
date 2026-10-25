Imports KBot.Controls

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SubunitateEditForm
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
        tips = New KBotToolTip(components)
        pnlCard = New Panel()
        tlyCorp = New KBotTableLayoutPanel()
        lblDenumire = New Label()
        txtDenumire = New KBotTextField()
        lblCapitol = New Label()
        txtCapitol = New KBotTextField()
        lblSs = New Label()
        cboSs = New KBotComboBox()
        lblZecimale = New Label()
        cboZecimale = New KBotComboBox()
        chkImplicita = New CheckBox()
        chkActiv = New CheckBox()
        pnlJos = New Panel()
        lblStare = New Label()
        btnSalveaza = New Button()
        btnRenunta = New Button()
        capBar = New KBotCaptionBar()
        pnlCard.SuspendLayout()
        tlyCorp.SuspendLayout()
        pnlJos.SuspendLayout()
        SuspendLayout()
        ' 
        ' pnlCard
        ' 
        pnlCard.Controls.Add(tlyCorp)
        pnlCard.Controls.Add(pnlJos)
        pnlCard.Controls.Add(capBar)
        pnlCard.Dock = DockStyle.Fill
        pnlCard.Location = New Point(2, 2)
        pnlCard.Margin = New Padding(0)
        pnlCard.Name = "pnlCard"
        pnlCard.Size = New Size(996, 696)
        pnlCard.TabIndex = 0
        pnlCard.Tag = "Card"
        ' 
        ' tlyCorp
        ' 
        tlyCorp.ColumnCount = 2
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 255F))
        tlyCorp.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        tlyCorp.Controls.Add(lblDenumire, 0, 0)
        tlyCorp.Controls.Add(txtDenumire, 1, 0)
        tlyCorp.Controls.Add(lblCapitol, 0, 1)
        tlyCorp.Controls.Add(txtCapitol, 1, 1)
        tlyCorp.Controls.Add(lblSs, 0, 2)
        tlyCorp.Controls.Add(cboSs, 1, 2)
        tlyCorp.Controls.Add(lblZecimale, 0, 3)
        tlyCorp.Controls.Add(cboZecimale, 1, 3)
        tlyCorp.Controls.Add(chkImplicita, 1, 4)
        tlyCorp.Controls.Add(chkActiv, 1, 5)
        tlyCorp.Dock = DockStyle.Fill
        tlyCorp.Location = New Point(0, 66)
        tlyCorp.Margin = New Padding(0)
        tlyCorp.Name = "tlyCorp"
        tlyCorp.Padding = New Padding(21, 15, 21, 6)
        tlyCorp.RowCount = 7
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 54F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Absolute, 42F))
        tlyCorp.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        tlyCorp.Size = New Size(996, 546)
        tlyCorp.TabIndex = 1
        ' 
        ' lblDenumire
        ' 
        lblDenumire.Dock = DockStyle.Fill
        lblDenumire.Location = New Point(21, 15)
        lblDenumire.Margin = New Padding(0)
        lblDenumire.Name = "lblDenumire"
        lblDenumire.Size = New Size(255, 54)
        lblDenumire.TabIndex = 0
        lblDenumire.Text = "Denumire *"
        lblDenumire.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtDenumire
        ' 
        txtDenumire.BackColor = Color.Transparent
        txtDenumire.Dock = DockStyle.Fill
        txtDenumire.Location = New Point(276, 19)
        txtDenumire.Margin = New Padding(0, 4, 0, 4)
        txtDenumire.MaxLength = 100
        txtDenumire.Name = "txtDenumire"
        txtDenumire.Size = New Size(699, 46)
        txtDenumire.TabIndex = 1
        txtDenumire.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipText(txtDenumire, "Numele subunității, unic în unitate.")
        ' 
        ' lblCapitol
        ' 
        lblCapitol.Dock = DockStyle.Fill
        lblCapitol.Location = New Point(21, 69)
        lblCapitol.Margin = New Padding(0)
        lblCapitol.Name = "lblCapitol"
        lblCapitol.Size = New Size(255, 54)
        lblCapitol.TabIndex = 2
        lblCapitol.Text = "Capitol *"
        lblCapitol.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' txtCapitol
        ' 
        txtCapitol.BackColor = Color.Transparent
        txtCapitol.Dock = DockStyle.Left
        txtCapitol.Location = New Point(276, 73)
        txtCapitol.Margin = New Padding(0, 4, 0, 4)
        txtCapitol.MaxLength = 5
        txtCapitol.Name = "txtCapitol"
        txtCapitol.Size = New Size(240, 46)
        txtCapitol.TabIndex = 3
        txtCapitol.TextPadding = New Padding(8, 0, 8, 0)
        tips.SetToolTipHeader(txtCapitol, "Capitol")
        tips.SetToolTipText(txtCapitol, "Capitolul bugetar al subunității, de forma 65.01. Același capitol îl pot avea doar o subunitate și copiii ei.")
        ' 
        ' lblSs
        ' 
        lblSs.Dock = DockStyle.Fill
        lblSs.Location = New Point(21, 123)
        lblSs.Margin = New Padding(0)
        lblSs.Name = "lblSs"
        lblSs.Size = New Size(255, 54)
        lblSs.TabIndex = 4
        lblSs.Text = "Sector și sursă *"
        lblSs.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboSs
        ' 
        cboSs.Dock = DockStyle.Fill
        cboSs.Location = New Point(276, 127)
        cboSs.Margin = New Padding(0, 4, 0, 4)
        cboSs.Name = "cboSs"
        cboSs.Size = New Size(699, 46)
        cboSs.TabIndex = 5
        tips.SetToolTipHeader(cboSs, "Sector și sursă")
        tips.SetToolTipText(cboSs, "Sectorul și sursa propuse unui cont nou al subunității (de exemplu 01A), când în bara de titlu este «Toate». Fiecare cont își are propriul sector și sursă.")
        ' 
        ' lblZecimale
        ' 
        lblZecimale.Dock = DockStyle.Fill
        lblZecimale.Location = New Point(21, 177)
        lblZecimale.Margin = New Padding(0)
        lblZecimale.Name = "lblZecimale"
        lblZecimale.Size = New Size(255, 54)
        lblZecimale.TabIndex = 6
        lblZecimale.Text = "Zecimale salarii"
        lblZecimale.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' cboZecimale
        ' 
        cboZecimale.Dock = DockStyle.Left
        cboZecimale.Location = New Point(276, 181)
        cboZecimale.Margin = New Padding(0, 4, 0, 4)
        cboZecimale.Name = "cboZecimale"
        cboZecimale.Size = New Size(240, 46)
        cboZecimale.TabIndex = 7
        tips.SetToolTipHeader(cboZecimale, "Zecimale salarii")
        tips.SetToolTipText(cboZecimale, "2 = salariile se introduc și se afișează cu două zecimale (proiecte europene).")
        ' 
        ' chkImplicita
        ' 
        chkImplicita.AutoSize = True
        chkImplicita.FlatStyle = FlatStyle.Flat
        chkImplicita.Location = New Point(276, 231)
        chkImplicita.Margin = New Padding(0)
        chkImplicita.Name = "chkImplicita"
        chkImplicita.Size = New Size(260, 26)
        chkImplicita.TabIndex = 8
        chkImplicita.Text = "Subunitatea implicită a unității"
        tips.SetToolTipText(chkImplicita, "Cea care se deschide prima. Una singură; prima subunitate creată este implicită.")
        ' 
        ' chkActiv
        ' 
        chkActiv.AutoSize = True
        chkActiv.Checked = True
        chkActiv.CheckState = CheckState.Checked
        chkActiv.FlatStyle = FlatStyle.Flat
        chkActiv.Location = New Point(276, 273)
        chkActiv.Margin = New Padding(0)
        chkActiv.Name = "chkActiv"
        chkActiv.Size = New Size(100, 26)
        chkActiv.TabIndex = 9
        chkActiv.Text = "Activă"
        ' 
        ' pnlJos
        ' 
        pnlJos.Controls.Add(lblStare)
        pnlJos.Controls.Add(btnSalveaza)
        pnlJos.Controls.Add(btnRenunta)
        pnlJos.Dock = DockStyle.Bottom
        pnlJos.Location = New Point(0, 612)
        pnlJos.Margin = New Padding(0)
        pnlJos.Name = "pnlJos"
        pnlJos.Padding = New Padding(21, 12, 21, 12)
        pnlJos.Size = New Size(996, 84)
        pnlJos.TabIndex = 2
        pnlJos.Tag = "Card"
        ' 
        ' lblStare
        ' 
        lblStare.AutoEllipsis = True
        lblStare.Dock = DockStyle.Fill
        lblStare.Location = New Point(201, 12)
        lblStare.Margin = New Padding(0)
        lblStare.Name = "lblStare"
        lblStare.Size = New Size(594, 60)
        lblStare.TabIndex = 0
        lblStare.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnSalveaza
        ' 
        btnSalveaza.Dock = DockStyle.Right
        btnSalveaza.FlatStyle = FlatStyle.Flat
        btnSalveaza.Location = New Point(795, 12)
        btnSalveaza.Margin = New Padding(0)
        btnSalveaza.Name = "btnSalveaza"
        btnSalveaza.Size = New Size(180, 60)
        btnSalveaza.TabIndex = 1
        btnSalveaza.Text = "Salvare"
        btnSalveaza.UseVisualStyleBackColor = True
        ' 
        ' btnRenunta
        ' 
        btnRenunta.DialogResult = DialogResult.Cancel
        btnRenunta.Dock = DockStyle.Left
        btnRenunta.FlatStyle = FlatStyle.Flat
        btnRenunta.Location = New Point(21, 12)
        btnRenunta.Margin = New Padding(0)
        btnRenunta.Name = "btnRenunta"
        btnRenunta.Size = New Size(180, 60)
        btnRenunta.TabIndex = 2
        btnRenunta.Text = "Renunță"
        btnRenunta.UseVisualStyleBackColor = True
        ' 
        ' capBar
        ' 
        capBar.Dock = DockStyle.Top
        capBar.IconImage = My.Resources.Resources.folder_open
        capBar.Location = New Point(0, 0)
        capBar.Margin = New Padding(0)
        capBar.Name = "capBar"
        capBar.OptionButtonImage = Nothing
        capBar.OptionButtonPadding = 0
        capBar.ShowTextScaleSlider = False
        capBar.Size = New Size(996, 66)
        capBar.TabIndex = 0
        capBar.TabStop = False
        capBar.Text = "Subunitate"
        ' 
        ' SubunitateEditForm
        ' 
        AcceptButton = btnSalveaza
        AutoScaleDimensions = New SizeF(144F, 144F)
        AutoScaleMode = AutoScaleMode.Dpi
        CancelButton = btnRenunta
        ClientSize = New Size(1000, 700)
        Controls.Add(pnlCard)
        FormBorderStyle = FormBorderStyle.None
        Margin = New Padding(4, 4, 4, 4)
        MaximizeBox = False
        MinimizeBox = False
        Name = "SubunitateEditForm"
        Padding = New Padding(2, 2, 2, 2)
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Subunitate"
        pnlCard.ResumeLayout(False)
        tlyCorp.ResumeLayout(False)
        tlyCorp.PerformLayout()
        pnlJos.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents tips As KBotToolTip
    Friend WithEvents pnlCard As Panel
    Friend WithEvents tlyCorp As KBotTableLayoutPanel
    Friend WithEvents lblDenumire As Label
    Friend WithEvents txtDenumire As KBotTextField
    Friend WithEvents lblCapitol As Label
    Friend WithEvents txtCapitol As KBotTextField
    Friend WithEvents lblSs As Label
    Friend WithEvents cboSs As KBotComboBox
    Friend WithEvents lblZecimale As Label
    Friend WithEvents cboZecimale As KBotComboBox
    Friend WithEvents chkImplicita As CheckBox
    Friend WithEvents chkActiv As CheckBox
    Friend WithEvents pnlJos As Panel
    Friend WithEvents lblStare As Label
    Friend WithEvents btnSalveaza As Button
    Friend WithEvents btnRenunta As Button
    Friend WithEvents capBar As KBotCaptionBar
End Class
